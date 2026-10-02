// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;

namespace AccountModelsLib
{
    public class DataDictionaryDbSyncLogsEFCls
    {
        public DataDictionaryDbSyncLogsEFCls()
        {
        }

        public long AddDataDictionaryDbSyncLogs(ref int Excp, ref string Msg, ref operations opp, DataDictionaryDbSyncLogs dlogs)
        {
            try
            {
                opp.db.DataDictionaryDbSyncLogs.Add(dlogs);
                opp.db.SaveChanges();
                Excp = 0;
                return dlogs.Id;
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
                Msg = ee.Message;
                Excp = 1;
                return 0;
            }
        }

        /// <summary>
        /// التقاط التغييرات الحالية من الـ ChangeTracker
        /// </summary>
        public List<PendingSyncEntry> GetPendingEntries(My_DbContext db)
        {
            var pendingSyncEntries = new List<PendingSyncEntry>();

            if (Common.Is_Save_DataDictionaryDbSyncLogs == 1)
            {
                pendingSyncEntries = db.ChangeTracker.Entries()
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

        /// <summary>
        /// المنطق الذكي الموحد: البحث عن السجلات المعلقة القديمة ودمج التعديل الحديث في ActionType2 أو إضافة سجل جديد
        /// </summary>
        
        public void CreateSyncLogs(  List<PendingSyncEntry> pendingEntries, My_DbContext db)
        {
            if (Common.Is_Save_DataDictionaryDbSyncLogs == 1 && pendingEntries != null)
            {
                long branchId = Common.Branch_Id; // جلب فرع العمل الحالي
                long userId = Common.User_Id; // جلب المستخدم الحالي

                foreach (var item in pendingEntries)
                {
                    var fallbackProp = item.Entry.Metadata.FindProperty("Id")
                                    ?? item.Entry.Metadata.FindProperty("ID")
                                    ?? item.Entry.Metadata.FindProperty("id");

                    if (fallbackProp == null) continue;

                    var idProperty = item.Entry.Property(fallbackProp.Name);
                    if (idProperty == null || idProperty.CurrentValue == null) continue;

                    long recordId = Convert.ToInt64(idProperty.CurrentValue);
                    if (recordId <= 0) continue;

                    string action = item.SavedState switch
                    {
                        EntityState.Added => "INSERT",
                        EntityState.Modified => "UPDATE",
                        EntityState.Deleted => "DELETE",
                        _ => "UNKNOWN"
                    };

                    if (action == "UNKNOWN") continue;

                    // استخراج بيانات الكيان كـ JSON لملء حقل payload
                    string payloadJson = null;
                    try
                    {
                        var entityObj = item.Entry.CurrentValues.ToObject();
                        payloadJson = JsonSerializer.Serialize(entityObj);
                    }
                    catch { }
                    // البحث الذكي عن سجل معلق (Pending) سابق لنفس الجدول ورقم السجل
                    //var existingLog = db.DataDictionaryDbSyncLogs.FirstOrDefault(x =>
                    //    x.TableName == item.TableName &&
                    //    x.RecordId == recordId &&
                    //    x.Status == "Pending");

                    DataDictionaryDbSyncLogsCLS dataDictionaryDbSyncLogs = new DataDictionaryDbSyncLogsCLS();
                    var existingLog = dataDictionaryDbSyncLogs.GetPendingDataDictionaryDbSyncLogs(db, item.TableName, recordId);
                    if (existingLog != null)
                    {
                     

                        // دمج التغيير الجديد في ActionType2 وتحديث التوقيت وتحديث الـ payload إن أردت
                        existingLog.ActionType2 = action;
                        existingLog.payload = payloadJson; // تحديث الـ payload بآخر التعديلات
                        existingLog.CreatedAt = DateTime.UtcNow;

                    }
                    else
                    {
                        // إنشاء سجل مزامنة جديد مطابق تماماً لهيكل الجدول
                        var newSyncLog = new DataDictionaryDbSyncLogs
                        {
                            TableName = item.TableName,
                            RecordId = recordId,
                            RecordIdStr = null, // في EF التعامل رقمي مباشر عبر الـ Id
                            ActionType = action,
                            ActionType2 = null,
                            Status = "Pending",
                            IsStatus = 0,
                            CreatedAt = DateTime.UtcNow,
                            Branch_Id = branchId,
                            User_Id = userId,
                            RetryCount = 0,
                            LastError = null,
                            Is_EF_Or_Adonet = 1, //   ADO.NET =2  ,   EF=1
                            payload = payloadJson, // تعبئة بيانات الكيان كـ JSON هنا
                            mysqlCmd = null        // فارغة لأن التنفيذ تم عبر EF Core وليس SQL خام
                        };
                        
                        dataDictionaryDbSyncLogs.AddNewOnly(db, newSyncLog);
                        //db.DataDictionaryDbSyncLogs.Add(newSyncLog);
                    }
                }
            }
        }
        /// <summary>
        /// توليد وإسناد Snowflake ID للكائنات الجديدة مع الفحص الاحتياطي للـ Primary Key
        /// </summary>
        public void AssignSnowflakeIdsToNewEntities(My_DbContext db)
        {
            var addedEntries = db.ChangeTracker.Entries()
                .Where(e => !(e.Entity is DataDictionaryDbSyncLogs) && e.State == EntityState.Added)
                .ToList();

            foreach (var entry in addedEntries)
            {
                var fallbackProp = entry.Metadata.FindProperty("Id")
                                ?? entry.Metadata.FindProperty("ID")
                                ?? entry.Metadata.FindProperty("id");

                if (fallbackProp != null && (fallbackProp.ClrType == typeof(long) || fallbackProp.ClrType == typeof(long?)))
                {
                    var idProperty = entry.Property(fallbackProp.Name);

                    if (idProperty != null)
                    {
                        long currentValue = 0;
                        if (idProperty.CurrentValue != null)
                        {
                            long.TryParse(idProperty.CurrentValue.ToString(), out currentValue);
                        }

                        // إذا كان الـ Id يساوي 0 أو يحمل قيمة سالبة مؤقتة من EF Core
                        if (currentValue <= 0)
                        {
                            long newSnowflakeId = CallSnowflakeIdGenerate.GenerateNewId();
                            idProperty.CurrentValue = newSnowflakeId;
                        }
                    }
                }
            }
        }
    }
    /*
    public class DataDictionaryDbSyncLogsEFCls
    {
        DataDictionaryDbSyncLogs dlogs = new DataDictionaryDbSyncLogs();
        public DataDictionaryDbSyncLogsEFCls()
        {

        }

        public long AddDataDictionaryDbSyncLogs(ref int Excp, ref string Msg, ref operations opp, DataDictionaryDbSyncLogs dlogs)
        {
            try
            {
                long DataDictionaryDbSyncLogs = 0;
                opp.db.DataDictionaryDbSyncLogs.Add(dlogs);
                opp.db.SaveChanges();
                Excp = 0;
                return dlogs.Id;
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
                Msg = ee.Message;
                Excp = 1;
                return 0;
            }
        }
        public List<PendingSyncEntry> GetPendingEntries(My_DbContext db)
        {
            List<PendingSyncEntry> pendingSyncEntries = new List<PendingSyncEntry>();
            if (Common.Is_Save_DataDictionaryDbSyncLogs == 1)
            {
                pendingSyncEntries =
                 db.ChangeTracker.Entries()
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
        public void CreateSyncLogs(List<PendingSyncEntry> pendingEntries, My_DbContext db)
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
                    var existingLog = db.DataDictionaryDbSyncLogs.FirstOrDefault(x =>
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

                        db.DataDictionaryDbSyncLogs.Add(newSyncLog);
                    }
                }
            }
        }
        public void GenerateSyncLogs(My_DbContext db)
        {
            if (Common.Is_Save_DataDictionaryDbSyncLogs == 1)
            {
                // 2. استبعاد DataDictionaryDbSyncLogs لمنع التكرار اللانهائي
                var modifiedEntries = db.ChangeTracker.Entries()
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
                    db.DataDictionaryDbSyncLogs.AddRange(syncLogs);
                }
            }
        }
        public void AssignSnowflakeIdsToNewEntities(My_DbContext db)
        {
            var addedEntries = db.ChangeTracker.Entries()
        .Where(e => !(e.Entity is DataDictionaryDbSyncLogs) && e.State == EntityState.Added)
        .ToList();

            foreach (var entry in addedEntries)
            {
                // var idProperty = entry.Property("Id")?? entry.Property("id")?? entry.Property("ID");
                // فحص احتياطي في حال كان الحقل ليس PK ولكن اسمه Id أو ID أو id بدون رمي Exception
                var fallbackProp = entry.Metadata.FindProperty("Id")
                                ?? entry.Metadata.FindProperty("ID")
                                ?? entry.Metadata.FindProperty("id");

                if (fallbackProp != null && (fallbackProp.ClrType == typeof(long) || fallbackProp.ClrType == typeof(long?)))
                {
                    var idProperty = entry.Property(fallbackProp.Name);

                    if (idProperty != null)
                    {
                        long currentValue = 0;
                        if (idProperty.CurrentValue != null)
                        {
                            long.TryParse(idProperty.CurrentValue.ToString(), out currentValue);
                        }

                        // إذا كان الـ Id يساوي 0 أو يحمل قيمة سالبة مؤقتة من EF Core
                        if (currentValue <= 0)
                        {
                            long newSnowflakeId = CallSnowflakeIdGenerate.GenerateNewId(); // توليد الرقم
                            idProperty.CurrentValue = newSnowflakeId;               // تعيين القيمة
                        }
                    }
                }
            }
        }
    }
    */
}
