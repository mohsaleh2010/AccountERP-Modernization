using System.Globalization;
using System.Security.Cryptography;
using AccountModelsLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AccountERP.AI.Files
{
    /// <summary>
    /// P6 (ai-assistant-plan.md §2.5، P6): تخزين مرفقات المساعد على القرص. النوع يُحدَّد من محتوى الملف (التوقيع) لا من الامتداد أو نوع المتصفح وحده،
    /// والحجم بحد AI_Settings.MaxUploadMb (الافتراضى 10MB، الصور حتى 5MB لأن نماذج الرؤية ترفض الأكبر). المسار يُبنى من المعرّف فقط (لا اسم المستخدم)
    /// ويُتحقق أنه داخل مجلد المرفقات. المجلد: AI:AttachmentsPath فى appsettings وإلا App_Data/ai-attachments تحت مجلد التطبيق.
    /// </summary>
    public sealed class AiAttachmentStore
    {
        public const int DefaultMaxMb = 10, MaxImageMb = 5, MaxFilesPerMessage = 5;
        public const string Accept = ".png,.jpg,.jpeg,.webp,.pdf,.xlsx,.csv";

        public const string MimePng = "image/png", MimeJpeg = "image/jpeg", MimeWebp = "image/webp", MimePdf = "application/pdf",
                            MimeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", MimeCsv = "text/csv";

        private readonly string _root;
        private readonly AiAttachmentsCLS _dal = new();
        private readonly AiProvidersCLS _providers = new();
        private static DateTime _lastSweep = DateTime.MinValue;
        private static readonly object SweepLock = new();

        public AiAttachmentStore(IConfiguration config, IHostEnvironment env)
            : this(string.IsNullOrWhiteSpace(config["AI:AttachmentsPath"]) ? Path.Combine(env.ContentRootPath, "App_Data", "ai-attachments") : config["AI:AttachmentsPath"]!) { }

        public AiAttachmentStore(string root)
        {
            _root = Path.GetFullPath(root);
        }

        public string Root => _root;

        public static string KindOf(string mime) => mime switch
        {
            MimePdf => "pdf",
            MimeXlsx or MimeCsv => "sheet",
            _ => "image",
        };

        public static bool IsMedia(string mime) => mime is MimePng or MimeJpeg or MimeWebp or MimePdf;

        public static AiAttachmentView View(AiAttachmentRow r) => new()
        {
            Id = r.Id, FileName = r.FileName, MimeType = r.MimeType, SizeBytes = r.SizeBytes, Kind = KindOf(r.MimeType),
            Expired = r.Status == AiAttachmentStatus.Expired,
        };

        public int MaxMb()
        {
            try
            {
                var s = _providers.GetSettings();
                if (s.TryGetValue(AiSettingKeys.MaxUploadMb, out var v) && int.TryParse(v, out int n) && n is >= 1 and <= 50) return n;
            }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
            return DefaultMaxMb;
        }

        private int RetentionDays()
        {
            try
            {
                var s = _providers.GetSettings();
                if (s.TryGetValue(AiSettingKeys.AttachmentRetentionDays, out var v) && int.TryParse(v, out int n) && n >= 1) return n;
            }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
            return 90;
        }

        /// <summary>النوع الحقيقى من أول بايتات الملف (والامتداد لـ xlsx/csv فقط لأنهما zip/نص). null = غير مسموح.</summary>
        public static string? Sniff(byte[] d, string fileName)
        {
            string ext = Path.GetExtension(fileName ?? "").ToLowerInvariant();
            bool Starts(params byte[] sig) => d.Length >= sig.Length && sig.Select((b, i) => d[i] == b).All(x => x);
            if (Starts(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)) return MimePng;
            if (Starts(0xFF, 0xD8, 0xFF)) return MimeJpeg;
            if (d.Length >= 12 && Starts(0x52, 0x49, 0x46, 0x46) && d[8] == 0x57 && d[9] == 0x45 && d[10] == 0x42 && d[11] == 0x50) return MimeWebp;
            if (Starts(0x25, 0x50, 0x44, 0x46, 0x2D)) return MimePdf;
            if (Starts(0x50, 0x4B, 0x03, 0x04) && ext == ".xlsx") return MimeXlsx;
            if (ext == ".csv" && d.Length > 0 && !d.Take(8192).Contains((byte)0)) return MimeCsv;
            return null;
        }

        /// <summary>يقرأ الملف بحد الحجم ويتحقق ويحفظ (قرص + صف AI_Attachments). يرمى InvalidOperationException برسالة عربية.</summary>
        public async Task<AiAttachmentRow> SaveAsync(long userId, string fileName, Stream content, CancellationToken ct)
        {
            if (userId <= 0) throw new InvalidOperationException("سجّل الدخول أولاً.");
            string name = CleanName(fileName);
            int maxMb = MaxMb();
            long max = maxMb * 1024L * 1024L;
            using var ms = new MemoryStream();
            var buf = new byte[81920];
            int n;
            while ((n = await content.ReadAsync(buf, ct)) > 0)
            {
                ms.Write(buf, 0, n);
                if (ms.Length > max) throw new InvalidOperationException($"الملف «{name}» أكبر من الحد المسموح ({maxMb}MB).");
            }
            byte[] data = ms.ToArray();
            if (data.Length == 0) throw new InvalidOperationException($"الملف «{name}» فارغ.");
            string mime = Sniff(data, name)
                ?? throw new InvalidOperationException($"نوع الملف «{name}» غير مسموح. المسموح: صور (PNG/JPG/WebP)، PDF، Excel (xlsx)، CSV.");
            if (KindOf(mime) == "image" && data.Length > MaxImageMb * 1024L * 1024L)
                throw new InvalidOperationException($"الصورة «{name}» أكبر من {MaxImageMb}MB (حد نماذج الرؤية). صغّرها أو صوّرها بدقة أقل.");

            long id = CallSnowflakeIdGenerate.GenerateNewId();
            DateTime now = DateTime.Now;
            string rel = Path.Combine(now.ToString("yyyy", CultureInfo.InvariantCulture), now.ToString("MM", CultureInfo.InvariantCulture),
                                      id.ToString(CultureInfo.InvariantCulture) + Ext(mime));
            string full = FullPath(rel);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            await File.WriteAllBytesAsync(full, data, ct);
            var row = new AiAttachmentRow
            {
                Id = id, User_Id = userId, FileName = name, MimeType = mime, SizeBytes = data.Length,
                Sha256 = Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant(), StoragePath = rel.Replace('\\', '/'),
            };
            try { _dal.Insert(row); }
            catch { TryDelete(rel); throw; }
            Sweep();
            return row;
        }

        public byte[]? Read(AiAttachmentRow row)
        {
            if (row.Status != AiAttachmentStatus.Active || string.IsNullOrWhiteSpace(row.StoragePath)) return null;
            string full = FullPath(row.StoragePath);
            return File.Exists(full) ? File.ReadAllBytes(full) : null;
        }

        public void TryDelete(string? rel)
        {
            if (string.IsNullOrWhiteSpace(rel)) return;
            try
            {
                string full = FullPath(rel);
                if (File.Exists(full)) File.Delete(full);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException) { operations.DataDictionaryLogerException(ex); }
        }

        /// <summary>مع حذف المحادثة: ملفاتها تُحذف من القرص وصفوفها تُعلَّم Expired (تبقى للتدقيق).</summary>
        public void ExpireConversation(long conversationId, long userId)
        {
            var list = _dal.ExpiryCandidates(0, conversationId, userId, 1000);
            foreach (var (_, path) in list) TryDelete(path);
            _dal.MarkExpired(list.Select(x => x.Id));
        }

        /// <summary>مدة الاحتفاظ (AttachmentRetentionDays): تُنفَّذ مع الرفع، مرة كل 6 ساعات على الأكثر.</summary>
        private void Sweep()
        {
            lock (SweepLock)
            {
                if (DateTime.UtcNow - _lastSweep < TimeSpan.FromHours(6)) return;
                _lastSweep = DateTime.UtcNow;
            }
            try
            {
                var list = _dal.ExpiryCandidates(RetentionDays());
                foreach (var (_, path) in list) TryDelete(path);
                _dal.MarkExpired(list.Select(x => x.Id));
            }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
        }

        private string FullPath(string rel)
        {
            string full = Path.GetFullPath(Path.Combine(_root, rel.Replace('/', Path.DirectorySeparatorChar)));
            if (!full.StartsWith(_root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("مسار مرفق غير صالح.");
            return full;
        }

        private static string Ext(string mime) => mime switch
        {
            MimePng => ".png", MimeJpeg => ".jpg", MimeWebp => ".webp", MimePdf => ".pdf", MimeXlsx => ".xlsx", MimeCsv => ".csv", _ => ".bin",
        };

        /// <summary>اسم للعرض فقط (بلا مسار ولا محارف تحكم)؛ لا يُستخدم فى مسار الحفظ.</summary>
        public static string CleanName(string? fileName)
        {
            string s = Path.GetFileName((fileName ?? "").Replace('\\', '/').Split('/').Last());
            s = new string(s.Where(ch => !char.IsControl(ch)).ToArray()).Trim();
            if (s.Length == 0) s = "file";
            return s.Length > 200 ? s[..200] : s;
        }
    }
}
