using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [NotMapped]
    public class CarsWorkOrdersViewModel: CarsWorkOrder
    {

        public long Serial_ { get; set; }

        public long Id { get; set; }
        //public long? WorkOrderNumber { get; set; }
        //public long CarsId { get; set; }
        public long? WarshaStoreId { get; set; }

        public string WarshaStoreName { get; set; }
        public string ChassisNumber { get; set; }
        public string CarNumber { get; set; }
        public string CarType { get; set; }
        public string Model { get; set; }
        public string EngineNumber { get; set; }
        public string FuelType { get; set; }
        public string Capacity { get; set; }
        public string Year { get; set; }
        public string TransmissionType { get; set; }
        public string ColorCode { get; set; }
        public string CarColor { get; set; }
        public string CarShapeName {  get; set; }
        //public long CustomerId { get; set; }
        public long CustomerSubAccount_Id { get; set; }
        public string CustomerName { get; set; }
        public string Customer_SubAccount_Name { get; set; }
        public string CustomerName_CarOwner { get; set; }
        public string CustomerName_RelatedToAccount { get; set; }

        //public string TaxInternalId { get; set; }

        //public long? TaxInvoiceNo { get; set; }
        //public int? IsUploadToPortal { get;   set; }
        //public decimal?  KhamatQnty_Value { get;   set; }
        public string Invoice_Item_Name { get;   set; }
        public string Item_Category_Name { get;   set; }
        public decimal? Qnty { get;   set; }
        public decimal? PriceUnit { get;   set; }
        public decimal? Item_Net_Total_Price { get;   set; }
        public string Specification { get;   set; }


        public long? Emp_Id { get; set; }
        public string  Emp_Name { get; set; }

        public string Status { get; set; }

        //public string CustomerSalesMan { get; set; }
        //public string Tel { get; set; }
        //public string Mobile { get; set; }
        //public string Address { get; set; }
        //public string CarMeterOnEntry { get; set; }
        //public DateTime? WorkOrdersDate { get; set; }

        //public DateTime? EntryDateTime { get; set; }
        //public DateTime? StartWorkDateTime { get; set; }
        //public DateTime? ExpectedEndWorkDate { get; set; }
        //public string CustomerComplaint { get; set; }
        //public string RequiredWorks { get; set; }
        //public long ReceptionEngineerId { get; set; }
        //public string ReceptionEngineerName { get; set; }


        //public long ReceptionManId { get; set; }
        //public string ReceptionManName { get; set; }

        //public DateTime? WorkOrderClosedDate { get; set; }
        //public int IsWorkOrderClosed { get; set; }

    }
}
