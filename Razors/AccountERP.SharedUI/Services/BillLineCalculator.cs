using AccountModelsLib;
using AccountModelsLib.Models;

namespace AccountERP.SharedUI.Services
{
    // 🟢 حسابات سطر وإجماليات فاتورة المشتريات/مرتجعها (IBills_Details / IBills_List). تقابل
    // Accounts.ItemDataBill.ComputePrice (سطر) و Make_Bill_Templat.GetTotalItemPrices/ComputeTotals (الرأس)، وهى نسخة مشتريات
    // مستقلة من InvoiceLineCalculator (المبيعات) كى لا يتأثر أيّ منهما بالآخر (قرار المستخدم: صفحة مشتريات مستقلة بمكوّنات جديدة).
    // الضريبة فى v1 تُحسب لكل صنف فقط (Is_Tax_Per_Item=1 عند الفاتورة الضريبية)؛ نسب الرأس صفر فلا تتكرر على الإجمالى.
    public static class BillLineCalculator
    {
        // محرك الحفظ القديم يدمج قيم الحقول نصاً داخل SQL (مثل "Item_No - " + Qnty_Another) فتصير الجملة غير صالحة عند null؛
        // الأصل يبنى الأسطر من DataSet أعمدته الرقمية 0 افتراضياً، فنُصفّر هنا كل decimal? فارغ (بلا لمس المحرك).
        private static readonly System.Reflection.PropertyInfo[] LineNullableDecimals = typeof(IBills_Details)
            .GetProperties().Where(p => p.PropertyType == typeof(decimal?) && p.CanWrite).ToArray();

        private static readonly System.Reflection.PropertyInfo[] HeaderNullableDecimals = typeof(IBills_List)
            .GetProperties().Where(p => p.PropertyType == typeof(decimal?) && p.CanWrite).ToArray();

        public static void ZeroNullNumerics(IBills_Details line)
        {
            foreach (var p in LineNullableDecimals)
                if (p.GetValue(line) is null) p.SetValue(line, 0m);
        }

        public static void ZeroNullNumerics(IBills_List header)
        {
            foreach (var p in HeaderNullableDecimals)
                if (p.GetValue(header) is null) p.SetValue(header, 0m);
        }

        public static void ComputeLine(IBills_Details line, decimal unitSmallValue)
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
            line.Total = line.Qnty_value;
            line.Item_Net_Total_Price = line.Qnty_value;

            // عمولة الصنف: نسبة على القيمة + قيمة ثابتة + عمولة الوحدة × الكمية (كما فى ComputeCommission).
            var qnty = largQnty + smallQnty / (unitSmallValue == 0 ? 1 : unitSmallValue);
            var commission = (line.Total ?? 0) * (line.Item_CommissionRate ?? 0) / 100 + (line.Item_CommissionValue ?? 0) + (line.Unit_CommissionValue ?? 0) * qnty;
            line.ItemCommissionTotal = Common.RoundMePositive(commission);
            line.Net_Total_Item_Commission = line.ItemCommissionTotal;

            line.Item_Total_WithoutTax = (line.Item_Net_Total_Price ?? 0) - (line.Net_Total_Item_Commission ?? 0);
            line.Item_TaxSales10_Value = (line.Item_Total_WithoutTax ?? 0) * (line.Item_TaxSales10 ?? 0) / 100;
            line.Item_ATS05_Value = (line.Item_Total_WithoutTax ?? 0) * (line.Item_ATS05 ?? 0) / 100;
            line.TaxSales10_Value = Common.RoundMePositive(line.Item_TaxSales10_Value ?? 0);

            line.Item_Net_Total_Price = (line.Item_Total_WithoutTax ?? 0) - (line.Item_ATS05_Value ?? 0) + (line.Item_TaxSales10_Value ?? 0);
            if ((line.Item_Net_Total_Price ?? 0) < 0) line.Item_Net_Total_Price = 0;
            if ((line.Item_Total_WithoutTax ?? 0) < 0) line.Item_Total_WithoutTax = 0;

            line.Item_TaxSales10_Value = Common.RoundMePositive(line.Item_TaxSales10_Value ?? 0);
            line.Item_ATS05_Value = Common.RoundMePositive(line.Item_ATS05_Value ?? 0);
            line.Qnty_value = Common.RoundMePositive(line.Qnty_value ?? 0);
            line.Item_Total_WithoutTax = Common.RoundMePositive(line.Item_Total_WithoutTax ?? 0);
            line.Item_Net_Total_Price = Common.RoundMePositive(line.Item_Net_Total_Price ?? 0);
        }

        // إعادة حساب ضريبة سطر موجود عند تبديل نوع الفاتورة/طريقة الضريبة (الكميات لا تتغير).
        public static void ApplyItemTax(IBills_Details line, decimal taxSalesRate, decimal ats05Rate)
        {
            line.Item_TaxSales10 = taxSalesRate;
            line.Item_ATS05 = ats05Rate;
            var without = line.Item_Total_WithoutTax ?? 0;
            line.Item_TaxSales10_Value = Common.RoundMePositive(without * taxSalesRate / 100);
            line.Item_ATS05_Value = Common.RoundMePositive(without * ats05Rate / 100);
            line.TaxSales10_Value = line.Item_TaxSales10_Value ?? 0;
            var net = without - (line.Item_ATS05_Value ?? 0) + (line.Item_TaxSales10_Value ?? 0);
            line.Item_Net_Total_Price = Common.RoundMePositive(net < 0 ? 0 : net);
        }

        // يقابل GetTotalItemPrices + ComputeCommission + ComputeTotals فى Make_Bill_Templat (الأصل):
        //  - عمولة الفاتورة (List_Final) = النسبة × الإجمالى + القيمة، وتُعطَّل وتُصفَّر مع الضريبة لكل صنف (Is_Tax_Per_Item=1)؛
        //  - Net_Total_CommissionValue = List_Final + عمولات الأصناف (آخر إسناد فى الأصل)؛
        //  - صافى الفاتورة = مجموع صوافى الأصناف - عمولة الفاتورة (+ الحمولة + الدمغات)، مقرَّباً وغير سالب؛
        //  - Total_PriceWithoutTax_Load_Payment (لغير الضريبة لكل صنف) = الإجمالى بلا ضريبة + الحمولة.
        // ضريبة الرأس غير مدعومة هنا (النسب صفر)، فلا يوجد أثر للحمولة على الضريبة.
        public static void ComputeHeaderTotals(IBills_List header, IEnumerable<IBills_Details> lines)
        {
            ZeroNullNumerics(header);
            var perItemTax = (header.Is_Tax_Per_Item ?? 0) == 1;
            decimal total = 0, net = 0, withoutTax = 0, itemCommission = 0, tax = 0, ats05 = 0;
            foreach (var l in lines.Where(l => (l.Item_DeletedFlage ?? 0) != 1))
            {
                total += l.Qnty_value ?? 0;
                net += l.Item_Net_Total_Price ?? 0;
                withoutTax += l.Item_Total_WithoutTax ?? 0;
                itemCommission += l.Net_Total_Item_Commission ?? 0;
                tax += l.Item_TaxSales10_Value ?? 0;
                ats05 += l.Item_ATS05_Value ?? 0;
            }

            if (perItemTax)
            {
                header.List_CommissionRate = 0;
                header.List_CommissionValue = 0;
            }
            var listFinal = perItemTax ? 0 : (header.List_CommissionRate ?? 0) * total / 100 + (header.List_CommissionValue ?? 0);
            header.List_Final_CommissionValue = Common.RoundMePositive(listFinal);

            header.Total = total;
            header.Total_Item_CommissionValue = itemCommission;
            header.Net_Total_CommissionValue = Common.RoundMePositive(listFinal + itemCommission);
            header.Total_ItemPriceWithoutTax = withoutTax;
            header.Total_ListPriceWithoutTax = withoutTax;
            header.Total_PriceWithoutTax = withoutTax - listFinal;
            if (!perItemTax) header.Total_PriceWithoutTax_Load_Payment = (header.Total_PriceWithoutTax ?? 0) + (header.Load_Payment ?? 0);
            header.TaxSales10_Value = tax;
            header.ATS05_Value = ats05;

            var netTotal = net - listFinal + (header.Load_Payment ?? 0) + (header.Damaghat ?? 0);
            header.Net_Total = Common.RoundMePositive(netTotal < 0 ? 0 : netTotal);
            header.Equivalent_Net_Total_Price = header.Net_Total;
            header.Equivalent_Net_Total_CommissionValue = header.Net_Total_CommissionValue;
        }
    }
}
