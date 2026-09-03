using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace AccountModelsLib
{

 
    using System.Data;
    using Microsoft.Data.SqlClient;
    //using Microsoft.Data.SqlClient;
     

    public static class MySqlCommandExtensions
    {

        public static string GetFullGeneratedQuery(this SqlCommand cmd)
        {
            string query = cmd.CommandText;

            foreach (SqlParameter p in cmd.Parameters)
            {
                string parameterName = p.ParameterName;

                // التأكد من وجود @ في بداية اسم البرامتر
                if (!parameterName.StartsWith("@"))
                    parameterName = "@" + parameterName;

                string formattedValue = FormatSqlValue(p.Value);

                // استبدال اسم البرامتر بالقيمة المنسقة بدقة
                // نستخدم Regex مع \b لتجنب استبدال برامترات تتشابه في جزء من الاسم مثل @Branch_ID و @Branch_ID_Login
                query = System.Text.RegularExpressions.Regex.Replace(
                    query,
                    $@"{parameterName}\b",
                    formattedValue,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return query;
        }

        private static string FormatSqlValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.String:
                case TypeCode.Char:
                    // تنظيف السنجل كوت لمنع كسر الجملة
                    return $"'{value.ToString().Replace("'", "''")}'";

                case TypeCode.DateTime:
                    // تنسيق التاريخ والوقت بالشكل المعياري المفهوم في SQL Server
                    return $"'{((DateTime)value):yyyy-MM-dd HH:mm:ss.fff}'";

                case TypeCode.Boolean:
                    return (bool)value ? "1" : "0";

                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return value.ToString();

                default:
                    if (value is Guid)
                        return $"'{value}'";

                    return $"'{value.ToString().Replace("'", "''")}'";
            }
        }
    }
}