# Projects & Project Dependencies

## Projects

40 real projects (excluding the 2 solution-folder groupings) are part of `Accounts.sln`/`AccountsNet.sln`. "Target Framework" reflects the Release-configuration multi-target where applicable (Debug configurations mostly build `net10.0-windows` only).

| Project | Type | Target Framework | Purpose |
|---|---|---|---|
| LoginPro | Library (WinForms) | net10.0-windows;net48 | Authentication/login screens, change-password, per-user/per-branch/per-form permission admin. Referenced by nearly every EXE shell for its `mylogin.testlogin()` login flow. |
| PrintableListView | Library (WinForms) | net10.0-windows;net48 | Custom printable `ListView` control + its print-options dialog; used across many report/list screens solution-wide. |
| Connection4It (`connectionit`) | Library (WinForms) | net10.0-windows;net48 | The most heavily-referenced shared library in the solution: DB connection helpers, dozens of shared lookup/picker dialogs (accounts tree, item/customer/supplier search, government regions, cost centers, financial periods), admin/permission forms, DB backup/config forms. Also unexpectedly contains a full clinic/pharmacy sub-module. |
| Account_Common_LIB | Library (WinForms) | net10.0-windows;net48 | Shared accounting library: banks/cash boxes, capital/partners, cost-center tree, currency factors, financial periods, fixed assets/depreciation, and the cash-voucher (`Order_Receipt_Cash`/`Sarf`) form family (many rewritten generations of the same screen). |
| General_Ledger | Library (WinForms) | net10.0-windows;net48 | General ledger / journal entries, trial balance ("Mezan Morgaa"), income statement, final accounts. Contains a second, legacy standalone `WinExe` copy of itself in `New folder\` (not built by the solution). |
| TafkeetProj | Library | net10.0;net48 | **Unknown / Not found in source** — 0 Form-derived classes found; referenced by `AccountModelsLib` and consumed widely, but its own purpose was not determined by this pass. |
| Store_LIB | Library (WinForms) | net10.0-windows;net48 | Inventory/store items, units of measure, barcode-print settings, item balance-with-expiry editing. |
| Reporting_System | Library (WinForms) | net10.0-windows;net48 | Standalone RDLC report screens (supplier/customer lists, expense reports, production-data price lists, item images). |
| Alter_DataBase | WinExe | net10.0-windows | Standalone DB administration/maintenance/migration tool: user & permission admin, schema/DB migrator (`SqlServerFullMigrator`), license/expiry & connection-string configuration, bulk data-seed/ALTER scripts. |
| Stores | WinExe | net10.0-windows;net48 | Standalone warehouse/inventory management app: dispatch ("Sarf") and receiving ("Estlam") orders, item cards, department transfers. **Its `Program.cs` `Main()` has `Application.Run` commented out** — see [Known Technical Constraints](known-constraints.md). |
| BarCodeCreator | WinExe | net10.0-windows;net48 | Standalone barcode-label creation/printing tool. |
| Project_ControlLIB | Library (WinForms) | net10.0-windows;net48 | Custom WinForms UI control toolkit — `MyButton`, `MyTextBox`, `MyComboBox`, `MyDataGridView`, `MyGroupBox`, etc., and the shared **`MyForm`** base `Form` class used (partially) across the solution. Not a "forms" project despite 2 stray Form-derived stub files. |
| ProductionsRates | Library (WinForms) | net10.0-windows;net48 | Textile/production module: weaving, embroidery, inspection ("Fa7s"), finishing ("Tashteeb"), machine maintenance, shift/roster, production-rate reports. Its entire `Work_Out` (outsourcing) feature (10 files) is excluded from the build. |
| ConstructionLib | Library (WinForms) | net10.0-windows;net48 | Construction/real-estate module: projects, buildings, units/spaces, services, customer property ownership, customer dues by period. |
| AccountModelsLib | Library | net10.0-windows;net10.0;net10.0-android;net48 | Core EF Core entity models (145 `DbSet<>` properties) **and** the base `operations` DAL class (raw ADO.NET) that almost everything else builds on. Multi-targets Android too, for reuse by the MAUI prototype. |
| General_LedgerAccount | Library (WinForms) | net10.0-windows;net48 | Effectively an empty stub project (1 form, itself empty) despite the name. |
| GHospital_Care (`HMIS\GHospital Care`) | WinExe | net10.0-windows;net48 | **Vendored third-party** hospital/clinic management system (vendor "GSoft") grafted with a copy of this solution's Arabic accounting subsystem. See [Solution Structure](overview.md#solution-structure). |
| AccountCheckMoney | Library (WinForms) | net10.0-windows;net48 | Bank-checks module: receive/issue/endorse checks, check status changes, opening-balance checks. |
| CarsSystemLib | Library (WinForms) | net10.0-windows;net48 | Car-service/workshop module: repair estimations, work orders, invoices (incl. e-invoice tax-portal submission), print layouts. Majority of its actual screens derive from `Project_ControlLIB.MyForm`, not bare `Form`. |
| BarCodeQR_GeneratorLIB | Library | net10.0-windows;net10.0;net48 | Barcode/QR generation library (built on `ZXing.Net`). |
| AccountERP.Web (`Razors\AccountERP.Web\AccountERP.Web`) | ASP.NET Core (Blazor Web App, server) | net10.0 | Prototype Blazor web front-end. |
| AccountERP.Web.Client (`Razors\AccountERP.Web\AccountERP.Web.Client`) | Blazor WebAssembly | net10.0 | Client half of the same Blazor web app. |
| AccountERP.MobileDesktop (`Razors\AccountERP.MobileDesktop`) | .NET MAUI Blazor Hybrid | net10.0-android;net10.0-windows10.0.19041.0 (+ios/maccatalyst) | Prototype cross-platform mobile/desktop shell hosting the shared Blazor UI. |
| ReservationSystemLIB | Library (WinForms) | net10.0-windows;net48 | Travel/tourism reservation module: visas, hotel bookings, flight tickets (+reversal), land transport, bus bookings, Hajj/Umrah packages — a repeating booking/list/print template across 6 travel-product types. |
| AccountCommonDataLIB | Library (WinForms) | net10.0-windows;net48 | DB connection/configuration layer (`DBConfig`, `ConnConfigInfoXML`, `EncryptProcess`), POS receipt-printing services, and Egyptian Tax Authority e-invoice portal submission helpers. |
| AccountERP.SharedUI (`Razors\AccountERP.SharedUI`) | Razor Class Library | net10.0 | Shared Blazor pages/components (Customers, Invoices, Login) reused by the Web/WebClient/MobileDesktop prototypes and referenced by `AccountsERPSystemNet10` for an in-progress `BlazorWebView` integration. |
| BackUp_Project | Library (WinForms) | net10.0-windows;net48 | System-tray-resident scheduled SQL Server backup utility for several hardcoded database names. |
| AccountsEInvoicesLIB | Library (WinForms) | net10.0-windows;net48 | Egyptian Tax Authority e-invoice module: item tax-code management, tax-branch/portal configuration. |
| administration | Library (WinForms) | net10.0-windows;net48 | User/permissions administration — substantially duplicates parts of `LoginPro`. |
| Advanced_Account | Library (WinForms) | net10.0-windows;net48 | Contains its own copy of **`MainFrm_AccountUpdate`** (10,623 lines, namespace `Accounts`) plus unrelated Spanish-labeled dashboard-template scaffolding (`GUI_MODERNISTA`) that looks like leftover UI-kit tutorial code. **Confirmed dead/disconnected for the live app**: neither `AccountsERPSystem.csproj`/`AccountsERPSystemNet10.csproj` nor `Accounts.csproj` has any `ProjectReference` to `Advanced_Account`, so this copy is never compiled into either EXE shell. The real, live MDI hub is the same-named, same-namespace class in the `Accounts` project itself — see `Accounts` row below. |
| Clinic | WinExe | .NET Framework 4.5 (legacy) | Standalone clinic/patient-visit management app; also links in unrelated Umrah/Hajj travel-booking forms from `Account_Common_LIB` via old-style file links. |
| ClinicPatient | WinExe | .NET Framework 4.5 (legacy) | Near-fork of `Clinic` with an added "drugs used" dimension. |
| DataBaseSettining (`DataBaseSetting`) | WinExe | .NET Framework 3.5 (legacy) | Standalone, earlier iteration of the DB-connection-configuration screen (predates the one now in `AccountCommonDataLIB`). |
| EASY E-INVOICE | WinExe | .NET Framework 4.8 (legacy, DevExpress-based) | Egyptian Tax Authority e-invoice desktop client; most of its actual screens use DevExpress `XtraForm`, not plain `Form` (undercounted by a naive scan — see `FORMS_INVENTORY.md`). |
| EncryptionCA | WinExe | legacy (non-SDK) | Small demo/utility app around the custom Caesar-cipher `EncryptProcess` class used solution-wide for "encrypting" stored credentials. |
| FreeMedical | WinExe | .NET Framework 2.0 (legacy) | Standalone free-treatment patient-tracking app. |
| AbsencePrecenseProj | Library | .NET Framework 3.5 (legacy, non-SDK) | HR attendance/absence registration module. |
| Accounts | Library (WinForms) | net10.0-windows;net48 | The largest single business/UI library (125 in-scope forms): invoicing, POS/restaurant, beauty-center appointments, work-in/work-out (outward processing), reports, special pricing. Hosts `MainFrmPOS` (POS MDI shell) **and** `AccountMainFrm\MainFrm_AccountUpdate` — confirmed (by `.csproj` inspection; see *Project Dependencies*) to be the actual, live main-menu MDI hub compiled into both EXE shells, ~15,300 lines split across `MainFrm_AccountUpdate.cs` (8,763 lines) + `MainFrm_AccountUpdate.Designer.cs` (6,554 lines), 620 `ToolStripMenuItem`/separator fields, 477 leaf menu actions with a directly-wired `.Click` handler resolving to a target form. See [`../.claude/docs/main-menu-screen-map.md`](../.claude/docs/main-menu-screen-map.md) for the full extracted menu→screen map. Referenced by both EXE shells. |
| AccountsERPSystem | WinExe | net10.0-windows | Main ERP EXE shell / entry point #1. |
| AccountsERPSystemNet10 | WinExe | net10.0-windows | Near-duplicate EXE shell / entry point #2, with in-progress `BlazorWebView`/DI wiring not yet used at runtime. See [Known Technical Constraints](known-constraints.md). |

---

## Project Dependencies

Core dependency hubs, most-referenced first (based on actual `<ProjectReference>` entries in every in-solution `.csproj`):

1. **Connection4It** (`connectionit`) — referenced directly or transitively by nearly every other project; the closest thing to a universal base library.
2. **Project_ControlLIB** — referenced by the large majority of UI projects (for shared controls and the `MyForm` base class).
3. **AccountModelsLib** — referenced by most data-touching projects (EF Core models + base `operations` DAL).
4. **AccountCommonDataLIB** — referenced by `LoginPro`, `Connection4It`, `Account_Common_LIB`, `General_Ledger`, `Store_LIB`, `Alter_DataBase`, `ReservationSystemLIB`, `AccountsERPSystem`, `AccountsERPSystemNet10`.

Two EXE shells sit at the top of the graph:

- **`AccountsERPSystem`** references: `AccountCheckMoney`, `AccountCommonDataLIB`, `Accounts`, `Account_Common_LIB`, `BarCodeQR_GeneratorLIB`, `CarsSystemLib`, `Connection4It`, `ConstructionLib`, `LoginPro`, `PrintableListView`, `ProductionsRates`, `Project_ControlLIB`, `Reporting_System`, `ReservationSystemLIB`, `Store_LIB`, `TafkeetProj`.
- **`AccountsERPSystemNet10`** references the same set **plus** `AccountModelsLib`, `General_LedgerAccount`, and `Razors\AccountERP.SharedUI` (the Blazor component library) — the only project reference distinguishing it from `AccountsERPSystem`.
- **`Accounts`** (the shared business library both shells host) references: `AccountCheckMoney`, `AccountModelsLib`, `Account_Common_LIB`, `BarCodeQR_GeneratorLIB`, `CarsSystemLib`, `Connection4It`, `ConstructionLib`, `General_Ledger`, `LoginPro`, `PrintableListView`, `ProductionsRates`, `Project_ControlLIB`, `Reporting_System`, `ReservationSystemLIB`, `Store_LIB`, `TafkeetProj`.

**Standalone leaf projects** — not referenced by any other in-solution project, i.e. self-contained EXE tools or libraries consumed only outside this solution's own reference graph: `EncryptionCA`, `DataBaseSetting`, `BackUp_Project`, `Alter_DataBase`, `Stores`, `BarCodeCreator`, `GHospital_Care`, `Clinic`, `ClinicPatient`, `FreeMedical`, `EASY E-INVOICE`, `AccountsEInvoicesLIB`, `administration`, `Advanced_Account` (**confirmed dead for the live app** — its own `MainFrm_AccountUpdate` copy is a disconnected duplicate never compiled into either EXE shell; the real, live MDI hub is the same-named/-namespaced class inside the `Accounts` project itself, which *is* formally referenced above. This was previously an open question in [Areas Requiring Further Investigation](investigations.md); it is now resolved).

**Razors group internal references:** `AccountERP.Web` → `AccountERP.Web.Client` + `AccountERP.SharedUI`; `AccountERP.Web.Client` → `AccountERP.SharedUI`; `AccountERP.MobileDesktop` → `AccountERP.SharedUI`; `AccountERP.SharedUI` → `AccountModelsLib`.