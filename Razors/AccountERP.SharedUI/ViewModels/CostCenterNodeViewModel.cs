using System;
using System.Collections.Generic;
using System.Data;

namespace AccountERP.SharedUI.ViewModels
{
    // 🟢 غلاف Blazor-only فوق صف واحد من DataTable المُعاد من AccountModelsLib.CostCenterCLS.Get_CostCenter()
    // (Self-Join حقيقي على جدول Cost_Center_Tree، Parent_Id يشير لنفس الجدول - نفس مبدأ
    // AccountTreeNodeViewModel لشجرة الحسابات بالضبط، لكن بدون أي فلترة فرع Branch_Id لأن Cost_Center_Tree
    // لا يحتوي هذا العمود إطلاقاً - شجرة مراكز التكلفة واحدة عامة للنظام كله). لا يُستبدَل به أي كتابة فعلية:
    // كل عمليات الإضافة/التعديل/الحذف تمر عبر AccountModelsLib.CostCenterCLS مباشرة بنفس الاستدعاءات
    // المستخدَمة من شاشة الويندوز فورمز (Cost_Center_TreeFrm/Add_NewCostCenter) - هذا النوع لعرض الشجرة فقط.
    public class CostCenterNodeViewModel
    {
        public long Id { get; set; }
        public long Parent_Id { get; set; }
        public string Name { get; set; } = "";
        public long? Code { get; set; }
        public string Code_Str { get; set; } = "";
        public string Notes { get; set; } = "";
        public long    Currency_Id { get; set; }
        public string Currency_Str { get; set; } = "";
        public int Head_Main_Cost { get; set; }
        public long lastPartIncrease { get; set; }
        public int Cost_Level { get; set; }
        public string FirstSubCode { get; set; } = "";
        public long Cost_Type { get; set; }
        public string NameE { get; set; } = "";
        public string Parent_Name { get; set; } = "";
        public string Parent_Code_Str { get; set; } = "";

        public List<CostCenterNodeViewModel> Children { get; } = new();

        public bool HasChildren => Children.Count > 0;

        // 🟢 يبني الشجرة الهرمية دفعة واحدة من كل الصفوف المُحمَّلة معاً - بنفس مبدأ AccountTreeNodeViewModel.
        // BuildTree (Main_Account_Id==0 جذر هناك)، هنا Parent_Id==0 (أو NULL فى القاعدة) هو نفس المعنى.
        public static List<CostCenterNodeViewModel> BuildTree(DataTable dt)
        {
            var byId = new Dictionary<long, CostCenterNodeViewModel>();
            var roots = new List<CostCenterNodeViewModel>();

            foreach (DataRow row in dt.Rows)
            {
                var node = FromRow(row);
                byId[node.Id] = node;
            }

            foreach (var node in byId.Values)
            {
                if (node.Parent_Id == 0 || !byId.TryGetValue(node.Parent_Id, out var parent))
                {
                    roots.Add(node);
                }
                else
                {
                    parent.Children.Add(node);
                }
            }

            SortRecursive(roots);
            return roots;
        }

        private static void SortRecursive(List<CostCenterNodeViewModel> nodes)
        {
            nodes.Sort((a, b) => (a.Code ?? 0).CompareTo(b.Code ?? 0));
            foreach (var n in nodes)
            {
                SortRecursive(n.Children);
            }
        }

        private static CostCenterNodeViewModel FromRow(DataRow row) => new()
        {
            Id = GetLong(row, "Id"),
            Parent_Id = GetLong(row, "Parent_Id"),
            Name = GetString(row, "Name"),
            Code = GetNullableLong(row, "Code"),
            Code_Str = GetString(row, "Code_Str"),
            Notes = GetString(row, "Notes"),
            Currency_Id = GetInt(row, "Currency_Id"),
            Currency_Str = GetString(row, "Currency_Str"),
            Head_Main_Cost = GetInt(row, "Head_Main_Cost"),
            lastPartIncrease = GetLong(row, "lastPartIncrease"),
            Cost_Level = GetInt(row, "Cost_Level"),
            FirstSubCode = GetString(row, "FirstSubCode"),
            Cost_Type = GetLong(row, "Cost_Type"),
            Parent_Name = GetString(row, "Parent_Name"),
            Parent_Code_Str = GetString(row, "Parent_Code_Str"),
        };

        private static string GetString(DataRow row, string column) =>
            row.Table.Columns.Contains(column) && row[column] != DBNull.Value ? row[column].ToString() ?? "" : "";

        private static int GetInt(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return 0;
            return int.TryParse(row[column].ToString(), out var v) ? v : 0;
        }

        private static long GetLong(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return 0;
            return long.TryParse(row[column].ToString(), out var v) ? v : 0;
        }

        private static long? GetNullableLong(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return null;
            return long.TryParse(row[column].ToString(), out var v) ? v : null;
        }
    }
}
