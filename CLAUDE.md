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

---

## Documentation Index

@docs/overview.md
@docs/projects.md
@docs/technology-stack.md
@docs/database.md
@docs/architecture.md
@docs/forms-inventory.md
@docs/coding-naming.md
@docs/configuration.md
@docs/known-constraints.md
@docs/dev-rules.md
@docs/investigations.md