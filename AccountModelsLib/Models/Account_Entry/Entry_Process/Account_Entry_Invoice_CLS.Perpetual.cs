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
        }

        public void Make_EntryForInvoices_Reverse(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Invoices_List_Id, DateTime dtFrom, DateTime dtTo, int IS_Posted_Flage)
        {
            Make_EntryForInvoices_Reverse_Legacy(ref Excp, ref Msg, ref opp, Invoices_List_Id, dtFrom, dtTo, IS_Posted_Flage);
            PostCogs(ref Excp, ref Msg, ref opp, Invoices_List_Id, true);
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
