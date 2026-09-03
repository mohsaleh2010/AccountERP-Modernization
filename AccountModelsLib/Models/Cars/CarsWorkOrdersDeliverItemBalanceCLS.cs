using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{

    public class CarsWorkOrdersDeliverItemBalanceCLS
    {
        MappingCLS mp = new MappingCLS();
        public void AddCarsWorkOrdersDeliverItemBalance(ref AccountModelsLib.operations opp, CarsWorkOrdersDeliverItemBalance carsWorkOrdersDeliverItemBalance)
        {

            opp.db.CarsWorkOrdersDeliverItemBalances.Add(carsWorkOrdersDeliverItemBalance);
            opp.db.SaveChanges();
        }
        public CarsWorkOrdersDeliverItemBalance getCarsWorkOrdersDeliverItemBalance(ref AccountModelsLib.operations opp, long Id)
        {
            CarsWorkOrdersDeliverItemBalance carWorkOrder = opp.db.CarsWorkOrdersDeliverItemBalances.Where(c => c.Id == Id).FirstOrDefault();
            return carWorkOrder;
        }
        public CarsWorkOrdersDeliverItemBalance getCarsWorkOrdersDeliverItemBalance(ref AccountModelsLib.operations opp, long WarshaStoresId, long CarsWorkOrdersId, long Item_Id)
        {
            CarsWorkOrdersDeliverItemBalance carWorkOrder = opp.db.CarsWorkOrdersDeliverItemBalances.Where(c => c.Item_Id == Item_Id & c.CarsWorkOrdersId == CarsWorkOrdersId & c.WarshaStoresId == WarshaStoresId).FirstOrDefault();
            return carWorkOrder;
        }
        public CarsWorkOrdersDeliverItemBalance getCarsWorkOrdersDeliverItemBalance(ref AccountModelsLib.operations opp, long CarsWorkOrdersId, long Item_Id)
        {
            CarsWorkOrdersDeliverItemBalance carWorkOrder = opp.db.CarsWorkOrdersDeliverItemBalances.Where(c => c.Item_Id == Item_Id & c.CarsWorkOrdersId == CarsWorkOrdersId).FirstOrDefault();
            return carWorkOrder;
        }
        public void Set_CarsWorkOrdersDeliverItemBalance(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            CarsWorkOrdersDeliverItemBalance carItem = getCarsWorkOrdersDeliverItemBalance(ref opp, dp.CarsWorkOrdersId??0, dp.Item_Id);

            if (carItem != null)
            {
                if (Transaction_TypeForWarsha == 1)
                {
                    carItem.Larg_Qnty = carItem.Larg_Qnty + dp.qua;
                    carItem.Small_Qnty = carItem.Small_Qnty + dp.Part_qua;
                    carItem.Item_No = carItem.Item_No + dp.Item_No;
                    carItem.Qnty = carItem.Qnty + dp.Qnty;
                }
                else
                {
                    carItem.Larg_Qnty = carItem.Larg_Qnty - dp.qua;
                    carItem.Small_Qnty = carItem.Small_Qnty - dp.Part_qua;
                    carItem.Item_No = carItem.Item_No - dp.Item_No;
                    carItem.Qnty = carItem.Qnty - dp.Qnty;
                }
                opp.db.Entry(carItem).State = EntityState.Modified;
                opp.db.SaveChanges();

            }
            else
            {

                CarsWorkOrdersDeliverItemBalance cwDBalance = mp.MappingGetCarsWorkOrdersDeliverItemBalance(dp);
                opp.db.CarsWorkOrdersDeliverItemBalances.Add(cwDBalance);
                opp.db.SaveChanges();
            }


        }

        public void Delete_CarsWorkOrdersDeliverItemBalance(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            CarsWorkOrdersDeliverItemBalance carItem = getCarsWorkOrdersDeliverItemBalance(ref opp, dp.CarsWorkOrdersId.Value, dp.Item_Id);

            if (carItem != null)
            {
                 
                opp.db.Entry(carItem).State = EntityState.Deleted;
                opp.db.SaveChanges();

            }
            
        }
        public void Change_CarsWorkOrdersDeliverItemBalance(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            CarsWorkOrdersDeliverItemBalance carItem = getCarsWorkOrdersDeliverItemBalance(ref opp, dp.CarsWorkOrdersId.Value, dp.Item_Id);

            if (carItem != null)
            {
                carItem.CarsWorkOrdersId = dp.CarsWorkOrdersId;
                opp.db.Entry(carItem).State = EntityState.Modified;
                opp.db.SaveChanges();

            }

        }
        public long SetCarsWorkOrdersRequestedDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp,Order_Purshing order_Purshing, int Transaction_TypeForWarsha)
        {
            /////////// Update CarsWorkOrdersRequestedDetails With detail purchasw
            long CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId ?? 0;
            if (CarsWorkOrdersRequestedDetailsId > 0)
            {
                CarsWorkOrdersRequestedDetail crd = new CarsWorkOrdersRequestedDetail();
                crd = opp.db.CarsWorkOrdersRequestedDetails.Where(c => c.Id == CarsWorkOrdersRequestedDetailsId).FirstOrDefault();
                if (crd != null)
                {
                    crd.Order_PurshingId = dp.OrderTable_id;
                    crd.Detailes_PurshingId = dp.Id;

                    
                    crd.Order_Purshing_No = order_Purshing.Order_Purshing_No;
                    crd.Order_Purshing_Date = order_Purshing.Order_date;

                    


                    crd.CarsWorkOrdersId = dp.CarsWorkOrdersId??0;
                    crd.CarsWorkOrdersDeliverDetailsId = dp.CarsWorkOrdersDeliverDetailsId;


                    crd.Larg_Qnty_Deliver = crd.Larg_Qnty_Deliver ?? 0;
                    crd.Small_Qnty_Deliver = crd.Small_Qnty_Deliver ?? 0;
                    crd.Item_No_Deliver = crd.Item_No_Deliver ?? 0;
                    


                    if (Transaction_TypeForWarsha == 1)
                    {
                        crd.Qnty_Deliver = crd.Qnty_Deliver + crd.Qnty;
                        crd.Larg_Qnty_Deliver = crd.Larg_Qnty_Deliver + crd.Larg_Qnty;
                        crd.Small_Qnty_Deliver = crd.Small_Qnty_Deliver + crd.Small_Qnty;
                        crd.Item_No_Deliver = crd.Item_No_Deliver + crd.Item_No;
                        crd.Is_ItemDeliver = true;
                    }
                    if (Transaction_TypeForWarsha == 2)
                    {
                        crd.Qnty_Deliver = crd.Qnty_Deliver - crd.Qnty;
                        crd.Larg_Qnty_Deliver = crd.Larg_Qnty_Deliver - crd.Larg_Qnty;
                        crd.Small_Qnty_Deliver = crd.Small_Qnty_Deliver - crd.Small_Qnty;
                        crd.Item_No_Deliver = crd.Item_No_Deliver - crd.Item_No;
                        crd.Is_ItemDeliver = true;

                        crd.Qnty_Deliver = Common.GetZeroIfNegative(crd.Qnty_Deliver ?? 0);
                        crd.Larg_Qnty_Deliver = Common.GetZeroIfNegative(crd.Larg_Qnty_Deliver ?? 0);
                        crd.Small_Qnty_Deliver = Common.GetZeroIfNegative(crd.Small_Qnty_Deliver ?? 0);
                        crd.Item_No_Deliver = Common.GetZeroIfNegative(crd.Item_No_Deliver ?? 0);
                    }
                    opp.db.Entry(crd).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
            }
            else
            {
                if (Transaction_TypeForWarsha == 1)
                {
                    CarsWorkOrdersRequestedDetail crd = mp.MappingFromDetailes_Purshing(ref dp);

                    crd.Qnty_Deliver = dp.Qnty;
                    crd.Larg_Qnty_Deliver = dp.qua;
                    crd.Small_Qnty_Deliver = dp.Part_qua;
                    crd.Item_No_Deliver = dp.Item_No;

                    opp.db.CarsWorkOrdersRequestedDetails.Add(crd);
                    opp.db.SaveChanges();
                }
            }

            return CarsWorkOrdersRequestedDetailsId;
        }

        public void DeleteCarsWorkOrdersRequestedDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            /////////// Update CarsWorkOrdersRequestedDetails With detail purchasw
            long CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId ?? 0;
            if (CarsWorkOrdersRequestedDetailsId > 0)
            {
                CarsWorkOrdersRequestedDetail crd = new CarsWorkOrdersRequestedDetail();
                crd = opp.db.CarsWorkOrdersRequestedDetails.Where(c => c.Id == CarsWorkOrdersRequestedDetailsId).FirstOrDefault();
                if (crd != null)
                {
                    
                    opp.db.Entry(crd).State = EntityState.Deleted;
                    opp.db.SaveChanges();
                }
            } 
        }

        public void ChangeCarsWorkOrdersRequestedDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            /////////// Update CarsWorkOrdersRequestedDetails With detail purchasw
            long CarsWorkOrdersRequestedDetailsId = dp.CarsWorkOrdersRequestedDetailsId ?? 0;
            if (CarsWorkOrdersRequestedDetailsId > 0)
            {
                CarsWorkOrdersRequestedDetail crd = new CarsWorkOrdersRequestedDetail();
                crd = opp.db.CarsWorkOrdersRequestedDetails.Where(c => c.Id == CarsWorkOrdersRequestedDetailsId).FirstOrDefault();
                if (crd != null)
                {
                    crd.CarsWorkOrdersId = dp.CarsWorkOrdersId??0;
                   
                    opp.db.Entry(crd).State = EntityState.Modified;
                    opp.db.SaveChanges();
                }
            }
        }
        public long SetCarsWorkOrdersItemProcessedDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp,Order_Purshing order_Purshing, int Transaction_TypeForWarsha)
        {
            long CarsWorkOrdersItemProcessedDetailId = 0;
            CarsWorkOrdersItemProcessedDetailCLS ProcessedCls = new CarsWorkOrdersItemProcessedDetailCLS();

            //CarsWorkOrdersItemProcessedDetail cwP = opp.db.CarsWorkOrdersItemProcessedDetails.Where(i => i.Item_Id == dp.Item_Id & i.CarsWorkOrdersId == dp.CarsWorkOrdersId & i.Is_AddToInvoice == 0).FirstOrDefault();

            CarsWorkOrdersItemProcessedDetail cwP = ProcessedCls.GetItemProcessedByDetailPurchase(ref opp, dp);
            /////////// Update CarsWorkOrdersRequestedDetails With detail purchasw

            if (cwP != null)
            {


                cwP.Order_PurshingId = dp.OrderTable_id;
                cwP.Detailes_PurshingId = dp.Id;
                cwP.Order_Purshing_No = order_Purshing.Order_Purshing_No;
                cwP.Order_Purshing_Date = order_Purshing.Order_date;

                cwP.CarsWorkOrdersDeliverDetailsId = dp.CarsWorkOrdersDeliverDetailsId;

                cwP.Qnty = cwP.Qnty ?? 0;
                cwP.Larg_Qnty = cwP.Larg_Qnty ?? 0;
                cwP.Small_Qnty = cwP.Small_Qnty ?? 0;
                cwP.Item_No = cwP.Item_No ?? 0;

                if (Transaction_TypeForWarsha == 1)
                {
                    cwP.Qnty = cwP.Qnty + dp.Qnty;
                    cwP.Larg_Qnty = cwP.Larg_Qnty + dp.qua;
                    cwP.Small_Qnty = cwP.Small_Qnty + dp.Part_qua;
                    cwP.Item_No = cwP.Item_No + dp.Item_No;

                }
                if (Transaction_TypeForWarsha == 2)
                {
                    cwP.Qnty = cwP.Qnty - cwP.Qnty;
                    cwP.Larg_Qnty = cwP.Larg_Qnty - dp.qua;
                    cwP.Small_Qnty = cwP.Small_Qnty - dp.Part_qua;
                    cwP.Item_No = cwP.Item_No - dp.Item_No;


                    cwP.Qnty = Common.GetZeroIfNegative(cwP.Qnty ?? 0);
                    cwP.Larg_Qnty = Common.GetZeroIfNegative(cwP.Larg_Qnty ?? 0);
                    cwP.Small_Qnty = Common.GetZeroIfNegative(cwP.Small_Qnty ?? 0);
                    cwP.Item_No = Common.GetZeroIfNegative(cwP.Item_No ?? 0);
                }
                if (cwP.Qnty == 0)
                {
                    ProcessedCls.RemoveItemProcess(ref opp,cwP);
                    
                }
                else
                {
                    ProcessedCls.EditItemProcess(ref opp, cwP);
                   
                }
            }

            else
            {
                if (Transaction_TypeForWarsha == 1)
                {
                    cwP = new CarsWorkOrdersItemProcessedDetail();
                    cwP = mp.MappingGetCarsWorkOrdersItemProcessedDetailsFrom_dp(dp, order_Purshing);
                    
                    ProcessedCls.Add(ref opp, cwP);

                }
            }

            CarsWorkOrdersItemProcessedDetailId = cwP.Id;
            return CarsWorkOrdersItemProcessedDetailId;
        }
        public void DeleteCarsWorkOrdersItemProcessedDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            long CarsWorkOrdersItemProcessedDetailId = 0;
            CarsWorkOrdersItemProcessedDetailCLS ProcessedCls = new CarsWorkOrdersItemProcessedDetailCLS();

            //CarsWorkOrdersItemProcessedDetail cwP = opp.db.CarsWorkOrdersItemProcessedDetails.Where(i => i.Item_Id == dp.Item_Id & i.CarsWorkOrdersId == dp.CarsWorkOrdersId & i.Is_AddToInvoice == 0).FirstOrDefault();

            CarsWorkOrdersItemProcessedDetail cwP = ProcessedCls.GetItemProcessedByDetailPurchase(ref opp, dp);
            /////////// Update CarsWorkOrdersRequestedDetails With detail purchasw

            if (cwP != null)
            { 
                    ProcessedCls.RemoveItemProcess(ref opp, cwP);
                 
            }
        }
        public void ChangeCarsWorkOrdersItemProcessedDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            long CarsWorkOrdersItemProcessedDetailId = 0;
            CarsWorkOrdersItemProcessedDetailCLS ProcessedCls = new CarsWorkOrdersItemProcessedDetailCLS();

            //CarsWorkOrdersItemProcessedDetail cwP = opp.db.CarsWorkOrdersItemProcessedDetails.Where(i => i.Item_Id == dp.Item_Id & i.CarsWorkOrdersId == dp.CarsWorkOrdersId & i.Is_AddToInvoice == 0).FirstOrDefault();

            CarsWorkOrdersItemProcessedDetail cwP = ProcessedCls.GetItemProcessedByDetailPurchase(ref opp, dp);
            /////////// Update CarsWorkOrdersRequestedDetails With detail purchasw

            if (cwP != null)
            {
                cwP.CarsWorkOrdersId = dp.CarsWorkOrdersId;
                ProcessedCls.EditItemProcess(ref opp, cwP);

            }
        }

    }
}
