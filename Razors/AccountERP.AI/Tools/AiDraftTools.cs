using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountERP.AI.Chat;
using AccountModelsLib;

namespace AccountERP.AI.Tools
{
    /// <summary>
    /// P5 (ai-assistant-plan.md §6): أدوات إعداد المسودات — فاتورة مبيعات، فاتورة مشتريات، إذن مخزن. الأداة لا تكتب أى شىء فى جداول المستندات
    /// ولا تمس الرصيد: تتحقق من العميل/المورد/المخزن/الأصناف وتحفظ صفاً فى AI_Drafts فقط، ثم تُظهر بطاقة «مسودة جاهزة» بزر يفتح الشاشة الحقيقية
    /// (?aiDraft=) فتعبّئ نفسها بمنطقها (الأسعار والضريبة)، والمستخدم يراجع ويحفظ بالزر العادى (المخزون والقيود بالمنطق القائم).
    /// الصلاحية = شاشة المستند نفسها فى قائمة المستخدم. الصنف غير المطابق لا يُنشأ أبداً؛ يُعرض «غير مطابق» ليختاره المستخدم.
    /// </summary>
    public static class AiDraftTools
    {
        public const string SalesTool = "create_sales_invoice_draft";
        public const string PurchaseTool = "create_purchase_invoice_draft";
        public const string StoreOrderTool = "create_store_order_draft";

        public const string RSalesInvoice = "/accounts/invoices/sales-invoice";
        public const string RPurchaseInvoice = "/accounts/invoices/purchase-invoice";
        public const string RStoreOrderPrefix = "/store/order/";

        public const int MaxLines = 200;
        private const int MaxNotes = 500;

        /// <summary>أنواع أذون الاستلام من التحويل تُنشأ من شاشة «استلام التحويل» فقط (كالشاشة).</summary>
        private static readonly int[] ReceiptTypes = { 1016, 1017 };

        // P7.4: لا مسودات أثناء التقييم (لا تُعرض الأدوات ولا تُنفَّذ ولو طلبها النموذج).
        public static bool CanSales(AiToolContext c) => !c.IsEval && c.HasRoute(RSalesInvoice);
        public static bool CanPurchase(AiToolContext c) => !c.IsEval && c.HasRoute(RPurchaseInvoice);
        public static bool CanStoreOrder(AiToolContext c) => !c.IsEval && StoreOrderTypes(c).Count > 0;
        public static bool Any(AiToolContext c) => CanSales(c) || CanPurchase(c) || CanStoreOrder(c);

        /// <summary>Views يحتاجها find_entity لمن يملك شاشة مستند فقط (بلا شاشة القوائم): العميل للمبيعات، المورد للمشتريات، الصنف والمخزن لأى مسودة.</summary>
        public static IEnumerable<string> ImpliedViews(AiToolContext c)
        {
            bool s = CanSales(c), p = CanPurchase(c), o = CanStoreOrder(c);
            if (s) yield return "v_customers";
            if (p) yield return "v_suppliers";
            if (s || p || o) { yield return "v_items"; yield return "v_stores"; }
        }

        // ───────────── أنواع أذون المخزن ─────────────

        public sealed record OrderTypeInfo(int Type, string Name, int Direction, bool StoreIsFrom, int[] OtherParties, bool ManualPrice, bool IsTransfer);

        private static List<OrderTypeInfo>? _typesCache;
        private static DateTime _typesAt;
        private static readonly object TypesLock = new();

        private static List<OrderTypeInfo> AllOrderTypes()
        {
            lock (TypesLock)
            {
                if (_typesCache != null && DateTime.UtcNow - _typesAt < TimeSpan.FromMinutes(10)) return _typesCache;
            }
            var dt = new operations().return_DataTable(
                "SELECT Order_Type, Name_Ar, Direction, Store_Side, From_Party_Types, To_Party_Types, Price_Source, Is_Transfer FROM Store_Order_Type_Settings WHERE Is_Active = 1 ORDER BY Order_Type");
            static int[] Parse(object o) => (Convert.ToString(o) ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                                      .Select(x => int.TryParse(x, out int n) ? n : 0).Where(n => n > 0).ToArray();
            var list = new List<OrderTypeInfo>();
            foreach (System.Data.DataRow r in dt.Rows)
            {
                int type = Convert.ToInt32(r["Order_Type"]);
                if (ReceiptTypes.Contains(type)) continue;
                bool storeIsFrom = Convert.ToInt32(r["Store_Side"]) == 1;
                list.Add(new OrderTypeInfo(type, (Convert.ToString(r["Name_Ar"]) ?? "").Trim(), Convert.ToInt32(r["Direction"]), storeIsFrom,
                    Parse(storeIsFrom ? r["To_Party_Types"] : r["From_Party_Types"]), Convert.ToInt32(r["Price_Source"]) == 3,
                    r["Is_Transfer"] != DBNull.Value && Convert.ToBoolean(r["Is_Transfer"])));
            }
            lock (TypesLock) { _typesCache = list; _typesAt = DateTime.UtcNow; }
            return list;
        }

        /// <summary>أنواع الأذون التى شاشتها (/store/order/{type}) فى قائمة المستخدم.</summary>
        public static List<OrderTypeInfo> StoreOrderTypes(AiToolContext c)
        {
            if (c.CachedOrderTypes != null) return c.CachedOrderTypes;
            List<OrderTypeInfo> list;
            try { list = AllOrderTypes().Where(t => c.HasRoute(RStoreOrderPrefix + t.Type.ToString(CultureInfo.InvariantCulture))).ToList(); }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); list = new(); }
            c.CachedOrderTypes = list;
            return list;
        }

        // ───────────── التعريفات ─────────────

        private static JsonObject Str(string d) => new() { ["type"] = "string", ["description"] = d };
        private static JsonObject Num(string d) => new() { ["type"] = "number", ["description"] = d };
        private static JsonObject Int(string d) => new() { ["type"] = "integer", ["description"] = d };
        private static JsonObject Bool(string d) => new() { ["type"] = "boolean", ["description"] = d };

        private static JsonObject LinesSchema(bool withPrice, string priceDesc) => new()
        {
            ["type"] = "array",
            ["description"] = "بنود المستند (حتى " + MaxLines + ")",
            ["items"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["item_id"] = Int("معرّف الصنف من find_entity (type=item). اتركه فارغاً إن لم تجد الصنف"),
                    ["item_text"] = Str("اسم الصنف كما كتبه المستخدم (يُعرض «غير مطابق» إن لم يوجد item_id)"),
                    ["qty_large"] = Num("الكمية بالوحدة الكبرى"),
                    ["qty_small"] = Num("الكمية بالوحدة الصغرى (اختيارى)"),
                    ["price"] = withPrice ? Num(priceDesc) : Num("تكلفة الوحدة الكبرى (لأنواع الأذون ذات التكلفة اليدوية فقط)"),
                    ["notes"] = Str("ملاحظة على البند (اختيارى)"),
                },
            },
        };

        private static JsonObject Obj(JsonObject props, params string[] required) =>
            new() { ["type"] = "object", ["properties"] = props, ["required"] = new JsonArray(required.Select(r => (JsonNode)JsonValue.Create(r)!).ToArray()) };

        private const string CommonRules = " لا يُحفظ أى مستند ولا يتحرك المخزون: تظهر للمستخدم بطاقة «مسودة جاهزة» يفتحها فى الشاشة ويراجعها ويحفظ بنفسه. " +
            "قبلها حدّد كل كيان بمعرّفه عبر find_entity، وإن تعددت المطابقات أو لم يُذكر المخزن فاسأل بـ ask_user. لا تخترع أصنافاً.";

        public static AiToolDef SalesDef() => new()
        {
            Name = SalesTool,
            Description = "أعدّ مسودة فاتورة مبيعات (عميل + مخزن + أصناف وكمياتها)." + CommonRules +
                          " السعر اختيارى: إن لم يذكره المستخدم اتركه فتطبّق الشاشة سعر العميل أو سعر الصنف.",
            Schema = Obj(new JsonObject
            {
                ["customer_id"] = Int("معرّف العميل من find_entity"),
                ["store_id"] = Int("معرّف المخزن من find_entity (type=store)"),
                ["lines"] = LinesSchema(true, "سعر الوحدة الكبرى إن ذكره المستخدم"),
                ["credit"] = Bool("فاتورة آجلة (الافتراضى نقدى)"),
                ["tax_invoice"] = Bool("فاتورة ضريبية؛ اتركه فارغاً لإعداد الشاشة الافتراضى"),
                ["invoice_date"] = Str("yyyy-MM-dd (الافتراضى اليوم)"),
                ["notes"] = Str("ملاحظات الفاتورة"),
            }, "customer_id", "store_id", "lines"),
        };

        public static AiToolDef PurchaseDef() => new()
        {
            Name = PurchaseTool,
            Description = "أعدّ مسودة فاتورة مشتريات (مورد + مخزن الاستلام + أصناف وكمياتها وأسعارها)." + CommonRules +
                          " سعر الشراء اختيارى: إن لم يُذكر تضع الشاشة آخر سعر شراء للصنف. الفاتورة الضريبية بالمشتريات ضريبتها على الصنف.",
            Schema = Obj(new JsonObject
            {
                ["supplier_id"] = Int("معرّف المورد من find_entity"),
                ["store_id"] = Int("معرّف المخزن من find_entity (type=store)"),
                ["lines"] = LinesSchema(true, "سعر شراء الوحدة الكبرى إن ذُكر"),
                ["credit"] = Bool("فاتورة آجلة (الافتراضى نقدى)"),
                ["tax_invoice"] = Bool("فاتورة ضريبية (ضريبة على الصنف)؛ اتركه فارغاً لإعداد الشاشة الافتراضى"),
                ["invoice_date"] = Str("yyyy-MM-dd (الافتراضى اليوم؛ من مستند مرفق: تاريخ فاتورة المورد)"),
                ["notes"] = Str("ملاحظات الفاتورة"),
                ["supplier_invoice_no"] = Str("رقم فاتورة المورد كما فى مستنده (من extract_document: doc_no) — يُكشف به التكرار ويُكتب فى الملاحظات"),
                ["source_attachment_id"] = Int("معرّف المرفق الذى قُرئت منه الفاتورة (اختيارى)"),
                ["doc_total"] = Num("الإجمالى النهائى فى مستند المورد (اختيارى؛ للمقارنة وكشف التكرار)"),
            }, "supplier_id", "store_id", "lines"),
        };

        public static AiToolDef StoreOrderDef(AiToolContext c)
        {
            var types = StoreOrderTypes(c);
            string list = string.Join("، ", types.Select(t => $"{t.Type} = {t.Name} ({(t.Direction == 1 ? "وارد" : t.Direction == 3 ? "تحويل" : "منصرف")})"));
            return new()
            {
                Name = StoreOrderTool,
                Description = "أعدّ مسودة إذن مخزن (إذن إضافة/استلام أو صرف أو تحويل) لمخزن وأصناف وكميات." + CommonRules +
                              " الأنواع المسموحة لهذا المستخدم: " + list + ". الجهة الأخرى (مخزن التحويل، العميل، المورد...) اختيارية؛ إن تُركت يختارها المستخدم فى الشاشة.",
                Schema = Obj(new JsonObject
                {
                    ["order_type"] = Int("نوع الإذن من القائمة أعلاه"),
                    ["store_id"] = Int("معرّف المخزن (جهة المخزون فى الإذن)"),
                    ["other_party_type"] = Int("نوع الجهة الأخرى: 1 مورد، 2 عميل، 4 مخزن، 20 قسم (اختيارى)"),
                    ["other_party_id"] = Int("معرّف الجهة الأخرى (اختيارى)"),
                    ["lines"] = LinesSchema(false, ""),
                    ["order_date"] = Str("yyyy-MM-dd (الافتراضى اليوم)"),
                    ["notes"] = Str("ملاحظات الإذن"),
                }, "order_type", "store_id", "lines"),
            };
        }

        // ───────────── التنفيذ ─────────────

        public static AiToolResult CreateSales(JsonObject a, AiToolContext c) => CreateInvoice(a, c, AiDraftKind.Sales);
        public static AiToolResult CreatePurchase(JsonObject a, AiToolContext c) => CreateInvoice(a, c, AiDraftKind.Purchase);

        private static AiToolResult CreateInvoice(JsonObject a, AiToolContext c, string kind)
        {
            var dal = new AiDraftsCLS();
            bool sales = kind == AiDraftKind.Sales;
            string partyKey = sales ? "customer_id" : "supplier_id";
            long partyId = Long(a, partyKey);
            if (partyId <= 0) throw new ArgumentException($"حدّد {(sales ? "العميل" : "المورد")} بمعرّفه ({partyKey}) من find_entity.");
            string? partyName = dal.ActiveName(sales ? "customer" : "supplier", partyId);
            if (partyName == null) throw new ArgumentException($"{(sales ? "العميل" : "المورد")} {partyId} غير موجود أو غير نشط. ابحث عنه بـ find_entity.");
            var (storeId, storeName) = Store(a, dal);

            var p = new AiDraftPayload
            {
                Kind = kind,
                PartyId = partyId,
                PartyName = partyName,
                StoreId = storeId,
                StoreName = storeName,
                Credit = Bool(a, "credit") == true,
                TaxInvoice = Bool(a, "tax_invoice"),
                Date = Date(a, "invoice_date"),
                Notes = Notes(a),
            };
            if (!sales) SourceAndDuplicates(a, p, c, dal);   // P6
            ReadLines(a, p, dal, checkBalance: sales, mergeDuplicates: false, allowPrice: true);
            string kindAr = sales ? "فاتورة مبيعات" : "فاتورة مشتريات";
            string title = $"مسودة {kindAr} — {partyName}";
            string url = (sales ? RSalesInvoice : RPurchaseInvoice) + "?aiDraft={id}" + (p.Credit ? "&agile=1" : "");
            return Save(c, dal, p, title, url, $"{partyName} · {storeName}" + (string.IsNullOrEmpty(p.RefNo) ? "" : " · رقم المورد " + p.RefNo));
        }

        public static AiToolResult CreateStoreOrder(JsonObject a, AiToolContext c)
        {
            var dal = new AiDraftsCLS();
            int type = (int)Long(a, "order_type");
            var info = StoreOrderTypes(c).FirstOrDefault(t => t.Type == type);
            if (info == null)
                return new AiToolResult
                {
                    Content = JsonSerializer.Serialize(new { error = $"نوع الإذن {type} غير مسموح لك أو غير موجود. الأنواع المسموحة: " + string.Join("، ", StoreOrderTypes(c).Select(t => t.Type + " " + t.Name)) }, Json),
                    IsError = true, Outcome = AiToolOutcome.Denied, Title = "مسودة إذن مخزن",
                };
            var (storeId, storeName) = Store(a, dal);
            var p = new AiDraftPayload
            {
                Kind = AiDraftKind.StoreOrder,
                OrderType = type,
                OrderTypeName = info.Name,
                StoreId = storeId,
                StoreName = storeName,
                Date = Date(a, "order_date"),
                Notes = Notes(a),
            };
            int otherType = (int)Long(a, "other_party_type");
            long otherId = Long(a, "other_party_id");
            if (otherType > 0 || otherId > 0)
            {
                if (otherType <= 0 || !info.OtherParties.Contains(otherType))
                    p.Warnings.Add("نوع الجهة الأخرى غير متاح لهذا الإذن؛ اخترها فى الشاشة.");
                else if (info.IsTransfer && otherType == 4 && otherId == storeId)
                    p.Warnings.Add("لا يكون التحويل لنفس المخزن؛ اختر المخزن الآخر فى الشاشة.");
                else { p.OtherPartyType = otherType; p.OtherPartyId = otherId; }
            }
            if (info.IsTransfer && p.OtherPartyId <= 0 && !p.Warnings.Any(w => w.Contains("التحويل")))
                p.Warnings.Add("حدّد المخزن المستلم فى الشاشة.");
            ReadLines(a, p, dal, checkBalance: info.Direction != 1, mergeDuplicates: true, allowPrice: info.ManualPrice);
            if (info.ManualPrice && p.Lines.Any(l => l.Price is null or <= 0))
                p.Warnings.Add("هذا الإذن يتطلب تكلفة لكل صنف؛ الأصناف بلا تكلفة تأخذ متوسط التكلفة ويمكن تعديلها فى الشاشة.");
            string title = $"مسودة {info.Name} — {storeName}";
            string url = RStoreOrderPrefix + type.ToString(CultureInfo.InvariantCulture) + "?aiDraft={id}";
            return Save(c, dal, p, title, url, info.Name);
        }

        /// <summary>
        /// P6: رقم فاتورة المورد ومصدرها: يُكتب الرقم فى الملاحظات (SupplierRefPrefix) فيبقى مع الفاتورة بعد الحفظ، ويُفحص التكرار
        /// (مسودات سابقة بنفس المورد والرقم، فاتورة مشتريات محفوظة تحمل الرقم، أو نفس اليوم ونفس الصافى). تحذير لا منع: القرار للمستخدم.
        /// </summary>
        private static void SourceAndDuplicates(JsonObject a, AiDraftPayload p, AiToolContext c, AiDraftsCLS dal)
        {
            string refNo = Text(a, "supplier_invoice_no", 60);
            refNo = Files.AiExtractionNormalizer.LatinDigits(refNo).Trim();
            if (refNo.Length > 0)
            {
                p.RefNo = refNo;
                string tag = AiDraftPayload.SupplierRefPrefix + refNo;
                p.Notes = string.IsNullOrWhiteSpace(p.Notes) ? tag : (p.Notes!.Contains(tag) ? p.Notes : tag + " — " + p.Notes);
                if (p.Notes!.Length > MaxNotes) p.Notes = p.Notes[..MaxNotes];
            }
            long att = Long(a, "source_attachment_id");
            if (att > 0)
            {
                var f = c.Attachments.FirstOrDefault(x => x.Id == att);
                if (f != null) { p.SourceAttachmentId = att; p.SourceFileName = f.FileName; }
            }
            if (a["doc_total"] is JsonValue) { decimal t = Dec(a, "doc_total"); if (t > 0) p.DocTotal = t; }
            try
            {
                foreach (var h in dal.FindPurchaseDuplicates(p.PartyId, p.RefNo, p.Date, p.DocTotal))
                {
                    string date = h.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
                    p.Warnings.Add(h.Source switch
                    {
                        "bill" => $"تكرار محتمل: رقم فاتورة المورد «{p.RefNo}» مسجّل بالفعل فى فاتورة المشتريات رقم {h.DocNo} ({date}).",
                        "bill_amount" => $"تكرار محتمل: فاتورة مشتريات رقم {h.DocNo} لنفس المورد بنفس التاريخ ونفس الإجمالى.",
                        _ when h.Status == AiDraftStatus.Consumed => $"تكرار محتمل: رقم فاتورة المورد «{p.RefNo}» استُخدم فى مسودة سابقة حُفظت برقم {h.DocNo} ({date}).",
                        _ => $"تكرار محتمل: يوجد مسودة أخرى معلّقة بنفس رقم فاتورة المورد «{p.RefNo}» ({date}).",
                    });
                }
            }
            catch (Exception ex) { operations.DataDictionaryLogerException(ex); }
        }

        private static (long Id, string Name) Store(JsonObject a, AiDraftsCLS dal)
        {
            long storeId = Long(a, "store_id");
            if (storeId <= 0) throw new ArgumentException("حدّد المخزن بمعرّفه (store_id). إن لم يذكره المستخدم فاسأله بـ ask_user.");
            string? name = dal.ActiveName("store", storeId);
            if (name == null) throw new ArgumentException($"المخزن {storeId} غير موجود أو غير نشط. ابحث عنه بـ find_entity (type=store).");
            return (storeId, name);
        }

        private static void ReadLines(JsonObject a, AiDraftPayload p, AiDraftsCLS dal, bool checkBalance, bool mergeDuplicates, bool allowPrice)
        {
            if (a["lines"] is not JsonArray arr || arr.Count == 0) throw new ArgumentException("أضف بنداً واحداً على الأقل فى lines.");
            if (arr.Count > MaxLines) throw new ArgumentException($"البنود أكثر من {MaxLines}.");
            var stock = new Dictionary<long, AiDraftStockRow>();
            foreach (var n in arr)
            {
                if (n is not JsonObject o) throw new ArgumentException("كل بند فى lines كائن JSON.");
                decimal ql = Dec(o, "qty_large"), qs = Dec(o, "qty_small");
                decimal? price = allowPrice && o["price"] is JsonValue ? Dec(o, "price") : null;
                string text = Text(o, "item_text", 200);
                long itemId = Long(o, "item_id");
                if (ql < 0 || qs < 0) throw new ArgumentException("الكمية لا تكون سالبة.");
                if (price < 0) throw new ArgumentException("السعر لا يكون سالباً.");
                if (ql + qs <= 0) throw new ArgumentException($"حدّد كمية للبند «{(text.Length > 0 ? text : itemId.ToString(CultureInfo.InvariantCulture))}».");

                if (itemId <= 0)
                {
                    p.Unmatched.Add(new AiDraftUnmatched { Text = text, QtyLarge = ql, QtySmall = qs, Price = price, Reason = "لم يُحدَّد الصنف" });
                    continue;
                }
                var row = dal.FindStock(itemId, p.StoreId);
                if (row == null)
                {
                    p.Unmatched.Add(new AiDraftUnmatched { Text = text.Length > 0 ? text : "صنف " + itemId, QtyLarge = ql, QtySmall = qs, Price = price, Reason = "الصنف غير معرّف فى مخزن " + p.StoreName });
                    continue;
                }
                if (!row.ItemActive)
                {
                    p.Unmatched.Add(new AiDraftUnmatched { Text = row.ItemName, QtyLarge = ql, QtySmall = qs, Price = price, Reason = "الصنف غير نشط" });
                    continue;
                }
                stock[row.UnitItemId] = row;
                var existing = mergeDuplicates ? p.Lines.FirstOrDefault(l => l.UnitItemId == row.UnitItemId) : null;
                if (existing != null)
                {
                    existing.QtyLarge += ql;
                    existing.QtySmall += qs;
                    if (price is > 0) existing.Price = price;
                    continue;
                }
                p.Lines.Add(new AiDraftLine
                {
                    UnitItemId = row.UnitItemId, ItemId = row.ItemId, ItemName = row.ItemName, Code = row.Code,
                    QtyLarge = ql, QtySmall = qs, Price = price, Notes = Text(o, "notes", 200),
                });
            }
            if (checkBalance)
                foreach (var g in p.Lines.GroupBy(l => l.UnitItemId))
                {
                    var r = stock[g.Key];
                    if (r.IsWithoutBalance) continue;
                    decimal need = g.Sum(l => l.QtyLarge * r.SmallPerLarge + l.QtySmall);
                    if (need > r.BalanceSmall)
                        p.Warnings.Add($"الكمية المطلوبة من «{r.ItemName}» أكبر من الرصيد المتاح فى المخزن ({Qty(r.FullQua)} {r.BUnit}{(r.DQua != 0 ? " + " + Qty(r.DQua) + " " + r.SUnit : "")}).");
                }
            if (p.Unmatched.Count > 0)
                p.Warnings.Insert(0, $"{p.Unmatched.Count} بند غير مطابق لن يُضاف تلقائياً؛ اختره يدوياً فى الشاشة.");
            if (p.Lines.Count == 0 && p.Unmatched.Count == 0) throw new ArgumentException("لا توجد بنود صالحة.");
        }

        private static AiToolResult Save(AiToolContext c, AiDraftsCLS dal, AiDraftPayload p, string title, string urlTemplate, string subtitle)
        {
            long id = dal.Insert(c.User.UserId, c.ConversationId, title, p);
            c.DraftIds.Add(id);
            string url = urlTemplate.Replace("{id}", id.ToString(CultureInfo.InvariantCulture));
            string sub = $"{subtitle} · {p.Lines.Count} صنف" + (p.Unmatched.Count > 0 ? $" · {p.Unmatched.Count} غير مطابق" : "");
            var warnings = new JsonArray(p.Warnings.Select(w => (JsonNode)JsonValue.Create(w)!).ToArray());
            return new AiToolResult
            {
                Content = JsonSerializer.Serialize(new
                {
                    ok = true,
                    draft_id = id,
                    shown = "ظهرت للمستخدم بطاقة «مسودة جاهزة» بزر يفتحها فى الشاشة. لم يُحفظ أى مستند ولم يتحرك المخزون؛ المستخدم يراجع ويحفظ بنفسه. لا تقل إنك حفظت الفاتورة ولا تكتب الرابط.",
                    matched_lines = p.Lines.Select(l => new { l.ItemName, l.Code, qty_large = l.QtyLarge, qty_small = l.QtySmall, price = l.Price }),
                    unmatched = p.Unmatched.Select(u => new { u.Text, u.Reason }),
                    warnings = p.Warnings,
                    duplicate_warning = p.Warnings.Any(w => w.StartsWith("تكرار محتمل")) ? "نبّه المستخدم بوضوح لاحتمال التكرار." : null,
                }, Json),
                RowCount = p.Lines.Count,
                Title = title,
                Block = new JsonObject
                {
                    ["type"] = "draft",
                    ["draft_id"] = id.ToString(CultureInfo.InvariantCulture),
                    ["kind"] = p.Kind,
                    ["title"] = title,
                    ["subtitle"] = sub,
                    ["url"] = url,
                    ["warnings"] = warnings,
                },
            };
        }

        // ───────────── مساعدات ─────────────

        private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        private static long Long(JsonObject a, string k)
        {
            if (a[k] is not JsonValue v) return 0;
            if (v.TryGetValue<long>(out long l)) return l;
            if (v.TryGetValue<double>(out double d) && d == Math.Floor(d) && d < long.MaxValue) return (long)d;
            if (v.TryGetValue<string>(out var s) && long.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out l)) return l;
            return 0;
        }

        private static decimal Dec(JsonObject a, string k)
        {
            if (a[k] is not JsonValue v) return 0;
            if (v.TryGetValue<decimal>(out decimal m)) return m;
            if (v.TryGetValue<double>(out double d)) return (decimal)d;
            if (v.TryGetValue<string>(out var s) && decimal.TryParse(s.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out m)) return m;
            throw new ArgumentException($"«{k}» يجب أن يكون رقماً.");
        }

        private static bool? Bool(JsonObject a, string k) => a[k] is JsonValue v && v.TryGetValue<bool>(out bool b) ? b : null;

        private static string Text(JsonObject a, string k, int max)
        {
            string s = a[k] is JsonValue v && v.TryGetValue<string>(out var t) ? t.Trim() : "";
            return s.Length > max ? s[..max] : s;
        }

        private static string? Notes(JsonObject a)
        {
            string s = Text(a, "notes", MaxNotes);
            return s.Length == 0 ? null : s;
        }

        private static DateTime? Date(JsonObject a, string k)
        {
            string s = Text(a, k, 20);
            if (s.Length == 0) return null;
            if (!DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                throw new ArgumentException($"«{k}» بصيغة yyyy-MM-dd.");
            if (Math.Abs((d - DateTime.Today).TotalDays) > 366) throw new ArgumentException("التاريخ بعيد جداً عن اليوم؛ تأكد منه مع المستخدم.");
            return d;
        }

        private static string Qty(decimal v) => v.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
