namespace AccountERP.SalesRepApp.Core
{
    public enum VisitState { NotVisited = 0, InProgress = 1, Done = 2 }

    /// <summary>حال عميل فى زيارات اليوم: من خطة اليوم + زيارات الوردية على السيرفر + ما على الجهاز ولم يُرفع بعد.</summary>
    public class VisitStatus
    {
        public long Customer_Id { get; set; }
        public string Customer_Name { get; set; } = "";
        public bool Planned { get; set; }
        public string Route_Name { get; set; } = "";
        public int Sequence { get; set; } = int.MaxValue;
        public VisitState State { get; set; }
        public DateTime? Check_In_At { get; set; }
        public DateTime? Check_Out_At { get; set; }
        public int? Distance_M { get; set; }
        public int? Radius_M { get; set; }
        public bool? Inside { get; set; }
        /// <summary>SalesRepVisitResult (الأعلى أولوية بين مستندات الزيارة).</summary>
        public int? Result { get; set; }
        public int Docs { get; set; }
        /// <summary>فيه حركة على الجهاز لم يحفظها السيرفر بعد.</summary>
        public bool Pending { get; set; }
    }

    /// <summary>
    /// الزيارات على الجهاز (المرحلة 7): دخول وخروج من عند العميل بالموقع والمسافة من نطاقه، بلا اتصال.
    /// نفس قواعد السيرفر (SalesRepVisitService): زيارة نشطة واحدة؛ الدخول أو أى مستند عند عميل يُنهى الزيارة المفتوحة عند غيره؛
    /// «لا بيع» تُنهى الزيارة؛ الموقع للتسجيل والمراجعة فقط (خارج النطاق تحذير لا منع).
    /// </summary>
    public sealed partial class SalesBook
    {
        /// <summary>أولوية نتيجة الزيارة كالسيرفر: تسليم، بيع، طلبية، تحصيل، مرتجع، لا بيع.</summary>
        private static readonly int[] ResultPriority = { 2, 1, 3, 4, 5, 6 };

        public const int DefaultRadiusM = 100;

        public static string ResultName(int? result) => result switch
        {
            1 => "بيع", 2 => "تسليم", 3 => "طلبية", 4 => "تحصيل", 5 => "مرتجع", 6 => "لا بيع", _ => "",
        };

        /// <summary>نتيجة المستند فى الزيارة (= SalesRepVisitResult)، أو null للدخول/الخروج.</summary>
        public static int? ResultOf(DocType type) => type switch
        {
            DocType.Sale => 1, DocType.Delivery => 2, DocType.TakeOrder => 3, DocType.Collection => 4, DocType.Cheque => 4,
            DocType.Return => 5, DocType.NoSale => 6, _ => null,
        };

        public static int? Better(int? current, int? incoming)
        {
            if (incoming == null) return current;
            if (current == null) return incoming;
            int rank(int r) { int i = Array.IndexOf(ResultPriority, r); return i < 0 ? int.MaxValue : i; }
            return rank(incoming.Value) < rank(current.Value) ? incoming : current;
        }

        /// <summary>المسافة بالأمتار (Haversine) = SalesRepVisitService.DistanceM.</summary>
        public static int DistanceM(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            const double r = 6371000;
            double rad(decimal d) => (double)d * Math.PI / 180;
            double dLat = rad(lat2 - lat1), dLng = rad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(rad(lat1)) * Math.Cos(rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return (int)Math.Round(r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
        }

        /// <summary>المسافة من موقع العميل المسجل ونطاقه؛ null لو لا موقع للعميل أو للجهاز.</summary>
        public (int Distance, int Radius, bool Inside)? Geofence(long customerId, decimal? lat, decimal? lng)
        {
            var c = Customer(customerId);
            if (c?.Latitude is not decimal clat || c.Longitude is not decimal clng || lat == null || lng == null) return null;
            int radius = c.Geofence_Radius_M is > 0 ? c.Geofence_Radius_M.Value : DefaultRadiusM;
            int d = DistanceM(clat, clng, lat.Value, lng.Value);
            return (d, radius, d <= radius);
        }

        /// <summary>زيارات اليوم: المخطَّطون بالترتيب ثم غير المخططين حسب وقت الدخول.</summary>
        public List<VisitStatus> TodayVisits()
        {
            var map = new Dictionary<long, VisitStatus>();
            VisitStatus of(long id)
            {
                if (!map.TryGetValue(id, out var v)) map[id] = v = new VisitStatus { Customer_Id = id, Customer_Name = Customer(id)?.Name ?? "" };
                return v;
            }

            foreach (var p in Plan)
            {
                var v = of(p.Customer_Id);
                v.Planned = true; v.Route_Name = p.Route_Name; v.Sequence = p.Sequence;
                if (v.Customer_Name == "") v.Customer_Name = p.Customer_Name;
            }

            long? serverOpen = null;
            foreach (var s in _store.Rows<VisitRow>("visits").OrderBy(x => x.Id))
            {
                var v = of(s.Customer_Id);
                if (v.Customer_Name == "") v.Customer_Name = s.Customer_Name;
                v.Planned |= s.Planned;
                if (s.Check_In_At != null && (v.Check_In_At == null || s.Check_In_At < v.Check_In_At)) v.Check_In_At = s.Check_In_At;
                if (s.Check_In_At != null) v.Check_Out_At = s.Check_Out_At;
                if (s.Distance_From_Customer_M != null && v.Distance_M == null) { v.Distance_M = s.Distance_From_Customer_M; v.Inside = s.Inside_Geofence; }
                v.Result = Better(v.Result, s.Result);
                v.Docs += s.Docs_Count;
                if (s.Check_In_At != null && s.Check_Out_At == null) serverOpen = s.Customer_Id;
            }

            // حركة الجهاز اليوم بالترتيب: تحدد الزيارة النشطة مثل السيرفر. المحفوظ على السيرفر محسوب فى visits بعد التنزيل.
            DateTime today = _now().Date;
            var events = _store.Outbox().Where(o => o.Customer_Id > 0 && o.Status != OutboxStatus.Rejected && o.Created_At.Date == today)
                                        .OrderBy(o => o.Created_At).ToList();
            long? active = events.Count > 0 ? null : serverOpen;
            foreach (var e in events)
            {
                var v = of(e.Customer_Id);
                if (e.Doc_Type == DocType.CheckOut)
                {
                    if (active == e.Customer_Id) { active = null; v.Check_Out_At = e.Created_At; }
                }
                else
                {
                    if (active is long other && other != e.Customer_Id) of(other).Check_Out_At = e.Created_At;
                    if (active != e.Customer_Id) { v.Check_Out_At = null; v.Check_In_At ??= e.Created_At; }
                    active = e.Customer_Id;
                    if (e.Doc_Type == DocType.CheckIn && v.Distance_M == null && Json.Deserialize<VisitInput>(e.Payload) is { } vi
                        && Geofence(e.Customer_Id, vi.Lat, vi.Lng) is { } g)
                    {
                        v.Distance_M = g.Distance; v.Inside = g.Inside;
                    }
                    if (e.Doc_Type == DocType.NoSale) { active = null; v.Check_Out_At = e.Created_At; }
                }
                if (e.Open)
                {
                    v.Pending = true;
                    v.Result = Better(v.Result, ResultOf(e.Doc_Type));
                    if (ResultOf(e.Doc_Type) != null) v.Docs++;
                }
            }

            foreach (var v in map.Values)
            {
                v.Radius_M = Customer(v.Customer_Id)?.Geofence_Radius_M;
                v.State = v.Customer_Id == active ? VisitState.InProgress
                        : v.Check_In_At != null || v.Result != null ? VisitState.Done : VisitState.NotVisited;
                if (v.State == VisitState.InProgress) v.Check_Out_At = null;
            }
            return map.Values.OrderBy(v => v.Planned ? 0 : 1).ThenBy(v => v.Sequence).ThenBy(v => v.Check_In_At ?? DateTime.MaxValue).ThenBy(v => v.Customer_Name).ToList();
        }

        /// <summary>الزيارة الجارية الآن (إن وُجدت).</summary>
        public VisitStatus? ActiveVisit() => TodayVisits().FirstOrDefault(v => v.State == VisitState.InProgress);

        public SaleCheck CheckCheckIn(VisitInput input)
        {
            var r = new SaleCheck();
            CommonChecks(r, Profile, input.Customer_Id);
            var current = ActiveVisit();
            if (current != null && current.Customer_Id == input.Customer_Id) r.Problems.Add("أنت داخل زيارة هذا العميل بالفعل.");
            else if (current != null) r.Warnings.Add($"ستنتهى زيارتك الحالية عند «{current.Customer_Name}».");
            if (input.Lat == null || input.Lng == null) r.Warnings.Add("لم يُحدَّد موقعك؛ تُسجَّل الزيارة بلا مسافة.");
            else if (input.Customer_Id > 0)
            {
                var c = Customer(input.Customer_Id);
                if (c != null && (c.Latitude == null || c.Longitude == null)) r.Warnings.Add("موقع العميل غير مسجل؛ يسجله المكتب لحساب المسافة.");
                else if (Geofence(input.Customer_Id, input.Lat, input.Lng) is { Inside: false } g)
                    r.Warnings.Add($"أنت على بُعد {g.Distance:N0} م من موقع العميل (النطاق {g.Radius:N0} م)؛ تُسجَّل الزيارة وتظهر للمراجعة.");
            }
            return r;
        }

        public Guid SaveCheckIn(VisitInput input)
        {
            var c = CheckCheckIn(input);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            return Enqueue(DocType.CheckIn, Json.Serialize(input), input.Customer_Id, 0);
        }

        public SaleCheck CheckCheckOut(long customerId)
        {
            var r = new SaleCheck();
            if (ActiveVisit()?.Customer_Id != customerId) r.Problems.Add("لا زيارة جارية عند هذا العميل.");
            return r;
        }

        public Guid SaveCheckOut(long customerId)
        {
            var c = CheckCheckOut(customerId);
            if (!c.Ok) throw new InvalidOperationException(string.Join("\n", c.Problems));
            return Enqueue(DocType.CheckOut, Json.Serialize(new VisitInput { Customer_Id = customerId }), customerId, 0);
        }
    }
}
