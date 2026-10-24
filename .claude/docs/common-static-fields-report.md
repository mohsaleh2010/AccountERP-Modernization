# تقرير حقول `Common` الساكنة بعد عزل `UserSessionContext` (ز3، 2026-10-24)

تقرير فحص فقط — لم يُعدَّل أى كود. الغرض: أى حقل ثابت فى `Common` ما زال يُكتب أثناء التشغيل (لا عند الإقلاع) فى الويب فيمكن أن يتسرب بين المستخدمين المتزامنين.

## الطريقة
- حصر الأعضاء الثابتة القابلة للتعديل فى `Common.cs` و`Common2.cs` (حذف `const`/`readonly`/الدوال): **508 عضواً**.
- المعزول بجلسة المستخدم (`UserSession`): 7 فقط — `User_Id`، `Branch_Id`، `User_Name`، `FullUserName`، `Is_User_Admin`، `IsAdministration_AlterDBLogin`، `Is_Force_Admin_Permission`.
- البحث عن كل إسناد `Common.X = / += / ++` فى المستودع (بلا `GHospital` و`MudBlazorCrmApp-main`)، وعن القراءات من كود يصل إليه الويب (`AccountModelsLib`، `Razors`، `AccountsERPSystemNet10`).
- حدود الفحص: الإسناد غير المؤهل داخل `Common.cs` نفسه لم يُحصَ إلا لدوال التحميل؛ مطابقة نصية فتدخل فيها بعض التعليقات؛ كلاسات ساكنة أخرى غير `Common` (مثل `CommonAccounts`) لم تُفحص.

## النتيجة
**لا يوجد حقل ثابت خاص بالمستخدم يُكتب من مسار الويب غير الحقول السبعة المعزولة.** 55 عضواً يُسند لهم خارج الحقول المعزولة، وتصنيفها:

### 1) إعداد عملية/قاعدة يُكتب عند الإقلاع — آمن
`IS_ReadConStrFromXMLInfo`، `DTAdmin`، `InstituteNameFromInfo`، `VersionWillExpire`، `dtExpire`، `RenewExpireCount`، `SystemInfo`، `Is_Ecrypted_Config`، `Is_Ecrypted_Menu`، `Project_ID`، `Project_Module_Str`، `IsInDesignAndDevelopmentTime`، `CommonTaxConfig`، `Use_Auto_Identity_Column`، وما شابهها. قيمتها واحدة لكل العملاء المتصلين بنفس القاعدة (يكتبها `SystemSettingsLoader` و`Program.cs` والمحمّلات).

### 2) مكتوبة أثناء التشغيل لكن قيمتها للقاعدة كلها لا للمستخدم — سباق غير ضار
| الحقل | أين يُكتب | لماذا لا يضر |
|---|---|---|
| `Year_Financial_Period_Id` | `Financial_PeriodCLS.Active_Period` | الفترة المالية النشطة واحدة للقاعدة؛ آخر كتابة هى الواقع |
| `Customer_Type` | `CustomerCLS` (تهيئة عميل جديد) يعيدها إلى 1 | لا شىء فى الويب يغيّرها عن 1؛ الإسناد مجرد إعادة للافتراضى |
| `TaxSales_Rate`، `ATS05_Sales_Rate` | `Common.Get_Tax_Rate()` | نسب الضريبة من قاعدة البيانات، نفس القيمة لكل المستخدمين |
| `Account_Type_ForShippmentCompanyAsServicesSupplier` | (قراءة فقط فى صفحة الربط؛ ظهر كإسناد بالخطأ) | — |
| `is_pharmacy` | (إشارة فى تعليق بنافذتى الصنف؛ لا إسناد فعلى) | — |

### 3) حالة شاشات WinForms تُقرأ من كود مشترك — خطر وظيفى لا تسرب
هذه تُكتب من شاشات الديسكتوب فقط (حسب اختيار المستخدم فى الشاشة)، ويقرؤها كود مشترك يصل إليه الويب، فيرى الويب قيمة الإقلاع/القاعدة دائماً:

| الحقل | قراءات من كود الويب | إسنادات من WinForms |
|---|---|---|
| `Project_Market` | 56 | 1 |
| `Balance_BeginingPeriod` | 38 (5 ملفات) | 40 |
| `Make_Store_Order_Sarf_Estlam_Automatic_From_Invoice` | 34 (4 ملفات) | 3 |
| `CompanyTitle` | 8 | 1 |
| `Flage_AccountDetails_View` | 4 | 18 |
| `Show_Item_TaxSale10_OnInvoiceMonitor` / `Show_Item_ATS05_OnInvoiceMonitor` / `Show_Item_TaxSale_On_Bill_Monitor` | 1–2 لكل منها | 2–4 |
| `Tax_BranchesId`، `VersionRun_On_OnePC_Using_HardwareInfo`، `Make_Store_Order_Sarf_Estlam_Automatic` | 1 لكل منها | 1–2 |

الأثر: لو اختار مستخدم ديسكتوب قيمة مختلفة فى شاشته فلن يطبّقها الويب (يستعمل قيمة SysConfig). هذا مقصود غالباً لأن الويب يعمل بإعداد القاعدة؛ يُراجع فقط `Balance_BeginingPeriod` و`Flage_AccountDetails_View` لو ظهر فرق فى أرصدة التقارير بين الويب والديسكتوب (بند فروق Razor/ديسكتوب د8).

### 4) حقول WinForms فقط لا يقرؤها الويب (تجاهل)
`IsSalesMan` (53 إسناداً)، `EditItemBalanceFromReport` (15)، `Store_Id_UsedBefore`، `Store_Index_UsedBefore`، `PrinterNameFor*`، `FormWidthDiff`، `StartupForm_Name`، `Branch_Name` وما شابه — لا قراءات من كود الويب.

## التوصية
لا تعديل مطلوب الآن. عند ظهور فرق تقارير بين الويب والديسكتوب ابدأ من الجدول (3). حارس `UserSessionContext.MisuseReporter` يسجّل أى قراءة للحقول السبعة المعزولة خارج جلسة، وهو ما يغطى الخطر الحقيقى (تسرب هوية المستخدم/الفرع).
