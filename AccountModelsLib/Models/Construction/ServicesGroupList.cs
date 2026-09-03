using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class ServicesGroupList
    {
         
        public long Id { get; set; } = 0;
        public long ProjectId { get; set; }
        public string Name { get; set; }

        //public Service s = new Service();
        //public Project p = new Project();
        public DataTable Get_ServicesGroup()
        {
            operations opp = new operations();
            return Get_ServicesGroup(ref opp);
        }


        public DataTable GetDataFill_List()
        {
            operations opp = new operations();
            return Get_ServicesGroup(ref opp);
        }
        public DataTable Get_ServicesGroup(ref operations opp, long Id = 0)
        {
            string mysql = " Select * from ConsServicesGroupList  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }

            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable Get_ServicesGroupListByProject(long ProjectId = 0)
        {
            operations opp = new operations();
            string mysql = " Select * from ConsServicesGroupList  ";
            if (ProjectId > 0)
            {
                mysql = mysql + " Where ProjectId=" + ProjectId;
            }

            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable Get_ServicesGroupListNotAssign(long ProjectId, long ServicesGroupListId)
        {
            operations opp = new operations();
            string mysql = @" SELECT        ConsService.Name, ConsService.Id, ConsProjectServices.ServiceId, ConsProjectServices.ProjectId
FROM            ConsService INNER JOIN
                         ConsProjectServices ON ConsService.Id = ConsProjectServices.ServiceId
WHERE        (ConsProjectServices.ServiceId NOT IN
                             (SELECT ServicesId 
                                FROM            ConsServicesGroups WHERE        (ServicesGroupListId = " + ServicesGroupListId + "))) AND(ConsProjectServices.ProjectId = " + ProjectId + ") ";


            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable Get_ServicesGroupListAssign(long GroupListId)
        {
            operations opp = new operations();
            string mysql = @" SELECT        ConsServicesGroups.Id, ConsServicesGroups.ServicesGroupListId, ConsServicesGroups.ServicesId, ConsService.Name
                FROM ConsServicesGroups INNER JOIN
                         ConsService ON ConsServicesGroups.ServicesId = ConsService.Id ";

            mysql = mysql + " WHERE   (ConsServicesGroups.ServicesGroupListId  = " + GroupListId + ") ";


            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public DataTable Get_ServicesGroupListAssignWithProject(long ProjectId, long GroupListId)
        {
            operations opp = new operations();
            string mysql = @" SELECT DISTINCT 
                         ConsServicesGroups.Id, ConsServicesGroups.ServicesGroupListId, ConsServicesGroups.ServicesId, ConsService.Name, ConsProjectServices.Price, ConsProjectServices.SubAccount_Id, ConsProjectServices.ProjectId
FROM            ConsServicesGroups INNER JOIN
                         ConsService ON ConsServicesGroups.ServicesId = ConsService.Id INNER JOIN
                         ConsProjectServices ON ConsServicesGroups.ServicesId = ConsProjectServices.ServiceId ";

            mysql = mysql + " WHERE (ConsServicesGroups.Id > 0)";
            mysql = mysql + " AND (ConsServicesGroups.ServicesGroupListId  = " + GroupListId + ") ";
            mysql = mysql + " AND (ConsProjectServices.ProjectId = " + ProjectId + ")";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void Get_ServicesGroupById(ref operations opp, long Id)
        {
            //if (Id > 0)
            //{
            //    mysql = mysql + " Where Id=" + Id;
            //}

            DataTable dt = Get_ServicesGroup(ref opp, Id);
            if (dt.Rows.Count > 0)
            {
                ReadData(dt, 0);
            }



        }
        public void ReadData(DataTable dt, int index)
        {
            Id = int.Parse(dt.Rows[index]["Id"].ToString());
            ProjectId = int.Parse(dt.Rows[index]["ProjectId"].ToString());
            Name = dt.Rows[index]["Name"].ToString();

        }
        public DataTable GetServicesGroupFull(ref operations opp, long ProjectId)
        {
            string mysql = @" SELECT        ConsServicesGroupList.*, ConsProject.Name AS ProjectName
FROM            ConsServicesGroupList INNER JOIN
                         ConsProject ON ConsServicesGroupList.ProjectId = ConsProject.Id ";
            mysql = mysql + @" WHERE(ConsServicesGroupList.Id> 0) ";
            if (ProjectId > 0)
            {
                mysql = mysql + @"  And (ConsServicesGroupList.ProjectId = " + ProjectId + ")   ";
            }

            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }

        public void InsertServicesGroup(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsServicesGroupList (ProjectId ,Name ) ";
            mysql = mysql + " VALUES ( " + ProjectId + " ,  N'" + Name + "')";

            Id = (int)opp.Insert_me_Return_Last_Identify(mysql);

        }
        public void InsertServicesGroupAll(ref operations opp)
        {
            InsertServicesGroup(ref opp);
            string mysql = @"INSERT INTO ConsServicesGroups(ServicesGroupListId, ServicesId) ";
            mysql = mysql + " SELECT " + Id + " AS ServicesGroupListId, ServiceId FROM ConsProjectServices ";
            mysql = mysql + " WHERE        (ProjectId = " + ProjectId + ") ";


            opp.insert_me(mysql);

        }
        public void InsertServicesGroupUnitesAll(ref operations opp,long ProjectUnitesId)
        {
            
            InsertServicesGroup(ref opp);
            string mysql = @" INSERT INTO ConsServicesUnites (ProjectUnitesId, ServicesId, ServicesGroupListId)
                            SELECT ConsProjectUnites.Id, ConsServicesGroups.ServicesId, ConsProjectUnites.ServicesGroupId
                            FROM ConsServicesGroups INNER JOIN
                            ConsProjectUnites ON ConsServicesGroups.ServicesGroupListId = ConsProjectUnites.ServicesGroupId
                            WHERE (ConsProjectUnites.Id =  " + ProjectUnitesId + ") ";
             
            opp.insert_me(mysql);

        }
        public void UpdateServicesGroup(ref operations opp)
        {

            string mysql = @"Update ConsServicesGroupList Set ProjectId=" + ProjectId + " ,Name=N'" + Name + "'";
            mysql = mysql + " Where Id=" + Id;

            opp.insert_me(mysql);

        }
        public void DeleteServicesGroup(ref operations opp)
        {

            string mysql = @"Delete From ConsServicesGroupList   ";
            mysql = mysql + " Where Id=" + Id;
            opp.insert_me(mysql);

        }
        public void DeleteConsServicesUnites(ref operations opp,long Id)
        {

            string mysql = @"Delete From ConsServicesUnites   ";
            mysql = mysql + " Where Id=" + Id;
            opp.insert_me(mysql);

        }
        public void DeleteConsServicesUnites(ref operations opp, long ProjectUnitesId, long ServicesGroupListId)
        {

            string mysql = @" Delete From ConsServicesUnites   ";
            mysql = mysql + " Where ProjectUnitesId="+ ProjectUnitesId + " AND   ServicesGroupListId=" + ServicesGroupListId;
            opp.insert_me(mysql);

        }
    }
}
