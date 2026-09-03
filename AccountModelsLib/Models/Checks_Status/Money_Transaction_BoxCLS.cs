
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data.OleDb;

using AccountModelsLib.Models;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.Models
{
    public class Money_Transaction_BoxCLS
    {
        public Money_Transaction_Box mt = new Money_Transaction_Box();

        operations opp;
        public Money_Transaction_BoxCLS()
        {
            opp = new operations();
        }
        public Money_Transaction_BoxCLS(ref operations opp_)
        {
            opp = opp_;
        }
        public long Lookup_CashPaymentMethodId { get; set; } = 0;
        public string Lookup_CashPaymentMethodName { get; set; } = "";
        public List<Money_Transaction_Box> Get_Money_Transaction_BoxList(long Order_MoneyId)
        {
            List<Money_Transaction_Box> mbLst = new List<Money_Transaction_Box>();
            mbLst = opp.db.Money_Transaction_Box.Where(o => o.Order_Mony_Id == Order_MoneyId).ToList();

            foreach (var item in mbLst)
            {
                if(item.Bank_Name.Trim()=="")
                {
                    Bank bnk = opp.db.Banks.AsNoTracking().Where(i=>i.Id==item.Bank_Id).FirstOrDefault();
                    if(bnk!=null)
                    {
                        item.Bank_Name = bnk.Bank_Name;
                    }
                }
            }
            return mbLst;
        }


        //public void InsertMoney_Transaction_Box(ref int Excp, ref string Msg, ref operations opp)
        //{
        //    //mt.InsertMoney_Transaction_Box(ref Excp, ref Msg, ref opp);
        //}
        public long InsertMoney_Transaction_Box(ref int Excp, ref string Msg, ref operations opp, long Order_Mony_Id, long Order_No, string Bank_Name,DateTime Transaction_Date, int Transaction_Type, decimal Import_Value, decimal Export_Value, decimal Transaction_Value, string Transaction_Value_, decimal Equivalent_Value, long Bank_Id, decimal Bank_Balance, long Supplier_Id, long Customer_Id, long SubAccount_Id, string Notes, decimal Balance_Before, decimal Balance_After, string Balance_BeforStr, string Balance_AfterStr, long BankAccount_Id, int DestructionSales, int Order_mony_id_transform, long Account_Cost_Center_Tree_Id, string Tafkeet_Value, decimal TotalValueWillPaid, decimal Rest_Value, string Account_Name, string Account_Code, int Account_Type, string Cost_Center, int Currency_Id, decimal Purchase_Ticket_Value, decimal Commission_Value, decimal Factor, int Entry_Currency_ID, string CostCenter_Name, long Invoice_Id, int Invoice_TypeId, long CarsWorkOrdersId, long Reference_Type_Table_Id, long DataDictionaryProcessesListsId)
        {
            
         
            Money_Transaction_Box mt = new Money_Transaction_Box();



            mt.Order_Mony_Id = Order_Mony_Id;
            mt.Transaction_Date = Transaction_Date;
            mt.Transaction_Type = Transaction_Type;
            mt.Import_Value = Import_Value;
            mt.Export_Value = Export_Value;
            mt.Transaction_Value = Transaction_Value;
            mt.Transaction_Value_ = Transaction_Value_;
            mt.Equivalent_Value = Equivalent_Value;
            mt.Bank_Id = Bank_Id;
            mt.Bank_Name = Bank_Name;
            mt.Order_No = Order_No;
            mt.Tafkeet_Value = Tafkeet_Value;
            mt.Supplier_Id = Supplier_Id;
            mt.Customer_Id = Customer_Id;
            mt.Account_Id = SubAccount_Id;
            mt.SubAccount_Id = SubAccount_Id;
            mt.Account_Type = Account_Type;
            mt.Invoice_Id = Invoice_Id;
            mt.Invoice_TypeId = Invoice_TypeId;

            mt.CarsWorkOrdersId = CarsWorkOrdersId;
            mt.Reference_Type_Table_Id = Reference_Type_Table_Id;
            mt.DataDictionaryProcessesListsId = DataDictionaryProcessesListsId;

            mt.Notes = Notes;
            mt.Balance_Before = Balance_Before;
            mt.Balance_After = Balance_After;

            mt.Balance_BeforStr = Balance_BeforStr;
            mt.Balance_AfterStr = Balance_AfterStr;

            mt.SubAccount_Id = SubAccount_Id;
            mt.Account_Name = Account_Name;

            mt.Equivalent_Value = Factor;
            mt.Factor = Factor;

            mt.Bank_Balance = Bank_Balance;

            mt.Lookup_CashPaymentMethodId = Lookup_CashPaymentMethodId;
            mt.Lookup_CashPaymentMethodName = Lookup_CashPaymentMethodName;

            mt.BankAccount_Id = BankAccount_Id;
            mt.DestructionSales = DestructionSales;
            mt.Order_mony_id_transform = Order_mony_id_transform;
            mt.Account_Cost_Center_Tree_Id = Account_Cost_Center_Tree_Id;
            mt.TotalValueWillPaid = TotalValueWillPaid;
            mt.Rest_Value = Rest_Value;
            mt.Account_Code = Account_Code;
            mt.Cost_Center = Cost_Center;
            mt.Currency_Id = Currency_Id;
            mt.Purchase_Ticket_Value = Purchase_Ticket_Value;
            mt.Commission_Value = Commission_Value;
            mt.Entry_Currency_ID = Entry_Currency_ID;
            mt.CostCenter_Name = CostCenter_Name;

            mt.User_Id = Common.User_Id;
            mt.Branch_Id = Common.Branch_Id;



            InsertMoney_Transaction_Box_(ref Excp, ref Msg, mt, ref opp);
            return mt.Id;
        }
        public void InsertMoney_Transaction_Box_(ref int Excp, ref string Msg, Money_Transaction_Box mt, ref operations opp)
        {

            try
            {
                var order = opp.db.Order_Money.AsNoTracking().Where(i => i.Id == mt.Order_Mony_Id).FirstOrDefault();
                if (order != null)
                {
                    mt.Order_No = order.Order_No;
                    //mt.Bank_Name = order.Bank.Bank_Name;
                }
                else
                {
                    mt.Order_No = 0;
                    mt.Bank_Name = "";
                }
            }
            catch (Exception ee)
            {

            }


            try
            {

                long Id__ = 0;
                long B_Id__ = 0;

                Common.GetTable_Id(ref opp, "Money_Transaction_Box", "Id", ref Id__, ref B_Id__);


                string mysql = " ";
                if (Common.Use_Auto_Identity_Column == 0)
                {

                    //mysql = "INSERT INTO Money_Transaction_Box(Id, Serial, Order_Mony_Id, Transaction_Date, Transaction_Type, Import_Value, Export_Value, Transaction_Value, Transaction_Value_, Equivalent_Value, Bank_Id, Bank_Balance, Supplier_Id, Customer_Id, Account_Id,SubAccount_Id, Notes, Balance_Before, Balance_After,Balance_BeforStr,Balance_AfterStr, BankAccount_Id, DestructionSales, Order_mony_id_transform, Account_Cost_Center_Tree_Id, User_Id, Branch_Id, Tafkeet_Value, TotalValueWillPaid, Rest_Value,Account_Name, Account_Code, Account_Type, Cost_Center, Currency_Id, Purchase_Ticket_Value, Commission_Value, Factor, Entry_Currency_ID, CostCenter_Name, OperationDate, Insert_ServerDateTime,Update_ServerDateTime, EditOperationDate, Edit_User_Id,Invoice_Id,Invoice_TypeId, Lookup_CashPaymentMethodId,Lookup_CashPaymentMethodName)";
                    //mysql = mysql + " VALUES( @Id,@Serial ,@Order_Mony_Id ,@Transaction_Date ,@Transaction_Type ,@Import_Value ,@Export_Value ,@Transaction_Value ,@Transaction_Value_ ,@Equivalent_Value ,@Bank_Id ,@Bank_Balance ,@Supplier_Id ,@Customer_Id ,@Account_Id,@SubAccount_Id ,@Notes ,@Balance_Before ,@Balance_After ,@Balance_BeforStr,@Balance_AfterStr,@BankAccount_Id ,@DestructionSales ,@Order_mony_id_transform ,@Account_Cost_Center_Tree_Id ,@User_Id ,@Branch_Id ,@Tafkeet_Value ,@TotalValueWillPaid ,@Rest_Value,@Account_Name ,@Account_Code ,@Account_Type ,@Cost_Center ,@Currency_Id ,@Purchase_Ticket_Value ,@Commission_Value ,@Factor ,@Entry_Currency_ID ,@CostCenter_Name ,@OperationDate ,@Insert_ServerDateTime,@Update_ServerDateTime ,@EditOperationDate ,@Edit_User_Id,@Invoice_Id,@Invoice_TypeId, @Lookup_CashPaymentMethodId,@Lookup_CashPaymentMethodName)";

                    if (mt.Id == 0)
                    {
                        mt.Id = Id__;
                        //mt.Serial = B_Id__;
                        DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp, ref   Msg, ref   opp,mt);
                        opp.db.Money_Transaction_Box.Add(mt);
                        opp.db.SaveChanges();
                    }
                    else
                    {

                        Money_Transaction_Box mbx = opp.db.Money_Transaction_Box.AsNoTracking().Where(i => i.Id == mt.Id).FirstOrDefault();
                        if (mbx != null)
                        {
                            opp.db.Entry(mt).State = EntityState.Modified;
                            opp.db.SaveChanges();
                        }
                    }

                    //opp.cmd.Parameters.AddWithValue("@Id", Id__);
                    //opp.cmd.Parameters.AddWithValue("@Serial", B_Id__);

                    //SetParameters(ref opp,mt);
                    //mt.Id = B_Id__;
                    //opp.insert_me(mysql);


                }
                else
                {
                    if (mt.Id == 0)
                    {
                        DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp, ref Msg, ref opp, mt);

                        //mt.Serial = B_Id__;
                        opp.db.Money_Transaction_Box.Add(mt);
                        opp.db.SaveChanges();
                    }
                    else
                    {

                        Money_Transaction_Box mbx = opp.db.Money_Transaction_Box.AsNoTracking().Where(i => i.Id == mt.Id).FirstOrDefault();
                        if (mbx != null)
                        {

                            opp.db.Entry(mt).State = EntityState.Modified;
                            opp.db.SaveChanges();
                        }
                    }
 
                    Excp = 0;// opp.Excp;
                }


                opp.cmd.Parameters.Clear();
                //////////يستخدم فى حالة توزيع مبلغ مدفوع على عدة فواتير للعميل
                InsertMoney_Transaction_BoxForInvoices(ref opp, mt);
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.cmd.Parameters.Clear();
            }


        }

        public void InsertMoney_Transaction_BoxForInvoices(ref operations opp, Money_Transaction_Box mt)
        {

            if (mt != null)
            {
                var SearchMti = opp.db.Money_Transaction_BoxForInvoices.Where(i => i.Money_Transaction_BoxId == mt.Id).ToList();
                if (SearchMti.Count() > 1)
                {
                    /////فى حالة وجود اكثر من سجل يتم حذفهم لانه تم توزيعهم بواسطة توزيع المبلغ على الفواتير
                    opp.db.Money_Transaction_BoxForInvoices.RemoveRange(SearchMti);
                    opp.db.SaveChanges();
                    return;
                }
                if (SearchMti.Count() == 1)
                {
                    /////فى حالة وجود سجل واحد فقط يتم حذفه لانه سيتم اعادة اضافته
                    opp.db.Money_Transaction_BoxForInvoices.RemoveRange(SearchMti);
                    opp.db.SaveChanges();

                }
            }
            try
            {
                Money_Transaction_BoxForInvoice mtv = null;
                if (mt.Account_Type == 2)
                {
                    mt.Invoice_Id = mt.Invoice_Id ?? 0;
                    if (mt.Invoice_TypeId == Common.Invoice_Type_Id & mt.Invoice_Id > 0)
                    {

                        long Invoices_ListId = 0;
                        long.TryParse(mt.Invoice_Id.ToString(), out Invoices_ListId);
                        if (mt.Invoice_Id > 0)
                        {
                            mtv = new Money_Transaction_BoxForInvoice()
                            {
                                Invoices_ListId = Invoices_ListId,
                                Bills_ListId = 0,

                                Invoice_TypeId = mt.Invoice_TypeId,
                            };
                            opp.db.Money_Transaction_BoxForInvoices.Add(mtv);
                            opp.db.SaveChanges();
                        }
                    }
                }


                if (mt.Account_Type == 5)
                {
                    mt.Invoice_Id = mt.Invoice_Id ?? 0;
                    if (mt.Invoice_TypeId == Common.Bill_Type_Id & mt.Invoice_Id > 0)
                    {

                        long Invoices_ListId = 0;
                        long.TryParse(mt.Invoice_Id.ToString(), out Invoices_ListId);
                        if (mt.Invoice_Id > 0)
                        {
                            mtv = new Money_Transaction_BoxForInvoice()
                            {
                                Invoices_ListId = 0,
                                Bills_ListId = Invoices_ListId,
                                Invoice_TypeId = mt.Invoice_TypeId,


                            };
                            opp.db.Money_Transaction_BoxForInvoices.Add(mtv);
                            opp.db.SaveChanges();
                        }
                    }
                }

                if (mtv != null)
                {
                    decimal Import_Value = 0;
                    decimal Export_Value = 0;
                    if (mt.Transaction_Type==1)
                    {
                        Import_Value = mt.Transaction_Value.Value;
                        Export_Value = 0;
                    }
                    else
                    {
                        Import_Value = 0;
                        Export_Value = mt.Transaction_Value.Value;

                    }
                    Money_Transaction_BoxForInvoice mtvATT = new Money_Transaction_BoxForInvoice()
                    {
                        Invoices_ListId = mtv.Invoices_ListId,
                        Bills_ListId = mtv.Invoices_ListId,
                        Invoice_TypeId = mtv.Invoice_TypeId,
                        
                        Transaction_Date = mt.Transaction_Date.Value,
                        Money_Transaction_BoxId = mt.Id,
                        Transaction_Value = mt.Transaction_Value.Value,
                        Import_Value = Import_Value,
                        Export_Value = Export_Value,
                        Customer_Id = mt.Customer_Id,
                        Supplier_Id = mt.Supplier_Id,
                        SubAccount_Id = mt.SubAccount_Id,
                        Transaction_Type = mt.Transaction_Type,
                        Bank_Id = mt.Bank_Id,
                        Reference_Type_Table_Id = mt.Reference_Type_Table_Id,
                        DataDictionaryProcessesListsId = mt.DataDictionaryProcessesListsId,

                        Order_Mony_Id = mt.Order_Mony_Id,
                        Order_Total_Value = mt.Transaction_Value,
                        Order_No = mt.Order_No,


                    };
                    opp.db.Money_Transaction_BoxForInvoices.Add(mtvATT);
                    opp.db.SaveChanges();

                }
            }
            catch (Exception ee)
            {

            }
        }

        public void SaveUpdateMoney_Transaction_BoxForInvoice(ref int Excp, ref string Msg, Money_Transaction_Box mt, List<Money_Transaction_BoxForInvoice> mtbInvLst)
        {
            try
            {

                var mtbinvoice = opp.db.Money_Transaction_BoxForInvoices.Where(i => i.Money_Transaction_BoxId == mt.Id).ToList();
                if(mtbinvoice!=null & mtbinvoice.Count()>0)
                {
                    opp.db.Money_Transaction_BoxForInvoices.RemoveRange(mtbinvoice);
                    opp.db.SaveChanges();
                }
                foreach (var item in mtbInvLst)
                {

                    if (item.Id == 0)
                    {
                        opp.db.Money_Transaction_BoxForInvoices.Add(item);
                    }
                    else
                    {
                        var SearchMti = opp.db.Money_Transaction_BoxForInvoices.Where(i => i.Id == item.Id).FirstOrDefault();
                        if (SearchMti != null)
                        {
                            opp.db.Entry(SearchMti).CurrentValues.SetValues(item);

                        }
                        else
                        {
                           opp.db.Money_Transaction_BoxForInvoices.Add(item);   
                        }
                    }

                }
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg=ee.Message;
              
            }
        }
        public void DeleteMoney_Transaction_BoxForInvoice(ref int Excp, ref string Msg, Money_Transaction_Box mt)
        {
            try
            {

                var mtbinvoice = opp.db.Money_Transaction_BoxForInvoices.Where(i => i.Money_Transaction_BoxId == mt.Id).ToList();
                if (mtbinvoice != null & mtbinvoice.Count() > 0)
                {
                    opp.db.Money_Transaction_BoxForInvoices.RemoveRange(mtbinvoice);
                    opp.db.SaveChanges();
                }
                
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
        }
        void SetParameters(ref operations opp, Money_Transaction_Box mt)
        {


            opp.cmd.Parameters.AddWithValue("@Order_Mony_Id", mt.Order_Mony_Id);
            opp.cmd.Parameters.AddWithValue("@Transaction_Date", mt.Transaction_Date);
            opp.cmd.Parameters.AddWithValue("@Transaction_Type", mt.Transaction_Type);
            opp.cmd.Parameters.AddWithValue("@Import_Value", mt.Import_Value);
            opp.cmd.Parameters.AddWithValue("@Export_Value", mt.Export_Value);
            opp.cmd.Parameters.AddWithValue("@Transaction_Value", mt.Transaction_Value);
            opp.cmd.Parameters.AddWithValue("@Transaction_Value_", mt.Transaction_Value_);
            opp.cmd.Parameters.AddWithValue("@Equivalent_Value", mt.Equivalent_Value);
            opp.cmd.Parameters.AddWithValue("@Bank_Id", mt.Bank_Id);
            opp.cmd.Parameters.AddWithValue("@Bank_Balance", mt.Bank_Balance);
            opp.cmd.Parameters.AddWithValue("@Supplier_Id", mt.Supplier_Id);
            opp.cmd.Parameters.AddWithValue("@Customer_Id", mt.Customer_Id);
            opp.cmd.Parameters.AddWithValue("@Account_Id", mt.Account_Id);
            opp.cmd.Parameters.AddWithValue("@Invoice_Id", mt.Invoice_Id);
            opp.cmd.Parameters.AddWithValue("@Invoice_TypeId", mt.Invoice_TypeId);

            opp.cmd.Parameters.AddWithValue("@Notes", mt.Notes);
            opp.cmd.Parameters.AddWithValue("@SubAccount_Id", mt.SubAccount_Id);
            opp.cmd.Parameters.AddWithValue("@Balance_Before", mt.Balance_Before);
            opp.cmd.Parameters.AddWithValue("@Balance_After", mt.Balance_After);

            opp.cmd.Parameters.AddWithValue("@Balance_BeforStr", mt.Balance_BeforStr);
            opp.cmd.Parameters.AddWithValue("@Balance_AfterStr", mt.Balance_AfterStr);

            opp.cmd.Parameters.AddWithValue("@BankAccount_Id", mt.BankAccount_Id);
            opp.cmd.Parameters.AddWithValue("@DestructionSales", mt.DestructionSales);
            opp.cmd.Parameters.AddWithValue("@Order_mony_id_transform", mt.Order_mony_id_transform);
            opp.cmd.Parameters.AddWithValue("@Account_Cost_Center_Tree_Id", mt.Account_Cost_Center_Tree_Id);
            opp.cmd.Parameters.AddWithValue("@User_Id", mt.User_Id);
            opp.cmd.Parameters.AddWithValue("@Branch_Id", mt.Branch_Id);
            opp.cmd.Parameters.AddWithValue("@Tafkeet_Value", mt.Tafkeet_Value);
            opp.cmd.Parameters.AddWithValue("@TotalValueWillPaid", mt.TotalValueWillPaid);
            opp.cmd.Parameters.AddWithValue("@Rest_Value", mt.Rest_Value);
            opp.cmd.Parameters.AddWithValue("@Account_Name", mt.Account_Name.ToString());

            opp.cmd.Parameters.AddWithValue("@Account_Code", mt.Account_Code);
            opp.cmd.Parameters.AddWithValue("@Account_Type", mt.Account_Type);
            opp.cmd.Parameters.AddWithValue("@Cost_Center", mt.Cost_Center);
            opp.cmd.Parameters.AddWithValue("@Currency_Id", mt.Currency_Id);
            opp.cmd.Parameters.AddWithValue("@Purchase_Ticket_Value", mt.Purchase_Ticket_Value);
            opp.cmd.Parameters.AddWithValue("@Commission_Value", mt.Commission_Value);
            opp.cmd.Parameters.AddWithValue("@Factor", mt.Factor);
            opp.cmd.Parameters.AddWithValue("@Entry_Currency_ID", mt.Entry_Currency_ID);
            opp.cmd.Parameters.AddWithValue("@CostCenter_Name", mt.CostCenter_Name);
            opp.cmd.Parameters.AddWithValue("@OperationDate", mt.OperationDate);
            opp.cmd.Parameters.AddWithValue("@Insert_ServerDateTime", mt.Insert_ServerDateTime);
            opp.cmd.Parameters.AddWithValue("@Update_ServerDateTime", mt.Update_ServerDateTime);
            opp.cmd.Parameters.AddWithValue("@EditOperationDate", mt.EditOperationDate);
            opp.cmd.Parameters.AddWithValue("@Edit_User_Id", mt.Edit_User_Id);
            opp.cmd.Parameters.AddWithValue("@Lookup_CashPaymentMethodId", mt.Lookup_CashPaymentMethodId);
            opp.cmd.Parameters.AddWithValue("@Lookup_CashPaymentMethodName", mt.Lookup_CashPaymentMethodName);



        }

       
    }
}
