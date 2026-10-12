using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.SalesRep
{
    [Table("SalesRep_Routes")]
    public class SalesRep_Route
    {
        [Key]
        public long Id { get; set; }
        public long Rep_Id { get; set; }
        public string Name { get; set; }
        public long Branch_Id { get; set; }
        public List<SalesRep_Route_Customer> Customers { get; set; } = new List<SalesRep_Route_Customer>();
    }

    [Table("SalesRep_Route_Customers")]
    public class SalesRep_Route_Customer
    {
        [Key]
        public long Id { get; set; }
        public long Route_Id { get; set; }
        [ForeignKey("Route_Id")]
        public SalesRep_Route Route { get; set; }
        public long Customer_Id { get; set; }
        public int DayOfWeek { get; set; } // 0: Sun, 6: Sat
    }

    [Table("SalesRep_Sync_Inbox")]
    public class SalesRep_Sync_Inbox
    {
        [Key]
        public long Id { get; set; }
        public long Device_Id { get; set; }
        public int Doc_Type { get; set; }
        public long Doc_Id { get; set; }
        public string Payload { get; set; } // JSON
        public int Status { get; set; } // 0: Received, 1: Processed, 2: Error
        public DateTime Received_At { get; set; }
        public DateTime? Processed_At { get; set; }
        public string Error { get; set; }
    }

    [Table("SalesRep_Invoice_Return_Links")]
    public class SalesRep_Invoice_Return_Links
    {
        [Key]
        public long Id { get; set; }
        public long Return_Detail_Id { get; set; }
        public long Original_Detail_Id { get; set; }
        public long Original_Invoice_Id { get; set; }
        public decimal Qty_Large { get; set; }
        public int Condition { get; set; }
    }

    [Table("SalesRep_Targets")]
    public class SalesRep_Target
    {
        [Key]
        public long Id { get; set; }
        public long Rep_Id { get; set; }
        public int Scope { get; set; }
        public long Scope_Id { get; set; }
        public DateTime Period_From { get; set; }
        public DateTime Period_To { get; set; }
        public decimal Target_Value { get; set; }
        public decimal Achieved_Amount { get; set; }
    }

    [Table("SalesRep_Commission_Plans")]
    public class SalesRep_Commission_Plan
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Discount_Returns { get; set; }
        public List<SalesRep_Commission_Plans_Tier> Tiers { get; set; } = new List<SalesRep_Commission_Plans_Tier>();
    }

    [Table("SalesRep_Commission_Plans_Tiers")]
    public class SalesRep_Commission_Plans_Tier
    {
        [Key]
        public long Id { get; set; }
        public long Plan_Id { get; set; }
        [ForeignKey("Plan_Id")]
        public SalesRep_Commission_Plan Plan { get; set; }
        public decimal Target_Percent_From { get; set; }
        public decimal Target_Percent_To { get; set; }
        public decimal Commission_Rate { get; set; }
    }
}
