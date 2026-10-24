using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AccountModelsLib.Models;

namespace AccountModelsLib
{
    /// <summary>
    /// نوع الطرف المربوط بحساب فى الشجرة؛ القيمة = Submain_Account.Account_Customer_Type_Value (Account_Type.Value) فلا يُعاد ترقيمها.
    /// الصندوق والبنك كلاهما فى جدول Banks ويفرّق بينهما Banks.Account_Type_Value.
    /// </summary>
    public enum AccountLinkKind
    {
        Customer = 2,
        Employee = 4,
        Supplier = 5,
        CashBox = 6,
        Bank = 7,
    }

    public enum AccountLinkState
    {
        Linked,        // الطرف يشير للحساب والحساب يشير للطرف
        Unlinked,      // لا حساب للطرف
        Broken,        // الطرف يشير لحساب غير موجود أو لا يشير إليه
    }

    /// <summary>صف طرف (عميل/مورد/موظف/صندوق/بنك) مع حسابه الحالى.</summary>
    public class AccountLinkRow
    {
        public long PartyId { get; set; }
        public string PartyName { get; set; } = "";
        public bool Active { get; set; } = true;
        public bool IsShipmentCompany { get; set; }
        public long SubAccount_Id { get; set; }
        public string SubAccount_Name { get; set; } = "";
        public string SubAccount_NO { get; set; } = "";
        public AccountLinkState State { get; set; }
    }

    /// <summary>حساب مرشَّح للربط: فرعى بلا أبناء، من نوع الطرف أو بلا نوع، وغير مربوط بطرف آخر.</summary>
    public class AccountLinkCandidate
    {
        public long Id { get; set; }
        public string SubAccount_Name { get; set; } = "";
        public string SubAccount_NO { get; set; } = "";
        public bool SameType { get; set; }
    }

    /// <summary>اقتراح مطابقة بالاسم: طرف غير مربوط وحساب شاغر من نوعه بنفس الاسم تماماً.</summary>
    public class AccountLinkMatch
    {
        public long PartyId { get; set; }
        public string PartyName { get; set; } = "";
        public long SubAccount_Id { get; set; }
        public string SubAccount_NO { get; set; } = "";
    }

    /// <summary>الحساب الأب الذى تُنشأ تحته حسابات نوع طرف فى الفرع (Account_Type_Main_Account_Id).</summary>
    public class AccountLinkParentRow
    {
        public long Id { get; set; }                 // 0 = غير معرَّف بعد
        public int Account_Type_Value { get; set; }
        public bool GeneralCustomer { get; set; }    // Account_Type_ValueGeneralCustomer = 1 (العملاء العموميون)
        public long Main_Account_Id { get; set; }
        public string Main_Account_Name { get; set; } = "";
        public string Main_Account_NO { get; set; } = "";
    }

    /// <summary>
    /// ربط الأطراف بحساباتها فى الشجرة (خطة 05 - المرحلة 7، قائمة «ملف» ← «ربط الحسابات») بدل MainAccountSettingListFrm و
    /// CustomerDataListFRMSubAccount و AccountsCustomerLinkFrm، ويغطى الصندوق والبنك والموظفين التى كانت عناصرها فارغة فى الديسك توب.
    /// الربط فى الاتجاهين كما فى AccountsCustomerLinkFrm: الطرف.SubAccount_Id = الحساب، والحساب.Account_Customer_Type_ID = الطرف
    /// ونوعه Account_Customer_Type_Value = نوع الطرف.
    /// قواعد حديثة (مثل ربط حساب العميل فى SAP/Odoo): الحساب فرعى بلا أبناء، ولا يُنقل حساب مربوط بطرف آخر تلقائياً (يُفك أولاً)،
    /// ولا يُغيَّر نوع حساب له نوع آخر. كل عملية فى معاملة واحدة.
    /// </summary>
    public class AccountLinksService
    {
        private readonly Func<operations> _newOpp;

        public AccountLinksService(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public static readonly AccountLinkKind[] Kinds =
            { AccountLinkKind.Customer, AccountLinkKind.Supplier, AccountLinkKind.Employee, AccountLinkKind.CashBox, AccountLinkKind.Bank };

        /// <summary>عنصر القائمة فى SysConfigMenus الذى تتبعه صلاحية كل تبويب.</summary>
        public static string MenuKey(AccountLinkKind kind) => kind switch
        {
            AccountLinkKind.Customer => "menuItemRelatedCustomerAccounts",
            AccountLinkKind.Supplier => "menuItemRelatedSupplierAccounts",
            AccountLinkKind.Employee => "menuItemRelatedEmployeeAccounts",
            AccountLinkKind.CashBox => "menuItemRelatedSandoukAccounts",
            _ => "menuItemRelatedBankAccounts",
        };

        public const string MainAccountsMenuKey = "menuItemRelatedMainAccounts";

        // نوع حساب الطرف: شركة الشحن مورد خدمات بحساب من النوع 65.
        private static int AccountTypeFor(AccountLinkKind kind, bool shipment) =>
            kind == AccountLinkKind.Supplier && shipment ? Common.Account_Type_ForShippmentCompanyAsServicesSupplier : (int)kind;

        private static bool TypeAllowed(AccountLinkKind kind, int accountType) =>
            accountType == 0 || accountType == (int)kind
            || (kind == AccountLinkKind.Supplier && accountType == Common.Account_Type_ForShippmentCompanyAsServicesSupplier);

        private class PartyRaw
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public int? Active { get; set; }
            public int? Shipment { get; set; }
            public long? SubAccount_Id { get; set; }
        }

        private static List<PartyRaw> LoadParties(operations opp, AccountLinkKind kind)
        {
            var db = opp.db;
            switch (kind)
            {
                case AccountLinkKind.Customer:
                    return db.Customer.AsNoTracking().Select(c => new PartyRaw { Id = c.Id, Name = c.Name, Active = c.Active, SubAccount_Id = c.SubAccount_Id }).ToList();
                case AccountLinkKind.Supplier:
                    return db.Suppliers.AsNoTracking().Select(s => new PartyRaw { Id = s.Id, Name = s.Name, Active = s.Active, Shipment = s.IS_ShipmentCompany, SubAccount_Id = s.SubAccount_Id }).ToList();
                case AccountLinkKind.Employee:
                    return db.Employees.AsNoTracking().Select(e => new PartyRaw { Id = e.Id, Name = e.name, Active = 1, SubAccount_Id = e.SubAccount_Id }).ToList();
                default:
                    int type = (int)kind;
                    return db.Banks.AsNoTracking().Where(b => b.Account_Type_Value == type)
                        .Select(b => new PartyRaw { Id = b.Id, Name = b.Bank_Name, Active = b.Active, SubAccount_Id = b.SubAccount_Id }).ToList();
            }
        }

        /// <summary>كل أطراف النوع مع حالة ربطها، غير المربوط والمعطوب أولاً ثم بالاسم.</summary>
        public List<AccountLinkRow> GetRows(AccountLinkKind kind)
        {
            var opp = _newOpp();
            var parties = LoadParties(opp, kind);
            var ids = parties.Select(p => p.SubAccount_Id ?? 0).Where(i => i > 0).Distinct().ToList();
            var accounts = opp.db.Submain_Account.AsNoTracking().Where(a => ids.Contains(a.Id))
                .Select(a => new { a.Id, a.SubAccount_Name, a.SubAccount_NO, a.Account_Customer_Type_ID, a.Account_Customer_Type_Value })
                .ToDictionary(a => a.Id);
            return parties.Select(p =>
            {
                long acc = p.SubAccount_Id ?? 0;
                var row = new AccountLinkRow
                {
                    PartyId = p.Id,
                    PartyName = p.Name ?? "",
                    Active = (p.Active ?? 1) != 0,
                    IsShipmentCompany = (p.Shipment ?? 0) == 1,
                    SubAccount_Id = acc,
                    State = AccountLinkState.Unlinked,
                };
                if (acc > 0)
                {
                    if (accounts.TryGetValue(acc, out var a))
                    {
                        row.SubAccount_Name = a.SubAccount_Name ?? "";
                        row.SubAccount_NO = a.SubAccount_NO ?? "";
                        row.State = (a.Account_Customer_Type_ID ?? 0) == p.Id && TypeAllowed(kind, a.Account_Customer_Type_Value)
                            ? AccountLinkState.Linked : AccountLinkState.Broken;
                    }
                    else row.State = AccountLinkState.Broken;
                }
                return row;
            })
            .OrderBy(r => r.State == AccountLinkState.Linked ? 1 : 0).ThenBy(r => r.PartyName)
            .ToList();
        }

        // الحسابات المربوطة فعلاً بطرف آخر من نفس الجدول (من جهة الطرف) - لا تُعرض كمرشحة ولا يُربط بها.
        private static HashSet<long> UsedAccounts(operations opp, AccountLinkKind kind) =>
            LoadParties(opp, kind).Select(p => p.SubAccount_Id ?? 0).Where(i => i > 0).ToHashSet();

        /// <summary>حسابات فرعية شاغرة يمكن ربطها بطرف من النوع فى الفرع: نوعه أولاً ثم الحسابات بلا نوع.</summary>
        public List<AccountLinkCandidate> GetCandidates(AccountLinkKind kind, long branchId)
        {
            var opp = _newOpp();
            var db = opp.db;
            int type = (int)kind;
            int shipment = Common.Account_Type_ForShippmentCompanyAsServicesSupplier;
            bool supplier = kind == AccountLinkKind.Supplier;
            var used = UsedAccounts(opp, kind);
            var list = db.Submain_Account.AsNoTracking()
                .Where(a => a.Branch_Id == branchId
                         && (a.Account_Customer_Type_Value == type || (supplier && a.Account_Customer_Type_Value == shipment))
                         && (a.Account_Customer_Type_ID ?? 0) == 0
                         && !db.Submain_Account.Any(c => c.Main_Account_Id == a.Id))
                .Select(a => new AccountLinkCandidate { Id = a.Id, SubAccount_Name = a.SubAccount_Name, SubAccount_NO = a.SubAccount_NO, SameType = true })
                .ToList();
            return list.Where(c => !used.Contains(c.Id)).OrderBy(c => c.SubAccount_NO).ToList();
        }

        /// <summary>
        /// ربط طرف بحساب. Excp: 0 نجاح، 2 مرفوض لقاعدة (Msg يشرح السبب)، 1 خطأ.
        /// حساب الطرف القديم يُفك من جهته (Account_Customer_Type_ID = 0) ويبقى فى الشجرة بنوعه.
        /// </summary>
        public void Link(ref int Excp, ref string Msg, AccountLinkKind kind, long partyId, long subAccountId)
        {
            Excp = 0;
            Msg = "";
            var opp = _newOpp();
            try
            {
                var party = LoadParties(opp, kind).FirstOrDefault(p => p.Id == partyId);
                if (party == null) { Excp = 2; Msg = "الطرف غير موجود"; return; }
                var account = opp.db.Submain_Account.AsNoTracking().FirstOrDefault(a => a.Id == subAccountId);
                if (account == null) { Excp = 2; Msg = "الحساب غير موجود"; return; }
                if (opp.db.Submain_Account.AsNoTracking().Any(a => a.Main_Account_Id == subAccountId))
                { Excp = 2; Msg = "اختر حساباً فرعياً (بلا حسابات تحته)"; return; }
                if (!TypeAllowed(kind, account.Account_Customer_Type_Value))
                { Excp = 2; Msg = "نوع الحساب لا يخص هذا النوع من الأطراف"; return; }
                // يُرفض فقط لو طرف آخر يحمل الحساب فعلاً؛ إشارة قديمة من جهة الحساب وحدها (لطرف لم يعد يحمله) تُستبدل.
                var otherOwner = LoadParties(opp, kind).FirstOrDefault(p => p.Id != partyId && (p.SubAccount_Id ?? 0) == subAccountId);
                if (otherOwner != null)
                { Excp = 2; Msg = "الحساب مربوط بـ «" + otherOwner.Name + "» - فك ربطه أولاً"; return; }

                int newType = AccountTypeFor(kind, (party.Shipment ?? 0) == 1);
                long oldAccount = party.SubAccount_Id ?? 0;
                using var tx = opp.db.Database.BeginTransaction();
                if (oldAccount > 0 && oldAccount != subAccountId)
                    opp.db.Database.ExecuteSqlInterpolated($"UPDATE Submain_Account SET Account_Customer_Type_ID = 0 WHERE Id = {oldAccount} AND Account_Customer_Type_ID = {partyId}");
                opp.db.Database.ExecuteSqlInterpolated($"UPDATE Submain_Account SET Account_Customer_Type_Value = {newType}, Account_Customer_Type_ID = {partyId} WHERE Id = {subAccountId}");
                SetPartyAccount(opp, kind, partyId, subAccountId);
                tx.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>فك ربط طرف: SubAccount_Id = 0 وحسابه يُفرَّغ من الطرف ويبقى فى الشجرة شاغراً.</summary>
        public void Unlink(ref int Excp, ref string Msg, AccountLinkKind kind, long partyId)
        {
            Excp = 0;
            Msg = "";
            var opp = _newOpp();
            try
            {
                var party = LoadParties(opp, kind).FirstOrDefault(p => p.Id == partyId);
                if (party == null) { Excp = 2; Msg = "الطرف غير موجود"; return; }
                long oldAccount = party.SubAccount_Id ?? 0;
                using var tx = opp.db.Database.BeginTransaction();
                if (oldAccount > 0)
                    opp.db.Database.ExecuteSqlInterpolated($"UPDATE Submain_Account SET Account_Customer_Type_ID = 0 WHERE Id = {oldAccount} AND Account_Customer_Type_ID = {partyId}");
                SetPartyAccount(opp, kind, partyId, 0);
                tx.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        private static void SetPartyAccount(operations opp, AccountLinkKind kind, long partyId, long subAccountId)
        {
            var now = DateTime.Now;
            long user = Common.User_Id;
            switch (kind)
            {
                case AccountLinkKind.Customer:
                    opp.db.Database.ExecuteSqlInterpolated($"UPDATE Customer SET SubAccount_Id = {subAccountId}, Edit_User_Id = {user}, EditOperationDate = {now}, Update_ServerDateTime = {now} WHERE Id = {partyId}");
                    break;
                case AccountLinkKind.Supplier:
                    opp.db.Database.ExecuteSqlInterpolated($"UPDATE Supplier SET SubAccount_Id = {subAccountId}, Edit_User_Id = {user}, EditOperationDate = {now}, Update_ServerDateTime = {now} WHERE Id = {partyId}");
                    break;
                case AccountLinkKind.Employee:
                    opp.db.Database.ExecuteSqlInterpolated($"UPDATE Employee SET SubAccount_Id = {subAccountId}, Edit_User_Id = {user}, EditOperationDate = {now}, Update_ServerDateTime = {now} WHERE Id = {partyId}");
                    break;
                default:
                    opp.db.Database.ExecuteSqlInterpolated($"UPDATE Banks SET SubAccount_Id = {subAccountId}, Edit_User_Id = {user}, EditOperationDate = {now}, Update_ServerDateTime = {now} WHERE Id = {partyId}");
                    break;
            }
        }

        /// <summary>
        /// ينشئ للطرف حساباً جديداً باسمه تحت الحساب الأب لنوعه فى الفرع (نفس مسار SupplierCLS: Sub_Account.Add_AccountSupplierCustomerOnlyAccount)
        /// ثم يربطه. يرفض لو الطرف مربوط فعلاً أو لا أب معرَّف للنوع فى الفرع.
        /// </summary>
        public long CreateAccount(ref int Excp, ref string Msg, AccountLinkKind kind, long partyId, long branchId)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var opp = _newOpp();
                var party = LoadParties(opp, kind).FirstOrDefault(p => p.Id == partyId);
                if (party == null) { Excp = 2; Msg = "الطرف غير موجود"; return 0; }
                if ((party.SubAccount_Id ?? 0) > 0 && opp.db.Submain_Account.AsNoTracking().Any(a => a.Id == party.SubAccount_Id))
                { Excp = 2; Msg = "الطرف مربوط بحساب بالفعل"; return 0; }
                int type = AccountTypeFor(kind, (party.Shipment ?? 0) == 1);
                long parent = 0;
                int costCenter = 0;
                Common.Get_Account_Type_Code(ref opp, type, 0, ref parent, ref costCenter, branchId);
                if (parent == 0)
                { Excp = 2; Msg = "حدد الحساب الرئيسى لهذا النوع فى الفرع أولاً (تبويب الحسابات الرئيسية)"; return 0; }

                long newId = -1;
                string code = "";
                var acc = new Sub_Account();
                if (kind == AccountLinkKind.Supplier) acc.spCls.sp.Id = partyId;
                acc.Add_AccountSupplierCustomerOnlyAccount(ref Excp, ref Msg, ref opp, ref newId, ref code, type, party.Name ?? "", branchId);
                if (Excp != 0 || newId <= 0)
                {
                    if (Excp == 0) Excp = 1;
                    if (string.IsNullOrEmpty(Msg)) Msg = "تعذر إنشاء الحساب";
                    return 0;
                }
                Link(ref Excp, ref Msg, kind, partyId, newId);
                return Excp == 0 ? newId : 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
                return 0;
            }
        }

        /// <summary>اقتراحات «مطابقة بالاسم»: لكل طرف غير مربوط حساب شاغر واحد بالضبط من نوعه فى الفرع بنفس الاسم (بعد حذف المسافات الزائدة).</summary>
        public List<AccountLinkMatch> SuggestMatches(AccountLinkKind kind, long branchId)
        {
            static string N(string s) => string.Join(" ", (s ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            var byName = GetCandidates(kind, branchId).GroupBy(c => N(c.SubAccount_Name))
                .Where(g => g.Key != "" && g.Count() == 1).ToDictionary(g => g.Key, g => g.First());
            var taken = new HashSet<long>();
            var result = new List<AccountLinkMatch>();
            foreach (var p in GetRows(kind).Where(r => r.State == AccountLinkState.Unlinked))
            {
                if (!byName.TryGetValue(N(p.PartyName), out var c) || !taken.Add(c.Id)) continue;
                result.Add(new AccountLinkMatch { PartyId = p.PartyId, PartyName = p.PartyName, SubAccount_Id = c.Id, SubAccount_NO = c.SubAccount_NO });
            }
            return result;
        }

        /// <summary>الحسابات الأب لأنواع الأطراف فى الفرع، مع صف فارغ (Id = 0) لكل نوع غير معرَّف.</summary>
        public List<AccountLinkParentRow> GetParents(long branchId)
        {
            var db = _newOpp().db;
            var rows = (from m in db.account_Type_Main_Account_Id.AsNoTracking()
                        where m.Account_Branch_Id == branchId
                        join a in db.Submain_Account.AsNoTracking() on m.Main_Account_Id equals a.Id into aj
                        from a in aj.DefaultIfEmpty()
                        select new AccountLinkParentRow
                        {
                            Id = m.Id,
                            Account_Type_Value = m.Account_Type_Value ?? 0,
                            GeneralCustomer = (m.Account_Type_ValueGeneralCustomer ?? 0) == 1,
                            Main_Account_Id = m.Main_Account_Id ?? 0,
                            Main_Account_Name = a != null ? a.SubAccount_Name : "",
                            Main_Account_NO = a != null ? a.SubAccount_NO : "",
                        }).ToList();
            var types = Kinds.Select(k => (int)k).Append(Common.Account_Type_ForShippmentCompanyAsServicesSupplier).ToList();
            foreach (int t in types)
                if (!rows.Any(r => r.Account_Type_Value == t && !r.GeneralCustomer))
                    rows.Add(new AccountLinkParentRow { Account_Type_Value = t });
            return rows.Where(r => types.Contains(r.Account_Type_Value))
                .OrderBy(r => types.IndexOf(r.Account_Type_Value)).ThenBy(r => r.GeneralCustomer).ToList();
        }

        /// <summary>
        /// تحديد الحساب الأب لنوع فى الفرع (إضافة صف جديد أو تعديل صفه) - مثل MainAccountSettingCLS.Add_Account_Type_Main_Account،
        /// والحساب الأب يأخذ النوع فقط لو كان بلا نوع (لا يُغيَّر نوع قائم). Excp: 0 نجاح، 2 مرفوض، 1 خطأ.
        /// </summary>
        public void SaveParent(ref int Excp, ref string Msg, long branchId, int accountType, bool generalCustomer, long mainAccountId)
        {
            Excp = 0;
            Msg = "";
            var opp = _newOpp();
            try
            {
                var account = opp.db.Submain_Account.AsNoTracking().FirstOrDefault(a => a.Id == mainAccountId);
                if (account == null) { Excp = 2; Msg = "الحساب غير موجود"; return; }
                if ((account.Account_Customer_Type_ID ?? 0) > 0)
                { Excp = 2; Msg = "هذا حساب طرف مربوط ولا يصلح حساباً رئيسياً"; return; }
                if (account.Account_Customer_Type_Value != 0 && account.Account_Customer_Type_Value != accountType)
                { Excp = 2; Msg = "الحساب له نوع آخر فى الشجرة"; return; }
                int general = generalCustomer ? 1 : 0;
                var existing = opp.db.account_Type_Main_Account_Id.AsNoTracking()
                    .FirstOrDefault(m => m.Account_Branch_Id == branchId && m.Account_Type_Value == accountType && (m.Account_Type_ValueGeneralCustomer ?? 0) == general);
                string name = account.SubAccount_Name ?? "";
                var now = DateTime.Now;
                using var tx = opp.db.Database.BeginTransaction();
                if (existing == null)
                {
                    string typeName = opp.db.Database.SqlQueryRaw<string>("SELECT TOP 1 Name AS Value FROM Account_Type WHERE Value = {0}", accountType).FirstOrDefault() ?? "";
                    if (Common.Use_Auto_Identity_Column == 0)
                        opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO Account_Type_Main_Account_Id (Id, Account_Type_Value, Account_Type_Name, Account_Branch_Id, Main_Account_Id, Account_Name, Account_Type_ValueGeneralCustomer, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {accountType}, {typeName}, {branchId}, {mainAccountId}, {name}, {general}, {now}, {Common.User_Id}, {Common.Branch_Id}, {now})");
                    else
                        opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO Account_Type_Main_Account_Id (Account_Type_Value, Account_Type_Name, Account_Branch_Id, Main_Account_Id, Account_Name, Account_Type_ValueGeneralCustomer, OperationDate, User_Id, Branch_Id, Insert_ServerDateTime)
VALUES ({accountType}, {typeName}, {branchId}, {mainAccountId}, {name}, {general}, {now}, {Common.User_Id}, {Common.Branch_Id}, {now})");
                }
                else
                {
                    opp.db.Database.ExecuteSqlInterpolated($@"
UPDATE Account_Type_Main_Account_Id SET Main_Account_Id = {mainAccountId}, Account_Name = {name}, Edit_User_Id = {Common.User_Id}, EditOperationDate = {now}, Update_ServerDateTime = {now}
WHERE Id = {existing.Id}");
                }
                if (account.Account_Customer_Type_Value == 0)
                    opp.db.Database.ExecuteSqlInterpolated($"UPDATE Submain_Account SET Account_Customer_Type_Value = {accountType} WHERE Id = {mainAccountId} AND Account_Customer_Type_Value = 0");
                tx.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }
    }
}
