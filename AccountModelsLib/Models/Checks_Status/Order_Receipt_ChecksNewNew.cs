using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountModelsLib; using AccountModelsLib.Models;
using AccountModelsLib;
namespace AccountCheckMoney
{
    public class Order_Receipt_ChecksNewNew
    {
        public Order_Receipt_ChecksNewNew()
        {

        }
        // نوع حساب شجرة الحسابات المختار (كان يُقرأ من AccountTreeViewUSCTRL الثابتة؛ صار يُمرَّر من المستدعى لأن الكلاس انتقل لمكتبة بلا واجهات).
        public long Tree_Account_Customer_Type_ID = 0;
        public int Tree_Account_Customer_Type_Value = 0;
        public long Account_Customer_Type_ID_From = 0;
        public int Account_Customer_Type_Value_From = 0;

        public long Account_Customer_Type_ID_To = 0;
        public int Account_Customer_Type_Value_To = 0;
        public bool Transfer_Check = false;
        public long Branch_Id = 1;
        public Checks_MemoryCLS cc = new Checks_MemoryCLS();
        public Order_Money_Check or = new Order_Money_Check();
        public Checks_Memory chm = new Checks_Memory();
        public Checks_Memory Ctransfeer = new Checks_Memory();
        public Checks_MemoryMainDataList chmlist = new Checks_MemoryMainDataList();
        public void GetChecks_MemoryData(long Checks_MemoryId)
        {
            cc.GetChecks_MemoryId(Checks_MemoryId);
            chm = cc.cm;
            or.GetOrder_Money_Check((long)chm.Order_Mony_Id);


        }

        public void Add_New_Check(ref int Excp, ref string Msg, bool EstlanCheckSader, bool Transfer_Check, Checks_MemoryTasweaCLS cts)
        {
            int Receipt = 0;
            operations opp = new operations(1);
            DateTime ProcessDate = (DateTime)chm.Day_Date;
            try
            {
                if (or.Transaction_type == 1)    /// import
                {

                    chm.Import_Value = chm.Check_Value;
                    chm.Export_Value = 0;
                    chm.Receipt = 1;  //فى حافظة الشيكات الواردة
                }
                if (or.Transaction_type == 2)    /// Export_Value
                {
                    chm.Export_Value = chm.Check_Value;
                    chm.Import_Value = 0;

                    chm.Receipt = 32;  //فى حافظة الشيكات الصادرة
                }
                //if (or.Transaction_type == 2 & Transfer_Check == true)
                //{
                //    chm.Receipt = 30;  // 
                //}
                //else
                //{


                //}

                or.Bank_Type = 1;
                or.Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id((DateTime)or.Order_Date);

                or.Total_Value = (decimal)chm.Check_Value;
                or.Tafkeet_Value = chm.Check_Value_;
                or.Order_Tafkeet_Value = chm.Check_Value_;
                or.InsertOrder_Money_Check(ref Excp, ref Msg, ref opp);
                chm.Order_Mony_Id = or.Id;
                if (Excp == 0)
                {
                    cc.InsertChecks_Memory(ref Excp, ref Msg, ref opp, chm, chmlist);
                }
                if (Excp == 0)
                {
                    string Notes = "";
                    DateTime EntryDate = (DateTime)or.Order_Date;
                    long Checks_Memory_Id = chm.Id;
                    //فى حالة تظهير الشيك

                    if (chm.Transfer_Check_Id > 0 & chm.Transaction_Type == 2)
                    {
                        //return;
                        Receipt = 30; //transfeered check
                        Notes = " تظهير الشيك رقم " + chm.Check_No;


                    }
                    //فى حالة استلام شيك صادر     
                    else if (chm.Transfer_Check_Id > 0 & chm.Transaction_Type == 1)
                    {
                        Receipt = 37; //شيك صادر تم استلامة

                        Notes = " شيك صادر تم استلامة رقم " + chm.Check_No;


                    }

                    if (chm.Transfer_Check_Id > 0)
                    {
                        int IsAllowEdit = 0;
                        cc.Change_CheckStaus(ref Excp, ref Msg, ref opp, Receipt, chm, (long)chm.Transfer_Check_Id, IsAllowEdit);

                        Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                        // aa.Make_EntryFor_Checks_Memory(ref Excp, ref Msg, ref opp, EntryDate, Checks_Memory_Id, cts, Notes, true);
                        bool IsTaswea = true;
                        ////قيد التسسوية
                        aa.Make_EntryChecks_Memory(ref Excp, ref Msg, ref opp, EntryDate, Checks_Memory_Id, cts, IsTaswea, Notes, true);

                        int Transaction_Type_ = 1;
                        if ((int)Ctransfeer.Transaction_Type == 1)
                        {
                            Transaction_Type_ = 2;
                        }
                        //SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, Transaction_Type_, Account_Customer_Type_Value_, Account_Customer_Type_ID_, 0, ProcessDate, true);
                        ////حساب العميل المورد لقيد التسوية    
                        SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, 2, chm, cts.Account_Customer_Type_Value_From_Taswea, cts.Account_Customer_Type_ID_From_Taswea, 0, ProcessDate, true);
                        SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, 1, chm, cts.Account_Customer_Type_Value_To_Taswea, cts.Account_Customer_Type_ID_To_Taswea, 0, ProcessDate, true);

                        IsTaswea = false;

                        aa.Make_EntryChecks_Memory(ref Excp, ref Msg, ref opp, EntryDate, Checks_Memory_Id, cts, IsTaswea, Notes, true);


                        if (Excp == 0)
                        {
                            //Submain_AccountCLS saFrom = new Submain_AccountCLS();
                            //saFrom.Get_Submain_Account((long)Ctransfeer.SubAccount_Id);
                            //int Account_Customer_Type_Value_ = saFrom.Account_Customer_Type_Value;
                            //long Account_Customer_Type_ID_ = saFrom.Account_Customer_Type_ID;


                        }

                    }
                    else
                    {

                        //long SubAccount_Id_From = 0;
                        //long SubAccount_Id_From = 0;
                        // انشاء القيد الجديد
                        Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                        //aa.Account_Entry_List_ID =  (long)chm.Account_Entry_List_ID;
                        //aa.Make_EntryForMoney_Checks_Memory(ref Excp, ref Msg, ref opp, (long)chm.SubAccount_Id_RelatedFrom, chm.Id, (DateTime)or.Order_Date, (DateTime)or.Order_Date, or.Transaction_type, 0, false);
                        aa.Make_EntryChecks_Memory(ref Excp, ref Msg, ref opp, EntryDate, Checks_Memory_Id, cts, Notes, false);
                    }
                }

                if (Excp == 0)
                {
                    ////حساب العميل المورد للقيد الجديد او النهائى
                    int Transaction_Type = (int)chm.Transaction_Type;
                    SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, Transaction_Type, chm, Account_Customer_Type_Value_From, Account_Customer_Type_ID_From, 0, ProcessDate, true);
                    SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, Transaction_Type, chm, Account_Customer_Type_Value_To, Account_Customer_Type_ID_To, 0, ProcessDate, true);




                }

                if (Excp == 0)
                {
                    cc.Checks_MemoryHistory(ref Excp, ref Msg, ref opp, chm.Id);
                }
                if (Excp == 0)
                {
                    opp.Commit();

                }
                else
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

        public void Edit_Check(ref int Excp, ref string Msg,int IsAllowEdit)
        {
            operations opp = new operations(1);
            try
            {
                // شيك أول المدة: اعكس أثره القديم على الرصيد قبل أى تعديل (القيم القديمة تُقرأ من القاعدة)
                bool isOpening = ReverseFirstBalanceFromDb(ref Excp, ref Msg, ref opp, chm.Id);
                DateTime ProcessDate = (DateTime)chm.Day_Date;
                or.Bank_Type = 1;
                or.Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id((DateTime)or.Order_Date);

                or.Total_Value = (decimal)chm.Check_Value;
                or.Tafkeet_Value = chm.Check_Value_;
                or.Order_Tafkeet_Value = chm.Check_Value_;
                or.Update_Order_Money_Check(ref Excp, ref Msg, ref opp);
                //chm.Order_Mony_Id = or.Id;

                if (Excp == 0)
                {
                    chm.Edit_User_Id = Common.User_Id;
                    chm.EditOperationDate = DateTime.Now.Date;

                    cc.UpdateChecks_Memory(ref Excp, ref Msg, ref opp, chm, chmlist);
                }

                // ثم طبّق القيمة الجديدة بنفس مسار الإضافة (شيك أول المدة بلا حركات أطراف ولا قيد)
                if (isOpening && Excp == 0)
                {
                    BtnSaveFirstBalance_Click(ref Excp, ref Msg, ref opp);
                }

                if (IsAllowEdit == 1 && !isOpening)
                {
                    if (Excp == 0)
                    {
                        int Transaction_Type =   (int)chm.Transaction_Type;
                        SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, Transaction_Type, chm, Account_Customer_Type_Value_From, Account_Customer_Type_ID_From, (long)chm.ProcessNo, ProcessDate, true);

                    }

                    if (Excp == 0)
                    {
                        int Transaction_Type =  (int)chm.Transaction_Type;
                        SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, Transaction_Type, chm, Account_Customer_Type_Value_To, Account_Customer_Type_ID_To, (long)chm.ProcessNo, ProcessDate, true);

                    }
                    if (Excp == 0)
                    {

                        //انشاء القيد
                        Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                        aa.Account_Entry_List_ID = (long)chm.Account_Entry_List_ID;
                        aa.Make_EntryForMoney_Checks_Memory(ref Excp, ref Msg, ref opp, (long)chm.SubAccount_Id_RelatedFrom, chm.Id, (DateTime)or.Order_Date, (DateTime)or.Order_Date, or.Transaction_type, 0,false );
                    }
                    if (Excp == 0)
                    {
                        cc.Checks_MemoryHistory(ref Excp, ref Msg, ref opp, chm.Id);
                    }
                }
                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
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

        public void Delete_Check(ref int Excp, ref string Msg)
        {
            long Checks_MemoryId = chm.Id;
            DataTable dt = cc.GeChecks_MemoryHistory(chm.Id);


            Excp = 0;
            Msg = "";

            operations opp = new operations(1);
            try
            {

                // شيك أول المدة: اعكس أثره على الرصيد قبل حذفه
                ReverseFirstBalanceFromDb(ref Excp, ref Msg, ref opp, Checks_MemoryId);
                cc.DeleteChecks_Memory(ref Excp, ref Msg, ref opp, Checks_MemoryId);

                or.DeleteOrder_Money_Check(ref Excp, ref Msg, ref opp, (long)chm.Order_Mony_Id);


                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    long Account_Entry_List_ID = long.Parse(dt.Rows[x]["Account_Entry_List_ID"].ToString());
                    long ProcessNo = long.Parse(dt.Rows[x]["ProcessNo"].ToString());

                    CustomersAccountCLS ca = new CustomersAccountCLS();
                    ca.Delete_Customer_Accounts_ProcessNo(ref Excp, ref Msg, ref opp, ProcessNo);
                    Suppliers_AccountsCls sa = new Suppliers_AccountsCls();
                    sa.Delete_Suppliers_Accounts_ProcessNo(ref Excp, ref Msg, ref opp, ProcessNo);


                    AccountEntry_CLS ac = new AccountEntry_CLS();
                    ac.Delete_Account_Entry(ref Excp, ref Msg, ref opp, Account_Entry_List_ID);
                }

                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();

                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                opp.Rollback();

            }
        }
        /// <summary>
        /// شيك أول المدة (Balance_BeginingPeriod=1) يترك أثره فى Account_Entry_Master_Balance عند إضافته؛ هذه الدالة تعكس هذا الأثر
        /// (تضيف القيمة القديمة سالبة بنفس نوع الحركة والمعامل والفترة والفرع) قبل تعديل الشيك أو حذفه، فلا يبقى الرصيد على قيمة قديمة.
        /// تقرأ القيم القديمة من القاعدة (لأن كائن الشيك فى الذاكرة يحمل القيم الجديدة وقت التعديل). لا تفعل شيئاً لغير شيك أول المدة.
        /// ترجع true إن كان الشيك شيك أول مدة.
        /// </summary>
        bool ReverseFirstBalanceFromDb(ref int Excp, ref string Msg, ref operations opp, long Checks_MemoryId)
        {
            DataTable dt = opp.return_DataTable(" SELECT c.Check_Value, c.SubAccount_Id_RelatedTo, c.SubAccount_Name_RelatedTo, c.Currency_Id, c.Transaction_Type, c.Factor, c.Balance_BeginingPeriod, c.Day_Date, c.Branch_Id AS Check_Branch_Id, o.Order_Date, o.Branch_Id AS Order_Branch_Id " +
                " FROM Checks_Memory c LEFT OUTER JOIN Order_Money_Check o ON c.Order_Mony_Id = o.Id WHERE c.Id=" + Checks_MemoryId);
            if (dt.Rows.Count == 0) return false;
            DataRow r = dt.Rows[0];
            if (r["Balance_BeginingPeriod"] == DBNull.Value || Convert.ToDecimal(r["Balance_BeginingPeriod"], System.Globalization.CultureInfo.InvariantCulture) != 1) return false;

            long relatedId = r["SubAccount_Id_RelatedTo"] == DBNull.Value ? 0 : Convert.ToInt64(r["SubAccount_Id_RelatedTo"]);
            decimal value = r["Check_Value"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Check_Value"], System.Globalization.CultureInfo.InvariantCulture);
            if (relatedId == 0 || value == 0) return true;

            long currencyId = r["Currency_Id"] == DBNull.Value ? Common.Entry_Currency_Id : Convert.ToInt64(r["Currency_Id"]);
            int transType = r["Transaction_Type"] == DBNull.Value ? 1 : Convert.ToInt32(Convert.ToDecimal(r["Transaction_Type"], System.Globalization.CultureInfo.InvariantCulture));
            decimal factor = r["Factor"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Factor"], System.Globalization.CultureInfo.InvariantCulture);
            if (factor <= 0) factor = currencyId == Common.Entry_Currency_Id ? 1 : Common.RoundMe(new CurrencyFactorsCLS().GetFactors(Common.Entry_Currency_Id, currencyId));
            DateTime periodDate = r["Order_Date"] != DBNull.Value ? Convert.ToDateTime(r["Order_Date"]) : (r["Day_Date"] != DBNull.Value ? Convert.ToDateTime(r["Day_Date"]) : DateTime.Now.Date);
            long branchId = r["Order_Branch_Id"] != DBNull.Value ? Convert.ToInt64(r["Order_Branch_Id"]) : (r["Check_Branch_Id"] != DBNull.Value ? Convert.ToInt64(r["Check_Branch_Id"]) : Common.Branch_Id);
            long yearPeriodId = Financial_PeriodCLS.Get_Year_Financial_Period_Id(periodDate);

            new FirstBalance_CLS().Save_FirstBalanceForCheck(ref Excp, ref Msg, ref opp, relatedId, r["SubAccount_Name_RelatedTo"]?.ToString() ?? "", yearPeriodId, branchId, currencyId, -value, transType, factor);
            return true;
        }
        public void Add_New_CheckFirstBalance(ref int Excp, ref string Msg)
        {
            operations opp = new operations(1);
            try
            {



                if (or.Transaction_type == 1)    /// import
                {
                    chm.Import_Value = chm.Check_Value;
                    chm.Export_Value = 0;
                    chm.Receipt = 1;
                }
                if (or.Transaction_type == 2)    /// import
                {
                    chm.Export_Value = chm.Check_Value;
                    chm.Import_Value = 0;
                    chm.Receipt = 32;
                }
                if (or.Transaction_type == 2 & Transfer_Check == true)
                {

                }
                else
                {


                }

                or.Bank_Type = 1;
                or.Year_Financial_Period_Id = Financial_PeriodCLS.Get_Year_Financial_Period_Id((DateTime)or.Order_Date);

                or.Total_Value = (decimal)chm.Check_Value;
                or.Tafkeet_Value = chm.Check_Value_;
                or.Order_Tafkeet_Value = chm.Check_Value_;
                or.InsertOrder_Money_Check(ref Excp, ref Msg, ref opp);
                chm.Order_Mony_Id = or.Id;
                if (Excp == 0)
                {
                    cc.InsertChecks_Memory(ref Excp, ref Msg, ref opp, chm, chmlist);
                }
                if (Excp == 0)
                {

                    BtnSaveFirstBalance_Click(ref Excp, ref Msg, ref opp);
                }
                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
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
        private void BtnSaveFirstBalance_Click(ref int Excp, ref string Msg, ref operations opp)
        {

            Excp = 0;
            Msg = "";
            long Submain_Account_ID = (long)chm.SubAccount_Id_RelatedTo;
            string SubAccount_Name = chm.SubAccount_Name_RelatedTo;
            long    Currency_Id = (long)chm.Currency_Id;

            long Year_Financial_Period_Id = or.Year_Financial_Period_Id;
            long Branch_Id = or.Branch_Id;

            CurrencyFactorsCLS cf = new CurrencyFactorsCLS();
            // المعامل المخزَّن على الشيك (يطابق ما يُعكَس عند التعديل/الحذف)، وإلا معامل العملة الحالى.
            decimal Factor = (chm.Factor ?? 0) > 0 ? (decimal)chm.Factor : (Currency_Id == Common.Entry_Currency_Id ? 1 : Common.RoundMe(cf.GetFactors(Common.Entry_Currency_Id, Currency_Id)));


            decimal First_Balance = (decimal)chm.Check_Value;

            FirstBalance_CLS ff = new FirstBalance_CLS();
            ff.Account_Customer_Type_ID = Tree_Account_Customer_Type_ID;
            ff.Account_Customer_Type_Value = Tree_Account_Customer_Type_Value;


            ff.Save_FirstBalanceForCheck(ref Excp, ref Msg, ref opp, Submain_Account_ID, SubAccount_Name, Year_Financial_Period_Id, Branch_Id, Currency_Id, First_Balance, (int)chm.Transaction_Type, Factor);


        }

        void SetSupplier_CustomerAccount(ref int Excp, ref string Msg, ref operations opp, int Transaction_Type, Checks_Memory cm, int Account_Customer_Type_Value, long Account_Customer_Type_ID, long ProcessNo, DateTime ProcessDate, bool IsDeletePrevOperation)
        {


            if (Account_Customer_Type_Value == Common.Customer_Account_Type) // عملاء
            {
                CustomersAccountCLS cu = new CustomersAccountCLS();
                cu.ProcessNo = ProcessNo;
                long Customer_Id = Account_Customer_Type_ID;
                cu.Set_Customer_Balance_With_CheckMoney(ref Excp, ref opp, Customer_Id, Transaction_Type, (long)cm.Order_Mony_Id, cm.Id, (decimal)cm.Check_Value, cm.Check_Notes, ProcessDate, IsDeletePrevOperation);


            }

            if (Account_Customer_Type_Value == Common.Supplier_Account_Type) // موردين
            {
                Suppliers_AccountsCls sp = new Suppliers_AccountsCls();
                sp.ProcessNo = ProcessNo;
                long Supplier_Id = Account_Customer_Type_ID;
                sp.Set_Supplier_Balance_With_CheckMoney(ref Excp, ref opp, Supplier_Id, Transaction_Type, (long)cm.Order_Mony_Id, cm.Id, (decimal)chm.Check_Value, cm.Check_Notes, ProcessDate, IsDeletePrevOperation);

            }
            //if (Excp == 0)
            //{
            //    //انشاء القيد
            //    Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
            //    aa.Make_EntryForMoney_Checks_Memory(ref Excp, ref Msg, ref opp, chm.Id, (DateTime)or.Order_Date, (DateTime)or.Order_Date, or.Transaction_type, 0, false);
            //}

        }
        public void SetCkeckMoneyAccountStatus(ref int Excp, ref string Msg, Checks_MemoryCLS cc, int Receipt, DateTime EntryDate, long SubAccount_Id_RelatedFromNew, string SubAccount_Name_RelatedFromNew, int Account_Customer_Type_ValueFromNew, long Account_Customer_Type_IDNew, string Notes, long SubAccount_Id_RelatedToNew, string SubAccount_Name_RelatedToNew, int Account_Customer_Type_ValueToNew, long Account_Customer_Type_IDToNew)
        {
            int Transaction_TypeAccountRelated = 2;
            //int Transaction_TypeAccountRelatedToNew = 1;
            operations opp = new operations(1);
            try
            {
                DateTime ProcessDate = EntryDate;
                long Checks_Memory_Id = chm.Id;
                long SubAccount_Id_RelatedFrom = (long)chm.SubAccount_Id_RelatedFrom;
                //انشاء القيد
                Account_Entry_Invoice_CLS aa = new Account_Entry_Invoice_CLS();
                //aa.Reference_Type = Common.Account_Type_Checks_Memory_Ward_Hafza;
                //aa.Reference_Type_Table_Id = Checks_Memory_Id;
                if (Excp == 0)
                {
                    Change_Check_Staus(ref Excp, ref opp, Receipt, chm);
                }
                
                
                aa.Make_EntryForChangeStatus_Checks_Memory(ref Excp, ref Msg, ref opp, EntryDate, Checks_Memory_Id, Transaction_TypeAccountRelated, SubAccount_Id_RelatedFromNew, SubAccount_Name_RelatedFromNew,   SubAccount_Id_RelatedToNew,   SubAccount_Name_RelatedToNew, Notes, true);
                
                if (Excp == 0)
                {
                    //Submain_AccountCLS saFrom = new Submain_AccountCLS();
                    //saFrom.Get_Submain_Account((long)chm.SubAccount_Id_RelatedFrom);
                    //int Account_Customer_Type_ValueFrom = saFrom.Account_Customer_Type_Value;
                    //long Account_Customer_Type_IDFrom = saFrom.Account_Customer_Type_ID;

                    

                    int Transaction_Type = 1;

                    //من الحساب الجديد
                    if (Account_Customer_Type_ValueFromNew == Common.Customer_Account_Type | Account_Customer_Type_ValueFromNew == Common.Supplier_Account_Type) // موردين // عملاء
                    {
                        Transaction_Type = 2;
                        long ProcessNo = 0; // عملية جديدة
                        SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, Transaction_Type, chm, Account_Customer_Type_ValueFromNew, Account_Customer_Type_IDNew, ProcessNo, ProcessDate, false);

                    }
                    ////الى الحساب الجديد
                    if (Account_Customer_Type_ValueToNew == Common.Customer_Account_Type | Account_Customer_Type_ValueToNew == Common.Supplier_Account_Type) // موردين // عملاء
                    {
                        Transaction_Type = 1;
                        long ProcessNo = 0; // عملية جديدة
                        SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, Transaction_Type, chm, Account_Customer_Type_ValueToNew, Account_Customer_Type_IDToNew, ProcessNo, ProcessDate, false);

                    }
                    //////من الحساب السابق
                    //if (Account_Customer_Type_ValueFrom == Common.Customer_Account_Type | Account_Customer_Type_ValueFrom == Common.Supplier_Account_Type) // موردين // عملاء
                    //{
                    //    Transaction_Type = 1;
                    //    long ProcessNo = 0; // عملية جديدة
                    //    SetSupplier_CustomerAccount(ref Excp, ref Msg, ref opp, Transaction_Type, chm, Account_Customer_Type_ValueFrom, Account_Customer_Type_IDFrom, ProcessNo, ProcessDate, false);

                    //}
                }

               
                if (Excp == 0)
                {
                    cc.Checks_MemoryHistory(ref Excp, ref Msg, ref opp, chm.Id);
                }

                if (Excp == 0)
                {
                    opp.Commit();
                }
                else
                {
                    opp.Rollback();
                }
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
                opp.Rollback();
            }

        }


         public void Change_Check_Staus(ref int Excp, ref operations opp, int Receipt, Checks_Memory cc)
        {
            int Statment_No = 0;
            long Check_Memory_Id = chm.Id;
            long SubAccount_Id_RelatedTo = (long)chm.SubAccount_Id_RelatedTo;
            string SubAccount_Name_RelatedTo = chm.SubAccount_Name_RelatedTo;
            string mysql = "  UPDATE Checks_Memory SET IsAllowEdit=0,IsAllowDelete=0,  Receipt =" + Receipt + ",SubAccount_Id_RelatedToBefor=" + SubAccount_Id_RelatedTo + ",SubAccount_Name_RelatedToBefor='" + SubAccount_Name_RelatedTo + "' WHERE (id = " + Check_Memory_Id + ")   ";

            Statment_No = opp.insert_me(mysql);
        }

        public long Make_New_Order_CheckNo(ref int Excp, int Transaction_type)
        {
            return or.Make_New_Order_CheckNo(ref Excp, Transaction_type);
        }
        public DataTable Get_Check_1(ref int Excp, int Transaction_Type, int Balance_BeginingPeriod)
        {

            string mysql = " ";
            try
            {
                mysql = @"   SELECT DISTINCT 
                         0 AS Serial_, '' AS SupNameTo, '' AS SubAccount_NameTO, '' AS SupNameFrom, '' AS SubAccount_NameFrom, Order_Money_Check.Order_No, Order_Money_Check.Order_Date, Order_Money_Check.Total_Value, 
                         Order_Money_Check.Currency_Id, Order_Money_Check.Bank_Type, Order_Money_Check.Man_Name, Checks_Memory.Order_Mony_Id, Checks_Memory.Check_No, Checks_Memory.Day_Date, Checks_Memory.Due_Date, 
                         Checks_Memory.Bank_id, Checks_Memory.Check_Value, Checks_Memory.Check_Value_, Checks_Memory.Customer_Id, Checks_Memory.Supplier_Id, Checks_Memory.Account_Id, Checks_Memory.Due_Check, 
                         Checks_Memory.Value_Receipt_Date, Checks_Memory.Notes, Checks_Memory.Exchanger_Name, Checks_Memory.First_Drawer_Name, Checks_Memory.Last_Drawer_Name, Checks_Memory.Bank_Balance, 
                         Checks_Memory.id AS Checks_Memory_id, Order_Money_Check.Id AS Order_Money_Check_Id, Checks_Memory.Transaction_Type, Checks_Memory.Receipt, Checks_Memory.SpentTo_Bank_Type, 
                         Checks_Memory.SpentTo_Bank_Id, Checks_Memory.Bank_Id_Under_Ta7seel, Checks_Memory.Spent_Date, Checks_Memory.Transfer_Check_Id, Checks_Memory.Balance_BeginingPeriod, LookUp_CheckStatus.StatusName, 
                         Checks_Memory.SubAccount_Id, Checks_Memory.SubAccount_Name, Checks_Memory.SubAccount_Id_RelatedTo, Checks_Memory.SubAccount_Name_RelatedTo, Checks_Memory.Bank_Name
FROM            Order_Money_Check INNER JOIN
                         Checks_Memory ON Order_Money_Check.Id = Checks_Memory.Order_Mony_Id INNER JOIN
                         LookUp_CheckStatus ON Checks_Memory.Receipt = LookUp_CheckStatus.S_Value
";
                mysql = mysql + " WHERE (Checks_Memory.Transaction_Type = " + Transaction_Type + ") And  (Checks_Memory.Balance_BeginingPeriod = " + Balance_BeginingPeriod + ")  ";

                mysql = mysql + "   ORDER BY Checks_Memory.Due_Date DESC ";
                operations opp = new operations();
                DataTable dt = opp.return_DataTable(mysql);
                opp = null;
                Excp = 0;

                Common.Make_TableColumnSerial(ref dt, "Serial_");
                return dt;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }


    }
}
