using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using AccountModelsLib;

namespace AccountERP.AI.Tools
{
    /// <summary>
    /// P3: نتيجة أداة بيانات كاملة (حتى 5000 صف) تبقى على الخادم؛ النموذج يرى عيّنة فقط ويشير إليها بـ dataset_id فى render_report،
    /// فكل رقم فى التقرير يأتى من هنا لا من النموذج (§5). Source = الأداة ومدخلاتها لإعادة التشغيل بفترة جديدة.
    /// </summary>
    public sealed class AiDataset
    {
        public string Title { get; set; } = "";
        public List<string> Columns { get; init; } = new();
        /// <summary>number / date / text لكل عمود.</summary>
        public List<string> Types { get; init; } = new();
        /// <summary>القيم: decimal للأرقام، string للتاريخ (yyyy-MM-dd) والنص، null للفارغ.</summary>
        public List<object?[]> Rows { get; init; } = new();
        public bool Truncated { get; set; }
        public string Tool { get; set; } = "";
        public JsonObject Args { get; set; } = new();
        public string? Sql { get; set; }

        public const string TNumber = "number", TDate = "date", TText = "text";

        public int IndexOf(string column) => Columns.FindIndex(c => string.Equals(c, column, StringComparison.OrdinalIgnoreCase));

        /// <summary>من نتيجة استعلام: يوحّد الأنواع (كل الأرقام decimal، التواريخ نص) ويستنتج نوع كل عمود.</summary>
        public static AiDataset FromQuery(AiQueryResult r)
        {
            var ds = new AiDataset { Columns = r.Columns.ToList(), Truncated = r.Truncated };
            foreach (var row in r.Rows) ds.Rows.Add(row.Select(Normalize).ToArray());
            for (int i = 0; i < ds.Columns.Count; i++)
            {
                var vals = r.Rows.Select(x => x[i]).Where(v => v != null).ToList();
                string t = vals.Count == 0 ? TText
                    : vals.All(v => v is DateTime or DateTimeOffset) ? TDate
                    : vals.All(IsNumber) ? TNumber : TText;
                ds.Types.Add(t);
            }
            return ds;
        }

        /// <summary>من صفوف جاهزة بأنواع معلنة.</summary>
        public static AiDataset FromRows(IEnumerable<string> columns, IEnumerable<string> types, IEnumerable<object?[]> rows)
        {
            var ds = new AiDataset { Columns = columns.ToList(), Types = types.ToList() };
            foreach (var row in rows) ds.Rows.Add(row.Select(Normalize).ToArray());
            return ds;
        }

        public static bool IsNumber(object? v) => v is decimal or double or float or int or long or short or byte;

        public static object? Normalize(object? v) => v switch
        {
            null or DBNull => null,
            DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : d.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            DateTimeOffset d => d.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            decimal m => m,
            double or float or int or long or short or byte => Convert.ToDecimal(v, CultureInfo.InvariantCulture),
            bool b => b ? 1m : 0m,
            byte[] => "(binary)",
            _ => Convert.ToString(v, CultureInfo.InvariantCulture),
        };

        // ───── التخزين داخل AI_SavedReports.DataJson ─────

        public JsonObject ToJson(int maxRows)
        {
            var rows = new JsonArray();
            foreach (var row in Rows.Take(maxRows))
                rows.Add(new JsonArray(row.Select(v => v switch
                {
                    null => null,
                    decimal m => (JsonNode)JsonValue.Create(m),
                    _ => JsonValue.Create(Convert.ToString(v, CultureInfo.InvariantCulture)),
                }).ToArray()));
            return new JsonObject
            {
                ["title"] = Title,
                ["columns"] = new JsonArray(Columns.Select(c => (JsonNode)JsonValue.Create(c)!).ToArray()),
                ["types"] = new JsonArray(Types.Select(c => (JsonNode)JsonValue.Create(c)!).ToArray()),
                ["rows"] = rows,
                ["truncated"] = Truncated || Rows.Count > maxRows,
                ["total_rows"] = Rows.Count,
            };
        }

        public static AiDataset FromJson(JsonObject o)
        {
            var ds = new AiDataset
            {
                Title = o["title"]?.GetValue<string>() ?? "",
                Columns = (o["columns"] as JsonArray ?? new()).Select(n => n?.GetValue<string>() ?? "").ToList(),
                Types = (o["types"] as JsonArray ?? new()).Select(n => n?.GetValue<string>() ?? TText).ToList(),
                Truncated = o["truncated"]?.GetValue<bool>() ?? false,
            };
            foreach (var r in o["rows"] as JsonArray ?? new())
            {
                var arr = r as JsonArray ?? new();
                ds.Rows.Add(arr.Select((n, i) => n is JsonValue v
                    ? (i < ds.Types.Count && ds.Types[i] == TNumber && v.TryGetValue<decimal>(out var m) ? m : (object?)(v.TryGetValue<string>(out var s) ? s : v.ToJsonString()))
                    : null).ToArray());
            }
            return ds;
        }
    }

    /// <summary>
    /// ذاكرة datasets لمدة ساعتين لكل مستخدم (Singleton). المعرّف عشوائى قصير؛ لا يقرأ مستخدم dataset غيره.
    /// تكفى لدورة الأدوات داخل الرسالة ولرسائل المتابعة القريبة («ارسمها كرسم دائرى»).
    /// </summary>
    public sealed class AiDatasetCache
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromHours(2);
        private const int MaxEntries = 3000;

        private sealed record Entry(long UserId, AiDataset Data, DateTime CreatedUtc);
        private readonly ConcurrentDictionary<string, Entry> _items = new(StringComparer.OrdinalIgnoreCase);

        public string Add(long userId, AiDataset ds)
        {
            Sweep();
            string id;
            do id = "ds_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            while (!_items.TryAdd(id, new Entry(userId, ds, DateTime.UtcNow)));
            return id;
        }

        public AiDataset? Get(string id, long userId) =>
            _items.TryGetValue((id ?? "").Trim(), out var e) && e.UserId == userId && DateTime.UtcNow - e.CreatedUtc < Ttl ? e.Data : null;

        private void Sweep()
        {
            var now = DateTime.UtcNow;
            foreach (var kv in _items)
                if (now - kv.Value.CreatedUtc >= Ttl) _items.TryRemove(kv.Key, out _);
            if (_items.Count >= MaxEntries)
                foreach (var kv in _items.OrderBy(k => k.Value.CreatedUtc).Take(_items.Count - MaxEntries + 100))
                    _items.TryRemove(kv.Key, out _);
        }
    }
}
