using System;

namespace AccountModelsLib.Pricing
{
    /// <summary>مدخلات حساب سطر بيع واحد (أسماء الحقول كما فى IInvoices_Details).</summary>
    public struct LineInput
    {
        /// <summary>1 السعر للكبرى، 2 للصغرى، 3 بالعدد الآخر (Unit_Used_for_Price).</summary>
        public int Unit_Used_for_Price;
        public decimal Larg_Qnty, Small_Qnty, Qnty_Another, PriceUnit;
        /// <summary>عدد الصغرى فى الكبرى (Small_unit).</summary>
        public decimal Factor;
        public decimal Item_CommissionRate, Item_CommissionValue, Unit_CommissionValue;
        public decimal Item_TaxSales10, Item_ATS05;
        /// <summary>عمولة العدد المحسوبة (Qnty_Another × قيمة العمولة) أو صفر.</summary>
        public decimal Total3Mola;
    }

    public struct LineResult
    {
        public decimal Qnty_value, ItemCommissionTotal, Item_Total_WithoutTax, Item_TaxSales10_Value, Item_ATS05_Value, Total, Item_Net_Total_Price;
    }

    /// <summary>
    /// حساب سطر البيع الوحيد فى النظام (توحيد المحرك 2026-10-17): نقل حرفى لـ ItemData.ComputePrice كما كان فى InvoiceLineCalculator.ComputeLine.
    /// ملف بلا أى اعتماد، يُترجَم داخل AccountModelsLib (شاشات الويب وخدمات المندوب) وداخل تطبيق المندوب (رابط للملف نفسه)، فالجهاز
    /// والسيرفر يحسبان بالكود نفسه. التقريب يُمرَّر: السيرفر Common.RoundMePositive (منازل SysConfig)، والجهاز بالمنازل التى ينزّلها السيرفر.
    /// </summary>
    public static class LineMath
    {
        public static LineResult Compute(LineInput i, Func<decimal, decimal> roundPositive)
        {
            var r = new LineResult();
            decimal f = i.Factor;
            r.Qnty_value = i.Unit_Used_for_Price switch
            {
                1 => i.Larg_Qnty * i.PriceUnit + (f == 0 ? 0 : i.Small_Qnty / f) * i.PriceUnit,
                2 => i.Larg_Qnty * f * i.PriceUnit + i.Small_Qnty * i.PriceUnit,
                3 => i.Qnty_Another * i.PriceUnit,
                _ => 0,
            };
            r.Total = r.Qnty_value;
            r.Item_Net_Total_Price = r.Qnty_value;

            // عمولة الصنف (= الخصم): نسبة على القيمة + قيمة ثابتة + عمولة الوحدة × الكمية.
            decimal qnty = i.Larg_Qnty + i.Small_Qnty / (f == 0 ? 1 : f);
            r.ItemCommissionTotal = roundPositive(r.Total * i.Item_CommissionRate / 100 + i.Item_CommissionValue + i.Unit_CommissionValue * qnty);

            r.Item_Total_WithoutTax = r.Item_Net_Total_Price - r.ItemCommissionTotal;
            r.Item_TaxSales10_Value = r.Item_Total_WithoutTax * i.Item_TaxSales10 / 100;
            r.Item_ATS05_Value = r.Item_Total_WithoutTax * i.Item_ATS05 / 100;
            r.Item_Net_Total_Price = r.Item_Total_WithoutTax - r.Item_ATS05_Value + r.Item_TaxSales10_Value;

            // عمولة العدد تُضاف للإجمالى والصافى بعد الضريبة.
            r.Total += i.Total3Mola;
            r.Item_Net_Total_Price += i.Total3Mola;

            if (r.Item_Net_Total_Price < 0) r.Item_Net_Total_Price = 0;
            if (r.Item_Total_WithoutTax < 0) r.Item_Total_WithoutTax = 0;

            r.Item_TaxSales10_Value = roundPositive(r.Item_TaxSales10_Value);
            r.Item_ATS05_Value = roundPositive(r.Item_ATS05_Value);
            r.Qnty_value = roundPositive(r.Qnty_value);
            r.Item_Total_WithoutTax = roundPositive(r.Item_Total_WithoutTax);
            r.Item_Net_Total_Price = roundPositive(r.Item_Net_Total_Price);
            return r;
        }

        /// <summary>التقريب الموجب بعدد منازل (كما Common.RoundMePositive بإعداد DecimalPlaces): القيمة المطلقة مقرّبة.</summary>
        public static Func<decimal, decimal> RoundPositive(int decimals) => x => Math.Round(Math.Abs(x), Math.Max(0, Math.Min(6, decimals)));   // net48: لا Math.Clamp
    }
}
