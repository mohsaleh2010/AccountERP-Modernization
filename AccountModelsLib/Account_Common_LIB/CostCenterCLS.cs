using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
 
using AccountModelsLib; 
using AccountModelsLib.Models;
namespace AccountModelsLib
{
    public class CostCenterCLS
    {
        public CostCenterCLS()
        { }
        static string Esc(string s) => (s ?? "").Replace("'", "''");
        public DataTable Get_CostCenter()
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT DISTINCT Cost_Center_Tree.Code_Str + N'  ' + Cost_Center_Tree.Name AS Name_CodeStr,
                         Cost_Center_Tree.ID, Cost_Center_Tree.Parent_Id, Cost_Center_Tree.Name, Cost_Center_Tree.Code, Cost_Center_Tree.Code_Str, Cost_Center_Tree.Cost_Type, 
                         Parent_Cost_Center_Tree.Name AS Parent_Name, Parent_Cost_Center_Tree.Code AS Parent_Code, Parent_Cost_Center_Tree.Code_Str AS Parent_Code_Str, 
                         Cost_Center_Tree.Notes, Cost_Center_Tree.Currency_Id, Cost_Center_Tree.Currency_Str, Cost_Center_Tree.Head_Main_Cost, Cost_Center_Tree.lastPartIncrease, 
                         Cost_Center_Tree.Cost_Level,Cost_Center_Tree.FirstSubCode
FROM            Cost_Center_Tree LEFT OUTER JOIN
                         Cost_Center_Tree AS Parent_Cost_Center_Tree ON Cost_Center_Tree.Parent_Id = Parent_Cost_Center_Tree.ID
Order By Cost_Center_Tree.Id
";

                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;
        }

        public bool Check_Account_No_Found_OnSubAccount(ref int Excp, long Cost_Id)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            try
            {
                string mysql;
                mysql = "Select * from Cost_Center_Tree  where Parent_Id=" + Cost_Id;
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 0;
                    opp = null;
                    return true;
                }
                else
                {
                    Excp = 0;
                    opp = null;
                    return false;
                }
            }
            catch
            {
                Excp = 0;
                opp = null;
                return true;
            }

        }
        public bool Check_CostCode_Found(ref int Excp, ref AccountModelsLib.operations opp, string Code_Str)
        {
            DataTable dt;
            try
            {
                string mysql;
                mysql = "Select * from Cost_Center_Tree where Code_Str= '" + Code_Str + "'";

                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 0;

                    return true;
                }
                else
                {
                    Excp = 0;

                    return false;
                }


            }
            catch
            {
                Excp = 1;
                return true;
            }
        }

        public bool Check_CostName_Found(ref int Excp, ref AccountModelsLib.operations opp, long CostCenter_Id, ref string CostCenter_Name)
        {
            DataTable dt;
            try
            {
                string mysql;
                mysql = "Select * from Cost_Center_Tree where Id= '" + CostCenter_Id + "'";

                dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 0;
                    CostCenter_Name = dt.Rows[0]["Name"].ToString();


                    return true;
                }
                else
                {
                    Excp = 0;

                    return false;
                }


            }
            catch
            {
                Excp = 1;
                return true;
            }
        }

        public void Add_NewCostCenter(ref int Excp, ref string Msg,ref operations opp, long Parent_Id, string Name, long Code, string Code_Str, int Cost_Type, string Notes, long Currency_Id, string Currency_Str, int Head_Main_Cost, long lastPartIncrease, int Cost_Level, string FirstSubCode)
        {
            string mysql = "";
            if (Common.Use_AutoNumeric_Submain_Account_Id == 0 | Common.Use_Auto_Identity_Column == 0)
            {
                long Id = 0;
                Id = Common.GETAutoNumber(ref opp, "Cost_Center_Tree", "Id", "");

                mysql = "INSERT INTO Cost_Center_Tree( Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode) ";
                mysql = mysql + " VALUES        (" + Id + ", " + Parent_Id + ",'" + Esc(Name) + "'," + Code + ",'" + Code_Str + "'," + Cost_Type + ",'" + Esc(Notes) + "'," + Currency_Id + ",'" + Currency_Str + "'," + Head_Main_Cost + "," + lastPartIncrease + "," + Cost_Level + ",'" + FirstSubCode + "')";
            }
            else
            {
                mysql = "INSERT INTO Cost_Center_Tree(   Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode) ";
                mysql = mysql + " VALUES        ( " + Parent_Id + ",'" + Esc(Name) + "'," + Code + ",'" + Code_Str + "'," + Cost_Type + ",'" + Esc(Notes) + "'," + Currency_Id + ",'" + Currency_Str + "'," + Head_Main_Cost + "," + lastPartIncrease + "," + Cost_Level + ",'" + FirstSubCode + "')";

            }
            try
            {

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void Add_NewCostCenter(ref int Excp, ref string Msg, long Parent_Id, string Name, long Code, string Code_Str, int Cost_Type, string Notes, long Currency_Id, string Currency_Str, int Head_Main_Cost, long lastPartIncrease, int Cost_Level, string FirstSubCode)
        {
           
            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            Add_NewCostCenter(ref   Excp, ref   Msg, ref   opp,   Parent_Id,   Name,   Code,   Code_Str,   Cost_Type,   Notes,   Currency_Id,   Currency_Str,   Head_Main_Cost,   lastPartIncrease,   Cost_Level,   FirstSubCode);
            

        }
        public void Edit_CostCenter(ref int Excp, ref string Msg, long Id, string Name, string Code_Str, string Notes)
        {
            // لا نكتب Code: الكود الرقمى مشتق من Code_Str عند الإنشاء/النقل وكان التعديل يمحوه (عيب 1 فى خطة مراكز التكلفة).
            string mysql = " ";
            mysql = " Update Cost_Center_Tree Set Name='" + Esc(Name) + "', Code_Str='" + Esc(Code_Str) + "',Notes='" + Esc(Notes) + "', Edit_User_Id=" + Common.User_Id + ", EditOperationDate='" + AccountModelsLib.operations.dateFormat(DateTime.Now) + "'   ";
            mysql = mysql + " Where Id=" + Id;
            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void Delete_CostCenter(ref int Excp, ref string Msg, long Id)
        {

            string mysql = " ";
            mysql = " Delete From  Cost_Center_Tree Where Id=" + Id;

            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                // منع حذف المستخدم (أبناء/حركات/ربط) بدل كسر المراجع؛ البديل هو الإغلاق (CostCenterService.SetStatus).
                var usage = AccountModelsLib.BusinessLibShared.CostCenters.CostCenterService.GetUsage(opp, Id);
                if (usage.InUse)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف مركز التكلفة لأنه مستخدم: " + usage.Describe();
                    return;
                }
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }

        public void Add_NewCostCenterForAccounts(ref int Excp, ref string Msg, long Cost_Center_Id, long Submain_Account_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                string mysql = " SELECT  * FROM   Cost_Center_ForAccounts ";
                mysql = mysql + " WHERE  (Cost_Center_Id = " + Cost_Center_Id + ") AND (Submain_Account_Id = " + Submain_Account_Id + ")";
                DataTable dt = opp.return_DataTable(mysql);
                if (dt.Rows.Count > 0)
                {
                    Excp = 1;
                    Msg = "تم إضافة مركز التكلفه للحساب من قبل";
                    opp.Rollback();
                    return;
                }
                 SaveCost_Center_ForAccounts(ref opp, Cost_Center_Id,   Submain_Account_Id);

                
                Excp = 0;
                Msg = "تمت الإضافه بنجاح";
                opp.Commit();
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }

        }

        void   SaveCost_Center_ForAccounts(ref AccountModelsLib.operations opp, long Cost_Center_Id, long Submain_Account_Id)
        {
            string mysql = "";
            if (Common.Use_AutoNumeric_Submain_Account_Id == 0 | Common.Use_Auto_Identity_Column == 0) 
            {
                long Id = 0;
                Id = Common.GETAutoNumber(ref opp, "Cost_Center_ForAccounts", "Id", "");

                mysql = " INSERT INTO Cost_Center_ForAccounts (Id,Cost_Center_Id, Submain_Account_Id) ";
                mysql = mysql + " VALUES  (" + Id + "," + Cost_Center_Id + "," + Submain_Account_Id + ") ";
            }
            else
            {
                mysql = " INSERT INTO Cost_Center_ForAccounts (Cost_Center_Id, Submain_Account_Id) ";
                mysql = mysql + " VALUES  (" + Cost_Center_Id + "," + Submain_Account_Id + ") ";
            }

            opp.insert_me(mysql);
        }
        public void Add_NewCostCenterForAccounts(ref int Excp, ref string Msg, ArrayList Arr_Cost_Center_Id, long Submain_Account_Id)
        {
            Excp = 1;
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                for (int i = 0; i < Arr_Cost_Center_Id.Count; i++)
                {


                    string mysql = " SELECT  * FROM   Cost_Center_ForAccounts ";
                    mysql = mysql + " WHERE  (Cost_Center_Id = " + Arr_Cost_Center_Id[i] + ") AND (Submain_Account_Id = " + Submain_Account_Id + ")";
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {

                        continue;
                    }

                    //mysql = " INSERT INTO Cost_Center_ForAccounts (Cost_Center_Id, Submain_Account_Id) ";
                    //mysql = mysql + " VALUES  (" + Arr_Cost_Center_Id[i] + "," + Submain_Account_Id + ") ";
                    SaveCost_Center_ForAccounts(ref opp,long.Parse( Arr_Cost_Center_Id[i].ToString()), Submain_Account_Id);

                    //opp.insert_me(mysql);
                    Excp = 0;

                }


                opp.Commit();
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = -1;
                Msg = ee.Message;
            }

        }
        public void Delete_CostCenterForAccounts(long Cost_Center_Id, long Submain_Account_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            string mysql = " Delete  FROM   Cost_Center_ForAccounts ";
            mysql = mysql + " WHERE  (Cost_Center_Id = " + Cost_Center_Id + ") AND (Submain_Account_Id = " + Submain_Account_Id + ")";
            opp.insert_me(mysql);




        }
        public DataTable Get_CostAssignForCost(long Submain_Account_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            string mysql = @" SELECT  distinct Cost_Center_Tree.*,  Cost_Center_ForAccounts.Id AS Cost_Center_ForAccounts_Id, Cost_Center_ForAccounts.Cost_Center_Id, Cost_Center_ForAccounts.Submain_Account_Id
FROM            Cost_Center_ForAccounts INNER JOIN
                         Cost_Center_Tree ON Cost_Center_ForAccounts.Cost_Center_Id = Cost_Center_Tree.ID ";
            //           string mysql = @"SELECT DISTINCT 
            //                         Cost_Center_ForAccounts.Id AS Cost_Center_ForAccounts_Id, Cost_Center_ForAccounts.Cost_Center_Id, Cost_Center_Tree.Name, 
            //                         Cost_Center_ForAccounts.Submain_Account_Id, Cost_Center_Tree.Id
            //
            //FROM            Cost_Center_ForAccounts INNER JOIN
            //                         Cost_Center_Tree ON Cost_Center_ForAccounts.Cost_Center_Id = Cost_Center_Tree.Id";
            mysql = mysql + " WHERE    (Cost_Center_ForAccounts.Submain_Account_Id = " + Submain_Account_Id + ")";
            mysql = mysql + " ORDER BY Cost_Center_Tree.ID ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;

        }
    }
}
