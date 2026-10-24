using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    public enum SalesRepNotifyEvent { OrderTaken = 1, OrderAssigned = 2, OrderDelivered = 3, OrderPartial = 4, OrderRefused = 5, PaymentReceived = 6 }

    public enum SalesRepNotifyChannel { Sms = 1, WhatsApp = 2 }

    public enum SalesRepNotifyStatus { Pending = 0, Sent = 1, Failed = 2, Cancelled = 3, Skipped = 4 }

    /// <summary>0 معطّل (لا يُنشأ شىء فى الطابور)، 1 بوابة HTTP عامة، 2 تجريبى: يعلّم الرسائل مُرسَلة بلا اتصال.</summary>
    public enum SalesRepNotifyProvider { Off = 0, Http = 1, Test = 2 }

    /// <summary>
    /// إشعار العميل SMS/WhatsApp (المرحلة 8، field-sales-van-presales-plan.md §6.4): طابور إرسال (Outbox) يُكتب فى نفس معاملة المستند
    /// فلا يوجد إشعار لمستند تراجع، ويُرسَل لاحقاً من السيرفر (لا من جهاز المندوب) مع إعادة محاولة تدريجية.
    /// البوابة عامة: عنوان HTTPS + ترويسة تفويض + قالب JSON فيه {to} و{message} و{channel}، فتناسب أى مزوّد دون ربط الكود بواحد.
    /// الكتابة فى الطابور لا ترمى أبداً حتى لا يفشل مستند المندوب بسبب إشعار.
    /// </summary>
    public class SalesRepNotifyService
    {
        private readonly operations _external;
        public SalesRepNotifyService(operations external = null) { _external = external; }

        public const string DefaultBodyTemplate = "{\"to\":\"{to}\",\"channel\":\"{channel}\",\"message\":\"{message}\"}";

        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private static readonly object SendLock = new object();

        public class SettingsView
        {
            public int Provider { get; set; }
            public string Endpoint_Url { get; set; } = "";
            public string Auth_Header { get; set; } = "";
            /// <summary>القيمة السرية لا تُرجَع للشاشة أبداً؛ يُرجَع فقط هل هى مُعدَّة.</summary>
            public bool Has_Auth_Value { get; set; }
            public string Body_Template { get; set; } = DefaultBodyTemplate;
            public string Default_Country_Code { get; set; } = "20";
            public int Max_Attempts { get; set; } = 3;
        }

        public class SettingsInput : SettingsView
        {
            /// <summary>قيمة جديدة للتفويض؛ فارغة = تبقى القيمة المحفوظة.</summary>
            public string Auth_Value { get; set; } = "";
            public bool Clear_Auth_Value { get; set; }
        }

        public class TemplateRow
        {
            public long Id { get; set; }
            public int Event_Type { get; set; }
            public int Channel { get; set; }
            public string Body { get; set; } = "";
            public bool Enabled { get; set; }
        }

        public class OutboxRow
        {
            public long Id { get; set; }
            public int Event_Type { get; set; }
            public int Channel { get; set; }
            public long? Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Message { get; set; } = "";
            public int Status { get; set; }
            public int Attempts { get; set; }
            public DateTime? Next_Attempt_At { get; set; }
            public DateTime? Sent_At { get; set; }
            public DateTime? Insert_ServerDateTime { get; set; }
            public string Last_Error { get; set; } = "";
        }

        public class ProcessResult { public int Sent { get; set; } public int Retry { get; set; } public int Failed { get; set; } }

        /// <summary>المتغيرات المتاحة فى القوالب.</summary>
        public static readonly string[] Placeholders = { "{customer}", "{rep}", "{doc_no}", "{amount}", "{date}" };

        // ============================== الإعداد والقوالب ==============================

        public SettingsView GetSettings() => Read(opp =>
        {
            var s = opp.db.SalesRep_Notify_Settings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefault();
            if (s == null) return new SettingsView();
            return new SettingsView
            {
                Provider = s.Provider, Endpoint_Url = s.Endpoint_Url ?? "", Auth_Header = s.Auth_Header ?? "", Has_Auth_Value = !string.IsNullOrEmpty(s.Auth_Value),
                Body_Template = string.IsNullOrWhiteSpace(s.Body_Template) ? DefaultBodyTemplate : s.Body_Template,
                Default_Country_Code = s.Default_Country_Code ?? "", Max_Attempts = s.Max_Attempts,
            };
        });

        public void SaveSettings(SettingsInput input) => Tx(opp =>
        {
            if (!Enum.IsDefined(typeof(SalesRepNotifyProvider), input.Provider)) throw new InvalidOperationException("مزوّد غير معروف.");
            string url = (input.Endpoint_Url ?? "").Trim();
            string body = string.IsNullOrWhiteSpace(input.Body_Template) ? DefaultBodyTemplate : input.Body_Template.Trim();
            if (input.Provider == (int)SalesRepNotifyProvider.Http)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) throw new InvalidOperationException("عنوان البوابة غير صحيح.");
                bool local = uri.IsLoopback;
                if (uri.Scheme != Uri.UriSchemeHttps && !(local && uri.Scheme == Uri.UriSchemeHttp))
                    throw new InvalidOperationException("عنوان البوابة يجب أن يكون HTTPS (يُسمح بـ HTTP للجهاز المحلى فقط).");
                if (body.IndexOf("{message}", StringComparison.Ordinal) < 0 || body.IndexOf("{to}", StringComparison.Ordinal) < 0)
                    throw new InvalidOperationException("قالب الطلب يجب أن يحوى {to} و{message}.");
            }
            string cc = new string((input.Default_Country_Code ?? "").Where(char.IsDigit).ToArray());
            if (cc.Length > 4) throw new InvalidOperationException("رمز الدولة غير صحيح.");
            if (input.Max_Attempts < 1 || input.Max_Attempts > 10) throw new InvalidOperationException("عدد المحاولات بين 1 و10.");
            string header = (input.Auth_Header ?? "").Trim();
            if (header.Length > 0 && header.Any(c => c <= ' ' || c == ':')) throw new InvalidOperationException("اسم ترويسة التفويض غير صحيح.");

            var s = opp.db.SalesRep_Notify_Settings.OrderBy(x => x.Id).FirstOrDefault();
            if (s == null)
            {
                s = new SalesRep_Notify_Setting { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                s.StampInsert(Common.Branch_Id);
                opp.db.SalesRep_Notify_Settings.Add(s);
            }
            else s.StampEdit();
            s.Provider = input.Provider;
            s.Endpoint_Url = url.Length == 0 ? null : url;
            s.Auth_Header = header.Length == 0 ? null : header;
            if (input.Clear_Auth_Value) s.Auth_Value = null;
            else if (!string.IsNullOrEmpty(input.Auth_Value)) s.Auth_Value = input.Auth_Value.Trim();
            s.Body_Template = body;
            s.Default_Country_Code = cc.Length == 0 ? null : cc;
            s.Max_Attempts = input.Max_Attempts;
            opp.db.SaveChanges();
            return 0;
        });

        /// <summary>قوالب كل (حدث، قناة)؛ غير الموجود يُرجَع بالنص الافتراضى ومعطّلاً (لا يُحفظ حتى يحفظه المستخدم).</summary>
        public List<TemplateRow> GetTemplates() => Read(opp =>
        {
            var saved = opp.db.SalesRep_Notify_Templates.AsNoTracking().ToList();
            var rows = new List<TemplateRow>();
            foreach (SalesRepNotifyEvent ev in Enum.GetValues(typeof(SalesRepNotifyEvent)))
                foreach (SalesRepNotifyChannel ch in Enum.GetValues(typeof(SalesRepNotifyChannel)))
                {
                    var t = saved.FirstOrDefault(x => x.Event_Type == (int)ev && x.Channel == (int)ch);
                    rows.Add(t != null
                        ? new TemplateRow { Id = t.Id, Event_Type = t.Event_Type, Channel = t.Channel, Body = t.Body, Enabled = t.Enabled }
                        : new TemplateRow { Event_Type = (int)ev, Channel = (int)ch, Body = DefaultBody(ev), Enabled = false });
                }
            return rows;
        });

        public void SaveTemplate(TemplateRow input) => Tx(opp =>
        {
            if (!Enum.IsDefined(typeof(SalesRepNotifyEvent), input.Event_Type) || !Enum.IsDefined(typeof(SalesRepNotifyChannel), input.Channel))
                throw new InvalidOperationException("الحدث أو القناة غير معروفين.");
            string body = (input.Body ?? "").Trim();
            if (body.Length == 0) throw new InvalidOperationException("نص الرسالة مطلوب.");
            if (body.Length > 500) throw new InvalidOperationException("نص الرسالة أطول من 500 حرف.");
            var t = opp.db.SalesRep_Notify_Templates.FirstOrDefault(x => x.Event_Type == input.Event_Type && x.Channel == input.Channel);
            if (t == null)
            {
                t = new SalesRep_Notify_Template { Id = CallSnowflakeIdGenerate.GenerateNewId(), Event_Type = input.Event_Type, Channel = input.Channel };
                t.StampInsert(Common.Branch_Id);
                opp.db.SalesRep_Notify_Templates.Add(t);
            }
            else t.StampEdit();
            t.Body = body;
            t.Enabled = input.Enabled;
            opp.db.SaveChanges();
            return 0;
        });

        public static string DefaultBody(SalesRepNotifyEvent ev) => ev switch
        {
            SalesRepNotifyEvent.OrderTaken => "عميلنا {customer}، تم تسجيل طلبيتكم رقم {doc_no} بقيمة {amount}. شكراً لتعاملكم معنا.",
            SalesRepNotifyEvent.OrderAssigned => "عميلنا {customer}، طلبيتكم رقم {doc_no} فى الطريق إليكم مع المندوب {rep}.",
            SalesRepNotifyEvent.OrderDelivered => "عميلنا {customer}، تم تسليم طلبيتكم رقم {doc_no} بقيمة {amount}. شكراً لكم.",
            SalesRepNotifyEvent.OrderPartial => "عميلنا {customer}، تم تسليم جزء من طلبيتكم رقم {doc_no} بقيمة {amount}، وسنوافيكم بالباقى.",
            SalesRepNotifyEvent.OrderRefused => "عميلنا {customer}، لم يتم تسليم طلبيتكم رقم {doc_no} اليوم. سنتواصل معكم لتحديد موعد جديد.",
            SalesRepNotifyEvent.PaymentReceived => "عميلنا {customer}، استلمنا منكم مبلغ {amount} بتاريخ {date} (المندوب {rep}). شكراً لكم.",
            _ => "{customer}",
        };

        // ============================== الكتابة فى الطابور ==============================

        /// <summary>
        /// يضيف رسائل الحدث للطابور فى معاملة المستند الجارية: رسالة لكل قناة قالبها مفعَّل. مرة واحدة لكل (حدث، قناة، مرجع).
        /// لا ترمى أبداً. المتغيرات: العميل والمندوب يُقرآن من القاعدة؛ docNo/amount/date اختيارية.
        /// </summary>
        public static void Enqueue(operations opp, SalesRepNotifyEvent ev, long customerId, long repId, long refId, string docNo = null, decimal? amount = null)
        {
            var added = new List<SalesRep_Notify_Outbox>();
            try
            {
                var settings = opp.db.SalesRep_Notify_Settings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefault();
                if (settings == null || settings.Provider == (int)SalesRepNotifyProvider.Off) return;
                var templates = opp.db.SalesRep_Notify_Templates.AsNoTracking().Where(t => t.Event_Type == (int)ev && t.Enabled).ToList();
                if (templates.Count == 0) return;
                var cust = opp.db.Database.SqlQuery<CustomerContact>($"SELECT ISNULL(Name, N'') AS Name, Mobile, Tel, Tel2 FROM Customer WHERE Id = {customerId}").AsEnumerable().FirstOrDefault();
                if (cust == null) return;
                string rep = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(e.name, N'') AS Value FROM SalesRep_Reps r LEFT JOIN Employee e ON e.Id = r.Employee_Id WHERE r.Id = {repId}").AsEnumerable().FirstOrDefault() ?? "";
                string phone = FirstValidPhone(settings.Default_Country_Code, cust.Mobile, cust.Tel, cust.Tel2);
                foreach (var t in templates)
                {
                    if (opp.db.SalesRep_Notify_Outboxes.Any(o => o.Event_Type == (int)ev && o.Channel == t.Channel && o.Ref_Id == refId)) continue;
                    var row = new SalesRep_Notify_Outbox
                    {
                        Id = CallSnowflakeIdGenerate.GenerateNewId(), Event_Type = (int)ev, Channel = t.Channel, Customer_Id = customerId, Rep_Id = repId, Ref_Id = refId,
                        Phone = phone, Message = Render(t.Body, cust.Name, rep, docNo, amount, DateTime.Today),
                        Status = phone == null ? (int)SalesRepNotifyStatus.Skipped : (int)SalesRepNotifyStatus.Pending,
                        Next_Attempt_At = DateTime.Now, Last_Error = phone == null ? "لا رقم هاتف صالح للعميل" : null,
                    };
                    row.StampInsert(Common.Branch_Id);
                    opp.db.SalesRep_Notify_Outboxes.Add(row);
                    added.Add(row);
                }
                if (added.Count > 0) opp.db.SaveChanges();
            }
            catch (Exception ee)
            {
                foreach (var r in added) opp.db.Entry(r).State = EntityState.Detached;
                operations.DataDictionaryLogerException(ee);
            }
        }

        private class CustomerContact { public string Name { get; set; } = ""; public string Mobile { get; set; } public string Tel { get; set; } public string Tel2 { get; set; } }

        public static string Render(string template, string customer, string rep, string docNo, decimal? amount, DateTime date)
        {
            string text = (template ?? "")
                .Replace("{customer}", customer ?? "").Replace("{rep}", rep ?? "").Replace("{doc_no}", docNo ?? "")
                .Replace("{amount}", amount == null ? "" : amount.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Replace("{date}", date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
            return text.Length > 1000 ? text.Substring(0, 1000) : text;
        }

        /// <summary>أول رقم صالح (جوال ثم هاتف) بصيغة دولية بلا + ولا فواصل؛ null إن لم يوجد.</summary>
        public static string FirstValidPhone(string countryCode, params string[] candidates)
        {
            foreach (var c in candidates) { var n = NormalizePhone(c, countryCode); if (n != null) return n; }
            return null;
        }

        public static string NormalizePhone(string raw, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            string digits = new string(raw.Where(char.IsDigit).ToArray());
            // أرقام عربية/هندية (٠-٩) تُحوَّل: char.IsDigit تقبلها وlong.Parse لا يهم هنا لأننا نبنى النص بأنفسنا.
            digits = new string(digits.Select(c => char.IsDigit(c) ? (char)('0' + (int)char.GetNumericValue(c)) : c).ToArray());
            string cc = new string((countryCode ?? "").Where(char.IsDigit).ToArray());
            if (digits.StartsWith("00")) digits = digits.Substring(2);
            else if (digits.StartsWith("0") && cc.Length > 0) digits = cc + digits.Substring(1);
            return digits.Length >= 10 && digits.Length <= 15 ? digits : null;
        }

        // ============================== الاستعلام والإدارة ==============================

        public List<OutboxRow> GetOutbox(int? status = null, int take = 200) => Read(opp => opp.db.Database.SqlQuery<OutboxRow>($@"
SELECT TOP ({take}) o.Id, o.Event_Type, o.Channel, o.Customer_Id, ISNULL(c.Name, N'') AS Customer_Name, ISNULL(o.Phone, N'') AS Phone, o.Message, o.Status, o.Attempts,
       o.Next_Attempt_At, o.Sent_At, o.Insert_ServerDateTime, ISNULL(o.Last_Error, N'') AS Last_Error
FROM SalesRep_Notify_Outbox o LEFT JOIN Customer c ON c.Id = o.Customer_Id
WHERE ({status} IS NULL OR o.Status = {status})
ORDER BY o.Insert_ServerDateTime DESC, o.Id DESC").ToList());

        /// <summary>إعادة محاولة رسالة فاشلة نهائياً: ترجع بالانتظار بعداد محاولات صفرى.</summary>
        public void Retry(long id) => Tx(opp =>
        {
            var o = opp.db.SalesRep_Notify_Outboxes.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("الرسالة غير موجودة.");
            if (o.Status != (int)SalesRepNotifyStatus.Failed) throw new InvalidOperationException("تُعاد المحاولة للرسائل الفاشلة فقط.");
            if (string.IsNullOrEmpty(o.Phone)) throw new InvalidOperationException("لا رقم هاتف للرسالة.");
            o.Status = (int)SalesRepNotifyStatus.Pending; o.Attempts = 0; o.Next_Attempt_At = DateTime.Now; o.Last_Error = null;
            o.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        public void Cancel(long id) => Tx(opp =>
        {
            var o = opp.db.SalesRep_Notify_Outboxes.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("الرسالة غير موجودة.");
            if (o.Status != (int)SalesRepNotifyStatus.Pending && o.Status != (int)SalesRepNotifyStatus.Failed) throw new InvalidOperationException("تُلغى الرسائل المنتظرة أو الفاشلة فقط.");
            o.Status = (int)SalesRepNotifyStatus.Cancelled; o.Next_Attempt_At = null;
            o.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== الإرسال ==============================

        /// <summary>يرسل الرسائل المستحقة. يُستدعى من خدمة الخلفية على السيرفر ومن زر «أرسل الآن»؛ نداء واحد فى كل مرة.</summary>
        public ProcessResult ProcessPending(int batch = 20)
        {
            var result = new ProcessResult();
            lock (SendLock)
            {
                var opp = _external ?? new operations();
                var settings = opp.db.SalesRep_Notify_Settings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefault();
                if (settings == null || settings.Provider == (int)SalesRepNotifyProvider.Off) return result;
                var now = DateTime.Now;
                var ids = opp.db.SalesRep_Notify_Outboxes.AsNoTracking()
                    .Where(o => o.Status == (int)SalesRepNotifyStatus.Pending && (o.Next_Attempt_At == null || o.Next_Attempt_At <= now))
                    .OrderBy(o => o.Id).Take(batch).Select(o => o.Id).ToList();
                foreach (long id in ids)
                {
                    var o = opp.db.SalesRep_Notify_Outboxes.First(x => x.Id == id);
                    string error = null, providerId = null;
                    try
                    {
                        providerId = Send(settings, o.Channel, o.Phone, o.Message);
                    }
                    catch (Exception ee) { error = Short(ee); }
                    o.Attempts++;
                    o.Update_ServerDateTime = DateTime.Now;
                    if (error == null)
                    {
                        o.Status = (int)SalesRepNotifyStatus.Sent; o.Sent_At = DateTime.Now; o.Next_Attempt_At = null; o.Last_Error = null; o.Provider_Message_Id = providerId;
                        result.Sent++;
                    }
                    else if (o.Attempts >= Math.Max(1, settings.Max_Attempts))
                    {
                        o.Status = (int)SalesRepNotifyStatus.Failed; o.Next_Attempt_At = null; o.Last_Error = error;
                        result.Failed++;
                    }
                    else
                    {
                        // تراجع تدريجى: 1 ثم 5 ثم 30 دقيقة ثم ساعتان.
                        int[] minutes = { 1, 5, 30, 120 };
                        o.Next_Attempt_At = DateTime.Now.AddMinutes(minutes[Math.Min(o.Attempts - 1, minutes.Length - 1)]);
                        o.Last_Error = error;
                        result.Retry++;
                    }
                    opp.db.SaveChanges();
                }
            }
            return result;
        }

        /// <summary>رسالة تجريبية مباشرة لرقم (لا تدخل الطابور) للتأكد من إعداد البوابة. ترمى برسالة الخطأ.</summary>
        public void SendTest(string phone, int channel)
        {
            var opp = _external ?? new operations();
            var s = opp.db.SalesRep_Notify_Settings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefault() ?? throw new InvalidOperationException("احفظ إعداد البوابة أولاً.");
            string to = NormalizePhone(phone, s.Default_Country_Code) ?? throw new InvalidOperationException("رقم الهاتف غير صالح.");
            if (!Enum.IsDefined(typeof(SalesRepNotifyChannel), channel)) throw new InvalidOperationException("القناة غير معروفة.");
            if (s.Provider == (int)SalesRepNotifyProvider.Off) throw new InvalidOperationException("الإرسال معطّل.");
            Send(s, channel, to, "رسالة تجريبية من نظام المندوبين.");
        }

        private static string Send(SalesRep_Notify_Setting s, int channel, string phone, string message)
        {
            if (s.Provider == (int)SalesRepNotifyProvider.Test) return "test";
            if (string.IsNullOrWhiteSpace(s.Endpoint_Url)) throw new InvalidOperationException("عنوان البوابة غير مُعدّ.");
            string body = (string.IsNullOrWhiteSpace(s.Body_Template) ? DefaultBodyTemplate : s.Body_Template)
                .Replace("{to}", JsonEscape(phone)).Replace("{channel}", channel == (int)SalesRepNotifyChannel.WhatsApp ? "whatsapp" : "sms")
                .Replace("{message}", JsonEscape(message));
            using var req = new HttpRequestMessage(HttpMethod.Post, s.Endpoint_Url) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
            if (!string.IsNullOrWhiteSpace(s.Auth_Header) && !string.IsNullOrEmpty(s.Auth_Value))
                req.Headers.TryAddWithoutValidation(s.Auth_Header, s.Auth_Value);
            using var resp = Http.SendAsync(req).GetAwaiter().GetResult();
            string text = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"البوابة ردّت {(int)resp.StatusCode}: {Cut(text, 200)}");
            return null;
        }

        public static string JsonEscape(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s ?? "")
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private static string Cut(string s, int n) => string.IsNullOrEmpty(s) ? "" : s.Length <= n ? s : s.Substring(0, n);

        /// <summary>رسالة الخطأ الداخلية مقصوصة؛ لا يُسجَّل فيها عنوان البوابة أو التفويض.</summary>
        private static string Short(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return Cut(ex.Message, 400);
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
}
