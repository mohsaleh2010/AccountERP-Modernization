using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections;
 
//using AccountModelsLib.DBModels;


namespace AccountModelsLib.Models
{
    public class CarsWorkOrdersDetailCLS
    {
        CarsWorkOrdersRequestedDetail CarsWorkOrdersDetail { get; set; }

        public CarsWorkOrdersDetailCLS()
        {

        }

        public void GetCarsWorkOrdersDetail(long carsWorkOrdersId)
        {

        }
        public List<CarsWorkOrdersRequestedDetail> GetCarsWorkOrdersRequestedDetails(ref AccountModelsLib.operations opp, long carsWorkOrdersId)
        {
            List<CarsWorkOrdersRequestedDetail> carsWorkOrdersDetails = opp.db.CarsWorkOrdersRequestedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId).Include(s => s.Store).ToList();
            return carsWorkOrdersDetails;
        }


        public List<ItemsOrderViewModel> GetCarsWorkOrdersDetailWithVM(ref AccountModelsLib.operations opp, long carsWorkOrdersId)
        {
            List<CarsWorkOrdersRequestedDetail> carsWorkOrdersDetails = opp.db.CarsWorkOrdersRequestedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId).Include(i => i.Items).Include(i => i.Items.unit).Include(u => u.Unit_item).Include(s => s.Store).ToList();
            return Set_ItemsViewModel(carsWorkOrdersDetails);
        }
        public List<ItemsOrderViewModel> GetItemsForCarsWithVM(ref AccountModelsLib.operations opp, long carsWorkOrdersId)
        {
            List<CarsWorkOrdersRequestedDetail> carsWorkOrdersDetails = opp.db.CarsWorkOrdersRequestedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId).Include(i => i.Items).Include(i => i.Items.unit).Include(u => u.Unit_item).Include(s => s.Store).ToList();
            return Set_ItemsViewModel(carsWorkOrdersDetails);
        }
        public List<ItemsOrderViewModel> Set_ItemsViewModel(List<CarsWorkOrdersRequestedDetail> carsWorkOrdersRequestedDetails)
        {
            //Items itm = new Items();
            List<ItemsOrderViewModel> itmsOrderVMList = new List<ItemsOrderViewModel>();

            foreach (CarsWorkOrdersRequestedDetail itm in carsWorkOrdersRequestedDetails)
            {
                ItemsOrderViewModel itmOrder_VM = new ItemsOrderViewModel();

                itmOrder_VM.Serial = 0;
                itmOrder_VM.Id = 0;
                itmOrder_VM.Item_Id = itm.Item_Id;
                itmOrder_VM.Unit_Item_Id = itm.Unit_ItemsId;

                itmOrder_VM.Item_Category_Id = itm.Item_Category_Id;
                itmOrder_VM.Item_Category_Name = itm.Item_Category_Name;


                itmOrder_VM.qua = itm.Larg_Qnty;
                itmOrder_VM.Part_Qua = itm.Small_Qnty;
                itmOrder_VM.Item_No = itm.Item_No;

                itmOrder_VM.Item_Name = itm.Invoice_Item_Name;
                itmOrder_VM.AnotherName = itm.Invoice_Item_Name;
                itmOrder_VM.ItemName_OnInvoice = itm.Invoice_Item_Name;


                itmOrder_VM.Store_id = itm.StoreId;

                itmOrder_VM.Store_Name = itm.Store.Store_Name;
                itmOrder_VM.Store_Type = itm.Store.StoreUsed_Type;

                //itmOrder_VM.Specification = itm.  ;
                itmOrder_VM.QnyStr = itm.QntyStr;
                itmOrder_VM.Code = itm.Items.code;
                itmOrder_VM.ItemCode = itm.Items.ItemCode;
                
                itmOrder_VM.Unit_Id = itm.Items.unit.id;
                itmOrder_VM.UnitStr = itm.Items.UnitStr;

                itmOrder_VM.B_Unit = itm.Items.unit.B_Unit;

                itmOrder_VM.S_unit = itm.Items.unit.S_unit;
                itmOrder_VM.Upon_Request = itm.Items.Upon_Request;
                itmOrder_VM.No3eboa = itm.Items.No3eboa;

                itmOrder_VM.S_Price = itm.PriceUnit;

                //itm_VMList.Small_unit = itm.Unit_item;
                itmOrder_VM.BlockNo = itm.Items.BlockNo;
                itmOrder_VM.shelfNo = itm.Items.shelfNo;
                itmOrder_VM.OrderOnshelfNo = itm.Items.OrderOnshelfNo;


                itmOrder_VM.Unit_Used_For_P_price_Default = itm.Items.Unit_Used_For_P_price_Default;
                itmOrder_VM.Unit_Used_For_S_price_Default = itm.Items.Unit_Used_For_S_price_Default;


                itmOrder_VM.CarsWorkOrdersId = itm.CarsWorkOrdersId;
                itmOrder_VM.CarsWorkOrdersRequestedDetailsId = itm.Id;
                itmOrder_VM.CarsWorkOrdersDeliverDetailsId = 0;

                itmsOrderVMList.Add(itmOrder_VM);
            }

            return itmsOrderVMList;

        }
        public bool CheckCarsWorkOrdersDetailToDelete(ref AccountModelsLib.operations opp, long carsWorkOrdersId)
        {
            bool IsValidToDelete = false;
            CarsWorkOrdersRequestedDetail carsWorkOrdersDetails = carsWorkOrdersDetails = opp.db.CarsWorkOrdersRequestedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == 0 & c.Is_SarfFromStore == 0).FirstOrDefault();
            if (carsWorkOrdersDetails != null)
            {
                IsValidToDelete = true;
            }

            return IsValidToDelete;
        }
        public bool CheckCarsWorkOrdersDetailRequestToDelete(ref int Excp,ref string Msg, ref AccountModelsLib.operations opp, long carsWorkOrdersId)
        {
            bool IsValidToDelete = true ;
            CarsWorkOrdersRequestedDetail carsWorkOrdersDetails = carsWorkOrdersDetails = opp.db.CarsWorkOrdersRequestedDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId & c.Is_AddToInvoice == 0 & c.Is_SarfFromStore == 0).FirstOrDefault();
            if (carsWorkOrdersDetails != null)
            {
                Excp = 1;
                Msg = "لا يمكن حذف الصنف لقدم تم صرفة من المخزن أو إضافتة للفاتورة";
                IsValidToDelete = false ;
            }
            //if (IsValidToDelete == true)
            //{
            //    MessageBox.Show("لا يمكن حذف الصنف لقدم تم صرفة من المخزن أو إضافتة للفاتورة");
            //    return;
            //}
            return IsValidToDelete;
        }
        public void AddOrEditDetails(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, List<CarsWorkOrdersRequestedDetail> carWorkOrderDetailItemsList)
        {
            try
            {
                foreach (CarsWorkOrdersRequestedDetail item in carWorkOrderDetailItemsList)
                {
                    if (item.Id > 0)
                    {
                        ///Edit
                        //opp.db.CarsWorkOrdersDetails.Attach(CarsWorkOrdersDetail);
                        opp.db.Entry(item).State = EntityState.Modified;
                        opp.db.SaveChanges();

                    }
                    else
                    {
                        opp.db.CarsWorkOrdersRequestedDetails.Add(item);
                        opp.db.SaveChanges();

                    }
                }
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void DeleteDetails(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, List<CarsWorkOrdersRequestedDetail> carsWorkOrdersDetailList)
        {
            foreach (var item in carsWorkOrdersDetailList)
            {
                string Msg_ = "";
                DeleteDetails(ref Excp, ref Msg_, ref opp, item);

                Msg = Msg + "\n\r" + Msg_;
            }
        }

        public void DeleteDetails(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, CarsWorkOrdersRequestedDetail carsWorkOrdersDetail)
        {
            try
            {

                if (carsWorkOrdersDetail.Id > 0)
                {
                    ///Edit
                    opp.db.CarsWorkOrdersRequestedDetails.Attach(carsWorkOrdersDetail);
                    //opp.db.Entry(CarsWorkOrdersDetail).State = System.Data.Entity.EntityState.Deleted;
                    opp.db.CarsWorkOrdersRequestedDetails.Remove(carsWorkOrdersDetail);
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
    }
}
