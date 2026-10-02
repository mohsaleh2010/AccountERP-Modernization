using AccountModelsLib;
using AccountModelsLib.Models;

namespace AccountERP.SharedUI.Services
{
    // 🟢 نقل حرفى لمنطق Accounts.ItemData.ComputePrice (حساب سطر صنف واحد فى الفاتورة) إلى خدمة C# خالصة
    // بلا أى اعتماد على عناصر تحكم WinForms، لتُستخدم من مكوّن Razor لإضافة/تعديل صنف الفاتورة.
    // نفس ترتيب الحساب بالضبط: قيمة الكمية حسب وحدة السعر المختارة -> العمولة -> الضريبة -> الصافى.
    public static class InvoiceLineCalculator
    {
        // 🟢 الأصل (WinForms) يبنى سطر الفاتورة من DataSet أعمدته الرقمية افتراضياً 0 (InvoiceDataSetCLS.MakeInvoiceDataSet)،
        // بينما IInvoices_Details خصائصه decimal? تبدأ null. محرك الحفظ القديم يدمج قيم هذه الحقول نصاً داخل استعلامات
        // SQL (مثل "Item_No - " + Qnty_Another) فيصير الاستعلام "Item_No -  WHERE" عند null -> خطأ صياغة SQL (اكتُشف حياً
        // عند اختبار الحفظ). لذا نُصفّر كل decimal? فارغ هنا، دون تعديل محرك الحفظ.
        private static readonly System.Reflection.PropertyInfo[] NullableDecimalProps = typeof(IInvoices_Details)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(decimal?) && p.CanWrite)
            .ToArray();

        public static void ZeroNullNumerics(IInvoices_Details line)
        {
            foreach (var p in NullableDecimalProps)
            {
                if (p.GetValue(line) is null) p.SetValue(line, 0m);
            }
        }

        private static readonly System.Reflection.PropertyInfo[] HeaderNullableDecimalProps = typeof(IInvoices_List)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(decimal?) && p.CanWrite)
            .ToArray();

        // نفس الفكرة لرأس الفاتورة (يُستدعى قبل الحفظ مباشرة).
        public static void ZeroNullNumerics(IInvoices_List header)
        {
            foreach (var p in HeaderNullableDecimalProps)
            {
                if (p.GetValue(header) is null) p.SetValue(header, 0m);
            }
        }

        public static void ComputeLine(IInvoices_Details line, decimal unitSmallValue)
        {
            ZeroNullNumerics(line);
            var largQnty = line.Larg_Qnty ?? 0;
            var smallQnty = line.Small_Qnty ?? 0;
            var qntyAnother = line.Qnty_Another ?? 0;
            var priceUnit = line.PriceUnit ?? 0;

            line.Qnty_value = line.Unit_Used_for_Price switch
            {
                1 => largQnty * priceUnit + (unitSmallValue == 0 ? 0 : smallQnty / unitSmallValue) * priceUnit,
                2 => largQnty * unitSmallValue * priceUnit + smallQnty * priceUnit,
                3 => qntyAnother * priceUnit,
                _ => 0,
            };

            // 🟢 يقابل ItemData.Compute_3Mola_Value: عمولة العدد = عدد القطع × قيمة _3Mola_Value، وتُحسب فقط لو IsCompute_3Mola_Value_OnInvoiceMonitor=1
            Compute3Mola(line);

            line.Total = line.Qnty_value;
            line.Item_Net_Total_Price = line.Qnty_value;

            // 🟢 يقابل ComputeCommission الأصلية بالضبط (عمولة الصنف: نسبة على القيمة + قيمة عمولة الوحدة * الكمية)
            var commissionRate = line.Item_CommissionRate ?? 0;
            var commissionValue = line.Item_CommissionValue ?? 0;
            var unitCommissionValue = line.Unit_CommissionValue ?? 0;
            var qnty = largQnty + smallQnty / (unitSmallValue == 0 ? 1 : unitSmallValue);
            var itemCommissionTotal = (line.Total ?? 0) * commissionRate / 100 + commissionValue + unitCommissionValue * qnty;
            line.ItemCommissionTotal = Common.RoundMePositive(itemCommissionTotal);
            line.Net_Total_Item_Commission = line.ItemCommissionTotal;

            line.Item_Total_WithoutTax = (line.Item_Net_Total_Price ?? 0) - (line.Net_Total_Item_Commission ?? 0);

            line.Item_TaxSales10_Value = (line.Item_Total_WithoutTax ?? 0) * (line.Item_TaxSales10 ?? 0) / 100;
            line.Item_ATS05_Value = (line.Item_Total_WithoutTax ?? 0) * (line.Item_ATS05 ?? 0) / 100;

            line.Item_Net_Total_Price = (line.Item_Total_WithoutTax ?? 0) - (line.Item_ATS05_Value ?? 0) + (line.Item_TaxSales10_Value ?? 0);

            // يقابل ItemData.ComputePrice سطر 1336-1338: عمولة العدد تُضاف إلى إجمالى السطر وصافيه بعد الضريبة.
            line.Total = (line.Total ?? 0) + (line.Total3Mola ?? 0);
            line.Item_Net_Total_Price = (line.Item_Net_Total_Price ?? 0) + (line.Total3Mola ?? 0);

            if ((line.Item_Net_Total_Price ?? 0) < 0) line.Item_Net_Total_Price = 0;
            if ((line.Item_Total_WithoutTax ?? 0) < 0) line.Item_Total_WithoutTax = 0;

            line.Item_TaxSales10_Value = Common.RoundMePositive(line.Item_TaxSales10_Value ?? 0);
            line.Item_ATS05_Value = Common.RoundMePositive(line.Item_ATS05_Value ?? 0);
            line.Qnty_value = Common.RoundMePositive(line.Qnty_value ?? 0);
            line.Item_Total_WithoutTax = Common.RoundMePositive(line.Item_Total_WithoutTax ?? 0);
            line.Item_Net_Total_Price = Common.RoundMePositive(line.Item_Net_Total_Price ?? 0);
        }

        // 🟢 قاعدة المستخدم (Is_Tax_Per_Item=1): الضريبة تُحسب على كل صنف على حدة من قيمته بعد العمولة، ولذلك يُجرَّد الرأس من
        // نسب الضريبة (لا تتكرر على الإجمالى). تُستدعى عند تبديل نوع الفاتورة/طريقة الضريبة لإعادة حساب أسطر موجودة دون
        // إعادة حساب الكميات (Item_Total_WithoutTax محسوبة أصلاً).
        public static void ApplyItemTax(IInvoices_Details line, decimal taxSalesRate, decimal ats05Rate)
        {
            line.Item_TaxSales10 = taxSalesRate;
            line.Item_ATS05 = ats05Rate;
            var without = line.Item_Total_WithoutTax ?? 0;
            line.Item_TaxSales10_Value = Common.RoundMePositive(without * taxSalesRate / 100);
            line.Item_ATS05_Value = Common.RoundMePositive(without * ats05Rate / 100);
            var net = without - (line.Item_ATS05_Value ?? 0) + (line.Item_TaxSales10_Value ?? 0) + (line.Total3Mola ?? 0);
            line.Item_Net_Total_Price = Common.RoundMePositive(net < 0 ? 0 : net);
        }

        public static void Compute3Mola(IInvoices_Details line)
        {
            line.Qnty_Another ??= 0;
            if (Common.IsCompute_3Mola_Value_OnInvoiceMonitor == 1)
            {
                line.Unit3Mola = Common._3Mola_Value;
                line.Total3Mola = line.Qnty_Another * line.Unit3Mola;
            }
            else
            {
                line.Unit3Mola = 0;
                line.Total3Mola = 0;
            }
        }

        // 🟢 يقابل NewFrmInvoive.GetTotalItemPrices + ComputeTotals (بند الضريبة على مستوى الفاتورة فقط،
        // Is_Tax_Per_Item=0 - حالة الضريبة لكل صنف على حدة تُجمَع من القيم المحسوبة أعلاه لكل سطر بالفعل).
        public static void ComputeHeaderTotals(IInvoices_List header, List<IInvoices_Details> lines, bool isTaxPerItem)
        {
            decimal total = 0, netTotal = 0, totalWithoutTax = 0, commissionValue = 0;
            decimal taxSalesValue = 0, ats05Value = 0, total3Mola = 0;

            foreach (var line in lines.Where(l => (l.Item_DeletedFlage ?? 0) != 1))
            {
                total += line.Qnty_value ?? 0;
                netTotal += line.Item_Net_Total_Price ?? 0;
                totalWithoutTax += line.Item_Total_WithoutTax ?? 0;
                commissionValue += line.Net_Total_Item_Commission ?? 0;
                total3Mola += line.Total3Mola ?? 0;
                if (isTaxPerItem)
                {
                    taxSalesValue += line.Item_TaxSales10_Value ?? 0;
                    ats05Value += line.Item_ATS05_Value ?? 0;
                }
            }

            header.Total = total;
            header.Qnty_Value = total;
            header.Total_Item_CommissionValue = commissionValue;
            header.Total_PriceWithoutTax = total - commissionValue;

            var listCommission = (header.List_CommissionRate ?? 0) * total / 100 + (header.List_CommissionValue ?? 0);
            header.List_Final_CommissionValue = listCommission;
            header.Net_Total_CommissionValue = listCommission + commissionValue;
            header.Total_PriceWithoutTax = total - header.Net_Total_CommissionValue;

            if (isTaxPerItem)
            {
                header.TaxSales10_Value = taxSalesValue;
                header.ATS05_Value = ats05Value;
            }
            else
            {
                header.TaxSales10_Value = Common.RoundMePositive((header.Total_PriceWithoutTax ?? 0) * (header.TaxSales10 ?? 0) / 100);
                header.ATS05_Value = Common.RoundMePositive((header.Total_PriceWithoutTax ?? 0) * (header.ATS05 ?? 0) / 100);
            }

            header.Net_Total = (header.Total_PriceWithoutTax ?? 0) + (header.TaxSales10_Value ?? 0) - (header.ATS05_Value ?? 0)
                                + (header.AnotherPayment ?? 0) + (header.Shipment_Value ?? 0)
                                + total3Mola; // يقابل NewFrmInvoive سطر 3885: Net_Total += Total_List3Mola
            header.Total_List3Mola = total3Mola;
            if ((header.Net_Total ?? 0) < 0) header.Net_Total = 0;

            header.Net_Total = Common.RoundMePositive(header.Net_Total ?? 0);
            header.NetInvoiceTotal = header.Net_Total;
            // يقابل NewFrmInvoive.ComputeInvoiceTotals: المكافئ بعملة النظام = الصافى (بدونهما يبقى Equivalent_Net_Total_Price = 0 فى الفاتورة المحفوظة)
            header.Equivalent_Net_Total_Price = header.Net_Total;
            header.Equivalent_Net_Total_CommissionValue = header.Net_Total_CommissionValue;
            header.Total_PriceWithoutTax = Common.RoundMePositive(header.Total_PriceWithoutTax ?? 0);
        }
    }
}
