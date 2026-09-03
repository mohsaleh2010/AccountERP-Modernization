# ERP_V1_2.md — AI Project Context, Business Rules & Canonical Database Schema

> **تاريخ الإصدار:** 2026-08-11
> **نوع الملف:** Markdown / AI Project Context / Database Schema Reference
> **النسخة:** ERP_V1_2
> **الغرض:** مرجع موحد وشامل للذكاء الاصطناعي لفهم نظام ERP المحاسبي والمخزني وقاعدة بياناته وموديولاته وقواعد العمل قبل كتابة SQL أو C# أو EF/EF Core أو تقارير أو تعديلات بنيوية.
> **مصادر الدمج:** `ERP.md` + `ERP_V1_1.md`

---

# 0. Canonical Source-of-Truth Rules

1. **أسماء قاعدة البيانات هي المصدر المرجعي (Source of Truth).** يجب استخدام أسماء الجداول والأعمدة كما هي حرفيًا، بما في ذلك الأخطاء الإملائية أو المسافات أو الاختصارات التاريخية.
2. لا تفترض أن `Id` هو Primary Key إلا إذا ثبت ذلك من DDL أو كود التطبيق.
3. لا تعتبر أي عمود ينتهي بـ `Id` Foreign Key مؤكدًا إلا بعد إثبات وجود العلاقة من DDL أو كود التطبيق أو بيانات فعلية.
4. لا تحذف أو تعيد تسمية أو تدمج جداول `Temp` أو `BACK` أو `History` تلقائيًا.
5. عند وجود تعارض بين الوصف الوظيفي والمخطط الفعلي، **المخطط الفعلي هو المرجع البنيوي**، بينما قواعد العمل المعتمدة هنا هي المرجع الوظيفي إلى أن يتم تصحيحها صراحةً.
6. عند وجود قيمة أو علاقة غير محسومة، لا تخترع حلاً؛ سجّلها كـ `Known Gap / Requires Confirmation`.
7. لا تطبق Naming Conventions على قاعدة البيانات عند توليد C# أو EF.
8. عند تعديل البيانات المالية أو المخزنية استخدم `TRANSACTION`، وتحقق من النسخ الاحتياطي وسلامة البيانات.
9. عند إنشاء استعلام مالي أو مخزني، افحص الفرع والفترة والعملة والصنف والمخزن ونوع المستند قبل التجميع.
10. لا تخلط بين المبالغ الأصلية والمبالغ المكافئة `Equivalent_*`.

---

# 1. Project Scope

النظام هو **ERP متعدد الوحدات**، مع تركيز أساسي على المحاسبة والمخزون، إضافة إلى التكامل مع المبيعات والمشتريات والضرائب والفاتورة الإلكترونية وصيانة السيارات والحجوزات والموارد البشرية والبنوك والإنتاج وغيرها.

الوحدات الرئيسية الموجودة في قاعدة البيانات تشمل على الأقل:

- الحسابات العامة ودفتر الأستاذ ومراكز التكلفة.
- الفروع والعملات والفترات المالية.
- العملاء والموردين والأصناف والوحدات والمخازن.
- المبيعات والمشتريات والفواتير والمرتجعات.
- الضرائب والفاتورة الإلكترونية.
- صيانة السيارات وأوامر الشغل والمقايسات.
- الحجوزات والخدمات والباقات والجلسات.
- البنوك والخزائن والشيكات والمدفوعات.
- الموظفين والحضور والرواتب.
- الإنتاج والنسيج والتصنيع.
- العقارات/المقاولات والخدمات المرتبطة بها.
- السياحة والبرامج والتذاكر والفنادق والتأشيرات.
- المستخدمين والصلاحيات والإعدادات والتدقيق والأخطاء.

---

# 2. Consolidated Business Rules

- العملات والقيم المكافئة `Equivalent`.
- الفروع ومراكز التكلفة.
- المخازن وحركات المخزون.
- الإنتاج و`WIP`.
- `GRIR` ومطابقة فواتير الموردين.
- `COGS`.
- `PPV`.
- تسويات الجرد.
- تكامل القيود مع ورشة صيانة السيارات.

---

# 2. AI Working Rules

عند تحليل أو تعديل أو إضافة كود للنظام:

1. اعتبر هذا الملف **Project Context** وليس مجرد شرح نظري.
2. حافظ على أسماء الجداول والكلاسات والحقول كما هي ما لم يطلب المستخدم تغييرها.
3. لا تنشئ قيود تسوية إضافية لميزان المراجعة إذا كان الغرض مجرد تحقيق الاتزان البرمجي؛ الاتزان تتم معالجته في الذاكرة/الكود وفق القواعد المذكورة.
4. عند التعامل مع التقارير المالية، أعطِ أولوية لقيم `Equivalent_*` وفق القواعد المعتمدة.
5. احترم `Account_Follows` و`Head_Main_Account` في توجيه الحسابات.
6. عند التعامل مع المخزون، اربط التأثير الكمي بالتأثير المحاسبي وفق نوع الحركة.
7. عند وجود تعارض أو قيمة غير محسومة في هذا الملف، **لا تفترض الحل**؛ أشر إلى التعارض واطلب تحديد القاعدة المعتمدة.
8. لا تستبدل قاعدة من قواعد المشروع بممارسة محاسبية عامة دون التنبيه إلى ذلك.
9. عند اقتراح تغيير في Database Schema أو Business Logic، وضّح أثر التغيير على:
   - القيود.
   - الأرصدة.
   - التقارير.
   - العملات المكافئة.
   - المخزون.
   - الفروع.
   - مراكز التكلفة.

---

# 3. Core Business & Architectural Rules

## 3.1 Trial Balance Balancing

- لا يتم إنشاء قيود تسوية موازية داخل قاعدة البيانات لمجرد فرض اتزان ميزان المراجعة.
- تتم معالجة وضمان اتزان الميزان والقوائم المالية **برمجياً داخل التطبيق (In-Memory / C# Logic)** أثناء التجميع والعرض.
- الهدف هو عدم تحميل قاعدة البيانات بقيود تسوية إضافية لا تمثل معاملات فعلية.

## 3.2 Multi-Branch

يدعم النظام الفروع المتعددة.

يتم استخدام `Branch_Id` في الكيانات/السجلات ذات الصلة، ومنها:

- `Submain_Account`
- `Account_Entry_List`
- `Account_Entry_Master_Balance`
- `Account_Type`

يجب أن تظل التقارير والأرصدة قابلة للتجميع أو العرض حسب الفرع.

## 3.3 Multi-Currency

- تسجل الحركة بالعملة الأصلية.
- يتم حساب قيمة مكافئة بالعملة المحلية الرئيسية باستخدام `Factor`.
- تستخدم حقول `Equivalent_*` في المعالجة المالية والتقارير وفق القاعدة المعتمدة.

### قاعدة Equivalent

الحقول ذات البادئة:

```text
Equivalent_
```

تمثل القيم المحولة إلى العملة المحلية/الموحدة.

## 3.4 Equivalent Currency Priority

في الاستعلامات و`ViewModels` والتقارير المالية:

- تكون قيم `Equivalent_*` هي الأساس الافتراضي لمعالجة وعرض القيم المالية الموحدة.
- تستخدم قيمة `Factor` في تحويل العملة.

> **Important:** ورد في المصدر وصف لسجل العملة المكافئة بأنه `Equivalent_Account_Currency_Id = 0`، بينما تعريف الـ Entity المعروض يضع القيمة الافتراضية `= 1`. هذا التعارض يجب عدم حسمه تلقائياً؛ يجب الرجوع إلى قاعدة البيانات الفعلية قبل تعديل الكود.

## 3.5 Sub-module Integration

يوجد ربط مباشر بين القيود المالية وموديول ورشة صيانة السيارات باستخدام:

```text
CarsWorkOrdersId
CarsId
WorkOrderNumber
```

كما يمكن ربط أنواع الحسابات بالكيانات التشغيلية عبر:

```text
Table_Name
```

ومن أمثلة القيم:

```text
Customer
Supplier
Employee
Banks
```

## 3.6 Cost Centers

يدعم النظام مراكز التكلفة من خلال الحقول المرتبطة بمراكز التكلفة في تفاصيل القيود والأرصدة وأنواع الحسابات، ومنها:

```text
CostCenter_Id
CostCenter_CodeChar
Cost_Center_Tree_ID
```

---

# 4. Financial Account Routing

## 4.1 Account_Follows

يحدد `Account_Follows` القائمة/الحساب الختامي الذي يتبع له الحساب.

القيم المعتمدة المذكورة:

| Value | Meaning |
|---:|---|
| 1 | حساب التشغيل |
| 2 | حساب المتاجرة |
| 3 | حساب الأرباح والخسائر / قائمة الدخل |
| 4 | الميزانية العمومية |

## 4.2 Head_Main_Account

التصنيف الرئيسي للحساب:

| Value | Meaning |
|---:|---|
| 1 | الأصول Assets |
| 2 | الالتزامات Liabilities |
| 3 | حقوق الملكية Equity |
| 4 | المصروفات وتكلفة المبيعات Expenses / COGS |
| 5 | الإيرادات والمبيعات Revenues / Sales |

## 4.3 Financial Statement Classification

- `Account_Follows = 3` → حسابات قائمة الدخل / الأرباح والخسائر.
- `Account_Follows = 4` → حسابات الميزانية العمومية.

---

# 5. Core Entities

## 5.1 Submain_Account

يمثل حساباً في شجرة الحسابات.

```csharp

- حساب المخزن يكون تحت حساب الأصول المتداولة/المخزون.
- `COGS` يوجه إلى حساب المتاجرة أو قائمة الدخل وفق الإعدادات المشار إليها (`Account_Follows = 2 أو 3`).
- مصاريف الشراء والتخزين يمكن تحميلها على تكلفة الصنف أو تسجيلها كمصروفات تشغيل وفق سياسة النظام.

---

# 11. Inventory Transaction Types

| Transaction Type | Quantity Effect | Financial Effect | Accounting Effect |
|---|---|---|---|
| Goods Receipt / GRN | زيادة المخزون (+) | يولد قيداً محاسبياً | مدين: المخزون / دائن: الموردين أو GRIR |
| Goods Issue | نقص المخزون (-) | يولد قيداً محاسبياً | مدين: COGS أو المصروف / دائن: المخزون |
| Transfer | (-) من الراسل و(+) للمستلم | لا يولد قيداً إذا كانت القيمة نفسها | نقل كمي بين المخازن |
| Overage | زيادة (+) | قيد تسوية | مدين: المخزون / دائن: أرباح أو إيرادات التسوية |
| Shortage | نقص (-) | قيد تسوية | مدين: خسائر التسوية أو أمين المخزن / دائن: المخزون |
| Sales Return | زيادة (+) | قيد تكلفة | مدين: المخزون / دائن: COGS |
| Purchase Return | نقص (-) | قيد تكلفة | مدين: المورد / دائن: المخزون |

---

# 12. Inventory Valuation

## 12.1 Weighted Average Cost

الطريقة الافتراضية لتقييم تكلفة المخزون هي:

**المتوسط المرجح للتكلفة (Weighted Average Cost).**

معادلة التكلفة الجديدة:

```text
New Cost =
(
    (Current Qty × Current Cost)
    +
    (Incoming Qty × Incoming Cost)
)
/
(Current Qty + Incoming Qty)
```

## 12.2 Issue Does Not Recalculate Average

- حركات الصرف أو البيع تسحب بالتكلفة المتوسطة الحالية.
- الصرف لا يعيد حساب متوسط التكلفة.

## 12.3 Negative Stock

القاعدة الحالية:

- يُحظر صرف صنف يؤدي إلى رصيد سلبي برمجياً.
- السبب: منع فساد معادلات المتوسط المرجح والتكلفة.

> يمكن أن تكون هذه القاعدة قابلة للتحويل إلى Option في النظام وفق سياسة العميل، إذا تم اعتماد ذلك لاحقاً.

---

# 13. Inventory & Accounting Integration

## 13.1 Goods Receipt — GRN

### Business Rule

الاستلام المادي للبضاعة ينشئ أثراً مالياً فورياً في الميزانية، بغض النظر عن وصول فاتورة المورد.

### Automatic Journal

```text
من حـ/ مخزن الخامات أو الإنتاج التام
    (Account Type 34 أو 35)

إلى حـ/ بضاعة مستلمة بدون فاتورة
    GRIR — Account Type 38
```

---

# 14. Supplier Invoice Matching

## 14.1 GRIR Closing

عند إدخال فاتورة المورد المالية:

```text
من حـ/ بضاعة مستلمة بدون فاتورة
    GRIR — Account Type 38

إلى حـ/ الموردين
    Supplier Account — Type 2
```

## 14.2 PPV

إذا وجد فرق بين:

- سعر الفاتورة.
- سعر الاستلام المستندي.

يتم ترحيل الفرق إلى:

```text
PPV — Purchase Price Variance
Account Type = 41
```

---

# 15. Production / WIP

## 15.1 Issue Raw Materials to WIP

عند صرف الخامات المباشرة للإنتاج:

```text
من حـ/ الإنتاج تحت التشغيل
    WIP — Account Type 37

إلى حـ/ مخزن الخامات
    Raw Materials — Account Type 35
```

## 15.2 Assembly Completion

عند إتمام التصنيع:

```text
من حـ/ مخزن إنتاج تام
    Finished Goods — Account Type 34

إلى حـ/ الإنتاج تحت التشغيل
    WIP — Account Type 37
```

---

# 16. Sales / Delivery / COGS

عند البيع وتسليم البضاعة:

- يتم تسجيل قيد الإيراد.
- بالتزامن يتم تخفيض المخزون وإثبات تكلفة البضاعة المباعة.

### COGS Journal

```text
من حـ/ تكلفة البضاعة المباعة
    COGS — Account Type 39

إلى حـ/ مخزن الإنتاج التام أو الخامات
    Account Type 34 أو 35
```

---

# 17. Inventory Adjustments

عند إجراء جرد فعلي:

- يتم تعديل الكميات الدفترية وفق محضر الجرد.
- يتم توجيه الفرق إلى قائمة الدخل.

## 17.1 Shortage

```text
من حـ/ تسويات وعجز المخزون
    Account Type 40

إلى حـ/ المخزن المختص
    Account Type 34 أو 35
```

## 17.2 Surplus

```text
من حـ/ المخزن المختص
    Account Type 34 أو 35

إلى حـ/ تسويات وعجز المخزون
    Account Type 40
```

---

# 18. Account_Type — Inventory / Production Types

الأنواع الوظيفية المعتمدة في المصدر:

| ID | Name | Nature | Account_Follows | Head_Main_Account | Function |
|---:|---|---|---:|---:|---|
| 34 | Finished Goods Inventory | Debit | 4 | 1 | تقييم المنتجات الجاهزة للبيع |
| 35 | Raw Materials Inventory | Debit | 4 | 1 | تقييم المواد الخام ومستلزمات الإنتاج |
| 36 | Semi-Finished Goods | Debit | 4 | 1 | تقييم الأجزاء/المكونات الوسيطة غير المكتملة |
| 37 | Work In Process (WIP) | Debit | 4 | 1 | تجميع الخامات والأجور والمصروفات غير المباشرة أثناء التصنيع |
| 38 | GRIR | Credit | 4 | 2 | إثبات الاستلام المادي قبل ورود فاتورة المورد |
| 39 | COGS | Debit | 3 | 4 | مقابلة الإيراد بتكلفة المخزون المباع |
| 40 | Inventory Adjustments / Shrinkage | Debit / Credit | 3 | 4 | معالجة فروق الجرد الفعلي عن الدفتري |
| 41 | Purchase Price Variance (PPV) | Debit / Credit | 3 | 4 | تسجيل فروق أسعار الشراء |
| 42 | Opening Balance Clearing Account | — | — | — | الأرصدة الافتتاحية / حسابات التأسيس |

---

# 19. Inventory Account Mapping

يتم ربط الحساب المخزني في `Submain_Account` بنوعه من خلال:

```text
Account_Customer_Type_Value
```

بحيث يطابق `ID / Value` الخاص بنوع الحساب في `Account_Type`.

مثال:

```text
Submain_Account.Account_Customer_Type_Value
            ↓
Account_Type.Id / Value
            ↓
Inventory / WIP / COGS / GRIR / PPV ...
```

---

# 20. Inventory Multi-Branch Rules

كل حركة مخزنية يجب أن ترتبط بوضوح بـ:

```text
Branch_Id
Store_Id
```

وذلك لضمان:

- دقة أرصدة كل فرع.
- دقة تقييم المخزون.
- استخراج ميزان المراجعة التحليلي لكل فرع.
- فصل حركات المخازن بين الفروع.

> `Store_Id` ورد في قواعد العمل المخزنية، لكن Entity محددة له لم ترد ضمن المعلومات الحالية.

---

# 21. Inventory Equivalent Currency

تُقيم حركات المخزون وتقارير تقييم آخر المدة بالعملة المحلية الموحدة باستخدام:

```text
Equivalent_*
```

في `Account_Entry_Details`.

يتم الاعتماد على:

```text
Factor
```

لتحويل القيم إلى العملة المحلية.

---

# 22. Aggregated Inventory / Financial Balances

يتم عكس الحركات المخزنية المباشرة في:

```text
Account_Entry_Master_Balance
```

وذلك بهدف:

- سرعة استخراج تقارير `COGS`.
- سرعة تقييم المخزون.
- توفير أرصدة مجمعة للتقارير.
- دعم العرض اللحظي للأرصدة.

ورد في المصدر أن سجل العملة المكافئة الموحدة يستخدم:

```text
Equivalent_Account_Currency_Id = 0
```

لكن تعريف `Account_Entry_Master_Balance` المرفق يحتوي على:

```csharp
public long? Equivalent_Account_Currency_Id { get; set; } = 1;
```

**يجب اعتبار هذه نقطة تحتاج إلى تأكيد من قاعدة البيانات الفعلية.**

---

# 23. Inventory Journal Matrix

| Event | Debit | Credit |
|---|---|---|
| GRN | Inventory | GRIR |
| Supplier Invoice | GRIR | Supplier |
| Purchase Price Variance | حسب اتجاه الفرق | PPV |
| Raw Material Issue | WIP | Raw Materials |
| Production Completion | Finished Goods | WIP |
| Sale / Delivery — Cost | COGS | Inventory |
| Inventory Shortage | Inventory Adjustment | Inventory |
| Inventory Surplus | Inventory | Inventory Adjustment |
| Sales Return — Cost | Inventory | COGS |
| Purchase Return | Supplier | Inventory |

---

# 24. User-Facing Product Capabilities

## 24.1 Financial Reporting

- ميزان مراجعة تفاعلي.
- تتبع الأرصدة:
  - الافتتاحية.

- توجيه الحسابات تلقائياً حسب `Account_Follows`.
- استخدام العملة المكافئة في التقارير الموحدة.

## 24.2 Multi-Branch & Multi-Currency

- دعم الفروع المتعددة.
- دعم العملات الأجنبية.
- التحويل إلى العملة المحلية الموحدة.
- إمكانية عرض وتحليل الحسابات حسب الفرع.

## 24.3 Cost Centers

- دعم مراكز التكلفة المتعددة.
- ربط القيود بمراكز التكلفة.
- دعم الهياكل الشجرية لمراكز التكلفة.

## 24.4 Customer / Supplier / Entity Integration

يمكن ربط الحسابات بكيانات تشغيلية مثل:

```text
Customer
Supplier
Employee
Banks
```

عن طريق:

```text
Table_Name
```

## 24.5 Inventory

- مخازن متعددة.
- مخزون خامات.
- إنتاج تام.
- إنتاج غير مكتمل.
- WIP.
- GRIR.
- COGS.
- PPV.
- تسويات الجرد.
- تقييم بالمتوسط المرجح.
- منع الرصيد السلبي وفق القاعدة الحالية.

---

# 25. User Guide Topics to Be Built Later

عند اكتمال النظام، يمكن بناء دليل مستخدم يغطي:

1. مفهوم شجرة الحسابات.
2. إنشاء وإدارة الحسابات.
3. قراءة ميزان المراجعة.
4. الأرصدة الافتتاحية والحركة والأرصدة الختامية.
5. العملات المتعددة.
6. العملة المكافئة.
7. القوائم المالية.
8. حساب التشغيل.
9. حساب المتاجرة.
10. الأرباح والخسائر.
11. الميزانية العمومية.
12. مراكز التكلفة.
13. إدارة المخازن.
14. حركات الاستلام والصرف والتحويل.
15. الجرد والتسويات.
16. الإنتاج وWIP.
17. GRIR ومطابقة الموردين.
18. COGS.
19. PPV.
20. التقارير.

---

# 26. Product / Technical Positioning

## Accuracy & Performance

المعالجة البرمجية للاتزان والتجميع تقلل الحاجة إلى إنشاء قيود تسوية إضافية في قاعدة البيانات.

## Multi-Branch

النظام مصمم لدعم تعدد الفروع مع فصل وتحليل الأرصدة والحركات حسب `Branch_Id`.

## Multi-Currency

القيم الأصلية محفوظة، مع وجود قيم مكافئة بالعملة المحلية الموحدة.

## Financial Reporting

التقارير تعتمد على هيكل شجرة الحسابات وتوجيه `Account_Follows`.

## Inventory Integration

الدورة المخزنية مترابطة مع الدورة المحاسبية والإنتاجية.

---

# 27. Known Gaps / Items Requiring Confirmation

هذه النقاط ظهرت من المعلومات الحالية لكنها لم تُحسم بشكل كامل:

### 27.1 Equivalent Currency ID

يوجد تعارض بين:

```text
Equivalent_Account_Currency_Id = 0
```

كوصف للسجل الموحد،

وبين:

```csharp
Equivalent_Account_Currency_Id = 1
```

في الـ Entity.

**Action:** التحقق من القيمة الفعلية المعتمدة في قاعدة البيانات.

### 27.2 Store Entity

تم ذكر:

```text
Store_Id
```

في قواعد المخازن، لكن لم يتم تقديم Entity الخاصة بالمخزن.

**Action:** إضافة `Store` / `Warehouse` Entity عند توفرها.

### 27.3 Account_Follows Values 1 & 2

تم تعريف:

```text
1 = تشغيل
2 = متاجرة
3 = أرباح وخسائر
4 = ميزانية عمومية
```

لكن تعريف `Account_Type` يشرح صراحةً فقط:

```text
3 = Profit & Loss
4 = Balance Sheet
```

**Action:** الحفاظ على القيم الحالية وعدم تغييرها حتى يتم تأكيد تصميم القوائم المالية الكامل.

### 27.4 Inventory Transaction Entities

لم يتم تقديم Entities الخاصة بـ:

- Goods Receipt.
- Goods Issue.
- Transfer.
- Purchase Return.
- Sales Return.

                               v
                         Account_Type
                               |
                               v
                         Submain_Account
```

---

# 30. Accounting Principles Embedded in the Current Design

1. المخزون أصل متداول.
2. الزيادة في المخزون مدينة.
3. النقص في المخزون دائن.
4. GRN يمكن أن ينشئ أثراً مالياً قبل وصول فاتورة المورد.
5. GRIR حساب وسيط بين الاستلام والفاتورة.
6. فرق سعر الشراء يعالج عبر PPV.
7. صرف الخامات للإنتاج ينقل التكلفة إلى WIP.
8. إتمام الإنتاج ينقل التكلفة من WIP إلى المنتج التام.
9. البيع يثبت الإيراد ويخفض المخزون عبر COGS.
10. الجرد يوجه الفروق إلى حساب تسويات المخزون.
11. التقييم الافتراضي للمخزون هو Weighted Average Cost.
12. الصرف لا يعيد حساب المتوسط.
13. الرصيد السلبي ممنوع وفق القاعدة الحالية.
14. القيم المكافئة تستخدم كأساس للتقارير الموحدة.
15. الاتزان البرمجي لميزان المراجعة يتم في التطبيق وليس بواسطة قيود تسوية اصطناعية.

---


---

# 3. Database Schema — Canonical Tables from ERP_V1_1

> هذا القسم يحتوي على تعريفات جداول قاعدة البيانات كما وردت في `ERP_V1_1.md`. تم إدراج كل جدول مرة واحدة فقط. لا يتم اعتبار العلاقات المذكورة في أوصاف الجداول Foreign Keys فعلية إلا إذا كانت مؤكدة في SQL Schema.

## 01 — الحسابات العامة والمالية
### `Account_Branches`

**الاستخدام الوظيفي:** تعريف فروع المنشأة المستخدمة في النظام المالي والتشغيلي، مع بيانات الفرع والعملة والتواريخ التشغيلية.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Branch_Name` | `nvarchar(50)` | `NULL` |
| `Address` | `nvarchar(50)` | `NULL` |
| `Code` | `bigint` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Currency_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_BranchesimagesLogo`

**الاستخدام الوظيفي:** تخزين شعار الفرع كمسار للصورة و/أو بيانات صورة ثنائية لاستخدامه في المستندات والتقارير.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `ImageLo Path` | `nvarchar(500)` | `NULL` |
| `ImageData` | `image` | `NULL` |

---
### `Account_BranchesTaxInfo`

**الاستخدام الوظيفي:** بيانات الإعدادات الضريبية/الفاتورة الإلكترونية الخاصة بكل فرع.

**عدد الأعمدة:** 34

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `testtype` | `nvarchar(500)` | `NULL` |
| `CompanyName` | `nvarchar(500)` | `NULL` |
| `RowCode` | `nvarchar(500)` | `NULL` |
| `TaxNumber` | `nvarchar(500)` | `NULL` |
| `RegistrationNumber` | `nvarchar(500)` | `NULL` |
| `Phone1` | `nvarchar(500)` | `NULL` |
| `buildingNumber` | `nvarchar(500)` | `NULL` |
| `postalCode` | `nvarchar(500)` | `NULL` |
| `room` | `nvarchar(500)` | `NULL` |
| `Address` | `nvarchar(500)` | `NULL` |
| `floor` | `nvarchar(500)` | `NULL` |
| `BranchID` | `nvarchar(500)` | `NULL` |
| `RegionCity` | `nvarchar(500)` | `NULL` |
| `databasename` | `nvarchar(500)` | `NULL` |
| `lastuuid` | `nvarchar(500)` | `NULL` |
| `landmark` | `nvarchar(500)` | `NULL` |
| `additionalInformation` | `nvarchar(500)` | `NULL` |
| ` vernate` | `nvarchar(500)` | `NULL` |
| `Street` | `nvarchar(500)` | `NULL` |
| `BankName` | `nvarchar(500)` | `NULL` |
| `AccountNo` | `nvarchar(500)` | `NULL` |
| `IBANNumber` | `nvarchar(500)` | `NULL` |
| `BankAddress` | `nvarchar(500)` | `NULL` |
| `ClientId` | `nvarchar(500)` | `NULL` |
| `ClientSecret` | `nvarchar(500)` | `NULL` |
| `ActivityCode` | `nvarchar(500)` | `NULL` |
| `TokenPin` | `nvarchar(500)` | `NULL` |
| `documentTypeVersion` | `nvarchar(500)` | `NULL` |
| `ClientIdPOS` | `nvarchar(500)` | `NULL` |
| `ClientSecretPOS` | `nvarchar(500)` | `NULL` |
| `SerialDevice` | `nvarchar(500)` | `NULL` |
| `SystemVersion` | `nvarchar(500)` | `NULL` |
| `documentTypeVersionPOS` | `nvarchar(500)` | `NULL` |

---
### `Account_Ehlak_Type`

**الاستخدام الوظيفي:** دليل أنواع الإهلاك المستخدمة في إدارة الأصول الثابتة والحسابات.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Details`

**الاستخدام الوظيفي:** سطور القيود اليومية، وتحتوي على الحسابات والمبالغ ونوع الحركة وترتيب السطر.

**عدد الأعمدة:** 56

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Line_No` | `int` | `NOT NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `Submain_Account_ID_` | `bigint` | `NULL` |
| `SubAccount_NO` | `nvarchar(max)` | `NULL` |
| `CostCenter_Id` | `bigint` | `NULL` |
| `CostCenter_CodeChar` | `nvarchar(max)` | `NULL` |
| `Notes_Details` | `nvarchar(max)` | `NULL` |
| `Transaction_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Transaction_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Import_Value` | `decimal(18, 3)` | `NULL` |
| `Export_Value` | `decimal(18, 3)` | `NULL` |
| `IS_Post_Transfer_To_Gl_Flage` | `tinyint` | `NULL` |
| `DAT` | `tinyint` | `NULL` |
| `Account_Name` | `nvarchar(max)` | `NULL` |
| `Equivalent_Currency_Value` | `decimal(18, 0)` | `NULL` |
| `Account_Code` | `bigint` | `NULL` |
| `Account_Customer_Type_Value` | `bigint` | `NULL` |
| `Account_Customer_Type_ID` | `bigint` | `NULL` |
| `Account_Employee_Type_ID` | `bigint` | `NULL` |
| `Transaction_DateFrom` | `datetime` | `NULL` |
| `Transaction_DateTo` | `datetime` | `NULL` |
| `Balance_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Balance_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Account_Currency_ID` | `int` | `NULL` |
| `Entry_Currency_ID` | `int` | `NULL` |
| `CostCenter_Name` | `nvarchar(max)` | `NULL` |
| `Master_Submain_Account_Id` | `nvarchar(max)` | `NULL` |
| `Master_Submain_Account_Name` | `nvarchar(max)` | `NULL` |
| `Branch_Code` | `bigint` | `NULL` |
| `Factor` | `decimal(18, 3)` | `NULL` |
| `Currency_Primary_Name` | `nvarchar(max)` | `NULL` |
| `Currency_Secondary_Name` | `nvarchar(max)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `DataDictionaryProcessesListsId` | `bigint` | `NULL` |
| `Reference_Type_Table_Id` | `bigint` | `NULL` |
| `MainAccount_Id` | `bigint` | `NULL` |
| `DocType` | `nvarchar(500)` | `NULL` |
| `DocNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Submain_Account_ID_` → محتمل: `Submain_Account`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `DataDictionaryProcessesListsId` → محتمل: `DataDictionaryProcessesLists`
- `MainAccount_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`

---
### `Account_Entry_List`

**الاستخدام الوظيفي:** رأس القيود اليومية المحاسبية: رقم القيد، التاريخ، اليومية/الكود، العملة وبيانات الترحيل.

**عدد الأعمدة:** 61

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `journal_Code` | `int` | `NOT NULL` |
| `Entry_Date` | `smalldatetime` | `NOT NULL` |
| `Entry_No` | `bigint` | `NULL` |
| `Currency_ID` | `int` | `NULL` |
| `Rate` | `decimal(18, 3)` | `NULL` |
| `Entry_Notes` | `nvarchar(max)` | `NULL` |
| `IS_Make_AutomaticFromTar7eel` | `tinyint` | `NULL` |
| `Branch_Id_Entry` | `bigint` | `NOT NULL` |
| `dYear` | `int` | `NULL` |
| `dMonth` | `int` | `NULL` |
| `dDay` | `int` | `NULL` |
| `SerialChar` | `nvarchar(max)` | `NULL` |
| `POHead_Id` | `bigint` | `NULL` |
| `ActionType_Code` | `int` | `NULL` |
| `Receipt_Id` | `bigint` | `NULL` |
| `Company_Code` | `int` | `NULL` |
| `DAT` | `tinyint` | `NULL` |
| `HDay` | `smallint` | `NULL` |
| `HMonth` | `smallint` | `NULL` |
| `HYear` | `int` | `NULL` |
| `User_ID_Login` | `bigint` | `NULL` |
| `Package_Code` | `bigint` | `NULL` |
| `RefID` | `bigint` | `NULL` |
| `IsProfitLossClose` | `bit` | `NULL` |
| `ActionDate` | `smalldatetime` | `NULL` |
| `Process_Date` | `smalldatetime` | `NULL` |
| `CreationUser_Code` | `bigint` | `NULL` |
| `IsUpdated` | `bigint` | `NULL` |
| `Doc_No` | `nvarchar(max)` | `NULL` |
| `User_Id_Login_Name` | `nvarchar(max)` | `NULL` |
| `Branch_ID_Login` | `int` | `NULL` |
| `Entry_Total` | `decimal(18, 3)` | `NULL` |
| `Transform_Type` | `int` | `NULL` |
| `IS_Transfer_To_Gl_Flage` | `int` | `NULL` |
| `IS_Transfer_To_Gl_Flage_Str` | `nvarchar(max)` | `NULL` |
| `Transfer_To_Gl_DateTime` | `datetime` | `NULL` |
| `Year_Financial_Period_Id` | `int` | `NULL` |
| `IS_FirstBalance` | `int` | `NULL` |
| `Status_Code` | `int` | `NULL` |
| `Entry_Currency_Name` | `nvarchar(max)` | `NULL` |
| `Entry_Currency_ID` | `int` | `NULL` |
| `Factor` | `decimal(18, 3)` | `NULL` |
| `Order_Money_Id` | `bigint` | `NULL` |
| `User_ID_LoginUpdate` | `int` | `NULL` |
| `Journal_Name` | `nvarchar(300)` | `NULL` |
| `Reference_Type` | `int` | `NULL` |
| `Reference_Type_Table_Id` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `IsAutomaticEntryOrManualy` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_ID` → محتمل: `Currency`, `Currency_Factor`
- `Order_Money_Id` → محتمل: `Order_Money_Check`, `Order_Money`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_Balance`

**الاستخدام الوظيفي:** أرصدة الحسابات المحاسبية/ميزان المراجعة الناتج عن الحركات والفترات المالية.

**عدد الأعمدة:** 32

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Year_Financial_Period_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `Account_Currency_Id` | `bigint` | `NULL` |
| `Equivalent_Account_Currency_Id` | `bigint` | `NULL` |
| `First_Balance` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Madin_Value` | `decimal(18, 3)` | `NULL` |
| `Da2n_Value` | `decimal(18, 3)` | `NULL` |
| `Balance` | `decimal(18, 3)` | `NULL` |
| `Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Madin_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Da2n_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `IS_Madin_Da2n` | `int` | `NULL` |
| `Cost_Center_Tree_ID` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `bigint` | `NULL` |
| `Factor` | `decimal(18, 0)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Cost_Center_Tree_ID` → محتمل: `Cost_Center_Tree`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_Balance_CostCenter`

**الاستخدام الوظيفي:** أرصدة الحسابات مع تحليلها حسب مراكز التكلفة.

**عدد الأعمدة:** 33

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Year_Financial_Period_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Account_Currency_Id` | `int` | `NULL` |
| `Equivalent_Account_Currency_Id` | `int` | `NULL` |
| `First_Balance` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Madin_Value` | `decimal(18, 3)` | `NULL` |
| `Da2n_Value` | `decimal(18, 3)` | `NULL` |
| `Balance` | `decimal(18, 3)` | `NULL` |
| `Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Madin_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Da2n_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `IS_Madin_Da2n` | `int` | `NULL` |
| `Cost_Center_Tree_ID` | `int` | `NULL` |
| `Cost_Center_Id` | `bigint` | `NULL` |
| `Cost_Center_Name` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Cost_Center_Tree_ID` → محتمل: `Cost_Center_Tree`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_Balance_CostCenterTemp`

**الاستخدام الوظيفي:** جدول مؤقت لمعالجة وتجميع أرصدة مراكز التكلفة قبل إخراج التقارير.

**عدد الأعمدة:** 46

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Last_Balance_Zero` | `bigint` | `NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `Account_Name` | `nvarchar(100)` | `NULL` |
| `Final_Fist_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Fist_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Final_Last_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Last_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Final_Transaction_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Transaction_Da2n` | `decimal(18, 3)` | `NULL` |
| `SubAccount_NO` | `nvarchar(100)` | `NULL` |
| `Code` | `bigint` | `NULL` |
| `Account_Code` | `nvarchar(100)` | `NULL` |
| `MainAccount_Name` | `nvarchar(100)` | `NULL` |
| `MainAccount_NO` | `nvarchar(100)` | `NULL` |
| `Main_Account_Id` | `bigint` | `NULL` |
| `MainCode` | `bigint` | `NULL` |
| `Equivalent_First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `First_Balance` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Account_Currency_Id` | `int` | `NULL` |
| `Final_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Final_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Prev_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Prev_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Year_Financial_Period_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Account_Currency_Id` | `int` | `NULL` |
| `AccountCode1` | `int` | `NULL` |
| `AccountCode2` | `int` | `NULL` |
| `AccountCode3` | `int` | `NULL` |
| `AccountCode4` | `int` | `NULL` |
| `AccountCode5` | `int` | `NULL` |
| `AccountCode6` | `int` | `NULL` |
| `Account_Follows` | `int` | `NULL` |
| `Totals` | `decimal(18, 3)` | `NULL` |
| `Cost_Center_Id` | `bigint` | `NULL` |
| `Cost_Center_Name` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Main_Account_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_Balance_MezanTemp`

**الاستخدام الوظيفي:** جدول مؤقت لمعالجة وتجميع بيانات ميزان المراجعة.

**عدد الأعمدة:** 46

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Last_Balance_Zero` | `bigint` | `NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `Account_Name` | `nvarchar(100)` | `NULL` |
| `Final_Fist_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Fist_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Final_Last_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Last_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Final_Transaction_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Transaction_Da2n` | `decimal(18, 3)` | `NULL` |
| `SubAccount_NO` | `nvarchar(100)` | `NULL` |
| `Code` | `nvarchar(100)` | `NULL` |
| `Account_Code` | `nvarchar(100)` | `NULL` |
| `MainAccount_Name` | `nvarchar(100)` | `NULL` |
| `MainAccount_NO` | `nvarchar(100)` | `NULL` |
| `Main_Account_Id` | `bigint` | `NULL` |
| `MainCode` | `nvarchar(100)` | `NULL` |
| `Equivalent_First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `First_Balance` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Account_Currency_Id` | `int` | `NULL` |
| `Final_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Final_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Prev_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Prev_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Year_Financial_Period_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Account_Currency_Id` | `int` | `NULL` |
| `AccountCode1` | `int` | `NULL` |
| `AccountCode2` | `int` | `NULL` |
| `AccountCode3` | `int` | `NULL` |
| `AccountCode4` | `int` | `NULL` |
| `AccountCode5` | `int` | `NULL` |
| `AccountCode6` | `int` | `NULL` |
| `Account_Follows` | `int` | `NULL` |
| `Totals` | `decimal(18, 3)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Head_Main_Account` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Main_Account_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_BalanceBACK`

**الاستخدام الوظيفي:** نسخة احتياطية/مرجعية من بيانات الأرصدة المحاسبية.

**عدد الأعمدة:** 31

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Year_Financial_Period_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Account_Currency_Id` | `int` | `NULL` |
| `Equivalent_Account_Currency_Id` | `int` | `NULL` |
| `First_Balance` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Madin_Value` | `decimal(18, 3)` | `NULL` |
| `Da2n_Value` | `decimal(18, 3)` | `NULL` |
| `Balance` | `decimal(18, 3)` | `NULL` |
| `Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Madin_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Da2n_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `IS_Madin_Da2n` | `int` | `NULL` |
| `Cost_Center_Tree_ID` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Cost_Center_Tree_ID` → محتمل: `Cost_Center_Tree`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_BalanceFirstAccount_Temp`

**الاستخدام الوظيفي:** جدول مؤقت لمعالجة أرصدة الحسابات عند إعداد الرصيد الأولي.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Submain_Account_ID` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(500)` | `NULL` |
| `Account_Currency_Id` | `bigint` | `NULL` |
| `Cost_Center_Id` | `bigint` | `NULL` |
| `Cost_Center_Name` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_Transaction`

**الاستخدام الوظيفي:** حركات الحسابات المالية التفصيلية المرتبطة بالمستندات والحسابات.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Master_Submain_Account_Id` | `nchar(10)` | `NULL` |
| `Account_Entry_List_Id` | `nchar(10)` | `NULL` |
| `Account_Entry_Details_Id` | `nchar(10)` | `NULL` |
| `Account_Name_Str` | `nchar(10)` | `NULL` |
| `Account_No_Str` | `nchar(10)` | `NULL` |
| `Notes_Details` | `nchar(10)` | `NULL` |
| `Equivalent_Import_Value` | `nchar(10)` | `NULL` |
| `Equivalent_Export_Value` | `nchar(10)` | `NULL` |
| `Balance` | `nchar(10)` | `NULL` |
| `Line_No` | `nchar(10)` | `NULL` |
| `CostCenter_Id` | `nchar(10)` | `NULL` |
| `CostCenter_Name` | `nchar(10)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Account_Entry_List_Id` → محتمل: `Account_Entry_List`
- `Account_Entry_Details_Id` → محتمل: `Account_Entry_Details`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_TransactionCostCenterTemp`

**الاستخدام الوظيفي:** بيانات مؤقتة لمعالجة حركات الحسابات مع مراكز التكلفة.

**عدد الأعمدة:** 35

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Last_Balance_Zero` | `bigint` | `NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `Account_Name` | `nvarchar(100)` | `NULL` |
| `Final_Fist_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Fist_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Final_Last_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Last_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Final_Transaction_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Transaction_Da2n` | `decimal(18, 3)` | `NULL` |
| `SubAccount_NO` | `nvarchar(100)` | `NULL` |
| `Code` | `nvarchar(100)` | `NULL` |
| `Account_Code` | `nvarchar(100)` | `NULL` |
| `MainAccount_Name` | `nvarchar(100)` | `NULL` |
| `MainAccount_NO` | `nvarchar(100)` | `NULL` |
| `Main_Account_Id` | `bigint` | `NULL` |
| `MainCode` | `bigint` | `NULL` |
| `Equivalent_First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Account_Currency_Id` | `int` | `NULL` |
| `Final_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Final_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Prev_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Prev_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Cost_Center_Id` | `bigint` | `NULL` |
| `Cost_Center_Name` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Main_Account_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Entry_Master_TransactionTemp`

**الاستخدام الوظيفي:** جدول مؤقت لمعالجة الحركات المالية وعرض/تجميع كشوف الحساب.

**عدد الأعمدة:** 34

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Last_Balance_Zero` | `bigint` | `NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `Account_Name` | `nvarchar(100)` | `NULL` |
| `Final_Fist_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Fist_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Final_Last_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Last_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Final_Transaction_Madin` | `decimal(18, 3)` | `NULL` |
| `Final_Transaction_Da2n` | `decimal(18, 3)` | `NULL` |
| `SubAccount_NO` | `nvarchar(100)` | `NULL` |
| `Code` | `nvarchar(100)` | `NULL` |
| `Account_Code` | `nvarchar(100)` | `NULL` |
| `MainAccount_Name` | `nvarchar(100)` | `NULL` |
| `MainAccount_NO` | `nvarchar(100)` | `NULL` |
| `Main_Account_Id` | `bigint` | `NULL` |
| `MainCode` | `nvarchar(100)` | `NULL` |
| `Equivalent_First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Account_Currency_Id` | `int` | `NULL` |
| `Final_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Final_Export_Value` | `decimal(18, 3)` | `NULL` |
| `Prev_Import_Value` | `decimal(18, 3)` | `NULL` |
| `Prev_Export_Value` | `decimal(18, 3)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Main_Account_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_FirstBalanceFirstTime`

**الاستخدام الوظيفي:** تسجيل/معالجة الأرصدة الافتتاحية عند بدء النظام أو أول إنشاء للبيانات.

**عدد الأعمدة:** 31

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Submain_Account_ID` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Year_Financial_Period_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Account_Currency_Id` | `int` | `NULL` |
| `Equivalent_Account_Currency_Id` | `int` | `NULL` |
| `First_Balance` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Madin_Value` | `decimal(18, 3)` | `NULL` |
| `Da2n_Value` | `decimal(18, 3)` | `NULL` |
| `Balance` | `decimal(18, 3)` | `NULL` |
| `Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_First_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Madin_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Da2n_Value` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Madin` | `decimal(18, 3)` | `NULL` |
| `Equivalent_Balance_Da2n` | `decimal(18, 3)` | `NULL` |
| `IS_Madin_Da2n` | `int` | `NULL` |
| `Cost_Center_Tree_ID` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_ID` → محتمل: `Submain_Account`
- `Cost_Center_Tree_ID` → محتمل: `Cost_Center_Tree`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Transaction_Details`

**الاستخدام الوظيفي:** تفاصيل إضافية للحركات المالية بين الحسابات.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Account_ID` | `bigint` | `NULL` |
| `Main_Account_ID` | `bigint` | `NULL` |
| `Relation_Main_Account_ID` | `bigint` | `NULL` |
| `Relation_Account_ID` | `bigint` | `NULL` |
| `Transaction_Date` | `datetime` | `NULL` |
| `Transaction_type` | `int` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `Transaction_Value` | `decimal(18, 2)` | `NULL` |
| `Balance` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Main_Account_ID` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Type`

**الاستخدام الوظيفي:** دليل أنواع الحسابات والتصنيف الهرمي للحسابات.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `Value` | `int` | `NULL` |
| `Account_Follows` | `int` | `NULL` |
| `Head_Main_Account` | `int` | `NULL` |
| `Active` | `int` | `NULL` |
| `Main_Account_Id` | `int` | `NULL` |
| `Cost_Center_Tree_ID` | `int` | `NULL` |
| `Customer_Type` | `int` | `NULL` |
| `Table_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Main_Account_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `Cost_Center_Tree_ID` → محتمل: `Cost_Center_Tree`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Account_Type_Main_Account_Id`

**الاستخدام الوظيفي:** ربط أنواع الحسابات بالحسابات الرئيسية حسب الفروع أو الهيكل المالي.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Account_Type_Name` | `nvarchar(100)` | `NULL` |
| `Account_Type_Value` | `int` | `NULL` |
| `Account_Branch_Id` | `int` | `NULL` |
| `Main_Account_Id` | `bigint` | `NULL` |
| `Account_Name` | `nvarchar(100)` | `NULL` |
| `Account_Type_ValueGeneralCustomer` | `int` | `NULL` |
| `Account_Follows` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Account_Branch_Id` → محتمل: `Account_Branches`, `Account_BranchesTaxInfo`, `Account_BranchesimagesLogo`
- `Main_Account_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `AccountTaxInfo`

**الاستخدام الوظيفي:** إعدادات ومعلومات الضرائب المستخدمة في الحسابات والمعاملات.

**عدد الأعمدة:** 7

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `TaxName_Ar` | `nvarchar(500)` | `NULL` |
| `TaxName_En` | `nvarchar(500)` | `NULL` |
| `TaxRate` | `decimal(18, 2)` | `NULL` |
| `MainCode` | `nvarchar(500)` | `NULL` |
| `SubCode` | `nvarchar(500)` | `NULL` |
| `MinAllowedValueToComputeTax` | `decimal(18, 2)` | `NULL` |

---
### `AccountTempBalance`

**الاستخدام الوظيفي:** جدول مؤقت لمعالجة وعرض الأرصدة عبر مستويات الحسابات الهرمية.

**عدد الأعمدة:** 50

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `SubAccount_Id` | `bigint` | `NULL` |
| `Main_Account_Id` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(200)` | `NULL` |
| `IS_Last_Level` | `int` | `NULL` |
| `Account_Level` | `int` | `NULL` |
| `Year_Financial_Period_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Balance_Madin` | `decimal(18, 2)` | `NULL` |
| `Balance_Da2n` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Balance_Madin` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Balance_Da2n` | `decimal(18, 2)` | `NULL` |
| `Final_madin` | `decimal(18, 2)` | `NULL` |
| `Final_da2n` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_madin` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_da2n` | `decimal(18, 2)` | `NULL` |
| `Final_Balance_Value` | `decimal(18, 2)` | `NULL` |
| `Account_Currency_Id` | `int` | `NULL` |
| `Equivalent_Account_Currency_Id` | `int` | `NULL` |
| `Final_madin_1` | `decimal(18, 2)` | `NULL` |
| `Final_da2n_1` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_madin_1` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_da2n_1` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Name_1` | `nvarchar(200)` | `NULL` |
| `Final_madin_2` | `decimal(18, 2)` | `NULL` |
| `Final_da2n_2` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_madin_2` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_da2n_2` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Name_2` | `nvarchar(200)` | `NULL` |
| `Final_madin_3` | `decimal(18, 2)` | `NULL` |
| `Final_da2n_3` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_madin_3` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_da2n_3` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Name_3` | `nvarchar(200)` | `NULL` |
| `Final_madin_4` | `decimal(18, 2)` | `NULL` |
| `Final_da2n_4` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_madin_4` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_da2n_4` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Name_4` | `nvarchar(200)` | `NULL` |
| `Final_madin_5` | `decimal(18, 2)` | `NULL` |
| `Final_da2n_5` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_madin_5` | `decimal(18, 2)` | `NULL` |
| `Final_Equivalent_da2n_5` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Name_5` | `nvarchar(200)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Main_Account_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Cost_Center_ForAccounts`

**الاستخدام الوظيفي:** ربط مراكز التكلفة بالحسابات.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Cost_Center_Id` | `bigint` | `NULL` |
| `Submain_Account_Id` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Submain_Account_Id` → محتمل: `Submain_Account`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Cost_Center_Tree`

**الاستخدام الوظيفي:** الشجرة الهرمية لمراكز التكلفة.

**عدد الأعمدة:** 28

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Code_Str` | `nvarchar(50)` | `NULL` |
| `CodeChar2` | `nvarchar(50)` | `NULL` |
| `Name` | `nvarchar(100)` | `NULL` |
| `NameE` | `varchar(100)` | `NULL` |
| `HeadCode` | `bigint` | `NULL` |
| `Parent_Id` | `bigint` | `NULL` |
| `Cost_Level` | `tinyint` | `NULL` |
| `isLastLevel` | `tinyint` | `NULL` |
| `isActive` | `bit` | `NULL` |
| `Notes` | `nvarchar(1024)` | `NULL` |
| `Branch_Code` | `smallint` | `NULL` |
| `Cost_Type` | `bigint` | `NULL` |
| `AcceptManualTrans` | `tinyint` | `NULL` |
| `Code` | `bigint` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Currency_Str` | `nvarchar(50)` | `NULL` |
| `Head_Main_Cost` | `int` | `NULL` |
| `lastPartIncrease` | `int` | `NULL` |
| `FirstSubCode` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Currency`

**الاستخدام الوظيفي:** دليل العملات.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Currency_Name` | `nvarchar(100)` | `NULL` |
| `Currency_Name_EN` | `nvarchar(100)` | `NULL` |
| `Value` | `int` | `NOT NULL` |
| `IS_GL_Currency_Flage` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `code` | `nvarchar(500)` | `NULL` |
| `Tax_BranchesId` | `bigint` | `NULL` |
| `rate` | `decimal(18, 2)` | `NULL` |
| `Desc_en` | `nvarchar(500)` | `NULL` |
| `Desc_ar` | `nvarchar(500)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Tax_BranchesId` → محتمل: `Tax_Branches`, `Tax_BranchesConfig`

---
### `Currency_Factor`

**الاستخدام الوظيفي:** معاملات تحويل/عوامل العملات وأسعار أو معاملات التكافؤ.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Currency_Primary_Name` | `nvarchar(50)` | `NULL` |
| `Currency_Primary_ISOne` | `int` | `NULL` |
| `Currency_ID_Primary` | `int` | `NULL` |
| `Factor` | `decimal(18, 3)` | `NULL` |
| `Currency_ID_Secondary` | `int` | `NULL` |
| `Currency_Secondary_Name` | `nvarchar(50)` | `NULL` |
| `IS_TheSameCurrencies` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Equivalent`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Equivalent_Date` | `datetime` | `NULL` |
| `Equivalent_Value` | `decimal(18, 2)` | `NULL` |
| `Reason` | `nvarchar(4000)` | `NULL` |
| `Done` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Financial_Period`

**الاستخدام الوظيفي:** الفترات المالية وحالتها وتواريخها.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `DateFrom` | `datetime` | `NOT NULL` |
| `DateTo` | `datetime` | `NULL` |
| `Done` | `int` | `NULL` |
| `FirstPeriod` | `int` | `NULL` |
| `IS_Active` | `int` | `NULL` |
| `IS_Finished_Posted` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Financial_Period_Begining_Balance`

**الاستخدام الوظيفي:** الأرصدة الافتتاحية للفترات المالية.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Main_Account_ID` | `int` | `NULL` |
| `Account_Type_Value` | `nchar(10)` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `SubAccount_Unit_Item_Id` | `int` | `NULL` |
| `DateFrom` | `datetime` | `NULL` |
| `DateTo` | `datetime` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `Large_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Main_Account_ID` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Financial_Period_Stores_Balance`

**الاستخدام الوظيفي:** أرصدة المخازن عند/حسب الفترات المالية.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Account_ID` | `int` | `NULL` |
| `DateFrom` | `datetime` | `NULL` |
| `DateTo` | `datetime` | `NULL` |
| `RawMaterialFirst` | `decimal(18, 2)` | `NULL` |
| `RawMaterialLast` | `decimal(18, 2)` | `NULL` |
| `UnderWorkingMaterialFirst` | `decimal(18, 2)` | `NULL` |
| `UnderWorkingMaterialLast` | `decimal(18, 2)` | `NULL` |
| `FinalProductFirst` | `decimal(18, 2)` | `NULL` |
| `FinalProductLast` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Journal_Codes`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Journal_Name` | `nvarchar(50)` | `NULL` |
| `Code` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Main_Accounts`

**الاستخدام الوظيفي:** دليل الحسابات الرئيسية.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Account_Id` | `bigint` | `NOT NULL` |
| `Account_No` | `nvarchar(50)` | `NULL` |
| `Account_Name` | `nvarchar(50)` | `NULL` |
| `Main_Accounts_Balance` | `decimal(18, 2)` | `NULL` |
| `Currency_ID` | `int` | `NULL` |
| `Notes` | `nvarchar(1000)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_ID` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `MainAccount`

**الاستخدام الوظيفي:** دليل/بيانات الحسابات الرئيسية في الهيكل المحاسبي.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Parent_Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Submain_Account`

**الاستخدام الوظيفي:** دليل الحسابات الفرعية التابعة للحسابات الرئيسية.

**عدد الأعمدة:** 41

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Main_Account_Id` | `bigint` | `NULL` |
| `SubAccount_NO` | `nvarchar(50)` | `NULL` |
| `lastPartIncrease` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(500)` | `NULL` |
| `Head_Main_Account` | `int` | `NULL` |
| `Account_Customer_Type_ID` | `bigint` | `NULL` |
| `Account_Customer_Type_Value` | `int` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Notes` | `nvarchar(1000)` | `NULL` |
| `CanDelete` | `int` | `NULL` |
| `Account_Follows` | `int` | `NULL` |
| `Account_Nature_Type` | `int` | `NULL` |
| `Accept_Cost_Center_Flage` | `int` | `NULL` |
| `Cost_Center_Tree_ID` | `bigint` | `NULL` |
| `Account_Level` | `int` | `NULL` |
| `Account_Code` | `nvarchar(50)` | `NULL` |
| `Company_ID_For_GL` | `int` | `NULL` |
| `Company_Name_For_GL` | `nvarchar(50)` | `NULL` |
| `Transform_Type` | `int` | `NULL` |
| `Account_Employee_Type_ID` | `int` | `NULL` |
| `Branch_ID_for_UseAcconts` | `int` | `NULL` |
| `ConnectWith_Gl_Flage` | `int` | `NULL` |
| `IS_SubAccount_FollowingAccount` | `int` | `NULL` |
| `IS_SubAccount_AccountType` | `int` | `NULL` |
| `FirstSubCode` | `nvarchar(50)` | `NULL` |
| `code` | `bigint` | `NULL` |
| `Is_Last_Level` | `int` | `NULL` |
| `Branch_ID` | `bigint` | `NULL` |
| `Accept_EntryNew_Flage` | `int` | `NULL` |
| `AccountCode1` | `bigint` | `NULL` |
| `AccountCode2` | `bigint` | `NULL` |
| `AccountCode3` | `bigint` | `NULL` |
| `AccountCode4` | `bigint` | `NULL` |
| `AccountCode5` | `bigint` | `NULL` |
| `AccountCode6` | `bigint` | `NULL` |
| `Is_MasrofatAffect` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Account_Type_ValueGeneralCustomer` | `int` | `NULL` |
| `CashFlowType` | `int` | `NULL` |
| `User_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Main_Account_Id` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `Cost_Center_Tree_ID` → محتمل: `Cost_Center_Tree`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---

## 02 — صيانة السيارات والورش
### `CarEstimationsValuesDetails`

**الاستخدام الوظيفي:** تفاصيل بنود مقايسة/تقدير صيانة السيارة.

**عدد الأعمدة:** 56

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CarEstimationsValuesListId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_ItemsId` | `bigint` | `NULL` |
| `ItemCode` | `nvarchar(100)` | `NULL` |
| `Item_Cate ry_ID` | `bigint` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(100)` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `Item_Notes` | `nvarchar(500)` | `NULL` |
| `Order_PurshingId` | `bigint` | `NULL` |
| `StoreId` | `bigint` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `Is_NotShowOnInvoice` | `int` | `NULL` |
| `Is_SarfFromStore` | `int` | `NULL` |
| `Is_AddToInvoice` | `int` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `Item_Cate ry_Name` | `nvarchar(500)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `CarEstimationsValuesListId` → محتمل: `CarEstimationsValuesLists`
- `Item_Cate ry_ID` → محتمل: `Item_Cate ry`
- `Order_PurshingId` → محتمل: `Order_Purshing`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `CarEstimationsValuesLists`

**الاستخدام الوظيفي:** رأس مقايسات/تقديرات صيانة السيارات.

**عدد الأعمدة:** 64

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `StoreId` | `bigint` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `CustomerId` | `bigint` | `NULL` |
| `EstimationsNumber` | `bigint` | `NULL` |
| `EstimationsDate` | `datetime` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Total_ListPriceWithoutTax_Workmanship` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_SpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_Oils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_Workmanship` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_SpareParts` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_Oils` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListOils_Value` | `decimal(18, 2)` | `NULL` |
| `TotalListTaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `TafkeetNet_Total` | `nvarchar(max)` | `NULL` |
| `ReceptionEngineerName` | `nvarchar(100)` | `NULL` |
| `ReceptionEngineerId` | `bigint` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `Qnty_ValueSpareParts` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalSpareParts` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalOils` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Total_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Net_Total` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueOils` | `decimal(18, 2)` | `NULL` |
| `Total_ItemPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Currency_id` | `int` | `NULL` |
| `Equivalent_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total_PriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `ATS05ListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05ListOils_Value` | `decimal(18, 2)` | `NULL` |
| `TotalListATS05_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListOils_Value` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListSpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListOils` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionList` | `decimal(18, 2)` | `NULL` |
| `Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `List_Final_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `List_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `List_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `CommissionListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Currency_id` → محتمل: `Currency`, `Currency_Factor`

---
### `Cars`

**الاستخدام الوظيفي:** البيانات الأساسية للسيارات والعملاء/المالكين وخصائص السيارة.

**عدد الأعمدة:** 41

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CustomerId` | `bigint` | `NULL` |
| `CustomerId_` | `bigint` | `NULL` |
| `Cars_ID` | `bigint` | `NULL` |
| `CustomerIdOld` | `bigint` | `NULL` |
| `Acc_No_Cust` | `nvarchar(50)` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `CarColor` | `nvarchar(500)` | `NULL` |
| `ColorCode` | `nvarchar(500)` | `NULL` |
| `CarType` | `nvarchar(500)` | `NULL` |
| `Model` | `nvarchar(500)` | `NULL` |
| `Lookup_CarTypeId` | `bigint` | `NULL` |
| `Lookup_CarTypeModeLId` | `bigint` | `NULL` |
| `EngineNumber` | `nvarchar(500)` | `NULL` |
| `ChassisNumber` | `nvarchar(500)` | `NULL` |
| `FuelType` | `nvarchar(500)` | `NULL` |
| `Capacity` | `nvarchar(500)` | `NULL` |
| `Year` | `nvarchar(500)` | `NULL` |
| `TransmissionType` | `nvarchar(500)` | `NULL` |
| `Lookup_TransmissionTypeId` | `bigint` | `NULL` |
| `Lookup_CarShapeId` | `bigint` | `NULL` |
| `Lookup_CarShape` | `nvarchar(500)` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `WarrantyStartDate` | `datetime` | `NULL` |
| `WarrantyEndDate` | `datetime` | `NULL` |
| `InsuranceCompany` | `nvarchar(500)` | `NULL` |
| `Key_Code` | `nvarchar(500)` | `NULL` |
| `Car_Purch_Date` | `datetime` | `NULL` |
| `Car_Purch_KM` | `int` | `NULL` |
| `CarAddress` | `nvarchar(500)` | `NULL` |
| `DriverTel` | `nvarchar(500)` | `NULL` |
| `DriverName` | `nvarchar(500)` | `NULL` |
| `CustomerName` | `nvarchar(500)` | `NULL` |
| `Tax_No` | `nvarchar(500)` | `NULL` |
| `Active` | `int` | `NULL` |
| `Repeat` | `int` | `NULL` |
| `RepeatN` | `int` | `NULL` |
| `IsChangeOwner` | `int` | `NULL` |
| `OldId` | `bigint` | `NULL` |
| `NewId` | `bigint` | `NULL` |
| `customeridNew` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Lookup_CarTypeId` → محتمل: `Lookup_CarType`, `Lookup_CarTypeModel`
- `Lookup_CarTypeModeLId` → محتمل: `Lookup_CarTypeModel`
- `Lookup_TransmissionTypeId` → محتمل: `Lookup_TransmissionType`
- `Lookup_CarShapeId` → محتمل: `Lookup_CarShape`

---
### `CarsExaminations`

**الاستخدام الوظيفي:** فحوصات السيارة عند الدخول للورشة وتسجيل نتائج الفحص.

**عدد الأعمدة:** 32

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Paint_Report` | `nvarchar(max)` | `NULL` |
| `Mechanics_Report` | `nvarchar(max)` | `NULL` |
| `Suspension_Report` | `nvarchar(max)` | `NULL` |
| `ExaminationNotes` | `nvarchar(max)` | `NULL` |
| `CarNumber` | `nvarchar(max)` | `NULL` |
| `ChassisNumber` | `nvarchar(max)` | `NULL` |
| `CustomerId` | `bigint` | `NULL` |
| `CustomerName` | `nvarchar(max)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `ExaminationDate` | `datetime` | `NULL` |
| `ExaminationNumber` | `bigint` | `NULL` |
| `CarMeterOnEntry` | `nvarchar(500)` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `IsCarsExaminationClosed` | `int` | `NULL` |
| `CarsExaminationClosedDate` | `datetime` | `NULL` |
| `CarsExaminationStatus` | `nvarchar(500)` | `NULL` |
| `Order_MoneyId` | `bigint` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Currency_Id` | `bigint` | `NULL` |
| `CarsExaminationValue` | `decimal(18, 2)` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Lookup_CarTypeId` | `bigint` | `NULL` |
| `Lookup_CarTypeModelId` | `bigint` | `NULL` |
| `CarType` | `nvarchar(500)` | `NULL` |
| `Model` | `nvarchar(500)` | `NULL` |
| `Year` | `nvarchar(500)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Order_MoneyId` → محتمل: `Order_Money_Check`, `Order_Money`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `Lookup_CarTypeId` → محتمل: `Lookup_CarType`, `Lookup_CarTypeModel`
- `Lookup_CarTypeModelId` → محتمل: `Lookup_CarTypeModel`

---
### `CarsTemp`

**الاستخدام الوظيفي:** بيانات مؤقتة مرتبطة بمعالجة/بحث بيانات السيارات.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CarNumber` | `nvarchar(50)` | `NULL` |
| `Count` | `int` | `NULL` |

---
### `CarsWorkOrders`

**الاستخدام الوظيفي:** رأس أوامر شغل وصيانة السيارات، مع السيارة والعميل والحالة والتواريخ.

**عدد الأعمدة:** 50

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `StoreId` | `bigint` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `CustomerId` | `bigint` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CustomerSalesMan` | `nvarchar(200)` | `NULL` |
| `Tel` | `nvarchar(200)` | `NULL` |
| `Mobile` | `nvarchar(200)` | `NULL` |
| `Address` | `nvarchar(200)` | `NULL` |
| `CarMeterOnEntry` | `nvarchar(200)` | `NULL` |
| `WorkOrdersDate` | `datetime` | `NULL` |
| `EntryDateTime` | `datetime` | `NULL` |
| `EntryDateOnly` | `date` | `NULL` |
| `EntryTimeOnly` | `time(7)` | `NULL` |
| `StartWorkDateTime` | `datetime` | `NULL` |
| `ExpectedEndWorkDate` | `datetime` | `NULL` |
| `CustomerComplaint` | `nvarchar(max)` | `NULL` |
| `RequiredWorks` | `nvarchar(max)` | `NULL` |
| `ExaminationReport` | `nvarchar(max)` | `NULL` |
| `ReceptionEngineerId` | `bigint` | `NULL` |
| `ReceptionEngineerName` | `nvarchar(200)` | `NULL` |
| `ReceptionManName` | `nvarchar(200)` | `NULL` |
| `IsWorkOrderClosed` | `int` | `NULL` |
| `WorkOrderClosedDate` | `datetime` | `NULL` |
| `ReceptionManId` | `bigint` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Lookup_CarTypeModeLId` | `bigint` | `NULL` |
| `Lookup_CarTypeId` | `bigint` | `NULL` |
| `Last_Invoice_ListId` | `bigint` | `NULL` |
| `Lookup_CarEntryTaskeenId` | `bigint` | `NULL` |
| `Lookup_CarEntryTaskeenName` | `nvarchar(100)` | `NULL` |
| `LookUp_CarStatusLocationId` | `bigint` | `NULL` |
| `LookUp_CarStatusLocationName` | `nvarchar(100)` | `NULL` |
| `WorkOrderClosed_Reason` | `nvarchar(max)` | `NULL` |
| `CustomerOrderNumberPO` | `nvarchar(max)` | `NULL` |
| `TaxInvoiceNo` | `bigint` | `NULL` |
| `TaxInternalId` | `nvarchar(500)` | `NULL` |
| `IsUploadToPortal` | `int` | `NULL` |
| `KhamatQnty_Value` | `decimal(18, 2)` | `NULL` |
| `TaxReverseInternalId` | `nvarchar(500)` | `NULL` |
| `TaxReverseInvoiceNo` | `bigint` | `NULL` |
| `Last_Invoices_List_ReverseId` | `bigint` | `NULL` |
| `CustomerId_` | `bigint` | `NULL` |
| `CustomerId_CarOwner` | `bigint` | `NULL` |
| `IsInsuranceCompanyStaus` | `int` | `NULL` |
| `CustomerOrderNumberGR` | `nvarchar(max)` | `NULL` |
| `Customer_Id_RelatedToAccount` | `bigint` | `NULL` |
| `Repairs_NotDone` | `nvarchar(max)` | `NULL` |
| `IsInvoiceTaxInfo` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Lookup_CarTypeModeLId` → محتمل: `Lookup_CarTypeModel`
- `Lookup_CarTypeId` → محتمل: `Lookup_CarType`, `Lookup_CarTypeModel`
- `Lookup_CarEntryTaskeenId` → محتمل: `Lookup_CarEntryTaskeen`
- `LookUp_CarStatusLocationId` → محتمل: `LookUp_CarStatusLocation`

---
### `CarsWorkOrdersDeliverDetails`

**الاستخدام الوظيفي:** تفاصيل الأصناف/قطع الغيار التي تم صرفها أو تسليمها لأمر الشغل.

**عدد الأعمدة:** 62

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_ItemsId` | `bigint` | `NULL` |
| `ItemCode` | `nvarchar(100)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(500)` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `Item_Notes` | `nvarchar(500)` | `NULL` |
| `Order_PurshingId` | `bigint` | `NULL` |
| `StoreId` | `bigint` | `NULL` |
| `Is_SarfFromStore` | `int` | `NULL` |
| `Is_AddToInvoice` | `int` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `Is_NotShowOnInvoice` | `int` | `NULL` |
| `Detailes_PurshingId` | `bigint` | `NULL` |
| `CarsWorkOrdersRequestedDetailsId` | `bigint` | `NULL` |
| `Item_Cate ry_Id` | `bigint` | `NULL` |
| `Item_Cate ry_Name` | `nvarchar(500)` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `Order_Purshing_No` | `bigint` | `NULL` |
| `Order_Purshing_Date` | `datetime` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Order_PurshingId` → محتمل: `Order_Purshing`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Detailes_PurshingId` → محتمل: `Detailes_Purshing_Import_Export`, `Detailes_Purshing`
- `CarsWorkOrdersRequestedDetailsId` → محتمل: `CarsWorkOrdersRequestedDetails`
- `Item_Cate ry_Id` → محتمل: `Item_Cate ry`

---
### `CarsWorkOrdersDeliverItemBalances`

**الاستخدام الوظيفي:** حركات أرصدة الأصناف المصروفة لأوامر الشغل.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `WarshaStoresId` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `ItemCode` | `nvarchar(200)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `Is_NotShowOnInvoice` | `int` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |

---
### `CarsWorkOrdersItemProcessedDetails`

**الاستخدام الوظيفي:** تفاصيل الأصناف والخدمات التي تمت معالجتها داخل أمر الشغل.

**عدد الأعمدة:** 69

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_ItemsId` | `bigint` | `NULL` |
| `Detailes_PurshingId` | `bigint` | `NULL` |
| `ItemCode` | `nvarchar(100)` | `NULL` |
| `CarsWorkOrdersRequestedDetailsId` | `bigint` | `NULL` |
| `CarsWorkOrdersDeliverItemBalancesId` | `bigint` | `NULL` |
| `CarsWorkOrdersDeliverDetailsId` | `bigint` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(100)` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `Item_Notes` | `nvarchar(500)` | `NULL` |
| `Order_PurshingId` | `bigint` | `NULL` |
| `StoreId` | `bigint` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `Is_NotShowOnInvoice` | `int` | `NULL` |
| `Is_SarfFromStore` | `int` | `NULL` |
| `Is_AddToInvoice` | `int` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `Item_Cate ry_Id` | `bigint` | `NULL` |
| `Item_Cate ry_Name` | `nvarchar(500)` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `Emp_Name` | `nvarchar(500)` | `NULL` |
| `IsMasna3iatKhamat` | `int` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `Order_Purshing_No` | `bigint` | `NULL` |
| `Order_Purshing_Date` | `datetime` | `NULL` |
| `Qnty_Value_P_Price` | `decimal(18, 2)` | `NULL` |
| `Profits` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Detailes_PurshingId` → محتمل: `Detailes_Purshing_Import_Export`, `Detailes_Purshing`
- `CarsWorkOrdersRequestedDetailsId` → محتمل: `CarsWorkOrdersRequestedDetails`
- `CarsWorkOrdersDeliverItemBalancesId` → محتمل: `CarsWorkOrdersDeliverItemBalances`
- `CarsWorkOrdersDeliverDetailsId` → محتمل: `CarsWorkOrdersDeliverDetails`
- `Order_PurshingId` → محتمل: `Order_Purshing`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Item_Cate ry_Id` → محتمل: `Item_Cate ry`

---
### `CarsWorkOrdersItemProcessedEmployees`

**الاستخدام الوظيفي:** الموظفون المشاركون في تنفيذ أصناف/خدمات أمر الشغل.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CarsWorkOrdersItemProcessedDetailsId` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `Emp_Name` | `nvarchar(500)` | `NULL` |
| `Ratio` | `decimal(18, 2)` | `NULL` |
| `Value` | `decimal(18, 2)` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `CarsWorkOrdersItemReversedProcessedDetailsId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `CarsWorkOrdersItemProcessedDetailsId` → محتمل: `CarsWorkOrdersItemProcessedDetails`
- `CarsWorkOrdersItemReversedProcessedDetailsId` → محتمل: `CarsWorkOrdersItemReversedProcessedDetails`

---
### `CarsWorkOrdersItemReversedProcessedDetails`

**الاستخدام الوظيفي:** تفاصيل عكس/إلغاء عمليات معالجة الأصناف داخل أمر الشغل.

**عدد الأعمدة:** 69

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_ItemsId` | `bigint` | `NULL` |
| `Detailes_PurshingId` | `bigint` | `NULL` |
| `ItemCode` | `nvarchar(100)` | `NULL` |
| `CarsWorkOrdersRequestedDetailsId` | `bigint` | `NULL` |
| `CarsWorkOrdersDeliverItemBalancesId` | `bigint` | `NULL` |
| `CarsWorkOrdersDeliverDetailsId` | `bigint` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(100)` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `Item_Notes` | `nvarchar(500)` | `NULL` |
| `Order_PurshingId` | `bigint` | `NULL` |
| `StoreId` | `bigint` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `Is_NotShowOnInvoice` | `int` | `NULL` |
| `Is_SarfFromStore` | `int` | `NULL` |
| `Is_AddToInvoice` | `int` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `Item_Cate ry_Id` | `bigint` | `NULL` |
| `Item_Cate ry_Name` | `nvarchar(500)` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `Emp_Name` | `nvarchar(500)` | `NULL` |
| `IsMasna3iatKhamat` | `int` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `Order_Purshing_No` | `bigint` | `NULL` |
| `Order_Purshing_Date` | `datetime` | `NULL` |
| `Qnty_Value_P_Price` | `decimal(18, 2)` | `NULL` |
| `Profits` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Detailes_PurshingId` → محتمل: `Detailes_Purshing_Import_Export`, `Detailes_Purshing`
- `CarsWorkOrdersRequestedDetailsId` → محتمل: `CarsWorkOrdersRequestedDetails`
- `CarsWorkOrdersDeliverItemBalancesId` → محتمل: `CarsWorkOrdersDeliverItemBalances`
- `CarsWorkOrdersDeliverDetailsId` → محتمل: `CarsWorkOrdersDeliverDetails`
- `Order_PurshingId` → محتمل: `Order_Purshing`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Item_Cate ry_Id` → محتمل: `Item_Cate ry`

---
### `CarsWorkOrdersRequestedDetails`

**الاستخدام الوظيفي:** الأصناف أو الخدمات المطلوبة ضمن أمر الشغل.

**عدد الأعمدة:** 67

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_ItemsId` | `bigint` | `NULL` |
| `ItemCode` | `nvarchar(100)` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(500)` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `Item_Notes` | `nvarchar(500)` | `NULL` |
| `StoreId` | `bigint` | `NULL` |
| `Is_SarfFromStore` | `int` | `NULL` |
| `Is_AddToInvoice` | `int` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Is_ItemDeliver` | `bit` | `NULL` |
| `Larg_Qnty_Deliver` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty_Deliver` | `decimal(18, 2)` | `NULL` |
| `Item_No_Deliver` | `decimal(18, 2)` | `NULL` |
| `Qnty_Deliver` | `decimal(18, 2)` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `Is_NotShowOnInvoice` | `int` | `NULL` |
| `Order_PurshingId` | `bigint` | `NULL` |
| `Detailes_PurshingId` | `bigint` | `NULL` |
| `CarsWorkOrdersDeliverDetailsId` | `bigint` | `NULL` |
| `Item_Cate ry_Id` | `bigint` | `NULL` |
| `Item_Cate ry_Name` | `nvarchar(500)` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `Order_Purshing_No` | `bigint` | `NULL` |
| `Order_Purshing_Date` | `datetime` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Order_PurshingId` → محتمل: `Order_Purshing`
- `Detailes_PurshingId` → محتمل: `Detailes_Purshing_Import_Export`, `Detailes_Purshing`
- `CarsWorkOrdersDeliverDetailsId` → محتمل: `CarsWorkOrdersDeliverDetails`
- `Item_Cate ry_Id` → محتمل: `Item_Cate ry`

---
### `ItemsForCars`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Lookup_CarTypeId` | `bigint` | `NULL` |
| `Lookup_CarTypeModeLId` | `bigint` | `NULL` |
| `IsMain` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Lookup_CarTypeId` → محتمل: `Lookup_CarType`, `Lookup_CarTypeModel`
- `Lookup_CarTypeModeLId` → محتمل: `Lookup_CarTypeModel`

---
### `Lookup_CarEntryTaskeen`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---
### `Lookup_CarShape`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---
### `LookUp_CarStatusLocation`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---
### `Lookup_CarType`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(255)` | `NULL` |
| `NameEn` | `nvarchar(500)` | `NULL` |
| `Code` | `nvarchar(500)` | `NULL` |

---
### `Lookup_CarTypeModel`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(500)` | `NULL` |
| `Lookup_CarTypeId` | `bigint` | `NULL` |
| `CarType` | `nvarchar(500)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Lookup_CarTypeId` → محتمل: `Lookup_CarType`, `Lookup_CarTypeModel`

---

## 03 — الفواتير والضرائب
### `Bills_Details`

**الاستخدام الوظيفي:** تفاصيل بنود فواتير الشراء/المبيعات من أصناف وكميات وأسعار ووحدات.

**عدد الأعمدة:** 98

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Qnty_Another` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NOT NULL` |
| `AnotherPay` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `code` | `nvarchar(50)` | `NULL` |
| `Make_Order` | `int` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(100)` | `NULL` |
| `Order_Purshing_Id_Automatic` | `bigint` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `NameSpecification` | `nvarchar(500)` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Larg_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `P_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `P_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Qnty_Another_Balance` | `decimal(18, 2)` | `NULL` |
| `Is_BuyFrom_Bills` | `int` | `NULL` |
| `Audience_price` | `decimal(18, 2)` | `NULL` |
| `Item_Expire_Date` | `datetime` | `NULL` |
| `QntyWidth` | `decimal(18, 2)` | `NULL` |
| `QntyLenght` | `decimal(18, 2)` | `NULL` |
| `QntyItemNo` | `decimal(18, 2)` | `NULL` |
| `Is_InvoiceItem_Kta3y` | `int` | `NULL` |
| `LargItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `SmallItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `Is_Alternate_Item_CommissionValue` | `int` | `NULL` |
| `Alternate_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Alternate_Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_Item_ExpireDate_Id` | `bigint` | `NULL` |
| `Merchant_Name` | `nvarchar(500)` | `NULL` |
| `Tash3ela` | `nvarchar(500)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_No_Free` | `decimal(18, 2)` | `NULL` |
| `StatementNo` | `nvarchar(100)` | `NULL` |
| `OperationNo` | `nvarchar(100)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Balance_Checked` | `int` | `NULL` |
| `Supplier_BalanceSet` | `int` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Item_Notes` | `nvarchar(max)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TotalSmall_Qnty` | `decimal(18, 2)` | `NULL` |
| `TotalSmall_QntyStr` | `nvarchar(500)` | `NULL` |
| `TotalSmall_QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `Qnty_FreeStr` | `nvarchar(500)` | `NULL` |
| `Qnty_Free` | `decimal(18, 2)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `S_price` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Store_Name` | `nvarchar(500)` | `NULL` |
| `UnitSmallStr` | `nvarchar(500)` | `NULL` |
| `Available_Larg_Balance` | `decimal(18, 2)` | `NULL` |
| `Available_Small_Balance` | `decimal(18, 2)` | `NULL` |
| `Unit_Item_SalesDateId` | `bigint` | `NULL` |
| `Total3Mola` | `decimal(18, 2)` | `NULL` |
| `Unit3Mola` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Unit_Item_ExpireDate_Id` → محتمل: `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Bills_Details_Reverse`

**الاستخدام الوظيفي:** تفاصيل بنود الفواتير العكسية/المرتجعات.

**عدد الأعمدة:** 99

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Qnty_Another` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `AnotherPay` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Notes` | `nvarchar(50)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `code` | `nvarchar(50)` | `NULL` |
| `Make_Order` | `int` | `NULL` |
| `supplieraccountdefor` | `decimal(18, 2)` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(100)` | `NULL` |
| `Order_Purshing_Id_Automatic` | `bigint` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `NameSpecification` | `nvarchar(500)` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Audience_price` | `decimal(18, 2)` | `NULL` |
| `Item_Expire_Date` | `datetime` | `NULL` |
| `QntyWidth` | `decimal(18, 2)` | `NULL` |
| `QntyLenght` | `decimal(18, 2)` | `NULL` |
| `QntyItemNo` | `decimal(18, 2)` | `NULL` |
| `Is_InvoiceItem_Kta3y` | `int` | `NULL` |
| `LargItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `SmallItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `Is_Alternate_Item_CommissionValue` | `int` | `NULL` |
| `Alternate_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Alternate_Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_Item_ExpireDate_Id` | `bigint` | `NULL` |
| `Merchant_Name` | `nvarchar(500)` | `NULL` |
| `Tash3ela` | `nvarchar(500)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_No_Free` | `decimal(18, 2)` | `NULL` |
| `StatementNo` | `nvarchar(100)` | `NULL` |
| `OperationNo` | `nvarchar(100)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Balance_Checked` | `int` | `NULL` |
| `Supplier_BalanceSet` | `int` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Item_Notes` | `nvarchar(500)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TotalSmall_Qnty` | `decimal(18, 2)` | `NULL` |
| `TotalSmall_QntyStr` | `nvarchar(500)` | `NULL` |
| `TotalSmall_QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `P_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `P_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `Qnty_FreeStr` | `nvarchar(500)` | `NULL` |
| `Qnty_Free` | `decimal(18, 2)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `S_price` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Store_Name` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `Is_BuyFrom_Bills` | `int` | `NULL` |
| `Qnty_Another_Balance` | `decimal(18, 2)` | `NULL` |
| `UnitSmallStr` | `nvarchar(500)` | `NULL` |
| `Available_Larg_Balance` | `decimal(18, 2)` | `NULL` |
| `Available_Small_Balance` | `decimal(18, 2)` | `NULL` |
| `Unit_Item_SalesDateId` | `bigint` | `NULL` |
| `Total3Mola` | `decimal(18, 2)` | `NULL` |
| `Unit3Mola` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Unit_Item_ExpireDate_Id` → محتمل: `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Bills_List`

**الاستخدام الوظيفي:** رأس فواتير الشراء/المبيعات وفق تصميم النظام، مع بيانات الطرف والتاريخ والإجماليات.

**عدد الأعمدة:** 93

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `License_Id` | `int` | `NULL` |
| `Invoice_Date` | `datetime` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `commission` | `decimal(18, 2)` | `NULL` |
| `commissionValue` | `decimal(18, 2)` | `NULL` |
| `Net_Total` | `decimal(18, 2)` | `NULL` |
| `Currency` | `nvarchar(50)` | `NULL` |
| `Currency_id` | `int` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Invoice_Type` | `int` | `NULL` |
| `Editable` | `int` | `NULL` |
| `Deleted` | `int` | `NULL` |
| `Deleted_Date` | `datetime` | `NULL` |
| `Control_Cusstomer_Account` | `int` | `NULL` |
| `Supplier_BalanceSet` | `int` | `NULL` |
| `PolicaNo` | `nvarchar(50)` | `NULL` |
| `TroodContainerNo` | `nvarchar(50)` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `Load_Payment` | `decimal(18, 2)` | `NULL` |
| `Damaghat` | `decimal(18, 2)` | `NULL` |
| `Second_Total` | `decimal(18, 2)` | `NULL` |
| `Load_Payment_OnTax` | `decimal(18, 2)` | `NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `CustomerAccountBefor` | `decimal(18, 2)` | `NULL` |
| `CustomerAccountAfter` | `decimal(18, 2)` | `NULL` |
| `supplieraccountbefor` | `decimal(18, 2)` | `NULL` |
| `supplieraccountafter` | `decimal(18, 2)` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Total_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Equivalent_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total` | `decimal(18, 2)` | `NULL` |
| `Equivalent_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Total` | `decimal(18, 2)` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `IsCommission_Value` | `int` | `NULL` |
| `Salesman_Id` | `int` | `NULL` |
| `Is_Buy_From_Bills_Items_Balance` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `IS_Make_Sarf_Item_BalanceFromInvoice` | `int` | `NULL` |
| `Is_InvoiceKta3y` | `int` | `NULL` |
| `Is_Tax_Per_Item` | `int` | `NULL` |
| `Is_Load_Payment_OnTax` | `int` | `NULL` |
| `Total_PriceWithoutTax_Load_Payment` | `decimal(18, 2)` | `NULL` |
| `PriceByTenns` | `decimal(18, 2)` | `NULL` |
| `Total_PriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Invoice_DateTime` | `datetime` | `NULL` |
| `GuId` | `uniqueidentifier` | `NULL` |
| `List_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `List_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `List_Final_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Order_MoneyId` | `bigint` | `NULL` |
| `BanksId` | `int` | `NULL` |
| `BanksNamePaymentMethod` | `nvarchar(100)` | `NULL` |
| `PaidValue` | `decimal(18, 2)` | `NULL` |
| `RestValue` | `decimal(18, 2)` | `NULL` |
| `Emp_Name` | `nvarchar(500)` | `NULL` |
| `Supplier_Notes` | `nvarchar(100)` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `TafkeetNet_Total` | `nvarchar(max)` | `NULL` |
| `Is_Invoice_Agile` | `int` | `NULL` |
| `Is_Invoice_AgileStr` | `nvarchar(100)` | `NULL` |
| `IsInvoiceTaxInfo` | `int` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Total_ItemPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `IsMaintainceInvoice` | `int` | `NULL` |
| `Total_List3Mola` | `decimal(18, 2)` | `NULL` |
| `WahbaValue` | `decimal(18, 2)` | `NULL` |
| `NawlounValue` | `decimal(18, 2)` | `NULL` |
| `Total3MolaRate` | `decimal(18, 2)` | `NULL` |
| `Value_3Mola` | `decimal(18, 2)` | `NULL` |
| `InvoiceDueDate` | `datetime` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `License_Id` → محتمل: `License_Days`
- `Currency_id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `Order_MoneyId` → محتمل: `Order_Money_Check`, `Order_Money`
- `BanksId` → محتمل: `Banks`, `Banks_Balance`

---
### `Bills_List_Reverse`

**الاستخدام الوظيفي:** رأس الفواتير العكسية/المرتجعات.

**عدد الأعمدة:** 93

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `License_Id` | `int` | `NULL` |
| `Invoice_Date` | `datetime` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `commission` | `decimal(18, 2)` | `NULL` |
| `commissionValue` | `decimal(18, 2)` | `NULL` |
| `Net_Total` | `decimal(18, 2)` | `NULL` |
| `Currency` | `nvarchar(50)` | `NULL` |
| `Currency_id` | `int` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Invoice_Type` | `int` | `NULL` |
| `Editable` | `int` | `NULL` |
| `Deleted` | `int` | `NULL` |
| `Deleted_Date` | `datetime` | `NULL` |
| `Control_Cusstomer_Account` | `int` | `NULL` |
| `Supplier_BalanceSet` | `int` | `NULL` |
| `PolicaNo` | `nvarchar(50)` | `NULL` |
| `TroodContainerNo` | `nvarchar(50)` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `Load_Payment` | `decimal(18, 2)` | `NULL` |
| `Damaghat` | `decimal(18, 2)` | `NULL` |
| `Second_Total` | `decimal(18, 2)` | `NULL` |
| `Load_Payment_OnTax` | `decimal(18, 2)` | `NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `supplierAccountAfter` | `decimal(18, 2)` | `NULL` |
| `supplierAccountBefor` | `decimal(18, 2)` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Total_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Equivalent_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total` | `decimal(18, 2)` | `NULL` |
| `Equivalent_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Total` | `decimal(18, 2)` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `IsCommission_Value` | `int` | `NULL` |
| `Salesman_Id` | `int` | `NULL` |
| `Is_Buy_From_Bills_Items_Balance` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `IS_Make_Sarf_Item_BalanceFromInvoice` | `int` | `NULL` |
| `Is_InvoiceKta3y` | `int` | `NULL` |
| `Is_Tax_Per_Item` | `int` | `NULL` |
| `Is_Load_Payment_OnTax` | `int` | `NULL` |
| `Total_PriceWithoutTax_Load_Payment` | `decimal(18, 2)` | `NULL` |
| `PriceByTenns` | `decimal(18, 2)` | `NULL` |
| `Total_PriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Invoice_DateTime` | `datetime` | `NULL` |
| `GuId` | `uniqueidentifier` | `NULL` |
| `List_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `List_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `List_Final_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Order_MoneyId` | `bigint` | `NULL` |
| `BanksId` | `int` | `NULL` |
| `BanksNamePaymentMethod` | `nvarchar(100)` | `NULL` |
| `PaidValue` | `decimal(18, 2)` | `NULL` |
| `RestValue` | `decimal(18, 2)` | `NULL` |
| `Emp_Name` | `nvarchar(500)` | `NULL` |
| `Supplier_Notes` | `nvarchar(100)` | `NULL` |
| `Is_Invoice_Agile` | `int` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `TafkeetNet_Total` | `nvarchar(max)` | `NULL` |
| `Is_Invoice_AgileStr` | `nvarchar(100)` | `NULL` |
| `IsInvoiceTaxInfo` | `int` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `CustomerAccountBefor` | `decimal(18, 2)` | `NULL` |
| `CustomerAccountAfter` | `decimal(18, 2)` | `NULL` |
| `Total_ItemPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `IsMaintainceInvoice` | `int` | `NULL` |
| `Total_List3Mola` | `decimal(18, 2)` | `NULL` |
| `WahbaValue` | `decimal(18, 2)` | `NULL` |
| `NawlounValue` | `decimal(18, 2)` | `NULL` |
| `Total3MolaRate` | `decimal(18, 2)` | `NULL` |
| `Value_3Mola` | `decimal(18, 2)` | `NULL` |
| `InvoiceDueDate` | `datetime` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `License_Id` → محتمل: `License_Days`
- `Currency_id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `Order_MoneyId` → محتمل: `Order_Money_Check`, `Order_Money`
- `BanksId` → محتمل: `Banks`, `Banks_Balance`

---
### `Invoices_Details`

**الاستخدام الوظيفي:** تفاصيل بنود فواتير البيع/الخدمة.

**عدد الأعمدة:** 103

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Qnty_Another` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `code` | `nvarchar(50)` | `NULL` |
| `Make_Order` | `int` | `NULL` |
| `Balance_Checked` | `int` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(100)` | `NULL` |
| `Order_Purshing_Id_Automatic` | `bigint` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `NameSpecification` | `nvarchar(500)` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `QntyWidth` | `decimal(18, 2)` | `NULL` |
| `QntyLenght` | `decimal(18, 2)` | `NULL` |
| `QntyItemNo` | `decimal(18, 2)` | `NULL` |
| `Bills_Details_Id` | `bigint` | `NULL` |
| `Purchase_Price` | `decimal(18, 2)` | `NULL` |
| `Audience_price` | `decimal(18, 2)` | `NULL` |
| `Item_Expire_Date` | `datetime` | `NULL` |
| `Is_InvoiceItem_Kta3y` | `int` | `NULL` |
| `LargItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `SmallItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `Is_Alternate_Item_CommissionValue` | `int` | `NULL` |
| `Alternate_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Alternate_Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_Item_ExpireDate_Id` | `bigint` | `NULL` |
| `Merchant_Name` | `nvarchar(500)` | `NULL` |
| `Tash3ela` | `nvarchar(500)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_No_Free` | `decimal(18, 2)` | `NULL` |
| `StatementNo` | `nvarchar(100)` | `NULL` |
| `OperationNo` | `nvarchar(100)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Supplier_BalanceSet` | `int` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Item_Notes` | `nvarchar(max)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TotalSmall_Qnty` | `decimal(18, 2)` | `NULL` |
| `TotalSmall_QntyStr` | `nvarchar(500)` | `NULL` |
| `TotalSmall_QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `CarsWorkOrdersItemProcessedDetailsId` | `bigint` | `NULL` |
| `Qnty_FreeStr` | `nvarchar(500)` | `NULL` |
| `Qnty_Free` | `decimal(18, 2)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `Invoices_List_Reverse_Id` | `bigint` | `NULL` |
| `Available_Larg_Balance` | `decimal(18, 2)` | `NULL` |
| `Available_Small_Balance` | `decimal(18, 2)` | `NULL` |
| `Larg_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `UnitSmallStr` | `nvarchar(500)` | `NULL` |
| `Store_Name` | `nvarchar(500)` | `NULL` |
| `AppointmentsId` | `bigint` | `NULL` |
| `AppointmentServicesId` | `bigint` | `NULL` |
| `Unit_Item_SalesDateId` | `bigint` | `NULL` |
| `Total3Mola` | `decimal(18, 2)` | `NULL` |
| `Unit3Mola` | `decimal(18, 2)` | `NULL` |
| `InvoiceKitchenStatusId` | `bigint` | `NULL` |
| `WaiterManId` | `bigint` | `NULL` |
| `ProductCost_Price` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `S_priceAvgCost` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bills_Details_Id` → محتمل: `Bills_Details`, `Bills_Details_Reverse`
- `Unit_Item_ExpireDate_Id` → محتمل: `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `CarsWorkOrdersItemProcessedDetailsId` → محتمل: `CarsWorkOrdersItemProcessedDetails`
- `Invoices_List_Reverse_Id` → محتمل: `Invoices_List_Reverse`
- `AppointmentServicesId` → محتمل: `AppointmentServices`
- `InvoiceKitchenStatusId` → محتمل: `Lookup_InvoiceKitchenStatus`

---
### `Invoices_Details_Reverse`

**الاستخدام الوظيفي:** تفاصيل بنود مرتجعات/عكس الفواتير.

**عدد الأعمدة:** 102

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Qnty_Another` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Notes` | `nvarchar(50)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `code` | `nvarchar(50)` | `NULL` |
| `Make_Order` | `int` | `NULL` |
| `Balance_Checked` | `int` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(100)` | `NULL` |
| `Order_Purshing_Id_Automatic` | `bigint` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `NameSpecification` | `nvarchar(500)` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `QntyWidth` | `decimal(18, 2)` | `NULL` |
| `QntyLenght` | `decimal(18, 2)` | `NULL` |
| `QntyItemNo` | `decimal(18, 2)` | `NULL` |
| `Bills_Details_Id` | `bigint` | `NULL` |
| `Purchase_Price` | `decimal(18, 2)` | `NULL` |
| `Audience_price` | `decimal(18, 2)` | `NULL` |
| `Item_Expire_Date` | `datetime` | `NULL` |
| `Is_InvoiceItem_Kta3y` | `int` | `NULL` |
| `LargItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `SmallItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `Is_Alternate_Item_CommissionValue` | `int` | `NULL` |
| `Alternate_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Alternate_Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_Item_ExpireDate_Id` | `bigint` | `NULL` |
| `Merchant_Name` | `nvarchar(500)` | `NULL` |
| `Tash3ela` | `nvarchar(500)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_No_Free` | `decimal(18, 2)` | `NULL` |
| `StatementNo` | `nvarchar(100)` | `NULL` |
| `OperationNo` | `nvarchar(100)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Supplier_BalanceSet` | `int` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Item_Notes` | `nvarchar(500)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TotalSmall_Qnty` | `decimal(18, 2)` | `NULL` |
| `TotalSmall_QntyStr` | `nvarchar(500)` | `NULL` |
| `TotalSmall_QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `CarsWorkOrdersItemProcessedDetailsId` | `bigint` | `NULL` |
| `Qnty_FreeStr` | `nvarchar(500)` | `NULL` |
| `Qnty_Free` | `decimal(18, 2)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `Available_Larg_Balance` | `decimal(18, 2)` | `NULL` |
| `Available_Small_Balance` | `decimal(18, 2)` | `NULL` |
| `Larg_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `UnitSmallStr` | `nvarchar(500)` | `NULL` |
| `Store_Name` | `nvarchar(500)` | `NULL` |
| `AppointmentsId` | `bigint` | `NULL` |
| `AppointmentServicesId` | `bigint` | `NULL` |
| `Unit_Item_SalesDateId` | `bigint` | `NULL` |
| `Total3Mola` | `decimal(18, 2)` | `NULL` |
| `Unit3Mola` | `decimal(18, 2)` | `NULL` |
| `InvoiceKitchenStatusId` | `bigint` | `NULL` |
| `WaiterManId` | `bigint` | `NULL` |
| `ProductCost_Price` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `S_priceAvgCost` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bills_Details_Id` → محتمل: `Bills_Details`, `Bills_Details_Reverse`
- `Unit_Item_ExpireDate_Id` → محتمل: `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `CarsWorkOrdersItemProcessedDetailsId` → محتمل: `CarsWorkOrdersItemProcessedDetails`
- `AppointmentServicesId` → محتمل: `AppointmentServices`
- `InvoiceKitchenStatusId` → محتمل: `Lookup_InvoiceKitchenStatus`

---
### `Invoices_DetailsTemp`

**الاستخدام الوظيفي:** جدول مؤقت لمعالجة وتجميع بيانات أثناء تنفيذ عمليات النظام أو إعداد التقارير.

**عدد الأعمدة:** 102

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Specification` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Qnty_Another` | `decimal(18, 2)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `code` | `nvarchar(50)` | `NULL` |
| `Make_Order` | `int` | `NULL` |
| `Balance_Checked` | `int` | `NULL` |
| `Invoice_Item_Name` | `nvarchar(100)` | `NULL` |
| `Order_Purshing_Id_Automatic` | `bigint` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `NameSpecification` | `nvarchar(500)` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Is_Item_CommissionValue` | `int` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `QntyWidth` | `decimal(18, 2)` | `NULL` |
| `QntyLenght` | `decimal(18, 2)` | `NULL` |
| `QntyItemNo` | `decimal(18, 2)` | `NULL` |
| `Bills_Details_Id` | `bigint` | `NULL` |
| `Purchase_Price` | `decimal(18, 2)` | `NULL` |
| `Audience_price` | `decimal(18, 2)` | `NULL` |
| `Item_Expire_Date` | `datetime` | `NULL` |
| `Is_InvoiceItem_Kta3y` | `int` | `NULL` |
| `LargItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `SmallItemsQnty_Free` | `decimal(18, 2)` | `NULL` |
| `Is_Alternate_Item_CommissionValue` | `int` | `NULL` |
| `Alternate_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Alternate_Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `Alternate_Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_Item_ExpireDate_Id` | `bigint` | `NULL` |
| `Merchant_Name` | `nvarchar(500)` | `NULL` |
| `Tash3ela` | `nvarchar(500)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Item_No_Free` | `decimal(18, 2)` | `NULL` |
| `StatementNo` | `nvarchar(100)` | `NULL` |
| `OperationNo` | `nvarchar(100)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Supplier_BalanceSet` | `int` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Item_Notes` | `nvarchar(max)` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `TotalSmall_Qnty` | `decimal(18, 2)` | `NULL` |
| `TotalSmall_QntyStr` | `nvarchar(500)` | `NULL` |
| `TotalSmall_QntyStrColumn` | `nvarchar(500)` | `NULL` |
| `Store_Name` | `nvarchar(500)` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `AppointmentsId` | `bigint` | `NULL` |
| `AppointmentServicesId` | `bigint` | `NULL` |
| `CarsWorkOrdersItemProcessedDetailsId` | `bigint` | `NULL` |
| `Unit_Item_SalesDateId` | `bigint` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Available_Larg_Balance` | `decimal(18, 2)` | `NULL` |
| `Available_Small_Balance` | `decimal(18, 2)` | `NULL` |
| `Larg_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `UnitSmallStr` | `nvarchar(500)` | `NULL` |
| `Qnty_FreeStr` | `nvarchar(500)` | `NULL` |
| `Qnty_Free` | `decimal(18, 2)` | `NULL` |
| `Total3Mola` | `decimal(18, 2)` | `NULL` |
| `Unit3Mola` | `decimal(18, 2)` | `NULL` |
| `WaiterManId` | `bigint` | `NULL` |
| `ProductCost_Price` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `S_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `InvoiceKitchenStatusId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bills_Details_Id` → محتمل: `Bills_Details`, `Bills_Details_Reverse`
- `Unit_Item_ExpireDate_Id` → محتمل: `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `AppointmentServicesId` → محتمل: `AppointmentServices`
- `CarsWorkOrdersItemProcessedDetailsId` → محتمل: `CarsWorkOrdersItemProcessedDetails`
- `InvoiceKitchenStatusId` → محتمل: `Lookup_InvoiceKitchenStatus`

---
### `Invoices_List`

**الاستخدام الوظيفي:** رأس فواتير البيع/الخدمة المرتبطة بالعملاء، وقد ترتبط بأوامر شغل السيارات.

**عدد الأعمدة:** 190

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `License_Id` | `int` | `NULL` |
| `Invoice_Date` | `datetime` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Qnty_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Net_Total` | `decimal(18, 2)` | `NULL` |
| `Currency` | `nvarchar(50)` | `NULL` |
| `Currency_id` | `int` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Invoice_Type` | `int` | `NULL` |
| `Editable` | `int` | `NULL` |
| `Deleted` | `int` | `NULL` |
| `Deleted_Date` | `datetime` | `NULL` |
| `Control_Cusstomer_Account` | `int` | `NULL` |
| `Customer_BalanceSet` | `int` | `NULL` |
| `Make_Order_Purshing` | `int` | `NULL` |
| `PolicaNo` | `nvarchar(50)` | `NULL` |
| `troodContainerNo` | `nvarchar(50)` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `DAMGHAT` | `decimal(18, 2)` | `NULL` |
| `commissionValue` | `decimal(18, 2)` | `NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `CustomerAccountBefor` | `decimal(18, 2)` | `NULL` |
| `CustomerAccountAfter` | `decimal(18, 2)` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Total_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Equivalent_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total` | `decimal(18, 2)` | `NULL` |
| `Equivalent_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Total` | `decimal(18, 2)` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `IsCommission_Value` | `int` | `NULL` |
| `Salesman_Id` | `int` | `NULL` |
| `AnotherPayment` | `decimal(18, 2)` | `NULL` |
| `Is_Buy_From_Bills_Items_Balance` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `IS_Make_Sarf_Item_BalanceFromInvoice` | `int` | `NULL` |
| `Is_InvoiceKta3y` | `int` | `NULL` |
| `Is_Tax_Per_Item` | `int` | `NULL` |
| `Is_Load_Payment_OnTax` | `int` | `NULL` |
| `Total_PriceWithoutTax_Load_Payment` | `decimal(18, 2)` | `NULL` |
| `PriceByTenns` | `decimal(18, 2)` | `NULL` |
| `Total_PriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Invoice_DateTime` | `datetime` | `NULL` |
| `GuId` | `uniqueidentifier` | `NULL` |
| `List_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `List_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `List_Final_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Order_MoneyId` | `bigint` | `NULL` |
| `BanksId` | `int` | `NULL` |
| `BanksNamePaymentMethod` | `nvarchar(100)` | `NULL` |
| `PaidValue` | `decimal(18, 2)` | `NULL` |
| `RestValue` | `decimal(18, 2)` | `NULL` |
| `Emp_Name` | `nvarchar(500)` | `NULL` |
| `Customer_Notes` | `nvarchar(100)` | `NULL` |
| `Is_Invoice_Agile` | `int` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `ShipmentCompany_Id` | `bigint` | `NULL` |
| `ShipmentCompany_Name` | `nvarchar(500)` | `NULL` |
| `Shipment_Value` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueSpareParts` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueOils` | `decimal(18, 2)` | `NULL` |
| `ATS05ListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05ListOils_Value` | `decimal(18, 2)` | `NULL` |
| `TotalListATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListOils_Value` | `decimal(18, 2)` | `NULL` |
| `TotalListTaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListOils_Value` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListSpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListOils` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionList` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_Workmanship` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_SpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_Oils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_Workmanship` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_SpareParts` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_Oils` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalSpareParts` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalOils` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total` | `decimal(18, 2)` | `NULL` |
| `Total_ItemPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `TafkeetNet_Total` | `nvarchar(max)` | `NULL` |
| `Total_ListPriceAfterTaxSales_Workmanship` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_SpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_Oils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales` | `decimal(18, 2)` | `NULL` |
| `IsInvoiceTaxInfo` | `int` | `NULL` |
| `Qnty_ValueRawMaterials` | `decimal(2, 0)` | `NULL` |
| `Item_Net_TotalRawMaterials` | `decimal(2, 0)` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CustomerName` | `nvarchar(500)` | `NULL` |
| `IsMaintainceInvoice` | `int` | `NULL` |
| `TaxInvoices_ListId` | `bigint` | `NULL` |
| `TaxInvoiceNo` | `bigint` | `NULL` |
| `TaxInternalId` | `nvarchar(50)` | `NULL` |
| `IsUploadToPortal` | `int` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `Is_DeletedProcessed` | `int` | `NULL` |
| `Total_ListPriceWithoutTax_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Customer_Id_RelatedToAccount` | `bigint` | `NULL` |
| `Is_Invoice_AgileStr` | `nvarchar(100)` | `NULL` |
| `AppointmentsId` | `bigint` | `NULL` |
| `CustomerAccountAfterNakdia` | `decimal(18, 2)` | `NULL` |
| `PaymentValueFor_Is_Invoice_Agile` | `decimal(18, 2)` | `NULL` |
| `OverPaidAmount` | `decimal(18, 2)` | `NULL` |
| `IsUseExtraAmountToPayDebt` | `bit` | `NULL` |
| `Total_List3Mola` | `decimal(18, 2)` | `NULL` |
| `WahbaValue` | `decimal(18, 2)` | `NULL` |
| `NawlounValue` | `decimal(18, 2)` | `NULL` |
| `Total3MolaRate` | `decimal(18, 2)` | `NULL` |
| `Value_3Mola` | `decimal(18, 2)` | `NULL` |
| `CustomerPhone` | `nvarchar(max)` | `NULL` |
| `Delivery_Address` | `nvarchar(max)` | `NULL` |
| `WaiterManId` | `bigint` | `NULL` |
| `Money_Transaction_BoxId` | `bigint` | `NULL` |
| `Invoices_ListTempId` | `bigint` | `NULL` |
| `IsDeliveryFeeAddedToInvoiceTotal` | `int` | `NULL` |
| `IsDeliveryFeeAddedToInvoiceTotalAfterTax` | `int` | `NULL` |
| `Is_Reconciled_With_ManDriver` | `int` | `NULL` |
| `RestValueFromCashNakdia` | `decimal(18, 2)` | `NULL` |
| `PaidValueFromCashNakdy` | `decimal(18, 2)` | `NULL` |
| `CustomerDeliveryName` | `nvarchar(500)` | `NULL` |
| `Service_Rate` | `decimal(18, 2)` | `NULL` |
| `Service_Amount` | `decimal(18, 2)` | `NULL` |
| `Delivery_Fee` | `decimal(18, 2)` | `NULL` |
| `Lookup_ShiftsId` | `bigint` | `NULL` |
| `TableId` | `bigint` | `NULL` |
| `DeliveryMan_DriverId` | `bigint` | `NULL` |
| `Lookup_DiscountTypesId` | `bigint` | `NULL` |
| `Is_Printed` | `int` | `NULL` |
| `InvoiceStatus` | `int` | `NULL` |
| `InvoiceStatus_Reason` | `nvarchar(max)` | `NULL` |
| `InvoiceDueDate` | `datetime` | `NULL` |
| `Lookup_OrderDeliveryTypesId` | `bigint` | `NULL` |
| `Lookup_InvoiceKitchenStatusId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `License_Id` → محتمل: `License_Days`
- `Currency_id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `Order_MoneyId` → محتمل: `Order_Money_Check`, `Order_Money`
- `BanksId` → محتمل: `Banks`, `Banks_Balance`
- `ShipmentCompany_Id` → محتمل: `ShipmentCompany`
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`
- `Money_Transaction_BoxId` → محتمل: `Money_Transaction_BoxForInvoices`, `Money_Transaction_Box`
- `Invoices_ListTempId` → محتمل: `Invoices_ListTemp`
- `Lookup_ShiftsId` → محتمل: `Lookup_Shifts`
- `TableId` → محتمل: `Table_ColWillChange`, `Lookup_Tables`

---
### `Invoices_List_Reverse`

**الاستخدام الوظيفي:** رأس مرتجعات/عكس فواتير البيع.

**عدد الأعمدة:** 192

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `License_Id` | `int` | `NULL` |
| `Invoice_Date` | `datetime` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Qnty_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Net_Total` | `decimal(18, 2)` | `NULL` |
| `Currency` | `nvarchar(50)` | `NULL` |
| `Currency_id` | `int` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Invoice_Type` | `int` | `NULL` |
| `Editable` | `int` | `NULL` |
| `Deleted` | `int` | `NULL` |
| `Deleted_Date` | `datetime` | `NULL` |
| `Control_Cusstomer_Account` | `int` | `NULL` |
| `Customer_BalanceSet` | `int` | `NULL` |
| `Make_Order_Purshing` | `int` | `NULL` |
| `PolicaNo` | `nvarchar(50)` | `NULL` |
| `troodContainerNo` | `nvarchar(50)` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `DAMGHAT` | `decimal(18, 2)` | `NULL` |
| `commissionValue` | `decimal(18, 2)` | `NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `CustomerAccountAfter` | `decimal(18, 2)` | `NULL` |
| `CustomerAccountBefor` | `decimal(18, 2)` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Total_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Equivalent_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total` | `decimal(18, 2)` | `NULL` |
| `Equivalent_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Total` | `decimal(18, 2)` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `IsCommission_Value` | `int` | `NULL` |
| `Salesman_Id` | `int` | `NULL` |
| `AnotherPayment` | `decimal(18, 2)` | `NULL` |
| `Is_Buy_From_Bills_Items_Balance` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `IS_Make_Sarf_Item_BalanceFromInvoice` | `int` | `NULL` |
| `Is_InvoiceKta3y` | `int` | `NULL` |
| `Is_Tax_Per_Item` | `int` | `NULL` |
| `Is_Load_Payment_OnTax` | `int` | `NULL` |
| `Total_PriceWithoutTax_Load_Payment` | `decimal(18, 2)` | `NULL` |
| `PriceByTenns` | `decimal(18, 2)` | `NULL` |
| `Total_PriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Invoice_DateTime` | `datetime` | `NULL` |
| `GuId` | `uniqueidentifier` | `NULL` |
| `List_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `List_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `List_Final_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Order_MoneyId` | `bigint` | `NULL` |
| `BanksId` | `int` | `NULL` |
| `BanksNamePaymentMethod` | `nvarchar(100)` | `NULL` |
| `PaidValue` | `decimal(18, 2)` | `NULL` |
| `RestValue` | `decimal(18, 2)` | `NULL` |
| `Emp_Name` | `nvarchar(500)` | `NULL` |
| `Customer_Notes` | `nvarchar(100)` | `NULL` |
| `Is_Invoice_Agile` | `int` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `ShipmentCompany_Id` | `bigint` | `NULL` |
| `ShipmentCompany_Name` | `nvarchar(500)` | `NULL` |
| `Shipment_Value` | `decimal(18, 2)` | `NULL` |
| `Items_GroupsId` | `bigint` | `NULL` |
| `TafkeetNet_Total` | `nvarchar(max)` | `NULL` |
| `IsInvoiceTaxInfo` | `int` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CustomerName` | `nvarchar(500)` | `NULL` |
| `TaxInternalId` | `nvarchar(500)` | `NULL` |
| `TaxInvoiceNo` | `bigint` | `NULL` |
| `IsUploadToPortal` | `int` | `NULL` |
| `IsMaintainceInvoice` | `int` | `NULL` |
| `TaxInvoices_ListId` | `bigint` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `Qnty_ValueWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueSpareParts` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueOils` | `decimal(18, 2)` | `NULL` |
| `ATS05ListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05ListOils_Value` | `decimal(18, 2)` | `NULL` |
| `TotalListATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListOils_Value` | `decimal(18, 2)` | `NULL` |
| `TotalListTaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListOils_Value` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListSpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListOils` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionList` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_Workmanship` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_SpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_Oils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_Workmanship` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_SpareParts` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_Oils` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalSpareParts` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalOils` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total` | `decimal(18, 2)` | `NULL` |
| `Total_ItemPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `TafkeetNet_Total1` | `nvarchar(max)` | `NULL` |
| `Total_ListPriceAfterTaxSales_Workmanship` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_SpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_Oils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueRawMaterials` | `decimal(2, 0)` | `NULL` |
| `Item_Net_TotalRawMaterials` | `decimal(2, 0)` | `NULL` |
| `Is_DeletedProcessed` | `int` | `NULL` |
| `Total_ListPriceWithoutTax_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Customer_Id_RelatedToAccount` | `bigint` | `NULL` |
| `Is_Invoice_AgileStr` | `nvarchar(100)` | `NULL` |
| `AppointmentsId` | `bigint` | `NULL` |
| `CustomerAccountAfterNakdia` | `decimal(18, 2)` | `NULL` |
| `PaymentValueFor_Is_Invoice_Agile` | `decimal(18, 2)` | `NULL` |
| `OverPaidAmount` | `decimal(18, 2)` | `NULL` |
| `IsUseExtraAmountToPayDebt` | `bit` | `NULL` |
| `Total_List3Mola` | `decimal(18, 2)` | `NULL` |
| `WahbaValue` | `decimal(18, 2)` | `NULL` |
| `NawlounValue` | `decimal(18, 2)` | `NULL` |
| `Total3MolaRate` | `decimal(18, 2)` | `NULL` |
| `Value_3Mola` | `decimal(18, 2)` | `NULL` |
| `CustomerPhone` | `nvarchar(max)` | `NULL` |
| `Delivery_Address` | `nvarchar(max)` | `NULL` |
| `WaiterManId` | `bigint` | `NULL` |
| `Money_Transaction_BoxId` | `bigint` | `NULL` |
| `Invoices_ListTempId` | `bigint` | `NULL` |
| `IsDeliveryFeeAddedToInvoiceTotal` | `int` | `NULL` |
| `IsDeliveryFeeAddedToInvoiceTotalAfterTax` | `int` | `NULL` |
| `Is_Reconciled_With_ManDriver` | `int` | `NULL` |
| `RestValueFromCashNakdia` | `decimal(18, 2)` | `NULL` |
| `PaidValueFromCashNakdy` | `decimal(18, 2)` | `NULL` |
| `CustomerDeliveryName` | `nvarchar(500)` | `NULL` |
| `Service_Rate` | `decimal(18, 2)` | `NULL` |
| `Service_Amount` | `decimal(18, 2)` | `NULL` |
| `Delivery_Fee` | `decimal(18, 2)` | `NULL` |
| `Lookup_ShiftsId` | `bigint` | `NULL` |
| `TableId` | `bigint` | `NULL` |
| `DeliveryMan_DriverId` | `bigint` | `NULL` |
| `Lookup_DiscountTypesId` | `bigint` | `NULL` |
| `Is_Printed` | `int` | `NULL` |
| `InvoiceStatus` | `int` | `NULL` |
| `InvoiceStatus_Reason` | `nvarchar(max)` | `NULL` |
| `InvoiceDueDate` | `datetime` | `NULL` |
| `Lookup_OrderDeliveryTypesId` | `bigint` | `NULL` |
| `Lookup_InvoiceKitchenStatusId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `License_Id` → محتمل: `License_Days`
- `Currency_id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `Order_MoneyId` → محتمل: `Order_Money_Check`, `Order_Money`
- `BanksId` → محتمل: `Banks`, `Banks_Balance`
- `ShipmentCompany_Id` → محتمل: `ShipmentCompany`
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`
- `Money_Transaction_BoxId` → محتمل: `Money_Transaction_BoxForInvoices`, `Money_Transaction_Box`
- `Invoices_ListTempId` → محتمل: `Invoices_ListTemp`
- `Lookup_ShiftsId` → محتمل: `Lookup_Shifts`
- `TableId` → محتمل: `Table_ColWillChange`, `Lookup_Tables`

---
### `Invoices_ListTemp`

**الاستخدام الوظيفي:** جدول مؤقت لمعالجة وتجميع بيانات أثناء تنفيذ عمليات النظام أو إعداد التقارير.

**عدد الأعمدة:** 190

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `License_Id` | `int` | `NULL` |
| `Invoice_Date` | `datetime` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `TaxSales10` | `decimal(18, 2)` | `NULL` |
| `ATS05` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Net_Total` | `decimal(18, 2)` | `NULL` |
| `Currency` | `nvarchar(50)` | `NULL` |
| `Currency_id` | `int` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Invoice_Type` | `int` | `NULL` |
| `Editable` | `int` | `NULL` |
| `Deleted` | `int` | `NULL` |
| `Deleted_Date` | `datetime` | `NULL` |
| `Control_Cusstomer_Account` | `int` | `NULL` |
| `Customer_BalanceSet` | `int` | `NULL` |
| `Make_Order_Purshing` | `int` | `NULL` |
| `PolicaNo` | `nvarchar(50)` | `NULL` |
| `troodContainerNo` | `nvarchar(50)` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `DAMGHAT` | `decimal(18, 2)` | `NULL` |
| `commissionValue` | `decimal(18, 2)` | `NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `CustomerAccountBefor` | `decimal(18, 2)` | `NULL` |
| `CustomerAccountAfter` | `decimal(18, 2)` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Total_Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `Equivalent_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total` | `decimal(18, 2)` | `NULL` |
| `Equivalent_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Total` | `decimal(18, 2)` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `IsCommission_Value` | `int` | `NULL` |
| `Salesman_Id` | `int` | `NULL` |
| `AnotherPayment` | `decimal(18, 2)` | `NULL` |
| `Is_Buy_From_Bills_Items_Balance` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `IS_Make_Sarf_Item_BalanceFromInvoice` | `int` | `NULL` |
| `Is_InvoiceKta3y` | `int` | `NULL` |
| `Is_Tax_Per_Item` | `int` | `NULL` |
| `Is_Load_Payment_OnTax` | `int` | `NULL` |
| `Total_PriceWithoutTax_Load_Payment` | `decimal(18, 2)` | `NULL` |
| `PriceByTenns` | `decimal(18, 2)` | `NULL` |
| `Total_PriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Invoice_DateTime` | `datetime` | `NULL` |
| `GuId` | `uniqueidentifier` | `NULL` |
| `List_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `List_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `List_Final_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Equivalent_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Order_MoneyId` | `bigint` | `NULL` |
| `BanksId` | `int` | `NULL` |
| `BanksNamePaymentMethod` | `nvarchar(100)` | `NULL` |
| `PaidValue` | `decimal(18, 2)` | `NULL` |
| `RestValue` | `decimal(18, 2)` | `NULL` |
| `Emp_Name` | `nvarchar(500)` | `NULL` |
| `Customer_Notes` | `nvarchar(100)` | `NULL` |
| `Is_Invoice_Agile` | `int` | `NULL` |
| `Is_Invoice_AgileStr` | `nvarchar(100)` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `AppointmentsId` | `bigint` | `NULL` |
| `CustomerAccountAfterNakdia` | `decimal(18, 2)` | `NULL` |
| `PaymentValueFor_Is_Invoice_Agile` | `decimal(18, 2)` | `NULL` |
| `OverPaidAmount` | `decimal(18, 2)` | `NULL` |
| `IsUseExtraAmountToPayDebt` | `bit` | `NULL` |
| `IsInvoiceTaxInfo` | `int` | `NULL` |
| `Is_DeletedProcessed` | `int` | `NULL` |
| `IsMaintainceInvoice` | `int` | `NULL` |
| `TaxInvoices_ListId` | `bigint` | `NULL` |
| `IsUploadToPortal` | `int` | `NULL` |
| `TaxInternalId` | `nvarchar(500)` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CustomerName` | `nvarchar(500)` | `NULL` |
| `Qnty_ValueSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListSparePartsOils_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListSparePartsOils_Value` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalSparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_SparePartsOils` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueRawMaterials` | `decimal(18, 2)` | `NULL` |
| `TaxInvoiceNo` | `bigint` | `NULL` |
| `Total_ListPriceWithoutTax_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_RawMaterial` | `decimal(18, 2)` | `NULL` |
| `Customer_Id_RelatedToAccount` | `bigint` | `NULL` |
| `TafkeetNet_Total` | `nvarchar(max)` | `NULL` |
| `Qnty_Value` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueSpareParts` | `decimal(18, 2)` | `NULL` |
| `Qnty_ValueOils` | `decimal(18, 2)` | `NULL` |
| `ATS05ListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `ATS05ListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05ListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `ATS05ListOils_Value` | `decimal(18, 2)` | `NULL` |
| `TotalListATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSalesListOils_Value` | `decimal(18, 2)` | `NULL` |
| `TotalListTaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListWorkmanship_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListSpareParts_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListOils_Rate` | `decimal(18, 2)` | `NULL` |
| `CommissionListWorkmanship_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListSpareParts_Value` | `decimal(18, 2)` | `NULL` |
| `CommissionListOils_Value` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListSpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionListOils` | `decimal(18, 2)` | `NULL` |
| `Total_CommissionList` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_Workmanship` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_SpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax_Oils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_Workmanship` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_SpareParts` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal_Oils` | `decimal(18, 2)` | `NULL` |
| `NetInvoiceTotal` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalWorkmanship` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalSpareParts` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalOils` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total` | `decimal(18, 2)` | `NULL` |
| `Total_ItemPriceWithoutTax` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_Workmanship` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_SpareParts` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales_Oils` | `decimal(18, 2)` | `NULL` |
| `Total_ListPriceAfterTaxSales` | `decimal(18, 2)` | `NULL` |
| `Item_Net_TotalRawMaterials` | `decimal(18, 2)` | `NULL` |
| `Total_List3Mola` | `decimal(18, 2)` | `NULL` |
| `WahbaValue` | `decimal(18, 2)` | `NULL` |
| `NawlounValue` | `decimal(18, 2)` | `NULL` |
| `Total3MolaRate` | `decimal(18, 2)` | `NULL` |
| `Value_3Mola` | `decimal(18, 2)` | `NULL` |
| `ShipmentCompany_Id` | `bigint` | `NULL` |
| `ShipmentCompany_Name` | `nvarchar(500)` | `NULL` |
| `Shipment_Value` | `decimal(18, 2)` | `NULL` |
| `InvoiceDueDate` | `datetime` | `NULL` |
| `Lookup_OrderDeliveryTypesId` | `bigint` | `NULL` |
| `Lookup_InvoiceKitchenStatusId` | `bigint` | `NULL` |
| `Lookup_ShiftsId` | `bigint` | `NULL` |
| `CustomerPhone` | `nvarchar(max)` | `NULL` |
| `Delivery_Address` | `nvarchar(max)` | `NULL` |
| `Service_Rate` | `decimal(18, 2)` | `NULL` |
| `Service_Amount` | `decimal(18, 2)` | `NULL` |
| `Delivery_Fee` | `decimal(18, 2)` | `NULL` |
| `TableId` | `bigint` | `NULL` |
| `DeliveryMan_DriverId` | `bigint` | `NULL` |
| `Lookup_DiscountTypesId` | `bigint` | `NULL` |
| `Is_Printed` | `int` | `NULL` |
| `WaiterManId` | `bigint` | `NULL` |
| `CustomerDeliveryName` | `nvarchar(500)` | `NULL` |
| `IsDeliveryFeeAddedToInvoiceTotal` | `int` | `NULL` |
| `IsDeliveryFeeAddedToInvoiceTotalAfterTax` | `int` | `NULL` |
| `Is_Reconciled_With_ManDriver` | `int` | `NULL` |
| `Invoices_ListTempId` | `bigint` | `NULL` |
| `InvoiceStatus` | `int` | `NULL` |
| `InvoiceStatus_Reason` | `nvarchar(max)` | `NULL` |
| `RestValueFromCashNakdia` | `decimal(18, 2)` | `NULL` |
| `PaidValueFromCashNakdy` | `decimal(18, 2)` | `NULL` |
| `Money_Transaction_BoxId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `License_Id` → محتمل: `License_Days`
- `Currency_id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `Order_MoneyId` → محتمل: `Order_Money_Check`, `Order_Money`
- `BanksId` → محتمل: `Banks`, `Banks_Balance`
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`
- `ShipmentCompany_Id` → محتمل: `ShipmentCompany`
- `Lookup_OrderDeliveryTypesId` → محتمل: `Lookup_OrderDeliveryTypes`
- `Lookup_InvoiceKitchenStatusId` → محتمل: `Lookup_InvoiceKitchenStatus`
- `Lookup_ShiftsId` → محتمل: `Lookup_Shifts`
- `TableId` → محتمل: `Table_ColWillChange`, `Lookup_Tables`

---
### `TaxInvoices_ItemsTaxes`

**الاستخدام الوظيفي:** الضرائب المطبقة على بنود الفواتير الضريبية.

**عدد الأعمدة:** 8

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `TaxInvoices_ListId` | `bigint` | `NOT NULL` |
| `TaxInvoices_LineItemsId` | `bigint` | `NOT NULL` |
| `itemCode` | `nvarchar(max)` | `NULL` |
| `taxType` | `nvarchar(max)` | `NULL` |
| `amount` | `decimal(18, 2)` | `NULL` |
| `subType` | `nvarchar(max)` | `NULL` |
| `rate` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`
- `TaxInvoices_LineItemsId` → محتمل: `TaxInvoices_LineItems`

---
### `TaxInvoices_LineItems`

**الاستخدام الوظيفي:** بنود الفاتورة الضريبية الإلكترونية.

**عدد الأعمدة:** 31

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `TaxInvoices_ListId` | `bigint` | `NULL` |
| `ItemCodeTaxAcceptedId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Id` | `bigint` | `NULL` |
| `CurrencyId` | `bigint` | `NULL` |
| `description` | `nvarchar(max)` | `NULL` |
| `itemType` | `nvarchar(max)` | `NULL` |
| `itemCode` | `nvarchar(max)` | `NULL` |
| `unitType` | `nvarchar(max)` | `NULL` |
| `quantity` | `decimal(18, 2)` | `NULL` |
| `weightUnitType` | `nvarchar(max)` | `NULL` |
| `weightQuantity` | `decimal(18, 2)` | `NULL` |
| `unitValue_currencySold` | `nvarchar(max)` | `NULL` |
| `unitValue_amountSold` | `decimal(18, 2)` | `NULL` |
| `unitValue_amountEGP` | `decimal(18, 2)` | `NULL` |
| `unitValue_currencyExchangeRate` | `decimal(18, 2)` | `NULL` |
| `factoryUnitValue` | `nvarchar(max)` | `NULL` |
| `salesTotal` | `decimal(18, 2)` | `NULL` |
| `discount_rate` | `decimal(18, 2)` | `NULL` |
| `discount_amount` | `decimal(18, 2)` | `NULL` |
| `internalCode` | `nvarchar(max)` | `NULL` |
| `itemsDiscount` | `decimal(18, 2)` | `NULL` |
| `netTotal` | `decimal(18, 2)` | `NULL` |
| `totalTaxableFees` | `decimal(18, 2)` | `NULL` |
| `valueDifference` | `decimal(18, 2)` | `NULL` |
| `total` | `decimal(18, 2)` | `NULL` |
| `amountEGP` | `decimal(18, 2)` | `NULL` |
| `currencySold` | `nvarchar(200)` | `NULL` |
| `amountSold` | `decimal(18, 2)` | `NULL` |
| `currencyExchangeRate` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`
- `ItemCodeTaxAcceptedId` → محتمل: `ItemCodeTaxAccepted`
- `Unit_Item_Id` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`
- `CurrencyId` → محتمل: `Currency`, `Currency_Factor`

---
### `TaxInvoices_List`

**الاستخدام الوظيفي:** رأس مستندات الفواتير الضريبية/الإلكترونية.

**عدد الأعمدة:** 29

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Tax_BranchesId` | `bigint` | `NULL` |
| `Invoices_ListId` | `bigint` | `NULL` |
| `TaxInvoiceNo` | `bigint` | `NULL` |
| `internalID` | `nvarchar(max)` | `NULL` |
| `CustomerId` | `bigint` | `NULL` |
| `Receiver_type` | `nvarchar(max)` | `NULL` |
| `Receiver_id` | `nvarchar(max)` | `NULL` |
| `Receiver_name` | `nvarchar(max)` | `NULL` |
| `issuer_type` | `nvarchar(max)` | `NULL` |
| `issuer_id` | `nvarchar(max)` | `NULL` |
| `issuer_name` | `nvarchar(max)` | `NULL` |
| `documentType` | `nvarchar(max)` | `NULL` |
| `documentTypeVersion` | `nvarchar(max)` | `NULL` |
| `dateTimeIssued` | `datetime` | `NULL` |
| `taxpayerActivityCode` | `nvarchar(max)` | `NULL` |
| `purchaseOrderReference` | `nvarchar(max)` | `NULL` |
| `purchaseOrderDescription` | `nvarchar(max)` | `NULL` |
| `salesOrderReference` | `nvarchar(max)` | `NULL` |
| `salesOrderDescription` | `nvarchar(max)` | `NULL` |
| `proformaInvoiceNumber` | `nvarchar(max)` | `NULL` |
| `totalDiscountAmount` | `decimal(18, 2)` | `NULL` |
| `totalSalesAmount` | `decimal(18, 2)` | `NULL` |
| `netAmount` | `decimal(18, 2)` | `NULL` |
| `taxTotals` | `decimal(18, 2)` | `NULL` |
| `totalAmount` | `decimal(18, 2)` | `NULL` |
| `extraDiscountAmount` | `decimal(18, 2)` | `NULL` |
| `totalItemsDiscountAmount` | `decimal(18, 2)` | `NULL` |
| `submissionId` | `nvarchar(max)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Tax_BranchesId` → محتمل: `Tax_Branches`, `Tax_BranchesConfig`

---
### `TaxInvoices_TaxesTotal`

**الاستخدام الوظيفي:** إجماليات الضرائب على الفاتورة الضريبية.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `TaxInvoices_ListId` | `bigint` | `NOT NULL` |
| `taxType` | `nvarchar(max)` | `NULL` |
| `amount` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`

---
### `TaxReverseInvoices_ItemsTaxes`

**الاستخدام الوظيفي:** ضرائب بنود الفواتير الضريبية العكسية.

**عدد الأعمدة:** 8

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `TaxInvoices_ListId` | `bigint` | `NOT NULL` |
| `TaxReverseInvoices_LineItemsId` | `bigint` | `NOT NULL` |
| `itemCode` | `nvarchar(max)` | `NULL` |
| `taxType` | `nvarchar(max)` | `NULL` |
| `amount` | `decimal(18, 2)` | `NULL` |
| `subType` | `nvarchar(max)` | `NULL` |
| `rate` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`
- `TaxReverseInvoices_LineItemsId` → محتمل: `TaxReverseInvoices_LineItems`

---
### `TaxReverseInvoices_LineItems`

**الاستخدام الوظيفي:** بنود الفواتير الضريبية العكسية.

**عدد الأعمدة:** 31

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `TaxInvoices_ListId` | `bigint` | `NULL` |
| `ItemCodeTaxAcceptedId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Id` | `bigint` | `NULL` |
| `CurrencyId` | `bigint` | `NULL` |
| `description` | `nvarchar(max)` | `NULL` |
| `itemType` | `nvarchar(max)` | `NULL` |
| `itemCode` | `nvarchar(max)` | `NULL` |
| `unitType` | `nvarchar(max)` | `NULL` |
| `quantity` | `decimal(18, 2)` | `NULL` |
| `weightUnitType` | `nvarchar(max)` | `NULL` |
| `weightQuantity` | `decimal(18, 2)` | `NULL` |
| `unitValue_currencySold` | `nvarchar(max)` | `NULL` |
| `unitValue_amountSold` | `decimal(18, 2)` | `NULL` |
| `unitValue_amountEGP` | `decimal(18, 2)` | `NULL` |
| `unitValue_currencyExchangeRate` | `decimal(18, 2)` | `NULL` |
| `factoryUnitValue` | `nvarchar(max)` | `NULL` |
| `salesTotal` | `decimal(18, 2)` | `NULL` |
| `discount_rate` | `decimal(18, 2)` | `NULL` |
| `discount_amount` | `decimal(18, 2)` | `NULL` |
| `internalCode` | `nvarchar(max)` | `NULL` |
| `itemsDiscount` | `decimal(18, 2)` | `NULL` |
| `netTotal` | `decimal(18, 2)` | `NULL` |
| `totalTaxableFees` | `decimal(18, 2)` | `NULL` |
| `valueDifference` | `decimal(18, 2)` | `NULL` |
| `total` | `decimal(18, 2)` | `NULL` |
| `amountEGP` | `decimal(18, 2)` | `NULL` |
| `currencySold` | `nvarchar(200)` | `NULL` |
| `amountSold` | `decimal(18, 2)` | `NULL` |
| `currencyExchangeRate` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`
- `ItemCodeTaxAcceptedId` → محتمل: `ItemCodeTaxAccepted`
- `Unit_Item_Id` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`
- `CurrencyId` → محتمل: `Currency`, `Currency_Factor`

---
### `TaxReverseInvoices_List`

**الاستخدام الوظيفي:** رأس الفواتير الضريبية العكسية/الإشعارات العكسية.

**عدد الأعمدة:** 29

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Tax_BranchesId` | `bigint` | `NULL` |
| `Invoices_ListId` | `bigint` | `NULL` |
| `TaxInvoiceNo` | `bigint` | `NULL` |
| `internalID` | `nvarchar(max)` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Receiver_type` | `nvarchar(max)` | `NULL` |
| `Receiver_id` | `nvarchar(max)` | `NULL` |
| `Receiver_name` | `nvarchar(max)` | `NULL` |
| `issuer_type` | `nvarchar(max)` | `NULL` |
| `issuer_id` | `nvarchar(max)` | `NULL` |
| `issuer_name` | `nvarchar(max)` | `NULL` |
| `documentType` | `nvarchar(max)` | `NULL` |
| `documentTypeVersion` | `nvarchar(max)` | `NULL` |
| `dateTimeIssued` | `datetime` | `NULL` |
| `taxpayerActivityCode` | `nvarchar(max)` | `NULL` |
| `purchaseOrderReference` | `nvarchar(max)` | `NULL` |
| `purchaseOrderDescription` | `nvarchar(max)` | `NULL` |
| `salesOrderReference` | `nvarchar(max)` | `NULL` |
| `salesOrderDescription` | `nvarchar(max)` | `NULL` |
| `proformaInvoiceNumber` | `nvarchar(max)` | `NULL` |
| `totalDiscountAmount` | `decimal(18, 2)` | `NULL` |
| `totalSalesAmount` | `decimal(18, 2)` | `NULL` |
| `netAmount` | `decimal(18, 2)` | `NULL` |
| `taxTotals` | `decimal(18, 2)` | `NULL` |
| `totalAmount` | `decimal(18, 2)` | `NULL` |
| `extraDiscountAmount` | `decimal(18, 2)` | `NULL` |
| `totalItemsDiscountAmount` | `decimal(18, 2)` | `NULL` |
| `submissionId` | `nvarchar(max)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Tax_BranchesId` → محتمل: `Tax_Branches`, `Tax_BranchesConfig`

---
### `TaxReverseInvoices_TaxesTotal`

**الاستخدام الوظيفي:** إجماليات ضرائب الفواتير الضريبية العكسية.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `TaxInvoices_ListId` | `bigint` | `NOT NULL` |
| `taxType` | `nvarchar(max)` | `NULL` |
| `amount` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`

---

## 04 — الحجوزات والخدمات والاشتراكات
### `AppointmentCustomerPackages`

**الاستخدام الوظيفي:** الباقات التي اشتراها العملاء وحالتها ومدتها.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CustomerId` | `bigint` | `NOT NULL` |
| `AppointmentPackageId` | `bigint` | `NOT NULL` |
| `StartDate` | `date` | `NULL` |
| `EndDate` | `date` | `NULL` |
| `RemainingSessions` | `int` | `NULL` |
| `IsActive` | `bit` | `NULL` |
| `Invoices_ListId` | `bigint` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `CreatedAt` | `datetime` | `NULL` |
| `UpdatedAt` | `datetime` | `NULL` |
| `StatusName` | `nvarchar(max)` | `NULL` |
| `Status` | `int` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `AppointmentPackageId` → محتمل: `AppointmentPackageServices`, `AppointmentPackages`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `AppointmentCustomerPackageServices`

**الاستخدام الوظيفي:** استهلاك خدمات الباقة للعميل وعدد الجلسات المستخدمة والمتبقية.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `AppointmentCustomerPackageId` | `bigint` | `NULL` |
| `AppointmentPackageId` | `bigint` | `NULL` |
| `ItemService_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `StartDate` | `datetime` | `NULL` |
| `EndDate` | `datetime` | `NULL` |
| `UsedSessions` | `int` | `NULL` |
| `RemainingSessions` | `int` | `NULL` |
| `TotalSessions` | `int` | `NULL` |
| `Status` | `tinyint` | `NULL` |
| `CreatedAt` | `datetime` | `NULL` |
| `UpdatedAt` | `datetime` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `AppointmentCustomerPackageId` → محتمل: `AppointmentCustomerPackages`, `AppointmentCustomerPackageServices`
- `AppointmentPackageId` → محتمل: `AppointmentPackageServices`, `AppointmentPackages`
- `ItemService_Id` → محتمل: `LookUp_ItemServiceCate ry`

---
### `AppointmentPackages`

**الاستخدام الوظيفي:** تعريف باقات الخدمات والاشتراكات وعدد الجلسات والأسعار.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name_Package` | `nvarchar(200)` | `NOT NULL` |
| `ItemServiceId` | `bigint` | `NULL` |
| `ItemService_Name` | `nvarchar(max)` | `NULL` |
| `DurationInDays` | `int` | `NULL` |
| `TotalSessions` | `int` | `NULL` |
| `Price` | `decimal(18, 2)` | `NOT NULL` |
| `Description` | `nvarchar(max)` | `NULL` |
| `IsActive` | `bit` | `NOT NULL` |
| `CreatedAt` | `datetime` | `NULL` |
| `UpdatedAt` | `datetime` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ItemServiceId` → محتمل: `LookUp_ItemServiceCate ry`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `AppointmentPackageServices`

**الاستخدام الوظيفي:** الخدمات التي تتكون منها كل باقة.

**عدد الأعمدة:** 8

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `AppointmentPackageId` | `bigint` | `NULL` |
| `ItemServiceId` | `bigint` | `NULL` |
| `ItemService_Name` | `nvarchar(max)` | `NULL` |
| `TotalSessions` | `int` | `NULL` |
| `Description` | `nvarchar(max)` | `NULL` |
| `IsActive` | `bit` | `NULL` |
| `CreatedAt` | `datetime` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `AppointmentPackageId` → محتمل: `AppointmentPackageServices`, `AppointmentPackages`
- `ItemServiceId` → محتمل: `LookUp_ItemServiceCate ry`

---
### `Appointments`

**الاستخدام الوظيفي:** رأس الحجوزات والمواعيد للعملاء، بما يشمل التاريخ والوقت والحالة.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `AppointmentNumber` | `bigint` | `NULL` |
| `AppointmentDateTime` | `datetime` | `NULL` |
| `AppointmentDate` | `datetime` | `NULL` |
| `AppointmentTime` | `datetime` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `CustomerName` | `nvarchar(150)` | `NULL` |
| `CustomerPhone` | `nvarchar(50)` | `NULL` |
| `AppointmentNotes` | `nvarchar(max)` | `NULL` |
| `IsCompleteStatus` | `int` | `NULL` |

---
### `AppointmentServiceEmployees`

**الاستخدام الوظيفي:** الموظفون/الفنيون المشاركون في خدمة الحجز وقيم أو نسب عمولتهم.

**عدد الأعمدة:** 8

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `AppointmentServiceId` | `bigint` | `NULL` |
| `EmployeeId` | `bigint` | `NULL` |
| `ContributionPercentage` | `decimal(5, 2)` | `NULL` |
| `IsContributionPercentage` | `int` | `NULL` |
| `ContributionValue` | `decimal(5, 2)` | `NULL` |
| `AppointmentServiceEmployeeNotes` | `nvarchar(max)` | `NULL` |
| `IsCompletePayed` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `AppointmentServiceId` → محتمل: `AppointmentServices`, `AppointmentServiceEmployees`

---
### `AppointmentServices`

**الاستخدام الوظيفي:** الخدمات أو العناصر المرتبطة بكل حجز وبيانات تنفيذها/مدتها.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `AppointmentId` | `bigint` | `NULL` |
| `ServiceItem_Id` | `bigint` | `NULL` |
| `ServiceItem_Name` | `nvarchar(max)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `S_Price` | `decimal(18, 2)` | `NULL` |
| `QntyValue` | `decimal(18, 2)` | `NULL` |
| `DurationMinutes` | `int` | `NULL` |
| `AppointmentServiceNotes` | `nvarchar(max)` | `NULL` |
| `EmployeeNames` | `nvarchar(max)` | `NULL` |
| `IsProcessedAsInvoice` | `int` | `NULL` |
| `TotalSessions` | `int` | `NULL` |
| `Invoices_List_Id` | `bigint` | `NULL` |
| `Invoices_Details_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Invoices_Details_Id` → محتمل: `Invoices_Details_Reverse`, `Invoices_Details`, `Invoices_DetailsTemp`

---
### `AppointmentSessionEmployees`

**الاستخدام الوظيفي:** الموظفون المشاركون في تنفيذ الجلسة ومستحقاتهم/عمولاتهم.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `AppointmentSessionId` | `bigint` | `NULL` |
| `AppointmentId` | `bigint` | `NOT NULL` |
| `AppointmentServiceId` | `bigint` | `NULL` |
| `ServiceItem_Id` | `bigint` | `NULL` |
| `EmployeeId` | `bigint` | `NULL` |
| `ContributionPercentage` | `decimal(5, 2)` | `NULL` |
| `IsContributionPercentage` | `int` | `NULL` |
| `ContributionValue` | `decimal(5, 2)` | `NULL` |
| `AppointmentServiceEmployeeNotes` | `nvarchar(max)` | `NULL` |
| `IsCompletePayed` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `AppointmentSessionId` → محتمل: `AppointmentSessionEmployees`, `AppointmentSessions`
- `AppointmentServiceId` → محتمل: `AppointmentServices`, `AppointmentServiceEmployees`

---
### `AppointmentSessions`

**الاستخدام الوظيفي:** الجلسات الفعلية المنفذة ضمن الحجوزات أو الباقات.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `AppointmentId` | `bigint` | `NULL` |
| `AppointmentServiceId` | `bigint` | `NULL` |
| `AppointmentCustomerPackageServiceId` | `bigint` | `NULL` |
| `AppointmentCustomerPackageId` | `bigint` | `NULL` |
| `ServiceItem_Id` | `bigint` | `NULL` |
| `EmployeeId` | `bigint` | `NULL` |
| `SessionDate` | `datetime` | `NULL` |
| `Status` | `int` | `NULL` |
| `StatusStr` | `nvarchar(50)` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `BeforeImagePath` | `nvarchar(500)` | `NULL` |
| `AfterImagePath` | `nvarchar(500)` | `NULL` |
| `Rating` | `int` | `NULL` |
| `Feedback` | `nvarchar(max)` | `NULL` |
| `DurationinMinutes` | `int` | `NOT NULL` |
| `EmployeeNames` | `nvarchar(max)` | `NULL` |
| `IsCompletePayed` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `AppointmentServiceId` → محتمل: `AppointmentServices`, `AppointmentServiceEmployees`
- `AppointmentCustomerPackageServiceId` → محتمل: `AppointmentCustomerPackageServices`
- `AppointmentCustomerPackageId` → محتمل: `AppointmentCustomerPackages`, `AppointmentCustomerPackageServices`

---

## 05 — الأصناف والمخزون والوحدات
### `Item`

**الاستخدام الوظيفي:** الدليل الرئيسي للأصناف والخدمات، ويشمل الأكواد والأسماء والوحدات والأسعار والضرائب والمخزون وخصائص الخدمة.

**عدد الأعمدة:** 118

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NULL` |
| `code` | `nvarchar(max)` | `NULL` |
| `Code2` | `nvarchar(500)` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `AnotherName` | `nvarchar(500)` | `NULL` |
| `ItemName_OnInvoice` | `nvarchar(500)` | `NULL` |
| `item_Name_` | `nvarchar(500)` | `NULL` |
| `S_id` | `bigint` | `NULL` |
| `Item_Cate ry_ID` | `bigint` | `NULL` |
| `No3eboa_old` | `decimal(18, 2)` | `NULL` |
| `No3eboa` | `decimal(18, 2)` | `NULL` |
| `No3eboa_BigUnit` | `decimal(18, 2)` | `NULL` |
| `No3eboa_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Active_flage` | `int` | `NULL` |
| `Upon_Request` | `decimal(18, 2)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `StoreUsed_Type` | `int` | `NULL` |
| `Items_Types_Name_Id` | `int` | `NULL` |
| `Items_Types_Name` | `nvarchar(200)` | `NULL` |
| `Width_` | `decimal(18, 2)` | `NULL` |
| `Height_` | `decimal(18, 2)` | `NULL` |
| `Somk` | `decimal(18, 2)` | `NULL` |
| `Somk_Unit` | `nvarchar(100)` | `NULL` |
| `Height_Width_Str` | `nvarchar(500)` | `NULL` |
| `Items_Name_Somk_Str` | `nvarchar(500)` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `S_Price` | `decimal(18, 2)` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `P_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `P_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_Item_No` | `decimal(18, 2)` | `NULL` |
| `P_price_Item_No` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_For_S_price_Default` | `int` | `NULL` |
| `Unit_Used_For_P_price_Default` | `int` | `NULL` |
| `Is_ItemWithoutBalance` | `int` | `NULL` |
| `Items_TypesDescription_Name` | `nvarchar(200)` | `NULL` |
| `Items_TypesDescriptionNameOther` | `nvarchar(200)` | `NULL` |
| `Items_TypesFactory_Name` | `nvarchar(200)` | `NULL` |
| `Items_TypesSize_Name` | `nvarchar(200)` | `NULL` |
| `Items_TypesColor_Name` | `nvarchar(200)` | `NULL` |
| `Items_TypesDegreeFarz_Name` | `nvarchar(200)` | `NULL` |
| `Items_TypesDescription_NameId` | `int` | `NULL` |
| `Items_TypesDescriptionNameOtherId` | `int` | `NULL` |
| `Items_TypesFactory_NameId` | `int` | `NULL` |
| `Items_TypesSize_NameId` | `int` | `NULL` |
| `Items_TypesColor_NameId` | `int` | `NULL` |
| `Items_TypesDegreeFarz_NameId` | `int` | `NULL` |
| `LookUp_CodeTaxTypesId` | `bigint` | `NULL` |
| `LookUp_TaxableTypesId` | `bigint` | `NULL` |
| `LookUp_TaxableSubTypesId` | `bigint` | `NULL` |
| `LookUp_NonTaxableTypesId` | `bigint` | `NULL` |
| `Items_NaturesId` | `bigint` | `NULL` |
| `CountryOfOrigin` | `nvarchar(500)` | `NULL` |
| `InternationalTaxNumber` | `nvarchar(500)` | `NULL` |
| `ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Pharmacology` | `nvarchar(500)` | `NULL` |
| `ScientificName` | `nvarchar(500)` | `NULL` |
| `Mark` | `nvarchar(500)` | `NULL` |
| `Licensing` | `nvarchar(500)` | `NULL` |
| `CommisionPurchase` | `decimal(18, 2)` | `NULL` |
| `Offers` | `nvarchar(500)` | `NULL` |
| `hiddenItem` | `nvarchar(500)` | `NULL` |
| `IsPrintBarCode` | `int` | `NULL` |
| `IsStopBuying` | `int` | `NULL` |
| `StorageTempratureDegree` | `decimal(18, 2)` | `NULL` |
| `TableNo` | `nvarchar(500)` | `NULL` |
| `BarCode2` | `nvarchar(max)` | `NULL` |
| `BarCode3` | `nvarchar(max)` | `NULL` |
| `BarCode4` | `nvarchar(max)` | `NULL` |
| `BarCode5` | `nvarchar(max)` | `NULL` |
| `BlockNo` | `int` | `NULL` |
| `shelfNo` | `int` | `NULL` |
| `OrderOnshelfNo` | `int` | `NULL` |
| `LookUp_CodeTaxTypeStr` | `nvarchar(500)` | `NULL` |
| `description` | `nvarchar(500)` | `NULL` |
| `descriptionAr` | `nvarchar(500)` | `NULL` |
| `ItemCode` | `nvarchar(500)` | `NULL` |
| `linkedCode` | `nvarchar(500)` | `NULL` |
| `ParentCode` | `nvarchar(500)` | `NULL` |
| `requestReason` | `nvarchar(500)` | `NULL` |
| `Items_GroupsId` | `bigint` | `NULL` |
| `Is_ShowOnInvoice` | `int` | `NULL` |
| `Is_NotShowOnInvoice` | `int` | `NULL` |
| `ItemCodeTaxAcceptedId` | `bigint` | `NULL` |
| `Is_UploadToTaxPortal` | `int` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `IsMasna3iatKhamat` | `int` | `NULL` |
| `Is_ItemOrService` | `int` | `NULL` |
| `UnitCodeTax` | `nvarchar(500)` | `NULL` |
| `CodeNameAr` | `nvarchar(500)` | `NULL` |
| `CodeName` | `nvarchar(500)` | `NULL` |
| `FactoryCode` | `nvarchar(500)` | `NULL` |
| `InternalCode` | `bigint` | `NULL` |
| `ItemCodeType` | `nvarchar(500)` | `NULL` |
| `ItemCodeBackUP` | `nvarchar(500)` | `NULL` |
| `ItemSizeDescription` | `nvarchar(500)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `S_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `Lookup_CarTypeId` | `bigint` | `NULL` |
| `IsCommissionEmployeeBased` | `int` | `NULL` |
| `CommissionEmployeeValue` | `decimal(18, 2)` | `NULL` |
| `ItemServiceCate ryId` | `bigint` | `NULL` |
| `TotalSessions` | `int` | `NULL` |
| `ServiceDurationInMinutes` | `int` | `NULL` |
| `IsRetailItem` | `int` | `NULL` |
| `RetailPrice` | `decimal(18, 2)` | `NULL` |
| `Image` | `bit` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Item_Cate ry_ID` → محتمل: `Item_Cate ry`
- `Items_Types_Name_Id` → محتمل: `Items_Types_Name`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Items_TypesDescription_NameId` → محتمل: `Items_TypesDescriptionNameOther`, `Items_TypesDescription_Name`
- `Items_TypesDescriptionNameOtherId` → محتمل: `Items_TypesDescriptionNameOther`
- `Items_TypesFactory_NameId` → محتمل: `Items_TypesFactory_Name`
- `Items_TypesSize_NameId` → محتمل: `Items_TypesSize_Name`
- `Items_TypesColor_NameId` → محتمل: `Items_TypesColor_Name`
- `Items_TypesDegreeFarz_NameId` → محتمل: `Items_TypesDegreeFarz_Name`
- `LookUp_TaxableTypesId` → محتمل: `LookUp_TaxableTypes`
- `LookUp_TaxableSubTypesId` → محتمل: `LookUp_TaxableSubTypes`
- `LookUp_NonTaxableTypesId` → محتمل: `LookUp_NonTaxableTypes`

---
### `Item_BarCodeSetting`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 30

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_Id` | `nvarchar(50)` | `NULL` |
| `FontHeader` | `nvarchar(50)` | `NULL` |
| `FontFooter` | `nvarchar(50)` | `NULL` |
| `Medium` | `nvarchar(50)` | `NULL` |
| `Center` | `nvarchar(50)` | `NULL` |
| `Width` | `nvarchar(50)` | `NULL` |
| `Height` | `nvarchar(50)` | `NULL` |
| `BarCodeHeight` | `nvarchar(50)` | `NULL` |
| `Space_X_Item` | `nvarchar(50)` | `NULL` |
| `Space_Y_Item` | `nvarchar(50)` | `NULL` |
| `TopMargin` | `nvarchar(50)` | `NULL` |
| `LeftMargin` | `nvarchar(50)` | `NULL` |
| `paperWidth` | `nvarchar(50)` | `NULL` |
| `paperHeight` | `nvarchar(50)` | `NULL` |
| `SetPaperSize` | `nvarchar(50)` | `NULL` |
| `PaperTopMargin` | `nvarchar(50)` | `NULL` |
| `PaperLeftMargin` | `nvarchar(50)` | `NULL` |
| `PaperRightMargin` | `nvarchar(50)` | `NULL` |
| `PaperBottomMargin` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TextUnderCode` | `nvarchar(500)` | `NULL` |
| `IsPrintPrice` | `nvarchar(500)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_BarCodeSettingDefault`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 29

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_Id` | `nvarchar(50)` | `NULL` |
| `FontHeader` | `nvarchar(50)` | `NULL` |
| `FontFooter` | `nvarchar(50)` | `NULL` |
| `Medium` | `nvarchar(50)` | `NULL` |
| `Center` | `nvarchar(50)` | `NULL` |
| `Width` | `nvarchar(50)` | `NULL` |
| `Height` | `nvarchar(50)` | `NULL` |
| `BarCodeHeight` | `nvarchar(50)` | `NULL` |
| `Space_X_Item` | `nvarchar(50)` | `NULL` |
| `Space_Y_Item` | `nvarchar(50)` | `NULL` |
| `TopMargin` | `nvarchar(50)` | `NULL` |
| `LeftMargin` | `nvarchar(50)` | `NULL` |
| `paperWidth` | `nvarchar(50)` | `NULL` |
| `paperHeight` | `nvarchar(50)` | `NULL` |
| `SetPaperSize` | `nvarchar(50)` | `NULL` |
| `PaperTopMargin` | `nvarchar(50)` | `NULL` |
| `PaperLeftMargin` | `nvarchar(50)` | `NULL` |
| `PaperRightMargin` | `nvarchar(50)` | `NULL` |
| `PaperBottomMargin` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TextUnderCode` | `nvarchar(500)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_Cate ry`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Cate ry_Name` | `nvarchar(50)` | `NULL` |
| `Cate ry_Type_Str` | `nvarchar(50)` | `NULL` |
| `Cate ry_Type_Id` | `int` | `NULL` |
| `Main_Item_Cate ry_Id` | `int` | `NULL` |
| `IS_Active` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Is_ItemOrService` | `int` | `NULL` |
| `Is_Default` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_First_Period_Balance`

**الاستخدام الوظيفي:** تخزين أو معالجة أرصدة مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 20

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Id` | `bigint` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Balance_Date` | `datetime` | `NULL` |
| `Active` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `S_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `P_price` | `decimal(18, 2)` | `NULL` |
| `S_price` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Unit_Item_Id` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_Image_BarCode`

**الاستخدام الوظيفي:** تخزين أو فهرسة صور مرتبطة بالكيان المقابل.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Item_Id` | `bigint` | `NULL` |
| `ImageBarCode` | `image` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_Images`

**الاستخدام الوظيفي:** تخزين أو فهرسة صور مرتبطة بالكيان المقابل.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `Item_Id` | `int` | `NULL` |
| `Picture` | `image` | `NULL` |
| `Description` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_ImagesStore`

**الاستخدام الوظيفي:** تخزين أو فهرسة صور مرتبطة بالكيان المقابل.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ImageId` | `bigint` | `NOT NULL` |
| `Item_Id` | `int` | `NULL` |
| `Description` | `nvarchar(500)` | `NULL` |
| `OriginalPath` | `nvarchar(500)` | `NULL` |
| `ImageData` | `image` | `NULL` |
| `Default_Img_Flage` | `bit` | `NULL` |
| `Item_Type` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_Main_Cate ry`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Main_Cate ry_Name` | `nvarchar(50)` | `NULL` |
| `Cate ry_Type_Id` | `int` | `NULL` |
| `Cate ry_Type_Str` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_PriceUnit_List`

**الاستخدام الوظيفي:** أسعار أو قواعد تسعير مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 37

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Item_Name` | `nvarchar(50)` | `NULL` |
| `Item_Unit` | `nvarchar(50)` | `NULL` |
| `Hour_Value` | `decimal(18, 2)` | `NULL` |
| `Item_UnitPrice` | `decimal(18, 2)` | `NULL` |
| `ItemsName` | `nvarchar(50)` | `NULL` |
| `DrawName` | `nvarchar(50)` | `NULL` |
| `Stitch_No` | `decimal(18, 2)` | `NULL` |
| `TextileDrawingName` | `nvarchar(500)` | `NULL` |
| `Alf_Hadfa_Price` | `decimal(18, 0)` | `NULL` |
| `ItemWeightTextile` | `decimal(18, 2)` | `NULL` |
| `Hadafat_In_NazlaNo` | `decimal(18, 0)` | `NULL` |
| `Unit_Item_ID` | `bigint` | `NULL` |
| `Item_ID` | `bigint` | `NULL` |
| `Abalek_No` | `bigint` | `NULL` |
| `Ableka_Price` | `bigint` | `NULL` |
| `Hadafat_Tash3heel` | `decimal(18, 2)` | `NULL` |
| `Item_Width` | `decimal(18, 2)` | `NULL` |
| `Item_Length` | `decimal(18, 2)` | `NULL` |
| `Item_Weight_Per_Square_meter` | `decimal(18, 2)` | `NULL` |
| `Items_Bodies_Name_Id` | `int` | `NULL` |
| `Items_Bodies_Name` | `nvarchar(50)` | `NULL` |
| `Items_Types_Name_Id` | `int` | `NULL` |
| `Items_Types_Name` | `nvarchar(50)` | `NULL` |
| `SubItem_Name` | `nvarchar(100)` | `NULL` |
| `Main_Item_Cate ry_Id_Lehaf_Wabara` | `int` | `NULL` |
| `SubItem_No` | `int` | `NULL` |
| `TMP_Hadafat_Prod` | `decimal(18, 2)` | `NULL` |
| `TMP_Nazalat_Prod` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Unit_Item_ID` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`
- `Items_Bodies_Name_Id` → محتمل: `Items_Bodies_Name`
- `Items_Types_Name_Id` → محتمل: `Items_Types_Name`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_Raw_Materials`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `MainItem_Id` | `bigint` | `NULL` |
| `SubItem_Id` | `bigint` | `NULL` |
| `Full_qua` | `decimal(18, 2)` | `NULL` |
| `D_Qua` | `decimal(18, 2)` | `NULL` |
| `Quantity` | `decimal(18, 2)` | `NULL` |
| `MainItem_Name` | `nvarchar(200)` | `NULL` |
| `SubItem_Name` | `nvarchar(200)` | `NULL` |
| `Big_unit` | `nvarchar(50)` | `NULL` |
| `Small_unit` | `nvarchar(50)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `S_unit_Value` | `decimal(18, 2)` | `NULL` |
| `Default_Store_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_Raw_MaterialsFor_Entage`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 35

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `MainOrderTable_id` | `bigint` | `NULL` |
| `SubOrderTable_id` | `bigint` | `NULL` |
| `Process_Date` | `datetime` | `NULL` |
| `MainItem_Id` | `bigint` | `NULL` |
| `SubItem_Id` | `bigint` | `NULL` |
| `MainItem_Name` | `nvarchar(200)` | `NULL` |
| `SubItem_Name` | `nvarchar(200)` | `NULL` |
| `SubUnit_item_id` | `bigint` | `NULL` |
| `MainUnit_item_id` | `bigint` | `NULL` |
| `Big_unit` | `nvarchar(50)` | `NULL` |
| `Small_unit` | `nvarchar(50)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `S_unit_Value` | `decimal(18, 2)` | `NULL` |
| `Raw_Full_qua` | `decimal(18, 2)` | `NULL` |
| `Raw_D_Qua` | `decimal(18, 2)` | `NULL` |
| `Raw_Quantity` | `decimal(18, 2)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Main_Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Main_Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Main_Qnty` | `decimal(18, 2)` | `NULL` |
| `MainStore_Id` | `bigint` | `NULL` |
| `SubStore_Id` | `bigint` | `NULL` |
| `SubStore_Name` | `nvarchar(50)` | `NULL` |
| `Notes_Item` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_Raw_MaterialsFor_Entage_Details`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_Raw_MaterialsFor_Entage_List_Id` | `bigint` | `NULL` |
| `Unit_Item_Id` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Order_Purshing_Id` | `bigint` | `NULL` |
| `Detailes_Purshing_Id` | `bigint` | `NULL` |
| `Order_Estlam_Sarf_Flage` | `int` | `NULL` |
| `Notes_Item` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Item_Raw_MaterialsFor_Entage_List_Id` → محتمل: `Item_Raw_MaterialsFor_Entage_List`
- `Unit_Item_Id` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`
- `Order_Purshing_Id` → محتمل: `Order_Purshing`
- `Detailes_Purshing_Id` → محتمل: `Detailes_Purshing_Import_Export`, `Detailes_Purshing`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Item_Raw_MaterialsFor_Entage_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Process_No` | `bigint` | `NULL` |
| `Notes_List` | `nvarchar(500)` | `NULL` |
| `Process_Date` | `datetime` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `ItemCodeTaxAccepted`

**الاستخدام الوظيفي:** بيانات/إعدادات/حركات ضريبية مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 38

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Tax_BranchesId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Barcode` | `nvarchar(max)` | `NULL` |
| `ParentCode` | `nvarchar(100)` | `NULL` |
| `CurrencyId` | `bigint` | `NULL` |
| `internalCode` | `nvarchar(max)` | `NULL` |
| `CodeName` | `nvarchar(max)` | `NULL` |
| `CodeNameAr` | `nvarchar(max)` | `NULL` |
| `DescriptionEn` | `nvarchar(max)` | `NULL` |
| `DescriptionAr` | `nvarchar(max)` | `NULL` |
| `ItemCode` | `nvarchar(max)` | `NULL` |
| `ItemCodeType` | `nvarchar(max)` | `NULL` |
| `Price` | `decimal(18, 2)` | `NULL` |
| `UnitCode` | `nvarchar(max)` | `NULL` |
| `UnitDescEn` | `nvarchar(max)` | `NULL` |
| `UnitDescAr` | `nvarchar(max)` | `NULL` |
| `activeFrom` | `datetime` | `NULL` |
| `activeTo` | `datetime` | `NULL` |
| `requestReason` | `nvarchar(100)` | `NULL` |
| `linkedCode` | `nvarchar(100)` | `NULL` |
| `IsRelatedToLocalItem` | `bit` | `NULL` |
| `ParentCodeLookupValue` | `nvarchar(500)` | `NULL` |
| `codeTypeID` | `nvarchar(500)` | `NULL` |
| `codeTypeLevelID` | `nvarchar(500)` | `NULL` |
| `codeTypeLevelNamePrimaryLang` | `nvarchar(500)` | `NULL` |
| `codeTypeLevelNameSecondaryLang` | `nvarchar(500)` | `NULL` |
| `parentCodeNamePrimaryLang` | `nvarchar(500)` | `NULL` |
| `parentCodeNameSecondaryLang` | `nvarchar(500)` | `NULL` |
| `parentLevelName` | `nvarchar(500)` | `NULL` |
| `codeTypeNameSecondaryLang` | `nvarchar(500)` | `NULL` |
| `active` | `nvarchar(500)` | `NULL` |
| `codeCate rization` | `nvarchar(max)` | `NULL` |
| `pageNo` | `int` | `NULL` |
| `ps_in_pageNo` | `int` | `NULL` |
| `Is_Repeated` | `int` | `NULL` |
| `Is_SendToERP` | `int` | `NULL` |
| `Is_RepeatedExcluded` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Tax_BranchesId` → محتمل: `Tax_Branches`, `Tax_BranchesConfig`
- `CurrencyId` → محتمل: `Currency`, `Currency_Factor`

---
### `ItemCodeTaxNotUploaded`

**الاستخدام الوظيفي:** بيانات/إعدادات/حركات ضريبية مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `internalCode` | `nvarchar(100)` | `NULL` |
| `Tax_BranchesId` | `bigint` | `NULL` |
| `CodeType` | `nvarchar(100)` | `NULL` |
| `ParentCode` | `nvarchar(100)` | `NULL` |
| `ItemCode` | `nvarchar(100)` | `NULL` |
| `CodeName` | `nvarchar(100)` | `NULL` |
| `CodeNameAr` | `nvarchar(100)` | `NULL` |
| `activeFrom` | `datetime` | `NULL` |
| `activeTo` | `datetime` | `NULL` |
| `description` | `nvarchar(100)` | `NULL` |
| `descriptionAr` | `nvarchar(100)` | `NULL` |
| `requestReason` | `nvarchar(100)` | `NULL` |
| `linkedCode` | `nvarchar(100)` | `NULL` |
| `IsUploaded` | `bit` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Tax_BranchesId` → محتمل: `Tax_Branches`, `Tax_BranchesConfig`

---
### `ItemCost`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 9

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Bills_DetailsId` | `bigint` | `NOT NULL` |
| `Invoice_Date` | `datetime` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `totalFull` | `decimal(18, 2)` | `NULL` |
| `AVGPrice` | `decimal(18, 2)` | `NULL` |
| `ROW NUMBER` | `int` | `NULL` |
| `FirstValues` | `decimal(18, 2)` | `NULL` |
| `FirstValuesAdded` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bills_DetailsId` → محتمل: `Bills_Details`, `Bills_Details_Reverse`

---
### `ItemCostAVG`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `AVGPrice` | `decimal(18, 2)` | `NULL` |

---
### `ItemRawEntagEstlamTemp`

**الاستخدام الوظيفي:** جدول مؤقت لمعالجة وتجميع بيانات أثناء تنفيذ عمليات النظام أو إعداد التقارير.

**عدد الأعمدة:** 36

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Item_Name` | `nvarchar(200)` | `NULL` |
| `S_Id` | `decimal(18, 2)` | `NULL` |
| `Id` | `decimal(18, 2)` | `NOT NULL` |
| `MainItem_Id` | `decimal(18, 2)` | `NULL` |
| `SubItem_Id` | `decimal(18, 2)` | `NULL` |
| `Full_qua` | `decimal(18, 2)` | `NULL` |
| `D_Qua` | `decimal(18, 2)` | `NULL` |
| `Quantity` | `decimal(18, 2)` | `NULL` |
| `MainItem_Name` | `nvarchar(200)` | `NULL` |
| `SubItem_Name` | `nvarchar(200)` | `NULL` |
| `Big_unit` | `nvarchar(200)` | `NULL` |
| `Small_unit` | `nvarchar(200)` | `NULL` |
| `Unit_Id` | `decimal(18, 2)` | `NULL` |
| `S_unit_Value` | `decimal(18, 2)` | `NULL` |
| `Default_Store_Id` | `decimal(18, 2)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Entry_Larg_Qnty` | `nvarchar(200)` | `NULL` |
| `Entry_Small_Qnty` | `nvarchar(200)` | `NULL` |
| `Entry_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_Index` | `nvarchar(200)` | `NULL` |
| `Store_Name` | `nvarchar(200)` | `NULL` |
| `SubFull_qua` | `decimal(18, 2)` | `NULL` |
| `SubD_Qua` | `decimal(18, 2)` | `NULL` |
| `SubItem_No` | `decimal(18, 2)` | `NULL` |
| `SubQntyBalance` | `decimal(18, 2)` | `NULL` |
| `CheckBalanceAllows` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_Bodies_Name`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ItemBody_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_Color_Name`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Color_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_Group`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---
### `Items_Nature`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ItemsNature_Name` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `IsActive` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_Types_Name`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ItemsType_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `IsActive` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_TypesColor_Name`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Type_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_TypesDegreeFarz_Name`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Type_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_TypesDescription_Name`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Type_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_TypesDescriptionNameOther`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Type_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_TypesFactory_Name`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Type_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Items_TypesSize_Name`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Type_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Maintaining_Item_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Maintaining_Item_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Textile_Item_Dept_Balance`

**الاستخدام الوظيفي:** تخزين أو معالجة أرصدة مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `Item_Id` | `int` | `NULL` |
| `Dept_Id` | `int` | `NULL` |
| `Dept_Name` | `nvarchar(50)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Weight` | `decimal(18, 2)` | `NULL` |
| `Item_Type` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Dept_Id` → محتمل: `Textile_Item_Dept_Balance`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `unit`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NULL` |
| `u_name` | `nvarchar(30)` | `NOT NULL` |
| `B_Unit` | `nvarchar(30)` | `NOT NULL` |
| `S_unit` | `nvarchar(30)` | `NOT NULL` |
| `B_Unit_Value` | `int` | `NULL` |
| `S_unit_Value` | `decimal(18, 2)` | `NULL` |
| `SS_unit_Value` | `int` | `NULL` |
| `IS_Default` | `int` | `NULL` |
| `Code` | `nvarchar(500)` | `NULL` |
| `Desc_ar` | `nvarchar(500)` | `NULL` |
| `Desc_en` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Unit_item`

**الاستخدام الوظيفي:** تعريف وحدات قياس الأصناف وربطها بالأصناف.

**عدد الأعمدة:** 42

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NULL` |
| `Item_id` | `bigint` | `NULL` |
| `Store_id` | `bigint` | `NULL` |
| `Store_Type` | `int` | `NULL` |
| `Code` | `nvarchar(max)` | `NULL` |
| `unit_id` | `bigint` | `NULL` |
| `Full_qua` | `decimal(18, 2)` | `NULL` |
| `D_Qua` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Reserved_Balance_Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Reserved_Balance_Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Big_unit` | `int` | `NULL` |
| `Small_unit` | `decimal(18, 2)` | `NULL` |
| `P_price` | `decimal(18, 2)` | `NULL` |
| `S_price` | `decimal(18, 2)` | `NULL` |
| `Specification` | `nvarchar(100)` | `NULL` |
| `Upon_Request` | `decimal(18, 2)` | `NULL` |
| `Currency` | `nvarchar(50)` | `NULL` |
| `active_flage` | `int` | `NULL` |
| `TaxSales_Value` | `decimal(18, 2)` | `NULL` |
| `New2` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `P_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `P_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_For_S_price_Default` | `int` | `NULL` |
| `Unit_Used_For_P_price_Default` | `int` | `NULL` |
| `S_price_Item_No` | `decimal(18, 2)` | `NULL` |
| `P_price_Item_No` | `decimal(18, 2)` | `NULL` |
| `Is_ItemWithoutBalance` | `int` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `S_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `ProductCost_Price` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Unit_Item_Balance_Date_Details`

**الاستخدام الوظيفي:** أرصدة الأصناف حسب التاريخ والوحدة.

**عدد الأعمدة:** 20

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Unit_Item_Balance_Date_List_Id` | `bigint` | `NULL` |
| `Balance_Date` | `datetime` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Id` | `bigint` | `NULL` |
| `Large_Balance` | `decimal(18, 2)` | `NULL` |
| `Small_Balance` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `S_price` | `decimal(18, 2)` | `NULL` |
| `P_price` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Unit_Item_Id` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Unit_Item_ExpireDate`

**الاستخدام الوظيفي:** تواريخ صلاحية/انتهاء وحدات أو دفعات الأصناف.

**عدد الأعمدة:** 23

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Tash3ela` | `nvarchar(50)` | `NULL` |
| `Item_Expire_Date` | `datetime` | `NULL` |
| `Larg_Balance` | `decimal(18, 2)` | `NULL` |
| `Small_Balance` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Bills_Details_Id` | `bigint` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Unit_Item_BatchGroupListId` | `bigint` | `NULL` |
| `BatchNo` | `bigint` | `NULL` |
| `QntyValue` | `decimal(18, 2)` | `NULL` |
| `IsActive` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Bills_Details_Id` → محتمل: `Bills_Details`, `Bills_Details_Reverse`

---

## 06 — المخازن والمشتريات والاستلام والصرف
### `Detailes_Purshing`

**الاستخدام الوظيفي:** تفاصيل عمليات الشراء.

**عدد الأعمدة:** 102

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `OrderTable_id` | `bigint` | `NULL` |
| `Unit_item_Table_Id` | `bigint` | `NULL` |
| `Code` | `nvarchar(500)` | `NULL` |
| `qua` | `decimal(18, 2)` | `NULL` |
| `Part_qua` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Balance_Id_From_Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Balance_Id_From_Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Balance_Id_From_Item_No` | `decimal(18, 2)` | `NULL` |
| `Balance_Id_To_Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Balance_Id_To_Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Balance_Id_To_Item_No` | `decimal(18, 2)` | `NULL` |
| `To_Store_Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Notes_From` | `nvarchar(50)` | `NULL` |
| `Notes_To` | `nvarchar(50)` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Machine_Id` | `int` | `NULL` |
| `Related_Detailes_Purshing_Id` | `bigint` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Qua_Befor` | `decimal(18, 2)` | `NULL` |
| `Part_qua_Befor` | `decimal(18, 2)` | `NULL` |
| `Item_No_Befor` | `decimal(18, 2)` | `NULL` |
| `Qua_After` | `decimal(18, 2)` | `NULL` |
| `Part_qua_After` | `decimal(18, 2)` | `NULL` |
| `Item_No_After` | `decimal(18, 2)` | `NULL` |
| `Notes_Item` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `QntyStr` | `nvarchar(500)` | `NULL` |
| `Item_NoStr` | `nvarchar(500)` | `NULL` |
| `QntyStr_Befor` | `nvarchar(500)` | `NULL` |
| `Item_NoStr_Befor` | `nvarchar(500)` | `NULL` |
| `QntyStr_After` | `nvarchar(500)` | `NULL` |
| `Item_NoStr_After` | `nvarchar(500)` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `WorkOrderNumber` | `nvarchar(500)` | `NULL` |
| `Invoice_No` | `nvarchar(500)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `Item_Total_WithoutTax` | `decimal(18, 2)` | `NULL` |
| `Item_ATS05_Value` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `Net_Total_Item_Commission` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `type_From` | `int` | `NULL` |
| `Type_To` | `int` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Item_DeletedFlage` | `int` | `NULL` |
| `Item_ATS05` | `decimal(18, 2)` | `NULL` |
| `Item_TaxSales10` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Unit_CommissionValue` | `decimal(18, 2)` | `NULL` |
| `Item_CommissionRate` | `decimal(18, 2)` | `NULL` |
| `ItemCommissionTotal` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_for_Price` | `int` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Small_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Import_Qua` | `decimal(18, 2)` | `NULL` |
| `Import_Part_Qua` | `decimal(18, 2)` | `NULL` |
| `Import_Item_No` | `decimal(18, 2)` | `NULL` |
| `Export_Qua` | `decimal(18, 2)` | `NULL` |
| `Export_Part_Qua` | `decimal(18, 2)` | `NULL` |
| `Export_Item_No` | `decimal(18, 2)` | `NULL` |
| `StoreId_Name` | `nvarchar(500)` | `NULL` |
| `StoreId` | `bigint` | `NULL` |
| `Id_From` | `bigint` | `NULL` |
| `Id_To` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `CarsWorkOrdersRequestedDetailsId` | `bigint` | `NULL` |
| `CarsWorkOrdersDeliverDetailsId` | `bigint` | `NULL` |
| `Type_From_Name` | `nvarchar(500)` | `NULL` |
| `Type_To_Name` | `nvarchar(500)` | `NULL` |
| `Qnty` | `decimal(18, 2)` | `NULL` |
| `Unit_Id` | `bigint` | `NULL` |
| `UnitStr` | `nvarchar(500)` | `NULL` |
| `CarsWorkOrdersItemProcessedDetailsId` | `bigint` | `NULL` |
| `Item_Cate ry_Id` | `bigint` | `NULL` |
| `Item_Cate ry_Name` | `nvarchar(500)` | `NULL` |
| `S_Price` | `decimal(18, 2)` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `Item_ProcessedFlage` | `int` | `NULL` |
| `ProcessNameNumber` | `nvarchar(500)` | `NULL` |
| `ProcessName` | `nvarchar(500)` | `NULL` |
| `Id_FromStr` | `nvarchar(500)` | `NULL` |
| `Id_ToStr` | `nvarchar(500)` | `NULL` |
| `ProcessNameId` | `int` | `NULL` |
| `EmployeeId` | `bigint` | `NULL` |
| `EmployeeName` | `nvarchar(500)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `S_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `FromStoreId` | `bigint` | `NULL` |
| `ToStoreId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Machine_Id` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `CarsWorkOrdersRequestedDetailsId` → محتمل: `CarsWorkOrdersRequestedDetails`
- `CarsWorkOrdersDeliverDetailsId` → محتمل: `CarsWorkOrdersDeliverDetails`
- `CarsWorkOrdersItemProcessedDetailsId` → محتمل: `CarsWorkOrdersItemProcessedDetails`
- `Item_Cate ry_Id` → محتمل: `Item_Cate ry`

---
### `Detailes_Purshing_Import_Export`

**الاستخدام الوظيفي:** تفاصيل عمليات الشراء المرتبطة بالاستيراد/التصدير.

**عدد الأعمدة:** 75

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Detailes_Purshing_Id` | `bigint` | `NULL` |
| `OrderTable_id` | `bigint` | `NULL` |
| `Unit_item_Table_Id` | `bigint` | `NULL` |
| `qua` | `decimal(18, 2)` | `NULL` |
| `Part_qua` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Balance_Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Balance_Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Balance_Item_No` | `decimal(18, 2)` | `NULL` |
| `Notes_From_To` | `nvarchar(500)` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Machine_Id` | `bigint` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Process_Date` | `datetime` | `NULL` |
| `Notes_To` | `nvarchar(100)` | `NULL` |
| `Notes_From` | `nvarchar(100)` | `NULL` |
| `IS_To_Converted` | `int` | `NULL` |
| `qua_Import` | `decimal(18, 2)` | `NULL` |
| `Part_Qua_Import` | `decimal(18, 2)` | `NULL` |
| `Item_No_Import` | `decimal(18, 2)` | `NULL` |
| `Quantity_Import` | `decimal(18, 2)` | `NULL` |
| `qua_Export` | `decimal(18, 2)` | `NULL` |
| `Part_Qua_Export` | `decimal(18, 2)` | `NULL` |
| `Item_No_Export` | `decimal(18, 2)` | `NULL` |
| `Quantity_Export` | `decimal(18, 2)` | `NULL` |
| `Quantity` | `decimal(18, 2)` | `NULL` |
| `Quantity_Balance` | `decimal(18, 2)` | `NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Invoice_Type_Id` | `bigint` | `NULL` |
| `Detail_Invoice_Id` | `bigint` | `NULL` |
| `Notes_Item` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Total_Value` | `decimal(18, 2)` | `NULL` |
| `Invoice_No` | `bigint` | `NULL` |
| `Invoice_Type_Str` | `nvarchar(500)` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `CarsWorkOrdersDetailsId` | `bigint` | `NULL` |
| `CarsWorkOrdersItemProcessedDetailsId` | `bigint` | `NULL` |
| `WorkOrderNumber` | `nvarchar(500)` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `ProcessName` | `nvarchar(500)` | `NULL` |
| `Id_FromStr` | `nvarchar(500)` | `NULL` |
| `Id_ToStr` | `nvarchar(500)` | `NULL` |
| `ProcessNameId` | `int` | `NULL` |
| `Type_From` | `int` | `NULL` |
| `Id_From` | `bigint` | `NULL` |
| `Type_To` | `int` | `NULL` |
| `Id_To` | `bigint` | `NULL` |
| `StoreId` | `bigint` | `NULL` |
| `StoreId_Name` | `nvarchar(500)` | `NULL` |
| `Type_From_Name` | `nvarchar(500)` | `NULL` |
| `Type_To_Name` | `nvarchar(500)` | `NULL` |
| `ProcessNameNumber` | `nvarchar(500)` | `NULL` |
| `EmployeeId` | `bigint` | `NULL` |
| `EmployeeName` | `nvarchar(500)` | `NULL` |
| `S_Price` | `decimal(18, 2)` | `NULL` |
| `P_Price` | `decimal(18, 2)` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Qnty_value` | `decimal(18, 2)` | `NULL` |
| `P_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `S_priceAvgCost` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_Price` | `decimal(18, 2)` | `NULL` |
| `PriceUnitImport` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_PriceImport` | `decimal(18, 2)` | `NULL` |
| `PriceUnitExport` | `decimal(18, 2)` | `NULL` |
| `Item_Net_Total_PriceExport` | `decimal(18, 2)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Detailes_Purshing_Id` → محتمل: `Detailes_Purshing_Import_Export`, `Detailes_Purshing`
- `Machine_Id` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `CarsWorkOrdersItemProcessedDetailsId` → محتمل: `CarsWorkOrdersItemProcessedDetails`

---
### `Detailes_Receiving`

**الاستخدام الوظيفي:** تفاصيل عمليات الاستلام.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `R_id` | `bigint` | `NULL` |
| `Unit_item_Id` | `bigint` | `NULL` |
| `unit_id` | `int` | `NOT NULL` |
| `Item_id` | `int` | `NOT NULL` |
| `qua` | `bigint` | `NULL` |
| `Part_qua` | `int` | `NOT NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Unit_item_Table_id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Unit_item_Id` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Order_Money`

**الاستخدام الوظيفي:** أوامر تشغيلية للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 45

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Order_No` | `bigint` | `NULL` |
| `Order_Date` | `datetime` | `NULL` |
| `Transaction_type` | `int` | `NULL` |
| `Total_Value` | `decimal(18, 3)` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Bank_Type` | `int` | `NULL` |
| `Bank_ID` | `bigint` | `NULL` |
| `Man_Name` | `nvarchar(max)` | `NULL` |
| `Is_Balances_Set` | `int` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `Order_mony_id_transform` | `int` | `NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `Year_Financial_Period_Id` | `bigint` | `NULL` |
| `Ticket_Supplier_Id` | `bigint` | `NULL` |
| `Order_Tafkeet_Value` | `nvarchar(max)` | `NULL` |
| `Order_Notes` | `nvarchar(max)` | `NULL` |
| `Tafkeet_Value` | `nvarchar(max)` | `NULL` |
| `Sandouk_Cost_Center_Tree_Id` | `bigint` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Entry_Currency_Name` | `nvarchar(max)` | `NULL` |
| `Factor` | `decimal(18, 3)` | `NULL` |
| `Entry_Currency_ID` | `int` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `Invoice_Type` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `MainOrder_MoneyId` | `bigint` | `NULL` |
| `Reverse_Order` | `int` | `NULL` |
| `Account_Type` | `int` | `NULL` |
| `Account_Id` | `bigint` | `NULL` |
| `IsMoreTheOneBoxShow` | `int` | `NULL` |
| `RequirementValue` | `decimal(18, 2)` | `NULL` |
| `StillValue` | `decimal(18, 2)` | `NULL` |
| `paidTotal` | `decimal(18, 2)` | `NULL` |
| `DataDictionaryProcessesListsId` | `bigint` | `NULL` |
| `Reference_Type_Table_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `Bank_ID` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `DataDictionaryProcessesListsId` → محتمل: `DataDictionaryProcessesLists`

---
### `Order_Money_Check`

**الاستخدام الوظيفي:** أوامر تشغيلية للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 35

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Order_No` | `bigint` | `NULL` |
| `Order_Date` | `datetime` | `NULL` |
| `Transaction_type` | `int` | `NULL` |
| `Total_Value` | `decimal(18, 2)` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Bank_Type` | `int` | `NULL` |
| `Bank_ID` | `int` | `NULL` |
| `Man_Name` | `nvarchar(100)` | `NULL` |
| `Is_Balances_Set` | `int` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `Order_mony_id_transform` | `int` | `NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Year_Financial_Period_Id` | `int` | `NULL` |
| `Ticket_Supplier_Id` | `int` | `NULL` |
| `Order_Tafkeet_Value` | `nvarchar(100)` | `NULL` |
| `Order_Notes` | `nvarchar(500)` | `NULL` |
| `Tafkeet_Value` | `nvarchar(100)` | `NULL` |
| `Sandouk_Cost_Center_Tree_Id` | `int` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Notes` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Entry_Currency_Name` | `nvarchar(100)` | `NULL` |
| `Factor` | `decimal(18, 2)` | `NULL` |
| `Entry_Currency_ID` | `int` | `NULL` |
| `Invoice_Type` | `int` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `Bank_ID` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`

---
### `Order_Purshing`

**الاستخدام الوظيفي:** أوامر الشراء.

**عدد الأعمدة:** 37

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Order_Purshing_No` | `bigint` | `NULL` |
| `Order_date` | `datetime` | `NULL` |
| `Order_Type` | `int` | `NULL` |
| `type_From` | `int` | `NULL` |
| `Id_From` | `bigint` | `NULL` |
| `Type_To` | `int` | `NULL` |
| `Id_To` | `bigint` | `NULL` |
| `Invoice_ID` | `bigint` | `NULL` |
| `Cust_Supplier_id` | `bigint` | `NULL` |
| `endd` | `int` | `NULL` |
| `end_order` | `int` | `NULL` |
| `Branch_ID` | `int` | `NULL` |
| `User_ID` | `int` | `NULL` |
| `Is_Converted` | `int` | `NULL` |
| `MainOrderTable_id` | `bigint` | `NULL` |
| `Id_FromStr` | `nvarchar(500)` | `NULL` |
| `Id_ToStr` | `nvarchar(500)` | `NULL` |
| `NotesAll` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `OrderTitle` | `nvarchar(500)` | `NULL` |
| `IS_EstlamRawMaterial` | `bit` | `NULL` |
| `WorkOrderNumber` | `nvarchar(500)` | `NULL` |
| `Invoice_No` | `nvarchar(500)` | `NULL` |
| `IsMoreTheOneOrderShow` | `int` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `EmployeeId` | `bigint` | `NULL` |
| `EmployeeName` | `nvarchar(500)` | `NULL` |
| `IsActiveMainOrder_Purshing` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_ID` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Order_Receiving`

**الاستخدام الوظيفي:** أوامر الاستلام.

**عدد الأعمدة:** 20

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `R_date` | `datetime` | `NOT NULL` |
| `type` | `nvarchar(1)` | `NULL` |
| `P_invoice` | `nvarchar(10)` | `NULL` |
| `Deptno` | `int` | `NULL` |
| `R_invoice` | `nvarchar(10)` | `NULL` |
| `Order_Receiveing_No` | `nvarchar(50)` | `NULL` |
| `Type_From` | `int` | `NULL` |
| `Id_From` | `int` | `NULL` |
| `Type_to` | `int` | `NULL` |
| `Id_to` | `int` | `NULL` |
| `Order_Type` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Order_Working_Textile`

**الاستخدام الوظيفي:** أوامر تشغيلية للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Order_Date` | `datetime` | `NULL` |
| `Machine_Id` | `int` | `NULL` |
| `Item_Id_Old` | `int` | `NULL` |
| `Item_Id_New` | `int` | `NULL` |
| `Is_Operat_Flage` | `int` | `NULL` |
| `Total_Nazalat` | `int` | `NULL` |
| `Total_Weight` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Machine_Id` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Order_Working_Textile_Details`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Order_Working_Textile_Id` | `bigint` | `NULL` |
| `Nazalat_Count` | `int` | `NULL` |
| `Color_Name` | `nvarchar(50)` | `NULL` |
| `Items_Color_Name_Id` | `int` | `NULL` |
| `Weight` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Order_Working_Textile_Id` → محتمل: `Order_Working_Textile_Details`, `Order_Working_Textile`
- `Items_Color_Name_Id` → محتمل: `Items_Color_Name`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Store`

**الاستخدام الوظيفي:** دليل المخازن والفروع/المواقع المخزنية.

**عدد الأعمدة:** 25

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Store_Name` | `nvarchar(50)` | `NULL` |
| `Store_location` | `nvarchar(50)` | `NULL` |
| `Emp_id` | `bigint` | `NULL` |
| `Job_id` | `bigint` | `NULL` |
| `StoreUsed_Type` | `int` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Employee_Name` | `nvarchar(500)` | `NULL` |
| `Store_Code` | `nvarchar(50)` | `NULL` |
| `IS_Default_Store` | `int` | `NULL` |
| `IS_Sell_SecondUnit` | `int` | `NULL` |
| `IS_ItemSecond_degree` | `int` | `NULL` |
| `Active` | `int` | `NULL` |
| `IsCar` | `int` | `NULL` |
| `IS_Virtual_OuterStore` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `bigint` | `NULL` |
| `IS_ServicesVirtual_Item_NoBalance` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `LookUp_StoreOrderTypesId` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Job_id` → محتمل: `jobslist`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `LookUp_StoreOrderTypesId` → محتمل: `LookUp_StoreOrderTypes`

---
### `Work_In_Details`

**الاستخدام الوظيفي:** تفاصيل إدخالات/حركات العمل أو الإنتاج.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Customer_Id` | `int` | `NULL` |
| `Request_Date` | `datetime` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Request_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Request_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Request_Item_NO` | `decimal(18, 2)` | `NULL` |
| `Export_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Export_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Export_Item_No` | `decimal(18, 2)` | `NULL` |
| `Item_Exported_Finish` | `int` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Item_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Work_In_Details_Balance`

**الاستخدام الوظيفي:** أرصدة مرتبطة بإدخالات العمل.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Balance_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Balance_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Balance_Item_NO` | `decimal(18, 2)` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Item_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Work_In_Details_Received`

**الاستخدام الوظيفي:** تفاصيل ما تم استلامه من عمليات الإدخال/الإنتاج.

**عدد الأعمدة:** 25

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `WorkDate` | `datetime` | `NULL` |
| `Work_In_Details_Id` | `bigint` | `NULL` |
| `Process_No` | `bigint` | `NULL` |
| `Draw_Id` | `bigint` | `NULL` |
| `Draw_Name` | `nvarchar(50)` | `NULL` |
| `Received_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Received_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Received_Item_NO` | `decimal(18, 2)` | `NULL` |
| `Price` | `decimal(18, 2)` | `NULL` |
| `Total_Price` | `decimal(18, 2)` | `NULL` |
| `Price_Selected_Index` | `int` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `CustomerAccountBefor` | `decimal(18, 2)` | `NULL` |
| `CustomerAccountAfter` | `decimal(18, 2)` | `NULL` |
| `Item_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Work_In_Details_Id` → محتمل: `Work_In_Details_Received`, `Work_In_Details_Balance`, `Work_In_Details`
- `Draw_Id` → محتمل: `Draws_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Work_Out_Details`

**الاستخدام الوظيفي:** تفاصيل إخراجات/حركات العمل أو الإنتاج.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Supplier_Id` | `int` | `NULL` |
| `Request_Date` | `datetime` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Request_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Request_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Request_Item_NO` | `decimal(18, 2)` | `NULL` |
| `Export_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Export_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Export_Item_No` | `decimal(18, 2)` | `NULL` |
| `Item_Exported_Finish` | `int` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Item_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Work_Out_Details_Balance`

**الاستخدام الوظيفي:** أرصدة مرتبطة بإخراجات العمل.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Balance_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Balance_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Balance_Item_NO` | `decimal(18, 2)` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Item_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Work_Out_Details_Received`

**الاستخدام الوظيفي:** تفاصيل ما تم استلامه من عمليات الإخراج.

**عدد الأعمدة:** 25

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `WorkDate` | `datetime` | `NULL` |
| `Work_Out_Details_Id` | `bigint` | `NULL` |
| `Process_No` | `bigint` | `NULL` |
| `Draw_Id` | `bigint` | `NULL` |
| `Draw_Name` | `nvarchar(50)` | `NULL` |
| `Received_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Received_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Received_Item_NO` | `decimal(18, 2)` | `NULL` |
| `Price` | `decimal(18, 2)` | `NULL` |
| `Total_Price` | `decimal(18, 2)` | `NULL` |
| `Price_Selected_Index` | `int` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `CustomerAccountBefor` | `decimal(18, 2)` | `NULL` |
| `CustomerAccountAfter` | `decimal(18, 2)` | `NULL` |
| `Item_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Work_Out_Details_Id` → محتمل: `Work_Out_Details_Received`, `Work_Out_Details`, `Work_Out_Details_Balance`
- `Draw_Id` → محتمل: `Draws_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---

## 07 — الموظفون والموارد البشرية والرواتب
### `Department`

**الاستخدام الوظيفي:** دليل الأقسام/الإدارات.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `dname` | `nvarchar(50)` | `NOT NULL` |
| `location` | `nvarchar(50)` | `NULL` |
| `Dept_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Dept_Id` → محتمل: `Textile_Item_Dept_Balance`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Employee`

**الاستخدام الوظيفي:** دليل الموظفين والعاملين بالنظام.

**عدد الأعمدة:** 88

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `F2` | `nvarchar(500)` | `NULL` |
| `F3` | `nvarchar(500)` | `NULL` |
| `name` | `nvarchar(500)` | `NULL` |
| `jobslistId` | `bigint` | `NULL` |
| `birth_date` | `datetime` | `NULL` |
| `address` | `nvarchar(50)` | `NULL` |
| `Department` | `bigint` | `NULL` |
| `National_Card_Id` | `nvarchar(50)` | `NULL` |
| `Marriage_Status` | `nvarchar(50)` | `NULL` |
| `Child_NO` | `bigint` | `NULL` |
| `Insurance_NO` | `nvarchar(50)` | `NULL` |
| `Joining_job_Date` | `smalldatetime` | `NULL` |
| `Actual_Salary` | `decimal(18, 2)` | `NULL` |
| `Salary_Type` | `bigint` | `NULL` |
| `Salary_Type_Houre` | `int` | `NULL` |
| `Contract_NO` | `nvarchar(50)` | `NULL` |
| `Work_Node_NO` | `nvarchar(50)` | `NULL` |
| `Work_Node_Date` | `smalldatetime` | `NULL` |
| `Medical_Checkup_Date` | `smalldatetime` | `NULL` |
| `Medicinal_Card_NO` | `nvarchar(50)` | `NULL` |
| `Criminal_Status_Card_NO` | `nvarchar(50)` | `NULL` |
| `Duty_Certificate_NO` | `nvarchar(50)` | `NULL` |
| `Loan_Balance` | `decimal(18, 2)` | `NULL` |
| `Installment_Value` | `decimal(18, 2)` | `NULL` |
| `License_Days_Balance` | `bigint` | `NULL` |
| `License_Total_Days` | `bigint` | `NULL` |
| `Changeable` | `bigint` | `NULL` |
| `Alternate_Cost_Of_Living` | `bigint` | `NULL` |
| `Surtax_Salary` | `decimal(18, 2)` | `NULL` |
| `Friends_Box` | `decimal(18, 2)` | `NULL` |
| `Stamp` | `decimal(18, 2)` | `NULL` |
| `Hour_Value` | `decimal(18, 2)` | `NULL` |
| `Insurance_Salary` | `decimal(18, 2)` | `NULL` |
| `Insurance_employee_Rate` | `decimal(18, 2)` | `NULL` |
| `Insurance_Institute_Rate` | `decimal(18, 2)` | `NULL` |
| `Military_Status` | `nvarchar(50)` | `NULL` |
| `Education` | `nvarchar(50)` | `NULL` |
| `Graduation_year` | `nvarchar(50)` | `NULL` |
| `Grade` | `nvarchar(50)` | `NULL` |
| `Education_Class` | `char(10)` | `NULL` |
| `Education_Specification` | `nvarchar(50)` | `NULL` |
| `restraint_CertificateNo` | `nvarchar(50)` | `NULL` |
| `restrain_Issued_From` | `nvarchar(50)` | `NULL` |
| `restrain_Issued_Date` | `smalldatetime` | `NULL` |
| `religion` | `nvarchar(50)` | `NULL` |
| `nationality` | `nvarchar(50)` | `NULL` |
| `Gender` | `nvarchar(50)` | `NULL` |
| `MatWabara` | `decimal(18, 2)` | `NULL` |
| `MatLehaf` | `decimal(18, 2)` | `NULL` |
| `MatArdia` | `decimal(18, 2)` | `NULL` |
| `Emp_Day_Value` | `decimal(18, 2)` | `NULL` |
| `active` | `int` | `NOT NULL` |
| `badl_Eat` | `decimal(18, 2)` | `NULL` |
| `badl_House` | `decimal(18, 2)` | `NULL` |
| `badl_Transport` | `decimal(18, 2)` | `NULL` |
| `First_PeriodHour_Value` | `decimal(18, 2)` | `NULL` |
| `Second_PeriodHour_Value` | `decimal(18, 2)` | `NULL` |
| `First_PeriodHourFrom` | `decimal(18, 2)` | `NULL` |
| `First_PeriodMinuteFrom` | `decimal(18, 2)` | `NULL` |
| `First_PeriodHourTo` | `decimal(18, 2)` | `NULL` |
| `First_PeriodMinuteTo` | `decimal(18, 2)` | `NULL` |
| `Second_PeriodHourFrom` | `decimal(18, 2)` | `NULL` |
| `Second_PeriodMinuteFrom` | `decimal(18, 2)` | `NULL` |
| `Second_PeriodHourTo` | `decimal(18, 2)` | `NULL` |
| `Second_PeriodMinuteTo` | `decimal(18, 2)` | `NULL` |
| `equivalent_presence` | `decimal(18, 2)` | `NULL` |
| `SubDeptID` | `bigint` | `NULL` |
| `Stick_No_price` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Account_Code` | `nvarchar(50)` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `Color_Name` | `nvarchar(100)` | `NULL` |
| `code` | `nvarchar(500)` | `NULL` |
| `Tel` | `nvarchar(500)` | `NULL` |
| `Tel2` | `nvarchar(500)` | `NULL` |
| `Mobile` | `nvarchar(500)` | `NULL` |
| `E_Mail` | `nvarchar(500)` | `NULL` |
| `Fax` | `nvarchar(500)` | `NULL` |
| `Web_Site` | `nvarchar(500)` | `NULL` |
| `job_list_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `jobslistId` → محتمل: `jobslist`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Employee_Production_Rate`

**الاستخدام الوظيفي:** معدلات إنتاج/أداء الموظفين.

**عدد الأعمدة:** 29

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Prod_Date` | `smalldatetime` | `NULL` |
| `Item_ID` | `bigint` | `NULL` |
| `Item_Name` | `nvarchar(100)` | `NULL` |
| `Item_Qnty` | `decimal(18, 2)` | `NULL` |
| `Time_Hour_From` | `int` | `NULL` |
| `Time_Minute_From` | `int` | `NULL` |
| `Time_Hour_To` | `int` | `NULL` |
| `Time_Minute_To` | `int` | `NULL` |
| `Work_Hour` | `int` | `NULL` |
| `Work_Minute` | `int` | `NULL` |
| `Hour_Value` | `decimal(18, 2)` | `NULL` |
| `Total_Value` | `decimal(18, 2)` | `NULL` |
| `Time_Hour_From_` | `nvarchar(50)` | `NULL` |
| `Time_Hour_To_` | `nvarchar(50)` | `NULL` |
| `Done` | `int` | `NULL` |
| `joint_Qnty` | `decimal(18, 2)` | `NULL` |
| `Wardia_Id` | `int` | `NULL` |
| `Machine_Id` | `int` | `NULL` |
| `Dasta_No` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Machine_Id` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `EmployeeRelatedItems`

**الاستخدام الوظيفي:** ربط الموظفين بأصناف أو خدمات.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `EmployeesId` | `bigint` | `NULL` |
| `jobsListId` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `EmployeesId` → محتمل: `AppointmentSessionEmployees`, `CarsWorkOrdersItemProcessedEmployees`, `AppointmentServiceEmployees`
- `jobsListId` → محتمل: `jobslist`

---
### `Payment_Salaries`

**الاستخدام الوظيفي:** مدفوعات/صرف الرواتب.

**عدد الأعمدة:** 36

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Salary_Date` | `smalldatetime` | `NULL` |
| `Date_From` | `smalldatetime` | `NULL` |
| `Date_To` | `smalldatetime` | `NULL` |
| `Total_Work_Hour` | `decimal(18, 2)` | `NULL` |
| `Total_Work_Minute` | `decimal(18, 2)` | `NULL` |
| `Total_Work_Time_value` | `decimal(18, 2)` | `NULL` |
| `Total_Loans` | `decimal(18, 2)` | `NULL` |
| `Total_Repayment` | `decimal(18, 2)` | `NULL` |
| `Total_Foes` | `decimal(18, 2)` | `NULL` |
| `Total_Insurance` | `decimal(18, 2)` | `NULL` |
| `Total_Fees` | `decimal(18, 2)` | `NULL` |
| `Total_Equivalent` | `decimal(18, 2)` | `NULL` |
| `Total_License_days_value` | `decimal(18, 2)` | `NULL` |
| `Total_Special_Days_value` | `decimal(18, 2)` | `NULL` |
| `Net_salary` | `decimal(18, 2)` | `NULL` |
| `Total_Payment` | `decimal(18, 2)` | `NULL` |
| `Total_Discount` | `decimal(18, 2)` | `NULL` |
| `Emp_Salary` | `decimal(18, 2)` | `NULL` |
| `Emp_Day_Value` | `decimal(18, 2)` | `NULL` |
| `Production_Value` | `decimal(18, 2)` | `NULL` |
| `Loan_BalanceBefor` | `decimal(18, 2)` | `NULL` |
| `Loan_BalanceStill` | `decimal(18, 2)` | `NULL` |
| `Week_Loan` | `decimal(18, 2)` | `NULL` |
| `Lates_Values` | `decimal(18, 2)` | `NULL` |
| `Total_Absence_Value` | `decimal(18, 2)` | `NULL` |
| `Sum_Of_Absence_Days` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Presence_absence`

**الاستخدام الوظيفي:** سجل الحضور والانصراف والغياب للموظفين.

**عدد الأعمدة:** 31

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Status` | `int` | `NULL` |
| `Start_hour` | `int` | `NULL` |
| `Start_minut` | `int` | `NULL` |
| `End_Hour` | `int` | `NULL` |
| `End_Minut` | `int` | `NULL` |
| `presence_date` | `smalldatetime` | `NULL` |
| `Actual_Work_Hour` | `int` | `NULL` |
| `Actual_Work_Minut` | `int` | `NULL` |
| `Late_Period_Hour` | `int` | `NULL` |
| `Late_Period_Minut` | `int` | `NULL` |
| `Over_Time_Hour` | `int` | `NULL` |
| `Over_Time_Minute` | `int` | `NULL` |
| `Over_Time_` | `nvarchar(50)` | `NULL` |
| `Start_Time_` | `nvarchar(50)` | `NULL` |
| `End_Time_` | `nvarchar(50)` | `NULL` |
| `Late_Period_` | `nvarchar(50)` | `NULL` |
| `Actual_Work_Time_` | `nvarchar(50)` | `NULL` |
| `Done` | `int` | `NULL` |
| `OutEarly_` | `nvarchar(50)` | `NULL` |
| `OutEarly_Minute` | `int` | `NULL` |
| `OutEarly_Hour` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Salaries`

**الاستخدام الوظيفي:** بيانات الرواتب والاستحقاقات.

**عدد الأعمدة:** 24

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Salary_Date` | `smalldatetime` | `NULL` |
| `Date_From` | `smalldatetime` | `NULL` |
| `Date_To` | `smalldatetime` | `NULL` |
| `Total_Work_Hour` | `bigint` | `NULL` |
| `Total_Work_Minute` | `bigint` | `NULL` |
| `Total_Work_Time_value` | `bigint` | `NULL` |
| `Total_Loans` | `bigint` | `NULL` |
| `Total_Repayment` | `bigint` | `NULL` |
| `Total_Foes` | `bigint` | `NULL` |
| `Total_Insurance` | `bigint` | `NULL` |
| `Total_Fees` | `bigint` | `NULL` |
| `Total_Equivalent` | `bigint` | `NULL` |
| `Total_License_days_value` | `bigint` | `NULL` |
| `Net_salary` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Salary_increment`

**الاستخدام الوظيفي:** زيادات وتعديلات الرواتب.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Emp_id` | `bigint` | `NULL` |
| `increment` | `decimal(18, 2)` | `NULL` |
| `date_of_increment` | `datetime` | `NULL` |
| `MatArdia` | `decimal(18, 2)` | `NULL` |
| `MatWabara` | `decimal(18, 2)` | `NULL` |
| `MatLehaf` | `decimal(18, 2)` | `NULL` |
| `Hour_Value` | `decimal(18, 2)` | `NULL` |
| `Emp_Day_Value` | `decimal(18, 2)` | `NULL` |
| `IncrementDate` | `datetime` | `NULL` |
| `Salary_Increment` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Salary_Type`

**الاستخدام الوظيفي:** دليل أنواع الرواتب.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Type` | `nvarchar(50)` | `NULL` |
| `used` | `int` | `NULL` |
| `xm` | `char(10)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---

## 08 — الخزائن والبنوك والمدفوعات والشيكات
### `Banks`

**الاستخدام الوظيفي:** تعريف الحسابات البنكية/الخزائن وربطها بالحسابات المالية.

**عدد الأعمدة:** 29

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Bank_Name` | `nvarchar(50)` | `NULL` |
| `Primary_balance` | `decimal(18, 3)` | `NULL` |
| `Last_Balance` | `decimal(18, 3)` | `NULL` |
| `Account_No` | `nvarchar(50)` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Is_Bank` | `int` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `firstBalanceDate` | `datetime` | `NULL` |
| `Supplier_Bank` | `int` | `NULL` |
| `Default_Bank` | `int` | `NULL` |
| `Account_Code` | `nvarchar(50)` | `NULL` |
| `Static_Flage` | `int` | `NULL` |
| `Branch_Name` | `nvarchar(50)` | `NULL` |
| `Account_Type_Value` | `int` | `NULL` |
| `CompanyLo Type` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `Active` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Bank_Name_En` | `nvarchar(200)` | `NULL` |
| `ActualBalance` | `decimal(18, 3)` | `NULL` |
| `DiffBalance` | `decimal(18, 3)` | `NULL` |
| `Branches_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Banks_Balance`

**الاستخدام الوظيفي:** أرصدة الحسابات البنكية عبر الفترات أو الحركات.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Bank_Id` | `int` | `NULL` |
| `Balance_Date` | `datetime` | `NULL` |
| `First_Balance` | `decimal(18, 3)` | `NULL` |
| `Last_Balance` | `decimal(18, 3)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bank_Id` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Capital_Money`

**الاستخدام الوظيفي:** بيانات رأس المال وحصص/مساهمات الشركاء.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `partner_Name` | `nvarchar(50)` | `NULL` |
| `Percentage` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `capital_pulled`

**الاستخدام الوظيفي:** المسحوبات من رأس المال وتفاصيلها المالية.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Capital_id` | `bigint` | `NULL` |
| `DATE` | `datetime` | `NOT NULL` |
| `Value` | `decimal(18, 2)` | `NULL` |
| `Banke_id` | `int` | `NOT NULL` |
| `Notes` | `text` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Capital_id` → محتمل: `capital_pulled`, `Capital_Money`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Checks_Memory`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 59

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NULL` |
| `Order_Mony_Id` | `bigint` | `NULL` |
| `Check_No` | `nvarchar(50)` | `NOT NULL` |
| `Day_Date` | `datetime` | `NULL` |
| `Due_Date` | `datetime` | `NULL` |
| `Bank_id` | `bigint` | `NULL` |
| `Check_Value` | `decimal(18, 2)` | `NULL` |
| `Check_Value_` | `nvarchar(500)` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `Due_Check` | `nvarchar(500)` | `NULL` |
| `Value_Receipt_Date` | `datetime` | `NULL` |
| `Receipt` | `int` | `NULL` |
| `Notes` | `nvarchar(2000)` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Exchanger_Name` | `nvarchar(100)` | `NULL` |
| `First_Drawer_Name` | `nvarchar(100)` | `NULL` |
| `Bank_Balance` | `decimal(18, 2)` | `NULL` |
| `Last_Drawer_Name` | `nvarchar(100)` | `NULL` |
| `Balance_Before` | `decimal(18, 2)` | `NULL` |
| `Balance_After` | `decimal(18, 2)` | `NULL` |
| `Bank_Name` | `nvarchar(500)` | `NULL` |
| `SpentTo_Bank_Id` | `int` | `NULL` |
| `SpentTo_Bank_Type` | `int` | `NULL` |
| `Spent_Date` | `datetime` | `NULL` |
| `Bank_Id_Under_Ta7seel` | `int` | `NULL` |
| `Transfer_Check_Id` | `bigint` | `NULL` |
| `Balance_BeginingPeriod` | `decimal(18, 2)` | `NULL` |
| `Check_Notes` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Account_Id` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `SubAccount_NO` | `nvarchar(500)` | `NULL` |
| `SubAccount_Name` | `nvarchar(100)` | `NULL` |
| `MainSubAccount_ID` | `bigint` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Factor` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Name_RelatedTo` | `nvarchar(500)` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `SubAccount_Id_RelatedTo` | `bigint` | `NULL` |
| `SubAccount_Name_RelatedToBefor` | `nvarchar(100)` | `NULL` |
| `SubAccount_Id_RelatedToBefor` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `IsAllowEdit` | `int` | `NULL` |
| `IsAllowDelete` | `int` | `NULL` |
| `SubAccount_Name_RelatedFrom` | `nvarchar(500)` | `NULL` |
| `SubAccount_Id_RelatedFrom` | `bigint` | `NULL` |
| `Transfer_Check_Id_TO` | `bigint` | `NULL` |
| `Transfer_Check_Id_From` | `bigint` | `NULL` |
| `Checks_MemoryMainDataListId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bank_id` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `Checks_MemoryMainDataListId` → محتمل: `Checks_MemoryMainDataList`

---
### `Checks_MemoryHistory`

**الاستخدام الوظيفي:** سجل تاريخي للاحتفاظ بالتغييرات أو الحركات السابقة.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Checks_MemoryId` | `bigint` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `SubAccount_Id_RelatedTo` | `bigint` | `NULL` |
| `SubAccount_Name_RelatedTo` | `nvarchar(500)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `SubAccount_Id_RelatedToBefor` | `bigint` | `NULL` |
| `SubAccount_Name_RelatedToBefor` | `nvarchar(500)` | `NULL` |
| `SubAccount_Name_RelatedFrom` | `nvarchar(500)` | `NULL` |
| `SubAccount_Id_RelatedFrom` | `bigint` | `NULL` |
| `HistoryReceipt` | `int` | `NULL` |
| `HistoryDate` | `datetime` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Checks_MemoryId` → محتمل: `Checks_MemoryMainDataList`, `Checks_Memory`, `Checks_MemoryHistory`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`

---
### `Checks_MemoryMainDataList`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Check_No` | `nvarchar(50)` | `NULL` |
| `Day_Date` | `datetime` | `NULL` |
| `Due_Date` | `datetime` | `NULL` |
| `Bank_id` | `bigint` | `NULL` |
| `Check_Value` | `decimal(18, 2)` | `NULL` |
| `Check_Value_` | `nvarchar(500)` | `NULL` |
| `Due_Check` | `nvarchar(500)` | `NULL` |
| `Value_Receipt_Date` | `datetime` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Exchanger_Name` | `nvarchar(500)` | `NULL` |
| `First_Drawer_Name` | `nvarchar(500)` | `NULL` |
| `Last_Drawer_Name` | `nvarchar(500)` | `NULL` |
| `Bank_Name` | `nvarchar(500)` | `NULL` |
| `Check_Notes` | `nvarchar(500)` | `NULL` |
| `Receipt` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bank_id` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`

---
### `Draws_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Draw_Name` | `nvarchar(50)` | `NULL` |
| `Price` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `loans`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Loan_Date` | `smalldatetime` | `NULL` |
| `Loan_Value` | `decimal(18, 2)` | `NULL` |
| `Reason` | `nvarchar(1000)` | `NULL` |
| `Loan_Week` | `int` | `NULL` |
| `Status` | `int` | `NULL` |
| `Balance` | `decimal(18, 2)` | `NULL` |
| `Done` | `bit` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Money_Financial_Notice`

**الاستخدام الوظيفي:** رأس إشعارات/مستندات مالية نقدية.

**عدد الأعمدة:** 28

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `OrderNo` | `bigint` | `NULL` |
| `OrderDate` | `date` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Transaction_TypeStr` | `nvarchar(50)` | `NULL` |
| `Order_Value` | `decimal(18, 2)` | `NULL` |
| `Order_Value_Str` | `nvarchar(500)` | `NULL` |
| `Account_Type` | `int` | `NULL` |
| `Account_Id` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Compete_SubAccount_Id` | `bigint` | `NULL` |
| `Account_Selection` | `int` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Reference_Type` | `int` | `NULL` |
| `Account_Entry_List` | `bigint` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`

---
### `Money_Financial_Notice_Items`

**الاستخدام الوظيفي:** بنود الإشعارات المالية.

**عدد الأعمدة:** 32

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `OrderNo` | `bigint` | `NULL` |
| `OrderDate` | `date` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Transaction_TypeStr` | `nvarchar(50)` | `NULL` |
| `Order_Value` | `decimal(18, 2)` | `NULL` |
| `Order_Value_Str` | `nvarchar(500)` | `NULL` |
| `Account_Type` | `int` | `NULL` |
| `Account_Id` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Compete_SubAccount_Id` | `bigint` | `NULL` |
| `Account_Selection` | `int` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `Item_Qnty` | `decimal(18, 2)` | `NULL` |
| `Item_Price` | `decimal(18, 2)` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `Reference_Type` | `int` | `NULL` |
| `Account_Entry_List` | `bigint` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`

---
### `Money_Financial_Notice_Items_Details`

**الاستخدام الوظيفي:** تفاصيل بنود الإشعارات المالية.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Money_Financial_Notice_Items_Id` | `bigint` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Item_QntyLarge` | `decimal(18, 2)` | `NULL` |
| `Item_QntySmall` | `nchar(10)` | `NULL` |
| `Item_Price` | `decimal(18, 2)` | `NULL` |
| `Item_ToTal` | `decimal(18, 2)` | `NULL` |
| `Unit_Item_Id` | `bigint` | `NULL` |
| `Store_Id` | `bigint` | `NULL` |
| `NotesDetails` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Money_Financial_Notice_Items_Id` → محتمل: `Money_Financial_Notice_Items_Details`, `Money_Financial_Notice_Items`
- `Unit_Item_Id` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Money_Transaction_Bank`

**الاستخدام الوظيفي:** حركات مالية على الحسابات البنكية.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Bank_Id` | `int` | `NULL` |
| `Transaction_Date` | `datetime` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `Transaction_Value` | `decimal(18, 2)` | `NULL` |
| `Bank_Balance` | `decimal(18, 2)` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Reference_Type` | `int` | `NULL` |
| `Reference_Type_Id` | `bigint` | `NULL` |
| `Account_ID` | `nchar(10)` | `NULL` |
| `Day_Registration_flage` | `int` | `NULL` |
| `DayReg_List_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bank_Id` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Money_Transaction_Box`

**الاستخدام الوظيفي:** حركات الخزينة النقدية.

**عدد الأعمدة:** 67

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Order_Mony_Id` | `bigint` | `NULL` |
| `Transaction_Date` | `datetime` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `Transaction_Value` | `decimal(18, 2)` | `NULL` |
| `Transaction_Value_` | `nvarchar(500)` | `NULL` |
| `Equivalent_Value` | `decimal(18, 3)` | `NULL` |
| `Bank_Id` | `bigint` | `NULL` |
| `Bank_Balance` | `decimal(18, 2)` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Account_Id` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `Balance_Before` | `decimal(18, 3)` | `NULL` |
| `Balance_After` | `decimal(18, 3)` | `NULL` |
| `BankAccount_Id` | `bigint` | `NULL` |
| `DestructionSales` | `int` | `NULL` |
| `Order_mony_id_transform` | `bigint` | `NULL` |
| `Account_Cost_Center_Tree_Id` | `bigint` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Tafkeet_Value` | `nvarchar(max)` | `NULL` |
| `TotalValueWillPaid` | `decimal(18, 3)` | `NULL` |
| `Rest_Value` | `decimal(18, 3)` | `NULL` |
| `Account_Name` | `nvarchar(max)` | `NULL` |
| `Account_Code` | `nvarchar(max)` | `NULL` |
| `Account_Type` | `int` | `NULL` |
| `Cost_Center` | `nvarchar(max)` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Purchase_Ticket_Value` | `decimal(18, 3)` | `NULL` |
| `Commission_Value` | `decimal(18, 3)` | `NULL` |
| `Factor` | `decimal(18, 3)` | `NULL` |
| `Entry_Currency_ID` | `int` | `NULL` |
| `CostCenter_Name` | `nvarchar(max)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Invoice_TypeId` | `int` | `NULL` |
| `Invoice_Id` | `bigint` | `NULL` |
| `Balance_BeforStr` | `nvarchar(max)` | `NULL` |
| `Balance_AfterStr` | `nvarchar(max)` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `Lookup_CashPaymentMethodId` | `bigint` | `NULL` |
| `Lookup_CashPaymentMethodName` | `nvarchar(500)` | `NULL` |
| `Order_No` | `bigint` | `NULL` |
| `Bank_Name` | `nvarchar(max)` | `NULL` |
| `BankSubAccount_Id` | `bigint` | `NULL` |
| `IsDefault_Bank` | `int` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `Invoice_No` | `bigint` | `NULL` |
| `CarNumber` | `nvarchar(max)` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `SubOrder_Mony_Id` | `int` | `NULL` |
| `Is_ProcessedFlage` | `int` | `NULL` |
| `SubOrder_Money_Id` | `bigint` | `NULL` |
| `TaxInternalId` | `nvarchar(max)` | `NULL` |
| `DataDictionaryProcessesListsId` | `bigint` | `NULL` |
| `Reference_Type_Table_Id` | `bigint` | `NULL` |
| `Serial` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Bank_Id` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`
- `BankAccount_Id` → محتمل: `LookUp_CheckStatusBankAccount`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `Lookup_CashPaymentMethodId` → محتمل: `Lookup_CashPaymentMethod`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `DataDictionaryProcessesListsId` → محتمل: `DataDictionaryProcessesLists`

---
### `Money_Transaction_BoxForInvoices`

**الاستخدام الوظيفي:** حركات الخزينة المرتبطة بالفواتير.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Money_Transaction_BoxId` | `bigint` | `NULL` |
| `Order_Mony_Id` | `bigint` | `NULL` |
| `Order_No` | `bigint` | `NULL` |
| `Invoices_ListId` | `bigint` | `NULL` |
| `Invoice_TypeId` | `int` | `NULL` |
| `Bills_ListId` | `bigint` | `NULL` |
| `Bank_Id` | `bigint` | `NULL` |
| `Reference_Type_Table_Id` | `bigint` | `NULL` |
| `DataDictionaryProcessesListsId` | `bigint` | `NULL` |
| `Transaction_Date` | `datetime` | `NULL` |
| `Order_Total_Value` | `decimal(18, 2)` | `NULL` |
| `Transaction_Value` | `decimal(18, 2)` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Money_Transaction_BoxId` → محتمل: `Money_Transaction_BoxForInvoices`, `Money_Transaction_Box`
- `Bills_ListId` → محتمل: `Bills_List_Reverse`, `Bills_List`
- `Bank_Id` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`
- `DataDictionaryProcessesListsId` → محتمل: `DataDictionaryProcessesLists`

---
### `Takset_Details`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 25

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Takset_List_Id` | `bigint` | `NULL` |
| `DateOfDue` | `datetime` | `NULL` |
| `kest_Value_needed` | `decimal(18, 2)` | `NULL` |
| `Payed_Value` | `decimal(18, 2)` | `NULL` |
| `Rest_Value` | `decimal(18, 2)` | `NULL` |
| `DateOfPayment` | `datetime` | `NULL` |
| `Increased_Values` | `decimal(18, 2)` | `NULL` |
| `Payment_Type` | `int` | `NULL` |
| `Payment_TypeStr` | `nvarchar(50)` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Order_Mony_Id` | `bigint` | `NULL` |
| `Payed_ValueSaved` | `decimal(18, 2)` | `NULL` |
| `Rest_ValueSaved` | `decimal(18, 2)` | `NULL` |
| `Increased_ValuesSaved` | `decimal(18, 2)` | `NULL` |
| `Total_Value_Payed` | `decimal(18, 2)` | `NULL` |
| `OrderNo` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Takset_List_Id` → محتمل: `Takset_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Takset_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 27

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Invoice_List_Id` | `bigint` | `NULL` |
| `Invoices_Details_Id` | `bigint` | `NULL` |
| `Total_Money` | `decimal(18, 2)` | `NULL` |
| `First_PaymentMoney` | `decimal(18, 2)` | `NULL` |
| `DateOfFirst_PaymentMoney` | `datetime` | `NULL` |
| `DateOfFirst_Takset` | `datetime` | `NULL` |
| `Statements_No` | `nvarchar(50)` | `NULL` |
| `Notebooks_No` | `nvarchar(50)` | `NULL` |
| `Guarantor_Name` | `nvarchar(50)` | `NULL` |
| `Guarantor_Id` | `nvarchar(50)` | `NULL` |
| `DateDay_Payment` | `int` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Takset_Number` | `int` | `NULL` |
| `kest_Value_needed` | `decimal(18, 2)` | `NULL` |
| `Increased_Values` | `decimal(18, 2)` | `NULL` |
| `Total_Takset_Value` | `decimal(18, 2)` | `NULL` |
| `DateOfProcess` | `datetime` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Invoices_Details_Id` → محتمل: `Invoices_Details_Reverse`, `Invoices_Details`, `Invoices_DetailsTemp`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---

## 09 — الإنتاج والنسيج والتصنيع
### `Machin_list`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Machin_Name` | `varchar(50)` | `NOT NULL` |
| `price_of_nazlla` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `active` | `int` | `NULL` |
| `Abstract_Hadafat` | `decimal(18, 2)` | `NULL` |
| `Speed` | `decimal(18, 2)` | `NULL` |
| `Machine_Width` | `int` | `NULL` |
| `Code` | `nvarchar(max)` | `NULL` |
| `Description` | `nvarchar(max)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Machin_list_Textile`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Machin_Name` | `varchar(50)` | `NOT NULL` |
| `price_of_nazlla` | `decimal(18, 2)` | `NULL` |
| `Code` | `nvarchar(50)` | `NULL` |
| `Description` | `nvarchar(500)` | `NULL` |
| `Speed` | `decimal(18, 2)` | `NULL` |
| `Machine_Width` | `int` | `NULL` |
| `Abstract_Hadafat` | `decimal(18, 0)` | `NULL` |
| `Machin_Type` | `int` | `NULL` |
| `Machine_Id` | `int` | `NULL` |
| `Price_Day_Alf_Hadfa` | `decimal(18, 2)` | `NULL` |
| `Machine_Stitch_No` | `decimal(18, 2)` | `NULL` |
| `Atala_efficiency` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Machine_Id` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Matwa_Items`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `Matwa_List_Id` | `int` | `NULL` |
| `Matwa_Item_Name` | `nvarchar(50)` | `NULL` |
| `Matwa_Item_No` | `decimal(18, 0)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Matwa_List_Id` → محتمل: `Matwa_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Matwa_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Matwa_Name` | `nvarchar(50)` | `NULL` |
| `Seda_Matwa_Price` | `decimal(18, 2)` | `NULL` |
| `Meparazaty_Matwa_Price` | `decimal(18, 2)` | `NULL` |
| `Type_ID` | `bigint` | `NULL` |
| `Type_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Matwa_Production_Process`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 22

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Prod_Date` | `datetime` | `NULL` |
| `Emp_Id` | `int` | `NULL` |
| `Matwa_List_Id` | `int` | `NULL` |
| `Matwa_Items_Id` | `int` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Item_Balance` | `decimal(18, 2)` | `NULL` |
| `Matwa_Price` | `decimal(18, 2)` | `NULL` |
| `Total_Price` | `decimal(18, 2)` | `NULL` |
| `Process_Description` | `nvarchar(50)` | `NULL` |
| `Salary_Type_Id` | `int` | `NULL` |
| `Matwa_Production_Rate_Id` | `bigint` | `NULL` |
| `Matwa_Production_SendToTextileMachine_Id` | `bigint` | `NULL` |
| `Machine_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Matwa_List_Id` → محتمل: `Matwa_List`
- `Matwa_Items_Id` → محتمل: `Matwa_Items`
- `Salary_Type_Id` → محتمل: `Salary_Type`
- `Matwa_Production_Rate_Id` → محتمل: `Matwa_Production_Rate`
- `Matwa_Production_SendToTextileMachine_Id` → محتمل: `Matwa_Production_SendToTextileMachine`
- `Machine_Id` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Matwa_Production_Rate`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Prod_Date` | `datetime` | `NULL` |
| `Emp_Id` | `int` | `NULL` |
| `Matwa_List_Id` | `int` | `NULL` |
| `Matwa_Items_Id` | `int` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Matwa_Price` | `decimal(18, 2)` | `NULL` |
| `Total_Price` | `decimal(18, 2)` | `NULL` |
| `Salary_Type_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Matwa_List_Id` → محتمل: `Matwa_List`
- `Matwa_Items_Id` → محتمل: `Matwa_Items`
- `Salary_Type_Id` → محتمل: `Salary_Type`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Matwa_Production_SendToTextileMachine`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Prod_Date` | `datetime` | `NULL` |
| `Machine_Id` | `int` | `NULL` |
| `Matwa_List_Id` | `int` | `NULL` |
| `Matwa_Items_Id` | `int` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Machine_Id` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`
- `Matwa_List_Id` → محتمل: `Matwa_List`
- `Matwa_Items_Id` → محتمل: `Matwa_Items`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Production_Atalat`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Atalat_Name` | `nvarchar(100)` | `NULL` |
| `Hour_Price_Value` | `decimal(18, 2)` | `NULL` |
| `Types` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Textile_Process_Transaction`

**الاستخدام الوظيفي:** سجل معاملات/حركات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 33

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Import_Qnty` | `decimal(18, 2)` | `NULL` |
| `Import_Weight` | `decimal(18, 2)` | `NULL` |
| `Export_Qnty` | `decimal(18, 2)` | `NULL` |
| `Export_Weight` | `decimal(18, 2)` | `NULL` |
| `Qnty_Transaction` | `decimal(18, 2)` | `NULL` |
| `Weight_Transaction` | `decimal(18, 2)` | `NULL` |
| `Qnty_Balance` | `decimal(18, 2)` | `NULL` |
| `Weight_Balance` | `decimal(18, 2)` | `NULL` |
| `Processed_Flage` | `int` | `NULL` |
| `From_Dept_Id` | `int` | `NULL` |
| `To_Dept_Id` | `int` | `NULL` |
| `Transaction_Date` | `datetime` | `NULL` |
| `Textile_Process_Transaction_Id` | `bigint` | `NULL` |
| `Processed_Flage_Date` | `datetime` | `NULL` |
| `Textile_Item_Dept_Balance_Id` | `int` | `NULL` |
| `Textile_Production_Rate_Id` | `bigint` | `NULL` |
| `Fa7s_Result_Id` | `int` | `NULL` |
| `Fa7s_Result_Name` | `nvarchar(50)` | `NULL` |
| `Notes` | `nvarchar(100)` | `NULL` |
| `Item_Type` | `int` | `NULL` |
| `Dept_Id_Transaction` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Is_Motarz` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Textile_Process_Transaction_Id` → محتمل: `Textile_Process_Transaction`
- `Textile_Item_Dept_Balance_Id` → محتمل: `Textile_Item_Dept_Balance`
- `Textile_Production_Rate_Id` → محتمل: `Textile_Production_Rate_Atlat`, `Textile_Production_Rate_ItemDetails`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Textile_Production_Rate_Atlat`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Textile_Production_Rate_Id` | `bigint` | `NULL` |
| `Atalat_Id` | `bigint` | `NULL` |
| `Atalat_Name` | `nvarchar(50)` | `NULL` |
| `Hour_No` | `decimal(18, 2)` | `NULL` |
| `Atla_Value` | `decimal(18, 2)` | `NULL` |
| `Hour_Price_Value` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Textile_Production_Rate_Id` → محتمل: `Textile_Production_Rate_Atlat`, `Textile_Production_Rate_ItemDetails`
- `Atalat_Id` → محتمل: `Production_Atalat`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Textile_Production_Rate_ItemDetails`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Textile_Production_Rate_Id` | `bigint` | `NULL` |
| `Item_PriceUnit_List_Id` | `bigint` | `NULL` |
| `Item_Name` | `nvarchar(50)` | `NULL` |
| `Nazalat_Prod` | `decimal(18, 2)` | `NULL` |
| `Hadafat_Prod` | `decimal(18, 2)` | `NULL` |
| `Hadafat_Prod_Net` | `decimal(18, 2)` | `NULL` |
| `Dasta_No_In_Wardia` | `decimal(18, 2)` | `NULL` |
| `Item_No_In_Wardia` | `decimal(18, 2)` | `NULL` |
| `Hadafat_Matluba` | `decimal(18, 2)` | `NULL` |
| `Nazalat_Matluba` | `decimal(18, 2)` | `NULL` |
| `Hadafat_Zeiada` | `decimal(18, 2)` | `NULL` |
| `Nazalat_Zeiada` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Textile_Production_Rate_Id` → محتمل: `Textile_Production_Rate_Atlat`, `Textile_Production_Rate_ItemDetails`
- `Item_PriceUnit_List_Id` → محتمل: `Item_PriceUnit_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---

## 10 — المقاولات/العقارات والخدمات الاستهلاكية
### `ConsCustomerDebitDetails`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CustomerDebitListId` | `bigint` | `NULL` |
| `CustomerId` | `int` | `NULL` |
| `ProjectUnitesId` | `int` | `NULL` |
| `ServicesId` | `int` | `NULL` |
| `UnitSize` | `decimal(18, 2)` | `NULL` |
| `ServicePrice` | `decimal(18, 2)` | `NULL` |
| `TotalDebit` | `decimal(18, 2)` | `NULL` |
| `latePrice` | `decimal(18, 2)` | `NULL` |
| `NetTotalDebit` | `decimal(18, 2)` | `NULL` |
| `SubAccount_Id_Customer` | `bigint` | `NULL` |
| `SubAccount_Id_Service` | `bigint` | `NULL` |
| `Account_Entry_ListId` | `bigint` | `NULL` |
| `PeriodId` | `bigint` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `PeriodServiceTypeId` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `CustomerDebitListId` → محتمل: `ConsCustomerDebitList`, `ConsCustomerDebitList2`
- `ProjectUnitesId` → محتمل: `ConsProjectUnites`
- `Account_Entry_ListId` → محتمل: `Account_Entry_List`
- `PeriodServiceTypeId` → محتمل: `ConsPeriodServiceType`

---
### `ConsCustomerDebitList`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `DebitDate` | `datetime` | `NULL` |
| `DateFrom` | `datetime` | `NULL` |
| `DateTo` | `datetime` | `NULL` |
| `DueDate` | `datetime` | `NULL` |
| `ServicesGroupListId` | `int` | `NULL` |
| `ProjectSpacesGroupId` | `int` | `NULL` |
| `ProjectId` | `int` | `NULL` |
| `ListDebitTotal` | `decimal(18, 2)` | `NULL` |
| `PeriodId` | `int` | `NULL` |
| `ServicesId` | `int` | `NULL` |
| `ListNetDebitTotal` | `decimal(18, 2)` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `PeriodServiceTypeId` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ServicesGroupListId` → محتمل: `ConsServicesGroupList`
- `ProjectSpacesGroupId` → محتمل: `ConsProjectSpacesGroup`
- `ProjectId` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`
- `PeriodServiceTypeId` → محتمل: `ConsPeriodServiceType`

---
### `ConsCustomerDebitList2`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 8

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CustomerId` | `int` | `NULL` |
| `ProjectId` | `int` | `NULL` |
| `Total` | `decimal(18, 2)` | `NULL` |
| `DateFrom` | `datetime` | `NULL` |
| `DateTo` | `datetime` | `NULL` |
| `CustomerOwnerId` | `nchar(10)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ProjectId` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`
- `CustomerOwnerId` → محتمل: `ConsCustomerOwner`

---
### `ConsCustomerOwner`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `CustomerId` | `int` | `NULL` |
| `ProjectUnitesId` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ProjectUnitesId` → محتمل: `ConsProjectUnites`

---
### `ConsDue_Period`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `DateFrom` | `datetime` | `NOT NULL` |
| `DateTo` | `datetime` | `NULL` |
| `PeriodYear` | `int` | `NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `Done` | `int` | `NULL` |
| `LatePrice` | `decimal(18, 2)` | `NULL` |
| `DueDate` | `datetime` | `NULL` |
| `FirstPeriod` | `int` | `NULL` |
| `IS_Active` | `int` | `NULL` |
| `IS_Finished_Posted` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `ConsPeriod`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `PeriodName` | `nvarchar(50)` | `NULL` |
| `DateFrom` | `datetime` | `NULL` |
| `DateTo` | `datetime` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

---
### `ConsPeriodServiceType`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

---
### `ConsProject`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(100)` | `NULL` |
| `Address` | `nvarchar(100)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

---
### `ConsProjectSpacesGroup`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `ProjectId` | `bigint` | `NULL` |
| `Size` | `decimal(18, 2)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ProjectId` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`

---
### `ConsProjectUnites`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ProjectId` | `int` | `NULL` |
| `ResidentialbuildingId` | `int` | `NULL` |
| `ResidentialbuildingName` | `nvarchar(100)` | `NULL` |
| `UnitNo` | `nvarchar(100)` | `NULL` |
| `UnitCode` | `nvarchar(100)` | `NULL` |
| `UnitTypeId` | `int` | `NULL` |
| `ProjectSpacesGroupId` | `int` | `NULL` |
| `ServicesGroupId` | `int` | `NULL` |
| `Size` | `decimal(18, 2)` | `NULL` |
| `WaterCounterTypeId` | `int` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `WaterCounterNo` | `nvarchar(100)` | `NULL` |
| `CustomerId` | `int` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `IsRelatedCustomer` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ProjectId` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`
- `ResidentialbuildingId` → محتمل: `ConsResidentialbuilding`
- `UnitTypeId` → محتمل: `ConsUnitType`
- `ProjectSpacesGroupId` → محتمل: `ConsProjectSpacesGroup`
- `ServicesGroupId` → محتمل: `ConsServicesGroups`, `ConsServicesGroupList`
- `WaterCounterTypeId` → محتمل: `ConsWaterCounterType`

---
### `ConsResidentialbuilding`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `Address` | `nvarchar(50)` | `NULL` |
| `ProjectId` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ProjectId` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`

---
### `ConsService`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(100)` | `NULL` |
| `Price` | `decimal(18, 2)` | `NULL` |
| `PeriodServiceTypeId` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `PeriodServiceTypeId` → محتمل: `ConsPeriodServiceType`

---
### `ConsServicesGroupList`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `ProjectId` | `bigint` | `NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ProjectId` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`

---
### `ConsServicesGroups`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ServicesGroupListId` | `int` | `NULL` |
| `ServicesId` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ServicesGroupListId` → محتمل: `ConsServicesGroupList`

---
### `ConsServicesUnites`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ProjectUnitesId` | `int` | `NOT NULL` |
| `ServicesId` | `int` | `NOT NULL` |
| `ServicesGroupListId` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `ProjectUnitesId` → محتمل: `ConsProjectUnites`
- `ServicesGroupListId` → محتمل: `ConsServicesGroupList`

---
### `ConsUnitType`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(100)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

---
### `ConsWaterCounterType`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

---

## 11 — السياحة والرحلات والتذاكر
### `Tours_Hotels`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 30

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Passenger_Name` | `nvarchar(100)` | `NULL` |
| `DateOfReserved` | `datetime` | `NULL` |
| `Hotel_Name` | `nvarchar(100)` | `NULL` |
| `City` | `nvarchar(100)` | `NULL` |
| `Country` | `nvarchar(100)` | `NULL` |
| `DateOfEntry` | `datetime` | `NULL` |
| `DateOfLeave` | `datetime` | `NULL` |
| `PurchasePrice` | `decimal(18, 2)` | `NULL` |
| `TicketPrice` | `decimal(18, 2)` | `NULL` |
| `Commission` | `decimal(18, 2)` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `IsTicket_Reverse` | `int` | `NULL` |
| `DateOfReverse` | `datetime` | `NULL` |
| `PriceReverse_ToSupplier` | `decimal(18, 2)` | `NULL` |
| `PriceReverse_FromCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffSupplier` | `decimal(18, 2)` | `NULL` |
| `FinalReverseDiff` | `decimal(18, 2)` | `NULL` |
| `Emp_Booking` | `nvarchar(200)` | `NULL` |
| `Emp_Booking_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Tours_Omra`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 30

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Is_Omra_Heg` | `int` | `NULL` |
| `Tours_Program_Omra_Heg_List_Id` | `int` | `NULL` |
| `Passenger_Name` | `nvarchar(100)` | `NULL` |
| `Mandop_Name` | `nvarchar(100)` | `NULL` |
| `DateOfReserved` | `datetime` | `NULL` |
| `DateOfTravel` | `datetime` | `NULL` |
| `DateOfReturn` | `datetime` | `NULL` |
| `PurchasePrice` | `decimal(18, 2)` | `NULL` |
| `TicketPrice` | `decimal(18, 2)` | `NULL` |
| `Commission` | `decimal(18, 2)` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `IsTicket_Reverse` | `int` | `NULL` |
| `DateOfReverse` | `datetime` | `NULL` |
| `PriceReverse_ToSupplier` | `decimal(18, 2)` | `NULL` |
| `PriceReverse_FromCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffSupplier` | `decimal(18, 2)` | `NULL` |
| `FinalReverseDiff` | `decimal(18, 2)` | `NULL` |
| `Emp_Booking` | `nvarchar(200)` | `NULL` |
| `Emp_Booking_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Tours_Program_Omra_Heg_List_Id` → محتمل: `Tours_Program_Omra_Heg_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Tours_Program_Omra_Heg_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 27

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Is_Omra_Heg` | `int` | `NULL` |
| `Prog_Name` | `nvarchar(100)` | `NULL` |
| `Travel_By` | `nvarchar(100)` | `NULL` |
| `Ticket_Price` | `decimal(18, 2)` | `NULL` |
| `Tashera_Price` | `decimal(18, 2)` | `NULL` |
| `Sakn_Makka` | `decimal(18, 2)` | `NULL` |
| `Sakn_Madina` | `decimal(18, 2)` | `NULL` |
| `Transport_Eg` | `decimal(18, 2)` | `NULL` |
| `Transport_Sa` | `decimal(18, 2)` | `NULL` |
| `Eshraf_Price` | `decimal(18, 2)` | `NULL` |
| `OtherCost` | `decimal(18, 2)` | `NULL` |
| `Prog_Total` | `decimal(18, 2)` | `NULL` |
| `Days_Makka` | `int` | `NULL` |
| `Days_Madina` | `int` | `NULL` |
| `Arrival_Port` | `nvarchar(100)` | `NULL` |
| `Travel_Port` | `nvarchar(100)` | `NULL` |
| `Is_Active` | `int` | `NULL` |
| `Sell_Price` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Tours_Tasherat`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 28

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Passenger_Name` | `nvarchar(100)` | `NULL` |
| `Tasherat_Type` | `nvarchar(100)` | `NULL` |
| `DateOfEstlamPassport` | `datetime` | `NULL` |
| `DateOfTaslem` | `datetime` | `NULL` |
| `PurchasePrice` | `decimal(18, 2)` | `NULL` |
| `TicketPrice` | `decimal(18, 2)` | `NULL` |
| `Commission` | `decimal(18, 2)` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Order_Money_Id` | `bigint` | `NULL` |
| `IsTicket_Reverse` | `int` | `NULL` |
| `DateOfReverse` | `datetime` | `NULL` |
| `PriceReverse_ToSupplier` | `decimal(18, 2)` | `NULL` |
| `PriceReverse_FromCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffSupplier` | `decimal(18, 2)` | `NULL` |
| `FinalReverseDiff` | `decimal(18, 2)` | `NULL` |
| `Emp_Booking` | `nvarchar(200)` | `NULL` |
| `Emp_Booking_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Order_Money_Id` → محتمل: `Order_Money_Check`, `Order_Money`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Tours_Ticketing_Air`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 38

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `DirectionFirst` | `nvarchar(100)` | `NULL` |
| `DirectionSecond` | `nvarchar(100)` | `NULL` |
| `DirectionThird` | `nvarchar(100)` | `NULL` |
| `DirectionTotal` | `nvarchar(100)` | `NULL` |
| `Ticket_No` | `nvarchar(100)` | `NULL` |
| `Booking_No` | `nvarchar(100)` | `NULL` |
| `Passenger_Name` | `nvarchar(100)` | `NULL` |
| `ISOneWayRoundTrip` | `int` | `NULL` |
| `Carrier_Company` | `nvarchar(100)` | `NULL` |
| `DateOfReserved` | `datetime` | `NULL` |
| `DateOfTravel` | `datetime` | `NULL` |
| `DateOfReturn` | `datetime` | `NULL` |
| `PurchasePrice` | `decimal(18, 2)` | `NULL` |
| `TicketPrice` | `decimal(18, 2)` | `NULL` |
| `Commission` | `decimal(18, 2)` | `NULL` |
| `Emp_Booking` | `nvarchar(100)` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Order_Money_Id` | `bigint` | `NULL` |
| `PaidValue` | `decimal(18, 2)` | `NULL` |
| `RestValue` | `decimal(18, 2)` | `NULL` |
| `IsTicket_Reverse` | `int` | `NULL` |
| `DateOfReverse` | `datetime` | `NULL` |
| `PriceReverse_ToSupplier` | `decimal(18, 2)` | `NULL` |
| `PriceReverse_FromCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffSupplier` | `decimal(18, 2)` | `NULL` |
| `FinalReverseDiff` | `decimal(18, 2)` | `NULL` |
| `Emp_Booking_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Order_Money_Id` → محتمل: `Order_Money_Check`, `Order_Money`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Tours_Ticketing_Land`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 43

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `DirectionTotal` | `nvarchar(100)` | `NULL` |
| `Emp_Booking` | `nvarchar(100)` | `NULL` |
| `Passenger_Name` | `nvarchar(100)` | `NULL` |
| `DateOfReserved` | `datetime` | `NULL` |
| `DateOfTravel` | `datetime` | `NULL` |
| `PurchasePrice_Eg` | `decimal(18, 2)` | `NULL` |
| `PurchasePrice_Sea` | `decimal(18, 2)` | `NULL` |
| `PurchasePrice_Sa` | `decimal(18, 2)` | `NULL` |
| `PurchasePrice_Masar` | `decimal(18, 2)` | `NULL` |
| `TicketCostOther` | `decimal(18, 2)` | `NULL` |
| `PurchasePrice_Total` | `decimal(18, 2)` | `NULL` |
| `TicketPrice_Eg` | `decimal(18, 2)` | `NULL` |
| `TicketPrice_Sea` | `decimal(18, 2)` | `NULL` |
| `TicketPrice_Sa` | `decimal(18, 2)` | `NULL` |
| `TicketPriceTotal` | `decimal(18, 2)` | `NULL` |
| `Commission` | `decimal(18, 2)` | `NULL` |
| `Supplier_Id_Eg` | `bigint` | `NULL` |
| `Supplier_Id_Sea` | `bigint` | `NULL` |
| `Supplier_Id_Sa` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `IsTicket_Reverse` | `int` | `NULL` |
| `DateOfReverse` | `datetime` | `NULL` |
| `PriceReverse_ToSupplier` | `decimal(18, 2)` | `NULL` |
| `PriceReverse_FromCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffCustomer` | `decimal(18, 2)` | `NULL` |
| `ReverseDiffSupplier` | `decimal(18, 2)` | `NULL` |
| `FinalReverseDiff` | `decimal(18, 2)` | `NULL` |
| `Transport_Eg` | `nvarchar(100)` | `NULL` |
| `Transport_Sea` | `nvarchar(100)` | `NULL` |
| `Transport_Sa` | `nvarchar(100)` | `NULL` |
| `DirectionEgypt` | `nvarchar(200)` | `NULL` |
| `DirectionSea` | `nvarchar(200)` | `NULL` |
| `DirectionSa` | `nvarchar(200)` | `NULL` |
| `Emp_Booking_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---

## 12 — النظام والمستخدمون والصلاحيات والإعدادات
### `Authentication`

**الاستخدام الوظيفي:** بيانات ربط المستخدمين بحسابات الدخول/الموظفين والمصادقة.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `UserName` | `nvarchar(50)` | `NOT NULL` |
| `Password` | `nvarchar(50)` | `NOT NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `DataDictionary`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `SQLText` | `nvarchar(max)` | `NULL` |
| `Read_Flage` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `MethodName` | `nvarchar(max)` | `NULL` |
| `TableName` | `nvarchar(1000)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `DataDictionaryDbSyncLogs`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `TableName` | `nvarchar(128)` | `NOT NULL` |
| `RecordId` | `bigint` | `NOT NULL` |
| `RecordIdStr` | `nvarchar(max)` | `NULL` |
| `ActionType` | `varchar(10)` | `NOT NULL` |
| `ActionType2` | `varchar(10)` | `NULL` |
| `Status` | `varchar(10)` | `NOT NULL` |
| `CreatedAt` | `datetime` | `NOT NULL` |
| `ProcessedAt` | `datetime` | `NULL` |
| `RetryCount` | `int` | `NOT NULL` |
| `LastError` | `nvarchar(max)` | `NULL` |
| `payload` | `nvarchar(max)` | `NULL` |
| `mysqlCmd` | `nvarchar(max)` | `NULL` |
| `Branch_Id` | `bigint` | `NOT NULL` |

---
### `DataDictionaryLogerExceptions`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 8

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Exception` | `nvarchar(max)` | `NULL` |
| `Message` | `nvarchar(max)` | `NULL` |
| `InnerException` | `nvarchar(max)` | `NULL` |
| `Source` | `nvarchar(max)` | `NULL` |
| `StackTrace` | `nvarchar(max)` | `NULL` |
| `MethodName` | `nvarchar(max)` | `NULL` |
| `ExcpDateTime` | `datetime` | `NULL` |

---
### `DataDictionaryLogerProcesses`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ProcessId` | `bigint` | `NULL` |
| `ProcessName` | `nvarchar(500)` | `NULL` |
| `SubProcessId` | `bigint` | `NULL` |
| `SubProcessName` | `nvarchar(500)` | `NULL` |
| `ProcessDocNo` | `bigint` | `NULL` |
| `ProcessTableId` | `bigint` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `UserName` | `nvarchar(500)` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `ProcessDateTime` | `datetime` | `NULL` |
| `ProcessNotes` | `nvarchar(500)` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `Qnty` | `nvarchar(500)` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Unit_Item_Id` → محتمل: `Unit_Item_Balance_Date_Details`, `Unit_item`, `Unit_itemEmployeeBalance`, `Unit_Item_ExpireDate`

---
### `DataDictionaryProcessesLists`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 6

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ProcessName` | `nvarchar(500)` | `NULL` |
| `OrderTypes_Value` | `bigint` | `NULL` |
| `BtnRecive_Text` | `nvarchar(50)` | `NULL` |
| `Order_Estlam_Sarf` | `int` | `NULL` |
| `Case_Name` | `nvarchar(50)` | `NULL` |

---
### `DataDictionaryProcessesNo`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TableName` | `nvarchar(500)` | `NULL` |

---
### `FORMS_DETAILES`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `FORM_ID` | `bigint` | `NULL` |
| `Parent_Id` | `bigint` | `NULL` |
| `FORM_NAME` | `nvarchar(50)` | `NULL` |
| `ColumnOrder` | `int` | `NULL` |
| `Form_NameID` | `nvarchar(100)` | `NULL` |
| `Form_Tag` | `nvarchar(50)` | `NULL` |
| `PROJECT_ID` | `int` | `NULL` |
| `IS_MainMenu` | `int` | `NULL` |
| `IS_VisibleWhenAssign` | `int` | `NULL` |
| `IS_Menu_Saved` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `FORM_ID` → محتمل: `FORMS_DETAILES`
- `PROJECT_ID` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Modules_Name`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Module_Name` | `nvarchar(50)` | `NULL` |
| `Module_Name_R` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `SysConfig`

**الاستخدام الوظيفي:** إعدادات النظام العامة أو إعدادات الوحدات والقوائم.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `KeysData` | `nvarchar(100)` | `NULL` |
| `DefaultValue` | `nvarchar(500)` | `NULL` |
| `Value` | `nvarchar(500)` | `NULL` |
| `DefaultDescription` | `nvarchar(500)` | `NULL` |
| `Description` | `nvarchar(500)` | `NULL` |
| `Client_B` | `nvarchar(500)` | `NULL` |
| `Client_A` | `nvarchar(500)` | `NULL` |
| `POS` | `nvarchar(500)` | `NULL` |
| `Market` | `nvarchar(500)` | `NULL` |
| `Factory` | `nvarchar(500)` | `NULL` |
| `AlFawzan` | `nvarchar(500)` | `NULL` |
| `Tourism` | `nvarchar(500)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

---
### `SysConfigMenus`

**الاستخدام الوظيفي:** إعدادات النظام العامة أو إعدادات الوحدات والقوائم.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Parent_Id` | `bigint` | `NULL` |
| `KeysData` | `nvarchar(100)` | `NULL` |
| `Description` | `nvarchar(500)` | `NULL` |
| `PROJECT_ID` | `int` | `NULL` |
| `Value` | `nvarchar(500)` | `NULL` |
| `DefaultValue` | `nvarchar(500)` | `NULL` |
| `DefaultDescription` | `nvarchar(500)` | `NULL` |
| `POS` | `nvarchar(500)` | `NULL` |
| `Market` | `nvarchar(500)` | `NULL` |
| `Factory` | `nvarchar(500)` | `NULL` |
| `Tourism` | `nvarchar(500)` | `NULL` |
| `AlFawzan` | `nvarchar(500)` | `NULL` |
| `Client_A` | `nvarchar(500)` | `NULL` |
| `Client_B` | `nvarchar(500)` | `NULL` |
| `UserSells` | `nvarchar(50)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `PROJECT_ID` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`

---
### `SysConfigMenusAr`

**الاستخدام الوظيفي:** إعدادات النظام العامة أو إعدادات الوحدات والقوائم.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Parent_Id` | `int` | `NULL` |
| `KeysData` | `nvarchar(100)` | `NULL` |
| `Description` | `nvarchar(500)` | `NULL` |
| `PROJECT_ID` | `int` | `NULL` |
| `Value` | `nvarchar(500)` | `NULL` |
| `DefaultValue` | `nvarchar(500)` | `NULL` |
| `DefaultDescription` | `nvarchar(500)` | `NULL` |
| `POS` | `nvarchar(500)` | `NULL` |
| `Market` | `nvarchar(500)` | `NULL` |
| `Factory` | `nvarchar(500)` | `NULL` |
| `Tourism` | `nvarchar(500)` | `NULL` |
| `AlFawzan` | `nvarchar(500)` | `NULL` |
| `Client_A` | `nvarchar(500)` | `NULL` |
| `Client_B` | `nvarchar(500)` | `NULL` |
| `UserSells` | `nvarchar(50)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `PROJECT_ID` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`

---
### `SysConfigMenusPERMATIONS`

**الاستخدام الوظيفي:** إعدادات صلاحيات الوصول والتحكم في وظائف النظام.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Menu_ID` | `int` | `NOT NULL` |
| `Parent_Id` | `int` | `NULL` |
| `USER_ID` | `int` | `NULL` |
| `PERMATION` | `nvarchar(500)` | `NULL` |
| `PROJECT_ID` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Menu_ID` → محتمل: `SysConfigMenusPERMATIONS`, `SysConfigMenusAr`, `SysConfigMenus`
- `USER_ID` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `PROJECT_ID` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`

---
### `SysPrinterSettings`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `InvoicePrinterName` | `nvarchar(250)` | `NOT NULL` |
| `KitchenPrinterName` | `nvarchar(250)` | `NOT NULL` |
| `PaperWidth` | `int` | `NOT NULL` |
| `InvoiceWidth` | `int` | `NOT NULL` |
| `MarginLeft` | `real` | `NOT NULL` |
| `StartHeightY` | `real` | `NOT NULL` |
| `ShowPreview` | `bit` | `NOT NULL` |
| `PrintKitchenSlip` | `bit` | `NOT NULL` |
| `FontHeader` | `nvarchar(max)` | `NULL` |

---
### `USER_DETAILES`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `USER_ID` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `USER_NAME` | `nvarchar(50)` | `NOT NULL` |
| `USER_PASSWORD` | `nvarchar(50)` | `NOT NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `AdminFlage` | `int` | `NULL` |
| `Account_Proj` | `int` | `NULL` |
| `HR_Proj` | `int` | `NULL` |
| `Store_Proj` | `int` | `NULL` |
| `Production_Proj` | `int` | `NULL` |
| `Gawda_Proj` | `int` | `NULL` |
| `Balance_Air` | `decimal(18, 2)` | `NULL` |
| `Balance_Road` | `decimal(18, 2)` | `NULL` |
| `Active` | `bit` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `USER_ID` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `USER_DETAILES_`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `USER_ID` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `USER_NAME` | `nvarchar(50)` | `NOT NULL` |
| `USER_PASSWORD` | `nvarchar(50)` | `NOT NULL` |
| `Branch_Id` | `int` | `NULL` |
| `AdminFlage` | `int` | `NULL` |
| `Account_Proj` | `int` | `NULL` |
| `HR_Proj` | `int` | `NULL` |
| `Store_Proj` | `int` | `NULL` |
| `Production_Proj` | `int` | `NULL` |
| `Gawda_Proj` | `int` | `NULL` |
| `Balance_Air` | `decimal(18, 2)` | `NULL` |
| `Balance_Road` | `decimal(18, 2)` | `NULL` |
| `Active` | `bit` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `USER_ID` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `USER_DETAILES_Branches_Permission`

**الاستخدام الوظيفي:** إعدادات صلاحيات الوصول والتحكم في وظائف النظام.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `USER_DETAILES_Id` | `int` | `NULL` |
| `Account_Branches_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `USER_DETAILES_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Branches_Id` → محتمل: `Account_Branches`, `Account_BranchesTaxInfo`, `Account_BranchesimagesLogo`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---

## 13 — الجداول المرجعية Lookup
### `Lookup_CashPaymentMethod`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---
### `LookUp_CheckStatus`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `StatusName` | `nvarchar(50)` | `NULL` |
| `S_Value` | `int` | `NULL` |
| `Status` | `int` | `NULL` |
| `OrderBy` | `int` | `NULL` |
| `IsProcessUsedToChange` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Active` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `LookUp_CheckStatusBankAccount`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `BankId` | `int` | `NULL` |
| `LookUp_CheckStatusId` | `int` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `BankId` → محتمل: `Banks`, `Money_Transaction_Bank`, `LookUp_CheckStatusBankAccount`, `Banks_Balance`
- `LookUp_CheckStatusId` → محتمل: `LookUp_CheckStatusBankAccount`, `LookUp_CheckStatus`

---
### `LookUp_CodeTaxType`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---
### `Lookup_DiscountTypes`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NOT NULL` |
| `Name_En` | `nvarchar(50)` | `NOT NULL` |

---
### `Lookup_InvoiceKitchenStatus`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(500)` | `NOT NULL` |
| `Name_En` | `nvarchar(500)` | `NOT NULL` |

---
### `Lookup_IsInsuranceCompanyStaus`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---
### `LookUp_ItemServiceCate ry`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 7

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `ServiceCate ryName` | `nvarchar(500)` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `IsActive` | `int` | `NULL` |
| `OrderIndex` | `int` | `NULL` |
| `GenderTypeId` | `int` | `NULL` |
| `GenderTypeDescription` | `nvarchar(500)` | `NULL` |

---
### `LookUp_NonTaxableType`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Code` | `nvarchar(200)` | `NULL` |
| `NameAr` | `nvarchar(200)` | `NULL` |
| `NameEn` | `nvarchar(200)` | `NULL` |

---
### `LookUp_NonTaxableTypes`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Code` | `nvarchar(200)` | `NULL` |
| `NameAr` | `nvarchar(200)` | `NULL` |
| `NameEn` | `nvarchar(200)` | `NULL` |

---
### `Lookup_OrderDeliveryTypes`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(500)` | `NOT NULL` |
| `Name_En` | `nvarchar(500)` | `NOT NULL` |
| `Default_Service_Rate` | `decimal(18, 2)` | `NOT NULL` |
| `Default_Delivery_Fee` | `decimal(18, 2)` | `NOT NULL` |

---
### `LookUp_PersonCompany_Type`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---
### `Lookup_Shifts`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 5

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(500)` | `NOT NULL` |
| `Name_En` | `nvarchar(500)` | `NOT NULL` |
| `Start_Time` | `time(7)` | `NOT NULL` |
| `End_Time` | `time(7)` | `NOT NULL` |

---
### `LookUp_StoreOrderTypes`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 6

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(50)` | `NULL` |
| `Type` | `int` | `NULL` |
| `IsHaveSelectedId` | `int` | `NULL` |
| `TableName` | `nvarchar(50)` | `NULL` |
| `IsStoreMainType` | `int` | `NULL` |

---
### `Lookup_Tables`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NOT NULL` |
| `Name_En` | `nvarchar(50)` | `NOT NULL` |
| `Is_Active` | `bit` | `NOT NULL` |

---
### `LookUp_TagerKontra`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 3

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(100)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

---
### `LookUp_TaxableSubTypes`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 6

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Code` | `nvarchar(500)` | `NULL` |
| `DescriptionEn` | `nvarchar(500)` | `NULL` |
| `DescriptionAr` | `nvarchar(500)` | `NULL` |
| `TaxtypeReference` | `nvarchar(500)` | `NULL` |
| `LookUp_TaxableTypeId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `LookUp_TaxableTypeId` → محتمل: `LookUp_TaxableTypes`

---
### `LookUp_TaxableTypes`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Code` | `nvarchar(max)` | `NULL` |
| `DescriptionEn` | `nvarchar(max)` | `NULL` |
| `DescriptionAr` | `nvarchar(max)` | `NULL` |

---
### `Lookup_TransmissionType`

**الاستخدام الوظيفي:** جدول دليل/قائمة اختيار مرجعية تستخدم لتوحيد القيم في شاشات ومعاملات النظام.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(50)` | `NULL` |

---

## 14 — جداول مساندة ومتنوعة
### ` v_City_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `NameAr` | `nvarchar(100)` | `NULL` |
| `NameEn` | `nvarchar(100)` | `NULL` |
| `Code` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| ` v_Country_ListId` | `bigint` | `NULL` |
| ` vern_Area_ListId` | `bigint` | `NULL` |
| ` vern_ListId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- ` v_Country_ListId` → محتمل: ` v_Country_List`
- ` vern_Area_ListId` → محتمل: ` vern_Area_List`
- ` vern_ListId` → محتمل: ` vern_List`

---
### ` v_Country_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `NameAr` | `nvarchar(100)` | `NULL` |
| `NameEn` | `nvarchar(100)` | `NULL` |
| `Code` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### ` vern_Area`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| ` vern_Id` | `bigint` | `NULL` |
| `Area_Name` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- ` vern_Id` → محتمل: ` vern_Area`, ` vern_List`, ` vern_Area_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### ` vern_Area_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Area_Name` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| ` v_Country_ListId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- ` v_Country_ListId` → محتمل: ` v_Country_List`

---
### ` vern_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| ` v_Name` | `nvarchar(100)` | `NULL` |
| `Code` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| ` v_Country_ListId` | `bigint` | `NULL` |
| ` vern_Area_ListId` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- ` v_Country_ListId` → محتمل: ` v_Country_List`
- ` vern_Area_ListId` → محتمل: ` vern_Area_List`

---
### `ALogError`

**الاستخدام الوظيفي:** سجل أخطاء وتشخيصات تشغيلية/برمجية للنظام.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `mysql` | `nvarchar(1000)` | `NULL` |
| `myDate` | `datetime` | `NULL` |
| `message` | `nvarchar(1000)` | `NULL` |

---
### `ClinicUsed`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Name` | `nvarchar(255)` | `NULL` |
| `Is_Active` | `int` | `NULL` |
| `Id` | `bigint` | `NOT NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Customer`

**الاستخدام الوظيفي:** دليل العملاء وبياناتهم الأساسية والمالية والتشغيلية.

**عدد الأعمدة:** 76

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(60)` | `NULL` |
| `CompanyName` | `nvarchar(150)` | `NULL` |
| `Active` | `int` | `NULL` |
| `Tax_No` | `nvarchar(100)` | `NULL` |
| `Tel` | `nvarchar(25)` | `NULL` |
| `Tel1` | `nvarchar(25)` | `NULL` |
| `Mobile` | `nvarchar(25)` | `NULL` |
| `Address` | `nvarchar(250)` | `NULL` |
| `E_Mail` | `nvarchar(200)` | `NULL` |
| ` v_Country_ListName` | `nvarchar(20)` | `NULL` |
| ` v_City_ListName` | `nvarchar(40)` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| ` vern_ListName` | `nvarchar(100)` | `NULL` |
| `Street` | `nvarchar(100)` | `NULL` |
| `BuildingNumber` | `nvarchar(100)` | `NULL` |
| `floor` | `nvarchar(200)` | `NULL` |
| `room` | `nvarchar(200)` | `NULL` |
| `landmark` | `nvarchar(200)` | `NULL` |
| `additionalInformation` | `nvarchar(200)` | `NULL` |
| `LookUp_PersonCompany_Type` | `nvarchar(100)` | `NULL` |
| `LookUp_PersonCompany_TypeId` | `bigint` | `NULL` |
| `Balance` | `decimal(18, 2)` | `NULL` |
| `Balance_Actual` | `decimal(18, 2)` | `NULL` |
| `Code` | `nvarchar(200)` | `NULL` |
| `Tel2` | `nvarchar(50)` | `NULL` |
| `Fax` | `nvarchar(50)` | `NULL` |
| `Web_Site` | `nvarchar(50)` | `NULL` |
| `Customer_Type` | `int` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `IsSalesMan` | `int` | `NULL` |
| `SalesMan_Id` | `int` | `NULL` |
| `Date_Balance_First` | `datetime` | `NULL` |
| `Balance_First` | `decimal(18, 2)` | `NULL` |
| `Location_Id` | `int` | `NULL` |
| `Price_Type_List_ID` | `bigint` | `NULL` |
| `Balance_Air` | `decimal(18, 2)` | `NULL` |
| `Balance_Road` | `decimal(18, 2)` | `NULL` |
| `Account_Code` | `nvarchar(500)` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Static_Flage` | `int` | `NULL` |
| `General_Customer_Flage` | `int` | `NULL` |
| ` verner_Name` | `nvarchar(50)` | `NULL` |
| `Group_Id` | `bigint` | `NULL` |
| `Ref_Cust_Supplier_Id` | `bigint` | `NULL` |
| `VisaNo` | `nvarchar(50)` | `NULL` |
| `VisaPassword` | `nvarchar(50)` | `NULL` |
| `CustomerNotes` | `nvarchar(500)` | `NULL` |
| `Commercial_Register` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ContactPerson` | `nvarchar(500)` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `CompanyContactName` | `nvarchar(200)` | `NULL` |
| ` vern_Area_ListName` | `nvarchar(200)` | `NULL` |
| `CompanyContactData` | `nvarchar(200)` | `NULL` |
| `NationalId` | `nvarchar(200)` | `NULL` |
| `PassportNo` | `nvarchar(200)` | `NULL` |
| `PostalNo` | `nvarchar(200)` | `NULL` |
| `RegistrationDate` | `datetime` | `NULL` |
| ` v_Country_ListId` | `bigint` | `NULL` |
| ` v_City_ListId` | `bigint` | `NULL` |
| ` vern_Area_ListId` | `bigint` | `NULL` |
| ` vern_ListId` | `bigint` | `NULL` |
| `Tax_BranchesId` | `bigint` | `NULL` |
| `General_Customer_FlageBackup` | `int` | `NULL` |
| `IsRelated` | `int` | `NULL` |
| `LookUp_PersonCompany_TypeCodeTax` | `nvarchar(50)` | `NULL` |
| `TaxCompanyName` | `nvarchar(500)` | `NULL` |
| `temp` | `int` | `NULL` |
| `IsInsuranceCompany` | `int` | `NULL` |
| `Customer_Id_RelatedToAccount` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `LookUp_PersonCompany_TypeId` → محتمل: `LookUp_PersonCompany_Type`
- `Location_Id` → محتمل: `LookUp_CarStatusLocation`
- `Price_Type_List_ID` → محتمل: `Price_Type_List`
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- ` v_Country_ListId` → محتمل: ` v_Country_List`
- ` v_City_ListId` → محتمل: ` v_City_List`
- ` vern_Area_ListId` → محتمل: ` vern_Area_List`
- ` vern_ListId` → محتمل: ` vern_List`
- `Tax_BranchesId` → محتمل: `Tax_Branches`, `Tax_BranchesConfig`

---
### `Customer_Price`

**الاستخدام الوظيفي:** أسعار أو قواعد تسعير مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 13

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Store_id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Customer_Receive`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `customer_id` | `bigint` | `NULL` |
| `descreption` | `nvarchar(500)` | `NULL` |
| `dt_date` | `datetime` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Customer_Supplier_Group`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 11

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `GroupName` | `nvarchar(500)` | `NULL` |
| `GroupType` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Customers_Accounts`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 47

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Import` | `decimal(18, 2)` | `NULL` |
| `Export` | `decimal(18, 2)` | `NULL` |
| `Balance` | `decimal(18, 2)` | `NULL` |
| `Balance_Actual` | `decimal(18, 2)` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Transaction_Date` | `datetime` | `NULL` |
| `Reference_Id` | `bigint` | `NULL` |
| `Reference_Type` | `int` | `NULL` |
| `Reference_Type_Table_Id` | `bigint` | `NULL` |
| `Payment_Type` | `int` | `NULL` |
| `Notes` | `nvarchar(4000)` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `Flage_View` | `int` | `NULL` |
| `BalanceImport` | `decimal(18, 2)` | `NULL` |
| `BalanceExport` | `decimal(18, 2)` | `NULL` |
| `BalanceDiff` | `nvarchar(100)` | `NULL` |
| `Reference_Type_View` | `nvarchar(100)` | `NULL` |
| `Item_Name` | `nvarchar(100)` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Larg_Qnty` | `nvarchar(100)` | `NULL` |
| `Small_Qnty` | `nvarchar(100)` | `NULL` |
| `Item_No` | `nvarchar(100)` | `NULL` |
| `Item_Size` | `nvarchar(100)` | `NULL` |
| `Qnty_Size` | `nvarchar(100)` | `NULL` |
| `Qnty_value` | `nvarchar(100)` | `NULL` |
| `Height_Width_Item` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Reference_Type_TableDetails_Id` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CustomerName` | `nvarchar(500)` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `TaxInternalId` | `nvarchar(500)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `Customer_Id_` | `bigint` | `NULL` |
| `Customer_Id_RelatedToAccount` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Customers_Accounts_Mokhtalat`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 40

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Customers_AccountsId` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Import` | `decimal(18, 2)` | `NULL` |
| `Export` | `decimal(18, 2)` | `NULL` |
| `Balance` | `decimal(18, 2)` | `NULL` |
| `Balance_Actual` | `decimal(18, 2)` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Transaction_Date` | `datetime` | `NULL` |
| `Reference_Id` | `bigint` | `NULL` |
| `Reference_Type` | `int` | `NULL` |
| `Reference_Type_Table_Id` | `bigint` | `NULL` |
| `Reference_Type_TableDetails_Id` | `bigint` | `NULL` |
| `Payment_Type` | `int` | `NULL` |
| `Notes` | `nvarchar(4000)` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `Flage_View` | `int` | `NULL` |
| `BalanceImport` | `nvarchar(100)` | `NULL` |
| `BalanceExport` | `nvarchar(100)` | `NULL` |
| `Reference_Type_View` | `nvarchar(100)` | `NULL` |
| `Item_Name` | `nvarchar(100)` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Larg_Qnty` | `nvarchar(100)` | `NULL` |
| `Small_Qnty` | `nvarchar(100)` | `NULL` |
| `Item_No` | `nvarchar(100)` | `NULL` |
| `Item_Size` | `nvarchar(100)` | `NULL` |
| `Qnty_Size` | `nvarchar(100)` | `NULL` |
| `Qnty_value` | `nvarchar(100)` | `NULL` |
| `Height_Width_Item` | `nvarchar(100)` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `BalanceDiff` | `decimal(18, 2)` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CustomerName` | `nvarchar(500)` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `TaxInternalId` | `nvarchar(500)` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |
| `Customer_Id_RelatedToAccount` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Customers_AccountsId` → محتمل: `Customers_Accounts_Mokhtalat`, `Customers_Accounts`

---
### `Customers_Destruction`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 19

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Customer_id` | `bigint` | `NULL` |
| `Customer_name` | `nvarchar(100)` | `NULL` |
| `small_quantity` | `decimal(18, 2)` | `NULL` |
| `large_quantity` | `decimal(18, 2)` | `NULL` |
| `price` | `decimal(18, 2)` | `NULL` |
| `total` | `decimal(18, 2)` | `NULL` |
| `dt_date` | `datetime` | `NULL` |
| `Account_ID` | `int` | `NULL` |
| `Unit_Item_Table_Id` | `int` | `NULL` |
| `Notes` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Day_Registration_Details`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 27

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Day_Reg_Id` | `bigint` | `NULL` |
| `SubAccount_Id` | `nvarchar(50)` | `NULL` |
| `SubAccount_NO` | `nvarchar(50)` | `NULL` |
| `SubAccount_Name` | `nvarchar(50)` | `NULL` |
| `Main_Account_Name` | `nvarchar(50)` | `NULL` |
| `Data_View` | `nvarchar(50)` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `TotalForAccount` | `nvarchar(50)` | `NULL` |
| `Import_Total` | `nvarchar(50)` | `NULL` |
| `Export_Total` | `nvarchar(50)` | `NULL` |
| `Bank_Name` | `nvarchar(50)` | `NULL` |
| `Transaction_type` | `int` | `NULL` |
| `Cust_Sup_Name` | `nvarchar(50)` | `NULL` |
| `process_Date` | `nvarchar(50)` | `NULL` |
| `DocNo` | `nvarchar(50)` | `NULL` |
| `Main_Account_ID` | `nvarchar(50)` | `NULL` |
| `Reg_Diff` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Day_Reg_Id` → محتمل: `Day_Registration_List`, `Day_Registration_Details`
- `Main_Account_ID` → محتمل: `Main_Accounts`, `Submain_Account`, `MainAccount`, `Account_Type_Main_Account_Id`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Day_Registration_List`

**الاستخدام الوظيفي:** رأس/قائمة سجلات للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Data_View` | `nvarchar(100)` | `NULL` |
| `Day_Date` | `datetime` | `NULL` |
| `DateFrom` | `datetime` | `NULL` |
| `DateTo` | `datetime` | `NULL` |
| `Notes` | `nvarchar(50)` | `NULL` |
| `Saved_Type` | `int` | `NULL` |
| `Day_Reg_No` | `nvarchar(50)` | `NULL` |
| `Balance_Transmit` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Equivalent_Machine_Problem`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Equivalent_Date` | `datetime` | `NULL` |
| `Equivalent_Value` | `decimal(18, 2)` | `NULL` |
| `Reason` | `nvarchar(4000)` | `NULL` |
| `Done` | `int` | `NULL` |
| `Type` | `int` | `NULL` |
| `Hour_Count` | `decimal(18, 2)` | `NULL` |
| `Price` | `decimal(18, 2)` | `NULL` |
| `Hadafat_No` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Fees`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Fee_Date` | `smalldatetime` | `NULL` |
| `Fee_Value` | `decimal(18, 2)` | `NULL` |
| `Reason` | `nvarchar(1000)` | `NULL` |
| `Done` | `bit` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `FixedAssetData`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 37

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Code` | `nvarchar(50)` | `NULL` |
| `Name_Ar` | `nvarchar(50)` | `NULL` |
| `Name_En` | `nvarchar(50)` | `NULL` |
| `Purchase_Date` | `datetime` | `NULL` |
| `Account_Ehlak_Status` | `int` | `NULL` |
| `Account_Ehlak_Status_Str` | `nvarchar(50)` | `NULL` |
| `Emp_Id` | `int` | `NULL` |
| `Emp_Receive_Date` | `datetime` | `NULL` |
| `Model` | `nvarchar(50)` | `NULL` |
| `Industry_Factory` | `nvarchar(50)` | `NULL` |
| `Fixed_Serial_No` | `nvarchar(50)` | `NULL` |
| `Location` | `nvarchar(50)` | `NULL` |
| `Factory_warrantyFinal_Date` | `datetime` | `NULL` |
| `Supplier_Name` | `nvarchar(50)` | `NULL` |
| `Factory_warranty_Details` | `nvarchar(50)` | `NULL` |
| `Account_Ehlak_Type` | `int` | `NULL` |
| `Account_Ehlak_Type_Str` | `nvarchar(50)` | `NULL` |
| `Main_Value` | `decimal(18, 2)` | `NULL` |
| `Accumulated_Depreciation_Value` | `decimal(18, 2)` | `NULL` |
| `Rate_of_depreciation` | `decimal(18, 2)` | `NULL` |
| `Book_value` | `decimal(18, 2)` | `NULL` |
| `beginning_service_Date` | `datetime` | `NULL` |
| `Price_scrap_Value` | `decimal(18, 2)` | `NULL` |
| `Last_Modified_Value` | `decimal(18, 2)` | `NULL` |
| `Default_Age` | `decimal(18, 2)` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Account_Code` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `Active` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `FixedAssetData_ImagesStore`

**الاستخدام الوظيفي:** تخزين أو فهرسة صور مرتبطة بالكيان المقابل.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ImageId` | `bigint` | `NOT NULL` |
| `FixedAssetData_Id` | `int` | `NULL` |
| `Description` | `nvarchar(500)` | `NULL` |
| `OriginalPath` | `nvarchar(500)` | `NULL` |
| `ImageData` | `image` | `NULL` |
| `Default_Img_Flage` | `bit` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `FixedAssetData_Id` → محتمل: `FixedAssetData_ImagesStore`, `FixedAssetData`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Foes`

**الاستخدام الوظيفي:** جدول باسم تاريخي/مختصر؛ الغرض الدقيق يحتاج تأكيدًا من كود التطبيق أو شاشة الاستخدام.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Foes_Date` | `smalldatetime` | `NULL` |
| `Foe_Value` | `decimal(18, 2)` | `NULL` |
| `Reason` | `nvarchar(1000)` | `NULL` |
| `Done` | `bit` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `jobslist`

**الاستخدام الوظيفي:** دليل الوظائف/المهن المستخدمة في بيانات الموظفين والعمليات.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `jobname` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `License_Days`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Emp_ID` | `bigint` | `NULL` |
| `Day_Date_From` | `smalldatetime` | `NULL` |
| `Day_Date_to` | `smalldatetime` | `NULL` |
| `Days_NO` | `bigint` | `NULL` |
| `Days_Value` | `decimal(18, 2)` | `NULL` |
| `Reason` | `nvarchar(50)` | `NULL` |
| `Balance` | `bigint` | `NULL` |
| `Done` | `bit` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Maintaining_Operations`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Machine_Id` | `int` | `NULL` |
| `Maintaining_Item_List_Id` | `int` | `NULL` |
| `Man_Name` | `nchar(10)` | `NULL` |
| `Description` | `nvarchar(500)` | `NULL` |
| `Maintaining_Date` | `datetime` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Machine_Id` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`
- `Maintaining_Item_List_Id` → محتمل: `Maintaining_Item_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `More_Account_Data`

**الاستخدام الوظيفي:** بيانات إضافية للحسابات.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(100)` | `NULL` |
| `Account_Id` | `bigint` | `NULL` |
| `Balance` | `decimal(18, 2)` | `NULL` |
| `Address` | `nvarchar(200)` | `NULL` |
| `Description` | `nvarchar(4000)` | `NULL` |
| `Notes` | `nvarchar(4000)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `More_Account_Registration`

**الاستخدام الوظيفي:** بيانات تسجيل/حركة إضافية مرتبطة بالحسابات.

**عدد الأعمدة:** 27

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `DayDate` | `datetime` | `NULL` |
| `Account_ID_From` | `bigint` | `NULL` |
| `Account_ID_SUB_From` | `bigint` | `NULL` |
| `Account_ID_TO` | `bigint` | `NULL` |
| `Account_ID_SUB_To` | `bigint` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `Transaction_Value` | `decimal(18, 2)` | `NULL` |
| `Account_MainID_From` | `bigint` | `NULL` |
| `Account_MainID_To` | `bigint` | `NULL` |
| `Day_Registration_flage` | `int` | `NULL` |
| `DayReg_List_Id` | `int` | `NULL` |
| `Credit_No` | `bigint` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Doc_No` | `nvarchar(50)` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `FirstBalanceFlage` | `int` | `NULL` |
| `SimpleFlage` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `More_Account_RegistrationTemp`

**الاستخدام الوظيفي:** نسخة مؤقتة لمعالجة بيانات تسجيل الحسابات.

**عدد الأعمدة:** 25

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `DayDate` | `datetime` | `NULL` |
| `Account_ID_From` | `bigint` | `NULL` |
| `Account_ID_SUB_From` | `bigint` | `NULL` |
| `Account_ID_TO` | `bigint` | `NULL` |
| `Account_ID_SUB_To` | `bigint` | `NULL` |
| `Import_Value` | `decimal(18, 2)` | `NULL` |
| `Export_Value` | `decimal(18, 2)` | `NULL` |
| `Transaction_Value` | `decimal(18, 2)` | `NULL` |
| `Account_MainID_From` | `bigint` | `NULL` |
| `Account_MainID_To` | `bigint` | `NULL` |
| `Day_Registration_flage` | `int` | `NULL` |
| `DayReg_List_Id` | `int` | `NULL` |
| `Credit_No` | `bigint` | `NULL` |
| `Notes` | `nvarchar(500)` | `NULL` |
| `Doc_No` | `nvarchar(50)` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `OrderTypes_Label`

**الاستخدام الوظيفي:** دليل أنواع وتصنيفات يستخدم كبيانات مرجعية.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `TypesID` | `bigint` | `NOT NULL` |
| `LabelFrom` | `nvarchar(50)` | `NULL` |
| `LabelTo` | `nvarchar(50)` | `NULL` |
| `Label_Name` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `PERMATIONS`

**الاستخدام الوظيفي:** إعدادات صلاحيات الوصول والتحكم في وظائف النظام.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `USER_ID` | `bigint` | `NULL` |
| `FORM_ID` | `bigint` | `NULL` |
| `PERMATION` | `int` | `NOT NULL` |
| `PROJECT_ID` | `bigint` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `USER_ID` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `FORM_ID` → محتمل: `FORMS_DETAILES`
- `PROJECT_ID` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`

---
### `Price_Purhase_Type_List`

**الاستخدام الوظيفي:** أسعار أو قواعد تسعير مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 12

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Price_Name` | `nvarchar(100)` | `NULL` |
| `IS_Active` | `int` | `NULL` |
| `Price_Type_List_Master_Id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Price_Type_List`

**الاستخدام الوظيفي:** أسعار أو قواعد تسعير مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Price_Name` | `nvarchar(50)` | `NULL` |
| `IS_Active` | `int` | `NULL` |
| `Price_Name_Master` | `nvarchar(50)` | `NULL` |
| `Price_Type_List_Master_Id` | `int` | `NULL` |
| `Plus_Minus` | `nchar(50)` | `NULL` |
| `Added_Value` | `decimal(18, 0)` | `NULL` |
| `IS_Rate` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Price_Type_Sell_Values`

**الاستخدام الوظيفي:** أسعار أو قواعد تسعير مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 16

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NOT NULL` |
| `Item_ID` | `bigint` | `NULL` |
| `Store_ID` | `bigint` | `NULL` |
| `Price_Type_List_ID` | `bigint` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `CommisionRat` | `decimal(18, 2)` | `NULL` |
| `CommisionValue` | `decimal(18, 2)` | `NULL` |
| `Is_CommisionRat` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Price_Type_List_ID` → محتمل: `Price_Type_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `PROJECT_DETAILES`

**الاستخدام الوظيفي:** جدول تفاصيل/سطور للكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 10

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `PROJECT_ID` | `int` | `NOT NULL` |
| `PROJECT_NAME` | `char(20)` | `NOT NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `PROJECT_ID` → محتمل: `ConsProject`, `ConsProjectUnites`, `ConsProjectSpacesGroup`, `PROJECT_DETAILES`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `ReportSellPurchaseItems`

**الاستخدام الوظيفي:** بيانات مساعدة أو مُجمعة لإعداد التقارير.

**عدد الأعمدة:** 26

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Invoices_List_Id` | `bigint` | `NULL` |
| `Invoice_Date` | `datetime` | `NULL` |
| `Item_Name` | `nvarchar(500)` | `NULL` |
| `Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Qnty_Another` | `decimal(18, 2)` | `NULL` |
| `Customer_Id` | `bigint` | `NULL` |
| `Customer_Name` | `nvarchar(500)` | `NULL` |
| `actual_S_price` | `decimal(18, 2)` | `NULL` |
| `Total_SellPrice` | `decimal(18, 2)` | `NULL` |
| `actual_P_price` | `decimal(18, 2)` | `NULL` |
| `Total_PurhasePrice` | `decimal(18, 2)` | `NULL` |
| `Diff_Total` | `decimal(18, 2)` | `NULL` |
| `Invoice_Type` | `int` | `NULL` |
| `Invoice_Id_No` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Requests_Details`

**الاستخدام الوظيفي:** تفاصيل الطلبات/الطلبات التشغيلية.

**عدد الأعمدة:** 20

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Customer_Id` | `int` | `NULL` |
| `Request_Date` | `datetime` | `NULL` |
| `Unit_Item_Table_Id` | `bigint` | `NULL` |
| `Request_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Request_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Request_Item_NO` | `decimal(18, 2)` | `NULL` |
| `Export_Qnty_Larg` | `decimal(18, 2)` | `NULL` |
| `Export_Qnty_Small` | `decimal(18, 2)` | `NULL` |
| `Export_Item_No` | `decimal(18, 2)` | `NULL` |
| `Item_Exported_Finish` | `int` | `NULL` |
| `Money_Transaction_Box_Id` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Money_Transaction_Box_Id` → محتمل: `Money_Transaction_BoxForInvoices`, `Money_Transaction_Box`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `ShipmentCompany`

**الاستخدام الوظيفي:** دليل شركات الشحن.

**عدد الأعمدة:** 2

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Name` | `nvarchar(500)` | `NULL` |

---
### `Special_Added_Days`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 17

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Emp_Id` | `bigint` | `NULL` |
| `Value_Date` | `datetime` | `NULL` |
| `Value_Mony` | `decimal(18, 2)` | `NULL` |
| `value_Time` | `decimal(18, 2)` | `NULL` |
| `value_Time_Text` | `nvarchar(100)` | `NULL` |
| `Notes` | `nvarchar(1000)` | `NULL` |
| `Done` | `int` | `NULL` |
| `New_Hour_Value` | `decimal(18, 2)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `SubMainAssets_Ehlak`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 18

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Assets_Account_ID` | `int` | `NULL` |
| `DateFrom` | `datetime` | `NULL` |
| `DateTo` | `datetime` | `NULL` |
| `Previous_Ehlak` | `decimal(18, 2)` | `NULL` |
| `Period_Ehalk` | `decimal(18, 2)` | `NULL` |
| `Total_Eklak` | `decimal(18, 2)` | `NULL` |
| `Asset_Value_Now` | `decimal(18, 2)` | `NULL` |
| `Asset_Value_Befor_Period_Ehalk` | `decimal(18, 2)` | `NULL` |
| `First_Period_Flage` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Supplier`

**الاستخدام الوظيفي:** دليل الموردين وبياناتهم الأساسية والمالية.

**عدد الأعمدة:** 68

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NULL` |
| `Name` | `nvarchar(500)` | `NULL` |
| `Tax_Registration_No` | `nvarchar(500)` | `NULL` |
| `Commercial_Register` | `nvarchar(500)` | `NULL` |
| `Active` | `int` | `NULL` |
| `Tel` | `nvarchar(500)` | `NULL` |
| `Tel2` | `nvarchar(500)` | `NULL` |
| `Mobile` | `nvarchar(500)` | `NULL` |
| `Fax` | `nvarchar(500)` | `NULL` |
| `E_Mail` | `nvarchar(500)` | `NULL` |
| `Address` | `nvarchar(500)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `Web_Site` | `nvarchar(500)` | `NULL` |
| `Balance` | `decimal(18, 2)` | `NULL` |
| `Balance_Actual` | `decimal(18, 2)` | `NULL` |
| `Supplier_Type` | `int` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Date_Balance_First` | `datetime` | `NULL` |
| `Balance_First` | `decimal(18, 2)` | `NULL` |
| `Balance_Air` | `decimal(18, 2)` | `NULL` |
| `Balance_Road` | `decimal(18, 2)` | `NULL` |
| `Account_Code` | `nvarchar(500)` | `NULL` |
| `Currency_Id` | `int` | `NULL` |
| `Customer_Type` | `int` | `NULL` |
| `Group_Id` | `bigint` | `NULL` |
| `Ref_Cust_Supplier_Id` | `bigint` | `NULL` |
| ` vern_Area_Id` | `int` | `NULL` |
| ` vern_Id` | `bigint` | `NULL` |
| ` vern_Area_Name` | `nvarchar(500)` | `NULL` |
| ` vern_Name` | `nvarchar(500)` | `NULL` |
| `OperationDate1` | `datetime` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `CompanyName` | `nvarchar(500)` | `NULL` |
| `CompanyContactName` | `nvarchar(500)` | `NULL` |
| `ContactPerson` | `nvarchar(500)` | `NULL` |
| `Cust_ID` | `int` | `NULL` |
| `Cust_Type_ID` | `int` | `NULL` |
| `Cust_Account_No` | `int` | `NULL` |
| `Cust_File_Type` | `nvarchar(5)` | `NULL` |
| `Cust_Type` | `nvarchar(100)` | `NULL` |
| ` v_Country_ListName` | `nvarchar(20)` | `NULL` |
| ` v_City_ListName` | `nvarchar(40)` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| ` vern_ListName` | `nvarchar(500)` | `NULL` |
| `Street` | `nvarchar(500)` | `NULL` |
| `BuildingNumber` | `nvarchar(500)` | `NULL` |
| `floor` | `nvarchar(500)` | `NULL` |
| `room` | `nvarchar(500)` | `NULL` |
| `landmark` | `nvarchar(500)` | `NULL` |
| `additionalInformation` | `nvarchar(500)` | `NULL` |
| `NationalId` | `nvarchar(500)` | `NULL` |
| `PassportNo` | `nvarchar(500)` | `NULL` |
| `PostalNo` | `nvarchar(500)` | `NULL` |
| `RegistrationDate` | `datetime` | `NULL` |
| ` v_Country_ListId` | `bigint` | `NULL` |
| ` v_City_ListId` | `bigint` | `NULL` |
| ` vern_Area_ListId` | `bigint` | `NULL` |
| ` vern_ListId` | `bigint` | `NULL` |
| `LookUp_PersonCompany_TypeId` | `bigint` | `NULL` |
| `LookUp_PersonCompany_Type` | `nvarchar(500)` | `NULL` |
| `Code` | `nvarchar(500)` | `NULL` |
| `CompanyContactData` | `nvarchar(500)` | `NULL` |
| `IsRelated` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Currency_Id` → محتمل: `Currency`, `Currency_Factor`
- ` vern_Area_Id` → محتمل: ` vern_Area`, ` vern_Area_List`
- ` vern_Id` → محتمل: ` vern_Area`, ` vern_List`, ` vern_Area_List`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- ` v_Country_ListId` → محتمل: ` v_Country_List`
- ` v_City_ListId` → محتمل: ` v_City_List`
- ` vern_Area_ListId` → محتمل: ` vern_Area_List`
- ` vern_ListId` → محتمل: ` vern_List`
- `LookUp_PersonCompany_TypeId` → محتمل: `LookUp_PersonCompany_Type`

---
### `Supplier_Price`

**الاستخدام الوظيفي:** أسعار أو قواعد تسعير مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `PriceUnit` | `decimal(18, 2)` | `NULL` |
| `Store_id` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `Price_PurchaseType` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Suppliers_Accounts`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 46

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `Supplier_Id` | `bigint` | `NULL` |
| `Import` | `decimal(18, 2)` | `NULL` |
| `Export` | `decimal(18, 2)` | `NULL` |
| `Balance` | `decimal(18, 2)` | `NULL` |
| `Balance_Actual` | `decimal(18, 2)` | `NULL` |
| `Transaction_Type` | `int` | `NULL` |
| `Transaction_Date` | `datetime` | `NULL` |
| `Reference_Id` | `bigint` | `NULL` |
| `Reference_Type` | `int` | `NULL` |
| `Reference_Type_Table_Id` | `bigint` | `NULL` |
| `Payment_Type` | `int` | `NULL` |
| `Notes` | `nvarchar(4000)` | `NULL` |
| `Day_Registration_flage` | `int` | `NOT NULL` |
| `DayReg_List_Id` | `bigint` | `NULL` |
| `Flage_View` | `int` | `NULL` |
| `BalanceImport` | `decimal(18, 2)` | `NULL` |
| `BalanceExport` | `decimal(18, 2)` | `NULL` |
| `BalanceDiff` | `decimal(18, 2)` | `NULL` |
| `Reference_Type_View` | `nvarchar(100)` | `NULL` |
| `Item_Name` | `nvarchar(100)` | `NULL` |
| `Qnty` | `nvarchar(100)` | `NULL` |
| `Item_No` | `nvarchar(100)` | `NULL` |
| `Item_Size` | `nvarchar(100)` | `NULL` |
| `Qnty_Size` | `nvarchar(100)` | `NULL` |
| `Qnty_value` | `nvarchar(100)` | `NULL` |
| `Height_Width_Item` | `nvarchar(100)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `Larg_Unit_Price` | `decimal(18, 2)` | `NULL` |
| `Item_Id` | `bigint` | `NULL` |
| `Larg_Qnty` | `nvarchar(100)` | `NULL` |
| `Small_Qnty` | `nvarchar(100)` | `NULL` |
| `Reference_Type_TableDetails_Id` | `bigint` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TaxInternalId` | `nvarchar(500)` | `NULL` |
| `SupplierName` | `nvarchar(500)` | `NULL` |
| `WorkOrderNumber` | `bigint` | `NULL` |
| `CarsWorkOrdersId` | `bigint` | `NULL` |
| `CarsId` | `bigint` | `NULL` |
| `CarNumber` | `nvarchar(500)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Suppliers_Destruction`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 20

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `supplier_name` | `varchar(50)` | `NULL` |
| `quantity` | `decimal(18, 2)` | `NULL` |
| `price` | `decimal(18, 2)` | `NULL` |
| `total` | `decimal(18, 2)` | `NULL` |
| `dt_date` | `datetime` | `NULL` |
| `supplier_id` | `bigint` | `NULL` |
| `Unit_Item_Table_Id` | `int` | `NULL` |
| `Notes` | `nvarchar(50)` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `SubAccount_Id` | `bigint` | `NULL` |
| `Account_Entry_List_ID` | `bigint` | `NULL` |
| `IS_Posted_Flage` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`
- `Account_Entry_List_ID` → محتمل: `Account_Entry_List`

---
### `Table_ColWillChange`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 4

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `TableName` | `nvarchar(255)` | `NULL` |
| `ColName` | `nvarchar(255)` | `NULL` |
| `DataType` | `nvarchar(255)` | `NULL` |
| `IsIdentity` | `nvarchar(255)` | `NULL` |

---
### `Tax_Branches`

**الاستخدام الوظيفي:** بيانات/إعدادات/حركات ضريبية مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 34

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `Account_BranchesId` | `bigint` | `NULL` |
| `Code` | `nvarchar(500)` | `NULL` |
| `Name` | `nvarchar(500)` | `NULL` |
| `TaxVatNumber` | `nvarchar(500)` | `NULL` |
| `TaxRegistrationNumber` | `nvarchar(500)` | `NULL` |
| `Commercial_Register` | `nchar(10)` | `NULL` |
| `Address` | `nvarchar(500)` | `NULL` |
| `BranchID` | `nvarchar(500)` | `NULL` |
| `Country` | `nvarchar(50)` | `NULL` |
| ` vernate` | `nvarchar(500)` | `NULL` |
| `RegionCity` | `nvarchar(500)` | `NULL` |
| `Street` | `nvarchar(500)` | `NULL` |
| `buildingNumber` | `nvarchar(500)` | `NULL` |
| `postalCode` | `nvarchar(500)` | `NULL` |
| `room` | `nvarchar(500)` | `NULL` |
| `floor` | `nvarchar(500)` | `NULL` |
| `lastuuid` | `nvarchar(500)` | `NULL` |
| `landmark` | `nvarchar(500)` | `NULL` |
| `additionalInformation` | `nvarchar(500)` | `NULL` |
| `Phone1` | `nvarchar(500)` | `NULL` |
| `Phone2` | `nvarchar(500)` | `NULL` |
| `Phone3` | `nvarchar(500)` | `NULL` |
| `Fax` | `nvarchar(500)` | `NULL` |
| `Email` | `nvarchar(500)` | `NULL` |
| `BankName` | `nvarchar(500)` | `NULL` |
| `AccountNo` | `nvarchar(500)` | `NULL` |
| `IBANNumber` | `nvarchar(500)` | `NULL` |
| `BankAddress` | `nvarchar(500)` | `NULL` |
| `Active` | `int` | `NULL` |
| `Lo ` | `image` | `NULL` |
| `User_Id` | `bigint` | `NULL` |
| `Branch_Id` | `bigint` | `NULL` |
| `IsDefault` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Account_BranchesId` → محتمل: `Account_Branches`, `Account_BranchesTaxInfo`, `Account_BranchesimagesLogo`
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Tax_BranchesConfig`

**الاستخدام الوظيفي:** بيانات/إعدادات/حركات ضريبية مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 31

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Tax_BranchesId` | `bigint` | `NOT NULL` |
| `Account_BranchesId` | `bigint` | `NULL` |
| `ClientId` | `nvarchar(500)` | `NULL` |
| `ClientSecret` | `nvarchar(500)` | `NULL` |
| `ActivityCode` | `nvarchar(500)` | `NULL` |
| `TokenPin` | `nvarchar(500)` | `NULL` |
| `IssuerId` | `nvarchar(500)` | `NULL` |
| `IssuerName` | `nvarchar(500)` | `NULL` |
| `IssuerType` | `nvarchar(50)` | `NULL` |
| `documentTypeVersion` | `nvarchar(500)` | `NULL` |
| `RowCode` | `nvarchar(500)` | `NULL` |
| `dataBaseType` | `nvarchar(500)` | `NULL` |
| `ConnectionString` | `nvarchar(500)` | `NULL` |
| `IsSign` | `int` | `NULL` |
| `testtype` | `nvarchar(500)` | `NULL` |
| `databasename` | `nvarchar(500)` | `NULL` |
| `ClientIdPOS` | `nvarchar(500)` | `NULL` |
| `ClientSecretPOS` | `nvarchar(500)` | `NULL` |
| `SerialDevice` | `nvarchar(500)` | `NULL` |
| `SystemVersion` | `nvarchar(500)` | `NULL` |
| `documentTypeVersionPOS` | `nvarchar(500)` | `NULL` |
| `PortalUrl` | `nvarchar(500)` | `NULL` |
| `TokenUrl` | `nvarchar(500)` | `NULL` |
| `SubmitUrl` | `nvarchar(500)` | `NULL` |
| `CancelUrl` | `nvarchar(500)` | `NULL` |
| `Host` | `nvarchar(500)` | `NULL` |
| `RowStatus` | `int` | `NULL` |
| `RegistrationTaxFlag` | `int` | `NULL` |
| `TaxInvoiceStatus` | `nvarchar(500)` | `NULL` |
| `IsDefault` | `int` | `NULL` |
| `portalType` | `nvarchar(500)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `Tax_BranchesId` → محتمل: `Tax_Branches`, `Tax_BranchesConfig`
- `Account_BranchesId` → محتمل: `Account_Branches`, `Account_BranchesTaxInfo`, `Account_BranchesimagesLogo`

---
### `Taxdocuments`

**الاستخدام الوظيفي:** بيانات/إعدادات/حركات ضريبية مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `TaxInvoices_ListId` | `bigint` | `NULL` |
| `invoices_ListId` | `bigint` | `NULL` |
| `internalId` | `nvarchar(max)` | `NULL` |
| `submissionId` | `nvarchar(max)` | `NULL` |
| `uuid` | `nvarchar(max)` | `NULL` |
| `hashKey` | `nvarchar(max)` | `NULL` |
| `longId` | `nvarchar(max)` | `NULL` |
| `Errors` | `nvarchar(max)` | `NULL` |
| `IsSuccessded` | `int` | `NULL` |
| `DateOfSent` | `datetime` | `NULL` |
| `StausStr` | `nvarchar(50)` | `NULL` |
| `Staus` | `int` | `NULL` |
| `TaxInvoiceNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`

---
### `TaxdocumentsReverse`

**الاستخدام الوظيفي:** بيانات/إعدادات/حركات ضريبية مرتبطة بالكيان المحدد في اسم الجدول.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `TaxInvoices_ListId` | `bigint` | `NULL` |
| `invoices_ListId` | `bigint` | `NULL` |
| `internalId` | `nvarchar(max)` | `NULL` |
| `submissionId` | `nvarchar(max)` | `NULL` |
| `uuid` | `nvarchar(max)` | `NULL` |
| `hashKey` | `nvarchar(max)` | `NULL` |
| `longId` | `nvarchar(max)` | `NULL` |
| `Errors` | `nvarchar(max)` | `NULL` |
| `IsSuccessded` | `int` | `NULL` |
| `DateOfSent` | `datetime` | `NULL` |
| `StausStr` | `nvarchar(50)` | `NULL` |
| `Staus` | `int` | `NULL` |
| `TaxInvoiceNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `TaxInvoices_ListId` → محتمل: `TaxInvoices_List`

---
### `tblCompany`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 14

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `CompanyID` | `int` | `NULL` |
| `CompanyName` | `nvarchar(250)` | `NULL` |
| `CompanyName_Ar` | `nvarchar(250)` | `NULL` |
| `StreetAddress` | `nvarchar(250)` | `NULL` |
| `StreetAddress_Ar` | `nvarchar(250)` | `NULL` |
| `SloganText` | `nvarchar(250)` | `NULL` |
| `Hotline1` | `nvarchar(50)` | `NULL` |
| `Hotline2` | `nvarchar(50)` | `NULL` |
| `Email` | `nvarchar(50)` | `NULL` |
| `Website` | `nvarchar(50)` | `NULL` |
| `Lo ` | `image` | `NULL` |
| `CurrencySign` | `nvarchar(50)` | `NULL` |
| `Mobile1` | `nvarchar(50)` | `NULL` |
| `Mobile2` | `nvarchar(50)` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `CompanyID` → محتمل: `ShipmentCompany`, `Lookup_IsInsuranceCompanyStaus`, `LookUp_PersonCompany_Type`, `tblCompany`

---
### `tItemDataMain`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 47

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ItemName` | `nvarchar(255)` | `NOT NULL` |
| `ItemName_New` | `nvarchar(255)` | `NULL` |
| `ItemName_Part_No` | `nvarchar(255)` | `NULL` |
| `Item_Start_Date` | `datetime` | `NULL` |
| `ItemName_Count` | `int` | `NULL` |
| `ItemCode` | `nvarchar(300)` | `NULL` |
| `ItemCode2` | `nvarchar(300)` | `NULL` |
| `ItemDiscreption` | `nvarchar(200)` | `NULL` |
| `Suply_Name` | `nvarchar(120)` | `NULL` |
| `Car_Type` | `nvarchar(100)` | `NULL` |
| `Model` | `nvarchar(50)` | `NULL` |
| `Item_Group` | `nvarchar(50)` | `NULL` |
| `Item_Marka` | `nvarchar(50)` | `NULL` |
| `Creat_Machine` | `nvarchar(50)` | `NULL` |
| `Mini_Requird` | `int` | `NULL` |
| `Mini_Requird_Max` | `int` | `NULL` |
| `TaxType` | `nvarchar(50)` | `NULL` |
| `TaxType_Sub` | `nvarchar(50)` | `NULL` |
| `Item_CodeType` | `nvarchar(50)` | `NULL` |
| `Item_Tax_Code` | `nvarchar(50)` | `NULL` |
| `Item_Have_Tax` | `int` | `NULL` |
| `Item_Have_Tax_A_T` | `int` | `NULL` |
| `Unit_Code` | `nvarchar(50)` | `NULL` |
| `Ubi_Price` | `decimal(18, 2)` | `NULL` |
| `Sale_Price` | `decimal(18, 2)` | `NULL` |
| `Item_Type` | `nvarchar(50)` | `NULL` |
| `Start_Balance-Date` | `datetime` | `NULL` |
| `Item_No_Group` | `int` | `NULL` |
| `Item_Name_Group` | `nvarchar(50)` | `NULL` |
| `Last_Purh_Date` | `datetime` | `NULL` |
| `Last_Sale_Date` | `datetime` | `NULL` |
| `Save` | `int` | `NULL` |
| `Trans_Sav` | `int` | `NULL` |
| `Have_BarCode` | `nvarchar(50)` | `NULL` |
| `Item_Work` | `nvarchar(50)` | `NULL` |
| `Item_Cost` | `decimal(18, 2)` | `NULL` |
| `Item_Cost_Now` | `decimal(18, 2)` | `NULL` |
| `User_Name` | `nvarchar(50)` | `NULL` |
| `Itme_Kind` | `nvarchar(50)` | `NULL` |
| `Item_Notes` | `nvarchar(2000)` | `NULL` |
| `Item_Pic` | `nvarchar(50)` | `NULL` |
| `Item_Stop_Purch` | `nvarchar(50)` | `NULL` |
| `Item_Stop_Sale` | `nvarchar(50)` | `NULL` |
| `unitType` | `nvarchar(50)` | `NULL` |
| `Unit` | `nvarchar(50)` | `NULL` |
| `Sale_Price_Half_Dealer` | `decimal(18, 2)` | `NULL` |
| `Sale_Price_Dealer` | `decimal(18, 2)` | `NULL` |

---
### `Total_of_Nazlla`

**الاستخدام الوظيفي:** جدول تشغيلي/مرجعي؛ تم استنتاج الغرض من اسم الجدول وأسماء أعمدته في ملف SQL المقدم.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `ID` | `bigint` | `NULL` |
| `date` | `smalldatetime` | `NOT NULL` |
| `number_of_nazlla` | `decimal(18, 2)` | `NULL` |
| `mashineID` | `int` | `NOT NULL` |
| `nazlla_price` | `decimal(18, 2)` | `NULL` |
| `total` | `decimal(18, 2)` | `NULL` |
| `Supplier_ID` | `bigint` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `Unit_itemEmployeeBalance`

**الاستخدام الوظيفي:** أرصدة الأصناف أو العهد المرتبطة بالموظفين.

**عدد الأعمدة:** 39

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `id` | `bigint` | `NOT NULL` |
| `Item_id` | `bigint` | `NULL` |
| `EmployeeId` | `bigint` | `NULL` |
| `Store_Type` | `int` | `NULL` |
| `Code` | `nvarchar(max)` | `NULL` |
| `unit_id` | `bigint` | `NULL` |
| `Full_qua` | `decimal(18, 2)` | `NULL` |
| `D_Qua` | `decimal(18, 2)` | `NULL` |
| `Item_No` | `decimal(18, 2)` | `NULL` |
| `Reserved_Balance_Larg_Qnty` | `decimal(18, 2)` | `NULL` |
| `Reserved_Balance_Small_Qnty` | `decimal(18, 2)` | `NULL` |
| `Big_unit` | `int` | `NULL` |
| `Small_unit` | `decimal(18, 2)` | `NULL` |
| `P_price` | `decimal(18, 2)` | `NULL` |
| `S_price` | `decimal(18, 2)` | `NULL` |
| `Specification` | `nvarchar(100)` | `NULL` |
| `Upon_Request` | `decimal(18, 2)` | `NULL` |
| `Currency` | `nvarchar(50)` | `NULL` |
| `active_flage` | `int` | `NULL` |
| `TaxSales_Value` | `decimal(18, 2)` | `NULL` |
| `New2` | `int` | `NULL` |
| `OperationDate` | `datetime` | `NULL` |
| `User_Id` | `int` | `NULL` |
| `Branch_Id` | `int` | `NULL` |
| `Insert_ServerDateTime` | `datetime` | `NULL` |
| `Update_ServerDateTime` | `datetime` | `NULL` |
| `EditOperationDate` | `datetime` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |
| `ProcessNo` | `bigint` | `NULL` |
| `TaxSales10_Value` | `decimal(18, 2)` | `NULL` |
| `S_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `S_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `P_price_LargeUnit` | `decimal(18, 2)` | `NULL` |
| `P_price_SmallUnit` | `decimal(18, 2)` | `NULL` |
| `Unit_Used_For_S_price_Default` | `int` | `NULL` |
| `Unit_Used_For_P_price_Default` | `int` | `NULL` |
| `S_price_Item_No` | `decimal(18, 2)` | `NULL` |
| `P_price_Item_No` | `decimal(18, 2)` | `NULL` |
| `Is_ItemWithoutBalance` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `User_Id` → محتمل: `USER_DETAILES_Branches_Permission`, `USER_DETAILES_`, `USER_DETAILES`

---
### `WorkOrderDetail`

**الاستخدام الوظيفي:** تفاصيل أوامر العمل العامة.

**عدد الأعمدة:** 15

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `WorkOrderListId` | `bigint` | `NOT NULL` |
| `Item_Id` | `bigint` | `NOT NULL` |
| `Item_Name` | `nvarchar(250)` | `NULL` |
| `UnitId` | `bigint` | `NOT NULL` |
| `UnitName` | `nvarchar(100)` | `NULL` |
| `RequiredQuantity` | `decimal(18, 4)` | `NULL` |
| `IssuedQuantity` | `decimal(18, 4)` | `NULL` |
| `ReceivedQuantity` | `decimal(18, 4)` | `NULL` |
| `Notes` | `nvarchar(max)` | `NULL` |
| `OperationDate` | `datetime2(7)` | `NULL` |
| `Insert_ServerDateTime` | `datetime2(7)` | `NULL` |
| `Update_ServerDateTime` | `datetime2(7)` | `NULL` |
| `EditOperationDate` | `datetime2(7)` | `NULL` |
| `Edit_User_Id` | `bigint` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `WorkOrderListId` → محتمل: `WorkOrderList`

---
### `WorkOrderList`

**الاستخدام الوظيفي:** رأس أوامر العمل العامة.

**عدد الأعمدة:** 21

| العمود | النوع | السماح بـ NULL |
|---|---|---|
| `Id` | `bigint` | `NOT NULL` |
| `WorkOrder_No` | `bigint` | `NOT NULL` |
| `WorkOrderDate` | `datetime2(7)` | `NULL` |
| `Status` | `int` | `NULL` |
| `StatusStr` | `nvarchar(250)` | `NULL` |
| `WorkOrderType` | `int` | `NULL` |
| `WorkOrderTypeStr` | `nvarchar(250)` | `NULL` |
| `StoreId` | `bigint` | `NULL` |
| `StoreName` | `nvarchar(250)` | `NULL` |
| `CustomerId` | `bigint` | `NULL` |
| `CustomerName` | `nvarchar(250)` | `NULL` |
| `MachineId` | `bigint` | `NULL` |
| `MachineName` | `nvarchar(250)` | `NULL` |
| `EmployeeId` | `bigint` | `NULL` |
| `EmployeeName` | `nvarchar(250)` | `NULL` |
| `NotesAll` | `nvarchar(max)` | `NULL` |
| `OperationDate` | `datetime2(7)` | `NULL` |
| `Insert_ServerDateTime` | `datetime2(7)` | `NULL` |
| `Update_ServerDateTime` | `datetime2(7)` | `NULL` |
| `EditOperationDate` | `datetime2(7)` | `NULL` |
| `Edit_User_Id` | `int` | `NULL` |

**علاقات منطقية محتملة (ليست Foreign Keys مؤكدة):**
- `MachineId` → محتمل: `Equivalent_Machine_Problem`, `Matwa_Production_SendToTextileMachine`

---

# 4. Integrated Logical Business Map, SQL Rules, and EF Rules

## 5. خريطة علاقات الأعمال المهمة — Logical Business Map

> هذه الخريطة ليست بديلًا عن Foreign Keys. هي نقطة بداية للـ AI لفهم تدفق البيانات، ويجب التحقق من الأعمدة الفعلية قبل تنفيذ SQL.

### المحاسبة
- `Account_Entry_List` → `Account_Entry_Details`
- `Account_Entry_Details` → الحسابات الرئيسية/الفرعية (`MainAccount`, `Main_Accounts`, `Submain_Account`) بحسب الحقول الموجودة.
- `Financial_Period` → الأرصدة والحركات المالية.
- `Account_Entry_Master_Balance*` → جداول تجميع/أرصدة مرتبطة بالحسابات والفترات ومراكز التكلفة.

### السيارات والورش
- `Customer` → `Cars`
- `Cars` → `CarsWorkOrders`
- `CarsWorkOrders` → `CarsWorkOrdersRequestedDetails`
- `CarsWorkOrders` → `CarsWorkOrdersItemProcessedDetails`
- `CarsWorkOrders` → `CarsWorkOrdersDeliverDetails`
- `CarsWorkOrders` → `CarsWorkOrdersItemProcessedEmployees`
- `CarsWorkOrders` → `Invoices_List` عبر الحقول التي تحمل اسم أمر الشغل، مثل `CarsWorkOrdersId` حيثما وجد.

### الفواتير
- `Bills_List` → `Bills_Details`
- `Invoices_List` → `Invoices_Details`
- `Bills_List_Reverse` → `Bills_Details_Reverse`
- `Invoices_List_Reverse` → `Invoices_Details_Reverse`
- الفواتير → الأصناف `Item`
- الفواتير → العملاء `Customer` أو الموردين `Supplier` حسب نوع المستند.

### الضرائب والفاتورة الإلكترونية
- `TaxInvoices_List` → `TaxInvoices_LineItems`
- `TaxInvoices_LineItems` → `TaxInvoices_ItemsTaxes`
- `TaxInvoices_List` → `TaxInvoices_TaxesTotal`
- `TaxReverseInvoices_List` → الجداول المناظرة للفواتير العكسية.
- `Tax_Branches` / `Tax_BranchesConfig` / `Account_BranchesTaxInfo` → إعدادات الفروع الضريبية.

### الأصناف والمخزون
- `Item` هو الكيان المركزي للصنف/الخدمة.
- `Unit_item` يحدد الوحدات المرتبطة بالأصناف.
- `Store` يمثل المخازن.
- `Unit_Item_Balance_Date_Details`, `Unit_Item_ExpireDate`, وحركات الإدخال والإخراج تمثل جوانب الرصيد والحركة.
- جداول `Item_*`, `Items_*`, و`Lookup_*` توفر التصنيفات والأكواد والخصائص والأسعار والضرائب.

### الحجوزات والخدمات
- `Customer` → `Appointments`
- `Appointments` → `AppointmentServices`
- `AppointmentServices` → `AppointmentServiceEmployees`
- `AppointmentPackages` → `AppointmentPackageServices`
- `Customer` → `AppointmentCustomerPackages`
- `AppointmentCustomerPackages` → `AppointmentCustomerPackageServices`
- `Appointments`/الباقات → `AppointmentSessions`
- `AppointmentSessions` → `AppointmentSessionEmployees`

### الموارد البشرية
- `Employee` → `Department`
- `Employee` → `Presence_absence`
- `Employee` → `Salaries`
- `Salaries` → `Salary_increment` / `Salary_Type` / `Payment_Salaries` بحسب منطق التطبيق.

## 6. قواعد توليد SQL للذكاء الاصطناعي

قبل كتابة أي استعلام، اتبع الخطوات التالية:

1. حدّد الوحدة الوظيفية.
2. حدّد جدول الرأس وجدول التفاصيل إن وجد.
3. حدّد الحقل الذي يربط الرأس بالتفاصيل.
4. تحقق من اسم العمود وحالته (`NULL`/`NOT NULL`) من هذا الملف.
5. لا تفترض PK/FK من الاسم وحده.
6. عند الحسابات، حدّد الفترة والفرع والعملة قبل تجميع الأرقام.
7. عند المخزون، حدّد الصنف والوحدة والمخزن والتاريخ قبل حساب الرصيد.
8. عند الفواتير، افصل بين الفاتورة الأصلية والمرتجع/العكس.
9. عند الضرائب، لا تعتبر إجمالي الضريبة مساويًا لمجموع تفاصيل الضرائب دون فحص طريقة التقريب.
10. عند استخدام `Temp` أو `BACK` أو جداول المعالجة، لا تعدّلها مباشرة إلا إذا كان كود التطبيق يثبت أنها جداول تشغيلية مقصودة.

## 7. قواعد خاصة بالتعامل مع المال والعملات

قد تحتوي الجداول المالية على أكثر من تمثيل للمبلغ، مثل المبلغ الأصلي والمكافئ/المحول ومعامل العملة.

لذلك:

- لا تجمع مبلغًا أصليًا مع مبلغ مكافئ.
- حدّد العملة قبل التجميع.
- افحص `Currency`, `Currency_Factor`, وحقول `Equivalent` أو الحقول التي تحمل معنى المكافئ.
- عند مقارنة الأرصدة، قارن نفس الفترة ونفس الفرع ونفس العملة.
- لا تعتمد على قيمة `Balance` المخزنة إذا كان المطلوب هو الرصيد التاريخي دون التحقق من تاريخ آخر تحديث وآلية تحديث الجدول.

## 8. قواعد خاصة بالأصناف والمخزون

`Item` هو أحد أهم الجداول المركزية في النظام.

عند التعامل معه، انتبه إلى وجود:

- كود الصنف وأكواد بديلة.
- اسم الصنف والاسم على الفاتورة.
- الوحدات الكبيرة والصغيرة.
- أسعار البيع والشراء.
- أسعار التكلفة والمتوسط.
- التصنيفات والأنواع والألوان والمقاسات.
- بيانات الضرائب.
- بلد المنشأ والأكواد الضريبية.
- خصائص الخدمة، مثل `Is_ItemOrService`, `TotalSessions`, `ServiceDurationInMinutes`.
- خصائص مرتبطة بالسيارات.
- الباركود.
- خصائص المخزون والطباعة وإظهار الصنف على الفاتورة.

## 9. قواعد خاصة بالفواتير والمرتجعات

عند تحليل مبيعات أو مشتريات:

1. ابدأ من جدول الرأس (`*_List`).
2. اربطه بالتفاصيل (`*_Details`) باستخدام معرف الفاتورة الفعلي.
3. أضف العميل/المورد.
4. أضف الأصناف.
5. افصل الفواتير الأصلية عن الجداول التي تحمل `Reverse`.
6. إذا كان التقرير ضريبيًا، اربط بالجداول الضريبية بدل استنتاج الضرائب من السعر فقط.

## 10. قواعد خاصة بصيانة السيارات

المسار المنطقي النموذجي:

`Customer` → `Cars` → `CarsWorkOrders` → التفاصيل/الخدمات/قطع الغيار → `Invoices_List` → التفاصيل المالية/الضريبية.

يجب عدم افتراض أن كل أمر شغل له فاتورة أو أن كل فاتورة مرتبطة بأمر شغل؛ تحقق من البيانات الفعلية.

## 11. الجداول المؤقتة والاحتياطية

الجداول التي تحتوي على:

- `Temp`
- `BACK`
- `History`

يجب اعتبارها جزءًا من تصميم التطبيق حتى يثبت العكس.

**ممنوع** على AI حذفها أو إعادة تصميمها أو دمجها تلقائيًا أثناء migration دون تحليل كود التطبيق والإجراءات المخزنة والاستعلامات التي تستخدمها.

## 12. جودة المخطط وملاحظات تقنية

المخطط يحتوي على أسماء تاريخية/غير متسقة، مثل:

- اختلاف `Item` / `Items`.
- اختلاف `Lookup` / `LookUp`.
- وجود أسماء تحتوي على مسافات داخل الأقواس، مثل أسماء فئات أو أعمدة.
- وجود أسماء بها أخطاء إملائية أو transliteration تاريخي مثل `Purshing`, `Detailes`, `Cate ry`, وغيرها.
- وجود أعمدة من نوع `image` القديمة في بعض الجداول.
- وجود عدد كبير من أعمدة التدقيق مثل `User_Id`, `OperationDate`, `Insert_ServerDateTime`, `Update_ServerDateTime`, `Edit_User_Id`, `ProcessNo`.

هذه الأسماء **جزء من المخطط الحالي** ويجب عدم تصحيحها في SQL المولد إلا إذا كان المطلوب صراحةً عمل migration/renaming.

## 13. ما الذي يحتاجه AI قبل إجراء تعديل بنيوي؟

قبل أي عملية من النوع التالي:

- إضافة/حذف عمود.
- تغيير نوع بيانات.
- إعادة تسمية جدول.
- إضافة PK/FK.
- دمج جداول.
- نقل البيانات.
- إنشاء EF Core model.
- إزالة `IDENTITY`.
- إنشاء migration.

يجب أولًا فحص:

1. كود التطبيق الذي يستخدم الجدول.
2. Stored Procedures.
3. Views.
4. Triggers.
5. Indexes.
6. Constraints.
7. SQL Queries المباشرة.
8. العلاقات الفعلية في بيانات الإنتاج.

## 14. توصية لإنشاء نموذج EF Core

عند توليد Entity classes:

- استخدم أسماء الجداول الفعلية باستخدام `ToTable`.
- لا تفترض Primary Key من اسم `Id`.
- إذا لم يوجد PK معروف، يجب تحديد كيفية التعامل معه قبل استخدام EF Core في عمليات التحديث.
- افصل جداول `Temp` عن الـ domain entities إذا كانت للاستخدام الداخلي فقط.
- استخدم Fluent API للأسماء غير القياسية.
- لا تغيّر أسماء الأعمدة في قاعدة البيانات لمجرد تحسين أسماء C#؛ يمكن عمل mapping بين اسم C# واسم SQL.

## 15. قاموس البيانات السريع

الكيانات الأساسية التي ينبغي أن يتعرف عليها AI بسرعة:

| الكيان | الجدول المرجعي الأساسي |
|---|---|
| الفروع | `Account_Branches` |
| العملات | `Currency` |
| الحسابات الرئيسية | `MainAccount` / `Main_Accounts` |
| الحسابات الفرعية | `Submain_Account` |
| القيود | `Account_Entry_List` |
| تفاصيل القيود | `Account_Entry_Details` |
| العملاء | `Customer` |
| الموردون | `Supplier` |
| الموظفون | `Employee` |
| الأصناف والخدمات | `Item` |
| المخازن | `Store` |
| الوحدات | `Unit_item` / `unit` |
| فواتير الشراء/البيع | `Bills_List` |
| تفاصيل الفواتير | `Bills_Details` |
| فواتير البيع/الخدمة | `Invoices_List` |
| السيارات | `Cars` |
| أوامر شغل السيارات | `CarsWorkOrders` |
| الحجوزات | `Appointments` |
| الجلسات | `AppointmentSessions` |
| الباقات | `AppointmentPackages` |
| البنوك | `Banks` |
| الخزائن | `Money_Transaction_Box` |
| الفترات المالية | `Financial_Period` |
| مراكز التكلفة | `Cost_Center_Tree` |
| الفواتير الضريبية | `TaxInvoices_List` |
| المستخدمون/المصادقة | `Authentication`, `USER_DETAILES` |
| الإعدادات | `SysConfig` |

## 16. تعليمات نهائية للـ AI Agent

عند سؤال المستخدم عن بيانات النظام، اتبع هذا التسلسل:

**Understand → Identify Module → Identify Tables → Verify Columns → Infer/Verify Relationships → Build Query → Validate Result**

ولا تنتقل مباشرة إلى كتابة SQL اعتمادًا على اسم الجدول فقط.

إذا كان السؤال غامضًا، اطلب تحديد:
- الفرع.
- الفترة.
- التاريخ.
- نوع الحركة.
- العميل/المورد.
- الصنف.
- العملة.
- نوع المستند.

إذا كانت البيانات المطلوبة يمكن أن تأتي من أكثر من مصدر، اعرض سبب اختيار الجدول، ولا تدمج جداول مالية مختلفة دون تفسير.

---

## 17. ملاحظة مصدر البيانات

هذا المستند تم توليده آليًا من ملف SQL المرفوع بتاريخ **{today}**.

**المعلومات البنيوية** (أسماء الجداول والأعمدة وأنواع البيانات وNULL/NOT NULL) مستخرجة من ملف SQL.

**الوصف الوظيفي** لبعض الجداول مستنتج من أسماء الجداول والأعمدة ومن بنية ERP الظاهرة في المخطط، ولذلك يجب اعتبار الوصف الوظيفي **مرجعًا مساعدًا وليس توثيقًا رسميًا لسلوك التطبيق** ما لم يتم تأكيده من كود C#/SQL أو من استخدام الجدول فعليًا.

**الإصدار:** `ERP Database Schema Markup v1.0 — {today}`


## 18. قاعدة إلزامية: تطابق أسماء C# Models مع SQL Tables/Columns

عند التعامل مع قاعدة البيانات من خلال **C# / Entity Framework / EF Core**، يجب الالتزام بقاعدة التسمية التالية:

### 18.1 أسماء الـ C# Model Classes

اسم كل `C# Model / Entity Class` يجب أن يكون **مطابقًا تمامًا لاسم جدول SQL**.

مثال:

```csharp
public class Customer
{
    // ...
}

public class Cars
{
    // ...
}

public class CarsWorkOrders
{
    // ...
}
```

إذا كان اسم الجدول يحتوي على اسم غير قياسي أو خطأ إملائي أو صيغة تاريخية، **يجب الاحتفاظ بالاسم كما هو** وعدم تصحيحه تلقائيًا.

مثال:

```csharp
public class Detailes_Purshing
{
    // ...
}
```

وليس:

```csharp
public class PurchaseDetails
{
    // ...
}
```

إلا إذا طلب المستخدم صراحةً إنشاء طبقة Domain منفصلة بأسماء مختلفة.

### 18.2 أسماء خصائص الـ C# Model

اسم كل Property في الـ C# Model يجب أن يكون **مطابقًا تمامًا لاسم عمود SQL**.

مثال إذا كان الجدول:

```sql
CREATE TABLE [dbo].[Customer]
(
    [Id] INT NOT NULL,
    [Name] NVARCHAR(200) NULL,
    [Phone] NVARCHAR(50) NULL
)
```

فيكون الـ Model:

```csharp
public class Customer
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
}
```

ولا يتم تغيير:

```text
Phone
```

إلى:

```text
PhoneNumber
```

ولا:

```text
Customer_Name
```

إلى:

```text
CustomerName
```

إلا إذا طلب المستخدم ذلك صراحة.

### 18.3 قاعدة EF DbSet

اسم كل `DbSet<T>` في `DbContext` يجب أن يكون **مطابقًا تمامًا لاسم جدول SQL**.

مثال:

```csharp
public DbSet<Customer> Customer { get; set; }

public DbSet<Cars> Cars { get; set; }

public DbSet<CarsWorkOrders> CarsWorkOrders { get; set; }

public DbSet<Bills_List> Bills_List { get; set; }

public DbSet<Bills_Details> Bills_Details { get; set; }
```

ويجب عدم استخدام أسماء مختلفة مثل:

```csharp
public DbSet<Customer> Customers { get; set; }
```

إذا كان اسم الجدول هو:

```text
Customer
```

بل يجب أن يكون:

```csharp
public DbSet<Customer> Customer { get; set; }
```

### 18.4 قاعدة EF Mapping

عند استخدام Fluent API، يجب أن يعكس الـ Mapping أسماء قاعدة البيانات الفعلية.

إذا كان اسم الـ Entity مطابقًا لاسم الجدول، يمكن استخدام:

```csharp
modelBuilder.Entity<Customer>()
    .ToTable(nameof(Customer));
```

وبالنسبة للأعمدة، إذا كانت أسماء الـ Properties مطابقة لأسماء الأعمدة، فلا حاجة عادةً إلى `HasColumnName()` لكل عمود.

عند الحاجة إلى تعريف صريح:

```csharp
modelBuilder.Entity<Customer>()
    .Property(x => x.Name)
    .HasColumnName(nameof(Customer.Name));
```

### 18.5 لا تستخدم Naming Conventions لإعادة تسمية قاعدة البيانات

يجب على AI عدم تطبيق convention تلقائي مثل:

- تحويل `Customer` إلى `Customers`.
- تحويل `Bills_List` إلى `BillsList`.
- تحويل `CarsWorkOrders` إلى `CarWorkOrder`.
- تحويل `User_Id` إلى `UserId`.
- تحويل `Account_Entry_Details` إلى `AccountEntryDetails`.

الأسماء الموجودة في SQL هي **Source of Truth**.

### 18.6 قاعدة توليد Models من Database

عند طلب:

- Generate C# Models
- Generate EF Core Entities
- Scaffold Database
- Create Entity Classes
- Create DbContext
- Reverse Engineer Database

يجب اتباع الآتي:

```text
SQL Table Name
      ↓
C# Entity Class Name
      ↓
DbSet Property Name
      ↓
SQL Table Mapping

All names remain identical.
```

وبالمثل:

```text
SQL Column Name
      ↓
C# Property Name
      ↓
EF Column Mapping

All names remain identical.
```

### 18.7 مثال كامل

إذا كانت قاعدة البيانات تحتوي على:

```text
Table:
CarsWorkOrders

Columns:
Id
CarsId
CustomerId
WorkOrderDate
Status
```

فيجب إنشاء:

```csharp
public class CarsWorkOrders
{
    public int Id { get; set; }

    public int CarsId { get; set; }

    public int CustomerId { get; set; }

    public DateTime WorkOrderDate { get; set; }

    public int Status { get; set; }
}
```

وفي `DbContext`:

```csharp
public DbSet<CarsWorkOrders> CarsWorkOrders { get; set; }
```

وعند استخدام Fluent API:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<CarsWorkOrders>()
        .ToTable(nameof(CarsWorkOrders));

    base.OnModelCreating(modelBuilder);
}
```

### 18.8 العلاقات لا تغير أسماء الأعمدة

إذا كان هناك Foreign Key مثل:

```text
CustomerId
```

فيجب أن يبقى:

```csharp
public int CustomerId { get; set; }
```

ولا يتم تحويله إلى:

```csharp
public int CustomerID { get; set; }
```

ولا:

```csharp
public int Customer_Id { get; set; }
```

حتى لو كانت إحدى هذه الصيغ أكثر شيوعًا في C#.

### 18.9 Navigation Properties

يمكن إضافة Navigation Properties لتحسين استخدام EF، ولكن **لا يجوز أن تؤدي إلى تغيير أسماء الأعمدة أو الـ FK Properties**.

مثال:

```csharp
public class CarsWorkOrders
{
    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }
}
```

اسم الـ FK يبقى مطابقًا لقاعدة البيانات:

```text
CustomerId
```

والـ Navigation Property:

```text
Customer
```

هي إضافة برمجية ولا تستبدل العمود الأصلي.

### 18.10 قاعدة عدم إعادة التسمية

**AI MUST NOT rename database objects when generating C# Models or EF Core code.**

يُمنع تلقائيًا:

- PascalCase conversion.
- Singular/Plural conversion.
- إزالة underscores.
- تصحيح الأخطاء الإملائية.
- تغيير الاختصارات.
- تغيير `Id` إلى `ID`.
- تغيير `User_Id` إلى `UserId`.
- تغيير أسماء الجداول القديمة إلى أسماء أحدث.

قاعدة النظام:

> **Database names are the canonical names. C# Models, Properties and DbSet names must preserve them exactly.**

### 18.11 استثناء وحيد

إذا طلب المستخدم صراحةً إنشاء **Clean Domain Model / DTO / ViewModel** بأسماء مختلفة عن قاعدة البيانات، يمكن استخدام أسماء C# مختلفة.

في هذه الحالة يجب الفصل بوضوح بين:

```text
Database Entity
        ↓
DTO / ViewModel / Domain Model
```

ولا يجوز تطبيق أسماء الـ Domain الجديدة على الـ Entity المرتبط مباشرة بقاعدة البيانات دون طلب صريح.

---


---

# 5. Final AI Agent Operating Instructions

عند تنفيذ أي مهمة على هذا المشروع، اتبع الترتيب التالي:

```text
Understand Request
      ↓
Identify Business Module
      ↓
Identify Canonical Tables
      ↓
Verify Exact Columns / Data Types
      ↓
Verify PK / FK / Logical Relationships
      ↓
Check Branch / Period / Currency / Store / Item / Document Context
      ↓
Apply Business Rules
      ↓
Generate SQL / C# / EF / Report Logic
      ↓
Validate Accounting + Inventory + Data Integrity
```

## Mandatory validation checklist

```text
[ ] Exact table names preserved
[ ] Exact column names preserved
[ ] No unverified PK/FK assumptions
[ ] Correct business module identified
[ ] Branch_Id / Branch context preserved
[ ] Financial period preserved
[ ] Currency preserved
[ ] Original and Equivalent amounts not mixed
[ ] Cost Center preserved where applicable
[ ] Debit/Credit direction verified
[ ] Inventory quantity and cost effects verified
[ ] Head/Detail relationships verified
[ ] Reverse/Return documents handled separately
[ ] Temp/BACK/History tables not modified blindly
[ ] Aggregated balances checked against source transactions
[ ] Financial reports remain consistent
[ ] C# Model name equals SQL table name
[ ] C# Property name equals SQL column name
[ ] EF DbSet name equals SQL table name
[ ] EF mapping preserves canonical SQL names
```

---

# 6. Known Conflicts / Items Requiring Confirmation

## 6.1 Equivalent_Account_Currency_Id

ورد في قواعد العمل وصف يشير إلى:

```text
Equivalent_Account_Currency_Id = 0
```

بينما تعريف `Account_Entry_Master_Balance` في المعلومات السابقة أظهر قيمة افتراضية مختلفة. **لا يتم حسم هذا التعارض تلقائيًا**؛ يجب الرجوع إلى قاعدة البيانات الفعلية وكود الترحيل.

## 6.2 Account_Follows

القيم الوظيفية المستخدمة في قواعد العمل الحالية:

| Value | Meaning |
|---:|---|
| 1 | التشغيل |
| 2 | المتاجرة |
| 3 | الأرباح والخسائر / قائمة الدخل |
| 4 | الميزانية العمومية |

يجب عدم تغيير هذه القيم دون تأكيد تصميم القوائم المالية.

## 6.3 Store / Warehouse

`Store` موجود فعليًا في مخطط `ERP_V1_1`، ولذلك أصبح تعريف المخزن جزءًا من الـ Schema المرجعي، مع استمرار قاعدة العمل التي تربط الحركة المخزنية بالفرع والمخزن.

## 6.4 Inventory Transaction Entities

قواعد العمل تصف GRN وGoods Issue وTransfer وReturns وAdjustments وProduction/WIP، بينما يجب تحديد الجداول الفعلية المستخدمة لكل عملية من المخطط وكود التطبيق قبل بناء أي عملية جديدة.

---

# 7. Version & Merge History

```text
ERP.md       → Baseline business / accounting / inventory context
ERP_V1_1.md  → Full database schema + additional AI/EF rules
ERP_V1_2.md  → Unified merged reference + canonical 279-table database schema
Date         → 2026-08-11
```

### Merge Policy

- تم دمج المعلومات الوظيفية من `ERP.md` مع المخطط الكامل من `ERP_V1_1.md`.
- تم منع تكرار تعريفات الجداول؛ **المخطط الكامل في قسم Database Schema هو المرجع البنيوي**.
- تم الاحتفاظ بقواعد العمل المحاسبية والمخزنية التي لا يوفرها مخطط قاعدة البيانات وحده.
- تم دمج قواعد AI وSQL وEF بدلاً من تكرارها.
- لم يتم تصحيح أسماء الجداول أو الأعمدة التاريخية أو الأخطاء الإملائية الموجودة في قاعدة البيانات.
- تم الحفاظ على نقاط التعارض كملاحظات تحتاج تأكيدًا بدلاً من اختراع قيمة نهائية.

---

# 8. Source Preservation

هذا الملف هو **نسخة موحدة للاستخدام مع AI/Codex/LLM Agents**، وليس بديلاً عن قاعدة البيانات الفعلية أو كود التطبيق.

عند اختلاف سلوك التطبيق الفعلي عن الوصف الموجود هنا، يجب تحليل:

- SQL DDL
- Stored Procedures
- Views
- Triggers
- Indexes
- Constraints
- C# / EF / EF Core code
- بيانات الإنتاج الفعلية

قبل إصدار حكم نهائي.
