using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AccountModelsLib.Models
{


    public class ItemCodeToSendPortal
    {

        public string codeType { get; set; } = "EGS";

        public string parentCode { get; set; }
        public string itemCode { get; set; } = "EG-123456789-123456784";
        public string codeName { get; set; }
        public string codeNameAr { get; set; }
        public string activeFrom { get; set; }
        public DateTime? activeTo { get; set; } = default;


        public string description { get; set; }
        public string descriptionAr { get; set; }

        public string requestReason { get; set; } = "test";
        public string linkedCode { get; set; }

    }
  



}