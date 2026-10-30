using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
 
using Microsoft.EntityFrameworkCore;

using System.Linq;
using AccountModelsLib.Models;
 
 
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;
namespace AccountModelsLib
{
    public class AccountEntry_CLS
    {

        public AccountEntry_CLS()
        {
        }
        public int Reference_Type = 0;
        public long Reference_Type_Table_Id = 0;
        public long IsAutomaticEntryOrManualy = 1;
        public long DataDictionaryProcessesListsId = 0;
        //public bool IsNotDeletePrevEntry = false;
        public long Entry_No = 0;
        public bool IsNotDeletePrevEntry { get; set; } = false;

        public long CarsWorkOrdersId = 0;
        public long CarsId = 0;
        public long WorkOrderNumber = 0;
        public string CarNumber = "";
        public DataTable Account_Entry_ToPrint__(long Account_Entry_List_ID)
        {
            string mysql = "";
             
            mysql = @" SELECT        0 AS Id_Test, Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Line_No, Account_Entry_List.Id AS Expr1, Account_Entry_List.Currency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Submain_Account.Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, Submain_Account.Company_Name_For_GL, 
                         Submain_Account.Account_Customer_Type_Value, Submain_Account.Transform_Type, Submain_Account.Account_Customer_Type_ID,   Submain_Account.Currency_Id AS Expr2, 
                         Submain_Account.Branch_ID, Account_Entry_List.Entry_Notes, Account_Entry_List.User_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_Details.ID AS Account_Entry_Details_Id, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Submain_Account.SubAccount_NO, Account_Entry_Details.Import_Value, Account_Entry_Details.Export_Value, 
                         Account_Entry_Details.Transaction_Value, Account_Entry_Details.Notes_Details, Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo, Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, Account_Entry_Details.Equivalent_Transaction_Value, 
                         Account_Entry_Details.Account_Currency_ID, Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Submain_Account.Main_Account_Id AS MainAccount_Id, 
                         '' AS MainAccount_Name, Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, Account_Entry_List.Year_Financial_Period_Id, 
                         Account_Entry_List.Doc_No, Account_Entry_List.IS_Make_AutomaticFromTar7eel, Journal_Codes.Journal_Name, Currency.Currency_Name_EN, Currency.Currency_Name, Account_Entry_List.Entry_Currency_ID AS Expr3, 
                         Account_Entry_List.Factor, Account_Entry_List.Entry_Currency_Name, Account_Entry_List.User_Id_Login_Name, Account_Entry_List.User_ID_LoginUpdate, Account_Entry_List.Branch_ID_Login
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id INNER JOIN
                         Currency ON Submain_Account.Currency_Id = Currency.[Value] LEFT OUTER JOIN
                         Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code";
            mysql = mysql + " WHERE        (Account_Entry_Details.Account_Entry_List_ID = " + Account_Entry_List_ID + ")  ";
            mysql = mysql + " ORDER BY Account_Entry_Details.Transaction_Type,Line_No ";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;

        }
        public void Get_Empty_dt_Details(ref DataTable dt_Details)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            Get_Empty_dt_Details(ref opp, ref dt_Details);
        }
        public void Get_Empty_dt_Details(ref AccountModelsLib.operations opp, ref DataTable dt_Details)
        {
            string mysql = @" SELECT        0 AS Id_Test, Account_Entry_Details.Account_Name, Account_Entry_Details.Line_No, Account_Entry_List.Id, Account_Entry_List.Currency_ID AS EntryCurrency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Submain_Account.Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.Company_ID_For_GL, Submain_Account.Company_Name_For_GL, 
                         Submain_Account.Account_Customer_Type_Value, Submain_Account.Transform_Type, Submain_Account.Account_Customer_Type_ID,   Submain_Account.Currency_Id, 
                         Submain_Account.Branch_ID, Account_Entry_List.Entry_Notes, Account_Entry_List.User_ID_Login, Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_Details.ID AS Account_Entry_Details_Id, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Submain_Account.SubAccount_NO, Account_Entry_Details.Import_Value, Account_Entry_Details.Export_Value, 
                         Account_Entry_Details.Transaction_Value, Account_Entry_Details.Notes_Details, Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, Account_Entry_Details.Account_Code AS Expr1, 
                         Account_Entry_Details.Account_Customer_Type_Value AS Expr2, Account_Entry_Details.Account_Customer_Type_ID AS Expr3, 
                         Account_Entry_Details.Transaction_DateFrom, Account_Entry_Details.Transaction_DateTo, Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, 
                         Account_Entry_Details.Equivalent_Transaction_Value, Account_Entry_Details.Account_Currency_ID, Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, 
                         Submain_Account.Main_Account_Id AS MainAccount_Id, '' AS MainAccount_Name, Account_Entry_Details.Factor, Account_Entry_Details.CarsWorkOrdersId, Account_Entry_Details.CarsId, 
                         Account_Entry_Details.WorkOrderNumber, Account_Entry_Details.CarNumber
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
WHERE        (Account_Entry_List.Id = - 1)";



            dt_Details = opp.return_DataTable(mysql);
        }
        public void AddRow_WhenBuild(ref int Excp, ref DataTable dt_Details, long Id_Test, string Account_Code, int Account_Type, long MainAccount_Id, string MainAccount_Name, long Submain_Account_ID, string SubAccount_Name, int MadinDa2n, decimal Entry_Value, string Notes_Details, decimal Equivalent_Currency_Value, long Account_Currency_Id, long    Entry_Currency_ID, long CostCenter_Id, string CostCenter_Name, long Branch_Id_Entry, decimal Factor)
        {

            try
            {
                string expression;
                expression = "Transaction_Type=1";
                DataRow[] foundRows;
                foundRows = dt_Details.Select(expression);

                int Count = foundRows.Length;

                //Entry_Value = Common.RoundMeFormat(Entry_Value);
                Entry_Value = Common.RoundMe(Entry_Value);
                decimal Equivalent_Transaction_Value = Equivalent_Currency_Value * Entry_Value;
                Equivalent_Transaction_Value = Common.RoundMe(Equivalent_Transaction_Value);
                DataRow dr = dt_Details.NewRow();

                try
                {
                    dr["Id_Test"] = Id_Test;
                }
                catch { }
                dr["Submain_Account_ID"] = Submain_Account_ID;
                dr["MainAccount_Id"] = MainAccount_Id;
                dr["SubAccount_Name"] = SubAccount_Name;
                dr["MainAccount_Name"] = MainAccount_Name;

                dr["Account_Currency_ID"] = Account_Currency_Id;
                dr["Entry_Currency_ID"] = Entry_Currency_ID;
                dr["Factor"] = Factor;
                dr["CostCenter_Id"] = CostCenter_Id;
                dr["CostCenter_Name"] = CostCenter_Name;
                dr["Account_Customer_Type_Value"] = Account_Type;
                dr["SubAccount_NO"] = Account_Code;

                try
                {
                    dr["Account_Code"] = Account_Code;
                }
                catch { }

                if (Account_Type == 10)
                {
                    AccountModelsLib.operations opp = new AccountModelsLib.operations();

                    string mysql = "Select * from Submain_Account Where Id=" + Submain_Account_ID;
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {

                        try
                        {
                            dr["Account_Customer_Type_ID"] = dt.Rows[0]["Account_Customer_Type_ID"].ToString();
                        }
                        catch
                        {
                            dr["Account_Customer_Type_ID"] = 0;
                        }
                        //try
                        //{
                        //    dr["Account_Employee_Type_ID"] = dt.Rows[0]["Account_Employee_Type_ID"].ToString();
                        //}
                        //catch
                        //{
                        //    dr["Account_Employee_Type_ID"] = 0;
                        //}

                        dr["Account_Code"] = dt.Rows[0]["Code"].ToString();

                        dr["SubAccount_NO"] = dt.Rows[0]["Code"].ToString();

                        dr["SubAccount_Name"] = dt.Rows[0]["SubAccount_Name"].ToString();
                    }
                }
                else
                {
                    if (Account_Type == 4)
                    {
                        dr["Account_Customer_Type_ID"] = Submain_Account_ID;
                        //dr["Account_Employee_Type_ID"] = Submain_Account_ID;
                    }
                    else
                    {
                        dr["Account_Customer_Type_ID"] = Submain_Account_ID;
                       // dr["Account_Employee_Type_ID"] = 0;
                    }


                    dr["SubAccount_NO"] = Account_Code;

                    dr["SubAccount_Name"] = SubAccount_Name;
                }

                int Transaction_Type = 0;
                if (MadinDa2n == 1)
                {
                    dr["Import_Value"] = Entry_Value;
                    dr["Export_Value"] = "0";
                    dr["Transaction_Type"] = "1";
                    Transaction_Type = 1;
                    dr["Equivalent_Import_Value"] = Equivalent_Transaction_Value;
                    dr["Equivalent_Export_Value"] = "0";



                }
                else
                {
                    dr["Import_Value"] = "0";
                    dr["Export_Value"] = Entry_Value;
                    dr["Transaction_Type"] = "2";
                    Transaction_Type = 2;
                    dr["Equivalent_Import_Value"] = 0;
                    dr["Equivalent_Export_Value"] = Equivalent_Transaction_Value;
                }

                dr["Equivalent_Transaction_Value"] = Equivalent_Transaction_Value;
                dr["Notes_Details"] = Notes_Details;
                dr["Equivalent_Currency_Value"] = Equivalent_Currency_Value;

                /////////////////////////
                try
                {
                    ////cccccc
                    dr["CarsWorkOrdersId"] = CarsWorkOrdersId;
                    dr["CarsId"] = CarsId;
                    dr["WorkOrderNumber"] = WorkOrderNumber;
                    dr["CarNumber"] = CarNumber;

                }
                catch (Exception ee)
                {

                }

                 
                if (Transaction_Type == 1)
                {
                    dt_Details.Rows.InsertAt(dr, Count);
                }
                else
                {
                    dt_Details.Rows.Add(dr);
                }

                //dt_Details.Rows.Add(dr);

                //for (int x = 0; x < dt_Details.Rows.Count; x++)
                //{
                //    int y = x + 1;
                //    dt_Details.Rows[x]["Line_No"] = y;
                //    //
                //}
                Excp = 0;
            }
            catch (Exception ee)
            {
                
                Excp = 1;
            }
        }

        public void AddRow_WhenBuild(ref int Excp, ref DataTable dt_Details, long Id_Test, string Account_Code, int Account_Type, long MainAccount_Id, string MainAccount_Name, long Submain_Account_ID, string SubAccount_Name, int MadinDa2n, decimal Entry_Value, string Notes_Details, decimal Equivalent_Currency_Value, long Account_Currency_Id, long    Entry_Currency_ID, long CostCenter_Id, string CostCenter_Name, long Branch_Id_Entry, decimal Factor, long CarsWorkOrdersId, long CarsId, long WorkOrderNumber, string CarNumber)
        {
            

            try
            {
                string expression;
                expression = "Transaction_Type=1";
                DataRow[] foundRows;
                foundRows = dt_Details.Select(expression);

                int Count = foundRows.Length;

                //Entry_Value = Common.RoundMeFormat(Entry_Value);
                Entry_Value = Common.RoundMe(Entry_Value);
                decimal Equivalent_Transaction_Value = Equivalent_Currency_Value * Entry_Value;
                Equivalent_Transaction_Value = Common.RoundMe(Equivalent_Transaction_Value);
                DataRow dr = dt_Details.NewRow();

                try
                {
                    dr["Id_Test"] = Id_Test;
                }
                catch { }
                dr["Submain_Account_ID"] = Submain_Account_ID;
                dr["MainAccount_Id"] = MainAccount_Id;
                dr["SubAccount_Name"] = SubAccount_Name;
                dr["MainAccount_Name"] = MainAccount_Name;

                dr["Account_Currency_ID"] = Account_Currency_Id;
                dr["Entry_Currency_ID"] = Entry_Currency_ID;
                dr["Factor"] = Factor;
                dr["CostCenter_Id"] = CostCenter_Id;
                dr["CostCenter_Name"] = CostCenter_Name;
                dr["Account_Customer_Type_Value"] = Account_Type;
                dr["SubAccount_NO"] = Account_Code;

                try
                {
                    dr["Account_Code"] = Account_Code;
                }
                catch { }

                if (Account_Type == 10)
                {
                    AccountModelsLib.operations opp = new AccountModelsLib.operations();

                    string mysql = "Select * from Submain_Account Where Id=" + Submain_Account_ID;
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {

                        try
                        {
                            dr["Account_Customer_Type_ID"] = dt.Rows[0]["Account_Customer_Type_ID"].ToString();
                        }
                        catch
                        {
                            dr["Account_Customer_Type_ID"] = 0;
                        }
                        //try
                        //{
                        //    dr["Account_Employee_Type_ID"] = dt.Rows[0]["Account_Employee_Type_ID"].ToString();
                        //}
                        //catch
                        //{
                        //    dr["Account_Employee_Type_ID"] = 0;
                        //}

                        dr["Account_Code"] = dt.Rows[0]["Code"].ToString();

                        dr["SubAccount_NO"] = dt.Rows[0]["Code"].ToString();

                        dr["SubAccount_Name"] = dt.Rows[0]["SubAccount_Name"].ToString();
                    }
                }
                else
                {
                    if (Account_Type == 4)
                    {
                        dr["Account_Customer_Type_ID"] = Submain_Account_ID;
                        //dr["Account_Employee_Type_ID"] = Submain_Account_ID;
                    }
                    else
                    {
                        dr["Account_Customer_Type_ID"] = Submain_Account_ID;
                        //dr["Account_Employee_Type_ID"] = 0;
                    }


                    dr["SubAccount_NO"] = Account_Code;

                    dr["SubAccount_Name"] = SubAccount_Name;
                }

                int Transaction_Type = 0;
                if (MadinDa2n == 1)
                {
                    dr["Import_Value"] = Entry_Value;
                    dr["Export_Value"] = "0";
                    dr["Transaction_Type"] = "1";
                    Transaction_Type = 1;
                    dr["Equivalent_Import_Value"] = Equivalent_Transaction_Value;
                    dr["Equivalent_Export_Value"] = "0";



                }
                else
                {
                    dr["Import_Value"] = "0";
                    dr["Export_Value"] = Entry_Value;
                    dr["Transaction_Type"] = "2";
                    Transaction_Type = 2;
                    dr["Equivalent_Import_Value"] = 0;
                    dr["Equivalent_Export_Value"] = Equivalent_Transaction_Value;
                }

                dr["Equivalent_Transaction_Value"] = Equivalent_Transaction_Value;
                dr["Notes_Details"] = Notes_Details;
                dr["Equivalent_Currency_Value"] = Equivalent_Currency_Value;

                /////////////////////////
                try
                {
                   // long CarsWorkOrdersId, long CarsId, long WorkOrderNumber,string CarNumber
                    ////cccccc
                    dr["CarsWorkOrdersId"] = CarsWorkOrdersId;
                    dr["CarsId"] = CarsId;
                    dr["WorkOrderNumber"] = WorkOrderNumber;
                    dr["CarNumber"] = CarNumber;

                }
                catch (Exception ee)
                {

                }
                if (Transaction_Type == 1)
                {
                    dt_Details.Rows.InsertAt(dr, Count);
                }
                else
                {
                    dt_Details.Rows.Add(dr);
                }

                //dt_Details.Rows.Add(dr);

                //for (int x = 0; x < dt_Details.Rows.Count; x++)
                //{
                //    int y = x + 1;
                //    dt_Details.Rows[x]["Line_No"] = y;
                //    //
                //}
                Excp = 0;
            }
            catch (Exception ee)
            {
                
                Excp = 1;
            }
        }
        public void EditRow_WhenBuild(ref int Excp,ref string Msg, ref DataTable dt_Details, int RowIndex, ref ArrayList arr_IdDelet, long Id_Test, string Account_Code, int Account_Type, long MainAccount_Id, string MainAccount_Name, long Submain_Account_ID, string SubAccount_Name, int MadinDa2n, decimal Entry_Value, string Notes_Details, decimal Equivalent_Currency_Value, long Account_Currency_Id, long    Entry_Currency_ID, long CostCenter_Id, string CostCenter_Name, long Branch_Id_Entry)
        {
            try
            {

                decimal Equivalent_Transaction_Value = Equivalent_Currency_Value * Entry_Value;
                Equivalent_Transaction_Value = Common.RoundMe(Equivalent_Transaction_Value);
                DataRow dr = dt_Details.NewRow();

                try
                {
                    dr["Id_Test"] = Id_Test;
                }
                catch { }
                dr["Submain_Account_ID"] = Submain_Account_ID;
                dr["MainAccount_Id"] = MainAccount_Id;
                dr["SubAccount_Name"] = SubAccount_Name;
                dr["MainAccount_Name"] = MainAccount_Name;

                dr["Account_Currency_ID"] = Account_Currency_Id;
                dr["Entry_Currency_ID"] = Entry_Currency_ID;
                dr["CostCenter_Id"] = CostCenter_Id;
                dr["CostCenter_Name"] = CostCenter_Name;
                dr["Account_Customer_Type_Value"] = Account_Type;
                dr["SubAccount_NO"] = Account_Code;

                try
                {
                    dr["Account_Code"] = Account_Code;
                }
                catch { }

                if (Account_Type == 10)
                {
                    AccountModelsLib.operations opp = new AccountModelsLib.operations();

                    string mysql = "Select * from Submain_Account Where Id=" + Submain_Account_ID;
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {

                        try
                        {
                            dr["Account_Customer_Type_ID"] = dt.Rows[0]["Account_Customer_Type_ID"].ToString();
                        }
                        catch
                        {
                            dr["Account_Customer_Type_ID"] = 0;
                        }
                        //try
                        //{
                        //    dr["Account_Employee_Type_ID"] = dt.Rows[0]["Account_Employee_Type_ID"].ToString();
                        //}
                        //catch
                        //{
                        //    dr["Account_Employee_Type_ID"] = 0;
                        //}

                        dr["Account_Code"] = dt.Rows[0]["Code"].ToString();

                        dr["SubAccount_NO"] = dt.Rows[0]["Code"].ToString();

                        dr["SubAccount_Name"] = dt.Rows[0]["SubAccount_Name"].ToString();
                    }
                }
                else
                {
                    if (Account_Type == 4)
                    {
                        dr["Account_Customer_Type_ID"] = Submain_Account_ID;
                        //dr["Account_Employee_Type_ID"] = Submain_Account_ID;
                    }
                    else
                    {
                        dr["Account_Customer_Type_ID"] = Submain_Account_ID;
                        //dr["Account_Employee_Type_ID"] = 0;
                    }


                    dr["SubAccount_NO"] = Account_Code;

                    dr["SubAccount_Name"] = SubAccount_Name;
                }
                int Transaction_Type = 1;
                if (MadinDa2n == 1)
                {
                    dr["Import_Value"] = Entry_Value;
                    dr["Export_Value"] = "0";
                    dr["Transaction_Type"] = "1";
                    Transaction_Type = 1;
                    dr["Equivalent_Import_Value"] = Equivalent_Transaction_Value;
                    dr["Equivalent_Export_Value"] = "0";



                }
                else
                {
                    dr["Import_Value"] = "0";
                    dr["Export_Value"] = Entry_Value;
                    dr["Transaction_Type"] = "2";
                    Transaction_Type = 2;
                    dr["Equivalent_Import_Value"] = 0;
                    dr["Equivalent_Export_Value"] = Equivalent_Transaction_Value;
                }

                dr["Equivalent_Transaction_Value"] = Equivalent_Transaction_Value;
                dr["Notes_Details"] = Notes_Details;
                dr["Equivalent_Currency_Value"] = Equivalent_Currency_Value;

                //   arr_IdDelet
                long Account_Entry_Details_IdEdit = 0;
                try
                {
                    Account_Entry_Details_IdEdit = long.Parse(dt_Details.Rows[RowIndex]["Account_Entry_Details_Id"].ToString());
                }
                catch { Account_Entry_Details_IdEdit = 0; }

                if (Account_Entry_Details_IdEdit != -1)
                {
                    //dt_Details.Rows.RemoveAt(index);
                    arr_IdDelet.Add(Account_Entry_Details_IdEdit);
                }

                //////
                dt_Details.Rows.RemoveAt(RowIndex);
                dt_Details.Rows.InsertAt(dr, RowIndex);

                //for (int x = 0; x < dt_Details.Rows.Count; x++)
                //{
                //    int y = x + 1;
                //    dt_Details.Rows[x]["Line_No"] = y;
                //    //
                //}
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg=ee.Message;
                
                Excp = 1;
            }
        }
        public void EditRow_Equivalent_Transaction_Value(ref int Excp,ref string Msg, ref DataTable dt_Details, decimal Equivalent_Currency_Value, ref ArrayList arr_IdDelet)
        {
            try
            {
                //

                for (int x = 0; x < dt_Details.Rows.Count; x++)
                {

                    decimal Equivalent_Transaction_Value = 0;
                    dt_Details.Rows[x]["Id_Test"] = 1;
                    int Transaction_Type = int.Parse(dt_Details.Rows[x]["Transaction_Type"].ToString());

                    if (Transaction_Type == 1)
                    {
                        decimal Import_Value = decimal.Parse(dt_Details.Rows[x]["Import_Value"].ToString());
                        Equivalent_Transaction_Value = Equivalent_Currency_Value * Import_Value;
                        Equivalent_Transaction_Value = Common.RoundMe(Equivalent_Transaction_Value);
                        dt_Details.Rows[x]["Equivalent_Import_Value"] = Equivalent_Transaction_Value;
                        dt_Details.Rows[x]["Equivalent_Export_Value"] = 0;
                        //dr["Import_Value"] = Entry_Value;
                        //dr["Export_Value"] = "0";
                        //dr["Transaction_Type"] = "1";
                        //Transaction_Type = 1;
                        //dr["Equivalent_Import_Value"] = Equivalent_Transaction_Value;
                        //dr["Equivalent_Export_Value"] = "0";



                    }
                    else
                    {
                        decimal Export_Value = decimal.Parse(dt_Details.Rows[x]["Export_Value"].ToString());
                        Equivalent_Transaction_Value = Equivalent_Currency_Value * Export_Value;
                        Equivalent_Transaction_Value = Common.RoundMe(Equivalent_Transaction_Value);
                        dt_Details.Rows[x]["Equivalent_Import_Value"] = 0;
                        dt_Details.Rows[x]["Equivalent_Export_Value"] = Equivalent_Transaction_Value;


                        //dr["Import_Value"] = "0";
                        //dr["Export_Value"] = Entry_Value;
                        //dr["Transaction_Type"] = "2";
                        //Transaction_Type = 2;
                        //dr["Equivalent_Import_Value"] = 0;
                        //dr["Equivalent_Export_Value"] = Equivalent_Transaction_Value;
                    }

                    dt_Details.Rows[x]["Equivalent_Transaction_Value"] = Equivalent_Transaction_Value;
                    dt_Details.Rows[x]["Equivalent_Currency_Value"] = Equivalent_Currency_Value;

                    ////////////////////////////////////
                    int Account_Customer_Type_Value = int.Parse(dt_Details.Rows[x]["Account_Customer_Type_Value"].ToString());
                    long Submain_Account_ID = long.Parse(dt_Details.Rows[x]["Submain_Account_ID"].ToString());
                    AccountModelsLib.operations opp = new AccountModelsLib.operations();

                    string mysql = "Select * from Submain_Account Where Id=" + Submain_Account_ID;
                    DataTable dt = opp.return_DataTable(mysql);

                    if (Account_Customer_Type_Value == 10)
                    {

                        if (dt.Rows.Count > 0)
                        {

                            try
                            {
                                dt_Details.Rows[x]["Account_Customer_Type_ID"] = dt.Rows[0]["Account_Customer_Type_ID"].ToString();
                            }
                            catch
                            {
                                dt_Details.Rows[x]["Account_Customer_Type_ID"] = 0;
                            }
                            //try
                            //{
                            //    dt_Details.Rows[x]["Account_Employee_Type_ID"] = dt.Rows[0]["Account_Employee_Type_ID"].ToString();
                            //}
                            //catch
                            //{
                            //    dt_Details.Rows[x]["Account_Employee_Type_ID"] = 0;
                            //}

                            dt_Details.Rows[x]["Account_Code"] = dt.Rows[0]["Code"].ToString();

                            dt_Details.Rows[x]["SubAccount_NO"] = dt.Rows[0]["Code"].ToString();

                            dt_Details.Rows[x]["SubAccount_Name"] = dt.Rows[0]["SubAccount_Name"].ToString();
                        }
                    }
                    else
                    {
                        if (Account_Customer_Type_Value == 4)
                        {
                            dt_Details.Rows[x]["Account_Customer_Type_ID"] = Submain_Account_ID;
                            //dt_Details.Rows[x]["Account_Employee_Type_ID"] = Submain_Account_ID;
                        }
                        else
                        {
                            dt_Details.Rows[x]["Account_Customer_Type_ID"] = Submain_Account_ID;
                            //dt_Details.Rows[x]["Account_Employee_Type_ID"] = 0;
                        }


                        dt_Details.Rows[x]["SubAccount_NO"] = dt.Rows[0]["Code"].ToString();

                        dt_Details.Rows[x]["SubAccount_Name"] = dt.Rows[0]["SubAccount_Name"].ToString();
                    }
                    //   arr_IdDelet
                    long Account_Entry_Details_IdEdit = 0;
                    try
                    {
                        Account_Entry_Details_IdEdit = long.Parse(dt_Details.Rows[x]["Account_Entry_Details_Id"].ToString());
                    }
                    catch { Account_Entry_Details_IdEdit = 0; }

                    if (Account_Entry_Details_IdEdit != -1)
                    {

                        arr_IdDelet.Add(Account_Entry_Details_IdEdit);
                    }


                }

                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                
                Excp = 1;
            }
        }
        public static bool CheckNewEntry_No(DateTime dt_Entry, long journal_Code, long Entry_No, ref long Account_Entry_List_Id)
        {
            Account_Entry_List_Id = 0;
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(dt_Entry);

            bool FoundEntry_No = false;

            string mysql = " SELECT  Entry_No,Id FROM  Account_Entry_List  ";
            mysql = mysql + " WHERE   (Year_Financial_Period_Id = " + Year_Financial_Period_Id + " ) AND (journal_Code = " + journal_Code + ") AND (Entry_No = " + Entry_No + ")";


            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                FoundEntry_No = true;
                Account_Entry_List_Id = long.Parse(dt.Rows[0]["Id"].ToString());
            }
            else
            {
                FoundEntry_No = false;
            }
            return FoundEntry_No;

        }

        public static long NewEntry_No(DateTime dt_Entry, int journal_Code)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return NewEntry_No(ref opp, dt_Entry, journal_Code, 0);

        }
        public static long NewEntry_No(ref AccountModelsLib.operations opp, DateTime dt_Entry, long journal_Code, long Account_Entry_List_ID)
        {
            long Entry_No = 1;
            if (Account_Entry_List_ID > 0)
            {
                int Excp = 0;
                string Msg = "";
                GetEntry_No(ref opp, ref Excp, ref Msg, Account_Entry_List_ID, ref Entry_No);

            }
            if (Entry_No == 1)
            {

                long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(dt_Entry);

                try
                {
                    string mysql = " SELECT MAX(Entry_No) AS Entry_No FROM  Account_Entry_List  Where Year_Financial_Period_Id=" + Year_Financial_Period_Id;
                    mysql = mysql + " GROUP BY Year_Financial_Period_Id ";

                    if (journal_Code > 0)
                    {

                        mysql = mysql + "  , journal_Code HAVING  (journal_Code =" + journal_Code + ") ";
                    }
                    DataTable dt = opp.return_DataTable(mysql);
                    try
                    {
                        Entry_No = long.Parse(dt.Rows[0]["Entry_No"].ToString());
                        Entry_No = Entry_No + 1;
                        return Entry_No;
                    }
                    catch { return Entry_No; }
                }
                catch { return Entry_No; }
            }
            return Entry_No;
        }
        public void Make_AccountEntry(ref int Excp, ref string Msg, ref long Account_Entry_List_ID, ref AccountModelsLib.operations opp, long User_ID_Login, string User_Id_Login_Name, long Branch_ID_Login, long Entry_No, DateTime Entry_Date, long journal_Code, long Currency_Id, string Entry_Notes, int Transform_Type, DataTable dt_Details, double Entry_Total, long Branch_Id_Entry, int IS_Make_AutomaticFromTar7eel, string Doc_No, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id)
        {
            Make_AccountEntry_(ref Excp, ref Msg, ref Account_Entry_List_ID, false, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, Entry_Date, journal_Code, Currency_Id, Entry_Notes, Transform_Type, dt_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Doc_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id);

        }
        /// <summary>
        /// نفس Make_AccountEntry (معاملة + دفتر العميل/المورد) لكن رقم القيد يُخصَّص داخل المعاملة بقفل (EntryNumbering) ويُعاد فى Entry_No.
        /// إضافة جديدة لشاشات Razor للقيد اليدوى؛ لا تغيّر سلوك الدوال القائمة.
        /// </summary>
        public void Make_AccountEntry_LockedNo(ref int Excp, ref string Msg, ref long Account_Entry_List_ID, ref long Entry_No, long User_ID_Login, string User_Id_Login_Name, long Branch_ID_Login, DateTime Entry_Date, long journal_Code, long Currency_Id, string Entry_Notes, int Transform_Type, DataTable dt_Details, double Entry_Total, long Branch_Id_Entry, int IS_Make_AutomaticFromTar7eel, string Doc_No, long Entry_Currency_ID, string Entry_Currency_Name, decimal Factor)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                Account_Entry_List_ID = 0;
                Entry_No = BusinessLibShared.GeneralLedger.EntryNumbering.Next(opp, Entry_Date, journal_Code);
                Make_AccountEntry_(ref Excp, ref Msg, ref Account_Entry_List_ID, false, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, Entry_Date, journal_Code, Currency_Id, Entry_Notes, Transform_Type, dt_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Doc_No, Entry_Currency_ID, Entry_Currency_Name, Factor, 0);
                if (Excp == 0) SetCustomerSupplierAccount(ref Excp, ref opp, dt_Details, Account_Entry_List_ID);
                if (Excp == 0) opp.Commit(); else opp.Rollback();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                try { opp.Rollback(); } catch { }
            }
        }

        void SetCustomerSupplierAccount(ref int Excp, ref AccountModelsLib.operations opp, DataTable dt_Details, long Account_Entry_List_ID)
        {
            for (int x = 0; x < dt_Details.Rows.Count; x++)
            {
                long Submain_Account_ID = long.Parse(dt_Details.Rows[x]["Submain_Account_ID"].ToString());
                string Account_Name = dt_Details.Rows[x]["SubAccount_Name"].ToString();
                long Account_Entry_Details_Id = long.Parse(dt_Details.Rows[x]["Account_Entry_Details_Id"].ToString());

                long Account_Customer_Type_Value = int.Parse(dt_Details.Rows[x]["Account_Customer_Type_Value"].ToString());

                if (Account_Customer_Type_Value == Common.Customer_Account_Type)
                {
                    CustomersAccountCLS ca = new CustomersAccountCLS();
                    ca.Set_Customer_Balance_WithEntry_List(ref Excp, ref opp, Submain_Account_ID, Account_Entry_List_ID, Account_Entry_Details_Id);
                }
                if (Account_Customer_Type_Value == Common.Supplier_Account_Type)
                {
                    Suppliers_AccountsCls sp = new Suppliers_AccountsCls();
                    sp.Set_Supplier_Balance_WithEntry_List(ref Excp, ref opp, Submain_Account_ID, Account_Entry_List_ID, Account_Entry_Details_Id);
                }

            }
        }
        void DeleteCustomerSupplierAccount(ref int Excp, ref AccountModelsLib.operations opp, DataTable dt_Details, long Account_Entry_List_ID)
        {
            for (int x = 0; x < dt_Details.Rows.Count; x++)
            {
                //long Submain_Account_ID = long.Parse(dt_Details.Rows[x]["Submain_Account_ID"].ToString());
                //string Account_Name = dt_Details.Rows[x]["SubAccount_Name"].ToString();
                //long Account_Entry_Details_Id = long.Parse(dt_Details.Rows[x]["Account_Entry_Details_Id"].ToString());

                long Account_Customer_Type_Value = int.Parse(dt_Details.Rows[x]["Account_Customer_Type_Value"].ToString());

                if (Account_Customer_Type_Value == Common.Customer_Account_Type)
                {
                    CustomersAccountCLS ca = new CustomersAccountCLS();
                    ca.Delete_Customer_Balance_WithEntry_List(ref Excp, ref opp, Account_Entry_List_ID);
                }
                if (Account_Customer_Type_Value == Common.Supplier_Account_Type)
                {
                    Suppliers_AccountsCls sp = new Suppliers_AccountsCls();
                    sp.Delete_Supplier_Balance_WithEntry_List(ref Excp, ref opp, Account_Entry_List_ID);
                }

            }
        }
        void DeleteCustomerSupplierAccountForEntryDetails(ref int Excp, ref AccountModelsLib.operations opp, DataTable dt_Details, long Account_Entry_List_ID, long Account_Entry_DetailsIdForDelete)
        {
            for (int x = 0; x < dt_Details.Rows.Count; x++)
            {
                //long Submain_Account_ID = long.Parse(dt_Details.Rows[x]["Submain_Account_ID"].ToString());
                //string Account_Name = dt_Details.Rows[x]["SubAccount_Name"].ToString();
                //long Account_Entry_Details_Id = long.Parse(dt_Details.Rows[x]["Account_Entry_Details_Id"].ToString());
                long Account_Entry_DetailsIdForLoop = long.Parse(dt_Details.Rows[x]["Account_Entry_Details_Id"].ToString());

                if (Account_Entry_DetailsIdForDelete == Account_Entry_DetailsIdForLoop)
                {

                    long Account_Customer_Type_Value = int.Parse(dt_Details.Rows[x]["Account_Customer_Type_Value"].ToString());

                    if (Account_Customer_Type_Value == Common.Customer_Account_Type)
                    {
                        CustomersAccountCLS ca = new CustomersAccountCLS();
                        ca.Delete_Customer_Balance_With_Entry_Details(ref Excp, ref opp, Account_Entry_List_ID, Account_Entry_DetailsIdForDelete);
                    }
                    if (Account_Customer_Type_Value == Common.Supplier_Account_Type)
                    {
                        Suppliers_AccountsCls sp = new Suppliers_AccountsCls();
                        sp.Delete_Supplier_Balance_WithEntry_Details(ref Excp, ref opp, Account_Entry_List_ID, Account_Entry_DetailsIdForDelete);
                    }
                }

            }
        }
        void DeleteCustomerSupplierAccountForEntryDetails(ref int Excp, ref AccountModelsLib.operations opp, Account_Entry_Details accD, long Account_Entry_List_ID, long Account_Entry_DetailsIdForDelete)
        {

            //long Submain_Account_ID = long.Parse(dt_Details.Rows[x]["Submain_Account_ID"].ToString());
            //string Account_Name = dt_Details.Rows[x]["SubAccount_Name"].ToString();
            //long Account_Entry_Details_Id = long.Parse(dt_Details.Rows[x]["Account_Entry_Details_Id"].ToString());


            long Account_Customer_Type_Value = accD.Account_Customer_Type_Value ?? 0;

            if (Account_Customer_Type_Value == Common.Customer_Account_Type)
            {
                CustomersAccountCLS ca = new CustomersAccountCLS();
                ca.Delete_Customer_Balance_With_Entry_Details(ref Excp, ref opp, Account_Entry_List_ID, Account_Entry_DetailsIdForDelete);
            }
            if (Account_Customer_Type_Value == Common.Supplier_Account_Type)
            {
                Suppliers_AccountsCls sp = new Suppliers_AccountsCls();
                sp.Delete_Supplier_Balance_WithEntry_Details(ref Excp, ref opp, Account_Entry_List_ID, Account_Entry_DetailsIdForDelete);
            }

        }


        public void Make_AccountEntry(ref int Excp, ref string Msg, ref long Account_Entry_List_ID, long User_ID_Login, string User_Id_Login_Name, long Branch_ID_Login, long Entry_No, DateTime Entry_Date, long journal_Code, long Currency_Id, string Entry_Notes, int Transform_Type, DataTable dt_Details, double Entry_Total, long Branch_Id_Entry, int IS_Make_AutomaticFromTar7eel, string Doc_No, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                Account_Entry_List_ID = 0;
                bool Effect_Blance_Direct = false;
                Make_AccountEntry_(ref Excp, ref Msg, ref Account_Entry_List_ID, Effect_Blance_Direct, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, Entry_Date, journal_Code, Currency_Id, Entry_Notes, Transform_Type, dt_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Doc_No, Entry_Currency_ID, Entry_Currency_Name, Factor, Order_Money_Id);
                if (Excp == 0)
                {
                    SetCustomerSupplierAccount(ref Excp, ref opp, dt_Details, Account_Entry_List_ID);
                }

                if (Excp == 0)
                {
                    opp.Commit();
                    return;
                }
                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();
                return;
            }
        }
        public void Make_AccountEntry_Editable(ref int Excp, ref string Msg, long Account_Entry_List_ID, ArrayList arr_IdDelet, long User_ID_Login, string User_Id_Login_Name, long Branch_ID_Login, long Entry_No, DateTime Entry_Date, long journal_Code, long Currency_Id, string Entry_Notes, int Transform_Type, DataTable dt_Details, double Entry_Total, long Branch_Id_Entry, int IS_Make_AutomaticFromTar7eel, string Doc_No, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                //Entry_Currency_ID,Entry_Currency_Name,Factor
                bool Effect_Blance_Direct = false;
                Make_AccountEntry_Edit(ref Excp, ref Msg, Account_Entry_List_ID, arr_IdDelet, Effect_Blance_Direct, ref opp, User_ID_Login, User_Id_Login_Name, Branch_ID_Login, Entry_No, Entry_Date, journal_Code, Currency_Id, Entry_Notes, Transform_Type, dt_Details, Entry_Total, Branch_Id_Entry, IS_Make_AutomaticFromTar7eel, Doc_No, Entry_Currency_ID, Entry_Currency_Name, Factor);



                if (Excp == 0)
                {
                    opp.Commit();
                    return;
                }
                if (Excp == 1)
                {
                    opp.Rollback();
                    return;
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();
                return;
            }
        }



        /////////////////////////////////////////////////////
        /*
          public void Delete_EntryFromAutomatic1(ref AccountModelsLib.operations opp, ref int Excp, ref string Msg, long Reference_Type_Table_Id, int Reference_Type)
        {
            if (Reference_Type_Table_Id == 0 | Reference_Type == 0 | IsNotDeletePrevEntry == true)
            {
                return;
            }
            string mysql = "";
            Excp = 0;
            long Account_Entry_List_ID = 0;


            mysql = @"  SELECT Account_Entry_List.* FROM  Account_Entry_List ";
            mysql = mysql + " WHERE(Reference_Type_Table_Id = " + Reference_Type_Table_Id + ") AND(Reference_Type = " + Reference_Type + ") ";
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count == 1)
            {
                //Entry_No = long.Parse(dt.Rows[0]["Entry_No"].ToString());
                Account_Entry_List_ID = long.Parse(dt.Rows[0]["Id"].ToString());

                Delete_Account_Entry(ref Excp, ref Msg, ref opp, Account_Entry_List_ID);
                //mysql = @"DELETE FROM Account_Entry_Details Where Account_Entry_List_ID=" + Account_Entry_List_ID;
                //opp.insert_me(mysql);


                //mysql = @"DELETE FROM Account_Entry_List Where Id=" + Account_Entry_List_ID;
                //opp.insert_me(mysql);
            }

        }
       
         */
        public void Delete_EntryFromAutomaticWithRef(ref AccountModelsLib.operations opp, ref int Excp, ref string Msg, long Account_Entry_List_ID, ref long Entry_No)
        {
            Entry_No = 0;
            string mysql = "";
            Excp = 0;


            mysql = @"SELECT        Account_Entry_List.* FROM Account_Entry_List";
            mysql = mysql + " WHERE Id=" + Account_Entry_List_ID;
            //mysql = mysql + " WHERE (Account_Entry_List.IS_Make_AutomaticFromTar7eel = 1) AND  (Account_Entry_List.Order_Money_Id = " + Order_Money_Id + ") AND (Account_Entry_List.Doc_No = '" + Doc_No + "') ";
            //mysql = mysql + "  AND ((Account_Entry_List.journal_Code = " + Common.Journal_Code_ForSandouk_Sarf + ")    OR (Account_Entry_List.journal_Code = " + Common.Journal_Code_ForSandouk_Estlam + ")) ";


            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 1)
            {
                Excp = 1;
                Msg = "يجب تعديل القيد بطريقة يدويه نظرا لحدوث توافق مع اكثر من قيد لبيانات الاذن";
                return;
            }
            if (dt.Rows.Count == 0)
            {
                Excp = 1;
                Msg = "يجب تعديل القيد بطريقة يدويه نظرا لعدم تمكن البحث الالى من تعديل القيد بطريقة الية";
                return;
            }
            if (dt.Rows.Count == 1)
            {
                Entry_No = long.Parse(dt.Rows[0]["Entry_No"].ToString());
                Account_Entry_List_ID = long.Parse(dt.Rows[0]["Id"].ToString());
            }

            Delete_Account_Entry(ref Excp, ref Msg, ref opp, Account_Entry_List_ID);
            //mysql = @"DELETE FROM Account_Entry_Details Where Account_Entry_List_ID=" + Account_Entry_List_ID;
            //opp.insert_me(mysql);


            //mysql = @"DELETE FROM Account_Entry_List Where Id=" + Account_Entry_List_ID;
            //opp.insert_me(mysql);


        }
        public void Delete_EntryFromAutomatic(ref AccountModelsLib.operations opp, ref int Excp, ref string Msg, long Reference_Type_Table_Id, int Reference_Type, bool IsNotDeletePrevEntry, long Account_Entry_List_ID = 0)
        {
            if (Reference_Type_Table_Id == 0 | Reference_Type == 0 | IsNotDeletePrevEntry == true)
            {
                return;
            }
            string mysql = "";
            Excp = 0;

            if (Account_Entry_List_ID == 0)
            {

                mysql = @"  SELECT Account_Entry_List.* FROM  Account_Entry_List ";
                mysql = mysql + " WHERE(Reference_Type_Table_Id = " + Reference_Type_Table_Id + ") AND(Reference_Type = " + Reference_Type + ") ";
                DataTable dt = opp.return_DataTable(mysql);

                if (dt.Rows.Count == 1)
                {
                    //Entry_No = long.Parse(dt.Rows[0]["Entry_No"].ToString());
                    Account_Entry_List_ID = long.Parse(dt.Rows[0]["Id"].ToString());
                    Entry_No = long.Parse(dt.Rows[0]["Entry_No"].ToString());
                    Delete_Account_Entry(ref Excp, ref Msg, ref opp, Account_Entry_List_ID);
                }
            }
            else
            {
                Delete_Account_Entry(ref Excp, ref Msg, ref opp, Account_Entry_List_ID);
            }



        }
        public void Delete_Account_Entry(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Account_Entry_List_ID)
        {

            string mysql = "";
            Excp = 0;


            mysql = @"DELETE FROM Account_Entry_Details Where Account_Entry_List_ID=" + Account_Entry_List_ID;
            opp.insert_me(mysql);


            mysql = @"DELETE FROM Account_Entry_List Where Id=" + Account_Entry_List_ID;
            opp.insert_me(mysql);


        }

        // //////////////////////////////////////////
        public void Make_DeletEntry_Editable(ref int Excp, ref string Msg, DataTable dt_Details, long Account_Entry_List_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
                Delete_Account_Entry(ref Excp, ref Msg, ref opp, Account_Entry_List_ID);
                //string mysql = "Delete From  Account_Entry_Details Where Account_Entry_List_ID= " + Account_Entry_List_ID;
                //opp.insert_me(mysql);

                //mysql = "Delete From  Account_Entry_List Where Id= " + Account_Entry_List_ID;
                //opp.insert_me(mysql);


                // SetCustomerSupplierAccount(ref Excp, ref opp, dt_Details, Account_Entry_List_ID);
                DeleteCustomerSupplierAccount(ref Excp, ref opp, dt_Details, Account_Entry_List_ID);
                if (Excp == 0)
                {
                    opp.Commit();
                    Excp = 0;

                }

                if (Excp == 1)
                {
                    opp.Rollback();
                    Excp = 1;
                }

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Msg = ee.Message;
                Excp = 1;

            }
        }





        public DateTime Transaction_DateFrom = new DateTime(1990, 1, 1);
        public DateTime Transaction_DateTo = new DateTime(1990, 1, 1);
        void Make_AccountEntry_(ref int Excp, ref string Msg, ref long Account_Entry_List_ID, bool Effect_Balance, ref AccountModelsLib.operations opp, long User_ID_Login, string User_Id_Login_Name, long Branch_ID_Login, long Entry_No, DateTime Entry_Date, long journal_Code, long Currency_Id, string Entry_Notes, int Transform_Type, DataTable dt_Details, double Entry_Total, long Branch_Id_Entry, int IS_Make_AutomaticFromTar7eel, string Doc_No, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor, long Order_Money_Id)
        {
           
            Delete_EntryFromAutomatic(ref opp, ref Excp, ref Msg, Reference_Type_Table_Id, Reference_Type, IsNotDeletePrevEntry, Account_Entry_List_ID);

            string mysql = "";
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Entry_Date);
            string Journal_Name = Common2.Journal_CodeName(ref opp, journal_Code);
            Account_Entry_List acl=  new Account_Entry_List();
            long ProcessNo = DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp, ref Msg, ref opp, acl);
            try
            {
                opp.cmd.Parameters.Clear();
                mysql = "INSERT INTO Account_Entry_List(ProcessNo,Factor,Entry_Currency_ID,Entry_Currency_Name,Currency_ID, Entry_No, Entry_Date, journal_Code,Journal_Name, Entry_Notes, Entry_Total,User_ID_Login, Branch_ID_Login,Transform_Type,Year_Financial_Period_Id,Branch_Id_Entry,Process_Date,IS_Make_AutomaticFromTar7eel,Doc_No,User_Id_Login_Name,Order_Money_Id,Reference_Type,Reference_Type_Table_Id,IsAutomaticEntryOrManualy)";
                mysql = mysql + " VALUES (@ProcessNo,@Factor,@Entry_Currency_ID,@Entry_Currency_Name,@Currency_ID,@Entry_No,@Entry_Date,@journal_Code,@Journal_Name,@Entry_Notes,@Entry_Total,@User_ID_Login,@Branch_ID_Login,@Transform_Type,@Year_Financial_Period_Id,@Branch_Id_Entry,@Process_Date,@IS_Make_AutomaticFromTar7eel,@Doc_No,@User_Id_Login_Name,@Order_Money_Id,@Reference_Type,@Reference_Type_Table_Id,@IsAutomaticEntryOrManualy)";

                opp.cmd.Parameters.AddWithValue("@ProcessNo", ProcessNo);
                opp.cmd.Parameters.AddWithValue("@Entry_Currency_ID", Entry_Currency_ID);
                opp.cmd.Parameters.AddWithValue("@Entry_Currency_Name", Entry_Currency_Name);

                opp.cmd.Parameters.AddWithValue("@Factor", Factor);
                opp.cmd.Parameters.AddWithValue("@Currency_ID", Currency_Id);
                opp.cmd.Parameters.AddWithValue("@Entry_No", Entry_No);
                opp.cmd.Parameters.AddWithValue("@Entry_Date", Entry_Date);
                opp.cmd.Parameters.AddWithValue("@journal_Code", journal_Code);
                opp.cmd.Parameters.AddWithValue("@Journal_Name", Journal_Name);
                opp.cmd.Parameters.AddWithValue("@Entry_Notes", Entry_Notes);
                opp.cmd.Parameters.AddWithValue("@Entry_Total", Entry_Total);
                opp.cmd.Parameters.AddWithValue("@User_ID_Login", User_ID_Login);
                opp.cmd.Parameters.AddWithValue("@Branch_ID_Login", Branch_ID_Login);
                opp.cmd.Parameters.AddWithValue("@Transform_Type", Transform_Type);
                opp.cmd.Parameters.AddWithValue("@Year_Financial_Period_Id", Year_Financial_Period_Id);
                opp.cmd.Parameters.AddWithValue("@Branch_Id_Entry", Branch_Id_Entry);
                opp.cmd.Parameters.AddWithValue("@Process_Date", DateTime.Now);
                opp.cmd.Parameters.AddWithValue("@IS_Make_AutomaticFromTar7eel", IS_Make_AutomaticFromTar7eel);
                opp.cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                opp.cmd.Parameters.AddWithValue("@User_Id_Login_Name", User_Id_Login_Name);
                opp.cmd.Parameters.AddWithValue("@Order_Money_Id", Order_Money_Id);
                opp.cmd.Parameters.AddWithValue("@Reference_Type", Reference_Type);
                opp.cmd.Parameters.AddWithValue("@Reference_Type_Table_Id", Reference_Type_Table_Id);
                opp.cmd.Parameters.AddWithValue("@IsAutomaticEntryOrManualy", IsAutomaticEntryOrManualy);


                

                Account_Entry_List_ID = opp.Insert_me_Return_Last_Identify(mysql);
                opp.cmd.Parameters.Clear();
                DateTime Order_Date = Entry_Date;
                string Order_Hijri_Date = "";// DataEntryAccountModelsLib.operations.GetHijriDate.GetConvertedDate(Entry_Date);

                //////////////loop for table with data
                for (int x = 0; x < dt_Details.Rows.Count; x++)
                {
                    long Submain_Account_ID = long.Parse(dt_Details.Rows[x]["Submain_Account_ID"].ToString());
                    long MainAccount_Id = long.Parse(dt_Details.Rows[x]["MainAccount_Id"].ToString());

                    string Account_Name = dt_Details.Rows[x]["SubAccount_Name"].ToString();
                    //mysql = "Select * from Submain_Account Where Id=" + Submain_Account_ID;
                    //DataTable dt = opp.return_DataTable(mysql);
                    long Account_Code = 0;

                    try
                    {
                        Account_Code =  long.Parse(dt_Details.Rows[x]["Account_Code"].ToString());
                    }
                    catch (Exception ee)
                    {
                         
                    }
                    long Account_Customer_Type_ID = long.Parse(dt_Details.Rows[x]["Account_Customer_Type_ID"].ToString());
                    long Account_Employee_Type_ID = 0;
                    //long.TryParse(dt_Details.Rows[x]["Account_Employee_Type_ID"].ToString(),out Account_Employee_Type_ID);

                    long Account_Customer_Type_Value = int.Parse(dt_Details.Rows[x]["Account_Customer_Type_Value"].ToString());
                    //long    Currency_Id = long.Parse(dt_Details.Rows[x]["Currency_Id"].ToString());
                    Factor = 1;//
                    try
                    {
                        decimal.Parse(dt_Details.Rows[x]["Factor"].ToString());
                    }
                    catch { }
                    int Transaction_Type = int.Parse(dt_Details.Rows[x]["Transaction_Type"].ToString());
                    decimal Import_Value = decimal.Parse(dt_Details.Rows[x]["Import_Value"].ToString());
                    decimal Export_Value = decimal.Parse(dt_Details.Rows[x]["Export_Value"].ToString());
                    decimal Transaction_Value = Import_Value + Export_Value;
                    decimal Equivalent_Currency_Value = decimal.Parse(dt_Details.Rows[x]["Equivalent_Currency_Value"].ToString());

                    decimal Equivalent_Import_Value = decimal.Parse(dt_Details.Rows[x]["Equivalent_Import_Value"].ToString());
                    decimal Equivalent_Export_Value = decimal.Parse(dt_Details.Rows[x]["Equivalent_Export_Value"].ToString());
                    decimal Equivalent_Transaction_Value = decimal.Parse(dt_Details.Rows[x]["Equivalent_Transaction_Value"].ToString());
                    long Account_Currency_ID = long.Parse(dt_Details.Rows[x]["Account_Currency_ID"].ToString());
                    Entry_Currency_ID = long.Parse(dt_Details.Rows[x]["Entry_Currency_ID"].ToString());


                    string Notes_Details = dt_Details.Rows[x]["Notes_Details"].ToString();
                    string CostCenter_Id = dt_Details.Rows[x]["CostCenter_Id"].ToString();
                    string CostCenter_Name = dt_Details.Rows[x]["CostCenter_Name"].ToString();
                    string Line_No = dt_Details.Rows[x]["Line_No"].ToString();


                    //long CarsWorkOrdersId= , long CarsId, long WorkOrderNumber,string CarNumber 

                    
                    long CarsWorkOrdersId = 0;
                    long CarsId = 0;
                    long WorkOrderNumber = 0;
                    string CarNumber = "";

                    try
                    {
                        CarsWorkOrdersId = long.Parse(dt_Details.Rows[x]["CarsWorkOrdersId"].ToString());
                        CarsId = long.Parse(dt_Details.Rows[x]["CarsId"].ToString());
                        WorkOrderNumber = long.Parse(dt_Details.Rows[x]["WorkOrderNumber"].ToString());
                        CarNumber = dt_Details.Rows[x]["CarNumber"].ToString();
                    }
                    catch (Exception ee)
                    { 
                    }


                    //int Account_Customer_Type_Value=int.Parse(dt_Details.Rows[x]["Account_Customer_Type_Value"].ToString());

                    //int Account_Customer_Type_ID=int.Parse(dt_Details.Rows[x]["Account_Customer_Type_ID"].ToString());
                    //int Account_Employee_Type_ID=int.Parse(dt_Details.Rows[x]["Account_Employee_Type_ID"].ToString());
                    //      
                    //string Account_Name = dt_Details.Rows[x]["SubAccount_Name"].ToString(); 

                    string Master_Submain_Account_Name = "";
                    string Master_Submain_Account_Id = "";

                    Get_ListAnotherAccountEntry(dt_Details, x, ref Master_Submain_Account_Name, ref Master_Submain_Account_Id);



                    ///Insert Into details
                    ///
                    opp.cmd.Parameters.Clear();
                    mysql = "INSERT INTO Account_Entry_Details(Account_Entry_List_ID, Submain_Account_ID,MainAccount_Id, Import_Value, Export_Value, Transaction_Value,Equivalent_Currency_Value, Transaction_Type, Notes_Details,Account_Customer_Type_Value,Account_Customer_Type_ID,Account_Code,Equivalent_Import_Value,Equivalent_Export_Value,Account_Currency_ID,Entry_Currency_ID,  Transaction_DateFrom,Transaction_DateTo,CostCenter_Id,CostCenter_Name,Line_No,Account_Name,Master_Submain_Account_Id,Master_Submain_Account_Name,Equivalent_Transaction_Value,Factor,CarsWorkOrdersId, CarsId,WorkOrderNumber,CarNumber, DataDictionaryProcessesListsId, Reference_Type_Table_Id) ";
                    mysql = mysql + " VALUES (@Account_Entry_List_ID,@Submain_Account_ID,@MainAccount_Id,@Import_Value,@Export_Value,@Transaction_Value,@Equivalent_Currency_Value,@Transaction_Type,@Notes_Details,@Account_Customer_Type_Value,@Account_Customer_Type_ID,@Account_Code,@Equivalent_Import_Value,@Equivalent_Export_Value,@Account_Currency_ID,@Entry_Currency_ID,@Transaction_DateFrom,@Transaction_DateTo,@CostCenter_Id,@CostCenter_Name,@Line_No,@Account_Name,@Master_Submain_Account_Id,@Master_Submain_Account_Name,@Equivalent_Transaction_Value,@Factor,@CarsWorkOrdersId,@CarsId,@WorkOrderNumber,@CarNumber, @DataDictionaryProcessesListsId, @Reference_Type_Table_Id)";

                   
                    opp.cmd.Parameters.AddWithValue("@Account_Entry_List_ID", Account_Entry_List_ID);
                    opp.cmd.Parameters.AddWithValue("@Submain_Account_ID", Submain_Account_ID);
                    opp.cmd.Parameters.AddWithValue("@MainAccount_Id", MainAccount_Id);
                    
                    opp.cmd.Parameters.AddWithValue("@Account_Name", Account_Name);


                    opp.cmd.Parameters.AddWithValue("@Import_Value", Import_Value);
                    opp.cmd.Parameters.AddWithValue("@Export_Value", Export_Value);
                    opp.cmd.Parameters.AddWithValue("@Transaction_Value", Transaction_Value);
                    ///////////////////////
                    opp.cmd.Parameters.AddWithValue("@Equivalent_Import_Value", Equivalent_Import_Value);
                    opp.cmd.Parameters.AddWithValue("@Equivalent_Export_Value", Equivalent_Export_Value);
                    opp.cmd.Parameters.AddWithValue("@Equivalent_Transaction_Value", Equivalent_Transaction_Value);



                    opp.cmd.Parameters.AddWithValue("@Account_Currency_ID", Account_Currency_ID);
                    opp.cmd.Parameters.AddWithValue("@Entry_Currency_ID", Entry_Currency_ID);
                    opp.cmd.Parameters.AddWithValue("@CostCenter_Id", @CostCenter_Id);
                    opp.cmd.Parameters.AddWithValue("@CostCenter_Name", CostCenter_Name);
                    opp.cmd.Parameters.AddWithValue("@Line_No", Line_No);


                    //////////////////
                    if (Transaction_DateFrom < new DateTime(2000, 1, 1))
                    {
                        opp.cmd.Parameters.AddWithValue("@Transaction_DateFrom", DBNull.Value);
                        opp.cmd.Parameters.AddWithValue("@Transaction_DateTo", DBNull.Value);
                    }
                    else
                    {
                        opp.cmd.Parameters.AddWithValue("@Transaction_DateFrom", Transaction_DateFrom);
                        opp.cmd.Parameters.AddWithValue("@Transaction_DateTo", Transaction_DateTo);
                    }


                    opp.cmd.Parameters.AddWithValue("@Equivalent_Currency_Value", Equivalent_Currency_Value);
                    opp.cmd.Parameters.AddWithValue("@Transaction_Type", Transaction_Type);
                    opp.cmd.Parameters.AddWithValue("@Notes_Details", Notes_Details);

                    opp.cmd.Parameters.AddWithValue("@Account_Customer_Type_Value", Account_Customer_Type_Value);
                    opp.cmd.Parameters.AddWithValue("@Account_Customer_Type_ID", Account_Customer_Type_ID);
                    //opp.cmd.Parameters.AddWithValue("@Account_Employee_Type_ID", Account_Employee_Type_ID);
                    opp.cmd.Parameters.AddWithValue("@Account_Code", Account_Code);


                    opp.cmd.Parameters.AddWithValue("@Master_Submain_Account_Id", Master_Submain_Account_Id);
                    opp.cmd.Parameters.AddWithValue("@Master_Submain_Account_Name", Master_Submain_Account_Name);

                    opp.cmd.Parameters.AddWithValue("@Factor", Factor);
                    opp.cmd.Parameters.AddWithValue("@CarsWorkOrdersId", CarsWorkOrdersId);
                    opp.cmd.Parameters.AddWithValue("@CarsId", CarsId);
                    opp.cmd.Parameters.AddWithValue("@WorkOrderNumber", WorkOrderNumber);
                    opp.cmd.Parameters.AddWithValue("@CarNumber", CarNumber);
                    opp.cmd.Parameters.AddWithValue("@DataDictionaryProcessesListsId", DataDictionaryProcessesListsId);
                    opp.cmd.Parameters.AddWithValue("@Reference_Type_Table_Id", Reference_Type_Table_Id);

                    long Account_Entry_Details_Id = opp.Insert_me_Return_Last_Identify(mysql);
                    //opp.insert_me(mysql);
                    opp.cmd.Parameters.Clear();

                    try
                    {
                        dt_Details.Rows[x]["Account_Entry_Details_Id"] = Account_Entry_Details_Id;
                    }
                    catch (Exception ee) { }
                    //////////////////////////////////////////
                    ///فى حالة انشاء قيد جديد وليس تحويل او قراءة قيد
                    ///

                    #region
                    if (Effect_Balance)
                    {
                        Excp = 0;
                        if (Transaction_Type == 1)
                        {

                            decimal Cach_Value = Import_Value;
                            Make_Transaction_Madin(ref Excp, ref opp, Account_Entry_List_ID, Transform_Type, Transaction_Type, Cach_Value, Order_Date, Order_Hijri_Date, Account_Currency_ID, Account_Customer_Type_Value, Account_Customer_Type_ID, Account_Employee_Type_ID, User_ID_Login, Branch_ID_Login, Notes_Details);
                            if (Excp == 1)
                            {
                                //opp.Rollback();
                                return;
                            }
                        }
                        else
                        {

                            decimal Cach_Value = Export_Value;
                            Make_Transaction_Da2n(ref Excp, ref opp, Account_Entry_List_ID, Transform_Type, Transaction_Type, Cach_Value, Order_Date, Order_Hijri_Date, Account_Currency_ID, Account_Customer_Type_Value, Account_Customer_Type_ID, Account_Employee_Type_ID, User_ID_Login, Branch_ID_Login, Notes_Details);
                            if (Excp == 1)
                            {
                                //opp.Rollback();
                                return;
                            }

                        }
                    }
                    #endregion

                }
                Msg = "";
                Excp = 0;
                return;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                return;
            }

        }
        public static void GetEntry_No(ref AccountModelsLib.operations opp, ref int Excp, ref string Msg, long Account_Entry_List_ID, ref long Entry_No)
        {

            string mysql = "";
            Excp = 0;

            if (Account_Entry_List_ID > 0)
            {

                mysql = @"  SELECT Account_Entry_List.* FROM  Account_Entry_List ";
                mysql = mysql + " WHERE (Id = " + Account_Entry_List_ID + ") ";
                DataTable dt = opp.return_DataTable(mysql);

                if (dt.Rows.Count == 1)
                {
                    //Entry_No = long.Parse(dt.Rows[0]["Entry_No"].ToString());
                    //Account_Entry_List_ID = long.Parse(dt.Rows[0]["Id"].ToString());
                    Entry_No = long.Parse(dt.Rows[0]["Entry_No"].ToString());

                }
            }




        }
        void Get_ListAnotherAccountEntry(DataTable dt_Details, int index, ref string Master_Submain_Account_Name, ref string Master_Submain_Account_Id)
        {
            //DataRow[] dr = new DataRow[dt_Details.Rows.Count - 1];
            //if (Transaction_Type == 1)
            //{
            //    dr = dt_Details.Select("Transaction_Type=2");
            //}
            //if (Transaction_Type == 2)
            //{
            //    dr = dt_Details.Select("Transaction_Type=1");
            //}

            ////if (dr.Length > 0)
            ////{
            ////    Master_Submain_Account_Name = dr[0]["SubAccount_Name"].ToString() ;
            ////    Master_Submain_Account_Id = dr[0]["Submain_Account_ID"].ToString();
            ////}

            ////for (int y = 1; y < dr.Length; y++)
            ////{
            ////    Master_Submain_Account_Name = Master_Submain_Account_Name + " - " + dr[y]["SubAccount_Name"].ToString();
            ////    Master_Submain_Account_Id = Master_Submain_Account_Id + " - " + dr[y]["Submain_Account_ID"].ToString();
            ////}



            for (int y = 0; y < dt_Details.Rows.Count; y++)
            {
                if (y == index)
                { continue; }

                int Transaction_Type_ = int.Parse(dt_Details.Rows[y]["Transaction_Type"].ToString());
                string Equivalent_Transaction_Value_ = "";

                if (Transaction_Type_ == 1)
                {
                    Equivalent_Transaction_Value_ = dt_Details.Rows[y]["Equivalent_Transaction_Value"].ToString();

                }
                else
                {
                    Equivalent_Transaction_Value_ = " { " + dt_Details.Rows[y]["Equivalent_Transaction_Value"].ToString() + " } ";

                }

                Master_Submain_Account_Name = Master_Submain_Account_Name + Equivalent_Transaction_Value_ + " - " + dt_Details.Rows[y]["SubAccount_Name"].ToString() + "\r\n";
                Master_Submain_Account_Id = Master_Submain_Account_Id + dt_Details.Rows[y]["Submain_Account_ID"].ToString();

                // if (y == 0)
                //{
                //    Master_Submain_Account_Name = dt_Details.Rows[y]["SubAccount_Name"].ToString() + Equivalent_Transaction_Value_ + " \r\n";
                //    Master_Submain_Account_Id = dt_Details.Rows[y]["Submain_Account_ID"].ToString();
                //}
                //else
                //{
                //    Master_Submain_Account_Name = Master_Submain_Account_Name + dt_Details.Rows[y]["SubAccount_Name"].ToString() + Equivalent_Transaction_Value_ + "\r\n";
                //    Master_Submain_Account_Id = Master_Submain_Account_Id + dt_Details.Rows[y]["Submain_Account_ID"].ToString();

                //}
            }


        }
        void Make_AccountEntry_Edit(ref int Excp, ref string Msg, long Account_Entry_List_ID, ArrayList arr_IdDelet, bool Effect_Balance, ref AccountModelsLib.operations opp, long User_ID_LoginUpdate, string User_Id_Login_Name, long Branch_ID_Login, long Entry_No, DateTime Entry_Date, long journal_Code, long Currency_Id, string Entry_Notes, int Transform_Type, DataTable dt_Details, double Entry_Total, long Branch_Id_Entry, int IS_Make_AutomaticFromTar7eel, string Doc_No, long    Entry_Currency_ID, string Entry_Currency_Name, decimal Factor)
        {
            DateTime ActionDate = DateTime.Now;
            string mysql = "";
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Entry_Date);
            string Journal_Name = Common2.Journal_CodeName(ref opp, journal_Code);
            try
            {
                opp.cmd.Parameters.Clear();
                mysql = "Update Account_Entry_List Set Year_Financial_Period_Id=@Year_Financial_Period_Id,Entry_Date=@Entry_Date, Currency_ID=@Currency_ID, Entry_No =@Entry_No, ActionDate=@ActionDate, journal_Code=@journal_Code ,Journal_Name=@Journal_Name, Entry_Notes =@Entry_Notes, Entry_Total=@Entry_Total,User_ID_LoginUpdate =@User_ID_LoginUpdate, Branch_ID_Login=@Branch_ID_Login,Transform_Type=@Transform_Type,   Branch_Id_Entry =@Branch_Id_Entry,Process_Date  =@Process_Date ,IS_Make_AutomaticFromTar7eel =@IS_Make_AutomaticFromTar7eel,Doc_No=@Doc_No,User_Id_Login_Name=@User_Id_Login_Name , Edit_User_Id=" + Common.User_Id + ", EditOperationDate=GETDATE() ";
                mysql = mysql + " , Entry_Currency_ID=@Entry_Currency_ID,Entry_Currency_Name=@Entry_Currency_Name,Factor =@Factor ";
                mysql = mysql + " Where Id=" + Account_Entry_List_ID;

                opp.cmd.Parameters.AddWithValue("@Entry_Currency_ID", Entry_Currency_ID);
                opp.cmd.Parameters.AddWithValue("@Entry_Currency_Name", Entry_Currency_Name);

                opp.cmd.Parameters.AddWithValue("@Factor", Factor);

                opp.cmd.Parameters.AddWithValue("@Currency_ID", Currency_Id);
                opp.cmd.Parameters.AddWithValue("@Entry_No", Entry_No);
                opp.cmd.Parameters.AddWithValue("@ActionDate", ActionDate);
                opp.cmd.Parameters.AddWithValue("@Entry_Date", Entry_Date);
                opp.cmd.Parameters.AddWithValue("@journal_Code", journal_Code);
                opp.cmd.Parameters.AddWithValue("@Journal_Name", Journal_Name);

                opp.cmd.Parameters.AddWithValue("@Entry_Notes", Entry_Notes);
                opp.cmd.Parameters.AddWithValue("@Entry_Total", Entry_Total);
                opp.cmd.Parameters.AddWithValue("@User_ID_LoginUpdate", User_ID_LoginUpdate);
                opp.cmd.Parameters.AddWithValue("@Branch_ID_Login", Branch_ID_Login);
                opp.cmd.Parameters.AddWithValue("@Transform_Type", Transform_Type);
                opp.cmd.Parameters.AddWithValue("@Year_Financial_Period_Id", Year_Financial_Period_Id);
                opp.cmd.Parameters.AddWithValue("@Branch_Id_Entry", Branch_Id_Entry);
                opp.cmd.Parameters.AddWithValue("@Process_Date", DateTime.Now);
                opp.cmd.Parameters.AddWithValue("@IS_Make_AutomaticFromTar7eel", IS_Make_AutomaticFromTar7eel);
                opp.cmd.Parameters.AddWithValue("@Doc_No", Doc_No);
                opp.cmd.Parameters.AddWithValue("@User_Id_Login_Name", User_Id_Login_Name);
                opp.cmd.Parameters.AddWithValue("@Account_Entry_List_ID", Account_Entry_List_ID);

                opp.insert_me(mysql);
                opp.cmd.Parameters.Clear();
                DateTime Order_Date = ActionDate;
                string Order_Hijri_Date = "";// DataEntryAccountModelsLib.operations.GetHijriDate.GetConvertedDate(Entry_Date);

                ///////Make Delete line
                for (int x = 0; x < arr_IdDelet.Count; x++)
                {
                    long Account_Entry_DetailsId = long.Parse(arr_IdDelet[x].ToString());


                    Account_Entry_Details account_Entry_Details = opp.db.Account_Entry_Details.AsNoTracking().Where(i => i.Id == Account_Entry_DetailsId).FirstOrDefault();
                    if (account_Entry_Details == null)
                    {
                        continue;
                    }
                    if (Excp == 0)
                    {
                        DeleteCustomerSupplierAccountForEntryDetails(ref Excp, ref opp, account_Entry_Details, Account_Entry_List_ID, Account_Entry_DetailsId);
                    }

                    mysql = "Delete From  Account_Entry_Details Where Id= " + Account_Entry_DetailsId;
                    int c = opp.insert_me(mysql);
                }


                //////////////loop for table with data
                for (int x = 0; x < dt_Details.Rows.Count; x++)
                {
                    long Id_Test = long.Parse(dt_Details.Rows[x]["Id_Test"].ToString());
                    int Transaction_Type = int.Parse(dt_Details.Rows[x]["Transaction_Type"].ToString());
                    if (Id_Test == 0) ///سطر بدون تعديل اى من القيد قبل التعديلات
                    {
                        long Account_Entry_Details_Id = long.Parse(dt_Details.Rows[x]["Account_Entry_Details_Id"].ToString());

                        Edit_AccountName(ref opp, dt_Details, Account_Entry_Details_Id, Transaction_Type);
                        continue;
                    }
                    long Submain_Account_ID = long.Parse(dt_Details.Rows[x]["Submain_Account_ID"].ToString());

                    string Account_Name = dt_Details.Rows[x]["SubAccount_Name"].ToString();
                    //mysql = "Select * from Submain_Account Where Id=" + Submain_Account_ID;
                    //DataTable dt = opp.return_DataTable(mysql);
                    long Account_Code = 0;
                    try
                    {
                        string Account_Code_ = dt_Details.Rows[x]["Account_Code"].ToString();
                        string SEP = Common.Account_Seperator_Char.ToString();
                        Account_Code_ = Account_Code_.Replace(SEP, "");
                        Account_Code = long.Parse(Account_Code_);
                    }
                    catch { }
                    long Account_Customer_Type_ID = long.Parse(dt_Details.Rows[x]["Account_Customer_Type_ID"].ToString());
                    long Account_Employee_Type_ID = 0;// long.Parse(dt_Details.Rows[x]["Account_Employee_Type_ID"].ToString());
                    long Account_Customer_Type_Value = long.Parse(dt_Details.Rows[x]["Account_Customer_Type_Value"].ToString());
                    //long    Currency_Id = long.Parse(dt_Details.Rows[x]["Currency_Id"].ToString());



                    decimal Import_Value = decimal.Parse(dt_Details.Rows[x]["Import_Value"].ToString());
                    decimal Export_Value = decimal.Parse(dt_Details.Rows[x]["Export_Value"].ToString());
                    decimal Transaction_Value = Import_Value + Export_Value;
                    decimal Equivalent_Currency_Value = decimal.Parse(dt_Details.Rows[x]["Equivalent_Currency_Value"].ToString());

                    decimal Equivalent_Import_Value = decimal.Parse(dt_Details.Rows[x]["Equivalent_Import_Value"].ToString());
                    decimal Equivalent_Export_Value = decimal.Parse(dt_Details.Rows[x]["Equivalent_Export_Value"].ToString());
                    decimal Equivalent_Transaction_Value = decimal.Parse(dt_Details.Rows[x]["Equivalent_Transaction_Value"].ToString());
                    long Account_Currency_ID = long.Parse(dt_Details.Rows[x]["Account_Currency_ID"].ToString());
                    //long Entry_Currency_ID = long.Parse(dt_Details.Rows[x]["Entry_Currency_ID"].ToString());


                    string Notes_Details = dt_Details.Rows[x]["Notes_Details"].ToString();
                    string CostCenter_Id = dt_Details.Rows[x]["CostCenter_Id"].ToString();
                    string CostCenter_Name = dt_Details.Rows[x]["CostCenter_Name"].ToString();
                    string Line_No = dt_Details.Rows[x]["Line_No"].ToString();

                    //int Account_Customer_Type_Value=int.Parse(dt_Details.Rows[x]["Account_Customer_Type_Value"].ToString());

                    //int Account_Customer_Type_ID=int.Parse(dt_Details.Rows[x]["Account_Customer_Type_ID"].ToString());
                    //int Account_Employee_Type_ID=int.Parse(dt_Details.Rows[x]["Account_Employee_Type_ID"].ToString());
                    //      
                    //string Account_Name = dt_Details.Rows[x]["SubAccount_Name"].ToString(); 

                    string Master_Submain_Account_Name = "";
                    string Master_Submain_Account_Id = "";

                    DataRow[] dr = new DataRow[dt_Details.Rows.Count - 1];
                    if (Transaction_Type == 1)
                    {
                        dr = dt_Details.Select("Transaction_Type=2");
                    }
                    if (Transaction_Type == 2)
                    {
                        dr = dt_Details.Select("Transaction_Type=1");
                    }

                    if (dr.Length > 0)
                    {
                        Master_Submain_Account_Name = dr[0]["SubAccount_Name"].ToString();
                        Master_Submain_Account_Id = dr[0]["Submain_Account_ID"].ToString();
                    }

                    for (int y = 1; y < dr.Length; y++)
                    {
                        Master_Submain_Account_Name = Master_Submain_Account_Name + " - " + dr[y]["SubAccount_Name"].ToString();
                        Master_Submain_Account_Id = Master_Submain_Account_Id + " - " + dr[y]["Submain_Account_ID"].ToString();
                    }





                    ///Insert Into details
                    ///
                    opp.cmd.Parameters.Clear();
                    mysql = "INSERT INTO Account_Entry_Details(Account_Entry_List_ID, Submain_Account_ID, Import_Value, Export_Value, Transaction_Value,Equivalent_Currency_Value, Transaction_Type, Notes_Details,Account_Customer_Type_Value,Account_Customer_Type_ID,Account_Code,Equivalent_Import_Value,Equivalent_Export_Value,Account_Currency_ID,Entry_Currency_ID,  Transaction_DateFrom,Transaction_DateTo,CostCenter_Id,CostCenter_Name,Line_No,Account_Name,Master_Submain_Account_Id,Master_Submain_Account_Name,Equivalent_Transaction_Value) ";
                    mysql = mysql + " VALUES (@Account_Entry_List_ID,@Submain_Account_ID,@Import_Value,@Export_Value,@Transaction_Value,@Equivalent_Currency_Value,@Transaction_Type,@Notes_Details,@Account_Customer_Type_Value,@Account_Customer_Type_ID,@Account_Code,@Equivalent_Import_Value,@Equivalent_Export_Value,@Account_Currency_ID,@Entry_Currency_ID,@Transaction_DateFrom,@Transaction_DateTo,@CostCenter_Id,@CostCenter_Name,@Line_No,@Account_Name,@Master_Submain_Account_Id,@Master_Submain_Account_Name,@Equivalent_Transaction_Value)";


                    opp.cmd.Parameters.AddWithValue("@Account_Entry_List_ID", Account_Entry_List_ID);
                    opp.cmd.Parameters.AddWithValue("@Submain_Account_ID", Submain_Account_ID);
                    opp.cmd.Parameters.AddWithValue("@Account_Name", Account_Name);


                    opp.cmd.Parameters.AddWithValue("@Import_Value", Import_Value);
                    opp.cmd.Parameters.AddWithValue("@Export_Value", Export_Value);
                    opp.cmd.Parameters.AddWithValue("@Transaction_Value", Transaction_Value);
                    ///////////////////////
                    opp.cmd.Parameters.AddWithValue("@Equivalent_Import_Value", Equivalent_Import_Value);
                    opp.cmd.Parameters.AddWithValue("@Equivalent_Export_Value", Equivalent_Export_Value);
                    opp.cmd.Parameters.AddWithValue("@Equivalent_Transaction_Value", Equivalent_Transaction_Value);



                    opp.cmd.Parameters.AddWithValue("@Account_Currency_ID", Account_Currency_ID);
                    opp.cmd.Parameters.AddWithValue("@Entry_Currency_ID", Entry_Currency_ID);
                    opp.cmd.Parameters.AddWithValue("@CostCenter_Id", @CostCenter_Id);
                    opp.cmd.Parameters.AddWithValue("@CostCenter_Name", CostCenter_Name);
                    opp.cmd.Parameters.AddWithValue("@Line_No", Line_No);


                    //////////////////
                    if (Transaction_DateFrom < new DateTime(2000, 1, 1))
                    {
                        opp.cmd.Parameters.AddWithValue("@Transaction_DateFrom", DBNull.Value);
                        opp.cmd.Parameters.AddWithValue("@Transaction_DateTo", DBNull.Value);
                    }
                    else
                    {
                        opp.cmd.Parameters.AddWithValue("@Transaction_DateFrom", Transaction_DateFrom);
                        opp.cmd.Parameters.AddWithValue("@Transaction_DateTo", Transaction_DateTo);
                    }


                    opp.cmd.Parameters.AddWithValue("@Equivalent_Currency_Value", Equivalent_Currency_Value);
                    opp.cmd.Parameters.AddWithValue("@Transaction_Type", Transaction_Type);
                    opp.cmd.Parameters.AddWithValue("@Notes_Details", Notes_Details);

                    opp.cmd.Parameters.AddWithValue("@Account_Customer_Type_Value", Account_Customer_Type_Value);
                    opp.cmd.Parameters.AddWithValue("@Account_Customer_Type_ID", Account_Customer_Type_ID);
                    //opp.cmd.Parameters.AddWithValue("@Account_Employee_Type_ID", Account_Employee_Type_ID);
                    opp.cmd.Parameters.AddWithValue("@Account_Code", Account_Code);


                    opp.cmd.Parameters.AddWithValue("@Master_Submain_Account_Id", Master_Submain_Account_Id);
                    opp.cmd.Parameters.AddWithValue("@Master_Submain_Account_Name", Master_Submain_Account_Name);




                    long Account_Entry_Details_Id2 = opp.Insert_me_Return_Last_Identify(mysql);

                    dt_Details.Rows[x]["Account_Entry_Details_Id"] = Account_Entry_Details_Id2;
                    opp.cmd.Parameters.Clear();

                    //////////////////////////////////////////
                    ///فى حالة انشاء قيد جديد وليس تحويل او قراءة قيد
                    ///

                    #region
                    if (Effect_Balance)
                    {
                        Excp = 0;
                        if (Transaction_Type == 1)
                        {

                            decimal Cach_Value = Import_Value;
                            Make_Transaction_Madin(ref Excp, ref opp, Account_Entry_List_ID, Transform_Type, Transaction_Type, Cach_Value, Order_Date, Order_Hijri_Date, Account_Currency_ID, Account_Customer_Type_Value, Account_Customer_Type_ID, Account_Employee_Type_ID, User_ID_LoginUpdate, Branch_ID_Login, Notes_Details);
                            if (Excp == 1)
                            {
                                //opp.Rollback();
                                return;
                            }
                        }
                        else
                        {

                            decimal Cach_Value = Export_Value;
                            Make_Transaction_Da2n(ref Excp, ref opp, Account_Entry_List_ID, Transform_Type, Transaction_Type, Cach_Value, Order_Date, Order_Hijri_Date, Account_Currency_ID, Account_Customer_Type_Value, Account_Customer_Type_ID, Account_Employee_Type_ID, User_ID_LoginUpdate, Branch_ID_Login, Notes_Details);
                            if (Excp == 1)
                            {
                                //opp.Rollback();
                                return;
                            }

                        }
                    }
                    #endregion

                }
                if (Excp == 0)
                {
                    SetCustomerSupplierAccount(ref Excp, ref opp, dt_Details, Account_Entry_List_ID);
                }
                Msg = "";
                Excp = 0;
                return;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                return;
            }

        }

        void Edit_AccountName(ref AccountModelsLib.operations opp, DataTable dt_Details, long Account_Entry_Details_Id, int Transaction_Type)
        {
            string Master_Submain_Account_Name = "";
            string Master_Submain_Account_Id = "";


            DataRow[] dr = new DataRow[dt_Details.Rows.Count - 1];
            if (Transaction_Type == 1)
            {
                dr = dt_Details.Select("Transaction_Type=2");
            }
            if (Transaction_Type == 2)
            {
                dr = dt_Details.Select("Transaction_Type=1");
            }

            if (dr.Length > 0)
            {
                Master_Submain_Account_Name = dr[0]["SubAccount_Name"].ToString();
                Master_Submain_Account_Id = dr[0]["Submain_Account_ID"].ToString();
            }

            for (int y = 1; y < dr.Length; y++)
            {
                Master_Submain_Account_Name = Master_Submain_Account_Name + " - " + dr[y]["SubAccount_Name"].ToString();
                Master_Submain_Account_Id = Master_Submain_Account_Id + " - " + dr[y]["Submain_Account_ID"].ToString();
            }

            string mysql = "Update Account_Entry_Details Set Master_Submain_Account_Id='" + Master_Submain_Account_Id + "'  , Master_Submain_Account_Name='" + Master_Submain_Account_Name + "', Edit_User_Id=" + Common.User_Id + ", EditOperationDate=GETDATE() Where Id=" + Account_Entry_Details_Id;
            opp.insert_me(mysql);

        }
        //مدين
        public void Make_Transaction_Madin(ref int Excp, ref AccountModelsLib.operations opp, long Account_Entry_List_ID, int Transform_Type, int Transaction_Type, decimal Cach_Value, DateTime Order_Date, string Order_Hijri_Date, long Currency_Id, long Customer_Type, long Account_Customer_ID, long User_Details_ID, long User_ID_Login, long Branch_ID_Login, string Notes)
        {



            try
            {
                string mysql = "";
                ArrayList arr = new ArrayList();

                decimal Balance_Air = 0;
                decimal Balance_Road = 0;


                string PaymentMethod = "Cash";

                string Transaction_Date = AccountModelsLib.operations.Get_DateTimeToInsert(Order_Date);// DataEntryAccountModelsLib.operations.AccountModelsLib.operations.Get_DateTimeToInsert(Order_Date);// DataEntryAccountModelsLib.operations.AccountModelsLib.operations.dateFormat(Order_Date);
                if (Transform_Type == 1) //طيران
                {
                    Balance_Air = Cach_Value;
                    Balance_Road = 0;
                }
                else
                {
                    Balance_Air = 0;
                    Balance_Road = Cach_Value;
                }




                if (Customer_Type == 1 | Customer_Type == 2 | Customer_Type == 3)
                {
                    mysql = "  UPDATE Customer SET Balance = Balance +  " + Cach_Value + " , Balance_Actual = Balance_Actual + " + Cach_Value + " ";
                    mysql = mysql + ", Balance_Air = Balance_Air + " + Balance_Air + "  , Balance_Road = Balance_Road + " + Balance_Road + " WHERE (Id = " + Account_Customer_ID + ")";
                    opp.insert_me(mysql);
                }
                if (Customer_Type == 4)
                {
                    mysql = "  UPDATE USER_DETAILES SET Balance = Balance + " + Cach_Value + " ,Balance_Actual=Balance_Actual +  " + Cach_Value + " ";
                    mysql = mysql + ",  Balance_Air = Balance_Air + " + Balance_Air + " ,  Balance_Road = Balance_Road + " + Balance_Road + " ";
                    mysql = mysql + "  WHERE (USER_ID = " + User_Details_ID + ")";
                    opp.insert_me(mysql);
                }
                if (Customer_Type == 5) // مورد
                {
                    mysql = "  UPDATE Supplier SET Balance = Balance +  " + Cach_Value + " , Balance_Actual = Balance_Actual + " + Cach_Value + " ";
                    mysql = mysql + ", Balance_Air = Balance_Air + " + Balance_Air + "  , Balance_Road = Balance_Road + " + Balance_Road + " WHERE (Id = " + Account_Customer_ID + ")";
                    opp.insert_me(mysql);
                }
                if (Customer_Type == 6) // الصندوق
                {
                    mysql = "  UPDATE Banks SET Last_Balance = Last_Balance +  " + Cach_Value + "   WHERE (Id = " + Account_Customer_ID + ")";
                    opp.insert_me(mysql);
                }


                //
                int PriceTypeFlage = 34; // 
                int Transaction_Type_Employee = 2;

                // وكيل - فرع
                if (Customer_Type == 1 | Customer_Type == 2)
                {

                    // وكيل - فرع
                    mysql = " INSERT INTO ReservationTicket_PriceTransaction ";
                    mysql = mysql + "(All_Import,Transform_Type,Account_Entry_List_ID, Transaction_Date, TransactionType, PriceTypeFlage, Notes,";
                    mysql = mysql + " Import, Export, Customer_Type, PaymentMethod, Currency_ID, ";
                    mysql = mysql + " User_ID, Branch_ID, ";
                    mysql = mysql + "  User_ID_Balance_Air, User_ID_Balance_Road, ";
                    mysql = mysql + " Branch_ID_Balance_Air, Branch_ID_Balance_Road) ";

                    //يتم وضع المبلغ فى الصادر    

                    mysql = mysql + " SELECT " + Cach_Value + "  as All_Import," + Transform_Type + " AS Transform_Type, " + Account_Entry_List_ID + " AS Account_Entry_List_ID," + Transaction_Date + " AS Transaction_Date," + Transaction_Type_Employee + "  AS TransactionType," + PriceTypeFlage + " as PriceTypeFlage,N'" + Notes + "' AS Notes ,";
                    mysql = mysql + "  " + Cach_Value + "   as Import, 0   as Export," + Customer_Type + " as Customer_Type,'" + PaymentMethod + "' AS PaymentMethod ," + Currency_Id + " AS Currency_ID ,";
                    //  mysql = mysql + " " + User_ID_login + " AS User_ID_login, " + Branch_ID_login + " AS Branch_ID_login ,";
                    mysql = mysql + " 0 AS User_ID_login,  Customer.ID AS  Customer_ID  ,";
                    mysql = mysql + " 0 AS USER_Balance_Air, 0 AS USER_Balance_Road , ";
                    mysql = mysql + " Customer.Balance_Air AS Customer_Balance_Air, Customer.Balance_Road AS Customer_Balance_Road ";
                    mysql = mysql + " FROM   Customer   ";
                    mysql = mysql + " Where Customer.ID=" + Account_Customer_ID + "";
                    opp.insert_me(mysql);
                }
                else if (Customer_Type == 3)
                {
                    //شركات السياحة
                    mysql = " INSERT INTO ReservationTicket_PriceTransaction ";
                    mysql = mysql + "(All_Import,Company_Tourism_ID,Transform_Type,Account_Entry_List_ID, Transaction_Date, TransactionType, PriceTypeFlage, Notes,";
                    mysql = mysql + " Import, Export, Customer_Type, PaymentMethod, Currency_ID, ";
                    //  mysql = mysql + " User_ID, Branch_ID, ";
                    mysql = mysql + "  User_ID_Balance_Air, User_ID_Balance_Road, ";
                    mysql = mysql + " Branch_ID_Balance_Air, Branch_ID_Balance_Road) ";

                    //يتم وضع المبلغ فى الصادر    

                    mysql = mysql + " SELECT " + Cach_Value + "  as All_Import," + Account_Customer_ID + " as Company_Tourism_ID," + Transform_Type + " AS Transform_Type, " + Account_Entry_List_ID + " AS Account_Entry_List_ID," + Transaction_Date + " AS Transaction_Date," + Transaction_Type_Employee + "  AS TransactionType," + PriceTypeFlage + " as PriceTypeFlage,N'" + Notes + "' AS Notes ,";
                    mysql = mysql + "  " + Cach_Value + "  as Import, 0   as Export," + Customer_Type + " as Customer_Type,'" + PaymentMethod + "' AS PaymentMethod ," + Currency_Id + " AS Currency_ID ,";
                    //   mysql = mysql + " " + User_ID_login + " AS User_ID_login, " + Branch_ID_login + " AS Branch_ID_login ,";
                    mysql = mysql + " 0 AS USER_Balance_Air, 0 AS USER_Balance_Road , ";
                    mysql = mysql + " Customer.Balance_Air AS Customer_Balance_Air, Customer.Balance_Road AS Customer_Balance_Road ";
                    mysql = mysql + " FROM   Customer   ";
                    mysql = mysql + " Where Customer.ID=" + Account_Customer_ID + "";
                    opp.insert_me(mysql);
                }
                //if (Customer_Type == 1 | Customer_Type == 2)
                else if (Customer_Type == 4)  //  موظف
                {
                    mysql = " INSERT INTO ReservationTicket_PriceTransaction ";
                    mysql = mysql + "(All_Import,Transform_Type,Account_Entry_List_ID, Transaction_Date, TransactionType, PriceTypeFlage, Notes,";
                    mysql = mysql + " Import, Export, Customer_Type, PaymentMethod, Currency_ID, ";
                    mysql = mysql + " User_ID, Branch_ID, ";
                    mysql = mysql + "  User_ID_Balance_Air, User_ID_Balance_Road, ";
                    mysql = mysql + " Branch_ID_Balance_Air, Branch_ID_Balance_Road) ";

                    //يتم وضع المبلغ فى الصادر للموظف

                    mysql = mysql + " SELECT " + Cach_Value + "  as All_Import, " + Transform_Type + " AS Transform_Type, " + Account_Entry_List_ID + " AS Account_Entry_List_ID," + Transaction_Date + "," + Transaction_Type_Employee + "  AS TransactionType," + PriceTypeFlage + " as PriceTypeFlage, N'" + Notes + "' AS Notes ,";
                    mysql = mysql + "  " + Cach_Value + "  as Import, 0  as Export," + Customer_Type + " as Customer_Type,'" + PaymentMethod + "' AS PaymentMethod ," + Currency_Id + " AS Currency_ID ,";
                    mysql = mysql + " USER_DETAILES.Id, USER_DETAILES.Branch_Id,";
                    mysql = mysql + " USER_DETAILES.Balance_Air AS USER_Balance_Air, USER_DETAILES.Balance_Road AS USER_Balance_Road , ";
                    mysql = mysql + " Customer.Balance_Air AS Customer_Balance_Air, Customer.Balance_Road AS Customer_Balance_Road ";
                    mysql = mysql + " FROM USER_DETAILES INNER JOIN Customer ON USER_DETAILES.Branch_Id = Customer.Id ";
                    mysql = mysql + " Where USER_DETAILES.Id=" + User_Details_ID + "";
                    opp.insert_me(mysql);
                }

                else if (Customer_Type == 5)  // موردين
                {
                }
                else if (Customer_Type == 6)  // الصندوق
                {
                }

                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;

            }

        }
        //دائن
        public void Make_Transaction_Da2n(ref int Excp, ref AccountModelsLib.operations opp, long Account_Entry_List_ID, int Transform_Type, int Transaction_Type, decimal Cach_Value, DateTime Order_Date, string Order_Hijri_Date, long Currency_Id, long Customer_Type, long Account_Customer_ID, long User_Details_ID, long User_ID_Login, long Branch_ID_Login, string Notes)
        {


            try
            {
                string mysql = "";
                ArrayList arr = new ArrayList();



                string PaymentMethod = "Cash";

                string Transaction_Date = AccountModelsLib.operations.Get_DateTimeToInsert(Order_Date);//DataEntryAccountModelsLib.operations.AccountModelsLib.operations.Get_DateTimeToInsert(Order_Date);// DataEntryAccountModelsLib.operations.AccountModelsLib.operations.dateFormat(Order_Date);
                decimal Balance_Air = 0;
                decimal Balance_Road = 0;

                if (Transform_Type == 1) //طيران
                {
                    Balance_Air = Cach_Value;
                    Balance_Road = 0;
                }
                else
                {
                    Balance_Air = 0;
                    Balance_Road = Cach_Value;
                }



                if (Customer_Type == 1 | Customer_Type == 2 | Customer_Type == 3)
                {
                    mysql = "  UPDATE Customer SET Balance = Balance -  " + Cach_Value + " , Balance_Actual = Balance_Actual - " + Cach_Value + " ";
                    mysql = mysql + ", Balance_Air = Balance_Air - " + Balance_Air + "  , Balance_Road = Balance_Road - " + Balance_Road + " WHERE (Id = " + Account_Customer_ID + ")";
                    opp.insert_me(mysql);
                }
                if (Customer_Type == 4)
                {
                    mysql = "  UPDATE USER_DETAILES SET Balance = Balance - " + Cach_Value + " ,Balance_Actual=Balance_Actual -  " + Cach_Value + " ";
                    mysql = mysql + ",  Balance_Air = Balance_Air - " + Balance_Air + " ,  Balance_Road = Balance_Road - " + Balance_Road + " ";
                    mysql = mysql + "  WHERE (USER_ID = " + User_Details_ID + ")";
                    opp.insert_me(mysql);
                }
                if (Customer_Type == 5) // مورد
                {
                    mysql = "  UPDATE Supplier SET Balance = Balance -  " + Cach_Value + " , Balance_Actual = Balance_Actual - " + Cach_Value + " ";
                    mysql = mysql + ", Balance_Air = Balance_Air - " + Balance_Air + "  , Balance_Road = Balance_Road - " + Balance_Road + " WHERE (Id = " + Account_Customer_ID + ")";
                    opp.insert_me(mysql);
                }
                if (Customer_Type == 6) // الصندوق
                {
                    mysql = "  UPDATE Banks SET Last_Balance = Last_Balance -  " + Cach_Value + "   WHERE (Id = " + Account_Customer_ID + ")";
                    opp.insert_me(mysql);
                }






                int PriceTypeFlage = 34;
                int Transaction_Type_Employee = 2;

                // وكيل - فرع
                if (Customer_Type == 1 | Customer_Type == 2)
                {
                    // وكيل - فرع
                    mysql = " INSERT INTO ReservationTicket_PriceTransaction ";
                    mysql = mysql + "(Transform_Type,Account_Entry_List_ID, Transaction_Date, TransactionType, PriceTypeFlage, Notes,";
                    mysql = mysql + " Import, Export, Customer_Type, PaymentMethod, Currency_ID, ";
                    mysql = mysql + " User_ID, Branch_ID, ";
                    mysql = mysql + "  User_ID_Balance_Air, User_ID_Balance_Road, ";
                    mysql = mysql + " Branch_ID_Balance_Air, Branch_ID_Balance_Road) ";

                    //يتم وضع المبلغ فى الصادر    

                    mysql = mysql + " SELECT " + Transform_Type + " AS Transform_Type, " + Account_Entry_List_ID + " AS Account_Entry_List_ID," + Transaction_Date + "," + Transaction_Type_Employee + "  AS TransactionType," + PriceTypeFlage + " as PriceTypeFlage,N'" + Notes + "' AS Notes ,";
                    mysql = mysql + "  0   as Import, " + Cach_Value + "   as Export," + Customer_Type + " as Customer_Type,'" + PaymentMethod + "' AS PaymentMethod ," + Currency_Id + " AS Currency_ID ,";
                    //  mysql = mysql + " " + User_ID_login + " AS User_ID_login, " + Branch_ID_login + " AS Branch_ID_login ,";
                    mysql = mysql + " 0 AS User_ID_login,  Customer.ID AS  Customer_ID  ,";
                    mysql = mysql + " 0 AS USER_Balance_Air, 0 AS USER_Balance_Road , ";
                    mysql = mysql + " Customer.Balance_Air AS Customer_Balance_Air, Customer.Balance_Road AS Customer_Balance_Road ";
                    mysql = mysql + " FROM   Customer   ";
                    mysql = mysql + " Where Customer.ID=" + Account_Customer_ID + "";
                    opp.insert_me(mysql);
                }
                else if (Customer_Type == 3)
                {
                    //شركات السياحة
                    mysql = " INSERT INTO ReservationTicket_PriceTransaction ";
                    mysql = mysql + "(Company_Tourism_ID,Transform_Type,Account_Entry_List_ID, Transaction_Date, TransactionType, PriceTypeFlage, Notes,";
                    mysql = mysql + " Import, Export, Customer_Type, PaymentMethod, Currency_ID, ";
                    //  mysql = mysql + " User_ID, Branch_ID, ";
                    mysql = mysql + "  User_ID_Balance_Air, User_ID_Balance_Road, ";
                    mysql = mysql + " Branch_ID_Balance_Air, Branch_ID_Balance_Road) ";

                    //يتم وضع المبلغ فى الصادر    

                    mysql = mysql + " SELECT " + Account_Customer_ID + " as Company_Tourism_ID," + Transform_Type + " AS Transform_Type, " + Account_Entry_List_ID + " AS Account_Entry_List_ID," + Transaction_Date + "," + Transaction_Type_Employee + "  AS TransactionType," + PriceTypeFlage + " as PriceTypeFlage,N'" + Notes + "' AS Notes ,";
                    mysql = mysql + "  0   as Import, " + Cach_Value + "   as Export," + Customer_Type + " as Customer_Type,'" + PaymentMethod + "' AS PaymentMethod ," + Currency_Id + " AS Currency_ID ,";
                    //   mysql = mysql + " " + User_ID_login + " AS User_ID_login, " + Branch_ID_login + " AS Branch_ID_login ,";
                    mysql = mysql + " 0 AS USER_Balance_Air, 0 AS USER_Balance_Road , ";
                    mysql = mysql + " Customer.Balance_Air AS Customer_Balance_Air, Customer.Balance_Road AS Customer_Balance_Road ";
                    mysql = mysql + " FROM   Customer   ";
                    mysql = mysql + " Where Customer.ID=" + Account_Customer_ID + "";
                    opp.insert_me(mysql);
                }
                //if (Customer_Type == 1 | Customer_Type == 2)
                else if (Customer_Type == 4)
                {
                    mysql = " INSERT INTO ReservationTicket_PriceTransaction ";
                    mysql = mysql + "(Transform_Type,Account_Entry_List_ID, Transaction_Date, TransactionType, PriceTypeFlage, Notes,";
                    mysql = mysql + " Import, Export, Customer_Type, PaymentMethod, Currency_ID, ";
                    mysql = mysql + " User_ID, Branch_ID, ";
                    mysql = mysql + "  User_ID_Balance_Air, User_ID_Balance_Road, ";
                    mysql = mysql + " Branch_ID_Balance_Air, Branch_ID_Balance_Road) ";

                    //يتم وضع المبلغ فى الصادر للموظف

                    mysql = mysql + " SELECT " + Transform_Type + " AS Transform_Type, " + Account_Entry_List_ID + " AS Account_Entry_List_ID," + Transaction_Date + "," + Transaction_Type_Employee + "  AS TransactionType," + PriceTypeFlage + " as PriceTypeFlage,N'" + Notes + "' AS Notes ,";
                    mysql = mysql + "  0   as Import, " + Cach_Value + "   as Export," + Customer_Type + " as Customer_Type,'" + PaymentMethod + "' AS PaymentMethod ," + Currency_Id + " AS Currency_ID ,";
                    mysql = mysql + " USER_DETAILES.Id, USER_DETAILES.Branch_Id,";
                    mysql = mysql + " USER_DETAILES.Balance_Air AS USER_Balance_Air, USER_DETAILES.Balance_Road AS USER_Balance_Road , ";
                    mysql = mysql + " Customer.Balance_Air AS Customer_Balance_Air, Customer.Balance_Road AS Customer_Balance_Road ";
                    mysql = mysql + " FROM USER_DETAILES INNER JOIN Customer ON USER_DETAILES.Branch_Id = Customer.Id ";
                    mysql = mysql + " Where USER_DETAILES.Id=" + User_Details_ID + "";
                    opp.insert_me(mysql);
                }

                else if (Customer_Type == 5)
                {
                }
                else if (Customer_Type == 6)
                {
                }

                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;

            }

        }

        public DataTable Get_Entry_Account(long Account_Entry_List_Id)
        {
            string mysql = @" SELECT        0 AS Id_Test, Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Line_No, Account_Entry_List.Id, Account_Entry_List.Currency_ID AS EntryCurrency_ID, 
                         Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, Account_Entry_List.journal_Code, Submain_Account.Id, 
                         Submain_Account.SubAccount_Name, Submain_Account.Account_Code As Account_CodeStr,Submain_Account.Code AS Account_Code,  Submain_Account.Company_ID_For_GL, Submain_Account.Company_Name_For_GL, 
                         Submain_Account.Account_Customer_Type_Value, Submain_Account.Transform_Type, Submain_Account.Account_Customer_Type_ID, 
                          Submain_Account.Currency_Id, Submain_Account.Branch_ID, Account_Entry_List.Entry_Notes, 
                         Account_Entry_List.User_ID_Login, Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_Details.ID AS Account_Entry_Details_Id, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Submain_Account.SubAccount_NO, Account_Entry_Details.Import_Value, 
                         Account_Entry_Details.Export_Value, Account_Entry_Details.Transaction_Value, Account_Entry_Details.Notes_Details, 
                         Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo, Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, 
                         Account_Entry_Details.Equivalent_Transaction_Value, Account_Entry_Details.Account_Currency_ID, Account_Entry_Details.Entry_Currency_ID, 
                         Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Submain_Account.Main_Account_Id AS MainAccount_Id, '' AS MainAccount_Name, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, 
                         Account_Entry_List.Year_Financial_Period_Id, Account_Entry_List.Doc_No, Account_Entry_List.IS_Make_AutomaticFromTar7eel, Account_Entry_List.Factor, 
                         Account_Entry_List.Entry_Currency_Name, Account_Entry_List.Entry_Currency_ID AS Entry_Currency_Id_List, Account_Entry_List.Reference_Type, Account_Entry_List.Reference_Type_Table_Id 
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id ";
            mysql = mysql + " WHERE     (Account_Entry_Details.Account_Entry_List_ID = " + Account_Entry_List_Id + ")";
            //mysql = mysql + " ORDER BY Account_Entry_Details.Transaction_Type,Line_No ";
            mysql = mysql + " Order By   Account_Entry_Details.Transaction_Type ";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable Get_Account_Entry_List(long Branch_Id, long Submain_Account_ID, long Journal_Code, long Account_Currency_Id, long CostCenter_Id, bool IsdtFrom, DateTime dtFrom, bool IsdtTo, DateTime dtTo, bool ISdtEnterFrom, DateTime dtEnterFrom, bool ISdtEnterTo, DateTime dtEnterTo, long Entry_NoFrom, long Entry_NoTo, int Notes_Details_Search_Flage, string Notes_Details, bool Is_EquivalentEntryValue, int EntryValueBetweenFlage, int MadinD2nAll, decimal EntryValueFrom, decimal EntryValueTo, long User_ID_Login)
        {
            string mysql = @" SELECT DISTINCT 
                         Account_Entry_List.Id, Account_Entry_List.journal_Code, Account_Entry_List.Entry_Date, Account_Entry_List.Entry_No, Account_Entry_List.Currency_ID, Account_Entry_List.Rate, Account_Entry_List.Entry_Notes, 
                         Account_Entry_List.IS_Make_AutomaticFromTar7eel, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.dYear, Account_Entry_List.dMonth, Account_Entry_List.dDay, Account_Entry_List.SerialChar, 
                         Account_Entry_List.POHead_Id, Account_Entry_List.ActionType_Code, Account_Entry_List.Receipt_Id, Account_Entry_List.Company_Code, Account_Entry_List.DAT, Account_Entry_List.HDay, 
                         Account_Entry_List.HMonth, Account_Entry_List.HYear, Account_Entry_List.User_ID_Login, Account_Entry_List.Package_Code, Account_Entry_List.RefID, Account_Entry_List.IsProfitLossClose, 
                         Account_Entry_List.ActionDate, Account_Entry_List.Process_Date, Account_Entry_List.CreationUser_Code, Account_Entry_List.IsUpdated, Account_Entry_List.Doc_No, Account_Entry_List.User_Id_Login_Name, 
                         Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type, Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, 
                         Account_Entry_List.Transfer_To_Gl_DateTime, Account_Entry_List.Year_Financial_Period_Id, Account_Entry_List.IS_FirstBalance, Account_Entry_List.Status_Code, Account_Entry_List.Entry_Currency_ID, 
                         Account_Entry_List.Factor, Account_Entry_List.Order_Money_Id, Account_Entry_List.User_ID_LoginUpdate, USER_DETAILES.Name AS User_Name_Add, USER_DETAILES_1.Name AS User_Name_Update, 
                         Account_Entry_List.Journal_Name, Account_Entry_List.Entry_Currency_Name AS Currency_Name
FROM            Account_Entry_List INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID LEFT OUTER JOIN
                         USER_DETAILES ON Account_Entry_List.User_ID_Login = USER_DETAILES.Id LEFT OUTER JOIN
                         USER_DETAILES AS USER_DETAILES_1 ON Account_Entry_List.User_ID_LoginUpdate = USER_DETAILES_1.Id 
 
";
            mysql = mysql + " WHERE  Account_Entry_List.ID >=0 ";

            if (IsdtFrom == true)
            {
                mysql = mysql + " AND  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom.Date) + ")  ";
            }
            if (IsdtTo == true)
            {
                mysql = mysql + " AND    (Account_Entry_List.Entry_Date <  " + AccountModelsLib.operations.Get_DateToInsert(dtTo.Date.AddDays(1)) + ")";
            }

            if (ISdtEnterFrom == true)
            {
                mysql = mysql + "  AND  (Account_Entry_List.Process_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtEnterFrom.Date) + ")  ";

            }
            if (ISdtEnterTo == true)
            {
                mysql = mysql + "  AND     (Account_Entry_List.Process_Date <  " + AccountModelsLib.operations.Get_DateToInsert(dtEnterTo.Date.AddDays(1)) + ")";

            }

            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (User_ID_Login > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.User_ID_Login = " + User_ID_Login + ") ";
            }

            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ") ";
            }
            if (Account_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Account_Currency_Id + ") ";
            }
            if (CostCenter_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_Details.CostCenter_Id = " + CostCenter_Id + ") ";
            }
            if (Reference_Type_Table_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Reference_Type = " + Reference_Type + ") ";
                mysql = mysql + " AND (Account_Entry_List.Reference_Type_Table_Id = " + Reference_Type_Table_Id + ") ";
            }

            mysql = mysql + Get_CompareValues(Is_EquivalentEntryValue, EntryValueBetweenFlage, MadinD2nAll, EntryValueFrom, EntryValueTo);


            ///////////////////////////////////////////////////////
            if (Notes_Details.Trim() != "")
            {
                if (Notes_Details_Search_Flage == 1)
                {
                    mysql = mysql + " AND (Account_Entry_Details.Notes_Details LIKE  '" + Notes_Details.Trim() + "%') ";
                }
                if (Notes_Details_Search_Flage == 2)
                {
                    mysql = mysql + " AND (Account_Entry_Details.Notes_Details LIKE  '%" + Notes_Details.Trim() + "%') ";
                }
                if (Notes_Details_Search_Flage == 3)
                {
                    mysql = mysql + " AND (Account_Entry_Details.Notes_Details LIKE  '%" + Notes_Details.Trim() + "') ";
                }
            }



            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }


            //if (IS_Make_AutomaticFromTar7eel > 0)
            //{
            //    mysql = mysql + " AND (Account_Entry_List.IS_Make_AutomaticFromTar7eel = " + IS_Make_AutomaticFromTar7eel + ") ";
            //}


            if (Entry_NoFrom > 0)
            {
                mysql = mysql + "  AND  (Account_Entry_List.Entry_No  >= " + Entry_NoFrom + ")  ";

            }
            if (Entry_NoTo > 0)
            {
                mysql = mysql + "   AND  (Account_Entry_List.Entry_No  <= " + Entry_NoTo + ")";

            }
            mysql = mysql + " Order by Account_Entry_List.Entry_No ";
            //WHERE        (Account_Entry_Details.CostCenter_Id = 0) AND (Account_Entry_Details.Submain_Account_ID = 0)
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }

        string Get_CompareValues(bool Is_EquivalentEntryValue, int EntryValueBetweenFlage, int MadinD2nAll, decimal EntryValueFrom, decimal EntryValueTo)
        {
            string mysql = "";

            if (EntryValueBetweenFlage > 0)
            {

                if (Is_EquivalentEntryValue == false)
                {
                    if (MadinD2nAll == 1)
                    {
                        if (EntryValueBetweenFlage == 1)
                            mysql = mysql + " AND (Account_Entry_Details.Import_Value = " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 2)
                            mysql = mysql + " AND (Account_Entry_Details.Import_Value > " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 3)
                            mysql = mysql + " AND (Account_Entry_Details.Import_Value >= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 4)
                            mysql = mysql + " AND (Account_Entry_Details.Import_Value < " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 5)
                            mysql = mysql + " AND (Account_Entry_Details.Import_Value <= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 6)
                            mysql = mysql + " AND (Account_Entry_Details.Import_Value <> " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 7)
                            mysql = mysql + " AND (Account_Entry_Details.Import_Value >= " + EntryValueFrom + ") AND (Account_Entry_Details.Import_Value <= " + EntryValueTo + ")";
                    }
                    if (MadinD2nAll == 2)
                    {
                        if (EntryValueBetweenFlage == 1)
                            mysql = mysql + " AND (Account_Entry_Details.Export_Value = " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 2)
                            mysql = mysql + " AND (Account_Entry_Details.Export_Value > " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 3)
                            mysql = mysql + " AND (Account_Entry_Details.Export_Value >= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 4)
                            mysql = mysql + " AND (Account_Entry_Details.Export_Value < " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 5)
                            mysql = mysql + " AND (Account_Entry_Details.Export_Value <= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 6)
                            mysql = mysql + " AND (Account_Entry_Details.Export_Value <> " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 7)
                            mysql = mysql + " AND (Account_Entry_Details.Export_Value >= " + EntryValueFrom + ") AND (Account_Entry_Details.Export_Value <= " + EntryValueTo + ")";
                    }

                    if (MadinD2nAll == 3)
                    {
                        if (EntryValueBetweenFlage == 1)
                            mysql = mysql + " AND ((Account_Entry_Details.Export_Value = " + EntryValueFrom + ")  OR (Account_Entry_Details.Import_Value = " + EntryValueFrom + ") )";
                        if (EntryValueBetweenFlage == 2)
                            mysql = mysql + " AND ((Account_Entry_Details.Export_Value > " + EntryValueFrom + ") OR (Account_Entry_Details.Import_Value > " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 3)
                            mysql = mysql + " AND ((Account_Entry_Details.Export_Value >= " + EntryValueFrom + ") OR (Account_Entry_Details.Import_Value >= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 4)
                            mysql = mysql + " AND ((Account_Entry_Details.Export_Value < " + EntryValueFrom + ") OR (Account_Entry_Details.Import_Value < " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 5)
                            mysql = mysql + " AND ((Account_Entry_Details.Export_Value <= " + EntryValueFrom + ")OR (Account_Entry_Details.Import_Value <= " + EntryValueFrom + ")  ";
                        if (EntryValueBetweenFlage == 6)
                            mysql = mysql + " AND ((Account_Entry_Details.Export_Value <> " + EntryValueFrom + ")OR (Account_Entry_Details.Import_Value <> " + EntryValueFrom + ")  ";
                        if (EntryValueBetweenFlage == 7)
                            mysql = mysql + " AND (((Account_Entry_Details.Export_Value >= " + EntryValueFrom + ") AND (Account_Entry_Details.Export_Value <= " + EntryValueTo + ")) OR ((Account_Entry_Details.Import_Value >= " + EntryValueFrom + ") AND (Account_Entry_Details.Import_Value <= " + EntryValueTo + ")))  ";
                    }
                }
                else if (Is_EquivalentEntryValue == true)
                {

                    if (MadinD2nAll == 1)
                    {
                        if (EntryValueBetweenFlage == 1)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Import_Value = " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 2)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Import_Value > " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 3)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Import_Value >= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 4)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Import_Value < " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 5)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Import_Value <= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 6)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Import_Value <> " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 7)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Import_Value >= " + EntryValueFrom + ") AND (Account_Entry_Details.Equivalent_Import_Value <= " + EntryValueTo + ")";
                    }
                    if (MadinD2nAll == 2)
                    {
                        if (EntryValueBetweenFlage == 1)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Export_Value = " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 2)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Export_Value > " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 3)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Export_Value >= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 4)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Export_Value < " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 5)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Export_Value <= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 6)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Export_Value <> " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 7)
                            mysql = mysql + " AND (Account_Entry_Details.Equivalent_Export_Value >= " + EntryValueFrom + ") AND (Account_Entry_Details.Equivalent_Export_Value <= " + EntryValueTo + ")";
                    }

                    if (MadinD2nAll == 3)
                    {
                        if (EntryValueBetweenFlage == 1)
                            mysql = mysql + " AND ((Account_Entry_Details.Equivalent_Export_Value = " + EntryValueFrom + ")  OR (Account_Entry_Details.Equivalent_Import_Value = " + EntryValueFrom + ") )";
                        if (EntryValueBetweenFlage == 2)
                            mysql = mysql + " AND ((Account_Entry_Details.Equivalent_Export_Value > " + EntryValueFrom + ") OR (Account_Entry_Details.Equivalent_Import_Value > " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 3)
                            mysql = mysql + " AND ((Account_Entry_Details.Equivalent_Export_Value >= " + EntryValueFrom + ") OR (Account_Entry_Details.Equivalent_Import_Value >= " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 4)
                            mysql = mysql + " AND ((Account_Entry_Details.Equivalent_Export_Value < " + EntryValueFrom + ") OR (Account_Entry_Details.Equivalent_Import_Value < " + EntryValueFrom + ") ";
                        if (EntryValueBetweenFlage == 5)
                            mysql = mysql + " AND ((Account_Entry_Details.Equivalent_Export_Value <= " + EntryValueFrom + ")OR (Account_Entry_Details.Equivalent_Import_Value <= " + EntryValueFrom + ")  ";
                        if (EntryValueBetweenFlage == 6)
                            mysql = mysql + " AND ((Account_Entry_Details.Equivalent_Export_Value <> " + EntryValueFrom + ")OR (Account_Entry_Details.Equivalent_Import_Value <> " + EntryValueFrom + ")  ";
                        if (EntryValueBetweenFlage == 7)
                            mysql = mysql + " AND (((Account_Entry_Details.Equivalent_Export_Value >= " + EntryValueFrom + ") AND (Account_Entry_Details.Equivalent_Export_Value <= " + EntryValueTo + ")) OR ((Account_Entry_Details.Equivalent_Import_Value >= " + EntryValueFrom + ") AND (Account_Entry_Details.Equivalent_Import_Value <= " + EntryValueTo + ")))  ";
                    }
                }


            }

            // Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, 
            //             Account_Entry_Details.Import_Value, Account_Entry_Details.Export_Value

            //    mysql = mysql + " AND (Account_Entry_Details.CostCenter_Id = " + CostCenter_Id + ") ";
            //}
            //Is_EntryValueBetween,int MadinD2nAll
            //Is_EntryValue, decimal EntryValueFrom, decimal EntryValueTo
            return mysql;

        }
        //        public DataTable Get_Account_Entry_List(DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id)
        //        {
        //            string mysql = @" SELECT     Account_Entry_List.*, Currency.Currency_Name, Currency.Currency_Name_EN, Journal_Codes.Journal_Name
        //FROM         Account_Entry_List INNER JOIN
        //                      Currency ON Account_Entry_List.Currency_ID = Currency.Value INNER JOIN
        //                      Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code
        //";
        //            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
        //            if (Journal_Code > 0)
        //            {
        //                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
        //            }
        //            if (Branch_Id > 0)
        //            {
        //                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
        //            }

        //            AccountModelsLib.operations opp = new AccountModelsLib.operations();
        //            DataTable dt = opp.return_DataTable(mysql);

        //            return dt;
        //        }


        public DataTable Get_MasterAccount_Entry_List(DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long    Entry_Currency_Id, int Account_Type_Value, long SubAccount_Id, bool IscbShowAccountListWithTransaction)
        {
            DataTable dt = new DataTable();
            ///////
            string mysql = @"";
            if (IscbShowAccountListWithTransaction == true)
            {
                dt = Get_MasterAccount_Entry_ListWithTransaction(dtFrom, dtTo, Journal_Code, Branch_Id, Entry_Currency_Id, Account_Type_Value, SubAccount_Id);
            }
            else
            {
                dt = Get_MasterAll_Account_List(Branch_Id, Entry_Currency_Id, Account_Type_Value, SubAccount_Id);

            }
            //mysql = mysql + "  ORDER BY Submain_Account.Code ";

            return dt;
        }
        DataTable Get_MasterAccount_Entry_ListWithTransaction(DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long    Entry_Currency_Id, int Account_Type_Value, long SubAccount_Id)
        {
            ///////
            string mysql = @"";
            //Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name,
            //Account_Entry_Details.Account_Customer_Type_ID, Account_Entry_Details.Account_Customer_Type_Value
            mysql = @" SELECT DISTINCT 
                         Submain_Account.SubAccount_Name, Submain_Account.SubAccount_Name AS Account_Name ,  Submain_Account.Account_Code, Submain_Account.SubAccount_NO, 
                         Submain_Account.Main_Account_Id, Submain_Account.Id,Submain_Account.Id AS Submain_Account_ID,  Submain_Account.code, Submain_Account.AccountCode1, Submain_Account.AccountCode2, Submain_Account.AccountCode3, 
                         Submain_Account.AccountCode4, Submain_Account.AccountCode5, Submain_Account.AccountCode6
                        , Submain_Account.Account_Customer_Type_ID, Submain_Account.Account_Customer_Type_Value
FROM            Account_Entry_List INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id ";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Entry_Currency_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            }

            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (Account_Type_Value > 0)
            {
                mysql = mysql + " AND (Submain_Account.Account_Customer_Type_Value = " + Account_Type_Value + ") ";
            }
            //if (Currency_Id > 0)
            //{
            //    mysql = mysql + " AND (Submain_Account.Currency_Id = " + Currency_Id + ") ";
            //}
            if (SubAccount_Id > 0)
            {
                mysql = mysql + " AND  (Submain_Account.Id = " + SubAccount_Id + ") ";
            }
            mysql = mysql + @"  ORDER BY  Submain_Account.Account_Customer_Type_Value,Submain_Account.AccountCode1,Submain_Account.AccountCode2,Submain_Account.AccountCode3,Submain_Account.AccountCode4,Submain_Account.AccountCode5,Submain_Account.AccountCode6  ";

            //mysql = mysql + "  ORDER BY Submain_Account.Code ";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        DataTable Get_MasterAll_Account_List(long Branch_Id, long    Entry_Currency_ID, int Account_Type_Value, long SubAccount_Id)
        {
            string mysql = @"";

            mysql = @" SELECT DISTINCT SubAccount_Id AS Submain_Account_ID, SubAccount_Name, Account_Code, SubAccount_NO, Main_Account_Id, code, AccountCode1, AccountCode2, AccountCode3, AccountCode4, AccountCode5, AccountCode6
 ,Account_Customer_Type_ID, Account_Customer_Type_Value

FROM            Submain_Account
WHERE        (SubAccount_Id NOT IN
                             (SELECT        Main_Account_Id
                                FROM            Submain_Account AS Submain_Account_1))";

            //if (Entry_Currency_Id > 0)
            //{
            //    mysql = mysql + " AND (Account_Entry_List.Entry_Currency_Id = " + Entry_Currency_Id + ") ";
            //}

            //if (Branch_Id > 0)
            //{
            //    mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            //}
            if (Account_Type_Value > 0)
            {
                mysql = mysql + " AND (Submain_Account.Account_Customer_Type_Value = " + Account_Type_Value + ") ";
            }
            //if (Currency_Id > 0)
            //{
            //    mysql = mysql + " AND (Submain_Account.Currency_Id = " + Currency_Id + ") ";
            //}
            if (SubAccount_Id > 0)
            {
                mysql = mysql + " AND  (Submain_Account.Id = " + SubAccount_Id + ") ";
            }
            mysql = mysql + @"  ORDER BY  Submain_Account.AccountCode1,Submain_Account.AccountCode2,Submain_Account.AccountCode3,Submain_Account.AccountCode4,Submain_Account.AccountCode5,Submain_Account.AccountCode6  ";

            //mysql = mysql + "  ORDER BY Submain_Account.Code ";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }

        public DataTable Get_MasterAccount_CostCenter_Lis(DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long Currency_Id, int Account_Type_Value, long SubAccount_Id)
        {
            string mysql = @" SELECT DISTINCT Cost_Center_Tree.Name, Account_Entry_Details.CostCenter_Id, Cost_Center_Tree.Code_Str
FROM            Account_Entry_List INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Cost_Center_Tree ON Account_Entry_Details.CostCenter_Id = Cost_Center_Tree.Id

";
            mysql = mysql + " WHERE    (Account_Entry_Details.CostCenter_Id > 0) AND (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (Account_Type_Value > 0)
            {
                mysql = mysql + " AND (Submain_Account.Account_Customer_Type_Value = " + Account_Type_Value + ") ";
            }
            if (Currency_Id > 0)
            {
                mysql = mysql + " AND (Submain_Account.Currency_Id = " + Currency_Id + ") ";
            }
            if (SubAccount_Id > 0)
            {
                mysql = mysql + " AND  (Submain_Account.Id = " + SubAccount_Id + ") ";
            }

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable Get_MasterAccount_Entry_Details(DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long Submain_Account_ID)
        {
            string mysql = @" SELECT        Account_Entry_List.Currency_ID AS Expr1, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, Account_Entry_List.journal_Code, 
                         Account_Entry_List.Entry_Notes, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.User_ID_Login, Account_Entry_List.Branch_ID_Login, 
                         Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type AS Expr2, Account_Entry_List.IS_Transfer_To_Gl_Flage, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, Account_Entry_List.Year_Financial_Period_Id, 
                         Currency.Currency_Name, Currency.Currency_Name_EN, Journal_Codes.Journal_Name, Account_Entry_Details.ID, Account_Entry_Details.Account_Entry_List_ID, 
                         Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Account_Entry_Details.SubAccount_NO AS Expr3, 
                         Account_Entry_Details.Transaction_Value, Account_Entry_Details.Notes_Details, Account_Entry_Details.Equivalent_Currency_Value, 
                         Account_Entry_Details.Transaction_Type, Account_Entry_Details.Account_Code AS Expr6, Account_Entry_Details.Account_Customer_Type_Value AS Expr7, 
                         Account_Entry_Details.Account_Customer_Type_ID AS Expr8,  
                         Account_Entry_Details.Transaction_DateFrom, Account_Entry_Details.Transaction_DateTo, Account_Entry_Details.Equivalent_Import_Value, 
                         Account_Entry_Details.Equivalent_Export_Value, Account_Entry_Details.Equivalent_Balance_Import_Value, 
                         Account_Entry_Details.Equivalent_Balance_Export_Value, Account_Entry_Details.Account_Currency_ID, Account_Entry_Details.Entry_Currency_ID, 
                         Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Account_Entry_Details.Line_No, 
                         Account_Entry_Details.Master_Submain_Account_Id, Account_Entry_Details.Master_Submain_Account_Name, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.SubAccount_NO, 
                         Account_Entry_Details.Import_Value, Account_Entry_Details.Export_Value, Account_Entry_Details.Balance_Import_Value, 
                         Account_Entry_Details.Balance_Export_Value, Account_Entry_Details.Equivalent_Transaction_Value
FROM            Account_Entry_List INNER JOIN
                         Currency ON Account_Entry_List.Currency_ID = Currency.Value INNER JOIN
                         Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.ID = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            mysql = mysql + "  AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ")";


            mysql = mysql + "  ORDER BY Account_Entry_List.IS_FirstBalance DESC ,Account_Entry_Details.Id ASC";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            Common.Make_TableColumnSerial(ref dt, "Serial_");


            //////////إضافة الاجمالى
            decimal Equivalent_Import_Value = 0;
            decimal Equivalent_Export_Value = 0;
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                }
                catch { }


                try
                {
                    Equivalent_Import_Value = Equivalent_Import_Value + decimal.Parse(dt.Rows[x]["Equivalent_Import_Value"].ToString());
                }
                catch { }

                try
                {
                    Equivalent_Export_Value = Equivalent_Export_Value + decimal.Parse(dt.Rows[x]["Equivalent_Export_Value"].ToString());
                }
                catch { }
            }

            DataRow dr = dt.NewRow();
            dr["Equivalent_Import_Value"] = Equivalent_Import_Value;
            dr["Equivalent_Export_Value"] = Equivalent_Export_Value;
            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);
            return dt;
        }
        public void Get_FirstBalance_Period(long Year_Financial_Period_Id, long Branch_Id, long Submain_Account_ID, ref decimal First_Balance_Madin, ref decimal First_Balance_Da2n, ref decimal Equivalent_First_Balance_Madin, ref decimal Equivalent_First_Balance_Da2n)
        {
            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n
            string mysql = "";
            mysql = @" SELECT        First_Balance_Madin, First_Balance_Da2n, Equivalent_First_Balance_Madin, Equivalent_First_Balance_Da2n
FROM            Account_Entry_Master_Balance
";
            mysql = mysql + " WHERE        (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";

            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ") ";
            }
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_Master_Balance.Submain_Account_ID = " + Submain_Account_ID + ")";
            }
             
                mysql = mysql + " AND (Account_Entry_Master_Balance.Account_Currency_Id = " + Common.Entry_Currency_Id + ")";
            
            
            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            First_Balance_Madin = 0;
            First_Balance_Da2n = 0;
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                try
                {
                    First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["First_Balance_Madin"].ToString());
                }
                catch { }

                try
                {
                    First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["First_Balance_Da2n"].ToString());
                }
                catch { }
                try
                {
                    Equivalent_First_Balance_Madin = Equivalent_First_Balance_Madin + decimal.Parse(dt.Rows[x]["Equivalent_First_Balance_Madin"].ToString());
                }
                catch { }

                try
                {
                    Equivalent_First_Balance_Da2n = Equivalent_First_Balance_Da2n + decimal.Parse(dt.Rows[x]["Equivalent_First_Balance_Da2n"].ToString());
                }
                catch { }
            }


        }

        public void Get_FirstBalance_PeriodNew(long Year_Financial_Period_Id, long Branch_Id, long Submain_Account_ID, ref decimal First_Balance_Madin, ref decimal First_Balance_Da2n, long Entry_Currency_ID)
        {
            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n
            string mysql = "";
            mysql = @" SELECT        First_Balance_Madin, First_Balance_Da2n, Equivalent_First_Balance_Madin, Equivalent_First_Balance_Da2n
FROM            Account_Entry_Master_Balance
";
            mysql = mysql + " WHERE        (Account_Entry_Master_Balance.Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            mysql = mysql + " AND (Account_Entry_Master_Balance.Equivalent_Account_Currency_Id = " + Entry_Currency_ID + ") ";
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_Master_Balance.Branch_Id = " + Branch_Id + ") ";
            }
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_Master_Balance.Submain_Account_ID = " + Submain_Account_ID + ")";
            }



            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            First_Balance_Madin = 0;
            First_Balance_Da2n = 0;
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                try
                {
                    First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["First_Balance_Madin"].ToString());
                }
                catch { }

                try
                {
                    First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["First_Balance_Da2n"].ToString());
                }
                catch { }

            }


        }

        public void Get_FirstBalance_PeriodNewCostCenter(long Year_Financial_Period_Id, long Branch_Id, long Cost_Center_Id, long Submain_Account_ID, ref decimal First_Balance_Madin, ref decimal First_Balance_Da2n, long Entry_Currency_ID)
        {
            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n
            string mysql = "";
            mysql = @" SELECT        SUM(First_Balance_Madin) AS First_Balance_Madin, SUM(First_Balance_Da2n) AS First_Balance_Da2n, SUM(Equivalent_First_Balance_Madin) AS Equivalent_First_Balance_Madin, 
                         SUM(Equivalent_First_Balance_Da2n) AS Equivalent_First_Balance_Da2n 
FROM            Account_Entry_Master_Balance_CostCenter
";
            mysql = mysql + " WHERE        ( Year_Financial_Period_Id = " + Year_Financial_Period_Id + ") ";
            mysql = mysql + " AND ( Equivalent_Account_Currency_Id = " + Entry_Currency_ID + ") ";
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND ( Branch_Id = " + Branch_Id + ") ";
            }
            //if (Cost_Center_Id > 0)
            //{
            //    mysql = mysql + " AND (Cost_Center_Id = " + Cost_Center_Id + ")";
            //}

            //if (Submain_Account_ID > 0)
            //{
            //    mysql = mysql + " AND (Submain_Account_ID = " + Submain_Account_ID + ")";
            //}

            if (Cost_Center_Id > 0)
            {
                mysql = mysql + " AND (Submain_Account_ID = " + Cost_Center_Id + ")";
            }

            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND (Submain_Account_ID = " + Submain_Account_ID + ")";
            }


            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            First_Balance_Madin = 0;
            First_Balance_Da2n = 0;
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                try
                {
                    First_Balance_Madin = First_Balance_Madin + decimal.Parse(dt.Rows[x]["First_Balance_Madin"].ToString());
                }
                catch { }

                try
                {
                    First_Balance_Da2n = First_Balance_Da2n + decimal.Parse(dt.Rows[x]["First_Balance_Da2n"].ToString());
                }
                catch { }

            }


        }
        //public DataTable Get_TransactionEntry()
        //{ }
        public DataTable Get_MasterAccount_Entry_Details_TarseedNew(long Year_Financial_Period_Id, DateTime Financial_Period_dtFom, DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long    Entry_Currency_ID, long Submain_Account_ID)
        {
          

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = Get_MasterAccount_Entry_Details_Data(ref opp, dtFrom, dtTo, Entry_Currency_ID, Journal_Code, Branch_Id, Submain_Account_ID);


            string Raseed_Awel_Moda = "رصيد أول المده";
            ///////////////الحصول على اجمالى الحركات السابقه واضافتها على رصيد اول المده
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;


            Mezan_MorgaaCLS mm = new Mezan_MorgaaCLS();

            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;

            if (dtFrom > Financial_Period_dtFom)
            {
                //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                DateTime New_dtTo = dtFrom.Subtract(ts);
                //DataTable dt_Acc = mm.Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID,0);
                DataTable dt_Prev = ComputeFromPeriodStartToSearchNew(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, Journal_Code, Entry_Currency_ID);
                if (dt_Prev.Rows.Count > 0)
                {

                    Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import_Value"].ToString());
                    Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export_Value"].ToString());

                    Raseed_Awel_Moda = "رصيد منقول";
                }
                
            }
            //حساب رصيد أول المده
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;


            Get_FirstBalance_PeriodNew(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref First_Balance_Madin, ref First_Balance_Da2n, Entry_Currency_ID);

            //إضافة رصيد اول المده على الرصيد المنقول
            decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            if (First_Balance == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;



            }
            else if (First_Balance > 0)
            {
                Final_First_Balance_Madin = First_Balance;
                Final_First_Balance_Da2n = 0;


            }
            else if (First_Balance < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = First_Balance * -1;


            }

            /////////////حساب رصيد الحركات
            Set_ComputeBalanceNew(ref dt, First_Balance);

            /////////////// إضافة رصيد أول المده او الرصيد المنقول
            DataRow dr1 = dt.NewRow();
            dr1["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr1["Balance_Export_Value"] = Final_First_Balance_Da2n;

            dr1["Notes_Details"] = Raseed_Awel_Moda;
            dt.Rows.InsertAt(dr1, 0);




            Common.Make_TableColumnSerialME(ref dt);


            //////////إضافة الاجمالى
            decimal Import_Value = 0;
            decimal Export_Value = 0;


            decimal Balance_Import_Value = 0;
            decimal Balance_Export_Value = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }



            }

            DataRow dr = dt.NewRow();

            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);

            //////////////////حساب الرصيد المنقول
            decimal Balance_Mankool = Import_Value - Export_Value;

            Balance_Mankool = Balance_Mankool + First_Balance;
            Balance_Mankool = Common.RoundMe(Balance_Mankool);





            if (Balance_Mankool == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool > 0)
            {
                Final_First_Balance_Madin = Balance_Mankool;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = Balance_Mankool * -1;
            }



            /////////////////////
            DataRow dr2 = dt.NewRow();
            dr2["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr2["Balance_Export_Value"] = Final_First_Balance_Da2n;


            dr2["Notes_Details"] = "رصيد منقول";

            dt.Rows.Add(dr2);

            return dt;
        }

        public DataTable Get_MasterAccount_Entry_Details_Tarseed(long Year_Financial_Period_Id, DateTime Financial_Period_dtFom, DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long Submain_Account_ID)
        {
            string mysql = @" SELECT        '' AS Serial_, Account_Entry_List.Currency_ID AS Account_Entry_Currency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Account_Entry_List.Entry_Notes, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.User_ID_Login, 
                         Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type AS Account_Entry_Transform_Type, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, 
                         Account_Entry_List.Year_Financial_Period_Id, Currency.Currency_Name, Currency.Currency_Name_EN, Journal_Codes.Journal_Name, Account_Entry_Details.ID, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 
                         Account_Entry_Details.SubAccount_NO AS Account_Entry_Details_SubAccount_NO, Account_Entry_Details.Transaction_Value, Account_Entry_Details.Notes_Details, 
                         Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, 
                         Account_Entry_Details.Account_Code AS Account_Entry_Details_Account_Code, 
                         Account_Entry_Details.Account_Customer_Type_Value AS Account_Entry_Details_Account_Customer_Type_Value, 
                         Account_Entry_Details.Account_Customer_Type_ID AS Account_Entry_Details_Account_Customer_Type_ID, 
                           Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo, Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, 
                         Account_Entry_Details.Equivalent_Balance_Import_Value, Account_Entry_Details.Equivalent_Balance_Export_Value, Account_Entry_Details.Account_Currency_ID, 
                         Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Account_Entry_Details.Line_No, 
                         Account_Entry_Details.Master_Submain_Account_Id, Account_Entry_Details.Master_Submain_Account_Name, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.SubAccount_NO, 
                         Account_Entry_Details.Import_Value, Account_Entry_Details.Export_Value, Account_Entry_Details.Balance_Import_Value, 
                         Account_Entry_Details.Balance_Export_Value, Account_Entry_Details.Equivalent_Transaction_Value
FROM            Account_Entry_List INNER JOIN
                         Currency ON Account_Entry_List.Currency_ID = Currency.Value INNER JOIN
                         Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            mysql = mysql + "  AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ")";
            mysql = mysql + " AND (Account_Entry_List.Entry_No   IS NOT NULL) ";

            // mysql = mysql + "  ORDER BY Account_Entry_List.IS_FirstBalance DESC ,Account_Entry_Details.Id ASC";
            mysql = mysql + "   ORDER BY Process_Date ";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);



            string Raseed_Awel_Moda = "رصيد أول المده";
            ///////////////الحصول على اجمالى الحركات السابقه واضافتها على رصيد اول المده
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;
            decimal Equivalent_Final_First_Balance_Madin = 0;
            decimal Equivalent_Final_First_Balance_Da2n = 0;

            Mezan_MorgaaCLS mm = new Mezan_MorgaaCLS();

            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;
            decimal Prev_Equivalent_Import_Value = 0;
            decimal Prev_Equivalent_Export_Value = 0;
            if (dtFrom > Financial_Period_dtFom)
            {
                //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                DateTime New_dtTo = dtFrom.Subtract(ts);
                //DataTable dt_Acc = mm.Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID,0);
                DataTable dt_Prev = ComputeFromPeriodStartToSearch(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, 0, 0);
                if (dt_Prev.Rows.Count > 0)
                {
                    //Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
                    //Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());
                    Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import_Value"].ToString());
                    Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export_Value"].ToString());
                    Prev_Equivalent_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Equivalent_Import_Value"].ToString());
                    Prev_Equivalent_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Equivalent_Export_Value"].ToString());
                    Raseed_Awel_Moda = "رصيد منقول";
                }
                #region comment رصيد منقول بدون اضافة اول المده
                //DataRow dr1 = dt.NewRow();
                //dr1["Equivalent_Import_Value"] = Prev_Equivalent_Import_Value;
                //dr1["Equivalent_Export_Value"] = Prev_Equivalent_Export_Value;
                //dr1["Import_Value"] = Prev_Import_Value;
                //dr1["Export_Value"] = Prev_Export_Value;

                //decimal Balance_Prev = Prev_Import_Value - Prev_Export_Value;
                //if (Balance_Prev > 0)
                //{
                //    dr1["Balance_Import_Value"] = Balance_Prev;
                //    dr1["Balance_Export_Value"] = 0;
                //}
                //else if (Balance_Prev<0)
                //{
                //    Balance_Prev = Balance_Prev * -1;
                //    dr1["Balance_Import_Value"] = 0;
                //    dr1["Balance_Export_Value"] = Balance_Prev;
                //}
                //else if (Balance_Prev < 0)
                //{
                //    dr1["Balance_Import_Value"] = 0;
                //    dr1["Balance_Export_Value"] = 0;
                //}
                //decimal Equivalent_Balance_Prev = Prev_Equivalent_Import_Value - Prev_Equivalent_Export_Value;
                //if (Balance_Prev > 0)
                //{
                //    dr1["Equivalent_Balance_Import_Value"] = Equivalent_Balance_Prev;
                //    dr1["Equivalent_Balance_Export_Value"] = 0;
                //}
                //else if (Balance_Prev < 0)
                //{
                //    Equivalent_Balance_Prev = Equivalent_Balance_Prev * -1;
                //    dr1["Equivalent_Balance_Import_Value"] = 0;
                //    dr1["Equivalent_Balance_Export_Value"] = Equivalent_Balance_Prev;
                //}
                //else if (Balance_Prev < 0)
                //{
                //    dr1["Equivalent_Balance_Import_Value"] = 0;
                //    dr1["Equivalent_Balance_Export_Value"] = 0;
                //}


                //dr1["Notes_Details"] = "رصيد منقول";

                //dt.Rows.Add(dr1);
                #endregion
            }
            //حساب رصيد أول المده
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;
            decimal Equivalent_First_Balance_Madin = 0;
            decimal Equivalent_First_Balance_Da2n = 0;

            Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref First_Balance_Madin, ref First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

            //إضافة رصيد اول المده على الرصيد المنقول
            decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            decimal Equivalent_First_Balance = (Equivalent_First_Balance_Madin - Equivalent_First_Balance_Da2n) + (Prev_Equivalent_Import_Value - Prev_Equivalent_Export_Value);
            if (First_Balance == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;



            }
            else if (First_Balance > 0)
            {
                Final_First_Balance_Madin = First_Balance;
                Final_First_Balance_Da2n = 0;


            }
            else if (First_Balance < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = First_Balance * -1;


            }
            if (Equivalent_First_Balance == 0)
            {


                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = 0;

            }
            else if (Equivalent_First_Balance > 0)
            {


                Equivalent_Final_First_Balance_Madin = Equivalent_First_Balance;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_First_Balance < 0)
            {



                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = Equivalent_First_Balance * -1;
            }
            /////////////حساب رصيد الحركات
            Set_ComputeBalance(ref dt, First_Balance, Equivalent_First_Balance);

            /////////////// إضافة رصيد أول المده او الرصيد المنقول
            DataRow dr1 = dt.NewRow();
            dr1["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr1["Balance_Export_Value"] = Final_First_Balance_Da2n;
            dr1["Equivalent_Balance_Import_Value"] = Equivalent_Final_First_Balance_Madin;
            dr1["Equivalent_Balance_Export_Value"] = Equivalent_Final_First_Balance_Da2n;
            dr1["Notes_Details"] = Raseed_Awel_Moda;
            dt.Rows.InsertAt(dr1, 0);




            Common.Make_TableColumnSerialME(ref dt);


            //////////إضافة الاجمالى
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            decimal Equivalent_Import_Value = 0;
            decimal Equivalent_Export_Value = 0;

            decimal Balance_Import_Value = 0;
            decimal Balance_Export_Value = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }


                try
                {
                    Equivalent_Import_Value = Equivalent_Import_Value + decimal.Parse(dt.Rows[x]["Equivalent_Import_Value"].ToString());
                    Equivalent_Import_Value = Common.RoundMe(Equivalent_Import_Value);
                }
                catch { }

                try
                {
                    Equivalent_Export_Value = Equivalent_Export_Value + decimal.Parse(dt.Rows[x]["Equivalent_Export_Value"].ToString());
                    Equivalent_Export_Value = Common.RoundMe(Equivalent_Export_Value);
                }
                catch { }
            }

            DataRow dr = dt.NewRow();
            dr["Equivalent_Import_Value"] = Equivalent_Import_Value;
            dr["Equivalent_Export_Value"] = Equivalent_Export_Value;
            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);

            //////////////////حساب الرصيد المنقول
            decimal Balance_Mankool = Import_Value - Export_Value;

            Balance_Mankool = Balance_Mankool + First_Balance;
            Balance_Mankool = Common.RoundMe(Balance_Mankool);

            decimal Equivalent_Balance_Mankool = Equivalent_Import_Value - Equivalent_Export_Value;

            Equivalent_Balance_Mankool = Equivalent_Balance_Mankool + Equivalent_First_Balance;
            Equivalent_Balance_Mankool = Common.RoundMe(Equivalent_Balance_Mankool);


            if (Balance_Mankool == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool > 0)
            {
                Final_First_Balance_Madin = Balance_Mankool;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = Balance_Mankool * -1;
            }

            if (Equivalent_Balance_Mankool == 0)
            {
                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_Balance_Mankool > 0)
            {
                Equivalent_Final_First_Balance_Madin = Equivalent_Balance_Mankool;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_Balance_Mankool < 0)
            {

                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = Equivalent_Balance_Mankool * -1;
            }

            /////////////////////
            DataRow dr2 = dt.NewRow();
            dr2["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr2["Balance_Export_Value"] = Final_First_Balance_Da2n;

            dr2["Equivalent_Balance_Import_Value"] = Equivalent_Final_First_Balance_Madin;
            dr2["Equivalent_Balance_Export_Value"] = Equivalent_Final_First_Balance_Da2n;

            dr2["Notes_Details"] = "رصيد منقول";

            dt.Rows.Add(dr2);

            return dt;
        }
        public DataTable Get_MasterAccount_Entry_Details_Tarseed_NoPeriod(DateTime dtFrom, DateTime dtTo, long    Entry_Currency_ID, long Journal_Code, long Branch_Id, long Submain_Account_ID, bool IS_AddLastBalance)
        {
            string mysql = @" SELECT        '' AS Serial_, Account_Entry_List.Currency_ID AS Account_Entry_Currency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Account_Entry_List.Entry_Notes, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.User_ID_Login, 
                         Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type AS Account_Entry_Transform_Type, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, 
                         Account_Entry_List.Year_Financial_Period_Id, Currency.Currency_Name, Currency.Currency_Name_EN, Journal_Codes.Journal_Name, Account_Entry_Details.ID, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 
                         Account_Entry_Details.SubAccount_NO AS Account_Entry_Details_SubAccount_NO, Account_Entry_Details.Transaction_Value, Account_Entry_Details.Notes_Details, 
                         Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, 
                         Account_Entry_Details.Account_Code AS Account_Entry_Details_Account_Code, 
                         Account_Entry_Details.Account_Customer_Type_Value AS Account_Entry_Details_Account_Customer_Type_Value, 
                         Account_Entry_Details.Account_Customer_Type_ID AS Account_Entry_Details_Account_Customer_Type_ID, 
                          Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo, Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, 
                         Account_Entry_Details.Equivalent_Balance_Import_Value, Account_Entry_Details.Equivalent_Balance_Export_Value, Account_Entry_Details.Account_Currency_ID, 
                         Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Account_Entry_Details.Line_No, 
                         Account_Entry_Details.Master_Submain_Account_Id, Account_Entry_Details.Master_Submain_Account_Name, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.SubAccount_NO, 
                         Account_Entry_Details.Import_Value, Account_Entry_Details.Export_Value, Account_Entry_Details.Balance_Import_Value, 
                         Account_Entry_Details.Balance_Export_Value, Account_Entry_Details.Equivalent_Transaction_Value
FROM            Account_Entry_List INNER JOIN
                         Currency ON Account_Entry_List.Currency_ID = Currency.Value INNER JOIN
                         Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }

            mysql = mysql + "  AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ")";
            mysql = mysql + " AND (Account_Entry_List.Entry_No   IS NOT NULL) ";

            //   mysql = mysql + "  ORDER BY Account_Entry_List.IS_FirstBalance DESC ,Account_Entry_Details.Id ASC";
            mysql = mysql + "   ORDER BY Process_Date ";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);



            string Raseed_Awel_Moda = "رصيد أول المده";
            ///////////////الحصول على اجمالى الحركات السابقه واضافتها على رصيد اول المده
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;
            decimal Equivalent_Final_First_Balance_Madin = 0;
            decimal Equivalent_Final_First_Balance_Da2n = 0;

            Mezan_MorgaaCLS mm = new Mezan_MorgaaCLS();
            DateTime Financial_Period_dtFom = new DateTime(2009, 1, 1);
            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;
            decimal Prev_Equivalent_Import_Value = 0;
            decimal Prev_Equivalent_Export_Value = 0;
            if (dtFrom > Financial_Period_dtFom)
            {
                //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                DateTime New_dtTo = dtFrom.Subtract(ts);
                //DataTable dt_Acc = mm.Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID,0);
                DataTable dt_Prev = ComputeFromPeriodStartToSearch(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, Journal_Code, Entry_Currency_ID);
                if (dt_Prev.Rows.Count > 0)
                {
                    //Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
                    //Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());
                    Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import_Value"].ToString());
                    Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export_Value"].ToString());
                    Prev_Equivalent_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Equivalent_Import_Value"].ToString());
                    Prev_Equivalent_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Equivalent_Export_Value"].ToString());
                    Raseed_Awel_Moda = "رصيد منقول";
                }

            }
            //حساب رصيد أول المده
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;
            decimal Equivalent_First_Balance_Madin = 0;
            decimal Equivalent_First_Balance_Da2n = 0;

            //Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

            //إضافة رصيد اول المده على الرصيد المنقول
            decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            decimal Equivalent_First_Balance = (Equivalent_First_Balance_Madin - Equivalent_First_Balance_Da2n) + (Prev_Equivalent_Import_Value - Prev_Equivalent_Export_Value);
            if (First_Balance == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;



            }
            else if (First_Balance > 0)
            {
                Final_First_Balance_Madin = First_Balance;
                Final_First_Balance_Da2n = 0;


            }
            else if (First_Balance < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = First_Balance * -1;


            }
            if (Equivalent_First_Balance == 0)
            {


                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = 0;

            }
            else if (Equivalent_First_Balance > 0)
            {


                Equivalent_Final_First_Balance_Madin = Equivalent_First_Balance;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_First_Balance < 0)
            {



                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = Equivalent_First_Balance * -1;
            }
            /////////////حساب رصيد الحركات
            Set_ComputeBalance(ref dt, First_Balance, Equivalent_First_Balance);

            /////////////// إضافة رصيد أول المده او الرصيد المنقول
            DataRow dr1 = dt.NewRow();
            dr1["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr1["Balance_Export_Value"] = Final_First_Balance_Da2n;
            dr1["Equivalent_Balance_Import_Value"] = Equivalent_Final_First_Balance_Madin;
            dr1["Equivalent_Balance_Export_Value"] = Equivalent_Final_First_Balance_Da2n;
            dr1["Notes_Details"] = Raseed_Awel_Moda;
            dt.Rows.InsertAt(dr1, 0);




            Common.Make_TableColumnSerialME(ref dt);


            //////////إضافة الاجمالى
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            decimal Equivalent_Import_Value = 0;
            decimal Equivalent_Export_Value = 0;

            decimal Balance_Import_Value = 0;
            decimal Balance_Export_Value = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }


                try
                {
                    Equivalent_Import_Value = Equivalent_Import_Value + decimal.Parse(dt.Rows[x]["Equivalent_Import_Value"].ToString());
                    Equivalent_Import_Value = Common.RoundMe(Equivalent_Import_Value);
                }
                catch { }

                try
                {
                    Equivalent_Export_Value = Equivalent_Export_Value + decimal.Parse(dt.Rows[x]["Equivalent_Export_Value"].ToString());
                    Equivalent_Export_Value = Common.RoundMe(Equivalent_Export_Value);
                }
                catch { }
            }

            DataRow dr = dt.NewRow();
            dr["Equivalent_Import_Value"] = Equivalent_Import_Value;
            dr["Equivalent_Export_Value"] = Equivalent_Export_Value;
            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);

            //////////////////حساب الرصيد المنقول
            decimal Balance_Mankool = Import_Value - Export_Value;

            Balance_Mankool = Balance_Mankool + First_Balance;
            Balance_Mankool = Common.RoundMe(Balance_Mankool);

            decimal Equivalent_Balance_Mankool = Equivalent_Import_Value - Equivalent_Export_Value;

            Equivalent_Balance_Mankool = Equivalent_Balance_Mankool + Equivalent_First_Balance;
            Equivalent_Balance_Mankool = Common.RoundMe(Equivalent_Balance_Mankool);


            if (Balance_Mankool == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool > 0)
            {
                Final_First_Balance_Madin = Balance_Mankool;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = Balance_Mankool * -1;
            }

            if (Equivalent_Balance_Mankool == 0)
            {
                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_Balance_Mankool > 0)
            {
                Equivalent_Final_First_Balance_Madin = Equivalent_Balance_Mankool;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_Balance_Mankool < 0)
            {

                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = Equivalent_Balance_Mankool * -1;
            }

            /////////////////////
            DataRow dr2 = dt.NewRow();
            dr2["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr2["Balance_Export_Value"] = Final_First_Balance_Da2n;

            dr2["Equivalent_Balance_Import_Value"] = Equivalent_Final_First_Balance_Madin;
            dr2["Equivalent_Balance_Export_Value"] = Equivalent_Final_First_Balance_Da2n;

            dr2["Notes_Details"] = "رصيد منقول";

            dt.Rows.Add(dr2);

            return dt;
        }
        public DataTable Get_MasterAccount_Entry_Details_Tarseed_NoPeriodNewBackup(DateTime dtFrom, DateTime dtTo, long    Entry_Currency_ID, long Journal_Code, long Branch_Id, long Submain_Account_ID, bool IS_AddLastBalance)
        {
            string mysql = @" SELECT        '' AS Serial_, Account_Entry_List.Entry_Currency_ID AS Account_Entry_Currency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Account_Entry_List.Entry_Notes, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.User_ID_Login, 
                         Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type AS Account_Entry_Transform_Type, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, 
                         Account_Entry_List.Year_Financial_Period_Id, Currency.Currency_Name, Currency.Currency_Name_EN, Journal_Codes.Journal_Name, Account_Entry_Details.ID, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 
                         Account_Entry_Details.SubAccount_NO AS Account_Entry_Details_SubAccount_NO,  Account_Entry_Details.Notes_Details, 
                         Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, 
                         Account_Entry_Details.Account_Code AS Account_Entry_Details_Account_Code, 
                         Account_Entry_Details.Account_Customer_Type_Value AS Account_Entry_Details_Account_Customer_Type_Value, 
                         Account_Entry_Details.Account_Customer_Type_ID AS Account_Entry_Details_Account_Customer_Type_ID, 
                         Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo, ";
            if (Entry_Currency_ID == 0)
            {
                mysql = mysql + @"
Account_Entry_Details.Equivalent_Transaction_Value AS Transaction_Value, 
Account_Entry_Details.Equivalent_Import_Value     AS Import_Value , 
Account_Entry_Details.Equivalent_Export_Value AS Export_Value, 
 Account_Entry_Details.Equivalent_Balance_Import_Value AS Balance_Import_Value,
Account_Entry_Details.Equivalent_Balance_Export_Value AS Balance_Export_Value ,  
                Account_Entry_Details.Transaction_Value AS Final_Transaction_Value,";
            }
            else
            {
                mysql = mysql + @" 
Account_Entry_Details.Transaction_Value,
Account_Entry_Details.Import_Value,
Account_Entry_Details.Export_Value,
Account_Entry_Details.Balance_Import_Value, 
Account_Entry_Details.Balance_Export_Value, 
                Account_Entry_Details.Equivalent_Transaction_Value AS Final_Transaction_Value, ";
            }
            mysql = mysql + @"
Account_Entry_Details.Account_Currency_ID, 
                         Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Account_Entry_Details.Line_No, 
                         Account_Entry_Details.Master_Submain_Account_Id, Account_Entry_Details.Master_Submain_Account_Name, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.SubAccount_NO
                         
FROM            Account_Entry_List INNER JOIN
                         Currency ON Account_Entry_List.Entry_Currency_ID = Currency.Value INNER JOIN
                         Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            //  mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date BETWEEN " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + "  AND  " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";

            // (Account_Entry_List.Entry_Date BETWEEN CONVERT(DATETIME, '01/01/2015', 102) AND CONVERT(DATETIME, '02/17/2015', 102))
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }

            mysql = mysql + "  AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ")";
            mysql = mysql + " AND (Account_Entry_List.Entry_No   IS NOT NULL) ";

            //   mysql = mysql + "  ORDER BY Account_Entry_List.IS_FirstBalance DESC ,Account_Entry_Details.Id ASC";
            mysql = mysql + "   ORDER BY Entry_Date ";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);



            string Raseed_Awel_Moda = "رصيد أول المده";
            ///////////////الحصول على اجمالى الحركات السابقه واضافتها على رصيد اول المده
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;


            Mezan_MorgaaCLS mm = new Mezan_MorgaaCLS();
            DateTime Financial_Period_dtFom = new DateTime(2009, 1, 1);
            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;

            if (dtFrom > Financial_Period_dtFom)
            {
                //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                DateTime New_dtTo = dtFrom.Subtract(ts);
                //DataTable dt_Acc = mm.Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID,0);
                DataTable dt_Prev = ComputeFromPeriodStartToSearchNew(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, Journal_Code, Entry_Currency_ID);
                if (dt_Prev.Rows.Count > 0)
                {
                    //Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
                    //Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());
                    Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import_Value"].ToString());
                    Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export_Value"].ToString());

                    Raseed_Awel_Moda = "رصيد منقول";
                }

            }
            //حساب رصيد أول المده
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;

            //Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

            //إضافة رصيد اول المده على الرصيد المنقول
            decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            if (First_Balance == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;



            }
            else if (First_Balance > 0)
            {
                Final_First_Balance_Madin = First_Balance;
                Final_First_Balance_Da2n = 0;


            }
            else if (First_Balance < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = First_Balance * -1;


            }

            /////////////حساب رصيد الحركات
            Set_ComputeBalanceNew(ref dt, First_Balance);

            /////////////// إضافة رصيد أول المده او الرصيد المنقول
            DataRow dr1 = dt.NewRow();
            dr1["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr1["Balance_Export_Value"] = Final_First_Balance_Da2n;

            dr1["Notes_Details"] = Raseed_Awel_Moda;
            dt.Rows.InsertAt(dr1, 0);




            Common.Make_TableColumnSerialME(ref dt);


            //////////إضافة الاجمالى
            decimal Import_Value = 0;
            decimal Export_Value = 0;

            decimal Balance_Import_Value = 0;
            decimal Balance_Export_Value = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }



            }

            DataRow dr = dt.NewRow();

            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);

            //////////////////حساب الرصيد المنقول
            decimal Balance_Mankool = Import_Value - Export_Value;

            Balance_Mankool = Balance_Mankool + First_Balance;
            Balance_Mankool = Common.RoundMe(Balance_Mankool);





            if (Balance_Mankool == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool > 0)
            {
                Final_First_Balance_Madin = Balance_Mankool;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = Balance_Mankool * -1;
            }



            /////////////////////
            DataRow dr2 = dt.NewRow();
            dr2["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr2["Balance_Export_Value"] = Final_First_Balance_Da2n;


            dr2["Notes_Details"] = "رصيد منقول";

            dt.Rows.Add(dr2);

            return dt;
        }
        public DataTable Get_MasterAccount_Entry_Details_Data(ref AccountModelsLib.operations opp, DateTime dtFrom, DateTime dtTo, long    Entry_Currency_ID, long Journal_Code, long Branch_Id, long Submain_Account_ID)
        {
            string mysql = @" SELECT        '' AS Serial_, Account_Entry_List.Entry_Currency_ID AS Account_Entry_Currency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Account_Entry_List.Entry_Notes, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.User_ID_Login, 
                         Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type AS Account_Entry_Transform_Type, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, 
                         Account_Entry_List.Year_Financial_Period_Id,   Account_Entry_List.Journal_Name, Account_Entry_Details.ID, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 
                         Account_Entry_Details.SubAccount_NO AS Account_Entry_Details_SubAccount_NO,  Account_Entry_Details.Notes_Details, 
                         Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, 
                         Account_Entry_Details.Account_Code AS Account_Entry_Details_Account_Code, 
                         Account_Entry_Details.Account_Customer_Type_Value AS Account_Entry_Details_Account_Customer_Type_Value, 
                         Account_Entry_Details.Account_Customer_Type_ID AS Account_Entry_Details_Account_Customer_Type_ID, 
                           Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo, Account_Entry_Details.CarsWorkOrdersId, Account_Entry_Details.CarsId, Account_Entry_Details.WorkOrderNumber, Account_Entry_Details.CarNumber ,
                        Account_Entry_Details.DocNo , Account_Entry_Details.DocType ,    ";
            if (Entry_Currency_ID == 0)
            {
                mysql = mysql + @"
Account_Entry_Details.Equivalent_Transaction_Value AS Transaction_Value, 
Account_Entry_Details.Equivalent_Import_Value     AS Import_Value , 
Account_Entry_Details.Equivalent_Export_Value AS Export_Value, 
 Account_Entry_Details.Equivalent_Balance_Import_Value AS Balance_Import_Value,
Account_Entry_Details.Equivalent_Balance_Export_Value AS Balance_Export_Value ,  
                Account_Entry_Details.Transaction_Value AS Final_Transaction_Value,";
            }
            else
            {
                mysql = mysql + @" 
Account_Entry_Details.Transaction_Value,
Account_Entry_Details.Import_Value,
Account_Entry_Details.Export_Value,
Account_Entry_Details.Balance_Import_Value, 
Account_Entry_Details.Balance_Export_Value, 
                Account_Entry_Details.Equivalent_Transaction_Value AS Final_Transaction_Value, ";
            }
            mysql = mysql + @"
Account_Entry_Details.Account_Currency_ID, 
                         Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Account_Entry_Details.Line_No, 
                         Account_Entry_Details.Master_Submain_Account_Id, Account_Entry_Details.Master_Submain_Account_Name, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.SubAccount_NO
                         , 
                         Account_Entry_List.Entry_Currency_Name AS Currency_Name

FROM            Account_Entry_List INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            //  mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date BETWEEN " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + "  AND  " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";

            // (Account_Entry_List.Entry_Date BETWEEN CONVERT(DATETIME, '01/01/2015', 102) AND CONVERT(DATETIME, '02/17/2015', 102))
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }

            mysql = mysql + "  AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ")";
            mysql = mysql + " AND (Account_Entry_List.Entry_No   IS NOT NULL) ";


            //mysql = mysql + "   ORDER BY Entry_Date ";

            mysql = mysql + "   ORDER BY Account_Entry_List.Entry_Date,  Account_Entry_Details.ID ";



            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }


        public DataTable Get_MasterAccount_Entry_Details_Tarseed_NoPeriodNew(DateTime dtFrom, DateTime dtTo, long    Entry_Currency_ID, long Journal_Code, long Branch_Id, long Submain_Account_ID, bool IS_AddLastBalance)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = Get_MasterAccount_Entry_Details_Data(ref opp, dtFrom, dtTo, Entry_Currency_ID, Journal_Code, Branch_Id, Submain_Account_ID);




            string Raseed_Awel_Moda = "رصيد أول المده";
            ///////////////الحصول على اجمالى الحركات السابقه واضافتها على رصيد اول المده
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;


            Mezan_MorgaaCLS mm = new Mezan_MorgaaCLS();
            DateTime Financial_Period_dtFom = new DateTime(2009, 1, 1);
            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;

            if (dtFrom > Financial_Period_dtFom)
            {
                //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                DateTime New_dtTo = dtFrom.Subtract(ts);
                //DataTable dt_Acc = mm.Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID,0);
                DataTable dt_Prev = ComputeFromPeriodStartToSearchNew(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID, Journal_Code, Entry_Currency_ID);
                if (dt_Prev.Rows.Count > 0)
                {
                    //Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
                    //Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());
                    Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import_Value"].ToString());
                    Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export_Value"].ToString());

                    Raseed_Awel_Moda = "رصيد منقول";
                }

            }
            //حساب رصيد أول المده
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;

            //Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

            //إضافة رصيد اول المده على الرصيد المنقول
            decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            if (First_Balance == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;



            }
            else if (First_Balance > 0)
            {
                Final_First_Balance_Madin = First_Balance;
                Final_First_Balance_Da2n = 0;


            }
            else if (First_Balance < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = First_Balance * -1;


            }

            /////////////حساب رصيد الحركات
            Set_ComputeBalanceNew(ref dt, First_Balance);

            /////////////// إضافة رصيد أول المده او الرصيد المنقول
            DataRow dr1 = dt.NewRow();
            dr1["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr1["Balance_Export_Value"] = Final_First_Balance_Da2n;

            dr1["Notes_Details"] = Raseed_Awel_Moda;
            dt.Rows.InsertAt(dr1, 0);




            Common.Make_TableColumnSerialME(ref dt);


            //////////إضافة الاجمالى
            decimal Import_Value = 0;
            decimal Export_Value = 0;

            decimal Balance_Import_Value = 0;
            decimal Balance_Export_Value = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }



            }

            DataRow dr = dt.NewRow();

            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);

            //////////////////حساب الرصيد المنقول
            decimal Balance_Mankool = Import_Value - Export_Value;

            Balance_Mankool = Balance_Mankool + First_Balance;
            Balance_Mankool = Common.RoundMe(Balance_Mankool);





            if (Balance_Mankool == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool > 0)
            {
                Final_First_Balance_Madin = Balance_Mankool;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = Balance_Mankool * -1;
            }



            /////////////////////
            DataRow dr2 = dt.NewRow();
            dr2["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr2["Balance_Export_Value"] = Final_First_Balance_Da2n;


            dr2["Notes_Details"] = "رصيد منقول";

            dt.Rows.Add(dr2);

            return dt;
        }
        public DataTable Get_MasterCostCenter_Details_Tarseed(long Year_Financial_Period_Id, DateTime Financial_Period_dtFom, DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long CostCenter_Id)
        {
            string mysql = @" SELECT        '' AS Serial_, Account_Entry_List.Currency_ID AS Account_Entry_Currency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Account_Entry_List.Entry_Notes, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.User_ID_Login, 
                         Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type AS Account_Entry_Transform_Type, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, 
                         Account_Entry_List.Year_Financial_Period_Id, Currency.Currency_Name, Currency.Currency_Name_EN, Journal_Codes.Journal_Name, Account_Entry_Details.ID, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 
                         Account_Entry_Details.SubAccount_NO AS Account_Entry_Details_SubAccount_NO, Account_Entry_Details.Transaction_Value, Account_Entry_Details.Notes_Details, 
                         Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, 
                         Account_Entry_Details.Account_Code AS Account_Entry_Details_Account_Code, 
                         Account_Entry_Details.Account_Customer_Type_Value AS Account_Entry_Details_Account_Customer_Type_Value, 
                         Account_Entry_Details.Account_Customer_Type_ID AS Account_Entry_Details_Account_Customer_Type_ID, 
                          Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo, Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, 
                         Account_Entry_Details.Equivalent_Balance_Import_Value, Account_Entry_Details.Equivalent_Balance_Export_Value, Account_Entry_Details.Account_Currency_ID, 
                         Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Account_Entry_Details.Line_No, 
                         Account_Entry_Details.Master_Submain_Account_Id, Account_Entry_Details.Master_Submain_Account_Name, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.SubAccount_NO, 
                         Account_Entry_Details.Import_Value, Account_Entry_Details.Export_Value, Account_Entry_Details.Balance_Import_Value, 
                         Account_Entry_Details.Balance_Export_Value, Account_Entry_Details.Equivalent_Transaction_Value
FROM            Account_Entry_List INNER JOIN
                         Currency ON Account_Entry_List.Currency_ID = Currency.Value INNER JOIN
                         Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            mysql = mysql + "  AND (Account_Entry_Details.CostCenter_Id = " + CostCenter_Id + ")";
            mysql = mysql + " AND (Account_Entry_List.Entry_No   IS NOT NULL) ";
            mysql = mysql + "  ORDER BY Account_Entry_List.Entry_Date ";
            //mysql = mysql + "  ORDER BY Account_Entry_List.IS_FirstBalance DESC ,Account_Entry_Details.Id ASC";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);



            string Raseed_Awel_Moda = "رصيد أول المده";
            ///////////////الحصول على اجمالى الحركات السابقه واضافتها على رصيد اول المده
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;
            decimal Equivalent_Final_First_Balance_Madin = 0;
            decimal Equivalent_Final_First_Balance_Da2n = 0;

            //  Mezan_MorgaaCLS mm = new   Mezan_MorgaaCLS();

            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;
            decimal Prev_Equivalent_Import_Value = 0;
            decimal Prev_Equivalent_Export_Value = 0;
            //if (dtFrom > Financial_Period_dtFom)
            //{
            //    //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
            //    TimeSpan ts = new TimeSpan(1, 0, 0, 0);
            //    DateTime New_dtTo = dtFrom.Subtract(ts);
            //    //DataTable dt_Acc = mm.Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID,0);
            //    DataTable dt_Prev = Compute_CostCenter_FromPeriodStartToSearch(Branch_Id, Financial_Period_dtFom, New_dtTo, CostCenter_Id, 0);
            //    if (dt_Prev.Rows.Count > 0)
            //    {
            //        //Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
            //        //Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());
            //        Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import_Value"].ToString());
            //        Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export_Value"].ToString());
            //        Prev_Equivalent_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Equivalent_Import_Value"].ToString());
            //        Prev_Equivalent_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Equivalent_Export_Value"].ToString());
            //        Raseed_Awel_Moda = "رصيد منقول";
            //    }

            //}
            //حساب رصيد أول المده
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;
            decimal Equivalent_First_Balance_Madin = 0;
            decimal Equivalent_First_Balance_Da2n = 0;
            Get_FirstBalanceForCostCenter(dtFrom, Journal_Code, Branch_Id, CostCenter_Id, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);
            // Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, CostCenter_Id, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

            //إضافة رصيد اول المده على الرصيد المنقول
            decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            decimal Equivalent_First_Balance = (Equivalent_First_Balance_Madin - Equivalent_First_Balance_Da2n) + (Prev_Equivalent_Import_Value - Prev_Equivalent_Export_Value);
            if (First_Balance == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;



            }
            else if (First_Balance > 0)
            {
                Final_First_Balance_Madin = First_Balance;
                Final_First_Balance_Da2n = 0;


            }
            else if (First_Balance < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = First_Balance * -1;


            }
            if (Equivalent_First_Balance == 0)
            {


                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = 0;

            }
            else if (Equivalent_First_Balance > 0)
            {


                Equivalent_Final_First_Balance_Madin = Equivalent_First_Balance;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_First_Balance < 0)
            {



                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = Equivalent_First_Balance * -1;
            }
            /////////////حساب رصيد الحركات
            Set_ComputeBalance(ref dt, First_Balance, Equivalent_First_Balance);

            /////////////// إضافة رصيد أول المده او الرصيد المنقول
            DataRow dr1 = dt.NewRow();
            dr1["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr1["Balance_Export_Value"] = Final_First_Balance_Da2n;
            dr1["Equivalent_Balance_Import_Value"] = Equivalent_Final_First_Balance_Madin;
            dr1["Equivalent_Balance_Export_Value"] = Equivalent_Final_First_Balance_Da2n;
            dr1["Notes_Details"] = Raseed_Awel_Moda;
            dt.Rows.InsertAt(dr1, 0);




            Common.Make_TableColumnSerialME(ref dt);


            //////////إضافة الاجمالى
            decimal Import_Value = 0;
            decimal Export_Value = 0;
            decimal Equivalent_Import_Value = 0;
            decimal Equivalent_Export_Value = 0;

            decimal Balance_Import_Value = 0;
            decimal Balance_Export_Value = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }


                try
                {
                    Equivalent_Import_Value = Equivalent_Import_Value + decimal.Parse(dt.Rows[x]["Equivalent_Import_Value"].ToString());
                    Equivalent_Import_Value = Common.RoundMe(Equivalent_Import_Value);
                }
                catch { }

                try
                {
                    Equivalent_Export_Value = Equivalent_Export_Value + decimal.Parse(dt.Rows[x]["Equivalent_Export_Value"].ToString());
                    Equivalent_Export_Value = Common.RoundMe(Equivalent_Export_Value);
                }
                catch { }
            }

            DataRow dr = dt.NewRow();
            dr["Equivalent_Import_Value"] = Equivalent_Import_Value;
            dr["Equivalent_Export_Value"] = Equivalent_Export_Value;
            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);

            //////////////////حساب الرصيد المنقول
            decimal Balance_Mankool = Import_Value - Export_Value;

            Balance_Mankool = Balance_Mankool + First_Balance;
            Balance_Mankool = Common.RoundMe(Balance_Mankool);

            decimal Equivalent_Balance_Mankool = Equivalent_Import_Value - Equivalent_Export_Value;

            Equivalent_Balance_Mankool = Equivalent_Balance_Mankool + Equivalent_First_Balance;
            Equivalent_Balance_Mankool = Common.RoundMe(Equivalent_Balance_Mankool);


            if (Balance_Mankool == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool > 0)
            {
                Final_First_Balance_Madin = Balance_Mankool;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = Balance_Mankool * -1;
            }

            if (Equivalent_Balance_Mankool == 0)
            {
                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_Balance_Mankool > 0)
            {
                Equivalent_Final_First_Balance_Madin = Equivalent_Balance_Mankool;
                Equivalent_Final_First_Balance_Da2n = 0;
            }
            else if (Equivalent_Balance_Mankool < 0)
            {

                Equivalent_Final_First_Balance_Madin = 0;
                Equivalent_Final_First_Balance_Da2n = Equivalent_Balance_Mankool * -1;
            }

            /////////////////////
            DataRow dr2 = dt.NewRow();
            dr2["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr2["Balance_Export_Value"] = Final_First_Balance_Da2n;

            dr2["Equivalent_Balance_Import_Value"] = Equivalent_Final_First_Balance_Madin;
            dr2["Equivalent_Balance_Export_Value"] = Equivalent_Final_First_Balance_Da2n;

            dr2["Notes_Details"] = "رصيد منقول";

            dt.Rows.Add(dr2);

            return dt;
        }
        public DataTable Get_MasterCostCenter_Details_TarseedNew(DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long CostCenter_Id, long Entry_Currency_ID)
        {
            string mysql = @" SELECT        '' AS Serial_, Account_Entry_List.Currency_ID AS Account_Entry_Currency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Account_Entry_List.Entry_Notes, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.User_ID_Login, 
                         Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type AS Account_Entry_Transform_Type, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, 
                         Account_Entry_List.Year_Financial_Period_Id, Currency.Currency_Name, Currency.Currency_Name_EN, Journal_Codes.Journal_Name, Account_Entry_Details.ID, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 
                         Account_Entry_Details.SubAccount_NO AS Account_Entry_Details_SubAccount_NO,  Account_Entry_Details.Notes_Details, 
                         Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, 
                         Account_Entry_Details.Account_Code AS Account_Entry_Details_Account_Code, 
                         Account_Entry_Details.Account_Customer_Type_Value AS Account_Entry_Details_Account_Customer_Type_Value, 
                         Account_Entry_Details.Account_Customer_Type_ID AS Account_Entry_Details_Account_Customer_Type_ID, 
                          Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo,
 
                      
Account_Entry_Details.Account_Currency_ID, 
                         Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Account_Entry_Details.Line_No, 
                         Account_Entry_Details.Master_Submain_Account_Id, Account_Entry_Details.Master_Submain_Account_Name, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.SubAccount_NO, 
 ";



            if (Entry_Currency_ID == 0)
            {
                mysql = mysql + @"
Account_Entry_Details.Equivalent_Transaction_Value AS Transaction_Value, 
Account_Entry_Details.Equivalent_Import_Value     AS Import_Value , 
Account_Entry_Details.Equivalent_Export_Value AS Export_Value, 
 Account_Entry_Details.Equivalent_Balance_Import_Value AS Balance_Import_Value,
Account_Entry_Details.Equivalent_Balance_Export_Value AS Balance_Export_Value ,  
                Account_Entry_Details.Transaction_Value AS Final_Transaction_Value ";
            }
            else
            {
                mysql = mysql + @" 
Account_Entry_Details.Transaction_Value,
Account_Entry_Details.Import_Value,
Account_Entry_Details.Export_Value,
Account_Entry_Details.Balance_Import_Value, 
Account_Entry_Details.Balance_Export_Value, 
                Account_Entry_Details.Equivalent_Transaction_Value AS Final_Transaction_Value  ";
            }

            mysql = mysql + @" FROM            Account_Entry_List INNER JOIN
                         Currency ON Account_Entry_List.Currency_ID = Currency.Value INNER JOIN
                         Journal_Codes ON Account_Entry_List.journal_Code = Journal_Codes.Code INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }
            mysql = mysql + "  AND (Account_Entry_Details.CostCenter_Id = " + CostCenter_Id + ")";
            mysql = mysql + " AND (Account_Entry_List.Entry_No   IS NOT NULL) ";
            mysql = mysql + "  ORDER BY Account_Entry_List.Entry_Date ";
            //mysql = mysql + "  ORDER BY Account_Entry_List.IS_FirstBalance DESC ,Account_Entry_Details.Id ASC";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);



            string Raseed_Awel_Moda = "رصيد أول المده";
            ///////////////الحصول على اجمالى الحركات السابقه واضافتها على رصيد اول المده
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;
            DateTime Financial_Period_dtFom = new DateTime(2009, 1, 1);
            //  Mezan_MorgaaCLS mm = new   Mezan_MorgaaCLS();

            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;


            //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
            TimeSpan ts = new TimeSpan(1, 0, 0, 0);
            DateTime New_dtTo = dtFrom.Subtract(ts);
            DateTime New_dtFrom = dtFrom.Subtract(ts);
            //DataTable dt_Acc = mm.Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID,0);
            DataTable dt_Prev = Get_FirstBalanceForCostCenterNew(Branch_Id, New_dtFrom, CostCenter_Id, Journal_Code, Entry_Currency_ID);
            if (dt_Prev.Rows.Count > 0)
            {
                //Prev_Import_Value = decimal.Parse(dt.Rows[0]["Final_Import_Value"].ToString());
                //Prev_Export_Value = decimal.Parse(dt.Rows[0]["Final_Export_Value"].ToString());
                Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import_Value"].ToString());
                Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export_Value"].ToString());

                Raseed_Awel_Moda = "رصيد منقول";
            }



            //حساب رصيد أول المده
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;


            //إضافة رصيد اول المده على الرصيد المنقول
            decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            //decimal Equivalent_First_Balance = (Equivalent_First_Balance_Madin - Equivalent_First_Balance_Da2n) + (Prev_Equivalent_Import_Value - Prev_Equivalent_Export_Value);
            if (First_Balance == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;



            }
            else if (First_Balance > 0)
            {
                Final_First_Balance_Madin = First_Balance;
                Final_First_Balance_Da2n = 0;


            }
            else if (First_Balance < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = First_Balance * -1;


            }

            /////////////حساب رصيد الحركات
            Set_ComputeBalanceNew(ref dt, First_Balance);

            /////////////// إضافة رصيد أول المده او الرصيد المنقول
            DataRow dr1 = dt.NewRow();
            dr1["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr1["Balance_Export_Value"] = Final_First_Balance_Da2n;

            dr1["Notes_Details"] = Raseed_Awel_Moda;
            dt.Rows.InsertAt(dr1, 0);




            Common.Make_TableColumnSerialME(ref dt);


            //////////إضافة الاجمالى
            decimal Import_Value = 0;
            decimal Export_Value = 0;


            decimal Balance_Import_Value = 0;
            decimal Balance_Export_Value = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }



            }

            DataRow dr = dt.NewRow();

            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);

            //////////////////حساب الرصيد المنقول
            decimal Balance_Mankool = Import_Value - Export_Value;

            Balance_Mankool = Balance_Mankool + First_Balance;
            Balance_Mankool = Common.RoundMe(Balance_Mankool);





            if (Balance_Mankool == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool > 0)
            {
                Final_First_Balance_Madin = Balance_Mankool;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = Balance_Mankool * -1;
            }



            /////////////////////
            DataRow dr2 = dt.NewRow();
            dr2["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr2["Balance_Export_Value"] = Final_First_Balance_Da2n;



            dr2["Notes_Details"] = "رصيد منقول";

            dt.Rows.Add(dr2);

            return dt;
        }

        public DataTable Get_MasterCostCenter_Details_TarseedNewNew(long Year_Financial_Period_Id, DateTime Financial_Period_dtFom, DateTime dtFrom, DateTime dtTo, long Journal_Code, long Branch_Id, long CostCenter_Id, long    Entry_Currency_ID, long Submain_Account_ID)
        {
            string mysql = @" SELECT        '' AS Serial_, Account_Entry_List.Currency_ID AS Account_Entry_Currency_ID, Account_Entry_List.Entry_No, Account_Entry_List.Entry_Date, 
                         Account_Entry_List.journal_Code, Account_Entry_List.Entry_Notes, Account_Entry_List.Branch_Id_Entry, Account_Entry_List.User_ID_Login, 
                         Account_Entry_List.Branch_ID_Login, Account_Entry_List.Entry_Total, Account_Entry_List.Transform_Type AS Account_Entry_Transform_Type, 
                         Account_Entry_List.IS_Transfer_To_Gl_Flage, Account_Entry_List.IS_Transfer_To_Gl_Flage_Str, Account_Entry_List.Transfer_To_Gl_DateTime, 
                         Account_Entry_List.Year_Financial_Period_Id,     Account_Entry_Details.ID, 
                         Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, 
                         Account_Entry_Details.SubAccount_NO AS Account_Entry_Details_SubAccount_NO,  Account_Entry_Details.Notes_Details, 
                         Account_Entry_Details.Equivalent_Currency_Value, Account_Entry_Details.Transaction_Type, 
                         Account_Entry_Details.Account_Code AS Account_Entry_Details_Account_Code, 
                         Account_Entry_Details.Account_Customer_Type_Value AS Account_Entry_Details_Account_Customer_Type_Value, 
                         Account_Entry_Details.Account_Customer_Type_ID AS Account_Entry_Details_Account_Customer_Type_ID, 
                           Account_Entry_Details.Transaction_DateFrom, 
                         Account_Entry_Details.Transaction_DateTo,
 
                      
Account_Entry_Details.Account_Currency_ID, 
                         Account_Entry_Details.Entry_Currency_ID, Account_Entry_Details.CostCenter_Id, Account_Entry_Details.CostCenter_Name, Account_Entry_Details.Line_No, 
                         Account_Entry_Details.Master_Submain_Account_Id, Account_Entry_Details.Master_Submain_Account_Name, Submain_Account.Id, 
                         Submain_Account.Main_Account_Id, Submain_Account.SubAccount_Name, Submain_Account.Account_Code, Submain_Account.SubAccount_NO, 
Account_Entry_List.Entry_Currency_Name AS Currency_Name, Account_Entry_List.Journal_Name ,  

 ";



            if (Entry_Currency_ID == 0)
            {
                mysql = mysql + @"
Account_Entry_Details.Equivalent_Transaction_Value AS Transaction_Value, 
Account_Entry_Details.Equivalent_Import_Value     AS Import_Value , 
Account_Entry_Details.Equivalent_Export_Value AS Export_Value, 
 Account_Entry_Details.Equivalent_Balance_Import_Value AS Balance_Import_Value,
Account_Entry_Details.Equivalent_Balance_Export_Value AS Balance_Export_Value ,  
                Account_Entry_Details.Transaction_Value AS Final_Transaction_Value ";
            }
            else
            {
                mysql = mysql + @" 
Account_Entry_Details.Transaction_Value,
Account_Entry_Details.Import_Value,
Account_Entry_Details.Export_Value,
Account_Entry_Details.Balance_Import_Value, 
Account_Entry_Details.Balance_Export_Value, 
                Account_Entry_Details.Equivalent_Transaction_Value AS Final_Transaction_Value  ";
            }

            mysql = mysql + @" FROM            Account_Entry_List INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            //mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            mysql = mysql + " WHERE  (Account_Entry_List.Entry_Date BETWEEN " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + "  AND  " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ") ";
            }

            mysql = mysql + "  AND (Account_Entry_Details.CostCenter_Id = " + CostCenter_Id + ")";
            mysql = mysql + " AND (Account_Entry_List.Entry_No   IS NOT NULL) ";
            mysql = mysql + "  ORDER BY Account_Entry_List.Entry_Date ";
            //mysql = mysql + "  ORDER BY Account_Entry_List.IS_FirstBalance DESC ,Account_Entry_Details.Id ASC";

            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataTable dt = opp.return_DataTable(mysql);

            ////////////////////////////////
            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;
            string Raseed_Awel_Moda = "رصيد أول المده";
            if (dtFrom > Financial_Period_dtFom)
            {
                //////////////حساب الفترة من تاريخ بدايه الفترة الماليه حتى تاريخ البحث الى
                TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                DateTime New_dtTo = dtFrom.Subtract(ts);
                //DataTable dt_Acc = mm.Get_Mezan_Morgaa_ByPeiod(Branch_Id, Financial_Period_dtFom, New_dtTo, Submain_Account_ID,0);
                //DataTable dt_Prev = Compute_CostCenter_FromPeriodStartToSearch(Branch_Id, dtFrom, New_dtTo, CostCenter_Id, Entry_Currency_ID);
                DataTable dt_Prev = ComputeFromPeriodStartToSearchNew_CostCenter(Branch_Id, Financial_Period_dtFom, New_dtTo, CostCenter_Id, Journal_Code, Entry_Currency_ID, Submain_Account_ID);

                if (dt_Prev.Rows.Count > 0)
                {

                    Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import_Value"].ToString());
                    Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export_Value"].ToString());

                    Raseed_Awel_Moda = "رصيد منقول";
                }

            }//////////////////////////////////////


            ///////////////الحصول على اجمالى الحركات السابقه واضافتها على رصيد اول المده
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;
            //DateTime Financial_Period_dtFom = new DateTime(2009, 1, 1);
            //  Mezan_MorgaaCLS mm = new   Mezan_MorgaaCLS();

            //حساب رصيد أول المده
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;

            Get_FirstBalance_PeriodNewCostCenter(Year_Financial_Period_Id, Branch_Id, CostCenter_Id, Submain_Account_ID, ref First_Balance_Madin, ref First_Balance_Da2n, Entry_Currency_ID);
            //إضافة رصيد اول المده على الرصيد المنقول
            decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
            //decimal Equivalent_First_Balance = (Equivalent_First_Balance_Madin - Equivalent_First_Balance_Da2n) + (Prev_Equivalent_Import_Value - Prev_Equivalent_Export_Value);
            if (First_Balance == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;



            }
            else if (First_Balance > 0)
            {
                Final_First_Balance_Madin = First_Balance;
                Final_First_Balance_Da2n = 0;


            }
            else if (First_Balance < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = First_Balance * -1;


            }

            /////////////حساب رصيد الحركات
            Set_ComputeBalanceNew(ref dt, First_Balance);

            /////////////// إضافة رصيد أول المده او الرصيد المنقول
            DataRow dr1 = dt.NewRow();
            dr1["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr1["Balance_Export_Value"] = Final_First_Balance_Da2n;

            dr1["Notes_Details"] = Raseed_Awel_Moda;
            dt.Rows.InsertAt(dr1, 0);




            Common.Make_TableColumnSerialME(ref dt);


            //////////إضافة الاجمالى
            decimal Import_Value = 0;
            decimal Export_Value = 0;


            decimal Balance_Import_Value = 0;
            decimal Balance_Export_Value = 0;


            for (int x = 0; x < dt.Rows.Count; x++)
            {

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }

                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }



            }

            DataRow dr = dt.NewRow();

            dr["Import_Value"] = Import_Value;
            dr["Export_Value"] = Export_Value;
            dr["Notes_Details"] = "إجمالى الحركه";

            dt.Rows.Add(dr);

            //////////////////حساب الرصيد المنقول
            decimal Balance_Mankool = Import_Value - Export_Value;

            Balance_Mankool = Balance_Mankool + First_Balance;
            Balance_Mankool = Common.RoundMe(Balance_Mankool);





            if (Balance_Mankool == 0)
            {
                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool > 0)
            {
                Final_First_Balance_Madin = Balance_Mankool;
                Final_First_Balance_Da2n = 0;
            }
            else if (Balance_Mankool < 0)
            {

                Final_First_Balance_Madin = 0;
                Final_First_Balance_Da2n = Balance_Mankool * -1;
            }



            /////////////////////
            DataRow dr2 = dt.NewRow();
            dr2["Balance_Import_Value"] = Final_First_Balance_Madin;
            dr2["Balance_Export_Value"] = Final_First_Balance_Da2n;



            dr2["Notes_Details"] = "رصيد منقول";

            dt.Rows.Add(dr2);

            return dt;
        }

        void Get_FirstBalanceForCostCenter(DateTime dtFrom, long Journal_Code, long Branch_Id, long CostCenter_Id, ref decimal Equivalent_First_Balance_Madin, ref decimal Equivalent_First_Balance_Da2n)
        {
            Equivalent_First_Balance_Madin = 0;
            Equivalent_First_Balance_Da2n = 0;
            string mysql = "";
            mysql = @"SELECT        SUM(Account_Entry_Details.Equivalent_Import_Value) AS Equivalent_Import_Value, SUM(Account_Entry_Details.Equivalent_Export_Value) 
                         AS Equivalent_Export_Value, SUM(Account_Entry_Details.Import_Value) AS Import_Value, SUM(Account_Entry_Details.Export_Value) AS Export_Value
FROM            Account_Entry_List INNER JOIN
                         Account_Entry_Details ON Account_Entry_List.Id = Account_Entry_Details.Account_Entry_List_ID ";



            mysql = mysql + " WHERE  (Account_Entry_List.Entry_No IS NOT NULL) AND  (Account_Entry_List.Entry_Date < " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + ")  ";
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Branch_Id_Entry = " + Branch_Id + ") ";
            }

            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.journal_Code = " + Journal_Code + ") ";
            }

            mysql = mysql + "  AND (Account_Entry_Details.CostCenter_Id = " + CostCenter_Id + ")";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                try
                {
                    Equivalent_First_Balance_Madin = decimal.Parse(dt.Rows[0]["Equivalent_Import_Value"].ToString());
                }
                catch { }
                try
                {
                    Equivalent_First_Balance_Da2n = decimal.Parse(dt.Rows[0]["Equivalent_Export_Value"].ToString());
                }
                catch { }
            }
        }
        public DataTable Get_FirstBalanceForCostCenterNew(long Branch_Id, DateTime dtFromNew, long CostCenter_Id, long Journal_Code, long Entry_Currency_ID)
        {
            string mysql = "";
            mysql = @" SELECT         0 AS Final_Fist_Balance_Madin, 0 AS Final_Fist_Balance_Da2n, 0 AS Final_Last_Balance_Madin, 0 AS Final_Last_Balance_Da2n,";
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + @" SUM(Account_Entry_Details.Import_Value) AS Import_Value, 
SUM(Account_Entry_Details.Export_Value) AS Export_Value 
   ";
            }
            if (Entry_Currency_ID == 0)
            {
                mysql = mysql + @" SUM(Account_Entry_Details.Equivalent_Import_Value) AS Import_Value,
SUM(Account_Entry_Details.Equivalent_Export_Value)  AS  Export_Value 

 ";
            }

            mysql = mysql + @"  
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            ///AND  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFom) + ")
            mysql = mysql + " WHERE   (Account_Entry_List.Entry_No IS NOT  NULL)  AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtFromNew) + ") ";

            if (CostCenter_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.CostCenter_Id  = " + CostCenter_Id + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Journal_Code = " + Journal_Code + ") ";
            }

            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.CostCenter_Id ";

            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            return dt;

        }

        public DataTable ComputeFromPeriodStartToSearch(long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Journal_Code, long Entry_Currency_ID)
        {
            string mysql = "";
            mysql = @" SELECT        Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, SUM(Account_Entry_Details.Import_Value) AS Import_Value, 
                         SUM(Account_Entry_Details.Export_Value) AS Export_Value, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code, 
                         SUM(Account_Entry_Details.Equivalent_Import_Value) AS Equivalent_Import_Value, SUM(Account_Entry_Details.Equivalent_Export_Value) 
                         AS Equivalent_Export_Value, 0 AS Final_Fist_Balance_Madin, 0 AS Final_Fist_Balance_Da2n, 0 AS Final_Last_Balance_Madin, 0 AS Final_Last_Balance_Da2n, 
                         0 AS Equivalent_Final_Fist_Balance_Madin, 0 AS Equivalent_Final_Fist_Balance_Da2n, 0 AS Equivalent_Final_Last_Balance_Madin, 
                         0 AS Equivalent_Final_Last_Balance_Da2n
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            mysql = mysql + " WHERE   (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.Submain_Account_ID  = " + Submain_Account_ID + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Journal_Code = " + Journal_Code + ") ";
            }

            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.Account_Code, Submain_Account.Currency_Id ";

            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            return dt;

        }

        public DataTable ComputeFromPeriodStartToSearchNew(long Branch_Id, DateTime dtFom, DateTime dtTo, long Submain_Account_ID, long Journal_Code, long Entry_Currency_ID)
        {
            string mysql = "";
            mysql = @" SELECT        Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name,Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code,";
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + @" SUM(Account_Entry_Details.Import_Value) AS Import_Value, 
SUM(Account_Entry_Details.Export_Value) AS Export_Value,
   ";
            }
            if (Entry_Currency_ID == 0)
            {
                mysql = mysql + @" SUM(Account_Entry_Details.Equivalent_Import_Value) AS Import_Value,
SUM(Account_Entry_Details.Equivalent_Export_Value)  AS  Export_Value,

 ";
            }

            mysql = mysql + @" 0 AS  Final_Fist_Balance_Madin,
0 AS  Final_Fist_Balance_Da2n,
0 AS  Final_Last_Balance_Madin, 
0 AS  Final_Last_Balance_Da2n 
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            mysql = mysql + " WHERE   (Account_Entry_List.Entry_No IS NOT  NULL) AND  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ") ";
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.Submain_Account_ID  = " + Submain_Account_ID + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Journal_Code = " + Journal_Code + ") ";
            }

            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.Account_Code, Submain_Account.Currency_Id ";

            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            return dt;

        }
        public DataTable ComputeFromPeriodStartToSearchNew_CostCenter(long Branch_Id, DateTime dtFrom, DateTime dtTo, long CostCenter_Id, long Journal_Code, long    Entry_Currency_ID, long Submain_Account_ID)
        {
            string mysql = "";
            mysql = @" SELECT        Account_Entry_Details.CostCenter_Id ,";
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + @" SUM(Account_Entry_Details.Import_Value) AS Import_Value, 
SUM(Account_Entry_Details.Export_Value) AS Export_Value,
   ";
            }
            if (Entry_Currency_ID == 0)
            {
                mysql = mysql + @" SUM(Account_Entry_Details.Equivalent_Import_Value) AS Import_Value,
SUM(Account_Entry_Details.Equivalent_Export_Value)  AS  Export_Value,

 ";
            }

            mysql = mysql + @" 0 AS  Final_Fist_Balance_Madin,
0 AS  Final_Fist_Balance_Da2n,
0 AS  Final_Last_Balance_Madin, 
0 AS  Final_Last_Balance_Da2n 
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id
";
            mysql = mysql + " WHERE   (Account_Entry_List.Entry_No IS NOT  NULL)  ";
            mysql = mysql + " AND  (Account_Entry_List.Entry_Date BETWEEN " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + "  AND  " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (CostCenter_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.CostCenter_Id  = " + CostCenter_Id + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }
            if (Journal_Code > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Journal_Code = " + Journal_Code + ") ";
            }
            if (Submain_Account_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_Details.Submain_Account_ID = " + Submain_Account_ID + ") ";
            }
            mysql = mysql + @" GROUP  BY Account_Entry_Details.CostCenter_Id ";

            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            return dt;

        }

        public DataTable Compute_CostCenter_FromPeriodStartToSearch(long Branch_Id, DateTime dtFrom, DateTime dtTo, long CostCenter_Id, long Entry_Currency_ID)
        {
            string mysql = "";
            mysql = @" SELECT        Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, SUM(Account_Entry_Details.Import_Value) AS Import_Value, 
                         SUM(Account_Entry_Details.Export_Value) AS Export_Value, Submain_Account.SubAccount_NO, Submain_Account.Code, Submain_Account.Account_Code, 
                         SUM(Account_Entry_Details.Equivalent_Import_Value) AS Equivalent_Import_Value, SUM(Account_Entry_Details.Equivalent_Export_Value) 
                         AS Equivalent_Export_Value, 0 AS Final_Fist_Balance_Madin, 0 AS Final_Fist_Balance_Da2n, 0 AS Final_Last_Balance_Madin, 0 AS Final_Last_Balance_Da2n, 
                         0 AS Equivalent_Final_Fist_Balance_Madin, 0 AS Equivalent_Final_Fist_Balance_Da2n, 0 AS Equivalent_Final_Last_Balance_Madin, 
                         0 AS Equivalent_Final_Last_Balance_Da2n
FROM            Account_Entry_Details INNER JOIN
                         Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.ID INNER JOIN
                         Submain_Account ON Account_Entry_Details.Submain_Account_ID = Submain_Account.Id
";
            mysql = mysql + " WHERE   (Account_Entry_List.Entry_No IS NOT  NULL)  ";
            //AND  (Account_Entry_List.Entry_Date >= " + AccountModelsLib.operations.Get_DateToInsert(dtFom) + ") AND (Account_Entry_List.Entry_Date <= " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")
            mysql = mysql + " AND  (Account_Entry_List.Entry_Date BETWEEN " + AccountModelsLib.operations.Get_DateToInsert(dtFrom) + "  AND  " + AccountModelsLib.operations.Get_DateToInsert(dtTo) + ")";
            if (CostCenter_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_Details.CostCenter_Id  = " + CostCenter_Id + ") ";
            }
            if (Branch_Id > 0)
            {
                mysql = mysql + " AND    (Account_Entry_List.Branch_Id_Entry  = " + Branch_Id + ") ";
            }
            if (Entry_Currency_ID > 0)
            {
                mysql = mysql + " AND (Account_Entry_List.Entry_Currency_ID = " + Entry_Currency_ID + ") ";
            }
            //if (Account_Currency_Id > 0)
            //{
            //    mysql = mysql + " AND (Submain_Account.Currency_Id = " + Account_Currency_Id + ") ";
            //}
            mysql = mysql + @" GROUP BY Account_Entry_List.Branch_Id_Entry, Account_Entry_Details.Submain_Account_ID, Account_Entry_Details.Account_Name, Submain_Account.SubAccount_NO, 
                         Submain_Account.Code, Submain_Account.Account_Code, Submain_Account.Currency_Id ";

            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            return dt;

        }
        void Set_ComputeBalance(ref DataTable dt, decimal First_Balance, decimal Equivalent_First_Balance)
        {
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;
            decimal Equivalent_Final_First_Balance_Madin = 0;
            decimal Equivalent_Final_First_Balance_Da2n = 0;


            decimal Import_Value = 0;
            decimal Export_Value = 0;
            decimal Equivalent_Import_Value = 0;
            decimal Equivalent_Export_Value = 0;

            decimal Balance = First_Balance;
            decimal Equivalent_Balance = Equivalent_First_Balance;
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                Import_Value = 0;
                Export_Value = 0;
                Equivalent_Import_Value = 0;
                Equivalent_Export_Value = 0;
                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }
                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }

                try
                {
                    Equivalent_Import_Value = Equivalent_Import_Value + decimal.Parse(dt.Rows[x]["Equivalent_Import_Value"].ToString());
                    Equivalent_Import_Value = Common.RoundMe(Equivalent_Import_Value);
                }
                catch { }
                try
                {
                    Equivalent_Export_Value = Equivalent_Export_Value + decimal.Parse(dt.Rows[x]["Equivalent_Export_Value"].ToString());
                    Equivalent_Export_Value = Common.RoundMe(Equivalent_Export_Value);
                }
                catch { }


                Balance = Balance + (Import_Value - Export_Value);
                Balance = Common.RoundMe(Balance);

                Equivalent_Balance = Equivalent_Balance + (Equivalent_Import_Value - Equivalent_Export_Value);
                Equivalent_Balance = Common.RoundMe(Equivalent_Balance);

                if (Balance == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance > 0)
                {
                    Final_First_Balance_Madin = Balance;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = Balance * -1;
                }

                if (Equivalent_Balance == 0)
                {
                    Equivalent_Final_First_Balance_Madin = 0;
                    Equivalent_Final_First_Balance_Da2n = 0;
                }
                else if (Equivalent_Balance > 0)
                {
                    Equivalent_Final_First_Balance_Madin = Equivalent_Balance;
                    Equivalent_Final_First_Balance_Da2n = 0;
                }
                else if (Equivalent_Balance < 0)
                {

                    Equivalent_Final_First_Balance_Madin = 0;
                    Equivalent_Final_First_Balance_Da2n = Equivalent_Balance * -1;
                }
                dt.Rows[x]["Balance_Import_Value"] = Final_First_Balance_Madin;
                dt.Rows[x]["Balance_Export_Value"] = Final_First_Balance_Da2n;

                dt.Rows[x]["Equivalent_Balance_Import_Value"] = Equivalent_Final_First_Balance_Madin;
                dt.Rows[x]["Equivalent_Balance_Export_Value"] = Equivalent_Final_First_Balance_Da2n;
            }
        }
        void Set_ComputeBalanceNew(ref DataTable dt, decimal First_Balance)
        {
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;


            decimal Import_Value = 0;
            decimal Export_Value = 0;

            decimal Balance = First_Balance;

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                Import_Value = 0;
                Export_Value = 0;

                try
                {
                    Import_Value = Import_Value + decimal.Parse(dt.Rows[x]["Import_Value"].ToString());
                    Import_Value = Common.RoundMe(Import_Value);
                }
                catch { }
                try
                {
                    Export_Value = Export_Value + decimal.Parse(dt.Rows[x]["Export_Value"].ToString());
                    Export_Value = Common.RoundMe(Export_Value);
                }
                catch { }




                Balance = Balance + (Import_Value - Export_Value);
                Balance = Common.RoundMe(Balance);


                if (Balance == 0)
                {
                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance > 0)
                {
                    Final_First_Balance_Madin = Balance;
                    Final_First_Balance_Da2n = 0;
                }
                else if (Balance < 0)
                {

                    Final_First_Balance_Madin = 0;
                    Final_First_Balance_Da2n = Balance * -1;
                }


                dt.Rows[x]["Balance_Import_Value"] = Final_First_Balance_Madin;
                dt.Rows[x]["Balance_Export_Value"] = Final_First_Balance_Da2n;


            }
        }
    }
}
