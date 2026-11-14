using System;
using System.Collections.Generic;
using System.Linq;
using MudBlazor;

namespace AccountERP.SharedUI.Services
{
    public class NotificationRecord
    {
        public required string Message { get; init; }
        public Severity Severity { get; init; }
        public DateTime TimestampUtc { get; init; }
        public bool IsRead { get; set; }
        /// <summary>P8.1.5: مسار داخلى (يبدأ بـ / ولا يبدأ بـ //) يفتحه الضغط على الإشعار؛ null = نص فقط.</summary>
        public string? Href { get; init; }
        /// <summary>P8.1.5: مفتاح منع التكرار (مثلاً brief:123)؛ إشعار بنفس المفتاح لا يُضاف مرتين.</summary>
        public string? Key { get; init; }
    }

    // خدمة بسيطة لحفظ آخر الإشعارات التى ظهرت كـ Snackbar (مثل تنبيه "الشاشة قيد الإنشاء") فى الجلسة
    // الحالية، حتى تبقى مرئية فى قائمة جرس الإشعارات (NotificationsMenu) بعد اختفاء التنبيه المؤقت نفسه
    // من الشاشة - بدون هذه الخدمة تظل القائمة فارغة دائماً رغم ظهور تنبيهات فعلية للمستخدم. مسجّلة كـ Scoped
    // (نسخة مستقلة لكل مستخدم على الويب، ونسخة واحدة لكل جلسة تطبيق على الديسك توب/الموبايل)، بنفس نمط
    // ThemeService/LanguageService.
    //
    // 🟢 IsRead منفصل عن مجرد "عرضها" فى القائمة: العناصر تبقى ظاهرة دائماً (حتى سقف MaxItems)، لكن شارة
    // العدد غير المقروء (UnreadCount) تُصفَّر فقط عند فتح القائمة فعلياً (MarkAllAsRead من NotificationsMenu)
    // - نفس سلوك أجراس الإشعارات المعتادة (تبقى فى السجل بعد قراءتها، لكن لا تُحتسب كـ "غير مقروءة" بعد ذلك).
    public class NotificationCenterService
    {
        private const int MaxItems = 20;
        private readonly List<NotificationRecord> _items = new();

        public IReadOnlyList<NotificationRecord> Items => _items;

        public int UnreadCount => _items.Count(i => !i.IsRead);

        public event Action? OnChanged;

        public void Add(string message, Severity severity = Severity.Info, string? href = null, string? key = null)
        {
            if (key != null && _items.Any(i => i.Key == key)) return;
            string? link = href is { Length: > 0 } h && h.StartsWith('/') && !h.StartsWith("//") ? h : null;
            _items.Insert(0, new NotificationRecord { Message = message, Severity = severity, TimestampUtc = DateTime.UtcNow, Href = link, Key = key });
            if (_items.Count > MaxItems) _items.RemoveAt(_items.Count - 1);
            OnChanged?.Invoke();
        }

        public void MarkAllAsRead()
        {
            if (UnreadCount == 0) return;
            foreach (var item in _items) item.IsRead = true;
            OnChanged?.Invoke();
        }

        public void Remove(NotificationRecord item)
        {
            if (_items.Remove(item)) OnChanged?.Invoke();
        }
    }
}
