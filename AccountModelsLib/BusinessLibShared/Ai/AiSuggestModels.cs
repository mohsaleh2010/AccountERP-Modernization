using System;
using System.Collections.Generic;
using System.Globalization;

namespace AccountModelsLib
{
    /// <summary>P8.8 (ai-assistant-p88-plan.md): أنواع الاقتراح.</summary>
    public static class AiSuggestKinds
    {
        public const string Expense = "expense";
        public const string Item = "item";
    }

    public static class AiSuggestSources
    {
        public const string Exact = "exact";
        public const string Neighbors = "neighbors";
        public const string Model = "model";
    }

    public static class AiSuggestOutcomes
    {
        public const string Shown = "shown";
        public const string Applied = "applied";
        public const string Dismissed = "dismissed";
        public const string Overridden = "overridden";

        public static bool IsValid(string o) => o == Shown || o == Applied || o == Dismissed || o == Overridden;
    }

    /// <summary>إعدادات الميزة مقروءة من AI_Settings (ص17).</summary>
    public class AiSuggestSettings
    {
        public bool Enabled { get; set; }
        public bool UseModel { get; set; } = true;
        public double MinConfidence { get; set; } = 0.35;
        public int HistoryMonths { get; set; } = 24;
        public int MaxRows { get; set; } = 50000;
        public bool ForAllUsers { get; set; }

        /// <summary>هامش الغموض (ص2): الفرق بين أعلى نتيجتين مطبّعتين أقل منه ⇒ النموذج.</summary>
        public double AmbiguityMargin { get; set; } = 0.15;

        public static AiSuggestSettings From(IDictionary<string, string> s)
        {
            var r = new AiSuggestSettings();
            if (s == null) return r;
            r.Enabled = Get(s, AiSettingKeys.SuggestEnabled, "0") == "1";
            r.UseModel = Get(s, AiSettingKeys.SuggestUseModel, "1") == "1";
            r.ForAllUsers = Get(s, AiSettingKeys.SuggestForAllUsers, "0") == "1";
            double d;
            if (double.TryParse(Get(s, AiSettingKeys.SuggestMinConfidence, "0.35"), NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                r.MinConfidence = Math.Max(0, Math.Min(1, d));
            int i;
            if (int.TryParse(Get(s, AiSettingKeys.SuggestHistoryMonths, "24"), out i)) r.HistoryMonths = Math.Max(1, Math.Min(120, i));
            if (int.TryParse(Get(s, AiSettingKeys.SuggestMaxRows, "50000"), out i)) r.MaxRows = Math.Max(100, Math.Min(500000, i));
            return r;
        }

        private static string Get(IDictionary<string, string> s, string k, string def)
        {
            string v;
            return s.TryGetValue(k, out v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : def;
        }
    }

    /// <summary>سابقة واحدة (سطر سند أو صنف موجود) فى الفهرس.</summary>
    public class AiSuggestPrecedent
    {
        public string Text { get; set; }
        public string TextNorm { get; set; }
        public string[] Tokens { get; set; } = new string[0];
        public long TargetId { get; set; }
        public string TargetCode { get; set; }
        public string TargetName { get; set; }
        /// <summary>expense: مركز التكلفة؛ item: المجموعة.</summary>
        public long SecondaryId { get; set; }
        public string SecondaryName { get; set; }
        public DateTime Date { get; set; }
        public long BranchId { get; set; }
        /// <summary>expense: نوع الحركة (صرف/قبض)؛ item: Item_Or_Service.</summary>
        public int SubType { get; set; }
    }

    /// <summary>دليل مختصر يُعرض فى «لماذا؟».</summary>
    public class AiSuggestEvidence
    {
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public string TargetName { get; set; }
    }

    /// <summary>مرشح بعد الترتيب.</summary>
    public class AiSuggestCandidate
    {
        public long TargetId { get; set; }
        public string TargetCode { get; set; }
        public string TargetName { get; set; }
        public long SecondaryId { get; set; }
        public string SecondaryName { get; set; }
        public double Score { get; set; }
        /// <summary>نصيب المرشح من مجموع الدرجات (0..1).</summary>
        public double Share { get; set; }
        public bool ExactHit { get; set; }
        public List<AiSuggestEvidence> Evidence { get; set; } = new List<AiSuggestEvidence>();
    }

    /// <summary>ما يُعاد للواجهة.</summary>
    public class AiSuggestion
    {
        public long Id { get; set; }
        public string Kind { get; set; }
        public long TargetId { get; set; }
        public string TargetCode { get; set; }
        public string TargetName { get; set; }
        public long SecondaryId { get; set; }
        public string SecondaryName { get; set; }
        public double Confidence { get; set; }
        public string Source { get; set; }
        public string Why { get; set; }
        public List<AiSuggestEvidence> Evidence { get; set; } = new List<AiSuggestEvidence>();
    }

    /// <summary>صف AI_Suggestions.</summary>
    public class AiSuggestionRow
    {
        public long Id { get; set; }
        public string Kind { get; set; }
        public long User_Id { get; set; }
        public long Branch_Id { get; set; }
        public string InputText { get; set; }
        public string InputHash { get; set; }
        public string SuggestedJson { get; set; }
        public string Source { get; set; }
        public decimal? Confidence { get; set; }
        public string Outcome { get; set; }
        public string FinalJson { get; set; }
        public string Screen { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
    }

    /// <summary>نسبة القبول لنوع واحد (ص18).</summary>
    public class AiSuggestStats
    {
        public string Kind { get; set; }
        public int Shown { get; set; }
        public int Applied { get; set; }
        public int Dismissed { get; set; }
        public int Overridden { get; set; }
        public int Total => Shown + Applied + Dismissed + Overridden;
        public double AcceptRate => (Applied + Overridden + Dismissed) == 0 ? 0 : (double)Applied / (Applied + Overridden + Dismissed);
    }

    /// <summary>حالة الفهرس للتبويب.</summary>
    public class AiSuggestIndexStatus
    {
        public string Kind { get; set; }
        public int Rows { get; set; }
        public int DistinctTexts { get; set; }
        public int Vectors { get; set; }
        public DateTime? BuiltAt { get; set; }
        public string Error { get; set; }
    }
}
