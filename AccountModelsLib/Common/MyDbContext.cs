 
using AccountModelsLib.Models;
using AccountModelsLib.SalesRep;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // 👈 إضافة هامة للتعرف على EntityEntry
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
namespace AccountModelsLib
{
    public class My_DbContext : DbContext
    {
        private readonly DataDictionaryDbSyncLogsEFCls _syncLogsHelper = new DataDictionaryDbSyncLogsEFCls();
         
        public static string ConnStr { get; set; } = "";
        // 1. المشيد الافتراضي بدون برامترات
        public My_DbContext()
        {
            EnsureDatabaseCreated();
        }
        public My_DbContext(DbConnection conn, bool contextOwnsConnection = true)
    : base(CreateOptions(conn, contextOwnsConnection))
        {
        }
        private static DbContextOptions<My_DbContext> CreateOptions(DbConnection conn, bool contextOwnsConnection)
        {
            var optionsBuilder = new DbContextOptionsBuilder<My_DbContext>();

            // ربط الـ DbConnection القائم مع تحديد هل يمتلك الـ Context هذا الاتصال أم لا
            optionsBuilder.UseSqlServer(conn, contextOptions =>
            {
                // إعدادات إضافية إذا لزم الأمر
            });

            return optionsBuilder.Options;
        }
        // 2. مشيد يستقبل نص الاتصال
        public My_DbContext(string connStr)
        {
            if (!string.IsNullOrEmpty(connStr))
            {
                ConnStr = connStr;
            }
            EnsureDatabaseCreated();
        }

        // 3. مشيد يستقبل الخيارات (DbContextOptions) وهو الأسلوب القياسي في EF Core
        public My_DbContext(DbContextOptions<My_DbContext> options) : base(options)
        {
            EnsureDatabaseCreated();
        }

        // بديل لـ SetInitializer في EF Core
        private void EnsureDatabaseCreated()
        {
            // ينشئ قاعدة البيانات إذا لم تكن موجودة (بديل CreateDatabaseIfNotExists)
            Database.EnsureCreated();
        }

        // تهيئة نص الاتصال في EF Core
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                try
                {
                    if (string.IsNullOrEmpty(ConnStr))
                    {
                        AccountModelsLib.operations opp = new AccountModelsLib.operations();
                        ConnStr = AccountModelsLib.operations.conn_str_;
                    }
                }
                catch (Exception)
                {
                    // يمكن إضافة معالجة للخطأ هنا
                }

                if (!string.IsNullOrEmpty(ConnStr))
                {
                    // لاستخدام SQL Server
                   
                    optionsBuilder
        .UseSqlServer(ConnStr)
        .EnableDetailedErrors() // <--- ADD THIS
        .EnableSensitiveDataLogging(); // <--- OPTIONAL: Shows column parameter values
                }

                
            }
        }

        // ضبط الدقة (Precision) للقيم العشرية (Decimal)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ضبط الدقة (18, 6) لجميع الخصائص من نوع decimal
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(18);
                        property.SetScale(6);
                    }
                }
            }
        }

        // حفظ التغييرات بشكل غير متزامن

        /*
        public My_DbContext() : base(ConnStr)
        {
            try
            {
                if (ConnStr == "")
                {
                    AccountModelsLib.operations opp = new AccountModelsLib.operations();
                    ConnStr = AccountModelsLib.operations.conn_str_;
                }
            }
            catch (Exception ee)
            {

            }
            Database.SetInitializer<My_DbContext>(new CreateDatabaseIfNotExists<My_DbContext>());

        }
        public My_DbContext(string ConnStr) : base(ConnStr)
        {
            Database.SetInitializer<My_DbContext>(new CreateDatabaseIfNotExists<My_DbContext>());

        }
        public My_DbContext(DbConnection Conn, bool contextOwnsConnection) : base(Conn, contextOwnsConnection)
        {

        }

         

        public override async Task<int> SaveChangesAsync()
        {
            return await SaveChangesAsync(CancellationToken.None);
        }
        */

        /*
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (Common.Use_Auto_Identity_Column == 1)
                {
                    // 1. قراءة التغيرات قبل أي إجراء على قاعدة البيانات
                    var pendingEntries = _syncLogsHelper.GetPendingEntries(this);

                    // الحفظ الأول (غير متزامن) لتوليد أرقام الـ Identity
                    int result = await base.SaveChangesAsync(cancellationToken);

                    // إنشاء سجلات المزامنة بناءً على الـ IDs الجديدة
                    _syncLogsHelper.CreateSyncLogs(pendingEntries, this);

                    // الحفظ الثاني (غير متزامن) لحفظ سجلات المزامنة
                    await base.SaveChangesAsync(cancellationToken);

                    return result;
                }
                else
                {
                    // 1. الخطوة الأولى: توليد وإسناد الـ Snowflake ID لكل الكائنات الجديدة في الـ Memory
                    _syncLogsHelper.AssignSnowflakeIdsToNewEntities(this);

                    // 2. الخطوة الثانية: إنشاء سجلات المزامنة
                    _syncLogsHelper.GenerateSyncLogs(this);

                    // 3. الخطوة الثالثة: حفظ البيانات وسجلات المزامنة معاً دفعة واحدة (غير متزامن)
                    return await base.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ee)
            {
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                throw;
            }
        }
        public override int SaveChanges()
        { 
            try
            {
                if (Common.Use_Auto_Identity_Column == 1)
                {
                    // 1. قراءة التغيرات قبل أي إجراء على قاعدة البيانات
                    var pendingEntries = _syncLogsHelper.GetPendingEntries(this);
                    int result = base.SaveChanges();
                    _syncLogsHelper.CreateSyncLogs(pendingEntries, this);
                    base.SaveChanges();
                    
                    return result;
                }
                else
                {
                    // 1. الخطوة الأولى: توليد وإسناد الـ Snowflake ID لكل الكائنات الجديدة التي لم يُحدد لها Id بعد
                    _syncLogsHelper.AssignSnowflakeIdsToNewEntities(this);

                    // 2. الخطوة الثانية: إنشاء سجلات المزامنة (الآن جميع الكائنات المضافة لديها Id جاهز في الـ Memory!)
                    _syncLogsHelper.GenerateSyncLogs(this);

                    // 3. الخطوة الثالثة: حفظ البيانات وسجلات المزامنة معاً في استدعاء واحد وقاعدة البيانات دفعة واحدة!
                    return base.SaveChanges();
                }
            }
            catch (Exception ee)
            {
                GetEntityWhenError();
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                //return 0;
                throw;
            }
        }

        */

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // صمام الأمان العام: إذا كانت المزامنة معطلة، احفظ مباشرة دون تتبع
            if (Common.Is_Save_DataDictionaryDbSyncLogs != 1)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            try
            {
                if (Common.Use_Auto_Identity_Column == 1)
                {
                    // 1. قراءة التغيرات قبل الحفظ (لأن الـ Identity لم يُولد بعد)
                    var pendingEntries = _syncLogsHelper.GetPendingEntries(this);

                    // 2. الحفظ الأول لتوليد أرقام الـ Identity الفعلية من قاعدة البيانات
                    int result = await base.SaveChangesAsync(cancellationToken);

                    // 3. إنشاء أو دمج سجلات المزامنة باستخدام الطريقة الذكية الموحدة
                    _syncLogsHelper.CreateSyncLogs(pendingEntries, this);

                    // 4. الحفظ الثاني لحفظ سجلات المزامنة
                    await base.SaveChangesAsync(cancellationToken);

                    return result;
                }
                else
                {
                    // 1. توليد وإسناد الـ Snowflake ID لكل الكائنات الجديدة في الـ Memory
                    _syncLogsHelper.AssignSnowflakeIdsToNewEntities(this);

                    // 2. قراءة التغيرات بعد توفر الـ IDs في الذاكرة
                    var pendingEntries = _syncLogsHelper.GetPendingEntries(this);

                    // 3. إنشاء أو دمج سجلات المزامنة باستخدام الطريقة الذكية الموحدة
                    _syncLogsHelper.CreateSyncLogs(pendingEntries, this);

                    // 4. حفظ البيانات وسجلات المزامنة معاً دفعة واحدة (Transaction واحدة)
                    return await base.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ee)
            {
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                throw;
            }
        }
        public override int SaveChanges()
        {
            // صمام الأمان العام: إذا كانت المزامنة معطلة، احفظ مباشرة دون تتبع
            //if (Common.Is_Save_DataDictionaryDbSyncLogs != 1)
            //{
            //    return base.SaveChanges();
            //}

            try
            {
                if (Common.Use_Auto_Identity_Column == 1)
                {
                    // 1. قراءة التغيرات قبل الحفظ (لأن الـ Identity لم يُولد بعد)
                    var pendingEntries = _syncLogsHelper.GetPendingEntries(this);

                    // 2. الحفظ الأول لتوليد أرقام الـ Identity الفعلية من قاعدة البيانات
                    int result = base.SaveChanges();

                    // 3. إنشاء أو دمج سجلات المزامنة باستخدام الطريقة الذكية الموحدة
                    _syncLogsHelper.CreateSyncLogs(pendingEntries, this);

                    // 4. الحفظ الثاني لحفظ سجلات المزامنة
                    base.SaveChanges();

                    return result;
                }
                else
                {
                    // 1. توليد وإسناد الـ Snowflake ID لكل الكائنات الجديدة في الـ Memory
                    _syncLogsHelper.AssignSnowflakeIdsToNewEntities(this);

                    // 2. قراءة التغيرات بعد توفر الـ IDs في الذاكرة
                    var pendingEntries = _syncLogsHelper.GetPendingEntries(this);

                    // 3. إنشاء أو دمج سجلات المزامنة باستخدام الطريقة الذكية الموحدة
                    _syncLogsHelper.CreateSyncLogs(pendingEntries, this);

                    // 4. حفظ البيانات وسجلات المزامنة معاً دفعة واحدة (Transaction واحدة)
                    return base.SaveChanges();
                }
            }
            catch (Exception ee)
            {
                AccountModelsLib.operations.DataDictionaryLogerException(ee);
                throw;
            }
        }
        void GetEntityWhenError()
        {
            // Get all tracked entity entries that are not unchanged or detached
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in modifiedEntries)
            {
                // The actual entity instance
                object entity = entry.Entity;

                // The state: Added, Modified, or Deleted
                EntityState state = entry.State;

                // Example: Target specific entity types
                if (entity is Customer customer)
                {
                    // Access specific Customer properties
                    var customerId = customer.Id;
                }
            }
        }
        
        // الدالة الأولى: التقاط العناصر المعدلة وحالتها قبل الحفظ
        private List<PendingSyncEntry> GetPendingEntries1()
        {
            List<PendingSyncEntry> pendingSyncEntries = new List<PendingSyncEntry>();
            if (Common.Is_Save_DataDictionaryDbSyncLogs == 1)
            {
                pendingSyncEntries =
                  ChangeTracker.Entries()
                    .Where(e => !(e.Entity is DataDictionaryDbSyncLogs) &&
                               (e.State == EntityState.Added ||
                                e.State == EntityState.Modified ||
                                e.State == EntityState.Deleted))
                    .Select(e => new PendingSyncEntry
                    {
                        Entry = e,
                        SavedState = e.State,
                        TableName = e.Metadata.GetTableName()
                         
                    })
                    .ToList();
            }
            return pendingSyncEntries;
        }
        // -------------------------------------------------------------
        // الدالة الثالثة: التحقق من وجود السجل وتحديث ActionType2 أو إضافة سجل جديد
        // -------------------------------------------------------------
        private void CreateSyncLogs1(List<PendingSyncEntry> pendingEntries)
        {
            if (Common.Is_Save_DataDictionaryDbSyncLogs == 1)
            {
                foreach (var item in pendingEntries)
                {
                    var idProperty = item.Entry.Property("Id");
                    if (idProperty == null || idProperty.CurrentValue == null) continue;

                    long recordId = Convert.ToInt64(idProperty.CurrentValue);

                    string action = item.SavedState switch
                    {
                        EntityState.Added => "INSERT",
                        EntityState.Modified => "UPDATE",
                        EntityState.Deleted => "DELETE",
                        _ => "UNKNOWN"
                    };

                    // 1. البحث عن سجل معلق سابقاً لنفس الجدول والرقم
                    var existingLog = DataDictionaryDbSyncLogs.FirstOrDefault(x =>
                        x.TableName == item.TableName &&
                        x.RecordId == recordId &&
                        x.Status == "Pending");

                    if (existingLog != null)
                    {
                        // 2. حالة العثور على السجل: نضع التغيير الجديد في ActionType2
                        existingLog.ActionType2 = action; // سينتج عنه 'UPDATE' أو 'DELETE'
                        existingLog.CreatedAt = DateTime.UtcNow; // تحديث توقيت أحدث تعديل
                    }
                    else
                    {
                        // 3. حالة عدم وجود السجل: إنشاء سجل جديد
                        var newSyncLog = new DataDictionaryDbSyncLogs
                        {
                            TableName = item.TableName,
                            RecordId = recordId,
                            ActionType = action,
                            ActionType2 = null, // لا يوجد إجراء ثاني بعد
                            Status = "Pending",
                            CreatedAt = DateTime.UtcNow
                        };

                        DataDictionaryDbSyncLogs.Add(newSyncLog);
                    }
                }
            }
        }
        private void GenerateSyncLogs1()
        {
            if (Common.Is_Save_DataDictionaryDbSyncLogs == 1)
            {
                // 2. استبعاد DataDictionaryDbSyncLogs لمنع التكرار اللانهائي
                var modifiedEntries = ChangeTracker.Entries()
                .Where(e => !(e.Entity is DataDictionaryDbSyncLogs) &&
                           (e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted))
                .ToList();

                if (!modifiedEntries.Any()) return;

                var syncLogs = new List<DataDictionaryDbSyncLogs>();

                foreach (var entry in modifiedEntries)
                {
                    string tableName = entry.Metadata.GetTableName();

                    var idProperty = entry.Property("Id");
                    if (idProperty == null || idProperty.CurrentValue == null) continue;

                    long recordId = Convert.ToInt64(idProperty.CurrentValue);

                    string action = entry.State switch
                    {
                        EntityState.Added => "INSERT",
                        EntityState.Modified => "UPDATE",
                        EntityState.Deleted => "DELETE",
                        _ => "UNKNOWN"
                    };

                    // 3. إنشاء كائن من الكلاس الجديد
                    syncLogs.Add(new DataDictionaryDbSyncLogs
                    {
                        TableName = tableName,
                        RecordId = recordId,
                        ActionType = action,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (syncLogs.Any())
                {
                    DataDictionaryDbSyncLogs.AddRange(syncLogs);
                }
            }
        }
        private void AssignSnowflakeIdsToNewEntities1()
        {
            var addedEntries = ChangeTracker.Entries()
        .Where(e => !(e.Entity is DataDictionaryDbSyncLogs) && e.State == EntityState.Added)
        .ToList();

            foreach (var entry in addedEntries)
            {
                var idProperty = entry.Property("Id");

                // إذا كان الـ Id لم يُحدد بعد (يساوي 0)
                if (idProperty != null && Convert.ToInt64(idProperty.CurrentValue) == 0)
                {
                    long newSnowflakeId = CallSnowflakeIdGenerate.GenerateNewId(); // توليد الرقم
                    idProperty.CurrentValue = newSnowflakeId;               // تعيين القيمة
                }
            }
        }
        
       

       
        public DbSet<Account_Type_Main_Account_Id> account_Type_Main_Account_Id { get; set; }



        public DbSet<FixedAssetData> fixedAssetData { get; set; }
        

        public DbSet<Unit_item> Unit_item { get; set; }
        public DbSet<Items> Item { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Store_Account_Determination> Store_Account_Determination { get; set; }
    
        public DbSet<Draws_List> DrawsLists { get; set; }
        public DbSet<SupplierPrice> SupplierPrice { get; set; }
         
        public DbSet<Unit_Item_ExpireDate> Unit_Item_ExpireDates { get; set; }
        public DbSet<Unit_Item_SalesDate> Unit_Item_SalesDates { get; set; }
        public DbSet<Unit_Item_BatchGroupList> Unit_Item_BatchGroupLists { get; set; }
        public DbSet<Unit_Item_Sales_CustomerList> Unit_Item_Sales_CustomerLists { get; set; }
        public DbSet<Unit_Item_Sales_ItemList> Unit_Item_Sales_ItemLists { get; set; }
        public DbSet<LookUp_ItemServiceCategory> LookUp_ItemServiceCategory { get; set; }


        


        public virtual DbSet<unit> unit { get; set; }


        public DbSet<AccountModelsLib.Models.Customer> Customer { get; set; }
        public DbSet<Customers_Accounts> Customers_Accounts { get; set; }
        public DbSet<Customers_Accounts_Mokhtalat> Customers_Accounts_Mokhtalat { get; set; }


        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Suppliers_Accounts> Suppliers_Accounts { get; set; }
        public DbSet<Account_Entry_List> Account_Entry_List { get; set; }
        public DbSet<Account_Entry_Details> Account_Entry_Details { get; set; }
        public DbSet<Submain_Account> Submain_Account { get; set; }
        
        public DbSet<Checks_Memory> Checks_Memory { get; set; }
        public DbSet<LookUp_CheckStatus> LookUp_CheckStatus { get; set; }
        public DbSet<Lookup_CarShape> Lookup_CarShape { get; set; }
        public DbSet<Lookup_CarEntryTaskeen> Lookup_CarEntryTaskeen { get; set; }
        public DbSet<Lookup_IsInsuranceCompanyStaus> Lookup_IsInsuranceCompanyStaus { get; set; }

        public DbSet<LookUp_CarStatusLocation> LookUp_CarStatusLocation { get; set; }


        public DbSet<AccountTaxInfo> AccountTaxInfo { get; set; }

        public DbSet<Lookup_CashPaymentMethod> Lookup_CashPaymentMethod { get; set; }
        public DbSet<Lookup_TransmissionType> Lookup_TransmissionType { get; set; }


        public DbSet<Items_Group> Items_Groups { get; set; }
        public DbSet<Items_Nature> Items_Natures { get; set; }


        public DbSet<LookUp_CodeTaxType> LookUp_CodeTaxTypes { get; set; }
        public DbSet<LookUp_TaxableType> LookUp_TaxableTypes { get; set; }
        public DbSet<LookUp_TaxableSubType> LookUp_TaxableSubTypes { get; set; }
        public DbSet<LookUp_NonTaxableType> LookUp_NonTaxableTypes { get; set; }
        public DbSet<Invoices_List> Invoices_List { get; set; }
        public DbSet<Invoices_Details> Invoices_Details { get; set; }

        public DbSet<Invoices_ListTemp> Invoices_ListTemp { get; set; }
        public DbSet<Invoices_DetailsTemp> Invoices_DetailsTemp { get; set; }


        public DbSet<Invoices_List_Reverse> Invoices_List_Reverse { get; set; }
        public DbSet<Invoices_Details_Reverse> Invoices_Details_Reverse { get; set; }


        public DbSet<Bills_List> Bills_List { get; set; }
        public DbSet<Bills_Details> Bills_Details { get; set; }
        public DbSet<Bills_Details_Reverse> Bills_Details_Reverse { get; set; }
        public DbSet<Bills_List_Reverse> Bills_List_Reverse { get; set; }
        public DbSet<RequestsDetail> RequestsDetails { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderLine> CustomerOrderLines { get; set; }
        public DbSet<CustomerOrderDeposit> CustomerOrderDeposits { get; set; }
        public DbSet<CustomerOrderFulfillment> CustomerOrderFulfillments { get; set; }

        public DbSet<Gov_Country_List> Gov_Country_List { get; set; }
        public DbSet<Govern_Area_List> Govern_Area_List { get; set; }
        public DbSet<Govern_List> Govern_List { get; set; }
        public DbSet<Gov_City_List> Gov_City_List { get; set; }

        public DbSet<Car> Cars { get; set; }
        public DbSet<CarsWorkOrder> CarsWorkOrders { get; set; }
        public DbSet<CarsExamination> CarsExaminations { get; set; }

        
        public DbSet<CarsWorkOrdersRequestedDetail> CarsWorkOrdersRequestedDetails { get; set; }
        public DbSet<CarsWorkOrdersDeliverDetail> CarsWorkOrdersDeliverDetails { get; set; }
        public DbSet<CarsWorkOrdersDeliverItemBalance> CarsWorkOrdersDeliverItemBalances { get; set; }
        public DbSet<CarsWorkOrdersItemProcessedDetail> CarsWorkOrdersItemProcessedDetails { get; set; }
        public DbSet<CarsWorkOrdersItemProcessedEmployee> CarsWorkOrdersItemProcessedEmployees { get; set; }
        public DbSet<CarEstimationsValuesList> CarEstimationsValuesLists { get; set; }
        public DbSet<CarEstimationsValuesDetail> CarEstimationsValuesDetails { get; set; }

      

        public DbSet<Machin_list> Machin_list { get; set; }
        public DbSet<ShipmentCompany> ShipmentCompany { get; set; }
        public DbSet<Customer_Supplier_Group> Customer_Supplier_Group { get; set; }
        public DbSet<Item_BarCodeSetting> Item_BarCodeSetting { get; set; }
        public DbSet<Item_BarCodeSettingDefault> Item_BarCodeSettingDefault { get; set; }


        public DbSet<Order_Purshing> Order_Purshings { get; set; }
        public DbSet<Detailes_Purshing> Detailes_Purshings { get; set; }
        public DbSet<Detailes_Purshing_Import_Export> Detailes_Purshing_Import_Exports { get; set; }
        public DbSet<Item_Branch_Cost> Item_Branch_Costs { get; set; }


        public DbSet<Lookup_CarType> Lookup_CarType { get; set; }
        public DbSet<Lookup_CarTypeModel> Lookup_CarTypeModel { get; set; }
        public DbSet<ItemsForCars> ItemsForCars { get; set; }


        /////////////////////////
        ///Tax
        public DbSet<ItemCodeTaxAccepted> ItemCodeTaxAccepted { get; set; }
        public DbSet<ItemCodeTaxNotUploaded> ItemCodeTaxNotUploaded { get; set; }
        public DbSet<Tax_Branch> Tax_Branches { get; set; }
        public DbSet<Tax_BranchConfig> Tax_BranchesConfig { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<TaxInvoices_LineItems> TaxInvoices_LineItems { get; set; }
        public DbSet<TaxInvoices_ItemsTaxes> TaxInvoices_ItemsTax { get; set; }
        public DbSet<TaxInvoices_List> TaxInvoices_List { get; set; }
        public DbSet<TaxInvoices_TaxesTotal> TaxInvoices_TaxesTotal { get; set; }
        public DbSet<Taxdocument> Taxdocuments { get; set; }
        public DbSet<TaxdocumentReverse> TaxdocumentsReverse { get; set; }

        public DbSet<TaxReverseInvoices_LineItems> TaxReverseInvoices_LineItems { get; set; }
        public DbSet<TaxReverseInvoices_ItemsTaxes> TaxReverseInvoices_ItemsTax { get; set; }
        public DbSet<TaxReverseInvoices_List> TaxReverseInvoices_List { get; set; }
        public DbSet<TaxReverseInvoices_TaxesTotal> TaxReverseInvoices_TaxesTotal { get; set; }


        public DbSet<DataDictionaryLogerException> DataDictionaryLogerExceptions { get; set; }
        public DbSet<DataDictionaryLogerProcess> DataDictionaryLogerProcesses { get; set; }
        public DbSet<DataDictionaryProcessesList> DataDictionaryProcessesLists { get; set; }
        public DbSet<DataDictionaryDbSyncLogs> DataDictionaryDbSyncLogs { get; set; }

        
        public DbSet<SysPrinterSettings> PrinterSettings { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentService> AppointmentServices { get; set; }
        public DbSet<AppointmentServiceEmployee> AppointmentServiceEmployees { get; set; }
        public DbSet<AppointmentSession> AppointmentSessions { get; set; }
        public DbSet<AppointmentSessionEmployee> AppointmentSessionEmployees { get; set; }
        public DbSet<AppointmentPackage> AppointmentPackages { get; set; }

        public DbSet<SalesRep_Rep> SalesRep_Reps { get; set; }
        public DbSet<SalesRep_Device> SalesRep_Devices { get; set; }
        public DbSet<SalesRep_Device_Activation> SalesRep_Device_Activations { get; set; }
        public DbSet<SalesRep_Device_Dataset> SalesRep_Device_Datasets { get; set; }
        public DbSet<SalesRep_Track_Point> SalesRep_Track_Points { get; set; }
        public DbSet<SalesRep_Shift> SalesRep_Shifts { get; set; }
        public DbSet<SalesRep_Shift_Stock> SalesRep_Shift_Stock { get; set; }
        public DbSet<SalesRep_Shift_Cash> SalesRep_Shift_Cash { get; set; }
        public DbSet<SalesRep_Payment_Method_Bank> SalesRep_Payment_Method_Banks { get; set; }
        public DbSet<SalesRep_Document_Link> SalesRep_Document_Links { get; set; }
        public DbSet<SalesRep_Visit> SalesRep_Visits { get; set; }
        public DbSet<Lookup_SalesRep_NoSaleReason> Lookup_SalesRep_NoSaleReasons { get; set; }
        public DbSet<Lookup_SalesRep_ReturnReason> Lookup_SalesRep_ReturnReasons { get; set; }
        public DbSet<SalesRep_Approval> SalesRep_Approvals { get; set; }
        public DbSet<SalesRep_Attachment> SalesRep_Attachments { get; set; }
        public DbSet<SalesRep_Credit_Policy> SalesRep_Credit_Policies { get; set; }
        public DbSet<SalesRep_Discount_Policy> SalesRep_Discount_Policies { get; set; }
        public DbSet<SalesRep_Route> SalesRep_Routes { get; set; }
        public DbSet<SalesRep_Route_Customer> SalesRep_Route_Customers { get; set; }
        public DbSet<SalesRep_Customer_Geo> SalesRep_Customer_Geos { get; set; }
        public DbSet<SalesRep_GPS_Track> SalesRep_GPS_Tracks { get; set; }
        public DbSet<SalesRep_Sync_Inbox> SalesRep_Sync_Inboxes { get; set; }
        public DbSet<SalesRep_Invoice_Return_Link> SalesRep_Invoice_Return_Links { get; set; }
        public DbSet<SalesRep_Order_Assignment> SalesRep_Order_Assignments { get; set; }
        public DbSet<SalesRep_Order_Assignment_Line> SalesRep_Order_Assignment_Lines { get; set; }
        public DbSet<SalesRep_Order_Delivery> SalesRep_Order_Deliveries { get; set; }
        public DbSet<SalesRep_Target> SalesRep_Targets { get; set; }
        public DbSet<SalesRep_Commission_Plan> SalesRep_Commission_Plans { get; set; }
        public DbSet<SalesRep_Commission_Plan_Tier> SalesRep_Commission_Plan_Tiers { get; set; }
        public DbSet<SalesRep_Commission_Run> SalesRep_Commission_Runs { get; set; }
        public DbSet<SalesRep_Commission_Run_Line> SalesRep_Commission_Run_Lines { get; set; }
        public DbSet<AppointmentPackageService> AppointmentPackageServices { get; set; }
        public DbSet<AppointmentCustomerPackage> AppointmentCustomerPackages { get; set; }

        public DbSet<AppointmentCustomerPackageService> AppointmentCustomerPackageServices { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<jobsList> jobsList { get; set; }
        public DbSet<EmployeeRelatedItem> EmployeeRelatedItems { get; set; }
        public DbSet<Money_Transaction_Box> Money_Transaction_Box { get; set; }
        public DbSet<Money_Transaction_BoxForInvoice> Money_Transaction_BoxForInvoices { get; set; }

        
        public DbSet<Order_Money> Order_Money { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<CarsWorkOrdersItemReversedProcessedDetail> CarsWorkOrdersItemReversedProcessedDetails { get; set; }
        public DbSet<WorkOrderList> WorkOrderList  { get; set; }
        public DbSet<WorkOrderDetail> WorkOrderDetail  { get; set; }
        
        public DbSet<SysConfig> SysConfig { get; set; }
        public DbSet<SysConfigMenus> SysConfigMenus { get; set; }
        //public DbSet<SysConfigs> SysConfigs { get; set; }
        public DbSet<SysConfigMenusPERMATIONS> SysConfigMenuPermission { get; set; }
        public DbSet<USER_DETAILE> USER_DETAILES { get; set; }
        public DbSet<Account_Branches> Account_Branches { get; set; }
        public DbSet<USER_DETAILES_Branches_Permission> USER_DETAILES_Branches_Permission { get; set; }

        
        public DbSet<Lookup_Shift> Lookup_Shifts { get; set; }
        public DbSet<Lookup_InvoiceKitchenStatus> Lookup_InvoiceKitchenStatus { get; set; }
        public DbSet<Lookup_OrderDeliveryTypes> Lookup_OrderDeliveryTypes { get; set; }
        public DbSet<Lookup_Table> Lookup_Tables { get; set; }
        public DbSet<Lookup_DiscountTypes> Lookup_DiscountTypes { get; set; }



         public DbSet<Account_Entry_Master_Balance> Account_Entry_Master_Balance { get; set; }
        //public DbSet<AccountEntryMasterBalanceCostCenter> Account_Entry_Master_Balance_CostCenter { get; set; }
        //public DbSet<AccountEntryMasterBalanceCostCenterTemp> Account_Entry_Master_Balance_CostCenterTemp { get; set; }
        //public DbSet<AccountEntryMasterBalanceMezanTemp> Account_Entry_Master_Balance_MezanTemp { get; set; }
        //public DbSet<AccountEntryMasterBalanceBACK> Account_Entry_Master_BalanceBACK { get; set; }
        public DbSet<Account_Entry_Master_BalanceFirstAccount_Temp> Account_Entry_Master_BalanceFirstAccount_Temp { get; set; }
        //public DbSet<AccountEntryMasterTransaction> Account_Entry_Master_Transaction { get; set; }
        //public DbSet<AccountEntryMasterTransactionCostCenterTemp> Account_Entry_Master_TransactionCostCenterTemp { get; set; }
        //public DbSet<AccountEntryMasterTransactionTemp> Account_Entry_Master_TransactionTemp { get; set; }
        //public DbSet<AccountFirstBalanceFirstTime> Account_FirstBalanceFirstTime { get; set; }
        //public DbSet<AccountTransactionDetails> Account_Transaction_Details { get; set; }
        //public DbSet<AccountType> Account_Type { get; set; }
        //public DbSet<AccountTypeMainAccountId> Account_Type_Main_Account_Id { get; set; }


        public DbSet<Item_Category> item_Category { get; set; }
        public DbSet<Item_First_Period_Balance> Item_First_Period_Balance { get; set; }
        public DbSet<Item_ImagesStore> Item_ImagesStore { get; set; }
        public DbSet<Item_Main_Category> Item_Main_Category { get; set; }
        public DbSet<Item_PriceUnit_List> Item_PriceUnit_List { get; set; }
        public DbSet<Item_Raw_Materials> Item_Raw_Materials { get; set; }
        public DbSet<Item_Raw_MaterialsFor_Entage> Item_Raw_MaterialsFor_Entage { get; set; }
        public DbSet<Item_Raw_MaterialsFor_Entage_Details> Item_Raw_MaterialsFor_Entage_Details { get; set; }
        public DbSet<Item_Raw_MaterialsFor_Entage_List> Item_Raw_MaterialsFor_Entage_List { get; set; }
        public DbSet<ItemCost> ItemCost { get; set; }
        public DbSet<ItemCostAVG> ItemCostAVG { get; set; }
        public DbSet<Price_Type_Sell_Values> Price_Type_Sell_Values { get; set; }
        public DbSet<ItemRawEntagEstlamTemp> ItemRawEntagEstlamTemp { get; set; }

    }

    public class PendingSyncEntry
    {
        public EntityEntry Entry { get; set; } // 👈 التعديل هنا
        public EntityState SavedState { get; set; }
        public string TableName { get; set; }
    }
}
