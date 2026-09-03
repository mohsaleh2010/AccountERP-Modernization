using System;
using System.Collections.Generic;
using System.Text;

namespace AccountERP.SharedUI
{
    public class AppSettings
    {
        // سيعرف هذا المتغير ما إذا كان التطبيق يعمل داخل ويندوز فورمز أم لا
        public bool IsWinForms { get; set; } = false;
        public bool IsLoadMainLayout { get; set; } = false;

        // 🟢 هل هذا استضافة ويب حقيقية (ASP.NET Core Blazor Server/WASM خلف متصفح) أم تطبيق ديسك توب/موبايل (MAUI Hybrid)؟
        // مهم لتحديد آلية تبديل اللغة الصحيحة: الويب يحتاج تمريرة عبر /Culture/SetCulture (كوكي + Redirect حقيقي)
        // بينما الديسك توب/الموبايل يبدّل اللغة فورياً فى الذاكرة. لا تعتمد على Environment.OSVersion.Platform
        // لتمييز هذا: الكود فى Blazor Server يعمل على السيرفر نفسه، فلو كان السيرفر Windows ستكون النتيجة
        // Win32NT فى الحالتين (ويب وديسك توب) بالتساوي، وهذا هو بالضبط الخطأ الذي كان موجوداً سابقاً.
        public bool IsWeb { get; set; } = false;
    }
}
