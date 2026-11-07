using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using static AccountModelsLib.StockMovementService;

namespace AccountModelsLib
{
    /// <summary>
    /// أذون المخزن (خطة 03 — المرحلة 1، الخدمة فقط): مسودة ← ترحيل ← عكس، فوق StockMovementService.
    /// قواعد كل نوع (الأطراف، الاتجاه، مصدر السعر، تحديث المتوسط) تُقرأ من Store_Order_Type_Settings بدل الكود.
    /// المسودة = Order_Purshing بـ Doc_Status = 0 وأسطرها فى Detailes_Purshing بلا أى أثر على الرصيد؛ الترحيل وحده يحرّك المخزون.
    /// العكس = رأس إذن عكسى بلا أسطر مرتبط بالأصلى + صفوف دفتر عكسية؛ فالقراءات القديمة تُفلتر بـ ISNULL(Doc_Status, 1) = 1.
    /// كل دالة تعمل داخل معاملة المستدعى (operations بعد BeginTransaction).
    /// </summary>
    public class StoreOrderService
    {
        /// <summary>رموز الأطراف كما فى connectionit\UsrCntrl\StoreOrders.cs.</summary>
        public const int PartyStore = 4, PartyDepartment = 20;

        /// <summary>طريقة التحويل بين المخازن (خطة 01 §2.7): تحددها الشاشة من الإعداد والصلاحية.</summary>
        public enum TransferMode { Direct = 1, WithReceiptConfirmation = 2 }

        public record OrderLineInput(long UnitItemId, decimal QtyLarge, decimal QtySmall, decimal ItemNo = 0, decimal? UnitCostLarge = null, string Notes = null);

        public record OrderInput(int OrderType, DateTime OrderDate, int TypeFrom, long IdFrom, int TypeTo, long IdTo,
                                 IReadOnlyList<OrderLineInput> Lines, string Notes = null, long InvoiceId = 0, long CostCenterId = 0);

        /// <summary>صف Store_Order_Type_Settings (عام لأن EF يملؤه من SQL خام).</summary>
        public class OrderTypeSettings
        {
            public int Order_Type { get; set; }
            public string Name_Ar { get; set; }
            public int Direction { get; set; }
            public int Store_Side { get; set; }
            public string From_Party_Types { get; set; }
            public int Default_From_Party { get; set; }
            public string To_Party_Types { get; set; }
            public int Default_To_Party { get; set; }
            public bool Is_Virtual_Outer_From { get; set; }
            public bool Is_Virtual_Outer_To { get; set; }
            public int Price_Source { get; set; }
            public bool Updates_Avg_Cost { get; set; }
            public bool Is_Purchase { get; set; }
            public bool Is_Transfer { get; set; }
            public bool Is_Active { get; set; }

            public int[] FromParties => ParseList(From_Party_Types);
            public int[] ToParties => ParseList(To_Party_Types);
            /// <summary>1 = جهة «من» هى المخزن، 2 = جهة «إلى» هى المخزن (lookUpStoreId القديم).</summary>
            public bool StoreIsFrom => Store_Side == 1;
            public bool IsManualPrice => Price_Source == 3;

            private static int[] ParseList(string s) =>
                (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(int.Parse).ToArray();
        }

        private readonly operations opp;
        private readonly StockMovementService engine;

        public StoreOrderService(operations opp_)
        {
            opp = opp_ ?? throw new ArgumentNullException(nameof(opp_));
            engine = new StockMovementService(opp);
        }

        // ============================== الإعداد ==============================

        public OrderTypeSettings GetSettings(int orderType)
        {
            var s = opp.db.Database.SqlQuery<OrderTypeSettings>($@"
SELECT Order_Type, Name_Ar, Direction, Store_Side, From_Party_Types, Default_From_Party, To_Party_Types, Default_To_Party,
       Is_Virtual_Outer_From, Is_Virtual_Outer_To, Price_Source, Updates_Avg_Cost, Is_Purchase, Is_Transfer, Is_Active
FROM Store_Order_Type_Settings WHERE Order_Type = {orderType}").AsEnumerable().FirstOrDefault();
            if (s == null) throw new InvalidOperationException($"نوع الإذن {orderType} غير معرَّف فى إعداد أنواع الأذون.");
            return s;
        }

        // ============================== المسودة ==============================

        /// <summary>
        /// ينشئ مسودة (draftId = 0) أو يستبدل رأس وأسطر مسودة قائمة. لا يمس الرصيد. يرجع رقم الإذن (Id).
        /// الرقم المسلسل يُحجز عند إنشاء المسودة (كالنظام الحالى).
        /// </summary>
        public long SaveDraft(OrderInput input, long draftId = 0)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var settings = GetSettings(input.OrderType);
            var stockLines = Validate(settings, input);

            DateTime now = DateTime.Now;
            long userId = Common.User_Id;
            Order_Purshing header;

            if (draftId > 0)
            {
                if (GetStatus(draftId) != (int)DocStatus.Draft)
                    throw new InvalidOperationException("لا يُعدَّل إلا الإذن المسودة؛ الإذن المرحَّل يُعكس.");
                header = opp.db.Order_Purshings.First(o => o.Id == draftId);
                if (header.Order_Type != input.OrderType)
                    throw new InvalidOperationException("لا يتغير نوع الإذن بعد إنشائه.");
                FillHeader(header, settings, input);
                header.Update_ServerDateTime = now;
                header.EditOperationDate = now;
                header.Edit_User_Id = userId;
                opp.db.Detailes_Purshings.RemoveRange(opp.db.Detailes_Purshings.Where(d => d.OrderTable_id == draftId));
                opp.db.SaveChanges();
            }
            else
            {
                header = new Order_Purshing
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(),
                    Order_Type = input.OrderType,
                    Order_Purshing_No = NextOrderNo(input.OrderType),
                    IsActiveMainOrder_Purshing = 1,
                    Branch_ID = Common.Branch_Id,
                    User_ID = userId,
                    OperationDate = now.Date,
                    Insert_ServerDateTime = now,
                };
                FillHeader(header, settings, input);
                opp.db.Order_Purshings.Add(header);
                opp.db.SaveChanges();
                opp.db.Database.ExecuteSqlInterpolated($"UPDATE Order_Purshing SET Doc_Status = {(int)DocStatus.Draft} WHERE Id = {header.Id}");
            }
            // مركز تكلفة الإذن (ق9): يُرحَّل على أسطر قيد الإذن عند الترحيل (InventoryPostingService.PostOrderEntry)
            BusinessLibShared.CostCenters.InvoiceCostCenter.SetHeaderCenter(opp, BusinessLibShared.CostCenters.InvoiceCostCenter.Table_StoreOrder, header.Id, input.CostCenterId);

            for (int i = 0; i < input.Lines.Count; i++)
            {
                var line = input.Lines[i];
                var um = stockLines[i];
                opp.db.Detailes_Purshings.Add(new Detailes_Purshing
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(),
                    OrderTable_id = header.Id,
                    Item_Id = um.Item_Id ?? 0,
                    Unit_item_Table_Id = um.id,
                    StoreId = um.Store_id,
                    Code = um.Code,
                    Unit_Id = um.unit_id,
                    qua = line.QtyLarge,
                    Part_qua = line.QtySmall,
                    Item_No = line.ItemNo,
                    type_From = input.TypeFrom,
                    Id_From = input.IdFrom,
                    Type_To = input.TypeTo,
                    Id_To = input.IdTo,
                    Transaction_Type = settings.Direction == 1 ? 1 : 2,   // من جهة المخزن: 1 وارد، 2 منصرف (التحويل منصرف من المصدر)
                    ProcessNameId = input.OrderType,
                    PriceUnit = line.UnitCostLarge,
                    Notes = line.Notes,
                    OperationDate = now.Date,
                    User_Id = userId,
                    Branch_Id = Common.Branch_Id,
                    Insert_ServerDateTime = now,
                });
            }
            opp.db.SaveChanges();

            Log(draftId > 0 ? 2 : 1, header, "مسودة إذن");
            return header.Id;
        }

        /// <summary>يلغى مسودة (Doc_Status = 3) مع إبقاء أسطرها للمراجعة؛ لا أثر على الرصيد.</summary>
        public void CancelDraft(long orderId)
        {
            if (GetStatus(orderId) != (int)DocStatus.Draft)
                throw new InvalidOperationException("لا تُلغى إلا المسودة؛ الإذن المرحَّل يُعكس.");
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Order_Purshing SET Doc_Status = {(int)DocStatus.CancelledDraft},
    Update_ServerDateTime = {DateTime.Now}, Edit_User_Id = {Common.User_Id} WHERE Id = {orderId}");
        }

        // ============================== الترحيل ==============================

        /// <summary>
        /// يرحّل مسودة: يعيد التحقق ثم يحرّك المخزون عبر المحرك. التحويل (1001/1006) يحتاج transferMode
        /// (تحددها الصفحة من StoreTransferPolicy): «مباشر» يحرك المخزنين معاً، و«بتأكيد استلام» يصرف من المرسل فقط
        /// ويجعل الشحنة «فى الطريق» (Transfer_Status = 1) حتى تُستلم بـ ReceiveTransfer.
        /// </summary>
        public List<PostedLine> Post(long orderId, TransferMode? transferMode = null)
        {
            if (GetStatus(orderId) != (int)DocStatus.Draft)
                throw new InvalidOperationException("لا يُرحَّل إلا الإذن المسودة.");

            var header = opp.db.Order_Purshings.AsNoTracking().First(o => o.Id == orderId);
            var lines = opp.db.Detailes_Purshings.Where(d => d.OrderTable_id == orderId).ToList();
            var settings = GetSettings(header.Order_Type ?? 0);
            var input = ToInput(header, lines);
            var stockLines = Validate(settings, input);

            var doc = new MovementDocument(orderId, header.Order_Type ?? 0, (header.Order_date ?? DateTime.Today).Date,
                InvoiceId: header.Invoice_ID ?? 0, TypeFrom: header.type_From ?? 0, IdFrom: header.Id_From, TypeTo: header.Type_To ?? 0, IdTo: header.Id_To);

            var result = new List<PostedLine>();
            if (settings.Is_Transfer)
            {
                if (settings.Order_Type == (int)StoreOrderType.EstlamFromStore_16 || settings.Order_Type == (int)StoreOrderType.SarfToStore_17)
                    throw new NotSupportedException("إذن الاستلام من المخزن (1016) يُنشأ من «استلام التحويل» على الشحنة، ولا يُصدر يدوياً.");
                if (transferMode == null)
                    throw new InvalidOperationException("حدد طريقة التحويل (مباشر أو بتأكيد استلام).");
                if (transferMode == TransferMode.WithReceiptConfirmation)
                {
                    // الخطوة الأولى: صرف من المرسل فقط؛ البضاعة «فى الطريق» بتكلفة الصرف المسجلة فى DPIE حتى الاستلام.
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var dp = lines[i];
                        var src = stockLines[i];
                        var outLine = engine.Post(doc, new[] { new MovementLine(src.id, -1, dp.qua ?? 0, dp.Part_qua ?? 0, dp.Item_No ?? 0, DetailId: dp.Id, Notes: dp.Notes) });
                        if (outLine.Count == 0) continue;
                        SetBalances(dp, outLine[0], (src.Small_unit ?? 0) <= 0 ? 1 : src.Small_unit.Value);
                        result.AddRange(outLine);
                    }
                    opp.db.Database.ExecuteSqlInterpolated($"UPDATE Order_Purshing SET Transfer_Status = {(int)TransferStatus.InTransit} WHERE Id = {orderId}");
                    opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Order_Purshing SET Doc_Status = {(int)DocStatus.Posted},
    Posted_Date = ISNULL(Posted_Date, {DateTime.Now}), Posted_By = ISNULL(Posted_By, {Common.User_Id}) WHERE Id = {orderId}");
                    opp.db.SaveChanges();
                    PostInventoryEntry(orderId);
                    Log(1, header, "ترحيل شحنة تحويل (فى الطريق)");
                    return result;
                }

                long destStoreId = settings.StoreIsFrom ? header.Id_To : header.Id_From;
                int destType = (settings.StoreIsFrom ? header.Type_To : header.type_From) ?? PartyStore;
                for (int i = 0; i < lines.Count; i++)
                {
                    var dp = lines[i];
                    var src = stockLines[i];
                    var outLine = engine.Post(doc, new[] { new MovementLine(src.id, -1, dp.qua ?? 0, dp.Part_qua ?? 0, dp.Item_No ?? 0, DetailId: dp.Id, Notes: dp.Notes) });
                    if (outLine.Count == 0) continue;   // خدمة / بلا رصيد
                    var dest = FindOrCreateDestLine(src, destStoreId, destType);
                    decimal destFactor = (dest.Small_unit ?? 0) <= 0 ? 1 : dest.Small_unit.Value;
                    decimal srcFactor = (src.Small_unit ?? 0) <= 0 ? 1 : src.Small_unit.Value;
                    // نفس الكمية بالوحدة الصغرى وبتكلفة المنصرف؛ يحدّث متوسط فرع المستلم (لا يتغير لو نفس الفرع).
                    var inLine = engine.Post(doc, new[] { new MovementLine(dest.id, +1, 0, Math.Abs(outLine[0].QtySmallSigned) * destFactor / srcFactor, dp.Item_No ?? 0,
                        UnitCostLarge: outLine[0].UnitCostSmall * destFactor, UpdatesAvgCost: true, DetailId: dp.Id, Notes: dp.Notes) });
                    dp.To_Store_Unit_Item_Table_Id = dest.id;
                    SetBalances(dp, outLine[0], srcFactor);
                    result.AddRange(outLine);
                    result.AddRange(inLine);
                }
            }
            else
            {
                int sign = settings.Direction == 1 ? +1 : -1;
                for (int i = 0; i < lines.Count; i++)
                {
                    var dp = lines[i];
                    var um = stockLines[i];
                    var posted = engine.Post(doc, new[] { new MovementLine(um.id, sign, dp.qua ?? 0, dp.Part_qua ?? 0, dp.Item_No ?? 0,
                        UnitCostLarge: sign > 0 && settings.IsManualPrice ? dp.PriceUnit : null,
                        UpdatesAvgCost: sign > 0 && settings.Updates_Avg_Cost,
                        IsPurchase: settings.Is_Purchase, DetailId: dp.Id, Notes: dp.Notes) });
                    if (posted.Count == 0) continue;
                    SetBalances(dp, posted[0], (um.Small_unit ?? 0) <= 0 ? 1 : um.Small_unit.Value);
                    result.AddRange(posted);
                }
            }

            // الحالة (Posted) يضعها المحرك؛ وإذن كل أسطره خدمات يُعلَّم مرحَّلاً هنا.
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Order_Purshing SET Doc_Status = {(int)DocStatus.Posted},
    Posted_Date = ISNULL(Posted_Date, {DateTime.Now}), Posted_By = ISNULL(Posted_By, {Common.User_Id}) WHERE Id = {orderId}");
            opp.db.SaveChanges();
            PostInventoryEntry(orderId);
            Log(1, header, "ترحيل إذن");
            return result;
        }

        /// <summary>قيد الجرد المستمر للإذن (لا يفعل شيئاً لفرع «دورى» وهو الافتراضى)؛ يرمى استثناءً لو نقص ربط حساب فيتراجع الترحيل كله.</summary>
        private void PostInventoryEntry(long orderId) => new InventoryPostingService().PostOrderEntry(opp, orderId);

        // ============================== العكس ==============================

        /// <summary>
        /// يعكس إذناً مرحَّلاً بإذن عكسى (نفس النوع، رقم جديد، بتاريخ اليوم، بلا أسطر) مرتبط به، ويرجع رقم الإذن العكسى.
        /// يُرفض لو أدى العكس لرصيد سالب (مثل عكس استلام بيعت كميته).
        /// </summary>
        public long Reverse(long orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("سبب العكس مطلوب.");
            var orig = opp.db.Order_Purshings.AsNoTracking().First(o => o.Id == orderId);
            DateTime now = DateTime.Now;
            var link = GetTransferLink(orderId);
            // شحنة لها استلام قائم: يُعكس الاستلام أولاً (لا يُعكس مستند بُنى عليه مستند آخر — م-03-3).
            if (link.Status != null && ActiveReceiptsCount(orderId) > 0)
                throw new InvalidOperationException("لهذه الشحنة أذون استلام قائمة؛ اعكس أذون الاستلام أولاً.");

            var rev = new Order_Purshing
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Order_Type = orig.Order_Type,
                Order_Purshing_No = NextOrderNo(orig.Order_Type ?? 0),
                Order_date = now.Date,
                Transaction_Type = orig.Transaction_Type,
                type_From = orig.type_From, Id_From = orig.Id_From, Id_FromStr = orig.Id_FromStr,
                Type_To = orig.Type_To, Id_To = orig.Id_To, Id_ToStr = orig.Id_ToStr,
                Invoice_ID = orig.Invoice_ID,
                IsActiveMainOrder_Purshing = 1,
                NotesAll = $"عكس الإذن رقم {orig.Order_Purshing_No}: {reason}",
                Branch_ID = Common.Branch_Id,
                User_ID = Common.User_Id,
                OperationDate = now.Date,
                Insert_ServerDateTime = now,
            };
            opp.db.Order_Purshings.Add(rev);
            opp.db.SaveChanges();

            var doc = new MovementDocument(rev.Id, orig.Order_Type ?? 0, now.Date, InvoiceId: orig.Invoice_ID ?? 0,
                TypeFrom: orig.type_From ?? 0, IdFrom: orig.Id_From, TypeTo: orig.Type_To ?? 0, IdTo: orig.Id_To);
            engine.Reverse(orderId, doc, reason);   // يتحقق أن الأصل مرحَّل، ويضع الحالتين والربط

            // عكس إذن استلام تحويل: الشحنة تعود «فى الطريق» بما لم يُستلم (ويُلغى إغلاق العجز إن كان).
            if (link.SourceOrderId is long shipmentId) RefreshTransferStatus(shipmentId, reopen: true);

            BusinessLibShared.CostCenters.InvoiceCostCenter.SetHeaderCenter(opp, BusinessLibShared.CostCenters.InvoiceCostCenter.Table_StoreOrder, rev.Id,
                BusinessLibShared.CostCenters.InvoiceCostCenter.GetHeaderCenter(opp, BusinessLibShared.CostCenters.InvoiceCostCenter.Table_StoreOrder, orderId));
            PostInventoryEntry(rev.Id);
            Log(3, orig, "عكس إذن: " + reason);
            return rev.Id;
        }

        // ============================== التحويل بخطوتين (خطة 03 المرحلة 3) ==============================

        /// <summary>حالة الشحنة فى Order_Purshing.Transfer_Status (NULL = ليست شحنة بخطوتين).</summary>
        public enum TransferStatus { InTransit = 1, PartiallyReceived = 2, Closed = 3 }

        /// <summary>سطر استلام: سطر الشحنة والكمية المستلمة بوحدتى صنف المرسل.</summary>
        public record ReceiptLineInput(long ShipmentDetailId, decimal QtyLarge, decimal QtySmall, string Notes = null);

        /// <summary>تقدم سطر الشحنة بالوحدة الصغرى: المشحون (من الدفتر) والمستلم (أذون استلام قائمة) وتكلفة الوحدة الصغرى.</summary>
        public class TransferLineProgress
        {
            public long Detail_Id { get; set; }
            public long Unit_Item_Id { get; set; }
            public decimal Factor { get; set; }
            public decimal Shipped_Small { get; set; }
            public decimal Received_Small { get; set; }
            public decimal Unit_Cost_Small { get; set; }
            public decimal Remaining_Small => Math.Max(0, Shipped_Small - Received_Small);
        }

        public List<TransferLineProgress> GetTransferProgress(long shipmentId) => opp.db.Database.SqlQuery<TransferLineProgress>($@"
SELECT d.Id AS Detail_Id, d.Unit_item_Table_Id AS Unit_Item_Id,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor,
       CAST(ISNULL((SELECT SUM(ABS(e.Qty_Small)) FROM Detailes_Purshing_Import_Export e
                    WHERE e.OrderTable_id = d.OrderTable_id AND e.Detailes_Purshing_Id = d.Id AND e.Engine_Posted = 1 AND e.Qty_Small < 0), 0) AS decimal(18,4)) AS Shipped_Small,
       CAST(ISNULL((SELECT SUM(ISNULL(rd.qua, 0) * CASE WHEN ISNULL(ru.Small_unit, 0) <= 0 THEN 1 ELSE ru.Small_unit END + ISNULL(rd.Part_qua, 0))
                    FROM Order_Purshing r
                    INNER JOIN Detailes_Purshing rd ON rd.OrderTable_id = r.Id AND rd.Source_Detail_Id = d.Id
                    LEFT JOIN Unit_item ru ON ru.id = rd.Unit_item_Table_Id
                    WHERE r.Source_Order_Id = d.OrderTable_id AND ISNULL(r.Doc_Status, 1) = 1), 0) AS decimal(18,4)) AS Received_Small,
       CAST(ISNULL((SELECT TOP 1 e.Unit_Cost FROM Detailes_Purshing_Import_Export e
                    WHERE e.OrderTable_id = d.OrderTable_id AND e.Detailes_Purshing_Id = d.Id AND e.Engine_Posted = 1 AND e.Qty_Small < 0), 0) AS decimal(18,6)) AS Unit_Cost_Small
FROM Detailes_Purshing d
LEFT JOIN Unit_item u ON u.id = d.Unit_item_Table_Id
WHERE d.OrderTable_id = {shipmentId}
ORDER BY d.Id").ToList();

        /// <summary>
        /// الخطوة الثانية: استلام الشحنة كلياً أو جزئياً بإذن «استلام من المخزن» (1016) مرحَّل ومرتبط بالشحنة وأسطرها.
        /// الكمية لا تتجاوز الباقى؛ الوارد بتكلفة الصرف المسجلة ويحدّث متوسط فرع المستلم (لا يتغير داخل نفس الفرع).
        /// سطر المستلم يُنشأ برصيد صفر لو غير موجود. يرجع معرّف إذن الاستلام.
        /// </summary>
        public long ReceiveTransfer(long shipmentId, DateTime date, IReadOnlyList<ReceiptLineInput> lines, string notes = null)
        {
            var ship = opp.db.Order_Purshings.AsNoTracking().First(o => o.Id == shipmentId);
            var link = GetTransferLink(shipmentId);
            if (GetStatus(shipmentId) != (int)DocStatus.Posted || link.Status is not ((int)TransferStatus.InTransit or (int)TransferStatus.PartiallyReceived))
                throw new InvalidOperationException("هذا الإذن ليس شحنة تحويل بانتظار الاستلام.");
            if (lines == null || lines.All(l => l.QtyLarge + l.QtySmall <= 0))
                throw new InvalidOperationException("أدخل كمية مستلمة لصنف واحد على الأقل.");

            var settings = GetSettings(ship.Order_Type ?? 0);
            long destId = settings.StoreIsFrom ? ship.Id_To : ship.Id_From;
            int destType = (settings.StoreIsFrom ? ship.Type_To : ship.type_From) ?? PartyStore;
            int srcType = (settings.StoreIsFrom ? ship.type_From : ship.Type_To) ?? PartyStore;
            long srcId = settings.StoreIsFrom ? ship.Id_From : ship.Id_To;
            CheckBalanceParty(destType, destId, "المخزن المستلم");

            var progress = GetTransferProgress(shipmentId).ToDictionary(p => p.Detail_Id);
            var srcIds = progress.Values.Select(p => p.Unit_Item_Id).ToList();
            var srcLines = opp.db.Unit_item.AsNoTracking().Where(u => srcIds.Contains(u.id)).ToDictionary(u => u.id);
            DateTime now = DateTime.Now;
            const int receiptType = (int)StoreOrderType.EstlamFromStore_16;

            var receipt = new Order_Purshing
            {
                Id = CallSnowflakeIdGenerate.GenerateNewId(),
                Order_Type = receiptType,
                Order_Purshing_No = NextOrderNo(receiptType),
                Order_date = date.Date,
                Transaction_Type = 1,
                type_From = srcType, Id_From = srcId,
                Type_To = destType, Id_To = destId,
                IsActiveMainOrder_Purshing = 1,
                NotesAll = string.IsNullOrWhiteSpace(notes) ? $"استلام التحويل رقم {ship.Order_Purshing_No}" : notes,
                Branch_ID = Common.Branch_Id,
                User_ID = Common.User_Id,
                OperationDate = now.Date,
                Insert_ServerDateTime = now,
            };
            opp.db.Order_Purshings.Add(receipt);
            opp.db.SaveChanges();
            opp.db.Database.ExecuteSqlInterpolated($"UPDATE Order_Purshing SET Source_Order_Id = {shipmentId}, Doc_Status = {(int)DocStatus.Draft} WHERE Id = {receipt.Id}");

            var doc = new MovementDocument(receipt.Id, receiptType, date.Date, TypeFrom: srcType, IdFrom: srcId, TypeTo: destType, IdTo: destId);
            foreach (var l in lines.Where(l => l.QtyLarge + l.QtySmall > 0))
            {
                if (l.QtyLarge < 0 || l.QtySmall < 0) throw new InvalidOperationException("كمية غير صحيحة.");
                if (!progress.TryGetValue(l.ShipmentDetailId, out var p) || !srcLines.TryGetValue(p.Unit_Item_Id, out var src))
                    throw new InvalidOperationException("سطر غير موجود فى الشحنة.");
                decimal qtySmall = l.QtyLarge * p.Factor + l.QtySmall;
                if (qtySmall > p.Remaining_Small)
                    throw new InvalidOperationException($"الكمية المستلمة للصنف {src.Code} أكبر من الباقى فى الطريق.");

                var dest = FindOrCreateDestLine(src, destId, destType);
                decimal destFactor = (dest.Small_unit ?? 0) <= 0 ? 1 : dest.Small_unit.Value;
                decimal destLarge = Math.Floor(qtySmall / destFactor), destSmall = qtySmall - destLarge * destFactor;
                var dp = new Detailes_Purshing
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(),
                    OrderTable_id = receipt.Id,
                    Item_Id = dest.Item_Id ?? 0,
                    Unit_item_Table_Id = dest.id,
                    StoreId = dest.Store_id,
                    Code = dest.Code,
                    Unit_Id = dest.unit_id,
                    qua = destLarge,
                    Part_qua = destSmall,
                    Item_No = 0,
                    type_From = srcType, Id_From = srcId, Type_To = destType, Id_To = destId,
                    Transaction_Type = 1,
                    ProcessNameId = receiptType,
                    PriceUnit = p.Unit_Cost_Small * destFactor,
                    Notes = l.Notes,
                    OperationDate = now.Date,
                    User_Id = Common.User_Id,
                    Branch_Id = Common.Branch_Id,
                    Insert_ServerDateTime = now,
                };
                opp.db.Detailes_Purshings.Add(dp);
                opp.db.SaveChanges();
                opp.db.Database.ExecuteSqlInterpolated($"UPDATE Detailes_Purshing SET Source_Detail_Id = {l.ShipmentDetailId} WHERE Id = {dp.Id}");

                var posted = engine.Post(doc, new[] { new MovementLine(dest.id, +1, destLarge, destSmall, 0,
                    UnitCostLarge: p.Unit_Cost_Small * destFactor, UpdatesAvgCost: true, DetailId: dp.Id, Notes: l.Notes) });
                if (posted.Count > 0) SetBalances(dp, posted[0], destFactor);
            }
            opp.db.SaveChanges();
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Order_Purshing SET Doc_Status = {(int)DocStatus.Posted},
    Posted_Date = ISNULL(Posted_Date, {DateTime.Now}), Posted_By = ISNULL(Posted_By, {Common.User_Id}) WHERE Id = {receipt.Id}");

            RefreshTransferStatus(shipmentId, reopen: false);
            PostInventoryEntry(receipt.Id);
            Log(1, receipt, $"استلام التحويل رقم {ship.Order_Purshing_No}");
            return receipt.Id;
        }

        /// <summary>
        /// يغلق شحنة لم تُستلم بالكامل بعجز (فقد/تلف فى الطريق) بسبب: الباقى لا يُستلم بعدها. الكمية خُصمت من المرسل عند الشحن،
        /// فلا حركة مخزون هنا؛ السبب يُحفظ على الشحنة ويُسجَّل فى سجل العمليات.
        /// </summary>
        public void CloseTransferShortage(long shipmentId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("سبب إغلاق العجز مطلوب.");
            var link = GetTransferLink(shipmentId);
            if (link.Status is not ((int)TransferStatus.InTransit or (int)TransferStatus.PartiallyReceived))
                throw new InvalidOperationException("لا يُغلق إلا شحنة بانتظار الاستلام.");
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Order_Purshing SET Transfer_Status = {(int)TransferStatus.Closed},
    Transfer_Close_Reason = {reason.Trim()} WHERE Id = {shipmentId}");
            var ship = opp.db.Order_Purshings.AsNoTracking().First(o => o.Id == shipmentId);
            Log(2, ship, "إغلاق شحنة بعجز: " + reason.Trim());
        }

        /// <summary>يعيد حساب حالة الشحنة من المستلم القائم؛ reopen يلغى إغلاق العجز (بعد عكس إذن استلام).</summary>
        private void RefreshTransferStatus(long shipmentId, bool reopen)
        {
            var link = GetTransferLink(shipmentId);
            if (link.Status == null) return;
            if (link.Status == (int)TransferStatus.Closed && !reopen) return;
            var prog = GetTransferProgress(shipmentId);
            decimal shipped = prog.Sum(p => p.Shipped_Small), received = prog.Sum(p => p.Received_Small);
            bool complete = received >= shipped;
            int status = complete ? (int)TransferStatus.Closed : received <= 0 ? (int)TransferStatus.InTransit : (int)TransferStatus.PartiallyReceived;
            // اكتمال الاستلام لا يترك سبب عجز؛ وإعادة الفتح بعد عكس استلام تلغى الإغلاق السابق.
            string reason = complete || reopen ? null : link.CloseReason;
            opp.db.Database.ExecuteSqlInterpolated($"UPDATE Order_Purshing SET Transfer_Status = {status}, Transfer_Close_Reason = {reason} WHERE Id = {shipmentId}");
        }

        private int ActiveReceiptsCount(long shipmentId) => opp.db.Database.SqlQuery<int>($@"
SELECT COUNT(*) AS Value FROM Order_Purshing WHERE Source_Order_Id = {shipmentId} AND ISNULL(Doc_Status, 1) = 1").AsEnumerable().First();

        /// <summary>حالة الشحنة وربط إذن الاستلام بشحنته (أعمدة غير موجودة فى الموديل القديم فتُقرأ بـ SQL).</summary>
        public TransferLink GetTransferLink(long orderId) => opp.db.Database.SqlQuery<TransferLink>($@"
SELECT Transfer_Status AS Status, Source_Order_Id AS SourceOrderId, Transfer_Close_Reason AS CloseReason
FROM Order_Purshing WHERE Id = {orderId}").AsEnumerable().First();

        public class TransferLink
        {
            public int? Status { get; set; }
            public long? SourceOrderId { get; set; }
            public string CloseReason { get; set; }
        }

        /// <summary>عكس + مسودة جديدة بنفس البيانات (بتاريخ اليوم) للتصحيح؛ يرجع رقم المسودة.</summary>
        public long ReverseAndCopy(long orderId, string reason)
        {
            Reverse(orderId, reason);
            var header = opp.db.Order_Purshings.AsNoTracking().First(o => o.Id == orderId);
            var lines = opp.db.Detailes_Purshings.AsNoTracking().Where(d => d.OrderTable_id == orderId).ToList();
            var input = ToInput(header, lines) with { OrderDate = DateTime.Today };
            return SaveDraft(input);
        }

        // ============================== التحقق ==============================

        /// <summary>يتحقق من الإذن ويرجع سطر المخزون لكل سطر بالترتيب.</summary>
        private List<Unit_item> Validate(OrderTypeSettings s, OrderInput input)
        {
            if (!s.Is_Active) throw new InvalidOperationException($"نوع الإذن «{s.Name_Ar}» غير مفعّل.");
            if (!s.FromParties.Contains(input.TypeFrom)) throw new InvalidOperationException($"جهة «من» غير مسموحة لنوع الإذن «{s.Name_Ar}».");
            if (!s.ToParties.Contains(input.TypeTo)) throw new InvalidOperationException($"جهة «إلى» غير مسموحة لنوع الإذن «{s.Name_Ar}».");

            int storeParty = s.StoreIsFrom ? input.TypeFrom : input.TypeTo;
            long storeId = s.StoreIsFrom ? input.IdFrom : input.IdTo;
            CheckBalanceParty(storeParty, storeId, "المخزن");

            int otherParty = s.StoreIsFrom ? input.TypeTo : input.TypeFrom;
            long otherId = s.StoreIsFrom ? input.IdTo : input.IdFrom;
            if (s.Is_Transfer)
            {
                if (otherId == storeId) throw new InvalidOperationException("لا يكون التحويل لنفس المخزن.");
                CheckBalanceParty(otherParty, otherId, "المخزن المحوَّل إليه");
            }

            if (input.Lines == null || input.Lines.Count == 0) throw new InvalidOperationException("الإذن بلا أصناف.");
            var ids = input.Lines.Select(l => l.UnitItemId).Distinct().ToList();
            var ums = opp.db.Unit_item.AsNoTracking().Where(u => ids.Contains(u.id)).ToDictionary(u => u.id);
            var result = new List<Unit_item>();
            foreach (var l in input.Lines)
            {
                if (!ums.TryGetValue(l.UnitItemId, out var um)) throw new InvalidOperationException("سطر مخزون غير موجود.");
                if (um.Store_id != storeId || (um.Store_Type ?? PartyStore) != storeParty) throw new InvalidOperationException($"الصنف {um.Code} ليس من المخزن المختار.");
                if (l.QtyLarge < 0 || l.QtySmall < 0 || l.QtyLarge + l.QtySmall <= 0) throw new InvalidOperationException($"كمية غير صحيحة للصنف {um.Code}.");
                if (s.IsManualPrice && (l.UnitCostLarge == null || l.UnitCostLarge < 0)) throw new InvalidOperationException($"سعر الوحدة مطلوب للصنف {um.Code}.");
                result.Add(um);
            }
            return result;
        }

        /// <summary>
        /// جهة رصيد = مخزن (4) أو قسم (20، قرار م-03-9). القسم صف فى Store بـ LookUp_StoreOrderTypesId = 20
        /// وأرصدته أسطر Unit_item بـ Store_Type = 20، كما فى الشاشة القديمة (FillcbOrderStorCLS / Check_ItemFoundOnStore).
        /// </summary>
        private void CheckBalanceParty(int partyType, long id, string label)
        {
            if (partyType != PartyStore && partyType != PartyDepartment)
                throw new InvalidOperationException($"جهة «{label}» يجب أن تكون مخزناً أو قسماً.");
            var store = opp.db.Stores.AsNoTracking().FirstOrDefault(x => x.Id == id);
            string kind = partyType == PartyDepartment ? "القسم" : "المخزن";
            if (store == null || (store.LookUp_StoreOrderTypesId ?? PartyStore) != partyType)
                throw new InvalidOperationException($"{kind} غير موجود.");
            if (store.Active == 0) throw new InvalidOperationException($"{kind} «{store.Store_Name}» غير نشط.");
        }

        // ============================== مساعدات ==============================

        private static void FillHeader(Order_Purshing h, OrderTypeSettings s, OrderInput input)
        {
            h.Order_date = input.OrderDate.Date;
            h.Transaction_Type = s.Direction == 1 ? 1 : 2;
            h.type_From = input.TypeFrom;
            h.Id_From = input.IdFrom;
            h.Type_To = input.TypeTo;
            h.Id_To = input.IdTo;
            h.Invoice_ID = input.InvoiceId;
            h.NotesAll = input.Notes;
        }

        private static OrderInput ToInput(Order_Purshing h, List<Detailes_Purshing> lines) =>
            new OrderInput(h.Order_Type ?? 0, h.Order_date ?? DateTime.Today, h.type_From ?? 0, h.Id_From, h.Type_To ?? 0, h.Id_To,
                lines.Select(d => new OrderLineInput(d.Unit_item_Table_Id, d.qua ?? 0, d.Part_qua ?? 0, d.Item_No ?? 0, d.PriceUnit, d.Notes)).ToList(),
                h.NotesAll, h.Invoice_ID ?? 0);

        /// <summary>رصيد السطر بعد الترحيل بالوحدتين (للطباعة والتقارير القديمة).</summary>
        private static void SetBalances(Detailes_Purshing dp, PostedLine p, decimal factor)
        {
            decimal before = p.BalanceSmallAfter - p.QtySmallSigned;
            dp.Qua_Befor = Math.Floor(before / factor);
            dp.Part_qua_Befor = before - dp.Qua_Befor * factor;
            dp.Qua_After = Math.Floor(p.BalanceSmallAfter / factor);
            dp.Part_qua_After = p.BalanceSmallAfter - dp.Qua_After * factor;
        }

        /// <summary>سطر الصنف فى المخزن المستلم؛ يُنشأ برصيد صفر لو غير موجود (كـ Check_ItemFoundOnStore القديمة).</summary>
        /// <summary>
        /// «تحويل أصناف مخزن لآخر برصيد صفر» (بديل Add_ALLItemFromTo_Store_): ينشئ فى المخزن/القسم المستلم سطراً برصيد صفر لكل صنف نشط
        /// فى المصدر غير موجود فيه (بنفس الوحدات والأسعار)، بلا أى حركة مخزون. categoryId = 0 لكل التصنيفات. يرجع عدد الأسطر المنشأة.
        /// </summary>
        public int CopyItemsToStore(int srcType, long srcStoreId, int destType, long destStoreId, long categoryId = 0, bool previewOnly = false)
        {
            if (srcStoreId == destStoreId && srcType == destType) throw new InvalidOperationException("المصدر والمستلم نفس المخزن.");
            CheckBalanceParty(srcType, srcStoreId, "المخزن المصدر");
            CheckBalanceParty(destType, destStoreId, "المخزن المستلم");
            var missing = opp.db.Database.SqlQuery<long>($@"
SELECT u.id AS Value FROM Unit_item u INNER JOIN Item i ON i.id = u.Item_id
WHERE u.Store_id = {srcStoreId} AND ISNULL(u.Store_Type, 4) = {srcType}
  AND ISNULL(i.Active_flage, 1) = 1 AND ISNULL(u.active_flage, 1) = 1
  AND ({categoryId} = 0 OR i.Item_Category_ID = {categoryId})
  AND NOT EXISTS (SELECT 1 FROM Unit_item d WHERE d.Item_id = u.Item_id AND d.Store_id = {destStoreId} AND ISNULL(d.Store_Type, 4) = {destType})").ToList();
            if (previewOnly) return missing.Count;
            foreach (var chunk in missing.Chunk(500))
            {
                var ids = chunk.ToList();
                foreach (var src in opp.db.Unit_item.AsNoTracking().Where(u => ids.Contains(u.id)).ToList())
                    FindOrCreateDestLine(src, destStoreId, destType);
            }
            return missing.Count;
        }

        /// <summary>سطر الصنف فى مخزن آخر (ينشئه بلا رصيد لو غير موجود، كما يفعل التحويل). يرجع Unit_item.id.</summary>
        public long EnsureStoreLine(long srcUnitItemId, long destStoreId)
        {
            var src = opp.db.Unit_item.AsNoTracking().FirstOrDefault(u => u.id == srcUnitItemId) ?? throw new InvalidOperationException("سطر المخزون غير موجود.");
            if (src.Store_id == destStoreId) return src.id;
            return FindOrCreateDestLine(src, destStoreId, PartyStore).id;
        }

        private Unit_item FindOrCreateDestLine(Unit_item src, long destStoreId, int destType)
        {
            var dest = opp.db.Unit_item.AsNoTracking().FirstOrDefault(u => u.Item_Id == src.Item_Id && u.Store_id == destStoreId
                && (u.Store_Type ?? PartyStore) == destType);
            if (dest != null) return dest;

            long? branchId = opp.db.Stores.AsNoTracking().Where(x => x.Id == destStoreId).Select(x => x.Branch_Id).FirstOrDefault();
            DateTime now = DateTime.Now;
            dest = new Unit_item
            {
                id = CallSnowflakeIdGenerate.GenerateNewId(),
                Item_Id = src.Item_Id, Store_id = destStoreId, Store_Type = destType, Branch_Id = branchId,
                Code = src.Code, unit_id = src.unit_id, Big_unit = src.Big_unit, Small_unit = src.Small_unit,
                Full_qua = 0, D_Qua = 0, Item_No = 0, Reserved_Balance_Larg_Qnty = 0, Reserved_Balance_Small_Qnty = 0,
                P_price = src.P_price, S_price = src.S_price, P_priceAvgCost = 0, Specification = src.Specification,
                Upon_Request = src.Upon_Request, Currency = src.Currency, active_flage = src.active_flage,
                Is_ItemWithoutBalance = src.Is_ItemWithoutBalance,
                S_price_LargeUnit = src.S_price_LargeUnit, S_price_SmallUnit = src.S_price_SmallUnit,
                P_price_LargeUnit = src.P_price_LargeUnit, P_price_SmallUnit = src.P_price_SmallUnit,
                Unit_Used_For_S_price_Default = src.Unit_Used_For_S_price_Default, Unit_Used_For_P_price_Default = src.Unit_Used_For_P_price_Default,
                S_price_Item_No = src.S_price_Item_No, P_price_Item_No = src.P_price_Item_No,
                OperationDate = now.Date, User_Id = Common.User_Id, Insert_ServerDateTime = now,
            };
            opp.db.Unit_item.Add(dest);
            opp.db.SaveChanges();
            opp.db.Entry(dest).State = EntityState.Detached;   // المحرك يقفل السطر ويقرؤه بنفسه
            return dest;
        }

        /// <summary>رقم الإذن التالى لكل نوع (كـ GetNewOrderNo القديمة) مع قفل يمنع تكرار الرقم بين حفظين متزامنين.</summary>
        private long NextOrderNo(int orderType)
        {
            long max = opp.db.Database.SqlQuery<long>($@"
SELECT ISNULL(MAX(Order_Purshing_No), 0) AS Value FROM Order_Purshing WITH (UPDLOCK, HOLDLOCK) WHERE Order_Type = {orderType}").AsEnumerable().First();
            return max < 0 ? 1 : max + 1;
        }

        private int GetStatus(long orderId)
        {
            var st = opp.db.Database.SqlQuery<int?>($"SELECT ISNULL(Doc_Status, 1) AS Value FROM Order_Purshing WHERE Id = {orderId}").AsEnumerable().ToList();
            if (st.Count == 0) throw new InvalidOperationException("الإذن غير موجود.");
            return st[0] ?? 1;
        }

        /// <summary>
        /// سجل العمليات بنفس أعمدة DataDictionaryLogerProcessCLS.AddNew لكن بأمر SQL مُمرَّر بالمعاملات: لو كان Id عموداً تلقائياً (Identity)
        /// يُدرج بلا Id، وإلا بمعرّف Snowflake. الطريقة القديمة (Add + SaveChanges) تفشل حين Use_Auto_Identity_Column = 0 على قاعدة Id فيها تلقائى،
        /// وتبتلع الخطأ لكن الصف الفاشل يبقى فى ChangeTracker فيُفشل SaveChanges التالى فى نفس المعاملة (ترحيل بعد حفظ المسودة).
        /// </summary>
        private void Log(long subProcessId, Order_Purshing header, string notes)
        {
            long processId = header.Order_Type ?? 0;
            string subName = subProcessId switch { 1 => "إضافة", 2 => "تعديل", 3 => "حذف", _ => "" };
            long newId = CallSnowflakeIdGenerate.GenerateNewId();
            opp.db.Database.ExecuteSqlInterpolated($@"
DECLARE @pn nvarchar(500) = ISNULL((SELECT TOP 1 ProcessName FROM DataDictionaryProcessesLists WHERE Id = {processId}), N'');
IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID('DataDictionaryLogerProcesses') AND name = 'Id')
    INSERT INTO DataDictionaryLogerProcesses (ProcessId, ProcessName, SubProcessId, SubProcessName, ProcessDocNo, ProcessTableId,
        User_Id, UserName, Branch_Id, ProcessDateTime, ProcessNotes, Item_Name, Qnty)
    VALUES ({processId}, @pn, {subProcessId}, {subName}, {header.Order_Purshing_No}, {header.Id},
        {Common.User_Id}, {Common.User_Name ?? ""}, {Common.Branch_Id}, {DateTime.Now}, {notes ?? ""}, N'', N'');
ELSE
    INSERT INTO DataDictionaryLogerProcesses (Id, ProcessId, ProcessName, SubProcessId, SubProcessName, ProcessDocNo, ProcessTableId,
        User_Id, UserName, Branch_Id, ProcessDateTime, ProcessNotes, Item_Name, Qnty)
    VALUES ({newId}, {processId}, @pn, {subProcessId}, {subName}, {header.Order_Purshing_No}, {header.Id},
        {Common.User_Id}, {Common.User_Name ?? ""}, {Common.Branch_Id}, {DateTime.Now}, {notes ?? ""}, N'', N'');");
        }
    }
}
