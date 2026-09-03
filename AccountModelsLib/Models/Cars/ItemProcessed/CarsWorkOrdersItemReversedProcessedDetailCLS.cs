using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class CarsWorkOrdersItemReversedProcessedDetailCLS
    {
        public List<CarsWorkOrdersItemReversedProcessedDetail> GetCarsWorkOrdersItemReversedProcessedDetailsList(ref AccountModelsLib.operations opp, long carsWorkOrdersId, int Is_AddToInvoice)
        {
            List<CarsWorkOrdersItemReversedProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemReversedProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == Is_AddToInvoice).Include(i => i.Items).Include(u => u.Unit).Include(s => s.Store).ToList();
            return carWorkOrderDetailProcessItemsList;
        }
        //public List<CarsWorkOrdersItemReversedProcessedDetail> GetCarsWorkOrdersItemReversedProcessedDetailsList(ref AccountModelsLib.operations opp, long carsWorkOrdersId, int Is_AddToInvoice)
        //{
        //    List<CarsWorkOrdersItemReversedProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == Is_AddToInvoice).Include(i => i.Items).Include(u => u.Unit).Include(s => s.Store).ToList();
        //    return carWorkOrderDetailProcessItemsList;
        //}
        //public List<CarsWorkOrdersItemProcessedDetail> GetItemProcessedListByCategory(ref AccountModelsLib.operations opp, long carsWorkOrdersId, long Item_Category_ID, int Is_AddToInvoice)
        //{
        //    List<CarsWorkOrdersItemProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == Is_AddToInvoice & c.Items.Item_Category_ID == Item_Category_ID).Include(i => i.Items).Include(u => u.Unit).ToList();
        //    return carWorkOrderDetailProcessItemsList;
        //}
        public List<CarsWorkOrdersItemReversedProcessedDetail> GetItemReversedProcessedListByCategory(ref AccountModelsLib.operations opp, long carsWorkOrdersId, long Item_Category_ID, int Is_AddToInvoice)
        {
            List<CarsWorkOrdersItemReversedProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemReversedProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == Is_AddToInvoice & c.Items.Item_Category_ID == Item_Category_ID).Include(i => i.Items).Include(u => u.Unit).ToList();
            return carWorkOrderDetailProcessItemsList;
        }
        public List<CarsWorkOrdersItemReversedProcessedDetail> GetItemProcessedListByCategoryAndIsMasna3iatKhamat(ref AccountModelsLib.operations opp, long carsWorkOrdersId, long Item_Category_ID, int Is_AddToInvoice)
        {
            List<CarsWorkOrdersItemReversedProcessedDetail> carWorkOrderDetailProcessItemsList = opp.db.CarsWorkOrdersItemReversedProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == Is_AddToInvoice & c.Items.Item_Category_ID == Item_Category_ID & c.Items.IsMasna3iatKhamat==1).Include(i => i.Items).Include(u => u.Unit).ToList();
            return carWorkOrderDetailProcessItemsList;
        }
        public CarsWorkOrdersItemReversedProcessedDetail GetItemProcessedByDetailPurchase(ref AccountModelsLib.operations opp, Detailes_Purshing dp)
        {
            CarsWorkOrdersItemReversedProcessedDetail cwP = opp.db.CarsWorkOrdersItemReversedProcessedDetails.Where(i => i.Item_Id == dp.Item_Id & i.CarsWorkOrdersId == dp.CarsWorkOrdersId & i.Is_AddToInvoice == 0).FirstOrDefault();
            return cwP;
        }

        public bool ISItemProcessedFound(ref AccountModelsLib.operations opp, long carsWorkOrdersId, long Item_Id)
        {
            bool IsFound = false;
            CarsWorkOrdersItemReversedProcessedDetail CarsWorkOrdersItemReversedProcessedDetail = opp.db.CarsWorkOrdersItemReversedProcessedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Item_Id == Item_Id & c.Is_AddToInvoice == 0).FirstOrDefault();
            if (CarsWorkOrdersItemReversedProcessedDetail == null)
            {
                IsFound = false;
            }
            else
            {
                IsFound = true;
            }
            return IsFound;
        }
       
        public int Add(ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemReversedProcessedDetail> itemProcessedList_New)
        {
            int Count = 0;
            foreach (var item in itemProcessedList_New)
            {
                Count = Count + AddReversed(ref opp, item);
            }
            //opp.db.CarsWorkOrdersItemReversedProcessedDetails.AddRange(itemProcessedList_New);
            //int Count = opp.db.SaveChanges();
            return Count;
        }
        public int AddReversed(ref AccountModelsLib.operations opp, CarsWorkOrdersItemReversedProcessedDetail itemProcessed_New)
        {
            int Count = 0;
            int Excp = 0;
            string Msg = "";
            ItemCLS itmcls = new ItemCLS();
            Items itm = itmcls.GetItemById(ref Excp, ref Msg, itemProcessed_New.Item_Id);

            itemProcessed_New.Is_NotShowOnInvoice = itm.Is_NotShowOnInvoice ?? 0;
            if (itemProcessed_New.Is_NotShowOnInvoice == 0)
            {
                opp.db.CarsWorkOrdersItemReversedProcessedDetails.Add(itemProcessed_New);
                Count = opp.db.SaveChanges();
            }
            return Count;
        }
        public int SaveEditableItemProcessed(ref int excp, ref string msg, ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemReversedProcessedDetail> itemProcessedList)
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
                    AddReversed(ref opp, item);

                    ////إضافة موظفين او الصانيعية
                    if (newEmpList != null)
                    {
                        if (newEmpList.Count > 0)
                        {
                            empProCLS.AddRevsrse(ref opp, newEmpList, item.Id);
                            
                        }
                    }
                }
            }

            return Count;
        }
        
        public bool CheckCarsWorkOrdersDetailToDelete(ref AccountModelsLib.operations opp, long id)
        {
            CarsWorkOrdersItemReversedProcessedDetail item = opp.db.CarsWorkOrdersItemReversedProcessedDetails.Where(i => i.Id == id & i.Is_AddToInvoice == 0).FirstOrDefault();
            if (item == null)
            {
                return false;
            }
            opp.db.CarsWorkOrdersItemReversedProcessedDetails.Remove(item);
            opp.db.SaveChanges();
            return true;
        }

        public bool CheckCarsWorkOrdersDetailToDelete(ref AccountModelsLib.operations opp, List<CarsWorkOrdersItemReversedProcessedDetail> ItemReversedProcessedList)
        {
            bool IsDelete = false;
            foreach (var item in ItemReversedProcessedList)
            {
                IsDelete = CheckCarsWorkOrdersDetailToDelete(ref opp, item.Id);
            }

            return IsDelete;
        }

        public void RemoveItemProcess(ref AccountModelsLib.operations opp, CarsWorkOrdersItemReversedProcessedDetail cwP)
        {
            opp.db.Entry(cwP).State = EntityState.Deleted;
            opp.db.SaveChanges();
        }

        public void EditItemProcess(ref AccountModelsLib.operations opp, CarsWorkOrdersItemReversedProcessedDetail cwP)
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
        public void EditItemReversedProcess(ref AccountModelsLib.operations opp, long CarsWorkOrdersItemReversedProcessedDetailsId,int Is_AddToInvoice)
        {
            try
            {
                CarsWorkOrdersItemReversedProcessedDetail cp = opp.db.CarsWorkOrdersItemReversedProcessedDetails.Where(p => p.Id == CarsWorkOrdersItemReversedProcessedDetailsId).FirstOrDefault();
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
        
       
    }
}
