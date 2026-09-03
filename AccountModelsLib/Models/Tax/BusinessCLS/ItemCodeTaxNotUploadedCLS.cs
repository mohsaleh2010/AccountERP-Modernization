using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{


    public class ItemCodeTaxNotUploadedCLS
    {
        operations opp = new operations();
        public ItemCodeTaxNotUploaded itemCodeTaxNotUploaded = new ItemCodeTaxNotUploaded();
        public List<ItemCodeTaxNotUploaded> itemCodeTaxNotUploadedList = new List<ItemCodeTaxNotUploaded>();

         public ItemCodeTaxNotUploadedCLS()
        {
            //activeFrom = new DateTime();
            //codeName = "";
        }

        public List<ItemCodeTaxNotUploaded> GetItemCodeTaxNotUploaded(bool IsShowAll)
        {
            if (IsShowAll == true)
            {
                itemCodeTaxNotUploadedList = opp.db.ItemCodeTaxNotUploaded.ToList();

            }
            else
            {
                itemCodeTaxNotUploadedList = opp.db.ItemCodeTaxNotUploaded.Where(x => x.IsUploaded == false).ToList();
            }

            
            return itemCodeTaxNotUploadedList;
        }
        public void SaveItemCodeTaxNotUploaded(ref int Excp,ref string Msg)
        {
            //MyDbContext.ConnStr = operations.conn_str_;
            //MyDbContext db = new MyDbContext();
            //operations opp = new operations();
            try
            {
                if (itemCodeTaxNotUploaded.Id == 0)
                {
                    opp.db.ItemCodeTaxNotUploaded.Add(itemCodeTaxNotUploaded);
                }
                else
                {
                    opp.db.Entry(itemCodeTaxNotUploaded).State = EntityState.Modified;
                }
                opp.db.SaveChanges();
                Excp= 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                 Msg=ee.Message;
            }
        }
        public void AddItemCodeTaxNotUploaded( ref operations opp)
        {
            //MyDbContext.ConnStr = operations.conn_str_;
            //MyDbContext db = new MyDbContext();
            //operations opp = new operations();
            
                if (itemCodeTaxNotUploaded.Id == 0)
                {
                    opp.db.ItemCodeTaxNotUploaded.Add(itemCodeTaxNotUploaded);
                }
                
                opp.db.SaveChanges();


        }
        public void EditItemCodeTaxNotUploaded(ref operations opp)
        {
            //MyDbContext.ConnStr = operations.conn_str_;
            //MyDbContext db = new MyDbContext();
            //operations opp = new operations();

            if (itemCodeTaxNotUploaded.Id > 0)
            {
                opp.db.Entry(itemCodeTaxNotUploaded).State = EntityState.Modified;
            }

            opp.db.SaveChanges();


        }
        public void DeleteItemCodeTaxNotUploaded(ref int Excp, ref string Msg, ItemCodeTaxNotUploaded itemCodeTax)
        {
            //MyDbContext.ConnStr = operations.conn_str_;
            //MyDbContext db = new MyDbContext();
            //operations opp = new operations();
            try
            {
                if (itemCodeTax.Id == 0)
                {
                    opp.db.ItemCodeTaxNotUploaded.Remove(itemCodeTax);
                }
                
                opp.db.SaveChanges();
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
