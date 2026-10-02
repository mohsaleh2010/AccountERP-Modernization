using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace AccountModelsLib.Models
{
    public class ItemsForCarsCLS
    {
        public operations opp;
        public ItemsForCarsCLS()
        {
            opp = new operations();
        }
        public ItemsForCarsCLS(ref operations opp_)
        {
            opp = opp_;
        }
        public List<ItemsForCars> itemsForCarsLst = new List<ItemsForCars>();
        public List<ItemsForCarsViewModel> itemsForCarsVMLst = new List<ItemsForCarsViewModel>();
        public List<ItemsOrderViewModel> itmsOrderVMList = new List<ItemsOrderViewModel>();
        public List<Items> itmsLst = new List<Items>();
        public List<Lookup_CarType> CarTypeLst = new List<Lookup_CarType>();
        public List<Items> GetAllItemsForCars(ref int Excp, ref string Msg, ref operations opp, Car car, long ItemCategoryId)
        {
            List<Items> itmList = new List<Items>();
            try
            {
                Excp = 0;
                Items im = new Items();
                //itmList = opp.db.Item.Include(s => s.Store).Include(u => u.unit).Where(i => i.ItemsForCars.Any(w => w.Lookup_CarTypeId == car.Lookup_CarTypeId & w.Lookup_CarTypeModeLId == car.Lookup_CarTypeModelId)).ToList();

                IQueryable<Items> query = opp.db.Item.Include(s => s.Store).Include(u => u.unit).Where(i => i.ItemsForCars.Any(w => w.Lookup_CarTypeId == car.Lookup_CarTypeId & w.Lookup_CarTypeModeLId == car.Lookup_CarTypeModelId));

                if (ItemCategoryId > 0)
                {
                    query = query.Where(i => i.Item_Category_ID == ItemCategoryId);
                }
                itmList = query.ToList();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
            return itmList;
        }

        public List<Items> GetAllItemsForCarsWorkOrdersRequestedDetail(ref int Excp, ref string Msg, ref operations opp, long? carsWorkOrdersId, List<CarsWorkOrdersRequestedDetail> carsWorkOrdersRequestedDetail, long ItemCategoryId)
        {
            List<Items> itmList = new List<Items>();
            try
            {
                Excp = 0;
                Items im = new Items();
                //itmList = opp.db.Item.Include(s => s.Store).Include(u => u.unit).Where(w => w.CarsWorkOrdersRequestedDetail.Any(a => a.CarsWorkOrdersId == carsWorkOrdersId)).ToList();

                ///////
                ///
                IQueryable<Items> query = opp.db.Item.Include(s => s.Store).Include(u => u.unit).Where(w => w.CarsWorkOrdersRequestedDetail.Any(a => a.CarsWorkOrdersId == carsWorkOrdersId));

                if (ItemCategoryId > 0)
                {
                    query = query.Where(i => i.Item_Category_ID == ItemCategoryId);
                }
                itmList = query.ToList();
                Excp = 0;

            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;

            }
            return itmList;
        }

        public List<ItemsOrderViewModel> GetAllItemsViewModel(ref int Excp, ref string Msg, long ItemCategoryId)
        {
            ItemCLS itmCls = new ItemCLS(ref opp);
            itmsOrderVMList = new List<ItemsOrderViewModel>();
            itmsOrderVMList = itmCls.GetAllItemsViewModel(ref Excp, ref Msg, ItemCategoryId);

            return itmsOrderVMList;
        }

        public List<ItemsOrderViewModel> GetAllItemsViewModel(ref int Excp, ref string Msg, long ItemCategoryId, List<ItemsForCarsViewModel> itemsForCarsVMLst)
        {
            GetAllItemsViewModel(ref   Excp, ref   Msg,   ItemCategoryId);
            List<ItemsOrderViewModel> itmsOrderVMListNew = new List<ItemsOrderViewModel>();
            foreach (var item in itmsOrderVMList)
            {
                bool IsFound = false;
                foreach (var itemCar in itemsForCarsVMLst)
                {
                    if (item.Item_Id == itemCar.Item_Id)
                    {
                        IsFound = true;
                        break;
                    }
                }
                if (!IsFound)
                {
                    itmsOrderVMListNew.Add(item);
                }
            }

            return itmsOrderVMListNew;
        }
        
        public List<Lookup_CarType> GetAllLookup_CarType(ref int Excp, ref string Msg)
        {
            Lookup_CarTypeCLS lookup_CarTypeCLS = new Lookup_CarTypeCLS(ref opp);
            CarTypeLst = lookup_CarTypeCLS.Get_CarTypeList();


            return CarTypeLst;
        }
        public List<ItemsForCarsViewModel> GetAllItemsForCarsViewModel(ref int Excp, ref string Msg, long Lookup_CarTypeId)
        {
            itemsForCarsVMLst = new List<ItemsForCarsViewModel>();
            itemsForCarsLst = new List<ItemsForCars>();
            itemsForCarsLst = opp.db.ItemsForCars.AsNoTracking().Where(i => i.Lookup_CarTypeId == Lookup_CarTypeId).Include(i => i.Item).Include(i => i.Lookup_CarType).ToList();

            Set_ItemsForCarsViewModel();
            return itemsForCarsVMLst;
        }
        public void Set_ItemsForCarsViewModel()
        {
            //Items itm = new Items();
            itemsForCarsVMLst = new List<ItemsForCarsViewModel>();

            foreach (ItemsForCars itm in itemsForCarsLst)
            {
                ItemsForCarsViewModel itmOrder_VM = new ItemsForCarsViewModel();


                itmOrder_VM.ItemsForCarsId = itm.Id;
                itmOrder_VM.Item_Id = itm.Item_Id;
                itmOrder_VM.Lookup_CarTypeId = itm.Lookup_CarTypeId;
                itmOrder_VM.Lookup_CarTypeModeLId = itm.Lookup_CarTypeModeLId;
                itmOrder_VM.IsMain = itm.IsMain;

                itmOrder_VM.Item_Name = itm.Item.Item_Name;
                itmOrder_VM.Lookup_CarTypeName = itm.Lookup_CarType.Name;
                itmOrder_VM.Lookup_CarTypeCode = itm.Lookup_CarType.Code;
                itmOrder_VM.Lookup_CarTypeModeLName = "";
                itemsForCarsVMLst.Add(itmOrder_VM);
            } 
        }

         
        public void DeleteCarsWorkOrdersItemProcessedEmployee(ref operations opp, long id)
        {
            ItemsForCars emp = opp.db.ItemsForCars.Where(i => i.Id == id).FirstOrDefault();
            if (emp != null)
            {
                opp.db.ItemsForCars.Remove(emp);
                opp.db.SaveChanges();
            }

        }

        public void AdditemsForCar(ItemsForCars itemsForCars)
        {
            if (itemsForCars != null)
            {
                ItemsForCars itemsCars = opp.db.ItemsForCars.Where(i => i.Item_Id == itemsForCars.Item_Id & i.Lookup_CarTypeId == itemsForCars.Lookup_CarTypeId).FirstOrDefault();
                if (itemsCars == null)
                {
                    opp.db.ItemsForCars.Add(itemsForCars);
                    opp.db.SaveChanges();
                }
            }
        }
        public void AddItemsForCarWhenAddItems(Items im)
        {
            if (im != null)
            {
                im.Lookup_CarTypeId = im.Lookup_CarTypeId ?? 0;
                if (im.Lookup_CarTypeId > 0)
                {
                    ItemsForCars itemsCars = new ItemsForCars();
                    itemsCars.Item_Id = im.Id;
                    itemsCars.Lookup_CarTypeId = im.Lookup_CarTypeId ?? 0;
                    itemsCars.Lookup_CarTypeModeLId = 0;
                    itemsCars.IsMain = 1;

                    opp.db.ItemsForCars.Add(itemsCars);
                    opp.db.SaveChanges();
                }
            }
        }
        public void AddItemsForCarWhenEditItems(Items im)
        {
            if (im != null)
            {
                ItemsForCars itemsCars = opp.db.ItemsForCars.Where(i => i.Item_Id == im.Id & i.IsMain == 1).FirstOrDefault();
                if (itemsCars != null)
                {
                    if (im.Lookup_CarTypeId > 0)
                    {
                        itemsCars.Item_Id = im.Id;
                        itemsCars.Lookup_CarTypeId = im.Lookup_CarTypeId ?? 0;
                        itemsCars.Lookup_CarTypeModeLId = 0;
                        itemsCars.IsMain = 1;

                        opp.db.Entry(itemsCars).State = EntityState.Modified;
                        opp.db.SaveChanges();
                    }
                    else
                    {
                        opp.db.Entry(itemsCars).State = EntityState.Deleted;
                        opp.db.SaveChanges();
                    }
                }
                else
                {
                    AddItemsForCarWhenAddItems(  im);
                }

            }
        }
    }

}
