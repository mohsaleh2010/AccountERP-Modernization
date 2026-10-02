using System.Data;

namespace AccountModelsLib.BusinessLibShared.Store
{
    // 🟢 قوائم الاختيار لحقول رأس الفاتورة (فاتورة المبيعات/المرتجع)، نسخة قراءة فقط من مصادر عناصر التحكم الأصلية:
    // employeeSellerUSR1 (كل الموظفين بالاسم)، وSalesMan_List.GetSalesMan (الموظفون بوظيفة مندوب مبيعات Common.job_list_Id_SalesMan والفعّالون)،
    // وShipmentCoUSR (جدول ShipmentCompany). تعيد Id وName.
    public class InvoiceHeaderLookupsCLS
    {
        public DataTable GetSellers()
        {
            return new AccountModelsLib.operations().return_DataTable("SELECT id AS Id, name AS Name FROM Employee ORDER BY name");
        }

        public DataTable GetSalesmen()
        {
            return new AccountModelsLib.operations().return_DataTable(
                "SELECT id AS Id, name AS Name FROM Employee WHERE jobsListId = " + AccountModelsLib.Common.job_list_Id_SalesMan + " AND active = 1 ORDER BY name");
        }

        public DataTable GetShipmentCompanies()
        {
            return new AccountModelsLib.operations().return_DataTable("SELECT Id, Name FROM ShipmentCompany ORDER BY Name");
        }
    }
}
