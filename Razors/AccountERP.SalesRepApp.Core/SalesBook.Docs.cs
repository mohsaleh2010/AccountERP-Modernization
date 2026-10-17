namespace AccountERP.SalesRepApp.Core
{
    /// <summary>
    /// بقية مستندات المندوب على الجهاز: المرتجع (مربوط بفاتورته أو بلا فاتورة)، أخذ طلبية، تسليم طلبية مسندة، تحصيل شيك، وعدم البيع.
    /// نفس المبدأ: فحص محلى يمنع الخطأ الواضح، وما يحتاج موافقة تحذير يصير «بانتظار المراجعة» على السيرفر.
    /// </summary>
    public sealed partial class SalesBook
    {
        public List<ReasonRow> ReturnReasons => _store.Rows<ReasonRow>("return_reasons").OrderBy(r => r.Name).ToList();
        public List<ReasonRow> NoSaleReasons => _store.Rows<ReasonRow>("nosale_reasons").OrderBy(r => r.Name).ToList();
        public List<BankRow> Banks => _store.Rows<BankRow>("banks").OrderBy(b => b.Name).ToList();
        public List<OrderItemRow> OrderItems => _store.Rows<OrderItemRow>("order_items").OrderBy(i => i.Item_Name).ToList();

        /// <summary>فواتير العميل القابلة للإرجاع، والكمية القابلة = المتبقى على السيرفر − مرتجعات الجهاز غير المحفوظة.</summary>
        public List<InvoiceView> ReturnableInvoices(long customerId)
        {
            var pending = PendingReturned();
            var list = _store.Rows<InvoiceView>("invoices").Where(i => i.Customer_Id == customerId).OrderByDescending(i => i.Invoice_Date).ToList();
            foreach (var inv in list)
                foreach (var l in inv.Lines)
                    if (pending.TryGetValue(l.Original_Detail_Id, out decimal p)) l.Returned_Small += p;
            return list.Where(i => i.Lines.Any(l => l.Returnable_Small > 0)).ToList();
        }

        private Dictionary<long, decimal> PendingReturned()
        {
            var map = new Dictionary<long, decimal>();
            var factors = _store.Rows<InvoiceView>("invoices").SelectMany(i => i.Lines).ToDictionary(l => l.Original_Detail_Id, l => l.Factor <= 0 ? 1 : l.Factor);
            foreach (var o in _store.Outbox(openOnly: true).Where(o => o.Doc_Type == DocType.Return))
                if (Json.Deserialize<ReturnInput>(o.Payload) is { } r)
                    foreach (var l in r.Lines.Where(l => l.Original_Detail_Id != null))
                    {
                        long id = l.Original_Detail_Id!.Value;
                        decimal f = factors.TryGetValue(id, out var x) ? x : 1;
                        map[id] = (map.TryGetValue(id, out var v) ? v : 0) + l.Qty_Large * f + l.Qty_Small;
                    }
            return map;
        }

        // ============================== المرتجع ==============================

        public SaleCheck CheckReturn(ReturnInput input)
        {
            var r = new SaleCheck();
            var profile = Profile;
            CommonChecks(r, profile, input.Customer_Id);
            if (input.Lines.Count == 0) r.Problems.Add("أضف صنفاً واحداً على الأقل.");
            var reasons = ReturnReasons.ToDictionary(x => x.Id);
            InvoiceView? inv = null;
            if (input.Original_Invoice_Id is long invId)
            {
                inv = ReturnableInvoices(input.Customer_Id).FirstOrDefault(i => i.Id == invId);
                if (inv == null) r.Problems.Add("الفاتورة غير متاحة للإرجاع لهذا العميل.");
                else if (inv.Age_Days > inv.Window_Days) r.Warnings.Add($"الفاتورة عمرها {inv.Age_Days} يوماً خارج نافذة الإرجاع ({inv.Window_Days})؛ سيحتاج موافقة المشرف.");
            }
            else if (profile is { Allow_Blind_Return: false }) r.Warnings.Add("المرتجع بلا فاتورة غير مسموح لك؛ سيحتاج موافقة المشرف.");
            foreach (var l in input.Lines)
            {
                string name;
                if (inv != null)
                {
                    var src = inv.Lines.FirstOrDefault(x => x.Original_Detail_Id == l.Original_Detail_Id);
                    if (src == null) { r.Problems.Add("سطر ليس من الفاتورة."); continue; }
                    name = src.Item_Name;
                    decimal f = src.Factor <= 0 ? 1 : src.Factor;
                    decimal want = l.Qty_Large * f + l.Qty_Small;
                    if (want > src.Returnable_Small) r.Problems.Add($"المرتجع من «{name}» أكبر من المتبقى فى الفاتورة.");
                    r.Net += want * src.Net_Unit_Price;
                }
                else
                {
                    var item = l.Unit_Item_Id is long u ? Item(u) : null;
                    if (item == null) { r.Problems.Add("صنف غير موجود على السيارة."); continue; }
                    name = item.Item_Name;
                }
                if (l.Qty_Large < 0 || l.Qty_Small < 0 || l.Qty_Large + l.Qty_Small <= 0) r.Problems.Add($"كمية «{name}» غير صحيحة.");
                if (!reasons.TryGetValue(l.Reason_Id, out var reason)) r.Problems.Add($"اختر سبب إرجاع «{name}».");
                else if (reason.Requires_Photo && (l.Photo == null || l.Photo.Length == 0)) r.Problems.Add($"سبب «{reason.Name}» يتطلب صورة لـ «{name}».");
                if (l.Condition is not (0 or 1)) r.Problems.Add($"حدد حالة «{name}».");
                if (l.Photo is { Length: > 2_000_000 }) r.Problems.Add($"صورة «{name}» أكبر من 2 ميجا.");
            }
            r.Net = Math.Round(r.Net, 2);
            return r;
        }

        public Guid SaveReturn(ReturnInput input)
        {
            var c = CheckReturn(input);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            return Enqueue(DocType.Return, Json.Serialize(input), input.Customer_Id, c.Net);
        }

        // ============================== أخذ طلبية ==============================

        /// <summary>يحسب أعمدة السطر كما يرسلها شاشة الطلبية للسيرفر: الإجمالى، الضريبة بعد الخصم، والصافى.</summary>
        public static TakeOrderLine PriceOrderLine(TakeOrderLine l, decimal factor, decimal taxRate)
        {
            decimal f = factor <= 0 ? 1 : factor;
            l.Gross = Math.Round((l.Qty_Large + l.Qty_Small / f) * l.Unit_Price, 2);
            l.Discount_Value = Math.Round(Math.Min(l.Discount_Value, l.Gross), 2);
            l.Tax_Value = Math.Round((l.Gross - l.Discount_Value) * taxRate / 100, 2);
            l.Line_Total = l.Gross - l.Discount_Value + l.Tax_Value;
            return l;
        }

        public SaleCheck CheckOrder(TakeOrderInput input, DateTime? today = null)
        {
            var r = new SaleCheck();
            var profile = Profile;
            CommonChecks(r, profile, input.Customer_Id);
            var items = OrderItems.ToDictionary(i => i.Id);
            if (input.Source_Store_Id <= 0) r.Problems.Add("لا مخزن مصدر للطلبيات؛ زامِن أولاً.");
            if (input.Lines.Count == 0) r.Problems.Add("أضف صنفاً واحداً على الأقل.");
            foreach (var l in input.Lines)
            {
                if (!items.TryGetValue(l.Unit_Item_Id, out var it)) { r.Problems.Add("صنف غير موجود فى المخزن المصدر."); continue; }
                if (l.Qty_Large < 0 || l.Qty_Small < 0 || l.Qty_Large + l.Qty_Small <= 0) r.Problems.Add($"كمية «{it.Item_Name}» غير صحيحة.");
                if (l.Unit_Price < 0 || l.Discount_Value < 0) r.Problems.Add($"سعر أو خصم «{it.Item_Name}» غير صحيح.");
                decimal want = l.Qty_Large * (it.Factor <= 0 ? 1 : it.Factor) + l.Qty_Small;
                if (want > it.Available_Small) r.Warnings.Add($"المتاح من «{it.Item_Name}» فى {it.Store_Name} {Math.Max(0, it.Available_Small):0.##} أقل من المطلوب؛ تُسجَّل الطلبية وينبَّه المخزن.");
                decimal rate = l.Gross > 0 ? l.Discount_Value / l.Gross * 100 : 0;
                if (profile?.Max_Discount_Rate is decimal max && rate > max) r.Warnings.Add($"خصم «{it.Item_Name}» {rate:0.##}% فوق حدك {max:0.##}%؛ سيحتاج موافقة المشرف.");
                r.Net += l.Line_Total;
            }
            r.Net = Math.Round(r.Net, 2);
            if (input.Deposit < 0 || input.Deposit > r.Net) r.Problems.Add("العربون بين صفر وصافى الطلبية.");
            if (input.Expected_Delivery_Date is DateTime d && d.Date < (today ?? _now()).Date) r.Problems.Add("تاريخ التسليم المتوقع قبل اليوم.");
            if (input.Deposit > 0) CashLimit(r, profile, input.Deposit_Payment_Method_Id, input.Deposit);
            return r;
        }

        public Guid SaveOrder(TakeOrderInput input)
        {
            var c = CheckOrder(input);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            return Enqueue(DocType.TakeOrder, Json.Serialize(input), input.Customer_Id, c.Net);
        }

        // ============================== التسليم ==============================

        /// <summary>الطلبيات المسندة التى لم يُسلَّم لها شىء على الجهاز بعد (تسليم معلّق يخفيها حتى يحسمها السيرفر).</summary>
        public List<OrderSync> OpenOrders()
        {
            var busy = _store.Outbox(openOnly: true).Where(o => o.Doc_Type == DocType.Delivery)
                .Select(o => Json.Deserialize<DeliveryInput>(o.Payload)?.Order_Id ?? 0).ToHashSet();
            return _store.Rows<OrderSync>("orders").Where(o => !busy.Contains(o.Order.Id))
                .OrderBy(o => o.Order.Expected_Delivery_Date ?? o.Order.Order_Date).ToList();
        }

        public SaleCheck CheckDelivery(DeliveryInput input)
        {
            var r = new SaleCheck();
            var order = OpenOrders().FirstOrDefault(o => o.Order.Id == input.Order_Id);
            CommonChecks(r, Profile, order?.Order.Customer_Id ?? 0);
            if (order == null) { r.Problems.Add("الطلبية غير متاحة للتسليم."); return r; }
            var lines = order.Lines.ToDictionary(l => l.Order_Line_Id);
            bool remaining = false;
            foreach (var ol in order.Lines)
            {
                var given = input.Lines.FirstOrDefault(l => l.Order_Line_Id == ol.Order_Line_Id);
                decimal f = ol.Factor <= 0 ? 1 : ol.Factor;
                decimal take = given == null ? 0 : given.Qty_Large * f + given.Qty_Small;
                if (given != null && (given.Qty_Large < 0 || given.Qty_Small < 0)) r.Problems.Add($"كمية «{ol.Item_Name}» غير صحيحة.");
                if (take > ol.Remaining_Qty_Small) r.Problems.Add($"المسلَّم من «{ol.Item_Name}» أكبر من المتبقى.");
                if (take > 0 && take > AvailableSmall(ol.Unit_Item_Id)) r.Problems.Add($"المتاح من «{ol.Item_Name}» بالسيارة لا يكفى.");
                if (take < ol.Remaining_Qty_Small) remaining = true;
                r.Net += take * ol.Net_Unit_Price;
            }
            if (input.Lines.Any(l => !lines.ContainsKey(l.Order_Line_Id))) r.Problems.Add("سطر ليس من الطلبية.");
            if (remaining)
            {
                if (string.IsNullOrWhiteSpace(input.Reason)) r.Problems.Add("سبب عدم تسليم المتبقى مطلوب.");
                if (input.Remaining_Action is not (0 or 1)) r.Problems.Add("اختر مصير المتبقى: إعادة المحاولة أو إغلاقه.");
            }
            r.Net = Math.Round(r.Net, 2);
            decimal toCollect = Math.Max(0, r.Net - order.Order.Deposit_Remaining);
            if (input.Collect_Cash && toCollect > 0) CashLimit(r, Profile, input.Payment_Method_Id, toCollect);
            return r;
        }

        public Guid SaveDelivery(DeliveryInput input)
        {
            var c = CheckDelivery(input);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            var order = OpenOrders().First(o => o.Order.Id == input.Order_Id);
            decimal collect = input.Collect_Cash ? Math.Max(0, c.Net - order.Order.Deposit_Remaining) : 0;
            return Enqueue(DocType.Delivery, Json.Serialize(input), order.Order.Customer_Id, collect);
        }

        // ============================== الشيك ==============================

        public SaleCheck CheckCheque(ChequeInput input)
        {
            var r = new SaleCheck();
            var profile = Profile;
            CommonChecks(r, profile, input.Customer_Id);
            if (profile is { Allow_Cheque_Collection: false }) r.Problems.Add("تحصيل الشيكات غير مفعّل لك.");
            if (string.IsNullOrWhiteSpace(input.Check_No)) r.Problems.Add("رقم الشيك مطلوب.");
            else if (input.Check_No.Trim().Length > 30 || !input.Check_No.Trim().All(char.IsAsciiDigit)) r.Problems.Add("رقم الشيك أرقام فقط (حتى 30 رقماً).");
            if (input.Bank_Id <= 0 || Banks.All(b => b.Id != input.Bank_Id)) r.Problems.Add("اختر البنك المسحوب عليه.");
            if (input.Amount <= 0) r.Problems.Add("المبلغ يجب أن يكون أكبر من صفر.");
            if (input.Due_Date == default) r.Problems.Add("تاريخ الاستحقاق مطلوب.");
            r.Net = Math.Round(input.Amount, 2);
            return r;
        }

        public Guid SaveCheque(ChequeInput input)
        {
            var c = CheckCheque(input);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            return Enqueue(DocType.Cheque, Json.Serialize(input), input.Customer_Id, c.Net);
        }

        // ============================== عدم البيع ==============================

        public SaleCheck CheckNoSale(NoSaleInput input)
        {
            var r = new SaleCheck();
            CommonChecks(r, Profile, input.Customer_Id);
            var reason = NoSaleReasons.FirstOrDefault(x => x.Id == input.Reason_Id);
            if (reason == null) r.Problems.Add("اختر السبب.");
            else if (reason.Requires_Photo && (input.Photo == null || input.Photo.Length == 0)) r.Problems.Add($"سبب «{reason.Name}» يتطلب صورة.");
            if (input.Photo is { Length: > 2_000_000 }) r.Problems.Add("الصورة أكبر من 2 ميجا.");
            // نفس قاعدة السيرفر: زيارة بها مستند اليوم لا تُسجَّل «لا بيع».
            if (_store.Outbox().Any(o => o.Customer_Id == input.Customer_Id && ResultOf(o.Doc_Type) is not (null or 6) && o.Status != OutboxStatus.Rejected
                                         && o.Created_At.Date == _now().Date))
                r.Problems.Add("للعميل مستند اليوم؛ لا تُسجَّل «لا بيع» لنفس الزيارة.");
            return r;
        }

        public Guid SaveNoSale(NoSaleInput input)
        {
            var c = CheckNoSale(input);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            return Enqueue(DocType.NoSale, Json.Serialize(input), input.Customer_Id, 0);
        }
    }
}
