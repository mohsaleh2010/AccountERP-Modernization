using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountERP.AI.Files;
using AccountModelsLib;

namespace AccountERP.AI.Tools
{
    /// <summary>
    /// P6 (ai-assistant-plan.md §4، §6): extract_document — يقرأ مرفقاً فى المحادثة الحالية ويعيد رأساً وبنوداً بثقة لكل حقل، ويُظهر للمستخدم بطاقة
    /// «بيانات مستخرجة» بالحقول ضعيفة الثقة مميّزة. قراءة فقط: لا يُنشئ صنفاً ولا مستنداً؛ المسودة بعدها بأدوات P5 (والصنف غير المعروف «غير مطابق»).
    /// متاحة فقط إن كان فى المحادثة مرفق، والمرفق يجب أن يخص المستخدم ونفس المحادثة.
    /// </summary>
    public static class AiExtractTools
    {
        public const string Tool = "extract_document";

        public static bool Allowed(AiToolContext c) => c.Attachments.Count > 0;

        public static AiToolDef Def(AiToolContext c)
        {
            string files = string.Join("، ", c.Attachments.Take(20).Select(a => $"{a.Id} = «{a.FileName}»"));
            return new AiToolDef
            {
                Name = Tool,
                Description = "اقرأ ملفاً رفعه المستخدم فى هذه المحادثة (صورة/PDF/Excel/CSV) واستخرج بياناته المنظّمة: الجهة ورقم المستند وتاريخه والإجماليات والبنود، مع ثقة لكل حقل " +
                              "وتحذيرات (حسابات لا تتسق، تكرار). تظهر للمستخدم بطاقة بالبيانات. بعدها: حدّد الجهة والأصناف بـ find_entity، واسأل بـ ask_user عند الغموض أو الحقول ضعيفة الثقة المؤثرة، " +
                              "ثم أعدّ المسودة المناسبة (فاتورة مشتريات لفاتورة مورد). لا تُنشئ أصنافاً: البند غير الموجود يُرسل بـ item_text بلا item_id. " +
                              "الملفات المتاحة: " + files,
                Schema = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["attachment_id"] = new JsonObject { ["type"] = "integer", ["description"] = "معرّف المرفق من القائمة" },
                        ["doc_type"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "purchase_invoice = فاتورة مورد، sales_invoice = طلب/فاتورة عميل، items_list = قائمة أصناف وكميات (إذن مخزن/طلبية)، other = نص عام",
                            ["enum"] = new JsonArray(AiExtractDocTypes.All.Select(v => (JsonNode)JsonValue.Create(v)!).ToArray()),
                        },
                    },
                    ["required"] = new JsonArray("attachment_id", "doc_type"),
                },
            };
        }

        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public static async Task<AiToolResult> RunAsync(JsonObject a, AiToolContext c, AiDocumentExtractor extractor)
        {
            long id = a["attachment_id"] is JsonValue v && (v.TryGetValue<long>(out long l) || (v.TryGetValue<string>(out var s) && long.TryParse(s, out l))) ? l : 0;
            string docType = a["doc_type"] is JsonValue dv && dv.TryGetValue<string>(out var dt) ? dt.Trim() : "";
            if (!AiExtractDocTypes.All.Contains(docType)) throw new ArgumentException("doc_type واحد من: " + string.Join("، ", AiExtractDocTypes.All));
            if (id <= 0 || !c.Attachments.Any(x => x.Id == id))
                throw new ArgumentException("المرفق غير موجود فى هذه المحادثة. المتاحة: " + string.Join("، ", c.Attachments.Select(x => x.Id.ToString(CultureInfo.InvariantCulture))));
            var row = new AiAttachmentsCLS().Get(id, c.User.UserId);
            if (row == null || row.Conversation_Id != c.ConversationId) throw new ArgumentException("المرفق غير موجود فى هذه المحادثة.");

            AiDocumentExtractor.Result res;
            try { res = await extractor.ExtractAsync(row, docType, c.User.UserId, c.Ct); }
            catch (InvalidOperationException ex)
            {
                return new AiToolResult
                {
                    Content = JsonSerializer.Serialize(new { error = ex.Message, hint = "أخبر المستخدم بالسبب باختصار." }, Json),
                    IsError = true, Outcome = AiToolOutcome.Error, Title = "قراءة «" + row.FileName + "»",
                };
            }
            var x = res.Extraction;
            var warnings = new List<string>(x.Warnings);
            // نفس الملف رُفع من قبل (نفس البصمة)
            try
            {
                var same = new AiAttachmentsCLS().FindSameFile(row.Sha256, row.Id, c.User.UserId);
                if (same.Count > 0)
                    warnings.Insert(0, $"رُفع نفس هذا الملف من قبل ({same.Count} مرة، آخرها {same[0].CreatedAt:yyyy-MM-dd}). تأكد أنه لم يُسجَّل بالفعل.");
            }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }

            var data = new AiExtraction { DocType = x.DocType, Currency = x.Currency, Fields = x.Fields, Lines = x.Lines, Warnings = warnings, Text = x.Text };
            object Header() => x.Fields.Where(f => f.Value.Length > 0).ToDictionary(f => f.Key, f => (object)new { value = f.Value, confidence = f.Confidence, needs_review = f.Low });
            string content = JsonSerializer.Serialize(new
            {
                attachment_id = row.Id,
                file = row.FileName,
                doc_type = x.DocType,
                header = Header(),
                lines = x.Lines.Select(l => new
                {
                    no = l.No, description = l.Description, code = l.Code, qty = l.Qty, unit = l.Unit, unit_price = l.UnitPrice,
                    discount = l.Discount, tax = l.Tax, line_total = l.LineTotal, confidence = l.Confidence, needs_review = l.LowFields,
                }),
                text = x.Text.Length > 0 ? x.Text : null,
                warnings,
                shown = "ظهرت للمستخدم بطاقة بالبيانات المستخرجة (الحقول ضعيفة الثقة مميّزة)؛ لا تكرر الجدول فى ردك.",
                next = x.DocType == AiExtractDocTypes.PurchaseInvoice
                    ? "ابحث عن المورد (party_name/party_tax_id) والأصناف بـ find_entity، ثم create_purchase_invoice_draft بالأسعار المقروءة وsupplier_invoice_no=doc_no وsource_attachment_id. اسأل عن المخزن إن لم يُعرف."
                    : x.DocType == AiExtractDocTypes.SalesInvoice ? "ابحث عن العميل والأصناف بـ find_entity ثم create_sales_invoice_draft (السعر من المستند فقط إن طلب المستخدم ذلك)."
                    : x.DocType == AiExtractDocTypes.ItemsList ? "ابحث عن الأصناف بـ find_entity ثم أعدّ المسودة التى يطلبها المستخدم (إذن مخزن أو فاتورة)."
                    : "لخّص أو أجب المستخدم من النص.",
            }, Json);
            if (content.Length > 60000) content = content[..60000];
            return new AiToolResult
            {
                Content = content,
                RowCount = x.Lines.Count,
                Title = "قراءة «" + row.FileName + "»" + (res.Cached ? " (محفوظة)" : ""),
                Block = new JsonObject
                {
                    ["type"] = "extraction",
                    ["attachment_id"] = row.Id.ToString(CultureInfo.InvariantCulture),
                    ["file_name"] = row.FileName,
                    ["data"] = JsonSerializer.SerializeToNode(data, AiDocumentExtractor.StoreJson),
                },
            };
        }
    }
}
