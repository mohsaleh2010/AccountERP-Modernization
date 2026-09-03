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


    public class LookUp_TaxableTypeCLS
    {
        public operations opp  ;
        public LookUp_TaxableType taxableType = new LookUp_TaxableType();
        public List<LookUp_TaxableType> taxableTypeList = new List<LookUp_TaxableType>();

        public LookUp_TaxableTypeCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public LookUp_TaxableTypeCLS(ref operations opp_)
        {
            opp = opp_;
             
        }
        public List<LookUp_TaxableType> GetLookUp_TaxableTypes()
        {
            
            taxableTypeList = opp.db.LookUp_TaxableTypes.ToList();


            return taxableTypeList;
        }
        public void GetLookUp_TaxableType(long Id)
        {

            taxableType = opp.db.LookUp_TaxableTypes.Where(c => c.Id == Id).FirstOrDefault();


        }
        public void SaveLookUp_TaxableType(ref int Excp, ref string Msg)
        {

            try
            {
                if (taxableType.Id == 0)
                {
                    
                    opp.db.LookUp_TaxableTypes.Add(taxableType);
                }
                else
                {
                    opp.db.Entry(taxableType).State = EntityState.Modified;
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
        public void DeleteLookUp_TaxableType(ref int Excp, ref string Msg, long Id)
        {
            try
            {
                LookUp_TaxableType lookUp_TaxableType = opp.db.LookUp_TaxableTypes.Where(c => c.Id == Id).FirstOrDefault();
                if (lookUp_TaxableType != null)
                {
                    opp.db.LookUp_TaxableTypes.Remove(taxableType);

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
