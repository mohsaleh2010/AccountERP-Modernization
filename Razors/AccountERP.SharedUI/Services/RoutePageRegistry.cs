using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace AccountERP.SharedUI.Services
{
    // 🟢 يبني جدول (مسار → نوع الصفحة) بالـ Reflection من كل سمات [@page] فى المجمّع، بنفس روح
    // KnownRoutes.cs (لا تسجيل يدوي لكل شاشة جديدة). يُستخدَم فقط من نظام التبويبات (TabWorkspaceService/
    // TabHost.razor) لمعرفة أي "Type" فعلي يجب رسمه عبر <DynamicComponent> لمسار معيّن، ولاستخراج قيم
    // باراميترات المسار (مثل {CustomerId:long}) يدوياً - لأننا نتجاوز <RouteView> القياسي عمداً حتى تبقى
    // كل شاشة مفتوحة كتبويب حيّة فى الذاكرة (KeepPanelsAlive) بدل إعادة إنشائها من الصفر عند كل تبديل تبويب.
    public static class RoutePageRegistry
    {
        private sealed record RouteEntry(string[] Segments, Type PageType, string Template);

        private static readonly Lazy<List<RouteEntry>> _entries = new(Build);

        private static List<RouteEntry> Build()
        {
            var list = new List<RouteEntry>();

            try
            {
                foreach (var type in typeof(SharedUIAssemblyMarker).Assembly.GetTypes())
                {
                    foreach (var route in type.GetCustomAttributes<RouteAttribute>(inherit: true))
                    {
                        if (string.IsNullOrWhiteSpace(route.Template)) continue;

                        var template = route.Template.TrimEnd('/');
                        var segments = template.Trim('/')
                            .Split('/', StringSplitOptions.RemoveEmptyEntries);

                        list.Add(new RouteEntry(segments, type, template));
                    }
                }
            }
            catch
            {
                // غير حرج: لو فشل الفحص لأي سبب، يبقى الجدول فارغاً فتُعامل كل الصفحات كغير قابلة للتبويب بأمان
            }

            return list;
        }

        // relativePath بدون "/" بادئة أو لاحقة ("customers/info/5") - يعيد true فقط لو وُجد @page مطابق تماماً
        // (نفس عدد الأجزاء، والأجزاء الحرفية متطابقة، وقيم أي {باراميتر} قابلة للتحويل لنوعها المطلوب).
        public static bool TryMatch(string relativePath, out Type? pageType, out Dictionary<string, object> routeValues, out string? template)
        {
            pageType = null;
            routeValues = new Dictionary<string, object>();
            template = null;

            var path = (relativePath ?? string.Empty).Split('?')[0].Trim('/');
            var segments = path.Length == 0 ? Array.Empty<string>() : path.Split('/');

            foreach (var entry in _entries.Value)
            {
                if (entry.Segments.Length != segments.Length) continue;

                var values = new Dictionary<string, object>();
                var matched = true;

                for (var i = 0; i < segments.Length; i++)
                {
                    var templateSegment = entry.Segments[i];

                    if (templateSegment.StartsWith("{", StringComparison.Ordinal) && templateSegment.EndsWith("}", StringComparison.Ordinal))
                    {
                        var inner = templateSegment[1..^1].TrimEnd('?');
                        var parts = inner.Split(':');
                        var paramName = parts[0];
                        var constraint = parts.Length > 1 ? parts[1] : null;

                        if (!TryConvert(segments[i], constraint, out var converted))
                        {
                            matched = false;
                            break;
                        }

                        values[paramName] = converted!;
                    }
                    else if (!string.Equals(templateSegment, segments[i], StringComparison.OrdinalIgnoreCase))
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    pageType = entry.PageType;
                    routeValues = values;
                    template = entry.Template;
                    return true;
                }
            }

            return false;
        }

        private static bool TryConvert(string raw, string? constraint, out object? value)
        {
            switch (constraint)
            {
                case "long":
                    if (long.TryParse(raw, out var longValue)) { value = longValue; return true; }
                    break;
                case "int":
                    if (int.TryParse(raw, out var intValue)) { value = intValue; return true; }
                    break;
                case "bool":
                    if (bool.TryParse(raw, out var boolValue)) { value = boolValue; return true; }
                    break;
                case "guid":
                    if (Guid.TryParse(raw, out var guidValue)) { value = guidValue; return true; }
                    break;
                case "double":
                    if (double.TryParse(raw, out var doubleValue)) { value = doubleValue; return true; }
                    break;
                default:
                    value = raw;
                    return true;
            }

            value = null;
            return false;
        }
    }
}
