// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
 
using System.Linq;

namespace AccountModelsLib
{
 
    public class DataDictionaryDbSyncLogsEFCls
    {
        DataDictionaryDbSyncLogs dlogs = new DataDictionaryDbSyncLogs();
        public DataDictionaryDbSyncLogsEFCls()
        {
            
        }

        public long  AddDataDictionaryDbSyncLogs(ref int Excp, ref string Msg, ref operations opp, DataDictionaryDbSyncLogs dlogs)
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
                //var idProperty = entry.Property("Id");

                //// إذا كان الـ Id لم يُحدد بعد (يساوي 0)
                //if (idProperty != null && Convert.ToInt64(idProperty.CurrentValue) == 0)
                //{
                //    long newSnowflakeId = CallSnowflakeIdGenerate.GenerateNewId(); // توليد الرقم
                //    idProperty.CurrentValue = newSnowflakeId;               // تعيين القيمة
                //}
                var idProperty = entry.Property("Id");

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