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
    public class Govern_ListViewModel
    {
        public long Id { get; set; } = 0;
        public long CountryId { get; set; }
        public long AreaId { get; set; }
        public string GovName { get; set; }
        public string AreaName { get; set; }
        public string CountryName { get; set; }

    }
    public class Govern_ListCLS
    {
        public Govern_ListCLS()
        {
        }
        //public Gov_Country_List ps = new Gov_Country_List();
        public Govern_Area_List govArea = new Govern_Area_List();

        CountryCLS ps = new CountryCLS();
        public Gov_Country_List GetGov_Country_List(ref AccountModelsLib.operations opp, long? Id)
        {
            Gov_Country_List gc = opp.db.Gov_Country_List.Where(i => i.Id == Id).FirstOrDefault();

            return gc;
        }
        public List<Govern_ListViewModel> Get_Govern_List(ref AccountModelsLib.operations opp, long Gov_Country_ListId, long Govern_Area_ListId)
        {
            var areas = opp.db.Govern_List.Where(i => i.Gov_Country_ListId == Gov_Country_ListId & i.Govern_Area_ListId == Govern_Area_ListId).Include(i => i.Govern_Area_List).Include(i => i.Gov_Country_List).OrderBy(i => i.Gov_Name).ToList();

            IQueryable<Govern_List> query = opp.db.Govern_List.Include(i => i.Govern_Area_List).Include(i => i.Gov_Country_List).OrderBy(i => i.Gov_Name);

            if (Gov_Country_ListId > 0)
            {
                query = query.Where(i => i.Gov_Country_ListId == Gov_Country_ListId);
            }
            if (Gov_Country_ListId > 0)
            {
                query = query.Where(i => i.Govern_Area_ListId == Govern_Area_ListId);
            }

            List<Govern_ListViewModel> areaList = new List<Govern_ListViewModel>();
            foreach (var item in query)
            {
                Govern_ListViewModel gg = new Govern_ListViewModel();
                gg.Id = item.Id;
                gg.GovName = item.Gov_Name;
                gg.AreaId = item.Govern_Area_List.Id;
                gg.AreaName = item.Govern_Area_List.Area_Name; ;
                gg.CountryId = item.Gov_Country_ListId;
                gg.CountryName = item.Gov_Country_List.NameAr;


                areaList.Add(gg);
            }


            return areaList;
        }
        public List<Govern_ListViewModel> Get_Govern_List(ref AccountModelsLib.operations opp)
        {
            var areas = opp.db.Govern_List.Include(i => i.Govern_Area_List).Include(i => i.Gov_Country_List).OrderBy(i => i.Gov_Name).ToList();


            List<Govern_ListViewModel> areaList = new List<Govern_ListViewModel>();
            foreach (var item in areas)
            {
                Govern_ListViewModel gg = new Govern_ListViewModel();
                gg.Id = item.Id;
                gg.GovName = item.Gov_Name;
                gg.AreaId = item.Govern_Area_List.Id;
                gg.AreaName = item.Govern_Area_List.Area_Name; ;
                gg.CountryId = item.Gov_Country_ListId;
                gg.CountryName = item.Gov_Country_List.NameAr;

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

            //List<Govern_ListViewModel> areaList = new List<Govern_ListViewModel>();
            //foreach (var item in areas)
            //{
            //    Govern_ListViewModel gg = new Govern_ListViewModel();
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


        public void Add_Govern_List(ref int Excp, ref string Msg, Govern_List ga)
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
                    opp1.db.Govern_List.Attach(ga);
                    opp1.db.Entry(ga).State = EntityState.Modified;

                }
                else
                {
                    opp1.db.Govern_List.Add(ga);
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

        /// <summary>محافظة بمعرّفها بلا تتبع (لتعديلها ثم تمريرها لـAdd_Govern_List) - إضافة لواجهة Razor.</summary>
        public Govern_List GetGovern_ListNoTracking(long Id)
        {
            return new AccountModelsLib.operations().db.Govern_List.AsNoTracking().Where(i => i.Id == Id).FirstOrDefault();
        }

        public void Delete(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Id)
        {
            try
            {


                // 🟢 إصلاح خلل الأصل (بموافقة المستخدم): كان الفحص `IQueryable != null` (دائماً true) فيمنع حذف أى محافظة؛ صار Any().
                bool hasCities = opp.db.Gov_City_List.Any(i => i.Govern_ListId == Id);
                if (hasCities)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف المحافظة تم تسجيل مدن بها";
                    return;
                }
                 Govern_List ga = opp.db.Govern_List.Where(i => i.Id == Id).SingleOrDefault();
                if (ga != null)
                {
                    opp.db.Govern_List.Remove(ga);
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
