# خطة تنفيذ «المساعد الذكي» (AI Assistant) لنظام AccountERP

> الخطة مبنية على قراءة فعلية للمستودع. ما لم أتحقق منه مُعلَّم بـ **[يحتاج تحقق]** ويُحسم فى بداية المرحلة المعنية.
> المسارات نسبية إلى `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects`.

---

## 1) الملخص التنفيذى والمعمارية المستهدفة

**الهدف:** مساعد محادثة داخل تطبيق الويب (Blazor / .NET 10) يفهم مخطط القاعدة وقواعد العمل، يجيب عن الأسئلة والتقارير بقراءة آمنة فقط، يفتح الشاشة المناسبة أو يرسم تقريراً، وينشئ **مسودات** فواتير وأذون مخزن يراجعها المستخدم ويحفظها من الشاشة الحقيقية. تطبيق WinForms لا يُمَس.

**القرارات المعمارية الأساسية (موصى بها):**

| القرار | التوصية | السبب |
|---|---|---|
| مكان التنفيذ | الخادم فقط (`InteractiveServer`) | مفاتيح الـ API والـ SQL لا تصل للمتصفح أو WASM أبداً |
| تجريد المزوّدين | `Microsoft.Extensions.AI` (`IChatClient`, `AIFunction`, `IEmbeddingGenerator`) كواجهة داخلية موحّدة | معيار .NET الحالى، ويغطى OpenAI / Azure OpenAI / Ollama / أى OpenAI-compatible |
| Claude | Adapter خاص فوق حزمة NuGet الرسمية `Anthropic` (وليس عبر shim متوافق مع OpenAI) | للاستفادة من prompt caching و adaptive thinking و `effort` ومعالجة `stop_reason: "refusal"`. **[يحتاج تحقق فى P0]:** هل الحزمة الرسمية تعطى `IChatClient` جاهزاً؛ إن لم تفعل نكتب Adapter رفيعاً |
| حلقة الوكيل | حلقة يدوية نملكها (`AssistantOrchestrator`) | نحتاج بوابات صلاحيات، تدقيق، إيقاف عند `ask_user`، وحدود تكلفة قبل كل tool call |
| الكتابة | مسودة فى جدول `AI_Drafts` ثم تعبئة الشاشة الحقيقية؛ الحفظ بزر المستخدم فقط | النموذج لا ينفّذ INSERT أبداً؛ منطق المخزون والضريبة والقيود يبقى فى مكانه |
| التقارير | `ReportSpec` (JSON) يرسمه مكوّن Blazor، والأرقام تُربط بـ `datasetId` من الخادم | النموذج لا ينسخ أرقاماً (لا هلوسة أرقام) ولا HTML خام (لا XSS) |
| مشروع جديد | `Razors/AccountERP.AI` (net10.0) يرجع له `AccountERP.Web` فقط | `AccountModelsLib` ثنائى الاستهداف (net48)، و`AccountERP.SharedUI` يستخدمه MAUI؛ لا نحمّلهما حزم LLM |

```mermaid
flowchart TD
  subgraph UI["AccountERP.SharedUI (Blazor, MudBlazor 9.7)"]
    P["AssistantPage.razor  /assistant"]
    W["AssistantWidget.razor (FAB + نافذة عائمة) داخل MainLayoutMud"]
    C["AssistantChat.razor (مشترك: رسائل، مرفقات، بطاقات، ask_user)"]
    R["AiReportView.razor (يرسم ReportSpec)"]
    P --> C
    W --> C
    C --> R
  end
  C -->|IAssistantSession (Scoped لكل Circuit)| O
  subgraph AI["Razors/AccountERP.AI (جديد)"]
    O["AssistantOrchestrator: حلقة الوكيل + بثّ + حدود + تدقيق"]
    PR["ProviderRegistry: IChatClient لكل مزوّد"]
    T["ToolRegistry + PermissionGate"]
    K["KnowledgeService: قاموس دلالى + أمثلة + بحث"]
    O --> PR
    O --> T
    O --> K
  end
  PR --> A1["Anthropic"]
  PR --> A2["OpenAI / Azure OpenAI"]
  PR --> A3["Gemini"]
  PR --> A4["Ollama / LM Studio / OpenAI-compatible"]
  PR --> A5["مزوّد مخصص: API Key + URL (أى خدمة)"]
  T --> RO["ReadOnlySqlExecutor: ScriptDom + allow-list + اتصال قراءة فقط"]
  T --> BS["خدمات قائمة: LedgerBalanceEngine, FinalAccountsService, StoreOrderService..."]
  T --> DR["AiDraftsCLS -> AI_Drafts"]
  T --> NAV["open_screen -> TabWorkspaceService / NavigationManager"]
  RO --> DB[("SQL Server: schema ai.* (views)")]
  BS --> DB
  DR --> DB
  O --> AUD["AI_Messages / AI_ToolCalls_Audit / AI_Usage"]
```

**حقائق تحقّقت منها وتؤثر فى التصميم:**
- `Razors/AccountERP.Web/AccountERP.Web/Program.cs` يسجّل الخدمات Scoped لكل Circuit (`TabWorkspaceService`, `UserPreferencesService`, `NotificationCenterService`) وجلسة المستخدم `AccountModelsLib.UserSession` (`User_Id`, `Branch_Id`, `Is_User_Admin`) عبر `UserSessionContext` و`UserSessionCircuitHandler`. خدمة المحادثة تتبع نفس النمط، فتتشارك الصفحة والنافذة العائمة نفس الحالة تلقائياً.
- `IDataProtectionProvider` مستخدم فعلاً فى `Api/SalesRepMobileApi.cs`، فنستخدمه لتشفير مفاتيح المزوّدين.
- `StoreOrderPage.razor` فيه `SaveDraft` و`SaveAndPost` عبر `StoreOrderService` (`AccountModelsLib/BusinessLibShared/Store/StoreOrderService.cs`): دورة مسودة ثم ترحيل موجودة فعلاً لأذون المخزن.
- `SalesInvoicePage.razor` و`PurchaseInvoicePage.razor`: منطق الحفظ داخل الصفحة (`SaveInvoiceCoreAsync`)، ولم أجد مفهوم مسودة للفواتير فى `BusinessLibInvoice`. توجد سابقة تعبئة من مستند آخر (`?fromOrder=`). لذلك مسودة الفاتورة تكون فى `AI_Drafts` وتُعبّأ بها الصفحة عبر `?aiDraft={id}`.
- إضافة القوائم: نمط `HelpMenu` و`InventoryValueMenu` فى `AccountModelsLib/AlterDB_CLS/StockEngineSchemaCLS.cs` (إدراج فقط، `KeysData` مشفّر بـ `Common.EncryptMe`، صلاحيات فى `SysConfigMenusPERMATIONS`).
- آخر رقم اختبار فى `.claude/docs/master-user-test-plan.md` هو **م-350**، فاختبارات هذه الخطة تبدأ من **م-351**.
- الاتصال الحالى `Integrated Security` على قاعدة محلية؛ إصدار SQL Server **[يحتاج تحقق]** (يحسم خيار المتجهات).

---

## 2) نموذج الأمان والسلامة

### 2.1 مسار القراءة (SQL للقراءة فقط) — دفاع متعدد الطبقات

| الطبقة | الإجراء |
|---|---|
| 1. هوية القاعدة | Login/User مستقل (مثل `erp_ai_reader`) له `SELECT` على schema `ai` فقط، و`DENY` على كل ما عداه. الـ connection string فى `ConnectionStrings:AiReadOnly` من user-secrets أو متغير بيئة، ولا يُكتب فى المستودع ولا السجلات (قاعدة CLAUDE.md رقم 6). إنشاء الـ Login خطوة يدوية للمسؤول بسكربت نسلّمه، لا تُنفَّذ تلقائياً |
| 2. سطح البيانات | النموذج يرى Views فقط (`ai.v_sales`, `ai.v_stock`...) بأسماء أعمدة واضحة، وليس الجداول الخام. تُستبعد كلياً: `USER_DETAILES` (كلمات المرور)، `SysConfig*` (قيم مشفّرة)، جداول `AI_*` نفسها، وأى جدول تراخيص أو اتصال |
| 3. فحص الاستعلام | `Microsoft.SqlServer.TransactSql.ScriptDom`: عبارة واحدة فقط من نوع `SELECT`؛ رفض `INTO`, `EXEC`, `OPENROWSET/OPENQUERY`, `xp_*`, `WAITFOR`, linked servers, أسماء ثلاثية/رباعية؛ كل كائن مرجعى يجب أن يكون فى allow-list؛ حقن `TOP (N)` إن غاب |
| 4. حدود التنفيذ | `CommandTimeout` 15 ثانية، حد أقصى 5000 صف، وحد لحجم النتيجة المرسلة للنموذج (أول 50 صفاً + إحصاءات؛ الباقى يبقى على الخادم كـ `datasetId`) |
| 5. تقييد الفرع | `sp_set_session_context 'branch_id'/'user_id'` على اتصال القراءة، والـ Views تفلتر بـ `SESSION_CONTEXT`. **[يحتاج تحقق]:** أى الجداول تحمل `Branch_Id` فعلاً (من `ERP_V1_3.md`) قبل كتابة كل View |
| 6. التدقيق | كل SQL يُسجَّل نصاً مع المستخدم والمدة وعدد الصفوف فى `AI_ToolCalls_Audit` |

### 2.2 مسار الكتابة
- لا توجد أداة تنفّذ DML على جداول النظام. أدوات الكتابة تكتب صفاً فى `AI_Drafts` فقط (جدول جديد).
- المستخدم يفتح الشاشة الحقيقية معبّأة، يراجع، ويضغط حفظ؛ فيمر الحفظ بكل المنطق القائم: `Unit_Item.Full_Qua`/`D_Qua`، `IsInvoiceTaxInfo`، `Is_Tax_Per_Item`، ترقيم `Make_New_Invoice_No`، القيود.
- لا يُستخدم `saveDirectly=1` من المساعد إطلاقاً.
- لا UPDATE/DELETE على صفوف قائمة (تفضيل المستخدم). جداول `AI_*` نفسها تتبع دورة حالة (`Status`) بدل الحذف.

### 2.3 الصلاحيات
- `PermissionGate` قبل كل tool call: كل أداة تعلن `RequiredMenuKey` (مفتاح `SysConfigMenus`)، وتُفحص مقابل `MenuStateService` للمستخدم الحالى. الأدوات غير المسموحة **لا تُرسل للنموذج أصلاً** (فلا يحاول استخدامها).
- `open_screen` يقبل فقط مسارات من كتالوج الشاشات المتقاطع مع قوائم المستخدم.
- Views التقارير تُربط بصلاحية القائمة المقابلة (مثلاً `ai.v_gl_entries` يتطلب صلاحية كشف الحساب).
- استخدام المساعد نفسه صلاحية قائمة (`menuItemAiAssistant`)، وإعداداته صلاحية أخرى للمديرين (`AdminFlage`).

### 2.4 حقن التعليمات (Prompt Injection)
- محتوى الملفات والصور ونتائج الاستعلامات وأسماء الأصناف والعملاء = **بيانات لا تعليمات**؛ تُغلَّف بوسوم واضحة ويُنصّ على ذلك فى system prompt.
- لا أدوات شبكة (لا web fetch) فى الإصدار الأول، فلا قناة تسريب.
- التقرير يُرسم من spec بلا سكربت؛ الروابط داخلية فقط من الكتالوج.
- الضمان الحقيقى ليس الـ prompt بل الطبقات: حساب قراءة فقط + allow-list + مسودات فقط + تأكيد بشرى.

### 2.5 الخصوصية والمفاتيح والتكلفة
- المفاتيح مشفّرة بـ `IDataProtector` (purpose: `AI.ProviderKeys`) فى `AI_Providers.ApiKeyEnc`؛ تُعرض مقنّعة (`••••1234`) ولا تُسجَّل. **تنبيه نشر:** يجب تثبيت مسار مفاتيح Data Protection (`PersistKeysToFileSystem`) وإلا ضاعت المفاتيح عند إعادة النشر **[يحتاج تحقق من الإعداد الحالى]**.
- وضع «محلى فقط»: يمنع أى مزوّد سحابى على مستوى المنشأة.
- وضع «تقليل البيانات»: يُرسل للنموذج المخطط والإحصاءات فقط دون صفوف العملاء (الأرقام تُعرض من الخادم مباشرة).
- حدود: توكنز يومية/شهرية لكل مستخدم، حد أقصى لدورات الأدوات فى الرسالة (مثلاً 12)، حد للمرفقات (10MB، PDF/صور/Excel)، و rate limit بنفس أسلوب `AddRateLimiter` الموجود.
- سجل تدقيق كامل: الرسائل، استدعاءات الأدوات، SQL، التوكنز، التكلفة التقديرية، المزوّد والنموذج.

---

## 3) طبقة المعرفة: كيف «يفهم» المساعد النظام

**التوصية: قاموس دلالى منسّق + بحث هجين، وليس RAG متجهى على كل شىء من اليوم الأول.**

| المكوّن | المصدر | التخزين |
|---|---|---|
| قاموس البيانات | توليد أولى من `ERP_V1_3.md` (تقطيع حسب الجدول) + مطابقة مع `INFORMATION_SCHEMA` الحية (الحقيقة للأعمدة/الأنواع، ولا نفترض PK/FK دون DDL) | `AI_Knowledge` (Kind=`table`) |
| Views الأعمال | `ai.v_sales`, `ai.v_sales_lines`, `ai.v_purchases`, `ai.v_purchase_lines`, `ai.v_stock_balance`, `ai.v_stock_moves`, `ai.v_gl_entries`, `ai.v_customers`, `ai.v_suppliers`, `ai.v_items`, `ai.v_cash_vouchers` — تُخفى فيها الأخطاء الإملائية (`Purshing`, `Detailes`) خلف أسماء واضحة دون تغيير أى جدول | schema `ai` |
| المسرد العربى | مدين=`Madin`، دائن=`Da2n`، صرف=`Sarf`، مشتريات=`Purshing`، إذن إضافة=1007، إذن صرف=1008، آجل/نقدى... من `docs/coding-naming.md` | `AI_Knowledge` (Kind=`glossary`) |
| قواعد العمل | الضريبة (`IsInvoiceTaxInfo`, `Is_Tax_Per_Item`، وعدم استخدام `Invoice_Type` كعلم ضريبى)، اتجاه حركة المخزون لكل نوع مستند | Kind=`rule` |
| أمثلة موثّقة | سؤال عربى ← SQL صحيح على Views (من مجموعة التقييم) | Kind=`example` |
| كتالوج الشاشات | من `Services/KnownRoutes.cs` و`RoutePageRegistry.cs` + `SysConfigMenus.RazorPath` + وصف وباراميترات كل شاشة (`?mode=closing`, `?tab=1..3`...) | Kind=`screen` |

**الاسترجاع:** system prompt ثابت (مخبّأ بالـ prompt caching) يحتوى ملخص Views والمسرد والقواعد؛ والتفاصيل تُجلب بأداة `search_schema`.

**خيارات البحث:**

| الخيار | مزايا | عيوب | الحكم |
|---|---|---|---|
| كلمات مفتاحية + مسرد (LIKE / Full-Text) | بلا تبعيات، يعمل أوفلاين | أضعف مع الصياغات الحرة | **البداية (P2)** |
| Embeddings فى `VARBINARY` + cosine فى الذاكرة | بسيط؛ المعرفة بضعة آلاف مقطع فقط | يحتاج نموذج embeddings | **الترقية (P7)** |
| SQL Server 2025 `VECTOR` | بحث داخل القاعدة | يتطلب إصداراً قد لا يكون عند العملاء | فقط إن ثبت الإصدار |
| قاعدة متجهات خارجية | قابلية توسع | تعقيد نشر بلا حاجة لهذا الحجم | مرفوض |

ملاحظة: Anthropic لا تقدّم embeddings؛ نموذج الـ embeddings يأتى من مزوّد آخر أو محلياً (نموذج متعدد اللغات عبر Ollama).

---

## 4) كتالوج الأدوات (Function Calling)

| الأداة | المدخلات | المخرجات | النوع | الصلاحية |
|---|---|---|---|---|
| `search_schema` | `query`, `kinds[]` | مقاطع معرفة (Views/أعمدة/أمثلة/قواعد) | قراءة | استخدام المساعد |
| `run_readonly_query` | `sql`, `purpose` | `datasetId`, أعمدة، عدد الصفوف، عيّنة 50 صفاً، مجاميع | قراءة | حسب Views المستعملة |
| `find_entity` | `type` (customer/supplier/item/store/account), `text` | أفضل 5 مطابقات مع درجة تشابه | قراءة | حسب النوع |
| `get_trial_balance` | `from`, `to`, `mode` (closing/totals), `level` | `datasetId` + ملخص | قراءة | ميزان المراجعة |
| `get_financial_statement` | `kind` (income/profit_loss/balance_sheet), الفترة | `datasetId` + ملخص | قراءة | القوائم الختامية |
| `get_account_statement` | `partyType`, `partyId`, `from`, `to` | `datasetId` + رصيد افتتاحى/ختامى | قراءة | كشف حساب |
| `get_stock_balance` | `itemId?`, `storeId?`, `asOf?` | `datasetId` | قراءة | أرصدة المخازن |
| `get_item_card` | `itemId`, `storeId?`, الفترة | `datasetId` | قراءة | كارت الصنف |
| `open_screen` | `routeKey`, `params{}` | بطاقة «فتح الشاشة» فى المحادثة | واجهة | صلاحية الشاشة |
| `render_report` | `ReportSpec` (يشير إلى `datasetId`s) | `reportId` + عرض فورى | واجهة | — |
| `ask_user` | `question`, `options[]`, `allowFreeText`, `multi` | إجابة المستخدم | تفاعل | — |
| `create_sales_invoice_draft` | عميل، مخزن، تاريخ، علم ضريبى، بنود (صنف/وحدة/كمية/سعر/خصم) | `draftId` + تحذيرات تحقق | **مسودة** | فاتورة مبيعات |
| `create_purchase_invoice_draft` | مورد، مخزن، رقم فاتورة المورد، بنود | `draftId` | **مسودة** | فاتورة مشتريات |
| `create_store_order_draft` | `orderType` (1007/1008), مخزن، بنود | `draftId` | **مسودة** | إذن المخزن بنوعه |
| `extract_document` | `attachmentId`, `docType` | JSON منظّم (رأس + بنود + ثقة لكل حقل) | قراءة | استخدام المساعد |

قواعد التنفيذ:
- الأدوات المالية تستدعى `LedgerBalanceEngine.GetBalances(Query)` وبناة `FinalAccountsService` (`BuildTrialBalance`, `BuildIncomeStatement`, `BuildProfitLoss`, `BuildBalanceSheet`) نفسها، فتتطابق أرقام المساعد مع الشاشات. هذه مفضّلة على SQL حر، ويُوجَّه النموذج لاستخدامها أولاً.
- مخطط كل أداة `strict` مع `additionalProperties:false`، ويُتحقق من المدخلات على الخادم قبل التنفيذ.
- أدوات المسودات تتحقق من وجود الكيانات فعلياً (لا تُنشأ عملاء/أصناف جديدة)، وتعيد تحذيرات (كمية أكبر من الرصيد، سعر يختلف عن آخر سعر).
- التنفيذ داخل `UserSessionContext.Use(session)` مع `AccountCultureGuard.EnsureInvariant()` كما تفعل الصفحات الحالية داخل `Task.Run`.

---

## 5) مخرجات التقارير

**التوصية: `ReportSpec` يرسمه `AiReportView.razor`، لا HTML خام من النموذج.**

لماذا:
1. الأمان: لا سكربت ولا HTML من النموذج داخل جلسة المستخدم المصادق.
2. الدقة: كل رقم يأتى من `datasetId` على الخادم؛ النموذج يحدد الشكل فقط.
3. الاتساق: نفس ثيم MudBlazor (فاتح/داكن، RTL، `Prefs.DateFormat`).
4. إعادة التشغيل: التقرير المحفوظ = spec + استعلامات بباراميترات، فيُعاد تنفيذه ببيانات اليوم.
5. تكلفة أقل: spec صغير بدل آلاف توكنز HTML.

**مكوّنات `ReportSpec`:** `title`, `subtitle`, `filters[]`, `kpis[]`, `sections[]` من الأنواع `table` (أعمدة بتنسيق: مبلغ/كمية/تاريخ/نسبة، مجاميع، ترتيب)، `chart` (bar/line/pie/stacked عبر MudChart)، `text` (ملاحظات تحليلية)، `drilldown` (رابط شاشة من الكتالوج).

**الوظائف:** طباعة (CSS print كما فى `StoreOrderPage`)، تصدير Excel عبر `MiniExcel` الموجود، PDF عبر طباعة المتصفح فى الإصدار الأول، حفظ/تثبيت فى `AI_SavedReports`، إعادة تشغيل بفترة جديدة، فتح فى تبويب عبر `TabWorkspaceService`.

**متى يفتح شاشة بدل رسم تقرير؟** إن طابق الطلب تقريراً قائماً (ميزان مراجعة، كشف حساب عميل، أرصدة مخزن، كارت صنف) يستدعى `open_screen` بالباراميترات ويعرض ملخصاً قصيراً فى المحادثة. التقرير المرسوم للأسئلة التى لا شاشة لها.
**[يحتاج عمل]:** بعض الشاشات لا تقبل كل الفلاتر عبر query string؛ نضيف `SupplyParameterFromQuery` حسب الحاجة فى P3 (إضافة فقط، بلا تغيير السلوك الافتراضى).

**وضع «تصميم حر» (اختيارى، مؤجّل):** HTML داخل `<iframe sandbox>` بلا `allow-same-origin` وبلا سكربت، مع CSP صارمة والبيانات محقونة من الخادم. مغلق افتراضياً.

---

## 6) تدفق إدخال البيانات

```
طلب نصى أو ملف/صورة
  -> extract_document (نموذج رؤية -> JSON منظّم + ثقة)
  -> find_entity لكل من المورد/العميل/المخزن/الأصناف
  -> ask_user عند الغموض (أكثر من مطابقة، أو لا مطابقة، أو وحدة غير واضحة)
  -> create_*_draft  (صف فى AI_Drafts، Status=Pending)
  -> بطاقة «مسودة جاهزة» فى المحادثة: ملخص + تحذيرات + زر «فتح فى شاشة الفاتورة»
  -> الشاشة الحقيقية ?aiDraft={id} معبّأة مع شريط «مسودة من المساعد — راجع قبل الحفظ»
  -> المستخدم يحفظ بالزر العادى -> المنطق القائم (مخزون، ضريبة، قيود)
  -> AI_Drafts.Status=Consumed + رقم المستند الناتج (للتدقيق)
```

- **الفواتير:** تعبئة `SalesInvoicePage` / `PurchaseInvoicePage` من `AI_Drafts` على غرار `?fromOrder=` الموجود. تمر البنود عبر نفس مسار إضافة البند (`OnItemDialogSaved`) كى تُحسب الضريبة والأسعار بمنطق الصفحة لا بمنطق النموذج.
- **إذن المخزن:** الخيار المفضّل إنشاء مسودة حقيقية عبر `StoreOrderService.SaveDraft` ثم فتح `/store/order/{type}?id=`، لأن دورة مسودة→ترحيل موجودة فعلاً. **[يحتاج تحقق فى P5]:** أن `SaveDraft` لا يحرّك المخزون؛ وإلا نستخدم `AI_Drafts` كما فى الفواتير.
- **مطابقة الكيانات:** تطبيع عربى (أ/إ/آ، ة/ه، ى/ي، التشكيل) + تشابه نصى + باركود/كود الصنف. الصنف غير الموجود لا يُنشأ؛ يُعرض كبند «غير مطابق» يختاره المستخدم.
- **منع التكرار:** فحص رقم فاتورة المورد + المورد + التاريخ قبل إنشاء مسودة مشتريات، مع تحذير.
- **`ask_user`:** الحلقة تتوقف وتُحفظ الحالة فى `AI_Messages`؛ الواجهة تعرض السؤال مع أزرار خيارات وحقل حر؛ الإجابة تعود كـ tool result. يعمل أيضاً بعد إعادة تحميل الصفحة.

---

## 7) تصميم الواجهة

كل شاشة جديدة تُصمَّم بمهارة `ui-ux-pro-max` (MudBlazor، متجاوبة، فاتح/داكن، RTL/LTR، أربع لغات عبر `Resources/SharedUIAssemblyMarker.*.resx`).

### 7.1 صفحة المساعد `/assistant` — `Pages/Assistant/AssistantPage.razor`
- **عمود جانبى (280px، يُطوى على الموبايل):** زر «محادثة جديدة»، بحث، قائمة المحادثات مجمّعة (اليوم/هذا الأسبوع/أقدم)، قسم «تقاريرى المثبّتة».
- **المنطقة الرئيسية:** شريط علوى (عنوان المحادثة، شارة النموذج الحالى، عدّاد الاستخدام). عند الفراغ: بطاقات اقتراح (مبيعات اليوم، أكبر 10 عملاء مديونية، أصناف تحت حد الطلب، إدخال فاتورة مشتريات من صورة).
- **الرسائل:** بثّ نصى حىّ؛ مؤشرات خطوات قابلة للطى («يبحث فى المخطط»، «ينفّذ استعلاماً» مع عرض الـ SQL لمن له صلاحية)؛ بطاقات نتيجة (تقرير، فتح شاشة، مسودة، سؤال).
- **شريط الإدخال:** حقل متعدد الأسطر، إرفاق (`MudFileUpload` كما فى `ItemOpeningBalancesPage`) + سحب وإفلات + لصق صورة، زر إيقاف أثناء التوليد.
- **لوحة التقرير:** على الشاشات العريضة يُفتح التقرير فى لوحة جانبية قابلة للتوسيع (نمط artifact)، وعلى الموبايل ملء الشاشة.

### 7.2 النافذة العائمة — `Components/Assistant/AssistantWidget.razor` داخل `MainLayoutMud.razor`
- FAB دائرى 56px بأيقونة واضحة، `position:fixed` أسفل اليسار أو اليمين حسب التفضيل، فوق المحتوى وتحت الحوارات، مع شارة عند وجود رد جديد.
- عند النقر: نافذة 400×600 (ملء الشاشة تحت 600px) بنفس `AssistantChat`، وأزرار: تصغير، فتح فى الصفحة الكاملة، محادثة جديدة.
- نفس خدمة الجلسة Scoped، فالمحادثة واحدة بين النافذة والصفحة. يُخفى الـ FAB فى صفحة `/assistant` وفى صفحات الدخول (`AuthLayout`) ولمن لا يملك الصلاحية.
- يُمرَّر سياق الشاشة الحالية (المسار وعنوانها) للمساعد ليفهم «هذه الفاتورة» أو «هذا العميل».

### 7.3 تفضيلات المستخدم
- `UserSettingKeys` (فى `AccountModelsLib/Models/Config/UserSettings/UserSettingsCLS.cs`): إضافة `ShowAiAssistantIcon` (افتراضى true) و`AiAssistantIconPosition` (`start`/`end`؛ منطقى لا يسار/يمين ليتوافق مع RTL، مع تسمية واضحة فى الواجهة).
- `UserPreferencesService`: خاصيتان + `SetShowAiAssistantIcon` / `SetAiAssistantIconPosition` بنفس النمط.
- `UserSettingsMenu.razor`: قسم «المساعد الذكى»: `MudSwitch` للإظهار + اختيار الموضع.

### 7.4 شاشة إعدادات الذكاء الاصطناعى `/settings/ai` — `Pages/Settings/AiSettingsPage.razor`
تبويبات:
1. **المزوّدون:** بطاقات (Anthropic، OpenAI، Google Gemini، Azure OpenAI، Ollama، LM Studio / OpenAI-compatible مخصص). لكل بطاقة: حالة، مفتاح API مقنّع، Base URL (والـ deployment لـ Azure)، زر «اختبار الاتصال»، زر «جلب النماذج» من endpoint النماذج، مفتاح تفعيل.
   - **«مزوّد مخصص» بالـ API + URL (أُضيف 2026-11-13 بطلب المستخدم):** زر «إضافة مزوّد مخصص» يسمح بربط **أى** خدمة ذكاء اصطناعى (عالمية جديدة، أو خادم داخل الشركة، أو بوابة مثل OpenRouter / LiteLLM / Groq / DeepSeek / Mistral / خوادم vLLM) بإدخال:
     - **الاسم** الظاهر للمزوّد.
     - **عنوان الـ URL** (Base URL)، مثل `https://api.example.com/v1` أو `http://192.168.1.50:8000/v1`.
     - **مفتاح الـ API** (اختيارى للخوادم المحلية)، مشفّر ومقنّع كباقى المفاتيح.
     - **البروتوكول:** OpenAI-compatible (الافتراضى، `/chat/completions`) أو Anthropic-compatible (`/v1/messages`) أو Gemini-compatible.
     - **طريقة إرسال المفتاح:** `Authorization: Bearer` (افتراضى) أو اسم Header مخصص (مثل `x-api-key` أو `api-key`)، مع **Headers إضافية** اختيارية (مشفّرة).
     - **النماذج:** «جلب النماذج» من `/models` إن كانت الخدمة تدعمه، وإلا إدخال معرّف النموذج يدوياً، مع تحديد قدراته (أدوات، رؤية، embeddings) وأسعاره لحساب التكلفة.
     - **مهلة الاتصال** وعلامة «محلى» (فتُسمح فى وضع «محلى فقط» إن كان العنوان داخل الشبكة).
     - زر **«اختبار الاتصال»** يرسل طلباً قصيراً ويعرض النتيجة والزمن ورسالة الخطأ الواضحة (عنوان خاطئ، مفتاح مرفوض، نموذج غير موجود).
     - يمكن إضافة أكثر من مزوّد مخصص، ويظهر كل واحد كبطاقة مستقلة ويُختار فى «النماذج والمهام» كأى مزوّد.
     - أمان: يُقبل `https` فقط للعناوين الخارجية (`http` مسموح للشبكة المحلية فقط)، ولا يُرسل المفتاح إلا للعنوان المسجّل نفسه.
2. **النماذج والمهام:** جدول ربط مهمة ← نموذج: محادثة/وكيل، استدلال عميق، استخراج/رؤية، سريع/رخيص (عناوين وتصنيف)، embeddings؛ مع ترتيب البدائل (fallback) و`max tokens` ومستوى الجهد. **حقول العيّنة (temperature) تظهر حسب قدرات النموذج فقط:** نماذج Claude الحالية ترفض `temperature` و`budget_tokens` وفرض `tool_choice` بخطأ 400، ويُتحكم فيها بـ `effort`.
3. **الخصوصية والحدود:** وضع محلى فقط، تقليل البيانات، حدود توكنز لكل مستخدم/دور، مدة الاحتفاظ بالمحادثات.
4. **الصلاحيات:** من يستخدم المساعد وأى مجموعات أدوات (قراءة/مسودات/استخراج) — مبنية فوق صلاحيات القوائم.
5. **الاستخدام والتكلفة:** لوحة (توكنز وتكلفة تقديرية حسب المستخدم/النموذج/اليوم، أكثر الأدوات استخداماً، نسبة الأخطاء).
6. **المعرفة:** إعادة بناء القاموس، تحرير المسرد والأمثلة، تشغيل مجموعة التقييم.

---

## 8) تغييرات قاعدة البيانات (جداول جديدة فقط)

تُنشأ فى ملف جديد `AccountModelsLib/AlterDB_CLS/AiAssistantSchemaCLS.cs` بنمط `SalesRepSchemaCLS.EnsureSchema()` (`IF NOT EXISTS`، إدراج فقط)، ويُستدعى من `Program.cs` للويب بجوار الاستدعاءات الحالية. المعرّفات بـ `CallSnowflakeIdGenerate.GenerateNewId()` كالمتبع.

| الجدول | الأعمدة الأساسية |
|---|---|
| `AI_Providers` | `Id`, `ProviderType`, `DisplayName`, `BaseUrl`, `ApiKeyEnc`, `ExtraJson` (deployment/api-version), `ApiProtocol` (openai/anthropic/gemini — للمزوّد المخصص), `AuthHeaderName` (nullable = Bearer), `ExtraHeadersEnc`, `TimeoutSeconds`, `IsCustom`, `IsEnabled`, `IsLocal`, `CreatedBy/At`, `UpdatedBy/At` |
| `AI_Models` | `Id`, `Provider_Id`, `ModelId`, `DisplayName`, `SupportsTools`, `SupportsVision`, `SupportsTemperature`, `ContextTokens`, `InputPricePerM`, `OutputPricePerM`, `IsEnabled` |
| `AI_TaskRouting` | `Id`, `TaskKey` (chat/reasoning/vision/fast/embedding), `Model_Id`, `Priority` (للـ fallback), `Effort`, `MaxTokens`, `Temperature` (nullable) |
| `AI_Settings` | `SettingKey`, `SettingValue` (LocalOnly, DataMinimization, RetentionDays, حدود افتراضية) |
| `AI_UserLimits` | `Id`, `User_Id` (nullable = افتراضى), `DailyTokens`, `MonthlyTokens`, `AllowDrafts`, `AllowUploads` |
| `AI_Conversations` | `Id`, `User_Id`, `Branch_Id`, `Title`, `Status` (Active/Archived), `CreatedAt`, `LastMessageAt` |
| `AI_Messages` | `Id`, `Conversation_Id`, `Seq`, `Role`, `ContentJson` (كتل النص/الأدوات كما أعادها المزوّد)، `Model_Id`, `InputTokens`, `OutputTokens`, `CacheReadTokens`, `CostEstimate`, `CreatedAt` |
| `AI_ToolCalls_Audit` | `Id`, `Message_Id`, `User_Id`, `ToolName`, `ArgsJson`, `SqlText`, `RowCount`, `DurationMs`, `Outcome` (Ok/Denied/Error), `ErrorText`, `CreatedAt` |
| `AI_Attachments` | `Id`, `Conversation_Id`, `FileName`, `MimeType`, `SizeBytes`, `StoragePath`, `Sha256`, `CreatedAt` |
| `AI_Drafts` | `Id`, `Conversation_Id`, `User_Id`, `Branch_Id`, `DocType`, `PayloadJson`, `WarningsJson`, `Status` (Pending/Consumed/Discarded/Expired), `ResultDocId`, `CreatedAt`, `ConsumedAt` |
| `AI_SavedReports` | `Id`, `User_Id`, `Title`, `SpecJson`, `QueriesJson`, `IsPinned`, `IsShared`, `CreatedAt`, `LastRunAt` |
| `AI_Knowledge` | `Id`, `Kind`, `KeyName`, `TitleAr`, `Content`, `Embedding` (VARBINARY nullable), `Source`, `Version`, `IsActive` |
| `AI_EvalCases` / `AI_EvalRuns` | سؤال، SQL/نتيجة متوقعة، وسوم؛ وتشغيلات مع النموذج والنتيجة |

- لا مفاتيح أجنبية على جداول النظام القائمة (مرجع منطقى بـ `User_Id` فقط)، فلا تتأثر الجداول القديمة.
- **Schema `ai` والـ Views:** تُنشأ فى نفس الملف بـ `CREATE OR ALTER VIEW` (كائنات جديدة فقط).
- **القوائم:** `menuItemAiAssistant` («المساعد الذكى» → `/assistant`) كعنصر رئيسى، و`menuItemAiSettings` («إعدادات الذكاء الاصطناعى» → `/settings/ai`) تحت الإعدادات للمديرين فقط — بنمط `HelpMenu`/`InventoryValueMenu`. **[يحتاج تحقق فى P0]:** قيمة `Parent_Id` و`PROJECT_ID` للعناصر الجذرية، وكيف يظهر عنصر بـ `RazorPath` فقط فى قائمة الديسك توب (سابقة `/help` موجودة فنتبعها).
- تسجيل المسارين فى `KnownRoutes.cs` و`RoutePageRegistry.cs` وأيقونة فى `MenuIcons.cs`.

---

## 9) خارطة المراحل (كل مرحلة = مهمة واحدة)

اختبارات المستخدم تُضاف إلى `.claude/docs/master-user-test-plan.md` بدءاً من م-351.

### P0 — الأساسات والمزوّدون والإعدادات
- **النطاق:** مشروع `Razors/AccountERP.AI`؛ `AiAssistantSchemaCLS`؛ DAL بنمط CLS (`AccountModelsLib/BusinessLibShared/Ai/AiProvidersCLS.cs` ...)؛ `ProviderRegistry` + Adapters؛ تشفير المفاتيح؛ `AiSettingsPage` (تبويبا المزوّدين والنماذج)؛ عنصرا القائمة.
- **ملفات:** جديد: المشروع، `AiAssistantSchemaCLS.cs`، `Pages/Settings/AiSettingsPage.razor`. تعديل: `Program.cs`، `AccountERP.Web.csproj`، `AccountsNet.sln`، `KnownRoutes.cs`، `RoutePageRegistry.cs`، ملفات resx.
- **القبول:** اختبار اتصال ناجح مع مزوّد سحابى ومحلى؛ المفتاح لا يظهر فى القاعدة نصاً ولا فى السجلات؛ إعادة التشغيل لا تكرر صفوفاً.
- **اختبارات:** م-351 «ظهور شاشة إعدادات الذكاء الاصطناعى للمدير فقط»؛ م-352 «إضافة مزوّد واختبار الاتصال»؛ م-353 «المفتاح محفوظ مشفّراً ويظهر مقنّعاً»؛ م-354 «ربط نموذج محلى Ollama وجلب قائمة النماذج»؛ م-382 «إضافة مزوّد مخصص بالـ API والـ URL واختبار الاتصال واستخدامه فى المحادثة».
- **مخاطر:** ثبات مفاتيح Data Protection؛ اختلاف واجهات المزوّدين (تُعزل داخل الـ Adapter).
- ✅ **تم التنفيذ 2026-11-13 (Opus 5.5):**
  - مشروع `Razors/AccountERP.AI` (net10.0، حزمة `Anthropic` 13.8.0، يرجع له الويب فقط، مضاف للحل تحت Razors): `AiKeyProtector` (purpose `AI.ProviderKeys`، المفتاح مشفّر + آخر 4 أحرف للعرض)، `AiUrlPolicy` (https للخارجى، http للشبكة المحلية فقط، رفض userinfo)، Adapters: Anthropic عبر الـ SDK الرسمى (`Models.List` كاختبار بلا توكنز)، OpenAI-compatible (OpenAI/LM Studio/مخصص)، Gemini، Azure، Ollama؛ `AiHttp` بلا Redirect ورسائل أخطاء عربية مع حذف المفتاح من أى رسالة؛ `AiAdminGateway` + `AddAccountErpAi`.
  - `AccountModelsLib`: `AlterDB_CLS/AiAssistantSchemaCLS.cs` (جداول **P0 فقط**: AI_Providers / AI_Models / AI_TaskRouting / AI_Settings + بذور الإعدادات + عنصر القائمة)، `BusinessLibShared/Ai/AiModels.cs` (DTOs + `IAiAdminGateway`)، `BusinessLibShared/Ai/AiProvidersCLS.cs` (DAL). باقى جداول §8 تُنشأ فى مراحلها.
  - شاشة `SharedUI/Pages/Settings/AiSettingsPage.razor` (`/settings/ai`): تبويب المزوّدين (بطاقات + تعديل + اختبار تلقائى بعد الحفظ + جلب النماذج وإضافة المحدد) وتبويب النماذج والمهام (قدرات وأسعار لكل نموذج + ربط مهمة ← نموذج أساسى وبدائل، temperature تظهر وتُحفظ فقط لنموذج يدعمها). 114 مفتاح ترجمة `AI_*`.
  - **قرارات أثناء التنفيذ:** (1) عنصر القائمة `menuItemAiSettings` بجوار «بيانات المعدلات الضريبية» تحت «ملف»، للمديرين فقط؛ عنصر «المساعد الذكى» (`/assistant`) **يُضاف فى P1** مع الصفحة نفسها حتى لا يظهر عنصر بلا شاشة. (2) `KnownRoutes`/`RoutePageRegistry` لا تحتاج تعديلاً (تُبنى بالـ Reflection)، والأيقونة من كلمة «إعدادات» تلقائياً. (3) علامة «محلى» تُحسب من العنوان لا من اختيار المستخدم حتى لا يتجاوز مزوّد سحابى وضع «محلى فقط». (4) `DataProtection:KeysPath` اختيارى فى appsettings؛ بدونه يبقى مسار المفاتيح الافتراضى كما هو (لا يُبطل كوكى الدخول الحالى) — **يجب ضبطه عند النشر على IIS**.
  - **تحقق:** بناء الويب ناجح؛ الجداول والقائمة أُنشئت على ClientNew؛ اختبار خدمة (33/33) شمل: عدم التكرار عند إعادة EnsureSchema، تشفير المفتاح والـ Headers، القناع، Bearer + Header إضافى، 401 برسالة عربية، عدم اتباع التحويل، عنوان لا يرد، Ollama، Anthropic بمفتاح وهمى (401 حقيقى)، ربط المهام وإسقاط temperature؛ وحُذفت صفوف الاختبار. اختبارات المستخدم م-351…م-354، م-382 فى `master-user-test-plan.md` (القسم ٦ط).

### P0.1 — الربط بتسجيل الدخول لدى المزوّد (طلب المستخدم 2026-11-13، خطة Fable 5.2)
- **الهدف:** طريقة ربط ثانية فى `/settings/ai` بجانب «مفتاح API»: يفتح المستخدم صفحة تسجيل الدخول لدى المزوّد فى تبويب جديد، يسجّل دخوله، فيرتبط النظام تلقائياً. طريقة المفتاح والـ URL تبقى كما هى.
- **حقائق تحقّقت منها (توثيق المزوّدين 2026-11-13):** OpenRouter يدعم OAuth PKCE بلا تسجيل تطبيق (`https://openrouter.ai/auth` ← `POST https://openrouter.ai/api/v1/auth/keys` يعيد `key`، ووضع «الكود» بلا `callback_url`). Gemini وAzure يدعمان OAuth لكن يلزم كل عميل إنشاء OAuth Client خاص به وعنوان رجوع https. Anthropic وOpenAI لا يتيحان OAuth لتطبيقات الطرف الثالث (واستخدام معرّف عميل Anthropic الخاص بأدواته انتحال غير مسموح، واشتراكات Claude الاستهلاكية لا تُستخدم لتشغيل تطبيقات أخرى).
- **النطاق (نسخة صغيرة وصادقة):**
  1. **OpenRouter — ربط حقيقى (OAuth PKCE S256):** نوع مزوّد جاهز جديد `openrouter` (بروتوكول OpenAI، `https://openrouter.ai/api/v1`) يتيح نماذج Claude/GPT/Gemini بحساب واحد. الناتج مفتاح API عادى يُحفظ مشفّراً فى `ApiKeyEnc`.
  2. **Anthropic / OpenAI / Gemini / Azure — ربط مُساعَد:** زر «افتح صفحة المفاتيح لدى المزوّد» يفتح لوحة المزوّد فى تبويب جديد، ينشئ المستخدم مفتاحاً ويلصقه. نص الشاشة يوضّح أن هؤلاء لا يتيحون ربطاً مباشراً ويشير إلى OpenRouter.
  3. **مؤجّل إلى P0.2 (عند طلب فعلى):** OAuth لـ Google/Azure Entra وOAuth2 العام (تسجيل تطبيق، refresh token، انتهاء صلاحية، ClientSecretEnc/TokenEnc/RefreshTokenEnc/TokenExpiresAt).
- **القرارات:**
  | القرار | الاختيار | السبب |
  |---|---|---|
  | من يحصل على ربط حقيقى | OpenRouter فقط | الوحيد الذى يعمل «افتح صفحة الدخول ← مربوط» بلا إعداد مسبق |
  | الحالة المعلّقة | فى الذاكرة (`AiSignInStore` Singleton): 10 دقائق، مرة واحدة، 5 لكل مستخدم | سرّ مؤقت يختفى بإعادة التشغيل؛ لا جدول للـ verifier |
  | عنوان الرجوع | `{origin}/ai/signin/callback`، والـ origin من `NavigationManager.BaseUri` داخل الدائرة | لا نثق بـ Host الطلب عند الرجوع |
  | الشبكة المحلية http | وضع رجوع إن كان العنوان https أو localhost، وإلا وضع «الكود» (بلا `callback_url`) يلصقه المستخدم؛ حقل اللصق ظاهر دائماً | يعمل على `http://192.168.x.x:5256` أياً كانت قيود المزوّد. **[يحتاج تحقق]:** قبول OpenRouter لرجوع http على localhost |
  | فتح صفحة المزوّد | رابط حقيقى `<a target="_blank" rel="noopener noreferrer">` بعد تجهيز الرابط | `window.open` من الخادم يحجبه المتصفح؛ ودائرة Blazor تبقى حيّة |
  | تحديث الشاشة | استعلام كل ثانيتين حتى «تم» أو انتهاء المهلة | بسيط بلا أحداث بين الدوائر |
  | تحديث التوكن | لا يوجد فى P0.1 | مفتاح OpenRouter لا ينتهى |
  | عنوان التبديل | ثابت `https://openrouter.ai/api/v1/auth/keys` لا من BaseUrl الصف | لا يُرسل الكود والـ verifier لعنوان آخر |
  | إلغاء الربط | يمسح المفتاح محلياً فقط مع تنبيه بحذفه من openrouter.ai/settings/keys | صادق: لا نحذف المفتاح البعيد |
- **قاعدة البيانات (إضافة أعمدة إن لم توجد، بلا مساس بالصفوف):** `AI_Providers.AuthMethod nvarchar(20) NULL` (`apikey` / `signin`)، `SignInAt datetime NULL`، `SignInBy bigint NULL`.
- **الملفات:** جديد: `Razors/AccountERP.AI/SignIn/AiSignInStore.cs`، `SignIn/OpenRouterSignIn.cs`، `SignIn/AiSignInEndpoints.cs` (`GET /ai/signin/callback`). معدّل: `AiAssistantSchemaCLS.cs`، `AiModels.cs` (`openrouter`، DTOs `AiSignInStart/AiSignInStatus`، دوال `StartSignIn/GetSignInStatus/CompleteSignInWithCodeAsync/Disconnect`)، `AiProvidersCLS.cs`، `AiAdminGateway.cs`، `ProviderRegistry.cs`، `AiServiceCollectionExtensions.cs`، `Program.cs`، `AiSettingsPage.razor`، ملفات resx، `master-user-test-plan.md`.
- **الأمان:** نقطة الرجوع تتحقق من state (موجود، غير منتهٍ، يُستهلك ذرّياً)، وأن كوكى الدخول يخص نفس المستخدم الذى بدأ الربط، وأنه ما زال يملك صلاحية الشاشة؛ `StartSignIn` يرفض من لا يملك الصلاحية؛ الكود والـ verifier والمفتاح لا تُسجَّل ولا تصل للمتصفح؛ صفحة الرجوع HTML ثابتة بـ `Cache-Control: no-store` و`Referrer-Policy: no-referrer`؛ Rate limit على المسار.
- **القبول:** الربط بالمفتاح كما هو (م-352، م-382)؛ ربط OpenRouter ينتهى بمفتاح مشفّر مقنّع وشارة واختبار ناجح بلا كتابة مفتاح؛ يعمل على LAN http بوضع الكود؛ state مكرر/منتهٍ/من مستخدم آخر يُرفض؛ إعادة EnsureSchema لا تكرر الأعمدة.
- **اختبارات:** م-383 «ربط OpenRouter بتسجيل الدخول وظهور شارة مربوط»؛ م-384 «الربط بلصق الكود على الشبكة المحلية http»؛ م-385 «جلب النماذج واختبار الاتصال بعد الربط بتسجيل الدخول»؛ م-386 «فتح صفحة المفاتيح لدى Anthropic وOpenAI ثم لصق المفتاح»؛ م-387 «إلغاء الربط يمسح المفتاح»؛ م-388 «انتهاء مهلة الربط بعد 10 دقائق برسالة واضحة»؛ م-389 «مستخدم بلا صلاحية لا يبدأ الربط ولا يكمله»؛ م-390 «الربط بمفتاح API ما زال يعمل».
- **مخاطر:** قيود عنوان الرجوع (يعالجها وضع الكود)؛ إعادة تشغيل الخادم أثناء الربط (رسالة «أعد المحاولة»)؛ المفتاح يبقى فى حساب OpenRouter بعد إلغاء الربط (تنبيه)؛ تغيّر واجهة OpenRouter (معزولة فى ملف واحد).
- ✅ **تم التنفيذ 2026-11-13 (Opus 5.5):** `SignIn/AiSignInStore.cs` + `OpenRouterSignIn.cs` + `AiSignInService.cs` (إكمال مشترك بين الرجوع ولصق الكود، تحقق صلاحية بنفس قاعدة MenuRepository عبر `AiProvidersCLS.UserHasMenu`) + `AiSignInEndpoints.cs` (Rate limit بسياسة المندوبين الموجودة)؛ أعمدة AuthMethod/SignInAt/SignInBy؛ نوع `openrouter`؛ الشاشة: اختيار طريقة الربط، لوحة تسجيل الدخول (رابط حقيقى + استعلام كل ثانيتين + لصق الكود)، شارة «مربوط بتسجيل الدخول»، «إلغاء الربط»، وزر «افتح صفحة المفاتيح» للمزوّدين الآخرين. **تحقق:** بناء ناجح؛ اختبار خدمة 26/26 (الأعمدة وعدم التكرار، PKCE، الصلاحية، وضع الكود على LAN ووضع الرجوع على localhost/https، كود وهمى يرفضه OpenRouter فعلياً برسالة عربية بلا حفظ، الاستخدام مرة واحدة، مستخدم آخر، حد 5 طلبات، الحفظ وإلغاء الربط، رفض Anthropic، نقطة الرجوع بـ state مزيف على الخادم الحى ← صفحة فشل آمنة no-store). الربط الكامل بحساب OpenRouter حقيقى يختبره المستخدم (م-383…م-390).

### P1 — واجهة المحادثة والنافذة العائمة
- **النطاق:** `AssistantChat`, `AssistantPage`, `AssistantWidget`؛ `AssistantSessionService` (Scoped)؛ بثّ؛ حفظ المحادثات؛ تفضيلا الأيقونة؛ محادثة نصية بلا أدوات.
- **ملفات:** جديد: `Pages/Assistant/*`, `Components/Assistant/*`. تعديل: `MainLayoutMud.razor`، `UserSettingsMenu.razor`، `UserPreferencesService.cs`، `UserSettingsCLS.cs` (إضافة مفاتيح فقط).
- **القبول:** نفس المحادثة فى الصفحة والنافذة؛ إخفاء/نقل الأيقونة يُحفظ للحساب؛ RTL وداكن سليمان؛ زر الإيقاف يعمل.
- **اختبارات:** م-355 «فتح صفحة المساعد من القائمة الرئيسية»؛ م-356 «الأيقونة العائمة تفتح نافذة المحادثة»؛ م-357 «إخفاء الأيقونة وتغيير موضعها من إعدادات الحساب»؛ م-358 «استمرار المحادثة بين النافذة والصفحة وبعد إعادة التحميل»؛ م-359 «العرض على الموبايل والوضع الداكن».
- **مخاطر:** تعارض z-index مع الحوارات وTabHost؛ انقطاع الـ Circuit أثناء البث (الحالة محفوظة فى القاعدة فتُستكمل).
- ✅ **تم التنفيذ 2026-11-13 (Opus 5.5):**
  - **القاعدة:** `AiAssistantSchemaCLS` + جدولا `AI_Conversations` و`AI_Messages` (أعمدة §8 + `ModelName`/`Status` ok·stopped·error/`ErrorText`/`DurationMs`)، وعنصر القائمة `menuItemAiAssistant` («المساعد الذكى» ← `/assistant`) تحت «ملف» بنمط «دليل المحاسب». DAL: `AccountModelsLib/BusinessLibShared/Ai/AiChatCLS.cs` (كل استعلام مقيّد بـ User_Id صاحب المحادثة). `IAiChatGateway` + DTOs فى `AiModels.cs`.
  - **AccountERP.AI:** `StreamChatAsync` لكل Adapter — Claude عبر حزمة Anthropic الرسمية (`Messages.CreateStreaming`، التفكير على الافتراضى adaptive، `effort` من ربط المهام، prompt caching تلقائى، بلا temperature/budget_tokens، `stop_reason: refusal` مُعالج)، و`Providers/HttpChatStreaming.cs` لـ OpenAI-compatible/OpenRouter/LM Studio (SSE، `max_completion_tokens` لـ OpenAI)، Azure (deployment)، Gemini (`streamGenerateContent?alt=sse`، يتجاهل أجزاء التفكير)، Ollama (NDJSON). `Chat/AiChatGateway.cs`: النموذج من ربط مهمة «محادثة» بالترتيب مع البدائل (عند خطأ أو رفض قبل ظهور أى نص)، وضع «محلى فقط»، تاريخ مدموج الأدوار بحد 60 ألف حرف يستبعد ردود الخطأ، تعليمات نظام ثابتة (بلا اسم المستخدم — تقليل البيانات) + سياق الشاشة، حفظ الرسالتين مع التوكنز والتكلفة التقديرية من أسعار النموذج، الإيقاف يحفظ الجزء المكتوب بحالة stopped.
  - **الواجهة (SharedUI):** `Services/AssistantSessionService.cs` (Scoped، مسجّلة فى الويب فقط) تتشاركه الصفحة والنافذة؛ `Components/Assistant/AssistantChat.razor` (بث حى بتحديث كل 60ms، Enter يرسل وShift+Enter سطر، إيقاف، نسخ، اقتراحات، `role=status` واحد للإعلان بدل بث كل حرف)، `Pages/Assistant/AssistantPage.razor` (`/assistant`: محادثات مجمّعة اليوم/7 أيام/أقدم، بحث فى العناوين والمحتوى، إعادة تسمية، حذف بتأكيد، عمود جانبى منبثق تحت 900px)، `Components/Assistant/AssistantWidget.razor` داخل `MainLayoutMud` (FAB 56px، نافذة 400×600 وملء الشاشة تحت 600px، Escape يغلق، شارة رد جديد، يمرر مسار الشاشة وعنوانها بلا معاملات الاستعلام)، `AiMarkdown.cs` (Markdig 0.40.0: HTML الخام معطّل، روابط http/https/نسبية فقط فى تبويب جديد، الصور تتحول لروابط)، `wwwroot/js/assistant.js`. تفضيلا `ShowAiAssistantIcon`/`AiAssistantIconPosition` فى `UserSettingKeys` و`UserPreferencesService` وقسم فى `UserSettingsMenu`. 51 مفتاح ترجمة `AIC_*`.
  - **قرارات أثناء التنفيذ (الموصى به):** (1) صلاحية «المساعد الذكى» للمديرين افتراضياً والباقون من شاشة الصلاحيات — المحادثة تستهلك رصيد المزوّد. (2) اقتراحات الشاشة الفارغة أسئلة لا تحتاج بيانات (شرح/ضرائب/صياغة) لأن قراءة البيانات فى P2؛ تعليمات النظام تمنع اختراع أرقام. (3) البديل عند رفض السلامة يعتمد على ربط المهام نفسه (يعمل مع أى مزوّد) بدل `fallbacks` من جانب الخادم الخاصة بـ Anthropic. (4) لا temperature لـ Claude (الحزمة لا تعرضه والنماذج الحالية ترفضه). (5) الحذف حقيقى لمحادثات المستخدم نفسه فقط (جداول جديدة)؛ تطبيق مدة الاحتفاظ (365 يوماً) مؤجّل لمهمة تنظيف لاحقة. (6) عنوان المحادثة = أول 60 حرفاً من أول سؤال بلا استدعاء نموذج إضافى.
  - **تحقق:** بناء الويب ناجح؛ اختبار خدمة 38/38 بخادم مزوّدين وهمى محلى (بلا تكلفة): المخطط والقائمة مرة واحدة وصلاحيات المديرين، البث للبروتوكولات الأربعة والتوكنز، جسم طلب Claude (effort وcache وبلا temperature)، خطأ 500 برسالة عربية، الجاهزية بلا ربط، البديل عند الخطأ وعند الرفض، الرفض بلا بديل، التاريخ المتبادل واستبعاد الأخطاء، الإيقاف وحفظ الجزء، منع مستخدم آخر (قراءة/إرسال/حذف)، حدود الإدخال، البحث فى المحتوى العربى، إعادة التسمية والحذف؛ ثم حُذفت كل صفوف الاختبار. اختبار المنقّى 6/6 (HTML خام، javascript:/data:، صورة، روابط آمنة، جدول). اكتُشف وأُصلح أثناء الاختبار: `StopReason` من الحزمة يعود بعلامتى تنصيص فكان الرفض لا يُكتشف، والعربى كان يُحفظ مهرّباً `\uXXXX` فيفشل البحث. اختبارات المستخدم م-355…م-359، م-391…م-394 فى `master-user-test-plan.md`.

### P2 — أسئلة وأجوبة للقراءة فقط
- **النطاق:** Views `ai.*` (مجموعة أولى: مبيعات، مشتريات، مخزون، عملاء، موردون، أصناف)؛ `ReadOnlySqlExecutor`؛ `PermissionGate`؛ الأدوات `search_schema`, `run_readonly_query`, `find_entity` والأدوات المالية؛ القاموس والمسرد؛ التدقيق والحدود.
- **القبول:** أى محاولة DML/DDL تُرفض وتُسجَّل؛ أرقام ميزان المراجعة من المساعد = شاشة الميزان؛ مستخدم بلا صلاحية لا يحصل على بيانات التقرير.
- **اختبارات:** م-360 «سؤال عن مبيعات فترة ومطابقته بتقرير النظام»؛ م-361 «رصيد صنف فى مخزن محدد»؛ م-362 «رفض طلب تعديل أو حذف بيانات»؛ م-363 «مستخدم بلا صلاحية كشف حساب لا يرى أرصدة العملاء»؛ م-364 «ظهور الاستعلام المنفّذ فى سجل التدقيق».
- **مخاطر:** دقة Text-to-SQL على مخطط قديم (تُعالَج بالـ Views والأدوات الجاهزة والأمثلة)؛ ترميز Windows-1256 فى بعض البيانات **[يحتاج تحقق]**؛ أداء استعلامات ثقيلة (timeout وحد صفوف).
- ✅ **تم التنفيذ 2026-11-13 (Opus 5.5):**
  - **القاعدة:** `AccountModelsLib/AlterDB_CLS/AiReadViewsCLS.cs` — schema `ai` و15 View (`v_branches, v_stores, v_items, v_customers, v_suppliers, v_sales, v_sales_lines, v_sales_returns, v_sales_return_lines, v_purchases, v_purchase_lines, v_purchase_returns, v_purchase_return_lines, v_stock_balance`) بأسماء أعمدة واضحة، `CREATE OR ALTER` فقط (لا يمس جدولاً). المحذوف مستبعد كالشاشات (`Is_DeletedProcessed` / `Deleted`)، وViews المبيعات ومرتجعها تفلتر بـ `SESSION_CONTEXT('ai_tax')` (صلاحيتا 920/921 عبر `Common.PermissionToShowTaxInvoiceOrNotORAll` كالشاشات) — بلا سياق لا تعيد صفاً. لا أعمدة اتصال شخصية ولا أرصدة مخزّنة. جدول `AI_ToolCalls_Audit` + DAL `AiToolAuditCLS`. تحقق: عدد صفوف كل View = الجدول الأصلى (لا تكرار من الربط).
  - **AccountERP.AI:** `Tools/ReadOnlySqlExecutor.cs` (ScriptDom 180.140.0، `TSql160Parser`: SELECT واحد، allow-list لـ Views المستخدم، رفض INTO/EXEC/OPENROWSET·OPENQUERY·OPENDATASOURCE/الأسماء الثلاثية/دوال بمخطط/المتغيرات غير المعرّفة؛ معاملة تُلغى دائماً، 15 ثانية، 5000 صف، `ConnectionStrings:AiReadOnly` اختيارى + سكربت `docs/ai-readonly-login.sql`)، `Tools/AiSchemaCatalog.cs` (وصف الـ Views بالعربى + المسرد + القواعد + الأمثلة + ربط كل View بشاشاتها)، `Tools/AiToolRegistry.cs` (الأدوات: `search_schema`, `run_readonly_query`, `find_entity` (بمعاملات لا نص مدمج), `get_periods_and_branches`, `get_trial_balance`, `get_financial_statement` على `FinalAccountsService` نفسه). الأدوات فى كل Adapter: Claude (tool_use مع بث `input_json_delta` وإعادة كتل التفكير بتوقيعها)، OpenAI/OpenRouter/LM Studio/Azure (`tool_calls`)، Gemini (`functionCall`/`functionResponse` مع `thoughtSignature`)، Ollama. حلقة الأدوات فى `AiChatGateway` (حد `MaxToolRoundsPerMessage` = 12 من الإعدادات، لا بديل لنموذج آخر بعد تنفيذ أداة، التوكنز مجمّعة على الدورات، الإيقاف يحفظ الجزء).
  - **الواجهة:** تحت كل رد «مصادر البيانات (n)» (عنوان كل خطوة، الصفوف، نص الاستعلام، المرفوض بالأحمر) وسطر حالة أثناء التنفيذ («استعلام قراءة…»)؛ `AssistantSessionService` يمرر شاشات المستخدم المسموحة (RazorPath من قائمة الويب) فى `AiUserContext`؛ تبويب «سجل التدقيق» فى `/settings/ai` (فلترة بالنتيجة وبحث وعرض الاستعلام، وحالة حساب القراءة). نص الشاشة الفارغة واقتراحان صارا عن البيانات. 22 مفتاح ترجمة.
  - **قرارات أثناء التنفيذ (الموصى به):** (1) الصلاحية = شاشات قائمة الويب نفسها (كل View/أداة مربوطة بمسار شاشة، والمدير يرى الكل) بدل مفاتيح جديدة. (2) لا فلترة إضافية بالفرع فى الـ Views لأن شاشات القوائم لا تفلتر به؛ عمود `branch_id` متاح، والأدوات المالية تقبل فقط الفروع المسموحة للمستخدم (`GetBranches`). (3) المعرفة فى الكود (`AiSchemaCatalog`) بدل جدول `AI_Knowledge` — تُنقل للجدول مع البحث المتجهى فى P7. (4) بلا حساب قراءة مخصص يُستخدم الاتصال الرئيسى مع الطبقات البرمجية، وتبويب التدقيق يوضّح ذلك. (5) `datasetId` مؤجّل لـ P3 (يحتاجه رسم التقارير). (6) أرصدة العملاء/الموردين وكشف الحساب ليست فى هذه المرحلة (لا أرقام قد تخالف شاشات الأرصدة).
  - **تحقق:** بناء الويب ناجح (0 أخطاء)؛ اختبار خدمة 80/80 بخادم مزوّدين وهمى (بلا تكلفة): المخطط وتطابق القاموس مع أعمدة كل View، 34/34 استعلاماً خبيثاً مرفوضاً و5/5 مشروعة مقبولة، إجمالى المبيعات عبر الـ View = استعلام الشاشة، فلتر الضريبية، حد 5000، الصلاحيات (مستخدم مبيعات فقط لا يرى المشتريات ولا الأدوات المالية ولو طلبها)، find_entity مع نص حقن، الميزان وقائمة الدخل من الأداة = المحرك، حلقة الأدوات للبروتوكولات الأربعة (الأدوات مرسلة، النتيجة تعود، توقيع التفكير يُعاد، التدقيق مربوط بالرسالة، التوكنز مجمّعة)، رفض DELETE وتسجيله والبيانات سليمة، التوقف عند 12 دورة، الإيقاف أثناء الدورة؛ ثم حُذفت كل صفوف الاختبار وربط المهام الخاص بالمستخدم لم يُمس. اختبارات المستخدم م-360…م-364، م-396…م-400.

### P3 — رسم التقارير وفتح الشاشات
- **النطاق:** `ReportSpec` + `AiReportView`؛ `render_report`, `open_screen`؛ كتالوج الشاشات؛ طباعة، Excel، حفظ/تثبيت، إعادة تشغيل.
- **اختبارات:** م-365 «تقرير بجدول ورسم بيانى من طلب عربى»؛ م-366 «فتح ميزان المراجعة بالفترة المطلوبة من المحادثة»؛ م-367 «تصدير التقرير إلى Excel وطباعته»؛ م-368 «حفظ تقرير وإعادة تشغيله بفترة جديدة».
- **مخاطر:** شاشات لا تقبل باراميترات query (تُضاف تدريجياً).
- ✅ **تم التنفيذ 2026-11-13 (Opus 5.5):**
  - **البيانات على الخادم:** كل أداة بيانات (`run_readonly_query`, `get_trial_balance`, `get_financial_statement`) تعيد `dataset_id` أول ناتجها، والبيانات الكاملة (حتى 5000 صف) فى `Tools/AiDataset.cs` (`AiDatasetCache` Singleton، ساعتان، لكل مستخدم بياناته فقط). `run_readonly_query` يقبل `date_from`/`date_to` كمعاملين `@date_from`/`@date_to` (لا تواريخ داخل النص) فيمكن إعادة التشغيل بفترة أخرى. معرّفات بيانات الرد السابق تُلحق بتاريخ المحادثة فيرسم المساعد منها فى رسالة متابعة («ارسمها دائرى») بلا استعلام جديد.
  - **`render_report`** (`Tools/AiReportEngine.cs`): ReportSpec = عنوان، فترة، مؤشرات (sum/avg/min/max/count/first)، أقسام table/chart (bar/line/pie/stacked)/text مع ترتيب وtop وتنسيقات (مبلغ/كمية/عدد/نسبة/تاريخ/نص). المحرك يتحقق من كل dataset وعمود ويجمع الأخطاء فى رسالة واحدة، والأرقام تُحسب على الخادم؛ الدائرى حتى 5 شرائح + «أخرى» (ui-ux-pro-max)، الجدول حتى 1000 صف معروض. كل تقرير صف فى جدول جديد `AI_SavedReports` (spec + مصادر بأدواتها ومدخلاتها + لقطة بيانات حتى 2000 صف) عبر `AiReportsCLS`.
  - **`open_screen` + `find_screen`** (`Tools/AiScreenCatalog.cs`): كتالوج بباراميترات متحقق منها (ميزان المراجعة from/to/branch/mode/filter، قائمة الدخل، التشغيل والمتاجرة، المركز المالى، فاتورة/مرتجع مبيعات ومشتريات بالمعرّف، بيانات العميل، قوائم الفواتير، أرصدة المخازن) + أى شاشة من قائمة المستخدم بمسارها كما هو (بحث عربى مطبّع). الصلاحية = الشاشة فى قائمة المستخدم؛ لا روابط خارجية؛ لا تنقّل تلقائى (زر يضغطه المستخدم). `AiUserContext.AllowedScreens` (المسار + العنوان من القائمة).
  - **الشاشات:** شاشات ميزان المراجعة وقائمة الدخل والتشغيل والمتاجرة والمركز المالى تقبل `?from=&to=&branch=&run=1` (والميزان `filter=`) عبر `Services/FinalAccountsQuery.cs` — إضافة فقط، بدونها نفس السلوك.
  - **الواجهة:** بطاقات «عرض التقرير» و«افتح الشاشة» تحت الرد (`AssistantChat`)؛ `AiReportView` (مؤشرات، جداول برأس ثابت وسطر إجمالى، رسوم SVG `AiChart` بلا JS بأنماط خطوط مختلفة وقيم فى tooltip وجدول بيانات بديل، مصادر البيانات)، شريط: فترة أخرى/تحديث، حفظ/تثبيت/إزالة، Excel (`Services/AiReportExcel.cs` بـ MiniExcel)، طباعة (printElement على ورق أبيض حتى فى الداكن)، فتح فى صفحة. حوار `AiReportDialog` وصفحة `/assistant/report/{id}`، و«تقاريرى» فى عمود `/assistant`. 41 مفتاح ترجمة × 4 لغات.
  - **إعادة التشغيل:** `RerunReportAsync` ينفّذ كل مصدر بنفس الأداة ومدخلاتها (الفترة فقط تتغير) عبر نفس مسار الأدوات بصلاحيات المستخدم الحالية، ويُسجَّل فى التدقيق، بلا استدعاء نموذج. حذف المحادثة يحذف تقاريرها غير المحفوظة فقط.
  - **قرارات أثناء التنفيذ (الموصى به):** (1) الرسوم SVG خاصة بدل MudChart (واجهة MudChart 9.7 تغيرت، وSVG يُطبع كما هو ويعكس RTL). (2) التقرير يُعرض فى حوار عريض/ملء الشاشة على الموبايل + صفحة مستقلة، بدل لوحة جانبية داخل الصفحة (أبسط ويعمل من النافذة العائمة). (3) الطباعة PDF عبر المتصفح فى هذا الإصدار. (4) «مشاركة» التقارير (`IsShared`) مؤجلة: العمود موجود ولا واجهة له.
  - **تحقق:** بناء الويب ناجح؛ اختبار خدمة 61/61 بلا نموذج (scratchpad p3test): رفض spec غير صالح (عنوان، dataset، عمود، نوع، سلاسل الدائرى)، المؤشرات والترتيب والإجماليات و«أخرى»، تطابق لقطة JSON، روابط الشاشات ورفض الحقن والتواريخ الخاطئة، مؤشر التقرير = إجمالى المبيعات بالاستعلام، إعادة التشغيل بفترة أخرى = إجمالى الفترة الجديدة ومسجّلة فى التدقيق ومرفوضة لمستخدم فقد الصلاحية، ميزان المراجعة فى تقرير = إجمالى المحرك، عزل المستخدمين (تقرير/بيانات)، open_screen بلا صلاحية/رابط خارجى مرفوض، حذف المحادثة؛ ثم حُذفت صفوف الاختبار. اختبارات المستخدم م-365…م-368، م-401…م-406.

### P4 — قرارات المستخدم `ask_user`
- **النطاق:** إيقاف/استئناف الحلقة، بطاقة السؤال بخيارات، استمرار بعد إعادة التحميل.
- **اختبارات:** م-369 «المساعد يسأل عند تشابه أسماء عملاء ويكمل بعد الاختيار»؛ م-370 «السؤال المعلّق يبقى بعد إعادة تحميل الصفحة».
- ✅ **تم التنفيذ 2026-11-14 (Opus 5.5):**
  - **الأداة:** `ask_user` فى `AiToolRegistry` (متاحة لكل مستخدم): `question` (حتى 500 حرف)، `options[]` (حتى 6، كل خيار حتى 120، تُقصّ وتُزال المكررة)، `allow_free_text` (افتراضى true)، `multi`. مدخلات غير صالحة ⇒ خطأ للنموذج فيصحّح دون توقف. `AiToolResult.PauseForUser` + بطاقة `{type:"ask"}`.
  - **الإيقاف والاستئناف (`AiChatGateway`):** الحلقة صارت `RunAsync` مشتركة بين `SendAsync` و`AnswerAsync` الجديدة. عند ask_user: تُنفَّذ بقية أدوات نفس الدورة، أى سؤال ثانٍ فى الدورة يُرد بخطأ، وتُحفظ رسالة المساعد بحالة `awaiting` والحالة الكاملة (الأدوار بكتل التفكير وتوقيعاتها + نتائج الدورة + مكان نتيجة السؤال) فى عمود جديد `AI_Messages.PendingJson` (إضافة عمود لجدول المساعد فقط). الإجابة: تحقق (الخيارات من القائمة فقط، واحد إلا مع multi، نص حتى 2000)، إغلاق ذرّى `ClosePending` (UPDATE بشرط `Status = awaiting`) فلا تُستأنف مرتين، حفظها رسالة مستخدم، ثم استئناف نفس النموذج بالإجابة نتيجةً للأداة. إن تغيّر النموذج أو فُقدت الحالة: الإجابة تُرسل نصاً («إجابتى على سؤالك: …») والسؤال بخياراته فى التاريخ. رسالة جديدة أثناء سؤال معلّق تغلقه «تجاوزه المستخدم».
  - **الواجهة:** بطاقة «سؤال من المساعد» فى `AssistantChat` (خيار واحد يُرسل بالضغط، المتعدد بمربعات + «إرسال الإجابة»، خانة «إجابة أخرى»، بعد الإجابة «إجابتك: …» والخيار مميّز، أو «لم تُجب»)، شارة «بانتظار إجابتك»، خانة الرسالة تصبح إجابة حرة للسؤال المعلّق (`AssistantSessionService.PendingAsk/AnswerAsync`)، إعلان `role=status`، 11 مفتاح ترجمة × 4 لغات. بعد الإجابة تُعاد قراءة الرسائل من القاعدة (وكذلك عند «أُجيب بالفعل» من تبويب آخر).
  - **قرارات أثناء التنفيذ (الموصى به):** (1) الاستئناف الحقيقى بنتيجة الأداة كما فى أطر الوكلاء الحديثة (حالة تشغيل محفوظة)، مع رجوع نصى آمن. (2) الإجابة رسالة مستخدم ورد المساعد بعدها رسالة جديدة بتوكنزها (لا تعديل لرسالة سابقة إلا إغلاق بطاقتها). (3) النص الحر مقبول دائماً من خانة الرسالة حتى لو `allow_free_text=false` (الخانة متاحة دائماً). (4) لا مهلة لانتهاء السؤال؛ معرّفات البيانات (ساعتان) إن انتهت يعيد النموذج الاستعلام. (5) عدّاد دورات الأدوات يبدأ من جديد بعد كل إجابة (كل استئناف بفعل المستخدم).
  - **تحقق:** بناء الويب ناجح؛ اختبار خدمة 88/88 بمزوّد وهمى (scratchpad p4test): العمود مرة واحدة، تحقق مدخلات الأداة، لكل البروتوكولات الأربعة (توقف، حفظ الحالة، بقاء السؤال بعد إعادة القراءة، تدقيق، استئناف بنتيجة الأداة لا بالنص، إعادة توقيع التفكير لـ Claude ومعرّف الاستدعاء لـ OpenAI وthoughtSignature لـ Gemini، إغلاق البطاقة، رفض الإجابة الثانية)، رفض خيار غريب/خيارين/فارغ/طويل/مستخدم آخر/رسالة خطأ، خيار + نص، متعدد، إجابتان متزامنتان ⇒ واحدة، حالة مفقودة وتغيّر النموذج ⇒ نصاً، التجاوز برسالة جديدة، دورة مختلطة (نتيجتان بالترتيب)، سؤالان فى دورة، سؤال غير صالح، الإيقاف أثناء الاستئناف، محادثة محذوفة؛ ثم حُذفت صفوف الاختبار وربط المستخدم لم يُمس. اختبارات المستخدم م-369، م-370، م-407…م-411.

### P5 — إدخال المسودات (مبيعات / مشتريات / إذن مخزن)
- **النطاق:** `AI_Drafts`؛ أدوات المسودات الثلاث؛ تعبئة `SalesInvoicePage`/`PurchaseInvoicePage` بـ `?aiDraft=`؛ مسار إذن المخزن؛ شريط «مسودة من المساعد».
- **القبول:** لا صف فى جداول الفواتير ولا تغيير فى `Unit_Item.Full_Qua`/`D_Qua` قبل ضغط المستخدم «حفظ»؛ بعد الحفظ يتحرك المخزون فى الاتجاه الصحيح لكل مخزن؛ أعلام الضريبة صحيحة.
- **اختبارات:** م-371 «مسودة فاتورة مبيعات من طلب نصى ولا أثر على المخزون قبل الحفظ»؛ م-372 «حفظ المسودة ينقص رصيد المخزن بالكمية الصحيحة»؛ م-373 «مسودة فاتورة مشتريات ضريبية بضريبة على الصنف»؛ م-374 «إذن إضافة وإذن صرف من المحادثة»؛ م-375 «مستخدم بلا صلاحية فاتورة المبيعات لا يستطيع إنشاء مسودة».
- **مخاطر (الأعلى فى الخطة):** منطق الحفظ داخل صفحات كبيرة؛ التعبئة يجب أن تمر بمسار إضافة البند نفسه. تحقق من النسخ المتشابهة للصفحات قبل التعديل (قاعدة CLAUDE.md رقم 3).
- ✅ **تم التنفيذ 2026-11-14 (Opus 5.5):**
  - **القاعدة:** جدول جديد `AI_Drafts` (User_Id، المحادثة/الرسالة، DraftType = sales/purchase/store_order، PayloadJson، Status = Pending/Consumed/Discarded، Target_Doc_Id/No، ConsumedAt) فى `AiAssistantSchemaCLS`، وDAL `AccountModelsLib/BusinessLibShared/Ai/AiDraftsCLS.cs` (كل دالة مقيّدة بالمستخدم؛ `MarkConsumed` بشرط `Status = Pending` فلا تُستهلك مرتين؛ قراءات التحقق بمعاملات؛ لا كتابة إلا فى AI_Drafts). نموذج المحتوى `AiDraftPayload` (Newtonsoft ليبقى المكتبة متوافقة).
  - **الأدوات** (`Razors/AccountERP.AI/Tools/AiDraftTools.cs`): `create_sales_invoice_draft`, `create_purchase_invoice_draft`, `create_store_order_draft`. الصلاحية = شاشة المستند فى قائمة المستخدم (`/accounts/invoices/sales-invoice`، `/accounts/invoices/purchase-invoice`، `/store/order/{type}` لكل نوع على حدة؛ أذون الاستلام 1016/1017 مستبعدة كالشاشة). التحقق: العميل/المورد نشط، المخزن نشط وليس وهمياً، كل صنف = صف Unit_item فى المخزن ونشط؛ وإلا «غير مطابق» بسببه (لا يُنشأ صنف)، كميات غير سالبة وغير صفرية، حتى 200 بند، تاريخ yyyy-MM-dd فى حدود سنة. تحذيرات: الكمية أكبر من الرصيد (للمنصرف، إلا أصناف بلا رصيد)، التحويل بلا مستلم أو لنفس المخزن، التكلفة اليدوية. الأذون تدمج البند المكرر (الشاشة ترفض التكرار). `find_entity` متاح لمن يملك شاشة مستند ولو بلا شاشة القوائم (العميل للمبيعات، المورد للمشتريات، الصنف والمخزن لأى مسودة). قسم «المسودات» فى تعليمات النظام يظهر فقط لمن يملك أداة.
  - **البطاقة:** كتلة `{type:"draft"}` ← `AiDraftCard` فى `AiChatMessage.Drafts`؛ الحالة تُقرأ من AI_Drafts عند تحميل المحادثة («لم تُحفظ بعد» بلون التحذير، «حُفظت برقم …» بالأخضر مع «عرض المستند»، «أُلغيت»)، التحذيرات تحت العنوان، وسطر «المساعد لا يحفظ المستندات». حذف المحادثة يلغى مسوداتها المعلّقة ويُبقى المستهلكة للتدقيق.
  - **الشاشات** (`?aiDraft=` لمستند جديد فقط، والمرتجعات مستبعدة): `SalesInvoicePage` (العميل عبر `OnCustomerPickedAsync` ويبقى قابلاً للتغيير، الضريبة عبر `ApplyTaxSettings`، السطر بنفس قيم `AddInvoiceItemDialog.OnItemPickedAsync` + `InvoiceLineCalculator.ComputeLine`، السعر = سعر المسودة أو سعر العميل `PriceListService.GetCustomerPrice` أو سعر الصنف)، `PurchaseInvoicePage` (المورد عبر `OnSupplierPickedAsync`، الضريبة عبر `OnTaxInvoiceChanged`، السطر كـ `AddBillItemDialog` + `BillLineCalculator.ComputeLine`، السعر = المسودة أو آخر سعر شراء)، `StoreOrderPage` (جهة المخزون بنوع المخزن/القسم المتاح، الجهة الأخرى إن كانت فى قائمتها، الأسطر عبر `StoreOrderPageService.GetLinesByIds` الجديدة بنفس أعمدة `SearchLines`). شريط مشترك `Components/Assistant/AiDraftBanner.razor` (غير المطابق + التحذيرات + «تجاهل المسودة»)، ومساعد `Services/AiDraftPageHelper.cs`. بعد الحفظ الناجح (الفاتورة، أو «حفظ مسودة»/«ترحيل» للإذن) ⇒ `MarkConsumed` برقم المستند؛ فتح مسودة مستهلكة يحوّل للمستند الناتج. 22 مفتاح ترجمة × 4 لغات.
  - **قرارات أثناء التنفيذ (الموصى به):** (1) إذن المخزن أيضاً عبر `AI_Drafts` لا `StoreOrderService.SaveDraft` — تحقّقنا أن SaveDraft لا يمس الرصيد، لكنه يكتب فى `Order_Purshing` ويحجز رقماً مسلسلاً قبل أن يضغط المستخدم، فتوحيد المسار للثلاثة يحقق «لا شىء قبل الحفظ» ويطابق ممارسة الأنظمة الحديثة (اقتراح مرحلى يعتمده المستخدم). (2) الأسطر تُبنى بنفس قيم وحسابات حوار الصنف (كمسار `?fromOrder=`) بدل فتح الحوار لكل بند. (3) نوع السعر من قوائم الأسعار (`Price_Type_Sell_Values`) لا يُطبَّق تلقائياً على أسطر المسودة؛ سعر العميل وسعر الصنف فقط، والمستخدم يعدّل. (4) فحص تكرار رقم فاتورة المورد مؤجّل لـ P6 (شاشة المشتريات لا حقل له حالياً). (5) لا مسودات للمرتجعات فى هذه المرحلة.
  - **تحقق:** بناء الويب ناجح (0 أخطاء ترجمة)؛ اختبار خدمة 54/54 بلا نموذج (scratchpad p5test): الجدول وإعادة التشغيل، ظهور الأدوات حسب الشاشات (مسؤول/مبيعات فقط/بلا صلاحية/نوع إذن واحد)، find_entity للعميل دون المورد لمستخدم المبيعات، رفض أداة المشتريات له، مسودة مبيعات (رابط، غير مطابق بسببين، تحذير الرصيد)، 14 مدخلاً خاطئاً مرفوضاً، مشتريات ضريبية بسعر، أذون 1002/1001/1004 (دمج، تحذيرات التحويل، التكلفة اليدوية، رفض 1004 لمستخدم 1002 ورفض 1016)، **لا صف جديد فى Invoices_List/Details وBills_List/Details وOrder_Purshing/Detailes_Purshing ورصيد Unit_item كما هو**، قراءة البطاقة ورفض رابط خارجى، الاستهلاك مرة واحدة ولمالكها فقط، الإلغاء لا يمس المستهلكة، `GetLinesByIds`؛ ثم حُذفت مسودات الاختبار. حفظ الفاتورة الفعلى وحركة المخزون بعده = اختبارات المستخدم م-371…م-375، م-412…م-417.

### P6 — استخراج المستندات والصور
- **النطاق:** الرفع والتخزين، `extract_document` بنموذج رؤية (صور، PDF، Excel)، ثقة لكل حقل، فحص التكرار، ربط بـ P5.
- **اختبارات:** م-376 «صورة فاتورة مورد تتحول إلى مسودة مشتريات»؛ م-377 «صنف غير معروف فى الصورة يُعرض للاختيار ولا يُنشأ»؛ م-378 «تحذير عند تكرار رقم فاتورة المورد»؛ م-379 «رفض ملف بحجم أو نوع غير مسموح».
- **مخاطر:** خط اليد والصور الرديئة والأرقام الهندية (٠-٩)؛ تُعرض الحقول ضعيفة الثقة مميّزة.
- ✅ **تم التنفيذ 2026-11-14 (Opus 5.5):**
  - **القاعدة:** جدول جديد `AI_Attachments` (المالك، المحادثة/الرسالة، الاسم، النوع الحقيقى، الحجم، Sha256، StoragePath نسبى، Status Active/Expired، آخر استخراج ExtractJson ونوعه ونموذجه وتوكنزه وتكلفته، Doc_No) وDAL `AiAttachmentsCLS`. أعمدة إضافية على جدول المساعد نفسه `AI_Drafts`: `Party_Id`, `Ref_No`, `Source_Attachment_Id` (لا تغيير على جداول النظام).
  - **التخزين** (`Razors/AccountERP.AI/Files/AiAttachmentStore.cs`): المجلد `AI:AttachmentsPath` وإلا `App_Data/ai-attachments/yyyy/MM/{id}.ext`؛ النوع من توقيع الملف (PNG/JPEG/WebP/PDF، وxlsx/csv بالامتداد + المحتوى) لا من اسم المتصفح؛ الحد `MaxUploadMb` (10) والصور حتى 5MB (حد نماذج الرؤية)، 5 ملفات للرسالة. الاحتفاظ `AttachmentRetentionDays` (90): الملف يُحذف من القرص والصف يبقى Expired (فحص كل 6 ساعات مع الرفع)، وحذف المحادثة يفعل نفس الشىء لملفاتها. المعاينة عبر تيار من الدائرة (blob) لا رابط عام.
  - **الاستخراج** (`Files/AiDocumentExtractor.cs` + `AiExtractionNormalizer.cs`): أداة `extract_document(attachment_id, doc_type)` تظهر فقط إن كان فى المحادثة مرفق، والمرفق لنفس المستخدم ونفس المحادثة. النموذج من ربط «استخراج/رؤية» ثم «محادثة» بشرط `SupportsVision` للصور/PDF (Excel/CSV نص جدولى بلا رؤية، وCSV بـ UTF-8 وإلا Windows-1256). الصورة/PDF تُرسل لنموذج الاستخراج فقط (كتلة `AiBlock.KMedia`: Anthropic image/document، OpenAI-compatible image_url/file، Gemini inlineData، Ollama images ولا PDF) ولا تدخل تاريخ المحادثة. الناتج يُطبَّع على الخادم: أرقام هندية وفواصل عربية، تواريخ يوم/شهر/سنة، ثقة لكل حقل (أقل من 0.75 = «راجع»)، وتحقق حسابى (الكمية × السعر، مجموع البنود، الإجمالى) يعلّم ما لا يتسق ولا يصحّحه. يُحفظ على صف المرفق فيُعاد بلا استدعاء نموذج لنفس النوع.
  - **التكرار (م-378):** نفس الملف (Sha256) ⇒ تحذير فى بطاقة الاستخراج. `create_purchase_invoice_draft` يقبل `supplier_invoice_no` و`source_attachment_id` و`doc_total`: الرقم يُكتب فى الملاحظات بالبادئة «فاتورة المورد رقم: » (فيبقى مع الفاتورة بعد الحفظ فى `Bills_List.Notes`)، ويُفحص: مسودات سابقة بنفس المورد والرقم، فواتير مشتريات غير محذوفة تحمل الرقم فى ملاحظاتها، أو نفس المورد واليوم والصافى. تحذير لا منع.
  - **الواجهة:** زر إرفاق 📎 فى مربع الكتابة (ولصق صورة مباشرة)، شرائح الملفات قبل الإرسال مع إزالة، الإرسال بملف بلا نص مسموح، شرائح الملفات فى رسالة المستخدم (ضغطها = معاينة)، بطاقة `AiExtractionView` («بيانات مستخرجة من …»: حقول الرأس وجدول البنود، الضعيف مميّز بلون + أيقونة + «راجع»، والتحذيرات)، وشريط المسودة يعرض الملف المصدر ورقم فاتورة المورد وإجمالى المستند. ترجمات ar/en/de/fr.
  - **قرارات أثناء التنفيذ (الموصى به):** (1) لا عمود جديد فى `Bills_List` لرقم فاتورة المورد (جداول جديدة فقط)؛ الرقم فى الملاحظات بالبادئة الثابتة، وكشف التكرار عليها + مسودات المساعد + نفس اليوم والإجمالى (ممارسة الأنظمة الحديثة: المورد + الرقم، وإلا التاريخ + المبلغ). (2) الصورة تذهب لنموذج الاستخراج لا لنموذج المحادثة (أرخص، ويعمل حتى لو نموذج المحادثة بلا رؤية، وإعادة الاستخدام بلا تكلفة). (3) الصور حتى 5MB. (4) التحذير لا يمنع الحفظ؛ القرار للمستخدم. (5) لا قراءة xls القديمة (xlsx وcsv فقط).
  - **تحقق:** بناء الويب ناجح (0 أخطاء)؛ اختبار خدمة 84/84 بمزوّد وهمى محلى (scratchpad p6test): التطبيع، فحص الأنواع والأحجام (م-379)، الرفع/الإزالة/الإرسال بلا نص/منع إعادة إرسال نفس المرفق، رسالة واضحة بلا نموذج رؤية، تدفق صورة ← استخراج ← مسودة مشتريات (م-376) والصنف المجهول غير مطابق ولا يُنشأ (م-377)، إعادة الاستخدام بلا استدعاء، تحذير نفس الملف ونفس رقم المورد (م-378)، شكل الصورة وPDF لكل البروتوكولات الأربعة، Excel وCSV عربى، لا أثر على المستندات والرصيد، حذف المحادثة يحذف الملفات. اختبارات المستخدم: م-376…م-379، م-418…م-423.

### P7 — تحسين المعرفة والتقييم
- **الخطة التفصيلية (2026-11-14):** `.claude/docs/ai-assistant-p7-plan.md` — 23 قراراً و7 أجزاء P7.1…P7.7 واختبارات م-424…م-443.
- **النطاق:** embeddings + بحث هجين، مجموعة الأسئلة الذهبية، شاشة تشغيل التقييم، تغذية راجعة (إعجاب/عدم إعجاب) تتحول لأمثلة بعد مراجعة.
- **اختبارات:** م-380 «تشغيل مجموعة التقييم وعرض نسبة النجاح»؛ م-381 «مقارنة نموذجين على نفس المجموعة».

### P7.8 — مدة الاحتفاظ بالمحادثات (أرشفة)
- **تم التنفيذ (2026-11-14):** قرار المستخدم (أُكّد اليوم): الأرشفة لا الحذف. `AiChatCLS.ArchiveInactive(days)` يجعل المحادثة النشطة التى لم تصلها رسالة منذ `ChatRetentionDays` (الافتراضى 365؛ 0 = بلا أرشفة تلقائية) «Archived» بلا حذف أى رسالة؛ تُطبَّق مع فتح قائمة المحادثات مرة كل 6 ساعات على الأكثر (`AiChatGateway.ArchiveSweep`). `GetConversations(…, archived)`، `SetConversationArchived` (صاحبها فقط)، القراءة والحذف للمؤرشفة مسموحان، والإرسال والإجابة مرفوضان برسالة «استعِدها أولاً». `AiChatAvailability.ChatRetentionDays` للعرض. واجهة /assistant: زر «المحادثات المؤرشفة»/«رجوع إلى المحادثات» مع تلميح المدة، «أرشفة»/«استعادة» فى قائمة كل محادثة، وشريط «هذه المحادثة مؤرشفة وللقراءة فقط» مع «استعادة» ومربع الكتابة معطّل. 9 مفاتيح (AIC_*Archiv*، AIC_BackToChats، AIC_NoArchived) × 4.
  - **تحقق:** بناء الويب بلا أخطاء؛ اختبار 25/25 (scratchpad archtest فى الجلسة ec38e1c3: محادثات اختبار مصطنعة القِدم فقط بعد التأكد أن لا محادثة حقيقية أقدم من 30 يوماً، الحد 365/300/400، بقاء الرسائل، القائمتان والبحث، رفض الإرسال، مستخدم آخر، استعادة/أرشفة يدوية/حذف، الأرشفة التلقائية وتقييدها بـ 6 ساعات، الإعداد 0/500/365، والمحادثات الحقيقية لم تتغير)؛ إعادة P7.7 53/53. اختبارات المستخدم م-446…م-448.

### P8 — مزايا «النظام الذكى» (كل ميزة مهمة فرعية مستقلة، انظر القسم 10).

**الترتيب والاعتماديات:** P0 → P1 → P2 → P3 → P4 → P5 → P6؛ P7 بالتوازى بعد P2.

---

## 10) خارطة «ERP ذكى» بعد المساعد

| # | الميزة | القيمة | الجهد | ملاحظات |
|---|---|---|---|---|
| 1 | الموجز اليومى للمدير (مبيعات، تحصيلات، سيولة، تنبيهات) عبر `NotificationCenterService` | عالية | منخفض | يعيد استخدام أدوات P2/P3؛ Worker مجدول مثل `SalesRepNotifyWorker` |
| 2 | بحث باللغة الطبيعية فى كل النظام (Ctrl+K: شاشات، عملاء، أصناف، مستندات) | عالية | منخفض | فوق `find_entity` وكتالوج الشاشات |
| 3 | شرح أخطاء الفاتورة الإلكترونية المصرية واقتراح التصحيح | عالية | منخفض-متوسط | قيمة محلية مباشرة |
| 4 | كشف الشذوذ فى القيود (مبالغ غير معتادة، تكرار، قيود خارج الدوام، حسابات نادرة) | عالية | متوسط | قواعد إحصائية أولاً ثم شرح بالنموذج؛ يكمل `EntriesIntegrityPage` |
| 5 | أولويات التحصيل (ترتيب العملاء حسب العمر والسلوك + مسودة رسالة تذكير) | عالية | متوسط | الإرسال بتأكيد المستخدم |
| 6 | إعادة الطلب الذكية وتوقع الطلب | عالية | متوسط-عالٍ | يكمل `StoreNeedsPage`؛ تنبؤ إحصائى والنموذج للشرح |
| 7 | توقع التدفق النقدى (13 أسبوعاً) | متوسطة-عالية | متوسط | من الاستحقاقات والأقساط والشيكات |
| 8 | تصنيف تلقائى (حساب/مركز تكلفة مقترح للمصروف، مجموعة للصنف الجديد) | متوسطة | منخفض-متوسط | اقتراح فقط، القرار للمستخدم |
| 9 | مساعد سياقى داخل كل شاشة («اشرح هذا الرصيد»، «لماذا لا يتزن؟») | متوسطة | منخفض | سياق الشاشة موجود من P1 |
| 10 | MCP Server يكشف أدوات القراءة لعملاء خارجيين (Claude Desktop وغيره) | متوسطة | متوسط | نفس `ToolRegistry` و`PermissionGate`؛ قراءة فقط أولاً بتوكن لكل مستخدم |
| 11 | إدخال صوتى بالعربية | متوسطة | متوسط | بعد استقرار P5 |
| 12 | مطابقة بنكية ذكية من كشف البنك (PDF/Excel) | عالية | عالٍ | تعتمد على P6 |

التنفيذ المقترح: 1 → 2 → 3 → 4 → 5 ثم الباقى حسب طلب العملاء.

---

## 11) توصيات النماذج والتكلفة والخصوصية

**نماذج Claude (أسعار API الرسمية لكل مليون توكن، من مرجع محدّث 2026-11-03):**

| النموذج | Model ID | إدخال / إخراج | الدور المقترح |
|---|---|---|---|
| Claude Opus 5.5 | `claude-opus-5-5` | $4 / $20 | **الوكيل الافتراضى** (استدلال، Text-to-SQL، مسودات) بجهد `medium` |
| Claude Sonnet 5.5 | `claude-sonnet-5-5` | $2 / $10 | بديل اقتصادى للوكيل إن أثبتت مجموعة التقييم كفايته؛ استخراج الفواتير |
| Claude Haiku 5.5 | `claude-haiku-5-5` | $0.10 / $0.50 | مهام سريعة: عناوين المحادثات، التصنيف، التطبيع |
| Claude Fable 5.2 | `claude-fable-5-2` | $8 / $40 | اختيارى للتحليلات الأثقل فقط؛ يتطلب احتفاظ بيانات 30 يوماً لدى المزوّد، فلا يناسب من يشترط عدم الاحتفاظ |

قيود تقنية يجب أن يراعيها الـ Adapter (تؤثر على شاشة الإعدادات):
- `temperature`/`top_p` و`budget_tokens` مرفوضة (400) على هذه النماذج؛ التحكم بـ `output_config.effort`، والتفكير adaptive.
- فرض `tool_choice` (`any`/`tool`) مرفوض على Opus 5.5 / Sonnet 5.5 / Fable 5.2؛ نستخدم `auto` + `strict: true`.
- يجب فحص `stop_reason` (ومنه `refusal`) قبل قراءة المحتوى.
- prompt caching للـ system prompt وتعريف الأدوات (ثابتان) يخفض التكلفة والزمن كثيراً.

**OpenAI / Gemini / Azure OpenAI:** مدعومة عبر نفس التجريد. لا أسمّى إصدارات محددة لأنها تتغير؛ قائمة النماذج وأسعارها تُجلب/تُدخل من شاشة الإعدادات. المطلوب من أى نموذج للوكيل: tool calling موثوق + مخرجات منظّمة؛ وللاستخراج: رؤية.

**المحلى (Ollama / LM Studio):** عائلات Qwen و Llama التى تدعم tool calling. توقع واقعى: جودة أقل فى Text-to-SQL والعربية والأدوات المتعددة، وتحتاج GPU مناسب (تقريباً 24GB VRAM فأكثر لنموذج محترم). مناسبة لـ: الخصوصية التامة، الأوفلاين، والمهام البسيطة. الحكم بمجموعة التقييم لا بالانطباع.

**العربية:** النماذج الكبرى الحالية جيدة فى الفصحى والعامية المصرية؛ نقاط الضعف المتوقعة: المصطلحات المحاسبية المحلية والأسماء المنقحرة فى المخطط (يعالجها المسرد)، والأرقام الهندية وخط اليد فى الصور (تعالجها درجات الثقة والمراجعة).

**تقدير تكلفة تقريبى (افتراضات: سؤال متوسط = 4 دورات أدوات، نحو 25 ألف توكن إدخال أغلبها من الكاش، ونحو 2000 توكن إخراج):**

| النموذج | تكلفة السؤال تقريباً | 20 مستخدماً × 30 سؤالاً × 22 يوماً |
|---|---|---|
| Opus 5.5 | نحو $0.06 | نحو $800 شهرياً |
| Sonnet 5.5 | نحو $0.03 | نحو $400 شهرياً |
| Haiku 5.5 | أقل من $0.005 | نحو $30–60 شهرياً |

استخراج صورة فاتورة واحدة: نحو $0.01–0.03. هذه تقديرات حسابية لا قياسات؛ تُستبدل بأرقام لوحة الاستخدام بعد P2.

**خيارات الخصوصية:** (أ) محلى فقط؛ (ب) سحابى مع تقليل البيانات (مخطط وإحصاءات فقط)؛ (ج) سحابى كامل. ويمكن التوجيه حسب المهمة (مثلاً الاستخراج سحابى والأسئلة المالية محلية).

---

## 12) قرارات مفتوحة للمستخدم (مع التوصية)

> ✅ **2026-11-13: وافق المستخدم على كل التوصيات الست كما هى** («موافق على التوصيات»). القرارات محسومة.

قرارات تحتاج اختيارك فعلاً:

| # | القرار | التوصية |
|---|---|---|
| 1 | هل يُسمح بإرسال بيانات المنشأة لمزوّد سحابى؟ | نعم مع «تقليل البيانات» افتراضياً، ووضع «محلى فقط» متاح لكل منشأة |
| 2 | النموذج الافتراضى للوكيل | `claude-opus-5-5`؛ والنزول إلى `claude-sonnet-5-5` فقط إن حافظ على نتيجة مجموعة التقييم |
| 3 | من يدفع ويملك مفتاح الـ API (أنت كمورّد أم كل عميل)؟ | كل عميل بمفتاحه (الجداول تدعم ذلك)، مع إمكانية مفتاح مركزى لاحقاً |
| 4 | إنشاء Login قراءة فقط على SQL Server عند كل عميل | نعم (سكربت نسلّمه)؛ البديل الأضعف: نفس الاتصال مع الاعتماد على الفاحص فقط — لا أنصح به |
| 5 | ظهور عنصر «المساعد الذكى» فى قائمة الديسك توب (الجدول مشترك) | يتبع سابقة `/help` الحالية؛ نتحقق من مظهره فى P0 ونقرر |
| 6 | مدة الاحتفاظ بالمحادثات والمرفقات | 12 شهراً للمحادثات، 90 يوماً للمرفقات، بالأرشفة لا الحذف |

قرارات حسمتها بالخيار الحديث (تُراجع إن اعترضت): تجريد `Microsoft.Extensions.AI` + Adapter رسمى لـ Claude؛ تقارير spec لا HTML خام؛ مسودات فقط للكتابة؛ بحث كلمات أولاً ثم embeddings فى الذاكرة؛ مشروع مستقل `AccountERP.AI`؛ الموضع المنطقى `start/end` للأيقونة.

---

## 13) استراتيجية التقييم والاختبار

- **المجموعة الذهبية (`AI_EvalCases`):** 60–100 سؤال عربى (فصحى وعامية مصرية) موزعة: مبيعات، مشتريات، مخزون، حسابات، عملاء/موردون، أسئلة غامضة يجب أن تولّد `ask_user`، وأسئلة يجب أن تُرفض (تعديل/حذف، بيانات مستخدمين). لكل حالة: الأداة المتوقعة، SQL مرجعى أو نتيجة متوقعة، ووسوم.
- **المقارنة:** بالنتيجة لا بنص الـ SQL (تنفيذ المرجعى والمولَّد على قاعدة اختبار ثابتة ومقارنة الجداول مع تسامح فى الترتيب والتقريب). الحالات المالية تُطابق مخرجات `FinalAccountsService`.
- **المقاييس:** نسبة النجاح، صحة اختيار الأداة، عدد الدورات، التوكنز، الزمن، التكلفة لكل حالة ناجحة.
- **الانحدار:** تشغيل إلزامى عند تغيير نموذج، أو system prompt، أو View، أو تعريف أداة؛ النتائج فى `AI_EvalRuns` وتُقارن من شاشة الإعدادات. التشغيل يستهلك رصيداً حقيقياً فيتم بموافقة المدير.
- **اختبارات الأمان (آلية، بلا نموذج):** مجموعة استعلامات خبيثة ضد `ReadOnlySqlExecutor` (DML، عبارات متعددة، `EXEC`, `INTO`, كائنات خارج القائمة، تعليقات مخادعة) يجب رفضها 100%؛ واختبار حقن عبر ملف مرفوع يحتوى تعليمات.
- **اختبارات الإدخال:** لكل نوع مستند: قراءة `Unit_Item.Full_Qua`/`D_Qua` قبل المسودة، بعد المسودة (بلا تغيير)، وبعد الحفظ (تغيير صحيح لكل مخزن) — وفق قاعدة CLAUDE.md رقم 12.
- **اختبارات المستخدم:** م-351 إلى م-381 أعلاه، تُضاف لكل مرحلة عند تنفيذها.

---

### Critical Files for Implementation
- `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects\Razors\AccountERP.Web\AccountERP.Web\Program.cs` — تسجيل خدمات المساعد واستدعاء `AiAssistantSchemaCLS.EnsureSchema()`
- `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects\AccountModelsLib\AlterDB_CLS\StockEngineSchemaCLS.cs` — النمط المرجعى لإضافة القوائم والجداول (`HelpMenu`, `InventoryValueMenu`) الذى يُحاكيه الملف الجديد `AiAssistantSchemaCLS.cs`
- `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects\Razors\AccountERP.SharedUI\Components\Layout\MainLayoutMud.razor` — تركيب `AssistantWidget` (الأيقونة العائمة)
- `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects\AccountModelsLib\Models\Config\UserSettings\UserSettingsCLS.cs` (مع `Razors\AccountERP.SharedUI\Services\UserPreferencesService.cs` و`Components\Shared\UserSettingsMenu.razor`) — تفضيلا إظهار الأيقونة وموضعها
- `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects\Razors\AccountERP.SharedUI\Pages\Invoices\SalesInvoicePage.razor` (ومثلها `PurchaseInvoicePage.razor` و`Pages\Store\StoreOrderPage.razor`) — تعبئة المسودة `?aiDraft=` عبر مسار الحفظ القائم
- `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects\AccountModelsLib\BusinessLibShared\GeneralLedger\FinalAccountsService.cs` و`LedgerBalanceEngine.cs` — أساس الأدوات المالية للقراءة
- `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects\ERP_V1_3.md` — مصدر القاموس الدلالى و Views الأعمال