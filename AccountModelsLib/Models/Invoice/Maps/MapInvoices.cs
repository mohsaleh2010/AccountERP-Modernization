using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public  class MapInvoices
    {
        public MapInvoices()
        {

        }

        public IInvoices_List MapSalesToInvoiceHeader(List<Unit_Item_SalesDate> salesList, IInvoices_List invLst)
        {
            if (salesList == null || salesList.Count == 0)
                throw new ArgumentException("قائمة المبيعات فارغة.");

            var first = salesList.First();

            //string Is_Invoice_AgileStr = "فاتورة أجل";

            //decimal Total = salesList.Sum(s => (s.Larg_Balance ?? 0) * (s.P_Price ?? 0));
            InvoicesClss inv = new InvoicesClss();
            long Invoice_No = inv.Make_New_Invoice_No(0);

            if (first.Customer == null)
            {
                CustomerCLS customerCLS = new CustomerCLS();
                Customer cs = customerCLS.GetCustomer(first.Customer_Id ?? 0);
                first.Customer = cs;
            }
             
             
            invLst.Invoice_Id = Invoice_No;
            invLst.Customer_Id = first.Customer_Id;
            invLst.CustomerName = first.Customer?.Name;
            invLst.SubAccount_Id = first.Customer?.SubAccount_Id ?? 0;
            invLst.SubAccount_Name = first.Customer?.Name;

            invLst.Invoice_Date = invLst.Invoice_Date;
            invLst.Invoice_DateTime = invLst.Invoice_Date;
            invLst.OperationDate = invLst.Invoice_Date;

            
            invLst.Is_Invoice_AgileStr = Common.GetInvoiceTypeAgile(invLst.Is_Invoice_Agile??1);
             //= Is_Invoice_Agile;
            invLst.Invoice_Type = Common.Invoice_Type_Id;
            invLst.User_Id = first.User_Id;
            invLst.Branch_Id = first.Branch_Id;
            invLst.Insert_ServerDateTime = DateTime.Now;
            invLst.EditOperationDate = DateTime.Now;
            invLst.Edit_User_Id = first.Edit_User_Id;
            invLst.Notes = "فاتورة ناتجة من عمليات البيع اليومية";
            invLst.GuId = Guid.NewGuid();
            invLst.Currency = Common.Currency_Name;
            invLst.Currency_id = Common.Entry_Currency_Id;

            invLst.Update_ServerDateTime = null;
            invLst.Customer_Id_RelatedToAccount = 0;
            invLst.License_Id = 0;
            invLst.Editable = 0;
            invLst.Deleted = 0;
            invLst.Deleted_Date = null;
            invLst.Control_Cusstomer_Account = 0;
            invLst.Customer_BalanceSet = 0;
            invLst.Make_Order_Purshing = 0;
            invLst.PolicaNo = "";
            invLst.troodContainerNo = "";
            invLst.Day_Registration_flage = 0;
            invLst.DAMGHAT = 0;
            invLst.commissionValue = 0;
            invLst.DayReg_List_Id = 0;
            //invLst.CustomerAccountBefor = 0;
            //invLst.CustomerAccountAfter = 0;
            invLst.Total_Item_CommissionValue = 0;
            invLst.Total_CommissionValue = 0;

            invLst.Equivalent_Net_Total = 0;
            invLst.Equivalent_CommissionValue = 0;
            invLst.Equivalent_Total = 0;
            invLst.IS_Posted_Flage = 0;
            invLst.Account_Entry_List_ID = 0;
            invLst.IsCommission_Value = 0;
            invLst.Salesman_Id = 0;
            invLst.AnotherPayment = 0;
            invLst.Is_Buy_From_Bills_Items_Balance = 0;
            invLst.IS_Make_Sarf_Item_BalanceFromInvoice = 0;
            invLst.Is_InvoiceKta3y = 0;
            invLst.Is_Tax_Per_Item = 0;
            invLst.Is_Load_Payment_OnTax = 0;
            invLst.Total_PriceWithoutTax_Load_Payment = 0;
            invLst.Total_PriceWithoutTax = 0;
            invLst.Total_ItemPriceWithoutTax = 0;
            invLst.PriceByTenns = 0;

            //invLst.List_CommissionValue = 0;
            invLst.List_CommissionRate = 0;
            ////invLst.List_Final_CommissionValue = 0;
            //invLst.Net_Total_CommissionValue = 0;
            //invLst.Equivalent_Net_Total_CommissionValue = 0;
            //invLst.Equivalent_Net_Total_Price = 0;
            invLst.ProcessNo = 0;
            invLst.Order_MoneyId = 0;
            invLst.BanksId = 0;
            invLst.BanksNamePaymentMethod = "";
            invLst.PaidValue = 0;
            invLst.RestValue = 0;
            invLst.Emp_Name = "";
            invLst.Customer_Notes = "";

            invLst.Emp_Id = 0;
            invLst.ShipmentCompany_Id = 0;
            invLst.ShipmentCompany_Name = "";
            invLst.Shipment_Value = 0;
            invLst.CarsWorkOrdersId = 0;
            invLst.WorkOrderNumber = 0;
            invLst.Is_DeletedProcessed = 0;

            invLst.Qnty_ValueWorkmanship = 0;
            invLst.Qnty_ValueSpareParts = 0;
            invLst.Qnty_ValueOils = 0;
            invLst.Qnty_ValueSparePartsOils = 0;
            invLst.Qnty_ValueRawMaterials = 0;

            invLst.CommissionListWorkmanship_Rate = 0;
            invLst.CommissionListSpareParts_Rate = 0;
            invLst.CommissionListOils_Rate = 0;
            invLst.CommissionListSparePartsOils_Rate = 0;
            invLst.CommissionListWorkmanship_Value = 0;
            invLst.CommissionListSpareParts_Value = 0;
            invLst.CommissionListOils_Value = 0;
            invLst.CommissionListSparePartsOils_Value = 0;
            invLst.Total_CommissionListWorkmanship = 0;
            invLst.Total_CommissionListSpareParts = 0;
            invLst.Total_CommissionListOils = 0;
            invLst.Total_CommissionListSparePartsOils = 0;
            invLst.Total_CommissionList = 0;

            invLst.TaxSales10 = 0;
            invLst.TaxSales10_Value = 0;
            invLst.Equivalent_TaxSales10_Value = 0;
            invLst.Equivalent_ATS05_Value = 0;

            invLst.TaxSalesListWorkmanship_Rate = 0;
            invLst.TaxSalesListSpareParts_Rate = 0;
            invLst.TaxSalesListOils_Rate = 0;
            invLst.TaxSalesListSparePartsOils_Rate = 0;
            invLst.TaxSalesListWorkmanship_Value = 0;
            invLst.TaxSalesListSpareParts_Value = 0;
            invLst.TaxSalesListOils_Value = 0;
            invLst.TaxSalesListSparePartsOils_Value = 0;
            invLst.TotalListTaxSales10_Value = 0;

            invLst.ATS05 = 0;
            invLst.ATS05_Value = 0;
            //invLst.Total_List3Mola = 0;
            invLst.ATS05ListWorkmanship_Rate = 0;
            invLst.ATS05ListSpareParts_Rate = 0;
            invLst.ATS05ListOils_Rate = 0;
            invLst.ATS05ListSparePartsOils_Rate = 0;
            invLst.ATS05ListWorkmanship_Value = 0;
            invLst.ATS05ListSpareParts_Value = 0;
            invLst.ATS05ListOils_Value = 0;
            invLst.ATS05ListSparePartsOils_Value = 0;
            invLst.TotalListATS05_Value = 0;

            invLst.Total_ListPriceWithoutTax_Workmanship = 0;
            invLst.Total_ListPriceWithoutTax_SpareParts = 0;
            invLst.Total_ListPriceWithoutTax_Oils = 0;
            invLst.Total_ListPriceWithoutTax_SparePartsOils = 0;
            invLst.Total_ListPriceWithoutTax_RawMaterial = 0;
            invLst.Total_ListPriceWithoutTax = 0;
            invLst.NetInvoiceTotal_Workmanship = 0;
            invLst.NetInvoiceTotal_SpareParts = 0;
            invLst.NetInvoiceTotal_Oils = 0;
            invLst.NetInvoiceTotal_SparePartsOils = 0;
            invLst.NetInvoiceTotal_RawMaterial = 0;

            invLst.Item_Net_TotalWorkmanship = 0;
            invLst.Item_Net_TotalSpareParts = 0;
            invLst.Item_Net_TotalOils = 0;
            invLst.Item_Net_TotalSparePartsOils = 0;
            invLst.Item_Net_TotalRawMaterials = 0;

            invLst.Total_ListPriceAfterTaxSales_Workmanship = 0;
            invLst.Total_ListPriceAfterTaxSales_SpareParts = 0;
            invLst.Total_ListPriceAfterTaxSales_Oils = 0;
            invLst.Total_ListPriceAfterTaxSales_SparePartsOils = 0;
            invLst.Total_ListPriceAfterTaxSales_RawMaterial = 0;
            invLst.Total_ListPriceAfterTaxSales = 0;

            invLst.IsInvoiceTaxInfo = 0;
            invLst.IsMaintainceInvoice = (int)InvoiceMaintainenceType.agencyWekalaKhodar;
            invLst.TaxInvoices_ListId = 0;
            invLst.TaxInternalId = "";
            invLst.TaxInvoiceNo = 0;
            invLst.IsUploadToPortal = 0;

            invLst.CarsId = 0;
            invLst.CarNumber = "";
            return invLst;
            /*
            return new Invoices_List
            {
                Invoice_Id = Invoice_No,
                Customer_Id = first.Customer_Id,
                CustomerName = first.Customer?.Name, // تأكد من Include(Customer) عند تحميل البيانات
                SubAccount_Id = first.Customer.SubAccount_Id,
                SubAccount_Name = first.Customer?.Name,

                Invoice_Date = DateTime.Now,
                Invoice_DateTime = DateTime.Now,
                OperationDate = DateTime.Now,

                Qnty_Value = salesList.Sum(s => s.QntyValue ?? 0),
                Total = Total,
                Item_Net_Total = Total,
                Net_Total = Total,
                NetInvoiceTotal = Total,
                TafkeetNet_Total = TafkeetProj.NumberToText.GetText(Total, 1),   // 
                Is_Invoice_AgileStr = Common.GetInvoiceTypeAgile(Is_Invoice_Agile),
                Is_Invoice_Agile = Is_Invoice_Agile,
                Invoice_Type = Common.Invoice_Type_Id,
                User_Id = first.User_Id,
                Branch_Id = first.Branch_Id,
                Insert_ServerDateTime = DateTime.Now,
                EditOperationDate = DateTime.Now,
                Edit_User_Id = first.Edit_User_Id,
                Notes = "فاتورة ناتجة من عمليات البيع اليومية",
                GuId = Guid.NewGuid(),
                Currency = Common.Currency_Name,
                Currency_id = Common.Entry_Currency_Id,

                Update_ServerDateTime = null,

                Customer_Id_RelatedToAccount = 0,

                License_Id = 0,
                Editable = 0,
                Deleted = 0,
                Deleted_Date = null,
                Control_Cusstomer_Account = 0,
                Customer_BalanceSet = 0,
                Make_Order_Purshing = 0,
                PolicaNo = "",
                troodContainerNo = "",
                Day_Registration_flage = 0,
                DAMGHAT = 0,
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
                AnotherPayment = 0,
                Is_Buy_From_Bills_Items_Balance = 0,

                IS_Make_Sarf_Item_BalanceFromInvoice = 0,
                Is_InvoiceKta3y = 0,
                Is_Tax_Per_Item = 0,
                Is_Load_Payment_OnTax = 0,
                Total_PriceWithoutTax_Load_Payment = 0,

                Total_PriceWithoutTax = 0,
                Total_ItemPriceWithoutTax = 0,

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
                Customer_Notes = "",

                Emp_Id = 0,
                ShipmentCompany_Id = 0,
                ShipmentCompany_Name = "",
                Shipment_Value = 0,
                CarsWorkOrdersId = 0,
                WorkOrderNumber = 0,
                Is_DeletedProcessed = 0,



                Qnty_ValueWorkmanship = 0,
                Qnty_ValueSpareParts = 0,
                Qnty_ValueOils = 0,
                Qnty_ValueSparePartsOils = 0,
                Qnty_ValueRawMaterials = 0,

                CommissionListWorkmanship_Rate = 0,
                CommissionListSpareParts_Rate = 0,
                CommissionListOils_Rate = 0,
                CommissionListSparePartsOils_Rate = 0,
                CommissionListWorkmanship_Value = 0,
                CommissionListSpareParts_Value = 0,
                CommissionListOils_Value = 0,
                CommissionListSparePartsOils_Value = 0,
                Total_CommissionListWorkmanship = 0,
                Total_CommissionListSpareParts = 0,
                Total_CommissionListOils = 0,
                Total_CommissionListSparePartsOils = 0,
                Total_CommissionList = 0,
                // List_Final_CommissionValue = 0 ,
                // Net_Total_CommissionValue = 0 ,



                TaxSales10 = 0,
                TaxSales10_Value = 0,

                Equivalent_TaxSales10_Value = 0,
                Equivalent_ATS05_Value = 0,

                TaxSalesListWorkmanship_Rate = 0,
                TaxSalesListSpareParts_Rate = 0,
                TaxSalesListOils_Rate = 0,
                TaxSalesListSparePartsOils_Rate = 0,
                TaxSalesListWorkmanship_Value = 0,
                TaxSalesListSpareParts_Value = 0,
                TaxSalesListOils_Value = 0,
                TaxSalesListSparePartsOils_Value = 0,
                TotalListTaxSales10_Value = 0,


                ATS05 = 0,
                ATS05_Value = 0,


                Total_List3Mola = 0,
                ATS05ListWorkmanship_Rate = 0,
                ATS05ListSpareParts_Rate = 0,
                ATS05ListOils_Rate = 0,
                ATS05ListSparePartsOils_Rate = 0,
                ATS05ListWorkmanship_Value = 0,
                ATS05ListSpareParts_Value = 0,
                ATS05ListOils_Value = 0,
                ATS05ListSparePartsOils_Value = 0,
                TotalListATS05_Value = 0,


                Total_ListPriceWithoutTax_Workmanship = 0,
                Total_ListPriceWithoutTax_SpareParts = 0,
                Total_ListPriceWithoutTax_Oils = 0,
                Total_ListPriceWithoutTax_SparePartsOils = 0,
                Total_ListPriceWithoutTax_RawMaterial = 0,
                Total_ListPriceWithoutTax = 0,
                NetInvoiceTotal_Workmanship = 0,
                NetInvoiceTotal_SpareParts = 0,
                NetInvoiceTotal_Oils = 0,
                NetInvoiceTotal_SparePartsOils = 0,
                NetInvoiceTotal_RawMaterial = 0,


                Item_Net_TotalWorkmanship = 0,
                Item_Net_TotalSpareParts = 0,
                Item_Net_TotalOils = 0,
                Item_Net_TotalSparePartsOils = 0,

                Item_Net_TotalRawMaterials = 0,

                Total_ListPriceAfterTaxSales_Workmanship = 0,
                Total_ListPriceAfterTaxSales_SpareParts = 0,
                Total_ListPriceAfterTaxSales_Oils = 0,
                Total_ListPriceAfterTaxSales_SparePartsOils = 0,
                Total_ListPriceAfterTaxSales_RawMaterial = 0,
                Total_ListPriceAfterTaxSales = 0,

                IsInvoiceTaxInfo = 0,
                IsMaintainceInvoice = (int)InvoiceMaintainenceType.agencyWekalaKhodar,
                TaxInvoices_ListId = 0,
                TaxInternalId = "",
                TaxInvoiceNo = 0,
                IsUploadToPortal = 0,

                CarsId = 0,
                CarNumber = "",
                
            };
            */
        }

        public Invoices_List MapSalesToInvoiceHeader(List<AppointmentService> salesList, long Customer_Id, int Is_Invoice_Agile)
        {

            //AppointmentsId = appointment.Id,
            //     Invoice_Date = DateTime.Now,
            //    Total = selectedDetails.Sum(x => x.Total),
            //    Invoices_Details = selectedDetails


            if (salesList == null || salesList.Count == 0)
                throw new ArgumentException("قائمة المبيعات فارغة.");

            var first = salesList.First();

            //string Is_Invoice_AgileStr = "فاتورة أجل";

            decimal Total = salesList.Sum(s => (s.QntyValue ?? 0) * (s.S_Price ?? 0));
            InvoicesClss inv = new InvoicesClss();
            long Invoice_No = inv.Make_New_Invoice_No(0);
            Customer cs = new Customer();
            if (Customer_Id > 0)
            {
                CustomerCLS customerCLS = new CustomerCLS();
                cs = customerCLS.GetCustomer(Customer_Id);

            }
            //CustomerCLS customerCLS = new CustomerCLS();
            //Customer cs = customerCLS.GetCustomerNoTracking(first.Customer_Id??0);
            return new Invoices_List
            {

                AppointmentsId = first.Id,



                Invoice_Id = Invoice_No,


                Customer_Id = cs?.Id ?? 0,
                CustomerName = cs?.Name ?? " ", // تأكد من Include(Customer) عند تحميل البيانات
                SubAccount_Id = cs?.SubAccount_Id ?? 0,
                SubAccount_Name = cs?.Name ?? " ",

                Invoice_Date = DateTime.Now,
                Invoice_DateTime = DateTime.Now,
                OperationDate = DateTime.Now,

                Qnty_Value = salesList.Sum(s => s.QntyValue ?? 0),
                Total = salesList.Sum(s => (s.QntyValue ?? 0) * (s.S_Price ?? 0)),
                Item_Net_Total = Total,
                Net_Total = Total,
                NetInvoiceTotal = Total,
                TafkeetNet_Total = TafkeetProj.NumberToText.GetText(Total, 1),   // 
                Is_Invoice_AgileStr = Common.GetInvoiceTypeAgile(Is_Invoice_Agile),
                Is_Invoice_Agile = Is_Invoice_Agile,
                Invoice_Type = 0,
                User_Id = Common.User_Id,
                Branch_Id = Common.Branch_Id,
                Insert_ServerDateTime = DateTime.Now,
                EditOperationDate = DateTime.Now,
                Edit_User_Id = 0,
                Notes = "فاتورة ناتجة من عمليات البيع اليومية",
                GuId = Guid.NewGuid(),
                Currency = Common.Currency_Name,
                Currency_id = Common.Entry_Currency_Id,

                Update_ServerDateTime = null,

                Customer_Id_RelatedToAccount = 0,

                License_Id = 0,
                Editable = 0,
                Deleted = 0,
                Deleted_Date = null,
                Control_Cusstomer_Account = 0,
                Customer_BalanceSet = 0,
                Make_Order_Purshing = 0,
                PolicaNo = "",
                troodContainerNo = "",
                Day_Registration_flage = 0,
                DAMGHAT = 0,
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
                AnotherPayment = 0,
                Is_Buy_From_Bills_Items_Balance = 0,

                IS_Make_Sarf_Item_BalanceFromInvoice = 0,
                Is_InvoiceKta3y = 0,
                Is_Tax_Per_Item = 0,
                Is_Load_Payment_OnTax = 0,
                Total_PriceWithoutTax_Load_Payment = 0,

                Total_PriceWithoutTax = 0,
                Total_ItemPriceWithoutTax = 0,

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
                Customer_Notes = "",

                Emp_Id = 0,
                ShipmentCompany_Id = 0,
                ShipmentCompany_Name = "",
                Shipment_Value = 0,
                CarsWorkOrdersId = 0,
                WorkOrderNumber = 0,
                Is_DeletedProcessed = 0,



                Qnty_ValueWorkmanship = 0,
                Qnty_ValueSpareParts = 0,
                Qnty_ValueOils = 0,
                Qnty_ValueSparePartsOils = 0,
                Qnty_ValueRawMaterials = 0,

                CommissionListWorkmanship_Rate = 0,
                CommissionListSpareParts_Rate = 0,
                CommissionListOils_Rate = 0,
                CommissionListSparePartsOils_Rate = 0,
                CommissionListWorkmanship_Value = 0,
                CommissionListSpareParts_Value = 0,
                CommissionListOils_Value = 0,
                CommissionListSparePartsOils_Value = 0,
                Total_CommissionListWorkmanship = 0,
                Total_CommissionListSpareParts = 0,
                Total_CommissionListOils = 0,
                Total_CommissionListSparePartsOils = 0,
                Total_CommissionList = 0,
                // List_Final_CommissionValue = 0 ,
                // Net_Total_CommissionValue = 0 ,



                TaxSales10 = 0,
                TaxSales10_Value = 0,

                Equivalent_TaxSales10_Value = 0,
                Equivalent_ATS05_Value = 0,

                TaxSalesListWorkmanship_Rate = 0,
                TaxSalesListSpareParts_Rate = 0,
                TaxSalesListOils_Rate = 0,
                TaxSalesListSparePartsOils_Rate = 0,
                TaxSalesListWorkmanship_Value = 0,
                TaxSalesListSpareParts_Value = 0,
                TaxSalesListOils_Value = 0,
                TaxSalesListSparePartsOils_Value = 0,
                TotalListTaxSales10_Value = 0,


                ATS05 = 0,
                ATS05_Value = 0,


                Total_List3Mola = 0,
                ATS05ListWorkmanship_Rate = 0,
                ATS05ListSpareParts_Rate = 0,
                ATS05ListOils_Rate = 0,
                ATS05ListSparePartsOils_Rate = 0,
                ATS05ListWorkmanship_Value = 0,
                ATS05ListSpareParts_Value = 0,
                ATS05ListOils_Value = 0,
                ATS05ListSparePartsOils_Value = 0,
                TotalListATS05_Value = 0,


                Total_ListPriceWithoutTax_Workmanship = 0,
                Total_ListPriceWithoutTax_SpareParts = 0,
                Total_ListPriceWithoutTax_Oils = 0,
                Total_ListPriceWithoutTax_SparePartsOils = 0,
                Total_ListPriceWithoutTax_RawMaterial = 0,
                Total_ListPriceWithoutTax = 0,
                NetInvoiceTotal_Workmanship = 0,
                NetInvoiceTotal_SpareParts = 0,
                NetInvoiceTotal_Oils = 0,
                NetInvoiceTotal_SparePartsOils = 0,
                NetInvoiceTotal_RawMaterial = 0,


                Item_Net_TotalWorkmanship = 0,
                Item_Net_TotalSpareParts = 0,
                Item_Net_TotalOils = 0,
                Item_Net_TotalSparePartsOils = 0,

                Item_Net_TotalRawMaterials = 0,

                Total_ListPriceAfterTaxSales_Workmanship = 0,
                Total_ListPriceAfterTaxSales_SpareParts = 0,
                Total_ListPriceAfterTaxSales_Oils = 0,
                Total_ListPriceAfterTaxSales_SparePartsOils = 0,
                Total_ListPriceAfterTaxSales_RawMaterial = 0,
                Total_ListPriceAfterTaxSales = 0,

                IsInvoiceTaxInfo = 0,
                IsMaintainceInvoice = 0,
                TaxInvoices_ListId = 0,
                TaxInternalId = "",
                TaxInvoiceNo = 0,
                IsUploadToPortal = 0,

                CarsId = 0,
                CarNumber = "",
            };
        }

        public Invoices_Details MapSalesToInvoiceDetail(Unit_Item_SalesDate sales,ref operations opp)
        {
            if (sales == null) return null;

            if(sales.Item== null)
            {
                ItemCLS itemCLS = new ItemCLS(ref opp );
                sales.Item = itemCLS.GetItemByIdNoTracking(sales.Item_Id ?? 0);
            }
            long Invoice_Id = 0;

            Invoices_Details invD = new Invoices_Details
            {
                Invoice_Id = Invoice_Id,
                AppointmentServicesId =0,
               
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
                
                Notes = "ناتج من عملية بيع يدوي", // ملاحظة اختيارية
                User_Id = sales.User_Id,
                Branch_Id = sales.Branch_Id,
                Edit_User_Id = sales.Edit_User_Id,
                ProcessNo = sales.ProcessNo,




                Small_Unit_Price = 0,
                PriceUnit = sales.P_Price,

                Unit_Used_for_Price = 1,
                Unit_Id = sales.Item.Unit_Id,
                UnitStr = sales.Item.UnitStr,
                UnitSmallStr = sales.Item.UnitStr,

                code = sales.Item.code,
                NameSpecification = sales.Item.ItemName_OnInvoice,


                Item_Notes = "ناتج من عملية بيع يدوي", //,
                QntyStr = SetQntyShow(sales),
                QntyStrColumn = SetQntyShow(sales),



                Specification = "",

                CarsWorkOrdersItemProcessedDetailsId = 0,



                Make_Order = 0,
                Balance_Checked = 0,

                Order_Purshing_Id_Automatic = 0,
                Store_Id = 0,
                Store_Name = "",

                Item_CommissionValue = 0,
                Is_Item_CommissionValue = 0,
                Unit_CommissionValue = 0,
                QntyWidth = 0,
                QntyLenght = 0,
                QntyItemNo = 0,
                Bills_Details_Id = 0,
                Purchase_Price = 0,
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

                Available_Larg_Balance = 0,
                Available_Small_Balance = 0,
                Larg_Qnty_Balance = 0,
                Small_Qnty_Balance = 0,

                Unit3Mola = sales.Unit3Mola,
                Total3Mola = sales.Total3Mola,


            };

            invD.Total3Mola = invD.Total3Mola ?? 0;
            Common.Compute_3Mola_Value(ref invD);
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

        public Invoices_Details MapSalesToInvoiceDetail(AppointmentService sales, ref operations opp)
        {
            if (sales == null) return null;

            long Invoice_Id = 0;

            ItemCLS itemCLS = new ItemCLS(ref opp);
            Items itm = itemCLS.GetItemByIdNoTracking(sales.ServiceItem_Id);
            Invoices_Details invD = new Invoices_Details
            { 
                Invoice_Id = Invoice_Id,
                AppointmentServicesId = sales.Id,
                Unit_Item_SalesDateId = 0,
                Item_Id = sales.ServiceItem_Id,
                Unit_Item_Table_Id = sales.Unit_Item_Table_Id,
                Larg_Qnty = sales.Qnty,
                Small_Qnty = 0,
                Qnty_Another = 0,
                Larg_Unit_Price = sales.S_Price,
                Qnty_value = sales.QntyValue,
                Total = sales.QntyValue,
                Item_Total_WithoutTax = sales.QntyValue,
                Item_Net_Total_Price = sales.QntyValue,
                OperationDate = DateTime.Now,
                Unit_Item_ExpireDate_Id = 0,
                Invoice_Item_Name = sales.ServiceItem_Name ?? " ",
                Unit3Mola = 0,
                Total3Mola = 0,

                Notes = "ناتج من عملية بيع يدوي", // ملاحظة اختيارية
                User_Id = Common.User_Id,
                Branch_Id = Common.Branch_Id,
                Edit_User_Id = 0,
                ProcessNo = 0,




                Small_Unit_Price = 0,
                PriceUnit = sales.S_Price,

                Unit_Used_for_Price = 1,
                Unit_Id = itm.Unit_Id,
                UnitStr = itm.UnitStr,
                UnitSmallStr = itm.UnitStr,

                code = itm.code,
                NameSpecification = itm.ItemName_OnInvoice,


                Item_Notes = "ناتج من عملية بيع يدوي", //,
                QntyStr = SetQntyShow(sales, itm),
                QntyStrColumn = SetQntyShow(sales, itm),



                Specification = "",

                CarsWorkOrdersItemProcessedDetailsId = 0,



                Make_Order = 0,
                Balance_Checked = 0,

                Order_Purshing_Id_Automatic = 0,
                Store_Id = 0,
                Store_Name = "",

                Item_CommissionValue = 0,
                Is_Item_CommissionValue = 0,
                Unit_CommissionValue = 0,
                QntyWidth = 0,
                QntyLenght = 0,
                QntyItemNo = 0,
                Bills_Details_Id = 0,
                Purchase_Price = 0,
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

                Available_Larg_Balance = 0,
                Available_Small_Balance = 0,
                Larg_Qnty_Balance = 0,
                Small_Qnty_Balance = 0,



            };

            invD.Total3Mola = invD.Total3Mola ?? 0;
            Common.Compute_3Mola_Value(ref invD);
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
        string SetQntyShow(AppointmentService sales, Items itm)
        {
            string QntyStr = "";
            //unitCLS unitCLS = new unitCLS();
            //unit un = unitCLS.GetUnitByIdAsNoTracking(sales.Item.Unit_Id ?? 0);


            QntyStr = Common.GetQntyString(itm.unit.S_unit_Value, sales.Qnty, itm.unit.B_Unit.ToString(), 0, itm.unit.S_unit.ToString());


            //QntyStr = Common.GetQntyLenghts_Str(QntyStr, "0".ToString(), carWorkOrderDetail.QntyLenght.ToString(), carWorkOrderDetail.QntyItemNo.ToString());
            return QntyStr;
        }
    }
}
