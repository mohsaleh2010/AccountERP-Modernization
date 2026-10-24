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
    public class City_ListViewModel
    {
        public long Id { get; set; } = 0;
        public string CityName { get; set; }
        public long? CountryId { get; set; }
        public string CountryName { get; set; }
        public long AreaId { get; set; }
       
        public string AreaName { get; set; }
        public long GovId { get; set; }

        public string GovName { get; set; }
    }
    public class Gov_City_ListCLS
    {
        public Gov_City_ListCLS()
        {
        }
        //public Gov_Country_List ps = new Gov_Country_List();
        public Govern_Area_List govArea = new Govern_Area_List();

        CountryCLS ps = new CountryCLS();
        public Gov_City_List GetGov_City_List(ref AccountModelsLib.operations opp, long? Id)
        {
            Gov_City_List gc = opp.db.Gov_City_List.Where(i => i.Id == Id).FirstOrDefault();

            return gc;
        }
        public List<City_ListViewModel> Get_City_List(ref AccountModelsLib.operations opp,long Gov_Country_ListId,long Govern_Area_ListId, long Govern_ListId)
        {
            //var areas = opp.db.Gov_City_List.Where(i => i.Gov_Country_ListId == Gov_Country_ListId & i.Govern_Area_ListId== Govern_Area_ListId & i.Govern_ListId == Govern_ListId).Include(i=>i.Govern_List).Include(i=>i.Govern_Area_List).Include(i=>i.Gov_Country_List).OrderBy(i=>i.NameAr).ToList();


            //var areas1 = opp.db.Gov_City_List.Include(i => i.Govern_List).Include(i => i.Govern_Area_List).Include(i => i.Gov_Country_List).OrderBy(i => i.NameAr);
            //var areas11 =  areas1.Where(i => i.Gov_Country_ListId == Gov_Country_ListId);
            //var areas111 =  areas11.Where(i => i.Govern_Area_ListId == Govern_Area_ListId);
            //var areas1111 =  areas111.Where(i => i.Govern_ListId == Govern_ListId);

            IQueryable<Gov_City_List> query = opp.db.Gov_City_List.Include(i => i.Govern_List).Include(i => i.Govern_Area_List).Include(i => i.Gov_Country_List).OrderBy(i => i.NameAr);
            if (Gov_Country_ListId > 0)
            {
                query =  query.Where(i => i.Gov_Country_ListId == Gov_Country_ListId);
            }
            if (Govern_Area_ListId > 0)
            {
                query = query.Where(i => i.Govern_Area_ListId == Govern_Area_ListId);
            }
            if (Govern_ListId > 0)
            {
                query = query.Where(i => i.Govern_ListId == Govern_ListId);
            }



             
            List<City_ListViewModel> areaList = new List<City_ListViewModel>();
            foreach (var item in query)
            {
                City_ListViewModel gg = new City_ListViewModel();
                gg.Id = item.Id;
                gg.CityName = item.NameAr;
                gg.GovId = item.Govern_List.Id;
                gg.GovName = item.Govern_List.Gov_Name;
                gg.AreaId = item.Govern_Area_List.Id;
                gg.AreaName = item.Govern_Area_List.Area_Name;;
                gg.CountryId = item.Gov_Country_ListId;
                gg.CountryName = item.Gov_Country_List.NameAr;
               

                areaList.Add(gg);
            }


            return areaList;
        }
        public List<City_ListViewModel> Get_City_List(ref AccountModelsLib.operations opp )
        {
            List<City_ListViewModel> areaList = new List<City_ListViewModel>();
            try
            {
                var areas = opp.db.Gov_City_List.Include(i => i.Govern_List).Include(i => i.Govern_Area_List).Include(i => i.Gov_Country_List).OrderBy(i => i.NameAr).ToList();


                
                foreach (var item in areas)
                {
                    City_ListViewModel gg = new City_ListViewModel();
                    gg.Id = item.Id;
                    gg.CityName = item.NameAr;
                    gg.GovId = item.Govern_List.Id;
                    gg.GovName = item.Govern_List.Gov_Name;
                    gg.AreaId = item.Govern_Area_List.Id;
                    gg.AreaName = item.Govern_Area_List.Area_Name; ;
                    gg.CountryId = item.Gov_Country_ListId;
                    gg.CountryName = item.Gov_Country_List.NameAr;


                    areaList.Add(gg);
                }


                
            }
            catch (Exception ee)
            { 
            }
            return areaList;
            //select new
            //            {
            //                Id = a.Id,
            //                AreaName = a.Area_Name,
            //                CountryName = a.Gov_Country_List.NameAr,
            //                CountryId = a.Gov_Country_List.Id

            //            };

            //List<City_ListViewModel> areaList = new List<City_ListViewModel>();
            //foreach (var item in areas)
            //{
            //    City_ListViewModel gg = new City_ListViewModel();
            //    gg.AreaName = item.AreaName;
            //    gg.CountryId = item.CountryId;
            //    gg.CountryName = item.CountryName;
            //    gg.Id = item.Id;

            //    areaList.Add(gg);
            //}


            //return areaList;
        }
        public Govern_Area_List GetGovern_Area(ref AccountModelsLib.operations opp, long Id)
        {

            Govern_Area_List ga = opp.db.Govern_Area_List.Where(i => i.Id == Id).FirstOrDefault();

            return ga;
        }


        public void Add_Gov_City_List(ref int Excp, ref string Msg , Gov_City_List ga)
        {
            AccountModelsLib.operations opp1 = new AccountModelsLib.operations();
            try
            {
                if (ga.Id > 0)
                {
                    
                    //if (ga != null)
                    //{
                    //    opp.db.Entry(ga).State = EntityState.Detached;
                    //}
                    //Govern_Area_List gaDB = opp.db.Govern_Area_List.Where(i => i.Id == ga.Id).FirstOrDefault();

                    //gaDB = ga;

                    //opp.db.Govern_Area_List.Attach(gaDB);
                    //opp.db.Entry(gaDB).State = EntityState.Modified;


                    opp1.db.Entry(ga).State = EntityState.Detached;
                    ga.Edit_User_Id = Common.User_Id; ga.EditOperationDate = DateTime.Now; // تدقيق التعديل (Edit_User_Id + EditOperationDate معاً)
                    opp1.db.Gov_City_List.Attach(ga);
                    opp1.db.Entry(ga).State = EntityState.Modified;

                }
                else
                {
                    opp1.db.Gov_City_List.Add(ga);
                }
                opp1.db.SaveChanges();
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
            }

        }

        /// <summary>حفظ مدينة لواجهة Razor: التعديل عبر Add_Gov_City_List؛ الإضافة تراعى كون Id عمود IDENTITY (راجع GeoIdentityInsertHelper).</summary>
        public void SaveCity(ref int Excp, ref string Msg, Gov_City_List city)
        {
            if (city.Id > 0) { Add_Gov_City_List(ref Excp, ref Msg, city); return; }
            try
            {
                var o = new AccountModelsLib.operations();
                if (GeoIdentityInsertHelper.IsIdentityId(o, "Gov_City_List"))
                {
                    GeoIdentityInsertHelper.Insert(o,
                        "INSERT INTO Gov_City_List (NameAr, NameEn, Code, Gov_Country_ListId, Govern_Area_ListId, Govern_ListId, Insert_ServerDateTime) VALUES (@n, @en, @code, @c, @a, @g, GETDATE())",
                        ("@n", city.NameAr), ("@en", city.NameEn), ("@code", city.Code), ("@c", city.Gov_Country_ListId), ("@a", city.Govern_Area_ListId), ("@g", city.Govern_ListId));
                    Excp = 0;
                    return;
                }
                Add_Gov_City_List(ref Excp, ref Msg, city);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>مدينة بمعرّفها بلا تتبع (لتعديلها ثم تمريرها لـAdd_Gov_City_List) - إضافة لواجهة Razor.</summary>
        public Gov_City_List GetGov_City_ListNoTracking(long Id)
        {
            return new AccountModelsLib.operations().db.Gov_City_List.AsNoTracking().Where(i => i.Id == Id).FirstOrDefault();
        }

        public void Delete(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Id)
        {
            try
            {
                

                // 🟢 إصلاح خللين حقيقيين فى الأصل (بموافقة المستخدم): (1) فحص `IQueryable != null` دائماً true فكان يمنع حذف أى مدينة،
                // وفحص الموردين كان يختبر customerL بدل supplierL؛ (2) الأخطر: كان الحذف الفعلى يزيل صف المحافظة من جدول Govern_List
                // بمعرّف المدينة بدل حذف المدينة من Gov_City_List.
                bool hasCustomers = opp.db.Customer.Any(i => i.Gov_City_ListId == Id);
                if (hasCustomers)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف المدينة تم تسجيل عميل بها";
                    return;
                }

                bool hasSuppliers = opp.db.Suppliers.Any(i => i.Gov_City_ListId == Id);
                if (hasSuppliers)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف المدينة تم تسجيل مورد بها";
                    return;
                }

                Gov_City_List ga = opp.db.Gov_City_List.Where(i => i.Id == Id).SingleOrDefault();
                if (ga != null)
                {
                    opp.db.Gov_City_List.Remove(ga);
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
