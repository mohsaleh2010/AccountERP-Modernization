namespace AccountModelsLib
{
    using AccountModelsLib.Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    public class Lookup_OrderDeliveryTypesCLs
    {
        public Lookup_OrderDeliveryTypes accTaxCLS = new Lookup_OrderDeliveryTypes();
        public DataTable Get_Lookup_OrderDeliveryTypes(ref operations opp)
        {
            opp = new operations();
            string mysql = "Select * from Lookup_OrderDeliveryTypes Order By Name  ";
            return opp.return_DataTableWithBuilder(mysql);
        }
        public string Get_Lookup_OrderDeliveryTypesName(ref operations opp, long Lookup_OrderDeliveryTypesId)
        {
            opp = new operations();
            string Name = "";
            var order = opp.db.Lookup_OrderDeliveryTypes.AsNoTracking().Where(a => a.Id == Lookup_OrderDeliveryTypesId).FirstOrDefault();
            if (order != null)
            {

                Name = order.Name;

            }

            return Name;
        }
        public string Get_Lookup_OrderDeliveryTypesName( long Lookup_OrderDeliveryTypesId)
        {
              operations   opp = new operations();
            string Name = "";
            var order = opp.db.Lookup_OrderDeliveryTypes.AsNoTracking().Where(a => a.Id == Lookup_OrderDeliveryTypesId).FirstOrDefault();
            if (order != null)
            {

                Name = order.Name;

            }

            return Name;
        }

        public List<Lookup_OrderDeliveryTypes> Get_TaxInfoRatioList(ref operations opp)
        {
            List<Lookup_OrderDeliveryTypes> accTaxList = new List<Lookup_OrderDeliveryTypes>();
            accTaxList = opp.db.Lookup_OrderDeliveryTypes.AsNoTracking().ToList();
            return accTaxList;
        }

    }
}

