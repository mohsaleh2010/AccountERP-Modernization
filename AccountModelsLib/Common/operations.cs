using AccountModelsLib.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage; // إذا كنت تتعامل مع IDbContextTransaction
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
//using Microsoft.Data.SqlClient;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
namespace AccountModelsLib
{

    /// <summary>
    /// Summary description for oprtation.
    /// </summary>
    public class operations : IDisposable
    {


        public bool MakeTransactionFlage = false;

        string Msg;
        public string str_sql_conn;
        public static string conn_str_ = "";
        public static string DabaseName = "";
        public static string DbServerName = "";
        public static string Username = "";
        public static string Password = "";
        public static string Used = "";
        public static string Is_Use_Provider = "";
        public static string DataBaseType = "";


        int search_ds_count_dl;

        public bool ConectionStaus;

        DataSet ds;

        //sql connection
        public SqlConnection conn;
        SqlCommandBuilder combd;
        public SqlDataAdapter ada;
        public SqlCommand cmd;
        SqlDataReader dr;
        public SqlTransaction tns;
        public int IsTestConn = 0;

        public My_DbContext db;
        //public OleDbConnection conn;

        //OleDbDataAdapter ada;
        //OleDbCommandBuilder combd;
        //public 	OleDbCommand cmd ;	
        //OleDbDataReader dr;
        //OleDbTransaction tns;
        public operations(int MakeTransaction_)
        {
            //Common.Customer_TypeChanged();

            //if (Common.VersionWillExpire == 1)
            //{
            //}

            Check_Correct_ConnStr();

            BeginTransaction(MakeTransaction_);
            //MakeTransactionFlage = true;
            //tns = conn.BeginTransaction();
            //cmd.Transaction = tns;

            //if (MakeTransaction_ == 2)
            //{
            //    //db = new My_DbContext(this.conn, true);
            //    db.Database.UseTransaction(tns);
            //}


        }
        public operations(bool TestConn, int IsTestConn_)
        {
            this.MakeTransactionFlage = false;
            this.IsTestConn = 0;
            this.IsTestConn = IsTestConn_;
            this.Check_Correct_ConnStr();
        }
        public operations(string str_conn)
        {
            //conn = new OleDbConnection(str_conn.ToString());
            //ds = new DataSet();
            //ada = new OleDbDataAdapter();
            //combd = new OleDbCommandBuilder(ada);
            //cmd = new OleDbCommand();

            conn = new SqlConnection(str_conn.ToString());
            ds = new DataSet();
            ada = new SqlDataAdapter();
            combd = new SqlCommandBuilder(ada);
            cmd = new SqlCommand();

            cmd.Connection = conn;
            db = new My_DbContext(this.conn, true);
            try
            {
                conn.Open();
                ConectionStaus = true;
                conn.Close();
            }
            catch (Exception ee) { ConectionStaus = false; }
        }
        public operations()
        {
            //Common.Customer_TypeChanged();

            Check_Correct_ConnStr();


        }
        //public operations(bool FirstRun)
        //{
        //    Common.Customer_TypeChanged();
        //    Check_Correct_ConnStr();
        //    if (FirstRun == false)
        //    {
        //        Common.ExpirationVersion();
        //    }
        //}
        public operations(bool OpenConn)
        {
            MyConn();
            //conn.Open();
            try
            {
                conn.Open();
                ConectionStaus = true;
                conn.Close();
                //db = new My_DbContext(this.conn, true);
            }
            catch (Exception)
            {

                if (this.IsTestConn == 0)
                {
                    // this.ConnectionError();
                }
            }
        }

        public void BeginTransaction(int MakeTransaction_ = 2)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            MakeTransactionFlage = true;
            tns = conn.BeginTransaction();
            cmd.Transaction = tns;
            if (MakeTransaction_ == 2)
            {
                //db = new My_DbContext(this.conn, true);

            }
            db.Database.UseTransaction(tns);
        }
        public void Commit()
        {
            if (MakeTransactionFlage)
            {


                tns.Commit();
                ////tns.Dispose();
                if (conn.State != System.Data.ConnectionState.Closed)
                {
                    conn.Close();
                }
            }

        }
        public void Rollback()
        {
            if (MakeTransactionFlage)
            {

                try
                {
                    tns.Rollback();
                    if (conn.State != System.Data.ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
                catch (Exception ee)
                {
                    //  MessageBox.Show(ee.Message);
                    throw (ee);
                }
            }

        }

        public static string ReturnConnStr()
        {


            if (Common.IS_ReadConStrFromXMLInfo == 1)
            {
                //conn_str_ = " Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=***;Data Source=***";
                ///conn_str_ readed from  Common.SetSystemParameters();  on program.cs file
                // conn_str_ = ReadXmlDataSetConnStr();
            }
            else
            {
                // Connect Timeout=10000
                //  conn_str_ = "Provider=SQLNCLI.1;Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";


                conn_str_ = "Data Source=***;Initial Catalog=***;Integrated Security=True";
                conn_str_ = "Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";


                conn_str_ = "Data Source=***;Initial Catalog=***;Integrated Security=True";
                //   conn_str_ = "Password=***;Persist Security Info=True;User ID=***;Initial Catalog=***;Data Source=***";


                //POS_SUN_DB
                //  conn_str_ = "Provider=SQLNCLI;Server=***;AttachDbFilename=E:\\POS_SUN_DB\\Connection.mdf; Database=***;Trusted_Connection=Yes;";

                // conn_str_ = "Server=***;AttachDbFilename=E:\\POS_SUN_DB\\Connection.mdf; Database=***;Trusted_Connection=Yes;";


                // conn_str_ =  "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=***;Data Source=***";
                //  conn_str_ = "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=***;Data Source=***; Connect Timeout=10000";
                //  conn_str_ = " Server=***;AttachDbFilename=|DataDirectory|Connection.mdf;Database=***;Trusted_Connection=Yes;";
                //      conn_str_ = "Password=***;Persist Security Info=True;User ID=***;Initial Catalog=***;Data Source=***";
                // ReadXmlDataSetConnStr();


                // conn_str_ = "Provider=SQLOLEDB.1;Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";
                //conn_str_= ReadXmlDataSetConnStr();
                //  conn_str_ = "Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";
                // sql express server
                // conn_str_ =  "Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Data Source=***;Initial File Name=C:\\db\\MytesttExpree.mdf"; 

                ///native client 

                /*  conn_str_ = "Provider=SQLOLEDB.1;Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";
                  conn_str_ = "Provider=SQLNCLI.1;Integrated Security=SSPI;Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";
               */


                conn_str_ = " Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";

                conn_str_ = "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=***;Data Source=***";
                conn_str_ = "Password=***;Persist Security Info=True;User ID=***;Initial Catalog=***;Data Source=***";
                // conn_str_ = "Password=***;Persist Security Info=True;User ID=***;Initial Catalog=***;Data Source=***";
                conn_str_ = "Password=***;Persist Security Info=True;User ID=***;Initial Catalog=***;Data Source=***";
                conn_str_ = "Integrated Security=SSPI;Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";
                conn_str_ = " Persist Security Info=False;User ID=***;Initial Catalog=***;Data Source=***";

                conn_str_ = " Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=***;Data Source=***";

            }
            //
            return conn_str_;
        }

        public void Check_Correct_ConnStr()
        {
            MyConn();
            try
            {
                this.conn.Open();
                this.ConectionStaus = true;
                this.conn.Close();
                db = new My_DbContext(this.conn, true);
            }
            catch (Exception)
            {

                if (this.IsTestConn == 0)
                {
                    ConectionStaus = false;

                    return;
                    //this.ConnectionError();
                }
                else
                {
                    ConectionStaus = true;

                    return;
                }
            }
        }
        //private void ConnectionError()
        //{
        //    new ConnErrorTest(ref this.conn).ShowDialog();
        //}
        public void MyConn()
        {
            try
            {
                if (conn_str_ == "")
                {
                    str_sql_conn = ReturnConnStr();
                }
                else
                {
                    str_sql_conn = conn_str_;
                }
                // str_sql_conn = "Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;User ID=***;password=***;Initial Catalog=***;Data Source=***";
                //  str_sql_conn = "Provider=SQLOLEDB.1;Persist Security Info=False;User ID=***;password=***;Initial Catalog=***;Data Source=***";

                ///***********************************************
                //sql connection
                conn = new SqlConnection(str_sql_conn.ToString());
                ds = new DataSet();
                ada = new SqlDataAdapter();
                cmd = new SqlCommand();
                cmd.Connection = conn;
                combd = new SqlCommandBuilder(ada);
                //conn=new OleDbConnection(str_sql_conn.ToString());
                //ds=new DataSet();
                //ada=new OleDbDataAdapter();
                //combd=new OleDbCommandBuilder(ada);
                //cmd=new OleDbCommand();
                //cmd.Connection=conn; 
            }
            catch (Exception ee) { }
        }
        /*
        public static string ReadXmlDataSetConnStr()
        {
            DataSet dss;
            long Excp = 0;
            EncryptProcess en;
            ConnConfigInfoXML cc;
            en = new EncryptProcess();
            cc = new ConnConfigInfoXML(ref Excp);

            dss = cc.dsReadXML();
            string ConStren = "";
            //ds.Tables["AuthenticationInfo"].Select("ID="+index);
            for (int x = 0; x < dss.Tables["AuthenticationInfo"].Rows.Count; x++)
            {
                if (dss.Tables["AuthenticationInfo"].Rows[x]["Used"].ToString() == "1")
                {
                    // DabaseNameEn = dss.Tables["AuthenticationInfo"].Rows[x]["DataBaseName"].ToString();
                    //DbServerEn = dss.Tables["AuthenticationInfo"].Rows[x]["DBSERVER"].ToString();
                    //UsernameEn = dss.Tables["AuthenticationInfo"].Rows[x]["USN"].ToString();
                    // PasswordEn = dss.Tables["AuthenticationInfo"].Rows[x]["PW"].ToString();
                    // Used = dss.Tables["AuthenticationInfo"].Rows[x]["Used"].ToString();
                    ConStren = dss.Tables["AuthenticationInfo"].Rows[x]["Str"].ToString();
                    try
                    {
                        DabaseName = dss.Tables["AuthenticationInfo"].Rows[x]["DataBaseName"].ToString();
                        DbServerName = dss.Tables["AuthenticationInfo"].Rows[x]["DBSERVER"].ToString();
                        Username = dss.Tables["AuthenticationInfo"].Rows[x]["USN"].ToString();
                        Password = dss.Tables["AuthenticationInfo"].Rows[x]["PW"].ToString();
                        Used = dss.Tables["AuthenticationInfo"].Rows[x]["Used"].ToString();
                        DataBaseType = dss.Tables["AuthenticationInfo"].Rows[0]["DataBaseType"].ToString();
                    }
                    catch
                    {

                    }
                }
                ConStren = dss.Tables["AuthenticationInfo"].Rows[0]["Str"].ToString();
                try
                {
                    Common.IS_ReadConStrFromXMLInfo = int.Parse(dss.Tables["AuthenticationInfo"].Rows[0]["Used"].ToString());
                }
                catch
                {

                    Common.IS_ReadConStrFromXMLInfo = 0;
                }
                try
                {
                    Common.dtExpire = DateTime.Parse(en.decrypt(dss.Tables["AuthenticationInfo"].Rows[0]["DT"].ToString()));
                }
                catch { }
                try
                {
                    Common.VersionWillExpire = int.Parse(en.decrypt(dss.Tables["AuthenticationInfo"].Rows[0]["STS"].ToString()));
                }
                catch { }

                try
                {
                    Common.RenewExpireCount = int.Parse(en.decrypt(dss.Tables["AuthenticationInfo"].Rows[0]["CSTS"].ToString()));
                }
                catch { }

                try
                {
                    Common.SystemInfo = en.decrypt(dss.Tables["AuthenticationInfo"].Rows[0]["SystemInfo"].ToString());
                }
                catch { }

                try
                {
                    Common.DTAdmin = en.decrypt(dss.Tables["AuthenticationInfo"].Rows[0]["DTAdmin"].ToString());
                }
                catch { Common.DTAdmin = "Stop"; }


                try
                {
                    string InstituteNameFromInfo = en.decrypt(dss.Tables["AuthenticationInfo"].Rows[0]["In"].ToString());
                    if (InstituteNameFromInfo.Trim() == "" | InstituteNameFromInfo.Trim() == "0" | InstituteNameFromInfo.Trim() == "-")
                    { Common.InstituteNameFromInfo = ""; }
                    else
                    {
                        Common.InstituteNameFromInfo = InstituteNameFromInfo;
                    }
                }
                catch { }





            }
            //if(DataBaseType=="5")
            //{
            //    DBConfigCLS dd = new DBConfigCLS();
            //    dd.IsRestartLocalDB = false;
            //    dd.AutoRebuildSave();
            //    ConStren = dd.ConStr;
            //}
            //   return en.decrypt(ConStren);
            conn_str_ = ConStren;
            return ConStren;

        }
        */
        /*
         public static string ReadXmlDataStyles()
         {
             DataSet dss;
             long Excp = 0;
             EncryptProcess en;
             ConnConfigInfoXML cc;
             en = new EncryptProcess();
             cc = new ConnConfigInfoXML(ref Excp);
             string Styles = "";
             dss = cc.dsReadXML();
             String ConStren = "";
             //ds.Tables["AuthenticationInfo"].Select("ID="+index);
             for (int x = 0; x < dss.Tables["AuthenticationInfo"].Rows.Count; x++)
             {
                 if (dss.Tables["AuthenticationInfo"].Rows[x]["Used"].ToString() == "1")
                 {
                     // DabaseNameEn = dss.Tables["AuthenticationInfo"].Rows[x]["DataBaseName"].ToString();
                     //DbServerEn = dss.Tables["AuthenticationInfo"].Rows[x]["DBSERVER"].ToString();
                     //UsernameEn = dss.Tables["AuthenticationInfo"].Rows[x]["USN"].ToString();
                     // PasswordEn = dss.Tables["AuthenticationInfo"].Rows[x]["PW"].ToString();
                     // Used = dss.Tables["AuthenticationInfo"].Rows[x]["Used"].ToString();
                     Styles = dss.Tables["AuthenticationInfo"].Rows[x]["Styles"].ToString();
                 }
                 Styles = dss.Tables["AuthenticationInfo"].Rows[0]["Styles"].ToString();
             }
             //   return en.decrypt(ConStren);
             return Styles;

         }

         */
        public static string Get_DateTime_NowToInsert()
        {
            DateTime now = DateTime.Now;
            return ("CONVERT(DATETIME, '" + now.ToString("MM/dd/yyyy HH:mm") + "', 102)");
        }

        /// <summary>
        /// Formate the date to be formate as  MM/dd/yyyy
        /// </summary>
        /// <param name="mydate">the date to be formated MM/dd/yyyy</param>
        /// <returns></returns>
        public static string dateFormat(DateTime mydate)
        {

            DateTime dt = new DateTime(mydate.Year, mydate.Month, mydate.Day, mydate.Hour, mydate.Minute, mydate.Second);
            if (Common.PointOfSale == 0)
            {
                return dt.ToString("MM/dd/yyyy");
            }
            else
            {
                return dt.ToString("MM/dd/yyyy HH:mm");
            }
        }
        public static string dateFormat(DateTime? mydate)
        {
            string dat = "";
            if (mydate != null)
            {
                DateTime dt = new DateTime(mydate.Value.Year, mydate.Value.Month, mydate.Value.Day, mydate.Value.Hour, mydate.Value.Minute, mydate.Value.Second);
                if (Common.PointOfSale == 0)
                {
                    dat = dt.ToString("MM/dd/yyyy");
                }
                else
                {
                    dat = dt.ToString("MM/dd/yyyy HH:mm");
                }
            }
            else
            {

            }
            return dat;

        }
        public static string dateTimeFormat(DateTime mydate)
        {

            DateTime dt = new DateTime(mydate.Year, mydate.Month, mydate.Day, 23, 60, 59);
            return dt.ToString("MM/dd/yyyy");

        }
        public static string dateFormatAr(DateTime mydate)
        {

            DateTime dt = new DateTime(mydate.Year, mydate.Month, mydate.Day);
            return dt.ToString("yyyy/MM/dd");

        }
        public static string TimeFormat(DateTime mydate)
        {

            DateTime dt = new DateTime(mydate.Year, mydate.Month, mydate.Day, mydate.Hour, mydate.Minute, mydate.Second);
            return dt.ToString("HH:mm t");

        }
        public static string TimeFormatToInsert(DateTime mydate)
        {
            string dd = "CONVERT(DATETIME, '" + mydate.ToString("HH:mm") + "', 102)";

            return dd;

        }
        /// <summary>
        /// Formate the date to be formate as  dd/MM/yyyy
        /// </summary>
        /// <param name="mydate">the date to be formated MM/dd/yyyy</param>
        /// <returns></returns>
        public static string dateFormat(DateTime mydate, string Type_dd)
        {
            DateTime dt = new DateTime(mydate.Year, mydate.Month, mydate.Day);
            return dt.ToString("dd/MM/yyyy");
        }
        public static string Get_DateTimeToInsert(DateTime mydate)
        {

            DateTime singDateTime = mydate;

            string dd = "CONVERT(DATETIME, '" + singDateTime.ToString("MM/dd/yyyy HH:mm") + "', 102)";

            return dd;

        }

        public static string Get_DateToInsert(DateTime mydate)
        {

            DateTime singDateTime = mydate;

            string dd = "CONVERT(DATETIME, '" + singDateTime.ToString("MM/dd/yyyy") + "', 102)";

            return dd;

        }
        public static string Get_DateToInsert(DateTime? mydate)
        {

            DateTime? singDateTime = mydate;

            string dd = "CONVERT(DATETIME, '" + singDateTime.Value.ToString("MM/dd/yyyy") + "', 102)";

            return dd;

        }

        public static string Get_DateTimeFormate(DateTime mydate)
        {

            DateTime singDateTime = mydate;

            string dd = "CONVERT(DATETIME, '" + singDateTime.ToString("MM/dd/yyyy hh:mm:ss") + "', 102)";

            return dd;

        }
        public static string Get_TimeToInsert(DateTime mydate)
        {

            DateTime singDateTime = mydate;

            string dd = "CONVERT(DATETIME, '" + singDateTime.ToString("HH:mm") + "', 102)";

            return dd;

        }
        public int search_ds_count
        {
            get
            {
                return search_ds_count_dl;
            }

        }

        /// <summary>
        /// Execute insert statement into table
        /// </summary>
        /// <param name="mysql_insert">insert statement into</param>
        /// <param name="Excep">Exception id</param>
        public void insert_me(string mysql_insert, ref int Excep)
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                long Identity = 0;
                mysql_insert = SetUpInsertCmd(mysql_insert, ref Identity);
                cmd.CommandText = mysql_insert.Trim();
                ada.InsertCommand = cmd;
                ada.InsertCommand.ExecuteNonQuery();
                conn.Close();
                try
                {
                    Get_SqlCommand_TextClear(cmd, false);
                }
                catch { }
            }
            catch (Exception e)
            {
                Excep = -1;
                throw (e);

            }
        }


        /// <summary>
        /// Execute insert statement into table and return the last identify column data
        /// </summary>
        /// <param name="mysql_insert">insert statement into</param>
        /// <param name="Excep">Exception id</param>
        public long Insert_me_Return_Last_Identify(string mysql_insert, ref int Excp)
        {
            long Identity = 0;
            try
            {
                if (Common.Use_Auto_Identity_Column == 1)
                {


                    mysql_insert = mysql_insert + "  SELECT  @@IDENTITY AS 'Identity' ";
                    MyConn();
                    if (conn.State == System.Data.ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    cmd.CommandText = mysql_insert.Trim();
                    dr = cmd.ExecuteReader();
                    dr.Read();
                    Identity = long.Parse(dr[0].ToString());

                    dr.Close();
                    try
                    {
                        Get_SqlCommand_TextClear(cmd, false, Identity);
                    }
                    catch { }
                    if (!MakeTransactionFlage)
                    {
                        conn.Close();
                    }
                    return Identity;

                }
                else
                {
                    insert_me(mysql_insert, ref Identity);
                    return Identity;
                }
            }
            catch (Exception e)
            {
                Excp = -1;
                //MessageBox.Show(e.Message);
                throw (e);

            }
        }
        public int Excp = 0;
        /// <summary>
        /// Execute insert statement into table and return the last identify column data
        /// </summary>
        /// <param name="mysql_insert">insert statement into</param>
        /// <param name="Excep">Exception id</param>
        public long Insert_me_Return_Last_Identify(string mysql_insert)
        {
            long Identity = 0;
            try
            {
                if (Common.Use_Auto_Identity_Column == 1)
                {
                    Excp = 0;



                    mysql_insert = mysql_insert + "  SELECT  @@IDENTITY AS 'Identity' ";
                    //MyConn();
                    if (conn.State == System.Data.ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    cmd.CommandText = mysql_insert.Trim();

                    dr = cmd.ExecuteReader();
                    dr.Read();
                    Identity = long.Parse(dr[0].ToString());

                    dr.Close();
                    try
                    {
                        Get_SqlCommand_TextClear(cmd, false, Identity);
                    }
                    catch { }
                    if (!MakeTransactionFlage)
                    {
                        conn.Close();
                    }
                    Excp = 0;


                    return Identity;
                }
                else
                {
                    insert_me(mysql_insert, ref Identity);
                    return Identity;
                }
            }
            catch (Exception e)
            {
                Excp = 1;
                //MessageBox.Show(e.Message);
                throw (e);

            }
        }



        string SetUpInsertCmd(string InsertCmd, ref long Identity)
        {
            if (Common.Use_Auto_Identity_Column == 0)
            {
                //long Id = CallSnowflakeIdGenerate.GenerateNewId();
                InsertCmd = MySqlParser.SmartEnsureIdInInsertCommandNew(InsertCmd, ref Identity, isAutoIncrement: false);
                //string mysql=cmdd.GetFullGeneratedQuery();
            }
            return InsertCmd;
        }
        public int insert_me(string mysql_insert)
        {
            int Count = 0;


            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            cmd.CommandType = CommandType.Text;
            long Identity = 0;
            mysql_insert = SetUpInsertCmd(mysql_insert, ref Identity);
            cmd.CommandText = mysql_insert.Trim();
            ada.InsertCommand = cmd;
            Count = ada.InsertCommand.ExecuteNonQuery();
            try
            {
                Get_SqlCommand_TextClear(cmd, false);
            }
            catch { }

            if (!MakeTransactionFlage)
            {
                conn.Close();
            }
            return Count;

        }


        public int insert_me(string mysql_insert, ref long Identity)
        {
            int Count = 0;


            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            cmd.CommandType = CommandType.Text;
            mysql_insert = SetUpInsertCmd(mysql_insert, ref Identity);
            cmd.CommandText = mysql_insert.Trim();
            ada.InsertCommand = cmd;
            Count = ada.InsertCommand.ExecuteNonQuery();
            try
            {
                Get_SqlCommand_TextClear(cmd, false);
            }
            catch { }

            if (!MakeTransactionFlage)
            {
                conn.Close();
            }
            return Count;

        }


        public int insert_me_SP(string SP_Name)
        {
            int Count = 0;
            //try
            //{

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = SP_Name.Trim();
            ada.InsertCommand = cmd;
            Count = ada.InsertCommand.ExecuteNonQuery();
            try
            {
                Get_SqlCommand_TextClear(cmd, false);
            }
            catch { }

            if (!MakeTransactionFlage)
            {
                conn.Close();
            }
            return Count;
            //}
            //catch(Exception ee)
            //{

            //return Count;
            //}

        }
        public int insert_me_SP(string SP_Name, string Out_P, ref long ReturnValue)
        {
            int Count = 0;
            //try
            //{

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = SP_Name.Trim();
            ada.InsertCommand = cmd;
            Count = ada.InsertCommand.ExecuteNonQuery();
            try
            {
                ReturnValue = long.Parse(cmd.Parameters["@" + Out_P + ""].Value.ToString());
                Get_SqlCommand_TextClear(cmd, false);
            }
            catch { }

            if (!MakeTransactionFlage)
            {
                conn.Close();
            }
            return Count;
            //}
            //catch(Exception ee)
            //{

            //return Count;
            //}

        }
        static string GetMethodList()
        {
            StackTrace trace = new StackTrace();
            int caller = 0;
            string MethodName = "";
            for (int x = 0; x < 6; x++)
            {
                string fileName = "";
                StackFrame frame = trace.GetFrame(x);
                if (frame == null)
                {
                    continue;
                }
                try
                {
                    fileName = frame.GetFileName();
                }
                catch (Exception ee)
                {

                }
                try
                {
                    MethodName = MethodName + " - " + fileName + " >> " + frame.GetMethod().Name;
                }
                catch (Exception rr)
                {
                    break;
                }

            }

            return MethodName;
        }

        string GetTableName(string cmd)
        {
            string TableName = "";
            cmd = cmd.Trim();
            string[] str = cmd.Split(' ');

            if (str[0].ToLower() == "update")
            {
                TableName = str[1];

            }
            if (str[0].ToLower() == "insert")
            {
                TableName = str[2];

            }
            if (str[0].ToLower() == "delete")
            {
                TableName = str[2];

            }

            return TableName;

        }
        void Get_SqlCommand_TextClear(SqlCommand cmdd, bool IsQuerySelect, long Identity = 0)
        {
            if (Common.Is_Save_DataDictionaryDbSyncLogs == 1 & IsQuerySelect == false)
            {
                var syncHelper = new DataDictionaryDbSyncLogsAdoCls();
                syncHelper.CreateSyncLog(cmd, conn, tns);

                //string mysql=cmdd.GetFullGeneratedQuery();
            }


            if (Common.Is_Save_DataDictionary == 0)
            {
                return;
            }
            try
            {
                string MethodName = GetMethodList();
                string TableName = GetTableName(cmdd.CommandText);


                string SqlCommand_TextClear = "";
                SqlCommand_TextClear = cmdd.CommandText;
                if (cmdd.Parameters.Count > 0)
                {
                    foreach (SqlParameter param in cmdd.Parameters)
                    {
                        string ParmValue = param.Value.ToString();
                        string paramName = param.ParameterName.ToString();
                        if (param.DbType == DbType.String)
                        {
                            ParmValue = "'" + ParmValue + "'";
                        }
                        if (param.DbType == DbType.Date | param.DbType == DbType.DateTime | param.DbType == DbType.Time)
                        {
                            try
                            {
                                DateTime dat = DateTime.Parse(ParmValue);
                                ParmValue = operations.Get_DateTimeToInsert(dat);
                            }
                            catch
                            {
                                ParmValue = "'" + ParmValue + "'";
                            }
                        }
                        SqlCommand_TextClear = SqlCommand_TextClear.Replace(paramName, ParmValue);
                    }
                }

                if (Identity > 0)
                {
                    SqlCommand_TextClear = Identity.ToString() + "  / " + SqlCommand_TextClear;
                }


                string mysql = "";
                //  mysql = "SELECT MAX(ID) AS ID FROM DataDictionary ";
                //cmdd.CommandText = mysql;
                //DataTable dt_Table = new DataTable();
                //cmdd.CommandText = mysql;
                //ada.SelectCommand = cmdd;

                //ada.Fill(dt_Table);


                //long Unique_Id = 0;
                //try
                //{
                //    Unique_Id = long.Parse(dt_Table.Rows[0]["ID"].ToString());
                //    Unique_Id = Unique_Id + 1;
                //}
                //catch
                //{
                //    Unique_Id = 1;
                //}

                mysql = "Insert Into DataDictionary( SQLText,Read_Flage,User_Id,Insert_ServerDateTime,MethodName,TableName) Values(@SqlCommand_TextClear__100__,0," + Common.User_Id + "," + operations.Get_DateTimeToInsert(DateTime.Now) + ",'" + MethodName + "','" + TableName + "')";

                //mysql = "Insert Into DataDictionary(ID,SQLText,Read_Flage,User_Id,Insert_ServerDateTime) Values(@Unique_Id__100__,@SqlCommand_TextClear__100__,0,"+Common.User_Id+","+operations.Get_DateTimeToInsert(DateTime.Now)+")";
                cmdd.CommandText = mysql;
                //cmdd.Parameters.Clear();
                try
                {
                    //cmdd.Parameters.AddWithValue("@Unique_Id__100__", Unique_Id);
                    cmdd.Parameters.AddWithValue("@SqlCommand_TextClear__100__", SqlCommand_TextClear);
                }
                catch { }

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                    cmdd.ExecuteNonQuery();
                    conn.Close();
                }
                else
                {
                    cmdd.ExecuteNonQuery();
                }



                try
                {
                    //cmdd.Parameters.RemoveAt("@Unique_Id__100__");
                    cmdd.Parameters.RemoveAt("@SqlCommand_TextClear__100__");

                }
                catch (Exception eee)
                {

                }
                try
                {
                    //cmdd.Parameters.RemoveAt("@Unique_Id__100__");
                    cmdd.Parameters.Clear();

                }
                catch (Exception eee)
                {

                }
            }
            catch (Exception ee)
            {
                string msg = ee.Message;
                try
                {
                    //cmdd.Parameters.RemoveAt("@Unique_Id__100__");
                    cmdd.Parameters.RemoveAt("@SqlCommand_TextClear__100__");

                }
                catch (Exception eee)
                {

                }
                try
                {
                    //cmdd.Parameters.RemoveAt("@Unique_Id__100__");
                    cmdd.Parameters.Clear();

                }
                catch (Exception eee)
                {

                }
            }
        }
        public static void DataDictionaryLogerException(UnhandledExceptionEventArgs ee)
        {

        }
        public static void DataDictionaryLogerException(Exception ee)
        {
            if (Common.Is_Save_DataDictionaryLogerException == 0)
            {
                return;
            }
            string InerMessage = "";
            try
            {

                if (ee.InnerException != null)
                {
                    if (ee.InnerException.InnerException != null)
                    {
                        InerMessage = ee.InnerException.InnerException.Message;
                    }
                }
                else
                {
                    InerMessage = ee.Message;
                }

            }
            catch (Exception eee)
            {
                InerMessage = ee.Message;
            }
            try
            {
                operations opp = new operations();
                DataDictionaryLogerException(opp, ee, InerMessage);
            }
            catch (Exception)
            {

                //throw;
            }

        }
        static void DataDictionaryLogerException(operations opp, Exception ee, string InnerException)
        {
            var task1 = Task.Run(() =>
            {
                opp.BeginTransaction(2);
                TaskDataDictionaryLogerException(opp, ee, InnerException);

            });

        }
        static void TaskDataDictionaryLogerException(operations opp, Exception ee, string InnerException)
        {
            try
            {
                string MethodName = GetMethodList();
                DataDictionaryLogerExceptionCLS dl = new DataDictionaryLogerExceptionCLS();
                dl.AddException(ref opp, ee, MethodName, InnerException);
                opp.Commit();
            }
            catch (Exception)
            {
                opp.Rollback();
            }
        }
        public void update_me(string mysql_up, ref int Excep)
        {
            try
            {
                insert_me(mysql_up);
                Excep = 0;
            }
            catch
            {
                Excep = -1;
            }
            //try
            //{
            //    if (conn.State == System.Data.ConnectionState.Closed)
            //    {
            //        conn.Open();
            //    }
            //    //mysql_up="update po_invoice_msg_from_dist set dist_seen=1 where id="+Session["id"] ;

            //    cmd.CommandText = mysql_up.Trim();
            //    ada.UpdateCommand = cmd;
            //    ada.UpdateCommand.ExecuteNonQuery();
            //    Excep = 0;
            //    conn.Close();
            //    try
            //    {
            //        Get_SqlCommand_TextClear(cmd   );
            //    }
            //    catch { }
            //}
            //catch (Exception e)
            //{
            //    Excep = -1;
            //    throw (e);

            //}
        }



        public void update_me(string mysql_up)
        {
            try
            {
                insert_me(mysql_up);
            }
            catch (Exception e)
            {
                //Excep=-1;
                throw (e);

            }


        }
        /// <summary>
        /// تنفيذ قائمة من الأوامر على قاعدة البيانات
        /// </summary>
        /// <param name="mysql_insert"></param>
        /// <returns>one if success or zero if failed</returns>
        public int insert_me_transaction(System.Collections.ArrayList mysql_insert)
        {
            int my_status;

            //SqlTransaction tns;
            //OleDbTransaction tns;
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            tns = conn.BeginTransaction();
            //cmd.Connection=conn;
            cmd.Transaction = tns;

            try
            {
                foreach (string x in mysql_insert)
                {
                    long Identity = 0;
                    string mysql = SetUpInsertCmd(x, ref Identity);
                    cmd.CommandText = mysql.Trim();
                    ada.InsertCommand = cmd;
                    ada.InsertCommand.ExecuteNonQuery();

                    try
                    {
                        Get_SqlCommand_TextClear(cmd, false);
                    }
                    catch { }
                }
                tns.Commit();
                my_status = 1;
            }
            catch (Exception e)
            {
                tns.Rollback();
                my_status = 0;
                Msg = e.Source + " \n" + e.Message.ToString();

            }
            finally
            {
                conn.Close();
            }
            return my_status;

        }

        //checked if sql no=mysql_No_Checked executed or not


        //checked if sql no=mysql_No_Checked executed or not
        public int insert_me_transaction(ref int Excp, int mysql_No_Checked, System.Collections.ArrayList mysql_insert)
        {
            int my_status = 0;
            int mysql_No_Status;
            int Counter = 0;

            //SqlTransaction tns;
            //OleDbTransaction tns;

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            tns = conn.BeginTransaction();
            //cmd.Connection=conn;
            cmd.Transaction = tns;

            try
            {
                string mysql = "";
                foreach (string x in mysql_insert)
                {
                    long Identity = 0;
                    mysql = SetUpInsertCmd(x, ref Identity);
                    Counter++;

                    cmd.CommandText = mysql.Trim();
                    ada.InsertCommand = cmd;
                    mysql_No_Status = ada.InsertCommand.ExecuteNonQuery();
                    if (mysql_No_Checked == Counter)
                    {
                        if (mysql_No_Status == 0)
                        {
                            Excp = 1000;
                            break;
                        }
                    }

                    try
                    {
                        Get_SqlCommand_TextClear(cmd, false);
                    }
                    catch { }

                }
                tns.Commit();
                my_status = 1;
            }
            catch (Exception e)
            {
                tns.Rollback();
                my_status = 0;
                Excp = 1;
                Msg = e.Source + " \n" + e.Message.ToString();
                //MessageBox.Show(Msg);
                throw (e);
            }
            finally
            {
                conn.Close();
            }
            return my_status;

        }



        public void delete_me(string mysql_up)
        {
            try
            {
                insert_me(mysql_up);
            }
            catch (Exception ee)
            {
                string msg = "خطأ فى تنفيذ العملية بسبب  ";
                msg = msg + ee.Message;
                //MessageBox.Show(msg);
            }

        }
        public void delete_me(string mysql_up, ref int Excp)
        {
            try
            {
                insert_me(mysql_up);
                Excp = 0;
            }
            catch
            {
                Excp = 1;
            }

        }

        public DataSet return_dataset(ref int Excp, string mysql, string TableName)
        {
            try
            {
                Excp = 0;
                return return_dataset(mysql, TableName);
            }
            catch (Exception ee)
            {
                Excp = 1;
                //MessageBox.Show(ee.Message);
                return ds;
            }

        }

        public DataSet return_dataset(string mysql, string TableName = "")
        {
            ds = new DataSet();
            try
            {

                // ada = new OleDbDataAdapter(mysql, conn);
                cmd.CommandText = mysql;
                ada.SelectCommand = cmd;
                ds.Clear();
                if (TableName == "")
                {
                    ada.Fill(ds);
                }
                else
                {
                    ada.Fill(ds, TableName);
                }
                try
                {
                    Get_SqlCommand_TextClear(cmd, true);
                }
                catch { }

                try
                {
                    int x = ds.Tables[0].Rows.Count;
                }
                catch (Exception ee)
                {

                }
                return ds;
            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
                return ds;
            }




        }


        public DataSet return_dataset(string mysql)
        {
            ds = new DataSet();

            try
            {

                return return_dataset(mysql, "");


            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
                return ds;
            }



        }
        public DataTable return_DataTable(string mysql)
        {

            DataTable dt = new DataTable();
            try
            {

                // ada = new OleDbDataAdapter(mysql, conn);
                cmd.CommandText = mysql;
                ada.SelectCommand = cmd;
                ds.Clear();
                ada.Fill(dt);
                int C = dt.Rows.Count;
                try
                {
                    //string searchName = "goo";

                    //// 1. Exact match (case-insensitive by default in DataTable)
                    //DataRow[] exactMatches = dt.Select($"Name = '{searchName}'");

                    //// 2. Partial match (Contains "John")
                    //DataRow[] partialMatches = dt.Select($"Name LIKE '%{searchName}%'");

                    //// Process search results
                    //foreach (DataRow row in partialMatches)
                    //{
                    //    string Name = row["Name"].ToString();
                    //}
                    Get_SqlCommand_TextClear(cmd, true);
                }
                catch { }

                return dt;
            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message);
                return dt;
            }
        }
        public DataTable return_DataTableWithBuilder(string mysql)
        {
            DataTable dt = new DataTable();

            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                //conn.Open();

                ada = new SqlDataAdapter(mysql, conn);
                combd = new SqlCommandBuilder(ada);


                ada.Fill(dt);
                int c = dt.Rows.Count;
                try
                {
                    Get_SqlCommand_TextClear(cmd, true);
                }
                catch { }
                return dt;
            }
            catch (Exception ee)
            {

                return dt;
            }
        }

        public SqlDataReader ReturnDataReader(string mysql_insert)
        {
            try
            {
                long Identity = 0;

                MyConn();
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                cmd.CommandText = mysql_insert.Trim();
                dr = cmd.ExecuteReader();

                return dr;
            }
            catch (Exception e)
            {

                //MessageBox.Show(e.Message);
                throw (e);

            }
        }

        //public void Dispose()
        //{
        //    //throw new NotImplementedException();
        //}
        ////////////////////////////////****************************************************
        // ==========================================
        // تنفيذ نمط IDisposable الصحيح
        // ==========================================
        // متغير لمتابعة هل تم التخلص من الموارد أم لا
        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // منع الـ Garbage Collector من استدعاء Finalizer مرتين
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // التخلص من الموارد المدارة (Managed Resources) مثل الـ DbContext
                    if (db != null)
                    {
                        db.Dispose();
                        db = null;
                    }
                }

                _disposed = true;
            }
        }

        // Finalizer احتياطي في حال تم نسيان استخدام using
        ~operations()
        {
            Dispose(false);
        }


    }
}