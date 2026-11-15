## P8.8 — التصنيف التلقائى (اقتراح فقط) — خطة تفصيلية

كُتبت 2026-11-15 (Opus، وكلاء Fable متوقفون) اعتماداً على الكود بعد P8.9.5 (`eca5b25`). تحل محل مخطط P8.8 فى `ai-assistant-p8-plan.md`. القرارات مرقّمة ت ص1… (ص = صنّف) وكلها محسومة بالخيار الموصى به؛ لا يُعاد التفكير فيها إلا إذا نقضها فحص من §10. قرارات P8 (ق1…ق30) ملزمة.

> ملاحظات قبل التنفيذ:
> - **اقتراح فقط:** لا يُحفظ شىء فى بيانات النظام إلا بضغطة المستخدم على «حفظ» المعتاد للشاشة. الاقتراح يملأ الحقل فى الواجهة فقط.
> - **لا تعديل على بيانات قديمة:** لا UPDATE/DELETE على جداول النظام؛ الجديد جدول واحد `AI_Suggestions` + إعدادات فى `AI_Settings` + View قراءة واحد.
> - **ويب + MAUI للواجهة، والمحرك فى الخادم:** الشاشتان مشتركتان فى SharedUI؛ الخدمة تُسجّل فى الويب فقط (نمط P8.3: `GetService(...) is X s && s.IsAvailable`، null فى MAUI ⇒ لا شريحة).
> - `AccountsEInvoicesLIB` لا يُمس.

### حقائق من الكود

- **سند الصرف/القبض متعدد السطور:** `Pages/Accounts/CashVoucherPage.razor` (`/accounts/cash/voucher`). سطر الإدخال: `AccountSelector` (حساب/عميل/مورد ← `_subId`, `_accountType`, `_subName`) + `CostCenterField` (`AccountIds="_lineAcc"`, `PreferredId="_lastCcId"`, `CenterIdChanged`, `CenterChanged`, `AllowedChanged` ⇒ `_ccSet.Policy` 2 = إلزامى) + المبلغ + `_lineNotes`. «إضافة سطر» `AddLine()` سطر 268. الحفظ `SaveAsync` سطر 326 عبر `_svc.SaveMultiVoucher`.
- **الملاحظة قبل الحساب؟** لا: الترتيب الحالى حساب ← مركز ← مبلغ ← ملاحظة. الاقتراح يحتاج نصاً ⇒ الشريحة تظهر بعد كتابة ملاحظة السطر (أو البيان العام `_orderNotes`) وقبل اختيار حساب.
- **الصفحات الشبيهة:** `CashVoucherPartyPage.razor` (`/accounts/cash/voucher-party`)، `CashManyBoxPage.razor` (`/accounts/cash/many-boxes`). المرحلة الأولى على `CashVoucherPage` فقط (ق ص3).
- **البيانات التاريخية:** `Money_Transaction_Box` فيه `Notes`, `SubAccount_Id`, `Account_Name`, `Account_Code`, `Account_Type`, `Account_Cost_Center_Tree_Id`, `CostCenter_Name`, `Transaction_Type`. `ai.v_cash_vouchers` يعرض `notes` و`party_type/party_id/party_name` **بلا مركز التكلفة** ⇒ View جديد `ai.v_expense_history` (ق ص6).
- **الصنف:** `Pages/Items/ItemEditPage.razor` (`/items/new`, `/items/{Id}`، ملفات تعريف). `_f.Name` عبر `OnNameChanged`، `_f.Category_Id` (إلزامى، `MudSelect` + بحث F9 `CategorySearchDialog`)، `_f.Group_Id` (`_lk.Groups`، اختيارى). الافتراضى `Default_Category_Item/Service` و«ملف التعريف» يغيّر الفئة.
- **التضمين:** `Knowledge/AiEmbeddingService`: `CurrentRoute(out reason)`, `EmbedQueryAsync(text, ct)` (مهلة 3 ث، قاطع 5 د)، `EmbedDocumentsAsync(route, texts, ct)`, `Normalize/ToBytes/FromBytes/Dot`. نمط فهرس `AiKnowledgeIndex` (أعمدة `Embedding varbinary(max)`, `EmbeddingModel_Id`, `ContentHash`).
- **نموذج سريع:** `AiChatGateway.CandidatesFor(…, { Fast, Chat })` + شارح بلا أدوات (نمط `AiAnomalyExplainer`: مهلة 30 ث، `Effort="low"`).
- **مهمة دورية:** `IAiScheduledJob { Name; TickAsync(nowUtc, ct) }` يديرها `AiSchedulerWorker`.

### القرارات (ص)

| # | القرار | السبب |
|---|---|---|
| ص1 | الهدف فى هذه المرحلة: (أ) **حساب + مركز تكلفة لسطر سند الصرف** من نص الملاحظة، (ب) **فئة + مجموعة للصنف الجديد** من اسمه | أعلى تكرار إدخال وأوضح سوابق؛ ما يفعله Xero/QuickBooks/Odoo |
| ص2 | الترتيب: (1) **مطابقة سوابق دقيقة** (نفس النص بعد التطبيع) ← (2) **أقرب سوابق بالكلمات** (BM25 مبسط على الرموز) + **التضمين** إن وُجد مسار ← (3) **النموذج** فقط عند الغموض (أعلى نتيجتين متقاربتين < هامش 0.15) أو لا سوابق | السوابق مجانية وقابلة للشرح؛ النموذج للحالات الصعبة فقط |
| ص3 | الشاشات: `CashVoucherPage` (صرف وقبض) و`ItemEditPage` (جديد فقط، لا تعديل صنف موجود). الصفحات الشبيهة لاحقاً بنفس المكوّن | تقليل السطح الأول |
| ص4 | الواجهة: **شريحة اقتراح** `AiSuggestChip` تحت الحقل: «اقتراح: مصروفات نقل (٧٢%) · مركز: الإدارة» + زر «تطبيق» + «تجاهل» + «لماذا؟» (السوابق الثلاث). لا ملء تلقائى أبداً | قرار المستخدم؛ لا مفاجآت |
| ص5 | يُستدعى الاقتراح بعد توقف الكتابة 600 مللى ث وطول النص ≥ 3 أحرف، ولا يُستدعى إن اختار المستخدم الحساب يدوياً قبلها | تكلفة واستجابة |
| ص6 | View جديد للقراءة `ai.v_expense_history`: `line_id, voucher_date, transaction_type, notes, sub_account_id, account_code, account_name, account_type, cost_center_id, cost_center_name, branch_id, user_id` من `Money_Transaction_Box` (غير المحذوف، `Account_Type` حساب عام فقط — لا عملاء/موردين) | `v_cash_vouchers` بلا مركز تكلفة؛ View جديد أسلم من تعديل القديم |
| ص7 | فهرس السوابق فى الذاكرة لكل مستأجر: أحدث 24 شهراً، ≤ 50,000 سطر، يُبنى عند أول طلب ويُحدَّث كل 6 ساعات (كائن singleton `AiSuggestIndex`)؛ لا تخزين نصوص سوابق فى جدول جديد | لا تكرار للبيانات؛ حجم صغير |
| ص8 | التضمين (اختيارى): متجهات السوابق تُحفظ فى `AI_Suggestions_Vectors` (`Kind, TextHash, Text, Embedding, EmbeddingModel_Id, CreatedAt`) لنصوص **مميزة** فقط (dedupe بالهاش)، يبنيها job ليلى بحد 2,000 نص/دورة | النصوص مكررة كثيراً؛ تكلفة أقل |
| ص9 | الثقة = وزن السوابق (تكرار × حداثة: نصف عمر 180 يوماً) مطبَّع؛ < 0.35 ⇒ لا شريحة | تجنّب الضجيج |
| ص10 | الصلاحية: الحسابات المقترحة تُرشَّح بما يسمح به `AccountSelector` للمستخدم والفرع، والمراكز بـ `CostCenterService.AllowedSet` للحساب المقترح؛ اقتراح غير مسموح يُسقط | الاقتراح لا يتجاوز الصلاحيات |
| ص11 | النموذج يُعطى: النص + أعلى 8 مرشحين (كود/اسم) فقط ويُطلب منه اختيار واحد أو «لا أعرف»؛ لا يُعطى شجرة الحسابات كاملة؛ أى اختيار خارج المرشحين يُرفض | لا هلوسة حسابات |
| ص12 | تقليل البيانات (`DataMinimization=1`): يُرسل للنموذج نص الملاحظة بعد حذف الأرقام الطويلة (≥ 6 أرقام) وأسماء الأطراف | خصوصية |
| ص13 | جدول `AI_Suggestions` (سجل لا مصدر حقيقة): `Id, Kind ('expense'/'item'), User_Id, Branch_Id, InputText, InputHash, SuggestedJson, Source ('exact'/'neighbors'/'model'), Confidence, Outcome ('shown'/'applied'/'dismissed'/'overridden'), FinalJson, Screen, CreatedAt, DecidedAt` | قياس الدقة وتقرير «نسبة القبول» |
| ص14 | «overridden» = طبّق المستخدم اقتراحاً ثم غيّر الحساب قبل الحفظ، أو حفظ بحساب آخر؛ يُسجَّل عند حفظ الشاشة (استدعاء خفيف `RecordOutcome`) | تغذية راجعة حقيقية |
| ص15 | لا تعلّم آلى من `AI_Suggestions`: السوابق الحقيقية (المحفوظة فى السندات) هى مصدر التعلم الوحيد | البيانات الأصلية هى الحقيقة |
| ص16 | الصنف: المرشحون = فئات/مجموعات الأصناف ذات الأسماء الأقرب (كلمات + تضمين) مع احترام ملف التعريف (`EffectiveProfile`) و`Item_Or_Service`؛ الاقتراح يظهر بجانب الفئة ولا يغيّر ملف التعريف إلا إن طبّقه المستخدم | سلوك الصفحة الحالى يبقى |
| ص17 | الإعدادات (`AI_Settings`): `SuggestEnabled` (0 افتراضياً — تفعيل من تبويب)، `SuggestUseModel` (1)، `SuggestMinConfidence` (0.35)، `SuggestHistoryMonths` (24)، `SuggestMaxRows` (50000) | موقوف افتراضياً كباقى P8 |
| ص18 | تبويب إعدادات «التصنيف» فى `/settings/ai`: التفعيل، الحدود، حالة الفهرس (أسطر/نصوص مميزة/متجهات)، «إعادة البناء»، ونسبة القبول آخر 30 يوماً لكل نوع | قياس |
| ص19 | لا أداة محادثة جديدة فى هذه المرحلة | الميزة داخل الشاشات |
| ص20 | MAUI: لا شريحة (الخدمة غير مسجّلة) | نمط P8.3/P8.9 |

### 1) المحرك (`Razors/AccountERP.AI/Suggest/`)

- `AiSuggestText` (pure): تطبيع عربى (إزالة التشكيل والتطويل، أ/إ/آ ← ا، ى/ي، ة/ه، أرقام عربية ← لاتينية)، رموز (≥ 2 حرف، إسقاط كلمات وقف قليلة: «مصروف، مصاريف، دفع، سداد، قيمة، عن، الى، من، فى»)، `Hash`.
- `AiSuggestIndex` (singleton): `EnsureAsync(tenantKey)`، قاعدة لكل `Kind`: قائمة سوابق (`TextNorm, Tokens, TargetId, TargetName, CostCenterId, Date, BranchId`)، فهرس مقلوب رمز ← قائمة، `Version`، `BuiltAt`. حمل من `ai.v_expense_history` عبر `ReadOnlySqlExecutor` (جلسة المستخدم).
- `AiSuggestRanker` (pure): `Rank(tokens, index, now) → List<Candidate(TargetId, Score, Evidence[3])>`: تطابق تام ×3، BM25 مبسط، وزن حداثة، دمج التضمين بـ RRF (k=60) كما فى `AiKnowledgeService.RankAsync`.
- `AiSuggestService` (scoped): `SuggestExpenseAsync(text, txType, branchId, user, ct)`, `SuggestItemAsync(name, kind, profile, user, ct)`: يحمّل/يرتّب، يرشّح بالصلاحيات (ص10)، يقرر النموذج (ص2)، يسجّل `AI_Suggestions` (shown)، يعيد `AiSuggestion { Id, Target, CostCenter?, Confidence, Source, Evidence[] }`. `RecordOutcome(id, outcome, finalJson)`.
- `AiSuggestPicker` (النموذج): `CandidatesFor(…, {Fast, Chat})`، بلا أدوات، JSON صارم `{"pick":"<code>|none","why":"…"}`، مهلة 8 ث، الرفض خارج المرشحين.
- `AiSuggestVectorJob : IAiScheduledJob`: ليلاً (02:00 محلى) يضمّن النصوص المميزة الناقصة (ص8).

### 2) الواجهة (`Razors/AccountERP.SharedUI`)

- `Components/Assistant/AiSuggestChip.razor`: بارامترات `Suggestion`, `OnApply`, `OnDismiss`؛ يعرض «لماذا؟» كقائمة منسدلة بالسوابق (تاريخ، نص مختصر، الحساب).
- `Services/AiSuggestUiService.cs` (Scoped، ويب فقط): غلاف + debounce + إلغاء الطلب السابق.
- `CashVoucherPage.razor`: الشريحة تحت `_lineNotes`؛ «تطبيق» يضبط `_selection`/`_subId`/`_subName`/`_accountType` عبر نفس مسار `OnSubAccountChangedAsync` ثم `_costCenterId` إن كان ضمن `_ccSet`؛ عند «حفظ» ناجح تُرسل النتائج لكل سطر عليه اقتراح.
- `ItemEditPage.razor`: الشريحة تحت `ie-catpick` للجديد فقط؛ «تطبيق» = `OnCategoryPicked(id)` + `_f.Group_Id`.
- `Components/Assistant/Settings/AiSuggestTab.razor` (تبويب جديد).

### 3) الصلاحيات والنطاق

- الشريحة تظهر فقط إن: الخدمة متاحة + `SuggestEnabled=1` + المستخدم يملك «المساعد الذكى» (`menuItemAiAssistant`) **أو** إعداد `SuggestForAllUsers` (0 افتراضياً).
- السوابق من كل الفروع المسموحة للمستخدم (`ai_box_perm`/فروعه)، لا فروع أخرى.

### 4) قاعدة البيانات (جديد فقط)

- View `ai.v_expense_history` فى `AiReadViewsCLS` + مساره فى `AiSchemaCatalog` (route = `/accounts/cash/voucher`).
- جدولا `AI_Suggestions`، `AI_Suggestions_Vectors` فى `AiAssistantSchemaCLS.Tables` (+ فهرس `IX_AI_Suggestions_Kind_CreatedAt`، فريد `UX_AI_SuggVec_Kind_Hash_Model`).
- 6 إعدادات (ص17 + `SuggestForAllUsers`).
- كل قيم decimal عبر `AiSqlParams` (درس P8.1.1)، وbyte[] عبر `SqlParameter VarBinary(-1)`.

### 5) الأجزاء

- **P8.8.1 — المخطط والنواة الصافية:** View، جدولان، إعدادات، `AiSuggestModels` (AccountModelsLib، فحص net48/C# 7.3 فى مشروع scratch)، `AiSuggestCLS` (Insert/RecordOutcome/Stats)، `AiSuggestText`، `AiSuggestRanker`. Harness: تطبيع، ترتيب، حداثة، RRF، هامش الغموض (≥ 60 فحصاً).
- **P8.8.2 — الفهرس والخدمة والنموذج:** `AiSuggestIndex`, `AiSuggestService`, `AiSuggestPicker`, `AiSuggestVectorJob`، تسجيل DI. Harness: سوابق مزيفة، ترشيح الصلاحيات، رفض اختيار خارج المرشحين، مهلة، تقليل البيانات، تسجيل outcome.
- **P8.8.3 — شاشة سند الصرف:** المكوّن + الخدمة الواجهية + الربط فى `CashVoucherPage` + مفاتيح `AIG_`.
- **P8.8.4 — شاشة الصنف + تبويب الإعدادات + الاختبارات:** الربط فى `ItemEditPage`، `AiSuggestTab`، اختبارات م-569…م-583 فى `master-user-test-plan.md`.

### 6) مفاتيح الترجمة (×4) — بادئة `AIG_` (نحو 60)

الشريحة (اقتراح، تطبيق، تجاهل، لماذا، الثقة، المصدر: سابقة مطابقة/سوابق قريبة/النموذج)، التبويب (التفعيل، الحدود، حالة الفهرس، إعادة البناء، نسبة القبول، رسائل الأخطاء).

### 7) Harness

نسخة من `p841test.csproj` (scratchpad)، مزوّد mock (`Regex.Unescape` قبل فحص العربية)، صف ربط مهمة بأولوية 0 يُحذف بعد الاختبار (لا `ReplaceRouting`).

### 8) اختبارات المستخدم (م-569…م-583)

- م-569 التصنيف — تفعيل الميزة من تبويب «التصنيف» وظهور حالة الفهرس.
- م-570 التصنيف — كتابة ملاحظة سطر سند صرف مكررة («بنزين سيارة») تُظهر اقتراح الحساب المعتاد.
- م-571 التصنيف — «تطبيق» يملأ الحساب ومركز التكلفة ولا يحفظ.
- م-572 التصنيف — «تجاهل» يخفى الشريحة ولا يغيّر شيئاً.
- م-573 التصنيف — «لماذا؟» يعرض ثلاث سوابق بتاريخها.
- م-574 التصنيف — نص غامض يستدعى النموذج ويقترح من المرشحين فقط.
- م-575 التصنيف — نص جديد كلياً بلا سوابق ولا نموذج ⇒ لا شريحة.
- م-576 التصنيف — اختيار الحساب يدوياً قبل الملاحظة ⇒ لا شريحة.
- م-577 التصنيف — مستخدم بلا صلاحية حساب مقترح ⇒ الاقتراح لا يظهر.
- م-578 التصنيف — مركز تكلفة إلزامى غير مسموح للحساب ⇒ يُقترح الحساب فقط.
- م-579 التصنيف — صنف جديد «زيت محرك 5W30» يقترح الفئة/المجموعة الأقرب.
- م-580 التصنيف — تعديل صنف موجود ⇒ لا شريحة.
- م-581 التصنيف — بعد الحفظ تظهر نسبة القبول فى التبويب.
- م-582 التصنيف — إيقاف الميزة يخفى الشرائح فوراً.
- م-583 التصنيف — تطبيق الموبايل/MAUI لا يعرض الشريحة ولا يتعطل.

### 9) المخاطر

- ملاحظات قديمة فارغة أو عامة جداً ⇒ دقة منخفضة: حد الثقة + «لا شريحة».
- حجم `Money_Transaction_Box` كبير ⇒ حد 24 شهراً/50 ألف سطر وتحميل كسول.
- اقتراح يخالف سياسة الشركة ⇒ لا تطبيق تلقائى أبداً.

### 10) فحوص مفتوحة (أول خطوة فى P8.8.1)

1. نسبة أسطر `Money_Transaction_Box` ذات `Notes` غير فارغة و`Account_Type` حساب عام (إن < 20% ⇒ استعمل `Order_Money.Order_Notes` بديلاً). (قراءة عدّ فقط، لا مراجعة بيانات.)
2. قيم `Account_Type` التى تعنى «حساب عام» مقابل عميل/مورد (من `AccountSelector`).
3. هل `Deleted` موجود فى `Money_Transaction_Box`؟ وإلا شرط `Order_Money`.
4. شكل `CostCenterService.AllowedSet` للحساب المقترح من الخادم (دون الواجهة).
5. هل `_lk.Groups` مرتبطة بالفئة (Group ضمن Category) أم مستقلة؟

### Critical Files

`Razors/AccountERP.SharedUI/Pages/Accounts/CashVoucherPage.razor`، `Pages/Items/ItemEditPage.razor`، `Components/Shared/CostCenterField.razor`، `Components/Shared/AccountSelector.razor`، `AccountModelsLib/AlterDB_CLS/AiReadViewsCLS.cs`، `AiAssistantSchemaCLS`، `Razors/AccountERP.AI/Knowledge/AiEmbeddingService.cs`، `Knowledge/AiKnowledgeService.cs` (RRF)، `Anomalies/AiAnomalyExplainer.cs` (نمط النموذج).

### تم التنفيذ

(فارغ)
