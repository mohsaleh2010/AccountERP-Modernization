using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountModelsLib.Models;


namespace AccountModelsLib.Models
{
    public class Invoices_ListCls
    {
        operations opp;
        public Invoices_ListCls()
        {
            opp = new operations();
        }
        public Invoices_ListCls(ref operations opp_)
        {
            opp = opp_;
        }
        public Invoices_List invList = new Invoices_List();

        //public List<Invoices_Details> invDetails { get; set; }

        public List<Invoices_Details> invD = new List<Invoices_Details>();
        public Invoices_Details invDNewEdit { get; set; } = new Invoices_Details();
        public void AddInvoices_List(ref int Excp, ref string Msg,  Invoices_List invList)
        {
            operations opp = new operations();
            AddInvoices_List(ref   Excp, ref   Msg, ref   opp,   invList);
        }
        public void AddInvoices_List(ref int Excp, ref string Msg, ref operations opp, Invoices_List invList)
        {
            if (invList.Id == 0)
            {
                try
                {
                    DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp, ref Msg, ref opp, invList);

                    opp.db.Invoices_List.Add(invList);
                    opp.db.SaveChanges();
                    Excp = 0;

                    invList.Invoices_ListTempId = invList.Id;
                    opp.db.SaveChanges();

                     
                }
                catch (Exception ee)
                {
                    invList.Id = 0;
                    operations.DataDictionaryLogerException(ee);
                    Msg = ee.Message;
                    Excp = 1;
                }
            }
            else
            {
                EditInvoices_List(ref Excp, ref Msg, ref opp, invList);
            }
        }

        public void DeleteInvoices_List(ref int Excp, ref string Msg, ref operations opp, Invoices_List invList)
        {
            try
            {
                opp.db.Invoices_List.Remove(invList);
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                invList.Id = 0;
                operations.DataDictionaryLogerException(ee);
                Msg = ee.Message;
                Excp = 1;
            }
        }
        public void EditInvoices_List(ref int Excp, ref string Msg, ref operations opp, Invoices_List invList)
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
                operations.DataDictionaryLogerException(ee);
                Msg = ee.Message;
                Excp = 1;
            }
        }

        public void Update_Invoices_List(ref int Excp, ref operations opp,  Invoices_List invL)
        {

            try
            {
                invL.Invoice_DateTime = invL.Invoice_Date;
                invL.Invoice_Date = invL.Invoice_Date.Value.Date;
                invL.Equivalent_Net_Total_Price = invL.Net_Total;
                invL.Equivalent_Net_Total_CommissionValue = invL.Net_Total_CommissionValue;

                opp.cmd.Parameters.Clear();

                opp.db.Entry(invL).State = EntityState.Modified;
                opp.db.SaveChanges();
                Excp = 0;

            }
            catch (Exception ee)
            {

                Excp = 1;

            }
        }
        public void AddInvoices_Details(ref int Excp, ref string Msg, ref operations opp, Invoices_Details invD)
        {
            try
            {
                opp.db.Invoices_Details.Add(invD);
                opp.db.SaveChanges();
                Excp = 0;


            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }



        public void EditInvoices_Details(ref int Excp, ref string Msg, ref operations opp, Invoices_Details invD)
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

        public void DeleteInvoices_Details(ref int Excp, ref string Msg, ref operations opp, Invoices_Details invD)
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

        public void GetInvoices_List(long Invoices_ListId)
        {
            invList = new Invoices_List();
            operations opp = new operations();
            //My_DbContext.ConnStr = opp.str_sql_conn;
            //My_DbContext db = new My_DbContext();

            try
            {
                invList = opp.db.Invoices_List.AsNoTracking().Where(i => i.Id == Invoices_ListId).FirstOrDefault();
            }
            catch (Exception ee)
            {

            }
        }
        public Invoices_List Get_Invoices_List(ref operations opp, long Invoice_List_Table_Id)
        {
           
            try
            {
                return opp.db.Invoices_List.AsNoTracking().Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault();

            }
            catch (Exception ee)
            {
                return null;
            }
        }
        public List<Invoices_List> GetInvoices_ListLstByCustomer(long Customer_Id)
        {
           List<Invoices_List> invLst = new List<Invoices_List>();
            operations opp = new operations();
            //My_DbContext.ConnStr = opp.str_sql_conn;
            //My_DbContext db = new My_DbContext();

            try
            {
                invLst = opp.db.Invoices_List.AsNoTracking().Where(i => i.Customer_Id == Customer_Id).ToList();
            }
            catch (Exception ee)
            {

            }
            return invLst;  
        }
        public Invoices_List GetInvoices_ListById(ref operations opp, long Invoices_ListId)
        {
            invList = new Invoices_List();



            invList = opp.db.Invoices_List.Where(i => i.Id == Invoices_ListId).FirstOrDefault();
            return invList;

        }
        public Invoices_List GetInvoices_ListById(long Invoices_ListId)
        {
            invList = GetInvoices_ListById(ref opp, Invoices_ListId);
            return invList;

        }
        public List<Invoices_List> GetInvoices_ListByCarsWorkOrdersId(ref operations opp, long CarsWorkOrdersId)
        {
            List<Invoices_List> invsList = new List<Invoices_List>();

            invsList = opp.db.Invoices_List.Where(i => i.CarsWorkOrdersId == CarsWorkOrdersId).ToList();
            return invsList;

        }
        public List<Invoices_List> GetInvoices_ListByCarsWorkOrdersIdAsNoTracking(ref operations opp, long CarsWorkOrdersId)
        {
            List<Invoices_List> invsList = new List<Invoices_List>();

            invsList = opp.db.Invoices_List.AsNoTracking().Where(i => i.CarsWorkOrdersId == CarsWorkOrdersId).ToList();
            return invsList;

        }
        public List<Invoices_Details> GetInvoices_Details(ref operations opp, long Invoices_ListId)
        {
            invD = new List<Invoices_Details>();
            if(opp==null)
            {
                opp = new operations();
            }
            invD = opp.db.Invoices_Details.Where(i => i.Invoice_Id == Invoices_ListId).ToList();
            return invD;

        }
       
        public List<Invoices_Details> GetInvoices_Details(ref operations opp, long Invoices_ListId, long Item_CategoryId)
        {
            invD = new List<Invoices_Details>();

            invD = opp.db.Invoices_Details.Where(i => i.Invoice_Id == Invoices_ListId & i.Items.Item_Category_ID == Item_CategoryId).Include(i => i.Items).Include(u => u.Unit).ToList();
            return invD;

        }

        
    }
}
