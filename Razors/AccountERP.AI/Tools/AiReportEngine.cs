using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountModelsLib;

namespace AccountERP.AI.Tools
{
    /// <summary>
    /// P3 (§5): ReportSpec → تقرير. النموذج يحدد الشكل فقط (عنوان، مؤشرات، جداول، رسوم، ملاحظات) ويشير للبيانات بـ dataset_id،
    /// والمحرك يتحقق من كل مرجع ويبنى الأرقام من البيانات على الخادم. لا HTML ولا سكربت من النموذج.
    /// </summary>
    public static class AiReportEngine
    {
        public const int MaxSections = 12, MaxKpis = 8, MaxTableRows = 1000, MaxChartPoints = 40, MaxPieSlices = 5, StoredRows = 2000;

        private static readonly string[] Aggs = { "sum", "avg", "min", "max", "count", "first" };

        public static JsonObject ToolSchema()
        {
            JsonObject S(string d) => new() { ["type"] = "string", ["description"] = d };
            JsonObject E(string d, params string[] v) => new() { ["type"] = "string", ["description"] = d, ["enum"] = new JsonArray(v.Select(x => (JsonNode)JsonValue.Create(x)!).ToArray()) };
            JsonArray Req(params string[] r) => new(r.Select(x => (JsonNode)JsonValue.Create(x)!).ToArray());
            var column = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["key"] = S("اسم العمود كما فى الـ dataset"),
                    ["label"] = S("عنوان العمود بالعربية"),
                    ["format"] = E("التنسيق", AiReportFormats.All),
                },
                ["required"] = Req("key"),
            };
            var kpi = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["label"] = S("عنوان المؤشر"),
                    ["dataset_id"] = S("المعرّف من نتيجة أداة بيانات"),
                    ["column"] = S("العمود (لـ count أى عمود)"),
                    ["agg"] = E("طريقة الحساب على كل صفوف الـ dataset", Aggs),
                    ["format"] = E("التنسيق", AiReportFormats.All),
                },
                ["required"] = Req("label", "dataset_id", "column", "agg"),
            };
            var section = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["type"] = E("table = جدول، chart = رسم بيانى، text = ملاحظة تحليلية قصيرة", "table", "chart", "text"),
                    ["title"] = S("عنوان القسم"),
                    ["text"] = S("نص الملاحظة (للنوع text فقط، نص عادى بلا HTML)"),
                    ["dataset_id"] = S("للجدول والرسم"),
                    ["columns"] = new JsonObject { ["type"] = "array", ["description"] = "أعمدة الجدول بترتيبها (اختيارى: الافتراضى كل الأعمدة)", ["items"] = column },
                    ["chart_kind"] = E("نوع الرسم", AiChartKinds.All),
                    ["x"] = S("عمود المحور الأفقى/التصنيف (للرسم)"),
                    ["y"] = new JsonObject { ["type"] = "array", ["description"] = "أعمدة القيم الرقمية (للرسم؛ الدائرى عمود واحد)", ["items"] = new JsonObject { ["type"] = "string" } },
                    ["y_labels"] = new JsonObject { ["type"] = "array", ["description"] = "أسماء السلاسل بالعربية بنفس ترتيب y (اختيارى)", ["items"] = new JsonObject { ["type"] = "string" } },
                    ["value_format"] = E("تنسيق قيم الرسم", AiReportFormats.All),
                    ["sort_by"] = S("ترتيب الصفوف بهذا العمود (اختيارى)"),
                    ["sort_desc"] = new JsonObject { ["type"] = "boolean", ["description"] = "تنازلى" },
                    ["top"] = new JsonObject { ["type"] = "integer", ["description"] = "أول N صف بعد الترتيب (اختيارى)" },
                    ["show_totals"] = new JsonObject { ["type"] = "boolean", ["description"] = "سطر الإجمالى للجدول (الافتراضى نعم)" },
                },
                ["required"] = Req("type"),
            };
            return new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["title"] = S("عنوان التقرير (بلا تواريخ؛ الفترة تُعرض تلقائياً من date_from/date_to)"),
                    ["subtitle"] = S("سطر وصفى قصير (اختيارى، بلا تواريخ)"),
                    ["date_from"] = S("بداية فترة التقرير yyyy-MM-dd (اختيارى)"),
                    ["date_to"] = S("نهاية فترة التقرير yyyy-MM-dd (اختيارى)"),
                    ["kpis"] = new JsonObject { ["type"] = "array", ["description"] = "مؤشرات رقمية أعلى التقرير (حتى 8)", ["items"] = kpi },
                    ["sections"] = new JsonObject { ["type"] = "array", ["description"] = "أقسام التقرير بالترتيب (حتى 12)", ["items"] = section },
                },
                ["required"] = Req("title", "sections"),
            };
        }

        private static string Str(JsonNode? n) => n is JsonValue v && v.TryGetValue<string>(out var s) ? s.Trim() : "";
        private static bool? Bool(JsonNode? n) => n is JsonValue v && v.TryGetValue<bool>(out var b) ? b : null;
        private static int? Int(JsonNode? n) => n is JsonValue v && v.TryGetValue<int>(out var i) ? i : null;
        private static string Clean(string s, int max)
        {
            s = new string(s.Where(ch => !char.IsControl(ch) || ch == '\n').ToArray()).Trim();
            return s.Length > max ? s[..max] : s;
        }

        /// <summary>
        /// يتحقق من الـ spec ويعيد نسخة نظيفة + الـ datasets المستخدمة. كل الأخطاء تُجمع فى رسالة واحدة (ArgumentException) ليصححها النموذج مرة واحدة.
        /// </summary>
        public static (JsonObject Spec, Dictionary<string, AiDataset> Data) Prepare(JsonObject a, Func<string, AiDataset?> lookup)
        {
            var errors = new List<string>();
            var used = new Dictionary<string, AiDataset>(StringComparer.OrdinalIgnoreCase);
            AiDataset? Ds(string id, string where)
            {
                if (id.Length == 0) { errors.Add(where + ": dataset_id مطلوب."); return null; }
                if (used.TryGetValue(id, out var d)) return d;
                d = lookup(id);
                if (d == null) { errors.Add(where + $": dataset_id «{id}» غير موجود أو انتهت صلاحيته؛ أعد تنفيذ أداة البيانات واستخدم المعرّف الجديد."); return null; }
                used[id] = d;
                return d;
            }
            bool Col(AiDataset d, string c, string where, bool numeric = false)
            {
                int i = d.IndexOf(c);
                if (i < 0) { errors.Add($"{where}: العمود «{c}» غير موجود. الأعمدة: {string.Join(", ", d.Columns)}."); return false; }
                if (numeric && d.Types[i] != AiDataset.TNumber) { errors.Add($"{where}: العمود «{c}» ليس رقمياً."); return false; }
                return true;
            }
            string Fmt(string f, string where)
            {
                if (f.Length == 0) return "";
                if (!AiReportFormats.All.Contains(f)) { errors.Add($"{where}: تنسيق غير معروف «{f}»."); return ""; }
                return f;
            }

            string title = Clean(Str(a["title"]), 200);
            if (title.Length == 0) errors.Add("title مطلوب.");
            var spec = new JsonObject { ["title"] = title, ["subtitle"] = Clean(Str(a["subtitle"]), 300) };
            foreach (var k in new[] { "date_from", "date_to" })
            {
                string t = Str(a[k]);
                if (t.Length == 0) continue;
                if (!DateTime.TryParseExact(t, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) errors.Add(k + " يجب أن يكون yyyy-MM-dd.");
                else spec[k] = t;
            }

            var kpis = new JsonArray();
            var kin = a["kpis"] as JsonArray ?? new JsonArray();
            if (kin.Count > MaxKpis) errors.Add($"الحد الأقصى {MaxKpis} مؤشرات.");
            int ki = 0;
            foreach (var n in kin.Take(MaxKpis).OfType<JsonObject>())
            {
                string where = $"kpis[{ki++}]";
                string agg = Str(n["agg"]).ToLowerInvariant(), col = Str(n["column"]), id = Str(n["dataset_id"]);
                if (!Aggs.Contains(agg)) { errors.Add($"{where}: agg يجب أن يكون واحداً من {string.Join(", ", Aggs)}."); continue; }
                var d = Ds(id, where);
                if (d == null || !Col(d, col, where, numeric: agg is not ("count" or "first"))) continue;
                kpis.Add(new JsonObject
                {
                    ["label"] = Clean(Str(n["label"]), 80), ["dataset_id"] = id, ["column"] = col, ["agg"] = agg,
                    ["format"] = Fmt(Str(n["format"]), where),
                });
            }
            spec["kpis"] = kpis;

            var sections = new JsonArray();
            var sin = a["sections"] as JsonArray ?? new JsonArray();
            if (sin.Count == 0) errors.Add("sections مطلوبة (قسم واحد على الأقل).");
            if (sin.Count > MaxSections) errors.Add($"الحد الأقصى {MaxSections} قسماً.");
            int si = 0;
            foreach (var n in sin.Take(MaxSections).OfType<JsonObject>())
            {
                string where = $"sections[{si++}]";
                string type = Str(n["type"]).ToLowerInvariant();
                var o = new JsonObject { ["type"] = type, ["title"] = Clean(Str(n["title"]), 150) };
                if (type == "text")
                {
                    string text = Clean(Str(n["text"]), 3000);
                    if (text.Length == 0) { errors.Add(where + ": text مطلوب."); continue; }
                    o["text"] = text;
                    sections.Add(o);
                    continue;
                }
                if (type is not ("table" or "chart")) { errors.Add(where + ": type يجب أن يكون table أو chart أو text."); continue; }
                string id = Str(n["dataset_id"]);
                var d = Ds(id, where);
                if (d == null) continue;
                o["dataset_id"] = id;
                string sortBy = Str(n["sort_by"]);
                if (sortBy.Length > 0 && Col(d, sortBy, where)) { o["sort_by"] = sortBy; o["sort_desc"] = Bool(n["sort_desc"]) ?? false; }
                if (Int(n["top"]) is int top && top > 0) o["top"] = Math.Min(top, MaxTableRows);
                if (type == "table")
                {
                    var cols = new JsonArray();
                    foreach (var c in (n["columns"] as JsonArray ?? new JsonArray()).OfType<JsonObject>())
                    {
                        string key = Str(c["key"]);
                        if (!Col(d, key, where)) continue;
                        cols.Add(new JsonObject { ["key"] = key, ["label"] = Clean(Str(c["label"]), 80), ["format"] = Fmt(Str(c["format"]), where) });
                    }
                    o["columns"] = cols;
                    o["show_totals"] = Bool(n["show_totals"]) ?? true;
                }
                else
                {
                    string kind = Str(n["chart_kind"]).ToLowerInvariant();
                    if (kind.Length == 0) kind = AiChartKinds.Bar;
                    if (!AiChartKinds.All.Contains(kind)) { errors.Add($"{where}: chart_kind يجب أن يكون {string.Join(", ", AiChartKinds.All)}."); continue; }
                    string x = Str(n["x"]);
                    if (x.Length == 0) { errors.Add(where + ": x مطلوب للرسم."); continue; }
                    if (!Col(d, x, where)) continue;
                    var ys = (n["y"] as JsonArray ?? new JsonArray()).Select(Str).Where(s => s.Length > 0).Distinct().ToList();
                    if (ys.Count == 0) { errors.Add(where + ": y مطلوب (عمود رقمى واحد على الأقل)."); continue; }
                    if (kind == AiChartKinds.Pie && ys.Count > 1) { errors.Add(where + ": الرسم الدائرى يقبل عموداً واحداً فى y."); continue; }
                    if (ys.Count > 6) { errors.Add(where + ": الحد الأقصى 6 سلاسل."); continue; }
                    if (!ys.All(y => Col(d, y, where, numeric: true))) continue;
                    var labels = (n["y_labels"] as JsonArray ?? new JsonArray()).Select(l => Clean(Str(l), 60)).ToList();
                    o["chart_kind"] = kind; o["x"] = x;
                    o["y"] = new JsonArray(ys.Select(y => (JsonNode)JsonValue.Create(y)!).ToArray());
                    o["y_labels"] = new JsonArray(ys.Select((y, i) => (JsonNode)JsonValue.Create(i < labels.Count && labels[i].Length > 0 ? labels[i] : y)!).ToArray());
                    o["value_format"] = Fmt(Str(n["value_format"]), where);
                }
                sections.Add(o);
            }
            spec["sections"] = sections;
            if (errors.Count > 0) throw new ArgumentException("ReportSpec غير صالح: " + string.Join(" | ", errors.Take(12)));
            return (spec, used);
        }

        // ───── البناء للعرض ─────

        public static string InferFormat(string column, string type)
        {
            if (type == AiDataset.TDate) return AiReportFormats.Date;
            if (type != AiDataset.TNumber) return AiReportFormats.Text;
            string c = column.ToLowerInvariant();
            if (c.Contains("percent") || c.Contains("pct") || c.Contains("ratio") || c.Contains("نسبة")) return AiReportFormats.Percent;
            if (c.EndsWith("count") || c.StartsWith("count") || c.Contains("_count") || c.EndsWith("_no") || c.EndsWith("_id") || c == "id") return AiReportFormats.Int;
            if (c.Contains("qty") || c.Contains("quantity") || c.Contains("كمية")) return AiReportFormats.Qty;
            return AiReportFormats.Amount;
        }

        private static IEnumerable<object?[]> Ordered(AiDataset d, JsonObject s)
        {
            IEnumerable<object?[]> rows = d.Rows;
            string sortBy = Str(s["sort_by"]);
            int si = sortBy.Length > 0 ? d.IndexOf(sortBy) : -1;
            if (si >= 0)
            {
                bool desc = Bool(s["sort_desc"]) ?? false;
                rows = desc ? rows.OrderByDescending(r => r[si], Cmp) : rows.OrderBy(r => r[si], Cmp);
            }
            return rows;
        }

        private static readonly Comparer<object?> Cmp = Comparer<object?>.Create((x, y) =>
        {
            if (x is decimal a && y is decimal b) return a.CompareTo(b);
            if (x == null) return y == null ? 0 : -1;
            if (y == null) return 1;
            return string.Compare(Convert.ToString(x, CultureInfo.InvariantCulture), Convert.ToString(y, CultureInfo.InvariantCulture), StringComparison.CurrentCulture);
        });

        public static AiReportDoc Materialize(JsonObject spec, IReadOnlyDictionary<string, AiDataset> data)
        {
            var doc = new AiReportDoc { Title = Str(spec["title"]), Subtitle = Str(spec["subtitle"]) };
            AiDataset? D(JsonNode? id) => data.TryGetValue(Str(id), out var d) ? d : null;

            foreach (var k in (spec["kpis"] as JsonArray ?? new()).OfType<JsonObject>())
            {
                var d = D(k["dataset_id"]);
                string agg = Str(k["agg"]), col = Str(k["column"]), fmt = Str(k["format"]);
                decimal? val = null;
                if (d != null)
                {
                    int i = d.IndexOf(col);
                    var nums = i < 0 ? new List<decimal>() : d.Rows.Select(r => r[i]).OfType<decimal>().ToList();
                    val = agg switch
                    {
                        "count" => d.Rows.Count,
                        "sum" => nums.Sum(),
                        "avg" => nums.Count > 0 ? Math.Round(nums.Average(), 4) : null,
                        "min" => nums.Count > 0 ? nums.Min() : null,
                        "max" => nums.Count > 0 ? nums.Max() : null,
                        _ => d.Rows.Count > 0 && i >= 0 && d.Rows[0][i] is decimal f ? f : null,
                    };
                    if (fmt.Length == 0) fmt = agg == "count" ? AiReportFormats.Int : i >= 0 ? InferFormat(col, d.Types[i]) : AiReportFormats.Amount;
                }
                doc.Kpis.Add(new AiReportKpi { Label = Str(k["label"]), Value = val, Format = fmt.Length == 0 ? AiReportFormats.Amount : fmt });
            }

            foreach (var s in (spec["sections"] as JsonArray ?? new()).OfType<JsonObject>())
            {
                string type = Str(s["type"]);
                var sec = new AiReportSection { Kind = type, Title = Str(s["title"]), Text = Str(s["text"]) };
                if (type == "text") { doc.Sections.Add(sec); continue; }
                var d = D(s["dataset_id"]);
                if (d == null) continue;
                var rows = Ordered(d, s).ToList();
                int? top = Int(s["top"]);
                sec.TotalRows = rows.Count;
                if (top is > 0) rows = rows.Take(top.Value).ToList();

                if (type == "table")
                {
                    var cols = (s["columns"] as JsonArray ?? new()).OfType<JsonObject>()
                        .Select(c => (Key: Str(c["key"]), Label: Str(c["label"]), Fmt: Str(c["format"]))).ToList();
                    if (cols.Count == 0) cols = d.Columns.Select(c => (Key: c, Label: "", Fmt: "")).ToList();
                    var idx = new List<int>();
                    foreach (var c in cols)
                    {
                        int i = d.IndexOf(c.Key);
                        if (i < 0) continue;
                        idx.Add(i);
                        sec.Columns.Add(new AiReportColumn { Key = c.Key, Label = c.Label.Length > 0 ? c.Label : c.Key, Format = c.Fmt.Length > 0 ? c.Fmt : InferFormat(c.Key, d.Types[i]) });
                    }
                    var shown = rows.Take(MaxTableRows).ToList();
                    foreach (var r in shown) sec.Rows.Add(idx.Select(i => r[i]).ToArray()!);
                    sec.ShowTotals = (Bool(s["show_totals"]) ?? true) && shown.Count > 1;
                    for (int j = 0; j < sec.Columns.Count; j++)
                    {
                        var c = sec.Columns[j];
                        bool summable = c.Format is AiReportFormats.Amount or AiReportFormats.Qty ||
                                        (c.Format == AiReportFormats.Int && !c.Key.EndsWith("_id", StringComparison.OrdinalIgnoreCase) && !c.Key.EndsWith("_no", StringComparison.OrdinalIgnoreCase) && c.Key != "id");
                        sec.Totals.Add(summable ? rows.Select(r => r[idx[j]]).OfType<decimal>().Sum() : null);
                    }
                    if (!sec.Totals.Any(t => t != null)) sec.ShowTotals = false;
                }
                else
                {
                    sec.ChartKind = Str(s["chart_kind"]);
                    int xi = d.IndexOf(Str(s["x"]));
                    var ys = (s["y"] as JsonArray ?? new()).Select(Str).ToList();
                    var yl = (s["y_labels"] as JsonArray ?? new()).Select(Str).ToList();
                    var yi = ys.Select(d.IndexOf).ToList();
                    if (xi < 0 || yi.Any(i => i < 0)) continue;
                    string vf = Str(s["value_format"]);
                    sec.ValueFormat = vf.Length > 0 ? vf : InferFormat(ys[0], d.Types[yi[0]]);
                    int cap = sec.ChartKind == AiChartKinds.Pie ? MaxPieSlices : MaxChartPoints;
                    var pts = rows.Take(cap).ToList();
                    var rest = rows.Skip(cap).ToList();
                    sec.ChartLabels = pts.Select(r => Convert.ToString(r[xi], CultureInfo.InvariantCulture) ?? "").ToList();
                    for (int j = 0; j < ys.Count; j++)
                    {
                        var vals = pts.Select(r => r[yi[j]] is decimal m ? m : 0m).ToList();
                        sec.Series.Add(new AiChartSeries { Name = j < yl.Count && yl[j].Length > 0 ? yl[j] : ys[j], Values = vals });
                    }
                    // الدائرى: الباقى فى شريحة «أخرى» حتى يبقى المجموع صحيحاً.
                    if (sec.ChartKind == AiChartKinds.Pie && rest.Count > 0)
                    {
                        sec.ChartLabels.Add("أخرى");
                        sec.Series[0].Values.Add(rest.Select(r => r[yi[0]] is decimal m ? m : 0m).Sum());
                    }
                }
                doc.Sections.Add(sec);
            }
            return doc;
        }

        // ───── التخزين ─────

        private static readonly JsonSerializerOptions Store = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public static string DataJson(IReadOnlyDictionary<string, AiDataset> data)
        {
            var o = new JsonObject();
            foreach (var (id, d) in data) o[id] = d.ToJson(StoredRows);
            return o.ToJsonString(Store);
        }

        public static string SourcesJson(IReadOnlyDictionary<string, AiDataset> data)
        {
            var o = new JsonObject();
            foreach (var (id, d) in data)
                o[id] = new JsonObject { ["title"] = d.Title, ["tool"] = d.Tool, ["args"] = d.Args.DeepClone(), ["sql"] = d.Sql, ["rows"] = d.Rows.Count, ["truncated"] = d.Truncated };
            return o.ToJsonString(Store);
        }

        public static Dictionary<string, AiDataset> ReadData(string? json)
        {
            var map = new Dictionary<string, AiDataset>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json)) return map;
            try
            {
                if (JsonNode.Parse(json) is JsonObject o)
                    foreach (var (id, n) in o)
                        if (n is JsonObject d) map[id] = AiDataset.FromJson(d);
            }
            catch (JsonException) { }
            return map;
        }

        public static JsonObject ReadObject(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new JsonObject();
            try { return JsonNode.Parse(json) as JsonObject ?? new JsonObject(); }
            catch (JsonException) { return new JsonObject(); }
        }

        /// <summary>هل يعتمد المصدر على فترة (فيُعاد تشغيله بفترة جديدة)؟ الاستعلام بمعاملى @date_from/@date_to، والأدوات المالية بـ from/to.</summary>
        public static bool HasPeriodArgs(string tool, JsonObject args) => tool == "run_readonly_query"
            ? args.ContainsKey("date_from") && args.ContainsKey("date_to")
            : args.ContainsKey("from") && args.ContainsKey("to");

        /// <summary>مدخلات إعادة التشغيل: نفس الأداة بنفس المدخلات مع استبدال الفترة فقط.</summary>
        public static JsonObject RerunArgs(string tool, JsonObject args, DateTime? from, DateTime? to)
        {
            var a = (JsonObject)args.DeepClone();
            if (from == null || to == null || !HasPeriodArgs(tool, a)) return a;
            string f = from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), t = to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (tool == "run_readonly_query") { a["date_from"] = f; a["date_to"] = t; }
            else { a["from"] = f; a["to"] = t; }
            return a;
        }

        /// <summary>فترة التقرير: من الـ spec، وإلا من أول مصدر يعتمد على فترة.</summary>
        public static (DateTime? From, DateTime? To) PeriodOf(JsonObject spec, IReadOnlyDictionary<string, AiDataset> data)
        {
            DateTime? P(string s) => DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
            var f = P(Str(spec["date_from"])); var t = P(Str(spec["date_to"]));
            if (f != null && t != null) return (f, t);
            foreach (var d in data.Values.Where(d => HasPeriodArgs(d.Tool, d.Args)))
            {
                bool q = d.Tool == "run_readonly_query";
                return (P(Str(d.Args[q ? "date_from" : "from"])), P(Str(d.Args[q ? "date_to" : "to"])));
            }
            return (f, t);
        }
    }
}
