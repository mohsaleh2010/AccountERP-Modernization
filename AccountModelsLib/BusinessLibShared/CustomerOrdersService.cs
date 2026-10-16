using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    /// <summary>
    /// خدمة طلبيات العملاء الحديثة (جداول Customer_Orders / Customer_Order_Lines / Customer_Order_Deposits / Customer_Order_Fulfillments).
    /// لا علاقة لها بجدول Requests_Details القديم. المعرّفات تُولَّد من التطبيق (Common.GETAutoNumber) كبقية الجداول.
    /// </summary>
    public class CustomerOrdersService
    {
        /// <summary>
        /// اتصال خارجى اختيارى: عند تمريره تعمل الدوال (عدا SaveOrder) داخل معاملة المستدعى — مثل تسليم طلبية المندوب الذى يحفظ الفاتورة
        /// وربط التنفيذ وتسوية العربون فى معاملة واحدة. بلا تمرير: اتصال جديد لكل دالة كما كان.
        /// </summary>
        private readonly operations _external;
        public CustomerOrdersService(operations external = null) { _external = external; }
        private operations Op() => _external ?? new operations();

        /// <summary>يمنع التعديل بعد التنفيذ الجزئى أو الإلغاء أو الإغلاق.</summary>
        public static bool IsEditable(CustomerOrder order, bool hasFulfillments)
        {
            var status = (CustomerOrderStatus)(order.Status ?? 0);
            return !hasFulfillments && (status == CustomerOrderStatus.Draft || status == CustomerOrderStatus.Confirmed);
        }

        // ===================== التنفيذ: تحويل الطلبية إلى فاتورة مبيعات =====================

        /// <summary>كمية مطلوب تنفيذها من سطر طلبية فى فاتورة واحدة.</summary>
        public record Allocation(long OrderLineId, decimal Larg, decimal Small);

        /// <summary>المتبقى لسطر = المطلوب − المنفَّذ − الملغى (لا يقل عن صفر).</summary>
        public static decimal RemainingLarg(CustomerOrderLine l) => Math.Max(0, (l.Qty_Larg ?? 0) - (l.Fulfilled_Qty_Larg ?? 0) - (l.Cancelled_Qty_Larg ?? 0));
        public static decimal RemainingSmall(CustomerOrderLine l) => Math.Max(0, (l.Qty_Small ?? 0) - (l.Fulfilled_Qty_Small ?? 0) - (l.Cancelled_Qty_Small ?? 0));

        public static bool CanFulfill(CustomerOrder order)
        {
            var s = (CustomerOrderStatus)(order.Status ?? 0);
            return s == CustomerOrderStatus.Confirmed || s == CustomerOrderStatus.PartiallyFulfilled;
        }

        /// <summary>
        /// يسجّل ربط فاتورة مبيعات محفوظة بسطور الطلبية بعد نجاح حفظها (محرك الفاتورة نفسه لا يُمَس). لا يُسمح بتجاوز المتبقى،
        /// ثم تُطابَق الكميات مع سطور الفاتورة الفعلية ويُعاد حساب الطلبية.
        /// </summary>
        public void RecordFulfillments(long orderId, long invoiceListId, List<Allocation> allocations)
        {
            var opp = Op();
            var db = opp.db;
            var order = db.CustomerOrders.First(o => o.Id == orderId);
            if (!CanFulfill(order)) throw new InvalidOperationException("Order is not open for fulfillment");
            var lines = db.CustomerOrderLines.Where(l => l.Order_Id == orderId).ToDictionary(l => l.Id);
            var now = DateTime.Now;
            foreach (var a in allocations.Where(a => a.Larg > 0 || a.Small > 0))
            {
                if (!lines.TryGetValue(a.OrderLineId, out var line)) throw new InvalidOperationException("Order line not found");
                if (a.Larg > RemainingLarg(line) || a.Small > RemainingSmall(line))
                    throw new InvalidOperationException("Quantity exceeds the remaining quantity of the order");
                db.CustomerOrderFulfillments.Add(new CustomerOrderFulfillment
                {
                    Id = Common.GETAutoNumber(ref opp, "Customer_Order_Fulfillments", "Id", "") + (Common.Use_Auto_Identity_Column == 0 ? 0 : db.ChangeTracker.Entries<CustomerOrderFulfillment>().Count()),
                    Order_Line_Id = a.OrderLineId,
                    Invoices_List_Id = invoiceListId,
                    Qty_Larg = a.Larg,
                    Qty_Small = a.Small,
                    Fulfill_Date = now,
                    OperationDate = now,
                    User_Id = Common.User_Id,
                    Branch_Id = Common.Branch_Id,
                    Insert_ServerDateTime = now,
                    Update_ServerDateTime = now
                });
            }
            db.SaveChanges();
            SyncInvoice(invoiceListId);
        }

        /// <summary>
        /// يطابق روابط فاتورة مع سطورها الفعلية فى Invoices_Details (Invoice_Id = Invoices_List.Id): الفاتورة المحذوفة أو الصنف المحذوف
        /// منها يُرجع كمياته للطلبية، والكمية المُنقَصة تُقلِّص الربط (الأحدث أولاً). يُستدعى بعد حفظ/تعديل الفاتورة وعند فتح الطلبية.
        /// </summary>
        public void SyncInvoice(long invoiceListId)
        {
            var opp = Op();
            var db = opp.db;
            var links = db.CustomerOrderFulfillments.Where(f => f.Invoices_List_Id == invoiceListId).OrderByDescending(f => f.Id).ToList();
            if (links.Count == 0) return;

            var dt = opp.return_DataTable(
                "SELECT Unit_Item_Table_Id, SUM(ISNULL(Larg_Qnty,0)) AS L, SUM(ISNULL(Small_Qnty,0)) AS S FROM Invoices_Details " +
                "WHERE Invoice_Id = " + invoiceListId + " AND ISNULL(Item_DeletedFlage,0) <> 1 GROUP BY Unit_Item_Table_Id");
            var available = dt.Rows.Cast<System.Data.DataRow>().ToDictionary(
                r => Convert.ToInt64(r["Unit_Item_Table_Id"]),
                r => (L: Convert.ToDecimal(r["L"]), S: Convert.ToDecimal(r["S"])));

            var lineIds = links.Select(f => f.Order_Line_Id).Distinct().ToList();
            var lineUnit = db.CustomerOrderLines.Where(l => lineIds.Contains(l.Id)).ToDictionary(l => l.Id, l => (Unit: l.Unit_Item_Table_Id ?? 0, Order: l.Order_Id));

            // توزيع كمية الفاتورة المتاحة لكل صنف على الروابط (الأقدم أولاً يحتفظ بكميته).
            foreach (var group in links.GroupBy(f => lineUnit.TryGetValue(f.Order_Line_Id, out var u) ? u.Unit : 0))
            {
                available.TryGetValue(group.Key, out var av);
                var leftL = av.L;
                var leftS = av.S;
                foreach (var f in group.OrderBy(f => f.Id))
                {
                    var newL = Math.Min(f.Qty_Larg ?? 0, leftL);
                    var newS = Math.Min(f.Qty_Small ?? 0, leftS);
                    leftL -= newL;
                    leftS -= newS;
                    if (newL <= 0 && newS <= 0) { db.CustomerOrderFulfillments.Remove(f); continue; }
                    if (newL != (f.Qty_Larg ?? 0) || newS != (f.Qty_Small ?? 0))
                    {
                        f.Qty_Larg = newL;
                        f.Qty_Small = newS;
                        f.Update_ServerDateTime = DateTime.Now;
                    }
                }
            }
            db.SaveChanges();
            // فاتورة فقدت كل روابطها (حُذفت) يعود عربونها المسوَّى عليها متاحاً.
            if (!db.CustomerOrderFulfillments.Any(f => f.Invoices_List_Id == invoiceListId)) ReleaseInvoiceDeposits(invoiceListId);
            foreach (var orderId in lineUnit.Values.Select(v => v.Order).Distinct()) RecalculateOrder(orderId);
        }

        /// <summary>يطابق كل فواتير الطلبية ثم يعيد حساب المنفَّذ والحالة (يلتقط حذف/تعديل الفاتورة من أى شاشة، بما فيها الديسك توب).</summary>
        public void SyncOrder(long orderId)
        {
            var db = Op().db;
            var lineIds = db.CustomerOrderLines.Where(l => l.Order_Id == orderId).Select(l => l.Id).ToList();
            var invoiceIds = db.CustomerOrderFulfillments.Where(f => lineIds.Contains(f.Order_Line_Id) && f.Invoices_List_Id != null)
                .Select(f => f.Invoices_List_Id!.Value).Distinct().ToList();
            foreach (var inv in invoiceIds) SyncInvoice(inv);
            RecalculateOrder(orderId);
        }

        /// <summary>يعيد حساب المنفَّذ لكل سطر من جدول الروابط، ثم حالة السطر والطلبية (لا يغيّر الملغاة/المسودة).</summary>
        public void RecalculateOrder(long orderId)
        {
            var db = Op().db;
            var order = db.CustomerOrders.First(o => o.Id == orderId);
            var lines = db.CustomerOrderLines.Where(l => l.Order_Id == orderId).ToList();
            var lineIds = lines.Select(l => l.Id).ToList();
            var sums = db.CustomerOrderFulfillments.Where(f => lineIds.Contains(f.Order_Line_Id))
                .GroupBy(f => f.Order_Line_Id)
                .Select(g => new { g.Key, L = g.Sum(f => f.Qty_Larg ?? 0), S = g.Sum(f => f.Qty_Small ?? 0) })
                .ToDictionary(x => x.Key);

            var status = (CustomerOrderStatus)(order.Status ?? 0);
            var active = status != CustomerOrderStatus.Cancelled && status != CustomerOrderStatus.Draft;
            foreach (var l in lines)
            {
                sums.TryGetValue(l.Id, out var s);
                l.Fulfilled_Qty_Larg = s?.L ?? 0;
                l.Fulfilled_Qty_Small = s?.S ?? 0;
                if (!active) continue;
                var fulfilledAny = (l.Fulfilled_Qty_Larg ?? 0) > 0 || (l.Fulfilled_Qty_Small ?? 0) > 0;
                var closedAny = (l.Cancelled_Qty_Larg ?? 0) > 0 || (l.Cancelled_Qty_Small ?? 0) > 0;
                l.Line_Status = (int)(RemainingLarg(l) <= 0 && RemainingSmall(l) <= 0
                    ? (fulfilledAny || !closedAny ? CustomerOrderLineStatus.Fulfilled : CustomerOrderLineStatus.Cancelled)
                    : fulfilledAny ? CustomerOrderLineStatus.Partial : CustomerOrderLineStatus.Open);
            }

            if (active)
            {
                var anyFulfilled = lines.Any(l => (l.Fulfilled_Qty_Larg ?? 0) > 0 || (l.Fulfilled_Qty_Small ?? 0) > 0);
                var allDone = lines.All(l => RemainingLarg(l) <= 0 && RemainingSmall(l) <= 0);
                var anyClosed = lines.Any(l => (l.Cancelled_Qty_Larg ?? 0) > 0 || (l.Cancelled_Qty_Small ?? 0) > 0);
                order.Status = (int)(allDone
                    ? (anyClosed ? CustomerOrderStatus.Closed : CustomerOrderStatus.Fulfilled)
                    : anyFulfilled ? CustomerOrderStatus.PartiallyFulfilled : CustomerOrderStatus.Confirmed);
            }
            order.Update_ServerDateTime = DateTime.Now;
            db.SaveChanges();
        }

        /// <summary>«إغلاق المتبقى»: يسجّل المتبقى الحالى لكل سطر ككمية ملغاة بسبب إلزامى، فتُغلَق الطلبية.</summary>
        public void CloseRemaining(long orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("Reason is required");
            SyncOrder(orderId);
            var db = Op().db;
            var order = db.CustomerOrders.First(o => o.Id == orderId);
            if (!CanFulfill(order)) throw new InvalidOperationException("Order is not open");
            var now = DateTime.Now;
            foreach (var l in db.CustomerOrderLines.Where(l => l.Order_Id == orderId).ToList())
            {
                l.Cancelled_Qty_Larg = (l.Cancelled_Qty_Larg ?? 0) + RemainingLarg(l);
                l.Cancelled_Qty_Small = (l.Cancelled_Qty_Small ?? 0) + RemainingSmall(l);
                l.Update_ServerDateTime = now;
            }
            order.Cancel_Reason = reason.Trim();
            order.Cancelled_By = Common.User_Id;
            order.Cancelled_Date = now;
            order.Edit_User_Id = Common.User_Id;
            order.EditOperationDate = now;
            db.SaveChanges();
            RecalculateOrder(orderId);
        }

        /// <summary>الفواتير المنفِّذة للطلبية: رقم الفاتورة وتاريخها ومعرّف رأسها وسطر الطلبية وكميته.</summary>
        /// <summary>
        /// المتاح والمحجوز لكل Unit_item (صف الصنف فى مخزن): الرصيد الفعلى (Full_Qua كبير / D_Qua صغير) والمحجوز = المتبقى
        /// (المطلوب − المنفَّذ − الملغى) من الطلبيات المؤكدة/المنفَّذة جزئياً **بخلاف** الطلبية excludeOrderId. الحجز محسوب للعرض فقط ولا يُخزَّن.
        /// المتاح = الرصيد − المحجوز.
        /// </summary>
        public System.Data.DataTable GetAvailability(IEnumerable<long> unitItemIds, long excludeOrderId)
        {
            var ids = string.Join(",", unitItemIds.Where(i => i > 0).Distinct());
            if (ids.Length == 0) return new System.Data.DataTable();
            string Res(string q, string f, string c) =>
                "ISNULL((SELECT SUM(ISNULL(l." + q + ",0) - ISNULL(l." + f + ",0) - ISNULL(l." + c + ",0)) FROM Customer_Order_Lines l " +
                "INNER JOIN Customer_Orders o ON o.Id = l.Order_Id WHERE l.Unit_Item_Table_Id = u.id AND o.Status IN (" +
                (int)CustomerOrderStatus.Confirmed + "," + (int)CustomerOrderStatus.PartiallyFulfilled + ") AND o.Id <> " + excludeOrderId +
                " AND ISNULL(l.Line_Status,0) <> " + (int)CustomerOrderLineStatus.Cancelled + "),0)";
            return Op().return_DataTable(
                "SELECT u.id AS Unit_Item_Id, ISNULL(u.Full_Qua,0) AS Bal_Larg, ISNULL(u.D_Qua,0) AS Bal_Small, " +
                Res("Qty_Larg", "Fulfilled_Qty_Larg", "Cancelled_Qty_Larg") + " AS Res_Larg, " +
                Res("Qty_Small", "Fulfilled_Qty_Small", "Cancelled_Qty_Small") + " AS Res_Small " +
                "FROM Unit_item u WHERE u.id IN (" + ids + ")");
        }

        public System.Data.DataTable GetOrderInvoices(long orderId)
        {
            return Op().return_DataTable(
                "SELECT f.Order_Line_Id, f.Invoices_List_Id, i.Invoice_Id AS Invoice_No, i.Invoice_Date, ISNULL(i.Net_Total,0) AS Net_Total, ISNULL(f.Qty_Larg,0) AS Qty_Larg, ISNULL(f.Qty_Small,0) AS Qty_Small " +
                "FROM Customer_Order_Fulfillments f INNER JOIN Customer_Order_Lines l ON l.Id = f.Order_Line_Id " +
                "LEFT JOIN Invoices_List i ON i.Id = f.Invoices_List_Id WHERE l.Order_Id = " + orderId + " ORDER BY i.Invoice_Date, i.Invoice_Id");
        }

        public (CustomerOrder Order, List<CustomerOrderLine> Lines)? GetOrder(long id)
        {
            var opp = Op();
            var order = opp.db.CustomerOrders.FirstOrDefault(o => o.Id == id);
            if (order == null) return null;
            var lines = opp.db.CustomerOrderLines.Where(l => l.Order_Id == id).OrderBy(l => l.Line_No).ToList();
            return (order, lines);
        }

        /// <summary>قائمة المتابعة: كل الفلاتر اختيارية. status = -1 لكل الحالات. overdueOnly = متأخرة عن تاريخ التسليم ولم تُنفَّذ/تُلغَ.</summary>
        public System.Data.DataTable ListOrders(DateTime from, DateTime to, long customerId, int status, bool paidOnly, bool overdueOnly, string customerNameLike)
        {
            var sql = new System.Text.StringBuilder();
            sql.Append("SELECT o.Id, o.Order_No, o.Order_Date, o.Expected_Delivery_Date, o.Customer_Id, c.Name AS Customer_Name, o.Status, ");
            sql.Append("ISNULL(o.Net_Total,0) AS Net_Total, ");
            sql.Append("ISNULL((SELECT SUM(d.Amount) FROM Customer_Order_Deposits d WHERE d.Order_Id = o.Id),0) AS Deposits, ");
            sql.Append("ISNULL((SELECT SUM(ISNULL(l.Qty_Larg,0)+ISNULL(l.Qty_Small,0)) FROM Customer_Order_Lines l WHERE l.Order_Id = o.Id),0) AS Qty_Total, ");
            sql.Append("ISNULL((SELECT SUM(ISNULL(l.Fulfilled_Qty_Larg,0)+ISNULL(l.Fulfilled_Qty_Small,0)) FROM Customer_Order_Lines l WHERE l.Order_Id = o.Id),0) AS Qty_Fulfilled ");
            sql.Append("FROM Customer_Orders o LEFT JOIN Customer c ON c.Id = o.Customer_Id ");
            sql.Append("WHERE o.Order_Date >= '" + operations.dateFormat(from.Date) + "' AND o.Order_Date < '" + operations.dateFormat(to.Date.AddDays(1)) + "' ");
            if (customerId > 0) sql.Append("AND o.Customer_Id = " + customerId + " ");
            if (status >= 0) sql.Append("AND o.Status = " + status + " ");
            if (paidOnly) sql.Append("AND EXISTS (SELECT 1 FROM Customer_Order_Deposits d WHERE d.Order_Id = o.Id AND ISNULL(d.Amount,0) > 0) ");
            if (overdueOnly)
                sql.Append("AND o.Expected_Delivery_Date < '" + operations.dateFormat(DateTime.Now.Date) + "' AND o.Status IN (" + (int)CustomerOrderStatus.Confirmed + "," + (int)CustomerOrderStatus.PartiallyFulfilled + ") ");
            if (!string.IsNullOrWhiteSpace(customerNameLike))
                sql.Append("AND c.Name LIKE N'%" + customerNameLike.Trim().Replace("'", "''") + "%' ");
            sql.Append("ORDER BY o.Order_Date DESC, o.Order_No DESC");
            return Op().return_DataTable(sql.ToString());
        }

        /// <summary>
        /// تقرير الأصناف المفتوحة: لكل صنف-مخزن المتبقى توريده (المطلوب − المنفَّذ − الملغى) من الطلبيات المؤكدة/المنفَّذة جزئياً، عدد الطلبيات،
        /// والرصيد الحالى (كبير/صغير) ليظهر العجز = المتبقى − الرصيد. فلاتر نصية اختيارية (تُهرَّب الفاصلة العليا).
        /// </summary>
        public System.Data.DataTable OpenItemsReport(string itemLike, string customerLike, long storeId)
        {
            var rem = "ISNULL(l.Qty_Larg,0) - ISNULL(l.Fulfilled_Qty_Larg,0) - ISNULL(l.Cancelled_Qty_Larg,0)";
            var remS = "ISNULL(l.Qty_Small,0) - ISNULL(l.Fulfilled_Qty_Small,0) - ISNULL(l.Cancelled_Qty_Small,0)";
            var sql = new System.Text.StringBuilder();
            sql.Append("SELECT u.id AS Unit_Item_Id, i.Item_Name, un.B_Unit, un.S_unit, st.Store_Name, ");
            sql.Append("SUM(" + rem + ") AS Rem_Larg, SUM(" + remS + ") AS Rem_Small, COUNT(DISTINCT o.Id) AS Orders_Count, ");
            sql.Append("ISNULL(MAX(u.Full_Qua),0) AS Bal_Larg, ISNULL(MAX(u.D_Qua),0) AS Bal_Small ");
            sql.Append("FROM Customer_Order_Lines l INNER JOIN Customer_Orders o ON o.Id = l.Order_Id ");
            sql.Append("INNER JOIN Unit_item u ON u.id = l.Unit_Item_Table_Id INNER JOIN Item i ON i.id = u.Item_id ");
            sql.Append("LEFT JOIN unit un ON un.id = u.unit_id LEFT JOIN Store st ON st.id = u.Store_id LEFT JOIN Customer c ON c.Id = o.Customer_Id ");
            sql.Append("WHERE o.Status IN (" + (int)CustomerOrderStatus.Confirmed + "," + (int)CustomerOrderStatus.PartiallyFulfilled + ") ");
            sql.Append("AND ISNULL(l.Line_Status,0) <> " + (int)CustomerOrderLineStatus.Cancelled + " ");
            sql.Append("AND ((" + rem + ") > 0 OR (" + remS + ") > 0) ");
            if (storeId > 0) sql.Append("AND u.Store_id = " + storeId + " ");
            if (!string.IsNullOrWhiteSpace(itemLike)) sql.Append("AND i.Item_Name LIKE N'%" + itemLike.Trim().Replace("'", "''") + "%' ");
            if (!string.IsNullOrWhiteSpace(customerLike)) sql.Append("AND c.Name LIKE N'%" + customerLike.Trim().Replace("'", "''") + "%' ");
            sql.Append("GROUP BY u.id, i.Item_Name, un.B_Unit, un.S_unit, st.Store_Name ORDER BY i.Item_Name");
            return Op().return_DataTable(sql.ToString());
        }

        /// <summary>
        /// يربط إذن استلام نقدية (سطر Money_Transaction_Box) بطلبية عميل كعربون. لا يُنشئ حركة نقدية (تُنشأ من إذن النقدية نفسه بمحركها الأصلى)؛
        /// يتحقق من أن الطلبية لنفس العميل وأنها غير ملغاة/مغلقة، ومن عدم ربط السطر مرتين.
        /// </summary>
        public long AddDeposit(long orderId, long orderMoneyId, long moneyBoxLineId, long customerId, decimal amount, DateTime date, string notes)
        {
            if (amount <= 0) throw new InvalidOperationException("Amount must be greater than zero");
            var opp = Op();
            var order = opp.db.CustomerOrders.FirstOrDefault(o => o.Id == orderId) ?? throw new InvalidOperationException("Order not found");
            if (order.Customer_Id != customerId) throw new InvalidOperationException("Order belongs to another customer");
            var status = (CustomerOrderStatus)(order.Status ?? 0);
            if (status == CustomerOrderStatus.Cancelled || status == CustomerOrderStatus.Closed)
                throw new InvalidOperationException("Order is cancelled or closed");
            if (orderMoneyId > 0 && opp.db.CustomerOrderDeposits.Any(d => d.Order_Money_Id == orderMoneyId))
                throw new InvalidOperationException("This receipt is already linked to an order");
            var now = DateTime.Now;
            var dep = new CustomerOrderDeposit
            {
                Id = Common.GETAutoNumber(ref opp, "Customer_Order_Deposits", "Id", ""),
                Order_Id = orderId,
                Money_Transaction_Box_Id = moneyBoxLineId,
                Order_Money_Id = orderMoneyId,
                Amount = amount,
                Consumed_Amount = 0,
                Deposit_Date = date,
                Notes = notes,
                Branch_Id = Common.Branch_Id,
                User_Id = Common.User_Id,
                OperationDate = now,
                Insert_ServerDateTime = now,
                Update_ServerDateTime = now
            };
            opp.db.CustomerOrderDeposits.Add(dep);
            opp.db.SaveChanges();
            return dep.Id;
        }

        // ================= ربط عربون الطلبية بالفاتورة المنفِّذة =================

        /// <summary>
        /// العربون المتبقى للطلبية (الإجمالى - المستهلك) ومجموع ما سُوِّي منه على كل فاتورة. يعيد (باقى العربون، جدول الفواتير: Invoices_List_Id, Applied).
        /// </summary>
        public (decimal Remaining, System.Data.DataTable PerInvoice) GetDepositBalance(long orderId)
        {
            var opp = Op();
            var t = opp.return_DataTable("SELECT ISNULL(SUM(ISNULL(Amount,0) - ISNULL(Consumed_Amount,0)),0) AS R FROM Customer_Order_Deposits WHERE Order_Id = " + orderId);
            var per = opp.return_DataTable("SELECT Invoices_List_Id, SUM(Amount) AS Applied FROM Customer_Order_Deposit_Applications WHERE Order_Id = " + orderId + " GROUP BY Invoices_List_Id");
            return (Convert.ToDecimal(t.Rows[0][0]), per);
        }

        /// <summary>
        /// تسوية عربون الطلبية على فاتورة نفّذت منها: يُستهلك العربون الأقدم أولاً بما لا يتجاوز صافى الفاتورة ولا باقى العربون ولا ما سُوِّى عليها سابقاً.
        /// لا يمس حركة النقدية ولا رصيد العميل (العربون قُيِّد له دائناً وقت الاستلام، والفاتورة تُقيَّد عليه مديناً — فالرصيد يتسوى من القيدين).
        /// هذا ربط تتبّع فقط. يعيد المبلغ المسوَّى.
        /// </summary>
        public decimal ApplyDepositsToInvoice(long orderId, long invoiceListId, decimal? requested = null)
        {
            var opp = Op();
            var fulfilled = opp.return_DataTable("SELECT COUNT(*) FROM Customer_Order_Fulfillments f INNER JOIN Customer_Order_Lines l ON l.Id = f.Order_Line_Id WHERE l.Order_Id = " + orderId + " AND f.Invoices_List_Id = " + invoiceListId);
            if (Convert.ToInt32(fulfilled.Rows[0][0]) == 0) throw new InvalidOperationException("Invoice is not linked to this order");
            var inv = opp.return_DataTable("SELECT ISNULL(Net_Total,0) AS Net FROM Invoices_List WHERE Id = " + invoiceListId);
            if (inv.Rows.Count == 0) throw new InvalidOperationException("Invoice not found");
            var net = Convert.ToDecimal(inv.Rows[0][0]);
            var already = Convert.ToDecimal(opp.return_DataTable("SELECT ISNULL(SUM(Amount),0) FROM Customer_Order_Deposit_Applications WHERE Invoices_List_Id = " + invoiceListId).Rows[0][0]);
            var deposits = opp.return_DataTable("SELECT Id, ISNULL(Amount,0) - ISNULL(Consumed_Amount,0) AS Rest FROM Customer_Order_Deposits WHERE Order_Id = " + orderId + " AND ISNULL(Amount,0) - ISNULL(Consumed_Amount,0) > 0 ORDER BY Deposit_Date, Id");
            var available = deposits.Rows.Cast<System.Data.DataRow>().Sum(r => Convert.ToDecimal(r["Rest"]));
            var toApply = Math.Min(Math.Min(net - already, available), requested ?? decimal.MaxValue);
            if (toApply <= 0) return 0;

            decimal left = toApply;
            var now = DateTime.Now;
            foreach (System.Data.DataRow r in deposits.Rows)
            {
                if (left <= 0) break;
                var take = Math.Min(left, Convert.ToDecimal(r["Rest"]));
                var depId = Convert.ToInt64(r["Id"]);
                var id = Common.GETAutoNumber(ref opp, "Customer_Order_Deposit_Applications", "Id", "");
                var ci = System.Globalization.CultureInfo.InvariantCulture;
                opp.insert_me("INSERT INTO Customer_Order_Deposit_Applications (Id, Deposit_Id, Order_Id, Invoices_List_Id, Amount, Applied_Date, User_Id, Branch_Id, Insert_ServerDateTime) VALUES (" +
                    id + "," + depId + "," + orderId + "," + invoiceListId + "," + take.ToString(ci) + ",'" + operations.dateFormat(now) + "'," + Common.User_Id + "," + Common.Branch_Id + ",'" + operations.dateFormat(now) + "')");
                opp.update_me("UPDATE Customer_Order_Deposits SET Consumed_Amount = ISNULL(Consumed_Amount,0) + " + take.ToString(ci) + ", Update_ServerDateTime = '" + operations.dateFormat(now) + "' WHERE Id = " + depId);
                left -= take;
            }
            return toApply;
        }

        /// <summary>يلغى تسوية العربون على فاتورة (عند حذفها/تصحيح الربط) فيعود العربون متاحاً.</summary>
        public void ReleaseInvoiceDeposits(long invoiceListId)
        {
            var opp = Op();
            var apps = opp.return_DataTable("SELECT Deposit_Id, SUM(Amount) AS A FROM Customer_Order_Deposit_Applications WHERE Invoices_List_Id = " + invoiceListId + " GROUP BY Deposit_Id");
            foreach (System.Data.DataRow r in apps.Rows)
                opp.update_me("UPDATE Customer_Order_Deposits SET Consumed_Amount = CASE WHEN ISNULL(Consumed_Amount,0) - " + Convert.ToDecimal(r["A"]).ToString(System.Globalization.CultureInfo.InvariantCulture) +
                              " < 0 THEN 0 ELSE ISNULL(Consumed_Amount,0) - " + Convert.ToDecimal(r["A"]).ToString(System.Globalization.CultureInfo.InvariantCulture) + " END WHERE Id = " + Convert.ToInt64(r["Deposit_Id"]));
            opp.delete_me("DELETE FROM Customer_Order_Deposit_Applications WHERE Invoices_List_Id = " + invoiceListId);
        }

        /// <summary>عربون الطلبية: المبلغ المدفوع والمستهلك (للعرض فى شاشة الطلبية).</summary>
        public System.Data.DataTable GetDeposits(long orderId)
        {
            return Op().return_DataTable(
                "SELECT d.Id, d.Deposit_Date, ISNULL(d.Amount,0) AS Amount, ISNULL(d.Consumed_Amount,0) AS Consumed_Amount, d.Order_Money_Id, o.Order_No " +
                "FROM Customer_Order_Deposits d LEFT JOIN Order_Money o ON o.Id = d.Order_Money_Id WHERE d.Order_Id = " + orderId + " ORDER BY d.Deposit_Date, d.Id");
        }

        /// <summary>طلبيات عميل قابلة لاستلام عربون (مؤكدة/جزئية/مسودة)، للقائمة المنسدلة فى إذن الاستلام.</summary>
        public System.Data.DataTable GetOrdersForDeposit(long customerId)
        {
            return Op().return_DataTable(
                "SELECT o.Id, o.Order_No, o.Order_Date, ISNULL(o.Net_Total,0) AS Net_Total, " +
                "ISNULL((SELECT SUM(d.Amount) FROM Customer_Order_Deposits d WHERE d.Order_Id = o.Id),0) AS Deposits " +
                "FROM Customer_Orders o WHERE o.Customer_Id = " + customerId + " AND o.Status IN (" + (int)CustomerOrderStatus.Draft + "," + (int)CustomerOrderStatus.Confirmed + "," +
                (int)CustomerOrderStatus.PartiallyFulfilled + ") ORDER BY o.Order_Date DESC, o.Order_No DESC");
        }

        /// <summary>تأكيد مسودة.</summary>
        public void Confirm(long orderId)
        {
            var opp = Op();
            var order = opp.db.CustomerOrders.First(o => o.Id == orderId);
            if ((CustomerOrderStatus)(order.Status ?? 0) != CustomerOrderStatus.Draft)
                throw new InvalidOperationException("Only draft orders can be confirmed");
            order.Status = (int)CustomerOrderStatus.Confirmed;
            order.Edit_User_Id = Common.User_Id;
            order.EditOperationDate = DateTime.Now;
            order.Update_ServerDateTime = DateTime.Now;
            opp.db.SaveChanges();
        }

        /// <summary>إلغاء بسبب (لا حذف فعلى). ممنوع بعد أى تنفيذ.</summary>
        public void Cancel(long orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("Cancel reason is required");
            if (HasFulfillments(orderId)) throw new InvalidOperationException("Order already has fulfillments");
            var opp = Op();
            if (IsAssignedToRep(opp, orderId)) throw new InvalidOperationException("الطلبية مسندة لمندوب؛ ألغِ الإسناد من «طلبيات المندوبين» أولاً.");
            var order = opp.db.CustomerOrders.First(o => o.Id == orderId);
            var status = (CustomerOrderStatus)(order.Status ?? 0);
            if (status == CustomerOrderStatus.Cancelled || status == CustomerOrderStatus.Closed || status == CustomerOrderStatus.Fulfilled)
                throw new InvalidOperationException("Order can no longer be cancelled");
            var now = DateTime.Now;
            order.Status = (int)CustomerOrderStatus.Cancelled;
            order.Cancel_Reason = reason.Trim();
            order.Cancelled_By = Common.User_Id;
            order.Cancelled_Date = now;
            order.Edit_User_Id = Common.User_Id;
            order.EditOperationDate = now;
            order.Update_ServerDateTime = now;
            foreach (var l in opp.db.CustomerOrderLines.Where(l => l.Order_Id == orderId))
            {
                l.Line_Status = (int)CustomerOrderLineStatus.Cancelled;
                l.Update_ServerDateTime = now;
            }
            opp.db.SaveChanges();
        }

        /// <summary>الطلبية مسندة لمندوب يسلّمها (خطة المندوبين المرحلة 5): أسطرها على مخزن السيارة فلا تُعدَّل/تُلغى حتى يُلغى الإسناد.</summary>
        public static bool IsAssignedToRep(operations opp, long orderId) => opp.return_DataTable(
            "IF OBJECT_ID(N'dbo.SalesRep_Order_Assignments', N'U') IS NOT NULL SELECT COUNT(*) FROM SalesRep_Order_Assignments WHERE Status = 0 AND Order_Id = " + orderId +
            " ELSE SELECT 0").Rows[0][0] is object v && Convert.ToInt32(v) > 0;

        public bool HasFulfillments(long orderId)
        {
            var opp = Op();
            var lineIds = opp.db.CustomerOrderLines.Where(l => l.Order_Id == orderId).Select(l => l.Id).ToList();
            return opp.db.CustomerOrderFulfillments.Any(f => lineIds.Contains(f.Order_Line_Id));
        }

        /// <summary>
        /// يحفظ طلبية جديدة (header.Id == 0) أو يعدّل طلبية قائمة قابلة للتعديل، بسطورها فى معاملة واحدة، ويعيد رقم السجل ورقم الطلبية.
        /// confirm=true يجعل الحالة «مؤكدة»، وإلا «مسودة».
        /// </summary>
        public (long Id, long OrderNo) SaveOrder(CustomerOrder header, List<CustomerOrderLine> lines, bool confirm)
        {
            if (header.Customer_Id == null || header.Customer_Id <= 0) throw new InvalidOperationException("Customer is required");
            if (lines == null || lines.Count == 0) throw new InvalidOperationException("At least one line is required");

            var opp = new operations();
            var db = opp.db;
            var now = DateTime.Now;
            var branchId = Common.Branch_Id;
            var userId = Common.User_Id;
            var isNew = header.Id == 0;
            // لا علاقة (navigation) معرّفة فى الموديل فلا يرتّب EF الإدراج تلقائياً: نحفظ الرأس أولاً ثم السطور، كلها داخل معاملة واحدة.
            using var tx = db.Database.BeginTransaction();

            CustomerOrder order;
            if (isNew)
            {
                order = new CustomerOrder
                {
                    Id = Common.GETAutoNumber(ref opp, "Customer_Orders", "Id", ""),
                    Branch_Id = branchId,
                    User_Id = userId,
                    OperationDate = now,
                    Insert_ServerDateTime = now
                };
                var year = (header.Order_Date ?? now).Year;
                var last = db.CustomerOrders
                    .Where(o => o.Branch_Id == branchId && o.Order_Date != null && o.Order_Date.Value.Year == year)
                    .Max(o => o.Order_No) ?? 0;
                order.Order_No = last + 1;
                db.CustomerOrders.Add(order);
            }
            else
            {
                order = db.CustomerOrders.First(o => o.Id == header.Id);
                if (!IsEditable(order, HasFulfillments(order.Id)))
                    throw new InvalidOperationException("Order can no longer be edited");
                if (IsAssignedToRep(opp, order.Id))
                    throw new InvalidOperationException("الطلبية مسندة لمندوب (أسطرها على مخزن سيارته)؛ ألغِ الإسناد من «طلبيات المندوبين» قبل التعديل.");
                order.Edit_User_Id = userId;
                order.EditOperationDate = now;
                db.CustomerOrderLines.RemoveRange(db.CustomerOrderLines.Where(l => l.Order_Id == order.Id));
            }

            order.Order_Date = header.Order_Date ?? now.Date;
            order.Expected_Delivery_Date = header.Expected_Delivery_Date;
            order.Customer_Id = header.Customer_Id;
            order.Store_Id = header.Store_Id;
            order.Currency_Id = header.Currency_Id;
            order.Sales_Person_Id = header.Sales_Person_Id;
            order.Notes = header.Notes;
            order.Status = (int)(confirm ? CustomerOrderStatus.Confirmed : CustomerOrderStatus.Draft);
            order.Update_ServerDateTime = now;
            db.SaveChanges();

            var lineNo = 0;
            var lineIdBase = Common.Use_Auto_Identity_Column == 0 ? 0 : Common.GETAutoNumber(ref opp, "Customer_Order_Lines", "Id", "");
            foreach (var src in lines)
            {
                lineNo++;
                var lineTotal = src.Line_Total ?? 0;
                db.CustomerOrderLines.Add(new CustomerOrderLine
                {
                    // Snowflake: معرّف جديد لكل سطر. غير ذلك (Max+1): لم يُحفظ شىء بعد فنزيد على الأساس بدل إعادة الاستعلام.
                    Id = Common.Use_Auto_Identity_Column == 0
                        ? Common.GETAutoNumber(ref opp, "Customer_Order_Lines", "Id", "")
                        : lineIdBase + lineNo - 1,
                    Order_Id = order.Id,
                    Line_No = lineNo,
                    Unit_Item_Table_Id = src.Unit_Item_Table_Id,
                    Qty_Larg = src.Qty_Larg ?? 0,
                    Qty_Small = src.Qty_Small ?? 0,
                    Unit_Price = src.Unit_Price ?? 0,
                    Discount_Percent = src.Discount_Percent ?? 0,
                    Discount_Value = src.Discount_Value ?? 0,
                    Tax_Value = src.Tax_Value ?? 0,
                    Line_Total = lineTotal,
                    Fulfilled_Qty_Larg = 0,
                    Fulfilled_Qty_Small = 0,
                    Cancelled_Qty_Larg = 0,
                    Cancelled_Qty_Small = 0,
                    Line_Status = (int)CustomerOrderLineStatus.Open,
                    Notes = src.Notes,
                    Branch_Id = branchId,
                    User_Id = userId,
                    OperationDate = now,
                    Insert_ServerDateTime = now,
                    Update_ServerDateTime = now
                });
            }

            order.Gross_Total = lines.Sum(l => (l.Line_Total ?? 0) + (l.Discount_Value ?? 0));
            order.Discount_Total = lines.Sum(l => l.Discount_Value ?? 0);
            order.Tax_Total = lines.Sum(l => l.Tax_Value ?? 0);
            order.Net_Total = lines.Sum(l => l.Line_Total ?? 0);

            db.SaveChanges();
            tx.Commit();
            return (order.Id, order.Order_No ?? 0);
        }
    }
}
