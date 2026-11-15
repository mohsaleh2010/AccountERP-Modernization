## P8.3 — شرح أخطاء الفاتورة الإلكترونية المصرية (خطة تفصيلية)

كُتبت 2026-11-15 بواسطة وكيل التخطيط Fable 5.2 اعتماداً على الكود بعد P8.2 (مكتملة)، وحفظها المنفّذ (الوكيل كان للقراءة فقط). تحل محل مخطط P8.3 فى `ai-assistant-p8-plan.md`. القرارات مرقّمة ف1… وكلها محسومة؛ المنفّذ لا يعيد التفكير فيها إلا إذا نقضها فحص من «فحوص مفتوحة».

> ملاحظات قبل التنفيذ:
> - **نتيجة «الفحص الأول»:** النظام لا يخزّن خطأ الرفض. `Taxdocuments`/`TaxdocumentsReverse` تُكتبان للمستندات المقبولة فقط، و`Errors` فيهما (إن حمل) أخطاء مستندات أخرى فى نفس الإرسال، و`Staus`/`StausStr` لا يكتبهما أى كود. نص الرفض يظهر فى `MessageBox` على WinForms فقط. لذلك **المسار الأساسى = لصق نص الخطأ**، والـ View يعطى حالة الرفع.
> - لا توجد شاشة Razor للفاتورة الإلكترونية. الزر يوضع فى `SalesInvoicePage` و`SalesInvoicesList` (ف14).
> - الحوار يعرض المطابقة الثابتة فوراً **بلا نموذج**، والنموذج عند الطلب فقط (ف3).
> - لا تغيير فى `AccountsEInvoicesLIB` ولا `AccountCommonDataLIB/SentToPortalTaxEG` ولا أى فورم WinForms. لا إرسال ولا إعادة إرسال ولا تصحيح تلقائى ولا استدعاء لبوابة الضرائب.
> - أكواد ETA الرسمية لم تُتحقق من وثائق SDK؛ الكتالوج يُبنى على أسماء خطوات التحقق والعبارات ومسارات الحقول، والأكواد تضاف لاحقاً من تبويب المعرفة (بديل الفحص 4).
> - **خلل قائم خارج النطاق (يُبلَّغ للمستخدم ولا يُصلَّح هنا):** `ConnectCLS.sendinvoice` يعرض «تم الارسال بنجاح» حين يرجع `submissionId` والمستند مرفوض (`rejectedDocuments`)، ولا يحفظ شيئاً؛ و`ResponseAfterSend.Error.ToString()` يُسقط الأكواد.

### حقائق من الكود

- **الإرسال:** `AccountCommonDataLIB/SentToPortalTaxEG/SendInvoiceToPortalCLS.cs` ⟵ `NewFolder/DocumentCLS.cs` ⟵ `ConnectCLS.sendinvoice`؛ المرتجع `SendInvoiceReverseToPortalCLS`/`DocumentReverseCLS`. المستدعون WinForms فقط (`CarsSystemLib/Invoice/LoadingSendInvoiceToPortalFrm.cs`، `LoadingSendInvoiceReverseToPortalFrm.cs`، `Invoices_ListFrmCar.cs`).
- **عند القبول:** صف `Taxdocuments` (`invoices_ListId`, `TaxInvoices_ListId`, `TaxInvoiceNo`, `uuid`, `hashKey`, `internalId`, `longId`, `submissionId`, `Errors`, `IsSuccessded = 1`, `DateOfSent`) ثم `Invoices_List.TaxInternalId` و`IsUploadToPortal = 1`. إعادة الإرسال تضع `IsSuccessded = 2` على الصف السابق.
- **عند الفشل:** `Msg = "لم يتم الارسال"` + `result.Content` الخام + `StatusCode` + `ErrorMessage`. لا صف.
- **رسائل محلية:** «لا توجد فاتورة»، «لم يتم اعداد الفاتورة لرفعها على منصة الضرايب»، «خطأ فى اعداد الفاتورة»، «تم رفع الفاتورة الى منظومة الضرايب المصرية» + «حالة رفع الفاتورة {status}».
- **نماذج الرد:** `AccountModelsLib/BusinessLibShared/Tax/Document/ResponseAfterSend.cs` (`rejectedDocuments[].error { code, message, target, propertyPath, details[] }`)؛ `Models/Tax/NewFolder1/GetDocumentModel.cs` (`validationResults.validationSteps[] { name, status, error }`، و`Validation` تنتج «֎ {name} Status: {status}»).
- **الجداول:** `Taxdocuments`/`TaxdocumentsReverse` 14 عموداً، `Id bigint IDENTITY`. `Invoices_List`: `TaxInvoices_ListId`, `TaxInternalId`, `TaxInvoiceNo`, `IsUploadToPortal`, `IsInvoiceTaxInfo`.
- **المعرفة:** `AI_Knowledge.Kind nvarchar(20)`، فهرس فريد `(Kind, KeyName)`. `AiKnowledgeKinds.All`. `SeedRows()` + `SyncSeed` (يدرج الجديد، يحدّث المزروع غير المعدَّل عند رفع `Version`، لا يحذف). `Visible` يفحص `ViewsCsv`/`RoutesCsv`. `RankAsync(only, minKeywordScore)`. `SaveAdmin` يقبل glossary/rule/example. `MaxContent = 4000`, `MaxKeywords = 1000`.
- **الترجمة:** البادئة الجديدة **`AIEI_`** غير مستعملة. اختبارات P8.3 من **م-487**.

### القرارات (ف = P8.3)

| # | القرار | السبب |
|---|---|---|
| ف1 | المصدر الأساسى لنص الخطأ = ما يلصقه المستخدم. المخزَّن (`Errors`) يُقرأ إن وُجد فقط | الرفض لا يُحفظ؛ Zoho/Odoo يعرضان رد الجهة ويشرحانه |
| ف2 | الكتالوج مقاطع `AI_Knowledge` بنوع `eta_error` مزروعة من الكود وقابلة للتعديل من تبويب المعرفة | يتغير مع ETA بلا نشر |
| ف3 | المطابقة ثابتة بلا نموذج وتُعرض فوراً؛ النموذج بزر «شرح مفصّل» أو فى المحادثة | تعمل بلا نموذج؛ نهج س1 |
| ف4 | الترتيب: كود ⟵ مسار حقل ⟵ خطوة تحقق ⟵ عبارة؛ كل مطابقة تحمل نوعها | الكود أدق؛ المنسوخ كثيراً بلا أكواد |
| ف5 | لا مطابقة ⟵ `RankAsync` مقصور على `eta_error` (حتى 3) «تقريبى»؛ لا شىء ⟵ «غير معروف» بلا تخمين | D365 يعرض الخام |
| ف6 | النموذج يصوغ من نص المقاطع فقط ويذكر `[eta:KEY]`؛ غير الموثَّق يقول ذلك | يمنع اختلاق قواعد ضريبية |
| ف7 | مقاطع `eta_error` خارج `search_schema` العام (`only == null`) | ضجيج |
| ف8 | `RoutesCsv` = `/accounts/invoices/list,/accounts/invoices/sales-invoice` | لا تظهر لمن لا يملك المبيعات |
| ف9 | View `ai.v_einvoice_status`: صف لكل فاتورة ضريبية (مبيعات ومرتجع) مع آخر محاولة رفع؛ يفلتر بـ `ai_tax` والمحذوف | سؤال شائع؛ حالة لكل مستند |
| ف10 | الـ View يعرض `errors_text` و`portal_status` رغم فراغهما | يعمل إن حُفظ الرفض لاحقاً |
| ف11 | بلا `hashKey` ولا أسرار البوابة | أسرار |
| ف12 | أداة واحدة `explain_einvoice_error(error_text?, sale_id?, doc_kind?)`؛ أحد الأولين إلزامى | أوضح للنموذج |
| ف13 | الصلاحية: `HasRoute("/accounts/invoices/list")` أو `HasRoute("/accounts/invoices/sales-invoice")`؛ الحالة تشترط `v_einvoice_status` فى `AllowedViews` | نظام الصلاحيات |
| ف14 | الزر فى `SalesInvoicePage` (ضريبية و`Id > 0`) وفى `SalesInvoicesList` (صف ضريبى). لا صفحة ولا عنصر قائمة | Odoo يضع الخطأ على الفاتورة |
| ف15 | لا زر فى WinForms | المساعد ويب فقط |
| ف16 | الحوار `MudDialog`: شريط حالة + حقل لصق (8000) + «اشرح» + النتائج + «شرح مفصّل» + «نسخ» | بسيط |
| ف17 | «شرح مفصّل» = `TryAskNew` برسالة مركّبة ثم `/assistant`؛ مع `HasAccess` فقط | س30 |
| ف18 | كل مقطع بأربعة حقول: المعنى، السبب الشائع، أين تصحّحه فى النظام، ملاحظة | عملى |
| ف19 | مفاتيح المطابقة فى `Keywords`: `code:… ; path:… ; step:… ; phrase:…` (مطبّعة) | لا عمود جديد |
| ف20 | نص الخطأ بيانات: يُقص إلى 8000 ويُحاط بعلامات فى ناتج الأداة | حقن التعليمات |
| ف21 | المدير يضيف/يعدّل `eta_error` من تبويب المعرفة (`SaveAdmin`) | بلا نشر |
| ف22 | لا جدول ولا إعداد جديد؛ التدقيق `AI_ToolCalls_Audit` (`einvoice_explain` للحوار عند الخطأ فقط) | نهج س22 |
| ف23 | الحوار والأداة للويب فقط؛ غياب البوابة ⟵ لا زر | ق28 |
| ف24 | لا تعديل للفاتورة ولا فتح تلقائى | لا تصحيح تلقائى |

### 1) مصادر البيانات وحالة الرفع

نص ملصوق (أساسى)؛ `ai.v_einvoice_status`؛ `AI_Knowledge` (`eta_error`). `upload_state`: `not_sent` (لا صف و`IsUploadToPortal` ≠ 1)، `sent` (آخر صف = 1)، `superseded` (آخر صف ليس 1)، `flag_only` (`IsUploadToPortal = 1` بلا صف). لا «مقبولة/مرفوضة» لأن حالة البوابة غير مخزَّنة.

### 2) المحلّل والمطابق (نقى)

`AiEtaErrorMatcher.Parse(raw)` ⟵ `AiEtaSignals { Codes, Paths, Steps, NormalizedText, Fragments }`: قص 8000؛ JSON متسامح (`error.code/target/propertyPath`, `details[]`, `rejectedDocuments[].error`, `validationSteps[]` غير Valid)؛ نصى: «֎ {name} Status: {status}»، رموز HTTP مع كلماتها، رموز `[A-Z]{1,4}\d{2,4}` و`…(Exceeded|Submitter|Structure|Submission)`؛ التطبيع `AiSearchText.Normalize`؛ `Fragments` حتى 20.
`Match(signals, rows)`: `code` تام ⟵ `path` تساوٍ أو لاحقة ⟵ `step` احتواء ⟵ `phrase` (≥ 6 أحرف) فى النص المطبّع؛ إزالة التكرار؛ حد 8؛ `Unmatched` حتى 5 × 300 حرف.

### 3) الكتالوج المزروع

`Razors/AccountERP.AI/Knowledge/AiEtaErrorCatalog.cs`، مفاتيح `eta:<slug>`، محتوى عربى بحقول ف18، نحو 40 مقطعاً: الاتصال والدخول (401، 403، المهلة، 429، 5xx)، التوقيع (التوكن، الشهادة، PIN، التوقيع)، بنية الإرسال (البنية، الحجم، التكرار، الإصدار)، الممول (الرقم الضريبى، الفرع، النشاط، خطوة الممول)، المستلم (الرقم، النوع، العنوان، الرقم القومى)، الأصناف (كود غير مسجّل، EGS/GS1، الوحدة، الوصف)، القيم والضرائب (الإجماليات، إجمالى السطر، نوع الضريبة، إجماليات الضرائب، الكسور، الخصم، العملة)، التاريخ والمراجع (مستقبلى، قديم، internalID مكرر، مرجع مفقود)، خطوات عامة (core/simple fields)، رسائل النظام المحلية الأربع. «أين تصحّحه» يسمّى شاشة النظام. لا مقطع يَعِد بقبول المصلحة.

### 4) قاعدة البيانات (كائنات جديدة فقط)

- View `ai.v_einvoice_status` فى `AiReadViewsCLS.Views`: الأعمدة `doc_kind, sale_id, invoice_no, invoice_date, customer_id, customer_name, branch_id, net_total, tax_invoice_no, internal_id, is_uploaded, uuid, submission_id, sent_date, send_state, errors_text, portal_status, attempts, upload_state`؛ `OUTER APPLY TOP (1)` على `Taxdocuments` (المقبول أولاً ثم الأحدث) و`OUTER APPLY COUNT(*)`؛ `WHERE` المحذوف + `IsInvoiceTaxInfo = 1` + `SESSION_CONTEXT(N'ai_tax') IN (-1, 1)`؛ `UNION ALL` للمرتجع بنفس الأعمدة (يُحذف إن نقضه الفحص 2).
- `AiSchemaCatalog.Views` + `AiViewDoc("v_einvoice_status", …, RSales)`.
- `AiKnowledgeKinds.EtaError = "eta_error"` فى `All`.
- لا جداول، لا إعدادات، لا فهارس على جداول النظام، لا عنصر قائمة.

### 5) الأجزاء

**P8.3.1 — الكتالوج والمطابق والـ View (بلا واجهة وبلا أداة).** أول خطوة: الفحوص 1، 2، 3، 7، 9. جديد `AccountModelsLib/BusinessLibShared/Ai/AiEInvoiceModels.cs` (`AiEInvoiceConst { MaxErrorChars = 8000, MaxMatches = 8, MaxApprox = 3, MaxUnmatched = 5, AuditTool = "einvoice_explain", DocInvoice = "invoice", DocReturn = "return" }`، `AiEtaSignals`، `AiEtaMatch { RowId, Key, Title, Content, MatchType, MatchedOn }`، `AiEtaMatchTypes { Code, Path, Step, Phrase, Approx }`، `AiEInvoiceStatus`، `AiEInvoiceExplainResult { Status, Matches, Unmatched, UsedStoredError, NoErrorText, Error }`، `IAiEInvoiceGateway { CanUse, GetStatus, ExplainAsync }`)، `AiEtaErrorMatcher.cs` (net48، Newtonsoft إن كان مرجعاً وإلا يُنقل إلى `AccountERP.AI`)، `AiEtaErrorCatalog.cs`. تعديل `AiKnowledgeService` (`SeedRows` + استبعاد `eta_error` حين `only == null` + `EtaErrorRows(hasRoute)` + `EtaApproxAsync`)، `AiReadViewsCLS`، `AiSchemaCatalog`، `AiKnowledgeCLS`. القبول: Ensure مرتين؛ الـ View بـ `ai_tax` -1/1 ثم 0 بلا صفوف؛ `SyncSeed` مرتين ثابت؛ `Parse`/`Match` للحالات؛ `search_schema` بلا `eta_error`؛ انحدار P7 وP8.2.1.

> **تم التنفيذ P8.3.1 (2026-11-15):** `AiEInvoiceModels.cs` + `AiEtaErrorMatcher.cs` (net48/C# 7.3 مُختبَر وحده، Newtonsoft) + `Knowledge/AiEtaErrorCatalog.cs` (46 مقطعاً) + `AiKnowledgeKinds.EtaError` + `ai.v_einvoice_status` + `AiViewDoc`. **الفحوص:** (1) جدولا `Taxdocuments`/`TaxdocumentsReverse` **غير موجودين** فى قاعدة التطوير (يُنشآن مع إعداد المنظومة)، فالـ View يُبنى برمزين `/*AI_TAXDOC*/`/`/*AI_TAXDOC_REV*/` يُستبدلان فى `Ensure` بالجدول أو بجدول مشتق فارغ بنفس الأعمدة (الحالة من `IsUploadToPortal` وحده)؛ (2) `TaxdocumentsReverse.invoices_ListId` = معرّف المرتجع (`DocumentReverseCLS`) ⇒ جزء المرتجع باقٍ؛ (3) `IsSuccessded = 2` = أُعيد الإرسال ⇒ `superseded`؛ (4) الأكواد الرسمية لم تُضف (المفاتيح step/path/phrase/HTTP + أسماء أخطاء الإرسال المعروفة BadStructure/MaximumSizeExceeded/IncorrectSubmitter/DuplicateSubmission)؛ (7) TOP 50 = 72 مللى؛ (9) Newtonsoft مرجع فى المكتبة. إضافات: `FragmentKeys` يربط رسالة JSON بكودها ومسارها فلا تُعدّ «غير مفسّرة»؛ رموز HTTP لا تُلتقط من أرقام عشرية؛ `Search` المتزامن أيضاً يستبعد `eta_error`؛ `SaveAdmin` يقبل `eta_error` ويضبط `RoutesCsv` (من P8.3.3). Harness `p83test` 44/44 (scratchpad الجلسة c0ee7a88)، وانحدار P7.3 51/51.

**P8.3.2 — البوابة وأداة المحادثة.** أول خطوة: الفحص 5. جديد `Razors/AccountERP.AI/EInvoice/AiEInvoiceGateway.cs` (`(ReadOnlySqlExecutor, AiKnowledgeService)`؛ `CanUse` ف13؛ `GetStatus` عبر `_sql.Execute` بـ `SessionFor`؛ `ExplainAsync`: صلاحية ⟵ حالة ⟵ نص ملصوق وإلا `ErrorsText` ⟵ `NoErrorText` ⟵ `Parse`/`Match` ⟵ `EtaApproxAsync`؛ الأخطاء تدقيق + `Error` بلا رمى). أداة `explain_einvoice_error` فى `AiToolRegistry` (ناتج: الحالة، `[eta:KEY] (نوع) العنوان` + المحتوى، غير الموجود، نص المستخدم داخل `<<<نص الخطأ … >>>`). قسم «## الفاتورة الإلكترونية» فى `SystemPrompt` حين الأداة متاحة. التسجيل Scoped. حالات تقييم جديدة: `eta_explain_paste` (tool)، `eta_not_sent_list` (result)، `eta_no_resend` (refuse). القبول (harness): «No slots found» ⟵ `eta:token-missing` phrase؛ JSON `receiver.id` ⟵ path؛ عشوائى ⟵ `Unmatched`؛ `sale_id` مرفوعة ⟵ `sent`؛ بلا المبيعات ⟵ `CanUse = false`؛ `TaxFilter = 0` ⟵ `Found = false`؛ 20000 حرف يُقص؛ حقن لا يغيّر الناتج.

> **تم التنفيذ P8.3.2 (2026-11-15):** `EInvoice/AiEInvoiceGateway.cs` فيه `AiEInvoiceLogic` (منطق مشترك: `Allowed`، `Status`، `ExplainAsync`، `ToolContent`، `StateAr`) و`AiEInvoiceGateway` (Scoped، خطاف `ContextSource`، تدقيق `einvoice_explain` للخطأ فقط). أداة `explain_einvoice_error` (`RunAsync`) فى `AiToolRegistry` + عنوانها؛ قسم «## الفاتورة الإلكترونية» فى `SystemPrompt` حين ف13؛ التسجيل Scoped؛ 3 حالات تقييم (`eta_explain_paste`، `eta_not_sent_list`، `eta_no_resend`) ⇒ المزروع 63. **الفحص 5:** حلقة الأدوات فى `AiChatGateway` تسجّل كل أداة تلقائياً. Harness `p83test` 65/65؛ انحدار P7.4 138/141 والثلاثة الباقية أعداد ثابتة «60 حالة» فى الـ harness القديم (صارت 63) — متوقع.

**P8.3.3 — الحوار والأزرار وتبويب المعرفة والترجمة.** أول خطوة: `ui-ux-pro-max` ثم الفحصان 6 و8. جديد `Services/AiEInvoiceService.cs` (Scoped، نمط `AiBriefService`)، `Components/Assistant/EInvoiceErrorDialog.razor` (`MudDialog` Medium، ملء الشاشة تحت 600px، شريط حالة `MudAlert` بأيقونة ونص، `uuid` LTR + نسخ، حقل 6 أسطر 8000 `dir=auto`، «اشرح» + `aria-busy`، بطاقة لكل مطابقة بشارة النوع، «لم نجد له شرحاً»، `aria-live`، «شرح مفصّل» مع `HasAccess` (false ⟵ `AIS_AssistantBusy`)، «نسخ الشرح»، تنبيه دائم). زر فى `SalesInvoicePage` و`SalesInvoicesList`. `AiKnowledgeTab` يدعم `eta_error`، و`SaveAdmin` يقبله (`RoutesCsv` ف8). التسجيل فى `Program.cs` فقط. مفاتيح `AIEI_` ×4؛ م-487…م-496.

> **تم التنفيذ P8.3.3 (2026-11-15):** `Services/AiEInvoiceService.cs` (Scoped، مسجّل فى `Program.cs` للويب فقط؛ `IsAvailable` = البوابة + `CanUse`، `CanAskAssistant`، `ComposeAsk` يقص النص 8000، `PlainText` للنسخ)، `Components/Assistant/EInvoiceErrorDialog.razor` (شريط حالة بأيقونة ولون، UUID LTR + نسخ، حقل 6 أسطر `dir=auto`، النص المحفوظ يُملأ تلقائياً، بطاقات بشارة نوع المطابقة، «لم نجد له شرحاً»، `aria-live`/`aria-busy`، «شرح مفصّل» ⇒ `TryAskNew` ثم `/assistant` وإلا `AIS_AssistantBusy`، ملء الشاشة تحت 600px). **الفحص 6:** الزر فى شريط `inv-dock-actions` بـ `SalesInvoicePage` (محفوظة + `_isTaxInvoice`، يخدم المرتجع بنوع `return`)؛ وفى `SalesInvoicesList` أيقونة لكل صف `IsInvoiceTaxInfo = 1` (الاستعلام `Invoices_List.*` فيه العمود، وللمرتجع كذلك). **الفحص 8:** `AiKnowledgeTab` يضيف النوع `eta_error` للاختيار والفلتر واللون والتلميح (`AIEI_HEta`)، و`SaveAdmin` يضبط `RoutesCsv`. 30 مفتاح `AIEI_` ×4. اختبارات م-487…م-496 فى الخطة الرئيسية. بناء الويب ناجح؛ لا اختبار متصفح (المستخدم يختبر عند الطلب). **P8.3 مكتملة.**

### 6) مفاتيح الترجمة — `AIEI_` (30)

| المفتاح | النص العربى |
|---|---|
| `AIEI_Button` | اشرح خطأ الفاتورة الإلكترونية |
| `AIEI_Title` | شرح خطأ الفاتورة الإلكترونية |
| `AIEI_TitleFor` | شرح خطأ الفاتورة الإلكترونية — فاتورة {0} |
| `AIEI_StateNotSent` | لم تُرسل هذه الفاتورة إلى بوابة الضرائب بعد |
| `AIEI_StateSent` | أُرسلت إلى بوابة الضرائب بتاريخ {0} |
| `AIEI_StateSuperseded` | أُرسلت سابقاً ثم أُعيد إرسالها؛ لا يوجد إرسال سارٍ مسجَّل |
| `AIEI_StateFlagOnly` | معلَّمة كمرفوعة بلا سجل إرسال |
| `AIEI_StateUnknown` | تعذّر قراءة حالة الفاتورة |
| `AIEI_Attempts` | عدد مرات الإرسال: {0} |
| `AIEI_Uuid` | الرقم التعريفى (UUID) |
| `AIEI_PasteLabel` | نص رسالة الخطأ |
| `AIEI_PasteHint` | الصق الرسالة كما ظهرت فى النظام أو فى بوابة الضرائب (يمكن نسخها بـ Ctrl+C من نافذة الرسالة) |
| `AIEI_StoredErrorNote` | هذا النص محفوظ مع آخر إرسال لهذه الفاتورة |
| `AIEI_Explain` | اشرح |
| `AIEI_Explaining` | جارٍ المطابقة… |
| `AIEI_ResultsCount` | {0} سبب مطابق |
| `AIEI_MatchCode` | مطابقة بالكود |
| `AIEI_MatchPath` | مطابقة بالحقل |
| `AIEI_MatchStep` | مطابقة بخطوة التحقق |
| `AIEI_MatchPhrase` | مطابقة بالنص |
| `AIEI_MatchApprox` | تقريبى |
| `AIEI_Unmatched` | لم نجد له شرحاً |
| `AIEI_NoMatch` | لم نجد هذا الخطأ فى الكتالوج. راجع الرسالة فى بوابة الضرائب أو اسأل المساعد. |
| `AIEI_AskAssistant` | شرح مفصّل من المساعد |
| `AIEI_CopyExplanation` | نسخ الشرح |
| `AIEI_Copied` | نُسخ الشرح |
| `AIEI_Disclaimer` | الشرح إرشادى. لا يُعدَّل شىء ولا يُعاد الإرسال من هنا. |
| `AIEI_Error` | تعذّر شرح الخطأ الآن. |
| `AIEI_KEta` | خطأ فاتورة إلكترونية |
| `AIEI_HEta` | مفاتيح المطابقة فى «الكلمات المفتاحية»: code:… ; path:… ; step:… ; phrase:… |

### 7) Harness (`p83test`)

النقى (`Parse` لـ JSON كامل/مقطوع، خطوات، HTTP، عربى محلى، فارغ، 20000؛ `ParseKeys`؛ `Match`)؛ المخطط (Ensure/SyncSeed مرتين، مفاتيح فريدة ≤ 150، Keywords ≤ 1000، Content ≤ 4000)؛ الـ View (`Validate`، ai_tax الثلاث، مقارنة `sent` باستعلام مستقل، زمن TOP 50)؛ البوابة والأداة؛ الانحدار P7/P8.2.1/P8.2.3.

### 8) اختبارات المستخدم

| الكود | العنوان |
|---|---|
| م-487 | زر «اشرح خطأ الفاتورة الإلكترونية» يظهر فى فاتورة المبيعات الضريبية المحفوظة فقط |
| م-488 | شريط الحالة يعرض «لم تُرسل» أو تاريخ الإرسال والرقم التعريفى |
| م-489 | لصق خطأ التوكن أو التوقيع يعرض السبب وأين يُصحَّح فوراً دون المساعد |
| م-490 | لصق رد البوابة JSON لخطأ فى بيانات العميل أو كود الصنف يعرض الشرح المطابق بالحقل |
| م-491 | نص غير معروف يعرض «لم نجد له شرحاً» دون تخمين |
| م-492 | «شرح مفصّل من المساعد» يفتح محادثة جديدة مستشهدة بالكتالوج، ولا يظهر لمن لا يملك المساعد |
| م-493 | لصق الخطأ فى المحادثة يُشرح، وطلب إعادة الإرسال أو التصحيح يُرفض بأدب |
| م-494 | «ما الفواتير الضريبية التى لم تُرفع هذا الشهر؟» يعيد القائمة حسب الصلاحية |
| م-495 | المدير يضيف مقطع خطأ من تبويب المعرفة فيُطابَق، ومستخدم بلا المبيعات لا يرى الزر |
| م-496 | الحوار باللغات الأربع والاتجاهين والداكن والهاتف، والزر فى قائمة الفواتير ومرتجع المبيعات |

### 9) المخاطر

كتالوج غير دقيق ضريبياً (ف6، ف18، التنبيه الدائم، المدير يعدّل)؛ النص بلا أكواد (ف4)؛ «تم الارسال بنجاح» مع رفض فعلى (خلل قائم خارج النطاق)؛ حالة البوابة غير مخزَّنة («أُرسلت» لا «مقبولة»)؛ أداء `OUTER APPLY` (الفحص 7)؛ الحقن (ف20)؛ أرقام ضريبية تذهب للنموذج فقط بضغطة «شرح مفصّل»؛ نحو 40 صفاً جديداً تُعيد بناء الفهرس المتجهى مرة.

### 10) فحوص مفتوحة

1. (P8.3.1) صفوف `Taxdocuments`/`TaxdocumentsReverse` بـ `Errors`/`StausStr` غير فارغ (قراءة فقط). وُجدت ⟵ عباراتها للمفاتيح؛ لا ⟵ اللصق.
2. (P8.3.1) `TaxdocumentsReverse.invoices_ListId` = معرّف المرتجع؛ وأعمدة المرتجع من `v_sales_returns`. لم يثبت ⟵ الفواتير فقط.
3. (P8.3.1) قيم `IsSuccessded` الأخرى ⟵ `superseded`.
4. (P8.3.1) أكواد ETA الرسمية. تعذّر ⟵ step/path/phrase فقط (المعتمد الآن).
5. (P8.3.2) هل مسار الأدوات يكتب التدقيق لكل أداة؛ لا ⟵ `Insert` صريح.
6. (P8.3.3) شريط أزرار `SalesInvoicePage` و`_isTaxInvoice` وخاصية الضريبة فى صفوف القائمة؛ لا ⟵ الصفحة فقط.
7. (P8.3.1) زمن TOP 50 > 400 مللى ⟵ حذف `attempts`.
8. (P8.3.3) `AiKnowledgeTab` والفروع الافتراضية.
9. (P8.3.1) Newtonsoft فى `AccountModelsLib`؛ لا ⟵ المطابق فى `AccountERP.AI`.
