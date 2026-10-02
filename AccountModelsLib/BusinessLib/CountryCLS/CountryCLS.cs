using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;

using Microsoft.EntityFrameworkCore;
 

using System.Linq;

using AccountModelsLib.Models;
namespace AccountModelsLib
{
    public class CountryCLS
    {
        public CountryCLS()
        {
        }
        private DataTable dataTable = null;
        AccountModelsLib.operations opp = new AccountModelsLib.operations();
        public DataTable GetData()
        {
            opp = new AccountModelsLib.operations( );
            string mysql = "Select * from Gov_Country_List";
            dataTable = opp.return_DataTableWithBuilder(mysql);

            if(dataTable.Rows.Count==0)
            {
                AddFirstCountry();
                opp = new AccountModelsLib.operations();
                dataTable = opp.return_DataTableWithBuilder(mysql);

            }
             
            return dataTable;

        }
        void AddFirstCountry()
        {
            opp = new AccountModelsLib.operations(2);
            string mysql = "Truncate Table Gov_Country_List";
            opp.insert_me(mysql);

            Gov_Country_List gc = new Gov_Country_List();
            gc.NameAr = "مصر";
            gc.NameEn = "Egypt";

            opp.db.Gov_Country_List.Add(gc);
            opp.db.SaveChanges();
            opp.Commit();
        }
        public Gov_Country_List GetCountry(ref AccountModelsLib.operations opp, long? Id)
        {
            Gov_Country_List gc = opp.db.Gov_Country_List.Where(i => i.Id == Id).FirstOrDefault();

            return gc;
        }

        // 🟢 إضافات لواجهة Razor (بطلب المستخدم: كود الأعمال فى AccountModelsLib لا داخل الصفحة). لا تغيّر أى دالة قائمة.
        /// <summary>كل الدول بلا تتبع (للعرض والقوائم).</summary>
        public List<Gov_Country_List> GetCountriesList()
        {
            var o = new AccountModelsLib.operations();
            var list = o.db.Gov_Country_List.AsNoTracking().OrderBy(i => i.NameAr).ToList();
            if (list.Count == 0)
            {
                GetData(); // يُنشئ «مصر» كأول دولة كما فى الأصل
                list = new AccountModelsLib.operations().db.Gov_Country_List.AsNoTracking().OrderBy(i => i.NameAr).ToList();
            }
            return list;
        }

        /// <summary>إضافة دولة (Id=0) أو تعديل اسمَيها العربى/الإنجليزى (Id&gt;0) مع إبقاء باقى الأعمدة كما هى.</summary>
        public void SaveCountry(ref int Excp, ref string Msg, Gov_Country_List country)
        {
            try
            {
                var o = new AccountModelsLib.operations();
                if (country.Id > 0)
                {
                    var existing = o.db.Gov_Country_List.Where(i => i.Id == country.Id).SingleOrDefault();
                    if (existing == null) { Excp = 1; Msg = "الدولة غير موجودة"; return; }
                    existing.NameAr = country.NameAr;
                    existing.NameEn = country.NameEn;
                }
                else if (GeoIdentityInsertHelper.IsIdentityId(o, "Gov_Country_List"))
                {
                    GeoIdentityInsertHelper.Insert(o,
                        "INSERT INTO Gov_Country_List (NameAr, NameEn, Code, Insert_ServerDateTime) VALUES (@ar, @en, @code, GETDATE())",
                        ("@ar", country.NameAr), ("@en", country.NameEn), ("@code", country.Code));
                    Excp = 0;
                    return;
                }
                else
                {
                    o.db.Gov_Country_List.Add(country);
                }
                o.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>دولة بمعرّفها بلا تتبع.</summary>
        public Gov_Country_List GetCountryNoTracking(long Id)
        {
            return new AccountModelsLib.operations().db.Gov_Country_List.AsNoTracking().Where(i => i.Id == Id).FirstOrDefault();
        }

        public void UpdateDataTable()
        {
            opp.ada.Update(dataTable);


        }
        public void Delete (ref int Excp, ref string Msg,  long Id)
        {
            try
            {
                opp = new AccountModelsLib.operations();
                // 🟢 إصلاح خلل الأصل (بموافقة المستخدم): فحص "هل يوجد أبناء" كان `IQueryable != null` (دائماً true) فيمنع الحذف دائماً،
                // وكان يفحص أعمدة خاطئة ويحذف من جدول المناطق بمعرّف الدولة. الآن: Any() على عمود الدولة الصحيح، والحذف من جدول الدول.
                bool hasAreas = opp.db.Govern_Area_List.Any(i => i.Gov_Country_ListId == Id);
                if (hasAreas)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف الدولة تم تسجيل مناطق بها";
                    return;
                }


                bool hasGovs = opp.db.Govern_List.Any(i => i.Gov_Country_ListId == Id);
                if (hasGovs)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف الدولة تم تسجيل محافظات بها";
                    return;
                }

                bool hasCities = opp.db.Gov_City_List.Any(i => i.Gov_Country_ListId == Id);
                if (hasCities)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف الدولة تم تسجيل مدن بها";
                    return;
                }

                Gov_Country_List gc = opp.db.Gov_Country_List.Where(i => i.Id == Id).SingleOrDefault();
                if (gc != null)
                {
                    opp.db.Gov_Country_List.Remove(gc);
                    opp.db.SaveChanges();
                    Excp = 0;
                }
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }


        }
    }
}

namespace AccountModelsLib
{
    /// <summary>
    /// 🟢 مساعد لإدراج صف فى جدول عموده Id من نوع IDENTITY (الدول/المناطق/المدن فى قواعد كثيرة). مسار EF فى MyDbContext يُسند Id (Snowflake) صراحةً
    /// حين Common.Use_Auto_Identity_Column = 0 فيفشل بـ"Cannot insert explicit value for identity column". هنا نُدرج بأمر SQL بمعاملات بلا Id
    /// فتولّده قاعدة البيانات. يُستخدم فقط من دوال الحفظ الجديدة لواجهة Razor؛ لا يغيّر أى دالة قائمة تستخدمها WinForms.
    /// </summary>
    internal static class GeoIdentityInsertHelper
    {
        public static bool IsIdentityId(AccountModelsLib.operations o, string table)
        {
            bool wasClosed = o.conn.State == System.Data.ConnectionState.Closed;
            if (wasClosed) o.conn.Open();
            try
            {
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM sys.identity_columns WHERE object_id = OBJECT_ID(@t) AND name = 'Id'", o.conn))
                {
                    cmd.Parameters.AddWithValue("@t", table);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
            finally
            {
                if (wasClosed) o.conn.Close();
            }
        }

        public static void Insert(AccountModelsLib.operations o, string sql, params (string Name, object Value)[] parameters)
        {
            bool wasClosed = o.conn.State == System.Data.ConnectionState.Closed;
            if (wasClosed) o.conn.Open();
            try
            {
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, o.conn))
                {
                    foreach (var p in parameters)
                    {
                        cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (wasClosed) o.conn.Close();
            }
        }
    }
}
