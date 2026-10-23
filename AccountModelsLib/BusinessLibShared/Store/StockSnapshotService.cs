using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>
    /// لقطات أرصدة الأصناف بالتاريخ والوقت (خطة 05 بند 23) لصفحة /store/balances/snapshots: بديل balanceSelect_Date وbalanceWithDate.
    /// اللقطة = نسخة من كل أسطر Unit_item فى Unit_Item_Balance_Date_Details بنفس Balance_Date. خلافاً للديسك توب لا تُحذف لقطة سابقة
    /// ولا يُعاد حفظها (سجل ثابت للمراجعة): كل حفظ يأخذ تاريخه ووقته حتى الثانية، ولا يُستدعى Common.Update_UnitItem لأنه يعدّل الأرصدة.
    /// </summary>
    public class StockSnapshotService
    {
        private readonly Func<operations> _newOpp;

        public StockSnapshotService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        public class SnapshotRow
        {
            public DateTime Balance_Date { get; set; }
            public int Lines { get; set; }
            public int Non_Zero_Lines { get; set; }
            public decimal Total_Value { get; set; }
            public string User_Name { get; set; }
        }

        public class CompareFilter
        {
            public DateTime Snapshot { get; set; }
            /// <summary>null = المقارنة بالرصيد الحالى (Unit_item)؛ وإلا بلقطة أخرى.</summary>
            public DateTime? CompareTo { get; set; }
            public long StoreId { get; set; }
            public long CategoryId { get; set; }
            public string Text { get; set; }
            public bool OnlyDifferences { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
        }

        public class CompareRow
        {
            public long Unit_Item_Id { get; set; }
            public string Store_Name { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Snapshot_Small { get; set; }
            public decimal Other_Small { get; set; }
            public decimal Diff_Small { get; set; }
            public int Total_Rows { get; set; }
            public int Diff_Rows { get; set; }
        }

        public class Option
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        /// <summary>اللقطات المحفوظة (الأحدث أولاً) بعدد أسطرها وقيمتها بسعر الشراء المحفوظ معها ومن حفظها.</summary>
        public List<SnapshotRow> GetSnapshots() => Run(opp => opp.db.Database.SqlQuery<SnapshotRow>($@"
SELECT g.Balance_Date, g.Lines, g.Non_Zero_Lines, g.Total_Value, ISNULL(ud.USER_NAME, N'') AS User_Name
FROM (
    SELECT d.Balance_Date, COUNT(*) AS Lines,
           SUM(CASE WHEN ISNULL(d.Large_Balance, 0) <> 0 OR ISNULL(d.Small_Balance, 0) <> 0 THEN 1 ELSE 0 END) AS Non_Zero_Lines,
           CAST(ISNULL(SUM((ISNULL(d.Large_Balance, 0) + ISNULL(d.Small_Balance, 0) / CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END)
                           * ISNULL(d.P_price, 0)), 0) AS decimal(18,2)) AS Total_Value,
           MIN(d.User_Id) AS User_Id
    FROM Unit_Item_Balance_Date_Details d
    LEFT JOIN Unit_item u ON u.id = d.Unit_Item_Id
    WHERE d.Balance_Date IS NOT NULL
    GROUP BY d.Balance_Date) g
LEFT JOIN USER_DETAILES ud ON ud.Id = g.User_Id
ORDER BY g.Balance_Date DESC").ToList());

        public List<Option> GetStores() => Run(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT Id, Store_Name AS Name FROM Store
WHERE Active = 1 AND ISNULL(IS_ServicesVirtual_Item_NoBalance, 0) = 0
ORDER BY IS_Default_Store DESC, Store_Name").ToList());

        /// <summary>
        /// يحفظ لقطة بكل أسطر Unit_item الآن (معاملة واحدة) ويرجع تاريخها ووقتها. يرفض إن وُجدت لقطة بنفس الثانية،
        /// ولا يحذف ولا يعدّل أى لقطة سابقة.
        /// </summary>
        public DateTime SaveSnapshot()
        {
            var now = DateTime.Now;
            var at = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            long userId = Common.User_Id, branchId = Common.Branch_Id;
            var opp = _newOpp();
            opp.BeginTransaction();
            try
            {
                bool exists = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Unit_Item_Balance_Date_Details WHERE Balance_Date = {at}").AsEnumerable().First() > 0;
                if (exists) throw new InvalidOperationException("توجد لقطة محفوظة بنفس التاريخ والوقت، أعد المحاولة بعد ثانية.");
                const string cols = "Balance_Date, Item_Id, Unit_Item_Id, Large_Balance, Small_Balance, Item_No, S_price, P_price, Store_Id, Unit_Id, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime";
                const string vals = "{0}, u.Item_id, u.id, ISNULL(u.Full_qua, 0), ISNULL(u.D_Qua, 0), ISNULL(u.Item_No, 0), ISNULL(u.S_price, 0), ISNULL(u.P_price, 0), " +
                                    "ISNULL(u.Store_id, 0), ISNULL(u.unit_id, 0), GETDATE(), {1}, {2}, GETDATE()";
                // ID ليس Identity فى بعض القواعد (Snowflake): رقم أساس جديد + ترتيب السطر؛ اللقطة التالية تبدأ من أساس أكبر بكثير (فارق الزمن)
                string sql = IsIdentity(opp, "Unit_Item_Balance_Date_Details", "ID")
                    ? $"INSERT INTO Unit_Item_Balance_Date_Details ({cols}) SELECT {vals} FROM Unit_item u"
                    : $"INSERT INTO Unit_Item_Balance_Date_Details (ID, {cols}) SELECT {{3}} + ROW_NUMBER() OVER (ORDER BY u.id), {vals} FROM Unit_item u";
                opp.db.Database.ExecuteSql(FormattableStringFactory.Create(sql, at, userId, branchId, CallSnowflakeIdGenerate.GenerateNewId()));
                opp.Commit();
                return at;
            }
            catch
            {
                opp.Rollback();
                throw;
            }
        }

        /// <summary>
        /// مقارنة لقطة بالرصيد الحالى أو بلقطة أخرى لكل سطر مخزون (بالوحدة الصغرى، بعامل الوحدة الحالى)، مرتبة بالفرق الأكبر.
        /// يُعرض السطر إن كان له رصيد فى أحد الطرفين (أو فرق فقط عند OnlyDifferences).
        /// </summary>
        public List<CompareRow> Compare(CompareFilter f)
        {
            string text = (f.Text ?? "").Trim();
            string like = "%" + text + "%";
            int useOther = f.CompareTo.HasValue ? 1 : 0;
            DateTime other = f.CompareTo ?? f.Snapshot;
            int onlyDiff = f.OnlyDifferences ? 1 : 0;
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            return Run(opp => opp.db.Database.SqlQuery<CompareRow>($@"
WITH a AS (SELECT Unit_Item_Id, SUM(ISNULL(Large_Balance, 0)) AS L, SUM(ISNULL(Small_Balance, 0)) AS S
           FROM Unit_Item_Balance_Date_Details WHERE Balance_Date = {f.Snapshot} GROUP BY Unit_Item_Id),
b AS (SELECT Unit_Item_Id, SUM(ISNULL(Large_Balance, 0)) AS L, SUM(ISNULL(Small_Balance, 0)) AS S
      FROM Unit_Item_Balance_Date_Details WHERE {useOther} = 1 AND Balance_Date = {other} GROUP BY Unit_Item_Id),
k AS (SELECT Unit_Item_Id FROM a UNION SELECT Unit_Item_Id FROM b UNION SELECT id FROM Unit_item WHERE {useOther} = 0),
l AS (
    SELECT u.id AS Unit_Item_Id, ISNULL(s.Store_Name, N'') AS Store_Name, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
           ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit, f.F AS Factor,
           CAST(ISNULL(a.L, 0) * f.F + ISNULL(a.S, 0) AS decimal(18,4)) AS Snapshot_Small,
           CAST(CASE WHEN {useOther} = 1 THEN ISNULL(b.L, 0) * f.F + ISNULL(b.S, 0)
                     ELSE ISNULL(u.Full_qua, 0) * f.F + ISNULL(u.D_Qua, 0) END AS decimal(18,4)) AS Other_Small
    FROM k
    INNER JOIN Unit_item u ON u.id = k.Unit_Item_Id
    INNER JOIN Item i ON i.id = u.Item_id
    LEFT JOIN Store s ON s.Id = u.Store_id
    LEFT JOIN unit un ON un.id = u.unit_id
    LEFT JOIN a ON a.Unit_Item_Id = u.id
    LEFT JOIN b ON b.Unit_Item_Id = u.id
    CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS F) f
    WHERE ({f.StoreId} = 0 OR u.Store_id = {f.StoreId})
      AND ({f.CategoryId} = 0 OR i.Item_Category_ID = {f.CategoryId})
      AND ({text} = N'' OR i.Item_Name LIKE {like} OR u.Code = {text} OR i.code = {text})
), v AS (
    SELECT l.*, CAST(l.Other_Small - l.Snapshot_Small AS decimal(18,4)) AS Diff_Small FROM l
    WHERE l.Snapshot_Small <> 0 OR l.Other_Small <> 0
)
SELECT v.*, COUNT(*) OVER () AS Total_Rows, SUM(CASE WHEN v.Diff_Small <> 0 THEN 1 ELSE 0 END) OVER () AS Diff_Rows
FROM v
WHERE {onlyDiff} = 0 OR v.Diff_Small <> 0
ORDER BY ABS(v.Diff_Small) DESC, v.Store_Name, v.Item_Name
OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY").ToList());
        }

        private static bool IsIdentity(operations opp, string table, string col) =>
            opp.db.Database.SqlQuery<int>(FormattableStringFactory.Create(
                "SELECT ISNULL(COLUMNPROPERTY(OBJECT_ID({0}), {1}, 'IsIdentity'), 0) AS Value", table, col)).AsEnumerable().First() == 1;

        private T Run<T>(Func<operations, T> f) => f(_newOpp());
    }
}
