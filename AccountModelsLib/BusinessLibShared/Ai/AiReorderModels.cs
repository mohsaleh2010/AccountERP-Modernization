using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AccountModelsLib
{
    /// <summary>P8.6 (ai-assistant-p86-plan.md): ثوابت إعادة الطلب.</summary>
    public static class AiReorderConst
    {
        public const string Route = "/store/needs";
        public const string PurchaseRoute = "/accounts/invoices/purchase-invoice";
        public const string StatusOpen = "open";
        public const string StatusDrafted = "drafted";
        public const string StatusDismissed = "dismissed";
        public const string MethodHolt = "holt";
        public const string MethodCroston = "croston";
        public const string MethodSeasonal = "seasonal";
        public const string ConfHigh = "high";
        public const string ConfMedium = "medium";
        public const string ConfLow = "low";
        public const int ReviewDays = 7;
    }

    public class AiReorderSettings
    {
        public bool Enabled { get; set; }
        public int LeadDays { get; set; } = 7;
        public double ServiceLevel { get; set; } = 0.95;
        public int MinSales { get; set; } = 4;
        public int HistoryWeeks { get; set; } = 52;
        public int MaxItems { get; set; } = 20000;
        public bool BriefAlert { get; set; } = true;

        public static AiReorderSettings From(IDictionary<string, string> s)
        {
            var r = new AiReorderSettings();
            if (s == null) return r;
            r.Enabled = Get(s, AiSettingKeys.ReorderEnabled, "0") == "1";
            r.BriefAlert = Get(s, AiSettingKeys.ReorderBriefAlert, "1") == "1";
            r.LeadDays = Int(s, AiSettingKeys.ReorderLeadDays, 7, 0, 365);
            r.MinSales = Int(s, AiSettingKeys.ReorderMinSales, 4, 1, 52);
            r.HistoryWeeks = Int(s, AiSettingKeys.ReorderHistoryWeeks, 52, 8, 156);
            r.MaxItems = Int(s, AiSettingKeys.ReorderMaxItems, 20000, 100, 200000);
            double d;
            if (double.TryParse(Get(s, AiSettingKeys.ReorderServiceLevel, "0.95"), NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                r.ServiceLevel = Math.Max(0.5, Math.Min(0.999, d));
            return r;
        }

        private static int Int(IDictionary<string, string> s, string k, int def, int min, int max)
        {
            int v;
            return int.TryParse(Get(s, k, def.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? Math.Max(min, Math.Min(max, v)) : def;
        }

        private static string Get(IDictionary<string, string> s, string k, string def)
        {
            string v;
            return s.TryGetValue(k, out v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : def;
        }
    }

    /// <summary>مدخل المحرك لكل (صنف، مخزن).</summary>
    public class AiReorderInput
    {
        public long UnitItemId { get; set; }
        public long ItemId { get; set; }
        public long StoreId { get; set; }
        public long BranchId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string StoreName { get; set; }
        public string LargeUnit { get; set; }
        /// <summary>عدد الصغرى فى الكبرى.</summary>
        public decimal Factor { get; set; } = 1;
        /// <summary>مبيعات أسبوعية بالصغرى (الأقدم أولاً، آخر عنصر = الأسبوع الأخير الكامل).</summary>
        public double[] Weekly { get; set; } = new double[0];
        public decimal OnHandSmall { get; set; }
        public decimal ManualReorderLarge { get; set; }
        public long LastSupplierId { get; set; }
        public string LastSupplierName { get; set; }
        /// <summary>آخر سعر شراء للوحدة الكبرى.</summary>
        public decimal LastPriceLarge { get; set; }
        public int LeadDays { get; set; }
    }

    /// <summary>صف AI_ReorderSuggestions.</summary>
    public class AiReorderRow
    {
        public long Id { get; set; }
        public long Run_Id { get; set; }
        public long UnitItem_Id { get; set; }
        public long Item_Id { get; set; }
        public long Store_Id { get; set; }
        public long Branch_Id { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string StoreName { get; set; }
        public string LargeUnit { get; set; }
        public decimal Factor { get; set; } = 1;
        public string Method { get; set; }
        public decimal AvgWeekly { get; set; }
        public decimal Sigma { get; set; }
        public int LeadDays { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal SafetyStock { get; set; }
        public decimal OnHand { get; set; }
        public decimal SuggestQtySmall { get; set; }
        public decimal SuggestQtyLarge { get; set; }
        public decimal ManualReorderLarge { get; set; }
        public long? LastSupplier_Id { get; set; }
        public string LastSupplierName { get; set; }
        public decimal? LastPrice { get; set; }
        public decimal? EstCost { get; set; }
        public DateTime? StockoutDate { get; set; }
        public string Confidence { get; set; }
        public string SeriesJson { get; set; }
        public string Status { get; set; } = AiReorderConst.StatusOpen;
        public long? Draft_Id { get; set; }
    }

    public class AiReorderRunRow
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public int? Items { get; set; }
        public int? Suggested { get; set; }
        public int? DurationMs { get; set; }
        public string ErrorText { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }

    public class AiReorderFilter
    {
        public long StoreId { get; set; }
        public string Search { get; set; }
        /// <summary>نفاد خلال X يوماً (0 = الكل).</summary>
        public int StockoutWithinDays { get; set; }
        public bool IncludeDismissed { get; set; }
        public int Page { get; set; }
    }

    public class AiReorderPage
    {
        public List<AiReorderRow> Items { get; set; } = new List<AiReorderRow>();
        public int Total { get; set; }
        public AiReorderRunRow Run { get; set; }
        public const int PageSize = 50;
    }

    public class AiReorderDraftResult
    {
        public List<(long DraftId, string SupplierName, int Lines)> Drafts { get; set; } = new List<(long, string, int)>();
        public int SkippedNoSupplier { get; set; }
    }

    public interface IAiReorderGateway
    {
        bool CanView(AiUserContext user);
        AiReorderPage List(AiReorderFilter filter, AiUserContext user);
        bool Dismiss(long id, AiUserContext user);
        AiReorderDraftResult CreateDrafts(IList<long> ids, AiUserContext user);
        System.Threading.Tasks.Task<string> ExplainAsync(long id, string language, AiUserContext user, System.Threading.CancellationToken ct);
        System.Threading.Tasks.Task<AiReorderRunRow> RunNowAsync(AiUserContext user, System.Threading.CancellationToken ct);
        AiReorderSettings GetSettings(AiUserContext user);
        void SaveSettings(AiReorderSettings s, AiUserContext user);
        List<(long SupplierId, string Name, int LeadDays)> SupplierLeads(AiUserContext user);
        void SetSupplierLead(long supplierId, int? leadDays, AiUserContext user);
    }

    public class AiReorderException : InvalidOperationException
    {
        public string Code { get; }
        public AiReorderException(string code, string message) : base(message) { Code = code ?? ""; }
    }

    /// <summary>
    /// P8.6.1 (ai-assistant-p86-plan.md ط2–ط8، ط10): التنبؤ ونقطة إعادة الطلب (pure، بلا قاعدة بيانات).
    /// Holt (مستوى + اتجاه) للمنتظم، Croston/SBA للمتقطع (&gt; 50% أسابيع صفر)، وموسمية بسيطة (52 أسبوعاً) إن توفر ≥ 104 أسبوعاً ومعامل دال.
    /// </summary>
    public static class AiForecast
    {
        public class Result
        {
            public string Method { get; set; }
            public double WeeklyForecast { get; set; }
            public double Sigma { get; set; }
            public int NonZeroWeeks { get; set; }
        }

        public static Result Forecast(double[] w)
        {
            var r = new Result { Method = AiReorderConst.MethodHolt };
            if (w == null || w.Length == 0) return r;
            r.NonZeroWeeks = w.Count(x => x > 0);
            double zeroShare = 1 - (double)r.NonZeroWeeks / w.Length;
            if (zeroShare > 0.5)
            {
                r.Method = AiReorderConst.MethodCroston;
                r.WeeklyForecast = Croston(w, 0.1);
                r.Sigma = StdDev(w);
                return r;
            }
            double[] fitted;
            r.WeeklyForecast = Math.Max(0, Holt(w, 0.3, 0.1, out fitted));
            r.Sigma = Rmse(w, fitted);
            if (w.Length >= 104)
            {
                double idx = SeasonalIndex(w);
                if (Math.Abs(idx - 1) >= 0.2)
                {
                    r.Method = AiReorderConst.MethodSeasonal;
                    r.WeeklyForecast = Math.Max(0, r.WeeklyForecast * idx);
                }
            }
            return r;
        }

        /// <summary>Holt الخطى: يعيد توقع الأسبوع التالى وقيم مطابقة خطوة بخطوة (للخطأ).</summary>
        public static double Holt(double[] y, double alpha, double beta, out double[] fitted)
        {
            fitted = new double[y.Length];
            if (y.Length == 0) return 0;
            double level = y[0], trend = y.Length > 1 ? y[1] - y[0] : 0;
            fitted[0] = y[0];
            for (int t = 1; t < y.Length; t++)
            {
                fitted[t] = level + trend;
                double prevLevel = level;
                level = alpha * y[t] + (1 - alpha) * (level + trend);
                trend = beta * (level - prevLevel) + (1 - beta) * trend;
            }
            return level + trend;
        }

        /// <summary>Croston مع تصحيح SBA (1 − α/2).</summary>
        public static double Croston(double[] y, double alpha)
        {
            double z = 0, p = 0; int q = 1; bool init = false;
            foreach (var v in y)
            {
                if (v > 0)
                {
                    if (!init) { z = v; p = q; init = true; }
                    else { z = alpha * v + (1 - alpha) * z; p = alpha * q + (1 - alpha) * p; }
                    q = 1;
                }
                else q++;
            }
            if (!init || p <= 0) return 0;
            return (1 - alpha / 2) * z / p;
        }

        /// <summary>نسبة متوسط الأسابيع الأربعة القادمة من العام الماضى إلى متوسط العام الماضى.</summary>
        public static double SeasonalIndex(double[] y)
        {
            int n = y.Length;
            if (n < 104) return 1;
            var lastYear = y.Skip(n - 52).ToArray();
            double avg = lastYear.Average();
            if (avg <= 0) return 1;
            // الأسابيع المقابلة للأسابيع القادمة = بعد 52 أسبوعاً من نهاية العام قبل الماضى
            var next = y.Skip(n - 52).Take(4).ToArray();
            var prevYearSame = y.Skip(n - 104).Take(52).ToArray();
            double prevAvg = prevYearSame.Average();
            if (prevAvg <= 0) return 1;
            double nextPrev = y.Skip(n - 104).Take(4).Average();
            // معامل الموسم لنفس الأسابيع فى السنتين: متوسطهما
            double i1 = next.Average() / avg, i2 = nextPrev / prevAvg;
            return (i1 + i2) / 2;
        }

        public static double StdDev(double[] y)
        {
            if (y.Length < 2) return 0;
            double m = y.Average();
            return Math.Sqrt(y.Sum(v => (v - m) * (v - m)) / (y.Length - 1));
        }

        public static double Rmse(double[] y, double[] f)
        {
            if (y.Length < 3) return StdDev(y);
            double s = 0; int n = 0;
            for (int i = 1; i < y.Length; i++) { s += (y[i] - f[i]) * (y[i] - f[i]); n++; }
            return n == 0 ? 0 : Math.Sqrt(s / n);
        }

        /// <summary>z لمستوى الخدمة (تقريب Acklam المختصر لدوال التوزيع الطبيعى العكسى).</summary>
        public static double Z(double p)
        {
            p = Math.Max(0.5, Math.Min(0.999, p));
            // تقريب Abramowitz-Stegun 26.2.23
            double t = Math.Sqrt(-2 * Math.Log(1 - p));
            return t - (2.515517 + 0.802853 * t + 0.010328 * t * t) / (1 + 1.432788 * t + 0.189269 * t * t + 0.001308 * t * t * t);
        }

        /// <summary>يحسب صف الاقتراح (ط3، ط6، ط10). null = بيانات غير كافية.</summary>
        public static AiReorderRow Suggest(AiReorderInput i, AiReorderSettings s, DateTime today)
        {
            var w = i.Weekly ?? new double[0];
            int sales = w.Count(x => x > 0);
            var f = Forecast(w);
            double weekly = f.WeeklyForecast, daily = weekly / 7.0;
            int lead = i.LeadDays > 0 ? i.LeadDays : s.LeadDays;
            double lr = (lead + AiReorderConst.ReviewDays) / 7.0;
            double safety = Z(s.ServiceLevel) * f.Sigma * Math.Sqrt(Math.Max(lr, 0));
            double rop = weekly * (lead / 7.0) + safety;
            double target = rop + weekly * (AiReorderConst.ReviewDays / 7.0);
            double onHand = (double)i.OnHandSmall;
            double need = Math.Max(0, target - onHand);
            decimal factor = i.Factor <= 0 ? 1 : i.Factor;
            decimal large = need <= 0 ? 0 : Math.Ceiling((decimal)need / factor);
            var row = new AiReorderRow
            {
                UnitItem_Id = i.UnitItemId, Item_Id = i.ItemId, Store_Id = i.StoreId, Branch_Id = i.BranchId,
                ItemName = i.ItemName, ItemCode = i.ItemCode, StoreName = i.StoreName, LargeUnit = i.LargeUnit, Factor = factor,
                Method = f.Method, AvgWeekly = Math.Round((decimal)weekly, 4), Sigma = Math.Round((decimal)f.Sigma, 4), LeadDays = lead,
                ReorderPoint = Math.Round((decimal)rop, 4), SafetyStock = Math.Round((decimal)safety, 4), OnHand = i.OnHandSmall,
                SuggestQtyLarge = large, SuggestQtySmall = large * factor, ManualReorderLarge = i.ManualReorderLarge,
                LastSupplier_Id = i.LastSupplierId > 0 ? (long?)i.LastSupplierId : null, LastSupplierName = i.LastSupplierName,
                LastPrice = i.LastPriceLarge > 0 ? (decimal?)i.LastPriceLarge : null,
                EstCost = i.LastPriceLarge > 0 ? (decimal?)Math.Round(large * i.LastPriceLarge, 2) : null,
                StockoutDate = daily > 0 ? (DateTime?)today.Date.AddDays(Math.Max(0, Math.Floor(Math.Max(0, onHand) / daily))) : null,
                Confidence = sales < s.MinSales ? AiReorderConst.ConfLow : (sales >= 26 && w.Length >= 52 ? AiReorderConst.ConfHigh : AiReorderConst.ConfMedium),
                SeriesJson = "[" + string.Join(",", w.Skip(Math.Max(0, w.Length - 26)).Select(x => Math.Round(x, 2).ToString(CultureInfo.InvariantCulture))) + "]",
            };
            return row;
        }
    }
}
