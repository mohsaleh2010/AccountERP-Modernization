# خطة تحويل إذونات النقدية (سندات القبض والصرف) وتقارير الخزينة إلى Razor

**الحالة:** الدراسة اكتملت (2026-09-25) والتنفيذ يبدأ بالمرحلة ن1. **النطاق:** قائمة «أوراق القبض» (`menuMoney`) **ما عدا شاشات الشيكات** (بقرار المستخدم)، وقائمة «حركة الخزينة والبنوك» (`menuBanks`).
**قواعد التنفيذ (من المستخدم):** لا تعديل على اللوجيك القديم؛ نستعمل الدوال والموديلات الموجودة؛ الأكواد الجديدة فى `AccountModelsLib` (خدمات تلفّ الدوال القديمة) وصفحات Razor؛ الاختيارات الفنية أختارها بنفسى؛ بيانات الاختبار تبقى.

---

## 1) ما تبيّن من الدراسة (من الكود الحى ومن قاعدة التطوير)

### 1.1 الجداول
| الجدول | الدور |
|---|---|
| `Order_Money` | **رأس الإذن**: `Order_No` (تسلسل لكل نوع/صندوق/تاريخ)، `Transaction_type` (1 استلام، 2 صرف)، `Bank_ID` (الصندوق)، `Total_Value`، `Man_Name` (مستلم من/مصروف إلى)، `Order_Notes`، `Factor`/`Entry_Currency_ID`، `Order_Tafkeet_Value`، `IS_Posted_Flage` و`Account_Entry_List_ID` (ترحيل القيد)، `Year_Financial_Period_Id`، `Invoice_Id`/`CarsWorkOrdersId` (ربط بفاتورة/أمر شغل)، `IsMoreTheOneBoxShow`. |
| `Money_Transaction_Box` | **سطور الإذن** (سطر لكل طرف/حساب): `Order_Mony_Id`، `Import_Value`/`Export_Value`/`Transaction_Value`، `Bank_Id`، `Customer_Id`/`Supplier_Id`/`SubAccount_Id`/`Account_Type`، `Account_Cost_Center_Tree_Id`، `Balance_Before/After` (رصيد الطرف قبل/بعد)، `Bank_Balance` (رصيد الصندوق بعد السطر)، `Factor`، `Notes`. |
| `Banks` (+`Banks_Balance`) | الصناديق (`Is_Bank=0`) والبنوك (`Is_Bank=1`): `SubAccount_Id` (حساب الصندوق فى الأستاذ)، `Currency_Id`، `Last_Balance`؛ `Banks_Balance` رصيد الصندوق اليومى (`Set_BankBalanceDay`). |
| `Customers_Accounts` / `Suppliers_Accounts` + `Customer.Balance`/`Supplier.Balance` | حركة حساب العميل/المورد (مرجع النوع 3 لسندات الاستلام) وتحديث الرصيد. |
| `Account_Entry_List`/`Account_Entry_Details` | القيد اليومى الناتج (يُنشأ تلقائياً). |
حجم قاعدة التطوير: 3092 إذناً (1565 استلام + 1527 صرف، 3071 مرحَّلة بقيد)، 3076 سطراً، صندوقان و9 بنوك، 617 يومية صندوق، 3098 قيداً.

### 1.2 المحرك (كله فى `AccountModelsLib.Money_Transaction_BoxOrderReportCLS`، 5176 سطراً — مشترك حى بين الويندوز والفواتير)
| العملية | الدالة | ما تفعله بالتسلسل |
|---|---|---|
| **إذن طرف واحد** (عميل/مورد ± عكسه) | `Make_Transaction_BanksBox_FromInvoices(ref Excp, ref Msg, ref opp, Money_Transaction_BoxVM)` | يقرأ الصندوق/عملته ← يُدخل `Order_Money` ← سطر `Money_Transaction_Box` (رصيد الصندوق الجديد = القديم ± القيمة) ← حركة `Customers_Accounts`/`Suppliers_Accounts` وتحديث الرصيد (`Set_Customer_Balance_WithMoney_Transaction`/`Set_Supplier_...`) ← `Set_BankBalanceDay` ← `Accounts_Entry_POST_CLS.Make_Entry_For_AccountsSandouk` (القيد). |
| **إذن متعدد الأسطر** (أى حساب/عميل/مورد، مراكز تكلفة) | `Make_Transaction_BanksBox_NewSarf(... DataTable dt_Order ...)` ثم `Set_BankBalanceDay` ثم `Make_Entry_For_AccountsSandouk` (الجيل الأحدث `..._NewSarf_(omMain, mbxList)` بالموديلات) | رأس واحد وأسطر بعدد الحسابات؛ كل سطر عميل/مورد يُحدِّث حسابه عبر `Set_TransactionMoney_For_Customer_SupplierAccounts`. |
| **تعديل** | `Edit_Transaction_BanksBox_NewSarf(_)`/`Edit_Transaction_BanksBox_NewSarfMulti_`/`..NewSarfForAccounts` | يحذف الأسطر المحذوفة ويعدّل القائمة ويعيد القيد (صلاحية 316). |
| **حذف** | `Delete_Transaction_BanksBox_NewSarf_New(ref Excp, ref Msg, ref opp, Order_MoneyId[, ref Entry_No])` | يعكس أثر الصندوق والعميل/المورد ويحذف القيد ثم الإذن. |
| **تحويل بين صندوقين** | `MakeOrderBetweenTwo_Sandouk(...)` | إذنا صرف واستلام مربوطان (عملتان وعامل تحويل). |
| **إذن عدة صناديق** | `Make_Transaction_BanksBox_FromInvoicesMulti_` / `Edit_Transaction_BanksBox_NewSarfMulti_` | رأس رئيسى وأذونات فرعية لكل صندوق. |
| **حساب رصيد صندوق** | `GetBank_Balance` / `Compute_Box_BalanceBeforDataDay` | من الحركات (لا من العمود المخزَّن). |

### 1.3 آلية إنشاء القيد (`Accounts_Entry_POST_CLS.Make_Entry_For_AccountsSandouk`)
لكل إذن قيد واحد: **سطر الصندوق** (حساب `Banks.SubAccount_Id`) + **سطور الأطراف** من `Money_Transaction_Box`. الاستلام: الصندوق مدين والأطراف دائنة (اليومية 2)؛ الصرف: العكس (اليومية 3). المرجع `Reference_Type=Common.Sandouk_Account_Type` و`Reference_Type_Table_Id = Order_Money.Id`. القيمة المكافئة = القيمة × `Factor`. بعد النجاح: `Order_Money.IS_Posted_Flage=1` و`Account_Entry_List_ID`. الرقم من `AccountEntry_CLS.NewEntry_No`. (ملاحظة موثَّقة سابقاً: إدراج القيد المركزى لا يمرّر `Id` لرأس القيد — راجع `accounting-engine-deep-dive.md` — يُتحقَّق منه فى اختبار كل مرحلة.)

### 1.4 ما يقرره الأصل قبل الحفظ (تُنقل كما هى)
رقم الإذن فريد لكل (نوع، صندوق، تاريخ/سنة حسب الإعداد) عبر `Make_New_Order_No`/`Check_Order_NoFound`؛ صندوق مطلوب؛ طرف مطلوب؛ قيمة > 0؛ الصرف لا يتجاوز رصيد الصندوق إلا إن `Common.IsBoxBalanceAllowSarfAny=1`؛ عامل التحويل عند اختلاف عملة الصندوق عن عملة القيد (`CurrencyFactorsCLS.GetFactors`)؛ التفقيط `TafkeetProj.NumberToText`؛ الحفظ داخل معاملة `operations(1)` ثم `Commit`، والقيد فى معاملة ثانية فى الإذن أحادى الطرف.

### 1.5 خريطة قائمة «أوراق القبض» غير الشيكات (كل عنصر ← فورم ← ما نبنيه)
| عنصر القائمة | الفورم الأصلى | الصفحة/المرحلة |
|---|---|---|
| إذن إستلام نقدية من عميل / صرف نقدية لمورد / (مرتجع) من مورد / (مرتجع) لعميل — `menuItemCustomer_Money`، `menuItemSuplierMoney`، `menuItemSuplierMoneyReverse`، `menuItemCustomerMoneyReverse` | `OrderMoney_Transaction(type[,reverse])` | **ن1** صفحة واحدة `?type=1|2&reverse=0|1` |
| إذن إستلام/صرف نقديه (عام) — `ToolStripMenuItemEstlamMoney`/`SarfMoney` وزرا شريط الأدوات | `New_OrderMoney_SarfNew(type)` | **ن2** صفحة متعددة الأسطر |
| إذن إستلام/صرف نقديه بعملة — `xEstlamMoneyCurrency`/`ToolStripMenuItemSarfMoneyCurrency` | `New_OrderMoney_SarfCurrencyNew(type)` | ن2 (نفس الصفحة، عملة الحساب مختلفة) |
| إذن إستلام/صرف نقديه كل الخزن — `...ManyBox` | `OrderMoney_TransactionManyForAccounts(type)` | **ن4** |
| تحويل نقديه من صندوق إلى آخر — `toolStripMenuItemMoneyBetweenBanks` | `Money_Between_BoxTar7eelFrm` | **ن3** |
| بحث أذونات النقديه / مدفوعات نقدية من العملاء / إلى الموردين / حركة النقديه للحسابات | `Cash_Money_List` / `...FromCustomers` / `...ToSuppliers` / `...FromAccounts` | **ن5** قائمة واحدة بوضعين |
| توزيع سند قبض على الفواتير (+تقريره) | `Invoices_List_ReceiptDistribution(+Report)` | **ن6** (دراسة إضافية قبل التنفيذ) |
مستبعد: إذن إستلام/صرف شيكات ومتابعتها وتقاريرها وأوراق القبض الخاصة بها. شاشات المستشفى (`New_OrderMoney_SarfHospital`) وتذاكر السياحة (`SalesTicket_OrderMoney`) غير مطلوبة الآن.

### 1.6 قائمة «حركة الخزينة والبنوك» (كلها تقارير) — ن7
`Money_Transaction_BoxFrm` (حركة الخزينة اليومى)، `DaysMoney_Transaction_Box` (يومية حركة الصندوق)، `Money_Transaction_BoxPeriod` (حركة الخزينة عن فترة، عنصران)، `Money_Transaction_BoxTotalDay` (إجمالى يوميات الخزينة عن فترة)، `BanksMoneyTransaction`(+`Accounts`) (كشف حركة الخزينة عن يوم وتفصيلى). «يومية الصندوق» (`BanksMoneyTransactionAccounts`) **محوَّلة سابقاً** (`CashBoxDailyJournal.razor`) وتستعمل نفس دوال المحرك: `Money_Box_Transaction`، `Get_Money_Box_Transaction_Period*`، `Get_Money_Box_Transaction_PeriodTotalPerDay`، `Compute_Box_BalanceBeforDataDay`.

---

## 2) قرارات التصميم (اخترتُها بنفسى)
1. **لا تعديل** على `Money_Transaction_BoxOrderReportCLS` ولا `Order_MoneyCLS` ولا `Accounts_Entry_POST_CLS`؛ خدمة جديدة `CashVouchersService` (فى `AccountModelsLib/BusinessLibShared`) تلفّها وتضيف فقط: قائمة الصناديق (نفس استعلام `SandOuk_AccountList` مع فلتر الصلاحيات إن `IS_Used_SandoukWithPermission=1`)، ترقيم الإذن، قراءة الإذن للعرض/التعديل، استعلامات القوائم (نسخ للقراءة من `Order_Receipt_Cash` التى تسكن مشروع WinForms ولا يمكن مرجعتها من Razor).
2. **صفحة واحدة لكل عائلة**، بمعاملات بدل صفحات متعددة (كما فى الفواتير): ن1 أحادى الطرف، ن2 متعدد الأسطر (يشمل العملة).
3. **المعاملة:** كل حفظ داخل `operations(1)` مع Commit/Rollback كالأصل؛ الأخطاء تظهر برسالة لا تُبلَع.
4. **الطباعة (`OrderMoneyPrint` RDLC) وتصدير PDF/Excel/Word:** مؤجَّلة لمرحلة الطباعة (قرار سابق للتقارير).
5. **العربون (خطة الطلبيات):** يُبنى داخل ن1 (استلام من عميل): خانة اختيارية «ربط بطلبية عميل» تكتب صفاً فى `Customer_Order_Deposits` بعد نجاح الإذن، ويُعرض إجمالى العربون فى شاشة الطلبية.
6. **الاختبار الحى بعد كل مرحلة** على `ClientNew` بالعميل P1 وأحد الصندوقين، والتحقق من: `Order_Money`/`Money_Transaction_Box`، رصيد الصندوق من الحركات، حركة العميل ورصيده (من الحركات)، القيد ومجموع مدينه = دائنه، `IS_Posted_Flage`. ولا حذف لبيانات الاختبار.

---

## 3) المراحل
| المرحلة | الناتج | قبول المرحلة |
|---|---|---|
| **ن0 — الأساس** | `CashVouchersService` (صناديق، ترقيم، قراءة إذن، عامل عملة، رصيد صندوق) + مفاتيح ترجمة `CV*` | البناء ناجح؛ استعلام الصناديق يطابق الديسك توب |
| **ن1 — إذن نقدية من/إلى عميل أو مورد (4 عناصر) + العربون** | `Pages/Accounts/CashVoucherPartyPage.razor` (`/accounts/cash/voucher-party?type=&reverse=`) وربط القوائم الأربع | إذن استلام من P1: رصيد الصندوق ↑ والعميل ↓ (بالحركات) وقيد متوازن؛ صرف لمورد؛ العكسيان؛ عربون مربوط بطلبية |
| **ن2 — إذن نقديه متعدد الأسطر (عام/بعملة)** | `Pages/Accounts/CashVoucherPage.razor` (`/accounts/cash/voucher?type=`) + فتح/تعديل/حذف إذن قائم (صلاحية 316) | إذن بثلاثة أسطر (عميل + مورد + حساب مصروف بمركز تكلفة) والقيد متوازن؛ تعديل؛ حذف يعكس كل أثر |
| **ن3 — تحويل بين صندوقين** | `CashTransferPage.razor` | صندوق ↓ وصندوق ↑ بعامل التحويل، القيدان |
| **ن4 — عدة صناديق** | صفحة/وضع «كل الخزن» | رأس رئيسى وأذونات فرعية |
| **ن5 — قوائم الأذونات** | `CashVouchersList.razor` (بحث عام، من العملاء، إلى الموردين، للحسابات) مع فتح الإذن/الحذف | مطابقة عدد النتائج لاستعلام الأصل على نفس الفترة |
| **ن6 — توزيع سند القبض على الفواتير** | بعد دراسة `Invoices_List_ReceiptDistribution` | تُحدَّد عند الوصول |
| **ن7 — تقارير الخزينة (8 عناصر)** | صفحات تقارير بدوال المحرك | مطابقة أرصدة أول/آخر المدة لدالة `Compute_Box_BalanceBeforDataDay` |
| **ن8 — الطباعة والتصدير** | مؤجَّلة لمرحلة الطباعة المشتركة | — |

## 4) مخاطر ونقاط انتباه موثَّقة
- سندات مرحَّلة بقيد: التعديل/الحذف يمر بمحرك القيود (تحقق من عدم ترك أسطر `Account_Entry_Details` يتيمة).
- `Common.User_Id`/`Common.Branch_Id` ومتغيرات `Common` ثابتة (`static`) تُقرأ داخل المحرك — على خادم متعدد المستخدمين تُبلَّغ للمستخدم (قاعدة سابقة) ولا تُعدَّل.
- الرقم `Order_Money.Id`/الأسطر: توليد الأرقام حسب `Common.Use_Auto_Identity_Column` داخل المحرك (لا نتدخل).
- دوال قديمة تبتلع الاستثناءات (`catch {}`) فى بعض المسارات (`InsertOrderMoneyNew`): نتحقق من النتيجة من القاعدة بعد الحفظ ونعرض الخطأ.

---
## 5) سجل التنفيذ

### ن0 + ن1 — ✅ نُفِّذت واختُبرت حياً (2026-09-25، قاعدة ClientNew)
- **الكود:** `AccountModelsLib/BusinessLibShared/CashVouchersService.cs` (صناديق، ترقيم، رصيد، عامل عملة، تفقيط، `SavePartyVoucher` عبر `Make_Transaction_BanksBox_FromInvoices` بلا تعديل عليه، قراءة إذن) + `CustomerOrdersService.AddDeposit/GetDeposits/GetOrdersForDeposit` + `Pages/Accounts/CashVoucherPartyPage.razor` (`/accounts/cash/voucher-party?type=1|2&reverse=0|1`، ومعاملا `customer`/`order` لاستلام عربون من شاشة الطلبية) + 24 مفتاح ترجمة `CV*`/`CO*`. ربط القوائم الأربع (607 صرف لمورد، 608 استلام مرتجع من مورد، 609 استلام من عميل، 610 صرف مرتجع لعميل) بـ`RazorPath` مشفَّر.
- **العربون:** يظهر فى شاشة الطلبية (جدول الإذونات المرتبطة + زر «إضافة عربون» يفتح إذن الاستلام بالعميل والطلبية مسبقة الاختيار). العمود الجديد `Customer_Order_Deposits.Order_Money_Id` (رأس الإذن) هو مفتاح الربط؛ **`Money_Transaction_Box.Id` غير فريد فى القاعدة** (3078 صفاً و3045 معرّفاً مختلفاً) فلا يُعتمد عليه وحده.
- **نتائج الاختبار:**
  - استلام 50 من P1 (إذن 147): `Order_Money` مرحَّل، سطر `Money_Transaction_Box` (استيراد 50)، رصيد P1 −50، حركة `Customers_Accounts` نوع 3، يومية الصندوق `Banks_Balance` محدَّثة، **القيد 641 مدين / العميل دائن 50 متوازن (اليومية 2، المرجع 6)**.
  - استلام 30 من العميل 122 مربوط بالطلبية 9006 (إذن 148): نفس الأثر + صف عربون 30 وظهوره فى شاشة الطلبية.
  - صرف 10 للمورد P1 (إذن 110): رصيد المورد 3000.40→2990.40، حركة `Suppliers_Accounts` نوع 4، **القيد: المورد مدين 10 / الصندوق 641 دائن 10 متوازن**.
  - لم يُختبر حياً: نسختا المرتجع (تستدعيان نفس المسار بـ`Reverse_Order=1`).
- **ملاحظات:** (1) الرصيد المعروض للصندوق من الحركات (`Compute_Box_BalanceBeforDataDay`) يخالف `Banks_Balance` فى قاعدة التطوير (فرق تاريخى)؛ المحرك يخزّن `Bank_Balance` بالعمود `Banks.Last_Balance` كالأصل. (2) الطباعة مؤجَّلة.

### ن2 — ✅ إذن نقديه متعدد الأسطر (اختُبر حياً 2026-09-25): إنشاء وعرض؛ الحذف محجوب بحماية؛ التعديل مؤجَّل
- **الكود:** `CashVouchersService.GetCostCenters/AccountInfo/NewLinesTable/AddLine/SaveMultiVoucher/HasAmbiguousLineIds/DeleteVoucher` + `Pages/Accounts/CashVoucherPage.razor` (`/accounts/cash/voucher?type=1|2` للإنشاء و`?id=` للعرض/الحذف) تعيد استعمال `AccountSelector` (عميل/مورد/حساب آخر + رصيده). القوائم 612 (استلام) و613 (صرف) و596 (إذن استلام نقدية فى قائمة المبيعات، بمعامل `src=sales`) مربوطة.
- **الاختبار:** إذن صرف 111 بسطرين (العميل P1 5 والمورد P1 5) بمركز تكلفة 11: رصيد العميل +5 (حركة نوع 4) ورصيد المورد −5 (حركة نوع 4)، **القيد متوازن: العميل مدين 5 + المورد مدين 5 / الصندوق 641 دائن 10**، الإذن مرحَّل. العرض للقراءة يعمل (السطور والتفقيط).
- **اكتشاف مهم (تفسير بند 32):** `Common.GetTable_Id` يولّد `MAX(Id)+1` **لكل فرع** (`GROUP BY Branch_Id HAVING Branch_Id=…`)، فمعرّفات `Money_Transaction_Box` تتكرر عبر الفروع (أسطر إذن اليوم 36–40 تشارك معرّفاتها أسطراً قديمة من فرع آخر). ودوال الحذف/التعديل الأصلية تعمل بالمعرّف وحده. **لذلك حجبتُ الحذف فى Razor** عندما يتكرر معرّف أى سطر (`HasAmbiguousLineIds`) حتى لا يُحذف سطر إذن آخر؛ جُرِّب: الضغط على «حذف الإذن» للإذن 111 لم يحذف شيئاً. الحل الجذرى (قرارك): إما تعديل `GetTable_Id` ليكون فريداً عبر الفروع أو تكييف دوال الحذف/التعديل بشرط الفرع — كود مشترك مع الديسك توب.
- **مؤجَّل:** تعديل الإذن (`Edit_Transaction_BanksBox_NewSarf_`) بنفس الحماية، وصفحة إذن العملة (`SarfCurrencyNew`)، والطباعة.

## سجل التنفيذ — ن7 تقارير حركة الخزينة (2026-09-25)
- بُنيت `CashBoxStatement.razor` (`/accounts/cash/box-statement?interval=0|1[&layout=days]`) وتغطى: حركة الخزينة اليومى/عن فترة، ويومية حركة الصندوق (جدولا استلام/صرف). و`CashBoxTotals.razor` (`/accounts/cash/box-totals`) لتقرير إجمالى يوميات الخزينة (تفصيلى/إجمالى الفترة).
- خدمة `CashVouchersService`: `BoxStatement` و`BoxDays` و`BoxTotalsPerDay` تنادى المحرك القديم بلا تعديل.
- رُبطت القوائم 634-639، و617 و813 على `/accounts/cashbox-daily-journal` الموجودة.
- التحقق: مجاميع يوم 2026/09/25 (استلام 80، صرف 40) تطابق SQL؛ والرصيد الأول+الحركة=النهائى؛ وأرصدة الأيام تتسلسل فى التقرير الإجمالى.
- لم تُختبر: طباعة/تصدير (مؤجلة)، ونسختا «عن فترة» 637/638 تشتركان مع 634/636 فى نفس الصفحة.

## سجل التنفيذ — ن4 إذن «كل الخزن» (2026-09-25)
- بُنيت `CashManyBoxPage.razor` (`/accounts/cash/many-boxes?type=1|2`) وخدمة `CashVouchersService.SaveManyBoxVoucher` (تغلف `Make_Transaction_BanksBox_FromInvoicesMulti_` بلا تعديل). رُبطت القائمتان 927 و928. الصفحة تعرض الصناديق ومبلغ لكل صندوق وتفعّل زر الإصدار صحيحاً.
- **الحفظ متعثر بعيب فى المحرك القديم**: `Get_Order_Money` تنسخ حقولاً محدودة من رأس الإذن فيبقى `Day_Registration_flage` = NULL فى سطر Order_Money الفرعى، والعمود لا يقبل NULL (الخطأ: Cannot insert the value NULL into column 'Day_Registration_flage'). الفورم الأصلى يمر بنفس الدالة فيتعثر بالمثل على هذه القاعدة. الإصلاح مقترح: نسخ `Day_Registration_flage`(=0) و`DayReg_List_Id` و`Year_Financial_Period_Id` و`Is_Balances_Set` فى `Get_Order_Money` — يحتاج موافقة لتعديل كود المحرك المشترك.
- خطأ وُجد وأُصلح فى الرازور: تحويل `(int)Bank_Id` لمعرّف الصندوق الكبير (snowflake) كان يعيد null؛ استُعملت نسخة `long`.
- تحديث (بعد إصلاح المحرك بسطر `Day_Registration_flage`): الحفظ ينجح (إذنان فرعيان رقم 150، سطرا صندوق 43 و44، القيد 96949511812812800 مرحَّل).
- **لكن القيد خرج مشوّهاً**: `Make_Entry_For_AccountsSandoukMulti` تجمع السطور بمعرّف سطر الخزينة `Money_Transaction_Box.Id`، ومعرّفا السطرين الجديدين (43 و44) يتكرران مع سطرين قديمين (Order_Mony_Id=43 بمبلغ 293000 و44 بمبلغ 990) فدخلا القيد كدائن زائد (لا يتوازن). هذه نفس مشكلة عدم فرادة معرّف السطر (بند 32). القيد التالى ينبغى عدم الاعتماد عليه: 96949511812812800 (إذن 150).

## سجل التنفيذ — إذونات النقدية المرتبطة بالفواتير والمرتجعات (2026-09-26)
- **آلية الربط فى الأصل** (تأكدت من الكود والبيانات): يُخزَّن على سطر `Money_Transaction_Box` كل من `DataDictionaryProcessesListsId` (نوع العملية) و`Reference_Type_Table_Id` (رقم المستند) وكذلك `Invoice_Id`/`Invoice_TypeId`؛ الأنواع: 1 فاتورة مبيعات (Invoices_List)، 7 مرتجع مبيعات (Invoices_List_Reverse)، 2 فاتورة مشتريات (Bills_List)، 8 مرتجع مشتريات (Bills_List_Reverse). المدفوع = مجموع `Transaction_Value` لهذا الزوج. قبض العميل على فاتورة مبيعات يضيف صفاً فى `Money_Transaction_BoxForInvoices`.
- **المنفَّذ:** `CashVouchersService.SavePartyVoucher(..., docType, docId)` تمرر الزوج للمحرك بلا تعديل عليه؛ `GetDocForPayment` (الطرف والصافى والمدفوع والمتبقى) و`DocDirection` (اتجاه الإذن لكل نوع). صفحة `voucher-party` تقبل `?doc=<Id>&dtype=1|2|7|8`: تثبّت الطرف والاتجاه وتقترح المتبقى وتعرض بطاقة المستند وتحدّث المدفوع بعد الحفظ. زر «إذن نقدية» فى صفحتى الفاتورة (مبيعات/مرتجع، مشتريات/مرتجع) بعد الحفظ.
- **اختُبر حياً:** مرتجع مبيعات 213 (نوع 7، صرف لعميل، مدفوع 100)، فاتورة مبيعات 1676 (نوع 1، قبض 1089 + صف BoxForInvoices)، فاتورة مشتريات 1917 (نوع 2، صرف 30). لم يُختبر مرتجع المشتريات (نوع 8) لأن مرتجعاته الموجودة مسددة بالكامل، ومساره مطابق فى الكود لنوع 7 بالعكس.
- **إصلاحات حصلت أثناء الاختبار:** (أ) توليد معرّف سطر الخزينة صار فريداً على مستوى الجدول عند `Use_Auto_Identity_Column != 1` (`Common.GetTable_Id`) فلا يتكرر مع فروع أخرى — يعالج بند 32 ويسمح بالحذف والقيد الصحيح؛ (ب) خصائص نصية تقبل NULL فى `DataDictionaryDbSyncLogs` صارت `string?`؛ (ج) جدول `Money_Transaction_BoxForInvoices` أُعيد إنشاؤه بلا IDENTITY (بند 36).
- **متبقٍ:** تعديل السند؛ سند الصرف بعملة أجنبية؛ حذف السندات من Razor (يمكن إعادة تقييم الحماية بعد إصلاح المعرّفات: السندات القديمة ما زالت تحمل معرّفات مكررة)؛ زر «استلام نقدية» داخل شاشة مدفوعات الفواتير؛ توزيع سند القبض؛ الطباعة.
