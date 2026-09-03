using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class CarsExamination
    {
        public long Id { get; set; }
        public string Paint_Report { get; set; }
        public string Mechanics_Report { get; set; }
        public string Suspension_Report { get; set; }
        public string ExaminationNotes { get; set; }
        public string CarNumber { get; set; }
        public string ChassisNumber { get; set; }
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public long? CarsId { get; set; }
        public long ExaminationNumber { get; set; }
        public string CarMeterOnEntry { get; set; }

         
        public string Year { get; set; }
        public string CarType { get; set; }
        
        public string Model { get; set; }
        public long Lookup_CarTypeId { get; set; } =  0;    
        public long Lookup_CarTypeModelId { get; set; } = 0;
        public DateTime? ExaminationDate { get; set; }
        public long User_Id { get; set; }
        public long Branch_Id { get; set; }
        public long? Order_MoneyId { get; set; }


        public int? IsCarsExaminationClosed { get; set; }
        public DateTime? CarsExaminationClosedDate { get; set; }
        public string CarsExaminationStatus { get; set; }
        public decimal? CarsExaminationValue { get; set; }


        public int  Transaction_Type { get; set; }
        public long  SubAccount_Id { get; set; }
        public string Notes { get; set; }
        public long  Currency_Id { get; set; }
        public long ProcessNo { get; set; }

        
    }
}
