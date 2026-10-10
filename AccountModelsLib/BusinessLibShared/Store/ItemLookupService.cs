using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// القوائم المرجعية للأصناف (خطة 04 — المرحلة 1): صفحة واحدة لكل الجداول البسيطة (اسم + نشط) بدل ~11 شاشة ويندوز.
    /// كل جدول معرَّف فى سجل ثابت (اسمه وأعمدته من الكود فقط، لا من المستخدم)، والقيم كلها معاملات.
    /// قواعد (حسب الأنظمة الحديثة): لا يُكرر الاسم؛ إعادة التسمية تحدّث نسخة الاسم المخزَّنة فى Item؛ الحذف للقيمة غير المستعملة فقط،
    /// والمستعملة تُوقف (للجداول التى بها عمود نشط).
    /// </summary>
    public class ItemLookupService
    {
        private readonly Func<operations> _newOpp;

        public ItemLookupService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        /// <summary>تعريف جدول مرجعى.</summary>
        public record LookupDef(string Kind, string Table, string IdCol, string NameCol, string ActiveCol, bool Identity, bool Audit,
                                string ItemIdCol, string ItemNameCol);

        public static readonly LookupDef[] Lookups =
        {
            new("types",        "Items_Types_Name",                "Id", "ItemsType_Name",   "IsActive", false, true,  "Items_Types_Name_Id",                "Items_Types_Name"),
            new("bodies",       "Items_Bodies_Name",               "Id", "ItemBody_Name",    null,       false, true,  null,                                 null),
            new("colors",       "Items_TypesColor_Name",           "Id", "Type_Name",        null,       false, true,  "Items_TypesColor_NameId",            "Items_TypesColor_Name"),
            new("descriptions", "Items_TypesDescription_Name",     "Id", "Type_Name",        null,       false, true,  "Items_TypesDescription_NameId",      "Items_TypesDescription_Name"),
            new("other-names",  "Items_TypesDescriptionNameOther", "Id", "Type_Name",        null,       false, true,  "Items_TypesDescriptionNameOtherId",  "Items_TypesDescriptionNameOther"),
            new("factories",    "Items_TypesFactory_Name",         "Id", "Type_Name",        null,       false, true,  "Items_TypesFactory_NameId",          "Items_TypesFactory_Name"),
            new("sizes",        "Items_TypesSize_Name",            "Id", "Type_Name",        null,       false, true,  "Items_TypesSize_NameId",             "Items_TypesSize_Name"),
            new("degrees",      "Items_TypesDegreeFarz_Name",      "Id", "Type_Name",        null,       false, true,  "Items_TypesDegreeFarz_NameId",       "Items_TypesDegreeFarz_Name"),
            new("natures",      "Items_Nature",                    "Id", "ItemsNature_Name", "IsActive", true,  true,  "Items_NaturesId",                    null),
            new("groups",       "Items_Group",                     "Id", "Name",             null,       true,  false, "Items_GroupsId",                     null),
        };

        public static LookupDef Get(string kind) =>
            Lookups.FirstOrDefault(l => string.Equals(l.Kind, kind, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("قائمة غير معروفة.");

        public class LookupRow
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public bool Is_Active { get; set; }
            public int Used_Count { get; set; }
        }

        public List<LookupRow> List(string kind, string text)
        {
            var d = Get(kind);
            string t = (text ?? "").Trim();
            string active = d.ActiveCol == null ? "CAST(1 AS bit)" : $"CAST(CASE WHEN ISNULL(l.[{d.ActiveCol}], 1) = 1 THEN 1 ELSE 0 END AS bit)";
            string used = d.ItemIdCol == null ? "0" : $"(SELECT COUNT(*) FROM Item i WHERE i.[{d.ItemIdCol}] = l.[{d.IdCol}])";
            string sql = $@"SELECT CAST(l.[{d.IdCol}] AS bigint) AS Id, ISNULL(l.[{d.NameCol}], N'') AS Name, {active} AS Is_Active, {used} AS Used_Count
FROM [{d.Table}] l WHERE ({{0}} = N'' OR l.[{d.NameCol}] LIKE {{1}}) ORDER BY l.[{d.NameCol}]";
            return Run(opp => opp.db.Database.SqlQuery<LookupRow>(FormattableStringFactory.Create(sql, t, "%" + t + "%")).ToList());
        }

        /// <summary>إضافة قيمة جديدة (يُرفض الاسم المكرر). يرجع معرّفها.</summary>
        public long Add(string kind, string name)
        {
            var d = Get(kind);
            string n = Clean(name);
            return Run(opp =>
            {
                EnsureUnique(opp, d, n, 0);
                DateTime now = DateTime.Now;
                if (d.Identity)
                {
                    string cols = d.Audit ? $"[{d.NameCol}], OperationDate, User_Id, Branch_Id, Insert_ServerDateTime" : $"[{d.NameCol}]";
                    string vals = d.Audit ? "{0}, {1}, {2}, {3}, {4}" : "{0}";
                    string sql = $"INSERT INTO [{d.Table}] ({cols}{(d.ActiveCol != null ? $", [{d.ActiveCol}]" : "")}) OUTPUT CAST(INSERTED.[{d.IdCol}] AS bigint) AS Value VALUES ({vals}{(d.ActiveCol != null ? ", 1" : "")})";
                    return opp.db.Database.SqlQuery<long>(FormattableStringFactory.Create(sql, n, now.Date, Common.User_Id, Common.Branch_Id, now)).AsEnumerable().First();
                }
                long id = CallSnowflakeIdGenerate.GenerateNewId();
                string sql2 = $@"INSERT INTO [{d.Table}] ([{d.IdCol}], [{d.NameCol}]{(d.Audit ? ", OperationDate, User_Id, Branch_Id, Insert_ServerDateTime" : "")}{(d.ActiveCol != null ? $", [{d.ActiveCol}]" : "")})
VALUES ({{0}}, {{1}}{(d.Audit ? ", {2}, {3}, {4}, {5}" : "")}{(d.ActiveCol != null ? ", 1" : "")})";
                opp.db.Database.ExecuteSql(FormattableStringFactory.Create(sql2, id, n, now.Date, Common.User_Id, Common.Branch_Id, now));
                return id;
            });
        }

        /// <summary>إعادة تسمية + تحديث نسخة الاسم المخزَّنة فى الأصناف (معاملة واحدة).</summary>
        public void Rename(string kind, long id, string name)
        {
            var d = Get(kind);
            string n = Clean(name);
            RunTx(opp =>
            {
                EnsureUnique(opp, d, n, id);
                DateTime now = DateTime.Now;
                string sql = d.Audit
                    ? $"UPDATE [{d.Table}] SET [{d.NameCol}] = {{0}}, Update_ServerDateTime = {{1}}, EditOperationDate = {{1}}, Edit_User_Id = {{2}} WHERE [{d.IdCol}] = {{3}}"
                    : $"UPDATE [{d.Table}] SET [{d.NameCol}] = {{0}} WHERE [{d.IdCol}] = {{3}}";
                opp.db.Database.ExecuteSql(FormattableStringFactory.Create(sql, n, now, Common.User_Id, id));
                if (d.ItemIdCol != null && d.ItemNameCol != null)
                    opp.db.Database.ExecuteSql(FormattableStringFactory.Create($"UPDATE Item SET [{d.ItemNameCol}] = {{0}} WHERE [{d.ItemIdCol}] = {{1}}", n, id));
                return 0;
            });
        }

        public void SetActive(string kind, long id, bool active)
        {
            var d = Get(kind);
            if (d.ActiveCol == null) throw new InvalidOperationException("هذه القائمة لا تدعم الإيقاف.");
            Run(opp => opp.db.Database.ExecuteSql(FormattableStringFactory.Create($"UPDATE [{d.Table}] SET [{d.ActiveCol}] = {{0}} WHERE [{d.IdCol}] = {{1}}", active ? 1 : 0, id)));
        }

        /// <summary>حذف قيمة غير مستعملة فى أى صنف.</summary>
        public void Delete(string kind, long id)
        {
            var d = Get(kind);
            Run(opp =>
            {
                if (d.ItemIdCol != null)
                {
                    int used = opp.db.Database.SqlQuery<int>(FormattableStringFactory.Create($"SELECT COUNT(*) AS Value FROM Item WHERE [{d.ItemIdCol}] = {{0}}", id)).AsEnumerable().First();
                    if (used > 0) throw new InvalidOperationException($"القيمة مستعملة فى {used} صنف؛ لا تُحذف" + (d.ActiveCol != null ? " ويمكن إيقافها." : "."));
                }
                return opp.db.Database.ExecuteSql(FormattableStringFactory.Create($"DELETE FROM [{d.Table}] WHERE [{d.IdCol}] = {{0}}", id));
            });
        }

        private static string Clean(string name)
        {
            string n = (name ?? "").Trim();
            if (n.Length == 0) throw new InvalidOperationException("الاسم مطلوب.");
            if (n.Length > 200) throw new InvalidOperationException("الاسم طويل جداً.");
            return n;
        }

        private static void EnsureUnique(operations opp, LookupDef d, string name, long exceptId)
        {
            int dup = opp.db.Database.SqlQuery<int>(FormattableStringFactory.Create(
                $"SELECT COUNT(*) AS Value FROM [{d.Table}] WHERE LTRIM(RTRIM([{d.NameCol}])) = {{0}} AND [{d.IdCol}] <> {{1}}", name, exceptId)).AsEnumerable().First();
            if (dup > 0) throw new InvalidOperationException("الاسم موجود بالفعل.");
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());

        private T RunTx<T>(Func<operations, T> f)
        {
            var opp = _newOpp();
            // يعمل داخل معاملة المستدعى إن وُجدت (لا معاملات متوازية على اتصال واحد)، ولا يُنهى إلا ما بدأه.
            bool own = opp.tns?.Connection == null;
            if (own) opp.BeginTransaction();
            try { T r = f(opp); if (own) opp.Commit(); return r; }
            catch { if (own) opp.Rollback(); throw; }
        }
    }
}
