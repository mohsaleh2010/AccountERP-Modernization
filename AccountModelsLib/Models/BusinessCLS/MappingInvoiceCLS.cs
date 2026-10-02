using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class MappingInvoiceCLS
    {
        public List<InvoiceDetailsViewModel> GetInvoices_DetailsViewModel(ref operations opp, List<Invoices_Details> invDs, Invoices_List invL, Items itm)
        {
            InvoiceListViewModel inLVM = new InvoiceListViewModel();
            List<InvoiceDetailsViewModel> invDVM_List = new List<InvoiceDetailsViewModel>();
            int Serial = 1;
            foreach (Invoices_Details inv in invDs)
            {
                InvoiceDetailsViewModel invDVM = new InvoiceDetailsViewModel();
                invDVM.Item_DeletedFlage = inv.Item_DeletedFlage;
                Serial++;
                invDVM.Serial = Serial;
                invDVM.Item_Name = inv.Invoice_Item_Name;
                invDVM.Invoice_Item_Name = inv.Invoice_Item_Name;
                invDVM.Specification = inv.Specification;


                invDVM.Code = inv.code;


                invDVM.Item_Net_Total_Price = inv.Item_Net_Total_Price;
                invDVM.Item_TaxSales10_Value = inv.Item_TaxSales10_Value;
                invDVM.Item_TaxSales10 = inv.Item_TaxSales10;
                invDVM.Item_ATS05_Value = inv.Item_ATS05_Value;
                invDVM.Item_ATS05 = inv.Item_ATS05;
                invDVM.Item_Total_WithoutTax = inv.Item_Total_WithoutTax;



                invDVM.Net_Total_Item_Commission = inv.Net_Total_Item_Commission;

                invDVM.Alternate_ItemCommissionTotal = inv.Alternate_ItemCommissionTotal;
                invDVM.Alternate_Item_CommissionValue = inv.Alternate_Item_CommissionValue;
                invDVM.Alternate_Unit_CommissionValue = inv.Alternate_Unit_CommissionValue;
                invDVM.Alternate_Item_CommissionRate = inv.Alternate_Item_CommissionRate;


                invDVM.ItemCommissionTotal = inv.ItemCommissionTotal;
                invDVM.Item_CommissionValue = inv.Item_CommissionValue;
                invDVM.Unit_CommissionValue = inv.Unit_CommissionValue;
                invDVM.Item_CommissionRate = inv.Item_CommissionRate;

                invDVM.Qnty_Value = inv.Qnty_value;

                invDVM.Larg_Unit_Price = inv.Larg_Unit_Price;

                invDVM.OperationNo = inv.OperationNo;
                invDVM.Merchant_Name = inv.Merchant_Name;

                invDVM.StatementNo = inv.StatementNo;

                invDVM.Larg_Qnty = inv.Larg_Qnty;

                invDVM.Small_Qnty = inv.Small_Qnty;


                invDVM.TotalSmall_QntyStr = inv.TotalSmall_QntyStr;
                invDVM.Qnty_Another = inv.Qnty_Another;

                invDVM.QntyStr = inv.QntyStr;

                //invDVM.Audience_price,
                invDVM.LargItemsQnty_Free = inv.LargItemsQnty_Free;
                invDVM.SmallItemsQnty_Free = inv.SmallItemsQnty_Free;
                invDVM.Item_No_Free = inv.Item_No_Free;

                invDVM.Item_Expire_Date = inv.Item_Expire_Date;
                invDVM.Tash3ela = inv.Tash3ela; ;

                invDVM.QntyLenght = inv.QntyLenght;
                invDVM.QntyWidth = inv.QntyWidth;
                invDVM.QntyItemNo = inv.QntyItemNo;



                invDVM.Store_Name = itm.Store.Store_Name;

                invDVM.Qnty_Free = inv.Qnty_Free;
                invDVM.Qnty_FreeStr = inv.Qnty_FreeStr;

                invDVM.Qnty = inv.Qnty;

                if (itm == null)
                {
                    itm = opp.db.Item.Where(i => i.Id == inv.Item_Id).Include(i => i.Store).FirstOrDefault();

                }
                invDVM.Is_InvoiceKta3y = invL.Is_InvoiceKta3y;
                if (invL.Is_InvoiceKta3y == 1)
                {
                    try
                    {
                        

                        invDVM.Items_Types_Name = itm.Items_Types_Name;
                        invDVM.Somk = itm.Somk;
                        invDVM.Somk_Unit = itm.Somk_Unit;





                        invDVM.Invoice_Item_Name = invDVM.Items_Types_Name + " " + invDVM.Somk + " " + invDVM.Somk_Unit;
                        //19.94999cm
                        //dt.Rows[x]["Size_"] = Common.ToIndicDigits(QntyLenght) + "  X  " + Common.ToIndicDigits(QntyWidth);
                        invDVM.Item_Name = invDVM.Invoice_Item_Name.ToString();
                        invDVM.Specification = Common.ToIndicDigits(inv.QntyLenght.Value.ToString()) + "  *  " + Common.ToIndicDigits(inv.QntyWidth.Value.ToString());
                        invDVM.Qnty_Another = invDVM.QntyItemNo;
                    }
                    catch (Exception ee)
                    { }


                    invDVM.Item_Name = invDVM.Item_Name.ToString()+ invDVM.Specification;

                    

                }
                else
                {
                    invDVM.Item_Name = invDVM.Item_Name.ToString() + invDVM.Specification;
                }

                invDVM_List.Add(invDVM);

                 ////////Totals

                try
                {
                    inLVM.Total_Qnty_Value = inLVM.Total_Qnty_Value + inv.Qnty_value ;
                }
                catch { }
                invL.Total_Item_CommissionValue = invL.Total_Item_CommissionValue + inv.Net_Total_Item_Commission ;
                try
                {
                    inLVM.Total_Item_TaxSales10_Value = inLVM.Total_Item_TaxSales10_Value + inv.Item_TaxSales10_Value ;
                }
                catch { }
                try
                {
                    inLVM.Total_Item_ATS05_Value = inLVM.Total_Item_ATS05_Value + inv.Item_ATS05_Value ;
                }
                catch { }
                try
                {
                    inLVM.Total_PriceWithoutTax = inLVM.Total_PriceWithoutTax + inv.Item_Total_WithoutTax ;
                }
                catch { }
                inLVM.Total_Item_Net_Total_Price = inLVM.Total_Item_Net_Total_Price + inv.Item_Net_Total_Price ;
                try
                {
                    inLVM.Total_Item_No = inLVM.Total_Item_No + inv.Qnty_Another ;
                }
                catch { }



            }
            inLVM.Total_Item_Net_Total_Price = Common.RoundMePositive(inLVM.Total_Item_Net_Total_Price);
            inLVM.Total_PriceWithoutTax = Common.RoundMePositive(inLVM.Total_PriceWithoutTax);
            inLVM.Total_Item_CommissionValue = Common.RoundMePositive(inLVM.Total_Item_CommissionValue);

            inLVM.Total_Item_ATS05_Value = Common.RoundMePositive(inLVM.Total_Item_ATS05_Value);
            inLVM.Total_Item_TaxSales10_Value = Common.RoundMePositive(inLVM.Total_Item_TaxSales10_Value);

            return invDVM_List;
        }

        /*  public List<InvoiceDetailsViewModel> GetInvoices_DetailsViewModel(List<Invoices_Details> invDs)
        {
            List<InvoiceDetailsViewModel> invDVM_List = new List<InvoiceDetailsViewModel>();

            foreach (Invoices_Details inv in invDs)
            {
                InvoiceDetailsViewModel invDVM = new InvoiceDetailsViewModel();
                 
                invDVM.Serial = 1;
                invDVM.Item_Name = inv.Invoice_Item_Name;
                 
                invDVM.Specification = inv.Specification;

                invDVM.Items_Types_Name = inv.Items_Types_Name;
                invDVM.Code = inv.code;
                invDVM.Store_Name = inv.Store_Name;


                

                invDVM.Qnty_Value = inv.Qnty_Value;

                


              
                invDVM.Qnty = inv.Qnty;
                

                //invDVM.Audience_price,
                 
                invDVM.Qnty_Free = inv.Qnty_Free;
                invDVM.QntyStr_Free = inv.QntyStr_Free;
                
                invDVM.Somk = inv.Somk;
                invDVM.Somk_Unit = inv.Somk_Unit;
                
                invDVM.Is_InvoiceKta3y = inv.Is_InvoiceKta3y;



                invDVM_List.Add(invDVM);



                invDVM.Id = inv.Id;
                invDVM.Invoice_Id = inv.Invoice_Id;
                invDVM.Item_Id = inv.Item_Id;
                invDVM.Unit_Item_Table_Id = inv.Unit_Item_Table_Id;
                invDVM.Bills_Details_Id = inv.Bills_Details_Id;
                invDVM.Unit_Item_ExpireDate_Id = inv.Unit_Item_ExpireDate_Id;
                invDVM.Purchase_Price = inv.Purchase_Price;
                invDVM.Audience_price = inv.Audience_price;

                invDVM.Larg_Qnty = inv.Larg_Qnty;
                invDVM.Small_Qnty = inv.Small_Qnty;
                invDVM.Qnty_Another = inv.Qnty_Another;
                invDVM.Qnty_value = inv.Qnty_value;
                invDVM.ATS05 = inv.ATS05;
invDVM.TaxSales10 = inv.TaxSales10
invDVM.ATS05_Value = inv.ATS05_Value
invDVM.TaxSales10_Value = inv.TaxSales10_Value;
                invDVM.Total = inv.Total;
invDVM.Notes = inv.Notes
invDVM.Unit_Id = inv.Unit_Id;

                invDVM.Make_Order = inv.Make_Order;
                invDVM.Balance_Checked = inv.Balance_Checked;
                invDVM.Invoice_Item_Name = inv.Invoice_Item_Name;
                invDVM.Order_Purshing_Id_Automatic = inv.Order_Purshing_Id_Automatic;
invDVM.Store_Id = inv.Store_Id;
                invDVM.NameSpecification = inv.NameSpecification;
                invDVM.Item_CommissionValue = inv.Item_CommissionValue;
                invDVM.Is_Item_CommissionValue = inv.Is_Item_CommissionValue;
                invDVM.Unit_CommissionValue = inv.Unit_CommissionValue;
                invDVM.QntyWidth = inv.QntyWidth;
                invDVM.QntyLenght = inv.QntyLenght;
invDVM.QntyItemNo = inv.QntyItemNo;
                invDVM.Item_Expire_Date = inv.Item_Expire_Date;
                invDVM.Is_InvoiceItem_Kta3y = inv.Is_InvoiceItem_Kta3y;
                invDVM.LargItemsQnty_Free = inv.LargItemsQnty_Free;
                invDVM.SmallItemsQnty_Free = inv.SmallItemsQnty_Free;
                invDVM.Is_Alternate_Item_CommissionValue = inv.Is_Alternate_Item_CommissionValue;
                invDVM.Alternate_Item_CommissionValue = inv.Alternate_Item_CommissionValue;
                invDVM.Alternate_Item_CommissionRate = inv.Alternate_Unit_CommissionRate;
                invDVM.Alternate_ItemCommissionTotal = inv.Alternate_ItemCommissionTotal;
invDVM.Net_Total_Item_Commission = inv.Net_Total_Item_Commission;
                invDVM.Item_CommissionRate = inv.Item_CommissionRate;
                invDVM.Unit_CommissionRate = inv.Unit_CommissionRate;
                invDVM.Alternate_Unit_CommissionRate = inv.Alternate_Unit_CommissionRate;
                invDVM.Alternate_Unit_CommissionValue = inv.Alternate_Unit_CommissionValue;
                invDVM.Merchant_Name = inv.Merchant_Name;
                invDVM.Tash3ela = inv.Tash3ela;
                invDVM.ItemCommissionTotal = inv.ItemCommissionTotal
invDVM.Item_No_Free = inv.Item_No_Free;
                invDVM.StatementNo = inv.StatementNo;
                invDVM.OperationNo = inv.OperationNo;
                invDVM.Item_Total_WithoutTax = inv.Item_Total_WithoutTax;
                invDVM.Supplier_BalanceSet = inv.Supplier_BalanceSet;
invDVM.Item_ATS05 = inv.Item_ATS05;
                invDVM.Item_ATS05_Value = inv.Item_ATS05_Value;
                invDVM.Item_TaxSales10 = inv.Item_TaxSales10;
invDVM.Item_TaxSales10_Value = inv.Item_TaxSales10_Value;
                invDVM.Item_Net_Total_Price = inv.Item_Net_Total_Price;
                invDVM.Item_Notes = inv.Item_Notes;
                invDVM.QntyStr = inv.QntyStr;
                invDVM.User_Id = inv.User_Id;
                invDVM.Branch_Id = inv.Branch_Id;
                invDVM.Insert_ServerDateTime = inv.Insert_ServerDateTime;
                invDVM.Update_ServerDateTime = inv.Update_ServerDateTime;
                invDVM.EditOperationDate = inv.EditOperationDate;
                invDVM.Edit_User_Id = inv.Edit_User_Id;
                invDVM.ProcessNo = inv.ProcessNo;
                invDVM.QntyStrColumn = inv.QntyStrColumn;
                invDVM.OperationDate = inv.OperationDate;
                invDVM.TotalSmall_Qnty = inv.TotalSmall_Qnty;
                invDVM.TotalSmall_QntyStr = inv.TotalSmall_QntyStr;
                invDVM.TotalSmall_QntyStrColumn = inv.TotalSmall_QntyStrColumn;
                invDVM.S_price_LargeUnit = inv.S_price_LargeUnit;
                invDVM.S_price_SmallUnit = inv.S_price_SmallUnit;
                invDVM.Item_DeletedFlage = inv.Item_DeletedFlage;
                invDVM.Item_ProcessedFlage = inv.Item_ProcessedFlage;
                invDVM.Larg_Unit_Price = inv.Larg_Unit_Price;
                invDVM.Small_Unit_Price = inv.Small_Unit_Price;
                invDVM.Unit_Used_for_Price = inv.Unit_Used_for_Price;
            }

            return invDVM_List;
        }
  */
    }
}
