using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    public class CarsWorkOrdersItemProcessedEmployee
    {
        public long Id { get; set; }
        public long CarsWorkOrdersItemProcessedDetailsId { get; set; }
        public long CarsWorkOrdersItemReversedProcessedDetailsId { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        public long? CarsId { get; set; }
        public long? Emp_Id { get; set; }
        public string Emp_Name { get; set; }
        public decimal? Ratio { get; set; }
        public decimal? Value { get; set; }
        public string Notes { get; set; }

        [ForeignKey("Emp_Id")]
        public Employee Employee { get; set; }

        [ForeignKey("CarsWorkOrdersItemProcessedDetailsId")]
        public CarsWorkOrdersItemProcessedDetail carsWorkOrdersItemProcessedDetail { get; set; }

        [ForeignKey("CarsWorkOrdersId")]
        public CarsWorkOrdersItemReversedProcessedDetail carsWorkOrdersItemReversedProcessedDetail { get; set; }

        [ForeignKey("CarsWorkOrdersId")]
        public virtual CarsWorkOrder CarsWorkOrder { get; set; }

    }
}