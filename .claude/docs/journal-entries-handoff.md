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
3. **م4-ج بقية المصادر:** ✅ المخازن (مشمولة فى م4-ب) ✅ الشيكات (أُنجز: `Account_Entry_Invoice_CLS.GuardCheck` قبل Commit فى `Order_Receipt_ChecksNewNew` للإضافة/التعديل/تغيير الحالة؛ اختبار م-178). ✅ التسويات المالية (أُنجز: `Account_Entry_Invoice_CLS.GuardTaswea` بعد `Make_EntryForMoney_Financial_Notice` فى الـ4 مواضع بـ`Money_Financial_NoticeCLS` و`..._ItemsCLS` — القيد يُنشأ دائماً بنوع 26؛ اختبار م-179). ✅ الأرصدة الافتتاحية: لا تُنشئ قيداً أصلاً (تصميم معتمد، `opening-balances-master-plan.md §3.3`) فلا حارس لها. ✅ فحص السيارات 54 (أُنجز: `GuardCarsExamination` بعد `Make_EntryForCarsExaminations` فى `CarsExaminationCLS`؛ اختبار م-180). **التالى →** بقية الوحدات: إتلاف الموردين `Make_EntryFor_Suppliers_Destruction` (sup_quantity.cs)، مستحقات `Make_EntryForMoney_DuePeriod`، مستشفى `Make_EntryForHospitalsServices`، ثم مقاولات/مندوبين إن وُجد لها قيود.
4. **م6:** تحديث `accounting-engine-deep-dive.md §8.5` (المخزون/COGS موجود الآن) و`open-work-index.md`.
5. **قراران معلّقان (يُسألان للمستخدم عند الحاجة):** (أ) تصحيح `Factor = 1` فى سطور تفاصيل القيد (الموصى به: يبقى مؤجلاً لأنه كود مشترك)؛ (ب) معنى `Financial_Period.Done/IS_Active` قبل إضافة فحص «الفترة مفتوحة».

## ملاحظات تشغيل
- قاعدة التطوير `ClientNew` على `.` بمصادقة Windows: `sqlcmd -S . -d ClientNew -E -Q "..."` للقراءة فقط.
- مفاتيح الترجمة تُضاف للـ4 ملفات `SharedUIAssemblyMarker.{ar,en,de,fr}.resx` (سكربت بايثون يُكتب بأداة Write لا heredoc).
- ربط القائمة: سطر فى `StockEngineSchemaCLS.cs` (قائمة مفتاح→مسار)؛ لا تستعمل `git add AccountModelsLib` عاماً — فى الشجرة تعديلات قديمة غير مرتبطة (AlterDB_CLS، InvoicesTotal*, Bills_ModelsClss…).
- لا اختبار حى للحفظ (يحتاج جلسة مستخدم)؛ المستخدم يختبر من الخطة.
