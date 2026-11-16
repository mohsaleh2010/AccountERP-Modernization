using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AccountModelsLib
{
    /// <summary>
    /// P8.12.1 (ai-assistant-p812-plan.md §3): طبقة بيانات AI_Bank*. حركات الدفاتر تُقرأ بـ SELECT فقط من Money_Transaction_Box (سندات الخزينة
    /// على البنك، بنفس شرط الإذونات الحقيقية Order_No ≠ 0/-1). فحص 1: Money_Transaction_Bank وChecks_Memory غير مستعملين (افتراض موثّق).
    /// </summary>
    public class AiBankRecCLS
    {
        private readonly Func<operations> _newOpp;

        public AiBankRecCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        // ───────────── البنوك ─────────────

        public List<(long Id, string Name, long BranchId)> Banks()
        {
            var list = new List<(long, string, long)>();
            Exec("SELECT id, ISNULL(Bank_Name, N''), ISNULL(Branch_Id, 0) FROM Banks WHERE ISNULL(Is_Bank, 0) = 1 AND ISNULL(Active, 1) = 1 ORDER BY Bank_Name",
                c => { }, r => list.Add((L(r, 0), S(r, 1), L(r, 2))));
            return list;
        }

        // ───────────── الكشوف ─────────────

        public long InsertStatement(AiBankStatementRow st, IList<AiBankLine> lines, long by)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            var db = _newOpp().db;
            db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_BankStatements (Id, Bank_Id, PeriodFrom, PeriodTo, OpeningBalance, ClosingBalance, Source, FileName, Attachment_Id, Status, LineCount, BalanceDiff, CreatedBy, CreatedAt)
VALUES ({id}, {st.Bank_Id}, {st.PeriodFrom}, {st.PeriodTo}, {AiSqlParams.Dec(st.OpeningBalance, 18, 3)}, {AiSqlParams.Dec(st.ClosingBalance, 18, 3)}, {Cut(st.Source, 20)},
        {Cut(st.FileName, 260)}, {(long?)null}, {Cut(st.Status ?? "Imported", 20)}, {lines.Count}, {AiSqlParams.Dec(st.BalanceDiff, 18, 3)}, {by}, {DateTime.Now})");
            foreach (var l in lines)
            {
                long lid = CallSnowflakeIdGenerate.GenerateNewId();
                db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_BankStatementLines (Id, Statement_Id, LineNo, LineDate, ValueDate, Description, Reference, Debit, Credit, Balance, Confidence, CreatedBy, CreatedAt)
VALUES ({lid}, {id}, {l.LineNo}, {l.Date}, {l.ValueDate}, {Cut(l.Description, 500)}, {Cut(l.Reference, 100)}, {AiSqlParams.Dec(l.Debit, 18, 3)}, {AiSqlParams.Dec(l.Credit, 18, 3)},
        {AiSqlParams.Dec(l.Balance, 18, 3)}, {AiSqlParams.Dec(l.Confidence, 9, 4)}, {by}, {DateTime.Now})");
            }
            st.Id = id;
            return id;
        }

        private const string StCols = "s.Id, s.Bank_Id, ISNULL(b.Bank_Name, N''), s.PeriodFrom, s.PeriodTo, s.OpeningBalance, s.ClosingBalance, s.Source, s.FileName, s.Status, ISNULL(s.LineCount, 0), s.BalanceDiff, s.CreatedAt";

        private static AiBankStatementRow ReadSt(DbDataReader r) => new AiBankStatementRow
        {
            Id = L(r, 0), Bank_Id = L(r, 1), BankName = S(r, 2), PeriodFrom = Dt(r, 3), PeriodTo = Dt(r, 4), OpeningBalance = Mn(r, 5), ClosingBalance = Mn(r, 6),
            Source = S(r, 7), FileName = S(r, 8), Status = S(r, 9), LineCount = Convert.ToInt32(r.GetValue(10), CultureInfo.InvariantCulture), BalanceDiff = Mn(r, 11), CreatedAt = Dt(r, 12),
        };

        public List<AiBankStatementRow> Statements(long bankId)
        {
            var list = new List<AiBankStatementRow>();
            Exec("SELECT TOP (100) " + StCols + " FROM AI_BankStatements s LEFT JOIN Banks b ON b.id = s.Bank_Id WHERE s.Bank_Id = @b ORDER BY s.CreatedAt DESC",
                c => Add(c, "@b", bankId), r => list.Add(ReadSt(r)));
            return list;
        }

        public AiBankStatementRow Statement(long id)
        {
            AiBankStatementRow row = null;
            Exec("SELECT " + StCols + " FROM AI_BankStatements s LEFT JOIN Banks b ON b.id = s.Bank_Id WHERE s.Id = @i", c => Add(c, "@i", id), r => row = ReadSt(r));
            return row;
        }

        public List<AiBankLine> Lines(long statementId)
        {
            var list = new List<AiBankLine>();
            Exec(@"SELECT Id, LineNo, LineDate, ValueDate, Description, Reference, Debit, Credit, Balance, Confidence FROM AI_BankStatementLines
WHERE Statement_Id = @s ORDER BY LineNo", c => Add(c, "@s", statementId), r => list.Add(new AiBankLine
            {
                Id = L(r, 0), LineNo = Convert.ToInt32(r.GetValue(1), CultureInfo.InvariantCulture), Date = Dt(r, 2), ValueDate = Dt(r, 3), Description = S(r, 4),
                Reference = S(r, 5), Debit = Mn(r, 6) ?? 0, Credit = Mn(r, 7) ?? 0, Balance = Mn(r, 8), Confidence = Mn(r, 9),
            }));
            return list;
        }

        // ───────────── حركات الدفاتر ─────────────

        /// <summary>سطور سندات الخزينة على البنك فى [from، to]. Signed موجب = قبض.</summary>
        public List<AiBankBookMove> BookMoves(long bankId, DateTime from, DateTime to)
        {
            var list = new List<AiBankBookMove>();
            Exec(@"SELECT m.Id, o.Id, ISNULL(o.Order_No, 0), CONVERT(date, m.Transaction_Date),
       CASE WHEN m.Transaction_Type = 1 THEN 1 ELSE -1 END * ISNULL(m.Transaction_Value, 0),
       COALESCE(cu.Name, su.Name, m.Account_Name), COALESCE(NULLIF(CONVERT(nvarchar(400), m.Notes), N''), CONVERT(nvarchar(400), o.Order_Notes))
FROM Money_Transaction_Box m
INNER JOIN Order_Money o ON o.Id = m.Order_Mony_Id
LEFT JOIN Customer cu ON cu.Id = m.Customer_Id AND m.Customer_Id > 0
LEFT JOIN Supplier su ON su.Id = m.Supplier_Id AND m.Supplier_Id > 0
WHERE m.Bank_Id = @b AND o.Order_No <> 0 AND o.Order_No <> -1 AND m.Transaction_Date >= @f AND m.Transaction_Date < @t
ORDER BY m.Transaction_Date, m.Id", c => { Add(c, "@b", bankId); Add(c, "@f", from.Date); Add(c, "@t", to.Date.AddDays(1)); },
                r => list.Add(new AiBankBookMove
                {
                    LineId = L(r, 0), VoucherId = L(r, 1), VoucherNo = L(r, 2), Date = r.GetDateTime(3), Signed = Mn(r, 4) ?? 0, Party = S(r, 5), Notes = S(r, 6),
                }), 120);
            return list;
        }

        /// <summary>ب10: حركات دفاتر مؤكدة فى أى كشف لنفس البنك.</summary>
        public HashSet<long> ConfirmedRefs(long bankId)
        {
            var set = new HashSet<long>();
            Exec(@"SELECT m.SystemRefsJson FROM AI_BankMatches m INNER JOIN AI_BankStatements s ON s.Id = m.Statement_Id
WHERE s.Bank_Id = @b AND m.Status = N'confirmed'", c => Add(c, "@b", bankId),
                r => { foreach (var x in JsonConvert.DeserializeObject<List<long>>(S(r, 0) ?? "[]") ?? new List<long>()) set.Add(x); });
            return set;
        }

        // ───────────── المطابقات ─────────────

        public List<AiBankMatchRow> Matches(long statementId)
        {
            var list = new List<AiBankMatchRow>();
            Exec("SELECT Id, Statement_Id, Kind, LinesJson, SystemRefsJson, Score, Status, Reason FROM AI_BankMatches WHERE Statement_Id = @s AND Status <> N'rejected' ORDER BY CreatedAt",
                c => Add(c, "@s", statementId), r => list.Add(new AiBankMatchRow
                {
                    Id = L(r, 0), Statement_Id = L(r, 1), Kind = S(r, 2), Lines = JsonConvert.DeserializeObject<List<int>>(S(r, 3) ?? "[]") ?? new List<int>(),
                    SystemRefs = JsonConvert.DeserializeObject<List<long>>(S(r, 4) ?? "[]") ?? new List<long>(), Score = Mn(r, 5) ?? 0, Status = S(r, 6), Reason = S(r, 7),
                }));
            return list;
        }

        public long InsertMatch(long statementId, AiBankMatchRow m, long by)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_BankMatches (Id, Statement_Id, Kind, LinesJson, SystemRefsJson, Score, Status, Reason, CreatedBy, CreatedAt)
VALUES ({id}, {statementId}, {Cut(m.Kind, 20)}, {JsonConvert.SerializeObject(m.Lines)}, {JsonConvert.SerializeObject(m.SystemRefs)}, {AiSqlParams.Dec(m.Score, 9, 4)},
        {Cut(m.Status ?? AiBankConst.StatusSuggested, 20)}, {Cut(m.Reason, 300)}, {by}, {DateTime.Now})");
            m.Id = id;
            return id;
        }

        /// <summary>يحذف الاقتراحات (غير المؤكدة) قبل إعادة المطابقة — صفوف جدولنا فقط.</summary>
        public int ClearSuggested(long statementId) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_BankMatches WHERE Statement_Id = {statementId} AND Status = N'suggested'");

        public bool SetMatchStatus(long id, string status, long by) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_BankMatches SET Status = {status}, DecidedBy = {by}, DecidedAt = {DateTime.Now}, UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND Status = N'suggested'") == 1;

        public long MatchStatement(long matchId)
        {
            long s = 0;
            Exec("SELECT Statement_Id FROM AI_BankMatches WHERE Id = @i", c => Add(c, "@i", matchId), r => s = L(r, 0));
            return s;
        }

        public void SetStatementStatus(long id, string status) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($"UPDATE AI_BankStatements SET Status = {Cut(status, 20)}, UpdatedAt = {DateTime.Now} WHERE Id = {id}");

        // ───────────── تعيين الأعمدة ─────────────

        public AiBankColumnMap Profile(long bankId)
        {
            AiBankColumnMap map = null;
            Exec("SELECT ColumnsJson FROM AI_BankImportProfiles WHERE Bank_Id = @b", c => Add(c, "@b", bankId),
                r => map = JsonConvert.DeserializeObject<AiBankColumnMap>(S(r, 0) ?? "{}"));
            return map;
        }

        public void SaveProfile(long bankId, AiBankColumnMap map, long by)
        {
            string json = JsonConvert.SerializeObject(map);
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
IF EXISTS (SELECT 1 FROM AI_BankImportProfiles WHERE Bank_Id = {bankId})
    UPDATE AI_BankImportProfiles SET ColumnsJson = {json}, UpdatedBy = {by}, UpdatedAt = {DateTime.Now} WHERE Bank_Id = {bankId}
ELSE
    INSERT INTO AI_BankImportProfiles (Id, Bank_Id, ColumnsJson, CreatedBy, CreatedAt) VALUES ({id}, {bankId}, {json}, {by}, {DateTime.Now})");
        }

        // ───────────── أدوات ─────────────

        private static long L(DbDataReader r, int i) => r.IsDBNull(i) ? 0 : Convert.ToInt64(r.GetValue(i), CultureInfo.InvariantCulture);
        private static decimal? Mn(DbDataReader r, int i) => r.IsDBNull(i) ? (decimal?)null : Convert.ToDecimal(r.GetValue(i), CultureInfo.InvariantCulture);
        private static DateTime? Dt(DbDataReader r, int i) => r.IsDBNull(i) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(i), CultureInfo.InvariantCulture);
        private static string S(DbDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture);
        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

        private static void Add(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private void Exec(string sql, Action<DbCommand> bind, Action<DbDataReader> row, int timeout = 30)
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
                    cmd.CommandTimeout = timeout;
                    bind(cmd);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) row(r);
                }
            }
            finally { if (wasClosed) conn.Close(); }
        }
    }
}
