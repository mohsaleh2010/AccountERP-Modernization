# خطة مؤجّلة: مراجعة تحديث Edit_User_Id و EditOperationDate فى كل دوال التعديل

**الحالة: مؤجّلة — لا تُنفَّذ إلا حين يطلبها المستخدم صراحةً فى جلسة قادمة.**

## الخلفية والقاعدة

بتاريخ 2026-09-29 أعطى المستخدم تعليمة قياسية: **عند تعديل أى بيان فى أى جدول يجب تحديث `Edit_User_Id` و`EditOperationDate` معاً** (وليس `Edit_User_Id` وحده) - راجع الذاكرة `audit-columns-user-branch-id` للتفصيل الكامل والسياق (فيها أيضاً قاعدة الإضافة: `User_Id`/`Branch_Id` عند الإدراج).

طلب المستخدم أمرين:
1. تطبيق القاعدة على أى عمل قادم (كل دالة حفظ/تعديل جديدة أو مُعدَّلة من الآن فصاعداً).
2. تسجيل خطة مؤجَّلة لمراجعة كل ما بُنى وانتهى قبل هذا التاريخ، بدل تركه بصمت.

هذا الملف هو الخطة المطلوبة فى البند 2.

## منهجية المراجعة المطلوبة (عند تنفيذ هذه الخطة لاحقاً)

لكل شاشة/دالة تعديل فى القائمة أدناه:
1. تأكد أن الجدول المستهدف فعلاً يملك عمودى `Edit_User_Id` و`EditOperationDate` (عبر نموذج EF أو `sys.columns`) — بعض الجداول لا تملكهما إطلاقاً فلا ينطبق عليها شىء.
2. اقرأ دالة التعديل نفسها كاملة (لا تكتفِ بنتيجة `grep`) وتأكد أنها تضبط كلا العمودين معاً بقيمة `Common.User_Id` والوقت الحالى، وليس أحدهما فقط.
3. إن كانت الدالة مشتركة مع الديسك توب (`AccountModelsLib`)، تحقّق هل الديسك توب يستفيد تلقائياً من الإصلاح (كما هو الحال غالباً) أم يحتاج بنداً منفصلاً - سجّله فى `razor-vs-desktop-fixes-backlog.md` حسب القاعدة القائمة، ولا تُعدَّل شاشة ديسك توب دون سؤال المستخدم (`razor-vs-desktop-edit-scope`).
4. أصلح الفجوة إن وُجدت، وابنِ المشروع، واختبر تعديلاً حياً واحداً على الأقل من الشاشة الفعلية (ليس فقط قراءة الكود) للتأكد أن العمودين تغيّرا فى القاعدة.
5. حدِّث حالة الصف فى الجدول أدناه إلى ✅ (أو ➖ إن تبيَّن أنه لا ينطبق) مع تاريخ الإصلاح.

## قائمة انطلاق (نتاج فحص آلى تلقائى بتاريخ 2026-09-29 - **ليست نهائية ولا موثوقة كاملةً**)

تنبيه مهم: القائمة أدناه نتجت من `grep` على أسماء الكلاسات المستخدَمة فعلياً فى صفحات الرازور (`Pages/**/*.razor`) وأسماء دوال تبدأ بـ`Update`/`Edit`، بحثاً عن وجود `Edit_User_Id` فى نفس الملف كمؤشر أولى فقط. **ثبت أن هذا المؤشر يُخطئ سلباً** (مثال مؤكَّد: `CashVouchersService.EditMultiVoucher` موجودة وحقيقية رغم أن الفحص الآلى أظهر "NONE" لعدم مطابقة صيغة التوقيع لنمط البحث). لذلك أى صف هنا **يجب قراءته والتحقق منه يدوياً وقت التنفيذ**، لا الوثوق بعمود "الإشارة الآلية" وحده.

### الطبقة أ: أُشير إليها بوجود `Edit_User_Id` فى نفس الملف (مرجَّح أنها مُطبَّقة جزئياً أو كلياً - تحتاج تأكيد فقط لا إصلاحاً من الصفر)
| الكلاس | الملف |
|---|---|
| AccountBranchesCLS | Models/Config/AccountBranchesCLS.cs |
| CurrencyCLS | BusinessLibShared/Currency/CurrencyCLS.cs |
| CustomerCLS | Models/CustomerSupplier/CustomerCLS.cs |
| CustomerOrdersService | BusinessLibShared/CustomerOrdersService.cs |
| DaysWorkSkowCLS | Account/DaysWorkSkowCLS.cs |
| FirstBalance_CLS | Account/FirstBalance_CLS.cs |
| InvoicesOperationListCLS | Models/Invoice/NewFolder1/InvoicesOperationListCLS.cs |
| Mezan_MorgaaCLS | Models/Account_Entry/Entry_Process/Mezan_MorgaaCLS.cs |
| StoreCLS | Models/Store/StoreCLS.cs |
| StoresGridCLS | Models/Store/StoresGridCLS.cs |
| SupplierCLS | Models/CustomerSupplier/SupplierCLS.cs |

### الطبقة ب: لها دالة Update/Edit حقيقية ولم يظهر `Edit_User_Id` إطلاقاً فى الملف (مرشَّحة بقوة لفجوة حقيقية)
| الكلاس | الملف | الدوال المكتشَفة |
|---|---|---|
| BillsCLS | Models/Invoice/Bills/BillsCLS.cs | UpdatePriceAll, UpdatePriceOne |
| InvoicesOperationCLS | Models/Invoice/NewFolder1/InvoicesOperationCLS.cs | Update_Invoices_Details, Update_Invoices_Details_Reverse |
| Assets_EhlakCLS | Account_Common_LIB/Assets_EhlakCLS.cs | Update_Ehlak_Asset |
| Money_Financial_NoticeCLS | Account_Common_LIB/Money_Financial_NoticeCLS.cs | Edit_Financial_Notice |
| FixedAssetCLS | Models/Account_Entry/FixedAssetCLS.cs | UpdateFixedAsset, Update_FixedAssetData_Full, Update_NewFixedAsset |
| AccountEntry_CLS | Models/Account_Entry/Entry_Process/AccountEntry_CLS.cs | EditRow_Equivalent_Transaction_Value, EditRow_WhenBuild |
| ItemCLS | Models/Items/ItemCLS.cs | EditService, Edit_Item, UpdateItemAndUnitItemPrices, UpdateItemBalance, UpdateItemPriceFromInvoice, UpdateS_Price |
| Money_Financial_Notice_ItemsCLS | Account_Common_LIB/Money_Financial_Notice_ItemsCLS.cs | Edit_Financial_Notice, Edit_Financial_Notice_ |
| TagerKontraCLS | Models/BusinessCLS/TagerKontraCLS.cs | UpdateUnitType |
| Journal_DaysListCLS | BusinessLibShared/GeneralLedger/Journal_DaysListCLS.cs | Update_Journal_Name |
| DailyItemSalesService | Batches/DailyItemSalesService.cs | UpdateSale |
| GroupsCLS | BusinessLib/GroupsCLS.cs | Update_Group |
| CostCenterCLS | Account_Common_LIB/CostCenterCLS.cs | Edit_CostCenter |
| CountryCLS | BusinessLib/CountryCLS/CountryCLS.cs | UpdateDataTable |
| SalesTotalInvoicesService | BusinessLibShared/SalesTotalInvoicesService.cs | Update |
| Submain_AccountCLS | Models/Account_Entry/Submain_Account/Submain_AccountCLS.cs | Edit_AccountsName, Edit_AccountsPosition, UpdateSubmain_Account |
| CustomersAccountCLS | Account/CustomersAccountCLS.cs | Updatet_Customer_BalanceWithEditInvoice |
| SalesMenCLS | Models/Employee/SalesMenCLS.cs | UpdateSalesMan |
| CashPaymentMethodCLS | Account_Common_LIB/CashPaymentMethodCLS.cs | UpdateDataTable |
| Add_Account_CodesCLS | Account_Common_LIB/Add_Account_CodesCLS.cs | EditAccount |
| Money_Transaction_BoxOrderReportCLS | Account_Common_LIB/Money_Transaction_BoxOrderReportCLS.cs | EditBankBalance, Edit_Order_Money, Edit_Transaction_BanksBox_NewSarf(+عدة نسخ) |
| CashVouchersService | BusinessLibShared/CashVouchersService.cs | **EditMultiVoucher معروفة من سياق الجلسة رغم أن الفحص الآلى لم يكتشفها - راجعها يدوياً بالتأكيد** |

### الطبقة ج: لم يظهر لها دالة Update/Edit فى الفحص الآلى (يُحتمل شاشات عرض/تقرير فقط بلا تعديل - تحتاج تأكيد سريع لا مراجعة كاملة)
Govern_AreaCLS، InvoiceValuesCLS، SupplierBillsDueCLS، ShipmentCompanyEFCLS، ItemsCardsCLS، InvoiceBeautyCLS، InvoiceHeaderLookupsCLS، Money_SpentReportsCLS، EmployeeService، Financial_PeriodCLS، Gov_City_ListCLS، TaxListCLS، Govern_ListCLS، PurchaseItemsReportCLS، CurrencyFactorsCLS، Account_Entry_Invoice_CLS، POS_InvoiceCLS، Account_BrancheCLS، Receive_TalabiaCLS

### غير مشمولة بهذا الفحص إطلاقاً
- أى شاشة/كلاس رازور بُنيت **بعد** 2026-09-29 (تُفحص وقتها مباشرة عند البناء، لا تُضاف هنا).
- الخدمات التقنية البحتة (DI/UI) مثل `AuthService`، `DialogService`، `LanguageService`، `ThemeService`، `TabWorkspaceService`، `MenuStateService`، `NotificationCenterService`، `UserPreferencesService` - لا تحفظ بيانات جداول أعمال، مستبعدة عمداً.
- كود الديسك توب (WinForms) بالكامل - هذه الخطة تخص الرازور فقط بحكم نطاق الجلسة؛ فجوات الديسك توب المكافئة تُسجَّل فى `razor-vs-desktop-fixes-backlog.md` عند اكتشافها أثناء تنفيذ هذه الخطة، ولا تُعدَّل دون سؤال المستخدم.

## طريقة الاستئناف

فى الجلسة التى يُطلَب فيها تنفيذ هذه الخطة: ابدأ بالطبقة ب (الأعلى ثقة كفجوة حقيقية)، ثم تأكَّد من الطبقة أ (تأكيد لا إصلاح غالباً)، ثم افحص الطبقة ج بسرعة لاستبعادها أو نقل ما يلزم منها إلى طبقة ب. استخدم `AskUserQuestion` لتقسيم الدفعات إن كان العدد كبيراً (نحو 30+ كلاساً)، بدل تنفيذها كلها فى جلسة واحدة.
