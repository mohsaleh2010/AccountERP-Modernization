using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class Residentialbuilding
    {
        
        public long Id { get; set; } = 0;
        public long ProjectId { get; set; } = 0;
        public string Name { get; set; }
        public string Address { get; set; }
       
        public DataTable Get_Residentialbuilding(long ProjectId = 0)
        {
            operations opp = new operations();
            return Get_ResidentialbuildingByProject(ref opp, ProjectId);
        }
        public DataTable Get_Residentialbuilding(ref operations opp, long Id = 0)
        {
             
            string mysql = " Select * from ConsResidentialbuilding  ";

            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }

            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void GetResidentialbuildingById(ref operations opp, long Id)
        {
          

            DataTable dt = Get_Residentialbuilding(ref opp, Id);
            ReadData(dt, 0);


        }
        public void ReadData(DataTable dt, int index)
        {
            Id = int.Parse(dt.Rows[index]["Id"].ToString());
            ProjectId = int.Parse(dt.Rows[index]["ProjectId"].ToString());
            Name = dt.Rows[index]["Name"].ToString();
           
            Address = dt.Rows[index]["Address"].ToString();

        }
        public DataTable Get_ResidentialbuildingByProject(ref operations opp, long ProjectId = 0)
        {
            string mysql = "";
            if (ProjectId == 0)
            {
                mysql = " Select * from ConsResidentialbuilding  ";
            }
            else
            {
                mysql = " Select * from ConsResidentialbuilding  Where ProjectId=" + ProjectId;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable Get_ResidentialbuildingFull(ref operations opp, long ProjectId = 0)
        {
            string mysql = "";
            mysql = @" SELECT        ConsResidentialbuilding.*, ConsProject.Name AS ProjectName
                           FROM ConsResidentialbuilding INNER JOIN
                         ConsProject ON ConsResidentialbuilding.ProjectId = ConsProject.Id  ";

            if (ProjectId > 0)
            {

                mysql = mysql + "  Where ConsResidentialbuilding.ProjectId=" + ProjectId;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }

        public DataTable Get_ResidentialbuildingByName(ref operations opp)
        {
            string mysql = " Select * from ConsResidentialbuilding Where  Name='" + Name + "'  ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void InsertResidentialbuilding(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsResidentialbuilding (Name ,Address ,ProjectId) ";
            mysql = mysql + " VALUES ('" + Name + "', '" + Address + "' ," + ProjectId + " )";

            opp.insert_me(mysql);

        }
        public void UpdateResidentialbuilding(ref operations opp)
        {

            string mysql = @"Update  ConsResidentialbuilding  Set Name='" + Name + "' ,Address= '" + Address + "' ,ProjectId=" + ProjectId + " ";
            mysql = mysql + "  Where Id=" + Id;

            opp.insert_me(mysql);

        }
        public void DeleteResidentialbuilding(ref operations opp)
        {

            string mysql = @" Delete from Residentialbuilding   ";
            mysql = mysql + "  Where Id=" + Id;

            opp.insert_me(mysql);

        }
    }
}
