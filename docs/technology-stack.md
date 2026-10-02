# Technology Stack

- **Language:** C# (mixed C# language-version eras — old non-partial-class WinForms patterns coexist with modern C# in the same solution).
- **UI framework:** Windows Forms (legacy .NET Framework and modern .NET side by side), plus an early-stage parallel **Blazor** (Web + WebAssembly + MAUI Hybrid) UI stack that is not yet wired into the production app.
- **Target frameworks:** predominantly dual-targeted `net10.0-windows` (Debug) / `net10.0-windows;net48` (Release) via SDK-style `.csproj`; a minority of legacy non-SDK `.csproj` files still target `.NET Framework v2.0`–`v4.8` directly.
- **Data access:** mixed — raw ADO.NET (`Microsoft.Data.SqlClient`, `SqlConnection`/`SqlCommand`/`SqlDataAdapter`), Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`), legacy typed `DataSet`/`TableAdapter` generated code, and (rarely, in a few older files) `System.Data.OleDb`/`System.Data.SqlClient` (deprecated). See [Database Access](database.md#database-access).
- **Reporting:** three coexisting report engines — Microsoft RDLC (`Microsoft.ReportingServices.ReportViewerControl.Winforms`, both v14 and v15 package references present), Crystal Reports (`CrystalDecisions.*`), used inconsistently even for the same report (e.g. `GHospital_Care` renders "print services" through both RDLC and Crystal for different forms).
- **Database:** Microsoft SQL Server (see [Database](database.md)).

---

## External Dependencies

Key NuGet packages referenced across the solution's in-scope projects (version numbers vary slightly by project; multiple major versions of some packages coexist — see [Known Technical Constraints](known-constraints.md)):

| Package | Notes |
|---|---|
| `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer` (v10) | Modern EF Core ORM — see [Database Access](database.md#database-access). |
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