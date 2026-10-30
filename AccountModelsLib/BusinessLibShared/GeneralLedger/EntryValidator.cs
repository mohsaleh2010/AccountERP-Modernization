using System;
using System.Collections.Generic;
using System.Data;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// الحارس المركزى للقيد (ق5): تحقق واحد قبل الحفظ لأى مصدر. يعيد قائمة مخالفات ولا يرفض بنفسه —
    /// المستدعى يقرر حسب الوضع (Warn: يعرض ويكمل، Reject: يمنع الحفظ عند أى مخالفة).
    /// أعمدة details المطلوبة هى نفسها التى يستعملها AccountEntry_CLS: Submain_Account_ID, Import_Value, Export_Value,
    /// Equivalent_Import_Value, Equivalent_Export_Value, CostCenter_Id.
    /// لم تُضَف «الفترة مفتوحة»: أعلام Financial_Period (Done/IS_Active) لا تطابق الفترة الجارية فى البيانات، فيُكتفى بوجود الفترة.
    /// </summary>
    public static class EntryValidator
    {
        public enum Mode { Warn, Reject }

        public class Issue
        {
            public string Code { get; set; }
            public string Message { get; set; }
            public int? Line { get; set; }
            public override string ToString() => Line.HasValue ? "سطر " + Line + ": " + Message : Message;
        }

        /// <summary>الوضع الافتراضى تحذيرى (قرار 7-أ)؛ يُحوَّل إلى Reject بعد مراجعة النتائج.</summary>
        public static Mode CurrentMode = Mode.Warn;

        private const decimal Tolerance = 0.005m;

        public static List<Issue> Validate(operations opp, DataTable details, DateTime entryDate)
        {
            var issues = new List<Issue>();
            if (details == null || details.Rows.Count == 0)
            {
                issues.Add(new Issue { Code = "LINES", Message = "القيد بلا سطور" });
                return issues;
            }
            if (details.Rows.Count < 2) issues.Add(new Issue { Code = "LINES", Message = "القيد يحتاج سطرين على الأقل" });

            decimal imp = 0, exp = 0, eImp = 0, eExp = 0;
            for (int i = 0; i < details.Rows.Count; i++)
            {
                var r = details.Rows[i];
                decimal di = Dec(r, "Import_Value"), de = Dec(r, "Export_Value");
                if (di < 0 || de < 0) issues.Add(new Issue { Code = "NEG", Line = i + 1, Message = "قيمة سالبة" });
                if (di > 0 && de > 0) issues.Add(new Issue { Code = "BOTH", Line = i + 1, Message = "السطر مدين ودائن معاً" });
                if (di == 0 && de == 0) issues.Add(new Issue { Code = "ZERO", Line = i + 1, Message = "سطر بلا قيمة" });
                imp += di; exp += de;
                eImp += Dec(r, "Equivalent_Import_Value"); eExp += Dec(r, "Equivalent_Export_Value");
            }
            if (Math.Abs(imp - exp) > Tolerance)
                issues.Add(new Issue { Code = "BAL", Message = "القيد غير متزن: مدين " + imp.ToString("N2") + " ≠ دائن " + exp.ToString("N2") });
            if (Math.Abs(eImp - eExp) > Tolerance)
                issues.Add(new Issue { Code = "BALEQ", Message = "القيد غير متزن بالمكافئ: مدين " + eImp.ToString("N2") + " ≠ دائن " + eExp.ToString("N2") });

            var per = opp.return_DataTable("SELECT COUNT(*) AS C FROM Financial_Period WHERE DateFrom <= " + operations.Get_DateToInsert(entryDate.Date) +
                                           " AND DateTo >= " + operations.Get_DateToInsert(entryDate.Date));
            if (Convert.ToInt32(per.Rows[0]["C"]) == 0)
                issues.Add(new Issue { Code = "PERIOD", Message = "التاريخ " + entryDate.ToString("yyyy/MM/dd") + " خارج أى فترة مالية" });

            var ids = new List<string>();
            foreach (DataRow r in details.Rows) ids.Add(Convert.ToInt64(r["Submain_Account_ID"]).ToString());
            var acc = opp.return_DataTable("SELECT Id, IS_Last_Level, Accept_Cost_Center_Flage FROM Submain_Account WHERE Id IN (" + string.Join(",", ids) + ")");
            for (int i = 0; i < details.Rows.Count; i++)
            {
                long id = Convert.ToInt64(details.Rows[i]["Submain_Account_ID"]);
                var rows = acc.Select("Id = " + id);
                if (rows.Length == 0) { issues.Add(new Issue { Code = "ACC", Line = i + 1, Message = "الحساب غير موجود" }); continue; }
                if (Dec(rows[0], "IS_Last_Level") != 1) issues.Add(new Issue { Code = "LEAF", Line = i + 1, Message = "الحساب ليس حساب ورقة (غير نهائى)" });
                bool accepts = Dec(rows[0], "Accept_Cost_Center_Flage") == 1;
                long cc = 0;
                if (details.Columns.Contains("CostCenter_Id") && details.Rows[i]["CostCenter_Id"] != DBNull.Value) long.TryParse(details.Rows[i]["CostCenter_Id"].ToString(), out cc);
                if (!accepts && cc > 0) issues.Add(new Issue { Code = "CCNO", Line = i + 1, Message = "الحساب لا يقبل مركز تكلفة" });
            }
            return issues;
        }

        /// <summary>
        /// يفحص قيداً أُنشئ فعلاً (داخل معاملة المنشئ) بقراءة رأسه وسطوره؛ للمصادر الآلية التى يبنى قيدها كود قديم مشترك فيُفحص بعد البناء (وضع تحذيرى: لا رفض).
        /// </summary>
        public static List<Issue> ValidateSaved(operations opp, long entryId)
        {
            var head = opp.return_DataTable("SELECT Entry_Date FROM Account_Entry_List WHERE ID = " + entryId);
            if (head.Rows.Count == 0) return new List<Issue> { new Issue { Code = "LIST", Message = "القيد غير موجود" } };
            var details = opp.return_DataTable("SELECT Submain_Account_ID, Import_Value, Export_Value, Equivalent_Import_Value, Equivalent_Export_Value, CostCenter_Id " +
                                               "FROM Account_Entry_Details WHERE Account_Entry_List_ID = " + entryId + " ORDER BY Id");
            return Validate(opp, details, Convert.ToDateTime(head.Rows[0]["Entry_Date"]));
        }

        /// <summary>true إن كان الحفظ ممنوعاً حسب الوضع الحالى.</summary>
        public static bool ShouldBlock(List<Issue> issues) => CurrentMode == Mode.Reject && issues.Count > 0;

        private static decimal Dec(DataRow r, string col)
            => r.Table.Columns.Contains(col) && r[col] != DBNull.Value && decimal.TryParse(r[col].ToString(), out var d) ? d : 0m;
    }
}
