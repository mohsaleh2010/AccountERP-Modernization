# تسليم خطة القيود — ابدأ من هنا بعد /clear (2026-10-30)

الخطة الأصلية: `journal-entries-unified-plan.md` · النتائج: `journal-entries-study-findings.md` · القائمة: `entries-menu-razor-plan.md` · الاختبارات: `master-user-test-plan.md` (م-154…م-175).

## ما أُنجز (commits على فرع razor-items-phases-0-5)
- م0 دراسة قراءة فقط ✅ — م1 الحارس المركزى (`EntrySourceRegistry/EntryValidator/EntryNumbering`) ✅
- م2 شاشات الإدخال الثلاث + فتح/نسخ/طباعة ✅ (`ManualEntryEditor` + `ManualEntryService`)
- م3 إنشاء قيود الصندوق آلياً ✅ (`SandoukEntriesBatchService`)
- م5 تقرير فحص السلامة ✅ كوداً: `EntryIntegrityService` + `/accounts/entries-integrity` (غير مربوط بقائمة؛ اختبار م-175)

## الأولويات المتبقية (القرارات الموصى بها مُعتمدة)
1. **م4-أ سندات الصندوق تحت الحارس (تحذيرى):** استدعاء `EntryValidator.Validate` من `CashVouchersService` و`SandoukEntriesBatchService` فقط (مسارات Razor، بلا مساس بالديسكتوب) وتسجيل المخالفات فى نتيجة العملية؛ لا رفض.
2. **م4-ب الفواتير:** مبيعات/مرتجع ثم مشتريات/مرتجع — قائمة التحقق الثابتة (ز-أ في الخطة §5 م4) + قاعدة 12 (فحص المخزون قبل/بعد). تعديل `AccountEntry_CLS` مشترك مع الديسكتوب: إضافات فقط.
3. **م4-ج بقية المصادر** حسب ترتيب الخطة (مخازن، شيكات، تسويات، أرصدة افتتاحية…).
4. **م6:** تحديث `accounting-engine-deep-dive.md §8.5` (المخزون/COGS موجود الآن) و`open-work-index.md`.
5. **قراران معلّقان (يُسألان للمستخدم عند الحاجة):** (أ) تصحيح `Factor = 1` فى سطور تفاصيل القيد (الموصى به: يبقى مؤجلاً لأنه كود مشترك)؛ (ب) معنى `Financial_Period.Done/IS_Active` قبل إضافة فحص «الفترة مفتوحة».

## ملاحظات تشغيل
- قاعدة التطوير `ClientNew` على `.` بمصادقة Windows: `sqlcmd -S . -d ClientNew -E -Q "..."` للقراءة فقط.
- مفاتيح الترجمة تُضاف للـ4 ملفات `SharedUIAssemblyMarker.{ar,en,de,fr}.resx` (سكربت بايثون يُكتب بأداة Write لا heredoc).
- ربط القائمة: سطر فى `StockEngineSchemaCLS.cs` (قائمة مفتاح→مسار)؛ لا تستعمل `git add AccountModelsLib` عاماً — فى الشجرة تعديلات قديمة غير مرتبطة (AlterDB_CLS، InvoicesTotal*, Bills_ModelsClss…).
- لا اختبار حى للحفظ (يحتاج جلسة مستخدم)؛ المستخدم يختبر من الخطة.
