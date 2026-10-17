namespace AccountERP.SalesRepApp.Core
{
    /// <summary>
    /// تسجيل مسار المندوب على الجهاز: يعمل فقط والمندوب داخل وردية مفتوحة والتتبع مفعّل له من المكتب. يقبل النقطة لو مر فاصل التسجيل
    /// منذ آخر نقطة، أو تحرك 150 م فأكثر بعد 30 ثانية على الأقل؛ ويُهمل الدقة الأسوأ من 150 م. النقاط تُرفع مع المزامنة.
    /// </summary>
    public sealed class RepTracker
    {
        public const int WorstAccuracyM = 150, MoveM = 150, MinGapSec = 30;
        private readonly LocalStore _store;
        private readonly RepSession _session;
        private TrackPoint? _last;

        public RepTracker(LocalStore store, RepSession session) { _store = store; _session = session; }

        public RepProfile? Profile => _store.Rows<RepProfile>("profile").FirstOrDefault();

        /// <summary>هل يجب أن يعمل التتبع الآن؟</summary>
        public bool ShouldRun => _session.LoggedIn && Profile is { Track_Location: true, ShiftOpen: true };

        public int IntervalSec => Math.Clamp(Profile?.Track_Interval_Sec ?? 120, 30, 900);

        public static bool Accept(TrackPoint? last, TrackPoint p, int intervalSec)
        {
            if (p.Accuracy_M is > WorstAccuracyM) return false;
            if (p.Latitude is < -90 or > 90 || p.Longitude is < -180 or > 180 || (p.Latitude == 0 && p.Longitude == 0)) return false;
            if (last == null) return true;
            double gap = (p.Recorded_At - last.Recorded_At).TotalSeconds;
            if (gap < MinGapSec) return false;
            if (gap >= intervalSec) return true;
            return SalesBook.DistanceM(last.Latitude, last.Longitude, p.Latitude, p.Longitude) >= MoveM;
        }

        /// <summary>يسجل النقطة لو استوفت الشروط. يرجع true لو حُفظت.</summary>
        public bool Record(TrackPoint p)
        {
            if (!ShouldRun || !Accept(_last, p, IntervalSec)) return false;
            p.Latitude = Math.Round(p.Latitude, 6);
            p.Longitude = Math.Round(p.Longitude, 6);
            _store.AddTrack(p);
            _last = p;
            return true;
        }
    }
}
