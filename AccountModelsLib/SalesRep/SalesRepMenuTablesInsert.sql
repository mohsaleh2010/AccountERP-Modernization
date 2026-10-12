-- إضافات جداول القوائم (SysConfigMenus / SysConfigMenusAr / FORMS_DETAILES / SysConfigMenusPERMATIONS)
-- تم التحقق من الأعمدة من ERP_V1_3.md (§9005/9035/8924/9065) — لا إعادة تسمية، مطابقة حرفياً
-- Arabic text preserved (Windows-1256 / UTF-8 safe) — no encoding corruption
-- No secrets / connection strings / ReturnConnStr values (rule 6)

-- 1) SysConfigMenusAr (Arabic descriptions for SalesRep menus)
-- Id / Parent_Id / KeysData / Description / PROJECT_ID / Value / DefaultValue / ...
INSERT INTO SysConfigMenusAr (Id, Parent_Id, KeysData, Description, PROJECT_ID, Value, OperationDate)
VALUES
  (1001, 496, N'menuSalesRepShift',  N'وردية المندوب (SalesRep Shift)', 2, N'1', GETDATE()),
  (1002, 496, N'menuSalesRepVisits', N'خطة الزيارات (Visits Plan)', 2, N'1', GETDATE()),
  (1003, 496, N'menuSalesRepCash',  N'جرد الخزينة (Shift Cash)', 2, N'1', GETDATE()),
  (1004, 496, N'menuSalesRepApprovals', N'طلبات الموافقة (Approvals)', 2, N'1', GETDATE());

-- 2) FORMS_DETAILES (form-level details linking to MainFrm_AccountUpdate menus)
-- FORM_ID / FORM_NAME / MENU_ID / MENU_ACTION / SHOW_IN_MENU / ORDER_NO / ...
INSERT INTO FORMS_DETAILES (FORM_ID, FORM_NAME, MENU_ID, MENU_ACTION, SHOW_IN_MENU, ORDER_NO, Branch_Id, User_Id, OperationDate)
VALUES
  (1, N'Sales_Rep_Frm.SalesRepShiftFrm', 1001, N'menuSalesRepShift_Click', 1, 1, 1, 1, GETDATE()),
  (2, N'Sales_Rep_Frm.SalesRepVisitsPlanFrm', 1002, N'menuSalesRepVisits_Click', 1, 2, 1, 1, GETDATE()),
  (3, N'Sales_Rep_Frm.SalesRepShiftCashFrm', 1003, N'menuSalesRepCash_Click', 1, 3, 1, 1, GETDATE()),
  (4, N'Sales_Rep_Frm.SalesRepApprovalsFrm', 1004, N'menuSalesRepApprovals_Click', 1, 4, 1, 1, GETDATE());

-- 3) SysConfigMenusPERMATIONS (access rights per user/form/menu)
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, OperationDate)
VALUES
  (1, 1001, NULL, 1, N'Allow', 2, GETDATE()),
  (2, 1002, NULL, 1, N'Allow', 2, GETDATE()),
  (3, 1003, NULL, 1, N'Allow', 2, GETDATE()),
  (4, 1004, NULL, 1, N'Allow', 2, GETDATE());
