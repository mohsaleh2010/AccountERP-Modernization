using AccountModelsLib.Models;
using Connection4It; 
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore; 
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
 
 
 
 
namespace AccountModelsLib
{
    public class GroupsCLS
    {
        public GroupsCLS()
        {
        }

        
        public DataTable Get_Customer_Supplier_Group(ref AccountModelsLib.operations opp,int GroupType)
        {
            opp = new AccountModelsLib.operations();
            string mysql = "Select * from Customer_Supplier_Group Where  GroupType=" + GroupType + "  Order By GroupName ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;
        }
        public void Add_Group(ref int Excp,ref string Msg, string GroupName, int GroupType)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            try
            {
                GroupName = (GroupName ?? "").Trim();
                // فحص التكرار عبر EF حتى لا يكسر اسم به علامة ' الاستعلام
                if (opp.db.Customer_Supplier_Group.Any(i => i.GroupName == GroupName && i.GroupType == GroupType))
                {
                    Msg = "الإسم مستخدم من قبل";
                    Excp = 1;
                    return;
                }

                // ID ليس Identity، فكان INSERT القديم بدونه يفشل دائماً (Cannot insert NULL into column 'ID')
                DateTime now = DateTime.Now;
                opp.db.Customer_Supplier_Group.Add(new Customer_Supplier_Group
                {
                    ID = CallSnowflakeIdGenerate.GenerateNewId(),
                    GroupName = GroupName,
                    GroupType = GroupType,
                    User_Id = Common.User_Id,
                    Branch_Id = Common.Branch_Id,
                    OperationDate = now,
                    Insert_ServerDateTime = now
                });
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception exceptionObj)
            {
                Msg = exceptionObj.InnerException?.Message ?? exceptionObj.Message;
                Excp = 1;
            }
        }

        public void Delete_Customer_Supplier_Group(ref int Excp,ref string Msg, string Value)
        {

             AccountModelsLib.operations opp = new AccountModelsLib.operations();

            try
            {
                string mysqlInUse = "Select ID From Customer Where Group_Id=" + Value +
                    " Union All Select ID From Supplier Where Group_Id=" + Value;
                DataTable dtInUse = opp.return_DataTable(mysqlInUse);
                if (dtInUse.Rows.Count > 0)
                {
                    Msg = "لا يمكن حذف المجموعة لوجود عملاء/موردين مرتبطين بها";
                    Excp = 1;
                    return;
                }

                string mysql = @" Delete From Customer_Supplier_Group Where ID=" + Value + "";

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception exceptionObj)
            {
                Msg = exceptionObj.Message.ToString();
                Excp = 1;
            }

        }

        public void Update_Group(ref int Excp, ref string Msg, string Id, string GroupName, int GroupType)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            try
            {
                long.TryParse(Id, out long Id_);
                GroupName = (GroupName ?? "").Trim();
                // فحص التكرار عبر EF حتى لا يكسر اسم به علامة ' الاستعلام
                bool isDup = opp.db.Customer_Supplier_Group.Any(i => i.GroupName == GroupName && i.GroupType == GroupType && i.ID != Id_);
                if (isDup)
                {
                    Msg = "الإسم مستخدم من قبل";
                    Excp = 1;
                    return;
                }
                var csg = opp.db.Customer_Supplier_Group.Where(i => i.ID == Id_).FirstOrDefault();

                if (csg == null)
                {
                    Msg = "المجموعة غير موجودة";
                    Excp = 1;
                }
                else
                {
                    //string mysql = "Update Customer_Supplier_Group Set GroupName='" + GroupName + "', Edit_User_Id=" + Common.User_Id + ", EditOperationDate='" + AccountModelsLib.operations.dateFormat(DateTime.Now.Date) + "' Where ID=" + Id;
                    //opp.insert_me(mysql);

                    csg.GroupName = GroupName;
                    csg.Edit_User_Id = Common.User_Id;
                    csg.EditOperationDate = DateTime.Now;

                    opp.db.SaveChanges();
                    Excp = 0;
                }
            }
            catch (Exception exceptionObj)
            {
                Msg = exceptionObj.Message.ToString();
                Excp = 1;
            }
        }
    }
}
