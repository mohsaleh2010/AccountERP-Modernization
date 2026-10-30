# تسليم خطة القيود — ابدأ من هنا بعد /clear (2026-10-30)

الخطة الأصلية: `journal-entries-unified-plan.md` · النتائج: `journal-entries-study-findings.md` · القائمة: `entries-menu-razor-plan.md` · الاختبارات: `master-user-test-plan.md` (م-154…م-179).

## ما أُنجز (commits على فرع razor-items-phases-0-5)
- م0 دراسة قراءة فقط ✅ — م1 الحارس المركزى (`EntrySourceRegistry/EntryValidator/EntryNumbering`) ✅
- م2 شاشات الإدخال الثلاث + فتح/نسخ/طباعة ✅ (`ManualEntryEditor` + `ManualEntryService`)
- م3 إنشاء قيود الصندوق آلياً ✅ (`SandoukEntriesBatchService`)
- م5 تقرير فحص السلامة ✅ كوداً: `EntryIntegrityService` + `/accounts/entries-integrity` (غير مربوط بقائمة؛ اختبار م-175)

## الأولويات المتبقية (القرارات الموصى بها مُعتمدة)
1. ✅ (أُنجز: `EntryValidator.ValidateSaved` يُستدعى من الخدمتين؛ `OrderOutcome.Warnings` ورسالة `SaveMultiVoucherInTx`؛ اختبار م-176) **م4-أ سندات الصندوق تحت الحارس (تحذيرى):** استدعاء `EntryValidator.Validate` من `CashVouchersService` و`SandoukEntriesBatchService` فقط (مسارات Razor، بلا مساس بالديسكتوب) وتسجيل المخالفات فى نتيجة العملية؛ لا رفض.
2. ✅ (أُنجز كوداً: `Guard` فى `Account_Entry_Invoice_CLS.Perpetual.cs` يستدعى `EntryValidator.ValidateSource` ويجمع المخالفات فى `EntryValidator.AutoWarnings` [ThreadStatic] بلا تغيير فى Excp/Msg؛ اختبار م-177؛ **ما بقى:** عرض AutoWarnings فى نتيجة حفظ الفاتورة فى Razor عند بناء الشاشة، وقاعدة 12 فحص المخزون تُختبر يدوياً) **م4-ب الفواتير:** مبيعات/مرتجع ثم مشتريات/مرتجع — قائمة التحقق الثابتة (ز-أ في الخطة §5 م4) + قاعدة 12 (فحص المخزون قبل/بعد). تعديل `AccountEntry_CLS` مشترك مع الديسكتوب: إضافات فقط.
3. **م4-ج بقية المصادر:** ✅ المخازن (مشمولة فى م4-ب) ✅ الشيكات (أُنجز: `Account_Entry_Invoice_CLS.GuardCheck` قبل Commit فى `Order_Receipt_ChecksNewNew` للإضافة/التعديل/تغيير الحالة؛ اختبار م-178). ✅ التسويات المالية (أُنجز: `Account_Entry_Invoice_CLS.GuardTaswea` بعد `Make_EntryForMoney_Financial_Notice` فى الـ4 مواضع بـ`Money_Financial_NoticeCLS` و`..._ItemsCLS` — القيد يُنشأ دائماً بنوع 26؛ اختبار م-179). ✅ الأرصدة الافتتاحية: لا تُنشئ قيداً أصلاً (تصميم معتمد، `opening-balances-master-plan.md §3.3`) فلا حارس لها. ✅ فحص السيارات 54 (أُنجز: `GuardCarsExamination` بعد `Make_EntryForCarsExaminations` فى `CarsExaminationCLS`؛ اختبار م-180). ✅ إتلاف الموردين 53 (أُنجز: `GuardSuppliersDestruction` بعد `Make_EntryFor_Suppliers_Destruction` فى موضعين بـ`sup_quantity.cs`؛ ملف 1256 عُدّل بايتات؛ اختبار م-181). ✅ مستحقات الفترة (أُنجز: `GuardDuePeriod` بعد `Make_EntryForMoney_DuePeriod` فى `ConstructionLib/Debit/DebitCLS.cs`؛ اختبار م-182). مستشفى `Make_EntryForHospitalsServices`: مؤجل عمداً (وحدة مضمّنة، قيد لكل سطر مع Sleep، مرجع غير موحد). **م4 انتهت — التالى → م6 (توثيق).**
4. ✅ (أُنجز 2026-10-30) **م6:** تحديث `accounting-engine-deep-dive.md §8.5` (المخزون/COGS موجود الآن) و`open-work-index.md`.
5. ✅ **القراران حُسما (2026-10-30):** (أ) `Factor`: صُحّح فى `AccountEntry_CLS.Make_AccountEntry_` — يُقرأ من عمود `Factor` لكل سطر (افتراضى 1 عند الغياب/الخطأ/≤0)؛ القيود القديمة بقيت 1 دون مساس (لا UPDATE). اختبار يدوى: حفظ قيد بعملة أجنبية ثم فحص `Account_Entry_Details.Factor`. (ب) `Financial_Period`: من `Financial_PeriodCLS` الفترة المختارة تأخذ `IS_Active=1` و`Done=Active` (0=مفتوحة) وغيرها `Done=1, IS_Active=0`؛ لكن البيانات تخالف الواقع (الفترة 5 مفتوحة/نشطة وتنتهى 2026-07-31، والفترة 6 الجارية `Done=1, IS_Active=0`) فأى فحص «الفترة مفتوحة» سيرفض قيوداً صحيحة ⇒ **أُضيف الفحص تحذيرياً فقط (PERIODCLOSED، 2026-10-30) ولا يُحوَّل لرفض** قبل أن يضبط المستخدم الفترة الجارية من شاشة الفترات.

## ملاحظات تشغيل
- قاعدة التطوير `ClientNew` على `.` بمصادقة Windows: `sqlcmd -S . -d ClientNew -E -Q "..."` للقراءة فقط.
- مفاتيح الترجمة تُضاف للـ4 ملفات `SharedUIAssemblyMarker.{ar,en,de,fr}.resx` (سكربت بايثون يُكتب بأداة Write لا heredoc).
- ربط القائمة: سطر فى `StockEngineSchemaCLS.cs` (قائمة مفتاح→مسار)؛ لا تستعمل `git add AccountModelsLib` عاماً — فى الشجرة تعديلات قديمة غير مرتبطة (AlterDB_CLS، InvoicesTotal*, Bills_ModelsClss…).
- لا اختبار حى للحفظ (يحتاج جلسة مستخدم)؛ المستخدم يختبر من الخطة.
