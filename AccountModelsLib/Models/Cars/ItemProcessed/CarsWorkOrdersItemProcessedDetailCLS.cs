using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class CarsWorkOrdersItemProcessedDetailCLS
    {
        public CarsWorkOrdersItemProcessedDetailCLS()
        {

        }
        public List<CarsWorkOrdersItemProcessedDetail> GetCarsWorkOrdersItemProcessedDetailsList(ref AccountModelsLib.operations opp, long carsWorkOrdersId, int Is_AddToInvoice)
        {
            //List<CarsWorkOrdersItemProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == Is_AddToInvoice).Include(i => i.Items).Include(u => u.Unit).Include(s => s.Store).ToList();
            IQueryable<CarsWorkOrdersItemProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId);
            if (Is_AddToInvoice > -1)
            {
                carWorkOrderDetailProcessItemsList = carWorkOrderDetailProcessItemsList.Where(c => c.Is_AddToInvoice == Is_AddToInvoice);
            }
            carWorkOrderDetailProcessItemsList = carWorkOrderDetailProcessItemsList.Include(i => i.Items).Include(u => u.Unit).Include(s => s.Store);

            return carWorkOrderDetailProcessItemsList.ToList();
        }
        public List<CarsWorkOrdersItemProcessedDetail> GetItemProcessedListByCategory(ref AccountModelsLib.operations opp, long carsWorkOrdersId, long Item_Category_ID, int Is_AddToInvoice)
        {
            List<CarsWorkOrdersItemProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == Is_AddToInvoice & c.Items.Item_Category_ID == Item_Category_ID).Include(i => i.Items).Include(u => u.Unit).ToList();
            return carWorkOrderDetailProcessItemsList;
        }
        public List<CarsWorkOrdersItemProcessedDetail> GetItemProcessedListByCategoryAndIsMasna3iatKhamat(ref AccountModelsLib.operations opp, long carsWorkOrdersId, long Item_Category_ID, int Is_AddToInvoice)
        {
            List<CarsWorkOrdersItemProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == Is_AddToInvoice & c.Items.Item_Category_ID == Item_Category_ID & c.Items.IsMasna3iatKhamat == 1).Include(i => i.Items).Include(u => u.Unit).ToList();
            return carWorkOrderDetailProcessItemsList;
        }
        public CarsWorkOrdersItemProcessedDetail GetItemProcessedByDetailPurchase(ref AccountModelsLib.operations opp, Detailes_Purshing dp)
        {
            CarsWorkOrdersItemProcessedDetail cwP = opp.db.CarsWorkOrdersItemProcessedDetails.Where(i => i.Detailes_PurshingId == dp.Id).FirstOrDefault();
            //CarsWorkOrdersItemProcessedDetail cwP = opp.db.CarsWorkOrdersItemProcessedDetails.Where(i => i.Item_Id == dp.Item_Id & i.CarsWorkOrdersId == dp.CarsWorkOrdersId & i.Is_AddToInvoice == 0).FirstOrDefault();
            return cwP;
        }

        public bool ISItemProcessedFound(ref AccountModelsLib.operations opp, long carsWorkOrdersId, long Item_Id)
        {
            bool IsFound = false;
            CarsWorkOrdersItemProcessedDetail carsWorkOrdersItemProcessedDetail = opp.db.CarsWorkOrdersItemProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Item_Id == Item_Id & c.Is_AddToInvoice == 0).FirstOrDefault();
            if (carsWorkOrdersItemProcessedDetail == null)
            {
                IsFound = false;
            }
            else
            {
                IsFound = true;
            }
            return IsFound;
        }

        public int Add(ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemProcessedDetail> itemProcessedList_New)
        {
            int Count = 0;
            foreach (var item in itemProcessedList_New)
            {
                Count = Count + Add(ref opp, item);
            }
            //opp.db.CarsWorkOrdersItemProcessedDetails.AddRange(itemProcessedList_New);
            //int Count = opp.db.SaveChanges();
            return Count;
        }
        public int Add(ref AccountModelsLib.operations opp, CarsWorkOrdersItemProcessedDetail itemProcessed_New)
        {
            int Count = 0;
            int Excp = 0;
            string Msg = "";
            ItemCLS itmcls = new ItemCLS(ref opp);
            //Items itm = itmcls.GetItemById(ref Excp, ref Msg, itemProcessed_New.Item_Id);
            int? Is_NotShowOnInvoice = itmcls.GetItem_Is_NotShowOnInvoice(ref Excp, ref Msg, itemProcessed_New.Item_Id);
            //itemProcessed_New.Is_NotShowOnInvoice = itm.Is_NotShowOnInvoice ?? 0;
            itemProcessed_New.Is_NotShowOnInvoice = Is_NotShowOnInvoice;
            if (itemProcessed_New.Is_NotShowOnInvoice == 0)
            {
                opp.db.CarsWorkOrdersItemProcessedDetails.Add(itemProcessed_New);
                Count = opp.db.SaveChanges();
            }
            return Count;
        }
        public int SaveEditableItemProcessed(ref int excp, ref string msg, ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemProcessedDetail> itemProcessedList)
        {
            int Count = 0;
            CarsWorkOrdersItemProcessedEmployeeCLS empProCLS = new CarsWorkOrdersItemProcessedEmployeeCLS();
            foreach (var item in itemProcessedList)
            {

                if (item.Id > 0)
                {
                    opp.db.Entry(item).State = EntityState.Modified;
                    Count = Count + opp.db.SaveChanges();
                }
                else
                {
                    List<CarsWorkOrdersItemProcessedEmployee> newEmpList = item.CarsWorkOrdersItemProcessedEmployee;
                    item.CarsWorkOrdersItemProcessedEmployee = null;
                    Add(ref opp, item);

                    ////إضافة موظفين او الصانيعية
                    if (newEmpList != null)
                    {
                        if (newEmpList.Count > 0)
                        {
                            empProCLS.Add(ref opp, newEmpList, item.Id);
                            //foreach (var itmEmp in newEmpList)
                            //{

                            //    itmEmp.CarsWorkOrdersItemProcessedDetailsId = item.Id;
                            //    opp.db.CarsWorkOrdersItemProcessedEmployees.Add(itmEmp);
                            //    opp.db.SaveChanges();
                            //}
                        }
                    }
                }
            }

            return Count;
        }

        public bool CheckCarsWorkOrdersDetailToDelete(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long id)
        {
            try
            {
                //صلاحية حذف الأصناف المضافة الى أمر الشغل
                bool IsDelete_CarsWorkOrdersItemProcessedDetail = Common.IsHavePermissionSysMenu(Common.User_Id, 923, 2, -1);

                if (IsDelete_CarsWorkOrdersItemProcessedDetail == false)
                {
                    Excp = 916;
                    Msg = "ليس لديك صلاحية لحذف أصناف أمر الشغل";

                    return false;
                }
                CarsWorkOrdersItemProcessedDetail item = opp.db.CarsWorkOrdersItemProcessedDetails.Where(i => i.Id == id & i.Is_AddToInvoice == 0).FirstOrDefault();
                if (item == null)
                {
                    return false;
                }
                opp.db.CarsWorkOrdersItemProcessedDetails.Remove(item);
                opp.db.SaveChanges();

                Excp = 0;
                Msg = "تم حذف الصنف";

                return true;
            }
            catch (Exception e)
            {

                Excp = 1;
                Msg = e.Message;
                return false;
            }
        }

        public bool CheckCarsWorkOrdersDetailToDelete(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemProcessedDetail> ItemProcessedList)
        {
            bool IsDelete = false;

            foreach (var item in ItemProcessedList)
            {
                IsDelete = CheckCarsWorkOrdersDetailToDelete(ref Excp, ref Msg, ref opp, item.Id);
                if (Excp > 0)
                {
                    break;
                }
            }

            return IsDelete;
        }

        public void RemoveItemProcess(ref AccountModelsLib.operations opp, CarsWorkOrdersItemProcessedDetail cwP)
        {
            opp.db.Entry(cwP).State = EntityState.Deleted;
            opp.db.SaveChanges();
        }

        public void EditItemProcess(ref AccountModelsLib.operations opp, CarsWorkOrdersItemProcessedDetail cwP)
        {
            try
            {
                opp.db.Entry(cwP).State = EntityState.Modified;
                opp.db.SaveChanges();
            }
            catch (Exception ee)
            {
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                throw;
            }
        }
        public void EditItemProcess(ref AccountModelsLib.operations opp, long CarsWorkOrdersItemProcessedDetailsId, int Is_AddToInvoice)
        {
            try
            {
                CarsWorkOrdersItemProcessedDetail cp = opp.db.CarsWorkOrdersItemProcessedDetails.Where(p => p.Id == CarsWorkOrdersItemProcessedDetailsId).FirstOrDefault();
                if (cp != null)
                {
                    cp.Is_AddToInvoice = Is_AddToInvoice;
                    opp.db.Entry(cp).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }


            }
            catch (Exception ee)
            {
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                throw;
            }
        }

        public List<CarsWorkOrdersViewModel> getCarsWorkOrderSearchCategoryItem(ref AccountModelsLib.operations opp, long CarsId, long Item_Category_Id, long Item_Id, int IsWorkOrderClosed, bool IsEntryDateTimeFrom, DateTime EntryDateTimeFrom, bool IsEntryDateTimeTo, DateTime EntryDateTimeTo)
        {

            List<CarsWorkOrdersViewModel> carWorkOrdersVM = new List<CarsWorkOrdersViewModel>();
            IQueryable<CarsWorkOrdersItemProcessedDetail> query = opp.db.CarsWorkOrdersItemProcessedDetails.AsNoTracking().Include(i => i.CarsWorkOrder).Include(s => s.CarsWorkOrder.Customer).Include(i => i.CarsWorkOrder.Car);
            if (CarsId > 0)
            {
                query = query.Where(i => i.CarsWorkOrder.CarsId == CarsId);
            }
            if (IsWorkOrderClosed > -1)
            {
                query = query.Where(i => i.CarsWorkOrder.IsWorkOrderClosed == IsWorkOrderClosed);
                int IsInvoiceTaxInfo = Common.PermissionToShowTaxInvoiceOrNotORAll();
                if (IsInvoiceTaxInfo > -1 & IsWorkOrderClosed==1)
                {
                    query = query.Where(i => i.CarsWorkOrder.IsInvoiceTaxInfo == IsInvoiceTaxInfo);
                }
            }
            if (Item_Id > 0)
            {
                query = query.Where(i => i.Item_Id == Item_Id);
            }
            if (Item_Category_Id > 0)
            {
                query = query.Where(i => i.Item_Category_Id == Item_Category_Id);
            }

            if (IsEntryDateTimeFrom == true)
            {
                DateTime dat = EntryDateTimeFrom.Date;
                query = query.Where(i =>  i.CarsWorkOrder.EntryDateTime.Value.Date >= dat);
            }
            if (IsEntryDateTimeTo == true)
            {
                DateTime dat = EntryDateTimeTo.Date;
                query = query.Where(i => i.CarsWorkOrder.EntryDateTime.Value.Date <= dat);
                //query = query.Where(i => i.EntryDateTime <= EntryDateTimeTo);
            }

            //query = query.Where(item =>item.CarsWorkOrdersItemProcessedDetail.Where(order => order.Is_AddToInvoice == 0)
            //      .Select(order => order.Item_Net_Total_Price).Sum() > -1);
            query = query.OrderBy(i => i.Id);
            carWorkOrdersVM = GetCarsWorkOrdersViewModel(query.Distinct().ToList());
            return carWorkOrdersVM;

        }
        public List<CarsWorkOrdersViewModel> GetCarsWorkOrdersViewModel(List<CarsWorkOrdersItemProcessedDetail> query)
        {
            CarsWorkOrdersCLS carsWorkOrdersCLS = new CarsWorkOrdersCLS();
            List<CarsWorkOrdersViewModel> cvLst = new List<CarsWorkOrdersViewModel>();
            foreach (var item in query)
            {


                CarsWorkOrdersViewModel cv = new CarsWorkOrdersViewModel();

                //int Serial_ = 1;


                cv.Id = item.Id;
                cv.WorkOrderNumber = item.CarsWorkOrder.WorkOrderNumber;
                cv.WarshaStoreId = item.CarsWorkOrder.StoreId;
                if (item.CarsWorkOrder.Store != null)
                {
                    cv.WarshaStoreName = item.CarsWorkOrder.Store.Store_Name;
                }
                cv.CarsId = item.CarsWorkOrder.CarsId;
                cv.CustomerId = item.CarsWorkOrder.CustomerId;
                cv.ChassisNumber = item.CarsWorkOrder.Car.ChassisNumber;
                cv.CarNumber = item.CarsWorkOrder.Car.CarNumber;
                cv.CarType = item.CarsWorkOrder.Car.CarType;
                cv.Model = item.CarsWorkOrder.Car.Model;
                cv.EngineNumber = item.CarsWorkOrder.Car.EngineNumber;
                cv.FuelType = item.CarsWorkOrder.Car.FuelType;
                cv.Capacity = item.CarsWorkOrder.Car.Capacity;
                cv.Year = item.CarsWorkOrder.Car.Year;
                cv.TransmissionType = item.CarsWorkOrder.Car.TransmissionType;
                cv.ColorCode = item.CarsWorkOrder.Car.ColorCode;
                cv.CarColor = item.CarsWorkOrder.Car.CarColor;

                cv.Address = item.CarsWorkOrder.Address;

                if (item.CarsWorkOrder.Customer != null)
                {
                    cv.CustomerName = item.CarsWorkOrder.Customer.Name;
                    cv.CustomerSalesMan = item.CarsWorkOrder.Customer.CompanyContactName;
                    cv.CustomerSubAccount_Id = item.CarsWorkOrder.Customer.SubAccount_Id;
                    cv.Customer_SubAccount_Name = item.CarsWorkOrder.Customer.Name;
                }



                cv.CarMeterOnEntry = item.CarsWorkOrder.CarMeterOnEntry;
                cv.WorkOrdersDate = item.CarsWorkOrder.WorkOrdersDate;

                cv.EntryDateTime = item.CarsWorkOrder.EntryDateTime;
                cv.StartWorkDateTime = item.CarsWorkOrder.StartWorkDateTime;
                cv.ExpectedEndWorkDate = item.CarsWorkOrder.ExpectedEndWorkDate;
                cv.CustomerComplaint = item.CarsWorkOrder.CustomerComplaint;
                cv.RequiredWorks = item.CarsWorkOrder.RequiredWorks;
                cv.ReceptionEngineerId = item.CarsWorkOrder.ReceptionEngineerId;
                cv.ReceptionEngineerName = item.CarsWorkOrder.ReceptionEngineerName;


                cv.ReceptionManId = item.CarsWorkOrder.ReceptionManId;
                cv.ReceptionManName = item.CarsWorkOrder.ReceptionManName;
                cv.IsInvoiceTaxInfo = item.CarsWorkOrder.IsInvoiceTaxInfo;

                cv.WorkOrderClosedDate = item.CarsWorkOrder.WorkOrderClosedDate;
                cv.IsWorkOrderClosed = item.CarsWorkOrder.IsWorkOrderClosed;
                cv.TaxInternalId = item.CarsWorkOrder.TaxInternalId;
                cv.TaxInvoiceNo = item.CarsWorkOrder.TaxInvoiceNo;
                cv.IsUploadToPortal = item.CarsWorkOrder.IsUploadToPortal;
                cv.KhamatQnty_Value = item.CarsWorkOrder.KhamatQnty_Value ?? 0;
                cv.CustomerOrderNumberPO = item.CarsWorkOrder.CustomerOrderNumberPO;
                cv.CustomerOrderNumberGR = item.CarsWorkOrder.CustomerOrderNumberGR;

                ////////////Items

                cv.Invoice_Item_Name = item.Invoice_Item_Name;
                cv.Item_Category_Name = item.Item_Category_Name;
                cv.Qnty = item.Qnty;
                cv.PriceUnit = item.PriceUnit;
                cv.Item_Net_Total_Price = item.Item_Net_Total_Price;
                cv.Specification = item.Specification;
                cv.Lookup_CarEntryTaskeenName = item.CarsWorkOrder.Lookup_CarEntryTaskeenName;
                cv.LookUp_CarStatusLocationName = item.CarsWorkOrder.LookUp_CarStatusLocationName;


                carsWorkOrdersCLS.GetCarsWorkOrdersStatus(ref cv);

                cvLst.Add(cv);
            }

            return cvLst;
        }
    }
}
