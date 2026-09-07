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

        public void Delete(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp, long Id)
        {
            try
            {
                

                var customerL = opp.db.Customer.Where(i => i.Gov_City_ListId == Id);
                if (customerL != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف المدينة تم تسجيل عميل بها";
                    return;
                }

                var supplierL = opp.db.Suppliers.Where(i => i.Gov_City_ListId == Id);
                if (customerL != null)
                {
                    Excp = 1;
                    Msg = "لا يمكن حذف المدينة تم تسجيل مورد بها";
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
