using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// خطة 05 بند 19: أنواع السيارات (Lookup_CarType) وموديلاتها (Lookup_CarTypeModel) تحت قائمة السيارات، بدل /items/lookups/car-types
    /// وشاشة CarsTypeListFrm. قواعد (حسب الأنظمة الحديثة): لا يُكرر اسم النوع، ولا اسم الموديل داخل نفس النوع؛ إعادة التسمية تحدّث نسخ الاسم
    /// المخزَّنة (Lookup_CarTypeModel.CarType، Cars.CarType، Cars.Model)؛ الحذف لغير المستعمل فقط (نوع بلا موديلات ولا أصناف ولا سيارات،
    /// وموديل بلا سيارات). كل القيم معاملات.
    /// </summary>
    public class CarTypeService
    {
        private readonly Func<operations> _newOpp;

        public CarTypeService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class TypeRow
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Name_En { get; set; }
            public string Code { get; set; }
            public int Models_Count { get; set; }
            public int Items_Count { get; set; }
            public int Cars_Count { get; set; }
        }

        public class ModelRow
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public long Type_Id { get; set; }
            public string Type_Name { get; set; }
            public int Cars_Count { get; set; }
        }

        // ===================== الأنواع =====================

        public List<TypeRow> ListTypes(string text)
        {
            string t = (text ?? "").Trim(), like = "%" + t + "%";
            return Run(opp => opp.db.Database.SqlQuery<TypeRow>($@"
SELECT l.Id, ISNULL(l.Name, N'') AS Name, ISNULL(l.NameEn, N'') AS Name_En, ISNULL(l.Code, N'') AS Code,
       (SELECT COUNT(*) FROM Lookup_CarTypeModel m WHERE m.Lookup_CarTypeId = l.Id) AS Models_Count,
       (SELECT COUNT(*) FROM Item i WHERE i.Lookup_CarTypeId = l.Id) AS Items_Count,
       (SELECT COUNT(*) FROM Cars c WHERE c.Lookup_CarTypeId = l.Id) AS Cars_Count
FROM Lookup_CarType l
WHERE ({t} = N'' OR l.Name LIKE {like} OR l.NameEn LIKE {like} OR l.Code = {t})
ORDER BY l.Name").ToList());
        }

        /// <summary>إضافة نوع (Id = 0) أو تعديله. يرجع المعرّف.</summary>
        public long SaveType(TypeRow r)
        {
            string name = Clean(r.Name, "اسم النوع", 255);
            string en = Optional(r.Name_En, "الاسم الإنجليزى", 500), code = Optional(r.Code, "الكود", 500);
            return RunTx(opp =>
            {
                int dup = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Lookup_CarType WHERE LTRIM(RTRIM(Name)) = {name} AND Id <> {r.Id}").AsEnumerable().First();
                if (dup > 0) throw new InvalidOperationException("نوع السيارة موجود بالفعل.");
                if (code != null && opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Lookup_CarType WHERE LTRIM(RTRIM(Code)) = {code} AND Id <> {r.Id}").AsEnumerable().First() > 0)
                    throw new InvalidOperationException("الكود مستعمل لنوع آخر.");
                if (r.Id == 0)
                    return opp.db.Database.SqlQuery<long>($"INSERT INTO Lookup_CarType (Name, NameEn, Code) OUTPUT INSERTED.Id AS Value VALUES ({name}, {en}, {code})").AsEnumerable().First();
                if (opp.db.Database.ExecuteSql($"UPDATE Lookup_CarType SET Name = {name}, NameEn = {en}, Code = {code} WHERE Id = {r.Id}") == 0)
                    throw new InvalidOperationException("النوع غير موجود؛ أعد تحميل الصفحة.");
                // نسخ الاسم المخزَّنة كالقديم (الموديلات والسيارات)
                opp.db.Database.ExecuteSql($"UPDATE Lookup_CarTypeModel SET CarType = {name} WHERE Lookup_CarTypeId = {r.Id} AND ISNULL(CarType, N'') <> {name}");
                opp.db.Database.ExecuteSql($"UPDATE Cars SET CarType = {name} WHERE Lookup_CarTypeId = {r.Id} AND ISNULL(CarType, N'') <> {name}");
                return r.Id;
            });
        }

        public void DeleteType(long id) => RunTx(opp =>
        {
            var u = opp.db.Database.SqlQuery<TypeRow>($@"SELECT l.Id, l.Name, l.NameEn AS Name_En, l.Code,
       (SELECT COUNT(*) FROM Lookup_CarTypeModel m WHERE m.Lookup_CarTypeId = l.Id) AS Models_Count,
       (SELECT COUNT(*) FROM Item i WHERE i.Lookup_CarTypeId = l.Id) AS Items_Count,
       (SELECT COUNT(*) FROM Cars c WHERE c.Lookup_CarTypeId = l.Id) AS Cars_Count
FROM Lookup_CarType l WHERE l.Id = {id}").AsEnumerable().FirstOrDefault() ?? throw new InvalidOperationException("النوع غير موجود؛ أعد تحميل الصفحة.");
            if (u.Models_Count > 0) throw new InvalidOperationException($"للنوع {u.Models_Count} موديل؛ احذفها أولاً.");
            if (u.Items_Count > 0 || u.Cars_Count > 0) throw new InvalidOperationException($"النوع مستعمل فى {u.Items_Count} صنف و{u.Cars_Count} سيارة؛ لا يُحذف.");
            return opp.db.Database.ExecuteSql($"DELETE FROM Lookup_CarType WHERE Id = {id}");
        });

        // ===================== الموديلات =====================

        public List<ModelRow> ListModels(long typeId, string text)
        {
            string t = (text ?? "").Trim(), like = "%" + t + "%";
            return Run(opp => opp.db.Database.SqlQuery<ModelRow>($@"
SELECT m.Id, ISNULL(m.Name, N'') AS Name, ISNULL(m.Lookup_CarTypeId, 0) AS Type_Id, COALESCE(l.Name, m.CarType, N'') AS Type_Name,
       (SELECT COUNT(*) FROM Cars c WHERE c.Lookup_CarTypeModelId = m.Id) AS Cars_Count
FROM Lookup_CarTypeModel m LEFT JOIN Lookup_CarType l ON l.Id = m.Lookup_CarTypeId
WHERE ({typeId} = 0 OR m.Lookup_CarTypeId = {typeId}) AND ({t} = N'' OR m.Name LIKE {like})
ORDER BY l.Name, m.Name").ToList());
        }

        /// <summary>إضافة موديل (Id = 0) أو تعديله؛ اسم النوع يُنسخ فى CarType كالقديم. يرجع المعرّف.</summary>
        public long SaveModel(ModelRow r)
        {
            string name = Clean(r.Name, "اسم الموديل", 500);
            if (r.Type_Id == 0) throw new InvalidOperationException("اختر نوع السيارة.");
            return RunTx(opp =>
            {
                string type = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Name, N'') AS Value FROM Lookup_CarType WHERE Id = {r.Type_Id}").AsEnumerable().FirstOrDefault()
                    ?? throw new InvalidOperationException("نوع السيارة غير موجود؛ أعد تحميل الصفحة.");
                int dup = opp.db.Database.SqlQuery<int>($@"SELECT COUNT(*) AS Value FROM Lookup_CarTypeModel
WHERE LTRIM(RTRIM(Name)) = {name} AND Lookup_CarTypeId = {r.Type_Id} AND Id <> {r.Id}").AsEnumerable().First();
                if (dup > 0) throw new InvalidOperationException("الموديل موجود بالفعل لنفس النوع.");
                if (r.Id == 0)
                    return opp.db.Database.SqlQuery<long>($"INSERT INTO Lookup_CarTypeModel (Name, CarType, Lookup_CarTypeId) OUTPUT INSERTED.Id AS Value VALUES ({name}, {type}, {r.Type_Id})").AsEnumerable().First();
                long oldType = opp.db.Database.SqlQuery<long>($"SELECT ISNULL(Lookup_CarTypeId, 0) AS Value FROM Lookup_CarTypeModel WHERE Id = {r.Id}").AsEnumerable().Select(v => (long?)v).FirstOrDefault() ?? -1;
                if (oldType == -1) throw new InvalidOperationException("الموديل غير موجود؛ أعد تحميل الصفحة.");
                if (oldType != r.Type_Id && opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Cars WHERE Lookup_CarTypeModelId = {r.Id}").AsEnumerable().First() > 0)
                    throw new InvalidOperationException("لا يتغير نوع موديل مستعمل فى سيارات.");
                opp.db.Database.ExecuteSql($"UPDATE Lookup_CarTypeModel SET Name = {name}, CarType = {type}, Lookup_CarTypeId = {r.Type_Id} WHERE Id = {r.Id}");
                opp.db.Database.ExecuteSql($"UPDATE Cars SET Model = {name} WHERE Lookup_CarTypeModelId = {r.Id} AND ISNULL(Model, N'') <> {name}");
                return r.Id;
            });
        }

        public void DeleteModel(long id) => RunTx(opp =>
        {
            int cars = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Cars WHERE Lookup_CarTypeModelId = {id}").AsEnumerable().First();
            if (cars > 0) throw new InvalidOperationException($"الموديل مستعمل فى {cars} سيارة؛ لا يُحذف.");
            return opp.db.Database.ExecuteSql($"DELETE FROM Lookup_CarTypeModel WHERE Id = {id}");
        });

        // ===================== مساعدات =====================

        private static string Clean(string v, string field, int max)
        {
            string n = (v ?? "").Trim();
            if (n.Length == 0) throw new InvalidOperationException(field + " مطلوب.");
            if (n.Length > max) throw new InvalidOperationException(field + " طويل جداً.");
            return n;
        }

        private static string Optional(string v, string field, int max)
        {
            string n = string.IsNullOrWhiteSpace(v) ? null : v.Trim();
            if (n?.Length > max) throw new InvalidOperationException(field + " طويل جداً.");
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
