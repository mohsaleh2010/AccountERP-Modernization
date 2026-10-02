using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class ProjectServices
    {
        public ProjectServices()
        {

        }
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long ServiceId { get; set; }
        public decimal Price { get; set; }

        public long SubAccount_Id { get; set; } = 0;

        //public Service s = new Service();
        //public Project p = new Project();
        public DataTable Get_ProjectServices()
        {
            operations opp = new operations();
            return Get_ProjectServices(ref opp);
        }
        public DataTable Get_ProjectServices(ref operations opp, long Id = 0)
        {
            string mysql = " Select * from ConsProjectServices  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void GetProjectServicesById(ref operations opp, long Id)
        {

            DataTable dt = Get_ProjectServices(ref opp, Id);
            ReadData(dt, 0);


        }
        public void ReadData(DataTable dt, int index)
        {
            Id = long.Parse(dt.Rows[index]["Id"].ToString());
            ProjectId = int.Parse(dt.Rows[index]["ProjectId"].ToString());
            ServiceId = long.Parse(dt.Rows[index]["ServiceId"].ToString());
            Price = decimal.Parse(dt.Rows[index]["Price"].ToString());
            SubAccount_Id= long.Parse(dt.Rows[index]["SubAccount_Id"].ToString());

        }
        public DataTable GetProjectServicesFull(ref operations opp, long ProjectId, long ServiceId)
        {
            string mysql = @" SELECT        ConsProject.Name AS ProjectName, ConsService.Name AS ServiceName, ConsProjectServices.*  , 
                         Submain_Account.Account_Customer_Type_Value, Submain_Account.Account_Customer_Type_ID, Submain_Account.SubAccount_Name
FROM            ConsProjectServices INNER JOIN
                         ConsProject ON ConsProjectServices.ProjectId = ConsProject.Id INNER JOIN
                         ConsService ON ConsProjectServices.ServiceId = ConsService.Id LEFT OUTER JOIN
                         Submain_Account ON ConsProjectServices.SubAccount_Id = Submain_Account.Id ";
            mysql = mysql + @" WHERE(ConsProjectServices.Id> 0) ";
            if (ProjectId > 0)
            {
                mysql = mysql + @"  And (ConsProjectServices.ProjectId = " + ProjectId + ")   ";
            }
            if (ServiceId > 0)
            {
                mysql = mysql + @"   AND(ConsProjectServices.ServiceId = " + ServiceId + ")  ";
            }
            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }

        public void InsertProjects(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsProjectServices (ProjectId ,ServiceId ,Price,SubAccount_Id) ";
            mysql = mysql + " VALUES ( " + ProjectId + " ,  " + ServiceId + "  ," + Price + " ,"+ SubAccount_Id + ")";

            opp.insert_me(mysql);

        }

        public void UpdateProjects(ref operations opp)
        {

            string mysql = @"Update ConsProjectServices Set ProjectId=" + ProjectId + " ,ServiceId=" + ServiceId + "  ,Price=" + Price + " ,SubAccount_Id="+ SubAccount_Id + "";
            mysql = mysql + " Where Id=" + Id;

            opp.insert_me(mysql);

        }
        public void DeleteProjects(ref operations opp)
        {

            string mysql = @"Delete From ConsProjectServices   ";
            mysql = mysql + " Where Id=" + Id;
            opp.insert_me(mysql);

        }
    }
}
