using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
   
 //using AccountModelsLib.DBModels;
namespace AccountModelsLib.Models
{
    public class Lookup_TablesCLS
    {
        public Lookup_TablesCLS()
        {

        }
        public Lookup_Table  _Table = new   Lookup_Table  ();
        public DataTable Get_Lookup_Table(int Lookup_OrderDeliveryTypesId, int Id = 0)
        {
            operations opp = new operations();
            return Get_TagerKontra(ref opp, Lookup_OrderDeliveryTypesId, Id);
        }
        public DataTable Get_TagerKontra(ref operations opp,int Lookup_OrderDeliveryTypesId, int Id = 0)
        {
            string mysql =  "";
           
              mysql = @"
               SELECT        t.Id, t.Name, [temp].NetInvoiceTotal,
                             (SELECT        t.Name
                                FROM            Employee AS w
                                WHERE        (t.Id = [temp].WaiterManId)) AS WaiterName
FROM            Lookup_Tables AS t LEFT OUTER JOIN
                         Invoices_ListTemp AS [temp] ON t.Id = [temp].TableId
WHERE        (t.Is_Active = 1)
ORDER BY t.Id";

            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }

            mysql = "";
            mysql = @" SELECT        t.Id, t.Name, [temp].NetInvoiceTotal, [temp].Invoice_Id, Employee.name AS WaiterName, [temp].Lookup_OrderDeliveryTypesId
FROM            Employee RIGHT OUTER JOIN
                         Invoices_ListTemp AS [temp] ON Employee.id = [temp].WaiterManId RIGHT OUTER JOIN
                         Lookup_Tables AS t ON [temp].TableId = t.Id
WHERE        (t.Is_Active = 1) 
";
            if (Lookup_OrderDeliveryTypesId > 0)
            {
                mysql = mysql + " AND ([temp].Lookup_OrderDeliveryTypesId = " + Lookup_OrderDeliveryTypesId + ") ";
            }
            mysql = mysql + " ORDER BY t.Id  ";
            mysql = mysql + "  ";
            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            int c = dt.Rows.Count;
            return dt;
        }
 
    }
}
