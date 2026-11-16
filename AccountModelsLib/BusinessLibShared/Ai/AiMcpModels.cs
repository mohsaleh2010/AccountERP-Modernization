using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>P8.10 (ai-assistant-p810-plan.md): ثوابت خادم MCP.</summary>
    public static class AiMcpConst
    {
        public const string Path = "/mcp";
        public const string TokenPrefix = "erp_mcp_";
        public const string ScopeRead = "read";
        public const string ScopeSql = "sql";
        public const int MaxActivePerUser = 5;
        public const int DefaultDays = 90;
        public const string ProtocolVersion = "2025-06-18";
        public const string ServerName = "accounterp-readonly";

        /// <summary>م6: قائمة السماح الثابتة (أدوات القراءة فقط). run_readonly_query يحتاج scope sql.</summary>
        public static readonly string[] AllowList =
        {
            "search_schema", "find_entity", "get_periods_and_branches", "get_trial_balance", "get_financial_statement",
            "get_collections_priority", "get_cashflow_forecast", "get_reorder_suggestions", "run_readonly_query",
        };
    }

    public class AiMcpSettings
    {
        public bool Enabled { get; set; }
        public bool AllowSqlScope { get; set; }
        public int TokenMaxDays { get; set; } = 365;

        public static AiMcpSettings From(IDictionary<string, string> s)
        {
            var r = new AiMcpSettings();
            if (s == null) return r;
            string v;
            r.Enabled = s.TryGetValue(AiSettingKeys.McpEnabled, out v) && v == "1";
            r.AllowSqlScope = s.TryGetValue(AiSettingKeys.McpAllowSqlScope, out v) && v == "1";
            int d;
            if (s.TryGetValue(AiSettingKeys.McpTokenMaxDays, out v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out d)) r.TokenMaxDays = Math.Max(1, Math.Min(3650, d));
            return r;
        }
    }

    public class AiApiTokenRow
    {
        public long Id { get; set; }
        public long User_Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string Scopes { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string LastUsedIp { get; set; }
        public long UseCount { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.Now;
        public bool HasScope(string s) => (Scopes ?? "").Split(',').Any(x => string.Equals(x.Trim(), s, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>ناتج الإنشاء: التوكن الكامل يُعرض مرة واحدة فقط (م3).</summary>
    public class AiApiTokenCreated
    {
        public AiApiTokenRow Row { get; set; }
        public string Token { get; set; }
    }

    public interface IAiMcpGateway
    {
        bool HasAssistant(AiUserContext user);
        AiMcpSettings GetSettings(AiUserContext user);
        void SaveSettings(AiMcpSettings s, AiUserContext user);
        AiApiTokenCreated CreateToken(string name, int days, bool sqlScope, AiUserContext user);
        List<AiApiTokenRow> MyTokens(AiUserContext user);
        List<AiApiTokenRow> AllTokens(AiUserContext user);
        bool Revoke(long id, AiUserContext user);
    }

    public class AiMcpException : InvalidOperationException
    {
        public string Code { get; }
        public AiMcpException(string code, string message) : base(message) { Code = code ?? ""; }
    }

    /// <summary>P8.10.1: توليد التوكن وهاشه (pure).</summary>
    public static class AiMcpTokenText
    {
        public static string NewToken()
        {
            var b = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
            return AiMcpConst.TokenPrefix + Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static string Hash(string token)
        {
            using (var sha = SHA256.Create())
            {
                var h = sha.ComputeHash(Encoding.UTF8.GetBytes(token ?? ""));
                var sb = new StringBuilder(64);
                foreach (var x in h) sb.Append(x.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>البادئة المعروضة (أول 12 حرفاً) للتمييز بلا كشف.</summary>
        public static string Prefix(string token) => string.IsNullOrEmpty(token) ? "" : token.Substring(0, Math.Min(12, token.Length));

        public static bool LooksValid(string token) =>
            !string.IsNullOrEmpty(token) && token.StartsWith(AiMcpConst.TokenPrefix, StringComparison.Ordinal) && token.Length >= 40 && token.Length <= 80;
    }

    /// <summary>P8.10.1: طبقة بيانات AI_ApiTokens.</summary>
    public class AiApiTokensCLS
    {
        private readonly Func<operations> _newOpp;

        public AiApiTokensCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public long Insert(long userId, string name, string hash, string prefix, string scopes, DateTime expiresAt)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_ApiTokens (Id, User_Id, Name, TokenHash, Prefix, Scopes, ExpiresAt, UseCount, CreatedBy, CreatedAt)
VALUES ({id}, {userId}, {Cut(name, 100)}, {hash}, {Cut(prefix, 20)}, {Cut(scopes, 100)}, {expiresAt}, 0, {userId}, {DateTime.Now})");
            return id;
        }

        private const string Cols = "t.Id, t.User_Id, ISNULL(u.USER_NAME, N''), t.Name, t.Prefix, t.Scopes, t.ExpiresAt, t.LastUsedAt, t.LastUsedIp, t.UseCount, t.RevokedAt, t.CreatedAt";

        private static AiApiTokenRow Read(DbDataReader r) => new AiApiTokenRow
        {
            Id = r.GetInt64(0), User_Id = r.GetInt64(1), UserName = S(r, 2), Name = S(r, 3), Prefix = S(r, 4), Scopes = S(r, 5), ExpiresAt = r.GetDateTime(6),
            LastUsedAt = r.IsDBNull(7) ? (DateTime?)null : r.GetDateTime(7), LastUsedIp = S(r, 8), UseCount = r.IsDBNull(9) ? 0 : Convert.ToInt64(r.GetValue(9), CultureInfo.InvariantCulture),
            RevokedAt = r.IsDBNull(10) ? (DateTime?)null : r.GetDateTime(10), CreatedAt = r.IsDBNull(11) ? (DateTime?)null : r.GetDateTime(11),
        };

        public AiApiTokenRow FindByHash(string hash)
        {
            AiApiTokenRow row = null;
            Exec("SELECT " + Cols + " FROM AI_ApiTokens t LEFT JOIN USER_DETAILES u ON u.Id = t.User_Id WHERE t.TokenHash = @h", c => Add(c, "@h", hash), r => row = Read(r));
            return row;
        }

        public List<AiApiTokenRow> ForUser(long userId)
        {
            var list = new List<AiApiTokenRow>();
            Exec("SELECT " + Cols + " FROM AI_ApiTokens t LEFT JOIN USER_DETAILES u ON u.Id = t.User_Id WHERE t.User_Id = @u ORDER BY t.CreatedAt DESC",
                c => Add(c, "@u", userId), r => list.Add(Read(r)));
            return list;
        }

        public List<AiApiTokenRow> All(int top)
        {
            var list = new List<AiApiTokenRow>();
            Exec("SELECT TOP (" + Math.Max(1, top).ToString(CultureInfo.InvariantCulture) + ") " + Cols + " FROM AI_ApiTokens t LEFT JOIN USER_DETAILES u ON u.Id = t.User_Id ORDER BY t.CreatedAt DESC",
                c => { }, r => list.Add(Read(r)));
            return list;
        }

        public int ActiveCount(long userId)
        {
            int n = 0;
            Exec("SELECT COUNT(*) FROM AI_ApiTokens WHERE User_Id = @u AND RevokedAt IS NULL AND ExpiresAt > GETDATE()", c => Add(c, "@u", userId),
                r => n = Convert.ToInt32(r.GetValue(0), CultureInfo.InvariantCulture));
            return n;
        }

        /// <summary>إلغاء لصاحب التوكن أو المدير.</summary>
        public bool Revoke(long id, long by, bool isAdmin) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_ApiTokens SET RevokedAt = {DateTime.Now}, RevokedBy = {by}, UpdatedBy = {by}, UpdatedAt = {DateTime.Now}
WHERE Id = {id} AND RevokedAt IS NULL AND ({(isAdmin ? 1 : 0)} = 1 OR User_Id = {by})") == 1;

        public void Touch(long id, string ip) =>
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_ApiTokens SET LastUsedAt = {DateTime.Now}, LastUsedIp = {Cut(ip, 64)}, UseCount = UseCount + 1 WHERE Id = {id}");

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
