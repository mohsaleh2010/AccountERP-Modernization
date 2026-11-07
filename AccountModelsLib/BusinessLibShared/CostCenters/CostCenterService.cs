using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.BusinessLibShared.CostCenters
{
    /// <summary>نوع العقدة (ق2). 0 = عام (كل البيانات القديمة وتعمل كما كانت).</summary>
    public enum CostCenterType { General = 0, Project = 1, Phase = 2, CostItem = 3, Department = 4, Site = 5, SalesRep = 6 }

    /// <summary>حالة العقدة (ق7). لا صف فى Cost_Center_State = نشط.</summary>
    public enum CostCenterStatus { Active = 1, Suspended = 2, Closed = 3 }

    /// <summary>
    /// خدمة مراكز التكلفة المشتركة (ق15): كل الاستعلامات بمعاملات (لا حقن، والاسم الذى فيه ' يُحفظ سليماً).
    /// الورقية تُحسب من الأبناء (لا نعتمد isLastLevel)، والحالة من Cost_Center_State (لا نعتمد isActive).
    /// الإضافة تمر على CostCenterCLS.Add_NewCostCenter كما هى لتبقى متوافقة مع الديسكتوب.
    /// </summary>
    public static class CostCenterService
    {
        public class Node
        {
            public long Id { get; set; }
            public long ParentId { get; set; }
            public long Code { get; set; }
            public string CodeStr { get; set; }
            public string Name { get; set; }
            public string NameE { get; set; }
            public long CostType { get; set; }
            public int Level { get; set; }
            public string Notes { get; set; }
            public int ChildCount { get; set; }
            public int EntryLines { get; set; }
            public int Status { get; set; }
            public bool IsLeaf => ChildCount == 0;
            public bool IsOpen => Status == (int)CostCenterStatus.Active;
        }

        public class Result
        {
            public bool Ok { get; set; }
            public string Message { get; set; }
            public long Id { get; set; }
            public static Result Fail(string m) => new Result { Ok = false, Message = m };
            public static Result Success(long id = 0) => new Result { Ok = true, Id = id };
        }

        public class Usage
        {
            public int Children { get; set; }
            public int EntryLines { get; set; }
            public int AccountLinks { get; set; }
            public int CashLines { get; set; }
            public bool InUse => Children + EntryLines + AccountLinks + CashLines > 0;
            public string Describe()
            {
                var p = new List<string>();
                if (Children > 0) p.Add(Children + " مركز فرعى");
                if (EntryLines > 0) p.Add(EntryLines + " سطر قيد");
                if (CashLines > 0) p.Add(CashLines + " سطر نقدية");
                if (AccountLinks > 0) p.Add(AccountLinks + " ربط بحساب");
                return string.Join("، ", p);
            }
        }

        private class NodeRow
        {
            public long Id { get; set; }
            public long ParentId { get; set; }
            public string CodeStr { get; set; }
            public int Level { get; set; }
        }

        /// <summary>الشجرة كاملة مع عدد الأبناء والحركات والحالة.</summary>
        public static List<Node> GetTree(operations opp)
        {
            return opp.db.Database.SqlQueryRaw<Node>(@"
SELECT CAST(t.Id AS bigint) AS Id, CAST(ISNULL(t.Parent_Id, 0) AS bigint) AS ParentId, CAST(ISNULL(t.Code, 0) AS bigint) AS Code,
       ISNULL(t.Code_Str, N'') AS CodeStr, ISNULL(t.Name, N'') AS Name, ISNULL(t.NameE, '') AS NameE,
       CAST(ISNULL(t.Cost_Type, 0) AS bigint) AS CostType, CAST(ISNULL(t.Cost_Level, 0) AS int) AS Level, ISNULL(t.Notes, N'') AS Notes,
       (SELECT COUNT(*) FROM Cost_Center_Tree c WHERE c.Parent_Id = t.Id) AS ChildCount,
       (SELECT COUNT(*) FROM Account_Entry_Details d WHERE d.CostCenter_Id = t.Id) AS EntryLines,
       CAST(ISNULL(s.Status, 1) AS int) AS Status
FROM Cost_Center_Tree t LEFT JOIN Cost_Center_State s ON s.Cost_Center_Id = t.Id
ORDER BY t.Id").ToList();
        }

        public static Usage GetUsage(operations opp, long id)
        {
            return new Usage
            {
                Children = Count(opp, $"SELECT COUNT(*) AS Value FROM Cost_Center_Tree WHERE Parent_Id = {id}"),
                EntryLines = Count(opp, $"SELECT COUNT(*) AS Value FROM Account_Entry_Details WHERE CostCenter_Id = {id}"),
                AccountLinks = Count(opp, $"SELECT COUNT(*) AS Value FROM Cost_Center_ForAccounts WHERE Cost_Center_Id = {id}"),
                CashLines = Count(opp, $"SELECT COUNT(*) AS Value FROM Money_Transaction_Box WHERE Account_Cost_Center_Tree_Id = {id}")
                          + Count(opp, $"SELECT COUNT(*) AS Value FROM Order_Money WHERE Sandouk_Cost_Center_Tree_Id = {id}"),
            };
        }

        private static int Count(operations opp, string sql) => opp.db.Database.SqlQueryRaw<int>(sql).AsEnumerable().First();

        /// <summary>كود الابن التالى = أكبر لاحقة بين الإخوة + 1 (لا عدد الإخوة + 1 كما فى المولّد القديم الذى يتصادم بعد الحذف).</summary>
        public static (string CodeStr, long Code, int Level) NextChildCode(operations opp, long parentId, string firstSubCode)
        {
            var parent = opp.db.Database.SqlQueryRaw<NodeRow>(
                "SELECT CAST(Id AS bigint) AS Id, CAST(ISNULL(Parent_Id,0) AS bigint) AS ParentId, ISNULL(Code_Str, N'') AS CodeStr, CAST(ISNULL(Cost_Level,0) AS int) AS Level FROM Cost_Center_Tree WHERE Id = {0}", parentId).AsEnumerable().FirstOrDefault();
            if (parent == null) throw new InvalidOperationException("المركز الأب غير موجود");
            char sep = Common.Account_Seperator_Char;
            var kids = opp.db.Database.SqlQueryRaw<string>("SELECT ISNULL(Code_Str, N'') AS Value FROM Cost_Center_Tree WHERE Parent_Id = {0}", parentId).ToList();
            long max = 0;
            foreach (var k in kids)
            {
                string last = k.Contains(sep) ? k.Substring(k.LastIndexOf(sep) + 1) : k;
                if (long.TryParse(last, out long n) && n > max) max = n;
            }
            long next = max + 1;
            string part = firstSubCode == "1" || next >= 10 ? next.ToString() : "0" + next;
            string codeStr = parent.CodeStr == "0" || parent.CodeStr == "" ? part : parent.CodeStr + sep + part;
            long.TryParse(codeStr.Replace(sep.ToString(), ""), out long code);
            return (codeStr, code, parent.Level + 1);
        }

        /// <summary>إضافة مركز تحت أب (أو جذر إن parentId=0). الاسم يُفحص ضد تكراره تحت نفس الأب.</summary>
        public static Result Add(operations opp, long parentId, string name, string nameE, CostCenterType type, string notes, string firstSubCode = "1")
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return Result.Fail("اسم مركز التكلفة مطلوب");
            if (NameExists(opp, parentId, name, 0)) return Result.Fail("يوجد مركز بنفس الاسم تحت نفس الأب");

            string codeStr; long code; int level;
            if (parentId == 0)
            {
                long max = opp.db.Database.SqlQueryRaw<long>("SELECT CAST(ISNULL(MAX(Code),0) AS bigint) AS Value FROM Cost_Center_Tree WHERE ISNULL(Parent_Id,0) = 0").AsEnumerable().First();
                code = max + 1; codeStr = code.ToString(); level = 1;
            }
            else
            {
                var c = NextChildCode(opp, parentId, firstSubCode);
                codeStr = c.CodeStr; code = c.Code; level = c.Level;
            }
            if (CodeExists(opp, codeStr, 0)) return Result.Fail("الكود " + codeStr + " مستخدم من قبل");

            int excp = 0; string msg = "";
            new CostCenterCLS().Add_NewCostCenter(ref excp, ref msg, ref opp, parentId, name, code, codeStr, (int)type, notes ?? "", 0, "", 0, 0, level, firstSubCode);
            if (excp != 0) return Result.Fail(msg);
            var id = opp.db.Database.SqlQueryRaw<long>("SELECT CAST(Id AS bigint) AS Value FROM Cost_Center_Tree WHERE Code_Str = {0}", codeStr).AsEnumerable().FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(nameE) && id > 0)
                opp.db.Database.ExecuteSqlInterpolated($"UPDATE Cost_Center_Tree SET NameE = {nameE.Trim()} WHERE Id = {id}");
            return Result.Success(id);
        }

        /// <summary>تعديل البيانات الوصفية فقط؛ لا يغيّر Code الرقمى ولا الهرم (النقل له Move).</summary>
        public static Result Update(operations opp, long id, string name, string nameE, CostCenterType type, string notes, long userId)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return Result.Fail("اسم مركز التكلفة مطلوب");
            var cur = Get(opp, id);
            if (cur == null) return Result.Fail("المركز غير موجود");
            if (NameExists(opp, cur.ParentId, name, id)) return Result.Fail("يوجد مركز بنفس الاسم تحت نفس الأب");
            var now = DateTime.Now;
            opp.db.Database.ExecuteSqlInterpolated($@"UPDATE Cost_Center_Tree SET Name = {name}, NameE = {(nameE ?? "").Trim()}, Cost_Type = {(long)type},
                Notes = {notes ?? ""}, Edit_User_Id = {(int)userId}, EditOperationDate = {now} WHERE Id = {id}");
            return Result.Success(id);
        }

        /// <summary>إيقاف/إغلاق/إعادة فتح (بديل الحذف). النشط يحذف صف الحالة فيعود للوضع الافتراضى.</summary>
        public static Result SetStatus(operations opp, long id, CostCenterStatus status, long userId)
        {
            if (Get(opp, id) == null) return Result.Fail("المركز غير موجود");
            if (status == CostCenterStatus.Active)
                opp.db.Database.ExecuteSqlInterpolated($"DELETE FROM Cost_Center_State WHERE Cost_Center_Id = {id}");
            else
                opp.db.Database.ExecuteSqlInterpolated($@"
IF EXISTS (SELECT 1 FROM Cost_Center_State WHERE Cost_Center_Id = {id})
    UPDATE Cost_Center_State SET Status = {(byte)status}, Changed_Date = GETDATE(), Changed_User_Id = {userId} WHERE Cost_Center_Id = {id}
ELSE
    INSERT INTO Cost_Center_State (Cost_Center_Id, Status, Changed_User_Id) VALUES ({id}, {(byte)status}, {userId})");
            return Result.Success(id);
        }

        /// <summary>حذف فعلى فقط لمركز غير مستخدم (لا أبناء ولا حركات ولا ربط)؛ غير ذلك يُرفض مع السبب ويُقترح الإغلاق.</summary>
        public static Result Delete(operations opp, long id)
        {
            if (Get(opp, id) == null) return Result.Fail("المركز غير موجود");
            var u = GetUsage(opp, id);
            if (u.InUse) return Result.Fail("لا يمكن حذف المركز لأنه مستخدم: " + u.Describe() + ". استخدم الإغلاق بدل الحذف.");
            using var tx = opp.db.Database.BeginTransaction();
            opp.db.Database.ExecuteSqlInterpolated($"DELETE FROM Cost_Center_Project WHERE Cost_Center_Id = {id}");
            opp.db.Database.ExecuteSqlInterpolated($"DELETE FROM Cost_Center_State WHERE Cost_Center_Id = {id}");
            opp.db.Database.ExecuteSqlInterpolated($"DELETE FROM Cost_Center_Tree WHERE Id = {id}");
            tx.Commit();
            return Result.Success(id);
        }

        /// <summary>
        /// نقل عقدة (مع فرعها) لأب آخر: يمنع الدوران، ويعيد توليد Code_Str/Code/Cost_Level للعقدة وكل أحفادها فى معاملة واحدة.
        /// newParentId = 0 يجعلها جذراً. لا يمس أى سطر قيد (المرجع هو Id لا الكود).
        /// </summary>
        public static Result Move(operations opp, long id, long newParentId, string firstSubCode = "1")
        {
            var node = Get(opp, id);
            if (node == null) return Result.Fail("المركز غير موجود");
            if (node.ParentId == newParentId) return Result.Success(id);
            if (newParentId == id || Descendants(opp, id).Any(d => d.Id == newParentId)) return Result.Fail("لا يمكن نقل المركز إلى أحد فروعه");
            if (newParentId != 0 && Get(opp, newParentId) == null) return Result.Fail("المركز الأب الجديد غير موجود");

            string newCodeStr; long newCode; int newLevel;
            if (newParentId == 0)
            {
                long max = opp.db.Database.SqlQueryRaw<long>("SELECT CAST(ISNULL(MAX(Code),0) AS bigint) AS Value FROM Cost_Center_Tree WHERE ISNULL(Parent_Id,0) = 0").AsEnumerable().First();
                newCode = max + 1; newCodeStr = newCode.ToString(); newLevel = 1;
            }
            else
            {
                var c = NextChildCode(opp, newParentId, firstSubCode);
                newCodeStr = c.CodeStr; newCode = c.Code; newLevel = c.Level;
            }
            string oldCodeStr = node.CodeStr; int delta = newLevel - node.Level; char sep = Common.Account_Seperator_Char;
            var subtree = Descendants(opp, id);

            using var tx = opp.db.Database.BeginTransaction();
            opp.db.Database.ExecuteSqlInterpolated($"UPDATE Cost_Center_Tree SET Parent_Id = {newParentId}, Code_Str = {newCodeStr}, Code = {newCode}, Cost_Level = {newLevel} WHERE Id = {id}");
            foreach (var d in subtree)
            {
                string cs = newCodeStr + d.CodeStr.Substring(oldCodeStr.Length);
                long.TryParse(cs.Replace(sep.ToString(), ""), out long cn);
                int lvl = d.Level + delta;
                opp.db.Database.ExecuteSqlInterpolated($"UPDATE Cost_Center_Tree SET Code_Str = {cs}, Code = {cn}, Cost_Level = {lvl} WHERE Id = {d.Id}");
            }
            tx.Commit();
            return Result.Success(id);
        }

        /// <summary>كل أحفاد العقدة (بدونها).</summary>
        public static List<Node> Descendants(operations opp, long id)
        {
            var all = opp.db.Database.SqlQueryRaw<NodeRow>(
                "SELECT CAST(Id AS bigint) AS Id, CAST(ISNULL(Parent_Id,0) AS bigint) AS ParentId, ISNULL(Code_Str, N'') AS CodeStr, CAST(ISNULL(Cost_Level,0) AS int) AS Level FROM Cost_Center_Tree").ToList();
            var byParent = all.ToLookup(r => r.ParentId);
            var res = new List<Node>(); var q = new Queue<long>(); q.Enqueue(id);
            while (q.Count > 0)
                foreach (var k in byParent[q.Dequeue()])
                { res.Add(new Node { Id = k.Id, ParentId = k.ParentId, CodeStr = k.CodeStr, Level = k.Level }); q.Enqueue(k.Id); }
            return res;
        }

        /// <summary>العقدة وأسلافها (للوراثة ق5).</summary>
        public static List<long> SelfAndAncestors(operations opp, long id)
        {
            var parents = opp.db.Database.SqlQueryRaw<NodeRow>(
                "SELECT CAST(Id AS bigint) AS Id, CAST(ISNULL(Parent_Id,0) AS bigint) AS ParentId, N'' AS CodeStr, 0 AS Level FROM Cost_Center_Tree").ToDictionary(r => r.Id, r => r.ParentId);
            var res = new List<long>();
            long cur = id;
            while (cur != 0 && parents.TryGetValue(cur, out long p) && !res.Contains(cur)) { res.Add(cur); cur = p; }
            return res;
        }

        public class ProjectInfo
        {
            public long CustomerId { get; set; }
            public string CustomerName { get; set; } = "";
            public decimal ContractValue { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            /// <summary>1 تخطيط، 2 قيد التنفيذ، 3 مكتمل، 4 متوقف.</summary>
            public int Status { get; set; } = 1;
            public decimal RetentionPercent { get; set; }
            public string Notes { get; set; } = "";
        }

        private class ProjectRow
        {
            public long CustomerId { get; set; }
            public string CustomerName { get; set; }
            public decimal ContractValue { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int Status { get; set; }
            public decimal RetentionPercent { get; set; }
            public string Notes { get; set; }
        }

        public class CustomerRow
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        public static ProjectInfo GetProject(operations opp, long costCenterId)
        {
            var r = opp.db.Database.SqlQueryRaw<ProjectRow>(@"
SELECT CAST(ISNULL(p.Customer_Id,0) AS bigint) AS CustomerId, ISNULL(c.Name, N'') AS CustomerName, p.Contract_Value AS ContractValue,
       p.Start_Date AS StartDate, p.End_Date AS EndDate, CAST(p.Status AS int) AS Status, p.Retention_Percent AS RetentionPercent, ISNULL(p.Notes, N'') AS Notes
FROM Cost_Center_Project p LEFT JOIN Customer c ON c.Id = p.Customer_Id WHERE p.Cost_Center_Id = {0}", costCenterId).AsEnumerable().FirstOrDefault();
            return r == null ? new ProjectInfo() : new ProjectInfo
            {
                CustomerId = r.CustomerId, CustomerName = r.CustomerName ?? "", ContractValue = r.ContractValue, StartDate = r.StartDate, EndDate = r.EndDate,
                Status = r.Status, RetentionPercent = r.RetentionPercent, Notes = r.Notes ?? ""
            };
        }

        public static Result SaveProject(operations opp, long costCenterId, ProjectInfo p, long userId)
        {
            if (Get(opp, costCenterId) == null) return Result.Fail("المركز غير موجود");
            if (p.ContractValue < 0) return Result.Fail("قيمة العقد لا تكون سالبة");
            if (p.RetentionPercent < 0 || p.RetentionPercent > 100) return Result.Fail("نسبة الاستقطاع بين 0 و100");
            if (p.StartDate.HasValue && p.EndDate.HasValue && p.EndDate < p.StartDate) return Result.Fail("تاريخ النهاية قبل البداية");
            long? cust = p.CustomerId > 0 ? p.CustomerId : (long?)null;
            opp.db.Database.ExecuteSqlInterpolated($@"
IF EXISTS (SELECT 1 FROM Cost_Center_Project WHERE Cost_Center_Id = {costCenterId})
    UPDATE Cost_Center_Project SET Customer_Id = {cust}, Contract_Value = {p.ContractValue}, Start_Date = {p.StartDate}, End_Date = {p.EndDate},
        Status = {(byte)p.Status}, Retention_Percent = {p.RetentionPercent}, Notes = {p.Notes ?? ""}, EditOperationDate = GETDATE(), Edit_User_Id = {userId}
    WHERE Cost_Center_Id = {costCenterId}
ELSE
    INSERT INTO Cost_Center_Project (Cost_Center_Id, Customer_Id, Contract_Value, Start_Date, End_Date, Status, Retention_Percent, Notes, User_Id)
    VALUES ({costCenterId}, {cust}, {p.ContractValue}, {p.StartDate}, {p.EndDate}, {(byte)p.Status}, {p.RetentionPercent}, {p.Notes ?? ""}, {userId})");
            return Result.Success(costCenterId);
        }

        public static List<CustomerRow> SearchCustomers(operations opp, string text)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return new List<CustomerRow>();
            return opp.db.Database.SqlQueryRaw<CustomerRow>("SELECT TOP 30 CAST(Id AS bigint) AS Id, ISNULL(Name, N'') AS Name FROM Customer WHERE Name LIKE {0} ORDER BY Name", "%" + text + "%").ToList();
        }

        // ───── المرحلة 2: سياسة الحساب + المركز الافتراضى + الربط + تقرير الفجوات ─────

        public class AccountPolicy
        {
            public int Policy { get; set; }                 // 0 ممنوع / 1 اختيارى / 2 إلزامى (الفعلى)
            public long DefaultCenterId { get; set; }
            public string DefaultCenterName { get; set; } = "";
            public int LinkCount { get; set; }
        }

        public class AccountRow
        {
            public long Id { get; set; }
            public string No { get; set; } = "";
            public string Name { get; set; } = "";
            public int Policy { get; set; }
            public int LinkCount { get; set; }
            public int ViolatingLines { get; set; }
        }

        private class PolicyRow { public int Policy { get; set; } public long DefaultCenterId { get; set; } public string DefaultCenterName { get; set; } public int LinkCount { get; set; } }

        private static bool HasPolicyColumn(operations opp) =>
            opp.db.Database.SqlQueryRaw<int>("SELECT CASE WHEN COL_LENGTH(N'dbo.Submain_Account', N'Cost_Center_Policy') IS NULL THEN 0 ELSE 1 END AS Value").AsEnumerable().First() == 1;

        /// <summary>التعبير الفعلى للسياسة: العمود الجديد إن وُجد وغير NULL وإلا العلم القديم (ق4).</summary>
        private static string PolicyExpr(operations opp, string alias) => HasPolicyColumn(opp)
            ? $"CAST(ISNULL({alias}.Cost_Center_Policy, CASE WHEN ISNULL({alias}.Accept_Cost_Center_Flage,0) = 1 THEN 1 ELSE 0 END) AS int)"
            : $"CAST(CASE WHEN ISNULL({alias}.Accept_Cost_Center_Flage,0) = 1 THEN 1 ELSE 0 END AS int)";

        public static AccountPolicy GetAccountPolicy(operations opp, long accountId)
        {
            var r = opp.db.Database.SqlQueryRaw<PolicyRow>($@"
SELECT {PolicyExpr(opp, "a")} AS Policy, CAST(ISNULL(a.Cost_Center_Tree_ID,0) AS bigint) AS DefaultCenterId, ISNULL(t.Name, N'') AS DefaultCenterName,
       (SELECT COUNT(*) FROM Cost_Center_ForAccounts l WHERE l.Submain_Account_Id = a.Id) AS LinkCount
FROM Submain_Account a LEFT JOIN Cost_Center_Tree t ON t.Id = a.Cost_Center_Tree_ID WHERE a.Id = {{0}}", accountId).AsEnumerable().FirstOrDefault();
            return r == null ? new AccountPolicy() : new AccountPolicy { Policy = r.Policy, DefaultCenterId = r.DefaultCenterId, DefaultCenterName = r.DefaultCenterName ?? "", LinkCount = r.LinkCount };
        }

        /// <summary>يحفظ سياسة حساب واحد (يعدّله المستخدم صراحةً) ويُبقى العلم القديم متسقاً حتى يتصرف الديسكتوب بنفس القاعدة.</summary>
        public static Result SetAccountPolicy(operations opp, long accountId, int policy, long defaultCenterId)
        {
            if (policy < 0 || policy > 2) return Result.Fail("سياسة غير صالحة");
            if (defaultCenterId > 0)
            {
                var c = Get(opp, defaultCenterId);
                if (c == null) return Result.Fail("المركز الافتراضى غير موجود");
                if (!c.IsLeaf) return Result.Fail("المركز الافتراضى يجب أن يكون مركزاً نهائياً (ورقة)");
                if (!c.IsOpen) return Result.Fail("المركز الافتراضى موقوف أو مغلق");
            }
            if (policy == 0) defaultCenterId = 0;
            int legacy = policy > 0 ? 1 : 0;
            if (!HasPolicyColumn(opp)) return Result.Fail("لم يُرقَّ المخطط بعد (Cost_Center_Policy) — أعد تشغيل التطبيق");
            opp.db.Database.ExecuteSqlInterpolated($"UPDATE Submain_Account SET Cost_Center_Policy = {(byte)policy}, Accept_Cost_Center_Flage = {legacy}, Cost_Center_Tree_ID = {defaultCenterId} WHERE Id = {accountId}");
            return Result.Success(accountId);
        }

        /// <summary>حسابات مربوطة بمركز (ربط مباشر على المركز نفسه).</summary>
        public static List<AccountRow> LinkedAccounts(operations opp, long centerId) =>
            opp.db.Database.SqlQueryRaw<AccountRow>($@"
SELECT DISTINCT CAST(a.Id AS bigint) AS Id, ISNULL(a.SubAccount_NO, N'') AS No, ISNULL(a.SubAccount_Name, N'') AS Name, {PolicyExpr(opp, "a")} AS Policy, 0 AS LinkCount, 0 AS ViolatingLines
FROM Cost_Center_ForAccounts l INNER JOIN Submain_Account a ON a.Id = l.Submain_Account_Id WHERE l.Cost_Center_Id = {{0}} ORDER BY No", centerId).ToList();

        /// <summary>بحث عن حسابات نهائية (قابلة للقيد) بالاسم أو الرقم.</summary>
        public static List<AccountRow> SearchAccounts(operations opp, string text)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return new List<AccountRow>();
            return opp.db.Database.SqlQueryRaw<AccountRow>($@"
SELECT TOP 60 CAST(a.Id AS bigint) AS Id, ISNULL(a.SubAccount_NO, N'') AS No, ISNULL(a.SubAccount_Name, N'') AS Name, {PolicyExpr(opp, "a")} AS Policy,
       (SELECT COUNT(*) FROM Cost_Center_ForAccounts l WHERE l.Submain_Account_Id = a.Id) AS LinkCount, 0 AS ViolatingLines
FROM Submain_Account a WHERE ISNULL(a.IS_Last_Level,0) = 1 AND (a.SubAccount_Name LIKE {{0}} OR a.SubAccount_NO LIKE {{0}}) ORDER BY a.SubAccount_NO", "%" + text + "%").ToList();
        }

        /// <summary>ربط جماعى: كل حساب × كل مركز (الأوراق فقط)، مع تخطى المكرر. يُضيف فقط. يعيد عدد الروابط الجديدة.</summary>
        public static Result BulkLink(operations opp, IEnumerable<long> accountIds, IEnumerable<long> centerIds)
        {
            var centers = centerIds.Distinct().ToList();
            var tree = GetTree(opp);
            var leafCenters = centers.Where(c => tree.Any(n => n.Id == c && n.IsLeaf)).ToList();
            if (leafCenters.Count == 0) return Result.Fail("اختر مركزاً نهائياً واحداً على الأقل");
            int added = 0;
            var cls = new CostCenterCLS();
            foreach (var acc in accountIds.Distinct())
            {
                var existing = opp.db.Database.SqlQueryRaw<long>("SELECT CAST(Cost_Center_Id AS bigint) AS Value FROM Cost_Center_ForAccounts WHERE Submain_Account_Id = {0}", acc).ToList();
                var fresh = leafCenters.Where(c => !existing.Contains(c)).ToList();
                if (fresh.Count == 0) continue;
                int excp = 0; string msg = "";
                cls.Add_NewCostCenterForAccounts(ref excp, ref msg, new System.Collections.ArrayList(fresh.Cast<object>().ToArray()), acc);
                if (excp == -1) return Result.Fail(msg);
                added += fresh.Count;
            }
            return Result.Success(added);
        }

        /// <summary>تقرير الفجوات: (1) حسابات سياستها إلزامى/اختيارى بلا أى ربط، (2) حسابات إلزامية لها أسطر قيود بلا مركز.</summary>
        public static List<AccountRow> GapReport(operations opp, bool onlyRequired)
        {
            string p = PolicyExpr(opp, "a");
            return opp.db.Database.SqlQueryRaw<AccountRow>($@"
SELECT CAST(a.Id AS bigint) AS Id, ISNULL(a.SubAccount_NO, N'') AS No, ISNULL(a.SubAccount_Name, N'') AS Name, {p} AS Policy,
       (SELECT COUNT(*) FROM Cost_Center_ForAccounts l WHERE l.Submain_Account_Id = a.Id) AS LinkCount,
       (SELECT COUNT(*) FROM Account_Entry_Details d WHERE d.Submain_Account_ID = a.Id AND ISNULL(d.CostCenter_Id,0) = 0) AS ViolatingLines
FROM Submain_Account a
WHERE ISNULL(a.IS_Last_Level,0) = 1 AND {p} >= {(onlyRequired ? 2 : 1)}
  AND ((SELECT COUNT(*) FROM Cost_Center_ForAccounts l WHERE l.Submain_Account_Id = a.Id) = 0
       OR (SELECT COUNT(*) FROM Account_Entry_Details d WHERE d.Submain_Account_ID = a.Id AND ISNULL(d.CostCenter_Id,0) = 0) > 0)
ORDER BY a.SubAccount_NO").ToList();
        }

        // ───── المرحلة 3: المراكز المسموحة لحساب فى شاشات الإدخال ─────

        public class AllowedCenter
        {
            public long Id { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string Path { get; set; } = "";
        }

        public class AllowedSet
        {
            public int Policy { get; set; }
            public long DefaultId { get; set; }
            public List<AllowedCenter> Centers { get; set; } = new List<AllowedCenter>();
        }

        /// <summary>
        /// المراكز المسموح بها للحساب: الأوراق المفتوحة المربوطة به؛ فإن لم يكن له ربط وسياسته ≠ 0 فكل الأوراق المفتوحة
        /// (نفس السلوك القديم). سياسة 0 = لا شىء. القرار النهائى للرفض/التحذير يبقى فى EntryValidator.
        /// </summary>
        public static AllowedSet AllowedFor(operations opp, long accountId)
        {
            var set = new AllowedSet();
            if (accountId <= 0) return set;
            var pol = GetAccountPolicy(opp, accountId);
            set.Policy = pol.Policy;
            if (pol.Policy == 0) return set;

            var tree = GetTree(opp);
            var byId = tree.ToDictionary(n => n.Id);
            var open = tree.Where(n => n.IsLeaf && n.IsOpen).ToList();
            var linked = opp.db.Database.SqlQueryRaw<long>("SELECT CAST(Cost_Center_Id AS bigint) AS Value FROM Cost_Center_ForAccounts WHERE Submain_Account_Id = {0}", accountId).ToList();
            var pick = linked.Count > 0 ? open.Where(n => linked.Contains(n.Id)).ToList() : open;

            string PathOf(Node n)
            {
                var parts = new List<string>();
                var cur = n; int guard = 0;
                while (cur != null && guard++ < 20) { parts.Insert(0, cur.Name); cur = cur.ParentId != 0 && byId.TryGetValue(cur.ParentId, out var p) ? p : null; }
                return string.Join(" › ", parts);
            }

            set.Centers = pick.OrderBy(n => n.CodeStr).Select(n => new AllowedCenter { Id = n.Id, Code = n.CodeStr, Name = n.Name, Path = PathOf(n) }).ToList();
            if (pol.DefaultCenterId > 0 && set.Centers.Any(c => c.Id == pol.DefaultCenterId)) set.DefaultId = pol.DefaultCenterId;
            return set;
        }

        /// <summary>كل الأوراق المفتوحة (لمركز رأس المستند، ق9): غير مرتبط بحساب بعينه، والترحيل على الأسطر يقيّده بسياسة كل حساب.</summary>
        public static AllowedSet AllOpenLeaves(operations opp)
        {
            var tree = GetTree(opp);
            var byId = tree.ToDictionary(n => n.Id);
            string PathOf(Node n)
            {
                var parts = new List<string>();
                var cur = n; int guard = 0;
                while (cur != null && guard++ < 20) { parts.Insert(0, cur.Name); cur = cur.ParentId != 0 && byId.TryGetValue(cur.ParentId, out var p) ? p : null; }
                return string.Join(" › ", parts);
            }
            return new AllowedSet
            {
                Policy = 1,
                Centers = tree.Where(n => n.IsLeaf && n.IsOpen).OrderBy(n => n.CodeStr)
                    .Select(n => new AllowedCenter { Id = n.Id, Code = n.CodeStr, Name = n.Name, Path = PathOf(n) }).ToList()
            };
        }

        /// <summary>
        /// المراكز المسموحة لعدة حسابات معاً (إذن «كل الخزن»: مركز واحد لطرف كل الصناديق): التقاطع، والسياسة الأشد (2 > 1 > 0).
        /// حساب سياسته 0 لا يقيّد غيره. الافتراضى يبقى فقط لو كان مشتركاً.
        /// </summary>
        public static AllowedSet AllowedForAll(operations opp, IEnumerable<long> accountIds)
        {
            var sets = accountIds.Where(i => i > 0).Distinct().Select(i => AllowedFor(opp, i)).Where(s => s.Policy != 0).ToList();
            if (sets.Count == 0) return new AllowedSet();
            var res = new AllowedSet { Policy = sets.Max(s => s.Policy) };
            var common = sets[0].Centers.Where(c => sets.All(s => s.Centers.Any(x => x.Id == c.Id))).ToList();
            res.Centers = common;
            var def = sets[0].DefaultId;
            if (def > 0 && common.Any(c => c.Id == def)) res.DefaultId = def;
            return res;
        }

        private static bool? _reject;
        private static DateTime _rejectAt;

        /// <summary>هل مخالفات مراكز التكلفة ترفض الحفظ؟ (CostCenter_Validation_Mode = 1). الافتراضى تحذير. مخزَّن دقيقة.</summary>
        public static bool RejectMode(operations opp)
        {
            if (_reject != null && (DateTime.UtcNow - _rejectAt).TotalSeconds < 60) return _reject.Value;
            bool r = false;
            try
            {
                var cfg = opp.db.Database.SqlQueryRaw<StoreTransferPolicy.KeyValueRow>("SELECT Id, KeysData, Value FROM SysConfig WHERE KeysData IS NOT NULL").ToList();
                var row = cfg.FirstOrDefault(x => Common.DecryptMe(x.KeysData).Trim() == CostCenterSchemaCLS.ValidationModeKey);
                r = row != null && Common.DecryptMe(row.Value ?? "").Trim() == "1";
            }
            catch { }
            _reject = r; _rejectAt = DateTime.UtcNow;
            return r;
        }

        public static void InvalidateMode() => _reject = null;

        public static Node Get(operations opp, long id) => GetTree(opp).FirstOrDefault(n => n.Id == id);

        private static bool NameExists(operations opp, long parentId, string name, long exceptId) =>
            opp.db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM Cost_Center_Tree WHERE ISNULL(Parent_Id,0) = {0} AND Id <> {1} AND Name = {2}", parentId, exceptId, name).AsEnumerable().First() > 0;

        private static bool CodeExists(operations opp, string codeStr, long exceptId) =>
            opp.db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM Cost_Center_Tree WHERE Id <> {0} AND Code_Str = {1}", exceptId, codeStr).AsEnumerable().First() > 0;
    }
}
