using AccountModelsLib.Models;
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace AccountModelsLib.Models
{
    public class InvoiceValuesCLS
    {
        //Invoices_ListCls inv_ListCls;
        //public Invoices_List invList = new Invoices_List();
        //public List<Invoices_Details> invDs = new List<Invoices_Details>();
        // public  operations opp;
        public InvoiceValuesCLS()
        {
            // opp = new operations();
            //inv_ListCls = new Invoices_ListCls(ref opp);
        }
        public InvoiceValuesCLS(ref operations opp_)
        {
            // opp = opp_;
            // inv_ListCls = new Invoices_ListCls(ref opp);
        }

        public void SetValuesDecimals(ref Invoices_List invL)
        {
            invL.Item_Net_TotalWorkmanship = Common.RoundMePositive(invL.Item_Net_TotalWorkmanship);
            invL.CommissionListWorkmanship_Rate = Common.RoundMePositive(invL.CommissionListWorkmanship_Rate);
            invL.Item_Net_TotalSpareParts = Common.RoundMePositive(invL.Item_Net_TotalSpareParts);
            invL.Item_Net_TotalOils = Common.RoundMePositive(invL.Item_Net_TotalOils);
            invL.Item_Net_TotalSparePartsOils = Common.RoundMePositive(invL.Item_Net_TotalSparePartsOils);
            invL.CommissionListSpareParts_Rate = Common.RoundMePositive(invL.CommissionListSpareParts_Rate);
            invL.CommissionListOils_Rate = Common.RoundMePositive(invL.CommissionListOils_Rate);
            ////+
            invL.CommissionListSparePartsOils_Rate = Common.RoundMePositive(invL.CommissionListSparePartsOils_Rate);
            invL.CommissionListWorkmanship_Value = Common.RoundMePositive(invL.CommissionListWorkmanship_Value);
            invL.CommissionListSpareParts_Value = Common.RoundMePositive(invL.CommissionListSpareParts_Value);
            invL.CommissionListOils_Value = Common.RoundMePositive(invL.CommissionListOils_Value);
            invL.CommissionListSparePartsOils_Value = Common.RoundMePositive(invL.CommissionListSparePartsOils_Value);

            invL.Total = Common.RoundMePositive(invL.Total);


            invL.Net_Total = Common.RoundMePositive(invL.Net_Total);



            invL.List_CommissionRate = Common.RoundMePositive(invL.List_CommissionRate);

            invL.Total_CommissionListWorkmanship = Common.RoundMePositive(invL.Total_CommissionListWorkmanship);
            invL.Total_CommissionListSpareParts = Common.RoundMePositive(invL.Total_CommissionListSpareParts);
            invL.Total_CommissionListOils = Common.RoundMePositive(invL.Total_CommissionListOils);
            invL.Total_CommissionListSparePartsOils = Common.RoundMePositive(invL.Total_CommissionListSparePartsOils);
            invL.Total_CommissionList = Common.RoundMePositive(invL.Total_CommissionList);

            invL.List_CommissionValue = Common.RoundMePositive(invL.List_CommissionValue);
            invL.List_Final_CommissionValue = Common.RoundMePositive(invL.List_Final_CommissionValue);

            ////////////
            invL.Net_Total_CommissionValue = Common.RoundMePositive(invL.Net_Total_CommissionValue);
            invL.Total_ListPriceWithoutTax_Workmanship = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_Workmanship);
            invL.Total_ListPriceWithoutTax_SpareParts = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_SpareParts);
            invL.Total_ListPriceWithoutTax_Oils = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_Oils);
            invL.Total_ListPriceWithoutTax_SparePartsOils = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_SparePartsOils);
            invL.Total_ListPriceWithoutTax = Common.RoundMePositive(invL.Total_ListPriceWithoutTax);
            invL.Total_PriceWithoutTax = Common.RoundMePositive(invL.Total_PriceWithoutTax);



            invL.ATS05ListWorkmanship_Value = Common.RoundMePositive(invL.ATS05ListWorkmanship_Value);
            invL.ATS05ListSpareParts_Value = Common.RoundMePositive(invL.ATS05ListSpareParts_Value);
            invL.ATS05ListOils_Value = Common.RoundMePositive(invL.ATS05ListOils_Value);
            ////+
            invL.ATS05ListSparePartsOils_Value = Common.RoundMePositive(invL.ATS05ListSparePartsOils_Value);

            invL.TotalListATS05_Value = Common.RoundMePositive(invL.TotalListATS05_Value);

            invL.ATS05_Value = Common.RoundMePositive(invL.ATS05_Value);
            invL.TaxSales10 = Common.RoundMePositive(invL.TaxSales10);
            invL.ATS05 = Common.RoundMePositive(invL.ATS05);


            invL.TaxSalesListWorkmanship_Value = Common.RoundMePositive(invL.TaxSalesListWorkmanship_Value);
            invL.TaxSalesListSpareParts_Value = Common.RoundMePositive(invL.TaxSalesListSpareParts_Value);
            invL.TaxSalesListOils_Value = Common.RoundMePositive(invL.TaxSalesListOils_Value);
            invL.TaxSalesListSparePartsOils_Value = Common.RoundMePositive(invL.TaxSalesListSparePartsOils_Value);
            invL.TotalListTaxSales10_Value = Common.RoundMePositive(invL.TotalListTaxSales10_Value);
            invL.TaxSales10_Value = Common.RoundMePositive(invL.TaxSales10_Value);

            ////////
            invL.NetInvoiceTotal_Workmanship = Common.RoundMePositive(invL.NetInvoiceTotal_Workmanship);
            invL.NetInvoiceTotal_SpareParts = Common.RoundMePositive(invL.NetInvoiceTotal_SpareParts);
            invL.NetInvoiceTotal_Oils = Common.RoundMePositive(invL.NetInvoiceTotal_Oils);
            invL.NetInvoiceTotal_SparePartsOils = Common.RoundMePositive(invL.NetInvoiceTotal_SparePartsOils);
            invL.NetInvoiceTotal = Common.RoundMePositive(invL.NetInvoiceTotal);

            invL.Equivalent_Net_Total_Price = Common.RoundMePositive(invL.Equivalent_Net_Total_Price);
            invL.Equivalent_Net_Total_CommissionValue = Common.RoundMePositive(invL.Equivalent_Net_Total_CommissionValue);


        }
        public void GetItemProcessedListTotals(Invoices_List invL, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            invL.Qnty_ValueWorkmanship = 0;
            invL.Qnty_ValueSpareParts = 0;
            invL.Qnty_ValueOils = 0;
            invL.Qnty_ValueSparePartsOils = 0;

            invL.Item_Net_TotalSpareParts = 0;
            invL.Item_Net_TotalOils = 0;
            invL.Item_Net_TotalWorkmanship = 0;
            invL.Total_ItemPriceWithoutTax = 0;
            invL.Net_Total = 0;
            invL.Total_Item_CommissionValue = 0;



            foreach (var item in ItemProcessedList)
            {
                item.Qnty_value = item.Qnty_value ?? 0;
                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Item_Total_WithoutTax = item.Item_Total_WithoutTax ?? 0;
                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Net_Total_Item_Commission = item.Net_Total_Item_Commission ?? 0;


                ///قطع غيار
                if (item.Items.Item_Category_ID == 1)
                {
                    invL.Qnty_ValueSpareParts = item.Qnty_value + invL.Qnty_ValueSpareParts;
                    invL.Item_Net_TotalSpareParts = item.Item_Net_Total_Price + invL.Item_Net_TotalSpareParts;

                }
                ///زيوت 
                if (item.Items.Item_Category_ID == 2)
                {
                    invL.Qnty_ValueOils = item.Qnty_value + invL.Qnty_ValueOils;
                    invL.Item_Net_TotalOils = item.Item_Net_Total_Price + invL.Item_Net_TotalOils;


                }
                ///مصنعيات 
                if (item.Items.Item_Category_ID == 3)
                {
                    invL.Qnty_ValueWorkmanship = item.Qnty_value + invL.Qnty_ValueWorkmanship;
                    invL.Item_Net_TotalWorkmanship = item.Item_Net_Total_Price + invL.Item_Net_TotalWorkmanship;

                }
                //////+
                invL.Qnty_ValueSparePartsOils = invL.Qnty_ValueSpareParts + invL.Qnty_ValueOils;


                invL.Total_ItemPriceWithoutTax = invL.Total_ItemPriceWithoutTax + item.Item_Total_WithoutTax;
                invL.Net_Total = invL.Net_Total + item.Item_Net_Total_Price;
                invL.Total_Item_CommissionValue = invL.Total_Item_CommissionValue + item.Net_Total_Item_Commission;
            }


        }
        public void ComputeTotals(Invoices_List invL, List<CarsWorkOrdersItemProcessedDetail> ItemsP)
        {
            invL.Item_Net_TotalSpareParts = invL.Item_Net_TotalSpareParts ?? 0;
            invL.Item_Net_TotalOils = invL.Item_Net_TotalOils ?? 0;
            invL.Item_Net_TotalSparePartsOils = invL.Item_Net_TotalSparePartsOils ?? 0;
            //checked nulls
            invL.Item_Net_TotalWorkmanship = invL.Item_Net_TotalWorkmanship ?? 0;
            invL.CommissionListWorkmanship_Rate = invL.CommissionListWorkmanship_Rate ?? 0;

            invL.CommissionListSpareParts_Rate = invL.CommissionListSpareParts_Rate ?? 0;
            invL.CommissionListOils_Rate = invL.CommissionListOils_Rate ?? 0;
            ////+
            invL.CommissionListSparePartsOils_Rate = invL.CommissionListSparePartsOils_Rate ?? 0;


            invL.CommissionListWorkmanship_Value = invL.CommissionListWorkmanship_Value ?? 0;

            invL.CommissionListSpareParts_Value = invL.CommissionListSpareParts_Value ?? 0;
            invL.CommissionListOils_Value = invL.CommissionListOils_Value ?? 0;
            ////+
            invL.CommissionListSparePartsOils_Value = invL.CommissionListSparePartsOils_Value ?? 0;
            /////Sales totales
            ///From Item Detail
            //    invL.Qnty_ValueWorkmanship
            //invL.Qnty_ValueSpareParts
            //invL.Qnty_ValueOils
            invL.Total = invL.Qnty_ValueOils + invL.Qnty_ValueSpareParts + invL.Qnty_ValueWorkmanship;
            invL.Qnty_Value = invL.Total;


            ////Net totals
            ///From Item Detail
            //invL.Item_Net_TotalWorkmanship
            //invL.Item_Net_TotalSpareParts
            //invL.Item_Net_TotalOils
            invL.Item_Net_Total = invL.Item_Net_TotalOils + invL.Item_Net_TotalSpareParts + invL.Item_Net_TotalWorkmanship;
            invL.Net_Total = invL.Item_Net_Total;// invL.Item_Net_TotalOils + invL.Item_Net_TotalSpareParts + invL.Item_Net_TotalWorkmanship;


            ///From invoice  Form  
            //    invL.CommissionListWorkmanship_Rate
            //invL.CommissionListSpareParts_Rate
            //invL.CommissionListOils_Rate
            decimal? CommissionListWorkmanship_ValueOfRate = invL.Item_Net_TotalWorkmanship * invL.CommissionListWorkmanship_Rate / 100;
            decimal? CommissionListSpareParts_ValueOfRate = invL.Item_Net_TotalSpareParts * invL.CommissionListSpareParts_Rate / 100;
            decimal? CommissionListOils_ValueOfRate = invL.Item_Net_TotalOils * invL.CommissionListOils_Rate / 100;
            decimal? CommissionListSparePartsOils_ValueOfRate = CommissionListOils_ValueOfRate + CommissionListSpareParts_ValueOfRate;
            decimal? Total_CommissionList_ValueOfRate = CommissionListWorkmanship_ValueOfRate + CommissionListSpareParts_ValueOfRate + CommissionListOils_ValueOfRate;
            invL.List_CommissionRate = invL.CommissionListWorkmanship_Rate + invL.CommissionListSpareParts_Rate + invL.CommissionListOils_Rate;
            ///From invoice  Form  
            //    invL.CommissionListWorkmanship_Value
            //invL.CommissionListSpareParts_Value
            //invL.CommissionListOils_Value
            decimal? TotalCommissionList_Value = invL.CommissionListWorkmanship_Value + invL.CommissionListSpareParts_Value + invL.CommissionListOils_Value;

            invL.Total_CommissionListWorkmanship = invL.CommissionListWorkmanship_Value + CommissionListWorkmanship_ValueOfRate;
            invL.Total_CommissionListSpareParts = invL.CommissionListSpareParts_Value + CommissionListSpareParts_ValueOfRate;
            invL.Total_CommissionListOils = invL.CommissionListOils_Value + CommissionListOils_ValueOfRate;
            ////+
            invL.Total_CommissionListSparePartsOils = invL.CommissionListSparePartsOils_Value + CommissionListSparePartsOils_ValueOfRate;
            invL.Total_CommissionList = invL.Total_CommissionListOils + invL.Total_CommissionListSpareParts + invL.Total_CommissionListWorkmanship;
            //invL.List_Final_CommissionValue  
            //invL.Net_Total_CommissionValue  
            invL.List_CommissionValue = TotalCommissionList_Value ?? 0;
            invL.List_Final_CommissionValue = invL.Total_CommissionList ?? 0;

            ////////////
            invL.Net_Total_CommissionValue = invL.Total_CommissionList + Total_CommissionList_ValueOfRate;

            invL.Total_ListPriceWithoutTax_Workmanship = invL.Qnty_ValueWorkmanship - invL.Total_CommissionListWorkmanship;
            invL.Total_ListPriceWithoutTax_SpareParts = invL.Qnty_ValueSpareParts - invL.Total_CommissionListSpareParts;
            invL.Total_ListPriceWithoutTax_Oils = invL.Qnty_ValueOils - invL.Total_CommissionListOils;
            ///////+
            invL.Total_ListPriceWithoutTax_SparePartsOils = invL.Total_ListPriceWithoutTax_SpareParts + invL.Total_ListPriceWithoutTax_Oils;

            invL.Total_ListPriceWithoutTax = invL.Total_ListPriceWithoutTax_Oils + invL.Total_ListPriceWithoutTax_SpareParts + invL.Total_ListPriceWithoutTax_Workmanship;

            invL.Total_PriceWithoutTax = invL.Total_ListPriceWithoutTax;// + invL.Total_ItemPriceWithoutTax;





            ///From invoice  Form  
            //    invL.ATS05ListWorkmanship_Rate
            //invL.txtATS05ListSpareParts_Rate
            //invL.ATS05ListOils_Rate

            invL.ATS05ListWorkmanship_Value = invL.Total_ListPriceWithoutTax_Workmanship * invL.ATS05ListWorkmanship_Rate / 100;
            invL.ATS05ListSpareParts_Value = invL.Total_ListPriceWithoutTax_SpareParts * invL.ATS05ListSpareParts_Rate / 100;
            invL.ATS05ListOils_Value = invL.Total_ListPriceWithoutTax_Oils * invL.ATS05ListOils_Rate / 100;

            ////+
            invL.ATS05ListSparePartsOils_Value = invL.ATS05ListSpareParts_Value + +invL.ATS05ListOils_Value;

            //invL.ATS05ListSparePartsOils_Value = invL.ATS05ListOils_Value + invL.ATS05ListSpareParts_Value;


            ////+
            //invL.ATS05ListSparePartsOils_Value = invL.ATS05ListOils_Value + invL.ATS05ListSpareParts_Value;

            invL.TotalListATS05_Value = invL.ATS05ListOils_Value + invL.ATS05ListSpareParts_Value + invL.ATS05ListWorkmanship_Value;

            invL.ATS05_Value = invL.TotalListATS05_Value;
            invL.TaxSales10 = 0;
            invL.ATS05 = 0;
            ///From invoice  Form  
            //    invL.TaxSalesListWorkmanship_Rate
            //invL.TaxSalesListSpareParts_Rate
            //invL.TaxSalesListOils_Rate

            invL.TaxSalesListWorkmanship_Value = invL.Total_ListPriceWithoutTax_Workmanship * invL.TaxSalesListWorkmanship_Rate / 100;
            invL.TaxSalesListSpareParts_Value = invL.Total_ListPriceWithoutTax_SpareParts * invL.TaxSalesListSpareParts_Rate / 100;
            invL.TaxSalesListOils_Value = invL.Total_ListPriceWithoutTax_Oils * invL.TaxSalesListOils_Rate / 100;
            ////+
            invL.TaxSalesListSparePartsOils_Value = invL.TaxSalesListSpareParts_Value + invL.TaxSalesListOils_Value;
            invL.TotalListTaxSales10_Value = invL.TaxSalesListOils_Value + invL.TaxSalesListSpareParts_Value + invL.TaxSalesListWorkmanship_Value;
            invL.TaxSales10_Value = invL.TotalListTaxSales10_Value;


            ////Total After  Only  Taxe Sales
            ////////
            invL.Total_ListPriceAfterTaxSales_Workmanship = invL.Total_ListPriceWithoutTax_Workmanship + invL.TaxSalesListWorkmanship_Value;
            invL.Total_ListPriceAfterTaxSales_SpareParts = invL.Total_ListPriceWithoutTax_SpareParts + invL.TaxSalesListSpareParts_Value;
            invL.Total_ListPriceAfterTaxSales_Oils = invL.Total_ListPriceWithoutTax_Oils + invL.TaxSalesListOils_Value;
            ////+
            invL.Total_ListPriceAfterTaxSales_SparePartsOils = invL.Total_ListPriceAfterTaxSales_SpareParts + invL.Total_ListPriceAfterTaxSales_Oils;

            invL.Total_ListPriceAfterTaxSales = invL.Total_ListPriceAfterTaxSales_Workmanship + invL.Total_ListPriceAfterTaxSales_SpareParts + invL.Total_ListPriceAfterTaxSales_Oils;



            ////Total After All Taxes
            ////////
            invL.NetInvoiceTotal_Workmanship = invL.Total_ListPriceWithoutTax_Workmanship + invL.TaxSalesListWorkmanship_Value - invL.ATS05ListWorkmanship_Value;
            invL.NetInvoiceTotal_SpareParts = invL.Total_ListPriceWithoutTax_SpareParts + invL.TaxSalesListSpareParts_Value - invL.ATS05ListSpareParts_Value;
            invL.NetInvoiceTotal_Oils = invL.Total_ListPriceWithoutTax_Oils + invL.TaxSalesListOils_Value - invL.ATS05ListOils_Value;
            ////+
            invL.NetInvoiceTotal_SparePartsOils = invL.NetInvoiceTotal_SpareParts + invL.NetInvoiceTotal_Oils;
            invL.NetInvoiceTotal = invL.NetInvoiceTotal_Oils + invL.NetInvoiceTotal_SpareParts + invL.NetInvoiceTotal_Workmanship;
            invL.Net_Total = invL.NetInvoiceTotal;

            invL.Equivalent_Net_Total_Price = invL.Net_Total;
            invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

            invL.TafkeetNet_Total = TafkeetProj.NumberToText.GetText(invL.Net_Total ?? 0, invL.Currency_id ?? 1);

            ComputeTaxForItems(ref invL, ItemsP);
            SetValuesDecimals(ref invL);
        }
        void ComputeTaxForItems(ref Invoices_List invL, List<CarsWorkOrdersItemProcessedDetail> ItemsP)
        {
            try
            {

                foreach (var item in ItemsP)
                {
                    invL.TaxSalesListWorkmanship_Rate = invL.TaxSalesListWorkmanship_Rate ?? 0;
                    invL.ATS05ListWorkmanship_Rate = invL.ATS05ListWorkmanship_Rate ?? 0;
                    if (item.Item_Category_Id == (long)ItemCategoryEnum.Masnaheaiat)
                    {
                        item.Item_TaxSales10 = invL.TaxSalesListWorkmanship_Rate;
                        item.Item_ATS05 = invL.ATS05ListWorkmanship_Rate;
                    }
                    if (item.Item_Category_Id == (long)ItemCategoryEnum.Oils)
                    {
                        item.Item_TaxSales10 = invL.TaxSalesListOils_Rate;
                        item.Item_ATS05 = invL.ATS05ListOils_Rate;
                    }
                    if (item.Item_Category_Id == (long)ItemCategoryEnum.SpareParts)
                    {
                        item.Item_TaxSales10 = invL.TaxSalesListSpareParts_Rate;
                        item.Item_ATS05 = invL.ATS05ListSpareParts_Rate;
                    }
                    item.Item_TaxSales10_Value = item.Item_Total_WithoutTax * item.Item_TaxSales10 / 100;
                    item.Item_ATS05_Value = item.Item_Total_WithoutTax * item.Item_ATS05 / 100;
                    item.Item_Net_Total_Price = item.Item_Total_WithoutTax + item.Item_TaxSales10_Value - item.Item_ATS05_Value;
                }
            }
            catch (Exception ee)
            {

            }
        }


        public void POS_GetTotalItemPrices(ref InvoicesOperationListCLS invL_O, ref InvoicesOperationCLS invD_O)
        {
            invL_O.invL.Total = 0;
            invL_O.invL.Total_Item_CommissionValue = 0;
            invL_O.invL.TaxSales10_Value = 0;
            invL_O.invL.ATS05_Value = 0;
            invL_O.invL.Total_ItemPriceWithoutTax = 0;
            invL_O.invL.Net_Total = 0;
            invL_O.invL.NetInvoiceTotal = 0;
            invL_O.invL.Total_List3Mola = 0;


            for (int x = 0; x < invD_O.invDlst.Count; x++)
            {
                if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "1")
                {
                    continue;
                }
                else
                {
                    //ArrExcludeIndex.Add(-1);
                }


                try
                {
                    invL_O.invL.Total_List3Mola = invL_O.invL.Total_List3Mola + decimal.Parse(invD_O.invDlst[x].Total3Mola.ToString());

                }
                catch { }

                try
                {
                    invL_O.invL.Total = invL_O.invL.Total + decimal.Parse(invD_O.invDlst[x].Qnty_value.ToString());

                }
                catch { }
                invL_O.invL.Total_Item_CommissionValue = invL_O.invL.Total_Item_CommissionValue + decimal.Parse(invD_O.invDlst[x].Net_Total_Item_Commission.ToString());
                //try
                //{
                //    invL_O.invL.TaxSales10_Value = invL_O.invL.TaxSales10_Value + decimal.Parse(invD_O.invDlst[x].Item_TaxSales10_Value.ToString());
                //}
                //catch { }
                //try
                //{
                //    invL_O.invL.ATS05_Value = invL_O.invL.ATS05_Value + decimal.Parse(invD_O.invDlst[x].Item_ATS05_Value.ToString());
                //}
                //catch { }
                try
                {
                    invL_O.invL.Total_ItemPriceWithoutTax = invL_O.invL.Total_ItemPriceWithoutTax + decimal.Parse(invD_O.invDlst[x].Item_Total_WithoutTax.ToString());
                    invL_O.invL.Total_ListPriceWithoutTax = invL_O.invL.Total_ItemPriceWithoutTax;
                    invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total_ItemPriceWithoutTax;

                }
                catch { }

                invL_O.invL.Net_Total = invL_O.invL.Net_Total + decimal.Parse(invD_O.invDlst[x].Item_Net_Total_Price.ToString());

            }
            invL_O.invL.Qnty_Value = invL_O.invL.Total;

            invL_O.invL.NetInvoiceTotal = invL_O.invL.Net_Total;


        }

        public void POS_InvoiceComputeTotalServicesValues(bool IsInvoiceTax, bool Is_Tax_Per_Item, ref InvoicesOperationListCLS invL_O)
        {

            try
            {
                invL_O.invL.Qnty_Value = invL_O.invL.Qnty_Value ?? 0;
                invL_O.invL.TaxSales10 = invL_O.invL.TaxSales10 ?? 0;
                invL_O.invL.Service_Rate = invL_O.invL.Service_Rate ?? 0;
                invL_O.invL.Delivery_Fee = invL_O.invL.Delivery_Fee ?? 0;
                invL_O.invL.Net_Total_CommissionValue = invL_O.invL.Net_Total_CommissionValue ?? 0;

                invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Qnty_Value;

                invL_O.invL.Service_Amount = invL_O.invL.Qnty_Value * invL_O.invL.Service_Rate / 100;

                if (invL_O.invL.IsDeliveryFeeAddedToInvoiceTotal == 0) ////لا يتم اضافة مصاريف التوصيل الى اجمالي الفاتورة
                {
                     
                    invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Qnty_Value   - invL_O.invL.Net_Total_CommissionValue;
                    ComputeCommissionRate(invL_O.invL);

                    invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.Service_Amount  ;

                    if (invL_O.invL.Total_PriceWithoutTax < 0)
                    {
                        invL_O.invL.Total_PriceWithoutTax = 0;
                    }
                    ComputeTaxPOS(  IsInvoiceTax,  Is_Tax_Per_Item, ref invL_O,0);
                  //  invL_O.invL.TaxSales10_Value = invL_O.invL.Total_PriceWithoutTax * invL_O.invL.TaxSales10 / 100;
                  //  invL_O.invL.TotalListTaxSales10_Value = invL_O.invL.TaxSales10_Value;

                    //invL_O.invL.Net_Total_CommissionValue = 0;

                   // invL_O.invL.NetInvoiceTotal = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.TaxSales10_Value;
                  
                }
                else
                {
                    if (invL_O.invL.IsDeliveryFeeAddedToInvoiceTotalAfterTax == 0) ///يتم اضافة مصاريف التوصيل الى اجمالي الفاتورة بعد حساب الضريبة
                    {
                        invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Qnty_Value - invL_O.invL.Net_Total_CommissionValue;
                        ComputeCommissionRate(invL_O.invL);

                        invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.Service_Amount;


                        if (invL_O.invL.Total_PriceWithoutTax < 0)
                        {
                            invL_O.invL.Total_PriceWithoutTax = 0;
                        }
                        ComputeTaxPOS(  IsInvoiceTax,   Is_Tax_Per_Item, ref invL_O,0);

                        // invL_O.invL.TaxSales10_Value = invL_O.invL.Total_PriceWithoutTax * invL_O.invL.TaxSales10 / 100;
                        // invL_O.invL.TotalListTaxSales10_Value = invL_O.invL.TaxSales10_Value;

                      

                       // invL_O.invL.NetInvoiceTotal = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.TaxSales10_Value + invL_O.invL.Delivery_Fee;
                        invL_O.invL.NetInvoiceTotal = invL_O.invL.NetInvoiceTotal +   invL_O.invL.Delivery_Fee;

                    }
                    else  ///يتم اضافة مصاريف التوصيل الى اجمالي الفاتورة قبل حساب الضريبة  
                    {

                        invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Qnty_Value - invL_O.invL.Net_Total_CommissionValue;
                        ComputeCommissionRate(invL_O.invL);

                       //invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.Service_Amount + invL_O.invL.Delivery_Fee;
                        invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.Service_Amount  ;
                       


                        if (invL_O.invL.Total_PriceWithoutTax < 0)
                        {
                            invL_O.invL.Total_PriceWithoutTax = 0;
                        }
                        ComputeTaxPOS(IsInvoiceTax, Is_Tax_Per_Item, ref invL_O, invL_O.invL.Delivery_Fee ?? 0);
                       // invL_O.invL.TaxSales10_Value = invL_O.invL.Total_PriceWithoutTax * invL_O.invL.TaxSales10 / 100;

                       // invL_O.invL.TotalListTaxSales10_Value = invL_O.invL.TaxSales10_Value;

                        //invL_O.invL.Net_Total_CommissionValue = 0;

                       // invL_O.invL.NetInvoiceTotal = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.TaxSales10_Value;

                    }
                }

                invL_O.invL.Net_Total = invL_O.invL.NetInvoiceTotal;
                invL_O.invL.NetInvoiceTotal = Common.RoundMe(invL_O.invL.NetInvoiceTotal,2);
                invL_O.invL.Net_Total = Common.RoundMe(invL_O.invL.Net_Total, 2);
                invL_O.invL.Service_Amount = Common.RoundMePositive(invL_O.invL.Service_Amount,2);
                invL_O.invL.TaxSales10_Value = Common.RoundMePositive(invL_O.invL.TaxSales10_Value, 2);
                invL_O.invL.Net_Total_CommissionValue = Common.RoundMePositive(invL_O.invL.Net_Total_CommissionValue, 2);
                 

            }
            catch (Exception ee)
            {
            }
        }

        void ComputeTaxPOS(bool IsInvoiceTax, bool Is_Tax_Per_Item, ref InvoicesOperationListCLS invL_O,decimal Delivery_Fee=0)
        {
            if (IsInvoiceTax == true)
            {

                if (Common.IsItemSellPriceContainTaxValue == 1)  // سعر الصنف شامل الضريبة
                {
                    decimal totalPrice = invL_O.invL.Qnty_Value ?? 0; // قيمة الفاتورة شاملة الضريبة (m تعني decimal)

                    totalPrice = totalPrice + Delivery_Fee;
                    decimal x = invL_O.invL.TaxSales10 ?? 0;           // على سبيل المثال: نسبة الضريبة 14%

                    // 1. حساب قيمة الفاتورة قبل الضريبة
                    decimal priceBeforeTax = totalPrice / (1 + (x / 100));

                    // 2. حساب قيمة الضريبة
                    decimal taxAmount = totalPrice - priceBeforeTax;
                    
                    invL_O.invL.NetInvoiceTotal = invL_O.invL.Qnty_Value;
                    invL_O.invL.Net_Total = invL_O.invL.Qnty_Value;
                    invL_O.invL.Qnty_Value = priceBeforeTax;
                    invL_O.invL.Total_PriceWithoutTax = priceBeforeTax;
                    invL_O.invL.TaxSales10_Value = taxAmount;
                   
                }
                else
                {
                    invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total_PriceWithoutTax   + invL_O.invL.Delivery_Fee;
                    invL_O.invL.TaxSales10_Value = invL_O.invL.Total_PriceWithoutTax * invL_O.invL.TaxSales10 / 100;

                    invL_O.invL.NetInvoiceTotal = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.TaxSales10_Value;
                }
            }
 

            invL_O.invL.TotalListTaxSales10_Value = invL_O.invL.TaxSales10_Value;



            invL_O.invL.TotalListTaxSales10_Value = Common.RoundMe(invL_O.invL.TotalListTaxSales10_Value);
            invL_O.invL.NetInvoiceTotal = Common.RoundMe(invL_O.invL.NetInvoiceTotal);
            invL_O.invL.Net_Total = Common.RoundMe(invL_O.invL.Net_Total);
            invL_O.invL.Qnty_Value = Common.RoundMe(invL_O.invL.Qnty_Value);
            invL_O.invL.Total_PriceWithoutTax = Common.RoundMe(invL_O.invL.Total_PriceWithoutTax);
            invL_O.invL.TaxSales10_Value = Common.RoundMe(invL_O.invL.TaxSales10_Value);
             
        }
        void ComputeCommissionRate(IInvoices_List invL)
        {
            try
            {
                // تأمين الكود: التحقق من أن إجمالي الفاتورة أكبر من صفر قبل القسمة
                if (invL.Qnty_Value > 0)
                {
                    // حساب نسبة الخصم كنسبة مئوية (مثلاً: 14.29)
                    invL.List_CommissionRate = (invL.Net_Total_CommissionValue / invL.Qnty_Value) * 100;

                    // أو إذا كنت تريد تقريب النسبة لأقرب رقمين عشريين لزوم الطباعة والعرض
                    invL.List_CommissionRate = Common.RoundMe(invL.List_CommissionRate, 2);
                }
                else
                {
                    invL.List_CommissionRate = 0;
                }
            }
            catch { invL.List_CommissionRate = 0; }
        }
        void ComputeCommissionRate(Invoices_List invL)
        {
            try
            {
                // تأمين الكود: التحقق من أن إجمالي الفاتورة أكبر من صفر قبل القسمة
                if (invL.Qnty_Value > 0)
                {
                    // حساب نسبة الخصم كنسبة مئوية (مثلاً: 14.29)
                    invL.List_CommissionRate = (invL.Net_Total_CommissionValue / invL.Qnty_Value) * 100;

                    // أو إذا كنت تريد تقريب النسبة لأقرب رقمين عشريين لزوم الطباعة والعرض
                    invL.List_CommissionRate = Common.RoundMe(invL.List_CommissionRate, 2);
                }
                else
                {
                    invL.List_CommissionRate = 0;
                }
            }
            catch { invL.List_CommissionRate = 0; }
        }


        public void ComputeInvoiceTotals(bool IsInvoiceTax, bool Is_Tax_Per_Item,ref InvoicesOperationListCLS invL_O,ref InvoicesOperationCLS invD_O)
        {
            
           // SetTaxDataOnForm();

            GetTotalItemPrices(ref   invL_O,  ref     invD_O);

            //SetTotalItemPricesOnScreen();
           // SetCommissionOnScreen();


            //ReadInvoiceListData(invL_O.invL);


             


            ComputeCommission(ref   invL_O);


            ComputeTotals(  IsInvoiceTax, Is_Tax_Per_Item ,ref invL_O ,ref invD_O );


            //SetTotalItemPricesOnScreen();



           // this.CustomerAccountAfter = (this.Invoice_Reverse != 0) ? (this.CustomerAccountBefor - invL_O.invL.Net_Total ?? 0) : (this.CustomerAccountBefor + invL_O.invL.Net_Total ?? 0);
            //this.GetCustomerAccountAfter();

           // SetTotalOnMonitor();
            //IsLoad_OldData_Flage = false;
        }
         
        public void ComputeTotals(bool IsInvoiceTax,bool Is_Tax_Per_Item,ref InvoicesOperationListCLS invL_O, ref InvoicesOperationCLS invD_O)
        {
            invL_O.invL.Item_Net_TotalSpareParts = invL_O.invL.Item_Net_TotalSpareParts ?? 0;
            invL_O.invL.Item_Net_TotalOils = invL_O.invL.Item_Net_TotalOils ?? 0;
            invL_O.invL.Item_Net_TotalSparePartsOils = invL_O.invL.Item_Net_TotalSparePartsOils ?? 0;
            //checked nulls
            invL_O.invL.Item_Net_TotalWorkmanship = invL_O.invL.Item_Net_TotalWorkmanship ?? 0;
            invL_O.invL.CommissionListWorkmanship_Rate = invL_O.invL.CommissionListWorkmanship_Rate ?? 0;

            invL_O.invL.CommissionListSpareParts_Rate = invL_O.invL.CommissionListSpareParts_Rate ?? 0;
            invL_O.invL.CommissionListOils_Rate = invL_O.invL.CommissionListOils_Rate ?? 0;
            ////+
            invL_O.invL.CommissionListSparePartsOils_Rate = invL_O.invL.CommissionListSparePartsOils_Rate ?? 0;


            invL_O.invL.CommissionListWorkmanship_Value = invL_O.invL.CommissionListWorkmanship_Value ?? 0;

            invL_O.invL.CommissionListSpareParts_Value = invL_O.invL.CommissionListSpareParts_Value ?? 0;
            invL_O.invL.CommissionListOils_Value = invL_O.invL.CommissionListOils_Value ?? 0;
            ////+
            invL_O.invL.CommissionListSparePartsOils_Value = invL_O.invL.CommissionListSparePartsOils_Value ?? 0;
            invL_O.invL.Item_Net_Total = invL_O.invL.Item_Net_Total ?? 0;// invL.Item_Net_TotalOils + invL.Item_Net_TotalSpareParts + invL.Item_Net_TotalWorkmanship;
                                                           // invL.Net_Total = invL.Item_Net_Total;// invL.Item_Net_TotalOils + invL.Item_Net_TotalSpareParts + invL.Item_Net_TotalWorkmanship;



            invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total - invL_O.invL.Net_Total_CommissionValue;

            invL_O.invL.Total_ListPriceWithoutTax = invL_O.invL.Total_PriceWithoutTax ?? 0;



            if (Is_Tax_Per_Item == true )
            {
                GetTotalTaxsFromItems(ref   invL_O, ref   invD_O);
                //invL_O.invL.ATS05_Value = invL_O.Total_Item_ATS05_Value;
                //invL_O.invL.TaxSales10_Value = invL_O.Total_Item_TaxSales10_Value;



                //invL_O.invL.Net_Total = invL_O.Total_Item_Net_Total_Price;
            }
            else
            {


                invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total - invL_O.invL.Net_Total_CommissionValue;


                //invL.invL.ATS05_Value = invL.invL.Total_Qnty_Value * invL.invL.ATS05;
                //invL.invL.TaxSales10_Value = invL.invL.Total_Qnty_Value * invL.invL.TaxSales10;
                invL_O.invL.ATS05_Value = invL_O.invL.Total_PriceWithoutTax * invL_O.invL.ATS05 / 100;
                invL_O.invL.ATS05ListSpareParts_Value = invL_O.invL.Total_ListPriceWithoutTax_SpareParts * invL_O.invL.ATS05ListSpareParts_Rate / 100;
                invL_O.invL.ATS05ListWorkmanship_Value = invL_O.invL.Total_ListPriceWithoutTax_Workmanship * invL_O.invL.ATS05ListWorkmanship_Rate / 100;
                invL_O.invL.ATS05ListOils_Value = invL_O.invL.Total_ListPriceWithoutTax_Oils * invL_O.invL.ATS05ListOils_Rate / 100;

                invL_O.invL.TaxSales10_Value = invL_O.invL.Total_PriceWithoutTax * invL_O.invL.TaxSales10 / 100;
                invL_O.invL.TaxSalesListSpareParts_Value = invL_O.invL.Total_ListPriceWithoutTax_SpareParts * invL_O.invL.TaxSalesListSpareParts_Rate / 100;
                invL_O.invL.TaxSalesListWorkmanship_Value = invL_O.invL.Total_ListPriceWithoutTax_Workmanship * invL_O.invL.TaxSalesListWorkmanship_Rate / 100;
                invL_O.invL.TaxSalesListOils_Value = invL_O.invL.Total_ListPriceWithoutTax_Oils * invL_O.invL.TaxSalesListOils_Rate / 100;



                invL_O.invL.Net_Total = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.TaxSales10_Value - invL_O.invL.ATS05_Value;

                invL_O.invL.Net_Total = invL_O.invL.Net_Total + invL_O.invL.Total_List3Mola;
                ComputeTaxForItems(  IsInvoiceTax, Is_Tax_Per_Item, ref invL_O.invL,ref invD_O.invDlst);
            }


            //if (invL_O.invL.IsInvoiceTaxInfo == 0)
            //{
            //    invL_O.invL.ATS05 = 0;
            //    invL_O.invL.TaxSales10 = 0;
            //    invL_O.invL.ATS05_Value = 0;
            //    invL_O.invL.TaxSales10_Value = 0;
            //}
            //else
            //{

            //    invL_O.invL.ATS05 = this.nuATS05List_Rate.Value / Common.Is_Div_100Or_1_Rate;
            //    invL_O.invL.TaxSales10 = this.nuTaxSalesList_Rate.Value / Common.Is_Div_100Or_1_Rate;

            //    //invL.invL.ATS05_Value = invL.invL.Total_Qnty_Value * invL.invL.ATS05;
            //    //invL.invL.TaxSales10_Value = invL.invL.Total_Qnty_Value * invL.invL.TaxSales10;
            //    invL_O.invL.ATS05_Value = invL_O.invL.Total_PriceWithoutTax * invL_O.invL.ATS05;
            //    invL_O.invL.TaxSales10_Value = invL_O.invL.Total_PriceWithoutTax * invL_O.invL.TaxSales10;

            //    invL_O.invL.Net_Total = invL_O.invL.Total_PriceWithoutTax + invL_O.invL.TaxSales10_Value - invL_O.invL.ATS05_Value;

            //}

            invL_O.invL.Net_Total = invL_O.invL.Net_Total + invL_O.invL.AnotherPayment;
            invL_O.invL.Net_Total = invL_O.invL.Net_Total + invL_O.invL.Shipment_Value;

            invL_O.invL.Net_Total = Common.RoundMePositive(invL_O.invL.Net_Total);
            invL_O.invL.AnotherPayment = Common.RoundMePositive(invL_O.invL.AnotherPayment);
            ////////////////

            /////Sales totales
            ///From Item Detail

            //invL.Total = invL.Qnty_ValueOils + invL.Qnty_ValueSpareParts + invL.Qnty_ValueWorkmanship;
            //invL.Qnty_Value = invL.Total;


            ////Net totals
            ///From Item Detail
            //invL.Item_Net_TotalWorkmanship
            //invL.Item_Net_TotalSpareParts
            //invL.Item_Net_TotalOils


            ///From invoice  Form  
            //    invL.CommissionListWorkmanship_Rate
            //invL.CommissionListSpareParts_Rate
            //invL.CommissionListOils_Rate
            //decimal? CommissionListWorkmanship_ValueOfRate = invL.Item_Net_TotalWorkmanship * invL.CommissionListWorkmanship_Rate / 100;
            //decimal? CommissionListSpareParts_ValueOfRate = invL.Item_Net_TotalSpareParts * invL.CommissionListSpareParts_Rate / 100;
            //decimal? CommissionListOils_ValueOfRate = invL.Item_Net_TotalOils * invL.CommissionListOils_Rate / 100;
            //decimal? CommissionListSparePartsOils_ValueOfRate = CommissionListOils_ValueOfRate + CommissionListSpareParts_ValueOfRate;
            //decimal? Total_CommissionList_ValueOfRate = CommissionListWorkmanship_ValueOfRate + CommissionListSpareParts_ValueOfRate + CommissionListOils_ValueOfRate;
            //invL.List_CommissionRate = invL.CommissionListWorkmanship_Rate + invL.CommissionListSpareParts_Rate + invL.CommissionListOils_Rate;
            ///From invoice  Form  
            //    invL.CommissionListWorkmanship_Value
            //invL.CommissionListSpareParts_Value
            //invL.CommissionListOils_Value
            //decimal? TotalCommissionList_Value = invL.CommissionListWorkmanship_Value + invL.CommissionListSpareParts_Value + invL.CommissionListOils_Value;

            //invL.Total_CommissionListWorkmanship = invL.CommissionListWorkmanship_Value + CommissionListWorkmanship_ValueOfRate;
            //invL.Total_CommissionListSpareParts = invL.CommissionListSpareParts_Value + CommissionListSpareParts_ValueOfRate;
            //invL.Total_CommissionListOils = invL.CommissionListOils_Value + CommissionListOils_ValueOfRate;
            //////+
            //invL.Total_CommissionListSparePartsOils = invL.CommissionListSparePartsOils_Value + CommissionListSparePartsOils_ValueOfRate;
            //invL.Total_CommissionList = invL.Total_CommissionListOils + invL.Total_CommissionListSpareParts + invL.Total_CommissionListWorkmanship;
            ////invL.List_Final_CommissionValue  
            ////invL.Net_Total_CommissionValue  
            //invL.List_CommissionValue = TotalCommissionList_Value ?? 0;
            //invL.List_Final_CommissionValue = invL.Total_CommissionList ?? 0;

            ////////////
            //invL.Net_Total_CommissionValue = invL.Total_CommissionList + Total_CommissionList_ValueOfRate;

            //invL.Total_ListPriceWithoutTax_Workmanship = invL.Qnty_ValueWorkmanship - invL.Total_CommissionListWorkmanship;
            //invL.Total_ListPriceWithoutTax_SpareParts = invL.Qnty_ValueSpareParts - invL.Total_CommissionListSpareParts;
            //invL.Total_ListPriceWithoutTax_Oils = invL.Qnty_ValueOils - invL.Total_CommissionListOils;
            /////////+
            //invL.Total_ListPriceWithoutTax_SparePartsOils = invL.Total_ListPriceWithoutTax_SpareParts + invL.Total_ListPriceWithoutTax_Oils;

            //invL.Total_ListPriceWithoutTax = invL.Total_ListPriceWithoutTax_Oils + invL.Total_ListPriceWithoutTax_SpareParts + invL.Total_ListPriceWithoutTax_Workmanship;






            ///From invoice  Form  
            //    invL.ATS05ListWorkmanship_Rate
            //invL.txtATS05ListSpareParts_Rate
            //invL.ATS05ListOils_Rate

            //invL.ATS05ListWorkmanship_Value = invL.Total_ListPriceWithoutTax_Workmanship * invL.ATS05ListWorkmanship_Rate / 100;
            //invL.ATS05ListSpareParts_Value = invL.Total_ListPriceWithoutTax_SpareParts * invL.ATS05ListSpareParts_Rate / 100;
            //invL.ATS05ListOils_Value = invL.Total_ListPriceWithoutTax_Oils * invL.ATS05ListOils_Rate / 100;

            ////+
            //invL.ATS05ListSparePartsOils_Value = invL.ATS05ListSpareParts_Value + +invL.ATS05ListOils_Value;

            //invL.ATS05ListSparePartsOils_Value = invL.ATS05ListOils_Value + invL.ATS05ListSpareParts_Value;


            ////+
            //invL.ATS05ListSparePartsOils_Value = invL.ATS05ListOils_Value + invL.ATS05ListSpareParts_Value;

            //invL.TotalListATS05_Value = invL.ATS05ListOils_Value + invL.ATS05ListSpareParts_Value + invL.ATS05ListWorkmanship_Value;

            //invL.ATS05_Value = invL.TotalListATS05_Value;
            //invL.TaxSales10 = 0;
            //invL.ATS05 = 0;
            /////From invoice  Form  
            ////    invL.TaxSalesListWorkmanship_Rate
            ////invL.TaxSalesListSpareParts_Rate
            ////invL.TaxSalesListOils_Rate

            //invL.TaxSalesListWorkmanship_Value = invL.Total_ListPriceWithoutTax_Workmanship * invL.TaxSalesListWorkmanship_Rate / 100;
            //invL.TaxSalesListSpareParts_Value = invL.Total_ListPriceWithoutTax_SpareParts * invL.TaxSalesListSpareParts_Rate / 100;
            //invL.TaxSalesListOils_Value = invL.Total_ListPriceWithoutTax_Oils * invL.TaxSalesListOils_Rate / 100;
            //////+
            //invL.TaxSalesListSparePartsOils_Value = invL.TaxSalesListSpareParts_Value + invL.TaxSalesListOils_Value;
            //invL.TotalListTaxSales10_Value = invL.TaxSalesListOils_Value + invL.TaxSalesListSpareParts_Value + invL.TaxSalesListWorkmanship_Value;
            //invL.TaxSales10_Value = invL.TotalListTaxSales10_Value;


            //////Total After  Only  Taxe Sales
            //////////
            //invL.Total_ListPriceAfterTaxSales_Workmanship = invL.Total_ListPriceWithoutTax_Workmanship + invL.TaxSalesListWorkmanship_Value;
            //invL.Total_ListPriceAfterTaxSales_SpareParts = invL.Total_ListPriceWithoutTax_SpareParts + invL.TaxSalesListSpareParts_Value;
            //invL.Total_ListPriceAfterTaxSales_Oils = invL.Total_ListPriceWithoutTax_Oils + invL.TaxSalesListOils_Value;
            //////+
            //invL.Total_ListPriceAfterTaxSales_SparePartsOils = invL.Total_ListPriceAfterTaxSales_SpareParts + invL.Total_ListPriceAfterTaxSales_Oils;

            //invL.Total_ListPriceAfterTaxSales = invL.Total_ListPriceAfterTaxSales_Workmanship + invL.Total_ListPriceAfterTaxSales_SpareParts + invL.Total_ListPriceAfterTaxSales_Oils;



            //////Total After All Taxes
            //////////
            //invL.NetInvoiceTotal_Workmanship = invL.Total_ListPriceWithoutTax_Workmanship + invL.TaxSalesListWorkmanship_Value - invL.ATS05ListWorkmanship_Value;
            //invL.NetInvoiceTotal_SpareParts = invL.Total_ListPriceWithoutTax_SpareParts + invL.TaxSalesListSpareParts_Value - invL.ATS05ListSpareParts_Value;
            //invL.NetInvoiceTotal_Oils = invL.Total_ListPriceWithoutTax_Oils + invL.TaxSalesListOils_Value - invL.ATS05ListOils_Value;
            //////+
            //invL.NetInvoiceTotal_SparePartsOils = invL.NetInvoiceTotal_SpareParts + invL.NetInvoiceTotal_Oils;
            //invL.NetInvoiceTotal = invL.NetInvoiceTotal_Oils + invL.NetInvoiceTotal_SpareParts + invL.NetInvoiceTotal_Workmanship;
            //invL.Net_Total = 






            if (invL_O.invL.Net_Total < 0)
            {
                invL_O.invL.Net_Total = 0;
            }



            invL_O.invL.NetInvoiceTotal = invL_O.invL.Net_Total;
            invL_O.invL.Equivalent_Net_Total_Price = invL_O.invL.Net_Total;
            invL_O.invL.Equivalent_Net_Total_CommissionValue = invL_O.invL.Net_Total_CommissionValue;

            invL_O.invL.TafkeetNet_Total = TafkeetProj.NumberToText.GetText(invL_O.invL.Net_Total ?? 0, invL_O.invL.Currency_id ?? 1);

            Common.SetValuesDecimals(ref invL_O.invL);
            
        }
        void SetTaxForEachItem(bool IsInvoiceTax ,bool Is_TaxPer_Item,IInvoices_Details item)
        {
            if (IsInvoiceTax == true)
            {
                if (Is_TaxPer_Item == false)
                {
                    operations opp = new operations();
                    //            ضريبة القيمة المضافة
                    //كود رئيسى T1
                    TaxListCLS tax = new TaxListCLS();
                    List<AccountTaxInfo> accTaxList = tax.Get_TaxInfoRatioList(ref opp);
                    AccountTaxInfo accInfo = accTaxList.Where(x => x.MainCode == "T1").FirstOrDefault();

                    accInfo = tax.Get_TaxInfoRatio(ref opp, "T1", "");
                   



                    ItemCLS itmCLS = new ItemCLS();
                    Items itmSearch = itmCLS.GetItemOnlyById(item.Item_Id ?? 0);

                    if (accInfo != null)
                    {
                        accInfo.TaxRate = accInfo.TaxRate ?? 0;
                        accInfo.MinAllowedValueToComputeTax = accInfo.MinAllowedValueToComputeTax ?? 0;
                        item.TaxSales10 = accInfo.TaxRate.Value * 100;
                        item.Item_TaxSales10 = accInfo.TaxRate.Value * 100;

                    }
                    else
                    {
                        item.TaxSales10 = 0;
                        item.Item_TaxSales10 = 0;
                    }

                    


                    if (itmSearch.Item_Category_ID == (long)ItemCategoryEnum.Masnaheaiat)
                    {
                        accInfo = accTaxList.Where(x => x.MainCode == "T4" & x.SubCode == "W004").FirstOrDefault();
                        if (accInfo != null)
                        {
                            accInfo.TaxRate = accInfo.TaxRate ?? 0;
                            accInfo.MinAllowedValueToComputeTax = accInfo.MinAllowedValueToComputeTax ?? 0;
                            item.ATS05 = accInfo.TaxRate.Value * 100;
                            item.Item_ATS05 = accInfo.TaxRate.Value * 100;
                        }
                        else
                        {
                            item.ATS05 = 0;
                            item.Item_ATS05 = 0;
                        }

                        
                    }
                    if (itmSearch.Item_Category_ID == (long)ItemCategoryEnum.SpareParts | itmSearch.Item_Category_ID == (long)ItemCategoryEnum.Oils)
                    {
                          accInfo = accTaxList.Where(x => x.MainCode == "T4" & x.SubCode == "W002").FirstOrDefault();
                        if (accInfo != null)
                        {
                            accInfo.TaxRate = accInfo.TaxRate ?? 0;
                            accInfo.MinAllowedValueToComputeTax = accInfo.MinAllowedValueToComputeTax ?? 0;
                            item.ATS05 = accInfo.TaxRate.Value * 100;
                            item.Item_ATS05 = accInfo.TaxRate.Value * 100;

                        }
                        else
                        {
                            item.ATS05 = 0;
                            item.Item_ATS05 = 0;
                        }
                       
                    }
                    


                }
            }

        }
        
        void ComputeTaxForItems(bool IsInvoiceTax, bool Is_TaxPer_Item, ref IInvoices_List invL,ref List<IInvoices_Details> ItemsP)
        {
            try
            {

                foreach (var item in ItemsP)
                {
                    long Item_Category_Id = 0;
                    SetTaxForEachItem(  IsInvoiceTax,   Is_TaxPer_Item, item);
                    invL.TaxSalesListWorkmanship_Rate = invL.TaxSalesListWorkmanship_Rate ?? 0;
                    invL.ATS05ListWorkmanship_Rate = invL.ATS05ListWorkmanship_Rate ?? 0;


                    //item.Item_ATS05 = invL.ATS05;
                    //item.Item_TaxSales10 = invL.TaxSales10;
                    //item.Item_TaxSales10 = invL.TaxSalesListSpareParts_Rate;
                    //item.Item_ATS05 = invL.ATS05ListSpareParts_Rate;
                    //if (Item_Category_Id == (long)ItemCategoryEnum.Masnaheaiat)
                    //{
                    //    item.Item_TaxSales10 = invL.TaxSalesListWorkmanship_Rate;

                    //}
                    //if (Item_Category_Id == (long)ItemCategoryEnum.Oils)
                    //{
                    //    item.Item_TaxSales10 = invL.TaxSalesListOils_Rate;
                    //    item.Item_ATS05 = invL.ATS05ListOils_Rate;
                    //}
                    //if (Item_Category_Id == (long)ItemCategoryEnum.SpareParts)
                    //{
                    //    item.Item_TaxSales10 = invL.TaxSalesListSpareParts_Rate;
                    //    item.Item_ATS05 = invL.ATS05ListSpareParts_Rate;
                    //}
                    item.Item_TaxSales10_Value = item.Item_Total_WithoutTax * item.Item_TaxSales10 / 100;
                    item.Item_ATS05_Value = item.Item_Total_WithoutTax * item.Item_ATS05 / 100;
                    item.Item_Net_Total_Price = item.Item_Total_WithoutTax + item.Item_TaxSales10_Value - item.Item_ATS05_Value;

                    item.Item_Net_Total_Price = item.Item_Net_Total_Price + item.Total3Mola;
                }
            }
            catch (Exception ee)
            {

            }
        }
        private void GetTotalTaxsFromItems(ref InvoicesOperationListCLS invL_O,ref InvoicesOperationCLS invD_O)
        {

            for (int x = 0; x < invD_O.invDlst.Count; x++)
            {
                if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "1")
                {
                    continue;
                }
                else
                {
                    //ArrExcludeIndex.Add(-1);
                }
                try
                {
                    invL_O.invL.TaxSales10_Value = invL_O.invL.TaxSales10_Value + decimal.Parse(invD_O.invDlst[x].Item_TaxSales10_Value.ToString());
                }
                catch { }
                try
                {
                    invL_O.invL.ATS05_Value = invL_O.invL.ATS05_Value + decimal.Parse(invD_O.invDlst[x].Item_ATS05_Value.ToString());
                }
                catch { }

            }

        }
        void ComputeCommission(ref InvoicesOperationListCLS invL_O)
        {


            decimal Commission = 0;
            //vs.Is_Item_CommissionValue = cbIs_Item_CommissionValue.Checked ? 0 : 1;



            invL_O.invL.List_Final_CommissionValue = 0;
            invL_O.invL.Net_Total_CommissionValue = 0;

            invL_O.invL.List_CommissionRate = invL_O.invL.List_CommissionRate ?? 0;
            invL_O.invL.Qnty_Value = invL_O.invL.Qnty_Value ?? 0;
            // الخصم نسبة
            Commission = invL_O.invL.List_CommissionRate.Value * invL_O.invL.Qnty_Value.Value / 100;
            invL_O.invL.List_Final_CommissionValue = invL_O.invL.List_Final_CommissionValue + Commission;



            // الخصم قيمه
            Commission = invL_O.invL.List_CommissionValue ?? 0;
            invL_O.invL.List_Final_CommissionValue = invL_O.invL.List_Final_CommissionValue + Commission;





            //if (Is_Tax_Per_Item == 1)
            //{
            //    invL_O.invL.Net_Total_CommissionValue = invL_O.invL.Total_Item_CommissionValue;

            //}
            //else
            //{
            //    invL_O.invL.Net_Total_CommissionValue = invL_O.invL.List_Final_CommissionValue + invL_O.invL.Total_Item_CommissionValue;
            //}
            invL_O.invL.Net_Total_CommissionValue = invL_O.invL.List_Final_CommissionValue + invL_O.invL.Total_Item_CommissionValue;


        }
        private void GetTotalItemPrices(ref InvoicesOperationListCLS invL_O,  ref InvoicesOperationCLS invD_O)
        {
            invL_O.invL.Total = 0;
            invL_O.invL.Total_Item_CommissionValue = 0;
            invL_O.invL.TaxSales10_Value = 0;
            invL_O.invL.ATS05_Value = 0;
            invL_O.invL.Total_ItemPriceWithoutTax = 0;
            invL_O.invL.Net_Total = 0;
            invL_O.invL.NetInvoiceTotal = 0;
            invL_O.invL.Total_List3Mola = 0;


            for (int x = 0; x < invD_O.invDlst.Count; x++)
            {
                if (invD_O.invDlst[x].Item_DeletedFlage.ToString() == "1")
                {
                    continue;
                }
                else
                {
                    //ArrExcludeIndex.Add(-1);
                }


                try
                {
                    invL_O.invL.Total_List3Mola = invL_O.invL.Total_List3Mola + decimal.Parse(invD_O.invDlst[x].Total3Mola.ToString());

                }
                catch { }

                try
                {
                    invL_O.invL.Total = invL_O.invL.Total + decimal.Parse(invD_O.invDlst[x].Qnty_value.ToString());

                }
                catch { }
                invL_O.invL.Total_Item_CommissionValue = invL_O.invL.Total_Item_CommissionValue + decimal.Parse(invD_O.invDlst[x].Net_Total_Item_Commission.ToString());
                //try
                //{
                //    invL_O.invL.TaxSales10_Value = invL_O.invL.TaxSales10_Value + decimal.Parse(invD_O.invDlst[x].Item_TaxSales10_Value.ToString());
                //}
                //catch { }
                //try
                //{
                //    invL_O.invL.ATS05_Value = invL_O.invL.ATS05_Value + decimal.Parse(invD_O.invDlst[x].Item_ATS05_Value.ToString());
                //}
                //catch { }
                try
                {
                    invL_O.invL.Total_ItemPriceWithoutTax = invL_O.invL.Total_ItemPriceWithoutTax + decimal.Parse(invD_O.invDlst[x].Item_Total_WithoutTax.ToString());
                    invL_O.invL.Total_ListPriceWithoutTax = invL_O.invL.Total_ItemPriceWithoutTax;
                    invL_O.invL.Total_PriceWithoutTax = invL_O.invL.Total_ItemPriceWithoutTax;

                }
                catch { }

                invL_O.invL.Net_Total = invL_O.invL.Net_Total + decimal.Parse(invD_O.invDlst[x].Item_Net_Total_Price.ToString());

            }
            invL_O.invL.Qnty_Value = invL_O.invL.Total;

            invL_O.invL.NetInvoiceTotal = invL_O.invL.Net_Total;


        }
    }
}
