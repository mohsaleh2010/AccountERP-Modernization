# خطة تحويل قائمة «الحسابات الختامية» (`ToolStripMenuItem_Reports`) إلى Razor

أُعدت فى 2026-10-31 بعد قراءة معالجات القائمة الحية فى `Accounts/AccountMainFrm/MainFrm_AccountUpdate.cs` وكود الشاشات نفسها وكلاسات الحساب.
مراجع: `accounting-engine-deep-dive.md` (المحرك)، `main-menu-screen-map.md` (القائمة)، `menu-main-account-razor-plan.md` (نمط التنفيذ والاختبار).

## 1) تصحيحات على الخريطة القديمة (من قراءة المعالجات الحية)
- «ميزان المراجعة ختامى بالمجاميع والأرصدة» يفتح فعلياً `Mezan_Morgaa_ByPeriodByAccountDeletePrevious(true)` وليس `ByPeriod`. أما «عن فترة بالمجاميع» فيفتح `Mezan_Morgaa_ByPeriod(false)`. الاثنان يستدعيان نفس المحرك `Mezan_MorgaaCLS.Get_Mezan_Morgaa_ByPeiodNew`.
- «بيانات أرصدة المواد عن فترة» (`menuItemFinancial_Period_Stores_Balance`): معالج الضغط **معلَّق بالكامل** فى الديسكتوب (الفورم فى مجلد `_1Old`)، أى أن العنصر لا يفعل شيئاً اليوم.
- «حساب التشغيل/المتاجرة/الأرباح والخسائر» نفس الفورم `Profit_Loss_StatFRMNew(1/2/3)`؛ زر «عرض» يحسب الثلاثة معاً، وأزرار الطباعة معطَّلة فى الكود.
- كل الشاشات تكتب/تقرأ جداول مؤقتة **عالمية** (`Account_Entry_Master_Balance_MezanTemp`, `..._TransactionTemp`) بـ`TRUNCATE` فى كل عرض ⇒ مستخدمان على الويب فى نفس اللحظة يفسدان نتائج بعضهما.

## 2) الدمج (16 عنصراً ⇒ 7 صفحات)
| الصفحة (مسار Razor) | عناصر القائمة (Name) | الأصل |
|---|---|---|
| `/accounts/final/trial-balance?mode=totals\|closing` | `ToolStripMenuItemMezanByPeriodMagame33`، `ToolStripMenuItem_Mezan_Morgaa`، والطباعتان `ToolStripMenuItemMezanByPeriodMagame3` و`...Magame3Arseda` | `Mezan_Morgaa_ByPeriod`، `...ByPeriodByAccountDeletePrevious`، `Mezan_Morgaa_ByMagameh*_RPT` (RDLC لنفس البيانات) |
| `/accounts/final/ledger?mode=fiscal\|range` | `toolStripMenuItemMaster_Account_DetailsTarseed`، `ToolStripMenuItemMasterAccount` | `Master_Account_DetailsTarseed22`، `Master_Account_Details2` |
| `/accounts/final/cost-center-ledger` | `ToolStripMenuItemMasterCostCenter`، `ToolStripMenuItemMasterCostCenter1` (القديم) | `Master_Account_CostCenter_TarsesdFRM` (الحى) و`..._FRM` (قديم يُدمج) |
| `/accounts/final/income-statement` | `toolStripMenuItemIncoming_Statement` | `Incoming_StatementByDate` |
| `/accounts/final/profit-loss?tab=1\|2\|3` | `ToolStripMenuItemTash7heelAccount`، `ToolStripMenuItemMotagra`، `ToolStripMenuItemArba7Wa7hsa2r` | `Profit_Loss_StatFRMNew` |
| `/accounts/final/balance-sheet` | `toolStripMenuItemAccountBalance` | `Get_Account_BalanceFRM2` + `BalanceCLS.GetBalanceData_WithLevel` |
| `/accounts/final/chart-print` | `toolStripMenuItemPrintTreeAccount` | `Submain_AccountPrintFrm` (RDLC لشجرة الحسابات) |
| — (مؤجَّل، «قيد الإنشاء») | `menuItemFinancial_Period_Stores_Balance` | معالجه معلَّق فى الأصل؛ الجرد الدورى للأصناف تغطيه `/store/opening-balances` |

## 3) القرارات المعمارية
1. **خدمة واحدة مشتركة** `FinalAccountsService` فى `AccountModelsLib/BusinessLibShared/GeneralLedger/` (نفس نمط `EntryPostingService`) تغلّف الكلاسات الحالية (`Mezan_MorgaaCLS`، `Incoming_StatementCLS`، `BalanceCLS`، `AccountEntry_CLS`) **بلا إعادة كتابة للمنطق** (لضمان تطابق الأرقام مع الديسكتوب) وتعيد صفوفاً مكتوبة النوع للصفحات.
2. **قفل عالمى** (`SemaphoreSlim(1,1)`) حول كل استدعاء يلمس الجداول المؤقتة، حتى لا يفسد مستخدمان الويب نتائج بعضهما. لا يحمى من الديسكتوب المفتوح بالتوازى (يُوثَّق).
3. **تحقق التطابق**: لكل شاشة، اختبار حى على `ClientNew` يقارن مجاميع الخدمة بمجاميع الاستدعاء المباشر للـCLS لنفس الفترة والفرع والعملة (يُسجَّل فى هذا المستند).
4. **قراءة فقط أولاً**. عمليات الكتابة فى الميزان («ترحيل الرصيد الختامى للفترة التالية» صلاحية 317، «تحديث رصيد أول المدة» صلاحية 318) تعيد كتابة `Account_Entry_Master_Balance` ⇒ **تُؤجَّل لحين قرارك** (قاعدة عدم تعديل البيانات القائمة). تظهر الأزرار معطَّلة بتلميح.
5. **الطباعة**: بدل RDLC، زر «طباعة» يستعمل CSS للطباعة (`@media print`) + تصدير Excel حيث وُجد فى الأصل. هذا يعوّض أيضاً الطباعة المعطَّلة فى «التشغيل/المتاجرة/الأرباح».
6. **الصلاحيات**: كل صفحة تحترم صلاحية عنصر القائمة نفسه (`PERMATIONS`)، والأزرار الحساسة بأرقامها الأصلية.
7. **الربط بالقوائم**: إضافة مفاتيح العناصر (Name) إلى `StockEngineSchemaCLS.StoreOrderMenus` (تملأ `RazorPath` الفارغ فقط). تبدأ كل صفحة بالتنفيذ ثم تُربط.
8. **تصميم الواجهة**: أنماط المشروع (MudBlazor، `inv-card`، RTL، الأرقام العشرية الثابتة) وتطبق مهارة ui-ux-pro-max كما فى الشاشات السابقة.
9. **نقاط خطر معروفة نتركها كما هى ونوثّقها**: ثبات `Account_Id = 3` لحقوق الملكية فى `BalanceCLS` (يُراجَع فى م5)، وغياب أى قيد إقفال فعلى (الربح يُحسب فى الذاكرة).

## 4) المراحل (مهمة واحدة لكل مرحلة، ثم commit وتحديث هذا المستند)
- **م0** الخدمة `FinalAccountsService` + القفل + عناصر المصدر المشتركة (قائمة الفترات المالية والعملات والفروع).
- **م1** ميزان المراجعة (الوضعان + الطباعة) وسحب الأستاذ بالنقر المزدوج (يعتمد على م2، يبدأ كرابط).
- **م2** الأستاذ المساعد (الوضعان).
- **م3** أستاذ مراكز التكلفة.
- **م4** قائمة الدخل + التشغيل/المتاجرة/الأرباح والخسائر.
- **م5** قائمة المركز المالى (مع مراجعة ثبات الحساب 3).
- **م6** طباعة الشجرة + صفحة «قيد الإنشاء» لأرصدة المواد + ربط القوائم النهائى + اختبارات المستخدم فى `master-user-test-plan.md`.

## 5) سجل التنفيذ
### م0 + م1 — 2026-10-31 (كوداً، ينتظر اختبار المستخدم م-185…م-191)
- `FinalAccountsService` (AccountModelsLib/BusinessLibShared/GeneralLedger): قفل عالمى، قوائم الفترات والفروع والعملات (-1 = بالمكافىء)، `ValidateRange` بنفس تحققات الديسكتوب، و`GetTrialBalance` يستدعى `Get_Mezan_Morgaa_ByPeiodNew` بنفس معاملات الديسكتوب (Is_To_Show_Prev = بداية البحث بعد بداية الفترة) ويجمّع المجموعات والإجمالى.
- الصفحة `/accounts/final/trial-balance` (`TrialBalancePage.razor`) بوضعين، وطباعة بـ CSS، ومفاتيح لغات الأربع (FA*/TB*).
- ربط القوائم الأربع فى `StoreOrderMenus` (تُطبَّق عند إقلاع الموقع).
- مؤجَّل بقرارك: «ترحيل الرصيد الختامى» (317) و«تحديث رصيد أول المدة» (318). والنقر المزدوج لفتح الأستاذ يضاف مع م2.
- لم يُختبر حياً بعد (الاختبار على المستخدم حسب قاعدة عدم الاختبار فى المتصفح دون طلب).

### م2 — 2026-10-31 (كوداً، ينتظر اختبار المستخدم م-192…م-199)
- `FinalAccountsService`: `GetJournals` و`GetAccountTypes` و`GetLedgerAccounts` (قائمة الحسابات الورقية بنفس `Get_MasterAccount_Entry_List` بعلامة «بحركات» False كما فى الديسكتوب) و`GetLedgerRange` (يستدعى `Get_MasterAccount_Entry_Details_Tarseed_NoPeriodNew`) و`GetLedgerFiscal` (يستدعى `Get_MasterAccount_Entry_Details_TarseedNew` مع `Get_Year_Financial_Period_Id`)، دون تعديل المنطق.
- الصفحة `/accounts/final/ledger` (`SubLedgerPage.razor`) بوضعين: `mode=range` («عن فترة»، تاريخان حران + إضافة الرصيد اللاحق) و`mode=fiscal` («بالفترة المالية»، اختيار الفترة يملأ التاريخين). النقر المزدوج على سطر القيد يفتحه (صلاحية 808 ⇒ مدين/دائن). الطباعة بـ CSS. مفاتيح لغات `LG*` بالأربع لغات.
- ربط القائمتين فى `StoreOrderMenus`: `ToolStripMenuItemMasterAccount` ⇒ range، `toolStripMenuItemMaster_Account_DetailsTarseed` ⇒ fiscal.
- ملاحظة: الأستاذ المساعد فى الديسكتوب يعرض قائمة حسابات فى شبكة؛ هنا استُبدلت بحقل بحث تلقائى (نفس البيانات) لأنه أنسب للويب.
- مؤجَّل: النقر المزدوج من الميزان (م1) ليفتح الأستاذ لحسابه.
- لم يُختبر حياً (الاختبار على المستخدم).
- تصحيح: خيار «إضافة الرصيد السابق» (`cbAddBalanceAfter`) يُمرَّر كمعامل `IS_AddLastBalance` ولا يُقرأ داخل `Get_MasterAccount_Entry_Details_Tarseed_NoPeriodNew`، فلا أثر له فى الديسكتوب؛ حُذف من الصفحة (كان اسمه خطأً «اللاحق»). الرصيد السابق يُنقل دائماً.
- إعادة تصميم (بعد دراسة `Master_Account_DetailsTarseed22`): شبكتان كالديسكتوب (الحسابات التى لها حركات ⇒ حركة الحساب المختار)، خيار «عرض الحسابات التى لها حركات» (فى وضع الفترة المالية فقط؛ وضع «عن فترة» يعرض كل الحسابات كالأصل)، قائمة الفترة باسمها والنشطة افتراضياً، تتبّع الفترة لتاريخ البداية، اختيار الحساب ببحث مباشر + شجرة (`AccountSelector`) مع عرض حركته تلقائياً، وبحث فى البيان، وF8. اختبارات م-200…م-203.

### م4 — 2026-11-12 (كوداً، ينتظر اختبار المستخدم م-317…م-324)
- `FinalAccountsService`: `GetIncomeStatement` (يملأ المحرك الجدول المؤقت بـ`Get_Mezan_Morgaa_ByPeiodNew` بنفس معاملات `ShowData` فى الديسكتوب: Is_To_Show_Prev=False وكل الحسابات، ثم `Incoming_StatementCLS.Get_Incoming_Statement` بالحسابات ذات الرصيد) و`GetProfitLoss` (Is_To_Show_Prev = بداية البحث بعد بداية الفترة كما فى `GetTOShowData`، ثم `Get_Final_AccountsArba7` يحسب التشغيل والمتاجرة والأرباح معاً). الاستدعاءان داخل القفل نفسه لأن القراءة من الجدول المؤقت العالمى. `BuildStatement` يحوّل صفوف المحرك (-20 رأس مجموعة، -10 إجمالى، بلا معرّف = مرحَّل) إلى صفوف مكتوبة النوع، ويربط النصوص الثابتة فى المحرك بمفاتيح ترجمة (ISRevenueTotal… PLToBS).
- الصفحتان `/accounts/final/income-statement` (`IncomeStatementPage.razor`: بطاقات إجمالى الإيرادات/المصروفات/صافى الربح أو الخسارة + الجدول) و`/accounts/final/profit-loss?tab=1|2|3` (`ProfitLossPage.razor`: ثلاثة تبويبات، «عرض» يحسب الثلاثة معاً كالأصل). مكوّنان مشتركان: `FinalStatementTable` و`FinalStatementStyles`. الطباعة بـ CSS (كانت معطَّلة فى الديسكتوب للتشغيل/المتاجرة/الأرباح). مفاتيح لغات IS*/PL*/FSFirstBalanceHint بالأربع لغات.
- ربط القوائم الأربع فى `StoreOrderMenus`: `toolStripMenuItemIncoming_Statement`، `ToolStripMenuItemTash7heelAccount` (tab=1)، `ToolStripMenuItemMotagra` (tab=2)، `ToolStripMenuItemArba7Wa7hsa2r` (tab=3).
- **قرار (يُسجَّل):** الشاشتان فى الديسكتوب تستدعيان `UpdateFirstBalance` قبل كل عرض (يحذف ويعيد بناء `Account_Entry_Master_Balance` للفترة والفرع). لا يُستدعى فى الويب (القرار §3.4 ونفس ميزان المراجعة): العرض للقراءة فقط وتنبيه فى الصفحة. السبب: قاعدة عدم تعديل البيانات القائمة، واتساق أرقام صفحات الويب مع بعضها.
- **مطابقة الديسكتوب حرفياً:** قائمة الدخل تستدعى المحرك بـ Is_To_Show_Prev=False حتى لو كانت البداية بعد بداية الفترة (كما فى الأصل)، والتحقق من التاريخين بـ`ValidateRange` (أشد قليلاً من الأصل الذى يفحص النهاية فقط، كميزان المراجعة).
- مؤجَّل: النقر المزدوج على حساب لفتح الأستاذ (موجود فى الديسكتوب لقائمة الدخل) — يُضاف مع سحب الميزان لأن صفحة الأستاذ لا تقبل حساباً فى الرابط بعد.
- لم يُختبر حياً (بناء الويب 0 أخطاء؛ الاختبار على المستخدم).

### م5 — 2026-11-12 (كوداً، ينتظر اختبار المستخدم م-325…م-332) — الخطة والقرارات بوكيل Fable 5.2
- الصفحة `/accounts/final/balance-sheet` (`BalanceSheetPage.razor` + مكوّن `BalanceSheetSectionTable`) بدل `Get_Account_BalanceFRM2`، وربط `toolStripMenuItemAccountBalance` فى `StockEngineSchemaCLS`. مفاتيح BS* و`FAErrBranchRequired` بالأربع لغات.
- `FinalAccountsService.GetBalanceSheet(period, branch)`: داخل القفل يملأ المحرك MezanTemp بنفس معاملات الديسكتوب (الفترة كاملة، Is_To_Show_Prev=False، العملة **0** = المكافىء كما يمرر الديسكتوب، لا -1)، ثم يقرأ حسابات الميزانية (`Account_Follows = 4`) بنفس شرط `BalanceCLS`، وصافى الربح من `Get_ProfitFrom_Incoming_Statement(branch, period, 1, 0)`. `BuildBalanceSheet` (دالة نقية) تبنى الشجرة فى C#.
- **قرار 1 (لا `UpdateFirstBalance`):** §3.4 كم4. تنبيه فى الصفحة بعدد حسابات الميزانية التى لها قيود مرحَّلة فى الفترة والفرع بلا صف فى `Account_Entry_Master_Balance` (عملة 0) — فى قاعدة التطوير الفترة النشطة فيها 15 صفاً فقط بالعملة 0، فيُتوقَّع التنبيه حتى تُفتح الشاشة فى الديسكتوب مرة.
- **قرار 2 (لا `BalanceCLS.GetBalanceData_WithLevel` على الويب):** يفرّغ جدولاً عالمياً ثانياً (`AccountTempBalance`) وينفّذ `ALTER TABLE` كل مرة ويبتلع الأخطاء، ويكرر صفوف الأب لكل SLevel، ويجمع مدين+دائن الأبناء بالقيمة المطلقة (أب له ابن مدين 100 وابن دائن 30 يظهر 130). الويب: صافى الحساب = مدين − دائن والأب = مجموع صافى أبنائه. أرقام الحسابات الفرعية مطابقة للديسكتوب؛ إجماليات الآباء قد تختلف (خطأ فى الديسكتوب، لم يُعدَّل).
- **قرار 3 (ثبات الحساب 3):** `BalanceCLS` يبقى كما هو للديسكتوب. التحقق من القاعدة: الحساب 3 فعلاً جذر حقوق الملكية (Head_Main_Account=3، مستوى 1). الويب لا يعتمد على الرقم: القسم = `Head_Main_Account` لجذر الحساب (1 أصول، 2 خصوم، 3 حقوق ملكية، غيرها ⇒ أصول)، وحقوق الملكية قسم مستقل بدل نقلها تحت الخصوم.
- **قرار 4:** صافى الربح سطر أخير فى حقوق الملكية وداخل إجماليها (الديسكتوب يضيفه بعد التجميع فلا يدخل إجمالى الأب)، مع شريط اتزان (الأصول = الخصوم + حقوق الملكية، والفرق). الأصول بطبيعة مدينة، الخصوم وحقوق الملكية بطبيعة دائنة، والرصيد العكسى بين قوسين. «مستوى العرض» = العمق من الجذر (الإجماليات لا تتغير)، والصفرى مخفى إلا بخيار.
- **نقطة للمراجعة (لم تُعدَّل):** صفحات م3/م4 تمرر -1 لـ«بالمكافىء» بينما هذه الشاشة فى الديسكتوب تمرر 0؛ يُراجَع ما يمرره اختيار «بالمكافىء» فى ديسكتوب الميزان/قائمة الدخل.
- لم يُختبر حياً (بناء الويب 0 أخطاء، واستعلاما الأوراق والتنبيه مجرَّبان على القاعدة).

### م6 — 2026-11-12 (كوداً، ينتظر اختبار المستخدم م-333…م-337) — الخطة والقرارات بوكيل Fable 5.2 (`financial-statements-review-plan.md` ف1)
- `/accounts/final/chart-print` (`ChartOfAccountsPrintPage.razor`) بدل `Submain_AccountPrintFrm`: `FinalAccountsService.GetChartOfAccounts(branch, includeShared)` قراءة فقط + `BuildChart` (دالة نقية: الأب، الجذر، العمق من الشجرة، فرعى = بلا أبناء). الأعمدة: الرقم، الاسم بإزاحة، المستوى، الأب، التصنيف الرئيسى (اسم الجذر)، الحساب الختامى، رئيسى/فرعى، العملة (`Currency.Value = Currency_Id`). المرشحات: الفرع، حتى مستوى، التصنيف، الحساب الختامى، الفرعية فقط، المشتركة، بحث. طباعة A4 بتكرار رأس الجدول وترويسة بالمستخدم والتاريخ.
- **قرار:** تصحيح خطأى الأصل: مرشح المستوى كان مهملاً (`get_TreeToPrint` يستبدل الاستعلام)، ومرشح الفرع كان يُسقط المشتركة (Branch_ID = -1)؛ المشتركة ظاهرة افتراضياً مع مفتاح لإخفائها. Excel مؤجَّل لبند 2.8.
- `/accounts/final/stores-period-balance` (`StoresPeriodBalancePage.razor`): صفحة «قيد الإنشاء» (الأصل معلَّق فى الديسكتوب) برابط `/store/opening-balances`.
- ربط `toolStripMenuItemPrintTreeAccount` و`menuItemFinancial_Period_Stores_Balance` فى `StoreOrderMenus` ⇒ كل عناصر «الحسابات الختامية» الـ 15 مربوطة. مفاتيح CP*/SPB* بالأربع لغات.
- 317/318 ما زالا مؤجَّلين؛ تصميمهما الصحيح فى `financial-statements-review-plan.md` ب-4 (ف6).
- بناء Web وAccountModelsLib 0 أخطاء. لم يُختبر حياً (الاختبار على المستخدم).
- **التالى:** ف2 (عملة «بالمكافىء» = 0) من `financial-statements-review-plan.md`.

### ف2 (مراجعة القوائم) — 2026-11-12 (كوداً، ينتظر اختبار المستخدم م-338…م-341)
- `FinalAccountsService.EquivalentCurrencyId` = **0** بدل -1 (و`BaseEquivalentCurrencyId` يشير إليه). تُصلح الصفحات الخمس (الميزان، الأستاذ المساعد، أستاذ المراكز، قائمة الدخل، التشغيل/المتاجرة/الأرباح) لأنها تبدأ به افتراضياً. يغلق «نقطة المراجعة» المفتوحة فى م5.
- التحقق: كل الشاشات الختامية فى الديسكتوب (ومنها `Master_Account_CostCenter_TarsesdFRM`) تستدعى `Currency_Data_AllItems("بالمكافىء")` بمعامل واحد ⇒ القيمة "0"؛ المحرك يتعامل مع `>0` و`==0` فقط. `UpdateFirstBalanceCostCenter` يعامل -1 كـ«كل العملات» و0 كالمكافىء — الآن يطابق الديسكتوب.
- بناء الويب 0 أخطاء. لم يُختبر حياً.
- **التالى:** ف3 (محرك الأرصدة للقراءة + الميزان عليه).
