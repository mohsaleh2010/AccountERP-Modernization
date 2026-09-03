using AccountModelsLib.Models;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using Microsoft.Data.SqlClient;
using System.Drawing;



namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for SuppliersAccount.
    /// </summary>
    public class Suppliers_AccountsCls
    {
        ArrayList arr;
        public long ProcessNo = 0;
        public Suppliers_AccountsCls()
        {
            //
            // TODO: Add constructor logic here
            //
            arr = new ArrayList();
        }
        public DataTable GetSupplierFirstBalance(long Supplier_Id)
        {

            DataTable dt = new DataTable();

            string mysql = " SELECT  * FROM Suppliers_Accounts WHERE (Supplier_Id = " + Supplier_Id + ")  And Reference_Type = -7 OR  Reference_Type = -8";
            operations opp = new operations();
            dt = opp.return_DataTable(mysql);
            return dt;

        }
        void UpdateProcessNo(ref int Excp, ref operations opp, string TableName, long TableId)
        {
            string mysql = "";
            try
            {
                mysql = @" Alter Table " + TableName + "  Add  ProcessNo numeric(18, 0) Default(0) ";
                opp.insert_me(mysql);
                mysql = "Update TableName Set ProcessNo=0  Where   (ProcessNo IS NULL)";
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {


            }

            if (ProcessNo == 0)
            {
                ProcessNo = Common.Make_AccountsProcessNo(ref Excp, ref opp);
            }


            try
            {
                mysql = "Update " + TableName + " Set ProcessNo=" + ProcessNo + " Where Id=" + TableId;
                opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }
        }
        public DataSet Get_Suppliers_Account(long  Supplier_Id, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {

            DataSet G_DS = new DataSet();
            try
            {
                string mysql = " SELECT Suppliers_Accounts.* FROM Suppliers_Accounts WHERE  Flage_View=1   ";
                if (_GetDateFrom == true)
                {
                    mysql = mysql + " AND (Transaction_Date >= '" + operations.dateFormat(GetDateFrom) + "')   ";
                }
                if (_GetDateTo == true)
                {
                    mysql = mysql + "  AND (Transaction_Date <= '" + operations.dateFormat(GetDateTo) + "' )  ";
                }
                mysql = mysql + " AND  (Supplier_Id = " + Supplier_Id + ")   ";
                if (DESC)
                {
                    mysql = mysql + "  ORDER BY ID  DESC";
                }
                else
                {
                    mysql = mysql + "  ORDER BY ID  ASC";
                }
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Suppliers_Accounts");
                //  Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                // Excp = 1;
                return G_DS;
            }
        }

        public DataSet Get_Suppliers_AccountComputeBalance_(int Supplier_Id, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {
            DateTime dtBefor = GetDateFrom;

            TimeSpan ts = new TimeSpan(1, 0, 0, 0);

            dtBefor = dtBefor.Subtract(ts);
            DataSet G_DS = new DataSet();

            operations opp = new operations();

            string mysql = "";
            decimal BalanceBefor = 0;
            try
            {
                bool IsFountBalanceBefor = false;
                int FirstRowToCompute = 0;
                if (_GetDateFrom == true)
                {
                    mysql = @"SELECT   (SUM(Export) - SUM(Import)) AS Balance FROM  Suppliers_Accounts ";
                    mysql = mysql + " GROUP BY Supplier_Id, Transaction_Date ";

                    mysql = mysql + " HAVING (Transaction_Date < '" + operations.dateFormat(GetDateFrom.Date) + "')  AND (Supplier_Id = " + Supplier_Id + ") ";
                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        IsFountBalanceBefor = true;
                        try { BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString()); }
                        catch { BalanceBefor = 0; }
                    }
                }


                mysql = " SELECT Suppliers_Accounts.*,'' As BalanceImport,'' As BalanceExport FROM Suppliers_Accounts WHERE Flage_View=1      ";
                if (_GetDateFrom == true)
                {
                    mysql = mysql + " AND (Transaction_Date >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                }
                if (_GetDateTo == true)
                {
                    mysql = mysql + "   AND (Transaction_Date <='" + operations.dateFormat(GetDateTo.Date) + "' )  ";
                }
                mysql = mysql + "  AND (Supplier_Id = " + Supplier_Id + ") ";

                mysql = mysql + "  ORDER BY    Transaction_Date";


                G_DS = opp.return_dataset(mysql, "Suppliers_Accounts");



                int Count = G_DS.Tables[0].Rows.Count;
                if (IsFountBalanceBefor == true)
                {
                    DataRow dr2 = G_DS.Tables[0].NewRow();
                    dr2["Balance"] = BalanceBefor;
                    dr2["Import"] = 0;
                    dr2["Export"] = 0;
                    dr2["Transaction_Date"] = dtBefor;
                    dr2["Reference_Type"] = 0;

                    dr2["Reference_Type_View"] = "—’Ìœ ”«»ﬁ";
                    G_DS.Tables[0].Rows.InsertAt(dr2, 0);
                    FirstRowToCompute = 1;
                }
                ///////////////////////////////////////////////////////////
                decimal Balance = BalanceBefor;
                decimal ImportTotal = 0;
                decimal ExportTotal = 0;
                for (int x = FirstRowToCompute; x < G_DS.Tables[0].Rows.Count; x++)
                {
                    decimal Import = decimal.Parse(G_DS.Tables[0].Rows[x]["Import"].ToString());
                    decimal Export = decimal.Parse(G_DS.Tables[0].Rows[x]["Export"].ToString());

                    string Reference_Type = G_DS.Tables[0].Rows[x]["Reference_Type"].ToString();
                    if ((Reference_Type == "-8" | Reference_Type == "-7") & x == 0)
                    {
                        G_DS.Tables[0].Rows[x]["Import"] = 0;
                        G_DS.Tables[0].Rows[x]["Export"] = 0;
                        Balance = Export - Import;
                        G_DS.Tables[0].Rows[x]["Balance"] = Balance;

                    }
                    else
                    {
                        ImportTotal = ImportTotal + Import;

                        ExportTotal = ExportTotal + Export;
                        Balance = Balance + Import - Export;
                        G_DS.Tables[0].Rows[x]["Balance"] = Balance;
                    }
                }

                DataRow dr = G_DS.Tables[0].NewRow();
                dr["Balance"] = 0;
                dr["Import"] = ImportTotal;
                dr["Export"] = ExportTotal;

                dr["Reference_Type_View"] = "-";
                dr["Notes"] = "≈Ã„«·Ï «·Õ—ﬂ« ";
                G_DS.Tables[0].Rows.Add(dr);
                decimal FinalBalance = BalanceBefor + ImportTotal - ExportTotal;

                DataRow dr1 = G_DS.Tables[0].NewRow();
                dr1["Balance"] = FinalBalance;
                dr1["Import"] = 0;
                dr1["Export"] = 0;
                //dr1["Reference_Type"] = "-";

                dr1["Reference_Type_View"] = "-";
                dr1["Notes"] = "’«›Ï «·—’Ìœ  ";
                G_DS.Tables[0].Rows.Add(dr1);


                //  Excp = 1;

                return G_DS;
            }
            catch (Exception ee)
            {
                // Excp = 1;
                return G_DS;
            }
        }
        void SetBalanceDiff(ref decimal BalanceDiff, ref DataRow dr)
        {
            dr["BalanceDiff"] = BalanceDiff;
            //dt.Rows[x]["BalanceDiff"] = BalanceDiff;
            //string BalanceDiff_1 = "";
            //if (BalanceDiff1 < 0)
            //{
            //    BalanceDiff1 = BalanceDiff1 * -1;
            //    BalanceDiff_1 = "( " + BalanceDiff_1.ToString() + " )";
            //}
            //dr11["BalanceDiff"] = BalanceDiff_1;
        }
        public DataTable Get_Suppliers_AccountComputeBalance(long  Supplier_Id, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {
            DataTable dtAccount = new DataTable();
            DateTime dtBefor = GetDateFrom;

            TimeSpan ts = new TimeSpan(1, 0, 0, 0);

            dtBefor = dtBefor.Subtract(ts);
            DataSet G_DS = new DataSet();

            operations opp = new operations();

            string mysql = "";
            decimal BalanceBefor = 0;
            decimal ImportBefor = 0;
            decimal ExportBefor = 0;
            try
            {
                bool IsFountBalanceBefor = false;
                int FirstRowToCompute = 0;
                if (_GetDateFrom == true)
                {
                    mysql = @"SELECT      SUM(Import)   AS Import, SUM(Export)   AS Export,  (   SUM(Import) - SUM(Export) ) AS Balance FROM  Suppliers_Accounts ";
                    //mysql = mysql + " Where (Transaction_Date < '" + operations.dateFormat(GetDateFrom.Date) + "')  AND (Supplier_Id = " + Supplier_Id + ") ";
                    mysql = mysql + " Where (CONVERT(DATE, Transaction_Date)  < '" + operations.dateFormat(GetDateFrom.Date) + "')   AND (Supplier_Id = " + Supplier_Id + ") ";

                    DataTable dt = opp.return_DataTable(mysql);
                    if (dt.Rows.Count > 0)
                    {
                        IsFountBalanceBefor = true;
                        try
                        {
                            BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                            ImportBefor = decimal.Parse(dt.Rows[0]["Import"].ToString());
                            ExportBefor = decimal.Parse(dt.Rows[0]["Export"].ToString());


                        }
                        catch { BalanceBefor = 0; }
                    }
                }


                mysql = " SELECT Suppliers_Accounts.*,'' As BalanceImport,'' As BalanceExport FROM Suppliers_Accounts WHERE Flage_View=1      ";
                if (_GetDateFrom == true)
                {

                    mysql = mysql + " AND (CONVERT(DATE, Transaction_Date) >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                }
                if (_GetDateTo == true)
                {
                    mysql = mysql + "   AND (CONVERT(DATE, Transaction_Date) <= '" + operations.dateFormat(GetDateTo.Date) + "') ";
                }
                mysql = mysql + "  AND (Supplier_Id = " + Supplier_Id + ") ";

                mysql = mysql + "  ORDER BY    Transaction_Date ,ProcessNo,Id ";


                dtAccount = opp.return_DataTable(mysql);




                ///////////////«·Õ’Ê· ⁄·Ï «Ã„«·Ï «·Õ—ﬂ«  «·”«»ﬁÂ Ê«÷«› Â« ⁄·Ï —’Ìœ «Ê· «·„œÂ
                decimal Final_First_Balance_Madin = 0;
                decimal Final_First_Balance_Da2n = 0;
                decimal First_Balance = BalanceBefor;


                //Õ”«» —’Ìœ √Ê· «·„œÂ
                decimal First_Balance_Madin = 0;
                decimal First_Balance_Da2n = 0;

                //Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref   First_Balance_Madin, ref   First_Balance_Da2n, ref Equivalent_First_Balance_Madin, ref Equivalent_First_Balance_Da2n);

                //≈÷«›… —’Ìœ «Ê· «·„œÂ ⁄·Ï «·—’Ìœ «·„‰ﬁÊ·
                //decimal First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
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

                /////////////Õ”«» —’Ìœ «·Õ—ﬂ« 
                Set_ComputeBalanceNew(ref dtAccount, First_Balance);
                if (IsFountBalanceBefor == true)
                {
                    /////////////// ≈÷«›… —’Ìœ √Ê· «·„œÂ «Ê «·—’Ìœ «·„‰ﬁÊ·
                    DataRow dr11 = dtAccount.NewRow();
                    dr11["BalanceImport"] = Final_First_Balance_Madin;
                    dr11["BalanceExport"] = Final_First_Balance_Da2n;

                    decimal BalanceDiff1 = Final_First_Balance_Madin - Final_First_Balance_Da2n;

                    SetBalanceDiff(ref BalanceDiff1, ref dr11);

                    dr11["Balance"] = First_Balance;
                    //dr11["Notes"] = " "—’Ìœ √Ê· «·„œÂ"";

                    dr11["Import"] = ImportBefor;
                    dr11["Export"] = ExportBefor;
                    dr11["Reference_Type"] = 0;

                    dr11["Reference_Type_View"] = "—’Ìœ ”«»ﬁ";
                    dtAccount.Rows.InsertAt(dr11, 0);

                }


                Common.Make_TableColumnSerialME(ref dtAccount);

                //////////≈÷«›… «·«Ã„«·Ï
                decimal Import = 0;
                decimal Export = 0;

                decimal Balance_Import_Value = 0;
                decimal Balance_Export_Value = 0;


                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import = Import + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import = Common.RoundMe(Import);
                    }
                    catch { }

                    try
                    {
                        Export = Export + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export = Common.RoundMe(Export);
                    }
                    catch { }



                }
                if (Common.IsShowTotalTransaction == 1)
                {
                    DataRow dr = dtAccount.NewRow();

                    dr["Import"] = Import;
                    dr["Export"] = Export;
                    dr["Balance"] = 0;
                    dr["Notes"] = "≈Ã„«·Ï «·Õ—ﬂÂ";

                    dtAccount.Rows.Add(dr);
                }

                //////////////////Õ”«» «·—’Ìœ «·„‰ﬁÊ·
                decimal Balance_Mankool = Import - Export;

                Balance_Mankool = Balance_Mankool;
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
                DataRow dr2 = dtAccount.NewRow();
                dr2["Balance"] = Balance_Mankool;
                dr2["BalanceImport"] = Final_First_Balance_Madin;
                dr2["BalanceExport"] = Final_First_Balance_Da2n;

                decimal BalanceDiff = Final_First_Balance_Da2n - Final_First_Balance_Madin;
                SetBalanceDiff(ref BalanceDiff, ref dr2);
                //dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";
                dr2["Notes"] = Common.NotesBalanceLabelMankol;

                dtAccount.Rows.Add(dr2);



                return dtAccount;
            }
            catch (Exception ee)
            {
                // Excp = 1;

            }
            return dtAccount;
        }

        public DataTable Get_Suppliers_AccountComputeBalancePeriod(long Supplier_Id, long Year_Financial_Period_Id, DateTime Financial_Period_dtFom, bool _GetDateFrom, DateTime GetDateFrom, bool _GetDateTo, DateTime GetDateTo, bool DESC)
        {
            DataTable dtAccount = new DataTable();
            DateTime dtBefor = GetDateFrom;

            TimeSpan ts = new TimeSpan(1, 0, 0, 0);

            dtBefor = dtBefor.Subtract(ts);
            DataSet G_DS = new DataSet();

            operations opp = new operations();

            string mysql = "";
            decimal Prev_BalanceBefor = 0;
            decimal Prev_Import_Value = 0;
            decimal Prev_Export_Value = 0;



            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;
            decimal First_Balance = 0;
            string Raseed_Awel_Moda = "—’Ìœ √Ê· «·„œÂ";

            //Õ”«» —’Ìœ √Ê· «·„œÂ
            decimal First_Balance_Madin = 0;
            decimal First_Balance_Da2n = 0;


            try
            {
                bool IsFountBalanceBefor = false;
                int FirstRowToCompute = 0;
                if (_GetDateFrom == true)
                {
                    

                    if (GetDateFrom.Date > Financial_Period_dtFom)
                    {
                        //////////////Õ”«» «·› —… „‰  «—ÌŒ »œ«ÌÂ «·› —… «·„«·ÌÂ Õ Ï  »œ«Ì…  «—ÌŒ «·»ÕÀ  

                        mysql = @"SELECT      SUM(Import)   AS Import, SUM(Export)   AS Export,  (   SUM(Import) - SUM(Export) ) AS Balance FROM  Suppliers_Accounts ";
                        mysql = mysql + " WHere (CONVERT(DATE, Transaction_Date)  >= '" + operations.dateFormat(Financial_Period_dtFom.Date) + "') AND  (CONVERT(DATE, Transaction_Date)  <  '" + operations.dateFormat(GetDateFrom.Date) + "') ";
                        mysql = mysql + "  AND (Supplier_Id = " + Supplier_Id + ") And  Reference_Type <> -7 ";
                        DataTable dt_Prev = opp.return_DataTable(mysql);
                        //if (dt.Rows.Count > 0)
                        //{
                        //    IsFountBalanceBefor = true;
                        //    try
                        //    {
                        //        BalanceBefor = decimal.Parse(dt.Rows[0]["Balance"].ToString());
                        //        ImportBefor = decimal.Parse(dt.Rows[0]["Import"].ToString());
                        //        ExportBefor = decimal.Parse(dt.Rows[0]["Export"].ToString());


                        //    }
                        //    catch { BalanceBefor = 0; }
                        //}

                        try
                        {
                            if (dt_Prev.Rows.Count > 0)
                            {
                                Prev_BalanceBefor = decimal.Parse(dt_Prev.Rows[0]["Balance"].ToString());
                                Prev_Import_Value = decimal.Parse(dt_Prev.Rows[0]["Import"].ToString());
                                Prev_Export_Value = decimal.Parse(dt_Prev.Rows[0]["Export"].ToString());
                                IsFountBalanceBefor = true;
                                Raseed_Awel_Moda = "—’Ìœ ”«»ﬁ";
                            }
                        }
                        catch { Prev_BalanceBefor = 0; }

                    }


                    long Branch_Id = Common.Branch_Id;
                    
                    SupplierCLS supplierCLS = new SupplierCLS(  opp);
                    supplierCLS.GetSupplierNoTracking(Supplier_Id);
                    long Submain_Account_ID = supplierCLS.sp.SubAccount_Id??0;
                    AccountEntry_CLS accountEntry_CLS = new AccountEntry_CLS();
                    
                   
                    decimal Equivalent_First_Balance_Madin_ = 0;
                    decimal Equivalent_First_Balance_Da2n_ = 0;

                    accountEntry_CLS.Get_FirstBalance_Period(Year_Financial_Period_Id, Branch_Id, Submain_Account_ID, ref First_Balance_Madin, ref First_Balance_Da2n, ref Equivalent_First_Balance_Madin_, ref Equivalent_First_Balance_Da2n_);
                     
                    //≈÷«›… —’Ìœ «Ê· «·„œÂ ⁄·Ï «·—’Ìœ «·„‰ﬁÊ·
                      First_Balance = (First_Balance_Madin - First_Balance_Da2n) + (Prev_Import_Value - Prev_Export_Value);
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
                }

                mysql = " SELECT Suppliers_Accounts.*,'' As BalanceImport,'' As BalanceExport FROM Suppliers_Accounts WHERE Flage_View=1      ";
                if (_GetDateFrom == true)
                {

                    mysql = mysql + " AND (CONVERT(DATE, Transaction_Date) >= '" + operations.dateFormat(GetDateFrom.Date) + "')  ";
                }
                if (_GetDateTo == true)
                {
                    mysql = mysql + "   AND (CONVERT(DATE, Transaction_Date) <= '" + operations.dateFormat(GetDateTo.Date) + "') ";
                }
                mysql = mysql + "  AND (Supplier_Id = " + Supplier_Id + ") And  Reference_Type <> -7";

                mysql = mysql + "  ORDER BY    Transaction_Date ,ProcessNo,Id ";


                dtAccount = opp.return_DataTable(mysql);


                 

                /////////////Õ”«» —’Ìœ «·Õ—ﬂ« 
                Set_ComputeBalanceNew(ref dtAccount, First_Balance);
                 
                    /////////////// ≈÷«›… —’Ìœ √Ê· «·„œÂ «Ê «·—’Ìœ «·„‰ﬁÊ·
                    DataRow dr11 = dtAccount.NewRow();
                    dr11["BalanceImport"] = Final_First_Balance_Madin;
                    dr11["BalanceExport"] = Final_First_Balance_Da2n;

                    decimal BalanceDiff1 = Final_First_Balance_Madin - Final_First_Balance_Da2n;

                    SetBalanceDiff(ref BalanceDiff1, ref dr11);

                    dr11["Balance"] = First_Balance;
                    //dr11["Notes"] = " "—’Ìœ √Ê· «·„œÂ"";

                    dr11["Import"] = Prev_Import_Value;
                    dr11["Export"] = Prev_Export_Value;
                    dr11["Reference_Type"] = 0;

                    dr11["Reference_Type_View"] = Raseed_Awel_Moda  ;
                    dtAccount.Rows.InsertAt(dr11, 0);
                 


                Common.Make_TableColumnSerialME(ref dtAccount);

                //////////≈÷«›… «·«Ã„«·Ï
                decimal Import = 0;
                decimal Export = 0;

                decimal Balance_Import_Value = 0;
                decimal Balance_Export_Value = 0;


                for (int x = 0; x < dtAccount.Rows.Count; x++)
                {

                    try
                    {
                        Import = Import + decimal.Parse(dtAccount.Rows[x]["Import"].ToString());
                        Import = Common.RoundMe(Import);
                    }
                    catch { }

                    try
                    {
                        Export = Export + decimal.Parse(dtAccount.Rows[x]["Export"].ToString());
                        Export = Common.RoundMe(Export);
                    }
                    catch { }



                }
                if (Common.IsShowTotalTransaction == 1)
                {
                    DataRow dr = dtAccount.NewRow();

                    dr["Import"] = Import;
                    dr["Export"] = Export;
                    dr["Balance"] = 0;
                    dr["Notes"] = "≈Ã„«·Ï «·Õ—ﬂÂ";

                    dtAccount.Rows.Add(dr);
                }

                //////////////////Õ”«» «·—’Ìœ «·„‰ﬁÊ·
                decimal Balance_Mankool = Import - Export;

                Balance_Mankool = Balance_Mankool;
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
                DataRow dr2 = dtAccount.NewRow();
                dr2["Balance"] = Balance_Mankool;
                dr2["BalanceImport"] = Final_First_Balance_Madin;
                dr2["BalanceExport"] = Final_First_Balance_Da2n;

                decimal BalanceDiff = Final_First_Balance_Da2n - Final_First_Balance_Madin;
                SetBalanceDiff(ref BalanceDiff, ref dr2);
                //dr2["Notes"] = "—’Ìœ „‰ﬁÊ·";
                dr2["Notes"] = Common.NotesBalanceLabelMankol;

                dtAccount.Rows.Add(dr2);



                return dtAccount;
            }
            catch (Exception ee)
            {
                // Excp = 1;

            }
            return dtAccount;
        }


        void Set_ComputeBalanceNew(ref DataTable dt, decimal First_Balance)
        {
            decimal Final_First_Balance_Madin = 0;
            decimal Final_First_Balance_Da2n = 0;


            decimal Import = 0;
            decimal Export = 0;

            decimal Balance = First_Balance;

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                Import = 0;
                Export = 0;

                try
                {
                    Import = Import + decimal.Parse(dt.Rows[x]["Import"].ToString());
                    Import = Common.RoundMe(Import);
                }
                catch { }
                try
                {
                    Export = Export + decimal.Parse(dt.Rows[x]["Export"].ToString());
                    Export = Common.RoundMe(Export);
                }
                catch { }




                Balance = Balance + (Import - Export);
                Balance = Common.RoundMe(Balance);
                dt.Rows[x]["Balance"] = Balance;

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


                dt.Rows[x]["BalanceImport"] = Final_First_Balance_Madin;
                dt.Rows[x]["BalanceExport"] = Final_First_Balance_Da2n;
                decimal BalanceDiff = Final_First_Balance_Da2n - Final_First_Balance_Madin;
                SetBalanceDiff(ref BalanceDiff, ref dt, x);

            }
        }

        void SetBalanceDiff(ref decimal BalanceDiff, ref DataTable dt, int x)
        {
            dt.Rows[x]["BalanceDiff"] = BalanceDiff;
            //string BalanceDiff_ = "";
            //if (BalanceDiff < 0)
            //{
            //    BalanceDiff = BalanceDiff * -1;
            //    BalanceDiff_ = "( " + BalanceDiff_.ToString() + " )";
            //}
            //if (BalanceDiff_.Trim() == "")
            //{
            //    dt.Rows[x]["BalanceDiff"] = "";
            //}
            //else
            //{
            //    dt.Rows[x]["BalanceDiff"] = Common.GetValuesAfterSetArabic(BalanceDiff_);
            //}
        }

        public DataSet Get_Supplier_List()
        {

            DataSet G_DS = new DataSet();
            try
            {
                string mysql = "SELECT  * FROM Supplier WHERE (Active = 1)  ORDER BY Name ";
                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Customer");
                // Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                //Excp =  1;
                return G_DS;
            }
        }
        public void Add_Invoice_Total_To_Account(ref int Excp, long Invoice_id, int Supplier_Id, decimal Invoice_Net_Total, decimal Invoice_Total)
        {
            /*try
			{	
				string mysql="";
				  
			 arr.Clear();
				mysql="UPDATE Unit_item SET Reserved_Balance_Larg_Qnty = Reserved_Balance_Larg_Qnty - "+Diff_Larg_Qnty+" , Reserved_Balance_Small_Qnty = Reserved_Balance_Small_Qnty - "+Diff_Small_Qnty+"  WHERE (Full_qua = "+New_Larg_Balance_+") AND (D_Qua = "+New_Small_Balance_+")   AND (Reserved_Balance_Larg_Qnty="+Reserved_Balance_Larg_Qnty+") AND (Reserved_Balance_Small_Qnty="+Reserved_Balance_Small_Qnty+") AND  (id = "+Unit_Item_Table_Id+")";
				arr.Add(mysql);
				

				mysql=" UPDATE Bills_Details SET Larg_Qnty ="+Larg_Qnty+", Small_Qnty ="+Small_Qnty+", Larg_Unit_Price ="+Larg_Unit_Price+", Small_Unit_Price ="+Small_Unit_Price+", Total ="+New_Total_Value+", ATS05 ="+ATS05+", TaxSales10 ="+TaxSales10+", Notes ='"+Notes+"', Qnty_Another ="+Qnty_Another+" ";
				mysql=mysql + " WHERE (Id ="+Bills_DetailsTable_Id+") ";
				arr.Add(mysql);
				mysql="UPDATE Bills_List SET Total =Total + "+Diff_Totals+" , Net_Total =Total + "+Diff_Totals+" WHERE ( Id = "+Bills_List_table_Id+")";
				arr.Add(mysql);
				
				operations opp=new operations();
				opp.insert_me_transaction(ref Excp,1,arr);
				 
				opp=null;
				 
			}
			catch(Exception e)
			{
				Excp=1;
				throw(e);
			}
			*/
        }
        decimal MyRound(decimal V)
        {
            return Common.RoundMePositive(V);
            /*
           double d = double.Parse(V.ToString());
           d = Common.RoundMe(d);
           decimal x = (decimal)d;
           return x;
             * */
        }
        public void Set_SuppliersAccount_Balance_Reverse(ref int Excp, long Supplier_Id, long Invoice_No, decimal Net_Total)
        {

            try
            {
                operations opp = new operations();
                Net_Total = MyRound(Net_Total);
                string mysql = "";
                ArrayList arr = new ArrayList();
                arr.Clear();
                mysql = "  UPDATE Supplier SET Balance = Balance - " + Net_Total + " ,Balance_Actual=Balance_Actual +  " + Net_Total + " WHERE (Id = " + Supplier_Id + ")";
                arr.Add(mysql);

                UpdateProcessNo(ref Excp, ref opp, "Bills_List_Reverse", Invoice_No);

                mysql = " INSERT INTO Suppliers_Accounts (ProcessNo,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                mysql = mysql + " SELECT Bills_List_Reverse.ProcessNo, Bills_List_Reverse.Supplier_Id, Bills_List_Reverse.Invoice_Id,Bills_List_Reverse.id, Bills_List_Reverse.Net_Total, Supplier.Balance , 2 AS Transaction_Type,Bills_List_Reverse.Invoice_Date,-3 AS Reference_Type ";
                mysql = mysql + " FROM Supplier INNER JOIN  Bills_List_Reverse ON Supplier.Id = Bills_List_Reverse.Supplier_Id ";
                mysql = mysql + " WHERE (Supplier.Id = " + Supplier_Id + ") AND (Bills_List_Reverse.Id = " + Invoice_No + ") ";
                arr.Add(mysql);

                mysql = "UPDATE Bills_List_Reverse SET Supplier_BalanceSet = 1,Editable=0 WHERE (Supplier_Id = " + Supplier_Id + ") ";
                arr.Add(mysql);

                opp.insert_me_transaction(ref Excp, 1, arr);
                opp = null;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }


        }

        public void Set_SuppliersAccount_Balance(ref int Excp, ref operations opp, DateTime Transaction_Date_, long Supplier_Id, decimal Net_Total, long Process_No, int Reference_Type)
        {

            Net_Total = MyRound(Net_Total);
            string mysql = "";
            long Reference_Id = 0;
            long Reference_Type_Table_Id = Process_No;
            decimal Import = Net_Total;
            int Transaction_Type = 1;
            DateTime Transaction_Date = Transaction_Date_;
            // int Reference_Type = -1;  /// ·· ÿ—Ì“
            decimal Balance = 0;

            mysql = " Update Supplier set Balance=Balance+" + Net_Total + " ,Balance_Actual=Balance_Actual+ " + Net_Total + " where id=" + Supplier_Id;
            opp.insert_me(mysql);
            DataSet dss = opp.return_dataset("Select * from Supplier where id=" + Supplier_Id);
            try
            {
                Balance = decimal.Parse(dss.Tables[0].Rows[0]["Balance"].ToString());
            }
            catch
            {
                Balance = 0;
                Excp = 1;
                return;
            }

            mysql = " INSERT INTO Suppliers_Accounts (Supplier_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            mysql = mysql + " Values(" + Supplier_Id + "," + Reference_Id + "," + Reference_Type_Table_Id + "," + Import + "," + Balance + "," + Transaction_Type + ",'" + operations.dateFormat(Transaction_Date) + "'," + Reference_Type + ")";
            opp.insert_me(mysql);
            Excp = 0;

        }




        public void Set_SuppliersAccount_Balance(ref int Excp, DateTime Transaction_Date_, long Supplier_Id, decimal Net_Total, long Process_No, int Reference_Type)
        {
            try
            {
                Net_Total = MyRound(Net_Total);
                string mysql = "";
                long Reference_Id = 0;
                long Reference_Type_Table_Id = Process_No;
                decimal Import = Net_Total;
                int Transaction_Type = 1;
                DateTime Transaction_Date = Transaction_Date_;
                // int Reference_Type = -1;  /// ·· ÿ—Ì“
                decimal Balance = 0;
                operations opp = new operations();
                mysql = " Update Supplier set Balance=Balance+" + Net_Total + " ,Balance_Actual=Balance_Actual+ " + Net_Total + " where id=" + Supplier_Id;
                opp.update_me(mysql);
                DataSet dss = opp.return_dataset("Select * from Supplier where id=" + Supplier_Id);
                try
                {
                    Balance = decimal.Parse(dss.Tables[0].Rows[0]["Balance"].ToString());
                }
                catch
                {
                    Balance = 0;
                    Excp = 1;
                    return;
                }

                mysql = " INSERT INTO Suppliers_Accounts (Supplier_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                mysql = mysql + " Values(" + Supplier_Id + "," + Reference_Id + "," + Reference_Type_Table_Id + "," + Import + "," + Balance + "," + Transaction_Type + ",'" + operations.dateFormat(Transaction_Date) + "'," + Reference_Type + ")";
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch
            {
                Excp = 1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Excp"></param>
        /// <param name="Transaction_Date_"></param>
        /// <param name="Supplier_Id"></param>
        /// <param name="Net_Total"></param>
        /// <param name="Process_No"></param>
        /// <param name="Reference_Type"></param>
        /// <param name="MadinOrDa2n">Madin=1 Or Da2n=2</param>
        public void Set_SuppliersAccount_Balance(ref int Excp, DateTime Transaction_Date_, long Supplier_Id, decimal Net_Total, long Process_No, int Reference_Type, int MadinOrDa2n)
        {
            try
            {
                Net_Total = MyRound(Net_Total);
                string mysql = "";
                long Reference_Id = 0;
                long Reference_Type_Table_Id = Process_No;
                decimal Import = Net_Total;
                int Transaction_Type = 1;
                DateTime Transaction_Date = Transaction_Date_;
                // int Reference_Type = -1;  /// ·· ÿ—Ì“
                decimal Balance = 0;
                operations opp = new operations();
                mysql = " Update Supplier set Balance=Balance+" + Net_Total + " ,Balance_Actual=Balance_Actual+ " + Net_Total + " where id=" + Supplier_Id;
                opp.update_me(mysql);
                DataSet dss = opp.return_dataset("Select * from Supplier where id=" + Supplier_Id);
                try
                {
                    Balance = decimal.Parse(dss.Tables[0].Rows[0]["Balance"].ToString());
                }
                catch
                {
                    Balance = 0;
                    Excp = 1;
                    return;
                }
                if (Reference_Type == -9) // „»Ì⁄«  œ‘  ·„Ê—œ
                {
                    Reference_Id = Process_No;
                }
                // UpdateProcessNo(ref Excp, ref opp, "Bills_List", Invoice_No);
                if (MadinOrDa2n == 2)
                {
                    mysql = " INSERT INTO Suppliers_Accounts (Supplier_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                    mysql = mysql + " Values(" + Supplier_Id + "," + Reference_Id + "," + Reference_Type_Table_Id + "," + Import + "," + Balance + "," + Transaction_Type + ",'" + operations.dateFormat(Transaction_Date) + "'," + Reference_Type + ")";
                }
                else if (MadinOrDa2n == 1)
                {
                    Import = Import * -1;
                    mysql = " INSERT INTO Suppliers_Accounts (Supplier_Id, Reference_Id,Reference_Type_Table_Id, export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                    mysql = mysql + " Values(" + Supplier_Id + "," + Reference_Id + "," + Reference_Type_Table_Id + "," + Import + "," + Balance + "," + Transaction_Type + ",'" + operations.dateFormat(Transaction_Date) + "'," + Reference_Type + ")";

                }

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch
            {
                Excp = 1;
            }
        }

        public void Set_SuppliersAccount_Balance(ref int Excp, int Supplier_Id, long Invoice_No, decimal Net_Total, string Notes)
        {

            try
            {
                operations opp = new operations();
                string mysql = "";
                ArrayList arr = new ArrayList();
                arr.Clear();
                mysql = "  UPDATE Supplier SET Balance = Balance + " + Net_Total + " ,Balance_Actual=Balance_Actual +  " + Net_Total + " WHERE (Id = " + Supplier_Id + ")";
                arr.Add(mysql);

                UpdateProcessNo(ref Excp, ref opp, "Bills_List", Invoice_No);

                mysql = " INSERT INTO Suppliers_Accounts (ProcessNo,Notes,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                mysql = mysql + " SELECT Bills_List.ProcessNo, N'" + Notes + "',Bills_List.Supplier_Id, Bills_List.Invoice_Id,Bills_List.id, Bills_List.Net_Total, Supplier.Balance , 1 AS Transaction_Type,Bills_List.Invoice_Date,2 AS Reference_Type ";
                mysql = mysql + " FROM Supplier INNER JOIN  Bills_List ON Supplier.Id = Bills_List.Supplier_Id ";
                mysql = mysql + " WHERE (Supplier.Id = " + Supplier_Id + ") AND (Bills_List.Id = " + Invoice_No + ") ";
                arr.Add(mysql);

                mysql = "UPDATE Bills_List SET Supplier_BalanceSet = 1,Editable=0 WHERE (Supplier_Id = " + Supplier_Id + ") ";
                arr.Add(mysql);

                opp.insert_me_transaction(ref Excp, 1, arr);
                opp = null;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }


        }
        public void Set_SuppliersAccount_Balance(ref int Excp, ref operations opp, long Supplier_Id, long Invoice_No, decimal Net_Total)
        {

            string mysql = "";

            mysql = "  UPDATE Supplier SET Balance = Balance + " + Net_Total + " ,Balance_Actual=Balance_Actual +  " + Net_Total + " WHERE (Id = " + Supplier_Id + ")";
            opp.insert_me(mysql);

            UpdateProcessNo(ref Excp, ref opp, "Bills_List", Invoice_No);

            mysql = " INSERT INTO Suppliers_Accounts (ProcessNo,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            mysql = mysql + " SELECT Bills_List.ProcessNo, Bills_List.Supplier_Id, Bills_List.Invoice_Id,Bills_List.id, Bills_List.Net_Total, Supplier.Balance , 1 AS Transaction_Type,Bills_List.Invoice_Date,2 AS Reference_Type ";
            mysql = mysql + " FROM Supplier INNER JOIN  Bills_List ON Supplier.Id = Bills_List.Supplier_Id ";
            mysql = mysql + " WHERE (Supplier.Id = " + Supplier_Id + ") AND (Bills_List.Id = " + Invoice_No + ") ";
            opp.insert_me(mysql);

            mysql = "UPDATE Bills_List SET Supplier_BalanceSet = 1,Editable=0 WHERE (Supplier_Id = " + Supplier_Id + ") ";
            opp.insert_me(mysql);





        }
        public void Set_SuppliersAccount_BalanceReverse(ref int Excp, ArrayList arr, long Supplier_Id, long Invoice_No, decimal Net_Total)
        {

            try
            {
                Net_Total = MyRound(Net_Total);
                string mysql = "";
                // ArrayList arr = new ArrayList();
                // arr.Clear();
                operations opp = new operations();
                mysql = "  UPDATE Supplier SET Balance = Balance - " + Net_Total + " ,Balance_Actual=Balance_Actual -  " + Net_Total + " WHERE (Id = " + Supplier_Id + ")";
                arr.Add(mysql);

                UpdateProcessNo(ref Excp, ref opp, "Bills_List_Reverse", Invoice_No);

                mysql = " INSERT INTO Suppliers_Accounts (ProcessNo,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
                mysql = mysql + " SELECT Bills_List_Reverse.ProcessNo,Bills_List_Reverse.Supplier_Id, Bills_List_Reverse.Invoice_Id,Bills_List_Reverse.id, Bills_List_Reverse.Net_Total, Supplier.Balance , 2 AS Transaction_Type,Bills_List_Reverse.Invoice_Date,8 AS Reference_Type ";
                mysql = mysql + " FROM Supplier INNER JOIN  Bills_List_Reverse ON Supplier.Id = Bills_List_Reverse.Supplier_Id ";
                mysql = mysql + " WHERE (Supplier.Id = " + Supplier_Id + ") AND (Bills_List_Reverse.Id = " + Invoice_No + ") ";
                arr.Add(mysql);

                mysql = "UPDATE Bills_List_Reverse SET Supplier_BalanceSet = 1,Editable=0 WHERE (Supplier_Id = " + Supplier_Id + ") ";
                arr.Add(mysql);

                int Status = opp.insert_me_transaction(arr);
                opp = null;
                if (Status == 1)
                {
                    Excp = 0;

                }
                else
                {
                    Excp = 1;
                }

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }


        }
        public void Set_SuppliersAccount_BalanceReverse(ref int Excp, ref operations opp, long Supplier_Id, long Bills_List_Reverse_Id, decimal Net_Total, string Notes)
        {
            string str = "";
            str = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Bills_List_Reverse_Id.ToString() + " and Reference_Type=" + Common.BilleReverse_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = MyRound(Net_Total);
            string mysql = "";

            mysql = "  UPDATE Supplier SET Balance = Balance - " + Net_Total + " ,Balance_Actual=Balance_Actual -  " + Net_Total + " WHERE (Id = " + Supplier_Id + ")";
            opp.insert_me(mysql);

            UpdateProcessNo(ref Excp, ref opp, "Bills_List_Reverse", Bills_List_Reverse_Id);



            mysql = " INSERT INTO Suppliers_Accounts (ProcessNo,Notes,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            mysql = mysql + " SELECT Bills_List_Reverse.ProcessNo,N'" + Notes + "', Bills_List_Reverse.Supplier_Id, Bills_List_Reverse.Invoice_Id,Bills_List_Reverse.id, Bills_List_Reverse.Net_Total, Supplier.Balance , 2 AS Transaction_Type,Bills_List_Reverse.Invoice_Date,8 AS Reference_Type ";
            mysql = mysql + " FROM Supplier INNER JOIN  Bills_List_Reverse ON Supplier.Id = Bills_List_Reverse.Supplier_Id ";
            mysql = mysql + " WHERE (Supplier.Id = " + Supplier_Id + ") AND (Bills_List_Reverse.Id = " + Bills_List_Reverse_Id + ") ";
            opp.insert_me(mysql);

            mysql = "UPDATE Bills_List_Reverse SET Supplier_BalanceSet = 1,Editable=0 WHERE (Supplier_Id = " + Supplier_Id + ") ";
            opp.insert_me(mysql);
        }
        public void Update_Supplier_BalanceWithEditBill_Reverse(ref int Excp, ref operations opp, DateTime Invoice_Date, long Supplier_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            Set_Supplier_Balance_MakeBill_Reverse(ref Excp, ref opp, Supplier_Id, New_Invoice_ListTable_id, Net_Total, Notes);


        }
        public void Set_Supplier_Balance_MakeBill(ref int Excp, ref operations opp, long Supplier_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            if (Common.IsHavePermissionSysMenu(Common.User_Id, 345, Common.Project_ID, 0) | (Common.Is_Set_Supplier_Balance_With_Items == 1))
            {
                this.Set_Supplier_Balance_WithItems_Bills_List(ref Excp, ref opp, Supplier_Id, New_Invoice_ListTable_id, Net_Total, Notes);
            }
            else
            {
                this.Set_Supplier_Balance_(ref Excp, ref opp, Supplier_Id, New_Invoice_ListTable_id, Net_Total, Notes);
            }
        }
        private void Set_Supplier_Balance_WithItems_Bills_List(ref int Excp, ref operations opp, long Supplier_Id, long New_Bill_ListTable_id, decimal Net_Total, string Notes)
        {
            string str = "";
            str = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + New_Bill_ListTable_id.ToString() + " and Reference_Type=" + Common.Bill_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);
            string[] textArray1 = new string[] { "  UPDATE Supplier SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Supplier_Id.ToString(), ")" };
            str = string.Concat(textArray1);
            opp.insert_me(str);

            UpdateProcessNo(ref Excp, ref opp, "Bills_List", New_Bill_ListTable_id);


            str = " INSERT INTO Suppliers_Accounts (ProcessNo,Larg_Unit_Price,Item_Id,Item_Name,Notes,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type, Qnty,Larg_Qnty,Small_Qnty, Item_No, Item_Size, Qnty_Size,Qnty_value,Height_Width_Item ) " + " SELECT   Bills_List.ProcessNo, Bills_Details.Larg_Unit_Price,Bills_Details.Item_Id,Item.Item_Name, Bills_Details.NameSpecification, Bills_List.Supplier_Id, Bills_List.Invoice_Id, Bills_List.ID, Bills_Details.Total, Supplier.Balance, 2 AS Transaction_Type, Bills_List.Invoice_Date,   " + Common.Bill_Type_Id.ToString() + " AS Reference_Type, Bills_Details.Larg_Qnty,Bills_Details.Larg_Qnty,Bills_Details.Small_Qnty, Bills_Details.Qnty_Another, Item.Height_Width_Str, CONVERT(nvarchar(100), Bills_Details.QntyWidth)  + N'' + CONVERT(nvarchar(100), Bills_Details.QntyLenght) AS Qnty_Size,  Bills_Details.Qnty_value, CONVERT(nvarchar(100), Item.Width_) + N'' + CONVERT(nvarchar(100), Item.Height_) AS Height_Width_Item  ";
            str = str + " FROM Supplier INNER JOIN  Bills_List ON Supplier.Id = Bills_List.Supplier_Id INNER JOIN  Bills_Details ON Bills_List.ID = Bills_Details.Invoice_Id INNER JOIN  Item ON Bills_Details.Item_Id = Item.id ";
            str = str + " WHERE (Supplier.Id = " + Supplier_Id.ToString() + ") AND (Bills_List.Id = " + New_Bill_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
            str = "UPDATE Bills_List SET Supplier_BalanceSet = 1,Editable=0 WHERE (id = " + New_Bill_ListTable_id.ToString() + ") ";
            opp.insert_me(str);

            SetSet_Supplier_Balance_With_Bills_List(ref opp, New_Bill_ListTable_id);
        }
        //public BillOperationListCLS invL = new BillOperationListCLS();


        void SetSet_Supplier_Balance_With_Bills_ListDataExport(ref operations opp, long New_Bill_ListTable_id, string Notes, decimal Value)
        {

            string str = "";
            str = @" INSERT INTO Suppliers_Accounts (ProcessNo, Notes, Supplier_Id, Reference_Id, Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type)  ";
            str = str + @" SELECT Bills_List.ProcessNo, '" + Notes + "' AS Notes, Bills_List.Supplier_Id, Bills_List.Invoice_Id, Bills_List.ID, " + Value + ", Supplier.Balance,  2 AS Transaction_Type, Bills_List.Invoice_Date, " + Common.Bill_Type_Id.ToString() + " AS Reference_Type ";
            str = str + @" FROM Supplier INNER JOIN Bills_List ON Supplier.Id = Bills_List.Supplier_Id ";
            str = str + @" WHERE  (Bills_List.Id = " + New_Bill_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
        }
        void SetSet_Supplier_Balance_With_Bills_ListDataExport_(ref operations opp, long New_Bill_ListTable_id, string Notes, decimal ValueImport, decimal ValueExport, int Transaction_Type)
        {

            string str = "";
            str = @" INSERT INTO Suppliers_Accounts (ProcessNo, Notes, Supplier_Id, Reference_Id, Reference_Type_Table_Id,Import, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type)  ";
            str = str + @" SELECT Bills_List.ProcessNo, '" + Notes + "' AS Notes, Bills_List.Supplier_Id, Bills_List.Invoice_Id, Bills_List.ID," + ValueImport + ", " + ValueExport + ", Supplier.Balance,  " + Transaction_Type + " AS Transaction_Type, Bills_List.Invoice_Date, " + Common.Bill_Type_Id.ToString() + " AS Reference_Type ";
            str = str + @" FROM Supplier INNER JOIN Bills_List ON Supplier.Id = Bills_List.Supplier_Id ";
            str = str + @" WHERE  (Bills_List.Id = " + New_Bill_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
        }
        void SetSet_Supplier_Balance_With_Bills_ReverseListDataExport_(ref operations opp, long New_Bill_ListTable_id, string Notes, decimal ValueImport, decimal ValueExport, int Transaction_Type)
        {

            string str = "";
            str = @" INSERT INTO Suppliers_Accounts (ProcessNo, Notes, Supplier_Id, Reference_Id, Reference_Type_Table_Id,Import, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type)  ";
            str = str + @" SELECT Bills_List_Reverse.ProcessNo, '" + Notes + "' AS Notes, Bills_List_Reverse.Supplier_Id, Bills_List_Reverse.Invoice_Id, Bills_List_Reverse.ID," + ValueImport + ", " + ValueExport + ", Supplier.Balance,  " + Transaction_Type + " AS Transaction_Type, Bills_List_Reverse.Invoice_Date, " + Common.Bill_Type_Id.ToString() + " AS Reference_Type ";
            str = str + @" FROM Supplier INNER JOIN Bills_List_Reverse ON Supplier.Id = Bills_List_Reverse.Supplier_Id ";
            str = str + @" WHERE  (Bills_List_Reverse.Id = " + New_Bill_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
        }
        public void Set_Supplier_Balance_(ref int Excp, ref operations opp, long Supplier_Id, long New_Bill_ListTable_id, decimal Net_Total, string Notes)
        {
            string str = "";
            str = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + New_Bill_ListTable_id.ToString() + " and Reference_Type=" + Common.Bill_Type_Id.ToString();
            opp.insert_me(str);
            string[] textArray1 = new string[] { "  UPDATE Supplier SET Balance = Balance + ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual +  ", Net_Total.ToString(), " WHERE (Id = ", Supplier_Id.ToString(), ")" };
            str = string.Concat(textArray1);
            opp.insert_me(str);

            UpdateProcessNo(ref Excp, ref opp, "Bills_List", New_Bill_ListTable_id);


            str = " INSERT INTO Suppliers_Accounts (ProcessNo,Notes,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Import, Balance, Transaction_Type, Transaction_Date,Reference_Type) ";
            string[] textArray2 = new string[] { str, " SELECT Bills_List.ProcessNo,N'", Notes, "',Bills_List.Supplier_Id, Bills_List.Invoice_Id,Bills_List.id, Bills_List.Net_Total, Supplier.Balance , 1 AS Transaction_Type,Bills_List.Invoice_Date,", Common.Bill_Type_Id.ToString(), " AS Reference_Type " };
            str = string.Concat(textArray2) + " FROM Supplier INNER JOIN  Bills_List ON Supplier.Id = Bills_List.Supplier_Id ";
            string[] textArray3 = new string[] { str, " WHERE (Supplier.Id = ", Supplier_Id.ToString(), ") AND (Bills_List.Id = ", New_Bill_ListTable_id.ToString(), ") " };
            str = string.Concat(textArray3);
            opp.insert_me(str);
            str = "UPDATE Bills_List SET Supplier_BalanceSet = 1,Editable=0 WHERE (Supplier_Id = " + Supplier_Id.ToString() + ") ";
            opp.insert_me(str);



        }
        void SetSet_Supplier_Balance_With_Bills_List(ref operations opp, long Bills_List_Id)
        {
            int AccountType = 0;
            int Transaction_Type = 0;
            string Notes = " ";
            string ValueImport = "0";
            string ValueExport = "0";
            Account_Entry_Invoice_CLS ac = new Account_Entry_Invoice_CLS();
            int Excp = 0;
            string Msg = "";
            DataTable dt = ac.Get_EntryFor_Bills(ref Excp, ref Msg, ref opp, Bills_List_Id, DateTime.Now.Date, DateTime.Now.Date, 0);
            if (dt.Rows.Count == 0)
            {
                return;
            }
            decimal ATS05_Value = 0;// decimal.Parse(dt.Rows[0]["ATS05_Value"].ToString());
            decimal Net_Total_CommissionValue = 0;// decimal.Parse(dt.Rows[0]["Net_Total_CommissionValue"].ToString());
            decimal TaxSales10_Value = 0;//decimal.Parse(dt.Rows[0]["TaxSales10_Value"].ToString());
            decimal DAMGHAT = 0;// decimal.Parse(dt.Rows[0]["Damaghat"].ToString());
            decimal Load_Payment = 0;//decimal.Parse(dt.Rows[0]["Load_Payment"].ToString());
            decimal Total_List3Mola = 0;
            try
            {
                Total_List3Mola = decimal.Parse(dt.Rows[0]["Total_List3Mola"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                ATS05_Value = decimal.Parse(dt.Rows[0]["ATS05_Value"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                Net_Total_CommissionValue = decimal.Parse(dt.Rows[0]["Net_Total_CommissionValue"].ToString());

            }
            catch (Exception ee)
            {
            }
            try
            {
                TaxSales10_Value = decimal.Parse(dt.Rows[0]["TaxSales10_Value"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                DAMGHAT = decimal.Parse(dt.Rows[0]["Damaghat"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                Load_Payment = decimal.Parse(dt.Rows[0]["Load_Payment"].ToString());
            }
            catch (Exception ee)
            {
            }
            if (ATS05_Value > 0)
            {
                ////Õ”«» √—»«Õ  Ã«—Ì… Ê’‰«⁄Ì…
                Transaction_Type = 2;
                AccountType = Common.ATS05_Value_Account_Type_ForSales;


                Notes = "√—»«Õ  Ã«—Ì… Ê’‰«⁄Ì…";
                ValueImport = "0";
                ValueExport = "Bills_List.ATS05_Value";
                SetSet_Supplier_Balance_With_Bills_ListDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);

            }
            if (Net_Total_CommissionValue > 0)
            {

                ////«·Œ’„ «·„”„ÊÕ Ì…
                Transaction_Type = 2;
                AccountType = Common.CommissionValue_Account_Type;

                //EntryValue = EntryValue * Factor;

                Notes = "«·Œ’„ «·„”„ÊÕ Ì…";
                ValueImport = "0";
                ValueExport = "Bills_List.Net_Total_CommissionValue";
                SetSet_Supplier_Balance_With_Bills_ListDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (TaxSales10_Value > 0)
            {


                ////÷—«∆» «·„»Ì⁄« 
                Transaction_Type = 1;
                AccountType = Common.TaxSales10_Account_Type_ForSales;


                Notes = "÷—«∆» «·„»Ì⁄« ";
                ValueImport = "Bills_List.TaxSales10_Value";
                ValueExport = "0";
                SetSet_Supplier_Balance_With_Bills_ListDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (Total_List3Mola > 0)
            {


                ////⁄„Ê·… «·»Ì⁄ - «·„‘ —Ì«  Êﬂ«·… «·Œ÷«—
                Transaction_Type = 2;
                AccountType = Common.Total_List3Mola_Account_Type_ForSales;


                Notes = "⁄„Ê·… «·»Ì⁄";
                ValueImport = "0";
                ValueExport = "Bills_List.Total_List3Mola";
                SetSet_Supplier_Balance_With_Bills_ListDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (DAMGHAT > 0)
            {

                ////«·œ„€…
                Transaction_Type = 1;
                AccountType = Common.DAMGHAT_Account_Type;

                //EntryValue = EntryValue * Factor;
                Notes = "«·œ„€…  ";
                ValueImport = "Bills_List.Damaghat";
                ValueExport = "0";
                SetSet_Supplier_Balance_With_Bills_ListDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);

            }
            if (Load_Payment > 0)
            {


                ////„’—Ê›«  «· Õ„Ì· ⁄·Ï ›« Ê—… «·„‘ —Ì« 
                Transaction_Type = 1;
                AccountType = Common.Masrofat_Load_Payment_Purchases_Account_Type;
                // EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Load_Payment"].ToString());


                Notes = "„’—Ê›«  «· Õ„Ì· ";
                ValueImport = "Bills_List.Load_Payment";
                ValueExport = "0";
                SetSet_Supplier_Balance_With_Bills_ListDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }

        }
        private void Set_Supplier_Balance_WithItems_BillReverse_List(ref int Excp, ref operations opp, long Supplier_Id, long New_Bill_ListTable_id, decimal Net_Total, string Notes)
        {
            string str = "";
            str = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + New_Bill_ListTable_id.ToString() + " and Reference_Type=" + Common.BilleReverse_Type_Id.ToString();
            opp.insert_me(str);
            Net_Total = Common.RoundMe(Net_Total);
            string[] textArray1 = new string[] { "  UPDATE Supplier SET Balance = Balance - ", Net_Total.ToString(), " ,Balance_Actual=Balance_Actual -  ", Net_Total.ToString(), " WHERE (Id = ", Supplier_Id.ToString(), ")" };
            str = string.Concat(textArray1);
            opp.insert_me(str);

            UpdateProcessNo(ref Excp, ref opp, "Bills_List_Reverse", New_Bill_ListTable_id);

            str = (" INSERT INTO Suppliers_Accounts (ProcessNo,Larg_Unit_Price,Item_Id,Item_Name,Notes,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type, Qnty,Larg_Qnty,Small_Qnty, Item_No, Item_Size, Qnty_Size ,Qnty_value,Height_Width_Item) " + " SELECT  Bills_List_Reverse.ProcessNo, Bills_Details_Reverse.Larg_Unit_Price,Bills_Details_Reverse.Item_Id,Item.Item_Name, Bills_Details_Reverse.NameSpecification, Bills_List_Reverse.Supplier_Id, Bills_List_Reverse.Invoice_Id, Bills_List_Reverse.ID, Bills_Details_Reverse.Total, Supplier.Balance, 2 AS Transaction_Type, Bills_List_Reverse.Invoice_Date,   " + Common.BilleReverse_Type_Id.ToString() + " AS Reference_Type, Bills_Details_Reverse.Larg_Qnty,Bills_Details_Reverse.Larg_Qnty,Bills_Details_Reverse.Small_Qnty, Bills_Details_Reverse.Qnty_Another, Item.Height_Width_Str, CONVERT(nvarchar(100), Bills_Details_Reverse.QntyWidth)  + N'' + CONVERT(nvarchar(100), Bills_Details_Reverse.QntyLenght) AS Qnty_Size,  Bills_Details_Reverse.Qnty_value, CONVERT(nvarchar(100), Item.Width_) + N'' + CONVERT(nvarchar(100), Item.Height_) AS Height_Width_Item   ") + " FROM Supplier INNER JOIN  Bills_List_Reverse ON Supplier.Id = Bills_List_Reverse.Supplier_Id INNER JOIN  Bills_Details_Reverse ON Bills_List_Reverse.ID = Bills_Details_Reverse.Invoice_Id INNER JOIN  Item ON Bills_Details_Reverse.Item_Id = Item.id ";
            string[] textArray2 = new string[] { str, " WHERE (Supplier.Id = ", Supplier_Id.ToString(), ") AND (Bills_List_Reverse.Id = ", New_Bill_ListTable_id.ToString(), ") " };
            str = string.Concat(textArray2);
            opp.insert_me(str);
            str = "UPDATE Bills_List_Reverse SET Supplier_BalanceSet = 1,Editable=0 WHERE (Supplier_Id = " + Supplier_Id.ToString() + ") ";
            opp.insert_me(str);


            SetSet_Supplier_Balance_With_Bills_ListReverse(ref opp, New_Bill_ListTable_id);

        }
        void SetSet_Supplier_Balance_With_Bills_ListReverse(ref operations opp, long Bills_List_Id)
        {
            int AccountType = 0;
            int Transaction_Type = 0;
            string Notes = " ";
            string ValueImport = "0";
            string ValueExport = "0";
            Account_Entry_Invoice_CLS ac = new Account_Entry_Invoice_CLS();
            int Excp = 0;
            string Msg = "";
            DataTable dt = ac.Get_EntryFor_Bills_Reverse(ref Excp, ref Msg, ref opp, Bills_List_Id, DateTime.Now.Date, DateTime.Now.Date, 0);
            if (dt.Rows.Count == 0)
            {
                return;
            }
            decimal ATS05_Value = 0;// decimal.Parse(dt.Rows[0]["ATS05_Value"].ToString());
            decimal Net_Total_CommissionValue = 0;//decimal.Parse(dt.Rows[0]["Net_Total_CommissionValue"].ToString());
            decimal TaxSales10_Value = 0;// decimal.Parse(dt.Rows[0]["TaxSales10_Value"].ToString());
            decimal DAMGHAT = 0;//decimal.Parse(dt.Rows[0]["Damaghat"].ToString());
            decimal Load_Payment = 0;//decimal.Parse(dt.Rows[0]["Load_Payment"].ToString());

            try
            {
                ATS05_Value = decimal.Parse(dt.Rows[0]["ATS05_Value"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                Net_Total_CommissionValue = decimal.Parse(dt.Rows[0]["Net_Total_CommissionValue"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                TaxSales10_Value = decimal.Parse(dt.Rows[0]["TaxSales10_Value"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                DAMGHAT = decimal.Parse(dt.Rows[0]["Damaghat"].ToString());
            }
            catch (Exception ee)
            {
            }
            try
            {
                Load_Payment = decimal.Parse(dt.Rows[0]["Load_Payment"].ToString());
            }
            catch (Exception ee)
            {
            }



            if (ATS05_Value > 0)
            {
                ////Õ”«» √—»«Õ  Ã«—Ì… Ê’‰«⁄Ì…
                Transaction_Type = 1;
                AccountType = Common.ATS05_Value_Account_Type_ForSales;


                Notes = "√—»«Õ  Ã«—Ì… Ê’‰«⁄Ì…";
                ValueImport = "Bills_List_Reverse.ATS05_Value";
                ValueExport = "0";
                Set_Supplier_Balance_With_Bills_List_ReverseDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (Net_Total_CommissionValue > 0)
            {

                ////«·Œ’„ «·„”„ÊÕ Ì…
                Transaction_Type = 1;
                AccountType = Common.CommissionValue_Account_Type;

                //EntryValue = EntryValue * Factor;

                Notes = "«·Œ’„ «·„”„ÊÕ Ì…";
                ValueImport = "Bills_List_Reverse.Net_Total_CommissionValue";
                ValueExport = "0";
                Set_Supplier_Balance_With_Bills_List_ReverseDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (TaxSales10_Value > 0)
            {


                ////÷—«∆» «·„»Ì⁄« 
                Transaction_Type = 2;
                AccountType = Common.TaxSales10_Account_Type_ForSales;


                Notes = "÷—«∆» «·„»Ì⁄« ";
                ValueImport = "0";
                ValueExport = "Bills_List_Reverse.TaxSales10_Value";
                Set_Supplier_Balance_With_Bills_List_ReverseDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (DAMGHAT > 0)
            {

                ////«·œ„€…
                Transaction_Type = 2;
                AccountType = Common.DAMGHAT_Account_Type;

                //EntryValue = EntryValue * Factor;
                Notes = "«·œ„€…  ";
                ValueImport = "0";
                ValueExport = "Bills_List_Reverse.Damaghat";
                Set_Supplier_Balance_With_Bills_List_ReverseDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }
            if (Load_Payment > 0)
            {

                ////„’—Ê›«  «· Õ„Ì· ⁄·Ï ›« Ê—… «·„‘ —Ì« 
                Transaction_Type = 2;
                AccountType = Common.Masrofat_Load_Payment_Purchases_Account_Type;
                // EntryValue = decimal.Parse(dt_ProcessName.Rows[x]["Load_Payment"].ToString());


                Notes = "„’—Ê›«  «· Õ„Ì· ";
                ValueImport = "0";
                ValueExport = "Bills_List_Reverse.Load_Payment";
                Set_Supplier_Balance_With_Bills_List_ReverseDataExport_(ref opp, Bills_List_Id, Notes, ValueImport, ValueExport, Transaction_Type, AccountType);
            }

        }

        void SetSet_Supplier_Balance_With_Bills_ListDataExport_(ref operations opp, long New_Bill_ListTable_id, string Notes, string ValueImport, string ValueExport, int Transaction_Type, int AccountType)
        {
            //Notes = dt.Rows[0]["Notes"].ToString();
            //string ProcessNo= dt.Rows[0]["ProcessNo"].ToString();
            //string Supplier_Id= dt.Rows[0]["Supplier_Id"].ToString();
            string str = "";
            str = @" INSERT INTO Suppliers_Accounts (ProcessNo, Notes, Supplier_Id, Reference_Id, Reference_Type_Table_Id,Import, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type)  ";
            str = str + @" SELECT Bills_List.ProcessNo, '" + Notes + "' AS Notes, Bills_List.Supplier_Id, Bills_List.Invoice_Id, Bills_List.ID," + ValueImport + ",  " + ValueExport + ", Supplier.Balance,  " + Transaction_Type + " AS Transaction_Type, Bills_List.Invoice_Date, " + Common.Bill_Type_Id.ToString() + " AS Reference_Type ";
            str = str + @" FROM Supplier INNER JOIN Bills_List ON Supplier.Id = Bills_List.Supplier_Id ";
            str = str + @" WHERE  (Bills_List.Id = " + New_Bill_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
        }


        void Set_Supplier_Balance_With_Bills_List_ReverseDataExport_(ref operations opp, long New_Bill_ListTable_id, string Notes, string ValueImport, string ValueExport, int Transaction_Type, int AccountType)
        {
            //Notes = dt.Rows[0]["Notes"].ToString();
            //string ProcessNo= dt.Rows[0]["ProcessNo"].ToString();
            //string Supplier_Id= dt.Rows[0]["Supplier_Id"].ToString();
            string str = "";
            str = @" INSERT INTO Suppliers_Accounts (ProcessNo, Notes, Supplier_Id, Reference_Id, Reference_Type_Table_Id,Import, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type)  ";
            str = str + @" SELECT Bills_List_Reverse.ProcessNo, '" + Notes + "' AS Notes, Bills_List_Reverse.Supplier_Id, Bills_List_Reverse.Invoice_Id, Bills_List_Reverse.ID," + ValueImport + ",  " + ValueExport + ", Supplier.Balance,  " + Transaction_Type + " AS Transaction_Type, Bills_List_Reverse.Invoice_Date, " + Common.BilleReverse_Type_Id.ToString() + " AS Reference_Type ";
            str = str + @" FROM Supplier INNER JOIN Bills_List_Reverse ON Supplier.Id = Bills_List_Reverse.Supplier_Id ";
            str = str + @" WHERE  (Bills_List_Reverse.Id = " + New_Bill_ListTable_id.ToString() + ") ";

            opp.insert_me(str);
        }


        public void Update_Supplier_BalanceWithEditBill(ref int Excp, ref operations opp, DateTime Invoice_Date, long Supplier_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            Set_Supplier_Balance_MakeBill(ref Excp, ref opp, Supplier_Id, New_Invoice_ListTable_id, Net_Total, Notes);


        }
        public void Set_Supplier_Balance_MakeBill_Reverse(ref int Excp, ref operations opp, long Supplier_Id, long New_Invoice_ListTable_id, decimal Net_Total, string Notes)
        {
            if (Common.IsHavePermissionSysMenu(Common.User_Id, 345, Common.Project_ID, 0) | (Common.Is_Set_Supplier_Balance_With_Items == 1))
            {
                this.Set_Supplier_Balance_WithItems_BillReverse_List(ref Excp, ref opp, Supplier_Id, New_Invoice_ListTable_id, Net_Total, Notes);
            }
            else
            {
                this.Set_SuppliersAccount_BalanceReverse(ref Excp, ref opp, Supplier_Id, New_Invoice_ListTable_id, Net_Total, Notes);
            }
        }


        public void Set_Supplier_Balance_WithMoney_TransactionSarf(ref int Excp, ref operations opp, long Supplier_Id, int Transaction_Type, long Order_Mony_Id, long Money_Transaction_BoxId, decimal Cash_Value, string Notes)
        {
            int Reference_Type = 4;
            long Reference_Type_Table_Id = Money_Transaction_BoxId;
            string mysql = "";

            mysql = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
            opp.insert_me(mysql);
            //if (Common.Balance_BeginingPeriod == 0)
            //{
            mysql = " UPDATE Supplier SET Balance = Balance -  " + Cash_Value + "   WHERE (Id = " + Supplier_Id + ")";
            // arr.Add(mysql);
            opp.insert_me(mysql);
            //}
            UpdateProcessNo(ref Excp, ref opp, "Money_Transaction_Box", Money_Transaction_BoxId);

            mysql = " INSERT INTO Suppliers_Accounts  (ProcessNo,Notes,Reference_Id, Reference_Type_Table_Id, Balance, Supplier_Id, Export,Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
            mysql = mysql + " SELECT Money_Transaction_Box.ProcessNo, N'" + Notes + "',Order_Money.Order_No, Money_Transaction_Box.Id, Supplier.Balance, Money_Transaction_Box.Supplier_Id,  Money_Transaction_Box.Transaction_Value,4 AS Reference_Type , " + Transaction_Type + " AS Transaction_Type, Money_Transaction_Box.Transaction_Date ,1 AS Payment_Type";
            mysql = mysql + " FROM Money_Transaction_Box INNER JOIN  Supplier ON Money_Transaction_Box.Supplier_Id = Supplier.Id ";
            mysql = mysql + "  INNER JOIN Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id ";
            mysql = mysql + " WHERE (Money_Transaction_Box.Order_Mony_Id = " + Order_Mony_Id + ") AND (Money_Transaction_Box.Supplier_Id = " + Supplier_Id + ")  AND (Money_Transaction_Box.id=" + Money_Transaction_BoxId + ")";
            //   arr.Add(mysql);
            opp.insert_me(mysql);

        }
        public void Set_Supplier_Balance_WithMoney_TransactionEstlam(ref int Excp, ref operations opp, long Supplier_Id, int Transaction_Type, long Order_Mony_Id, long Money_Transaction_BoxId, decimal Cash_Value, string Notes)
        {
            int Reference_Type = 3;
            long Reference_Type_Table_Id = Money_Transaction_BoxId;
            string mysql = "";

            mysql = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
            opp.insert_me(mysql);
            //if (Common.Balance_BeginingPeriod == 0)
            //{
            mysql = " UPDATE Supplier SET Balance = Balance +   " + Cash_Value + "   WHERE (Id = " + Supplier_Id + ")";
            //  arr.Add(mysql);
            opp.insert_me(mysql);
            //}

            UpdateProcessNo(ref Excp, ref opp, "Money_Transaction_Box", Money_Transaction_BoxId);

            mysql = " INSERT INTO Suppliers_Accounts  (ProcessNo,Notes,Reference_Id, Reference_Type_Table_Id, Balance, Supplier_Id, Import,Reference_Type, Transaction_Type, Transaction_Date,Payment_Type) ";
            mysql = mysql + " SELECT  Money_Transaction_Box.ProcessNo,N'" + Notes + "',Order_Money.Order_No, Money_Transaction_Box.Id, Supplier.Balance, Money_Transaction_Box.Supplier_Id,  Money_Transaction_Box.Transaction_Value,3 AS Reference_Type , " + Transaction_Type + " AS Transaction_Type, Money_Transaction_Box.Transaction_Date ,1 AS Payment_Type";
            mysql = mysql + " FROM Money_Transaction_Box INNER JOIN  Supplier ON Money_Transaction_Box.Supplier_Id = Supplier.Id ";
            mysql = mysql + "  INNER JOIN Order_Money ON Money_Transaction_Box.Order_Mony_Id = Order_Money.Id ";
            mysql = mysql + " WHERE (Money_Transaction_Box.Order_Mony_Id = " + Order_Mony_Id + ") AND (Money_Transaction_Box.Supplier_Id = " + Supplier_Id + ")  AND (Money_Transaction_Box.id=" + Money_Transaction_BoxId + ")";
            //  arr.Add(mysql);
            opp.insert_me(mysql);




        }
        public void Set_Supplier_Balance_WithMoney_Transaction(ref int Excp, ref operations opp, long Supplier_Id, int Transaction_Type, long Order_Mony_Id, long Money_Transaction_BoxId, decimal Cash_Value, string Notes)
        {
            if (Transaction_Type == 1)
            {
                Set_Supplier_Balance_WithMoney_TransactionEstlam(ref Excp, ref opp, Supplier_Id, Transaction_Type, Order_Mony_Id, Money_Transaction_BoxId, Cash_Value, Notes);
            }
            if (Transaction_Type == 2)
            {
                Set_Supplier_Balance_WithMoney_TransactionSarf(ref Excp, ref opp, Supplier_Id, Transaction_Type, Order_Mony_Id, Money_Transaction_BoxId, Cash_Value, Notes);

            }
        }



        public void Set_Supplier_Balance_With_CheckMoney_Sarf(ref int Excp, ref operations opp, long Supplier_Id, int Transaction_Type, long Order_Mony_Id, long Checks_MemoryId, decimal Check_Value, string Notes, DateTime ProcessDate, bool IsDeletePrevOperation = false)
        {
            int Reference_Type = 6;
            long Reference_Type_Table_Id = Checks_MemoryId;
            string mysql = "";

            if (IsDeletePrevOperation == true)
            {
                mysql = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
                opp.insert_me(mysql);
            }
            //if (Common.Balance_BeginingPeriod == 0)
            //{
            mysql = "  UPDATE Supplier SET Balance = Balance -  " + Check_Value + "   WHERE (Id = " + Supplier_Id + ")";
            opp.insert_me(mysql);
            //}

            UpdateProcessNo(ref Excp, ref opp, "Checks_Memory", Checks_MemoryId);

            mysql = " INSERT INTO Suppliers_Accounts  (ProcessNo,Reference_Id, Reference_Type_Table_Id, Export, Reference_Type, Balance, Supplier_Id , Transaction_Type, Transaction_Date,Payment_Type,Flage_View,Notes ) ";
            mysql = mysql + " SELECT Checks_Memory.ProcessNo,Order_Money_Check.Order_No, Checks_Memory.id, Checks_Memory.Check_Value, 6 AS Reference_Type, 0,   " + Supplier_Id + ", " + Transaction_Type + " AS Transaction_Type, '" + operations.dateFormat(ProcessDate) + "',2 AS Payment_Type ," + Common.Flage_AccountDetails_View + " AS Flage_View ,N'" + Notes + "' ";
            mysql = mysql + @" FROM            Checks_Memory INNER JOIN
                         Supplier ON Checks_Memory.SubAccount_Id_RelatedFrom = Supplier.SubAccount_Id INNER JOIN
                         Order_Money_Check ON Checks_Memory.Order_Mony_Id = Order_Money_Check.Id ";
            mysql = mysql + " WHERE (Checks_Memory.Order_Mony_Id = " + Order_Mony_Id + ")     AND (Checks_Memory.id=" + Checks_MemoryId + ")  AND (Supplier.Id = " + Supplier_Id + ")";
            //AND ( Supplier.Id = " + Supplier_Id + ")
            opp.insert_me(mysql);

        }
        public void Set_Supplier_Balance_With_CheckMoney_Estlam(ref int Excp, ref operations opp, long Supplier_Id, int Transaction_Type, long Order_Mony_Id, long Checks_MemoryId, decimal Check_Value, string Notes, DateTime ProcessDate, bool IsDeletePrevOperation = false)
        {
            int Reference_Type = 5;
            long Reference_Type_Table_Id = Checks_MemoryId;
            string mysql = "";

            if (IsDeletePrevOperation == true)
            {
                mysql = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
                opp.insert_me(mysql);
            }
            /////·Ì” —’Ìœ √Ê· «·„œ…
            //if (Common.Balance_BeginingPeriod == 0)
            //{
            mysql = "  UPDATE Supplier SET Balance = Balance +  " + Check_Value + "   WHERE (Id = " + Supplier_Id + ")";
            opp.insert_me(mysql);
            //}

            UpdateProcessNo(ref Excp, ref opp, "Checks_Memory", Checks_MemoryId);

            mysql = " INSERT INTO Suppliers_Accounts  (ProcessNo,Reference_Id, Reference_Type_Table_Id, Import, Reference_Type, Balance,Supplier_Id , Transaction_Type, Transaction_Date,Payment_Type,Flage_View,Notes ) ";
            mysql = mysql + " SELECT Checks_Memory.ProcessNo,Order_Money_Check.Order_No, Checks_Memory.id, Checks_Memory.Check_Value, 5 AS Reference_Type, Supplier.Balance,  " + Supplier_Id + " , " + Transaction_Type + " AS Transaction_Type, '" + operations.dateFormat(ProcessDate) + "',2 AS Payment_Type ," + Common.Flage_AccountDetails_View + " AS Flage_View ,N'" + Notes + "'";
            mysql = mysql + @" FROM            Checks_Memory INNER JOIN
                         Supplier ON Checks_Memory.SubAccount_Id_RelatedTo = Supplier.SubAccount_Id INNER JOIN
                         Order_Money_Check ON Checks_Memory.Order_Mony_Id = Order_Money_Check.Id ";
            mysql = mysql + " WHERE (Checks_Memory.Order_Mony_Id = " + Order_Mony_Id + ")    AND (Checks_Memory.id=" + Checks_MemoryId + ") AND (Supplier.Id = " + Supplier_Id + ")";
            //AND ( Supplier.Id = " + Supplier_Id + ") 
            opp.insert_me(mysql);




        }
        public void Set_Supplier_Balance_With_CheckMoney(ref int Excp, ref operations opp, long Supplier_Id, int Transaction_Type, long Order_Mony_Id, long Checks_MemoryId, decimal Check_Value, string Notes, DateTime ProcessDate, bool IsDeletePrevOperation = false)
        {
            if (Transaction_Type == 1)
            {
                Set_Supplier_Balance_With_CheckMoney_Estlam(ref Excp, ref opp, Supplier_Id, Transaction_Type, Order_Mony_Id, Checks_MemoryId, Check_Value, Notes, ProcessDate, IsDeletePrevOperation);
            }
            if (Transaction_Type == 2)
            {
                Set_Supplier_Balance_With_CheckMoney_Sarf(ref Excp, ref opp, Supplier_Id, Transaction_Type, Order_Mony_Id, Checks_MemoryId, Check_Value, Notes, ProcessDate, IsDeletePrevOperation);

            }
        }
        public void Set_Supplier_Balance_WithEntry_List(ref int Excp, ref operations opp, long SubAccount_Id, long Account_Entry_List_ID, long Account_Entry_Details_Id)
        {

            string str = "";
            str = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Account_Entry_List_ID.ToString() + " and Reference_Type_TableDetails_Id =" + Account_Entry_Details_Id + " AND Reference_Type=" + Common.Entry_Type_Id.ToString();
            opp.insert_me(str);

            //Larg_Unit_Price,Item_Name,
            //Invoices_Details.Larg_Unit_Price,Item.Item_Name,
            UpdateProcessNo(ref Excp, ref opp, "Money_Transaction_Box", Account_Entry_List_ID);


            str = @"INSERT INTO Suppliers_Accounts(ProcessNo,Larg_Unit_Price, Item_Id, Item_Name, Notes, Supplier_Id, Reference_Id, Reference_Type_Table_Id, Reference_Type_TableDetails_Id,Export, Import, Balance, Transaction_Type, Transaction_Date, Reference_Type, Larg_Qnty, Small_Qnty, Item_No, Item_Size,Qnty_Size, Qnty_value, Height_Width_Item) ";
            str = str + " SELECT   Distinct  Account_Entry_List.ProcessNo,   0 AS Larg_Unit_Price, 0 AS Item_Id, 0 AS Item_Name, Account_Entry_Details.Notes_Details, Supplier.Id AS Supplier_Id, Account_Entry_List.Entry_No, Account_Entry_Details.Account_Entry_List_ID, Account_Entry_Details.Id,  Account_Entry_Details.Equivalent_Import_Value, Account_Entry_Details.Equivalent_Export_Value, Supplier.Balance, Account_Entry_Details.Transaction_Type, Account_Entry_List.Entry_Date, " + Common.Entry_Type_Id.ToString() + " AS Reference_Type, '-' AS Larg_Qnty, '-' AS Small_Qnty, '-' AS Item_No, '-' AS Item_Size, '-' AS Qnty_Size, '-' AS Qnty_value, '-' AS Height_Width_Item ";
            str = str + " FROM Supplier INNER JOIN Account_Entry_Details ON Supplier.SubAccount_Id = Account_Entry_Details.Submain_Account_ID INNER JOIN Account_Entry_List ON Account_Entry_Details.Account_Entry_List_ID = Account_Entry_List.Id  ";
            str = str + " WHERE (Supplier.SubAccount_Id = " + SubAccount_Id.ToString() + ") AND (Account_Entry_List.id = " + Account_Entry_List_ID.ToString() + ") AND (Account_Entry_Details.id = " + Account_Entry_Details_Id.ToString() + ")";

            opp.insert_me(str);

        }
        public void Delete_Supplier_Balance_WithEntry_List(ref int Excp, ref operations opp, long Account_Entry_List_ID)
        {

            string str = "";
            str = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Account_Entry_List_ID.ToString() + "     AND Reference_Type=" + Common.Entry_Type_Id.ToString();
            opp.insert_me(str);
        }
        public void Delete_Supplier_Balance_WithEntry_Details(ref int Excp, ref operations opp, long Account_Entry_List_ID, long Account_Entry_DetailsId)
        {

            string str = "";
            str = "  Delete from  Suppliers_Accounts Where Reference_Type_TableDetails_Id=" + Account_Entry_DetailsId + " AND  Reference_Type_Table_Id=" + Account_Entry_List_ID.ToString() + "     AND Reference_Type=" + Common.Entry_Type_Id.ToString();
            opp.insert_me(str);
        }

        public void Delete_Supplier_Balance(ref int Excp, ref operations opp, long Reference_Type_Table_Id, int Reference_Type)
        {

            string str = "";
            str = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + "     AND Reference_Type=" + Reference_Type.ToString();
            opp.insert_me(str);
        }
        public int Delete_Suppliers_Accounts_OneRow(ref int Excp, ref string Msg, long Customer_Id, DateTime Transaction_Date, string Reference_Type, string Reference_Type_Table_Id, string Transaction_Type)
        {
            int c = 0;
            try
            {

                operations opp = new operations();

                CustomersAccountCLS ac = new CustomersAccountCLS();
                string mysql = "Delete from Suppliers_Accounts Where Supplier_Id= " + Customer_Id + " AND Transaction_Date='" + operations.dateFormat(Transaction_Date) + "' AND   Reference_Type=" + Reference_Type + " And Reference_Type_Table_Id=" + Reference_Type_Table_Id + " AND Transaction_Type=" + Transaction_Type + "";
                c = opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {

                Msg = ee.Message;
                Excp = 1;

            }
            return c;

        }
        public int Delete_Suppliers_Accounts_ProcessNo(ref int Excp, ref string Msg, long ProcessNo)
        {
            operations opp = new operations();
            return Delete_Suppliers_Accounts_ProcessNo(ref Excp, ref Msg, ref opp, ProcessNo);
        }
        public int Delete_Suppliers_Accounts_ProcessNo(ref int Excp, ref string Msg, ref operations opp, long ProcessNo)
        {
            int c = 0;
            CustomersAccountCLS ac = new CustomersAccountCLS();
            string mysql = "Delete from Suppliers_Accounts Where ProcessNo= " + ProcessNo + "  ";
            c = opp.insert_me(mysql);
            Excp = 0;

            return c;

        }

        public void Set_Supplier_Balance_Make_Destruction(ref int Excp, ref operations opp, long Supplier_Id, long New_Suppliers_Destruction_id, decimal Net_Total, string Notes)
        {
            this.Set_SuppliersAccount_BalanceDestruction(ref Excp, ref opp, Supplier_Id, New_Suppliers_Destruction_id, Net_Total, Notes);
        }

        public void Set_SuppliersAccount_BalanceDestruction(ref int Excp, ref operations opp, long Supplier_Id, long Suppliers_Destruction_Id, decimal Net_Total, string Notes)
        {

            Net_Total = MyRound(Net_Total);

            int Reference_Type = Common.Bill_Suppliers_Destruction_Id;
            long Reference_Type_Table_Id = Suppliers_Destruction_Id;
            string mysql = "";

            mysql = "  Delete from  Suppliers_Accounts Where Reference_Type_Table_Id=" + Reference_Type_Table_Id.ToString() + " and Reference_Type=" + Reference_Type;
            opp.insert_me(mysql);


            mysql = "  UPDATE Supplier SET Balance = Balance - " + Net_Total + " ,Balance_Actual=Balance_Actual -  " + Net_Total + " WHERE (Id = " + Supplier_Id + ")";
            opp.insert_me(mysql);

            UpdateProcessNo(ref Excp, ref opp, "Suppliers_Destruction", Suppliers_Destruction_Id);


            // mysql = " INSERT INTO Suppliers_Accounts(ProcessNo, Larg_Unit_Price, Item_Id, Item_Name, Notes, Supplier_Id, Reference_Id, Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date, Reference_Type, Qnty, Larg_Qnty, Small_Qnty, Item_No, Item_Size, Qnty_Size, Qnty_value, Height_Width_Item)
            mysql = " INSERT INTO Suppliers_Accounts(ProcessNo,Larg_Unit_Price,Notes,Supplier_Id, Reference_Id,Reference_Type_Table_Id, Export, Balance, Transaction_Type, Transaction_Date,Reference_Type,Qnty, Larg_Qnty) ";
            mysql = mysql + "  SELECT Suppliers_Destruction.ProcessNo,Suppliers_Destruction.Price, N'" + Notes + "' AS Expr1, Suppliers_Destruction.supplier_id, Suppliers_Destruction.ProcessNo AS Expr2, Suppliers_Destruction.id, Suppliers_Destruction.total, Supplier.Balance,  2 AS Transaction_Type, Suppliers_Destruction.dt_date, " + Common.Bill_Suppliers_Destruction_Id + " AS Reference_Type   ,Suppliers_Destruction.quantity,Suppliers_Destruction.quantity";
            mysql = mysql + " FROM Supplier INNER JOIN Suppliers_Destruction ON Supplier.Id = Suppliers_Destruction.supplier_id ";
            mysql = mysql + " WHERE (Supplier.Id = " + Supplier_Id + ") AND (Suppliers_Destruction.Id = " + Suppliers_Destruction_Id + ") ";
            opp.insert_me(mysql);

            //mysql = "UPDATE Suppliers_Destruction_Id SET Supplier_BalanceSet = 1,Editable=0 WHERE (Supplier_Id = " + Supplier_Id + ") ";
            //opp.insert_me(mysql);
        }
        public void SetSupplierFirstBalance(ref int Excp, ref operations opp, bool IsFirst, long Supplier_Id, decimal Balance, int Transform_Type, DateTime datBalance, string Notes)
        {
            long Customers_Accounts_Id = 0;
            Suppliers_AccountsCls cc = new Suppliers_AccountsCls();
            DateTime datNow = DateTime.Now.Date;
            DataTable dt = cc.GetSupplierFirstBalance(Supplier_Id);
            if (dt.Rows.Count > 0)
            {
                Customers_Accounts_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                datNow = DateTime.Parse(dt.Rows[0]["Transaction_Date"].ToString());
            }
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(datBalance);
            string mysql = "";
            decimal BalanceSaved = Balance;


            decimal Import = 0;
            decimal Export = 0;

            if (Transform_Type == 1)
            {
                Balance = Balance * -1;

                Import = 0;
                Export = BalanceSaved;

            }


            else
            {
                Import = BalanceSaved;
                Export = 0;
            }



            if (IsFirst == false)
            {
                Excp = 10;
                return;
            }

            if (Customers_Accounts_Id > 0)
            {
                mysql = " Update Suppliers_Accounts Set Transaction_Date='" + operations.dateFormat(datBalance) + "' , Notes=N'" + Notes + "',Balance = " + Balance + ",Export=" + Export + ",Import=" + Import + "  Where Id=" + Customers_Accounts_Id;
                opp.insert_me(mysql);
            }
            else
            {
                mysql = " INSERT INTO Suppliers_Accounts (Supplier_Id, Reference_Id,Reference_Type_Table_Id, Export,Import, Balance, Transaction_Type, Transaction_Date,Reference_Type,Notes) ";
                mysql += " VALUES(" + Supplier_Id + ",0," + Supplier_Id + "," + Export + "," + Import + "," + Balance + ",2,'" + operations.dateFormat(datBalance) + "',-7,N'" + Notes + "')";
                opp.insert_me(mysql);
            }
        }
    }
}
// else if (Reference_Type == "5")
//{
//    Reference_Type_View = "≈” ·«„ ‘Ìﬂ";
//}
//else if (Reference_Type == "6")
//{
//    Reference_Type_View = "’—› ‘Ìﬂ";
//}
//else if (Reference_Type == "35")
//{
//    Reference_Type_View = " ⁄œÌ· ≈” ·«„ ‘Ìﬂ";
//}
//else if (Reference_Type == "36")
//{
//    Reference_Type_View = " ⁄œÌ· ’—› ‘Ìﬂ";
//}
//else if (Reference_Type == "37")
//{
//    Reference_Type_View = "≈” ·«„ ‘Ìﬂ „Õ–Ê›";
//}
//else if (Reference_Type == "38")
//{
//    Reference_Type_View = "’—› ‘Ìﬂ „Õ–Ê›";
//}