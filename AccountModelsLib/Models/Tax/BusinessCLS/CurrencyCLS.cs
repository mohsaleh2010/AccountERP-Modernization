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


    public class CurrencyCLS
    {
        public AccountModelsLib.operations opp  ;
        public Currency curr = new Currency();
        public List<Currency> currList = new List<Currency>();

        public CurrencyCLS()
        {
            opp = new AccountModelsLib.operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public CurrencyCLS(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
             
        }
        public List<Currency> GetCurrency()
        {

            currList = opp.db.Currency.ToList();


            return currList;
        }
        public Currency GetCurrency(long Id)
        {

            curr = opp.db.Currency.Where(c => c.Id == Id).FirstOrDefault();
            return  curr;

        }
        public void SaveCurrency(ref int Excp, ref string Msg)
        {

            try
            {
                if (curr.Id == 0)
                {
                    var curr1 = opp.db.Currency
                   .OrderByDescending(p => p.Value)
                   .FirstOrDefault();
                    if (curr1 != null)
                    {
                        curr.Value = curr1.Value + 1;
                    }
                    else
                    {
                        curr.Value = 1;
                    }
                    opp.db.Currency.Add(curr);
                }
                else
                {
                    opp.db.Entry(curr).State = EntityState.Modified;
                }
                opp.db.SaveChanges();

                string mysql = "update Currency Set Value=Id";
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }
        public void DeleteCurrency(ref int Excp, ref string Msg, long Id)
        {
            try
            {
                Currency cur = opp.db.Currency.Where(c => c.Id == Id).FirstOrDefault();
                if (cur != null)
                {
                    opp.db.Currency.Remove(curr);

                    opp.db.SaveChanges();
                    Excp = 0;
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }


    }
}
