using System.Globalization;
using AccountModelsLib;

namespace AccountERP.SharedUI.Services
{
    /// <summary>قالب Excel لأرصدة أول المدة وقراءته (خطة 02). المفتاح عمود Unit_Item_Id؛ باقى الأعمدة للقراءة البشرية ما عدا الأربعة القابلة للتعديل.</summary>
    public static class ItemOpeningExcel
    {
        public const string KeyColumn = "Unit_Item_Id";
        public const string LargeColumn = "Large";
        public const string SmallColumn = "Small";
        public const string PacksColumn = "Packs";
        public const string CostColumn = "CostLarge";

        public static byte[] BuildTemplate(IEnumerable<ItemOpeningBalanceListService.OpeningRow> rows)
        {
            var data = rows.Select(r => new Dictionary<string, object>
            {
                // المعرّف نصاً: أرقام Snowflake أكبر من دقة الرقم العشرى (2^53) فكانت ستتغير فى Excel.
                [KeyColumn] = r.Unit_Item_Id.ToString(CultureInfo.InvariantCulture),
                ["Store"] = r.Store_Name,
                ["Item"] = r.Item_Name,
                ["Code"] = r.Code,
                ["LargeUnit"] = r.B_Unit,
                ["SmallUnit"] = r.S_Unit,
                ["Factor"] = r.Factor,
                [LargeColumn] = r.Open_Large,
                [SmallColumn] = r.Open_Small,
                [PacksColumn] = r.Open_No,
                [CostColumn] = r.Cost_Large,
            }).ToList();
            using var ms = new MemoryStream();
            MiniExcelLibs.MiniExcel.SaveAs(ms, data);
            return ms.ToArray();
        }

        /// <summary>يقرأ الملف إلى أسطر حفظ. الصف بلا Unit_Item_Id أو بقيمة غير رقمية/سالبة يُسجَّل فى المشكلات ولا يُحفظ.</summary>
        public static (List<ItemOpeningBalanceListService.SaveLine> Lines, List<string> Problems) Parse(Stream xlsx)
        {
            var lines = new List<ItemOpeningBalanceListService.SaveLine>();
            var problems = new List<string>();
            int rowNo = 1;
            foreach (IDictionary<string, object> row in MiniExcelLibs.MiniExcel.Query(xlsx, useHeaderRow: true))
            {
                rowNo++;
                bool okKey = TryDecimal(row, KeyColumn, out var key);
                bool ok1 = TryDecimal(row, LargeColumn, out var large);
                bool ok2 = TryDecimal(row, SmallColumn, out var small);
                bool ok3 = TryDecimal(row, PacksColumn, out var packs);
                bool ok4 = TryDecimal(row, CostColumn, out var cost);
                if (!okKey || key <= 0 || key != Math.Floor(key))
                {
                    problems.Add($"{rowNo}: {KeyColumn}");
                    continue;
                }
                if (!(ok1 && ok2 && ok3 && ok4) || large < 0 || small < 0 || packs < 0 || cost < 0)
                {
                    problems.Add($"{rowNo}: {LargeColumn}/{SmallColumn}/{PacksColumn}/{CostColumn}");
                    continue;
                }
                lines.Add(new ItemOpeningBalanceListService.SaveLine { UnitItemId = (long)key, QtyLarge = large, QtySmall = small, ItemNo = packs, CostLarge = cost });
            }
            return (lines, problems);
        }

        // خلية فارغة = 0 (ليست خطأ) إلا عمود المفتاح؛ نص غير رقمى = خطأ.
        private static bool TryDecimal(IDictionary<string, object> row, string col, out decimal value)
        {
            value = 0;
            if (!row.TryGetValue(col, out var raw) || raw == null || (raw is string e && string.IsNullOrWhiteSpace(e))) return col != KeyColumn;
            switch (raw)
            {
                case double d:
                    if (col == KeyColumn && Math.Abs(d) > 9007199254740992d) return false;   // رقم كبير فقد دقته؛ يجب أن يبقى المعرّف نصاً
                    value = (decimal)d; return true;
                case decimal m: value = m; return true;
                case int i: value = i; return true;
                case long l: value = l; return true;
                case string s when decimal.TryParse(s.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var p): value = p; return true;
                default: return false;
            }
        }
    }
}
