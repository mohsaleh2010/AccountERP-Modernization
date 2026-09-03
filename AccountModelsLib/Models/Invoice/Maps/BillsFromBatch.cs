using AccountModelsLib.Models;
 
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class BillsFromBatch
    {


        public decimal WahbaValue = 0;
        public decimal NawlounValue = 0;
        public decimal Total3MolaRate = 0;
        public decimal Total3Mola = 0;  
        public decimal Value_3Mola = 0; 
        public void CreateInvoiceFromSales(ref List<Unit_Item_SalesDate> salesList, ref IBills_List Bills_List, ref List<IBills_Details> invDlst, int Is_Invoice_Agile, AccountModelsLib.operations opp)
        {

            try
            {
                // إنشاء الفاتورة
                Bills_List Bills_List_ = MapSalesToInvoiceHeader(salesList, Bills_List, Is_Invoice_Agile);
                Bills_List = Bills_List_;

                // تحويل كل عملية بيع إلى تفصيلة فاتورة
                List<Bills_Details> Bills_Details = salesList.Select(s =>
                {
                    var detail = MapSalesToInvoiceDetail(s); // دالة سبق شرحها
                    detail.Invoice_Id = Bills_List_.Id;
                    return detail;
                }).ToList();
                //invDlst = Bills_Details;
                foreach (var item in Bills_Details)
                {
                    invDlst.Add(item);
                }

                // تحديث عمليات البيع وربطها بالفاتورة
                //foreach (var s in salesList)
                //{
                //    s.Bills_ListId = Bills_List.Id;
                //    s.IsProcessedAsBill = 1;

                //}



            }
            catch (Exception ex)
            {

                throw new Exception("فشل إنشاء الفاتورة: " + ex.Message);
            }

        }
        public void SetBills(ref int Excp, ref string Msg, ref long Bills_ListId, List<Unit_Item_SalesDate> salesList, long Supplier_Id, int Is_Invoice_Agile, AccountModelsLib.operations opp)
        {
            try
            {
                Excp = 0;
                Msg = "";
                BillOperationCLS invD_O = new BillOperationCLS(0);
                BillOperationListCLS invL_O = new BillOperationListCLS(0);

                invL_O.salesList = salesList;

                invL_O.Invoice_Type_Id = Common.Invoice_Type_Id;
                invL_O.invL.Is_Invoice_Agile = Is_Invoice_Agile;
                invL_O.invL.Total_List3Mola = Total3Mola;
                invL_O.invL.Value_3Mola = Value_3Mola;
                invL_O.invL.WahbaValue = WahbaValue;
                invL_O.invL.NawlounValue = NawlounValue;
                invL_O.invL.Total3MolaRate = Total3MolaRate;
                
                invL_O.invL.Supplier_Id = Supplier_Id;

                CreateInvoiceFromSales(ref invL_O.salesList, ref invL_O.invL, ref invD_O.invDlst, Is_Invoice_Agile, opp);


                //invL_O.invL = Bills_List;
                //invD_O.invDlst = invDlst;
                //Get_MakeInvoiceListData(Common.Invoice_Type_Id);




                Msg = "";
                Bills_ModelsClss rc = new Bills_ModelsClss();
                //invL_O.Is_Invoice_Agile = Is_Invoice_Agile;


                //Get_InvoiceListData(Common.Bill_Type_Id);
                rc.Make_Bill_Update_SupplierAccount(ref Excp, ref Msg, invD_O, invL_O);

                Bills_ListId = invL_O.invL.Id;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }

        }
        public List<Bills_Details> MapSalesListToInvoiceDetails(List<Unit_Item_SalesDate> salesList)
        {
            return salesList?.Select(MapSalesToInvoiceDetail).ToList();
        }


        public Bills_List MapSalesToInvoiceHeader(List<Unit_Item_SalesDate> salesList, IBills_List bills_List, int Is_Invoice_Agile)
        {
            if (salesList == null || salesList.Count == 0)
                throw new ArgumentException("قائمة المبيعات فارغة.");

            var first = salesList.First();

            //string Is_Invoice_AgileStr = "فاتورة أجل";

            decimal Total = salesList.Sum(s => (s.Larg_Balance ?? 0) * (s.P_Price ?? 0));

            bills_List.Total_List3Mola= bills_List.Total_List3Mola?? 0;
            bills_List.WahbaValue = bills_List.WahbaValue ?? 0;
            bills_List.NawlounValue = bills_List.NawlounValue ?? 0;
            bills_List.Total3MolaRate = bills_List.Total3MolaRate ?? 0;
            bills_List.Value_3Mola = bills_List.Value_3Mola ?? 0;
         
 
         

            decimal Net_Total = Total - bills_List.Value_3Mola.Value - bills_List.WahbaValue.Value - bills_List.NawlounValue.Value;
            Bills_ModelsClss inv = new Bills_ModelsClss();
            long Invoice_No = inv.Make_New_Invoice_No(0);
            SupplierCLS supplierCLS = new SupplierCLS();
            supplierCLS.Get_Supplier(bills_List.Supplier_Id ?? 0);


            //CustomerCLS customerCLS = new CustomerCLS();
            //Customer cs = customerCLS.GetCustomerNoTracking(first.Customer_Id??0);
            return new Bills_List
            {
                
                WahbaValue = bills_List.WahbaValue ?? 0,
                NawlounValue = bills_List.NawlounValue ?? 0,
                 Total3MolaRate = bills_List.Total3MolaRate ?? 0,
                 Value_3Mola = bills_List.Value_3Mola ?? 0,



                Total_List3Mola = bills_List.Total_List3Mola,
                Invoice_Id = Invoice_No,
                Supplier_Id = bills_List.Supplier_Id,


                SubAccount_Id = supplierCLS.sp.SubAccount_Id,
                SubAccount_Name = supplierCLS.sp.Name,

                Invoice_Date = DateTime.Now,
                Invoice_DateTime = DateTime.Now,
                OperationDate = DateTime.Now,


                Total = salesList.Sum(s => (s.Larg_Balance ?? 0) * (s.P_Price ?? 0)),

                Net_Total = Net_Total,

                TafkeetNet_Total = TafkeetProj.NumberToText.GetText(Net_Total, 1),   // 
                Is_Invoice_AgileStr = Common.GetInvoiceTypeAgile(Is_Invoice_Agile),
                Is_Invoice_Agile = Is_Invoice_Agile,
                Invoice_Type = 0,
                User_Id = first.User_Id,
                Branch_Id = first.Branch_Id,
                Insert_ServerDateTime = DateTime.Now,
                EditOperationDate = DateTime.Now,
                Edit_User_Id = first.Edit_User_Id,
                Notes = "فاتورة ناتجة من عمليات البيع اليومية",
                Supplier_Notes = "",

                GuId = Guid.NewGuid(),
                Currency = Common.Currency_Name,
                Currency_id = Common.Entry_Currency_Id,

                Update_ServerDateTime = null,
                License_Id = 0,



                Editable = 0,
                Deleted = 0,
                Deleted_Date = null,
                Control_Cusstomer_Account = 0,

                PolicaNo = "",

                Day_Registration_flage = 0,

                commissionValue = 0,
                DayReg_List_Id = 0,
                CustomerAccountBefor = 0,
                CustomerAccountAfter = 0,

                Total_Item_CommissionValue = 0,
                Total_CommissionValue = 0,


                Equivalent_Net_Total = 0,
                Equivalent_CommissionValue = 0,
                Equivalent_Total = 0,
                IS_Posted_Flage = 0,
                Account_Entry_List_ID = 0,
                IsCommission_Value = 0,
                Salesman_Id = 0,

                Is_Buy_From_Bills_Items_Balance = 0,

                IS_Make_Sarf_Item_BalanceFromInvoice = 0,
                Is_InvoiceKta3y = 0,
                Is_Tax_Per_Item = 0,
                Is_Load_Payment_OnTax = 0,
                Total_PriceWithoutTax_Load_Payment = 0,

                Total_PriceWithoutTax = 0,


                PriceByTenns = 0,



                List_CommissionValue = 0,
                List_CommissionRate = 0,
                List_Final_CommissionValue = 0,
                Net_Total_CommissionValue = 0,
                Equivalent_Net_Total_CommissionValue = 0,
                Equivalent_Net_Total_Price = 0,

                ProcessNo = 0,
                Order_MoneyId = 0,
                BanksId = 0,
                BanksNamePaymentMethod = "",
                PaidValue = 0,
                RestValue = 0,
                Emp_Name = "",


                Emp_Id = 0,

                CarsWorkOrdersId = 0,

                TaxSales10 = 0,
                TaxSales10_Value = 0,

                Equivalent_TaxSales10_Value = 0,
                Equivalent_ATS05_Value = 0,
                ATS05 = 0,
                ATS05_Value = 0,

                IsInvoiceTaxInfo = 0,


                Supplier_BalanceSet = 0,

                TroodContainerNo = "0",

                Load_Payment = 0,
                Damaghat = 0,
                Second_Total = 0,
                Load_Payment_OnTax = 0,
                IsMaintainceInvoice=1,
                //CustomerAccountbefor = 0,
                //SupplierAccountafter = 0,


            };
        }


        string SetQntyShow(Unit_Item_SalesDate sales)
        {
            string QntyStr = "";
            unitCLS unitCLS = new unitCLS();
            unit un = unitCLS.GetUnitByIdAsNoTracking(sales.Item.Unit_Id ?? 0);
            //decimal   Qnty = Common.Get_Item_Quantity(carWorkOrderDetail.Larg_Qnty ?? 0, carWorkOrderDetail.Small_Qnty ?? 0, SelectedUnitItem.Small_unit ?? 1);
            //string QntyStr = Common.GetQntyString(invD.Larg_Qnty.ToString(), im.unit.B_Unit.ToString(), invD.Small_Qnty.ToString(), im.unit.S_unit.ToString());


            QntyStr = Common.GetQntyString(un.S_unit_Value, sales.Larg_Balance, un.B_Unit.ToString(), 0, un.S_unit.ToString());


            //QntyStr = Common.GetQntyLenghts_Str(QntyStr, "0".ToString(), carWorkOrderDetail.QntyLenght.ToString(), carWorkOrderDetail.QntyItemNo.ToString());
            return QntyStr;
        }
        public Bills_Details MapSalesToInvoiceDetail(Unit_Item_SalesDate sales)
        {
            if (sales == null) return null;

            long Invoice_Id = 0;

            Bills_Details invD = new Bills_Details
            {
                Invoice_Id = Invoice_Id,
                Unit_Item_SalesDateId = sales.Id,
                Item_Id = sales.Item_Id,
                Unit_Item_Table_Id = sales.Unit_Item_Table_Id,
                Larg_Qnty = sales.Larg_Balance,
                Small_Qnty = 0,
                Qnty_Another = sales.Item_No,
                Larg_Unit_Price = sales.P_Price,
                Qnty_value = sales.QntyValue,
                Total = sales.QntyValue,
                Item_Total_WithoutTax = sales.QntyValue,
                Item_Net_Total_Price = sales.QntyValue,
                OperationDate = sales.SalesDate,
                Unit_Item_ExpireDate_Id = sales.Unit_Item_ExpireDateId,
                Invoice_Item_Name = sales.ItemName_OnInvoice ?? sales.Item_Name ?? sales.ItemNameSource,
                Unit3Mola = 0,
                Total3Mola = 0,

                Notes = "ناتج من عملية بيع يدوي", // ملاحظة اختيارية
                User_Id = sales.User_Id,
                Branch_Id = sales.Branch_Id,
                Edit_User_Id = sales.Edit_User_Id,
                ProcessNo = sales.ProcessNo,


                P_price_LargeUnit = 0,
                P_price_SmallUnit = 0,



                S_price = 0,

                Small_Unit_Price = 0,
                PriceUnit = sales.P_Price,

                Unit_Used_for_Price = 1,
                Unit_Id = sales.Item.Unit_Id,
                UnitStr = sales.Item.UnitStr,


                code = sales.Item.code,
                NameSpecification = sales.Item.ItemName_OnInvoice,


                Item_Notes = "ناتج من عملية بيع يدوي", //,
                QntyStr = SetQntyShow(sales),
                QntyStrColumn = SetQntyShow(sales),



                Specification = "",

                Make_Order = 0,
                Balance_Checked = 0,

                Order_Purshing_Id_Automatic = 0,
                Store_Id = 0,


                Item_CommissionValue = 0,
                Is_Item_CommissionValue = 0,
                Unit_CommissionValue = 0,
                QntyWidth = 0,
                QntyLenght = 0,
                QntyItemNo = 0,

                Audience_price = 0,
                Item_Expire_Date = null,
                Is_InvoiceItem_Kta3y = 0,
                LargItemsQnty_Free = 0,
                SmallItemsQnty_Free = 0,
                Is_Alternate_Item_CommissionValue = 0,
                Alternate_Item_CommissionValue = 0,
                Alternate_Item_CommissionRate = 0,
                Alternate_ItemCommissionTotal = 0,
                Net_Total_Item_Commission = 0,
                Item_CommissionRate = 0,
                Unit_CommissionRate = 0,
                Alternate_Unit_CommissionRate = 0,
                Alternate_Unit_CommissionValue = 0,

                Merchant_Name = "",
                Tash3ela = "",
                ItemCommissionTotal = 0,
                Item_No_Free = 0,
                StatementNo = "",
                OperationNo = "",

                Supplier_BalanceSet = 0,

                TaxSales10 = 0,
                TaxSales10_Value = 0,
                Item_TaxSales10 = 0,
                Item_TaxSales10_Value = 0,

                ATS05 = 0,

                ATS05_Value = 0,
                Item_ATS05 = 0,
                Item_ATS05_Value = 0,


                Insert_ServerDateTime = DateTime.Now,
                Update_ServerDateTime = DateTime.Now,
                EditOperationDate = DateTime.Now,

                TotalSmall_Qnty = 0,
                TotalSmall_QntyStr = "",
                TotalSmall_QntyStrColumn = "",
                S_price_LargeUnit = 0,
                S_price_SmallUnit = 0,
                Item_DeletedFlage = 0,
                Item_ProcessedFlage = 0,

                Qnty_Free = 0,
                Qnty = 0,
                Qnty_FreeStr = "",


                Larg_Qnty_Balance = 0,
                Small_Qnty_Balance = 0,

                AnotherPay = 0,



                Qnty_Another_Balance = 0,
                Is_BuyFrom_Bills = 0,


            };

            invD.Total3Mola = invD.Total3Mola ?? 0;
            // Common.Compute_3Mola_Value(ref invD);
            invD.Total = invD.Total + invD.Total3Mola ?? 0;

            invD.Item_Net_Total_Price = invD.Total;


            if ((invD.Item_Net_Total_Price < 0) | (invD.Item_Net_Total_Price == 0))
            {
                invD.Item_Net_Total_Price = 0;
            }



            if ((invD.Item_Total_WithoutTax < 0) | (invD.Item_Total_WithoutTax == 0))
            {
                invD.Item_Total_WithoutTax = 0;
            }

            return invD;
        }
    }
}
