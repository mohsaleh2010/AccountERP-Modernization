using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{

    [Table("ShipmentCompany")]
    public class ShipmentCompany
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        public string Name { get; set; }
        public long SubAccount_Id { get;  set; }

        //public string Phone { get; set; }

        //public string Notes { get; set; }

        // Add any other properties belonging to ShipmentCompany
    }
}