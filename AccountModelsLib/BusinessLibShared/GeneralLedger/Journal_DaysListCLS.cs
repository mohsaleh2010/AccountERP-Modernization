using System;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    // 🟢 نسخة مُنقولة من General_Ledger.Journal_DaysListCLS (فى General_Ledger\General_Ledger\
    // Journal_DaysList\Journal_DaysListCLS.cs، مشروع WinForms فقط) - نُقلت إلى هنا لتصبح قابلة
    // للاستدعاء من Blazor أيضاً (نفس نمط نقل CurrencyCLS سابقاً)، مع تغيير واحد فقط ضروري:
    // استبدال MessageBox.Show المُستخدَم داخل الكلاس نفسه (غير متاح لغير WinForms) بنفس نمط
    // Financial_PeriodCLS الموجود بالفعل فى نفس المشروع (ref int Excp + ref string Msg بدل
    // عرض الرسالة مباشرة) - النصوص العربية والشروط والاستعلامات نفسها بالضبط دون أي تغيير.
    // النسخة القديمة فى General_Ledger لم تُحذف عمداً (يستخدمها داخلياً نموذج Journal_DaysListFrm
    // المُكرَّر الموجود فعلياً داخل مشروع General_Ledger نفسه، غير المُتاح من التطبيق الحي لكنه
    // لا يزال يُبنى ضمن ذلك المشروع) - النموذج الحي الفعلي (Accounts\General_LedgerAccount\
    // Journal_DaysList\Journal_DaysListFrm.cs) عُدِّل ليستخدم هذه النسخة الجديدة عبر using alias.
    public class Journal_DaysListCLS
    {
        public Journal_DaysListCLS()
        {
        }

        public DataTable Gey_Journal_Codes(ref operations opp)
        {
            opp = new operations();
            string mysql = "Select * from Journal_Codes Order By Code ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);
            return dt;
        }

        public void Add_Journal_Codes(ref int Excp, ref string Msg, string Code, string Journal_Name)
        {
            string mysql = " Select * from Journal_Codes Where Code=" + Code + "";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Msg = "الكود مستخدم من قبل";
                Excp = 1;
                return;
            }
            mysql = @" INSERT INTO Journal_Codes (Journal_Name, Code) VALUES  ('" + Journal_Name + "'," + Code + ") ";

            opp.insert_me(mysql);
            Excp = 0;
        }

        // 🟢 دالة جديدة ضرورية فقط لاختلاف نمط الواجهة: شاشة الويندوز فورمز تعدّل اسم اليومية عبر
        // شبكة مرتبطة (DataGridView) وتحفظ التعديلات دفعة واحدة عبر opp.ada.Update(dataTable) (زر
        // "حفظ") - آلية خاصة بـ SqlDataAdapter/DataTable-binding لا مقابل لها فى Blazor. هذه الدالة
        // تنفّذ نفس الأثر بالضبط (تحديث عمود Journal_Name لصف واحد بمعرفه Code) كاستدعاء صريح - لا
        // تُضيف أي قاعدة عمل جديدة لم تكن موجودة أصلاً فى شاشة الويندوز فورمز.
        public void Update_Journal_Name(ref int Excp, ref string Msg, string Code, string Journal_Name)
        {
            operations opp = new operations();
            try
            {
                string mysql = "Update Journal_Codes Set Journal_Name='" + Journal_Name + "' Where Code=" + Code + "";
                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }

        public void Delete_Journal_Codes(ref int Excp, ref string Msg, string Code)
        {
            string mysql = "Select * from Account_Entry_List Where journal_Code=" + Code + "";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                Msg = "لا يمكن حذف اليوميه نظرا لإستخدامها فى القيود";
                Excp = 1;
                return;
            }
            try
            {
                mysql = @" Delete From Journal_Codes Where Code=" + Code + "";

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }
    }
}
