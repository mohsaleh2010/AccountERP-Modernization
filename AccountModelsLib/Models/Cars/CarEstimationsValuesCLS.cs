
using AccountModelsLib.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
 
namespace AccountModelsLib
{
    public class CarEstimationsValuesCLS
    {
        public Car car = new Car();
        public Customer customer = new Customer();
        public CarEstimationsValuesList estList = new CarEstimationsValuesList();
        public CarEstimationsValuesDetail estDet = new CarEstimationsValuesDetail();

        public List<CarEstimationsValuesDetail> estDet_Lst = new List<CarEstimationsValuesDetail>();

        //public Customer customer = new Customer();
        public AccountModelsLib.operations opp;
        public CarEstimationsValuesCLS()
        {
            opp = new AccountModelsLib.operations();
        }
        public CarEstimationsValuesCLS(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
        }


        public void AddCarEstimationsValuesList(ref int Excp, ref string Msg)
        {


            try
            {
                estList.EstimationsNumber = GetEstimationsNumber();

                opp.db.CarEstimationsValuesLists.Add(estList);
                opp.db.SaveChanges();
                Excp= 0;    
            }
            catch (Exception ee)
            {
                estList.Id= 0;
                Excp = 1;
                Msg= ee.Message;

            }
                

        }
        public void SaveCarEstimationsValuesList(ref int Excp, ref string Msg,   List<CarEstimationsValuesDetail> estDet_Lst)
        {
            try
            {
                opp = new AccountModelsLib.operations(2);

                if (estList.Id == 0)
                {
                    AddCarEstimationsValuesList(ref Excp, ref Msg);
                }
                else
                {
                    EditEstimationsValuesList();
                }
                if (Excp == 0)
                {
                    SaveCarEstimationsValuesDetails(ref Excp, ref Msg,  estDet_Lst);
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
                opp.Rollback();
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void SaveCarEstimationsValuesDetails(ref int Excp, ref string Msg, List<CarEstimationsValuesDetail> estDet_Lst)
        {
            try
            {
                string mysql = "Delete   from CarEstimationsValuesDetails Where CarEstimationsValuesListId=" + estList.Id;
                opp.insert_me(mysql);


                foreach (var item in estDet_Lst)
                {
                    item.Id = 0;
                    item.CarEstimationsValuesListId= estList.Id;
                    CarEstimationsValuesDetail itmD = item;
                    opp.db.CarEstimationsValuesDetails.Add(itmD);
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
        public long GetEstimationsNumber()
        {

            long EstimationsNumber = 0;
            int year = estList.EstimationsDate.Value.Year;

            long? maxAge = opp.db.CarEstimationsValuesLists.Where(p => p.EstimationsDate.Value.Year == year).Select(p => p.EstimationsNumber).DefaultIfEmpty(0).Max();




            if (maxAge.HasValue)
            {
                if (maxAge.Value == 0)
                {
                    EstimationsNumber = 1;
                }
                else
                {
                    EstimationsNumber = maxAge.Value;
                    EstimationsNumber++;
                }

            }

            return EstimationsNumber;
        }
        public void GetMainData(long CarId)
        {
            CarsCLS crs = new CarsCLS();
            try
            {
                if (CarId > 0)
                {
                    crs.getCarById(CarId);
                    car = crs.car;

                    /////////Get customer 
                    if (crs.car != null)
                    {
                        long CustomerId = crs.car.CustomerId;
                        CustomerCLS cs = new CustomerCLS();
                        cs.GetCustomerByIdAsNoTracking(CustomerId);
                        customer = cs.cs;
                    }
                }
            }
            catch (Exception ee)
            {

            }


        }
        public void getEstimationsValuesListId(long Id)
        {
            try
            {
                estList = new CarEstimationsValuesList();
                estList = opp.db.CarEstimationsValuesLists.Where(c => c.Id == Id).Include(a => a.Car).Include(a => a.Customer).FirstOrDefault();
                car = estList.Car;
                customer = estList.Customer;
            }
            catch (Exception ee)
            {
            }
        }
        public void getEstimationsValuesListIdWithItems(long Id)
        {
            try
            {
                estList = new CarEstimationsValuesList();
                estList = opp.db.CarEstimationsValuesLists.Where(c => c.Id == Id).Include(a => a.Car).Include(a => a.Customer).FirstOrDefault();
                car = estList.Car;
                customer = estList.Customer;
                estDet_Lst = opp.db.CarEstimationsValuesDetails.AsNoTracking().Where(i => i.CarEstimationsValuesListId == Id).ToList();

                GetItemProcessedListTotals(estList, estDet_Lst);
                ComputeTotals(estList);
            }
            catch (Exception ee)
            {


            }
        }
        public void GetItemProcessedListTotals(CarEstimationsValuesList invL, List<CarEstimationsValuesDetail> ItemProcessedList)
        {
            invL.Qnty_ValueWorkmanship =  0;
            invL.Qnty_ValueSpareParts =   0;
            invL.Qnty_ValueOils = 0;
            invL.Item_Net_TotalSpareParts =   0;
            invL.Item_Net_TotalOils = 0;
            invL.Item_Net_TotalWorkmanship =   0;
            invL.Total_ItemPriceWithoutTax =   0;
            invL.Net_Total =   0;
            invL.Total_Item_CommissionValue =  0;
            

            ItemCLS itmCls = new ItemCLS();
            foreach (var item in ItemProcessedList)
            {
                item.Qnty_value = item.Qnty_value ?? 0;
                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Item_Total_WithoutTax = item.Item_Total_WithoutTax ?? 0;
                item.Item_Net_Total_Price = item.Item_Net_Total_Price ?? 0;
                item.Net_Total_Item_Commission = item.Net_Total_Item_Commission ?? 0;
                long Item_Category_ID = item.Item_Category_ID ?? 0; ;
              //Items itm = itmCls.GetItemOnlyById(item.Item_Id);
              //  if(itm!=null)
              //  {
              //      Item_Category_ID = itm.Item_Category_ID??0;
              //  }
                
                ///قطع غيار
                if ( Item_Category_ID == (int)ItemCategoryEnum.SpareParts)
                {
                    invL.Qnty_ValueSpareParts = item.Qnty_value + invL.Qnty_ValueSpareParts;
                    invL.Item_Net_TotalSpareParts = item.Item_Net_Total_Price + invL.Item_Net_TotalSpareParts;

                }
                ///زيوت 
                if ( Item_Category_ID == (int)ItemCategoryEnum.Oils)
                {
                    invL.Qnty_ValueOils = item.Qnty_value + invL.Qnty_ValueOils;
                    invL.Item_Net_TotalOils = item.Item_Net_Total_Price + invL.Item_Net_TotalOils;


                }
                ///مصنعيات 
                if ( Item_Category_ID == (int)ItemCategoryEnum.Masnaheaiat)
                {
                    invL.Qnty_ValueWorkmanship = item.Qnty_value + invL.Qnty_ValueWorkmanship;
                    invL.Item_Net_TotalWorkmanship = item.Item_Net_Total_Price + invL.Item_Net_TotalWorkmanship;

                }
                invL.Total_ItemPriceWithoutTax = invL.Total_ItemPriceWithoutTax + item.Item_Total_WithoutTax;
                invL.Net_Total = invL.Net_Total + item.Item_Net_Total_Price;
                invL.Total_Item_CommissionValue = invL.Total_Item_CommissionValue + item.Net_Total_Item_Commission;
            }


        }
        public void ComputeTotals(CarEstimationsValuesList invL)
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


            ////Net totals
            ///From Item Detail
            //invL.Item_Net_TotalWorkmanship
            //invL.Item_Net_TotalSpareParts
            //invL.Item_Net_TotalOils
            invL.Net_Total = invL.Item_Net_TotalOils + invL.Item_Net_TotalSpareParts + invL.Item_Net_TotalWorkmanship;


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

            invL.Total_ListPriceWithoutTax_Workmanship = invL.Item_Net_TotalWorkmanship - invL.Total_CommissionListWorkmanship;
            invL.Total_ListPriceWithoutTax_SpareParts = invL.Item_Net_TotalSpareParts - invL.Total_CommissionListSpareParts;
            invL.Total_ListPriceWithoutTax_Oils = invL.Item_Net_TotalOils - invL.Total_CommissionListOils;
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
            //invL.TaxSales10 = 0;
            //invL.ATS05 = 0;
            ///From invoice  Form  
            //    invL.TaxSalesListWorkmanship_Rate
            //invL.TaxSalesListSpareParts_Rate
            //invL.TaxSalesListOils_Rate

            invL.TaxSalesListWorkmanship_Value = invL.Total_ListPriceWithoutTax_Workmanship * invL.TaxSalesListWorkmanship_Rate / 100;
            invL.TaxSalesListSpareParts_Value = invL.Total_ListPriceWithoutTax_SpareParts * invL.TaxSalesListSpareParts_Rate / 100;
            invL.TaxSalesListOils_Value = invL.Item_Net_TotalOils * invL.TaxSalesListOils_Rate / 100;
            invL.TotalListTaxSales10_Value = invL.TaxSalesListOils_Value + invL.TaxSalesListSpareParts_Value + invL.TaxSalesListWorkmanship_Value;
            invL.TaxSales10_Value = invL.TotalListTaxSales10_Value;

            ////////
            invL.NetInvoiceTotal_Workmanship = invL.Total_ListPriceWithoutTax_Workmanship + invL.TaxSalesListWorkmanship_Value - invL.ATS05ListWorkmanship_Value;
            invL.NetInvoiceTotal_SpareParts = invL.Total_ListPriceWithoutTax_SpareParts + invL.TaxSalesListSpareParts_Value - invL.ATS05ListSpareParts_Value;
            invL.NetInvoiceTotal_Oils = invL.Total_ListPriceWithoutTax_Oils + invL.TaxSalesListOils_Value - invL.ATS05ListOils_Value;
            invL.NetInvoiceTotal = invL.NetInvoiceTotal_Oils + invL.NetInvoiceTotal_SpareParts + invL.NetInvoiceTotal_Workmanship;
            invL.Net_Total = invL.NetInvoiceTotal;

            invL.Equivalent_Net_Total_Price = invL.Net_Total;
            invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

            invL.TafkeetNet_Total = TafkeetProj.NumberToText.GetText(invL.Net_Total ?? 0, invL.Currency_id ?? 1);
            SetValuesDecimals(ref invL);
        }

        void SetValuesDecimals(ref CarEstimationsValuesList invL)
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
            //invL.TaxSales10 = Common.RoundMePositive(invL.TaxSales10);
            //invL.ATS05 = Common.RoundMePositive(invL.ATS05);


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
        public void EditEstimationsValuesList( )
        {
             
                opp.db.Entry(estList).State = EntityState.Modified;
                opp.db.SaveChanges(); 
        }
        public void DeletetEstimationsValuesList(ref int Excp, ref string Msg,long Id)
        {
            try
            {
                Excp = 0;
                opp = new AccountModelsLib.operations(2);
                 
                
                CarEstimationsValuesList estList_ = opp.db.CarEstimationsValuesLists.Where(i => i.Id == Id).FirstOrDefault();
                if (estList_ != null)
                {
                    opp.db.CarEstimationsValuesLists.Remove(estList_);
                    opp.db.SaveChanges();
                }
                string mysql = "Delete   from CarEstimationsValuesDetails Where CarEstimationsValuesListId=" + estList_.Id;
                opp.insert_me(mysql);
                opp.Commit();
                Excp = 0;

            }
            catch (Exception ee)
            {
                opp.Rollback();
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                
                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void DeletetCarEstimationsValuesDetails(ref int Excp, ref string Msg, long Id)
        {
            try
            {
                Excp = 0;
                opp = new AccountModelsLib.operations(2);


                CarEstimationsValuesDetail est_det_ = opp.db.CarEstimationsValuesDetails.Where(i => i.Id == Id).FirstOrDefault();
                if (est_det_ != null)
                {
                    opp.db.CarEstimationsValuesDetails.Remove(est_det_);
                    opp.db.SaveChanges();
                }
                
                opp.Commit();
                Excp = 0;

            }
            catch (Exception ee)
            {
                opp.Rollback();
                AccountModelsLib.operations.DataDictionaryLogerException(ee);

                Excp = 1;
                Msg = ee.Message;
            }

        }
        public void EditEstimationsValuesListWithItems(ref int Excp, ref string Msg, List<CarsWorkOrdersRequestedDetail> deletedcarWorkOrderDetailItemsList)
        {
            //opp.BeginTransaction(2);

            //EditEstimationsValuesList(ref Excp, ref Msg);
            //if (Excp == 1)
            //{
            //    opp.Rollback();
            //    return;
            //}
            
            
            //if (Excp == 0)
            //{
            //    opp.Commit();
            //}
            //else
            //{
            //    opp.Rollback();
            //}
            
        }

        public List<CarsWorkOrdersViewModel> getEstimationsValues(long CarEstimationsValuesListsId,long  EstimationsNumber, string SearchStr, long CustomerId,   bool IsEntryDateTimeFrom, DateTime EntryDateTimeFrom, bool IsEntryDateTimeTo, DateTime EntryDateTimeTo)
        {
            long EstimationsNumber_ = long.Parse(EstimationsNumber.ToString());
            SearchStr = SearchStr.Trim();
            List<CarsWorkOrdersViewModel> carWorkOrdersVM = new List<CarsWorkOrdersViewModel>();
            IQueryable<CarEstimationsValuesList> query = opp.db.CarEstimationsValuesLists.Include(i => i.Customer).Include(s => s.Store).Include(i => i.Car).Include(i => i.Car.Lookup_CarShape);
            if (SearchStr.Trim() != "")
            {
                query = query.Where(i => i.Car.CarNumber.Contains(SearchStr) | i.Car.ChassisNumber.Contains(SearchStr) | i.Car.Model.Contains(SearchStr) | i.Car.CarType.Contains(SearchStr) | i.Customer.Tel.Contains(SearchStr));
            }
            if (EstimationsNumber>0)
            {
                query = query.Where(i => i.EstimationsNumber == EstimationsNumber_) ;
            }
            if (CarEstimationsValuesListsId > 0)
            {
                query = query.Where(i => i.Id == CarEstimationsValuesListsId);
            }
            if (CustomerId > 0)
            {
                query = query.Where(i => i.CustomerId == CustomerId);
            }
            
            if (IsEntryDateTimeFrom == true)
            {
                
                DateTime dat = EntryDateTimeFrom.Date;
                query = query.Where(i =>  i.EstimationsDate.Value.Date >= dat);


                //query = query.Where(i => i.EntryDateOnly >= EntryDateTimeFrom);
            }
            if (IsEntryDateTimeTo == true)
            {
                DateTime dat = EntryDateTimeTo.Date;
                query = query.Where(i =>  i.EstimationsDate.Value.Date <= dat);
                //query = query.Where(i => i.EntryDateTime <= EntryDateTimeTo);
            }

            query = query.OrderBy(i => i.Id);
            carWorkOrdersVM = GetCarEstimationsValuesViewModel(query);
            return carWorkOrdersVM;


        }
        List<CarsWorkOrdersViewModel> GetCarEstimationsValuesViewModel(IQueryable<CarEstimationsValuesList> query )
        {
            List<CarsWorkOrdersViewModel> carWorkOrdersVM = new List<CarsWorkOrdersViewModel>();

            int Serial_ = 1;
            foreach (var item in query)
            {
                 

                //int Serial_ = 1;
                CarsWorkOrdersViewModel cv = new CarsWorkOrdersViewModel();
                cv.Serial_ = Serial_++;
                cv.Id = item.Id;
                cv.WorkOrderNumber = item.EstimationsNumber;
                cv.WarshaStoreId = item.StoreId;
                if (item.Store != null)
                {
                    cv.WarshaStoreName = item.Store.Store_Name;
                }
                cv.CarsId = item.CarsId;
                cv.CustomerId = item.CustomerId;
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
               

                if (item.Customer != null)
                {
                    cv.CustomerName = item.Customer.Name;
                    cv.CustomerSalesMan = item.Customer.CompanyContactName;
                    cv.CustomerSubAccount_Id = item.Customer.SubAccount_Id;
                    cv.Customer_SubAccount_Name = item.Customer.Name;
                    cv.Address = item.Customer.Address;

                    cv.Tel = item.Customer.Tel;
                    cv.Mobile = item.Customer.Mobile;
                }


              

              
                cv.WorkOrdersDate = item.EstimationsDate;

              
                cv.ReceptionEngineerId = item.ReceptionEngineerId??0;
                cv.ReceptionEngineerName = item.ReceptionEngineerName;


               

                carWorkOrdersVM.Add(cv);
            }
            return carWorkOrdersVM;
        }

         

        public void getEstimationsValuesListIdWithItemsToPrint(long Id, ref List<CarEstimationsValuesDetail> invD_S, ref List<CarEstimationsValuesDetail> invD_O, ref List<CarEstimationsValuesDetail> invD_M)
        {
            try
            {
                estList = new CarEstimationsValuesList();
                estList = opp.db.CarEstimationsValuesLists.AsNoTracking().Where(c => c.Id == Id).Include(a => a.Car).Include(a => a.Customer).FirstOrDefault();
                car = estList.Car;
                customer = estList.Customer;
                //estDet_Lst = opp.db.CarEstimationsValuesDetails.AsNoTracking().Include(i => i.Items).Where(i => i.CarEstimationsValuesListId == Id).ToList();

                long Item_Category_ID = (long)ItemCategoryEnum.SpareParts;
                invD_S = new  List<CarEstimationsValuesDetail>();
                invD_S = opp.db.CarEstimationsValuesDetails.AsNoTracking().Where(i => i.CarEstimationsValuesListId == Id & i.Item_Category_ID == Item_Category_ID).ToList();

                Item_Category_ID = (long)ItemCategoryEnum.Oils;
                invD_O = new List<CarEstimationsValuesDetail>();
                invD_O = opp.db.CarEstimationsValuesDetails.AsNoTracking().Where(i => i.CarEstimationsValuesListId == Id & i.Item_Category_ID == Item_Category_ID).ToList();

                Item_Category_ID = (long)ItemCategoryEnum.Masnaheaiat;
                invD_M = new  List<CarEstimationsValuesDetail> ();
                invD_M = opp.db.CarEstimationsValuesDetails.AsNoTracking().Where(i => i.CarEstimationsValuesListId == Id & i.Item_Category_ID == Item_Category_ID).ToList();
            }
            catch (Exception ee)
            {


            }
        }

    }
}
