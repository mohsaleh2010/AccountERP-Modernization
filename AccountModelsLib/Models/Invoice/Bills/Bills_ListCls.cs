using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountModelsLib.Models;


namespace AccountModelsLib.Models
{
    public class Bills_ListCls
    {
        AccountModelsLib.operations opp;
        public Bills_ListCls()
        {
            opp = new AccountModelsLib.operations();
        }
        public Bills_ListCls(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
        }
        public Bills_List invList = new Bills_List();

        //public List<Bills_Details> invDetails { get; set; }

        public List<Bills_Details> invD = new List<Bills_Details>();
        public List<Bills_Details_Reverse> invDR = new List<Bills_Details_Reverse>();
        public Bills_Details invDNewEdit { get; set; } = new Bills_Details();
        public void AddBills_List(ref int Excp, ref string Msg,  Bills_List invList)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            AddBills_List(ref   Excp, ref   Msg, ref   opp,   invList);
        }
        public void AddBills_List(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Bills_List invList)
        {
            if (invList.Id == 0)
            {
                try
                {

                    opp.db.Bills_List.Add(invList);
                    opp.db.SaveChanges();
                    Excp = 0;
                }
                catch (Exception ee)
                {
                    invList.Id = 0;
                    AccountModelsLib.operations.DataDictionaryLogerException(ee);
                    Msg = ee.Message;
                    Excp = 1;
                }
            }
            else
            {
                EditBills_List(ref Excp, ref Msg, ref opp, invList);
            }
        }

        public void DeleteBills_List(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Bills_List invList)
        {
            try
            {
                opp.db.Bills_List.Remove(invList);
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                invList.Id = 0;
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                Msg = ee.Message;
                Excp = 1;
            }
        }
        public void EditBills_List(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Bills_List invList)
        {
            try
            {
                opp.db.Entry(invList).State = EntityState.Modified;
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                //invList.Id = 0;
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                Msg = ee.Message;
                Excp = 1;
            }
        }
        public void AddBills_Details(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Bills_Details invD)
        {
            try
            {
                opp.db.Bills_Details.Add(invD);
                opp.db.SaveChanges();
                Excp = 0;


            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }



        public void EditBills_Details(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Bills_Details invD)
        {
            try
            {
                opp.db.Entry(invD).State = EntityState.Modified;
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }

        public void DeleteBills_Details(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, Bills_Details invD)
        {
            try
            {
                opp.db.Entry(invD).State = EntityState.Deleted;
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }

        public void GetBills_List(long Bills_ListId)
        {
            invList = new Bills_List();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            //My_DbContext.ConnStr = opp.str_sql_conn;
            //My_DbContext db = new My_DbContext();

            try
            {
                invList = opp.db.Bills_List.AsNoTracking().Where(i => i.Id == Bills_ListId).FirstOrDefault();
            }
            catch (Exception ee)
            {

            }
        }
        public Bills_List GetBills_ListById(ref AccountModelsLib.operations opp, long Bills_ListId)
        {
            invList = new Bills_List();



            invList = opp.db.Bills_List.Where(i => i.Id == Bills_ListId).FirstOrDefault();
            return invList;

        }
        public Bills_List GetBills_ListById(long Bills_ListId)
        {
            invList = GetBills_ListById(ref opp, Bills_ListId);
            return invList;

        }
        public List<Bills_List> GetBills_ListByCarsWorkOrdersId(ref AccountModelsLib.operations opp, long CarsWorkOrdersId)
        {
            List<Bills_List> invsList = new List<Bills_List>();

            invsList = opp.db.Bills_List.Where(i => i.CarsWorkOrdersId == CarsWorkOrdersId).ToList();
            return invsList;

        }
        public List<Bills_List> GetBills_ListByCarsWorkOrdersIdAsNoTracking(ref AccountModelsLib.operations opp, long CarsWorkOrdersId)
        {
            List<Bills_List> invsList = new List<Bills_List>();

            invsList = opp.db.Bills_List.AsNoTracking().Where(i => i.CarsWorkOrdersId == CarsWorkOrdersId).ToList();
            return invsList;

        }
        public List<Bills_Details> GetBills_Details(ref AccountModelsLib.operations opp, long Bills_ListId)
        {
            invD = new List<Bills_Details>();
            if (opp == null)
            {
                opp = new AccountModelsLib.operations();
            }
            //context.Configuration.ProxyCreationEnabled = false;
            opp.db.ChangeTracker.LazyLoadingEnabled = false;
            invD = opp.db.Bills_Details.AsNoTracking().Where(i => i.Invoice_Id == Bills_ListId).ToList();
            return invD;

             

        }

        public List<Bills_Details> GetBills_Details(ref AccountModelsLib.operations opp, long Bills_ListId, long Item_CategoryId)
        {
            invD = new List<Bills_Details>();

            invD = opp.db.Bills_Details.Where(i => i.Invoice_Id == Bills_ListId & i.Items.Item_Category_ID == Item_CategoryId).Include(i => i.Items).Include(u => u.Unit).ToList();
            return invD;

        }

        public List<Bills_Details_Reverse> GetBills_Details_Reverse(ref AccountModelsLib.operations opp, long invoice_List_Table_Id)
        {
            invDR = new List<Bills_Details_Reverse>();
            if (opp == null)
            {
                opp = new AccountModelsLib.operations();
            }
            //context.Configuration.ProxyCreationEnabled = false;
            //opp.db.Configuration.ProxyCreationEnabled = false;
            opp.db.ChangeTracker.LazyLoadingEnabled = false;
            invDR = opp.db.Bills_Details_Reverse.AsNoTracking().Where(i => i.Invoice_Id == invoice_List_Table_Id).ToList();
            return invDR;



            
        }
    }
}
