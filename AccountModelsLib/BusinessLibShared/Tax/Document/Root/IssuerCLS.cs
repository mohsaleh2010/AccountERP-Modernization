using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models.Tax
{


    public class IssuerCls
    {
        public operations opp;


        public IssuerCls()
        {


        }
        public IssuerCls(ref operations opp_)
        {
            opp = opp_;

        }

        public AccountModelsLib.Models.Tax.Issuer GetIssuer()
        {
            AccountModelsLib.Models.Tax.Issuer iss = new AccountModelsLib.Models.Tax.Issuer();
            iss.id = Common.CommonTaxConfig.TaxRegistrationNumber;
            iss.name = Common.CommonTaxConfig.IssuerName;
            iss.type = Common.CommonTaxConfig.IssuerType;
            iss.address = new Address
            {
                branchID = Common.CommonTaxConfig.branchID,
                country = Common.CommonTaxConfig.country,
                governate = Common.CommonTaxConfig.governate,
                regionCity = Common.CommonTaxConfig.regionCity,
                street = Common.CommonTaxConfig.street,
                buildingNumber = Common.CommonTaxConfig.buildingNumber,
                postalCode = Common.CommonTaxConfig.postalCode,
                floor = Common.CommonTaxConfig.floor,
                room = Common.CommonTaxConfig.room,
                landmark = Common.CommonTaxConfig.landmark,
                additionalInformation = Common.CommonTaxConfig.additionalInformation
            };
            return iss;
        }

    }
}
