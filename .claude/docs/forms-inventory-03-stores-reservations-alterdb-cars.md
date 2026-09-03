## Forms Inventory — Part 3: Stores, ReservationSystemLIB, Alter_DataBase, CarsSystemLib

*Methodology note: source files in these four projects are inconsistently encoded (UTF-8 vs. legacy Windows-1256 Arabic, no BOM); captions were extracted via per-file encoding auto-detection to avoid mojibake.*

### Stores
**Startup form:** `MainFrm_Store` (per the disabled/commented-out `Main11()` method and the login→MDI flow it shows) — **but** the active `Main()` in `Stores/Program.cs` has its `Application.Run(MainFrm)` call commented out, so as checked into this tree, the compiled entry point does not actually launch any UI form. Looks like an in-progress refactor left mid-way — see `CLAUDE.md`'s *Known Technical Constraints*.

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
- A genuine **DB administration/maintenance/migration tool** — does live `CREATE TABLE`/`ALTER TABLE`/`DROP TABLE`, seeds default data, manages users/permissions (`USER_DETAILES`, `PERMATIONS`, `FORMS_DETAILES`).
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
