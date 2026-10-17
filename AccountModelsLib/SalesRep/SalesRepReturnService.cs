using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 4: المرتجع المربوط بالفاتورة والمرتجع بلا فاتورة (§6.6، ق6، ق15).
    /// - المستند يُبنى على السيرفر من سطر الفاتورة الأصلية (لا ثقة بأسعار الواجهة): نفس السعر والخصم والضريبة والتكلفة (P_priceAvgCost)
    ///   والتشغيلة، والمبالغ بنسبة الكمية المرتجعة بتقريب تراكمى فمجموع المرتجعات عند إرجاع السطر كله = السطر الأصلى بالضبط.
    ///   خصم رأس الفاتورة الأصلية يُوزَّع على الأسطر بنسبة قيمتها. المرتجع الواحد = فاتورة أصلية واحدة.
    /// - المسموح بإرجاعه = المباع (المدفوع، بلا المجانى) − مجموع المرتجع سابقاً من SalesRep_Invoice_Return_Links (محسوب لا مخزَّن).
    /// - الصالح ← مخزن السيارة، التالف ← مخزن تالف السيارة (السبب التالف يفرض الحالة)، السبب إلزامى والصورة حسب السبب.
    /// - خارج نافذة الإرجاع (ملف المندوب، الافتراضى 30 يوماً) = موافقة المشرف مرة واحدة لنفس المستند.
    /// - بلا فاتورة (ق15): لمن عنده Allow_Blind_Return فقط، بلا موافقة، بسعر الأمان = أدنى سعر بيع تاريخى للصنف − سقف خصم المندوب،
    ///   والتكلفة = متوسط الفرع الحالى، بلا ضريبة، ويُعلَّم Is_Blind ويظهر فى تقرير المرتجعات.
    /// - الحفظ بمحرك المرتجع الحالى (Make_Invoice_Reverse_Update_CustomerAccountInTx) فى معاملة واحدة مع الفحوص والربط. الرد النقدى
    ///   = إذن صرف من صندوق العهدة مربوط بالمرتجع (لا يتجاوز النقدية بالعهدة)، وإلا يُخصم المرتجع من رصيد العميل.
    /// </summary>
    public class SalesRepReturnService
    {
        public const int DefaultReturnWindowDays = 30;
        public const int MaxPhotoBytes = 5 * 1024 * 1024;

        private readonly operations _external;
        public SalesRepReturnService(operations external = null) { _external = external; }

        public class ReasonRow
        {
            public long Id { get; set; }
            public string Name { get; set; } = "";
            public bool Is_Damaged { get; set; }
            public bool Requires_Photo { get; set; }
            public bool Requires_Batch { get; set; }
            public bool Active { get; set; } = true;
            /// <summary>عدد أسطر المرتجع التى استعملت السبب (للعرض فى شاشة الأسباب).</summary>
            public int Used_Count { get; set; }
        }

        /// <summary>فاتورة بيع للعميل يمكن الإرجاع منها.</summary>
        public class InvoicePick
        {
            public long Id { get; set; }
            public long Invoice_No { get; set; }
            public DateTime? Invoice_Date { get; set; }
            public decimal Net_Total { get; set; }
            public int Is_Credit { get; set; }
            public int Age_Days { get; set; }
            public bool Inside_Window { get; set; }
        }

        /// <summary>سطر قابل للإرجاع من الفاتورة الأصلية (الكميات بالوحدة الصغرى، السعر الصافى للوحدة الصغرى بعد الخصم والضريبة).</summary>
        public class ReturnableLine
        {
            public long Original_Detail_Id { get; set; }
            public long Unit_Item_Id { get; set; }
            public long Item_Id { get; set; }
            public string Item_Name { get; set; } = "";
            public string Code { get; set; } = "";
            public string B_Unit { get; set; } = "";
            public string S_Unit { get; set; } = "";
            public decimal Factor { get; set; } = 1;
            public decimal Sold_Small { get; set; }
            public decimal Returned_Small { get; set; }
            public decimal Returnable_Small => Math.Max(0, Sold_Small - Returned_Small);
            public decimal Net_Unit_Price { get; set; }
            public bool Has_Batch { get; set; }
            /// <summary>المجانى المصروف مع السطر والمرتجع منه سابقاً (بالوحدة الصغرى): يُرجع بنسبة المدفوع تلقائياً.</summary>
            public decimal Free_Small { get; set; }
            public decimal Free_Returned_Small { get; set; }
            public long? Batch_Id { get; set; }
            public string Batch_Label { get; set; } = "";
            /// <summary>الصنف له تشغيلات مسجلة (فالسبب الذى يتطلب التشغيلة يفرضها).</summary>
            public bool Item_Has_Batches { get; set; }
        }

        public class OriginalView
        {
            public long Id { get; set; }
            public long Invoice_No { get; set; }
            public DateTime? Invoice_Date { get; set; }
            public long Customer_Id { get; set; }
            public string Customer_Name { get; set; } = "";
            public decimal Net_Total { get; set; }
            public bool Is_Credit { get; set; }
            public int Age_Days { get; set; }
            public int Window_Days { get; set; }
            public bool Inside_Window => Age_Days <= Window_Days;
            [System.ComponentModel.DataAnnotations.Schema.NotMapped] public List<ReturnableLine> Lines { get; set; } = new();
        }

        public class ReturnLineInput
        {
            /// <summary>سطر الفاتورة الأصلية (المرتجع المربوط).</summary>
            public long? Original_Detail_Id { get; set; }
            /// <summary>سطر الصنف (المرتجع بلا فاتورة فقط).</summary>
            public long? Unit_Item_Id { get; set; }
            public decimal Qty_Large { get; set; }
            public decimal Qty_Small { get; set; }
            public long Reason_Id { get; set; }
            public int Condition { get; set; }
            /// <summary>التشغيلة الممسوحة (Unit_Item_ExpireDate.Id)؛ فارغة = تشغيلة سطر الفاتورة إن وجدت.</summary>
            public long? Batch_Id { get; set; }
            public byte[] Photo { get; set; }
            public string Photo_Content_Type { get; set; }
        }

        public class ReturnInput
        {
            public Guid Doc_Guid { get; set; }
            public long Customer_Id { get; set; }
            /// <summary>Invoices_List.Id للمرتجع المربوط؛ فارغ = بلا فاتورة.</summary>
            public long? Original_Invoice_Id { get; set; }
            public bool Refund_Cash { get; set; }
            public string Notes { get; set; }
            public List<ReturnLineInput> Lines { get; set; } = new();
        }

        /// <summary>سطر المستند المحسوب للعرض قبل الحفظ.</summary>
        public class PreviewLine
        {
            public string Item_Name { get; set; } = "";
            public decimal Qty_Small { get; set; }
            public decimal Free_Small { get; set; }
            public decimal Net_Unit_Price { get; set; }
            public decimal Value { get; set; }
            public int Condition { get; set; }
        }

        /// <summary>نتيجة مسح باركود (الصنف أو التشغيلة) فى شاشة المرتجع.</summary>
        public class ScanResult
        {
            public long? Original_Detail_Id { get; set; }
            public long? Unit_Item_Id { get; set; }
            public long? Batch_Id { get; set; }
            public string Batch_Label { get; set; } = "";
            public string Item_Name { get; set; } = "";
        }

        public class ReturnCheck
        {
            public SalesRepSaleService.CheckResult Result { get; set; } = new();
            public List<PreviewLine> Lines { get; set; } = new();
            public decimal Net_Total { get; set; }
            public decimal Cash_On_Hand { get; set; }
            public bool Is_Blind { get; set; }
            public bool Ok => Result.Ok;
        }

        public class ReturnRow
        {
            public long Id { get; set; }
            public long Return_No { get; set; }
            public DateTime? Return_Date { get; set; }
            public long? Rep_Id { get; set; }
            public string Rep_Name { get; set; } = "";
            public string Customer_Name { get; set; } = "";
            public long? Original_Invoice_Id { get; set; }
            public long? Original_Invoice_No { get; set; }
            public bool Is_Blind { get; set; }
            public bool Has_Damaged { get; set; }
            public decimal Net_Total { get; set; }
            public int Is_Credit { get; set; }
        }

        // ============================== القراءة ==============================

        public List<ReasonRow> GetReasons() => Read(opp => opp.db.Lookup_SalesRep_ReturnReasons.AsNoTracking().Where(r => r.Active).OrderBy(r => r.Name)
            .Select(r => new ReasonRow { Id = r.Id, Name = r.Name, Is_Damaged = r.Is_Damaged, Requires_Photo = r.Requires_Photo, Requires_Batch = r.Requires_Batch }).ToList());

        /// <summary>فواتير بيع العميل فى آخر days يوماً التى بقى فيها مسموح بالإرجاع، الأحدث أولاً.</summary>
        public List<InvoicePick> GetCustomerInvoices(long repId, long customerId, int days = 90) => Read(opp =>
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            DateTime from = DateTime.Today.AddDays(-Math.Max(1, days));
            var rows = opp.db.Database.SqlQuery<InvoicePick>($@"
SELECT l.Id, CAST(ISNULL(l.Invoice_Id, 0) AS bigint) AS Invoice_No, l.Invoice_Date, CAST(ISNULL(l.Net_Total, 0) AS decimal(18,2)) AS Net_Total,
       ISNULL(l.Is_Invoice_Agile, 0) AS Is_Credit, 0 AS Age_Days, CAST(0 AS bit) AS Inside_Window
FROM Invoices_List l
WHERE l.Customer_Id = {customerId} AND ISNULL(l.Deleted, 0) = 0 AND l.Invoice_Date >= {from}
  AND EXISTS (SELECT 1 FROM Invoices_Details d
              LEFT JOIN Unit_item u ON u.id = d.Unit_Item_Table_Id
              CROSS APPLY (SELECT CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS Factor) f
              OUTER APPLY (SELECT SUM(k.Qty_Large) AS L, SUM(k.Qty_Small) AS S FROM SalesRep_Invoice_Return_Links k WHERE k.Original_Detail_Id = d.Id) k
              WHERE d.Invoice_Id = l.Id
                AND ISNULL(d.Larg_Qnty, 0) * f.Factor + ISNULL(d.Small_Qnty, 0) > ISNULL(k.L, 0) * f.Factor + ISNULL(k.S, 0))
ORDER BY l.Invoice_Date DESC, l.Id DESC").ToList();
            foreach (var r in rows)
            {
                r.Age_Days = Age(r.Invoice_Date);
                r.Inside_Window = r.Age_Days <= ctx.Return_Window_Days;
            }
            return rows;
        });

        public OriginalView GetOriginal(long repId, long invoiceId) => Read(opp => Original(opp, SalesRepSaleService.Context(opp, repId), invoiceId));

        /// <summary>سعر الأمان للمرتجع بلا فاتورة للوحدة الصغرى (0 = لا سعر بيع تاريخى للصنف فلا يُقبل بلا فاتورة).</summary>
        public decimal GetBlindPrice(long repId, long unitItemId) => Read(opp => BlindPrice(opp, SalesRepSaleService.Context(opp, repId), unitItemId));

        /// <summary>
        /// مسح باركود فى شاشة المرتجع: باركود التشغيلة (Unit_Item_ExpireDate.Tash3ela أو BatchNo) أو باركود الصنف (Unit_item.Code).
        /// المربوط: يرجع سطر الفاتورة الأصلية الذى بقى منه مسموح بإرجاعه (والتشغيلة لا تخالف تشغيلة السطر). بلا فاتورة: سطر الصنف
        /// فى مخزن السيارة (أو أى سطر للصنف). الكود غير الموجود استثناء برسالة.
        /// </summary>
        public ScanResult Scan(long repId, long? originalInvoiceId, string code) => Read(opp =>
        {
            code = (code ?? "").Trim();
            if (code.Length == 0) throw new InvalidOperationException("الكود فارغ.");
            var ctx = SalesRepSaleService.Context(opp, repId);
            var batches = opp.db.Database.SqlQuery<BatchRow>($@"
SELECT e.Id, CAST(ISNULL(e.Item_Id, 0) AS bigint) AS Item_Id, ISNULL(NULLIF(e.Tash3ela, N''), ISNULL(CAST(e.BatchNo AS nvarchar(30)), N'')) AS Label
FROM Unit_Item_ExpireDate e WHERE e.Tash3ela = {code} OR (e.BatchNo > 0 AND CAST(e.BatchNo AS nvarchar(30)) = {code})").ToList();
            var items = opp.db.Database.SqlQuery<long>($"SELECT DISTINCT CAST(Item_id AS bigint) AS Value FROM Unit_item WHERE Code = {code}").ToList();

            if (originalInvoiceId is > 0)
            {
                var lines = Original(opp, ctx, originalInvoiceId.Value).Lines.Where(x => x.Returnable_Small > 0).ToList();
                if (batches.Count > 0)
                {
                    var exact = lines.FirstOrDefault(x => batches.Any(b => b.Id == x.Batch_Id));
                    var open = exact ?? lines.FirstOrDefault(x => x.Batch_Id == null && batches.Any(b => b.Item_Id == x.Item_Id));
                    if (open != null)
                    {
                        var b = exact != null ? batches.First(b => b.Id == exact.Batch_Id) : batches.First(b => b.Item_Id == open.Item_Id);
                        return new ScanResult { Original_Detail_Id = open.Original_Detail_Id, Batch_Id = b.Id, Batch_Label = b.Label, Item_Name = open.Item_Name };
                    }
                    var other = lines.FirstOrDefault(x => batches.Any(b => b.Item_Id == x.Item_Id));
                    if (other != null) throw new InvalidOperationException($"التشغيلة {code} غير تشغيلة «{other.Item_Name}» فى الفاتورة الأصلية ({other.Batch_Label}).");
                }
                var line = lines.FirstOrDefault(x => items.Contains(x.Item_Id));
                if (line != null) return new ScanResult { Original_Detail_Id = line.Original_Detail_Id, Item_Name = line.Item_Name };
                throw new InvalidOperationException($"الكود {code} ليس لصنف بقى منه مسموح بإرجاعه فى الفاتورة الأصلية.");
            }

            var batch = batches.FirstOrDefault();
            if (batch == null && items.Count > 1) throw new InvalidOperationException($"الكود {code} مشترك بين {items.Count} صنفاً؛ اختر الصنف بالاسم.");
            long itemId = batch?.Item_Id ?? (items.Count > 0 ? items[0] : 0);
            if (itemId <= 0) throw new InvalidOperationException($"الكود {code} غير موجود.");
            long van = ctx.Van_Store_Id;
            var unit = opp.db.Database.SqlQuery<UnitPick>($@"
SELECT TOP 1 u.id AS Id, ISNULL(i.Item_Name, N'') AS Item_Name FROM Unit_item u JOIN Item i ON i.id = u.Item_id
WHERE u.Item_id = {itemId} ORDER BY CASE WHEN u.Store_id = {van} THEN 0 WHEN u.Code = {code} THEN 1 ELSE 2 END, u.id").AsEnumerable().FirstOrDefault()
                ?? throw new InvalidOperationException($"الكود {code} غير موجود.");
            return new ScanResult { Unit_Item_Id = unit.Id, Batch_Id = batch?.Id, Batch_Label = batch?.Label ?? "", Item_Name = unit.Item_Name };
        });

        public class BatchRow { public long Id { get; set; } public long Item_Id { get; set; } public string Label { get; set; } = ""; }
        public class UnitPick { public long Id { get; set; } public string Item_Name { get; set; } = ""; }

        // ============================== أسباب الإرجاع ==============================

        /// <summary>كل أسباب الإرجاع (النشطة والموقوفة) لشاشة إدارة الأسباب، مع عدد مرات الاستعمال.</summary>
        public List<ReasonRow> GetAllReasons() => Read(opp => opp.db.Database.SqlQuery<ReasonRow>($@"
SELECT r.Id, r.Name, r.Is_Damaged, r.Requires_Photo, r.Requires_Batch, r.Active,
       (SELECT COUNT(*) FROM SalesRep_Invoice_Return_Links k WHERE k.Reason_Id = r.Id) AS Used_Count
FROM Lookup_SalesRep_ReturnReasons r ORDER BY CASE WHEN r.Active = 1 THEN 0 ELSE 1 END, r.Name").ToList());

        /// <summary>
        /// يضيف سبباً (Id = 0) أو يعدّله. لا حذف: السبب يُوقف (Active = false) فيختفى من شاشة المرتجع ويبقى على المرتجعات القديمة.
        /// الاسم إلزامى وفريد. السبب التالف يرجع لمخزن التالف دائماً.
        /// </summary>
        public long SaveReason(ReasonRow input) => Tx(opp =>
        {
            string name = (input?.Name ?? "").Trim();
            if (name.Length == 0) throw new InvalidOperationException("اسم السبب مطلوب.");
            if (name.Length > 200) throw new InvalidOperationException("اسم السبب أطول من 200 حرف.");
            if (opp.db.Lookup_SalesRep_ReturnReasons.Any(r => r.Name == name && r.Id != input.Id)) throw new InvalidOperationException($"السبب «{name}» موجود.");
            Lookup_SalesRep_ReturnReason row;
            if (input.Id > 0)
            {
                row = opp.db.Lookup_SalesRep_ReturnReasons.FirstOrDefault(r => r.Id == input.Id) ?? throw new InvalidOperationException("السبب غير موجود.");
                if (row.Active && !input.Active && opp.db.Lookup_SalesRep_ReturnReasons.Count(r => r.Active) <= 1)
                    throw new InvalidOperationException("لا يمكن إيقاف آخر سبب نشط؛ المرتجع يحتاج سبباً.");
                row.StampEdit();
            }
            else
            {
                row = new Lookup_SalesRep_ReturnReason { Id = CallSnowflakeIdGenerate.GenerateNewId() };
                row.StampInsert(Common.Branch_Id);
                opp.db.Lookup_SalesRep_ReturnReasons.Add(row);
            }
            row.Name = name;
            row.Is_Damaged = input.Is_Damaged;
            row.Requires_Photo = input.Requires_Photo;
            row.Requires_Batch = input.Requires_Batch;
            row.Active = input.Active;
            opp.db.SaveChanges();
            return row.Id;
        });

        /// <summary>تقرير مرتجعات المندوبين (و«المرتجعات بلا فاتورة» بـ blindOnly) فى الفترة.</summary>
        public List<ReturnRow> GetReturns(DateTime from, DateTime to, long? repId = null, bool blindOnly = false) => Read(opp =>
        {
            DateTime f = from.Date, t = to.Date.AddDays(1);
            long rep = repId ?? 0;
            return opp.db.Database.SqlQuery<ReturnRow>($@"
SELECT r.Id, CAST(ISNULL(r.Invoice_Id, 0) AS bigint) AS Return_No, r.Invoice_Date AS Return_Date, s.Rep_Id, ISNULL(e.name, N'') AS Rep_Name,
       ISNULL(c.Name, N'') AS Customer_Name, o.Original_Invoice_Id, oi.Invoice_Id AS Original_Invoice_No,
       CAST(ISNULL(o.Is_Blind, 0) AS bit) AS Is_Blind, CAST(ISNULL(o.Has_Damaged, 0) AS bit) AS Has_Damaged,
       CAST(ISNULL(r.Net_Total, 0) AS decimal(18,2)) AS Net_Total, ISNULL(r.Is_Invoice_Agile, 0) AS Is_Credit
FROM SalesRep_Document_Links l
JOIN Invoices_List_Reverse r ON r.Id = l.Doc_Id
JOIN SalesRep_Shifts s ON s.Id = l.Shift_Id
LEFT JOIN SalesRep_Reps rp ON rp.Id = s.Rep_Id LEFT JOIN Employee e ON e.Id = rp.Employee_Id
LEFT JOIN Customer c ON c.Id = r.Customer_Id
OUTER APPLY (SELECT MAX(k.Original_Invoice_Id) AS Original_Invoice_Id, MAX(CAST(k.Is_Blind AS int)) AS Is_Blind, MAX(k.Condition) AS Has_Damaged
             FROM SalesRep_Invoice_Return_Links k JOIN Invoices_Details_Reverse d ON d.Id = k.Return_Detail_Id WHERE d.Invoice_Id = r.Id) o
LEFT JOIN Invoices_List oi ON oi.Id = o.Original_Invoice_Id
WHERE l.Doc_Type = {(int)SalesRepDocType.SaleReturn} AND r.Invoice_Date >= {f} AND r.Invoice_Date < {t}
  AND ({rep} = 0 OR s.Rep_Id = {rep}) AND ({(blindOnly ? 1 : 0)} = 0 OR ISNULL(o.Is_Blind, 0) = 1)
ORDER BY r.Invoice_Date DESC, r.Id DESC").ToList();
        });

        // ============================== الفحص والموافقات ==============================

        /// <summary>يبنى المرتجع ويفحصه بلا حفظ؛ المنع القاطع استثناء، والخروج عن نافذة الإرجاع مخالفة تحتاج موافقة.</summary>
        public ReturnCheck Check(long repId, ReturnInput input) => Read(opp =>
        {
            var b = Build(opp, SalesRepSaleService.Context(opp, repId), input);
            return b.Check;
        });

        public ReturnCheck RequestApprovals(long repId, ReturnInput input, string reason = null) => Tx(opp =>
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            var b = Build(opp, ctx, input);
            foreach (var v in b.Check.Result.Violations.Where(v => v.Approval_Status != (int)SalesRepApprovalStatus.Pending && !v.Covered))
            {
                var a = new SalesRep_Approval
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(), Type = (int)v.Type, Ref_Document_Guid = input.Doc_Guid, Ref_Doc_Type = (int)SalesRepDocType.SaleReturn,
                    Requested_By = Common.User_Id, Status = (int)SalesRepApprovalStatus.Pending,
                    Reason = SalesRepSaleService.Trim((string.IsNullOrWhiteSpace(reason) ? "" : reason.Trim() + " — ") + v.Message, 1000),
                    Rep_Id = ctx.Rep_Id, Shift_Id = ctx.Shift_Id, Customer_Id = input.Customer_Id, Requested_Value = v.Requested, Allowed_Value = v.Allowed,
                };
                a.StampInsert(ctx.Branch_Id);
                opp.db.SalesRep_Approvals.Add(a);
            }
            opp.db.SaveChanges();
            return Build(opp, ctx, input).Check;
        });

        // ============================== الحفظ ==============================

        /// <summary>يحفظ المرتجع بمحرك المرتجع ويربط أسطره بأسطر الفاتورة الأصلية وبالوردية. يرجع Invoices_List_Reverse.Id.</summary>
        public long SaveReturn(long repId, ReturnInput input) => Tx(opp =>
        {
            var ctx = SalesRepSaleService.Context(opp, repId);
            if (input.Doc_Guid == Guid.Empty) throw new InvalidOperationException("معرّف المستند مطلوب.");
            var b = Build(opp, ctx, input);
            if (!b.Check.Ok) throw new SalesRepApprovalRequiredException(b.Check.Result.Violations.Where(v => !v.Covered).ToList());

            // سطر الصنف فى مخزن الوجهة (السيارة/التالف) يُنشأ بلا رصيد لو غير موجود، كما يفعل التحويل.
            var orders = new StoreOrderService(opp);
            foreach (var l in b.Lines)
            {
                long target = l.Input.Condition == (int)SalesRepReturnCondition.Damaged ? ctx.Damaged_Store_Id : ctx.Van_Store_Id;
                long line = orders.EnsureStoreLine(l.Source_Unit_Item_Id, target);
                l.Detail.Unit_Item_Table_Id = line;
                l.Detail.Store_Id = target;
                l.Detail.Store_Name = target == ctx.Damaged_Store_Id ? ctx.Damaged_Store_Name : ctx.Van_Store_Name;
                // التشغيلة تخص سطر المخزن الأصلى؛ تبقى فقط لو رجع الصنف لنفس السطر (وتُحفظ للمراجعة فى صف الربط دائماً).
                if (line != l.Source_Unit_Item_Id) l.Detail.Unit_Item_ExpireDate_Id = 0;
            }

            var h = b.InvL.invL;
            h.Invoice_Id = opp.db.Database.SqlQuery<long>($@"
SELECT CAST(ISNULL(MAX(Invoice_Id), 0) + 1 AS bigint) AS Value FROM Invoices_List_Reverse WITH (UPDLOCK, HOLDLOCK) WHERE Invoice_Type = {h.Invoice_Type ?? 0}").AsEnumerable().First();
            decimal before = SalesRepSaleService.CustomerBalance(opp, input.Customer_Id);
            h.CustomerAccountBefor = before;
            h.CustomerAccountAfter = before - (h.Net_Total ?? 0);

            int excp = 0;
            string msg = "";
            new Requests_Cls().Make_Invoice_Reverse_Update_CustomerAccountInTx(ref excp, ref msg, ref opp, b.InvL, b.InvD);
            long returnId = h.Id;
            if (excp != 0 || returnId <= 0 || b.InvD.invDlst.Any(d => d.Id <= 0))
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(msg) ? "تعذر حفظ المرتجع." : msg);
            // المجانى المرتجع: المحرك يضيفه لـ Unit_item ويكتب صف دفتره (FreeQtyLedgerCLS) فيدخل «المرتجع» ورصيد المخزن المحسوب.

            foreach (var l in b.Lines)
            {
                var link = new SalesRep_Invoice_Return_Link
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(), Return_Detail_Id = l.Detail.Id, Original_Detail_Id = l.Original_Detail_Id,
                    Original_Invoice_Id = input.Original_Invoice_Id, Is_Blind = l.Original_Detail_Id == null,
                    Qty_Large = l.Detail.Larg_Qnty ?? 0, Qty_Small = l.Detail.Small_Qnty ?? 0, Qty_ItemNo = l.Detail.Qnty_Another ?? 0,
                    Net_Unit_Price = l.Net_Unit_Price, Unit_Cost = l.Detail.P_priceAvgCost, Reason_Id = l.Input.Reason_Id, Condition = l.Input.Condition,
                    Unit_Item_ExpireDate_Id = l.Batch_Id is > 0 ? l.Batch_Id : null,
                };
                link.StampInsert(ctx.Branch_Id);
                if (l.Input.Photo is { Length: > 0 })
                {
                    var photo = new SalesRep_Attachment
                    {
                        Id = CallSnowflakeIdGenerate.GenerateNewId(), Ref_Type = (int)SalesRepAttachmentRefType.Return, Ref_Id = link.Id,
                        File_Data = l.Input.Photo, Content_Type = SalesRepSaleService.Trim(l.Input.Photo_Content_Type ?? "image/jpeg", 100), Taken_At = DateTime.Now,
                    };
                    photo.StampInsert(ctx.Branch_Id);
                    opp.db.SalesRep_Attachments.Add(photo);
                    link.Photo_Id = photo.Id;
                }
                opp.db.SalesRep_Invoice_Return_Links.Add(link);
            }
            opp.db.SaveChanges();

            SalesRepSaleService.Link(opp, ctx.Shift_Id.Value, SalesRepDocType.SaleReturn, returnId, ctx.Branch_Id);
            // الرد النقدى من صندوق العهدة: لو لم ينشئ المحرك إذنه (MakeOrderMoneyFromInvoiceAutomaticWithoutForm = 0) يُنشأ إذن صرف مربوط بالمرتجع.
            decimal net = Math.Round(h.Net_Total ?? 0, 2);
            if (input.Refund_Cash && net > 0 && VouchersOfReturn(opp, returnId).Count == 0)
            {
                var vouchers = new CashVouchersService();
                long cur = opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(currency_Id, 1) AS bigint) AS Value FROM Banks WHERE Id = {ctx.Cash_Box_Id}").AsEnumerable().FirstOrDefault();
                // صرف لعميل = نوع 2 عكسى (Reverse_Order = 1): يُسجَّل على حساب العميل لا المورد.
                vouchers.SavePartyVoucherInTx(opp, 2, 1, ctx.Cash_Box_Id, (h.Invoice_Date ?? DateTime.Today).Date, 0, "رد نقدى لمرتجع المندوب " + ctx.Rep_Name,
                    net, input.Customer_Id, vouchers.GetFactor(cur), Common.InvoiceReverse_Type_Id, returnId);
            }
            var docs = new List<(SalesRepDocType, long)> { (SalesRepDocType.SaleReturn, returnId) };
            foreach (long orderMoneyId in VouchersOfReturn(opp, returnId))
            {
                SalesRepSaleService.Link(opp, ctx.Shift_Id.Value, SalesRepDocType.CashVoucher, orderMoneyId, ctx.Branch_Id);
                docs.Add((SalesRepDocType.CashVoucher, orderMoneyId));
            }
            SalesRepVisitService.Attach(opp, ctx.Shift_Id.Value, input.Customer_Id, SalesRepVisitResult.Return, docs.ToArray());

            var used = opp.db.SalesRep_Approvals.Where(a => a.Ref_Document_Guid == input.Doc_Guid && a.Status == (int)SalesRepApprovalStatus.Approved && a.Ref_Doc_Id == null).ToList();
            foreach (var a in used) { a.Ref_Doc_Id = returnId; a.Shift_Id ??= ctx.Shift_Id; a.StampEdit(); }
            opp.db.SaveChanges();
            return returnId;
        });

        // ============================== البناء ==============================

        private class BuiltLine
        {
            public ReturnLineInput Input { get; set; }
            public IInvoices_Details Detail { get; set; }
            public long? Original_Detail_Id { get; set; }
            public long Source_Unit_Item_Id { get; set; }
            public long? Batch_Id { get; set; }
            public decimal Net_Unit_Price { get; set; }
            public string Name { get; set; } = "";
            public decimal Qty_Small { get; set; }
            public decimal Free_Small { get; set; }
            public decimal Factor { get; set; } = 1;
        }

        private class Built
        {
            public InvoicesOperationListCLS InvL { get; set; }
            public InvoicesOperationCLS InvD { get; set; }
            public List<BuiltLine> Lines { get; set; } = new();
            public ReturnCheck Check { get; set; } = new();
        }

        /// <summary>حقول السطر التى تتناسب مع الكمية؛ تُقسم بتقريب تراكمى. الأسعار والنسب تبقى كما فى السطر الأصلى.</summary>
        private static readonly string[] ScaledMoney =
        {
            nameof(IInvoices_Details.Qnty_value), nameof(IInvoices_Details.Total), nameof(IInvoices_Details.Total3Mola),
            nameof(IInvoices_Details.ItemCommissionTotal), nameof(IInvoices_Details.Net_Total_Item_Commission), nameof(IInvoices_Details.Item_CommissionValue),
            nameof(IInvoices_Details.Alternate_ItemCommissionTotal), nameof(IInvoices_Details.Alternate_Item_CommissionValue),
            nameof(IInvoices_Details.Item_Total_WithoutTax), nameof(IInvoices_Details.Item_TaxSales10_Value), nameof(IInvoices_Details.Item_ATS05_Value),
            nameof(IInvoices_Details.TaxSales10_Value), nameof(IInvoices_Details.ATS05_Value), nameof(IInvoices_Details.Item_Net_Total_Price),
        };

        private static readonly string[] ScaledQty = { nameof(IInvoices_Details.Qnty_Another), nameof(IInvoices_Details.QntyItemNo) };

        private Built Build(operations opp, SalesRepSaleService.SaleContext ctx, ReturnInput input)
        {
            if (!ctx.CanSell) throw new InvalidOperationException("لا وردية مفتوحة للمندوب؛ المرتجع يتم داخل وردية مفتوحة فقط.");
            if (input.Customer_Id <= 0) throw new InvalidOperationException("اختر العميل.");
            var lines = input.Lines.Where(l => l.Qty_Large > 0 || l.Qty_Small > 0).ToList();
            if (lines.Count == 0) throw new InvalidOperationException("حدد كمية مرتجعة لصنف واحد على الأقل.");
            if (lines.Any(l => l.Qty_Large < 0 || l.Qty_Small < 0)) throw new InvalidOperationException("الكمية لا تكون سالبة.");
            bool blind = input.Original_Invoice_Id is not > 0;
            if (blind && !ctx.Allow_Blind_Return) throw new InvalidOperationException("المندوب غير مسموح له بالمرتجع بلا فاتورة؛ اختر الفاتورة الأصلية.");

            var reasons = opp.db.Lookup_SalesRep_ReturnReasons.AsNoTracking().ToList().ToDictionary(r => r.Id);
            foreach (var l in lines)
            {
                if (!reasons.TryGetValue(l.Reason_Id, out var reason) || !reason.Active) throw new InvalidOperationException("سبب الإرجاع مطلوب لكل صنف.");
                if (!Enum.IsDefined(typeof(SalesRepReturnCondition), l.Condition)) throw new InvalidOperationException("حالة الصنف المرتجع غير صحيحة.");
                if (reason.Is_Damaged && l.Condition != (int)SalesRepReturnCondition.Damaged) throw new InvalidOperationException($"السبب «{reason.Name}» يعنى صنفاً تالفاً؛ يرجع لمخزن التالف.");
                if (reason.Requires_Photo && !(l.Photo is { Length: > 0 })) throw new InvalidOperationException($"السبب «{reason.Name}» يتطلب صورة للصنف.");
                if (l.Photo is { Length: > MaxPhotoBytes }) throw new InvalidOperationException("حجم الصورة أكبر من 5 ميجابايت.");
            }

            var invL = new InvoicesOperationListCLS(1);
            var invD = new InvoicesOperationCLS(1);
            var b = new Built { InvL = invL, InvD = invD };
            var h = invL.invL;
            var customer = opp.db.Database.SqlQuery<CustomerRow>($"SELECT Name, CAST(ISNULL(SubAccount_Id, 0) AS bigint) AS SubAccount_Id FROM Customer WHERE Id = {input.Customer_Id}")
                .AsEnumerable().FirstOrDefault() ?? throw new InvalidOperationException("العميل غير موجود.");
            DateTime now = DateTime.Now;
            decimal headerDiscount = 0;
            Invoices_List orig = null;

            if (!blind)
            {
                orig = opp.db.Invoices_List.AsNoTracking().FirstOrDefault(i => i.Id == input.Original_Invoice_Id) ?? throw new InvalidOperationException("الفاتورة الأصلية غير موجودة.");
                if ((orig.Deleted ?? 0) == 1) throw new InvalidOperationException("الفاتورة الأصلية محذوفة.");
                if (orig.Customer_Id != input.Customer_Id) throw new InvalidOperationException("الفاتورة الأصلية لعميل آخر.");
                if (lines.Any(l => l.Original_Detail_Id is not > 0)) throw new InvalidOperationException("كل أصناف المرتجع من أسطر الفاتورة الأصلية.");
                if (lines.GroupBy(l => l.Original_Detail_Id).Any(g => g.Count() > 1)) throw new InvalidOperationException("سطر الفاتورة مكرر فى المرتجع.");

                var view = Original(opp, ctx, orig.Id);
                var details = opp.db.Invoices_Details.AsNoTracking().Where(d => d.Invoice_Id == orig.Id).ToList().ToDictionary(d => d.Id);
                decimal origTotal = details.Values.Sum(d => d.Qnty_value ?? 0);
                decimal origDiscount = orig.List_Final_CommissionValue ?? 0;
                foreach (var l in lines)
                {
                    var rl = view.Lines.FirstOrDefault(x => x.Original_Detail_Id == l.Original_Detail_Id) ?? throw new InvalidOperationException("سطر ليس من الفاتورة الأصلية.");
                    var od = details[rl.Original_Detail_Id];
                    decimal qty = l.Qty_Large * rl.Factor + l.Qty_Small;
                    if (qty > rl.Returnable_Small)
                        throw new InvalidOperationException($"الكمية المرتجعة من «{rl.Item_Name}» ({qty:0.##}) أكبر من المسموح بإرجاعه ({rl.Returnable_Small:0.##}).");

                    var d = MappingHelper.MapScalarProperties<IInvoices_Details, Invoices_Details>(od);
                    decimal sold = rl.Sold_Small, prev = rl.Returned_Small;
                    foreach (var name in ScaledMoney) Scale(d, od, name, sold, prev, qty, 2);
                    foreach (var name in ScaledQty) Scale(d, od, name, sold, prev, qty, 4);
                    headerDiscount += Part(origTotal > 0 ? origDiscount * (od.Qnty_value ?? 0) / origTotal : 0, sold, prev, qty, 2);
                    decimal free = FreeToReturn(rl, qty);
                    decimal freeItemNo = Part(od.Item_No_Free ?? 0, sold, prev, qty, 4);
                    long? batch = LineBatch(opp, l, rl.Item_Id, rl.Batch_Id, reasons[l.Reason_Id], rl.Item_Name, rl.Item_Has_Batches);
                    PrepareLine(d, rl.Factor, qty, now, free, freeItemNo);
                    b.Lines.Add(new BuiltLine
                    {
                        Input = l, Detail = d, Original_Detail_Id = od.Id, Source_Unit_Item_Id = od.Unit_Item_Table_Id ?? 0, Batch_Id = batch,
                        Net_Unit_Price = rl.Net_Unit_Price, Name = rl.Item_Name, Qty_Small = qty, Free_Small = free, Factor = rl.Factor,
                    });
                    invD.invDlst.Add(d);
                }

                h.IsInvoiceTaxInfo = orig.IsInvoiceTaxInfo ?? 0;
                h.Is_Tax_Per_Item = orig.Is_Tax_Per_Item ?? 0;
                h.TaxSales10 = orig.TaxSales10 ?? 0;
                h.ATS05 = orig.ATS05 ?? 0;
                h.IsMaintainceInvoice = orig.IsMaintainceInvoice;
                h.Currency_id = orig.Currency_id;
                h.Currency = orig.Currency ?? "";

                // نافذة الإرجاع: خارجها = موافقة المشرف.
                if (!view.Inside_Window)
                    b.Check.Result.Violations.Add(new SalesRepSaleService.Violation
                    {
                        Type = SalesRepApprovalType.OutsideReturnWindow, Requested = view.Age_Days, Allowed = view.Window_Days,
                        Message = $"الفاتورة {orig.Invoice_Id} عمرها {view.Age_Days} يوماً (نافذة الإرجاع {view.Window_Days})",
                    });
            }
            else
            {
                if (lines.Any(l => l.Unit_Item_Id is not > 0 || l.Original_Detail_Id != null)) throw new InvalidOperationException("حدد الصنف لكل سطر فى المرتجع بلا فاتورة.");
                if (lines.GroupBy(l => l.Unit_Item_Id).Any(g => g.Count() > 1)) throw new InvalidOperationException("الصنف مكرر فى المرتجع.");
                long branch = ctx.Branch_Id;
                foreach (var l in lines)
                {
                    var src = opp.db.Database.SqlQuery<BlindSourceRow>($@"
SELECT u.id AS Unit_Item_Id, CAST(u.Item_id AS bigint) AS Item_Id, ISNULL(i.Item_Name, N'') AS Item_Name, ISNULL(u.Code, ISNULL(i.code, N'')) AS Code,
       CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor, CAST(ISNULL(u.unit_id, 0) AS bigint) AS Unit_Id,
       ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit
FROM Unit_item u JOIN Item i ON i.id = u.Item_id LEFT JOIN unit un ON un.id = u.unit_id WHERE u.id = {l.Unit_Item_Id}").AsEnumerable().FirstOrDefault()
                        ?? throw new InvalidOperationException("الصنف غير موجود.");
                    decimal price = BlindPrice(opp, ctx, src.Unit_Item_Id);
                    if (price <= 0) throw new InvalidOperationException($"لا سعر بيع سابق للصنف «{src.Item_Name}»؛ لا يُقبل بلا فاتورة.");
                    decimal qty = l.Qty_Large * src.Factor + l.Qty_Small;
                    decimal cost = opp.db.Item_Branch_Costs.AsNoTracking().Where(c => c.Item_Id == src.Item_Id && c.Branch_Id == branch).Select(c => (decimal?)c.Avg_Cost).FirstOrDefault() ?? 0;
                    decimal value = Math.Round(qty * price, 2);
                    var d = new IInvoices_Details
                    {
                        Item_Id = src.Item_Id, Invoice_Item_Name = src.Item_Name, code = src.Code, Unit_Id = src.Unit_Id, UnitStr = src.B_Unit, UnitSmallStr = src.S_Unit,
                        Unit_Used_for_Price = 2, PriceUnit = price, Small_Unit_Price = price, Larg_Unit_Price = Math.Round(price * src.Factor, 4),
                        Qnty_value = value, Total = value, Item_Total_WithoutTax = value, Item_Net_Total_Price = value,
                        P_priceAvgCost = cost, Purchase_Price = cost, Make_Order = 0, Bills_Details_Id = 0, Unit_Item_ExpireDate_Id = 0,
                        Item_Expire_Date = now.Date, Tash3ela = "", StatementNo = "", OperationNo = "", Merchant_Name = "", Specification = "", NameSpecification = "",
                        Item_Notes = "مرتجع بلا فاتورة بسعر الأمان", Notes = "",
                    };
                    bool itemHasBatches = opp.db.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Unit_Item_ExpireDate WHERE Item_Id = {src.Item_Id}").AsEnumerable().First() > 0;
                    long? batch = LineBatch(opp, l, src.Item_Id, null, reasons[l.Reason_Id], src.Item_Name, itemHasBatches);
                    PrepareLine(d, src.Factor, qty, now, 0, 0);
                    b.Lines.Add(new BuiltLine { Input = l, Detail = d, Source_Unit_Item_Id = src.Unit_Item_Id, Net_Unit_Price = price, Name = src.Item_Name, Qty_Small = qty, Batch_Id = batch, Factor = src.Factor });
                    invD.invDlst.Add(d);
                }
                h.IsInvoiceTaxInfo = 0; h.Is_Tax_Per_Item = 0; h.TaxSales10 = 0; h.ATS05 = 0;
                h.Currency_id = opp.db.Database.SqlQuery<long>($"SELECT CAST(ISNULL(currency_Id, 1) AS bigint) AS Value FROM Banks WHERE Id = {ctx.Cash_Box_Id}").AsEnumerable().FirstOrDefault();
                h.Currency = "";
            }

            // ---- الرأس ----
            h.Invoice_Type = 0;
            h.Is_InvoiceKta3y = 0;
            h.IS_Make_Sarf_Item_BalanceFromInvoice = 1;
            h.Is_Invoice_Agile = input.Refund_Cash ? 0 : 1;
            h.BanksId = input.Refund_Cash ? ctx.Cash_Box_Id : 0;
            h.Customer_Id = input.Customer_Id;
            h.CustomerName = customer.Name ?? "";
            h.SubAccount_Id = customer.SubAccount_Id;
            h.SubAccount_Name = h.CustomerName;
            h.Salesman_Id = ctx.Employee_Id;
            h.Branch_Id = ctx.Branch_Id;
            h.User_Id = Common.User_Id;
            h.Invoice_Date = now; h.Invoice_DateTime = now; h.InvoiceDueDate = now; h.Insert_ServerDateTime = now; h.OperationDate = now.Date;
            string origNote = orig == null ? "مرتجع بلا فاتورة" : $"مرتجع من الفاتورة رقم {orig.Invoice_Id}";
            h.Notes = SalesRepSaleService.Trim(string.IsNullOrWhiteSpace(input.Notes) ? origNote : origNote + " — " + input.Notes.Trim(), 500);
            h.PolicaNo = "0"; h.troodContainerNo = "0"; h.Customer_Notes = ""; h.Emp_Name = ""; h.ShipmentCompany_Name = "";
            h.Emp_Id = 0; h.ShipmentCompany_Id = 0; h.Shipment_Value = 0; h.AnotherPayment = 0; h.IS_Posted_Flage = 0;
            h.List_CommissionRate = 0;
            h.List_CommissionValue = headerDiscount;
            ComputeHeader(h, invD.invDlst, h.IsInvoiceTaxInfo == 1 && h.Is_Tax_Per_Item == 1);
            ZeroNulls(h);
            invL.Invoice_Type_Id = Common.InvoiceReverse_Type_Id;
            invL.cs.Name = h.CustomerName;

            // ---- الرد النقدى: لا يتجاوز النقدية بالعهدة ----
            decimal net = Math.Round(h.Net_Total ?? 0, 2);
            if (input.Refund_Cash && net > ctx.Cash_On_Hand)
                throw new InvalidOperationException($"الرد النقدى {net:N2} أكبر من النقدية بالعهدة {ctx.Cash_On_Hand:N2}؛ اجعل المرتجع على حساب العميل.");

            b.Check.Net_Total = net;
            b.Check.Cash_On_Hand = ctx.Cash_On_Hand;
            b.Check.Is_Blind = blind;
            b.Check.Lines = b.Lines.Select(l => new PreviewLine
            {
                Item_Name = l.Name, Qty_Small = l.Qty_Small, Free_Small = l.Free_Small, Net_Unit_Price = l.Net_Unit_Price, Condition = l.Input.Condition,
                Value = Math.Round(l.Detail.Item_Net_Total_Price ?? 0, 2),
            }).ToList();
            ApprovalStatus(opp, input.Doc_Guid, b.Check.Result);
            return b;
        }

        /// <summary>قيمة جزء الكمية qty بعد prev من sold، بتقريب تراكمى.</summary>
        private static decimal Part(decimal full, decimal sold, decimal prev, decimal qty, int digits)
        {
            if (sold <= 0) return 0;
            return Math.Round(full * (prev + qty) / sold, digits, MidpointRounding.AwayFromZero) - Math.Round(full * prev / sold, digits, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// المجانى المرتجع مع qty من المدفوع: بنسبة المدفوع بتقريب تراكمى (لوحدة صغرى كاملة إن كان المجانى صحيحاً) مطروحاً منه
        /// المرتجع فعلاً من المجانى، فمجموع المرتجعات عند إرجاع السطر كله = المجانى الأصلى بالضبط.
        /// </summary>
        internal static decimal FreeToReturn(ReturnableLine rl, decimal qty)
        {
            decimal full = rl.Free_Small, left = Math.Max(0, full - rl.Free_Returned_Small);
            if (full <= 0 || rl.Sold_Small <= 0 || left <= 0) return 0;
            if (rl.Returned_Small + qty >= rl.Sold_Small) return left;
            int digits = full == Math.Floor(full) ? 0 : 4;
            decimal target = Math.Round(full * (rl.Returned_Small + qty) / rl.Sold_Small, digits, MidpointRounding.AwayFromZero);
            return Math.Clamp(target - rl.Free_Returned_Small, 0, left);
        }

        /// <summary>
        /// تشغيلة السطر: الممسوحة (لنفس الصنف، ولا تخالف تشغيلة سطر الفاتورة) أو تشغيلة الفاتورة. السبب الذى يتطلب التشغيلة
        /// يفرضها للصنف الذى له تشغيلات مسجلة.
        /// </summary>
        private static long? LineBatch(operations opp, ReturnLineInput l, long itemId, long? original, Lookup_SalesRep_ReturnReason reason, string itemName, bool itemHasBatches)
        {
            long? batch = original is > 0 ? original : null;
            if (l.Batch_Id is > 0)
            {
                long? owner = opp.db.Database.SqlQuery<long?>($"SELECT CAST(Item_Id AS bigint) AS Value FROM Unit_Item_ExpireDate WHERE Id = {l.Batch_Id.Value}").AsEnumerable().FirstOrDefault();
                if (owner != itemId) throw new InvalidOperationException($"التشغيلة الممسوحة ليست للصنف «{itemName}».");
                if (batch != null && batch != l.Batch_Id) throw new InvalidOperationException($"التشغيلة الممسوحة للصنف «{itemName}» غير تشغيلة الفاتورة الأصلية.");
                batch = l.Batch_Id;
            }
            if (reason.Requires_Batch && itemHasBatches && batch == null)
                throw new InvalidOperationException($"السبب «{reason.Name}» يتطلب التشغيلة؛ امسح باركود تشغيلة «{itemName}».");
            return batch;
        }

        private static void Scale(IInvoices_Details target, Invoices_Details source, string name, decimal sold, decimal prev, decimal qty, int digits)
        {
            var p = typeof(IInvoices_Details).GetProperty(name);
            decimal full = (decimal?)p.GetValue(source) ?? 0;
            p.SetValue(target, Part(full, sold, prev, qty, digits));
        }

        /// <summary>الكمية والقيم الثابتة لسطر المرتجع: المدفوع والمجانى المرتجع معه (بلا قيمة)، وأعمدة التدقيق للمستخدم الحالى.</summary>
        private static void PrepareLine(IInvoices_Details d, decimal factor, decimal qtySmall, DateTime now, decimal freeSmall, decimal freeItemNo)
        {
            d.Id = 0; d.Invoice_Id = 0;
            d.Larg_Qnty = Math.Floor(qtySmall / factor);
            d.Small_Qnty = qtySmall - d.Larg_Qnty * factor;
            d.TotalSmall_Qnty = qtySmall;
            d.Qnty = Math.Round(qtySmall / factor, 4);
            d.LargItemsQnty_Free = Math.Floor(freeSmall / factor);
            d.SmallItemsQnty_Free = freeSmall - d.LargItemsQnty_Free * factor;
            d.Qnty_Free = Math.Round(freeSmall / factor, 4);
            d.Item_No_Free = freeItemNo;
            d.Order_Purshing_Id_Automatic = null; d.CarsWorkOrdersItemProcessedDetailsId = null; d.AppointmentServicesId = null; d.InvoiceKitchenStatusId = null;
            d.Item_DeletedFlage = 0; d.Item_ProcessedFlage = 0; d.Make_Order = 0; d.Balance_Checked = 0;
            d.User_Id = Common.User_Id; d.Branch_Id = Common.Branch_Id; d.OperationDate = now.Date; d.Insert_ServerDateTime = now;
            d.Update_ServerDateTime = null; d.EditOperationDate = null; d.Edit_User_Id = null;
            d.Notes ??= ""; d.Specification ??= ""; d.NameSpecification ??= ""; d.Tash3ela ??= ""; d.StatementNo ??= ""; d.OperationNo ??= ""; d.Merchant_Name ??= "";
            d.Item_Expire_Date ??= now.Date;
            foreach (var p in typeof(IInvoices_Details).GetProperties().Where(p => p.PropertyType == typeof(decimal?) && p.CanWrite))
                if (p.GetValue(d) is null) p.SetValue(d, 0m);
        }

        /// <summary>إجماليات الرأس بنفس قواعد InvoiceLineCalculator.ComputeHeaderTotals (شاشة الفاتورة): خصم الرأس قيمة ثابتة هنا.</summary>
        private static void ComputeHeader(IInvoices_List h, List<IInvoices_Details> lines, bool taxPerItem)
        {
            decimal total = lines.Sum(l => l.Qnty_value ?? 0), commission = lines.Sum(l => l.Net_Total_Item_Commission ?? 0), mola = lines.Sum(l => l.Total3Mola ?? 0);
            h.Total = total;
            h.Qnty_Value = total;
            h.Total_Item_CommissionValue = commission;
            h.List_Final_CommissionValue = h.List_CommissionValue ?? 0;
            h.Net_Total_CommissionValue = h.List_Final_CommissionValue + commission;
            h.Total_PriceWithoutTax = total - h.Net_Total_CommissionValue;
            if (taxPerItem)
            {
                h.TaxSales10_Value = lines.Sum(l => l.Item_TaxSales10_Value ?? 0);
                h.ATS05_Value = lines.Sum(l => l.Item_ATS05_Value ?? 0);
            }
            else
            {
                h.TaxSales10_Value = Common.RoundMePositive((h.Total_PriceWithoutTax ?? 0) * (h.TaxSales10 ?? 0) / 100);
                h.ATS05_Value = Common.RoundMePositive((h.Total_PriceWithoutTax ?? 0) * (h.ATS05 ?? 0) / 100);
            }
            h.Total_List3Mola = mola;
            decimal net = (h.Total_PriceWithoutTax ?? 0) + (h.TaxSales10_Value ?? 0) - (h.ATS05_Value ?? 0) + mola;
            h.Net_Total = Common.RoundMePositive(net < 0 ? 0 : net);
            h.NetInvoiceTotal = h.Net_Total;
            h.Equivalent_Net_Total_Price = h.Net_Total;
            h.Equivalent_Net_Total_CommissionValue = h.Net_Total_CommissionValue;
            h.Total_PriceWithoutTax = Common.RoundMePositive(h.Total_PriceWithoutTax ?? 0);
        }

        private static void ZeroNulls(IInvoices_List h)
        {
            foreach (var p in typeof(IInvoices_List).GetProperties().Where(p => p.PropertyType == typeof(decimal?) && p.CanWrite))
                if (p.GetValue(h) is null) p.SetValue(h, 0m);
        }

        internal static OriginalView Original(operations opp, SalesRepSaleService.SaleContext ctx, long invoiceId)
        {
            var inv = opp.db.Database.SqlQuery<OriginalView>($@"
SELECT l.Id, CAST(ISNULL(l.Invoice_Id, 0) AS bigint) AS Invoice_No, l.Invoice_Date, CAST(ISNULL(l.Customer_Id, 0) AS bigint) AS Customer_Id,
       ISNULL(c.Name, N'') AS Customer_Name, CAST(ISNULL(l.Net_Total, 0) AS decimal(18,2)) AS Net_Total,
       CAST(CASE WHEN ISNULL(l.Is_Invoice_Agile, 0) = 1 THEN 1 ELSE 0 END AS bit) AS Is_Credit, 0 AS Age_Days, 0 AS Window_Days
FROM Invoices_List l LEFT JOIN Customer c ON c.Id = l.Customer_Id WHERE l.Id = {invoiceId}").AsEnumerable().FirstOrDefault()
                ?? throw new InvalidOperationException("الفاتورة الأصلية غير موجودة.");
            inv.Age_Days = Age(inv.Invoice_Date);
            inv.Window_Days = ctx.Return_Window_Days;
            inv.Lines = opp.db.Database.SqlQuery<ReturnableLine>($@"
SELECT d.Id AS Original_Detail_Id, CAST(ISNULL(d.Unit_Item_Table_Id, 0) AS bigint) AS Unit_Item_Id, CAST(ISNULL(d.Item_Id, 0) AS bigint) AS Item_Id,
       ISNULL(NULLIF(d.Invoice_Item_Name, N''), ISNULL(i.Item_Name, N'')) AS Item_Name, ISNULL(d.code, ISNULL(i.code, N'')) AS Code,
       ISNULL(un.B_Unit, N'') AS B_Unit, ISNULL(un.S_unit, N'') AS S_Unit, f.Factor,
       CAST(ISNULL(d.Larg_Qnty, 0) * f.Factor + ISNULL(d.Small_Qnty, 0) AS decimal(18,4)) AS Sold_Small,
       CAST(ISNULL((SELECT SUM(k.Qty_Large) FROM SalesRep_Invoice_Return_Links k WHERE k.Original_Detail_Id = d.Id), 0) * f.Factor
            + ISNULL((SELECT SUM(k.Qty_Small) FROM SalesRep_Invoice_Return_Links k WHERE k.Original_Detail_Id = d.Id), 0) AS decimal(18,4)) AS Returned_Small,
       CAST(0 AS decimal(18,6)) AS Net_Unit_Price,
       CAST(CASE WHEN ISNULL(d.Unit_Item_ExpireDate_Id, 0) > 0 THEN 1 ELSE 0 END AS bit) AS Has_Batch,
       CAST(ISNULL(d.LargItemsQnty_Free, 0) * f.Factor + ISNULL(d.SmallItemsQnty_Free, 0) AS decimal(18,4)) AS Free_Small,
       CAST(ISNULL((SELECT SUM(ISNULL(r.LargItemsQnty_Free, 0)) FROM SalesRep_Invoice_Return_Links k JOIN Invoices_Details_Reverse r ON r.Id = k.Return_Detail_Id
                    WHERE k.Original_Detail_Id = d.Id), 0) * f.Factor
          + ISNULL((SELECT SUM(ISNULL(r.SmallItemsQnty_Free, 0))
                    FROM SalesRep_Invoice_Return_Links k JOIN Invoices_Details_Reverse r ON r.Id = k.Return_Detail_Id
                    WHERE k.Original_Detail_Id = d.Id), 0) AS decimal(18,4)) AS Free_Returned_Small,
       NULLIF(CAST(ISNULL(d.Unit_Item_ExpireDate_Id, 0) AS bigint), 0) AS Batch_Id,
       ISNULL(NULLIF(e.Tash3ela, N''), ISNULL(CAST(e.BatchNo AS nvarchar(30)), N'')) AS Batch_Label,
       CAST(CASE WHEN EXISTS (SELECT 1 FROM Unit_Item_ExpireDate x WHERE x.Item_Id = d.Item_Id) THEN 1 ELSE 0 END AS bit) AS Item_Has_Batches
FROM Invoices_Details d
LEFT JOIN Unit_item u ON u.id = d.Unit_Item_Table_Id
LEFT JOIN Item i ON i.id = d.Item_Id
LEFT JOIN unit un ON un.id = u.unit_id
LEFT JOIN Unit_Item_ExpireDate e ON e.Id = d.Unit_Item_ExpireDate_Id
CROSS APPLY (SELECT CAST(CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END AS decimal(18,6)) AS Factor) f
WHERE d.Invoice_Id = {invoiceId} AND ISNULL(d.Larg_Qnty, 0) * f.Factor + ISNULL(d.Small_Qnty, 0) > 0
ORDER BY d.Id").ToList();
            // السعر الصافى للوحدة الصغرى = نصيب السطر من صافى الفاتورة (بعد خصم الرأس والضريبة) ÷ الكمية المدفوعة.
            var nets = opp.db.Invoices_Details.AsNoTracking().Where(d => d.Invoice_Id == invoiceId).Select(d => new { d.Id, Net = d.Item_Net_Total_Price ?? 0 }).ToList();
            decimal sumNet = nets.Sum(n => n.Net);
            foreach (var l in inv.Lines)
            {
                decimal lineNet = nets.FirstOrDefault(n => n.Id == l.Original_Detail_Id)?.Net ?? 0;
                decimal share = sumNet > 0 ? inv.Net_Total * lineNet / sumNet : 0;
                l.Net_Unit_Price = l.Sold_Small > 0 ? Math.Round(share / l.Sold_Small, 4) : 0;
            }
            return inv;
        }

        /// <summary>سعر الأمان (ق15): أدنى سعر بيع صافٍ تاريخى للوحدة الصغرى من الصنف − سقف خصم المندوب.</summary>
        private static decimal BlindPrice(operations opp, SalesRepSaleService.SaleContext ctx, long unitItemId)
        {
            long itemId = opp.db.Unit_item.AsNoTracking().Where(u => u.id == unitItemId).Select(u => u.Item_Id).FirstOrDefault() ?? 0;
            if (itemId <= 0) return 0;
            decimal? min = opp.db.Database.SqlQuery<decimal?>($@"
SELECT CAST(MIN(d.Item_Net_Total_Price / (ISNULL(d.Larg_Qnty, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(d.Small_Qnty, 0))) AS decimal(18,6)) AS Value
FROM Invoices_Details d JOIN Invoices_List l ON l.Id = d.Invoice_Id JOIN Unit_item u ON u.id = d.Unit_Item_Table_Id
WHERE d.Item_Id = {itemId} AND ISNULL(l.Deleted, 0) = 0 AND ISNULL(d.Item_Net_Total_Price, 0) > 0
  AND ISNULL(d.Larg_Qnty, 0) * CASE WHEN ISNULL(u.Small_unit, 0) <= 0 THEN 1 ELSE u.Small_unit END + ISNULL(d.Small_Qnty, 0) > 0").AsEnumerable().FirstOrDefault();
            if (min is not > 0) return 0;
            decimal disc = Math.Clamp(ctx.Max_Discount_Rate ?? 0, 0, 100);
            return Math.Round(min.Value * (1 - disc / 100), 4);
        }

        private static void ApprovalStatus(operations opp, Guid docGuid, SalesRepSaleService.CheckResult result)
        {
            var approvals = opp.db.SalesRep_Approvals.AsNoTracking().Where(a => a.Ref_Document_Guid == docGuid && a.Ref_Doc_Id == null).ToList();
            foreach (var v in result.Violations)
            {
                var mine = approvals.Where(a => a.Type == (int)v.Type).OrderByDescending(a => a.Insert_ServerDateTime).ToList();
                var ok = mine.FirstOrDefault(a => a.Status == (int)SalesRepApprovalStatus.Approved && (a.Approved_Value ?? a.Requested_Value ?? 0) >= v.Requested);
                var pending = mine.FirstOrDefault(a => a.Status == (int)SalesRepApprovalStatus.Pending && (a.Requested_Value ?? 0) >= v.Requested);
                var last = ok ?? pending ?? mine.FirstOrDefault(a => (a.Requested_Value ?? 0) >= v.Requested);
                v.Approval_Id = last?.Id;
                v.Approval_Status = last?.Status;
            }
        }

        private static List<long> VouchersOfReturn(operations opp, long returnId) => opp.db.Database.SqlQuery<long>($@"
SELECT DISTINCT CAST(Order_Mony_Id AS bigint) AS Value FROM Money_Transaction_Box
WHERE DataDictionaryProcessesListsId = {Common.InvoiceReverse_Type_Id} AND Reference_Type_Table_Id = {returnId} AND ISNULL(Order_Mony_Id, 0) > 0").ToList();

        private static int Age(DateTime? date) => date == null ? 0 : Math.Max(0, (int)(DateTime.Today - date.Value.Date).TotalDays);

        public class CustomerRow { public string Name { get; set; } = ""; public long SubAccount_Id { get; set; } }

        public class BlindSourceRow
        {
            public long Unit_Item_Id { get; set; }
            public long Item_Id { get; set; }
            public string Item_Name { get; set; } = "";
            public string Code { get; set; } = "";
            public decimal Factor { get; set; } = 1;
            public long Unit_Id { get; set; }
            public string B_Unit { get; set; } = "";
            public string S_Unit { get; set; } = "";
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
