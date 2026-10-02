using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class ProjectSpacesGroup
    {
         
        public long Id { get; set; } = 0;
        public long ProjectId { get; set; } = 0;
        public string Name { get; set; }
        public decimal Size { get; set; } = 0;



        public DataTable Get_ProjectSpacesGroup()
        {
            operations opp = new operations();
            return Get_ProjectSpacesGroup(ref opp);
        }
        public DataTable GetDataFill_List()
        {
            operations opp = new operations();
            return Get_ProjectSpacesGroup(ref opp);
        }

        public DataTable Get_ProjectSpacesGroup(ref operations opp, long Id = 0)
        {
            string mysql = " Select * from ConsProjectSpacesGroup  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void GetProjectSpacesGroupById(ref operations opp, long Id)
        {
            //if (Id > 0)
            //{
            //    mysql = mysql + " Where Id=" + Id;
            //}

            DataTable dt = Get_ProjectSpacesGroup(ref opp, Id);
            ReadData(dt, 0);


        }
        public void ReadData(DataTable dt, int index)
        {
            Id = long.Parse(dt.Rows[index]["Id"].ToString());
            ProjectId = int.Parse(dt.Rows[index]["ProjectId"].ToString());
            Name = dt.Rows[index]["Name"].ToString();
            Size = decimal.Parse(dt.Rows[index]["Size"].ToString());


        }
        public DataTable GetProjectSpacesGroupFull(ref operations opp, long ProjectId)
        {
            string mysql = @" SELECT        ConsProject.Name AS ProjectName, ConsProjectSpacesGroup.*
FROM            ConsProjectSpacesGroup INNER JOIN
                         ConsProject ON ConsProjectSpacesGroup.ProjectId = ConsProject.Id ";
            mysql = mysql + @" WHERE(ConsProjectSpacesGroup.Id> 0) ";
            if (ProjectId > 0)
            {
                mysql = mysql + @"  And (ConsProjectSpacesGroup.ProjectId = " + ProjectId + ")   ";
            }

            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }

        public void InsertProjectSpacesGroup(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsProjectSpacesGroup(  Name, [Size], ProjectId) ";
            mysql = mysql + " VALUES ( '" + Name + "', " + Size + "   ," + ProjectId + " )";

            opp.insert_me(mysql);

        }

        public void UpdateProjectSpacesGroup(ref operations opp)
        {

            string mysql = @"Update  ConsProjectSpacesGroup Set Name='" + Name + "', [Size]=" + Size + ",   ProjectId=" + ProjectId + " ";
            mysql = mysql + " Where Id=" + Id;

            opp.insert_me(mysql);

        }
        public void DeleteProjectSpacesGroup(ref operations opp)
        {

            string mysql = @"Delete From ConsProjectSpacesGroup   ";
            mysql = mysql + " Where Id=" + Id;
            opp.insert_me(mysql);

        }
    }
}
