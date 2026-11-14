using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AccountModelsLib
{
    /// <summary>
    /// معاملات عشرية بدقة صريحة لجداول المساعد. المعامل العشرى غير المحدد فى ExecuteSqlInterpolated
    /// يُرسل decimal(18,2) فتُقرَّب التكلفة 0.003 إلى صفر وسعر 0.075 إلى 0.08.
    /// </summary>
    internal static class AiSqlParams
    {
        /// <summary>تكلفة تقديرية بالدولار — أعمدة CostEstimate/ExtractCost من نوع decimal(18,6).</summary>
        public static SqlParameter Cost(decimal? value) => Dec(value, 18, 6);

        /// <summary>سعر المليون رمز — أعمدة InputPricePerM/OutputPricePerM من نوع decimal(18,4).</summary>
        public static SqlParameter PricePerM(decimal? value) => Dec(value, 18, 4);

        public static SqlParameter Dec(decimal? value, byte precision, byte scale) =>
            new SqlParameter { SqlDbType = SqlDbType.Decimal, Precision = precision, Scale = scale, Value = (object)value ?? DBNull.Value };
    }
}
