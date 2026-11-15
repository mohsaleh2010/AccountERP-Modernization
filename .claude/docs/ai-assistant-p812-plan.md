## P8.12 — المطابقة البنكية الذكية — خطة تفصيلية

كُتبت 2026-11-15 (Opus) بعد P8.9.5. تحل محل مخطط P8.12. «حسب طلب العملاء». القرارات ب1… محسومة بالموصى به.

> ملاحظات قبل التنفيذ:
> - **لا تعديل على حركات البنك القديمة** ولا «تسوية» تلقائية. المطابقة تُحفظ فى جداول `AI_Bank*` فقط.
> - **الفروق تُقترح كمسودة قيد** يفتحها المستخدم فى شاشة القيد ويحفظها بنفسه (نمط `AI_Drafts`).
> - الاستخراج عبر مسار P6 (`AiDocumentExtractor`) مع نوع مستند جديد؛ أو استيراد CSV/Excel مباشر بلا نموذج (الأدق).
> - ويب فقط.

### حقائق من الكود

- **البنوك والخزائن:** جدول `Banks` (`id, Bank_Name, Account_No, SubAccount_Id, Is_Bank, Currency_Id, Branch_Id, Primary_balance, firstBalanceDate, Active`) — الخزائن والبنوك فى جدول واحد بعلامة `Is_Bank`. شاشة `/main-data/banks`.
- **حركات البنك فى النظام:** `Money_Transaction_Box` (عبر `ai.v_cash_vouchers`: `box_id, is_bank, voucher_date, amount, amount_equivalent, is_receipt, party_name, notes, voucher_no, source_doc_type/id`) و`Money_Transaction_Bank` (`Bank_Id, Transaction_Date, Import_Value, Export_Value, Transaction_Value, Transaction_Type, Reference_Type, Reference_Type_Id`) — بلا View قراءة، والعلاقة بينهما فحص 1. الشيكات `Checks_Memory` (`Bank_id, Due_Date, Spent_Date, Check_No, Check_Value`) ⇒ `ai.v_cheques` من P8.7.
- **الاستخراج (P6):** `Files/AiDocumentExtractor` بأنواع `AiExtractDocTypes` (`PurchaseInvoice`, `SalesInvoice`, `ItemsList`) + `AiExtractionNormalizer` (أرقام عربية، تواريخ d/M/y، ثقة < 0.75 منخفضة، فحوص حسابية) + `AI_Attachments.ExtractJson` (مخزّن مؤقتاً). حد الملف 10 MB.
- **القيود:** `ManualEntryService` (`AccountModelsLib/BusinessLibShared/GeneralLedger/ManualEntryService.cs`)، صفحات `/accounts/journal-entry`, `/simple`, `/madin-da2n`. `AI_Drafts` يدعم `sales`, `purchase`, `store_order` فقط.

### القرارات (ب)

| # | القرار | السبب |
|---|---|---|
| ب1 | المصادر: (أ) **استيراد CSV/Excel** بتعيين أعمدة يُحفظ لكل بنك (الأدق، بلا نموذج)، (ب) **PDF/صورة كشف** عبر P6 بنوع `BankStatement` جديد | البنوك المصرية تعطى Excel/PDF |
| ب2 | بعد الاستيراد: فحص الرصيد (افتتاحى + حركات = ختامى) إلزامى؛ فشل ⇒ لا مطابقة حتى يصحح المستخدم الأسطر | جودة الاستخراج |
| ب3 | المطابقة على مراحل: (1) **دقيقة**: نفس المبلغ والاتجاه + التاريخ ± `BankMatchDays` (3) + رقم مرجع/شيك إن وُجد ⇒ ثقة عالية؛ (2) **واحد-لكثير/كثير-لواحد** بمجموع مطابق داخل نافذة 5 أيام (حد 4 عناصر)؛ (3) **النموذج** للأسطر الباقية: يُعطى السطر + أعلى 10 مرشحين ويختار أو «لا شىء» | ما تفعله Xero/QuickBooks/Odoo |
| ب4 | كل مطابقة **اقتراح** حتى يؤكدها المستخدم (تأكيد جماعى للدقيقة بضغطة) | المراجع البشرى هو المسؤول |
| ب5 | الأسطر غير المطابقة فى الكشف ⇒ «اقتراح قيد»: رسوم بنكية/فوائد/ضريبة دمغة تُكتشف بكلمات مفتاحية + النموذج ⇒ حساب مقترح (يعيد استعمال P8.8 إن نُفّذ) ⇒ مسودة قيد | أكثر الفروق شيوعاً |
| ب6 | نوع مسودة جديد `journal` فى `AI_Drafts` + صفحة القيد البسيط تقبل `?aiDraft=` (جديد فقط) | نمط P5 |
| ب7 | الأسطر غير المطابقة فى النظام ⇒ قائمة «فى الدفاتر لا فى البنك» (شيكات لم تُصرف، إيداعات بالطريق) | تقرير التسوية القياسى |
| ب8 | تقرير التسوية: رصيد البنك − شيكات معلّقة + إيداعات بالطريق = رصيد الدفاتر؛ يُطبع | المخرج المحاسبى المعروف |
| ب9 | الجداول: `AI_BankStatements (Id, Bank_Id, PeriodFrom, PeriodTo, OpeningBalance, ClosingBalance, Source ('csv'/'extract'), Attachment_Id, Status, CreatedBy, CreatedAt)`، `AI_BankStatementLines (Id, Statement_Id, LineDate, ValueDate, Description, Reference, Debit, Credit, Balance, Confidence)`، `AI_BankMatches (Id, Statement_Id, Kind ('exact'/'group'/'model'/'manual'), LinesJson, SystemRefsJson, Score, Status ('suggested'/'confirmed'/'rejected'), DecidedBy, DecidedAt)`، `AI_BankImportProfiles (Bank_Id, ColumnsJson)` | جديد فقط |
| ب10 | حركة نظام مؤكدة فى مطابقة لا تُقترح مرة أخرى فى كشف لاحق | منع التكرار |
| ب11 | الصلاحية: شاشة جديدة `/accounts/bank-reconciliation` + `PERMATIONS`؛ البنوك المسموحة عبر `ai_box_perm` | النظام القائم |
| ب12 | إعدادات: `BankRecEnabled` (0)، `BankMatchDays` (3)، `BankGroupMaxItems` (4)، `BankUseModel` (1) | موقوف افتراضياً |

### 1) الكود (`Razors/AccountERP.AI/BankRec/`)

- `AiBankImport` (CSV/Excel → أسطر؛ تعيين الأعمدة؛ تحليل التواريخ والأرقام العربية بإعادة استعمال `AiExtractionNormalizer`).
- نوع استخراج `BankStatement` فى `AiDocumentExtractor` (تعليمات + مخطط JSON + فحص الرصيد).
- `AiBankMatcher` (pure): المراحل 1 و2، النتيجة مرتبة.
- `AiBankModelMatcher` (النموذج للمرحلة 3، مرشحون فقط).
- `AiBankRecGateway`: استيراد، مطابقة، تأكيد/رفض، يدوى، اقتراح قيود، تقرير التسوية.
- `ai.v_bank_movements` (View جديد يوحّد حركات البنك من المصادر بعد فحص 1).

### 2) الواجهة

- `Pages/Accounts/BankReconciliationPage.razor`: اختيار بنك وفترة ← رفع ملف/تعيين أعمدة ← عرض جنباً إلى جنب (الكشف | الدفاتر) مع ألوان (مؤكد/مقترح/غير مطابق)، تأكيد جماعى، مطابقة يدوية بالسحب أو التحديد، «اقتراح قيد» لكل سطر غير مطابق، تقرير التسوية للطباعة. شاشة ضيقة ⇒ تبويبان بدل عمودين.

### 3) الأجزاء

- **P8.12.1 — المخطط والاستيراد:** الجداول، الإعدادات، الـ View، `AiBankImport` + harness (CSV عربى/إنجليزى، فحص الرصيد).
- **P8.12.2 — المطابقة:** `AiBankMatcher` + النموذج + البوابة + harness (دقيق، تجميع، حدود، منع التكرار).
- **P8.12.3 — الاستخراج من PDF/صورة** (نوع `BankStatement`).
- **P8.12.4 — الشاشة + مسودة القيد (`journal`) + تقرير التسوية + مفاتيح `AIN_` + الاختبارات** م-636…م-652.

### 4) اختبارات المستخدم (م-636…م-652)

- م-636 المطابقة البنكية — فتح الشاشة واختيار بنك وفترة.
- م-637 المطابقة البنكية — استيراد كشف Excel وتعيين الأعمدة وحفظ التعيين.
- م-638 المطابقة البنكية — كشف لا يتزن رصيده يمنع المطابقة برسالة واضحة.
- م-639 المطابقة البنكية — إيداع بنفس المبلغ والتاريخ يُطابق تلقائياً كاقتراح دقيق.
- م-640 المطابقة البنكية — إيداع واحد يقابل ثلاث حركات بنفس المجموع.
- م-641 المطابقة البنكية — تأكيد جماعى للمطابقات الدقيقة.
- م-642 المطابقة البنكية — رفض مطابقة يعيد السطرين لغير المطابق.
- م-643 المطابقة البنكية — مطابقة يدوية بين سطرين.
- م-644 المطابقة البنكية — رسوم بنكية غير مطابقة تقترح مسودة قيد.
- م-645 المطابقة البنكية — فتح مسودة القيد وحفظها من شاشة القيد.
- م-646 المطابقة البنكية — شيك صادر لم يُصرف يظهر «فى الدفاتر لا فى البنك».
- م-647 المطابقة البنكية — تقرير التسوية يتزن ويطبع.
- م-648 المطابقة البنكية — كشف PDF يُستخرج وتظهر الأسطر منخفضة الثقة مميزة.
- م-649 المطابقة البنكية — حركة مؤكدة لا تُقترح فى كشف الشهر التالى.
- م-650 المطابقة البنكية — مستخدم بلا صلاحية البنك لا يراه.
- م-651 المطابقة البنكية — النموذج يقترح مرشحاً من القائمة فقط.
- م-652 المطابقة البنكية — إيقاف الميزة يخفى القائمة.

### 5) المخاطر

- ازدواج مصادر الحركات (`Money_Transaction_Box` و`Money_Transaction_Bank`) ⇒ فحص 1 قبل أى كود.
- كشوف PDF الممسوحة رديئة ⇒ CSV/Excel هو المسار الموصى به فى الشاشة.

### 6) فحوص مفتوحة (أول خطوة فى P8.12.1)

1. العلاقة بين `Money_Transaction_Box` (بنك) و`Money_Transaction_Bank` و`Checks_Memory`: أى منها مصدر حركة البنك الفعلية بلا تكرار.
2. واجهة حفظ القيد فى `ManualEntryService` لبناء مسودة `journal` وإعادة فتحها فى `JournalEntrySimplePage`.
3. هل توجد حزمة قراءة Excel فى الحل (ClosedXML/ExcelDataReader) أم CSV فقط فى البداية.

### Critical Files

`Razors/AccountERP.AI/Files/AiDocumentExtractor.cs`، `Files/AiExtractionNormalizer.cs`، `Tools/AiDraftTools.cs`، `AiDraftsCLS`، `AccountModelsLib/BusinessLibShared/GeneralLedger/ManualEntryService.cs`، `Pages/Accounts/JournalEntrySimplePage.razor`، `AiReadViewsCLS.cs`.

### تم التنفيذ

(فارغ)
