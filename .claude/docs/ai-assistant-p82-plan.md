## P8.2 — بحث Ctrl+K باللغة الطبيعية (خطة تفصيلية)

كُتبت 2026-11-15 بواسطة وكيل التخطيط Fable 5.2 اعتماداً على الكود بعد P8.1 (الإيداع 37601e8). تحل محل مخطط P8.2 فى `ai-assistant-p8-plan.md`. القرارات مرقّمة س1… وكلها محسومة؛ المنفّذ لا يعيد التفكير فيها إلا إذا نقضها فحص من «فحوص مفتوحة» (والبديل مكتوب مع كل فحص).

> ملاحظات قبل التنفيذ:
> - خالفت الخطة المخطط القديم فى نقطة واحدة: الشاشات تؤخذ من `MenuStateService.UserAllowedMenus` مباشرة لا من `AiUserContext.AllowedScreens`. السبب: `AssistantSessionService.BuildUser()` يملأ `AllowedScreens` بالاسم العربى فقط (`m.Description`) ولعناصر `RazorPath` فقط، واللوحة تحتاج الأسماء الأربعة والأب والأيقونة. مصدر الصلاحية هو نفسه.
> - لا يوجد اليوم أى بحث فى القائمة ولا أى اختصار Ctrl عام (الوحيد `ctrlKey` داخل `assistant.js` لحقل الكتابة)، فلا تعارض داخلى.
> - «الترتيب الهجين عبر `AiKnowledgeService`» مرفوض فى هذه القطعة (س4).
> - لم يُتحقق من قيم `SysConfigMenus.RazorPath` لشاشات بطاقة العميل/المورد، ولا من فهارس `Invoices_List.Invoice_Id`، ولا من دالة قراءة `AI_Settings` داخل البوابات. كلها فى «فحوص مفتوحة» كأول خطوة.

### حقائق من الكود (إضافية على رأس ملف P8)

- **التخطيط:** `Razors/AccountERP.SharedUI/Components/Layout/MainLayoutMud.razor`. الشريط `MudAppBar` فيه `MudSpacer` (سطر 105) ثم مجموعة الأزرار. `<AssistantWidget />` خارج `MudRTLProvider` (سطر 231) ويضبط `dir` بنفسه. دالة `Tr(key)` المحلية تضبط `CurrentUICulture` من `LangService.CurrentCultureInfo`.
- **القوائم:** `MenuViewModel` (`AccountModelsLib/Models/Config/SysConfigMenusCLS.cs`): `Menu_ID`, `ParentId`, `KeysData`, `RazorPath`, `Description`, `Description_En/_Fr/_DE`. `MenuLocalization.GetEffectiveRoute()` و`GetLocalizedDescription(culture)` و`FindMenuForRoute(path)` (مطابقة بالبادئة). `KnownRoutes.IsImplemented(route)` يحدد الشاشات المبنية فعلاً. `MenuIcons.GetLeafIcon(description)`.
- **التنقل:** `NavigationManager.NavigateTo(url)` يكفى؛ `MainLayoutMud.UpdateWorkspaceForCurrentRoute` يفتح التبويب.
- **بحث الكيانات الحالى:** `AiToolRegistry.FindEntity` (خاص): `SELECT TOP (8)` من `ai.v_customers/v_suppliers/v_items/v_stores/v_branches` بمعاملات `@q/@starts/@exact` عبر `_sql.Execute(sql, views, Session(c), p, c.Ct)`؛ الـ Views المسموحة = `c.AllowedViews` ∪ `AiDraftTools.ImpliedViews(c)`. لا تطبيع عربى فى SQL ولا تقييد بالفرع.
- **سياق الأدوات:** `AiChatGateway.BuildToolContext(user, ct)` (internal static): `AllowedViews` + `TaxFilter` من `Common.PermissionToShowTaxInvoiceOrNotORAll()`. `AiToolContext.HasRoute(route)` يقارن المسار بلا query. `ReadOnlySqlExecutor.SessionFor(taxFilter, userId)`.
- **المنفّذ:** `ReadOnlySqlExecutor.Execute(sql, allowedViews, sessionValues, parameters, ct)` متزامن، مهلته ثابتة 15 ث، يسجّل `ct.Register(cmd.Cancel)` ويرمى `OperationCanceledException` عند الإلغاء؛ المهلة الأقصر تُفرض بـ `CancellationTokenSource.CancelAfter`. `Validate` يقبل عبارة SELECT واحدة (ومنها `UNION ALL` وجداول مشتقة) ودوال مدمجة بلا مخطط ومعاملات مسماة.
- **الـ Views:** `v_sales` (`sale_id`, `invoice_no` = `Invoices_List.Invoice_Id` `long?`, `invoice_date`, `customer_name`, `branch_id`, `branch_name`, `net_total`) مفلتر بـ `ai_tax`. `v_sales_returns` (`return_id`, `return_no`, `return_date`). `v_purchases` (`purchase_id`, `invoice_no`, `supplier_name`). `v_purchase_returns` (`return_id`, `return_no`). لا View يقيّد بالفرع.
- **كتالوج الشاشات:** `AiScreenCatalog.Screens` فيه `sales_invoice`, `sales_return`, `purchase_invoice`, `purchase_return`, `customer_info` بباراميتر `id`، و`Build(screen, params)` يعيد `(Url, Detail)`. لا مدخل للمورد ولا للصنف. `AiScreenCatalog.Normalize` فى `AccountERP.AI` الذى لا يرجع له `SharedUI`.
- **صفحات الفتح:** `/customers/info/{CustomerId:long}`, `/suppliers/info/{SupplierId:long}`, `/items/{Id:long}`, `/accounts/invoices/sales-invoice/{Id:long?}`, `sales-return/{Id:long?}`, `purchase-invoice/{Id:long?}`, `purchase-return/{Id:long?}`, `/accounts/cash/voucher?type=&id=`.
- **المساعد:** `AssistantSessionService` (Scoped، ويب فقط): `HasAccess`, `IsSending`, `NewConversation()` (لا يفعل شيئاً أثناء الإرسال), `SendAsync(text, AiScreenContext)`، مشترك بين `/assistant` والنافذة العائمة.
- **الواجهة الرقيقة:** `Services/AiBriefService.cs` هو النمط: `sp.GetService<IAi…Gateway>()` (null ⇒ غير متاح)، `Bg(() => …)` = `Task.Run` + `AccountCultureGuard.EnsureInvariant()`، اشتراك `MenuState.OnAuthChanged`.
- **JS:** `wwwroot/js/assistant.js` موديول ES يُستورد بـ `import("./_content/AccountERP.SharedUI/js/assistant.js")`؛ `interop.js` عام فى الويب وMAUI.
- **التفضيلات:** `UserSettingKeys` (`AccountModelsLib/Models/Config/UserSettings/UserSettingsCLS.cs`) + `UserPreferencesService` (`ShowAiAssistantIcon` مثال كامل) + `Components/Shared/UserSettingsMenu.razor`.
- **المخطط:** `AiAssistantSchemaCLS.Tables` (الأعمدة بعد `Id` + ثابت `Audit` + فهارس) و`DefaultSettings`.
- **قيد البناء:** `AccountModelsLib` يُبنى لـ `net48` فى Release، فملفاته الجديدة بأسلوب `AiBriefModels.cs`: بلا `record` ولا `required`/`init` ولا `[..n]` ولا `string.Contains(char)` ولا nullable annotations.
- **MAUI:** `MauiProgram.cs` لا يسجّل `AssistantSessionService` ولا بوابات AI.
- **الاختبارات:** آخر اختبار مستخدم م-468، فاختبارات P8.2 تبدأ من **م-469**.

### القرارات (س = P8.2)

| # | القرار | السبب |
|---|---|---|
| س1 | اللوحة بلا أى استدعاء نموذج: مطابقة نصية ثابتة + SQL مكتوب بأيدينا. النموذج يُستدعى فقط حين يختار المستخدم صف «اسأل المساعد» | Linear/VS Code/Odoo: اللوحة فورية ومجانية |
| س2 | المجموعات بالترتيب: الشاشات، العملاء، الموردون، الأصناف، المستندات، ثم «اسأل المساعد» أخيراً. المخازن والفروع خارج القطعة | D365/Odoo يجمّعان النتائج بالنوع |
| س3 | الشاشات من `MenuStateService.UserAllowedMenus`: الأوراق التى `KnownRoutes.IsImplemented(GetEffectiveRoute())`، مطابقة على الأسماء الأربعة ومسار الآباء، وتُعرض بلغة الواجهة مع مسار الأب كسطر ثانٍ | نفس مصدر الصلاحية؛ بحث بالعربية يجد الشاشة ولو الواجهة إنجليزية |
| س4 | لا ترتيب هجين عبر `AiKnowledgeService` | التضمين استدعاء نموذج لكل ضغطة (ينقض س1) |
| س5 | الشاشات فى الذاكرة فوراً مع كل حرف. بحث البيانات بعد 250 مللى، بحد أدنى حرفين (أو رقم للمستندات) | الشاشات 80% من الاستعمال |
| س6 | كل بحث بيانات = عبارة SELECT واحدة (`UNION ALL`) عبر `ReadOnlySqlExecutor.Execute` | اتصال واحد ونفس مسار الحماية |
| س7 | مهلة البيانات 2.5 ث وإلغاء البحث السابق؛ المهلة ⇒ تبقى الشاشات + سطر «تأخر» | اللوحة لا تنتظر |
| س8 | حد 8 شاشات و5 لكل مجموعة بيانات و8 «الأخيرة» | قوائم قصيرة |
| س9 | تطبيع عربى موحّد `AiSearchText.Normalize` فى `AccountModelsLib` (أ/إ/آ/ٱ→ا، ة→ه، ى→ي، حذف التشكيل والتطويل، الأرقام الهندية والفارسية→لاتينية، أحرف صغيرة، ضم المسافات)، ونفسه داخل SQL بسلسلة `REPLACE` | `LIKE` وحده لا يساوى «احمد» بـ«أحمد» |
| س10 | متعدد الكلمات = AND (حتى 4 كلمات) `LIKE '%كلمة%'` على الاسم المطبّع؛ الكود تساوى أو بادئة | «احمد محمد» يجد «أحمد على محمد» |
| س11 | المستندات بالرقم فقط: أرقام فقط، أو كلمة نطاق + رقم (`فاتورة 123`, `#123`, `inv 123`, `مرتجع 12`). تام ثم بادئة بنطاقات رقمية لا `LIKE` | النطاقات تستعمل الفهرس |
| س12 | الرقم المكرر يعرض حتى 5 مستندات الأحدث أولاً بالتاريخ والطرف والفرع والصافى | الأرقام ليست فريدة عبر الفروع |
| س13 | النوع يُبحث فيه فقط إن كان الـ View مسموحاً **و**المستخدم يملك شاشة الفتح (`HasRoute`) | كل نتيجة تُفتح؛ لا كشف لمن لا يملك الشاشة |
| س14 | المستندات مقيّدة بفروع المستخدم `FinalAccountsService.GetBranches()` (مع إبقاء NULL/0/-1) زيادة على `TaxFilter` | مطابق لنهج الموجز (ق12) |
| س15 | الكيانات بلا تقييد فرع كما فى `find_entity` | بيانات أساسية مشتركة |
| س16 | سياق المستخدم من الدائرة (`AssistantSessionService.BuildUser()`)، و`AiToolContext` والفروع مخزّنة 5 دقائق وتُمسح عند `OnAuthChanged` | `ForUser` يقرأ القاعدة لكل ضغطة |
| س17 | الفتح برابط يبنيه الخادم من `AiScreenCatalog.Build` (معرّف رقمى)، والواجهة ترفض رابطاً لا يبدأ بـ `/` أو فيه `//` أو `:` أو `\` | نفس قاعدة `OpenScreen` |
| س18 | مدخلان جديدان فى `AiScreenCatalog.Screens`: `supplier_info` (`/suppliers/info`, `id`) و`item_card` (`/items`, `id`) | يلزمان للرابط ويفيدان `open_screen` |
| س19 | «الأخيرة» فى جدول جديد `AI_SearchRecent` (`HitCount`, `LastUsedAt`) تظهر والحقل فارغ وتعطى دفعة ترتيب (frecency) | VS Code/Linear |
| س20 | «مسح الأخيرة» يضع `HiddenAt` ولا يحذف؛ إعادة الاستعمال تمسحه | «الأرشفة لا الحذف»؛ على صفوف جدولنا الجديد فقط |
| س21 | عناصر «الأخيرة» يُعاد فحص صلاحيتها عند العرض | صلاحية سُحبت ⇒ يختفى العنصر |
| س22 | لا صف تدقيق لكل بحث؛ `palette_search` فى `AI_ToolCalls_Audit` للخطأ والمهلة فقط | مئات الصفوف يومياً بلا فائدة |
| س23 | Ctrl+K/Cmd+K بـ `e.code === 'KeyK'` مع `preventDefault`؛ الضغط ثانية يغلق؛ لا يُفتح و`MudDialog` مفتوح | `e.key` على لوحة عربية = «ن» |
| س24 | بديل `/` خارج حقول الإدخال وبلا مفاتيح تعديل | GitHub/Linear |
| س25 | تفضيل مستخدم `SearchShortcuts` (افتراضى مفعّل) يوقف الاختصارين؛ الزر يبقى | WCAG 2.1.4 |
| س26 | زر بحث ظاهر فى الشريط العلوى مكان `MudSpacer`: «بحث… Ctrl K» من `sm`، وأيقونة فقط على الهاتف | الاكتشاف |
| س27 | مكوّن خاص (ليس `MudDialog`): طبقة + لوحة بنمط ARIA combobox + `aria-activedescendant` | النمط القياسى للوحة الأوامر |
| س28 | الأسهم/Home/End/Enter/Esc/Tab فى JS، وEnter يستدعى .NET برقم الصف | Blazor Server: رحلة لكل سهم بطيئة |
| س29 | Enter أثناء انتظار البيانات: صف نشط (شاشة) يُفتح فوراً؛ لا نتائج ⇒ يُؤجَّل. «اسأل المساعد» لا يكون الافتراضى إلا بعد اكتمال البحث بلا نتائج | يمنع إرسالاً غير مقصود للنموذج |
| س30 | «اسأل المساعد: …» = `NewConversation()` ثم `SendAsync` بلا انتظار ثم `NavigateTo("/assistant")`. أثناء `IsSending` لا إرسال: تنبيه + انتقال. يظهر فقط مع `HasAccess` ونص ≥ حرفين | مطابق لقرار P8.1 |
| س31 | بادئات نطاق ثابتة: `عميل/عملاء/customer/kunde/client`، `مورد/supplier/lieferant/fournisseur`، `صنف/اصناف/item/artikel/article` + نص ⇒ ذلك النوع؛ `فاتورة/invoice/inv/rechnung/facture/#` أو `مرتجع/return` + رقم ⇒ مستندات؛ `?`/`؟` أولاً أو آخراً ⇒ «اسأل المساعد» فقط. الشاشات تُبحث دائماً بالنص الكامل | «اللغة الطبيعية» المجانية |
| س32 | اللوحة فى الويب وMAUI بمجموعة الشاشات؛ البيانات و«الأخيرة» و«اسأل المساعد» للويب فقط | مطابق لـ ق28 |
| س33 | إعداد `PaletteDataSearch` (افتراضى 1) فى `AI_Settings` يوقف بحث البيانات للجميع؛ تخزين 60 ث | مفتاح أمان لقواعد كبيرة |
| س34 | لا علامة «غير مفعّل» على الكيانات ولا فلترة بها | دلالة `active_flag` غير مؤكدة |
| س35 | سندات النقدية بالرقم خارج القطعة | رقمها يتصادم مع الفواتير |

---

### 1) المجموعات ومصادر البيانات

| المجموعة (`Kind`) | المصدر | شرط الظهور (س13) | الفتح | السطر الثانى |
|---|---|---|---|---|
| `screen` | `MenuState.UserAllowedMenus` | ورقة مبنية (`KnownRoutes.IsImplemented`) | `GetEffectiveRoute()` | مسار الآباء «مبيعات › فواتير» |
| `customer` | `ai.v_customers` | View (مع `ImpliedViews`) + `HasRoute("/customers/info")` | `customer_info` + id | الكود |
| `supplier` | `ai.v_suppliers` | View + `HasRoute("/suppliers/info")` | `supplier_info` + id (جديد) | الكود |
| `item` | `ai.v_items` | View + `HasRoute("/items")` | `item_card` + id (جديد) ⇒ `/items/{id}` | الكود |
| `sale` | `ai.v_sales` | View + `HasRoute("/accounts/invoices/sales-invoice")` | `sales_invoice` + `sale_id` | التاريخ · العميل · الفرع · الصافى |
| `sale_return` | `ai.v_sales_returns` | View + `HasRoute(".../sales-return")` | `sales_return` + `return_id` | كذلك |
| `purchase` | `ai.v_purchases` | View + `HasRoute(".../purchase-invoice")` | `purchase_invoice` + `purchase_id` | التاريخ · المورد · الفرع · الصافى |
| `purchase_return` | `ai.v_purchase_returns` | View + `HasRoute(".../purchase-return")` | `purchase_return` + `return_id` | كذلك |
| `ask` | — | `AssistantSessionService.HasAccess` ونص ≥ حرفين | س30 | — |

المدير (`IsAdmin`) يمر من `HasRoute` دائماً.

### 2) تحليل النص والترتيب (نقى، بلا قاعدة بيانات)

**`AiSearchText.Parse(raw)`**
1. قص إلى 100 حرف، ثم `Normalize`.
2. `?`/`؟` فى أول أو آخر النص ⇒ `AskOnly = true` والنص بدونها.
3. `#` متبوعة بأرقام ⇒ `Scope = "doc"`, `DocNumber`.
4. الكلمة الأولى من جدول البادئات (س31) **وبعدها نص**: نطاق كيان ⇒ `Scope = kind` و`DataText` = الباقى. نطاق مستند ⇒ فقط إن كان الباقى أرقاماً (`DocNumber`)، وإلا يُتجاهل النطاق.
5. النص كله أرقام (1–12 خانة) ⇒ `DocNumber` بلا نطاق (تُبحث المستندات والكيانات بالكود معاً).
6. `Tokens` = كلمات `DataText` بطول ≥ 2 (أو أى طول إن كانت أرقاماً)، أول 4.

**`AiSearchText.Score(query, title, altTitles, parentPath, hitCount)`** للشاشات (0 = لا تطابق):
- كل كلمة يجب أن تطابق فى العنوان (أى لغة) أو مسار الآباء.
- لكل كلمة: كلمة كاملة 100، بادئة كلمة 60، جزء 30، فى مسار الآباء فقط 10. تُجرَّب مع «ال» وبدونها.
- العنوان = النص كله +200؛ يبدأ بالنص +100.
- دفعة التكرار: `min(hitCount, 10) × 5`.
- التعادل: العنوان الأقصر ثم الأبجدى. إزالة التكرار بالمسار الكامل.

**ترتيب البيانات (`rnk` فى SQL):** الكود = النص 0، الاسم المطبّع = النص 1، الاسم يبدأ بالنص 2، الكود يبدأ بالنص 3، غير ذلك 4؛ ثم طول الاسم ثم الاسم. المستندات: التام 0، البادئة 1؛ ثم التاريخ تنازلياً.

### 3) الاستعلام الموحّد (س6)

يُبنى من ثوابت فقط؛ كل قيمة معامل. كل جزء جدول مشتق بنفس الأعمدة التسعة:
`kind, id, title, code, doc_date, party, branch, amount, rnk`.

جزء كيان (`NX(col)` = `REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER(ISNULL(col, N'')), N'أ', N'ا'), N'إ', N'ا'), N'آ', N'ا'), N'ة', N'ه'), N'ى', N'ي'), N'ـ', N'')`):

```sql
SELECT * FROM (
  SELECT TOP (5) N'customer' AS kind, customer_id AS id,
         customer_name COLLATE DATABASE_DEFAULT AS title,
         CAST(customer_code AS nvarchar(100)) COLLATE DATABASE_DEFAULT AS code,
         CAST(NULL AS date) AS doc_date, CAST(NULL AS nvarchar(300)) AS party,
         CAST(NULL AS nvarchar(200)) AS branch, CAST(NULL AS decimal(28,4)) AS amount,
         CASE WHEN CAST(customer_code AS nvarchar(100)) = @exact THEN 0
              WHEN NX(customer_name) = @qn THEN 1
              WHEN NX(customer_name) LIKE @starts THEN 2
              WHEN CAST(customer_code AS nvarchar(100)) LIKE @cstarts THEN 3 ELSE 4 END AS rnk
  FROM ai.v_customers
  WHERE (NX(customer_name) LIKE @t0 AND NX(customer_name) LIKE @t1)
     OR CAST(customer_code AS nvarchar(100)) LIKE @cstarts
  ORDER BY rnk, LEN(customer_name), customer_name
) AS e1
UNION ALL …
```

جزء مستند (المبيعات):

```sql
SELECT * FROM (
  SELECT TOP (5) N'sale' AS kind, sale_id AS id,
         CAST(invoice_no AS nvarchar(30)) COLLATE DATABASE_DEFAULT AS title,
         CAST(NULL AS nvarchar(100)) AS code, invoice_date AS doc_date,
         customer_name COLLATE DATABASE_DEFAULT AS party,
         branch_name COLLATE DATABASE_DEFAULT AS branch,
         CAST(net_total AS decimal(28,4)) AS amount,
         CASE WHEN invoice_no = @n THEN 0 ELSE 1 END AS rnk
  FROM ai.v_sales
  WHERE (invoice_no = @n OR invoice_no BETWEEN @a1 AND @b1
         OR invoice_no BETWEEN @a2 AND @b2 OR invoice_no BETWEEN @a3 AND @b3)
    AND (branch_id IN (@br0, @br1) OR branch_id IS NULL OR branch_id IN (0, -1))
  ORDER BY rnk, invoice_date DESC, sale_id DESC
) AS d1
```

- المعاملات: `@exact` = الخام بعد القص؛ `@qn` = المطبّع؛ `@starts` = المطبّع المهرَّب + `%`؛ `@cstarts` = الخام المهرَّب + `%`؛ `@t0…@t3` = `%كلمة%`؛ `@n`؛ `@a1..@b3` من `AiSearchText.PrefixRanges(n)`؛ `@br0…`.
- تهريب `LIKE` بنمط `FindEntity`: `[`→`[[]`، `%`→`[%]`، `_`→`[_]`.
- المرتجعات: `return_id/return_no/return_date`. المشتريات: `purchase_id`, `supplier_name`.
- لا أجزاء مسموحة ⇒ لا استعلام. نطاقات البادئة تُحذف إن تجاوزت `long` أو الرقم أطول من 9 خانات.
- الفروع الفارغة ⇒ تُحذف أجزاء المستندات وتُسجَّل فى `SkippedKinds`.

### 4) قاعدة البيانات (كائنات جديدة فقط)

`AiAssistantSchemaCLS.Tables` يضاف له:

```csharp
// P8.2 (س19–س21): آخر ما فتحه المستخدم من لوحة البحث. صف لكل (مستخدم، نوع، مفتاح). HiddenAt = «مسح الأخيرة» بلا حذف.
("AI_SearchRecent", @"
    [User_Id] [bigint] NOT NULL,
    [Kind] [nvarchar](20) NOT NULL,
    [RefKey] [nvarchar](300) NOT NULL,
    [Title] [nvarchar](300) NOT NULL,
    [Subtitle] [nvarchar](300) NULL,
    [Url] [nvarchar](400) NOT NULL,
    [HitCount] [int] NOT NULL DEFAULT ((1)),
    [LastUsedAt] [datetime] NOT NULL DEFAULT (getdate()),
    [HiddenAt] [datetime] NULL,",
 @"CREATE UNIQUE NONCLUSTERED INDEX [UX_AI_SearchRecent_User_Kind_Key] ON [dbo].[AI_SearchRecent]([User_Id], [Kind], [RefKey]);
CREATE NONCLUSTERED INDEX [IX_AI_SearchRecent_User_Last] ON [dbo].[AI_SearchRecent]([User_Id], [LastUsedAt] DESC);"),
```

- `RefKey`: للشاشة = المسار الكامل بأحرف صغيرة؛ للبيانات = المعرّف نصاً. `Title` للشاشة = الاسم العربى (يُعاد توطينه عند العرض).
- `DefaultSettings`: `(AiSettingKeys.PaletteDataSearch, "1")` + الثابت فى `AiModels.cs`.
- `UserSettingKeys.SearchShortcuts = "SearchShortcuts"`.
- لا Views جديدة، لا عنصر قائمة جديد، لا مفاتيح أجنبية، لا مساس بأى صف قائم.

### 5) الأجزاء (كل جزء قابل للإيداع والاختبار وحده)

**P8.2.1 — النواة النقية والمخطط والـ DAL (بلا واجهة)**

- جديد `AccountModelsLib/BusinessLibShared/Ai/AiSearchModels.cs` (namespace `AccountModelsLib`، أسلوب net48):
  - `static class AiSearchKinds`: `Screen, Customer, Supplier, Item, Sale, SaleReturn, Purchase, PurchaseReturn, Ask` + `Entities[]`, `Documents[]`.
  - `static class AiSearchConst`: `MinChars = 2`, `MaxQueryChars = 100`, `MaxTokens = 4`, `DebounceMs = 250`, `DataTimeoutMs = 2500`, `PerGroup = 5`, `ScreensMax = 8`, `RecentMax = 8`, `RecentLoad = 50`, `MaxDocDigits = 12`, `AuditTool = "palette_search"`.
  - `class AiSearchQuery { string Raw; string Normalized; string DataText; List<string> Tokens; string Scope; long? DocNumber; bool AskOnly; bool HasDataText }`.
  - `class AiSearchHit { string Kind; long Id; string Title; string Code; DateTime? Date; string Party; string Branch; decimal? Amount; string Url; string RefKey }`.
  - `class AiSearchResult { List<AiSearchHit> Hits; bool TimedOut; bool Disabled; List<string> SkippedKinds; long DurationMs; string Error }`.
  - `class AiSearchRecentRow { long Id; long User_Id; string Kind; string RefKey; string Title; string Subtitle; string Url; int HitCount; DateTime LastUsedAt; DateTime? HiddenAt }`.
  - `interface IAiSearchGateway`: `List<string> AllowedKinds(AiUserContext user)`؛ `Task<AiSearchResult> SearchAsync(string text, AiUserContext user, CancellationToken ct)`؛ `List<AiSearchRecentRow> GetRecent(AiUserContext user, int top)`؛ `void Touch(string kind, string refKey, string title, string subtitle, string url, AiUserContext user)`؛ `void ClearRecent(long userId)`.
- جديد `AccountModelsLib/BusinessLibShared/Ai/AiSearchText.cs`: `Normalize`, `Parse`, `Score`, `EscapeLike`, `PrefixRanges(long n)` (حتى 3 نطاقات), `IsSafeInternalUrl` (س17), جدول البادئات `ScopeWords` (ثابت خاص، مطبّع).
- جديد `AccountModelsLib/BusinessLibShared/Ai/AiSearchCLS.cs` (نمط `AiToolAuditCLS`: `Func<operations> _newOpp`، `ExecuteSqlInterpolated`، `CallSnowflakeIdGenerate.GenerateNewId()`):
  - `Touch(userId, kind, refKey, title, subtitle, url)`: `UPDATE … SET HitCount = HitCount + 1, LastUsedAt = now, HiddenAt = NULL, Title, Subtitle, Url, UpdatedBy, UpdatedAt WHERE User_Id AND Kind AND RefKey`؛ صفر صفوف ⇒ `INSERT … WHERE NOT EXISTS` (نمط `AiBriefCLS`).
  - `List(userId, top)` (`HiddenAt IS NULL` بترتيب `LastUsedAt DESC`).
  - `HideAll(userId)` (`SET HiddenAt = now WHERE User_Id AND HiddenAt IS NULL`).
- تعديل: `AlterDB_CLS/AiAssistantSchemaCLS.cs`، `BusinessLibShared/Ai/AiModels.cs` (`AiSettingKeys.PaletteDataSearch`)، `Models/Config/UserSettings/UserSettingsCLS.cs` (`SearchShortcuts`).
- القبول: `EnsureSchema` مرتين بلا خطأ؛ `Normalize("أحْمَـد ٢٠٢٥ الإدارة")` = `احمد 2025 الاداره`؛ حالات `Parse`/`Score`/`PrefixRanges` تمر؛ `Touch` مرتين ⇒ صف واحد `HitCount = 2`؛ `HideAll` ثم `Touch` ⇒ يظهر؛ بناء `AccountModelsLib` Release (net48) ناجح.

> **تم التنفيذ (2026-11-15):** الملفات الثلاثة الجديدة + `AI_SearchRecent` + `AiSettingKeys.PaletteDataSearch` (افتراضى 1) + `UserSettingKeys.SearchShortcuts`. إضافات صغيرة على الخطة: `AiSearchKinds.DocScope = "doc"` و`IsEntity/IsDocument/IsRecentKind`؛ `AiSearchConst.MaxPrefixDigits = 9`؛ التطبيع يحوّل أيضاً `ی`→`ي` و`ک`→`ك` ويحذف الألف الخنجرية؛ جدول البادئات يقبل صيغ «ال» (العميل، المورد، الصنف، الفاتورة، المرتجع) وجموعاً إنجليزية؛ `Score(query, title, altTitles, parentPath, hitCount)` يطبّع مدخلاته بنفسه؛ `PrefixRanges` يعيد `List<(long From, long To)>`؛ `Touch` يعيد التحديث إن سبقه إدراج متزامن (لا تضيع ضغطة). **بناء net48:** المكتبة كلها لا تُبنى net48 اليوم (EF Core 10 لا يدعمها — قائم قبل P8.2)، فاختُبر `AiSearchText.cs` + `AiSearchModels.cs` وحدهما net48 بـ C# 7.3 ونجحا؛ `AiSearchCLS.cs` بنفس صياغة `AiBriefCLS`. Harness `p821test` 83/83 (scratchpad الجلسة 76845c8e)، وانحدار P8.1.1 113/113.

**P8.2.2 — اللوحة بمجموعة الشاشات (ويب + MAUI، بلا قاعدة بيانات)**

- أول خطوة: مهارة `ui-ux-pro-max`.
- جديد `Razors/AccountERP.SharedUI/Services/CommandPaletteService.cs` (Scoped): المنشئ `(IServiceProvider sp, MenuStateService menu, LanguageService lang)`؛ `GetService<IAiSearchGateway>()` و`GetService<AssistantSessionService>()` (قد يكونان null)؛ `IsAvailable => _menu.IsAuthenticated && _menu.IsLoaded`؛ `event Action OpenRequested` + `RequestOpen()`؛ `List<PaletteItem> SearchScreens(string text, IDictionary<string,int> hitCounts)` — فهرس شاشات يُبنى مرة ويُعاد عند `OnAuthChanged`/`OnMainMenuChanged`/تغيّر اللغة (`Route`, `TitleLocalized`, `AllTitlesNormalized`, `ParentPathLocalized`, `ParentPathNormalized`, `Icon`)؛ `sealed class PaletteItem { Kind, Title, Subtitle, Trailing, Icon, Url, RefKey, long Id }`.
- جديد `Razors/AccountERP.SharedUI/Components/Shared/CommandPalette.razor` (+ `<style>` ببادئة `.cmdk-`):
  - الخدمة بـ `GetService` (null ⇒ لا يرسم).
  - البنية: `div.cmdk-scrim` (ضغطه يغلق) ⟵ `div.cmdk-panel role="dialog" aria-modal="true" aria-label=AIS_Title dir=…` ⟵ رأس: أيقونة + `input#cmdk-input role="combobox" aria-expanded aria-controls="cmdk-list" aria-autocomplete="list" aria-activedescendant autocomplete="off" spellcheck="false" maxlength="100"` + زر إغلاق ⟵ `div#cmdk-list role="listbox"` بمجموعات `role="group" aria-labelledby` وصفوف `role="option" id="cmdk-opt-{i}" data-idx="{i}" aria-selected` ⟵ تذييل تلميحات (يُخفى على الهاتف) ⟵ `div.cmdk-sr aria-live="polite"`.
  - القياسات: عرض `min(640px, 100vw - 32px)`، أعلى `12vh`، القائمة `min(60vh, 480px)`، `z-index: 1450`، صف ≥ 44px، خط الحقل ≥ 16px. تحت 600px ملء الشاشة (`inset: 0`، `100dvh`). ألوان `--mud-palette-*` فقط. الصف النشط `color-mix(primary 12%)` + شريط `inset-inline-start`. `prefers-reduced-motion`. `@media print { display: none }`. خصائص منطقية للـ RTL.
  - `@oninput` ⇒ `OnTextChanged(value)` يحسب الشاشات فوراً.
  - `[JSInvokable] OpenFromShortcut()`, `ActivateFromKeyboard(int index)`, `CloseFromKeyboard()`.
  - `Activate(item)`: `IsSafeInternalUrl` ⇒ إغلاق ثم `Nav.NavigateTo(item.Url)`.
  - يغلق عند `LocationChanged` و`OnAuthChanged`.
- جديد `Razors/AccountERP.SharedUI/wwwroot/js/palette.js` (موديول ES):
  - `init(dotnet, { shortcuts })`: مستمع `keydown` واحد على `document` (capture، محمى بعلم على `window`). يتجاهل `isComposing`/`repeat`. `(ctrlKey||metaKey) && !shiftKey && !altKey && code === 'KeyK'` ⇒ `preventDefault` + فتح/إغلاق. `/` بلا تعديل وخارج الحقول ⇒ كذلك. لا يفتح مع `.mud-dialog`.
  - `setOptions({ shortcuts })`.
  - `opened(panelEl, inputEl)`: يحفظ `activeElement`، يركّز الحقل، يقفل تمرير `body`، `keydown`: `ArrowDown/Up` (دائرى)، `Home/End`، `PageUp/Down` (±5)، `Enter` ⇒ `ActivateFromKeyboard`، `Escape` ⇒ `CloseFromKeyboard` + `stopPropagation`، `Tab` يدور بين الحقل وزر الإغلاق. الفأرة فوق صف تجعله نشطاً.
  - `sync(count)`: النشط = 0، `aria-selected`/`aria-activedescendant`، `scrollIntoView({ block: 'nearest' })`.
  - `closed()`: يفك القفل ويعيد التركيز. `dispose()`.
- تعديل `MainLayoutMud.razor`: `MudSpacer` + زر البحث (حين `MenuState.IsLoaded`) + `MudSpacer`؛ `button.cmdk-trigger` بنص `AIS_TriggerText` وشارة `Ctrl K` (`d-none d-sm-flex`) و`MudIconButton` (`d-sm-none`) بـ `aria-label` و`aria-keyshortcuts="Control+K"`؛ `<CommandPalette />` بجوار `<AssistantWidget />`.
- التسجيل: `Program.cs` (بعد سطر 129) و`Razors/AccountERP.MobileDesktop/MauiProgram.cs`: `AddScoped<AccountERP.SharedUI.Services.CommandPaletteService>()`.
- القبول: Ctrl+K والزر و`/` تفتح؛ شاشات المستخدم فقط؛ الأسهم/Enter/Esc والتركيز يعود؛ Tab لا يخرج؛ شاشة «قيد الإنشاء» لا تظهر؛ `/` داخل حقل يكتب الحرف؛ Ctrl+K مع حوار مفتوح لا يفعل شيئاً؛ الهاتف ملء الشاشة؛ MAUI بلا أخطاء لغياب البوابة.

> **تم التنفيذ (2026-11-15):** بدأ بمهارة `ui-ux-pro-max` (حلقة تركيز ظاهرة، صفوف ≥ 44px (48px فعلياً)، `prefers-reduced-motion`، التركيز لا يُحجب). **الفحص 8:** `--mud-zindex-dialog` = 1400 والتنبيهات 1500 فى MudBlazor 9.7 ⇒ اللوحة 1450 والطبقة 1449؛ الحوار المفتوح يُكشف بـ `.mud-dialog-container`. **الفحص 9:** `AccountERP.SalesRepApp` لا يرجع لـ `SharedUI` ولا يستعمل `MainLayoutMud`. الملفات: `Services/CommandPaletteService.cs` (فهرس الشاشات يُعاد بناؤه عند تغيّر مرجع/عدد `UserAllowedMenus` أو اللغة أو `OnAuthChanged`/`OnMainMenuChanged`؛ الأوراق فقط بلا تكرار للمسار؛ مسار الآباء للعرض بلغة الواجهة وللمطابقة بالأربع؛ `IsMac` + حدث `Changed` لعرض ⌘ K)، `Components/Shared/CommandPalette.razor`، `wwwroot/js/palette.js`، `MainLayoutMud.razor` (زر + `<CommandPalette />`)، التسجيل فى `Program.cs` و`MauiProgram.cs`، و37 مفتاح `AIS_` ×4 دفعة واحدة. فروق صغيرة عن الخطة: التوسيط بـ `inset-inline: 0; margin-inline: auto` بدل `transform`؛ Home/End تتحرك فى القائمة فقط مع Ctrl أو والحقل فارغ (وإلا تبقى لتحريك المؤشر داخل النص)؛ `forgetRestore()` فى JS حتى لا يعود التركيز لعنصر الشاشة القديمة بعد التنقل؛ `/` يُقبل بـ `e.key === '/'` أو `e.code === 'Slash'`؛ الاختصارات مفعّلة دائماً هنا (`SearchShortcuts` يُربط فى P8.2.5). بناء الويب وMAUI (Windows) ناجح. اختبار المتصفح عند طلب المستخدم (م-469، م-470، م-471، م-472، م-483، م-485 جزء الشاشات).

**P8.2.3 — بوابة بحث البيانات (بلا واجهة)**

- جديد `Razors/AccountERP.AI/Search/AiSearchGateway.cs` (`public sealed class AiSearchGateway : IAiSearchGateway`، المنشئ `(ReadOnlySqlExecutor sql)`):
  - جدول ثابت `KindSpec(Kind, View, IdCol, TitleCol, CodeCol, DateCol, PartyCol, OpenRoute, ScreenKey, IsDocument)` بقيم §1.
  - `AllowedKinds(user)`: `ctx = AiChatGateway.BuildToolContext(user, default)`؛ Views = `ctx.AllowedViews` ∪ `AiDraftTools.ImpliedViews(ctx)` للكيانات و`ctx.AllowedViews` للمستندات؛ ثم `ctx.HasRoute(OpenRoute)`.
  - `SearchAsync`: `Parse` ⇒ `AskOnly` أو لا نص ⇒ فارغة. `PaletteDataSearch != "1"` ⇒ `Disabled`. الأنواع = المسموحة ∩ النطاق (المستندات فقط مع `DocNumber`). الفروع من `new FinalAccountsService().GetBranches()`. SQL (§3) ثم `Task.Run` لـ `_sql.Execute(sql, views, ReadOnlySqlExecutor.SessionFor(ctx.TaxFilter, user.UserId), parameters, linkedCt)` مع `CancelAfter(DataTimeoutMs)`.
  - `OperationCanceledException`: إلغاء خارجى ⇒ يُرمى؛ وإلا `TimedOut = true` + تدقيق `palette_search`.
  - `AiSqlRejectedException` أو خطأ: `operations.DataDictionaryLogerException(ex)` + تدقيق + `Error` (بلا رمى).
  - الصفوف: `Url = AiScreenCatalog.Build(AiScreenCatalog.Find(ScreenKey), new JsonObject { ["id"] = id }).Url`؛ `RefKey = id`.
  - `GetRecent`: `AiSearchCLS.List` ثم إسقاط أنواع غير مسموحة. `Touch`/`ClearRecent` تفرضان `user.UserId`؛ `Touch` يرفض رابطاً غير آمن ونوعاً غير معروف.
  - ذاكرة داخل المثيل: `ctx` والفروع 5 دقائق، الإعداد 60 ث لنفس `UserId`.
- تعديل: `Tools/AiScreenCatalog.cs` (`supplier_info`, `item_card`)؛ `AiServiceCollectionExtensions.cs`: `services.AddScoped<IAiSearchGateway>(sp => new Search.AiSearchGateway(sp.GetRequiredService<Tools.ReadOnlySqlExecutor>()));`.
- القبول (harness): `Validate` يقبل العبارة؛ «احمد» يجد «أحمد»؛ كلمتان AND؛ كود تام يتصدر؛ رقم تام ثم بادئة؛ مستخدم مبيعات فقط بلا `v_purchases`/`v_suppliers`؛ `TaxFilter` يغيّر النتائج؛ فرع غير مسموح لا يظهر؛ مهلة ⇒ `TimedOut`؛ إلغاء خارجى يُرمى؛ `%` و`'` و`[` آمنة؛ الإعداد 0 ⇒ `Disabled`؛ الروابط تطابق القوالب؛ انحدار `open_screen`.

> **تم التنفيذ (2026-11-15):** `Razors/AccountERP.AI/Search/AiSearchGateway.cs` + مدخلا `supplier_info`/`item_card` فى `AiScreenCatalog` + التسجيل Scoped. **الفحوص المفتوحة:** (1) `/customers/info` و`/suppliers/info` و`/items` قيم `RazorPath` فعلية (أعمدة `SysConfigMenus` مخزّنة بإزاحة حرفية 0x6F26 فى قاعدة التطوير) ⇒ الشرط الصارم. (2) `/items/{Id:long}` = `ItemEditPage`. (3) لا فهارس على `Invoices_List`/`Bills_List` ونظيريهما؛ نطاقات البادئة بقيت لأن المتوسط 49–74 مللى (الهدف < 400). (4) `/accounts/invoices/list` لا يقيّد بالفرع إطلاقاً؛ أُبقى س14 (فروع `USER_DETAILES` عبر `FinalAccountsService.GetBranches`) لأنه نظام صلاحيات الفروع القائم والأحوط. (5) الإعداد يُقرأ بـ `AiProvidersCLS.GetSettings()` والخطأ `AiToolOutcome.Error`. (6) `Validate` يقبل العبارة الكاملة (7 أجزاء). (7) `UserSessionContext` = `AsyncLocal` فينتقل إلى `Task.Run`. **فروق صغيرة:** `@exact`/`@cstarts` تستعمل `DataText` المطبّع (لا الخام) حتى لا تدخل كلمة النطاق فى مقارنة الكود؛ `NX` يطوى أيضاً `ٱ` و`ی` و`ک` مطابقاً لـ `Normalize`؛ الترتيب النهائى فى C# (المجموعة ثم `rnk` ثم طول الاسم/التاريخ) لأن ترتيب `UNION ALL` غير مضمون؛ مهلة انتهت والنتيجة وصلت ⇒ تُعامل كمهلة؛ `AccountCultureGuard` غير متاح فى `AccountERP.AI` (التحويل كله `InvariantCulture`)؛ خطافات اختبار `ContextSource`/`BranchSource`/`SettingsSource`/`TimeoutMsOverride` و`LastSql`. Harness `p823test` 65/65 (scratchpad الجلسة c0ee7a88)، وانحدار P8.2.1 83/83.

**P8.2.4 — نتائج البيانات و«الأخيرة» فى اللوحة**

- `CommandPaletteService`: `HasData => _gw != null && _chat != null && _chat.UserId > 0`؛ `SearchDataAsync(text, ct)` (`Task.Run` + `EnsureInvariant()`، `user = _chat.BuildUser()`) مع ذاكرة آخر 20 نصاً 30 ث؛ `RecentAsync()` (يحمّل `RecentLoad` عند كل فتح، يبنى `hitCounts`، أول `RecentMax` بعد فحص س21)؛ `TouchAsync(item)` (بلا انتظار)؛ `ClearRecentAsync()`.
- `CommandPalette.razor`: debounce بـ `CancellationTokenSource` + `Task.Delay`؛ رقم تسلسل يرفض النتائج المتأخرة؛ skeleton بعد 150 مللى + `aria-busy`؛ المجموعات بعناوين `AIS_Grp*`؛ المبلغ بثقافة الواجهة والتاريخ بـ `Prefs.DateFormat`؛ الأرقام `dir="ltr"`؛ حقل فارغ ⇒ «الأخيرة» + «مسح»، وإلا `AIS_EmptyHint`؛ `AIS_NoResults`/`AIS_DataSlow`/`AIS_DataError` سطر صغير؛ س29؛ `Activate`: `TouchAsync` ثم التنقل.
- القبول: اسم عميل ⇒ الشاشات فوراً ثم العملاء < ثانية؛ لا نتائج قديمة؛ فتح عميل/صنف/فاتورة صحيح؛ «الأخيرة» والتكرار؛ «مسح» بلا حذف صفوف؛ MAUI بلا بيانات ولا أخطاء.

> **تم التنفيذ (2026-11-15):** `CommandPaletteService`: `HasData`، `SearchDataAsync` (ذاكرة آخر 20 نصاً 30 ث، المهلة والخطأ لا يُحفظان، تُمسح مع تغيّر المستخدم/القائمة/اللغة)، `RecentAsync` (يعيد توطين اسم الشاشة ومسارها من الفهرس ويسقط ما ليس فى القائمة — س21 — ويبنى `HitCounts`)، `ScreenIcon`، `Touch(item)` بلا انتظار (يرفع `HitCounts` محلياً فوراً)، `ClearRecentAsync`. `CommandPalette.razor`: مجموعات بترتيب س2 (`_groups` + قائمة مسطحة للفهارس)، debounce 250 بـ `CancellationTokenSource`، رقم تسلسل `_seq` يرفض المتأخر، هيكل انتظار بعد 150 مللى مع `aria-busy`، أسطر `AIS_DataSlow`/`AIS_DataError`، المستندات: «رقم N» + نوع · تاريخ (`Prefs.DateFormat`) · طرف · فرع والصافى `N2` بثقافة الواجهة فى عمود `dir=ltr`، الكيانات: «كود N» (الكود 0 لا يُعرض)، «الأخيرة» حين الحقل فارغ مع زر «مسح الأخيرة» + Snackbar + منطقة حية، س29 (Enter بلا صفوف والبحث جارٍ ⇒ يُنفَّذ على أول نتيجة عند وصولها). JS: `sync(keep)` يُبقى الصف النشط عند وصول البيانات بعد الشاشات، و`focusInput()`. فرق صغير: شرط بحث البيانات = رقم مستند أو كلمة ≥ حرفين (`Tokens`) بدل طول النص. بناء الويب ناجح؛ اختبار المتصفح عند طلب المستخدم (م-473…م-481، م-486).

**P8.2.5 — «اسأل المساعد»، بادئات النطاق، تفضيل الاختصارات، اكتمال الترجمة**

- `AssistantSessionService.cs`: `public bool TryAskNew(string text, AiScreenContext context)` — false إن `!HasAccess` أو `IsSending` أو نص فارغ؛ وإلا `NewConversation(); _ = SendAsync(text, context); return true;`.
- `CommandPalette.razor`: صف `ask` أخير (`AIS_AskAssistant`) بأيقونة `AutoAwesome`؛ `TryAskNew(text, context)` (context = منطق `AssistantWidget.CurrentContext`)؛ false ⇒ Snackbar `AIS_AssistantBusy`؛ ثم `NavigateTo("/assistant")`. `AskOnly` ⇒ الصف الوحيد. شرائح `AIS_HintScopes` فى الحالة الفارغة.
- `UserPreferencesService.cs` (`SearchShortcuts` + `SetSearchShortcuts(bool)`) و`UserSettingsMenu.razor` (`AIS_PrefShortcuts`)؛ `setOptions` عند `Prefs.OnChanged`.
- إكمال `AIS_*` ×4؛ إضافة م-469…م-486 إلى `master-user-test-plan.md`؛ تحديث «تم التنفيذ».
- القبول: «مبيعات الشهر الماضى؟» + Enter ⇒ `/assistant` بمحادثة جديدة؛ أثناء رد جارٍ تنبيه بلا إرسال؛ بلا صلاحية المساعد لا صف؛ «عميل احمد» عملاء فقط؛ «فاتورة 120» مستندات؛ «فاتورة مبيعات» الشاشة؛ إيقاف التفضيل يعطل Ctrl+K و`/` ويبقى الزر.

### 6) مفاتيح الترجمة (×4: ar/en/de/fr) — بادئة `AIS_` (37 مفتاحاً)

| المفتاح | النص العربى |
|---|---|
| `AIS_Title` | البحث السريع |
| `AIS_TriggerText` | بحث… |
| `AIS_TriggerAria` | فتح البحث السريع |
| `AIS_Placeholder` | ابحث عن شاشة أو عميل أو صنف أو رقم فاتورة… |
| `AIS_Close` | إغلاق |
| `AIS_GrpRecent` | الأخيرة |
| `AIS_GrpScreens` | الشاشات |
| `AIS_GrpCustomers` | العملاء |
| `AIS_GrpSuppliers` | الموردون |
| `AIS_GrpItems` | الأصناف |
| `AIS_GrpDocuments` | المستندات |
| `AIS_KindSale` | فاتورة مبيعات |
| `AIS_KindSaleReturn` | مرتجع مبيعات |
| `AIS_KindPurchase` | فاتورة مشتريات |
| `AIS_KindPurchaseReturn` | مرتجع مشتريات |
| `AIS_DocNo` | رقم {0} |
| `AIS_Code` | كود {0} |
| `AIS_AskAssistant` | اسأل المساعد: {0} |
| `AIS_AskHint` | يرسل سؤالك إلى المساعد الذكى |
| `AIS_AssistantBusy` | المساعد يكتب رداً الآن. انتظر انتهاءه ثم أعد السؤال. |
| `AIS_NoResults` | لا نتائج مطابقة |
| `AIS_NoResultsAsk` | لا نتائج. اضغط Enter لسؤال المساعد. |
| `AIS_Searching` | جارٍ البحث فى البيانات… |
| `AIS_DataSlow` | البحث فى البيانات تأخر. أكمل الكتابة لتضييقه. |
| `AIS_DataError` | تعذّر البحث فى البيانات الآن. |
| `AIS_EmptyHint` | اكتب اسم شاشة أو عميل أو صنف، أو رقم فاتورة. |
| `AIS_HintScopes` | جرّب: «عميل أحمد»، «صنف سكر»، «فاتورة 120»، أو اختم بعلامة ؟ لسؤال المساعد |
| `AIS_ClearRecent` | مسح الأخيرة |
| `AIS_RecentCleared` | مُسحت قائمة الأخيرة |
| `AIS_KeyNavigate` | تنقل |
| `AIS_KeyOpen` | فتح |
| `AIS_KeyClose` | إغلاق |
| `AIS_ResultsCount` | {0} نتيجة |
| `AIS_PrefShortcuts` | اختصارات البحث السريع (Ctrl+K و /) |
| `AIS_PrefShortcutsHint` | عند الإيقاف يبقى زر البحث فى الشريط العلوى |
| `AIS_ShortcutCtrlK` | Ctrl K |
| `AIS_ShortcutCmdK` | ⌘ K |

أسماء الشاشات من أعمدة `SysConfigMenus` لا من الـ resx. لا عنصر قائمة جديد.

### 7) اختبار الخدمة (harness)

مشروع console فى scratchpad (`p82test`) يرجع لـ `AccountERP.AI` و`AccountModelsLib`: `operations.conn_str_` ثم `SystemSettingsLoader.LoadTenantSettings()` ثم `UserSessionContext.Current` (أو `AiUserContextFactory.ForUser(id).Run(...)`). لا نص اتصال فى ملف يودَع. ينظّف فقط صفوف `AI_SearchRecent` لمستخدمين وهميين (9820000001…3) وصفوف التدقيق التى أنشأها.

- **النقى:** `Normalize`، `Parse` (عادى، أرقام، `#12`، «فاتورة 12»، «فاتورة مبيعات»، «عميل احمد»، «عميل» وحدها، `؟` أولاً وآخراً، 150 حرفاً، 6 كلمات)، `Score`، `PrefixRanges` (5، 123، 10 خانات، حدود `long`)، `EscapeLike`، `IsSafeInternalUrl`.
- **المخطط والـ DAL:** `EnsureSchema` مرتين؛ الإعداد مرة؛ `Touch` ×2؛ `List` بالترتيب؛ `HideAll` ثم `Touch`؛ عزل المستخدمين؛ 8 خيوط `Touch` ⇒ صف واحد.
- **البوابة:** `Validate` للعبارة الكاملة أولاً؛ عينة حقيقية للقراءة فقط (همزة، كود تام، صنف بكلمتين، آخر رقم فاتورة تام ثم بادئة، مقارنة العدد باستعلام مستقل)؛ سياق مبيعات فقط ⇒ SQL بلا `v_purchases`/`v_suppliers`؛ بلا `/customers/info` ⇒ بلا العملاء؛ `TaxFilter`؛ الفرع؛ مهلة 1 مللى ⇒ `TimedOut` + تدقيق؛ إلغاء خارجى؛ حقن (`%`, `_`, `[`, `'`, `;--`)؛ الإعداد 0؛ الروابط السبعة؛ `GetRecent` يسقط نوعاً سُحبت صلاحيته.
- **الأداء:** متوسط 20 بحثاً (الهدف < 400 مللى؛ يُسجَّل الرقم).
- **الانحدار:** harness P8.1.1 (113) وP3/`open_screen` إن توفر.

### 8) اختبارات المستخدم (تُضاف إلى `master-user-test-plan.md`)

| الكود | العنوان |
|---|---|
| م-469 | فتح لوحة البحث السريع بـ Ctrl+K وبزر الشريط العلوى وإغلاقها بـ Esc مع رجوع التركيز لمكانه |
| م-470 | البحث عن شاشة بالاسم العربى مع اختلاف الهمزة والتاء المربوطة وفتحها بـ Enter |
| م-471 | التنقل بين النتائج بالأسهم وHome وEnd دون خروج التركيز من حقل البحث |
| م-472 | مستخدم محدود الصلاحيات لا يجد فى اللوحة شاشة ليست فى قائمته |
| م-473 | البحث عن عميل بالاسم وبالكود وفتح بطاقته من اللوحة |
| م-474 | البحث عن صنف بكلمتين من اسمه وفتح بطاقته |
| م-475 | البحث عن مورد وفتح بياناته، وعدم ظهور الموردين لمستخدم بلا شاشة الموردين |
| م-476 | كتابة رقم فاتورة مبيعات تعرض الفاتورة بتاريخها وعميلها وصافيها وتفتحها |
| م-477 | الرقم المكرر بين الفروع يعرض كل المستندات المطابقة، والأرقام الهندية تعمل كاللاتينية |
| م-478 | صلاحية الفواتير الضريبية/غير الضريبية وتقييد الفرع ينعكسان على مستندات اللوحة |
| م-479 | مستخدم بلا شاشة المشتريات لا تظهر له فواتير المشتريات ولا مرتجعاتها |
| م-480 | بادئات النطاق: «عميل أحمد» و«صنف سكر» و«فاتورة 120» تقصر النتائج، و«فاتورة مبيعات» تعرض الشاشة |
| م-481 | قائمة «الأخيرة» تظهر عند فتح اللوحة فارغة، وترتفع الشاشة المتكررة، و«مسح الأخيرة» يفرغها |
| م-482 | صف «اسأل المساعد» يرسل السؤال فى محادثة جديدة ويفتح صفحة المساعد، ولا يظهر لمن لا يملك المساعد |
| م-483 | مفتاح «/» يفتح اللوحة خارج الحقول فقط، وCtrl+K لا يعمل وحوار مفتوح |
| م-484 | إيقاف «اختصارات البحث السريع» من إعدادات الحساب يعطل Ctrl+K و«/» ويبقى الزر |
| م-485 | اللوحة باللغات الأربع والاتجاهين والوضع الداكن وشاشة الهاتف (ملء الشاشة) |
| م-486 | الكتابة السريعة لا تعرض نتائج قديمة، وتأخر البيانات يُبقى الشاشات مع سطر تنبيه |

### 9) المخاطر

- **مسح كامل على جداول كبيرة:** `LIKE '%…%'` مع `REPLACE` بلا فهرس. الضابط: حرفان، `TOP 5`، 2.5 ث، إلغاء السابق، `PaletteDataSearch`. إن تجاوز المتوسط 400 مللى: الحد الأدنى 3 أحرف للكيانات.
- **تعارض collation فى `UNION ALL`:** `COLLATE DATABASE_DEFAULT`؛ إن رُفض يُقسم إلى عبارتين (كيانات، مستندات) بالتتابع.
- **`REPLACE` تحت collation غير حساس للهمزة:** النتيجة صحيحة، لكن `ى`/`ي` و`ة`/`ه` قد تختلف بين القواعد؛ الـ harness يحسم.
- **Ctrl+K فى المتصفح:** `preventDefault` فى capture يكفى داخل الصفحة، ولا يعمل والتركيز فى شريط العنوان (مقبول).
- **إعادة اتصال الدائرة:** `init` يستبدل المرجع ولا يضيف مستمعاً ثانياً، وكل `invokeMethodAsync` داخل `try/catch`.
- **سباق Enter:** س29؛ م-486.
- **لقطة عنوان قديمة فى «الأخيرة»:** مقبول؛ `Touch` يحدّث.
- **تقييد الفرع قد يخالف شاشة قائمة الفواتير:** الفحص 4 يحسم.
- **مدخلا `AiScreenCatalog`:** يغيّران وصف `open_screen` للنموذج (سطران).
- **بناء net48:** القبول فى P8.2.1 يشترط بناء Release.

### 10) فحوص مفتوحة (أول خطوة فى الجزء المذكور)

1. **(P8.2.3)** هل `/customers/info` و`/suppliers/info` و`/items` قيم `RazorPath` فعلية (قراءة فقط)؟ إن كانت البطاقة تُفتح من `/customers/list` فقط بلا عنصر خاص وبلا فحص صلاحية إضافى: الشرط «أى من» (`/customers/info` أو `/customers/list`؛ ومثله للمورد). وإلا الشرط الصارم.
2. **(P8.2.3)** `/items/{Id:long}` هى بطاقة الصنف (`ItemEditPage`) وتقبل `Item.id`؛ وهل تفحص صلاحية التعديل.
3. **(P8.2.3)** فهرس على `Invoices_List.Invoice_Id` و`Bills_List.Invoice_Id` ونظيريهما (`sys.indexes`). بلا فهرس: تُحذف نطاقات البادئة إن تجاوز القياس 400 مللى. لا فهرس جديد على جداول النظام.
4. **(P8.2.3)** كيف تقيّد `/accounts/invoices/list` الفواتير بالفرع، ومعنى `branch_id` = 0/-1/NULL؛ يُوحَّد س14 معها.
5. **(P8.2.3)** اسم دالة قراءة `AI_Settings` فى `AiChatGateway.SafeSettings()` وثابت حالة الخطأ فى `AiToolOutcome`.
6. **(P8.2.3)** `ReadOnlySqlExecutor.Validate` يقبل `SELECT * FROM (…) AS x UNION ALL …` مع `COLLATE` و`CAST(NULL AS date)` (أول اختبار).
7. **(P8.2.3)** `UserSessionContext` ينتقل إلى `Task.Run` (قراءة `AccountModelsLib/Common/UserSessionContext.cs`).
8. **(P8.2.2)** قيم `z-index` لـ MudBlazor 9.7 وصنف الحوار المفتوح (`.mud-dialog` أو `.mud-overlay-dialog`).
9. **(P8.2.2)** هل `AccountERP.SalesRepApp` يستعمل `MainLayoutMud`؛ إن نعم فالمكوّن يتجاهل غياب الخدمة.
10. **(P8.2.5)** `SendAsync` بلا انتظار ثم التنقل لا يقطع الدورة.

### Critical Files for Implementation

- `Razors/AccountERP.SharedUI/Components/Layout/MainLayoutMud.razor`
- `Razors/AccountERP.AI/Tools/AiToolRegistry.cs` + `Tools/ReadOnlySqlExecutor.cs` + `Tools/AiScreenCatalog.cs`
- `Razors/AccountERP.SharedUI/Services/AssistantSessionService.cs` (+ `Services/AiBriefService.cs` كنمط)
- `AccountModelsLib/AlterDB_CLS/AiAssistantSchemaCLS.cs` (+ `AiReadViewsCLS.cs`)
- `Razors/AccountERP.AI/AiServiceCollectionExtensions.cs` + `Razors/AccountERP.Web/AccountERP.Web/Program.cs` + `Razors/AccountERP.MobileDesktop/MauiProgram.cs`

ملفات جديدة: `AccountModelsLib/BusinessLibShared/Ai/AiSearchModels.cs`، `AiSearchText.cs`، `AiSearchCLS.cs`؛ `Razors/AccountERP.AI/Search/AiSearchGateway.cs`؛ `Razors/AccountERP.SharedUI/Services/CommandPaletteService.cs`؛ `Razors/AccountERP.SharedUI/Components/Shared/CommandPalette.razor`؛ `Razors/AccountERP.SharedUI/wwwroot/js/palette.js`.
