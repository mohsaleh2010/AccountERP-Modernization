using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccountModelsLib
{
    /// <summary>صف معدل ضريبى: القيمتان القابلتان للتعديل TaxRate و MinAllowedValueToComputeTax فقط.</summary>
    public class TaxRateRow
    {
        public long Id { get; set; }
        public string TaxName_Ar { get; set; } = "";
        public string TaxName_En { get; set; } = "";
        public decimal TaxRate { get; set; }
        public decimal MinAllowedValue { get; set; }
    }

    /// <summary>
    /// المعدلات الضريبية (خطة 05 - المرحلة 7، «ملف» ← «بيانات المعدلات الضريبية») بدل TaxListFrm.
    /// تعديل العمودين فقط؛ لا إضافة ولا حذف، ولا «إعادة الضبط» (Set_TaxInfo تحذف الجدول وتعيد إنشاءه).
    /// الاستعلام بأعمدة صريحة لأن قاعدة البيانات قد لا تحوى MainCode/SubCode التى فى الموديل.
    /// </summary>
    public class TaxRatesService
    {
        private readonly Func<operations> _newOpp;

        public TaxRatesService(Func<operations> newOpp = null)
        {
            _newOpp = newOpp ?? (() => new operations());
        }

        public List<TaxRateRow> GetRates()
        {
            var db = _newOpp().db;
            var conn = db.Database.GetDbConnection();
            bool wasClosed = conn.State != ConnectionState.Open;
            if (wasClosed) conn.Open();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, TaxName_Ar, TaxName_En, TaxRate, MinAllowedValueToComputeTax FROM AccountTaxInfo ORDER BY TaxName_Ar";
                var list = new List<TaxRateRow>();
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new TaxRateRow
                    {
                        Id = r.GetInt64(0),
                        TaxName_Ar = r.IsDBNull(1) ? "" : r.GetString(1),
                        TaxName_En = r.IsDBNull(2) ? "" : r.GetString(2),
                        TaxRate = r.IsDBNull(3) ? 0m : r.GetDecimal(3),
                        MinAllowedValue = r.IsDBNull(4) ? 0m : r.GetDecimal(4),
                    });
                return list;
            }
            finally { if (wasClosed) conn.Close(); }
        }

        /// <summary>يحفظ الصفوف المعدلة. Excp: 0 نجاح، 2 مرفوض (Msg)، 1 خطأ. المعدل بين 0 و100 والحد الأدنى غير سالب.</summary>
        public void Save(ref int Excp, ref string Msg, IEnumerable<TaxRateRow> rows)
        {
            Excp = 0;
            Msg = "";
            try
            {
                var list = rows.ToList();
                foreach (var row in list)
                {
                    if (row.TaxRate < 0 || row.TaxRate > 100) { Excp = 2; Msg = $"{row.TaxName_Ar}: 0 - 100"; return; }
                    if (row.MinAllowedValue < 0) { Excp = 2; Msg = $"{row.TaxName_Ar}: >= 0"; return; }
                }
                var db = _newOpp().db;
                using var tx = db.Database.BeginTransaction();
                foreach (var row in list)
                {
                    decimal rate = row.TaxRate, min = row.MinAllowedValue;
                    long id = row.Id;
                    db.Database.ExecuteSqlInterpolated(
                        $"UPDATE AccountTaxInfo SET TaxRate = {rate}, MinAllowedValueToComputeTax = {min} WHERE Id = {id}");
                }
                tx.Commit();
            }
            catch (Exception ex) { Excp = 1; Msg = ex.InnerException?.Message ?? ex.Message; }
        }
    }
}
