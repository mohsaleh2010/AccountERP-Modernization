using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 3: إدارة سياسات الائتمان والخصم التى تفحصها SalesRepSaleService (البيع الفورى وأخذ الطلبية والتسليم).
    /// - الائتمان: صف لعميل أو لمجموعة عملاء (الأخص يغلب، ق7): الحد، وأقصى أيام تأخر، ومنع البيع الآجل عند التأخر.
    /// - الخصم: صف لكل (مندوب / مجموعة عملاء / صنف / مجموعة أصناف): «حد بلا موافقة» وحد أقصى مطلق اختيارى لا يُتجاوز حتى بموافقة.
    ///   التخزين كما يقرؤه الفحص: بلا حد مطلق → Max_Discount_Rate = حد الموافقة؛ معه → Requires_Approval_Above = حد الموافقة و Max = المطلق.
    /// السياسات إعداد (لا مستندات)، فالحذف مسموح ويؤثر على الفحوص القادمة فقط.
    /// </summary>
    public class SalesRepPolicyService
    {
        private readonly operations _external;
        public SalesRepPolicyService(operations external = null) { _external = external; }

        public enum TargetKind { Customer = 0, CustomerGroup = 1, Rep = 2, Item = 3, ItemGroup = 4 }

        public class Target { public long Id { get; set; } public string Name { get; set; } = ""; }

        public class CreditRow
        {
            public long Id { get; set; }
            public long? Customer_Id { get; set; }
            public long? Group_Id { get; set; }
            public string Target_Name { get; set; } = "";
            public decimal Credit_Limit { get; set; }
            public int? Max_Overdue_Days { get; set; }
            public bool Block_On_Overdue { get; set; } = true;
            public bool IsGroup => Group_Id != null;
        }

        public class DiscountRow
        {
            public long Id { get; set; }
            public int Scope { get; set; }
            public long Scope_Id { get; set; }
            public string Target_Name { get; set; } = "";
            /// <summary>أقصى خصم بلا موافقة المشرف.</summary>
            public decimal No_Approval_Rate { get; set; }
            /// <summary>حد مطلق لا يُعتمد فوقه (null = لا حد؛ ما فوق حد الموافقة يحتاج موافقة فقط).</summary>
            public decimal? Hard_Max_Rate { get; set; }
        }

        // ============================== البحث عن الهدف ==============================

        public List<Target> SearchTargets(TargetKind kind, string search, int take = 50) => Read(opp =>
        {
            string like = string.IsNullOrWhiteSpace(search) ? "" : search.Trim();
            return kind switch
            {
                TargetKind.Customer => opp.db.Database.SqlQuery<Target>($@"SELECT TOP ({take}) CAST(Id AS bigint) AS Id, ISNULL(Name, N'') AS Name FROM Customer
WHERE {like} = N'' OR Name LIKE N'%' + {like} + N'%' ORDER BY Name").ToList(),
                TargetKind.CustomerGroup => opp.db.Database.SqlQuery<Target>($@"SELECT TOP ({take}) CAST(ID AS bigint) AS Id, ISNULL(GroupName, N'') AS Name FROM Customer_Supplier_Group
WHERE GroupType = {Common.Customer_Account_Type} AND ({like} = N'' OR GroupName LIKE N'%' + {like} + N'%') ORDER BY GroupName").ToList(),
                TargetKind.Rep => opp.db.Database.SqlQuery<Target>($@"SELECT TOP ({take}) r.Id, ISNULL(e.name, N'') AS Name FROM SalesRep_Reps r LEFT JOIN Employee e ON e.Id = r.Employee_Id
WHERE {like} = N'' OR e.name LIKE N'%' + {like} + N'%' ORDER BY e.name").ToList(),
                TargetKind.Item => opp.db.Database.SqlQuery<Target>($@"SELECT TOP ({take}) CAST(id AS bigint) AS Id, ISNULL(Item_Name, N'') AS Name FROM Item
WHERE ISNULL(Active_flage, 1) = 1 AND ({like} = N'' OR Item_Name LIKE N'%' + {like} + N'%') ORDER BY Item_Name").ToList(),
                TargetKind.ItemGroup => opp.db.Database.SqlQuery<Target>($@"SELECT TOP ({take}) CAST(Id AS bigint) AS Id, ISNULL(Name, N'') AS Name FROM Items_Group
WHERE {like} = N'' OR Name LIKE N'%' + {like} + N'%' ORDER BY Name").ToList(),
                _ => new List<Target>(),
            };
        });

        // ============================== الائتمان ==============================

        public List<CreditRow> GetCreditPolicies() => Read(opp => opp.db.Database.SqlQuery<CreditRow>($@"
SELECT p.Id, p.Customer_Id, p.Group_Id,
       ISNULL(CASE WHEN p.Customer_Id IS NOT NULL THEN c.Name ELSE g.GroupName END, N'') AS Target_Name,
       CAST(p.Credit_Limit AS decimal(18,2)) AS Credit_Limit, p.Max_Overdue_Days, p.Block_On_Overdue
FROM SalesRep_Credit_Policy p LEFT JOIN Customer c ON c.Id = p.Customer_Id LEFT JOIN Customer_Supplier_Group g ON g.ID = p.Group_Id
ORDER BY CASE WHEN p.Group_Id IS NOT NULL THEN 0 ELSE 1 END, Target_Name").ToList());

        /// <summary>يضيف أو يعدّل سياسة ائتمان لعميل أو لمجموعة (واحد فقط)، صف واحد لكل هدف. يرجع المعرّف.</summary>
        public long SaveCredit(CreditRow input) => Tx(opp =>
        {
            if ((input.Customer_Id is > 0) == (input.Group_Id is > 0)) throw new InvalidOperationException("اختر عميلاً أو مجموعة عملاء (واحداً فقط).");
            if (input.Credit_Limit < 0) throw new InvalidOperationException("حد الائتمان لا يكون سالباً.");
            if (input.Max_Overdue_Days is < 0) throw new InvalidOperationException("أيام التأخر لا تكون سالبة.");
            long? cust = input.Customer_Id is > 0 ? input.Customer_Id : null, grp = input.Group_Id is > 0 ? input.Group_Id : null;
            if (opp.db.SalesRep_Credit_Policies.Any(p => p.Id != input.Id && ((cust != null && p.Customer_Id == cust) || (grp != null && p.Group_Id == grp))))
                throw new InvalidOperationException(cust != null ? "لهذا العميل سياسة ائتمان بالفعل؛ عدّلها." : "لهذه المجموعة سياسة ائتمان بالفعل؛ عدّلها.");
            SalesRep_Credit_Policy p;
            if (input.Id == 0)
            {
                p = new SalesRep_Credit_Policy { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                p.StampInsert(Common.Branch_Id);
                opp.db.SalesRep_Credit_Policies.Add(p);
            }
            else
            {
                p = opp.db.SalesRep_Credit_Policies.FirstOrDefault(x => x.Id == input.Id) ?? throw new InvalidOperationException("السياسة غير موجودة.");
                p.StampEdit();
            }
            p.Customer_Id = cust; p.Group_Id = grp;
            p.Credit_Limit = Math.Round(input.Credit_Limit, 2);
            p.Max_Overdue_Days = input.Max_Overdue_Days;
            p.Block_On_Overdue = input.Block_On_Overdue;
            opp.db.SaveChanges();
            return p.Id;
        });

        public void DeleteCredit(long id) => Tx(opp =>
        {
            var p = opp.db.SalesRep_Credit_Policies.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("السياسة غير موجودة.");
            opp.db.SalesRep_Credit_Policies.Remove(p);
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== الخصم ==============================

        public List<DiscountRow> GetDiscountPolicies() => Read(opp => opp.db.Database.SqlQuery<DiscountRow>($@"
SELECT p.Id, p.Scope, p.Scope_Id,
       ISNULL(CASE p.Scope WHEN {(int)SalesRepDiscountScope.Rep} THEN e.name WHEN {(int)SalesRepDiscountScope.CustomerGroup} THEN g.GroupName
                           WHEN {(int)SalesRepDiscountScope.Item} THEN i.Item_Name ELSE ig.Name END, N'') AS Target_Name,
       CAST(ISNULL(p.Requires_Approval_Above, p.Max_Discount_Rate) AS decimal(9,2)) AS No_Approval_Rate,
       CAST(CASE WHEN p.Requires_Approval_Above IS NULL THEN NULL ELSE p.Max_Discount_Rate END AS decimal(9,2)) AS Hard_Max_Rate
FROM SalesRep_Discount_Policy p
LEFT JOIN SalesRep_Reps r ON p.Scope = {(int)SalesRepDiscountScope.Rep} AND r.Id = p.Scope_Id LEFT JOIN Employee e ON e.Id = r.Employee_Id
LEFT JOIN Customer_Supplier_Group g ON p.Scope = {(int)SalesRepDiscountScope.CustomerGroup} AND g.ID = p.Scope_Id
LEFT JOIN Item i ON p.Scope = {(int)SalesRepDiscountScope.Item} AND i.id = p.Scope_Id
LEFT JOIN Items_Group ig ON p.Scope = {(int)SalesRepDiscountScope.ItemGroup} AND ig.Id = p.Scope_Id
ORDER BY p.Scope, Target_Name").ToList());

        public long SaveDiscount(DiscountRow input) => Tx(opp =>
        {
            if (!Enum.IsDefined(typeof(SalesRepDiscountScope), input.Scope)) throw new InvalidOperationException("نطاق السياسة غير صحيح.");
            if (input.Scope_Id <= 0) throw new InvalidOperationException("اختر المندوب أو المجموعة أو الصنف.");
            if (input.No_Approval_Rate < 0 || input.No_Approval_Rate > 100) throw new InvalidOperationException("نسبة الخصم بين 0 و 100.");
            if (input.Hard_Max_Rate is decimal h && (h > 100 || h < input.No_Approval_Rate))
                throw new InvalidOperationException("الحد الأقصى المطلق يكون بين حد الموافقة و 100.");
            if (opp.db.SalesRep_Discount_Policies.Any(p => p.Id != input.Id && p.Scope == input.Scope && p.Scope_Id == input.Scope_Id))
                throw new InvalidOperationException("لهذا الهدف سياسة خصم بالفعل؛ عدّلها.");
            SalesRep_Discount_Policy p;
            if (input.Id == 0)
            {
                p = new SalesRep_Discount_Policy { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                p.StampInsert(Common.Branch_Id);
                opp.db.SalesRep_Discount_Policies.Add(p);
            }
            else
            {
                p = opp.db.SalesRep_Discount_Policies.FirstOrDefault(x => x.Id == input.Id) ?? throw new InvalidOperationException("السياسة غير موجودة.");
                p.StampEdit();
            }
            p.Scope = input.Scope; p.Scope_Id = input.Scope_Id;
            decimal free = Math.Round(input.No_Approval_Rate, 2);
            if (input.Hard_Max_Rate is decimal hard) { p.Requires_Approval_Above = free; p.Max_Discount_Rate = Math.Round(hard, 2); }
            else { p.Requires_Approval_Above = null; p.Max_Discount_Rate = free; }
            opp.db.SaveChanges();
            return p.Id;
        });

        public void DeleteDiscount(long id) => Tx(opp =>
        {
            var p = opp.db.SalesRep_Discount_Policies.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("السياسة غير موجودة.");
            opp.db.SalesRep_Discount_Policies.Remove(p);
            opp.db.SaveChanges();
            return 0;
        });

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
