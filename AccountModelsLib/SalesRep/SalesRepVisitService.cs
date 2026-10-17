using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 6 (أ): خطوط السير، خطة اليوم، الزيارات، أسباب عدم البيع، ومواقع العملاء.
    /// - الخط: عملاء بيوم زيارة (System.DayOfWeek) ووتيرة: 1 أسبوعى، 2 كل أسبوعين (أسبوع أ/ب = Week_Parity 1/2)، 4 شهرى (أسبوع الشهر 1-4).
    ///   أسبوع أ/ب يُعدّ من السبت 2024-01-06 (لا ISO حتى لا ينكسر التناوب آخر السنة)؛ الأيام 29-31 بلا زيارة شهرية.
    /// - خطة اليوم تُثبَّت عند فتح الوردية (صفوف زيارات مخططة)، فتعديل الخط لاحقاً لا يغيّر الأيام السابقة؛ «تحديث الخطة» يضيف الناقص فقط.
    /// - الزيارة التلقائية (قرار المستخدم): كل مستند لعميل يُربط بزيارته فى الوردية (المفتوحة، ثم المخططة، وإلا تُنشأ غير مخططة).
    ///   زيارة نشطة واحدة: الدخول عند عميل يُخرج المندوب من زيارته المفتوحة عند غيره. النتيجة = أعلى أولوية بين مستندات الزيارة.
    /// - الموقع للتسجيل والمراجعة فقط (ق17): الزيارة خارج النطاق تُقبل بعلامة Inside_Geofence = 0.
    /// </summary>
    public class SalesRepVisitService
    {
        private readonly operations _external;
        public SalesRepVisitService(operations external = null) { _external = external; }

        /// <summary>بداية عدّ أسابيع «كل أسبوعين» (سبت).</summary>
        public static readonly DateTime ParityEpoch = new DateTime(2024, 1, 6);

        /// <summary>أولوية نتيجة الزيارة (الأعلى أولاً): التسليم والبيع تنفيذ فعلى، ثم الطلبية، ثم التحصيل، ثم المرتجع، ثم لا بيع.</summary>
        private static readonly SalesRepVisitResult[] ResultPriority =
        {
            SalesRepVisitResult.Delivery, SalesRepVisitResult.Sale, SalesRepVisitResult.Order,
            SalesRepVisitResult.Collection, SalesRepVisitResult.Return, SalesRepVisitResult.NoSale,
        };

        public class RouteRow
        {
            public long Id { get; set; }
            public string Name { get; set; } = "";
            public long Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public long? Area_Id { get; set; }
            public string Area_Name { get; set; } = "";
            public bool Active { get; set; } = true;
            public int Customers_Count { get; set; }
        }

        public class RouteCustomerRow
        {
            public long Id { get; set; }
            public long Route_Id { get; set; }
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public int Visit_Day { get; set; }
            public int Frequency { get; set; } = 1;
            public int? Week_Parity { get; set; }
            public int Sequence { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public int? Geofence_Radius_M { get; set; }
        }

        public class PlanRow
        {
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public string Route_Name { get; set; } = "";
            public int Sequence { get; set; }
        }

        public class VisitRow
        {
            public long Id { get; set; }
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public bool Planned { get; set; }
            public DateTime? Check_In_At { get; set; }
            public DateTime? Check_Out_At { get; set; }
            public int? Distance_From_Customer_M { get; set; }
            public bool? Inside_Geofence { get; set; }
            public int? Result { get; set; }
            public long? No_Sale_Reason_Id { get; set; }
            public string No_Sale_Reason { get; set; } = "";
            public string Notes { get; set; }
            public int Docs_Count { get; set; }
            public bool Has_Photo { get; set; }
            public bool IsOpen => Check_In_At != null && Check_Out_At == null;
            public bool IsMissed => Check_In_At == null && Result == null;
        }

        public class ReasonRow
        {
            public long Id { get; set; }
            public string Name { get; set; } = "";
            public bool Requires_Photo { get; set; }
            public bool Active { get; set; } = true;
            public int Used_Count { get; set; }
        }

        public class NoSaleInput
        {
            public long Customer_Id { get; set; }
            public long Reason_Id { get; set; }
            public string Notes { get; set; }
            public byte[] Photo { get; set; }
            public string Photo_Content_Type { get; set; }
            public decimal? Lat { get; set; }
            public decimal? Lng { get; set; }
        }

        public class Option { public long Id { get; set; } public string Name { get; set; } = ""; }

        // ============================== الخطوط ==============================

        public List<RouteRow> GetRoutes(long? repId = null) => Read(opp =>
        {
            long rep = repId ?? 0;
            return opp.db.Database.SqlQuery<RouteRow>($@"
SELECT r.Id, r.Name, r.Rep_Id, ISNULL(e.name, N'') AS Rep_Name, r.Area_Id, ISNULL(a.Area_Name, N'') AS Area_Name, r.Active,
       (SELECT COUNT(*) FROM SalesRep_Route_Customers c WHERE c.Route_Id = r.Id) AS Customers_Count
FROM SalesRep_Routes r JOIN SalesRep_Reps p ON p.Id = r.Rep_Id LEFT JOIN Employee e ON e.Id = p.Employee_Id
LEFT JOIN Govern_Area_List a ON a.Id = r.Area_Id
WHERE {rep} = 0 OR r.Rep_Id = {rep}
ORDER BY CASE WHEN r.Active = 1 THEN 0 ELSE 1 END, e.name, r.Name").ToList();
        });

        public List<Option> GetAreas() => Read(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT CAST(Id AS bigint) AS Id, ISNULL(Area_Name, N'') AS Name FROM Govern_Area_List ORDER BY Area_Name").ToList());

        /// <summary>يضيف خطاً (Id = 0) أو يعدّله. لا حذف: الخط يُوقف فيخرج من خطة الأيام القادمة وتبقى زياراته السابقة.</summary>
        public long SaveRoute(RouteRow input) => Tx(opp =>
        {
            string name = (input?.Name ?? "").Trim();
            if (name.Length == 0) throw new InvalidOperationException("اسم الخط مطلوب.");
            if (name.Length > 200) throw new InvalidOperationException("اسم الخط أطول من 200 حرف.");
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == input.Rep_Id) ?? throw new InvalidOperationException("اختر المندوب.");
            if (opp.db.SalesRep_Routes.Any(r => r.Rep_Id == rep.Id && r.Name == name && r.Id != input.Id))
                throw new InvalidOperationException($"للمندوب خط باسم «{name}».");
            if (input.Area_Id != null && opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Govern_Area_List WHERE Id = {input.Area_Id}").AsEnumerable().First() == 0)
                throw new InvalidOperationException("المنطقة غير موجودة.");
            SalesRep_Route row;
            if (input.Id > 0)
            {
                row = opp.db.SalesRep_Routes.FirstOrDefault(r => r.Id == input.Id) ?? throw new InvalidOperationException("الخط غير موجود.");
                if (row.Rep_Id != rep.Id && opp.db.SalesRep_Route_Customers.Any(c => c.Route_Id == row.Id))
                    CheckCustomersFreeFor(opp, rep.Id, row.Id);
                row.StampEdit();
            }
            else
            {
                row = new SalesRep_Route { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                row.StampInsert(rep.Branch_Id ?? Common.Branch_Id);
                opp.db.SalesRep_Routes.Add(row);
            }
            row.Name = name;
            row.Rep_Id = rep.Id;
            row.Area_Id = input.Area_Id;
            row.Active = input.Active;
            opp.db.SaveChanges();
            return row.Id;
        });

        public List<RouteCustomerRow> GetRouteCustomers(long routeId) => Read(opp => opp.db.Database.SqlQuery<RouteCustomerRow>($@"
SELECT c.Id, c.Route_Id, c.Customer_Id, ISNULL(u.Name, N'') AS Customer_Name, c.Visit_Day, c.Frequency, c.Week_Parity, c.Sequence,
       g.Latitude, g.Longitude, g.Geofence_Radius_M
FROM SalesRep_Route_Customers c LEFT JOIN Customer u ON u.Id = c.Customer_Id LEFT JOIN SalesRep_Customer_Geo g ON g.Customer_Id = c.Customer_Id
WHERE c.Route_Id = {routeId}
ORDER BY c.Visit_Day, c.Sequence, u.Name").ToList());

        /// <summary>
        /// يضيف عميلاً للخط (Id = 0) أو يعدّل يومه ووتيرته وترتيبه. العميل مرة واحدة فى الخط، ولا يكون فى خطين نشطين لنفس المندوب
        /// بنفس اليوم (زيارة مكررة). الوتيرة: 1 بلا أسبوع، 2 أسبوع أ/ب (1/2)، 4 أسبوع الشهر 1-4.
        /// </summary>
        public long SaveRouteCustomer(RouteCustomerRow input) => Tx(opp =>
        {
            if (input == null) throw new InvalidOperationException("لا بيانات.");
            var route = opp.db.SalesRep_Routes.AsNoTracking().FirstOrDefault(r => r.Id == input.Route_Id) ?? throw new InvalidOperationException("الخط غير موجود.");
            if (input.Customer_Id <= 0 || opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Customer WHERE Id = {input.Customer_Id}").AsEnumerable().First() == 0)
                throw new InvalidOperationException("اختر العميل.");
            if (input.Visit_Day < 0 || input.Visit_Day > 6) throw new InvalidOperationException("يوم الزيارة غير صحيح.");
            int? parity = input.Frequency switch
            {
                1 => null,
                2 => input.Week_Parity is 1 or 2 ? input.Week_Parity : throw new InvalidOperationException("اختر أسبوع أ أو ب للزيارة كل أسبوعين."),
                4 => input.Week_Parity is >= 1 and <= 4 ? input.Week_Parity : throw new InvalidOperationException("اختر أسبوع الشهر (1-4) للزيارة الشهرية."),
                _ => throw new InvalidOperationException("الوتيرة غير صحيحة."),
            };
            if (input.Sequence < 0 || input.Sequence > 9999) throw new InvalidOperationException("الترتيب من 0 إلى 9999.");
            if (opp.db.SalesRep_Route_Customers.Any(c => c.Route_Id == route.Id && c.Customer_Id == input.Customer_Id && c.Id != input.Id))
                throw new InvalidOperationException("العميل موجود فى هذا الخط.");
            string other = opp.db.Database.SqlQuery<string>($@"
SELECT TOP 1 r.Name AS Value FROM SalesRep_Route_Customers c JOIN SalesRep_Routes r ON r.Id = c.Route_Id
WHERE c.Customer_Id = {input.Customer_Id} AND c.Visit_Day = {input.Visit_Day} AND r.Rep_Id = {route.Rep_Id} AND r.Active = 1
  AND r.Id <> {route.Id} AND c.Id <> {input.Id}").AsEnumerable().FirstOrDefault();
            if (other != null) throw new InvalidOperationException($"العميل فى خط «{other}» لنفس المندوب ونفس اليوم.");

            SalesRep_Route_Customer row;
            if (input.Id > 0)
            {
                row = opp.db.SalesRep_Route_Customers.FirstOrDefault(c => c.Id == input.Id && c.Route_Id == route.Id) ?? throw new InvalidOperationException("سطر الخط غير موجود.");
                row.StampEdit();
            }
            else
            {
                row = new SalesRep_Route_Customer { Id = CallSnowflakeIdGenerate.GenerateNewId(), Route_Id = route.Id };
                row.StampInsert(route.Branch_Id ?? Common.Branch_Id);
                opp.db.SalesRep_Route_Customers.Add(row);
            }
            row.Customer_Id = input.Customer_Id;
            row.Visit_Day = input.Visit_Day;
            row.Frequency = input.Frequency;
            row.Week_Parity = parity;
            row.Sequence = input.Sequence;
            opp.db.SaveChanges();
            return row.Id;
        });

        /// <summary>يحذف عميلاً من الخط (إعداد لا مستند): يخرج من خطط الأيام القادمة وتبقى زياراته السابقة.</summary>
        public void RemoveRouteCustomer(long id) => Tx(opp =>
        {
            var row = opp.db.SalesRep_Route_Customers.FirstOrDefault(c => c.Id == id) ?? throw new InvalidOperationException("سطر الخط غير موجود.");
            opp.db.SalesRep_Route_Customers.Remove(row);
            opp.db.SaveChanges();
            return 0;
        });

        private static void CheckCustomersFreeFor(operations opp, long repId, long routeId)
        {
            string clash = opp.db.Database.SqlQuery<string>($@"
SELECT TOP 1 r.Name AS Value FROM SalesRep_Route_Customers c
JOIN SalesRep_Route_Customers o ON o.Customer_Id = c.Customer_Id AND o.Visit_Day = c.Visit_Day AND o.Route_Id <> c.Route_Id
JOIN SalesRep_Routes r ON r.Id = o.Route_Id
WHERE c.Route_Id = {routeId} AND r.Rep_Id = {repId} AND r.Active = 1").AsEnumerable().FirstOrDefault();
            if (clash != null) throw new InvalidOperationException($"بعض عملاء الخط فى خط «{clash}» للمندوب الجديد بنفس اليوم.");
        }

        // ============================== موقع العميل ==============================

        /// <summary>يحفظ موقع العميل ونطاقه (صف واحد لكل عميل). lat/lng = null يحذف الموقع.</summary>
        public void SaveCustomerGeo(long customerId, decimal? lat, decimal? lng, int radiusM = 100) => Tx(opp =>
        {
            if (customerId <= 0) throw new InvalidOperationException("اختر العميل.");
            var row = opp.db.SalesRep_Customer_Geos.FirstOrDefault(g => g.Customer_Id == customerId);
            if (lat == null && lng == null)
            {
                if (row != null) { opp.db.SalesRep_Customer_Geos.Remove(row); opp.db.SaveChanges(); }
                return 0;
            }
            if (lat == null || lng == null) throw new InvalidOperationException("أدخل خط العرض وخط الطول معاً.");
            if (lat < -90 || lat > 90) throw new InvalidOperationException("خط العرض من -90 إلى 90.");
            if (lng < -180 || lng > 180) throw new InvalidOperationException("خط الطول من -180 إلى 180.");
            if (radiusM < 10 || radiusM > 5000) throw new InvalidOperationException("نطاق العميل من 10 إلى 5000 متر.");
            if (row == null)
            {
                row = new SalesRep_Customer_Geo { Id = CallSnowflakeIdGenerate.GenerateNewId(), Customer_Id = customerId };
                row.StampInsert(Common.Branch_Id);
                opp.db.SalesRep_Customer_Geos.Add(row);
            }
            else row.StampEdit();
            row.Latitude = Math.Round(lat.Value, 6);
            row.Longitude = Math.Round(lng.Value, 6);
            row.Geofence_Radius_M = radiusM;
            row.Verified_By = Common.User_Id;
            row.Verified_At = DateTime.Now;
            opp.db.SaveChanges();
            return 0;
        });

        /// <summary>المسافة بالأمتار بين نقطتين (Haversine).</summary>
        public static int DistanceM(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            const double r = 6371000;
            double rad(decimal d) => (double)d * Math.PI / 180;
            double dLat = rad(lat2 - lat1), dLng = rad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(rad(lat1)) * Math.Cos(rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return (int)Math.Round(r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
        }

        // ============================== خطة اليوم ==============================

        /// <summary>هل يُزار عميل الخط فى هذا التاريخ حسب يومه ووتيرته؟</summary>
        public static bool IsDue(int visitDay, int frequency, int? weekParity, DateTime date)
        {
            if ((int)date.DayOfWeek != visitDay) return false;
            return frequency switch
            {
                1 => true,
                2 => weekParity == WeekParity(date),
                4 => weekParity == WeekOfMonth(date),
                _ => false,
            };
        }

        /// <summary>أسبوع أ (1) أو ب (2) بعدّ الأسابيع من السبت 2024-01-06.</summary>
        public static int WeekParity(DateTime date)
        {
            int days = (int)Math.Floor((date.Date - ParityEpoch).TotalDays);
            int week = (int)Math.Floor(days / 7.0);
            return ((week % 2) + 2) % 2 == 0 ? 1 : 2;
        }

        /// <summary>أسبوع الشهر 1-4 (الأيام 1-7 = 1 … 22-28 = 4)، و5 للأيام 29-31 (بلا زيارة شهرية).</summary>
        public static int WeekOfMonth(DateTime date) => (date.Day - 1) / 7 + 1;

        /// <summary>عملاء المندوب المطلوب زيارتهم فى تاريخ (من خطوطه النشطة)، بالترتيب ومرة واحدة لكل عميل.</summary>
        public List<PlanRow> PlanFor(long repId, DateTime date) => Read(opp => Plan(opp, repId, date));

        internal static List<PlanRow> Plan(operations opp, long repId, DateTime date)
        {
            var rows = opp.db.Database.SqlQuery<PlanSource>($@"
SELECT c.Customer_Id, ISNULL(u.Name, N'') AS Customer_Name, r.Name AS Route_Name, c.Visit_Day, c.Frequency, c.Week_Parity, c.Sequence
FROM SalesRep_Route_Customers c JOIN SalesRep_Routes r ON r.Id = c.Route_Id LEFT JOIN Customer u ON u.Id = c.Customer_Id
WHERE r.Rep_Id = {repId} AND r.Active = 1 AND c.Visit_Day = {(int)date.DayOfWeek}").ToList();
            return rows.Where(r => IsDue(r.Visit_Day, r.Frequency, r.Week_Parity, date))
                .GroupBy(r => r.Customer_Id).Select(g => g.OrderBy(r => r.Sequence).First())
                .OrderBy(r => r.Sequence).ThenBy(r => r.Customer_Name)
                .Select(r => new PlanRow { Customer_Id = r.Customer_Id, Customer_Name = r.Customer_Name, Route_Name = r.Route_Name, Sequence = r.Sequence }).ToList();
        }

        public class PlanSource
        {
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public string Route_Name { get; set; } = "";
            public int Visit_Day { get; set; }
            public int Frequency { get; set; }
            public int? Week_Parity { get; set; }
            public int Sequence { get; set; }
        }

        /// <summary>يثبّت خطة يوم الوردية: زيارة مخططة لكل عميل مستحق ليست له زيارة فى الوردية. يرجع عدد المضاف.</summary>
        internal static int MaterializePlan(operations opp, SalesRep_Shift shift)
        {
            var have = opp.db.SalesRep_Visits.Where(v => v.Shift_Id == shift.Id).Select(v => v.Customer_Id).ToList().ToHashSet();
            int added = 0;
            foreach (var p in Plan(opp, shift.Rep_Id, shift.Shift_Date))
            {
                if (have.Contains(p.Customer_Id)) continue;
                var v = new SalesRep_Visit { Id = CallSnowflakeIdGenerate.GenerateNewId(), Shift_Id = shift.Id, Customer_Id = p.Customer_Id, Planned = true };
                v.StampInsert(shift.Branch_Id ?? Common.Branch_Id);
                opp.db.SalesRep_Visits.Add(v);
                added++;
            }
            if (added > 0) opp.db.SaveChanges();
            return added;
        }

        /// <summary>«تحديث الخطة»: يضيف للوردية المفتوحة عملاء الخط المستحقين الذين أُضيفوا بعد فتحها. لا يحذف شيئاً.</summary>
        public int RefreshPlan(long repId) => Tx(opp => MaterializePlan(opp, OpenShift(opp, repId)));

        // ============================== الزيارات ==============================

        public List<VisitRow> GetShiftVisits(long shiftId) => Read(opp => opp.db.Database.SqlQuery<VisitRow>($@"
SELECT v.Id, v.Customer_Id, ISNULL(u.Name, N'') AS Customer_Name, v.Planned, v.Check_In_At, v.Check_Out_At, v.Distance_From_Customer_M,
       v.Inside_Geofence, v.Result, v.No_Sale_Reason_Id, ISNULL(n.Name, N'') AS No_Sale_Reason, v.Notes,
       (SELECT COUNT(*) FROM SalesRep_Document_Links l WHERE l.Visit_Id = v.Id) AS Docs_Count,
       CAST(CASE WHEN v.Photo_Id IS NULL THEN 0 ELSE 1 END AS bit) AS Has_Photo
FROM SalesRep_Visits v LEFT JOIN Customer u ON u.Id = v.Customer_Id LEFT JOIN Lookup_SalesRep_NoSaleReasons n ON n.Id = v.No_Sale_Reason_Id
WHERE v.Shift_Id = {shiftId}
ORDER BY v.Planned DESC, v.Id").ToList());

        /// <summary>
        /// تسجيل حضور يدوى عند عميل (اختيارى): يستعمل زيارته المفتوحة أو المخططة أو ينشئ غير مخططة، ويسجّل الموقع إن وُجد
        /// ويحسب المسافة من موقع العميل والنطاق (للمراجعة فقط). يُخرج المندوب من زيارته المفتوحة عند عميل آخر. يرجع رقم الزيارة.
        /// </summary>
        /// <para><paramref name="at"/>: وقت الحضور على جهاز المندوب (المستند المرفوع بلا اتصال)، يُحصر بين فتح الوردية والآن.</para>
        public long CheckIn(long repId, long customerId, decimal? lat = null, decimal? lng = null, DateTime? at = null) => Tx(opp =>
        {
            var shift = OpenShift(opp, repId);
            var v = Active(opp, shift, customerId, When(shift, at));
            if (lat != null && lng != null && v.Check_In_Lat == null) SetLocation(opp, v, lat.Value, lng.Value);
            opp.db.SaveChanges();
            return v.Id;
        });

        /// <summary>
        /// خروج الجهاز من عند العميل: ينهى زيارته المفتوحة فى الوردية إن وُجدت (وإلا لا شىء؛ فحضور لاحق عند عميل آخر أنهاها).
        /// يرجع رقم الزيارة أو 0.
        /// </summary>
        public long CheckOutCustomer(long repId, long customerId, DateTime? at = null) => Tx(opp =>
        {
            var shift = OpenShift(opp, repId);
            var v = opp.db.SalesRep_Visits.Where(x => x.Shift_Id == shift.Id && x.Customer_Id == customerId && x.Check_In_At != null && x.Check_Out_At == null)
                .OrderByDescending(x => x.Id).FirstOrDefault();
            if (v == null) return 0L;
            DateTime when = When(shift, at);
            v.Check_Out_At = when < v.Check_In_At ? v.Check_In_At : when;
            v.StampEdit();
            opp.db.SaveChanges();
            return v.Id;
        });

        /// <summary>زيارات الوردية المفتوحة للمندوب (للجهاز)؛ فارغة إن لم تكن له وردية مفتوحة.</summary>
        internal static List<VisitRow> OpenShiftVisits(operations opp, long repId)
        {
            long shiftId = opp.db.SalesRep_Shifts.AsNoTracking()
                .Where(s => s.Rep_Id == repId && s.Status != (int)SalesRepShiftStatus.Closed && s.Status != (int)SalesRepShiftStatus.Closing)
                .Select(s => s.Id).FirstOrDefault();
            return shiftId == 0 ? new List<VisitRow>() : new SalesRepVisitService(opp).GetShiftVisits(shiftId);
        }

        /// <summary>وقت الجهاز محصوراً بين فتح الوردية والآن (لا مستقبل ولا قبل الوردية).</summary>
        internal static DateTime When(SalesRep_Shift shift, DateTime? at)
        {
            DateTime now = DateTime.Now;
            if (at == null || at > now) return now;
            DateTime from = shift.Opened_At ?? shift.Shift_Date.Date;
            return at < from ? from : at.Value;
        }

        /// <summary>انتهاء الزيارة (الخروج من عند العميل).</summary>
        public void CheckOut(long repId, long visitId) => Tx(opp =>
        {
            var shift = OpenShift(opp, repId);
            var v = opp.db.SalesRep_Visits.FirstOrDefault(x => x.Id == visitId && x.Shift_Id == shift.Id) ?? throw new InvalidOperationException("الزيارة غير موجودة فى الوردية المفتوحة.");
            if (v.Check_In_At == null) throw new InvalidOperationException("لم يُسجَّل حضور فى هذه الزيارة.");
            if (v.Check_Out_At != null) return 0;
            v.Check_Out_At = DateTime.Now;
            v.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        /// <summary>
        /// «لا بيع»: السبب إلزامى، والصورة إلزامية لو السبب يطلبها. يُرفض لزيارة بها مستندات (نتيجتها أعلى من لا بيع).
        /// تُنهى الزيارة. يرجع رقم الزيارة.
        /// </summary>
        public long RecordNoSale(long repId, NoSaleInput input) => Tx(opp =>
        {
            if (input == null || input.Customer_Id <= 0) throw new InvalidOperationException("اختر العميل.");
            var shift = OpenShift(opp, repId);
            var reason = opp.db.Lookup_SalesRep_NoSaleReasons.AsNoTracking().FirstOrDefault(r => r.Id == input.Reason_Id && r.Active)
                ?? throw new InvalidOperationException("اختر سبب عدم البيع.");
            bool hasPhoto = input.Photo != null && input.Photo.Length > 0;
            if (reason.Requires_Photo && !hasPhoto) throw new InvalidOperationException($"السبب «{reason.Name}» يحتاج صورة.");
            if (hasPhoto && input.Photo.Length > 5 * 1024 * 1024) throw new InvalidOperationException("الصورة أكبر من 5 ميجا.");
            var v = Active(opp, shift, input.Customer_Id);
            if (v.Result != null && v.Result != (int)SalesRepVisitResult.NoSale)
                throw new InvalidOperationException("الزيارة بها مستندات؛ لا تُسجَّل «لا بيع».");
            if (input.Lat != null && input.Lng != null && v.Check_In_Lat == null) SetLocation(opp, v, input.Lat.Value, input.Lng.Value);
            v.Result = (int)SalesRepVisitResult.NoSale;
            v.No_Sale_Reason_Id = reason.Id;
            v.Notes = SalesRepSaleService.Trim(string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(), 1000);
            if (hasPhoto)
            {
                var photo = new SalesRep_Attachment
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(), Ref_Type = (int)SalesRepAttachmentRefType.NoSale, Ref_Id = v.Id,
                    File_Data = input.Photo, Content_Type = SalesRepSaleService.Trim(input.Photo_Content_Type ?? "image/jpeg", 100), Taken_At = DateTime.Now,
                    Lat = input.Lat, Lng = input.Lng,
                };
                photo.StampInsert(shift.Branch_Id ?? Common.Branch_Id);
                opp.db.SalesRep_Attachments.Add(photo);
                v.Photo_Id = photo.Id;
            }
            v.Check_Out_At = DateTime.Now;
            v.StampEdit();
            opp.db.SaveChanges();
            return v.Id;
        });

        /// <summary>
        /// الزيارة التلقائية: تُستدعى بعد حفظ مستند لعميل داخل الوردية. تربط روابط المستندات (التى بلا زيارة) بزيارة العميل النشطة
        /// وترفع نتيجتها حسب الأولوية. يرجع رقم الزيارة.
        /// </summary>
        internal static long Attach(operations opp, long shiftId, long customerId, SalesRepVisitResult result, params (SalesRepDocType Type, long Id)[] docs)
        {
            if (customerId <= 0) return 0;
            var shift = opp.db.SalesRep_Shifts.First(s => s.Id == shiftId);
            var v = Active(opp, shift, customerId);
            v.Result = Better(v.Result, result);
            if (v.Result != (int)SalesRepVisitResult.NoSale) v.No_Sale_Reason_Id = null;
            v.StampEdit();
            foreach (var (type, id) in docs)
            {
                var link = opp.db.SalesRep_Document_Links.FirstOrDefault(l => l.Doc_Type == (int)type && l.Doc_Id == id);
                if (link != null && link.Visit_Id == null) link.Visit_Id = v.Id;
            }
            opp.db.SaveChanges();
            return v.Id;
        }

        /// <summary>عند إقفال الوردية: إنهاء الزيارات المفتوحة. الزيارات المخططة بلا حضور تبقى «فائتة».</summary>
        internal static void CloseOpenVisits(operations opp, long shiftId)
        {
            DateTime now = DateTime.Now;
            foreach (var v in opp.db.SalesRep_Visits.Where(x => x.Shift_Id == shiftId && x.Check_In_At != null && x.Check_Out_At == null).ToList())
            {
                v.Check_Out_At = now;
                v.StampEdit();
            }
            opp.db.SaveChanges();
        }

        public static int? Better(int? current, SalesRepVisitResult incoming)
        {
            if (current == null) return (int)incoming;
            int rank(int r) { int i = Array.IndexOf(ResultPriority, (SalesRepVisitResult)r); return i < 0 ? int.MaxValue : i; }
            return rank((int)incoming) < rank(current.Value) ? (int)incoming : current;
        }

        /// <summary>زيارة العميل النشطة فى الوردية: المفتوحة، ثم المخططة التى لم تبدأ، وإلا تُنشأ غير مخططة. تُنهى زيارة مفتوحة لعميل آخر.</summary>
        private static SalesRep_Visit Active(operations opp, SalesRep_Shift shift, long customerId, DateTime? at = null)
        {
            DateTime now = at ?? DateTime.Now;
            foreach (var o in opp.db.SalesRep_Visits.Where(x => x.Shift_Id == shift.Id && x.Customer_Id != customerId && x.Check_In_At != null && x.Check_Out_At == null).ToList())
            {
                o.Check_Out_At = now < o.Check_In_At ? o.Check_In_At : now;
                o.StampEdit();
            }
            var v = opp.db.SalesRep_Visits.Where(x => x.Shift_Id == shift.Id && x.Customer_Id == customerId && x.Check_In_At != null && x.Check_Out_At == null)
                        .OrderByDescending(x => x.Id).FirstOrDefault()
                    ?? opp.db.SalesRep_Visits.Where(x => x.Shift_Id == shift.Id && x.Customer_Id == customerId && x.Result == null)
                        .OrderByDescending(x => x.Check_In_At != null).ThenByDescending(x => x.Id).FirstOrDefault();
            // زيارة بلا نتيجة بعد (مخططة لم تبدأ، أو حضور خرج منه بلا مستند) تُستكمل؛ زيارة انتهت بنتيجة تعنى عودة جديدة فتُنشأ زيارة.
            if (v != null && v.Check_Out_At != null) { v.Check_Out_At = null; v.StampEdit(); }
            if (v == null)
            {
                v = new SalesRep_Visit { Id = CallSnowflakeIdGenerate.GenerateNewId(), Shift_Id = shift.Id, Customer_Id = customerId, Planned = false };
                v.StampInsert(shift.Branch_Id ?? Common.Branch_Id);
                opp.db.SalesRep_Visits.Add(v);
            }
            if (v.Check_In_At == null) { v.Check_In_At = now; v.StampEdit(); }
            return v;
        }

        private static void SetLocation(operations opp, SalesRep_Visit v, decimal lat, decimal lng)
        {
            if (lat < -90 || lat > 90 || lng < -180 || lng > 180) throw new InvalidOperationException("الموقع غير صحيح.");
            v.Check_In_Lat = Math.Round(lat, 6);
            v.Check_In_Lng = Math.Round(lng, 6);
            var geo = opp.db.SalesRep_Customer_Geos.AsNoTracking().FirstOrDefault(g => g.Customer_Id == v.Customer_Id);
            if (geo == null) { v.Distance_From_Customer_M = null; v.Inside_Geofence = null; return; }
            v.Distance_From_Customer_M = DistanceM(geo.Latitude, geo.Longitude, lat, lng);
            v.Inside_Geofence = v.Distance_From_Customer_M <= geo.Geofence_Radius_M;
        }

        private static SalesRep_Shift OpenShift(operations opp, long repId)
        {
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == repId) ?? throw new InvalidOperationException("ملف المندوب غير موجود.");
            if (!rep.Active) throw new InvalidOperationException("ملف المندوب موقوف.");
            return opp.db.SalesRep_Shifts.FirstOrDefault(s => s.Rep_Id == repId && s.Status != (int)SalesRepShiftStatus.Closed && s.Status != (int)SalesRepShiftStatus.Closing)
                ?? throw new InvalidOperationException("لا وردية مفتوحة للمندوب.");
        }

        // ============================== أسباب عدم البيع ==============================

        public List<ReasonRow> GetReasons() => Read(opp => opp.db.Lookup_SalesRep_NoSaleReasons.AsNoTracking().Where(r => r.Active).OrderBy(r => r.Name)
            .Select(r => new ReasonRow { Id = r.Id, Name = r.Name, Requires_Photo = r.Requires_Photo, Active = r.Active }).ToList());

        public List<ReasonRow> GetAllReasons() => Read(opp => opp.db.Database.SqlQuery<ReasonRow>($@"
SELECT r.Id, r.Name, r.Requires_Photo, r.Active, (SELECT COUNT(*) FROM SalesRep_Visits v WHERE v.No_Sale_Reason_Id = r.Id) AS Used_Count
FROM Lookup_SalesRep_NoSaleReasons r ORDER BY CASE WHEN r.Active = 1 THEN 0 ELSE 1 END, r.Name").ToList());

        /// <summary>يضيف سبباً (Id = 0) أو يعدّله. لا حذف: السبب يُوقف فيختفى من الوردية ويبقى على الزيارات القديمة.</summary>
        public long SaveReason(ReasonRow input) => Tx(opp =>
        {
            string name = (input?.Name ?? "").Trim();
            if (name.Length == 0) throw new InvalidOperationException("اسم السبب مطلوب.");
            if (name.Length > 200) throw new InvalidOperationException("اسم السبب أطول من 200 حرف.");
            if (opp.db.Lookup_SalesRep_NoSaleReasons.Any(r => r.Name == name && r.Id != input.Id)) throw new InvalidOperationException($"السبب «{name}» موجود.");
            Lookup_SalesRep_NoSaleReason row;
            if (input.Id > 0)
            {
                row = opp.db.Lookup_SalesRep_NoSaleReasons.FirstOrDefault(r => r.Id == input.Id) ?? throw new InvalidOperationException("السبب غير موجود.");
                if (row.Active && !input.Active && opp.db.Lookup_SalesRep_NoSaleReasons.Count(r => r.Active) <= 1)
                    throw new InvalidOperationException("لا يمكن إيقاف آخر سبب نشط؛ «لا بيع» يحتاج سبباً.");
                row.StampEdit();
            }
            else
            {
                row = new Lookup_SalesRep_NoSaleReason { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                row.StampInsert(Common.Branch_Id);
                opp.db.Lookup_SalesRep_NoSaleReasons.Add(row);
            }
            row.Name = name;
            row.Requires_Photo = input.Requires_Photo;
            row.Active = input.Active;
            opp.db.SaveChanges();
            return row.Id;
        });

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
                try { opp.Rollback(); } catch { }
                throw;
            }
        }
    }
}
