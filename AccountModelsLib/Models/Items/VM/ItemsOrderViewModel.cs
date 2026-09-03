using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public  class ItemsOrderViewModel
    {
        public long Serial { get; set; }
        public long Id { get; set; }
        public long Item_Id { get; set; }
        public long Unit_Item_Id { get; set; }
        public string Item_Name { get; set; }
        public string AnotherName { get; set; }
        public string ItemName_OnInvoice { get; set; }
        public string Item_Category_Name { get; set; }
 

        public long? Item_Category_Id { get; set; }
        public long? Store_id { get; set; }

        public string Store_Name { get; set; }
        public Nullable<int> Store_Type { get; set; }
        public decimal?  qua { get; set; }
        public decimal? Part_Qua { get; set; }
        public decimal? Item_No { get; set; }
        public string Specification { get; set; }
        public string QnyStr { get; set; }
        public string Code { get; set; }
        public string ItemCode { get; set; }
        public long? Unit_Id { get; set; }

        public string UnitStr { get; set; }
        
        
        public string B_Unit { get; set; }

        public string S_unit { get; set; }
        public decimal? Upon_Request { get; set; }
        public decimal? No3eboa { get; set; }

        public decimal? S_Price { get; set; }

        public decimal? Small_unit { get; set; }
        public int? BlockNo { get; set; }
        public int? shelfNo { get; set; }
        public int? OrderOnshelfNo { get; set; }


        public int? Unit_Used_For_P_price_Default { get; set; }
        public int? Unit_Used_For_S_price_Default { get; set; }

        public long? CarsWorkOrdersId { get; set; }
        public long? CarsWorkOrdersRequestedDetailsId { get; set; }
        public long? CarsWorkOrdersDeliverDetailsId { get; set; }


        
       
        public string ItemServiceCategoryName { get; set; }
       
        public string GenderTypeDescription { get; set; }
        public int? GenderTypeId { get; set; }
        public int? TotalSessions { get; set; } = 1;
        
        public int ServiceDurationInMinutes { get; set; }
        public int IsCommissionEmployeeBased_True { get; set; }
        public int IsCommissionEmployeeBased { get; set; }
        public decimal CommissionEmployeeValue { get; set; }

        public long? ItemServiceCategoryId { get; set; }
        public int Is_ItemOrService { get;   set; }
        public string  ItemOrServiceStr { get;   set; }

        
    }
}
