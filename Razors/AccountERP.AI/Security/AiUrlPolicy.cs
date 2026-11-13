using System.Net;
using System.Net.Sockets;

namespace AccountERP.AI.Security
{
    /// <summary>
    /// قواعد عنوان المزوّد (خطة §7.4): https إجبارى للعناوين الخارجية، وhttp مسموح للشبكة المحلية فقط (localhost، عناوين خاصة، .local/.lan).
    /// لا DNS هنا: اسم نطاق عادى يُعامل كخارجى.
    /// </summary>
    public static class AiUrlPolicy
    {
        public static bool IsLocalHost(Uri uri)
        {
            string host = uri.IdnHost.Trim('[', ']');
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".lan", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase)
                || !host.Contains('.') && !IPAddress.TryParse(host, out _))
                return true;
            if (!IPAddress.TryParse(host, out var ip)) return false;
            if (IPAddress.IsLoopback(ip)) return true;
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6UniqueLocal;
            byte[] b = ip.GetAddressBytes();
            return b[0] == 10
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                || (b[0] == 192 && b[1] == 168)
                || (b[0] == 169 && b[1] == 254)
                || (b[0] == 100 && b[1] >= 64 && b[1] <= 127); // CGNAT / Tailscale
        }

        /// <summary>يتحقق من العنوان ويعيده بلا شرطة أخيرة. يرمى رسالة عربية عند الرفض.</summary>
        public static Uri Validate(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                throw new InvalidOperationException("عنوان الـ URL غير صحيح. مثال: https://api.example.com/v1 أو http://192.168.1.50:8000/v1");
            if (!string.IsNullOrEmpty(uri.UserInfo))
                throw new InvalidOperationException("لا تضع اسم مستخدم أو كلمة مرور داخل العنوان؛ استخدم حقل المفتاح.");
            if (uri.Scheme == Uri.UriSchemeHttp && !IsLocalHost(uri))
                throw new InvalidOperationException("العناوين الخارجية تتطلب https؛ http مسموح للشبكة المحلية فقط.");
            return new Uri(uri.GetLeftPart(UriPartial.Path).TrimEnd('/'));
        }
    }
}
