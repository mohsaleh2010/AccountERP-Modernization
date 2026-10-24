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
    public class Govern_AreaViewModel
    {
        public long Id { get; set; } = 0;
        public long CountryId { get; set; }
        public string AreaName { get; set; }
        public string CountryName { get; set; }

    }
    public class Govern_AreaCLS
    {
        public Govern_AreaCLS()
        {
        }
        //public Gov_Country_List ps = new Gov_Country_List();
        public Govern_Area_List govArea = new Govern_Area_List();

        CountryCLS ps = new CountryCLS();

        public List<Govern_AreaViewModel> Get_Govern_Area(ref AccountModelsLib.operations opp, long Gov_Country_ListId)
        {
            //var areas = opp.db.Govern_Area_List.Where(i => i.Gov_Country_ListId == Gov_Country_ListId).Include(i => i.Gov_Country_List).OrderBy(i => i.Area_Name).ToList();

            IQueryable<Govern_Area_List> query = opp.db.Govern_Area_List.Include(i => i.Gov_Country_List).OrderBy(i => i.Area_Name) ;
            if(Gov_Country_ListId>0)
            {
                query = query.Where(i => i.Gov_Country_ListId == Gov_Country_ListId);
            }

            List<Govern_AreaViewModel> areaList = new List<Govern_AreaViewModel>();
            foreach (var item in query)
            {
                Govern_AreaViewModel gg = new Govern_AreaViewModel();
                gg.AreaName = item.Area_Name;
                gg.CountryId = item.Gov_Country_ListId;
                gg.CountryName = item.Gov_Country_List.NameAr;
                gg.Id = item.Id;

                areaList.Add(gg);
            }


            return areaList;
        }
        public List<Govern_AreaViewModel> Get_Govern_Area(ref AccountModelsLib.operations opp)
        {
            var areas = opp.db.Govern_Area_List.Include(i => i.Gov_Country_List).OrderBy(i => i.Area_Name).ToList();
            List<Govern_AreaViewModel> areaList = new List<Govern_AreaViewModel>();
            foreach (var item in areas)
            {
                Govern_AreaViewModel gg = new Govern_AreaViewModel();
                gg.AreaName = item.Area_Name;
                gg.CountryId = item.Gov_Country_ListId;
                gg.CountryName = item.Gov_Country_List.NameAr;
                gg.Id = item.Id;

                areaList.Add(gg);
            }


            return areaList;
            //select new
            //            {
            //                Id = a.Id,
            //                AreaName = a.Area_Name,
            //                CountryName = a.Gov_Country_List.NameAr,
            //                CountryId = a.Gov_Country_List.Id

            //            };

            //List<Govern_AreaViewModel> areaList = new List<Govern_AreaViewModel>();
            //foreach (var item in areas)
            //{
            //    Govern_AreaViewModel gg = new Govern_AreaViewModel();
            //    gg.AreaName = item.AreaName;
            //    gg.CountryId = item.CountryId;
            //    gg.CountryName = item.CountryName;
            //    gg.Id = item.Id;

            //    areaList.Add(gg);
            //}


            //return areaList;
        }
        public Govern_Area_List GetGovern_Area(ref AccountModelsLib.operations opp, long? Id)
        {

            Govern_Area_List ga = opp.db.Govern_Area_List.Where(i => i.Id == Id).FirstOrDefault();

            return ga;
        }


        public void Add_Govern_Area(ref int Excp, ref string Msg, Govern_Area_List ga)
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
                    opp1.db.Govern_Area_List.Attach(ga);
                    opp1.db.Entry(ga).State = EntityState.Modified;

                }
                else
                {
                    opp1.db.Govern_Area_List.Add(ga);
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

        /// <summary>حفظ منطقة لواجهة Razor: التعديل عبر Add_Govern_Area؛ الإضافة تراعى كون Id عمود IDENTITY (راجع GeoIdentityInsertHelper).</summary>
        public void SaveArea(ref int Excp, ref string Msg, Govern_Area_List ga)
        {
            if (ga.Id > 0) { Add_Govern_Area(ref Excp, ref Msg, ga); return; }
            try
            {
                var o = new AccountModelsLib.operations();
                if (GeoIdentityInsertHelper.IsIdentityId(o, "Govern_Area_List"))
                {
                    GeoIdentityInsertHelper.Insert(o,
                        "INSERT INTO Govern_Area_List (Gov_Country_ListId, Area_Name, ProcessNo, Insert_ServerDateTime) VALUES (@c, @n, 0, GETDATE())",
                        ("@c", ga.Gov_Country_ListId), ("@n", ga.Area_Name));
                    Excp = 0;
                    return;
                }
                Add_Govern_Area(ref Excp, ref Msg, ga);
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.InnerException != null ? ee.InnerException.Message : ee.Message;
            }
        }

        /// <summary>منطقة بمعرّفها بلا تتبع (لتعديلها ثم تمريرها لـAdd_Govern_Area) - إضافة لواجهة Razor.</summary>
        public Govern_Area_List GetGovern_AreaNoTracking(long Id)
        {
            return new AccountModelsLib.operations().db.Govern_Area_List.AsNoTracking().Where(i => i.Id == Id).FirstOrDefault();
        }

        public void Delete_ProjectUnites(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Id)
        {
            try
            {
                // 🟢 إصلاح خلل الأصل (بموافقة المستخدم): كان الفحص `IQueryable != null` (دائماً true) فيمنع حذف أى منطقة؛ صار Any().
                bool hasGovs = opp.db.Govern_List.Any(i => i.Govern_Area_ListId == Id);
                if (hasGovs)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف المنطقة تم تسجيل محافظات بها";
                    return;
                }

                bool hasCities = opp.db.Gov_City_List.Any(i => i.Govern_Area_ListId == Id);
                if (hasCities)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف المنطقة تم تسجيل مدن بها";
                    return;
                }

                Govern_Area_List ga = opp.db.Govern_Area_List.Where(i => i.Id == Id).SingleOrDefault();
                if (ga != null)
                {
                    opp.db.Govern_Area_List.Remove(ga);
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
