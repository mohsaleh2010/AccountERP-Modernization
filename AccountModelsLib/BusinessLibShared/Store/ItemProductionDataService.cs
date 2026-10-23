using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    /// <summary>
    /// خطة 05 بند 22: بيانات إنتاج أصناف النسيج (Item_PriceUnit_List) بدل Item_ProductionData وتقريرى بيانات أصناف الوبرة/اللحاف.
    /// لكل صنف نوع منتج صريح (Main_Item_Category_Id_Lehaf_Wabara: 1 وبرة بصف واحد، 2 لحاف بقطع فرعية لكل منها صف باسم ورقم)
    /// بدل استنتاجه من رقم النوع الرئيسى للفئة كالقديم. الصف يجمع بيانات النسيج (الرسمة، الحدفات، الوزن، الأبعاد، سعر الألف حدفة،
    /// الهيئة والنوع) وبيانات التطريز (الرسمة، الغرز، الوحدة، سعر الألف وحدة، الأباليك)، ويُكتب Item_Name المركّب كالقديم
    /// («اسم الصنف الرسمة الغرز الوحدة») لأن شاشات معدلات الإنتاج تقرؤه. الإضافة بـ EF (معرّف Snowflake وسجل المزامنة)،
    /// والقراءة والتعديل بـ SQL بمعاملات (بعض أعمدة الجدول bigint بينما خصائص النموذج int).
    /// </summary>
    public class ItemProductionDataService
    {
        public const int Wabara = 1, Lehaf = 2;
        /// <summary>وحدات قياس التطريز كقائمة الشاشة القديمة (تُحفظ نصاً فى Item_Unit).</summary>
        public static readonly string[] Units = { "الدستة", "قطعة", "غرزة", "الكيلو", "المتر", "المتر المربع" };

        private readonly Func<operations> _newOpp;
        private readonly Func<int, bool> _hasPermission;

        public ItemProductionDataService(Func<operations> newOpp = null, Func<int, bool> hasPermission = null)
        {
            _newOpp = newOpp ?? (() => new operations());
            _hasPermission = hasPermission ?? (p => Common.IsHavePermission(Common.User_Id, p, Common.Project_ID, Common.Branch_Id));
        }

        public class ProductionRow
        {
            public long Id { get; set; }
            public long Item_Id { get; set; }
            public int Kind { get; set; } = Wabara;
            public string Sub_Name { get; set; }
            public int Sub_No { get; set; }
            // النسيج
            public string Textile_Drawing { get; set; }
            public decimal Hadafat_Nazla { get; set; }
            public decimal Hadafat_Tash3heel { get; set; }
            public decimal Weight { get; set; }
            public decimal Length { get; set; }
            public decimal Width { get; set; }
            public decimal Weight_M2 { get; set; }
            public decimal Alf_Hadfa_Price { get; set; }
            public long Body_Id { get; set; }
            public string Body_Name { get; set; }
            public long Type_Id { get; set; }
            public string Type_Name { get; set; }
            // التطريز
            public string Draw_Name { get; set; }
            public decimal Stitch_No { get; set; }
            public string Unit { get; set; }
            public decimal Unit_Price { get; set; }
            public long Abalek_No { get; set; }
            public long Ableka_Price { get; set; }
        }

        /// <summary>صف تقرير بيانات الإنتاج (كل الأصناف).</summary>
        public class ReportRow : ProductionRow
        {
            public string Item_Code { get; set; }
            public string Item_Name { get; set; }
            public string Category { get; set; }
        }

        private const string Columns = @"p.id AS Id, CAST(ISNULL(p.Item_ID, 0) AS bigint) AS Item_Id, CAST(ISNULL(p.Main_Item_Category_Id_Lehaf_Wabara, 1) AS int) AS Kind,
       ISNULL(p.SubItem_Name, N'') AS Sub_Name, ISNULL(p.SubItem_No, 0) AS Sub_No,
       ISNULL(p.TextileDrawingName, N'') AS Textile_Drawing, CAST(ISNULL(p.Hadafat_In_NazlaNo, 0) AS decimal(18,2)) AS Hadafat_Nazla,
       CAST(ISNULL(p.Hadafat_Tash3heel, 0) AS decimal(18,2)) AS Hadafat_Tash3heel, CAST(ISNULL(p.ItemWeightTextile, 0) AS decimal(18,2)) AS Weight,
       CAST(ISNULL(p.Item_Length, 0) AS decimal(18,2)) AS Length, CAST(ISNULL(p.Item_Width, 0) AS decimal(18,2)) AS Width,
       CAST(ISNULL(p.Item_Weight_Per_Square_meter, 0) AS decimal(18,2)) AS Weight_M2, CAST(ISNULL(p.Alf_Hadfa_Price, 0) AS decimal(18,2)) AS Alf_Hadfa_Price,
       CAST(ISNULL(p.Items_Bodies_Name_Id, 0) AS bigint) AS Body_Id, ISNULL(p.Items_Bodies_Name, N'') AS Body_Name,
       CAST(ISNULL(p.Items_Types_Name_Id, 0) AS bigint) AS Type_Id, ISNULL(p.Items_Types_Name, N'') AS Type_Name,
       ISNULL(p.DrawName, N'') AS Draw_Name, CAST(ISNULL(p.Stitch_No, 0) AS decimal(18,2)) AS Stitch_No, ISNULL(p.Item_Unit, N'') AS Unit,
       CAST(ISNULL(p.Item_UnitPrice, 0) AS decimal(18,2)) AS Unit_Price, CAST(ISNULL(p.Abalek_No, 0) AS bigint) AS Abalek_No,
       CAST(ISNULL(p.Ableka_Price, 0) AS bigint) AS Ableka_Price";

        // ===================== القراءة =====================

        public List<ProductionRow> List(long itemId) => Run(opp => opp.db.Database.SqlQuery<ProductionRow>(System.Runtime.CompilerServices.FormattableStringFactory.Create(
            $"SELECT {Columns} FROM Item_PriceUnit_List p WHERE p.Item_ID = {{0}} ORDER BY ISNULL(p.SubItem_No, 0), p.SubItem_Name", itemId)).ToList());

        /// <summary>تقرير بيانات الإنتاج: النوع 0 الكل، 1 وبرة، 2 لحاف؛ بحث بكود/اسم الصنف أو الرسمة.</summary>
        public List<ReportRow> Report(int kind, string text, long categoryId)
        {
            string t = (text ?? "").Trim(), like = "%" + t + "%";
            return Run(opp => opp.db.Database.SqlQuery<ReportRow>(System.Runtime.CompilerServices.FormattableStringFactory.Create($@"
SELECT {Columns}, ISNULL(i.code, N'') AS Item_Code, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(c.Category_Name, N'') AS Category
FROM Item_PriceUnit_List p JOIN Item i ON i.id = p.Item_ID LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
WHERE ({{0}} = 0 OR ISNULL(p.Main_Item_Category_Id_Lehaf_Wabara, 1) = {{0}})
  AND ({{1}} = N'' OR i.Item_Name LIKE {{2}} OR i.code = {{1}} OR p.TextileDrawingName LIKE {{2}} OR p.DrawName LIKE {{2}} OR p.SubItem_Name LIKE {{2}})
  AND ({{3}} = 0 OR i.Item_Category_ID = {{3}})
ORDER BY i.Item_Name, ISNULL(p.SubItem_No, 0), p.SubItem_Name", kind, t, like, categoryId)).ToList());
        }

        // ===================== الحفظ =====================

        /// <summary>إضافة صف (Id = 0) أو تعديله. الوبرة صف واحد للصنف؛ قطع اللحاف بأسماء غير مكررة. يرجع المعرّف.</summary>
        public long Save(ProductionRow r)
        {
            if (!_hasPermission(ItemEditorService.PermissionEdit)) throw new InvalidOperationException("ليس لديك صلاحية تعديل بيانات الصنف.");
            Validate(r);
            return RunTx(opp =>
            {
                string itemName = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Item_Name, N'') AS Value FROM Item WHERE id = {r.Item_Id}").AsEnumerable().FirstOrDefault()
                    ?? throw new InvalidOperationException("الصنف غير موجود؛ أعد تحميل الصفحة.");
                var others = List(opp, r.Item_Id).Where(x => x.Id != r.Id).ToList();
                if (others.Any(x => x.Kind != r.Kind)) throw new InvalidOperationException("نوع المنتج (وبرة / لحاف) يتغير لكل بيانات الصنف معاً من «تغيير النوع».");
                if (r.Kind == Wabara && others.Count > 0) throw new InvalidOperationException("الوبرة لها صف بيانات إنتاج واحد؛ عدّله بدل إضافة صف آخر.");
                if (r.Kind == Lehaf && others.Any(x => string.Equals(x.Sub_Name?.Trim(), r.Sub_Name, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("اسم القطعة موجود بالفعل لهذا الصنف.");
                r.Body_Name = LookupName(opp, "bodies", r.Body_Id);
                r.Type_Name = LookupName(opp, "types", r.Type_Id);
                // الاسم المركّب كالقديم (Item_List.Add_Item): «اسم الصنف الرسمة الغرز الوحدة»
                string full = $"{itemName} {r.Draw_Name} {r.Stitch_No.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)} {r.Unit}".Trim();
                if (full.Length > 100) full = full.Substring(0, 100);
                string itemsName = itemName.Length > 100 ? itemName.Substring(0, 100) : itemName;
                DateTime now = DateTime.Now;
                if (r.Id == 0)
                {
                    var e = new Item_PriceUnit_List
                    {
                        Item_ID = r.Item_Id, Unit_Item_ID = 0, Item_Name = full, ItemsName = itemsName, Main_Item_Category_Id_Lehaf_Wabara = r.Kind,
                        SubItem_Name = r.Sub_Name, SubItem_No = r.Sub_No, OperationDate = now.Date, User_Id = Common.User_Id, Branch_Id = Common.Branch_Id,
                        Insert_ServerDateTime = now, ProcessNo = 0,
                    };
                    Apply(e, r);
                    opp.db.Item_PriceUnit_List.Add(e);
                    opp.db.SaveChanges();
                    return e.id;
                }
                int n = opp.db.Database.ExecuteSql($@"UPDATE Item_PriceUnit_List SET
    Item_Name = {full}, ItemsName = {itemsName}, Main_Item_Category_Id_Lehaf_Wabara = {r.Kind}, SubItem_Name = {r.Sub_Name}, SubItem_No = {r.Sub_No},
    TextileDrawingName = {r.Textile_Drawing}, Hadafat_In_NazlaNo = {r.Hadafat_Nazla}, Hadafat_Tash3heel = {r.Hadafat_Tash3heel},
    ItemWeightTextile = {r.Weight}, Item_Length = {r.Length}, Item_Width = {r.Width}, Item_Weight_Per_Square_meter = {r.Weight_M2},
    Alf_Hadfa_Price = {r.Alf_Hadfa_Price}, Items_Bodies_Name_Id = {r.Body_Id}, Items_Bodies_Name = {r.Body_Name},
    Items_Types_Name_Id = {r.Type_Id}, Items_Types_Name = {r.Type_Name},
    DrawName = {r.Draw_Name}, Stitch_No = {r.Stitch_No}, Item_Unit = {r.Unit}, Item_UnitPrice = {r.Unit_Price},
    Abalek_No = {r.Abalek_No}, Ableka_Price = {r.Ableka_Price},
    Update_ServerDateTime = {now}, EditOperationDate = {now}, Edit_User_Id = {Common.User_Id}
WHERE id = {r.Id} AND Item_ID = {r.Item_Id}");
                if (n == 0) throw new InvalidOperationException("صف بيانات الإنتاج غير موجود؛ أعد تحميل الصفحة.");
                return r.Id;
            });
        }

        /// <summary>تغيير نوع المنتج لكل صفوف الصنف (اللحاف ← الوبرة يحتاج صفاً واحداً على الأكثر).</summary>
        public void SetKind(long itemId, int kind)
        {
            if (!_hasPermission(ItemEditorService.PermissionEdit)) throw new InvalidOperationException("ليس لديك صلاحية تعديل بيانات الصنف.");
            if (kind is not (Wabara or Lehaf)) throw new InvalidOperationException("نوع المنتج غير صحيح.");
            RunTx(opp =>
            {
                var rows = List(opp, itemId);
                if (kind == Wabara && rows.Count > 1) throw new InvalidOperationException("للصنف أكثر من قطعة؛ احذف القطع الزائدة قبل تحويله إلى وبرة.");
                return opp.db.Database.ExecuteSql($"UPDATE Item_PriceUnit_List SET Main_Item_Category_Id_Lehaf_Wabara = {kind} WHERE Item_ID = {itemId}");
            });
        }

        public void Delete(long itemId, long id)
        {
            if (!_hasPermission(ItemEditorService.PermissionEdit)) throw new InvalidOperationException("ليس لديك صلاحية تعديل بيانات الصنف.");
            Run(opp => opp.db.Database.ExecuteSql($"DELETE FROM Item_PriceUnit_List WHERE id = {id} AND Item_ID = {itemId}"));
        }

        // ===================== مساعدات =====================

        private static List<ProductionRow> List(operations opp, long itemId) => opp.db.Database.SqlQuery<ProductionRow>(System.Runtime.CompilerServices.FormattableStringFactory.Create(
            $"SELECT {Columns} FROM Item_PriceUnit_List p WHERE p.Item_ID = {{0}}", itemId)).ToList();

        private static void Validate(ProductionRow r)
        {
            if (r.Item_Id == 0) throw new InvalidOperationException("احفظ الصنف أولاً.");
            if (r.Kind is not (Wabara or Lehaf)) throw new InvalidOperationException("اختر نوع المنتج (وبرة / لحاف).");
            static string T(string v, int max, string field)
            {
                string s = string.IsNullOrWhiteSpace(v) ? null : v.Trim();
                if (s?.Length > max) throw new InvalidOperationException(field + " طويل جداً.");
                return s;
            }
            r.Sub_Name = T(r.Sub_Name, 200, "اسم القطعة");
            if (r.Kind == Lehaf && r.Sub_Name == null) throw new InvalidOperationException("اسم القطعة مطلوب للحاف.");
            if (r.Kind == Wabara) { r.Sub_Name = null; r.Sub_No = 0; }
            r.Textile_Drawing = T(r.Textile_Drawing, 1000, "اسم رسمة النسيج");
            r.Draw_Name = T(r.Draw_Name, 100, "اسم رسمة التطريز");
            r.Unit = T(r.Unit, 100, "وحدة القياس");
            if (r.Unit != null && !Units.Contains(r.Unit)) throw new InvalidOperationException("وحدة القياس غير صحيحة.");
            if (r.Sub_No < 0 || r.Abalek_No < 0 || r.Ableka_Price < 0
                || new[] { r.Hadafat_Nazla, r.Hadafat_Tash3heel, r.Weight, r.Length, r.Width, r.Weight_M2, r.Alf_Hadfa_Price, r.Stitch_No, r.Unit_Price }.Any(v => v < 0))
                throw new InvalidOperationException("بيانات الإنتاج لا تقبل قيماً سالبة.");
            // أعمدة بلا كسور فى الجدول (decimal(18,0))
            r.Hadafat_Nazla = Math.Round(r.Hadafat_Nazla, 0);
            r.Alf_Hadfa_Price = Math.Round(r.Alf_Hadfa_Price, 0);
        }

        private static void Apply(Item_PriceUnit_List e, ProductionRow r)
        {
            e.TextileDrawingName = r.Textile_Drawing; e.Hadafat_In_NazlaNo = r.Hadafat_Nazla; e.Hadafat_Tash3heel = r.Hadafat_Tash3heel;
            e.ItemWeightTextile = r.Weight; e.Item_Length = r.Length; e.Item_Width = r.Width; e.Item_Weight_Per_Square_meter = r.Weight_M2;
            e.Alf_Hadfa_Price = r.Alf_Hadfa_Price;
            e.Items_Bodies_Name_Id = checked((int)r.Body_Id); e.Items_Bodies_Name = r.Body_Name;
            e.Items_Types_Name_Id = checked((int)r.Type_Id); e.Items_Types_Name = r.Type_Name;
            e.DrawName = r.Draw_Name; e.Stitch_No = r.Stitch_No; e.Item_Unit = r.Unit; e.Item_UnitPrice = r.Unit_Price;
            e.Abalek_No = r.Abalek_No; e.Ableka_Price = r.Ableka_Price;
        }

        /// <summary>اسم قيمة الهيئة/النوع من جدولها المرجعى (فارغ = بدون).</summary>
        private static string LookupName(operations opp, string kind, long id)
        {
            if (id == 0) return null;
            var d = ItemLookupService.Get(kind);
            return opp.db.Database.SqlQuery<string>(System.Runtime.CompilerServices.FormattableStringFactory.Create(
                $"SELECT ISNULL([{d.NameCol}], N'') AS Value FROM [{d.Table}] WHERE [{d.IdCol}] = {{0}}", id)).AsEnumerable().FirstOrDefault()
                ?? throw new InvalidOperationException("قيمة الهيئة/النوع غير موجودة؛ أعد تحميل الصفحة.");
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
