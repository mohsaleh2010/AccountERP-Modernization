using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AccountERP.SalesRepApp.Core
{
    /// <summary>أسرار الجهاز (توكن التجديد، مفتاح القاعدة) — على الجهاز SecureStorage، وفى الاختبار ذاكرة.</summary>
    public interface ISecretStore
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value);
        Task RemoveAsync(string key);
    }

    public sealed class MemorySecretStore : ISecretStore
    {
        private readonly Dictionary<string, string> _values = new();
        public Task<string?> GetAsync(string key) => Task.FromResult(_values.TryGetValue(key, out var v) ? v : null);
        public Task SetAsync(string key, string value) { _values[key] = value; return Task.CompletedTask; }
        public Task RemoveAsync(string key) { _values.Remove(key); return Task.CompletedTask; }
    }

    public class OutboxItem
    {
        public Guid Doc_Guid { get; set; }
        public DocType Doc_Type { get; set; }
        public string Payload { get; set; } = "";
        public DateTime Created_At { get; set; }
        public OutboxStatus Status { get; set; }
        public string? Error { get; set; }
        public long? Result_Ref_Id { get; set; }
        public int Attempts { get; set; }
        public DateTime? Next_Try_At { get; set; }
        public long Customer_Id { get; set; }
        public decimal Amount { get; set; }
        /// <summary>لم يُحفظ على السيرفر بعد ولم يُرفض: أثره (المخزون/العهدة) محسوب محلياً.</summary>
        public bool Open => Status is OutboxStatus.Queued or OutboxStatus.Received or OutboxStatus.Error or OutboxStatus.PendingReview;
    }

    /// <summary>
    /// القاعدة المحلية المشفرة (SQLCipher): إعدادات، صفوف مجموعات البيانات كما نزلت (JSON بمفتاحها) ونسخة كل مجموعة، والصادر.
    /// </summary>
    public sealed class LocalStore : IDisposable
    {
        private readonly SqliteConnection _conn;
        private static bool _initialized;

        public LocalStore(string path, string key)
        {
            if (!_initialized) { SQLitePCL.Batteries_V2.Init(); _initialized = true; }
            var csb = new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadWriteCreate, Password = key };
            _conn = new SqliteConnection(csb.ToString());
            _conn.Open();
            Exec(@"
CREATE TABLE IF NOT EXISTS kv (k TEXT PRIMARY KEY, v TEXT);
CREATE TABLE IF NOT EXISTS ds_rows (ds TEXT NOT NULL, k INTEGER NOT NULL, json TEXT NOT NULL, PRIMARY KEY (ds, k));
CREATE TABLE IF NOT EXISTS outbox (
    guid TEXT PRIMARY KEY, seq INTEGER NOT NULL, type INTEGER NOT NULL, payload TEXT NOT NULL, created_at TEXT NOT NULL,
    status INTEGER NOT NULL, error TEXT, result_ref INTEGER, attempts INTEGER NOT NULL DEFAULT 0, next_try_at TEXT,
    customer_id INTEGER NOT NULL DEFAULT 0, amount TEXT NOT NULL DEFAULT '0');");
        }

        // ============================== الإعدادات ==============================

        public string? Get(string key)
        {
            using var cmd = Cmd("SELECT v FROM kv WHERE k = $k", ("$k", key));
            return cmd.ExecuteScalar() as string;
        }

        public void Set(string key, string? value)
        {
            if (value == null) { using var d = Cmd("DELETE FROM kv WHERE k = $k", ("$k", key)); d.ExecuteNonQuery(); return; }
            using var cmd = Cmd("INSERT INTO kv (k, v) VALUES ($k, $v) ON CONFLICT(k) DO UPDATE SET v = excluded.v", ("$k", key), ("$v", value));
            cmd.ExecuteNonQuery();
        }

        public DateTime? GetTime(string key) => DateTime.TryParse(Get(key), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var t) ? t : null;
        public void SetTime(string key, DateTime? t) => Set(key, t?.ToString("o", CultureInfo.InvariantCulture));

        // ============================== مجموعات البيانات ==============================

        public long? Version(string dataset) => long.TryParse(Get("ver:" + dataset), out long v) ? v : null;

        /// <summary>يطبق نتيجة تنزيل فى معاملة: الكامل يستبدل المجموعة، والفرق يضيف/يعدّل ويحذف، ثم يحفظ النسخة.</summary>
        public void ApplyPull(PullResult r)
        {
            using var tx = _conn.BeginTransaction();
            if (r.Full) { using var del = Cmd("DELETE FROM ds_rows WHERE ds = $ds", ("$ds", r.Dataset)); del.Transaction = tx; del.ExecuteNonQuery(); }
            foreach (var row in r.Rows)
            {
                long key = KeyOf(r.Dataset, row);
                using var up = Cmd("INSERT INTO ds_rows (ds, k, json) VALUES ($ds, $k, $j) ON CONFLICT(ds, k) DO UPDATE SET json = excluded.json",
                    ("$ds", r.Dataset), ("$k", key), ("$j", row.GetRawText()));
                up.Transaction = tx;
                up.ExecuteNonQuery();
            }
            foreach (long key in r.Deleted)
            {
                using var del = Cmd("DELETE FROM ds_rows WHERE ds = $ds AND k = $k", ("$ds", r.Dataset), ("$k", key));
                del.Transaction = tx;
                del.ExecuteNonQuery();
            }
            using (var ver = Cmd("INSERT INTO kv (k, v) VALUES ($k, $v) ON CONFLICT(k) DO UPDATE SET v = excluded.v", ("$k", "ver:" + r.Dataset), ("$v", r.Version.ToString(CultureInfo.InvariantCulture))))
            {
                ver.Transaction = tx;
                ver.ExecuteNonQuery();
            }
            tx.Commit();
        }

        /// <summary>مفتاح الصف كما يحسبه السيرفر: Id، أو رقم العميل فى خطة اليوم، أو رقم الطلبية.</summary>
        public static long KeyOf(string dataset, JsonElement row)
        {
            if (dataset == "plan") return row.GetProperty("Customer_Id").GetInt64();
            if (dataset == "orders") return row.GetProperty("Order").GetProperty("Id").GetInt64();
            return row.GetProperty("Id").GetInt64();
        }

        public List<T> Rows<T>(string dataset)
        {
            var list = new List<T>();
            using var cmd = Cmd("SELECT json FROM ds_rows WHERE ds = $ds ORDER BY k", ("$ds", dataset));
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) { var v = Json.Deserialize<T>(rd.GetString(0)); if (v != null) list.Add(v); }
            return list;
        }

        public T? Row<T>(string dataset, long key) where T : class
        {
            using var cmd = Cmd("SELECT json FROM ds_rows WHERE ds = $ds AND k = $k", ("$ds", dataset), ("$k", key));
            return cmd.ExecuteScalar() is string s ? Json.Deserialize<T>(s) : null;
        }

        public int Count(string dataset)
        {
            using var cmd = Cmd("SELECT COUNT(*) FROM ds_rows WHERE ds = $ds", ("$ds", dataset));
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
        }

        // ============================== الصادر ==============================

        public void Enqueue(OutboxItem item)
        {
            using var cmd = Cmd(@"INSERT INTO outbox (guid, seq, type, payload, created_at, status, attempts, customer_id, amount)
VALUES ($g, (SELECT IFNULL(MAX(seq), 0) + 1 FROM outbox), $t, $p, $c, $s, 0, $cu, $a)",
                ("$g", item.Doc_Guid.ToString()), ("$t", (int)item.Doc_Type), ("$p", item.Payload), ("$c", item.Created_At.ToString("o", CultureInfo.InvariantCulture)),
                ("$s", (int)item.Status), ("$cu", item.Customer_Id), ("$a", item.Amount.ToString(CultureInfo.InvariantCulture)));
            cmd.ExecuteNonQuery();
        }

        public List<OutboxItem> Outbox(bool openOnly = false)
        {
            var list = new List<OutboxItem>();
            using var cmd = Cmd("SELECT guid, type, payload, created_at, status, error, result_ref, attempts, next_try_at, customer_id, amount FROM outbox ORDER BY seq");
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var o = new OutboxItem
                {
                    Doc_Guid = Guid.Parse(rd.GetString(0)), Doc_Type = (DocType)rd.GetInt32(1), Payload = rd.GetString(2),
                    Created_At = DateTime.Parse(rd.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    Status = (OutboxStatus)rd.GetInt32(4), Error = rd.IsDBNull(5) ? null : rd.GetString(5), Result_Ref_Id = rd.IsDBNull(6) ? null : rd.GetInt64(6),
                    Attempts = rd.GetInt32(7),
                    Next_Try_At = rd.IsDBNull(8) ? null : DateTime.Parse(rd.GetString(8), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    Customer_Id = rd.GetInt64(9), Amount = decimal.Parse(rd.GetString(10), CultureInfo.InvariantCulture),
                };
                if (!openOnly || o.Open) list.Add(o);
            }
            return list;
        }

        public void UpdateOutbox(Guid guid, OutboxStatus status, string? error, long? resultRef, int attempts, DateTime? nextTry)
        {
            using var cmd = Cmd("UPDATE outbox SET status = $s, error = $e, result_ref = $r, attempts = $a, next_try_at = $n WHERE guid = $g",
                ("$s", (int)status), ("$e", (object?)error ?? DBNull.Value), ("$r", (object?)resultRef ?? DBNull.Value), ("$a", attempts),
                ("$n", (object?)nextTry?.ToString("o", CultureInfo.InvariantCulture) ?? DBNull.Value), ("$g", guid.ToString()));
            cmd.ExecuteNonQuery();
        }

        // ============================== داخلى ==============================

        private void Exec(string sql) { using var cmd = Cmd(sql); cmd.ExecuteNonQuery(); }

        private SqliteCommand Cmd(string sql, params (string Name, object Value)[] args)
        {
            var cmd = _conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (var (n, v) in args) cmd.Parameters.AddWithValue(n, v);
            return cmd;
        }

        public void Dispose() => _conn.Dispose();
    }
}
