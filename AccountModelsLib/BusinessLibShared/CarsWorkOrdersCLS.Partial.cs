using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
 
 
using System.Runtime.CompilerServices;
 
 
namespace AccountModelsLib.Models
{
    public partial class CarsWorkOrdersCLS
    {
        public void GetCarReverseInvoice(ref int Excp, ref string Msg, long Invoice_List_Table_Id, ref Invoices_List_Reverse invL, ref List<Invoices_Details_Reverse> invD_S, ref List<Invoices_Details_Reverse> invD_O, ref List<Invoices_Details_Reverse> invD_M)
        {
            Invoices_List_ReverseCls invCls = new Invoices_List_ReverseCls();
            invL = invCls.GetInvoices_List_ReverseById(ref opp, Invoice_List_Table_Id);

            long Item_Category_ID = (long)ItemCategoryEnum.SpareParts;
            invD_S = new List<Invoices_Details_Reverse>();
            invD_S = invCls.GetInvoices_Details_Reverse(ref opp, Invoice_List_Table_Id, Item_Category_ID);

            Item_Category_ID = (long)ItemCategoryEnum.Oils;
            invD_O = new List<Invoices_Details_Reverse>();
            invD_O = invCls.GetInvoices_Details_Reverse(ref opp, Invoice_List_Table_Id, Item_Category_ID);

            Item_Category_ID = (long)ItemCategoryEnum.Masnaheaiat;
            invD_M = new List<Invoices_Details_Reverse>();
            invD_M = invCls.GetInvoices_Details_Reverse(ref opp, Invoice_List_Table_Id, Item_Category_ID);

        }
        public List<CarsWorkOrdersViewModel> GetCarInvoiceReverse(ref int Excp, ref string Msg, ref long CarsWorkOrdersId, ref long Invoice_List_Table_Id, ref Invoices_List_Reverse invL, ref List<Invoices_Details_Reverse> invD, int Is_AddToInvoice)
        {
            Invoices_List_ReverseCls invCls = new Invoices_List_ReverseCls();
            //invL = new Invoices_List();
            //invD = new List<Invoices_Details>();

            if (Invoice_List_Table_Id > 0)
            {
                invL = invCls.GetInvoices_List_ReverseById(ref opp, Invoice_List_Table_Id);
                if (invL != null)
                {
                    CarsWorkOrdersId = invL.CarsWorkOrdersId ?? 0;
                    invD = invCls.GetInvoices_Details_Reverse(ref opp, Invoice_List_Table_Id);
                    Is_AddToInvoice = 1;
                }
                else
                {
                    invL = new Invoices_List_Reverse();
                    invD = new List<Invoices_Details_Reverse>();
                }
            }
            else
            {

            }




            List<CarsWorkOrdersViewModel> carVM = new List<CarsWorkOrdersViewModel>();
            try
            {

                carVM = getItemsReversedProcessedForInvoice(CarsWorkOrdersId, Is_AddToInvoice);


            }
            catch (Exception ee)
            {
            }
            return carVM;
        }
        public List<CarsWorkOrdersViewModel> getItemsReversedProcessedForInvoice(long CarsWorkOrdersId_, int Is_AddToInvoice)
        {

            long Item_Category_ID = 0;
            ///////get item for ItemProcessedDetail
            CarsWorkOrdersItemReversedProcessedDetailCLS ItemProcessedDetailCLS = new  CarsWorkOrdersItemReversedProcessedDetailCLS ();


            // اجمالى الخامات التى تم اضافتها على صنف يسمى اجمالى الخامات

            // SetComputeRawMaterialsTotalsOld(ref opp, ItemProcessedDetailCLS, CarsWorkOrdersId_, Is_AddToInvoice);
            //CarsWorkOrdersItemProcessedDetail Khamat = SetComputeRawMaterialsTotals(CarsWorkOrdersId_, Is_AddToInvoice);
            //if(Khamat.Qnty_value>0)
            //{

            //}


            ///////////////
            ItemReversedProcessedList = new List<CarsWorkOrdersItemReversedProcessedDetail> ();
            ItemReversedProcessedList = ItemProcessedDetailCLS.GetCarsWorkOrdersItemReversedProcessedDetailsList(ref opp, CarsWorkOrdersId_, Is_AddToInvoice);

            Item_Category_ID = (long)ItemCategoryEnum.SpareParts;
            ItemReversedProcessedList_SpareParts = new  List<CarsWorkOrdersItemReversedProcessedDetail> ();
            ItemReversedProcessedList_SpareParts = ItemProcessedDetailCLS.GetItemReversedProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);
            
            Item_Category_ID = (long)ItemCategoryEnum.Oils;
            ItemReversedProcessedList_Oils = new List<CarsWorkOrdersItemReversedProcessedDetail> ();
            ItemReversedProcessedList_Oils = ItemProcessedDetailCLS.GetItemReversedProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);

            Item_Category_ID = (long)ItemCategoryEnum.Masnaheaiat;
            ItemReversedProcessedList_Workmanships = new List<CarsWorkOrdersItemReversedProcessedDetail> ();
            ItemReversedProcessedList_Workmanships = ItemProcessedDetailCLS.GetItemReversedProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);




            ///get work order data by CarsWorkOrdersId
            ResetSearchCriteria();
            CarsWorkOrdersId = CarsWorkOrdersId_;
            List<CarsWorkOrdersViewModel> CarsWorkOrdersVMList = getCarsWorkOrder();
            if (CarsWorkOrdersVMList != null)
            {
                CarsWorkOrdersViewModel = CarsWorkOrdersVMList.FirstOrDefault();
            }
            return CarsWorkOrdersVMList;

        }

        public bool GetCheckIfFoundInvoiceReverse(ref int excp, ref string msg, ref long carsWorkOrdersId, ref long invoice_List_Table_Id)
        {
            invoice_List_Table_Id = 0;
            bool IsFound = false;
            Invoices_List_ReverseCls invCls = new Invoices_List_ReverseCls();
            List<Invoices_List_Reverse> invList = invCls.GetInvoices_List_ReverseByCarsWorkOrdersId(ref opp, carsWorkOrdersId);
            if (invList != null)
            {
                if (invList.Count > 0)
                {
                    invoice_List_Table_Id = invList[0].Id;
                    IsFound = true;
                }
            }
            else
            {
                IsFound = false;
            }
            return IsFound;
        }

        public void GetComputeTotalsAllReverse(Invoices_List_Reverse invL, IEnumerable<AICarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            GetItemProcessedListTotalsReverse(invL, ItemProcessedList.ToList());
            ComputeTotalsReverse(invL, ItemProcessedList.ToList());
        }
        public void GetComputeTotalsAllReverse_(Invoices_List_Reverse invL, IReadOnlyList<AICarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            GetItemProcessedListTotalsReverse(invL, ItemProcessedList.ToList());
            ComputeTotalsReverse(invL, ItemProcessedList.ToList());
        }

        public void GetComputeTotalsAllReverse(Invoices_List_Reverse invL, List<AICarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            GetItemProcessedListTotalsReverse(invL, ItemProcessedList);
            ComputeTotalsReverse(invL, ItemProcessedList);
        }
        public void GetItemProcessedListTotalsReverse(Invoices_List_Reverse invL, List<AICarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            invL.Qnty_ValueWorkmanship = 0;
            invL.Qnty_ValueSpareParts = 0;
            invL.Qnty_ValueOils = 0;

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
                invL.Total_ItemPriceWithoutTax = invL.Total_ItemPriceWithoutTax + item.Item_Total_WithoutTax;
                invL.Net_Total = invL.Net_Total + item.Item_Net_Total_Price;
                invL.Total_Item_CommissionValue = invL.Total_Item_CommissionValue + item.Net_Total_Item_Commission;
            }


        }
        public void ComputeTotalsReverse(Invoices_List_Reverse invL, IEnumerable<AICarsWorkOrdersItemProcessedDetail> ItemsP)
        {
            invL.Item_Net_TotalSpareParts = invL.Item_Net_TotalSpareParts ?? 0;
            invL.Item_Net_TotalOils = invL.Item_Net_TotalOils ?? 0;
            //checked nulls
            invL.Item_Net_TotalWorkmanship = invL.Item_Net_TotalWorkmanship ?? 0;
            invL.CommissionListWorkmanship_Rate = invL.CommissionListWorkmanship_Rate ?? 0;

            invL.CommissionListSpareParts_Rate = invL.CommissionListSpareParts_Rate ?? 0;
            invL.CommissionListOils_Rate = invL.CommissionListOils_Rate ?? 0;


            invL.CommissionListWorkmanship_Value = invL.CommissionListWorkmanship_Value ?? 0;

            invL.CommissionListSpareParts_Value = invL.CommissionListSpareParts_Value ?? 0;
            invL.CommissionListOils_Value = invL.CommissionListOils_Value ?? 0;
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
            invL.Total_ListPriceWithoutTax = invL.Total_ListPriceWithoutTax_Oils + invL.Total_ListPriceWithoutTax_SpareParts + invL.Total_ListPriceWithoutTax_Workmanship;

            invL.Total_PriceWithoutTax = invL.Total_ListPriceWithoutTax;// + invL.Total_ItemPriceWithoutTax;





            ///From invoice  Form  
            //    invL.ATS05ListWorkmanship_Rate
            //invL.txtATS05ListSpareParts_Rate
            //invL.ATS05ListOils_Rate

            invL.ATS05ListWorkmanship_Value = invL.Total_ListPriceWithoutTax_Workmanship * invL.ATS05ListWorkmanship_Rate / 100;
            invL.ATS05ListSpareParts_Value = invL.Total_ListPriceWithoutTax_SpareParts * invL.ATS05ListSpareParts_Rate / 100;
            invL.ATS05ListOils_Value = invL.Total_ListPriceWithoutTax_Oils * invL.ATS05ListOils_Rate / 100;

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
            invL.TotalListTaxSales10_Value = invL.TaxSalesListOils_Value + invL.TaxSalesListSpareParts_Value + invL.TaxSalesListWorkmanship_Value;
            invL.TaxSales10_Value = invL.TotalListTaxSales10_Value;


            ////Total After  Only  Taxe Sales
            ////////
            invL.Total_ListPriceAfterTaxSales_Workmanship = invL.Total_ListPriceWithoutTax_Workmanship + invL.TaxSalesListWorkmanship_Value;
            invL.Total_ListPriceAfterTaxSales_SpareParts = invL.Total_ListPriceWithoutTax_SpareParts + invL.TaxSalesListSpareParts_Value;
            invL.Total_ListPriceAfterTaxSales_Oils = invL.Total_ListPriceWithoutTax_Oils + invL.TaxSalesListOils_Value;
            invL.Total_ListPriceAfterTaxSales = invL.Total_ListPriceAfterTaxSales_Workmanship + invL.Total_ListPriceAfterTaxSales_SpareParts + invL.Total_ListPriceAfterTaxSales_Oils;



            ////Total After All Taxes
            ////////
            invL.NetInvoiceTotal_Workmanship = invL.Total_ListPriceWithoutTax_Workmanship + invL.TaxSalesListWorkmanship_Value - invL.ATS05ListWorkmanship_Value;
            invL.NetInvoiceTotal_SpareParts = invL.Total_ListPriceWithoutTax_SpareParts + invL.TaxSalesListSpareParts_Value - invL.ATS05ListSpareParts_Value;
            invL.NetInvoiceTotal_Oils = invL.Total_ListPriceWithoutTax_Oils + invL.TaxSalesListOils_Value - invL.ATS05ListOils_Value;
            invL.NetInvoiceTotal = invL.NetInvoiceTotal_Oils + invL.NetInvoiceTotal_SpareParts + invL.NetInvoiceTotal_Workmanship;
            invL.Net_Total = invL.NetInvoiceTotal;

            invL.Equivalent_Net_Total_Price = invL.Net_Total;
            invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

            invL.TafkeetNet_Total = TafkeetProj.NumberToText.GetText(invL.Net_Total ?? 0, invL.Currency_id ?? 1);

            ComputeTaxForItems(ref invL, ItemsP);
            SetValuesDecimals(ref invL);
        }
        void ComputeTaxForItems(ref Invoices_List_Reverse invL, IEnumerable<AICarsWorkOrdersItemProcessedDetail> ItemsP)
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
        void SetValuesDecimals(ref Invoices_List_Reverse invL)
        {
            invL.Item_Net_TotalWorkmanship = Common.RoundMePositive(invL.Item_Net_TotalWorkmanship);
            invL.CommissionListWorkmanship_Rate = Common.RoundMePositive(invL.CommissionListWorkmanship_Rate);
            invL.Item_Net_TotalSpareParts = Common.RoundMePositive(invL.Item_Net_TotalSpareParts);
            invL.Item_Net_TotalOils = Common.RoundMePositive(invL.Item_Net_TotalOils);
            invL.CommissionListSpareParts_Rate = Common.RoundMePositive(invL.CommissionListSpareParts_Rate);
            invL.CommissionListOils_Rate = Common.RoundMePositive(invL.CommissionListOils_Rate);
            invL.CommissionListWorkmanship_Value = Common.RoundMePositive(invL.CommissionListWorkmanship_Value);
            invL.CommissionListSpareParts_Value = Common.RoundMePositive(invL.CommissionListSpareParts_Value);
            invL.CommissionListOils_Value = Common.RoundMePositive(invL.CommissionListOils_Value);

            invL.Total = Common.RoundMePositive(invL.Total);


            invL.Net_Total = Common.RoundMePositive(invL.Net_Total);



            invL.List_CommissionRate = Common.RoundMePositive(invL.List_CommissionRate);

            invL.Total_CommissionListWorkmanship = Common.RoundMePositive(invL.Total_CommissionListWorkmanship);
            invL.Total_CommissionListSpareParts = Common.RoundMePositive(invL.Total_CommissionListSpareParts);
            invL.Total_CommissionListOils = Common.RoundMePositive(invL.Total_CommissionListOils);
            invL.Total_CommissionList = Common.RoundMePositive(invL.Total_CommissionList);

            invL.List_CommissionValue = Common.RoundMePositive(invL.List_CommissionValue);
            invL.List_Final_CommissionValue = Common.RoundMePositive(invL.List_Final_CommissionValue);

            ////////////
            invL.Net_Total_CommissionValue = Common.RoundMePositive(invL.Net_Total_CommissionValue);
            invL.Total_ListPriceWithoutTax_Workmanship = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_Workmanship);
            invL.Total_ListPriceWithoutTax_SpareParts = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_SpareParts);
            invL.Total_ListPriceWithoutTax_Oils = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_Oils);
            invL.Total_ListPriceWithoutTax = Common.RoundMePositive(invL.Total_ListPriceWithoutTax);
            invL.Total_PriceWithoutTax = Common.RoundMePositive(invL.Total_PriceWithoutTax);



            invL.ATS05ListWorkmanship_Value = Common.RoundMePositive(invL.ATS05ListWorkmanship_Value);
            invL.ATS05ListSpareParts_Value = Common.RoundMePositive(invL.ATS05ListSpareParts_Value);
            invL.ATS05ListOils_Value = Common.RoundMePositive(invL.ATS05ListOils_Value);

            invL.TotalListATS05_Value = Common.RoundMePositive(invL.TotalListATS05_Value);

            invL.ATS05_Value = Common.RoundMePositive(invL.ATS05_Value);
            invL.TaxSales10 = Common.RoundMePositive(invL.TaxSales10);
            invL.ATS05 = Common.RoundMePositive(invL.ATS05);


            invL.TaxSalesListWorkmanship_Value = Common.RoundMePositive(invL.TaxSalesListWorkmanship_Value);
            invL.TaxSalesListSpareParts_Value = Common.RoundMePositive(invL.TaxSalesListSpareParts_Value);
            invL.TaxSalesListOils_Value = Common.RoundMePositive(invL.TaxSalesListOils_Value);
            invL.TotalListTaxSales10_Value = Common.RoundMePositive(invL.TotalListTaxSales10_Value);
            invL.TaxSales10_Value = Common.RoundMePositive(invL.TaxSales10_Value);

            ////////
            invL.NetInvoiceTotal_Workmanship = Common.RoundMePositive(invL.NetInvoiceTotal_Workmanship);
            invL.NetInvoiceTotal_SpareParts = Common.RoundMePositive(invL.NetInvoiceTotal_SpareParts);
            invL.NetInvoiceTotal_Oils = Common.RoundMePositive(invL.NetInvoiceTotal_Oils);
            invL.NetInvoiceTotal = Common.RoundMePositive(invL.NetInvoiceTotal);

            invL.Equivalent_Net_Total_Price = Common.RoundMePositive(invL.Equivalent_Net_Total_Price);
            invL.Equivalent_Net_Total_CommissionValue = Common.RoundMePositive(invL.Equivalent_Net_Total_CommissionValue);


        }
        public void GetItemReversedProcessedListTotalsByCategory(List<CarsWorkOrdersItemReversedProcessedDetail> ItemProcessedListByCategory)
        {
            CarsWorkOrdersItemReversedProcessedDetail itemTNew = new CarsWorkOrdersItemReversedProcessedDetail();
            try
            {

                itemTNew.Id = 0;
                itemTNew.Item_Net_Total_Price = 0;// GetItemProcessedListTotals(ItemProcessedList);
                itemTNew.Qnty_value = 0;// 
                itemTNew.Item_Name = "الإجمالى";
                itemTNew.Invoice_Item_Name = "الإجمالى";
                foreach (var it in ItemProcessedListByCategory)
                {
                    it.Item_Net_Total_Price = it.Item_Net_Total_Price ?? 0;
                    it.Net_Total_Item_Commission = it.Net_Total_Item_Commission ?? 0;
                    it.Qnty_value = it.Qnty_value ?? 0;
                    it.Item_Total_WithoutTax = it.Item_Total_WithoutTax ?? 0;
                    it.Item_ATS05_Value = it.Item_ATS05_Value ?? 0;
                    it.Item_TaxSales10_Value = it.Item_TaxSales10_Value ?? 0;
                    it.Total = it.Total ?? 0;

                    itemTNew.Item_Net_Total_Price = itemTNew.Item_Net_Total_Price + it.Item_Net_Total_Price;
                    itemTNew.Total = itemTNew.Total + it.Total;
                    itemTNew.Net_Total_Item_Commission = itemTNew.Net_Total_Item_Commission + it.Net_Total_Item_Commission;
                    itemTNew.Qnty_value = itemTNew.Qnty_value + it.Qnty_value;
                    itemTNew.Item_Total_WithoutTax = itemTNew.Item_Total_WithoutTax + it.Item_Total_WithoutTax;
                    itemTNew.Item_ATS05_Value = itemTNew.Item_ATS05_Value + it.Item_ATS05_Value;
                    itemTNew.Item_TaxSales10_Value = itemTNew.Item_TaxSales10_Value + it.Item_TaxSales10_Value;

                }
            }
            catch (Exception ee)
            {

            }
            ItemProcessedListByCategory.Add(itemTNew);


        }


        public void BuildInvoiceReverse(ref int Excp, ref string Msg, Invoices_List_Reverse invL, CarsWorkOrdersCLS carsWorkOrderCls, List<CarsWorkOrdersItemReversedProcessedDetail> ItemProcessedList)
        {

            try
            {


                Requests_ModelCls rc = new Requests_ModelCls();
                Excp = 0;
                Msg = "";
                rc.Make_InvoiceReverse_ForCars(ref Excp, ref Msg, ref opp, carsWorkOrderCls, invL);


            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public List<Invoices_Details_Reverse> GetInvoices_Details_ReverseList(List<CarsWorkOrdersItemReversedProcessedDetail> ItemProcessedList)
        {
            Invoices_Details_Reverse ind = new Invoices_Details_Reverse();
            List<Invoices_Details_Reverse> indList = new List<Invoices_Details_Reverse>();
            foreach (var item in ItemProcessedList)
            {
                ind = new Invoices_Details_Reverse();
                //CarsWorkOrdersItemProcessedDetail item = new CarsWorkOrdersItemProcessedDetail();
                ind.CarsWorkOrdersItemProcessedDetailsId = item.Id;
                ind.Invoice_Id = 0;
                ind.Item_Id = item.Item_Id;
                ind.Unit_Item_Table_Id = item.Unit_ItemsId;
                ind.Specification = item.Specification;
                //ind.Item_Category_Id = item.Item_Category_Id;
                //ind.Item_Category_Name = item.Item_Category_Name;



                ind.Larg_Qnty = item.Larg_Qnty;
                ind.Small_Qnty = item.Small_Qnty;
                ind.Qnty_Another = item.Item_No;
                ind.Qnty = item.Qnty;
                ind.Larg_Unit_Price = item.Larg_Unit_Price;
                ind.Small_Unit_Price = item.Small_Unit_Price;
                ind.Unit_Used_for_Price = item.Unit_Used_for_Price;
                ind.Qnty_value = item.Qnty_value;

                ind.Item_ATS05 = item.Item_ATS05;
                ind.Item_ATS05_Value = item.Item_ATS05_Value;

                ind.Item_TaxSales10 = item.Item_TaxSales10;
                ind.Item_TaxSales10_Value = item.Item_TaxSales10_Value;

                ind.Total = item.Total;

                ind.Unit_Id = item.Unit_Id ?? 0;
                ind.UnitStr = item.UnitStr;
                ind.code = item.ItemCode;

                ind.Invoice_Item_Name = item.Invoice_Item_Name;

                ind.Store_Id = item.Store_Id ?? 0;
                ind.NameSpecification = item.Invoice_Item_Name;
                ind.Item_CommissionValue = item.Item_CommissionValue;
                ind.Is_Item_CommissionValue = item.Is_Item_CommissionValue;
                ind.Unit_CommissionValue = item.Unit_CommissionValue;



                ind.Net_Total_Item_Commission = item.Net_Total_Item_Commission;
                ind.Item_CommissionRate = item.Item_CommissionRate;
                ind.Unit_CommissionRate = item.Unit_CommissionRate;

                ind.Item_Total_WithoutTax = item.Item_Total_WithoutTax;
                ind.ItemCommissionTotal = item.ItemCommissionTotal;
                ind.Item_Net_Total_Price = item.Item_Net_Total_Price;

                ind.QntyStr = item.QntyStr;
                ind.QntyStrColumn = item.QntyStrColumn;
                ind.OperationDate = item.OperationDate;
                ind.User_Id = item.User_Id;
                ind.Branch_Id = item.Branch_Id;
                ind.Insert_ServerDateTime = item.Insert_ServerDateTime;
                ind.Update_ServerDateTime = item.Update_ServerDateTime;
                ind.EditOperationDate = item.EditOperationDate;
                ind.Edit_User_Id = item.Edit_User_Id;
                ind.ProcessNo = item.ProcessNo;
                if (item.Unit != null)
                {
                    ind.TotalSmall_Qnty = Cls_Conversion_Unit.GetTotalSmallQntity(item.Larg_Qnty ?? 0, item.Small_Qnty ?? 0, item.Unit.S_unit_Value ?? 0); ;

                    ind.TotalSmall_QntyStr = Common.GetQntyString("0", item.Unit.B_Unit.ToString(), ind.TotalSmall_Qnty.ToString(), item.Unit.S_unit_Value.ToString());
                    ind.TotalSmall_QntyStrColumn = ind.TotalSmall_QntyStr;
                }

                ind.S_price_LargeUnit = item.S_price_LargeUnit;
                ind.S_price_SmallUnit = item.S_price_SmallUnit;
                ind.Notes = item.Item_Notes;
                ind.Item_Notes = item.Item_Notes;


                ind.ATS05 = 0;
                ind.ATS05_Value = 0;
                ind.TaxSales10 = 0;
                ind.TaxSales10_Value = 0;
                ind.Order_Purshing_Id_Automatic = 0;
                ind.Make_Order = 0;
                ind.Balance_Checked = 0;
                ind.QntyWidth = 0;
                ind.QntyLenght = 0;
                ind.QntyItemNo = 0;
                ind.Bills_Details_Id = 0;
                ind.Purchase_Price = 0;
                ind.Audience_price = 0;
                ind.Item_Expire_Date = null;
                ind.Is_InvoiceItem_Kta3y = 0;

                ind.Is_Alternate_Item_CommissionValue = 0;
                ind.Alternate_Item_CommissionValue = 0;
                ind.Alternate_Item_CommissionRate = 0;
                ind.Alternate_ItemCommissionTotal = 0;

                ind.Alternate_Unit_CommissionRate = 0;
                ind.Alternate_Unit_CommissionValue = 0;
                ind.Unit_Item_ExpireDate_Id = 0;
                ind.Merchant_Name = "";
                ind.Tash3ela = "";


                ind.StatementNo = "";
                ind.OperationNo = "";

                ind.Supplier_BalanceSet = 0;
                ind.Item_DeletedFlage = 0;
                ind.Item_ProcessedFlage = 0;

                ind.Item_No_Free = 0;
                ind.Qnty_Free = 0;

                ind.Qnty_FreeStr = "";
                ind.LargItemsQnty_Free = 0;
                ind.SmallItemsQnty_Free = 0;


                indList.Add(ind);
            }
            return indList;
        }


        public void EditInvoiceReverse(ref int Excp, ref string Msg, CarsWorkOrdersCLS carsWorkOrderCls, Invoices_List_Reverse invL, List<Invoices_Details_Reverse> invDL, List<CarsWorkOrdersItemReversedProcessedDetail> ItemReversedProcessedList)
        {

            try
            {
                //List<Invoices_Details> indList = GetInvoices_DetailsList(ItemProcessedList);

                Requests_ModelCls rc = new Requests_ModelCls();
                Excp = 0;
                Msg = "";
                rc.Edit_InvoiceReverse_ForCars(ref Excp, ref Msg, ref opp, carsWorkOrderCls, invL, invDL);
            }
            catch (Exception ee)
            {
            }

        }
        public void DeleteInvoiceReverse(ref int Excp, ref string Msg, long Invoices_List_Id)
        {

            try
            {
                //List<Invoices_Details> indList = GetInvoices_DetailsList(ItemProcessedList);

                Requests_ModelCls rc = new Requests_ModelCls();
                Excp = 0;
                Msg = "";
                rc.Delete_InvoiceReverse_ForCars(ref Excp, ref Msg, ref opp, Invoices_List_Id);
            }
            catch (Exception ee)
            {
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
            }

        }

        
    }
}
