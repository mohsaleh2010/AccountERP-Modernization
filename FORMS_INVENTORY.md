# Forms & Screen-Navigation Inventory

Detailed per-project screen catalog for the solution documented in CLAUDE.md (imported from its **Forms Inventory** / **Screen Navigation** sections — read CLAUDE.md first for solution structure, architecture, and conventions). Every project's WinForms screens are cataloged here: purpose, key operations, navigation targets, and DB tables/procs touched, plus the per-project "batch observations" (duplication, dead code, encoding issues, etc.) found while cataloging them.

Per the *Areas Requiring Further Investigation* note in CLAUDE.md, treat every per-project form count below as a **lower bound** — several projects use custom base classes (Project_ControlLIB.MyForm, Connection4It.Template_Frm) that a naive ` : Form ` scan misses; only projects explicitly marked "re-verified" below have been corrected for this.

---

## Forms Inventory

**Scope and method note:** every WinForms project in the solution was scanned for classes declaring `class X : Form` (or `: System.Windows.Forms.Form`), directly. This misses forms that inherit a **custom intermediate base class** instead — confirmed real cases: `Project_ControlLIB.MyForm` (used across `Connection4It`, `Account_Common_LIB`, `CarsSystemLib`, `General_Ledger`, `ConstructionLib`, `Advanced_Account`'s `MainFrm_AccountUpdate`, and more), `Connection4It.Template_Frm` (used by at least one `ProductionsRates` form), and DevExpress's `XtraForm` (used throughout `EASY E-INVOICE`, which is almost entirely undercounted by this method — see its entry below). Where a batch's re-verification pass caught these, the corrected count and the specific forms are noted; where it wasn't re-verified, treat the count as a **lower bound**. Total forms found via the direct `: Form` pattern alone: **≈712** across 34 WinForms-bearing projects (Razor/Blazor pages in the `Razors\` group are a different UI paradigm and are listed separately, not here).

Every entry below is grounded in actual code (control declarations, SQL/table names, `.Show()`/`.ShowDialog()` call sites). Where a column could not be determined from the code examined, it says **Unknown** rather than guessing — this is common for `DB Tables/Procs` where a Form delegates entirely to a same-folder `*CLS` helper class whose own source wasn't in scope for that pass.

### LoginPro
**Startup form (if EXE):** N/A (Library — consumed by nearly every EXE shell via `mylogin.testlogin()`)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Authentication | LoginPro\Authentication.cs | Main login screen (Arabic RTL, "تسجيل الدخول") | Validates username/password, loads saved-credentials XML, selects branch | DBConfig (settings) | USER_DETAILES |
| Authentication_ | LoginPro\Authentication_.cs | Newer variant of Authentication (custom `MyButton` controls) | Same login flow as Authentication | DBConfig | USER_DETAILES |
| AuthenticationOld | LoginPro\AuthenticationOld.cs | Legacy/older login screen, reads ConnConfigInfoXML directly | Same login flow, older UI style | DBConfig | USER_DETAILES |
| LoginMain | LoginPro\LoginMain.cs | Splash/fade-in-out screen, no DB access | Opacity timer animation | AuthenticationOld (ShowDialog after fade) | — |
| ChangePassword | LoginPro\ChangePassword.cs | Change own password (Arabic; some strings show as mojibake — legacy ANSI encoding) | Validates old/new password, updates password | — | USER_DETAILES |
| Branch_UserPermission | LoginPro\Branch_UserPermission.cs | Assign which branches a user may access | Checklist of branches, save/delete permission rows | — | USER_DETAILES, USER_DETAILES_Branches_Permission |
| frmadmin | LoginPro\frmadmin.cs | Per-user/per-branch/per-project form-level permission admin | Add/remove permission rows per main-menu/sub-menu | — | FORMS_DETAILES, PERMATIONS |
| ValidModules | LoginPro\ValidModules.cs | Stub form with 3 unwired buttons (HR/Accounting/Store) | None — appears unused/placeholder | — | — |
| ADDNEWUSER | LoginPro\Users\ADDNEWUSER.cs | Add/edit/delete users, module flags (Account/HR/Store/Production/Gawda/Admin) | Insert/update/delete user, set branch | — | USER_DETAILES, USER_DETAILES_Branches_Permission |
| Authentication__2 | LoginPro\Users\Authentication_.cs | Duplicate login-form variant (file name ≠ class name) | Same login flow | DBConfig | USER_DETAILES |
| UsersList | LoginPro\Users\UsersList.cs | Grid list of users by branch | Fill grid via UsersCLS | ADDNEWUSER (add/edit) | USER_DETAILES (via UsersCLS) |

### Reporting_System
**Startup form (if EXE):** N/A (Library — RDLC report viewer forms hosted by other shells)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| SuppliersDataListPrintFrm | SuppliersRPT\SuppliersDataListPintFRM.cs | Print supplier list report | Filter by Active, bind list-view-model, refresh RDLC | — | via SupplierCLS.Get_SupplierVMByActive |
| CustomerDataFRm | CustomerRPT\CustomerDataFRm.cs | Print customer list report | Filter Active/Main-vs-General customer | — | via CustomerCLS.GetCustomerToPrint |
| Masrofat_RPT_FRM | Masrofat\Masrofat_RPT_FRM.cs | Money-spent (expenses) report, cash or check | Select account (tree dialog), date range, order-by | Connection4It.Account_Common.Account_ListFrmTree | via Money_SpentReportsCLS (dynamic SQL) |
| Item_QntyPurchasingFRM_ | Item_Balance_Purchase\Item_QntyPurchasingFRM_.cs | Supplier purchase price list report | Filter by supplier/category, TableAdapter Fill* variants | — | Supplier_Price (typed DataSet) |
| Items_ProductionDataWabaraFRM | ItemProductionData\Items_ProductionDataWabaraFRM.cs | Item price/unit list report (textile "Wabara" type) | Filter category+price type | — | Item_PriceUnit_List_Print (typed DataSet) |
| Items_ProductionDataLehafFRM | ItemProductionData\Items_ProductionDataLehafFRM.cs | Same report for "Lehaf" type (near-duplicate of above) | Filter category+price type | — | Item_PriceUnit_List_Print |
| Test | Reporting_System\Test.cs | Empty stub form | None | — | — |
| OrderMoneyPrint | OrderMoney\OrderMoneyPrint.cs | Print a single money order/receipt | Loads one Order_Money row, sets RDLC parameters | — | Order_Money (typed DataSet) |
| PrintingItemImageFrm | Reporting_System\PrintingItemImageFrm.cs | Shows item image + equivalent items list | Reads image bytes (ImageData) from DB into PictureBox | — | Item_PriceUnit_List (via TableAdapter) |
| Estlam_ProductionToStoreFRM | Estlam_Production\Estlam_ProductionToStoreFRM.cs | Production-received-into-store report | Filter by date range/store | — | EstlamProduction (typed DataSet) |
| MasrofatPrint | Reporting_System\MasrofatPrint.cs | Generic expenses print (parameterized RDLC) | Sets CompanyName/Category/dtFrom/dtTo/Title params | — | embedded rdlc, no direct query |
| CustomerInvoicesFRM | CarSystem\CustomerInvoices\CustomerInvoicesFRM.cs | Customer list report (near-duplicate of CustomerDataFRm, reused for Car system) | Filter Active/General customer | — | Customer (typed DataSet) |

### Clinic
**Startup form (if EXE):** N/A (Library — Main_Frm_Clinic is the MDI shell opened by a host EXE)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Main_Frm_Clinic | Clinic\Main_Frm_Clinic.cs | MDI main menu for clinic module | Menu routing only | ChangePassword, DBConfig, BackupFrm, GroupsListFrm, Customer_Info, PatientClinic, DrugsListFrm, MedicalTestListFrm, RadiologyListFrm, PatientClinicList | — |
| PatientClinic | Clinic\Clinic\PatientClinic.cs | Core patient-visit encounter form: diagnosis + drugs/tests/radiology lines | Add/edit/delete visit-detail rows, create visit header | Connection4It.PrintInvoice | PatientClinicListCLS: ClinicPationtList/ClinicPationtDetails tables |
| PrintInvoice | Clinic\Print\PrintInvoice.cs (namespace `Accounts`, linked/shared file — not clinic-specific) | Generic sales-invoice RDLC print | Builds totals rows, sets RDLC params, chooses template by company | — | via Accounts.Requests.Requests_Cls |
| RadiologyListFrm | Clinic\MainData\Radiology\RadiologyListFrm.cs | CRUD grid of radiology test types | Fill/update/delete grid rows | AddDrugs (generic add dialog) | ClinicRadiology (via RadiologyCLS) |
| DrugsListFrm | Clinic\MainData\Drugs\DrugsListFrm.cs | CRUD grid of drug lookup types | Fill/update/delete grid rows | AddDrugs | ClinicDrugs (via DrugsListCLS) |
| MedicalTestListFrm | Clinic\MainData\MedicalTest\MedicalTestListFrm.cs | CRUD grid of medical test lookup types | Fill/update/delete grid rows | AddDrugs | ClinicMedicalTests (via MedicalTestCLS) |
| PatientClinicList | Clinic\Clinic\PatientClinicList.cs | Search/list clinic visits by date/name/diagnosis | Search, total ticket price row | PatientClinic (double-click row) | via PatientClinicListCLS |
| AddDrugs | Clinic\MainData\Drugs\AddDrugs.cs | Generic "add lookup value" dialog (name is legacy leftover, reused for drugs/tests/radiology) | Validate + insert | — | via DrugsListCLS.Add_Currency |
| Omra_Tours | Clinic\Tours_Omra\Omra_Tours.cs (namespace `Account_Common_LIB`, linked/shared file — unrelated to clinic domain) | Umrah/Hajj travel ticket booking | Ticket pricing, reverse-ticket flow, order-money in/out | Ticket_Tours.ReverseTicket_Tours, Money_Transaction.Cash_Money.OrderMoney_Transaction | Tours_Omra, Customer/Customers_Accounts, Supplier/Suppliers_Accounts |
| Omra_Tours_List | Clinic\Tours_Omra\Omra_Tours_List.cs (namespace `Account_Common_LIB`) | List/search Omra/Hajj tickets by program | Totals commission/ticket/purchase price | Omra_Tours (double-click) | via Omra_ToursCLS |
| AddProgramOmra | Clinic\Tours_Omra\AddProgramOmra.cs (namespace `Account_Common_LIB`) | Add/edit Umrah/Hajj travel package pricing | Compute total from component prices | — | Tours_Program_Omra_Heg_List |
| AddOmraProgramList | Clinic\Tours_Omra\AddOmraProgramList.cs (namespace `Account_Common_LIB`) | List Umrah/Hajj travel programs | Filter active/type | AddProgramOmra (double-click) | Tours_Program_Omra_Heg_List |

### ClinicPatient
**Startup form (if EXE):** N/A (Library, near-fork of Clinic — namespace of most forms is still `Clinic`)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Main_Frm_Clinic | ClinicPatient\Main_Frm_Clinic.cs (namespace `Clinic`) | MDI main menu (adds "Drugs Used" vs Clinic's version) | Menu routing | ChangePassword, DBConfig, BackupFrm, GroupsListFrm, Patient_Info, PatientClinic, DrugsListFrm, MedicalTestListFrm, RadiologyListFrm, PatientClinicList, UsedListFrm | — |
| Form1 | ClinicPatient\Form1.cs | Empty unused stub | None | — | — |
| UsedListFrm | ClinicPatient\MainData\Used\UsedListFrm.cs | CRUD grid of "drug usage/dosage instructions" lookup | Fill/update/delete grid | AddDrugs | ClinicUsed (via UsedCLS) |
| PatientClinic | ClinicPatient\Clinic\PatientClinic.cs | Patient-visit encounter (adds ClinicUsed_Id dimension vs Clinic's version) | Add/edit/delete visit lines; inline "new lookup" shortcuts | Connection4It.PrintRosheta / PrintRoshetaNoName (prescription print), RadiologyListFrm/MedicalTestListFrm/DrugsListFrm/UsedListFrm (as dialogs) | ClinicPationtList/ClinicPationtDetails |
| PatientClinicList | ClinicPatient\Clinic\PatientClinicList.cs | Search/list visits | Search by date/name/diagnosis | PatientClinic (double-click) | via PatientClinicListCLS |
| AddDrugs | ClinicPatient\MainData\Drugs\AddDrugs.cs | Generic add-lookup dialog | Validate + insert | — | via DrugsListCLS |
| DrugsListFrm | ClinicPatient\MainData\Drugs\DrugsListFrm.cs | CRUD grid of drugs | Fill/update/delete | AddDrugs | ClinicDrugs |
| MedicalTestListFrm | ClinicPatient\MainData\MedicalTest\MedicalTestListFrm.cs | CRUD grid of medical tests | Fill/update/delete | AddDrugs | ClinicMedicalTests |
| RadiologyListFrm | ClinicPatient\MainData\Radiology\RadiologyListFrm.cs | CRUD grid of radiology types | Fill/update/delete | AddDrugs | ClinicRadiology |

### FreeMedical
**Startup form (if EXE):** MainFrm (FreeMedical\FreeMedical\MainFrm.cs) — via Program.cs `Application.Run(new MainFrm())` pattern (MDI shell)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| MainFrm | FreeMedical\FreeMedical\MainFrm.cs | MDI shell for free-treatment patient tracking | Menu routing | AddNew, AddMedicalType, SearchPatientFrm | — |
| AddNew | FreeMedical\FreeMedical\AddNew.cs | Add/edit free-medical patient record | Validate Name/National-ID/file-ID, save | — | via PatientCLS (Patients table) |
| SearchPatientFrm | FreeMedical\FreeMedical\NewFolder1\SearchPatientFrm.cs | Search patients + printable list report | Search by name/national-ID/medical type, print via custom ListView | AddNew (edit) | via PatientCLS.Get_Patients |
| AddMedicalTypeAll | FreeMedical\FreeMedical\NewFolder1\AddMedicalTypeAll.cs | Add new medical-type description lookup | Validate + insert | — | via PatientCLS.AddNewMedicalTypeDescription |
| AddMedicalType | FreeMedical\FreeMedical\NewFolder1\AddMedicalType.cs | CRUD grid of medical-type descriptions | Fill/update/delete | AddMedicalTypeAll | via PatientCLS |
| PrintOptions | FreeMedical\PrintableListView\PrintOptions.cs (linked copy of the PrintableListView project's own form) | Print-options dialog (columns/title/fit-to-page) for the custom printable ListView control | — | — | — |
| mainForm | FreeMedical\Project_ControlLIB\MohSalehCntrl1\CNTRLS\mainForm.cs (linked control-lib leftover) | Trivial empty designer stub | None | — | — |
| form | FreeMedical\Project_ControlLIB\MohSalehCntrl1\CNTRLS\form.cs (linked control-lib leftover) | Trivial empty stub | None | — | — |
| Form1 | FreeMedical\Project_ControlLIB\MohSalehCntrl1\Form1.cs (linked control-lib leftover) | Trivial empty stub | None | — | — |

### Store_LIB
**Startup form (if EXE):** N/A (Library)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| ItemBalanceWithExpireFrm | Items\Item_Balance\ItemBalanceWithExpireFrm.cs | Item balance editor with expiry-date batches | Search item by code/name, edit expire-date balances grid, print barcode | Mitems_List_PH (edit/add item), PrintBarCodeForOneItemsSaidalia | via ItemCLS/ItemDataPhCLS; Unit_item, Unit_Item_ExpireDate |
| balanceOnlyEdit_Units | Report\Needed_balance\balanceOnlyEdit_Units.cs | Store/category balance report + inline balance edit | Filter by store/category/item-type dims, save updated balances | Store_LIB.Report.Needed_balance.BalancePRT (print) | via Stores_Items_List / ItemCLS.UpdateItemBalance |
| Store_ListOLD | 1_Old\Store_ListOLD.cs | Legacy store master CRUD (name/location/employee/job) | Add/update/delete store, empty-store data-wipe | — | Store, Employee, jobsList, Item, Unit_item |
| Item_detailes | Report\Item_detailes.cs | Item movement detail dialog (push/receive between stores) | Loads Order_Purshing/Detailes_Purshing join by store | (commented-out) Recived_order_Push | Item, Unit_item, unit, Order_Purshing, Detailes_Purshing, Store, Supplier, Customer, Department |
| unit.cs → class `Units_List` | 1_POS\unit.cs | Unit-of-measure CRUD (big/small unit + conversion factor) | Add/update/delete unit | — | unit (via Class2/Unit legacy classes) |
| BarCodeSettingsForm | ItemsList_Store_\BarCode\BarCodeSettingsForm.cs | Configure barcode sticker/print settings (built entirely in code, no designer) for BarCodeCtrlNew | Apply/reset settings live to control preview | — | — (in-memory control settings only) |
| unit_.cs → class `unit_` | 1_Old\unit_.cs | Near-duplicate legacy unit-of-measure CRUD dialog (same code as Units_List, different layout) | Add/update/delete unit | — | unit |

### AccountCheckMoney
**Startup form (if EXE):** N/A (Library — bank-checks module)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Order_Receipt_ChecksFrmNew | Order_ChecksFrm\Order_Receipt_ChecksFrmNew.cs | Create/view a receive-or-issue-checks order (header + check lines) | Add check, save order, delete order/check, print | Check_DataFrmNewNew (add/edit check), Transfer_Checks_List (endorse), PrintableListView.PrintableCLS | via Order_Receipt_Checks class (Order_Money/Checks_Memory) |
| CheckPrintPrnt | Print\CheckPrintPrnt.cs | RDLC print of check register (import/export or endorsement report) | Sets RDLC params, chooses embedded report by ReportNo | — | embedded rdlc, dt passed in |
| ChangeCheck_ToAccountDataFrmNew | ChangeCheck_MemoryStatus\ChangeCheck_ToAccountDataFrmNew.cs | Change a check's "memory status"/related account | Loads check data, validates new From/To accounts, saves status change | — | via Order_Receipt_ChecksNewNew (Checks_Memory) |
| FirstBalanceImports_Checks_List | FirstBalance_Checks\FirstBalanceImports_Checks_List.cs | Opening-balance checks import/list screen | Fill list, print via thread | Check_DataFrmNewNew | via Order_Receipt_Checks.Get_Check_1 |
| FirstBalance_Checks_List | FirstBalance_Checks\FirstBalance_Checks_List.cs | Opening-balance checks list | Fill list, totals row | FirstBalanceCheck_DataFrmNewNew | via Order_Receipt_ChecksNewNew.Get_Check_1/Get_Check_OrderMoney |
| Check_DataTasweaData | Order_ChecksFrm\Check_DataTasweaData.cs | Check "settlement" (Taswea) dialog: pick From/To accounts + status | Read/save account selection | — | via Checks_MemoryTasweaCLS |
| Transfer_Checks_ListOld | Order_ChecksFrm\Transfer_Checks_ListOld.cs | Legacy check-endorsement/transfer selection list | Select check to endorse/transfer | Check_DataFrmNewNew | via Order_Receipt_Checks.Get_Check_OrderMoney |

### administration
**Startup form (if EXE):** N/A (Library — user/permissions administration)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| ADDNEWUSER | administration\ADDNEWUSER.cs | Add/edit/delete users (module flags: Account/HR/Store/Production/Gawda/Admin/Active) | Insert/update/delete USER_DETAILES | — | USER_DETAILES |
| MAIN_FORM_IN_ADMINI | administration\MAIN_FORM_IN_ADMINI.cs | Admin module main menu | Menu routing | ADDNEWUSER, frmadmin, LoginPro.ChangePassword, MenuFrmAdmin | — |
| MenuFrmAdmin | administration\MenuFrmAdmin.cs | Main-menu/sub-menu permission assignment per user/project | Assign/remove main-menu and sub-menu permissions | — | via MenuFrmAdminCLS (SysConfigMenus/PERMATIONS) |
| LOGINFRM | administration\LOGINFRM.cs | Standalone debug/test login form | Loops all USER_DETAILES comparing plaintext, shows debug MessageBoxes | — | USER_DETAILES |
| frmadmin | administration\frmadmin.cs | Per-user/per-project form-level permission screen (duplicate of LoginPro's frmadmin, no branch dimension) | Add/remove permission rows | — | FORMS_DETAILES, PERMATIONS |
| Form1 | administration\Form1.cs | Empty unused stub | None | — | — |

### AbsencePrecenseProj
**Startup form (if EXE):** N/A (Library, legacy v3.5 non-SDK csproj — HR attendance module)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| presence_AbsenceStatusFrm | ReportsFrm\presence_AbsenceStatusFrm.cs | Attendance/absence status report (count-of-days or list-of-days) | Query and render list, print via custom ListView | — | via InOutReg.Get_Emp_Precence_AbsenceCount/Days |
| MainFrm | AbsencePrecense\MainFrm.cs | MDI shell for attendance module (skin-engine theming) | Toolbar buttons open InOutRegistration with different modes | InOutRegistration (In/Out/Absence/MyOut), presence_AbsenceStatusFrm | — |
| InOutRegistrationRange | AbsenceFrm\InOutRegistrationRange.cs | Bulk in/out/absence/license-day registration over a date range | Register selected employees across date range, validates time overlaps | — | via InOutReg class |
| InOutRegistration | AbsenceFrm\InOutRegistration.cs | Single-day in/out/absence/license-day registration (largest, most tab-heavy form in project) | Register/delete in/out/absence entries for a day | — | via InOutReg class |
| Form1 | AbsencePrecenseProj\Form1.cs | Empty unused stub | None | — | — |

### Advanced_Account
**Startup form (if EXE):** N/A (Library)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| FormMainMenu2 | Advanced_Account\FormMainMenu2.cs | Modern dashboard-style MDI shell (colored side menu, panel-hosted child forms) | ActivateButton theming, OpenChildForm pattern | FormOrders, Customer_Info | — |
| MainFrm_AccountUpdate | Advanced_Account\MainFrm_AccountUpdate.cs | **The actual ERP main shell** — enormous legacy MDI menu form (~9,000 lines / ~484KB; base class `Project_ControlLIB.MyForm`, so missed by a naive `: Form` scan; contains 718 `new <Form>(...)` navigation calls) covering checks, invoices (local+government e-invoice), accounts tree, bills, stores/items, reports, day registers, tasheel/motagra, profit-loss | Menu-driven navigation to essentially the whole ERP surface | Dozens of forms across Store_LIB, AccountCheckMoney, Reporting_System, Account_Common_LIB, General_Ledger, etc. | (many, not individually enumerated) |
| FormOrders | Advanced_Account\FormOrders.cs | Simple themed placeholder panel | LoadTheme() colors buttons from ThemeColor | — | — |
| FormMainMenu | Advanced_Account\FormMainMenu.cs | Earlier/duplicate dashboard shell (Spanish-labeled buttons: Productos/Clientes/Compras/Empleados/Pagos/Reportes) | OpenChildForm pattern | FormOrders | — |
| Form1 (namespace `GUI_MODERNISTA`) | Advanced_Account\Form1.cs | Dashboard-shell UI-kit template/tutorial copy (Spanish UI, mostly commented-out) | AbrirFormEnPanel pattern | — (calls commented out) | — |

### BarCodeCreator
**Startup form (if EXE):** BarCodeCreator_MainFrm (BarCodeCreator\BarCodeCreator_MainFrm.cs) — inferred from menu structure; Form1.cs is an unused stub

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| ItemsListFrm | Items\ItemsListFrm.cs | CRUD grid of items for barcode printing | Fill/update/delete grid | AddItems | via ItemsFactorsCLS |
| AddItems | Items\AddItems.cs | Add new item directly for barcode assignment | Insert Item + Unit_item rows, optional auto-code update | — | Item, Unit_item |
| BarCodeCreator_MainFrm | BarCodeCreator\BarCodeCreator_MainFrm.cs | MDI shell | Menu routing | Store_LIB.PrintItemsBarCodeSettingFor_Developer, ItemsListFrm, AccountCommonDataLIB.DBConfig, LoginPro.ChangePassword | — |
| Form1 | BarCodeCreator\Form1.cs | Empty unused stub | None | — | — |

### AccountsEInvoicesLIB
**Startup form (if EXE):** N/A (Library — Egyptian Tax Authority e-invoice module)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| LoadItemCodeFromTaxPortal | ItemTax\LoadItemCodeFromTaxPortal.cs | Grid of tax-portal-accepted item (GS1/EGS) codes | Fill grid | — | via ItemCodeTaxAcceptedCLS |
| NewItemCodeListNotUploaded | ItemTax\NewItemCodeListNotUploaded.cs | Manage locally created item codes not yet submitted to the ETA e-invoice portal | Add/edit code, async "Send to Tax Portal" (RestSharp) | Mitems_ListTax | via ItemCodeTaxNotUploadedCLS |
| BranchesFrm | AccountsEInvoicesLIB\BranchesFrm.cs | CRUD of company tax branches + e-invoice portal config | Add/edit/delete branch; stores ClientId/ClientSecret/TokenPin/portal URLs (config fields present — values not reproduced here) | — | Tax_Branch, Tax_BranchConfig |
| Mitems_ListTax | ItemTax\Mitems_ListTax.cs | Add/edit a single tax item code (EGS auto-generated from internal code + registration number) | Validate + save | — | via ItemCodeTaxNotUploadedCLS |

### EncryptionCA
**Startup form:** `encryption` (EncryptionCA\Main.cs — contains the project's `[STAThread] static void Main()`)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| ReadXml | EncryptionCA\ReadXml.cs | Debug/test form for reading & rewriting ConnInfo.xml | Read XML, mutate a row, write back | — | — (XML file, not DB) |
| encryption | EncryptionCA\Main.cs | **Startup form** — simple Caesar-cipher encrypt/decrypt demo UI | Encrypt/decrypt text with numeric key | Form1 | — |
| Form1 | EncryptionCA\Form1.cs | Simple encode/decode demo using default key | Encode/decode via EncryptProcess | — | — |

### AccountCommonDataLIB
**Startup form (if EXE):** N/A (Library — the shared DB-config/connection-error UI used by nearly every EXE shell)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| DBConfig | Forms\DBConfig.cs | **Shared DB-connection configuration screen** used across most EXE projects in the suite | Configure server/db/credentials by connection style (SQL Express, Windows Auth, LocalDB, etc.), test connection, attach/detach .mdf, save to encrypted XML | — | via DBConfigCLS; tests raw SqlConnection |
| ConnErrorTest | Forms\ConnErrorTest.cs | Auto-shown by the `operations` base class when a DB connection fails | Test connection / Reconfigure / Exit app | DBConfig | — |
| MsgBoxShowFrm | DBContext\MsgBoxShow.cs | Generic message-box replacement (Msg/MsgTitle properties + OK) | Display message | — | — |

Also reviewed (non-Form DB-access core classes in this project): `DBContext\operations.cs` wraps raw ADO.NET SqlConnection access, auto-validates the connection string on construction and pops `ConnErrorTest` on failure, plus a license/expiration check (`Common.ExpirationVersion()`); `DBContext\ConnConfigInfoXML.cs` reads/writes `CommonFiles\ConnInfo.xml` (the persistent store behind `DBConfig`, contains credential-shaped fields — values not reproduced); `DBContext\EncryptionClass\EncryptProcess.cs` is the simple Caesar-cipher-style shift cipher described in *Database Access* — not real cryptography.

### Project_ControlLIB
**Startup form (if EXE):** N/A (Library). Despite the name suggesting only controls, grep found 2 `Form`-derived classes — confirmed on inspection these are incidental/leftover, not the project's purpose.

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Form1 | MohSalehCntrl1\Form1.cs | Trivial empty designer-generated stub, unused | None | — | — |
| MyForm | MyControls\MyForm.cs | Lightweight base `Form` subclass (RTL, centered start position, Times New Roman) — the shared base class other projects' forms inherit from | None (no business logic) | — | — |

Note: the project is genuinely a custom WinForms **control library** (MyButton, MyTextBox, MyGroupBox, MyComboBox, MyNumericTextBox, BarCodeCtrlNew, etc.), multi-targeted `net10.0-windows` + `net48`. The 2 "forms" found are not real screens.

### PrintableListView
**Startup form (if EXE):** N/A (Library)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| PrintOptions | PrintableListView\PrintOptions.cs | Print-options dialog (column checklist, all-vs-selected rows, fit-to-page, title) for the custom PrintableListView ListView control used for reports across the whole suite | Returns selected columns/options via public properties | — | — |

### General_LedgerAccount
**Startup form (if EXE):** N/A (Library)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Form1 | General_LedgerAccount\Form1.cs | Empty unused stub — no logic despite the "General Ledger" project name | None | — | — |

### BackUp_Project
**Startup form (if EXE):** N/A (Library, hosted as a system-tray background utility by a host EXE)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| FrmMainBackup | BackUp_Project\BackUp_Project\FrmMainBackup.cs | System-tray-resident SQL Server backup scheduler | Timer-driven (5–6pm-ish) or manual "Backup Now"; runs `BACKUP DATABASE ... TO DISK` for several hardcoded DB names into dated folders under `E:\DBBackup\`; registry "run at Windows startup" toggle | — | `BACKUP DATABASE` T-SQL against several hardcoded database names (not reproduced) |

### DataBaseSettining
**Startup form:** DBConfig (DataBaseSettining\DBConfig.cs — has its own `[STAThread] static void Main()`: `Application.Run(new DBConfig(""))`)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| DBConfig | DataBaseSettining\DBConfig.cs | Standalone earlier iteration of the DB-connection-config screen (predates the one now in AccountCommonDataLIB) | Configure/test connection, save to ConnInfo.xml, detach DB | — | — (XML config + `sp_detach_db`) |

### EASY E-INVOICE
**Startup form:** `frmshocecompany` (EASY E-INVOICE\BaseForm\frmshocecompany.cs) — per `Program.cs`: `Application.Run(new frmshocecompany())`. **Form1.cs is unused generated boilerplate, never referenced.**

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| frmshocecompany | BaseForm\frmshocecompany.cs | **Startup form** — company/e-invoice setup screen | Sets DB params, loads Tax_BranchVM list on load | — | via Tax_BranchCLS |
| Form1 | EASY E-INVOICE\Form1.cs | Unused generated stub, not referenced by Program.cs | None | — | — |

**Important scope note:** the "1" form-count for this project (from the plain `: Form` grep) is a severe undercount. This project actually contains a large number of additional forms (frmBranch, frmCustomer, frmProduct, FrmProductRec/Send, FrmCurrncy, MainForm/MainForm2/XtraForm1‑3, FrmIqrar, FrmSendExcell, FrmSendEmanualFrm, FrmSearchEinvoice, FrmAddItem, WaitForm1, License/TRIAL) that derive from **DevExpress's `XtraForm`**, not plain `Form`. There is also a `SDK\` folder implementing the Egyptian Tax Authority e-invoice/e-receipt web-service integration, and a `Test\` folder with a DevExpress UI-kit demo. **Full cataloguing of the XtraForm screens is flagged under *Areas Requiring Further Investigation*.**

### AccountsERPSystem
**Startup form:** via `Program.cs` → `Accounts.LoadingFrm` (post-login loading) → **`MainFrm_AccountUpdate`** (from Advanced_Account) as the MDI shell, or `MainFrmPOS` if `Common.PointOfSale==1`. Flow: culture setup → `Common.SetSystemParameters()`/`GetLoadParameter()` → `mylogin.testlogin()` (LoginPro) → `LoadingFrm.ShowDialog()` → hardware-serial check (`HardWareInfo.CheckSerials_Compatibility`) → `Application.Run(new MainFrm_AccountUpdate())`.

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Form1 | AccountsERPSystem\Form1.cs | Unused generated stub, not referenced by Program.cs | None | — | — |

### AccountsERPSystemNet10
**Startup form:** Same flow/pattern as AccountsERPSystem — `Application.Run(new MainFrm_AccountUpdate())`, byte-for-byte near-identical `Main()`.

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Form1 | AccountsERPSystemNet10\Form1.cs | Unused generated stub, not referenced by Program.cs | None | — | — |

**Comparison to AccountsERPSystem:** these are **near-duplicate EXE shells** — same startup sequence, same `MainFrm_AccountUpdate` main form actually run. AccountsERPSystemNet10 differs by referencing `Razors\AccountERP.SharedUI\AccountERP.SharedUI.csproj` and wiring `Microsoft.AspNetCore.Components.WebView.WindowsForms` (`AddWindowsFormsBlazorWebView`) plus a full DI container (`ServiceCollection`) registering `My_DbContext`, `operations`, `MenuRepository`, `MenuStateService`, `LanguageService`, `IAuthService`/`WebAuthService`, `AppSettings` — but this DI/BlazorWebView plumbing is built and **not yet wired into navigation**: `MainFrm_AccountUpdate` (the legacy WinForms MDI shell) is still what actually runs at startup, indicating an in-progress migration path toward hosting Razor/Blazor Hybrid components inside the legacy WinForms shell. Also directly references `AccountModelsLib` and `General_LedgerAccount` (which `AccountsERPSystem`'s csproj does not).

**Batch observations (small projects, applies across LoginPro/Reporting_System/Clinic/ClinicPatient/FreeMedical/Store_LIB/AccountCheckMoney/administration/AbsencePrecenseProj/Advanced_Account/BarCodeCreator/AccountsEInvoicesLIB/EncryptionCA/AccountCommonDataLIB/Project_ControlLIB/PrintableListView/General_LedgerAccount/BackUp_Project/DataBaseSettining/EASY E-INVOICE/AccountsERPSystem/AccountsERPSystemNet10):**
- **Pervasive duplication:** many "New"/"Old"/"_2"-suffixed sibling classes do nearly the same thing (LoginPro's `Authentication`/`Authentication_`/`AuthenticationOld`/`Authentication__2`; `DBConfig` re-implemented almost verbatim in `AccountCommonDataLIB` and `DataBaseSettining`; `frmadmin`/`ADDNEWUSER` duplicated between `LoginPro` and `administration`).
- **Cross-project linked source files:** several "small" projects compile in `.cs` files whose namespace doesn't match the containing project (Clinic project contains `Account_Common_LIB.Omra_Tours*` and `Accounts.PrintInvoice`; FreeMedical embeds `PrintableListView.PrintOptions` and `Project_ControlLIB.MohSalehCNTRL` stub forms) — classic old-style "Add Existing Item as Link" reuse instead of project/package references.
- **DAL pattern is uniform but risky:** virtually all data access goes through the hand-rolled `operations` class exposing `return_dataset`/`return_DataTable`/`insert_me`/`update_me`/`delete_me`, fed raw string-concatenated SQL everywhere observed — no parameterized queries seen, so SQL-injection exposure is the norm, not an exception.
- **Error handling idiom:** dominant pattern is bare `try { } catch { }` or `catch (Exception ee) { }` with `ee` unused — failures are silently swallowed rather than logged, except where a `MessageBox.Show` surfaces a DB error to the user.
- **Encoding inconsistency:** several older files (Store_LIB `unit.cs`/`unit_.cs`, AbsencePrecenseProj `InOutRegistration*.cs`, `administration\frmadmin.cs`, `LoginPro\AuthenticationOld.cs`) show Arabic literals as mojibake when read as UTF-8, suggesting a legacy ANSI/Windows-1256 codepage; newer files use clean UTF-8 Arabic — a mixed-encoding codebase.
- **Legacy/modern coexistence:** v2.0/v3.5/v4.5/v4.8 non-SDK `.csproj` files (FreeMedical, AbsencePrecenseProj, Clinic, ClinicPatient, DataBaseSettining, EASY E-INVOICE, EncryptionCA) sit in the same solution as modern SDK-style, multi-targeted projects — an incremental, still-in-progress .NET Framework → modern .NET migration.
- **Naming is inconsistent throughout:** PascalCase/camelCase/ALL_CAPS coexist even within one file; class names and file names frequently diverge (e.g. `Users\Authentication_.cs` contains class `Authentication__2`; `Store_LIB\1_POS\unit.cs` contains class `Units_List`).

### GHospital_Care (HMIS) — vendored third-party product
**Main/startup form:** `Session.Login` (launched via `Application.Run(new Session.Login())` in `Program.cs`; on successful login it opens `MainWindow`, the MDI container that hosts all other forms as MDI children). Note: `Program.cs` also has a hardcoded expiry check — `if (DateTime.Now.Date > new DateTime(2024,8,25)) Application.Exit();` — meaning **this build will not run at all** past that date until the check is edited.

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Login | Session/Login.cs | Startup login screen | Authenticates via `USER_DETAILES` table; hardcodes Arabic hospital name "مستشفى الديولى" instead of reading company name; opens DB config dialog | MainWindow, external `DBConfig` (Account_Common_LIB) | USER_DETAILES, tblCompany |
| Logout | Session/Logout.cs | Logout confirmation/screen | Minimal logic | — | — |
| SessionData | Session/SessionData.cs | Not a Form (session state holder class) | Stores UserName etc. | — | — |
| MainWindow | MainWindow.cs | Main MDI parent / navigation shell with menu | Loads menu permissions from DB (`SetSystemParameters_Menus_DB`), hosts ~90 menu click handlers that open child forms | Nearly every form in the project plus external forms from `Account_Common_LIB`/`LoginPro` | Menu permission table via `AccountCommonDataLIB.Common` |
| Form1 | Form1.cs | Unknown/leftover scaffold form; queries wards | Loads `tblWards` into a grid | — | tblWards |
| Form2 | Form2.cs | Stub, default VS template form | None (stub) | — | — |
| HomeTask | Forms/HomeTask.cs | Company/hospital profile & logo setup (mislabeled "Home Task") | CRUD on company profile incl. logo upload | — | tblCompany, tblUserAccount, SP_COMPANY_PROFILE |
| AccountSetup | Accounts/AccountSetup.cs | Chart-of-accounts entry (create/edit accounts) | Save/edit account records | — | tblAccounts, SP_SAVE_tblAccounts |
| BankSummary | Accounts/BankSummary.cs | Bank account balance summary | Loads bank-type accounts & balance for a period | — | tblAccounts (AccType='Bank Account') |
| Cashbook | Accounts/Cashbook.cs | Cashbook report (UI built, date-range/grid controls) | **Stub** — constructor only calls InitializeComponent | — | none wired |
| DailyExpense | Accounts/DailyExpense.cs | Daily expense report | **Stub** | — | none |
| Ledger | Accounts/Ledger.cs | General ledger view | **Stub** | — | none |
| MonthlySalesAchivements | Accounts/MonthlySalesAchivements.cs | Monthly sales achievement report | **Stub** | — | none |
| ProfitLoss | Accounts/ProfitLoss.cs | Profit & Loss statement | **Stub** | — | none |
| VoucherEntry | Accounts/VoucherEntry.cs | Voucher/payment entry | Generates next voucher no., loads accounts, saves voucher | — | tblVoucher, tblAccounts |
| DoctorAdvance | Doctors/DoctorAdvance.cs | Doctor salary advance | **Stub**, unreferenced anywhere | — | none |
| DoctorList | Doctors/DoctorList.cs | List of doctors | Loads doctor grid | — | tblDoctors |
| DoctorMaster | Doctors/DoctorMaster.cs | Doctor master CRUD | Auto-generates DoctorID via SP, save/edit | — | tblDoctors, tblDoctorSpecialization, SP_GENERATE_DOCTORID, SP_SAVE_tblDoctors |
| DoctorSalary | Doctors/DoctorSalary.cs | Doctor salary | **Stub**, unreferenced anywhere | — | none |
| EmployeeAdvance | Employees/EmployeeAdvance.cs | Employee salary advance | **Stub**, unreferenced anywhere | — | none |
| EmployeeList | Employees/EmployeeList.cs | List of employees | Loads employee grid | — | tblEmployees |
| EmployeeMaster | Employees/EmployeeMaster.cs | Employee master CRUD incl. photo | Auto-generate EmpID, save employee, update photograph | — | tblEmployees, tblDepartments, SP_GENERATE_EMPID, SP_SAVE_tblEmployees |
| SalaryGenerator | Employees/SalaryGenerator.cs | Generates monthly salary sheet | Runs SP to build salary sheet for month/year | — | tblSalarySheet, tblSalary, SP_CREATE_SALARY_SHEET |
| SalarySetup | Employees/SalarySetup.cs | Employee salary structure setup | Save/edit per-employee salary config | — | tblSalary, tblEmployees, SP_SAVE_tblSalary |
| SalarySheet | Employees/SalarySheet.cs | Prints salary sheet (Crystal Report) | Calls shared `ReportTools.ReportLoader.LoadSalarySheet` | — | via ReportLoader |
| AddLeave | HRM Management/AddLeave.cs | Leave request entry | **Stub** | — | none |
| LeaveStatus | HRM Management/LeaveStatus.cs | Leave status view | **Stub** | — | none |
| LeaveTypeSetup | HRM Management/LeaveTypeSetup.cs | Leave type setup | **Stub** | — | none |
| ViewLeaveDetails | HRM Management/ViewLeaveDetails.cs | View leave details | **Stub** | — | none |
| AboutGHospitalCare | Help/AboutGHospitalCare.cs | About box | Minimal | — | — |
| CheckUpdate | Help/CheckUpdate.cs | Check-for-update dialog | Minimal | — | — |
| Contents | Help/Contents.cs | Help contents | Minimal | — | — |
| Register | Help/Register.cs | Product registration/activation dialog | Toggle "have license"/"purchase", opens `http://www.gsoft-bd.com/ghospital-care/activator/`, disables parent while shown | MainWindow (parent re-enable) | — |
| Search | Help/Search.cs | Help search | Minimal | — | — |
| TechnicalSupport | Help/TechnicalSupport.cs | Technical support info | Minimal | — | — |
| BedFeeInfo | IndoorPatient/BedFeeInfo.cs | Bed fee info | **Stub**, unreferenced | — | none |
| DailyDoctorWiseVisitSchedule | IndoorPatient/DailyDoctorWiseVisitSchedule.cs | Daily doctor visit schedule | **Stub**, unreferenced | — | none |
| DischargeIP | IndoorPatient/DischargeIP.cs | Discharge in-patient | Menu-wired but constructor/handlers do nothing beyond Hide() — UI built (992-line Designer) but not functional | Opened from MainWindow | none wired |
| DoctorWisePatient (class DoctorWisePatient_IP) | IndoorPatient/DoctorWisePatient.cs | List IP services/bills by doctor for a date range | Loads doctor list, joined query for services by doctor | Opened from MainWindow | tblIPBillDetail, tblIPBill, tblIP, tblServices, tblSubServices |
| IPAdmission | IndoorPatient/IPAdmission.cs | In-patient admission entry | Admits OP patient to IP, auto-generates IDs, saves IP/IPBill records | Opened from MainWindow | tblOP, tblIP, tblIPBill, tblWards, tblDoctors, SP_GENERATE_IPID, SP_GENERATE_IPBILLID, SP_SAVE_tblIPBill |
| IPBedShiftment | IndoorPatient/IPBedShiftment.cs | Move patient between ward/room/bed | Loads wards/rooms/free beds, updates assignment | Opened from MainWindow | tblIP, tblWards, tblRooms, tblBed |
| IPBilling | IndoorPatient/IPBilling.cs | In-patient billing (main) | Bill lookup, line-item detail, voucher totals, save bill/detail | Opened from MainWindow | tblIPBill, tblIPBillDetail, tblIPVoucher, tblIP, SP_SAVE_tblIPBill, SP_SAVE_tblIPBillDetail, SP_GENERATE_IPBILLID |
| IPBilling_OUTDATE | IndoorPatient/IPBilling_OUTDATE.cs | Titled "تسجيل خروج المريض" (patient discharge registration) despite filename | **Stub** (38 lines), unreferenced — deprecated/abandoned earlier discharge form | — | none |
| IPBilling_One | IndoorPatient/IPBilling_One.cs | Near-duplicate of IPBilling ("IP Billing" title too) — alternate/simplified single-bill variant | Same table/SP pattern as IPBilling | Opened from MainWindow; opens `Account_Entry_Print` | tblIPBill, tblIPBillDetail, tblIPVoucher, tblIP, SP_GENERATE_IPID, SP_GENERATE_IPBILLID |
| IPBilling_One_En | IndoorPatient/IPBilling_One_En.cs | English-labeled near-duplicate of IPBilling_One | Same table/SP pattern | **Orphaned** — never instantiated anywhere in the project | tblIPBill, tblIPBillDetail, tblIPVoucher |
| IPList | IndoorPatient/IPList.cs | List active in-patients | Loads active IP grid | Opened from MainWindow | tblIP (Status='Active') |
| MedicalTreatment | IndoorPatient/MedicalTreatment.cs | Medical treatment record | **Stub**, but wired from MainWindow menu | Opened from MainWindow | none wired |
| MonthlyAdmissionAndDischargeDetails | IndoorPatient/MonthlyAdmissionAndDischargeDetails.cs | Monthly admission/discharge report | **Stub**, unreferenced | — | none |
| PatientAmountReceive | IndoorPatient/PatientAmountReceive.cs | Receive payment from IP patient | Saves IP voucher, generates voucher no., loads patient/bill | Opened from MainWindow | tblIPVoucher, tblIP, tblIPBill, SP_SAVE_tblIPVoucher |
| PatientsInformation | IndoorPatient/PatientsInformation.cs | IP patients by admission date range | Loads IP grid filtered by date | — | tblIP |
| PrintIPBill | IndoorPatient/PrintIPBill.cs | Print an IP bill | Loads bill from a view | — | ViewIPBill |
| ServiceTreatement | IndoorPatient/ServiceTreatement.cs | Service treatment | **Stub**, unreferenced | — | none |
| VisitConfirmation | IndoorPatient/VisitConfirmation.cs | Visit confirmation | **Stub**, unreferenced | — | none |
| VisitSchedule | IndoorPatient/VisitSchedule.cs | Visit schedule | **Stub**, unreferenced | — | none |
| IPServiceItems | IndoorServicesItems/IPServiceItems.cs | Add service items to an IP bill (title "IP Billing") | Loads bill, joins IPBill+IP, adds detail lines | Opened from MainWindow | tblIPBill, tblIP |
| PrintServicesItems_HIS | IndoorServicesItems/PrintServicesItems_HIS.cs | Print services report via Microsoft RDLC report viewer | Takes a DataTable, renders via `Microsoft.Reporting.WinForms` | — | (passed DataTable; no direct SQL) |
| PrintServicesItemsfrm | IndoorServicesItems/PrintServicesItemsfrm.cs | Print services report via Crystal Reports — duplicate reporting path vs. PrintServicesItems_HIS using a different engine | Loads Crystal report with passed DataTable | — | (passed DataTable) |
| ServicesItemsfrm | IndoorServicesItems/ServicesItemsfrm.cs | Enter lab/service result values for a patient/sub-service | Delegates to `IndoorServicesItemsCLS` helper | — | tblSubServices, tblSubServices_Results (via helper) |
| Account_Entry_Print | Journal_Entry/Account_Entry_Print.cs | Print/view a journal entry (title "عرض القيد وطباعته") | Renders entry for printing | Opened from NewAccount_EntryFRM | Account_Entry_List/Details (via Accounts_Entry_POST_CLS) |
| Add_Account_EntryFRM | Journal_Entry/New folder/Add_Account_EntryFRM.cs | Add an account line to a journal entry | — | **Orphaned** — only referenced in commented-out code | Unknown |
| NewAccount_EntryFRM | Journal_Entry/NewAccount_EntryFRM.cs | Main journal/voucher entry form (Arabic, title "إدخال القيود") — large (2202 lines) | Debit/credit grid entry, balance validation ("القيد غير متزن"), save via `AccountCommonDataLIB.Common` helpers rather than direct SQL, opens print form | Opened from MainWindow (multiple menu items); opens Account_Entry_Print | Account_Entry_List / Account_Entry_Details (via shared lib) |
| NewAccount_EntryFRM_C | Journal_Entry/NewAccount_EntryFRM_C.cs | Near-duplicate of NewAccount_EntryFRM (same title, 1889 lines) | Same pattern | **Orphaned** — never instantiated anywhere | same as above |
| BloodGroup | LabTest/BloodGroup.cs | Blood group test entry | **Stub** (duplicate name w/ ReportViewer.BloodGroup and Pathology.BloodGroupTest) | — | none |
| ElectrolytesReport | LabTest/ElectrolytesReport.cs | Electrolytes test report | **Stub** | — | none |
| FebrilAntigen | LabTest/FebrilAntigen.cs | Febrile antigen test | **Stub** (duplicate of Pathology.FebrilAntigen, which is fully implemented) | — | none |
| PeripheralBloodFilmStudy | LabTest/PeripheralBloodFilmStudy.cs | Peripheral blood film study | **Stub** | — | none |
| Make_Account_Entry_Automatic | Master_Account/Make_Account_Entry_Automatic.cs | Auto-generate cashbox journal entries | Unknown detail (126 lines) | **Orphaned** — never instantiated anywhere | Unknown |
| MasterAccountPrintFRM | Master_Account/MasterAccountPrintFRM.cs | Print sub-ledger for an account | Print report | Opened from Master_Account_Details2 and Master_Account_DetailsTarseed2 | Unknown (report-only) |
| Master_Account_CostCenter_FRM | Master_Account/Master_Account_CostCenter_FRM.cs | Cost-center sub-ledger | Opens Master_CostCenterPrintFRM | **Orphaned** itself — not opened from MainWindow or elsewhere | Submain_Account-family tables |
| Master_Account_Details2 | Master_Account/Master_Account_Details2.cs | Account sub-ledger by date | Loads ledger detail, opens print form | Opened from MainWindow; opens MasterAccountPrintFRM | Submain_Account, Account_Entry_List/Details |
| Master_Account_DetailsTarseed2 | Master_Account/Master_Account_DetailsTarseed2.cs | Account sub-ledger by period (carry-forward variant) | Loads ledger detail, opens print form | Opened from MainWindow; opens MasterAccountPrintFRM | Submain_Account, Account_Entry_List/Details |
| Master_CostCenterPrintFRM | Master_Account/Master_CostCenterPrintFRM.cs | Print cost-center sub-ledger | Print report | Opened from Master_Account_CostCenter_FRM and _TarsesdFRM | Unknown (report-only) |
| Master_Account_CostCenter_TarsesdFRM | Master_Account/New folder/Master_Account_CostCenter_TarsesdFRM.cs | Cost-center sub-ledger, period/carry-forward variant | Opens Master_CostCenterPrintFRM; references `Account_Entry_Master_Balance_MezanTemp` | **Orphaned** — never instantiated | Account_Entry_Master_Balance_MezanTemp, Submain_Account |
| Review_Account_Entry | Master_Account/Review_Account_Entry.cs | Review/audit posted journal entries | Browse/search entries | Opened from MainWindow | Account_Entry_List/Details |
| Tar7eel_Account_Entry | Master_Account/Tar7eel_Account_Entry.cs | Post ("ترحيل") journal entries to ledger | Unknown detail (132 lines) | **Orphaned** — never instantiated anywhere | Account_Entry_List/Details (likely) |
| DutyByCommon | Nurses/DutyByCommon.cs | Nurse duty report (all nurses), via Crystal Reports | Calls shared `ReportTools.ReportLoader.LoadNurseDutySheetAll` | Opened from MainWindow | via ReportLoader |
| DutyByNurse | Nurses/DutyByNurse.cs | Nurse duty report (single nurse) | Validates Nurse ID, calls `ReportLoader.LoadNurseDutySheetByNurse` | Opened from MainWindow | via ReportLoader |
| NurseDutySchedule | Nurses/NurseDutySchedule.cs | Assign nurse duty schedule | Save duty assignment, lookup existing | Opened from MainWindow | NurseInformation, tblNurseDuty, SP_SAVE_tblNurseDuty |
| NurseGenerator | Nurses/NurseGenerator.cs | Generate nurse salary sheet | Runs SP to build salary sheet for month/year | Opened from MainWindow | Nurse_Salary_Sheet, tblNurseSalary, SP_Nurse_SALARY_SHEET |
| NurseInfo | Nurses/NurseInfo.cs | Nurse master CRUD | Generate NurseID, save/edit/delete | Opened from MainWindow | NurseInformation |
| NurseSalary | Nurses/NurseSalary.cs | Nurse salary setup | Save salary config | Opened from MainWindow | tblNurseSalary, NurseInformation, SP_SAVE_SalaryNurse |
| NurseSheet | Nurses/NurseSheet.cs | Nurse salary sheet print (Crystal Reports) | Calls `ReportLoader.LoadNurseSalarySheet` | Opened from MainWindow | via ReportLoader |
| OPConsultation | OutdoorPatient/OPConsultation.cs | OP consultation/prescription (substantial 723-line Designer) | **Stub** code-behind — **orphaned**, never instantiated anywhere despite large built UI | — | none wired |
| OPDrWise | OutdoorPatient/OPDrWise.cs | OP visits by doctor | Loads doctor list, filters OP by doctor | Opened from MainWindow | tblDoctors, tblOP |
| OPInformation | OutdoorPatient/OPInformation.cs | Outpatient registration | Auto-generate OPID, save OP record; joins to accounting `Submain_Account` | Opened from MainWindow | tblOP, tblDoctors, tblPatientList, Submain_Account, SP_GENERATE_OPID, SP_SAVE_tblOP |
| OPInformationReport | OutdoorPatient/OPInformationReport.cs | OP report by date range | Loads OP grid by date | Opened from MainWindow | tblOP |
| OPServiceTreatment | OutdoorPatient/OPServiceTreatment.cs | OP service treatment list by date range | Loads OP grid by date | Opened from MainWindow | tblOP |
| OutpatientBilling | OutdoorPatient/OutpatientBilling.cs | Outpatient billing | Save OP bill + bill details | Opened from MainWindow | tblOPBill, tblOP, tblOPBillDetails, SP_SAVE_tblOPBill, SP_SAVE_tblOP, SP_SAVE_tblOPBillDetails |
| BloodGroupTest | Pathology/BloodGroupTest.cs | Blood group test report entry | Loads pathologist/lab/doctor lists, auto report no., save | Opened from MainWindow | tblPathologist, tblLabs, tblDoctors, tblBloodGrouping, SP_SAVE_tblBloodGrouping |
| BloodTest | Pathology/BloodTest.cs | Blood test | **Stub** (large 1392-line Designer but no wired logic) | — | none |
| FebrilAntigen | Pathology/FebrilAntigen.cs | Febrile antigen test entry | Loads pathologist/lab/doctor lists, auto report no., save | Opened from MainWindow | tblPathologist, tblLabs, tblDoctors, tblFebrilAntigen, SP_SAVE_tblFebrilAntigen |
| SerologicalTest | Pathology/SerologicalTest.cs | Serological test entry | Same CRUD pattern as above | Opened from MainWindow | tblPathologist, tblLabs, tblDoctors, tblSerologicalTest, SP_SAVE_tblSerologicalTest |
| StoolExam | Pathology/StoolExam.cs | Stool examination | **Stub** (large Designer, 1832 lines, unwired) | — | none |
| Ultrasongraphy | Pathology/Ultrasongraphy.cs | Ultrasonography | **Stub** — but its stored proc `SP_SAVE_tblUltraSonography` is actually called from **UrineExamine.cs** (see below), a likely copy-paste bug | — | tblUltraSonography (referenced from UrineExamine, not here) |
| UrineExamine | Pathology/UrineExamine.cs | Urine examination entry | Loads pathologist/lab/doctor lists, auto report no.; **saves via `SP_SAVE_tblUltraSonography`** (the Ultrasonography SP) with urine-exam parameters — apparent copy-paste bug, observed not fixed | Opened from MainWindow | tblPathologist, tblLabs, tblDoctors, SP_SAVE_tblUltraSonography |
| LossProfitStatement | Pharmacy/LossProfitStatement.cs | Pharmacy loss/profit statement | **Stub** | — | none |
| Medicine | Pharmacy/Medicine.cs | Medicine master CRUD | Auto ID, save, list | Opened from MainWindow | tblMedicine, SP_SAVE_tblMedicine |
| Payment | Pharmacy/Payment.cs | Pharmacy payment | **Stub** | — | none |
| Purchase | Pharmacy/Purchase.cs | Pharmacy purchase | **Stub** | — | none |
| Recieve | Pharmacy/Recieve.cs | Pharmacy stock receive | **Stub** | — | none |
| Sale | Pharmacy/Sale.cs | Pharmacy sale | **Stub** | — | none |
| Stock | Pharmacy/Stock.cs | Pharmacy stock view | **Stub** | — | none |
| PrintIPBill_Thermal | PrintPatientReciept/PrintIPBill_Thermal.cs | Print IP bill on thermal-receipt format | Loads bill from view | — | ViewIPBill |
| BloodGroup (ReportViewer) | ReportViewer/BloodGroup.cs | Blood-group-test report viewer | **Stub**-level code (25 lines) | — | none directly |
| FebrilAntigenViewer | ReportViewer/FebrilAntigenViewer.cs | Febrile antigen report viewer | **Stub**-level (25 lines) | — | none directly |
| OPList | ReportViewer/OPList.cs | Outdoor patient list report viewer | **Stub** | — | none |
| SerologicalReport | ReportViewer/SerologicalReport.cs | Serological test report viewer | **Stub**-level (25 lines) | — | none |
| ViewReport | ReportViewer/ViewReport.cs | Generic report viewer shell | **Stub**-level (36 lines) | — | none |
| BackupDatabase | Settings/BackupDatabase.cs | Database backup | Creates a `.gs`-extension backup file to default or user-chosen path | Opened from MainWindow | (backup mechanism, table detail unknown) |
| BedSetup | Settings/BedSetup.cs | Bed master setup | Save bed record, loads wards/rooms | Opened from MainWindow | tblBed, tblWards, tblRooms, SP_SAVE_tblBed |
| ChangePassword | Settings/ChangePassword.cs | Change password (local) | **Stub**, unreferenced — MainWindow instead uses external `LoginPro.ChangePassword` | — | none |
| CompanyContract | Settings/CompanyContract.cs | Company/insurance contract setup | Save contract, auto ID | Opened from MainWindow | tblCompanyContract, SP_SAVE_tblCompanyContract |
| Departments | Settings/Departments.cs | Department master | CRUD, auto ID | Opened from MainWindow | tblDepartments, SP_SAVE_tblDepartments |
| Deseases | Settings/Deseases.cs | Disease master | CRUD, auto ID | Opened from MainWindow | tblDesease, SP_SAVE_tblDesease |
| DoctorSpecialization | Settings/DoctorSpecialization.cs | Doctor specialization master | CRUD, auto ID | Opened from MainWindow | tblDoctorSpecialization, SP_SAVE_tblDoctorSpecialization |
| HospitalProfile | Settings/HospitalProfile.cs | Hospital/company profile setup | Save/load company profile | Opened from MainWindow | tblCompany, SP_SAVE_tblCompany |
| Labs | Settings/Labs.cs | Lab master | CRUD, auto ID | Opened from MainWindow | tblLabs, SP_SAVE_tblLabs |
| Pathologist | Settings/Pathologist.cs | Pathologist master | CRUD, auto ID | Opened from MainWindow | tblPathologist, SP_SAVE_tblPathologist |
| RoomSetup | Settings/RoomSetup.cs | Room master (per ward) | CRUD via `insert_me_SP` helper | Opened from MainWindow | tblRooms, tblWards, SP_SAVE_tblRooms |
| ServiceSetup | Settings/ServiceSetup.cs | Service master | CRUD, auto ID | Opened from MainWindow | tblServices, SP_SAVE_tblServices |
| SubServiceReportItemsSetup | Settings/SubServiceReportItemsSetup.cs | Report items per sub-service | CRUD | Opened from MainWindow | tblServices, tblSubServices, tblSubServices_ReportItem, SP_SAVE_tblSubServices_ReportItem |
| SubServiceSetup | Settings/SubServiceSetup.cs | Sub-service master | CRUD | Opened from MainWindow | tblServices, tblSubServices, SP_SAVE_tblSubServices |
| SubServices_ReportItemValues | Settings/SubServices_ReportItemValues.cs | Reference/normal values per report item | CRUD | Opened from MainWindow | tblServices, tblSubServices, tblSubServices_ReportItem, tblSubServices_ReportItemValues, SP_SAVE_tblSubServices_ReportItemValues |
| UserManagment | Settings/UserManagment.cs | User account management | CRUD on user accounts — note: manages `tblUserAccount`, but Login.cs actually authenticates against `USER_DETAILES`, a **different, disconnected table** | Opened from MainWindow | tblUserAccount, SP_SAVE_tblUserAccount |
| WardSetup | Settings/WardSetup.cs | Ward master | CRUD via `insert_me_SP` helper | Opened from MainWindow | tblWards, SP_SAVE_tblWards |

**Batch observations (GHospital_Care):**
- **Naming convention split:** hospital-domain tables use `tblXxx` PascalCase (tblIP, tblOP, tblDoctors, tblIPBill, etc.), while the embedded double-entry accounting subsystem (Journal_Entry/, Master_Account/) uses `Underscore_Case` table names (Account_Entry_List, Account_Entry_Details, Submain_Account, Money_Transaction_Box) — two distinct data models grafted into one app.
- **Two parallel/inconsistent user tables:** `Settings/UserManagment.cs` performs CRUD on `tblUserAccount`, but `Session/Login.cs` authenticates against a different table, `USER_DETAILES` — the two do not appear connected; a comment in Login.cs shows the `tblUserAccount`-based query was commented out in favor of `USER_DETAILES`.
- **No password hashing observed** — Login.cs builds a parameterized query comparing plaintext `USER_PASSWORD`; no hashing/salting code seen.
- **Namespace/folder mismatch:** many files under `Master_Account/` and `Journal_Entry/` declare `namespace GHospital_Care` (the project root namespace) instead of a per-folder sub-namespace, unlike most other folders.
- **A significant fraction of forms are empty stubs** — constructor only calls `InitializeComponent()`, sometimes with a fully built Designer/UI but zero wired event-handler logic. Entire folders (`HRM Management/`, most of `Pharmacy/` except `Medicine.cs`, most of `LabTest/`) are stub-only. Some stubs are still wired into the MainWindow menu, others are completely orphaned, and some corresponding MainWindow menu click handlers are themselves empty.
- **Duplicate/near-duplicate forms:** `IPBilling.cs`, `IPBilling_One.cs`, `IPBilling_One_En.cs` (three ~1100-1200 line forms, same tables/SPs, title "IP Billing"; `_En` variant never referenced anywhere); `NewAccount_EntryFRM.cs` (2202 lines) and `NewAccount_EntryFRM_C.cs` (1889 lines, never instantiated); `LabTest/FebrilAntigen.cs` (stub) duplicates `Pathology/FebrilAntigen.cs` (fully implemented); `IndoorServicesItems/PrintServicesItems_HIS.cs` and `PrintServicesItemsfrm.cs` both print "services items" via two different reporting engines (RDLC vs Crystal Reports).
- **Likely copy-paste bug (observed, not fixed):** `Pathology/UrineExamine.cs`'s save routine calls stored procedure `SP_SAVE_tblUltraSonography` (the Ultrasonography SP) while passing urine-exam-specific parameters.
- **Hardcoded values observed:** `Program.cs` has a hardcoded hard-stop expiry date (`2024-08-25`); `Session/Login.cs` hardcodes an Arabic hospital display name instead of reading it from `tblCompany`.
- **Data-access pattern:** most hospital-domain forms use raw inline SQL/stored procedures directly via `SqlCommand`/`SqlDataAdapter` (no repository/DAL layer) with a small local `Conn` connection-string helper class; some forms instead use the shared `AccountModelsLib.operations` helper — usage is inconsistent even within the same folder.
- **Shared report-generation helper:** `ReportTools.ReportLoader` is used by several Crystal-Reports-based forms (SalarySheet, NurseSheet, DutyByCommon, DutyByNurse).
- **Error handling pattern:** consistent `try/catch` around DB/file operations with `MessageBox.Show(...)`; some catches are empty/swallowed.
- **External/shared library dependency:** MainWindow references many forms not defined in this project at all — they come from `Account_Common_LIB`/`LoginPro` (e.g. `Account_Tree_CodesFrm`, `MoneyBanksFrm`, `BackupFrm`, `DBConfig`, `LoginPro.ChangePassword`) — a real part of the app's navigation graph reused from the ERP's shared accounting/infrastructure module.
- **Commercial/licensed product indicators:** `Help/Register.cs` and `Session/Login.cs` both link to `gsoft-bd.com`, confirming this is a commercial off-the-shelf hospital management product (vendor "GSoft", likely Bangladesh-based) rather than a bespoke build.
- **Mixed language UI:** most hospital-domain forms (patients, doctors, nurses, pharmacy, settings) are English-labeled; the accounting/journal-entry subsystem (`Journal_Entry/`, `Master_Account/`) is entirely Arabic-labeled — the accounting module was localized/adapted for an Arabic-speaking client while the hospital module remains in English.

*Methodology note (Stores/ReservationSystemLIB/Alter_DataBase/CarsSystemLib batch below): source files in these four projects are inconsistently encoded (UTF-8 vs. legacy Windows-1256 Arabic, no BOM); captions were extracted via per-file encoding auto-detection to avoid mojibake.*

### Stores
**Startup form:** `MainFrm_Store` (per the disabled/commented-out `Main11()` method and the login→MDI flow it shows) — **but** the active `Main()` in `Stores/Program.cs` has its `Application.Run(MainFrm)` call commented out, so as checked into this tree, the compiled entry point does not actually launch any UI form. Looks like an in-progress refactor left mid-way — see *Known Technical Constraints*.

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Departments | Stores/Departments.cs | "بيانات الأقسام" (Department data) — CRUD for warehouse departments | Add/edit/delete department | opened from MainFrm_Store | Department, Unit_item, item, employee, store |
| Doors | Stores/Doors.cs | "البوابه" (The Gate) — lists purchase orders pending gate/security clearance | list pending dispatch orders | opened from MainFrm_Store | Order_Purshing |
| EditFirstBalance | Stores/EditFirstBalance/EditFirstBalance.cs | "كارت الصنف" (Item Card) — sets an item's opening/first balance | edit opening balance via FirstBalanceCLS | no internal callers found (likely dynamically loaded) | Detailes_Purshing, Order_Purshing |
| Form1 | Stores/Form1.cs | Empty unmodified VS template stub | none | none | none — dead code |
| helpabout | Stores/helpabout.cs | About box | show version/about info | opened from MainFrm_Store | none |
| ItemsCards | Stores/ItemsCards/ItemsCards.cs | "كارت الصنف" (Item Card) — item transaction ledger | view item movement history | opened from MainFrm_Store (x2) | Item, Unit_item, unit, Order_Purshing, Detailes_Purshing (via ItemsCardsCLS) |
| ItemsCardsForDept | Stores/ItemsCards/ItemsCardsForDept/ItemsCardsForDept.cs | Per-department variant of item card | view item movement per department | no internal caller found | same as ItemsCards |
| ItemsCardsStore_Days | Stores/ItemsCards/ItemsCardsForDept/ItemsCardsStore_Days.cs | "يومية المخزن" (Warehouse daily journal) | daily warehouse transaction log | opened from MainFrm_Store | via ItemsCardsCLS |
| ItemsCardsStore | Stores/ItemsCards/ItemsCardsStore.cs | "حركة المخازن" (Warehouse movement) | view store-to-store movement | opened from MainFrm_Store (x2) | via ItemsCardsCLS |
| EstlamProductionItems | Stores/ItemsCards/ItemsRunning/EstlamProductionItems.cs | "خامات مصروفة للإنتاج" (materials issued to production) | list/manage production-issue transactions | no internal caller found | Unit_item, Item (via CLS) |
| ItemsHalk | Stores/ItemsCards/ItemsRunning/ItemsHalk.cs | "أصناف فى قائمة الهالك" (scrap/waste items) | manage scrapped/damaged items | referenced (commented-out) from MainFrm_Store | via ItemsCardsCLS |
| ItemsRunning | Stores/ItemsCards/ItemsRunning/ItemsRunning.cs | **Generic, reusable transaction-list form**, ctor `(OrderType_, Sarf_Estlam_)` | reused 6× from MainFrm_Store for destruction, maintenance-items, surplus/deficit adjustment, production output | opened from MainFrm_Store (6 distinct menu handlers) | Unit_item, Item (via CLS) |
| MainFrm_Store | Stores/MainFrm_Store.cs | "المخازن" (Stores/Warehouses) — MDI main-menu hub for the whole EXE | ~100+ menu click handlers opening local + shared-library forms; login/permission check | opens ~30 forms | PERMATIONS, USER_DETAILES |
| item_ | Stores/New_Forms_In_Account/item_.cs | "تكويد الأصناف" — item master CRUD | add/edit item, unit, store links | opens item_report; no internal caller found | item, unit, unit_item, store, Unit_item |
| item_report | Stores/New_Forms_In_Account/item_report.cs | "تقرير عن الاصناف" — item report | print/view item report | opened from item_ | Item |
| Store_List | Stores/New_Forms_In_Account/Store_List.cs | "بيانات المخازن" — warehouse master list | CRUD stores | opens store_report; no internal caller found | Store, Employee, item |
| store_report | Stores/New_Forms_In_Account/store_report.cs | "تقرير عن المخازن" — store report | print/view store report | opened from Store_List | Store |
| unit (class Units_List) | Stores/New_Forms_In_Account/unit.cs | "الوحدات" — measurement-unit CRUD | add/edit/delete unit | opened from MainFrm_Store | unit |
| New_Orders_purchasing_List | Stores/Orders/Order_Purchase/New_Orders_purchasing_List.cs | "أوامر الصرف الجديدة" — new dispatch/issue orders browse list | select/create dispatch order | opens Order_PurshingFrm; opened from MainFrm_Store | Order_Purshing |
| Order_PurshingFrm | Stores/Orders/Order_Purchase/Order_PurshingFrm.cs | "إذن الصرف" — dispatch/issue permit | create/edit dispatch order lines | via Order_purchasingClss | Order_Purshing, Detailes_Purshing, Unit_item, Invoices_Details, Invoices_List |
| New_Orders_purchasing_List_Mortaga3 | Stores/Orders/Order_Purchase_Mortaga3Sarf/New_Orders_purchasing_List_Mortaga3.cs | **Near-duplicate** of above for "Mortaga3" (returned) dispatch orders | same, reversed workflow | opens Order_PurshingFrm_Mortaga3 | Order_Purshing (via Order_purchasingClss_Mortaga3) |
| Order_PurshingFrm_Mortaga3 | Stores/Orders/Order_Purchase_Mortaga3Sarf/Order_PurshingFrm_Mortaga3.cs | "إذن الصرف" — reversed-dispatch permit | create/edit reversed dispatch lines | via Order_purchasingClss_Mortaga3 | Invoices_Details_Reverse, Invoices_List_Reverse, Order_Purshing, Detailes_Purshing |
| New_Order_Receive_List | Stores/Orders/Order_Receive/New_Order_Receive_List.cs | "أوامر الإستلام الجديدة" — new receiving orders | select/create receiving order | opens Order_ReceiveingFrm | Order_Receiving |
| Order_ReceiveingFrm | Stores/Orders/Order_Receive/Order_ReceiveingFrm.cs | "إذن إستلام من مورد" — receipt permit from supplier | create/edit receiving order lines | via Order_ReceiveingClss | Order_Receiving, Detailes_Receiving, Bills_Details, Bills_List, Unit_item |
| New_Order_Receive_List_Mortaga3Estlam | Stores/Orders/Order_Receive_Mortaga3Estlam/New_Order_Receive_List_Mortaga3Estlam.cs | **Near-duplicate** of above for reversed receiving | same, reversed workflow | opens Order_ReceiveingFrm_Mortaga3Estlam | via Order_ReceiveingClss_Mortaga3Estlam |
| Order_ReceiveingFrm_Mortaga3Estlam | Stores/Orders/Order_Receive_Mortaga3Estlam/Order_ReceiveingFrm_Mortaga3Estlam.cs | "إذن إستلام من مورد" — reversed-receiving permit | create/edit reversed receiving lines | via Order_ReceiveingClss_Mortaga3Estlam | Bills_Details_Reverse, Bills_List_Reverse, Order_Receiving, Unit_item |
| OrderChoice | Stores/OrdersNew/OrderChoice.cs | "بيانات الإذن" — chooser dialog to start a new stock-movement order | select order source/destination and type | via OrderChoiceCLS; opens Order_Details flow | Store, Department, Supplier, Customer, Item, Unit_item, Order_Purshing |
| Order_Details | Stores/OrdersNew/Order_Details.cs | Order line-item detail/balance entry (newer "OrdersNew" module) | add/edit lines, balance adjustments across stores | via Order_DetailsCls; opened by TransferItems (out of scope) | Order_Purshing, Detailes_Purshing, Unit_item, OrderTypes, OrderTypes_Label, Department, Supplier, Customer, Store, Machin_list_Textile |
| search | Stores/search.cs | General item-movement search/report, joins purchasing/receiving history by date range | filter/report on item transactions | opened from MainFrm_Store | Detailes_Purshing, Order_Purshing, Detailes_Receiving, Order_Receiving, item, unit, unit_item, department |

**Batch observations (Stores):**
- `Program.cs`'s active `Main()` has its `Application.Run(MainFrm)` call commented out — the visible active code path never shows any UI; a fully-commented `Main11()` reveals the intended flow (login → `MainFrm_Store`).
- `MainFrm_Store` is a single giant MDI hub with 100+ click handlers, opening both local and shared-library forms outside this table's `class X : Form` pattern.
- Heavy direct ADO.NET with **string-concatenated SQL** throughout — a pervasive SQL-injection-shaped pattern, no parameterized queries observed.
- Business logic is split into companion `...Cls`/`...CLS` classes, a partial MVP-style separation (though many forms still embed SQL directly too).
- **"Mortaga3"/"Estlam" (return) variants duplicate the purchase/receive order forms almost verbatim** against parallel `_Reverse`-suffixed tables — classic copy-paste-per-workflow instead of a shared parameterized form.
- `ItemsRunning` shows the opposite, better pattern: one generic form parameterized by `(OrderType, Sarf_Estlam)` reused for 6+ different stock-adjustment menu commands.
- Several forms have **no `new X()` call anywhere in the project** — presumably launched dynamically by name from a permissions/menu table (mirroring the `FORMS_DETAILES`/`PERMATIONS` pattern confirmed in Alter_DataBase), not via hardcoded menu wiring.
- Uses the `operations` DAL pervasively (23 files); only 1 file references `My_DbContext` (EF Core) — EF is essentially unused at the UI layer here despite being available.
- Arabic-only UI, `RightToLeft = Yes`.

### ReservationSystemLIB
| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Tasherat_ToursPrint | Tasherat_Tours/Print/Tasherat_ToursPrint.cs | "طباعة بيانات الحجز" — visa booking voucher print | render/print report | opened by Tasherat_Tours flow | via Tasherat_ToursCLS |
| Tasherat_Tours | Tasherat_Tours/Tasherat_Tours.cs | "تأشيرة سياحه" (tourism visa) — visa booking entry | create/edit booking; posts Customer/Supplier balance changes | via Tasherat_ToursCLS; opens print | Tours_Tasherat, Customer, Supplier, Customers_Accounts, Suppliers_Accounts, Order_Money, Money_Transaction_Box |
| Tasherat_Tours_List | Tasherat_Tours/Tasherat_Tours_List.cs | "تأشيرات السياحة" — visas list | list, edit, print visas | opens Tasherat_Tours, Tasherat_ToursPrint | Tours_Tasherat |
| Hotel_Tours | Ticket_Hotels/Hotel_Tours.cs | "حجز فندق" (hotel booking) — same pattern as Tasherat_Tours | create/edit hotel booking; posts balances | via Hotel_ToursCLS | Tours_Hotels, Customer, Supplier, Customers_Accounts, Suppliers_Accounts |
| Hotel_Tours_List | Ticket_Hotels/Hotel_Tours_List.cs | "حجز فنادق" — hotel bookings list | list/browse | opens Hotel_Tours, Ticket_HotelsPrint | Tours_Hotels |
| Ticket_HotelsPrint | Ticket_Hotels/Print/Ticket_HotelsPrint.cs | "طباعة بيانات الحجز" — hotel voucher print | print | opened from Hotel_Tours_List | via Hotel_ToursCLS |
| TicketSales_ForSupplier | Ticket_SalesRPT/TicketSales_ForSupplier.cs | "تقرير مبيعات التذاكر" — per-supplier ticket sales report | filter/print report | standalone report form | Suppliers_Accounts (implied) |
| Ticket_ToursPrint | Ticket_Tours/Print/Ticket_ToursPrint.cs | "طباعة بيانات الحجز" — flight ticket voucher print (shared) | print | opened from SalesTicket_Tours_List and _List_Reverse | flight ticket tables (via CLS) |
| ReverseTicket_Tours | Ticket_Tours/ReverseTicket_Tours.cs | "بيانات إرتجاع التذكرة" — cancellation/refund workflow | process ticket return | no direct SQL in-form; delegates to CLS | Unknown (delegates) |
| SalesTicket_Tours | Ticket_Tours/SalesTicket_Tours.cs | "حجز تذاكر الطيران" — flight ticket booking, main entry | create/edit booking; posts Customer/Supplier balances | opens print flow | Customer, Supplier, Customers_Accounts, Suppliers_Accounts |
| SalesTicket_Tours_List | Ticket_Tours/SalesTicket_Tours_List.cs | "تذاكر الطيران" — flight tickets list | list/browse | opens Ticket_ToursPrint | tickets table (via CLS) |
| SalesTicket_Tours_List_Reverse | Ticket_Tours/SalesTicket_Tours_List_Reverse.cs | "تذاكر الطيران المرتجع" — returned tickets list | list/browse returned tickets | opens Ticket_ToursPrint | tickets table (via CLS) |
| SalesTicket_Tours_Old | Ticket_Tours/SalesTicket_Tours_Old.cs | "بيع تذاكر واستلام نقدية" — **legacy/alternate** ticket-sales+cash-receipt form on a different schema | create ticket sale + cash receipt | posts Customer/Supplier balances | TicketList, Customer, Supplier, Customers_Accounts, Suppliers_Accounts |
| Ticket_Tours_LandPrint | Ticket_Tours_Land/Print/Ticket_Tours_LandPrint.cs | "طباعة بيانات الحجز" — land-transport voucher print | print | opened from Ticket_Tours_Land_List | via Ticket_Tours_LandCLS |
| Ticket_Tours_Land | Ticket_Tours_Land/Ticket_Tours_Land.cs | "الحجز البرى" — multi-leg land transport booking | create/edit; posts balances for up to 3 suppliers | via Ticket_Tours_LandCLS | Tours_Ticketing_Land, Customer, Supplier (×3 legs), Customers_Accounts, Suppliers_Accounts |
| Ticket_Tours_Land_List | Ticket_Tours_Land/Ticket_Tours_Land_List.cs | "الحجز البرى" list | list/browse | opens Ticket_Tours_LandPrint | Tours_Ticketing_Land |
| AddBus | Tours_Bus/AddBus.cs | "بيانات الباص" — add/edit bus master record | CRUD bus (model, number, notes) | via BusListCLS | Tours_Bus_List |
| AddBusList | Tours_Bus/AddBusList.cs | "بيانات الباصات" — buses master list | list/browse buses | opens AddBus, BusList_FrmPrint | Tours_Bus_List |
| Bus_Tours | Tours_Bus/Bus_Tours.cs | "حجز الباص" — bus booking entry | create/edit; posts balances | — | Customer, Supplier, Customers_Accounts, Suppliers_Accounts |
| Bus_Tours_List | Tours_Bus/Bus_Tours_List.cs | "بيانات حجز الباصات" — bus bookings list | list/browse | opens BusToursPrint | bus booking table |
| BusList_FrmPrint | Tours_Bus/Print/BusList_FrmPrint.cs | "طباعة" — print bus master list | print | opened from AddBusList | Tours_Bus_List |
| BusToursPrint | Tours_Bus/PrintTours/BusToursPrint.cs | "طباعة بيانات الحجز" — bus booking voucher print | print | opened from Bus_Tours_List | bus booking table |
| AddOmraProgramList | Tours_Omra/AddOmraProgramList.cs | "برامج الحج / والعمرة" — Hajj/Umrah programs list | list/browse programs | opens AddProgramOmra, ProgramPrint | Omra program table |
| AddProgramOmra | Tours_Omra/AddProgramOmra.cs | "برنامج حج / عمرة" — add/edit a pilgrimage package | CRUD program | opened from AddOmraProgramList | Omra program table |
| Omra_Tours | Tours_Omra/Omra_Tours.cs | "حجز رحلة عمرة / حج" — Hajj/Umrah trip booking | create/edit booking; posts balances | — | Customer, Supplier, Customers_Accounts, Suppliers_Accounts |
| Omra_Tours_List | Tours_Omra/Omra_Tours_List.cs | "رحلات عمرة / حج" — trips list | list/browse | opens Tours_OmraPrint | Omra booking table |
| Tours_OmraPrint | Tours_Omra/Print/Tours_OmraPrint.cs | "طباعة بيانات الحجز" — trip voucher print | print | opened from Omra_Tours_List | Omra booking table |
| ProgramPrint | Tours_Omra/PrintProgram/ProgramPrint.cs | "طباعة بيانات الحجز" — program print | print | opened from AddOmraProgramList | Omra program table |

**Batch observations (ReservationSystemLIB):**
- Extremely strong **repeating template across 6 travel-product modules** (Visa/Tasherat, Hotel, Flight/Ticket_Tours, Land, Bus, Omra/Hajj): each has `[Booking Form] + [XxxCLS business-logic class] + [List form] + [Print form]`, with near-line-for-line identical SQL against parallel tables and identical Customer/Supplier balance-posting logic — a bug fixed in one module's `CLS` may not be fixed in the other five.
- Every booking module posts real financial side-effects directly (`UPDATE Customer SET Balance = Balance ± ...`) and writes ledger rows to `Customers_Accounts`/`Suppliers_Accounts` — this module owns accounting effects, not just reservation records.
- `SalesTicket_Tours_Old` and `ReverseTicket_Tours` coexist with `SalesTicket_Tours`/`_List`/`_List_Reverse` using a **different schema** (`TicketList` vs. per-module `Tours_*` tables) — evidence of a prior generation of the ticketing feature left in the tree alongside its replacement.
- All SQL is raw ADO.NET, string-concatenated — no parameterized queries observed anywhere in this project.
- Uses the `operations` DAL pervasively (16 files); **no `My_DbContext`/EF Core usage found anywhere** in this library.
- All-Arabic UI, RightToLeft forms. This is a class library (no `Program.cs`/startup form); consumed by a host EXE not covered by this scan.

### Alter_DataBase (top-level project)
**Startup form:** `MainFrm_DB`, launched via `Application.Run(new MainFrm_DB())` in `Alter_DataBase/Program.cs`, gated by an optional `LOGINFRM.ShowDialog()` login screen and a hardcoded date-range check (`2018-01-01`..`2027-06-06`).

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| ADDNEWUSER | Administration/ADDNEWUSER.cs | "إضافة مستخدم جديد" — add new user | create/edit/delete users & passwords | opened from MainFrm_DB | USER_DETAILES, SysConfigMenusPERMATIONS |
| frmadmin | Administration/frmadmin.cs | "تحديد الصلاحيه" — per-user, per-form permission grid | grant/revoke form access | opened from MAIN_FORM_IN_ADMINI | PERMATIONS, FORMS_DETAILES, USER_DETAILES |
| MAIN_FORM_IN_ADMINI | Administration/MAIN_FORM_IN_ADMINI.cs | "برنامج الأداره" — sub-MDI hub for admin module | menu hub | opened from MainFrm_DB; opens frmadmin | none directly |
| MenuFrmAdmin | Administration/MenuFrmAdmin.cs | "تحديد الصلاحيه" — menu-item-based permission assignment | grant/revoke per menu item | opened from MainFrm_DB and Set_ProjecType | FORMS_DETAILES, PERMATIONS |
| MenuFrmAdminServices | Administration/MenuFrmAdminServices.cs | "تحديد الصلاحيه" — services-based permission assignment | grant/revoke per "service" | opened from MainFrm_DB | FORMS_DETAILES, PERMATIONS, SysConfigServicesPERMATIONS, SysConfigMenusPERMATIONS |
| Bank_UserPermission | BankPermission/Bank_UserPermission.cs | "صلاحيات البنك / الخزينه" — bank/treasury permissions | assign which bank/cash accounts a user may access | opened from MainFrm_DB | USER_DETAILES, SysConfigBanksPERMATIONS |
| EmptyDB | EmptyDB/EmptyDB.cs | "Set New DB" — empties transactional data to prep a fresh DB | wipe transaction tables | opened from MainFrm_DB | Unknown |
| EncryptData | EmptyDB/EncryptData.cs | "Set New DB" (same caption, copy-pasted) — encrypts sensitive settings/data | encrypt config data | opened from MainFrm_DB | Unknown |
| GETIDFRM | HardWare/GETIDFRM.cs | "Hardware ID" — displays machine hardware ID for license-locking | read/display HW serial | opened from LOGINFRM | none |
| UniqFrm | HardWare/UniqFrm.cs | "Hardware ID" (duplicate caption) — similar hardware serial display/registration | read/display/register HW serial | opened from LOGINFRM | none |
| LOGINFRM | LOGINFRM.cs | Login gate before MainFrm_DB | authenticate; opens GETIDFRM/UniqFrm | shown before MainFrm_DB | USER_DETAILES (implied) |
| MainFrm_DB | MainFrm_DB.cs | MDI main menu/hub — **startup form** | ~30 menu handlers opening admin/config sub-forms | opens SetNewDB, EncryptData, Set_ProjecType_One, Bank_UserPermission, MenuFrmAdmin, MenuFrmAdminServices, UseXMLConn, ADDNEWUSER, MAIN_FORM_IN_ADMINI, EmptyDB, AlterDBForm, ExpireOnly, ExpireSystemInfo, SqlServerFullMigratorFrm, SysConfigFRM, SysConfigFRMNotAll, SysConfig_MenusFRM | none directly |
| ConfigFormSqlLite | SQLLiteConfig/ConfigFormSqlLite.cs | SQLite connection configuration | configure SQLite connection | **no internal caller found** — possibly dead/unused | none directly |
| SqlServerFullMigratorFrm | SqlServerFullMigrator/SqlServerFullMigratorFrm.cs | Full SQL Server DB schema/data migrator UI | clone tables, PKs/FKs/indexes/constraints/defaults to a destination DB | opened from MainFrm_DB | dynamic — reads sys.tables/sys.key_constraints/sys.foreign_keys/sys.indexes |
| AlterDBForm | SysConfig/AlterDBForm.cs | Bulk-alters Bank/Store/chart-of-accounts data | activate stores, recalc Submain_Account codes | opened from MainFrm_DB | Banks, Store, Submain_Account |
| ConfigForm | SysConfig/ConfigForm.cs | "Snowflake Configuration Settings" — configures a Snowflake cloud DB connection (unexpected — rest of system is SQL Server only) | configure Snowflake connection | **no internal caller found** — possibly dead/experimental | none directly |
| ExpireOnly | SysConfig/ExpireOnly.cs | Version-expiration date setter | set/encrypt expiration date | opened from MainFrm_DB | encrypted XML config |
| ExpireSystemInfo | SysConfig/ExpireSystemInfo.cs | Encrypted system/license info setter | set/encrypt system info | opened from MainFrm_DB | encrypted XML config |
| SetNewDB | SysConfig/SetNewDB.cs | "Set New DB" — configure/initialize a new database target | set new DB connection | opened from MainFrm_DB | none directly |
| Set_ProjecType | SysConfig/Set_ProjecType.cs | "Set Menu Data" — configure which project modules/menu items are active | set active modules | opened from MainFrm_DB; opens MenuFrmAdmin | SysConfigMenus (implied) |
| Set_ProjecType_One | SysConfig/Set_ProjecType_One.cs | Very large (3000+ line) all-in-one setup/migration script runner | ALTER TABLE, seed default data (Banks, Item, Price_Type_List, Customer_Supplier_Group), set encrypted system info | opened from MainFrm_DB | Invoices_Details(_Reverse), Bills_Details(_Reverse), SysConfig, SysConfigMenusPERMATIONS, Store, Banks, Item, Customer_Supplier_Group, Price_Type_List |
| SysConfigFRM | SysConfig/SysConfigFRM.cs | System config key/value editor | add/edit config keys | opened from MainFrm_DB; opens SysConfig_Add_Frm | SysConfig, SysConfigMenus |
| SysConfigFRMNotAll | SysConfig/SysConfigFRMNotAll.cs | Restricted/filtered variant of SysConfigFRM (same caption) | add/edit config keys (filtered set) | opened from MainFrm_DB; opens SysConfig_Add_Frm | SysConfig, SysConfigMenus |
| SysConfig_Add_Frm | SysConfig/SysConfig_Add_Frm.cs | Add-new-config-key dialog | insert new config key | opened from SysConfigFRM/SysConfigFRMNotAll/SysConfig_MenusFRM | SysConfig |
| SysConfig_MenusFRM | SysConfig/SysConfig_MenusFRM.cs | Menu-config key/value editor (same caption as SysConfigFRM) | add/edit menu config keys | opened from MainFrm_DB; opens SysConfig_Add_Frm | SysConfigMenus |
| UseXMLConn | UseXMLConn.cs | XML-based DB connection string configuration (encrypted) | encrypt/decrypt & save connection XML | opened from MainFrm_DB | encrypted XML config |

**Batch observations (Alter_DataBase):**
- A genuine **DB administration/maintenance/migration tool** — does live `CREATE TABLE`/`ALTER TABLE`/`DROP TABLE`, seeds default data, manages users/permissions (`USER_DETAILES`, `PERMATIONS`, `FORMS_DETAILES`), configures encrypted license/connection info.
- `SqlServerFullMigrator.cs`/`SqlServerFullMigratorFrm.cs` is **noticeably more modern code** than the rest of the codebase — uses C# interpolated strings and parameterized `SqlCommand` rather than string concatenation, suggesting it was added much later.
- Confirms the **permission/menu-loading architecture**: `FORMS_DETAILES` maps form names to IDs, `PERMATIONS`/`SysConfigMenusPERMATIONS`/`SysConfigServicesPERMATIONS` gate per-user access — strongly suggesting other modules' "orphan" forms elsewhere in the solution are dispatched dynamically by name from these tables.
- `ConfigForm` (Snowflake) and `ConfigFormSqlLite` (SQLite) have **no internal callers found** and reference DB technologies inconsistent with the rest of the SQL-Server-only codebase — likely dead/experimental code.
- Near-duplicate forms with overlapping purposes: `EmptyDB`/`EncryptData` (both "Set New DB"), `SysConfigFRM`/`SysConfigFRMNotAll`/`SysConfig_MenusFRM` (all "SysConfigFRM"), `GETIDFRM`/`UniqFrm` (both "Hardware ID").
- Same raw string-concatenated ADO.NET pattern as Stores/ReservationSystemLIB, except in the newer `SqlServerFullMigrator` code. Uses the `operations` DAL (15 files); no EF Core usage found.
- `SysConfig`/`SysConfigMenus` values are stored **encrypted**; the startup gate has a hardcoded expiry window (`2018-01-01`..`2027-06-06`) baked into `Program.cs`.

### CarsSystemLib
| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| ItemEstimationDataCar | EstimationValues/ItemEstimationDataCar.cs | "أصناف المخزن" — item-picker grid for a repair/service estimation | select items/prices for an estimation | opened from AddEstimationsValueFrm / AddEstimationsValueEditNameFrm | via `operations` DAL |
| Form1 | Invoice/Form1.cs | Empty unmodified VS template stub | none | none | none — dead code |
| Invoices_ListFrm | Invoice/Invoices_ListFrm.cs | "قائمة فواتير المبيعات" — sales invoices list | browse/filter workshop sales invoices; ctor overload `(int Invoice_Types)` | **no internal caller found in this library** | invoice tables (via DAL) |
| LoadingSendInvoiceReverseToPortalFrm | Invoice/LoadingSendInvoiceReverseToPortalFrm.cs | Progress dialog submitting a credit-note/reversed invoice to an external e-invoice tax portal | ctor `(long invoice_ListId_)`; submit to portal | opened from Car_ReverseInvoiceFrm | invoice tables (via DAL) |
| LoadingSendInvoiceToPortalFrm | Invoice/LoadingSendInvoiceToPortalFrm.cs | Same as above for original (non-reversed) invoices | ctor `(long invoice_ListId_)`; submit to portal | opened from Car_InvoiceFrm | invoice tables (via DAL) |
| CarsExaminationsPrntFrm | Print/CarsExaminations/CarsExaminationsPrntFrm.cs | Print layout for a car examination/inspection report (caption copy-pasted from work-order print) | render/print report | opened from AddExamination | via DAL |
| CustomerInvoicesWarshaFrm | Print/CustomerInvoicesWarsha/CustomerInvoicesWarshaFrm.cs | Print layout for customer workshop invoice summaries | render/print | opened from Invoices_ListCountFrmCar / Invoices_ListFrmCar / CarWorkOrderForItemsFrm | via DAL |
| EstimationValuesPrint | Print/EstimationValues/EstimationValuesPrint.cs | "طباعة المقايسة" — print the estimation | render/print estimation document | opened from AddEstimationsValueFrm / EditNameFrm | via DAL |
| Car_InvoicePrimaryPrint | Print/Invoices/Car_InvoicePrimaryPrint.cs | "Primary/preliminary" invoice print (caption copy-pasted, mislabeled) | render/print | opened from several out-of-scope forms | via DAL |
| Car_InvoicePrint | Print/Invoices/Car_InvoicePrint.cs | Standard invoice/work-order print | render/print | opened from Car_InvoiceFrm, Car_ReverseInvoiceFrm, ItemsDeliveredToProcessWorkOrder | via DAL |
| Car_InvoiceTAXPrint | Print/Invoices/Car_InvoiceTAXPrint.cs | Tax-invoice print variant | render/print | opened from Car_InvoiceFrm / Car_ReverseInvoiceFrm | via DAL |
| Masna3iatTotalFrm | Print/Masna3iatTotal/Masna3iatTotalFrm.cs | Prints summary totals of workshop labor/manufacturing charges across work orders for a period | render/print totals report | opened from Employess_Total_ItemsPrice | CarsWorkOrdersItemProcessedEmployees (implied) |
| WorkOrderPrintPrnt | Print/WorkOrder/WorkOrderPrintPrnt.cs | Print a workshop work order (caption copy-pasted) | render/print | opened from CarWorkOrdersList | via DAL |
| WorkOrderStatusRPT | Print/WorkOrderStatus/WorkOrderStatusRPT.cs | Print work-order status report | render/print | opened from CanceledWorkOrderListFrm / OpenedEndWorkDateWorkOrderListFrm / OpenedWorkOrderListFrm | via DAL |
| ItemsListCarsWithItems | Store/ItemsListCarsWithItems.cs | "بيانات الأصناف" — general items browse/lookup list | browse/select items | **no internal caller found in this library** | via DAL |
| ItemDataCar | WorkOrder/ItemDataCar.cs | "أصناف الفاتورة" — **heavily reused generic item-picker grid** | select items/quantities for an invoice or work order | opened from ≥8 different parent forms | via DAL |
| ItemProcessDataCar | WorkOrder/ItemProcessDataCar.cs | "أصناف الفاتورة" — item-picker for "processed" work-order items | ctor overloads for reversed/processed detail types | opened from several out-of-scope forms | via DAL |
| ItemsWorkOrderFrm | WorkOrder/ItemsWorkOrderFrm.cs | "أمر الشغل" — work-order items entry | ctor overload `(long CarsWorkOrdersId_)`; add/edit work-order items | **no internal caller found in this library** | via DAL |

**Batch observations (CarsSystemLib):**
- This is the only one of the four projects where the **majority of forms use the shared custom base class `Project_ControlLIB.MyForm`** (48 files) rather than bare `Form`. The 18 forms above (matching the literal `class X : Form` scope) are a minority, functioning mainly as reusable "item-picker"/print sub-dialogs invoked from the ~48 `MyForm`-based screens outside this scan's pattern.
- Extensive dead/duplicate code under `NewFolder1`/`1111` subfolders — near-identical copies of live forms; the `.csproj` explicitly `<Compile Remove>`s some of these, confirming they're deliberately excluded from the build.
- Several print-form window titles are **copy-pasted and never updated**: `Car_InvoicePrimaryPrint`, `Car_InvoiceTAXPrint`, `Car_InvoicePrint`, `CarsExaminationsPrntFrm`, `WorkOrderPrintPrnt` all carry the same literal caption regardless of actual purpose.
- `ItemDataCar` is a genuinely reused generic item-picker invoked from ≥8 different parent forms — a real shared-component pattern, in contrast to the print-title duplication above.
- Little to no raw inline SQL found directly in this project's in-scope form files; data access routes more consistently through the shared `operations` DAL (56 files) than in Stores/ReservationSystemLIB.
- Invoice submission integrates with an external "portal" — consistent with e-invoicing tax-authority integration (exact protocol not verified). No `My_DbContext`/EF Core usage found.

### Connection4It (connectionit)

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Account_ListFrmTree | AccountsTree/Account_TreeView/Account_ListFrmTree.cs | "شجرة الحسابات - حسابات الأستاذ" — chart-of-accounts tree picker | Displays account tree, lets caller pick a ledger account node | None visible | Unknown (uses shared account-tree control) |
| AlterADDNEWUSER | Administration/AlterADDNEWUSER.cs | "إضافة مستخدم جديد" — add/edit system user account | Insert/update/delete/select against USER_DETAILES directly (raw SQL) | Employees picker, SysConfigCLS | USER_DETAILES |
| AlterFrmAdmin | Administration/AlterFrmAdmin.cs | "تحديد الصلاحيه" — per-user form/menu permission assignment | Reads FORMS_DETAILES/PROJECT_DETAILES, inserts/deletes PERMATIONS rows | AlterADDNEWUSER, LOGINFRM | FORMS_DETAILES, PERMATIONS, PROJECT_DETAILES, USER_DETAILES |
| MAIN_FORM_IN_ADMINI | Administration/MAIN_FORM_IN_ADMINI.cs | "برنامج الأداره" — main shell/menu for the Administration sub-app | Launches admin sub-forms | AlterADDNEWUSER, AlterFrmAdmin, MenuFrmAdmin | None visible |
| MenuFrmAdmin | Administration/MenuFrmAdmin.cs | Menu-tree variant of permission assignment | Same PERMATIONS/FORMS_DETAILES CRUD via menu-tree UI | MenuFrmAdminCLS, SysConfigCLS | FORMS_DETAILES, PERMATIONS, PROJECT_DETAILES, USER_DETAILES |
| MenuFrmAdminServices | Administration/MenuFrmAdminServices.cs | Same as MenuFrmAdmin, scoped to a "Services" module | Same pattern | MenuFrmAdminCLS | FORMS_DETAILES, PERMATIONS, PROJECT_DETAILES, USER_DETAILES |
| MenuFrmAdmin_ | Administration/NewFolder1/MenuFrmAdmin_.cs | Near-duplicate of MenuFrmAdmin (lives under "NewFolder1") | Same pattern | MenuFrmAdminCLS, SysConfigCLS | FORMS_DETAILES, PERMATIONS, PROJECT_DETAILES, USER_DETAILES |
| BackupFrm | Alter_DataBase/BackupFrm.cs | DB backup/restore UI | Calls `Connection4It.DBAccess.BackupDatabase`/`RestoreDatabase`, uses `BackupDB_CLS` | None visible | sysdatabases, sysservers |
| ConnErrorTest | Alter_DataBase/ConnErrorTest.cs | "خطأ فى الإتصال بقاعدة البيانات" — shown on DB-connection failure | Retests connection via `operations`, opens DBConfig to fix settings | DBConfig | None (connectivity test only) |
| DBConfig | Alter_DataBase/DBConfig.cs | DB connection configuration editor (server/db/auth mode/uid/pwd) | Builds/persists encrypted `ConnConfigInfoXML`; **contains a hardcoded/commented-out real-looking connection string with server IP, sa user and a password — value not reproduced** | None visible | None (config only) |
| ExpireSystem | Alter_DataBase/ExpireSystem.cs | License/trial expiration configuration screen | Edits IsExpire/ExpireCount/dtExpire settings via ConnConfigInfoXML | None visible | None (config only) |
| FrmMainBackup | Alter_DataBase/FrmMainBackup.cs | "DataBaseBackup" — background/scheduled backup runner | Timer + background Thread driven `Create_Backup()` | None visible | Unknown |
| BackUpDBConfig | Alter_DataBase/NewFolder1/BackUpDBConfig.cs | Duplicate/older variant of DBConfig for backup connection settings ("NewFolder1" — dead-code candidate) | Same ConnConfigInfoXML pattern | None visible | None |
| DBConfigOld | Alter_DataBase/NewFolder1/DBConfigOld.cs | Explicitly named "Old" — superseded duplicate of DBConfig (dead-code candidate) | Same ConnConfigInfoXML pattern, uses `operations` | None visible | None |
| FrmStaticEditSysConfig | Alter_DataBase/StaticInvoiceNotesSysConfig.cs | "تغيير الإعدادات" — edits one system parameter (invoice-notes/customer-account visibility flag) | `AccountCommonDataLIB.Common.Save_SystemParametersDB(...)` | None visible | System parameters store |
| NewBank | Banks/NewBank.cs | "بيانات البنك" — add/edit bank master record | CRUD via `BanksClss` | None visible | Unknown (via BanksClss) |
| ChangeCompanyNameFrmcs | ChangeCompanyNameFrmcs.cs | Change company display name setting | Uses Employees picker | Employees | Unknown |
| CostCenterSearchFrm | CostCenter/CostCenterSearchFrm.cs | "بحث مراكز التكلفة" — cost-center lookup/picker dialog | Grid search over cost centers via BindingSource | None visible | Unknown |
| EditCustomerFirstBalance | FirstBalance_Financial_Period/EditCustomerFirstBalance.cs | "تعديل رصيد أول" — edit a customer's opening balance | Uses `FirstBalance_CLS`, `CustomersAccount` | None visible | Unknown (via FirstBalance_CLS) |
| EditSupplierFirstBalance | FirstBalance_Financial_Period/EditSupplierFirstBalance.cs | Same, for suppliers | Uses `FirstBalance_CLS`, `SuppliersAccount` | None visible | Unknown |
| FirstBalance_Financial_Period_FRM | FirstBalance_Financial_Period/FirstBalance_Financial_Period_FRM.cs | "أرصده أول المدة" — main opening-balances management grid | Uses `CurrencyFactorsCLS`, `FirstBalance_CLS` | EditCustomerFirstBalance/EditSupplierFirstBalance (inferred) | Unknown |
| AddGov_City_List | Government/Gov_City_List/AddGov_City_List.cs | "المحافظة" — add/edit governorate/city lookup record | Uses `Gov_City_ListCLS` | None visible | Journal_Codes (list form reads it) |
| Gov_City_ListFrm | Government/Gov_City_List/Gov_City_ListFrm.cs | "المحافظات" — list/manage governorates | BindingSource grid, opens Add form | AddGov_City_List | Journal_Codes |
| AddGovern_Area | Government/Govern_Area/AddGovern_Area.cs | "المنطقة" — add/edit area/district lookup | Uses `Govern_AreaCLS` | None visible | Journal_Codes |
| Govern_AreaListFrm | Government/Govern_Area/Govern_AreaListFrm.cs | "المناطق" — list/manage areas | BindingSource grid; commented-out currency-form template leftover | AddGovern_Area | Journal_Codes |
| AddGovern_List | Government/Govern_List/AddGovern_List.cs | "المحافظة" — add/edit governorate lookup | Uses `Govern_ListCLS` | None visible | Journal_Codes |
| Govern_ListFrm | Government/Govern_List/Govern_ListFrm.cs | "المحافظات" — list/manage governorates | BindingSource grid | AddGovern_List | Journal_Codes |
| Add_ALLItemFromTo_Store | ItemsList_Store_/Add_ALLItemFromTo_Store.cs | "إضافة كل الأصناف فى مخزن" — bulk-add all items into a store | Raw SQL SELECT/INSERT against Item, Unit_item, Unit_item2, store | ItemList | Item, Unit_item, Unit_item2, store |
| Add_ItemToStore | ItemsList_Store_/Add_ItemToStore.cs | "إضافة صنف للمخزن" — add a single item to a store | Raw SQL insert/update/delete across purchasing/unit tables | None visible | Detailes_Purshing, Order_Purshing, Unit_item, Item |
| ItemsBarCode (class in file `ItemsBarCode.cs`) | ItemsList_Store_/BarCode/ItemsBarCode.cs | Barcode printing UI **(filename/class-name swap — see observations)** | Uses `operations` | None visible | Item, Item_BarCodeSetting (inferred) |
| PrintBarCodeFrm | ItemsList_Store_/BarCode/PrintBarCodeFrm.cs | Barcode print layout/preview form | Reads `Item_Image_BarCode`, uses `operations` | None visible | Item_Image_BarCode |
| ItemsBarCode (class actually named `PrintItemsBarCode`, file `PrintItemsBarCode.cs`) | ItemsList_Store_/BarCode/PrintItemsBarCode.cs | "باركود الأصناف" — item barcode management/config **(filename/class swap — see observations)** | INSERT/DELETE `Item_BarCodeSetting`, `Item_Image_BarCode`; reads image files; font dialog | None visible | Item, Item_BarCodeSetting, Item_Image_BarCode |
| Store_Dept_Items_List | ItemsList_Store_/Dept_Item/Dept_Items_List.cs | "قائمة الأصناف" — items list scoped to a department/store | Same raw-SQL purchasing/unit CRUD pattern as Add_ItemToStore | None visible | Detailes_Purshing, Order_Purshing, Unit_item, Item |
| frmImagesStore | ItemsList_Store_/Item_ImagesStore/frmImagesStore.cs | "صور الأصناف والمستندات" — item images/documents gallery | Update/delete `Item_ImagesStore`, print preview | frmNewImage | Item_ImagesStore |
| frmNewImage | ItemsList_Store_/Item_ImagesStore/frmNewImage.cs | "New Image" — add a new item image | Inserts binary file into `Item_ImagesStore` | None visible | Item_ImagesStore |
| frmNewImageLogo | ItemsList_Store_/Item_ImagesStore/frmNewImageLogo.cs | Add a company/branch logo image | Inserts binary file into `Account_BranchesimagesLogo` | None visible | Account_BranchesimagesLogo |
| Item_List_Search | ItemsList_Store_/Item_List_Search.cs | "بحث أصناف" — item search/lookup dialog | Uses `operations`, ListView results | None visible | Unknown |
| Item_MainData | ItemsList_Store_/Item_MainData.cs | "بيانات الصنف" — main item master data editor | Insert/update Item, Unit_item, purchasing detail tables | None visible | Item, Store, Unit_item, Detailes_Purshing, Order_Purshing |
| Item_ProductionData | ItemsList_Store_/Item_ProductionData.cs | "بيانات إنتاج الإصناف" — item production data editor | `Update Data`-style SQL | Item_List | Unknown |
| Item_MainData_ | ItemsList_Store_/New folder/Item_MainData_.cs | Near-duplicate of Item_MainData ("New folder" — dead-code candidate) | Same title | Unknown | Unknown |
| AddSubItem | ItemsList_Store_/SubItems/AddSubItem.cs | "أصناف اللحاف - الكوفرته" — add a sub-item variant (bedding/quilt) | Uses `SubItemsCLS` | None visible | Unknown |
| Item_ProductionData (dup class name) | ItemsList_Store_/SubItems/Item_ProductionData.cs | Same title as the other Item_ProductionData, different ctor — likely a fork | `Update Data`-style SQL | Item_List | Unknown |
| SubItemListFrm | ItemsList_Store_/SubItems/SubItemListFrm.cs | "إصناف اللحاف" — list/manage sub-items | BindingSource grid; same commented-out template leftover | AddSubItem | Journal_Codes |
| Form1 | Reporting/Form1.cs | Empty/unused scaffold form | None (stub) | None | None |
| ItemPurchaseQntyFrm | Reporting/ItemPurchaseQntyFrm.cs | "أرصدة الأصناف بسعر المورد" — item balances report priced at supplier cost | Builds RDLC report via `Items_CLS` | None visible | Unknown |
| ItemQntyFrm | Reporting/ItemQntyFrm.cs | "أرصدة الأصناف بسعر البيع" — item balances report priced at sale price | RDLC report via `Items_CLS` | None visible | Unknown |
| ItemQntyFrmOLD | Reporting/ItemQntyFrmOLD.cs | Confirmed older/smaller version of ItemQntyFrm — dead-code candidate | RDLC report (older adapter-based) | None visible | Unknown |
| Item_QntyPurchasing | Reporting/Item_QntyPurchasing.cs | "أرصدة أصناف المشتريات" — purchasing-side item balances report | RDLC report | None visible | Unknown |
| ItemsSearch | SearchFRM/ItemsSearch.cs | "بحث" — generic item search dialog | Grid search over Item table | None visible | Item |
| ItemsSearchAny | SearchFRM/ItemsSearchAny.cs | "بحث" — near-duplicate of ItemsSearch with extra ColumnId param | Generic column search | None visible | Unknown |
| ItemsSearchAnyEn | SearchFRM/ItemsSearchAnyEn.cs | English-labeled variant of ItemsSearchAny | Generic column search | None visible | Unknown |
| ProjUI | Styles/ProjUI.cs | "تغيير وجهة البرنامج" — application skin/theme switcher | Uses `SkinSoft.VisualStyler`/`Sunisoft.IrisSkin`, `ConnConfigInfoXML` | None visible | None |
| ProjUI_ | Styles/Styles/ProjUI_.cs | Near-duplicate of ProjUI, largely stubbed — dead-code candidate | Same intent | None visible | None |
| AddGroup | SupplierAndCustomer/Groups/AddGroup.cs | "المجموعات" — add/edit a customer/supplier group | Uses `GroupsCLS` | None visible | Unknown |
| GroupsListFrm | SupplierAndCustomer/Groups/GroupsListFrm.cs | "مجموعات العملاء" — list customer groups | BindingSource grid; same template leftover | AddGroup | Unknown |
| SalesMan_Info | SupplierAndCustomer/SalesMan_Info.cs | "تكويد مندوبين المبيعات" — salesman master data coding | Raw SQL against `employee`/`Sub_Account` | None visible | employee, Sub_Account |
| OrderDeliveryTypesList_Frm | TaxInfo/OrderDeliveryTypesListFrm.cs | Order delivery-type / default-fee table (restaurant-style dine-in/delivery/takeaway) | Grid via `Lookup_OrderDeliveryTypesCLs` | None visible | Unknown |
| TaxListFrm | TaxInfo/TaxListFrm.cs | "بيانات المعدلات الضريبية" — tax rate table management | CRUD via internal logic | None visible | Unknown |
| Form1 | Test/Form1.cs | Empty scaffold/test form — dead code | None (stub) | None | None |
| AboutUs | UsrCntrl/AboutUs.cs | "عن الشركة" — About/company info dialog | Static display | None visible | None |
| AddDrugs | UsrCntrl/Clinic/AddDrugs.cs | Title mislabeled "العملات الجديده" (copy-paste from a currency form); add drug/medicine record | Uses `DrugsListCLS` | None visible | Unknown |
| DrugsListFrm | UsrCntrl/Clinic/DrugsListFrm.cs | List/manage drugs (Clinic module reusing the shared lib) | BindingSource grid; same template leftover | AddDrugs | Unknown |
| Patient_Info | UsrCntrl/Clinic/Patient_Info.cs | Clinic patient master data (unusual vertical-specific module inside a generic shared library) | Raw SQL insert/update against Customer, Customers_Accounts, Financial_Period_Begining_Balance, Submain_Account | Account_ListFrmTree | Customer, Customers_Accounts, Financial_Period_Begining_Balance, Submain_Account |
| Form1 | UsrCntrl/Print/Form1.cs | Print test/config harness form (wires up PrintRosheta + PrintReportDirectly) | Calls `PrintRosheta.Show_PrintInvoice()`, `PrintReportDirectly.PrintMyReport_A5Test` | PrintRosheta, PrintReportDirectly | None |
| PrintRosheta | UsrCntrl/Print/PrintRosheta.cs | "الطباعة" — clinic prescription ("Rosheta") print form | Uses `PatientClinicListCLS`, ReportViewer | None visible | Unknown |
| PrintRoshetaNoName | UsrCntrl/Print/PrintRoshetaNoName.cs | Variant of PrintRosheta without patient name printed | Uses `PatientClinicListCLS` | None visible | Unknown |
| Template_Frm | UsrCntrl/Template_Frm.cs | Blank starter/template form for cloning new forms from (also used elsewhere as a real base class — see ProductionsRates) | Empty scaffold | None visible | None |

**Batch observations (Connection4It):**
- Most business forms use Arabic captions while class/file names are English/transliterated; several files still carry stub English titles ("Form1", "BackupFrm") indicating unfinished localization or template leftovers.
- **Character-encoding hazard**: source files are inconsistently UTF-8, Windows-1256, or plain ASCII — the same folder can mix encodings (e.g. `Alter_DataBase/NewFolder1/*.cs` vs. sibling `Alter_DataBase/BackupFrm.cs`).
- **Filename/class-name swap bug candidate**: `BarCode/ItemsBarCode.cs` actually declares `class PrintItemsBarCode`, and `BarCode/PrintItemsBarCode.cs` declares `class ItemsBarCode` — the two files' names are swapped relative to their class names. Observation only, not fixed.
- **Copy-paste template pattern**: many `XxxListFrm` list/grid forms (`Govern_AreaListFrm`, `GroupsListFrm`, `DrugsListFrm`, `SubItemListFrm`, and Account_Common_LIB equivalents) contain an identical commented-out line referencing a currency-factor template class, proving they were all cloned from a shared list-form template without cleanup.
- **No shared custom base Form** among the 67 forms matched by the plain `: Form` pattern here — but a separate set of ~11 forms elsewhere in this same project (`EmployeeLisFrm`, `Customer_Info`, `Supplier_Info`, `CountryLisFrm`, etc.) inherit `Project_ControlLIB.MyForm` instead, showing a partial migration to a shared base form.
- Direct raw ADO.NET SQL string-building (via `operations`'s `insert_me`/`update_me`/`return_dataset`/`return_DataTable`, which accept whole caller-built SQL strings) is the dominant DB-access pattern; only a minority of files reference EF Core (`db.`) directly.
- Table names span administration (`USER_DETAILES`, `PERMATIONS`, `FORMS_DETAILES`, `PROJECT_DETAILES`), inventory (`Item`, `Unit_item`, `Store`, `Detailes_Purshing`, `Order_Purshing`, `Item_BarCodeSetting`), lookup tables (`Journal_Codes`), and even accounting tables (`Customer`, `Customers_Accounts`, `Submain_Account`) touched directly from the Clinic `Patient_Info` form — Connection4It bleeds into accounting-table writes directly rather than delegating to Account_Common_LIB.
- **Dead/duplicate code candidates**: `Administration/NewFolder1/MenuFrmAdmin_.cs`, `Alter_DataBase/NewFolder1/BackUpDBConfig.cs`/`DBConfigOld.cs`, `ItemsList_Store_/New folder/Item_MainData_.cs`, `Reporting/ItemQntyFrmOLD.cs`, `Styles/Styles/ProjUI_.cs`, `Test/Form1.cs`, `SearchFRM/ItemsSearchAny.cs`/`ItemsSearchAnyEn.cs`.
- **Cross-domain surprise**: a full clinic/pharmacy module lives inside the generic "Connection4It" shared connectivity library and writes directly to core accounting tables — architecturally out of place.
- **Sensitive value found, not reproduced**: `Alter_DataBase/DBConfig.cs` has a commented-out connection-string literal containing what appears to be a real server IP, `sa` username and password. This should be scrubbed/rotated by the team. Hardcoded `sa`/blank-password connection strings also appear in `Alter_DataBase/BackupDB_CLS.cs`/`BackupFrm.cs` for master-database backup/restore operations.

### Account_Common_LIB

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Account_ListFrmTree | Account_Common/Account_TreeView/Account_ListFrmTree.cs | Chart-of-accounts tree picker (same purpose as connectionit's copy, separate implementation) | Tree display/selection | None visible | Unknown |
| BanksFrm | Banks/BanksFrm.cs | "حساب البنوك" — bank accounts list/management | ListView over `BanksClss`; reads Financial_Period_Begining_Balance | NewBank, BanksFirstBalance | Financial_Period_Begining_Balance |
| DetectAccount_Branches_FRM | Banks/DetectAccount_Branches_FRM.cs | "بيانات الصندوق" — detect/select a cash-box branch | Branch-list combo selection | None visible | Unknown |
| MoneyBanksFrm | Banks/MoneyBanksFrm.cs | "بيانات الخزن" — cash boxes list, older raw-SQL implementation | ListView over `BanksClss` | None visible | Financial_Period_Begining_Balance |
| MoneyBanksFrm1 | Banks/MoneyBanksFrm1.cs | Near-duplicate of MoneyBanksFrm, rewritten to use EF entities — a newer parallel implementation | Same list purpose plus EF model usage | None visible | Financial_Period_Begining_Balance |
| NewBank | Banks/NewBank.cs | Add/edit bank record (duplicate of connectionit's NewBank, same title) | CRUD via `BanksClss` | None visible | Unknown |
| NewSandouk | Banks/NewSandouk.cs | Add/edit cash box/vault record | CRUD via `BanksClss` | None visible | Unknown |
| SandoukBranch | Banks/SandoukBranch.cs | Company cash-box branches list | ListView over `BanksClss` | DetectAccount_Branches_FRM | Unknown |
| Capital_Money | Capital_Money/Capital_Money.cs | "شركاء رأس المال" — capital/partners share-capital main screen | Unknown ops (no SQL hint found) | None visible | Unknown |
| CapitalFRM (file Capital_paulled.cs) | Capital_Money/Capital_paulled.cs | "سحب نقديه لشريك" — cash withdrawal by a partner | Reads/writes Capital_Money | None visible | Capital_Money |
| Capital_frm (file capitalfrm.cs) | Capital_Money/capitalfrm.cs | "قائمة الشركاء" — partners list | Uses `Capital_class`, `Machin_list_class`, `job_List` | None visible | Unknown |
| frm_View_detaile | Capital_Money/frm_View_detaile.cs | "تفاصيل المسحوبات" — detail view of partner withdrawals | Reads Capital_Money, capital_pulled | None visible | Capital_Money, capital_pulled |
| Add_NewCostCenter | Cost_CenterTree/Add_NewCostCenter.cs | "إضافة مركز تكلفه جديد" — add a new cost center node | Tree-node add via `CostCenterCLS`, `Sub_Account` | None visible | Unknown |
| CostCenterForAccounts_FRM | Cost_CenterTree/CostCenterForAccounts_FRM.cs | Assign a single cost center to an account | Uses `CostCenterCLS`, tree events | None visible | Unknown |
| Cost_Center_TreeFrm | Cost_CenterTree/Cost_Center_TreeFrm.cs | Cost-center tree directory/browser | `CostCenterTreeViewUSCTRL1.SetTreeFill()` | None visible | Cost_Center_Tree (read via control) |
| Form1 | Cost_CenterTree/Form1.cs | Empty/unused scaffold form — dead code | None (stub) | None | None |
| MultiCostCenterForAccounts_FRM | Cost_CenterTree/MultiCostCenterForAccounts_FRM.cs | Assign multiple cost centers to an account | Uses `CostCenterCLS` | None visible | Unknown |
| AddCurrency | CurrencyFactors/AddCurrency.cs | Add a new currency | Uses `CurrencyFactorsCLS` | None visible | Unknown |
| AddCurrencyFactor | CurrencyFactors/AddCurrencyFactor.cs | Add a currency exchange-rate factor (the template many list forms were copy-pasted from) | Uses `CurrencyFactorsCLS` | None visible | Unknown |
| CurrencyListFrm | CurrencyFactors/CurrencyListFrm.cs | Currencies list | BindingSource grid | AddCurrency, AddCurrencyFactor | Unknown |
| Currency_Factors | CurrencyFactors/Currency_Factors.cs | Currency exchange-rate history list | BindingSource grid | AddCurrencyFactor | Currency_Factor |
| Financial_PeriodFrm | Financial_Period/Financial_PeriodFrm.cs | Financial period open/close management | Uses `Financial_PeriodCLS`, `DayRegistrationCLS`; **P&L navigation code is entirely commented out** (disabled feature) | None active | Unknown |
| BanksFirstBalance | FirstBalance_Financial_Period/BanksFirstBalance.cs | Edit opening balances for banks/cash boxes | Uses `BanksClss` | None visible | Unknown |
| EditSupplierFirstBalance | FirstBalance_Financial_Period/EditSupplierFirstBalance.cs | Edit a supplier's opening balance (separate implementation from connectionit's same-named file) | Uses `FirstBalance_CLS`, `Suppliers_AccountsCls` | None visible | Unknown |
| FirstBalanceMezan_FirstTime_RPT | FirstBalance_Financial_Period/Print/FirstBalanceMezan_FirstTime_RPT.cs | Summarized trial-balance report for the opening period | ReportViewer-based RDLC report | None visible | Unknown |
| Add_Account_Category | Fixed_Assets/Add_Account_Category.cs | Add a new account category (for fixed assets) | Uses `Add_Account_CodesCLS`, `Submain_AccountCLS`, tree node | None visible | Unknown |
| Add_NewFixedAsset | Fixed_Assets/Add_NewFixedAsset.cs | Add a new fixed asset record | Tree-node based category assignment | None visible | Unknown |
| FinancialPeriodEhlak | Fixed_Assets/FinancialPeriodEhlak.cs | Depreciation ("Ehlak") run for a financial period | Uses `Assets_EhlakCLS` | None visible | Unknown |
| FixesAsset_TreeFrm | Fixed_Assets/FixesAsset_TreeFrm.cs | Fixed-assets tree directory | Uses `Submain_AccountCLS`, `Sub_Account` | None visible | Cost_Center_Tree (shared tree control) |
| frmNewImage | Fixed_Assets/frmNewImage.cs | Attach an image to a fixed asset | Inserts binary file into `FixedAssetData_ImagesStore` | None visible | FixedAssetData_ImagesStore |
| Assets_EhlakFrm_ | Fixed_Assets/New folder/Assets_EhlakFrm_.cs | Near-duplicate of the depreciation form ("New folder" — dead-code candidate) | Unknown | Unknown | Unknown |
| EhlakFixesAssetFrm | Fixed_Assets/New folder/EhlakFixesAssetFrm.cs | Near-duplicate/older variant, mismatched title (copy-paste leftover) — dead-code candidate | Unknown | Unknown | Unknown |
| Form1 | Form1.cs | Empty/unused scaffold form at project root — dead code | None (stub) | None | None |
| AddItemOnFinancialOrder | Money_Financial_Notice/Items/AddItemOnFinancialOrder.cs | Add an item line to a financial adjustment notice | Item selection | None visible | Unknown |
| Money_Financial_NoticePrint | Money_Financial_Notice/Items/Print/Money_Financial_NoticePrint.cs | Print the financial notice/voucher | ReportViewer report; reads Currency, Customer, Submain_Account, Supplier | Order_Receipt_Cash (report param) | Money_Financial_Notice_Items_Details, Currency, Customer, Submain_Account, Supplier |
| Taswya_MoneyItem | Money_Financial_Notice/Items/Taswya_MoneyItem.cs | Financial adjustment (debit/credit) notice, single item | Uses `Money_Financial_Notice_ItemsCLS` | None visible | Unknown |
| Taswya_MoneyMulti_Item | Money_Financial_Notice/Items/Taswya_MoneyMulti_Item.cs | Same purpose, multi-item variant | Uses `Money_Financial_Notice_ItemsCLS` | AddItemOnFinancialOrder, Money_Financial_NoticePrint | Unknown |
| Taswya_Money_ReviewItem | Money_Financial_Notice/Items/Taswya_Money_ReviewItem.cs | Review/audit list of adjustment notices (single-item) | ListView; opens Taswya_MoneyItem for edit | Taswya_MoneyItem | Unknown |
| Taswya_Money_Review_Multi_Item | Money_Financial_Notice/Items/Taswya_Money_Review_Multi_Item.cs | Same review purpose, multi-item variant | Opens Taswya_MoneyMulti_Item for edit | Taswya_MoneyMulti_Item | Unknown |
| Taswya_Money | Money_Financial_Notice/Taswya_Money.cs | Header-level financial adjustment notice | Uses `Money_Financial_NoticeCLS` | None visible | Unknown |
| Taswya_Money_Review | Money_Financial_Notice/Taswya_Money_Review.cs | Review list for header-level notices | ListView over `Money_Financial_NoticeCLS` | None visible | Unknown |
| Cash_DataFrm | Money_Transaction/Cash_Money/1Order_Receipt_CashFrm/OrderCash_Money/Cash_DataFrm.cs | Cash-amount entry sub-form | Uses `Check_Data`, `Money_Transaction_BoxOrderReportCLS` | None visible | Unknown |
| Order_Receipt_CashFrm | Money_Transaction/Cash_Money/1Order_Receipt_CashFrm/OrderCash_Money/Order_Receipt_CashFrm.cs | Cash disbursement/receipt voucher — oldest ("1…") version in a numbered folder series | Uses `Account_ListFrm`, `Cash_DataFrm`, `Check_Data`, prints via `OrderMoneyPrint_` | Account_ListFrm, Cash_DataFrm, OrderMoneyPrint_ | Unknown |
| New_OrderMoney_Transaction | Money_Transaction/Cash_Money/5Order_Receipt_CashFrm/New_OrderMoney_Transaction.cs | Cash receipt entry — "5…" version in the same numbered-folder history | Uses `operations`, `Money_Transaction_BoxOrderReportCLS` | OrderMoneyPrint, PrintOrder_Receipt_CashFrm | Unknown |
| SalesTicket_OrderMoney_Transaction | Money_Transaction/Cash_Money/6Order_Receipt_CashFrm/SalesTicket_OrderMoney/SalesTicket_OrderMoney_Transaction.cs | Ticket sales + cash receipt (tours/travel-agency use case), "6…" version | Raw SQL insert `TicketList`, `Customers_Accounts`, `Suppliers_Accounts`; update Customer/Supplier | OrderMoneyPrint | TicketList, Customer, Customers_Accounts, Supplier, Suppliers_Accounts |
| SalesTicket_OrderMoney_TransactionTours | Money_Transaction/Cash_Money/6Order_Receipt_CashFrm/SalesTicket_OrderMoney/SalesTicket_OrderMoney_TransactionTours.cs | Near-duplicate of the above, "Tours" variant | Same pattern | OrderMoneyPrint | Same tables |
| DaysMoney_Transaction_Box | Money_Transaction/Cash_Money/8Sandouk_Transaction/DaysMoney_Transaction_Box.cs | "يومية الصندوق" — daily cash-box journal/log | Uses `Capital_class`, `Money_Transaction_BoxOrderReportCLS` | None visible | Unknown |
| Money_Transaction_BoxFrm | Money_Transaction/Cash_Money/8Sandouk_Transaction/Money_Transaction_BoxFrm.cs | "حركة الخزينة" — cash-box transaction movement screen | Uses `Capital_class`, `Money_Transaction_BoxOrderReportCLS` | None visible | Unknown |
| BanksMoneyTransaction | Money_Transaction/Cash_Money/90Sandouk_Transaction/BanksMoneyTransaction.cs | Daily cash-box statement (bank side, "90…" version) | Uses `Money_Transaction_BoxOrderReportCLS`; opens New_OrderMoney_SarfNew, prints via OrderMoneyPrint | MoneyTransactionDayPrint, New_OrderMoney_SarfNew, OrderMoneyPrint, Order_MoneyToShow | Unknown |
| OrderMoney_HISPrint | Money_Transaction/Cash_Money/Hospital/OrderMoney_HISPrint.cs | Voucher print form for a Hospital Information System (HIS) integration/vertical | ReportViewer report | None visible | Unknown |
| New_OrderMoney_SarfCurrencyNew | Money_Transaction/Cash_MoneyNew/New_OrderMoney_SarfCurrencyNew.cs | Cash disbursement with currency conversion, part of a "New" rewrite generation | Uses `AccountEntry_CLS`, `CurrencyFactorsCLS`, `operations` | Add_Account_To_OrderMoney, New_OrderMoney_SarfNew, OrderMoneyPrint | Unknown |
| New_OrderMoney_SarfHospital | Money_Transaction/Cash_MoneyNew/New_OrderMoney_SarfHospital.cs | Same disbursement flow, Hospital-vertical variant | Same DAL classes | Add_Account_To_OrderMoney, New_OrderMoney_SarfNew, OrderMoneyPrint | Unknown |
| New_OrderMoney_SarfNew | Money_Transaction/Cash_MoneyNew/New_OrderMoney_SarfNew.cs | Core/base cash-disbursement form that other Sarf* variants call into and clone from | `GetBalanceAfterBeforCustomer(Supplier)` balance calculators, `operations` | Add_Account_To_OrderMoney, OrderMoneyPrint | Unknown |
| New_OrderMoney_SarfNewNew | Money_Transaction/Cash_MoneyNew/New_OrderMoney_SarfNewNew.cs | Largest variant (2287 lines) — "yet newer" rewrite, adds CustomerCLS/SupplierCLS and Money_Transaction_Box integration | Same balance-calculator pattern plus extra account entry logic | Add_Account_To_OrderMoney, Money_Transaction_Box, New_OrderMoney_SarfNew, OrderMoneyPrint, Order_Money | Unknown |
| OrderMoney_Transaction | Money_Transaction/Cash_MoneyNew/OrderMoney_Transaction.cs | Issue a cash-box voucher (car-rental "CarsWorkOrder" integration visible) | Uses `Money_Transaction_BoxCLS`, `Money_Transaction_BoxVM`, `operations` | OrderMoneyPrint, Order_Receipt_Cash, Order_Receipt_CashFrm | Unknown |
| Cash_Money_ListFromAccounts | Money_Transaction/MoneyTransactionReport/Cash_Money_ListFromAccounts.cs | Cash movement report filtered by account | BindingSource grid | New_OrderMoney_SarfNew, OrderMoneyPrint, Order_MoneyToShow, Order_Receipt_CashFrm | Unknown |
| MoneyTransactionDayPrint | Money_Transaction/PrintReport/MoneyTransactionDayPrint.cs | Printable daily cash-box statement | ReportViewer report via `Money_Transaction_BoxOrderReportCLS` | None visible | Unknown |
| Money_Transaction_BoxPeriodPintFrm | Money_Transaction/PrintReport/Money_Transaction_BoxPeriodPintFrm.cs | Printable cash-box statement for a date range/period | ReportViewer report | None visible | Unknown |
| OrderMoneyPrint | Money_Transaction/PrintReport/PrintOrderMoney/OrderMoneyPrint.cs | Generic voucher print form, called by nearly every Sarf/Order_Receipt form as the shared print target | ReportViewer report via `Order_Receipt_Cash` | None visible | Unknown |
| OrderMoneyPrint_ | Money_Transaction/PrintReport/PrintOrderMoney/OrderMoneyPrint_.cs | Near-duplicate of OrderMoneyPrint, used specifically by the oldest Order_Receipt_CashFrm ("1…" folder) | ReportViewer report | None visible | Unknown |

**Batch observations (Account_Common_LIB):**
- **Numbered "version history" folders**: `Money_Transaction/Cash_Money/` contains folders literally named `1Order_Receipt_CashFrm` through `90Sandouk_Transaction`, plus `Cash_MoneyNew/` as a further rewrite generation — clear evidence of repeated in-place forking/rewriting of the same cash-voucher feature over time rather than deleting superseded versions (some numbered folders are empty/abandoned).
- Within that lineage, `New_OrderMoney_SarfNew`, `New_OrderMoney_SarfNewNew`, `New_OrderMoney_SarfHospital`, `New_OrderMoney_SarfCurrencyNew` are four large (1400–2300 line) near-duplicate forms implementing essentially the same "cash disbursement" screen for different verticals/currencies — a strong refactor/dedup candidate, not touched here.
- `OrderMoneyPrint` vs `OrderMoneyPrint_` and `MoneyBanksFrm` vs `MoneyBanksFrm1` are additional near-duplicate old/new pairs.
- **Confirmed shared custom base Form class**: `Project_ControlLIB.MyForm` is used by ~23 other forms in this project (e.g. `Account_Tree_CodesFrm`, `NewMainAccount`, `Money_Between_BoxTar7eelFrm`) — architecturally significant but excluded from the table above since they don't match the plain `: Form` pattern.
- DAL usage: relies mainly on `AccountCommonDataLIB.operations` (raw ADO.NET plus an EF Core `My_DbContext` field); direct EF LINQ usage found in only 2 files here (much less than Connection4It's 23) — this project leans slightly more on the older raw-SQL/DataTable pattern.
- **Cross-vertical surprise**: a Hospital Information System (HIS) print form and Hospital-specific disbursement form live inside the general accounting library, plus a "Tours"/travel-ticket sales variant and car-rental references — this "common" library has accreted client/vertical-specific one-off forms rather than staying generic.
- **Dead/duplicate code candidates**: `Fixed_Assets/New folder/Assets_EhlakFrm_.cs`/`EhlakFixesAssetFrm.cs`, `Form1.cs` at project root and in `Cost_CenterTree/` (both confirmed blank scaffolds), `Banks/MoneyBanksFrm1.cs`, `Money_Transaction/PrintReport/PrintOrderMoney/OrderMoneyPrint_.cs`.
- `Financial_PeriodFrm.cs` has its entire Profit & Loss statement navigation commented out — a disabled/incomplete feature left in place.
- No hardcoded credentials found directly in this project's form files (connection/config forms live in Connection4It's `Alter_DataBase`, not here).

**Cross-cutting note (Connection4It + Account_Common_LIB):** both projects' DAL root is confirmed as `AccountModelsLib.operations` (raw `SqlConnection`/`SqlCommand`, caller-built SQL strings, `return_dataset`/`return_DataTable`/`insert_me`/`update_me`/`delete_me`) with `AccountCommonDataLIB.operations : AccountModelsLib.operations` adding the EF Core `My_DbContext` (`db` field) for a subset of newer code paths — exactly the mixed ADO.NET + EF Core pattern described under *Database Access*.

### Accounts
**Likely main/MDI form(s):** Two MDI containers exist. `MainFrmPOS` (`AccountMainFrm\MainFrmPOS.cs`, directly derives `Form`, `IsMdiContainer = true`) is the Point-of-Sale shell — title "Point Of Sale منصة الحسابات - منظومة الحسابات الشاملة" — exposing ~70 menu handlers that open nearly every POS/customer/supplier/report screen. The **dominant hub for the whole library**, however, is `MainFrm_AccountUpdate` (`AccountMainFrm\MainFrm_AccountUpdate.cs`, 8,980 lines, `IsMdiContainer = true`, physically lives in the `Advanced_Account` project — see that project's entry above) — it is out of the strict 125-file scope below because it derives from `Project_ControlLIB.MyForm` rather than `Form` directly, but it contains 718 `new <Form>(...)` navigation calls and is clearly the general-ledger/accounting main menu, opening the large majority of the forms in this table.

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| MainFrmPOS | AccountMainFrm/MainFrmPOS.cs | POS/restaurant main menu shell, MDI container | Hosts ~70 menu actions: customers/suppliers, items, invoices, tables, tax, DB backup, password change | Opens DriversSettlementFrm, SalesReportsTotal__POS, SalesReportsurchasPrice (+more via menu) | None directly; delegates to opened forms/helper classes |
| TakseePRNT | AddAcounts/TakseePRNT.cs | Print/preview of customer installment (Takseet) schedule | Renders via ReportViewer from data passed by caller | Opened by Takseet_Details.cs, Takseet_List.cs | None visible (data passed in) |
| Takseet_Details | AddAcounts/Takseet_Details.cs | Customer installment details/cancel-payment screen | Add/Cancel/Delete installment payment via `TakseetCLS`, `Accounts_Entry_POST_CLS`; Commit/Rollback on `operations` | Opens TakseePRNT | Journal posting delegated to Accounts_Entry_POST_CLS |
| CalendarAppointmentsFrm | BeautyCenter/CalendarAppointmentsFrm.cs | Beauty-center appointment calendar | Refresh, view appointment details | Opened by MainFrm_AccountUpdate | EF: `opp.db.Appointments` |
| FrmCustomerPackageDetails | BeautyCenter/CustomerPackages/FrmCustomerPackageDetails.cs | Detail/edit view of a customer's purchased service package | Add service to package | Opened by FrmCustomerPackagesList.cs | Unknown |
| FrmAddEditProducts | BeautyCenter/FrmAddEditProducts.cs | Product/item picker for beauty-center packages | Confirm/close selection via `ItemCLS` | Unknown | Delegated to ItemCLS |
| FrmPackageEditMulti | BeautyCenter/Package/FrmPackageEditMulti.cs | Edit a beauty-center service/product package (multi-item) | Save/Cancel, toggle Products/Services, uses `AppointmentPackagesCLS`, `ItemCLS` | Opened by FrmPackagesListMulti.cs | EF: `opp.db.Item` |
| AppointmentsListForm22 | BeautyCenter/Session2/AppointmentsListForm22.cs | List of appointments | List view only | Opened by DashboardForm.cs | Unknown |
| DashboardForm | BeautyCenter/Session2/DashboardForm.cs | "X-ray Centers - Dashboard" — sub-vertical dashboard (medical imaging center variant) | Nav buttons: Appointments, Dashboard | Opens AppointmentsListForm22; opened by MainFrm_AccountUpdate | Unknown |
| FrmAddNewSession | BeautyCenter/Session2/FrmAddNewSession.cs | Add a treatment/imaging session with before/after photos | Browse Before/After images, Save | Unknown | Unknown |
| AccountsCustomerLinkFrm | Customer_Supplier/Customers/AccountsCustomerLinkFrm.cs | Link GL sub-accounts to Customer records | Load unlinked/linked accounts, save link via EF | Opened by MainFrm_AccountUpdate | EF: `opp.db.Submain_Account`, `opp.db.Customer`, `SaveChanges` |
| Edit_Item_PriceType | Customer_Supplier/Customers/Customer_Price/Edit_Item_PriceType.cs | Edit sale price of a single item | Edit/Close via `Prices_List_ValuesCLS` | Opened by SearchItemPrices_List_Values.cs | Delegated to Prices_List_ValuesCLS |
| Edit_Prices_List_Values | Customer_Supplier/Customers/Customer_Price/Edit_Prices_List_Values.cs | Bulk-edit customer price-list values | Edit by type/item, print via `Prices_List_ValuesCLS` | Opened by MainFrm_AccountUpdate, Prices_List_Values.cs | Delegated |
| Price_Type_Edit_PurchasFRM | Customer_Supplier/Customers/Customer_Price/Price_Type_Edit_PurchasFRM.cs | Edit a purchase price type | OK/Delete via `Price_Type_ListCLS` | Opened by Price_Type_List_PurchasFRM.cs | Delegated |
| CustomersLinkToAccountsFrm | Customer_Supplier/Customers/CustomersLinkToAccountsFrm.cs | Mirror of AccountsCustomerLinkFrm from the Customer side | Load, save link | Opened by MainFrm_AccountUpdate | EF: `opp.db.Customer`, `opp.db.Submain_Account` |
| CustomerAccountItemPrintFrm | Customer_Supplier/Print/CustomerAccountItemPrintFrm.cs | Print preview of customer account/item statement | ReportViewer print | Called from Customer statement screens (out of scope) | Unknown |
| CustomerAccountPrintFrm | Customer_Supplier/Print/CustomerAccountPrintFrm.cs | Print preview of customer account statement | ReportViewer print | Opened by Customer_AccountMokhtalat*.cs (4 variants), Supplier_AccountNewFrm*.cs | Unknown |
| CustomerSupplierAccountPrnt | Customer_Supplier/Print/CustomerSupplierAccountPrnt.cs | Print preview of combined customer/supplier account | ReportViewer print | Opened by Customer_BalanceMokhtalatList.cs, Supplier_BalanceList.cs | Unknown |
| CustomerAccountPrintFrm_ | Customer_Supplier/Suppliers/Print/CustomerAccountPrintFrm.cs | Duplicate of CustomerAccountPrintFrm reused for Suppliers (same filename, different folder) | ReportViewer print | Opened by Supplier_AccountNewFrm.cs, Supplier_AccountNewFrmPeriod.cs | Unknown |
| Suppliers_PricePRT | Customer_Supplier/Suppliers/Suppliers_Price/Print/Suppliers_PricePRT.cs | Print preview of supplier price list | ReportViewer print | Unknown | Unknown |
| Suppliers_NotHavePrice_ | Customer_Supplier/Suppliers/Suppliers_Price/Suppliers_NotHavePrice_.cs | List/manage suppliers without a set price | Add/Edit/Delete, print, search via `Supplier_PriceCLS` | Unknown | Delegated to Supplier_PriceCLS |
| Form2 | Form2.cs | Empty scaffold form, no logic — appears unused/leftover | None | None | None |
| IncomeStatementFrm_RPT | General_LedgerAccount/Final_accounts/IncomeStatement/IncomeStatementFrm_RPT.cs | Income statement report display | ReportViewer only | Opened by Incoming_StatementByDate.cs | Delegated to Incoming_StatementCLS |
| Incoming_Statement | General_LedgerAccount/Final_accounts/IncomeStatement/Incoming_Statement.cs | Income statement entry/generation screen | Show, Print via `Incoming_StatementCLS`, `Mezan_MorgaaCLS` | Unknown | Delegated |
| Incoming_StatementByDate | General_LedgerAccount/Final_accounts/IncomeStatement/Incoming_StatementByDate.cs | Income statement filtered by date | Show, Print, Master lookup | Opens IncomeStatementFrm_RPT, Mezan_Morgaa_ByMagamehFrm_RPT_RPT; opened by MainFrm_AccountUpdate | Delegated |
| Incoming_StatementByFinancialPeriod | General_LedgerAccount/Final_accounts/IncomeStatement/Incoming_StatementByFinancialPeriod.cs | Income statement filtered by financial period | Show, Print | Unknown | Delegated |
| Profit_Loss_StatFRMNew | General_LedgerAccount/Final_accounts/IncomeStatement/Profit_Loss_StatFRMNew.cs | Final accounts / P&L statement, newer variant | Show, split-button print | Opened by MainFrm_AccountUpdate | Delegated to Incoming_StatementCLS |
| Incoming_StatementOLD | General_LedgerAccount/Final_accounts/Incoming_StatementOLD.cs | Deprecated/legacy version of the income statement screen (filename explicitly marked OLD) | Show, Print | Unknown | Delegated |
| Profit_Loss_StatFRM | General_LedgerAccount/Final_accounts/Profit_Loss_StatFRM.cs | Final accounts / P&L statement, older variant — duplicate of Profit_Loss_StatFRMNew | Show, split-button print | Opened by MainFrm_AccountUpdate | Delegated to Incoming_StatementCLS |
| AddJournalDays | General_LedgerAccount/Journal_DaysList/AddJournalDays.cs | Add a "journal day" code/name entry. Namespace `General_Ledger.Journal_DaysList` though the file lives in the Accounts project | Validate code, Add via `Journal_DaysListCLS` | Unknown | Delegated |
| Journal_DaysListFrm | General_LedgerAccount/Journal_DaysList/Journal_DaysListFrm.cs | List/manage journal-day codes. Namespace `General_Ledger` (mismatched with Accounts project) | Add/New/Close, add-update/delete grid rows | Opened by MainFrm_AccountUpdate | Journal_Codes |
| Account_Entry_Print | General_LedgerAccount/Journal_Entry/Account_Entry_Print.cs | Display and print a journal entry (voucher) | ReportViewer print via `AccountEntry_CLS` | Unknown | Delegated |
| MasterAccountPrintFRM | General_LedgerAccount/Master_Account/MasterAccountPrintFRM.cs | Print sub-ledger (Master Account) for a GL account | Close, print button | Unknown | Unknown |
| Master_Account_CostCenter_FRM | General_LedgerAccount/Master_Account/Master_Account_CostCenter_FRM.cs | Sub-ledger view by cost center | Close, master lookup, via `AccountEntry_CLS` | Opened by MainFrm_AccountUpdate | Delegated |
| Master_Account_Details2 | General_LedgerAccount/Master_Account/Master_Account_Details2.cs | Sub-ledger (Master Account) detail view for one account | Reset, close, master lookup, via `AccountEntry_CLS` | Opened by MainFrm_AccountUpdate | Delegated |
| Master_CostCenterPrintFRM | General_LedgerAccount/Master_Account/Master_CostCenterPrintFRM.cs | Print sub-ledger for a cost center | Close, print | Unknown | Unknown |
| Master_Account_CostCenter_TarsesdFRM | General_LedgerAccount/.../New folder/Master_Account_CostCenter_TarsesdFRM.cs | Sub-ledger by cost center with balance carry-forward ("New folder", near-duplicate) | Reset, close, update balance, "Tar7eel" (post) balance | Opened by MainFrm_AccountUpdate | Direct SQL: Submain_Account, Account_Entry_Master_Balance, Account_Entry_Master_Balance_MezanTemp |
| Tar7eel_Account_Entry | General_LedgerAccount/Master_Account/Tar7eel_Account_Entry.cs | Post ("ترحيل") journal entries to the ledger | Make Post, Cancel Post, Exit via `Accounts_Entry_POST_CLS` | Opened by MainFrm_AccountUpdate | Delegated to Accounts_Entry_POST_CLS |
| Mezan_Morgaa_ByMagamehFrm_RPT | General_LedgerAccount/Mezan_Morgaa_Final_Accounts/Mezan_Morgaa_ByMagamehFrm_RPT.cs | Trial balance report grouped by summary | Show via `Mezan_MorgaaCLS` | Opened by MainFrm_AccountUpdate, Incoming_StatementByDate.cs, Mezan_Morgaa_ByPeriod*.cs (5 call sites) | Delegated |
| Mezan_Morgaa_ByMagameh_ArsedaFrm_RPT | General_LedgerAccount/Mezan_Morgaa_Final_Accounts/Mezan_Morgaa_ByMagameh_ArsedaFrm_RPT.cs | Closing trial balance with summaries and balances | Show via `Mezan_MorgaaCLS` | Opened by MainFrm_AccountUpdate + several Mezan_Morgaa_ByPeriod* variants | Delegated |
| Mezan_Morgaa_ByPeriod | General_LedgerAccount/Mezan_Morgaa_Final_Accounts/Mezan_Morgaa_ByPeriod.cs | Trial balance for a period | Close, master lookup, update balance, Tar7eel balance | Opens Mezan_Morgaa_ByMagamehFrm_RPT / _ArsedaFrm_RPT; opened by MainFrm_AccountUpdate | Direct SQL: Submain_Account, Account_Entry_Master_Balance, Account_Entry_Master_Balance_MezanTemp; EF `opp.db.Currency` |
| Mezan_Morgaa_ByPeriodByAccount | General_LedgerAccount/.../Mezan_Morgaa_ByPeriodByAccount.cs | Trial balance for a period, single account — near-duplicate of Mezan_Morgaa_ByPeriod | Same as above + account search | Opens same two RPT forms; opened by MainFrm_AccountUpdate | Same direct SQL tables |
| Mezan_Morgaa_ByPeriodNew | General_LedgerAccount/.../New folder/Mezan_Morgaa_ByPeriodNew.cs | Newer/alternate variant of the period trial balance ("New folder", another near-duplicate) | Same pattern | Opens the two RPT forms | Direct SQL: Account_Entry_Master_Balance |
| Suppliers_DestructionFrm | InvoicesFrm/DestructionSales/Suppliers_DestructionFrm.cs | Record/manage "Destruction Sales" to suppliers (damaged/waste goods) | New/Add/Edit/Delete/Close buttons | Unknown | Direct SQL: suppliers_Destruction |
| Item_List | InvoicesFrm/Item_List.cs | Item picker dialog — thin wrapper hosting a `Store_LIB.Stores_Items` user control | Delegates all item listing/search to the embedded Store_LIB control | Opened by ItemDataBill.cs, ItemData_ph_Bill.cs, ItemData_.cs | Delegated to Store_LIB |
| PrintBillInvoice | InvoicesFrm/Print/PrintBillInvoice.cs | Print preview of a purchase/sales bill invoice | ReportViewer print via `BillsClss` | Opens PrintInvoice | Delegated |
| BillItemsListFrm | InvoicesFrm/UpdateBillPrices/BillItemsListFrm.cs | Empty stub — no logic in code-behind, generic title equal to class name | None | None | None |
| InvoicesReverseTotal | InvoicesFrm/invoices/InvoicesReverse/InvoicesReverseTotal.cs | Sales-return (reverse invoice) totals screen | Insert/Update/Delete via `InvoicesTotalReverseCls` | Unknown | Delegated |
| Invoices_ListFrm2 | InvoicesFrm/invoices/NewFolder1/Invoices_ListFrm2.cs | Invoice list/search screen ("NewFolder1") | Search, Show via `InvoicesClss` | Unknown | Delegated |
| Order_PurshingFrm | InvoicesFrm/invoices/Order_PurshingFrm.cs | Purchasing/disbursement order screen ("إذن الصرف") | Make Order via `InvoicesClss`, `Order_purchasingClss` | Unknown | Delegated |
| Item_Transaction_TotalyDetails | Item/Item_Transaction_TotalyDetails.cs | Item transaction totals/details report | Show data, print (all/single) via `ItemCLS`, `ItemsCardsCLS`, `ItemsSearchCLS` | Unknown | EF: `opp.db.Item` |
| CashierForm | PointOfSales/NewFolder1/CashierForm.cs | Restaurant/café cashier screen — MVVM-style with `CashierViewModel : INotifyPropertyChanged` | Pay, Print | Opens PaymentForm | Unknown |
| PaymentForm | PointOfSales/NewFolder1/PaymentForm.cs | POS payment/confirm dialog | Confirm payment | Opened by CashierForm.cs | Unknown |
| POS_Invoice_Edit | PointOfSales/POS_Invoice_Edit.cs | Edit an in-progress POS sales invoice/order | Close order, Delete, Qty +/-, Save, Search, print bill/invoice via multiple CLS classes | Unknown | Delegated to multiple CLS classes |
| PrintInvoicePOSTest_OLD | PointOfSales/PrintPOS/Test/PrintInvoicePOSTest_OLD.cs | Explicitly-named test/experimental POS print form (deprecated) | ReportViewer print via `BillsClss` | Unknown | Delegated |
| ActiveOrdersFrm | PointOfSales/TablesList/ActiveOrdersFrm.cs | List of active/open POS orders with filtering | Filter buttons, grid of Invoice/Table/Waiter/Total via `ActiveOrdersFrmCLS` | Unknown | Reads temp invoices table via `ActiveOrdersFrmCLS` |
| DriversSettlementFrm | PointOfSales/TablesList/DriversSettlementFrm.cs | Settle delivery drivers' cash custody/float | Settle button via `DriversSettlementCLS`, `Money_Transaction_BoxOrderReportCLS` | Opened by MainFrmPOS | Direct SQL update statement |
| PurchasePeriodTotal | Profit_Loss_Statement/PurchaseTotal/PurchasePeriodTotal.cs | Total purchases report for a period | Close, Show, split-button print via `ItemsSearchCLS`, `SalesCostCLS` | Unknown | Delegated |
| PurchaseTotalPrnt | Profit_Loss_Statement/PurchaseTotal/PurchaseTotalPrnt.cs | Print preview of the purchase-period-total report | ReportViewer print | Unknown | Unknown |
| Edit_SalesCostFrm | Profit_Loss_Statement/SalesCost/Edit_SalesCostFrm.cs | Edit a sales-cost account entry | OK/Close | Opened by SalesCostFrm.cs, SalesMasrofat.cs | Unknown |
| SalesPeriodTotal | Profit_Loss_Statement/SalesTotal/SalesPeriodTotal.cs | Total sales report for a period | Close, Show, split-button print via `ItemsSearchCLS`, `SalesCostCLS` | Unknown | Delegated |
| SalesTotalPrnt | Profit_Loss_Statement/SalesTotal/SalesTotalPrnt.cs | Print preview of the sales-period-total report | ReportViewer print | Unknown | Unknown |
| Capita_frm (file Machinefrm.cs) | Profit_Loss_Statement/Special_Accounts/Machinefrm.cs | Master list of "Machines"/vehicles with capital share % and price-per-trip ("نزلة"), old non-partial VS-era class | Add/Edit/Delete machine entry | Opened by MainFrm_AccountUpdate | Direct SQL: FROM Machin_list |
| tottalof_nazlla (file Nazzlla_Detailes.cs) | Profit_Loss_Statement/Special_Accounts/Nazzlla_Detailes.cs | Trip ("نزلة") totals/profit entry for one machine — companion to Capita_frm | Add/Close/New trip totals | Opened by MainFrm_AccountUpdate | Delegated (uses `operations`) |
| frm_View_detaile_nazlla (file frm_View_detaile.cs) | Profit_Loss_Statement/Special_Accounts/frm_View_detaile.cs | View/print detail of recorded trips | Delete, print, view | Opened by MainFrm_AccountUpdate | Direct SQL: FROM Machin_list |
| Total_Balance_Period | Profit_Loss_Statement/Total_Balance_Report/Total_Balance_Period.cs | Inventory/stock valuation report for a period | Show, print via `DaysWorkSkowCLS` | Opens Total_Balance_Period_FRM | Delegated |
| Total_Balance_Period_FRM | Profit_Loss_Statement/Total_Balance_Report/Total_Balance_Period_FRM.cs | Print preview of the inventory/stock-valuation report | ReportViewer print | Opened by Total_Balance_Period.cs | Unknown |
| Receve_Talabia | Recive_TAlbia/Receve_Talabia.cs | Register receipt of orders/deliveries | Close/Delete/Preview/Print/Save/Setup/Show via `Receive_TalabiaCLS` | Opened by MainFrm_AccountUpdate | Delegated |
| Help | Reports/Help.cs | Empty scaffold form, no content | None | None | None |
| InvoiceSalesTaxPRNT | Reports/InvoiceTax/InvoiceSalesTaxPRNT.cs | Print preview of sales-tax invoice report | ReportViewer print | Opened by Purchase_ListTaxes.cs, Invoices_ListTaxesFrm.cs, Invoices_ListTaxesFrm_1.cs | Unknown |
| ItemPriceFRM | Reports/ItemPriceFRM.cs | Item price report | Show | Unknown | Unknown |
| Item_Qnty_SalesFRM | Reports/Item_Qnty_SalesFRM.cs | Total customer sales-quantity report | Show | Unknown | Unknown |
| Money_SpentCustomer | Reports/MoneyOrders/Customers/Money_SpentCustomer.cs | Money orders spent/received report for customers | Search, print via `Money_SpentCustomerSuppliersCLS` | Opened by MainFrm_AccountUpdate | Delegated |
| Money_SpentSupplier | Reports/MoneyOrders/Suppliers/Money_SpentSupplier.cs | Money orders spent/received report for suppliers | Search, print via `Money_SpentSuppliersCLS` | Opened by MainFrm_AccountUpdate | Delegated |
| balance_Profits | Reports/Profits/balance_Profits.cs | Profit report on items sold | Refresh, print | Unknown | Unknown |
| PurchaseReports_POS | Reports/SalesPurchaseReports/PurchaseReports_POS.cs | Sales-return reports for POS | Search, Show, print | Unknown | Unknown |
| SalesReportsPRNT | Reports/SalesPurchaseReports/ReportPrint/SalesReportsPRNT.cs | Print preview of sales report | ReportViewer print | Unknown | Unknown |
| SalesReportsurchasPrice | Reports/SalesPurchaseReports/ReportPrint/SalesReportsurchasPrice.cs | Print preview of sales report with purchase-price comparison | Search, Show, print | Opened by MainFrmPOS | Unknown |
| SalesReportsurchasPricePOS | Reports/SalesPurchaseReports/ReportPrint/SalesReportsurchasPricePOS.cs | POS variant of the above report | Search, Show, print | Unknown | Unknown |
| SalesReports_POS | Reports/SalesPurchaseReports/SalesReports_POS.cs | POS sales report | Search, Show, print | Unknown | Unknown |
| BalanceWithPurchasePric | Reports/SalesPurchaseReportsTotals/BalanceWithPurchasePric/BalanceWithPurchasePric.cs | Item-sales valuation report vs purchase price | Save report, Show order, print (12 handler variants — heavily reworked/patched) | Opened by MainFrm_AccountUpdate | Unknown |
| BalanceWithPurchasePricPRNT | Reports/.../BalanceWithPurchasePric/BalanceWithPurchasePricPRNT.cs | Print preview of the above report | ReportViewer print | Unknown | Unknown |
| BalanceWithPurchasePric_SUM | Reports/.../BalanceWithPurchasePric/BalanceWithPurchasePric_SUM.cs | Summary variant of BalanceWithPurchasePric (near-duplicate) | Same save/show/print pattern | Opened by MainFrm_AccountUpdate | Unknown |
| BillPurchaseReportsPRNT | Reports/.../BillReportPrint/BillPurchaseReportsPRNT.cs | Print preview of bill/purchase report | ReportViewer print | Opened by PurchaseReportsTotal.cs | Unknown |
| frmChart | Reports/SalesPurchaseReportsTotals/Chart.cs | Chart/graph display screen | Clear/Splash/Start chart buttons | Opens frmData; opened by Data.cs | Unknown |
| frmData | Reports/SalesPurchaseReportsTotals/Data.cs | Data grid feeding the chart screen | Show data | Opens frmChart; opened by Purchase_ListTaxes.cs, Chart.cs, PurchaseReportsTotal.cs | Unknown |
| InvoiceSalesReportsPRNT | Reports/.../InvoiceReportPrint/InvoiceSalesReportsPRNT.cs | Print preview of invoice-sales report | ReportViewer print | Opened by SalesReportsTotal.cs, SalesReportsTotal__POS.cs | Unknown |
| SalesReportsTotal__POS | Reports/SalesPurchaseReportsTotals/SalesReportsTotal__POS.cs | POS sales-report totals | Print, print preview, search via `SalesReportsTotalCLS` | Opens InvoiceSalesReportsPRNT; opened by MainFrmPOS | Delegated |
| CustomerSalesReportsPRNT | Reports/Wekala/CustomerSalesReportsPRNT.cs | Print preview of customer sales report for agency/"Wekala" module | ReportViewer print | Opened by FrmDailyCustomerSales.cs | Unknown |
| ItemSalesReportsPRNT | Reports/Wekala/ItemSalesReportsPRNT.cs | Print preview of item sales report for agency/"Wekala" module | ReportViewer print | Opened by FrmDaily_All_ItemSales.cs, FrmDailyItemSales.cs | Unknown |
| Edit_Export_ItemQnty | Requests/Edit_Export_ItemQnty.cs | Edit exported/disbursed item quantities | OK/Close | Opened by Request_To_Market.cs | Direct SQL: Price_Type_Sell_Values, Item |
| PrintInvoice | Requests/InvoiceTemplate/Print/PrintInvoice.cs | Print an invoice (Requests/InvoiceTemplate module) | ReportViewer print via `BillsClss`, `Money_Transaction_BoxOrderReportCLS`, `Requests_Cls` | Opened by FrmDailyCustomerSales*, PrintBillInvoice.cs, NewFrmInvoive.cs | Delegated |
| PrintInvoice1 | Requests/InvoiceTemplate/Print/PrintInvoice1.cs | Alternate/duplicate invoice print layout | Same as PrintInvoice | Unknown | Delegated |
| ItemDataPh | Requests/ItemData/ItemDataPh.cs | Item picker for building an invoice's item lines | Add/OK/Close via `Cls_Conversion_Unit`, `InvoicesOperationCLS`, `ItemCLS`, `ItemDataPhCLS` | Opened by EditFRM_Invoices.cs, Make_Invoices.cs, EditFRM_Invoices_Reverse.cs, Make_Invoices_Reverse.cs (heavy reuse, ~10 call sites) | Direct SQL: Unit_item |
| ItemDataCar | Requests/ItemData/NewFolder1/ItemDataCar.cs | Near-duplicate of ItemDataPh, variant for a "Car"-related module ("NewFolder1") | Add/OK/Close via same CLS classes | Unknown | Direct SQL: Unit_item |
| ItemData_ | Requests/ItemData/NewFolder1/ItemData_.cs | Another near-duplicate item picker | Add/OK/Close, Show items via `Cls_Conversion_Unit` | Opens Item_List | Direct SQL: Price_Type_Sell_Values, Unit_item, Customer_Price |
| ItemData_2 | Requests/ItemData/NewFolder1/ItemData_2.cs | Yet another near-duplicate item picker | Add/OK/Close, Show items via `Cls_Conversion_Unit`, `InvoicesOperationCLS` | Unknown | Direct SQL: Price_Type_Sell_Values, Unit_item |
| Add_Requests | Requests/RequestFiles/Add_Requests.cs | Create/manage purchase-style requests/orders | Add/Close/Delete/Show order via `Requests_Cls` | Unknown | Delegated |
| Request_To_Market | Requests/RequestFiles/Request_To_Market.cs | "Request to Market" order screen — older DataSet-based code with Hashtable-based ListView grouping | Close/Edit/Finish/Invoice/Show/Export via `Requests_Cls` | Opens Edit_Export_ItemQnty | Delegated |
| Request_With_Money | Requests/RequestFiles/Request_With_Money.cs | Requests paid with a prepaid amount | Show money/order via `Requests_Cls` | Unknown | Delegated |
| Static_NotesInInvoice | Requests/Static_NotesInInvoice.cs | Configure the fixed note text printed on invoices; Load handler still named `ChangeCompanyNameFrmcs_Load` — leftover from a copy-pasted template | Save note, store image via a system-parameter key | Unknown | System parameter: Static_NotesInInvoicePrintNotShowCustomerAccount |
| AddTagerKontra | Requests/TagerKontra/AddTagerKontra.cs | Add a "Tager Kontra" lookup entry (exact domain meaning not determinable from code); Load handler still named `AddRealStateType_Load` and field `txtRealStateTypeName` — evidence of copy-paste from an unrelated "Real Estate Type" template | Add via `TagerKontraCLS` | Opened by TagerKontraCLSListFrm.cs | Delegated |
| TagerKontraCLSListFrm | Requests/TagerKontra/TagerKontraCLSListFrm.cs | List/manage "Tager Kontra" master entries | Add/Close/New, Delete via `TagerKontraCLS` | Opens AddTagerKontra; opened by MainFrm_AccountUpdate, ItemData.cs | Delegated |
| CustomerSalesMan | SalesManProcess/CustomerSalesMan.cs | Manage sales-rep assignment to customers | Add/Delete via `CustomersAccountCLS` | Unknown | Delegated |
| Frm_Special_price | Special_price_4_Customers/Frm_Special_price.cs | Manage special/negotiated prices for specific customers (blank title; old non-partial VS-era class) | Add/Edit/Delete item via `Special_price_CLS` | Opened by MainFrm_AccountUpdate | Delegated |
| Test | Test.cs | Developer scratch/test form (textbox tab-order tests, print-button experiments) — not production UI | None meaningful | Unknown | Direct SQL: Item, Unit_Item, myTestTable (test queries) |
| Add_Requests_Work_In | Work_In/Add_Requests_Work_In.cs | Create outward-processing ("تصنيع للغير") request for goods sent to a third party for work | Add/Close/Delete, print, Show via `Requests_Work_In_Cls` | Unknown | Delegated |
| Draws_List_In_FRM | Work_In/Draws_List_In_FRM.cs | Master list of "draw" items with price, for the Work-In module (old non-partial class) | Add/Edit/Delete item | Unknown | Unknown |
| Edit_Import_ItemQntyIn | Work_In/Edit_Import_ItemQntyIn.cs | Edit imported/received item quantities for Work-In | Close/Delete/OK/Receive | Unknown | Unknown |
| Make_Invoices_Work_In | Work_In/Make_Invoices_Work_In.cs | Generate the outward-processing invoice | Print, Total-update via `Requests_Work_In_Cls` | Opened by Receive_Requests_Work_In.cs | Delegated |
| Receive_Destruction_Work_In | Work_In/New Folder/Receive_Destruction_Work_In.cs | Record damaged/destroyed goods received in Work-In ("New Folder") | Add/Close/Delete, print, Show, Make Invoice via `Cls_Conversion_Unit`, `Requests_Work_In_Cls` | Opens Make_Invoices_Work_In | Delegated |
| Work_InPrintInvoice | Work_In/Print/Work_InPrintInvoice.cs | Print preview of Work-In invoice | ReportViewer print | Unknown | Unknown |
| Receive_Requests_Work_In | Work_In/Receive_Requests_Work_In.cs | Receive/process outward-processing requests | Add/Close/Delete, Make Invoice via `Cls_Conversion_Unit`, `Requests_Work_In_Cls` | Opens Make_Invoices_Work_In | Delegated |
| Request_To_Work_In | Work_In/Request_To_Work_In.cs | Create a Work-In (outward-processing) request (blank title) | Close/Edit/Finish/Invoice/Show, Export via `Requests_Work_In_Cls` | Unknown | Delegated |
| Work_In_Reports_Receive | Work_In/Work_In_Reports_Receive.cs | Report on items sent to a third party for processing | Delete, print, Show via `Requests_Work_In_Cls` | Unknown | Delegated |
| Work_In_Reports_SendANDBalance | Work_In/Work_In_Reports_SendANDBalance.cs | Report on items received back + balance for Work-In | Print, Show via `Requests_Work_In_Cls` | Unknown | Delegated |
| Edit_Import_ItemQnty | Work_Out/Edit_Import_ItemQnty.cs | Edit imported/received item quantities for Work-Out (mirror of Edit_Import_ItemQntyIn) | Close/Delete/OK/Receive | Unknown | Unknown |
| Make_Invoices_Work_Out | Work_Out/Make_Invoices_Work_Out.cs | Generate the outward-processing invoice for Work-Out | Print, Total-update via `Requests_Work_Out_Cls` | Opened by Receive_Requests_Work_Out.cs, Request_To_Work_Out.cs, Receive_Destruction_Work_Out.cs | Direct SQL: from Supplier |
| Receive_Destruction_Work_Out | Work_Out/New Folder/Receive_Destruction_Work_Out.cs | Record damaged/destroyed goods for Work-Out, external processing | Add/Close/Delete, print, Show, Make Invoice via `Cls_Conversion_Unit`, `Requests_Work_Out_Cls` | Opens Make_Invoices_Work_Out | Delegated |
| Draws_ListPrint | Work_Out/Print/Draws_ListPrint.cs | Print preview of the Draws list | ReportViewer print via `DrawsServiceCls` | Opened by Draws_ListFRM.cs, Draws_ListFRM2.cs | Delegated |
| Work_OutPrintInvoice | Work_Out/Print/Work_OutPrintInvoice.cs | Print preview of Work-Out invoice | ReportViewer print via `Requests_Work_Out_Cls` | Unknown | Delegated |
| Receive_Requests_Work_Out | Work_Out/Receive_Requests_Work_Out.cs | Receive/process outward-processing (external) requests | Add/Close/Delete, Make Invoice via `Cls_Conversion_Unit`, `Requests_Work_Out_Cls` | Opens Make_Invoices_Work_Out | Delegated |
| Request_To_Work_Out | Work_Out/Request_To_Work_Out.cs | Create a Work-Out (external processing) request (blank title) | Close/Edit/Finish/Invoice/Show, Export via `Requests_Work_Out_Cls` | Opens Edit_Import_ItemQnty, Make_Invoices_Work_Out | Delegated |
| Work_Out_Reports_Receive | Work_Out/Work_Out_Reports_Receive.cs | Report on items received from external processing | Delete, print, Show via `Requests_Work_Out_Cls` | Unknown | Delegated |
| Work_Out_Reports_SendANDBalance | Work_Out/Work_Out_Reports_SendANDBalance.cs | Report on items sent to supplier + balance for Work-Out | Print, Show via `Requests_Work_Out_Cls` | Unknown | Delegated |

**Batch observations (Accounts):**
- **Two navigation styles, one real hub.** `MainFrmPOS` (in-scope, `: Form`) is the POS/restaurant MDI shell; `MainFrm_AccountUpdate` (out-of-scope, `: Project_ControlLIB.MyForm`, 8,980 lines, 718 `new <Form>()` calls) is the true accounting main menu and opens the large majority of forms listed above. Both are `IsMdiContainer = true`.
- **Shared base form exists but this project's Form-derived files bypass it.** 95 files elsewhere in Accounts derive from `Project_ControlLIB.MyForm` (10 more from bare `MyForm`) — but by construction none of the 125 files in this table use it (they derive `Form` directly), so `MyForm`'s cross-cutting behavior does not apply to this file set.
- **Layered DAL, but thinly used at the Form layer.** The core DAL is used **directly** in only 6 of the 125 form files (all visibly newer, EF-first code). The rest delegate all data access to intermediate `...CLS`/`...Clss` business-logic wrapper classes not part of this file set — most "DB Tables/Procs" cells above are Unknown/Delegated for this reason, not because no DB access occurs.
- **A handful of forms bypass the DAL entirely** with raw inline SQL (against `Submain_Account`, `Account_Entry_Master_Balance(_MezanTemp)`, `Machin_list`, `suppliers_Destruction`, `Unit_item`, `Price_Type_Sell_Values`, `Item`, `Journal_Codes`), concentrated in the General_Ledger/Mezan_Morgaa (trial balance) and Special_Accounts (machine/trip) areas.
- **Error handling is uniform but often silent.** 933 `catch (Exception ...)` blocks across the project, almost all paired with `MessageBox.Show(...)` — but 423 blocks are empty `catch { }` swallow-alls.
- **Confirmation pattern:** destructive actions typically go through `AccountCommonDataLIB.Common.MsgBoxYesNo(...)` before calling into a CLS class and then `opp.Commit()`/`opp.Rollback()`.
- **Two coding eras coexist.** 15 files are old-style non-partial `class X : System.Windows.Forms.Form` (.NET 1.x pattern) versus 109 modern `partial class X : Form` files.
- **Copy-paste template leftovers observed directly in code:** `Static_NotesInInvoice`'s Load handler is still named `ChangeCompanyNameFrmcs_Load`; `AddTagerKontra`'s Load handler is `AddRealStateType_Load` with field `txtRealStateTypeName` — both indicate forms were cloned from unrelated templates without renaming internals.
- **Character-encoding inconsistency:** mix of UTF-8 and Windows-1256 source files, sometimes within the same folder.
- **Widespread near-duplicate/parallel forms**, maintained by copy-paste rather than shared components — notably the entire `Work_In` folder is mirrored almost 1:1 by `Work_Out` (send-to-third-party vs. received-from-third-party variants of the same workflow).
- **Dead/placeholder forms present in the tree:** `Form2.cs`, `Reports/Help.cs`, `InvoicesFrm/UpdateBillPrices/BillItemsListFrm.cs` contain no logic beyond `InitializeComponent()`; `Test.cs` is a developer scratch form with direct test SQL against `myTestTable`.
- **Business-vertical scope is broader than pure accounting.** Alongside core GL/invoicing/inventory, this project also contains a `BeautyCenter` module (appointments, packages, an "X-ray Centers" dashboard suggesting a medical-imaging vertical) and a `PointOfSales` restaurant/café module.
- **Naming conventions:** Arabic-labeled UI is the norm; button handlers are almost universally `Btn<Action>_Click`/`btn<Action>_Click` (casing inconsistent); printable screens are named `<Subject>PRNT`/`Print<Subject>` and are typically thin ReportViewer wrappers with no business logic of their own.

### General_Ledger
*Two directory trees both contain Form-derived classes. `General_Ledger\General_Ledger\` is the current SDK-style library the solution actually references. `General_Ledger\New folder\General_Ledger\` is the separate legacy standalone app described under **Solution Structure** — its rows are marked "(legacy)" below.*

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Review_Account_Entry | AccountEntryReview/Review_Account_Entry.cs | "مراجعة القيود" — multi-tab filter/search screen | Search/filter via AccountEntry_CLS | NewAccount_EntryMadinDa2nFRM | Account_Entry_List/Details (via AccountEntry_CLS) |
| Get_Account_BalanceFRM | Final_accounts/Get_Account_BalanceFRM.cs | Computes account balances (WIP/test-looking, untranslated title) | Raw SQL SELECT/INSERT/UPDATE/DELETE building temp balances | Account_ListFrm | AccountTempBalance, Submain_Account, Account_Entry_Master_Balance, Main_Account |
| Get_Account_BalanceFRM2 | Final_accounts/Get_Account_BalanceFRM2.cs | "قائمة المركز المالى" (financial position / balance sheet) | Uses BalanceCLS, Mezan_MorgaaCLS | Unknown | Submain_Account, AccountTempBalance |
| IncomeStatementFrm_RPT | Final_accounts/IncomeStatement/IncomeStatementFrm_RPT.cs | "قائمة الدخل" RDLC report viewer | Renders IncomeStatementFrm_RPT.rdlc | — | Unknown (report-bound) |
| Incoming_Statement | Final_accounts/IncomeStatement/Incoming_Statement.cs | Income statement builder. **Excluded from build** via `<Compile Remove>` | Uses Incoming_StatementCLS, Mezan_MorgaaCLS | — | Account_Entry_Master_Balance |
| Incoming_StatementByDate | Final_accounts/IncomeStatement/Incoming_StatementByDate.cs | Income statement by date range. **Excluded from build** | Uses Mezan_MorgaaCLS, Incoming_StatementCLS | Mezan_Morgaa_ByMagamehFrm_RPT, IncomeStatementFrm_RPT, Master_Account_DetailsTarseed2 | Unknown |
| Incoming_StatementByFinancialPeriod | Final_accounts/IncomeStatement/Incoming_StatementByFinancialPeriod.cs | Income statement by financial period. **Excluded from build** | Uses Incoming_StatementCLS, Mezan_MorgaaCLS | Unknown | Unknown |
| Profit_Loss_StatFRMNew | Final_accounts/IncomeStatement/Profit_Loss_StatFRMNew.cs | "الحسابات الختاميه" P&L, newer variant. **Excluded from build** | Uses Mezan_MorgaaCLS | Unknown | Unknown |
| Incoming_StatementOLD | Final_accounts/Incoming_StatementOLD.cs | Legacy income statement — the one actually still compiled | Unknown | Unknown | Unknown |
| Profit_Loss_StatFRM | Final_accounts/Profit_Loss_StatFRM.cs | "الحسابات الختاميه" P&L statement (active counterpart of the excluded "New" version) | Unknown | Unknown | Unknown |
| Form1 | Form1.cs | Empty scaffold/placeholder form | None | — | — |
| AddJournalDays | Journal_DaysList/AddJournalDays.cs | "اليوميه" — add/edit a journal-day code record | Validate, save via Journal_DaysListCLS | — | Journal_Codes |
| Journal_DaysListFrm | Journal_DaysList/Journal_DaysListFrm.cs | "اليوميـــات" — journals list | List via operations/DataAdapter, Journal_DaysListCLS | AddJournalDays, AddCurrencyFactor | Journal_Codes |
| Account_Entry_Print | Journal_Entry/Account_Entry_Print.cs | "عرض القيد وطباعته" — view & print a journal entry (typed DataSet/TableAdapter + RDLC) | Load Account_Entry_List_Print dataset, render report | — | Account_Entry_List_Print (view/query) |
| NewAccount_EntryFRM | Journal_Entry/NewFolder1/NewAccount_EntryFRM.cs | "إدخال القيود" — main journal-entry data-entry form. **Not found by a naive `: Form` search** — base class is `Project_ControlLIB.MyForm` | Uses AccountEntry_CLS heavily | — | Account_Entry_List/Details |
| NewAccount_EntryMadinDa2nFRM | Journal_Entry/NewFolder1/NewAccount_EntryMadinDa2nFRM.cs | "إدخال القيود" — debit/credit ("Madin/Da2n") variant, same base `MyForm` | Uses AccountEntry_CLS heavily | — | Account_Entry_List/Details |
| NewSimpleAccount_EntryMadinDa2nFRM | Journal_Entry/NewFolder1/NewSimpleAccount_EntryMadinDa2nFRM.cs | "إدخال القيود" — simplified debit/credit entry variant (3rd near-duplicate in this folder) | Uses AccountEntry_CLS, CurrencyFactorsCLS, PrintReportDirectly | Add_Account_EntryFRM, NewAccount_EntryMadinDa2nFRM | Account_Entry_List/Details |
| MasterAccountPrintFRM | Master_Account/MasterAccountPrintFRM.cs | "طباعة أستاذ مساعد حساب" — print subsidiary ledger (typed DataSet, fill code commented out) | Load ds_Balance.MasterAccount, render MasterAccountPrint.rdlc | — | Unknown (report-bound) |
| Master_CostCenterPrintFRM | Master_Account/Master_CostCenterPrintFRM.cs | "طباعة أستاذ مساعد حساب مراكز التكلفه" — print cost-center subsidiary ledger | Load ds_Balance cost-center variant, render report | — | Unknown (report-bound) |
| Master_Account_CostCenter_TarsesdFRM | Master_Account/New folder/Master_Account_CostCenter_TarsesdFRM.cs | "الأستاذ المساعد لمراكز التكلفه" — cost-center subsidiary ledger; opening-balance update / post-balance toolbar | Raw SQL SELECT/INSERT/DELETE/UPDATE; uses AccountEntry_CLS, Mezan_MorgaaCLS | NewAccount_EntryFRM, NewAccount_EntryMadinDa2nFRM | Account_Entry_Master_Balance, Submain_Account, Account_Entry_Master_Balance_MezanTemp |
| Tar7eel_Account_Entry | Master_Account/Tar7eel_Account_Entry.cs | "ترحيل القيود" — post/un-post journal entries for a date range | BtMakePost/BtnCancel_Post; uses Accounts_Entry_POST_CLS | — | Unknown (via Accounts_Entry_POST_CLS) |
| Mezan_Morgaa_ByMagamehFrm_RPT | Mezan_Morgaa_Final_Accounts/Mezan_Morgaa_ByMagamehFrm_RPT.cs | "ميزان المراجعه عن فترة بالمجاميع" — trial balance by totals (RDLC) | Uses Mezan_MorgaaCLS | — | Unknown |
| Mezan_Morgaa_ByMagameh_ArsedaFrm_RPT | Mezan_Morgaa_Final_Accounts/Mezan_Morgaa_ByMagameh_ArsedaFrm_RPT.cs | "ميزان المراجعه ختامى بالمجاميع والأرصدة" — closing trial balance, totals & balances (RDLC) | Uses Mezan_MorgaaCLS | — | Unknown |
| Add_Branch | New folder/Add_Branch.cs | "فرع جديد" — add/edit a company branch | Code+name entry, save via BranchesListCLS | — | Unknown (via BranchesListCLS) |
| BranchesListFrm | New folder/BranchesListFrm.cs | "فروع الشركه" — company branches list | List via operations/DataAdapter, BranchesListCLS | Add_Branch, AddCurrencyFactor | Unknown (via BranchesListCLS) |
| ReportTest | ReportTest.cs | Scratch/dev-test RDLC report form, unlocalized title | Load ds_Balance.MezanMoragaa, render report | — | Unknown (report-bound) |
| DayReg_List (legacy) | New folder\General_Ledger\DayRegistration/DayReg_List.cs | List "Day Registration" records | List via DayRegistrationCLS | DayRegistrationFrm | Unknown |
| DayRegistrationFrm (legacy) | New folder\General_Ledger\DayRegistration/DayRegistrationFrm.cs | Create/edit a day registration | Save via DayRegistrationCLS | Edit_DayRegistrationFrm, DayRegist_RPTSHOW | Unknown |
| Edit_DayRegistrationFrm (legacy) | New folder\General_Ledger\DayRegistration/Edit_DayRegistrationFrm.cs | Edit an existing day registration; Arabic validations render as mojibake in this file | Validate, save | — | Unknown |
| Form1 (legacy, DayRegistration) | New folder\General_Ledger\DayRegistration/Form1.cs | Empty/placeholder form | None | — | — |
| TempComplex_DayRegistrationFrmSP (legacy) | .../Special_Accounts/Complex_DayRegistration/TempComplex_DayRegistrationFrmSP.cs | Special/complex day registration base; also serves as **base class** for the next two forms | Uses Edit_DayRegistrationSPCLS | — | Unknown |
| Complex_DayRegistrationFrmSP (legacy) | .../Complex_DayRegistration/Complex_DayRegistrationFrmSP.cs | Special-account day registration entry/copy form. Inherits `TempComplex_DayRegistrationFrmSP` — **not found by naive `: Form` search** | Btn_Ok, Btn_Add, BtnDelete handlers | — | Unknown |
| EditComplex_DayRegistrationFrmSP (legacy) | .../Complex_DayRegistration/EditComplex_DayRegistrationFrmSP.cs | Edit/browse special-account day registrations, First/Prev/Next/Last nav. Inherits `TempComplex_DayRegistrationFrmSP` | Record-browser nav buttons | — | Unknown |
| Edit_DayRegistrationFrmSP (legacy) | .../Special_Accounts/Edit_DayRegistrationFrmSP.cs | Edit special-account day registration | Uses Edit_DayRegistrationSPCLS | — | Unknown |
| Get_Account_BalanceFRM (legacy) | New folder\General_Ledger\Final_accounts/Get_Account_BalanceFRM.cs | Account balance report, legacy predecessor (older schema `Submain_Account.SubAccount_Id` vs current `.Id`) | Delete/rebuild AccountTempBalance; tree picker via `Connection4It.Account_Common.Account_ListFrmTree` | Account_ListFrmTree | AccountTempBalance, Submain_Account, Account_Entry_Master_Balance |
| Get_Account_BalanceFRM2 (legacy) | .../Final_accounts/Get_Account_BalanceFRM2.cs | Account balance lookup, legacy variant 2 | Uses BalanceCLS | — | Submain_Account/AccountTempBalance |
| Incoming_Statement (legacy) | .../Final_accounts/Incoming_Statement.cs | Income statement, legacy predecessor of the Tree-A form of the same name | Uses Incoming_StatementCLS | — | Same as current equivalent |
| Profit_Loss_StatFRM (legacy) | .../Final_accounts/Profit_Loss_StatFRM.cs | Profit & loss statement, legacy predecessor | Uses Incoming_StatementCLS | — | Same as current equivalent |
| FirstBalance_Financial_Period_FRM (legacy) | .../FirstBalance_Financial_Period/FirstBalance_Financial_Period_FRM.cs | Set/enter opening balances for a financial period | Uses FirstBalance_CLS, CurrencyFactorsCLS | — | Unknown |
| Form1 (legacy, root) | New folder\General_Ledger\Form1.cs | Empty/placeholder form | None | — | — |
| Form2 (legacy) | New folder\General_Ledger\Form2.cs | Scratch/test form: ad-hoc `Select * from customer` query, column-grouping demo | Ad-hoc query, grid demo | — | Customer |
| AddJournalDays (legacy) | .../Journal_DaysList/AddJournalDays.cs | Add/edit journal day, legacy predecessor | Uses Journal_DaysListCLS | — | Journal_Codes |
| Journal_DaysListFrm (legacy) | .../Journal_DaysList/Journal_DaysListFrm.cs | Journal days list, legacy predecessor | Uses Journal_DaysListCLS | AddCurrencyFactor | Journal_Codes |
| Add_Account_EntryFRM (legacy) | .../Journal_Entry/Add_Account_EntryFRM.cs | Add a journal/account entry, currency handling | Uses CurrencyFactorsCLS | — | Unknown |
| NewAccount_EntryFRM (legacy, "New folder") | .../Journal_Entry/New folder/NewAccount_EntryFRM.cs | Create new journal entry — duplicate copy, same class name as sibling below | Uses AccountEntry_CLS | Add_Account_EntryFRM | Account_Entry_List/Details |
| NewAccount_EntryFRMOLD (legacy) | .../Journal_Entry/New folder/NewAccount_EntryFRMOLD.cs | Older superseded copy — class is still literally named `NewAccount_EntryFRM` despite the OLD filename | Uses AccountEntry_CLS | Add_Account_EntryFRM | Account_Entry_List/Details |
| NewAccount_EntryFRM (legacy, primary) | .../Journal_Entry/NewAccount_EntryFRM.cs | Create new journal entry — main legacy version | Uses AccountEntry_CLS, CurrencyFactorsCLS | Add_Account_EntryFRM | Account_Entry_List/Details |
| MainFrm (legacy) | New folder\General_Ledger\MainFrm.cs | Main MDI menu/shell of the standalone legacy GL app (WinExe) | Logs form opens to FORMS_DETAILES | Account_Tree_CodesFrm, CurrencyListFrm, Financial_PeriodFrm, Form1, Get_Account_BalanceFRM(RPT), Journal_DaysListFrm, Master_Account_CostCenter_FRM, Master_Account_Details2, Master_Account_DetailsTarseed2, Money_Between_Box, NewAccount_EntryFRM, New_Cash_Money_List, New_OrderMoney_Sarf, ProjUI, Review_Account_Entry, DBConfig | FORMS_DETAILES |
| Make_Account_Entry_Automatic (legacy) | .../Master_Account/Make_Account_Entry_Automatic.cs | Auto-generate account entries (period-end automatic postings) | Uses Accounts_Entry_POST_CLS | — | Unknown |
| MasterAccountPrintFRM (legacy) | .../Master_Account/MasterAccountPrintFRM.cs | Print master chart of accounts, legacy predecessor | RDLC report render | — | Unknown |
| Master_Account_CostCenter_FRM (legacy) | .../Master_Account/Master_Account_CostCenter_FRM.cs | Manage master account cost-center assignment | Uses AccountEntry_CLS | NewAccount_EntryFRM | Unknown |
| Master_Account_Details2 (legacy) | .../Master_Account/Master_Account_Details2.cs | View/edit master account details, variant 2 | Uses AccountEntry_CLS | NewAccount_EntryFRM | Unknown |
| Master_Account_DetailsTarseed2 (legacy) | .../Master_Account/Master_Account_DetailsTarseed2.cs | Master account details, "Tarseed" (posting) variant 2 | Uses AccountEntry_CLS | NewAccount_EntryFRM | Unknown |
| Master_CostCenterPrintFRM (legacy) | .../Master_Account/Master_CostCenterPrintFRM.cs | Print master cost-center list, legacy predecessor | RDLC report render | — | Unknown |
| Master_Account_Details (legacy) | .../Master_Account/New folder/Master_Account_Details.cs | View/edit master account details, base variant — duplicate of Details2 | Uses AccountEntry_CLS | NewAccount_EntryFRM | Unknown |
| Master_Account_DetailsTarseed (legacy) | .../Master_Account/New folder/Master_Account_DetailsTarseed.cs | Master account details Tarseed variant, base — duplicate of Tarseed2 | Uses AccountEntry_CLS | NewAccount_EntryFRM | Unknown |
| Review_Account_Entry (legacy) | .../Master_Account/Review_Account_Entry.cs | Review/browse journal entries, legacy predecessor | Uses AccountEntry_CLS | NewAccount_EntryFRM | Account_Entry_List/Details |
| Tar7eel_Account_Entry (legacy) | .../Master_Account/Tar7eel_Account_Entry.cs | Carry-forward/rollover of account entries, legacy predecessor | Uses Accounts_Entry_POST_CLS | — | Unknown |

**Batch observations (General_Ledger):** see also *Solution Structure* for the two-tree situation. **Discovery gap found and corrected**: a plain `class X : Form` grep misses forms inheriting a custom base class — this project has two such chains (`Project_ControlLIB.MyForm : Form` used by the current tree's journal-entry forms, and a 2-level chain `TempComplex_DayRegistrationFrmSP : Form` → `Complex_DayRegistrationFrmSP`/`EditComplex_DayRegistrationFrmSP` in the legacy tree) — bringing the true count to **58**, not the 54 a naive scan finds. The current tree explicitly excludes several physically-present files from compilation via `<Compile Remove>` (`Profit_Loss_StatFRMNew.cs`, `Incoming_StatementByFinancialPeriod.cs`, `Incoming_StatementByDate.cs`, `Incoming_Statement.cs`) — dead code as far as the build is concerned. Heavy duplication within the legacy tree (`Master_Account_Details` vs `_Details2`, `_DetailsTarseed` vs `_DetailsTarseed2`; the class name `NewAccount_EntryFRM` is declared identically in three different files/folders) — strong evidence of copy-paste versioning via folder renaming ("New folder", "OLD" suffixes) instead of source control. Forms rarely touch SQL directly; almost all delegate to a `*_CLS` business-logic class colocated in the project. A handful of report-print forms use the older typed-DataSet/TableAdapter + RDLC pattern with commented-out `TableAdapter.Fill` calls (`MasterAccountPrintFRM`, `Master_CostCenterPrintFRM`, `ReportTest`) — these reports may not load live data. No direct `My_DbContext` usage found in any of the 58 form files. Naming: form classes suffixed `FRM`/`Frm`/`Frm_RPT`/`ListFrm`; Arabic terms transliterated directly into English identifiers (`Da2n`=دائن/credit, `Madin`=مدين/debit, `Tar7eel`=ترحيل/carry-forward, `Tarseed`=ترصيد/balance-posting, `Mezan_Morgaa`=ميزان مراجعة/trial balance) — this transliteration convention recurs across ProductionsRates and other modules too.

### ProductionsRates

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| Equivalent_Machine_ProblemFRM | Employee_Production/Equivalent_Machine_ProblemFRM.cs | Unlocalized title; by name, tracks "equivalent machine" allocation/issue for employee production | Unknown | — | Unknown |
| Emp_Production_RateFrm | Employee_Production/Emp_Production_RateFrm.cs | "تقرير معدلات إنتاج العامل" (employee production rate report). **Not found by a naive `: Form` search** — base class is `Connection4It.Template_Frm` | BtnSearch, BtnSaveAddedValue; uses `operations` | — | Unknown |
| Item_EstlamProduction_balance | EstlamProduction/Item_EstlamProduction_balance.cs | Balance of items received into production ("Estlam"=receiving); Arabic title unreadable (mojibake) | `new operations(1)` | — | Unknown |
| Item_SendToEstlamProduction_FRM | EstlamProduction/Item_SendToEstlamProduction_FRM.cs | "إرسال الأصناف إلى" — send items into the receiving/production stage | Unknown | — | Unknown |
| EditBalance_FRM | EstlamProduction/New folder/EditBalance_FRM.cs | "تعديل رصيد الصنف" — manual balance adjustment | Unknown | — | Unknown |
| Estlam_Item_List_Qnty_ | Estlam_Items/Estlam_Item_List_Qnty_.cs | List of received item quantities (Arabic title unreadable) | `new operations(1)` | — | Unknown |
| Details_Item_After_Analysis_balance | Fa7s_Dept/Details_Item_After_Analysis_balance.cs | Detail balance of items after inspection/analysis ("Fa7s"=inspection); Arabic title unreadable | Unknown | — | Unknown |
| Item_After_Analysis_balance | Fa7s_Dept/Item_After_Analysis_balance.cs | Item balance after inspection/analysis; Arabic title unreadable | `new operations(1)` | — | Unknown |
| Item_Fa7s_balance | Fa7s_Dept/Item_Fa7s_balance.cs | Inspection-department item balance; Arabic title unreadable | `new operations(1)` | — | Unknown |
| Item_SendToTasheel_WorkOut_FRM | Fa7s_Dept/Item_SendToTasheel_WorkOut_FRM.cs | "إرسال الأصناف إلى" — send items to "Tasheel" outsourcing stage | Unknown | — | Unknown |
| Item_SendToTatrez_FRM | Fa7s_Dept/Item_SendToTatrez_FRM.cs | "إرسال الأصناف إلى" — send items to embroidery (Tatreez) stage | Unknown | — | Unknown |
| Item_AnalysisFRM | Fa7s_Dept/Item_ِAnalysisFRM.cs | "تحليل الأصناف" — item analysis/inspection entry | Unknown | — | Unknown |
| Form1 | Form1.cs | Unlocalized default title — stub/unused | None | — | — |
| Maintaining_Item_ListFRM | Maintaing_Machine/Maintaining_Item_ListFRM.cs | "قائمة بعمليات صيانة الميكنة" — list of machine maintenance operations | Uses Maintaining_Item_ListCLS | — | Unknown |
| Maintaining_OperationsFrm | Maintaing_Machine/Maintaining_OperationsFrm.cs | "عمليات الصيانة" — record a machine maintenance operation | Uses Maintaining_OperationsCLS | — | Unknown |
| Matwa_ListFRM | Matwa_Seda/Matwa_ListFRM.cs | "قائمة المطواة" — warp-beam ("Matwa") list | Unknown | — | Unknown |
| Matwa_SedaProductionRate | Matwa_Seda/Matwa_SedaProductionRate.cs | "معدلات إنتاج السدا" — warp/"Seda" production rates | Unknown | — | Unknown |
| Matwa_Production_Send | Matwa_Seda/SendMatwa/Matwa_Production_Send.cs | "إرسال المطاوى إلى ماكينات النسيج" — send warp beams to weaving machines | Unknown | — | Unknown |
| NewOrderWork_FRM | OrderWorking/NewOrderWork_FRM.cs | "إضافة أمر التشغيل" — add production/work order | Opened with `(Id, bool)` constructor | — | Unknown |
| OrderWorkListFRM | OrderWorking/OrderWorkListFRM.cs | "أوامر التشغيل" — work orders list | List of production orders | NewOrderWork_FRM | Unknown |
| EmpProductionRate_ | ProductionRate/EmpProductionRate_.cs | "معدلات إنتاج التطريز" — embroidery production rates | Unknown | — | Unknown |
| MachineTatreezFrm | ProductionRate/Machine_List/MachineTatreezFrm.cs | "بيان بمكينات التطريز" — embroidery machines master list | Unknown | — | Unknown |
| MachineTextileFrm | ProductionRate/Machine_List/MachineTextileFrm.cs | "بيان بمكينات نسيج المصنع" — factory weaving machines master list | Unknown | — | Unknown |
| AddAtalat | Production_Atalat/AddAtalat.cs | "العطلات" — add/edit a holiday/off-day | Unknown | — | Unknown |
| AtalatListFrm | Production_Atalat/AtalatListFrm.cs | "العطلات" — holidays list | Opens AddAtalat | AddAtalat | Unknown |
| Item_SendToFRM | Tashteeb_Dept/Item_SendToFRM.cs | "إرسال الأصناف إلى" — send items from finishing (Tashteeb) dept onward | Unknown | — | Unknown |
| Item_Tashteeb_balance | Tashteeb_Dept/Item_Tashteeb_balance.cs | Finishing-department item balance; Arabic title unreadable | `new operations(1)` | — | Unknown |
| Item_Tatreez_balance | Tatreez_Dept/Item_Tatreez_balance.cs | Embroidery-department item balance; Arabic title unreadable | `new operations(1)` | — | Unknown |
| Textile_Efficiency_DetailsLehaf | Textile_Efficiency_RPT/Lehaf/Textile_Efficiency_DetailsLehaf.cs | "معدلات وكفائة النسيج" — weaving rates & efficiency detail, "Lehaf" (blanket) product line | Unknown | — | Unknown |
| Textile_EfficiencyWabara | Textile_Efficiency_RPT/Wabara/Textile_EfficiencyWabara.cs | "معدلات وكفائة النسيج" — "Wabara" product-line efficiency report | Unknown | — | Unknown |
| Textile_Efficiency_DetailsWabara | Textile_Efficiency_RPT/Wabara/Textile_Efficiency_DetailsWabara.cs | "معدلات وكفائة النسيج" — Wabara efficiency detail variant | Unknown | — | Unknown |
| Textile_ProductionRateSearch | Textile_ProductionRateSearch.cs | "معدلات إنتاج عمال النسيج" — weaving workers' production rates search/report | `new operations()` | — | Unknown |
| Textile_Production_Rates | Textile_Production_Rates.cs | "معدلات إنتاج النسيج" — weaving production rates, main entry/report form | Unknown | — | Unknown |
| Textile_Production_Rates_OrderWabara | Textile_Production_Rates/New folder/Textile_Production_Rates_OrderWabara.cs | Same title as above — Wabara order-based variant | `new operations()` | NewOrderWork_FRM(id,true), Textile_Item_AtalatDetails | Unknown |
| Textile_Item_AtalatDetails | Textile_Production_Rates/Textile_Item_AtalatDetails.cs | "تفاصيل الإنتاج" — production/holiday-adjustment detail | `new operations()` | — | Unknown |
| Textile_Production_Rates_New | Textile_Production_Rates/Textile_Production_Rates_New.cs | Same title — newer variant of main weaving production-rate form (duplicate) | Unknown | — | Unknown |
| Textile_Production_Rates_OrderLehaf | Textile_Production_Rates/Textile_Production_Rates_OrderLehaf.cs | Same title — Lehaf order-based variant | Unknown | NewOrderWork_FRM(id,true) | Unknown |
| Textile_Production_Rates_OrderWabara_ | Textile_Production_Rates/Textile_Production_Rates_OrderWabara_.cs | Same title — another Wabara order-based variant (trailing `_`, near-duplicate) | Unknown | — | Unknown |
| Wardiat | Wardiat.cs | "بيان ورديات المصنع" — factory shift/roster report | Uses WardiatCLS | — | Unknown |
| Add_Requests_Work_Out | Work_Out/Add_Requests_Work_Out.cs | **Excluded from build.** Add an outsourcing ("Work Out") request | Unknown | — | Unknown |
| Draws_ListFRM | Work_Out/Draws_ListFRM.cs | **Excluded from build.** Non-partial class (no Designer file); list of "draws" related to outsourcing | Unknown | — | Unknown |
| Edit_Import_ItemQnty | Work_Out/Edit_Import_ItemQnty.cs | **Excluded from build.** Edit imported item quantities for outsourcing | Unknown | — | Unknown |
| Make_Invoices_Work_Out | Work_Out/Make_Invoices_Work_Out.cs | **Excluded from build.** Create supplier invoices for outsourced work | `new operations()` | opened by Receive_Destruction_Work_Out, Receive_Requests_Work_Out | Unknown |
| frm_supplier_addqu_ | Work_Out/New Folder/DestructionSales/suppliers_quantitiy/frm_supplier_addqu_.cs | **Excluded from build.** Add supplier "destruction sales" quantity | Raw SQL SELECT (string-concatenated params) | — | suppliers_Destruction |
| Receive_Destruction_Work_Out | Work_Out/New Folder/Receive_Destruction_Work_Out.cs | **Excluded from build.** Receive destruction/scrap goods from outsourcing | Unknown | Make_Invoices_Work_Out | Unknown |
| Receive_Requests_Work_Out | Work_Out/Receive_Requests_Work_Out.cs | **Excluded from build.** Receive outsourcing requests | Unknown | Make_Invoices_Work_Out | Unknown |
| Request_To_Work_Out | Work_Out/Request_To_Work_Out.cs | **Excluded from build.** Request items be sent out for outsourcing | Unknown | — | Unknown |
| Work_Out_Reports_Receive | Work_Out/Work_Out_Reports_Receive.cs | **Excluded from build.** Report of goods received from outsourcing | Unknown | — | Unknown |
| Work_Out_Reports_SendANDBalance | Work_Out/Work_Out_Reports_SendANDBalance.cs | **Excluded from build.** Report of goods sent to outsourcing & remaining balance | Unknown | — | Unknown |

**Batch observations (ProductionsRates):** department-per-folder structure using transliterated Arabic module names (`Fa7s_Dept`=inspection, `Tashteeb_Dept`=finishing, `Tatreez_Dept`=embroidery, `Matwa_Seda`=warp beam, `Estlam_Items`/`EstlamProduction`=receiving, `Work_Out`=outsourcing/subcontracting). **The entire `Work_Out` (outsourcing) feature — 10 of the 49 discovered form files — is excluded from compilation** via explicit `<Compile Remove>` entries — present on disk and cross-referencing each other as if functional, but dead code from the current build's perspective; the single largest technical-constraint finding in this project. **Discovery gap found and corrected**: `Emp_Production_RateFrm` inherits `Connection4It.Template_Frm` rather than `Form` directly, bringing the true count to **49**, not 48. Mixed DAL: many `*_balance` forms call `operations` with an integer argument (`new operations(1)`, suggesting a module/company-id parameter); other areas delegate to dedicated `*CLS` helper classes. No `My_DbContext` usage found in any active (non-excluded) form. **Encoding issue**: most Arabic captions in this project's Designer.cs files render as `????` mojibake (legacy ANSI/Windows-1256), unlike General_Ledger/ConstructionLib where Arabic renders correctly — purpose for affected forms was inferred from file/folder names, flagged as speculative where noted. At least 5 "Textile_Production_Rates*" forms share the identical window title — apparently product-line-specific or iterative variants never consolidated.

### ConstructionLib

| Form/Class | File | Purpose | Key Operations | Navigates To | DB Tables/Procs |
|---|---|---|---|---|---|
| AddCustomerOwner | Construction/CustomerOwner/AddCustomerOwner.cs | **Excluded from build** (superseded by AddCustomerOwnerList); title "المشروعات" (Projects) — likely copy-paste leftover | Unknown | — | Unknown |
| AddCustomerOwnerList | Construction/CustomerOwner/AddCustomerOwnerList.cs | "أملاك العميل" — add/edit a customer-owner-property record | Direct SqlConnection/SqlCommand; also `AccountModelsLib.operations`; instantiates CustomerOwnerCLS, ProjectUnitesCLS, Project | opened by CustomerOwnerListFrm | Direct SQL + CustomerOwnerCLS |
| CustomerOwnerListFrm | Construction/CustomerOwner/CustomerOwnerListFrm.cs | "أملاك العملاء" — list/search customer properties | SqlConnection/SqlCommand; CustomerOwnerCLS | AddCustomerOwnerList (Add/Edit) | Direct SQL + CustomerOwnerCLS |
| DebitFrm | Construction/Debit/DebitFrm.cs | "مديونية العملاء" — customer dues/debit report by project/building/spaces-group and financial period | Uses DebitCLS; reusable UserControls | — | Via DebitCLS (likely ConsCustomerDebitList) |
| Due_PeriodFrm | Construction/Due_PeriodFrm.cs | Manage financial/due periods for construction billing (title mojibake in Designer.cs) | Uses Due_PeriodCLS, Get_Year_ConsDue_Period_Id | Cross-module link to General Ledger's P&L reporting (via commented-out Profit_Loss_StatCls call) | ConsCustomerDebitList (via Due_PeriodCLS SQL) |
| AddProjectServices | Construction/ProjectServices/AddProjectServices.cs | "خدمات المشروعات" — add/edit a project service line | Uses ProjectServicesCLS | opened by ProjectServicesListFrm | Via ProjectServicesCLS |
| ProjectServicesListFrm | Construction/ProjectServices/ProjectServicesListFrm.cs | "خدمات المشروع" — list project services | SqlConnection/SqlCommand; ProjectServicesCLS | AddProjectServices; AddCurrencyFactor | Direct SQL + ProjectServicesCLS |
| AddProjectSpaces | Construction/ProjectSpaces/AddProjectSpaces.cs | "وحدات المشروع" — add/edit a project space/unit. Instantiates `ProjectUnitesCLS` not `ProjectSpacesCLS` despite folder/class name — overlap with the ProjectUnit module (speculative: may be dead/unused) | Uses ProjectUnitesCLS | — | Via ProjectUnitesCLS |
| ProjectSpacesListFrm | Construction/ProjectSpaces/ProjectSpacesListFrm.cs | "وحدات المشروع" — list project spaces/units. Opens `AddProjectUnites`, not its own sibling `AddProjectSpaces` (same naming overlap) | SqlConnection/SqlCommand; ProjectUnitesCLS | AddProjectUnites (Add/Edit) | Direct SQL + ProjectUnitesCLS |
| AddProjectSpacesGroup | Construction/ProjectSpacesGroup/AddProjectSpacesGroup.cs | "إضافة مجموعة" — add a project spaces group | Simple field-bound add form; no direct SQL/DAL visible at Form level | — | Unknown |
| ProjectSpacesGroupListFrm | Construction/ProjectSpacesGroup/ProjectSpacesGroupListFrm.cs | "مجموعات مساحة الوحدات" — list spaces groups | SqlConnection/SqlCommand; ProjectSpacesGroupCLS | AddProjectSpacesGroup | Direct SQL + ProjectSpacesGroupCLS |
| AddProjectUnites | Construction/ProjectUnit/AddProjectUnites.cs | "وحدات المشروع" — add/edit a project unit | Uses ProjectUnitesCLS | — | Via ProjectUnitesCLS |
| ProjectUnitesListFrm | Construction/ProjectUnit/ProjectUnitesListFrm.cs | "وحدات المشروع" — list project units | SqlConnection/SqlCommand; ProjectUnitesCLS | AddProjectUnites; AddCurrencyFactor | Direct SQL + ProjectUnitesCLS |
| AddProject | Construction/ProjectsList/AddProject.cs | "المشروعات" — add/edit a construction project; validates name uniqueness before insert | Uses ProjectsCLS | — | Table ConsProject (via ProjectsCLS) |
| ProjectsListFrm | Construction/ProjectsList/ProjectsListFrm.cs | "قائمة المشروعات" — list/manage projects | Raw SQL `Select * from ConsProject Order By Name` via ProjectsCLS | AddProject; AddCurrencyFactor | Table ConsProject |
| AddResidentialbuilding | Construction/Residentialbuilding/AddResidentialbuilding.cs | "العمارة - المبنى السكنى" — add/edit a residential building/tower | Uses ResidentialbuildingCLS | — | Via ResidentialbuildingCLS |
| ResidentialbuildingListFrm | Construction/Residentialbuilding/ResidentialbuildingListFrm.cs | "المشروعات - المبانى - العمارات" — list buildings under projects | SqlConnection/SqlCommand; ResidentialbuildingCLS | AddResidentialbuilding | Direct SQL + ResidentialbuildingCLS |
| AddServicesGroupsList | Construction/ServicesGroupsList/AddServicesGroupsList.cs | "مجموعة الخدمات" — add a services group | Uses ServicesGroupsListCLS | — | Via ServicesGroupsListCLS |
| ServicesGroupAssignFrm | Construction/ServicesGroupsList/ServicesGroupAssignFrm.cs | Assign services/units to a services group per project (title mojibake). Has hardcoded `long Project_Id = 9;` | Direct raw SQL INSERT/SELECT/DELETE (string-concatenated) via `operations opp`, ServicesGroupsListCLS | — | ConsServicesGroups, ConsServicesUnites, ConsProjectUnites |
| ServicesGroupsListFrm | Construction/ServicesGroupsList/ServicesGroupsListFrm.cs | "مجموعة الخدمات" — list services groups | SqlConnection/SqlCommand; ServicesGroupsListCLS | AddServicesGroupsList | Direct SQL + ServicesGroupsListCLS |
| AddService | Construction/ServicesList/AddService.cs | "قائمة الخدمات" — add/edit a service (title reuses the list form's title, likely copy-paste) | Uses ServicesCLS | — | Via ServicesCLS |
| ServicesListFrm | Construction/ServicesList/ServicesListFrm.cs | "قائمة الخدمات" — services list | SqlConnection/SqlCommand; ServicesCLS | AddService; AddCurrencyFactor | Direct SQL + ServicesCLS |
| AddUnitType | Construction/UnitType/AddUnitType.cs | "نوع الوحدة" — add/edit a unit type; validates name uniqueness | Uses UnitTypeCLS | — | Via UnitTypeCLS |
| UnitTypeListFrm | Construction/UnitType/UnitTypeListFrm.cs | "أنواع الوحدات" — unit types list | SqlConnection/SqlCommand; UnitTypeCLS | AddUnitType; AddCurrencyFactor | Direct SQL + UnitTypeCLS |
| AddMainAccountSetting | MainAccountSetting/AddMainAccountSetting.cs | Link a GL main account to a construction "account type" setting; title "خدمات المشروعات" (mismatched, reused from ProjectServices — copy-paste) | Uses MainAccountSettingCLS | opened by MainAccountSettingListFrm | Via MainAccountSettingCLS |
| CustomerSubAccountListFrm | MainAccountSetting/CustomerSubAccountListFrm.cs | "ربط حساب العملاء" — link customer GL sub-accounts, list form. **Not found by a naive `: Form` search** — base class is `Project_ControlLIB.MyForm` | Uses `operations`; opens edit dialog | CustomerSubAccountListFrmEdit (ShowDialog) | Direct SQL/operations |
| CustomerSubAccountListFrmEdit | MainAccountSetting/CustomerSubAccountListFrmEdit.cs | "ربط حساب العميل" — edit/link a single customer's GL sub-account | Uses MainAccountSettingCLS | opened by CustomerSubAccountListFrm | Via MainAccountSettingCLS |
| MainAccountSettingListFrm | MainAccountSetting/MainAccountSettingListFrm.cs | "ربط الحسابات" — list/manage main-account settings. **Not found by a naive `: Form` search** — base class is `Project_ControlLIB.MyForm` | Uses `operations` | AddMainAccountSetting (ShowDialog, both add & edit) | Direct SQL/operations |
| SupplierSubAccountListFrm | MainAccountSetting/SupplierSubAccountListFrm.cs | Title "ربط حساب العملاء" (says "customers" though this is the supplier form — mismatched/copy-paste). **Not found by a naive `: Form` search** — base class is `Project_ControlLIB.MyForm` | Uses `operations`; opens edit dialog | SupplierSubAccountListFrmEdit (ShowDialog) | Direct SQL/operations |
| SupplierSubAccountListFrmEdit | MainAccountSetting/SupplierSubAccountListFrmEdit.cs | "ربط حساب المورد" — edit/link a single supplier's GL sub-account | Uses MainAccountSettingCLS | opened by SupplierSubAccountListFrm | Via MainAccountSettingCLS |

**Batch observations (ConstructionLib):** consistent CRUD master/detail naming convention — nearly every entity has an `Add<Entity>` modal form paired with an `<Entity>ListFrm` grid form, the list form opening the Add form via `ShowDialog()` for both create and edit. **Discovery gap found and corrected**: three forms in `MainAccountSetting/` inherit `Project_ControlLIB.MyForm` rather than `Form` directly, bringing the true count to **30**, not 27 — finding them also resolved two previously-"Unknown" navigation edges. Shared DAL: entity CRUD forms delegate persistence to a matching `<Entity>CLS` wrapper class, which calls into `AccountModelsLib`/`operations` helpers wrapping raw ADO.NET; list forms additionally instantiate `SqlConnection`/`SqlCommand` directly themselves — no strict separation of concerns. **No `My_DbContext`/EF Core usage found anywhere in this project** — ConstructionLib appears to be raw-SQL/DAL-only. Table names use a `Cons*` prefix (`ConsProject`, `ConsCustomerDebitList`, `ConsServicesGroups`, `ConsServicesUnites`, `ConsProjectUnites`). Naming/module overlap between "ProjectSpaces" and "ProjectUnit" folders suggests they're either the same concept split in two, or `AddProjectSpaces` is dead/unused code (not fully verified). Several Add forms reuse a mismatched Arabic title copy-pasted from a different form (`AddCustomerOwner`, `AddMainAccountSetting`, `AddService`, `SupplierSubAccountListFrm`). `ServicesGroupAssignFrm.cs` has a hardcoded `long Project_Id = 9;` with a garbled Arabic comment. A shared external `AddCurrencyFactor` dialog (from Account_Common_LIB) is opened via `ShowDialog()` from at least 5 list forms here — a cross-cutting "currency factor" concern reused across most master-list screens solution-wide. `Due_PeriodFrm` cross-references `Profit_Loss_StatCls`, a coupling point between the Construction module and General Ledger's final-accounts/reporting area.

**Note on total form count:** the re-verified totals across the three projects above are General_Ledger 58, ProductionsRates 49, ConstructionLib 30 (137 total, up from the 129 a naive `: Form`-only scan finds) — this confirms the solution-wide caveat at the top of this section: **treat any naive-grep-derived form count in this document as a lower bound**, since custom base classes (`Project_ControlLIB.MyForm`, `Connection4It.Template_Frm`, and further subclass chains of those) are used throughout the solution and are easy to miss.

---

## Screen Navigation

The solution does not have one single navigation tree — it has **several independent MDI hubs**, one per deployable EXE, each opening dozens to hundreds of child forms by menu click (`new SomeForm().Show()`/`.ShowDialog()` with `MdiParent = this`, or a plain `ShowDialog()` for non-MDI dialogs). Only the confirmed top-level structure is given below; the full 700+-node graph is not reproduced here (see the per-project tables above for individual "Navigates To" edges — do not invent edges not listed there).

```text
Main ERP app (AccountsERPSystem / AccountsERPSystemNet10)
 Program.cs
  └─ mylogin.testlogin() [LoginPro]           — authenticate
  └─ Accounts.LoadingFrm.ShowDialog()          — load system parameters
  └─ if PointOfSale == 1:
       └─ MainFrmPOS [Accounts]                — POS/restaurant MDI hub (~70 menu actions)
     else:
       └─ MainFrm_AccountUpdate [Advanced_Account, ~9000 lines, 718 nav calls]
            — the REAL main menu of the ERP; opens (non-exhaustively):
            ├─ General ledger / journal entries (General_Ledger, General_LedgerAccount-under-Accounts)
            ├─ Trial balance / income statement / final accounts (Mezan_Morgaa*, Incoming_Statement*, Profit_Loss_Stat*)
            ├─ Invoicing (InvoicesFrm/*, Requests/*, PointOfSales/*)
            ├─ Customers/Suppliers (Customer_Supplier/*)
            ├─ Checks (AccountCheckMoney/*)
            ├─ Cars/workshop (CarsSystemLib/*)
            ├─ Construction (ConstructionLib/*)
            ├─ Production rates (ProductionsRates/*)
            ├─ Reservations (ReservationSystemLIB/*)
            ├─ Reports (Reports/*, Profit_Loss_Statement/*)
            ├─ Work-In / Work-Out outward processing (Work_In/*, Work_Out/*)
            └─ Beauty-center appointments (BeautyCenter/*)

Standalone WinExe tools (each with its own independent MDI hub, not reachable from the main ERP app):
 Alter_DataBase → LOGINFRM → MainFrm_DB          — DB admin/migration/permissions tool
 Stores → MainFrm_Store                          — warehouse/inventory app (currently not launched — see Known Technical Constraints)
 BarCodeCreator → BarCodeCreator_MainFrm          — barcode label tool
 GHospital_Care → Session.Login → MainWindow      — vendored hospital system (own accounting sub-tree)
 Clinic / ClinicPatient → Main_Frm_Clinic          — standalone clinic apps
 FreeMedical → MainFrm                            — standalone free-treatment tracker
 EASY E-INVOICE → frmshocecompany                 — e-invoice desktop client
 DataBaseSettining → DBConfig                     — standalone DB-config tool
 EncryptionCA → encryption (Main.cs) → Form1       — cipher demo tool
 administration → MAIN_FORM_IN_ADMINI → MenuFrmAdmin/frmadmin/ADDNEWUSER — admin tool
 General_Ledger (legacy tree) → MainFrm            — old standalone GL app, not built by the solution

Prototype Blazor stack (Razors\ group — not reachable from any WinForms shell at runtime):
 AccountERP.Web / AccountERP.Web.Client → Login.razor, Home.razor, Counter*.razor,
   Customers/CustomerDataList*.razor, Invoices/Invoice*.razor, Weather*.razor (template scaffolding still present)
```

Within `MainFrm_AccountUpdate`, `MainFrm_Store`, `MainWindow` (GHospital_Care) and `MainFrm_DB`, a recurring pattern is a **permission-gated dynamic dispatch**: menu items are checked against `PERMATIONS`/`FORMS_DETAILES`/`SysConfigMenusPERMATIONS`-family tables before being shown/enabled, and several forms across the solution have **no `new X()` call found anywhere in their own project** — these are very likely opened by reflection/dynamic instantiation using a form name stored in one of those permission tables, rather than a hardcoded menu handler. This was independently observed in `Stores`, `CarsSystemLib`, and is consistent with the permission architecture confirmed in `Alter_DataBase`.

---

