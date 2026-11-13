using System.Data;
using AccountModelsLib;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace AccountERP.AI.Tools
{
    /// <summary>نتيجة استعلام قراءة: الأعمدة والصفوف (حتى الحد الأقصى) وهل قُطعت.</summary>
    public sealed class AiQueryResult
    {
        public List<string> Columns { get; } = new();
        public List<object?[]> Rows { get; } = new();
        public bool Truncated { get; set; }
        public long DurationMs { get; set; }
    }

    /// <summary>رفض استعلام قبل تنفيذه (رسالة عربية تُعاد للنموذج وتُسجَّل فى التدقيق).</summary>
    public sealed class AiSqlRejectedException(string message) : Exception(message);

    /// <summary>
    /// مسار القراءة الوحيد للمساعد (ai-assistant-plan.md §2.1). دفاع متعدد الطبقات:
    /// 1) ScriptDom: عبارة SELECT واحدة فقط، بلا INTO / متغيرات غير معرّفة / دوال بمخطط / OPENROWSET·OPENQUERY / أسماء ثلاثية أو رباعية،
    ///    وكل جدول مرجعى View من schema ai ضمن ما يحق للمستخدم (أو اسم CTE معرّف فى نفس الاستعلام).
    /// 2) الاتصال: ConnectionStrings:AiReadOnly (حساب SELECT على schema ai فقط — يُنشئه المسؤول بسكربت docs) إن وُجد، وإلا الاتصال الرئيسى.
    /// 3) التنفيذ داخل معاملة تُلغى دائماً (Rollback) + مهلة 15 ثانية + حد 5000 صف + SESSION_CONTEXT للقيود (ai_tax).
    /// </summary>
    public sealed class ReadOnlySqlExecutor
    {
        public const int MaxRows = 5000;
        public const int TimeoutSeconds = 15;
        public const int MaxSqlChars = 8000;

        private readonly string? _readOnlyConn;

        public ReadOnlySqlExecutor(IConfiguration? config)
        {
            string? cs = config?.GetConnectionString("AiReadOnly");
            _readOnlyConn = string.IsNullOrWhiteSpace(cs) ? null : cs;
        }

        public bool HasDedicatedLogin => _readOnlyConn != null;

        private string ConnectionString => _readOnlyConn ?? operations.ReturnConnStr();

        /// <summary>
        /// يتحقق من النص دون تنفيذ. allowedViews = أسماء Views (بلا schema) المسموحة لهذا المستخدم؛ parameters = أسماء المعاملات المسموح الإشارة إليها (@p0...).
        /// يرمى AiSqlRejectedException برسالة عربية.
        /// </summary>
        public static void Validate(string sql, ISet<string> allowedViews, ISet<string>? parameters = null)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new AiSqlRejectedException("الاستعلام فارغ.");
            if (sql.Length > MaxSqlChars) throw new AiSqlRejectedException($"الاستعلام أطول من الحد ({MaxSqlChars} حرف).");

            var parser = new TSql160Parser(initialQuotedIdentifiers: true);
            TSqlFragment tree;
            IList<ParseError> errors;
            using (var reader = new StringReader(sql)) tree = parser.Parse(reader, out errors);
            if (errors.Count > 0)
                throw new AiSqlRejectedException("صيغة الاستعلام غير صحيحة: " + errors[0].Message + $" (سطر {errors[0].Line}).");

            if (tree is not TSqlScript script || script.Batches.Count != 1 || script.Batches[0].Statements.Count != 1)
                throw new AiSqlRejectedException("مسموح بعبارة SELECT واحدة فقط (لا عبارات متعددة ولا GO).");
            if (script.Batches[0].Statements[0] is not SelectStatement select)
                throw new AiSqlRejectedException("مسموح بالقراءة فقط (SELECT). أى تعديل أو حذف أو إضافة أو تنفيذ إجراءات مرفوض.");
            if (select.Into != null)
                throw new AiSqlRejectedException("SELECT ... INTO غير مسموح (ينشئ جدولاً).");

            var v = new GuardVisitor(allowedViews, parameters ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            // أسماء الـ CTE المعرّفة فى نفس الاستعلام مسموحة كمصادر.
            if (select.WithCtesAndXmlNamespaces?.CommonTableExpressions != null)
                foreach (var cte in select.WithCtesAndXmlNamespaces.CommonTableExpressions)
                    v.CteNames.Add(cte.ExpressionName.Value);
            select.Accept(v);
            if (v.Error != null) throw new AiSqlRejectedException(v.Error);
            if (v.TableCount == 0) throw new AiSqlRejectedException("الاستعلام لا يقرأ من أى View (استخدم ai.v_... ).");
        }

        /// <summary>
        /// ينفّذ استعلاماً سبق التحقق منه (يُعاد التحقق هنا احتياطاً). sessionValues تُضبط بـ sp_set_session_context للقراءة فقط
        /// (ai_tax: -1/0/1). المعاملة تُلغى دائماً حتى لو تسلل شىء، فلا يبقى أى أثر.
        /// </summary>
        public AiQueryResult Execute(string sql, ISet<string> allowedViews, IReadOnlyDictionary<string, object> sessionValues,
                                     IReadOnlyDictionary<string, object>? parameters, CancellationToken ct)
        {
            var paramNames = new HashSet<string>(parameters?.Keys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            Validate(sql, allowedViews, paramNames);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = new AiQueryResult();
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using var tx = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            try
            {
                foreach (var (key, value) in sessionValues)
                {
                    using var ctx = new SqlCommand("EXEC sys.sp_set_session_context @key = @k, @value = @v, @read_only = 1;", conn, tx);
                    ctx.Parameters.AddWithValue("@k", key);
                    ctx.Parameters.AddWithValue("@v", value);
                    ctx.ExecuteNonQuery();
                }
                using var cmd = new SqlCommand(sql, conn, tx) { CommandTimeout = TimeoutSeconds };
                if (parameters != null)
                    foreach (var (name, value) in parameters)
                        cmd.Parameters.AddWithValue(name.StartsWith('@') ? name : "@" + name, value ?? DBNull.Value);
                using var reg = ct.Register(() => { try { cmd.Cancel(); } catch { } });
                using (var r = cmd.ExecuteReader())
                {
                    for (int i = 0; i < r.FieldCount; i++)
                        result.Columns.Add(string.IsNullOrEmpty(r.GetName(i)) ? "col" + (i + 1) : r.GetName(i));
                    while (r.Read())
                    {
                        if (result.Rows.Count >= MaxRows) { result.Truncated = true; break; }
                        var row = new object?[r.FieldCount];
                        for (int i = 0; i < r.FieldCount; i++) row[i] = r.IsDBNull(i) ? null : r.GetValue(i);
                        result.Rows.Add(row);
                    }
                    if (result.Truncated) cmd.Cancel();
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                throw new AiSqlRejectedException($"تجاوز الاستعلام المهلة ({TimeoutSeconds} ثانية). ضيّق الفترة أو أضف شروطاً.");
            }
            catch (SqlException ex) when (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException(ex.Message, ex, ct);
            }
            catch (SqlException ex)
            {
                // خطأ فى الاستعلام نفسه (عمود غير موجود...): يعود للنموذج ليصحح، بلا تفاصيل الاتصال.
                throw new AiSqlRejectedException("خطأ من قاعدة البيانات: " + ex.Message);
            }
            finally
            {
                try { tx.Rollback(); } catch { /* الاتصال قد يكون أُغلق بالإلغاء */ }
            }
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
            return result;
        }

        /// <summary>يرفض كل ما ليس قراءة من Views المسموحة.</summary>
        private sealed class GuardVisitor(ISet<string> allowedViews, ISet<string> parameters) : TSqlFragmentVisitor
        {
            public readonly HashSet<string> CteNames = new(StringComparer.OrdinalIgnoreCase);
            public string? Error;
            public int TableCount;

            private void Deny(string msg) { Error ??= msg; }

            public override void ExplicitVisit(NamedTableReference node)
            {
                var so = node.SchemaObject;
                if (so.ServerIdentifier != null || so.DatabaseIdentifier != null)
                    Deny("الأسماء الثلاثية/الرباعية (قاعدة أو خادم آخر) غير مسموحة.");
                else if (so.SchemaIdentifier == null && CteNames.Contains(so.BaseIdentifier.Value))
                { /* CTE معرّف داخل الاستعلام */ }
                else if (so.SchemaIdentifier == null || !string.Equals(so.SchemaIdentifier.Value, AiReadViewsCLS.Schema, StringComparison.OrdinalIgnoreCase))
                    Deny($"المصدر «{Name(so)}» غير مسموح. اقرأ فقط من Views المساعد ai.v_... المذكورة فى التعليمات.");
                else if (!allowedViews.Contains(so.BaseIdentifier.Value))
                    Deny($"لا تملك صلاحية قراءة «{Name(so)}» (الشاشة المقابلة غير مسموحة لك) أو أنه غير موجود.");
                else TableCount++;
                base.ExplicitVisit(node);
            }

            private static string Name(SchemaObjectName so) =>
                string.Join(".", new[] { so.ServerIdentifier, so.DatabaseIdentifier, so.SchemaIdentifier, so.BaseIdentifier }.Where(i => i != null).Select(i => i!.Value));

            public override void ExplicitVisit(SchemaObjectFunctionTableReference node) { Deny("دوال الجداول المعرّفة بمخطط غير مسموحة."); }
            public override void ExplicitVisit(OpenRowsetTableReference node) { Deny("OPENROWSET غير مسموح."); }
            public override void ExplicitVisit(BulkOpenRowset node) { Deny("OPENROWSET غير مسموح."); }
            public override void ExplicitVisit(OpenQueryTableReference node) { Deny("OPENQUERY غير مسموح."); }
            public override void ExplicitVisit(OpenXmlTableReference node) { Deny("OPENXML غير مسموح."); }
            public override void ExplicitVisit(AdHocTableReference node) { Deny("OPENDATASOURCE غير مسموح."); }
            public override void ExplicitVisit(FullTextTableReference node) { Deny("بحث النص الكامل غير مسموح."); }
            public override void ExplicitVisit(VariableTableReference node) { Deny("متغيرات الجداول غير مسموحة."); }
            public override void ExplicitVisit(VariableMethodCallTableReference node) { Deny("متغيرات الجداول غير مسموحة."); }
            public override void ExplicitVisit(ChangeTableChangesTableReference node) { Deny("CHANGETABLE غير مسموح."); }
            public override void ExplicitVisit(ChangeTableVersionTableReference node) { Deny("CHANGETABLE غير مسموح."); }
            public override void ExplicitVisit(DataModificationTableReference node) { Deny("تعديل البيانات غير مسموح."); }
            public override void ExplicitVisit(OpenJsonTableReference node) { base.ExplicitVisit(node); }

            public override void ExplicitVisit(FunctionCall node)
            {
                // الدوال المدمجة فقط (SUM, YEAR, FORMAT...)؛ دالة بمخطط (dbo.fn) أو استدعاء على هدف قد تنفّذ منطقاً غير معروف.
                if (node.CallTarget != null) Deny($"الدالة «{node.FunctionName?.Value}» باستدعاء مؤهَّل غير مسموحة؛ استخدم الدوال المدمجة فقط.");
                base.ExplicitVisit(node);
            }

            public override void ExplicitVisit(VariableReference node)
            {
                if (!parameters.Contains(node.Name) && !parameters.Contains(node.Name.TrimStart('@')))
                    Deny("المتغيرات غير مسموحة فى الاستعلام؛ ضع القيم مباشرة.");
                base.ExplicitVisit(node);
            }

            public override void ExplicitVisit(SelectSetVariable node) { Deny("إسناد المتغيرات غير مسموح."); }
        }
    }
}
