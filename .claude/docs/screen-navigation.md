# Screen Navigation

The solution does not have one single navigation tree — it has **several independent MDI hubs**, one per deployable EXE, each opening dozens to hundreds of child forms by menu click (`new SomeForm().Show()`/`.ShowDialog()` with `MdiParent = this`, or a plain `ShowDialog()` for non-MDI dialogs). Only the confirmed top-level structure is given below; the full 700+-node graph is not reproduced here (see the per-project tables in `forms-inventory.md` for individual "Navigates To" edges — do not invent edges not listed there).

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
 Stores → MainFrm_Store                          — warehouse/inventory app (currently not launched — see CLAUDE.md's Known Technical Constraints)
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

Within `MainFrm_AccountUpdate`, `MainFrm_Store`, `MainWindow` (GHospital_Care) and `MainFrm_DB`, a recurring pattern is a **permission-gated dynamic dispatch**: menu items are checked against `PERMATIONS`/`FORMS_DETAILES`/`SysConfigMenusPERMATIONS`-family tables before being shown/enabled, and several forms across the solution have **no `new X()` call found anywhere in their own project** — these are very likely opened by reflection/dynamic instantiation using a form name stored in one of those permission tables, rather than a hardcoded menu handler. This was independently observed in `Stores`, `CarsSystemLib`, and is consistent with the permission architecture confirmed in `Alter_DataBase`. The exact dispatch mechanism was not located in source — see `CLAUDE.md`'s *Areas Requiring Further Investigation*.
