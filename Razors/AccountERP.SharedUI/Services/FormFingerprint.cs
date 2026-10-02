using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AccountERP.SharedUI.Services;

/// <summary>
/// بصمة نصية لقيم نموذج الإدخال (كل الخصائص العددية/النصية/التاريخية القابلة للقراءة، بلا خصائص التنقل) - تُستخدم لقاعدة
/// «زر الحفظ يُفعَّل عند وجود تغيير فقط ويتعطل بعد الحفظ حتى تغيير جديد»: تُؤخذ البصمة عند تحميل/حفظ السجل ثم تُقارَن بالحالية.
/// </summary>
public static class FormFingerprint
{
    public static string Of(params object?[] parts)
    {
        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            Append(sb, part);
            sb.Append('|');
        }
        return sb.ToString();
    }

    private static void Append(StringBuilder sb, object? value)
    {
        if (value is null) { sb.Append('~'); return; }
        var type = value.GetType();
        if (IsScalar(type))
        {
            sb.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            return;
        }
        if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable) { Append(sb, item); sb.Append(','); }
            return;
        }
        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && IsScalar(p.PropertyType))
                     .OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            sb.Append(p.Name).Append('=');
            var v = p.GetValue(value);
            sb.Append(v is null ? "~" : Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)).Append(';');
        }
    }

    private static bool IsScalar(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid) || t == typeof(TimeSpan);
    }
}
