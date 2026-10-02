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
        public Car car = new Car();
        public Customer customer_CarOwner = new Customer();
        public Customer customer_ForAccount = new Customer();
        public CarsWorkOrder carWorkOrder = new CarsWorkOrder();

        public CarsWorkOrdersDeliverDetail ItemDelivered = new CarsWorkOrdersDeliverDetail();
        public List<CarsWorkOrdersDeliverDetail> ItemDeliveredList = new List<CarsWorkOrdersDeliverDetail>();


        public List<Invoices_List> invsList = new List<Invoices_List>();
        public List<Invoices_List_Reverse> invsRList = new List<Invoices_List_Reverse>();
        public CarsWorkOrdersRequestedDetail ItemRequest = new CarsWorkOrdersRequestedDetail();
        public List<CarsWorkOrdersRequestedDetail> ItemRequestList = new List<CarsWorkOrdersRequestedDetail>();
        public List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList = new List<CarsWorkOrdersItemProcessedDetail>();
        public List<AICarsWorkOrdersItemProcessedDetail> AIItemProcessedList = new List<AICarsWorkOrdersItemProcessedDetail>();
        public List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList_SpareParts;
        public List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList_Oils;
        public List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList_Workmanships;
        public List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList_RawMaterials;

        public List<CarsWorkOrdersItemReversedProcessedDetail> ItemReversedProcessedList = new List<CarsWorkOrdersItemReversedProcessedDetail>();
        public List<AICarsWorkOrdersItemProcessedDetail> AIItemReversedProcessedList = new List<AICarsWorkOrdersItemProcessedDetail>();
        public List<CarsWorkOrdersItemReversedProcessedDetail> ItemReversedProcessedList_SpareParts;
        public List<CarsWorkOrdersItemReversedProcessedDetail> ItemReversedProcessedList_Oils;
        public List<CarsWorkOrdersItemReversedProcessedDetail> ItemReversedProcessedList_Workmanships;
        public List<CarsWorkOrdersItemReversedProcessedDetail> ItemReversedProcessedList_RawMaterials;

        public CarsWorkOrdersViewModel CarsWorkOrdersViewModel = new CarsWorkOrdersViewModel();
        //public Customer customer = new Customer();
        public AccountModelsLib.operations opp;
        public CarsWorkOrdersCLS()
        {
            opp = new AccountModelsLib.operations();
        }
        public CarsWorkOrdersCLS(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
        }
        public Car getCar()
        {
            return car;
        }
        public CarsWorkOrder geCarWorkOrder(long Id)
        {
            carWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == Id).Include(c => c.Car).FirstOrDefault();
            return carWorkOrder;
        }
        public void getCarWorkOrderId(long Id)
        {
            try
            {
                carWorkOrder = new CarsWorkOrder();
                carWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == Id).Include(a => a.Car).Include(a => a.Customer).FirstOrDefault();
                car = carWorkOrder.Car;

                customer_ForAccount = carWorkOrder.Customer;

                CarsCLS carCLS = new CarsCLS(ref opp);
                customer_CarOwner = carCLS.getCarCustomerOwner(car);



            }
            catch (Exception ee)
            {


            }
        }

        public void AddWorkOrder(ref int Excp, ref string Msg)
        {
            try
            {
                opp = new AccountModelsLib.operations(2);
                carWorkOrder.WorkOrderNumber = GetNewWorkOrderNumber();
                if (carWorkOrder.EntryDateTime != null)
                {
                    carWorkOrder.EntryDateOnly = new DateTime(carWorkOrder.EntryDateTime.Value.Year, carWorkOrder.EntryDateTime.Value.Month, carWorkOrder.EntryDateTime.Value.Day);

                    carWorkOrder.EntryTimeOnly = new TimeSpan(carWorkOrder.EntryDateTime.Value.Hour, carWorkOrder.EntryDateTime.Value.Minute, carWorkOrder.EntryDateTime.Value.Second);
                }


                opp.db.CarsWorkOrders.Add(carWorkOrder);
                opp.db.SaveChanges();
                Excp = 0;
                opp.Commit();
            }
            catch (Exception ee)
            {
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public long GetNewWorkOrderNumber()
        {

            long WorkOrderNumber = 0;
            int year = carWorkOrder.WorkOrdersDate.Value.Year;

            long? maxAge = opp.db.CarsWorkOrders.Where(p => p.WorkOrdersDate.Value.Year == year).Select(p => p.WorkOrderNumber).DefaultIfEmpty(0).Max();

            //var estado = opp.db.CarsWorkOrders.Where(p => p.WorkOrdersDate.Value.Year == year).Max(c => c.WorkOrderNumber);

            //IQueryable<CarsWorkOrder> query = opp.db.CarsWorkOrders.Where(p => p.WorkOrdersDate.Value.Year == year);
            //long? v = query.Max(c => c.WorkOrderNumber);


            if (maxAge.HasValue)
            {
                if (maxAge.Value == 0)
                {
                    WorkOrderNumber = 1;
                }
                else
                {
                    WorkOrderNumber = maxAge.Value;
                    WorkOrderNumber++;
                }

            }

            return WorkOrderNumber;
        }
        public void EditWorkOrder(ref int Excp, ref string Msg)
        {
            try
            {
                opp.db.Entry(carWorkOrder).State =  EntityState.Modified;
                opp.db.SaveChanges();


                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void DeletetWorkOrder(ref int Excp, ref string Msg)
        {
            try
            {
                Excp = 0;
                opp = new AccountModelsLib.operations(2);
                if (carWorkOrder == null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف أمر الشغل";


                }
                long CarsWorkOrdersId = carWorkOrder.Id;
                var invoice = opp.db.Invoices_List.Where(a => a.CarsWorkOrdersId == CarsWorkOrdersId);
                if (invoice.Any())
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف أمر الشغل لتسجيل فاتورة ";

                }




                CarsWorkOrdersItemProcessedDetail ItemProcessed = opp.db.CarsWorkOrdersItemProcessedDetails.Where(a => a.CarsWorkOrdersId == CarsWorkOrdersId).FirstOrDefault();
                if (ItemProcessed != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف أمر الشغل لتسجيل أصناف عليها  ";

                    //opp.db.Entry(ItemProcessed).State = System.Data.Entity.EntityState.Deleted;
                    //opp.db.SaveChanges();
                }

                CarsWorkOrdersRequestedDetail request = opp.db.CarsWorkOrdersRequestedDetails.Where(a => a.CarsWorkOrdersId == CarsWorkOrdersId).FirstOrDefault();
                if (request != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف أمر الشغل لتسجيل أصناف عليها  ";

                    //opp.db.Entry(request).State = System.Data.Entity.EntityState.Deleted;
                    //opp.db.SaveChanges();
                }
                Detailes_Purshing detailes_Purshing = opp.db.Detailes_Purshings.Where(a => a.CarsWorkOrdersId == CarsWorkOrdersId).FirstOrDefault();
                if (detailes_Purshing != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف أمر الشغل لتسجيل أصناف عليها من المخزن  ";

                    //opp.db.Entry(request).State = System.Data.Entity.EntityState.Deleted;
                    //opp.db.SaveChanges();
                }

                if (Excp == 0)
                {
                    CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(i => i.Id == CarsWorkOrdersId).FirstOrDefault();
                    if (carsWorkOrder != null)
                    {
                        opp.db.CarsWorkOrders.Remove(carsWorkOrder);
                        opp.db.SaveChanges();
                    }
                    //    opp.db.Entry(carWorkOrder).State = System.Data.Entity.EntityState.Deleted;
                    //opp.db.SaveChanges();
                    opp.Commit();
                    Excp = 0;
                }
            }
            catch (Exception ee)
            {
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }

        }

        public void EditWorkOrderWithItems(ref int Excp, ref string Msg, List<CarsWorkOrdersRequestedDetail> deletedcarWorkOrderDetailItemsList)
        {
            opp.BeginTransaction(2);

            EditWorkOrder(ref Excp, ref Msg);
            if (Excp == 1)
            {
                opp.Rollback();
                return;
            }
            //carWorkOrderDetailItemsList  List<CarsWorkOrdersDetail> carWorkOrderDetailItemsList
            CarsWorkOrdersDetailCLS carsWorkOrdersDetailCLS = new CarsWorkOrdersDetailCLS();
            carsWorkOrdersDetailCLS.AddOrEditDetails(ref Excp, ref Msg, ref opp, ItemRequestList);

            ////////// delete item
            ///

            carsWorkOrdersDetailCLS.DeleteDetails(ref Excp, ref Msg, ref opp, deletedcarWorkOrderDetailItemsList);

            if (Excp == 0)
            {
                opp.Commit();
            }
            else
            {
                opp.Rollback();
            }
            /////////SaveItems

        }
        public List<CarsWorkOrdersViewModel> getCarsWorkOrder(long CarsWorkOrdersId_)
        {
            ResetSearchCriteria();
            CarsWorkOrdersId = CarsWorkOrdersId_;
            //string SearchStr = "";
            //long CustomerId = 0;
            //int IsWorkOrderClosed = -1;
            //bool IsEntryDateTimeFrom = false;
            //DateTime EntryDateTimeFrom = DateTime.Now;
            //bool IsEntryDateTimeTo = false;
            //DateTime EntryDateTimeTo = DateTime.Now;

            return getCarsWorkOrder();

        }
        public List<CarsWorkOrdersViewModel> getCarsWorkOrderWithDetailItems(long CarsWorkOrdersId_)
        {
            ResetSearchCriteria();
            CarsWorkOrdersId = CarsWorkOrdersId_;


            ///////get item for work order
            CarsWorkOrdersDetailCLS carsWorkOrdersDetailCLS = new CarsWorkOrdersDetailCLS();
            ItemRequestList = new List<CarsWorkOrdersRequestedDetail>();
            ItemRequestList = carsWorkOrdersDetailCLS.GetCarsWorkOrdersRequestedDetails(ref opp, CarsWorkOrdersId);


            ///get work order data by CarsWorkOrdersId
            List<CarsWorkOrdersViewModel> CarsWorkOrdersVMList = getCarsWorkOrder();
            if (CarsWorkOrdersVMList != null)
            {
                CarsWorkOrdersViewModel = CarsWorkOrdersVMList.FirstOrDefault();
            }
            return CarsWorkOrdersVMList;

        }

        public List<CarsWorkOrdersViewModel> getCarsWorkOrderDeiveredProcessedDetailItems(long CarsWorkOrdersId_, int Is_AddToInvoice)
        {



            ///////get request item for work order
            CarsWorkOrdersDetailCLS carsWorkOrdersDetailCLS = new CarsWorkOrdersDetailCLS();
            ItemRequestList = new List<CarsWorkOrdersRequestedDetail>();
            ItemRequestList = carsWorkOrdersDetailCLS.GetCarsWorkOrdersRequestedDetails(ref opp, CarsWorkOrdersId_);

            ///////get item for Item Delivered
            CarsWorkOrdersDeliverDetailCLS ItemDeliver = new CarsWorkOrdersDeliverDetailCLS();
            ItemDeliveredList = new List<CarsWorkOrdersDeliverDetail>();
            ItemDeliveredList = ItemDeliver.SetCarsWorkOrdersItemDeliverDetail(ref opp, CarsWorkOrdersId_);



            ///////get item for ItemProcessedDetail
            ///
            GetItemProcessedDetail(ref opp, CarsWorkOrdersId_, Is_AddToInvoice);

            //GetItemReversedProcessedDetail(ref opp, CarsWorkOrdersId_, Is_AddToInvoice);
            ///get work order data by CarsWorkOrdersId
            ResetSearchCriteria();
            CarsWorkOrdersId = CarsWorkOrdersId_;
            List<CarsWorkOrdersViewModel> CarsWorkOrdersVMList = getCarsWorkOrder();
            if (CarsWorkOrdersVMList != null)
            {
                CarsWorkOrdersViewModel = CarsWorkOrdersVMList.FirstOrDefault();
            }


            ////////////Get Invoices List for workorder
            ///
            invsList = GetInvoices_List(ref opp, CarsWorkOrdersId_);

            return CarsWorkOrdersVMList;

        }

        public void GetItemProcessedDetail(ref AccountModelsLib.operations opp, long carsWorkOrdersId_, int Is_AddToInvoice)
        {
            CarsWorkOrdersItemProcessedDetailCLS ItemProcessedDetail = new CarsWorkOrdersItemProcessedDetailCLS();
            ItemProcessedList = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList = ItemProcessedDetail.GetCarsWorkOrdersItemProcessedDetailsList(ref opp, carsWorkOrdersId_, Is_AddToInvoice);
            foreach (CarsWorkOrdersItemProcessedDetail item in ItemProcessedList)
            {
                item.P_Price = item.P_Price ?? 0;
                item.Qnty = item.Qnty ?? 0;
                item.Qnty_Value_P_Price = item.Qnty_Value_P_Price ?? 0;

                item.Qnty_Value_P_Price = item.P_Price * item.Qnty;

                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Qnty_value = item.Qnty_value ?? 0;

                item.Profits = item.Qnty_value - item.Qnty_Value_P_Price;

                item.Qnty_Value_P_Price = Common.RoundMe(item.Qnty_Value_P_Price);
                item.Profits = Common.RoundMe(item.Profits);
                item.Qnty_value = Common.RoundMe(item.Qnty_value);

            }
        }
        public void GetItemReversedProcessedDetail(ref AccountModelsLib.operations opp, long carsWorkOrdersId_, int Is_AddToInvoice)
        {
            CarsWorkOrdersItemReversedProcessedDetailCLS ItemProcessedDetail = new CarsWorkOrdersItemReversedProcessedDetailCLS();
            ItemReversedProcessedList = new List<CarsWorkOrdersItemReversedProcessedDetail>();
            ItemReversedProcessedList = ItemProcessedDetail.GetCarsWorkOrdersItemReversedProcessedDetailsList(ref opp, carsWorkOrdersId_, Is_AddToInvoice);

        }
        public List<Invoices_List> GetInvoices_List(ref AccountModelsLib.operations opp, long CarsWorkOrdersId_)
        {
            List<Invoices_List> invsList = new List<Invoices_List>();
            Invoices_ListCls lnvcls = new Invoices_ListCls();
            invsList = lnvcls.GetInvoices_ListByCarsWorkOrdersId(ref opp, CarsWorkOrdersId_);
            return invsList;
        }
        public List<Invoices_List_Reverse> GetInvoices_List_Reverse(ref AccountModelsLib.operations opp, long CarsWorkOrdersId_)
        {
            List<Invoices_List_Reverse> invsList = new List<Invoices_List_Reverse>();
            Invoices_List_ReverseCls lnvcls = new Invoices_List_ReverseCls();
            invsList = lnvcls.GetInvoices_List_ReverseByCarsWorkOrdersId(ref opp, CarsWorkOrdersId_);
            return invsList;
        }
        public List<CarsWorkOrdersViewModel> getItemsProcessedForInvoice(long CarsWorkOrdersId_, int Is_AddToInvoice)
        {

            long Item_Category_ID = 0;
            ///////get item for ItemProcessedDetail
            CarsWorkOrdersItemProcessedDetailCLS ItemProcessedDetailCLS = new CarsWorkOrdersItemProcessedDetailCLS();


            // اجمالى الخامات التى تم اضافتها على صنف يسمى اجمالى الخامات

            // SetComputeRawMaterialsTotalsOld(ref opp, ItemProcessedDetailCLS, CarsWorkOrdersId_, Is_AddToInvoice);
            //CarsWorkOrdersItemProcessedDetail Khamat = SetComputeRawMaterialsTotals(CarsWorkOrdersId_, Is_AddToInvoice);
            //if(Khamat.Qnty_value>0)
            //{

            //}


            ///////////////
            ItemProcessedList = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList = ItemProcessedDetailCLS.GetCarsWorkOrdersItemProcessedDetailsList(ref opp, CarsWorkOrdersId_, Is_AddToInvoice);

            Item_Category_ID = (long)ItemCategoryEnum.SpareParts;
            ItemProcessedList_SpareParts = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList_SpareParts = ItemProcessedDetailCLS.GetItemProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);

            Item_Category_ID = (long)ItemCategoryEnum.Oils;
            ItemProcessedList_Oils = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList_Oils = ItemProcessedDetailCLS.GetItemProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);

            Item_Category_ID = (long)ItemCategoryEnum.Masnaheaiat;
            ItemProcessedList_Workmanships = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList_Workmanships = ItemProcessedDetailCLS.GetItemProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);




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

        public void getItemsProcessedForInvoicePreview(ref Invoices_List invL, long CarsWorkOrdersId_, int Is_AddToInvoice)
        {

            long Item_Category_ID = 0;
            ///////get item for ItemProcessedDetail
            CarsWorkOrdersItemProcessedDetailCLS ItemProcessedDetailCLS = new CarsWorkOrdersItemProcessedDetailCLS();

            ItemProcessedList = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList = ItemProcessedDetailCLS.GetCarsWorkOrdersItemProcessedDetailsList(ref opp, CarsWorkOrdersId_, Is_AddToInvoice);



            Item_Category_ID = (long)ItemCategoryEnum.Masnaheaiat;
            ItemProcessedList_Workmanships = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList_Workmanships = ItemProcessedDetailCLS.GetItemProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);




            Item_Category_ID = (long)ItemCategoryEnum.SpareParts;
            ItemProcessedList_SpareParts = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList_SpareParts = ItemProcessedDetailCLS.GetItemProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);

            Item_Category_ID = (long)ItemCategoryEnum.Oils;
            ItemProcessedList_Oils = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList_Oils = ItemProcessedDetailCLS.GetItemProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);

            //// Collect spare parts + oils
            ItemProcessedList_SpareParts.AddRange(ItemProcessedList_Oils);


            ////////////////Compute Totals
            invL = new Invoices_List();
            invL.Total = invL.Total ?? 0;
            invL.Qnty_Value = invL.Qnty_Value ?? 0;
            invL.TaxSales10 = invL.TaxSales10 ?? 0;
            invL.ATS05 = invL.ATS05 ?? 0;
            invL.TaxSales10_Value = invL.TaxSales10_Value ?? 0;
            invL.ATS05_Value = invL.ATS05_Value ?? 0;
            invL.NetInvoiceTotal = invL.NetInvoiceTotal ?? 0;
            invL.NetInvoiceTotal_Workmanship = invL.NetInvoiceTotal_Workmanship ?? 0;
            invL.NetInvoiceTotal_SpareParts = invL.NetInvoiceTotal_SpareParts ?? 0;
            invL.NetInvoiceTotal_Oils = invL.NetInvoiceTotal_Oils ?? 0;
            invL.NetInvoiceTotal_SparePartsOils = invL.NetInvoiceTotal_SparePartsOils ?? 0;
            invL.Total_ListPriceWithoutTax = invL.Total_ListPriceWithoutTax ?? 0;
            invL.TotalListTaxSales10_Value = invL.TotalListTaxSales10_Value ?? 0;
            invL.Total_ListPriceAfterTaxSales = invL.Total_ListPriceAfterTaxSales ?? 0;
            invL.TotalListATS05_Value = invL.TotalListATS05_Value ?? 0;
            invL.Qnty_ValueWorkmanship = invL.Qnty_ValueWorkmanship ?? 0;
            invL.Qnty_ValueSpareParts = invL.Qnty_ValueSpareParts ?? 0;
            invL.Qnty_ValueOils = invL.Qnty_ValueOils ?? 0;
            invL.Qnty_ValueSparePartsOils = invL.Qnty_ValueSparePartsOils ?? 0;
            invL.Qnty_ValueRawMaterials = invL.Qnty_ValueRawMaterials ?? 0;


            invL.Net_Total = invL.Net_Total ?? 0;

            foreach (var item in ItemProcessedList)
            {
                if (item.Item_Category_Id == (long)ItemCategoryEnum.RawMaterials)
                {
                    continue;
                }
                invL.Total = invL.Total + item.Total;
                invL.Qnty_Value = invL.Qnty_Value + item.Qnty_value;


                if (item.Item_Category_Id == (long)ItemCategoryEnum.Masnaheaiat)
                {
                    invL.Qnty_ValueWorkmanship = invL.Qnty_ValueWorkmanship + item.Qnty_value;
                }
                if (item.Item_Category_Id == (long)ItemCategoryEnum.Oils)
                {
                    invL.Qnty_ValueOils = invL.Qnty_ValueOils + item.Qnty_value;
                }
                if (item.Item_Category_Id == (long)ItemCategoryEnum.SpareParts)
                {
                    invL.Qnty_ValueSpareParts = invL.Qnty_ValueSpareParts + item.Qnty_value;
                }
                ///+
                invL.Qnty_ValueSparePartsOils = invL.Qnty_ValueOils + invL.Qnty_ValueSpareParts;

                if (item.Item_Category_Id == (long)ItemCategoryEnum.RawMaterials)
                {
                    invL.Qnty_ValueRawMaterials = invL.Qnty_ValueRawMaterials + item.Qnty_value;
                }

            }
            invL.TaxSales10 = 0;
            invL.ATS05 = 0;
            invL.TaxSales10_Value = 0;
            invL.ATS05_Value = 0;

            //            ضريبة القيمة المضافة
            //كود رئيسى T1
            TaxListCLS tax = new TaxListCLS();

            List<AccountTaxInfo> accTaxList = tax.Get_TaxInfoRatioList(ref opp);

            AccountTaxInfo accInfo = accTaxList.Where(x => x.MainCode == "T1").FirstOrDefault();

            accInfo = tax.Get_TaxInfoRatio(ref opp, "T1", "");
            if (accInfo != null)
            {
                accInfo.TaxRate = accInfo.TaxRate ?? 0;
                accInfo.MinAllowedValueToComputeTax = accInfo.MinAllowedValueToComputeTax ?? 0;
                if (accInfo.MinAllowedValueToComputeTax == 0)
                {
                    invL.TaxSalesListWorkmanship_Rate = accInfo.TaxRate.Value * 100;
                    invL.TaxSalesListSpareParts_Rate = accInfo.TaxRate.Value * 100;
                    invL.TaxSalesListOils_Rate = accInfo.TaxRate.Value * 100;
                    ////+
                    invL.TaxSalesListSparePartsOils_Rate = accInfo.TaxRate.Value * 100;
                }
                else
                {
                    invL.TaxSalesListWorkmanship_Rate = 0;
                    invL.TaxSalesListSpareParts_Rate = 0;
                    invL.TaxSalesListOils_Rate = 0;
                    ////+
                    invL.TaxSalesListSparePartsOils_Rate = 0;

                    if (invL.Total_ListPriceWithoutTax_Workmanship.Value > accInfo.MinAllowedValueToComputeTax)
                    {
                        invL.TaxSalesListWorkmanship_Rate = accInfo.TaxRate.Value * 100;
                    }

                    if (invL.Total_ListPriceWithoutTax_SpareParts.Value > accInfo.MinAllowedValueToComputeTax)
                    {
                        invL.TaxSalesListSpareParts_Rate = accInfo.TaxRate.Value * 100;
                        ////+
                        invL.TaxSalesListSparePartsOils_Rate = accInfo.TaxRate.Value * 100;
                    }
                    if (invL.Total_ListPriceWithoutTax_Oils.Value > accInfo.MinAllowedValueToComputeTax)
                    {
                        invL.TaxSalesListOils_Rate = accInfo.TaxRate.Value * 100;
                        ////+
                        invL.TaxSalesListSparePartsOils_Rate = accInfo.TaxRate.Value * 100;
                    }
                }

            }
            else
            {
                invL.TaxSalesListWorkmanship_Rate = 0;
                invL.TaxSalesListSpareParts_Rate = 0;
                invL.TaxSalesListOils_Rate = 0;
                ////+
                invL.TaxSalesListSparePartsOils_Rate = 0;

            }



            invL.Total_ListPriceWithoutTax = invL.Qnty_Value;
            invL.Total_ListPriceWithoutTax_Workmanship = invL.Qnty_ValueWorkmanship;
            invL.Total_ListPriceWithoutTax_SpareParts = invL.Qnty_ValueSpareParts;
            invL.Total_ListPriceWithoutTax_Oils = invL.Qnty_ValueOils;
            ////+
            invL.Total_ListPriceWithoutTax_SparePartsOils = invL.Total_ListPriceWithoutTax_SpareParts + invL.Total_ListPriceWithoutTax_Oils;



            invL.TaxSalesListWorkmanship_Value = invL.Total_ListPriceWithoutTax_Workmanship * invL.TaxSalesListWorkmanship_Rate / 100;
            invL.TaxSalesListSpareParts_Value = invL.Total_ListPriceWithoutTax_SpareParts * invL.TaxSalesListSpareParts_Rate / 100;
            invL.TaxSalesListOils_Value = invL.Total_ListPriceWithoutTax_Oils * invL.TaxSalesListOils_Rate / 100;

            ////+
            invL.TaxSalesListSparePartsOils_Value = invL.TaxSalesListSpareParts_Value + invL.TaxSalesListOils_Value;

            invL.TotalListTaxSales10_Value = invL.TaxSalesListWorkmanship_Value + invL.TaxSalesListSpareParts_Value + invL.TaxSalesListOils_Value;

            invL.Total_ListPriceAfterTaxSales = invL.TotalListTaxSales10_Value + invL.Total_ListPriceWithoutTax;
            invL.NetInvoiceTotal = invL.Total_ListPriceAfterTaxSales;
            invL.Net_Total = invL.NetInvoiceTotal;
            invL.TafkeetNet_Total = TafkeetProj.NumberToText.GetText(invL.Net_Total ?? 0, invL.Currency_id ?? 1);


            invL.Total = Common.RoundMe(invL.Total);
            invL.Qnty_Value = Common.RoundMe(invL.Qnty_Value);
            invL.TaxSales10 = Common.RoundMe(invL.TaxSales10);
            invL.ATS05 = Common.RoundMe(invL.ATS05);
            invL.TaxSales10_Value = Common.RoundMe(invL.TaxSales10_Value);
            invL.ATS05_Value = Common.RoundMe(invL.ATS05_Value);
            invL.NetInvoiceTotal = Common.RoundMe(invL.NetInvoiceTotal);
            invL.NetInvoiceTotal_Workmanship = Common.RoundMe(invL.NetInvoiceTotal_Workmanship);
            invL.NetInvoiceTotal_SpareParts = Common.RoundMe(invL.NetInvoiceTotal_SpareParts);
            invL.NetInvoiceTotal_Oils = Common.RoundMe(invL.NetInvoiceTotal_Oils);
            invL.NetInvoiceTotal_SparePartsOils = Common.RoundMe(invL.NetInvoiceTotal_SparePartsOils);
            invL.Total_ListPriceWithoutTax = Common.RoundMe(invL.Total_ListPriceWithoutTax);
            invL.TotalListTaxSales10_Value = Common.RoundMe(invL.TotalListTaxSales10_Value);
            invL.Total_ListPriceAfterTaxSales = Common.RoundMe(invL.Total_ListPriceAfterTaxSales);
            invL.TotalListATS05_Value = Common.RoundMe(invL.TotalListATS05_Value);
            invL.Qnty_ValueWorkmanship = Common.RoundMe(invL.Qnty_ValueWorkmanship);
            invL.Qnty_ValueSpareParts = Common.RoundMe(invL.Qnty_ValueSpareParts);
            invL.Qnty_ValueOils = Common.RoundMe(invL.Qnty_ValueOils);
            invL.Qnty_ValueSparePartsOils = Common.RoundMe(invL.Qnty_ValueSparePartsOils);
            invL.Qnty_ValueRawMaterials = Common.RoundMe(invL.Qnty_ValueRawMaterials);

        }
        public CarsWorkOrdersItemProcessedDetail SetComputeRawMaterialsTotals(long CarsWorkOrdersId_, int Is_AddToInvoice)
        {
            CarsWorkOrdersItemProcessedDetailCLS ItemProcessedDetailCLS = new CarsWorkOrdersItemProcessedDetailCLS();

            CarsWorkOrdersItemProcessedDetail ProRawMaterials = new CarsWorkOrdersItemProcessedDetail();
            //return ProRawMaterials;

            long Item_Category_ID = 0;
            ///////get item for ItemProcessedDetail



            Item_Category_ID = (long)ItemCategoryEnum.RawMaterials;
            ItemProcessedList_RawMaterials = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList_RawMaterials = ItemProcessedDetailCLS.GetItemProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);
            //// Get ItemName : IsMasna3iatKhamat=1;
            if (ItemProcessedList_RawMaterials.Count > 0)
            {
                ProRawMaterials = SetComputeRawMaterialsTotals(CarsWorkOrdersId_, ItemProcessedList_RawMaterials);
            }

            return ProRawMaterials;
        }

        public CarsWorkOrdersItemProcessedDetail SetComputeRawMaterialsTotalsOld(ref AccountModelsLib.operations opp, CarsWorkOrdersItemProcessedDetailCLS ItemProcessedDetailCLS, long CarsWorkOrdersId_, int Is_AddToInvoice)
        {
            CarsWorkOrdersItemProcessedDetail ProRawMaterials = new CarsWorkOrdersItemProcessedDetail();
            return ProRawMaterials;

            long Item_Category_ID = 0;
            ///////get item for ItemProcessedDetail

            //////
            //
            //حذف الصف لاعادة حفظه
            Item_Category_ID = (long)ItemCategoryEnum.Masnaheaiat;
            List<CarsWorkOrdersItemProcessedDetail> ItemProcessedToDelete = ItemProcessedDetailCLS.GetItemProcessedListByCategoryAndIsMasna3iatKhamat(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);
            if (ItemProcessedToDelete.Count > 0)
            {
                opp.db.CarsWorkOrdersItemProcessedDetails.RemoveRange(ItemProcessedToDelete);
                opp.db.SaveChanges();
            }

            ///////////إضافة مصنعيات الخامات //////////////////


            Item_Category_ID = (long)ItemCategoryEnum.RawMaterials;
            ItemProcessedList_RawMaterials = new List<CarsWorkOrdersItemProcessedDetail>();
            ItemProcessedList_RawMaterials = ItemProcessedDetailCLS.GetItemProcessedListByCategory(ref opp, CarsWorkOrdersId_, Item_Category_ID, Is_AddToInvoice);
            //// Get ItemName : IsMasna3iatKhamat=1;
            if (ItemProcessedList_RawMaterials.Count > 0)
            {
                ProRawMaterials = SetComputeRawMaterialsTotalsOld(CarsWorkOrdersId_, ItemProcessedList_RawMaterials);
            }

            return ProRawMaterials;
        }
        public CarsWorkOrdersItemProcessedDetail SetComputeRawMaterialsTotals(long CarsWorkOrdersId, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            CarsWorkOrdersItemProcessedDetail ProRawMaterials = new CarsWorkOrdersItemProcessedDetail(1);



            ProRawMaterials.Qnty_value = ProRawMaterials.Qnty_value ?? 0;
            ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Item_Net_Total_Price ?? 0;
            ProRawMaterials.Item_Total_WithoutTax = ProRawMaterials.Item_Total_WithoutTax ?? 0;
            ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Item_Net_Total_Price ?? 0;
            ProRawMaterials.Net_Total_Item_Commission = ProRawMaterials.Net_Total_Item_Commission ?? 0;
            foreach (var item in ItemProcessedList)
            {
                item.Qnty_value = item.Qnty_value ?? 0;
                item.P_Price = item.P_Price ?? 0;
                item.P_priceAvgCost = item.P_priceAvgCost ?? 0;
                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Item_Total_WithoutTax = item.Item_Total_WithoutTax ?? 0;
                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Net_Total_Item_Commission = item.Net_Total_Item_Commission ?? 0;

                ///  خامات
                if (item.Items.Item_Category_ID == (long)ItemCategoryEnum.RawMaterials)
                {
                    ProRawMaterials.P_priceAvgCost = ProRawMaterials.P_priceAvgCost + item.P_priceAvgCost;
                    ProRawMaterials.P_Price = ProRawMaterials.P_Price + item.P_Price;
                    ProRawMaterials.Qnty_value = ProRawMaterials.Qnty_value + item.Qnty_value;
                    ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Item_Net_Total_Price + item.Item_Net_Total_Price;
                    ProRawMaterials.Item_Total_WithoutTax = ProRawMaterials.Item_Total_WithoutTax + item.Item_Total_WithoutTax;
                    ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Item_Net_Total_Price + item.Item_Net_Total_Price;
                    ProRawMaterials.Net_Total_Item_Commission = ProRawMaterials.Net_Total_Item_Commission + item.Net_Total_Item_Commission;
                }
            }

            return ProRawMaterials;
        }

        public CarsWorkOrdersItemProcessedDetail SetComputeRawMaterialsTotalsOld(long CarsWorkOrdersId, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            CarsWorkOrdersItemProcessedDetail ProRawMaterials = new CarsWorkOrdersItemProcessedDetail(1);



            ProRawMaterials.Qnty_value = ProRawMaterials.Qnty_value ?? 0;
            ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Item_Net_Total_Price ?? 0;
            ProRawMaterials.Item_Total_WithoutTax = ProRawMaterials.Item_Total_WithoutTax ?? 0;
            ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Item_Net_Total_Price ?? 0;
            ProRawMaterials.Net_Total_Item_Commission = ProRawMaterials.Net_Total_Item_Commission ?? 0;
            foreach (var item in ItemProcessedList)
            {
                item.Qnty_value = item.Qnty_value ?? 0;
                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Item_Total_WithoutTax = item.Item_Total_WithoutTax ?? 0;
                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Net_Total_Item_Commission = item.Net_Total_Item_Commission ?? 0;

                ///  خامات
                if (item.Items.Item_Category_ID == (long)ItemCategoryEnum.RawMaterials)
                {
                    ProRawMaterials.Qnty_value = ProRawMaterials.Qnty_value + item.Qnty_value;
                    ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Item_Net_Total_Price + item.Item_Net_Total_Price;
                    ProRawMaterials.Item_Total_WithoutTax = ProRawMaterials.Item_Total_WithoutTax + item.Item_Total_WithoutTax;
                    ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Item_Net_Total_Price + item.Item_Net_Total_Price;
                    ProRawMaterials.Net_Total_Item_Commission = ProRawMaterials.Net_Total_Item_Commission + item.Net_Total_Item_Commission;
                }
            }
            int Excp = 0;
            ItemCLS itmCls = new ItemCLS();
            List<UnitItemsModelView> itmRaw = itmCls.GetUnit_ItemBYIsMasna3iatKhamat(ref Excp, ref opp);
            if (itmRaw.Count == 0)
            {
                itmCls.SetItemIsMasna3iatKhamat(ref opp);
                itmRaw = itmCls.GetUnit_ItemBYIsMasna3iatKhamat(ref Excp, ref opp);
            }
            if (itmRaw.Count == 0)
            {
                ProRawMaterials = null;
                return ProRawMaterials;
            }
            if (itmRaw.Count > 0)
            {
                ProRawMaterials.Item_Id = itmRaw[0].Item_Id;
                ProRawMaterials.Item_Name = itmRaw[0].Item_Name;
                ProRawMaterials.Invoice_Item_Name = itmRaw[0].ItemName_OnInvoice;
                ProRawMaterials.Unit_Id = itmRaw[0].Unit_Id;
                ProRawMaterials.Unit_ItemsId = itmRaw[0].Unit_Item_Id;
                ProRawMaterials.UnitStr = itmRaw[0].UnitStr;
                ProRawMaterials.Item_Category_Id = itmRaw[0].Item_Category_Id;
                ProRawMaterials.Item_Category_Name = itmRaw[0].Item_Category_Name;

            }
            ProRawMaterials.IsMasna3iatKhamat = 1;
            ProRawMaterials.Total = ProRawMaterials.Qnty_value;
            ProRawMaterials.Item_Net_Total_Price = ProRawMaterials.Qnty_value;
            ProRawMaterials.Item_Total_WithoutTax = ProRawMaterials.Qnty_value;
            ProRawMaterials.Larg_Unit_Price = ProRawMaterials.Qnty_value;
            ProRawMaterials.PriceUnit = ProRawMaterials.Qnty_value;
            ProRawMaterials.Unit_Used_for_Price = 1;
            ProRawMaterials.Small_Unit_Price = 0;
            //// حساب الضرايب
            ProRawMaterials.Item_TaxSales10_Value = 0;
            ProRawMaterials.Item_ATS05_Value = 0;
            ProRawMaterials.Item_Net_Total_Price = Common.RoundMePositive(ProRawMaterials.Item_Net_Total_Price.Value);
            ProRawMaterials.Item_CommissionValue = 0;
            ProRawMaterials.Item_CommissionRate = 0;
            ProRawMaterials.Unit_CommissionValue = 0;
            ProRawMaterials.ItemCommissionTotal = 0;
            ProRawMaterials.Net_Total_Item_Commission = 0;
            ProRawMaterials.Is_AddToInvoice = 0;
            ProRawMaterials.Is_SarfFromStore = 0;
            ProRawMaterials.Item_DeletedFlage = -1;
            ProRawMaterials.Item_ProcessedFlage = -1;
            ProRawMaterials.CarsWorkOrdersId = CarsWorkOrdersId;
            ProRawMaterials.Specification = "";
            ProRawMaterials.Item_ATS05 = 0;
            ProRawMaterials.Item_TaxSales10 = 0;

            ProRawMaterials.QntyStr = "1";
            ProRawMaterials.QntyStrColumn = "1";
            ProRawMaterials.Qnty = 1;
            ProRawMaterials.Item_Notes = "";
            ProRawMaterials.Larg_Qnty = 1;
            ProRawMaterials.Small_Qnty = 0;
            ProRawMaterials.Item_No = 0;

            ProRawMaterials.ItemCommissionTotal = 0;
            //ProRawMaterials.Alternate_ItemCommissionTotal = 0;
            ProRawMaterials.Net_Total_Item_Commission = 0;
            ProRawMaterials.ItemCommissionTotal = 0;
            ProRawMaterials.Net_Total_Item_Commission = 0;

            opp.db.CarsWorkOrdersItemProcessedDetails.Add(ProRawMaterials);
            opp.db.SaveChanges();
            return ProRawMaterials;
        }

        public void SetItemProcessedListTotals(List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            CarsWorkOrdersItemProcessedDetail itemT = new CarsWorkOrdersItemProcessedDetail();
            itemT.Id = 0;
            itemT.Item_Net_Total_Price = GetItemProcessedListTotals(ItemProcessedList);
            itemT.Item_Name = "الإجمالى";
            itemT.Invoice_Item_Name = "الإجمالى";

            ItemProcessedList.Add(itemT);
        }
        public decimal GetItemProcessedListTotals(List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {

            decimal Total = 0;

            foreach (var item in ItemProcessedList)
            {
                Total = Total + item.Item_Net_Total_Price ?? 0;
                //Total = Total + item.Item_Net_Total_Price ?? 0;

            }
            return Total;
        }
        public void GetItemProcessedListTotalsByCategory(List<CarsWorkOrdersItemProcessedDetail> ItemProcessedListByCategory)
        {
            CarsWorkOrdersItemProcessedDetail itemTNew = new CarsWorkOrdersItemProcessedDetail();

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
            ItemProcessedListByCategory.Add(itemTNew);


        }


        public void GetComputeTotalsAll(Invoices_List invL, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            InvoiceValuesCLS inv = new InvoiceValuesCLS();


            inv.GetItemProcessedListTotals(invL, ItemProcessedList);
            inv.ComputeTotals(invL, ItemProcessedList);
        }
        public void ComputeTotals(Invoices_List invL, List<CarsWorkOrdersItemProcessedDetail> ItemsP)
        {
            InvoiceValuesCLS inv = new InvoiceValuesCLS();
            inv.ComputeTotals(  invL, ItemsP    );

            return;
            //invL.Item_Net_TotalSpareParts = invL.Item_Net_TotalSpareParts ?? 0;
            //invL.Item_Net_TotalOils = invL.Item_Net_TotalOils ?? 0;
            //invL.Item_Net_TotalSparePartsOils = invL.Item_Net_TotalSparePartsOils ?? 0;
            ////checked nulls
            //invL.Item_Net_TotalWorkmanship = invL.Item_Net_TotalWorkmanship ?? 0;
            //invL.CommissionListWorkmanship_Rate = invL.CommissionListWorkmanship_Rate ?? 0;

            //invL.CommissionListSpareParts_Rate = invL.CommissionListSpareParts_Rate ?? 0;
            //invL.CommissionListOils_Rate = invL.CommissionListOils_Rate ?? 0;
            //////+
            //invL.CommissionListSparePartsOils_Rate = invL.CommissionListSparePartsOils_Rate ?? 0;


            //invL.CommissionListWorkmanship_Value = invL.CommissionListWorkmanship_Value ?? 0;

            //invL.CommissionListSpareParts_Value = invL.CommissionListSpareParts_Value ?? 0;
            //invL.CommissionListOils_Value = invL.CommissionListOils_Value ?? 0;
            //////+
            //invL.CommissionListSparePartsOils_Value = invL.CommissionListSparePartsOils_Value ?? 0;
            ///////Sales totales
            /////From Item Detail
            ////    invL.Qnty_ValueWorkmanship
            ////invL.Qnty_ValueSpareParts
            ////invL.Qnty_ValueOils
            //invL.Total = invL.Qnty_ValueOils + invL.Qnty_ValueSpareParts + invL.Qnty_ValueWorkmanship;
            //invL.Qnty_Value = invL.Total;


            //////Net totals
            /////From Item Detail
            ////invL.Item_Net_TotalWorkmanship
            ////invL.Item_Net_TotalSpareParts
            ////invL.Item_Net_TotalOils
            //invL.Item_Net_Total = invL.Item_Net_TotalOils + invL.Item_Net_TotalSpareParts + invL.Item_Net_TotalWorkmanship;
            //invL.Net_Total = invL.Item_Net_Total;// invL.Item_Net_TotalOils + invL.Item_Net_TotalSpareParts + invL.Item_Net_TotalWorkmanship;


            /////From invoice  Form  
            ////    invL.CommissionListWorkmanship_Rate
            ////invL.CommissionListSpareParts_Rate
            ////invL.CommissionListOils_Rate
            //decimal? CommissionListWorkmanship_ValueOfRate = invL.Item_Net_TotalWorkmanship * invL.CommissionListWorkmanship_Rate / 100;
            //decimal? CommissionListSpareParts_ValueOfRate = invL.Item_Net_TotalSpareParts * invL.CommissionListSpareParts_Rate / 100;
            //decimal? CommissionListOils_ValueOfRate = invL.Item_Net_TotalOils * invL.CommissionListOils_Rate / 100;
            //decimal? CommissionListSparePartsOils_ValueOfRate = CommissionListOils_ValueOfRate + CommissionListSpareParts_ValueOfRate;
            //decimal? Total_CommissionList_ValueOfRate = CommissionListWorkmanship_ValueOfRate + CommissionListSpareParts_ValueOfRate + CommissionListOils_ValueOfRate;
            //invL.List_CommissionRate = invL.CommissionListWorkmanship_Rate + invL.CommissionListSpareParts_Rate + invL.CommissionListOils_Rate;
            /////From invoice  Form  
            ////    invL.CommissionListWorkmanship_Value
            ////invL.CommissionListSpareParts_Value
            ////invL.CommissionListOils_Value
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

            //////////////
            //invL.Net_Total_CommissionValue = invL.Total_CommissionList + Total_CommissionList_ValueOfRate;

            //invL.Total_ListPriceWithoutTax_Workmanship = invL.Qnty_ValueWorkmanship - invL.Total_CommissionListWorkmanship;
            //invL.Total_ListPriceWithoutTax_SpareParts = invL.Qnty_ValueSpareParts - invL.Total_CommissionListSpareParts;
            //invL.Total_ListPriceWithoutTax_Oils = invL.Qnty_ValueOils - invL.Total_CommissionListOils;
            /////////+
            //invL.Total_ListPriceWithoutTax_SparePartsOils = invL.Total_ListPriceWithoutTax_SpareParts + invL.Total_ListPriceWithoutTax_Oils;

            //invL.Total_ListPriceWithoutTax = invL.Total_ListPriceWithoutTax_Oils + invL.Total_ListPriceWithoutTax_SpareParts + invL.Total_ListPriceWithoutTax_Workmanship;

            //invL.Total_PriceWithoutTax = invL.Total_ListPriceWithoutTax;// + invL.Total_ItemPriceWithoutTax;





            /////From invoice  Form  
            ////    invL.ATS05ListWorkmanship_Rate
            ////invL.txtATS05ListSpareParts_Rate
            ////invL.ATS05ListOils_Rate

            //invL.ATS05ListWorkmanship_Value = invL.Total_ListPriceWithoutTax_Workmanship * invL.ATS05ListWorkmanship_Rate / 100;
            //invL.ATS05ListSpareParts_Value = invL.Total_ListPriceWithoutTax_SpareParts * invL.ATS05ListSpareParts_Rate / 100;
            //invL.ATS05ListOils_Value = invL.Total_ListPriceWithoutTax_Oils * invL.ATS05ListOils_Rate / 100;

            //////+
            //invL.ATS05ListSparePartsOils_Value = invL.ATS05ListSpareParts_Value + +invL.ATS05ListOils_Value;

            ////invL.ATS05ListSparePartsOils_Value = invL.ATS05ListOils_Value + invL.ATS05ListSpareParts_Value;


            //////+
            ////invL.ATS05ListSparePartsOils_Value = invL.ATS05ListOils_Value + invL.ATS05ListSpareParts_Value;

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
            //invL.Net_Total = invL.NetInvoiceTotal;

            //invL.Equivalent_Net_Total_Price = invL.Net_Total;
            //invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

            //invL.TafkeetNet_Total = TafkeetProj.NumberToText.GetText(invL.Net_Total ?? 0, invL.Currency_id ?? 1);

            //ComputeTaxForItems(ref invL, ItemsP);
            //SetValuesDecimals(ref invL);
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
        void SetValuesDecimals(ref Invoices_List invL)
        {
            InvoiceValuesCLS inv = new InvoiceValuesCLS();
            inv.SetValuesDecimals(ref invL);

            return;

            //invL.Item_Net_TotalWorkmanship = Common.RoundMePositive(invL.Item_Net_TotalWorkmanship);
            //invL.CommissionListWorkmanship_Rate = Common.RoundMePositive(invL.CommissionListWorkmanship_Rate);
            //invL.Item_Net_TotalSpareParts = Common.RoundMePositive(invL.Item_Net_TotalSpareParts);
            //invL.Item_Net_TotalOils = Common.RoundMePositive(invL.Item_Net_TotalOils);
            //invL.Item_Net_TotalSparePartsOils = Common.RoundMePositive(invL.Item_Net_TotalSparePartsOils);
            //invL.CommissionListSpareParts_Rate = Common.RoundMePositive(invL.CommissionListSpareParts_Rate);
            //invL.CommissionListOils_Rate = Common.RoundMePositive(invL.CommissionListOils_Rate);
            //////+
            //invL.CommissionListSparePartsOils_Rate = Common.RoundMePositive(invL.CommissionListSparePartsOils_Rate);
            //invL.CommissionListWorkmanship_Value = Common.RoundMePositive(invL.CommissionListWorkmanship_Value);
            //invL.CommissionListSpareParts_Value = Common.RoundMePositive(invL.CommissionListSpareParts_Value);
            //invL.CommissionListOils_Value = Common.RoundMePositive(invL.CommissionListOils_Value);
            //invL.CommissionListSparePartsOils_Value = Common.RoundMePositive(invL.CommissionListSparePartsOils_Value);

            //invL.Total = Common.RoundMePositive(invL.Total);


            //invL.Net_Total = Common.RoundMePositive(invL.Net_Total);



            //invL.List_CommissionRate = Common.RoundMePositive(invL.List_CommissionRate);

            //invL.Total_CommissionListWorkmanship = Common.RoundMePositive(invL.Total_CommissionListWorkmanship);
            //invL.Total_CommissionListSpareParts = Common.RoundMePositive(invL.Total_CommissionListSpareParts);
            //invL.Total_CommissionListOils = Common.RoundMePositive(invL.Total_CommissionListOils);
            //invL.Total_CommissionListSparePartsOils = Common.RoundMePositive(invL.Total_CommissionListSparePartsOils);
            //invL.Total_CommissionList = Common.RoundMePositive(invL.Total_CommissionList);

            //invL.List_CommissionValue = Common.RoundMePositive(invL.List_CommissionValue);
            //invL.List_Final_CommissionValue = Common.RoundMePositive(invL.List_Final_CommissionValue);

            //////////////
            //invL.Net_Total_CommissionValue = Common.RoundMePositive(invL.Net_Total_CommissionValue);
            //invL.Total_ListPriceWithoutTax_Workmanship = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_Workmanship);
            //invL.Total_ListPriceWithoutTax_SpareParts = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_SpareParts);
            //invL.Total_ListPriceWithoutTax_Oils = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_Oils);
            //invL.Total_ListPriceWithoutTax_SparePartsOils = Common.RoundMePositive(invL.Total_ListPriceWithoutTax_SparePartsOils);
            //invL.Total_ListPriceWithoutTax = Common.RoundMePositive(invL.Total_ListPriceWithoutTax);
            //invL.Total_PriceWithoutTax = Common.RoundMePositive(invL.Total_PriceWithoutTax);



            //invL.ATS05ListWorkmanship_Value = Common.RoundMePositive(invL.ATS05ListWorkmanship_Value);
            //invL.ATS05ListSpareParts_Value = Common.RoundMePositive(invL.ATS05ListSpareParts_Value);
            //invL.ATS05ListOils_Value = Common.RoundMePositive(invL.ATS05ListOils_Value);
            //////+
            //invL.ATS05ListSparePartsOils_Value = Common.RoundMePositive(invL.ATS05ListSparePartsOils_Value);

            //invL.TotalListATS05_Value = Common.RoundMePositive(invL.TotalListATS05_Value);

            //invL.ATS05_Value = Common.RoundMePositive(invL.ATS05_Value);
            //invL.TaxSales10 = Common.RoundMePositive(invL.TaxSales10);
            //invL.ATS05 = Common.RoundMePositive(invL.ATS05);


            //invL.TaxSalesListWorkmanship_Value = Common.RoundMePositive(invL.TaxSalesListWorkmanship_Value);
            //invL.TaxSalesListSpareParts_Value = Common.RoundMePositive(invL.TaxSalesListSpareParts_Value);
            //invL.TaxSalesListOils_Value = Common.RoundMePositive(invL.TaxSalesListOils_Value);
            //invL.TaxSalesListSparePartsOils_Value = Common.RoundMePositive(invL.TaxSalesListSparePartsOils_Value);
            //invL.TotalListTaxSales10_Value = Common.RoundMePositive(invL.TotalListTaxSales10_Value);
            //invL.TaxSales10_Value = Common.RoundMePositive(invL.TaxSales10_Value);

            //////////
            //invL.NetInvoiceTotal_Workmanship = Common.RoundMePositive(invL.NetInvoiceTotal_Workmanship);
            //invL.NetInvoiceTotal_SpareParts = Common.RoundMePositive(invL.NetInvoiceTotal_SpareParts);
            //invL.NetInvoiceTotal_Oils = Common.RoundMePositive(invL.NetInvoiceTotal_Oils);
            //invL.NetInvoiceTotal_SparePartsOils = Common.RoundMePositive(invL.NetInvoiceTotal_SparePartsOils);
            //invL.NetInvoiceTotal = Common.RoundMePositive(invL.NetInvoiceTotal);

            //invL.Equivalent_Net_Total_Price = Common.RoundMePositive(invL.Equivalent_Net_Total_Price);
            //invL.Equivalent_Net_Total_CommissionValue = Common.RoundMePositive(invL.Equivalent_Net_Total_CommissionValue);


        }
        public void GetItemProcessedListTotals(Invoices_List invL, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            InvoiceValuesCLS inv = new InvoiceValuesCLS();
            inv.GetItemProcessedListTotals(  invL, ItemProcessedList);

            return;
            //invL.Qnty_ValueWorkmanship = 0;
            //invL.Qnty_ValueSpareParts = 0;
            //invL.Qnty_ValueOils = 0;
            //invL.Qnty_ValueSparePartsOils = 0;

            //invL.Item_Net_TotalSpareParts = 0;
            //invL.Item_Net_TotalOils = 0;
            //invL.Item_Net_TotalWorkmanship = 0;
            //invL.Total_ItemPriceWithoutTax = 0;
            //invL.Net_Total = 0;
            //invL.Total_Item_CommissionValue = 0;



            //foreach (var item in ItemProcessedList)
            //{
            //    item.Qnty_value = item.Qnty_value ?? 0;
            //    item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
            //    item.Item_Total_WithoutTax = item.Item_Total_WithoutTax ?? 0;
            //    item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
            //    item.Net_Total_Item_Commission = item.Net_Total_Item_Commission ?? 0;


            //    ///قطع غيار
            //    if (item.Items.Item_Category_ID == 1)
            //    {
            //        invL.Qnty_ValueSpareParts = item.Qnty_value + invL.Qnty_ValueSpareParts;
            //        invL.Item_Net_TotalSpareParts = item.Item_Net_Total_Price + invL.Item_Net_TotalSpareParts;

            //    }
            //    ///زيوت 
            //    if (item.Items.Item_Category_ID == 2)
            //    {
            //        invL.Qnty_ValueOils = item.Qnty_value + invL.Qnty_ValueOils;
            //        invL.Item_Net_TotalOils = item.Item_Net_Total_Price + invL.Item_Net_TotalOils;


            //    }
            //    ///مصنعيات 
            //    if (item.Items.Item_Category_ID == 3)
            //    {
            //        invL.Qnty_ValueWorkmanship = item.Qnty_value + invL.Qnty_ValueWorkmanship;
            //        invL.Item_Net_TotalWorkmanship = item.Item_Net_Total_Price + invL.Item_Net_TotalWorkmanship;

            //    }
            //    //////+
            //    invL.Qnty_ValueSparePartsOils = invL.Qnty_ValueSpareParts + invL.Qnty_ValueOils;


            //    invL.Total_ItemPriceWithoutTax = invL.Total_ItemPriceWithoutTax + item.Item_Total_WithoutTax;
            //    invL.Net_Total = invL.Net_Total + item.Item_Net_Total_Price;
            //    invL.Total_Item_CommissionValue = invL.Total_Item_CommissionValue + item.Net_Total_Item_Commission;
            //}


        }
        public long LookUp_CheckStatusLocationId = 0;
        public long CarsWorkOrdersId = 0;
        public string SearchStr = "";
        public long CustomerId = 0;
        public long CarsId = 0;
        public long WorkOrderNumber = 0;

        public int IsWorkOrderClosed = -1;
        public bool IsEntryDateTimeFrom = false;
        public DateTime EntryDateTimeFrom;
        public bool IsEntryDateTimeTo = false;
        public DateTime EntryDateTimeTo;

        public bool IsExpectedEndWorkDateFrom = false;
        public DateTime ExpectedEndWorkDateFrom;
        public bool IsExpectedEndWorkDateTo = false;
        public DateTime ExpectedEndWorkDateTo;

        public void ResetSearchCriteria()
        {
            LookUp_CheckStatusLocationId = 0;
            CarsWorkOrdersId = 0;
            SearchStr = "";
            CustomerId = 0;
            CarsId = 0;
            WorkOrderNumber = 0;
            IsWorkOrderClosed = -1;
            IsEntryDateTimeFrom = false;
            EntryDateTimeFrom = DateTime.Now.Date;
            IsEntryDateTimeTo = false;
            EntryDateTimeTo = DateTime.Now.Date;


            IsExpectedEndWorkDateFrom = false;
            ExpectedEndWorkDateFrom = DateTime.Now.Date;
            IsExpectedEndWorkDateTo = false;
            ExpectedEndWorkDateTo = DateTime.Now.Date;

        }
        public IQueryable<CarsWorkOrder> QueryableCarsWorkOrder()
        {
            IQueryable<CarsWorkOrder> query = opp.db.CarsWorkOrders.AsNoTracking().Include(i => i.Customer).Include(c => c.Customer_CarOwner).Include(s => s.Store).Include(i => i.Car).Include(i => i.Car.Lookup_CarShape);
            return query;
        }
        public List<CarsWorkOrdersViewModel> getCarsWorkOrder()
        {
            SearchStr = SearchStr.Trim();
            List<CarsWorkOrdersViewModel> carWorkOrdersVM = new List<CarsWorkOrdersViewModel>();

            //IQueryable<CarsWorkOrder> query = opp.db.CarsWorkOrders.AsNoTracking().Include(i => i.Customer).Include(c=>c.Customer_CarOwner).Include(s => s.Store).Include(i => i.Car).Include(i => i.Car.Lookup_CarShape);
            IQueryable<CarsWorkOrder> query = QueryableCarsWorkOrder();
            long WorkOrderNumber = 0;
            if (SearchStr.Trim() != "")
            {
                try
                {
                    WorkOrderNumber = long.Parse(SearchStr);
                    //query = query.Where(i => i.WorkOrderNumber == WorkOrderNumber);
                }
                catch (Exception)
                {
                }
            }

            //if (SearchStr.Trim() != "")
            //{
            //    query = query.Where(i => i.WorkOrderNumber == WorkOrderNumber | i.TaxInternalId.Contains(SearchStr) | i.Customer.Name.Contains(SearchStr) | i.Car.CarNumber.Contains(SearchStr) | i.Car.ChassisNumber.Contains(SearchStr) | i.Car.Model.Contains(SearchStr) | i.Car.CarType.Contains(SearchStr) | i.Customer.Tel.Contains(SearchStr));


            //}
            //if (WorkOrderNumber > 0)
            //{
            //    query = query.Where(i => i.WorkOrderNumber == WorkOrderNumber   );
            //}


            if (this.WorkOrderNumber > 0)
            {
                query = query.Where(i => i.WorkOrderNumber == this.WorkOrderNumber);

            }
            else
            {
                if (WorkOrderNumber == 0)
                {
                    if (SearchStr.Trim() != "")
                    {
                        query = query.Where(i => i.TaxInternalId.Contains(SearchStr) | i.Customer.Name.Contains(SearchStr) | i.Car.CarNumber.Contains(SearchStr) | i.Car.ChassisNumber.Contains(SearchStr) | i.Car.Model.Contains(SearchStr) | i.Car.CarType.Contains(SearchStr) | i.Customer.Tel.Contains(SearchStr));
                    }

                }
                if (WorkOrderNumber > 0)
                {
                    query = query.Where(i => i.WorkOrderNumber == WorkOrderNumber | i.TaxInternalId.Contains(SearchStr) | i.Customer.Name.Contains(SearchStr) | i.Car.CarNumber.Contains(SearchStr) | i.Car.ChassisNumber.Contains(SearchStr) | i.Car.Model.Contains(SearchStr) | i.Car.CarType.Contains(SearchStr) | i.Customer.Tel.Contains(SearchStr));

                }
            }
            ///////////////////
            if (CarsId > 0)
            {
                query = query.Where(i => i.CarsId == CarsId);
            }
            if (CarsWorkOrdersId > 0)
            {
                query = query.Where(i => i.Id == CarsWorkOrdersId);
            }


            if (CustomerId > 0)
            {
                query = query.Where(i => i.CustomerId == CustomerId);
            }
            if (IsWorkOrderClosed > -1)
            {
                query = query.Where(i => i.IsWorkOrderClosed == IsWorkOrderClosed);

                int IsInvoiceTaxInfo = Common.PermissionToShowTaxInvoiceOrNotORAll();
                if (IsInvoiceTaxInfo > -1 & IsWorkOrderClosed == 1)
                {
                    query = query.Where(i => i.IsInvoiceTaxInfo == IsInvoiceTaxInfo);
                }
            }
            if (LookUp_CheckStatusLocationId > 0)
            {
                query = query.Where(i => i.LookUp_CarStatusLocationId == LookUp_CheckStatusLocationId);
            }

            if (IsEntryDateTimeFrom == true)
            {
                DateTime dat = EntryDateTimeFrom.Date;
                query = query.Where(i =>  i.EntryDateTime.Value.Date >= dat);
            }
            if (IsEntryDateTimeTo == true)
            {
                DateTime dat = EntryDateTimeTo.Date;
                query = query.Where(i =>  i.EntryDateTime.Value.Date <= dat);
                //query = query.Where(i => i.EntryDateTime <= EntryDateTimeTo);
            }
            if (IsExpectedEndWorkDateFrom == true)
            {
                DateTime dat = ExpectedEndWorkDateFrom.Date;
                query = query.Where(i =>  i.ExpectedEndWorkDate.Value.Date >= dat);
            }
            if (IsExpectedEndWorkDateTo == true)
            {
                DateTime dat = ExpectedEndWorkDateTo.Date;
                query = query.Where(i =>  i.ExpectedEndWorkDate.Value.Date <= dat);
                //query = query.Where(i => i.EntryDateTime <= EntryDateTimeTo);
            }
            //query = query.Where(item =>item.CarsWorkOrdersItemProcessedDetail.Where(order => order.Is_AddToInvoice == 0)
            //      .Select(order => order.Item_Net_Total_Price).Sum() > -1);
            query = query.OrderBy(i => i.Id);
            carWorkOrdersVM = GetCarsWorkOrdersViewModel(query);
            return carWorkOrdersVM;

        }
        public List<CarsWorkOrdersViewModel> getCarsWorkOrderWithNewItems(long CarsWorkOrdersId, string SearchStr, long CustomerId, int IsWorkOrderClosed, bool IsEntryDateTimeFrom, DateTime EntryDateTimeFrom, bool IsEntryDateTimeTo, DateTime EntryDateTimeTo)
        {
            SearchStr = SearchStr.Trim();
            List<CarsWorkOrdersViewModel> carWorkOrdersVM = new List<CarsWorkOrdersViewModel>();
            IQueryable<CarsWorkOrder> query = opp.db.CarsWorkOrders.Include(i => i.Customer).Include(i => i.Car).Include(i => i.Car.Lookup_CarShape);
            query = query.Include(i => i.carsWorkOrdersDetails);
            if (SearchStr.Trim() != "")
            {
                query = query.Where(i => i.Car.CarNumber.Contains(SearchStr) | i.Car.ChassisNumber.Contains(SearchStr));
            }

            if (CarsWorkOrdersId > 0)
            {
                query = query.Where(i => i.Id == CarsWorkOrdersId);
            }
            if (CustomerId > 0)
            {
                query = query.Where(i => i.CustomerId == CustomerId);
            }
            if (IsWorkOrderClosed > 0)
            {
                query = query.Where(i => i.IsWorkOrderClosed == IsWorkOrderClosed);
            }

            if (IsEntryDateTimeFrom == true)
            {
                DateTime dat = EntryDateTimeFrom.Date;
                query = query.Where(i =>  i.EntryDateTime.Value.Date >= dat);
            }
            if (IsEntryDateTimeTo == true)
            {
                //DbFunctions.TruncateTime(i.EntryDateTime)
                DateTime dat = EntryDateTimeTo.Date;
                query = query.Where(i =>  i.EntryDateTime.Value.Date <= dat);
            }

            carWorkOrdersVM = GetCarsWorkOrdersViewModel(query, 0);
            return carWorkOrdersVM;

        }
        public List<CarsWorkOrdersViewModel> getCarsWorkOrderSearchCategoryItem(long Item_Category_Id, int IsWorkOrderClosed, bool IsEntryDateTimeFrom, DateTime EntryDateTimeFrom, bool IsEntryDateTimeTo, DateTime EntryDateTimeTo)
        {
            SearchStr = SearchStr.Trim();
            List<CarsWorkOrdersViewModel> carWorkOrdersVM = new List<CarsWorkOrdersViewModel>();
            IQueryable<CarsWorkOrder> query = opp.db.CarsWorkOrders.AsNoTracking().Include(i => i.Customer).Include(s => s.Store).Include(i => i.Car).Include(i => i.CarsWorkOrdersItemProcessedDetail);
            long WorkOrderNumber = 0;
            if (SearchStr.Trim() != "")
            {
                try
                {
                    WorkOrderNumber = long.Parse(SearchStr);
                    //query = query.Where(i => i.WorkOrderNumber == WorkOrderNumber);
                }
                catch (Exception)
                {
                }
            }

            //if (SearchStr.Trim() != "")
            //{
            //    query = query.Where(i => i.WorkOrderNumber == WorkOrderNumber | i.TaxInternalId.Contains(SearchStr) | i.Customer.Name.Contains(SearchStr) | i.Car.CarNumber.Contains(SearchStr) | i.Car.ChassisNumber.Contains(SearchStr) | i.Car.Model.Contains(SearchStr) | i.Car.CarType.Contains(SearchStr) | i.Customer.Tel.Contains(SearchStr));


            //}
            //if (WorkOrderNumber > 0)
            //{
            //    query = query.Where(i => i.WorkOrderNumber == WorkOrderNumber   );
            //}

            if (WorkOrderNumber == 0)
            {
                query = query.Where(i => i.TaxInternalId.Contains(SearchStr) | i.Customer.Name.Contains(SearchStr) | i.Car.CarNumber.Contains(SearchStr) | i.Car.ChassisNumber.Contains(SearchStr) | i.Car.Model.Contains(SearchStr) | i.Car.CarType.Contains(SearchStr) | i.Customer.Tel.Contains(SearchStr));


            }
            if (WorkOrderNumber > 0)
            {
                query = query.Where(i => i.WorkOrderNumber == WorkOrderNumber | i.TaxInternalId.Contains(SearchStr) | i.Customer.Name.Contains(SearchStr) | i.Car.CarNumber.Contains(SearchStr) | i.Car.ChassisNumber.Contains(SearchStr) | i.Car.Model.Contains(SearchStr) | i.Car.CarType.Contains(SearchStr) | i.Customer.Tel.Contains(SearchStr));

            }
            ///////////////////
            if (CarsId > 0)
            {
                query = query.Where(i => i.CarsId == CarsId);
            }
            if (CarsWorkOrdersId > 0)
            {
                query = query.Where(i => i.Id == CarsWorkOrdersId);
            }


            if (CustomerId > 0)
            {
                query = query.Where(i => i.CustomerId == CustomerId);
            }
            if (IsWorkOrderClosed > -1)
            {
                query = query.Where(i => i.IsWorkOrderClosed == IsWorkOrderClosed);
            }
            if (LookUp_CheckStatusLocationId > 0)
            {
                query = query.Where(i => i.LookUp_CarStatusLocationId == LookUp_CheckStatusLocationId);
            }

            if (IsEntryDateTimeFrom == true)
            {
                DateTime dat = EntryDateTimeFrom.Date;
                query = query.Where(i =>  i.EntryDateTime.Value.Date >= dat);
            }
            if (IsEntryDateTimeTo == true)
            {
                DateTime dat = EntryDateTimeTo.Date;
                query = query.Where(i =>  i.EntryDateTime.Value.Date <= dat);
                //query = query.Where(i => i.EntryDateTime <= EntryDateTimeTo);
            }

            //query = query.Where(item =>item.CarsWorkOrdersItemProcessedDetail.Where(order => order.Is_AddToInvoice == 0)
            //      .Select(order => order.Item_Net_Total_Price).Sum() > -1);
            query = query.OrderBy(i => i.Id);
            carWorkOrdersVM = GetCarsWorkOrdersViewModel(query);
            return carWorkOrdersVM;

        }
        List<CarsWorkOrdersViewModel> GetCarsWorkOrdersViewModel(IQueryable<CarsWorkOrder> query, int Is_SarfFromStore = -1)
        {
            string sql = query.ToQueryString();
            
            try
            {
                List<CarsWorkOrder> CarsWorkOrderList = query.ToList();
                return GetCarsWorkOrdersViewModel(CarsWorkOrderList, Is_SarfFromStore);
            }
            catch (System.Data.SqlTypes.SqlNullValueException ee)
            {

                throw;
            }
            
            
        }
        List<CarsWorkOrdersViewModel> GetCarsWorkOrdersViewModel(List<CarsWorkOrder> query, int Is_SarfFromStore = -1)
        {
            List<CarsWorkOrdersViewModel> carWorkOrdersVM = new List<CarsWorkOrdersViewModel>();

            int Serial_ = 1;
            foreach (var item in query)
            {
                if (Is_SarfFromStore > -1)
                {
                    Is_SarfFromStore = 1;
                    List<CarsWorkOrdersRequestedDetail> carsWorkOrdersDetails__ = item.carsWorkOrdersDetails.Where(i => i.Is_SarfFromStore == 0).ToList();
                    if (carsWorkOrdersDetails__.Count == 0)
                    {
                        continue;
                    }


                }

                //int Serial_ = 1;
                CarsWorkOrdersViewModel cv = new CarsWorkOrdersViewModel();


                cv = GetCarsWorkOrdersViewModel(item);
                cv.Serial_ = Serial_++;
                //cv.Id = item.Id;
                //cv.WorkOrderNumber = item.WorkOrderNumber;
                //cv.WarshaStoreId = item.StoreId;
                //if (item.Store != null)
                //{
                //    cv.WarshaStoreName = item.Store.Store_Name;
                //}
                //cv.CarsId = item.CarsId;
                //cv.CustomerId = item.CustomerId;
                //cv.ChassisNumber = item.Car.ChassisNumber;
                //cv.CarNumber = item.Car.CarNumber;
                //cv.CarType = item.Car.CarType;
                //cv.Model = item.Car.Model;
                //cv.EngineNumber = item.Car.EngineNumber;
                //cv.FuelType = item.Car.FuelType;
                //cv.Capacity = item.Car.Capacity;
                //cv.Year = item.Car.Year;
                //cv.TransmissionType = item.Car.TransmissionType;
                //cv.ColorCode = item.Car.ColorCode;
                //cv.CarColor = item.Car.CarColor;
                //try
                //{
                //    if (item.Car.Lookup_CarShape != null)
                //    {
                //        cv.CarShapeName = item.Car.Lookup_CarShape.Name;
                //    }
                //}
                //catch (Exception ee)
                //{

                //}
                //cv.Address = item.Address;

                //if (item.Customer != null)
                //{
                //    cv.CustomerName = item.Customer.Name;
                //    cv.CustomerSalesMan = item.Customer.CompanyContactName;
                //    cv.CustomerSubAccount_Id = item.Customer.SubAccount_Id;
                //    cv.Customer_SubAccount_Name = item.Customer.Name;
                //}


                //cv.Tel = item.Tel;
                //cv.Mobile = item.Mobile;

                //cv.CarMeterOnEntry = item.CarMeterOnEntry;
                //cv.WorkOrdersDate = item.WorkOrdersDate;

                //cv.EntryDateTime = item.EntryDateTime;
                //cv.StartWorkDateTime = item.StartWorkDateTime;
                //cv.ExpectedEndWorkDate = item.ExpectedEndWorkDate;
                //cv.CustomerComplaint = item.CustomerComplaint;
                //cv.RequiredWorks = item.RequiredWorks;
                //cv.ReceptionEngineerId = item.ReceptionEngineerId;
                //cv.ReceptionEngineerName = item.ReceptionEngineerName;


                //cv.ReceptionManId = item.ReceptionManId;
                //cv.ReceptionManName = item.ReceptionManName;

                //cv.WorkOrderClosedDate = item.WorkOrderClosedDate;
                //cv.IsWorkOrderClosed = item.IsWorkOrderClosed;
                //cv.TaxInternalId = item.TaxInternalId;
                //cv.TaxInvoiceNo = item.TaxInvoiceNo;
                //cv.IsUploadToPortal = item.IsUploadToPortal;

                carWorkOrdersVM.Add(cv);
            }
            return carWorkOrdersVM;
        }
        public CarsWorkOrdersViewModel GetCarsWorkOrdersViewModel(CarsWorkOrder item)
        {
            CarsWorkOrdersViewModel cv = new CarsWorkOrdersViewModel();

            //int Serial_ = 1;


            cv.Id = item.Id;
            cv.WorkOrderNumber = item.WorkOrderNumber;
            cv.WarshaStoreId = item.StoreId;
            if (item.Store != null)
            {
                cv.WarshaStoreName = item.Store.Store_Name;
            }
            cv.CarsId = item.CarsId;
            cv.CustomerId = item.CustomerId;
            cv.Customer_Id_RelatedToAccount = item.Customer_Id_RelatedToAccount;

            cv.ChassisNumber = item.Car.ChassisNumber;
            cv.CarNumber = item.Car.CarNumber;
            cv.CarType = item.Car.CarType;
            cv.Model = item.Car.Model;
            cv.EngineNumber = item.Car.EngineNumber;
            cv.FuelType = item.Car.FuelType;
            cv.Capacity = item.Car.Capacity;
            cv.Year = item.Car.Year;
            cv.TransmissionType = item.Car.TransmissionType;
            cv.ColorCode = item.Car.ColorCode;
            cv.CarColor = item.Car.CarColor;
            try
            {
                if (item.Car.Lookup_CarShape != null)
                {
                    cv.CarShapeName = item.Car.Lookup_CarShape.Name;
                }
            }
            catch (Exception ee)
            {

            }
            cv.Address = item.Address;

            if (item.Customer != null)
            {
                cv.CustomerName = item.Customer.Name;
                cv.CustomerSalesMan = item.Customer.CompanyContactName;
                cv.CustomerSubAccount_Id = item.Customer.SubAccount_Id;
                cv.Customer_SubAccount_Name = item.Customer.Name;
            }
            if (item.Customer_CarOwner != null)
            {
                cv.CustomerName_CarOwner = item.Customer_CarOwner.Name;


            }
            if (item.Customer_RelatedToAccount != null)
            {
                cv.CustomerName_RelatedToAccount = item.Customer_RelatedToAccount.Name;


            }

            cv.CustomerId_CarOwner = item.CustomerId_CarOwner;

            cv.Tel = item.Tel;
            cv.Mobile = item.Mobile;

            cv.CarMeterOnEntry = item.CarMeterOnEntry;
            cv.WorkOrdersDate = item.WorkOrdersDate;

            cv.EntryDateTime = item.EntryDateTime;
            cv.StartWorkDateTime = item.StartWorkDateTime;
            cv.ExpectedEndWorkDate = item.ExpectedEndWorkDate;
            cv.CustomerComplaint = item.CustomerComplaint;
            cv.RequiredWorks = item.RequiredWorks;
            cv.ReceptionEngineerId = item.ReceptionEngineerId;
            cv.ReceptionEngineerName = item.ReceptionEngineerName;


            cv.ReceptionManId = item.ReceptionManId;
            cv.ReceptionManName = item.ReceptionManName;


            cv.TaxInternalId = item.TaxInternalId;
            cv.TaxInvoiceNo = item.TaxInvoiceNo;
            cv.IsUploadToPortal = item.IsUploadToPortal;
            cv.KhamatQnty_Value = item.KhamatQnty_Value ?? 0;
            cv.CustomerOrderNumberPO = item.CustomerOrderNumberPO;
            cv.CustomerOrderNumberGR = item.CustomerOrderNumberGR;
            cv.WorkOrderClosed_Reason = item.WorkOrderClosed_Reason;
            cv.WorkOrderClosedDate = item.WorkOrderClosedDate;
            cv.IsWorkOrderClosed = item.IsWorkOrderClosed;
            cv.Repairs_NotDone = item.Repairs_NotDone;
            cv.IsInsuranceCompanyStaus = item.IsInsuranceCompanyStaus;

            GetCarsWorkOrdersStatus(ref cv);

            return cv;
        }
        public void GetCarsWorkOrdersStatus(ref CarsWorkOrdersViewModel cv)
        {
            if (cv.IsWorkOrderClosed == 0)
            {
                cv.Status = "مفتوح";

            }
            else if (cv.IsWorkOrderClosed == 1)
            {
                cv.Status = "مغلق";
            }
            else if (cv.IsWorkOrderClosed == 2)
            {
                cv.Status = "ملغي";
            }
        }
        public int SetAllItemFromDeliveredToProcessedList(bool IsRebeatItem, List<CarsWorkOrdersDeliverDetail> DeliveredItemsList)
        {
            CarsWorkOrdersItemProcessedDetailCLS ProcessedCls = new CarsWorkOrdersItemProcessedDetailCLS();
            List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList_New = new List<CarsWorkOrdersItemProcessedDetail>();
            MappingCLS mp = new MappingCLS();
            foreach (var item in DeliveredItemsList)
            {
                bool IsFound = false;
                long carsWorkOrdersId = item.CarsWorkOrdersId ?? 0;
                long Item_Id = item.Item_Id;
                //فى حالة تكرار الصنف
                if (IsRebeatItem == true)
                {
                    IsFound = false;
                }
                else
                {
                    ///التأكد من وجود الصنف
                    IsFound = ProcessedCls.ISItemProcessedFound(ref opp, carsWorkOrdersId, Item_Id);
                }

                if (IsFound == false)
                {
                    CarsWorkOrdersItemProcessedDetail ItemProcessed = new CarsWorkOrdersItemProcessedDetail();

                    ItemProcessed = mp.MappingGetItemProcessed_From_Deliverd((CarsWorkOrdersDeliverDetail)item);
                    ItemProcessedList_New.Add(ItemProcessed);
                    int Count = ProcessedCls.Add(ref opp, ItemProcessed);
                }

            }

            //int Count = ProcessedCls.Add(ref opp, ItemProcessedList_New);
            return 0;
        }

        public int SetAllItemFromProcessedToReversedList(bool IsRebeatItem, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            CarsWorkOrdersItemReversedProcessedDetailCLS ProcessedCls = new CarsWorkOrdersItemReversedProcessedDetailCLS();
            List<CarsWorkOrdersItemReversedProcessedDetail> ItemProcessedList_New = new List<CarsWorkOrdersItemReversedProcessedDetail>();
            MappingCLS mp = new MappingCLS();
            foreach (var item in ItemProcessedList)
            {
                bool IsFound = false;
                long carsWorkOrdersId = item.CarsWorkOrdersId ?? 0;
                long Item_Id = item.Item_Id;
                //فى حالة تكرار الصنف
                if (IsRebeatItem == true)
                {
                    IsFound = false;
                }
                else
                {
                    ///التأكد من وجود الصنف
                    IsFound = ProcessedCls.ISItemProcessedFound(ref opp, carsWorkOrdersId, Item_Id);
                }

                if (IsFound == false)
                {
                    CarsWorkOrdersItemReversedProcessedDetail ItemProcessed = new CarsWorkOrdersItemReversedProcessedDetail();

                    ItemProcessed = mp.MappingGetItemReversed_From_Processed((CarsWorkOrdersItemProcessedDetail)item);
                    ItemProcessedList_New.Add(ItemProcessed);
                    int Count = ProcessedCls.AddReversed(ref opp, ItemProcessed);
                }

            }

            //int Count = ProcessedCls.Add(ref opp, ItemProcessedList_New);
            return 0;
        }

        public int SetAllItemFromDeliveredToProcessed(bool IsRebeatItem, CarsWorkOrdersDeliverDetail item)
        {
            CarsWorkOrdersItemProcessedDetailCLS ProcessedCls = new CarsWorkOrdersItemProcessedDetailCLS();
            List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList_New = new List<CarsWorkOrdersItemProcessedDetail>();
            MappingCLS mp = new MappingCLS();

            bool IsFound = false;
            long carsWorkOrdersId = item.CarsWorkOrdersId ?? 0;
            long Item_Id = item.Id;
            //فى حالة تكرار الصنف
            if (IsRebeatItem == true)
            {
                IsFound = false;
            }
            else
            {
                ///التأكد من وجود الصنف
                IsFound = ProcessedCls.ISItemProcessedFound(ref opp, carsWorkOrdersId, Item_Id);
            }

            if (IsFound == false)
            {
                CarsWorkOrdersItemProcessedDetail ItemProcessed = new CarsWorkOrdersItemProcessedDetail();

                ItemProcessed = mp.MappingGetItemProcessed_From_Deliverd((CarsWorkOrdersDeliverDetail)item);
                ItemProcessedList_New.Add(ItemProcessed);
            }


            int Count = ProcessedCls.Add(ref opp, ItemProcessedList_New);
            return Count;

        }

        public int SaveEditableItemProcessed(ref int excp, ref string msg, List<CarsWorkOrdersItemProcessedDetail> itemProcessedList)
        {
            int Count = 0;
            try
            {
                CarsWorkOrdersItemProcessedDetailCLS ProcessedCls = new CarsWorkOrdersItemProcessedDetailCLS();

                Count = ProcessedCls.SaveEditableItemProcessed(ref excp, ref msg, ref opp, itemProcessedList);
                excp = 0;
                return Count;
            }
            catch (Exception ee)
            {
                excp = 1;
                msg = ee.Message;
                return Count;

            }
        }
        public int SaveEditableItemReversedProcessed(ref int excp, ref string msg, List<CarsWorkOrdersItemReversedProcessedDetail> itemReversedProcessedList)
        {
            int Count = 0;
            try
            {
                CarsWorkOrdersItemReversedProcessedDetailCLS ProcessedCls = new CarsWorkOrdersItemReversedProcessedDetailCLS();

                Count = ProcessedCls.SaveEditableItemProcessed(ref excp, ref msg, ref opp, itemReversedProcessedList);
                excp = 0;
                return Count;
            }
            catch (Exception ee)
            {
                excp = 1;
                msg = ee.Message;
                return Count;

            }
        }
        public void SaveProcessedItemsPrice(ref int excp, ref string msg, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            //صلاحية تعديل اسعار الأصناف المضافة الى أمر الشغل
            bool IsEdit_CarsWorkOrdersItemProcessedDetail = Common.IsHavePermissionSysMenu(Common.User_Id, 924, 2, -1);
            if (IsEdit_CarsWorkOrdersItemProcessedDetail == false)
            {
                excp = 924;
                msg = "ليس لديك صلاحية تعديل اسعار الأصناف المضافة الى أمر الشغل";
                return;
            }
            if (ValidateToAddEditItem(ref msg) == false)
            {
                excp = 924;
                //msg = "لا يمكن تعديل الأسعار بعد غلق أمر الشغل";
                return;
            }
            ValidateEditableItemProcessed(ref excp, ref msg, ItemProcessedList);
            if (excp == 0)
            {
                int Count = SaveEditableItemProcessed(ref excp, ref msg, ItemProcessedList);
            }

        }
        public void ValidateEditableItemProcessed(ref int excp, ref string msg, List<CarsWorkOrdersItemProcessedDetail> itemProcessedList)
        {


            try
            {
                foreach (var item in itemProcessedList)
                {
                    item.Qnty = item.Qnty ?? 0;
                    item.Larg_Qnty = item.Larg_Qnty ?? 0;
                    item.Small_Qnty = item.Small_Qnty ?? 0;
                    item.Item_No = item.Item_No ?? 0;
                    item.PriceUnit = item.PriceUnit ?? 0;
                    item.Larg_Unit_Price = item.Larg_Unit_Price ?? 0;
                    item.Small_Unit_Price = item.Small_Unit_Price ?? 0;
                    item.Unit_Used_for_Price = item.Unit_Used_for_Price ?? 1;
                    item.S_price_LargeUnit = item.S_price_LargeUnit ?? 0;
                    item.S_price_SmallUnit = item.S_price_SmallUnit ?? 0;
                    item.Qnty_value = item.Qnty_value ?? 0;
                    item.Item_Total_WithoutTax = item.Item_Total_WithoutTax ?? 0;
                    item.Item_ATS05 = item.Item_ATS05 ?? 0;
                    item.Item_ATS05_Value = item.Item_ATS05_Value ?? 0;
                    item.Item_TaxSales10 = item.Item_TaxSales10 ?? 0;
                    item.Item_TaxSales10_Value = item.Item_TaxSales10_Value ?? 0;
                    item.Total = item.Total ?? 0;
                    item.Is_Item_CommissionValue = item.Is_Item_CommissionValue ?? 0;
                    item.Item_CommissionValue = item.Item_CommissionValue ?? 0;
                    item.Item_CommissionRate = item.Item_CommissionRate ?? 0;
                    item.Unit_CommissionRate = item.Unit_CommissionRate ?? 0;
                    item.Net_Total_Item_Commission = item.Net_Total_Item_Commission ?? 0;
                    item.ItemCommissionTotal = item.ItemCommissionTotal ?? 0;
                    item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;


                    item.Order_PurshingId = item.Order_PurshingId ?? 0;
                    item.Detailes_PurshingId = item.Detailes_PurshingId;

                    item.Order_Purshing_No = item.Order_Purshing_No;
                    item.Order_Purshing_Date = item.Order_Purshing_Date;




                    item.StoreId = item.StoreId ?? 0;
                    item.Is_SarfFromStore = item.Is_SarfFromStore ?? 0;
                    item.Is_AddToInvoice = item.Is_AddToInvoice ?? 0;
                    item.Item_DeletedFlage = item.Item_DeletedFlage ?? 0;
                    item.Item_ProcessedFlage = item.Item_ProcessedFlage ?? 0;


                }
                excp = 0;
            }
            catch (Exception ee)
            {
                excp = 1;
                msg = ee.Message;

            }
        }


        public void BuildInvoice(ref int Excp, ref string Msg, Invoices_List invL, CarsWorkOrdersCLS carsWorkOrderCls, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {

            try
            {


                Requests_ModelCls rc = new Requests_ModelCls();
                Excp = 0;
                Msg = "";
                rc.Make_Invoice_ForCars(ref Excp, ref Msg, ref opp, carsWorkOrderCls, invL);


            }
            catch (Exception ee)
            {
            }

        }
        public void EditInvoice(ref int Excp, ref string Msg, CarsWorkOrdersCLS carsWorkOrderCls, Invoices_List invL, List<Invoices_Details> invDL, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {

            try
            {
                //List<Invoices_Details> indList = GetInvoices_DetailsList(ItemProcessedList);

                Requests_ModelCls rc = new Requests_ModelCls();
                Excp = 0;
                Msg = "";
                rc.Edit_Invoice_ForCars(ref Excp, ref Msg, ref opp, carsWorkOrderCls, invL, invDL);
            }
            catch (Exception ee)
            {
            }

        }
        public void DeleteInvoice(ref int Excp, ref string Msg, long Invoices_List_Id)
        {

            try
            {
                //List<Invoices_Details> indList = GetInvoices_DetailsList(ItemProcessedList);

                Requests_ModelCls rc = new Requests_ModelCls();
                Excp = 0;
                Msg = "";
                rc.Delete_Invoice_ForCars(ref Excp, ref Msg, ref opp, Invoices_List_Id);
            }
            catch (Exception ee)
            {
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
            }

        }

        public List<Invoices_Details> GetInvoices_DetailsList(List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            Invoices_Details ind = new Invoices_Details();
            List<Invoices_Details> indList = new List<Invoices_Details>();
            foreach (var item in ItemProcessedList)
            {
                ind = new Invoices_Details();
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
                ind.Purchase_Price = item.P_Price;
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
        public void GetCarInvoice(ref int Excp, ref string Msg, long Invoice_List_Table_Id, ref Invoices_List invL, ref List<Invoices_Details> invD_S, ref List<Invoices_Details> invD_O, ref List<Invoices_Details> invD_M)
        {
            Invoices_ListCls invCls = new Invoices_ListCls();
            invL = invCls.GetInvoices_ListById(ref opp, Invoice_List_Table_Id);

            long Item_Category_ID = (long)ItemCategoryEnum.SpareParts;
            invD_S = new List<Invoices_Details>();
            invD_S = invCls.GetInvoices_Details(ref opp, Invoice_List_Table_Id, Item_Category_ID);

            Item_Category_ID = (long)ItemCategoryEnum.Oils;
            invD_O = new List<Invoices_Details>();
            invD_O = invCls.GetInvoices_Details(ref opp, Invoice_List_Table_Id, Item_Category_ID);

            Item_Category_ID = (long)ItemCategoryEnum.Masnaheaiat;
            invD_M = new List<Invoices_Details>();
            invD_M = invCls.GetInvoices_Details(ref opp, Invoice_List_Table_Id, Item_Category_ID);

        }
        public List<CarsWorkOrdersViewModel> GetCarInvoice(ref int Excp, ref string Msg, ref long CarsWorkOrdersId, ref long Invoice_List_Table_Id, ref Invoices_List invL, ref List<Invoices_Details> invD, int Is_AddToInvoice)
        {
            Invoices_ListCls invCls = new Invoices_ListCls();
            //invL = new Invoices_List();
            //invD = new List<Invoices_Details>();

            if (Invoice_List_Table_Id > 0)
            {
                invL = invCls.GetInvoices_ListById(ref opp, Invoice_List_Table_Id);
                if (invL != null)
                {
                    CarsWorkOrdersId = invL.CarsWorkOrdersId ?? 0;
                    invD = invCls.GetInvoices_Details(ref opp, Invoice_List_Table_Id);
                    Is_AddToInvoice = 1;
                }
                else
                {
                    invL = new Invoices_List();
                    invD = new List<Invoices_Details>();
                }
            }


            List<CarsWorkOrdersViewModel> carVM = new List<CarsWorkOrdersViewModel>();
            try
            {

                carVM = getItemsProcessedForInvoice(CarsWorkOrdersId, Is_AddToInvoice);


            }
            catch (Exception ee)
            {
            }
            return carVM;
        }

        public void GetCheckIfFoundInvoiceCreatedBeforOrNot(ref int excp, ref string msg, ref long carsWorkOrdersId, ref Invoices_List invl)
        {
            invl = new Invoices_List();


            Invoices_ListCls invCls = new Invoices_ListCls();
            List<Invoices_List> invList = invCls.GetInvoices_ListByCarsWorkOrdersIdAsNoTracking(ref opp, carsWorkOrdersId);
            if (invList != null)
            {
                if (invList.Count > 0)
                {
                    invl = invList[0];

                }
                else
                {
                    invl = null;
                }
            }
            else
            {
                invl = null;
            }


        }


        public void SaveRepairs_NotDone(ref int Excp, ref string Msg, long carsWorkOrdersId, string Repairs_NotDone)
        {
            try
            {
                CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == carsWorkOrdersId).FirstOrDefault();
                if (carsWorkOrder != null)
                {
                    // اجمالى الخامات التى تم اضافتها   

                    // SetComputeRawMaterialsTotalsOld(ref opp, ItemProcessedDetailCLS, CarsWorkOrdersId_, Is_AddToInvoice);
                    CarsWorkOrdersItemProcessedDetail Khamat = SetComputeRawMaterialsTotals(carsWorkOrdersId, 0);

                    carsWorkOrder.KhamatQnty_Value = Khamat.Qnty_value ?? 0;


                    //if (Khamat.Qnty_value > 0)
                    //{
                    //    carsWorkOrder.KhamatQnty_Value = Khamat.Qnty_value;
                    //}


                    carsWorkOrder.Repairs_NotDone = Repairs_NotDone;
                    opp.db.Entry(carsWorkOrder).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
                else
                {
                    Excp = 10;
                    Msg = "خطأ فى ايجاد أمر الشغل";
                }
                Excp = 0;
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void SendToAccount(ref int Excp, ref string Msg, long carsWorkOrdersId, string Repairs_NotDone)
        {
            try
            {
                CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == carsWorkOrdersId).FirstOrDefault();
                if (carsWorkOrder != null)
                {
                    // اجمالى الخامات التى تم اضافتها   

                    // SetComputeRawMaterialsTotalsOld(ref opp, ItemProcessedDetailCLS, CarsWorkOrdersId_, Is_AddToInvoice);
                    CarsWorkOrdersItemProcessedDetail Khamat = SetComputeRawMaterialsTotals(carsWorkOrdersId, 0);

                    carsWorkOrder.KhamatQnty_Value = Khamat.Qnty_value ?? 0;


                    //if (Khamat.Qnty_value > 0)
                    //{
                    //    carsWorkOrder.KhamatQnty_Value = Khamat.Qnty_value;
                    //}

                    /////////تسكين
                    carsWorkOrder.LookUp_CarStatusLocationId = (long)CarStatusLocationEnum.InWaitingAcconting;
                    carsWorkOrder.Repairs_NotDone = Repairs_NotDone;
                    opp.db.Entry(carsWorkOrder).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
                else
                {
                    Excp = 10;
                    Msg = "خطأ فى ايجاد أمر الشغل";
                }
                Excp = 0;
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
            }
        }

        public CarsWorkOrder SetWorkOrderOpenAndSendToWarsha(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long carsWorkOrdersId, string Repairs_NotDone = null)
        {
            

            CarsWorkOrder carsWorkOrder = new CarsWorkOrder();
            try
            {
                carsWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == carsWorkOrdersId).FirstOrDefault();
                if (carsWorkOrder != null)
                {
                    if (carsWorkOrder.IsUploadToPortal == 1)
                    {
                        //Excp = 1;
                        Msg = "تم رفع الفاتورة الى منظومة الضرايب";
                    }

                    carsWorkOrder.IsWorkOrderClosed = 0;
                    carsWorkOrder.IsInvoiceTaxInfo = 0;
                    carsWorkOrder.LookUp_CarStatusLocationId = (long)CarStatusLocationEnum.InWarsha;

                    if (Repairs_NotDone != null)
                    {
                        carsWorkOrder.Repairs_NotDone = Repairs_NotDone;
                    }

                    opp.db.Entry(carsWorkOrder).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
                else
                {
                    Excp = 10;
                    Msg = "خطأ فى ايجاد أمر الشغل";
                }
                Excp = 0;
                return carsWorkOrder;
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
                return carsWorkOrder;
            }
        }
        public CarsWorkOrder SetWorkOrderOpenAndSendToWarsha1(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long carsWorkOrdersId)
        {
            //
            CarsWorkOrder carsWorkOrder = new CarsWorkOrder();
            try
            {
                carsWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == carsWorkOrdersId).FirstOrDefault();
                if (carsWorkOrder != null)
                {
                    if (carsWorkOrder.IsUploadToPortal == 1)
                    {
                        //Excp = 1;
                        Msg = "تم رفع الفاتورة الى منظومة الضرايب";
                    }

                    carsWorkOrder.IsWorkOrderClosed = 0;
                    carsWorkOrder.IsInvoiceTaxInfo = 0;
                    carsWorkOrder.LookUp_CarStatusLocationId = (long)CarStatusLocationEnum.InWarsha;

                    opp.db.Entry(carsWorkOrder).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
                else
                {
                    Excp = 10;
                    Msg = "خطأ فى ايجاد أمر الشغل";
                }
                Excp = 0;
                return carsWorkOrder;
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
                return carsWorkOrder;
            }

        }
        public void SendToWarsha(ref int Excp, ref string Msg, long carsWorkOrdersId)
        {
            SetWorkOrderOpenAndSendToWarsha(ref Excp, ref Msg, ref opp, carsWorkOrdersId);
        }
        public void SendToReceiption(ref int Excp, ref string Msg, long carsWorkOrdersId)
        {
            SetWorkOrderOpenAndSendToReceiption(ref Excp, ref Msg, carsWorkOrdersId);
        }
        public void SetWorkOrderOpenAndSendToReceiption(ref int Excp, ref string Msg, long carsWorkOrdersId)
        {
            try
            {
                CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == carsWorkOrdersId).FirstOrDefault();
                if (carsWorkOrder != null)
                {
                    // اجمالى الخامات التى تم اضافتها   

                    // SetComputeRawMaterialsTotalsOld(ref opp, ItemProcessedDetailCLS, CarsWorkOrdersId_, Is_AddToInvoice);
                    CarsWorkOrdersItemProcessedDetail Khamat = SetComputeRawMaterialsTotals(carsWorkOrdersId, 0);

                    carsWorkOrder.KhamatQnty_Value = Khamat.Qnty_value ?? 0;


                    //if (Khamat.Qnty_value > 0)
                    //{
                    //    carsWorkOrder.KhamatQnty_Value = Khamat.Qnty_value;
                    //}

                    /////////تسكين
                    carsWorkOrder.LookUp_CarStatusLocationId = (long)CarStatusLocationEnum.InReception;
                    opp.db.Entry(carsWorkOrder).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
                else
                {
                    Excp = 10;
                    Msg = "خطأ فى ايجاد أمر الشغل";
                }
                Excp = 0;
            }
            catch (Exception ee)
            {

                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void SetCarStatusLocationTo(ref int Excp, ref string Msg, long CarsWordOrderId, long CarStatusLocationId)
        {
            try
            {
                CarsWorkOrder carsWorkOrder = opp.db.CarsWorkOrders.Where(c => c.Id == CarsWordOrderId).FirstOrDefault();
                if (carsWorkOrder != null)
                {
                    carsWorkOrder.LookUp_CarStatusLocationId = CarStatusLocationId;
                    CarStatusLocationEnum carStatusLocationEnum = (CarStatusLocationEnum)CarStatusLocationId;
                    carsWorkOrder.LookUp_CarStatusLocationName = carStatusLocationEnum.ToString();
                    opp.db.SaveChanges();
                }
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = "";
            }
        }
        public List<CarsWorkOrdersViewModel> getCarsWorkOrderForReversedDetailItems(long CarsWorkOrdersId_, int Is_AddToInvoice)
        {



            ///////get request item for work order
            CarsWorkOrdersDetailCLS carsWorkOrdersDetailCLS = new CarsWorkOrdersDetailCLS();
            //ItemRequestList = new List<CarsWorkOrdersRequestedDetail>();
            //ItemRequestList = carsWorkOrdersDetailCLS.GetCarsWorkOrdersRequestedDetails(ref opp, CarsWorkOrdersId_);

            ///////get item for Item Delivered
            //CarsWorkOrdersDeliverDetailCLS ItemDeliver = new CarsWorkOrdersDeliverDetailCLS();
            //ItemDeliveredList = new List<CarsWorkOrdersDeliverDetail>();
            //ItemDeliveredList = ItemDeliver.SetCarsWorkOrdersItemDeliverDetail(ref opp, CarsWorkOrdersId_);



            ///////get item for ItemProcessedDetail
            ///
            GetItemProcessedDetail(ref opp, CarsWorkOrdersId_, Is_AddToInvoice);
            int Is_AddToInvoice_ = 0;
            try
            {
                GetItemReversedProcessedDetail(ref opp, CarsWorkOrdersId_, Is_AddToInvoice_);
            }
            catch (Exception ee)
            {
            }


            ///get work order data by CarsWorkOrdersId
            ResetSearchCriteria();
            CarsWorkOrdersId = CarsWorkOrdersId_;
            List<CarsWorkOrdersViewModel> CarsWorkOrdersVMList = getCarsWorkOrder();
            if (CarsWorkOrdersVMList != null)
            {
                CarsWorkOrdersViewModel = CarsWorkOrdersVMList.FirstOrDefault();
            }


            ////////////Get Invoices List for workorder
            ///
            //invsList = GetInvoices_List(ref opp, CarsWorkOrdersId_);
            invsRList = GetInvoices_List_Reverse(ref opp, CarsWorkOrdersId_);

            return CarsWorkOrdersVMList;

        }
        public bool ValidateToAddEditItem(ref string msg)
        {
            bool IsValid = true;


            if (carWorkOrder.IsWorkOrderClosed == 1 | carWorkOrder.LookUp_CarStatusLocationId == (long)CarStatusLocationEnum.InWaitingAcconting)
            {

                msg = "لا يمكن تنفيذ العملية أمر الشغل مغلق او تم تحويلة للحسابات";
                IsValid = false;
            }

            return IsValid;
        }
    }
}
