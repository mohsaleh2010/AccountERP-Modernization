using Newtonsoft.Json;
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


    public class ReceiverCls
    {
        public ReceiverCls()
        {

        }

        public Receiver GetReceiver(long CustomerId)
        {
            Customer cs = new Customer();

            CustomerCLS csCls = new CustomerCLS();
            cs = csCls.GetCustomer(CustomerId);
            Receiver receiver = new Receiver();
            receiver.id = cs.Tax_No;
            receiver.name = cs.TaxCompanyName;
            receiver.type = cs.LookUp_PersonCompany_TypeCodeTax;//"B";// 

            if (Common.IsUseStaticDataForCustomerReceiver == 1)
            {
                receiver.address = new Address_Receiver
                {
                    country = "EG",
                    governate ="Cairo",
                    regionCity ="Cairo",
                    street = "1",
                    buildingNumber ="1",
                    postalCode = "1",
                    floor = "1",
                    room = "1",
                    landmark = "",
                    additionalInformation =  ""
                };
            }
            else
            {
                receiver.address = new Address_Receiver
                {
                    country = cs.Gov_Country_ListName ?? "EG",
                    governate = cs.Govern_ListName ?? "Cairo",
                    regionCity = cs.Gov_City_ListName ?? "Cairo",
                    street = cs.Street ?? "1",
                    buildingNumber = cs.BuildingNumber ?? "1",
                    postalCode = cs.PostalNo ?? "1",
                    floor = cs.floor ?? "1",
                    room = cs.room ?? "1",
                    landmark = cs.landmark ?? "1",
                    additionalInformation = cs.additionalInformation ?? "1"
                };
            }
            return receiver;
        }
    }

}
