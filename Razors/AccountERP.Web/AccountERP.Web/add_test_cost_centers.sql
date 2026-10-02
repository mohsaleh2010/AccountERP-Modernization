-- ═══════════════════════════════════════════════════════════════════════════════
-- اختبار: إضافة 3 مراكز تكلفة آباء (جذور) + 50 مركز تكلفة أبناء موزّعة عليهم
-- ليُكتمل اختبار شجرة مراكز التكلفة في شاشة ربط مراكز التكلفة (AssignCostCentersDialog)
--
-- الجدول: Cost_Center_Tree  (Id ليس identity في بيانات التشغيل — يُولَّد عبر GETAutoNumber)
--   الأعمدة المُستخدَمة من INSERT الفعلي في Add_NewCostCenter:
--   Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes,
--   Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode
--
-- نمط التكويد (من Common.MakeAutomatic_CostCenterCode + Add_NewCostCenter):
--   - الجذر (Parent_Id = 0):            FirstSubCode يحدد عدد خانات كود الجذر
--       FirstSubCode = '1'   → كود الجذر بلا أصفار بادئة  (1, 2, 3, ...)
--       FirstSubCode = '01'  → كود الجذر بخانتين          (01, 02, ...)
--   - الابن: Code_Str = كود_الأب + '-' + رقم تسلسلي بخانتين إذا كان < 10
--   - lastPartIncrease = الرقم التسلسلي داخل الأب (SubAccount_Count + 1)
--   - Code = قيمة Code_Str مع إزالة الفواصل '-' (لونغ)
--
-- ⚠ لا تُنفَّذ مرتين بدون تعديل MAX/الـ BASE — الأكواد تتكرر.
-- ═══════════════════════════════════════════════════════════════════════════════

SET NOCOUNT ON;

DECLARE @baseId BIGINT = (SELECT ISNULL(MAX(Id), 0) FROM Cost_Center_Tree);

-- بعض الحقول الثابتة النمطية المستخدمة فعلاً في القاعدة (تُترك محايدة إذا لم تُستخدَم)
DECLARE @Cost_Type INT = 0;
DECLARE @Currency_Id INT = 0;
DECLARE @Currency_Str NVARCHAR(50) = N'';
DECLARE @Notes NVARCHAR(1024) = N'';

-- ─────────────────────────────────────────────────────────────
-- 1) ثلاثة آباء (جذور) Parent_Id = 0
-- ─────────────────────────────────────────────────────────────
-- الأب 1: كود 1   (FirstSubCode = '1')
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 1, 0, N'مركز تكلفة اختبار — الإدارات', 1, N'1', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 1, 1, N'1');

-- الأب 2: كود 2
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 2, 0, N'مركز تكلفة اختبار — المشروعات', 2, N'2', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 1, 1, N'1');

-- الأب 3: كود 3
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 3, 0, N'مركز تكلفة اختبار — الفروع', 3, N'3', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 1, 1, N'1');

-- ─────────────────────────────────────────────────────────────
-- 2) خمسون ابناً: 17 للأب 1، 17 للأب 2، 16 للأب 3
--    Code_Str = كود الأب + '-' + رقم بخانتين (خانتان لأن FirstSubCode!='1' للأبناء)
-- ─────────────────────────────────────────────────────────────

-- الأب 1 (Id = @baseId+1, Code_Str='1') — 17 ابناً
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 4,  @baseId + 1, N'إدارة الموارد البشرية',        101, N'1-01', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 1,  2, N'1-01');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 5,  @baseId + 1, N'إدارة المالية',                 102, N'1-02', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 2,  2, N'1-02');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 6,  @baseId + 1, N'إدارة التسويق',                103, N'1-03', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 3,  2, N'1-03');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 7,  @baseId + 1, N'إدارة المبيعات',               104, N'1-04', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 4,  2, N'1-04');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 8,  @baseId + 1, N'إدارة المشتريات',              105, N'1-05', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 5,  2, N'1-05');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 9,  @baseId + 1, N'إدارة التخطيط الاستراتيجي',    106, N'1-06', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 6,  2, N'1-06');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 10, @baseId + 1, N'إدارة تكنولوجيا المعلومات',    107, N'1-07', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 7,  2, N'1-07');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 11, @baseId + 1, N'إدارة خدمة العملاء',           108, N'1-08', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 8,  2, N'1-08');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 12, @baseId + 1, N'إدارة الجودة',                 109, N'1-09', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 9,  2, N'1-09');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 13, @baseId + 1, N'إدارة الشؤون القانونية',       110, N'1-10', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 10, 2, N'1-10');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 14, @baseId + 1, N'إدارة العلاقات العامة',        111, N'1-11', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 11, 2, N'1-11');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 15, @baseId + 1, N'إدارة الأمن والسلامة',         112, N'1-12', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 12, 2, N'1-12');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 16, @baseId + 1, N'إدارة المخازن',                113, N'1-13', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 13, 2, N'1-13');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 17, @baseId + 1, N'إدارة النقل الداخلي',          114, N'1-14', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 14, 2, N'1-14');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 18, @baseId + 1, N'إدارة التأمينات والمعاشات',    115, N'1-15', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 15, 2, N'1-15');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 19, @baseId + 1, N'إدارة التدريب والتطوير',       116, N'1-16', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 16, 2, N'1-16');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 20, @baseId + 1, N'إدارة الاستثمار',              117, N'1-17', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 17, 2, N'1-17');

-- الأب 2 (Id = @baseId+2, Code_Str='2') — 17 ابناً
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 21, @baseId + 2, N'مشروع بناء المخزن الجديد',     201, N'2-01', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 1,  2, N'2-01');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 22, @baseId + 2, N'مشروع تطوير موقع الويب',       202, N'2-02', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 2,  2, N'2-02');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 23, @baseId + 2, N'مشروع حملة إعلانية',           203, N'2-03', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 3,  2, N'2-03');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 24, @baseId + 2, N'مشروع تحديث نظام الفوترة',     204, N'2-04', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 4,  2, N'2-04');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 25, @baseId + 2, N'مشروع دراسة جدوى التوسع',      205, N'2-05', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 5,  2, N'2-05');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 26, @baseId + 2, N'مشروع تركيب أنظمة كاميرات',    206, N'2-06', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 6,  2, N'2-06');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 27, @baseId + 2, N'مشروع أرشفة المستندات',        207, N'2-07', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 7,  2, N'2-07');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 28, @baseId + 2, N'مشروع تجهيز فرع العين السخنة', 208, N'2-08', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 8,  2, N'2-08');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 29, @baseId + 2, N'مشروع نظم إدارة الطاقة',       209, N'2-09', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 9,  2, N'2-09');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 30, @baseId + 2, N'مشروع تطبيق الموبايل',         210, N'2-10', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 10, 2, N'2-10');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 31, @baseId + 2, N'مشروع تحديث أسطول النقل',      211, N'2-11', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 11, 2, N'2-11');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 32, @baseId + 2, N'مشروع موسم الحج والعمرة',      212, N'2-12', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 12, 2, N'2-12');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 33, @baseId + 2, N'مشروع عربة الغذاء المتنقلة',   213, N'2-13', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 13, 2, N'2-13');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 34, @baseId + 2, N'مشروع ترقية خطوط الإنتاج',     214, N'2-14', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 14, 2, N'2-14');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 35, @baseId + 2, N'مشروع نظام الحضور البيومتري',  215, N'2-15', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 15, 2, N'2-15');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 36, @baseId + 2, N'مشروع تطوير المخازن الذكية',   216, N'2-16', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 16, 2, N'2-16');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 37, @baseId + 2, N'مشروع البرنامج التأهيلي',      217, N'2-17', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 17, 2, N'2-17');

-- الأب 3 (Id = @baseId+3, Code_Str='3') — 16 ابناً
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 38, @baseId + 3, N'فرع القاهرة الكبرى',           301, N'3-01', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 1,  2, N'3-01');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 39, @baseId + 3, N'فرع الإسكندرية',               302, N'3-02', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 2,  2, N'3-02');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 40, @baseId + 3, N'فرع الجيزة',                   303, N'3-03', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 3,  2, N'3-03');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 41, @baseId + 3, N'فرع الدلتا',                   304, N'3-04', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 4,  2, N'3-04');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 42, @baseId + 3, N'فرع الصعيد — أسيوط',          305, N'3-05', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 5,  2, N'3-05');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 43, @baseId + 3, N'فرع سوهاج',                   306, N'3-06', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 6,  2, N'3-06');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 44, @baseId + 3, N'فرع الأقصر والأقصر السياحية', 307, N'3-07', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 7,  2, N'3-07');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 45, @baseId + 3, N'فرع البحر الأحمر',             308, N'3-08', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 8,  2, N'3-08');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 46, @baseId + 3, N'فرع العين السخنة',            309, N'3-09', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 9,  2, N'3-09');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 47, @baseId + 3, N'فرع الساحل الشمالي',          310, N'3-10', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 10, 2, N'3-10');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 48, @baseId + 3, N'فرع الغردقة',                 311, N'3-11', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 11, 2, N'3-11');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 49, @baseId + 3, N'فرع شرم الشيخ',               312, N'3-12', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 12, 2, N'3-12');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 50, @baseId + 3, N'فرع مطروح',                   313, N'3-13', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 13, 2, N'3-13');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 51, @baseId + 3, N'فرع الوادي الجديد',           314, N'3-14', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 14, 2, N'3-14');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 52, @baseId + 3, N'فرع بورسعيد',                 315, N'3-15', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 15, 2, N'3-15');
INSERT INTO Cost_Center_Tree (Id, Parent_Id, Name, Code, Code_Str, Cost_Type, Notes, Currency_Id, Currency_Str, Head_Main_Cost, lastPartIncrease, Cost_Level, FirstSubCode)
VALUES (@baseId + 53, @baseId + 3, N'فرع السويس',                  316, N'3-16', @Cost_Type, @Notes, @Currency_Id, @Currency_Str, 0, 16, 2, N'3-16');

-- ─────────────────────────────────────────────────────────────
-- 3) معاينة ما أُدخل
-- ─────────────────────────────────────────────────────────────
SELECT Id, Parent_Id, Name, Code, Code_Str, Cost_Level, lastPartIncrease
FROM Cost_Center_Tree
WHERE Id > @baseId
ORDER BY Id;