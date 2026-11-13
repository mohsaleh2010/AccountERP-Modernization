using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Components;

namespace AccountERP.SharedUI.Components.Assistant
{
    /// <summary>
    /// تحويل رد المساعد (Markdown) إلى HTML آمن: HTML الخام معطّل (يظهر نصاً)، الروابط http/https والنسبية فقط وتُفتح فى تبويب جديد،
    /// والصور تتحول لروابط نصية فلا يُحمَّل أى عنوان تلقائياً (يمنع تسريب بيانات عبر صورة يكتبها النموذج بعد حقن تعليمات).
    /// </summary>
    public static class AiMarkdown
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .UsePipeTables()
            .UseGridTables()
            .UseEmphasisExtras()
            .UseAutoLinks()
            .UseTaskLists()
            .Build();

        public static MarkupString Render(string? markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return new MarkupString("");
            MarkdownDocument doc = Markdown.Parse(markdown, Pipeline);
            foreach (var link in doc.Descendants<LinkInline>())
            {
                if (link.IsImage) link.IsImage = false;   // الصورة تظهر رابطاً يضغطه المستخدم بنفسه
                if (!IsSafeUrl(link.Url)) link.Url = "#";
                link.GetAttributes().AddPropertyIfNotExist("target", "_blank");
                link.GetAttributes().AddPropertyIfNotExist("rel", "noopener noreferrer nofollow");
            }
            foreach (var auto in doc.Descendants<AutolinkInline>())
                if (!IsSafeUrl(auto.Url)) auto.Url = "#";
            foreach (var table in doc.Descendants<Markdig.Extensions.Tables.Table>())
                table.GetAttributes().AddClass("ai-md-table");
            using var writer = new StringWriter();
            var renderer = new Markdig.Renderers.HtmlRenderer(writer);
            Pipeline.Setup(renderer);
            renderer.Render(doc);
            writer.Flush();
            return new MarkupString(writer.ToString());
        }

        private static bool IsSafeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            url = url.Trim();
            if (url.StartsWith('/') && !url.StartsWith("//")) return true;   // مسار داخل النظام
            if (url.StartsWith('#')) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeHttp);
        }
    }
}
