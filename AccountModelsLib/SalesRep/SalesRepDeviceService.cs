using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// المرحلة 7 (قرار المستخدم 2026-10-17 «رمز تفعيل وPIN»): المكتب يصدر رمز تفعيل لمندوب صالحاً مرة ولمدة محدودة، والمندوب يضع PIN
    /// على الجهاز (يُخزَّن PBKDF2)، والجهاز يأخذ توكن تجديد عشوائياً مربوطاً به (يُخزَّن SHA-256 ويتبدل مع كل استعمال) ويمكن حظره.
    /// لا تمر كلمات مرور النظام عبر الإنترنت. توكن الوصول القصير يصدره مضيف الويب (Data Protection) ويحمل رقم الجهاز وToken_Stamp.
    /// </summary>
    public class SalesRepDeviceService
    {
        public const int DefaultCodeHours = 24;
        public const int MaxPinFailures = 5;
        public static readonly TimeSpan PinLockout = TimeSpan.FromMinutes(15);
        private const string CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";   // بلا حروف ملتبسة (0/O، 1/I/L)
        private const int CodeLength = 10;
        private const int PinIterations = 100_000;

        private readonly operations _external;
        public SalesRepDeviceService(operations external = null) { _external = external; }

        public class Option { public long Id { get; set; } public string Name { get; set; } = ""; }

        public record IssuedCode(long Id, string Code, DateTime Expires_At);

        public class ActivationRow
        {
            public long Id { get; set; }
            public long Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public string Code_Hint { get; set; } = "";
            public DateTime Expires_At { get; set; }
            public long? Act_As_User_Id { get; set; }
            public string Act_As_User_Name { get; set; } = "";
            public DateTime? Used_At { get; set; }
            public long? Device_Id { get; set; }
            public DateTime? Revoked_At { get; set; }
            public DateTime? Insert_ServerDateTime { get; set; }
            public string Issued_By { get; set; } = "";
            /// <summary>0 صالح، 1 مستعمل، 2 منتهٍ، 3 ملغى.</summary>
            public int State => Revoked_At != null ? 3 : Used_At != null ? 1 : Expires_At <= DateTime.Now ? 2 : 0;
        }

        public class DeviceRow
        {
            public long Id { get; set; }
            public long Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public string Device_Uid { get; set; } = "";
            public string Device_Name { get; set; } = "";
            public string Platform { get; set; } = "";
            public string App_Version { get; set; } = "";
            public string Act_As_User_Name { get; set; } = "";
            public DateTime? Activated_At { get; set; }
            public DateTime? Last_Login_At { get; set; }
            public DateTime? Last_Sync_At { get; set; }
            public bool Is_Blocked { get; set; }
            public string Blocked_Reason { get; set; } = "";
            public DateTime? Pin_Locked_Until { get; set; }
            public int Pending_Docs { get; set; }
            public bool Pin_Locked => Pin_Locked_Until > DateTime.Now;
        }

        public class ActivateRequest
        {
            public string Code { get; set; } = "";
            public string Device_Uid { get; set; } = "";
            public string Platform { get; set; }
            public string Device_Name { get; set; }
            public string App_Version { get; set; }
            public string Pin { get; set; } = "";
        }

        public class LoginRequest
        {
            public string Device_Uid { get; set; } = "";
            public string Refresh_Token { get; set; } = "";
            /// <summary>إلزامى للدخول التفاعلى؛ التجديد الخلفى (Refresh) بلا PIN.</summary>
            public string Pin { get; set; }
            public string App_Version { get; set; }
        }

        /// <summary>ما يعود للجهاز بعد التفعيل/الدخول/التجديد: توكن التجديد الجديد (مرة واحدة) وختم التوكن لتوكن الوصول.</summary>
        public class SessionGrant
        {
            public long Device_Id { get; set; }
            public long Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public string Refresh_Token { get; set; } = "";
            public long Token_Stamp { get; set; }
            /// <summary>بادئة أرقام مستندات الجهاز وآخر تسلسل استلمه السيرفر لكل نوع (الجهاز يبدأ بعده لو أُعيد تثبيته).</summary>
            public string Doc_Prefix { get; set; } = "";
            public Dictionary<int, long> Doc_Seqs { get; set; } = new();
        }

        /// <summary>هوية الجهاز الموثَّقة لطلب واحد.</summary>
        public record DeviceIdentity(long DeviceId, long RepId, long BranchId, long UserId, string UserName, string RepName);

        // ============================== المكتب ==============================

        public List<Option> GetReps() => Read(opp => opp.db.Database.SqlQueryRaw<Option>(@"
SELECT r.Id, ISNULL(e.Name, N'') AS Name FROM SalesRep_Reps r LEFT JOIN Employee e ON e.Id = r.Employee_Id
WHERE r.Active = 1 ORDER BY e.Name").ToList());

        /// <summary>مستخدمو النظام النشطون لاختيار من تُسجَّل باسمه مستندات الجهاز.</summary>
        public List<Option> GetUsers() => Read(opp => opp.db.Database.SqlQueryRaw<Option>(@"
SELECT CAST(USER_ID AS bigint) AS Id, ISNULL(NULLIF(NAME, N''), ISNULL(USER_NAME, N'')) AS Name FROM USER_DETAILES ORDER BY Name").ToList());

        /// <summary>يصدر رمز تفعيل لمندوب؛ الرمز النصى يعود هنا فقط ولا يُخزَّن إلا مُجزَّأ.</summary>
        public IssuedCode IssueActivation(long repId, long? actAsUserId = null, int validHours = DefaultCodeHours) => Tx(opp =>
        {
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == repId) ?? throw new InvalidOperationException("المندوب غير موجود.");
            if (!rep.Active) throw new InvalidOperationException("المندوب موقوف.");
            if (validHours < 1 || validHours > 24 * 7) throw new InvalidOperationException("مدة صلاحية الرمز من ساعة إلى 7 أيام.");
            long user = actAsUserId is > 0 ? actAsUserId.Value : Common.User_Id;
            if (opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM USER_DETAILES WHERE USER_ID = {user}").AsEnumerable().First() == 0)
                throw new InvalidOperationException("المستخدم الذى تُسجَّل باسمه المستندات غير موجود.");
            string code = NewCode();
            var row = new SalesRep_Device_Activation
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(), Rep_Id = repId, Code_Hash = HashCode(code), Code_Hint = code.Substring(code.Length - 3),
                Expires_At = DateTime.Now.AddHours(validHours), Act_As_User_Id = user,
            };
            row.StampInsert(rep.Branch_Id ?? Common.Branch_Id);
            opp.db.SalesRep_Device_Activations.Add(row);
            opp.db.SaveChanges();
            return new IssuedCode(row.Id, FormatCode(code), row.Expires_At);
        });

        public List<ActivationRow> GetActivations(bool openOnly, int take = 200) => Read(opp => opp.db.Database.SqlQuery<ActivationRow>($@"
SELECT TOP ({take}) a.Id, a.Rep_Id, ISNULL(e.Name, N'') AS Rep_Name, ISNULL(a.Code_Hint, N'') AS Code_Hint, a.Expires_At, a.Act_As_User_Id,
       ISNULL(NULLIF(u.NAME, N''), ISNULL(u.USER_NAME, N'')) AS Act_As_User_Name, a.Used_At, a.Device_Id, a.Revoked_At, a.Insert_ServerDateTime,
       ISNULL(NULLIF(iu.NAME, N''), ISNULL(iu.USER_NAME, N'')) AS Issued_By
FROM SalesRep_Device_Activations a
LEFT JOIN SalesRep_Reps r ON r.Id = a.Rep_Id LEFT JOIN Employee e ON e.Id = r.Employee_Id
LEFT JOIN USER_DETAILES u ON u.USER_ID = a.Act_As_User_Id LEFT JOIN USER_DETAILES iu ON iu.USER_ID = a.User_Id
WHERE {(openOnly ? 1 : 0)} = 0 OR (a.Used_At IS NULL AND a.Revoked_At IS NULL AND a.Expires_At > GETDATE())
ORDER BY a.Insert_ServerDateTime DESC").ToList());

        public void RevokeActivation(long id) => Tx(opp =>
        {
            var a = opp.db.SalesRep_Device_Activations.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("الرمز غير موجود.");
            if (a.Used_At != null) throw new InvalidOperationException("الرمز استُعمل بالفعل؛ احظر الجهاز بدلاً منه.");
            if (a.Revoked_At == null) { a.Revoked_At = DateTime.Now; a.StampEdit(); opp.db.SaveChanges(); }
            return 0;
        });

        public List<DeviceRow> GetDevices() => Read(opp => opp.db.Database.SqlQuery<DeviceRow>($@"
SELECT d.Id, d.Rep_Id, ISNULL(e.Name, N'') AS Rep_Name, d.Device_Uid, ISNULL(d.Device_Name, N'') AS Device_Name, ISNULL(d.Platform, N'') AS Platform,
       ISNULL(d.App_Version, N'') AS App_Version, ISNULL(NULLIF(u.NAME, N''), ISNULL(u.USER_NAME, N'')) AS Act_As_User_Name,
       d.Activated_At, d.Last_Login_At, d.Last_Sync_At, d.Is_Blocked, ISNULL(d.Blocked_Reason, N'') AS Blocked_Reason, d.Pin_Locked_Until,
       (SELECT COUNT(*) FROM SalesRep_Sync_Inbox i WHERE i.Device_Id = d.Id AND i.Status IN ({(int)SalesRepSyncStatus.PendingReview}, {(int)SalesRepSyncStatus.Error})) AS Pending_Docs
FROM SalesRep_Devices d
LEFT JOIN SalesRep_Reps r ON r.Id = d.Rep_Id LEFT JOIN Employee e ON e.Id = r.Employee_Id
LEFT JOIN USER_DETAILES u ON u.USER_ID = d.Act_As_User_Id
ORDER BY d.Is_Blocked, e.Name, d.Last_Login_At DESC").ToList());

        /// <summary>حظر الجهاز: يسرى فوراً (كل طلب يتحقق) ويبطل توكناته. لا يمس مستنداته المرفوعة.</summary>
        public void Block(long deviceId, string reason) => Tx(opp =>
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("سبب الحظر مطلوب.");
            var d = Device(opp, deviceId);
            d.Is_Blocked = true;
            d.Blocked_Reason = SalesRepSaleService.Trim(reason.Trim(), 300);
            d.Token_Hash = null; d.Prev_Token_Hash = null; d.Token_Stamp++;
            d.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        /// <summary>فك الحظر لا يعيد الدخول: الجهاز يحتاج رمز تفعيل جديداً (توكناته أُبطلت عند الحظر).</summary>
        public void Unblock(long deviceId) => Tx(opp =>
        {
            var d = Device(opp, deviceId);
            d.Is_Blocked = false; d.Blocked_Reason = null; d.Pin_Failed_Count = 0; d.Pin_Locked_Until = null;
            d.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        public void ClearPinLock(long deviceId) => Tx(opp =>
        {
            var d = Device(opp, deviceId);
            d.Pin_Failed_Count = 0; d.Pin_Locked_Until = null;
            d.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== الجهاز ==============================

        /// <summary>تفعيل الجهاز برمز المكتب ووضع الـ PIN. الرمز يُستهلك فى نفس المعاملة.</summary>
        public SessionGrant Activate(ActivateRequest req) => Tx(opp =>
        {
            string uid = (req?.Device_Uid ?? "").Trim();
            if (uid.Length < 8 || uid.Length > 200) throw new SalesRepDeviceAuthException("bad_request", "معرّف الجهاز غير صحيح.");
            ValidatePin(req.Pin);
            string hash = HashCode(NormalizeCode(req.Code));
            var a = opp.db.SalesRep_Device_Activations.FirstOrDefault(x => x.Code_Hash == hash);
            if (a == null || a.Revoked_At != null || a.Used_At != null) throw new SalesRepDeviceAuthException("invalid_code", "رمز التفعيل غير صحيح أو استُعمل.");
            if (a.Expires_At <= DateTime.Now) throw new SalesRepDeviceAuthException("expired_code", "انتهت صلاحية رمز التفعيل؛ اطلب رمزاً جديداً من المكتب.");
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == a.Rep_Id);
            if (rep == null || !rep.Active) throw new SalesRepDeviceAuthException("rep_inactive", "المندوب موقوف.");

            var d = opp.db.SalesRep_Devices.FirstOrDefault(x => x.Device_Uid == uid);
            if (d != null && d.Is_Blocked) throw new SalesRepDeviceAuthException("blocked", "هذا الجهاز محظور؛ يفك المكتب الحظر أولاً.");
            DateTime now = DateTime.Now;
            if (d == null)
            {
                d = new SalesRep_Device { Id = CallSnowflakeIdGenerate.GenerateNewId(), Device_Uid = uid };
                d.StampInsert(rep.Branch_Id ?? Common.Branch_Id);
                opp.db.SalesRep_Devices.Add(d);
            }
            else
            {
                d.StampEdit();
                // جهاز انتقل لمندوب آخر: يبدأ تنزيله من الصفر.
                if (d.Rep_Id != a.Rep_Id)
                    opp.db.SalesRep_Device_Datasets.RemoveRange(opp.db.SalesRep_Device_Datasets.Where(s => s.Device_Id == d.Id));
            }
            d.Rep_Id = a.Rep_Id;
            d.Platform = SalesRepSaleService.Trim(req.Platform?.Trim(), 50);
            d.Device_Name = SalesRepSaleService.Trim(req.Device_Name?.Trim(), 100);
            d.App_Version = SalesRepSaleService.Trim(req.App_Version?.Trim(), 30);
            d.Act_As_User_Id = a.Act_As_User_Id;
            d.Pin_Hash = HashPin(req.Pin);
            d.Pin_Failed_Count = 0; d.Pin_Locked_Until = null;
            d.Activated_At = now; d.Last_Login_At = now;
            string token = Rotate(d);
            opp.db.SaveChanges();

            a.Used_At = now; a.Device_Id = d.Id; a.StampEdit();
            opp.db.SaveChanges();
            return Grant(opp, d, token);
        });

        /// <summary>الدخول التفاعلى: توكن التجديد + الـ PIN. 5 أخطاء متتالية تقفل الـ PIN 15 دقيقة.</summary>
        public SessionGrant Login(LoginRequest req)
        {
            // محاولة PIN خاطئة تُحفظ ولو فشل الطلب، فالتحقق خارج معاملة الخطأ.
            var (grant, error) = Tx(opp =>
            {
                var d = DeviceByToken(opp, req);
                if (d.Pin_Locked_Until > DateTime.Now)
                    return ((SessionGrant)null, new SalesRepDeviceAuthException("pin_locked", $"الـ PIN مقفل حتى {d.Pin_Locked_Until:HH:mm} بعد محاولات خاطئة."));
                if (!VerifyPin(req.Pin ?? "", d.Pin_Hash))
                {
                    d.Pin_Failed_Count++;
                    if (d.Pin_Failed_Count >= MaxPinFailures) { d.Pin_Locked_Until = DateTime.Now.Add(PinLockout); d.Pin_Failed_Count = 0; }
                    d.StampEdit();
                    opp.db.SaveChanges();
                    return (null, new SalesRepDeviceAuthException("invalid_pin", "الـ PIN غير صحيح."));
                }
                d.Pin_Failed_Count = 0; d.Pin_Locked_Until = null;
                d.Last_Login_At = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(req.App_Version)) d.App_Version = SalesRepSaleService.Trim(req.App_Version.Trim(), 30);
                string token = Rotate(d);
                d.StampEdit();
                opp.db.SaveChanges();
                return (Grant(opp, d, token), (SalesRepDeviceAuthException)null);
            });
            if (error != null) throw error;
            return grant;
        }

        /// <summary>تجديد خلفى بلا PIN (المزامنة أثناء العمل): يبدّل توكن التجديد.</summary>
        public SessionGrant Refresh(LoginRequest req) => Tx(opp =>
        {
            var d = DeviceByToken(opp, req);
            string token = Rotate(d);
            d.StampEdit();
            opp.db.SaveChanges();
            return Grant(opp, d, token);
        });

        /// <summary>يتحقق من توكن الوصول (رقم الجهاز + الختم) عند كل طلب: الحظر وإيقاف المندوب يسريان فوراً.</summary>
        public DeviceIdentity Authenticate(long deviceId, long tokenStamp) => Read(opp =>
        {
            var d = opp.db.SalesRep_Devices.AsNoTracking().FirstOrDefault(x => x.Id == deviceId)
                    ?? throw new SalesRepDeviceAuthException("invalid_token", "الجهاز غير مسجل.");
            if (d.Is_Blocked) throw new SalesRepDeviceAuthException("blocked", "هذا الجهاز محظور.");
            if (d.Token_Stamp != tokenStamp || d.Token_Hash == null) throw new SalesRepDeviceAuthException("invalid_token", "انتهت الجلسة؛ سجّل الدخول مجدداً.");
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == d.Rep_Id);
            if (rep == null || !rep.Active) throw new SalesRepDeviceAuthException("rep_inactive", "المندوب موقوف.");
            long user = d.Act_As_User_Id ?? 0;
            string userName = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(USER_NAME, N'') AS Value FROM USER_DETAILES WHERE USER_ID = {user}").AsEnumerable().FirstOrDefault() ?? "";
            string repName = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Name, N'') AS Value FROM Employee WHERE Id = {rep.Employee_Id}").AsEnumerable().FirstOrDefault() ?? "";
            return new DeviceIdentity(d.Id, d.Rep_Id, rep.Branch_Id ?? Common.Branch_Id, user, userName, repName);
        });

        /// <summary>جلسة مستخدم لمعالجة طلب الجهاز: المستندات باسم المستخدم المحدد وعلى فرع المندوب.</summary>
        public static UserSession SessionOf(DeviceIdentity id)
        {
            var s = UserSession.FromProcessDefaults();
            s.User_Id = id.UserId;
            s.User_Name = id.UserName;
            s.FullUserName = id.UserName;
            s.Branch_Id = id.BranchId;
            s.Is_User_Admin = 0;
            s.IsAdministration_AlterDBLogin = 0;
            s.Is_Force_Admin_Permission = 0;
            return s;
        }

        /// <summary>يسجل وقت آخر مزامنة (بلا ختم تعديل حتى لا تتضخم أعمدة التدقيق).</summary>
        public static void Touch(operations opp, long deviceId) =>
            opp.db.Database.ExecuteSqlInterpolated($"UPDATE SalesRep_Devices SET Last_Sync_At = {DateTime.Now} WHERE Id = {deviceId}");

        // ============================== التجزئة ==============================

        public static string NormalizeCode(string code) =>
            new string((code ?? "").ToUpperInvariant().Where(ch => CodeAlphabet.IndexOf(ch) >= 0).ToArray());

        public static string FormatCode(string code) => code.Length == CodeLength ? code.Substring(0, 5) + "-" + code.Substring(5) : code;

        public static string HashCode(string normalized) => Sha256Hex("salesrep-activation:" + normalized);

        public static string HashToken(string token) => Sha256Hex("salesrep-refresh:" + token);

        /// <summary>PIN من 4 إلى 8 أرقام، لا رقماً مكرراً ولا تسلسلاً (1234/4321).</summary>
        public static void ValidatePin(string pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length < 4 || pin.Length > 8 || !pin.All(ch => ch >= '0' && ch <= '9'))
                throw new SalesRepDeviceAuthException("weak_pin", "الـ PIN من 4 إلى 8 أرقام.");
            bool same = pin.All(ch => ch == pin[0]);
            bool up = Enumerable.Range(1, pin.Length - 1).All(i => pin[i] - pin[i - 1] == 1);
            bool down = Enumerable.Range(1, pin.Length - 1).All(i => pin[i - 1] - pin[i] == 1);
            if (same || up || down) throw new SalesRepDeviceAuthException("weak_pin", "الـ PIN سهل التخمين؛ لا تكرر رقماً ولا تستعمل تسلسلاً.");
        }

        public static string HashPin(string pin)
        {
            byte[] salt = RandomBytes(16);
            byte[] hash = Pbkdf2(pin, salt, PinIterations);
            return $"{PinIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPin(string pin, string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            var parts = stored.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out int it)) return false;
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expected = Convert.FromBase64String(parts[2]);
            byte[] actual = Pbkdf2(pin ?? "", salt, it);
            return FixedEquals(actual, expected);
        }

        // ============================== داخلى ==============================

        private static SalesRep_Device Device(operations opp, long id) =>
            opp.db.SalesRep_Devices.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("الجهاز غير موجود.");

        private static SalesRep_Device DeviceByToken(operations opp, LoginRequest req)
        {
            string uid = (req?.Device_Uid ?? "").Trim();
            var d = opp.db.SalesRep_Devices.FirstOrDefault(x => x.Device_Uid == uid) ?? throw new SalesRepDeviceAuthException("invalid_token", "الجهاز غير مسجل؛ فعّله برمز من المكتب.");
            if (d.Is_Blocked) throw new SalesRepDeviceAuthException("blocked", "هذا الجهاز محظور.");
            string h = HashToken(req.Refresh_Token ?? "");
            bool current = d.Token_Hash != null && FixedEquals(Encoding.ASCII.GetBytes(h), Encoding.ASCII.GetBytes(d.Token_Hash));
            bool previous = !current && d.Prev_Token_Hash != null && FixedEquals(Encoding.ASCII.GetBytes(h), Encoding.ASCII.GetBytes(d.Prev_Token_Hash));
            if (!current && !previous) throw new SalesRepDeviceAuthException("invalid_token", "انتهت الجلسة؛ فعّل الجهاز برمز جديد من المكتب.");
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == d.Rep_Id);
            if (rep == null || !rep.Active) throw new SalesRepDeviceAuthException("rep_inactive", "المندوب موقوف.");
            // السابق مقبول مرة واحدة (رد سابق ضاع): بعد التبديل لا يبقى إلا الجديد والحالى.
            if (previous) d.Token_Hash = null;
            return d;
        }

        /// <summary>توكن تجديد جديد: الحالى يصير سابقاً، والختم يتغير فتبطل توكنات الوصول القديمة.</summary>
        private static string Rotate(SalesRep_Device d)
        {
            string token = Base64Url(RandomBytes(32));
            d.Prev_Token_Hash = d.Token_Hash;
            d.Token_Hash = HashToken(token);
            d.Token_Stamp = d.Token_Stamp + 1;
            return token;
        }

        private static SessionGrant Grant(operations opp, SalesRep_Device d, string token)
        {
            var rep = opp.db.SalesRep_Reps.AsNoTracking().First(r => r.Id == d.Rep_Id);
            string repName = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Name, N'') AS Value FROM Employee WHERE Id = {rep.Employee_Id}").AsEnumerable().FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(d.Doc_Prefix))
            {
                // M + رقم تسلسلى للجهاز (M001، M002…)، تحت قفل حتى لا يأخذ جهازان نفس البادئة.
                int n = opp.db.Database.SqlQuery<int>($@"
SELECT ISNULL(MAX(TRY_CAST(SUBSTRING(Doc_Prefix, 2, 9) AS int)), 0) + 1 AS Value FROM SalesRep_Devices WITH (UPDLOCK, HOLDLOCK)
WHERE Doc_Prefix LIKE N'M%'").AsEnumerable().First();
                d.Doc_Prefix = "M" + n.ToString("000");
                opp.db.SaveChanges();
            }
            var seqs = opp.db.SalesRep_Sync_Inboxes.AsNoTracking().Where(i => i.Device_Id == d.Id && i.Device_Doc_Seq != null)
                .GroupBy(i => i.Doc_Type).Select(g => new { g.Key, Max = g.Max(i => i.Device_Doc_Seq.Value) }).ToDictionary(x => x.Key, x => x.Max);
            return new SessionGrant { Device_Id = d.Id, Rep_Id = d.Rep_Id, Rep_Name = repName, Refresh_Token = token, Token_Stamp = d.Token_Stamp, Doc_Prefix = d.Doc_Prefix, Doc_Seqs = seqs };
        }

        private static string NewCode()
        {
            byte[] b = RandomBytes(CodeLength);
            var sb = new StringBuilder(CodeLength);
            foreach (byte x in b) sb.Append(CodeAlphabet[x % CodeAlphabet.Length]);
            return sb.ToString();
        }

        private static byte[] RandomBytes(int n)
        {
            byte[] b = new byte[n];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
            return b;
        }

        private static byte[] Pbkdf2(string pin, byte[] salt, int iterations)
        {
#if NET
            return Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(pin), salt, iterations, HashAlgorithmName.SHA256, 32);
#else
            using var kdf = new Rfc2898DeriveBytes(pin, salt, iterations, HashAlgorithmName.SHA256);
            return kdf.GetBytes(32);
#endif
        }

        private static string Sha256Hex(string s)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "").ToLowerInvariant();
        }

        private static string Base64Url(byte[] b) => Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static bool FixedEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
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
                try { opp.Rollback(); } catch { }
                throw;
            }
        }
    }

    /// <summary>خطأ مصادقة جهاز برمز ثابت يترجمه الـ API لحالة HTTP، ورسالة عربية للمندوب.</summary>
    public class SalesRepDeviceAuthException : Exception
    {
        public string Code { get; }
        public SalesRepDeviceAuthException(string code, string message) : base(message) { Code = code; }
    }
}
