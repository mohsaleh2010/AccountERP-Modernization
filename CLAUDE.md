# CLAUDE.md — Quick Reference

This repository contains many independent codebases. **This document covers only the `Accounts.sln`/`AccountsNet.sln` solution** — a large, long-lived Egyptian ERP/accounting system (WinForms, mid-migration from .NET Framework to modern .NET, dual-targeting `net10.0-windows;net48`). Core modules: General Ledger, multi-branch/currency accounting, inventory, sales/purchasing, Egyptian e-invoicing, car workshop, tourism/reservations, textile production, construction, HR, POS, beauty center. A vendored hospital system (`GHospital_Care`) is bundled. A parallel Blazor/MAUI prototype exists in `Razors/` but is not yet wired into production.

**Canonical schema reference:** `ERP_V1_3.md` at the solution root (490KB) — authoritative for all DB schema/business rules.

**Companion:** `FORMS_INVENTORY.md` — full per-screen catalog and navigation map.

---

## Critical Always-Loaded Rules

1. **Never rename DB tables/columns or C# model properties** — match `ERP_V1_3.md` exactly, typos included (`Purshing`, `Detailes`).
2. **Never assume `Id` is a PK or `*Id` is a valid FK** — verify DDL/constraints first.
3. **Check for near-duplicate forms** before modifying (numbered folders, `New folder`, `_New`/`Old`/`OLD` suffixes) — fixes often miss parallel copies.
4. **Check `.csproj` for `<Compile Remove>`** before assuming a file is dead code.
5. **Use the existing `operations`/`*CLS` DAL layer** matching the surrounding project's pattern — don't introduce a fourth data-access style.
6. **Never write raw connection strings, passwords, or `ReturnConnStr()` values** to files/commits/logs — treat as live secrets.
7. **Preserve existing error-handling style** (`try/catch` + `MessageBox.Show` + optional `DataDictionaryLogerException`).
8. **Arabic text may be Windows-1256 encoded** — detect and preserve encoding; don't corrupt with UTF-8 tooling.
9. **Open forms via constructor parameters**: `new TargetForm(args).Show()`/`.ShowDialog()`.
10. **Respect the permission system** (`PERMATIONS`/`FORMS_DETAILES`/`USER_DETAILES`) — it is load-bearing, not decorative.
11. **Invoice tax flags**: `IsInvoiceTaxInfo = 1` = tax invoice; `Is_Tax_Per_Item = 1` = tax per item (never tax the total again or discount the total after per-item tax). **Never use `Invoice_Type` as a tax flag** (keep it only as the numbering-series arg of `Make_New_Invoice_No`/`Make_New_Invoice_ReverseNo`). `IsMaintainceInvoice == 1` = car-maintenance invoice.
12. **Invoice tests must check stock**: read `Unit_Item.Full_Qua`/`D_Qua` before and after saving (sale decreases, sale return increases, purchase increases, purchase return decreases; per store).

---

## Documentation Index (not auto-loaded — Read a file only when the task needs it)

| File | Read when |
|---|---|
| `docs/overview.md` | solution layout, out-of-scope folders, legacy copies |
| `docs/projects.md` | project list, purposes, project references |
| `docs/technology-stack.md` | NuGet packages, frameworks, report engines |
| `docs/database.md` | DB access patterns, `operations` DAL, connection config |
| `docs/architecture.md` | layering, static state, base forms |
| `docs/forms-inventory.md` | where forms/menus are (then `FORMS_INVENTORY.md`) |
| `docs/coding-naming.md` | coding/naming conventions, Arabic transliterations (`Da2n`, `Madin`, `Sarf` …) |
| `docs/configuration.md` | config files, XML conn info, startup flags |
| `docs/known-constraints.md` | known risks/limitations |
| `docs/dev-rules.md` | full development rules (summary above) |
| `docs/investigations.md` | open/unresolved questions |
| `docs/invoices-plan.md` | invoice-screen plan (large — search, don't read whole) |

Read large files (`MainFrm_AccountUpdate*.cs`, `StockEngineSchemaCLS.cs`, `ERP_V1_3.md`, `FORMS_INVENTORY.md`) by Grep + ranged Read, never whole.