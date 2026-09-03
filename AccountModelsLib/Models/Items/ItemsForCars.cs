using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class ItemsForCars
    {
        public long Id { get; set; }
        public long Item_Id { get; set; }
        public long Lookup_CarTypeId { get; set; }
        public long Lookup_CarTypeModeLId { get; set; }
        public int IsMain { get; set; }

        //[ForeignKey("Item_Id")]
        //public List<Items> Items { get; set; }

        //[ForeignKey("Lookup_CarTypeId")]
        //public List<Lookup_CarType> Lookup_CarType { get; set; }

        //[ForeignKey("Lookup_CarTypeModeLId")]
        //public List<Lookup_CarTypeModel> Lookup_CarTypeModel { get; set; }


        [ForeignKey("Item_Id")]
        public virtual Items Item { get; set; }

        [ForeignKey("Lookup_CarTypeId")]
        public virtual Lookup_CarType Lookup_CarType { get; set; }


        [ForeignKey("Lookup_CarTypeModeLId")]
        public virtual Lookup_CarTypeModel Lookup_CarTypeModel { get; set; }


    }

}
