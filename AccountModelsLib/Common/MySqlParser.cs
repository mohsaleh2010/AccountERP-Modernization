using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace AccountModelsLib
{



    public class MySqlParser
    {
        public static string EnsureIdInInsertCommand(string query, string newIdValue = "11111")
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;

            // Pattern لتفكيك جملة INSERT إلى 3 أجزاء (الوصول لجدول، الأعمدة، والقيم)
            string pattern = @"(?i)^\s*INSERT\s+INTO\s+(?<table_info>[^\(]+)\((?<columns>[^\)]+)\)\s*VALUES\s*\((?<values>.*)\)\s*;?\s*$";
            Match match = Regex.Match(query, pattern, RegexOptions.Singleline);

            if (!match.Success)
            {
                // إذا كانت الصيغة غير مطابقة للشكل القياسي، تُرجع الجملة الأصلية
                return query;
            }

            string tableInfo = match.Groups["table_info"].Value;
            string columnsStr = match.Groups["columns"].Value;
            string valuesStr = match.Groups["values"].Value;

            // تقسيم الأعمدة والقيم لمعاينتها
            string[] columns = columnsStr.Split(',');

            // التحقق مما إذا كان العمود الأول هو Id (مع تجاهل حالة الأحرف)
            string firstColumn = columns[0].Trim().Trim('`', '\'', '"');
            if (firstColumn.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                // العمود الأول بالفعل هو Id - لا داعي لتعديل الجملة
                return query;
            }

            // إعداد قيمة الـ ID الجديدة مع التنسيق المناسب (إذا كانت نصية توضع بين سينجل كوت)
            string formattedIdValue = int.TryParse(newIdValue, out _) ? newIdValue : $"'{newIdValue}'";

            // بناء الجملة الجديدة مع إضافة Id وقيمته في البداية
            StringBuilder updatedQuery = new StringBuilder();
            updatedQuery.Append($"INSERT INTO {tableInfo.Trim()} (Id, {columnsStr.Trim()}) ");
            updatedQuery.Append($"VALUES ({formattedIdValue}, {valuesStr.Trim()})");

            // إضافة الفاصلة المنقوطة إذا كانت موجودة في الاستعلام الأصلي
            if (query.TrimEnd().EndsWith(";"))
            {
                updatedQuery.Append(";");
            }

            return updatedQuery.ToString();
        }
        public static string SmartEnsureIdInInsertCommand(string query, string newIdValue = "xyz", bool isAutoIncrement = false)
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;

            // Regex لالتقاط اسم الجدول والأعمدة والقيم
            string pattern = @"(?i)^\s*INSERT\s+INTO\s+(?<table_info>[^\(]+)\((?<columns>[^\)]+)\)\s*VALUES\s*\((?<values>.*)\)(?<tail>\s*;?\s*)$";
            Match match = Regex.Match(query, pattern, RegexOptions.Singleline);

            if (!match.Success)
                return query;

            string tableInfo = match.Groups["table_info"].Value;
            string columnsStr = match.Groups["columns"].Value;
            string valuesStr = match.Groups["values"].Value;
            string tail = match.Groups["tail"].Value;

            // فحص هل Id هو أول عمود
            int firstCommaIndex = columnsStr.IndexOf(',');
            string firstColumn = (firstCommaIndex != -1 ? columnsStr.Substring(0, firstCommaIndex) : columnsStr)
                                 .Trim().Trim('`', '\'', '"');

            bool hasIdAsFirstCol = firstColumn.Equals("Id", StringComparison.OrdinalIgnoreCase);

            // حالة 1: إذا كان العمود Auto Increment وكان الـ Id مكتوباً، يفضل إزالته من الاستعلام لتوليده تلقائياً
            if (isAutoIncrement)
            {
                // إذا كان Auto Increment، من الأفضل ترك قاعدة البيانات تولده تلقائياً وعدم إقحام قيمة له
                return query;
            }

            // حالة 2: ليس Auto Increment والـ Id غير موجود في البداية -> يتم إضافته
            if (!hasIdAsFirstCol)
            {
                string formattedIdValue = int.TryParse(newIdValue, out _) ? newIdValue : $"'{newIdValue}'";
                return $"INSERT INTO {tableInfo.Trim()} (Id, {columnsStr.Trim()}) VALUES ({formattedIdValue}, {valuesStr.Trim()}){tail}";



            }

            return query;
        }


        public static string SmartEnsureIdInInsertCommand1(string query, string newIdValue = "xyz", bool isAutoIncrement = false)
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;

            // Regex لالتقاط اسم الجدول والأعمدة والقيم
            string pattern = @"(?i)^\s*INSERT\s+INTO\s+(?<table_info>[^\(]+)\((?<columns>[^\)]+)\)\s*VALUES\s*\((?<values>.*)\)(?<tail>\s*;?\s*)$";
            Match match = Regex.Match(query, pattern, RegexOptions.Singleline);

            if (!match.Success)
                return query;

            string tableInfo = match.Groups["table_info"].Value.Trim();
            string columnsStr = match.Groups["columns"].Value.Trim();
            string valuesStr = match.Groups["values"].Value.Trim();
            string tail = match.Groups["tail"].Value;

            // تنظيف اسم الجدول لضمان استخدامه بشكل صحيح مع IDENTITY_INSERT (إزالة الأقواس أو Clean Name)
            string tableName = tableInfo.Trim('`', '"', '[', ']');

            // فحص هل Id موجود في أول عمود
            int firstCommaIndex = columnsStr.IndexOf(',');
            string firstColumn = (firstCommaIndex != -1 ? columnsStr.Substring(0, firstCommaIndex) : columnsStr)
                                 .Trim().Trim('`', '\'', '"', '[', ']');

            bool hasIdAsFirstCol = firstColumn.Equals("Id", StringComparison.OrdinalIgnoreCase);

            string finalSqlQuery = query;

            // إذا لم يكن الـ Id موجوداً في أول عمود، نلصقه في البداية
            if (!hasIdAsFirstCol)
            {
                string formattedIdValue = int.TryParse(newIdValue, out _) ? newIdValue : $"'{newIdValue}'";
                finalSqlQuery = $"INSERT INTO {tableInfo} (Id, {columnsStr}) VALUES ({formattedIdValue}, {valuesStr}){tail}";
            }

            // إذا كان العمود Auto Increment (Identity)، يجب تغليفه بـ SET IDENTITY_INSERT باسم الجدول الديناميكي
            if (isAutoIncrement)
            {
                return $"SET IDENTITY_INSERT {tableName} ON; {finalSqlQuery} SET IDENTITY_INSERT {tableName} OFF;";
            }

            return finalSqlQuery;
        }



        /// <summary>
        /// دالة جديدة لتوليد ودمج استعلام تتبع المزامنة ذكياً (Upsert / Smart Merging) مع استعلام التنفيذ الأصلي في ADO.NET
        /// </summary>
        public static void AppendSyncLogToQueryEF(My_DbContext db, string tableName, long recordId, string recordIdStr, string actionType, string mysqlCmdText)
        {
            // صمام الأمان: إذا كانت المزامنة معطلة أو الـ db غير متوفر، اخرج فوراً
            if (Common.Is_Save_DataDictionaryDbSyncLogs != 1 || db == null)
                return;

            long branchId = Common.Branch_Id;
            long userId = Common.User_Id;

            // البحث الذكي عن سجل معلق (Pending) سابق لنفس الجدول والرقم أو نص الـ Where
            //var existingLog = db.DataDictionaryDbSyncLogs
            //    .FirstOrDefault(d => d.TableName == tableName &&
            //                         d.Status == "Pending" &&
            //                         (d.RecordId == recordId || (d.RecordIdStr == recordIdStr && recordIdStr != null)));
            DataDictionaryDbSyncLogsCLS dataDictionaryDbSyncLogs = new DataDictionaryDbSyncLogsCLS();
            var existingLog = dataDictionaryDbSyncLogs.GetPendingDataDictionaryDbSyncLogs(db, tableName, recordId, recordIdStr);

            if (existingLog != null)
            {
                // دمج الإجراء الجديد في ActionType2 وتحديث أمر الـ SQL الخام والتوقيت
                existingLog.ActionType2 = actionType;
                existingLog.mysqlCmd = mysqlCmdText; // تحديث أمر SQL الخام الأخير
                existingLog.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                // إنشاء سجل مزامنة جديد مع تعبئة أمر SQL الخام
                var newSyncLog = new DataDictionaryDbSyncLogs
                {
                    TableName = tableName,
                    RecordId = recordId,
                    RecordIdStr = recordIdStr,
                    ActionType = actionType,
                    ActionType2 = null,
                    Status = "Pending",
                    IsStatus = 0,
                    CreatedAt = DateTime.UtcNow,
                    Branch_Id = branchId,
                    User_Id = userId,
                    RetryCount = 0,
                    LastError = null,
                    Is_EF_Or_Adonet = 2, //   ADO.NET =2  ,   EF=1
                    payload = null,          // لا يوجد كائن EF مباشر في ADO.NET لتحويله لـ JSON هنا
                    mysqlCmd = mysqlCmdText  // تخزين استعلام SQL الخام هنا بدقة!
                };

                dataDictionaryDbSyncLogs.AddNewOnly(db, newSyncLog);
            }

            db.SaveChanges();
        }



        public static string SmartEnsureIdInInsertCommandNew(My_DbContext db, string query, ref long Identity, bool isAutoIncrement = false)
        {
            if (Common.Use_Auto_Identity_Column == 1)
            {
                return query;
            }

            string newIdValue = CallSnowflakeIdGenerate.GenerateNewId().ToString();

            long.TryParse(newIdValue, out Identity);
            // string result111 = MySqlParser.SmartEnsureIdInInsertCommandNew(cmdd.CommandText, Id.ToString(), isAutoIncrement: false);


            if (string.IsNullOrWhiteSpace(query))
                return query;

            // تجهيز القيمة وتنسيقها (إضافة ' ' إذا كانت نصية)
            string formattedIdValue = long.TryParse(newIdValue, out _) ? newIdValue : $"'{newIdValue}'";

            string tableName = string.Empty;
            string finalSqlQuery = string.Empty;

            // ------------------------------------------------------------------
            // الحالة الأولى: INSERT INTO ... SELECT
            // ------------------------------------------------------------------
            string selectPattern = @"(?i)^\s*INSERT\s+INTO\s+(?<table_info>[^\(]+)\((?<columns>[^\)]+)\)\s*(?<select_clause>SELECT\s+.*)$";
            Match selectMatch = Regex.Match(query, selectPattern, RegexOptions.Singleline);

            if (selectMatch.Success)
            {
                string tableInfo = selectMatch.Groups["table_info"].Value.Trim();
                string columnsStr = selectMatch.Groups["columns"].Value.Trim();
                string selectClause = selectMatch.Groups["select_clause"].Value.Trim();

                tableName = CleanIdentifier(tableInfo);

                // فحص هل Id هو أول عمود
                if (!HasIdAsFirstColumn(columnsStr))
                {
                    // إقحام Id في الأعمدة، وإقحام نفس القيمة بعد كلمة SELECT فى بداية *كل* فرع من فروع
                    // UNION/UNION ALL/INTERSECT/EXCEPT - وليس الفرع الأول فقط كما كان سابقاً. الإقحام فى
                    // الفرع الأول فقط كان يجعل عدد أعمدة ذلك الفرع أكبر بواحد من باقى الفروع، فيفشل SQL Server
                    // بخطأ "All queries combined using a UNION, INTERSECT or EXCEPT operator must have an
                    // equal number of expressions in their target lists" لأى استعلام INSERT...SELECT...UNION
                    // SELECT... (مثال حقيقى: CustomersAccountCLS.GetCustomers_Accounts_Mokhtalat).
                    string newColumns = $"Id, {columnsStr}";
                    string newSelectClause = InjectValueIntoEveryUnionBranch(selectClause, formattedIdValue);

                    finalSqlQuery = $"INSERT INTO {tableInfo} ({newColumns}) {newSelectClause}";
                }
                else
                {
                    finalSqlQuery = query;
                }
            }
            // ------------------------------------------------------------------
            // الحالة الثانية: INSERT INTO ... VALUES
            // ------------------------------------------------------------------
            else
            {
                string valuesPattern = @"(?i)^\s*INSERT\s+INTO\s+(?<table_info>[^\(]+)\((?<columns>[^\)]+)\)\s*VALUES\s*\((?<values>.*)\)(?<tail>\s*;?\s*)$";
                Match valuesMatch = Regex.Match(query, valuesPattern, RegexOptions.Singleline);

                if (!valuesMatch.Success)
                    return query; // إذا كانت صيغة غير معروفة نرجع الاستعلام كما هو

                string tableInfo = valuesMatch.Groups["table_info"].Value.Trim();
                string columnsStr = valuesMatch.Groups["columns"].Value.Trim();
                string valuesStr = valuesMatch.Groups["values"].Value.Trim();
                string tail = valuesMatch.Groups["tail"].Value;

                tableName = CleanIdentifier(tableInfo);

                if (!HasIdAsFirstColumn(columnsStr))
                {
                    finalSqlQuery = $"INSERT INTO {tableInfo} (Id, {columnsStr}) VALUES ({formattedIdValue}, {valuesStr}){tail}";
                }
                else
                {
                    finalSqlQuery = query;
                }
            }
            // ==================================================================
            // *الخطوة المضافة:* تفعيل وتتبع المزامنة لعمليات الـ INSERT عبر ADO.NET
            // ==================================================================
            if (!string.IsNullOrEmpty(tableName) && Identity > 0)
            {

                // finalSqlQuery = AppendSyncLogToQuery(tableName, Identity, "INSERT", finalSqlQuery);
                // AppendSyncLogToQueryEF(db, tableName, Identity, "INSERT", finalSqlQuery);
                AppendSyncLogToQueryEF(db, tableName, Identity, null, "INSERT", finalSqlQuery);

            }
            // ------------------------------------------------------------------
            // تغليف الاستعلام بـ SET IDENTITY_INSERT إذا كان Auto Increment
            // ------------------------------------------------------------------
            if (isAutoIncrement && !string.IsNullOrEmpty(tableName))
            {
                return $"SET IDENTITY_INSERT {tableName} ON; {finalSqlQuery} SET IDENTITY_INSERT {tableName} OFF;";
            }

            return finalSqlQuery;
        }


        /// <summary>
        /// Router Method لتوجيه استعلامات ADO.NET بناءً على نوع العملية
        /// </summary>
        public static string RouteAndProcessCommand(ref My_DbContext db, string query, ref long Identity, bool isAutoIncrement = false)
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;

            string trimmedQuery = query.TrimStart();

            // التوجيه لعمليات الإدخال
            if (trimmedQuery.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
            {
                return SmartEnsureIdInInsertCommandNew(db, query, ref Identity, isAutoIncrement);
            }
            // التوجيه لعمليات التعديل والحذف
            else if (trimmedQuery.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                     trimmedQuery.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessUpdateDeleteQuery(ref db, query);
            }

            return query;
        }

        /// <summary>
        /// معالجة وتتبع استعلامات الـ UPDATE و DELETE واستخراج شروط الـ WHERE كنص
        /// </summary>
        /// <summary>
        /// معالجة استعلامات UPDATE و DELETE واستخدام نفس دالة AppendSyncLogToQuery للمزامنة
        /// </summary>
        public static string ProcessUpdateDeleteQuery(ref My_DbContext db, string query)
        {
            if (Common.Is_Save_DataDictionaryDbSyncLogs != 1 || string.IsNullOrWhiteSpace(query))
                return query;

            string tableName = string.Empty;
            string actionType = string.Empty;
            string whereClauseText = string.Empty;
            long recordId = 0;

            // 1. التحقق من استعلام UPDATE
            string updatePattern = @"(?i)^\s*UPDATE\s+(?<table_info>[^\s]+)\s+SET\s+(?<set_clause>.*?)(?:\s+WHERE\s+(?<where_clause>.*))?(\s*;?\s*)$";
            Match updateMatch = Regex.Match(query, updatePattern, RegexOptions.Singleline);

            if (updateMatch.Success)
            {
                tableName = CleanIdentifier(updateMatch.Groups["table_info"].Value.Trim());
                actionType = "UPDATE";
                whereClauseText = updateMatch.Groups["where_clause"].Value.Trim();
                recordId = ExtractFirstNumericIdFromWhere(whereClauseText);
            }
            else
            {
                // 2. التحقق من استعلام DELETE
                string deletePattern = @"(?i)^\s*DELETE\s+(?:FROM\s+)?(?<table_info>[^\s]+)(?:\s+WHERE\s+(?<where_clause>.*))?(\s*;?\s*)$";
                Match deleteMatch = Regex.Match(query, deletePattern, RegexOptions.Singleline);

                if (deleteMatch.Success)
                {
                    tableName = CleanIdentifier(deleteMatch.Groups["table_info"].Value.Trim());
                    actionType = "DELETE";
                    whereClauseText = deleteMatch.Groups["where_clause"].Value.Trim();
                    recordId = ExtractFirstNumericIdFromWhere(whereClauseText);
                }
            }

            // إذا توفر الجدول ونوع العملية، نقوم بتسجيل التتبع عبر EF
            if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(actionType))
            {
                // التصحيح هنا: تمرير البارامترات بالترتيب الصحيح (db, tableName, recordId, recordIdStr, actionType)
                AppendSyncLogToQueryEF(db, tableName, recordId, whereClauseText, actionType, query);
            }

            // إرجاع الاستعلام الأصلي لتنفيذه عبر ADO.NET
            return query;
        }

        /// <summary>
        /// استخراج رقم الـ ID الرقمي من شرط الـ WHERE لتعبئة الـ RecordId الإجباري
        /// </summary>
        private static long ExtractFirstNumericIdFromWhere(string whereClause)
        {
            if (string.IsNullOrEmpty(whereClause)) return 0;
            // ابحث عن مطابقة مثل Id = 123 أو id = 123 أو ID = 123
            Match match = Regex.Match(whereClause, @"(?i)(?:\bId\b\s*=\s*)(?<num>\d+)");
            if (match.Success && long.TryParse(match.Groups["num"].Value, out long val))
            {
                return val;
            }
            return 0;
        }
        // يقسّم عبارة الـ SELECT عند كل UNION/UNION ALL/INTERSECT/EXCEPT على المستوى الأعلى (النمط البسيط
        // لاستعلامات هذا المشروع لا يستخدم هذه الكلمات داخل استعلامات فرعية متداخلة)، ثم يُقحم نفس قيمة الـ Id
        // بعد كلمة SELECT فى بداية كل فرع على حدة، لا الفرع الأول فقط.
        private static readonly Regex UnionBranchSeparator = new Regex(@"(?i)\b(UNION\s+ALL|UNION|INTERSECT|EXCEPT)\b", RegexOptions.Compiled);

        private static string InjectValueIntoEveryUnionBranch(string selectClause, string formattedValue)
        {
            string[] parts = UnionBranchSeparator.Split(selectClause);
            for (int i = 0; i < parts.Length; i += 2)
            {
                parts[i] = Regex.Replace(parts[i], @"(?i)^\s*SELECT\s+", $"SELECT {formattedValue}, ");
            }
            return string.Join(" ", parts);
        }

        private static bool HasIdAsFirstColumn(string columnsStr)
        {
            int firstCommaIndex = columnsStr.IndexOf(',');
            string firstColumn = (firstCommaIndex != -1 ? columnsStr.Substring(0, firstCommaIndex) : columnsStr);
            firstColumn = CleanIdentifier(firstColumn);

            return firstColumn.Equals("Id", StringComparison.OrdinalIgnoreCase);
        }

        private static string CleanIdentifier(string input)
        {
            return input.Trim().Trim('`', '\'', '"', '[', ']');
        }

    }
}