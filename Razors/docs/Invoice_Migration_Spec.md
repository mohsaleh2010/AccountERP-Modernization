# مواصفات فاتورة المبيعات/المشتريات ومرتجعاتها — تأسيس لإعادة البناء بـ Razor/Blazor

> **الهدف من هذا الملف:** توثيق المنطق التجاري الحقيقي لشاشات الفواتير الأربعة (فاتورة مبيعات، مرتجع مبيعات، فاتورة مشتريات، مرتجع مشتريات) كما هو منفّذ فعلياً في الكود القديم (WinForms)، ليكون أساساً لبناء نفس الشاشات ضمن مشاريع `Razors\` (AccountERP.Web / AccountERP.Web.Client / AccountERP.MobileDesktop / AccountERP.SharedUI). هذا التوثيق نتيجة مراجعة كود فعلية للمشروع، وليس افتراضات.

---

## 1. الفورمز الحقيقية في الكود القديم (Accounts project)

ملاحظة مهمة: ملف `CLAUDE.md` (الوثيقة المرجعية للمشروع) لم يوثّق هذه الفورمز بدقة في القائمة الأولى، لأنها ترث من `Project_ControlLIB.MyForm` بدلاً من `Form` مباشرة، فلم يلتقطها الفحص الآلي البسيط. هذه هي الفورمز الصحيحة بعد مراجعة الكود مباشرة:

| النوع | فورم الإنشاء | فورم التعديل | الكلاس/الفورم الأساسي (base) | فورم اختيار الصنف |
|---|---|---|---|---|
| فاتورة مبيعات | `Requests\InvoiceTemplate\Invoice\Make_Invoices.cs` | `...\Invoice\EditFRM_Invoices.cs` | `Requests\InvoiceTemplate\InvoiceTemplate\NewFrmInvoive.cs` (٥٩٤٢ سطر — المحرك الحقيقي؛ `Make_Invoices : NewFrmInvoive`) | `ItemData` (عادي) / `ItemDataPh` (وضع الصيدلية) |
| مرتجع مبيعات | `Reverse\Make_Invoices_Reverse.cs` | `Reverse\EditFRM_Invoices_Reverse.cs` | نفس `NewFrmInvoive` | نفس فورمز اختيار الصنف |
| فاتورة مشتريات | `InvoicesFrm\Bills\Bill\Make_Bill.cs` | `Bills\Bill\Edit_Maked_Bill.cs` | `Bills\Make_Bill_Templat.cs` | `ItemDataBill` / `ItemData_ph_Bill` |
| مرتجع مشتريات | `Bills\Reverse\Make_Bill_Reverse.cs` | `Bills\Reverse\Edit_Maked_Bill_Reverse.cs` | نفس `Make_Bill_Templat` | نفس فورمز اختيار الصنف |

- `Bills_ListFrm.cs` هي فقط شاشة بحث/استعراض تفتح `Edit_Maked_Bill` عند الضغط المزدوج — ليست شاشة إنشاء.
- المشتريات معمارياً مرآة للمبيعات، بمجلد فرعي واحد فرق (`Bill/` مقابل `Reverse/`).

### الكلاسات المنطقية (Business Logic) المسؤولة فعلياً عن التنفيذ

- **المبيعات:** `AccountModelsLib.Requests_ModelCls` (ملف `BusinessLibShared\Requests.partial.cs`):
  - `Make_Invoice_Update_CustomerAccount` — فاتورة بيع جديدة
  - `Edit_Invoice_Update_CustomerAccount` — تعديل فاتورة بيع
  - (في `Requests_Cls.cs`) `Make_Invoice_Reverse_Update_CustomerAccount` / `Edit_InvoiceReverse_Update_CustomerAccount` — مرتجع البيع
- **المشتريات:** `BillsClss`/`Bills_ModelsClss` (`BusinessLibInvoice\Bills_ModelsClss.cs`):
  - `Make_Bill_Update_SupplierAccount`, `Edit_Bill_Update_Supplier_Account`
  - `Make_Bills_List_Reverse_Update_SupplierAccount`, `Edit_Bill_List_reverse_Update_Supplier_Account`
- **ترحيل رصيد العميل/المورد:** `CustomersAccountCLS.Set_Customer_Balance_Invoice` / `Set_Customer_BalanceInvoiceReverse` — و — `Suppliers_AccountsCls.Set_Supplier_Balance_MakeBill` / `..._Reverse`
- **القيد المحاسبي التلقائي (GL):** `Account_Entry_Invoice_CLS.Make_EntryForInvoices` / `..._Reverse`
- **طبقة الوصول للبيانات (DAL):** نمط هجين — رأس/تفاصيل الفاتورة تُدرج عبر كيانات EF Core مباشرة (`invL_O.Insert_Invoices_List`, `invD_O.Insert_Invoices_Details`, داخل `operations.db`)، لكن **كل تأثيرات المخزون والسعر منفذة بكود SQL خام مُركّب كنص** عبر `opp.insert_me(...)`.

---

## 2. أهم اكتشاف: تحديث رصيد الصنف ليس دائماً مباشراً

تحديث `Unit_item` عند حفظ الفاتورة **مشروط بإعدادات النظام**، وليس سلوكاً ثابتاً واحداً:

```
لو (Common.Project_Market == 1) و (invL_O.invL.IS_Make_Sarf_Item_BalanceFromInvoice == 1):
    → تحديث مباشر بـ SQL خام:
      UPDATE Unit_item
      SET Full_qua = Full_qua ± Larg_Qnty,
          D_Qua    = D_Qua ± Small_Qnty,
          Item_No  = Item_No ± Qnty_Another
      WHERE id = Unit_Item_Table_Id

وإلا لو (Common.Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice == 1):
    → لا يُعدَّل الرصيد من الفاتورة نفسها إطلاقاً؛ بدلاً من ذلك تُنشأ تلقائياً
      حركة صرف/استلام مخزنية (Order_Purshing / Order_Receiving) عبر
      Order_Details_AutomaticCls (في مشروع Store_LIB)، وهذه الحركة المخزنية
      المنفصلة هي التي تُغيّر رصيد الصنف فعلياً لاحقاً.
```

**اتجاه التأثير (مؤكد من الكود، وموافق لما تم رصده أثناء الاختبار اليدوي):**
| نوع الفاتورة | تأثير رصيد الصنف |
|---|---|
| فاتورة مبيعات | نقص (`-`) |
| مرتجع مبيعات | زيادة (`+`) |
| فاتورة مشتريات | زيادة (`+`) |
| مرتجع مشتريات | نقص (`-`) |

> **قرار تصميم مطلوب لإعادة البناء بـ Razor:** لازم يُحسم أي المسارين سيُعتمد في النظام الجديد — تحديث مباشر لرصيد الصنف من الفاتورة، أو إنشاء أمر مخزني تلقائي منفصل يتولى التحديث. المساران مختلفان جوهرياً في الكود، وليسا مجرد تفصيل شكلي.

---

## 3. الجداول المتأثرة — المبيعات مقابل المشتريات

- **فاتورة/مرتجع المبيعات تكتب في جداول منفصلة تماماً وليس بعلامة نوع في نفس الجدول:**
  - بيع: `Invoices_List` / `Invoices_Details`
  - مرتجع بيع: `Invoices_List_Reverse` / `Invoices_Details_Reverse` (عبر `Insert_Invoices_Details_Reverse`)
- **المشتريات (بحسب المراجعة، يحتاج تأكيد إضافي من `ERP_V1_3.md` قبل الاعتماد الكامل):** يبدو أن الاتجاهين (شراء ومرتجع شراء) يستخدمان نفس جدولي `Bills_List` / `Bills_Details`، مع تحديثات بفروق (delta) بإشارة معاكسة، بدلاً من جداول `_Reverse` منفصلة كما في المبيعات.

---

## 4. خطوات الحفظ الكاملة (تنطبق على الأنواع الأربعة بنفس الشكل العام)

كل هذه الخطوات تتم داخل **معاملة قاعدة بيانات واحدة (transaction)**: `opp.BeginTransaction()` ... `opp.Commit()` / `opp.Rollback()` عبر كائن `operations opp` مشترك.

1. **التحقق (Validation):** عميل/مورد مُختار، مندوب مبيعات مُختار إذا كان `Common.ShowSalesMan == 1`، عملة مُختارة.
2. **إدراج/تحديث رأس الفاتورة** (`Invoices_List`/`Bills_List` أو مقابلاتها `_Reverse`) — يحمل:
   - `Invoice_Id` (الرقم المعروض للمستخدم — **مُسلسل منفصل تماماً عن المفتاح الأساسي `Id`**؛ يُنشأ عبر `InvoicesClss.Make_New_Invoice_No(Invoice_Type)` ويُتحقق من عدم تكراره (`Check_InvoiceNo_Found`) فقط لنوع الفاتورة "الحكومية"/بوابة الضرائب (`rdk3am`)، وليس للنوع العادي).
   - `Customer_Id` / `Supplier_Id`
   - `SubAccount_Id` + `SubAccount_Name` — **الحساب الفرعي في شجرة الحسابات المرتبط بالعميل/المورد، يُسجَّل بشكل مكرر مباشرة على رأس الفاتورة وقت الحفظ** (وليس عبر ربط حي بجدول العميل/المورد وقت التقرير لاحقاً) — هذا ما يعتمد عليه ترحيل القيد المحاسبي.
   - `Currency_id`, `Branch_Id`, `User_Id`
   - `CustomerAccountBefor` / `CustomerAccountAfter` — لقطة (snapshot) لرصيد الحساب قبل/بعد الفاتورة، تُخزَّن على نفس الصف.
   - حقول الضريبة: `TaxSales10`, `ATS05`, وقيمها المحسوبة (`..._Value`)
   - `Total_PriceWithoutTax`, `Net_Total`
   - `PolicaNo` / `troodContainerNo` (مراجع شحن/حاويات)
   - `Salesman_Id`
   - `Emp_Id` / `Emp_Name` (البائع)
   - `ShipmentCompany_Id` / `Shipment_Value`
   - `IS_Make_Sarf_Item_BalanceFromInvoice` (فلاج يحدد مسار تحديث المخزون — انظر البند ٢)
   - `Is_InvoiceKta3y` — فلاج وضع "فاتورة قص القطعة" (خاص بالنسيج، يضيف حقول طول/عرض إضافية)
   - `Is_Invoice_Agile` + `PaymentValueFor_Is_Invoice_Agile` — فلاج الدفع الآجل/المؤجل
3. **حلقة على أصناف الفاتورة:**
   - إدراج صفوف التفاصيل — تحمل ثلاث كميات متزامنة بوحدات مختلفة: `Larg_Qnty` (وحدة كبيرة), `Small_Qnty` (وحدة صغيرة), `Qnty_Another` (عدد قطع)
   - تحديث رصيد المخزون (مشروط — انظر البند ٢)
   - **للمشتريات فقط:** `UpdatePurchasePrice` — يُحدّث سعر البيع (`Item.S_price`/`Unit_Item.S_price`) بسعر الشراء الفعلي، فقط إذا `Common.Is_Update_Price_On_invoice == 1`
   - `Set_ItemTransactionFromInvoice` / `FromBill` — يسجّل صف حركة صنف (سجل تدقيق/Audit trail)
   - `SetBalanceWithExpireDate_Tash3ela_*` — تتبع رصيد الدفعات بتاريخ الصلاحية، فقط تحت `Project_Market == 1`
4. **ترحيل رصيد العميل/المورد** — عبر `Set_Customer_Balance_Invoice`/`Set_Supplier_Balance_MakeBill` أو مقابلاتها `_Reverse` — مشروط بـ `Common.IsSetCustomers_AccountsForInvoice` / `IsSetSuppliers_AccountsForBills`.
5. **ترحيل قيد يومية تلقائي (اختياري لكن ليس دائماً):**
   - للفاتورة العادية: مشروط بـ `Common.IS_Make_Automatic_Queeod == 1`
   - **لمرتجع المبيعات: بدون شرط — يُرحَّل قيد دائماً** (لم يُتحقق بعد إن كان هذا ينطبق أيضاً على مرتجع المشتريات — يحتاج تأكيد لاحق).
6. **إنشاء سند قبض/صرف نقدي تلقائي (اختياري)** — إذا `Common.MakeOrderMoneyFromInvoiceAutomaticWithoutForm == 1` والفاتورة ليست آجلة (`Is_Invoice_Agile`).

---

## 5. تفاصيل تصميمية غير ظاهرة من الواجهة (لازم تُنقل لتصميم Razor)

- **رقم الفاتورة منفصل عن المعرّف الداخلي (`Id`)** — كما في البند ٢ من رأس الفاتورة أعلاه.
- **`SubAccount_Id`/`SubAccount_Name` يُسجَّلان وقت الحفظ** وليسا عبر Join حي لاحقاً — القيد المحاسبي يعتمد على القيمة المخزَّنة وقتها.
- **تعديل فاتورة قائمة يستخدم آلة حالة (state machine) للأصناف** عبر حقل `Item_ProcessedFlage`:
  - `0` = صنف جديد لم يُحفظ بعد
  - `100` = صنف مُعالَج (تمت معالجته)
  - `-1` = صنف للحذف
  - كل حالة تقود لحساب فرق مختلف في الرصيد (مقارنة `OLD_Larg_Qnty` بالكمية الجديدة) — **وليس استبدالاً كاملاً لكل الأصناف عند كل حفظ**.
- **`Invoice_Type` (0/1، أزرار راديو `rdK7as`/`rdk3am`)** يُبدّل بين الفاتورة العادية والفاتورة الحكومية (بوابة الضرائب المصرية) — يغيّر: نسب الضريبة الافتراضية، آلية ترقيم الفاتورة، والتحقق من تكرار الرقم.
- **الفلاجات الثابتة العامة (Global static) التالية على `AccountCommonDataLIB.Common` تتحكم بصمت في أي من الخطوات أعلاه يُنفَّذ فعلياً:**
  - `Project_Market`
  - `IS_Make_Sarf_Item_BalanceFromInvoice`
  - `Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice`
  - `Is_InvoiceKta3y`
  - `Is_Invoice_Agile`
  - `IS_Make_Automatic_Queeod`
  - `IsSetCustomers_AccountsForInvoice` / `IsSetSuppliers_AccountsForBills`
  - `Is_Update_Price_On_invoice`
  - `MakeOrderMoneyFromInvoiceAutomaticWithoutForm`
  - `ShowSalesMan`

  **توصية للنظام الجديد:** هذه يجب أن تصبح **إعدادات صريحة لكل عميل/نشر (deployment)** في النظام الجديد، وليست متغيرات عامة ضمنية (ambient globals) كما هي الآن — وإلا سيختلف سلوك نسخة Razor بشكل غير متوقع عن أي نسخة WinForms تتم مقارنتها بها.

---

## 6. حالة مشروع Razor الحالي (Razors\ folder) بالنسبة لشاشات الفواتير

من مراجعة سابقة لمشاريع `Razors\`:

- توجد ٤ نسخ متوازية من شاشة الفاتورة (`Invoice.razor`, `Invoice1.razor`, `Invoice2.razor`, `Invoice3.razor`) — نمط تكرار/نسخ مشابه لما هو موجود في الكود القديم. الأكمل منها (`Invoice1`/`Invoice2`) بها إضافة أصناف وحساب إجمالي/ضريبة، لكن قوائم العملاء/الأصناف بها **بيانات وهمية ثابتة (hardcoded)**.
- **لا يوجد حفظ فعلي في قاعدة البيانات في أي من الشاشات الأربع** — زر "حفظ واعتماد الفاتورة" يستدعي `SaveInvoiceToDb()` الذي لا يفعل شيئاً سوى مسح الحالة من الذاكرة (`InvoiceState.Clear()`).
- لا يوجد اختيار مورّد، ولا أي منطق لتأثيرات الرصيد (المخزون/الحساب) في أي مكان بمشاريع Razor حتى الآن.
- ملفات `.razor.cs` الست الخاصة بالفواتير (`Invoice.razor.cs`, `Invoice1.razor.cs`, `Invoice2.razor.cs`, `Invoice3.razor.cs`, `ItemSelectorModal.razor.cs`, `"ItemSelectorModal - Copy.razor.cs"`) هي **نسخة واحدة متطابقة تماماً** من كود غير متعلق (كود Customers بالخطأ) — كود ميت بالكامل يجب حذفه، وليس أساساً يُبنى عليه.
- شاشة `CustomerDataList`/`CustomerDataList2` وشاشة `Login` هما النموذجان الوحيدان الجاهزان فعلياً وموصولان بقاعدة البيانات الحقيقية عبر الكلاسات القديمة نفسها (`CustomerCLS`, `AuthService`) — نمط جيد يُحتذى به عند بناء شاشات الفواتير.

**الخلاصة:** شاشات الفواتير في Razor تحتاج بناء منطق تجاري شبه كامل من الصفر (الشكل موجود، لكن التنفيذ الفعلي فارغ)، بينما نمط الربط بقاعدة البيانات وطبقة الـ DI (كما في شاشة العملاء) جاهز ويمكن تكراره لنفس الشاشات الجديدة.

---

## 7. نقاط تحتاج تأكيد إضافي (لم تُحسم بعد)

- تأكيد إن كانت جداول مرتجع المشتريات (`Bills_List`/`Bills_Details` بفروق سالبة) فعلاً كما هي، أو أن هناك جداول `_Reverse` منفصلة مثل المبيعات — يحتاج مراجعة مباشرة لـ `ERP_V1_3.md` أو لكود `Make_Bill_Reverse.cs`/`Edit_Maked_Bill_Reverse.cs` بتفصيل أكبر.
- تأكيد إن كان ترحيل القيد التلقائي غير المشروط (بدون فحص `IS_Make_Automatic_Queeod`) ينطبق فقط على مرتجع المبيعات، أم على مرتجع المشتريات أيضاً.
