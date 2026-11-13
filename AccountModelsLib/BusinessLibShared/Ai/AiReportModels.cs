using System;
using System.Collections.Generic;

namespace AccountModelsLib
{
    // ───────────── P3: التقارير وفتح الشاشات (ai-assistant-plan.md §5) ─────────────

    /// <summary>تنسيق عمود أو مؤشر فى التقرير المرسوم.</summary>
    public static class AiReportFormats
    {
        public const string Amount = "amount", Qty = "qty", Int = "int", Percent = "percent", Date = "date", Text = "text";
        public static readonly string[] All = { Amount, Qty, Int, Percent, Date, Text };
    }

    public static class AiChartKinds
    {
        public const string Bar = "bar", Line = "line", Pie = "pie", Stacked = "stacked";
        public static readonly string[] All = { Bar, Line, Pie, Stacked };
    }

    /// <summary>شاشة من قائمة المستخدم (المسار والعنوان) — لأداة find_screen وopen_screen.</summary>
    public class AiScreenRef
    {
        public string Path { get; set; } = "";
        public string Title { get; set; } = "";
    }

    /// <summary>بطاقة تقرير تحت رد المساعد (الملخص فقط؛ التقرير الكامل يُقرأ بـ GetReport).</summary>
    public class AiReportCard
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
    }

    /// <summary>بطاقة «افتح الشاشة» تحت رد المساعد.</summary>
    public class AiScreenLinkView
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        /// <summary>وصف الباراميترات بالعربى («من 2026-10-01 إلى 2026-10-31، فرع 1»).</summary>
        public string Detail { get; set; } = "";
    }

    /// <summary>P5: بطاقة «مسودة جاهزة» تحت رد المساعد: ملخص + تحذيرات + زر «فتح فى الشاشة». الحالة تُقرأ من AI_Drafts عند العرض.</summary>
    public class AiDraftCard
    {
        public long Id { get; set; }
        public string Kind { get; set; } = "";
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Url { get; set; } = "";
        public List<string> Warnings { get; set; } = new List<string>();
        public string Status { get; set; } = AiDraftStatus.Pending;
        public string DocNo { get; set; } = "";
    }

    public class AiReportKpi
    {
        public string Label { get; set; } = "";
        public decimal? Value { get; set; }
        public string Format { get; set; } = AiReportFormats.Amount;
    }

    public class AiReportColumn
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string Format { get; set; } = AiReportFormats.Text;
        public bool IsNumeric => Format is AiReportFormats.Amount or AiReportFormats.Qty or AiReportFormats.Int or AiReportFormats.Percent;
    }

    public class AiChartSeries
    {
        public string Name { get; set; } = "";
        public List<decimal> Values { get; set; } = new List<decimal>();
    }

    /// <summary>قسم من التقرير: table / chart / text.</summary>
    public class AiReportSection
    {
        public string Kind { get; set; } = "table";
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        // الجدول
        public List<AiReportColumn> Columns { get; set; } = new List<AiReportColumn>();
        /// <summary>القيم: decimal للأرقام، string للباقى (التاريخ yyyy-MM-dd)، null للفارغ.</summary>
        public List<object[]> Rows { get; set; } = new List<object[]>();
        /// <summary>مجموع كل عمود رقمى على كل صفوف القسم (null = لا معنى للجمع).</summary>
        public List<decimal?> Totals { get; set; } = new List<decimal?>();
        public bool ShowTotals { get; set; }
        /// <summary>عدد الصفوف قبل القص (top أو حد العرض).</summary>
        public int TotalRows { get; set; }
        // الرسم
        public string ChartKind { get; set; } = AiChartKinds.Bar;
        public string ValueFormat { get; set; } = AiReportFormats.Amount;
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<AiChartSeries> Series { get; set; } = new List<AiChartSeries>();
    }

    public class AiReportSourceView
    {
        public string Title { get; set; } = "";
        public string Sql { get; set; } = "";
        public int RowCount { get; set; }
        public bool Truncated { get; set; }
    }

    /// <summary>التقرير جاهزاً للعرض (يُبنى على الخادم من الـ spec واللقطة المحفوظة؛ لا HTML من النموذج).</summary>
    public class AiReportDoc
    {
        public long Id { get; set; }
        public long? ConversationId { get; set; }
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        /// <summary>يمكن إعادة تشغيله بفترة جديدة (مصدر واحد على الأقل يعتمد على تاريخى البداية والنهاية).</summary>
        public bool CanRerun { get; set; }
        public bool IsSaved { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public List<AiReportKpi> Kpis { get; set; } = new List<AiReportKpi>();
        public List<AiReportSection> Sections { get; set; } = new List<AiReportSection>();
        public List<AiReportSourceView> Sources { get; set; } = new List<AiReportSourceView>();
    }

    /// <summary>تقرير محفوظ فى قائمة «تقاريرى».</summary>
    public class AiSavedReportView
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastRunAt { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
    }
}
