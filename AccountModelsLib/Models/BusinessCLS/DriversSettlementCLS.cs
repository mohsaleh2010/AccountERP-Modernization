using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
   
 //using AccountModelsLib.DBModels;
namespace AccountModelsLib.Models
{
    public class DriversSettlementCLS
    {
        public DriversSettlementCLS()
        {
              
        }

        public DataTable Get_DriversSettlement(int Lookup_OrderDeliveryTypesId, long DeliveryMan_DriverId, int Is_Reconciled_With_ManDriver)
        {
            operations opp = new operations();
            return Get_DriversSettlement(ref opp, Lookup_OrderDeliveryTypesId, DeliveryMan_DriverId, Is_Reconciled_With_ManDriver);
        }
        public DataTable Get_DriversSettlement(ref operations opp,int Lookup_OrderDeliveryTypesId, long DeliveryMan_DriverId, int Is_Reconciled_With_ManDriver)
        {
            string mysql =  "";
            mysql = @"
                SELECT 
                    inv.Id  ,
                    inv.Invoice_Id  ,
                    inv.Invoice_DateTime,
                    inv.NetInvoiceTotal,  -- صافي الفاتورة بدون التوصيل أو شاملها حسب طريقة الحفظ الأصلية
                    inv.Delivery_Fee,
                    inv.IsDeliveryFeeAddedToInvoiceTotal -- الحقل الـ int الجديد (1 يعني مضاف، 0 يعني منفصل)
                FROM [dbo].[Invoices_List] inv Where Id >0 ";

            if (DeliveryMan_DriverId > 0)
            {
                mysql += " And inv.DeliveryMan_DriverId = " + DeliveryMan_DriverId;
            }
            if (Lookup_OrderDeliveryTypesId > 0)
            {
                mysql += " And inv.Lookup_OrderDeliveryTypesId = " + Lookup_OrderDeliveryTypesId;
            }
            if (Is_Reconciled_With_ManDriver > 0)
            {
                mysql += " And inv.Is_Reconciled_With_ManDriver = " + Is_Reconciled_With_ManDriver;
            }

             
            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            int c = dt.Rows.Count;
            return dt;
        }
 
    }
}
