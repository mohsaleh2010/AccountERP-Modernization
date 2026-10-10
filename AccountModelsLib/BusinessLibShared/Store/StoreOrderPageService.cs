using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>حفظ ثم ترحيل فى معاملة واحدة: لو فشل الترحيل لا تبقى المسودة معدَّلة.</summary>
        public long SaveAndPost(OrderInput input, long draftId, TransferMode? mode)
        {
            return RunTx(opp =>
            {
                var svc = new StoreOrderService(opp);
                long id = svc.SaveDraft(input, draftId);
                svc.Post(id, mode);
                return id;
            });
        }

        public void CancelDraft(long orderId) => RunTx(opp => { new StoreOrderService(opp).CancelDraft(orderId); return 0; });

        public long Reverse(long orderId, string reason) => RunTx(opp => new StoreOrderService(opp).Reverse(orderId, reason));

        public long ReverseAndCopy(long orderId, string reason) => RunTx(opp => new StoreOrderService(opp).ReverseAndCopy(orderId, reason));

        // ============================== القراءة ==============================

        public (OrderHeaderView Header, List<OrderLineView> Lines) GetOrder(long orderId)
        {
            return Run(opp =>
            {
                var h = opp.db.Database.SqlQuery<OrderHeaderView>($@"
SELECT p.Id, ISNULL(p.Order_Type, 0) AS Order_Type, CAST(ISNULL(p.Order_Purshing_No, 0) AS bigint) AS Order_No,
       ISNULL(p.Order_date, GETDATE()) AS Order_Date, ISNULL(p.type_From, 0) AS Type_From, ISNULL(p.Id_From, 0) AS Id_From,
       ISNULL(p.Type_To, 0) AS Type_To, ISNULL(p.Id_To, 0) AS Id_To, p.NotesAll AS Notes, ISNULL(p.Doc_Status, 1) AS Doc_Status,
       p.Reversal_Of_Id, p.Reversed_By_Id,
       (SELECT CAST(o.Order_Purshing_No AS bigint) FROM Order_Purshing o WHERE o.Id = p.Reversal_Of_Id) AS Reversal_Of_No,
       (SELECT CAST(o.Order_Purshing_No AS bigint) FROM Order_Purshing o WHERE o.Id = p.Reversed_By_Id) AS Reversed_By_No
FROM Order_Purshing p WHERE p.Id = {orderId}").AsEnumerable().FirstOrDefault();
                if (h == null) return (null, new List<OrderLineView>());

                // الإذن العكسى بلا أسطر: تُعرض أسطر الأصل للمرجع.
                long linesOf = h.Reversal_Of_Id ?? h.Id;
                var lines = opp.db.Database.SqlQuery<OrderLineView>($@"
SELECT d.Unit_item_Table_Id AS Unit_Item_Id, i.Item_Name, ISNULL(u.Code, i.code) AS Code,
       ISNULL(un.B_Unit, '') AS B_Unit, ISNULL(un.S_unit, '') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(ISNULL(d.qua, 0) AS decimal(18,4)) AS Qty_Large, CAST(ISNULL(d.Part_qua, 0) AS decimal(18,4)) AS Qty_Small,
       CAST(d.PriceUnit AS decimal(18,6)) AS Unit_Cost_Large, d.Notes,
       CAST(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Balance_Small
FROM Detailes_Purshing d
INNER JOIN Unit_item u ON u.id = d.Unit_item_Table_Id
INNER JOIN Item i ON i.id = u.Item_id
LEFT JOIN unit un ON un.id = u.unit_id
WHERE d.OrderTable_id = {linesOf}
ORDER BY d.Id").ToList();
                return (h, lines);
            });
        }

        // ============================== مساعدات ==============================

        private static T Run<T>(Func<operations, T> f) => f(new operations());

        private static T RunTx<T>(Func<operations, T> f)
        {
            var opp = new operations();
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
