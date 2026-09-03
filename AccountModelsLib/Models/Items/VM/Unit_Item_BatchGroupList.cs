using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [NotMapped]
    public class Unit_Item_BatchGroupListVM: Unit_Item_BatchGroupList
    {

        public string Supplier_Name { get; set; }


       
    }
}

