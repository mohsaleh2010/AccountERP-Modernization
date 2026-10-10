using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using static AccountModelsLib.StoreOrderService;

namespace AccountModelsLib
{
    /// <summary>
    /// واجهة صفحة الإذن (خطة 03 — المرحلة 2) فوق StoreOrderService: كل دالة تفتح اتصالها ومعاملتها وتُنهيها،
    /// والقراءات بـ SQL مُمرَّر بالمعاملات. لا منطق مخزون هنا؛ التحقق والترحيل كله فى StoreOrderService/المحرك.
    /// </summary>
    public class StoreOrderPageService
    {
        private readonly Func<operations> _newOpp;

        /// <summary>newOpp: مصدر الاتصال (افتراضياً operations() بإعداد البرنامج)؛ يُمرَّر اتصال آخر للاختبار فقط.</summary>
        public StoreOrderPageService(Func<operations> newOpp = null) => _newOpp = newOpp ?? (() => new operations());

        /// <summary>جهة اختيار (مخزن، قسم، عميل، مورد، موظف، ماكينة).</summary>
        public class PartyOption
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        /// <summary>سطر مخزون للاختيار مع رصيده.</summary>
        public class StockLineOption
        {
            public long Id { get; set; }
            public long Item_Id { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Full_qua { get; set; }
            public decimal D_Qua { get; set; }
            public decimal Avg_Cost_Large { get; set; }
            public bool Without_Balance { get; set; }
        }

        public class OrderHeaderView
        {
            public long Id { get; set; }
            public int Order_Type { get; set; }
            public long Order_No { get; set; }
            public DateTime Order_Date { get; set; }
            public int Type_From { get; set; }
            public long Id_From { get; set; }
            public int Type_To { get; set; }
            public long Id_To { get; set; }
            public string Notes { get; set; }
            public int Doc_Status { get; set; }
            public long? Reversal_Of_Id { get; set; }
            public long? Reversed_By_Id { get; set; }
            public long? Reversal_Of_No { get; set; }
            public long? Reversed_By_No { get; set; }
            /// <summary>هل للإذن حركات من المحرك؟ الأذون القديمة (قبل المحرك) لا تُعكس من الصفحة الجديدة.</summary>
            public bool Engine_Posted { get; set; }
            /// <summary>التحويل بخطوتين: حالة الشحنة (1 فى الطريق، 2 جزئى، 3 مكتمل/مغلق) وسبب إغلاق العجز.</summary>
            public int? Transfer_Status { get; set; }
            public string Transfer_Close_Reason { get; set; }
            /// <summary>إذن الاستلام (1016): الشحنة التى استلمها.</summary>
            public long? Source_Order_Id { get; set; }
            public long? Source_Order_No { get; set; }
            public int? Source_Order_Type { get; set; }
        }

        /// <summary>سطر شحنة مع المشحون والمستلم والباقى (بالوحدة الصغرى) لبطاقة الاستلام.</summary>
        public class TransferLineView
        {
            public long Detail_Id { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Shipped_Small { get; set; }
            public decimal Received_Small { get; set; }
            public decimal Remaining_Small { get; set; }
        }

        /// <summary>ما تعرضه الصفحة عن طريقة التحويل للمستخدم الحالى.</summary>
        public record TransferUi(StoreTransferPolicy.Mode Mode, bool ShowDirectChoice);

        /// <summary>فلتر قائمة الأذون. OrderType = 0 لكل الأنواع، Status = -1 لكل الحالات.</summary>
        public class OrderSearchFilter
        {
            public int OrderType { get; set; }
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
            public int Status { get; set; } = -1;
            public string Text { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; } = 50;
            /// <summary>شحنات التحويل بانتظار الاستلام فقط (فى الطريق أو مستلمة جزئياً).</summary>
            public bool PendingTransfers { get; set; }
        }

        /// <summary>صف فى قائمة الأذون.</summary>
        public class OrderListRow
        {
            public long Id { get; set; }
            public int Order_Type { get; set; }
            public long Order_No { get; set; }
            public DateTime Order_Date { get; set; }
            public int Type_From { get; set; }
            public string From_Name { get; set; }
            public int Type_To { get; set; }
            public string To_Name { get; set; }
            public int Doc_Status { get; set; }
            public long? Reversal_Of_Id { get; set; }
            public int Lines_Count { get; set; }
            public decimal? Total_Value { get; set; }
            public string Notes { get; set; }
            public int? Transfer_Status { get; set; }
            public bool Has_Shortage { get; set; }
            public int Total_Rows { get; set; }
        }

        public class StatusCount
        {
            public int Doc_Status { get; set; }
            public int Cnt { get; set; }
        }

        /// <summary>أنواع الأذون المفعّلة (للفلتر).</summary>
        public List<OrderTypeOption> GetOrderTypes() => Run(opp => opp.db.Database.SqlQuery<OrderTypeOption>($@"
SELECT Order_Type, Name_Ar, Direction FROM Store_Order_Type_Settings WHERE Is_Active = 1 ORDER BY Order_Type").ToList());

        public class OrderTypeOption
        {
            public int Order_Type { get; set; }
            public string Name_Ar { get; set; }
            public int Direction { get; set; }
        }

        public class OrderLineView
        {
            public long Unit_Item_Id { get; set; }
            public string Item_Name { get; set; }
            public string Code { get; set; }
            public string B_Unit { get; set; }
            public string S_Unit { get; set; }
            public decimal Factor { get; set; }
            public decimal Qty_Large { get; set; }
            public decimal Qty_Small { get; set; }
            public decimal? Unit_Cost_Large { get; set; }
            public string Notes { get; set; }
            /// <summary>الرصيد الحالى لسطر المخزون بالوحدة الصغرى (لمعاينة الرصيد بعد فى المسودة).</summary>
            public decimal Balance_Small { get; set; }
        }

        /// <summary>الجهات التى لها قائمة اختيار (الباقى — هالك، تسوية، إنتاج، تشغيل، ورشة، قطع غيار — بلا معرّف).</summary>
        public static bool PartyHasList(int partyType) => partyType is PartyStore or PartyDepartment or 1 or 2 or 23 or 40;

        public OrderTypeSettings GetSettings(int orderType) => Run(opp => new StoreOrderService(opp).GetSettings(orderType));

        /// <summary>قائمة الجهة كما فى FillcbOrderStorCLS / OrderChoiceCLS (المخزن والقسم بعلَم المخزن الخارجى الوهمى).</summary>
        public List<PartyOption> GetParties(int partyType, bool virtualOuter)
        {
            return Run(opp =>
            {
                var db = opp.db.Database;
                int outer = virtualOuter ? 1 : 0;
                return partyType switch
                {
                    PartyStore or PartyDepartment => db.SqlQuery<PartyOption>($@"
SELECT Id, Store_Name AS Name FROM Store
WHERE Active = 1 AND ISNULL(IS_Virtual_OuterStore, 0) = {outer} AND ISNULL(LookUp_StoreOrderTypesId, 4) = {partyType}
  AND ISNULL(IS_ServicesVirtual_Item_NoBalance, 0) = 0
ORDER BY IS_Default_Store DESC, Store_Name").ToList(),
                    1 => db.SqlQuery<PartyOption>($"SELECT Id, Name FROM Supplier WHERE active = 1 ORDER BY Name").ToList(),
                    2 => db.SqlQuery<PartyOption>($"SELECT Id, Name FROM Customer WHERE active = 1 ORDER BY Name").ToList(),
                    40 => db.SqlQuery<PartyOption>($"SELECT CAST(id AS bigint) AS Id, name AS Name FROM Employee WHERE active = 1 ORDER BY name").ToList(),
                    23 => db.SqlQuery<PartyOption>($"SELECT CAST(Id AS bigint) AS Id, Machin_Name AS Name FROM Machin_list WHERE active = 1 ORDER BY Machin_Name").ToList(),
                    _ => new List<PartyOption>(),
                };
            });
        }

        /// <summary>أصناف المخزن/القسم بالاسم أو الكود (أول 50)، مع الرصيد ومتوسط الفرع بالوحدة الكبرى.</summary>
        public List<StockLineOption> SearchLines(int storeType, long storeId, string text, bool withBalanceOnly)
        {
            if (storeId <= 0) return new List<StockLineOption>();
            string like = "%" + (text ?? "").Trim() + "%";
            string code = (text ?? "").Trim();
            int onlyBal = withBalanceOnly ? 1 : 0;
            return Run(opp => opp.db.Database.SqlQuery<StockLineOption>($@"
SELECT TOP 50 u.id AS Id, u.Item_id AS Item_Id, i.Item_Name, ISNULL(u.Code, i.code) AS Code,
       ISNULL(un.B_Unit, '') AS B_Unit, ISNULL(un.S_unit, '') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(ISNULL(u.Full_qua, 0) AS decimal(18,4)) AS Full_qua, CAST(ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS D_Qua,
       CAST(ISNULL(u.P_priceAvgCost, 0) AS decimal(18,6)) AS Avg_Cost_Large,
       CAST(CASE WHEN ISNULL(i.Is_ItemWithoutBalance, 0) = 1 THEN 1 ELSE 0 END AS bit) AS Without_Balance
FROM Unit_item u
INNER JOIN Item i ON i.id = u.Item_id
LEFT JOIN unit un ON un.id = u.unit_id
WHERE u.Store_id = {storeId} AND ISNULL(u.Store_Type, 4) = {storeType}
  AND ISNULL(i.Active_flage, 1) = 1 AND ISNULL(u.active_flage, 1) = 1
  AND (i.Item_Name LIKE {like} OR u.Code = {code} OR i.code = {code})
  AND ({onlyBal} = 0 OR ISNULL(u.Full_qua, 0) > 0 OR ISNULL(u.D_Qua, 0) > 0)
ORDER BY i.Item_Name").ToList());
        }

        // ============================== دورة المستند ==============================

        public long SaveDraft(OrderInput input, long draftId) => RunTx(opp => new StoreOrderService(opp).SaveDraft(input, draftId));

        /// <summary>
        /// حفظ ثم ترحيل فى معاملة واحدة: لو فشل الترحيل لا تبقى المسودة معدَّلة. طريقة التحويل (1001/1006) تحددها
        /// StoreTransferPolicy لا الصفحة؛ directRequested يُعتبر فقط مع الإعداد 2 ولمن يملك الصلاحية.
        /// </summary>
        public long SaveAndPost(OrderInput input, long draftId, bool directRequested = false)
        {
            return RunTx(opp =>
            {
                var svc = new StoreOrderService(opp);
                long id = svc.SaveDraft(input, draftId);
                var mode = svc.GetSettings(input.OrderType).Is_Transfer
                    ? StoreTransferPolicy.Effective(opp, Common.User_Id, directRequested)
                    : (TransferMode?)null;
                svc.Post(id, mode);
                return id;
            });
        }

        public TransferUi GetTransferUi() => Run(opp => new TransferUi(StoreTransferPolicy.GetMode(opp), StoreTransferPolicy.ShowDirectChoice(opp, Common.User_Id)));

        /// <summary>أسطر الشحنة مع المشحون والمستلم والباقى.</summary>
        public List<TransferLineView> GetTransferLines(long shipmentId) => Run(opp =>
        {
            var prog = new StoreOrderService(opp).GetTransferProgress(shipmentId).ToDictionary(p => p.Detail_Id);
            var names = opp.db.Database.SqlQuery<TransferLineView>($@"
SELECT d.Id AS Detail_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
       ISNULL(un.B_Unit, '') AS B_Unit, ISNULL(un.S_unit, '') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(0 AS decimal(18,4)) AS Shipped_Small, CAST(0 AS decimal(18,4)) AS Received_Small, CAST(0 AS decimal(18,4)) AS Remaining_Small
FROM Detailes_Purshing d
LEFT JOIN Unit_item u ON u.id = d.Unit_item_Table_Id
LEFT JOIN Item i ON i.id = ISNULL(u.Item_id, NULLIF(d.Item_Id, 0))
LEFT JOIN unit un ON un.id = u.unit_id
WHERE d.OrderTable_id = {shipmentId}
ORDER BY d.Id").ToList();
            foreach (var n in names)
                if (prog.TryGetValue(n.Detail_Id, out var p))
                {
                    n.Shipped_Small = p.Shipped_Small; n.Received_Small = p.Received_Small; n.Remaining_Small = p.Remaining_Small;
                }
            return names;
        });

        public long ReceiveTransfer(long shipmentId, DateTime date, IReadOnlyList<ReceiptLineInput> lines, string notes) =>
            RunTx(opp => new StoreOrderService(opp).ReceiveTransfer(shipmentId, date, lines, notes));

        public void CloseTransferShortage(long shipmentId, string reason) =>
            RunTx(opp => { new StoreOrderService(opp).CloseTransferShortage(shipmentId, reason); return 0; });

        public void CancelDraft(long orderId) => RunTx(opp => { new StoreOrderService(opp).CancelDraft(orderId); return 0; });

        public long Reverse(long orderId, string reason) => RunTx(opp => new StoreOrderService(opp).Reverse(orderId, reason));

        public long ReverseAndCopy(long orderId, string reason) => RunTx(opp => new StoreOrderService(opp).ReverseAndCopy(orderId, reason));

        // ============================== القراءة ==============================

        public (OrderHeaderView Header, List<OrderLineView> Lines) GetOrder(long orderId)
        {
            return Run(opp =>
            {
                var h = opp.db.Database.SqlQuery<OrderHeaderView>($@"
SELECT p.Id, CASE WHEN p.Order_Type BETWEEN 1 AND 23 THEN p.Order_Type + 1000 ELSE ISNULL(p.Order_Type, 0) END AS Order_Type, CAST(ISNULL(p.Order_Purshing_No, 0) AS bigint) AS Order_No,
       ISNULL(p.Order_date, GETDATE()) AS Order_Date, ISNULL(p.type_From, 0) AS Type_From, ISNULL(p.Id_From, 0) AS Id_From,
       ISNULL(p.Type_To, 0) AS Type_To, ISNULL(p.Id_To, 0) AS Id_To, p.NotesAll AS Notes, ISNULL(p.Doc_Status, 1) AS Doc_Status,
       p.Reversal_Of_Id, p.Reversed_By_Id,
       (SELECT CAST(o.Order_Purshing_No AS bigint) FROM Order_Purshing o WHERE o.Id = p.Reversal_Of_Id) AS Reversal_Of_No,
       (SELECT CAST(o.Order_Purshing_No AS bigint) FROM Order_Purshing o WHERE o.Id = p.Reversed_By_Id) AS Reversed_By_No,
       CAST(CASE WHEN EXISTS (SELECT 1 FROM Detailes_Purshing_Import_Export e WHERE e.OrderTable_id = p.Id AND e.Engine_Posted = 1)
                 THEN 1 ELSE 0 END AS bit) AS Engine_Posted,
       p.Transfer_Status, p.Transfer_Close_Reason, p.Source_Order_Id,
       (SELECT CAST(o.Order_Purshing_No AS bigint) FROM Order_Purshing o WHERE o.Id = p.Source_Order_Id) AS Source_Order_No,
       (SELECT o.Order_Type FROM Order_Purshing o WHERE o.Id = p.Source_Order_Id) AS Source_Order_Type
FROM Order_Purshing p WHERE p.Id = {orderId}").AsEnumerable().FirstOrDefault();
                if (h == null) return (null, new List<OrderLineView>());

                // الإذن العكسى بلا أسطر: تُعرض أسطر الأصل للمرجع.
                long linesOf = h.Reversal_Of_Id ?? h.Id;
                var lines = opp.db.Database.SqlQuery<OrderLineView>($@"
SELECT d.Unit_item_Table_Id AS Unit_Item_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(ISNULL(u.Code, i.code), N'') AS Code,
       ISNULL(un.B_Unit, '') AS B_Unit, ISNULL(un.S_unit, '') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(ISNULL(d.qua, 0) AS decimal(18,4)) AS Qty_Large, CAST(ISNULL(d.Part_qua, 0) AS decimal(18,4)) AS Qty_Small,
       CAST(d.PriceUnit AS decimal(18,6)) AS Unit_Cost_Large, d.Notes,
       CAST(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Balance_Small
FROM Detailes_Purshing d
LEFT JOIN Unit_item u ON u.id = d.Unit_item_Table_Id   -- أذون قديمة قد يكون سطر مخزونها محذوفاً: يظهر السطر بلا اسم
LEFT JOIN Item i ON i.id = ISNULL(u.Item_id, NULLIF(d.Item_Id, 0))
LEFT JOIN unit un ON un.id = u.unit_id
WHERE d.OrderTable_id = {linesOf}
ORDER BY d.Id").ToList();
                return (h, lines);
            });
        }

        // ============================== قائمة الأذون (المرحلة 4) ==============================

        /// <summary>
        /// بحث الأذون (بديل Order_detailes): فلتر بالنوع والفترة والحالة ونص (رقم الإذن بالضبط أو اسم الجهة أو الملاحظات)، مرتَّب بالأحدث،
        /// مع ترقيم صفحات. اسم الجهة حسب نوعها (مخزن/قسم، مورد، عميل، موظف، ماكينة)؛ القيمة من دفتر المحرك فقط
        /// (|الكمية الصغرى| × تكلفة الوحدة الصغرى، والتحويل نصفها لأن له صفّى صرف واستلام).
        /// </summary>
        public List<OrderListRow> SearchOrders(OrderSearchFilter f)
        {
            var (cte, args) = BaseQuery(f);
            int skip = Math.Max(0, f.Skip), take = Math.Clamp(f.Take, 1, 500);
            var all = args.Concat(new object[] { f.Status, skip, take }).ToArray();
            int a = args.Length;
            string sql = cte + $@"
SELECT q.Id, q.Order_Type, q.Order_No, q.Order_Date, q.Type_From, q.From_Name, q.Type_To, q.To_Name, q.Doc_Status, q.Reversal_Of_Id, q.Notes, q.Transfer_Status, q.Has_Shortage,
       (SELECT COUNT(*) FROM Detailes_Purshing d WHERE d.OrderTable_id = ISNULL(q.Reversal_Of_Id, q.Id)) AS Lines_Count,
       (SELECT CAST(SUM(ABS(ISNULL(e.Qty_Small, 0)) * ISNULL(e.Unit_Cost, 0)) / CASE WHEN q.Order_Type IN (1001, 1006, 1016, 1017) THEN 2 ELSE 1 END AS decimal(18,2))
        FROM Detailes_Purshing_Import_Export e WHERE e.OrderTable_id = q.Id AND e.Engine_Posted = 1) AS Total_Value,
       COUNT(*) OVER () AS Total_Rows
FROM q
WHERE ({{{a}}} = -1 OR q.Doc_Status = {{{a}}})
ORDER BY q.Order_Date DESC, q.Id DESC
OFFSET {{{a + 1}}} ROWS FETCH NEXT {{{a + 2}}} ROWS ONLY";
            return Run(opp => opp.db.Database.SqlQuery<OrderListRow>(FormattableStringFactory.Create(sql, all)).ToList());
        }

        /// <summary>عدد الأذون لكل حالة بنفس فلتر القائمة عدا الحالة (لشرائح الحالة).</summary>
        public List<StatusCount> CountByStatus(OrderSearchFilter f)
        {
            var (cte, args) = BaseQuery(f);
            string sql = cte + "\nSELECT q.Doc_Status, COUNT(*) AS Cnt FROM q GROUP BY q.Doc_Status";
            return Run(opp => opp.db.Database.SqlQuery<StatusCount>(FormattableStringFactory.Create(sql, args)).ToList());
        }

        /// <summary>
        /// تعريف الأذون المطابقة للفلتر (عدا الحالة) كـ CTE باسم q، والقيم كمعاملات مرقّمة {0}..{n} لا نصاً داخل SQL.
        /// الأذون القديمة جداً بأرقام الأنواع القصيرة 1..23 (Load_Combo1) تُعرض كنوعها الحالى 1001..1023.
        /// مشترك بين القائمة وعدّاد الحالات فيتطابقان دائماً.
        /// </summary>
        private static (string Sql, object[] Args) BaseQuery(OrderSearchFilter f)
        {
            string text = (f.Text ?? "").Trim();
            long no = long.TryParse(text, out var n) ? n : -1;
            DateTime from = (f.From ?? new DateTime(1900, 1, 1)).Date;
            DateTime to = (f.To ?? new DateTime(2999, 1, 1)).Date.AddDays(1);
            var args = new object[] { f.OrderType, from, to, text, no, "%" + text + "%", f.PendingTransfers ? 1 : 0 };
            const string sql = @"
WITH q AS (
    SELECT p.Id, CASE WHEN p.Order_Type BETWEEN 1 AND 23 THEN p.Order_Type + 1000 ELSE ISNULL(p.Order_Type, 0) END AS Order_Type, CAST(ISNULL(p.Order_Purshing_No, 0) AS bigint) AS Order_No,
           ISNULL(p.Order_date, p.Insert_ServerDateTime) AS Order_Date,
           ISNULL(p.type_From, 0) AS Type_From, fn.Name AS From_Name, ISNULL(p.Type_To, 0) AS Type_To, tn.Name AS To_Name,
           ISNULL(p.Doc_Status, 1) AS Doc_Status, p.Reversal_Of_Id, p.NotesAll AS Notes, p.Transfer_Status,
           CAST(CASE WHEN p.Transfer_Close_Reason IS NOT NULL THEN 1 ELSE 0 END AS bit) AS Has_Shortage
    FROM Order_Purshing p
    OUTER APPLY (SELECT CASE WHEN p.type_From IN (4, 20) THEN (SELECT Store_Name FROM Store WHERE Id = p.Id_From)
                             WHEN p.type_From = 1 THEN (SELECT Name FROM Supplier WHERE Id = p.Id_From)
                             WHEN p.type_From = 2 THEN (SELECT Name FROM Customer WHERE Id = p.Id_From)
                             WHEN p.type_From = 40 THEN (SELECT name FROM Employee WHERE id = p.Id_From)
                             WHEN p.type_From = 23 THEN (SELECT Machin_Name FROM Machin_list WHERE Id = p.Id_From) END AS Name) fn
    OUTER APPLY (SELECT CASE WHEN p.Type_To IN (4, 20) THEN (SELECT Store_Name FROM Store WHERE Id = p.Id_To)
                             WHEN p.Type_To = 1 THEN (SELECT Name FROM Supplier WHERE Id = p.Id_To)
                             WHEN p.Type_To = 2 THEN (SELECT Name FROM Customer WHERE Id = p.Id_To)
                             WHEN p.Type_To = 40 THEN (SELECT name FROM Employee WHERE id = p.Id_To)
                             WHEN p.Type_To = 23 THEN (SELECT Machin_Name FROM Machin_list WHERE Id = p.Id_To) END AS Name) tn
    WHERE ISNULL(p.Order_Purshing_No, 0) <> -1
      AND (p.Order_Type BETWEEN 1001 AND 1023 OR p.Order_Type BETWEEN 1 AND 23)
      AND ({0} = 0 OR CASE WHEN p.Order_Type BETWEEN 1 AND 23 THEN p.Order_Type + 1000 ELSE ISNULL(p.Order_Type, 0) END = {0})
      AND ISNULL(p.Order_date, p.Insert_ServerDateTime) >= {1} AND ISNULL(p.Order_date, p.Insert_ServerDateTime) < {2}
      AND ({3} = N'' OR p.Order_Purshing_No = {4} OR fn.Name LIKE {5} OR tn.Name LIKE {5} OR p.NotesAll LIKE {5})
      AND ({6} = 0 OR (p.Transfer_Status IN (1, 2) AND ISNULL(p.Doc_Status, 1) = 1))
)";
            return (sql, args);
        }

        // ============================== مساعدات ==============================

        private T Run<T>(Func<operations, T> f) => f(_newOpp());

        private T RunTx<T>(Func<operations, T> f)
        {
            var opp = _newOpp();
            opp.BeginTransaction();
            try
            {
                T r = f(opp);
                opp.Commit();
                return r;
            }
            catch
            {
                opp.Rollback();
                throw;
            }
        }
    }
}
