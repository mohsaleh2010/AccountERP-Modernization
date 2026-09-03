using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    [NotMapped]
    public  class CarsExaminationVM: CarsExamination
    {
        public string  Serial { get; set; }
        public string USER_NAME { get; set; }
        public string Name { get; set; }
        public long Customer_Id { get; set; }
         


    }
}
