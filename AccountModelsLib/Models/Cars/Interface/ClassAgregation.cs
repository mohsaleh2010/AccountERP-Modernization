using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class ClassAgregation
    {
        //public ClassItemDetail ItemProcessedList { get; set; } = new ClassItemDetail();
        public List<ClassItemDetail> ItemProcessedList { get; set; } = new  List<ClassItemDetail> ();
    }
}
