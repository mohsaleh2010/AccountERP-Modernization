using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class CarsWorkOrdersDeliverDetailCLS
    {


        public void Add(ref AccountModelsLib.operations opp, CarsWorkOrdersDeliverDetail de)
        {
            opp.db.CarsWorkOrdersDeliverDetails.Add(de);
            opp.db.SaveChanges();   
        }
        public void Edit(ref AccountModelsLib.operations opp, CarsWorkOrdersDeliverDetail de)
        {
            opp.db.Entry(de).State=EntityState.Modified;
            opp.db.SaveChanges();
        }
        public void Delete(ref AccountModelsLib.operations opp, CarsWorkOrdersDeliverDetail de)
        {
            opp.db.Entry(de).State = EntityState.Deleted;
            opp.db.SaveChanges();
        }
        public  List<CarsWorkOrdersDeliverDetail> SetCarsWorkOrdersItemDeliverDetail(ref AccountModelsLib.operations opp, long carsWorkOrdersId)
        {
            List<CarsWorkOrdersDeliverDetail> carsWorkOrdersDeliverDetails = opp.db.CarsWorkOrdersDeliverDetails.Where(c => c.CarsWorkOrdersId == carsWorkOrdersId).Include(i=>i.items).ToList();
            return carsWorkOrdersDeliverDetails;
        }
        public CarsWorkOrdersDeliverDetail GetCarsWorkOrdersDeliverDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp)
        {
            CarsWorkOrdersDeliverDetail cwP = opp.db.CarsWorkOrdersDeliverDetails.Where(i => i.Detailes_PurshingId == dp.Id).FirstOrDefault();
            //CarsWorkOrdersDeliverDetail cwP = opp.db.CarsWorkOrdersDeliverDetails.Where(i => i.Item_Id == dp.Item_Id & i.CarsWorkOrdersId == dp.CarsWorkOrdersId & i.Is_AddToInvoice == 0).FirstOrDefault();
            return cwP;
        }
        public CarsWorkOrdersDeliverDetail Set_CarsWorkOrdersDeliverDetail(ref AccountModelsLib.operations opp,Detailes_Purshing  dp,Order_Purshing order_Purshing, int Transaction_TypeForWarsha)
        {
            CarsWorkOrdersDeliverDetail itmDeliverDetail = new CarsWorkOrdersDeliverDetail();

            itmDeliverDetail = GetCarsWorkOrdersDeliverDetail(ref   opp,   dp);
            if (itmDeliverDetail == null)
            {
                itmDeliverDetail = new CarsWorkOrdersDeliverDetail();
            }
            MappingCLS mp = new MappingCLS();
            itmDeliverDetail = mp.MappingGetCarsWorkOrdersDeliverDetail(ref itmDeliverDetail, dp, order_Purshing);

            if (itmDeliverDetail.Id > 0)
            {
                Edit(ref opp, itmDeliverDetail);
            }
            else
            {
                Add(ref opp, itmDeliverDetail);
            }

            return itmDeliverDetail;
        }
        public void Delete_CarsWorkOrdersDeliverDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            CarsWorkOrdersDeliverDetail itmDeliverDetail = new CarsWorkOrdersDeliverDetail();

            itmDeliverDetail = GetCarsWorkOrdersDeliverDetail(ref opp, dp);
            if (itmDeliverDetail != null)
            {
                Delete(ref opp, itmDeliverDetail);
            } 
        }
        public void Change_CarsWorkOrdersDeliverDetail(ref AccountModelsLib.operations opp, Detailes_Purshing dp, int Transaction_TypeForWarsha)
        {
            CarsWorkOrdersDeliverDetail itmDeliverDetail = new CarsWorkOrdersDeliverDetail();

            itmDeliverDetail = GetCarsWorkOrdersDeliverDetail(ref opp, dp);
            if (itmDeliverDetail != null)
            {
                itmDeliverDetail.CarsWorkOrdersId = dp.CarsWorkOrdersId;
                
                Edit(ref opp, itmDeliverDetail);
            }
        }
    }
}
