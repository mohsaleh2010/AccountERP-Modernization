using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// مسار المندوب (المرحلة 7): الجهاز يسجل موقعه أثناء الوردية المفتوحة فقط ويرفع النقاط مع المزامنة؛ المكتب يرى مسار اليوم على خريطة
    /// مع الزيارات والتوقفات والمسافة. يفعّله المكتب لكل مندوب (مطفأ افتراضياً). النقاط خارج وردية أو لمندوب غير مفعّل تُهمل،
    /// والمكرر (نفس الجهاز والوقت) يُتجاهل فالرفع آمن للتكرار.
    /// </summary>
    public class SalesRepTrackService
    {
        public const int DefaultIntervalSec = 120;
        public const int MaxBatch = 1000;
        /// <summary>دقة أسوأ من هذا لا تدخل فى المسافة ولا التوقفات (تُحفظ وتُعرض فقط).</summary>
        public const int GoodAccuracyM = 100;

        private readonly operations _external;
        public SalesRepTrackService(operations external = null) { _external = external; }

        public class PointInput
        {
            public DateTime Recorded_At { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public int? Accuracy_M { get; set; }
            public decimal? Speed_Kmh { get; set; }
        }

        public class UploadResult
        {
            public int Accepted { get; set; }
            public int Skipped { get; set; }
        }

        public class PointRow
        {
            public DateTime Recorded_At { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public int? Accuracy_M { get; set; }
        }

        public class VisitPin
        {
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public DateTime? Check_In_At { get; set; }
            public DateTime? Check_Out_At { get; set; }
            public decimal? Check_In_Lat { get; set; }
            public decimal? Check_In_Lng { get; set; }
            public int? Distance_From_Customer_M { get; set; }
            public bool? Inside_Geofence { get; set; }
            public int? Result { get; set; }
            public bool Planned { get; set; }
        }

        public class Stop
        {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public int Minutes => (int)Math.Round((To - From).TotalMinutes);
        }

        public class DayTrack
        {
            public List<PointRow> Points { get; set; } = new();
            public List<VisitPin> Visits { get; set; } = new();
            public List<Stop> Stops { get; set; } = new();
            public decimal Distance_Km { get; set; }
            public DateTime? First_At { get; set; }
            public DateTime? Last_At { get; set; }
            public bool Tracking_Enabled { get; set; }
        }

        private class ShiftSpan
        {
            public long Id { get; set; }
            public DateTime From { get; set; }
            public DateTime? To { get; set; }
        }

        // ============================== الجهاز ==============================

        public UploadResult Upload(SalesRepDeviceService.DeviceIdentity id, IEnumerable<PointInput> points) => Tx(opp =>
        {
            var list = (points ?? Enumerable.Empty<PointInput>()).Where(p => p != null).ToList();
            if (list.Count > MaxBatch) throw new InvalidOperationException($"أقصى {MaxBatch} نقطة فى الدفعة.");
            var result = new UploadResult();
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == id.RepId);
            if (rep == null || !rep.Track_Location || list.Count == 0) { result.Skipped = list.Count; return result; }

            DateTime now = DateTime.Now, oldest = now.AddDays(-8);
            DateTime min = Second(list.Min(p => p.Recorded_At)), max = Second(list.Max(p => p.Recorded_At));
            var shifts = opp.db.Database.SqlQuery<ShiftSpan>($@"
SELECT Id, ISNULL(Opened_At, Shift_Date) AS [From], Closed_At AS [To] FROM SalesRep_Shifts
WHERE Rep_Id = {id.RepId} AND ISNULL(Opened_At, Shift_Date) <= {max} AND (Closed_At IS NULL OR Closed_At >= {min})").ToList();
            var have = opp.db.SalesRep_Track_Points.AsNoTracking().Where(t => t.Device_Id == id.DeviceId && t.Recorded_At >= min && t.Recorded_At <= max)
                .Select(t => t.Recorded_At).ToList().ToHashSet();

            foreach (var p in list.OrderBy(p => p.Recorded_At))
            {
                DateTime t = Second(p.Recorded_At);
                var shift = shifts.FirstOrDefault(s => s.From <= t && (s.To ?? now) >= t);
                bool ok = shift != null && t <= now.AddMinutes(5) && t >= oldest
                          && p.Latitude is >= -90 and <= 90 && p.Longitude is >= -180 and <= 180 && !(p.Latitude == 0 && p.Longitude == 0)
                          && p.Accuracy_M is null or (>= 0 and <= 5000) && have.Add(t);
                if (!ok) { result.Skipped++; continue; }
                var row = new SalesRep_Track_Point
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(), Device_Id = id.DeviceId, Rep_Id = id.RepId, Shift_Id = shift.Id, Recorded_At = t,
                    Latitude = Math.Round(p.Latitude, 6), Longitude = Math.Round(p.Longitude, 6), Accuracy_M = p.Accuracy_M,
                    Speed_Kmh = p.Speed_Kmh is decimal sp && sp >= 0 && sp < 1000 ? Math.Round(sp, 2) : null,
                };
                row.StampInsert(id.BranchId);
                opp.db.SalesRep_Track_Points.Add(row);
                result.Accepted++;
            }
            opp.db.SaveChanges();
            return result;
        });

        /// <summary>الوقت بالثانية الكاملة كما يُخزَّن ويُقارَن (datetime فى SQL Server ليس دقيقاً فى أجزاء الثانية).</summary>
        private static DateTime Second(DateTime t) => new DateTime(t.Ticks - t.Ticks % TimeSpan.TicksPerSecond, t.Kind);

        // ============================== المكتب ==============================

        /// <summary>مسار مندوب فى يوم: النقاط بالترتيب، زيارات ورديات اليوم بموقع الحضور، التوقفات (5 دقائق فأكثر فى 60 م)، والمسافة.</summary>
        public DayTrack GetDay(long repId, DateTime date) => Read(opp =>
        {
            DateTime from = date.Date, to = from.AddDays(1);
            var day = new DayTrack
            {
                Tracking_Enabled = opp.db.SalesRep_Reps.AsNoTracking().Where(r => r.Id == repId).Select(r => r.Track_Location).FirstOrDefault(),
                Points = opp.db.SalesRep_Track_Points.AsNoTracking().Where(t => t.Rep_Id == repId && t.Recorded_At >= from && t.Recorded_At < to)
                    .OrderBy(t => t.Recorded_At)
                    .Select(t => new PointRow { Recorded_At = t.Recorded_At, Latitude = t.Latitude, Longitude = t.Longitude, Accuracy_M = t.Accuracy_M }).ToList(),
                Visits = opp.db.Database.SqlQuery<VisitPin>($@"
SELECT v.Customer_Id, ISNULL(c.Name, N'') AS Customer_Name, v.Check_In_At, v.Check_Out_At, v.Check_In_Lat, v.Check_In_Lng,
       v.Distance_From_Customer_M, v.Inside_Geofence, v.Result, v.Planned
FROM SalesRep_Visits v JOIN SalesRep_Shifts s ON s.Id = v.Shift_Id LEFT JOIN Customer c ON c.Id = v.Customer_Id
WHERE s.Rep_Id = {repId} AND s.Shift_Date >= {from} AND s.Shift_Date < {to}
ORDER BY CASE WHEN v.Check_In_At IS NULL THEN 1 ELSE 0 END, v.Check_In_At, v.Id").ToList(),
            };
            day.First_At = day.Points.FirstOrDefault()?.Recorded_At;
            day.Last_At = day.Points.LastOrDefault()?.Recorded_At;
            day.Distance_Km = DistanceKm(day.Points);
            day.Stops = Stops(day.Points);
            return day;
        });

        /// <summary>طول المسار بالكيلومتر: النقاط الدقيقة فقط، وتُهمل القفزات غير الممكنة (أسرع من 150 كم/س).</summary>
        public static decimal DistanceKm(IReadOnlyList<PointRow> points)
        {
            double m = 0;
            PointRow prev = null;
            foreach (var p in points.Where(p => p.Accuracy_M is null or <= GoodAccuracyM))
            {
                if (prev != null)
                {
                    int d = SalesRepVisitService.DistanceM(prev.Latitude, prev.Longitude, p.Latitude, p.Longitude);
                    double hours = Math.Max((p.Recorded_At - prev.Recorded_At).TotalHours, 1.0 / 3600);
                    if (d / 1000.0 / hours > 150) continue;   // نقطة شاذة: تُهمل ويُقاس التالى من آخر نقطة سليمة
                    m += d;
                }
                prev = p;
            }
            return Math.Round((decimal)(m / 1000), 2);
        }

        /// <summary>التوقفات: نقاط متتالية داخل نصف قطر من أول نقطة فيها لمدة لا تقل عن الحد.</summary>
        public static List<Stop> Stops(IReadOnlyList<PointRow> points, int minMinutes = 5, int radiusM = 60)
        {
            var stops = new List<Stop>();
            var good = points.Where(p => p.Accuracy_M is null or <= GoodAccuracyM).ToList();
            int i = 0;
            while (i < good.Count)
            {
                int j = i;
                while (j + 1 < good.Count && SalesRepVisitService.DistanceM(good[i].Latitude, good[i].Longitude, good[j + 1].Latitude, good[j + 1].Longitude) <= radiusM) j++;
                if ((good[j].Recorded_At - good[i].Recorded_At).TotalMinutes >= minMinutes)
                {
                    var cluster = good.GetRange(i, j - i + 1);
                    stops.Add(new Stop
                    {
                        From = good[i].Recorded_At, To = good[j].Recorded_At,
                        Latitude = Math.Round(cluster.Average(p => p.Latitude), 6), Longitude = Math.Round(cluster.Average(p => p.Longitude), 6),
                    });
                }
                i = j + 1;
            }
            return stops;
        }

        private T Read<T>(Func<operations, T> f) => f(_external ?? new operations());

        private T Tx<T>(Func<operations, T> f)
        {
            if (_external != null) return f(_external);
            var opp = new operations(2);
            try
            {
                var r = f(opp);
                opp.Commit();
                return r;
            }
            catch
            {
                opp.Rollback();
                throw;
            }
        }
    }
}
