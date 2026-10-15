# SalesRep (Field Sales / المندوبين) — Handoff Document

> محادثة كاملة مُلخّصة لتقديمها للذكاء الاصطناعي لاحقاً.
> تاريخ العمل: 2026-10-12 → 2026-10-15 · الفرع: `master` · الحل: `Accounts.sln` / `AccountsNet.sln`

---

## 1. السياق

- **المستودع:** `E:\MohSaleh\MyWorkAll\SIT_Projects .Net\SIT_Projects` — ERP مصري كبير (WinForms، ترحيل .NET Framework → .NET 10، dual-target `net10.0-windows;net48`).
- **القاعدة:** SQL Server `ClientNew` (من `CommonFiles\ConnInfo.xml` — `Encrypt=False`، `Integrated Security=SSPI`).
- **المرجع الأصلي:** `ERP_V1_3.md` (490KB) — مصدر الحقيقة للـ Schema والأسماء.
- **الوثائق:** `CLAUDE.md` + `docs/*.md` (overview, projects, database, architecture, coding-naming, known-constraints, dev-rules…).

## 2. القواعد الملتزمة (من CLAUDE.md / dev-rules.md / المستخدم)

1. **لا تعديل/حذف بيانات قديمة** (`لا تعدل فى البيانات القديمة`) — كل كتابة اختبار داخل `Rollback`.
2. **لا إعادة تسمية** جداول/أعمدة/خصائص C# — مطابقة `ERP_V1_3.md` حرفياً (الأخطاء الإملائية مثل `Purshing`, `Detailes` محفوظة).
3. **لا افتراض أن `Id` PK أو `*Id` FK** — تحقق من DDL أولاً.
4. **لا كتابة connection strings / كلمات سر / `ReturnConnStr()`** في ملفات/commits/logs.
5. **الطبقات:** Form → `*CLS` → `operations` DAL (نفس نمط المحيط).
6. **معالجة الأخطاء:** `try/catch` + `MessageBox.Show` (+ `DataDictionaryLogerException` اختيارياً).
7. **الترميز:** ملفات كثيرة Windows-1256 (بدون BOM) — لا تفسد العربية بأدوات UTF-8.
8. **الصلاحيات:** `PERMATIONS`/`SysConfigMenusPERMATIONS`/`USER_DETAILES` load-bearing.
9. **قواعد الفواتير (rule 11/12 من dev-rules):**
   - `IsInvoiceTaxInfo=1` فاتورة ضريبية؛ `Is_Tax_Per_Item=1` ضريبة على الصنف (لا تُحسب مرة أخرى على الإجمالي).
   - **`Invoice_Type` ليس علم ضريبة** — سلسلة ترقيم فقط (`Make_New_Invoice_No`).
   - **اختبار أي فاتورة = فحص رصيد المخزون:** `Unit_item.Full_qua` / `D_Qua` قبل/بعد (مبيعات تنقص، مرتجع يزيد).
10. **git attribution:** `git -c user.name=MohSaleh -c user.email=moh_saleh2010@yahoo.com commit` + `Co-Authored-By: Claude Code <noreply@anthropic.com>`.

## 3. الموديول المنفّذ: SalesRep (المندوبين / Field Sales / Van Sales)

### Phase 1 — Schema (`salesrep-schema-phase1.sql` في جذر الحل)
20 جدول: `SalesRep_Reps` (مندوب: Van_Store, Damaged_Store, Cash_Box, Sales_Mode), `SalesRep_Devices`, `SalesRep_Shifts` (وردية: Open/Closed), `SalesRep_Shift_Stock`, `SalesRep_Shift_Cash` (جرد خزينة), `SalesRep_Document_Links` (ربط مستندات بالوردية), `SalesRep_Visits` (خطة زيارات), `Lookup_SalesRep_NoSaleReason`, `Lookup_SalesRep_ReturnReason`, `SalesRep_Approvals` (طلبات استثناء), `SalesRep_Attachments`, `SalesRep_Credit_Policy`, `SalesRep_Discount_Policy`, `SalesRep_Routes` + `SalesRep_Route_Customers`, `SalesRep_Sync_Inbox` (أوفلاين), `SalesRep_Invoice_Return_Links`, `SalesRep_Targets`, `SalesRep_Commission_Plans` + `Tiers`.
- IDs: Snowflake (`CallSnowflakeIdGenerate.GenerateNewId()`).

### Phase 2 — Services (`AccountModelsLib\SalesRep\`)
- `SalesRepModels.cs` + `SalesRepModels_Additional.cs` (EF Core entities + enums: `SalesRepSalesMode`, `SalesRepShiftStatus`, `SalesRepDocType`, `SalesRepApprovalType`, `SalesRepApprovalStatus`).
- `SalesRepShiftService` (OpenShift/CloseShift + **LoadVan/UnloadVan** عبر `StoreOrderService.SaveDraft` + `Post` — `OrderType=1001`, `PartyStore=4`).
- `SalesRepSalesService` (CreateFieldSaleInvoice + **CreateFieldSaleInvoiceDirect** + ValidateCredit/Discount + CalculateCustomerBalance).
- `SalesRepReturnService` (CreateReturnInvoice + `SalesRep_Invoice_Return_Links`).
- `SalesRepApprovalService` (CreateApprovalRequest/ApproveRequest).
- `SalesRepSyncService` (GetSyncData).
- `SalesRepCommissionService` (CalculateCommission).
- `SalesRepOfflineSyncService` (RegisterDevice/GetSyncPackage + **RouteTo*Engine**: `RouteToInvoiceEngine` → `Account_Entry_Invoice_CLS.Make_EntryForInvoices`، `RouteToReturnEngine` → `Make_EntryForInvoices_Reverse`، `RouteToCashVoucherEngine` → `Money_Transaction_BoxOrderReportCLS`/`CashVouchersService`، `RouteToCustomerOrderEngine` stub).
- `SalesRepTestRunner` (8 اختبارات داخل `BeginTransaction` + `Rollback` — `TestConnStr` اختياري).
- `ScratchSalesRepTests.cs` (مشغّل اختبار scratch).

### Phase 3 — WinForms (`Accounts\Sales_Rep_Frm\`)
- `SalesRepShiftFrm.cs` — وردية: فتح/إقفال + **Load Van/Unload** + **`btnAddItem` picker** + `grdDocLinks` + `grdLines` + `BuildOrderLinesFromGrid()` (→ `StoreOrderService.OrderLineInput`).
- `SimpleItemPickerFrm.cs` — اختيار صنف من `Unit_item` عبر `operations(2)` مباشرة (يعرض id/Item_Id/Store_id، يرجع `SelectedUnitItemId`).
- `SalesRepVisitsPlanFrm.cs` — خطة زيارات (قراءة فقط + زر جدولة).
- `SalesRepShiftCashFrm.cs` — جرد خزينة (قراءة فقط + زر Count Cash).
- `SalesRepApprovalsFrm.cs` — طلبات موافقة + `btnApprove` → `SalesRepApprovalService.ApproveRequest`.

### Razor (`Razors\AccountERP.SharedUI\Pages\SalesRep\`)
- `ShiftPage.razor` (`/salesrep/shift`), `VisitsPlanPage.razor` (`/salesrep/visits`), `CashCountPage.razor` (`/salesrep/cash`), `ApprovalsPage.razor` (`/salesrep/approvals`).
- `Routes.razor` **لا يحتاج تعديلاً** — الـ `Router` يلتقط الصفحات تلقائياً عبر `AppAssembly` (`@typeof(SharedUIAssemblyMarker).Assembly`).

### القوائم والصلاحيات
- `MainFrm_AccountUpdate.Designer.cs` (Windows-1256!): 4 حقول `ToolStripMenuItem` + init + إضافتها لـ `menuSalesManProcess.DropDownItems`. (ملحوظة: التعديل تم بـ PowerShell `[Encoding]::GetEncoding(1256)` لحفظ الترميز؛ الـ labels إنجليزية `Shift - Sales Rep`… لتجنب تلف العربية).
- `MainFrm_AccountUpdate.cs`: 4 handlers (`menuSalesRepShift_Click`… → `new Sales_Rep_Frm.*Frm(); MdiParent=this; Show();`) — أُدرجت بعد `menuItemSalesSccounts_Click` (سطر ~2160).
- `AccountModelsLib\SalesRep\SalesRepMenuTablesInsert.sql`: `SysConfigMenusAr` (IDs 1001–1004, Parent 496) + `SysConfigMenusPERMATIONS` (`Allow`).
- **`FORMS_DETAILES` غير مستخدم** في `MainFrm_AccountUpdate` (جدول وصف فقط §8924) — أُزيلت إدخالاته من الـ SQL.
- `SalesRepPermissionsInsert.sql` (قديم/مدمج في MenuTables).

## 4. سجل الـ commits (بالترتيب)

`bd71398` (Phase 3 كامل) → `fc6e1c4` → `4c4e6f8` → `b876a55` → `5dc3458` (سابقة) ثم:
- `bd71398` Phase 3: Load/Unload + 3 screens + RouteTo*Engine
- `4990e34` menu wiring (designer) — **كان فيه عيوب أُصلحت في 38ca7f7**
- `c7adc78` grdLines
- `f9e2062` CreateFieldSaleInvoiceDirect
- `d8c96ee` stock-balance check (Full_qua/D_Qua)
- `26ef8d6` 4 menu click handlers (.cs)
- `da36131` build fixes (Full_qua case, Item_Id, oppLocal readonly)
- `56873fa` grdLines→LoadVan/UnloadVan
- `2f2b6a9` / `eb8e169` / `26d041d` permission & menu SQL (FORMS_DETAILES removed)
- `fe93c86` scratch test runner
- `8f95478` Razor pages — **كان فيها عيوب أُصلحت في 38ca7f7**
- `3afb68c` btnAddItem (SubItemListFrm — لاحقاً أُصلح)
- `60d868c` **SimpleItemPickerFrm** (بديل SubItemListFrm)
- `fc7ae54` re-run verification marker
- **`38ca7f7` إصلاح عيوب Blocking للبناء** (انظر §6.أ)

## 5. نتائج الاختبار (SalesRepTestRunner — 8/8 ✅)

داخل معاملة ملغاة (Rollback) على `ClientNew`:
1. فتح الوردية ✅ 2. إدارة الموافقات ✅ 3. البيع الميداني + **فحص رصيد المخزون Full_qua/D_Qua قبل/بعد** ✅ 4. المرتجع ✅ 5. المزامنة ✅ 6. إقفال + عمولة ✅ 7. تسجيل جهاز ✅ 8. حزمة المزامنة ✅
**التحقق:** `SalesRep_Reps/Devices/Shifts/Approvals = 0` للـ rep 999901 بعد Rollback — لا أثر على الإنتاج.

## 6. المتبقى (للعمل القادم)

### 6.أ — أُصلح في `38ca7f7` (كان Blocking البناء — الجلسات السابقة بنت AccountModelsLib فقط فلم تُكتشف)

**`MainFrm_AccountUpdate.Designer.cs` (Windows-1256):**
1. ربط القوائم (4990e34) استخدم `'...'` (char literal) بدل `"..."` → CS1012/CS1011 (16 خطأ). أُصلحت لـ double-quoted.
2. نفس الـ block أُدرج مرتين (نسخة بسماء + نسخة فارغة) → حُذفت النسخة الثانية (23 سطر).
3. تعريفات الحقول الـ 4 تكررت (5976-5979) → CS0102. حُذف التكرار.

**`SalesRepShiftFrm.cs`:**
4. الـ constructor استدعى `InitializeComponents()` قبل إسناد `_opp`/`_shiftService` → NullReferenceException عند فتح الشاشة. أُعيد الترتيب.
5. `grdLines` مُعلنة لكن لم تُنشأ/تُضاف → NullReferenceException عند PickItemFromGrid. أُنشئت بـ 4 أعمدة وأُضيفت لـ Controls؛ كُبّر حجم النموذج.
6. `btnAddItem` أُنشئت لكن لم تُضاف لـ Controls وكانت تحت `btnLoadVan`. نُقلت لـ x=684 وأُضيفت؛ صارت field.
7. `PickItemFromGrid` مرّرت `List<anonymous-type>` إلى `List<dynamic>` → CS1503. صارت تُسقِط إلى `UnitItemRow`.

**`SimpleItemPickerFrm.cs`:**
8. `List<dynamic>` → `List<UnitItemRow>` (class جديد: Id/Item_Id/Store_id).

**صفحات Razor الـ 4:**
9. `@inject ...OperationsService` غير موجودة أصلًا → CS0234. أُزيلت.
10. `@attribute [Authorize]` بدون استيراد `Microsoft.AspNetCore.Authorization` (والصلاحيات غير موصولة في النموذج الأولي أصلاً) → CS0246. أُزيلت.

**النتيجة:** AccountModelsLib ✅ 0 errors · Accounts ✅ 0 errors · AccountERP.SharedUI ✅ 0 errors · AccountsERPSystemNet10 (EXE shell) ✅ 0 errors.

### 6.ب — مازال مفتوحاً

| # | البند | النوع |
|---|---|---|
| 1 | تشغيل `SalesRepMenuTablesInsert.sql` (SysConfigMenusAr + SysConfigMenusPERMATIONS) | DBA |
| 2 | ~~بناء كامل الحل~~ ✅ **تم** (Accounts + Razors + EXE shell — 0 errors) | ~~Build~~ |
| 3 | اختبار `SimpleItemPickerFrm` + `grdLines` مع بيانات `Unit_item` حقيقية (runtime) | Test |
| 4 | اختبار صفحات Razor من المتصفح عبر `/salesrep/*` | Test |
| 5 | `RouteToCashVoucherEngine`/`RouteToCustomerOrderEngine` لا يزالان stub (جزئياً عبر CashVouchersService) | Code |
| 6 | `SalesRepSalesService.CreateFieldSaleInvoice` (غير-Direct) ما زال يرجع Snowflake فقط — الـ Direct هو المسار الفعلي | Code |
| 7 | صفحات Razor الآن عرض-only (لا تربط بالـ services بعد) — ربط البيانات الحقيقي لاحقاً | Code |

## 7. ملاحظات تقنية مهمة

- **ملفات Windows-1256 (بدون BOM):** `MainFrm_AccountUpdate.cs` و`.Designer.cs` — أي تعديل عليها يجب بـ `[System.Text.Encoding]::GetEncoding(1256)` أو `Edit` سيفشل/يفسد العربية. الـ Edit tool يرفض الملف غير-UTF8 صراحةً.
- **`Unit_item` خصائص:** `Full_qua` (حرف صغير q) لا `Full_Qua` — C# case-sensitive؛ `Item_Id` لا `Item_id`.
- **`SubItemListFrm`** يتطلب `long Item_Id_` في constructor ولا يعرض `SelectedItemId` — لذلك بُني `SimpleItemPickerFrm` بدلاً منه.
- **`operations` readonly:** لا يمكن تمرير `_opp` كـ `ref` — استخدم `operations oppLocal = _opp;`.
- **أسماء المستندات:** `Make_EntryForInvoices(ref excp, ref msg, ref opp, invoiceId, dtFrom, dtTo, IS_Posted_Flage)` — التوقيع من `Account_Entry_Invoice_CLS.cs:61`.
- **SSL:** كان يمنع التشغيل (`0x80090325`) — حل بـ `Encrypt=False` في TestConnStr.

## 8. الالتزامات الأمنية

- ✅ صفر connection strings/كلمات سر مكتوبة في أي commit.
- ✅ صفر UPDATE/DELETE على بيانات سابقة.
- ✅ العربية محفوظة (Windows-1256 محفوظة في الملفات الأصلية؛ UTF-8 في الملفات الجديدة).
- ✅ كل commit بـ `Co-Authored-By: Claude Code <noreply@anthropic.com>`.
