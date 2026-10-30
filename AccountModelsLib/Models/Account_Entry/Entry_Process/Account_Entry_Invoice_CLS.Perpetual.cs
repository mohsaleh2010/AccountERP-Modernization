using System;
using System.Data;

namespace AccountModelsLib
{
    /// <summary>
    /// نقاط الدخول الفعلية لقيد فاتورة البيع/المرتجع (نفس التوقيع القديم فلا تتغير الاستدعاءات فى الديسكتوب وRazor).
    /// الدالة القديمة محفوظة كما هى باسم *_Legacy للمراجعة فقط ولا تُستدعى من هنا إلا كأول خطوة.
    /// الجديدة = القديمة حرفياً ثم قيد تكلفة البضاعة المباعة المنفصل (InventoryPostingService.PostInvoiceCogsEntry)
    /// الذى لا يفعل شيئاً إلا لفرع طريقته «مستمر» وتاريخ الفاتورة ≥ تاريخ البدء.
    /// </summary>
    public partial class Account_Entry_Invoice_CLS
    {
        public void Make_EntryForInvoices(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Invoices_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            Make_EntryForInvoices_Legacy(ref Excp, ref Msg, ref opp, Invoices_List_Id, dtFrom, dtTo, IS_Posted_Flage);
            PostCogs(ref Excp, ref Msg, ref opp, Invoices_List_Id, false);
            Guard(Excp, opp, new[] { Common.Invoice_Type_Id, InventoryPostingService.InvoiceCogsReferenceType }, Invoices_List_Id, "فاتورة مبيعات");
        }

        public void Make_EntryForInvoices_Reverse(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Invoices_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            Make_EntryForInvoices_Reverse_Legacy(ref Excp, ref Msg, ref opp, Invoices_List_Id, dtFrom, dtTo, IS_Posted_Flage);
            PostCogs(ref Excp, ref Msg, ref opp, Invoices_List_Id, true);
            Guard(Excp, opp, new[] { Common.InvoiceReverse_Type_Id, InventoryPostingService.InvoiceReturnCogsReferenceType }, Invoices_List_Id, "مرتجع مبيعات");
        }

        // فواتير الشراء/مرتجع الشراء: القديمة حرفياً (مدين المشتريات) ثم قيد منفصل ينقل قيمة المخزنى من المشتريات إلى حساب المخزن (فرع «مستمر» فقط)
        public void Make_EntryFor_Bills(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Bills_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            Make_EntryFor_Bills_Legacy(ref Excp, ref Msg, ref opp, Bills_List_Id, dtFrom, dtTo, IS_Posted_Flage);
            PostBillStock(ref Excp, ref Msg, ref opp, Bills_List_Id, false);
            Guard(Excp, opp, new[] { Common.Bill_Type_Id, InventoryPostingService.BillStockReferenceType }, Bills_List_Id, "فاتورة مشتريات");
        }

        public void Make_EntryFor_Bills_Reverse(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Bills_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            Make_EntryFor_Bills_Reverse_Legacy(ref Excp, ref Msg, ref opp, Bills_List_Id, dtFrom, dtTo, IS_Posted_Flage);
            PostBillStock(ref Excp, ref Msg, ref opp, Bills_List_Id, true);
            Guard(Excp, opp, new[] { Common.BilleReverse_Type_Id, InventoryPostingService.BillReturnStockReferenceType }, Bills_List_Id, "مرتجع مشتريات");
        }

        // الحارس المركزى (وضع تحذيرى): يفحص القيود المبنية ويسجل المخالفات فى EntryValidator.AutoWarnings دون أى تغيير فى Excp/Msg أو الحفظ.
        private static void Guard(int excp, AccountModelsLib.operations opp, int[] refTypes, long docId, string label)
        {
            if (excp != 0 || docId <= 0) return;
            BusinessLibShared.GeneralLedger.EntryValidator.RecordAuto(
                BusinessLibShared.GeneralLedger.EntryValidator.ValidateSource(opp, refTypes, docId), label + " " + docId);
        }

        // نفس الحارس لقيود الشيكات (Reference_Type = 5): يُستدعى قبل Commit فى Order_Receipt_ChecksNewNew
        public static void GuardCheck(int excp, AccountModelsLib.operations opp, long checksMemoryId)
        {
            Guard(excp, opp, new[] { Common.Checks_Memory_Reference_Type }, checksMemoryId, "شيك");
        }

        // نفس الحارس لقيود التسويات المالية (Reference_Type = 26 للرأس والأصناف): يُستدعى بعد Make_EntryForMoney_Financial_Notice
        public static void GuardTaswea(int excp, AccountModelsLib.operations opp, long noticeId)
        {
            Guard(excp, opp, new[] { Common.Account_Type_Taswea_Malia }, noticeId, "تسوية مالية");
        }

        // نفس الحارس لقيود تقارير فحص السيارات (Reference_Type = Account_Type_ForCarsExaminations): يُستدعى بعد Make_EntryForCarsExaminations
        public static void GuardCarsExamination(int excp, AccountModelsLib.operations opp, long examinationId)
        {
            Guard(excp, opp, new[] { Common.Account_Type_ForCarsExaminations }, examinationId, "تقرير فحص سيارة");
        }

        private static void PostBillStock(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long billId, bool isReturn)
        {
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0 || Excp != 0 || billId <= 0) return;
            try
            {
                new InventoryPostingService().PostBillStockEntry(opp, billId, isReturn);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }

        private static void PostCogs(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long invoiceId, bool isReturn)
        {
            // نفس شرط القديمة: بلا قيود تلقائية للفواتير ⇒ لا قيد تكلفة. وفشل القيد الأصلى ⇒ لا نضيف فوقه.
            if (Common.IS_Make_AutomaticFromTar7eel_For_Invoice == 0 || Excp != 0 || invoiceId <= 0) return;
            try
            {
                new InventoryPostingService().PostInvoiceCogsEntry(opp, invoiceId, isReturn);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }
        }
    }
}
