using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;

using AccountModelsLib.Models;
 
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace AccountModelsLib.Models
{
    /// <summary>
    /// Summary description for BanksClss.
    /// </summary>
    public class BanksClss
    {
        public Bank bank = new Bank();
        public List<Bank> bnkList = new List<Bank>();
        AccountModelsLib.operations opp;
        DataSet G_DS;
        public BanksClss(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
            G_DS = new DataSet();
            // TODO: Add constructor logic here
            //
        }
        public BanksClss()
        {
            opp = new AccountModelsLib.operations();
            G_DS = new DataSet();
            // TODO: Add constructor logic here
            //
        }

        //public List<BankCLS> GetBanks()
        //{
        //    bank
        //}
        // int Account_Type_Value, long  Branch_Id, string Branch_Name, string Bank_Name, decimal Primary_Balance, string Account_No, int Is_Bank, long Currency_Id, DateTime dt, int Supplier_Bank, long New_Submain_Account_Id, string Account_Code
        public List<BanksListViewModel> GetBanksListWithValues(int Is_Bank, long Branch_Id)
        {
            List<BanksListViewModel> banksListViewModels = new List<BanksListViewModel>();

            List<Bank> bnkList = new List<Bank>();
            bnkList = opp.db.Banks.Where(i => i.Is_Bank == Is_Bank & i.Active == 1 & i.Branch_Id == Branch_Id).OrderByDescending(i => i.Default_Bank).ToList();

            foreach (var bnk in bnkList)
            {
                BanksListViewModel vm = new BanksListViewModel();
                vm.Id = 0;

                vm.OrderNo = 0;
                vm.BankName = bnk.Bank_Name;
                vm.BankId = bnk.Id;
                vm.Id = bnk.Id;
                vm.Cach_Value = 0;
                vm.Notes = "";
                vm.SubAccount_Id = bnk.SubAccount_Id;
                vm.Currency_Id = bnk.Currency_Id ?? 1;
                vm.Default_Bank = bnk.Default_Bank;

                banksListViewModels.Add(vm);

            }
            return banksListViewModels;
        }
        public Money_Transaction_Box GetBanksWithNewValues(int Is_Bank, long Box_Id)
        {


            Bank bnk = new Bank();
            bnk = opp.db.Banks.Where(i => i.Is_Bank == Is_Bank & i.Active == 1 & i.Id == Box_Id).FirstOrDefault();



            Money_Transaction_Box mm = new Money_Transaction_Box();
            try
            {

                mm.Order_No = 0;
                mm.Bank_Name = bnk.Bank_Name;
                mm.Bank_Id = bnk.Id;

                mm.Transaction_Value = 0;
                mm.Notes = "";
                mm.SubAccount_Id = 0;
                mm.BankSubAccount_Id = bnk.SubAccount_Id;
                mm.Currency_Id = bnk.Currency_Id ?? 1;
                mm.Entry_Currency_ID = Common.Entry_Currency_Id;
                mm.IsDefault_Bank = bnk.Default_Bank;
                mm.Factor = GetCurrencyFactor(mm.Currency_Id ?? 0);


            }
            catch (Exception ee)
            {

            }


            return mm;
        }
        public List<Money_Transaction_Box> GetBanksListWithNewValues(int Is_Bank, long Branch_Id)
        {
            List<Money_Transaction_Box> mbx = new List<Money_Transaction_Box>();

            List<Bank> bnkList = new List<Bank>();
            bnkList = opp.db.Banks.Where(i => i.Is_Bank == Is_Bank & i.Active == 1 & i.Branch_Id == Branch_Id).OrderByDescending(i => i.Default_Bank).ToList();

            foreach (var bnk in bnkList)
            {

                Money_Transaction_Box mm = new Money_Transaction_Box();

                mm.Order_No = 0;
                mm.Bank_Name = bnk.Bank_Name;
                mm.Bank_Id = bnk.Id;

                mm.Transaction_Value = 0;
                mm.Notes = "";
                mm.SubAccount_Id = 0;
                mm.BankSubAccount_Id = bnk.SubAccount_Id;
                mm.Currency_Id = bnk.Currency_Id ?? 1;
                mm.Entry_Currency_ID = Common.Entry_Currency_Id;
                mm.IsDefault_Bank = bnk.Default_Bank;
                mm.Factor = GetCurrencyFactor(mm.Currency_Id??0);


                mbx.Add(mm);
            }
            return mbx;
        }
        decimal GetCurrencyFactor(long Entry_Currency_Id)
        {
            decimal Factor = 1;
            if (Entry_Currency_Id == Common.Entry_Currency_Id)
            {
                Factor = 1;
            }
            else
            {
                CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                  Factor = cc.GetFactors(Common.Entry_Currency_Id, Entry_Currency_Id);
                Factor = Common.RoundMe(Factor);
            }
            return Factor;
        }
        public void GetMake_New_Order_No(ref int Excp, ref long Order_No, int Transaction_Type, DateTime dtOrderDate, List<BanksListViewModel> banksListViewModels)
        {
            try
            {
                long Order_NoSub = 0;
                Money_Transaction_BoxOrderReportCLS ot = new Money_Transaction_BoxOrderReportCLS();
                Excp = 0;
                BanksListViewModel defaultBank = banksListViewModels.Where(d => d.Default_Bank == 1).FirstOrDefault();
                if (defaultBank != null)
                {
                    defaultBank.OrderNo = ot.Make_New_Order_No(ref Excp, Transaction_Type, dtOrderDate, (int)defaultBank.BankId);
                    Order_No = defaultBank.OrderNo;
                    Order_NoSub = defaultBank.OrderNo;
                }
                else
                {
                    if (banksListViewModels.Count > 0)
                    {
                        SetDefaultBankForInvoice(ref Excp, banksListViewModels[0].BankId, banksListViewModels[0].Currency_Id ?? 1);
                    }
                }

                foreach (var item in banksListViewModels)
                {
                    if (item.Id == defaultBank.Id)
                    {
                        continue;
                    }
                    else
                    {
                        Order_NoSub = Order_NoSub + 1;
                        item.OrderNo = Order_NoSub;
                    }

                }
            }
            catch (Exception rr)
            {


            }
        }
        public void GetMake_New_Order_No(ref int Excp, ref long Order_No, int Transaction_Type, DateTime dtOrderDate, List<Money_Transaction_Box> mbxLst)
        {
            try
            {
                long Order_NoSub = 0;
                Money_Transaction_BoxOrderReportCLS ot = new Money_Transaction_BoxOrderReportCLS();
                Excp = 0;
                Money_Transaction_Box mbx = mbxLst.Where(d => d.IsDefault_Bank == 1).FirstOrDefault();
                if (mbx != null)
                {
                    mbx.Order_No = ot.Make_New_Order_No(ref Excp, Transaction_Type, dtOrderDate, mbx.Bank_Id);
                    Order_No = mbx.Order_No??1;
                    Order_NoSub = mbx.Order_No??1;
                }
                else
                {
                    if (mbxLst.Count > 0)
                    {
                        SetDefaultBankForInvoice(ref Excp, mbxLst[0].Bank_Id, mbxLst[0].Currency_Id ?? 1);
                    }
                }

                foreach (var item in mbxLst)
                {
                    if (item.Bank_Id == mbx.Bank_Id)
                    {
                        continue;
                    }
                    else
                    {
                        Order_NoSub = Order_NoSub + 1;
                        item.Order_No = Order_NoSub;
                    }

                }
            }
            catch (Exception rr)
            {


            }
        }
        public void GetMake_New_Order_No(ref int Excp, ref long Order_No, int Transaction_Type, DateTime dtOrderDate, Money_Transaction_Box mbx)
        {
            try
            {
                long Order_NoSub = 0;
                Money_Transaction_BoxOrderReportCLS ot = new Money_Transaction_BoxOrderReportCLS();
                Excp = 0;

                if (mbx != null)
                {
                    mbx.Order_No = ot.Make_New_Order_No(ref Excp, Transaction_Type, dtOrderDate, mbx.Bank_Id);
                    Order_No = mbx.Order_No ?? 1;
                    Order_NoSub = mbx.Order_No ?? 1;
                }
                else
                {

                    SetDefaultBankForInvoice(ref Excp, mbx.Bank_Id, mbx.Currency_Id ?? 1);

                }



            }
            catch (Exception rr)
            {


            }
        }
        public void GetCurrencyData(List<BanksListViewModel> banksListViewModels)
        {///������
            try
            {
                foreach (var item in banksListViewModels)
                {
                    long Entry_Currency_Id = item.Currency_Id ?? 1;
                    if (Entry_Currency_Id == Common.Entry_Currency_Id)
                    {
                        item.Factor = 1;
                    }
                    else
                    {
                        CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                        decimal Factor = cc.GetFactors(Common.Entry_Currency_Id, Entry_Currency_Id);
                        item.Factor = Common.RoundMe(Factor);


                    }
                }

            }
            catch
            {


            }
        }

        public void GetCurrencyData(List<Money_Transaction_Box> mbxLst)
        { 
            try
            {
                foreach (var item in mbxLst)
                {
                    long Entry_Currency_Id = item.Currency_Id ?? 1;
                    if (Entry_Currency_Id == Common.Entry_Currency_Id)
                    {
                        item.Factor = 1;
                    }
                    else
                    {
                        CurrencyFactorsCLS_ cc = new CurrencyFactorsCLS_();
                        decimal Factor = cc.GetFactors(Common.Entry_Currency_Id, Entry_Currency_Id);
                        item.Factor = Common.RoundMe(Factor);


                    }
                }

            }
            catch
            {


            }
        }
        public void Add_Banks(ref int Excp, ref string Msg, Bank ba, bool Use_AccountSelect)
        {
            //BanksClss bCls = new BanksClss();
            //AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            if (Use_AccountSelect == false)
            {
                ba.SubAccount_Id = -1;
            }

            Excp = 0;
            Msg = "";

            AddNewBank(ref Excp, ref Msg, ba);



        }

        public void Edit_Banks_Balance(ref int Excp, long Bank_Id, string Bank_Name, decimal Primary_Balance, string Account_No)
        {
            try
            {
                string mysql = " UPDATE Banks SET Bank_Name =  '" + Bank_Name + "' id = WHERE (id = " + Bank_Id + ") ";
                AccountModelsLib.operations opp = new AccountModelsLib.operations();

                opp.update_me(mysql, ref Excp);
                opp = null;
                Excp = 1;

            }
            catch (Exception e)
            {
                Excp = -1;
                throw (e);
            }

        }

        public void Edit_Banks_Balance(ref int Excp, long Bank_Id, decimal Last_Balance)
        {
            try
            {
                string mysql = " UPDATE Banks SET Last_Balance =   " + Last_Balance + "    WHERE (id = " + Bank_Id + ") ";
                AccountModelsLib.operations opp = new AccountModelsLib.operations();

                opp.update_me(mysql, ref Excp);
                opp = null;
                Excp = 1;

            }
            catch (Exception e)
            {
                Excp = -1;
                throw (e);
            }

        }
        public void SetDefaultBankForInvoice(ref int Excp, long Bank_Id, long Currency_Id)
        {
            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();

                string mysql = " UPDATE Banks SET Default_Bank = 0 Where Currency_Id=" + Currency_Id;
                opp.insert_me(mysql);

                mysql = " UPDATE Banks SET Default_Bank =  1   WHERE (id = " + Bank_Id + ") AND (Currency_Id = " + Currency_Id + ") ";
                opp.insert_me(mysql);
                //opp = null;
                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);
            }

        }
        public void SetUpdateSandouk(ref int Excp, long Bank_Id, int Is_Default_Bank, long Account_Branches_ID)
        {
            string mysql = "";
            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                if (Is_Default_Bank == 1)
                {
                    mysql = " UPDATE Banks SET Default_Bank = 0 Where Is_Bank=0";
                    opp.insert_me(mysql);
                }
                mysql = " UPDATE Banks SET Default_Bank =  " + Is_Default_Bank + " , Branch_Id=" + Account_Branches_ID + "  WHERE (id = " + Bank_Id + ")   ";
                opp.insert_me(mysql);
                opp = null;
                Excp = 1;

            }
            catch (Exception e)
            {
                Excp = -1;
                throw (e);
            }

        }
        void InsertOrderMoneyNew(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, ref long Order_Mony_Id2, long Order_No, int Transaction_type, DateTime Order_Date, decimal Total_Value, long Currency_Id, int  Bank_Type_, string Man_Name, long Bank_ID, int Order_Mony_Id_Transform, long Invoice_ID, int Invoice_Type, long User_Id, long Branch_Id, long Year_Financial_Period_Id, long  Ticket_Supplier_Id, string Order_Tafkeet_Value, string Order_Notes, long Sandouk_Cost_Center_Tree_Id, int IS_Posted_Flage, decimal Factor, long    Entry_Currency_ID, string Entry_Currency_Name, long CarsWorkOrdersId)
        {

            try
            {

                //long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(Order_Date);

                Order_MoneyCLS om = new Order_MoneyCLS();

                om.om.Order_No = Order_No;
                om.om.Transaction_type = Transaction_type;
                om.om.Order_Date = Order_Date;
                om.om.Total_Value = Total_Value;
                om.om.Currency_Id = Currency_Id;
                om.om.Bank_Type = Bank_Type_;
                om.om.Man_Name = Man_Name;
                om.om.Bank_Id = Bank_ID;
                om.om.Order_Notes = Order_Notes;
                om.om.User_Id = Common.User_Id;
                om.om.Branch_Id = Common.Branch_Id;
                om.om.OperationDate = DateTime.Now.Date;
                om.om.Insert_ServerDateTime = DateTime.Now.Date;
                om.om.Factor = Factor;
                om.om.Year_Financial_Period_Id = Year_Financial_Period_Id;


                om.om.Is_Balances_Set = 0;
                om.om.Day_Registration_flage = 0;
                om.om.DayReg_List_Id = 0;
                om.om.Order_mony_id_transform = Order_Mony_Id_Transform;
                om.om.Invoice_Id = Invoice_ID;
                om.om.Invoice_Type = Invoice_Type;
                om.om.CarsWorkOrdersId = CarsWorkOrdersId;



                om.om.Ticket_Supplier_Id = Ticket_Supplier_Id;
                om.om.Order_Tafkeet_Value = Order_Tafkeet_Value;
                om.om.Sandouk_Cost_Center_Tree_Id = Sandouk_Cost_Center_Tree_Id;
                om.om.IS_Posted_Flage = IS_Posted_Flage;
                om.om.Entry_Currency_ID = Entry_Currency_ID;
                om.om.Entry_Currency_Name = Entry_Currency_Name;


                om.InsertOrder_Money(ref Excp, ref Msg, ref opp, om.om);

                Order_Mony_Id2 = om.om.Id;

            }
            catch (Exception ee)
            {

            }
        }
        public long InsertMoney_Transaction_Box(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Order_Mony_Id, long Order_No, string Bank_Name,DateTime Transaction_Date, int Transaction_Type, long Bank_id, decimal Bank_Balance, decimal Import_Value, decimal Export_Value, decimal Transaction_Value, string Transaction_Value_, long Customer_Id, long Supplier_Id, long Account_Id, int Account_Type, string Notes, int Account_Selection_, decimal Supplier_Customer_Balance_Befor, long SubAccount_Id, string Account_Name, decimal Factor, string Tafkeet_Value, long Invoice_Id, int Invoice_TypeId, long CarsWorkOrdersId,long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {

            Money_Transaction_BoxCLS mt = new Money_Transaction_BoxCLS();

            long    Currency_Id = 1;
            //decimal Import_Value = 0;
            //decimal Export_Value = 0;
            decimal Balance_Before = 0;
            decimal Balance_After = 0;
            int Equivalent_Value = 1;
            long BankAccount_Id = 0;
            int DestructionSales = 0;
            int Order_mony_id_transform = -1;
            long Account_Cost_Center_Tree_Id = 0;
            decimal TotalValueWillPaid = 0;
            decimal Rest_Value = 0;
            string Account_Code = "";
            string Cost_Center = "";
            string CostCenter_Name = "";
            decimal Purchase_Ticket_Value = 0;
            decimal Commission_Value = 0;
            long Entry_Currency_ID = 0;
            string Balance_BeforStr = "";
            string Balance_AfterStr = "";
            return mt.InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id, Order_No, Bank_Name, Transaction_Date, Transaction_Type, Import_Value, Export_Value, Transaction_Value, Transaction_Value_, Equivalent_Value, Bank_id, Bank_Balance, Supplier_Id, Customer_Id,   SubAccount_Id, Notes, Balance_Before, Balance_After, Balance_BeforStr, Balance_AfterStr, BankAccount_Id, DestructionSales, Order_mony_id_transform, Account_Cost_Center_Tree_Id, Tafkeet_Value, TotalValueWillPaid, Rest_Value, Account_Name, Account_Code, Account_Type, Cost_Center, Currency_Id, Purchase_Ticket_Value, Commission_Value, Factor, Entry_Currency_ID, CostCenter_Name, Invoice_Id, Invoice_TypeId, CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId);

        }

        public void Edit_BanksFirst_Balance(ref int Excp, ref string Msg, long Bank_Id,long Order_No,string  Bank_Name, decimal Last_Balance, decimal NewFirseBalance, decimal Primary_BalanceDiff, DateTime dtFirstBalance, long Financial_Period_Begining_Balance_Id, int Bank_Type)
        {
            long Order_Mony_Id = 0;
            long Entry_Currency_ID = 1;
            string Entry_Currency_Name = "";
            int Ticket_Supplier_Id = 0;
            int Sandouk_Cost_Center_Tree_Id = 0;
            int IS_Posted_Flage = 0;
            string Order_Notes = "";
            string Order_Tafkeet_Value = TafkeetProj.NumberToText.GetText(Primary_BalanceDiff, Entry_Currency_ID);
            int Bank_Type_ = 0;
            int Order_Mony_Id_Transform = -1;
            long Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id(DateTime.Now.Date);
            int Factor = 1;
            long Invoice_ID = 0;
            int Invoice_Type = 0;

            long CarsWorkOrdersId = 0;
            long Reference_Type_Table_Id = 0;
             long DataDictionaryProcessesListsId = 0;
            try
            {
                decimal Import_Value = 0;
                decimal Export_Value = 0;
                decimal Transaction_Type = 0;
                if (Primary_BalanceDiff > 0)
                {
                    Import_Value = Primary_BalanceDiff;
                    Export_Value = 0;
                }
                else
                {

                    Import_Value = 0;
                    Export_Value = Primary_BalanceDiff;
                    Export_Value = Export_Value * -1;
                }
                Transaction_Type = Primary_BalanceDiff;
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                ArrayList arr = new ArrayList();
                string mysql = " ";
                //  Last_Balance = Last_Balance + Primary_BalanceDiff;
                if (Financial_Period_Begining_Balance_Id == -1 & Bank_Type == 0)
                {
                    mysql = " UPDATE Banks SET firstBalanceDate ='" + AccountModelsLib.operations.dateFormat(dtFirstBalance) + "',Primary_balance = Primary_balance + " + Primary_BalanceDiff + "  , Last_Balance = Last_Balance + " + Primary_BalanceDiff + "  WHERE (id = " + Bank_Id + ") ";
                    arr.Add(mysql);
                    mysql = " UPDATE Money_Transaction_Box SET Transaction_Date = '" + AccountModelsLib.operations.dateFormat(dtFirstBalance) + "' WHERE (Account_Id = 0) AND (Transaction_Type = 0)    AND (Bank_Id = " + Bank_Id + ")";
                    arr.Add(mysql);

                    mysql = " UPDATE Money_Transaction_Box SET Bank_Balance = Bank_Balance + " + Primary_BalanceDiff + "   WHERE   (Bank_Id = " + Bank_Id + ")";
                    arr.Add(mysql);

                    mysql = " UPDATE Order_Money SET Order_Date ='" + AccountModelsLib.operations.dateFormat(dtFirstBalance) + "', Total_Value =Total_Value + " + Primary_BalanceDiff + " WHERE (Order_No = 0) AND (Transaction_type = 0) AND  (Bank_ID = " + Bank_Id + ") ";
                    arr.Add(mysql);
                }
                else if (Financial_Period_Begining_Balance_Id != -1 & Bank_Type == 0)
                {

                    mysql = " UPDATE Banks SET   firstBalanceDate ='" + AccountModelsLib.operations.dateFormat(dtFirstBalance) + "', Last_Balance = Last_Balance + " + Primary_BalanceDiff + "  WHERE (id = " + Bank_Id + ") ";
                    arr.Add(mysql);

                    mysql = " UPDATE   Financial_Period_Begining_Balance  Set Import_Value=" + NewFirseBalance + " where Id=" + Financial_Period_Begining_Balance_Id;
                    arr.Add(mysql);

                    //mysql = " INSERT INTO Order_Money (Order_No, Transaction_type,Order_Date, Total_Value, Currency_Id, Bank_Type, Man_Name,Bank_ID) ";
                    //mysql = mysql + " VALUES (-1,0,'" + AccountModelsLib.operations.dateFormat(DateTime.Now.Date) + "'," + Primary_BalanceDiff + ",1,0,' '," + Bank_Id + ")";
                    //long Order_Mony_Id = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);


                    InsertOrderMoneyNew(ref Excp, ref Msg, ref opp, ref Order_Mony_Id, -1, 0, DateTime.Now.Date, Primary_BalanceDiff, 1, 0, " ", Bank_Id, Order_Mony_Id_Transform, Invoice_ID, Invoice_Type, Common.User_Id, Common.Branch_Id, Year_Financial_Period_Id, Ticket_Supplier_Id, Order_Tafkeet_Value, Order_Notes, Sandouk_Cost_Center_Tree_Id, IS_Posted_Flage, Factor, Entry_Currency_ID, Entry_Currency_Name, CarsWorkOrdersId);

                    //mysql = " INSERT INTO Money_Transaction_Box (Order_Mony_Id, Transaction_Date, Transaction_Type,Import_Value,Export_Value, Transaction_Value, Transaction_Value_, Bank_Id, Bank_Balance, Supplier_Id, Customer_Id,Account_Id, Notes) ";
                    //mysql = mysql + " VALUES (" + Order_Mony_Id + ",'" + AccountModelsLib.operations.dateFormat(DateTime.Now.Date) + "',0," + Import_Value + "," + Export_Value + ",0,' '," + Bank_Id + "," + Last_Balance + ",0,0,0,' ')";
                    //opp.insert_me(mysql);..

                  


                    InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp, Order_Mony_Id, Order_No, Bank_Name, DateTime.Now.Date, 0, Bank_Id, Last_Balance, Import_Value, Export_Value, 0, "", 0, 0, 0, 0, " ", 0, 0, 0, "", Factor, "", 0, 0,       CarsWorkOrdersId,   Reference_Type_Table_Id,   DataDictionaryProcessesListsId);


                }

                else if (Financial_Period_Begining_Balance_Id == -1 & Bank_Type == 1)
                {
                    mysql = " UPDATE Banks SET firstBalanceDate ='" + AccountModelsLib.operations.dateFormat(dtFirstBalance) + "', Primary_balance = Primary_balance + " + Primary_BalanceDiff + "  , Last_Balance = Last_Balance + " + Primary_BalanceDiff + "  WHERE (id = " + Bank_Id + ") ";
                    arr.Add(mysql);

                    mysql = " INSERT INTO Money_Transaction_Bank  (Bank_Id, Transaction_Date ,  Bank_Balance, Transaction_Type, Reference_Type,Account_ID,Reference_Type_Id,Import_Value,Export_Value) ";
                    mysql = mysql + " VALUES (" + Bank_Id + ",'" + AccountModelsLib.operations.dateFormat(DateTime.Now.Date) + "', " + Last_Balance + ",0,10,0,0," + Import_Value + "," + Export_Value + ") ";
                    arr.Add(mysql);
                }

                else if (Financial_Period_Begining_Balance_Id != -1 & Bank_Type == 1)
                {
                    mysql = " UPDATE Banks SET  firstBalanceDate ='" + AccountModelsLib.operations.dateFormat(dtFirstBalance) + "', Last_Balance = Last_Balance + " + Primary_BalanceDiff + "  WHERE (id = " + Bank_Id + ") ";
                    arr.Add(mysql);

                    mysql = " UPDATE   Financial_Period_Begining_Balance  Set Import_Value=" + NewFirseBalance + " where Id=" + Financial_Period_Begining_Balance_Id;
                    arr.Add(mysql);

                    mysql = " INSERT INTO Money_Transaction_Bank  (Bank_Id, Transaction_Date ,  Bank_Balance, Transaction_Type, Reference_Type,Account_ID,Reference_Type_Id,Import_Value,Export_Value) ";
                    mysql = mysql + " VALUES (" + Bank_Id + ",'" + AccountModelsLib.operations.dateFormat(DateTime.Now.Date) + "', " + Last_Balance + ",0,10,0,0," + Import_Value + "," + Export_Value + ") ";
                    arr.Add(mysql);
                }

                int Status = opp.insert_me_transaction(arr);
                if (Status == 1)
                {
                    Msg="�� ����� ���� ��� �����";
                    Excp = 0;
                }
                else
                {
                    Msg = "��� ��  ����� ���� ��� �����" ;
                    Excp = 1;
                }
                opp = null;


            }
            catch (Exception e)
            {
                Excp = -1;
                throw (e);
            }

        }

        public DataSet Get_Banks(ref int Excp)
        {
            G_DS.Clear();
            try
            {
                string mysql;
                mysql = "Select * from Banks ORDER BY Is_Bank, Supplier_Bank  ";
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                G_DS = opp.return_dataset(mysql, "Banks");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                Excp = -1;
                return G_DS;
            }
        }
        public DataSet Get_Banks(ref int Excp, int Account_Type_Value, bool BankSupplier)
        {
            G_DS.Clear();
            try
            {
                string mysql;
                mysql = "Select * from Banks Where Account_Type_Value= " + Account_Type_Value;
                if (!BankSupplier)
                {
                    mysql = mysql + " AND Supplier_Bank=0 ";
                }

                mysql = mysql + " ORDER BY Is_Bank, Supplier_Bank  ";
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                G_DS = opp.return_dataset(mysql, "Banks");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                Excp = -1;
                return G_DS;
            }
        }

        public DataTable Get_SandoukList()
        {
            DataTable dt = new DataTable();
            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                string mysql;
                mysql = @"SELECT        Account_Branches.Branch_Name, Banks.*
FROM   Banks LEFT OUTER JOIN
                         Account_Branches ON Banks.Branch_Id = Account_Branches.Id 
";


                mysql = mysql + " Where  Is_Bank=0 ";

                dt = opp.return_DataTable(mysql);

                opp = null;
                return dt;
            }
            catch
            {

                return dt;
            }
        }
        public void Delete_Banks(ref int Excp, ref string Msg, long Bank_Id, long SubAccount_Id)
        {
            try
            {
                string mysql = "";
                AccountModelsLib.operations opp = new AccountModelsLib.operations(2);

                DataSet ds = opp.return_dataset("Select * from Order_Money Where Bank_ID=" + Bank_Id);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    Msg="�� ����� ����� ����� ���� ����� ������ ��� ������� ���� ����� \\ �������";
                    Excp = 10;
                    return;
                }

                Sub_Account sa = new Sub_Account();
                sa.Delete_Account(ref Excp, ref Msg, ref opp,Common.Bank_Account_Type, Bank_Id, SubAccount_Id);

                if (Excp == 0)
                {
                    opp.Commit();
                    Msg = "��  ��� �����/������� �����";
                }
                else
                {
                    opp.Rollback();
                    string Msg2 = "��� �� ��� �����/�������";
                    Msg = Msg2 + "\n\r" + Msg2;
                }
            }
            catch (Exception e)
            {
                Excp = 1;
                Msg = e.Message;
            }
        }


        ///////////////////aaaaaaaaaaaaa
        ///
        DataTable dt = new DataTable();
        public int Is_Bank = 0;
        public void Get_Bank(ref AccountModelsLib.operations opp, long Id)
        {
            string mysql = " Select * from Banks where Id=" + Id;
            dt = opp.return_DataTable(mysql);

            bank = new Bank();
            bank = opp.db.Banks.Where(b => b.Id == Id).FirstOrDefault();
            //ReadBankData(dt, 0);


        }
        public DataTable Bank_Data(int Is_Bank_)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return Bank_Data(ref opp, Is_Bank_);
        }
        public DataTable Bank_Data(ref AccountModelsLib.operations opp, int Is_Bank_)
        {
            Is_Bank = Is_Bank_;
            string mysql = "Select * from Banks  where Is_Bank=" + Is_Bank_;

            if (Is_Bank_ == 1)
            {
                mysql = mysql + " AND Supplier_Bank=0";
            }
            dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_AllBanks(int Is_Bank)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = " Select * from Banks  Where Is_Bank=" + Is_Bank;
            dt = opp.return_DataTable(mysql);
            return dt;
        }
        public List<Bank> Get_AllBanks(ref AccountModelsLib.operations opp ,int Is_Bank)
        {
             
            List<Bank> bnks = new List<Bank>();
            bnks = opp.db.Banks.AsNoTracking().Where(b=>b.Is_Bank== Is_Bank & b.Active==1).ToList();
          
            return bnks;
        }

        public void InsertBank(ref int Excp, ref AccountModelsLib.operations opp, Bank bnk)
        {
            if (Common.Use_AutoNumeric_Bank_Id_With_Submain_Account_Id == 1)
            {

                //string mysql = " INSERT INTO Banks (Account_Type_Value,Branch_Name,Account_Branches_ID,Bank_Name, Primary_balance,Last_Balance,Account_No,Is_Bank,SubAccount_Id,currency_Id,firstBalanceDate,Supplier_Bank,Account_Code) ";
                //mysql = mysql + " VALUES (" + Account_Type_Value + ",'" + Branch_Name + "'," + Account_Branches_ID + ",'" + Bank_Name + "'," + Primary_Balance + "," + Primary_Balance + ", '" + Account_No + "'," + Is_Bank + "," + SubAccount_Id + "," + Currency_Id + ",'" + AccountModelsLib.operations.dateFormat(FirstBalanceDate) + "'," + Supplier_Bank + ",'" + Account_Code + "') ";
                //Id = opp.Insert_me_Return_Last_Identify(mysql, ref Excp);
            }
            else
            {
                bnk.Id = bnk.SubAccount_Id;
                //string mysql = " INSERT INTO Banks (Id,Account_Type_Value,Branch_Name,Account_Branches_ID,Bank_Name, Primary_balance,Last_Balance,Account_No,Is_Bank,SubAccount_Id,currency_Id,firstBalanceDate,Supplier_Bank,Account_Code) ";
                //mysql = mysql + " VALUES (" + SubAccount_Id + "," + Account_Type_Value + ",'" + Branch_Name + "'," + Account_Branches_ID + ",'" + Bank_Name + "'," + Primary_Balance + "," + Primary_Balance + ", '" + Account_No + "'," + Is_Bank + "," + SubAccount_Id + "," + Currency_Id + ",'" + AccountModelsLib.operations.dateFormat(FirstBalanceDate) + "'," + Supplier_Bank + ",'" + Account_Code + "') ";
                //opp.insert_me(mysql);
            }
            opp.db.Banks.Add(bnk);
            opp.db.SaveChanges();
        }
        public long  InsertBank(ref int excp, ref string msg, ref operations opp, Bank ba, Submain_Account sa_)
        {
            if (Common.Use_AutoNumeric_Bank_Id_With_Submain_Account_Id == 0 | Common.Use_Auto_Identity_Column == 0)
            {
                ba.Id = sa_.Id;
            }
            ba.Bank_Name = sa_.SubAccount_Name;
            
            ba.SubAccount_Id = sa_.Id;
            ba.Currency_Id = sa_.Currency_Id;
            ba.Primary_balance = 0;
            ba.Last_Balance = 0;
            ba.firstBalanceDate = DateTime.Now.Date;
            ba.Account_Code = sa_.SubAccount_NO;
            ba.Branch_Id = sa_.Branch_Id;
            ba.Account_Type_Value = sa_.Account_Customer_Type_Value;

            ba.Active = 1; 
             
            opp.db.Banks.Add(ba);
            opp.db.SaveChanges();


            return ba.Id;
            
            

            
             
            
           

            
             
        }
        
        public Bank GetBank(ref AccountModelsLib.operations opp, long SubAccount_Id)
        {
            Bank bnk = opp.db.Banks.Where(b => b.SubAccount_Id == SubAccount_Id).FirstOrDefault();
             
            
            //string mysql = "SELECT * FROM Banks where SubAccount_Id =" + SubAccount_Id;
            //DataTable dt = opp.return_DataTable(mysql);

            return bnk;
        }
        public void UpdateBank(ref int Excp, ref AccountModelsLib.operations opp, Submain_Account  sa)
        {

            //DataTable dt = GetBank(ref opp, sa.SubAccount_Id);
            Bank bnk = GetBank(ref opp, sa.Id);
            if (bnk == null)
            {
                bnk.Bank_Name = sa.SubAccount_Name;
                bnk.Primary_balance = 0;
                bnk.Last_Balance = 0;
                bnk.SubAccount_Id = sa.Id;
                bnk.Currency_Id = sa.Currency_Id;

                bnk.firstBalanceDate = DateTime.Now.Date;
                bnk.Account_Code = sa.SubAccount_NO;
                bnk.Branch_Id = sa.Branch_Id;
                bnk.Account_Type_Value = sa.Account_Customer_Type_Value;
                bnk.Id = sa.Id;

                InsertBank(ref Excp, ref opp, bnk);
            }
            else
            {
                //Id = long.Parse(dt.Rows[0]["Id"].ToString());
                //Get_Bank(ref opp, Id);

                bnk.Bank_Name = sa.SubAccount_Name;
                bnk.Currency_Id = sa.Currency_Id;
                bnk.Account_Code = sa.Account_Code;
                bnk.SubAccount_Id = sa.Id;

                UpdateBank(ref opp, bnk);
            }
        }
        public long UpdateBank(ref AccountModelsLib.operations opp, Bank bnk)
        {
            //string mysql = " Update Banks Set Account_Type_Value=" + Account_Type_Value + ",Branch_Name='" + Branch_Name + "',Account_Branches_ID=" + Account_Branches_ID + ",Bank_Name='" + Bank_Name + "', Primary_balance=" + Primary_Balance + ",Last_Balance=" + Last_Balance + ",Account_No='" + Account_No + "',Is_Bank=" + Is_Bank + ",SubAccount_Id=" + SubAccount_Id + ",currency_Id=" + Currency_Id + ",firstBalanceDate='" + AccountModelsLib.operations.dateFormat(FirstBalanceDate) + "',Supplier_Bank=" + Supplier_Bank + ",Account_Code='" + Account_Code + "' ";
            //mysql = mysql + " Where Id=" + Id;
            //opp.insert_me(mysql);
            opp.db.Entry(bnk).State = EntityState.Modified;
            opp.db.SaveChanges();

            return bnk.Id;

        }
        public void AddNewBank(ref int Excp, ref string Msg, Bank ba)
        {
            Excp = 0;
            Msg = "";
            AccountModelsLib.operations opp = new AccountModelsLib.operations(2);
            try
            {

                string Account_Code = "";
                Sub_Account acc = new Sub_Account();

                long New_Submain_Account_Id = ba.SubAccount_Id;
                acc.ba = ba;
                //int Account_Type_Value = Common.Bank_Account_Type;
                // 🟢 يستخدم فرع الصندوق/البنك نفسه (ba.Branch_Id، الذى اختاره المستخدم فعلياً فى الشاشة) بدل Common.Branch_Id
                // (فرع جلسة المستخدم الحالية) - كانا قد يختلفان لو أضاف المستخدم صندوقاً لفرع غير فرع جلسته الحالية،
                // فيُنشأ حساب الصندوق فى الشجرة بالفرع الخطأ. راجع branch-cash-boxes-plan.md (بند 3).
                acc.Add_AccountSupplierCustomerOther(ref Excp, ref Msg, ref opp, ref New_Submain_Account_Id, ref Account_Code, ba.Account_Type_Value ?? 6, ba.Bank_Name, ba.Branch_Id ?? Common.Branch_Id, ba.Currency_Id ?? 1);

                ba.Id = acc.New_Account_Id;
                ba.SubAccount_Id = New_Submain_Account_Id;

                opp.Commit();
                Excp = 0;

                Excp = 0;
                acc.UpdateSubmain_SubAccount_Id(ref Excp, ref Msg);

            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void DeleteBank(long Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DeleteBank(ref opp, Id);
        }
        public void DeleteBank(ref AccountModelsLib.operations opp, long Id)
        {

            string mysql = "";
            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = "Update Banks set active=0 Where Id= " + Id;
            }
            if (Common.Update_DeleteSupplier == 1)
            {
                mysql = "delete from Banks where Id= " + Id;
            }
            opp.insert_me(mysql);
        }
        public void MakeDeleteBanks(ref AccountModelsLib.operations opp, ref int Excp, ref string Msg, long Id, long SubAccount_Id)
        {
            string mysql = "";

            if (Common.Update_DeleteSupplier == 0)
            {
                mysql = "Update Banks set active=0 Where Id= " + Id;
                mysql = mysql + "Update Banks set active=0 Where SubAccount_Id= " + SubAccount_Id;
            }
            if (Common.Update_DeleteSupplier == 1)
            {
                mysql = "delete from Banks where Id= " + Id;
                mysql = mysql + "delete from Banks where SubAccount_Id= " + SubAccount_Id;
            }
            try
            {
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }

       
    }


}

