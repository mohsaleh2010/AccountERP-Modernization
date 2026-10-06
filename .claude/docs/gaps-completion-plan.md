# خطة استكمال النواقص (2026-10-02)

**المصدر:** مراجعة كل ملفات `.claude/docs` (خطط القوائم، `razor-vs-desktop-fixes-backlog`، `winforms-porting-checklist`، `opening-balances-master-plan`، `edit-audit-columns-retrofit-plan`، `testing-plan`، `open-work-index`) مقابل حالة الكود الفعلية. ما كان مجرد قرار تأجيل صريح من المستخدم مُدرَج فى §6 ولا يُنفَّذ.

**قاعدة العمل:** لا تعديل لكود الديسكتوب/المشترك (`AccountModelsLib`) إلا بموافقة على كل دفعة؛ كل بند يُنفَّذ ثم يُبنى ويُختبر حياً ويُحدَّث هنا وفى `testing-plan.md`.

---

## 1) خلاصة الوضع

| المحور | الوضع |
|---|---|
| شاشات الرازور المبنية | قوائم «بيانات أساسية» و«الحسابات» و«المبيعات» و«المشتريات» و«الخزينة» و«الطلبيات» مبنية (بقرارات تأجيل موثَّقة §6). |
| الاختبار اليدوى المنهجى | 117 اختباراً: ✅0، ❌1 (اختبار 7)، 🔁6 (1،2،3،5،6،8 تحتاج إعادة)، ⬜110 لم تُختبر. |
| تدقيق أعمدة التعديل | **غير مكتمل**: صفوف الطبقة ب ما زالت بلا `Edit_User_Id` (تحققت: `BillsCLS`، `ItemCLS`، `FixedAssetCLS`، `CostCenterCLS`، `Submain_AccountCLS`، `Add_Account_CodesCLS`، `Assets_EhlakCLS`، `GroupsCLS`، `CashPaymentMethodCLS`، `SalesTotalInvoicesService` = 0 ظهور). المطبَّق فعلاً: 23 كلاس فقط (الطبقة أ + `SalesMenCLS` + `Order_MoneyCLS` وغيرها). |
| أرصدة أول المدة | المرحلة 1 «قيد التنفيذ» ولا دليل فى الكود على اكتمالها؛ المراحل 2-6 لم تبدأ. |
| تنظيف الشجرة | تم (14 commit)؛ بقى `HMIS` وإعدادات `.claude` بلا commit عمداً. |

---

## 2) النواقص مرتَّبة بالأولوية

### أولوية 1 — أخطاء مالية/بيانات حقيقية (تحتاج قراراً ثم تنفيذاً)

| # | البند | المرجع | ما المطلوب | يحتاج موافقة؟ |
|---|---|---|---|---|
| 1.1 | **تدقيق أعمدة التعديل — الطبقة ب** (24 كلاساً مرشَّحاً) | `edit-audit-columns-retrofit-plan.md` | لكل دالة تعديل: التحقق أن الجدول يملك `Edit_User_Id`/`EditOperationDate`، ثم ضبطهما معاً بـ`Common.User_Id` والوقت، وبناء + اختبار تعديل حى. تُنفَّذ بدفعات (4-5 كلاسات). | نعم لكل دفعة (كود مشترك) |
| 1.2 | `Update_NewFixedAsset` يحذف بيانات الأصل بدل تحديثها (الديسكتوب) | `winforms-porting-checklist` المجموعة ب 🔴 | تعديل `FixedAssetsUSCTRL.UpdateFixedAsset` ليستدعى `Update_FixedAssetData_Full` (أو دمج الدالتين). | نعم (ويندوز) |
| 1.3 | `Get_AssetsEhlakForPeriod` يفشل عند اختيار فرع (الديسكتوب) | نفسه 🟠 | استدعاء `Get_AssetsEhlakForPeriod_Full` من `Assets_EhlakFrm_`. | نعم (ويندوز) |
| 1.4 | `Money_Transaction_Box.Id` غير فريد (3078 صفاً/3045 معرّفاً) | backlog #32 | فحص توليد الأرقام فى المحرك وإصلاحه؛ خطر ربط/حذف عميل خاطئ. | نعم (مشترك) |
| 1.5 | مخاطرة `AccountEntry_CLS.Make_AccountEntry_` قد تترك `Account_Entry_List.Id=NULL` | checklist المجموعة ج | **جلسة تحقق حى فقط أولاً** (استعلام: `COUNT(*) WHERE Id IS NULL`)، لا إصلاح قبل النتيجة. | قرار سابق: التوقف عند الاكتشاف |
| 1.6 | أرصدة أول المدة — المرحلة 1 (الصناديق/البنوك → `FirstBalance_CLS.Save_FirstBalance`، حذف بقايا `Last_Balance`، تتبع مسار رصيد البنك `Is_Bank=1`) | `opening-balances-master-plan` §6 | **تحقق (2026-10-02): لم تُنفَّذ** — الصناديق/البنوك ما زالت تمر عبر `BanksClss.Edit_BanksFirst_Balance`، و`Save_FirstBalance` تُستدعى فقط من `CustomerCLS`/`SupplierCLS`. المطلوب تنفيذ المرحلة 1 كاملة. | نعم (مشترك) |
| 1.7 | `AddBranch` لا تُنشئ `Submain_Account`/`Cost_Center_Tree` للفرع الجديد | `accounts-tree-multibranch-plan` | تشخيص السبب (استُبعد معاملة EF/ADO.NET) ثم الإصلاح. | نعم |
| 1.8 | قاعدة Branch_Id: افتراضى `-1` عند إضافة حساب (Razor + ويندوز) وتصحيح بقية القواعد على `ClientNew` | `db-data-fixes-rollout-plan` | تعديل شاشتى «إضافة حساب»، وفحص باقى القواعد قبل تنفيذها. | نعم (بيانات + كود) |
| 1.9 | صناديق الفروع (4 بنود) | `branch-cash-boxes-plan` | (1) تصحيح `Submain_Account.Branch_Id` بيانات، (2) `BanksClss.AddNewBank` يستعمل `ba.Branch_Id`، (3) `GetBoxes` بفرع المستخدم، (4) عرض «صندوق — فرع». البند 4 لا يحتاج موافقة. | نعم عدا 4 |
| 1.10 | **فهرس §8 قديم:** يقول إن عيب مخزون الكمية المجانية لم يُعدَّل، لكن `razor-vs-desktop-fixes-backlog` #24 يوثِّق أنه **أُصلح ومُختبَر** (2026-09-24). | — | تصحيح الفهرس. | لا |

### أولوية 2 — اختبارات لم تُنفَّذ لشىء بُنى (لا كود جديد)

| # | البند | المرجع |
|---|---|---|
| 2.1 | إعادة اختبار 1،2،3،5،6،8 ثم الاختبارات 4 و9-117 (110 اختباراً) | `testing-plan.md` |
| 2.2 | حوار ربط مراكز التكلفة (cascade) — الكود مكتمل 0 أخطاء، لم يُختبر | index §11 / memory `sessionfree` |
| 2.3 | صلاحيات بمستخدم مقيَّد: 982 (حذف صنف/فاتورة)، 983 (إلغاء طلبية)، فاتورة الإجمالى | `menu-invoice-permissions-plan` |
| 2.4 | واجهة الطلبيات كلها (إنشاء/تأكيد/عربون/إلغاء بلا 983) — «لم يُختبر» حسب الخطة | `customer-orders-razor-plan` |
| 2.5 | مرتجع المبيعات بالحقول الإضافية، وتاريخ صلاحية ≠ اليوم | `sales-invoice-all-columns-plan` |
| 2.6 | واجهة وضع «حفظ الأصناف»، ومنع تكرار اسم المخزن، حذف/تفريغ المخزن، حفظ حوار فرع الصندوق، حذف بنك، إلغاء ربط عميل بمندوب | `menu-main-data` / `menu-sales` |
| 2.7 | الطباعة الفعلية `window.print()` للباركود وكشف العملاء/الموردين | `barcode-print-settings`، `menu-main-data` |
| 2.8 | `CustomerSupplierMixedInfo` (إضافة/ظهور فعلى) و`CashManyBoxPage` و`SupplierDestruction` حذف | `menu-main-data` §4، backlog #35 |
| 2.9 | شاشات الأقساط/الإجمالى: حذف الفاتورة الإجمالية لم يُجرَّب | backlog §12 بند 30 |

### أولوية 3 — فجوات وظيفية فى الرازور (قرارات منتج)

| # | البند | الحالة |
|---|---|---|
| 3.1 | شيكات أول المدة وارد/صادر (صفوف 394/395) | **تحقق (2026-10-02): غير مبنية** — لا توجد صفحة `first-balance-checks` فى `Razors/`. تُبنى كمكوّن واحد بباراميتر `Transaction_Type` (وتُراعى قاعدة الشيكات التراكمية المحسومة). |
| 3.2 | «أرصدة الفواتير: توزيع إذن نقدية على الفواتير وحفظه/حذفه» (954) | لم يُبنَ (الجزء الكاتب) رغم أن أذون النقدية بُنيت لاحقاً → قابل للبناء الآن |
| 3.3 | زر «إذن نقدية» فى `InvoicePaymentsPage` وفتح الإذن بالنقر المزدوج | نفسه |
| 3.4 | حذف فاتورة/مرتجع محفوظ كامل فى Razor (المبيعات) | بند 37 يقول بُنى (932/982)؛ مرتجع المبيعات مذكور «لا يوجد» فى سجل 09-26 → **تأكيد** |
| 3.5 | تعديل إذن نقدية متعدد الأسطر = حذف+إعادة إصدار (مقيَّد) و«حذف أذون النقدية من Razor محجوب فى بعض المسارات» | index §10 |
| 3.6 | `JournalEntries`: (د) فتح القيد كاملاً، (هـ) `Cost_Type`/تعدد المشروعات | مؤجَّل بقرار حتى طلب صريح |
| 3.7 | شركة الشحن فى فاتورة المبيعات: لم تُختبر (الجدول فارغ) | يحتاج صف تجريبى |

### أولوية 4 — محاذاة الديسكتوب مع الرازور (قرار لكل بند)

| # | البند | مرجع |
|---|---|---|
| 4.1 | فروق #8، #16، #17، #25-#31، #36 (سكربت `IDENTITY` فى `AlterDB_CLS` ~5922)، #34 (`Day + 1` آخر الشهر) | `razor-vs-desktop-fixes-backlog` |
| 4.2 | التحقق الحى أن المجموعة أ فى `winforms-porting-checklist` (4 إصلاحات مشتركة) تعمل فعلاً فى شاشات الويندوز بعد إعادة النشر (`Taswya_Money`، كشف العملاء…) | checklist |
| 4.3 | 14 مرتجع مشتريات + 169 فاتورة/مرتجع مبيعات قديمة علمتها NULL | **مؤجَّل بقرارك 2026-09-29** |

### أولوية 5 — نظافة وصيانة

| # | البند |
|---|---|
| 5.1 | `AccountsEInvoicesLIB` يفشل restore (`NU1605`): توحيد `RestSharp` (106.15→114) و`WebView.WindowsForms` (10.0.71→10.0.80). |
| 5.2 | مراجعة تعديلات `HMIS` (28 ملفاً) وقرار commit/تجاهل؛ وقرار إعدادات `.claude` المحلية. |
| 5.3 | تدقيق رسائل commits للمشاريع الكبيرة (`Accounts`، `Store_LIB`) — قد تحوى تغييرات خارج «التدقيق». |
| 5.4 | تقرير (لا تحويل) حقول `Common` الساكنة لكل مستخدم الخطرة على سيرفر Razor متعدد المستخدمين. |
| 5.5 | ترجمات `fr`/`de` معلَّمة `TODO` فى resx. |
| 5.6 | `Item_First_Period_Balance`: 3811 صفاً مقابل 35 رصيداً فعلياً — يُعاد بناء الجدول (مرحلة 5 أرصدة). |

---

## 3) ترتيب التنفيذ المقترح

1. **جلسة تصحيح الوثائق + تحقق سريع (بلا كود):** تصحيح فهرس §8 (بند 1.10)، التحقق من 3.1 و3.4 و1.6 (ما نُفِّذ فعلاً)، وتشغيل استعلام 1.5 لمعرفة حجم المخاطرة.
2. **دفعة تدقيق أعمدة التعديل رقم 1** (بند 1.1، أعلى ثقة كفجوة): `BillsCLS`، `InvoicesOperationCLS`، `ItemCLS`، `Submain_AccountCLS`، `CashVouchersService`.
3. **بنود 1.2 و1.3** (إصلاحا الديسكتوب البسيطان والمُصمَّمان مسبقاً).
4. **أرصدة أول المدة المرحلة 1** (1.6) بعد التحقق.
5. **اختبار حوار مراكز التكلفة** ثم متابعة الاختبارات 1،2،3،5،6،8 ← 4 ← 9+ (تحتاج مشاركتك فى المتصفح أو جلسة `razor-tester`).
6. بنود الأولوية 3 بحسب قرار المنتج، ثم 4 و5.

## 4) سجل التنفيذ

*(يُحدَّث هنا بعد كل بند.)*

### 2026-10-02 — تدقيق أعمدة التعديل، دفعة 1 (بقرارك: تعديلات المستخدم المباشرة فقط) — ثُبِّتت فى commit، وأُصلح أيضاً `StoreCLS` (`Edit_User_Id = Common.User_Id` بدل 1)
- **نُفِّذ (بُنى `AccountModelsLib` 0 أخطاء، لم يُختبر حياً بعد، لم يُثبَّت فى commit):**
  - `BillsCLS.UpdatePrice` (يخدم `UpdatePriceOne/All` وشاشة «أسعار البيع للمشتريات»): `Edit_User_Id = Common.User_Id` و`EditOperationDate = DateTime.Now` على `Bills_Details`.
  - `ItemCLS`: `UpdateS_Price` (الصنف + UPDATE الخام لـ`Unit_Item`)، `Edit_Item` (النسخة بـ`ref opp` والنسخة القديمة الثانية) على `Items` و`Unit_item`، و`EditItemPrice` (UPDATE الخام)، `EditService`، `UpdateItemAndUnitItemPrices`.
- **➖ لا ينطبق:** `Submain_AccountCLS` (الجدول بلا العمودين حسب `ERP_V1_3.md`)، `CashVouchersService.EditMultiVoucher` (حذف وإعادة إصدار؛ ينشئ صفاً جديداً بـ`User_Id`).
- **مستثنى بقرارك:** تحديثات الأرصدة/الأسعار التلقائية من حفظ الفواتير (`UpdateItemPriceFromInvoice`، `UpdateItemBalance`، سطرا ~356/~473 القديمان، `UpdatePurchasePrice`). **مؤجَّل لدفعة مستقلة:** `InvoicesOperationCLS` (الموديل لا يعرف العمودين).
- **اكتشافان جانبيان (لم يُعدَّلا):** (1) `StoreCLS` (سطر ~394) يضبط `Edit_User_Id = 1` ثابتاً بدل `Common.User_Id` — خلل فى كلاس عُدّ «مطبَّقاً». (2) تفعيل/إلغاء تفعيل الصنف فى `ItemCLS` (~2276-2277 و~2374 و~2525 و~2592) يكتب `User_Id`/`OperationDate` (حقلا الإنشاء) عند التعديل بدل أعمدة التعديل، وتبديل `Active_flage` عبر EF (~2328) بلا أعمدة تعديل.
- **المتبقى لإغلاق الدفعة:** اختبار حى: تعديل سعر بيع من «أسعار البيع للمشتريات» (يتحقق `Bills_Details` و`Item` و`Unit_Item`) — يحتاج تشغيل السيرفر.

### 2026-10-06 — تدقيق أعمدة التعديل، دفعة 2 (بموافقتك)
- **نُفِّذ (بُنى `AccountModelsLib` 0 أخطاء؛ لم يُختبر حياً):** `FixedAssetCLS.Update_FixedAssetData_Full` (معاملان جديدان `@Edit_User_Id`/`@EditOperationDate`) و`DeleteFixedAsset`/`MakeDeleteFixedAssetData` (تعطيل `active=0`)، `CostCenterCLS.Edit_CostCenter`، `GroupsCLS.Update_Group`، `Assets_EhlakCLS.Update_Ehlak_Asset`. التعديل بايتياً لحفظ ترميز `Assets_EhlakCLS` (غير UTF-8).
- **➖ لا ينطبق:** `CashPaymentMethodCLS.UpdateDataTable` (`Lookup_CashPaymentMethod` بلا العمودين)، `Add_Account_CodesCLS.EditAccount` (`Submain_Account` بلا العمودين).
- **لم يُمسّ عمداً:** `FixedAssetCLS.Update_NewFixedAsset` و`UpdateFixedAsset(ref opp)` (يحذفان الصف) — إصلاحهما هو بند 1.2.
- **المتبقى من الطبقة ب:** `Money_Financial_NoticeCLS`/`_ItemsCLS`، `AccountEntry_CLS`، `TagerKontraCLS`، `Journal_DaysListCLS`، `DailyItemSalesService`، `CountryCLS`، `SalesTotalInvoicesService`، `CustomersAccountCLS`، `SalesMenCLS`، `Money_Transaction_BoxOrderReportCLS`، `InvoicesOperationCLS` (دفعة مستقلة).

### 2026-10-06 — تدقيق أعمدة التعديل، دفعة 3 (بقرارك: الدالتان الآمنتان فقط)
- **نُفِّذ (بُنى 0 أخطاء؛ لم يُختبر حياً):** `Journal_DaysListCLS.Update_Journal_Name` (`Journal_Codes`)، `SalesMenCLS.SetCustomerSalesMan`/`UnsetCustomerSalesMan` (`Customer`).
- **➖ لا ينطبق/مستثنى:** `TagerKontraCLS` (الجدول بلا العمودين)، `Money_Financial_Notice(_Items)CLS.Edit_Financial_Notice` (عكس وإعادة إضافة + تحديثات أرصدة تلقائية)، `DailyItemSalesService` (تحديث تلقائى)، `CountryCLS.UpdateDataTable` (DataAdapter عام)، `CustomersAccountCLS` (أرصدة تلقائية).
- **مؤجَّل:** `AccountEntry_CLS.Make_AccountEntry_Edit` (محرك القيود المركزى؛ يسجّل `User_ID_LoginUpdate` أصلاً والجدول يملك العمودين — قرار مستقل)، `Money_Transaction_BoxOrderReportCLS` (فحص منفصل)، `InvoicesOperationCLS` و`SalesTotalInvoicesService` (الموديل لا يعرف العمودين).

### 2026-10-06 — بنود 1.2 و1.3 (إصلاحا الديسكتوب، بموافقتك)
- **1.2 نُفِّذ:** `Account_Common_LIB/Fixed_Assets/FixedAssetsUSCTRL.cs` ~304: `Update_NewFixedAsset` (تحذف صف `FixedAssetData`) ← `Update_FixedAssetData_Full` (UPDATE كامل للأعمدة الـ24 + مزامنة اسم الحساب + أعمدة التعديل). الدالة القديمة بقيت فى `FixedAssetCLS` بلا استدعاء.
- **1.3 نُفِّذ:** `Account_Common_LIB/Fixed_Assets/New folder/Assets_EhlakFrm_.cs` ~133: `Get_AssetsEhlakForPeriod` ← `Get_AssetsEhlakForPeriod_Full` (يُضيف `JOIN FixedAssetData` الناقص). الشاشة حيّة (تُفتح من `MainFrm_AccountUpdate` سطر ~5531، والمجلد `New folder` غير مستبعد من `.csproj`).
- بُنى `Account_Common_LIB` (net10.0-windows) 0 أخطاء. **لم يُختبر حياً** (يحتاج تشغيل الديسكتوب: تعديل أصل ثابت ثم التحقق أن صف `FixedAssetData` باقٍ بالقيم الجديدة؛ وفتح حساب الإهلاك بفرع محدَّد).
- **نسخ مكررة لم تُمَس (خارج `AccountsNew`/`Accounts.sln`):** `AccountsNew/AccountMainFrm/MainFrm_AccountUpdate.cs` و`Advanced_Account/MainFrm_AccountUpdate.cs` تستخدمان `Assets_EhlakFrm_` نفسها من `Account_Common_LIB`، فيستفيدان من الإصلاح تلقائياً.

### 2026-10-06 — بند 1.6: تتبع مسار رصيد البنك والصندوق (قراءة فقط، لا تعديل كود)
**الصندوق (`Is_Bank=0`) — `Money_Transaction_BoxOrderReportCLS.Compute_Box_BalanceBeforDataDay`:**
- الرصيد = `Banks.Primary_balance` + مجموع `Import_Value` − مجموع `Export_Value` من `Money_Transaction_Box` (كل التواريخ عند `ISAllDates=true`).
- **يُقيَّد عند الصفر:** `if (First_BalanceAfterDay < 0) → 0` — أى رصيد سالب (سحب زائد) يظهر صفراً ولا يُرى العجز.
- **لا يقرأ `Financial_Period_Begining_Balance` إطلاقاً:** رصيد أول المدة المخزَّن لفترة تالية لا يدخل هذا الحساب، فالرصيد يعتمد فقط على `Primary_balance` (أول تعامل) — مسارا أول المدة (أ) و(ب) غير موحَّدين فعلاً.
**البنك (`Is_Bank=1`) — `BanksGridCLS.ApplyBankComputedBalance` (Razor):**
- يُحسب من ميزان المراجعة بالفترة (`Mezan_MorgaaCLS.Get_Mezan_Morgaa_ByPeiodNew`): `Final_Last_Balance_Madin − Final_Last_Balance_Da2n` للحساب `SubAccount_Id` (أول المدة + الحركات من القيود). **أى أن للبنك حساباً فعلياً وقيوداً** — وهذا يخالف وصف الخطة القديم أن البنوك منقطعة عن الشجرة (يصح على الصناديق وحدها).
- تحذير موثَّق بالكود: استدعاؤه يعيد بناء جداول مؤقتة مشتركة (`Account_Entry_Master_Balance_MezanTemp`/`TransactionTemp`) غير معزولة بالجلسة → خطر تضارب مستخدمَين متزامنين (موجود فى شاشة الميزان الأصلية أيضاً).
**بقايا `Banks.Last_Balance` (ما زالت تُكتب):** `Edit_Banks_Balance` (سطر 366)، `Edit_BanksFirst_Balance` (أسطر ~569 و~606: `Last_Balance = Last_Balance + diff`)، و`Money_Transaction_Box.Bank_Balance` يُخزَّن من `Last_Balance` (backlog #34). الشاشات الحية لا تقرأه للعرض، لكنه يلوّث عمود الحركة.
**ما يتبقى لقرارك قبل تنفيذ المرحلة 1:**
1. هل يبقى الصندوق على `Primary_balance + حركات` (مع إزالة قصّ الصفر)، أم ينتقل لنفس محرك الميزان كالبنك (توحيد حقيقى مع `Account_Entry_Master_Balance`)؟
2. هل يُزال القصّ عند الصفر (يُظهر العجز) أم هو سلوك مقصود؟
3. إزالة كتابات `Last_Balance` الثلاث بعد التأكد أن لا شاشة حية تقرأه.
- **قرارات المستخدم (2026-10-06):** (1) رصيد الصندوق يبقى `Primary_balance + حركات` دون الانتقال لمحرك الميزان. (2) قصّ الرصيد السالب عند الصفر **لا يُلمَس الآن** (مسجَّل فقط). (3) **لم يُحسَم بعد:** إزالة كتابات `Banks.Last_Balance` الثلاث — تبقى معلَّقة لقرار لاحق؛ لا تنفيذ للمرحلة 1 قبله.
