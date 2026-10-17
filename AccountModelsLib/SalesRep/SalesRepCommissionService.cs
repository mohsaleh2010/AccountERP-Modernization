using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 6 (ب): الأهداف وخطط العمولة وحسابها واعتمادها وقيدها، ولوحة أداء المندوب.
    /// القياس (قرارات 2026-10-17 فى سجل العمل):
    /// - صافى المبيعات = فواتير المندوب المربوطة بوردياته فى الفترة بدون الضريبة (الضريبة تُطرح فقط لفاتورة ضريبية IsInvoiceTaxInfo=1)،
    ///   ناقص مرتجعاته المربوطة لو الخطة «تخصم المرتجعات».
    /// - التحصيل = أسطر خزنة العملاء فى إذونات النقدية المربوطة بوردياته (قبض − صرف) + الشيكات التى حصّلها.
    /// - هدف الصنف/المجموعة يُقاس بقيمة مبيعات الصنف أو المجموعة: قيمة الفاتورة بدون ضريبة موزعة على أسطرها بنسبة صافى كل سطر.
    /// - الشرائح: الشريحة التى بلغها المندوب تُطبَّق على كامل الأساس (ق: شريحة واحدة على الكل).
    /// - سرعة التحصيل: تحصيلات العميل تُوزَّع على أقدم فواتيره الآجلة أولاً (FIFO)؛ ما حصّله المندوب فى الفترة وسدّد فاتورة
    ///   خلال Collection_Speed_Days من تاريخها يأخذ Collection_Speed_Bonus_Rate %.
    /// - الاعتماد: من غير من حسب الدفعة (فصل المهام)، ويُنشئ قيداً: مدين حساب المصروف فى الخطة / دائن حساب الموظف، بفرع المندوب.
    /// </summary>
    public class SalesRepCommissionService
    {
        /// <summary>Reference_Type لقيد العمولة (Reference_Type_Table_Id = رقم الدفعة).</summary>
        public const int CommissionReferenceType = 3002;

        private readonly operations _external;
        public SalesRepCommissionService(operations external = null) { _external = external; }

        // ============================== الأنواع ==============================

        public class Option { public long Id { get; set; } public string Name { get; set; } = ""; }

        public class TargetRow
        {
            public long Id { get; set; }
            public long Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public DateTime Period_From { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            public DateTime Period_To { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);
            public int Scope { get; set; }
            public long? Scope_Id { get; set; }
            public string Scope_Name { get; set; } = "";
            public decimal? Target_Qty { get; set; }
            public decimal? Target_Value { get; set; }
        }

        public class TierRow
        {
            public long Id { get; set; }
            public int Component { get; set; }
            public decimal Achievement_From { get; set; }
            public decimal? Achievement_To { get; set; }
            public decimal? Rate { get; set; }
            public decimal? Fixed_Amount { get; set; }
        }

        public class PlanRow
        {
            public long Id { get; set; }
            public string Name { get; set; } = "";
            public bool Deduct_Returns { get; set; } = true;
            public int? Collection_Speed_Days { get; set; }
            public decimal? Collection_Speed_Bonus_Rate { get; set; }
            public long? Expense_Account_Id { get; set; }
            public string Expense_Account_Name { get; set; } = "";
            public bool Active { get; set; } = true;
            public int Reps_Count { get; set; }
            [System.ComponentModel.DataAnnotations.Schema.NotMapped] public List<TierRow> Tiers { get; set; } = new();
        }

        public class RepPlanRow
        {
            public long Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public long? Plan_Id { get; set; }
            public string Plan_Name { get; set; } = "";
            public bool Active { get; set; }
        }

        public class RunRow
        {
            public long Id { get; set; }
            public long Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public long Plan_Id { get; set; }
            public string Plan_Name { get; set; } = "";
            public DateTime Period_From { get; set; }
            public DateTime Period_To { get; set; }
            public int Status { get; set; }
            public decimal Total_Amount { get; set; }
            public DateTime? Approved_At { get; set; }
            public long? Account_Entry_List_ID { get; set; }
            public long? Entry_No { get; set; }
            public DateTime? Insert_ServerDateTime { get; set; }
        }

        public class RunLineRow
        {
            public long Id { get; set; }
            public int Component { get; set; }
            public long? Scope_Id { get; set; }
            public decimal Base_Amount { get; set; }
            public decimal? Target_Value { get; set; }
            public decimal? Achievement_Rate { get; set; }
            public decimal? Rate { get; set; }
            public decimal Amount { get; set; }
            public string Notes { get; set; } = "";
        }

        /// <summary>أرقام المندوب فى فترة (أساس العمولة ولوحة الأداء).</summary>
        public class Measures
        {
            public decimal Gross_Sales { get; set; }
            public decimal Returns { get; set; }
            public decimal Net_Sales { get; set; }
            public decimal Collection { get; set; }
            public int Invoices { get; set; }
            public int New_Customers { get; set; }
            public Dictionary<long, decimal> Item_Sales { get; set; } = new();
            public Dictionary<long, decimal> Group_Sales { get; set; } = new();
        }

        public class TargetProgress
        {
            public long Target_Id { get; set; }
            public int Scope { get; set; }
            public long? Scope_Id { get; set; }
            public string Scope_Name { get; set; } = "";
            public DateTime Period_From { get; set; }
            public DateTime Period_To { get; set; }
            public decimal Target { get; set; }
            public decimal Actual { get; set; }
            public decimal? Achievement { get; set; }
        }

        public class ReasonCount { public string Name { get; set; } = ""; public int Count { get; set; } }

        public class AgingRow
        {
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public decimal Balance { get; set; }
            public decimal B0_30 { get; set; }
            public decimal B31_60 { get; set; }
            public decimal B61_90 { get; set; }
            public decimal B90 { get; set; }
        }

        public class Performance
        {
            public Measures Measures { get; set; } = new();
            public int Shifts { get; set; }
            public int Planned_Visits { get; set; }
            public int Planned_Done { get; set; }
            public int Missed { get; set; }
            public int Unplanned { get; set; }
            public int No_Sale { get; set; }
            public int Productive { get; set; }
            public List<ReasonCount> No_Sale_Reasons { get; set; } = new();
            public List<TargetProgress> Targets { get; set; } = new();
            public List<AgingRow> Aging { get; set; } = new();
        }

        // ============================== قوائم مساعدة ==============================

        public List<Option> GetReps() => Read(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT r.Id, ISNULL(e.name, N'') AS Name FROM SalesRep_Reps r LEFT JOIN Employee e ON e.Id = r.Employee_Id ORDER BY e.name").ToList());

        public List<Option> SearchItems(string search, int take = 50) => Read(opp =>
        {
            string like = (search ?? "").Trim();
            return opp.db.Database.SqlQuery<Option>($@"SELECT TOP ({take}) CAST(id AS bigint) AS Id, ISNULL(Item_Name, N'') AS Name FROM Item
WHERE ISNULL(Active_flage, 1) = 1 AND ({like} = N'' OR Item_Name LIKE N'%' + {like} + N'%') ORDER BY Item_Name").ToList();
        });

        public List<Option> SearchItemGroups(string search, int take = 50) => Read(opp =>
        {
            string like = (search ?? "").Trim();
            return opp.db.Database.SqlQuery<Option>($@"SELECT TOP ({take}) CAST(Id AS bigint) AS Id, ISNULL(Name, N'') AS Name FROM Items_Group
WHERE {like} = N'' OR Name LIKE N'%' + {like} + N'%' ORDER BY Name").ToList();
        });

        /// <summary>حسابات الدليل الطرفية (لا حساب تحتها بالكود) للبحث عن حساب مصروف العمولة.</summary>
        public List<Option> SearchAccounts(string search, int take = 50) => Read(opp =>
        {
            string like = (search ?? "").Trim();
            return opp.db.Database.SqlQuery<Option>($@"SELECT TOP ({take}) s.Id, ISNULL(s.SubAccount_NO, N'') + N' — ' + ISNULL(s.SubAccount_Name, N'') AS Name FROM Submain_Account s
WHERE NOT EXISTS (SELECT 1 FROM Submain_Account c WHERE c.SubAccount_NO LIKE s.SubAccount_NO + N'-%')
  AND ({like} = N'' OR s.SubAccount_Name LIKE N'%' + {like} + N'%' OR s.SubAccount_NO LIKE {like} + N'%')
ORDER BY s.SubAccount_NO").ToList();
        });

        // ============================== الأهداف ==============================

        public List<TargetRow> GetTargets(long? repId, DateTime? from, DateTime? to) => Read(opp =>
        {
            long rep = repId ?? 0;
            DateTime f = (from ?? new DateTime(1900, 1, 1)).Date, t = (to ?? new DateTime(9999, 12, 31)).Date;
            return opp.db.Database.SqlQuery<TargetRow>($@"
SELECT g.Id, g.Rep_Id, ISNULL(e.name, N'') AS Rep_Name, g.Period_From, g.Period_To, g.Scope, g.Scope_Id,
       ISNULL(CASE g.Scope WHEN {(int)SalesRepTargetScope.Item} THEN i.Item_Name WHEN {(int)SalesRepTargetScope.ItemGroup} THEN ig.Name END, N'') AS Scope_Name,
       g.Target_Qty, g.Target_Value
FROM SalesRep_Targets g
LEFT JOIN SalesRep_Reps r ON r.Id = g.Rep_Id LEFT JOIN Employee e ON e.Id = r.Employee_Id
LEFT JOIN Item i ON g.Scope = {(int)SalesRepTargetScope.Item} AND i.id = g.Scope_Id
LEFT JOIN Items_Group ig ON g.Scope = {(int)SalesRepTargetScope.ItemGroup} AND ig.Id = g.Scope_Id
WHERE ({rep} = 0 OR g.Rep_Id = {rep}) AND g.Period_To >= {f} AND g.Period_From <= {t}
ORDER BY g.Period_From DESC, e.name, g.Scope").ToList();
        });

        /// <summary>
        /// الهدف: الإجمالى والتحصيل والصنف والمجموعة بالقيمة؛ العملاء الجدد بالعدد (Target_Qty).
        /// لا يتكرر نفس الهدف (مندوب + نوع + صنف/مجموعة) فى فترتين متداخلتين.
        /// </summary>
        public long SaveTarget(TargetRow input) => Tx(opp =>
        {
            if (input == null) throw new InvalidOperationException("بيانات الهدف ناقصة.");
            if (!opp.db.SalesRep_Reps.AsNoTracking().Any(r => r.Id == input.Rep_Id)) throw new InvalidOperationException("اختر المندوب.");
            if (!Enum.IsDefined(typeof(SalesRepTargetScope), input.Scope)) throw new InvalidOperationException("نوع الهدف غير صحيح.");
            if (input.Period_To.Date < input.Period_From.Date) throw new InvalidOperationException("نهاية الفترة قبل بدايتها.");
            var scope = (SalesRepTargetScope)input.Scope;
            bool scoped = scope is SalesRepTargetScope.Item or SalesRepTargetScope.ItemGroup;
            if (scoped && (input.Scope_Id ?? 0) <= 0) throw new InvalidOperationException(scope == SalesRepTargetScope.Item ? "اختر الصنف." : "اختر مجموعة الأصناف.");
            if (scope == SalesRepTargetScope.NewCustomers)
            {
                if ((input.Target_Qty ?? 0) <= 0) throw new InvalidOperationException("حدد عدد العملاء الجدد المستهدف.");
            }
            else if ((input.Target_Value ?? 0) <= 0) throw new InvalidOperationException("حدد قيمة الهدف.");
            long? scopeId = scoped ? input.Scope_Id : null;
            DateTime f = input.Period_From.Date, t = input.Period_To.Date;
            if (opp.db.SalesRep_Targets.AsNoTracking().Any(g => g.Id != input.Id && g.Rep_Id == input.Rep_Id && g.Scope == input.Scope && g.Scope_Id == scopeId
                                                            && g.Period_From <= t && g.Period_To >= f))
                throw new InvalidOperationException("يوجد هدف بنفس النوع لهذا المندوب فى فترة متداخلة.");

            SalesRep_Target g;
            if (input.Id == 0)
            {
                g = new SalesRep_Target { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                g.StampInsert(RepBranch(opp, input.Rep_Id));
                opp.db.SalesRep_Targets.Add(g);
            }
            else
            {
                g = opp.db.SalesRep_Targets.FirstOrDefault(x => x.Id == input.Id) ?? throw new InvalidOperationException("الهدف غير موجود.");
                g.StampEdit();
            }
            g.Rep_Id = input.Rep_Id; g.Period_From = f; g.Period_To = t; g.Scope = input.Scope; g.Scope_Id = scopeId;
            g.Target_Qty = scope == SalesRepTargetScope.NewCustomers ? Math.Round(input.Target_Qty ?? 0, 0) : null;
            g.Target_Value = scope == SalesRepTargetScope.NewCustomers ? null : Math.Round(input.Target_Value ?? 0, 2);
            opp.db.SaveChanges();
            return g.Id;
        });

        /// <summary>الهدف إعداد؛ الدفعات المحسوبة تحفظ قيمته فى أسطرها فحذفه لا يغيّرها.</summary>
        public void DeleteTarget(long id) => Tx(opp =>
        {
            var g = opp.db.SalesRep_Targets.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("الهدف غير موجود.");
            opp.db.SalesRep_Targets.Remove(g);
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== خطط العمولة ==============================

        public List<PlanRow> GetPlans(bool activeOnly = false) => Read(opp =>
        {
            var plans = opp.db.Database.SqlQuery<PlanRow>($@"
SELECT p.Id, p.Name, p.Deduct_Returns, p.Collection_Speed_Days, p.Collection_Speed_Bonus_Rate, p.Expense_Account_Id,
       ISNULL(s.SubAccount_NO + N' — ' + s.SubAccount_Name, N'') AS Expense_Account_Name, p.Active,
       (SELECT COUNT(*) FROM SalesRep_Reps r WHERE r.Commission_Plan_Id = p.Id) AS Reps_Count
FROM SalesRep_Commission_Plans p LEFT JOIN Submain_Account s ON s.Id = p.Expense_Account_Id
WHERE {(activeOnly ? 1 : 0)} = 0 OR p.Active = 1
ORDER BY p.Active DESC, p.Name").ToList();
            var ids = plans.Select(p => p.Id).ToList();
            var tiers = opp.db.SalesRep_Commission_Plan_Tiers.AsNoTracking().Where(t => ids.Contains(t.Plan_Id)).ToList();
            foreach (var p in plans)
                p.Tiers = tiers.Where(t => t.Plan_Id == p.Id).OrderBy(t => t.Component).ThenBy(t => t.Achievement_From)
                    .Select(t => new TierRow { Id = t.Id, Component = t.Component, Achievement_From = t.Achievement_From, Achievement_To = t.Achievement_To, Rate = t.Rate, Fixed_Amount = t.Fixed_Amount })
                    .ToList();
            return plans;
        });

        /// <summary>
        /// يحفظ الخطة وشرائحها معاً (الشرائح تُستبدل بالكامل؛ الدفعات السابقة تحفظ نسبها فى أسطرها).
        /// شرائح كل مكوّن: من &gt;= 0، إلى &gt; من (فارغ = بلا حد)، ولا تتداخل؛ ولكل شريحة نسبة أو مبلغ ثابت.
        /// </summary>
        public long SavePlan(PlanRow input) => Tx(opp =>
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Name)) throw new InvalidOperationException("اكتب اسم الخطة.");
            string name = Trim(input.Name.Trim(), 200);
            if (opp.db.SalesRep_Commission_Plans.AsNoTracking().Any(p => p.Id != input.Id && p.Name == name)) throw new InvalidOperationException("يوجد خطة بنفس الاسم.");
            if ((input.Expense_Account_Id ?? 0) <= 0) throw new InvalidOperationException("اختر حساب مصروف العمولة.");
            if (opp.return_DataTable("SELECT Id FROM Submain_Account WHERE Id = " + input.Expense_Account_Id.Value).Rows.Count == 0)
                throw new InvalidOperationException("حساب المصروف غير موجود فى الدليل.");
            if (input.Collection_Speed_Days is < 0 or > 3650) throw new InvalidOperationException("أيام سرعة التحصيل بين 0 و3650.");
            if (input.Collection_Speed_Bonus_Rate is < 0 or > 100) throw new InvalidOperationException("نسبة مكافأة سرعة التحصيل بين 0 و100.");
            if ((input.Collection_Speed_Days == null) != (input.Collection_Speed_Bonus_Rate == null))
                throw new InvalidOperationException("مكافأة سرعة التحصيل تحتاج الأيام والنسبة معاً.");
            ValidateTiers(input.Tiers ?? new List<TierRow>());

            SalesRep_Commission_Plan p;
            if (input.Id == 0)
            {
                p = new SalesRep_Commission_Plan { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                p.StampInsert(Common.Branch_Id);
                opp.db.SalesRep_Commission_Plans.Add(p);
            }
            else
            {
                p = opp.db.SalesRep_Commission_Plans.FirstOrDefault(x => x.Id == input.Id) ?? throw new InvalidOperationException("الخطة غير موجودة.");
                p.StampEdit();
                opp.db.SalesRep_Commission_Plan_Tiers.RemoveRange(opp.db.SalesRep_Commission_Plan_Tiers.Where(t => t.Plan_Id == p.Id));
            }
            p.Name = name; p.Deduct_Returns = input.Deduct_Returns; p.Active = input.Active; p.Expense_Account_Id = input.Expense_Account_Id;
            p.Collection_Speed_Days = input.Collection_Speed_Days;
            p.Collection_Speed_Bonus_Rate = input.Collection_Speed_Bonus_Rate is decimal b ? Math.Round(b, 4) : null;
            foreach (var t in input.Tiers ?? new List<TierRow>())
            {
                var row = new SalesRep_Commission_Plan_Tier
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(), Plan_Id = p.Id, Component = t.Component,
                    Achievement_From = Math.Round(t.Achievement_From, 4), Achievement_To = t.Achievement_To is decimal to ? Math.Round(to, 4) : null,
                    Rate = t.Rate is decimal r ? Math.Round(r, 4) : null, Fixed_Amount = t.Fixed_Amount is decimal fx ? Math.Round(fx, 2) : null,
                };
                row.StampInsert(Common.Branch_Id);
                opp.db.SalesRep_Commission_Plan_Tiers.Add(row);
            }
            opp.db.SaveChanges();
            return p.Id;
        });

        private static void ValidateTiers(List<TierRow> tiers)
        {
            foreach (var t in tiers)
            {
                if (!Enum.IsDefined(typeof(SalesRepCommissionComponent), t.Component)) throw new InvalidOperationException("مكوّن الشريحة غير صحيح.");
                if (t.Achievement_From < 0) throw new InvalidOperationException("بداية الشريحة لا تقل عن صفر.");
                if (t.Achievement_To is decimal to && to <= t.Achievement_From) throw new InvalidOperationException("نهاية الشريحة يجب أن تزيد على بدايتها.");
                if ((t.Rate ?? 0) <= 0 && (t.Fixed_Amount ?? 0) <= 0) throw new InvalidOperationException("لكل شريحة نسبة أو مبلغ ثابت.");
                if (t.Rate is < 0 or > 100) throw new InvalidOperationException("نسبة الشريحة بين 0 و100.");
                if (t.Fixed_Amount < 0) throw new InvalidOperationException("المبلغ الثابت لا يكون سالباً.");
            }
            foreach (var g in tiers.GroupBy(t => t.Component))
            {
                var list = g.OrderBy(t => t.Achievement_From).ToList();
                for (int i = 1; i < list.Count; i++)
                {
                    decimal prevTo = list[i - 1].Achievement_To ?? decimal.MaxValue;
                    if (list[i].Achievement_From < prevTo) throw new InvalidOperationException("شرائح نفس المكوّن متداخلة.");
                }
            }
        }

        /// <summary>الخطة المستعملة فى دفعات أو المسندة لمندوبين لا تُحذف (أوقفها بدلاً من ذلك).</summary>
        public void DeletePlan(long id) => Tx(opp =>
        {
            var p = opp.db.SalesRep_Commission_Plans.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("الخطة غير موجودة.");
            if (opp.db.SalesRep_Commission_Runs.AsNoTracking().Any(r => r.Plan_Id == id)) throw new InvalidOperationException("الخطة لها دفعات عمولة؛ أوقفها بدلاً من حذفها.");
            if (opp.db.SalesRep_Reps.AsNoTracking().Any(r => r.Commission_Plan_Id == id)) throw new InvalidOperationException("الخطة مسندة لمندوبين؛ غيّر خطتهم أولاً.");
            opp.db.SalesRep_Commission_Plan_Tiers.RemoveRange(opp.db.SalesRep_Commission_Plan_Tiers.Where(t => t.Plan_Id == id));
            opp.db.SalesRep_Commission_Plans.Remove(p);
            opp.db.SaveChanges();
            return 0;
        });

        public List<RepPlanRow> GetRepPlans() => Read(opp => opp.db.Database.SqlQuery<RepPlanRow>($@"
SELECT r.Id AS Rep_Id, ISNULL(e.name, N'') AS Rep_Name, r.Commission_Plan_Id AS Plan_Id, ISNULL(p.Name, N'') AS Plan_Name, r.Active
FROM SalesRep_Reps r LEFT JOIN Employee e ON e.Id = r.Employee_Id LEFT JOIN SalesRep_Commission_Plans p ON p.Id = r.Commission_Plan_Id
ORDER BY r.Active DESC, e.name").ToList());

        /// <summary>يسند خطة عمولة للمندوب (null = بلا خطة). الخطة الموقوفة لا تُسند.</summary>
        public void SetRepPlan(long repId, long? planId) => Tx(opp =>
        {
            var rep = opp.db.SalesRep_Reps.FirstOrDefault(r => r.Id == repId) ?? throw new InvalidOperationException("ملف المندوب غير موجود.");
            if (planId is long pid && !opp.db.SalesRep_Commission_Plans.AsNoTracking().Any(p => p.Id == pid && p.Active))
                throw new InvalidOperationException("الخطة غير موجودة أو موقوفة.");
            rep.Commission_Plan_Id = planId;
            rep.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== القياس ==============================

        private class DocValue { public long Id { get; set; } public long Customer_Id { get; set; } public decimal Value { get; set; } }
        private class LineValue { public long Invoice_Id { get; set; } public long Item_Id { get; set; } public long Group_Id { get; set; } public decimal Weight { get; set; } }

        private const string SaleExTax = "CAST(ISNULL(i.Net_Total, 0) - CASE WHEN ISNULL(i.IsInvoiceTaxInfo, 0) = 1 THEN ISNULL(i.TaxSales10_Value, 0) ELSE 0 END AS decimal(18,4))";

        private static string ShiftsOf(long repId, DateTime from, DateTime to) =>
            $"SELECT s.Id FROM SalesRep_Shifts s WHERE s.Rep_Id = {repId} AND s.Shift_Date >= '{from:yyyy-MM-dd}' AND s.Shift_Date <= '{to:yyyy-MM-dd}'";

        /// <summary>مبيعات ومرتجعات وتحصيل المندوب وعملاؤه الجدد ومبيعات كل صنف ومجموعة فى الفترة (بتاريخ الوردية).</summary>
        public Measures Measure(long repId, DateTime from, DateTime to) => Read(opp => Measure(opp, repId, from.Date, to.Date, true));

        private static Measures Measure(operations opp, long repId, DateTime from, DateTime to, bool deductReturns)
        {
            string shifts = ShiftsOf(repId, from, to);
            var m = new Measures();
            var sales = opp.db.Database.SqlQueryRaw<DocValue>($@"
SELECT i.Id, CAST(ISNULL(i.Customer_Id, 0) AS bigint) AS Customer_Id, {SaleExTax} AS Value
FROM SalesRep_Document_Links l JOIN Invoices_List i ON i.Id = l.Doc_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.SaleInvoice} AND l.Shift_Id IN ({shifts})").ToList();
            var returns = opp.db.Database.SqlQueryRaw<DocValue>($@"
SELECT i.Id, CAST(ISNULL(i.Customer_Id, 0) AS bigint) AS Customer_Id, {SaleExTax} AS Value
FROM SalesRep_Document_Links l JOIN Invoices_List_Reverse i ON i.Id = l.Doc_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.SaleReturn} AND l.Shift_Id IN ({shifts})").ToList();
            m.Invoices = sales.Count;
            m.Gross_Sales = Common.RoundMe(sales.Sum(s => s.Value));
            m.Returns = Common.RoundMe(returns.Sum(s => s.Value));
            m.Net_Sales = Common.RoundMe(m.Gross_Sales - (deductReturns ? m.Returns : 0));

            // التحصيل: أسطر خزنة العملاء فى إذونات الوردية (قبض موجب، صرف سالب) + الشيكات المحصّلة.
            m.Collection = Common.RoundMe(opp.db.Database.SqlQueryRaw<decimal>($@"
SELECT CAST(ISNULL(SUM(CASE WHEN b.Transaction_Type = 2 THEN -1 ELSE 1 END * ISNULL(b.Transaction_Value, 0)), 0) AS decimal(18,4)) AS Value
FROM SalesRep_Document_Links l JOIN Money_Transaction_Box b ON b.Order_Mony_Id = l.Doc_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.CashVoucher} AND l.Shift_Id IN ({shifts}) AND ISNULL(b.Customer_Id, 0) > 0").AsEnumerable().First()
              + opp.db.Database.SqlQueryRaw<decimal>($@"
SELECT CAST(ISNULL(SUM(ISNULL(m.Check_Value, 0)), 0) AS decimal(18,4)) AS Value
FROM SalesRep_Document_Links l JOIN Checks_Memory m ON m.Id = l.Doc_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.Cheque} AND l.Shift_Id IN ({shifts})").AsEnumerable().First());

            // عميل جديد = أول فاتورة له فى النظام كله فاتورة للمندوب داخل الفترة.
            m.New_Customers = opp.db.Database.SqlQueryRaw<int>($@"
SELECT COUNT(DISTINCT i.Customer_Id) AS Value
FROM SalesRep_Document_Links l JOIN Invoices_List i ON i.Id = l.Doc_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.SaleInvoice} AND l.Shift_Id IN ({shifts}) AND ISNULL(i.Customer_Id, 0) > 0
  AND NOT EXISTS (SELECT 1 FROM Invoices_List o WHERE o.Customer_Id = i.Customer_Id AND o.Id <> i.Id
                  AND (o.Invoice_Date < i.Invoice_Date OR (o.Invoice_Date = i.Invoice_Date AND o.Id < i.Id)))").AsEnumerable().First();

            // مبيعات الصنف/المجموعة: قيمة الفاتورة بدون ضريبة موزعة على أسطرها بنسبة صافى السطر.
            AddLines(opp, m, sales, $@"
SELECT d.Invoice_Id, CAST(ISNULL(d.Item_Id, 0) AS bigint) AS Item_Id, CAST(ISNULL(it.Items_GroupsId, 0) AS bigint) AS Group_Id,
       CAST(ISNULL(d.Item_Net_Total_Price, ISNULL(d.Total, 0)) AS decimal(18,4)) AS Weight
FROM SalesRep_Document_Links l JOIN Invoices_Details d ON d.Invoice_Id = l.Doc_Id LEFT JOIN Item it ON it.id = d.Item_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.SaleInvoice} AND l.Shift_Id IN ({shifts})", 1);
            if (deductReturns)
                AddLines(opp, m, returns, $@"
SELECT d.Invoice_Id, CAST(ISNULL(d.Item_Id, 0) AS bigint) AS Item_Id, CAST(ISNULL(it.Items_GroupsId, 0) AS bigint) AS Group_Id,
       CAST(ISNULL(d.Item_Net_Total_Price, ISNULL(d.Total, 0)) AS decimal(18,4)) AS Weight
FROM SalesRep_Document_Links l JOIN Invoices_Details_Reverse d ON d.Invoice_Id = l.Doc_Id LEFT JOIN Item it ON it.id = d.Item_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.SaleReturn} AND l.Shift_Id IN ({shifts})", -1);
            return m;
        }

        private static void AddLines(operations opp, Measures m, List<DocValue> docs, string sql, int sign)
        {
            var lines = opp.db.Database.SqlQueryRaw<LineValue>(sql).ToList();
            foreach (var doc in docs)
            {
                var mine = lines.Where(l => l.Invoice_Id == doc.Id).ToList();
                decimal w = mine.Sum(l => l.Weight);
                foreach (var l in mine)
                {
                    decimal share = w != 0 ? doc.Value * l.Weight / w : (mine.Count > 0 ? doc.Value / mine.Count : 0);
                    m.Item_Sales[l.Item_Id] = (m.Item_Sales.TryGetValue(l.Item_Id, out var a) ? a : 0) + sign * share;
                    if (l.Group_Id > 0) m.Group_Sales[l.Group_Id] = (m.Group_Sales.TryGetValue(l.Group_Id, out var g) ? g : 0) + sign * share;
                }
            }
        }

        private class Receipt { public long Id { get; set; } public int Kind { get; set; } public DateTime Date { get; set; } public decimal Value { get; set; } public long Invoice_Id { get; set; } }
        private class CreditInvoice { public long Id { get; set; } public DateTime Date { get; set; } public decimal Value { get; set; } }

        /// <summary>
        /// قيمة ما حصّله المندوب فى الفترة وسدّد فواتير آجلة خلال «days» يوماً من تاريخها (FIFO على أقدم فواتير العميل الآجلة).
        /// الدفعات المربوطة بفاتورة بعينها (مقدّم البيع) تخصم من فاتورتها أولاً ولا تحتسب مكافأة. الرصيد الافتتاحى والمرتجعات لا تدخل التوزيع.
        /// </summary>
        private static decimal FastCollected(operations opp, long repId, DateTime from, DateTime to, int days)
        {
            string shifts = ShiftsOf(repId, from, to);
            var repBox = opp.db.Database.SqlQueryRaw<long>($@"
SELECT b.Id AS Value FROM SalesRep_Document_Links l JOIN Money_Transaction_Box b ON b.Order_Mony_Id = l.Doc_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.CashVoucher} AND l.Shift_Id IN ({shifts}) AND ISNULL(b.Customer_Id, 0) > 0 AND b.Transaction_Type = 1").ToHashSet();
            var repCheques = opp.db.Database.SqlQueryRaw<long>($@"
SELECT l.Doc_Id AS Value FROM SalesRep_Document_Links l WHERE l.Doc_Type = {(int)SalesRepDocType.Cheque} AND l.Shift_Id IN ({shifts})").ToHashSet();
            if (repBox.Count == 0 && repCheques.Count == 0) return 0;

            string boxIds = string.Join(",", repBox.DefaultIfEmpty(0)), chqIds = string.Join(",", repCheques.DefaultIfEmpty(0));
            var customers = opp.db.Database.SqlQueryRaw<long>($@"
SELECT DISTINCT CAST(Customer_Id AS bigint) AS Value FROM Money_Transaction_Box WHERE Id IN ({boxIds})
UNION SELECT DISTINCT CAST(Customer_Id AS bigint) FROM Checks_Memory WHERE Id IN ({chqIds}) AND ISNULL(Customer_Id, 0) > 0").ToList();

            DateTime end = to.Date.AddDays(1);
            decimal fast = 0;
            foreach (long c in customers)
            {
                var invoices = opp.db.Database.SqlQueryRaw<CreditInvoice>($@"
SELECT i.Id, CAST(i.Invoice_Date AS datetime) AS Date, CAST(ISNULL(i.Net_Total, 0) AS decimal(18,4)) AS Value
FROM Invoices_List i WHERE i.Customer_Id = {c} AND ISNULL(i.Is_Invoice_Agile, 0) = 1 AND i.Invoice_Date < '{end:yyyy-MM-dd}'
ORDER BY i.Invoice_Date, i.Id").ToList();
                var receipts = opp.db.Database.SqlQueryRaw<Receipt>($@"
SELECT b.Id, 0 AS Kind, CAST(ISNULL(b.Transaction_Date, b.Insert_ServerDateTime) AS datetime) AS Date, CAST(ISNULL(b.Transaction_Value, 0) AS decimal(18,4)) AS Value,
       CAST(CASE WHEN ISNULL(b.DataDictionaryProcessesListsId, 0) = 1 THEN ISNULL(b.Invoice_Id, 0) ELSE 0 END AS bigint) AS Invoice_Id
FROM Money_Transaction_Box b WHERE b.Customer_Id = {c} AND b.Transaction_Type = 1 AND ISNULL(b.Transaction_Date, b.Insert_ServerDateTime) < '{end:yyyy-MM-dd}'
UNION ALL
SELECT m.Id, 1, CAST(ISNULL(m.Day_Date, m.Insert_ServerDateTime) AS datetime), CAST(ISNULL(m.Check_Value, 0) AS decimal(18,4)), CAST(0 AS bigint)
FROM Checks_Memory m WHERE m.Customer_Id = {c} AND ISNULL(m.Day_Date, m.Insert_ServerDateTime) < '{end:yyyy-MM-dd}'").ToList();

                var open = invoices.ToDictionary(i => i.Id, i => i.Value);
                foreach (var r in receipts.Where(r => r.Invoice_Id > 0 && open.ContainsKey(r.Invoice_Id)))
                    open[r.Invoice_Id] = Math.Max(0, open[r.Invoice_Id] - r.Value);

                foreach (var r in receipts.Where(r => r.Invoice_Id == 0).OrderBy(r => r.Date).ThenBy(r => r.Id))
                {
                    bool mine = r.Kind == 0 ? repBox.Contains(r.Id) : repCheques.Contains(r.Id);
                    decimal left = r.Value;
                    foreach (var inv in invoices)
                    {
                        if (left <= 0) break;
                        decimal due = open[inv.Id];
                        if (due <= 0) continue;
                        decimal take = Math.Min(due, left);
                        open[inv.Id] = due - take;
                        left -= take;
                        if (mine && (r.Date.Date - inv.Date.Date).TotalDays <= days) fast += take;
                    }
                }
            }
            return Common.RoundMe(fast);
        }

        // ============================== الحساب ==============================

        /// <summary>
        /// يحسب دفعة عمولة للمندوب بخطته عن فترة ويحفظها «محسوبة». لا تتداخل دفعتان لنفس المندوب؛
        /// إعادة الحساب لدفعة محسوبة تستبدل أسطرها (RecalculateRun).
        /// </summary>
        public long Calculate(long repId, DateTime from, DateTime to) => Tx(opp =>
        {
            DateTime f = from.Date, t = to.Date;
            if (t < f) throw new InvalidOperationException("نهاية الفترة قبل بدايتها.");
            if (t >= DateTime.Today.AddDays(1)) throw new InvalidOperationException("لا تُحسب العمولة لفترة لم تنتهِ أيامها بعد.");
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == repId) ?? throw new InvalidOperationException("ملف المندوب غير موجود.");
            if ((rep.Commission_Plan_Id ?? 0) <= 0) throw new InvalidOperationException("لا توجد خطة عمولة مسندة لهذا المندوب.");
            if (opp.db.SalesRep_Commission_Runs.AsNoTracking().Any(r => r.Rep_Id == repId && r.Period_From <= t && r.Period_To >= f))
                throw new InvalidOperationException("توجد دفعة عمولة لهذا المندوب فى فترة متداخلة.");
            var run = new SalesRep_Commission_Run
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(), Rep_Id = repId, Plan_Id = rep.Commission_Plan_Id.Value, Period_From = f, Period_To = t,
                Status = (int)SalesRepCommissionRunStatus.Calculated,
            };
            run.StampInsert(rep.Branch_Id ?? Common.Branch_Id);
            opp.db.SalesRep_Commission_Runs.Add(run);
            Fill(opp, run);
            return run.Id;
        });

        /// <summary>يعيد حساب دفعة لم تُعتمد بأحدث الأرقام والخطة الحالية للمندوب.</summary>
        public void RecalculateRun(long runId) => Tx(opp =>
        {
            var run = opp.db.SalesRep_Commission_Runs.FirstOrDefault(r => r.Id == runId) ?? throw new InvalidOperationException("الدفعة غير موجودة.");
            if (run.Status != (int)SalesRepCommissionRunStatus.Calculated) throw new InvalidOperationException("الدفعة معتمدة؛ لا يعاد حسابها.");
            var rep = opp.db.SalesRep_Reps.AsNoTracking().First(r => r.Id == run.Rep_Id);
            if ((rep.Commission_Plan_Id ?? 0) <= 0) throw new InvalidOperationException("لا توجد خطة عمولة مسندة لهذا المندوب.");
            run.Plan_Id = rep.Commission_Plan_Id.Value;
            run.StampEdit();
            opp.db.SalesRep_Commission_Run_Lines.RemoveRange(opp.db.SalesRep_Commission_Run_Lines.Where(l => l.Run_Id == run.Id));
            opp.db.SaveChanges();
            Fill(opp, run);
            return 0;
        });

        public void DeleteRun(long runId) => Tx(opp =>
        {
            var run = opp.db.SalesRep_Commission_Runs.FirstOrDefault(r => r.Id == runId) ?? throw new InvalidOperationException("الدفعة غير موجودة.");
            if (run.Status != (int)SalesRepCommissionRunStatus.Calculated) throw new InvalidOperationException("الدفعة معتمدة؛ لا تُحذف.");
            opp.db.SalesRep_Commission_Run_Lines.RemoveRange(opp.db.SalesRep_Commission_Run_Lines.Where(l => l.Run_Id == run.Id));
            opp.db.SalesRep_Commission_Runs.Remove(run);
            opp.db.SaveChanges();
            return 0;
        });

        private static void Fill(operations opp, SalesRep_Commission_Run run)
        {
            var plan = opp.db.SalesRep_Commission_Plans.AsNoTracking().FirstOrDefault(p => p.Id == run.Plan_Id) ?? throw new InvalidOperationException("خطة العمولة غير موجودة.");
            if (!plan.Active) throw new InvalidOperationException("خطة العمولة موقوفة.");
            var tiers = opp.db.SalesRep_Commission_Plan_Tiers.AsNoTracking().Where(x => x.Plan_Id == plan.Id).ToList();
            var m = Measure(opp, run.Rep_Id, run.Period_From, run.Period_To, plan.Deduct_Returns);
            var targets = opp.db.SalesRep_Targets.AsNoTracking()
                .Where(g => g.Rep_Id == run.Rep_Id && g.Period_From >= run.Period_From && g.Period_To <= run.Period_To).ToList();
            decimal? Sum(SalesRepTargetScope s) { var v = targets.Where(g => g.Scope == (int)s).Sum(g => g.Target_Value ?? 0); return v > 0 ? v : null; }
            decimal? totalTarget = Sum(SalesRepTargetScope.Total), collectionTarget = Sum(SalesRepTargetScope.Collection);
            var lines = new List<SalesRep_Commission_Run_Line>();

            void Add(SalesRepCommissionComponent c, long? scopeId, decimal baseAmount, decimal? target, bool needsTarget, string notes)
            {
                var mine = tiers.Where(x => x.Component == (int)c).ToList();
                if (mine.Count == 0) return;
                decimal? ach = target is decimal tv && tv > 0 ? Math.Round(baseAmount / tv * 100, 4) : null;
                if (needsTarget && ach == null) return;
                // بلا هدف: المكوّن يُحسب بشريحة «من 0» فقط (العمولة الأساسية).
                decimal level = ach ?? 0;
                var tier = mine.Where(x => level >= x.Achievement_From && (x.Achievement_To == null || level < x.Achievement_To.Value))
                    .OrderByDescending(x => x.Achievement_From).FirstOrDefault();
                decimal amount = tier == null || baseAmount <= 0 ? 0 : Common.RoundMe(baseAmount * (tier.Rate ?? 0) / 100 + (tier.Fixed_Amount ?? 0));
                lines.Add(new SalesRep_Commission_Run_Line
                {
                    Component = (int)c, Scope_Id = scopeId, Base_Amount = Common.RoundMe(baseAmount), Target_Value = target, Achievement_Rate = ach,
                    Rate = tier?.Rate, Amount = amount,
                    Notes = Trim((notes ?? "") + (tier == null ? " — لم يبلغ أى شريحة" : tier.Fixed_Amount > 0 ? $" — مبلغ ثابت {tier.Fixed_Amount:N2}" : ""), 500).Trim(' ', '—'),
                });
            }

            Add(SalesRepCommissionComponent.NetSales, null, m.Net_Sales, totalTarget, false,
                plan.Deduct_Returns ? $"مبيعات {m.Gross_Sales:N2} − مرتجعات {m.Returns:N2}" : $"مبيعات {m.Gross_Sales:N2} (بلا خصم المرتجعات)");
            Add(SalesRepCommissionComponent.Collection, null, m.Collection, collectionTarget, false, "تحصيل نقدى وشيكات");
            Add(SalesRepCommissionComponent.TotalTarget, null, m.Net_Sales, totalTarget, true, "مكافأة الهدف الإجمالى");
            foreach (var g in targets.Where(g => g.Scope is (int)SalesRepTargetScope.Item or (int)SalesRepTargetScope.ItemGroup).OrderBy(g => g.Scope))
            {
                long sid = g.Scope_Id ?? 0;
                bool item = g.Scope == (int)SalesRepTargetScope.Item;
                decimal actual = item ? (m.Item_Sales.TryGetValue(sid, out var a) ? a : 0) : (m.Group_Sales.TryGetValue(sid, out var b) ? b : 0);
                string name = item ? opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Item_Name, N'') AS Value FROM Item WHERE id = {sid}").AsEnumerable().FirstOrDefault()
                                   : opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Name, N'') AS Value FROM Items_Group WHERE Id = {sid}").AsEnumerable().FirstOrDefault();
                Add(SalesRepCommissionComponent.ItemTarget, sid, actual, g.Target_Value, true, (item ? "صنف: " : "مجموعة: ") + (name ?? ""));
            }

            if (plan.Collection_Speed_Days is int days && (plan.Collection_Speed_Bonus_Rate ?? 0) > 0)
            {
                decimal fast = FastCollected(opp, run.Rep_Id, run.Period_From, run.Period_To, days);
                lines.Add(new SalesRep_Commission_Run_Line
                {
                    Component = (int)SalesRepCommissionComponent.Collection, Base_Amount = fast, Rate = plan.Collection_Speed_Bonus_Rate,
                    Amount = Common.RoundMe(fast * plan.Collection_Speed_Bonus_Rate.Value / 100),
                    Notes = $"مكافأة سرعة التحصيل: ما سُدِّد خلال {days} يوماً من تاريخ الفاتورة",
                });
            }

            foreach (var l in lines)
            {
                l.Id = CallSnowflakeIdGenerate.GenerateNewId();
                l.Run_Id = run.Id;
                l.StampInsert(run.Branch_Id ?? Common.Branch_Id);
                opp.db.SalesRep_Commission_Run_Lines.Add(l);
            }
            run.Total_Amount = Common.RoundMe(lines.Sum(l => l.Amount));
            opp.db.SaveChanges();
        }

        // ============================== الاعتماد ==============================

        /// <summary>
        /// يعتمد الدفعة ويقيدها: مدين حساب مصروف الخطة / دائن حساب الموظف بفرع المندوب (Reference_Type 3002 + رقم الدفعة).
        /// المعتمِد غير من حسبها. دفعة بصفر تُعتمد بلا قيد.
        /// </summary>
        public long? ApproveRun(long runId) => Tx(opp =>
        {
            var run = opp.db.SalesRep_Commission_Runs.FirstOrDefault(r => r.Id == runId) ?? throw new InvalidOperationException("الدفعة غير موجودة.");
            if (run.Status != (int)SalesRepCommissionRunStatus.Calculated) throw new InvalidOperationException("الدفعة معتمدة من قبل.");
            if (run.User_Id == Common.User_Id || run.Edit_User_Id == Common.User_Id) throw new InvalidOperationException("لا يعتمد المستخدم دفعة حسبها بنفسه.");
            var plan = opp.db.SalesRep_Commission_Plans.AsNoTracking().First(p => p.Id == run.Plan_Id);
            var rep = opp.db.SalesRep_Reps.AsNoTracking().First(r => r.Id == run.Rep_Id);
            long branchId = rep.Branch_Id ?? Common.Branch_Id;
            if (run.Total_Amount > 0)
            {
                if ((plan.Expense_Account_Id ?? 0) <= 0) throw new InvalidOperationException("حدد حساب مصروف العمولة فى الخطة أولاً.");
                string name = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(e.name, N'') AS Value FROM Employee e WHERE e.Id = {rep.Employee_Id}").AsEnumerable().FirstOrDefault() ?? "";
                run.Account_Entry_List_ID = CommissionEntry(opp, run, rep, plan.Expense_Account_Id.Value, branchId,
                    $"عمولة المندوب {name} عن الفترة {run.Period_From:yyyy-MM-dd} إلى {run.Period_To:yyyy-MM-dd}");
            }
            run.Status = run.Total_Amount > 0 ? (int)SalesRepCommissionRunStatus.Posted : (int)SalesRepCommissionRunStatus.Approved;
            run.Approved_By = Common.User_Id;
            run.Approved_At = DateTime.Now;
            run.StampEdit();
            opp.db.SaveChanges();
            return run.Account_Entry_List_ID;
        });

        private record AccountInfo(long Id, int Type, string Name, string Code, long CurrencyId, long MainAccountId);

        private static AccountInfo Account(operations opp, long id)
        {
            var dt = opp.return_DataTable("SELECT Id, ISNULL(Account_Customer_Type_Value, 0) AS T, SubAccount_Name, SubAccount_NO, ISNULL(Currency_Id, 1) AS Cur, ISNULL(Main_Account_Id, 0) AS M FROM Submain_Account WHERE Id = " + id);
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            long cur = Convert.ToInt64(r["Cur"]);
            return new AccountInfo(Convert.ToInt64(r["Id"]), Convert.ToInt32(r["T"]), r["SubAccount_Name"]?.ToString()?.Trim() ?? "", r["SubAccount_NO"]?.ToString()?.Trim() ?? "",
                cur <= 0 ? 1 : cur, Convert.ToInt64(r["M"]));
        }

        private static long CommissionEntry(operations opp, SalesRep_Commission_Run run, SalesRep_Rep rep, long expenseAccountId, long branchId, string notes)
        {
            var debit = Account(opp, expenseAccountId) ?? throw new InvalidOperationException("حساب مصروف العمولة غير موجود فى الدليل.");
            long empSub = opp.db.Employees.AsNoTracking().Where(e => e.Id == rep.Employee_Id).Select(e => e.SubAccount_Id ?? 0).FirstOrDefault();
            var credit = (empSub > 0 ? Account(opp, empSub) : null)
                         ?? throw new InvalidOperationException("الموظف المرتبط بالمندوب ليس له حساب فى الدليل؛ أنشئ حسابه من شاشة الموظفين أولاً.");
            decimal value = run.Total_Amount;
            long entryCurrency = Common.Entry_Currency_Id <= 0 ? 1 : Common.Entry_Currency_Id;
            var curName = opp.return_DataTable("SELECT Currency_Name FROM Currency WHERE Value = " + entryCurrency);
            string currencyName = curName.Rows.Count > 0 ? curName.Rows[0][0]?.ToString() ?? "" : "";
            var aa = new AccountEntry_CLS { Reference_Type = CommissionReferenceType, Reference_Type_Table_Id = run.Id };
            var dt = new System.Data.DataTable();
            aa.Get_Empty_dt_Details(ref opp, ref dt);
            int excp = 0;
            aa.AddRow_WhenBuild(ref excp, ref dt, 0, debit.Code, debit.Type, debit.MainAccountId, "", debit.Id, debit.Name, 1, value, notes, 1, debit.CurrencyId, entryCurrency, 0, "", branchId, 1);
            if (excp == 0) aa.AddRow_WhenBuild(ref excp, ref dt, 0, credit.Code, credit.Type, credit.MainAccountId, "", credit.Id, credit.Name, 2, value, notes, 1, credit.CurrencyId, entryCurrency, 0, "", branchId, 1);
            if (excp != 0) throw new InvalidOperationException("تعذر تجهيز أسطر قيد العمولة.");
            for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["Line_No"] = i + 1;

            const long journalCode = 1;
            DateTime date = DateTime.Today;
            long entryNo = AccountEntry_CLS.NewEntry_No(ref opp, date, journalCode, 0);
            long entryId = 0;
            string msg = "";
            aa.Make_AccountEntry(ref excp, ref msg, ref entryId, ref opp, Common.User_Id, Common.User_Name, Common.Branch_Id, entryNo, date, journalCode, entryCurrency, notes,
                Common.Transform_Type, dt, (double)value, branchId, 1, run.Id.ToString(), entryCurrency, currencyName, 1, 0);
            if (excp != 0 || entryId <= 0) throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "تعذر إنشاء قيد العمولة." : msg);
            return entryId;
        }

        // ============================== العرض ==============================

        public List<RunRow> GetRuns(long? repId, int take = 200) => Read(opp =>
        {
            long rep = repId ?? 0;
            return opp.db.Database.SqlQuery<RunRow>($@"
SELECT TOP ({take}) r.Id, r.Rep_Id, ISNULL(e.name, N'') AS Rep_Name, r.Plan_Id, ISNULL(p.Name, N'') AS Plan_Name, r.Period_From, r.Period_To, r.Status, r.Total_Amount,
       r.Approved_At, r.Account_Entry_List_ID, CAST(a.Entry_No AS bigint) AS Entry_No, r.Insert_ServerDateTime
FROM SalesRep_Commission_Runs r
LEFT JOIN SalesRep_Reps sr ON sr.Id = r.Rep_Id LEFT JOIN Employee e ON e.Id = sr.Employee_Id
LEFT JOIN SalesRep_Commission_Plans p ON p.Id = r.Plan_Id LEFT JOIN Account_Entry_List a ON a.ID = r.Account_Entry_List_ID
WHERE {rep} = 0 OR r.Rep_Id = {rep}
ORDER BY CASE WHEN r.Status = 0 THEN 0 ELSE 1 END, r.Period_From DESC, e.name").ToList();
        });

        public List<RunLineRow> GetRunLines(long runId) => Read(opp => opp.db.SalesRep_Commission_Run_Lines.AsNoTracking().Where(l => l.Run_Id == runId)
            .OrderBy(l => l.Component).ThenBy(l => l.Id)
            .Select(l => new RunLineRow
            {
                Id = l.Id, Component = l.Component, Scope_Id = l.Scope_Id, Base_Amount = l.Base_Amount, Target_Value = l.Target_Value,
                Achievement_Rate = l.Achievement_Rate, Rate = l.Rate, Amount = l.Amount, Notes = l.Notes ?? "",
            }).ToList());

        // ============================== لوحة الأداء ==============================

        private class VisitStat { public bool Planned { get; set; } public int? Result { get; set; } public int Status { get; set; } public string Reason { get; set; } }

        /// <summary>الزيارات المخططة والمنفّذة والفائتة وغير المخططة، وأسباب عدم البيع، وتقدم الأهداف، وأعمار ديون عملاء المندوب.</summary>
        public Performance GetPerformance(long repId, DateTime from, DateTime to) => Read(opp =>
        {
            DateTime f = from.Date, t = to.Date;
            var p = new Performance { Measures = Measure(opp, repId, f, t, true) };
            string shifts = ShiftsOf(repId, f, t);
            p.Shifts = opp.db.Database.SqlQueryRaw<int>($"SELECT COUNT(*) AS Value FROM ({shifts}) x").AsEnumerable().First();
            var visits = opp.db.Database.SqlQueryRaw<VisitStat>($@"
SELECT v.Planned, v.Result, s.Status, ISNULL(n.Name, N'') AS Reason
FROM SalesRep_Visits v JOIN SalesRep_Shifts s ON s.Id = v.Shift_Id LEFT JOIN Lookup_SalesRep_NoSaleReasons n ON n.Id = v.No_Sale_Reason_Id
WHERE v.Shift_Id IN ({shifts})").ToList();
            p.Planned_Visits = visits.Count(v => v.Planned);
            p.Planned_Done = visits.Count(v => v.Planned && v.Result != null);
            p.Missed = visits.Count(v => v.Planned && v.Result == null && v.Status == (int)SalesRepShiftStatus.Closed);
            p.Unplanned = visits.Count(v => !v.Planned);
            p.No_Sale = visits.Count(v => v.Result == (int)SalesRepVisitResult.NoSale);
            p.Productive = visits.Count(v => v.Result != null && v.Result != (int)SalesRepVisitResult.NoSale);
            p.No_Sale_Reasons = visits.Where(v => v.Result == (int)SalesRepVisitResult.NoSale).GroupBy(v => v.Reason)
                .Select(g => new ReasonCount { Name = g.Key, Count = g.Count() }).OrderByDescending(r => r.Count).ToList();

            var targets = opp.db.SalesRep_Targets.AsNoTracking().Where(g => g.Rep_Id == repId && g.Period_To >= f && g.Period_From <= t).ToList();
            foreach (var g in targets.OrderBy(g => g.Scope).ThenBy(g => g.Period_From))
            {
                // كل هدف يقاس على فترته هو (لا فترة اللوحة) حتى لا يقارن هدف شهر بمبيعات يوم.
                var gm = Measure(opp, repId, g.Period_From, g.Period_To, true);
                long sid = g.Scope_Id ?? 0;
                decimal target = g.Scope == (int)SalesRepTargetScope.NewCustomers ? g.Target_Qty ?? 0 : g.Target_Value ?? 0;
                decimal actual = (SalesRepTargetScope)g.Scope switch
                {
                    SalesRepTargetScope.Total => gm.Net_Sales,
                    SalesRepTargetScope.Collection => gm.Collection,
                    SalesRepTargetScope.NewCustomers => gm.New_Customers,
                    SalesRepTargetScope.Item => gm.Item_Sales.TryGetValue(sid, out var a) ? Common.RoundMe(a) : 0,
                    SalesRepTargetScope.ItemGroup => gm.Group_Sales.TryGetValue(sid, out var b) ? Common.RoundMe(b) : 0,
                    _ => 0,
                };
                string name = g.Scope == (int)SalesRepTargetScope.Item
                    ? opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Item_Name, N'') AS Value FROM Item WHERE id = {sid}").AsEnumerable().FirstOrDefault()
                    : g.Scope == (int)SalesRepTargetScope.ItemGroup
                        ? opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Name, N'') AS Value FROM Items_Group WHERE Id = {sid}").AsEnumerable().FirstOrDefault() : "";
                p.Targets.Add(new TargetProgress
                {
                    Target_Id = g.Id, Scope = g.Scope, Scope_Id = g.Scope_Id, Scope_Name = name ?? "", Period_From = g.Period_From, Period_To = g.Period_To,
                    Target = target, Actual = actual, Achievement = target > 0 ? Math.Round(actual / target * 100, 1) : null,
                });
            }
            p.Aging = Aging(opp, repId, t);
            return p;
        });

        private class BalanceRow { public long Id { get; set; } public string Name { get; set; } = ""; public decimal Balance { get; set; } }

        private class AgingInvoice { public decimal Net { get; set; } public DateTime Due { get; set; } }

        /// <summary>
        /// أعمار ديون العملاء الذين باع لهم المندوب أو على خطوط سيره: الرصيد المدين يُغطّى بأحدث الفواتير الآجلة أولاً (FIFO)،
        /// والباقى فى كل فاتورة يدخل شريحة عمره من تاريخ استحقاقها (أو تاريخها) حتى «asOf». الرصيد بلا فواتير يذهب لأقدم شريحة.
        /// </summary>
        private static List<AgingRow> Aging(operations opp, long repId, DateTime asOf)
        {
            // الرصيد من نفس مصادر كشف العميل (CustomerCLS.MakeCustomer_MokhtalatDataByGroupNew): حساب العميل + حساب المورد المرتبط، عدا النوع 25،
            // لكن باستعلام واحد على اتصال الشاشة بدلاً من جدول Mokhtalat المؤقت الذى يُفرَّغ ويُملأ من اتصال آخر لكل عميل.
            var customers = opp.db.Database.SqlQuery<BalanceRow>($@"
SELECT c.Id, ISNULL(c.Name, N'') AS Name,
       CAST(ISNULL((SELECT SUM(ISNULL(a.Export, 0) - ISNULL(a.Import, 0)) FROM Customers_Accounts a WHERE a.Customer_Id = c.Id AND a.Reference_Type <> 25), 0)
          + ISNULL((SELECT SUM(ISNULL(sa.Export, 0) - ISNULL(sa.Import, 0)) FROM Suppliers_Accounts sa JOIN Supplier sp ON sp.Id = sa.Supplier_Id
                    WHERE sp.Ref_Cust_Supplier_Id = c.Id AND sa.Reference_Type <> 25), 0) AS decimal(18,4)) AS Balance
FROM Customer c
WHERE c.Id IN (SELECT i.Customer_Id FROM SalesRep_Document_Links l JOIN SalesRep_Shifts s ON s.Id = l.Shift_Id JOIN Invoices_List i ON i.Id = l.Doc_Id
               WHERE s.Rep_Id = {repId} AND l.Doc_Type = {(int)SalesRepDocType.SaleInvoice})
   OR c.Id IN (SELECT rc.Customer_Id FROM SalesRep_Route_Customers rc JOIN SalesRep_Routes r ON r.Id = rc.Route_Id WHERE r.Rep_Id = {repId} AND r.Active = 1)").ToList();
            var rows = new List<AgingRow>();
            foreach (var c in customers)
            {
                decimal bal = Common.RoundMe(c.Balance);
                if (bal <= 0) continue;
                var row = new AgingRow { Customer_Id = c.Id, Customer_Name = c.Name, Balance = bal };
                var invoices = opp.db.Database.SqlQuery<AgingInvoice>($@"
SELECT CAST(ISNULL(Net_Total, 0) AS decimal(18,4)) AS Net, CAST(ISNULL(InvoiceDueDate, Invoice_Date) AS datetime) AS Due
FROM Invoices_List WHERE Customer_Id = {c.Id} AND ISNULL(Is_Invoice_Agile, 0) = 1 ORDER BY Invoice_Date DESC, Id DESC").ToList();
                decimal left = bal;
                foreach (var inv in invoices)
                {
                    if (left <= 0) break;
                    decimal part = Math.Min(left, inv.Net);
                    left -= part;
                    Bucket(row, (asOf - inv.Due.Date).TotalDays, part);
                }
                if (left > 0) row.B90 += left;
                rows.Add(row);
            }
            return rows.OrderByDescending(r => r.B90).ThenByDescending(r => r.Balance).ToList();
        }

        private static void Bucket(AgingRow row, double days, decimal value)
        {
            if (days <= 30) row.B0_30 += value;
            else if (days <= 60) row.B31_60 += value;
            else if (days <= 90) row.B61_90 += value;
            else row.B90 += value;
        }

        // ============================== مساعدات ==============================

        private static long RepBranch(operations opp, long repId) =>
            opp.db.SalesRep_Reps.AsNoTracking().Where(r => r.Id == repId).Select(r => r.Branch_Id).FirstOrDefault() ?? Common.Branch_Id;

        private static string Trim(string s, int max) => s == null ? null : s.Length <= max ? s : s.Substring(0, max);

        private T Read<T>(Func<operations, T> f) => f(_external ?? new operations());

        private T Tx<T>(Func<operations, T> f)
        {
            if (_external != null) return f(_external);
            var opp = new operations(2);
            try
            {
                var r = f(opp);
                opp.Commit();
                return r;
            }
            catch
            {
                try { opp.Rollback(); } catch { }
                throw;
            }
        }
    }
}
