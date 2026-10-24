using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// أرصدة الأصناف لدى الموظفين (خطة المخازن 03، بديل balance_Employee / القائمة 870) لصفحة /store/balances/employees.
    /// قراءة فقط من Unit_itemEmployeeBalance (رصيد عهدة الصنف لكل موظف؛ تكتبه أذون الصرف القديمة للموظف). المحرك الجديد لا يكتبه،
    /// فالشاشة تعرض ما سجّله مسار الأذون القديم. الكمية = الكبرى × المعامل + الصغرى بالوحدة الصغرى.
    /// </summary>
    public class EmployeeItemBalancesService
    {
        private readonly Func<operations> _newOpp;

        public EmployeeItemBalancesService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class Option { public long Id { get; set; } public string Name { get; set; } }

        public class Filter
        {
            public long EmployeeId { get; set; }
            public long StoreId { get; set; }
            public long CategoryId { get; set; }
            public string Text { get; set; }
            public bool IncludeZero { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
        }

        public class Row
        {
            public long Id { get; set; }
            public long Employee_Id { get; set; }
            public string Employee_Name { get; set; }
            public long Item_Id { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string Category_Name { get; set; }
            public string Store_Name { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Qty_Small { get; set; }
            public decimal? Price_Large { get; set; }
            public decimal? Value { get; set; }
            public int Total_Rows { get; set; }
            public int Employees { get; set; }
            public decimal Total_Value { get; set; }
        }

        /// <summary>الموظفون الذين لهم سطر عهدة (لا كل الموظفين).</summary>
        public List<Option> GetEmployees() => Run(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT CAST(e.Id AS bigint) AS Id, ISNULL(e.name, N'') AS Name FROM Employee e
WHERE EXISTS (SELECT 1 FROM Unit_itemEmployeeBalance b WHERE b.EmployeeId = e.Id)
ORDER BY e.name").ToList());

        /// <summary>الأسطر مرتبة بالموظف ثم الصنف؛ عدد الموظفين وإجمالى القيمة على كل النتيجة لا الصفحة فقط.</summary>
        public List<Row> GetBalances(Filter f)
        {
            string text = (f.Text ?? "").Trim();
            string like = "%" + text + "%";
            int includeZero = f.IncludeZero ? 1 : 0;
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            return Run(opp => opp.db.Database.SqlQuery<Row>($@"
WITH l AS (
    SELECT b.id AS Id, CAST(b.EmployeeId AS bigint) AS Employee_Id, ISNULL(e.name, N'') AS Employee_Name,
           CAST(i.id AS bigint) AS Item_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(i.code, N'') AS Code,
           ISNULL(c.Category_Name, N'') AS Category_Name, ISNULL(s.Store_Name, N'') AS Store_Name,
           ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit, f.F AS Factor,
           CAST(ISNULL(b.Full_qua, 0) * f.F + ISNULL(b.D_Qua, 0) AS decimal(18,4)) AS Qty_Small,
           CAST(COALESCE(NULLIF(b.P_price, 0), NULLIF(i.P_Price, 0)) AS decimal(18,6)) AS Price_Large
    FROM Unit_itemEmployeeBalance b
    INNER JOIN Item i ON i.id = b.Item_id
    LEFT JOIN Employee e ON e.Id = b.EmployeeId
    LEFT JOIN Store s ON s.Id = i.S_id
    LEFT JOIN Item_Category c ON c.ID = i.Item_Category_ID
    LEFT JOIN unit un ON un.id = ISNULL(b.unit_id, i.Unit_Id)
    CROSS APPLY (SELECT CAST(COALESCE(NULLIF(b.Small_unit, 0), NULLIF(un.S_unit_Value, 0), 1) AS decimal(18,6)) AS F) f
    WHERE ({f.EmployeeId} = 0 OR b.EmployeeId = {f.EmployeeId})
      AND ({f.StoreId} = 0 OR i.S_id = {f.StoreId})
      AND ({f.CategoryId} = 0 OR i.Item_Category_ID = {f.CategoryId})
      AND ({text} = N'' OR i.Item_Name LIKE {like} OR i.code = {text} OR e.name LIKE {like})
), v AS (
    SELECT l.*, CAST(CASE WHEN l.Price_Large IS NULL THEN NULL ELSE l.Qty_Small / l.Factor * l.Price_Large END AS decimal(18,2)) AS Value
    FROM l WHERE {includeZero} = 1 OR l.Qty_Small <> 0
)
SELECT v.*, COUNT(*) OVER () AS Total_Rows, (SELECT COUNT(DISTINCT x.Employee_Id) FROM v x) AS Employees,
       CAST(ISNULL(SUM(v.Value) OVER (), 0) AS decimal(18,2)) AS Total_Value
FROM v
ORDER BY v.Employee_Name, v.Item_Name
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList());
        }

        private T Run<T>(Func<operations, T> f) => f(_newOpp());
    }
}
