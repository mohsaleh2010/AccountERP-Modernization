using System;
using System.Collections.Generic;
using System.Data;

namespace AccountERP.SharedUI.ViewModels
{
    // 🟢 غلاف Blazor-only فوق صف واحد من DataTable المُعاد من AccountModelsLib.AccountsTree.Get_FixedAsset_Account
    // (نفس شجرة الحسابات العامة Submain_Account.Main_Account_Id ذاتية المرجعية، لكن مُقيَّدة على
    // Account_Customer_Type_Value=8 فقط - نفس شرط Main_FixedAsset_Id/FixedAssetTreeViewUSCTRL الأصلى).
    // العقد هنا يمكن أن تكون "فئة" (Category) أو "أصل فعلى" - التمييز بينهما لا يتم من عمود فى هذا الصف
    // إطلاقاً (كلاهما Account_Customer_Type_Value=8 بالضبط، كما يضبطه Add_Account_Category.ReadData نفسه
    // عمداً) بل فقط بوجود صف FixedAssetData مطابق لهذا الـ SubAccount_Id أم لا - يُتحقَّق منه عند التحديد
    // فقط (FixedAssetCLS.Get_FixedAsset)، تماماً كما يفعل trAccount_Common_LIB_AfterSelect_ الأصلى.
    public class FixedAssetNodeViewModel
    {
        public long Id { get; set; }
        public long Main_Account_Id { get; set; }
        public string SubAccount_NO { get; set; } = "";
        public string SubAccount_Name { get; set; } = "";
        public int Branch_Id { get; set; }
        public int Account_Follows { get; set; }

        public List<FixedAssetNodeViewModel> Children { get; } = new();

        public bool HasChildren => Children.Count > 0;

        public static List<FixedAssetNodeViewModel> BuildTree(DataTable dt)
        {
            var byId = new Dictionary<long, FixedAssetNodeViewModel>();
            var roots = new List<FixedAssetNodeViewModel>();

            foreach (DataRow row in dt.Rows)
            {
                var node = FromRow(row);
                byId[node.Id] = node;
            }

            foreach (var node in byId.Values)
            {
                if (node.Main_Account_Id == 0 || !byId.TryGetValue(node.Main_Account_Id, out var parent))
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

        private static void SortRecursive(List<FixedAssetNodeViewModel> nodes)
        {
            nodes.Sort((a, b) => string.CompareOrdinal(a.SubAccount_NO, b.SubAccount_NO));
            foreach (var n in nodes)
            {
                SortRecursive(n.Children);
            }
        }

        private static FixedAssetNodeViewModel FromRow(DataRow row) => new()
        {
            Id = GetLong(row, "Id"),
            Main_Account_Id = GetLong(row, "Main_Account_Id"),
            SubAccount_NO = GetString(row, "SubAccount_NO"),
            SubAccount_Name = GetString(row, "SubAccount_Name"),
            Branch_Id = GetInt(row, "Branch_Id"),
            Account_Follows = GetInt(row, "Account_Follows"),
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
    }
}
