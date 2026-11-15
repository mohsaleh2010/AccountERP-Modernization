using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.8.1 (ai-assistant-p88-plan.md §4): طبقة بيانات AI_Suggestions وAI_Suggestions_Vectors بنمط AiCollectCLS.
    /// الكتابة على هذين الجدولين فقط.
    /// </summary>
    public class AiSuggestCLS
    {
        private readonly Func<operations> _newOpp;

        public AiSuggestCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public long Insert(AiSuggestionRow s)
        {
            long id = s.Id != 0 ? s.Id : CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Suggestions (Id, Kind, User_Id, Branch_Id, InputText, InputHash, SuggestedJson, Source, Confidence, Outcome, Screen, CreatedBy, CreatedAt)
VALUES ({id}, {Cut(s.Kind, 20)}, {s.User_Id}, {s.Branch_Id}, {Cut(s.InputText, 400)}, {Cut(s.InputHash, 64)}, {s.SuggestedJson}, {Cut(s.Source, 20)},
        {AiSqlParams.Dec(s.Confidence, 9, 4)}, {Cut(s.Outcome ?? AiSuggestOutcomes.Shown, 20)}, {Cut(s.Screen, 200)}, {s.User_Id}, {DateTime.Now})");
            s.Id = id;
            return id;
        }

        /// <summary>ص14: يسجل قرار المستخدم على اقتراحه هو فقط. false إن لم يوجد أو لم يكن له.</summary>
        public bool RecordOutcome(long id, long userId, string outcome, string finalJson)
        {
            if (!AiSuggestOutcomes.IsValid(outcome)) return false;
            return _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Suggestions SET Outcome = {outcome}, FinalJson = {finalJson}, DecidedAt = {DateTime.Now}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND User_Id = {userId}") == 1;
        }

        public AiSuggestionRow Get(long id)
        {
            AiSuggestionRow row = null;
            Exec(@"SELECT Id, Kind, User_Id, Branch_Id, InputText, InputHash, SuggestedJson, Source, Confidence, Outcome, FinalJson, Screen, CreatedAt, DecidedAt
FROM AI_Suggestions WHERE Id = @id", c => Add(c, "@id", id), r => row = new AiSuggestionRow
            {
                Id = r.GetInt64(0),
                Kind = S(r, 1),
                User_Id = r.GetInt64(2),
                Branch_Id = r.IsDBNull(3) ? 0 : r.GetInt64(3),
                InputText = S(r, 4),
                InputHash = S(r, 5),
                SuggestedJson = S(r, 6),
                Source = S(r, 7),
                Confidence = r.IsDBNull(8) ? (decimal?)null : Convert.ToDecimal(r.GetValue(8), CultureInfo.InvariantCulture),
                Outcome = S(r, 9),
                FinalJson = S(r, 10),
                Screen = S(r, 11),
                CreatedAt = r.IsDBNull(12) ? (DateTime?)null : r.GetDateTime(12),
                DecidedAt = r.IsDBNull(13) ? (DateTime?)null : r.GetDateTime(13),
            });
            return row;
        }

        /// <summary>ص18: نسبة القبول آخر N يوماً لكل نوع.</summary>
        public List<AiSuggestStats> Stats(int days)
        {
            var map = new Dictionary<string, AiSuggestStats>(StringComparer.Ordinal);
            Exec(@"SELECT Kind, Outcome, COUNT(*) FROM AI_Suggestions WHERE CreatedAt >= @from GROUP BY Kind, Outcome",
                c => Add(c, "@from", DateTime.Now.AddDays(-Math.Max(1, days))), r =>
                {
                    string k = S(r, 0) ?? "";
                    AiSuggestStats st;
                    if (!map.TryGetValue(k, out st)) map[k] = st = new AiSuggestStats { Kind = k };
                    int n = Convert.ToInt32(r.GetValue(2), CultureInfo.InvariantCulture);
                    switch (S(r, 1))
                    {
                        case AiSuggestOutcomes.Applied: st.Applied += n; break;
                        case AiSuggestOutcomes.Dismissed: st.Dismissed += n; break;
                        case AiSuggestOutcomes.Overridden: st.Overridden += n; break;
                        default: st.Shown += n; break;
                    }
                });
            return new List<AiSuggestStats>(map.Values);
        }

        /// <summary>عدد اقتراحات المستخدم اليوم (حد يومى للنموذج).</summary>
        public int CountModelToday(long userId)
        {
            int n = 0;
            Exec("SELECT COUNT(*) FROM AI_Suggestions WHERE User_Id = @u AND Source = N'model' AND CreatedAt >= @d",
                c => { Add(c, "@u", userId); Add(c, "@d", DateTime.Today); }, r => n = Convert.ToInt32(r.GetValue(0), CultureInfo.InvariantCulture));
            return n;
        }

        // ───────────── السوابق (ص6، ص7، ص16) ─────────────

        /// <summary>
        /// سوابق سندات المصروف/القبض (ai.v_expense_history) لكل الصناديق (ai_box_perm = 0؛ الترشيح بالفروع فى الخدمة).
        /// قراءة مباشرة لأن ReadOnlySqlExecutor يقف عند 5000 صف؛ SESSION_CONTEXT يُضبط على نفس الاتصال ويمسحه إرجاعه للمجمّع.
        /// </summary>
        public List<AiSuggestPrecedent> LoadExpenseHistory(int months, int maxRows)
        {
            var list = new List<AiSuggestPrecedent>();
            ExecWithContext(@"SELECT TOP (@max) voucher_date, ISNULL(transaction_type, 0), notes, sub_account_id, account_code, account_name,
       ISNULL(cost_center_id, 0), cost_center_name, ISNULL(branch_id, 0)
FROM ai.v_expense_history
WHERE voucher_date >= @from AND notes IS NOT NULL AND LEN(notes) >= 2
ORDER BY voucher_date DESC, line_id DESC",
                c => { Add(c, "@max", Math.Max(1, maxRows)); Add(c, "@from", DateTime.Today.AddMonths(-Math.Max(1, months))); },
                r => list.Add(new AiSuggestPrecedent
                {
                    Date = r.IsDBNull(0) ? DateTime.MinValue : r.GetDateTime(0),
                    SubType = Convert.ToInt32(r.GetValue(1), CultureInfo.InvariantCulture),
                    Text = S(r, 2),
                    TargetId = r.IsDBNull(3) ? 0 : Convert.ToInt64(r.GetValue(3), CultureInfo.InvariantCulture),
                    TargetCode = S(r, 4),
                    TargetName = S(r, 5),
                    SecondaryId = Convert.ToInt64(r.GetValue(6), CultureInfo.InvariantCulture),
                    SecondaryName = S(r, 7),
                    BranchId = Convert.ToInt64(r.GetValue(8), CultureInfo.InvariantCulture),
                }));
            return list;
        }

        /// <summary>سوابق الأصناف (ai.v_item_classes): الهدف = الفئة، الثانوى = المجموعة، النوع = صنف/خدمة.</summary>
        public List<AiSuggestPrecedent> LoadItemClasses(int maxRows)
        {
            var list = new List<AiSuggestPrecedent>();
            ExecWithContext(@"SELECT TOP (@max) ISNULL(created_date, '2000-01-01'), item_or_service, item_name, category_id, category_name,
       ISNULL(group_id, 0), group_name
FROM ai.v_item_classes WHERE item_name IS NOT NULL ORDER BY item_id DESC",
                c => Add(c, "@max", Math.Max(1, maxRows)),
                r => list.Add(new AiSuggestPrecedent
                {
                    Date = r.GetDateTime(0),
                    SubType = Convert.ToInt32(r.GetValue(1), CultureInfo.InvariantCulture),
                    Text = S(r, 2),
                    TargetId = Convert.ToInt64(r.GetValue(3), CultureInfo.InvariantCulture),
                    TargetName = S(r, 4),
                    SecondaryId = Convert.ToInt64(r.GetValue(5), CultureInfo.InvariantCulture),
                    SecondaryName = S(r, 6),
                }));
            return list;
        }

        /// <summary>هل الحساب الفرعى ما زال موجوداً (لا اقتراح لحساب محذوف).</summary>
        public HashSet<long> ExistingSubAccounts(IEnumerable<long> ids)
        {
            var set = new HashSet<long>();
            var arr = new List<long>(ids ?? new long[0]);
            if (arr.Count == 0) return set;
            var parts = new List<string>();
            foreach (var x in arr) parts.Add(x.ToString(CultureInfo.InvariantCulture));
            Exec("SELECT Id FROM Submain_Account WHERE Id IN (" + string.Join(",", parts) + ")", c => { },
                r => set.Add(Convert.ToInt64(r.GetValue(0), CultureInfo.InvariantCulture)));
            return set;
        }

        // ───────────── المتجهات (ص8) ─────────────

        /// <summary>كل متجهات نوع لنموذج تضمين: TextHash → (Text, Embedding).</summary>
        public Dictionary<string, KeyValuePair<string, byte[]>> LoadVectors(string kind, long modelId)
        {
            var map = new Dictionary<string, KeyValuePair<string, byte[]>>(StringComparer.Ordinal);
            Exec(@"SELECT TextHash, Text, Embedding FROM AI_Suggestions_Vectors WHERE Kind = @k AND EmbeddingModel_Id = @m AND Embedding IS NOT NULL",
                c => { Add(c, "@k", kind); Add(c, "@m", modelId); },
                r => map[r.GetString(0)] = new KeyValuePair<string, byte[]>(r.GetString(1), (byte[])r.GetValue(2)));
            return map;
        }

        public HashSet<string> VectorHashes(string kind, long modelId)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            Exec("SELECT TextHash FROM AI_Suggestions_Vectors WHERE Kind = @k AND EmbeddingModel_Id = @m",
                c => { Add(c, "@k", kind); Add(c, "@m", modelId); }, r => set.Add(r.GetString(0)));
            return set;
        }

        public int CountVectors(string kind)
        {
            int n = 0;
            Exec("SELECT COUNT(*) FROM AI_Suggestions_Vectors WHERE Kind = @k AND Embedding IS NOT NULL",
                c => Add(c, "@k", kind), r => n = Convert.ToInt32(r.GetValue(0), CultureInfo.InvariantCulture));
            return n;
        }

        public void InsertVector(string kind, string textHash, string text, byte[] embedding, long modelId, int dim)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            var blob = new SqlParameter("@e", SqlDbType.VarBinary, -1) { Value = (object)embedding ?? DBNull.Value };
            _newOpp().db.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM AI_Suggestions_Vectors WHERE Kind = @k AND TextHash = @h AND EmbeddingModel_Id = @m)
INSERT INTO AI_Suggestions_Vectors (Id, Kind, TextHash, Text, Embedding, EmbeddingModel_Id, EmbeddingDim, CreatedAt)
VALUES (@id, @k, @h, @t, @e, @m, @d, GETDATE())",
                new SqlParameter("@id", id), new SqlParameter("@k", kind), new SqlParameter("@h", textHash),
                new SqlParameter("@t", Cut(text, 400) ?? ""), blob, new SqlParameter("@m", modelId), new SqlParameter("@d", dim));
        }

        /// <summary>عند تغيير نموذج التضمين: يحذف متجهات النماذج الأخرى (جدولنا فقط).</summary>
        public int DeleteVectorsExceptModel(long modelId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_Suggestions_Vectors WHERE EmbeddingModel_Id <> {modelId}");

        // ───────────── أدوات ─────────────

        private static string S(DbDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture);
        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

        private static void Add(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private void ExecWithContext(string sql, Action<DbCommand> bind, Action<DbDataReader> row)
        {
            var db = _newOpp().db;
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using (var ctx = conn.CreateCommand())
                {
                    ctx.CommandText = "EXEC sp_set_session_context @key = N'ai_box_perm', @value = 0; EXEC sp_set_session_context @key = N'ai_tax', @value = -1;";
                    ctx.ExecuteNonQuery();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandTimeout = 120;
                    bind(cmd);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) row(r);
                }
            }
            finally
            {
                try
                {
                    using (var clr = conn.CreateCommand())
                    {
                        clr.CommandText = "EXEC sp_set_session_context @key = N'ai_box_perm', @value = NULL; EXEC sp_set_session_context @key = N'ai_tax', @value = NULL;";
                        clr.ExecuteNonQuery();
                    }
                }
                catch (Exception) { }
                if (wasClosed) conn.Close();
            }
        }

        private void Exec(string sql, Action<DbCommand> bind, Action<DbDataReader> row)
        {
            var db = _newOpp().db;
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    bind(cmd);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) row(r);
                }
            }
            finally { if (wasClosed) conn.Close(); }
        }
    }
}
