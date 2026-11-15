using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// P2 (ai-assistant-plan.md §2.1، §3): سطح القراءة الوحيد الذى يراه المساعد — Views فى schema «ai» بأسماء أعمدة واضحة تُخفى
    /// الأخطاء الإملائية القديمة (Purshing، Detailes) دون تغيير أى جدول. كائنات جديدة فقط (CREATE OR ALTER VIEW على schema ai)، لا تمس الجداول.
    /// Views المبيعات ومرتجعها تفلتر بصلاحية عرض الفواتير الضريبية/غير الضريبية (920/921) من SESSION_CONTEXT(N'ai_tax')
    /// الذى يضبطه ReadOnlySqlExecutor (-1 = الكل، 0، 1)؛ بلا سياق لا تعيد أى صف. لا أعمدة اتصال شخصية (هاتف، عنوان، رقم قومى) — تقليل البيانات.
    /// المحذوف مستبعد كما تفعل الشاشات: Invoices_*.Is_DeletedProcessed و Bills_*.Deleted.
    /// </summary>
    public static class AiReadViewsCLS
    {
        public const string Schema = "ai";

        /// <summary>(اسم الـ View بلا schema، نص الإنشاء).</summary>
        public static readonly (string Name, string Sql)[] Views =
        {
            ("v_branches", @"CREATE OR ALTER VIEW ai.v_branches AS
SELECT b.Id AS branch_id, b.Branch_Name AS branch_name, b.Code AS branch_code
FROM dbo.Account_Branches b;"),
            ("v_stores", @"CREATE OR ALTER VIEW ai.v_stores AS
SELECT s.id AS store_id, s.Store_Name AS store_name, s.Store_Code AS store_code, s.Branch_Id AS branch_id, s.Active AS active_flag
FROM dbo.Store s;"),
            ("v_items", @"CREATE OR ALTER VIEW ai.v_items AS
SELECT i.id AS item_id, i.code AS item_code, i.Item_Name AS item_name, i.Item_Category_ID AS category_id, c.Category_Name AS category_name,
       i.Active_flage AS active_flag, u.B_Unit AS large_unit, u.S_unit AS small_unit, u.S_unit_Value AS small_per_large,
       i.S_price_LargeUnit AS sale_price_large, i.S_price_SmallUnit AS sale_price_small,
       i.P_price_LargeUnit AS purchase_price_large, i.P_price_SmallUnit AS purchase_price_small, i.P_priceAvgCost AS avg_cost
FROM dbo.Item i
LEFT JOIN dbo.Item_Category c ON c.ID = i.Item_Category_ID
LEFT JOIN dbo.unit u ON u.id = i.Unit_Id;"),
            ("v_customers", @"CREATE OR ALTER VIEW ai.v_customers AS
SELECT c.Id AS customer_id, c.Code AS customer_code, c.Name AS customer_name, c.Active AS active_flag,
       c.General_Customer_Flage AS is_general_customer, c.Branch_Id AS branch_id,
       c.Customer_Type AS customer_type, c.Group_Id AS group_id, c.Currency_Id AS currency_id
FROM dbo.Customer c;"),
            ("v_suppliers", @"CREATE OR ALTER VIEW ai.v_suppliers AS
SELECT s.Id AS supplier_id, s.Code AS supplier_code, s.Name AS supplier_name, s.Active AS active_flag,
       s.IS_ShipmentCompany AS is_shipment_company, s.Branch_Id AS branch_id
FROM dbo.Supplier s;"),
            ("v_sales", @"CREATE OR ALTER VIEW ai.v_sales AS
SELECT l.ID AS sale_id, l.Invoice_Id AS invoice_no, CONVERT(date, l.Invoice_Date) AS invoice_date, l.Invoice_Date AS invoice_datetime,
       l.Customer_Id AS customer_id, c.Name AS customer_name, l.Branch_Id AS branch_id, b.Branch_Name AS branch_name,
       l.Total AS total, l.commission AS commission, l.TaxSales10_Value AS sales_tax, l.ATS05_Value AS ats05_value,
       l.DAMGHAT AS stamp_duty, l.Net_Total AS net_total, l.Equivalent_Net_Total AS net_total_equivalent, l.Currency_id AS currency_id,
       l.IsInvoiceTaxInfo AS is_tax_invoice, l.Is_Tax_Per_Item AS is_tax_per_item, l.Is_Invoice_Agile AS is_credit,
       l.PaidValue AS paid_value, l.RestValue AS rest_value, l.Salesman_Id AS salesman_id, l.IsMaintainceInvoice AS is_maintenance,
       l.IS_Posted_Flage AS is_posted, l.Notes AS notes, CONVERT(date, l.InvoiceDueDate) AS due_date
FROM dbo.Invoices_List l
LEFT JOIN dbo.Customer c ON c.Id = l.Customer_Id
LEFT JOIN dbo.Account_Branches b ON b.Id = l.Branch_Id
WHERE ISNULL(l.Is_DeletedProcessed, 0) <> 1
  AND CAST(SESSION_CONTEXT(N'ai_tax') AS int) IN (-1, ISNULL(l.IsInvoiceTaxInfo, 0));"),
            // P8.3 (ف9–ف11): حالة رفع الفاتورة الإلكترونية لكل فاتورة/مرتجع ضريبى. Taxdocuments* تُنشأ مع إعداد المنظومة فقط،
            // فإن غابت يُستبدل الرمز بجدول مشتق فارغ بنفس الأعمدة (الحالة من IsUploadToPortal وحده). لا hashKey ولا أسرار البوابة.
            ("v_einvoice_status", @"CREATE OR ALTER VIEW ai.v_einvoice_status AS
SELECT N'invoice' AS doc_kind, l.ID AS sale_id, l.Invoice_Id AS invoice_no, CONVERT(date, l.Invoice_Date) AS invoice_date,
       l.Customer_Id AS customer_id, c.Name AS customer_name, l.Branch_Id AS branch_id, l.Net_Total AS net_total,
       l.TaxInvoiceNo AS tax_invoice_no, l.TaxInternalId AS internal_id, ISNULL(l.IsUploadToPortal, 0) AS is_uploaded,
       t.uuid AS uuid, t.submissionId AS submission_id, t.DateOfSent AS sent_date, t.IsSuccessded AS send_state,
       t.Errors AS errors_text, t.StausStr AS portal_status, ISNULL(n.cnt, 0) AS attempts,
       CASE WHEN t.Id IS NULL AND ISNULL(l.IsUploadToPortal, 0) <> 1 THEN N'not_sent'
            WHEN t.Id IS NULL THEN N'flag_only'
            WHEN t.IsSuccessded = 1 THEN N'sent' ELSE N'superseded' END AS upload_state
FROM dbo.Invoices_List l
LEFT JOIN dbo.Customer c ON c.Id = l.Customer_Id
OUTER APPLY (SELECT TOP (1) d.Id, d.uuid, d.submissionId, d.DateOfSent, d.IsSuccessded, d.Errors, d.StausStr
             FROM /*AI_TAXDOC*/ d WHERE d.invoices_ListId = l.ID
             ORDER BY CASE WHEN d.IsSuccessded = 1 THEN 0 ELSE 1 END, d.Id DESC) t
OUTER APPLY (SELECT COUNT(*) AS cnt FROM /*AI_TAXDOC*/ d WHERE d.invoices_ListId = l.ID) n
WHERE ISNULL(l.Is_DeletedProcessed, 0) <> 1 AND ISNULL(l.IsInvoiceTaxInfo, 0) = 1
  AND CAST(SESSION_CONTEXT(N'ai_tax') AS int) IN (-1, 1)
UNION ALL
SELECT N'return' AS doc_kind, l.ID AS sale_id, l.Invoice_Id AS invoice_no, CONVERT(date, l.Invoice_Date) AS invoice_date,
       l.Customer_Id AS customer_id, c.Name AS customer_name, l.Branch_Id AS branch_id, l.Net_Total AS net_total,
       l.TaxInvoiceNo AS tax_invoice_no, l.TaxInternalId AS internal_id, ISNULL(l.IsUploadToPortal, 0) AS is_uploaded,
       t.uuid AS uuid, t.submissionId AS submission_id, t.DateOfSent AS sent_date, t.IsSuccessded AS send_state,
       t.Errors AS errors_text, t.StausStr AS portal_status, ISNULL(n.cnt, 0) AS attempts,
       CASE WHEN t.Id IS NULL AND ISNULL(l.IsUploadToPortal, 0) <> 1 THEN N'not_sent'
            WHEN t.Id IS NULL THEN N'flag_only'
            WHEN t.IsSuccessded = 1 THEN N'sent' ELSE N'superseded' END AS upload_state
FROM dbo.Invoices_List_Reverse l
LEFT JOIN dbo.Customer c ON c.Id = l.Customer_Id
OUTER APPLY (SELECT TOP (1) d.Id, d.uuid, d.submissionId, d.DateOfSent, d.IsSuccessded, d.Errors, d.StausStr
             FROM /*AI_TAXDOC_REV*/ d WHERE d.invoices_ListId = l.ID
             ORDER BY CASE WHEN d.IsSuccessded = 1 THEN 0 ELSE 1 END, d.Id DESC) t
OUTER APPLY (SELECT COUNT(*) AS cnt FROM /*AI_TAXDOC_REV*/ d WHERE d.invoices_ListId = l.ID) n
WHERE ISNULL(l.Is_DeletedProcessed, 0) <> 1 AND ISNULL(l.IsInvoiceTaxInfo, 0) = 1
  AND CAST(SESSION_CONTEXT(N'ai_tax') AS int) IN (-1, 1);"),
            ("v_sales_lines", @"CREATE OR ALTER VIEW ai.v_sales_lines AS
SELECT d.Id AS line_id, l.ID AS sale_id, l.Invoice_Id AS invoice_no, CONVERT(date, l.Invoice_Date) AS invoice_date,
       l.Customer_Id AS customer_id, c.Name AS customer_name, l.Branch_Id AS branch_id,
       d.Item_Id AS item_id, i.code AS item_code, i.Item_Name AS item_name, d.Store_Id AS store_id, s.Store_Name AS store_name,
       d.Larg_Qnty AS large_qty, d.Small_Qnty AS small_qty, d.Qnty AS qty, d.UnitStr AS unit_name, d.PriceUnit AS unit_price,
       d.Total AS line_total, d.TaxSales10_Value AS line_sales_tax, d.ATS05_Value AS line_ats05_value, d.Item_Net_Total_Price AS line_net,
       d.P_priceAvgCost AS avg_cost_at_sale, l.IsInvoiceTaxInfo AS is_tax_invoice
FROM dbo.Invoices_Details d
INNER JOIN dbo.Invoices_List l ON l.ID = d.Invoice_Id
LEFT JOIN dbo.Customer c ON c.Id = l.Customer_Id
LEFT JOIN dbo.Item i ON i.id = d.Item_Id
LEFT JOIN dbo.Store s ON s.id = d.Store_Id
WHERE ISNULL(l.Is_DeletedProcessed, 0) <> 1
  AND CAST(SESSION_CONTEXT(N'ai_tax') AS int) IN (-1, ISNULL(l.IsInvoiceTaxInfo, 0));"),
            ("v_sales_returns", @"CREATE OR ALTER VIEW ai.v_sales_returns AS
SELECT l.ID AS return_id, l.Invoice_Id AS return_no, CONVERT(date, l.Invoice_Date) AS return_date,
       l.Customer_Id AS customer_id, c.Name AS customer_name, l.Branch_Id AS branch_id, b.Branch_Name AS branch_name,
       l.Total AS total, l.commission AS commission, l.TaxSales10_Value AS sales_tax, l.ATS05_Value AS ats05_value,
       l.DAMGHAT AS stamp_duty, l.Net_Total AS net_total, l.Equivalent_Net_Total AS net_total_equivalent, l.Currency_id AS currency_id,
       l.IsInvoiceTaxInfo AS is_tax_invoice, l.Is_Invoice_Agile AS is_credit, l.Notes AS notes
FROM dbo.Invoices_List_Reverse l
LEFT JOIN dbo.Customer c ON c.Id = l.Customer_Id
LEFT JOIN dbo.Account_Branches b ON b.Id = l.Branch_Id
WHERE ISNULL(l.Is_DeletedProcessed, 0) <> 1
  AND CAST(SESSION_CONTEXT(N'ai_tax') AS int) IN (-1, ISNULL(l.IsInvoiceTaxInfo, 0));"),
            ("v_sales_return_lines", @"CREATE OR ALTER VIEW ai.v_sales_return_lines AS
SELECT d.Id AS line_id, l.ID AS return_id, l.Invoice_Id AS return_no, CONVERT(date, l.Invoice_Date) AS return_date,
       l.Customer_Id AS customer_id, c.Name AS customer_name, l.Branch_Id AS branch_id,
       d.Item_Id AS item_id, i.code AS item_code, i.Item_Name AS item_name, d.Store_Id AS store_id, s.Store_Name AS store_name,
       d.Larg_Qnty AS large_qty, d.Small_Qnty AS small_qty, d.Qnty AS qty, d.UnitStr AS unit_name, d.PriceUnit AS unit_price,
       d.Total AS line_total, d.TaxSales10_Value AS line_sales_tax, d.ATS05_Value AS line_ats05_value, d.Item_Net_Total_Price AS line_net
FROM dbo.Invoices_Details_Reverse d
INNER JOIN dbo.Invoices_List_Reverse l ON l.ID = d.Invoice_Id
LEFT JOIN dbo.Customer c ON c.Id = l.Customer_Id
LEFT JOIN dbo.Item i ON i.id = d.Item_Id
LEFT JOIN dbo.Store s ON s.id = d.Store_Id
WHERE ISNULL(l.Is_DeletedProcessed, 0) <> 1
  AND CAST(SESSION_CONTEXT(N'ai_tax') AS int) IN (-1, ISNULL(l.IsInvoiceTaxInfo, 0));"),
            ("v_purchases", @"CREATE OR ALTER VIEW ai.v_purchases AS
SELECT l.ID AS purchase_id, l.Invoice_Id AS invoice_no, CONVERT(date, l.Invoice_Date) AS invoice_date,
       l.Supplier_Id AS supplier_id, s.Name AS supplier_name, l.Branch_Id AS branch_id, b.Branch_Name AS branch_name,
       l.Total AS total, l.commission AS commission, l.TaxSales10_Value AS sales_tax, l.ATS05_Value AS ats05_value,
       l.Damaghat AS stamp_duty, l.Net_Total AS net_total, l.Equivalent_Net_Total AS net_total_equivalent, l.Currency_id AS currency_id,
       l.IsInvoiceTaxInfo AS is_tax_invoice, l.Is_Tax_Per_Item AS is_tax_per_item, l.Is_Invoice_Agile AS is_credit,
       l.PaidValue AS paid_value, l.RestValue AS rest_value, l.IS_Posted_Flage AS is_posted, l.Notes AS notes
FROM dbo.Bills_List l
LEFT JOIN dbo.Supplier s ON s.Id = l.Supplier_Id
LEFT JOIN dbo.Account_Branches b ON b.Id = l.Branch_Id
WHERE ISNULL(l.Deleted, 0) <> 1;"),
            ("v_purchase_lines", @"CREATE OR ALTER VIEW ai.v_purchase_lines AS
SELECT d.Id AS line_id, l.ID AS purchase_id, l.Invoice_Id AS invoice_no, CONVERT(date, l.Invoice_Date) AS invoice_date,
       l.Supplier_Id AS supplier_id, sp.Name AS supplier_name, l.Branch_Id AS branch_id,
       d.Item_Id AS item_id, i.code AS item_code, i.Item_Name AS item_name, d.Store_Id AS store_id, s.Store_Name AS store_name,
       d.Larg_Qnty AS large_qty, d.Small_Qnty AS small_qty, d.Qnty AS qty, d.UnitStr AS unit_name, d.PriceUnit AS unit_price,
       d.Total AS line_total, d.TaxSales10_Value AS line_sales_tax, d.ATS05_Value AS line_ats05_value, d.Item_Net_Total_Price AS line_net
FROM dbo.Bills_Details d
INNER JOIN dbo.Bills_List l ON l.ID = d.Invoice_Id
LEFT JOIN dbo.Supplier sp ON sp.Id = l.Supplier_Id
LEFT JOIN dbo.Item i ON i.id = d.Item_Id
LEFT JOIN dbo.Store s ON s.id = d.Store_Id
WHERE ISNULL(l.Deleted, 0) <> 1;"),
            ("v_purchase_returns", @"CREATE OR ALTER VIEW ai.v_purchase_returns AS
SELECT l.ID AS return_id, l.Invoice_Id AS return_no, CONVERT(date, l.Invoice_Date) AS return_date,
       l.Supplier_Id AS supplier_id, s.Name AS supplier_name, l.Branch_Id AS branch_id, b.Branch_Name AS branch_name,
       l.Total AS total, l.commission AS commission, l.TaxSales10_Value AS sales_tax, l.ATS05_Value AS ats05_value,
       l.Damaghat AS stamp_duty, l.Net_Total AS net_total, l.Equivalent_Net_Total AS net_total_equivalent, l.Currency_id AS currency_id,
       l.IsInvoiceTaxInfo AS is_tax_invoice, l.Is_Invoice_Agile AS is_credit, l.Notes AS notes
FROM dbo.Bills_List_Reverse l
LEFT JOIN dbo.Supplier s ON s.Id = l.Supplier_Id
LEFT JOIN dbo.Account_Branches b ON b.Id = l.Branch_Id
WHERE ISNULL(l.Deleted, 0) <> 1;"),
            ("v_purchase_return_lines", @"CREATE OR ALTER VIEW ai.v_purchase_return_lines AS
SELECT d.Id AS line_id, l.ID AS return_id, l.Invoice_Id AS return_no, CONVERT(date, l.Invoice_Date) AS return_date,
       l.Supplier_Id AS supplier_id, sp.Name AS supplier_name, l.Branch_Id AS branch_id,
       d.Item_Id AS item_id, i.code AS item_code, i.Item_Name AS item_name, d.Store_Id AS store_id, s.Store_Name AS store_name,
       d.Larg_Qnty AS large_qty, d.Small_Qnty AS small_qty, d.Qnty AS qty, d.UnitStr AS unit_name, d.PriceUnit AS unit_price,
       d.Total AS line_total, d.TaxSales10_Value AS line_sales_tax, d.ATS05_Value AS line_ats05_value, d.Item_Net_Total_Price AS line_net
FROM dbo.Bills_Details_Reverse d
INNER JOIN dbo.Bills_List_Reverse l ON l.ID = d.Invoice_Id
LEFT JOIN dbo.Supplier sp ON sp.Id = l.Supplier_Id
LEFT JOIN dbo.Item i ON i.id = d.Item_Id
LEFT JOIN dbo.Store s ON s.id = d.Store_Id
WHERE ISNULL(l.Deleted, 0) <> 1;"),
            ("v_stock_balance", @"CREATE OR ALTER VIEW ai.v_stock_balance AS
SELECT ui.id AS balance_row_id, ui.Item_id AS item_id, i.code AS item_code, i.Item_Name AS item_name,
       ui.Store_id AS store_id, s.Store_Name AS store_name, s.Branch_Id AS branch_id,
       ui.Full_qua AS large_qty, ui.D_Qua AS small_qty, u.B_Unit AS large_unit, u.S_unit AS small_unit,
       ui.Small_unit AS small_per_large,
       ISNULL(ui.Full_qua, 0) * ISNULL(NULLIF(ui.Small_unit, 0), 1) + ISNULL(ui.D_Qua, 0) AS total_small_qty,
       ui.P_priceAvgCost AS avg_cost_large, ui.P_price_LargeUnit AS purchase_price_large, ui.S_price_LargeUnit AS sale_price_large
FROM dbo.Unit_item ui
LEFT JOIN dbo.Item i ON i.id = ui.Item_id
LEFT JOIN dbo.Store s ON s.id = ui.Store_id
LEFT JOIN dbo.unit u ON u.id = ui.unit_id;"),
            // P8.1: سطور إذون النقدية (سند قبض = 1، صرف = 2) كما تقرؤها «قوائم الإذونات» (CashVouchersService.SearchVouchers): الإذن له رقم (≠ 0، -1).
            // العملة = عملة الصندوق (Banks.Currency_Id؛ عملة السطر قد تخالفها)، والفرع = فرع الصندوق (Branch_Id السطر صفر غالباً؛ 0/-1 = كل الفروع).
            // الطرف: عميل إن Customer_Id > 0، وإلا مورد إن Supplier_Id > 0، وإلا حساب (Account_Type غير موثوق). المعادل = القيمة × Factor.
            // صلاحية الصناديق (SysConfigBanksPERMATIONS) حين IS_Used_SandoukWithPermission = 1 عبر SESSION_CONTEXT ai_box_perm؛ بلا سياق لا صفوف.
            ("v_cash_vouchers", @"CREATE OR ALTER VIEW ai.v_cash_vouchers AS
SELECT m.Id AS line_id, o.Id AS voucher_id, o.Order_No AS voucher_no, CONVERT(date, m.Transaction_Date) AS voucher_date,
       m.Transaction_Type AS transaction_type, CASE WHEN m.Transaction_Type = 1 THEN 1 ELSE 0 END AS is_receipt,
       m.Bank_Id AS box_id, b.Bank_Name AS box_name, b.Is_Bank AS is_bank, b.Branch_Id AS branch_id, b.Currency_Id AS currency_id,
       ISNULL(m.Transaction_Value, 0) AS amount, ISNULL(NULLIF(m.Factor, 0), 1) AS factor,
       ISNULL(m.Transaction_Value, 0) * ISNULL(NULLIF(m.Factor, 0), 1) AS amount_equivalent,
       CASE WHEN m.Customer_Id > 0 THEN N'customer' WHEN m.Supplier_Id > 0 THEN N'supplier' ELSE N'account' END AS party_type,
       CASE WHEN m.Customer_Id > 0 THEN m.Customer_Id WHEN m.Supplier_Id > 0 THEN m.Supplier_Id ELSE m.SubAccount_Id END AS party_id,
       COALESCE(CASE WHEN m.Customer_Id > 0 THEN cu.Name END, CASE WHEN m.Supplier_Id > 0 THEN su.Name END, m.Account_Name) AS party_name,
       m.Lookup_CashPaymentMethodName AS payment_method, COALESCE(NULLIF(m.Notes, N''), o.Order_Notes) AS notes,
       m.DataDictionaryProcessesListsId AS source_doc_type, m.Reference_Type_Table_Id AS source_doc_id
FROM dbo.Money_Transaction_Box m
INNER JOIN dbo.Order_Money o ON o.Id = m.Order_Mony_Id
LEFT JOIN dbo.Banks b ON b.id = m.Bank_Id
LEFT JOIN dbo.Customer cu ON cu.Id = m.Customer_Id
LEFT JOIN dbo.Supplier su ON su.Id = m.Supplier_Id
WHERE o.Order_No <> 0 AND o.Order_No <> -1
  AND (CAST(SESSION_CONTEXT(N'ai_box_perm') AS int) = 0
       OR (CAST(SESSION_CONTEXT(N'ai_box_perm') AS int) = 1 AND " + BoxPermToken + @"));"),
            // P8.4 (ai-assistant-p84-plan.md ش23، §4): صف لكل سطر قيد. is_manual = Reference_Type 0 (فحص 6)؛ Journal_Name ممتلئ دائماً (فحص 11).
            // لا فلتر ضريبى ولا SESSION_CONTEXT (القيود ليست فواتير). الماسح يقرأ منه للقراءة فقط.
            ("v_gl_entries", @"CREATE OR ALTER VIEW ai.v_gl_entries AS
SELECT d.ID AS line_id, l.ID AS entry_id, l.Entry_No AS entry_no, CONVERT(date, l.Entry_Date) AS entry_date,
       l.Insert_ServerDateTime AS inserted_at, l.journal_Code AS journal_code, l.Journal_Name AS journal_name,
       l.Branch_Id_Entry AS branch_id, b.Branch_Name AS branch_name, l.Year_Financial_Period_Id AS period_id,
       ISNULL(l.Reference_Type, 0) AS reference_type, l.Reference_Type_Table_Id AS reference_id,
       CASE WHEN ISNULL(l.Reference_Type, 0) = 0 THEN 1 ELSE 0 END AS is_manual,
       CASE WHEN ISNULL(l.IS_Transfer_To_Gl_Flage, 0) = 1 THEN 1 ELSE 0 END AS is_posted,
       CASE WHEN ISNULL(l.IS_FirstBalance, 0) = 1 THEN 1 ELSE 0 END AS is_opening,
       CASE WHEN ISNULL(l.IsProfitLossClose, 0) = 1 THEN 1 ELSE 0 END AS is_closing,
       l.User_ID_Login AS user_id, l.Entry_Notes AS entry_notes,
       d.Line_No AS line_no, d.Submain_Account_ID AS account_id, a.SubAccount_NO AS account_no,
       COALESCE(a.SubAccount_Name, d.Account_Name) AS account_name, d.CostCenter_Id AS cost_center_id,
       ISNULL(d.Import_Value, 0) AS debit, ISNULL(d.Export_Value, 0) AS credit,
       ISNULL(d.Equivalent_Import_Value, 0) AS debit_equivalent, ISNULL(d.Equivalent_Export_Value, 0) AS credit_equivalent,
       d.Entry_Currency_ID AS currency_id, d.Notes_Details AS line_notes
FROM dbo.Account_Entry_Details d
INNER JOIN dbo.Account_Entry_List l ON l.ID = d.Account_Entry_List_ID
LEFT JOIN dbo.Submain_Account a ON a.Id = d.Submain_Account_ID
LEFT JOIN dbo.Account_Branches b ON b.Id = l.Branch_Id_Entry;"),
            // P8.5 (ai-assistant-p85-plan.md ت3): دفتر العميل = Customers_Accounts (Reference_Type <> 25 حرفياً كالشاشة) + دفتر المورد المرتبط.
            // الرصيد = SUM(debit - credit) بلا أى كتابة (لا SetFirstBalanceForAllCustomers ولا الجدول الوسيط). بلا هاتف (ت23).
            ("v_customer_ledger", @"CREATE OR ALTER VIEW ai.v_customer_ledger AS
SELECT a.Id AS movement_id, a.Customer_Id AS customer_id, CONVERT(date, a.Transaction_Date) AS movement_date,
       ISNULL(a.Export, 0) AS debit, ISNULL(a.Import, 0) AS credit,
       a.Reference_Type AS reference_type, a.Reference_Type_Table_Id AS reference_id,
       ISNULL(a.Branch_Id, 0) AS branch_id, N'customer' AS ledger_source, a.Notes AS notes
FROM dbo.Customers_Accounts a
INNER JOIN dbo.Customer c ON c.Id = a.Customer_Id
WHERE a.Reference_Type <> 25
UNION ALL
SELECT sa.Id, sp.Ref_Cust_Supplier_Id, CONVERT(date, sa.Transaction_Date),
       ISNULL(sa.Export, 0), ISNULL(sa.Import, 0),
       sa.Reference_Type, sa.Reference_Type_Table_Id,
       ISNULL(sa.Branch_Id, 0), N'linked_supplier', sa.Notes
FROM dbo.Suppliers_Accounts sa
INNER JOIN dbo.Supplier sp ON sp.Id = sa.Supplier_Id
WHERE sa.Reference_Type <> 25 AND sp.Ref_Cust_Supplier_Id > 0;"),
            // P8.5 (ت10): حد الائتمان من SalesRep_Credit_Policy (العميل يغلب المجموعة). الجدول غائب ⇒ جدول مشتق فارغ (فحص 5).
            ("v_customer_credit", @"CREATE OR ALTER VIEW ai.v_customer_credit AS
SELECT c.Id AS customer_id, c.Group_Id AS group_id,
       COALESCE(pc.Credit_Limit, pg.Credit_Limit) AS credit_limit,
       COALESCE(pc.Max_Overdue_Days, pg.Max_Overdue_Days) AS max_overdue_days,
       CASE WHEN pc.Id IS NOT NULL THEN N'customer' WHEN pg.Id IS NOT NULL THEN N'group' END AS limit_source
FROM dbo.Customer c
LEFT JOIN " + CreditToken + @" pc ON pc.Customer_Id = c.Id
LEFT JOIN " + CreditToken + @" pg ON pc.Id IS NULL AND pg.Customer_Id IS NULL AND pg.Group_Id = c.Group_Id;"),
            // P8.8 (ai-assistant-p88-plan.md ص6): سوابق التصنيف = أسطر سندات الخزينة إلى حساب عام (لا عميل ولا مورد) مع مركز التكلفة.
            // قراءة فقط؛ نفس شرط الصلاحية والسندات الحقيقية كـ v_cash_vouchers.
            ("v_expense_history", @"CREATE OR ALTER VIEW ai.v_expense_history AS
SELECT m.Id AS line_id, CONVERT(date, m.Transaction_Date) AS voucher_date, m.Transaction_Type AS transaction_type,
       COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(400), m.Notes))), N''), CONVERT(nvarchar(400), o.Order_Notes)) AS notes,
       m.SubAccount_Id AS sub_account_id, CONVERT(nvarchar(100), m.Account_Code) AS account_code,
       CONVERT(nvarchar(300), COALESCE(a.SubAccount_Name, m.Account_Name)) AS account_name, m.Account_Type AS account_type,
       NULLIF(m.Account_Cost_Center_Tree_Id, 0) AS cost_center_id,
       CONVERT(nvarchar(300), COALESCE(NULLIF(m.CostCenter_Name, N''), m.Cost_Center)) AS cost_center_name,
       b.Branch_Id AS branch_id, m.User_Id AS user_id
FROM dbo.Money_Transaction_Box m
INNER JOIN dbo.Order_Money o ON o.Id = m.Order_Mony_Id
LEFT JOIN dbo.Banks b ON b.id = m.Bank_Id
LEFT JOIN dbo.Submain_Account a ON a.Id = m.SubAccount_Id
WHERE o.Order_No <> 0 AND o.Order_No <> -1
  AND ISNULL(m.Customer_Id, 0) = 0 AND ISNULL(m.Supplier_Id, 0) = 0 AND ISNULL(m.SubAccount_Id, 0) > 0
  AND (CAST(SESSION_CONTEXT(N'ai_box_perm') AS int) = 0
       OR (CAST(SESSION_CONTEXT(N'ai_box_perm') AS int) = 1 AND " + BoxPermToken + @"));"),
            // P8.8 (ص16): سوابق تصنيف الأصناف = الأصناف النشطة بفئتها ومجموعتها ونوعها (صنف/خدمة).
            ("v_item_classes", @"CREATE OR ALTER VIEW ai.v_item_classes AS
SELECT i.id AS item_id, i.Item_Name AS item_name, i.Item_Category_ID AS category_id, c.Category_Name AS category_name,
       NULLIF(i.Items_GroupsId, 0) AS group_id, g.Name AS group_name, ISNULL(i.Is_ItemOrService, 1) AS item_or_service,
       CONVERT(date, COALESCE(i.Insert_ServerDateTime, i.OperationDate)) AS created_date
FROM dbo.Item i
LEFT JOIN dbo.Item_Category c ON c.ID = i.Item_Category_ID
LEFT JOIN dbo.Items_Group g ON g.Id = i.Items_GroupsId
WHERE ISNULL(i.Active_flage, 1) = 1 AND ISNULL(i.Item_Category_ID, 0) > 0;"),
        };

        /// <summary>
        /// جدول صلاحيات الصناديق ليس فى كل القواعد (غير موجود فى ERP_V1_3.md ولا فى قاعدة التطوير)، فيُكتب شرطه وقت الإنشاء:
        /// موجود ⇒ الصناديق الممنوحة للمستخدم فقط؛ غير موجود ⇒ لا صناديق ممنوحة (1 = 0). يُعاد التقييم مع كل EnsureSchema.
        /// </summary>
        private const string BoxPermToken = "/*AI_BOX_PERM*/";
        private const string BoxPermExists = @"EXISTS (SELECT 1 FROM dbo.SysConfigBanksPERMATIONS p
                       WHERE p.Account_Branks_Id = m.Bank_Id AND p.USER_DETAILES_Id = CAST(SESSION_CONTEXT(N'ai_user') AS bigint))";

        private const string CreditToken = "/*AI_CREDIT*/";
        private const string CreditEmpty = "(SELECT TOP (0) CAST(NULL AS bigint) AS Id, CAST(NULL AS bigint) AS Customer_Id, CAST(NULL AS bigint) AS Group_Id, " +
            "CAST(NULL AS decimal(18,4)) AS Credit_Limit, CAST(NULL AS int) AS Max_Overdue_Days)";

        private const string TaxDocToken = "/*AI_TAXDOC*/", TaxDocRevToken = "/*AI_TAXDOC_REV*/";
        private const string TaxDocEmpty = "(SELECT TOP (0) CAST(NULL AS bigint) AS Id, CAST(NULL AS bigint) AS invoices_ListId, CAST(NULL AS nvarchar(max)) AS uuid, " +
            "CAST(NULL AS nvarchar(max)) AS submissionId, CAST(NULL AS datetime) AS DateOfSent, CAST(NULL AS int) AS IsSuccessded, " +
            "CAST(NULL AS nvarchar(max)) AS Errors, CAST(NULL AS nvarchar(50)) AS StausStr)";

        public static IReadOnlyCollection<string> ViewNames => Views.Select(v => v.Name).ToList();

        /// <summary>ينشئ schema ai ويحدّث الـ Views (CREATE OR ALTER آمن للتكرار، ولا بيانات فيه).</summary>
        public static void Ensure(operations opp)
        {
            opp.db.Database.ExecuteSqlRaw("IF SCHEMA_ID(N'ai') IS NULL EXEC(N'CREATE SCHEMA ai');");
            bool hasBoxPerm = opp.db.Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN OBJECT_ID(N'dbo.SysConfigBanksPERMATIONS', N'U') IS NULL THEN 0 ELSE 1 END AS Value").AsEnumerable().First() == 1;
            bool hasTaxDoc = TableExists(opp, "dbo.Taxdocuments"), hasTaxDocRev = TableExists(opp, "dbo.TaxdocumentsReverse");
            bool hasCredit = TableExists(opp, "dbo.SalesRep_Credit_Policy");
            foreach (var (_, sql) in Views)
                opp.db.Database.ExecuteSqlRaw(sql.Replace(BoxPermToken, hasBoxPerm ? BoxPermExists : "1 = 0")
                    .Replace(TaxDocToken, hasTaxDoc ? "dbo.Taxdocuments" : TaxDocEmpty)
                    .Replace(TaxDocRevToken, hasTaxDocRev ? "dbo.TaxdocumentsReverse" : TaxDocEmpty)
                    .Replace(CreditToken, hasCredit ? "dbo.SalesRep_Credit_Policy" : CreditEmpty));
        }

        private static bool TableExists(operations opp, string name) =>
            opp.db.Database.SqlQueryRaw<int>("SELECT CASE WHEN OBJECT_ID({0}, N'U') IS NULL THEN 0 ELSE 1 END AS Value", name).AsEnumerable().First() == 1;
    }
}
