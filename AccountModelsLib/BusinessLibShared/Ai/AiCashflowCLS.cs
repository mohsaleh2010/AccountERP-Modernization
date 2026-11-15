using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>P8.7.1 (ai-assistant-p87-plan.md §3): طبقة بيانات AI_CashflowRuns/Adjustments/Exclusions (نمط AiCollectCLS). الكتابة على هذه الجداول فقط.</summary>
    public class AiCashflowCLS
    {
        private readonly Func<operations> _newOpp;

        public AiCashflowCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        // ───────────── اللقطات ─────────────

        public long InsertRun(AiCashflowRunRow r)
        {
            long id = r.Id != 0 ? r.Id : CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_CashflowRuns (Id, User_Id, BranchIds, AsOf, Scenario, Source, FactsJson, Narrative, Language, Status, CreatedBy, CreatedAt)
VALUES ({id}, {r.User_Id}, {Cut(r.BranchIds, 400)}, {r.AsOf.Date}, {Cut(r.Scenario, 20)}, {Cut(r.Source ?? "manual", 20)}, {r.FactsJson}, {r.Narrative},
        {Cut(r.Language, 5)}, {Cut(r.Status ?? "Done", 20)}, {r.User_Id}, {DateTime.Now})");
            r.Id = id;
            return id;
        }

        public void SetNarrative(long id, string narrative, string lang) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($"UPDATE AI_CashflowRuns SET Narrative = {narrative}, Language = {Cut(lang, 5)}, UpdatedAt = {DateTime.Now} WHERE Id = {id}");

        /// <summary>أحدث لقطة للمستخدم ونطاق الفروع (اختيارياً قبل تاريخ).</summary>
        public AiCashflowRunRow LatestRun(long userId, string branchIds, DateTime? before = null)
        {
            AiCashflowRunRow row = null;
            Exec(@"SELECT TOP (1) Id, User_Id, BranchIds, AsOf, Scenario, Source, FactsJson, Narrative, Language, Status, CreatedAt
FROM AI_CashflowRuns WHERE User_Id = @u AND ISNULL(BranchIds, N'') = @b AND Status = N'Done' AND (@before IS NULL OR CreatedAt < @before)
ORDER BY CreatedAt DESC", c => { Add(c, "@u", userId); Add(c, "@b", branchIds ?? ""); Add(c, "@before", before.HasValue ? (object)before.Value : DBNull.Value); },
                r => row = ReadRun(r));
            return row;
        }

        /// <summary>لقطة أُخذت فى أسبوع سابق (للدقة): أحدث لقطة AsOf داخل [weekStart، weekStart+7).</summary>
        public AiCashflowRunRow RunForWeek(long userId, string branchIds, DateTime weekStart)
        {
            AiCashflowRunRow row = null;
            Exec(@"SELECT TOP (1) Id, User_Id, BranchIds, AsOf, Scenario, Source, FactsJson, Narrative, Language, Status, CreatedAt
FROM AI_CashflowRuns WHERE User_Id = @u AND ISNULL(BranchIds, N'') = @b AND Status = N'Done' AND AsOf >= @f AND AsOf < @t
ORDER BY AsOf ASC, CreatedAt ASC", c => { Add(c, "@u", userId); Add(c, "@b", branchIds ?? ""); Add(c, "@f", weekStart.Date); Add(c, "@t", weekStart.Date.AddDays(7)); },
                r => row = ReadRun(r));
            return row;
        }

        private static AiCashflowRunRow ReadRun(DbDataReader r) => new AiCashflowRunRow
        {
            Id = r.GetInt64(0), User_Id = r.GetInt64(1), BranchIds = S(r, 2), AsOf = r.GetDateTime(3), Scenario = S(r, 4), Source = S(r, 5),
            FactsJson = S(r, 6), Narrative = S(r, 7), Language = S(r, 8), Status = S(r, 9), CreatedAt = r.IsDBNull(10) ? (DateTime?)null : r.GetDateTime(10),
        };

        // ───────────── التعديلات اليدوية ─────────────

        public long InsertAdjustment(AiCashflowAdjustmentRow a)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_CashflowAdjustments (Id, User_Id, Branch_Id, WeekStart, Direction, Amount, Description, IsActive, CreatedBy, CreatedAt)
VALUES ({id}, {a.User_Id}, {a.Branch_Id}, {a.WeekStart.Date}, {Cut(a.Direction, 5)}, {AiSqlParams.Dec(a.Amount, 18, 3)}, {Cut(a.Description, 300)}, 1, {a.User_Id}, {DateTime.Now})");
            a.Id = id;
            return id;
        }

        /// <summary>إلغاء (IsActive = 0) لصاحب التعديل أو المدير.</summary>
        public bool CancelAdjustment(long id, long userId, bool isAdmin) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_CashflowAdjustments SET IsActive = 0, UpdatedBy = {userId}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND IsActive = 1 AND ({(isAdmin ? 1 : 0)} = 1 OR User_Id = {userId})") == 1;

        /// <summary>التعديلات النشطة داخل الأفق لفروع (null = الكل؛ الفرع 0 = عام).</summary>
        public List<AiCashflowAdjustmentRow> ActiveAdjustments(DateTime from, DateTime to, IList<long> branchIds)
        {
            var list = new List<AiCashflowAdjustmentRow>();
            Exec(@"SELECT a.Id, a.User_Id, a.Branch_Id, a.WeekStart, a.Direction, a.Amount, a.Description, a.IsActive, ISNULL(u.USER_NAME, N'')
FROM AI_CashflowAdjustments a LEFT JOIN USER_DETAILES u ON u.Id = a.User_Id
WHERE a.IsActive = 1 AND a.WeekStart >= @f AND a.WeekStart < @t ORDER BY a.WeekStart, a.CreatedAt",
                c => { Add(c, "@f", from.Date); Add(c, "@t", to.Date); },
                r =>
                {
                    var row = new AiCashflowAdjustmentRow
                    {
                        Id = r.GetInt64(0), User_Id = r.GetInt64(1), Branch_Id = r.IsDBNull(2) ? 0 : r.GetInt64(2), WeekStart = r.GetDateTime(3),
                        Direction = S(r, 4), Amount = Convert.ToDecimal(r.GetValue(5), CultureInfo.InvariantCulture), Description = S(r, 6), IsActive = true, UserName = S(r, 8),
                    };
                    if (branchIds == null || row.Branch_Id == 0 || branchIds.Contains(row.Branch_Id)) list.Add(row);
                });
            return list;
        }

        // ───────────── الاستبعادات ─────────────

        public HashSet<long> Exclusions()
        {
            var set = new HashSet<long>();
            Exec("SELECT SubAccount_Id FROM AI_CashflowExclusions", c => { }, r => set.Add(Convert.ToInt64(r.GetValue(0), CultureInfo.InvariantCulture)));
            return set;
        }

        public void SetExcluded(long subAccountId, bool excluded, long by)
        {
            if (excluded)
            {
                long id = CallSnowflakeIdGenerate.GenerateNewId();
                _newOpp().db.Database.ExecuteSqlInterpolated($@"
IF NOT EXISTS (SELECT 1 FROM AI_CashflowExclusions WHERE SubAccount_Id = {subAccountId})
INSERT INTO AI_CashflowExclusions (Id, SubAccount_Id, CreatedBy, CreatedAt) VALUES ({id}, {subAccountId}, {by}, {DateTime.Now})");
            }
            else
                _newOpp().db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_CashflowExclusions WHERE SubAccount_Id = {subAccountId}");
        }

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
