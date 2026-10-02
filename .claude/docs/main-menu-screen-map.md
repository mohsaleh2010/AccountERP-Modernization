# Main ERP Menu -> WinForms Screen Map (`mainMenu1`, `Accounts.MainFrm_AccountUpdate`)

**Source of truth confirmed for this document:** the class actually compiled into the running ERP shells
(`AccountsERPSystem`, `AccountsERPSystemNet10`) is `Accounts\AccountMainFrm\MainFrm_AccountUpdate.cs` +
`MainFrm_AccountUpdate.Designer.cs` (namespace `Accounts`) — **not** `Advanced_Account\MainFrm_AccountUpdate.cs`.
Verified by direct `.csproj` inspection: `AccountsERPSystem.csproj` and `Accounts.csproj` both lack any
`ProjectReference`/file reference to the `Advanced_Account` project, so the 10,623-line `Advanced_Account` copy
(despite matching `CLAUDE.md`'s '~9,000 lines, 718 nav calls' description) cannot be the type the compiler
resolves for `new MainFrm_AccountUpdate()` in either EXE shell's `Program.cs`. It is a disconnected, dead
duplicate for the purposes of the live app. This resolves the open question in `CLAUDE.md`'s
*Areas Requiring Further Investigation* section ("Whether `Advanced_Account`... is reached via a formal
`ProjectReference`") — **it is not** reached at all from the live shells; `CLAUDE.md` and
`.claude/docs/screen-navigation.md` should be corrected accordingly.

**Also note:** the file is Windows-1256 (Arabic) encoded, not UTF-8/ASCII — confirmed by raw byte inspection
(`0xCA 0xDB 0xED 0xED 0xD1 ...` decodes as `تغيير كلمة المرور` under cp1256). Re-open it with that codepage;
naive UTF-8 tooling will mojibake every Arabic caption, per `CLAUDE.md`'s *Known Technical Constraints*.

**Extraction stats:** 620 `ToolStripMenuItem`/`ToolStripSeparator` fields declared in the
Designer file, 509 with a directly-wired `.Click` handler (leaf actions), 
606 `private void X(object sender, EventArgs e)` handler methods found in the
code-behind, across 19 top-level menus on `mainMenu1`.

**How this map was built** (per the user's standing instruction — see the `locate-winforms-screen-via-mainmenu`
memory): parsed every `ToolStripMenuItem` field in `MainFrm_AccountUpdate.Designer.cs` for its `Name`, Arabic
`Text` caption, `DropDownItems.AddRange` parent/child wiring, and `.Click +=` handler wiring; then, for every leaf
item's handler, scanned the handler's method body in `MainFrm_AccountUpdate.cs` for `new SomeForm(...)` calls to
identify the target screen(s) it opens. This is mechanical extraction, not interpretation — where a handler body
was not found, or contained no recognizable `new Form(...)` call (e.g. it delegates to a helper method, reuses an
existing MDI child instance, or is gated by a permission check before opening anything), that is flagged explicitly
below rather than guessed at.

---

## ملف  <sub>`menuItemFile`</sub>

### تغيير وجهة البرنامج  <sub>`menuUI`</sub>

- **إختيار** — `Name=menuSlectStyle`, `Click=menuSlectStyle_Click` → `ProjUI`
- **بدون** — `Name=menuNoSkin`, `Click=menuNoSkin_Click` → *(no `new Form(...)` found in handler — see note)*
- **Test** — `Name=menuItem152`, `Click=menuItem152_Click` → `ProjUI_`

- **تغيير كلمة المرور** — `Name=menuItemChangePassword`, `Click=menuItemChangePassword_Click` → `ChangePassword`
- **القائمة الجانبية** — `Name=menuItemListSide`, `Click=menuItemListSide_Click` → *(no `new Form(...)` found in handler — see note)*
- **ضبط قاعدة البيانات** — `Name=menuItemDBConfig`, `Click=menuItemDBConfig_Click` → `AccountCommonDataLIB.DBConfig`
- **BackUp & Restore Database** — `Name=menuItemDBBackup`, `Click=menuItemDBBackup_Click` → `FrmMainBackup`, `BackupFrm`
- **Update DataBase** — `Name=toolStripMenuItemUpdateDataBase`, `Click=toolStripMenuItemUpdateDataBase_Click` → *(no `new Form(...)` found in handler — see note)*
- **Reload Setting** — `Name=StripMenuItemReloadSetting`, `Click=StripMenuItemReloadSetting_Click` → *(no `new Form(...)` found in handler — see note)*
- **تغيير إسم الشركة** — `Name=menuItemCompanyName`, `Click=menuItemCompanyName_Click` → `ChangeCompanyNameFrmcs`
- **بيانات المعدلات الضريبية** — `Name=TaxInfotoolStripMenuItem`, `Click=TaxInfotoolStripMenuItem_Click` → *(no `new Form(...)` found in handler — see note)*

### ربط الحسابات  <sub>`menuItemRelatedAccounts`</sub>

- **ربط الحسابات الرئيسية** — `Name=menuItemRelatedMainAccounts`, `Click=menuItemRelatedMainAccounts_Click` → `ConstructionLib.MainAccountSettingListFrm`
- **ربط العملاء بالحسابات** — `Name=menuItemRelatedCustomerAccounts`, `Click=menuItemRelatedCustomerAccounts_Click` → `CustomerDataListFRMSubAccount`, `AccountsCustomerLinkFrm`
- **ربط الموردين بالحسابات** — `Name=menuItemRelatedSupplierAccounts`, `Click=menuItemRelatedSupplierAccounts_Click` → `CustomerDataListFRMSubAccount`
- **ربط الصندوق بالحسابات** — `Name=menuItemRelatedSandoukAccounts`, `Click=menuItemRelatedSandoukAccountsToolStripMenuItem_Click` → *(no `new Form(...)` found in handler — see note)*
- **ربط البنك بالحسابات** — `Name=menuItemRelatedBankAccounts`, `Click=nmenuItemRelatedBankAccountsToolStripMenuItem_Click` → *(no `new Form(...)` found in handler — see note)*
- **ربط الموظفين بالحسابات** — `Name=menuItemRelatedEmployeeAccounts`, `Click=nmenuItemRelatedEmployeeAccountsToolStripMenuItem_Click` → *(no `new Form(...)` found in handler — see note)*

- **المستخدمين** — `Name=ToolStripMenuItemUserDaetails`, `Click=ToolStripMenuItemUserDaetails_Click` → `ADDNEWUSER`, `UsersList`
- **صلاحيات الفروع** — `Name=toolStripMenuItemBranch_UserPermission`, `Click=toolStripMenuItemBranch_UserPermission_Click` → `Branch_UserPermission`
- **إعدادات طابعات النظام** — `Name=toolStripMenuItemPrinterSelection`, `Click=toolStripMenuItemPrinterSelection_Click` → `PrinterSelectionFrm`
- **صلاحيات المستخدمين** — `Name=toolStripMenuItemPermissinForm`, `Click=toolStripMenuItemPermissinForm_Click` → `frmadmin`
- **غلق** — `Name=menuItemClose`, `Click=menuItemClose_Click` → *(no `new Form(...)` found in handler — see note)*

## بيانات أساسية  <sub>`menuItemMainData`</sub>

### العملاء  <sub>`menuItemCustomer`</sub>

- **بيانات العملاء** — `Name=menuCustomerInfo`, `Click=menuCustomerInfo_Click` → `Connection4It.Customer_Info`
- **قائمة العملاء السيارات** — `Name=menuItemCustomerCarsLIST`, `Click=menuItemCustomerCarsLIST_Click` → `CustomerDataListFRM`
- **قائمة العملاء** — `Name=menuItemCustomerLIST`, `Click=menuItemCustomerLIST_Click` → `CustomerDataListFRM`
- **مجموعات العملاء** — `Name=menuItemCustomerGroup`, `Click=menuItemCustomerGroup_Click` → `GroupsListFrm`
- **بيانات عملاء متنوعون** — `Name=menuItemGeneral_Customer_Flage`, `Click=menuItemGeneral_Customer_Flage_Click` → `Connection4It.Customer_Info`
- **طباعة كشف بيانات العملاء** — `Name=menuItemCustomerDataListPrint`, `Click=menuItemCustomerDataListPrint_Click` → `Reporting_System.CustomerRPT.CustomerDataFRm`
- **بيانات حساب عميل - مورد - مختلط** — `Name=menuItemSupplierCustomer`, `Click=menuItemSupplierCustomer_Click` → `Connection4It.Customer_SupplierInfo`
- **ضبط الحسابات المختلطة** — `Name=menuItemConfigSharedAccount`, `Click=menuItemConfigSharedAccount_Click` → `Accounts.Customer_SupplierAccountFrmLv`

### الموردين  <sub>`menuItemSupplier`</sub>

- **بيانات الموردين** — `Name=menuSupplierInfo`, `Click=menuSupplierInfo_Click` → `Connection4It.Supplier_Info`
- **قائمة بيانات الموردين** — `Name=menuItemSupplierList`, `Click=menuItemSupplierList_Click` → `SupplierDataListFRM`
- **مجموعات الموردين** — `Name=menuItemSupplierGroup`, `Click=menuItem121_Click` → `GroupsListFrm`
- **طباعة كشف بيانات الموردين** — `Name=menuItemSupplierDataListPrint`, `Click=menuItemSupplierDataListPrint_Click` → `Reporting_System.SuppliersRPT.SuppliersDataListPrintFrm`

### مندوبين المبيعات  <sub>`menuSalesManMain`</sub>

- **بيانات مندوبين المبيعات** — `Name=menuSalesMan`, `Click=menuSalesMan_Click` → `Connection4It.SalesMan_Info`
- **عملاء مندوبى المبيعات** — `Name=menuItemCustomerSalesMan`, `Click=menuItemCustomerSalesMan_Click` → `Accounts.SalesManProcess.CustomerSalesMan`

- **فروع الشركه** — `Name=ToolStripMenuItemBranches`, `Click=ToolStripMenuItemBranches_Click` → `BranchesListFrm`
- **صندوق الفروع** — `Name=ToolStripMenuItemBankBranch`, `Click=ToolStripMenuItemBankBranch_Click` → `Account_Common_LIB.Banks.SandoukBranch`
- **تغيير الإعدادات** — `Name=toolStripMenuItemEditSysConfig`, `Click=ToolStripMenuItemEditSysConfig_Click` → `FrmStaticEditSysConfig`
- **بيانات حساب البنوك** — `Name=menuItemBanks`, `Click=menuItemBanks_Click` → *(no `new Form(...)` found in handler — see note)*
- **طرق الدفع النقدى** — `Name=toolStripMenuItemCashPaymentMethod`, `Click=toolStripMenuItemCashPaymentMethod_Click` → `CashPaymentMethodFrm`
- **بيانات الصندوق - الخزينة ** — `Name=menuItemMoneybanks`, `Click=menuItemMoneybanks_Click` → `MoneyBanksFrm`
- **بيانات المخازن والأقسام** — `Name=menuStoreList`, `Click=menuStoreList_Click` → `Store_LIB.Store_ListFrm`
- **بيانات شركات الشحن** — `Name=toolStripMenuItemShipmentCompany`, `Click=toolStripMenuItemShipmentCompany_Click` → `ShipmentCompanyLisFrm`
- **بيانات الماكينات** — `Name=toolStripMenuItemMachine_List`, `Click=toolStripMenuItemMachine_List_Click` → `Machin_listFrm`
- **بيانات الموظفين / العاملين** — `Name=ToolStripMenuItemEmployeeList`, `Click=ToolStripMenuItemEmployeeList_Click` → `EmployeeLisFrm`
- **نص بيان الفاتورة** — `Name=toolStripMenuItemInvoiceNotes`, `Click=ToolStripMenuItemInvoiceNotes_Click` → `Static_NotesInInvoice`
- **تست طباعة البار كود** — `Name=menuItemPrintBarCode`, `Click=menuItemPrintBarCode_Click` → `Store_LIB.PrintItemsBarCodeSettingFor_Developer`, `Store_LIB.PrintItemsBarCodeSettingFor_DeveloperNew`, `Store_LIB.BarCodeSettingsForm`
- **إعدادات طباعة البار كود** — `Name=menuItemPrintBarCodeSetting`, `Click=menuItemPrintBarCodeSetting_Click` → `Store_LIB.PrintItemsBarCodeSettingFor_User`
- **رسمات التشغيل الخارجى** — `Name=menuDrawList`, `Click=menuDrawList_Click` → `Accounts.Work_Out.Draws_ListFRM2`

> **تحديث (2026-09-29) — نقل قام به المستخدم مباشرة فى الـ Designer:** قسم "أرصدة أول المدة" كاملًا (`menuItemFirstBalance` وأولاده الثلاثة: شيكات صادرة/واردة أول المدة، أرصدة أول المدة) نُقل من هنا إلى قائمة **الحسابات** (`menuMainAccount`) — انظر أدناه. البند الرابع الذى كان هنا (`menuItemOldKewd` / "قيود إفتتاحية سابقة") **لم يُنقل بل حُذف من القائمة نهائيًا**: لا يوجد له حقل `ToolStripMenuItem` فى `MainFrm_AccountUpdate.Designer.cs` بعد الآن ولا استدعاء له فى أى `DropDownItems.AddRange`؛ الدالة `menuItemOldKewd_Click` ما زالت موجودة فى `MainFrm_AccountUpdate.cs` لكنها أصبحت كود ميت (orphaned handler) لا يستدعيه أى عنصر قائمة حاليًا.

- **أنواع الكونترا** — `Name=ToolStripMenuItemtagerKontraName`, `Click=ToolStripMenuItemtagerKontraName_Click` → `TagerKontraCLSListFrm`, `FirstBalance_Financial_Period_FRM`
- **الدول** — `Name=ToolStripMenuItemCountry`, `Click=ToolStripMenuItemCountry_Click` → `CountryLisFrm`
- **المناطق** — `Name=ToolStripMenuItemGovern_Area`, `Click=ToolStripMenuItemGovern_Area_Click` → `Govern_AreaListFrm`
- **المحافظات** — `Name=ToolStripMenuItemGovern_List`, `Click=ToolStripMenuItemGovern_List_Click` → `Govern_ListFrm`
- **المدن** — `Name=ToolStripMenuItemGov_City_List`, `Click=ToolStripMenuItemGov_City_List_Click` → `Gov_City_ListFrm`

## الحسابات  <sub>`menuMainAccount`</sub>

### اليوميات  <sub>`menuItemDays`</sub>

- **يومية الصادر والوارد** — `Name=menuDayShowWork`, `Click=menuDayShowWork_Click` → *(no `new Form(...)` found in handler — see note)*
- **يومية مصروفات** — `Name=menuItemExpensDays`, `Click=menuItemExpensDays_Click` → `Money_SpentReports`
- **يومية الصندوق** — `Name=menuItemBoxMonyDays`, `Click=menuItemBoxMonyDays_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_Box`, `Account_Common_LIB.New_Money_Transaction.BanksMoneyTransactionAccounts`

#### المخازن  <sub>`menuItem24`</sub>

- **حركة المخازن أصناف مبيعات** — `Name=menuItemStoreMoveSellItems`, `Click=menuItemStoreMoveSellItems_Click` → `ItemsCardsStore`
- **حركة المخازن أصناف مشتريات** — `Name=menuItemStoreMovePurchaseItems`, `Click=menuItemStoreMovePurchaseItems_Click` → `ItemsCardsStore`
- **تقرير يومية المخزن أصناف مبيعات** — `Name=menuItemmenuItemStoreDayCardsSellItems`, `Click=menuItemmenuItemStoreDayCardsSellItems_Click` → `ItemsCardsStore_Days`
- **تقرير يومية المخزن أصناف مشتريات** — `Name=menuItemmenuItemStoreDayCardsPurchaseItems`, `Click=menuItemmenuItemStoreDayCardsPurchaseItems_Click` → `ItemsCardsStore_Days`


- **شجرة الحسابات** — `Name=menuAccountsTree`, `Click=menuAccountsTree_Click` → `Account_Tree_CodesFrm`

### الأصول الثابته  <sub>`ToolStripMenuItemMainFixedAsset`</sub>

- **دليل الإصول الثابته** — `Name=ToolStripMenuItemMainFixedAssetLists`, `Click=ToolStripMenuItemMainFixedAssetLists_Click` → `Account_Common_LIB.FixesAsset_TreeFrm`
- **حساب الإهلاك** — `Name=ToolStripMenuItemEhlakCompute`, `Click=ToolStripMenuItemEhlakCompute_Click` → `Assets_EhlakFrm_`
- **الإهلاك** — `Name=menuItemDestruction`, `Click=menuItemDestruction_Click` → `Assets_EhlakFrm`

- **الفترات الماليه** — `Name=ToolStripMenuItem_FinancialPeriod`, `Click=ToolStripMenuItem_FinancialPeriod_Click` → `Financial_PeriodFrm`

### أرصدة  أول المدة  <sub>`menuItemFirstBalance`</sub> — **نُقلت إلى هنا من "بيانات أساسية" بتاريخ 2026-09-29**

- **شيكات صادرة أول المدة** — `Name=menuItemCheck_BeginBalanceExport`, `Click=menuItemCheck_BeginBalance_Click` → `Order_Receipt_ChecksFrm`, `FirstBalance_Checks_List`
- **شيكات واردة  أول المدة** — `Name=menuItemCheck_BeginBalanceImport`, `Click=menuItemCheck_BeginBalanceImport_Click` → `Order_Receipt_ChecksFrm`, `FirstBalance_Checks_List`
- **أرصدة أول المدة** — `Name=menuItemNewKoudFirstMezania`, `Click=menuItem35_Click` → `Complex_DayRegistrationFrmSP`, `FirstBalanceMezan_FirstTime`, `FirstBalance_Financial_Period_FRM`

⚠️ **ملحوظة:** كامل هذا الفرع (`menuItemFirstBalance`) حاليًا `Visible = false` فى الـ Designer — أى أنه مخفى فعليًا عن كل المستخدمين فى القائمة الحالية، وليس فقط مكانه تغيّر. تحقق من هذا قبل افتراض أن المستخدمين يمكنهم الوصول إليه.

⚠️ **حُذف بالكامل (كان هنا سابقًا فى نفس الموضع):** عنصر مكرر بنفس الغرض تقريبًا باسم `ToolStripMenuItemFinancial_Period_FirstBalance` ("أرصدة أول المده" → `Mezan_Morgaa_ByPeriod`, `FirstBalanceMezan_FirstTime`, `FirstBalance_Financial_Period_FRM`) كان موجودًا هنا مباشرة. لا يوجد له حقل فى الـ Designer ولا دالة `_Click` فى الكود بعد الآن — حُذف بالكامل (حقلًا ومعالجًا)، وليس فقط أُخفى. يبدو أن المستخدم وحَّد الوصول على `menuItemNewKoudFirstMezania` وحده بدل وجود مدخلين متكررين لنفس الشاشات تقريبًا.

- **العمـــــلات** — `Name=toolStripMenuItemCurrencyListFrm`, `Click=toolStripMenuItemCurrencyListFrm_Click` → `CurrencyListFrm`, `CurrencyListFrmNew`
- **سعر العمله** — `Name=ToolStripMenuItemCurrency_Factors`, `Click=ToolStripMenuItemCurrency_Factors_Click` → `Account_Common_LIB.Currency_Factors`
- **قائمة اليوميات** — `Name=ToolStripMenuItem_JournalsDays`, `Click=ToolStripMenuItem_JournalsDays_Click` → `Journal_DaysListFrm`
- **مراكز تكلفة الحسابات** — `Name=ToolStripMenuItemCostCenterForAccounts`, `Click=ToolStripMenuItemCostCenterForAccounts_Click` → `CostCenterForAccounts_FRM`
- **مراكز التكلفة** — `Name=ToolStripMenuItemCostTree`, `Click=ToolStripMenuItemCostTree_Click` → `Account_Common_LIB.Cost_CenterTree.Cost_Center_TreeFrm`
- **كشف أرصدة العملاء** — `Name=menuItemCustomerAccountBalance`, `Click=CustomerAccountBalance_Click` → `Customer_BalanceMokhtalatList`
- **أرصدة العملاء بتورايخ سابقة** — `Name=menuItemCustomerAccountBalanceDatesBefor` → *(no click handler)*
- **كشف أرصدة الحسابات المختلطة** — `Name=menuItemCustomerAccountBalance_Mokhtalat`, `Click=menuItem117_Click` → `Accounts.Customer_SupplierAccountFrmLv`
- **حساب العملاء** — `Name=menuItemCustomerAccountCars`, `Click=menuItemCustomerAccountCars_Click` → *(no `new Form(...)` found in handler — see note)*
- **حساب عملاء السيارات - بالفترة المالية** — `Name=menuItemCustomerAccountCarsPeriod`, `Click=menuItemCustomerAccountCarsPeriod_Click` → *(no `new Form(...)` found in handler — see note)*
- **حساب عملاء - بالفترة المالية** — `Name=menuItemCustomerAccountPeriod`, `Click=menuItemCustomerAccountPeriod_Click` → `Customer_AccountMokhtalat_Period`
- **حساب عملاء** — `Name=menuItemCustomerAccount`, `Click=menuItemCustomerAccount_Click` → `Customer_AccountFrmLv`, `Customer_AccountMokhtalat`
- **كشف أرصدة الموردين** — `Name=menuItemSupplier4ListBalance`, `Click=menuItemSupplier4ListBalance_Click` → `Supplier_BalanceList`
- **حساب الموردين - فترات مالية** — `Name=menuItemSuppliersAccountPeriod`, `Click=menuItemSuppliersAccountPeriod_Click` → `Supplier_AccountNewFrmPeriod`
- **حساب الموردين** — `Name=menuItemSuppliersAccount`, `Click=menuItem128_Click` → `Supplier_AccountNewFrm`

### إشعارات التسوية المالية  <sub>`menuItemTaswyaMalya`</sub>

- **إصدار إشعار التسوية** — `Name=menuItemNewTaswya`, `Click=menuItemNewTaswya_Click` → `Taswya_Money`
- **مراجعة إصدارات التسوية** — `Name=menuItemReviewTaswya`, `Click=menuItemReviewTaswya_Click` → `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_Review`

### التسوية المالية بالصنف  <sub>`menuItemTasweaByItemOne`</sub>

- **إصدار التسوية بالصنف** — `Name=menuItemNewTaswyaItems`, `Click=menuItemNewTaswyaItems_Click` → `Taswya_MoneyItem`
- **مراجعة التسوية بالصنف** — `Name=menuItemReviewTaswyaItems`, `Click=menuItemReviewTaswyaItems_Click` → `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_ReviewItem`

### التسوية المالية بالأصناف  <sub>`menuItemTasweaByItemMulti`</sub>

- **إصدار التسوية بالأصناف** — `Name=menuItemNewTaswyaMultiItems`, `Click=menuItemNewTaswyaMultiItems_Click` → `Taswya_MoneyMulti_Item`
- **مراجعة التسوية بالأصناف** — `Name=menuItemReviewTaswyaMultiItems`, `Click=menuItemReviewTaswyaMultiItems_Click` → `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_Review_Multi_Item`


## القيود  <sub>`ToolStripMenuAccountEntry`</sub>

- **إدخال القيود** — `Name=ToolStripNew_MenuAccountEntry`, `Click=ToolStripNew_MenuAccountEntry_Click` → `NewAccount_EntryFRM`
- **إدخال القيود** — `Name=ToolStripNew_MenuAccountEntryMadinDa2n`, `Click=ToolStripNew_MenuAccountEntryMadinDa2n_Click` → `NewAccount_EntryMadinDa2nFRM`
- **إدخال القيود البسيطة** — `Name=toolStripMenuNewSimpleAccount_EntryMadinDa2nFRM`, `Click=toolStripMenuNewSimpleAccount_EntryMadinDa2nFRM_Click` → `NewSimpleAccount_EntryMadinDa2nFRM`
- **مراجعة القيود** — `Name=ToolStripMenuAccountEntryReview`, `Click=ToolStripMenuAccountEntryReview_Click` → `Review_Account_Entry`
- **إنشاء قيود الصندوق أليا** — `Name=ToolStripMenuItemMake_Account_Entry_Automatic`, `Click=ToolStripMenuItemMake_Account_Entry_Automatic_Click` → `Make_Account_Entry_Automatic`
- **ترحيل القيود** — `Name=ToolStripMenuAccountEntryTar7eel`, `Click=ToolStripMenuAccountEntryTar7eel_Click` → `Tar7eel_Account_Entry`
- **aaaaaaTest** — `Name=aaaaaaTestToolStripMenuItem` → *(no click handler)*

## الحسابات الختامية  <sub>`ToolStripMenuItem_Reports`</sub>

- **طباعة شجرة الحسابات** — `Name=toolStripMenuItemPrintTreeAccount`, `Click=toolStripMenuItemPrintTreeAccount_Click` → `Account_Common_LIB.Submain_AccountPrintFrm`
- **الأستاذ المساعد  بالفتره الماليه** — `Name=toolStripMenuItemMaster_Account_DetailsTarseed`, `Click=toolStripMenuItemMaster_Account_DetailsTarseed_Click` → `Accounts.General_LedgerAccount.Master_Account_DetailsTarseed22`, `Master_Account_DetailsTarseed2`
- **الأستاذ المساعد عن فتره** — `Name=ToolStripMenuItemMasterAccount`, `Click=ToolStripMenuItemMasterAccount_Click` → `Master_Account_Details2`
- **ميزان المراجعه عن فترة بالمجاميع** — `Name=ToolStripMenuItemMezanByPeriodMagame33`, `Click=ToolStripMenuItemMezanByPeriodMagame33_Click` → `Mezan_Morgaa_ByPeriod`
- **ميزان المراجعه ختامى بالمجاميع والأرصدة** — `Name=ToolStripMenuItem_Mezan_Morgaa`, `Click=ToolStripMenuItem_Mezan_Morgaa_Click` → `Mezan_Morgaa_ByPeriod`, `Mezan_Morgaa_ByPeriodByAccount`
- **طباعة ميزان المراجعه عن فترة بالمجاميع** — `Name=ToolStripMenuItemMezanByPeriodMagame3`, `Click=ToolStripMenuItemMezanByPeriodMagame3_Click` → `Mezan_Morgaa_ByMagamehFrm_RPT`
- **طباعة ميزان المراجعه ختامى بالمجاميع والأرصدة** — `Name=ToolStripMenuItemMezanByPeriodToolStripMenuItemMezanByPeriodMagame3Arseda`, `Click=ToolStripMenuItemMezanByPeriodToolStripMenuItemMezanByPeriodMagame3Arseda_Click` → `Mezan_Morgaa_ByMagameh_ArsedaFrm_RPT`
- **أستاذ مساعد مراكز التكلفه** — `Name=ToolStripMenuItemMasterCostCenter`, `Click=ToolStripMenuItemMasterCostCenter_Click` → `Master_Account_CostCenter_TarsesdFRM`
- **قائمة الدخل** — `Name=toolStripMenuItemIncoming_Statement`, `Click=toolStripMenuItemIncoming_Statement_Click` → `Incoming_StatementByDate`
- **حساب التشغيل** — `Name=ToolStripMenuItemTash7heelAccount`, `Click=ToolStripMenuItemTash7heelAccount_Click` → `Profit_Loss_StatFRMNew`
- **حساب المتاجره** — `Name=ToolStripMenuItemMotagra`, `Click=ToolStripMenuItemMotagra_Click` → `Profit_Loss_StatFRMNew`
- **حساب الأرباح والخسائر** — `Name=ToolStripMenuItemArba7Wa7hsa2r`, `Click=ToolStripMenuItemArba7Wa7hsa2r_Click` → `Profit_Loss_StatFRMNew`
- ** أستاذ مساعد مراكز التكلفه قديم** — `Name=ToolStripMenuItemMasterCostCenter1`, `Click=ToolStripMenuItemMasterCostCenter1_Click` → `Master_Account_CostCenter_FRM`
- **إعداد قائمة المركز المالى** — `Name=toolStripMenuItemAccountBalance`, `Click=toolStripMenuItemAccountBalance_Click` → `Get_Account_BalanceFRM2`
- **بيانات أرصدة المواد عن فترة** — `Name=menuItemFinancial_Period_Stores_Balance`, `Click=menuItemFinancial_Period_Stores_Balance_Click` → `Financial_Period_Stores_BalanceFRM`

## السيارات  <sub>`toolStripMenuItemCars`</sub>

- **إضافة سيارة عميل سابق** — `Name=ToolStripMenuItemAddCar`, `Click=ToolStripMenuItemAddCar_Click` → `AddCarFrm`
- **إضافة سيارة عميل جديد** — `Name=ToolStripMenuItemAddCarNewCustomer`, `Click=ToolStripMenuItemAddCarNewCustomer_Click` → `CarsSystemLib.AddCarFrm`
- **قائمة السيارات** — `Name=ToolStripMenuItemCarList`, `Click=ToolStripMenuItemCarList_Click` → `CarsListFrm`
- **أوامر التشغيل** — `Name=ToolStripMenuItemWorkOrderList`, `Click=ToolStripMenuItemWorkOrderList_Click` → `WorkOrderListFrm`
- **اصدار أمر شغل جديد** — `Name=ToolStripMenuItemWorkOrderNew`, `Click=ToolStripMenuItemWorkOrderNew_Click` → `CarsSystemLib.AddWorkOrder`
- **إصدار مقايسة اصلاح** — `Name=ToolStripMenuItemAddEstimationsValue`, `Click=ToolStripMenuItemAddEstimationsValue_Click` → `AddEstimationsValueEditNameFrm`, `AddEstimationsValueFrm`
- **مقايسات الإصلاح** — `Name=ToolStripMenuItemEstimationsValueList`, `Click=ToolStripMenuItemEstimationsValueList_Click` → `EstimationValuesListFrm`
- **دخول الورشة - التسكين** — `Name=ToolStripMenuItemCarEntryTaskeen`, `Click=ToolStripMenuItemCarEntryTaskeen_Click` → `CarEntryTaskeenFrm`
- **حالة أوامر الشغل** — `Name=ToolStripMenuItemCarStatusLocation`, `Click=ToolStripMenuItemCarStatusLocation_Click` → `StatusWorkOrderListFrm`
- **أوامر شغل سيارة** — `Name=ToolStripMenuItemCarWorkOrders`, `Click=ToolStripMenuItemCarWorkOrders_Click` → `CarWorkOrdersList`
- **مصنعيات أوامر الشغل** — `Name=ToolStripMenuItemEmployessItemsPrice`, `Click=ToolStripMenuItemEmployessItemsPrice_Click` → `CarsSystemLib.EmployessItemsPriceFrm`
- **بيان مصنعيات الفنيين** — `Name=ToolStripMenuItemEmployessItems`, `Click=ToolStripMenuItemEmployessItems_Click` → `Employess_All_ItemsPriceFrm`
- **مراجعة مصنعيات الفنيين** — `Name=ToolStripMenuItemEmployessItemsReview`, `Click=ToolStripMenuItemEmployessItemsReview_Click` → `Employess_Review_ItemsPriceFrm`
- **إجمالى مصنعيات أوامر الشغل خلال فترة** — `Name=ToolStripMenuItemEmployessItemsTotal`, `Click=ToolStripMenuItemEmployessItemsTotal_Click` → `Employess_Total_ItemsPrice`
- **تحديد أعمال الصنايعية** — `Name=ToolStripMenuItemEmployeeRelatedItem`, `Click=ToolStripMenuItemEmployeeRelatedItem_Click` → `CarsSystemLib.EmployeeRelatedItemFrm`
- **أنواع السيارات** — `Name=lookupCarTypeToolStripMenuItem`, `Click=lookupCarTypeToolStripMenuItem_Click` → `CarsTypeListFrm`
- **موديلات السيارات** — `Name=lookupCarTypeModelToolStripMenuItem`, `Click=lookupCarTypeModelToolStripMenuItem_Click` → *(no `new Form(...)` found in handler — see note)*
- **أصناف قطع غيار السيارات** — `Name=ToolStripMenuItemItemsForCars`, `Click=ToolStripMenuItemItemsForCars_Click` → `CarsTypeItemsFrm`
- **إضافة تقرير فحص** — `Name=ToolStripMenuItemAddExamination`, `Click=ToolStripMenuItemAddExamination_Click` → `AddExamination`
- **تقارير الفحص** — `Name=ToolStripMenuItemToolStripMenuItemExaminationList`, `Click=ToolStripMenuItemToolStripMenuItemExaminationList_Click` → *(no `new Form(...)` found in handler — see note)*

## الأصناف  <sub>`toolStripMenuItemItemsMain`</sub>

- **قائمة الأصناف ومواصفاتها - السيارات** — `Name=menuItemItemsListSpecCars`, `Click=menuItemItemsListSpecCars_Click` → `item`, `ItemsListCars`
- **قائمة الأصناف - سيراميك** — `Name=menuItemItemsListSpecSeramek`, `Click=menuItemItemsListSpecSeramek_Click` → `item`, `ItemsListSeramek`, `Mitems_List_Sermek`
- **قائمة الأصناف ومواصفاتها - أدوية** — `Name=menuItemItemsListSpecAdwia`, `Click=menuItemItemsListSpecAdwia_Click` → `item`, `ItemsListAdwia`
- **أصناف ادوية تباع بالتجزئة** — `Name=toolStripMenuItemItemsListAdwiaRetail`, `Click=toolStripMenuItemItemsListAdwiaRetail_Click` → `item`, `ItemsListAdwiaRetail`
- **إضافة صنف - أدوية** — `Name=toolStripMenuItemMitems_List_PH_R`, `Click=toolStripMenuItemMitems_List_PH_R_Click` → `Mitems_List_PH_R`
- **قائمة   الأصناف** — `Name=menuItemItemsListGrid`, `Click=menuItemItemsListGrid_Click` → `item`, `ItemsListGridFrm`, `Mitems_List_Sermek`
- **قائمة   الأصناف** — `Name=menuItemItemsListView`, `Click=menuItemItemsListView_Click` → `item`, `ItemsListViewFrm`, `Mitems_List_Sermek`
- **إضافة صنف بالمواصفات** — `Name=menuItemItemsListSpecGlass`, `Click=MenuItemItemsListSpec_Click` → `Mitems_List_WH`
- **إضافة الأصناف / الخدمات** — `Name=menuItemItemsListNoBalance`, `Click=menuItemItemsListNoBalance_Click` → `Mitems_List_NoBalance`
- **إضافة صنف** — `Name=menuItemItemsList`, `Click=menuItemItemsList_Click` → `item`, `Mitems_ListNew`, `Connection4It.Mitems_List_NoBalance`, `Connection4It.NewItemCodeList`, `Connection4It.Mitems_ListTax`
- **إضافة صنف** — `Name=menuItemItemsListNoPrice`, `Click=menuItemItemsListNoPrice_Click` → `Mitems_ListNewNoPrice`

### بيانات الأصناف  <sub>`menuItemItemData`</sub>

- ** بيانات أصناف** — `Name=menuItem109`, `Click=menuItem109_Click` → `Reporting_System.ItemDataPurchase_ListFRM`, `Store_LIB.ItemDataProduction_ListFRM`
- **بيانات أصناف منتجات المصنع - بالمخازن** — `Name=menuItem108`, `Click=menuItem108_Click` → `item`, `Mitems_List`
- **تقرير طباعة  بيانات أصناف** — `Name=menuItem107`, `Click=menuItem107_Click` → `Reporting_System.ItemProductionData.Items_ProductionDataWabaraFRM`
- **-** — `Name=menuItem53` → *(no click handler)*
- **بيانات أصناف مشتريات المصنع - بالمخازن** — `Name=menuItem110`, `Click=menuItem110_Click` → `item`, `Mitems_List`
- **تقرير قائمة بيانات أصناف المشتريات** — `Name=menuItem111`, `Click=menuItem111_Click` → `Store_LIB.ItemDataPurchase_ListFRM`

### إصناف النسيج  <sub>`menuItemItemsAllNesseg`</sub>

- **الأصناف والمنتجات** — `Name=menuProducts`, `Click=menuProducts_Click` → `item`, `Mitems_List`, `AccountModelsLib.operations`
- **تقرير طباعة  بيانات أصناف الحاف** — `Name=menuItemItems_ProductionDataFRMLehaf`, `Click=menuItemItems_ProductionDataFRMLehaf_Click` → `Reporting_System.ItemProductionData.Items_ProductionDataLehafFRM`
- **تقرير طباعة  بيانات أصناف الوبرة** — `Name=menuItemItems_ProductionDataFRMWabara`, `Click=menuItemItems_ProductionDataFRMWabara_Click` → *(no `new Form(...)` found in handler — see note)*
- **بيانات أصناف منتجات المصنع - بالمخازن** — `Name=menuItemSellItems`, `Click=menuItemSellItems_Click` → `item`, `Mitems_List`
- ** بيانات أصناف الوبرة** — `Name=menuItemItemData_ListWabara`, `Click=menuItemItemData_List_Lehaf_Click` → `Store_LIB.ItemDataProduction_ListFRM`
- ** بيانات أصناف اللحاف** — `Name=menuItemItemData_List_Lehaf`, `Click=menuItemItemData_ListWabara_Click` → `Store_LIB.ItemDataProduction_ListFRM`
- **بيانات أصناف مشتريات المصنع - بالمخازن** — `Name=menuItemItemPurchase`, `Click=menuItemItemPurchase_Click` → `item`, `Mitems_List`
- **تقرير قائمة بيانات أصناف المشتريات** — `Name=menuItem16ItemItemData_ListFRM`, `Click=menuItem16ItemItemData_ListFRM_Click` → `Store_LIB.ItemDataPurchase_ListFRM`

### أسعار الأصناف والمنتجات  <sub>`menuItemPrices`</sub>

- ** أنواع أسعار  مبيعات الأصناف** — `Name=menuPrice_Type_List`, `Click=menuPrice_Type_List_Click` → `Price_Type_ListFRM`
- **تصنيف أسعار مبيعات الأصناف** — `Name=menuPriceCategory`, `Click=menuPriceCategory_Click` → `Prices_List_Edit_Values`
- **تعديل أسعار مبيعات الأصناف** — `Name=menuItemEditPrices`, `Click=menuItemEditPrices_Click` → `Edit_Prices_List_Values`
- **بحث أسعار الصنف** — `Name=menuItemSearchItemPrices_List_Values`, `Click=menuItemSearchItemPrices_List_Values_Click` → `SearchItemPrices_List_Values`
- **طباعة أسعار الأصناف** — `Name=menuItemItemPrices`, `Click=menuItemItemPrices_Click` → `Accounts.Reports.ItemPriceFRM`
- **أصناف و أسعار العملاء** — `Name=MenuCustomer_PriceCLS`, `Click=MenuCustomer_PriceCLS_Click` → `Customer_Price`
- **أنواع سعر المشتريات** — `Name=menuItemPricePurchaseType`, `Click=menuItemPricePurchaseType_Click` → `Price_Type_List_PurchasFRM`
- **أسعار مشتريات الأصناف** — `Name=menuItemPurchaedItemPrice`, `Click=menuItemPurchaedItemPrice_Click` → `Supplier_Price`

- **أرصدة الأصناف بتاريخ الصلاحية** — `Name=menuItemItemBalanceWithExpire`, `Click=menuItemItemBalanceWithExpire_Click` → `ItemBalanceWithExpireFrm`
- **أرصدة أصناف الأدوية** — `Name=ToolStripMenuItemItemBalanceAdwia`, `Click=ToolStripMenuItemItemBalanceAdwia_Click` → `Item_balance_ph`
- **تقرير نواقص الأصناف - حد الطلب للمبيعات** — `Name=ToolStripMenuItemInvoiceUponRequest`, `Click=ToolStripMenuItemInvoiceUponRequest_Click` → `Needed_balance_phGrid`
- **أنواع فئات الأصناف** — `Name=menuItemItemMainCategory`, `Click=menuItemItemMainCategory_Click` → `ItemMainCategoryFRM`
- **(no Text)** — `Name=menuItemItemٍSubCategory` → *(no click handler)*
- **وحدات الأصناف** — `Name=menuUnits`, `Click=menuUnits_Click` → `UnitListFrm`
- **أنواع الصنف** — `Name=menuItemItem_Types`, `Click=menuItemItem_Types_Click` → `Items_Types_Name`
- **هيئات الصنف** — `Name=menuItemBodies_Item`, `Click=menuItemBodies_Item_Click` → `Items_Bodies_Name`
- **ألوان الصنف** — `Name=menuItem9Item_Color`, `Click=menuItem9Item_Color_Click` → `Items_TypesColor_Name`
- **وصف الصنف** — `Name=ToolStripMenuitemsTypesDescription`, `Click=ToolStripMenuitemsTypesDescription_Click` → `Items_TypesDescription`
- **إسم الصنف** — `Name=ToolStripMenuItems_TypesDescriptionNameOther`, `Click=ToolStripMenuItems_TypesDescriptionNameOther_Click` → `Items_TypesDescriptionNameOther`
- **مصانع الصنف** — `Name=ToolStripMenuItems_TypesFactory_Name`, `Click=ToolStripMenuItems_TypesFactory_Name_Click` → `Items_TypesFactory_Name`
- **مقاسات الصنف** — `Name=ToolStripMenuItems_TypesSize_Name`, `Click=ToolStripMenuItems_TypesSize_Name_Click` → `Items_TypesSize_Name`
- **فرز درجات الصنف** — `Name=ToolStripMenuItems_TypesDegreeFarz_Name`, `Click=ToolStripMenuItems_TypesDegreeFarz_Name_Click` → `TypesDegreeFarz_Name`
- **مجموعة الصنف** — `Name=ToolStripMenuItemItems_Groups`, `Click=ToolStripMenuItemItems_Groups_Click` → `Items_GroupFrm`
- **طبيعة الصنف** — `Name=ToolStripMenuItemItems_Nature`, `Click=ToolStripMenuItemItems_NatureFrm_Click` → `Items_NatureFrm`
- **طباعة الباركود للأدوية** — `Name=menuItemPrintBarCodeForOneItemSaidalia`, `Click=menuItemPrintBarCodeForOneItemSaidalia_Click` → `Store_LIB.PrintBarCodeForOneItemsSaidalia`
- **طباعة البار كود للصنف** — `Name=menuItemPrintBarCodeForOneItemWithPrice`, `Click=menuItemPrintBarCodeForOneItemWithPrice_Click` → `Store_LIB.PrintBarCodeForOneItemsWithPrice`
- **طباعة البار كود قائمة الأصناف** — `Name=toolStripMenuItemBill_PrintItemsBarCodeItemsList`, `Click=toolStripMenuItemBill_PrintItemsBarCodeItemsList_Click` → `Store_LIB.Bill_PrintItemsBarCodeItemsList`
- **toolStripMenuItem3** — `Name=toolStripMenuItem3` → *(no click handler)*
- **طباعة البار كود للصنف** — `Name=menuItemPrintBarCodeForOneItem`, `Click=menuItemPrintBarCodeForOneItem_Click` → `Store_LIB.PrintBarCodeForOneItems`

## المخازن  <sub>`ToolStripMenuItemStoreTransaction`</sub>

### أذونات الصرف والإستلام  <sub>`menuItemOrderTransaction`</sub>

- **إذن صرف / تحويل لمخزن أو قسم** — `Name=menuItemOrderSarf`, `Click=menuItemOrderSarf_Click` → `TransferItemsWorkNew`, `Order_DetailsWorkNewFrm`
- **إذن مرتجع / تحويل  لمخزن أو قسم** — `Name=menuItemOrderMortaga3Sarf`, `Click=menuItemOrderMortaga3Sarf_Click` → `TransferItemsWorkNew`, `Order_DetailsWorkNewFrm`
- **إذن إستلام من مورد** — `Name=menuItemOrderEstlamSupplier`, `Click=menuItemOrderEstlamSupplier_Click` → *(no `new Form(...)` found in handler — see note)*
- **إذن مرتجع إلى مورد** — `Name=menuItemOrderEstlamMortaga3Supplier`, `Click=menuItem47_Click` → `TransferItemsWorkNew`, `Order_DetailsWorkNewFrm`
- **إذن صرف قطع غيار الماكينات وخطوط الإنتاج** — `Name=menuItemOrderSarfMachine`, `Click=menuItemOrderSarfMachine_Click` → *(no `new Form(...)` found in handler — see note)*
- **إذن مرتجع قطع غيار الماكينات وخطوط الإنتاج** — `Name=menuItemOrderEstlamFromMachine`, `Click=menuItemOrderEstlamFromMachine_Click` → `Order_DetailsWorkNewFrm`
- **إذن صرف لعميل** — `Name=menuItemOrderSarfCustomer`, `Click=menuItemOrderSarfCustomer_Click` → `Order_DetailsWorkNewFrm`
- **إذن مرتجع من عميل** — `Name=menuItemOrderSarfMortagahCustomer`, `Click=menuItem39_Click_1` → `Order_DetailsWorkNewFrm`
- **إذن تسوية بالزيادة** — `Name=menuItemOrderTasweaZeada`, `Click=menuItem48_Click` → `Order_DetailsWorkNewFrm`
- **إذن تسوية بالنقص** — `Name=menuItemOrderTasweaNakis`, `Click=menuItem49_Click` → `Order_DetailsWorkNewFrm`
- **إذن إستلام من الإنتاج** — `Name=menuItemOrderTasweaEntag`, `Click=menuItem50_Click` → `Order_DetailsWorkNewFrm`
- **إذن صرف إلى الإنتاج** — `Name=menuItemOrderSarfToEntag`, `Click=menuItem51_Click` → `Order_DetailsWorkNewFrm`
- **إذن صرف إلى مخزن خارجى** — `Name=menuItemOrderSarfFromOuterStore2`, `Click=MenuItemOrderSarfFromOuterStore2_Click` → `Order_DetailsWorkNewFrm`
- **إذن إستلام من مخزن خارجى** — `Name=menuItemOrderEstlamFromOuterStore2`, `Click=MenuItemOrderEstlamFromOuterStore2_Click` → `Order_DetailsWorkNewFrm`
- **إذن صرف قطع غيار** — `Name=menuItemOrderSarfToKeta3`, `Click=menuItem52_Click` → `Order_DetailsWorkNewFrm`
- **إذن صرف للهالك** — `Name=menuItemOrderSarfToHalek`, `Click=menuItem149_Click` → `Order_DetailsWorkNewFrm`
- **إذن صرف إلى  المخزن** — `Name=menuItemOrderSarfToStore`, `Click=menuItemOrderSarfToStore_Click` → `Order_DetailsWorkNewFrm`
- **إذن إستلام من المخزن** — `Name=menuItemOrderEstlamFromStore`, `Click=menuItemOrderEstlamFromStore_Click` → `Order_DetailsWorkNewFrm`
- **إذن صرف للورشة** — `Name=menuItemOrderSarfWarsha`, `Click=menuItemOrderSarfToWarsha_Click` → `Order_DetailsWorkNewFrm`
- **إذن استلام / مرتجع صرف من الورشة** — `Name=menuItemOrderEstlamFromWarsha`, `Click=menuItemOrderEstlamFromWarsha_Click` → `TransferItemsWorkNew`, `Order_DetailsWorkNewFrm`
- **إذن صرف لموظف** — `Name=menuItemOrderSarfEmployee`, `Click=menuItemOrderSarfEmployee_Click` → `Order_DetailsWorkNewFrm`
- **إذن إستلام من موظف** — `Name=menuItemOrderEstlamFromEmployee`, `Click=menuItemOrderEstlamFromEmployee_Click` → `Order_DetailsWorkNewFrm`
- ** إذن صرف لقسم** — `Name=menuItemOrderSarfDepartment`, `Click=menuItemOrderSarfDepartment_Click` → *(no `new Form(...)` found in handler — see note)*
- **إذن إستلام من قسم** — `Name=menuItemOrderEstlamFromDepartment`, `Click=menuItemOrderEstlamFromDepartmentToolStripMenuItem_Click` → *(no `new Form(...)` found in handler — see note)*

### حركة الأصناف  <sub>`menuItemItemTransaction`</sub>

- **كارت الصنف** — `Name=menuItemCardsStore`, `Click=menuItem55_Click` → `ItemsCards`
- **حركة المخازن** — `Name=menuItemStoreTransaction`, `Click=menuItem57_Click` → `ItemsCardsStore`
- **تقرير يومية المخزن** — `Name=menuItemStoreDays`, `Click=menuItem58_Click` → `ItemsCardsStore_Days`
- **حركة المخازن أصناف مبيعات** — `Name=menuItem59menuItemSellsStoreTransaction`, `Click=menuItem59_Click` → `ItemsCardsStore`
- **حركة المخازن أصناف مشتريات** — `Name=menuItem59menuItemPurchaseStoreTransaction`, `Click=menuItem60_Click` → `ItemsCardsStore`
- **تقرير يومية المخزن أصناف مبيعات** — `Name=menuItemStoreDaysSells`, `Click=menuItem61_Click` → `ItemsCardsStore_Days`
- **حركة الأصناف فى المخازن عن فترة** — `Name=menuItemItemTrasaction`, `Click=menuItem144_Click` → `Item_Transaction_BalanceL2`
- **الرصيد الإفتتاحى للأصناف** — `Name=menuItemItemFirstBalance`, `Click=menuItem147_Click` → `Store_LIB.Item_FirstBalance`
- **حركة الصنف / كارت الصنف** — `Name=ToolStripMenuItem_Transaction_Details`, `Click=ToolStripMenuItem_Transaction_Details_Click` → `Item_Transaction_Details`
- **حركة الصنف / كارت الصنف كمية وقيمة** — `Name=ToolStripMenuItem_Transaction_DetailsWithCost`, `Click=ToolStripMenuItem_Transaction_DetailsWithCost_Click` → *(no `new Form(...)` found in handler — see note)*
- **تعديل أرصدة المخازن** — `Name=menuItemEditStoreBalance`, `Click=menuItem54_Click` → `Store_LIB.balance_`
- **تحويل أصناف مخزن لأخر برصيد صفر** — `Name=menuItemAddItemsToStore`, `Click=menuItemAddItemsToStore_Click` → `Add_ALLItemFromTo_Store_`

- **تعديل أرصدة الأصناف** — `Name=menuItemItemBalanceOnly2`, `Click=menuItemItemBalanceOnly2_Click` → `ItemBalanceWithExpireFrm`, `Store_LIB.balanceOnlyEdit`
- **تعديل أرصدة الأصناف** — `Name=menuItemItemBalanceOnlyEdit`, `Click=menuItemItemBalanceOnlyEdit_Click` → `Store_LIB.balanceOnlyEdit_Units`
- **أرصدة الأصناف بتورايخ سابقة** — `Name=menuItemItemsListBeforBalance`, `Click=MenuItemItemsListBeforBalance_Click` → `Store_LIB.Accountss.Item_Balance_Date`
- **حفظ أرصدة الأصناف بالتاريخ والوقت الحالى** — `Name=menuItemItemsListSavedBalance`, `Click=MenuItemItemsListSavedBalance_Click` → *(no `new Form(...)` found in handler — see note)*
- **صرف أصناف أوامر التشغيل** — `Name=ToolStripMenuItemWorkOrderListWithNewItems`, `Click=ToolStripMenuItemWorkOrderListWithNewItems_Click` → *(no `new Form(...)` found in handler — see note)*
- **الأصناف المصروفة لأوامر الشغل** — `Name=ToolStripMenuItemCarWorkOrderItemsSarf`, `Click=ToolStripMenuItemCarWorkOrderItemsSarf_Click` → `CarWorkOrderItem_Sarf_FromStoreFrm`

## يوميات الوكالة  <sub>`ToolStripMenuItemdaysWekala`</sub>

- **استلام من مورد متعدد** — `Name=ToolStripMenuItemBatchReceiptMultiple`, `Click=ToolStripMenuItemBatchReceiptMultiple_Click` → `FrmBatchReceiptMultiple`
- **استلام صنف من مورد** — `Name=ToolStripMenuItemBatchReceiptSingle`, `Click=ToolStripMenuItemBatchReceiptSingle_Click` → `FrmBatchReceiptMultiple`
- **بيان إستلام أصناف** — `Name=toolStripMenuItemFrmBatchReceiptMultiple`, `Click=toolStripMenuItemFrmBatchReceiptMultiple_Click` → `FrmBatchItemsReceiptList`
- **بيان بالموردين موردى الأصناف خلال فترة** — `Name=ToolStripMenuItemBatchReceiptList`, `Click=ToolStripMenuItemBatchReceiptList_Click` → `FrmBatchReceiptList`
- **قائمة البيع  اليومى للعملاء** — `Name=ToolStripMenuItemFrmSales_CustomerList`, `Click=ToolStripMenuItemFrmSales_CustomerList_Click` → `FrmSales_CustomerList`
- **شاشة البيع اليومية للعميل** — `Name=ToolStripMenuItemFrmDailyCustomerSales`, `Click=aToolStripMenuItemFrmDailyCustomerSales_Click` → `FrmDailyCustomerSales`
- **قائمة البيع اليومية للأصناف ** — `Name=ToolStripMenuItemFrmSales_ItemsList`, `Click=ToolStripMenuItemFrmSales_ItemsList_Click` → `FrmSales_ItemsList`
- **شاشة بيع صنف** — `Name=toolStripMenuItemFrmDailyCustomerSalesListReport`, `Click=toolStripMenuItemFrmDailyCustomerSalesListReport_Click` → `FrmDailyCustomerSalesSingle`
- **شاشة المبيعات اليومية للصنف** — `Name=frmDailyItemSalesToolStripMenuItem`, `Click=ToolStripMenuItemDailyItemSales_Click` → `FrmDailyItemSales`
- **شاشة المبيعات اليومية لكل الصنف** — `Name=FrmDaily_All_ItemSalesToolStripMenuItem`, `Click=FrmDaily_All_ItemSalesToolStripMenuItem_Click` → `FrmDaily_All_ItemSales`
- **بيان مبيعات الأصناف** — `Name=ToolStripMenuItemFrmDailyCustomerSalesSingle`, `Click=ToolStripMenuItemFrmDailyCustomerSalesSingle_Click` → `FrmDailyCustomerSalesListReport`

## مراكز التجميل  <sub>`ToolStripMainMenuBeautyCenter`</sub>

- **الخدمات** — `Name=aToolStripMenuItemFrmServicesList`, `Click=aToolStripMenuItemFrmServicesList_Click` → `FrmServicesList`
- **قائمة فئات الخدمات** — `Name=ToolStripFrmServiceCategoryList`, `Click=ToolStripFrmServiceCategoryList_Click` → `FrmServiceCategoryList`
- **تحديد  أعمال وخدمات الموظفات** — `Name=ToolStripMenuEmployeeRelatedItemServicesFrm`, `Click=ToolStripMenuEmployeeRelatedItemServicesFrm_Click` → `EmployeeRelatedItemServicesFrm`
- **حجز موعد** — `Name=dToolStripMenuItem`, `Click=dToolStripMenuItem_Click` → `FrmAppointment`
- **عرض الحجز** — `Name=ToolStripMenuFrmAppointment`, `Click=ToolStripMenuFrmAppointment_Click` → `FrmAppointment`
- **الحجوزات** — `Name=ToolStripMenuCalendarAppointmentsFrm`, `Click=ToolStripMenuCalendarAppointmentsFrm_Click` → `CalendarAppointmentsFrm`
- **الحجوزات** — `Name=cToolStripMenuItem2`, `Click=cToolStripMenuItem2_Click` → `CalendarAppointmentsForm`
- **d** — `Name=dToolStripMenuItem1`, `Click=dToolStripMenuItem1_Click` → `FrmInvoiceCreate`
- **الجلسات** — `Name=ssToolStripMenuItem`, `Click=ssToolStripMenuItem_Click` → `FrmSessions`
- **إدارة باقات الجلسات** — `Name=ToolStripMenuFrmPackages`, `Click=ToolStripMenuFrmPackages_Click` → `FrmPackagesListMulti`
- **إدارة باقات العملاء** — `Name=ToolStripMenuFrmCustomerPackagesList`, `Click=ToolStripMenuFrmCustomerPackagesList_Click` → `FrmCustomerPackagesList`
- **إضافة جلسة** — `Name=ToolStripMenuFrmAddPackageSession`, `Click=ToolStripMenuFrmAddPackageSession_Click` → `DashboardForm`, `FrmAddPackageSession2`

## المبيعات  <sub>`menuSell`</sub>

### الطلبيات  <sub>`menuMainRequest`</sub>

- **إضافة طلبية** — `Name=menuNewrequests`, `Click=menuNewrequests_Click` → `Accounts.Requests.Add_Requests`
- **عرض الطلبيات** — `Name=menuRequests_Show`, `Click=menuRequests_Show_Click` → `Accounts.Requests.Request_To_Market`
- **بحث الطلبيات ذات المبالغ المدفوعة** — `Name=menuRequestWithMoney`, `Click=menuRequestWithMoney_Click` → `Accounts.Requests.Request_With_Money`

- **فاتورة جديدة نقدى** — `Name=toolStripMenuItemNewInvoiceMoney`, `Click=toolStripMenuItemNewInvoiceMoney_Click` → `Accounts.Requests.Make_Invoices`
- **فاتورة جديدة ** — `Name=menuNewInvoice`, `Click=menuNewInvoice_Click` → *(no `new Form(...)` found in handler — see note)*
- **فاتورة جديدة - حفظ الأصناف** — `Name=menuNewInvoiceToSaveDirectly`, `Click=menuNewInvoiceToSaveDirectly_Click` → *(no `new Form(...)` found in handler — see note)*
- **فاتورة جديدة قطاعى** — `Name=menuNewInvoiceIs_Kta3y`, `Click=MenuNewInvoiceIs_Kta3y_Click` → `Accounts.Requests.Make_Invoices`
- **فاتورة جديدة  ق.ع** — `Name=menuNewInvoiceGov`, `Click=menuNewInvoiceGov_Click` → `Invoices`, `Accounts.Requests.Make_Invoices`
- **فاتورة مرتجع مبيعات نقدى** — `Name=menuInvoiceReverseNakdy`, `Click=menuInvoiceReverseNakdy_Click` → `Accounts.Requests.Make_Invoices_Reverse`
- **فاتورة مرتجع مبيعات** — `Name=menuInvoiceReverse`, `Click=menuInvoiceReverse_Click` → `Accounts.Requests.Make_Invoices_Reverse`
- **فاتورة مرتجع قطاعى** — `Name=menuNewInvoiceIs_MortagahKta3y`, `Click=MenuNewInvoiceIs_MortagahKta3y_Click` → `Accounts.Requests.Make_Invoices_Reverse`
- **فاتورة مبيعات إجمالى** — `Name=menuNewInvoiceGovTotal`, `Click=menuItem17_Click` → `Accounts.InvoicesFrm.invoices.InvoicesTotal`
- **فاتورة مبيعات إجمالى ق.خ** — `Name=menuNewInvoiceTotal`, `Click=menuItem18_Click` → `Accounts.InvoicesFrm.invoices.InvoicesTotal`
- **مبيعات دشت لمورد** — `Name=menuItemDestructionSales`, `Click=menuItemDestructionSales_Click` → `DestructionSales.Suppliers_DestructionFrm`
- **فاتورة  مردود مبيعات إجمالى** — `Name=menuInvoiceReverseTotal`, `Click=menuInvoiceReverseTotal_Click` → `Accounts.InvoicesFrm.invoices.InvoicesReverseTotal`
- **تسجيل الطلبيات** — `Name=menuItemTalabia`, `Click=menuItemTalabia_Click` → `Receve_Talabia`
- ** عرض فواتير  المبيعات** — `Name=menuInvoices`, `Click=menuInvoices_Click` → `Invoices_ListFrm`
- ** عرض فواتير مرتجع المبيعات** — `Name=menuInvoicesReverse`, `Click=menuInvoicesReverse_Click` → `Invoices_Reverse_List`
- **أرصدة الفواتير - مدفوعات وتوزيع نقدية** — `Name=toolStripMenuItemInvoices_List_ReceiptDistributionReport`, `Click=toolStripMenuItemInvoices_List_ReceiptDistributionReport_Click` → `Invoices_List_ReceiptDistributionReport`
- **شاشة متابعة الفواتير المستحقة - تقرير أعمار الديون (Aging Report)** — `Name=toolStripMenuItemInvoices_List_WithDueDate`, `Click=toolStripMenuItemInvoices_List_WithDueDate_Click` → `Invoices_List_ReceiptDistributionReport`
- **أرصدة الفواتير - مدفوعات** — `Name=toolStripMenuItemInvoices_List_Payment_`, `Click=toolStripMenuItemInvoices_List_Payment__Click` → `InvoicesFrm.invoices.Invoices_List_Payment_`
- ** عرض الفواتير  ق.ع** — `Name=menuInvoicesGov`, `Click=menuInvoicesGov_Click` → `Invoices_ListFrm`

### تقسيط المبيعات  <sub>`ToolStripMenuItemTakseet`</sub>

- **بيانات الأقساط** — `Name=ToolStripMenuItemTakseet_List`, `Click=ToolStripMenuItemTakseet_List_Click` → `Takseet_List`


## المشتريات  <sub>`menuPurchase`</sub>

- **فاتورة جديدة ق.ع** — `Name=menuNewBillGov`, `Click=menuNewBillGov_Click` → `Bills`, `Make_Bill`
- **فاتورة جديدة ** — `Name=menuNewBill`, `Click=menuNewBill_Click` → `Make_Bill`, `Bills`
- **فاتورة جديدة - حفظ الأصناف** — `Name=menuNewBillWithSaveItems`, `Click=menuNewBillWithSaveItems_Click` → `Make_Bill`
- **فاتورة جديدة نقدي** — `Name=menuNewBillNakdy`, `Click=menuNewBillNakdy_Click` → `Make_Bill`
- **فاتورة مرتجع المشتريات** — `Name=menuBillReverse`, `Click=menuBillReverse_Click` → `Make_Bill_Reverse`
- **أسعار البيع للمشتريات** — `Name=toolStripBillsList_Edit_SPriceFrm`, `Click=toolStripBillsList_Edit_SPriceFrm_Click` → *(no `new Form(...)` found in handler — see note)*
- **تقارير مشتريات أصناف** — `Name=menuPurchaseReport_2`, `Click=menuPurchaseReport_2_Click` → *(no `new Form(...)` found in handler — see note)*
- ** عرض فواتير المشتريات** — `Name=menuBillsGov`, `Click=menuBillsGov_Click` → `Bills_ListFrm`
- ** عرض فواتير  مرتجع المشتريات** — `Name=menuBillListReverse`, `Click=menuBillListReverse_Click` → `Bills_List_Reverse`
- ** عرض الفواتير  ق.خ** — `Name=menuBills`, `Click=menuBills_Click` → `Bills_ListFrm`

## تعاملات المندوبين  <sub>`menuSalesManProcess`</sub>

- **فواتير المبيعات** — `Name=menuSalesInvoiceSalesman`, `Click=menuPulled_Items_Click` → `Accounts.Requests.Pulles_Items`, `Accounts.Requests.Make_Invoices`
- **مردودات المبيعات** — `Name=menuSalesInvoiceReverse`, `Click=menuSalesInvoiceReverse_Click` → `Accounts.Requests.Make_Invoices_Reverse`
- **إذن إستلام شيكات** — `Name=menuItemSalesOrderCheck`, `Click=menuItemSalesOrderCheck_Click` → `Order_Receipt_ChecksFrmNew`
- **إذن إستلام نقدية** — `Name=menuItemSalesOrderCach`, `Click=menuItemSalesOrderCach_Click` → `Order_Receipt_CashFrm`, `OrderMoneyPrint`, `New_OrderMoney_SarfNew`
- **حساب مندوب المبيعات** — `Name=menuItemSalesSccounts`, `Click=menuItemSalesSccounts_Click` → `Customer_AccountMokhtalat`

## أوراق القبض  <sub>`menuMoney`</sub>

- **إذن إستلام شيكات** — `Name=menuItemReceiveChech`, `Click=menuItemReceiveChech_Click` → *(no `new Form(...)` found in handler — see note)*
- **إذن صرف شيكات** — `Name=menuItemSendCheck`, `Click=menuItemSendCheck_Click` → *(no `new Form(...)` found in handler — see note)*
- **متابعة الشيكات** — `Name=menuCheckFollow`, `Click=menuCheckFollow_Click` → `Imports_Checks_List`
- **تقارير الشيكات الواردة والصادرة** — `Name=menuItemCheckReport`, `Click=menuItemCheckReport_Click` → `Imports_Checks_ListReport`
- **شيكات اقترب موعد استحقاقها** — `Name=menuItemCheckReportWithDueDate`, `Click=menuItemCheckReportWithDueDate_Click` → `ChecksWithDueDate`
- **الشيكات الصادرة / المحولة** — `Name=menuItemMe7weal`, `Click=menuItemMe7weal_Click` → `Export_Checks_List`
- **إذن إستلام نقديه** — `Name=ToolStripMenuItemEstlamMoney`, `Click=ToolStripMenuItemEstlamMoney_Click` → `New_OrderMoney_SarfNew`, `New_OrderMoney_Transaction`
- **إذن صرف نقديه** — `Name=ToolStripMenuItemSarfMoney`, `Click=ToolStripMenuItemSarfMoney_Click` → `New_OrderMoney_SarfNew`, `New_OrderMoney_Transaction`
- **إذن إستلام نقديه** — `Name=xEstlamMoneyCurrency`, `Click=xEstlamMoneyCurrency_Click` → `New_OrderMoney_SarfCurrencyNew`, `New_OrderMoney_Transaction`
- **إذن صرف نقديه** — `Name=ToolStripMenuItemSarfMoneyCurrency`, `Click=ToolStripMenuItemSarfMoneyCurrency_Click` → `New_OrderMoney_SarfCurrencyNew`, `New_OrderMoney_Transaction`
- **إذن إستلام نقديه - كل الخزن** — `Name=ToolStripMenuItemEstlamMoneyManyBox`, `Click=ToolStripMenuItemEstlamMoneyManyBox_Click` → `OrderMoney_TransactionManyForAccounts`, `New_OrderMoney_Transaction`
- **إذن صرف نقديه كل الخزن** — `Name=ToolStripMenuItemSarfMoneyManyBox`, `Click=ToolStripMenuItemSarfMoneyManyBox_Click` → `OrderMoney_TransactionManyForAccounts`, `New_OrderMoney_Transaction`

### إذونات النقدية  <sub>`menuItemAllOrderSimple`</sub>

- **إذن إستلام نقدية من عميل** — `Name=menuItemCustomer_Money`, `Click=menuItemCustomer_Money_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.OrderMoney_Transaction`
- **إذن صرف نقدية لمورد** — `Name=menuItemSuplierMoney`, `Click=menuItemSuplierMoney_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.OrderMoney_Transaction`
- **إذن إستلام نقدية (مرتجع) من مورد** — `Name=menuItemSuplierMoneyReverse`, `Click=menuItemSuplierMoneyReverse_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.OrderMoney_Transaction`
- **إذن صرف  نقدية  (مرتجع) لعميل** — `Name=menuItemCustomerMoneyReverse`, `Click=menuItemCustomerMoneyReverse_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.OrderMoney_Transaction`

- **تحويل نقديه من صندوق إلى أخر** — `Name=toolStripMenuItemMoneyBetweenBanks`, `Click=toolStripMenuItemMoneyBetweenBanks_Click` → `Money_Between_BoxTar7eelFrm`
- **مدفوعات نقدية من العملاء** — `Name=menuItemCustomerMoney`, `Click=menuItemCustomerMoney_Click` → `Cash_Money_ListFromCustomers`
- **مدفوعات نقدية إلى الموردين** — `Name=menuItemMonyToSuppliers`, `Click=menuItemMonyToSuppliers_Click` → `Cash_Money_ListToSuppliers`
- **حركة النقديه للحسابات** — `Name=ToolStripMenuItem_Cash_Money_ListFromAccounts`, `Click=ToolStripMenuItem_Cash_Money_ListFromAccounts_Click` → `Cash_Money_ListFromAccounts`
- **بحث أذونات النقديه** — `Name=menuItemSearchOrders`, `Click=menuItemSearchOrders_Click` → `Cash_Money_List`
- **توزيع سند قبض على الفواتير** — `Name=ToolStripMenuInvoices_List_ReceiptDistribution`, `Click=ToolStripMenuInvoices_List_ReceiptDistribution_Click` → `Invoices_List_ReceiptDistribution`

## حركة الخزينة والبنوك  <sub>`menuBanks`</sub>

- **حركة الخزينة اليومى** — `Name=menuMoney_Transaction_Box`, `Click=menuMoney_Transaction_Box_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_BoxFrm`
- **يومية حركة الصندوق** — `Name=menuItemDaysBox`, `Click=menuItemDaysBox_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.DaysMoney_Transaction_Box`
- **حركة الخزينة عن فترة** — `Name=menumenuMoney_Transaction_BoxInterval`, `Click=menumenuMoney_Transaction_BoxInterval_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_BoxFrm`
- **حركة الخزينة اليومى** — `Name=menuMoney_Transaction_BoxPeriod`, `Click=menuItem129_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_BoxPeriod`
- **حركة الخزينة عن فترة** — `Name=menuMoney_Transaction_BoxPeriodBy`, `Click=menuItem130_Click` → `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_BoxPeriod`
- **تقرير إجمالى يوميات الخزينة عن فترة** — `Name=menuMoney_TransactionTotal`, `Click=menuItem132_Click_1` → `Money_Transaction_BoxTotalDay`
- **كشف حركة الخزينة عن يوم** — `Name=aToolStripMenuItemMoneyDay`, `Click=aToolStripMenuItemMoneyDay_Click` → `Account_Common_LIB.New_Money_Transaction.BanksMoneyTransaction`
- **كشف حركة الخزينة تفصيلى عن يوم** — `Name=ToolStripMenuItemMoneyDayDetails`, `Click=ToolStripMenuItemMoneyDayDetails_Click` → `Account_Common_LIB.New_Money_Transaction.BanksMoneyTransactionAccounts`

## معادلات الإنتاج  <sub>`menuItemProductionMenuNew`</sub>

- **أوامر شغل الإنتاج** — `Name=toolStripMenuItemFrmWorkOrderList`, `Click=toolStripMenuItemFrmWorkOrderList_Click` → `FrmWorkOrderList`
- **إنشاء أمر شغل إنتاج** — `Name=toolStripMenuItemFrmWorkOrderEdit`, `Click=toolStripMenuItemFrmWorkOrderEdit_Click` → `FrmWorkOrderEdit`

### إنتاج النسيج  <sub>`menuItemProductionMenu`</sub>

- **ورديات المصنع** — `Name=menuItemWardiat`, `Click=menuItemWardiat_Click` → `ProductionsRates.Wardiat`

#### أوامر تشغيل النسيج  <sub>`menuItemOrderTash3heel`</sub>

- **أوامر التشغيل الجديدة** — `Name=menuItemNewOrderWorking`, `Click=menuItemNewOrderWorking_Click` → `ProductionsRates.NewOrderWork_FRM`
- **أوامر تشغيل الماكينات** — `Name=menuItemOrderWorkList`, `Click=menuItemOrderWorkList_Click` → `ProductionsRates.OrderWorking.OrderWorkListFRM`

#### النسيج  <sub>`menuItemNaseeg`</sub>

- **مكينات النسيج** — `Name=menuItemTextileMachine`, `Click=menuItemTextileMachine_Click` → `ProductionsRates.MachineTextileFrm`
- **إدخال إنتاج النسيج** — `Name=menuItemProdctionTextile`, `Click=menuItemProdctionTextile_Click` → `ProductionsRates.Textile_Production_Rates_New`
- **إدخال  إنتاج نسيج الوبرة من أمر التكليف** — `Name=menuItemmenuItemProdctionTextileOrderWabara`, `Click=menuItemmenuItemProdctionTextileOrder_Click` → `ProductionsRates.Textile_Production_Rates_OrderWabara`
- **إدخال  إنتاج نسيج اللحاف من أمر التكليف** — `Name=menuItemmenuItemProdctionTextileOrderLehaf`, `Click=menuItemmenuItemProdctionTextileOrderLehaf_Click` → `ProductionsRates.Textile_Production_Rates_OrderLehaf`
- **عرض  إنتاج  عامل النسيج** — `Name=menuItemTextileProductionSearch`, `Click=menuItemTextileProductionSearch_Click` → `ProductionsRates.Textile_ProductionRateSearch`
- **تقرير معدلات إنتاج الوبرة اليومى** — `Name=menuItemWabaraDay`, `Click=menuItem37_Click` → `ProductionsRates.Textile_Efficiency_DetailsWabara`
- **تقرير معدلات إنتاج اللحاف اليومى** — `Name=menuItemLehafDay`, `Click=menuItemLehafDay_Click` → `ProductionsRates.Textile_Efficiency_DetailsLehaf`
- **تقرير معدلات إنتاج الوبرة** — `Name=menuItemProductionRate`, `Click=menuItemProductionRate_Click` → `ProductionsRates.Textile_EfficiencyWabara`

- **أنواع العطلات** — `Name=menuItemAtalat`, `Click=menuItemAtalat_Click` → `ProductionsRates.AtalatListFrm`

#### صيانة الماكينات  <sub>`menuItemMaintaince`</sub>

- **قائمة بعمليات صيانة الميكنة** — `Name=menuItemMaintaining_Item_List`, `Click=menuItemMaintaining_Item_List_Click` → `ProductionsRates.Maintaining_Item_ListFRM`
- **عمليات صيانة المكينات** — `Name=menuItemMaintaingOperation`, `Click=menuItemMaintaingOperation_Click` → `ProductionsRates.Maintaining_OperationsFrm`

#### إنتاج السدا والمبرزاتى  <sub>`menuItemSdea`</sub>

- **بيانات المطواة** — `Name=menuItemMetwaList`, `Click=menuItemMetwaList_Click` → `ProductionsRates.Matwa_ListFRM`
- **إنتاج السدا** — `Name=menuItem15_Seda`, `Click=menuItem15_Seda_Meparzaty_Click` → `ProductionsRates.Matwa_SedaProductionRate`
- **إنتاج المبرزاتى** — `Name=menuItem_Meparzaty`, `Click=menuItem_Meparzaty_Click` → `ProductionsRates.Matwa_SedaProductionRate`
- **إرسال المطواة إلى مكينات النسيج** — `Name=menuItemSendMetawaToTextile`, `Click=menuItemSendMetawaToTextile_Click` → `ProductionsRates.Matwa_Production_Send`
- **أرصدة أصناف المطواة** — `Name=menuItemMatwaBalanceItem`, `Click=menuItemMatwaBalanceItem_Click` → `ProductionsRates.Form1`

#### رسمات الأصناف  <sub>`menuItemRasmat`</sub>

- **بيانات  رسمات الأصناف** — `Name=menuItemItemPrice`, `Click=menuItemItemPrice_Click` → `Store_LIB.ItemProductionData_ListFRM`

#### التطريز  <sub>`menuItemTatreez`</sub>

- **مكينات التطريز** — `Name=menuItemMachinTatreez`, `Click=menuItemMachinTatreez_Click` → `ProductionsRates.MachineTatreezFrm`
- **معدلات إنتاج التطريز** — `Name=menuItemTatreezProductionRate__`, `Click=menuItemTatreezProductionRate___Click` → `ProductionsRates.EmpProductionRate_`

#### حسابات خاصة  <sub>`menuSpecialAccount`</sub>

##### حساب النزلات  <sub>`Menu_Adel_`</sub>

- **إضافة ميكنه ** — `Name=menuItem22`, `Click=menuItem22_Click` → `Capita_frm`
- **إضافة عدد من النزلات** — `Name=menuItem25`, `Click=menuItem25_Click` → `tottalof_nazlla`
- **بيان بتفاصيل النزلات** — `Name=menuItem23`, `Click=menuItem23_Click` → `frm_View_detaile_nazlla`

##### حساب الشركاء   <sub>`menu_Capital_account`</sub>

- **إضافة شريك** — `Name=menuItem12`, `Click=menuItem12_Click` → `Capital_frm`
- **بيان مسحوبات الشركاء** — `Name=menuItem14`, `Click=menuItem14_Click` → `frm_View_detaile`
- **سحب مبلغ نقدى لشريك** — `Name=menuItem21`, `Click=menuItem21_Click` → `CapitalFRM`



### تشغيل خارجى  <sub>`menuWorkOut`</sub>

- **إرسال طلبية تشغيل خارجى** — `Name=menuSendWorkOut`, `Click=menuSendWorkOut_Click` → `Accounts.Work_Out.Add_Requests_Work_Out`
- **إستلام  طلبية تشغيل خارجى** — `Name=menuReceiveWorkOut2`, `Click=menuReceiveWorkOut2_Click` → `Accounts.Work_Out.Receive_Requests_Work_Out`
- **مبيعات هالك تشغيل خارجى** — `Name=menuSalesDestruction`, `Click=menuSalesDestruction_Click` → `Accounts.Work_Out.Receive_Destruction_Work_Out`

#### تقارير بالأصناف   <sub>`menuItemsReports`</sub>

- **الأصناف المستلمة** — `Name=menuItemsReceived`, `Click=menuItemsReceived_Click_1` → `Accounts.Work_Out.Work_Out_Reports_Receive`
- **الأصناف المرسلة** — `Name=menuItemsSend`, `Click=menuItemsSend_Click` → `Accounts.Work_Out.Work_Out_Reports_SendANDBalance`
- **أرصدة الأصناف عند الموردين** — `Name=menuItemsBalance`, `Click=menuItemsBalance_Click` → `Accounts.Work_Out.Work_Out_Reports_SendANDBalance`


### تشغيل / تصنيع للغير  <sub>`menuWorkIn`</sub>

- **إستلام طلبية تشغيل للغير** — `Name=menuEstlamWorkIn`, `Click=MenuEstlamWorkIn_Click` → `Accounts.Work_In.Add_Requests_Work_In`
- **إرسال طلبية تشغيل للغير** — `Name=menuSendWorkIn`, `Click=MenuSendWorkIn_Click` → `Work_In.Receive_Requests_Work_In`
- **مبيعات هالك تشغيل للغير** — `Name=menuSalesDestructionIN`, `Click=MenuSalesDestructionIN_Click` → `Accounts.Work_In.Receive_Destruction_Work_In`

#### تقارير أصناف تصنيع للغير  <sub>`menuItemsReportsIn`</sub>

- **الأصناف المرسله** — `Name=menuItemsReceivedIn`, `Click=MenuItemsReceivedIn_Click` → `Accounts.Work_In.Work_In_Reports_Receive`
- **الأصناف المستلمة** — `Name=menuItemsSendIn`, `Click=MenuItemsSendIn_Click` → `Accounts.Work_In.Work_In_Reports_SendANDBalance`
- **أرصدة الأصناف للعملاء** — `Name=menuItemsBalanceIn`, `Click=MenuItemsBalanceIn_Click` → `Accounts.Work_In.Work_In_Reports_SendANDBalance`


- **مكونات الأصناف** — `Name=menuItemRawMaterial`, `Click=menuItem106_Click` → `SubItemListRawFrm`
- **مراجعة مكونات الأصناف** — `Name=menuItemRawMaterialReview`, `Click=menuItem112_Click` → `ItemRawsReviews`
- **تغيير هوية صنف** — `Name=menuItemChangeItemRaw`, `Click=menuItemChangeItemRaw_Click` → `Order_DetailsWorkMulti`
- **تقرير تغيير هوية صنف** — `Name=menuItemChangeItemRawList`, `Click=menuItemChangeItemRawList_Click` → `ChangeItemRaw_List`
- **إذن إستلام من مخزن خارجى** — `Name=menuItemOrderEstlamFromOuterStore`, `Click=MenuItemOrderEstlamFromOuterStore_Click` → `Order_DetailsWorkNewFrm`
- **تقرير  قائمة الأصناف المنتجة** — `Name=menuItemItem_Raw_MaterialsFor_EntageFrm`, `Click=menuItemItem_Raw_MaterialsFor_EntageFrm_Click` → `Item_Raw_MaterialsFor_EntageFrm`
- **تقرير خامات الأصناف المنتجة** — `Name=menuItemItem_Raw_Materials_DetailsFrm`, `Click=menuItemItem_Raw_Materials_DetailsFrm_Click` → `Item_Raw_Materials_DetailsFrm`
- **تقرير  قائمة الأصناف المنتجة** — `Name=menuItemItemProcuced`, `Click=menuItem134_Click` → `Item_Raw_MaterialsFor_EntageFrm_`
- **إجمالى الأصناف التى تم إنتاجها** — `Name=menuItemTotalItemProduced`, `Click=menuItem135_Click` → `Item_Raw_MaterialsFor_EntageTotalFrmNew`
- **تقرير خامات الأصناف المنتجة** — `Name=menuItemRawProduced`, `Click=menuItem136_Click` → `Item_Raw_MaterialsFrm`
- **إجمالى  خامات الأصناف التى تم إنتاجها** — `Name=menuItemRawProducedTotal`, `Click=menuItem137_Click` → `Store_LIB.Item_Raw_MaterialTotalFrmNew`
- **تقرير إجمالى المبيعات عن فترة** — `Name=menuItemTotalSellsPeriod`, `Click=menuItem139_Click` → `Accounts.SalesPeriodTotal`
- **تقرير إجمالى المشتريات عن فترة** — `Name=menuItemPurchaseTotalPeriod`, `Click=menuItem140_Click` → `Accounts.PurchasePeriodTotal`
- **تقرير الجرد عن فترة** — `Name=menuItemGardReport`, `Click=menuItem143_Click` → `Accounts.Total_Balance_Period`

##  تقارير  <sub>`menuItemReports`</sub>

- **تقرير إجمالى مبيعات ونقدية** — `Name=menuItemSalesMoneyFrm`, `Click=MenuItemSalesMoneyFrm_Click` → `SalesMasrofat`
- **تقرير تكلفة البضاعة المباعة** — `Name=menuItemSalesCostFrm`, `Click=menuItemSalesCostFrm_Click` → `SalesCostFrm`
- **تقرير تحليل إجمالى مبيعات العملاء** — `Name=menuItemCustomersSaless`, `Click=menuItemCustomersSaless_Click` → `Accounts.Item_Qnty_SalesFRM`
- **تقارير مبيعات أصناف** — `Name=menuItemReportItemSales`, `Click=menuItem26_Click` → `SalesReports`, `PurchaseReports`, `Accounts.InvoicesFrm.invoices.InvoicesReverseTotal`, `PurchaseReportsTotal`, `SalesCostFrm`, `DestructionSales.Suppliers_DestructionFrm`, `Receve_Talabia`, `Edit_DayRegistrationFrmSP`, `Accounts.Work_Out.Receive_Destruction_Work_Out`, `SalesReportsTotal`, `Order_Receipt_ChecksFrm`, `FirstBalance_Checks_List`, `Assets_EhlakFrm`, `Financial_PeriodFrm`, `Financial_Period_Stores_BalanceFRM`, `Money_Transaction_Bank`, `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_Box_Banks`, `Complex_DayRegistrationFrmSP`, `FirstBalanceMezan_FirstTime`, `FirstBalance_Financial_Period_FRM`, `Accounts.DayRegistration.Special_Accounts.Complex_DayRegistration.EditComplex_DayRegistrationFrmSP`, `ProjUI`, `OpenFileDialog`, `ChangePassword`, `Money_SpentCustomer`, `Money_SpentSupplier`, `MoneyBanksFrm`, `BanksFrm`, `Accounts.Requests.Make_Invoices`, `Accounts.Requests.Make_Invoices_Reverse`, `Make_Bill`, `Make_Bill_Reverse`, `Connection4It.Customer_Info`, `Connection4It.Supplier_Info`, `Mitems_List`, `Order_Receipt_CashFrm`, `OrderMoneyPrint`, `New_OrderMoney_SarfNew`, `Stores.balance_Profits`, `Accounts.AboutUS.AboutUSa`, `ProductionsRates.MachineTextileFrm`, `ProductionsRates.Wardiat`, `ProductionsRates.Textile_Production_Rates_New`, `Store_LIB.ItemProductionData_ListFRM`, `ProductionsRates.Textile_ProductionRateSearch`, `Price_Type_ListFRM`, `Prices_List_Edit_Values`, `Supplier_Price`, `Store_LIB.ItemCategoryFRM`, `Accounts.Reports.ItemPriceFRM`, `Cash_Money_ListFromCustomers`, `Cash_Money_ListToSuppliers`, `Accounts.Item_Qnty_SalesFRM`, `ProductionsRates.Textile_EfficiencyWabara`, `ProductionsRates.Maintaining_Item_ListFRM`, `ProductionsRates.Maintaining_OperationsFrm`, `Account_Common_LIB.Money_Transaction.Cash_Money.OrderMoney_Transaction`, `ProductionsRates.EmpProductionRate_`, `ProductionsRates.MachineTatreezFrm`, `ProductionsRates.Matwa_SedaProductionRate`, `ProductionsRates.Matwa_ListFRM`, `ProductionsRates.Matwa_Production_Send`, `ProductionsRates.Form1`, `Reporting_System.ItemProductionData.Items_ProductionDataWabaraFRM`, `Reporting_System.ItemProductionData.Items_ProductionDataLehafFRM`, `Store_LIB.ItemDataProduction_ListFRM`, `Store_LIB.ItemDataPurchase_ListFRM`, `CustomerDataListFRM`, `SupplierDataListFRM`, `ItemMainCategoryFRM`, `ProductionsRates.NewOrderWork_FRM`, `ProductionsRates.OrderWorking.OrderWorkListFRM`, `item`, `ItemsCardsStore`, `ItemsCardsStore_Days`, `Money_SpentReports`, `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_Box`, `Account_Common_LIB.New_Money_Transaction.BanksMoneyTransactionAccounts`, `Reporting_System.Masrofat.Masrofat_RPT_FRM`, `SearchItemPrices_List_Values`, `Items_Types_Name`, `Items_Bodies_Name`, `Items_TypesColor_Name`, `ProductionsRates.Textile_Production_Rates_OrderWabara`, `ProductionsRates.Textile_Production_Rates_OrderLehaf`, `ProductionsRates.AtalatListFrm`, `ProductionsRates.Textile_Efficiency_DetailsWabara`, `ProductionsRates.Textile_Efficiency_DetailsLehaf`, `AccountCommonDataLIB.DBConfig`, `Reporting_System.CustomerRPT.CustomerDataFRm`, `Reporting_System.SuppliersRPT.SuppliersDataListPrintFrm`, `Price_Type_List_PurchasFRM`, `FrmMainBackup`, `BackupFrm`, `Edit_Prices_List_Values`, `TransferItemsWorkNew`, `Order_DetailsWorkNewFrm`, `ItemsCards`, `ItemPurchaseQntyFrm`, `Store_LIB.ItemQntyFrm`, `balance`, `BalanceWith_Sell_Value`, `Item_BalanceFrom`, `Store_LIB.ItemQntyFrmOLD`, `TotalItemBalanceSellValuesFRM`, `Reporting_System.Item_Balance_Purchase.Item_QntyPurchasingFRM_`, `BalanceWith_Purchase_Value`, `ItemsRunning`, `Reporting_System.Estlam_Production.Estlam_ProductionToStoreFRM`, `ItemsHalk`, `Item_Sarf_FromToStoreDetailes_Purshing_QntyFrm`, `Item_Sarf_FromStoreFrm`, `Store_LIB.balance_`, `Stores.balance`, `Store_LIB.Accountss.Item_Balance`, `Accounts.Accountss.Item_Balance`, `NeedeD_balance`, `Needed_balance_Total`, `Recived_order_Push`, `Order_detailes`, `Item_detailes`, `ItemDeptBalance`, `Order_ListRPT`, `Reporting_System.ItemDataPurchase_ListFRM`, `SubItemListRawFrm`, `ItemRawsReviews`, `SubItemRawSarfList_Frm`, `Accounts.Test`, `Order_DetailsWorkEdit`, `Store_LIB.PrintItemsBarCodeSettingFor_Developer`, `AccountModelsLib.operations`, `Item_Raw_MaterialsFor_EntageFrm`, `Item_Raw_Materials_DetailsFrm`, `ChangeCompanyNameFrmcs`, `Add_ALLItemFromTo_Store_`, `Order_DetailsWorkMulti`, `ChangeItemRaw_List`, `Item_AllBalance`, `Invoices_Reverse_List`, `Invoices_ListFrm`, `Bills_List_Reverse`, `Taswya_Money`, `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_Review`, `Connection4It.Customer_SupplierInfo`, `Accounts.Customer_SupplierAccountFrmLv`, `Taswya_MoneyItem`, `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_ReviewItem`, `GroupsListFrm`, `Supplier_AccountNewFrm`, `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_BoxPeriod`, `Customer_BalanceList`, `Supplier_BalanceList`, `Taswya_MoneyMulti_Item`, `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_Review_Multi_Item`, `Money_Transaction_BoxTotalDay`, `Item_Raw_MaterialsFor_EntageFrm_`, `Item_Raw_MaterialsFor_EntageTotalFrmNew`, `Item_Raw_MaterialsFrm`, `Store_LIB.Item_Raw_MaterialTotalFrmNew`, `Customer_AccountFrmLv`, `Customer_AccountMokhtalat`, `Accounts.SalesPeriodTotal`, `Accounts.PurchasePeriodTotal`, `Customer_BalanceMokhtalatList`, `Accounts.Total_Balance_Period`, `Item_Transaction_BalanceL2`, `Export_Checks_List`, `Imports_Checks_ListReport`, `Store_LIB.Item_FirstBalance`, `OrderDetails_List`, `ProjUI_`, `Store_LIB.PrintBarCodeForOneItems`, `Store_LIB.PrintItemsBarCodeSettingFor_DeveloperNew`, `Store_LIB.BarCodeSettingsForm`, `Store_LIB.PrintItemsBarCodeSettingFor_User`, `BalanceWith_Purchase_Price`, `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_BoxFrm`, `SalesTicket_Tours`, `SalesTicket_Tours_List`, `SalesTicket_Tours_List_Reverse`, `Tasherat_Tours`, `Tasherat_Tours_List`, `Ticket_Tours_Land`, `Ticket_Tours_Land_List`, `AddProgramOmra`, `Hotel_Tours`, `Hotel_Tours_List`, `AddOmraProgramList`, `Omra_Tours`, `Omra_Tours_List`, `Accounts.Work_In.Add_Requests_Work_In`, `Work_In.Receive_Requests_Work_In`, `Accounts.Work_In.Receive_Destruction_Work_In`, `Accounts.Work_In.Work_In_Reports_Receive`, `Accounts.Work_In.Work_In_Reports_SendANDBalance`, `AddBus`, `AddBusList`, `Bus_Tours`, `Bus_Tours_List`, `SalesMasrofat`, `Mitems_ListNew`, `Connection4It.Mitems_List_NoBalance`, `Connection4It.NewItemCodeList`, `Connection4It.Mitems_ListTax`, `Mitems_ListNewNoPrice`, `Mitems_List_WH`, `Takseet_List`, `Store_LIB.balanceOnlyNoSpec`, `Store_LIB.balanceOnly`, `Store_LIB.balance_Price`, `Store_LIB.ItemNoQntyPrice`, `Store_LIB.Accountss.Item_Balance_Date`, `FrmStaticEditSysConfig`, `BalanceWithPurchasePric`, `BalanceWithPurchasePric_SUM`, `Static_NotesInInvoice`, `balanceWithDate`, `Accounts.Accountss.Item_Balance_Date`, `balanceSelect_Date`, `CustomersLinkToAccountsFrm`, `ItemCategoryFRM`, `UnitListFrm`, `Account_Common_LIB.FixesAsset_TreeFrm`, `Assets_EhlakFrm_`, `CurrencyListFrm`, `CurrencyListFrmNew`, `Account_Common_LIB.Currency_Factors`, `Journal_DaysListFrm`, `CostCenterForAccounts_FRM`, `Account_Common_LIB.Cost_CenterTree.Cost_Center_TreeFrm`, `NewAccount_EntryFRM`, `NewAccount_EntryMadinDa2nFRM`, `Review_Account_Entry`, `Make_Account_Entry_Automatic`, `Tar7eel_Account_Entry`, `Account_Common_LIB.Submain_AccountPrintFrm`, `Accounts.General_LedgerAccount.Master_Account_DetailsTarseed22`, `Master_Account_DetailsTarseed2`, `Master_Account_Details2`, `Mezan_Morgaa_ByPeriod`, `Mezan_Morgaa_ByPeriodByAccount`, `Mezan_Morgaa_ByMagamehFrm_RPT`, `Mezan_Morgaa_ByMagameh_ArsedaFrm_RPT`, `Master_Account_CostCenter_TarsesdFRM`, `Incoming_StatementByDate`, `Profit_Loss_StatFRMNew`, `Get_Account_BalanceFRM2`, `Master_Account_CostCenter_FRM`, `ADDNEWUSER`, `UsersList`, `Branch_UserPermission`, `frmadmin`, `Money_Between_BoxTar7eelFrm`, `Order_Cash`, `Account_Common_LIB.New_Money_Transaction.BanksMoneyTransaction`, `Cash_Money_List`, `TagerKontraCLSListFrm`, `Mitems_List_NoBalance`, `Items_TypesDescription`, `Items_TypesDescriptionNameOther`, `Items_TypesFactory_Name`, `Items_TypesSize_Name`, `TypesDegreeFarz_Name`, `InvoicesFrm.invoices.Invoices_List_Payment_`, `balance_CostAvgLastBalance`, `balance_CostAvgbyLastDate`, `EmployeeLisFrm`, `ShipmentCompanyLisFrm`, `NewSimpleAccount_EntryMadinDa2nFRM`, `ChecksWithDueDate`, `ItemBalanceWithExpireFrm`, `Store_LIB.balanceOnlyEdit`, `ItemsListSeramek`, `Mitems_List_Sermek`, `ItemsListAdwia`, `Needed_balance_phGrid`, `Item_balance_ph`, `CountryLisFrm`, `Govern_AreaListFrm`, `Govern_ListFrm`, `Gov_City_ListFrm`, `AddCarFrm`, `CarsListFrm`, `CarsSystemLib.AddCarFrm`, `Items_NatureFrm`, `Items_GroupFrm`, `WorkOrderListFrm`, `CarsSystemLib.AddWorkOrder`, `ItemsListCars`, `AddEstimationsValueEditNameFrm`, `AddEstimationsValueFrm`, `EstimationValuesListFrm`, `CarEntryTaskeenFrm`, `CashPaymentMethodFrm`, `CarWorkOrdersList`, `StatusWorkOrderListFrm`, `Reporting_System.SupplierDataFRM_Update`, `Store_LIB.balanceOnlyEditPrices`, `CarsSystemLib.EmployessItemsPriceFrm`, `CarsSystemLib.EmployeeRelatedItemFrm`, `CarsSystemLib.CustomerCarsListFrm`, `BillsList_Edit_SPriceFrm`, `Mitems_List_CarNew`, `CarsChangeOwnerFrm`, `Customer_AccountMokhtalat_EF`, `Customer_AccountMokhtalatEF_Period`, `CarsChangeBranchesOwnerFrm`, `Cash_Money_ListFromAccounts`, `New_OrderMoney_Transaction`, `New_OrderMoney_SarfNewNew`, `OrderMoney_TransactionManyForAccounts`, `Invoices_ListFrmCar`, `CarWorkOrderForItemsFrm`, `ItemsListCarsWithItems`, `Item_Raw_MaterialTotalFrm`, `CanceledWorkOrderListFrm`, `OpenedWorkOrderListFrm`, `OpenedEndWorkDateWorkOrderListFrm`, `balance_Employee`, `Item_Sarf_FromStoreDetailes_Purshing_Import_ExportFrm`, `Store_LIB.BalanceWithCostFrm`, `ItemsListGridFrm`, `ItemsListViewFrm`, `Item_Transaction_Details`, `Item_Transaction_DetailsWithCost`, `Machin_listFrm`, `Item_Sarf_FromToStoreDetailes_Purshing_ImportExportFrm`, `CarsTypeListFrm`, `CarsTypeItemsFrm`, `CarWorkOrderItem_Sarf_FromStoreFrm`, `AddExamination`, `ExaminationsListPaymentFrm`, `Invoices_ListCountFrmCar`, `Employess_All_ItemsPriceFrm`, `Employess_Total_ItemsPrice`, `Employess_Review_ItemsPriceFrm`, `Invoices_ListTaxesFrm`, `Purchase_ListTaxes`, `ItemsListCars_WithCategory_Browse`, `Store_LIB.balance_SellBuyPriceTotal`, `Store_LIB.balanceOnlyEdit_Units`, `FrmBatchReceiptMultiple`, `FrmBatchReceiptList`, `FrmDailyItemSales`, `FrmSales_CustomerList`, `FrmSales_ItemsList`, `FrmDailyCustomerSales`, `FrmBatchItemsReceiptList`, `FrmDailyCustomerSalesSingle`, `FrmDailyCustomerSalesListReport`, `FrmServicesList`, `FrmServiceCategoryList`, `EmployeeRelatedItemServicesFrm`, `FrmAppointment`, `CalendarAppointmentsFrm`, `CalendarAppointmentsForm`, `FrmInvoiceCreate`, `FrmSessions`, `FrmDaily_All_ItemSales`, `FrmPackagesListMulti`, `FrmCustomerPackagesList`, `DashboardForm`, `FrmAddPackageSession2`, `Invoices_List_ReceiptDistribution`, `Invoices_List_ReceiptDistributionReport`, `FrmWorkOrderEdit`, `FrmWorkOrderList`, `Customer_AccountMokhtalat_Period`, `Supplier_AccountNewFrmPeriod`, `ConstructionLib.MainAccountSettingListFrm`, `CustomerDataListFRMSubAccount`, `AccountsCustomerLinkFrm`, `Mitems_List_PH_R`, `ItemsListAdwiaRetail`, `Store_LIB.PrintBarCodeForOneItemsSaidalia`, `PrinterSelectionFrm`, `Store_LIB.PrintBarCodeForOneItemsWithPrice`, `Store_LIB.Bill_PrintItemsBarCodeItemsList`, `BranchesListFrm`, `Account_Common_LIB.Banks.SandoukBranch`
- **تقارير مرتجع مبيعات أصناف** — `Name=menuItemReportItemSalesReverse`, `Click=menuItem27_Click` → `SalesReports`, `PurchaseReports`, `Accounts.InvoicesFrm.invoices.InvoicesReverseTotal`, `PurchaseReportsTotal`, `SalesCostFrm`, `DestructionSales.Suppliers_DestructionFrm`, `Receve_Talabia`, `Edit_DayRegistrationFrmSP`, `Accounts.Work_Out.Receive_Destruction_Work_Out`, `SalesReportsTotal`, `Order_Receipt_ChecksFrm`, `FirstBalance_Checks_List`, `Assets_EhlakFrm`, `Financial_PeriodFrm`, `Financial_Period_Stores_BalanceFRM`, `Money_Transaction_Bank`, `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_Box_Banks`, `Complex_DayRegistrationFrmSP`, `FirstBalanceMezan_FirstTime`, `FirstBalance_Financial_Period_FRM`, `Accounts.DayRegistration.Special_Accounts.Complex_DayRegistration.EditComplex_DayRegistrationFrmSP`, `ProjUI`, `OpenFileDialog`, `ChangePassword`, `Money_SpentCustomer`, `Money_SpentSupplier`, `MoneyBanksFrm`, `BanksFrm`, `Accounts.Requests.Make_Invoices`, `Accounts.Requests.Make_Invoices_Reverse`, `Make_Bill`, `Make_Bill_Reverse`, `Connection4It.Customer_Info`, `Connection4It.Supplier_Info`, `Mitems_List`, `Order_Receipt_CashFrm`, `OrderMoneyPrint`, `New_OrderMoney_SarfNew`, `Stores.balance_Profits`, `Accounts.AboutUS.AboutUSa`, `ProductionsRates.MachineTextileFrm`, `ProductionsRates.Wardiat`, `ProductionsRates.Textile_Production_Rates_New`, `Store_LIB.ItemProductionData_ListFRM`, `ProductionsRates.Textile_ProductionRateSearch`, `Price_Type_ListFRM`, `Prices_List_Edit_Values`, `Supplier_Price`, `Store_LIB.ItemCategoryFRM`, `Accounts.Reports.ItemPriceFRM`, `Cash_Money_ListFromCustomers`, `Cash_Money_ListToSuppliers`, `Accounts.Item_Qnty_SalesFRM`, `ProductionsRates.Textile_EfficiencyWabara`, `ProductionsRates.Maintaining_Item_ListFRM`, `ProductionsRates.Maintaining_OperationsFrm`, `Account_Common_LIB.Money_Transaction.Cash_Money.OrderMoney_Transaction`, `ProductionsRates.EmpProductionRate_`, `ProductionsRates.MachineTatreezFrm`, `ProductionsRates.Matwa_SedaProductionRate`, `ProductionsRates.Matwa_ListFRM`, `ProductionsRates.Matwa_Production_Send`, `ProductionsRates.Form1`, `Reporting_System.ItemProductionData.Items_ProductionDataWabaraFRM`, `Reporting_System.ItemProductionData.Items_ProductionDataLehafFRM`, `Store_LIB.ItemDataProduction_ListFRM`, `Store_LIB.ItemDataPurchase_ListFRM`, `CustomerDataListFRM`, `SupplierDataListFRM`, `ItemMainCategoryFRM`, `ProductionsRates.NewOrderWork_FRM`, `ProductionsRates.OrderWorking.OrderWorkListFRM`, `item`, `ItemsCardsStore`, `ItemsCardsStore_Days`, `Money_SpentReports`, `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_Box`, `Account_Common_LIB.New_Money_Transaction.BanksMoneyTransactionAccounts`, `Reporting_System.Masrofat.Masrofat_RPT_FRM`, `SearchItemPrices_List_Values`, `Items_Types_Name`, `Items_Bodies_Name`, `Items_TypesColor_Name`, `ProductionsRates.Textile_Production_Rates_OrderWabara`, `ProductionsRates.Textile_Production_Rates_OrderLehaf`, `ProductionsRates.AtalatListFrm`, `ProductionsRates.Textile_Efficiency_DetailsWabara`, `ProductionsRates.Textile_Efficiency_DetailsLehaf`, `AccountCommonDataLIB.DBConfig`, `Reporting_System.CustomerRPT.CustomerDataFRm`, `Reporting_System.SuppliersRPT.SuppliersDataListPrintFrm`, `Price_Type_List_PurchasFRM`, `FrmMainBackup`, `BackupFrm`, `Edit_Prices_List_Values`, `TransferItemsWorkNew`, `Order_DetailsWorkNewFrm`, `ItemsCards`, `ItemPurchaseQntyFrm`, `Store_LIB.ItemQntyFrm`, `balance`, `BalanceWith_Sell_Value`, `Item_BalanceFrom`, `Store_LIB.ItemQntyFrmOLD`, `TotalItemBalanceSellValuesFRM`, `Reporting_System.Item_Balance_Purchase.Item_QntyPurchasingFRM_`, `BalanceWith_Purchase_Value`, `ItemsRunning`, `Reporting_System.Estlam_Production.Estlam_ProductionToStoreFRM`, `ItemsHalk`, `Item_Sarf_FromToStoreDetailes_Purshing_QntyFrm`, `Item_Sarf_FromStoreFrm`, `Store_LIB.balance_`, `Stores.balance`, `Store_LIB.Accountss.Item_Balance`, `Accounts.Accountss.Item_Balance`, `NeedeD_balance`, `Needed_balance_Total`, `Recived_order_Push`, `Order_detailes`, `Item_detailes`, `ItemDeptBalance`, `Order_ListRPT`, `Reporting_System.ItemDataPurchase_ListFRM`, `SubItemListRawFrm`, `ItemRawsReviews`, `SubItemRawSarfList_Frm`, `Accounts.Test`, `Order_DetailsWorkEdit`, `Store_LIB.PrintItemsBarCodeSettingFor_Developer`, `AccountModelsLib.operations`, `Item_Raw_MaterialsFor_EntageFrm`, `Item_Raw_Materials_DetailsFrm`, `ChangeCompanyNameFrmcs`, `Add_ALLItemFromTo_Store_`, `Order_DetailsWorkMulti`, `ChangeItemRaw_List`, `Item_AllBalance`, `Invoices_Reverse_List`, `Invoices_ListFrm`, `Bills_List_Reverse`, `Taswya_Money`, `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_Review`, `Connection4It.Customer_SupplierInfo`, `Accounts.Customer_SupplierAccountFrmLv`, `Taswya_MoneyItem`, `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_ReviewItem`, `GroupsListFrm`, `Supplier_AccountNewFrm`, `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_BoxPeriod`, `Customer_BalanceList`, `Supplier_BalanceList`, `Taswya_MoneyMulti_Item`, `Account_Common_LIB.Money_Financial_Notice.Taswya_Money_Review_Multi_Item`, `Money_Transaction_BoxTotalDay`, `Item_Raw_MaterialsFor_EntageFrm_`, `Item_Raw_MaterialsFor_EntageTotalFrmNew`, `Item_Raw_MaterialsFrm`, `Store_LIB.Item_Raw_MaterialTotalFrmNew`, `Customer_AccountFrmLv`, `Customer_AccountMokhtalat`, `Accounts.SalesPeriodTotal`, `Accounts.PurchasePeriodTotal`, `Customer_BalanceMokhtalatList`, `Accounts.Total_Balance_Period`, `Item_Transaction_BalanceL2`, `Export_Checks_List`, `Imports_Checks_ListReport`, `Store_LIB.Item_FirstBalance`, `OrderDetails_List`, `ProjUI_`, `Store_LIB.PrintBarCodeForOneItems`, `Store_LIB.PrintItemsBarCodeSettingFor_DeveloperNew`, `Store_LIB.BarCodeSettingsForm`, `Store_LIB.PrintItemsBarCodeSettingFor_User`, `BalanceWith_Purchase_Price`, `Account_Common_LIB.Money_Transaction.Cash_Money.Money_Transaction_BoxFrm`, `SalesTicket_Tours`, `SalesTicket_Tours_List`, `SalesTicket_Tours_List_Reverse`, `Tasherat_Tours`, `Tasherat_Tours_List`, `Ticket_Tours_Land`, `Ticket_Tours_Land_List`, `AddProgramOmra`, `Hotel_Tours`, `Hotel_Tours_List`, `AddOmraProgramList`, `Omra_Tours`, `Omra_Tours_List`, `Accounts.Work_In.Add_Requests_Work_In`, `Work_In.Receive_Requests_Work_In`, `Accounts.Work_In.Receive_Destruction_Work_In`, `Accounts.Work_In.Work_In_Reports_Receive`, `Accounts.Work_In.Work_In_Reports_SendANDBalance`, `AddBus`, `AddBusList`, `Bus_Tours`, `Bus_Tours_List`, `SalesMasrofat`, `Mitems_ListNew`, `Connection4It.Mitems_List_NoBalance`, `Connection4It.NewItemCodeList`, `Connection4It.Mitems_ListTax`, `Mitems_ListNewNoPrice`, `Mitems_List_WH`, `Takseet_List`, `Store_LIB.balanceOnlyNoSpec`, `Store_LIB.balanceOnly`, `Store_LIB.balance_Price`, `Store_LIB.ItemNoQntyPrice`, `Store_LIB.Accountss.Item_Balance_Date`, `FrmStaticEditSysConfig`, `BalanceWithPurchasePric`, `BalanceWithPurchasePric_SUM`, `Static_NotesInInvoice`, `balanceWithDate`, `Accounts.Accountss.Item_Balance_Date`, `balanceSelect_Date`, `CustomersLinkToAccountsFrm`, `ItemCategoryFRM`, `UnitListFrm`, `Account_Common_LIB.FixesAsset_TreeFrm`, `Assets_EhlakFrm_`, `CurrencyListFrm`, `CurrencyListFrmNew`, `Account_Common_LIB.Currency_Factors`, `Journal_DaysListFrm`, `CostCenterForAccounts_FRM`, `Account_Common_LIB.Cost_CenterTree.Cost_Center_TreeFrm`, `NewAccount_EntryFRM`, `NewAccount_EntryMadinDa2nFRM`, `Review_Account_Entry`, `Make_Account_Entry_Automatic`, `Tar7eel_Account_Entry`, `Account_Common_LIB.Submain_AccountPrintFrm`, `Accounts.General_LedgerAccount.Master_Account_DetailsTarseed22`, `Master_Account_DetailsTarseed2`, `Master_Account_Details2`, `Mezan_Morgaa_ByPeriod`, `Mezan_Morgaa_ByPeriodByAccount`, `Mezan_Morgaa_ByMagamehFrm_RPT`, `Mezan_Morgaa_ByMagameh_ArsedaFrm_RPT`, `Master_Account_CostCenter_TarsesdFRM`, `Incoming_StatementByDate`, `Profit_Loss_StatFRMNew`, `Get_Account_BalanceFRM2`, `Master_Account_CostCenter_FRM`, `ADDNEWUSER`, `UsersList`, `Branch_UserPermission`, `frmadmin`, `Money_Between_BoxTar7eelFrm`, `Order_Cash`, `Account_Common_LIB.New_Money_Transaction.BanksMoneyTransaction`, `Cash_Money_List`, `TagerKontraCLSListFrm`, `Mitems_List_NoBalance`, `Items_TypesDescription`, `Items_TypesDescriptionNameOther`, `Items_TypesFactory_Name`, `Items_TypesSize_Name`, `TypesDegreeFarz_Name`, `InvoicesFrm.invoices.Invoices_List_Payment_`, `balance_CostAvgLastBalance`, `balance_CostAvgbyLastDate`, `EmployeeLisFrm`, `ShipmentCompanyLisFrm`, `NewSimpleAccount_EntryMadinDa2nFRM`, `ChecksWithDueDate`, `ItemBalanceWithExpireFrm`, `Store_LIB.balanceOnlyEdit`, `ItemsListSeramek`, `Mitems_List_Sermek`, `ItemsListAdwia`, `Needed_balance_phGrid`, `Item_balance_ph`, `CountryLisFrm`, `Govern_AreaListFrm`, `Govern_ListFrm`, `Gov_City_ListFrm`, `AddCarFrm`, `CarsListFrm`, `CarsSystemLib.AddCarFrm`, `Items_NatureFrm`, `Items_GroupFrm`, `WorkOrderListFrm`, `CarsSystemLib.AddWorkOrder`, `ItemsListCars`, `AddEstimationsValueEditNameFrm`, `AddEstimationsValueFrm`, `EstimationValuesListFrm`, `CarEntryTaskeenFrm`, `CashPaymentMethodFrm`, `CarWorkOrdersList`, `StatusWorkOrderListFrm`, `Reporting_System.SupplierDataFRM_Update`, `Store_LIB.balanceOnlyEditPrices`, `CarsSystemLib.EmployessItemsPriceFrm`, `CarsSystemLib.EmployeeRelatedItemFrm`, `CarsSystemLib.CustomerCarsListFrm`, `BillsList_Edit_SPriceFrm`, `Mitems_List_CarNew`, `CarsChangeOwnerFrm`, `Customer_AccountMokhtalat_EF`, `Customer_AccountMokhtalatEF_Period`, `CarsChangeBranchesOwnerFrm`, `Cash_Money_ListFromAccounts`, `New_OrderMoney_Transaction`, `New_OrderMoney_SarfNewNew`, `OrderMoney_TransactionManyForAccounts`, `Invoices_ListFrmCar`, `CarWorkOrderForItemsFrm`, `ItemsListCarsWithItems`, `Item_Raw_MaterialTotalFrm`, `CanceledWorkOrderListFrm`, `OpenedWorkOrderListFrm`, `OpenedEndWorkDateWorkOrderListFrm`, `balance_Employee`, `Item_Sarf_FromStoreDetailes_Purshing_Import_ExportFrm`, `Store_LIB.BalanceWithCostFrm`, `ItemsListGridFrm`, `ItemsListViewFrm`, `Item_Transaction_Details`, `Item_Transaction_DetailsWithCost`, `Machin_listFrm`, `Item_Sarf_FromToStoreDetailes_Purshing_ImportExportFrm`, `CarsTypeListFrm`, `CarsTypeItemsFrm`, `CarWorkOrderItem_Sarf_FromStoreFrm`, `AddExamination`, `ExaminationsListPaymentFrm`, `Invoices_ListCountFrmCar`, `Employess_All_ItemsPriceFrm`, `Employess_Total_ItemsPrice`, `Employess_Review_ItemsPriceFrm`, `Invoices_ListTaxesFrm`, `Purchase_ListTaxes`, `ItemsListCars_WithCategory_Browse`, `Store_LIB.balance_SellBuyPriceTotal`, `Store_LIB.balanceOnlyEdit_Units`, `FrmBatchReceiptMultiple`, `FrmBatchReceiptList`, `FrmDailyItemSales`, `FrmSales_CustomerList`, `FrmSales_ItemsList`, `FrmDailyCustomerSales`, `FrmBatchItemsReceiptList`, `FrmDailyCustomerSalesSingle`, `FrmDailyCustomerSalesListReport`, `FrmServicesList`, `FrmServiceCategoryList`, `EmployeeRelatedItemServicesFrm`, `FrmAppointment`, `CalendarAppointmentsFrm`, `CalendarAppointmentsForm`, `FrmInvoiceCreate`, `FrmSessions`, `FrmDaily_All_ItemSales`, `FrmPackagesListMulti`, `FrmCustomerPackagesList`, `DashboardForm`, `FrmAddPackageSession2`, `Invoices_List_ReceiptDistribution`, `Invoices_List_ReceiptDistributionReport`, `FrmWorkOrderEdit`, `FrmWorkOrderList`, `Customer_AccountMokhtalat_Period`, `Supplier_AccountNewFrmPeriod`, `ConstructionLib.MainAccountSettingListFrm`, `CustomerDataListFRMSubAccount`, `AccountsCustomerLinkFrm`, `Mitems_List_PH_R`, `ItemsListAdwiaRetail`, `Store_LIB.PrintBarCodeForOneItemsSaidalia`, `PrinterSelectionFrm`, `Store_LIB.PrintBarCodeForOneItemsWithPrice`, `Store_LIB.Bill_PrintItemsBarCodeItemsList`, `BranchesListFrm`, `Account_Common_LIB.Banks.SandoukBranch`
- **تقارير المبيعات** — `Name=menuItemSalesTotal`, `Click=menuItemSalesTotal_Click` → `SalesReportsTotal`
- **تقارير مرتجع المبيعات** — `Name=menuItemSalesTR`, `Click=menuItemSalesTR_Click` → `SalesReportsTotal`
- **تقارير مشتريات أصناف** — `Name=menuPurchaseReport`, `Click=menuPurchaseReport_Click` → *(no `new Form(...)` found in handler — see note)*
- **تقارير مردود مشتريات أصناف** — `Name=menuItemPurchaseReportReverse`, `Click=menuItem29_Click` → `PurchaseReports`
- **تقارير إجمالى  مردود المشتريات** — `Name=menuItemPurchaseReportReverseTotal`, `Click=menuItem24_Click_1` → `PurchaseReportsTotal`
- **تقارير إجمالى مشتريات** — `Name=menuPurchaseReportTotal`, `Click=menuItem7_Click_1` → `PurchaseReportsTotal`
- **تقرير ضرايب فواتير المبيعات** — `Name=toolStripMenuItemInvoices_ListTaxes`, `Click=toolStripMenuItemInvoices_ListTaxes_Click` → `Invoices_ListTaxesFrm`
- **تقرير ضرايب فواتير المشتريات** — `Name=toolStripMenuItemBills_ListTaxes`, `Click=toolStripMenuItemBills_ListTaxes_Click` → `Purchase_ListTaxes`, `Invoices_ListFrm`
- **تقارير الإيرادات و  المصروفات** — `Name=menuItemEradatMasrofatReport`, `Click=menuItem151_Click` → `Money_SpentReports`

### تقارير المصروفات  <sub>`menuItemMasrofatReport`</sub>

- **المصروفات** — `Name=menuItemMasroufat`, `Click=menuItem28_Click` → *(no `new Form(...)` found in handler — see note)*
- **المصروفات  يومية أمريكية** — `Name=menuItemMasrofateUSA`, `Click=menuItemMasrofateUSA_Click` → `Reporting_System.Masrofat.Masrofat_RPT_FRM`

- **أرباح البيع** — `Name=menuItemProfitSales`, `Click=menuItemProfitSales_Click` → `Stores.balance_Profits`
- **تقارير تقييم مبيعات الأصناف** — `Name=MenuBalanceWithPurchasePric`, `Click=MenuBalanceWithPurchasePric_Click` → `BalanceWithPurchasePric`
- **أرصدة الأصناف** — `Name=menuItemItemBalanceOnly`, `Click=MenuItemItemBalanceOnly_Click` → `Store_LIB.balanceOnlyNoSpec`
- **أرصدة الأصناف - بحث بالمواصفات** — `Name=menuItemItemBalanceOnlySpec`, `Click=menuItemItemBalanceOnlySpec_Click` → `Store_LIB.balanceOnly`
- **أرصدة الأصناف كمية ومتوسط تكلفة** — `Name=menuItemItemBalanceWithCost`, `Click=menuItemItemBalanceWithCost_Click` → *(no `new Form(...)` found in handler — see note)*
- **أرصدة الأصناف كمية وقيمة بأخر سعر شراء وبيع** — `Name=menuItemItemBalanceSellBuyPriceTotal`, `Click=menuItemItemBalanceSellBuyPriceTotal_Click` → `Store_LIB.balance_SellBuyPriceTotal`
- **أرصدة الأصناف كمية وقيمة بأخر سعر** — `Name=menuItemItemBalance`, `Click=menuItemItemBalance_Click` → `Store_LIB.balance_`
- **إجمالى أرصدة الأصناف فى كل المخازن** — `Name=menuItemItemBalanceTotal_`, `Click=menuItemItemBalanceTotal__Click` → `Item_AllBalance`

### أرصدة الأصناف بتورايخ سابقة  <sub>`toolStripMenuItemItem_Balance_Date`</sub>

- **أرصدة الأصناف بتورايخ سابقة** — `Name=aToolStripMenuItem`, `Click=ToolStripMenuItemItem_Balance_Date_Click` → `balanceWithDate`, `Accounts.Accountss.Item_Balance_Date`
- **حفظ أرصدة الأصناف بالتاريخ والوقت الحالى** — `Name=ToolStripMenuItem_Save_ItemBalanceNow`, `Click=ToolStripMenuItem_Save_ItemBalanceNow_Click` → `balanceSelect_Date`

- **تقييم الأصناف بالمخزن بمتوسط سعر شراء اخر كمية** — `Name=toolStripMenuItembalance_CostAvg`, `Click=toolStripMenuItembalance_CostAvg_Click` → `balance_CostAvgLastBalance`
- **تقييم الأصناف بالمخزن بمتوسط شراء فترة** — `Name=toolStripMenuItembalance_CostAvgPurchasePeriod`, `Click=toolStripMenuItembalance_CostAvgPurchasePeriod_Click` → `balance_CostAvgbyLastDate`
- **تقرير سعر شراء وحدة الصنف** — `Name=toolStripMenuItemPurchasItemNo`, `Click=ToolStripMenuItemPurchasItemNo_Click` → `Store_LIB.balance_Price`, `Store_LIB.ItemNoQntyPrice`
- **مدفوعات من العملاء** — `Name=menuItemCustometMoney`, `Click=menuItemCustometMoney_Click` → `Money_SpentCustomer`
- **مدفوعات إلى الموردين** — `Name=menuItemSupplierMoney`, `Click=menuItemSupplierMoney_Click` → `Money_SpentSupplier`

##  تقارير المخزن  <sub>`menuItemReportStore`</sub>

- **بحث أذونات المخزن** — `Name=menuItemOrdersList`, `Click=menuItem148_Click` → `Order_detailes`

###  بحث أذونات الصرف - حركات المخازن  <sub>`menuItemOrderReport`</sub>

- **بحث    إذن صرف لمخزن أو قسم** — `Name=menuItem99`, `Click=menuItem99_Click` → `Order_detailes`
- **بحث   أذونات الاستلام من المورد** — `Name=menuItem101`, `Click=menuItem101_Click` → `Order_detailes`
- **بحث أذونات مرتجع إلى مورد** — `Name=menuItem102`, `Click=menuItem102_Click` → `Order_detailes`
- **بحث أذونات صرف لعميل** — `Name=menuItem118`, `Click=menuItem118_Click` → `Order_detailes`
- **بحث أذونات مرتجع  من العميل** — `Name=menuItem104`, `Click=menuItem104_Click` → `Order_detailes`
- **بحث أذونات تسوية بالزيادة** — `Name=menuItem122`, `Click=menuItem122_Click` → `Order_detailes`
- **بحث أذونات تسوية بالنقص** — `Name=menuItem123`, `Click=menuItem123_Click` → `Order_detailes`
- **بحث أذونات إستلام من الإنتاج** — `Name=menuItem124`, `Click=menuItem124_Click` → `Order_detailes`
- **بحث  أذونات صرف إلى الإنتاج** — `Name=menuItem125`, `Click=menuItem125_Click` → `Order_detailes`
- **بحث  أذونات صرف قطع غيار** — `Name=menuItem126`, `Click=menuItem126_Click` → `Order_detailes`
- **بحث  أذونات صرف قطع الغيار للماكينات وخطوط الإنتاج** — `Name=SarfMachineToolStripMenuItem`, `Click=SarfMachineToolStripMenuItem_Click` → `Order_detailes`
- **بحث أذونات مرتجع قطع غيار الماكينات وخطوط الإنتاج** — `Name=EstlamMachineToolStripMenuItem`, `Click=EstlamMachineToolStripMenuItem_Click` → `Order_detailes`
- **بحث أذونات إستلام من موظف** — `Name=EstlamFromEmployeeToolStripMenuItem`, `Click=EstlamFromEmployeeToolStripMenuItem_Click` → `Order_detailes`
- **بحث أذونات صرف إلى موظف** — `Name=SarfToFromEmployeeToolStripMenuItem` → *(no click handler)*

- **تقرير الأصناف المنصرفة من المخزن - كمية** — `Name=Item_Sarf_FromStoreDetailes_Purshing_Export`, `Click=Item_Sarf_FromStoreDetailes_Purshing_Export_Click` → `Item_Sarf_FromStoreDetailes_Purshing_Import_ExportFrm`, `Item_Sarf_FromStoreFrm`
- **تقرير الأصناف المنصرفة من المخزن - كمية وقيمة** — `Name=Item_Sarf_FromStoreDetailes_Purshing_ExportWithValues`, `Click=Item_Sarf_FromStoreDetailes_Purshing_ExportWithValues_Click` → `Item_Sarf_FromStoreDetailes_Purshing_Import_ExportFrm`, `Item_Sarf_FromStoreFrm`
- **تقرير الأصناف الواردة إلى المخزن - كمية** — `Name=Item_Sarf_FromStoreDetailes_Purshing_Import`, `Click=Item_Sarf_FromStoreDetailes_PurshingImport_Click` → `Item_Sarf_FromStoreDetailes_Purshing_Import_ExportFrm`, `Item_Sarf_FromStoreFrm`
- **تقرير الأصناف الواردة إلى المخزن - كمية وقيمة** — `Name=Item_Sarf_FromStoreDetailes_Purshing_ImportWithValues`, `Click=Item_Sarf_FromStoreDetailes_Purshing_ImportWithValues_Click` → `Item_Sarf_FromStoreDetailes_Purshing_Import_ExportFrm`, `Item_Sarf_FromStoreFrm`
- **تقرير مراقبة حركة الأصناف بين الجهات** — `Name=ToolStripMenuItemItem_Sarf_FromToStoreDetailes_Purshing_Import_Export`, `Click=ToolStripMenuItemItem_Sarf_FromToStoreDetailes_Purshing_Import_Export_Click` → `Item_Sarf_FromToStoreDetailes_Purshing_QntyFrm`, `Item_Sarf_FromStoreFrm`
- **تقرير صرف قطع الغيار للماكينات** — `Name=toolStripMenuItemSarfMachineReport`, `Click=toolStripMenuItemSarfMachineReport_Click` → `Item_Sarf_FromToStoreDetailes_Purshing_QntyFrm`, `Item_Sarf_FromStoreFrm`
- **تقرير مرتجع قطع الغيار للماكينات** — `Name=toolStripMenuItemEstlamMachineReport`, `Click=toolStripMenuItemEstlamMachineReport_Click` → `Item_Sarf_FromToStoreDetailes_Purshing_QntyFrm`, `Item_Sarf_FromStoreFrm`
- **تقرير تفصيلي لحركة صرف وارتجاع قطع الغيار للماكينات** — `Name=toolStripMenuItemDetailsSarfEstlamMachineReport`, `Click=toolStripMenuItemDetailsSarfEstlamMachineReport_Click` → `Item_Sarf_FromToStoreDetailes_Purshing_ImportExportFrm`, `Item_Sarf_FromStoreFrm`
- **أصناف فى قائمة الهالك** — `Name=menuItemSarfToHalek`, `Click=menuItemSarfToHalek_Click` → `ItemsRunning`, `ItemsHalk`, `Item_Sarf_FromToStoreDetailes_Purshing_QntyFrm`, `Item_Sarf_FromStoreFrm`
- **تقرير الإستلام من الموردين** — `Name=menuItemEstlamFromSupplier`, `Click=menuItemEstlamFromSupplier_Click` → `Item_Sarf_FromToStoreDetailes_Purshing_QntyFrm`, `Item_Sarf_FromStoreFrm`
- **أصناف تم تسويتها بالزيادة** — `Name=menuItemTasweatZeada`, `Click=menuItemTasweatZeada_Click` → `ItemsRunning`
- **إصناف تم تسويتها بالنقص** — `Name=menuItemTasweatNakis`, `Click=menuItem83_Click` → `ItemsRunning`
- **أرصدة الأصناف بسعر المورد** — `Name=menuItemBalanceSupplierPrice`, `Click=menuItem63_Click` → `ItemPurchaseQntyFrm`
- **أرصدة الأصناف بسعر البيع** — `Name=menuItemBalanceSellPrice`, `Click=menuItem64_Click` → `Store_LIB.ItemQntyFrm`
- **أرصدة الأصناف فى المخازن** — `Name=menuItemBalance`, `Click=menuItemBalance_Click` → `balance`

### أرصدة أصناف المبيعات  <sub>`menuItemSellsBalance`</sub>

- **تقييم إرصدة الأصناف لسعر البيع** — `Name=menuItem67`, `Click=menuItem67_Click` → `BalanceWith_Sell_Value`
- **إجمالى أرصدة فئات الأصناف** — `Name=menuItem68`, `Click=ItemBalanceCategory_Click` → `Item_BalanceFrom`
- **تقييم أجمالى أرصدة أصناف المبيعات** — `Name=menuItem69`, `Click=menuItem69_Click` → `Store_LIB.ItemQntyFrmOLD`
- **رصيد أصناف  مبيعات كمية وقيمة** — `Name=menuItem70`, `Click=menuItem70_Click` → `TotalItemBalanceSellValuesFRM`

### أرصدة أصناف مشتريات  <sub>`menuItemPurchaseBalance`</sub>

- **أرصدة أصناف مشتريات كمية وقيمة** — `Name=menuItem72`, `Click=menuItem72_Click` → `Reporting_System.Item_Balance_Purchase.Item_QntyPurchasingFRM_`
- **تقييم إرصدة الأصناف لسعر الشراء** — `Name=menuItem73`, `Click=menuItem73_Click` → `BalanceWith_Purchase_Value`
- **تقييم أرصدة الأصناف بنوع السعر** — `Name=menuItemPriceTypeSupplier`, `Click=menuItemPriceTypeSupplier_Click` → `BalanceWith_Purchase_Price`
- **تقييم أجمالى أرصدة أصناف المشتريات** — `Name=menuItem74`, `Click=menuItem74_Click` → `BalanceWith_Purchase_Value`

### منتجات مستلمة من الإنتاج  <sub>`menuItemMostalemEntag`</sub>

- **تقرير منتجات مستلمة من الإنتاج** — `Name=menuItem76`, `Click=menuItem76_Click` → `ItemsRunning`
- **إجمالى منتجات مستلمة من الإنتاج** — `Name=menuItem77`, `Click=menuItem77_Click` → `Reporting_System.Estlam_Production.Estlam_ProductionToStoreFRM`

### تقارير صرف أصناف وخامات  <sub>`menuItemSarfReport`</sub>

- **خامات مصروفة للإنتاج** — `Name=menuItem79`, `Click=menuItem79_Click` → `ItemsRunning`
- **إجمالى خامات مصروفة للإنتاج** — `Name=toolStripMenuItem1`, `Click=toolStripMenuItem1_Click` → `Item_Raw_MaterialTotalFrm`
- **قطع الغيار المصروفة** — `Name=menuItemSarfToSpareParts`, `Click=menuItemSarfToSpareParts_Click` → `ItemsRunning`
- **تقرير الأصناف التى تم صرفها / إستلامها** — `Name=ToolStripMenuItem_Sarf_FromStore`, `Click=ToolStripMenuItem_Sarf_FromStore_Click` → `Item_Sarf_FromStoreFrm`

- **بيان بأسعار الأصناف** — `Name=menuItemPricesList`, `Click=menuItem84_Click` → `Store_LIB.balance_`, `Stores.balance`, `Store_LIB.Accountss.Item_Balance`, `Accounts.Accountss.Item_Balance`
- **تقرير إحتياجات الأصناف** — `Name=toolStripMenuItemUponRequest`, `Click=toolStripMenuItemUponRequest_Click` → *(no `new Form(...)` found in handler — see note)*

### تقاير الإحتياجات  <sub>`menuItemNeeded`</sub>

- **تقارير إحتياجات المخازن - أصناف مبيعات** — `Name=menuItem86`, `Click=menuItem86_Click` → `NeedeD_balance`
- **تقارير إحتياجات المخازن - أصناف مشتريات** — `Name=menuItem87`, `Click=menuItem87_Click` → `NeedeD_balance`
- **تقارير إحتياجات أصناف المبيعات** — `Name=menuItem88`, `Click=menuItem88_Click` → `Needed_balance_Total`
- **تقارير إحتياجات أصناف مشتريات** — `Name=menuItem89`, `Click=menuItem89_Click` → `Needed_balance_Total`

- **رصيد أصناف لدى الموظفين او عهد** — `Name=ToolStripMenuItememployeeBalance`, `Click=ToolStripMenuItememployeeBalance_Click` → `balance_Employee`

## خدمات سياحية  <sub>`toolStripMenuItemToursServices`</sub>

### الفنادق  <sub>`menuItemHotel`</sub>

- **حجز فندق** — `Name=menuItemHotels`, `Click=MenuItemHotels_Click` → `Hotel_Tours`
- **قائمة حجز الفنادق** — `Name=menuItemHotelsList`, `Click=MenuItemHotelsList_Click` → `Hotel_Tours_List`

### تذاكر البرى  <sub>`menuItemLand`</sub>

- **حجز برى** — `Name=menuItemNewLand`, `Click=MenuItemNewLand_Click` → `Ticket_Tours_Land`
- **قائمة الحجز البرى** — `Name=menuItemLandList`, `Click=MenuItemLandList_Click` → `Ticket_Tours_Land_List`

### تأشيرات سياحية  <sub>`menuItemTasherat`</sub>

- **تـاشيرة سياحة** — `Name=menuItemTasherat_New`, `Click=MenuItemTasherat_New_Click` → `Tasherat_Tours`
- **قائمة التأشيرات الصادرة** — `Name=menuItemTasherat_List`, `Click=MenuItemTasherat_List_Click` → `Tasherat_Tours_List`

### حجز  الباصات  <sub>`menuItemBus`</sub>

- **إضافة باص** — `Name=MenuItemAddBus`, `Click=MenuItemAddBus_Click` → `AddBus`
- **قائمة الباصات** — `Name=MenuItemBusList`, `Click=MenuItemBusList_Click` → `AddBusList`
- **حجز باص** — `Name=MenuItemReserveBus`, `Click=MenuItemReserveBus_Click` → `Bus_Tours`
- **قائمة حجز الباصات** — `Name=MenuItemBusReservedList`, `Click=MenuItemBusReservedList_Click` → `Bus_Tours_List`

### برامج الحج والعمرة  <sub>`menuItemOmra`</sub>

- **إضافة برنامج** — `Name=menuItemAddPro`, `Click=MenuItemAddPro_Click` → `AddProgramOmra`
- **برامج الحج / العمرة** — `Name=menuItemProList`, `Click=MenuItemProList_Click` → `AddOmraProgramList`
- **حجز حج / عمرة** — `Name=menuItemOmraReserve`, `Click=MenuItemOmraReserve_Click` → `Omra_Tours`
- **بيانات حجز الحج / العمرة** — `Name=menuItemOmraList`, `Click=MenuItemOmraList_Click` → `Omra_Tours_List`

### مبيعات تذاكر الطيران  <sub>`menuItemTicket`</sub>

- **تذاكر الطيران** — `Name=menuItemAirTicketing`, `Click=MenuItemAirTicketing_Click` → `SalesTicket_Tours`
- **قائمة تذاكر الطيران** — `Name=menuItemTicketList`, `Click=MenuItemTicketList_Click` → `SalesTicket_Tours_List`
- **قائمة تذاكر الطيران المرتجع** — `Name=menuItemTicketList_Reverse`, `Click=DateOfReverse_Reverse_Click` → `SalesTicket_Tours_List_Reverse`


---

## Items whose `.Click` handler name was wired but no matching method body was found in the code-behind

These are candidates for the permission-gated/reflection dynamic-dispatch pattern flagged in `CLAUDE.md`, or the
handler lives in a different partial-class file / base class not scanned here — verify individually before
assuming dead code.

- `menuItemBanks` (`Click=menuItemBanks_Click`) — under بيانات أساسية
- `menuItemReceiveChech` (`Click=menuItemReceiveChech_Click`) — under أوراق القبض
- `menuItemSendCheck` (`Click=menuItemSendCheck_Click`) — under أوراق القبض

## Items with a handler body but no recognizable `new Form(...)` call

These likely delegate to a helper/shared method, reuse an already-open MDI child, only set state (e.g. `Common.*`
flags) before another action, or are guarded by an early-return permission check. Read the handler body directly
before assuming there is no target screen.

- `menuNoSkin` (`Click=menuNoSkin_Click`) — under ملف > تغيير وجهة البرنامج
- `menuItemListSide` (`Click=menuItemListSide_Click`) — under ملف
- `toolStripMenuItemUpdateDataBase` (`Click=toolStripMenuItemUpdateDataBase_Click`) — under ملف
- `StripMenuItemReloadSetting` (`Click=StripMenuItemReloadSetting_Click`) — under ملف
- `TaxInfotoolStripMenuItem` (`Click=TaxInfotoolStripMenuItem_Click`) — under ملف
- `menuItemRelatedSandoukAccounts` (`Click=menuItemRelatedSandoukAccountsToolStripMenuItem_Click`) — under ملف > ربط الحسابات
- `menuItemRelatedBankAccounts` (`Click=nmenuItemRelatedBankAccountsToolStripMenuItem_Click`) — under ملف > ربط الحسابات
- `menuItemRelatedEmployeeAccounts` (`Click=nmenuItemRelatedEmployeeAccountsToolStripMenuItem_Click`) — under ملف > ربط الحسابات
- `menuItemClose` (`Click=menuItemClose_Click`) — under ملف
- `menuDayShowWork` (`Click=menuDayShowWork_Click`) — under الحسابات > اليوميات
- `menuItemCustomerAccountCars` (`Click=menuItemCustomerAccountCars_Click`) — under الحسابات
- `menuItemCustomerAccountCarsPeriod` (`Click=menuItemCustomerAccountCarsPeriod_Click`) — under الحسابات
- `lookupCarTypeModelToolStripMenuItem` (`Click=lookupCarTypeModelToolStripMenuItem_Click`) — under السيارات
- `ToolStripMenuItemToolStripMenuItemExaminationList` (`Click=ToolStripMenuItemToolStripMenuItemExaminationList_Click`) — under السيارات
- `menuItemItems_ProductionDataFRMWabara` (`Click=menuItemItems_ProductionDataFRMWabara_Click`) — under الأصناف > إصناف النسيج
- `menuItemOrderEstlamSupplier` (`Click=menuItemOrderEstlamSupplier_Click`) — under المخازن > أذونات الصرف والإستلام
- `menuItemOrderSarfMachine` (`Click=menuItemOrderSarfMachine_Click`) — under المخازن > أذونات الصرف والإستلام
- `menuItemOrderSarfDepartment` (`Click=menuItemOrderSarfDepartment_Click`) — under المخازن > أذونات الصرف والإستلام
- `menuItemOrderEstlamFromDepartment` (`Click=menuItemOrderEstlamFromDepartmentToolStripMenuItem_Click`) — under المخازن > أذونات الصرف والإستلام
- `ToolStripMenuItem_Transaction_DetailsWithCost` (`Click=ToolStripMenuItem_Transaction_DetailsWithCost_Click`) — under المخازن > حركة الأصناف
- `menuItemItemsListSavedBalance` (`Click=MenuItemItemsListSavedBalance_Click`) — under المخازن
- `ToolStripMenuItemWorkOrderListWithNewItems` (`Click=ToolStripMenuItemWorkOrderListWithNewItems_Click`) — under المخازن
- `menuNewInvoice` (`Click=menuNewInvoice_Click`) — under المبيعات
- `menuNewInvoiceToSaveDirectly` (`Click=menuNewInvoiceToSaveDirectly_Click`) — under المبيعات
- `toolStripBillsList_Edit_SPriceFrm` (`Click=toolStripBillsList_Edit_SPriceFrm_Click`) — under المشتريات
- `menuPurchaseReport_2` (`Click=menuPurchaseReport_2_Click`) — under المشتريات
- `menuPurchaseReport` (`Click=menuPurchaseReport_Click`) — under  تقارير
- `menuItemMasroufat` (`Click=menuItem28_Click`) — under  تقارير > تقارير المصروفات
- `menuItemItemBalanceWithCost` (`Click=menuItemItemBalanceWithCost_Click`) — under  تقارير
- `toolStripMenuItemUponRequest` (`Click=toolStripMenuItemUponRequest_Click`) — under  تقارير المخزن

**Totals:** 67 submenu groups, 477 leaf actions rendered.