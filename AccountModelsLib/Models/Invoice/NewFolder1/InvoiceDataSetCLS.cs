using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace AccountModelsLib.Models
{
    public class InvoiceDataSetCLS
    {
        DataSet New_Items_ds;
        public InvoiceDataSetCLS()
        { }
        public DataTable MakeInvoiceDataTable()
        {
            MakeInvoiceDataSet();
            DataTable dt = New_Items_ds.Tables[0];
            return dt;
        }
        public DataSet MakeInvoiceDataSet()
        {
            this.New_Items_ds = new DataSet();
            DataTable table = new DataTable();
            this.New_Items_ds.Tables.Add(table);
            DataColumn id = new DataColumn("id", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(id);
            DataColumn Item_Name = new DataColumn("Item_Name", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_Name);
            DataColumn Specification = new DataColumn("Specification", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Specification);
            DataColumn ATS05 = new DataColumn("ATS05", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(ATS05);
            DataColumn ATS05_Value = new DataColumn("ATS05_Value", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(ATS05_Value);
            DataColumn TaxSales10 = new DataColumn("TaxSales10", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(TaxSales10);
            DataColumn TaxSales10_Value = new DataColumn("TaxSales10_Value", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(TaxSales10_Value);
            DataColumn Item_ATS05 = new DataColumn("Item_ATS05", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_ATS05);
            DataColumn Item_ATS05_Value = new DataColumn("Item_ATS05_Value", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_ATS05_Value);
            DataColumn Item_TaxSales10 = new DataColumn("Item_TaxSales10", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_TaxSales10);
            DataColumn Item_TaxSales10_Value = new DataColumn("Item_TaxSales10_Value", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_TaxSales10_Value);
            DataColumn Larg_Unit_Price = new DataColumn("Larg_Unit_Price", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Larg_Unit_Price);
            DataColumn Qnty_value = new DataColumn("Qnty_value", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Qnty_value);
            DataColumn Item_Net_Total_Price = new DataColumn("Item_Net_Total_Price", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_Net_Total_Price);
            DataColumn Net_Total_Price = new DataColumn("Net_Total_Price", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Net_Total_Price);
            DataColumn Total_PriceWithoutTax = new DataColumn("Total_PriceWithoutTax", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Total_PriceWithoutTax);
            DataColumn Item_Total_WithoutTax = new DataColumn("Item_Total_WithoutTax", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_Total_WithoutTax);


            DataColumn Item_Total = new DataColumn("Item_Total", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_Total);
            DataColumn Larg_Qnty = new DataColumn("Larg_Qnty", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Larg_Qnty);
            DataColumn Small_Qnty = new DataColumn("Small_Qnty", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Small_Qnty);
            DataColumn Qnty_Another = new DataColumn("Qnty_Another", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Qnty_Another);

            DataColumn Qnty = new DataColumn("Qnty", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Qnty);


            DataColumn PriceUnitIndex = new DataColumn("PriceUnitIndex", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(PriceUnitIndex);
            DataColumn Currency_id = new DataColumn("Currency_id", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Currency_id);
            DataColumn Item_id = new DataColumn("Item_id", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_id);
            DataColumn unit_id = new DataColumn("unit_id", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(unit_id);
            DataColumn B_Unit = new DataColumn("B_Unit", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(B_Unit);
            DataColumn S_Unit = new DataColumn("S_Unit", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(S_Unit);
            DataColumn Unit_Item_Table_Id = new DataColumn("Unit_Item_Table_Id", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Unit_Item_Table_Id);
            DataColumn Code = new DataColumn("Code", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Code);
            DataColumn Available_Larg_Balance = new DataColumn("Available_Larg_Balance", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Available_Larg_Balance);
            DataColumn Available_Small_Balance = new DataColumn("Available_Small_Balance", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Available_Small_Balance);
            DataColumn Make_Order = new DataColumn("Make_Order", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Make_Order);
            DataColumn Store_Id = new DataColumn("Store_Id", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Store_Id);
            DataColumn Store_Name = new DataColumn("Store_Name", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Store_Name);
            DataColumn Store_Code = new DataColumn("Store_Code", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Store_Code);
            DataColumn Small_Unit_Value = new DataColumn("Small_Unit_Value", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Small_Unit_Value);


            DataColumn Item_CommissionValue = new DataColumn("Item_CommissionValue", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_CommissionValue);
            DataColumn Is_Item_CommissionValue = new DataColumn("Is_Item_CommissionValue", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Is_Item_CommissionValue);
            DataColumn Unit_CommissionValue = new DataColumn("Unit_CommissionValue", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Unit_CommissionValue);

            DataColumn Alternate_ItemCommissionTotal = new DataColumn("Alternate_ItemCommissionTotal", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Alternate_ItemCommissionTotal);



            DataColumn Net_Total_Item_Commission = new DataColumn("Net_Total_Item_Commission", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Net_Total_Item_Commission);



            DataColumn QntyWidth = new DataColumn("QntyWidth", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(QntyWidth);
            DataColumn QntyLenght = new DataColumn("QntyLenght", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(QntyLenght);
            DataColumn QntyItemNo = new DataColumn("QntyItemNo", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(QntyItemNo);
            DataColumn Somk_Unit = new DataColumn("Somk_Unit", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Somk_Unit);
            DataColumn Items_Types_Name = new DataColumn("Items_Types_Name", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Items_Types_Name);
            DataColumn Items_Types_Name_Id = new DataColumn("Items_Types_Name_Id", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Items_Types_Name_Id);
            DataColumn Somk = new DataColumn("Somk", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Somk);
            DataColumn Is_InvoiceItem_Kta3y = new DataColumn("Is_InvoiceItem_Kta3y", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Is_InvoiceItem_Kta3y);
            DataColumn Bills_Details_Id = new DataColumn("Bills_Details_Id", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Bills_Details_Id);
            DataColumn Item_Expire_Date = new DataColumn("Item_Expire_Date", Type.GetType("System.DateTime"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_Expire_Date);
            DataColumn Audience_price = new DataColumn("Audience_price", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Audience_price);
            DataColumn Purchase_Price = new DataColumn("Purchase_Price", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Purchase_Price);
            DataColumn Is_Buy_From_Bills_Items_Balance = new DataColumn("Is_Buy_From_Bills_Items_Balance", Type.GetType("System.Int32"));
            this.New_Items_ds.Tables[0].Columns.Add(Is_Buy_From_Bills_Items_Balance);
            DataColumn StatementNo = new DataColumn("StatementNo", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(StatementNo);
            DataColumn OperationNo = new DataColumn("OperationNo", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(OperationNo);
            DataColumn Merchant_Name = new DataColumn("Merchant_Name", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Merchant_Name);
            DataColumn Tash3ela = new DataColumn("Tash3ela", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Tash3ela);
            DataColumn LargItemsQnty_Free = new DataColumn("LargItemsQnty_Free", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(LargItemsQnty_Free);
            DataColumn SmallItemsQnty_Free = new DataColumn("SmallItemsQnty_Free", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(SmallItemsQnty_Free);
            DataColumn Item_No_Free = new DataColumn("Item_No_Free", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_No_Free);
            DataColumn Item_No = new DataColumn("Item_No", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_No);
            DataColumn Is_Alternate_Item_CommissionValue = new DataColumn("Is_Alternate_Item_CommissionValue", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Is_Alternate_Item_CommissionValue);
            DataColumn Alternate_Item_CommissionValue = new DataColumn("Alternate_Item_CommissionValue", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Alternate_Item_CommissionValue);
            DataColumn Alternate_Item_CommissionRate = new DataColumn("Alternate_Item_CommissionRate", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Alternate_Item_CommissionRate);
            DataColumn Item_CommissionRate = new DataColumn("Item_CommissionRate", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_CommissionRate);
            DataColumn Unit_CommissionRate = new DataColumn("Unit_CommissionRate", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Unit_CommissionRate);
            DataColumn Alternate_Unit_CommissionRate = new DataColumn("Alternate_Unit_CommissionRate", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Alternate_Unit_CommissionRate);
            DataColumn Alternate_Unit_CommissionValue = new DataColumn("Alternate_Unit_CommissionValue", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(Alternate_Unit_CommissionValue);
            DataColumn ItemCommissionTotal = new DataColumn("ItemCommissionTotal", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(ItemCommissionTotal);
            DataColumn Item_DeletedFlage = new DataColumn("Item_DeletedFlage", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_DeletedFlage);
            DataColumn Item_ProcessedFlage = new DataColumn("Item_ProcessedFlage", Type.GetType("System.String"));
            this.New_Items_ds.Tables[0].Columns.Add(Item_ProcessedFlage);
            DataColumn Unit_Used_for_Price = new DataColumn("Unit_Used_for_Price", Type.GetType("System.Int16"));
            this.New_Items_ds.Tables[0].Columns.Add(Unit_Used_for_Price);
            DataColumn Invoices_Details_Id = new DataColumn("Invoices_Details_Id", Type.GetType("System.Int32"));
            this.New_Items_ds.Tables[0].Columns.Add(Invoices_Details_Id);
            DataColumn Customer_Id = new DataColumn("Customer_Id", Type.GetType("System.Int32"));
            this.New_Items_ds.Tables[0].Columns.Add(Customer_Id);
            DataColumn Unit_Item_ExpireDate_Id = new DataColumn("Unit_Item_ExpireDate_Id", Type.GetType("System.Int32"));
            this.New_Items_ds.Tables[0].Columns.Add(Unit_Item_ExpireDate_Id);

            DataColumn QntyStr = new DataColumn("QntyStr", Type.GetType("System.String"));
            New_Items_ds.Tables[0].Columns.Add(QntyStr);

            DataColumn Item_Notes = new DataColumn("Item_Notes", Type.GetType("System.String"));
            New_Items_ds.Tables[0].Columns.Add(Item_Notes);

            DataColumn Balance_Checked = new DataColumn("Balance_Checked", Type.GetType("System.Int32"));
            this.New_Items_ds.Tables[0].Columns.Add(Balance_Checked);
            DataColumn BanksNamePaymentMethod = new DataColumn("BanksNamePaymentMethod", Type.GetType("System.String"));
            New_Items_ds.Tables[0].Columns.Add(BanksNamePaymentMethod);

            DataColumn BanksId = new DataColumn("BanksId", Type.GetType("System.Int32"));
            this.New_Items_ds.Tables[0].Columns.Add(BanksId);
            DataColumn Order_MoneyId = new DataColumn("Order_MoneyId", Type.GetType("System.Int32"));
            this.New_Items_ds.Tables[0].Columns.Add(Order_MoneyId);


            DataColumn PaidValue = new DataColumn("PaidValue", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(PaidValue);

            DataColumn RestValue = new DataColumn("RestValue", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(RestValue);

            DataColumn NameSpecification = new DataColumn("NameSpecification", Type.GetType("System.String"));
            New_Items_ds.Tables[0].Columns.Add(NameSpecification);
            DataColumn QntyStrColumn = new DataColumn("QntyStrColumn", Type.GetType("System.String"));
            New_Items_ds.Tables[0].Columns.Add(QntyStrColumn);

            DataColumn TotalSmall_Qnty = new DataColumn("TotalSmall_Qnty", Type.GetType("System.Decimal"));
            this.New_Items_ds.Tables[0].Columns.Add(TotalSmall_Qnty);

            DataColumn TotalSmall_QntyStr = new DataColumn("TotalSmall_QntyStr", Type.GetType("System.String"));
            New_Items_ds.Tables[0].Columns.Add(TotalSmall_QntyStr);

            DataColumn TotalSmall_QntyStrColumn = new DataColumn("TotalSmall_QntyStrColumn", Type.GetType("System.String"));
            New_Items_ds.Tables[0].Columns.Add(TotalSmall_QntyStrColumn);


            return this.New_Items_ds;
        }

        //public void Set_ItemTransactionFromInvoices1(long Unit_item_Table_Id, decimal Net_Total, decimal qua, decimal Part_qua, decimal Item_No, long Machine_Id, int Transaction_Type, DateTime Process_Date, long Invoice_Id, long Detail_Invoice_Id, int Invoice_Type_Id, string Notes, string Notes_To, string Notes_From, string Notes_Item, long Invoice_No)
        //{
        //    operations opp = new operations();
        //    Set_ItemTransactionFromInvoices(ref opp, Unit_item_Table_Id,   Net_Total,   qua,   Part_qua,   Item_No,   Machine_Id,   Transaction_Type,   Process_Date,   Invoice_Id,   Detail_Invoice_Id,   Invoice_Type_Id,   Notes,   Notes_To,   Notes_From,   Notes_Item,   Invoice_No);
        //}
        public void Set_ItemTransactionFromInvoices1(ref operations opp, long Unit_item_Table_Id, decimal Total_Value, decimal qua, decimal Part_qua, decimal Item_No, long Machine_Id, int Transaction_Type, DateTime Process_Date, long Invoice_Id, long Detail_Invoice_Id, int Invoice_Type_Id, string Notes, string Notes_To, string Notes_From, string Notes_Item, long Invoice_No)
        {

            //decimal Total_Value
            //Invoice_No

            this.Delete_ItemTransactionFromInvoices(ref opp, Unit_item_Table_Id, Invoice_Id, Detail_Invoice_Id, Machine_Id, Invoice_Type_Id);


            string mysql = "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =" + Unit_item_Table_Id + " And Invoice_Id=" + Invoice_Id + " And  Detail_Invoice_Id=" + Detail_Invoice_Id + " AND Invoice_Type_Id=" + Invoice_Type_Id + "";
            opp.insert_me(mysql);

            mysql = "Select Small_unit from Unit_item Where Id=" + Unit_item_Table_Id;
            DataTable dtt = opp.return_DataTable(mysql);
            decimal Small_unit = 1;
            try
            {
                Small_unit = decimal.Parse(dtt.Rows[0]["Small_unit"].ToString());
            }
            catch { }
            long Detailes_Purshing_Id = 0;
            long OrderTable_id = 0;
            decimal Balance_Larg_Qnty = 0;
            decimal Balance_Small_Qnty = 0;
            decimal Balance_Item_No = 0;




            string Notes_From_To = Notes_From + " " + Notes_To;
            decimal Quantity = Common.Get_Item_Quantity(qua, Part_qua, Small_unit); ;


            int IS_To_Converted = 0;
            decimal qua_Import = 0;
            decimal Part_Qua_Import = 0;
            decimal Item_No_Import = 0;
            decimal Quantity_Import = 0;
            decimal qua_Export = 0;
            decimal Part_Qua_Export = 0;
            decimal Item_No_Export = 0;
            decimal Quantity_Export = 0;

            decimal Quantity_Balance = 0;


            if (Transaction_Type == 1)
            {
                qua_Import = qua;
                Part_Qua_Import = Part_qua;
                Item_No_Import = Item_No;
                Quantity_Import = Quantity;
            }
            else
            {
                qua_Export = qua;
                Part_Qua_Export = Part_qua;
                Item_No_Export = Item_No;
                Quantity_Export = Quantity;
            }



            mysql = "INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Notes_From_To, Notes, Machine_Id, Transaction_Type,Process_Date, Notes_To, Notes_From, IS_To_Converted, qua_Import, Part_Qua_Import, Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Quantity,Quantity_Balance,  Invoice_Id, Detail_Invoice_Id,  Invoice_Type_Id,Notes_Item,User_Id,OperationDate) ";
            mysql = mysql + " VALUES (@Detailes_Purshing_Id, @OrderTable_id, @Unit_item_Table_Id, @qua, @Part_qua, @Item_No, @Balance_Larg_Qnty, @Balance_Small_Qnty, @Balance_Item_No, @Notes_From_To, @Notes, @Machine_Id, @Transaction_Type,@Process_Date, @Notes_To, @Notes_From, @IS_To_Converted, @qua_Import, @Part_Qua_Import, @Item_No_Import, @Quantity_Import, @qua_Export, @Part_Qua_Export, @Item_No_Export, @Quantity_Export, @Quantity,@Quantity_Balance,  @Invoice_Id,  @Detail_Invoice_Id, @Invoice_Type_Id,@Notes_Item,@user_Id,@OperationDate)  ";




            opp.cmd.Parameters.AddWithValue("@Detailes_Purshing_Id", Detailes_Purshing_Id);
            opp.cmd.Parameters.AddWithValue("@OrderTable_id", OrderTable_id);
            opp.cmd.Parameters.AddWithValue("@Unit_item_Table_Id", Unit_item_Table_Id);
            opp.cmd.Parameters.AddWithValue("@qua", qua);
            opp.cmd.Parameters.AddWithValue("@Part_qua", Part_qua);
            opp.cmd.Parameters.AddWithValue("@Item_No", Item_No);
            opp.cmd.Parameters.AddWithValue("@Balance_Larg_Qnty", Balance_Larg_Qnty);
            opp.cmd.Parameters.AddWithValue("@Balance_Small_Qnty", Balance_Small_Qnty);
            opp.cmd.Parameters.AddWithValue("@Balance_Item_No", Balance_Item_No);
            opp.cmd.Parameters.AddWithValue("@Notes_From_To", Notes_From_To);
            opp.cmd.Parameters.AddWithValue("@Notes", Notes);
            opp.cmd.Parameters.AddWithValue("@Machine_Id", Machine_Id);
            opp.cmd.Parameters.AddWithValue("@Transaction_Type", Transaction_Type);
            opp.cmd.Parameters.AddWithValue("@Process_Date", Process_Date);
            opp.cmd.Parameters.AddWithValue("@Notes_To", Notes_To);
            opp.cmd.Parameters.AddWithValue("@Notes_From", Notes_From);
            opp.cmd.Parameters.AddWithValue("@IS_To_Converted", IS_To_Converted);
            opp.cmd.Parameters.AddWithValue("@qua_Import", qua_Import);
            opp.cmd.Parameters.AddWithValue("@Part_Qua_Import", Part_Qua_Import);
            opp.cmd.Parameters.AddWithValue("@Item_No_Import", Item_No_Import);
            opp.cmd.Parameters.AddWithValue("@Quantity_Import", Quantity_Import);


            opp.cmd.Parameters.AddWithValue("@qua_Export", qua_Export);
            opp.cmd.Parameters.AddWithValue("@Part_Qua_Export", Part_Qua_Export);
            opp.cmd.Parameters.AddWithValue("@Item_No_Export", Item_No_Export);
            opp.cmd.Parameters.AddWithValue("@Quantity_Export", Quantity_Export);
            opp.cmd.Parameters.AddWithValue("@Quantity", Quantity);
            opp.cmd.Parameters.AddWithValue("@Quantity_Balance", Quantity_Balance);
            opp.cmd.Parameters.AddWithValue("@Invoice_Id", Invoice_Id);
            opp.cmd.Parameters.AddWithValue("@Invoice_Type_Id", Invoice_Type_Id);
            opp.cmd.Parameters.AddWithValue("@Detail_Invoice_Id", Detail_Invoice_Id);
            opp.cmd.Parameters.AddWithValue("@Notes_Item", Notes_Item);

            opp.cmd.Parameters.AddWithValue("@user_Id", Common.User_Id);
            opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now);
            opp.cmd.Parameters.AddWithValue("@Total_Value", Total_Value);
            opp.cmd.Parameters.AddWithValue("@Invoice_No", Invoice_No);

            opp.insert_me(mysql);
            opp.cmd.Parameters.Clear();

        }

        public void Set_ItemTransactionFromInvoices(long Unit_item_Table_Id,decimal PriceUnit, decimal Net_Total, decimal qua, decimal Part_qua, decimal Item_No, long Machine_Id, int Transaction_Type, DateTime Process_Date, long Invoice_Id, long Detail_Invoice_Id, int Invoice_Type_Id, string Notes, string Notes_To, string Notes_From, string Notes_Item, long Invoice_No, int Type_From = 0, int Type_To = 0, long Id_From = 0, long Id_To = 0, long StoreId = 0, string StoreId_Name = "")
        {
            operations opp = new operations();
            this.Set_ItemTransactionFromInvoices(ref opp, Unit_item_Table_Id, PriceUnit, Net_Total, qua, Part_qua, Item_No, Machine_Id, Transaction_Type, Process_Date, Invoice_Id, Detail_Invoice_Id, Invoice_Type_Id, Notes, Notes_To, Notes_From, Notes_Item, Invoice_No, Type_From, Type_To, Id_From, Id_To, StoreId, StoreId_Name);
        }
        public void Set_ItemTransactionFromBill(ref operations opp, BillOperationCLS invd, BillOperationListCLS invL, long Machine_Id, int Transaction_Type, int ProcessNameId, string ProcessName)
        {
            Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();

             
            if (ProcessNameId == Common.Bill_Type_Id)
            {
                die.Type_From = Common.Supplier_StoreTransactionCode;
                die.Type_To = Common.Store_StoreTransactionCode;

                die.Type_From_Name = "„Ê—œ"; 
                die.Type_To_Name =  "„Œ“‰";

                die.Id_From = invL.Supplier_Id;
                die.Id_To = invd.Store_Id;

                die.Id_FromStr = invL.Supplier_Name ;
                die.Id_ToStr = invd.Store_Name;
            }
            if (ProcessNameId == Common.BilleReverse_Type_Id)
            {
                die.Type_From = Common.Store_StoreTransactionCode;
                die.Type_To = Common.Supplier_StoreTransactionCode;
                die.Type_From_Name = "„Œ“‰";
                die.Type_To_Name = "„Ê—œ";

                die.Id_From = invd.Store_Id;
                die.Id_To = invL.Supplier_Id;

                die.Id_FromStr = invd.Store_Name;
                die.Id_ToStr = invL.Supplier_Name;
            }

            //long StoreId = invd.Store_Id;
            //string StoreId_Name = invd.Store_Name;
             
            //long Invoice_No = invL.Invoice_No;


            this.Delete_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invL.Bills_List_Id, invd.Bills_Details_Id, Machine_Id, invL.Invoice_Type_Id);
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", invd.Unit_Item_Table_Id.ToString(), " And Invoice_Id=", invL.Bills_List_Id.ToString(), " And  Detail_Invoice_Id=", invd.Bills_Details_Id.ToString(), " AND Invoice_Type_Id=", ProcessNameId.ToString() };
            string str = string.Concat(textArray1);
            opp.insert_me(str);
            str = "Select Small_unit from Unit_item Where Id=" + invd.Unit_Item_Table_Id.ToString();
            DataTable table = opp.return_DataTable(str);
            decimal Small_unit = 1M;
            try
            {
                Small_unit = decimal.Parse(table.Rows[0]["Small_unit"].ToString());
            }
            catch
            {
            }
            //long Detailes_Purshing_Id = 0;
            //long OrderTable_id = 0;


            decimal Quantity = Common.Get_Item_Quantity(invd.Larg_Qnty, invd.Small_Qnty, Small_unit);
            int IS_To_Converted = 0;



           
            if (Transaction_Type == 1)
            {


                die.qua_Import = invd.Larg_Qnty;
                die.Part_Qua_Import = invd.Small_Qnty; ;
                die.Item_No_Import = invd.Qnty_Another; ;
                die.Quantity_Import = Quantity;
                
                die.qua_Export = 0;
                die.Part_Qua_Export = 0;
                die.Item_No_Export = 0;
                die.Quantity_Export = 0;


                die.PriceUnitImport = invd.PriceUnit;
                die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

                die.PriceUnitExport = 0;
                die.Item_Net_Total_PriceExport = 0;
            }
            else
            {
                die.qua_Import = 0;
                die.Part_Qua_Import = 0; ;
                die.Item_No_Import = 0; ;
                die.Quantity_Import = 0;


                die.qua_Export = invd.Larg_Qnty; ;
                die.Part_Qua_Export = invd.Small_Qnty;
                die.Item_No_Export = invd.Qnty_Another;
                die.Quantity_Export = Quantity;


                die.PriceUnitImport = 0;
                die.Item_Net_Total_PriceImport = 0;

                die.PriceUnitExport = invd.PriceUnit;
                die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;

            }



            //die.PriceUnitImport = invd.PriceUnit;
            //die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

            //die.PriceUnitExport = invd.PriceUnit;
            //die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;


            die.PriceUnit = invd.PriceUnit;
            die.Item_Net_Total_Price = invd.Item_Net_Total_Price;
            die.Qnty_value = invd.Qnty_value;
             
            die.S_Price = 0;
            die.P_Price = 0; 
            die.P_priceAvgCost = 0;
            die.S_priceAvgCost = 0;
            /////////////////////////////
            ///

            die.StoreId_Name = invd.Store_Name;
            die.StoreId = invd.Store_Id; 

            die.ProcessNameId = ProcessNameId;
            die.ProcessName = ProcessName;
            die.ProcessNameNumber = invL.Invoice_No.ToString();
            die.Invoice_Type_Str = ProcessName;

            die.Notes_To = die.Id_ToStr;
            die.Notes_From = die.Id_FromStr;
            die.Notes_Item = invd.Item_Notes + " " + invL.Supplier_Name;
             


            die.Invoice_Id = invL.Bills_List_Id;
            die.Invoice_Type_Id = invL.Invoice_Type_Id;
            die.Detail_Invoice_Id = invd.Bills_Details_Id;
            die.Invoice_No = invL.Invoice_No;
            die.Detailes_Purshing_Id = 0;
            die.OrderTable_id = 0;


            die.Notes_From_To = " „‰ " + die.Id_FromStr + " «·Ï " + die.Id_ToStr;


            die.Notes = invL.Notes; 

            die.Unit_item_Table_Id = invd.Unit_Item_Table_Id;
            die.qua = invd.Larg_Qnty;
            die.Part_qua = invd.Small_Qnty;
            die.Item_No = invd.Qnty_Another;
            die.Balance_Larg_Qnty = 0;
            die.Balance_Small_Qnty = 0;
            die.Balance_Item_No = 0;

            die.Machine_Id = Machine_Id;
            die.Transaction_Type = Transaction_Type;
            die.Process_Date = invL.Invoice_Date;

            die.IS_To_Converted = IS_To_Converted;

            die.Quantity = Quantity;
            die.Quantity_Balance = 0;

           
           

            die.User_Id = Common.User_Id;
            die.Branch_Id = Common.Branch_Id;
            die.ProcessNo = 0;
            die.CarNumber = "";
            die.CarsId = 0;
            die.CarsWorkOrdersDetailsId = 0;
            die.CarsWorkOrdersId = 0;
            die.WorkOrderNumber = "";
            die.OperationDate = DateTime.Now;

            Detailes_Purshing_Import_ExportCLS dieCls = new Detailes_Purshing_Import_ExportCLS();
            dieCls.Add(ref opp, die);


            opp.cmd.Parameters.Clear();
        }
        public void Set_ItemTransactionFromBill(ref operations opp, BillOperationCLS_New invd, BillOperationListCLS_New invL, long Machine_Id, int Transaction_Type, int ProcessNameId, string ProcessName)
        {
            //Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();


            //if (ProcessNameId == Common.Bill_Type_Id)
            //{
            //    die.Type_From = Common.Supplier_StoreTransactionCode;
            //    die.Type_To = Common.Store_StoreTransactionCode;

            //    die.Type_From_Name = "„Ê—œ";
            //    die.Type_To_Name = "„Œ“‰";

            //    die.Id_From = invL.Supplier_Id;
            //    die.Id_To = invd.Store_Id;

            //    die.Id_FromStr = invL.Supplier_Name;
            //    die.Id_ToStr = invd.Store_Name;
            //}
            //if (ProcessNameId == Common.BilleReverse_Type_Id)
            //{
            //    die.Type_From = Common.Store_StoreTransactionCode;
            //    die.Type_To = Common.Supplier_StoreTransactionCode;
            //    die.Type_From_Name = "„Œ“‰";
            //    die.Type_To_Name = "„Ê—œ";

            //    die.Id_From = invd.Store_Id;
            //    die.Id_To = invL.Supplier_Id;

            //    die.Id_FromStr = invd.Store_Name;
            //    die.Id_ToStr = invL.Supplier_Name;
            //}

            ////long StoreId = invd.Store_Id;
            ////string StoreId_Name = invd.Store_Name;

            ////long Invoice_No = invL.Invoice_No;


            //this.Delete_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id, invL.Bills_List_Id, invd.Bills_Details_Id, Machine_Id, invL.Invoice_Type_Id);
            //string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", invd.Unit_Item_Table_Id.ToString(), " And Invoice_Id=", invL.Bills_List_Id.ToString(), " And  Detail_Invoice_Id=", invd.Bills_Details_Id.ToString(), " AND Invoice_Type_Id=", ProcessNameId.ToString() };
            //string str = string.Concat(textArray1);
            //opp.insert_me(str);
            //str = "Select Small_unit from Unit_item Where Id=" + invd.Unit_Item_Table_Id.ToString();
            //DataTable table = opp.return_DataTable(str);
            //decimal Small_unit = 1M;
            //try
            //{
            //    Small_unit = decimal.Parse(table.Rows[0]["Small_unit"].ToString());
            //}
            //catch
            //{
            //}
            ////long Detailes_Purshing_Id = 0;
            ////long OrderTable_id = 0;


            //decimal Quantity = Common.Get_Item_Quantity(invd.Larg_Qnty, invd.Small_Qnty, Small_unit);
            //int IS_To_Converted = 0;




            //if (Transaction_Type == 1)
            //{


            //    die.qua_Import = invd.Larg_Qnty;
            //    die.Part_Qua_Import = invd.Small_Qnty; ;
            //    die.Item_No_Import = invd.Qnty_Another; ;
            //    die.Quantity_Import = Quantity;

            //    die.qua_Export = 0;
            //    die.Part_Qua_Export = 0;
            //    die.Item_No_Export = 0;
            //    die.Quantity_Export = 0;


            //    die.PriceUnitImport = invd.PriceUnit;
            //    die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

            //    die.PriceUnitExport = 0;
            //    die.Item_Net_Total_PriceExport = 0;
            //}
            //else
            //{
            //    die.qua_Import = 0;
            //    die.Part_Qua_Import = 0; ;
            //    die.Item_No_Import = 0; ;
            //    die.Quantity_Import = 0;


            //    die.qua_Export = invd.Larg_Qnty; ;
            //    die.Part_Qua_Export = invd.Small_Qnty;
            //    die.Item_No_Export = invd.Qnty_Another;
            //    die.Quantity_Export = Quantity;


            //    die.PriceUnitImport = 0;
            //    die.Item_Net_Total_PriceImport = 0;

            //    die.PriceUnitExport = invd.PriceUnit;
            //    die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;

            //}



            ////die.PriceUnitImport = invd.PriceUnit;
            ////die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

            ////die.PriceUnitExport = invd.PriceUnit;
            ////die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;


            //die.PriceUnit = invd.PriceUnit;
            //die.Item_Net_Total_Price = invd.Item_Net_Total_Price;
            //die.Qnty_value = invd.Qnty_value;

            //die.S_Price = 0;
            //die.P_Price = 0;
            //die.P_priceAvgCost = 0;
            //die.S_priceAvgCost = 0;
            ///////////////////////////////
            /////

            //die.StoreId_Name = invd.Store_Name;
            //die.StoreId = invd.Store_Id;

            //die.ProcessNameId = ProcessNameId;
            //die.ProcessName = ProcessName;
            //die.ProcessNameNumber = invL.Invoice_No.ToString();
            //die.Invoice_Type_Str = ProcessName;

            //die.Notes_To = die.Id_ToStr;
            //die.Notes_From = die.Id_FromStr;
            //die.Notes_Item = invd.Item_Notes + " " + invL.Supplier_Name;



            //die.Invoice_Id = invL.Bills_List_Id;
            //die.Invoice_Type_Id = invL.Invoice_Type_Id;
            //die.Detail_Invoice_Id = invd.Bills_Details_Id;
            //die.Invoice_No = invL.Invoice_No;
            //die.Detailes_Purshing_Id = 0;
            //die.OrderTable_id = 0;


            //die.Notes_From_To = " „‰ " + die.Id_FromStr + " «·Ï " + die.Id_ToStr;


            //die.Notes = invL.Notes;

            //die.Unit_item_Table_Id = invd.Unit_Item_Table_Id;
            //die.qua = invd.Larg_Qnty;
            //die.Part_qua = invd.Small_Qnty;
            //die.Item_No = invd.Qnty_Another;
            //die.Balance_Larg_Qnty = 0;
            //die.Balance_Small_Qnty = 0;
            //die.Balance_Item_No = 0;

            //die.Machine_Id = Machine_Id;
            //die.Transaction_Type = Transaction_Type;
            //die.Process_Date = invL.Invoice_Date;

            //die.IS_To_Converted = IS_To_Converted;

            //die.Quantity = Quantity;
            //die.Quantity_Balance = 0;




            //die.User_Id = Common.User_Id;
            //die.Branch_Id = Common.Branch_Id;
            //die.ProcessNo = 0;
            //die.CarNumber = "";
            //die.CarsId = 0;
            //die.CarsWorkOrdersDetailsId = 0;
            //die.CarsWorkOrdersId = 0;
            //die.WorkOrderNumber = "";
            //die.OperationDate = DateTime.Now;

            //Detailes_Purshing_Import_ExportCLS dieCls = new Detailes_Purshing_Import_ExportCLS();
            //dieCls.Add(ref opp, die);


            //opp.cmd.Parameters.Clear();
        }
        public void Set_ItemTransactionFromBill(ref operations opp, IBills_Details  invd, BillOperationListCLS invL, long Machine_Id, int Transaction_Type, int ProcessNameId, string ProcessName)
        {
            Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();


            if (ProcessNameId == Common.Bill_Type_Id)
            {
                die.Type_From = Common.Supplier_StoreTransactionCode;
                die.Type_To = Common.Store_StoreTransactionCode;

                die.Type_From_Name = "„Ê—œ";
                die.Type_To_Name = "„Œ“‰";

                die.Id_From = invL.Supplier_Id;
                die.Id_To = invd.Store_Id;

                die.Id_FromStr = invL.supplier.Name;
                die.Id_ToStr = invd.Store_Name;
            }
            if (ProcessNameId == Common.BilleReverse_Type_Id)
            {
                die.Type_From = Common.Store_StoreTransactionCode;
                die.Type_To = Common.Supplier_StoreTransactionCode;
                die.Type_From_Name = "„Œ“‰";
                die.Type_To_Name = "„Ê—œ";

                die.Id_From = invd.Store_Id;
                die.Id_To = invL.Supplier_Id;

                die.Id_FromStr = invd.Store_Name;
                die.Id_ToStr = invL.supplier.Name;
            }

            //long StoreId = invd.Store_Id;
            //string StoreId_Name = invd.Store_Name;

            //long Invoice_No = invL.Invoice_No;


            this.Delete_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id??0, invL.Bills_List_Id, invd.Id, Machine_Id, invL.Invoice_Type_Id);
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", invd.Unit_Item_Table_Id.ToString(), " And Invoice_Id=", invL.Bills_List_Id.ToString(), " And  Detail_Invoice_Id=", invd.Id.ToString(), " AND Invoice_Type_Id=", ProcessNameId.ToString() };
            string str = string.Concat(textArray1);
            opp.insert_me(str);
            str = "Select Small_unit from Unit_item Where Id=" + invd.Unit_Item_Table_Id.ToString();
            DataTable table = opp.return_DataTable(str);
            decimal Small_unit = 1M;
            try
            {
                Small_unit = decimal.Parse(table.Rows[0]["Small_unit"].ToString());
            }
            catch
            {
            }
            //long Detailes_Purshing_Id = 0;
            //long OrderTable_id = 0;


            decimal Quantity = Common.Get_Item_Quantity(invd.Larg_Qnty??0, invd.Small_Qnty??0, Small_unit);
            int IS_To_Converted = 0;




            if (Transaction_Type == 1)
            {


                die.qua_Import = invd.Larg_Qnty;
                die.Part_Qua_Import = invd.Small_Qnty; ;
                die.Item_No_Import = invd.Qnty_Another; ;
                die.Quantity_Import = Quantity;

                die.qua_Export = 0;
                die.Part_Qua_Export = 0;
                die.Item_No_Export = 0;
                die.Quantity_Export = 0;


                die.PriceUnitImport = invd.PriceUnit;
                die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

                die.PriceUnitExport = 0;
                die.Item_Net_Total_PriceExport = 0;
            }
            else
            {
                die.qua_Import = 0;
                die.Part_Qua_Import = 0; ;
                die.Item_No_Import = 0; ;
                die.Quantity_Import = 0;


                die.qua_Export = invd.Larg_Qnty; ;
                die.Part_Qua_Export = invd.Small_Qnty;
                die.Item_No_Export = invd.Qnty_Another;
                die.Quantity_Export = Quantity;


                die.PriceUnitImport = 0;
                die.Item_Net_Total_PriceImport = 0;

                die.PriceUnitExport = invd.PriceUnit;
                die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;

            }



            //die.PriceUnitImport = invd.PriceUnit;
            //die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

            //die.PriceUnitExport = invd.PriceUnit;
            //die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;


            die.PriceUnit = invd.PriceUnit;
            die.Item_Net_Total_Price = invd.Item_Net_Total_Price;
            die.Qnty_value = invd.Qnty_value;

            die.S_Price = 0;
            die.P_Price = 0;
            die.P_priceAvgCost = 0;
            die.S_priceAvgCost = 0;
            /////////////////////////////
            ///

            die.StoreId_Name = invd.Store_Name;
            die.StoreId = invd.Store_Id;

            die.ProcessNameId = ProcessNameId;
            die.ProcessName = ProcessName;
            die.ProcessNameNumber = invL.Invoice_No.ToString();
            die.Invoice_Type_Str = ProcessName;

            die.Notes_To = die.Id_ToStr;
            die.Notes_From = die.Id_FromStr;
            die.Notes_Item = invd.Item_Notes + " " + invL.Supplier_Name;



            die.Invoice_Id = invL.Bills_List_Id;
            die.Invoice_Type_Id = invL.Invoice_Type_Id;
            die.Detail_Invoice_Id = invd.Id;
            die.Invoice_No = invL.Invoice_No;
            die.Detailes_Purshing_Id = 0;
            die.OrderTable_id = 0;


            die.Notes_From_To = " „‰ " + die.Id_FromStr + " «·Ï " + die.Id_ToStr;


            die.Notes = invL.Notes;

            die.Unit_item_Table_Id = invd.Unit_Item_Table_Id ?? 0;
            die.qua = invd.Larg_Qnty;
            die.Part_qua = invd.Small_Qnty;
            die.Item_No = invd.Qnty_Another;
            die.Balance_Larg_Qnty = 0;
            die.Balance_Small_Qnty = 0;
            die.Balance_Item_No = 0;

            die.Machine_Id = Machine_Id;
            die.Transaction_Type = Transaction_Type;
            die.Process_Date = invL.Invoice_Date;

            die.IS_To_Converted = IS_To_Converted;

            die.Quantity = Quantity;
            die.Quantity_Balance = 0;




            die.User_Id = Common.User_Id;
            die.Branch_Id = Common.Branch_Id;
            die.ProcessNo = 0;
            die.CarNumber = "";
            die.CarsId = 0;
            die.CarsWorkOrdersDetailsId = 0;
            die.CarsWorkOrdersId = 0;
            die.WorkOrderNumber = "";
            die.OperationDate = DateTime.Now;

            Detailes_Purshing_Import_ExportCLS dieCls = new Detailes_Purshing_Import_ExportCLS();
            dieCls.Add(ref opp, die);


            opp.cmd.Parameters.Clear();
        }
        public void Set_ItemTransactionFromBill(ref operations opp, IBills_Details invd, BillOperationListCLS_New invL_O, long Machine_Id, int Transaction_Type, int ProcessNameId, string ProcessName)
        {
            Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();


            if (ProcessNameId == Common.Bill_Type_Id)
            {
                die.Type_From = Common.Supplier_StoreTransactionCode;
                die.Type_To = Common.Store_StoreTransactionCode;

                die.Type_From_Name = "„Ê—œ";
                die.Type_To_Name = "„Œ“‰";

                die.Id_From = invL_O.invL.Supplier_Id;
                die.Id_To = invd.Store_Id;

                die.Id_FromStr = invL_O.invL.SubAccount_Name;
                die.Id_ToStr = invd.Store_Name;
            }
            if (ProcessNameId == Common.BilleReverse_Type_Id)
            {
                die.Type_From = Common.Store_StoreTransactionCode;
                die.Type_To = Common.Supplier_StoreTransactionCode;
                die.Type_From_Name = "„Œ“‰";
                die.Type_To_Name = "„Ê—œ";

                die.Id_From = invd.Store_Id;
                die.Id_To = invL_O.invL.Supplier_Id;

                die.Id_FromStr = invd.Store_Name;
                die.Id_ToStr = invL_O.invL.SubAccount_Name;
            }

            //long StoreId = invd.Store_Id;
            //string StoreId_Name = invd.Store_Name;

            //long Invoice_No = invL.Invoice_No;


            this.Delete_ItemTransactionFromInvoices(ref opp, invd.Unit_Item_Table_Id ?? 0, invd.Id, invd.Id, Machine_Id, ProcessNameId);
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", invd.Unit_Item_Table_Id.ToString(), " And Invoice_Id=", invL_O.invL.Id.ToString(), " And  Detail_Invoice_Id=", invd.Id.ToString(), " AND Invoice_Type_Id=", ProcessNameId.ToString() };
            string str = string.Concat(textArray1);
            opp.insert_me(str);
            str = "Select Small_unit from Unit_item Where Id=" + invd.Unit_Item_Table_Id.ToString();
            DataTable table = opp.return_DataTable(str);
            decimal Small_unit = 1M;
            try
            {
                Small_unit = decimal.Parse(table.Rows[0]["Small_unit"].ToString());
            }
            catch
            {
            }
            //long Detailes_Purshing_Id = 0;
            //long OrderTable_id = 0;


            decimal Quantity = Common.Get_Item_Quantity(invd.Larg_Qnty ?? 0, invd.Small_Qnty ?? 0, Small_unit);
            int IS_To_Converted = 0;




            if (Transaction_Type == 1)
            {


                die.qua_Import = invd.Larg_Qnty;
                die.Part_Qua_Import = invd.Small_Qnty; ;
                die.Item_No_Import = invd.Qnty_Another; ;
                die.Quantity_Import = Quantity;

                die.qua_Export = 0;
                die.Part_Qua_Export = 0;
                die.Item_No_Export = 0;
                die.Quantity_Export = 0;


                die.PriceUnitImport = invd.PriceUnit;
                die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

                die.PriceUnitExport = 0;
                die.Item_Net_Total_PriceExport = 0;
            }
            else
            {
                die.qua_Import = 0;
                die.Part_Qua_Import = 0; ;
                die.Item_No_Import = 0; ;
                die.Quantity_Import = 0;


                die.qua_Export = invd.Larg_Qnty; ;
                die.Part_Qua_Export = invd.Small_Qnty;
                die.Item_No_Export = invd.Qnty_Another;
                die.Quantity_Export = Quantity;


                die.PriceUnitImport = 0;
                die.Item_Net_Total_PriceImport = 0;

                die.PriceUnitExport = invd.PriceUnit;
                die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;

            }



            //die.PriceUnitImport = invd.PriceUnit;
            //die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

            //die.PriceUnitExport = invd.PriceUnit;
            //die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;


            die.PriceUnit = invd.PriceUnit;
            die.Item_Net_Total_Price = invd.Item_Net_Total_Price;
            die.Qnty_value = invd.Qnty_value;

            die.S_Price = 0;
            die.P_Price = 0;
            die.P_priceAvgCost = 0;
            die.S_priceAvgCost = 0;
            /////////////////////////////
            ///

            die.StoreId_Name = invd.Store_Name;
            die.StoreId = invd.Store_Id;

            die.ProcessNameId = ProcessNameId;
            die.ProcessName = ProcessName;
            die.ProcessNameNumber = invL_O.invL.Invoice_Id.ToString();
            die.Invoice_Type_Str = ProcessName;

            die.Notes_To = die.Id_ToStr;
            die.Notes_From = die.Id_FromStr;
            die.Notes_Item = invd.Item_Notes + " " + invL_O.invL.SubAccount_Name;



            die.Invoice_Id = invL_O.invL.Id;
            die.Invoice_Type_Id = ProcessNameId;
            die.Detail_Invoice_Id = invd.Id;
            die.Invoice_No = invL_O.invL.Invoice_Id??0;
            die.Detailes_Purshing_Id = 0;
            die.OrderTable_id = 0;


            die.Notes_From_To = " „‰ " + die.Id_FromStr + " «·Ï " + die.Id_ToStr;


            die.Notes = invL_O.invL.Notes;

            die.Unit_item_Table_Id = invd.Unit_Item_Table_Id ?? 0;
            die.qua = invd.Larg_Qnty;
            die.Part_qua = invd.Small_Qnty;
            die.Item_No = invd.Qnty_Another;
            die.Balance_Larg_Qnty = 0;
            die.Balance_Small_Qnty = 0;
            die.Balance_Item_No = 0;

            die.Machine_Id = Machine_Id;
            die.Transaction_Type = Transaction_Type;
            die.Process_Date = invL_O.invL.Invoice_Date.Value;

            die.IS_To_Converted = IS_To_Converted;

            die.Quantity = Quantity;
            die.Quantity_Balance = 0;




            die.User_Id = Common.User_Id;
            die.Branch_Id = Common.Branch_Id;
            die.ProcessNo = 0;
            die.CarNumber = "";
            die.CarsId = 0;
            die.CarsWorkOrdersDetailsId = 0;
            die.CarsWorkOrdersId = 0;
            die.WorkOrderNumber = "";
            die.OperationDate = DateTime.Now;

            Detailes_Purshing_Import_ExportCLS dieCls = new Detailes_Purshing_Import_ExportCLS();
            dieCls.Add(ref opp, die);


            opp.cmd.Parameters.Clear();
        }
        public void Set_ItemTransactionFromInvoice(ref operations opp, Invoices_Details  invd_O, InvoicesOperationListCLS invL_O, long Machine_Id, int Transaction_Type, int ProcessNameId, string ProcessName)
        {
            Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();

            if (invL_O.cs == null)
            {
                CustomerCLS customerCLS = new CustomerCLS();
                 customerCLS.GetCustomerByIdAsNoTracking(ref   opp,   Common.Customer_Id_ForInvoiceNakdy);
                invL_O.cs = customerCLS.cs;
            }
            if (ProcessNameId == Common.InvoiceReverse_Type_Id)
            {
                die.Type_From = Common.Customer_StoreTransactionCode;
                die.Type_To = Common.Store_StoreTransactionCode;

                die.Type_From_Name = "⁄„Ì·";
                die.Type_To_Name = "„Œ“‰";

                die.Id_From = invL_O.invL.Customer_Id;
                die.Id_To = invd_O.Store_Id;

                die.Id_FromStr = invL_O.cs?.Name ?? "»œÊ‰ «”„ / €Ì— „Õœœ";
                
                die.Id_ToStr =  invd_O.Store_Name;
            }
            if (ProcessNameId == Common.Invoice_Type_Id)
            {
                die.Type_From = Common.Store_StoreTransactionCode;
                die.Type_To = Common.Customer_StoreTransactionCode;

                die.Type_From_Name =  "„Œ“‰";
                die.Type_To_Name = "⁄„Ì·"; 

                die.Id_From = invd_O.Store_Id;
                die.Id_To = invL_O.invL.Customer_Id;

                die.Id_FromStr = invd_O.Store_Name;
               
                die.Id_ToStr = invL_O.cs?.Name ?? "»œÊ‰ «”„ / €Ì— „Õœœ";
            }


            string str = "";
            this.Delete_ItemTransactionFromInvoices(ref opp, invd_O.Unit_Item_Table_Id ?? 0, invL_O.invL.Id, invd_O.Id, Machine_Id, invL_O.Invoice_Type_Id);
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", invd_O.Unit_Item_Table_Id.ToString(), " And Invoice_Id=", invL_O.invL.Id.ToString(), " And  Detail_Invoice_Id=", invd_O.Id.ToString(), " AND Invoice_Type_Id=", ProcessNameId.ToString() };
            str = string.Concat(textArray1);
            opp.insert_me(str);


            //string[] textArray1 = new string[] { "Select Id From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", invd_O.Unit_Item_Table_Id.ToString(), " And Invoice_Id=", invL_O.invL.Id.ToString(), " And  Detail_Invoice_Id=", invd_O.Id.ToString(), " AND Invoice_Type_Id=", ProcessNameId.ToString() };
            //  str = string.Concat(textArray1);
            //DataTable dt= opp.return_DataTable(str);
            //long Id = 0;
            //if(dt.Rows.Count>0)
            //{
            //    long.TryParse(dt.Rows[0]["Id"].ToString(),out Id);
            //}
            str = "Select Small_unit from Unit_item Where Id=" + invd_O.Unit_Item_Table_Id.ToString();
            DataTable table = opp.return_DataTable(str);
            decimal Small_unit = 1M;
            try
            {
                Small_unit = decimal.Parse(table.Rows[0]["Small_unit"].ToString());
            }
            catch
            {
            }
            //long Detailes_Purshing_Id = 0;
            //long OrderTable_id = 0;


            decimal Quantity = Common.Get_Item_Quantity(invd_O.Larg_Qnty??0, invd_O.Small_Qnty??0, Small_unit);
            int IS_To_Converted = 0;



           
            if (Transaction_Type == 1)
            {


                die.qua_Import = invd_O.Larg_Qnty;
                die.Part_Qua_Import = invd_O.Small_Qnty; ;
                die.Item_No_Import = invd_O.Qnty_Another; ;
                die.Quantity_Import = Quantity;

                die.qua_Export = 0;
                die.Part_Qua_Export = 0;
                die.Item_No_Export = 0;
                die.Quantity_Export = 0;

                die.PriceUnitImport = invd_O.Larg_Unit_Price;
                die.Item_Net_Total_PriceImport = invd_O.Item_Net_Total_Price;

                die.PriceUnitExport = 0;
                die.Item_Net_Total_PriceExport = 0;
            }
            else
            {
                die.qua_Import = 0;
                die.Part_Qua_Import = 0; ;
                die.Item_No_Import =   0;  ;
                die.Quantity_Import = 0;

                die.qua_Export = invd_O.Larg_Qnty;
                die.Part_Qua_Export = invd_O.Small_Qnty;
                die.Item_No_Export = invd_O.Qnty_Another;
                die.Quantity_Export = Quantity;

                die.PriceUnitImport = 0;
                die.Item_Net_Total_PriceImport = 0;

                die.PriceUnitExport = invd_O.Larg_Unit_Price;
                die.Item_Net_Total_PriceExport = invd_O.Item_Net_Total_Price;
            }


            //die.PriceUnitImport = invd.PriceUnit;
            //die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

            //die.PriceUnitExport = invd.PriceUnit;
            //die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;


            die.PriceUnit = invd_O.Larg_Unit_Price;
            die.Item_Net_Total_Price = invd_O.Item_Net_Total_Price;
            die.Qnty_value = invd_O.Qnty_value;

            die.S_Price = 0;
            die.P_Price = 0;
            die.P_priceAvgCost = 0;
            die.S_priceAvgCost = 0;
            ////////////////////////////////

            die.StoreId_Name = invd_O.Store_Name;
            die.StoreId = invd_O.Store_Id;



            die.ProcessNameId = ProcessNameId;
            die.ProcessName = ProcessName;
            die.ProcessNameNumber = invL_O.invL.Invoice_Id.ToString();
            die.Invoice_Type_Str = ProcessName;

            die.Notes_To = die.Id_ToStr;
            die.Notes_From = die.Id_FromStr;

            die.Notes_Item = invd_O.Item_Notes + " " + invL_O.cs?.Name??"€Ì— „Õœœ";//.Customer_Name;
             


            die.Invoice_Id = invL_O.invL.Id;
            die.Invoice_Type_Id = invL_O.Invoice_Type_Id;
            die.Detail_Invoice_Id = invd_O.Id;
            die.Invoice_No = invL_O.invL.Invoice_Id??0;
            die.Detailes_Purshing_Id = 0;
            die.OrderTable_id = 0;


            die.Notes_From_To = " „‰ " + die.Id_FromStr +  " «·Ï " + die.Id_ToStr ;

            die.Notes = invL_O.invL.Notes;



            die.Unit_item_Table_Id = invd_O.Unit_Item_Table_Id??0;
            die.qua = invd_O.Larg_Qnty;
            die.Part_qua = invd_O.Small_Qnty;
            die.Item_No = invd_O.Qnty_Another;
            die.Balance_Larg_Qnty = 0;
            die.Balance_Small_Qnty = 0;
            die.Balance_Item_No = 0;

            die.Machine_Id = Machine_Id;
            die.Transaction_Type = Transaction_Type;
            die.Process_Date = invL_O.invL.Invoice_Date;

            die.IS_To_Converted = IS_To_Converted;

            die.Quantity = Quantity;
            die.Quantity_Balance = 0;
             
            

            die.User_Id = Common.User_Id;
            die.Branch_Id = Common.Branch_Id;
            die.ProcessNo = 0;
            die.CarNumber = "";
            die.CarsId = 0;
            die.CarsWorkOrdersDetailsId = 0;
            die.CarsWorkOrdersId = 0;
            die.WorkOrderNumber = "";
            die.OperationDate = DateTime.Now;

            
            Detailes_Purshing_Import_Export dieNew = MappingHelper.MapScalarProperties<Detailes_Purshing_Import_Export, Detailes_Purshing_Import_Export>(die);
            Detailes_Purshing_Import_ExportCLS dieCls = new Detailes_Purshing_Import_ExportCLS();
            dieNew.Id = 0;
             dieCls.Add(ref opp, dieNew);
            //opp.db.Entry(dieNew).State = System.Data.Entity.EntityState.Added;

            // 3. Õ›Ÿ Â–« «·ﬂ«∆‰ «·„Õœœ ›ﬁÿ (Â–« Ì„‰⁄ «·«‰ÂÌ«— »”»» „Œ·›«  «·‘«‘… «·√Œ—Ï)
           

            //str = "Delete From Detailes_Purshing_Import_Export Where  Id= "+Id;
            //opp.insert_me(str);

            opp.cmd.Parameters.Clear();
        }
        public void Set_ItemTransactionFromInvoiceR(ref operations opp, IInvoices_Details invD, InvoicesOperationListCLS invL_O, long Machine_Id, int Transaction_Type, int ProcessNameId, string ProcessName)
        {
            Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();
            if (ProcessNameId == Common.InvoiceReverse_Type_Id)
            {
                die.Type_From = Common.Customer_StoreTransactionCode;
                die.Type_To = Common.Store_StoreTransactionCode;

                die.Type_From_Name = "⁄„Ì·";
                die.Type_To_Name = "„Œ“‰";

                die.Id_From = invL_O.invL.Customer_Id;
                die.Id_To = invD.Store_Id;

                die.Id_FromStr = invL_O.cs.Name;//.Customer_Name;
                die.Id_ToStr = invD.Store_Name;
            }
            if (ProcessNameId == Common.Invoice_Type_Id)
            {
                die.Type_From = Common.Store_StoreTransactionCode;
                die.Type_To = Common.Customer_StoreTransactionCode;

                die.Type_From_Name = "„Œ“‰";
                die.Type_To_Name = "⁄„Ì·";

                die.Id_From = invD.Store_Id;
                die.Id_To = invL_O.invL.Customer_Id;

                die.Id_FromStr = invD.Store_Name;
                die.Id_ToStr = invL_O.cs.Name;//.Customer_Name;
            }



            this.Delete_ItemTransactionFromInvoices(ref opp, invD.Unit_Item_Table_Id ?? 0, invL_O.Invoices_List_Id, invD.Id, Machine_Id, invL_O.Invoice_Type_Id);
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", invD.Unit_Item_Table_Id.ToString(), " And Invoice_Id=", invL_O.Invoices_List_Id.ToString(), " And  Detail_Invoice_Id=", invD.Id.ToString(), " AND Invoice_Type_Id=", ProcessNameId.ToString() };
            string str = string.Concat(textArray1);
            opp.insert_me(str);
            str = "Select Small_unit from Unit_item Where Id=" + invD.Unit_Item_Table_Id.ToString();
            DataTable table = opp.return_DataTable(str);
            decimal Small_unit = 1M;
            try
            {
                Small_unit = decimal.Parse(table.Rows[0]["Small_unit"].ToString());
            }
            catch
            {
            }
            //long Detailes_Purshing_Id = 0;
            //long OrderTable_id = 0;


            decimal Quantity = Common.Get_Item_Quantity(invD.Larg_Qnty ?? 0, invD.Small_Qnty ?? 0, Small_unit);
            int IS_To_Converted = 0;




            if (Transaction_Type == 1)
            {


                die.qua_Import = invD.Larg_Qnty;
                die.Part_Qua_Import = invD.Small_Qnty; ;
                die.Item_No_Import = invD.Qnty_Another; ;
                die.Quantity_Import = Quantity;

                die.qua_Export = 0;
                die.Part_Qua_Export = 0;
                die.Item_No_Export = 0;
                die.Quantity_Export = 0;

                die.PriceUnitImport = invD.Larg_Unit_Price;
                die.Item_Net_Total_PriceImport = invD.Item_Net_Total_Price;

                die.PriceUnitExport = 0;
                die.Item_Net_Total_PriceExport = 0;
            }
            else
            {
                die.qua_Import = 0;
                die.Part_Qua_Import = 0; ;
                die.Item_No_Import = 0; ;
                die.Quantity_Import = 0;

                die.qua_Export = invD.Larg_Qnty;
                die.Part_Qua_Export = invD.Small_Qnty;
                die.Item_No_Export = invD.Qnty_Another;
                die.Quantity_Export = Quantity;

                die.PriceUnitImport = 0;
                die.Item_Net_Total_PriceImport = 0;

                die.PriceUnitExport = invD.Larg_Unit_Price;
                die.Item_Net_Total_PriceExport = invD.Item_Net_Total_Price;
            }


            //die.PriceUnitImport = invd.PriceUnit;
            //die.Item_Net_Total_PriceImport = invd.Item_Net_Total_Price;

            //die.PriceUnitExport = invd.PriceUnit;
            //die.Item_Net_Total_PriceExport = invd.Item_Net_Total_Price;


            die.PriceUnit = invD.Larg_Unit_Price;
            die.Item_Net_Total_Price = invD.Item_Net_Total_Price;
            die.Qnty_value = invD.Qnty_value;

            die.S_Price = 0;
            die.P_Price = 0;
            die.P_priceAvgCost = 0;
            die.S_priceAvgCost = 0;
            ////////////////////////////////

            die.StoreId_Name = invD.Store_Name;
            die.StoreId = invD.Store_Id;



            die.ProcessNameId = ProcessNameId;
            die.ProcessName = ProcessName;
            die.ProcessNameNumber = invL_O.invL.Invoice_Id.ToString();
            die.Invoice_Type_Str = ProcessName;

            die.Notes_To = die.Id_ToStr;
            die.Notes_From = die.Id_FromStr;

            die.Notes_Item = invD.Item_Notes + " " + invL_O.cs.Name;//.Customer_Name;



            die.Invoice_Id = invL_O.Invoices_List_Id;
            die.Invoice_Type_Id = invL_O.Invoice_Type_Id;
            die.Detail_Invoice_Id = invD.Id;
            die.Invoice_No = invL_O.invL.Invoice_Id??0;
            die.Detailes_Purshing_Id = 0;
            die.OrderTable_id = 0;


            die.Notes_From_To = " „‰ " + die.Id_FromStr + " «·Ï " + die.Id_ToStr;

            die.Notes = invL_O.invL.Notes;



            die.Unit_item_Table_Id = invD.Unit_Item_Table_Id ?? 0;
            die.qua = invD.Larg_Qnty;
            die.Part_qua = invD.Small_Qnty;
            die.Item_No = invD.Qnty_Another;
            die.Balance_Larg_Qnty = 0;
            die.Balance_Small_Qnty = 0;
            die.Balance_Item_No = 0;

            die.Machine_Id = Machine_Id;
            die.Transaction_Type = Transaction_Type;
            die.Process_Date = invL_O.invL.Invoice_Date;

            die.IS_To_Converted = IS_To_Converted;

            die.Quantity = Quantity;
            die.Quantity_Balance = 0;



            die.User_Id = Common.User_Id;
            die.Branch_Id = Common.Branch_Id;
            die.ProcessNo = 0;
            die.CarNumber = "";
            die.CarsId = 0;
            die.CarsWorkOrdersDetailsId = 0;
            die.CarsWorkOrdersId = 0;
            die.WorkOrderNumber = "";
            die.OperationDate = DateTime.Now;


            Detailes_Purshing_Import_ExportCLS dieCls = new Detailes_Purshing_Import_ExportCLS();
            dieCls.Add(ref opp, die);


            opp.cmd.Parameters.Clear();
        }

        public void Set_ItemTransactionFromInvoices(ref operations opp, long Unit_item_Table_Id,decimal PriceUnit, decimal Total_Value, decimal qua, decimal Part_qua, decimal Item_No, long Machine_Id, int Transaction_Type, DateTime Process_Date, long Invoice_Id, long Detail_Invoice_Id, int Invoice_Type_Id, string Notes, string Id_ToStr, string Id_FromStr, string Notes_Item, long Invoice_No, int Type_From = 0, int Type_To = 0, long Id_From = 0, long Id_To = 0, long StoreId = 0, string StoreId_Name = "")
        {
            this.Delete_ItemTransactionFromInvoices(ref opp, Unit_item_Table_Id, Invoice_Id, Detail_Invoice_Id, Machine_Id, Invoice_Type_Id);
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", Unit_item_Table_Id.ToString(), " And Invoice_Id=", Invoice_Id.ToString(), " And  Detail_Invoice_Id=", Detail_Invoice_Id.ToString(), " AND Invoice_Type_Id=", Invoice_Type_Id.ToString() };
            string str = string.Concat(textArray1);
            opp.insert_me(str);
            str = "Select Small_unit from Unit_item Where Id=" + Unit_item_Table_Id.ToString();
            DataTable table = opp.return_DataTable(str);
            decimal Small_unit = 1M;
            try
            {
                Small_unit = decimal.Parse(table.Rows[0]["Small_unit"].ToString());
            }
            catch
            {
            }
            long Detailes_Purshing_Id = 0L;
            long OrderTable_id = 0L;
            decimal Balance_Larg_Qnty = 0M;
            decimal Balance_Small_Qnty = 0M;
            decimal Balance_Item_No = 0M;

            decimal Quantity = Common.Get_Item_Quantity(qua, Part_qua, Small_unit);
            int IS_To_Converted = 0;
            decimal qua_Import = 0M;
            decimal Part_Qua_Import = 0M;
            decimal Item_No_Import = 0M;
            decimal Quantity_Import = 0M;
            decimal qua_Export = 0M;
            decimal Part_Qua_Export = 0M;
            decimal Item_No_Export = 0M;
            decimal Quantity_Export = 0M;
            decimal Quantity_Balance = 0M;

            Detailes_Purshing_Import_Export die = new Detailes_Purshing_Import_Export();

            if (Transaction_Type == 1)
            {
                qua_Import = qua;
                Part_Qua_Import = Part_qua;
                Item_No_Import = Item_No;
                Quantity_Import = Quantity;

                die.PriceUnitImport = PriceUnit;
                die.Item_Net_Total_PriceImport = Total_Value;

                die.PriceUnitExport = 0;
                die.Item_Net_Total_PriceExport = 0;


            }
            else
            {
                qua_Export = qua;
                Part_Qua_Export = Part_qua;
                Item_No_Export = Item_No;
                Quantity_Export = Quantity;

                die.PriceUnitImport = 0;
                die.Item_Net_Total_PriceImport = 0;

                die.PriceUnitExport = PriceUnit;
                die.Item_Net_Total_PriceExport = Total_Value;

            }
            die.Item_Net_Total_Price = Total_Value;
            die.PriceUnit = PriceUnit;
            
            die.Qnty_value = Total_Value;

            die.S_Price = 0;
            die.P_Price = 0;
            die.P_priceAvgCost = 0;
            die.S_priceAvgCost = 0;
            ////////////////////////////////

            

            string Notes_From_To = Id_FromStr + " " + Id_ToStr;

            die.Type_From = Type_From;

            die.Type_To = Type_To;
            die.Id_From = Id_From;
            die.Id_To = Id_To;

            die.StoreId_Name = StoreId_Name;
            die.StoreId = StoreId;

            die.Id_ToStr = Id_ToStr;
            die.Id_FromStr = Id_FromStr;

            die.ProcessNameId = Invoice_Type_Id;
            die.ProcessName = Notes;
            die.ProcessNameNumber = Invoice_Id.ToString();
            die.Invoice_Type_Str = Notes;

            die.Notes_To = Id_ToStr;
            die.Notes_From = Id_FromStr;

            die.Notes_Item = Notes_Item;


            die.Invoice_Id = Invoice_Id;
            die.Invoice_Type_Id = Invoice_Type_Id;
            die.Detail_Invoice_Id = Detail_Invoice_Id;
            die.Invoice_No = Invoice_No;
            die.Detailes_Purshing_Id = Detailes_Purshing_Id;
            die.OrderTable_id = OrderTable_id;


            die.Notes_From_To = Notes_Item;

            die.Notes = Notes;



            die.Unit_item_Table_Id = Unit_item_Table_Id;
            die.qua = qua;
            die.Part_qua = Part_qua;
            die.Item_No = Item_No;
            die.Balance_Larg_Qnty = Balance_Larg_Qnty;
            die.Balance_Small_Qnty = Balance_Small_Qnty;
            die.Balance_Item_No = Balance_Item_No;

            die.Machine_Id = (int)Machine_Id;
            die.Transaction_Type = Transaction_Type;
            die.Process_Date = Process_Date;

            die.IS_To_Converted = IS_To_Converted;
            die.qua_Import = qua_Import;
            die.Part_Qua_Import = Part_Qua_Import;
            die.Item_No_Import = Item_No_Import;
            die.Quantity_Import = Quantity_Import;
            die.qua_Export = qua_Export;
            die.Part_Qua_Export = Part_Qua_Export;
            die.Item_No_Export = Item_No_Export;
            die.Quantity_Export = Quantity_Export;
            die.Quantity = Quantity;
            die.Quantity_Balance = Quantity_Balance;

            die.User_Id = Common.User_Id;
            die.OperationDate = DateTime.Now;
           


            Detailes_Purshing_Import_ExportCLS dieCls = new Detailes_Purshing_Import_ExportCLS();
            dieCls.Add(ref opp, die);


            opp.cmd.Parameters.Clear();
        }


        public void Set_ItemTransactionFromInvoicesOld(ref operations opp, long Unit_item_Table_Id, decimal Total_Value, decimal qua, decimal Part_qua, decimal Item_No, long Machine_Id, int Transaction_Type, DateTime Process_Date, long Invoice_Id, long Detail_Invoice_Id, int Invoice_Type_Id, string Notes, string Notes_To, string Notes_From, string Notes_Item, long Invoice_No)
        {
            this.Delete_ItemTransactionFromInvoices(ref opp, Unit_item_Table_Id, Invoice_Id, Detail_Invoice_Id, Machine_Id, Invoice_Type_Id);
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", Unit_item_Table_Id.ToString(), " And Invoice_Id=", Invoice_Id.ToString(), " And  Detail_Invoice_Id=", Detail_Invoice_Id.ToString(), " AND Invoice_Type_Id=", Invoice_Type_Id.ToString() };
            string str = string.Concat(textArray1);
            opp.insert_me(str);
            str = "Select Small_unit from Unit_item Where Id=" + Unit_item_Table_Id.ToString();
            DataTable table = opp.return_DataTable(str);
            decimal Small_unit = 1M;
            try
            {
                Small_unit = decimal.Parse(table.Rows[0]["Small_unit"].ToString());
            }
            catch
            {
            }
            long Detailes_Purshing_Id = 0L;
            long OrderTable_id = 0L;
            decimal Balance_Larg_Qnty = 0M;
            decimal Balance_Small_Qnty = 0M;
            decimal Balance_Item_No = 0M;
            string str2 = Notes_From + " " + Notes_To;
            decimal Quantity = Common.Get_Item_Quantity(qua, Part_qua, Small_unit);
            int IS_To_Converted = 0;
            decimal qua_Import = 0M;
            decimal Part_Qua_Import = 0M;
            decimal Item_No_Import = 0M;
            decimal Quantity_Import = 0M;
            decimal qua_Export = 0M;
            decimal Part_Qua_Export = 0M;
            decimal Item_No_Export = 0M;
            decimal Quantity_Export = 0M;
            decimal Quantity_Balance = 0M;
            if (Transaction_Type == 1)
            {
                qua_Import = qua;
                Part_Qua_Import = Part_qua;
                Item_No_Import = Item_No;
                Quantity_Import = Quantity;
            }
            else
            {
                qua_Export = qua;
                Part_Qua_Export = Part_qua;
                Item_No_Export = Item_No;
                Quantity_Export = Quantity;
            }
            opp.cmd.Parameters.Clear();
            str = "INSERT INTO Detailes_Purshing_Import_Export(Detailes_Purshing_Id, OrderTable_id, Unit_item_Table_Id, qua, Part_qua, Item_No, Balance_Larg_Qnty, Balance_Small_Qnty, Balance_Item_No, Notes_From_To, Notes, Machine_Id, Transaction_Type,Process_Date, Notes_To, Notes_From, IS_To_Converted, qua_Import, Part_Qua_Import, Item_No_Import, Quantity_Import, qua_Export, Part_Qua_Export, Item_No_Export, Quantity_Export, Quantity,Quantity_Balance,  Invoice_Id, Detail_Invoice_Id,  Invoice_Type_Id,Notes_Item,User_Id,OperationDate,Total_Value,Invoice_No) " + " VALUES (@Detailes_Purshing_Id, @OrderTable_id, @Unit_item_Table_Id, @qua, @Part_qua, @Item_No, @Balance_Larg_Qnty, @Balance_Small_Qnty, @Balance_Item_No, @Notes_From_To, @Notes, @Machine_Id, @Transaction_Type,@Process_Date, @Notes_To, @Notes_From, @IS_To_Converted, @qua_Import, @Part_Qua_Import, @Item_No_Import, @Quantity_Import, @qua_Export, @Part_Qua_Export, @Item_No_Export, @Quantity_Export, @Quantity,@Quantity_Balance,  @Invoice_Id,  @Detail_Invoice_Id, @Invoice_Type_Id,@Notes_Item,@user_Id,@OperationDate,@Total_Value,@Invoice_No)  ";
            opp.cmd.Parameters.AddWithValue("@Detailes_Purshing_Id", Detailes_Purshing_Id);
            opp.cmd.Parameters.AddWithValue("@OrderTable_id", OrderTable_id);
            opp.cmd.Parameters.AddWithValue("@Unit_item_Table_Id", Unit_item_Table_Id);
            opp.cmd.Parameters.AddWithValue("@qua", qua);
            opp.cmd.Parameters.AddWithValue("@Part_qua", Part_qua);
            opp.cmd.Parameters.AddWithValue("@Item_No", Item_No);
            opp.cmd.Parameters.AddWithValue("@Balance_Larg_Qnty", Balance_Larg_Qnty);
            opp.cmd.Parameters.AddWithValue("@Balance_Small_Qnty", Balance_Small_Qnty);
            opp.cmd.Parameters.AddWithValue("@Balance_Item_No", Balance_Item_No);
            opp.cmd.Parameters.AddWithValue("@Notes_From_To", str2);
            opp.cmd.Parameters.AddWithValue("@Notes", Notes);
            opp.cmd.Parameters.AddWithValue("@Machine_Id", Machine_Id);
            opp.cmd.Parameters.AddWithValue("@Transaction_Type", Transaction_Type);
            opp.cmd.Parameters.AddWithValue("@Process_Date", Process_Date);
            opp.cmd.Parameters.AddWithValue("@Notes_To", Notes_To);
            opp.cmd.Parameters.AddWithValue("@Notes_From", Notes_From);
            opp.cmd.Parameters.AddWithValue("@IS_To_Converted", IS_To_Converted);
            opp.cmd.Parameters.AddWithValue("@qua_Import", qua_Import);
            opp.cmd.Parameters.AddWithValue("@Part_Qua_Import", Part_Qua_Import);
            opp.cmd.Parameters.AddWithValue("@Item_No_Import", Item_No_Import);
            opp.cmd.Parameters.AddWithValue("@Quantity_Import", Quantity_Import);
            opp.cmd.Parameters.AddWithValue("@qua_Export", qua_Export);
            opp.cmd.Parameters.AddWithValue("@Part_Qua_Export", Part_Qua_Export);
            opp.cmd.Parameters.AddWithValue("@Item_No_Export", Item_No_Export);
            opp.cmd.Parameters.AddWithValue("@Quantity_Export", Quantity_Export);
            opp.cmd.Parameters.AddWithValue("@Quantity", Quantity);
            opp.cmd.Parameters.AddWithValue("@Quantity_Balance", Quantity_Balance);
            opp.cmd.Parameters.AddWithValue("@Invoice_Id", Invoice_Id);
            opp.cmd.Parameters.AddWithValue("@Invoice_Type_Id", Invoice_Type_Id);
            opp.cmd.Parameters.AddWithValue("@Detail_Invoice_Id", Detail_Invoice_Id);
            opp.cmd.Parameters.AddWithValue("@Notes_Item", Notes_Item);
            opp.cmd.Parameters.AddWithValue("@user_Id", Common.User_Id);
            opp.cmd.Parameters.AddWithValue("@OperationDate", DateTime.Now);
            opp.cmd.Parameters.AddWithValue("@Total_Value", Total_Value);
            opp.cmd.Parameters.AddWithValue("@Invoice_No", Invoice_No);
            opp.insert_me(str);
            opp.cmd.Parameters.Clear();
        }

        public void Delete_ItemTransactionFromInvoices1(ref operations opp, long Unit_item_Table_Id, long Invoice_Id, long Detail_Invoice_Id, long Machine_Id, int Invoice_Type_Id)
        {
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", Unit_item_Table_Id.ToString(), " And Invoice_Id=", Invoice_Id.ToString(), " And  Detail_Invoice_Id=", Detail_Invoice_Id.ToString(), " AND Invoice_Type_Id=", Invoice_Type_Id.ToString() };
            string str = string.Concat(textArray1);
            opp.insert_me(str);
        }

        public void Delete_ItemTransactionFromInvoices1(ref operations opp, long Unit_item_Table_Id, decimal qua, decimal Part_qua, decimal Item_No, long Machine_Id, int Transaction_Type, DateTime Process_Date, long Invoice_Id, long Detail_Invoice_Id, int Invoice_Type_Id, string Notes, string Notes_To, string Notes_From, string Notes_Item)
        {

            string mysql = "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =" + Unit_item_Table_Id + " And Invoice_Id=" + Invoice_Id + " And  Detail_Invoice_Id=" + Detail_Invoice_Id + " AND Invoice_Type_Id=" + Invoice_Type_Id + "";
            opp.insert_me(mysql);



        }

        public void Delete_ItemTransactionFromInvoices(ref operations opp, long Unit_item_Table_Id, long Invoice_Id, long Detail_Invoice_Id, long Machine_Id, int Invoice_Type_Id)
        {
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", Unit_item_Table_Id.ToString(), " And Invoice_Id=", Invoice_Id.ToString(), " And  Detail_Invoice_Id=", Detail_Invoice_Id.ToString(), " AND Invoice_Type_Id=", Invoice_Type_Id.ToString() };
            string str = string.Concat(textArray1);
            int c=opp.insert_me(str);
        }

        public void Delete_ItemTransactionFromInvoices(ref operations opp, long Unit_item_Table_Id, decimal qua, decimal Part_qua, decimal Item_No, long Machine_Id, int Transaction_Type, DateTime Process_Date, long Invoice_Id, long Detail_Invoice_Id, int Invoice_Type_Id, string Notes, string Notes_To, string Notes_From, string Notes_Item)
        {
            string[] textArray1 = new string[] { "Delete From Detailes_Purshing_Import_Export Where Unit_item_Table_Id =", Unit_item_Table_Id.ToString(), " And Invoice_Id=", Invoice_Id.ToString(), " And  Detail_Invoice_Id=", Detail_Invoice_Id.ToString(), " AND Invoice_Type_Id=", Invoice_Type_Id.ToString() };
            string str = string.Concat(textArray1);
            opp.insert_me(str);
        }

    }
}
