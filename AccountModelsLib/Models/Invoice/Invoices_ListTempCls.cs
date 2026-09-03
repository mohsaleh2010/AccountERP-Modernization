using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountModelsLib.Models;
 

namespace AccountModelsLib.Models
{
    public class Invoices_ListTempCls
    {
        operations opp;
        public Invoices_ListTempCls()
        {
            opp = new operations();
        }
        public Invoices_ListTempCls(ref operations opp_)
        {
            opp = opp_;
        }
        public Invoices_ListTemp invList = new Invoices_ListTemp();
        //public List<Invoices_DetailsTemp> invDetails { get; set; }

        public List<Invoices_DetailsTemp> invD = new List<Invoices_DetailsTemp>();
        public Invoices_DetailsTemp invDNewEdit { get; set; } = new Invoices_DetailsTemp();
        public void AddInvoices_List(ref int Excp, ref string Msg,  Invoices_ListTemp invList)
        {
            operations opp = new operations();
            AddInvoices_List(ref   Excp, ref   Msg, ref   opp,   invList);
        }
        public void AddInvoices_List(ref int Excp, ref string Msg, ref operations opp, Invoices_ListTemp invList)
        {
            if (invList.Id == 0)
            {
                try
                {
                    //DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp, ref Msg, ref opp, invList);

                    opp.db.Invoices_ListTemp.Add(invList);
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
            else
            {
                EditInvoices_List(ref Excp, ref Msg, ref opp, invList);
            }
        }

        public void DeleteInvoices_List(ref int Excp, ref string Msg, ref operations opp, Invoices_ListTemp invList)
        {
            try
            {
                opp.db.Invoices_ListTemp.Remove(invList);
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
        public void DeleteInvoices_ListTemp(ref int Excp, ref string Msg, ref operations opp,long Invoices_ListTempId  )
        {
            try
            {
                Invoices_ListTemp invoices_ListTemp= opp.db.Invoices_ListTemp.Where(i => i.Id == Invoices_ListTempId).FirstOrDefault();
                if (invoices_ListTemp != null)
                {
                    opp.db.Invoices_ListTemp.Remove(invoices_ListTemp);
                }

                List<Invoices_DetailsTemp> invoices_DetailsTempLst = opp.db.Invoices_DetailsTemp.Where(i => i.Invoice_Id == Invoices_ListTempId).ToList();

                if (invoices_DetailsTempLst != null & invoices_DetailsTempLst.Count > 0)
                {
                    opp.db.Invoices_DetailsTemp.RemoveRange(invoices_DetailsTempLst);
                }
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                 
                operations.DataDictionaryLogerException(ee);
                Msg = ee.Message;
                Excp = 1;
            }
        }
        public void ChangeStatus_Invoices_ListTemp(ref int Excp, ref string Msg, ref operations opp, long Invoices_ListTempId, int Status)
        {
            try
            {
                Invoices_ListTemp invoices_ListTemp = opp.db.Invoices_ListTemp.Where(i => i.Id == Invoices_ListTempId).FirstOrDefault();
                invoices_ListTemp.InvoiceStatus = Status;
                opp.db.Entry(invoices_ListTemp).State = EntityState.Modified;

 
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {

                operations.DataDictionaryLogerException(ee);
                Msg = ee.Message;
                Excp = 1;
            }
        }
        public void EditInvoices_List(ref int Excp, ref string Msg, ref operations opp, Invoices_ListTemp invList)
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
        public void AddInvoices_Details(ref int Excp, ref string Msg, ref operations opp, Invoices_DetailsTemp invD)
        {
            try
            {
                opp.db.Invoices_DetailsTemp.Add(invD);
                opp.db.SaveChanges();
                Excp = 0;


            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }



        public void EditInvoices_Details(ref int Excp, ref string Msg, ref operations opp, Invoices_DetailsTemp invD)
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

        public void DeleteInvoices_Details(ref int Excp, ref string Msg, ref operations opp, Invoices_DetailsTemp invD)
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
            invList = new  Invoices_ListTemp ();
            operations opp = new operations();
            //My_DbContext.ConnStr = opp.str_sql_conn;
            //My_DbContext db = new My_DbContext();

            try
            {
                invList = opp.db.Invoices_ListTemp.AsNoTracking().Where(i => i.Id == Invoices_ListId).FirstOrDefault();
            }
            catch (Exception ee)
            {

            }
        }
        public Invoices_ListTemp Get_Invoices_ListTemp(ref operations opp, long Invoice_List_Table_Id)
        {
            
            try
            {
                return opp.db.Invoices_ListTemp.Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault();

            }
            catch (Exception ee)
            {
                return null;
            }
        }
        public IInvoices_List Get_IInvoices_ListTemp(ref operations opp, long Invoice_List_Table_Id)
        {

            try
            {
                Invoices_ListTemp inv = opp.db.Invoices_ListTemp.AsNoTracking().Where(i => i.Id == Invoice_List_Table_Id).FirstOrDefault();
                IInvoices_List IInvlst = inv as IInvoices_List;

                return IInvlst;
            }
            catch (Exception ee)
            {
                return null;
            }
        }
        public long Get_IInvoices_ListIDTemp(ref operations opp, long selectedCustomer_Id, long selectedTableId)
        {
            long Invoice_List_Table_Id = 0;
            try
            {
                IQueryable<Invoices_ListTemp> inv = opp.db.Invoices_ListTemp.AsNoTracking() ;
                if(selectedCustomer_Id > 0)
                {
                    inv = inv.Where(i => i.Customer_Id == selectedCustomer_Id);
                }
                if(selectedTableId > 0)
                {
                    inv = inv.Where(i => i.TableId == selectedTableId);
                }  

                var invList = inv.FirstOrDefault();
                if(invList != null)
                {
                      Invoice_List_Table_Id = invList.Id;
                }   
                return Invoice_List_Table_Id;
            }
            catch (Exception ee)
            {
                return Invoice_List_Table_Id;
            }
        }
        public List<Invoices_ListTemp> GetInvoices_ListLstByCustomer(long Customer_Id)
        {
           List<Invoices_ListTemp> invLst = new List<Invoices_ListTemp>();
            operations opp = new operations();
            //My_DbContext.ConnStr = opp.str_sql_conn;
            //My_DbContext db = new My_DbContext();

            try
            {
                invLst = opp.db.Invoices_ListTemp.AsNoTracking().Where(i => i.Customer_Id == Customer_Id).ToList();
            }
            catch (Exception ee)
            {

            }
            return invLst;  
        }
        public Invoices_ListTemp GetInvoices_ListById(ref operations opp, long Invoices_ListId)
        {
            invList = new  Invoices_ListTemp ();



            invList = opp.db.Invoices_ListTemp.Where(i => i.Id == Invoices_ListId).FirstOrDefault();
            return invList;

        }
        public Invoices_ListTemp GetInvoices_ListById(long Invoices_ListId)
        {
            invList = GetInvoices_ListById(ref opp, Invoices_ListId);
            return invList;

        }
        public List<Invoices_ListTemp> GetInvoices_ListByCarsWorkOrdersId(ref operations opp, long CarsWorkOrdersId)
        {
            List<Invoices_ListTemp> invsList = new List<Invoices_ListTemp>();

            invsList = opp.db.Invoices_ListTemp.Where(i => i.CarsWorkOrdersId == CarsWorkOrdersId).ToList();
            return invsList;

        }
        public List<Invoices_ListTemp> GetInvoices_ListByCarsWorkOrdersIdAsNoTracking(ref operations opp, long CarsWorkOrdersId)
        {
            List<Invoices_ListTemp> invsList = new List<Invoices_ListTemp>();

            invsList = opp.db.Invoices_ListTemp.AsNoTracking().Where(i => i.CarsWorkOrdersId == CarsWorkOrdersId).ToList();
            return invsList;

        }
        public List<Invoices_DetailsTemp> GetInvoices_Details(ref operations opp, long Invoices_ListId)
        {
            invD = new List<Invoices_DetailsTemp>();
            if(opp==null)
            {
                opp = new operations();
            }
            invD = opp.db.Invoices_DetailsTemp.Where(i => i.Invoice_Id == Invoices_ListId).ToList();
            return invD;

        }
        public List<Invoices_DetailsTemp> GetInvoices_DetailsToEdit(ref operations opp, long Invoices_ListId)
        {
            invD = new List<Invoices_DetailsTemp>();
            try
            {
                if (opp == null)
                {
                    opp = new operations();
                }
                //context.Configuration.ProxyCreationEnabled = false;
                opp.db.ChangeTracker.LazyLoadingEnabled = false;
                invD = opp.db.Invoices_DetailsTemp.AsNoTracking().Where(i => i.Invoice_Id == Invoices_ListId).ToList();
            }
            catch (Exception ee)
            {
                 
            }
            return invD;

        }
        public List<Invoices_DetailsTemp> GetInvoices_Details(ref operations opp, long Invoices_ListId, long Item_CategoryId)
        {
            invD = new List<Invoices_DetailsTemp>();

            invD = opp.db.Invoices_DetailsTemp.Where(i => i.Invoice_Id == Invoices_ListId & i.Items.Item_Category_ID == Item_CategoryId).Include(i => i.Items).Include(u => u.Unit).ToList();
            return invD;

        }

        internal List<Invoices_Details_Reverse> GetInvoices_Details_Reverse(ref operations opp, long invoice_List_Table_Id)
        {
            List<Invoices_Details_Reverse> invD = new  List<Invoices_Details_Reverse> ();

            invD = opp.db.Invoices_Details_Reverse .Where(i => i.Invoice_Id == invoice_List_Table_Id).ToList();
            return invD;
        }
    }
}
