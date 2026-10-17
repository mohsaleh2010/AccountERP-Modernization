using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// بقايا المرحلة 2 (قرار المستخدم 2026-10-17): الدفع غير النقدى للمندوب.
    /// لكل طريقة دفع غير نقدية (Lookup_CashPaymentMethod) بنك (Banks.Is_Bank = 1) عام أو لفرع؛ المندوب يختار الطريقة فقط،
    /// فيُنشأ الإذن على ذلك البنك وسطره يحمل الطريقة، ويُربط بالوردية. لا يدخل عهدة المندوب ولا يُعدّ عند الإقفال.
    /// </summary>
    public class SalesRepPaymentService
    {
        private readonly operations _external;
        public SalesRepPaymentService(operations external = null) { _external = external; }

        public class Option { public long Id { get; set; } public string Name { get; set; } = ""; }

        public class MappingRow
        {
            public long Id { get; set; }
            public long Payment_Method_Id { get; set; }
            public string Method_Name { get; set; } = "";
            public long? Scope_Branch_Id { get; set; }
            public string Branch_Name { get; set; } = "";
            public long Bank_Id { get; set; }
            public string Bank_Name { get; set; } = "";
            public bool Active { get; set; } = true;
        }

        /// <summary>أين يدخل التحصيل: الصندوق/البنك، والطريقة المسجَّلة على السطر (0 للنقدى كالأصل).</summary>
        public record Receipt(long BoxId, long MethodId, string MethodName, bool IsCash);

        // ============================== الإعداد ==============================

        /// <summary>طرق الدفع غير النقدية من جدول النظام.</summary>
        public List<Option> GetNonCashMethods() => Read(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT CAST(Id AS bigint) AS Id, ISNULL(Name, N'') AS Name FROM Lookup_CashPaymentMethod
WHERE Id <> {SalesRepShiftService.CashPaymentMethodId} ORDER BY Id").ToList());

        /// <summary>البنوك (لا الصناديق) النشطة.</summary>
        public List<Option> GetBanks() => Read(opp => opp.db.Database.SqlQueryRaw<Option>(
            "SELECT CAST(Id AS bigint) AS Id, ISNULL(Bank_Name, N'') AS Name FROM Banks WHERE Is_Bank = 1 AND ISNULL(Active, 1) = 1 ORDER BY Bank_Name").ToList());

        public List<Option> GetBranches() => Read(opp => opp.db.Database.SqlQueryRaw<Option>(
            "SELECT CAST(Id AS bigint) AS Id, ISNULL(Branch_Name, N'') AS Name FROM Account_Branches ORDER BY Id").ToList());

        public List<MappingRow> GetMappings() => Read(opp => opp.db.Database.SqlQueryRaw<MappingRow>(@"
SELECT m.Id, m.Payment_Method_Id, ISNULL(p.Name, N'') AS Method_Name, m.Scope_Branch_Id, ISNULL(br.Branch_Name, N'') AS Branch_Name,
       m.Bank_Id, ISNULL(b.Bank_Name, N'') AS Bank_Name, m.Active
FROM SalesRep_Payment_Method_Banks m
LEFT JOIN Lookup_CashPaymentMethod p ON p.Id = m.Payment_Method_Id
LEFT JOIN Account_Branches br ON br.Id = m.Scope_Branch_Id
LEFT JOIN Banks b ON b.Id = m.Bank_Id
ORDER BY m.Payment_Method_Id, CASE WHEN m.Scope_Branch_Id IS NULL THEN 0 ELSE 1 END, m.Scope_Branch_Id").ToList());

        /// <summary>يحفظ ربط طريقة ببنك؛ لا يتكرر نفس (الطريقة، الفرع). يرجع رقم الصف.</summary>
        public long SaveMapping(MappingRow input) => Tx(opp =>
        {
            if (input.Payment_Method_Id == SalesRepShiftService.CashPaymentMethodId) throw new InvalidOperationException("النقدى يدخل صندوق عهدة المندوب؛ اختر طريقة غير نقدية.");
            if (!opp.db.Lookup_CashPaymentMethod.AsNoTracking().Any(p => p.Id == input.Payment_Method_Id)) throw new InvalidOperationException("اختر طريقة الدفع.");
            if (input.Bank_Id <= 0 || opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Banks WHERE Is_Bank = 1 AND Id = {input.Bank_Id}").AsEnumerable().First() == 0)
                throw new InvalidOperationException("اختر بنكاً (لا صندوقاً).");
            long? scope = input.Scope_Branch_Id is > 0 ? input.Scope_Branch_Id : null;
            if (opp.db.SalesRep_Payment_Method_Banks.Any(m => m.Id != input.Id && m.Payment_Method_Id == input.Payment_Method_Id && m.Scope_Branch_Id == scope))
                throw new InvalidOperationException("لهذه الطريقة بنك مسجَّل لنفس الفرع؛ عدّله بدل إضافة آخر.");

            var row = input.Id > 0 ? opp.db.SalesRep_Payment_Method_Banks.FirstOrDefault(m => m.Id == input.Id) ?? throw new InvalidOperationException("الإعداد غير موجود.") : null;
            if (row == null)
            {
                row = new SalesRep_Payment_Method_Bank { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                row.StampInsert(Common.Branch_Id);
                opp.db.SalesRep_Payment_Method_Banks.Add(row);
            }
            else row.StampEdit();
            row.Payment_Method_Id = input.Payment_Method_Id;
            row.Scope_Branch_Id = scope;
            row.Bank_Id = input.Bank_Id;
            row.Active = input.Active;
            opp.db.SaveChanges();
            return row.Id;
        });

        public void DeleteMapping(long id) => Tx(opp =>
        {
            var row = opp.db.SalesRep_Payment_Method_Banks.FirstOrDefault(m => m.Id == id) ?? throw new InvalidOperationException("الإعداد غير موجود.");
            opp.db.SalesRep_Payment_Method_Banks.Remove(row);
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== الاستعمال ==============================

        /// <summary>طرق الدفع المتاحة للمندوب: النقدى دائماً، وكل طريقة غير نقدية لها بنك نشط لفرعه أو عام.</summary>
        public List<Option> GetMethodsForRep(long repId) => Read(opp =>
        {
            var rep = opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == repId) ?? throw new InvalidOperationException("المندوب غير موجود.");
            long branch = rep.Branch_Id ?? Common.Branch_Id;
            var methods = opp.db.Lookup_CashPaymentMethod.AsNoTracking().ToList();
            var mapped = opp.db.SalesRep_Payment_Method_Banks.AsNoTracking().Where(m => m.Active && (m.Scope_Branch_Id == null || m.Scope_Branch_Id == branch))
                .Select(m => m.Payment_Method_Id).Distinct().ToList();
            return methods.Where(m => m.Id == SalesRepShiftService.CashPaymentMethodId || mapped.Contains(m.Id))
                .OrderBy(m => m.Id == SalesRepShiftService.CashPaymentMethodId ? 0 : 1).ThenBy(m => m.Id)
                .Select(m => new Option { Id = m.Id, Name = m.Name ?? "" }).ToList();
        });

        /// <summary>
        /// يحدد أين يدخل تحصيل المندوب: النقدى (أو 0) = صندوق العهدة بلا وسم كالأصل؛ غير النقدى = بنك الطريقة لفرع المندوب ثم العام،
        /// والسطر يحمل الطريقة. طريقة بلا بنك = خطأ يوضح الإعداد المطلوب.
        /// </summary>
        public static Receipt Resolve(operations opp, SalesRepSaleService.SaleContext ctx, long methodId)
        {
            if (methodId <= 0 || methodId == SalesRepShiftService.CashPaymentMethodId)
                return new Receipt(ctx.Cash_Box_Id, 0, "", true);
            var method = opp.db.Lookup_CashPaymentMethod.AsNoTracking().FirstOrDefault(p => p.Id == methodId) ?? throw new InvalidOperationException("طريقة الدفع غير موجودة.");
            var map = opp.db.SalesRep_Payment_Method_Banks.AsNoTracking()
                .Where(m => m.Active && m.Payment_Method_Id == methodId && (m.Scope_Branch_Id == null || m.Scope_Branch_Id == ctx.Branch_Id))
                .OrderBy(m => m.Scope_Branch_Id == null ? 1 : 0).FirstOrDefault()
                ?? throw new InvalidOperationException($"لم يُحدَّد بنك لطريقة الدفع «{method.Name}»؛ حدّده من شاشة «بنوك طرق دفع المندوبين».");
            return new Receipt(map.Bank_Id, methodId, method.Name ?? "", false);
        }

        /// <summary>عملة الصندوق/البنك لمعامل الإذن.</summary>
        public static long CurrencyOf(operations opp, long boxId) =>
            opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(currency_Id, 1) AS bigint) AS Value FROM Banks WHERE Id = {boxId}").AsEnumerable().FirstOrDefault();

        /// <summary>يسم أسطر أذونات أنشأها محرك الفاتورة فى نفس المعاملة بطريقة الدفع (أسطر جديدة فقط، لا يمس حركة قديمة).</summary>
        public static void TagVouchers(operations opp, IEnumerable<long> orderMoneyIds, Receipt receipt)
        {
            if (receipt.IsCash) return;
            foreach (long id in orderMoneyIds)
                opp.db.Database.ExecuteSqlInterpolated($@"
UPDATE Money_Transaction_Box SET Lookup_CashPaymentMethodId = {receipt.MethodId}, Lookup_CashPaymentMethodName = {receipt.MethodName}
WHERE Order_Mony_Id = {id} AND ISNULL(Lookup_CashPaymentMethodId, 0) = 0");
        }

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
