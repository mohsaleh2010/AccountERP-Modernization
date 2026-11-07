using System;
using System.Collections.Generic;
using System.Linq;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib.SalesRep
{
    /// <summary>
    /// خطة المندوبين — المرحلة 1: ملف المندوب الميدانى (§5.1 SalesRep_Reps) والمعالج الذى ينشئ لمندوب موجود
    /// مخزن السيارة ومخزن تالفها (§4.1، ق3) وصندوق العهدة (§4.2) بحساباتها فى الشجرة، فى معاملة واحدة.
    /// المندوب = موظف بوظيفة Common.job_list_Id_SalesMan (نفس SalesMenCLS). لا يعدّل أى صف قائم: المخزن/الصندوق
    /// المختار من الموجود يُربط كما هو، والجديد فقط يُنشأ بـ IsCar = 1 وEmp_id = المندوب.
    /// </summary>
    public class SalesRepProfileService
    {
        private readonly operations _external;

        /// <summary>بلا معامل: كل حفظ فى معاملته ويُثبَّت. بمعاملة المستدعى (operations بعد BeginTransaction): لا Commit ولا Rollback هنا.</summary>
        public SalesRepProfileService(operations external = null) { _external = external; }

        private operations Begin() => _external ?? new operations(2);
        private void Done(operations opp) { if (_external == null) opp.Commit(); }
        private void Fail(operations opp) { if (_external == null) { try { opp.Rollback(); } catch { } } }

        public class RepRow
        {
            public long Id { get; set; }
            public long Employee_Id { get; set; }
            public string Employee_Name { get; set; } = "";
            public long Branch_Id { get; set; }
            public string Branch_Name { get; set; } = "";
            public long Van_Store_Id { get; set; }
            public string Van_Store_Name { get; set; } = "";
            public long Damaged_Store_Id { get; set; }
            public string Damaged_Store_Name { get; set; } = "";
            public long Cash_Box_Id { get; set; }
            public string Cash_Box_Name { get; set; } = "";
            public int Sales_Mode { get; set; }
            public long? Price_Type_List_ID { get; set; }
            public decimal? Max_Discount_Rate { get; set; }
            public decimal? Max_Cash_On_Hand { get; set; }
            public bool Allow_Credit_Sales { get; set; }
            public bool Allow_Blind_Return { get; set; }
            public int? Return_Window_Days { get; set; }
            public bool Allow_Cheque_Collection { get; set; }
            public int? Max_Cheque_Due_Days { get; set; }
            public bool Track_Location { get; set; }
            public int? Track_Interval_Sec { get; set; }
            public int End_Of_Day_Stock_Policy { get; set; }
            public long? Supervisor_Employee_Id { get; set; }
            public long? Cost_Center_Id { get; set; }
            public bool Active { get; set; }
            public bool Has_Shifts { get; set; }
        }

        public class Option
        {
            public long Id { get; set; }
            public string Name { get; set; } = "";
            public long Branch_Id { get; set; }
        }

        /// <summary>بيانات المعالج/التعديل. Van_Store_Id/Damaged_Store_Id/Cash_Box_Id = 0 تعنى «إنشاء جديد».</summary>
        public class ProfileInput
        {
            public long Id { get; set; }
            public long Employee_Id { get; set; }
            public long Van_Store_Id { get; set; }
            public long Damaged_Store_Id { get; set; }
            public long Cash_Box_Id { get; set; }
            public int Sales_Mode { get; set; }
            public long? Price_Type_List_ID { get; set; }
            public decimal? Max_Discount_Rate { get; set; }
            public decimal? Max_Cash_On_Hand { get; set; }
            public bool Allow_Credit_Sales { get; set; }
            public bool Allow_Blind_Return { get; set; }
            public int? Return_Window_Days { get; set; }
            public bool Allow_Cheque_Collection { get; set; }
            public int? Max_Cheque_Due_Days { get; set; }
            public bool Track_Location { get; set; }
            public int? Track_Interval_Sec { get; set; }
            public int End_Of_Day_Stock_Policy { get; set; }
            public long? Supervisor_Employee_Id { get; set; }
            public long? Cost_Center_Id { get; set; }
            public bool Active { get; set; } = true;
        }

        public List<RepRow> GetReps() => Read(opp => opp.db.Database.SqlQuery<RepRow>($@"
SELECT r.Id, r.Employee_Id, ISNULL(e.name, N'') AS Employee_Name, ISNULL(r.Branch_Id, 0) AS Branch_Id, ISNULL(b.Branch_Name, N'') AS Branch_Name,
       r.Van_Store_Id, ISNULL(vs.Store_Name, N'') AS Van_Store_Name, r.Damaged_Store_Id, ISNULL(ds.Store_Name, N'') AS Damaged_Store_Name,
       r.Cash_Box_Id, ISNULL(bx.Bank_Name, N'') AS Cash_Box_Name, r.Sales_Mode, r.Price_Type_List_ID, r.Max_Discount_Rate, r.Max_Cash_On_Hand,
       r.Allow_Credit_Sales, r.Allow_Blind_Return, r.Return_Window_Days, r.Allow_Cheque_Collection, r.Max_Cheque_Due_Days, r.Track_Location, r.Track_Interval_Sec, r.End_Of_Day_Stock_Policy, r.Supervisor_Employee_Id, r.Cost_Center_Id, r.Active,
       CAST(CASE WHEN EXISTS (SELECT 1 FROM SalesRep_Shifts s WHERE s.Rep_Id = r.Id) THEN 1 ELSE 0 END AS bit) AS Has_Shifts
FROM SalesRep_Reps r
LEFT JOIN Employee e ON e.Id = r.Employee_Id
LEFT JOIN Account_Branches b ON b.Id = r.Branch_Id
LEFT JOIN Store vs ON vs.Id = r.Van_Store_Id
LEFT JOIN Store ds ON ds.Id = r.Damaged_Store_Id
LEFT JOIN Banks bx ON bx.Id = r.Cash_Box_Id
ORDER BY r.Active DESC, e.name").ToList());

        /// <summary>المندوبون (وظيفة المندوب، نشطون) الذين ليس لهم ملف ميدانى بعد.</summary>
        public List<Option> GetCandidates() => Read(opp =>
        {
            long job = Common.job_list_Id_SalesMan;
            return opp.db.Database.SqlQuery<Option>($@"
SELECT e.Id, ISNULL(e.name, N'') AS Name, ISNULL(e.Branch_Id, 0) AS Branch_Id FROM Employee e
WHERE e.jobsListId = {job} AND e.active = 1 AND NOT EXISTS (SELECT 1 FROM SalesRep_Reps r WHERE r.Employee_Id = e.Id)
ORDER BY e.name").ToList();
        });

        /// <summary>كل الموظفين النشطين (للمشرف).</summary>
        public List<Option> GetEmployees() => Read(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT Id, ISNULL(name, N'') AS Name, ISNULL(Branch_Id, 0) AS Branch_Id FROM Employee WHERE active = 1 ORDER BY name").ToList());

        /// <summary>مخازن الفرع (طرف «مخزن») غير المربوطة بمندوب آخر.</summary>
        public List<Option> GetFreeStores(long branchId, long exceptRepId = 0) => Read(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT s.Id, s.Store_Name AS Name, ISNULL(s.Branch_Id, 0) AS Branch_Id FROM Store s
WHERE s.Active = 1 AND ISNULL(s.LookUp_StoreOrderTypesId, 4) = 4 AND ISNULL(s.IS_ServicesVirtual_Item_NoBalance, 0) = 0
  AND ISNULL(s.IS_Virtual_OuterStore, 0) = 0 AND ISNULL(s.Branch_Id, 0) = {branchId}
  AND NOT EXISTS (SELECT 1 FROM SalesRep_Reps r WHERE r.Id <> {exceptRepId} AND (r.Van_Store_Id = s.Id OR r.Damaged_Store_Id = s.Id))
ORDER BY ISNULL(s.IsCar, 0) DESC, s.Store_Name").ToList());

        /// <summary>صناديق الفرع (Is_Bank = 0) غير المربوطة بمندوب آخر.</summary>
        public List<Option> GetFreeBoxes(long branchId, long exceptRepId = 0) => Read(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT b.Id, b.Bank_Name AS Name, ISNULL(b.Branch_Id, 0) AS Branch_Id FROM Banks b
WHERE ISNULL(b.Is_Bank, 0) = 0 AND ISNULL(b.Active, 1) = 1 AND ISNULL(b.Branch_Id, 0) = {branchId}
  AND NOT EXISTS (SELECT 1 FROM SalesRep_Reps r WHERE r.Id <> {exceptRepId} AND r.Cash_Box_Id = b.Id)
ORDER BY b.Bank_Name").ToList());

        public List<Option> GetPriceLists() => Read(opp => opp.db.Database.SqlQuery<Option>($@"
SELECT CAST(ID AS bigint) AS Id, ISNULL(Price_Name, N'') AS Name, CAST(0 AS bigint) AS Branch_Id FROM Price_Type_List WHERE ISNULL(IS_Active, 1) = 1 ORDER BY Price_Name").ToList());

        /// <summary>
        /// ينشئ ملف المندوب. أى طرف بقيمة 0 يُنشأ: مخزن السيارة «سيارة - اسم المندوب»، مخزن التالف «تالف سيارة - اسم المندوب»،
        /// صندوق العهدة «عهدة - اسم المندوب»، كلها بفرع المندوب وبحساباتها. تُرجع معرّف الملف.
        /// </summary>
        public long Create(ProfileInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var opp = Begin();
            try
            {
                var emp = opp.db.Employees.AsNoTracking().FirstOrDefault(e => e.Id == input.Employee_Id)
                          ?? throw new InvalidOperationException("الموظف غير موجود.");
                if (emp.jobsListId != Common.job_list_Id_SalesMan || emp.active != 1)
                    throw new InvalidOperationException("الموظف ليس مندوب مبيعات نشطاً.");
                if (opp.db.SalesRep_Reps.Any(r => r.Employee_Id == emp.Id))
                    throw new InvalidOperationException("للمندوب ملف ميدانى بالفعل.");

                long branchId = emp.Branch_Id is > 0 ? emp.Branch_Id.Value : Common.Branch_Id;
                string name = (emp.name ?? "").Trim();
                ValidatePolicies(input);

                long vanId = input.Van_Store_Id > 0 ? CheckStore(opp, input.Van_Store_Id, branchId, 0) : CreateStore(opp, "سيارة - " + name, emp, branchId);
                long damagedId = input.Damaged_Store_Id > 0 ? CheckStore(opp, input.Damaged_Store_Id, branchId, 0) : CreateStore(opp, "تالف سيارة - " + name, emp, branchId);
                if (vanId == damagedId) throw new InvalidOperationException("مخزن التالف يجب أن يختلف عن مخزن السيارة (ق3).");
                long boxId = input.Cash_Box_Id > 0 ? CheckBox(opp, input.Cash_Box_Id, branchId, 0) : CreateBox(opp, "عهدة - " + name, branchId);

                var rep = new SalesRep_Rep
                {
                    Id = CallSnowflakeIdGenerate.GenerateNewId(),
                    Employee_Id = emp.Id,
                    Van_Store_Id = vanId,
                    Damaged_Store_Id = damagedId,
                    Cash_Box_Id = boxId,
                };
                Apply(rep, input);
                rep.StampInsert(branchId);
                opp.db.SalesRep_Reps.Add(rep);
                opp.db.SaveChanges();
                Done(opp);
                return rep.Id;
            }
            catch
            {
                Fail(opp);
                throw;
            }
        }

        /// <summary>تعديل السياسات. تغيير المخازن/الصندوق مسموح فقط قبل أول وردية (الوردية تقرأ رصيد السيارة من دفترها).</summary>
        public void Update(ProfileInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var opp = Begin();
            try
            {
                var rep = opp.db.SalesRep_Reps.FirstOrDefault(r => r.Id == input.Id) ?? throw new InvalidOperationException("الملف غير موجود.");
                ValidatePolicies(input);
                bool linksChanged = input.Van_Store_Id != rep.Van_Store_Id || input.Damaged_Store_Id != rep.Damaged_Store_Id || input.Cash_Box_Id != rep.Cash_Box_Id;
                if (linksChanged)
                {
                    if (opp.db.SalesRep_Shifts.Any(s => s.Rep_Id == rep.Id))
                        throw new InvalidOperationException("لا يتغير مخزن السيارة أو التالف أو الصندوق بعد أول وردية.");
                    long branchId = rep.Branch_Id ?? Common.Branch_Id;
                    rep.Van_Store_Id = CheckStore(opp, input.Van_Store_Id, branchId, rep.Id);
                    rep.Damaged_Store_Id = CheckStore(opp, input.Damaged_Store_Id, branchId, rep.Id);
                    if (rep.Van_Store_Id == rep.Damaged_Store_Id) throw new InvalidOperationException("مخزن التالف يجب أن يختلف عن مخزن السيارة (ق3).");
                    rep.Cash_Box_Id = CheckBox(opp, input.Cash_Box_Id, branchId, rep.Id);
                }
                Apply(rep, input);
                rep.StampEdit();
                opp.db.SaveChanges();
                Done(opp);
            }
            catch
            {
                Fail(opp);
                throw;
            }
        }

        private static void Apply(SalesRep_Rep rep, ProfileInput input)
        {
            rep.Sales_Mode = input.Sales_Mode;
            rep.Price_Type_List_ID = input.Price_Type_List_ID is > 0 ? input.Price_Type_List_ID : null;
            rep.Max_Discount_Rate = input.Max_Discount_Rate;
            rep.Max_Cash_On_Hand = input.Max_Cash_On_Hand;
            rep.Allow_Credit_Sales = input.Allow_Credit_Sales;
            rep.Allow_Blind_Return = input.Allow_Blind_Return;
            rep.Return_Window_Days = input.Return_Window_Days;
            rep.Allow_Cheque_Collection = input.Allow_Cheque_Collection;
            rep.Max_Cheque_Due_Days = input.Allow_Cheque_Collection ? input.Max_Cheque_Due_Days : null;
            rep.Track_Location = input.Track_Location;
            rep.Track_Interval_Sec = input.Track_Location && input.Track_Interval_Sec is int sec ? Math.Clamp(sec, 30, 900) : null;
            rep.End_Of_Day_Stock_Policy = input.End_Of_Day_Stock_Policy;
            rep.Supervisor_Employee_Id = input.Supervisor_Employee_Id is > 0 ? input.Supervisor_Employee_Id : null;
            rep.Cost_Center_Id = input.Cost_Center_Id is > 0 ? input.Cost_Center_Id : null;
            rep.Active = input.Active;
        }

        private static void ValidatePolicies(ProfileInput input)
        {
            if (!Enum.IsDefined(typeof(SalesRepSalesMode), input.Sales_Mode)) throw new InvalidOperationException("نمط البيع غير صحيح.");
            if (!Enum.IsDefined(typeof(SalesRepEndOfDayStockPolicy), input.End_Of_Day_Stock_Policy)) throw new InvalidOperationException("سياسة المتبقى آخر اليوم غير صحيحة.");
            if (input.Max_Discount_Rate is < 0 or > 100) throw new InvalidOperationException("سقف الخصم بين 0 و100%.");
            if (input.Return_Window_Days is < 0 or > 3650) throw new InvalidOperationException("نافذة الإرجاع بين 0 و3650 يوماً.");
            if (input.Max_Cheque_Due_Days is < 0 or > 3650) throw new InvalidOperationException("أقصى أيام استحقاق الشيك بين 0 و3650 يوماً.");
            if (input.Max_Cash_On_Hand is < 0) throw new InvalidOperationException("حد النقدية بالعهدة لا يكون سالباً.");
            if (input.Supervisor_Employee_Id is > 0 && input.Supervisor_Employee_Id == input.Employee_Id) throw new InvalidOperationException("المشرف لا يكون المندوب نفسه.");
        }

        private static long CheckStore(operations opp, long storeId, long branchId, long exceptRepId)
        {
            var s = opp.db.Stores.AsNoTracking().FirstOrDefault(x => x.Id == storeId) ?? throw new InvalidOperationException("المخزن غير موجود.");
            if (s.Active != 1 || (s.LookUp_StoreOrderTypesId ?? StoreOrderService.PartyStore) != StoreOrderService.PartyStore)
                throw new InvalidOperationException($"المخزن «{s.Store_Name}» غير نشط أو ليس مخزناً.");
            if ((s.Branch_Id ?? 0) != branchId) throw new InvalidOperationException($"المخزن «{s.Store_Name}» من فرع آخر غير فرع المندوب.");
            if (opp.db.SalesRep_Reps.Any(r => r.Id != exceptRepId && (r.Van_Store_Id == storeId || r.Damaged_Store_Id == storeId)))
                throw new InvalidOperationException($"المخزن «{s.Store_Name}» مربوط بمندوب آخر.");
            return storeId;
        }

        private static long CheckBox(operations opp, long boxId, long branchId, long exceptRepId)
        {
            var b = opp.db.Banks.AsNoTracking().FirstOrDefault(x => x.Id == boxId) ?? throw new InvalidOperationException("الصندوق غير موجود.");
            if ((b.Is_Bank ?? 0) != 0) throw new InvalidOperationException($"«{b.Bank_Name}» بنك وليس صندوقاً.");
            if ((b.Branch_Id ?? 0) != branchId) throw new InvalidOperationException($"الصندوق «{b.Bank_Name}» من فرع آخر غير فرع المندوب.");
            if (opp.db.SalesRep_Reps.Any(r => r.Id != exceptRepId && r.Cash_Box_Id == boxId))
                throw new InvalidOperationException($"الصندوق «{b.Bank_Name}» مربوط بمندوب آخر.");
            return boxId;
        }

        /// <summary>ينشئ مخزناً (طرف 4) بحسابه فى الشجرة تحت «مخازن إنتاج تام» عبر نفس مسار شاشة المخازن.</summary>
        private static long CreateStore(operations opp, string name, Employee emp, long branchId)
        {
            DateTime now = DateTime.Now;
            var store = new Store
            {
                Store_Name = name,
                Store_Code = "",
                Store_location = "",
                Employee_Name = emp.name ?? "",
                StoreUsed_Type = 1,
                Emp_id = emp.Id,
                Job_id = emp.jobsListId ?? 0,
                IS_Default_Store = 0,
                IS_Sell_SecondUnit = 0,
                IS_ItemSecond_degree = 0,
                IS_Virtual_OuterStore = 0,
                IS_ServicesVirtual_Item_NoBalance = 0,
                IsCar = 1,
                Active = 1,
                LookUp_StoreOrderTypesId = StoreOrderService.PartyStore,
                Branch_Id = branchId,
                User_Id = Common.User_Id,
                OperationDate = now.Date,
                Insert_ServerDateTime = now,
            };
            int excp = 0; string msg = "", code = ""; long subId = -1;
            var acc = new Sub_Account { store = store };
            acc.Add_AccountSupplierCustomerOther(ref excp, ref msg, ref opp, ref subId, ref code, Common.Account_Type_ForStoreEntagTam, name, branchId, 1);
            if (excp != 0 || store.Id <= 0) throw new InvalidOperationException("تعذر إنشاء المخزن «" + name + "»: " + msg);
            return store.Id;
        }

        /// <summary>ينشئ صندوقاً (Is_Bank = 0) بحسابه تحت «حساب الصندوق» عبر نفس مسار شاشة الصناديق.</summary>
        private static long CreateBox(operations opp, string name, long branchId)
        {
            var branchName = opp.db.Database.SqlQuery<string>($"SELECT ISNULL(Branch_Name, N'') AS Value FROM Account_Branches WHERE Id = {branchId}").AsEnumerable().FirstOrDefault() ?? "";
            var ba = new Bank
            {
                Bank_Name = name,
                Account_Type_Value = Common.Sandouk_Account_Type,
                Is_Bank = 0,
                Branch_Id = branchId,
                Branch_Name = branchName,
                Currency_Id = 1,
                Primary_balance = 0,
                firstBalanceDate = DateTime.Now.Date,
                Supplier_Bank = 0,
                Default_Bank = 0,
                Account_No = "",
                Active = 1,
            };
            int excp = 0; string msg = "", code = ""; long subId = -1;
            var acc = new Sub_Account { ba = ba };
            acc.Add_AccountSupplierCustomerOther(ref excp, ref msg, ref opp, ref subId, ref code, Common.Sandouk_Account_Type, name, branchId, 1);
            long id = acc.New_Account_Id;
            if (excp != 0 || id <= 0) throw new InvalidOperationException("تعذر إنشاء صندوق العهدة: " + msg);
            return id;
        }

        private T Read<T>(Func<operations, T> f) => f(_external ?? new operations());
    }
}
