# سجل العمل التشغيلى (Work log)

> يُحدَّث **قبل** كل خطوة طويلة/خطرة (ماذا، الملفات، طريقة التراجع) و**بعدها** (النتيجة) — لأن الكهرباء تنقطع ويُطفأ الجهاز.
> الإضافة فى آخر الملف فقط. التفاصيل الكاملة فى ملفات الخطط (inventory-0x).
> عند الاستئناف بعد انقطاع: اقرأ آخر سطر هنا، ثم `git status`، ثم الملفات المذكورة فى آخر خطوة.

## 2026-10-09

| الوقت | الخطوة | الحالة | التراجع / ملاحظات |
|---|---|---|---|
| — | خطة 01 م1: سكربت المحرك على ClientNew + StockMovementService | ✅ commit bd222b8 | rollback فى ذيل stock-engine-phase1-schema.sql |
| — | خطة 02 م1-3: تنظيف أول المدة (بلا حذف الأيتام) + ItemOpeningBalanceService | ✅ commit 8fc0476 | نسخة Item_First_Period_Balance_Backup_20261009 |
| — | خطة 03 م1: جدول Store_Order_Type_Settings (22 نوعاً) على ClientNew + StoreOrderService | ✅ commit 1d7db66 | `DROP TABLE dbo.Store_Order_Type_Settings` |
| — | فلتر المسودات على 16 ملفاً (قراءات قديمة) | ✅ على القرص، **بلا commit** | نسخ قبل التعديل فى `%TEMP%\filterbak\`؛ أو `git checkout -- <file>` |
| — | حماية الشاشات القديمة: StoreOrderLegacyGuard.cs + 7 دوال + FirstBalanceCLS | ✅ على القرص، بلا commit | حذف الملف الجديد + `git checkout` للملفات |
| — | ترقية قواعد العملاء: StockEngineSchemaCLS.cs + استدعاء فى AlterDB(d) وProgram.cs للنسختين | ✅ على القرص، بلا commit؛ مختبر على قاعدة مؤقتة EngineUpgradeTest (حُذفت) | حذف الملف + `git checkout` لـ AlterDB_CLS.cs وProgram.cs ×2 |
| — | إصلاح AccountCommonDataLIB.csproj: ConnectCLS المحلى بدل رابط `SIT_Projects 2.0`، وBouncyCastle 1.9.0 + Pkcs11Interop 5.1.2 كحزم NuGet | ✅ على القرص، بلا commit | `git checkout -- AccountCommonDataLIB/AccountCommonDataLIB.csproj` |
| — | بناء Store_LIB (يشمل AccountCommonDataLIB/Connection4It) | ✅ 0 أخطاء | — |
| — | بناء Reporting_System + AccountsERPSystem + AccountsERPSystemNet10 | ⛔ انقطع (انطفاء الجهاز) — يُعاد | المخرجات فى `%TEMP%\wfbuild_*` فقط، لا أثر على المشروع |

### حالة git الآن
- commits اليوم: bd222b8، 8fc0476، 1d7db66.
- **غير مُسلَّم (بانتظار بناء VS حسب قرارك):** فلتر المسودات + الحماية + الترقية + إصلاح csproj (القائمة فى inventory-03 §7).
- ملفات جلسة أخرى لا تُلمس: InvoicesReverseTotal.cs، InvoicesTotal*.cs، CashVouchersService.cs، Money_Transaction_BoxOrderReportCLS.cs، HMIS/*.

### الخطوة التالية
- إعادة بناء Reporting_System ثم AccountsERPSystem ثم AccountsERPSystemNet10 (كل واحد خطوة مستقلة مسجلة هنا).

### الخطوات (تُضاف تلقائياً)
| الوقت | الخطوة | الحالة | التراجع / ملاحظات |
|---|---|---|---|
| 07:15 | بناء Reporting_System | ⏳ بدأ | مخرجات %TEMP%\wfbuild_Reporting_System |
| 07:15 | بناء Reporting_System | Buildsucceeded.  0 Error(s) | — |
| 07:15 | بناء AccountsERPSystem | ⏳ بدأ | مخرجات %TEMP%\wfbuild_AccountsERPSystem |
| 07:22 | بناء AccountsERPSystem | ⛔ انقطع 07:16 (انطفاء) | يُعاد |
| 07:22 | بناء AccountsERPSystem (إعادة) | ⏳ بدأ | سجل %TEMP%\b_AccountsERPSystem.log |
| 07:24 | بناء AccountsERPSystem (إعادة) | Build FAILED. 26 Error(s) | — |
| 07:25 | بناء AccountsERPSystem | سبب الفشل: Accounts.csproj يشير لـ DevComponents.DotNetBar2.dll فى مجلد SIT_Projects 2.0 غير الموجود (ليس من تعديلاتنا) | فحص كل المشاريع |
| 07:25 | فحص المراجع الخارجية | داخل الحل: Accounts→DevComponents.DotNetBar2 14.1.0.37، BarCodeQR_GeneratorLIB→Neodynamic.SDK.Barcode 6.0.2018.508 (نسخ محلية موجودة فى bin) | بانتظار قرار المستخدم |
| 07:29 | قرار: إيقاف البناء هنا | المستخدم يبنى فى Visual Studio على الجهاز الآخر؛ لا تغيير لـ Accounts.csproj ولا BarCodeQR_GeneratorLIB.csproj | — |
| 07:29 | الحالة النهائية للجلسة | مبنى هنا: AccountModelsLib، Store_LIB (+AccountCommonDataLIB، Connection4It)، Reporting_System — 0 أخطاء. لم يُبنَ: Accounts والنسختان (مراجع SIT_Projects 2.0). بلا commit حتى بناء VS | الخطوة التالية: بناء الحل فى VS ثم commit للملفات المذكورة فى inventory-03 §7 |
| 07:32 | فحص المطابقة (قراءة فقط) | ClientNew: 1398 سطر مخزون، 2 برصيد، 5 فقط مخزَّن≠الدفتر، 0 سالب | المطابقة ليست عائقاً هنا؛ قد تختلف عند العملاء |
| 07:36 | طلب: استئناف بناء الديسك توب باستكمال الملفات من E:\MohSaleh\MyWorkAll\SIT_Projects.Net\files | ⏳ فحص المجلد | — |
| 07:36 | نسخ DevComponents.DotNetBar2.dll (7.3.0.1 من مجلد files) إلى المسار الذى يتوقعه Accounts.csproj داخل E:\MohSaleh\MyWorkAll\SIT_Projects 2.0 (خارج المستودع) ثم بناء Accounts | ⏳ | التراجع: حذف مجلد E:\MohSaleh\MyWorkAll\SIT_Projects 2.0 (أُنشئ الآن) |
| 07:39 | بناء Accounts (DotNetBar 7.3.0.1) | Build succeeded. 0 Error(s) | سجل %TEMP%\b_Accounts.log |
| 07:40 | بناء AccountsERPSystem | ⏳ بدأ | سجل %TEMP%\b_AccountsERPSystem.log |
| 07:42 | بناء AccountsERPSystem | Build succeeded. 0 Error(s) | — |
| 07:42 | بناء AccountsERPSystemNet10 | ⏳ بدأ | سجل %TEMP%\b_AccountsERPSystemNet10.log |
| 07:45 | بناء AccountsERPSystemNet10 | Build FAILED. 1 Error(s) | — |
| 07:45 | بناء AccountsERPSystem / Net10 | AccountsERPSystem ✅ 0 أخطاء؛ Net10 ⛔ 1: AccountERP.SharedUI.csproj سطر 19 يربط app.css من ..\..\..\Backup\Razors\… غير الموجود | فحص البديل المحلى |
| 07:45 | تعديل Razors/AccountERP.SharedUI/AccountERP.SharedUI.csproj سطر 19: رابط app.css إلى ..\AccountERP.MobileDesktop\wwwroot\app.css (نفس الملف داخل المستودع) | ⏳ | التراجع: git checkout -- Razors/AccountERP.SharedUI/AccountERP.SharedUI.csproj |
| 07:45 | تعديل رابط app.css | ✅ تم ( 1 file changed, 1 insertion(+), 1 deletion(-)) | — |
| 07:45 | بناء AccountsERPSystemNet10 (إعادة) | ⏳ بدأ | سجل %TEMP%\b_AccountsERPSystemNet10.log |
| 07:46 | نسخ Neodynamic.SDK.Barcode.dll من مجلد files إلى E:\MohSaleh\MyWorkAll\SIT_Projects 2.0\packages (المسار الذى يتوقعه BarCodeQR_GeneratorLIB.csproj) | ⏳ | التراجع: حذف الملف/مجلد SIT_Projects 2.0 |
| 07:46 | نسخ Neodynamic | ✅ | — |
| 07:47 | بناء AccountsERPSystemNet10 (إعادة) | Build succeeded. 0 Error(s) | — |
| 07:47 | بناء BarCodeQR_GeneratorLIB (بـ Neodynamic الحقيقية) | ⏳ بدأ | — |
| 07:47 | بناء BarCodeQR_GeneratorLIB | Build succeeded. 0 Error(s) (تحذيرات Neodynamic: 0) | — |
| 07:47 | ✅ الحل يُبنى بالكامل على هذا الجهاز | AccountModelsLib، Store_LIB، Reporting_System، Accounts، BarCodeQR_GeneratorLIB، AccountsERPSystem، AccountsERPSystemNet10 — كلها 0 أخطاء | الخطوة التالية: commit (بانتظار الموافقة) |
| 23:00 | commit | ✅ 424b8ff (فلتر المسودات + الحماية + ترقية القواعد، 25 ملفاً) و465fe7c (إصلاح csproj ×2) | git revert <hash> |
| 23:19 | القسم كجهة رصيد فى StoreOrderService (م-03-9) + قرارا م-03-10/11 فى الخطة | ✅ اختبار 12/12 داخل معاملة ملغاة، بلا commit | git checkout -- AccountModelsLib/BusinessLibShared/Store/StoreOrderService.cs |
| 23:53 | المرحلة 2: StoreOrderPageService + StoreOrderPage (تصميم الصفين) + إصلاح Log | ✅ اختبار متصفح (1004 مسودة/ترحيل، 1013 ترحيل/عكس) ونُظفت البيانات؛ بلا commit | حذف الملفين الجديدين + git checkout لـ StoreOrderService.cs وملفات resx |
| 00:08 | تصميم احترافى لصفحة الإذن (إصلاح تحميل InvoiceStyles + سطر إدخال + الرصيد بعد) | ✅ مُراجَع من المتصفح؛ بلا commit | git checkout للملفات |

## 2026-10-10 — تفويض تنفيذ ذاتى (طلب المستخدم: البدء بعد 50 دقيقة، اختيار التوصية عند أى قرار، الانتقال للخطة التالية تلقائياً، وتسجيل كل شىء للمراجعة)

### قائمة التنفيذ بالترتيب
1. إكمال نقل ملاحظات الإذن أسفل جدول الأصناف (بُنى بلا أخطاء، يُراجَع فى المتصفح) ثم commit لكل شغل المرحلة 2 (الصفحة + الخدمة + إصلاح السجل + الترجمة + التصميم).
2. ربط عناصر القائمة (SysConfigMenus.RazorPath) للأنواع البسيطة الـ 18 بالصفحة الجديدة (قرار المستخدم).
3. خطة 03 المرحلة 4: قائمة وبحث الأذون (قرار المستخدم).
4. بعدها حسب خطة 03 §5: طباعة الإذن، ثم المرحلة 3 (التحويل) بالتوصيات، ثم ما يليها فى خريطة الطريق inventory-00.
- القاعدة: قبل كل خطوة طويلة سطر «بدأ» وبعدها النتيجة هنا؛ أى بيانات اختبار تُنظَّف؛ لا لمس لملفات الجلسة الأخرى.

| الوقت | الخطوة | الحالة | التراجع / ملاحظات |
|---|---|---|---|
| 04:43 | انتظار 50 دقيقة قبل البدء (طلب المستخدم) | ⏳ | — |
| 04:44 | قاعدة دائمة من المستخدم: كل الشاشات تتبع ui-ux-pro-max (حُفظت فى الذاكرة) | ✅ | — |
| 04:45 | قاعدة دائمة من المستخدم: أى اختيار/قرار يُحسم طبقاً للأنظمة المتقدمة الحديثة، ويُسجَّل السبب هنا وفى جدول قرارات الخطة (حُفظت فى الذاكرة) | ✅ | — |
| 04:48 | المستخدم ألغى الانتظار؛ بدء التنفيذ الآن | ▶ | — |
| 04:49 | خطوة 1: الملاحظات أسفل جدول الأصناف — تحقق متصفح ✅ | ✅ | — |
| 04:53 | خطوة 2: ربط 19 عنصر قائمة (18 نوعاً) بـ /store/order/{type} عبر RazorPath — خطوة LinkStoreOrderMenus فى StockEngineSchemaCLS (تملأ الفارغ فقط) وتشغيلها على ClientNew | ✅ 19 صفاً فقط تغيّرت (مقارنة قبل/بعد)؛ القائمة فى المتصفح تفتح الصفحة؛ التحويلات الأربعة باقية «قريباً» | التراجع: UPDATE SysConfigMenus SET RazorPath=NULL للمعرّفات 532-543,540,541,693,852,853,862,863,868,869 |
| 19:01 | خطوة 3 (خطة 03 م4): StoreOrdersList.razor (/store/orders) + SearchOrders/CountByStatus/GetOrderTypes فى StoreOrderPageService + قرار: عرض الأذون القديمة بالأرقام القصيرة 1..23 كأنواعها 1001..1023 (للعرض فقط) + الفترة الافتراضية آخر 30 يوماً + العكس للأذون المرحَّلة بالمحرك فقط | ✅ مختبر (قراءة فقط على 19 إذناً + المتصفح) | حذف الملف الجديد + git checkout للخدمة |
| 19:08 | قرارات خطوة 3 (حسب الأنظمة الحديثة): (1) الأذون القديمة بالأرقام القصيرة تظهر فى القائمة وتُفتح للقراءة بشريحة «إذن قديم»؛ (2) أسطر الأذون القديمة التى حُذف سطر مخزونها تظهر «صنف محذوف (سطر مخزون N)» بدل إخفائها؛ (3) ربط 15 عنصر «تقارير الأذونات» + «تقرير قائمة إذونات المخازن» بالقائمة الجديدة، مع تصحيح عنصرين فى القديم (879 كان يفتح 1022 بدل 1023، و881 بلا شاشة) | ✅ 15 صفاً فقط تغيّرت فى SysConfigMenus | UPDATE SysConfigMenus SET RazorPath=NULL WHERE Id IN (794,796-805,878-881) |
| 19:12 | خطوة 4: طباعة الإذن من الصفحة (طباعة المتصفح + «حفظ كـ PDF» بنفس أسلوب CustomerDataPrintReport): رأس الشركة، الرقم/التاريخ/الحالة، من/إلى، الأسطر بالمجاميع، الملاحظات، 3 توقيعات، علامة «مسودة» للمسودة | ✅ معاينة المتصفح لمسودة اختبار ثم حُذفت (0 صفوف، الأرصدة صفر) | git checkout للصفحة |
| 19:17 | خطة 01 م7 + خطة 03 م3 (بدأ): StoreTransferPolicy (إعداد Store_Transfer_Mode 0/1/2 + صلاحية متقدمة StoreTransferDirectWithoutReceipt تحت 321) + أعمدة Transfer_Status/Source_Order_Id/Transfer_Close_Reason على Order_Purshing وSource_Detail_Id على Detailes_Purshing + ReceiveTransfer/CloseTransferShortage/GetTransferProgress؛ تطبيق على ClientNew | ✅ | الأعمدة الجديدة تقبل NULL؛ التراجع: DELETE من SysConfig/SysConfigMenus للمفتاحين + DROP للأعمدة الأربعة والفهرس IX_Order_Purshing_Source_Order |
| 19:31 | خطة 03 م3 — اختبار الخدمة داخل معاملة ملغاة | ✅ 19/19 (شحن، فى الطريق، استلام جزئى/كامل، رفض الزيادة والعكس قبل عكس الاستلام وترحيل 1016 يدوياً، إغلاق بعجز ثم إعادة الفتح بعكس الاستلام، مباشر) | — |
| 19:31 | إصلاح المحرك: السطر الجديد (مستلم تحويل) يأخذ متوسط فرعه حتى لو لم يتغير المتوسط (كان يبقى صفراً) | ✅ | git checkout لـ StockMovementService.cs |
| 19:31 | اختبار المتصفح (الإعداد 2 مؤقتاً): شحنة 6 ← فى الطريق (المرسل 4) ← استلام 4 (إذن 1016 مرتبط، مخزن خامات 4 بتكلفة 15) ← إغلاق الباقى بعجز بسبب؛ ثم حُذفت كل بيانات الاختبار وأُعيد الإعداد إلى 1 (مباشر) كما كان | ✅ | — |
| 19:31 | قرارات (حسب الأنظمة الحديثة): (1) الإعداد الجديد يأخذ قيمة القديم (ClientNew = 1 مباشر) فلا يتغير سلوك أى عميل؛ (2) 1016 يُنشأ فقط من استلام الشحنة، و1017 بلا ربط (الشحنة تغطيه)؛ (3) «إذن صرف لقسم» = 1001 إلى قسم و«إذن إستلام من قسم» = 1006 من قسم (كانا بلا شاشة)؛ (4) «إذن إستلام من المخزن» يفتح قائمة الشحنات بانتظار الاستلام؛ (5) العجز يُغلق بسبب بلا حركة مخزون (الكمية خُصمت عند الشحن) | ✅ | — |
| 19:38 | خطة 03 م5: أرصدة الأصناف والتقييم — StoreBalancesService + StoreBalancesList.razor (/store/balances?price=&type=&asof=) + ربط 9 عناصر قائمة (767، 755، 756، 875، 765، 913، 766، 759، 527) | ✅ قراءة فقط؛ تحقق المتصفح (3×98 + 12×899 = 11,082) | حذف الملفين + UPDATE SysConfigMenus SET RazorPath=NULL للمعرّفات التسعة |
| 19:38 | قرارات م5: (1) «كمية وقيمة» و«كمية ومتوسط تكلفة» = التقييم بمتوسط تكلفة الفرع (المعتمد فى المحرك)؛ (2) الرصيد بتاريخ من دفتر الحركة مع تنبيه أن حركات الشاشات القديمة بلا دفتر تظهر فرقاً (يُعالَج فى مطابقة الأرصدة، خطة 01 م3)؛ (3) أسطر Unit_item اليتيمة بلا صنف لا تُعرض؛ (4) مؤجَّل: أرصدة لدى الموظفين (لا تُحفظ كرصيد) وبتاريخ الصلاحية (يحتاج تتبع التشغيلات) | ✅ | — |
| 19:43 | خطة 01 م3: مطابقة أرصدة الأصناف — GetReconciliation/AcceptStoredBalances فى StoreBalancesService + StockMovementService.PostLedgerCorrection (عملية 901) + StoreReconciliationPage.razor (/store/reconciliation) + ربط 555 و935 | ✅ اختبار داخل معاملة ملغاة (التصحيح يجعل المحسوب = المخزَّن بلا تغيير للرصيد، والثانى لا شىء)؛ تحقق المتصفح | حذف الملفين + git checkout للخدمتين + RazorPath=NULL لـ 555 و935 |
| 19:43 | قرار (حسب الأنظمة الحديثة، تصحيح لخطة 01 §2.4): «إذن التسوية» يحرك الرصيد والدفتر معاً فلا يزيل الفرق؛ لذا إجراءان: اعتماد المخزَّن = صف تصحيح فى الدفتر فقط (901) بسبب، والفرق الفعلى فى الجرد = إذن تسوية 1007/1008 | ✅ | — |
| 19:43 | ⚠ للمراجعة: ClientNew فيها 4 أسطر بفرق (أصناف مطرز فريزيه فى مخزنى الإنتاج: +96، +79، +20، −3). لم أصحّحها — القرار لك (اعتماد المخزَّن أو جرد فعلى) من شاشة المطابقة | ⏸ | — |
| 19:48 | خطة 01 م4: كارت الصنف الموحَّد — StoreItemCardService + StoreItemCardPage.razor (/store/item-card?line=&type=20&value=1) + ربط 545، 546، 554، 882 | ✅ قراءة فقط؛ اختبار على سطرين حقيقيين (171 حركة، الرصيد الجارى = المحسوب)، والمتصفح (رصيد أول/وارد/منصرف/آخر + تنبيه الفرق مع رابط المطابقة) | حذف الملفين + RazorPath=NULL للمعرّفات الأربعة |
| 19:54 | خطة 03 م6: تقارير الصرف والوارد + الاحتياجات — StoreMovementReportService (GetSummary، GetNeeds) + StoreMovementReport.razor (/store/reports/movements?type=) + StoreNeedsPage.razor (/store/needs) + ربط 23 عنصر قائمة | ✅ اختبار داخل معاملة ملغاة (وارد 10 بقيمة 150، منصرف 3 بقيمة 45، احتياج 13 بآخر شراء 15)؛ تحقق المتصفح | حذف الملفات الثلاثة + RazorPath=NULL للعناصر الـ 23 |
| 19:54 | ملاحظة بيانات ClientNew: صفوف الدفتر المرتبطة بالأذون (21) تشير لأسطر مخزون محذوفة، فالتقرير فارغ على هذه القاعدة؛ حد الطلب مضبوط لسطر واحد فقط | ℹ | — |
| 23:34 | خطة 03: نسخ أصناف مخزن لآخر برصيد صفر — StoreOrderService.CopyItemsToStore (معاينة/تنفيذ) + StoreCopyItemsPage.razor (/store/copy-items) + ربط 556 | ✅ اختبار داخل معاملة ملغاة (122 معاينة = 122 منشأ، ثم 0، الأرصدة صفر، رفض نفس المخزن)؛ تحقق المتصفح (معاينة 76 بلا تنفيذ) | حذف الصفحة + git checkout + RazorPath=NULL لـ 556 |
| 00:20 | خطة 04 م1 (جزء): القوائم المرجعية العشر — ItemLookupService (عرض/إضافة/تعديل يحدّث نسخة الاسم فى Item/إيقاف/حذف ممنوع إن كانت مستعملة) + ItemLookupsPage.razor (/items/lookups/{kind}) + ربط 10 عناصر قائمة (496-498، 814-818، 845، 846) | ✅ اختبار داخل معاملة ملغاة (ملاحظة: عدّاد الهوية لـ Items_Nature وItems_Group تقدّم 1 — بلا أثر)؛ تحقق المتصفح (132 لوناً؛ بعض الألوان القديمة بأسماء فارغة ومستعملة فتظهر بلا حذف) | حذف الملفين + RazorPath=NULL للعناصر العشرة |
| 00:55 | خطة 04 م1 (إكمال): ItemUnitCategoryService + ItemUnitsPage.razor (/items/units) + ItemCategoriesPage.razor (/items/categories) + ربط 493، 494، 495 + 43 مفتاح ترجمة | ✅ اختبار 20/20 داخل معاملة ملغاة؛ 3 صفوف فقط تغيّرت فى SysConfigMenus؛ تحقق المتصفح (7 وحدات بالتحويل، 8 أنواع و50 فئة مع التصفية). ملاحظة: الشاشة القديمة لإضافة الفئة لا تحدد ID والعمود ليس هوية على ClientNew فكانت ستفشل — الجديدة تولّده. وحدتان قديمتان (17، 33) بنفس الاسم «العدد» تبقيان قابلتين للتعديل | حذف 3 ملفات + RazorPath=NULL لـ 493-495 |
| 00:55 | قرارات م-04-6 وم-04-7 (الوحدات والفئات) مسجلة فى خطة 04 | ✅ | — |
