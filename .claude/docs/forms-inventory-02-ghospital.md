## Forms Inventory — Part 2: GHospital_Care (vendored third-party product)

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

### Batch observations (GHospital_Care)
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
