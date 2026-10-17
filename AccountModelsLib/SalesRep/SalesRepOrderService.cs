using System;
using System.Collections.Generic;
using System.Linq;
using AccountERP.SharedUI.Services;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 5: النمط المختلط مع طلبيات العملاء (§6.4-6.5، قرارات المستخدم 2026-10-16).
    /// - الإسناد: طلبية مؤكدة بلا تنفيذ تُسنَد لمندوب؛ أسطرها تنتقل لأصناف مخزن السيارة (EnsureStoreLine) فيصير حجزها على السيارة بنفس
    ///   منطق الطلبيات (المطلوب − المنفَّذ − الملغى)، والأصل يُحفظ فى SalesRep_Order_Assignment_Lines لإلغاء الإسناد قبل أى تنفيذ.
    ///   الطلبية المسندة لا تُعدَّل ولا تُلغى من شاشة الطلبيات حتى يُلغى الإسناد.
    /// - اقتراح التحميل: المحجوز للطلبيات المسندة − رصيد السيارة، من سطر المخزن الأصلى.
    /// - التسليم بزر واحد: الفاتورة تُبنى على السيرفر من أسطر الطلبية بسعرها المتفق عليه (الخصم والضريبة بنسبة السطر)، وتُحفظ بمسار
    ///   بيع المندوب (المتاح والائتمان والموافقات)، ثم ربط التنفيذ وتسوية العربون تلقائياً والتحصيل النقدى للباقى — كلها فى معاملة واحدة.
    /// - المتبقى (رفض كلى/جزئى): سبب إلزامى والمندوب يختار: إعادة المحاولة (يبقى محجوزاً) أو إغلاق المتبقى (يعود متاحاً للبيع الفورى).
    /// - أخذ طلبية من المندوب: من مخزن يختاره (الافتراضى آخر مخزن حُمِّل منه)، بنفس فحوص البيع الفورى (الخصم وحد الائتمان بموافقة المشرف)،
    ///   وتُحفظ مؤكدة وتُسنَد له فوراً، والعربون الاختيارى إذن استلام على صندوق العهدة مربوط بالطلبية والوردية — كلها فى معاملة واحدة.
    /// </summary>
    public class SalesRepOrderService
    {
        private readonly operations _external;
        public SalesRepOrderService(operations external = null) { _external = external; }

        public class OrderRow
        {
            public long Id { get; set; }
            public long Order_No { get; set; }
            public DateTime? Order_Date { get; set; }
            public DateTime? Expected_Delivery_Date { get; set; }
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public decimal Net_Total { get; set; }
            public int Status { get; set; }
            public decimal Deposit_Remaining { get; set; }
            public int Lines_Count { get; set; }
            public long? Assignment_Id { get; set; }
            public DateTime? Planned_Date { get; set; }
            /// <summary>قيمة المتبقى تقريباً (صافى السطر × نسبة المتبقى).</summary>
            public decimal Remaining_Value { get; set; }
        }

        public class LoadSuggestion
        {
            public long Source_Unit_Item_Id { get; set; }
            public long Source_Store_Id { get; set; }
            public string Source_Store_Name { get; set; } = "";
            public long Van_Unit_Item_Id { get; set; }
            public string Item_Name { get; set; } = "";
            public string B_Unit { get; set; } = "";
            public string S_Unit { get; set; } = "";
            public decimal Factor { get; set; } = 1;
            public decimal Reserved_Small { get; set; }
            public decimal Van_Small { get; set; }
            public decimal Suggest_Small => Math.Max(0, Reserved_Small - Van_Small);
        }

        public class DeliveryLine
        {
            public long Order_Line_Id { get; set; }
            public long Unit_Item_Id { get; set; }
            public string Item_Name { get; set; } = "";
            public string B_Unit { get; set; } = "";
            public string S_Unit { get; set; } = "";
            public decimal Factor { get; set; } = 1;
            public decimal Remaining_Large { get; set; }
            public decimal Remaining_Small { get; set; }
            public decimal Remaining_Qty_Small => Remaining_Large * Factor + Remaining_Small;
            /// <summary>صافى الوحدة الصغرى المتفق عليه (صافى السطر ÷ كميته).</summary>
            public decimal Net_Unit_Price { get; set; }
            public decimal Van_Small { get; set; }
        }

        public class DeliveryView
        {
            public long Order_Id { get; set; }
            public long Order_No { get; set; }
            public DateTime? Order_Date { get; set; }
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public long Assignment_Id { get; set; }
            public decimal Deposit_Remaining { get; set; }
            public List<DeliveryLine> Lines { get; set; } = new();
        }

        public class DeliveryLineInput
        {
            public long Order_Line_Id { get; set; }
            public decimal Qty_Large { get; set; }
            public decimal Qty_Small { get; set; }
        }

        public class DeliveryInput
        {
            public Guid Doc_Guid { get; set; }
            public long Order_Id { get; set; }
            public List<DeliveryLineInput> Lines { get; set; } = new();
            /// <summary>الباقى بعد العربون يُحصَّل نقداً الآن؛ وإلا على حساب العميل (بفحص الائتمان).</summary>
            public bool Collect_Cash { get; set; }
            /// <summary>طريقة دفع الباقى المحصَّل الآن (0/1 = نقدى فى العهدة).</summary>
            public long Payment_Method_Id { get; set; }
            /// <summary>للمتبقى غير المسلَّم: Retry أو Close (إلزامى لو بقى متبقٍ).</summary>
            public int? Remaining_Action { get; set; }
            public string Reason { get; set; }
            public string Notes { get; set; }
        }

        public class DeliveryCheck
        {
            public SalesRepSaleService.CheckResult Result { get; set; } = new();
            public decimal Net_Total { get; set; }
            public decimal Deposit_Applied { get; set; }
            public decimal To_Collect { get; set; }
            public bool Has_Remaining { get; set; }
            public bool Ok => Result.Ok;
        }

        public class DeliveryResult
        {
            public long Delivery_Id { get; set; }
            public long? Invoice_Id { get; set; }
            public int Order_Status { get; set; }
        }

        // ============================== القراءة ==============================

        /// <summary>طلبيات مؤكدة بلا تنفيذ ولا إسناد قائم (لفرع المندوب)، الأقدم تسليماً أولاً.</summary>
        public List<OrderRow> GetAssignable(long repId, string search = null) => Read(opp =>
        {
            var rep = Rep(opp, repId);
            long branch = rep.Branch_Id ?? Common.Branch_Id;
            string like = string.IsNullOrWhiteSpace(search) ? "" : search.Trim();
            return opp.db.Database.SqlQuery<OrderRow>($@"
SELECT o.Id, CAST(ISNULL(o.Order_No, 0) AS bigint) AS Order_No, o.Order_Date, o.Expected_Delivery_Date, CAST(ISNULL(o.Customer_Id, 0) AS bigint) AS Customer_Id,
       ISNULL(c.Name, N'') AS Customer_Name, CAST(ISNULL(o.Net_Total, 0) AS decimal(18,2)) AS Net_Total, ISNULL(o.Status, 0) AS Status,
       CAST(ISNULL((SELECT SUM(ISNULL(d.Amount,0) - ISNULL(d.Consumed_Amount,0)) FROM Customer_Order_Deposits d WHERE d.Order_Id = o.Id), 0) AS decimal(18,2)) AS Deposit_Remaining,
       (SELECT COUNT(*) FROM Customer_Order_Lines l WHERE l.Order_Id = o.Id) AS Lines_Count,
       CAST(NULL AS bigint) AS Assignment_Id, CAST(NULL AS date) AS Planned_Date, CAST(ISNULL(o.Net_Total, 0) AS decimal(18,2)) AS Remaining_Value
FROM Customer_Orders o LEFT JOIN Customer c ON c.Id = o.Customer_Id
WHERE o.Status = {(int)CustomerOrderStatus.Confirmed} AND ISNULL(o.Branch_Id, {branch}) = {branch}
  AND NOT EXISTS (SELECT 1 FROM Customer_Order_Fulfillments f JOIN Customer_Order_Lines l ON l.Id = f.Order_Line_Id WHERE l.Order_Id = o.Id)
  AND NOT EXISTS (SELECT 1 FROM SalesRep_Order_Assignments a WHERE a.Order_Id = o.Id AND a.Status = {(int)SalesRepAssignmentStatus.Assigned})
  AND ({like} = N'' OR c.Name LIKE N'%' + {like} + N'%' OR CAST(o.Order_No AS nvarchar(30)) = {like})
ORDER BY ISNULL(o.Expected_Delivery_Date, o.Order_Date), o.Order_No").ToList();
        });

        /// <summary>الطلبيات المسندة للمندوب وما زالت مفتوحة (طلبيات اليوم وما بعده)، الأقرب تسليماً أولاً.</summary>
        public List<OrderRow> GetRepOrders(long repId) => Read(opp => opp.db.Database.SqlQuery<OrderRow>($@"
SELECT o.Id, CAST(ISNULL(o.Order_No, 0) AS bigint) AS Order_No, o.Order_Date, o.Expected_Delivery_Date, CAST(ISNULL(o.Customer_Id, 0) AS bigint) AS Customer_Id,
       ISNULL(c.Name, N'') AS Customer_Name, CAST(ISNULL(o.Net_Total, 0) AS decimal(18,2)) AS Net_Total, ISNULL(o.Status, 0) AS Status,
       CAST(ISNULL((SELECT SUM(ISNULL(d.Amount,0) - ISNULL(d.Consumed_Amount,0)) FROM Customer_Order_Deposits d WHERE d.Order_Id = o.Id), 0) AS decimal(18,2)) AS Deposit_Remaining,
       (SELECT COUNT(*) FROM Customer_Order_Lines l WHERE l.Order_Id = o.Id) AS Lines_Count,
       a.Id AS Assignment_Id, a.Planned_Date,
       CAST(ISNULL((SELECT SUM(ISNULL(l.Line_Total, 0) *
              CASE WHEN ISNULL(l.Qty_Larg,0) * f.Factor + ISNULL(l.Qty_Small,0) <= 0 THEN 0 ELSE
                ((ISNULL(l.Qty_Larg,0) - ISNULL(l.Fulfilled_Qty_Larg,0) - ISNULL(l.Cancelled_Qty_Larg,0)) * f.Factor
                 + ISNULL(l.Qty_Small,0) - ISNULL(l.Fulfilled_Qty_Small,0) - ISNULL(l.Cancelled_Qty_Small,0))
                / (ISNULL(l.Qty_Larg,0) * f.Factor + ISNULL(l.Qty_Small,0)) END)
              FROM Customer_Order_Lines l JOIN Unit_item u ON u.id = l.Unit_Item_Table_Id
              CROSS APPLY (SELECT CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS Factor) f
              WHERE l.Order_Id = o.Id), 0) AS decimal(18,2)) AS Remaining_Value
FROM SalesRep_Order_Assignments a JOIN Customer_Orders o ON o.Id = a.Order_Id LEFT JOIN Customer c ON c.Id = o.Customer_Id
WHERE a.Rep_Id = {repId} AND a.Status = {(int)SalesRepAssignmentStatus.Assigned}
  AND o.Status IN ({(int)CustomerOrderStatus.Confirmed}, {(int)CustomerOrderStatus.PartiallyFulfilled})
ORDER BY ISNULL(a.Planned_Date, ISNULL(o.Expected_Delivery_Date, o.Order_Date)), o.Order_No").ToList());

        /// <summary>اقتراح التحميل: لكل صنف بالسيارة المحجوز للطلبيات المسندة مقابل رصيد السيارة، ومصدره سطر المخزن الأصلى.</summary>
        public List<LoadSuggestion> GetLoadSuggestion(long repId) => Read(opp =>
        {
            var rep = Rep(opp, repId);
            var rows = opp.db.Database.SqlQuery<LoadSuggestion>($@"
SELECT CAST(MIN(ISNULL(al.Original_Unit_Item_Id, 0)) AS bigint) AS Source_Unit_Item_Id, CAST(0 AS bigint) AS Source_Store_Id, CAST(N'' AS nvarchar(200)) AS Source_Store_Name,
       al.Van_Unit_Item_Id, MAX(ISNULL(i.Item_Name, N'')) AS Item_Name, MAX(ISNULL(un.B_Unit, N'')) AS B_Unit, MAX(ISNULL(un.S_unit, N'')) AS S_Unit,
       CAST(MAX(f.Factor) AS decimal(18,6)) AS Factor,
       CAST(SUM(CASE WHEN r.L > 0 THEN r.L ELSE 0 END * f.Factor + CASE WHEN r.S > 0 THEN r.S ELSE 0 END) AS decimal(18,4)) AS Reserved_Small,
       CAST(0 AS decimal(18,4)) AS Van_Small
FROM SalesRep_Order_Assignments a
JOIN SalesRep_Order_Assignment_Lines al ON al.Assignment_Id = a.Id
JOIN Customer_Order_Lines l ON l.Id = al.Order_Line_Id
JOIN Customer_Orders o ON o.Id = a.Order_Id
JOIN Unit_item u ON u.id = al.Van_Unit_Item_Id LEFT JOIN Item i ON i.id = u.Item_id LEFT JOIN unit un ON un.id = u.unit_id
CROSS APPLY (SELECT CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS Factor) f
CROSS APPLY (SELECT ISNULL(l.Qty_Larg,0) - ISNULL(l.Fulfilled_Qty_Larg,0) - ISNULL(l.Cancelled_Qty_Larg,0) AS L,
                    ISNULL(l.Qty_Small,0) - ISNULL(l.Fulfilled_Qty_Small,0) - ISNULL(l.Cancelled_Qty_Small,0) AS S) r
WHERE a.Rep_Id = {repId} AND a.Status = {(int)SalesRepAssignmentStatus.Assigned}
  AND o.Status IN ({(int)CustomerOrderStatus.Confirmed}, {(int)CustomerOrderStatus.PartiallyFulfilled}) AND ISNULL(l.Line_Status, 0) <> {(int)CustomerOrderLineStatus.Cancelled}
GROUP BY al.Van_Unit_Item_Id").ToList();
            var engine = new StockMovementService(opp);
            foreach (var r in rows)
            {
                r.Van_Small = engine.GetComputedBalanceSmall(r.Van_Unit_Item_Id);
                var src = opp.db.Database.SqlQuery<StoreRow>($"SELECT CAST(ISNULL(u.Store_id, 0) AS bigint) AS Id, ISNULL(s.Store_Name, N'') AS Name FROM Unit_item u LEFT JOIN Store s ON s.Id = u.Store_id WHERE u.id = {r.Source_Unit_Item_Id}")
                    .AsEnumerable().FirstOrDefault();
                r.Source_Store_Id = src?.Id ?? 0;
                r.Source_Store_Name = src?.Name ?? "";
            }
            return rows.Where(r => r.Reserved_Small > 0).OrderBy(r => r.Item_Name).ToList();
        });

        public class StoreRow { public long Id { get; set; } public string Name { get; set; } = ""; }

        /// <summary>شاشة التسليم: المتبقى لكل سطر وسعره المتفق عليه ورصيد السيارة والعربون المتبقى.</summary>
        public DeliveryView GetDelivery(long repId, long orderId) => Read(opp =>
        {
            var a = ActiveAssignment(opp, repId, orderId);
            var order = opp.db.CustomerOrders.AsNoTracking().First(o => o.Id == orderId);
            var engine = new StockMovementService(opp);
            var view = new DeliveryView
            {
                Order_Id = order.Id, Order_No = order.Order_No ?? 0, Order_Date = order.Order_Date, Customer_Id = order.Customer_Id ?? 0, Assignment_Id = a.Id,
                Customer_Name = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Name, N'') AS Value FROM Customer WHERE Id = {order.Customer_Id ?? 0}").AsEnumerable().FirstOrDefault() ?? "",
                Deposit_Remaining = DepositRemaining(opp, orderId),
            };
            foreach (var x in Lines(opp, orderId))
            {
                var l = x.Line;
                view.Lines.Add(new DeliveryLine
                {
                    Order_Line_Id = l.Id, Unit_Item_Id = l.Unit_Item_Table_Id ?? 0, Item_Name = x.Info.Item_Name, B_Unit = x.Info.B_Unit, S_Unit = x.Info.S_Unit, Factor = x.Info.Factor,
                    Remaining_Large = CustomerOrdersService.RemainingLarg(l), Remaining_Small = CustomerOrdersService.RemainingSmall(l),
                    Net_Unit_Price = x.OrderedSmall > 0 ? Math.Round((l.Line_Total ?? 0) / x.OrderedSmall, 4) : 0,
                    Van_Small = engine.GetComputedBalanceSmall(l.Unit_Item_Table_Id ?? 0),
                });
            }
            return view;
        });

        // ============================== الإسناد ==============================

        /// <summary>يسند طلبية مؤكدة بلا تنفيذ للمندوب: أسطرها تنتقل لأصناف السيارة فيصير حجزها عليها. يرجع معرّف الإسناد.</summary>
        public long Assign(long repId, long orderId, DateTime? plannedDate = null) => Tx(opp =>
        {
            var rep = Rep(opp, repId);
            if (!rep.Active) throw new InvalidOperationException("ملف المندوب موقوف.");
            var order = opp.db.CustomerOrders.FirstOrDefault(o => o.Id == orderId) ?? throw new InvalidOperationException("الطلبية غير موجودة.");
            if ((order.Status ?? 0) != (int)CustomerOrderStatus.Confirmed) throw new InvalidOperationException("تُسنَد الطلبية المؤكدة فقط.");
            var orders = new CustomerOrdersService(opp);
            if (orders.HasFulfillments(orderId)) throw new InvalidOperationException("الطلبية نُفِّذ منها جزء؛ لا تُسنَد للمندوب.");
            if (opp.db.SalesRep_Order_Assignments.Any(x => x.Order_Id == orderId && x.Status == (int)SalesRepAssignmentStatus.Assigned))
                throw new InvalidOperationException("الطلبية مسندة لمندوب بالفعل.");
            var lines = opp.db.CustomerOrderLines.Where(l => l.Order_Id == orderId).ToList();
            if (lines.Count == 0) throw new InvalidOperationException("الطلبية بلا أسطر.");

            var assignment = new SalesRep_Order_Assignment
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(), Order_Id = orderId, Rep_Id = repId, Planned_Date = (plannedDate ?? order.Expected_Delivery_Date ?? DateTime.Today).Date,
                Status = (int)SalesRepAssignmentStatus.Assigned, Original_Store_Id = order.Store_Id,
            };
            assignment.StampInsert(rep.Branch_Id ?? Common.Branch_Id);
            opp.db.SalesRep_Order_Assignments.Add(assignment);
            var stores = new StoreOrderService(opp);
            DateTime now = DateTime.Now;
            foreach (var l in lines)
            {
                long original = l.Unit_Item_Table_Id ?? 0;
                if (original <= 0) throw new InvalidOperationException("سطر طلبية بلا صنف.");
                long vanLine = stores.EnsureStoreLine(original, rep.Van_Store_Id);
                var al = new SalesRep_Order_Assignment_Line { Id = CallSnowflakeIdGenerate.GenerateNewId(), Assignment_Id = assignment.Id, Order_Line_Id = l.Id, Original_Unit_Item_Id = original, Van_Unit_Item_Id = vanLine };
                al.StampInsert(rep.Branch_Id ?? Common.Branch_Id);
                opp.db.SalesRep_Order_Assignment_Lines.Add(al);
                l.Unit_Item_Table_Id = vanLine;
                l.Update_ServerDateTime = now;
            }
            order.Store_Id = rep.Van_Store_Id;
            order.Edit_User_Id = Common.User_Id;
            order.EditOperationDate = now;
            order.Update_ServerDateTime = now;
            opp.db.SaveChanges();
            return assignment.Id;
        });

        /// <summary>يلغى الإسناد قبل أى تنفيذ: الأسطر تعود لأصنافها الأصلية والطلبية لمخزنها، بسبب إلزامى.</summary>
        public void Unassign(long repId, long orderId, string reason) => Tx(opp =>
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("سبب إلغاء الإسناد مطلوب.");
            var a = opp.db.SalesRep_Order_Assignments.FirstOrDefault(x => x.Rep_Id == repId && x.Order_Id == orderId && x.Status == (int)SalesRepAssignmentStatus.Assigned)
                    ?? throw new InvalidOperationException("الطلبية غير مسندة لهذا المندوب.");
            if (new CustomerOrdersService(opp).HasFulfillments(orderId))
                throw new InvalidOperationException("سُلِّم جزء من الطلبية؛ لا يُلغى الإسناد. أغلق المتبقى أو أكمل التسليم.");
            var map = opp.db.SalesRep_Order_Assignment_Lines.Where(x => x.Assignment_Id == a.Id).ToList().ToDictionary(x => x.Order_Line_Id);
            DateTime now = DateTime.Now;
            foreach (var l in opp.db.CustomerOrderLines.Where(l => l.Order_Id == orderId).ToList())
                if (map.TryGetValue(l.Id, out var m) && m.Original_Unit_Item_Id is > 0) { l.Unit_Item_Table_Id = m.Original_Unit_Item_Id; l.Update_ServerDateTime = now; }
            var order = opp.db.CustomerOrders.First(o => o.Id == orderId);
            order.Store_Id = a.Original_Store_Id;
            order.Edit_User_Id = Common.User_Id; order.EditOperationDate = now; order.Update_ServerDateTime = now;
            a.Status = (int)SalesRepAssignmentStatus.Unassigned;
            a.Ended_At = now;
            a.End_Reason = SalesRepSaleService.Trim(reason.Trim(), 500);
            a.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== أخذ طلبية من المندوب ==============================

        /// <summary>سطر طلبية جديدة: الكمية والسعر (للوحدة الكبرى) والإجمالى قبل الخصم والخصم والضريبة والصافى كما حسبها حوار الصنف.</summary>
        public class TakeOrderLine
        {
            public long Unit_Item_Id { get; set; }
            public decimal Qty_Large { get; set; }
            public decimal Qty_Small { get; set; }
            public decimal Unit_Price { get; set; }
            public decimal Gross { get; set; }
            public decimal Discount_Value { get; set; }
            public decimal Tax_Value { get; set; }
            public decimal Line_Total { get; set; }
            public string Notes { get; set; }
        }

        public class TakeOrderInput
        {
            public Guid Doc_Guid { get; set; }
            public long Customer_Id { get; set; }
            public long Source_Store_Id { get; set; }
            public long? Currency_Id { get; set; }
            public DateTime? Expected_Delivery_Date { get; set; }
            public string Notes { get; set; }
            /// <summary>عربون يُقبض الآن (0 = بلا عربون): نقداً فى صندوق العهدة، أو بطريقة غير نقدية فى بنكها.</summary>
            public decimal Deposit { get; set; }
            public long Deposit_Payment_Method_Id { get; set; }
            public List<TakeOrderLine> Lines { get; set; } = new();
            public decimal Net_Total => Math.Round(Lines.Sum(l => l.Line_Total), 2);
        }

        public class TakeOrderResult
        {
            public long Order_Id { get; set; }
            public long Order_No { get; set; }
            public long Assignment_Id { get; set; }
            public long? Deposit_Voucher_Id { get; set; }
        }

        /// <summary>مخازن يأخذ منها المندوب طلبية: المخازن الفعلية النشطة عدا مخازن السيارات وتالفها، بنفس ترتيب حوار الصنف.</summary>
        public List<StoreRow> GetSourceStores(long repId) => Read(opp =>
        {
            Rep(opp, repId);
            return opp.db.Database.SqlQueryRaw<StoreRow>("SELECT CAST(Id AS bigint) AS Id, ISNULL(Store_Name, N'') AS Name FROM Store WHERE Active = 1 AND IS_Virtual_OuterStore = 0" +
                SalesRepSaleService.ExcludeRepStoresSql(opp, "Store.Id") + " ORDER BY IS_Default_Store DESC, Store_Name").ToList();
        });

        /// <summary>المخزن الافتراضى لطلبية المندوب: مصدر آخر تحميل لسيارته، وإلا أول مخزن متاح.</summary>
        public long GetDefaultSourceStore(long repId) => Read(opp =>
        {
            var rep = Rep(opp, repId);
            var stores = new SalesRepOrderService(opp).GetSourceStores(repId).Select(s => s.Id).ToList();
            long last = opp.db.Order_Purshings.AsNoTracking()
                .Where(o => o.Order_Type == SalesRepShiftService.TransferType && o.Id_To == rep.Van_Store_Id)
                .OrderByDescending(o => o.Id).Select(o => o.Id_From).FirstOrDefault();
            return stores.Contains(last) ? last : stores.FirstOrDefault();
        });

        public SalesRepSaleService.CheckResult CheckTake(long repId, TakeOrderInput input) => Read(opp =>
        {
            Validate(input);
            var result = new SalesRepSaleService(opp).Check(repId, TakeInputOf(opp, input));
            bool cashDeposit = input.Deposit_Payment_Method_Id <= 0 || input.Deposit_Payment_Method_Id == SalesRepShiftService.CashPaymentMethodId;
            if (input.Deposit > 0 && cashDeposit && SalesRepSaleService.CashLimitWarning(SalesRepSaleService.Context(opp, repId), input.Deposit) is string w)
                result.Warnings.Add(w);
            return result;
        });

        public SalesRepSaleService.CheckResult RequestTakeApprovals(long repId, TakeOrderInput input, string reason = null) => Tx(opp =>
        {
            Validate(input);
            return new SalesRepSaleService(opp).RequestApprovals(repId, TakeInputOf(opp, input), reason, SalesRepDocType.CustomerOrder);
        });

        /// <summary>
        /// يحفظ طلبية المندوب مؤكدة على المخزن المختار، يسندها له (فينتقل حجزها لسيارته)، يربطها بالوردية، ويقبض العربون إن وُجد —
        /// فى معاملة واحدة بعد إعادة الفحص (مخالفة بلا موافقة = SalesRepApprovalRequiredException).
        /// </summary>
        public TakeOrderResult TakeOrder(long repId, TakeOrderInput input) => Tx(opp =>
        {
            Validate(input);
            var ctx = SalesRepSaleService.Context(opp, repId);
            if (!ctx.CanSell) throw new InvalidOperationException("لا وردية مفتوحة للمندوب؛ أخذ الطلبية يتم داخل وردية مفتوحة فقط.");
            var check = new SalesRepSaleService(opp).Check(repId, TakeInputOf(opp, input));
            if (!check.Ok) throw new SalesRepApprovalRequiredException(check.Violations.Where(v => !v.Covered).ToList());

            var header = new CustomerOrder
            {
                Customer_Id = input.Customer_Id, Currency_Id = input.Currency_Id, Order_Date = DateTime.Today, Expected_Delivery_Date = input.Expected_Delivery_Date?.Date,
                Notes = SalesRepSaleService.Trim(input.Notes?.Trim(), 500), Store_Id = input.Source_Store_Id, Sales_Person_Id = ctx.Employee_Id,
            };
            var lines = input.Lines.Select(l => new CustomerOrderLine
            {
                Unit_Item_Table_Id = l.Unit_Item_Id, Qty_Larg = l.Qty_Large, Qty_Small = l.Qty_Small, Unit_Price = l.Unit_Price,
                Discount_Value = Math.Round(l.Discount_Value, 2), Discount_Percent = l.Gross > 0 ? Math.Round(l.Discount_Value / l.Gross * 100, 2) : 0,
                Tax_Value = Math.Round(l.Tax_Value, 2), Line_Total = Math.Round(l.Line_Total, 2), Notes = l.Notes,
            }).ToList();
            var orders = new CustomerOrdersService(opp);
            var (orderId, orderNo) = orders.SaveOrder(header, lines, true);
            long assignmentId = new SalesRepOrderService(opp).Assign(repId, orderId, input.Expected_Delivery_Date);
            SalesRepSaleService.Link(opp, ctx.Shift_Id.Value, SalesRepDocType.CustomerOrder, orderId, ctx.Branch_Id);

            long? voucherId = null;
            decimal deposit = Math.Round(input.Deposit, 2);
            if (deposit > 0)
            {
                var receipt = SalesRepPaymentService.Resolve(opp, ctx, input.Deposit_Payment_Method_Id);
                var vouchers = new CashVouchersService();
                var v = vouchers.SavePartyVoucherInTx(opp, 1, 0, receipt.BoxId, DateTime.Today, 0,
                    $"عربون الطلبية {orderNo}{(receipt.IsCash ? "" : " — " + receipt.MethodName)} — المندوب {ctx.Rep_Name}",
                    deposit, input.Customer_Id, vouchers.GetFactor(SalesRepPaymentService.CurrencyOf(opp, receipt.BoxId)), 0, 0, receipt.MethodId, receipt.MethodName);
                orders.AddDeposit(orderId, v.OrderId, v.BoxLineId, input.Customer_Id, deposit, DateTime.Today, "عربون عند أخذ الطلبية — المندوب " + ctx.Rep_Name);
                SalesRepSaleService.Link(opp, ctx.Shift_Id.Value, SalesRepDocType.CashVoucher, v.OrderId, ctx.Branch_Id);
                voucherId = v.OrderId;
            }
            SalesRepVisitService.Attach(opp, ctx.Shift_Id.Value, input.Customer_Id, SalesRepVisitResult.Order,
                voucherId == null ? new[] { (SalesRepDocType.CustomerOrder, orderId) } : new[] { (SalesRepDocType.CustomerOrder, orderId), (SalesRepDocType.CashVoucher, voucherId.Value) });

            var used = opp.db.SalesRep_Approvals.Where(a => a.Ref_Document_Guid == input.Doc_Guid && a.Status == (int)SalesRepApprovalStatus.Approved && a.Ref_Doc_Id == null).ToList();
            foreach (var a in used) { a.Ref_Doc_Id = orderId; a.Shift_Id ??= ctx.Shift_Id; a.StampEdit(); }
            opp.db.SaveChanges();
            return new TakeOrderResult { Order_Id = orderId, Order_No = orderNo, Assignment_Id = assignmentId, Deposit_Voucher_Id = voucherId };
        });

        private static void Validate(TakeOrderInput input)
        {
            if (input.Doc_Guid == Guid.Empty) throw new InvalidOperationException("معرّف المستند مطلوب.");
            if (input.Customer_Id <= 0) throw new InvalidOperationException("اختر العميل.");
            if (input.Lines == null || input.Lines.Count == 0) throw new InvalidOperationException("الطلبية بلا أصناف.");
            if (input.Lines.Any(l => l.Unit_Item_Id <= 0 || l.Qty_Large < 0 || l.Qty_Small < 0 || l.Qty_Large + l.Qty_Small <= 0))
                throw new InvalidOperationException("كمية سطر غير صحيحة.");
            if (input.Lines.Any(l => l.Unit_Price < 0 || l.Discount_Value < 0 || l.Tax_Value < 0 || l.Line_Total < 0))
                throw new InvalidOperationException("سعر أو خصم سطر غير صحيح.");
            if (input.Deposit < 0) throw new InvalidOperationException("العربون لا يكون سالباً.");
            if (Math.Round(input.Deposit, 2) > input.Net_Total) throw new InvalidOperationException("العربون أكبر من صافى الطلبية.");
            if (input.Expected_Delivery_Date is DateTime d && d.Date < DateTime.Today) throw new InvalidOperationException("تاريخ التسليم المتوقع قبل اليوم.");
        }

        /// <summary>مدخل الفحص: الكمية بالصغرى، ونسبة الخصم = خصم السطر ÷ إجماليه، والآجل = الصافى − العربون.</summary>
        private static SalesRepSaleService.SaleInput TakeInputOf(operations opp, TakeOrderInput input)
        {
            var ids = input.Lines.Select(l => l.Unit_Item_Id).Distinct().ToList();
            var units = opp.db.Unit_item.AsNoTracking().Where(u => ids.Contains(u.id)).Select(u => new { u.id, u.Store_id, u.Small_unit }).ToList().ToDictionary(u => u.id);
            decimal onAccount = Math.Max(0, input.Net_Total - Math.Round(input.Deposit, 2));
            return new SalesRepSaleService.SaleInput
            {
                Doc_Guid = input.Doc_Guid, Customer_Id = input.Customer_Id, Order_Taking = true, Source_Store_Id = input.Source_Store_Id,
                Is_Credit = onAccount > 0, Net_Total = onAccount,
                Lines = input.Lines.Select(l =>
                {
                    units.TryGetValue(l.Unit_Item_Id, out var u);
                    decimal f = (u?.Small_unit ?? 0) <= 0 ? 1 : u.Small_unit.Value;
                    return new SalesRepSaleService.SaleLine
                    {
                        Unit_Item_Id = l.Unit_Item_Id, Store_Id = u?.Store_id ?? 0, Qty_Small = l.Qty_Large * f + l.Qty_Small,
                        Discount_Rate = l.Gross > 0 ? Math.Round(l.Discount_Value / l.Gross * 100, 4) : 0,
                    };
                }).ToList(),
            };
        }

        // ============================== التسليم ==============================

        public DeliveryCheck Check(long repId, DeliveryInput input) => Read(opp =>
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            var b = Build(opp, ctx, input);
            if (b.InvL == null) return b.Check;
            b.Check.Result = new SalesRepSaleService(opp).Check(repId, SaleInputOf(opp, input, b));
            return b.Check;
        });

        public DeliveryCheck RequestApprovals(long repId, DeliveryInput input, string reason = null) => Tx(opp =>
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            var b = Build(opp, ctx, input);
            if (b.InvL == null) return b.Check;
            b.Check.Result = new SalesRepSaleService(opp).RequestApprovals(repId, SaleInputOf(opp, input, b), reason);
            return b.Check;
        });

        /// <summary>
        /// التسليم: فاتورة بالكميات المسلَّمة (إن وُجدت) بمسار بيع المندوب، ربط التنفيذ، تسوية العربون، تحصيل الباقى نقداً إن طُلب،
        /// ثم المتبقى حسب الإجراء، وسجل المحاولة — فى معاملة واحدة.
        /// </summary>
        public DeliveryResult Deliver(long repId, DeliveryInput input) => Tx(opp =>
        {
            if (input.Doc_Guid == Guid.Empty) throw new InvalidOperationException("معرّف المستند مطلوب.");
            var ctx = SalesRepSaleService.Context(opp, repId);
            var b = Build(opp, ctx, input);
            var orders = new CustomerOrdersService(opp);
            long? invoiceId = null;
            long? restVoucherId = null;
            decimal applied = 0, collected = 0;
            if (b.InvL != null)
            {
                var sales = new SalesRepSaleService(opp);
                invoiceId = sales.SaveSale(repId, input.Doc_Guid, b.InvL, b.InvD, prepaid: b.Check.Deposit_Applied, collectNow: input.Collect_Cash, orderPriced: true,
                    paymentMethodId: input.Payment_Method_Id);
                orders.RecordFulfillments(input.Order_Id, invoiceId.Value, b.Allocations);
                if (b.Check.Deposit_Applied > 0) applied = orders.ApplyDepositsToInvoice(input.Order_Id, invoiceId.Value, b.Check.Deposit_Applied);
                // مع العربون الفاتورة آجلة: الباقى نقداً بإذن استلام على صندوق العهدة مربوط بالوردية.
                decimal rest = Math.Round(b.Check.Net_Total - applied, 2);
                if (applied > 0 && input.Collect_Cash && rest > 0)
                {
                    var receipt = SalesRepPaymentService.Resolve(opp, ctx, input.Payment_Method_Id);
                    var vouchers = new CashVouchersService();
                    var v = vouchers.SavePartyVoucherInTx(opp, 1, 0, receipt.BoxId, DateTime.Today, 0,
                        $"تحصيل تسليم الطلبية {b.Order.Order_No}{(receipt.IsCash ? "" : " — " + receipt.MethodName)} — المندوب {ctx.Rep_Name}",
                        rest, b.Order.Customer_Id ?? 0, vouchers.GetFactor(SalesRepPaymentService.CurrencyOf(opp, receipt.BoxId)), Common.Invoice_Type_Id, invoiceId.Value,
                        receipt.MethodId, receipt.MethodName);
                    SalesRepSaleService.Link(opp, ctx.Shift_Id.Value, SalesRepDocType.CashVoucher, v.OrderId, ctx.Branch_Id);
                    restVoucherId = v.OrderId;
                    collected = rest;
                }
                else if (applied == 0 && input.Collect_Cash) collected = b.Check.Net_Total;
            }

            if (b.Check.Has_Remaining && input.Remaining_Action == (int)SalesRepRemainingAction.Close)
                orders.CloseRemaining(input.Order_Id, input.Reason.Trim());
            else orders.RecalculateOrder(input.Order_Id);

            var order = opp.db.CustomerOrders.AsNoTracking().First(o => o.Id == input.Order_Id);
            var a = b.Assignment;
            var status = (CustomerOrderStatus)(order.Status ?? 0);
            if (status == CustomerOrderStatus.Fulfilled) a.Status = (int)SalesRepAssignmentStatus.Delivered;
            else if (status == CustomerOrderStatus.Closed) a.Status = (int)SalesRepAssignmentStatus.Closed;
            if (a.Status != (int)SalesRepAssignmentStatus.Assigned) { a.Ended_At = DateTime.Now; a.End_Reason = b.Check.Has_Remaining ? SalesRepSaleService.Trim(input.Reason, 500) : null; }
            a.StampEdit();

            var d = new SalesRep_Order_Delivery
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(), Assignment_Id = a.Id, Order_Id = input.Order_Id, Shift_Id = ctx.Shift_Id, Invoice_Id = invoiceId,
                Result = (int)(invoiceId == null ? SalesRepDeliveryResult.Refused : b.Check.Has_Remaining ? SalesRepDeliveryResult.Partial : SalesRepDeliveryResult.Full),
                Remaining_Action = b.Check.Has_Remaining ? input.Remaining_Action : null,
                Reason = b.Check.Has_Remaining ? SalesRepSaleService.Trim(input.Reason?.Trim(), 500) : null,
                Deposit_Applied = applied, Cash_Collected = collected, Notes = SalesRepSaleService.Trim(input.Notes?.Trim(), 500),
            };
            d.StampInsert(ctx.Branch_Id);
            opp.db.SalesRep_Order_Deliveries.Add(d);
            opp.db.SaveChanges();
            SalesRepSaleService.Link(opp, ctx.Shift_Id.Value, SalesRepDocType.CustomerOrder, input.Order_Id, ctx.Branch_Id);
            var docs = new List<(SalesRepDocType, long)> { (SalesRepDocType.CustomerOrder, input.Order_Id) };
            if (restVoucherId != null) docs.Add((SalesRepDocType.CashVoucher, restVoucherId.Value));
            SalesRepVisitService.Attach(opp, ctx.Shift_Id.Value, order.Customer_Id ?? 0, SalesRepVisitResult.Delivery, docs.ToArray());
            return new DeliveryResult { Delivery_Id = d.Id, Invoice_Id = invoiceId, Order_Status = order.Status ?? 0 };
        });

        // ============================== البناء ==============================

        private class LineInfo { public string Item_Name { get; set; } = ""; public string Code { get; set; } = ""; public string B_Unit { get; set; } = ""; public string S_Unit { get; set; } = ""; public decimal Factor { get; set; } = 1; public long Item_Id { get; set; } public long Store_Id { get; set; } public string Store_Name { get; set; } = ""; }

        private class OrderLineX { public CustomerOrderLine Line { get; set; } public LineInfo Info { get; set; } public decimal OrderedSmall { get; set; } }

        private class Built
        {
            public CustomerOrder Order { get; set; }
            public SalesRep_Order_Assignment Assignment { get; set; }
            public InvoicesOperationListCLS InvL { get; set; }
            public InvoicesOperationCLS InvD { get; set; }
            public List<CustomerOrdersService.Allocation> Allocations { get; set; } = new();
            public DeliveryCheck Check { get; set; } = new();
        }

        private static List<OrderLineX> Lines(operations opp, long orderId)
        {
            var lines = opp.db.CustomerOrderLines.AsNoTracking().Where(l => l.Order_Id == orderId).OrderBy(l => l.Line_No).ToList();
            var result = new List<OrderLineX>();
            foreach (var l in lines)
            {
                var info = opp.db.Database.SqlQuery<LineInfo>($@"
SELECT ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(i.code, N'') AS Code, ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor, CAST(ISNULL(u.Item_id, 0) AS bigint) AS Item_Id,
       CAST(ISNULL(u.Store_id, 0) AS bigint) AS Store_Id, ISNULL(s.Store_Name, N'') AS Store_Name
FROM Unit_item u LEFT JOIN Item i ON i.id = u.Item_id LEFT JOIN unit un ON un.id = u.unit_id LEFT JOIN Store s ON s.Id = u.Store_id
WHERE u.id = {l.Unit_Item_Table_Id ?? 0}").AsEnumerable().FirstOrDefault() ?? new LineInfo();
                result.Add(new OrderLineX { Line = l, Info = info, OrderedSmall = (l.Qty_Larg ?? 0) * info.Factor + (l.Qty_Small ?? 0) });
            }
            return result;
        }

        private Built Build(operations opp, SalesRepSaleService.SaleContext ctx, DeliveryInput input)
        {
            if (!ctx.CanSell) throw new InvalidOperationException("لا وردية مفتوحة للمندوب؛ التسليم يتم داخل وردية مفتوحة فقط.");
            var a = opp.db.SalesRep_Order_Assignments.FirstOrDefault(x => x.Rep_Id == ctx.Rep_Id && x.Order_Id == input.Order_Id && x.Status == (int)SalesRepAssignmentStatus.Assigned)
                    ?? throw new InvalidOperationException("الطلبية غير مسندة لهذا المندوب.");
            var order = opp.db.CustomerOrders.AsNoTracking().First(o => o.Id == input.Order_Id);
            if (!CustomerOrdersService.CanFulfill(order)) throw new InvalidOperationException("الطلبية غير مفتوحة للتسليم.");
            var b = new Built { Order = order, Assignment = a };
            var lines = Lines(opp, order.Id).ToDictionary(x => x.Line.Id);
            if (input.Lines.Any(l => l.Qty_Large < 0 || l.Qty_Small < 0)) throw new InvalidOperationException("الكمية لا تكون سالبة.");
            if (input.Lines.GroupBy(l => l.Order_Line_Id).Any(g => g.Count() > 1)) throw new InvalidOperationException("سطر الطلبية مكرر.");
            var picked = new List<(OrderLineX X, decimal L, decimal S)>();
            foreach (var l in input.Lines.Where(l => l.Qty_Large > 0 || l.Qty_Small > 0))
            {
                if (!lines.TryGetValue(l.Order_Line_Id, out var x)) throw new InvalidOperationException("سطر ليس من الطلبية.");
                if (l.Qty_Large > CustomerOrdersService.RemainingLarg(x.Line) || l.Qty_Small > CustomerOrdersService.RemainingSmall(x.Line))
                    throw new InvalidOperationException($"الكمية المسلَّمة من «{x.Info.Item_Name}» أكبر من المتبقى فى الطلبية.");
                if ((x.Info.Store_Id) != ctx.Van_Store_Id) throw new InvalidOperationException($"«{x.Info.Item_Name}» ليس على مخزن سيارة المندوب.");
                picked.Add((x, l.Qty_Large, l.Qty_Small));
            }
            // المتبقى بعد هذا التسليم
            b.Check.Has_Remaining = lines.Values.Any(x =>
            {
                var given = picked.FirstOrDefault(p => p.X.Line.Id == x.Line.Id);
                return CustomerOrdersService.RemainingLarg(x.Line) - given.L > 0 || CustomerOrdersService.RemainingSmall(x.Line) - given.S > 0;
            });
            if (b.Check.Has_Remaining)
            {
                if (string.IsNullOrWhiteSpace(input.Reason)) throw new InvalidOperationException("سبب عدم تسليم المتبقى مطلوب.");
                if (input.Remaining_Action is not ((int)SalesRepRemainingAction.Retry or (int)SalesRepRemainingAction.Close))
                    throw new InvalidOperationException("اختر مصير المتبقى: إعادة المحاولة أو إغلاق المتبقى.");
            }
            if (picked.Count == 0) return b;   // رفض كلى: بلا فاتورة

            var invL = new InvoicesOperationListCLS();
            var invD = new InvoicesOperationCLS(0);
            bool anyTax = false;
            foreach (var (x, takeL, takeS) in picked)
            {
                var l = x.Line;
                decimal orderedLarge = x.Info.Factor > 0 ? x.OrderedSmall / x.Info.Factor : 0;
                decimal gross = (l.Unit_Price ?? 0) * orderedLarge;
                decimal tax = l.Tax_Value ?? 0, without = (l.Line_Total ?? 0) - tax;
                // الخصم المتفق عليه = الإجمالى − صافى السطر قبل الضريبة، يُنقل قيمةً بنسبة الكمية المسلَّمة (أعمدة النسب بخانتين فالقيمة أدق)،
                // والضريبة نسبتها من السطر (الأعمدة بخانتين، كإدخال الشاشة).
                decimal share = x.OrderedSmall > 0 ? (takeL * x.Info.Factor + takeS) / x.OrderedSmall : 0;
                decimal discValue = Math.Round(Math.Max(0, gross - without) * share, 2, MidpointRounding.AwayFromZero);
                decimal taxRate = without > 0 && tax > 0 ? Math.Round(tax / without * 100, 2) : 0;
                anyTax |= taxRate > 0;
                var d = new IInvoices_Details
                {
                    Item_Id = x.Info.Item_Id, Unit_Item_Table_Id = l.Unit_Item_Table_Id, Invoice_Item_Name = x.Info.Item_Name, code = x.Info.Code,
                    Store_Id = ctx.Van_Store_Id, Store_Name = ctx.Van_Store_Name, UnitStr = x.Info.B_Unit, UnitSmallStr = x.Info.S_Unit,
                    Unit_Used_for_Price = 1, Make_Order = 0, Unit_Item_ExpireDate_Id = 0, Bills_Details_Id = 0, Item_Expire_Date = DateTime.Now.Date,
                    Tash3ela = "", StatementNo = "", OperationNo = "", Merchant_Name = "", Specification = "", NameSpecification = "", Notes = "",
                    Item_Notes = $"تسليم الطلبية {order.Order_No}",
                    Item_TaxSales10 = taxRate, Item_ATS05 = 0, Larg_Qnty = takeL, Small_Qnty = takeS, PriceUnit = l.Unit_Price ?? 0, Larg_Unit_Price = l.Unit_Price ?? 0,
                    Item_CommissionRate = 0, Item_CommissionValue = discValue, Item_DeletedFlage = 0, Item_ProcessedFlage = 0,
                };
                InvoiceLineCalculator.ComputeLine(d, x.Info.Factor);
                invD.invDlst.Add(d);
                b.Allocations.Add(new CustomerOrdersService.Allocation(l.Id, takeL, takeS));
            }

            var h = invL.invL;
            SalesRepSaleService.FillSaleHeader(opp, ctx, invL, invD, order.Customer_Id ?? 0, order.Currency_Id,
                $"تسليم الطلبية رقم {order.Order_No}" + (string.IsNullOrWhiteSpace(input.Notes) ? "" : " — " + input.Notes.Trim()), anyTax);
            decimal net = Math.Round(h.Net_Total ?? 0, 2);
            decimal deposit = Math.Min(DepositRemaining(opp, order.Id), net);
            // العربون يجعل الفاتورة آجلة (العميل دائن بالعربون فيتسوى من القيدين)؛ بلا عربون: نقدى لو يُحصَّل الآن وإلا آجل.
            SalesRepSaleService.FinishSaleHeader(opp, ctx, invL, invD, deposit > 0 || !input.Collect_Cash);

            b.InvL = invL; b.InvD = invD;
            b.Check.Net_Total = net;
            b.Check.Deposit_Applied = deposit;
            b.Check.To_Collect = input.Collect_Cash ? Math.Max(0, net - deposit) : 0;
            return b;
        }

        private static SalesRepSaleService.SaleInput SaleInputOf(operations opp, DeliveryInput input, Built b) => new()
        {
            Doc_Guid = input.Doc_Guid, Customer_Id = b.Order.Customer_Id ?? 0,
            Is_Credit = (b.InvL.invL.Is_Invoice_Agile ?? 0) == 1 && !(b.Check.Deposit_Applied > 0 && input.Collect_Cash),
            Net_Total = Math.Max(0, b.Check.Net_Total - b.Check.Deposit_Applied),
            Lines = SalesRepSaleService.LinesOf(opp, b.InvL, b.InvD), Order_Priced = true, Payment_Method_Id = input.Payment_Method_Id,
        };

        /// <summary>رقم فاتورة المبيعات التالى بنفس قواعد Make_New_Invoice_No (النوع/الفرع/السنة حسب الإعداد) مع قفل داخل المعاملة.</summary>
        internal static long NextInvoiceNo(operations opp, long branchId, DateTime date)
        {
            int byType = Common.Is_InvoiceNoBy_Invoice_Type == 1 ? 1 : 0, byBranch = Common.Is_InvoiceNoBy_Branch_Id == 1 ? 1 : 0, byYear = Common.Is_InvoiceNoBy_FinancialYear == 1 ? 1 : 0;
            return opp.db.Database.SqlQuery<long>($@"
SELECT CAST(ISNULL(MAX(Invoice_Id), 0) + 1 AS bigint) AS Value FROM Invoices_List WITH (UPDLOCK, HOLDLOCK)
WHERE Id > 0 AND ({byType} = 0 OR Invoice_Type = 0) AND ({byBranch} = 0 OR Branch_Id = {branchId}) AND ({byYear} = 0 OR YEAR(Invoice_Date) = {date.Year})").AsEnumerable().First();
        }

        private static decimal DepositRemaining(operations opp, long orderId) => opp.db.Database.SqlQuery<decimal>($@"
SELECT CAST(ISNULL(SUM(ISNULL(Amount, 0) - ISNULL(Consumed_Amount, 0)), 0) AS decimal(18,2)) AS Value FROM Customer_Order_Deposits WHERE Order_Id = {orderId}").AsEnumerable().First();

        private static SalesRep_Order_Assignment ActiveAssignment(operations opp, long repId, long orderId) =>
            opp.db.SalesRep_Order_Assignments.AsNoTracking().FirstOrDefault(x => x.Rep_Id == repId && x.Order_Id == orderId && x.Status == (int)SalesRepAssignmentStatus.Assigned)
            ?? throw new InvalidOperationException("الطلبية غير مسندة لهذا المندوب.");

        private static SalesRep_Rep Rep(operations opp, long repId) =>
            opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == repId) ?? throw new InvalidOperationException("المندوب غير موجود.");

        private T Read<T>(Func<operations, T> f) => f(_external ?? new operations());

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
