
 

namespace AccountModelsLib.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("SysPrinterSettings")]
    public class SysPrinterSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; } = 1; // ﬁÌ„… À«» … ·√‰Â« ≈⁄œ«œ«  „ÊÕœ… ··„Õÿ…

        [Required]
        [StringLength(250)]
        public string InvoicePrinterName { get; set; } = "POS 80";

        [Required]
        [StringLength(250)]
        public string KitchenPrinterName { get; set; } = "POS 80";


        [Required]
        [StringLength(250)]
        public string BarCodePrinterName { get; set; } = "POS 80";

        
        public int PaperWidth { get; set; } = 290;
        public int InvoiceWidth { get; set; } = 270;
        public float MarginLeft { get; set; } = 10f;
        public float StartHeightY { get; set; } = 10f;
        public bool ShowPreview { get; set; } = false;
        public bool PrintKitchenSlip { get; set; } = false;
       
        public string FontHeader { get; set; } = " ";

    }
}
 