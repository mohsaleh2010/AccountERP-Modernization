using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// P7.4: جداول التقييم (AI_EvalCases / AI_EvalRuns / AI_EvalResults، جداول جديدة). لا قراءة ولا كتابة فى جداول النظام القائمة
    /// (إلا اسم المستخدم من USER_DETAILES للعرض).
    /// </summary>
    public class AiEvalCLS
    {
        private readonly Func<operations> _newOpp;

        public AiEvalCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        // ───────────── الحالات ─────────────

        private const string CaseSelect = @"SELECT Id, CaseKey, Question, Category, Dialect, CheckType, ExpectedTools, ForbiddenTools, ReferenceSql, ReferenceArgsJson,
       ExpectedText, Tags, Notes, Source, Source_Ref_Id, Version, IsEdited, IsActive
FROM AI_EvalCases";

        private static AiEvalCaseRow ReadCase(DbDataReader r) => new AiEvalCaseRow
        {
            Id = r.GetInt64(0),
            CaseKey = S(r, 1) ?? "",
            Question = S(r, 2) ?? "",
            Category = S(r, 3) ?? "",
            Dialect = S(r, 4) ?? "msa",
            CheckType = S(r, 5) ?? "",
            ExpectedTools = S(r, 6),
            ForbiddenTools = S(r, 7),
            ReferenceSql = S(r, 8),
            ReferenceArgsJson = S(r, 9),
            ExpectedText = S(r, 10),
            Tags = S(r, 11),
            Notes = S(r, 12),
            Source = S(r, 13) ?? "seed",
            Source_Ref_Id = r.IsDBNull(14) ? (long?)null : r.GetInt64(14),
            Version = r.IsDBNull(15) ? 1 : r.GetInt32(15),
            IsEdited = !r.IsDBNull(16) && r.GetBoolean(16),
            IsActive = r.IsDBNull(17) || r.GetBoolean(17),
        };

        public List<AiEvalCaseRow> ListCases()
        {
            var list = new List<AiEvalCaseRow>();
            Exec(_newOpp().db, CaseSelect + " ORDER BY Category, CaseKey", _ => { }, r => list.Add(ReadCase(r)));
            return list;
        }

        public AiEvalCaseRow GetCase(long id)
        {
            AiEvalCaseRow found = null;
            Exec(_newOpp().db, CaseSelect + " WHERE Id = @id", cmd => Add(cmd, "@id", id), r => found = ReadCase(r));
            return found;
        }

        public void InsertCase(AiEvalCaseRow c, long? userId)
        {
            if (c.Id == 0) c.Id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_EvalCases (Id, CaseKey, Question, Category, Dialect, CheckType, ExpectedTools, ForbiddenTools, ReferenceSql, ReferenceArgsJson,
                          ExpectedText, Tags, Notes, Source, Source_Ref_Id, Version, IsEdited, IsActive, CreatedBy, CreatedAt)
VALUES ({c.Id}, {Cut(c.CaseKey, 100)}, {Cut(c.Question, 2000)}, {Cut(c.Category, 30)}, {Cut(c.Dialect, 10)}, {Cut(c.CheckType, 20)},
        {Cut(c.ExpectedTools, 300)}, {Cut(c.ForbiddenTools, 300)}, {c.ReferenceSql}, {Cut(c.ReferenceArgsJson, 2000)},
        {Cut(c.ExpectedText, 1000)}, {Cut(c.Tags, 300)}, {Cut(c.Notes, 1000)}, {Cut(c.Source, 30)}, {c.Source_Ref_Id}, {c.Version}, {c.IsEdited}, {c.IsActive},
        {userId}, {DateTime.Now})");
        }

        /// <summary>تحديث حالة مزروعة من الكود (المستدعى يتحقق: Source=seed وغير معدّلة ونسخة أعلى).</summary>
        public void UpdateFromSeed(AiEvalCaseRow c) => Update(c, false, null);

        /// <summary>تعديل المدير: edited=true يحمى الحالة المزروعة من تحديثات الكود.</summary>
        public void UpdateByAdmin(AiEvalCaseRow c, long? userId) => Update(c, true, userId);

        private void Update(AiEvalCaseRow c, bool edited, long? userId)
        {
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_EvalCases SET Question = {Cut(c.Question, 2000)}, Category = {Cut(c.Category, 30)}, Dialect = {Cut(c.Dialect, 10)}, CheckType = {Cut(c.CheckType, 20)},
       ExpectedTools = {Cut(c.ExpectedTools, 300)}, ForbiddenTools = {Cut(c.ForbiddenTools, 300)}, ReferenceSql = {c.ReferenceSql},
       ReferenceArgsJson = {Cut(c.ReferenceArgsJson, 2000)}, ExpectedText = {Cut(c.ExpectedText, 1000)}, Tags = {Cut(c.Tags, 300)}, Notes = {Cut(c.Notes, 1000)},
       Version = {c.Version}, IsEdited = CASE WHEN {edited} = 1 THEN 1 ELSE IsEdited END, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {c.Id}");
        }

        public void SetCaseActive(long id, bool active, long? userId)
        {
            _newOpp().db.Database.ExecuteSqlInterpolated(
                $"UPDATE AI_EvalCases SET IsActive = {active}, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now} WHERE Id = {id}");
        }

        // ───────────── التشغيلات ─────────────

        private const string RunSelect = @"SELECT r.Id, r.Group_Id, r.User_Id, u.USER_NAME, r.Model_Id, r.ModelName, r.ProviderName, r.Effort, r.MaxTokens, r.Status,
       r.FilterJson, r.ConfigJson, r.PromptHash, r.TotalCases, r.DoneCases, r.PassedCases, r.FailedCases, r.ErrorCases, r.ToolOkCases,
       r.InputTokens, r.OutputTokens, r.CostEstimate, r.DurationMs, r.StartedAt, r.FinishedAt, r.ErrorText
FROM AI_EvalRuns r LEFT JOIN USER_DETAILES u ON u.Id = r.User_Id";

        private static AiEvalRunRow ReadRun(DbDataReader r) => new AiEvalRunRow
        {
            Id = r.GetInt64(0),
            Group_Id = r.GetInt64(1),
            User_Id = r.GetInt64(2),
            UserName = r.IsDBNull(3) ? "" : Convert.ToString(r.GetValue(3)) ?? "",
            Model_Id = r.GetInt64(4),
            ModelName = S(r, 5) ?? "",
            ProviderName = S(r, 6) ?? "",
            Effort = S(r, 7),
            MaxTokens = r.IsDBNull(8) ? (int?)null : r.GetInt32(8),
            Status = S(r, 9) ?? "",
            FilterJson = S(r, 10),
            ConfigJson = S(r, 11),
            PromptHash = S(r, 12)?.Trim(),
            TotalCases = r.GetInt32(13),
            DoneCases = r.GetInt32(14),
            PassedCases = r.GetInt32(15),
            FailedCases = r.GetInt32(16),
            ErrorCases = r.GetInt32(17),
            ToolOkCases = r.GetInt32(18),
            InputTokens = r.IsDBNull(19) ? (long?)null : r.GetInt64(19),
            OutputTokens = r.IsDBNull(20) ? (long?)null : r.GetInt64(20),
            CostEstimate = r.IsDBNull(21) ? (decimal?)null : r.GetDecimal(21),
            DurationMs = r.IsDBNull(22) ? (long?)null : r.GetInt64(22),
            StartedAt = r.IsDBNull(23) ? (DateTime?)null : r.GetDateTime(23),
            FinishedAt = r.IsDBNull(24) ? (DateTime?)null : r.GetDateTime(24),
            ErrorText = S(r, 25),
        };

        public void InsertRun(AiEvalRunRow x)
        {
            if (x.Id == 0) x.Id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_EvalRuns (Id, Group_Id, User_Id, Model_Id, ModelName, ProviderName, Effort, MaxTokens, Status, FilterJson, ConfigJson, PromptHash,
                         TotalCases, DoneCases, PassedCases, FailedCases, ErrorCases, ToolOkCases, InputTokens, OutputTokens, CostEstimate, DurationMs,
                         StartedAt, CreatedBy, CreatedAt)
VALUES ({x.Id}, {x.Group_Id}, {x.User_Id}, {x.Model_Id}, {Cut(x.ModelName, 200)}, {Cut(x.ProviderName, 200)}, {Cut(x.Effort, 20)}, {x.MaxTokens},
        {Cut(x.Status, 20)}, {Cut(x.FilterJson, 2000)}, {x.ConfigJson}, {x.PromptHash}, {x.TotalCases}, 0, 0, 0, 0, 0, 0, 0, NULL, 0,
        {x.StartedAt ?? DateTime.Now}, {x.User_Id}, {DateTime.Now})");
        }

        /// <summary>عدّادات التشغيل بعد كل حالة (تُقرأ للمتابعة الحية).</summary>
        public void UpdateProgress(AiEvalRunRow x)
        {
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_EvalRuns SET DoneCases = {x.DoneCases}, PassedCases = {x.PassedCases}, FailedCases = {x.FailedCases}, ErrorCases = {x.ErrorCases},
       ToolOkCases = {x.ToolOkCases}, InputTokens = {x.InputTokens}, OutputTokens = {x.OutputTokens}, CostEstimate = {AiSqlParams.Cost(x.CostEstimate)},
       DurationMs = {x.DurationMs}, UpdatedAt = {DateTime.Now}
WHERE Id = {x.Id}");
        }

        /// <summary>ينهى تشغيلاً ما زال Running فقط (لا يكتب فوق حالة نهائية).</summary>
        public void Finish(long runId, string status, string errorText)
        {
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_EvalRuns SET Status = {Cut(status, 20)}, ErrorText = {Cut(errorText, 1000)}, FinishedAt = {DateTime.Now}, UpdatedAt = {DateTime.Now}
WHERE Id = {runId} AND Status = {AiEvalRunStatus.Running}");
        }

        /// <summary>تشغيلات بقيت Running من عملية سابقة للخادم ⇒ «انقطع». يعيد عددها.</summary>
        public int MarkInterrupted(IEnumerable<long> exceptRunIds)
        {
            var keep = new HashSet<long>(exceptRunIds ?? Enumerable.Empty<long>());
            var running = new List<long>();
            Exec(_newOpp().db, "SELECT Id FROM AI_EvalRuns WHERE Status = @s", cmd => Add(cmd, "@s", AiEvalRunStatus.Running), r => running.Add(r.GetInt64(0)));
            int n = 0;
            var db = _newOpp().db;
            foreach (long id in running.Where(i => !keep.Contains(i)))
                n += db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_EvalRuns SET Status = {AiEvalRunStatus.Interrupted}, ErrorText = {"انقطع التشغيل (أُعيد تشغيل الخادم أثناءه)."}, FinishedAt = {DateTime.Now}
WHERE Id = {id} AND Status = {AiEvalRunStatus.Running}");
            return n;
        }

        public AiEvalRunRow GetRun(long id)
        {
            AiEvalRunRow found = null;
            Exec(_newOpp().db, RunSelect + " WHERE r.Id = @id", cmd => Add(cmd, "@id", id), r => found = ReadRun(r));
            return found;
        }

        public List<AiEvalRunRow> RunsOfGroup(long groupId)
        {
            var list = new List<AiEvalRunRow>();
            Exec(_newOpp().db, RunSelect + " WHERE r.Group_Id = @g ORDER BY r.Id", cmd => Add(cmd, "@g", groupId), r => list.Add(ReadRun(r)));
            return list;
        }

        public List<AiEvalRunRow> ListRuns(int top = 100)
        {
            if (top <= 0 || top > 1000) top = 100;
            var list = new List<AiEvalRunRow>();
            Exec(_newOpp().db, RunSelect.Replace("SELECT r.Id", "SELECT TOP (@top) r.Id") + " ORDER BY r.StartedAt DESC, r.Id DESC",
                cmd => Add(cmd, "@top", top), r => list.Add(ReadRun(r)));
            return list;
        }

        /// <summary>متوسط التوكنز لكل حالة فى تشغيلات سابقة لنفس النموذج (للتكلفة التقديرية)، أو null بلا تاريخ.</summary>
        public (double In, double Out)? AvgTokensPerCase(long modelId)
        {
            (double, double)? found = null;
            Exec(_newOpp().db, @"SELECT AVG(CAST(res.InputTokens AS float)), AVG(CAST(res.OutputTokens AS float)), COUNT(*)
FROM AI_EvalResults res INNER JOIN AI_EvalRuns r ON r.Id = res.Run_Id
WHERE r.Model_Id = @m AND res.InputTokens IS NOT NULL AND res.Outcome <> 'skipped'",
                cmd => Add(cmd, "@m", modelId),
                r => { if (!r.IsDBNull(0) && r.GetInt32(2) >= 3) found = (r.GetDouble(0), r.IsDBNull(1) ? 0 : r.GetDouble(1)); });
            return found;
        }

        // ───────────── النتائج ─────────────

        public void InsertResult(AiEvalResultRow x)
        {
            if (x.Id == 0) x.Id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_EvalResults (Id, Run_Id, Case_Id, CaseKey, CaseVersion, Outcome, ToolOk, FailReason, AnswerText, ToolsJson, Rounds,
                            InputTokens, OutputTokens, CostEstimate, DurationMs, CreatedAt)
VALUES ({x.Id}, {x.Run_Id}, {x.Case_Id}, {Cut(x.CaseKey, 100)}, {x.CaseVersion}, {Cut(x.Outcome, 20)}, {x.ToolOk}, {Cut(x.FailReason, 1000)},
        {x.AnswerText}, {x.ToolsJson}, {x.Rounds}, {x.InputTokens}, {x.OutputTokens}, {AiSqlParams.Cost(x.CostEstimate)}, {x.DurationMs}, {DateTime.Now})");
        }

        public List<AiEvalResultRow> ListResults(long runId)
        {
            var list = new List<AiEvalResultRow>();
            Exec(_newOpp().db, @"SELECT x.Id, x.Run_Id, x.Case_Id, x.CaseKey, x.CaseVersion, x.Outcome, x.ToolOk, x.FailReason, x.AnswerText, x.ToolsJson, x.Rounds,
       x.InputTokens, x.OutputTokens, x.CostEstimate, x.DurationMs, x.CreatedAt, c.Question, c.Category, c.CheckType
FROM AI_EvalResults x LEFT JOIN AI_EvalCases c ON c.Id = x.Case_Id
WHERE x.Run_Id = @r ORDER BY x.CreatedAt, x.Id", cmd => Add(cmd, "@r", runId),
                r => list.Add(new AiEvalResultRow
                {
                    Id = r.GetInt64(0),
                    Run_Id = r.GetInt64(1),
                    Case_Id = r.GetInt64(2),
                    CaseKey = S(r, 3) ?? "",
                    CaseVersion = r.GetInt32(4),
                    Outcome = S(r, 5) ?? "",
                    ToolOk = !r.IsDBNull(6) && r.GetBoolean(6),
                    FailReason = S(r, 7),
                    AnswerText = S(r, 8),
                    ToolsJson = S(r, 9),
                    Rounds = r.IsDBNull(10) ? (int?)null : r.GetInt32(10),
                    InputTokens = r.IsDBNull(11) ? (int?)null : r.GetInt32(11),
                    OutputTokens = r.IsDBNull(12) ? (int?)null : r.GetInt32(12),
                    CostEstimate = r.IsDBNull(13) ? (decimal?)null : r.GetDecimal(13),
                    DurationMs = r.IsDBNull(14) ? (int?)null : r.GetInt32(14),
                    CreatedAt = r.IsDBNull(15) ? (DateTime?)null : r.GetDateTime(15),
                    Question = S(r, 16) ?? "",
                    Category = S(r, 17) ?? "",
                    CheckType = S(r, 18) ?? "",
                }));
            return list;
        }

        // ───────────── مساعدات ─────────────

        private static string S(DbDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i));

        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

        private static void Add(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static void Exec(My_DbContext db, string sql, Action<DbCommand> bind, Action<DbDataReader> row)
        {
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
