using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class CustomerOwner
    {
        public CustomerOwner()
        {

        }
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public int ProjectSpacesId { get; set; } = 0;


        public DataTable Get_CustomerOwner()
        {
            operations opp = new operations();
            return Get_CustomerOwner(ref opp);
        }
        public DataTable Get_CustomerOwner(ref operations opp)
        {
            string mysql = " Select * from ConsCustomerOwner  ";
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }

        public DataTable GetCustomerOwnerFull(ref operations opp, long CustomerId, long ProjectId, int RealStateTypeId)
        {
            string mysql = @" SELECT        ConsCustomerOwner.Id, ConsCustomerOwner.CustomerId, ConsCustomerOwner.ProjectSpacesId, ConsProject.Name AS ProjectName, ConsUnitType.Name AS RealStateTypeName, Customer.Name AS CustomerName, 
                         ConsProjectUnites.[Size]
FROM            Customer INNER JOIN
                         ConsCustomerOwner ON Customer.Id = ConsCustomerOwner.CustomerId INNER JOIN
                         ConsProjectUnites ON ConsCustomerOwner.ProjectSpacesId = ConsProjectUnites.Id INNER JOIN
                         ConsProject ON ConsProjectUnites.ProjectId = ConsProject.Id INNER JOIN
                         ConsUnitType ON ConsProjectUnites.RealStateTypeId = ConsUnitType.Id ";
            mysql = mysql + @" WHERE(ConsUnitType.Id> 0) ";
            if (CustomerId > 0)
            {
                mysql = mysql + @"  And (ConsCustomerOwner.CustomerId = " + CustomerId + ")   ";
            }
            if (ProjectId > 0)
            {

                mysql = mysql + @"  And (ConsProjectUnites.ProjectId = " + ProjectId + ")   ";
            }
            if (RealStateTypeId > 0)
            {

                mysql = mysql + @"  And (ConsProjectUnites.RealStateTypeId = " + RealStateTypeId + ")   ";
            }

            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }

        public void InsertCustomerOwner(ref operations opp)
        {

            string mysql = @"INSERT INTO ConsCustomerOwner(  ProjectSpacesId, CustomerId) ";
            mysql = mysql + " VALUES (   " + ProjectSpacesId + "  ," + CustomerId + " )";

            opp.insert_me(mysql);

        }

        public void UpdateCustomerOwner(ref operations opp)
        {

            string mysql = @"Update  ConsCustomerOwner Set   ProjectSpacesId=" + ProjectSpacesId + " , CustomerId=" + CustomerId + " ";
            mysql = mysql + " Where Id=" + Id;

            opp.insert_me(mysql);

        }
        public void DeleteCustomerOwner(ref operations opp)
        {

            string mysql = @"Delete From ConsCustomerOwner   ";
            mysql = mysql + " Where Id=" + Id;
            opp.insert_me(mysql);

        }
    }
}
