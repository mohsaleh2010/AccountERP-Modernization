# خطة: إظهار كل أعمدة جدول أصناف فاتورة المبيعات (Razor) واختبارها (2026-09-24)

**طلب المستخدم:** فى الأصل (WinForms) بعض أعمدة `LvItems` تُخفى بجعل `Width=0` بحسب إعدادات الشركة. المطلوب: تغيير الإعدادات لتظهر **كل** الأعمدة، وإضافة حقول قيمها فى شاشة إضافة الصنف، ثم اختبارها. التنفيذ بلا الرجوع للمستخدم.

## 1) ما وُجد (من `NewFrmInvoive.cs` سطر 2503-2700 و`ItemData.cs` `SetConfigDataonMonitor`)
الإعداد الرئيسى `Is_ActiveOrNot_Show_ColumnInvoiceMonitor` (=1 عندك) يجعل الأعمدة تُخفى بـ`Width=0` حسب إعدادات فرعية `Show_*_OnInvoiceMonitor`. نفس الإعدادات تُخفى بها حقول شاشة إضافة الصنف الأصلية `ItemData`.

| العمود / الحقل | الإعداد (SysConfig) | القيمة القديمة |
|---|---|---|
| عمولة الصنف (الإجمالى الفرعى للقسم) | `Show_Commision_Value_OnInvoiceMonitor` | 1 |
| قيمة عمولة الصنف | `Show_Item_CommissionValue_OnInvoiceMonitor` | 1 |
| نسبة عمولة الصنف | `Show_Item_CommissionRate_OnInvoiceMonitor` | 0 |
| عمولة الوحدة | `Show_Unit_CommissionValue_OnInvoiceMonitor` | 0 |
| إجمالى عمولة الصنف | `Show_ItemCommissionTotal_OnInvoiceMonitor` | 0 |
| العمولة البديلة (4 أعمدة + رئيسى) | `Show_Alternate_Commission_OnInvoiceMonitor` و`Show_Alternate_Item_CommissionRate/_Unit_CommissionValue/_Item_CommissionValue/_ItemCommissionTotal_OnInvoiceMonitor` | 0 كلها |
| العمود ×3 (3Mola) | `Show_3Mola_Value_OnInvoiceMonitor` | 0 |
| كود الصنف | `Show_ItemsCode_OnInvoiceMonitor` | 0 |
| عدد القطع (ItemNo) | `Show_ItemNo_OnInvoiceMonitor` | 1 |
| ضريبة المبيعات (نسبة/قيمة) | `Show_Item_TaxSale10_OnInvoiceMonitor` و`..._Rate_` و`..._Value_` | 0 |
| ضريبة الخصم والإضافة ATS05 (نسبة/قيمة) | `Show_Item_ATS05_OnInvoiceMonitor` و`..._Rate_` و`..._Value_` | 0 |
| اسم المخزن | `Show_StoreName_OnInvoiceMonitor` | 0 |
| رقم البيان `StatementNo` | `Show_StatementNo_Value_OnInvoiceMonitor` | 0 |
| رقم العملية `OperationNo` | `Show_OperstionNo_Value_OnInvoiceMonitor` | 0 |
| اسم التاجر `Merchant_Name` | `Show_Merchant_Name_OnInvoiceMonitor` | 0 |
| التشغيلة `Tash3ela` | `Show_Tash3ela_OnInvoiceMonitor` | 0 |
| الكمية المجانية (كبرى/صغرى) | `Show_ItemsQnty_Free_OnInvoiceMonitor` | 0 |
| عدد القطع المجانية | `Show_Item_No_Free_OnInvoiceMonitor` | 0 |
| تاريخ الصلاحية | `Show_Item_Expire_Date_OnInvoiceMonitor` | 0 |
| الإجمالى بلا ضريبة | `Show_columnTotalNoTax_OnInvoiceMonitor` | 0 |
| قيمة الكمية `Qnty_Value` | `Show_Qnty_Value_OnInvoiceMonitor` | 0 |
لا تُغيَّر: `Show_Qnty_With_Unit=1` و`Enable_ItemNoOnInvoice=1` (أصلاً 1)، و`Enabel/Use_cbIs_TaxPer_Item_OnInvoiceMonitor` (تغيّر سلوك الضريبة لا العرض).

## 2) الخطوات
1. **الإعدادات:** جعل كل إعدادات `Show_*_OnInvoiceMonitor` أعلاه = 1 (بأداة `hidden-setting.ps1`، تبقى مفعَّلة حتى تقول)، ثم `POST /Dev/ReloadSettings`.
2. **الأعمدة فى صفحة المبيعات (`SalesInvoicePage.razor`):** جدول الأصناف الحالى يعرض الكمية والسعر والصافى فقط. إضافة أعمدة/سطور فرعية لكل قيمة مفعَّلة بإعدادها (نفس منطق الأصل: تظهر عند الإعداد = 1).
3. **الحقول فى حوار إضافة الصنف (`AddInvoiceItemDialog.razor`):** تظهر حسب نفس الإعدادات، بقيم افتراضية كالأصل (تاريخ الصلاحية = اليوم، الضريبة من إعداد الشركة...):
   عمولة (نسبة/قيمة/وحدة)، عدد القطع، ضريبة مبيعات ونسبة، ATS05 ونسبة، رقم البيان، رقم العملية، التاجر، التشغيلة، كمية مجانية (كبرى/صغرى)، قطع مجانية، تاريخ الصلاحية.
4. **الحساب:** `InvoiceLineCalculator.ComputeLine` يدعم أصلاً العمولة والضريبة؛ تُراجَع بقية القيم (مجانى، قيمة الكمية، بلا ضريبة).
5. **الاختبار بالمتصفح والقاعدة:** فاتورة آجلة جديدة بكل الحقول مملوءة → حفظ → التحقق من `Invoices_Details` (كل عمود) والصافى، حركة العميل، ورصيد المخزن (خطة الفواتير: المبيعات تنقص، بما فيه الكمية المجانية).
6. **التوثيق:** تسجيل الفروق فى `razor-vs-desktop-fixes-backlog.md` والإعدادات المغيَّرة (قيم قديمة/جديدة) هنا.

## 3) سجل التنفيذ (2026-09-24)
**الإعدادات المغيَّرة (تبقى مفعَّلة حتى تقول؛ للاسترجاع `hidden-setting.ps1 -Key <الاسم> -Set <القيمة القديمة>`):**
كل إعدادات `Show_*_OnInvoiceMonitor` المذكورة فى الجدول أعلاه صارت **1** (القيم القديمة موضحة بالجدول: كلها 0 عدا `Show_Commision_Value` و`Show_Item_CommissionValue` و`Show_ItemNo` كانت 1)، و**`IsCompute_3Mola_Value_OnInvoiceMonitor` من 0 إلى 1** (تفعيل حساب عمولة العدد: `_3Mola_Value`=3 لكل قطعة). غُيِّر بالخطأ ثم أُعيد لقيمته الأصلية 0: `Enabel_cbIs_TaxPer_Item_OnInvoiceMonitor` و`Use_cbIs_TaxPer_Item_OnInvoiceMonitor`.

**المبنى (Razor فقط، لا كود ديسك توب):**
- `Services/InvoiceColumnSettings.cs` (جديد): بوابات الإعدادات كالأصل (الرئيسى `Is_ActiveOrNot_Show_ColumnInvoiceMonitor` ثم الفرعية؛ القيمة "0" فى `Statement_Name`/`Tager_Kontra_Name` تعنى بلا اسم مخصص).
- `InvoiceLineCalculator.cs`: عمولة العدد `Compute3Mola` (تُضاف لإجمالى السطر وصافيه بعد الضريبة كـ`ItemData.ComputePrice` 1336-1338)، وفى الرأس `Total_List3Mola` ويُضاف للصافى كـ`NewFrmInvoive` 3885.
- `AddInvoiceItemDialog.razor`: قسم «بيانات إضافية للصنف» (`ShowExtraFields` من صفحة المبيعات): العدد، عمولة (نسبة/قيمة/وحدة/إجمالى)، ضريبة المبيعات وATS05 (تعمل فقط مع فاتورة ضريبية بضريبة لكل صنف)، عمولة العدد، رقم البيان، رقم العملية، التاجر، التشغيلة، كمية مجانية (كبرى/صغرى) وقطع مجانية، تاريخ الصلاحية، الإجمالى بلا ضريبة، قيمة الكمية.
- `SalesInvoicePage.razor`: شارات تحت كل صنف لكل عمود مفعَّل له قيمة (`inv-extras`/`inv-xchip` فى `InvoiceStyles.razor`). 25 مفتاح ترجمة بأربع لغات.

**الاختبار حياً (بالمتصفح والقاعدة):**
- فاتورة آجلة 1665 (عادية): كمية 3 × 100، عدد 4، عمولة 10% + 5 + 2×3 = 41، عمولة عدد 12، رقم بيان/عملية/تاجر/تشغيلة، مجانى 1 كبرى و2 قطع ⇒ الصافى **271.00** (259 بلا ضريبة + 12). القاعدة: كل أعمدة `Invoices_Details` صحيحة، `Total_List3Mola=12`، المخزون 1 → **−3** (3+1 مجانى)، عدد القطع −6 (4+2)، حركة العميل واحدة 271.
- فاتورة آجلة ضريبية للصنف 1666: كمية 2 × 200، عدد 5، عمولة 5% + 10 + 1×2 = 32، ضريبة 14% = 51.52، ATS05 1% = 3.68، عمولة عدد 15 ⇒ الصافى **430.84**؛ الرأس (`TaxSales10_Value`، `ATS05_Value`، `Total_List3Mola=15`، `IsInvoiceTaxInfo=1`) والسطر والمخزون (−3) وحركة العميل (430.84) كلها صحيحة.
- فتح سطر محفوظ للتعديل يُحمِّل كل القيم، وحفظ التعديل (كمية مجانية 1→2، رقم العملية) يعمل ويحفظ السطر.

**عيب محرك مكتشف (لم يُعدَّل، يحتاج قرارك لأنه كود مشترك بالديسك توب):** انظر backlog §حادى عشر.
**غير مُختبر:** شاشة مرتجع المبيعات بالحقول الإضافية، وتاريخ الصلاحية بقيمة غير اليوم.
