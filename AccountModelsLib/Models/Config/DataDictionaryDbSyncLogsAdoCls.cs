using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient; // أو System.Data.SqlClient

namespace AccountModelsLib
{
    public class DataDictionaryDbSyncLogsAdoCls
    {
        /// <summary>
        /// كائن يحمل البيانات المستخرجة من الاستعلام
        /// </summary>
        public class ParsedSqlInfo
        {
            public string ActionType { get; set; }     // INSERT, UPDATE, DELETE
            public string TableName { get; set; }      // اسم الجدول
            public string RecordIdValue { get; set; }  // القيمة النصية لـ Id أو شرط WHERE الكامل
            public long NumericRecordId { get; set; }  // القيمة الرقمية لـ Id (إن وجد)
        }

        // =========================================================================
        // 1. الدالة المخصصة لاستخراج اسم الجدول والـ RecordId و شرط WHERE من الاستعلام
        // =========================================================================
        public ParsedSqlInfo ExtractSqlInfo(SqlCommand cmd)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.CommandText))
                return null;

            string sql = cmd.CommandText.Trim();
            var info = new ParsedSqlInfo();

            // أ. تحديد نوع العملية (Action)
            if (Regex.IsMatch(sql, @"^\s*INSERT", RegexOptions.IgnoreCase))
                info.ActionType = "INSERT";
            else if (Regex.IsMatch(sql, @"^\s*UPDATE", RegexOptions.IgnoreCase))
                info.ActionType = "UPDATE";
            else if (Regex.IsMatch(sql, @"^\s*DELETE", RegexOptions.IgnoreCase))
                info.ActionType = "DELETE";
            else
                info.ActionType = "UNKNOWN";

            // ب. استخراج اسم الجدول (TableName)
            info.TableName = ExtractTableName(sql, info.ActionType);

            // ج. استخراج الـ RecordId أو شرط WHERE
            ExtractRecordIdOrWhere(sql, cmd.Parameters, info);

            return info;
        }

        private string ExtractTableName(string sql, string actionType)
        {
            string pattern = actionType switch
            {
                "INSERT" => @"INSERT\s+INTO\s+([^\s\(\[\`""]+)",
                "UPDATE" => @"UPDATE\s+([^\s\(\[\`""]+)",
                "DELETE" => @"DELETE\s+(?:FROM\s+)?([^\s\(\[\`""]+)",
                _ => null
            };

            if (pattern == null) return "UNKNOWN";

            var match = Regex.Match(sql, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string rawTable = match.Groups[1].Value.Replace("[", "").Replace("]", "");
                if (rawTable.Contains("."))
                {
                    rawTable = rawTable.Split('.').Last(); // استخراج اسم الجدول بدون dbo.
                }
                return rawTable;
            }

            return "UNKNOWN";
        }

        private void ExtractRecordIdOrWhere(string sql, SqlParameterCollection parameters, ParsedSqlInfo info)
        {
            if (info.ActionType == "INSERT")
            {
                // استخراج الأعمدة والقيم من أمر الـ INSERT
                var matchCols = Regex.Match(sql, @"\(([^)]+)\)\s*VALUES\s*\(([^)]+)\)", RegexOptions.IgnoreCase);
                if (matchCols.Success)
                {
                    var cols = matchCols.Groups[1].Value.Split(',').Select(c => c.Trim().Replace("[", "").Replace("]", "")).ToList();
                    var vals = matchCols.Groups[2].Value.Split(',').Select(v => v.Trim()).ToList();

                    int idIndex = cols.FindIndex(c => c.Equals("Id", StringComparison.OrdinalIgnoreCase));
                    if (idIndex >= 0 && idIndex < vals.Count)
                    {
                        info.RecordIdValue = ResolveParameterOrValue(vals[idIndex], parameters);
                    }
                }
            }
            else if (info.ActionType == "UPDATE" || info.ActionType == "DELETE")
            {
                // البحث عن شرط WHERE
                var whereMatch = Regex.Match(sql, @"WHERE\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (whereMatch.Success)
                {
                    string whereClause = whereMatch.Groups[1].Value.Trim().TrimEnd(';');

                    // فحص هل الشرط يحتوي على Id فقط (مثل: WHERE Id = @Id أو WHERE [Id] = 123)
                    var singleIdMatch = Regex.Match(whereClause, @"^\s*\[?Id\]?\s*=\s*([^\s;]+)\s*$", RegexOptions.IgnoreCase);

                    if (singleIdMatch.Success)
                    {
                        info.RecordIdValue = ResolveParameterOrValue(singleIdMatch.Groups[1].Value, parameters);
                    }
                    else
                    {
                        // يحتوي على عدة أعمدة أو لا يوجد Id -> استخراج شرط WHERE بالكامل وتعويض قيم البارامترات فيه
                        info.RecordIdValue = ReplaceParametersInWhere(whereClause, parameters);
                    }
                }
            }

            // تحويل القيمة إلى long إذا كانت رقماً صريحاً لاستخدامه في العمود الرقمي
            if (long.TryParse(info.RecordIdValue, out long numericId))
            {
                info.NumericRecordId = numericId;
            }
        }

        private string ResolveParameterOrValue(string rawVal, SqlParameterCollection parameters)
        {
            if (rawVal.StartsWith("@") && parameters != null && parameters.Contains(rawVal))
            {
                var paramVal = parameters[rawVal].Value;
                return paramVal != null && paramVal != DBNull.Value ? paramVal.ToString() : null;
            }
            return rawVal.Trim('\'', '"', ' ');
        }

        private string ReplaceParametersInWhere(string whereClause, SqlParameterCollection parameters)
        {
            if (parameters == null || parameters.Count == 0) return whereClause;

            string result = whereClause;
            foreach (SqlParameter p in parameters)
            {
                string pValue = (p.Value == DBNull.Value || p.Value == null) ? "NULL" : $"'{p.Value}'";
                if (p.DbType == DbType.Int32 || p.DbType == DbType.Int64 || p.DbType == DbType.Decimal || p.DbType == DbType.Double)
                {
                    pValue = p.Value?.ToString() ?? "NULL";
                }

                result = Regex.Replace(result, Regex.Escape(p.ParameterName) + @"\b", pValue, RegexOptions.IgnoreCase);
            }
            return result;
        }

        // =========================================================================
        // 2. الدالة الجديدة المستهدفة: تستقبل الـ SqlCommand مباشرة
        // =========================================================================
        public void CreateSyncLog(SqlCommand command, SqlConnection connection, SqlTransaction transaction = null)
        {
            try
            {
                if (Common.Is_Save_DataDictionaryDbSyncLogs != 1) return;

                // أ. استخراج البيانات تلقائياً من الـ SqlCommand
                var parsedInfo = ExtractSqlInfo(command);

                if (parsedInfo == null || string.IsNullOrEmpty(parsedInfo.TableName) || string.IsNullOrEmpty(parsedInfo.RecordIdValue))
                    return;

                string tableName = parsedInfo.TableName;
                string action = parsedInfo.ActionType;
                long recordId = parsedInfo.NumericRecordId; // القيمة الرقمية للـ Id
                string whereClauseOrStringId = parsedInfo.RecordIdValue; // النص (سواء كان Id أو شرط WHERE كامل)

                // ب. البحث عن سجل معلق (Pending) سابق لنفس الجدول والرقم/الشرط
                string selectSql = @"
                SELECT TOP 1 Id 
                FROM DataDictionaryDbSyncLogs 
                WHERE TableName = @TableName 
                  AND (RecordId = @RecordId OR RecordIdStr = @RecordIdStr)
                  AND Status = 'Pending';";

                long existingLogId = 0;

                using (SqlCommand checkCmd = new SqlCommand(selectSql, connection, transaction))
                {
                    checkCmd.Parameters.AddWithValue("@TableName", tableName);
                    checkCmd.Parameters.AddWithValue("@RecordId", recordId);
                    checkCmd.Parameters.AddWithValue("@RecordIdStr", whereClauseOrStringId);

                    object result = checkCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        existingLogId = Convert.ToInt64(result);
                    }
                }

                DateTime utcNow = DateTime.UtcNow;

                if (existingLogId > 0)
                {
                    // ج. حالة العثور على السجل: تحديث ActionType2
                    string updateSql = @"
                    UPDATE DataDictionaryDbSyncLogs 
                    SET ActionType2 = @ActionType2, 
                        CreatedAt = @CreatedAt 
                    WHERE Id = @Id;";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, connection, transaction))
                    {
                        updateCmd.Parameters.AddWithValue("@ActionType2", action);
                        updateCmd.Parameters.AddWithValue("@CreatedAt", utcNow);
                        updateCmd.Parameters.AddWithValue("@Id", existingLogId);

                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // د. إضافة سجل جديد مع توليد Snowflake ID
                    long newLogId = CallSnowflakeIdGenerate.GenerateNewId();

                    string insertSql = @"
                    INSERT INTO DataDictionaryDbSyncLogs 
                    (Id, TableName, RecordId, RecordIdStr, ActionType, ActionType2, Status, CreatedAt) 
                    VALUES 
                    (@Id, @TableName, @RecordId, @RecordIdStr, @ActionType, NULL, 'Pending', @CreatedAt);";

                    using (SqlCommand insertCmd = new SqlCommand(insertSql, connection, transaction))
                    {
                        insertCmd.Parameters.AddWithValue("@Id", newLogId);
                        insertCmd.Parameters.AddWithValue("@TableName", tableName);
                        insertCmd.Parameters.AddWithValue("@RecordId", recordId);
                        insertCmd.Parameters.AddWithValue("@RecordIdStr", whereClauseOrStringId);
                        insertCmd.Parameters.AddWithValue("@ActionType", action);
                        insertCmd.Parameters.AddWithValue("@CreatedAt", utcNow);

                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ee)
            {

                
            }
        }
    }
}