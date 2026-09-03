using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public  class Detailes_PurshingCLS
    {
        public Detailes_PurshingCLS()
        {
            
        }
        public  Detailes_Purshing  GetDetailes_PurshingById(ref operations opp, long Detailes_PurshingsId)
        {
             Detailes_Purshing  dp  = opp.db.Detailes_Purshings.Where(i => i.Id == Detailes_PurshingsId).Include(o => o.Order_Purshing).Include(i => i.Items).Include(u => u.Unit_item).FirstOrDefault();
            return dp ;
        }
        public Detailes_Purshing GetDetailes_PurshingToDeleteByIdAsNoTracking(ref operations opp, long Detailes_PurshingsId)
        {
           Detailes_Purshing dp = opp.db.Detailes_Purshings.AsNoTracking().Where(i => i.Id == Detailes_PurshingsId).Include(o => o.Order_Purshing).Include(i => i.Items).Include(u => u.Unit_item).FirstOrDefault();
            //Detailes_Purshing dp = opp.db.Detailes_Purshings.AsNoTracking().Where(i => i.Id == Detailes_PurshingsId).Include(i => i.Items).Include(u => u.Unit_item).FirstOrDefault();
            return dp;
        }
        public Detailes_Purshing GetDetailes_PurshingASNoTrackingById(ref operations opp, long Detailes_PurshingsId)
        {
            Detailes_Purshing dp = opp.db.Detailes_Purshings.AsNoTracking().Where(i => i.Id == Detailes_PurshingsId).Include(o => o.Order_Purshing).Include(i => i.Items).Include(u => u.Unit_item).FirstOrDefault();
            return dp;
        }
        public List<Detailes_Purshing> GetDetailes_PurshingListByOrderId(ref operations opp, long OrderTable_id)
        {
            List<Detailes_Purshing> dpList = opp.db.Detailes_Purshings.Where(i => i.OrderTable_id == OrderTable_id).ToList();
            return dpList;
        }
        public List<Detailes_Purshing> GetDetailes_PurshingList(ref operations opp,long OrderTable_id)
        {
            List<Detailes_Purshing> dpList = opp.db.Detailes_Purshings.Where(i=>i.OrderTable_id== OrderTable_id).Include(o=>o.Order_Purshing).Include(i=>i.Items).Include(u=>u.Unit_item).ToList();
               return dpList;
        }
        public List<Detailes_Purshing> GetDetailes_PurshingAsNoTrackingListToCheckFound(ref operations opp, long OrderTable_id)
        {
            List<Detailes_Purshing> dpList = opp.db.Detailes_Purshings.AsNoTracking().Where(i => i.OrderTable_id == OrderTable_id).ToList();
            return dpList;
        }
        public void AddDetailes_Purshing(ref operations opp, Detailes_Purshing dp)
        {
            try
            {
                if (dp != null)
                {
                    opp.db.Detailes_Purshings.Add(dp);
                    opp.db.SaveChanges();

                     

                }
            }
            catch (Exception ee)
            {
                opp.db.Entry(dp).State = EntityState.Detached;
                throw;
            }

        }
        public void AddDetailes_Purshing(ref operations opp, Detailes_Purshing dp,Order_Purshing order_Purshing)
        {
            try
            {
                if (dp != null)
                {
                    opp.db.Detailes_Purshings.Add(dp);
                    opp.db.SaveChanges();

                    DataDictionaryLogerProcessCLS ddlpCLS = new DataDictionaryLogerProcessCLS(ref opp);
                    
                    ddlpCLS.AddNew(1, order_Purshing, dp);
                }
            }
            catch (Exception ee)
            {
                opp.db.Entry(dp).State = EntityState.Detached;
                throw;
            }

        }
        public void UpdateDetailes_Purshing(ref int Excp, ref operations opp, Detailes_Purshing dp)
        {
            opp.db.Entry(dp).State = EntityState.Modified;
            opp.db.SaveChanges();

        }
        public void DeleteDetailes_Purshing(ref operations opp, Detailes_Purshing dp)
        {
            if (dp != null)
            {
                if (dp.Id > 0)
                {
                    opp.db.Detailes_Purshings.Remove(dp);
                    opp.db.SaveChanges();
                }
            }

        }

     

    }
}
