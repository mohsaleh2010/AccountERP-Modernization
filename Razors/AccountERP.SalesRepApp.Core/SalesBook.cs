namespace AccountERP.SalesRepApp.Core
{
    /// <summary>حساب سطر البيع كما على السيرفر (InvoiceLineCalculator، السعر للوحدة الكبرى).</summary>
    public readonly record struct LineCalc(decimal Value, decimal Discount, decimal WithoutTax, decimal Tax, decimal Net, decimal QtySmall, decimal DiscountRate);

    public class SaleCheck
    {
        /// <summary>تمنع الحفظ (لا وردية، مخزون السيارة لا يكفى، بيانات ناقصة، تجاوز 72 ساعة بلا مزامنة).</summary>
        public List<string> Problems { get; } = new();
        /// <summary>لا تمنع: السيرفر يجعل المستند «بانتظار المراجعة» ويطلب موافقة المشرف (خصم/آجل)، أو تنبيه حد النقدية.</summary>
        public List<string> Warnings { get; } = new();
        public decimal Net { get; set; }
        public bool Ok => Problems.Count == 0;
    }

    /// <summary>
    /// البيع والتحصيل على الجهاز من البيانات المنزَّلة: السعر الافتراضى، المتاح بالسيارة (رصيد السيرفر − مبيعات الجهاز غير المحفوظة بعد)،
    /// الفحص قبل الحفظ، ثم إضافة المستند للصادر بمعرّف جديد. السيرفر يعيد كل الفحوص عند الرفع.
    /// </summary>
    public sealed class SalesBook
    {
        private readonly LocalStore _store;
        private readonly RepSession _session;
        private readonly Func<DateTime> _now;

        public SalesBook(LocalStore store, RepSession session, Func<DateTime>? now = null)
        {
            _store = store; _session = session; _now = now ?? (() => DateTime.Now);
        }

        public RepProfile? Profile => _store.Rows<RepProfile>("profile").FirstOrDefault();
        public List<PaymentMethod> Methods => _store.Rows<PaymentMethod>("payment_methods").OrderBy(m => m.Is_Cash ? 0 : 1).ThenBy(m => m.Id).ToList();
        public List<CustomerRow> Customers => _store.Rows<CustomerRow>("customers").OrderBy(c => c.Name).ToList();
        public List<PlanRow> Plan => _store.Rows<PlanRow>("plan").OrderBy(p => p.Sequence).ToList();
        public List<ItemRow> Items => _store.Rows<ItemRow>("items").OrderBy(i => i.Item_Name).ToList();
        public ItemRow? Item(long unitItemId) => _store.Row<ItemRow>("items", unitItemId);
        public CustomerRow? Customer(long id) => _store.Row<CustomerRow>("customers", id);

        public static LineCalc Calc(SaleLine l, decimal factor)
        {
            decimal f = factor <= 0 ? 1 : factor;
            decimal value = (l.Qty_Large + l.Qty_Small / f) * l.Unit_Price;
            decimal disc = Math.Round(l.Discount_Value, 2);
            decimal without = Math.Max(0, value - disc);
            decimal tax = Math.Round(without * l.Tax_Rate / 100, 2);
            return new LineCalc(Math.Round(value, 2), disc, Math.Round(without, 2), tax, Math.Round(without + tax, 2), l.Qty_Large * f + l.Qty_Small,
                value > 0 ? Math.Round(disc / value * 100, 4) : 0);
        }

        /// <summary>سعر الوحدة الكبرى: نوع سعر العميل، ثم نوع المندوب، ثم سعر بيع الصنف.</summary>
        public decimal DefaultPrice(ItemRow item, CustomerRow? customer)
        {
            var prices = _store.Rows<PriceRow>("prices").Where(p => p.Item_Id == item.Item_Id).ToList();
            long? type = customer?.Price_Type_List_ID is > 0 ? customer.Price_Type_List_ID : Profile?.Price_Type_List_ID;
            var p = type == null ? null : prices.FirstOrDefault(x => x.Price_Type_List_ID == type);
            return p?.Price ?? item.Price_Large;
        }

        /// <summary>المتاح بالصغرى: رصيد السيرفر − كميات مبيعات الجهاز التى لم يحفظها السيرفر ولم تُرفض.</summary>
        public decimal AvailableSmall(long unitItemId)
        {
            var item = Item(unitItemId);
            var stock = _store.Row<StockRow>("stock", unitItemId);
            if (item == null || stock == null) return 0;
            decimal f = item.Factor <= 0 ? 1 : item.Factor;
            decimal server = stock.Full_Qua * f + stock.D_Qua;
            return server - PendingSmall(unitItemId, f);
        }

        private decimal PendingSmall(long unitItemId, decimal factor)
        {
            decimal sum = 0;
            foreach (var o in _store.Outbox(openOnly: true).Where(o => o.Doc_Type == DocType.Sale))
            {
                var s = Json.Deserialize<SaleInput>(o.Payload);
                if (s == null) continue;
                sum += s.Lines.Where(l => l.Unit_Item_Id == unitItemId).Sum(l => l.Qty_Large * factor + l.Qty_Small);
            }
            return sum;
        }

        /// <summary>نقدية العهدة: رصيد السيرفر عند آخر مزامنة + المقبوض نقداً على الجهاز ولم يُحفظ بعد.</summary>
        public decimal CashOnHand()
        {
            decimal server = Profile?.Cash_On_Hand ?? 0;
            var cash = Methods.Where(m => m.Is_Cash).Select(m => m.Id).ToHashSet();
            bool IsCash(long m) => m <= 0 || cash.Contains(m);
            decimal pending = 0;
            foreach (var o in _store.Outbox(openOnly: true))
            {
                if (o.Doc_Type == DocType.Sale && Json.Deserialize<SaleInput>(o.Payload) is { Is_Credit: false } s && IsCash(s.Payment_Method_Id)) pending += o.Amount;
                if (o.Doc_Type == DocType.Collection && Json.Deserialize<CollectionInput>(o.Payload) is { } c && IsCash(c.Payment_Method_Id)) pending += o.Amount;
            }
            return server + pending;
        }

        public SaleCheck Check(SaleInput input)
        {
            var r = new SaleCheck();
            var profile = Profile;
            CommonChecks(r, profile, input.Customer_Id);
            if (input.Lines.Count == 0) r.Problems.Add("أضف صنفاً واحداً على الأقل.");
            foreach (var g in input.Lines.GroupBy(l => l.Unit_Item_Id))
            {
                var item = Item(g.Key);
                if (item == null) { r.Problems.Add("صنف غير موجود على السيارة."); continue; }
                foreach (var l in g)
                {
                    if (l.Qty_Large < 0 || l.Qty_Small < 0 || l.Qty_Large + l.Qty_Small <= 0) r.Problems.Add($"كمية «{item.Item_Name}» غير صحيحة.");
                    if (l.Unit_Price < 0 || l.Discount_Value < 0) r.Problems.Add($"سعر أو خصم «{item.Item_Name}» غير صحيح.");
                    var c = Calc(l, item.Factor);
                    r.Net += c.Net;
                    if (profile?.Max_Discount_Rate is decimal max && c.DiscountRate > max)
                        r.Warnings.Add($"خصم «{item.Item_Name}» {c.DiscountRate:0.##}% فوق حدك {max:0.##}%؛ سيحتاج موافقة المشرف.");
                }
                decimal want = g.Sum(l => l.Qty_Large * (item.Factor <= 0 ? 1 : item.Factor) + l.Qty_Small);
                decimal have = AvailableSmall(g.Key);
                if (want > have) r.Problems.Add($"المتاح من «{item.Item_Name}» بالسيارة {Format(have, item)} أقل من المطلوب {Format(want, item)}.");
            }
            if (input.Is_Credit && profile is { Allow_Credit_Sales: false }) r.Warnings.Add("البيع الآجل غير مسموح لك؛ سيحتاج موافقة المشرف.");
            if (!input.Is_Credit) CashLimit(r, profile, input.Payment_Method_Id, r.Net);
            r.Net = Math.Round(r.Net, 2);
            return r;
        }

        public Guid SaveSale(SaleInput input)
        {
            var c = Check(input);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            return Enqueue(DocType.Sale, Json.Serialize(input), input.Customer_Id, c.Net);
        }

        public SaleCheck CheckCollection(CollectionInput input)
        {
            var r = new SaleCheck();
            CommonChecks(r, Profile, input.Customer_Id);
            if (input.Amount <= 0) r.Problems.Add("المبلغ يجب أن يكون أكبر من صفر.");
            CashLimit(r, Profile, input.Payment_Method_Id, input.Amount);
            r.Net = Math.Round(input.Amount, 2);
            return r;
        }

        public Guid SaveCollection(CollectionInput input)
        {
            var c = CheckCollection(input);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            return Enqueue(DocType.Collection, Json.Serialize(input), input.Customer_Id, c.Net);
        }

        private void CommonChecks(SaleCheck r, RepProfile? profile, long customerId)
        {
            if (profile == null) { r.Problems.Add("لم تكتمل أول مزامنة بعد."); return; }
            if (!profile.ShiftOpen) r.Problems.Add("لا وردية مفتوحة؛ يفتحها المكتب بالتحميل ثم زامِن.");
            var (ok, reason) = _session.CanSellOffline();
            if (!ok) r.Problems.Add(reason!);
            if (customerId <= 0 || Customer(customerId) == null) r.Problems.Add("اختر العميل.");
        }

        private void CashLimit(SaleCheck r, RepProfile? profile, long methodId, decimal amount)
        {
            if (profile?.Max_Cash_On_Hand is not decimal max) return;
            bool cash = methodId <= 0 || Methods.Any(m => m.Id == methodId && m.Is_Cash);
            if (!cash) return;
            decimal after = CashOnHand() + amount;
            if (after > max) r.Warnings.Add($"النقدية بالعهدة بعد القبض {after:N2} تتجاوز الحد {max:N2}؛ ورِّد جزءاً منها.");
        }

        private Guid Enqueue(DocType type, string payload, long customerId, decimal amount)
        {
            var guid = Guid.NewGuid();
            _store.Enqueue(new OutboxItem
            {
                Doc_Guid = guid, Doc_Type = type, Payload = payload, Created_At = _now(), Status = OutboxStatus.Queued, Customer_Id = customerId, Amount = amount,
            });
            return guid;
        }

        public static string Format(decimal small, ItemRow item)
        {
            decimal f = item.Factor <= 0 ? 1 : item.Factor;
            if (f == 1) return $"{small:0.##} {item.B_Unit}".Trim();
            decimal large = Math.Floor(small / f), rest = small - large * f;
            return rest == 0 ? $"{large:0.##} {item.B_Unit}".Trim() : $"{large:0.##} {item.B_Unit} + {rest:0.##} {item.S_Unit}".Trim();
        }
    }
}
