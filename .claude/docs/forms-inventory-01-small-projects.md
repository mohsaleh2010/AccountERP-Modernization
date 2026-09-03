## Forms Inventory — Part 1: Login, Reporting, Clinic-family, and other small/medium WinForms projects

**Scope and method note:** every WinForms project in the solution was scanned for classes declaring `class X : Form` (or `: System.Windows.Forms.Form`), directly. This misses forms that inherit a **custom intermediate base class** instead — confirmed real cases: `Project_ControlLIB.MyForm` (used across `Connection4It`, `Account_Common_LIB`, `CarsSystemLib`, `General_Ledger`, `ConstructionLib`, `Advanced_Account`'s `MainFrm_AccountUpdate`, and more), `Connection4It.Template_Frm` (used by at least one `ProductionsRates` form), and DevExpress's `XtraForm` (used throughout `EASY E-INVOICE`, which is almost entirely undercounted by this method — see its entry below). Where a batch's re-verification pass caught these, the corrected count and the specific forms are noted; where it wasn't re-verified, treat the count as a **lower bound**. Total forms found via the direct `: Form` pattern alone: **≈712** across 34 WinForms-bearing projects (Razor/Blazor pages in the `Razors\` group are a different UI paradigm and are listed separately in `screen-navigation.md`, not here).

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

Also reviewed (non-Form DB-access core classes in this project): `DBContext\operations.cs` wraps raw ADO.NET SqlConnection access, auto-validates the connection string on construction and pops `ConnErrorTest` on failure, plus a license/expiration check (`Common.ExpirationVersion()`); `DBContext\ConnConfigInfoXML.cs` reads/writes `CommonFiles\ConnInfo.xml` (the persistent store behind `DBConfig`, contains credential-shaped fields — values not reproduced); `DBContext\EncryptionClass\EncryptProcess.cs` is the simple Caesar-cipher-style shift cipher described in `CLAUDE.md`'s *Database Access* — not real cryptography.

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

**Important scope note:** the "1" form-count for this project (from the plain `: Form` grep) is a severe undercount. This project actually contains a large number of additional forms (frmBranch, frmCustomer, frmProduct, FrmProductRec/Send, FrmCurrncy, MainForm/MainForm2/XtraForm1‑3, FrmIqrar, FrmSendExcell, FrmSendEmanualFrm, FrmSearchEinvoice, FrmAddItem, WaitForm1, License/TRIAL) that derive from **DevExpress's `XtraForm`**, not plain `Form`. There is also a `SDK\` folder implementing the Egyptian Tax Authority e-invoice/e-receipt web-service integration, and a `Test\` folder with a DevExpress UI-kit demo. **Full cataloguing of the XtraForm screens is flagged under `CLAUDE.md`'s *Areas Requiring Further Investigation*.**

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

### Batch observations — small/medium projects
*(Applies across LoginPro/Reporting_System/Clinic/ClinicPatient/FreeMedical/Store_LIB/AccountCheckMoney/administration/AbsencePrecenseProj/Advanced_Account/BarCodeCreator/AccountsEInvoicesLIB/EncryptionCA/AccountCommonDataLIB/Project_ControlLIB/PrintableListView/General_LedgerAccount/BackUp_Project/DataBaseSettining/EASY E-INVOICE/AccountsERPSystem/AccountsERPSystemNet10.)*

- **Pervasive duplication:** many "New"/"Old"/"_2"-suffixed sibling classes do nearly the same thing (LoginPro's `Authentication`/`Authentication_`/`AuthenticationOld`/`Authentication__2`; `DBConfig` re-implemented almost verbatim in `AccountCommonDataLIB` and `DataBaseSettining`; `frmadmin`/`ADDNEWUSER` duplicated between `LoginPro` and `administration`).
- **Cross-project linked source files:** several "small" projects compile in `.cs` files whose namespace doesn't match the containing project (Clinic project contains `Account_Common_LIB.Omra_Tours*` and `Accounts.PrintInvoice`; FreeMedical embeds `PrintableListView.PrintOptions` and `Project_ControlLIB.MohSalehCNTRL` stub forms) — classic old-style "Add Existing Item as Link" reuse instead of project/package references.
- **DAL pattern is uniform but risky:** virtually all data access goes through the hand-rolled `operations` class exposing `return_dataset`/`return_DataTable`/`insert_me`/`update_me`/`delete_me`, fed raw string-concatenated SQL everywhere observed — no parameterized queries seen, so SQL-injection exposure is the norm, not an exception.
- **Error handling idiom:** dominant pattern is bare `try { } catch { }` or `catch (Exception ee) { }` with `ee` unused — failures are silently swallowed rather than logged, except where a `MessageBox.Show` surfaces a DB error to the user.
- **Encoding inconsistency:** several older files (Store_LIB `unit.cs`/`unit_.cs`, AbsencePrecenseProj `InOutRegistration*.cs`, `administration\frmadmin.cs`, `LoginPro\AuthenticationOld.cs`) show Arabic literals as mojibake when read as UTF-8, suggesting a legacy ANSI/Windows-1256 codepage; newer files use clean UTF-8 Arabic — a mixed-encoding codebase.
- **Legacy/modern coexistence:** v2.0/v3.5/v4.5/v4.8 non-SDK `.csproj` files (FreeMedical, AbsencePrecenseProj, Clinic, ClinicPatient, DataBaseSettining, EASY E-INVOICE, EncryptionCA) sit in the same solution as modern SDK-style, multi-targeted projects — an incremental, still-in-progress .NET Framework → modern .NET migration.
- **Naming is inconsistent throughout:** PascalCase/camelCase/ALL_CAPS coexist even within one file; class names and file names frequently diverge (e.g. `Users\Authentication_.cs` contains class `Authentication__2`; `Store_LIB\1_POS\unit.cs` contains class `Units_List`).
