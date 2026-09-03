using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib.Models
{
    public class Account_Type_Main_Account_IdCls
    {

        public Account_Type_Main_Account_Id atmi = new Account_Type_Main_Account_Id();
        public DataTable Get_ProjectServices()
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return Get_ProjectServices(ref opp);
        }
        public DataTable Get_ProjectServices(ref AccountModelsLib.operations opp, long Id = 0)
        {
            string mysql = " Select * from ConsProjectServices  ";
            if (Id > 0)
            {
                mysql = mysql + " Where Id=" + Id;
            }
            DataTable dt = opp.return_DataTable(mysql);

            return dt;
        }
        public void GetProjectServicesById(ref AccountModelsLib.operations opp, long Id)
        {

            DataTable dt = Get_ProjectServices(ref opp, Id);
            ReadData(dt, 0);


        }
        public void ReadData(DataTable dt, int index)
        {
            atmi.Id = long.Parse(dt.Rows[index]["Id"].ToString());
            atmi.Account_Type_Value = int.Parse(dt.Rows[index]["ProjectId"].ToString());
            atmi.Main_Account_Id = int.Parse(dt.Rows[index]["ServiceId"].ToString());
            atmi.User_Id = int.Parse(dt.Rows[index]["User_Id"].ToString());
            atmi.Account_Branch_Id = int.Parse(dt.Rows[index]["Account_Branch_Id"].ToString());

        }
        public DataTable GetProjectServicesFull(ref AccountModelsLib.operations opp, long ProjectId, long ServiceId)
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

        //public void InsertProjects(ref AccountModelsLib.operations opp)
        //{

        //    string mysql = @"INSERT INTO ConsProjectServices (ProjectId ,ServiceId ,Price,SubAccount_Id) ";
        //    mysql = mysql + " VALUES ( " + Account_Type_Value + " ,  " + Main_Account_Id + "  ," + User_Id + " ," + Account_Branch_Id + ")";

        //    opp.insert_me(mysql);

        //}

        //public void UpdateProjects(ref AccountModelsLib.operations opp)
        //{

        //    string mysql = @"Update ConsProjectServices Set ProjectId=" + Account_Type_Value + " ,ServiceId=" + Main_Account_Id + "  ,Price=" + User_Id + " ,SubAccount_Id=" + Account_Branch_Id + "";
        //    mysql = mysql + " Where Id=" + Id;

        //    opp.insert_me(mysql);

        //}
        //public void DeleteProjects(ref AccountModelsLib.operations opp)
        //{

        //    string mysql = @"Delete From ConsProjectServices   ";
        //    mysql = mysql + " Where Id=" + Id;
        //    opp.insert_me(mysql);

        //}
    }
}
