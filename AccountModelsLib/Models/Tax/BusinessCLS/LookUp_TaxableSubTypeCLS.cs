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


    public class LookUp_TaxableSubTypeCLS
    {
        public operations opp;
        public LookUp_TaxableSubType taxableSubType = new LookUp_TaxableSubType();
        public List<LookUp_TaxableSubType> taxableSubTypeList = new List<LookUp_TaxableSubType>();

        public LookUp_TaxableSubTypeCLS()
        {
            opp = new operations();
            //activeFrom = new DateTime();
            //codeName = "";
        }
        public LookUp_TaxableSubTypeCLS(ref operations opp_)
        {
            opp = opp_;

        }
        public List<LookUp_TaxableSubType> GetLookUp_TaxableSubTypes()
        {

            taxableSubTypeList = opp.db.LookUp_TaxableSubTypes.ToList();


            return taxableSubTypeList;
        }
        public void GetLookUp_TaxableSubType(long Id)
        {

            taxableSubType = opp.db.LookUp_TaxableSubTypes.Where(c => c.Id == Id).FirstOrDefault();


        }
        public void SaveLookUp_TaxableType(ref int Excp, ref string Msg)
        {

            try
            {
                if (taxableSubType.Id == 0)
                {

                    opp.db.LookUp_TaxableSubTypes.Add(taxableSubType);
                }
                else
                {
                    opp.db.Entry(taxableSubType).State = EntityState.Modified;
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
                LookUp_TaxableSubType lookUp_TaxableType = opp.db.LookUp_TaxableSubTypes.Where(c => c.Id == Id).FirstOrDefault();
                if (lookUp_TaxableType != null)
                {
                    opp.db.LookUp_TaxableSubTypes.Remove(taxableSubType);

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

        public List<LookUp_TaxableSubType> GetlookUp_TaxableSubTypes(long LookUp_TaxableTypesId)
        {
            IQueryable<LookUp_TaxableSubType> query = opp.db.LookUp_TaxableSubTypes;
            if (LookUp_TaxableTypesId > 0)
            {
                query = query.Where(i => i.LookUp_TaxableTypeId == LookUp_TaxableTypesId);
            }
            List<LookUp_TaxableSubType> item_Group = query.ToList();

            return item_Group;
        }
        public List<LookUp_TaxableSubType> GetlookUp_TaxableSubTypes(string TaxtypeReference)
        {
            IQueryable<LookUp_TaxableSubType> query = opp.db.LookUp_TaxableSubTypes;
            if (TaxtypeReference.Trim() != "")
            {
                query = query.Where(i => i.TaxtypeReference == TaxtypeReference);
            }
            List<LookUp_TaxableSubType> item_Group = query.ToList();

            return item_Group;
        }
    }
}
