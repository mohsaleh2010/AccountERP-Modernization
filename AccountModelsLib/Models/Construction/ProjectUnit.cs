using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class ProjectUnit
    {
        public ProjectUnit()
        {

        }
        public long Id { get; set; } = 0;
        public long ProjectId { get; set; }
        public long ResidentialbuildingId { get; set; }
        public string ResidentialbuildingName { get; set; }
        public string UnitNo { get; set; }
        public string UnitCode { get; set; }

        public long ProjectSpacesGroupId { get; set; }
        public long ServicesGroupId { get; set; }
        public decimal Size { get; set; } = 0;
        public long WaterCounterTypeId { get; set; }
        public string WaterCounterNo { get; set; }

        public long UnitTypeId { get; set; } = 0;
        public string Notes { get; set; }
        public long CustomerId { get; set; }
        public long SubAccount_Id { get; set; }
        public int IsRelatedCustomer { get; set; }

        public DataTable Get_ProjectUnites()
        {
            operations opp = new operations();
            return Get_ProjectUnites(ref opp);
        }
        public DataTable Get_ProjectUnites(ref operations opp, long Id = 0)
        {
            string mysql = " Select * from ConsProjectUnites  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void GetProjectUnitesById(ref operations opp, long Id)
        {

            DataTable dt = Get_ProjectUnites(ref opp, Id);
            ReadData(dt, 0);


        }
        public void ReadData(DataTable dt, int index)
        {
            Id = long.Parse(dt.Rows[index]["Id"].ToString());
            ProjectId = int.Parse(dt.Rows[index]["ProjectId"].ToString());
            ResidentialbuildingId = int.Parse(dt.Rows[index]["ResidentialbuildingId"].ToString());
            ResidentialbuildingName = dt.Rows[index]["ResidentialbuildingName"].ToString();
            UnitNo = dt.Rows[index]["UnitNo"].ToString();
            UnitCode = dt.Rows[index]["UnitCode"].ToString();

            ProjectSpacesGroupId = int.Parse(dt.Rows[index]["ProjectSpacesGroupId"].ToString());
            ServicesGroupId = int.Parse(dt.Rows[index]["ServicesGroupId"].ToString());
            Size = decimal.Parse(dt.Rows[index]["Size"].ToString());
            WaterCounterTypeId = int.Parse(dt.Rows[index]["WaterCounterTypeId"].ToString());
            WaterCounterNo = dt.Rows[index]["ResidentialbuildingId"].ToString();

            UnitTypeId = int.Parse(dt.Rows[index]["UnitTypeId"].ToString());
            Notes = dt.Rows[index]["Notes"].ToString();
            CustomerId = long.Parse(dt.Rows[index]["CustomerId"].ToString());
            SubAccount_Id = long.Parse(dt.Rows[index]["SubAccount_Id"].ToString());
            IsRelatedCustomer = int.Parse(dt.Rows[index]["IsRelatedCustomer"].ToString());
        }
        public DataTable GetProjectUnitesFull(ref operations opp, long ProjectId, long ResidentialbuildingId)
        {
            string mysql = @" SELECT        ConsProject.Name AS ProjectName, ConsProjectUnites.*
FROM            ConsProjectUnites INNER JOIN
                         ConsProject ON ConsProjectUnites.ProjectId = ConsProject.Id ";
            mysql = mysql + @" WHERE(ConsProjectUnites.Id> 0) ";
            if (ProjectId > 0)
            {
                mysql = mysql + @"  And (ConsProjectUnites.ProjectId = " + ProjectId + ")   ";
            }

            if (ResidentialbuildingId > 0)
            {
                mysql = mysql + @"  And (ConsProjectUnites.ResidentialbuildingId  = " + ResidentialbuildingId + ")   ";
            }
            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }
        public DataTable GetProjectUnitesFull(long ProjectId, long ResidentialbuildingId, long ServicesGroupId, long ProjectSpacesGroupId)
        {
            operations opp = new operations();
            return GetProjectUnitesFull(ref opp, ProjectId, ResidentialbuildingId, ServicesGroupId, ProjectSpacesGroupId);
        }
        public DataTable GetProjectUnitesFull(ref operations opp, long ProjectId, long ResidentialbuildingId, long ServicesGroupId, long ProjectSpacesGroupId)
        {
            string mysql = @" SELECT        ConsProject.Name AS ProjectName, Customer.Name AS CustomerName, ConsProjectUnites.Id, ConsProjectUnites.ProjectId, ConsProjectUnites.ResidentialbuildingId, ConsProjectUnites.ResidentialbuildingName, 
                         ConsProjectUnites.UnitNo, ConsProjectUnites.UnitCode, ConsProjectUnites.UnitTypeId, ConsProjectUnites.ProjectSpacesGroupId, ConsProjectUnites.ServicesGroupId, ConsProjectUnites.[Size], 
                         ConsProjectUnites.WaterCounterTypeId, ConsProjectUnites.Notes, ConsProjectUnites.WaterCounterNo, ConsProjectUnites.CustomerId, ConsProjectUnites.IsRelatedCustomer, 
                         ConsProjectUnites.SubAccount_Id AS SubAccount_Id_ProjectUnites, Customer.SubAccount_Id AS SubAccount_Id_Customer
FROM            ConsProjectUnites INNER JOIN
                         ConsProject ON ConsProjectUnites.ProjectId = ConsProject.Id INNER JOIN
                         Customer ON ConsProjectUnites.CustomerId = Customer.Id  ";
            mysql = mysql + @" WHERE(ConsProjectUnites.Id> 0) ";
            if (ProjectId > 0)
            {
                mysql = mysql + @"  And (ConsProjectUnites.ProjectId = " + ProjectId + ")   ";
            }

            if (ResidentialbuildingId > 0)
            {
                mysql = mysql + @"  And (ConsProjectUnites.ResidentialbuildingId  = " + ResidentialbuildingId + ")   ";
            }
            if (ServicesGroupId > 0)
            {
                mysql = mysql + @"  And (ConsProjectUnites.ServicesGroupId  = " + ServicesGroupId + ")   ";
            }
            if (ProjectSpacesGroupId > 0)
            {
                mysql = mysql + @"  And (ConsProjectUnites.ProjectSpacesGroupId  = " + ProjectSpacesGroupId + ")   ";
            }
            mysql = mysql + @"   ORDER BY ConsProjectUnites.[Size] ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }
        public string GetUnitCode()
        {
            string UnitCode = ResidentialbuildingName + " " + UnitNo;
            return UnitCode;
        }
        public void InsertProjectUnites(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsProjectUnites(ProjectId, ResidentialbuildingId, ResidentialbuildingName, UnitNo, UnitCode, UnitTypeId, ServicesGroupId,ProjectSpacesGroupId, [Size], WaterCounterTypeId,  WaterCounterNo, CustomerId, SubAccount_Id,IsRelatedCustomer,Notes )";
            mysql = mysql + " VALUES (  " + ProjectId + " , " + ResidentialbuildingId + " ,  '" + ResidentialbuildingName + "'  ,  '" + UnitNo + "',  '" + UnitCode + "', " + UnitTypeId + " , " + ServicesGroupId + " ," + ProjectSpacesGroupId + " , " + Size + "  , " + WaterCounterTypeId + ",  '" + WaterCounterNo + "' , " + CustomerId + ", " + SubAccount_Id + " , " + IsRelatedCustomer + ", '" + global::AccountModelsLib.SqlText.Q(Notes) + "')";

            Id = opp.Insert_me_Return_Last_Identify(mysql);

        }

        public void UpdateProjectUnites(ref operations opp)
        {

            string mysql = @"Update    ConsProjectUnites Set ProjectId=  " + ProjectId + " , ResidentialbuildingId= " + ResidentialbuildingId + "  , ResidentialbuildingName= '" + ResidentialbuildingName + "'  , UnitNo=  '" + UnitNo + "' , UnitCode='" + UnitCode + "'  , UnitTypeId= " + UnitTypeId + "  ,ServicesGroupId=" + ServicesGroupId + ", ProjectSpacesGroupId=  " + ProjectSpacesGroupId + " , [Size]=" + Size + "  , WaterCounterTypeId= " + WaterCounterTypeId + " ,  WaterCounterNo= '" + WaterCounterNo + "'   , CustomerId=" + CustomerId + "  , SubAccount_Id=" + SubAccount_Id + " ,IsRelatedCustomer=" + IsRelatedCustomer + "  ,Notes ='" + global::AccountModelsLib.SqlText.Q(Notes) + "' ";
            mysql = mysql + " Where Id=" + Id;

            opp.insert_me(mysql);

        }
        public void DeleteProjectUnites(ref operations opp)
        {

            string mysql = @"Delete From ConsProjectUnites   ";
            mysql = mysql + " Where Id=" + Id;
            opp.insert_me(mysql);

        }
    }
}
