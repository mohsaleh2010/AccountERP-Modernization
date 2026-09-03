using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AccountModelsLib.Models
{
    public class Checks_MemoryCLS
    {
        public Checks_MemoryCLS()
        {

        }
        public Checks_Memory cm = new Checks_Memory();
        //public void InsertChecks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp)
        //{
        //    try
        //    {
        //        string mysql = " ";
        //        if (Common.Use_Auto_Identity_Column == 0)
        //        {
        //            long Id_ = 0;
        //            Id_ = Common.GetTable_Id(ref opp, "Checks_Memory", "Id");

        //            mysql = " INSERT INTO Checks_Memory (Id,Order_Mony_Id, Check_No, Day_Date, Due_Date, Bank_id,Bank_Balance, Check_Value, Check_Value_,Import_Value,Export_Value, Customer_Id, Supplier_Id, Account_Id,MainSubAccount_ID, Due_Check, Transaction_Type, Exchanger_Name, First_Drawer_Name, Last_Drawer_Name,Check_Notes, Balance_Before,Balance_After,Balance_BeginingPeriod,Bank_Name, Transfer_Check_Id,Transfer_Check_Id_From,SubAccount_Id,SubAccount_Name,Receipt,Factor, IS_Posted_Flage ,Currency_Id,Account_Entry_List_ID,  SubAccount_Id_RelatedTo,SubAccount_Name_RelatedTo,SubAccount_Id_RelatedToBefor,SubAccount_Name_RelatedToBefor, IsAllowEdit,IsAllowDelete,SubAccount_Id_RelatedFrom,SubAccount_Name_RelatedFrom) ";
        //            mysql = mysql + " VALUES (" + Id_ + "," + Order_Mony_Id + ",'" + Check_No + "','" + AccountModelsLib.operations.dateFormat(Day_Date) + "','" + AccountModelsLib.operations.dateFormat(Due_Date) + "'," + Bank_id + "," + Bank_Balance + "," + Check_Value + ",'" + Check_Value_ + "'," + Import_Value + "," + Export_Value + "," + Customer_Id + "," + Supplier_Id + "," + SubAccount_Id + "," + MainSubAccount_ID + ",'" + Due_Check + "'," + Transaction_Type + ",'" + Exchanger_Name + "','" + First_Drawer_Name + "','" + Last_Drawer_Name + "','" + Check_Notes + "'," + Balance_Before + "," + Balance_After + "," + Balance_BeginingPeriod + ",'" + Bank_Name + "'," + Transfer_Check_Id + "," + Transfer_Check_Id + "," + SubAccount_Id + ",'" + SubAccount_Name + "'," + Receipt + "," + Factor + "," + IS_Posted_Flage + " ," + Currency_Id + "," + Account_Entry_List_ID + "," + SubAccount_Id_RelatedTo + ",'" + SubAccount_Name_RelatedTo + "'," + SubAccount_Id_RelatedToBefor + ",'" + SubAccount_Name_RelatedToBefor + "'," + IsAllowEdit + "," + IsAllowDelete + "," + SubAccount_Id_RelatedFrom + ",'" + SubAccount_Name_RelatedFrom + "') ";

        //            SetParameters(ref opp);

        //            opp.insert_me(mysql);
        //        }
        //        else
        //        {


        //            mysql = " INSERT INTO Checks_Memory (Order_Mony_Id, Check_No, Day_Date, Due_Date, Bank_id,Bank_Balance, Check_Value, Check_Value_,Import_Value,Export_Value, Customer_Id, Supplier_Id, Account_Id,MainSubAccount_ID, Due_Check, Transaction_Type, Exchanger_Name, First_Drawer_Name, Last_Drawer_Name,Check_Notes, Balance_Before,Balance_After,Balance_BeginingPeriod,Bank_Name, Transfer_Check_Id,Transfer_Check_Id_From,SubAccount_Id,SubAccount_Name,Receipt,Factor, IS_Posted_Flage ,Currency_Id,Account_Entry_List_ID,  SubAccount_Id_RelatedTo,SubAccount_Name_RelatedTo,SubAccount_Id_RelatedToBefor,SubAccount_Name_RelatedToBefor, IsAllowEdit,IsAllowDelete,SubAccount_Id_RelatedFrom,SubAccount_Name_RelatedFrom) ";
        //            mysql = mysql + " VALUES (" + Order_Mony_Id + ",'" + Check_No + "','" + AccountModelsLib.operations.dateFormat(Day_Date) + "','" + AccountModelsLib.operations.dateFormat(Due_Date) + "'," + Bank_id + "," + Bank_Balance + "," + Check_Value + ",'" + Check_Value_ + "'," + Import_Value + "," + Export_Value + "," + Customer_Id + "," + Supplier_Id + "," + SubAccount_Id + "," + MainSubAccount_ID + ",'" + Due_Check + "'," + Transaction_Type + ",'" + Exchanger_Name + "','" + First_Drawer_Name + "','" + Last_Drawer_Name + "','" + Check_Notes + "'," + Balance_Before + "," + Balance_After + "," + Balance_BeginingPeriod + ",'" + Bank_Name + "'," + Transfer_Check_Id + "," + Transfer_Check_Id + "," + SubAccount_Id + ",'" + SubAccount_Name + "'," + Receipt + "," + Factor + "," + IS_Posted_Flage + " ," + Currency_Id + "," + Account_Entry_List_ID + "," + SubAccount_Id_RelatedTo + ",'" + SubAccount_Name_RelatedTo + "'," + SubAccount_Id_RelatedToBefor + ",'" + SubAccount_Name_RelatedToBefor + "'," + IsAllowEdit + "," + IsAllowDelete + ", " + SubAccount_Id_RelatedFrom + ",'" + SubAccount_Name_RelatedFrom + "') ";

        //            SetParameters(ref opp);

        //            Id = opp.Insert_me_Return_Last_Identify(mysql);
        //            Excp = opp.Excp;
        //        }


        //        opp.cmd.Parameters.Clear();
        //    }
        //    catch (Exception ee)
        //    {
        //        Msg = ee.Message;
        //        Excp = 1;
        //        opp.cmd.Parameters.Clear();
        //    }


        //}

        public long InsertChecks_MemoryMainDataList(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Checks_MemoryMainDataList chm)
        {
            long Id_ = 0;
            try
            {
                string mysql = " ";

                if (Common.Use_Auto_Identity_Column == 0)
                {

                    Id_ = Common.GetTable_Id(ref opp, "Checks_MemoryMainDataList", "Id");
                    mysql = @" INSERT INTO[dbo].[Checks_MemoryMainDataList](Id ,[Check_No],[Day_Date],[Due_Date],[Bank_id],[Check_Value],[Check_Value_],[Due_Check],[Exchanger_Name],[First_Drawer_Name],[Last_Drawer_Name],[Bank_Name],[Check_Notes])";
                    mysql = mysql + @" VALUES (" + Id_ + ", '" + chm.Check_No + "','" + AccountModelsLib.operations.dateFormat(chm.Day_Date) + "','" + AccountModelsLib.operations.dateFormat(chm.Due_Date) + "' ," + chm.Bank_id + ", " + chm.Check_Value + ",'" + chm.Check_Value_ + "'    , '" + chm.Due_Check + "', '" + chm.Exchanger_Name + "','" + chm.First_Drawer_Name + "' ,'" + chm.Last_Drawer_Name + "'   ,'" + chm.Bank_Name + "'  ,'" + chm.Check_Notes + "' ) ";

                    opp.insert_me(mysql);
                }
                else
                {


                    mysql = @" INSERT INTO[dbo].[Checks_MemoryMainDataList]( [Check_No],[Day_Date],[Due_Date],[Bank_id],[Check_Value],[Check_Value_],[Due_Check],[Exchanger_Name],[First_Drawer_Name],[Last_Drawer_Name],[Bank_Name],[Check_Notes])";
                    mysql = mysql + @" VALUES (  '" + chm.Check_No + "','" + AccountModelsLib.operations.dateFormat(chm.Day_Date) + "','" + AccountModelsLib.operations.dateFormat(chm.Due_Date) + "' ," + chm.Bank_id + ", " + chm.Check_Value + ",'" + chm.Check_Value_ + "'    , '" + chm.Due_Check + "', '" + chm.Exchanger_Name + "','" + chm.First_Drawer_Name + "' ,'" + chm.Last_Drawer_Name + "'   ,'" + chm.Bank_Name + "'  ,'" + chm.Check_Notes + "' ) ";

                    Id_ = opp.Insert_me_Return_Last_Identify(mysql);
                    Excp = opp.Excp;
                }


                opp.cmd.Parameters.Clear();

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }

            return Id_;
        }

        public void InsertChecks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Checks_Memory chm, Checks_MemoryMainDataList chmlist)
        {
            chmlist.Id = InsertChecks_MemoryMainDataList(ref Excp, ref Msg, ref opp, chmlist);

            long ProcessNo = DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp, ref Msg, ref opp, chm);
            try
            {
                string mysql = " ";
                if (Common.Use_Auto_Identity_Column == 0)
                {
                    long Id_ = 0;
                    Id_ = Common.GetTable_Id(ref opp, "Checks_Memory", "Id");

                    mysql = " INSERT INTO Checks_Memory (Id,ProcessNo,Order_Mony_Id,Checks_MemoryMainDataListId, Check_No, Day_Date, Due_Date, Bank_id,Bank_Balance, Check_Value, Check_Value_,Import_Value,Export_Value, Customer_Id, Supplier_Id, Account_Id,MainSubAccount_ID, Due_Check, Transaction_Type, Exchanger_Name, First_Drawer_Name, Last_Drawer_Name,Check_Notes, Balance_Before,Balance_After,Balance_BeginingPeriod,Bank_Name, Transfer_Check_Id,Transfer_Check_Id_From,SubAccount_Id,SubAccount_Name,Receipt,Factor, IS_Posted_Flage ,Currency_Id,Account_Entry_List_ID,  SubAccount_Id_RelatedTo,SubAccount_Name_RelatedTo,SubAccount_Id_RelatedToBefor,SubAccount_Name_RelatedToBefor, IsAllowEdit,IsAllowDelete,SubAccount_Id_RelatedFrom,SubAccount_Name_RelatedFrom) ";
                    mysql = mysql + " VALUES (" + Id_ + "," + ProcessNo + "," + chm.Order_Mony_Id + "," + chmlist.Id + ",'" + chm.Check_No + "','" + AccountModelsLib.operations.dateFormat(chm.Day_Date) + "','" + AccountModelsLib.operations.dateFormat(chm.Due_Date) + "'," + chm.Bank_id + "," + chm.Bank_Balance + "," + chm.Check_Value + ",'" + chm.Check_Value_ + "'," + chm.Import_Value + "," + chm.Export_Value + "," + chm.Customer_Id + "," + chm.Supplier_Id + "," + chm.SubAccount_Id + "," + chm.MainSubAccount_ID + ",'" + chm.Due_Check + "'," + chm.Transaction_Type + ",'" + chm.Exchanger_Name + "','" + chm.First_Drawer_Name + "','" + chm.Last_Drawer_Name + "','" + chm.Check_Notes + "'," + chm.Balance_Before + "," + chm.Balance_After + "," + chm.Balance_BeginingPeriod + ",'" + chm.Bank_Name + "'," + chm.Transfer_Check_Id + "," + chm.Transfer_Check_Id + "," + chm.SubAccount_Id + ",'" + chm.SubAccount_Name + "'," + chm.Receipt + "," + chm.Factor + "," + chm.IS_Posted_Flage + " ," + chm.Currency_Id + "," + chm.Account_Entry_List_ID + "," + chm.SubAccount_Id_RelatedTo + ",'" + chm.SubAccount_Name_RelatedTo + "'," + chm.SubAccount_Id_RelatedToBefor + ",'" + chm.SubAccount_Name_RelatedToBefor + "'," + chm.IsAllowEdit + "," + chm.IsAllowDelete + "," + chm.SubAccount_Id_RelatedFrom + ",'" + chm.SubAccount_Name_RelatedFrom + "') ";

                    SetParameters(ref opp);

                    opp.insert_me(mysql);
                }
                else
                {


                    mysql = " INSERT INTO Checks_Memory (Order_Mony_Id, ProcessNo, Checks_MemoryMainDataListId  , Check_No, Day_Date, Due_Date, Bank_id,Bank_Balance, Check_Value, Check_Value_,Import_Value,Export_Value, Customer_Id, Supplier_Id, Account_Id,MainSubAccount_ID, Due_Check, Transaction_Type, Exchanger_Name, First_Drawer_Name, Last_Drawer_Name,Check_Notes, Balance_Before,Balance_After,Balance_BeginingPeriod,Bank_Name, Transfer_Check_Id,Transfer_Check_Id_From,SubAccount_Id,SubAccount_Name,Receipt,Factor, IS_Posted_Flage ,Currency_Id,Account_Entry_List_ID,  SubAccount_Id_RelatedTo,SubAccount_Name_RelatedTo,SubAccount_Id_RelatedToBefor,SubAccount_Name_RelatedToBefor, IsAllowEdit,IsAllowDelete,SubAccount_Id_RelatedFrom,SubAccount_Name_RelatedFrom) ";
                    mysql = mysql + " VALUES (" + chm.Order_Mony_Id + "," + ProcessNo + "," + chmlist.Id + ",'" + chm.Check_No + "','" + AccountModelsLib.operations.dateFormat(chm.Day_Date) + "','" + AccountModelsLib.operations.dateFormat(chm.Due_Date) + "'," + chm.Bank_id + "," + chm.Bank_Balance + "," + chm.Check_Value + ",'" + chm.Check_Value_ + "'," + chm.Import_Value + "," + chm.Export_Value + "," + chm.Customer_Id + "," + chm.Supplier_Id + "," + chm.SubAccount_Id + "," + chm.MainSubAccount_ID + ",'" + chm.Due_Check + "'," + chm.Transaction_Type + ",'" + chm.Exchanger_Name + "','" + chm.First_Drawer_Name + "','" + chm.Last_Drawer_Name + "','" + chm.Check_Notes + "'," + chm.Balance_Before + "," + chm.Balance_After + "," + chm.Balance_BeginingPeriod + ",'" + chm.Bank_Name + "'," + chm.Transfer_Check_Id + "," + chm.Transfer_Check_Id + "," + chm.SubAccount_Id + ",'" + chm.SubAccount_Name + "'," + chm.Receipt + "," + chm.Factor + "," + chm.IS_Posted_Flage + " ," + chm.Currency_Id + "," + chm.Account_Entry_List_ID + "," + chm.SubAccount_Id_RelatedTo + ",'" + chm.SubAccount_Name_RelatedTo + "'," + chm.SubAccount_Id_RelatedToBefor + ",'" + chm.SubAccount_Name_RelatedToBefor + "'," + chm.IsAllowEdit + "," + chm.IsAllowDelete + ", " + chm.SubAccount_Id_RelatedFrom + ",'" + chm.SubAccount_Name_RelatedFrom + "') ";

                    SetParameters(ref opp);

                    chm.Id = opp.Insert_me_Return_Last_Identify(mysql);
                    Excp = opp.Excp;
                }


                opp.cmd.Parameters.Clear();
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }


        }
        public void UpdateChecks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Checks_Memory chm, Checks_MemoryMainDataList chmList)
        {
            UpdateChecks_MemoryMainDataList(ref Excp, ref Msg, ref opp, chmList);
            try
            {
                string mysql = "";
                mysql = " Update Checks_Memory  Set Order_Mony_Id=" + chm.Order_Mony_Id + " , Check_No=" + chm.Check_No + " , Day_Date='" + AccountModelsLib.operations.dateFormat(chm.Day_Date) + "' , Due_Date='" + AccountModelsLib.operations.dateFormat(chm.Due_Date) + "' , Bank_id=" + chm.Bank_id + " ,Bank_Balance=" + chm.Bank_Balance + " , Check_Value=" + chm.Check_Value + " , Check_Value_='" + chm.Check_Value_ + "' ,Import_Value=" + chm.Import_Value + " ,Export_Value=" + chm.Export_Value + " , Customer_Id=" + chm.Customer_Id + " , Supplier_Id=" + chm.Supplier_Id + " , Account_Id=" + chm.Account_Id + " ,MainSubAccount_ID=" + chm.MainSubAccount_ID + " , Due_Check='" + chm.Due_Check + "' , Transaction_Type=" + chm.Transaction_Type + " , Exchanger_Name='" + chm.Exchanger_Name + "' , First_Drawer_Name='" + chm.First_Drawer_Name + "' , Last_Drawer_Name='" + chm.Last_Drawer_Name + "' ,Check_Notes= '" + chm.Check_Notes + "' , Balance_Before=" + chm.Balance_Before + " ,Balance_After=" + chm.Balance_After + " ,Balance_BeginingPeriod=" + chm.Balance_BeginingPeriod + " ,Bank_Name='" + chm.Bank_Name + "' , Transfer_Check_Id=" + chm.Transfer_Check_Id + " ,SubAccount_Id=" + chm.SubAccount_Id + " ,SubAccount_Name='" + chm.SubAccount_Name + "' ,Receipt=" + chm.Receipt + " ,Factor=" + chm.Factor + " , IS_Posted_Flage =" + chm.IS_Posted_Flage + " ,Currency_Id=" + chm.Currency_Id + " ,Account_Entry_List_ID=" + chm.Account_Entry_List_ID + " ,SubAccount_Id_RelatedTo=" + chm.SubAccount_Id_RelatedTo + " ,SubAccount_Name_RelatedTo='" + chm.SubAccount_Name_RelatedTo + "' ,SubAccount_Id_RelatedFrom=" + chm.SubAccount_Id_RelatedFrom + ",SubAccount_Name_RelatedFrom='" + chm.SubAccount_Name_RelatedFrom + "'";
                mysql = mysql + "  Where Id=" + chm.Id + "";
                SetParameters(ref opp);

                opp.insert_me(mysql);

                opp.cmd.Parameters.Clear();
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }
        }

        public void UpdateChecks_MemoryMainDataList(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Checks_MemoryMainDataList chm)
        {

            try
            {
                string mysql = " ";
                mysql = @"Update   Checks_MemoryMainDataList Set  [Check_No]='" + chm.Check_No + "'" +
                ",[Day_Date]='" + AccountModelsLib.operations.dateFormat(chm.Day_Date) + "'" +
                ",[Due_Date]='" + AccountModelsLib.operations.dateFormat(chm.Due_Date) + "'" +
                ",[Bank_id]=" + chm.Bank_id + "" +
                ",[Check_Value]=" + chm.Check_Value + "" +
                ",[Check_Value_]='" + chm.Check_Value_ + "'" +
                ",[Due_Check]='" + chm.Due_Check + "'" +
                ",[Exchanger_Name]='" + chm.Exchanger_Name + "'" +
                ",[First_Drawer_Name]='" + chm.First_Drawer_Name + "'" +
                ",[Last_Drawer_Name]='" + chm.Last_Drawer_Name + "'" +
                ",[Bank_Name]='" + chm.Bank_Name + "'" +
                ",[Check_Notes]='" + chm.Check_Notes + "'";

                mysql = mysql + "  Where Id=" + chm.Id + "";
                opp.insert_me(mysql);

                opp.cmd.Parameters.Clear();

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }


        }
        public void UpdateChecks_MemoryIsAllowEdit(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, int IsAllowEdit, long Checks_Memory_Id)
        {
            try
            {
                string mysql = "";
                mysql = " Update Checks_Memory  Set IsAllowEdit=" + IsAllowEdit + " ";
                mysql = mysql + "  Where Id=" + Checks_Memory_Id + "";
                opp.insert_me(mysql);
                opp.cmd.Parameters.Clear();
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }
        }
        public void UpdateChecks_MemoryIsAllowDelete(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, int IsAllowDelete, long Checks_Memory_Id)
        {
            try
            {
                string mysql = "";
                mysql = " Update Checks_Memory  Set IsAllowDelete=" + IsAllowDelete + " ";
                mysql = mysql + "  Where Id=" + Checks_Memory_Id + "";


                opp.insert_me(mysql);

                opp.cmd.Parameters.Clear();
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }
        }


        void SetParameters(ref AccountModelsLib.operations opp)
        {
            //opp.cmd.Parameters.AddWithValue("@Order_No", Order_No);
            //opp.cmd.Parameters.AddWithValue("@SubAccount_Id_RelatedTo", IdSubAccount_Id_RelatedTo;
            //opp.cmd.Parameters.AddWithValue("@SubAccount_Name_RelatedTo", SubAccount_Name_RelatedTo);
            //opp.cmd.Parameters.AddWithValue("@SubAccount_Id_RelatedToBefor", ISubAccount_Id_RelatedToBeford);
            //opp.cmd.Parameters.AddWithValue("@SubAccount_Name_RelatedToBefor", SubAccount_Name_RelatedToBefor);

        }
        public void Change_CheckStaus(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, int Receipt, Checks_Memory ccm, long Checks_Memory_IdTransfered, int IsAllowEdit)
        {
            try
            {
                string mysql = "";
                //mysql = " Update Checks_Memory  Set  Receipt=" + Receipt + ", Transfer_Check_Id_To=" + ccm.Id + " ,SubAccount_Id_RelatedTo=" + ccm.SubAccount_Id_RelatedTo + ",SubAccount_Name_RelatedTo='" + ccm.SubAccount_Name_RelatedTo + "',IsAllowEdit="+ IsAllowEdit + " ";
                //mysql = mysql + "  Where Id=" + Checks_Memory_IdTransfered + "";
                mysql = " Update Checks_Memory  Set  Receipt=" + Receipt + ", Transfer_Check_Id_To=" + ccm.Id + " ,    IsAllowEdit=" + IsAllowEdit + " ";
                mysql = mysql + "  Where Id=" + Checks_Memory_IdTransfered + "";


                opp.insert_me(mysql);
                Excp = 0;

            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;

            }
        }
        public DataTable GetChecks_Memory(long Checks_Memory_Id)
        {
            string mysql = @"SELECT        Order_Money_Check.Order_No, Order_Money_Check.Order_Date, Checks_Memory.*, Currency.Currency_Name
FROM            Checks_Memory INNER JOIN
                         Order_Money_Check ON Checks_Memory.Order_Mony_Id = Order_Money_Check.Id INNER JOIN
                         Currency ON Checks_Memory.Currency_Id = Currency.ID ";
            mysql = mysql + " WHERE(Checks_Memory.id = " + Checks_Memory_Id + ")";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }

        public void GetChecks_MemoryId(long Checks_Memory_Id)
        {
            string mysql = @"SELECT    * From       Checks_Memory  ";
            mysql = mysql + " WHERE (Checks_Memory.id = " + Checks_Memory_Id + ")";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                ReadData(dt, 0);
            }
        }

        public Checks_Memory GetChecks_MemoryById(long Checks_Memory_Id)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            var cm = opp.db.Checks_Memory.Where(p => p.Id == Checks_Memory_Id).FirstOrDefault();
            Checks_Memory cmm = cm;
            return cmm;
        }
        void ReadData(DataTable dt, int index)
        {
            cm.Id = Common.GetValueLong(dt, index, "Id");
            cm.Order_Mony_Id = Common.GetValueLong(dt, index, "Order_Mony_Id");

            cm.Check_No = Common.GetValueString(dt, index, "Check_No");
            cm.Day_Date = Common.GetValueNullDateTime(dt, index, "Day_Date");
            cm.Due_Date = Common.GetValueNullDateTime(dt, index, "Due_Date");

            cm.Bank_id = Common.GetValueInt(dt, index, "Bank_id");

            cm.Check_Value = Common.GetValueDecimal(dt, index, "Check_Value");

            cm.Check_Value_ = Common.GetValueString(dt, index, "Check_Value_");

            cm.Import_Value = Common.GetValueDecimal(dt, index, "Import_Value");
            cm.Export_Value = Common.GetValueDecimal(dt, index, "Export_Value");

            cm.Due_Check = Common.GetValueString(dt, index, "Due_Check");

            cm.Value_Receipt_Date = Common.GetValueNullDateTime(dt, index, "Value_Receipt_Date");
            cm.Receipt = Common.GetValueInt(dt, index, "Receipt");
            cm.Transaction_Type = Common.GetValueInt(dt, index, "Transaction_Type");
            cm.Exchanger_Name = Common.GetValueString(dt, index, "Exchanger_Name");
            cm.First_Drawer_Name = Common.GetValueString(dt, index, "First_Drawer_Name");
            cm.Bank_Balance = Common.GetValueDecimal(dt, index, "Bank_Balance");
            cm.Last_Drawer_Name = Common.GetValueString(dt, index, "Last_Drawer_Name");

            cm.Balance_Before = Common.GetValueDecimal(dt, index, "Balance_Before");

            cm.Balance_After = Common.GetValueDecimal(dt, index, "Balance_After");
            cm.Bank_Name = Common.GetValueString(dt, index, "Bank_Name");
            cm.SpentTo_Bank_Id = Common.GetValueInt(dt, index, "SpentTo_Bank_Id");
            cm.SpentTo_Bank_Type = Common.GetValueInt(dt, index, "SpentTo_Bank_Type");
            cm.Spent_Date = Common.GetValueNullDateTime(dt, index, "Spent_Date");
            cm.Bank_Id_Under_Ta7seel = Common.GetValueInt(dt, index, "Bank_Id_Under_Ta7seel");
            cm.Transfer_Check_Id = Common.GetValueLong(dt, index, "Transfer_Check_Id");
            cm.Balance_BeginingPeriod = Common.GetValueInt(dt, index, "Balance_BeginingPeriod");
            cm.Check_Notes = Common.GetValueString(dt, index, "Check_Notes");
            cm.OperationDate = Common.GetValueNullDateTime(dt, index, "OperationDate");
            cm.User_Id = Common.GetValueInt(dt, index, "User_Id");
            cm.Branch_Id = Common.GetValueInt(dt, index, "Branch_Id");
            cm.Insert_ServerDateTime = Common.GetValueNullDateTime(dt, index, "Insert_ServerDateTime");
            cm.Update_ServerDateTime = Common.GetValueNullDateTime(dt, index, "Update_ServerDateTime");
            cm.EditOperationDate = Common.GetValueNullDateTime(dt, index, "EditOperationDate");
            cm.Edit_User_Id = Common.GetValueInt(dt, index, "Edit_User_Id");
            cm.Customer_Id = Common.GetValueLong(dt, index, "Customer_Id");
            cm.Supplier_Id = Common.GetValueLong(dt, index, "Supplier_Id");
            cm.Account_Id = Common.GetValueLong(dt, index, "Account_Id");
            cm.SubAccount_Id = Common.GetValueLong(dt, index, "SubAccount_Id");
            cm.SubAccount_NO = Common.GetValueString(dt, index, "SubAccount_NO");
            cm.SubAccount_Name = Common.GetValueString(dt, index, "SubAccount_Name");
            cm.MainSubAccount_ID = Common.GetValueLong(dt, index, "MainSubAccount_ID");

            cm.IS_Posted_Flage = Common.GetValueInt(dt, index, "IS_Posted_Flage");
            cm.Currency_Id = Common.GetValueInt(dt, index, "Currency_Id");

            cm.Account_Entry_List_ID = Common.GetValueLong(dt, index, "Account_Entry_List_ID");
            cm.SubAccount_Id_RelatedTo = Common.GetValueLong(dt, index, "SubAccount_Id_RelatedTo");
            cm.SubAccount_Name_RelatedTo = Common.GetValueString(dt, index, "SubAccount_Name_RelatedTo");

            cm.SubAccount_Id_RelatedFrom = Common.GetValueLong(dt, index, "SubAccount_Id_RelatedFrom");
            cm.SubAccount_Name_RelatedFrom = Common.GetValueString(dt, index, "SubAccount_Name_RelatedFrom");


            cm.Factor = Common.GetValueDecimal(dt, index, "Factor");
            cm.ProcessNo = Common.GetValueLong(dt, index, "ProcessNo");
            cm.IsAllowEdit = Common.GetValueInt(dt, index, "IsAllowEdit");
            cm.IsAllowDelete = Common.GetValueInt(dt, index, "IsAllowDelete");

        }
        /*
        void ReadData(DataTable dt, int index)
        {
            Id = Common.GetValueLong(dt, index, "Id");
            Order_Mony_Id = Common.GetValueLong(dt, index, "Order_Mony_Id");

            Check_No = Common.GetValueString(dt, index, "Check_No");
            Day_Date = Common.GetValueNullDateTime(dt, index, "Day_Date");
            Due_Date = Common.GetValueNullDateTime(dt, index, "Due_Date");

            Bank_id = Common.GetValueInt(dt, index, "Bank_id");

            Check_Value = Common.GetValueDecimal(dt, index, "Check_Value");

            Check_Value_ = Common.GetValueString(dt, index, "Check_Value_");

            Import_Value = Common.GetValueDecimal(dt, index, "Import_Value");
            Export_Value = Common.GetValueDecimal(dt, index, "Export_Value");

            Due_Check = Common.GetValueString(dt, index, "Due_Check");

            Value_Receipt_Date = Common.GetValueNullDateTime(dt, index, "Value_Receipt_Date");
            Receipt = Common.GetValueInt(dt, index, "Receipt");
            Transaction_Type = Common.GetValueInt(dt, index, "Transaction_Type");
            Exchanger_Name = Common.GetValueString(dt, index, "Exchanger_Name");
            First_Drawer_Name = Common.GetValueString(dt, index, "First_Drawer_Name");
            Bank_Balance = Common.GetValueDecimal(dt, index, "Bank_Balance");
            Last_Drawer_Name = Common.GetValueString(dt, index, "Last_Drawer_Name");

            Balance_Before = Common.GetValueDecimal(dt, index, "Balance_Before");

            Balance_After = Common.GetValueDecimal(dt, index, "Balance_After");
            Bank_Name = Common.GetValueString(dt, index, "Bank_Name");
            SpentTo_Bank_Id = Common.GetValueInt(dt, index, "SpentTo_Bank_Id");
            SpentTo_Bank_Type = Common.GetValueInt(dt, index, "SpentTo_Bank_Type");
            Spent_Date = Common.GetValueNullDateTime(dt, index, "Spent_Date");
            Bank_Id_Under_Ta7seel = Common.GetValueInt(dt, index, "Bank_Id_Under_Ta7seel");
            Transfer_Check_Id = Common.GetValueLong(dt, index, "Transfer_Check_Id");
            Balance_BeginingPeriod = Common.GetValueInt(dt, index, "Balance_BeginingPeriod");
            Check_Notes = Common.GetValueString(dt, index, "Check_Notes");
            OperationDate = Common.GetValueNullDateTime(dt, index, "OperationDate");
            User_Id = Common.GetValueInt(dt, index, "User_Id");
            Branch_Id = Common.GetValueInt(dt, index, "Branch_Id");
            Insert_ServerDateTime = Common.GetValueNullDateTime(dt, index, "Insert_ServerDateTime");
            Update_ServerDateTime = Common.GetValueNullDateTime(dt, index, "Update_ServerDateTime");
            EditOperationDate = Common.GetValueNullDateTime(dt, index, "EditOperationDate");
            Edit_User_Id = Common.GetValueInt(dt, index, "Edit_User_Id");
            Customer_Id = Common.GetValueLong(dt, index, "Customer_Id");
            Supplier_Id = Common.GetValueLong(dt, index, "Supplier_Id");
            Account_Id = Common.GetValueLong(dt, index, "Account_Id");
            SubAccount_Id = Common.GetValueLong(dt, index, "SubAccount_Id");
            SubAccount_NO = Common.GetValueString(dt, index, "SubAccount_NO");
            SubAccount_Name = Common.GetValueString(dt, index, "SubAccount_Name");
            MainSubAccount_ID = Common.GetValueLong(dt, index, "MainSubAccount_ID");

            IS_Posted_Flage = Common.GetValueInt(dt, index, "IS_Posted_Flage");
            Currency_Id = Common.GetValueInt(dt, index, "Currency_Id");

            Account_Entry_List_ID = Common.GetValueLong(dt, index, "Account_Entry_List_ID");
            SubAccount_Id_RelatedTo = Common.GetValueLong(dt, index, "SubAccount_Id_RelatedTo");
            SubAccount_Name_RelatedTo = Common.GetValueString(dt, index, "SubAccount_Name_RelatedTo");

            SubAccount_Id_RelatedFrom = Common.GetValueLong(dt, index, "SubAccount_Id_RelatedFrom");
            SubAccount_Name_RelatedFrom = Common.GetValueString(dt, index, "SubAccount_Name_RelatedFrom");


            Factor = Common.GetValueDecimal(dt, index, "Factor");
            ProcessNo = Common.GetValueLong(dt, index, "ProcessNo");
            IsAllowEdit = Common.GetValueInt(dt, index, "IsAllowEdit");
            IsAllowDelete = Common.GetValueInt(dt, index, "IsAllowDelete");

        }
        */
        public void Checks_MemoryHistory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Checks_MemoryId)
        {
            try
            {
                string mysql = @"INSERT INTO Checks_MemoryHistory
                         (Checks_MemoryId, Account_Entry_List_ID, SubAccount_Id_RelatedTo, SubAccount_Name_RelatedTo, SubAccount_Id_RelatedToBefor, SubAccount_Name_RelatedToBefor, SubAccount_Id_RelatedFrom, 
                         SubAccount_Name_RelatedFrom, ProcessNo, HistoryReceipt, HistoryDate)
SELECT        id, Account_Entry_List_ID, SubAccount_Id_RelatedTo, SubAccount_Name_RelatedTo, SubAccount_Id_RelatedToBefor, SubAccount_Name_RelatedToBefor, SubAccount_Id_RelatedFrom, SubAccount_Name_RelatedFrom, 
                         ProcessNo, Receipt, GETDATE() AS Expr1
FROM            Checks_Memory ";
                mysql = mysql + " WHERE (id = " + Checks_MemoryId + ")";

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }
        public DataTable GeChecks_MemoryHistory(long Checks_Memory_Id)
        {
            string mysql = @"SELECT    * From       Checks_MemoryHistory  ";
            mysql = mysql + " WHERE (Checks_MemoryHistory.Checks_MemoryId = " + Checks_Memory_Id + ")";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }


        public void DeleteChecks_Memory(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Checks_Memory_Id)
        {

            string mysql = "";
            mysql = " Delete From Checks_Memory   ";
            mysql = mysql + "  Where Id=" + Checks_Memory_Id + "";
            SetParameters(ref opp);

            opp.insert_me(mysql);


        }
    }
}
