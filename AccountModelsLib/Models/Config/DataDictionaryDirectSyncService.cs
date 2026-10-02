// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{

    using System;
    using System.Collections.Generic;
    using System.Data;
   
    using System.Linq;
    using Microsoft.Data.SqlClient;
    public class DataDictionaryDirectSyncService
    {
        /// <summary>
        /// الدالة الرئيسية لبدء المزامنة المباشرة بين قاعدة البيانات المحلية والمركزية
        /// </summary>
        public static void SyncPendingLogsDirectly(My_DbContext localDb, string mainServerConnectionString)
        {
            // صمام الأمان: إذا كانت المزامنة معطلة، اخرج فوراً
            if (Common.Is_Save_DataDictionaryDbSyncLogs != 1)
                return;

            try
            {
                // 1. جلب السجلات المعلقة (Pending) التي لم تتجاوز 5 محاولات فاشلة
                var pendingLogs = localDb.DataDictionaryDbSyncLogs
                    .Where(x => x.Status == "Pending" && x.RetryCount < 5)
                    .OrderBy(x => x.CreatedAt)
                    .Take(50) // معالجة على دفعات (Batching)
                    .ToList();

                if (!pendingLogs.Any())
                    return;

                // فتح اتصال بقاعدة البيانات المحلية لجلب البيانات الحية
                //string localConnectionString = localDb.Database.GetDbConnection().ConnectionString;
                string localConnectionString =  operations.conn_str_;

                using (var mainConn = new SqlConnection(mainServerConnectionString))
                {
                    mainConn.Open();

                    foreach (var log in pendingLogs)
                    {
                        try
                        {
                            // تحديد الإجراء النهائي (الأخذ بعين الاعتبار ActionType2 إن وجد)
                            string effectiveAction = !string.IsNullOrEmpty(log.ActionType2) ? log.ActionType2 : log.ActionType;
                            Dictionary<string, object> rowData = null;

                            // 2. إذا لم يكن الإجراء DELETE، نقوم بجلب البيانات الحية مباشرة من الجدول المحلي
                            using (var localConn = new SqlConnection(localConnectionString))
                            {
                                if (!effectiveAction.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                                {
                                    rowData = GetLiveRecordDataFromTable(localConn, log.TableName, log.RecordId, log.RecordIdStr);

                                    if (rowData == null || rowData.Count == 0)
                                    {
                                        log.RetryCount++;
                                        log.LastError = "Record not found in the local table.";
                                        continue;
                                    }
                                }

                                // 3. تطبيق التغيير مباشرة على السيرفر الرئيسي
                                bool isExecuted = ExecuteCommandOnMainServer(mainConn, log.TableName, effectiveAction, log.RecordId, log.RecordIdStr, rowData);

                                if (isExecuted)
                                {
                                    // الخيار الأول: تحديث الحالة إلى Completed وتصفية الخطأ
                                    log.Status = "Completed";
                                    log.IsStatus = 1;
                                    log.LastError = null;

                                    // الخيار الثاني (بديل): إذا كنت تريد حذف السجل نهائياً من جدول المزامنة بعد النجاح
                                    // localDb.DataDictionaryDbSyncLogs.Remove(log);
                                }
                                else
                                {
                                    log.RetryCount++;
                                    log.LastError = "Failed to execute command on main server.";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.RetryCount++;
                            log.LastError = ex.Message;
                            operations.DataDictionaryLogerException(ex);
                        }
                    }

                    // حفظ التحديثات لحالة السجلات محلياً
                    localDb.SaveChanges();
                }
            }
            catch (Exception ee)
            {
                operations.DataDictionaryLogerException(ee);
            }
        }

        /// <summary>
        /// جلب بيانات السجل مباشرة من الجدول المحلي بشكل ديناميكي
        /// </summary>
        public static Dictionary<string, object> GetLiveRecordDataFromTable(SqlConnection conn, string tableName, long recordId, string recordIdStr)
        {
            var rowData = new Dictionary<string, object>();
            string query = string.Empty;

            if (recordId > 0)
            {
                query = $"SELECT * FROM [{tableName}] WHERE Id = {recordId} OR ID = {recordId} OR id = {recordId}";
            }
            else if (!string.IsNullOrEmpty(recordIdStr))
            {
                query = $"SELECT * FROM [{tableName}] WHERE {recordIdStr}";
            }
            else
            {
                return null;
            }

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.GetValue(i);
                                rowData[columnName] = value == DBNull.Value ? null : value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                operations.DataDictionaryLogerException(ex);
                return null;
            }

            return rowData;
        }

        /// <summary>
        /// تنفيذ أمر (INSERT / UPDATE / DELETE) مباشرة على قاعدة بيانات السيرفر الرئيسي
        /// </summary>
        private static bool ExecuteCommandOnMainServer(SqlConnection mainConn, string tableName, string action, long recordId, string recordIdStr, Dictionary<string, object> rowData)
        {
            string sql = string.Empty;

            try
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = mainConn;

                    if (action.Equals("INSERT", StringComparison.OrdinalIgnoreCase) && rowData != null)
                    {
                        // بناء استعلام INSERT ديناميكي
                        var columns = rowData.Keys.Select(k => $"[{k}]").ToList();
                        var parameters = rowData.Keys.Select(k => $"@{k}").ToList();

                        sql = $"INSERT INTO [{tableName}] ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)})";

                        foreach (var kvp in rowData)
                        {
                            cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }
                    }
                    else if (action.Equals("UPDATE", StringComparison.OrdinalIgnoreCase) && rowData != null)
                    {
                        // بناء استعلام UPDATE ديناميكي (تحديث كافة الأعمدة ما عدا الـ Id الأساسي)
                        var setClauses = rowData.Keys
                            .Where(k => !k.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                                        !k.Equals("ID", StringComparison.OrdinalIgnoreCase) &&
                                        !k.Equals("id", StringComparison.OrdinalIgnoreCase))
                            .Select(k => $"[{k}] = @{k}")
                            .ToList();

                        string whereClause = recordId > 0 ? "[Id] = @MainRecordId OR [ID] = @MainRecordId" : recordIdStr;

                        sql = $"UPDATE [{tableName}] SET {string.Join(", ", setClauses)} WHERE {whereClause}";

                        foreach (var kvp in rowData)
                        {
                            cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }

                        if (recordId > 0)
                        {
                            cmd.Parameters.AddWithValue("@MainRecordId", recordId);
                        }
                    }
                    else if (action.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                    {
                        // بناء استعلام DELETE
                        string whereClause = recordId > 0 ? "[Id] = @MainRecordId OR [ID] = @MainRecordId" : recordIdStr;
                        sql = $"DELETE FROM [{tableName}] WHERE {whereClause}";

                        if (recordId > 0)
                        {
                            cmd.Parameters.AddWithValue("@MainRecordId", recordId);
                        }
                    }

                    if (!string.IsNullOrEmpty(sql))
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // في حالة كان التحديث فشل لأن السجل غير موجود على السيرفر الرئيسي، يمكنك هنا محاولة تحويله لـ INSERT تلقائياً (Upsert)
                operations.DataDictionaryLogerException(ex);
                return false;
            }

            return false;
        }
    }
}