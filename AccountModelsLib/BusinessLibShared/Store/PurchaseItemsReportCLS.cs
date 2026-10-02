using System;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.Store
{
    // 🟢 نسخة قراءة فقط من Store_LIB.SalesReportsCLS(3).GetPurchaseDetails / GetPurchaseSumDetails (تقرير 903 «بيان المشتريات»).
    // Store_LIB مشروع WinForms غير مرجَع من مشروع Razor، فنُقلت الاستعلامات هنا (بلا MessageBox، والخطأ يُرمى للمستدعى) دون المساس بكود سطح المكتب.
    // الفروق عن الأصل: «إلى تاريخ» يشمل اليوم كاملاً (Invoice_Date < اليوم التالى) لأن الأصل (<= منتصف الليل) كان يستبعد فواتير ذلك اليوم المخزَّنة بوقت؛ وتُحدَّد أعمدة النتيجة صراحةً بدل Bills_Details.* + Bills_List.* (الأعمدة المتكررة الأسماء كانت تُعاد تسميتها تلقائياً)،
    // وكل القيم الرقمية أرقام حقيقية (لا نصوص من المستخدم فى SQL).
    public class PurchaseItemsReportCLS
    {
        private const string Joins =
            " FROM Bills_Details INNER JOIN Bills_List ON Bills_Details.Invoice_Id = Bills_List.ID " +
            "INNER JOIN Unit_item INNER JOIN unit ON Unit_item.unit_id = unit.id " +
            "INNER JOIN Item ON Unit_item.Item_id = Item.id ON Bills_Details.Unit_Item_Table_Id = Unit_item.id " +
            "INNER JOIN Supplier ON Bills_List.Supplier_Id = Supplier.Id ";

        // يقابل Stores_Items_List.ItemsList_Data_sales(true, categoryId): الأصناف الفعّالة مرتبة بالاسم، والفئة -1 = كل الفئات.
        public DataTable GetItems(long itemCategoryId)
        {
            var opp = new AccountModelsLib.operations();
            var sql = "SELECT Item.Id, Item.Item_Name FROM Item INNER JOIN Store ON Item.S_id = Store.id WHERE (Item.Active_flage = 1) ";
            if (itemCategoryId != -1) sql += " AND (Item.Item_Category_ID = " + itemCategoryId + ") ";
            sql += " ORDER BY Item.Item_Name ";
            return opp.return_DataTable(sql);
        }

        // يقابل GetPurchaseDetails: سطر لكل صنف بكل فاتورة مشتريات. isInvoiceAgile: -1 الكل، 0 نقدى، 1 آجل.
        public DataTable GetDetails(long supplierId, bool useFrom, DateTime from, bool useTo, DateTime to, long itemId, long invoiceNo, int isInvoiceAgile)
        {
            var opp = new AccountModelsLib.operations();
            var sql =
                "SELECT Item.Item_Name, unit.B_Unit, unit.S_unit, unit.S_unit_Value, Supplier.Name, " +
                "Bills_Details.Larg_Qnty, Bills_Details.Small_Qnty, Bills_Details.Qnty_Another, Bills_Details.Qnty_value, Bills_Details.Larg_Unit_Price, " +
                "Bills_Details.Item_CommissionValue, Bills_Details.Unit_CommissionValue, Bills_Details.Invoice_Id, " +
                "Bills_List.Invoice_Id AS Invoice_Id_No, Bills_List.Invoice_Date, Bills_List.Is_Invoice_Agile " +
                Joins + " WHERE (Bills_List.id > 0) ";
            if (supplierId > 0) sql += " AND (Bills_List.Supplier_Id = " + supplierId + ") ";
            if (useFrom) sql += " AND (Bills_List.Invoice_Date >= '" + AccountModelsLib.operations.dateFormat(from) + "') ";
            if (useTo) sql += " AND (Bills_List.Invoice_Date < '" + AccountModelsLib.operations.dateFormat(to.Date.AddDays(1)) + "') ";
            if (itemId > 0) sql += " AND (Bills_Details.Item_Id = " + itemId + ") ";
            if (invoiceNo > 0) sql += " AND (Bills_List.Invoice_Id = " + invoiceNo + ") ";
            if (isInvoiceAgile > -1) sql += " AND (Bills_List.Is_Invoice_Agile = " + isInvoiceAgile + ") ";
            sql += " ORDER BY Bills_List.ID ";
            return opp.return_DataTable(sql);
        }

        // يقابل GetPurchaseSumDetails: إجماليات لكل (صنف، مورد، وحدة) بالفترة (الأصل يطبّق التاريخين دائماً)، ويستبعد ما إجماليه <= minTotal.
        public DataTable GetSums(long supplierId, DateTime from, DateTime to, long itemId, decimal minTotal, int isInvoiceAgile)
        {
            var opp = new AccountModelsLib.operations();
            var sql =
                "SELECT SUM(Bills_Details.Item_CommissionValue) AS Item_CommissionValue, SUM(Bills_Details.Unit_CommissionValue) AS Unit_CommissionValue, " +
                "SUM(Bills_Details.Larg_Qnty) AS Larg_Qnty, SUM(Bills_Details.Small_Qnty) AS Small_Qnty, SUM(Bills_Details.Qnty_Another) AS Qnty_Another, " +
                "SUM(Bills_Details.Qnty_value) AS Qnty_value, Item.Item_Name, Supplier.Name, unit.B_Unit, unit.S_unit, unit.S_unit_Value " +
                Joins + " WHERE (Bills_List.id > 0) ";
            if (supplierId > 0) sql += " AND (Bills_List.Supplier_Id = " + supplierId + ") ";
            sql += " AND (Bills_List.Invoice_Date >= '" + AccountModelsLib.operations.dateFormat(from) + "') AND (Bills_List.Invoice_Date < '" + AccountModelsLib.operations.dateFormat(to.Date.AddDays(1)) + "') ";
            if (itemId > 0) sql += " AND (Bills_Details.Item_Id = " + itemId + ") ";
            if (isInvoiceAgile > -1) sql += " AND (Bills_List.Is_Invoice_Agile = " + isInvoiceAgile + ") ";
            sql += " GROUP BY Item.Item_Name, Supplier.Name, unit.B_Unit, unit.S_unit, unit.S_unit_Value ";
            if (isInvoiceAgile > -1) sql += ", Bills_List.Is_Invoice_Agile ";
            sql += " HAVING (SUM(Bills_Details.Qnty_value) > " + minTotal.ToString(System.Globalization.CultureInfo.InvariantCulture) + ") ";
            return opp.return_DataTable(sql);
        }
    }
}
