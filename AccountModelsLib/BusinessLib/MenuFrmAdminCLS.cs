using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
  
using Microsoft.EntityFrameworkCore; 
   
 
using AccountModelsLib.Models;
 
namespace AccountModelsLib
{
   
    public class MenuFrmAdminCLS
    {
        public MenuFrmAdminCLS()
        {

        }

        public DataTable Get_SysConfigMenusPERMATIONS_Main(string  PERMATION, int PROJECT_ID, long USER_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @" SELECT SysConfigMenusPERMATIONS.*, SysConfigMenus.KeysData, SysConfigMenus.Description
                                FROM SysConfigMenusPERMATIONS INNER JOIN
                            SysConfigMenus ON SysConfigMenusPERMATIONS.Menu_ID = SysConfigMenus.Id ";

            mysql = mysql + " Where SysConfigMenusPERMATIONS.Parent_Id=0 And SysConfigMenusPERMATIONS.PERMATION=N'" + PERMATION + "' And SysConfigMenusPERMATIONS.PROJECT_ID=" + PROJECT_ID + " And SysConfigMenusPERMATIONS.USER_ID=" + USER_ID + " ";

            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_SysConfigMenusPERMATIONS_Main_Services(  long USER_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @" SELECT        tblServices.*
FROM            SysConfigServicesPERMATIONS INNER JOIN
                         tblServices ON SysConfigServicesPERMATIONS.Services_ID = tblServices.ID
WHERE        (SysConfigServicesPERMATIONS.USER_ID = "+ USER_ID + ") ";

            
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }

        public DataTable Get_SysConfigMenusNOTPERMATIONS_MainServices(  long USER_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @" SELECT        tblServices.*
FROM            tblServices
WHERE        (ID NOT IN
                             (SELECT        Services_ID
                                FROM            SysConfigServicesPERMATIONS
                                WHERE        (USER_ID = "+ USER_ID + "))) ";

            
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_SysConfigMenusPERMATIONS_Sub(string PERMATION, long MainMenu_ID, long USER_ID,bool IsPermission=true)
        {  
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @" SELECT SysConfigMenusPERMATIONS.*, SysConfigMenus.KeysData, SysConfigMenus.Description
                                FROM SysConfigMenusPERMATIONS INNER JOIN
                         SysConfigMenus ON SysConfigMenusPERMATIONS.Menu_ID = SysConfigMenus.Id ";
            mysql = mysql + "  Where     SysConfigMenusPERMATIONS.Parent_Id=" + MainMenu_ID + " And SysConfigMenusPERMATIONS.USER_ID=" + USER_ID + " ";

            
            if(IsPermission==true)
            {
                mysql = mysql + "    And  SysConfigMenusPERMATIONS.PERMATION=N'" + PERMATION + "'";

            }
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_SysConfigMenusPERMATIONS_PermittesSubServices(  long Services_ID, long USER_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @" SELECT        tblSubServices.*
FROM            SysConfigServicesPERMATIONS INNER JOIN
                         tblSubServices ON SysConfigServicesPERMATIONS.SubServices_Id = tblSubServices.SSID
WHERE        (tblSubServices.SSParent  = " + Services_ID + ") AND (SysConfigServicesPERMATIONS.USER_ID = "+ USER_ID + ") ";
            
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_SysConfigMenusPERMATIONS_SubServices(  long SubServices_ID, long USER_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @" SELECT        SSID, SSName, SSName_Ar, Description, SSRate, SSParent, Account_IdMasrofat, Account_IdEradat
FROM            tblSubServices
WHERE        (SSID NOT IN
                             (SELECT        SubServices_Id
                                FROM            SysConfigServicesPERMATIONS
                                WHERE        (USER_ID = "+ USER_ID + "))) AND (SSParent = "+ SubServices_ID + ") ";
            
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }
        public DataTable Get_SysConfigMenusPERMATIONS_Sub_(string PERMATION, long MainMenu_ID, long USER_ID)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @"  SELECT        KeysData, Id, Parent_Id, Description

FROM            SysConfigMenus
WHERE      SysConfigMenus.Parent_Id=" + MainMenu_ID + " ";
            mysql = mysql + @" And   (Id NOT IN (SELECT        Menu_ID
                                FROM            SysConfigMenusPERMATIONS
                                WHERE        (USER_ID = " + USER_ID + " ) AND (PROJECT_ID = 2))) ";
 
            
            DataTable dt = opp.return_DataTable(mysql);
            return dt;
        }

        public void Update_SysConfigMenusPERMATIONS_Sub(long ID, string PERMATION)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = @"   Update  SysConfigMenusPERMATIONS Set PERMATION =N'" + PERMATION + "' Where Id=" + ID + "";
            opp.insert_me(mysql);

        }

        public void Update_SysConfigMenusPERMATIONS_Sub(string mysql)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            int c=opp.insert_me(mysql);

        }
        public void Update_SysConfigMenusPERMATIONS_Sub(ref string mysql, long ID, string PERMATION)
        {

            mysql = mysql + @"   Update  SysConfigMenusPERMATIONS Set PERMATION =N'" + PERMATION + "' Where Id=" + ID + " ";


        }
        public void Update_SysConfigMenusPERMATIONS_Sub_OtherUser(ref string mysql, long ID,ArrayList arrOtherUsers, string PERMATION)
        {
            if(arrOtherUsers.Count==0)
            {
                return;
            }
            string msql= "Select * from SysConfigMenusPERMATIONS Where Id=" + ID + " ";
            AccountModelsLib.operations opp=new AccountModelsLib.operations();
            DataTable dt=opp.return_DataTable(msql);
            if(dt.Rows.Count>0)
            {
                string Menu_ID = dt.Rows[0]["Menu_ID"].ToString();

                for(int x=0;x<arrOtherUsers.Count;x++)
                {
                    mysql = mysql + @"   Update  SysConfigMenusPERMATIONS Set PERMATION =N'" + PERMATION + "' Where Menu_ID=" + Menu_ID + " And USER_ID="+ arrOtherUsers[x] + "";

                }
            }
            

        }

        public void Add_SysConfigMenusPERMATIONS_SubServices(ref string mysql, long USER_ID, long   SubServices_ID,int PROJECT_ID)
        {

             mysql = mysql + @" INSERT INTO SysConfigServicesPERMATIONS (SubServices_Id,Services_Id, USER_ID, PROJECT_ID) ";
            mysql = mysql + " VALUES        (" + SubServices_ID + " ,0," + USER_ID + " ," + PROJECT_ID + ")";


        }
        public void Delete_SysConfigMenusPERMATIONS_Service(ref string mysql, long UserId, long Services_ID, int projectId )
        {

            mysql = mysql + @"   Delete from   SysConfigServicesPERMATIONS   WHERE (Services_ID = "+ Services_ID + ") AND (USER_ID =  " + UserId + ") AND (PROJECT_ID =  " + projectId + ")";


        }

        public void Delete_SysConfigMenusPERMATIONS_SubService(ref string mysql, long UserId, long SubServices_ID, int projectId)
        {

            mysql = mysql + @"   Delete from   SysConfigServicesPERMATIONS   WHERE (SubServices_ID = " + SubServices_ID + ") AND (USER_ID =  " + UserId + ") AND (PROJECT_ID =  " + projectId + ")";


        }
        public void Update_SysConfigMenusPERMATIONS_Services(ref string mysql, long USER_ID, long Services_ID,int PROJECT_ID)
        {

            mysql = mysql + @" INSERT INTO SysConfigServicesPERMATIONS (Services_ID, USER_ID, PROJECT_ID) ";
            mysql = mysql + " VALUES        (" + Services_ID +" ," + USER_ID + " ," + PROJECT_ID+")";


        }
        public void Delete_PatientClinicCLS(ref int Excp, long Id)
        {

            string mysql = " ";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();

            try
            {
                mysql = @" Delete From ClinicDrugs Where Id=" + Id + "";

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception exceptionObj)
            {
                //MessageBox.Show(exceptionObj.Message.ToString());
            }

        }
    }
}
