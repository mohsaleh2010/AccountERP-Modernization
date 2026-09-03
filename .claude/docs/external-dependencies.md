# External Dependencies

Key NuGet packages referenced across `Accounts.sln`/`AccountsNet.sln`'s in-scope projects (version numbers vary slightly by project; multiple major versions of some packages coexist — see `CLAUDE.md`'s *Known Technical Constraints*).

| Package | Notes |
|---|---|
| `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer` (v10) | Modern EF Core ORM — see `CLAUDE.md`'s *Database Access*. |
| `EntityFramework` (v6.5.x) | **Legacy EF6** — coexists with EF Core; `app.config`'s `<entityFramework>` section is EF6-era. Extent of live EF6 usage vs. leftover config was not fully traced. |
| `Microsoft.Data.SqlClient` (v7) | Modern SQL Server ADO.NET client — the DAL's primary connection type. |
| `System.Data.SqlClient` (v4.9) | **Deprecated** legacy SQL Server client — present alongside the modern one in some projects. |
| `System.Data.OleDb` | Legacy OLE DB access — used by a few older classes (e.g. `Connection4It.ConnectionString`), largely superseded. |
| `Microsoft.Data.Sqlite`, `SQLitePCLRaw.bundle_e_sqlcipher` | SQLite support — purpose/consumer not fully confirmed (possibly for the MAUI prototype's offline storage; flagged for investigation). |
| `CrystalDecisions.CrystalReports.Engine`, `.ReportSource`, `.Shared`, `.Windows.Forms` | Crystal Reports engine — one of two coexisting report renderers. |
| `Microsoft.ReportingServices.ReportViewerControl.Winforms` (v14 **and** v15 both referenced across the solution), `ReportViewerCore.WinForms` | Microsoft RDLC ReportViewer — the other coexisting report renderer. |
| `Microsoft.SqlServer.Types` (v14 **and** v170 both referenced), `Microsoft.SqlServer.SqlManagementObjects` (SMO, v172 **and** v181 both referenced) | SQL Server spatial types / server-management API — used by `Alter_DataBase`'s DB-migration tooling among others. |
| `ZXing.Net` | Barcode/QR code generation — `BarCodeQR_GeneratorLIB`. |
| `RestSharp` (v106 **and** v114 both referenced) | HTTP client — used for e-invoice tax-portal API calls (`AccountsEInvoicesLIB`). |
| `Newtonsoft.Json` | JSON serialization. |
| `IdGen` | Twitter-Snowflake-style distributed unique ID generator — consumer not fully traced. |
| `Microsoft.AspNetCore.Components*`, `Microsoft.AspNetCore.Components.WebView.WindowsForms`, `Microsoft.AspNetCore.Components.WebView.Maui`, `Microsoft.Maui.Controls`, `MudBlazor` | The Blazor/MAUI prototype stack (`Razors\` group + `AccountsERPSystemNet10`'s not-yet-wired `BlazorWebView` integration). |
| `Microsoft.AspNetCore.SystemWebAdapters` | ASP.NET-to-ASP.NET-Core migration shim — suggests some System.Web-era code may be involved in the Blazor Web prototype. |
| `System.Configuration.ConfigurationManager` | Needed for `app.config`-style config access from modern .NET (SDK-style) projects. |

Full NuGet/package lists for the 6 legacy non-SDK-style projects (`Clinic`, `ClinicPatient`, `DataBaseSetting`, `EASY E-INVOICE`, `FreeMedical`, `EncryptionCA`, `AbsencePrecenseProj`) were not separately enumerated beyond what surfaced in the forms-inventory passes — check their `packages.config`/legacy reference lists directly if working on any of them.
