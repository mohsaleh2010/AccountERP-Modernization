namespace AccountModelsLib.Models
{
    // 🟢 قيم مشتركة بين نقطة ضبط الكوكي ("/Auth/Persist" فى AccountERP.Web\Program.cs) وقارئه
    // (MainLayoutMud.razor فى AccountERP.SharedUI) - فى ملف واحد حتى لا تتفرق القيمتان عن بعضهما.
    public static class AuthCookieConstants
    {
        public const string CookieName = "AccountERP.Auth";
        public const string ProtectorPurpose = "AccountERP.AuthCookie.v1";
    }
}
