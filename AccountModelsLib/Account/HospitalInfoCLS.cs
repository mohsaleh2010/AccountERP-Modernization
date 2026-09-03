using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Data;
namespace AccountModelsLib
{
    public class HospitalInfoCLS
    {
        public HospitalInfoCLS()
        {

        }
        public static string CompanyID = "1";
        public static string CompanyName = "AL-Deuli Hospital";
        public static string CompanyName_Ar = "مستشفى الديولى الخيرى";
        public static string StreetAddress = "";
        public static string StreetAddress_Ar = "ميت بدر حلاوة";
        public static string SloganText = "";
        public static string Hotline1 = "040/2863475";
        public static string Hotline2 = "040/2864717";
        public static string Email = "";
        public static string Website = "";
        public static string CurrencySign = "";
        public static string Logo = "";
        public static string Mobile1 = "01092006494";
        public static string Mobile2 = "";


        public static  void CreateTable()
        {
            string mysql = "";
            operations opp = new operations();
            try
            {
                mysql = @"drop TABLE  [tblCompany]";
                opp.insert_me(mysql);
            }
            catch { }

            //try
            //{
            //    mysql = @" truncate TABLE  [tblCompany]";
            //    opp.insert_me(mysql);
            //}
            //catch { }
            mysql = @"CREATE TABLE [dbo].[tblCompany](
	[CompanyID] [int] NULL,
	[CompanyName] [nvarchar](250) NULL,
	[CompanyName_Ar] [nvarchar](250) NULL,
	[StreetAddress] [nvarchar](250) NULL,
	[StreetAddress_Ar] [nvarchar](250) NULL,
	[SloganText] [nvarchar](250) NULL,
	[Hotline1] [nvarchar](50) NULL,
	[Hotline2] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[Website] [nvarchar](50) NULL,
	[Logo] [image] NULL,
	[CurrencySign] [nvarchar](50) NULL,
	[Mobile1] [nvarchar](50) NULL,
	[Mobile2] [nvarchar](50) NULL
)";

            try
            {

                opp.insert_me(mysql);
            }
            catch { }

            try
            {
                mysql = @" INSERT INTO tblCompany(CompanyID, CompanyName, CompanyName_Ar, StreetAddress, StreetAddress_Ar, SloganText, Hotline1, Hotline2, Email, Website,    CurrencySign, Mobile1, Mobile2) ";
                mysql = mysql + " VALUES ("+ CompanyID + ",N'"+ CompanyName + "',N'"+ CompanyName_Ar + "',N'"+ StreetAddress + "',N'"+ StreetAddress_Ar + "',N'"+ SloganText + "',N'"+ Hotline1 + "',N'"+ Hotline2 + "',N'"+ Email + "',N'"+ Website + "'  ,N'"+ CurrencySign + "',N'"+ Mobile1 + "',N'"+ Mobile2 + "')";

                opp.insert_me(mysql);

                 LogoIMG();
            }
            catch(Exception ee ) { }
        }

        public static void  LogoIMG()
        {
            //Bitmap img = global::AccountModelsLib.Properties.Resources.logo;
            ////image img = global::GHospital_Care.Properties.Resources.logo;
            //byte[] imageData;
            //ImageConverter converter = new ImageConverter();
            //imageData = (byte[])converter.ConvertTo(img, typeof(byte[]));
            
            //string mysql = "update tblCompany set Logo=@ImageData Where CompanyID=1";
            //operations opp = new operations();
            //opp.cmd.Parameters.Add(new SqlParameter("@ImageData", (object)imageData));
            //opp.insert_me(mysql);

             

        }

        public static DataTable GetCompany()
        {
           

            string mysql = " SELECT TOP (1)   tblCompany.* FROM tblCompany WHERE(CompanyID = 1)";
            operations opp = new operations();
            DataTable dt = opp.return_DataTable(mysql);
            return dt;

            

        }
    }
}
