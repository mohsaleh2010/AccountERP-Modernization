using System;

namespace AccountModelsLib
{
    /// <summary>
    /// تهريب النصوص اللى بتتحط جوه '...' فى جمل SQL المبنية بالدمج: علامة ' بتتحول لـ '' عشان الجملة متتكسرش
    /// (مثلاً ملاحظات فاتورة فيها ' كانت بتفشّل الحفظ كله). نفس سلوك الدمج القديم بالظبط لأى قيمة تانية (null => "").
    /// </summary>
    public static class SqlText
    {
        public static string Q(object value)
        {
            return (Convert.ToString(value) ?? "").Replace("'", "''");
        }
    }
}
