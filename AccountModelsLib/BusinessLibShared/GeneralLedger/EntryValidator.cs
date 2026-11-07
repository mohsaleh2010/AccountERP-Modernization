using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.BusinessLibShared.GeneralLedger
{
    /// <summary>
    /// الحارس المركزى للقيد (ق5): تحقق واحد قبل الحفظ لأى مصدر. يعيد قائمة مخالفات ولا يرفض بنفسه —
    /// المستدعى يقرر حسب الوضع (Warn: يعرض ويكمل، Reject: يمنع الحفظ عند أى مخالفة).
    /// أعمدة details المطلوبة هى نفسها التى يستعملها AccountEntry_CLS: Submain_Account_ID, Import_Value, Export_Value,
    /// Equivalent_Import_Value, Equivalent_Export_Value, CostCenter_Id.
    /// فحص PERIODCLOSED (Done/IS_Active) تحذيرى فقط: يظهر خطأً حتى تُضبط الفترة الجارية من شاشة الفترات.
    /// </summary>
    public static class EntryValidator
    {
        public enum Mode { Warn, Reject }

        public class Issue
        {
            public string Code { get; set; }
            public string Message { get; set; }
            public int? Line { get; set; }
            /// <summary>مخالفة لا تمنع الحفظ حتى فى وضع Reject (مراكز التكلفة فى وضعها التحذيرى).</summary>
            public bool WarnOnly { get; set; }
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

            var per = opp.return_DataTable("SELECT Done, IS_Active FROM Financial_Period WHERE DateFrom <= " + operations.Get_DateToInsert(entryDate.Date) +
                                           " AND DateTo >= " + operations.Get_DateToInsert(entryDate.Date));
            if (per.Rows.Count == 0)
                issues.Add(new Issue { Code = "PERIOD", Message = "التاريخ " + entryDate.ToString("yyyy/MM/dd") + " خارج أى فترة مالية" });
            else if (Dec(per.Rows[0], "Done") == 1 || Dec(per.Rows[0], "IS_Active") != 1)
                issues.Add(new Issue { Code = "PERIODCLOSED", Message = "الفترة المالية لتاريخ " + entryDate.ToString("yyyy/MM/dd") + " مغلقة أو غير نشطة (تحذير فقط)" });

            var ids = new List<string>();
            foreach (DataRow r in details.Rows) ids.Add(Convert.ToInt64(r["Submain_Account_ID"]).ToString());
            // Cost_Center_Policy (عمود جديد ق4) قد لا يكون موجوداً فى قاعدة لم تُرقَّ بعد: NULL/غيابه = اقرأ Accept_Cost_Center_Flage القديم.
            bool hasPolicy = opp.db.Database.SqlQueryRaw<int>("SELECT CASE WHEN COL_LENGTH(N'dbo.Submain_Account', N'Cost_Center_Policy') IS NULL THEN 0 ELSE 1 END AS Value").AsEnumerable().First() == 1;
            var acc = opp.return_DataTable("SELECT Id, IS_Last_Level, Accept_Cost_Center_Flage" + (hasPolicy ? ", Cost_Center_Policy" : "") +
                                           " FROM Submain_Account WHERE Id IN (" + string.Join(",", ids) + ")");
            var ccIds = new HashSet<long>();
            var ccOfLine = new long[details.Rows.Count];
            for (int i = 0; i < details.Rows.Count; i++)
            {
                if (details.Columns.Contains("CostCenter_Id") && details.Rows[i]["CostCenter_Id"] != DBNull.Value && long.TryParse(details.Rows[i]["CostCenter_Id"].ToString(), out long c) && c > 0)
                { ccOfLine[i] = c; ccIds.Add(c); }
            }
            var ccFacts = ccIds.Count == 0 ? null : CostCenterFacts(opp, ccIds);
            bool ccRejects = CostCenters.CostCenterService.RejectMode(opp);
            for (int i = 0; i < details.Rows.Count; i++)
            {
                long id = Convert.ToInt64(details.Rows[i]["Submain_Account_ID"]);
                var rows = acc.Select("Id = " + id);
                if (rows.Length == 0) { issues.Add(new Issue { Code = "ACC", Line = i + 1, Message = "الحساب غير موجود" }); continue; }
                if (Dec(rows[0], "IS_Last_Level") != 1) issues.Add(new Issue { Code = "LEAF", Line = i + 1, Message = "الحساب ليس حساب ورقة (غير نهائى)" });
                int policy = hasPolicy && rows[0]["Cost_Center_Policy"] != DBNull.Value ? (int)Dec(rows[0], "Cost_Center_Policy") : (Dec(rows[0], "Accept_Cost_Center_Flage") == 1 ? 1 : 0);
                long cc = ccOfLine[i];
                if (policy == 0 && cc > 0) issues.Add(new Issue { Code = "CCNO", Line = i + 1, Message = "الحساب لا يقبل مركز تكلفة" });
                if (policy == 2 && cc == 0) issues.Add(new Issue { Code = "CCREQ", Line = i + 1, Message = "الحساب يتطلب مركز تكلفة", WarnOnly = !ccRejects });
                if (cc > 0 && policy != 0 && ccFacts != null)
                {
                    if (!ccFacts.Exists.Contains(cc)) { issues.Add(new Issue { Code = "CCNF", Line = i + 1, Message = "مركز التكلفة غير موجود" }); continue; }
                    if (ccFacts.HasChildren.Contains(cc)) issues.Add(new Issue { Code = "CCLEAF", Line = i + 1, Message = "مركز التكلفة غير ورقى (له مراكز فرعية)", WarnOnly = !ccRejects });
                    if (ccFacts.Inactive.Contains(cc)) issues.Add(new Issue { Code = "CCCLOSED", Line = i + 1, Message = "مركز التكلفة موقوف أو مغلق", WarnOnly = !ccRejects });
                    // الربط (ق5): يُفحص فقط لحساب له ربط مضبوط، ويكفى ربط المركز أو أحد أسلافه (وراثة)
                    if (ccFacts.LinkedCenters.TryGetValue(id, out var linked) && linked.Count > 0 && !ccFacts.Ancestors[cc].Any(linked.Contains))
                        issues.Add(new Issue { Code = "CCLINK", Line = i + 1, Message = "مركز التكلفة غير مربوط بهذا الحساب", WarnOnly = !ccRejects });
                }
            }
            return issues;
        }

        private class CcFacts
        {
            public HashSet<long> Exists = new HashSet<long>();
            public HashSet<long> HasChildren = new HashSet<long>();
            public HashSet<long> Inactive = new HashSet<long>();
            public Dictionary<long, List<long>> Ancestors = new Dictionary<long, List<long>>();
            public Dictionary<long, HashSet<long>> LinkedCenters = new Dictionary<long, HashSet<long>>();
        }

        /// <summary>حقائق المراكز المستعملة فى القيد: وجود، ورقية، حالة، أسلاف، وروابط الحسابات.</summary>
        private static CcFacts CostCenterFacts(operations opp, HashSet<long> ccIds)
        {
            var f = new CcFacts();
            var tree = opp.return_DataTable("SELECT Id, ISNULL(Parent_Id,0) AS Parent_Id FROM Cost_Center_Tree");
            var parent = new Dictionary<long, long>();
            foreach (DataRow r in tree.Rows) { long id = Convert.ToInt64(r["Id"]); long p = Convert.ToInt64(r["Parent_Id"]); parent[id] = p; if (p != 0) f.HasChildren.Add(p); }
            foreach (long cc in ccIds)
            {
                if (parent.ContainsKey(cc)) f.Exists.Add(cc);
                var chain = new List<long>(); long cur = cc;
                while (cur != 0 && parent.TryGetValue(cur, out long p) && !chain.Contains(cur)) { chain.Add(cur); cur = p; }
                f.Ancestors[cc] = chain;
            }
            var state = opp.return_DataTable("SELECT Cost_Center_Id FROM Cost_Center_State WHERE Status <> 1");
            foreach (DataRow r in state.Rows) f.Inactive.Add(Convert.ToInt64(r[0]));
            var inactiveInChain = new HashSet<long>();
            foreach (long cc in ccIds) if (f.Ancestors[cc].Exists(f.Inactive.Contains)) inactiveInChain.Add(cc);
            f.Inactive = inactiveInChain;
            var links = opp.return_DataTable("SELECT Cost_Center_Id, Submain_Account_Id FROM Cost_Center_ForAccounts");
            foreach (DataRow r in links.Rows)
            {
                long a = Convert.ToInt64(r["Submain_Account_Id"]);
                if (!f.LinkedCenters.TryGetValue(a, out var set)) f.LinkedCenters[a] = set = new HashSet<long>();
                set.Add(Convert.ToInt64(r["Cost_Center_Id"]));
            }
            return f;
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

        /// <summary>
        /// يفحص كل قيود مستند واحد (الأصلى + قيود التكلفة/المخزون المرتبطة به) بعد بنائها. لا يرمى أبداً: فشل الفحص لا يمنع الحفظ.
        /// </summary>
        public static List<Issue> ValidateSource(operations opp, int[] referenceTypes, long referenceId)
        {
            var all = new List<Issue>();
            try
            {
                var ids = opp.return_DataTable("SELECT ID FROM Account_Entry_List WHERE Reference_Type IN (" + string.Join(",", referenceTypes) +
                                               ") AND Reference_Type_Table_Id = " + referenceId);
                foreach (DataRow r in ids.Rows) all.AddRange(ValidateSaved(opp, Convert.ToInt64(r[0])));
            }
            catch { }
            return all;
        }

        /// <summary>
        /// مخالفات القيود الآلية للمستند الأخير فى هذا الخيط (الوضع التحذيرى): تُمسح عند كل بناء جديد ويقرؤها المستدعى بعد الحفظ.
        /// </summary>
        [ThreadStatic] private static List<string> _autoWarnings;
        public static List<string> AutoWarnings => _autoWarnings ?? (_autoWarnings = new List<string>());

        public static void RecordAuto(List<Issue> issues, string label)
        {
            AutoWarnings.Clear();
            foreach (var i in issues) AutoWarnings.Add(label + " — " + i);
        }

        /// <summary>true إن كان الحفظ ممنوعاً حسب الوضع الحالى.</summary>
        public static bool ShouldBlock(List<Issue> issues) => CurrentMode == Mode.Reject && issues.Exists(i => !i.WarnOnly);

        private static decimal Dec(DataRow r, string col)
            => r.Table.Columns.Contains(col) && r[col] != DBNull.Value && decimal.TryParse(r[col].ToString(), out var d) ? d : 0m;
    }
}
