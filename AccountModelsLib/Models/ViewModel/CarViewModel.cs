using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [NotMapped]
    public class CarViewModel : Car
    {

        public int Serial { get; set; }
        public string CustomerName { get; set; }
        public string CarShapeName { get; set; }
    }
}
