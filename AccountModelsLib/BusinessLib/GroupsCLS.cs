using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using Connection4It; 
using Microsoft.EntityFrameworkCore; 
 
 
 
 
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
            string mysql = " Select * from Customer_Supplier_Group Where GroupName='" + GroupName + "' And GroupType="+ GroupType + " ";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Msg="الإسم مستخدم من قبل" ;
                Excp = 1;
                return;
            }
            
            mysql = @"INSERT INTO Customer_Supplier_Group (GroupName,  GroupType ) ";
            mysql = mysql + " VALUES ('" + GroupName + "', " + GroupType + " )";

            opp.insert_me(mysql);
            Excp = 0;

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
                string mysqlDup = " Select * from Customer_Supplier_Group Where GroupName='" + GroupName + "' And GroupType=" + GroupType + " And ID<>" + Id + " ";
                DataTable dt = opp.return_DataTable(mysqlDup);
                if (dt.Rows.Count > 0)
                {
                    Msg = "الإسم مستخدم من قبل";
                    Excp = 1;
                    return;
                }

                string mysql = "Update Customer_Supplier_Group Set GroupName='" + GroupName + "', Edit_User_Id=" + Common.User_Id + ", EditOperationDate='" + AccountModelsLib.operations.dateFormat(DateTime.Now) + "' Where ID=" + Id;
                opp.update_me(mysql);
                Excp = 0;
            }
            catch (Exception exceptionObj)
            {
                Msg = exceptionObj.Message.ToString();
                Excp = 1;
            }
        }
    }
}
