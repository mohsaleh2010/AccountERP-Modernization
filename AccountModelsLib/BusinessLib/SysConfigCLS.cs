
using AccountModelsLib;

using AccountModelsLib.Models;
using Connection4It;


using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AccountModelsLib
{

    public static class SqlStringExtensions
    {
        /// <summary>
        /// Escapes single quotes for raw T-SQL strings by replacing ' with ''
        /// </summary>
        public static string ToSqlSafe(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Replace("'", "''");
        }
    }
    public class SysConfigCLS
    {
        public SysConfigCLS()
        {
        }
        public DataTable Get_SysConfigList(ref AccountModelsLib.operations opp, string KeysData)
        {
            opp = new AccountModelsLib.operations();
            string mysql = "Select * from SysConfig ";
            if (KeysData.Trim() != "")
            {
                mysql = mysql + " Where KeysData='" + KeysData + "'";
            }
            mysql = mysql + "  Order By KeysData Asc  ";
            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }
        public DataTable Get_SysConfigMenus(ref AccountModelsLib.operations opp, string KeysData)
        {
            opp = new AccountModelsLib.operations();
            string mysql = "Select * from SysConfigMenus   ";
            if (KeysData.Trim() != "")
            {
                mysql = mysql + " Where KeysData='" + KeysData + "'";
            }
            mysql = mysql + "  Order By KeysData Asc  ";
            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }

        public DataTable Get_SysConfigMenus_List(ref AccountModelsLib.operations opp)
        {
            opp = new AccountModelsLib.operations();
            string mysql = "Select * from SysConfigMenus            Order By Parent_Id  ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }
        public DataTable Get_SysConfigMenus_List(long Id)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            string mysql = "Select * from SysConfigMenus       Where Id=" + Id + "     Order By Parent_Id  ";

            DataTable dt = opp.return_DataTableWithBuilder(mysql);

            return dt;
        }


        public void Set_ProjectSetting(ref AccountModelsLib.operations opp, ref string message, string ColumnName)
        {

            string mysql = "UPDATE SysConfig SET Value = " + ColumnName + ", DefaultValue = " + ColumnName + "";

            opp.insert_me(mysql);


            ///////////////////////////////////////////
            message = message + "/n/r";
            message = message + " UPDATE SysConfig ";
            ////////////////////////////////////////////
            ///



        }

        public void Set_TruncateUSER_DETAILES(ref AccountModelsLib.operations opp)
        {
            string mysql = "Truncate Table USER_DETAILES";
            opp.insert_me(mysql);

            mysql = "Truncate Table SysConfigMenusPERMATIONS";
            opp.insert_me(mysql);

        }

        public void Set_TruncateUSERS(ref AccountModelsLib.operations opp)
        {
            string mysql = "Truncate Table USER_DETAILES";
            opp.insert_me(mysql);



        }
        public void Set_TruncateMenuPermission(ref AccountModelsLib.operations opp)
        {


            string mysql = "Truncate Table SysConfigMenusPERMATIONS";
            opp.insert_me(mysql);

        }
        public void Set_ProjectMenu(ref AccountModelsLib.operations opp, ref string message, string ColumnName)
        {
            string mysql = "UPDATE SysConfigMenus SET Value = " + ColumnName + ", DefaultValue = " + ColumnName + "";

            opp.insert_me(mysql);


            ///////////////////////////////////////////
            message = message + "/n/r";
            message = message + " UPDATE SysConfigMenus ";
            ////////////////////////////////////////////

        }


        public bool Is_Admin = true;
        public void Set_User_Permission(ref AccountModelsLib.operations opp, ref string message, int PROJECT_ID, string USER_NAME, string USER_PASSWORD, int AdminFlage, int Active, int Account_Proj, int HR_Proj, int Store_Proj, int Production_Proj, int Gawda_Proj)
        {
            string mysql = "";
            long USER_Id = 0;
            mysql = "Select * from USER_DETAILES Where USER_NAME='" + USER_NAME + "'";
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                USER_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                mysql = "Update    USER_DETAILES Set USER_NAME='" + USER_NAME + "'" +
                    ", USER_PASSWORD='" + USER_PASSWORD + "', Branch_Id=" + Common.Branch_Id + ", AdminFlage=" + AdminFlage + ",Active=" + Active + ", Account_Proj=" + Account_Proj + ", HR_Proj=" + HR_Proj + ", Store_Proj=" + Store_Proj + ", Production_Proj=" + Production_Proj + ", Gawda_Proj=" + Gawda_Proj + " Where Id= " + USER_Id + " ";

                opp.insert_me(mysql);
            }

            else
            {
                // USER_DETAILES.Id لم يعد Identity بعد تحويل USER_ID إلى Id - يُولَّد المعرّف هنا كباقى الجداول
                USER_Id = CallSnowflakeIdGenerate.GenerateNewId();
                mysql = "  INSERT INTO USER_DETAILES (Id, USER_NAME, USER_PASSWORD, Branch_Id, AdminFlage,Active, Account_Proj, HR_Proj, Store_Proj, Production_Proj, Gawda_Proj) ";
                mysql = mysql + " values ( " + USER_Id + ", '" + USER_NAME + "','" + USER_PASSWORD + "'," + Common.Branch_Id + "," + AdminFlage + "," + Active + "," + Account_Proj + "," + HR_Proj + "," + Store_Proj + "," + Production_Proj + "," + Gawda_Proj + ") ";
                opp.insert_me(mysql);
            }

            ///////////////////////////////////////////
            message = message + "/n/r";
            message = message + " User Added : Admin ";
            ////////////////////////////////////////////


            if (Is_Admin == true)
            {
                mysql = "INSERT INTO SysConfigMenusPERMATIONS (Parent_Id,    PERMATION, PROJECT_ID, Menu_ID, USER_ID)";
                mysql = mysql + " SELECT Parent_Id,   Value, PROJECT_ID, Id, " + USER_Id + " AS USER_ID FROM SysConfigMenus";// Where PROJECT_ID="+ PROJECT_ID + "";
                opp.insert_me(mysql);

                ///////////////////////////////////////////
                message = message + "/n/r";
                message = message + "  Added Permission  : Admin ";
                ////////////////////////////////////////////
                ///
            }
        }
        public void Set_NewUser(ref AccountModelsLib.operations opp, ref string message, int PROJECT_ID, string USER_NAME, string USER_PASSWORD, int AdminFlage, int Active, int Account_Proj, int HR_Proj, int Store_Proj, int Production_Proj, int Gawda_Proj)
        {
            string mysql = "";
            long USER_Id = 0;
            mysql = "Select * from USER_DETAILES Where USER_NAME='" + USER_NAME + "'";
            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count > 0)
            {
                USER_Id = long.Parse(dt.Rows[0]["Id"].ToString());
                mysql = "Update    USER_DETAILES Set USER_NAME='" + USER_NAME + "'" +
                    ", USER_PASSWORD='" + USER_PASSWORD + "', Branch_Id=" + Common.Branch_Id + ", AdminFlage=" + AdminFlage + ",Active=" + Active + ", Account_Proj=" + Account_Proj + ", HR_Proj=" + HR_Proj + ", Store_Proj=" + Store_Proj + ", Production_Proj=" + Production_Proj + ", Gawda_Proj=" + Gawda_Proj + " Where Id= " + USER_Id + " ";

                opp.insert_me(mysql);
            }

            else
            {
                // USER_DETAILES.Id لم يعد Identity بعد تحويل USER_ID إلى Id - يُولَّد المعرّف هنا كباقى الجداول
                USER_Id = CallSnowflakeIdGenerate.GenerateNewId();
                mysql = "  INSERT INTO USER_DETAILES (Id, USER_NAME, USER_PASSWORD, Branch_Id, AdminFlage,Active, Account_Proj, HR_Proj, Store_Proj, Production_Proj, Gawda_Proj) ";
                mysql = mysql + " values ( " + USER_Id + ", '" + USER_NAME + "','" + USER_PASSWORD + "'," + Common.Branch_Id + "," + AdminFlage + "," + Active + "," + Account_Proj + "," + HR_Proj + "," + Store_Proj + "," + Production_Proj + "," + Gawda_Proj + ") ";
                opp.insert_me(mysql);
            }

            ///////////////////////////////////////////
            message = message + "/n/r";
            message = message + " User Added : Admin ";
            ////////////////////////////////////////////



            //mysql = "INSERT INTO SysConfigMenusPERMATIONS (Parent_Id,    PERMATION, PROJECT_ID, Menu_ID, USER_ID)";
            //mysql = mysql + " SELECT Parent_Id,   Value, PROJECT_ID, Id, " + USER_Id + " AS USER_ID FROM SysConfigMenus";// Where PROJECT_ID="+ PROJECT_ID + "";
            //opp.insert_me(mysql);

            /////////////////////////////////////////////
            //message = message + "/n/r";
            //message = message + "  Added Permission  : Admin ";
            ////////////////////////////////////////////
            ///

        }
        public void Set_User_Permission(ref AccountModelsLib.operations opp, ref string message)
        {
            string mysql = "";
            long USER_Id = 0;
            mysql = "Select * from USER_DETAILES  ";
            DataTable dt = opp.return_DataTable(mysql);
            mysql = "";
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                USER_Id = long.Parse(dt.Rows[x]["Id"].ToString());
                mysql = mysql + " INSERT INTO SysConfigMenusPERMATIONS (Parent_Id,    PERMATION, PROJECT_ID, Menu_ID, USER_ID)";
                mysql = mysql + " SELECT Parent_Id,   Value, PROJECT_ID, Id, " + USER_Id + " AS USER_ID FROM SysConfigMenus";// Where PROJECT_ID="+ PROJECT_ID + "";

                try
                {
                    opp.insert_me(mysql);
                    mysql = "";
                }
                catch (Exception ee) { mysql = ""; }
            }








            ///////////////////////////////////////////
            message = message + "/n/r";
            message = message + "  Added Permission  : Admin ";
            ////////////////////////////////////////////
            ///

        }

       
        public void SetUpdateMenusPermisionAddedNew()
        {
            EncryptProcess en = new EncryptProcess();
            string PERMATION_Ok = en.encrypt("1");
            string PERMATION_Not = en.encrypt("0");
            string PERMATION = "";
            string mysql = "";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            try
            {
                mysql = "select * from USER_DETAILES";
                DataTable dtUsers = opp.return_DataTable(mysql);
                ////////////////////
                mysql = "Select * from SysConfigMenus";
                DataTable dtMenus = opp.return_DataTable(mysql);

                for (int y = 0; y < dtUsers.Rows.Count; y++)
                {
                    long USER_ID = long.Parse(dtUsers.Rows[y]["Id"].ToString());
                    for (int x = 0; x < dtMenus.Rows.Count; x++)
                    {
                        string Parent_Id = dtMenus.Rows[x]["Parent_Id"].ToString();

                        string Menu_ID = dtMenus.Rows[x]["ID"].ToString();
                        if(Menu_ID== "963")
                        {
                            int xxxx = 0;
                        }
                        string PROJECT_ID = dtMenus.Rows[x]["PROJECT_ID"].ToString();
                        long Value = 0;
                        long.TryParse(en.decrypt(dtMenus.Rows[x]["Value"].ToString()), out Value);

                        mysql = "Select * from SysConfigMenusPERMATIONS Where USER_ID=" + USER_ID + " And Menu_ID=" + Menu_ID + "";
                        DataTable dtt = opp.return_DataTable(mysql);
                        if (dtt.Rows.Count > 0)
                        {
                            continue;
                        }
                        if (Value == 1)
                        {
                            PERMATION = PERMATION_Ok;

                        }
                        else
                        {
                            PERMATION = PERMATION_Not;
                        }

                        mysql = " INSERT INTO SysConfigMenusPERMATIONS (Parent_Id,    PERMATION, PROJECT_ID, Menu_ID, USER_ID)";
                        mysql = mysql + " Values(" + Parent_Id + ",N'" + PERMATION + "', " + PROJECT_ID + "," + Menu_ID + "," + USER_ID + ")";

                        try
                        {
                            opp.insert_me(mysql);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
            }

        }
        public void SetAllMenusAdded(long USER_ID, int PROJECT_ID)
        {
            EncryptProcess en = new EncryptProcess();
            string PERMATION = en.encrypt("1");

            string mysql = "";
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            try
            {
                mysql = "Delete  from SysConfigMenusPERMATIONS Where USER_ID=" + USER_ID + " And  PROJECT_ID=" + PROJECT_ID + "  ";
                opp.insert_me(mysql);

                //mysql = "select * from USER_DETAILES";
                //DataTable dtUsers = opp.return_DataTable(mysql);
                ////////////////////
                mysql = " Select * from SysConfigMenus Where PROJECT_ID=" + PROJECT_ID + "";
                DataTable dt = opp.return_DataTable(mysql);


                //long   User_Id = long.Parse(dtUsers.Rows[y]["USER_ID"].ToString());
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    string Parent_Id = dt.Rows[x]["Parent_Id"].ToString();

                    string Menu_ID = dt.Rows[x]["ID"].ToString();
                    //string PROJECT_ID = dt.Rows[x]["PROJECT_ID"].ToString();

                    mysql = "Select * from SysConfigMenusPERMATIONS Where USER_ID=" + USER_ID + " And Menu_ID=" + Menu_ID + "";
                    DataTable dtt = opp.return_DataTable(mysql);
                    if (dtt.Rows.Count > 0)
                    {
                        long Id = long.Parse(dtt.Rows[0]["Id"].ToString());
                        mysql = "Update  SysConfigMenusPERMATIONS Set      PERMATION=N'" + PERMATION + "' Where Id=" + Id;


                        opp.insert_me(mysql);
                    }
                    else
                    {
                        mysql = "INSERT INTO SysConfigMenusPERMATIONS (Parent_Id,    PERMATION, PROJECT_ID, Menu_ID, USER_ID)";
                        mysql = mysql + " Values(" + Parent_Id + ",N'" + PERMATION + "', " + PROJECT_ID + "," + Menu_ID + "," + USER_ID + ")";
                        try
                        {
                            opp.insert_me(mysql);
                        }
                        catch { }
                    }

                }
            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
            }

        }
        public void SetAllMenusAddedTheSamePermission(ref int Excp, ref string Msg, long USER_ID, int PROJECT_ID, ArrayList arrOtherUsers)
        {
            if (arrOtherUsers.Count == 0)
            { return; }

            try
            {
                EncryptProcess en = new EncryptProcess();
                string PERMATION_1 = en.encrypt("1");
                string PERMATION_0 = en.encrypt("0");

                string mysql = "";
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                //////The Main User Permission
                string msql = "Select * From SysConfigMenusPERMATIONS Where USER_ID=" + USER_ID + " And  PROJECT_ID=" + PROJECT_ID + "";
                DataTable dtMainPermission = opp.return_DataTable(msql);

                for (int x = 0; x < arrOtherUsers.Count; x++)
                {
                    ///Set All Permission 0 for selected Uers List
                    mysql = mysql + " Update    SysConfigMenusPERMATIONS Set PERMATION=N'" + PERMATION_0 + "' Where USER_ID=" + arrOtherUsers[x] + " And  PROJECT_ID=" + PROJECT_ID + "  ";

                    for (int y = 0; dtMainPermission.Rows.Count > y; y++)
                    {
                        mysql = mysql + " Update    SysConfigMenusPERMATIONS Set PERMATION=N'" + dtMainPermission.Rows[y]["PERMATION"].ToString() + "' Where USER_ID=" + arrOtherUsers[x] + " And  Menu_ID=" + dtMainPermission.Rows[y]["Menu_ID"].ToString() + "  ";

                    }
                }

                opp.insert_me(mysql);
                Excp = 0;
                Msg = "";


            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = ee.Message;
                //MessageBox.Show(ee.Message);
            }

        }
        public void Encrypt_Config_Menu(ref string msg)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigList(ref opp, "");

            DataRow[] dr = dt.Select("KeysData = 'Is_Ecrypted'");
            if (dr.Length == 0)
            {
                msg = "Data Already Encrypted";
                //MessageBox.Show("Data Already Encrypted");
                return;

            }
            Common.Is_Ecrypted_Config = 1;
            EncryptSysCofig(dt, ref opp);
            Encrypt_SysConfigMenus(ref opp);
            msg = "Data   Encrypted";
            //MessageBox.Show("Data   Encrypted");
        }
        public void Encrypt_Menu(ref string msg)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigMenus(ref opp, "");

            DataRow[] dr = dt.Select("KeysData = 'menuItemFile'");
            if (dr.Length == 0)
            {
                msg = "Data Already Encrypted";
                //MessageBox.Show("Data Already Encrypted");
                return;

            }
            Common.Is_Ecrypted_Menu = 1;
            //  EncryptSysCofig(dt, ref opp);
            Encrypt_SysConfigMenus(ref opp);
            msg = "Data   Encrypted";
            //MessageBox.Show("Data   Encrypted");
        }
        public void Encrypt_Config(ref string msg)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigList(ref opp, "");

            DataRow[] dr = dt.Select("KeysData = 'Is_Ecrypted'");
            if (dr.Length == 0)
            {
                msg = "Data Already Encrypted";
                //MessageBox.Show("Data Already Encrypted");
                return;

            }
            Common.Is_Ecrypted_Config = 1;
            EncryptSysCofig(dt, ref opp);
            //Encrypt_SysConfigMenus(ref opp);
            msg = "Data   Encrypted";
            //MessageBox.Show("Data   Encrypted");
        }


        public void Encrypt_Configuration(ref string msg)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigList(ref opp, "");

            DataRow[] dr = dt.Select("KeysData = 'Is_Ecrypted'");
            if (dr.Length == 0)
            {
                msg = "Data Already Encrypted";
                //MessageBox.Show("Data Already Encrypted");
                return;

            }
            Common.Is_Ecrypted_Config = 1;
            EncryptSysCofig(dt, ref opp);

            msg = "Data   Encrypted";
            //MessageBox.Show("Data   Encrypted");
        }
        void EncryptSysCofig(DataTable dt, ref AccountModelsLib.operations opp)
        {



            string mysql = "";
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                string KeysData = dt.Rows[x]["KeysData"].ToString(); // dataGridViewTrial.Rows[x].Cells["KeysData"].Value.ToString();
                string DefaultValue = dt.Rows[x]["DefaultValue"].ToString(); // dataGridViewTrial.Rows[x].Cells["DefaultValue"].Value.ToString();
                string Value = dt.Rows[x]["Value"].ToString(); // dataGridViewTrial.Rows[x].Cells["Value"].Value.ToString();
                string DefaultDescription = dt.Rows[x]["DefaultDescription"].ToString(); // dataGridViewTrial.Rows[x].Cells["DefaultDescription"].Value.ToString();
                string Description = dt.Rows[x]["Description"].ToString(); // dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();

                string POS = dt.Rows[x]["POS"].ToString(); // dataGridViewTrial.Rows[x].Cells["POS"].Value.ToString();
                string Market = dt.Rows[x]["Market"].ToString(); // dataGridViewTrial.Rows[x].Cells["Market"].Value.ToString();
                string Factory = dt.Rows[x]["Factory"].ToString(); // dataGridViewTrial.Rows[x].Cells["Factory"].Value.ToString();
                string Tourism = dt.Rows[x]["Tourism"].ToString(); // dataGridViewTrial.Rows[x].Cells["Tourism"].Value.ToString();
                string AlFawzan = dt.Rows[x]["AlFawzan"].ToString(); // dataGridViewTrial.Rows[x].Cells["Tourism"].Value.ToString();
                string Client_A = dt.Rows[x]["Client_A"].ToString(); // dataGridViewTrial.Rows[x].Cells["Client_A"].Value.ToString();
                string Client_B = dt.Rows[x]["Client_B"].ToString(); // dataGridViewTrial.Rows[x].Cells["Client_B"].Value.ToString();


                string ID = dt.Rows[x]["ID"].ToString(); // dataGridViewTrial.Rows[x].Cells["ID"].Value.ToString();

                if (KeysData == "Is_Ecrypted")
                {
                    Value = "1";

                }
                KeysData = Common.EncryptMe(KeysData);
                DefaultValue = Common.EncryptMe(DefaultValue);
                Value = Common.EncryptMe(Value);
                DefaultDescription = Common.EncryptMe(DefaultDescription);
                Description = Common.EncryptMe(Description);
                POS = Common.EncryptMe(POS);
                Market = Common.EncryptMe(Market);
                Factory = Common.EncryptMe(Factory);
                Tourism = Common.EncryptMe(Tourism);
                AlFawzan = Common.EncryptMe(AlFawzan);
                Client_A = Common.EncryptMe(Client_A);
                Client_B = Common.EncryptMe(Client_B);


                mysql = mysql + @" Update SysConfig Set KeysData = N'" + KeysData + "' ,  ";
                mysql = mysql + "  DefaultValue = N'" + DefaultValue + "' ,";
                mysql = mysql + " Value = N'" + Value + "' ,";
                mysql = mysql + " DefaultDescription = N'" + global::AccountModelsLib.SqlText.Q(DefaultDescription) + "' ,";
                mysql = mysql + " Description = N'" + global::AccountModelsLib.SqlText.Q(Description) + "',  ";

                mysql = mysql + " POS = N'" + POS + "' , ";
                mysql = mysql + " Market = N'" + Market + "' , ";
                mysql = mysql + " Factory = N'" + Factory + "' , ";
                mysql = mysql + " Tourism = N'" + Tourism + "' , ";
                mysql = mysql + " AlFawzan = N'" + AlFawzan + "' , ";
                mysql = mysql + " Client_A = N'" + Client_A + "' , ";
                mysql = mysql + " Client_B = N'" + Client_B + "'  ";
                mysql = mysql + " Where Id= " + ID + " ";





            }
            opp.insert_me(mysql);


        }

        private void Encrypt_SysConfigMenus(ref AccountModelsLib.operations opp)
        {
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigMenus_List(ref opp);
            string mysql = "";
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                string KeysData = dt.Rows[x]["KeysData"].ToString(); // dataGridViewTrial.Rows[x].Cells["KeysData"].Value.ToString();
                string DefaultValue = dt.Rows[x]["DefaultValue"].ToString(); // dataGridViewTrial.Rows[x].Cells["DefaultValue"].Value.ToString();
                string Value = dt.Rows[x]["Value"].ToString();  //dataGridViewTrial.Rows[x].Cells["Value"].Value.ToString();
                string DefaultDescription = dt.Rows[x]["DefaultDescription"].ToString(); // dataGridViewTrial.Rows[x].Cells["DefaultDescription"].Value.ToString();
                string Description = dt.Rows[x]["Description"].ToString();  //dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();
                //string Description_Ar = dt.Rows[x]["Description_Ar"].ToString();  //dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();
                string Description_En = dt.Rows[x]["Description_En"].ToString();  //dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();
                string Description_Fr = dt.Rows[x]["Description_Fr"].ToString();  //dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();
                string Description_DE = dt.Rows[x]["Description_DE"].ToString();  //dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();
                string RazorPath = dt.Rows[x]["RazorPath"].ToString();  //dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();




                string ID = dt.Rows[x]["ID"].ToString(); // dataGridViewTrial.Rows[x].Cells["ID"].Value.ToString();

                string POS = dt.Rows[x]["POS"].ToString(); // dataGridViewTrial.Rows[x].Cells["POS"].Value.ToString();
                string Market = dt.Rows[x]["Market"].ToString(); // dataGridViewTrial.Rows[x].Cells["Market"].Value.ToString();
                string Factory = dt.Rows[x]["Factory"].ToString(); // dataGridViewTrial.Rows[x].Cells["Factory"].Value.ToString();
                string Tourism = dt.Rows[x]["Tourism"].ToString(); // dataGridViewTrial.Rows[x].Cells["Tourism"].Value.ToString();
                string AlFawzan = dt.Rows[x]["AlFawzan"].ToString();//  dataGridViewTrial.Rows[x].Cells["Tourism"].Value.ToString();
                string Client_A = dt.Rows[x]["Client_A"].ToString(); // dataGridViewTrial.Rows[x].Cells["Client_A"].Value.ToString();
                string Client_B = dt.Rows[x]["Client_B"].ToString();  //dataGridViewTrial.Rows[x].Cells["Client_B"].Value.ToString();
                string UserSells = dt.Rows[x]["UserSells"].ToString(); // dataGridViewTrial.Rows[x].Cells["UserSells"].Value.ToString();
                //Description = AccountCommonDataLIB.Common.EncryptMe(Description);

                KeysData = Common.EncryptMe(KeysData);
                DefaultValue = Common.EncryptMe(DefaultValue);
                Value = Common.EncryptMe(Value);
                DefaultDescription = Common.EncryptMe(DefaultDescription);
                Description = Common.EncryptMe(Description);
                Description_En = Common.EncryptMe(Description_En);
                Description_Fr = Common.EncryptMe(Description_Fr);
                Description_DE = Common.EncryptMe(Description_DE);
                RazorPath = Common.EncryptMe(RazorPath);

                POS = Common.EncryptMe(POS);
                Market = Common.EncryptMe(Market);
                Factory = Common.EncryptMe(Factory);
                Tourism = Common.EncryptMe(Tourism);
                AlFawzan = Common.EncryptMe(AlFawzan);
                Client_A = Common.EncryptMe(Client_A);
                Client_B = Common.EncryptMe(Client_B);
                UserSells = Common.EncryptMe(UserSells);




                mysql = mysql + @" Update SysConfigMenus Set KeysData = N'" + KeysData + "' ,  ";
                mysql = mysql + "  DefaultValue = N'" + DefaultValue + "' ,";
                mysql = mysql + " Value = N'" + Value + "' ,";
                mysql = mysql + " DefaultDescription = N'" + global::AccountModelsLib.SqlText.Q(DefaultDescription) + "' ,";
                mysql = mysql + " Description = N'" + global::AccountModelsLib.SqlText.Q(Description) + "' , ";
                mysql = mysql + " Description_En = N'" + Description_En.ToSqlSafe() + "' , ";
                mysql = mysql + " Description_Fr = N'" + Description_Fr.ToSqlSafe() + "' , ";
                mysql = mysql + " Description_DE = N'" + Description_DE.ToSqlSafe() + "' , ";
                mysql = mysql + " RazorPath = N'" + RazorPath + "' , ";

                mysql = mysql + " POS = N'" + POS + "' , ";
                mysql = mysql + " Market = N'" + Market + "' , ";
                mysql = mysql + " Factory = N'" + Factory + "' , ";
                mysql = mysql + " Tourism = N'" + Tourism + "' , ";
                mysql = mysql + " AlFawzan = N'" + AlFawzan + "' , ";
                mysql = mysql + " Client_A = N'" + Client_A + "' , ";
                mysql = mysql + " Client_B = N'" + Client_B + "' , ";
                mysql = mysql + " UserSells = N'" + UserSells + "'  ";

                mysql = mysql + " Where Id= " + ID + " ";

                //try
                //{
                //    opp.insert_me(mysql);
                //}
                //catch (Exception ee)
                //{


                //}

            }
            opp.insert_me(mysql);

        }
        public void Decrypt_Config_Menu(ref AccountModelsLib.operations opp)
        {
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigList(ref opp, "");

            DataRow[] dr = dt.Select("KeysData = 'Is_Ecrypted'");
            if (dr.Length == 1)
            {
                //msg = "Data Already Decrypted";

                return;

            }
            Common.Is_Ecrypted_Config = 1;
            DecryptSysCofig(dt, ref opp);
            Decrypt_SysConfigMenus(ref opp);
        }
        public void Decrypt_Menu(ref AccountModelsLib.operations opp)
        {
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigList(ref opp, "");

            DataRow[] dr = dt.Select("KeysData = 'Is_Ecrypted'");
            if (dr.Length == 1)
            {
                //msg = "Data Already Decrypted";

                return;

            }
            Common.Is_Ecrypted_Config = 1;

            Decrypt_SysConfigMenus(ref opp);
        }
        public void Decrypt_Config(ref AccountModelsLib.operations opp)
        {
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigList(ref opp, "");

            DataRow[] dr = dt.Select("KeysData = 'Is_Ecrypted'");
            if (dr.Length == 1)
            {
                //msg = "Data Already Decrypted";

                return;

            }
            Common.Is_Ecrypted_Config = 1;
            DecryptSysCofig(dt, ref opp);

        }

        void DecryptSysCofig(DataTable dt, ref AccountModelsLib.operations opp)
        {

            //AccountModelsLib.operations opp = new AccountModelsLib.operations();

            string mysql = " ";
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                string KeysData = dt.Rows[x]["KeysData"].ToString(); // dataGridViewTrial.Rows[x].Cells["KeysData"].Value.ToString();
                string DefaultValue = dt.Rows[x]["DefaultValue"].ToString(); // dataGridViewTrial.Rows[x].Cells["DefaultValue"].Value.ToString();
                string Value = dt.Rows[x]["Value"].ToString(); // dataGridViewTrial.Rows[x].Cells["Value"].Value.ToString();
                string DefaultDescription = dt.Rows[x]["DefaultDescription"].ToString(); // dataGridViewTrial.Rows[x].Cells["DefaultDescription"].Value.ToString();
                string Description = dt.Rows[x]["Description"].ToString(); // dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();

                string POS = dt.Rows[x]["POS"].ToString(); // dataGridViewTrial.Rows[x].Cells["POS"].Value.ToString();
                string Market = dt.Rows[x]["Market"].ToString(); // dataGridViewTrial.Rows[x].Cells["Market"].Value.ToString();
                string Factory = dt.Rows[x]["Factory"].ToString(); // dataGridViewTrial.Rows[x].Cells["Factory"].Value.ToString();
                string Tourism = dt.Rows[x]["Tourism"].ToString(); // dataGridViewTrial.Rows[x].Cells["Tourism"].Value.ToString();
                string AlFawzan = dt.Rows[x]["AlFawzan"].ToString(); // dataGridViewTrial.Rows[x].Cells["Tourism"].Value.ToString();
                string Client_A = dt.Rows[x]["Client_A"].ToString(); // dataGridViewTrial.Rows[x].Cells["Client_A"].Value.ToString();
                string Client_B = dt.Rows[x]["Client_B"].ToString(); // dataGridViewTrial.Rows[x].Cells["Client_B"].Value.ToString();


                string ID = dt.Rows[x]["ID"].ToString(); // dataGridViewTrial.Rows[x].Cells["ID"].Value.ToString();


                KeysData = Common.DecryptMe(KeysData);
                DefaultValue = Common.DecryptMe(DefaultValue);
                Value = Common.DecryptMe(Value);
                DefaultDescription = Common.DecryptMe(DefaultDescription);
                Description = Common.DecryptMe(Description);
                POS = Common.DecryptMe(POS);
                Market = Common.DecryptMe(Market);
                Factory = Common.DecryptMe(Factory);
                Tourism = Common.DecryptMe(Tourism);
                AlFawzan = Common.DecryptMe(AlFawzan);
                Client_A = Common.DecryptMe(Client_A);
                Client_B = Common.DecryptMe(Client_B);

                if (ID == "111") //Is_Ecrypted
                {
                    Value = "0";

                }

                mysql = mysql + @" Update SysConfig Set KeysData = N'" + KeysData + "' ,  ";
                mysql = mysql + "  DefaultValue = N'" + DefaultValue + "' ,";
                mysql = mysql + " Value = N'" + Value + "' ,";
                mysql = mysql + " DefaultDescription = N'" + global::AccountModelsLib.SqlText.Q(DefaultDescription) + "' ,";
                mysql = mysql + " Description = N'" + global::AccountModelsLib.SqlText.Q(Description) + "',  ";

                mysql = mysql + " POS = N'" + POS + "' , ";
                mysql = mysql + " Market = N'" + Market + "' , ";
                mysql = mysql + " Factory = N'" + Factory + "' , ";
                mysql = mysql + " Tourism = N'" + Tourism + "' , ";
                mysql = mysql + " AlFawzan = N'" + AlFawzan + "' , ";
                mysql = mysql + " Client_A = N'" + Client_A + "' , ";
                mysql = mysql + " Client_B = N'" + Client_B + "'  ";
                mysql = mysql + " Where Id= " + ID + " ";


                try
                {
                    opp.insert_me(mysql);
                    mysql = "";
                }
                catch (Exception ee)
                {

                     
                }


            }
            //opp.insert_me(mysql);


        }
        private void Decrypt_SysConfigMenus00(ref AccountModelsLib.operations opp)
        {
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigMenus_List(ref opp);
            string mysql = "";
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                string KeysData = dt.Rows[x]["KeysData"].ToString(); // dataGridViewTrial.Rows[x].Cells["KeysData"].Value.ToString();
                string DefaultValue = dt.Rows[x]["DefaultValue"].ToString(); // dataGridViewTrial.Rows[x].Cells["DefaultValue"].Value.ToString();
                string Value = dt.Rows[x]["Value"].ToString();  //dataGridViewTrial.Rows[x].Cells["Value"].Value.ToString();
                string DefaultDescription = dt.Rows[x]["DefaultDescription"].ToString(); // dataGridViewTrial.Rows[x].Cells["DefaultDescription"].Value.ToString();
                string Description = dt.Rows[x]["Description"].ToString();  //dataGridViewTrial.Rows[x].Cells["Description"].Value.ToString();
                string ID = dt.Rows[x]["ID"].ToString(); // dataGridViewTrial.Rows[x].Cells["ID"].Value.ToString();

                string POS = dt.Rows[x]["POS"].ToString(); // dataGridViewTrial.Rows[x].Cells["POS"].Value.ToString();
                string Market = dt.Rows[x]["Market"].ToString(); // dataGridViewTrial.Rows[x].Cells["Market"].Value.ToString();
                string Factory = dt.Rows[x]["Factory"].ToString(); // dataGridViewTrial.Rows[x].Cells["Factory"].Value.ToString();
                string Tourism = dt.Rows[x]["Tourism"].ToString(); // dataGridViewTrial.Rows[x].Cells["Tourism"].Value.ToString();
                string AlFawzan = dt.Rows[x]["AlFawzan"].ToString();//  dataGridViewTrial.Rows[x].Cells["Tourism"].Value.ToString();
                string Client_A = dt.Rows[x]["Client_A"].ToString(); // dataGridViewTrial.Rows[x].Cells["Client_A"].Value.ToString();
                string Client_B = dt.Rows[x]["Client_B"].ToString();  //dataGridViewTrial.Rows[x].Cells["Client_B"].Value.ToString();
                string UserSells = dt.Rows[x]["UserSells"].ToString(); // dataGridViewTrial.Rows[x].Cells["UserSells"].Value.ToString();


                KeysData = Common.DecryptMe(KeysData);
                DefaultValue = Common.DecryptMe(DefaultValue);
                Value = Common.DecryptMe(Value);
                DefaultDescription = Common.DecryptMe(DefaultDescription);
                Description = Common.DecryptMe(Description);

                POS = Common.DecryptMe(POS);
                Market = Common.DecryptMe(Market);
                Factory = Common.DecryptMe(Factory);
                Tourism = Common.DecryptMe(Tourism);
                AlFawzan = Common.DecryptMe(AlFawzan);
                Client_A = Common.DecryptMe(Client_A);
                Client_B = Common.DecryptMe(Client_B);
                UserSells = Common.DecryptMe(UserSells);




                mysql = mysql + @" Update SysConfigMenus Set KeysData = N'" + KeysData + "' ,  ";
                mysql = mysql + "  DefaultValue = N'" + DefaultValue + "' ,";
                mysql = mysql + " Value = N'" + Value + "' ,";
                mysql = mysql + " DefaultDescription = N'" + global::AccountModelsLib.SqlText.Q(DefaultDescription) + "' ,";
                mysql = mysql + " Description = N'" + global::AccountModelsLib.SqlText.Q(Description) + "' , ";

                mysql = mysql + " POS = N'" + POS + "' , ";
                mysql = mysql + " Market = N'" + Market + "' , ";
                mysql = mysql + " Factory = N'" + Factory + "' , ";
                mysql = mysql + " Tourism = N'" + Tourism + "' , ";
                mysql = mysql + " AlFawzan = N'" + AlFawzan + "' , ";
                mysql = mysql + " Client_A = N'" + Client_A + "' , ";
                mysql = mysql + " Client_B = N'" + Client_B + "' , ";
                mysql = mysql + " UserSells = N'" + UserSells + "'  ";

                mysql = mysql + " Where Id= " + ID + " ";



            }
            opp.insert_me(mysql);

        }
        public void Decrypt_SysConfigMenus(ref AccountModelsLib.operations opp)
        {
            SysConfigCLS ss = new SysConfigCLS();
            DataTable dt = ss.Get_SysConfigMenus_List(ref opp);
            string mysql = " ";

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                string KeysData = dt.Rows[x]["KeysData"].ToString();
                string DefaultValue = dt.Rows[x]["DefaultValue"].ToString();
                string Value = dt.Rows[x]["Value"].ToString();
                string DefaultDescription = dt.Rows[x]["DefaultDescription"].ToString();
                string Description = dt.Rows[x]["Description"].ToString();
                string Description_En = dt.Rows[x]["Description_En"].ToString();
                string Description_Fr = dt.Rows[x]["Description_Fr"].ToString();
                string Description_DE = dt.Rows[x]["Description_DE"].ToString();
                string RazorPath = dt.Rows[x]["RazorPath"].ToString();

                string POS = dt.Rows[x]["POS"].ToString();
                string Market = dt.Rows[x]["Market"].ToString();
                string Factory = dt.Rows[x]["Factory"].ToString();
                string Tourism = dt.Rows[x]["Tourism"].ToString();
                string AlFawzan = dt.Rows[x]["AlFawzan"].ToString();
                string Client_A = dt.Rows[x]["Client_A"].ToString();
                string Client_B = dt.Rows[x]["Client_B"].ToString();
                string UserSells = dt.Rows[x]["UserSells"].ToString();

                string Id = dt.Rows[x]["Id"].ToString();


                KeysData = Common.DecryptMe(KeysData);
                DefaultValue = Common.DecryptMe(DefaultValue);
                Value = Common.DecryptMe(Value);
                DefaultDescription = Common.DecryptMe(DefaultDescription);
                Description = Common.DecryptMe(Description);
                Description_En = Common.DecryptMe(Description_En);
                Description_Fr = Common.DecryptMe(Description_Fr);
                Description_DE = Common.DecryptMe(Description_DE);
                RazorPath = Common.DecryptMe(RazorPath);

                POS = Common.DecryptMe(POS);
                Market = Common.DecryptMe(Market);
                Factory = Common.DecryptMe(Factory);
                Tourism = Common.DecryptMe(Tourism);
                AlFawzan = Common.DecryptMe(AlFawzan);
                Client_A = Common.DecryptMe(Client_A);
                Client_B = Common.DecryptMe(Client_B);


                mysql = @" Update SysConfigMenus Set KeysData = N'" + KeysData + "' ,  ";
                mysql = mysql + "  DefaultValue = N'" + DefaultValue + "' ,";
                mysql = mysql + " Value = N'" + Value + "' ,";
                mysql = mysql + " DefaultDescription = N'" + global::AccountModelsLib.SqlText.Q(DefaultDescription) + "' ,";
                mysql = mysql + " Description = N'" + global::AccountModelsLib.SqlText.Q(Description) + "' , ";
                mysql = mysql + " Description_En = N'" + Description_En.ToSqlSafe() + "' , ";
                mysql = mysql + " Description_Fr = N'" + Description_Fr.ToSqlSafe() + "' , ";
                mysql = mysql + " Description_DE = N'" + Description_DE.ToSqlSafe() + "' , ";
                mysql = mysql + " RazorPath = N'" + RazorPath + "' , ";

                mysql = mysql + " POS = N'" + POS + "' , ";
                mysql = mysql + " Market = N'" + Market + "' , ";
                mysql = mysql + " Factory = N'" + Factory + "' , ";
                mysql = mysql + " Tourism = N'" + Tourism + "' , ";
                mysql = mysql + " AlFawzan = N'" + AlFawzan + "' , ";
                mysql = mysql + " Client_A = N'" + Client_A + "' , ";
                mysql = mysql + " Client_B = N'" + Client_B + "' , ";
                mysql = mysql + " UserSells = N'" + UserSells + "'  ";

                mysql = mysql + " Where Id= " + Id + " ";

                try
                {
                    opp.insert_me(mysql);
                }
                catch (Exception ee)
                {

                }
            }
            try
            {
                //opp.insert_me(mysql);
            }
            catch (Exception ee)
            {

            }
        }


        string[] GetAllTables(SqlConnection connection)
        {
            List<string> result = new List<string>();
            SqlCommand cmd = new SqlCommand("SELECT name FROM sys.Tables", connection);
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
                result.Add(reader["name"].ToString());

            connection.Close();
            return result.ToArray();
        }

        ArrayList GetTablesLookUpNotEmpty()
        {
            ArrayList tb = new ArrayList();

            //tb.Add("CustomerLS");
            //tb.Add("SupplierLS");
            tb.Add("Account_Branches");
            tb.Add("Account_Ehlak_Type");
            //tb.Add("Account_Type");
            tb.Add("Account_Type_Main_Account_Id");
            tb.Add("AccountTaxInfo");
            tb.Add("ConsUnitType");
            tb.Add("Currency");
            tb.Add("Currency_Factor");
            tb.Add("FORMS_DETAILES");
            tb.Add("Govern_List");
            tb.Add("Govern_Area");
            tb.Add("Journal_Codes");
            tb.Add("LookUp_CheckStatus");
            tb.Add("OrderTypes");
            tb.Add("OrderTypes_Label");
            tb.Add("PERMATIONS");
            tb.Add("Submain_Account");
            tb.Add("SysConfig");
            tb.Add("SysConfigMenus");
            tb.Add("SysConfigMenusPERMATIONS");
            tb.Add("DataDictionaryProcessesLists");
            tb.Add("DataDictionaryProcessesNo");



            //tb.Add("unit");
            tb.Add("USER_DETAILES");
            return tb;


        }

        public void AlterDBColumnDataType(ref AccountModelsLib.operations opp)
        {
            string mysql = @"select column_name,* from information_schema.columns  WHERE (DATA_TYPE = N'float') 
                          order by table_name";
            DataTable dt = opp.return_DataTable(mysql);
            //OR (DATA_TYPE = N'numeric')
            int c = dt.Rows.Count;

            foreach (DataRow r in dt.Rows)
            {
                string DATA_TYPE = r["DATA_TYPE"].ToString();
                string Column_name = r["Column_name"].ToString();
                string TableName = r["TABLE_NAME"].ToString();

                if (DATA_TYPE == "numeric")
                {
                    AlterDB_CLS.AlterColumn(ref opp, TableName, Column_name, "bigint");
                }
                if (DATA_TYPE == "float")
                {
                    AlterDB_CLS.AlterColumn(ref opp, TableName, Column_name, "decimal(18,2)");
                }
            }

        }
        public void ClearTblesData(ref AccountModelsLib.operations opp)
        {
            string mysql = "";
            ArrayList tb = new ArrayList();
            tb = GetTablesLookUpNotEmpty();


            string[] tables = GetAllTables(opp.conn);
            foreach (var item in tables)
            {
                string TableName = item;
                if (item == "Account_Branches")
                {
                    int x = 0;
                }
                int index = tb.IndexOf(item);

                if (index < 0)
                {
                    mysql = mysql + " TRUNCATE TABLE " + TableName;
                }

            }
            opp.insert_me(mysql);
            mysql = @"

update Banks set Primary_balance=0,Last_Balance=0  ";
            opp.insert_me(mysql);
            mysql = @"

            INSERT INTO Customer_Supplier_Group
                         (GroupName, GroupType, User_Id, Branch_Id, Edit_User_Id, ProcessNo)
VALUES        (N'مجموعة العملاء الأولى', 2, 1, 1, 1, 0) ";
            opp.insert_me(mysql);
            mysql = @" 
INSERT INTO Customer_Supplier_Group
                         (GroupName, GroupType, User_Id, Branch_Id, Edit_User_Id, ProcessNo)
VALUES        (N'مجموعة الموردين الأولى', 5, 1, 1, 1, 0)
";
            opp.insert_me(mysql);
            mysql = @" 
 INSERT INTO Price_Type_List
                         (Price_Name, IS_Active, Price_Name_Master, Price_Type_List_Master_Id, Plus_Minus, Added_Value, IS_Rate, User_Id, Branch_Id, Edit_User_Id, ProcessNo)
VALUES        (N'نوع السعر الاساسى', 1, N'إختيار السعر', - 1, N'-', 0, 0, 1, 1, 1, 0)
";
            opp.insert_me(mysql);
            mysql = @" 
INSERT INTO Banks
                         (Bank_Name, Primary_balance, Last_Balance, SubAccount_Id, Is_Bank, Currency_Id, Default_Bank, Supplier_Bank, Account_Type_Value, User_Id, Branch_Id, Active, ProcessNo)
VALUES        (N'الخزينة الرئيسية', 0, 0, 0, 0, 1, 1, 0, 6, 1, 1, 1, 0)
";
            opp.insert_me(mysql);
            UpdateDataTables(ref   opp);
            /*
                mysql = @" 

  SET IDENTITY_INSERT [dbo].[Customer] ON  
                 
INSERT [dbo].[Customer] ([Id], [Name], [Active], [Balance], [Balance_Actual], [Code], [Address], [Tel], [Tel2], [Mobile], [E_Mail], [Fax], [Web_Site], [Customer_Type], [SubAccount_Id], [IsSalesMan], [SalesMan_Id], [Date_Balance_First], [Balance_First], [Location_Id], [Price_Type_List_ID], [Tax_No], [Balance_Air], [Balance_Road], [Account_Code], [Currency_Id], [Static_Flage], [General_Customer_Flage], [Group_Id], [Ref_Cust_Supplier_Id],[LookUp_PersonCompany_TypeId]) VALUES (CAST(1 AS Numeric(10, 0)), N'عملاء متنوعون', 1, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'1', N'-', N'', N'', N'', N'', N'', N'', 1, CAST(3211 AS Numeric(18, 0)), 0, NULL, CAST(N'2019-07-21 00:00:00.000' AS DateTime), CAST(0.00 AS Decimal(18, 2)), NULL, CAST(0 AS Numeric(18, 0)), N'', CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), 1, 1, 0, 1, 1, 0,1)
INSERT [dbo].[Customer] ([Id], [Name], [Active], [Balance], [Balance_Actual], [Code], [Address], [Tel], [Tel2], [Mobile], [E_Mail], [Fax], [Web_Site], [Customer_Type], [SubAccount_Id], [IsSalesMan], [SalesMan_Id], [Date_Balance_First], [Balance_First], [Location_Id], [Price_Type_List_ID], [Tax_No], [Balance_Air], [Balance_Road], [Account_Code], [Currency_Id], [Static_Flage], [General_Customer_Flage], [Group_Id], [Ref_Cust_Supplier_Id],[LookUp_PersonCompany_TypeId]) VALUES (CAST(2 AS Numeric(10, 0)), N'عملاء دائمون', 1, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'1', N'-', N'', N'', N'', N'', N'', N'', 1, CAST(3211 AS Numeric(18, 0)), 0, NULL, CAST(N'2019-07-21 00:00:00.000' AS DateTime), CAST(0.00 AS Decimal(18, 2)), NULL, CAST(0 AS Numeric(18, 0)), N'', CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), 1, 1, 0, 0, 1, 0,1)
 SET IDENTITY_INSERT [dbo].[Customer] OFF 
";
             
            opp.insert_me(mysql);
            mysql = @" 
SET IDENTITY_INSERT [dbo].[Item] ON 

INSERT INTO Item
                         (id, code, Item_Name, S_id, Item_Category_ID, No3eboa_old, No3eboa, No3eboa_BigUnit, No3eboa_SmallUnit, Active_flage, Upon_Request, Unit_Id, StoreUsed_Type, AnotherName)
VALUES        (CAST(1 AS Numeric(10, 0)), N'1', N'صنف 1', 1, 1, NULL, CAST(1.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 1, CAST(0.00 AS Decimal(18, 2)), 9, 1, N'')
SET IDENTITY_INSERT [dbo].[Item] OFF
";
            opp.insert_me(mysql);
            mysql = @" 


SET IDENTITY_INSERT [dbo].[Store] ON 

INSERT [dbo].[Store] ([id], [Store_Name], [Store_location], [Emp_id], [Job_id], [StoreUsed_Type], [Branch_Id], [Employee_Name], [Store_Code], [IS_Default_Store], [IS_Sell_SecondUnit], [IS_ItemSecond_degree], [Active],[LookUp_StoreOrderTypesId]) VALUES (CAST(1 AS Numeric(18, 0)), N'المخزن الرئيسى', N'-', CAST(0 AS Numeric(18, 0)), 0, 1, 1, N'', N'0', 1, 0, 0, 1,4)
SET IDENTITY_INSERT [dbo].[Store] OFF

";
            opp.insert_me(mysql);
            mysql = @" 


SET IDENTITY_INSERT [dbo].[Supplier] ON 
INSERT [dbo].[Supplier] ([Id], [Name], [Code], [Address], [Tel], [Tel2], [Mobile], [E_Mail], [Fax], [Web_Site], [Balance], [Balance_Actual], [Supplier_Type], [Registration_No], [Active], [SubAccount_Id], [Date_Balance_First], [Balance_First], [Balance_Air], [Balance_Road], [Account_Code], [Currency_Id], [Tax_Registration_No], [Group_Id], [Ref_Cust_Supplier_Id]) VALUES (CAST(3212 AS Numeric(10, 0)), N'مورد 1', N'1', N'-', N'', N'', N'', N'', N'', N'', CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 1, NULL, 1, CAST(3212 AS Numeric(18, 0)), CAST(N'2019-07-21 00:00:00.000' AS DateTime), 0, CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), 1, 1, N'', 2, 0)
SET IDENTITY_INSERT [dbo].[Supplier] OFF

";
            opp.insert_me(mysql);
            mysql = @" 
SET IDENTITY_INSERT [dbo].[Unit_item] ON 

INSERT [dbo].[Unit_item] ([id], [Item_id], [Store_id], [Store_Type], [Code], [unit_id], [Full_qua], [D_Qua], [Item_No], [Reserved_Balance_Larg_Qnty], [Reserved_Balance_Small_Qnty], [Big_unit], [Small_unit], [P_price], [S_price], [Specification], [Upon_Request], [Currency], [active_flage], [TaxSales10_Value], [New2]) VALUES (CAST(1 AS Numeric(18, 0)), 1, CAST(1 AS Numeric(18, 0)), 4, N'1', 9, CAST(0 AS Numeric(18, 0)), CAST(0 AS Numeric(18, 0)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 1, 12, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'جنية مصرى', 1, NULL, 1)
SET IDENTITY_INSERT [dbo].[Unit_item] OFF
";
            opp.insert_me(mysql);
            mysql = @" 

SET IDENTITY_INSERT [dbo].[Item_Category] ON 

INSERT [dbo].[Item_Category] ([ID], [Category_Name], [Category_Type_Str], [Category_Type_Id], [Main_Item_Category_Id], [IS_Active], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(1 AS Numeric(18, 0)), N'فئة 1', N'مبيعات', 1, 1, 1, NULL, 0, 0, NULL, NULL, NULL, 0)
SET IDENTITY_INSERT [dbo].[Item_Category] OFF
";
            opp.insert_me(mysql);
            mysql = @" 
SET IDENTITY_INSERT [dbo].[Item_Main_Category] ON 

INSERT [dbo].[Item_Main_Category] ([ID], [Main_Category_Name], [Category_Type_Id], [Category_Type_Str], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(1 AS Numeric(18, 0)), N'الفئة الأولى ', 1, N'مبيعات', NULL, 0, 0, NULL, NULL, NULL, 0)
SET IDENTITY_INSERT [dbo].[Item_Main_Category] OFF
";
            opp.insert_me(mysql);
            mysql = @" 


SET IDENTITY_INSERT [dbo].[unit] ON 

INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(1 AS Numeric(18, 0)), N'وزن', N'كيلو', N'جرام', 1, 1000, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(9 AS Numeric(18, 0)), N'الدست', N'الدستة', N'قطعة', 1, 12, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(17 AS Numeric(18, 0)), N'العدد', N'قطعة', N'--', 1, 1, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(27 AS Numeric(18, 0)), N'بوكس', N'كرتونة', N'دستة', 1, 10, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(28 AS Numeric(18, 0)), N'المتر', N'متر', N'سم', 1, 100, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(29 AS Numeric(18, 0)), N'علبة', N'علبة', N'بكرة', 1, 12, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(30 AS Numeric(18, 0)), N'جوبير هرمية', N'يارده', N' سم', 1, 91, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(31 AS Numeric(18, 0)), N'كرتونة', N'كرتونه', N'علبة', 1, 10, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(32 AS Numeric(18, 0)), N'الوزن بالطن', N'الطن', N'الكيلو', 1, 1000, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(33 AS Numeric(18, 0)), N'المتر المربع', N'²م', N'²سم', 1, 10000, NULL, 0, 0, NULL, NULL, NULL, 0)
INSERT [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES (CAST(34 AS Numeric(18, 0)), N'المتر المربع (1)', N'متر مربع', N'-', 1, 1, NULL, 0, 0, NULL, NULL, NULL, 0)
SET IDENTITY_INSERT [dbo].[unit] OFF
 

";
            opp.insert_me(mysql);
            */

            AlterDB_CLS.Set_Account_Type_Table(ref opp);
           
        }
        
        void UpdateDataTables(ref operations opp)
        { 
            // 1. Parent Categories
            AppendTableInsert("[dbo].[Item_Main_Category]", @"
INSERT INTO [dbo].[Item_Main_Category] 
    ([ID], [Main_Category_Name], [Category_Type_Id], [Category_Type_Str], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) 
VALUES 
    (1, N'الفئة الأولى', 1, N'مبيعات', NULL, 0, 0, NULL, NULL, NULL, 0);
");

            AppendTableInsert("[dbo].[Item_Category]", @"
INSERT INTO [dbo].[Item_Category] 
    ([ID], [Category_Name], [Category_Type_Str], [Category_Type_Id], [Main_Item_Category_Id], [IS_Active], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) 
VALUES 
    (1, N'فئة 1', N'مبيعات', 1, 1, 1, NULL, 0, 0, NULL, NULL, NULL, 0);
");

            // 2. Units of Measure
            AppendTableInsert("[dbo].[unit]", @"
INSERT INTO [dbo].[unit] ([id], [u_name], [B_Unit], [S_unit], [B_Unit_Value], [S_unit_Value], [OperationDate], [User_Id], [Branch_Id], [Insert_ServerDateTime], [Update_ServerDateTime], [EditOperationDate], [Edit_User_Id]) VALUES 
    (1,  N'وزن',            N'كيلو',     N'جرام',  1, 1000,  NULL, 0, 0, NULL, NULL, NULL, 0),
    (9,  N'الدست',          N'الدستة',   N'قطعة',  1, 12,    NULL, 0, 0, NULL, NULL, NULL, 0),
    (17, N'العدد',          N'قطعة',     N'--',    1, 1,     NULL, 0, 0, NULL, NULL, NULL, 0),
    (27, N'بوكس',           N'كرتونة',   N'دستة',  1, 10,    NULL, 0, 0, NULL, NULL, NULL, 0),
    (28, N'المتر',          N'متر',      N'سم',    1, 100,   NULL, 0, 0, NULL, NULL, NULL, 0),
    (29, N'علبة',           N'علبة',     N'بكرة',  1, 12,    NULL, 0, 0, NULL, NULL, NULL, 0),
    (30, N'جوبير هرمية',    N'يارده',    N'سم',    1, 91,    NULL, 0, 0, NULL, NULL, NULL, 0),
    (31, N'كرتونة',         N'كرتونه',   N'علبة',  1, 10,    NULL, 0, 0, NULL, NULL, NULL, 0),
    (32, N'الوزن بالطن',    N'الطن',     N'الكيلو',1, 1000,  NULL, 0, 0, NULL, NULL, NULL, 0),
    (33, N'المتر المربع',   N'²م',       N'²سم',   1, 10000, NULL, 0, 0, NULL, NULL, NULL, 0),
    (34, N'المتر المربع (1)',N'متر مربع',N'-',     1, 1,     NULL, 0, 0, NULL, NULL, NULL, 0);
");

            // 3. Stores
            AppendTableInsert("[dbo].[Store]", @"
INSERT INTO [dbo].[Store] 
    ([id], [Store_Name], [Store_location], [Emp_id], [Job_id], [StoreUsed_Type], [Branch_Id], [Employee_Name], [Store_Code], [IS_Default_Store], [IS_Sell_SecondUnit], [IS_ItemSecond_degree], [Active], [LookUp_StoreOrderTypesId]) 
VALUES 
    (1, N'المخزن الرئيسى', N'-', 0, 0, 1, 1, N'', N'0', 1, 0, 0, 1, 4);
");

            // 4. Items (Depends on Category & Unit)
            AppendTableInsert("[dbo].[Item]", @"
INSERT INTO [dbo].[Item] 
    ([id], [code], [Item_Name], [S_id], [Item_Category_ID], [No3eboa_old], [No3eboa], [No3eboa_BigUnit], [No3eboa_SmallUnit], [Active_flage], [Upon_Request], [Unit_Id], [StoreUsed_Type], [AnotherName]) 
VALUES 
    (1, N'1', N'صنف 1', 1, 1, NULL, 1.00, 0.00, 0.00, 1, 0.00, 9, 1, N'');
");

            // 5. Unit Items (Depends on Item, Store, and Unit)
            AppendTableInsert("[dbo].[Unit_item]", @"
INSERT INTO [dbo].[Unit_item] 
    ([id], [Item_id], [Store_id], [Store_Type], [Code], [unit_id], [Full_qua], [D_Qua], [Item_No], [Reserved_Balance_Larg_Qnty], [Reserved_Balance_Small_Qnty], [Big_unit], [Small_unit], [P_price], [S_price], [Specification], [Upon_Request], [Currency], [active_flage], [TaxSales10_Value], [New2]) 
VALUES 
    (1, 1, 1, 4, N'1', 9, 0, 0, 0.00, 0.00, 0.00, 1, 12, 0.00, 0.00, NULL, 0, N'جنية مصرى', 1, NULL, 1);
");

            // 6. Customers
            AppendTableInsert("[dbo].[Customer]", @"
INSERT INTO [dbo].[Customer] 
    ([Id], [Name], [Active], [Balance], [Balance_Actual], [Code], [Address], [Tel], [Tel2], [Mobile], [E_Mail], [Fax], [Web_Site], [Customer_Type], [SubAccount_Id], [IsSalesMan], [SalesMan_Id], [Date_Balance_First], [Balance_First], [Location_Id], [Price_Type_List_ID], [Tax_No], [Balance_Air], [Balance_Road], [Account_Code], [Currency_Id], [Static_Flage], [General_Customer_Flage], [Group_Id], [Ref_Cust_Supplier_Id], [LookUp_PersonCompany_TypeId]) 
VALUES 
    (1, N'عملاء متنوعون', 1, 0.00, 0.00, N'1', N'-', N'', N'', N'', N'', N'', N'', 1, 3211, 0, NULL, '2019-07-21', 0.00, NULL, 0, N'', 1.00, 1.00, 1, 1, 0, 1, 1, 0, 1),
    (2, N'عملاء دائمون',  1, 0.00, 0.00, N'2', N'-', N'', N'', N'', N'', N'', N'', 1, 3211, 0, NULL, '2019-07-21', 0.00, NULL, 0, N'', 1.00, 1.00, 1, 1, 0, 0, 1, 0, 1);
");

            // 7. Suppliers
            AppendTableInsert("[dbo].[Supplier]", @"
INSERT INTO [dbo].[Supplier] 
    ([Id], [Name], [Code], [Address], [Tel], [Tel2], [Mobile], [E_Mail], [Fax], [Web_Site], [Balance], [Balance_Actual], [Supplier_Type], [Registration_No], [Active], [SubAccount_Id], [Date_Balance_First], [Balance_First], [Balance_Air], [Balance_Road], [Account_Code], [Currency_Id], [Tax_Registration_No], [Group_Id], [Ref_Cust_Supplier_Id]) 
VALUES 
    (3212, N'مورد 1', N'1', N'-', N'', N'', N'', N'', N'', N'', 0.00, 0.00, 1, NULL, 1, 3212, '2019-07-21', 0.00, 1.00, 1.00, 1, 1, N'', 2, 0);
");
            opp.insert_me(sb.ToString());
        }
        StringBuilder sb = new StringBuilder();
        string  AppendTableInsert(string tableName, string insertSql)
        {
            bool useIdentity = (Common.Use_Auto_Identity_Column == 1);
           
            if (useIdentity)
                sb.AppendLine($"SET IDENTITY_INSERT {tableName} ON;");

            sb.AppendLine(insertSql.Trim());

            if (useIdentity)
                sb.AppendLine($"SET IDENTITY_INSERT {tableName} OFF;");

            sb.AppendLine();

            return sb.ToString();
        }
    }
}
