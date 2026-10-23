
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{

    public class CarsWorkOrder
    {
        public long Id { get; set; }
        public long? WorkOrderNumber { get; set; }

        public long? StoreId { get; set; }
        public long CarsId { get; set; }
        public long CustomerId { get; set; }
        public long Customer_Id_RelatedToAccount { get; set; }
       

         
        public long CustomerId_CarOwner { get; set; }
        public int? IsInsuranceCompanyStaus { get; set; }

        
        public string? CustomerSalesMan { get; set; }
        public string? Tel { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? CarMeterOnEntry { get; set; }
        public DateTime? WorkOrdersDate { get; set; }
        public DateTime? EntryDateTime { get; set; }
        public DateTime? EntryDateOnly { get; set; }
        public TimeSpan? EntryTimeOnly { get; set; }
        public DateTime? StartWorkDateTime { get; set; }
        public DateTime? ExpectedEndWorkDate { get; set; }
        public string? CustomerComplaint { get; set; }
        public string? RequiredWorks { get; set; }
        public string? ExaminationReport { get; set; }


        public long ReceptionEngineerId { get; set; }
        public string? ReceptionEngineerName { get; set; }


        public long ReceptionManId { get; set; }

        public string? ReceptionManName { get; set; }
        public long? Last_Invoice_ListId { get; set; }
        public long? Lookup_CarEntryTaskeenId { get; set; }
        public string? Lookup_CarEntryTaskeenName { get; set; }
        public long? LookUp_CarStatusLocationId { get; set; }
        public string? LookUp_CarStatusLocationName { get; set; }
        public DateTime? WorkOrderClosedDate { get; set; }
        public int? IsWorkOrderClosed { get; set; }
        public int? IsInvoiceTaxInfo { get; set; }

        
        public string?  WorkOrderClosed_Reason { get; set; }
        public decimal? KhamatQnty_Value { get; set; }
        public string?  CustomerOrderNumberPO { get; set; }
        public string? CustomerOrderNumberGR { get; set; }

        
        public string? TaxInternalId { get; set; }

        public long? Last_Invoices_List_ReverseId { get; set; }
        public string? TaxReverseInternalId { get; set; }
        public long? TaxReverseInvoiceNo { get; set; }
        public string? Repairs_NotDone { get; set; }

        public int? IsUploadToPortal { get; set; }
        public long? TaxInvoiceNo { get; set; }
        public long? User_Id { get; set; }
        public long? Branch_Id { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [ForeignKey("CustomerId_CarOwner")]
        public Customer Customer_CarOwner { get; set; }

        [ForeignKey("Customer_Id_RelatedToAccount")]
        public Customer Customer_RelatedToAccount { get; set; }
        [ForeignKey("CarsId")]
        public Car Car { get; set; }

        [ForeignKey("StoreId")]
        public Store Store { get; set; }

        public virtual ICollection<CarsWorkOrdersRequestedDetail> carsWorkOrdersDetails { get; set; }
        public virtual ICollection<CarsWorkOrdersItemProcessedDetail> CarsWorkOrdersItemProcessedDetail { get; set; }
        




        //public  CarsWorkOrdersDetail  carsWorkOrdersDetails { get; set; }
    }
}
