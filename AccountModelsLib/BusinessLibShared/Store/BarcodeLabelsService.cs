using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// أصناف طباعة ملصقات الباركود (خطة 05 بند 25) لصفحة /items/barcode-labels: بديل PrintBarCodeForOneItems
    /// وPrintBarCodeForOneItemsWithPrice وBill_PrintItemsBarCodeItemsList. قراءة فقط: بحث صنف، أو تحميل أصناف مخزن/فئة
    /// مع رصيدها بالوحدة الكبرى (لطباعة ملصق لكل قطعة). سعر البيع من Unit_item للمخزن المختار واحتياطياً Item.S_Price.
    /// </summary>
    public class BarcodeLabelsService
    {
        private readonly Func<operations> _newOpp;

        public BarcodeLabelsService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class LabelItem
        {
            public long Item_Id { get; set; }
            public string Code { get; set; }
            public string BarCode2 { get; set; }
            public string BarCode3 { get; set; }
            public string Item_Name { get; set; }
            public string Name_On_Invoice { get; set; }
            public string English_Name { get; set; }
            public decimal Sale_Price { get; set; }
            public decimal Balance_Large { get; set; }
        }

        private const string Columns = @"
    i.id AS Item_Id, ISNULL(i.code, N'') AS Code, ISNULL(i.BarCode2, N'') AS BarCode2, ISNULL(i.BarCode3, N'') AS BarCode3,
    ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(i.ItemName_OnInvoice, N'') AS Name_On_Invoice, ISNULL(i.AnotherName, N'') AS English_Name";

        /// <summary>بحث بالاسم أو الكود أو الباركود (أول 30).</summary>
        public List<LabelItem> Search(string text)
        {
            string t = (text ?? "").Trim();
            string like = "%" + t + "%";
            return Run(opp => opp.db.Database.SqlQuery<LabelItem>(System.Runtime.CompilerServices.FormattableStringFactory.Create($@"
SELECT TOP 30 {Columns},
    CAST(ISNULL(i.S_Price, 0) AS decimal(18,4)) AS Sale_Price, CAST(0 AS decimal(18,4)) AS Balance_Large
FROM Item i
WHERE ISNULL(i.Active_flage, 1) = 1
  AND ({{0}} = N'' OR i.Item_Name LIKE {{1}} OR i.code = {{0}} OR i.BarCode2 = {{0}} OR i.BarCode3 = {{0}} OR i.AnotherName LIKE {{1}})
ORDER BY CASE WHEN i.code = {{0}} THEN 0 ELSE 1 END, i.Item_Name", t, like)).ToList());
        }

        /// <summary>
        /// أصناف مخزن و/أو فئة (صنف واحد لكل سطر مخزون؛ بلا مخزن = صف لكل صنف برصيد كل المخازن). onlyInStock يستبعد الرصيد الصفرى/السالب.
        /// </summary>
        public List<LabelItem> Load(long storeId, long categoryId, bool onlyInStock) => Run(opp => opp.db.Database.SqlQuery<LabelItem>(
            System.Runtime.CompilerServices.FormattableStringFactory.Create($@"
SELECT {Columns},
    CAST(COALESCE(NULLIF(MAX(CASE WHEN {{0}} <> 0 THEN u.S_price END), 0), i.S_Price, 0) AS decimal(18,4)) AS Sale_Price,
    CAST(ISNULL(SUM(ISNULL(u.Full_qua, 0) + ISNULL(u.D_Qua, 0) / CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END), 0) AS decimal(18,4)) AS Balance_Large
FROM Item i
INNER JOIN Unit_item u ON u.Item_id = i.id AND ISNULL(u.active_flage, 1) = 1
INNER JOIN Store s ON s.Id = u.Store_id AND ISNULL(s.LookUp_StoreOrderTypesId, 4) = 4
WHERE ISNULL(i.Active_flage, 1) = 1 AND ISNULL(i.Is_ItemWithoutBalance, 0) = 0
  AND ({{0}} = 0 OR u.Store_id = {{0}})
  AND ({{1}} = 0 OR i.Item_Category_ID = {{1}})
GROUP BY i.id, i.code, i.BarCode2, i.BarCode3, i.Item_Name, i.ItemName_OnInvoice, i.AnotherName, i.S_Price
HAVING {{2}} = 0 OR SUM(ISNULL(u.Full_qua, 0) + ISNULL(u.D_Qua, 0) / CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END) > 0
ORDER BY i.Item_Name", storeId, categoryId, onlyInStock ? 1 : 0)).ToList());

        private T Run<T>(Func<operations, T> f) => f(_newOpp());
    }
}
