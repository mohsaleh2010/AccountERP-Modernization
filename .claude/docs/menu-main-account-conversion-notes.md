# خطة تحويل قائمة "الحسابات" (menuMainAccount) إلى Razor — ملاحظات الجلسة

**الحالة:** اكتملت مرحلة البحث والتوثيق (5 من 5 مجموعات فرعية). لم يُكتب أي كود Razor حتى الآن لهذه المجموعة — الخطة التفصيلية للتنفيذ في [menu-main-account-razor-plan.md](menu-main-account-razor-plan.md)، بانتظار موافقتك قبل بدء الكود.

**قرار من المستخدم بخصوص "الإهلاك" (menuItemDestruction):** الشاشة غير مكتملة أصلاً في الويندوز فورم (الكود المرتبط بها محذوف/معلّق بالكامل، والكلاس المشار إليه `Assets_EhlakFrm` غير موجود في المشروع، والكلاس اليتيم البديل `EhlakFixesAssetFrm` غير مكتمل ولم يُستخدم أبدًا). **القرار: تخطّيها حاليًا ووضع صفحة Razor بعنوان "قيد الإنشاء" (Under Construction) بدلاً من محاولة استنتاج منطق عمل غير موجود.**

---

## خلفية الجلسة (بالترتيب الزمني)

1. **تصحيح توثيق خاطئ في `CLAUDE.md` و `.claude/docs/screen-navigation.md`**: كانت الوثائق تفترض أن الفورم الحقيقي `MainFrm_AccountUpdate` (القائمة الرئيسية الفعلية للتطبيق) موجود في مشروع `Advanced_Account`. تم التحقق مباشرة من ملفات `.csproj` وثبت أن ذلك خاطئ:
   - لا `AccountsERPSystem.csproj` ولا `Accounts.csproj` يشير إلى `Advanced_Account` بأي `ProjectReference`.
   - النسخة الحية الفعلية المُجمَّعة داخل البرنامج هي: [`Accounts/AccountMainFrm/MainFrm_AccountUpdate.cs`](../../Accounts/AccountMainFrm/MainFrm_AccountUpdate.cs) + [`.Designer.cs`](../../Accounts/AccountMainFrm/MainFrm_AccountUpdate.Designer.cs) (نفس الـ namespace `Accounts`، لكن مشروع مختلف).
   - نسخة `Advanced_Account\MainFrm_AccountUpdate.cs` (10,623 سطر) هي نسخة ميتة/مكررة غير مُستخدمة إطلاقًا في التطبيق الحي.
   - تم تحديث الملفين ليعكسا هذه الحقيقة.

2. **بناء خريطة القائمة الكاملة**: [`.claude/docs/main-menu-screen-map.md`](main-menu-screen-map.md) — تحليل برمجي (Python) لملف الـ Designer (بترميز Windows-1256 الصحيح للعربية) استخرج:
   - 620 عنصر قائمة (`ToolStripMenuItem`)، 67 مجموعة فرعية، 19 قائمة رئيسية.
   - 477 إجراء ورقة (leaf action) تم ربطه بنجاح بالفورم الهدف عبر تتبع `Name` → `Click` handler → `new SomeForm(...)`.
   - 30 عنصر بدون فورم هدف واضح (يُفوّض لدالة مساعدة أو معطّل بصلاحيات)، و3 عناصر بدون جسم handler موجود.

3. **حفظ ذاكرة دائمة** (`locate-winforms-screen-via-mainmenu`): توثيق منهج تتبع القوائم كأسلوب معياري لأي تحويل مستقبلي.

---

## المرحلة الحالية: دراسة قائمة "الحسابات" (menuMainAccount) الفرعية

بناءً على طلبك، تم تقسيم قائمة "الحسابات" إلى 5 مجموعات بحثية موازية (subagents). النتائج:

### ✅ مكتمل (4 من 5)

#### 1. اليوميات وحركة المخازن (Journals & Store Movement)
7 شاشات، جميعها موثّقة بالكامل: يومية الصادر والوارد (`Accounts.DaysWorkShow.DaysWorkSkow`)، يومية مصروفات (`Reporting_System.Money_SpentReports`, ctor `(1)`)، يومية الصندوق (`Account_Common_LIB.New_Money_Transaction.BanksMoneyTransactionAccounts` — ملاحظة: الكود القديم `Money_Transaction_Box(false)` معطّل/ميت)، وأربع شاشات حركة مخازن تتقاسم كلاسين فقط (`Store_LIB.ItemsCardsStore` و`Store_LIB.ItemsCardsStore_Days`) بفارق باراميتر الـ constructor فقط (1=مبيعات، 2=مشتريات). **تنبيه مهم**: `Stores.csproj` غير مُشار إليه إطلاقًا من `Accounts.csproj` — أي نسخ مطابقة تحت مجلد `Stores\` هي كود ميت خارج النطاق.

#### 2. الأصول الثابتة (Fixed Assets)
3 عناصر: "دليل الأصول الثابتة" (`Account_Common_LIB.FixesAsset_TreeFrm` — شجرة حسابات + لوحة تفاصيل ~20 حقل + معرض صور)، "حساب الإهلاك" (`Assets_EhlakFrm_` مع underscore زائدة — يطبّق فعليًا طريقتي القسط الثابت والمتناقص بمعادلات موثقة بالكامل)، و"الإهلاك" (`menuItemDestruction`) **معطّل تمامًا** — الكود محذوف بالكامل (comment) ويشير لكلاس `Assets_EhlakFrm` (بدون underscore) غير موجود في المشروع نهائيًا. يحتاج قرار عمل: تجاهله أو استيضاح الغرض منه من فريق العمل.

#### 3. كشوف حسابات العملاء والموردين (Customer/Supplier Statements)
10 عناصر. أهم استنتاج: مفهوم "الحساب المختلط" (Mokhtalat) مثبت بالكود — `Supplier.Ref_Cust_Supplier_Id` يربط مورد بعميل هو نفس الطرف التجاري، وتُدمج الحسابات عبر UNION query. تم تأكيد أن `menuItemCustomerAccountBalanceDatesBefor` عنصر قائمة **معطّل فعليًا وليس خطأ استخراج** (لا `.Click` مربوط إطلاقًا). الشاشات الأحدث (`Customer_AccountMokhtalat_EF` و`...EF_Period`) تستخدم `DataGridView` حقيقي (أفضل مرجع للتحويل) بعكس البقية التي تستخدم `PrintableListView` القديم.

#### 4. إشعارات التسوية المالية (Financial Settlement Notices)
6 عناصر تتلخص في نمطين فقط: (أ) تسوية عامة بدون صنف (`Taswya_Money` + `Taswya_Money_Review`، كلاس `Money_Financial_NoticeCLS`)، (ب) تسوية بصنف واحد أو عدة أصناف — **نفس الكلاس التجاري تمامًا** (`Money_Financial_Notice_ItemsCLS`) بفارق دالة overload فقط (صف واحد مقابل DataTable متعدد الصفوف). التوصية الصريحة من البحث: بناء مكوّن Razor واحد فقط لحالة "بصنف" بدلاً من شاشتين منفصلتين، وواجهة مراجعة واحدة بدلاً من اثنتين متطابقتين فعليًا.

### ✅ الفترات المالية والعملات ومراكز التكلفة (المجموعة الخامسة — أُنجزت بعد إعادة المحاولة)

7 عناصر، أبرز النتائج:
- **العمـلات**: الشاشة الحية `CurrencyListFrmNew` (ليست `CurrencyListFrm` القديمة — معلّقة/ميتة) — **الأسهل تحويلاً على الإطلاق**: الكلاس `CurrencyCLS` مكتوب بالفعل بـ EF Core async نظيف بدون أي SQL خام، صالح للاستدعاء المباشر من Blazor كما هو.
- **سعر العملة**: شاشة قديمة (ADO.NET خام + `SqlDataAdapter.Update`)، تُنشئ تلقائيًا صفين إضافيين "self-factor" (Factor=1) عند إضافة أي زوج عملات — قاعدة عمل يجب الحفاظ عليها.
- **الفترات المالية**: `Financial_PeriodFrm` + `Financial_PeriodCLS`، بسيطة نسبيًا (قائمة + تنشيط/حذف بشرط عدم وجود قيود مرتبطة).
- **أرصدة أول المدة**: الأعقد — تعتمد على محرك ميزان المراجعة المشترك `Mezan_MorgaaCLS` (~2100 سطر) + `FirstBalance_CLS` لشاشة التعديل الفرعية (`FirstBalance_Financial_Period_FRM`، اتضح أنها نافذة تعديل فرعية وليست شاشة بديلة كما كان يُظن).
- **قائمة اليوميات** و**مراكز تكلفة الحسابات**: مكتشَف أن كلا الكلاسين مُعرّفان فعليًا تحت `namespace General_Ledger` رغم أن ملفاتهما فعليًا داخل `Accounts`/`Account_Common_LIB` — خطأ تسمية قديم يجب توخي الحذر منه عند البحث في الكود لاحقًا.
- **مراكز التكلفة (الشجرة)**: نفس شكل شجرة الحسابات تمامًا (`Parent_Id` ذاتي المرجعية) — **فرصة إعادة استخدام مباشرة لمكوّن `AccountTreePicker.razor`** الموجود بالفعل، بعد تعميمه ليقبل مصدر بيانات مختلف (`CostCenterCLS.Get_CostCenter()` بدل حسابات الأستاذ العام).

**جدول الملخص الكامل والمسارات الدقيقة لكل ملف محفوظ في نتائج البحث — راجع الرسالة الكاملة للـ agent الخامس إذا احتجت التفاصيل الحرفية.**

---

## الخطوة التالية

تم الانتهاء من مرحلة البحث بالكامل (5/5). الخطة التفصيلية للتنفيذ (أي شاشة تُبنى كمكوّن جديد، أيها يُدمج مع غيره بدل التكرار، وترتيب الأولوية المقترح) موجودة في: [menu-main-account-razor-plan.md](menu-main-account-razor-plan.md) — بانتظار موافقتك عليها قبل كتابة أي كود.
