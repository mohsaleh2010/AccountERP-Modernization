using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// بيع المندوب من شاشة الديسك توب: واجهة عامة فوق نفس مسار الجهاز (SalesRepSyncService.BuildSale ثم SalesRepSaleService.SaveSale)،
    /// فلا منطق جديد: نفس بناء الفاتورة وفحوص المخزن والخصم والائتمان والموافقات وربط الوردية. الخدمة لا تكتب قيداً.
    /// </summary>
    public class SalesRepDeskSaleService
    {
        private readonly operations _external;
        public SalesRepDeskSaleService(operations external = null) { _external = external; }

        /// <summary>صنف على مخزن السيارة بسعره للعميل (نوع سعر العميل ثم نوع المندوب ثم السعر الافتراضى) ونسبة ضريبته والمتاح بالوحدة الصغرى.</summary>
        public class VanItem
        {
            public long Id { get; set; }
            public long Item_Id { get; set; }
            public string Item_Name { get; set; } = "";
            public string Code { get; set; } = "";
            public string BarCode2 { get; set; } = "";
            public string B_Unit { get; set; } = "";
            public string S_Unit { get; set; } = "";
            public decimal Factor { get; set; } = 1;
            public decimal Price_Large { get; set; }
            public decimal Tax_Rate { get; set; }
            public decimal Available_Small { get; set; }
        }

        public class SaleCheck
        {
            public decimal Net_Total { get; set; }
            public decimal Cash_On_Hand { get; set; }
            public SalesRepSaleService.CheckResult Result { get; set; } = new();
            public bool Ok => Result.Ok;
        }

        /// <summary>أصناف مخزن السيارة المطابقة للبحث (اسم/كود/باركود)؛ فارغ = أول 50. السعر حسب نوع سعر العميل إن وُجد.</summary>
        public List<VanItem> SearchItems(long repId, long customerId, string search)
        {
            var opp = _external ?? new operations();
            var ctx = SalesRepSaleService.Context(opp, repId);
            string t = (search ?? "").Trim().Replace("'", "''");
            string like = t.Length == 0 ? "" : $" AND (i.Item_Name LIKE N'%{t}%' OR CAST(i.code AS nvarchar(100)) = N'{t}' OR i.BarCode2 = N'{t}')";
            long customerType = customerId <= 0 ? 0 : opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(Price_Type_List_ID, 0) AS bigint) AS Value FROM Customer WHERE Id = {customerId}").AsEnumerable().FirstOrDefault();
            long priceType = customerType > 0 ? customerType : (ctx.Price_Type_List_ID ?? 0);
            return opp.db.Database.SqlQueryRaw<VanItem>($@"
SELECT TOP (50) u.id AS Id, CAST(ISNULL(u.Item_id, 0) AS bigint) AS Item_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(CAST(i.code AS nvarchar(100)), N'') AS Code,
       ISNULL(i.BarCode2, N'') AS BarCode2, ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(CASE WHEN {priceType} > 0 AND ISNULL(p.PriceUnit, 0) > 0 THEN p.PriceUnit
                 ELSE ISNULL(NULLIF(u.S_price_LargeUnit, 0), ISNULL(i.S_price_LargeUnit, 0)) END AS decimal(18,4)) AS Price_Large,
       CAST(ISNULL(i.TaxSales10_Value, 0) AS decimal(9,4)) AS Tax_Rate,
       CAST(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Available_Small
FROM Unit_item u JOIN Item i ON i.id = u.Item_id LEFT JOIN unit un ON un.id = u.unit_id
OUTER APPLY (SELECT TOP 1 v.PriceUnit FROM Price_Type_Sell_Values v WHERE v.Price_Type_List_ID = {priceType} AND v.Item_ID = u.Item_id) p
WHERE u.Store_id = {ctx.Van_Store_Id} AND i.Active_flage = 1{like}
ORDER BY i.Item_Name").ToList();
        }

        /// <summary>صافى سطر بالمحرك الموحد (نفس حساب شاشة الفاتورة والجهاز): الخصم قيمة لا تتجاوز قيمة السطر.</summary>
        public static decimal LineNet(decimal qtyLarge, decimal qtySmall, decimal priceLarge, decimal discount, decimal taxRate, decimal factor)
        {
            var l = new SalesRepOrderService.TakeOrderLine
            {
                Qty_Large = qtyLarge, Qty_Small = qtySmall, Unit_Price = priceLarge, Discount_Value = Math.Max(0, discount), Tax_Rate = Math.Max(0, Math.Min(100, taxRate))
            };
            SalesRepOrderService.RepriceLine(l, factor);
            return l.Line_Total;
        }

        /// <summary>يفحص البيع بلا حفظ: الصافى والمخالفات وحالة موافقاتها لهذا المستند. يرمى استثناء للمنع القاطع.</summary>
        public SaleCheck Check(long repId, Guid docGuid, SalesRepSyncService.DeviceSaleInput input)
        {
            var opp = _external ?? new operations();
            return Evaluate(opp, repId, docGuid, input, request: false);
        }

        /// <summary>يطلب موافقة المشرف لكل مخالفة لم تُغطَّ (بسبب اختيارى)، ثم يرجع الفحص.</summary>
        public SaleCheck RequestApprovals(long repId, Guid docGuid, SalesRepSyncService.DeviceSaleInput input, string reason = null) =>
            Tx(opp => Evaluate(opp, repId, docGuid, input, request: true, reason));

        /// <summary>يحفظ فاتورة المندوب فى معاملة واحدة (بناء الرأس والرقم والحفظ وربط الوردية). يرجع رقم الفاتورة (Invoices_List.Id).</summary>
        public long Save(long repId, Guid docGuid, SalesRepSyncService.DeviceSaleInput input) => Tx(opp =>
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            var (invL, invD) = SalesRepSyncService.BuildSale(opp, ctx, input);
            return new SalesRepSaleService(opp).SaveSale(repId, docGuid, invL, invD, paymentMethodId: input.Is_Credit ? 0 : input.Payment_Method_Id);
        });

        private static SaleCheck Evaluate(operations opp, long repId, Guid docGuid, SalesRepSyncService.DeviceSaleInput input, bool request, string reason = null)
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            var (invL, invD) = SalesRepSyncService.BuildSale(opp, ctx, input);
            var receipt = SalesRepPaymentService.Resolve(opp, ctx, input.Is_Credit ? 0 : input.Payment_Method_Id);
            var sale = new SalesRepSaleService.SaleInput
            {
                Doc_Guid = docGuid, Customer_Id = input.Customer_Id, Is_Credit = input.Is_Credit, Net_Total = Math.Round(invL.invL.Net_Total ?? 0, 2),
                Lines = SalesRepSaleService.LinesOf(opp, invL, invD), Payment_Method_Id = receipt.MethodId,
            };
            var svc = new SalesRepSaleService(opp);
            var result = request ? svc.RequestApprovals(repId, sale, reason) : svc.Check(repId, sale);
            if (!input.Is_Credit && receipt.IsCash) result.Warnings.AddRange(svc.CashWarnings(repId, sale.Net_Total));
            return new SaleCheck { Net_Total = sale.Net_Total, Cash_On_Hand = ctx.Cash_On_Hand, Result = result };
        }

        private T Tx<T>(Func<operations, T> f)
        {
            if (_external != null) return f(_external);
            var opp = new operations(2);
            try
            {
                var r = f(opp);
                opp.Commit();
                return r;
            }
            catch
            {
                try { opp.Rollback(); } catch { }
                throw;
            }
        }
    }
}
