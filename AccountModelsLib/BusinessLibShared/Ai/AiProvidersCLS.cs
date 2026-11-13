using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// طبقة بيانات المساعد الذكى (P0): AI_Providers / AI_Models / AI_TaskRouting / AI_Settings. تخزّن ما يصلها كما هو؛
    /// تشفير المفاتيح مسؤولية AccountERP.AI (لا نص صريح يصل هنا). الحذف هنا لصفوف هذه الجداول الجديدة فقط التى أنشأها المدير.
    /// </summary>
    public class AiProvidersCLS
    {
        private readonly Func<operations> _newOpp;

        public AiProvidersCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        // ───────────── المزوّدون ─────────────

        public List<AiProviderRow> GetProviders() => Read(
            @"SELECT Id, ProviderType, DisplayName, BaseUrl, ApiKeyEnc, ApiKeyHint, ExtraJson, ApiProtocol, AuthHeaderName, ExtraHeadersEnc,
                     TimeoutSeconds, IsCustom, IsEnabled, IsLocal, LastTestAt, LastTestOk, LastTestMessage, AuthMethod, SignInAt, SignInBy
              FROM AI_Providers ORDER BY IsCustom, DisplayName", r => new AiProviderRow
            {
                Id = r.GetInt64(0),
                ProviderType = Str(r, 1),
                DisplayName = Str(r, 2),
                BaseUrl = NStr(r, 3),
                ApiKeyEnc = NStr(r, 4),
                ApiKeyHint = NStr(r, 5),
                ExtraJson = NStr(r, 6),
                ApiProtocol = NStr(r, 7),
                AuthHeaderName = NStr(r, 8),
                ExtraHeadersEnc = NStr(r, 9),
                TimeoutSeconds = r.IsDBNull(10) ? 60 : r.GetInt32(10),
                IsCustom = !r.IsDBNull(11) && r.GetBoolean(11),
                IsEnabled = !r.IsDBNull(12) && r.GetBoolean(12),
                IsLocal = !r.IsDBNull(13) && r.GetBoolean(13),
                LastTestAt = r.IsDBNull(14) ? (DateTime?)null : r.GetDateTime(14),
                LastTestOk = r.IsDBNull(15) ? (bool?)null : r.GetBoolean(15),
                LastTestMessage = NStr(r, 16),
                AuthMethod = NStr(r, 17),
                SignInAt = r.IsDBNull(18) ? (DateTime?)null : r.GetDateTime(18),
                SignInBy = r.IsDBNull(19) ? (long?)null : r.GetInt64(19),
            });

        public AiProviderRow GetProvider(long id) => GetProviders().FirstOrDefault(p => p.Id == id);

        /// <summary>إدراج (Id = 0) أو تعديل. يعيد المعرّف.</summary>
        public long SaveProvider(AiProviderRow p, long userId)
        {
            var db = _newOpp().db;
            if (p.Id == 0)
            {
                p.Id = CallSnowflakeIdGenerate.GenerateNewId();
                db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Providers (Id, ProviderType, DisplayName, BaseUrl, ApiKeyEnc, ApiKeyHint, ExtraJson, ApiProtocol, AuthHeaderName, ExtraHeadersEnc,
                          TimeoutSeconds, IsCustom, IsEnabled, IsLocal, AuthMethod, CreatedBy, CreatedAt)
VALUES ({p.Id}, {p.ProviderType}, {p.DisplayName}, {p.BaseUrl}, {p.ApiKeyEnc}, {p.ApiKeyHint}, {p.ExtraJson}, {p.ApiProtocol}, {p.AuthHeaderName}, {p.ExtraHeadersEnc},
        {p.TimeoutSeconds}, {p.IsCustom}, {p.IsEnabled}, {p.IsLocal}, {p.AuthMethod}, {userId}, GETDATE())");
            }
            else
            {
                db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Providers SET DisplayName = {p.DisplayName}, BaseUrl = {p.BaseUrl}, ApiKeyEnc = {p.ApiKeyEnc}, ApiKeyHint = {p.ApiKeyHint},
       ExtraJson = {p.ExtraJson}, ApiProtocol = {p.ApiProtocol}, AuthHeaderName = {p.AuthHeaderName}, ExtraHeadersEnc = {p.ExtraHeadersEnc},
       TimeoutSeconds = {p.TimeoutSeconds}, IsEnabled = {p.IsEnabled}, IsLocal = {p.IsLocal}, AuthMethod = {p.AuthMethod}, UpdatedBy = {userId}, UpdatedAt = GETDATE()
WHERE Id = {p.Id}");
            }
            return p.Id;
        }

        /// <summary>P0.1: حفظ المفتاح الناتج عن تسجيل الدخول لدى المزوّد (مشفّراً بالفعل).</summary>
        public void SaveSignInKey(long providerId, string keyEnc, string keyHint, long userId)
        {
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Providers SET ApiKeyEnc = {keyEnc}, ApiKeyHint = {keyHint}, AuthMethod = {AiAuthMethods.SignIn}, SignInAt = GETDATE(), SignInBy = {userId},
       LastTestAt = NULL, LastTestOk = NULL, LastTestMessage = NULL, UpdatedBy = {userId}, UpdatedAt = GETDATE()
WHERE Id = {providerId}");
        }

        /// <summary>P0.1: إلغاء الربط — يمسح المفتاح المحلى فقط.</summary>
        public void ClearKey(long providerId, long userId)
        {
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Providers SET ApiKeyEnc = NULL, ApiKeyHint = NULL, AuthMethod = NULL, SignInAt = NULL, SignInBy = NULL,
       LastTestAt = NULL, LastTestOk = NULL, LastTestMessage = NULL, UpdatedBy = {userId}, UpdatedAt = GETDATE()
WHERE Id = {providerId}");
        }

        /// <summary>
        /// هل يملك المستخدم صلاحية عنصر القائمة (نفس قاعدة MenuRepository: صف صلاحية PERMATION = "1" والعنصر وصفه ظاهران)؟
        /// تُستخدم حيث لا توجد دائرة Blazor (نقطة الرجوع من المزوّد) ولتأكيد الصلاحية على الخادم.
        /// </summary>
        public bool UserHasMenu(long userId, string menuKey)
        {
            if (userId <= 0) return false;
            var rows = Read(@"SELECT m.KeysData, p.PERMATION FROM SysConfigMenusPERMATIONS p INNER JOIN SysConfigMenus m ON p.Menu_ID = m.Id
                              WHERE p.USER_ID = " + userId + @" AND ISNULL(m.IsShowToUser, 1) = 1 AND ISNULL(p.IsShowToUser, 1) = 1",
                r => (Key: NStr(r, 0), Perm: NStr(r, 1)));
            foreach (var (key, perm) in rows)
            {
                if (key == null || perm == null) continue;
                if (Common.DecryptMe(key).Trim() == menuKey && Common.DecryptMe(perm).Trim() == "1") return true;
            }
            return false;
        }

        public void SaveTestResult(long providerId, bool ok, string message)
        {
            if (message != null && message.Length > 1000) message = message.Substring(0, 1000);
            _newOpp().db.Database.ExecuteSqlInterpolated(
                $"UPDATE AI_Providers SET LastTestAt = GETDATE(), LastTestOk = {ok}, LastTestMessage = {message} WHERE Id = {providerId}");
        }

        /// <summary>يحذف المزوّد ونماذجه وربط المهام عليها فى معاملة واحدة.</summary>
        public void DeleteProvider(long providerId)
        {
            var db = _newOpp().db;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_TaskRouting WHERE Model_Id IN (SELECT Id FROM AI_Models WHERE Provider_Id = {providerId})");
            db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_Models WHERE Provider_Id = {providerId}");
            db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_Providers WHERE Id = {providerId}");
            tx.Commit();
        }

        // ───────────── النماذج ─────────────

        public List<AiModelRow> GetModels() => Read(
            @"SELECT m.Id, m.Provider_Id, m.ModelId, m.DisplayName, m.SupportsTools, m.SupportsVision, m.SupportsTemperature, m.SupportsEmbeddings,
                     m.ContextTokens, m.InputPricePerM, m.OutputPricePerM, m.IsEnabled, p.DisplayName
              FROM AI_Models m LEFT JOIN AI_Providers p ON p.Id = m.Provider_Id
              ORDER BY p.DisplayName, m.ModelId", r => new AiModelRow
            {
                Id = r.GetInt64(0),
                Provider_Id = r.GetInt64(1),
                ModelId = Str(r, 2),
                DisplayName = Str(r, 3),
                SupportsTools = !r.IsDBNull(4) && r.GetBoolean(4),
                SupportsVision = !r.IsDBNull(5) && r.GetBoolean(5),
                SupportsTemperature = !r.IsDBNull(6) && r.GetBoolean(6),
                SupportsEmbeddings = !r.IsDBNull(7) && r.GetBoolean(7),
                ContextTokens = r.IsDBNull(8) ? (int?)null : r.GetInt32(8),
                InputPricePerM = r.IsDBNull(9) ? (decimal?)null : r.GetDecimal(9),
                OutputPricePerM = r.IsDBNull(10) ? (decimal?)null : r.GetDecimal(10),
                IsEnabled = !r.IsDBNull(11) && r.GetBoolean(11),
                ProviderName = Str(r, 12),
            });

        public long SaveModel(AiModelRow m, long userId)
        {
            var db = _newOpp().db;
            if (m.Id == 0)
            {
                int dup = db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM AI_Models WHERE Provider_Id = {m.Provider_Id} AND ModelId = {m.ModelId}").AsEnumerable().First();
                if (dup > 0) throw new InvalidOperationException($"النموذج {m.ModelId} مضاف بالفعل لهذا المزوّد.");
                m.Id = CallSnowflakeIdGenerate.GenerateNewId();
                db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Models (Id, Provider_Id, ModelId, DisplayName, SupportsTools, SupportsVision, SupportsTemperature, SupportsEmbeddings,
                       ContextTokens, InputPricePerM, OutputPricePerM, IsEnabled, CreatedBy, CreatedAt)
VALUES ({m.Id}, {m.Provider_Id}, {m.ModelId}, {m.DisplayName}, {m.SupportsTools}, {m.SupportsVision}, {m.SupportsTemperature}, {m.SupportsEmbeddings},
        {m.ContextTokens}, {m.InputPricePerM}, {m.OutputPricePerM}, {m.IsEnabled}, {userId}, GETDATE())");
            }
            else
            {
                db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Models SET DisplayName = {m.DisplayName}, SupportsTools = {m.SupportsTools}, SupportsVision = {m.SupportsVision},
       SupportsTemperature = {m.SupportsTemperature}, SupportsEmbeddings = {m.SupportsEmbeddings}, ContextTokens = {m.ContextTokens},
       InputPricePerM = {m.InputPricePerM}, OutputPricePerM = {m.OutputPricePerM}, IsEnabled = {m.IsEnabled}, UpdatedBy = {userId}, UpdatedAt = GETDATE()
WHERE Id = {m.Id}");
            }
            return m.Id;
        }

        public void DeleteModel(long modelId)
        {
            var db = _newOpp().db;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_TaskRouting WHERE Model_Id = {modelId}");
            db.Database.ExecuteSqlInterpolated($"DELETE FROM AI_Models WHERE Id = {modelId}");
            tx.Commit();
        }

        // ───────────── ربط المهام ─────────────

        public List<AiTaskRoutingRow> GetRouting() => Read(
            "SELECT Id, TaskKey, Model_Id, Priority, Effort, MaxTokens, Temperature FROM AI_TaskRouting ORDER BY TaskKey, Priority", r => new AiTaskRoutingRow
            {
                Id = r.GetInt64(0),
                TaskKey = Str(r, 1),
                Model_Id = r.GetInt64(2),
                Priority = r.GetInt32(3),
                Effort = Str(r, 4),
                MaxTokens = r.IsDBNull(5) ? (int?)null : r.GetInt32(5),
                Temperature = r.IsDBNull(6) ? (decimal?)null : r.GetDecimal(6),
            });

        /// <summary>يستبدل ربط المهام بالكامل (جدول إعداد جديد لا يحوى إلا ما أدخله المدير) فى معاملة واحدة.</summary>
        public void ReplaceRouting(List<AiTaskRoutingRow> rows, long userId)
        {
            var db = _newOpp().db;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlRaw("DELETE FROM AI_TaskRouting");
            foreach (var r in rows)
            {
                long id = CallSnowflakeIdGenerate.GenerateNewId();
                string effort = string.IsNullOrWhiteSpace(r.Effort) ? null : r.Effort;
                db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_TaskRouting (Id, TaskKey, Model_Id, Priority, Effort, MaxTokens, Temperature, CreatedBy, CreatedAt)
VALUES ({id}, {r.TaskKey}, {r.Model_Id}, {r.Priority}, {effort}, {r.MaxTokens}, {r.Temperature}, {userId}, GETDATE())");
            }
            tx.Commit();
        }

        // ───────────── الإعدادات ─────────────

        public Dictionary<string, string> GetSettings() =>
            Read("SELECT SettingKey, SettingValue FROM AI_Settings", r => (Key: Str(r, 0), Value: Str(r, 1)))
                .GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

        // ───────────── أدوات ─────────────

        private List<T> Read<T>(string sql, Func<DbDataReader, T> map)
        {
            var db = _newOpp().db;
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var list = new List<T>();
                using var r = cmd.ExecuteReader();
                while (r.Read()) list.Add(map(r));
                return list;
            }
            finally { if (wasClosed) conn.Close(); }
        }

        private static string Str(DbDataReader r, int i) => r.IsDBNull(i) ? "" : r.GetString(i);
        private static string NStr(DbDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);
    }
}
