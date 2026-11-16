## P8.7 — توقع التدفق النقدى (13 أسبوعاً) — خطة تفصيلية

كُتبت 2026-11-15 (Opus) بعد P8.9.5 (`eca5b25`). تحل محل مخطط P8.7 فى `ai-assistant-p8-plan.md`. القرارات و1… (و = ورقة التدفق) محسومة بالخيار الموصى به؛ قرارات P8 (ق1…ق30) ملزمة.

> ملاحظات قبل التنفيذ:
> - **الأرقام كلها محسوبة بلا نموذج.** النموذج يكتب فقرة سرد واحدة تمر على `AiBriefNarrator.Verify` (لا رقم غير موجود فى الحقائق).
> - **قراءة فقط** من بيانات النظام عبر `ReadOnlySqlExecutor`؛ الكتابة الوحيدة فى جداول `AI_*` الجديدة.
> - **ليس تقريراً محاسبياً:** توقع إدارى؛ الشاشة تقول ذلك صراحة، ولا قيود ولا ترحيل.
> - ويب فقط (الصفحة فى SharedUI، والخدمة مسجّلة فى الويب؛ MAUI يخفى القائمة).

### حقائق من الكود

- **الرصيد الافتتاحى للنقدية:** P8.1 حسم أن السيولة = الخزائن/البنوك فقط (`BoxBalance` مع قص السالب إلى 0) عبر `ai.v_cash_vouchers` (`box_id, is_bank, branch_id, currency_id, amount_equivalent, is_receipt`) و`Banks` (`Primary_balance`, `firstBalanceDate`, `Currency_Id`, `Branch_Id`). المحرك موجود فى `Brief/AiBriefCollector` (BoxTotalsPerDay/BoxBalance) ⇒ يُعاد استعماله لا نسخه.
- **المقبوضات المتوقعة من العملاء:** `AiCollectionsScorer` (P8.5) يحسب الفواتير المفتوحة بتوزيع FIFO، وتاريخ الاستحقاق = `ISNULL(due_date, invoice_date + TermsDays)` (`AiCollectSettings.TermsDays`). `v_sales.due_date` قليل الامتلاء (P8.1.1).
- **المدفوعات المتوقعة للموردين:** `ai.v_purchases` فيه `rest_value`, `is_credit`, `invoice_date` **بلا due_date** ⇒ استحقاق = تاريخ الفاتورة + `SupplierTermsDays` (إعداد جديد).
- **الشيكات:** `Checks_Memory` (`Check_No, Due_Date, Check_Value, Transaction_Type, Receipt, Spent_Date, Customer_Id, Supplier_Id, Bank_id, Branch_Id, Currency_Id, Factor, IS_Posted_Flage`) **بلا View قراءة** ⇒ View جديد `ai.v_cheques`.
- **الأقساط:** لا جداول أقساط عملاء فى النظام (البحث فى ERP_V1_3.md لم يجد إلا سلف الموظفين) ⇒ لا مصدر أقساط فى هذه المرحلة (و8).
- **المصروفات المتكررة:** لا جدول مصروفات دورية؛ تُستنتج من `ai.v_cash_vouchers` (صرف إلى حساب عام بنمط شهرى ثابت) (و6).
- **العرض:** `AiReportView`/`AiReportDialog` + `AiChart` (SVG) من P3، و`AI_SavedReports`.
- **السرد:** `AiBriefNarrator.NarrateAsync/Verify/Split/BuildPayload`، `CandidateFilter`/`TimeoutOverride` للاختبار.
- **المهام الدورية:** `IAiScheduledJob`؛ تنبيه الموجز عبر `AiBriefAlerts.Evaluate` (pure).

### القرارات (و)

| # | القرار | السبب |
|---|---|---|
| و1 | الأفق 13 أسبوعاً (أسبوع يبدأ السبت)، بالعملة المكافئة فقط، لكل الفروع المسموحة أو فرع واحد | المعيار الشائع لتوقع الخزينة (13-week cash flow) |
| و2 | الأسطر: رصيد أول الأسبوع؛ مقبوضات (عملاء آجل، شيكات واردة، مبيعات نقدية متوقعة)؛ مدفوعات (موردون آجل، شيكات صادرة، مصروفات متكررة)؛ صافى؛ رصيد آخر الأسبوع | البنية القياسية |
| و3 | سيناريوهان: **أساسى** (المقبوضات بمعدل تأخير العميل التاريخى) و**متحفظ** (المقبوضات × نسبة تحصيل تاريخية، تأخير +14 يوماً، المدفوعات فى موعدها) | يطلبه كل مدير مالى؛ ثلاث سيناريوهات تزيد التعقيد بلا فائدة |
| و4 | تأخير العميل = وسيط (أيام السداد − أيام الاستحقاق) لآخر 12 شهراً من حاسب P8.5؛ بلا تاريخ كافٍ (< 3 فواتير) ⇒ متوسط الشركة | واقعية |
| و5 | مبيعات نقدية متوقعة = متوسط المقبوضات النقدية غير المرتبطة بفواتير آجلة لآخر 8 أسابيع مماثلة (نفس يوم الأسبوع) — مع إعداد `CashflowIncludeCashSales` (1) | أكبر مصدر نقدية لمحلات التجزئة |
| و6 | مصروف متكرر = حساب عام صُرف له فى ≥ 3 من آخر 4 أشهر بتباين مبلغ ≤ 25%؛ يُتوقع فى نفس يوم الشهر (وسيط) بالوسيط | استنتاج بسيط قابل للشرح؛ قائمة قابلة للاستبعاد يدوياً |
| و7 | الشيكات: واردة `Receipt=0` لم تُحصّل بعد وتاريخ استحقاقها فى الأفق ⇒ مقبوضات؛ صادرة لم تُصرف ⇒ مدفوعات؛ المتأخرة (استحقاق < اليوم) توضع فى الأسبوع الأول بعلامة «متأخر» | ما تفعله أدوات الخزينة |
| و8 | لا أقساط ولا قروض فى هذه المرحلة؛ «تعديلات يدوية» (و9) تغطيها | لا مصدر بيانات |
| و9 | تعديلات يدوية: المستخدم يضيف سطراً (وصف، أسبوع، مبلغ، داخل/خارج) يُحفظ فى `AI_CashflowAdjustments` ويدخل الحساب | الواقع دائماً فيه ما لا يعرفه النظام |
| و10 | نتيجة الحساب تُحفظ لقطة فى `AI_CashflowRuns` (`Id, User_Id, BranchIds, AsOf, Scenario, FactsJson, Narrative, Lang, Status, CreatedAt`)؛ إعادة الحساب بضغطة، ولقطة أسبوعية تلقائية يوم السبت 07:00 للمشتركين | مقارنة توقع الأسبوع الماضى بالفعلى |
| و11 | **دقة التوقع:** بعد مرور أسبوع تُقارن لقطة الأسبوع الماضى بالفعلى (المقبوضات/المدفوعات من `v_cash_vouchers`) وتُعرض نسبة الخطأ | بناء الثقة |
| و12 | تنبيه الموجز `cashflow_low`: أول أسبوع يقل فيه الرصيد المتحفظ عن `CashflowMinBalance` (إعداد، 0 افتراضياً = موقوف) | قيمة P8.1 |
| و13 | السرد: فقرة ≤ 120 كلمة (أسوأ أسبوع، أكبر 3 عوامل، توصية عامة) بنفس مدقّق P8.1؛ فشل التحقق ⇒ بلا سرد | لا أرقام مخترعة |
| و14 | الصلاحية: شاشة جديدة `/accounts/cashflow-forecast` مع عنصر قائمة وصلاحية فى `PERMATIONS`؛ الخزائن عبر `ai_box_perm` (P8.1.1)؛ العملاء/الموردون حسب فروع المستخدم | النظام القائم |
| و15 | أداة محادثة `get_cashflow_forecast(scenario, branch?)` تعيد الحقائق (لا تعيد الحساب إن وُجدت لقطة < 6 ساعات) | استعمال من المحادثة |
| و16 | التفاصيل: كل خلية قابلة للنقر ⇒ قائمة مصادرها (فاتورة/شيك/مصروف متكرر/تعديل) مع رابط للشاشة الأصلية | قابلية التدقيق |
| و17 | إعدادات: `CashflowEnabled` (0)، `SupplierTermsDays` (30)، `CashflowMinBalance` (0)، `CashflowIncludeCashSales` (1)، `CashflowRecurringMinMonths` (3)، `CashflowConservativeDelayDays` (14) | موقوف افتراضياً |

### 1) المحرك (`Razors/AccountERP.AI/Cashflow/`)

- `AiCashflowModels` (AccountModelsLib، net48-safe): `CashflowFacts { AsOf, Weeks[13]{Start, Opening, Inflows{Customers, Cheques, CashSales, Manual}, Outflows{Suppliers, Cheques, Recurring, Manual}, Net, Closing}, Scenario, Sources[] , LowWeek? }`.
- `AiCashflowSources` (SQL عبر `ReadOnlySqlExecutor.SessionFor`): الافتتاحى (إعادة استعمال `AiBriefCollector` box balances)، الفواتير المفتوحة (`AiCollectionsScorer.Invoices` + التأخير)، الموردون (`v_purchases` + FIFO مقابل المدفوعات من `v_cash_vouchers` party_type supplier)، الشيكات (`ai.v_cheques`)، المتكرر، المبيعات النقدية.
- `AiCashflowEngine` (pure): `Build(sources, scenario, settings, asOf) → CashflowFacts` (توزيع على الأسابيع، المتأخر ⇒ الأسبوع 1، السيناريو المتحفظ).
- `AiCashflowGateway` (scoped): `Get(branch, scenario)`, `Recompute`, `Adjustments CRUD` (للمستخدم صاحبها فقط)، `Accuracy(lastRunId)`.
- `AiCashflowJob : IAiScheduledJob` (السبت 07:00).
- `AiCashflowNarrator` (غلاف رفيع حول `AiBriefNarrator`).

### 2) الواجهة

- صفحة `Pages/Accounts/CashflowForecastPage.razor` (`/accounts/cashflow-forecast`): رأس (التاريخ، الفرع، السيناريو، «إعادة الحساب»)، رسم خطى SVG للرصيد (أساسى/متحفظ + خط الحد الأدنى)، جدول 13 عموداً قابل للتمرير أفقياً (عمود الأسطر ثابت)، سرد، «تعديلات يدوية»، «دقة الأسبوع الماضى»، طباعة A4 أفقى. التصميم حسب ui-ux-pro-max ([[ui-design-skill-all-screens]]).
- `CashflowCellDialog.razor` لمصادر الخلية.
- `Settings/AiCashflowTab.razor` (الإعدادات و17 + قائمة المتكررات المستبعدة).

### 3) قاعدة البيانات (جديد فقط)

- View `ai.v_cheques`: `cheque_id, cheque_no, issue_date, due_date, amount, amount_equivalent (× Factor), is_incoming (Transaction_Type), is_collected (Receipt/Spent_Date), customer_id, supplier_id, bank_id, branch_id, currency_id, is_posted`.
- جداول: `AI_CashflowRuns`، `AI_CashflowAdjustments` (`Id, User_Id, Branch_Id, WeekStart, Direction, Amount, Description, CreatedAt, IsActive`)، `AI_CashflowExclusions` (`SubAccount_Id`, `User_Id`, `CreatedAt`).
- إعدادات و17؛ عنصر قائمة + صف `FORMS_DETAILES` حسب نمط P8.4.4/P8.5.3.

### 4) الأجزاء

- **P8.7.1 — المخطط والنواة:** View، 3 جداول، إعدادات، النماذج، `AiCashflowEngine` (pure) + harness (توزيع الأسابيع، السيناريوهان، المتأخر، المتكرر، التعديلات) ≥ 80 فحصاً.
- **P8.7.2 — المصادر والبوابة والمهمة:** `AiCashflowSources` (fake SQL كنمط P8.4.2)، البوابة، المهمة، السرد، الدقة.
- **P8.7.3 — الصفحة والقائمة والتبويب** + مفاتيح `AIW_`.
- **P8.7.4 — الأداة + تنبيه الموجز + الاختبارات** م-584…م-598.

### 5) مفاتيح الترجمة — بادئة `AIW_` (نحو 120)

### 6) اختبارات المستخدم (م-584…م-598)

- م-584 التدفق النقدى — فتح الشاشة من القائمة وظهور 13 أسبوعاً.
- م-585 التدفق النقدى — الرصيد الافتتاحى = مجموع أرصدة الخزائن المسموحة.
- م-586 التدفق النقدى — فاتورة آجلة جديدة تظهر فى أسبوع استحقاقها.
- م-587 التدفق النقدى — شيك وارد مستحق الأسبوع القادم يظهر فى المقبوضات.
- م-588 التدفق النقدى — شيك متأخر يظهر فى الأسبوع الأول بعلامة متأخر.
- م-589 التدفق النقدى — السيناريو المتحفظ أقل أو يساوى الأساسى.
- م-590 التدفق النقدى — إضافة تعديل يدوى تغيّر الأسبوع المختار فقط.
- م-591 التدفق النقدى — النقر على خلية يعرض مصادرها وروابطها.
- م-592 التدفق النقدى — استبعاد مصروف متكرر من التبويب.
- م-593 التدفق النقدى — السرد لا يحتوى رقماً غير موجود فى الجدول.
- م-594 التدفق النقدى — مستخدم بخزينة واحدة يرى رصيدها فقط.
- م-595 التدفق النقدى — طباعة A4 أفقى.
- م-596 التدفق النقدى — سؤال المحادثة «هل سيكفى النقد الشهر القادم؟» يستعمل الأداة.
- م-597 التدفق النقدى — تنبيه الموجز عند ضبط حد أدنى أعلى من الرصيد المتوقع.
- م-598 التدفق النقدى — «دقة الأسبوع الماضى» تظهر بعد لقطتين.

### 7) المخاطر

- `due_date` شبه فارغ ⇒ الاعتماد على `TermsDays`؛ يُعرض تنبيه «الاستحقاقات تقديرية» حين > 50% محسوبة.
- أرصدة خزائن سالبة مقصوصة (P8.1) قد تخفى عجزاً ⇒ ملاحظة فى الشاشة.
- تكرار مدفوعات الموردين (فاتورة سُددت بشيك مسجّل) ⇒ الشيكات المرتبطة بمورد تُخصم من المتبقى قبل التوزيع (فحص 3).

### 8) فحوص مفتوحة (أول خطوة فى P8.7.1)

1. معنى `Checks_Memory.Transaction_Type` و`Receipt` و`Spent_Date` (وارد/صادر/محصّل) من شاشات الشيكات الحالية.
2. هل `Bills_List.RestValue` يتحدث عند السداد اللاحق أم ثابت عند الحفظ؟ (يحدد FIFO مقابل `v_cash_vouchers`).
3. هل الشيك الوارد يُسجَّل أيضاً فى `Money_Transaction_Box` عند الاستلام (عدّ مزدوج)؟
4. بداية الأسبوع من إعدادات الشركة إن وُجدت، وإلا السبت.

### Critical Files

`Razors/AccountERP.AI/Brief/AiBriefCollector.cs`، `Brief/AiBriefNarrator.cs`، `Brief/AiBriefAlerts.cs`، `Collections/AiCollectionsScorer.cs`، `AccountModelsLib/AlterDB_CLS/AiReadViewsCLS.cs`، `Tools/AiToolRegistry.cs`، `Components/Assistant/AiReportView.razor`/`AiChart`.

### تم التنفيذ

- **P8.7.1 `72dc818` (2026-11-16):** View `ai.v_cheques` (أعمدة خام)، جداول `AI_CashflowRuns`/`AI_CashflowAdjustments`/`AI_CashflowExclusions`، 8 إعدادات (أُضيف `CashflowBaseDelayDays`=7 و`CashflowConservativeCollectPct`=80)، `AiCashflowModels`، `AiCashflowEngine` (pure: السبت بداية الأسبوع، المتأخر ⇒ الأسبوع 1، كشف المتكرر، FIFO)، `AiCashflowCLS`.
- **P8.7.2 `c4551c9`:** `Cashflow/AiCashflowReader` (الافتتاحى = BoxBalance × GetFactor لكل خزينة مسموحة بلا قص السالب + تنبيه؛ العملاء: رصيد `v_customer_ledger` ⇒ أحدث الفواتير الآجلة FIFO والباقى «رصيد سابق» متأخر للمدير فقط؛ الموردون: `RestValue` كما فى الشاشة + مدة الموردين؛ الشيكات: `receipt = 0 AND spent_date IS NULL`، **Transaction_Type 1 = وارد (افتراض، فحص 1 لم يُنفَّذ)**؛ المبيعات النقدية = متوسط 8 أسابيع من `v_sales is_credit = 0`)، `AiCashflowService` (لقطة 6 ساعات `Scenario = both`، الدقة، مهمة السبت 07:00 لمن له لقطة يدوية خلال 30 يوماً)، `AiCashflowNarrator`، `AiCashflowGateway`. **قرار:** تأخير العميل الأساسى إعداد ثابت (7 أيام) بدل الوسيط التاريخى لكل عميل (و4) — أبسط ويُضبط من التبويب.
- **P8.7.3 `b471954`:** الصفحة `/accounts/cashflow-forecast`، عنصر قائمة بجوار «أولويات التحصيل» عبر مساعد عام جديد `SiblingMenu`، تبويب «التدفق النقدى»، 94 مفتاح `AIW_` ×4.
- **P8.7.4:** أداة `get_cashflow_forecast` (RunAsync)، قاعدة معرفة `cashflow_forecast`، تنبيه الموجز `cashflow_low` (مجموعة `cashflow`، المتحفظ، فقط حين الحد الأدنى > 0)، مفتاحا `AIB_Al_cashflow_low` ×4.
- **الاختبارات (harness ومستخدم م-584…م-598) مؤجلة بطلب المستخدم 2026-11-16. فحوص §8 لم تُنفَّذ.**
