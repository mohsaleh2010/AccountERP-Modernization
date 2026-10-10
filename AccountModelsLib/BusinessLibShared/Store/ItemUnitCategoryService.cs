using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// وحدات الأصناف وفئاتها (خطة 04 — المرحلة 1، الجزء الثانى) بدل UnitListFrm وItemMainCategoryFRM/ItemCategoryFRM.
    /// قواعد (حسب الأنظمة الحديثة):
    /// - معامل التحويل (عدد الصغرى فى الكبرى) لا يتغير لوحدة مستعملة فى أصناف، لأن Unit_item يحفظ نسخة منه وتُحسب بها الأرصدة.
    /// - تعديل اسم الوحدة الكبرى يحدّث نسخته فى Item.UnitStr فقط؛ المستندات (الفواتير/الأذون) تحتفظ بالاسم وقت إصدارها.
    /// - وحدة افتراضية واحدة، وفئة افتراضية واحدة لكل نوع (أصناف/خدمات).
    /// - إيقاف الفئة يمنع اختيارها للأصناف الجديدة فقط ولا يوقف أصنافها (الشاشة القديمة كانت توقف كل الأصناف حتى عند إعادة التشغيل).
    /// - الحذف للمستعمل ممنوع. الاسم لا يُكرر، إلا أن السجلات القديمة المكررة تُعدَّل ما دام اسمها لم يتغير.
    /// - المعرّف: هوية إن كان العمود هوية؛ وإلا Snowflake مثل EF، عدا الفئة الرئيسية فـ MAX+1 لأن Item_Category.Main_Item_Category_Id من نوع int.
    /// </summary>
    public class ItemUnitCategoryService
    {
        private readonly Func<operations> _newOpp;

        public ItemUnitCategoryService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        /// <summary>أنواع الفئة كما فى الشاشة القديمة (الفهرس هو Category_Type_Id).</summary>
        public static readonly string[] CategoryTypes = { "غير محدد", "مبيعات", "مشتريات", "غير مكتمل التصنيع" };

        // ===================== الوحدات =====================

        public class UnitRow
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Big_Unit { get; set; }
            public string Small_Unit { get; set; }
            public decimal Factor { get; set; }
            public int Retail_Parts { get; set; }
            public string Code { get; set; }
            public bool Is_Default { get; set; }
            public int Used_Count { get; set; }
        }

        public List<UnitRow> ListUnits(string text)
        {
            string t = (text ?? "").Trim();
            return Run(opp => opp.db.Database.SqlQuery<UnitRow>($@"
SELECT u.id AS Id, u.u_name AS Name, u.B_Unit AS Big_Unit, u.S_unit AS Small_Unit, CAST(ISNULL(u.S_unit_Value, 1) AS decimal(18,4)) AS Factor,
       ISNULL(u.SS_unit_Value, 1) AS Retail_Parts, ISNULL(u.Code, N'') AS Code, CAST(CASE WHEN u.IS_Default = 1 THEN 1 ELSE 0 END AS bit) AS Is_Default,
       (SELECT COUNT(*) FROM Unit_item ui WHERE ui.unit_id = u.id) + (SELECT COUNT(*) FROM Item i WHERE i.Unit_Id = u.id) AS Used_Count
FROM unit u
WHERE ({t} = N'' OR u.u_name LIKE {"%" + t + "%"} OR u.B_Unit LIKE {"%" + t + "%"} OR u.S_unit LIKE {"%" + t + "%"} OR u.Code LIKE {"%" + t + "%"})
ORDER BY u.u_name").ToList());
        }

        /// <summary>إضافة (Id = 0) أو تعديل وحدة. يرجع المعرّف.</summary>
        public long SaveUnit(UnitRow u)
        {
            string name = Clean(u.Name, "اسم الوحدة"), big = Clean(u.Big_Unit, "الوحدة الكبرى"), small = Clean(u.Small_Unit, "الوحدة الصغرى");
            if (u.Factor <= 0) throw new InvalidOperationException("عدد الوحدات الصغرى فى الكبرى يجب أن يكون أكبر من صفر.");
            if (u.Retail_Parts < 1) throw new InvalidOperationException("أجزاء التجزئة يجب ألا تقل عن 1.");
            string code = string.IsNullOrWhiteSpace(u.Code) ? null : u.Code.Trim();
            return RunTx(opp =>
            {
                int dup = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM unit WHERE LTRIM(RTRIM(u_name)) = {name} AND id <> {u.Id} AND NOT EXISTS (SELECT 1 FROM unit o WHERE o.id = {u.Id} AND LTRIM(RTRIM(o.u_name)) = {name})").AsEnumerable().First();
                if (dup > 0) throw new InvalidOperationException("اسم الوحدة موجود بالفعل.");
                DateTime now = DateTime.Now;
                if (u.Is_Default) opp.db.Database.ExecuteSql($"UPDATE unit SET IS_Default = 0 WHERE IS_Default = 1 AND id <> {u.Id}");
                if (u.Id == 0)
                {
                    long? id = IsIdentity(opp, "unit", "id") ? null : CallSnowflakeIdGenerate.GenerateNewId();
                    if (id == null)
                        return opp.db.Database.SqlQuery<long>($@"INSERT INTO unit (u_name, B_Unit, S_unit, B_Unit_Value, S_unit_Value, SS_unit_Value, Code, IS_Default, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
OUTPUT CAST(INSERTED.id AS bigint) AS Value VALUES ({name}, {big}, {small}, 1, {u.Factor}, {u.Retail_Parts}, {code}, {(u.Is_Default ? 1 : 0)}, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})").AsEnumerable().First();
                    opp.db.Database.ExecuteSql($@"INSERT INTO unit (id, u_name, B_Unit, S_unit, B_Unit_Value, S_unit_Value, SS_unit_Value, Code, IS_Default, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({id.Value}, {name}, {big}, {small}, 1, {u.Factor}, {u.Retail_Parts}, {code}, {(u.Is_Default ? 1 : 0)}, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})");
                    return id.Value;
                }
                var old = opp.db.Database.SqlQuery<UnitRow>($@"SELECT id AS Id, u_name AS Name, B_Unit AS Big_Unit, S_unit AS Small_Unit, CAST(ISNULL(S_unit_Value, 1) AS decimal(18,4)) AS Factor,
       ISNULL(SS_unit_Value, 1) AS Retail_Parts, ISNULL(Code, N'') AS Code, CAST(0 AS bit) AS Is_Default,
       (SELECT COUNT(*) FROM Unit_item ui WHERE ui.unit_id = unit.id) + (SELECT COUNT(*) FROM Item i WHERE i.Unit_Id = unit.id) AS Used_Count
FROM unit WHERE id = {u.Id}").AsEnumerable().FirstOrDefault() ?? throw new InvalidOperationException("الوحدة غير موجودة.");
                if (old.Used_Count > 0 && old.Factor != u.Factor)
                    throw new InvalidOperationException($"الوحدة مستعملة فى {old.Used_Count} سطر صنف؛ لا يتغير عدد الصغرى فى الكبرى لأنه يغيّر الأرصدة. أنشئ وحدة جديدة.");
                opp.db.Database.ExecuteSql($@"UPDATE unit SET u_name = {name}, B_Unit = {big}, S_unit = {small}, S_unit_Value = {u.Factor}, SS_unit_Value = {u.Retail_Parts}, Code = {code},
       IS_Default = {(u.Is_Default ? 1 : 0)}, Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE id = {u.Id}");
                if (old.Big_Unit != big)
                    opp.db.Database.ExecuteSql($"UPDATE Item SET UnitStr = {big} WHERE Unit_Id = {u.Id}");
                return u.Id;
            });
        }

        public void DeleteUnit(long id)
        {
            Run(opp =>
            {
                int used = opp.db.Database.SqlQuery<int>($"SELECT (SELECT COUNT(*) FROM Unit_item WHERE unit_id = {id}) + (SELECT COUNT(*) FROM Item WHERE Unit_Id = {id}) AS Value").AsEnumerable().First();
                if (used > 0) throw new InvalidOperationException($"الوحدة مستعملة فى {used} صنف/سطر؛ لا تُحذف.");
                return opp.db.Database.ExecuteSql($"DELETE FROM unit WHERE id = {id}");
            });
        }

        // ===================== الفئات =====================

        public class MainCategoryRow
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public int Type_Id { get; set; }
            public int Sub_Count { get; set; }
            public int Used_Count { get; set; }
        }

        public class CategoryRow
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public long Main_Id { get; set; }
            public string Main_Name { get; set; }
            public int Type_Id { get; set; }
            /// <summary>0 غير محدد، 1 أصناف، 2 خدمات.</summary>
            public int Item_Or_Service { get; set; }
            public bool Is_Default { get; set; }
            public bool Is_Active { get; set; }
            public int Used_Count { get; set; }
            /// <summary>ملف النشاط (Item_Category.Item_Profile): فارغ = عام، أو قيمة من ItemEditorService.CategoryProfiles.</summary>
            public string Profile { get; set; }
        }

        public List<MainCategoryRow> ListMainCategories() =>
            Run(opp => opp.db.Database.SqlQuery<MainCategoryRow>($@"
SELECT m.ID AS Id, ISNULL(m.Main_Category_Name, N'') AS Name, ISNULL(m.Category_Type_Id, 0) AS Type_Id,
       (SELECT COUNT(*) FROM Item_Category c WHERE c.Main_Item_Category_Id = m.ID) AS Sub_Count,
       (SELECT COUNT(*) FROM Item i JOIN Item_Category c ON c.ID = i.Item_Category_ID WHERE c.Main_Item_Category_Id = m.ID) AS Used_Count
FROM Item_Main_Category m ORDER BY m.Category_Type_Id, m.Main_Category_Name").ToList());

        /// <summary>الفئات؛ mainId = 0 للكل.</summary>
        public List<CategoryRow> ListCategories(long mainId, string text)
        {
            string t = (text ?? "").Trim();
            return Run(opp => opp.db.Database.SqlQuery<CategoryRow>($@"
SELECT c.ID AS Id, ISNULL(c.Category_Name, N'') AS Name, CAST(ISNULL(c.Main_Item_Category_Id, 0) AS bigint) AS Main_Id, ISNULL(m.Main_Category_Name, N'') AS Main_Name,
       ISNULL(c.Category_Type_Id, 0) AS Type_Id, ISNULL(c.Is_ItemOrService, 0) AS Item_Or_Service,
       CAST(CASE WHEN c.Is_Default = 1 THEN 1 ELSE 0 END AS bit) AS Is_Default, CAST(CASE WHEN ISNULL(c.IS_Active, 1) = 1 THEN 1 ELSE 0 END AS bit) AS Is_Active,
       (SELECT COUNT(*) FROM Item i WHERE i.Item_Category_ID = c.ID) AS Used_Count, ISNULL(c.Item_Profile, N'') AS Profile
FROM Item_Category c LEFT JOIN Item_Main_Category m ON m.ID = c.Main_Item_Category_Id
WHERE ({mainId} = 0 OR c.Main_Item_Category_Id = {mainId}) AND ({t} = N'' OR c.Category_Name LIKE {"%" + t + "%"})
ORDER BY m.Main_Category_Name, c.Category_Name").ToList());
        }

        public long SaveMainCategory(MainCategoryRow m)
        {
            string name = Clean(m.Name, "اسم النوع");
            int type = CheckType(m.Type_Id);
            return RunTx(opp =>
            {
                int dup = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item_Main_Category WHERE LTRIM(RTRIM(Main_Category_Name)) = {name} AND ID <> {m.Id} AND NOT EXISTS (SELECT 1 FROM Item_Main_Category o WHERE o.ID = {m.Id} AND LTRIM(RTRIM(o.Main_Category_Name)) = {name})").AsEnumerable().First();
                if (dup > 0) throw new InvalidOperationException("الاسم موجود بالفعل.");
                DateTime now = DateTime.Now;
                if (m.Id == 0)
                {
                    if (IsIdentity(opp, "Item_Main_Category", "ID"))
                        return opp.db.Database.SqlQuery<long>($@"INSERT INTO Item_Main_Category (Main_Category_Name, Category_Type_Id, Category_Type_Str, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
OUTPUT CAST(INSERTED.ID AS bigint) AS Value VALUES ({name}, {type}, {CategoryTypes[type]}, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})").AsEnumerable().First();
                    long id = opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(MAX(ID), 0) + 1 AS bigint) AS Value FROM Item_Main_Category WITH (UPDLOCK, HOLDLOCK)").AsEnumerable().First();
                    opp.db.Database.ExecuteSql($@"INSERT INTO Item_Main_Category (ID, Main_Category_Name, Category_Type_Id, Category_Type_Str, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({id}, {name}, {type}, {CategoryTypes[type]}, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})");
                    return id;
                }
                opp.db.Database.ExecuteSql($@"UPDATE Item_Main_Category SET Main_Category_Name = {name}, Category_Type_Id = {type}, Category_Type_Str = {CategoryTypes[type]},
       Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE ID = {m.Id}");
                return m.Id;
            });
        }

        public void DeleteMainCategory(long id)
        {
            Run(opp =>
            {
                int subs = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item_Category WHERE Main_Item_Category_Id = {id}").AsEnumerable().First();
                if (subs > 0) throw new InvalidOperationException($"النوع يحتوى على {subs} فئة؛ احذفها أو انقلها أولاً.");
                return opp.db.Database.ExecuteSql($"DELETE FROM Item_Main_Category WHERE ID = {id}");
            });
        }

        public long SaveCategory(CategoryRow c)
        {
            string name = Clean(c.Name, "اسم الفئة");
            int type = CheckType(c.Type_Id);
            if (c.Item_Or_Service is < 0 or > 2) throw new InvalidOperationException("نوع الفئة (أصناف/خدمات) غير صحيح.");
            if (c.Is_Default && c.Item_Or_Service == 0) throw new InvalidOperationException("الفئة الافتراضية يجب أن تكون للأصناف أو للخدمات.");
            if (c.Is_Default && !c.Is_Active) throw new InvalidOperationException("الفئة الافتراضية لا تكون موقوفة.");
            string profile = string.IsNullOrWhiteSpace(c.Profile) ? null : c.Profile.Trim().ToLowerInvariant();
            if (profile != null && !ItemEditorService.CategoryProfiles.Contains(profile)) throw new InvalidOperationException("ملف النشاط غير صحيح.");
            return RunTx(opp =>
            {
                int main = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item_Main_Category WHERE ID = {c.Main_Id}").AsEnumerable().First();
                if (main == 0) throw new InvalidOperationException("اختر نوع الفئة الرئيسى.");
                int dup = opp.db.Database.SqlQuery<int>($@"SELECT COUNT(*) AS Value FROM Item_Category
WHERE LTRIM(RTRIM(Category_Name)) = {name} AND Main_Item_Category_Id = {c.Main_Id} AND ID <> {c.Id}
AND NOT EXISTS (SELECT 1 FROM Item_Category o WHERE o.ID = {c.Id} AND LTRIM(RTRIM(o.Category_Name)) = {name} AND o.Main_Item_Category_Id = {c.Main_Id})").AsEnumerable().First();
                if (dup > 0) throw new InvalidOperationException("الفئة موجودة بالفعل تحت نفس النوع.");
                if (c.Is_Default)
                    opp.db.Database.ExecuteSql($"UPDATE Item_Category SET Is_Default = 0 WHERE Is_Default = 1 AND Is_ItemOrService = {c.Item_Or_Service} AND ID <> {c.Id}");
                DateTime now = DateTime.Now;
                int mainId = checked((int)c.Main_Id);
                if (c.Id == 0)
                {
                    long? id = IsIdentity(opp, "Item_Category", "ID") ? null : CallSnowflakeIdGenerate.GenerateNewId();
                    if (id == null)
                        return opp.db.Database.SqlQuery<long>($@"INSERT INTO Item_Category (Category_Name, Category_Type_Id, Category_Type_Str, Main_Item_Category_Id, IS_Active, Is_ItemOrService, Is_Default, Item_Profile, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
OUTPUT CAST(INSERTED.ID AS bigint) AS Value VALUES ({name}, {type}, {CategoryTypes[type]}, {mainId}, {(c.Is_Active ? 1 : 0)}, {c.Item_Or_Service}, {(c.Is_Default ? 1 : 0)}, {profile}, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})").AsEnumerable().First();
                    opp.db.Database.ExecuteSql($@"INSERT INTO Item_Category (ID, Category_Name, Category_Type_Id, Category_Type_Str, Main_Item_Category_Id, IS_Active, Is_ItemOrService, Is_Default, Item_Profile, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({id.Value}, {name}, {type}, {CategoryTypes[type]}, {mainId}, {(c.Is_Active ? 1 : 0)}, {c.Item_Or_Service}, {(c.Is_Default ? 1 : 0)}, {profile}, {now.Date}, {Common.User_Id}, {Common.Branch_Id}, {now})");
                    return id.Value;
                }
                opp.db.Database.ExecuteSql($@"UPDATE Item_Category SET Category_Name = {name}, Category_Type_Id = {type}, Category_Type_Str = {CategoryTypes[type]}, Main_Item_Category_Id = {mainId},
       IS_Active = {(c.Is_Active ? 1 : 0)}, Is_ItemOrService = {c.Item_Or_Service}, Is_Default = {(c.Is_Default ? 1 : 0)}, Item_Profile = {profile},
       Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id} WHERE ID = {c.Id}");
                return c.Id;
            });
        }

        /// <summary>إيقاف/تشغيل الفئة دون المساس بأصنافها.</summary>
        public void SetCategoryActive(long id, bool active)
        {
            Run(opp =>
            {
                if (!active)
                {
                    int def = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item_Category WHERE ID = {id} AND Is_Default = 1").AsEnumerable().First();
                    if (def > 0) throw new InvalidOperationException("الفئة الافتراضية لا تُوقف؛ اجعل فئة أخرى افتراضية أولاً.");
                }
                return opp.db.Database.ExecuteSql($"UPDATE Item_Category SET IS_Active = {(active ? 1 : 0)}, Update_ServerDateTime = {DateTime.Now}, Edit_User_Id = {Common.User_Id} WHERE ID = {id}");
            });
        }

        public void DeleteCategory(long id)
        {
            Run(opp =>
            {
                int used = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Item WHERE Item_Category_ID = {id}").AsEnumerable().First();
                if (used > 0) throw new InvalidOperationException($"الفئة مستعملة فى {used} صنف؛ لا تُحذف ويمكن إيقافها.");
                return opp.db.Database.ExecuteSql($"DELETE FROM Item_Category WHERE ID = {id}");
            });
        }

        // ===================== مساعدات =====================

        private static int CheckType(int t) =>
            t >= 0 && t < CategoryTypes.Length ? t : throw new InvalidOperationException("طبيعة الفئة غير صحيحة.");

        private static bool IsIdentity(operations opp, string table, string col) =>
            opp.db.Database.SqlQuery<int>(FormattableStringFactory.Create(
                "SELECT ISNULL(COLUMNPROPERTY(OBJECT_ID({0}), {1}, 'IsIdentity'), 0) AS Value", table, col)).AsEnumerable().First() == 1;

        private static string Clean(string name, string label)
        {
            string n = (name ?? "").Trim();
            if (n.Length == 0) throw new InvalidOperationException(label + " مطلوب.");
            if (n.Length > 200) throw new InvalidOperationException(label + " طويل جداً.");
            return n;
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());

        private T RunTx<T>(Func<operations, T> f)
        {
            var opp = _newOpp();
            bool own = opp.tns?.Connection == null;
            if (own) opp.BeginTransaction();
            try { T r = f(opp); if (own) opp.Commit(); return r; }
            catch { if (own) opp.Rollback(); throw; }
        }
    }
}
