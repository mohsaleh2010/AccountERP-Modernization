using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>عقدة فى شجرة القوائم كما تعرضها شاشة الصلاحيات.</summary>
    public class MenuPermissionNode
    {
        public long Id { get; set; }
        public long Parent_Id { get; set; }
        public string Key { get; set; } = "";
        public string Description { get; set; } = "";
        public string Description_En { get; set; } = "";
        public string RazorPath { get; set; } = "";
        public bool Hidden { get; set; }   // SysConfigMenus.IsShowToUser = 0: لا تظهر فى القائمة (مثل صلاحيات العمليات تحت 321)
    }

    /// <summary>
    /// صلاحيات القوائم لكل مستخدم (SysConfigMenusPERMATIONS، PROJECT_ID = 2) بدل MenuFrmAdmin: صف لكل (مستخدم × قائمة)
    /// وقيمة PERMATION مشفرة "1" أو "0" كما يقرؤها الديسك توب (SearchMenuStrip) والويب (MenuRepository.GetUserAllowedMenus)
    /// وIsHavePermissionSysMenu لصلاحيات العمليات (917، 316، 811 ... تحت المجموعة 321).
    /// الحفظ يغيّر قيمة الصف القائم أو يضيف صفاً ناقصاً (Id بـ Snowflake - كان SetUpdateMenusPermisionAddedNew يفشل بصمت لأن Id ليس Identity).
    /// </summary>
    public class MenuPermissionService
    {
        public const int ProjectId = 2;
        public const string PermissionsMenuKey = "toolStripMenuItemPermissinForm";

        private readonly Func<operations> _newOpp;

        public MenuPermissionService(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public class MenuRow
        {
            public long Id { get; set; }
            public long Parent_Id { get; set; }
            public string KeysData { get; set; }
            public string Description { get; set; }
            public string Description_En { get; set; }
            public string RazorPath { get; set; }
            public string Value { get; set; }
            public int? IsShowToUser { get; set; }
        }

        public class GrantRow
        {
            public long Id { get; set; }
            public long Menu_ID { get; set; }
            public string PERMATION { get; set; }
        }

        private static string D(string s)
        {
            try { return string.IsNullOrEmpty(s) ? "" : Common.DecryptMe(s).Trim(); } catch { return ""; }
        }

        private List<MenuRow> LoadMenus(operations opp) => opp.db.Database.SqlQueryRaw<MenuRow>(
            "SELECT Id, ISNULL(Parent_Id, 0) AS Parent_Id, KeysData, Description, Description_En, RazorPath, Value, IsShowToUser FROM SysConfigMenus WHERE PROJECT_ID = {0}",
            ProjectId).ToList();

        private List<GrantRow> LoadGrants(operations opp, long userId) => opp.db.Database.SqlQueryRaw<GrantRow>(
            "SELECT Id, ISNULL(Menu_ID, 0) AS Menu_ID, PERMATION FROM SysConfigMenusPERMATIONS WHERE USER_ID = {0} AND PROJECT_ID = {1}",
            userId, ProjectId).ToList();

        public List<MenuPermissionNode> GetTree()
        {
            return LoadMenus(_newOpp()).Select(m => new MenuPermissionNode
            {
                Id = m.Id,
                Parent_Id = m.Parent_Id,
                Key = D(m.KeysData),
                Description = D(m.Description),
                Description_En = D(m.Description_En),
                RazorPath = D(m.RazorPath),
                Hidden = (m.IsShowToUser ?? 1) == 0,
            }).ToList();
        }

        /// <summary>القوائم المسموحة للمستخدم (PERMATION = "1").</summary>
        public HashSet<long> GetGranted(long userId) =>
            LoadGrants(_newOpp(), userId).Where(g => D(g.PERMATION) == "1").Select(g => g.Menu_ID).ToHashSet();

        /// <summary>
        /// يجعل صلاحيات المستخدم = granted تماماً. Excp: 0 نجاح، 2 مرفوض، 1 خطأ. يرجع عدد الصفوف التى تغيّرت.
        /// لا يسحب المستخدم من نفسه شاشة الصلاحيات (ولا القوائم التى فوقها) حتى لا يُغلق الباب على نفسه.
        /// </summary>
        public int Save(ref int Excp, ref string Msg, long userId, ISet<long> granted, long actingUserId)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var opp = _newOpp();
                var menus = LoadMenus(opp);
                if (userId == actingUserId)
                {
                    var byId = menus.ToDictionary(m => m.Id);
                    var self = menus.FirstOrDefault(m => D(m.KeysData) == PermissionsMenuKey);
                    for (long id = self?.Id ?? 0; id != 0 && byId.ContainsKey(id); id = byId[id].Parent_Id)
                        if (!granted.Contains(id)) { Excp = 2; Msg = "لا يمكنك سحب شاشة الصلاحيات من حسابك"; return 0; }
                }
                var grants = LoadGrants(opp, userId).GroupBy(g => g.Menu_ID).ToDictionary(g => g.Key, g => g.ToList());
                string yes = Common.EncryptMe("1"), no = Common.EncryptMe("0");
                var now = DateTime.Now;
                int changed = 0;
                using var tx = opp.db.Database.BeginTransaction();
                foreach (var m in menus)
                {
                    bool want = granted.Contains(m.Id);
                    string value = want ? yes : no;
                    if (grants.TryGetValue(m.Id, out var rows))
                    {
                        foreach (var r in rows.Where(r => (D(r.PERMATION) == "1") != want))
                        {
                            opp.db.Database.ExecuteSqlInterpolated($@"
UPDATE SysConfigMenusPERMATIONS SET PERMATION = {value}, Edit_User_Id = {actingUserId}, EditOperationDate = {now}, Update_ServerDateTime = {now} WHERE Id = {r.Id}");
                            changed++;
                        }
                    }
                    else
                    {
                        Insert(opp, userId, m, value, actingUserId, now);
                        changed++;
                    }
                }
                tx.Commit();
                return changed;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
                return 0;
            }
        }

        private static void Insert(operations opp, long userId, MenuRow m, string value, long actingUserId, DateTime now)
        {
            opp.db.Database.ExecuteSqlInterpolated($@"
INSERT INTO SysConfigMenusPERMATIONS (Id, Menu_ID, Parent_Id, USER_ID, PERMATION, PROJECT_ID, IsShowToUser, OperationDate, Insert_ServerDateTime, Edit_User_Id)
VALUES ({CallSnowflakeIdGenerate.GenerateNewId()}, {m.Id}, {m.Parent_Id}, {userId}, {value}, {ProjectId}, 1, {now}, {now}, {actingUserId})");
        }

        /// <summary>
        /// صفوف المستخدم الجديد: نسخة من صلاحيات مستخدم آخر، أو الافتراضى من SysConfigMenus.Value (مثل SetUpdateMenusPermisionAddedNew).
        /// لا يمس مستخدماً له صفوف بالفعل.
        /// </summary>
        public void SeedNewUser(long userId, long copyFrom, long actingUserId)
        {
            var opp = _newOpp();
            if (LoadGrants(opp, userId).Count > 0) return;
            var menus = LoadMenus(opp);
            HashSet<long> granted = copyFrom > 0
                ? GetGranted(copyFrom)
                : menus.Where(m => D(m.Value) == "1").Select(m => m.Id).ToHashSet();
            string yes = Common.EncryptMe("1"), no = Common.EncryptMe("0");
            var now = DateTime.Now;
            using var tx = opp.db.Database.BeginTransaction();
            foreach (var m in menus) Insert(opp, userId, m, granted.Contains(m.Id) ? yes : no, actingUserId, now);
            tx.Commit();
        }
    }
}
