using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
   
 //using AccountModelsLib.DBModels;
namespace AccountModelsLib.Models
{
    public class ActiveOrdersFrmCLS
    {
        public ActiveOrdersFrmCLS()
        {
              
        }
         
        public DataTable Get_ActiveOrders(int Lookup_OrderDeliveryTypesId )
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return Get_ActiveOrders(ref opp, Lookup_OrderDeliveryTypesId );
        }
        public DataTable Get_ActiveOrders(ref AccountModelsLib.operations opp,int currentFilterType)
        {
            string mysql =  "";
              mysql = @"
                SELECT 
                    temp.Id  ,
                    temp.Invoice_Id   ,
                    temp.Lookup_OrderDeliveryTypesId, -- 1: صالة، 2: سفري، 3: توصيل
                    ISNULL(t.Name, '---') AS TableName,
                    ISNULL(e.name, '---') AS WaiterName,
                    temp.Invoice_DateTime, -- بافتراض وجود حقل وقت الإنشاء بالجدول
                    (ISNULL(temp.Qnty_Value, 0) + ISNULL(temp.Service_Amount, 0) + ISNULL(temp.TaxSales10_Value, 0) - ISNULL(temp.Net_Total_CommissionValue, 0)) AS Total_Amount,
                    temp.TableId
                FROM [dbo].[Invoices_ListTemp] temp
                LEFT OUTER JOIN [dbo].[Lookup_Tables] t ON temp.TableId = t.Id
                LEFT OUTER JOIN [dbo].[Employee] e ON temp.WaiterManId = e.id
                WHERE 1=1";
            //Invoices_List invoices_List = new Invoices_List();
            //invoices_List.Net_Total_CommissionValue
            // إضافة شرط التصفية ديناميكياً بناءً على الزر المضغوط
            if (currentFilterType > 0)
            {
                mysql += " AND temp.Lookup_OrderDeliveryTypesId = " + currentFilterType;
            }

            mysql += " ORDER BY temp.Id DESC";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            int c = dt.Rows.Count;
            return dt;
        }
 
    }
}
