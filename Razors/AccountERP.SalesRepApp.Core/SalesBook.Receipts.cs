using System.Globalization;

namespace AccountERP.SalesRepApp.Core
{
    /// <summary>
    /// المستندات المطبوعة على الجهاز: لقطة تُحفظ مع المستند لحظة الحفظ، وعدّاد الطباعة (الطبعة الثانية فما بعدها «نسخة»).
    /// الحساب نفس حساب الحفظ (Calc / سعر الوحدة الصافى)، فالمطبوع = ما رُفع للسيرفر.
    /// </summary>
    public sealed partial class SalesBook
    {
        private static string ReceiptKey(Guid g) => "receipt." + g.ToString("N");
        private static string PrintedKey(Guid g) => "printed." + g.ToString("N");
        private static string N2(decimal v) => v.ToString("N2", CultureInfo.InvariantCulture);

        /// <summary>المستند المطبوع: اللقطة المحفوظة، وإلا يُبنى الآن من بيانات الجهاز. null لنوع بلا طباعة.</summary>
        public Receipt? Receipt(Guid guid)
        {
            Receipt? r = null;
            string? json = _store.Get(ReceiptKey(guid));
            if (json != null) r = Json.Deserialize<Receipt>(json);
            if (r == null && Doc(guid) is { } doc) r = BuildReceipt(doc);
            if (r != null) r.Copy = PrintCount(guid) > 0;
            return r;
        }

        public int PrintCount(Guid guid) => int.TryParse(_store.Get(PrintedKey(guid)), out int n) ? n : 0;

        public void MarkPrinted(Guid guid) => _store.Set(PrintedKey(guid), (PrintCount(guid) + 1).ToString(CultureInfo.InvariantCulture));

        /// <summary>يحفظ لقطة المستند بعد إضافته للصادر؛ فشل اللقطة لا يُفشل الحفظ (تُبنى عند الطلب).</summary>
        private void SnapshotReceipt(Guid guid)
        {
            try
            {
                if (Doc(guid) is { } doc && BuildReceipt(doc) is { } r) _store.Set(ReceiptKey(guid), Json.Serialize(r));
            }
            catch (Exception) { /* تُبنى من بيانات الجهاز عند أول طباعة */ }
        }

        public Receipt? BuildReceipt(OutboxItem doc)
        {
            string? title = doc.Doc_Type switch
            {
                DocType.Sale => Json.Deserialize<SaleInput>(doc.Payload)?.Is_Credit == true ? "فاتورة بيع آجل" : "فاتورة بيع نقدى",
                DocType.Return => "مرتجع مبيعات",
                DocType.Collection => "إيصال تحصيل",
                DocType.Cheque => "إيصال استلام شيك",
                DocType.Delivery => "تسليم طلبية",
                DocType.TakeOrder => "طلبية",
                _ => null,
            };
            if (title == null) return null;

            var p = Profile;
            var r = new Receipt { Doc_Guid = doc.Doc_Guid, Title = title, Doc_No = doc.Doc_No };
            r.Line(p?.Company_Name ?? "", bold: true, large: true, center: true)
             .Line(p?.Company_Address ?? "", center: true)
             .Line(string.IsNullOrWhiteSpace(p?.Company_Phone) ? "" : "هاتف " + p!.Company_Phone, center: true)
             .Line(string.IsNullOrWhiteSpace(p?.Tax_Number) ? "" : "الرقم الضريبى " + p!.Tax_Number, center: true)
             .Rule()
             .Line(title, bold: true, large: true, center: true)
             .Pair("رقم المستند", doc.Doc_No)
             .Pair("التاريخ", doc.Created_At.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))
             .Pair("العميل", Customer(doc.Customer_Id)?.Name)
             .Pair("المندوب", p?.Rep_Name)
             .Rule();

            switch (doc.Doc_Type)
            {
                case DocType.Sale: SaleBody(r, Json.Deserialize<SaleInput>(doc.Payload)!); break;
                case DocType.Return: ReturnBody(r, Json.Deserialize<ReturnInput>(doc.Payload)!); break;
                case DocType.Collection:
                {
                    var c = Json.Deserialize<CollectionInput>(doc.Payload)!;
                    r.Total = Math.Round(c.Amount, 2);
                    r.Pair("المبلغ", N2(r.Total), bold: true, large: true).Pair("طريقة الدفع", MethodName(c.Payment_Method_Id)).Line(c.Notes ?? "");
                    break;
                }
                case DocType.Cheque:
                {
                    var c = Json.Deserialize<ChequeInput>(doc.Payload)!;
                    r.Total = Math.Round(c.Amount, 2);
                    r.Pair("رقم الشيك", c.Check_No).Pair("البنك", Banks.FirstOrDefault(b => b.Id == c.Bank_Id)?.Name)
                     .Pair("الاستحقاق", c.Due_Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Pair("الساحب", c.Drawer_Name)
                     .Pair("المبلغ", N2(r.Total), bold: true, large: true).Line(c.Notes ?? "");
                    break;
                }
                case DocType.Delivery: DeliveryBody(r, Json.Deserialize<DeliveryInput>(doc.Payload)!); break;
                case DocType.TakeOrder: OrderBody(r, Json.Deserialize<TakeOrderInput>(doc.Payload)!); break;
            }
            r.Rule().Line("شكراً لتعاملكم", center: true);
            return r;
        }

        private string MethodName(long id) => Methods.FirstOrDefault(m => m.Id == id)?.Name ?? Methods.FirstOrDefault(m => m.Is_Cash)?.Name ?? "نقدى";

        private void SaleBody(Receipt r, SaleInput s)
        {
            decimal value = 0, disc = 0, tax = 0, net = 0;
            foreach (var l in s.Lines)
            {
                var item = Item(l.Unit_Item_Id) ?? new ItemRow { Item_Name = "صنف", Factor = 1 };
                var c = Calc(l, item.Factor);
                value += c.Value; disc += c.Discount; tax += c.Tax; net += c.Net;
                r.Line(item.Item_Name, bold: true).Pair($"{Format(c.QtySmall, item)} × {N2(l.Unit_Price)}", N2(c.Value));
                if (c.Discount > 0) r.Pair("خصم", "-" + N2(c.Discount));
            }
            r.Total = Math.Round(net, 2);
            r.Rule().Pair("الإجمالى", N2(value));
            if (disc > 0) r.Pair("الخصم", "-" + N2(disc));
            if (tax > 0) r.Pair("الضريبة", N2(tax));
            r.Pair("الصافى", N2(r.Total), bold: true, large: true)
             .Pair("الدفع", s.Is_Credit ? "على الحساب" : MethodName(s.Payment_Method_Id))
             .Line(s.Notes ?? "");
        }

        private void ReturnBody(Receipt r, ReturnInput s)
        {
            var invoice = s.Original_Invoice_Id is long id ? _store.Rows<InvoiceView>("invoices").FirstOrDefault(i => i.Id == id) : null;
            if (invoice != null) r.Pair("الفاتورة الأصلية", invoice.Invoice_No.ToString(CultureInfo.InvariantCulture)).Rule();
            decimal total = 0;
            foreach (var l in s.Lines)
            {
                var src = invoice?.Lines.FirstOrDefault(x => x.Original_Detail_Id == l.Original_Detail_Id);
                var item = src != null ? new ItemRow { Item_Name = src.Item_Name, B_Unit = src.B_Unit, S_Unit = src.S_Unit, Factor = src.Factor }
                         : Item(l.Unit_Item_Id ?? 0) ?? new ItemRow { Item_Name = "صنف", Factor = 1 };
                decimal f = item.Factor <= 0 ? 1 : item.Factor, small = l.Qty_Large * f + l.Qty_Small;
                decimal v = src == null ? 0 : Math.Round(small * src.Net_Unit_Price, 2);
                total += v;
                r.Line(item.Item_Name + (l.Condition == 1 ? " (تالف)" : ""), bold: true).Pair(Format(small, item), src == null ? "—" : N2(v));
            }
            r.Total = Math.Round(total, 2);
            r.Rule().Pair("قيمة المرتجع", N2(r.Total), bold: true, large: true)
             .Pair("التسوية", s.Refund_Cash ? "رُدت نقداً" : "تُخصم من الحساب")
             .Line(invoice == null ? "مرتجع بلا فاتورة: تُحدد قيمته بالمكتب." : "")
             .Line(s.Notes ?? "");
        }

        private void DeliveryBody(Receipt r, DeliveryInput s)
        {
            var order = _store.Rows<OrderSync>("orders").FirstOrDefault(o => o.Order.Id == s.Order_Id);
            if (order == null) { r.Line("طلبية غير موجودة على الجهاز."); return; }
            r.Pair("رقم الطلبية", order.Order.Order_No.ToString(CultureInfo.InvariantCulture)).Rule();
            decimal total = 0;
            foreach (var l in s.Lines)
            {
                var ol = order.Lines.FirstOrDefault(x => x.Order_Line_Id == l.Order_Line_Id);
                if (ol == null) continue;
                var item = new ItemRow { Item_Name = ol.Item_Name, B_Unit = ol.B_Unit, S_Unit = ol.S_Unit, Factor = ol.Factor };
                decimal f = ol.Factor <= 0 ? 1 : ol.Factor, small = l.Qty_Large * f + l.Qty_Small;
                if (small <= 0) continue;
                decimal v = Math.Round(small * ol.Net_Unit_Price, 2);
                total += v;
                r.Line(ol.Item_Name, bold: true).Pair(Format(small, item), N2(v));
            }
            total = Math.Round(total, 2);
            decimal deposit = Math.Min(order.Order.Deposit_Remaining, total), due = total - deposit;
            r.Total = total;
            r.Rule().Pair("قيمة المسلَّم", N2(total), bold: true, large: true);
            if (deposit > 0) r.Pair("من العربون", "-" + N2(deposit));
            r.Pair(s.Collect_Cash ? "المحصَّل" : "على الحساب", N2(due), bold: true);
            if (s.Collect_Cash && due > 0) r.Pair("طريقة الدفع", MethodName(s.Payment_Method_Id));
            r.Line(s.Notes ?? "");
        }

        private void OrderBody(Receipt r, TakeOrderInput s)
        {
            var items = OrderItems.ToDictionary(i => i.Id);
            decimal gross = 0, disc = 0, tax = 0, net = 0;
            foreach (var l in s.Lines)
            {
                items.TryGetValue(l.Unit_Item_Id, out var it);
                var item = new ItemRow { Item_Name = it?.Item_Name ?? "صنف", B_Unit = it?.B_Unit ?? "", S_Unit = it?.S_Unit ?? "", Factor = it?.Factor ?? 1 };
                decimal f = item.Factor <= 0 ? 1 : item.Factor;
                gross += l.Gross; disc += l.Discount_Value; tax += l.Tax_Value; net += l.Line_Total;
                r.Line(item.Item_Name, bold: true).Pair($"{Format(l.Qty_Large * f + l.Qty_Small, item)} × {N2(l.Unit_Price)}", N2(l.Gross));
                if (l.Discount_Value > 0) r.Pair("خصم", "-" + N2(l.Discount_Value));
            }
            r.Total = Math.Round(net, 2);
            r.Rule().Pair("الإجمالى", N2(gross));
            if (disc > 0) r.Pair("الخصم", "-" + N2(disc));
            if (tax > 0) r.Pair("الضريبة", N2(tax));
            r.Pair("صافى الطلبية", N2(r.Total), bold: true, large: true)
             .Pair("التسليم المتوقع", s.Expected_Delivery_Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            if (s.Deposit > 0) r.Pair("عربون مدفوع", N2(s.Deposit)).Pair("طريقة الدفع", MethodName(s.Deposit_Payment_Method_Id));
            r.Line(s.Notes ?? "");
        }
    }
}
