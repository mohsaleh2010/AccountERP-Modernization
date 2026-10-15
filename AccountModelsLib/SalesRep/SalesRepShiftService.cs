using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 2: الوردية والتحميل والتفريغ ونقل التالف والإقفال (§6.1، §6.7 بعد توافق خطة 06).
    /// كل حركة مخزنية = إذن مخزن عادى عبر StoreOrderService والمحرك (لا كتابة رصيد هنا):
    /// - التحميل/التفريغ/نقل التالف = تحويل 1001؛ مباشر أو بتأكيد استلام حسب StoreTransferPolicy (نفس قاعدة المخازن).
    /// - الوردية تُفتح عند تأكيد المندوب استلام التحميل (م-06-2)، أو عند الإصدار لو التحويل مباشر، أو يدوياً لو السيارة
    ///   بها عهدة من أمس (ق9). وردية واحدة غير مقفلة لكل مندوب (فهرس فى القاعدة).
    /// - الإقفال: المتوقع = رصيد السيارة المحسوب من دفتر المحرك، والفعلى من الجرد، والفرق أذون تسوية 1008/1007 مسودة
    ///   يعتمدها المشرف من صفحة الإذن. الكميات بالوحدة الصغرى.
    /// كل دالة كتابة فى معاملتها الخاصة، أو فى معاملة المستدعى إن مُرِّرت (للاختبار).
    /// </summary>
    public class SalesRepShiftService
    {
        public const int TransferType = (int)StoreOrderType.SarfTahweel_1;
        public const int SurplusType = 1007, ShortageType = 1008;

        private readonly operations _external;
        public SalesRepShiftService(operations external = null) { _external = external; }

        public class ShiftView
        {
            public long Id { get; set; }
            public long Rep_Id { get; set; }
            public DateTime Shift_Date { get; set; }
            public int Status { get; set; }
            public DateTime? Opened_At { get; set; }
            public DateTime? Closed_At { get; set; }
            public long? Load_Order_Id { get; set; }
            public long? Unload_Order_Id { get; set; }
            public long? Damaged_Transfer_Order_Id { get; set; }
            public decimal? Stock_Diff_Value { get; set; }
        }

        /// <summary>سطر رصيد فى مخزن السيارة أو التالف.</summary>
        public class StockLine
        {
            public long Unit_Item_Id { get; set; }
            public long Item_Id { get; set; }
            public string Item_Name { get; set; } = "";
            public string Code { get; set; } = "";
            public string B_Unit { get; set; } = "";
            public string S_Unit { get; set; } = "";
            public decimal Factor { get; set; }
            public decimal Balance_Small { get; set; }
        }

        /// <summary>تحويل يخص المندوب (تحميل/تفريغ/تالف) مع حالته.</summary>
        public class TransferView
        {
            public long Id { get; set; }
            public long Order_No { get; set; }
            public DateTime Order_Date { get; set; }
            public long Id_From { get; set; }
            public string From_Name { get; set; } = "";
            public long Id_To { get; set; }
            public string To_Name { get; set; } = "";
            public int Doc_Status { get; set; }
            public int? Transfer_Status { get; set; }
            public int Lines { get; set; }
        }

        public class CountInput
        {
            public long Unit_Item_Id { get; set; }
            public decimal Counted_Small { get; set; }
        }

        // ============================== قراءة ==============================

        /// <summary>الوردية غير المقفلة للمندوب (أو null).</summary>
        public ShiftView GetCurrentShift(long repId) => Read(opp => Current(opp, repId));

        public List<ShiftView> GetShifts(long repId, int take = 30) => Read(opp => opp.db.SalesRep_Shifts.AsNoTracking()
            .Where(s => s.Rep_Id == repId).OrderByDescending(s => s.Opened_At).Take(take).Select(View).ToList());

        /// <summary>أرصدة مخزن بالوحدة الصغرى (المخزَّن، والمحرك يبقيه مطابقاً للدفتر).</summary>
        public List<StockLine> GetStock(long storeId, bool withBalanceOnly = true) => Read(opp => Stock(opp, storeId, withBalanceOnly));

        public List<SalesRep_Shift_Stock> GetShiftStock(long shiftId) => Read(opp => opp.db.SalesRep_Shift_Stock.AsNoTracking()
            .Where(s => s.Shift_Id == shiftId).ToList());

        /// <summary>تحويلات المندوب: من/إلى مخزن السيارة أو التالف، مع ما زال «فى الطريق».</summary>
        public List<TransferView> GetTransfers(long repId, int take = 30) => Read(opp =>
        {
            var rep = Rep(opp, repId);
            return opp.db.Database.SqlQuery<TransferView>($@"
SELECT TOP ({take}) o.Id, ISNULL(o.Order_Purshing_No, 0) AS Order_No, ISNULL(o.Order_date, o.OperationDate) AS Order_Date,
       o.Id_From, ISNULL(sf.Store_Name, N'') AS From_Name, o.Id_To, ISNULL(st.Store_Name, N'') AS To_Name,
       ISNULL(o.Doc_Status, 1) AS Doc_Status, o.Transfer_Status,
       (SELECT COUNT(*) FROM Detailes_Purshing d WHERE d.OrderTable_id = o.Id) AS Lines
FROM Order_Purshing o
LEFT JOIN Store sf ON sf.Id = o.Id_From
LEFT JOIN Store st ON st.Id = o.Id_To
WHERE o.Order_Type = {TransferType} AND o.Reversal_Of_Id IS NULL AND ISNULL(o.Doc_Status, 1) IN (0, 1)
  AND (o.Id_From IN ({rep.Van_Store_Id}, {rep.Damaged_Store_Id}) OR o.Id_To IN ({rep.Van_Store_Id}, {rep.Damaged_Store_Id}))
ORDER BY o.Id DESC").ToList();
        });

        /// <summary>
        /// اقتراح التحميل (§6.1-2): لكل صنف باعه المندوب من السيارة فى آخر N يوماً = متوسط المنصرف اليومى × أيام التغطية − رصيد السيارة.
        /// طلبيات العملاء المجدولة مع المندوب تُضاف فى المرحلة 5.
        /// </summary>
        public List<(long Item_Id, decimal Suggested_Small)> SuggestLoad(long repId, int lookBackDays = 14, int coverDays = 1) => Read(opp =>
        {
            var rep = Rep(opp, repId);
            DateTime from = DateTime.Today.AddDays(-lookBackDays);
            var rows = opp.db.Database.SqlQuery<SuggestRow>($@"
SELECT u.Item_id AS Item_Id,
       CAST(ISNULL(SUM(CASE WHEN x.Engine_Posted = 1 AND x.Qty_Small < 0 AND ISNULL(o.Order_Type, 0) <> {TransferType} THEN -x.Qty_Small ELSE 0 END), 0) AS decimal(18,4)) AS Out_Small,
       CAST(MAX(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0)) AS decimal(18,4)) AS Balance_Small
FROM Unit_item u
LEFT JOIN Detailes_Purshing_Import_Export x ON x.Unit_item_Table_Id = u.id AND x.Process_Date >= {from}
LEFT JOIN Order_Purshing o ON o.Id = x.OrderTable_id
WHERE u.Store_id = {rep.Van_Store_Id} AND ISNULL(u.Store_Type, 4) = 4
GROUP BY u.Item_id").ToList();
            return rows.Select(r => (r.Item_Id, Math.Max(0, Math.Ceiling(r.Out_Small / lookBackDays * coverDays) - r.Balance_Small)))
                       .Where(r => r.Item2 > 0).ToList();
        });

        public class SuggestRow { public long Item_Id { get; set; } public decimal Out_Small { get; set; } public decimal Balance_Small { get; set; } }

        // ============================== الوردية ==============================

        /// <summary>فتح وردية يدوياً (عهدة من أمس أو بلا تحميل). يرفض لو هناك وردية غير مقفلة.</summary>
        public long OpenShift(long repId, string notes = null) => Tx(opp => Open(opp, Rep(opp, repId), null, notes));

        /// <summary>
        /// إصدار إذن تحميل (1001: المخزن الرئيسى ← السيارة) ويُرحَّل حسب طريقة التحويل. التحويل المباشر يفتح الوردية فوراً
        /// (أو يُضاف لوردية مفتوحة كتحميل إضافى)؛ بتأكيد الاستلام تبقى «فى الطريق» حتى ConfirmLoad. يرجع رقم الإذن.
        /// </summary>
        public long IssueLoad(long repId, long sourceStoreId, IReadOnlyList<StoreOrderService.OrderLineInput> lines, bool directRequested = false, string notes = null) => Tx(opp =>
        {
            var rep = ActiveRep(opp, repId);
            if (sourceStoreId == rep.Van_Store_Id || sourceStoreId == rep.Damaged_Store_Id)
                throw new InvalidOperationException("مصدر التحميل يجب أن يكون مخزناً غير مخزنى المندوب.");
            var current = Current(opp, repId);
            if (current != null && current.Status != (int)SalesRepShiftStatus.Open && current.Status != (int)SalesRepShiftStatus.Reopened)
                throw new InvalidOperationException("الوردية الحالية قيد الإقفال؛ لا تحميل حتى تُقفل.");

            long orderId = Transfer(opp, sourceStoreId, rep.Van_Store_Id, lines, directRequested, notes ?? "تحميل سيارة المندوب");
            if (IsInTransit(opp, orderId)) return orderId;   // الوردية تُفتح عند تأكيد المندوب

            long shiftId = current?.Id ?? Open(opp, rep, orderId, null);
            if (current != null) Link(opp, shiftId, SalesRepDocType.StoreOrder, orderId, rep.Branch_Id);
            return orderId;
        });

        /// <summary>تأكيد المندوب استلام التحميل (كلياً أو جزئياً)؛ يفتح الوردية إن لم تكن مفتوحة (م-06-2). يرجع رقم الوردية.</summary>
        public long ConfirmLoad(long repId, long loadOrderId, IReadOnlyList<StoreOrderService.ReceiptLineInput> lines) => Tx(opp =>
        {
            var rep = ActiveRep(opp, repId);
            var ship = opp.db.Order_Purshings.AsNoTracking().FirstOrDefault(o => o.Id == loadOrderId) ?? throw new InvalidOperationException("إذن التحميل غير موجود.");
            if (ship.Order_Type != TransferType || ship.Id_To != rep.Van_Store_Id) throw new InvalidOperationException("الإذن ليس تحميلاً لسيارة هذا المندوب.");
            long receiptId = new StoreOrderService(opp).ReceiveTransfer(loadOrderId, DateTime.Today, lines, "تأكيد استلام المندوب");

            var current = Current(opp, repId);
            if (current == null) return Open(opp, rep, loadOrderId, null, receiptId);
            if (current.Status == (int)SalesRepShiftStatus.Closing) throw new InvalidOperationException("الوردية الحالية قيد الإقفال.");
            if (!Linked(opp, SalesRepDocType.StoreOrder, loadOrderId)) Link(opp, current.Id, SalesRepDocType.StoreOrder, loadOrderId, rep.Branch_Id);
            Link(opp, current.Id, SalesRepDocType.StoreOrder, receiptId, rep.Branch_Id);
            return current.Id;
        });

        /// <summary>تفريغ (1001 عكسى: السيارة ← المخزن الرئيسى)، وأمين المخزن يؤكد الاستلام من صفحة الأذون.</summary>
        public long Unload(long shiftId, long destStoreId, IReadOnlyList<StoreOrderService.OrderLineInput> lines, bool directRequested = false) => Tx(opp =>
        {
            var (shift, rep) = OpenShiftAndRep(opp, shiftId, allowClosing: true);
            if (destStoreId == rep.Van_Store_Id || destStoreId == rep.Damaged_Store_Id) throw new InvalidOperationException("وجهة التفريغ يجب أن تكون مخزناً غير مخزنى المندوب.");
            long orderId = Transfer(opp, rep.Van_Store_Id, destStoreId, lines, directRequested, "تفريغ سيارة المندوب");
            Link(opp, shift.Id, SalesRepDocType.StoreOrder, orderId, rep.Branch_Id);
            if (shift.Unload_Order_Id == null) { shift.Unload_Order_Id = orderId; shift.StampEdit(); opp.db.SaveChanges(); }
            return orderId;
        });

        /// <summary>نقل كل رصيد مخزن تالف السيارة إلى المخزن المركزى للتالف (1001)، والإتلاف لاحقاً بإذن 1013 (§6.7-3).</summary>
        public long TransferDamaged(long shiftId, long centralDamagedStoreId, bool directRequested = false) => Tx(opp =>
        {
            var (shift, rep) = OpenShiftAndRep(opp, shiftId, allowClosing: true);
            if (centralDamagedStoreId == rep.Van_Store_Id || centralDamagedStoreId == rep.Damaged_Store_Id) throw new InvalidOperationException("اختر المخزن المركزى للتالف.");
            var lines = Stock(opp, rep.Damaged_Store_Id, true)
                .Select(s => new StoreOrderService.OrderLineInput(s.Unit_Item_Id, Math.Floor(s.Balance_Small / s.Factor), s.Balance_Small % s.Factor)).ToList();
            if (lines.Count == 0) throw new InvalidOperationException("مخزن التالف فارغ.");
            long orderId = Transfer(opp, rep.Damaged_Store_Id, centralDamagedStoreId, lines, directRequested, "نقل تالف سيارة المندوب");
            Link(opp, shift.Id, SalesRepDocType.StoreOrder, orderId, rep.Branch_Id);
            shift.Damaged_Transfer_Order_Id ??= orderId;
            shift.StampEdit();
            opp.db.SaveChanges();
            return orderId;
        });

        /// <summary>
        /// بدء الإقفال (§6.7-2): يحسب لكل صنف فى السيارة الافتتاحى والمحمَّل والمفرَّغ والمتوقع من دفتر المحرك، ويحوّل الوردية
        /// إلى «قيد الإقفال» فلا يُقبل عليها تحميل جديد. يُعاد استدعاؤه لإعادة الحساب قبل الإقفال النهائى.
        /// </summary>
        public List<SalesRep_Shift_Stock> StartClosing(long shiftId) => Tx(opp =>
        {
            var (shift, rep) = OpenShiftAndRep(opp, shiftId, allowClosing: true);
            if (InTransitFor(opp, rep.Van_Store_Id, toVan: true))
                throw new InvalidOperationException("يوجد تحميل للسيارة بانتظار تأكيد المندوب؛ أكّده أو أغلقه بعجز أولاً.");
            var rows = BuildStock(opp, shift, rep);
            shift.Status = (int)SalesRepShiftStatus.Closing;
            shift.StampEdit();
            opp.db.SaveChanges();
            return rows;
        });

        /// <summary>تسجيل الجرد الفعلى (بالوحدة الصغرى) لأصناف السيارة.</summary>
        public void RecordCount(long shiftId, IReadOnlyList<CountInput> counts) => Tx(opp =>
        {
            var shift = opp.db.SalesRep_Shifts.FirstOrDefault(s => s.Id == shiftId) ?? throw new InvalidOperationException("الوردية غير موجودة.");
            if (shift.Status != (int)SalesRepShiftStatus.Closing) throw new InvalidOperationException("ابدأ الإقفال أولاً.");
            var rows = opp.db.SalesRep_Shift_Stock.Where(s => s.Shift_Id == shiftId).ToDictionary(s => s.Unit_Item_Table_Id);
            foreach (var c in counts)
            {
                if (c.Counted_Small < 0) throw new InvalidOperationException("الكمية المعدودة لا تكون سالبة.");
                if (!rows.TryGetValue(c.Unit_Item_Id, out var row)) throw new InvalidOperationException("صنف ليس ضمن جرد هذه الوردية.");
                row.Counted_Closing = c.Counted_Small;
                row.Diff = c.Counted_Small - row.Expected_Closing;
                row.StampEdit();
            }
            opp.db.SaveChanges();
            return 0;
        });

        /// <summary>
        /// الإقفال النهائى: كل الأصناف يجب أن تُجرد؛ الفروق تولد إذن تسوية بالعجز 1008 وإذن بالزيادة 1007 كمسودتين يعتمدهما المشرف
        /// (§6.7-2)، وقيمة العجز بلقطة متوسط تكلفة الفرع. بسياسة «تفريغ» (ق9) يُشترط وجود إذن تفريغ.
        /// </summary>
        public void CloseShift(long shiftId, string notes = null) => Tx(opp =>
        {
            var (shift, rep) = OpenShiftAndRep(opp, shiftId, allowClosing: true);
            if (shift.Status != (int)SalesRepShiftStatus.Closing) throw new InvalidOperationException("ابدأ الإقفال وسجّل الجرد أولاً.");
            if (rep.End_Of_Day_Stock_Policy == (int)SalesRepEndOfDayStockPolicy.Unload && shift.Unload_Order_Id == null)
                throw new InvalidOperationException("سياسة المندوب: تفريغ المتبقى آخر اليوم — أصدر إذن التفريغ قبل الإقفال.");
            if (InTransitFor(opp, rep.Van_Store_Id, toVan: true))
                throw new InvalidOperationException("يوجد تحميل للسيارة بانتظار تأكيد المندوب.");

            // إعادة الحساب لحظة الإقفال: أى حركة بعد بدء الإقفال تظهر فى المتوقع، والمعدود يبقى كما سُجِّل.
            var counted = opp.db.SalesRep_Shift_Stock.AsNoTracking().Where(s => s.Shift_Id == shiftId).ToDictionary(s => s.Unit_Item_Table_Id, s => s.Counted_Closing);
            var rows = BuildStock(opp, shift, rep);
            foreach (var r in rows)
            {
                if (!counted.TryGetValue(r.Unit_Item_Table_Id, out var c) || c == null)
                    throw new InvalidOperationException("لم تُجرد كل أصناف السيارة.");
                r.Counted_Closing = c;
                r.Diff = c.Value - r.Expected_Closing;
            }
            opp.db.SaveChanges();

            var orders = new StoreOrderService(opp);
            var shortage = rows.Where(r => r.Diff < 0).ToList();
            var surplus = rows.Where(r => r.Diff > 0).ToList();
            long? shortageId = shortage.Count == 0 ? null : orders.SaveDraft(new StoreOrderService.OrderInput(ShortageType, DateTime.Today,
                StoreOrderService.PartyStore, rep.Van_Store_Id, 8, 0, Lines(opp, shortage), $"عجز جرد سيارة المندوب — وردية {shift.Id}"));
            long? surplusId = surplus.Count == 0 ? null : orders.SaveDraft(new StoreOrderService.OrderInput(SurplusType, DateTime.Today,
                7, 0, StoreOrderService.PartyStore, rep.Van_Store_Id, Lines(opp, surplus), $"زيادة جرد سيارة المندوب — وردية {shift.Id}"));
            foreach (var r in shortage) r.Diff_Order_Id = shortageId;
            foreach (var r in surplus) r.Diff_Order_Id = surplusId;
            if (shortageId != null) Link(opp, shift.Id, SalesRepDocType.StoreOrder, shortageId.Value, rep.Branch_Id);
            if (surplusId != null) Link(opp, shift.Id, SalesRepDocType.StoreOrder, surplusId.Value, rep.Branch_Id);

            shift.Stock_Diff_Value = Math.Round(rows.Sum(r => (r.Diff ?? 0) * (r.Unit_Cost ?? 0)), 2);
            shift.Status = (int)SalesRepShiftStatus.Closed;
            shift.Closed_At = DateTime.Now;
            shift.Closed_By = Common.User_Id;
            if (!string.IsNullOrWhiteSpace(notes)) shift.Notes = notes.Trim();
            shift.StampEdit();
            opp.db.SaveChanges();
            return 0;
        });

        // ============================== داخلى ==============================

        private static ShiftView View(SalesRep_Shift s) => new ShiftView
        {
            Id = s.Id, Rep_Id = s.Rep_Id, Shift_Date = s.Shift_Date, Status = s.Status, Opened_At = s.Opened_At, Closed_At = s.Closed_At,
            Load_Order_Id = s.Load_Order_Id, Unload_Order_Id = s.Unload_Order_Id, Damaged_Transfer_Order_Id = s.Damaged_Transfer_Order_Id, Stock_Diff_Value = s.Stock_Diff_Value,
        };

        private static ShiftView Current(operations opp, long repId)
        {
            var s = opp.db.SalesRep_Shifts.AsNoTracking().FirstOrDefault(x => x.Rep_Id == repId && x.Status != (int)SalesRepShiftStatus.Closed);
            return s == null ? null : View(s);
        }

        private static SalesRep_Rep Rep(operations opp, long repId) =>
            opp.db.SalesRep_Reps.AsNoTracking().FirstOrDefault(r => r.Id == repId) ?? throw new InvalidOperationException("ملف المندوب غير موجود.");

        private static SalesRep_Rep ActiveRep(operations opp, long repId)
        {
            var rep = Rep(opp, repId);
            if (!rep.Active) throw new InvalidOperationException("ملف المندوب موقوف.");
            return rep;
        }

        private static (SalesRep_Shift, SalesRep_Rep) OpenShiftAndRep(operations opp, long shiftId, bool allowClosing)
        {
            var shift = opp.db.SalesRep_Shifts.FirstOrDefault(s => s.Id == shiftId) ?? throw new InvalidOperationException("الوردية غير موجودة.");
            bool ok = shift.Status == (int)SalesRepShiftStatus.Open || shift.Status == (int)SalesRepShiftStatus.Reopened
                      || (allowClosing && shift.Status == (int)SalesRepShiftStatus.Closing);
            if (!ok) throw new InvalidOperationException("الوردية مقفلة.");
            return (shift, Rep(opp, shift.Rep_Id));
        }

        private long Open(operations opp, SalesRep_Rep rep, long? loadOrderId, string notes, long? receiptId = null)
        {
            if (!rep.Active) throw new InvalidOperationException("ملف المندوب موقوف.");
            if (Current(opp, rep.Id) != null) throw new InvalidOperationException("للمندوب وردية غير مقفلة؛ أقفلها أولاً.");
            DateTime now = DateTime.Now;
            var shift = new SalesRep_Shift
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Rep_Id = rep.Id,
                Shift_Date = now.Date,
                Status = (int)SalesRepShiftStatus.Open,
                Opened_At = now,
                Load_Order_Id = loadOrderId,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            };
            shift.StampInsert(rep.Branch_Id ?? Common.Branch_Id);
            opp.db.SalesRep_Shifts.Add(shift);
            opp.db.SaveChanges();
            if (loadOrderId != null) Link(opp, shift.Id, SalesRepDocType.StoreOrder, loadOrderId.Value, rep.Branch_Id);
            if (receiptId != null) Link(opp, shift.Id, SalesRepDocType.StoreOrder, receiptId.Value, rep.Branch_Id);

            // الرصيد الافتتاحى للوردية (§6.1): رصيد السيارة قبل التحميل الذى فتحها؛ ذلك التحميل يظهر فى «المحمَّل».
            var engine = new StockMovementService(opp);
            var lineIds = opp.db.Unit_item.AsNoTracking().Where(u => u.Store_id == rep.Van_Store_Id && (u.Store_Type ?? 4) == 4).Select(u => u.id).ToList();
            var opened = MovesBy(opp, ShiftOrders(opp, shift.Id), lineIds);
            foreach (long id in lineIds)
            {
                decimal bal = engine.GetComputedBalanceSmall(id);
                opened.TryGetValue(id, out var m);
                if (bal == 0 && m == null) continue;
                decimal loaded = m?.In_Qty ?? 0;
                var row = new SalesRep_Shift_Stock { Id = CallSnowflakeIdGenerate.GenerateNewId(), Shift_Id = shift.Id, Unit_Item_Table_Id = id, Opening = bal - loaded, Loaded = loaded, Expected_Closing = bal };
                row.StampInsert(shift.Branch_Id ?? 0);
                opp.db.SalesRep_Shift_Stock.Add(row);
            }
            opp.db.SaveChanges();
            return shift.Id;
        }

        private static long Transfer(operations opp, long fromStoreId, long toStoreId, IReadOnlyList<StoreOrderService.OrderLineInput> lines, bool directRequested, string notes)
        {
            var orders = new StoreOrderService(opp);
            long id = orders.SaveDraft(new StoreOrderService.OrderInput(TransferType, DateTime.Today,
                StoreOrderService.PartyStore, fromStoreId, StoreOrderService.PartyStore, toStoreId, lines, notes));
            orders.Post(id, StoreTransferPolicy.Effective(opp, Common.User_Id, directRequested));
            return id;
        }

        private static bool IsInTransit(operations opp, long orderId) =>
            new StoreOrderService(opp).GetTransferLink(orderId)?.Status is (int)StoreOrderService.TransferStatus.InTransit or (int)StoreOrderService.TransferStatus.PartiallyReceived;

        private static bool InTransitFor(operations opp, long storeId, bool toVan) => opp.db.Database.SqlQuery<int>($@"
SELECT COUNT(*) AS Value FROM Order_Purshing
WHERE Order_Type = {TransferType} AND ISNULL(Doc_Status, 1) = 1 AND Transfer_Status IN (1, 2) AND {(toVan ? 1 : 0)} = 1 AND Id_To = {storeId}").AsEnumerable().First() > 0;

        private static bool Linked(operations opp, SalesRepDocType type, long docId) =>
            opp.db.SalesRep_Document_Links.Any(l => l.Doc_Type == (int)type && l.Doc_Id == docId);

        private static void Link(operations opp, long shiftId, SalesRepDocType type, long docId, long? branchId)
        {
            if (Linked(opp, type, docId)) return;
            var link = new SalesRep_Document_Link { Id = CallSnowflakeIdGenerate.GenerateNewId(), Doc_Type = (int)type, Doc_Id = docId, Shift_Id = shiftId };
            link.StampInsert(branchId ?? Common.Branch_Id);
            opp.db.SalesRep_Document_Links.Add(link);
            opp.db.SaveChanges();
        }

        public class MoveRow { public long Unit_Item_Id { get; set; } public decimal In_Qty { get; set; } public decimal Out_Qty { get; set; } }

        /// <summary>وارد ومنصرف أسطر بعينها من مستندات بعينها (بالوحدة الصغرى) من دفتر المحرك — بالمعرّف لا بالوقت.</summary>
        private static Dictionary<long, MoveRow> MovesBy(operations opp, IEnumerable<long> orderIds, IEnumerable<long> unitItemIds)
        {
            var o = orderIds.Distinct().ToList();
            var u = unitItemIds.Distinct().ToList();
            if (o.Count == 0 || u.Count == 0) return new Dictionary<long, MoveRow>();
            return opp.db.Database.SqlQueryRaw<MoveRow>($@"
SELECT Unit_item_Table_Id AS Unit_Item_Id,
       CAST(SUM(CASE WHEN Qty_Small > 0 THEN Qty_Small ELSE 0 END) AS decimal(18,4)) AS In_Qty,
       CAST(SUM(CASE WHEN Qty_Small < 0 THEN -Qty_Small ELSE 0 END) AS decimal(18,4)) AS Out_Qty
FROM Detailes_Purshing_Import_Export
WHERE Engine_Posted = 1 AND OrderTable_id IN ({string.Join(",", o)}) AND Unit_item_Table_Id IN ({string.Join(",", u)})
GROUP BY Unit_item_Table_Id").ToList().ToDictionary(r => r.Unit_Item_Id);
        }

        private static List<long> ShiftOrders(operations opp, long shiftId) => opp.db.SalesRep_Document_Links.AsNoTracking()
            .Where(l => l.Shift_Id == shiftId && l.Doc_Type == (int)SalesRepDocType.StoreOrder).Select(l => l.Doc_Id).ToList();

        /// <summary>
        /// يعيد بناء صفوف المطابقة. الافتتاحى = لقطة الفتح (رصيد السيارة قبل التحميل الذى فتح الوردية). المحمَّل = وارد السيارة من
        /// أذون الوردية، والمفرَّغ = منصرفها منها (بالمعرّف). المتوقع = الرصيد المحسوب الآن من الدفتر. «أخرى» = الفرق الباقى
        /// (حركات من خارج الوردية؛ وتصير «مباع/مرتجع» عند ربط فواتير المرحلة 3) فالمعادلة تتوازن دائماً.
        /// </summary>
        private static List<SalesRep_Shift_Stock> BuildStock(operations opp, SalesRep_Shift shift, SalesRep_Rep rep)
        {
            var existing = opp.db.SalesRep_Shift_Stock.Where(s => s.Shift_Id == shift.Id).ToDictionary(s => s.Unit_Item_Table_Id);
            var lines = opp.db.Unit_item.AsNoTracking().Where(u => u.Store_id == rep.Van_Store_Id && (u.Store_Type ?? 4) == 4)
                .Select(u => new { u.id, u.Item_Id }).ToList();
            var moves = MovesBy(opp, ShiftOrders(opp, shift.Id), lines.Select(l => l.id));
            var engine = new StockMovementService(opp);
            long branchId = rep.Branch_Id ?? Common.Branch_Id;
            var result = new List<SalesRep_Shift_Stock>();
            foreach (var l in lines)
            {
                decimal expected = engine.GetComputedBalanceSmall(l.id);
                moves.TryGetValue(l.id, out var m);
                existing.TryGetValue(l.id, out var row);
                if (row == null)
                {
                    if (expected == 0 && m == null) continue;
                    row = new SalesRep_Shift_Stock { Id = CallSnowflakeIdGenerate.GenerateNewId(), Shift_Id = shift.Id, Unit_Item_Table_Id = l.id };
                    row.StampInsert(branchId);
                    opp.db.SalesRep_Shift_Stock.Add(row);
                }
                else row.StampEdit();
                row.Loaded = m?.In_Qty ?? 0;
                row.Unloaded = m?.Out_Qty ?? 0;
                row.Expected_Closing = expected;
                row.Other_Movement = expected - row.Opening - row.Loaded + row.Unloaded + row.Sold + row.Delivered - row.Returned_Good;
                row.Unit_Cost = opp.db.Item_Branch_Costs.AsNoTracking().Where(c => c.Item_Id == l.Item_Id && c.Branch_Id == branchId).Select(c => (decimal?)c.Avg_Cost).FirstOrDefault() ?? 0;
                if (row.Counted_Closing != null) row.Diff = row.Counted_Closing - expected;
                result.Add(row);
            }
            opp.db.SaveChanges();
            return result;
        }

        private static List<StoreOrderService.OrderLineInput> Lines(operations opp, List<SalesRep_Shift_Stock> rows)
        {
            var ids = rows.Select(r => r.Unit_Item_Table_Id).ToList();
            var factors = opp.db.Unit_item.AsNoTracking().Where(u => ids.Contains(u.id))
                .ToDictionary(u => u.id, u => (u.Small_unit ?? 0) <= 0 ? 1m : u.Small_unit.Value);
            return rows.Select(r =>
            {
                decimal qty = Math.Abs(r.Diff ?? 0), f = factors[r.Unit_Item_Table_Id];
                return new StoreOrderService.OrderLineInput(r.Unit_Item_Table_Id, Math.Floor(qty / f), qty % f);
            }).ToList();
        }

        private static List<StockLine> Stock(operations opp, long storeId, bool withBalanceOnly) => opp.db.Database.SqlQuery<StockLine>($@"
SELECT u.id AS Unit_Item_Id, u.Item_id AS Item_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(u.Code, ISNULL(i.code, N'')) AS Code,
       ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(ISNULL(u.Full_qua, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(u.D_Qua, 0) AS decimal(18,4)) AS Balance_Small
FROM Unit_item u
JOIN Item i ON i.id = u.Item_id
LEFT JOIN unit un ON un.id = u.unit_id
WHERE u.Store_id = {storeId} AND ISNULL(u.Store_Type, 4) = 4
  AND ({(withBalanceOnly ? 1 : 0)} = 0 OR ISNULL(u.Full_qua, 0) <> 0 OR ISNULL(u.D_Qua, 0) <> 0)
ORDER BY i.Item_Name").ToList();

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
