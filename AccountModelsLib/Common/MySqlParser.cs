using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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





        public static string SmartEnsureIdInInsertCommandNew(string query, ref long Identity, bool isAutoIncrement = false)
        {
            if(Common.Use_Auto_Identity_Column==1)
            {
                return query;
            }

            string newIdValue =   CallSnowflakeIdGenerate.GenerateNewId().ToString();

            long.TryParse(newIdValue, out Identity);
           // string result111 = MySqlParser.SmartEnsureIdInInsertCommandNew(cmdd.CommandText, Id.ToString(), isAutoIncrement: false);


            if (string.IsNullOrWhiteSpace(query))
                return query;

            // تجهيز القيمة وتنسيقها (إضافة ' ' إذا كانت نصية)
            string formattedIdValue = int.TryParse(newIdValue, out _) ? newIdValue : $"'{newIdValue}'";

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
                    // إقحام Id في الأعمدة، وإقحام القيمة بعد كلمة SELECT مباشرة
                    string newColumns = $"Id, {columnsStr}";
                    string newSelectClause = Regex.Replace(selectClause, @"(?i)^SELECT\s+", $"SELECT {formattedIdValue}, ");

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

            // ------------------------------------------------------------------
            // تغليف الاستعلام بـ SET IDENTITY_INSERT إذا كان Auto Increment
            // ------------------------------------------------------------------
            if (isAutoIncrement && !string.IsNullOrEmpty(tableName))
            {
                return $"SET IDENTITY_INSERT {tableName} ON; {finalSqlQuery} SET IDENTITY_INSERT {tableName} OFF;";
            }

            return finalSqlQuery;
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