using AccountModelsLib;
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using AccountModelsLib.Models;
using System.Collections.Generic;

using System.Linq;

namespace AccountModelsLib
{ 
     
    public class InvoiceStateService
    {
        // رأس الفاتورة
        public long SelectedCustomerId { get; set; }
        public Customer SelectedCustomer { get; set; }
        public int InvoiceTypeId { get; set; } = 1; // مبيعات / مشتريات
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public string InvoiceNotes { get; set; }

        // تفاصيل الفاتورة (الأصناف المضافة)
        public List<InvoiceDetailVm> InvoiceItems { get; set; } = new List<InvoiceDetailVm>();

        // حسابات الإجماليات
        public decimal TotalBeforeTax => InvoiceItems.Sum(i => i.TotalValue);
        public decimal TotalTax => InvoiceItems.Sum(i => i.TaxValue);
        public decimal NetTotal => TotalBeforeTax + TotalTax;

        public void Clear()
        {
            SelectedCustomerId = 0;
            SelectedCustomer = null;
            InvoiceItems.Clear();
            InvoiceNotes = string.Empty;
        }
    }

    // ViewModel مرن يدمج الصنف بالوحدة بناءً على ملفاتك
    public class InvoiceDetailVm
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public long UnitItemId { get; set; } // الربط مع Unit_item
        public decimal QuantityFull { get; set; } // Full_qua
        public decimal QuantityPart { get; set; } // D_Qua
        public decimal ConversionFactor { get; set; } // Small_unit
        public decimal Price { get; set; } // S_price أو P_price
        public decimal TaxRate { get; set; } // TaxSales10_Value

        public decimal TotalValue => ((QuantityFull * ConversionFactor) + QuantityPart) * Price;
        public decimal TaxValue => TotalValue * (TaxRate / 100);
    }
    public class SelectedItemEventArgs
    {
        public Items Item { get; set; }
        public decimal QtyFull { get; set; }
        public decimal QtyPart { get; set; }
        public decimal Price { get; set; }
    }
}
