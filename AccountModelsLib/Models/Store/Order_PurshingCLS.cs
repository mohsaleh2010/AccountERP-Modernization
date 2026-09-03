using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class Order_PurshingCLS
    {
        operations opp;
        public Order_PurshingCLS()
        {
            opp = new operations();
        }
        public Order_PurshingCLS(operations opp_)
        {
            opp = opp_;
        }
        public Order_Purshing GetOrder_PurshingId(ref operations opp, long Order_PurshingId)
        {
            Order_Purshing opMainId = opp.db.Order_Purshings.Where(i => i.Id == Order_PurshingId).FirstOrDefault();
            return opMainId;
        }
        public Order_Purshing GetOrder_PurshingIdAsNoTracking(long Order_PurshingId)
        {
            Order_Purshing opMainId = opp.db.Order_Purshings.AsNoTracking().Where(i => i.Id == Order_PurshingId).FirstOrDefault();
            return opMainId;
        }

        public List<Order_Purshing> GetByMainOrderTable_id(ref operations opp, long MainOrderTable_id)
        {
            List<Order_Purshing> opMainId = opp.db.Order_Purshings.Where(i => i.MainOrderTable_id == MainOrderTable_id).ToList();
            return opMainId;
        }
        public void AddOrder_Purshing(ref operations opp, Order_Purshing op)
        {
            if (op != null)
            {
                //Order_Purshing opnew = MappingHelper.MapScalarProperties<Order_Purshing, Order_Purshing>(op);

                opp.db.Order_Purshings.Add(op);
                opp.db.SaveChanges();
                //op.Id = opnew.Id;
                DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
                //ddlpCLS.AddNew(op.Order_Type.Value, 1, op.Id, op.Order_Purshing_No,0, "");
                ddlpCLS.AddNew(1, op, "");


            }

        }
        public void UpdateOrder_Purshing(ref int Excp, ref operations opp, Order_Purshing order_Purshing)
        {
            opp.db.Entry(order_Purshing).State = EntityState.Modified;
            opp.db.SaveChanges();

            DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
            //ddlpCLS.AddNew(order_Purshing.Order_Type.Value, 2, order_Purshing.Id, order_Purshing.Order_Purshing_No,0, "");
            ddlpCLS.AddNew(2, order_Purshing, "");


        }
        public void Update_Order_FromMainId(ref int Excp, ref operations opp, Order_Purshing order_Purshing, Order_Purshing order_Purshing_C)
        {
            string mysql = "Update Order_Purshing Set  MainOrderTable_id =" + order_Purshing_C.Id + " Where Id=" + order_Purshing.Id;
            opp.insert_me(mysql);
        }

        public long GetLastOrder_PurshingForcarWorkOrder(long CarsWorkOrdersId)
        {
            long Order_PurshingId = 0;
            List<Order_Purshing> opLst = opp.db.Order_Purshings.AsNoTracking().Where(i => i.CarsWorkOrdersId == CarsWorkOrdersId).OrderByDescending(i => i.Id).ToList();
            if (opLst.Count > 0)
            {
                Order_PurshingId = opLst[0].Id;
            }
            return Order_PurshingId;
        }
    }


}
