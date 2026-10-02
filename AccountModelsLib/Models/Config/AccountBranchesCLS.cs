using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AccountModelsLib.Models
{
    /// <summary>صف قائمة فروع الشركة (يقابل أعمدة BranchesListFrm: الكود، الفرع، عملة الفرع).</summary>
    public class BranchRow
    {
        public long Id { get; set; }
        public long Code { get; set; }
        public string Branch_Name { get; set; } = "";
        public string Address { get; set; } = "";
        public long Currency_Id { get; set; }
        public string Currency_Name { get; set; } = "";
    }

    /// <summary>
    /// 🟢 كود أعمال فروع الشركة لواجهة Razor (بطلب المستخدم: فى AccountModelsLib). يقابل General_Ledger.BranchesListCLS (مشروع WinForms، ويعرض MessageBox
    /// من داخل الكلاس) بنفس المنطق تماماً لكن يُرجع الرسائل عبر Msg بدل MessageBox: الإضافة تفحص تكرار الكود ثم تُدرج الفرع (Id = Code) وتنشئ له حساباً فرعياً
    /// ومركز تكلفة فى معاملة واحدة، والحذف يُمنع لو للفرع قيود. الكلاس الأصلى لم يُعدَّل.
    /// </summary>
    public class AccountBranchesCLS
    {
        public List<BranchRow> GetBranches()
        {
            var opp = new operations();
            var dt = opp.return_DataTable(
                "SELECT Id, ISNULL(Code, 0) AS Code, ISNULL(Branch_Name, '') AS Branch_Name, ISNULL(Address, '') AS Address, ISNULL(Currency_Id, 0) AS Currency_Id, ISNULL(Currency_Name, '') AS Currency_Name FROM Account_Branches ORDER BY Code");
            return dt.Rows.Cast<DataRow>().Select(r => new BranchRow
            {
                Id = Convert.ToInt64(r["Id"]),
                Code = Convert.ToInt64(r["Code"]),
                Branch_Name = r["Branch_Name"]?.ToString() ?? "",
                Address = r["Address"]?.ToString() ?? "",
                Currency_Id = Convert.ToInt64(r["Currency_Id"]),
                Currency_Name = r["Currency_Name"]?.ToString() ?? "",
            }).ToList();
        }

        /// <summary>
        /// يقابل BranchesListCLS.Add_Account_Branches (+ حقل العنوان، وUser_Id/Branch_Id للمستخدم الحالى عند الإضافة - غير موجودين فى الأصل رغم وجود
        /// أعمدتهما فى الجدول، بطلب المستخدم أن تُحفَظ هذه الأعمدة فى كل شاشة). Excp: 0 نجاح، 1 الكود مستخدم، 2 خطأ (Msg بنص الخطأ).
        /// </summary>
        public void AddBranch(ref int Excp, ref string Msg, long Code, string Branch_Name, string Address, long Currency_Id, string Currency_Name)
        {
            var opp = new operations(1);
            try
            {
                var existing = opp.return_DataTable("Select * from Account_Branches Where Code=" + Code);
                if (existing.Rows.Count > 0)
                {
                    opp.Rollback();
                    Excp = 1;
                    Msg = "الكود مستخدم من قبل";
                    return;
                }

                string name = (Branch_Name ?? "").Replace("'", "''");
                string address = (Address ?? "").Replace("'", "''");
                string currencyName = (Currency_Name ?? "").Replace("'", "''");
                opp.insert_me(" INSERT INTO Account_Branches (Id,Branch_Name,Address,Code,Currency_Id,Currency_Name,User_Id,Branch_Id,OperationDate,Insert_ServerDateTime,Update_ServerDateTime) VALUES  (" +
                    Code + ",N'" + name + "',N'" + address + "'," + Code + "," + Currency_Id + ",N'" + currencyName + "'," + Common.User_Id + "," + Common.Branch_Id + ",GETDATE(),GETDATE(),GETDATE()) ");

                var submain = new Submain_AccountCLS();
                submain.sb = new Submain_Account
                {
                    SubAccount_Name = Branch_Name,
                    Currency_Id = Currency_Id,
                    Branch_Id = Code,
                    FirstSubCode = "0",
                    Code = 0,
                    lastPartIncrease = 0,
                    SubAccount_NO = " ",
                    Account_Code = "0",
                    Head_Main_Account = 0,
                    Account_Follows = 0,
                    Notes = " ",
                    Account_Customer_Type_Value = 0,
                    Account_Level = 0,
                    Account_Nature_Type = 0,
                    Accept_Cost_Center_Flage = 0,
                };
                submain.InsertSubmain_Account(ref opp);

                int excpCost = 0;
                string msgCost = "";
                new CostCenterCLS().Add_NewCostCenter(ref excpCost, ref msgCost, ref opp, 0, Branch_Name, 0, Code.ToString(), 0, "", Currency_Id, "", 0, 0, 1, "1");

                opp.Commit();
                Excp = 0;
            }
            catch (Exception ee)
            {
                try { opp.Rollback(); } catch { }
                Excp = 2;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>
        /// تعديل اسم الفرع وكوده وعنوانه (الأصل يحدّث الجدول كله بـDataAdapter من الشبكة القابلة للتحرير، بلا عنوان ولا Edit_User_Id - بطلب المستخدم تُحفَظ
        /// هذه الأعمدة الآن فى كل شاشة عند التعديل).
        /// </summary>
        public void UpdateBranch(ref int Excp, ref string Msg, long Id, string Branch_Name, string Address, long Code)
        {
            try
            {
                var opp = new operations();
                var dup = opp.return_DataTable("Select Id from Account_Branches Where Code=" + Code + " AND Id<>" + Id);
                if (dup.Rows.Count > 0)
                {
                    Excp = 1;
                    Msg = "الكود مستخدم من قبل";
                    return;
                }
                string name = (Branch_Name ?? "").Replace("'", "''");
                string address = (Address ?? "").Replace("'", "''");
                opp.update_me("UPDATE Account_Branches SET Branch_Name = N'" + name + "', Address = N'" + address + "', Code = " + Code +
                    ", Edit_User_Id = " + Common.User_Id + ", EditOperationDate = GETDATE(), Update_ServerDateTime = GETDATE() WHERE Id = " + Id);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 2;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>يقابل BranchesListCLS.Delete_Account_Branches. Excp: 0 نجاح، 1 لا يمكن الحذف لوجود قيود، 2 خطأ.</summary>
        public void DeleteBranch(ref int Excp, ref string Msg, long Id)
        {
            try
            {
                var opp = new operations();
                var entries = opp.return_DataTable("Select TOP 1 Id from Account_Entry_List Where Branch_ID_Login=" + Id);
                if (entries.Rows.Count > 0)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف الفرع نظرا لإستخدامه فى القيود";
                    return;
                }
                opp.insert_me(" Delete From Account_Branches Where Id=" + Id);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 2;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }
    }
}
