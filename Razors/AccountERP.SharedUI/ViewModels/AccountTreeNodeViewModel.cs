using System;
using System.Collections.Generic;
using System.Data;

namespace AccountERP.SharedUI.ViewModels
{
    // 🟢 غلاف Blazor-only فوق صف واحد من DataTable المُعاد من AccountModelsLib.AccountsTree.Get_SubMain_Account(...)
    // (استعلام Self-Join حقيقي على جدول Submain_Account، Main_Account_Id يشير لنفس الجدول - راجع
    // AccountsTree.cs/Submain_AccountCLS.Get_SQL_AccountsCode فى AccountModelsLib). لا يُستبدَل به الكيان
    // الحقيقي AccountModelsLib.Models.Submain_Account فى أي عملية حفظ/تعديل/حذف - هذا النوع لعرض الشجرة فقط،
    // كل عمليات الكتابة تمر عبر Submain_AccountCLS.sb (نفس الكيان الحقيقي) تماماً كما تفعل شاشة الويندوز فورمز.
    public class AccountTreeNodeViewModel
    {
        public long Id { get; set; }
        public long Main_Account_Id { get; set; }
        public string SubAccount_Name { get; set; } = "";
        public string SubAccount_NO { get; set; } = "";
        public int Head_Main_Account { get; set; }
        public int Account_Nature_Type { get; set; }
        public long? Currency_Id { get; set; }
        public string Notes { get; set; } = "";
        public int Account_Follows { get; set; }
        public int CashFlowType { get; set; }
        public int Account_Customer_Type_Value { get; set; }
        public long? Account_Customer_Type_ID { get; set; }
        public int IS_Last_Level { get; set; }
        public int Accept_Cost_Center_Flage { get; set; }
        public int Accept_EntryNew_Flage { get; set; }
        public int Is_MasrofatAffect { get; set; }
        public int IS_SubAccount_AccountType { get; set; }
        public int IS_SubAccount_FollowingAccount { get; set; }
        public string FirstSubCode { get; set; } = "";
        public long? Code { get; set; }
        public int Account_Level { get; set; }
        public long Branch_Id { get; set; }
        public int CanDelete { get; set; }
        public int Account_Type_ValueGeneralCustomer { get; set; }
        public long Cost_Center_Tree_Id { get; set; }

        public List<AccountTreeNodeViewModel> Children { get; } = new();

        public bool HasChildren => Children.Count > 0;

        // 🟢 يبني الشجرة الهرمية دفعة واحدة من كل صفوف الفرع (Branch) المُحمَّلة معاً - بنفس مبدأ MenuViewModel
        // (ParentId==0 جذر) المستخدَم بالفعل لشجرة القوائم الجانبية، هنا Main_Account_Id==0 هو نفس المعنى.
        public static List<AccountTreeNodeViewModel> BuildTree(DataTable dt)
        {
            var byId = new Dictionary<long, AccountTreeNodeViewModel>();
            var roots = new List<AccountTreeNodeViewModel>();

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

        private static void SortRecursive(List<AccountTreeNodeViewModel> nodes)
        {
            nodes.Sort((a, b) => (a.Code ?? 0).CompareTo(b.Code ?? 0));
            foreach (var n in nodes)
            {
                SortRecursive(n.Children);
            }
        }

        private static AccountTreeNodeViewModel FromRow(DataRow row) => new()
        {
            Id = GetLong(row, "Id"),
            Main_Account_Id = GetLong(row, "Main_Account_Id"),
            SubAccount_Name = GetString(row, "SubAccount_Name"),
            SubAccount_NO = GetString(row, "SubAccount_NO"),
            Head_Main_Account = GetInt(row, "Head_Main_Account"),
            Account_Nature_Type = GetInt(row, "Account_Nature_Type"),
            Currency_Id = GetNullableInt(row, "Currency_Id"),
            Notes = GetString(row, "Notes"),
            Account_Follows = GetInt(row, "Account_Follows"),
            CashFlowType = GetInt(row, "CashFlowType"),
            Account_Customer_Type_Value = GetInt(row, "Account_Customer_Type_Value"),
            Account_Customer_Type_ID = GetNullableLong(row, "Account_Customer_Type_ID"),
            IS_Last_Level = GetInt(row, "IS_Last_Level"),
            Accept_Cost_Center_Flage = GetInt(row, "Accept_Cost_Center_Flage"),
            Accept_EntryNew_Flage = GetInt(row, "Accept_EntryNew_Flage"),
            Is_MasrofatAffect = GetInt(row, "Is_MasrofatAffect"),
            IS_SubAccount_AccountType = GetInt(row, "IS_SubAccount_AccountType"),
            IS_SubAccount_FollowingAccount = GetInt(row, "IS_SubAccount_FollowingAccount"),
            FirstSubCode = GetString(row, "FirstSubCode"),
            Code = GetNullableLong(row, "Code"),
            Account_Level = GetInt(row, "Account_Level"),
            Branch_Id = GetLong(row, "Branch_Id"),
            CanDelete = GetInt(row, "CanDelete"),
            Account_Type_ValueGeneralCustomer = GetInt(row, "Account_Type_ValueGeneralCustomer"),
            Cost_Center_Tree_Id = GetLong(row, "Cost_Center_Tree_ID"),
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

        private static int? GetNullableInt(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return null;
            return int.TryParse(row[column].ToString(), out var v) ? v : null;
        }

        private static long? GetNullableLong(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value) return null;
            return long.TryParse(row[column].ToString(), out var v) ? v : null;
        }
    }
}
