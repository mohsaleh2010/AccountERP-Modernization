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


    public class ReceiverCls
    {
        public ReceiverCls()
        {
            
        }

        public Receiver GetReceiver(long CustomerId)
        {
            Customer cs=new Customer();

            CustomerCLS csCls = new CustomerCLS();
            cs=csCls.GetCustomer(CustomerId);
            Receiver receiver = new Receiver();
            receiver.id = cs.Tax_No;
            receiver.name = cs.TaxCompanyName;
            receiver.type = cs.LookUp_PersonCompany_Type;
            receiver.address = new Address
            { 
                country = cs.Gov_Country_ListName,
                governate = cs.Govern_ListName,
                regionCity = cs.Gov_City_ListName,
                street = cs.Street,
                buildingNumber = cs.BuildingNumber,
                postalCode = cs.PostalNo,
                floor = cs.floor,
                room = cs.room,
                landmark = cs.landmark,
                additionalInformation = cs.additionalInformation
            };
            return receiver;
        }
    }
    
}
