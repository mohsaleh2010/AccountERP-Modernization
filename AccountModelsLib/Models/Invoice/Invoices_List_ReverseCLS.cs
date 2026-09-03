using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountModelsLib.Models;
   

namespace AccountModelsLib.Models
{
    public class Invoices_List_ReverseCls
    {
        public Invoices_List_ReverseCls()
        {

        }
        public Invoices_List_Reverse invLR = new Invoices_List_Reverse();
       
        //public List<Invoices_Details_Reverse> invDetails { get; set; }
    
        public List<Invoices_Details_Reverse> invDR_Lst = new List<Invoices_Details_Reverse>();
        public Invoices_Details_Reverse invDNewEdit { get; set; } = new   Invoices_Details_Reverse  ();
        public void AddInvoices_List_Reverse(ref int Excp,ref string Msg,ref operations opp, Invoices_List_Reverse invList)
        {
            try
            {
                long v = DataDictionaryProcessesNoCLS.GetProcessesNo(ref Excp,ref Msg, ref   opp, invList);
                opp.db.Invoices_List_Reverse.Add(invList);
                opp.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                invList.Id = 0;
                operations.DataDictionaryLogerException(ee);
                Msg=ee.Message;
                Excp = 1;
            }
        }

        public void DeleteInvoices_List_Reverse(ref int Excp, ref string Msg, ref operations opp, Invoices_List_Reverse invList)
        {
            try
            {
                opp.db.Invoices_List_Reverse.Remove(invList);
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
        public void EditInvoices_List_Reverse(ref int Excp, ref string Msg, ref operations opp, Invoices_List_Reverse invList)
        {
            try
            {
                opp.db.Entry(invList).State=EntityState.Modified;
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
        public void AddInvoices_Details_Reverse(ref int Excp, ref string Msg, ref operations opp, Invoices_Details_Reverse invD)
        {
            try
            {
                opp.db.Invoices_Details_Reverse.Add(invD);
                opp.db.SaveChanges();
                Excp = 0;

              
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }

       

        public void EditInvoices_Details_Reverse(ref int Excp, ref string Msg, ref operations opp, Invoices_Details_Reverse invD)
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
        public void Update_Invoices_List_Reverse(ref int Excp, ref operations opp,  Invoices_List_Reverse invL_R)
        {
            Invoices_List_Reverse invL_F = opp.db.Invoices_List_Reverse.Find(invL_R.Id);
            try
            {
                if (invL_F != null)
                {
                    opp.db.Entry(invL_F).CurrentValues.SetValues(invL_R);
                    opp.db.SaveChanges();
                    Excp = 0;
                }
            }
            catch (Exception)
            {
                Excp = 1;
            }

             
        }
        public void DeleteInvoices_Details_Reverse(ref int Excp, ref string Msg, ref operations opp, Invoices_Details_Reverse invD)
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

        public void GetInvoices_List_Reverse(long Invoices_List_ReverseId)
        {
            invLR = new Invoices_List_Reverse();
            operations opp = new operations();
            //My_DbContext.ConnStr = opp.str_sql_conn;
            //My_DbContext db = new My_DbContext();

            try
            {
                invLR = opp.db.Invoices_List_Reverse.Where(i => i.Id == Invoices_List_ReverseId).FirstOrDefault();
            }
            catch (Exception ee)
            {

            }
        }
        public Invoices_List_Reverse GetInvoices_List_ReverseById(ref operations opp, long Invoices_List_ReverseId)
        {
            invLR = new Invoices_List_Reverse();
            

            
                invLR = opp.db.Invoices_List_Reverse.Where(i => i.Id == Invoices_List_ReverseId).FirstOrDefault();
                return invLR;
             
        }
        public List<Invoices_List_Reverse> GetInvoices_List_ReverseByCarsWorkOrdersId(ref operations opp, long CarsWorkOrdersId)
        {
            List <Invoices_List_Reverse> invsList = new   List<Invoices_List_Reverse>();

            invsList = opp.db.Invoices_List_Reverse.Where(i => i.CarsWorkOrdersId == CarsWorkOrdersId).ToList();
            return invsList;

        }

        public List<Invoices_Details_Reverse> GetInvoices_Details_Reverse(ref operations opp, long Invoices_List_ReverseId)
        {
            invDR_Lst = new  List<Invoices_Details_Reverse>   ();
            
            invDR_Lst = opp.db.Invoices_Details_Reverse.Where(i => i.Invoice_Id == Invoices_List_ReverseId).ToList();
            return invDR_Lst;

        }

        public List<Invoices_Details_Reverse> GetInvoices_Details_Reverse(ref operations opp, long Invoices_List_ReverseId,long Item_CategoryId)
        {
            invDR_Lst = new List<Invoices_Details_Reverse>();

            invDR_Lst = opp.db.Invoices_Details_Reverse.Where(i => i.Invoice_Id == Invoices_List_ReverseId & i.Items.Item_Category_ID== Item_CategoryId).Include(i => i.Items).Include(u => u.Unit).ToList();
            return invDR_Lst;

        }
    }
}
