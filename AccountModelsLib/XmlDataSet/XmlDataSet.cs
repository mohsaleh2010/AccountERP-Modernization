using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace AccountModelsLib
{
    public class XmlDataSet
    {
        public XmlDataSet()
        {

        }
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
                        AccountModelsLib.operations.DabaseName = dss.Tables["AuthenticationInfo"].Rows[x]["DataBaseName"].ToString();
                        AccountModelsLib.operations.DbServerName = dss.Tables["AuthenticationInfo"].Rows[x]["DBSERVER"].ToString();
                        AccountModelsLib.operations.Username = dss.Tables["AuthenticationInfo"].Rows[x]["USN"].ToString();
                        AccountModelsLib.operations.Password = dss.Tables["AuthenticationInfo"].Rows[x]["PW"].ToString();
                        AccountModelsLib.operations.Used = dss.Tables["AuthenticationInfo"].Rows[x]["Used"].ToString();
                        AccountModelsLib.operations.DataBaseType = dss.Tables["AuthenticationInfo"].Rows[0]["DataBaseType"].ToString();
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
                    Common.VersionWillExpire = int.Parse(en.decrypt(dss.Tables["AuthenticationInfo"].Rows[0]["STS"].ToString()));
                }
                catch { }
                try
                {
                    if (Common.VersionWillExpire == 1)
                    {
                        Common.dtExpire = DateTime.Parse(en.decrypt(dss.Tables["AuthenticationInfo"].Rows[0]["DT"].ToString()));
                    }
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
            AccountModelsLib.operations.conn_str_ = ConStren;
            return ConStren;

        }
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


        public static string ReadLoaclXmlDataInfo()
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

    }
}
