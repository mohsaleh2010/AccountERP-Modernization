using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public class Car 
    {
       
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string  ChassisNumber { get; set; }
        public string  CarNumber { get; set; }
        public string  CarType { get; set; }
        public string  Model { get; set; }
        public string  EngineNumber { get; set; }
        public string  FuelType { get; set; }
        public string Capacity { get; set; }
        public string Year { get; set; }
        public string  TransmissionType { get; set; }
        public long?   Lookup_TransmissionTypeId { get; set; }
        public string  ColorCode { get; set; }
        public string  CarColor { get; set; }
        public string DriverTel { get; set; }
        public string DriverName { get; set; }
        public string CarAddress { get; set; }
        
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public string InsuranceCompany { get; set; }
        public string  Notes { get; set; }
        public long? Lookup_CarShapeId { get; set; }
         
        public long? Lookup_CarTypeId { get; set; }
        public long? Lookup_CarTypeModelId { get; set; }
        public int Active { get; set; } = 1;
        public int? IsChangeOwner { get; set; } = 0;
        public long?  OldId { get; set; } = 0;
        public long? NewId { get; set; } = 0;


        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [ForeignKey("Lookup_CarShapeId")]
        public Lookup_CarShape Lookup_CarShape { get; set; }

        [ForeignKey("Lookup_TransmissionTypeId")]
        public Lookup_TransmissionType Lookup_TransmissionType { get; set; }


        [ForeignKey("Lookup_CarTypeId")]
        public Lookup_CarType Lookup_CarType { get; set; }

        [ForeignKey("Lookup_CarTypeModelId")]
        public Lookup_CarTypeModel Lookup_CarTypeModel { get; set; }

    }
}
