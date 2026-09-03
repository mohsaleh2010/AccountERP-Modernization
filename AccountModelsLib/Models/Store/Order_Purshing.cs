using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [Table("Order_Purshing")]
    public class Order_Purshing
    {
        [Key]
        public long Id { get; set; }
        public long Order_Purshing_No { get; set; }
        public DateTime? Order_date { get; set; }
        public int? Order_Type { get; set; }
        public int? Transaction_Type { get; set; }
        public string OrderTitle { get; set; }
        public int? type_From { get; set; }
        public long Id_From { get; set; }
        public int? Type_To { get; set; }
        public long Id_To { get; set; }

        public long Cust_Supplier_id { get; set; }
        public int? endd { get; set; }
        public int? end_order { get; set; }
        public int? Branch_ID { get; set; }
        public int? User_ID { get; set; }
        public int? Is_Converted { get; set; }
        public long MainOrderTable_id { get; set; }
        public int? IsActiveMainOrder_Purshing { get; set; }

        public string Id_FromStr { get; set; }
        public string Id_ToStr { get; set; }
        public string NotesAll { get; set; }
        public DateTime? OperationDate { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }
        public long? CarsWorkOrdersId { get; set; }
        public string WorkOrderNumber { get; set; }
        public string CarNumber { get; set; }
        public long? CarsId { get; set; }

        public bool? IS_EstlamRawMaterial { get; set; }
        public int? IsMoreTheOneOrderShow { get; set; }

        public long? Invoice_ID { get; set; }
        public string Invoice_No { get; set; }
        public long? EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        [ForeignKey("Id_From")]
        public Store StoreFrom { get; set; }
        [ForeignKey("Id_To")]
        public Store StoreTo { get; set; }

    }
}
