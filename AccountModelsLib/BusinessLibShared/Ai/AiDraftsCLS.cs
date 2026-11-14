using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AccountModelsLib
{
    public static class AiDraftKind
    {
        public const string Sales = "sales";
        public const string Purchase = "purchase";
        public const string StoreOrder = "store_order";
    }

    public static class AiDraftStatus
    {
        public const string Pending = "Pending";
        public const string Consumed = "Consumed";
        public const string Discarded = "Discarded";
    }

    /// <summary>سطر صنف مطابق فى المسودة: UnitItemId = صف Unit_item (الصنف فى المخزن). السعر null = الشاشة تحدده بمنطقها.</summary>
    public class AiDraftLine
    {
        public long UnitItemId { get; set; }
        public long ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string Code { get; set; } = "";
        public decimal QtyLarge { get; set; }
        public decimal QtySmall { get; set; }
        public decimal? Price { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>بند لم يُطابق صنفاً فى المخزن: لا يُنشأ صنف أبداً؛ يُعرض للمستخدم ليختاره بنفسه.</summary>
    public class AiDraftUnmatched
    {
        public string Text { get; set; } = "";
        public decimal QtyLarge { get; set; }
        public decimal QtySmall { get; set; }
        public decimal? Price { get; set; }
        public string Reason { get; set; } = "";
    }

    /// <summary>محتوى المسودة (PayloadJson). الحقول غير المستخدمة لنوع ما تبقى افتراضية.</summary>
    public class AiDraftPayload
    {
        public string Kind { get; set; } = AiDraftKind.Sales;
        /// <summary>العميل (مبيعات) أو المورد (مشتريات).</summary>
        public long PartyId { get; set; }
        public string PartyName { get; set; } = "";
        public long StoreId { get; set; }
        public string StoreName { get; set; } = "";
        /// <summary>إذن المخزن: نوع الإذن (Store_Order_Type_Settings.Order_Type) والجهة الأخرى (اختيارية).</summary>
        public int OrderType { get; set; }
        public string OrderTypeName { get; set; } = "";
        public int OtherPartyType { get; set; }
        public long OtherPartyId { get; set; }
        public string OtherPartyName { get; set; } = "";
        public DateTime? Date { get; set; }
        /// <summary>null = إعداد الشاشة الافتراضى.</summary>
        public bool? TaxInvoice { get; set; }
        /// <summary>فاتورة آجلة (agile=1).</summary>
        public bool Credit { get; set; }
        public string Notes { get; set; }
        public List<AiDraftLine> Lines { get; set; } = new List<AiDraftLine>();
        public List<AiDraftUnmatched> Unmatched { get; set; } = new List<AiDraftUnmatched>();
        public List<string> Warnings { get; set; } = new List<string>();
        /// <summary>P6: رقم فاتورة المورد (أو مستند العميل) كما فى المستند المرفق؛ يُكتب فى الملاحظات بالبادئة SupplierRefPrefix.</summary>
        public string RefNo { get; set; }
        /// <summary>P6: المرفق الذى استُخرجت منه المسودة واسمه.</summary>
        public long SourceAttachmentId { get; set; }
        public string SourceFileName { get; set; }
        /// <summary>P6: إجمالى المستند كما قرأه المساعد (للمقارنة بعد تعبئة الشاشة).</summary>
        public decimal? DocTotal { get; set; }

        /// <summary>P6: بادئة رقم فاتورة المورد فى ملاحظات الفاتورة (Bills_List.Notes) — بها يُكشف التكرار بعد الحفظ.</summary>
        public const string SupplierRefPrefix = "فاتورة المورد رقم: ";

        public string ToJson() => JsonConvert.SerializeObject(this);
        public static AiDraftPayload FromJson(string json)
        {
            try { return string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<AiDraftPayload>(json); }
            catch (JsonException) { return null; }
        }
    }

    public class AiDraftRow
    {
        public long Id { get; set; }
        public long User_Id { get; set; }
        public long? Conversation_Id { get; set; }
        public string DraftType { get; set; } = "";
        public string Title { get; set; } = "";
        public string PayloadJson { get; set; } = "";
        public string Status { get; set; } = AiDraftStatus.Pending;
        public long? Target_Doc_Id { get; set; }
        public string Target_Doc_No { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConsumedAt { get; set; }

        public AiDraftPayload Payload => AiDraftPayload.FromJson(PayloadJson);
    }

    /// <summary>صف Unit_item (الصنف فى مخزن) بما تحتاجه المسودة والشاشات لبناء السطر.</summary>
    public class AiDraftStockRow
    {
        public long UnitItemId { get; set; }
        public long ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string Code { get; set; } = "";
        public long StoreId { get; set; }
        public string StoreName { get; set; } = "";
        public decimal FullQua { get; set; }
        public decimal DQua { get; set; }
        public string BUnit { get; set; } = "";
        public string SUnit { get; set; } = "";
        /// <summary>عدد الصغرى فى الكبرى (unit.S_unit_Value، 1 لو صفر).</summary>
        public decimal SmallPerLarge { get; set; } = 1;
        public decimal SalePrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal AvgCostLarge { get; set; }
        public bool ItemActive { get; set; }
        public bool IsWithoutBalance { get; set; }
        public decimal BalanceSmall => FullQua * (SmallPerLarge <= 0 ? 1 : SmallPerLarge) + DQua;
    }

    /// <summary>
    /// P5: مسودات المساعد (AI_Drafts، جدول جديد). كل دالة مقيّدة بـ User_Id (لا يفتح مستخدم مسودة غيره).
    /// القراءة من جداول النظام هنا للتحقق وبناء الأسطر فقط (SELECT بمعاملات)، ولا كتابة إلا فى AI_Drafts.
    /// </summary>
    public class AiDraftsCLS
    {
        private readonly Func<operations> _newOpp;

        public AiDraftsCLS(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public long Insert(long userId, long? conversationId, string title, AiDraftPayload payload)
        {
            long id = CallSnowflakeIdGenerate.GenerateNewId();
            DateTime now = DateTime.Now;
            string json = payload.ToJson();
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
INSERT INTO AI_Drafts (Id, User_Id, Conversation_Id, Message_Id, DraftType, Title, PayloadJson, Status, CreatedBy, CreatedAt,
                       Party_Id, Ref_No, Source_Attachment_Id)
VALUES ({id}, {userId}, {conversationId}, NULL, {payload.Kind}, {Cut(title, 200)}, {json}, {AiDraftStatus.Pending}, {userId}, {now},
        {(payload.PartyId > 0 ? payload.PartyId : (long?)null)}, {Cut(NormalizeRef(payload.RefNo), 60)}, {(payload.SourceAttachmentId > 0 ? payload.SourceAttachmentId : (long?)null)})");
            return id;
        }

        /// <summary>رقم المستند للمقارنة: بلا مسافات، أرقام لاتينية، أحرف كبيرة. فارغ ⇒ null.</summary>
        public static string NormalizeRef(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var sb = new System.Text.StringBuilder();
            foreach (char ch in s.Trim())
            {
                if (char.IsWhiteSpace(ch)) continue;
                if (ch >= '٠' && ch <= '٩') sb.Append((char)('0' + (ch - '٠')));
                else if (ch >= '۰' && ch <= '۹') sb.Append((char)('0' + (ch - '۰')));
                else sb.Append(char.ToUpperInvariant(ch));
            }
            return sb.Length == 0 ? null : sb.ToString();
        }

        /// <summary>تكرار محتمل لمستند من نفس الجهة.</summary>
        public class DuplicateHit
        {
            /// <summary>draft = مسودة سابقة للمساعد، bill = فاتورة مشتريات محفوظة (رقم المورد فى ملاحظاتها)، bill_amount = نفس التاريخ والإجمالى.</summary>
            public string Source { get; set; } = "";
            public long Id { get; set; }
            public string DocNo { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime? Date { get; set; }
        }

        /// <summary>
        /// P6: كشف تكرار فاتورة المورد (ممارسة الأنظمة الحديثة: نفس المورد + نفس رقم فاتورته، وإلا نفس التاريخ والإجمالى). قراءة فقط:
        /// مسودات المساعد المعلّقة/المحفوظة، وفواتير المشتريات غير المحذوفة التى تحمل رقم المورد فى ملاحظاتها، وفواتير نفس اليوم بنفس الصافى.
        /// </summary>
        public List<DuplicateHit> FindPurchaseDuplicates(long supplierId, string refNo, DateTime? docDate, decimal? docTotal, long excludeDraftId = 0)
        {
            var hits = new List<DuplicateHit>();
            if (supplierId <= 0) return hits;
            var db = _newOpp().db;
            string norm = NormalizeRef(refNo);
            if (norm != null)
            {
                Exec(db, @"SELECT TOP 5 Id, Status, Target_Doc_No, CreatedAt FROM AI_Drafts
WHERE DraftType = @k AND Party_Id = @p AND Ref_No = @r AND Status IN (@s1, @s2) AND Id <> @x ORDER BY CreatedAt DESC",
                    cmd => { Add(cmd, "@k", AiDraftKind.Purchase); Add(cmd, "@p", supplierId); Add(cmd, "@r", norm); Add(cmd, "@s1", AiDraftStatus.Pending); Add(cmd, "@s2", AiDraftStatus.Consumed); Add(cmd, "@x", excludeDraftId); },
                    r => hits.Add(new DuplicateHit { Source = "draft", Id = r.GetInt64(0), Status = r.IsDBNull(1) ? "" : r.GetString(1), DocNo = r.IsDBNull(2) ? "" : r.GetString(2), Date = r.IsDBNull(3) ? (DateTime?)null : r.GetDateTime(3) }));
                string like = "%" + AiDraftPayload.SupplierRefPrefix + EscapeLike(refNo.Trim()) + "%";
                Exec(db, @"SELECT TOP 5 ID, Invoice_Id, Invoice_Date FROM Bills_List
WHERE Supplier_Id = @p AND ISNULL(Deleted, 0) = 0 AND Notes LIKE @l ESCAPE '\' ORDER BY Invoice_Date DESC",
                    cmd => { Add(cmd, "@p", supplierId); Add(cmd, "@l", like); },
                    r => hits.Add(new DuplicateHit { Source = "bill", Id = Convert.ToInt64(r.GetValue(0)), DocNo = r.IsDBNull(1) ? "" : Convert.ToString(r.GetValue(1)), Date = r.IsDBNull(2) ? (DateTime?)null : r.GetDateTime(2) }));
            }
            if (docDate != null && docTotal is decimal t && t > 0)
                Exec(db, @"SELECT TOP 5 ID, Invoice_Id, Invoice_Date FROM Bills_List
WHERE Supplier_Id = @p AND ISNULL(Deleted, 0) = 0 AND Invoice_Date >= @d AND Invoice_Date < DATEADD(day, 1, @d) AND ABS(ISNULL(Net_Total, 0) - @t) < 0.01
ORDER BY Invoice_Date DESC",
                    cmd => { Add(cmd, "@p", supplierId); Add(cmd, "@d", docDate.Value.Date); Add(cmd, "@t", t); },
                    r =>
                    {
                        long id = Convert.ToInt64(r.GetValue(0));
                        if (hits.Any(h => h.Source == "bill" && h.Id == id)) return;
                        hits.Add(new DuplicateHit { Source = "bill_amount", Id = id, DocNo = r.IsDBNull(1) ? "" : Convert.ToString(r.GetValue(1)), Date = r.IsDBNull(2) ? (DateTime?)null : r.GetDateTime(2) });
                    });
            return hits;
        }

        private static string EscapeLike(string s) => s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");

        public void LinkMessage(IEnumerable<long> ids, long messageId, long userId)
        {
            var list = ids?.Where(i => i > 0).Distinct().ToList() ?? new List<long>();
            if (list.Count == 0) return;
            var db = _newOpp().db;
            foreach (var id in list)
                db.Database.ExecuteSqlInterpolated($"UPDATE AI_Drafts SET Message_Id = {messageId} WHERE Id = {id} AND User_Id = {userId} AND Message_Id IS NULL");
        }

        public AiDraftRow Get(long id, long userId)
        {
            AiDraftRow found = null;
            Exec(_newOpp().db, @"SELECT Id, User_Id, Conversation_Id, DraftType, Title, PayloadJson, Status, Target_Doc_Id, Target_Doc_No, CreatedAt, ConsumedAt
                                 FROM AI_Drafts WHERE Id = @id AND User_Id = @u",
                cmd => { Add(cmd, "@id", id); Add(cmd, "@u", userId); },
                r => found = new AiDraftRow
                {
                    Id = r.GetInt64(0),
                    User_Id = r.GetInt64(1),
                    Conversation_Id = r.IsDBNull(2) ? (long?)null : r.GetInt64(2),
                    DraftType = r.IsDBNull(3) ? "" : r.GetString(3),
                    Title = r.IsDBNull(4) ? "" : r.GetString(4),
                    PayloadJson = r.IsDBNull(5) ? "" : r.GetString(5),
                    Status = r.IsDBNull(6) ? AiDraftStatus.Pending : r.GetString(6),
                    Target_Doc_Id = r.IsDBNull(7) ? (long?)null : r.GetInt64(7),
                    Target_Doc_No = r.IsDBNull(8) ? null : r.GetString(8),
                    CreatedAt = r.IsDBNull(9) ? DateTime.MinValue : r.GetDateTime(9),
                    ConsumedAt = r.IsDBNull(10) ? (DateTime?)null : r.GetDateTime(10),
                });
            return found;
        }

        /// <summary>بعد حفظ المستند بنجاح: يُغلق مرة واحدة فقط (شرط Status = Pending). يرجع false لو كان مستهلكاً من قبل.</summary>
        public bool MarkConsumed(long id, long userId, long docId, string docNo)
        {
            DateTime now = DateTime.Now;
            return _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Drafts SET Status = {AiDraftStatus.Consumed}, Target_Doc_Id = {docId}, Target_Doc_No = {Cut(docNo, 50)}, ConsumedAt = {now},
       UpdatedBy = {userId}, UpdatedAt = {now}
WHERE Id = {id} AND User_Id = {userId} AND Status = {AiDraftStatus.Pending}") > 0;
        }

        public void Discard(long id, long userId)
        {
            DateTime now = DateTime.Now;
            _newOpp().db.Database.ExecuteSqlInterpolated($@"
UPDATE AI_Drafts SET Status = {AiDraftStatus.Discarded}, UpdatedBy = {userId}, UpdatedAt = {now}
WHERE Id = {id} AND User_Id = {userId} AND Status = {AiDraftStatus.Pending}");
        }

        /// <summary>حالة مسودات رسائل محادثة (لبطاقات «تم الحفظ برقم ...»).</summary>
        public Dictionary<long, (string Status, string DocNo)> Statuses(IEnumerable<long> ids, long userId)
        {
            var map = new Dictionary<long, (string, string)>();
            var list = ids?.Where(i => i > 0).Distinct().ToList() ?? new List<long>();
            if (list.Count == 0) return map;
            string inList = string.Join(",", list);   // أرقام long فقط
            Exec(_newOpp().db, "SELECT Id, Status, Target_Doc_No FROM AI_Drafts WHERE User_Id = @u AND Id IN (" + inList + ")",
                cmd => Add(cmd, "@u", userId),
                r => map[r.GetInt64(0)] = (r.IsDBNull(1) ? "" : r.GetString(1), r.IsDBNull(2) ? "" : r.GetString(2)));
            return map;
        }

        /// <summary>مع حذف المحادثة: المسودات المعلّقة تُلغى (المستهلكة تبقى للتدقيق).</summary>
        public void DetachConversation(long conversationId, long userId)
        {
            var db = _newOpp().db;
            DateTime now = DateTime.Now;
            db.Database.ExecuteSqlInterpolated($@"UPDATE AI_Drafts SET Status = {AiDraftStatus.Discarded}, UpdatedAt = {now}
WHERE Conversation_Id = {conversationId} AND User_Id = {userId} AND Status = {AiDraftStatus.Pending}");
            db.Database.ExecuteSqlInterpolated($"UPDATE AI_Drafts SET Conversation_Id = NULL, Message_Id = NULL WHERE Conversation_Id = {conversationId} AND User_Id = {userId}");
        }

        // ───────────── قراءة للتحقق وبناء الأسطر ─────────────

        private const string StockSelect = @"SELECT ui.Id, ui.Item_id, i.Item_Name, i.code, ui.Store_id, s.Store_Name, ISNULL(ui.Full_qua, 0), ISNULL(ui.D_Qua, 0),
       u.B_Unit, u.S_unit, ISNULL(u.S_unit_Value, 1), ISNULL(ui.S_price, 0), ISNULL(NULLIF(ui.P_price_LargeUnit, 0), ISNULL(ui.P_Price, 0)),
       ISNULL(ui.P_priceAvgCost, 0), ISNULL(i.Active_flage, 0), ISNULL(i.Is_ItemWithoutBalance, 0)
FROM Unit_item ui
INNER JOIN unit u ON ui.unit_id = u.id
INNER JOIN Item i ON ui.Item_id = i.id
INNER JOIN Store s ON ui.Store_id = s.id";

        private static AiDraftStockRow ReadStock(DbDataReader r)
        {
            decimal D(int i) => r.IsDBNull(i) ? 0 : Convert.ToDecimal(r.GetValue(i));
            string S(int i) => r.IsDBNull(i) ? "" : Convert.ToString(r.GetValue(i))?.Trim() ?? "";
            decimal f = D(10);
            return new AiDraftStockRow
            {
                UnitItemId = Convert.ToInt64(r.GetValue(0)),
                ItemId = Convert.ToInt64(r.GetValue(1)),
                ItemName = S(2),
                Code = S(3),
                StoreId = Convert.ToInt64(r.GetValue(4)),
                StoreName = S(5),
                FullQua = D(6),
                DQua = D(7),
                BUnit = S(8),
                SUnit = S(9),
                SmallPerLarge = f <= 0 ? 1 : f,
                SalePrice = D(11),
                PurchasePrice = D(12),
                AvgCostLarge = D(13),
                ItemActive = D(14) == 1,
                IsWithoutBalance = D(15) == 1,
            };
        }

        /// <summary>الصنف فى المخزن (أول صف Unit_item للصنف والمخزن) أو null.</summary>
        public AiDraftStockRow FindStock(long itemId, long storeId)
        {
            AiDraftStockRow found = null;
            Exec(_newOpp().db, StockSelect + " WHERE ui.Item_id = @i AND ui.Store_id = @s ORDER BY ui.Id",
                cmd => { Add(cmd, "@i", itemId); Add(cmd, "@s", storeId); },
                r => { if (found == null) found = ReadStock(r); });
            return found;
        }

        public List<AiDraftStockRow> GetStock(IEnumerable<long> unitItemIds)
        {
            var list = new List<AiDraftStockRow>();
            var ids = unitItemIds?.Where(i => i > 0).Distinct().ToList() ?? new List<long>();
            if (ids.Count == 0) return list;
            Exec(_newOpp().db, StockSelect + " WHERE ui.Id IN (" + string.Join(",", ids) + ")", _ => { }, r => list.Add(ReadStock(r)));
            return list;
        }

        /// <summary>اسم العميل/المورد/المخزن النشط أو null. المخزن: نشط وليس مخزناً خارجياً وهمياً (كقائمة مخازن الفاتورة).</summary>
        public string ActiveName(string entity, long id)
        {
            string sql = entity switch
            {
                "customer" => "SELECT Name FROM Customer WHERE Id = @id AND ISNULL(Active, 1) = 1",
                "supplier" => "SELECT Name FROM Supplier WHERE Id = @id AND ISNULL(Active, 1) = 1",
                "store" => "SELECT Store_Name FROM Store WHERE Id = @id AND Active = 1 AND ISNULL(IS_Virtual_OuterStore, 0) = 0",
                _ => throw new ArgumentException(entity),
            };
            string name = null;
            Exec(_newOpp().db, sql, cmd => Add(cmd, "@id", id), r => name = r.IsDBNull(0) ? "" : Convert.ToString(r.GetValue(0))?.Trim() ?? "");
            return name;
        }

        private static string Cut(string s, int max) => s == null ? null : (s.Length > max ? s.Substring(0, max) : s);

        private static void Add(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static void Exec(My_DbContext db, string sql, Action<DbCommand> bind, Action<DbDataReader> row)
        {
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                bind(cmd);
                using var r = cmd.ExecuteReader();
                while (r.Read()) row(r);
            }
            finally { if (wasClosed) conn.Close(); }
        }
    }
}
