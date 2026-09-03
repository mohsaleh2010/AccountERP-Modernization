using IdGen;
using Microsoft.EntityFrameworkCore;
 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace AccountModelsLib.Models
{
    [NotMapped]
    public class Submain_AccountCLS
    {
        public Submain_Account sb = new Submain_Account();

        public Submain_AccountCLS()
        {

        }


        public DataTable dt = new DataTable();


        public DataTable GetMainSubmain_Account(ref operations opp, long Submain_Account_ID)
        {

            ///التاكد من الحساب ليس له فروع
            string mysql = "Select * from Submain_Account  Where Main_Account_Id=" + Submain_Account_ID;
            return opp.return_DataTable(mysql);
        }
        public DataTable GetSubmain_Account(ref operations opp, long Submain_Account_ID)
        {

            ///التاكد من الحساب ليس له فروع
            string mysql = "Select * from Submain_Account  Where  Id=" + Submain_Account_ID;
            return opp.return_DataTable(mysql);
        }
        public Submain_Account  GetSubmain_AccountEFAsNoTracking(ref operations opp, long SubAccount_Id)
        {

            ///التاكد من الحساب ليس له فروع
            sb = opp.db.Submain_Account.AsNoTracking().Where(a=>a.Id== SubAccount_Id).FirstOrDefault();
            return sb;
        }
        public DataTable GetSubmain_Account(ref AccountModelsLib.operations opp, int AccountType)
        {
             
            DataTable dt = new DataTable();
            string mysql = "  SELECT * FROM Submain_Account WHERE (Account_Customer_Type_Value = " + AccountType + ")";
            dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count == 0)
            {
                //Sub_Account sb = new Sub_Account();
                try
                {
                     AddSbuAccountFrom_Account_TypeAutomatic(ref opp, AccountType);
                }
                catch (Exception ee) { }
                dt = new DataTable();
                dt = opp.return_DataTable(mysql);

            }
            return dt;

        }
        public void AddSbuAccountFrom_Account_TypeAutomatic(ref operations opp, int Account_Customer_Type_Value)
        {
            string mysql = "";

            mysql = "Select * from Account_Type Where Value=" + Account_Customer_Type_Value;
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count == 0)
            {
                return;
            }

            int Account_Follows = 4;
            int Head_Main_Account = 1;
            string Account_Type_Name = dt.Rows[0]["Name"].ToString();
            Account_Type_Name = Account_Type_Name + " (إضافة اّلية) ";
            try
            {
                Account_Follows = int.Parse(dt.Rows[0]["Account_Follows"].ToString());
            }
            catch { }
            try
            {
                Head_Main_Account = int.Parse(dt.Rows[0]["Head_Main_Account"].ToString());
            }
            catch { }

         long   Main_Account_Id = GetMain_Account_Id(ref opp, Account_Customer_Type_Value, Head_Main_Account);
            Submain_AccountCLS sa = new Submain_AccountCLS();
            sa.sb.Is_MasrofatAffect = 0;


            sa.sb.FirstSubCode = "01";
            sa.sb.Code = 0;
            sa.sb.Account_Level = 2;
            sa.sb.lastPartIncrease = 0;
            sa.sb.Main_Account_Id = Main_Account_Id;
             
            sa.sb.SubAccount_NO = "";
            sa.sb.SubAccount_Name = Account_Type_Name;
            sa.sb.Head_Main_Account = 1;

            sa.sb.Currency_Id = 1;
            sa.sb.CanDelete = 0;
            sa.sb.Account_Follows = Account_Follows;
            sa.sb.Notes = "";
            sa.sb.Accept_EntryNew_Flage = 0;
            sa.sb.Accept_Cost_Center_Flage = 0;
            sa.sb.Cost_Center_Tree_Id = 0;
            sa.sb.Account_Nature_Type = 1;
            sa.sb.Account_Customer_Type_Value = Account_Customer_Type_Value;

            sa.sb.IS_SubAccount_AccountType = 0;
            sa.sb.IS_SubAccount_FollowingAccount = 0;
            sa.sb.Branch_Id = Common.Branch_Id;

            sa.InsertSubmain_Account(ref opp);
        }
        long GetMain_Account_Id(ref operations opp, int Account_Customer_Type_Value, int Head_Main_Account)
        {
            string mysql = "";
            mysql = "Select * from Submain_Account Where Main_Account_Id = 0 And Head_Main_Account =" + Head_Main_Account + "";
            DataTable dtSubmain_Account = opp.return_DataTable(mysql);
            long Main_Account_Id = 0;
            if (dtSubmain_Account.Rows.Count > 0)
            {
                Main_Account_Id = long.Parse(dtSubmain_Account.Rows[0]["Id"].ToString());
            }

            return Main_Account_Id;
        }
        public void Get_Submain_Account(long SubAccount_Id)
        {
            operations opp = new operations();
            Get_Submain_Account(ref opp, SubAccount_Id);
        }
        public void Get_Submain_Account(long Account_Customer_Type_ID, int Account_Customer_Type_Value)
        {
            operations opp = new operations();
            //string mysql = " Select * from Submain_Account Where Account_Customer_Type_ID =" + Account_Customer_Type_ID + " And Account_Customer_Type_Value=" + Account_Customer_Type_Value + "";
            //dt = opp.return_DataTable(mysql);

            sb = opp.db.Submain_Account.AsNoTracking().Where(a => a.Account_Customer_Type_ID == Account_Customer_Type_ID && a.Account_Customer_Type_Value == Account_Customer_Type_Value).FirstOrDefault();

        }
        public Submain_Account  Get_Submain_Account(ref operations opp, long SubAccount_Id)
        {
            //operations opp = new operations();
            //string mysql = " Select * from Submain_Account Where Account_Customer_Type_ID =" + Account_Customer_Type_ID + " And Account_Customer_Type_Value=" + Account_Customer_Type_Value + "";
            //dt = opp.return_DataTable(mysql);

            sb = opp.db.Submain_Account.AsNoTracking().Where(a => a.Id == SubAccount_Id).FirstOrDefault();
            return sb;

            //string mysql = " Select * from Submain_Account Where Id =" + SubAccount_Id;
            //dt = opp.return_DataTable(mysql);

            //if (dt.Rows.Count > 0)
            //{
            //    try
            //    {
            //        ReadData(dt, 0);
            //    }
            //    catch (Exception ee)
            //    {


            //    }
            //}

        }
        public void ReadData(DataTable dt, int index)
        {
            //sb.SubAccount_Id = long.Parse(dt.Rows[index]["SubAccount_Id"].ToString());
            //sb.SubAccount_Name = dt.Rows[index]["SubAccount_Name"].ToString();



            //try
            //{
            //    sb.Account_Customer_Type_Value = int.Parse(dt.Rows[index]["Account_Customer_Type_Value"].ToString());
            //}
            //catch (Exception)
            //{

            //    sb.Account_Customer_Type_Value = 0;
            //}

            //try
            //{
            //    sb.Account_Customer_Type_ID = long.Parse(dt.Rows[index]["Account_Customer_Type_ID"].ToString());
            //}
            //catch (Exception)
            //{

            //    sb.Account_Customer_Type_ID = 0;
            //}
            //sb.Currency_Id = int.Parse(dt.Rows[index]["Currency_Id"].ToString());
            //sb.Main_Account_Id = long.Parse(dt.Rows[index]["Main_Account_Id"].ToString());
            //sb.lastPartIncrease = long.Parse(dt.Rows[index]["lastPartIncrease"].ToString());
            //sb.Account_Level = int.Parse(dt.Rows[index]["Account_Level"].ToString());
            //sb.Code = long.Parse(dt.Rows[index]["Code"].ToString());
            //sb.FirstSubCode = dt.Rows[index]["FirstSubCode"].ToString();
            //sb.SubAccount_NO = dt.Rows[index]["SubAccount_NO"].ToString();
            //sb.Account_Code = dt.Rows[index]["Account_Code"].ToString();
            //sb.Head_Main_Account = int.Parse(dt.Rows[index]["Head_Main_Account"].ToString());

            //sb.CanDelete = int.Parse(dt.Rows[index]["CanDelete"].ToString());
            //sb.Account_Follows = int.Parse(dt.Rows[index]["Account_Follows"].ToString());
            //sb.Notes = dt.Rows[index]["Notes"].ToString();

            //sb.Accept_EntryNew_Flage = int.Parse(dt.Rows[index]["Accept_EntryNew_Flage"].ToString());
            //sb.Accept_Cost_Center_Flage = int.Parse(dt.Rows[index]["Accept_Cost_Center_Flage"].ToString());
            //sb.Cost_Center_Tree_Id = long.Parse(dt.Rows[index]["Cost_Center_Tree_ID"].ToString());
            //sb.Account_Nature_Type = int.Parse(dt.Rows[index]["Account_Nature_Type"].ToString());

            //IS_Last_Level = int.Parse(dt.Rows[index]["IS_Last_Level"].ToString());
            //IS_SubAccount_AccountType = int.Parse(dt.Rows[index]["IS_SubAccount_AccountType"].ToString());
            //IS_SubAccount_FollowingAccount = int.Parse(dt.Rows[index]["IS_SubAccount_FollowingAccount"].ToString());
            //Branch_Id = int.Parse(dt.Rows[index]["Branch_Id"].ToString());
            //Is_MasrofatAffect = int.Parse(dt.Rows[index]["Is_MasrofatAffect"].ToString());
            //AccountCode1 = long.Parse(dt.Rows[index]["AccountCode1"].ToString());

            //AccountCode2 = long.Parse(dt.Rows[index]["AccountCode2"].ToString());
            //AccountCode3 = long.Parse(dt.Rows[index]["AccountCode3"].ToString());
            //AccountCode4 = long.Parse(dt.Rows[index]["AccountCode4"].ToString());
            //AccountCode5 = long.Parse(dt.Rows[index]["AccountCode5"].ToString());
            //AccountCode6 = long.Parse(dt.Rows[index]["AccountCode6"].ToString());

        }
        public void InsertSubmain_Account(ref operations opp)
        {
            
            if (sb.Id > 0)
            {
                Get_Submain_Account(ref opp, sb.Id);
            }
            else
            {
                string mysql = "";


                int AccountCode1 = 0;
                int AccountCode2 = 0;
                int AccountCode3 = 0;
                int AccountCode4 = 0;
                int AccountCode5 = 0;
                int AccountCode6 = 0;
                GetAccountCodeParts(sb.SubAccount_NO, ref AccountCode1, ref AccountCode2, ref AccountCode3, ref AccountCode4, ref AccountCode5, ref AccountCode6);


                if (Common.Use_AutoNumeric_Submain_Account_Id == 0 | Common.Use_Auto_Identity_Column == 0)
                {
                    long Submain_Account_Id = 0;
                    Submain_Account_Id = CallSnowflakeIdGenerate.GenerateNewId();

                  //  Common.GETAutoNumber(ref opp, ref Submain_Account_Id);
                    sb.Id = Submain_Account_Id;
                    sb.Account_Code = sb.Account_Code ?? "0";
                    sb.AccountCode1 = sb.AccountCode1 ?? 0;
                    sb.AccountCode3 = sb.AccountCode3 ?? 0;
                    sb.AccountCode5=sb.AccountCode5 ?? 0;
                    sb.AccountCode2=sb.AccountCode2 ?? 0;
                    sb.AccountCode4=sb.AccountCode4 ?? 0;
                    sb.AccountCode6 = sb.AccountCode6 ?? 0;
                    //mysql = " INSERT INTO Submain_Account (Id,FirstSubCode,Code,Account_Level,lastPartIncrease,Main_Account_Id, SubAccount_Name,SubAccount_NO,Account_Code,Head_Main_Account,    Currency_Id,CanDelete,Account_Follows, Notes,Accept_EntryNew_Flage, Accept_Cost_Center_Flage, Cost_Center_Tree_ID, Account_Nature_Type,Account_Customer_Type_Value,Account_Customer_Type_ID,IS_Last_Level,IS_SubAccount_AccountType,IS_SubAccount_FollowingAccount,Branch_Id,AccountCode1, AccountCode2, AccountCode3, AccountCode4, AccountCode5,AccountCode6,Is_MasrofatAffect) ";
                    //mysql = mysql + " VALUES (" + sb.Id + ",'" + sb.FirstSubCode + "'," + sb.Code + "," + sb.Account_Level + "," + sb.lastPartIncrease + "," + sb.Main_Account_Id + ",'" + sb.SubAccount_Name + "','" + sb.SubAccount_NO + "','" + sb.SubAccount_NO + "'," + sb.Head_Main_Account + ", " + sb.Currency_Id + "," + sb.CanDelete + "," + sb.Account_Follows + ",'" + sb.Notes + "'," + sb.Accept_EntryNew_Flage + ", " + sb.Account_Follows + "," + sb.Accept_Cost_Center_Flage + ", " + sb.Account_Nature_Type + " ," + sb.Account_Follows + "," + sb.Account_Customer_Type_Value + "," + sb.Account_Follows + "," + sb.Account_Customer_Type_ID + "," + sb.Account_Follows + "," + sb.IS_SubAccount_FollowingAccount + "," + sb.Account_Follows + "," + sb.AccountCode1  + "," + sb.Account_Follows + "," + sb.AccountCode3  + "," + sb.Account_Follows + "," + sb.AccountCode5  + "," + sb.Is_MasrofatAffect + ")";
                    //opp.insert_me(mysql);
                    

                }
                else
                {
                    //mysql = " INSERT INTO Submain_Account (FirstSubCode,Code,Account_Level,lastPartIncrease,Main_Account_Id, SubAccount_Name,SubAccount_NO,Account_Code,Head_Main_Account,    Currency_Id,CanDelete,Account_Follows, Notes,Accept_EntryNew_Flage, Accept_Cost_Center_Flage, Cost_Center_Tree_ID, Account_Nature_Type,Account_Customer_Type_Value,Account_Customer_Type_ID,IS_Last_Level,IS_SubAccount_AccountType,IS_SubAccount_FollowingAccount,Branch_Id,AccountCode1, AccountCode2, AccountCode3, AccountCode4, AccountCode5,AccountCode6,Is_MasrofatAffect) ";
                    //mysql = mysql + " VALUES ('" + sb.FirstSubCode + "'," + sb.Code + "," + sb.Account_Level + "," + sb.lastPartIncrease + "," + sb.Main_Account_Id + ",'" + sb.SubAccount_Name + "','" + sb.SubAccount_NO + "','" + sb.SubAccount_NO + "'," + sb.Head_Main_Account + ", " + sb.Currency_Id + "," + sb.CanDelete + "," + sb.Account_Follows + ",'" + sb.Notes + "'," + sb.Accept_EntryNew_Flage + ", " + sb.Accept_Cost_Center_Flage + "," + sb.Cost_Center_Tree_Id + ", " + sb.Account_Nature_Type + " ," + sb.Account_Customer_Type_Value + "," + sb.Account_Customer_Type_ID + "," + sb.Account_Customer_Type_ID + "," + sb.IS_SubAccount_AccountType + "," + sb.IS_SubAccount_FollowingAccount + "," + sb.Branch_Id + "," + sb.AccountCode1 + "," + sb.AccountCode2 + "," + sb.AccountCode3 + "," + sb.AccountCode4 + "," + sb.AccountCode5 + "," + sb.AccountCode6 + "," + sb.Is_MasrofatAffect + ")";
                    //sb.Id = opp.Insert_me_Return_Last_Identify(mysql);
                }
                opp.db.Submain_Account.Add(sb);


                List<Submain_Account> subList = opp.db.Submain_Account.Where(s => s.Main_Account_Id!= 0 & s.Account_Customer_Type_Value != Common.Account_Type_ForOther & s.Id == sb.Main_Account_Id).ToList();
                foreach (var sub in subList)
                {
                    sub.IS_Last_Level = 0;
                    sub.Account_Customer_Type_Value = sb.Account_Customer_Type_Value;
                }
                opp.db.SaveChanges();
                //mysql = "Update Submain_Account Set IS_Last_Level=0 ,Account_Customer_Type_Value=" + sb.Account_Customer_Type_Value + " Where Main_Account_Id <> 0 And Account_Customer_Type_Value <>" + Common.Account_Type_ForOther + " AND  Id=" + sb.Main_Account_Id;
                //opp.insert_me(mysql);
            }

        }
        public void InsertSubmain_AccountAutomatic(ref operations opp)
        {
            //string mysql = "";


            //int AccountCode1 = 0;
            //int AccountCode2 = 0;
            //int AccountCode3 = 0;
            //int AccountCode4 = 0;
            //int AccountCode5 = 0;
            //int AccountCode6 = 0;
            //GetAccountCodeParts(sb.SubAccount_NO, ref AccountCode1, ref AccountCode2, ref AccountCode3, ref AccountCode4, ref AccountCode5, ref AccountCode6);

            //if (sb.Id > 0)
            //{
            //    Get_Submain_Account(ref opp, sb.Id);
            //}
            //else
            //{
            //    if (Common.Use_AutoNumeric_Submain_Account_Id == 0 | Common.Use_Auto_Identity_Column == 0)
            //    {
            //        long Submain_Account_Id = 0;
            //        Common.GETAutoNumber(ref opp, ref Submain_Account_Id);
            //        sb.Id = Submain_Account_Id;

            //        mysql = " INSERT INTO Submain_Account ( Id,FirstSubCode,Code,Account_Level,lastPartIncrease,Main_Account_Id, SubAccount_Name,SubAccount_NO,Account_Code,Head_Main_Account,    Currency_Id,CanDelete,Account_Follows, Notes,Accept_EntryNew_Flage, Accept_Cost_Center_Flage, Cost_Center_Tree_ID, Account_Nature_Type,Account_Customer_Type_Value,Account_Customer_Type_ID,IS_Last_Level,IS_SubAccount_AccountType,IS_SubAccount_FollowingAccount,Branch_Id,AccountCode1, AccountCode2, AccountCode3, AccountCode4, AccountCode5,AccountCode6,Is_MasrofatAffect) ";
            //        mysql = mysql + " VALUES (" + sb.Id + ",'" + sb.FirstSubCode + "'," + sb.Code + "," + sb.Account_Level + "," + sb.lastPartIncrease + "," + sb.Main_Account_Id + ",'" + sb.SubAccount_Name + "','" + sb.SubAccount_NO + "','" + sb.SubAccount_NO + "'," + sb.Head_Main_Account + ", " + sb.Currency_Id + "," + sb.CanDelete + "," + sb.Account_Follows + ",'" + sb.Notes + "'," + sb.Accept_EntryNew_Flage + ", " + sb.Accept_Cost_Center_Flage + "," + sb.Cost_Center_Tree_Id + ", " + sb.Account_Nature_Type + " ," + sb.Account_Customer_Type_Value + "," + sb.Account_Customer_Type_ID + "," + sb.Account_Customer_Type_ID + "," + sb.IS_SubAccount_AccountType + "," + sb.IS_SubAccount_FollowingAccount + "," + sb.Branch_Id + "," + sb.AccountCode1 + "," + sb.AccountCode2 + "," + sb.AccountCode3 + "," + sb.AccountCode4 + "," + sb.AccountCode5 + "," + sb.AccountCode6 + "," + sb.Is_MasrofatAffect + ")";
            //        opp.insert_me(mysql);


            //    }
            //    else
            //    {
            //        mysql = " INSERT INTO Submain_Account (FirstSubCode,Code,Account_Level,lastPartIncrease,Main_Account_Id, SubAccount_Name,SubAccount_NO,Account_Code,Head_Main_Account,    Currency_Id,CanDelete,Account_Follows, Notes,Accept_EntryNew_Flage, Accept_Cost_Center_Flage, Cost_Center_Tree_ID, Account_Nature_Type,Account_Customer_Type_Value,Account_Customer_Type_ID,IS_Last_Level,IS_SubAccount_AccountType,IS_SubAccount_FollowingAccount,Branch_Id,AccountCode1, AccountCode2, AccountCode3, AccountCode4, AccountCode5,AccountCode6,Is_MasrofatAffect) ";
            //        mysql = mysql + " VALUES ('" + sb.FirstSubCode + "'," + sb.Code + "," + sb.Account_Level + "," + sb.lastPartIncrease + "," + sb.Main_Account_Id + ",'" + sb.SubAccount_Name + "','" + sb.SubAccount_NO + "','" + sb.SubAccount_NO + "'," + sb.Head_Main_Account + ", " + sb.Currency_Id + "," + sb.CanDelete + "," + sb.Account_Follows + ",'" + sb.Notes + "'," + sb.Accept_EntryNew_Flage + ", " + sb.Accept_Cost_Center_Flage + "," + sb.Cost_Center_Tree_Id + ", " + sb.Account_Nature_Type + " ," + sb.Account_Customer_Type_Value + "," + sb.Account_Customer_Type_ID + "," + sb.Account_Customer_Type_ID + "," + sb.IS_SubAccount_AccountType + "," + sb.IS_SubAccount_FollowingAccount + "," + sb.Branch_Id + "," + sb.AccountCode1 + "," + sb.AccountCode2 + "," + sb.AccountCode3 + "," + sb.AccountCode4 + "," + sb.AccountCode5 + "," + sb.AccountCode6 + "," + sb.Is_MasrofatAffect + ")";
            //        sb.Id = opp.Insert_me_Return_Last_Identify(mysql);
            //    }

               
            //}

        }

        public void UpdateSubmain_Account(ref operations opp)
        {
            int AccountCode1 = 0;
            int AccountCode2 = 0;
            int AccountCode3 = 0;
            int AccountCode4 = 0;
            int AccountCode5 = 0;
            int AccountCode6 = 0;
            GetAccountCodeParts(sb.SubAccount_NO, ref AccountCode1, ref AccountCode2, ref AccountCode3, ref AccountCode4, ref AccountCode5, ref AccountCode6);

            string mysql = "";

            mysql = " Update Submain_Account Set   Main_Account_Id=  " + sb.Main_Account_Id + " ,FirstSubCode=  '" + sb.FirstSubCode + "' ,Code=  '" + sb.Code + "' ,Account_Level=  " + sb.Account_Level + " ,lastPartIncrease=  " + sb.lastPartIncrease + " , SubAccount_Name=  '" + sb.SubAccount_Name + "' ,SubAccount_NO=  '" + sb.SubAccount_NO + " ',Account_Code= '" + sb.SubAccount_NO + " ' ,Head_Main_Account=  " + sb.Head_Main_Account + " ,    Currency_Id=  " + sb.Currency_Id + " ,CanDelete=  " + sb.CanDelete + " ,Account_Follows=  " + sb.Account_Follows + " , Notes= ' " + sb.Notes + "' ,Accept_EntryNew_Flage=  " + sb.Accept_EntryNew_Flage + " , Accept_Cost_Center_Flage=  " + sb.Accept_Cost_Center_Flage + " , Cost_Center_Tree_ID=  " + sb.Cost_Center_Tree_Id + " , Account_Nature_Type=  " + sb.Account_Nature_Type + " ,Account_Customer_Type_Value=  " + sb.Account_Customer_Type_Value + " ,Account_Customer_Type_ID=  " + sb.Account_Customer_Type_ID + " ,IS_Last_Level=  " + sb.IS_Last_Level + " ,IS_SubAccount_AccountType=  " + sb.IS_SubAccount_AccountType + " ,IS_SubAccount_FollowingAccount=  " + sb.IS_SubAccount_FollowingAccount + " ,Branch_Id=  " + sb.Branch_Id + " ,AccountCode1=  " + AccountCode1 + " , AccountCode2=  " + AccountCode2 + " , AccountCode3=  " + AccountCode3 + " , AccountCode4=  " + AccountCode4 + " , AccountCode5=  " + AccountCode5 + " ,AccountCode6=  " + AccountCode6 + " ,Is_MasrofatAffect=" + sb.Is_MasrofatAffect + " ";
            mysql = mysql + " Where  Id=  " + sb.Id + "";
            opp.insert_me(mysql);


            //mysql = " Update Submain_Account  Set  Is_MasrofatAffect=" + Is_MasrofatAffect + ", FirstSubCode='" + FirstSubCode + "', SubAccount_Name='" + SubAccount_Name + "',    Currency_Id=" + Currency_Id + ", Account_Follows=" + Account_Follows + ", Notes='" + Notes + "',lastPartIncrease=" + lastPartIncrease + " ";
            //mysql = mysql + ",Accept_EntryNew_Flage=" + Accept_EntryNew_Flage + ", Accept_Cost_Center_Flage=" + Accept_Cost_Center_Flage + ", Cost_Center_Tree_ID=" + Cost_Center_Tree_ID + ", Account_Nature_Type=" + Account_Nature_Type + ",Account_Customer_Type_Value=" + Account_Customer_Type_Value + "";
            //mysql = mysql + ",SubAccount_NO='" + SubAccount_NO + "'";
            //mysql = mysql + ",IS_SubAccount_FollowingAccount= " + IS_SubAccount_FollowingAccount + " ";
            //mysql = mysql + ",IS_SubAccount_AccountType= " + IS_SubAccount_AccountType + " ";
            //mysql = mysql + ", AccountCode1=" + AccountCode1 + ", AccountCode2=" + AccountCode2 + ", AccountCode3=" + AccountCode3 + ", AccountCode4=" + AccountCode4 + ", AccountCode5=" + AccountCode5 + ",AccountCode6=" + AccountCode6 + " ";
            //mysql = mysql + "  where SubAccount_Id =" + SubAccount_Id;


            mysql = "Update   Account_Type_Main_Account_Id Set  Account_Type_Value=" + sb.Account_Customer_Type_Value + " Where Main_Account_Id=" + sb.Id + "";
            opp.insert_me(mysql);



            mysql = "Update Account_Entry_Details Set Account_Name=N'" + sb.SubAccount_Name + "' Where Submain_Account_ID=" + sb.Id + "   ";
            opp.insert_me(mysql);
        }
        public void UpdateSubmain_Account(ref operations opp, string SubAccount_Name, long Account_IdMasrofat, long Account_IdEradat)
        {
            string mysql = "Update Submain_Account Set SubAccount_Name=N'" + SubAccount_Name + "'   Where  Id=" + Account_IdMasrofat + " Or  Id=" + Account_IdEradat + " ";
            opp.insert_me(mysql);
            //string mysql = "Delete from Submain_Account Where  Id=" + Account_IdMasrofat + " Or  Id=" + Account_IdEradat + " ";
            //opp.insert_me(mysql);

        }
        public int UpdateSubmain_Account(ref operations opp, string SubAccount_Name, long SubAccount_Id, long Account_Customer_Type_ID,long Account_Customer_Type_Value)
        {
     
            string mysql = " Update Submain_Account Set SubAccount_Name ='" + SubAccount_Name + "' Where  Id=" + SubAccount_Id + " And Account_Customer_Type_ID=" + Account_Customer_Type_ID + " And Account_Customer_Type_Value=" + Account_Customer_Type_Value + "";
            int C = opp.insert_me(mysql);
            return C;
        }
        public void UpdateSubmain_Account(ref operations opp, string Name_Ar, long SubAccount_Id)
        {

            //,Account_Follows=@Account_Follows
            string mysql = "Update Submain_Account Set SubAccount_Name=@SubAccount_Name   ";
            mysql = mysql + " Where  Id=@SubAccount_Id";
            opp.cmd.Parameters.AddWithValue("@SubAccount_Id", SubAccount_Id);
            opp.cmd.Parameters.AddWithValue("@SubAccount_Name", Name_Ar);
            //opp.cmd.Parameters.AddWithValue("@Account_Follows", Account_Follows);
            opp.insert_me(mysql);
        }
        public void UpdateSubmain_Account(ref operations opp, string SubAccount_Name, string SubAccount_Name_En, long Account_IdMasrofat, long Account_IdEradat)
        {

            string mysql = "Update Submain_Account Set SubAccount_Name=N'" + SubAccount_Name + "'  , SubAccount_Name_En=N'" + SubAccount_Name_En + "' Where  Id=" + Account_IdMasrofat + " Or  Id=" + Account_IdEradat + " ";
            opp.insert_me(mysql);
        }
        



        void GetAccountCodeParts(string SubAccount_NO, ref int Code1, ref int Code2, ref int Code3, ref int Code4, ref int Code5, ref int Code6)
        {

            string[] CodeArray = SubAccount_NO.Split('-');

            Code1 = (CodeArray.Length > 0 && int.TryParse(CodeArray[0]?.ToString(), out int c1)) ? c1 : 0;
            Code2 = (CodeArray.Length > 1 && int.TryParse(CodeArray[1]?.ToString(), out int c2)) ? c2 : 0;
            Code3 = (CodeArray.Length > 2 && int.TryParse(CodeArray[2]?.ToString(), out int c3)) ? c3 : 0;
            Code4 = (CodeArray.Length > 3 && int.TryParse(CodeArray[3]?.ToString(), out int c4)) ? c4 : 0;
            Code5 = (CodeArray.Length > 4 && int.TryParse(CodeArray[4]?.ToString(), out int c5)) ? c5 : 0;
            Code6 = (CodeArray.Length > 5 && int.TryParse(CodeArray[5]?.ToString(), out int c6)) ? c6 : 0;
            //try
            //{
            //      int.TryParse(CodeArray[0].ToString(), out Code1);
            //}
            //catch { }
            //try
            //{
            //      int.TryParse(CodeArray[1].ToString(), out Code2);
            //}
            //catch { }
            //try
            //{
            //       int.TryParse(CodeArray[2].ToString(), out Code3);
            //}
            //catch { }
            //try
            //{
            //      int.TryParse(CodeArray[3].ToString(), out Code4);
            //}
            //catch { }
            //try
            //{
            //      int.TryParse(CodeArray[4].ToString(), out Code5);
            //}
            //catch { }

            //try
            //{
            //       int.TryParse(CodeArray[5].ToString(), out Code6);
            //}
            //catch { }
        }
       

        public string Get_SQL_AccountsCode()
        {

            string mysql = @" SELECT DISTINCT 
                         Main_Account.SubAccount_Name + '  /  ' + Submain_Account.SubAccount_Name AS Name, 
                         Submain_Account.SubAccount_NO + '  ' + Submain_Account.SubAccount_Name AS Account_Name_No, Main_Account.SubAccount_Name AS MainAccount_Name, 
                         Main_Account.SubAccount_NO AS MainAccount_No,   
                         Main_Account.Currency_Id AS main_Currency_Id, Main_Account.Notes AS main_Notes, Main_Account.Account_Follows As Main_Account_Follows, Main_Account.CashFlowType As Main_CashFlowType
                    ,Submain_Account.*
                    FROM            Submain_Account LEFT OUTER JOIN
                         Submain_Account AS Main_Account ON Submain_Account.Main_Account_Id = Main_Account.Id
                            ";
            return mysql;
        }
        public DataTable Get_FixedAsset_Account(ref AccountModelsLib.operations opp, ref int Excp, int Branch_Id)
        {


            DataTable dt = new DataTable();
            try
            {
                string mysql = Get_SQL_AccountsCode();

                mysql = mysql + "  WHERE   (Submain_Account.Id > 0 ) ";
                mysql = mysql + "  AND  (Submain_Account.Account_Customer_Type_Value = 8) ";
                if (Branch_Id > 0)
                {
                    mysql = mysql + "  And   ((Submain_Account.Branch_Id = " + Branch_Id + ") OR (Submain_Account.Branch_Id = -1 ))";
                }

                mysql = mysql + " ORDER BY Submain_Account.Id ";

                dt = opp.return_DataTable(mysql);
                Excp = 1;

                return dt;
            }
            catch (Exception ee)
            {
                Excp = -1;
                return dt;
            }
        }
        public DataTable Get_SubMain_Account_AllAccounts(ref int Excp)
        {
            operations opp = new operations();


            DataTable dt = new DataTable();
            try
            {
                string mysql = Get_SQL_AccountsCode();

                mysql = mysql + " ORDER BY Submain_Account.Id ";// " ORDER BY Submain_Account.Head_Main_Account, Submain_Account.lastPartIncrease ";


                dt = opp.return_DataTable(mysql);
                Excp = 1;

                return dt;
            }
            catch (Exception ee)
            {
                Excp = -1;
                return dt;
            }
        }
        public DataTable get_TreeToPrint(long Branch_Id, int Account_Level)
        {



            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n
            string mysql = @" SELECT        Submain_Account.*, MainSubmain_Account.SubAccount_Name AS MainAccount_Name, MainSubmain_Account.SubAccount_NO AS MainSubAccount_NO, 
                         MainSubmain_Account.Code AS MainCode
FROM            Submain_Account LEFT OUTER JOIN
                         Submain_Account AS MainSubmain_Account ON Submain_Account.Main_Account_Id = MainSubmain_Account.Id ";
            mysql = mysql + " Where Submain_Account.Branch_Id=" + Branch_Id + " AND Submain_Account.Account_Level <= " + Account_Level + " ";

            mysql = mysql + "  ORDER BY AccountCode1, AccountCode2, AccountCode3, AccountCode4, AccountCode5, AccountCode6 ";

            //  Equivalent_First_Balance_Madin,Equivalent_First_Balance_Da2n
            mysql = @" SELECT        * from Submain_Account ";
            mysql = mysql + " Where Branch_Id=" + Branch_Id + " ";// AND Account_Level <= " + sb.Account_Level + " ";

            mysql = mysql + "  ORDER BY Head_Main_Account, AccountCode1, AccountCode2, AccountCode3, AccountCode4, AccountCode5, AccountCode6 ";

            DataTable dt = new DataTable();
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            dt = opp.return_DataTable(mysql);
            return dt;
        }
        public string Get_Submain_Account_Name(int Account_Id, ref decimal Submain_Account_Balance)
        {
            string mysql = @" SELECT DISTINCT 
                         Main_Account.SubAccount_Name + '  /  ' + Submain_Account.SubAccount_Name AS Name, 
                         Submain_Account.SubAccount_NO + '  ' + Submain_Account.SubAccount_Name AS Account_Name_No, Main_Account.SubAccount_Name AS MainAccount_Name, 
                         Main_Account.SubAccount_NO AS MainAccount_No,   
                         Main_Account.Currency_Id AS main_Currency_Id, Main_Account.Notes AS main_Notes, Submain_Account.*
FROM            Submain_Account LEFT OUTER JOIN
                         Submain_Account AS Main_Account ON Submain_Account.Main_Account_Id = Main_Account.Id Where Submain_Account.Id=" + Account_Id;
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            DataSet ds = opp.return_dataset(mysql);
            Submain_Account_Balance = 0;
            string Account_Name = "";
            if (ds.Tables[0].Rows.Count > 0)
            {
                //Submain_Account_Balance = decimal.Parse(ds.Tables[0].Rows[0]["Submain_Account_Balance"].ToString());
                Account_Name = ds.Tables[0].Rows[0]["SubAccount_Name"].ToString();
            }
            return Account_Name;
        }

        public void Edit_AccountsPosition(ref int Excp, long SubAccount_Id, long NewMain_Account_Id)
        {

            try
            {
                string mysql = " UPDATE Submain_Account SET Main_Account_Id = " + NewMain_Account_Id + " WHERE (SubAccount_Id = " + sb.Id + ") ";
                operations opp = new operations();

                opp.insert_me(mysql);

                Excp = 0;

            }
            catch (Exception e)
            {
                Excp = 1;

            }

        }
        public void Edit_AccountsName(ref int Excp, ref operations opp, long SubAccount_Id, string SubAccount_Name)
        {
            string mysql = " UPDATE Submain_Account SET SubAccount_Name = '" + SubAccount_Name + "' WHERE (SubAccount_Id = " + sb.Id + ") ";

            opp.insert_me(mysql);

            Excp = 0;


        }

        public void MakeSubNewCode_ALLCHILDE(ref int Excp, long main_account_Id)
        {
            //Submain_AccountCLS submain_AccountCLS = new Submain_AccountCLS();
            //submain_AccountCLS.MakeSubNewCode_ALLCHILDE(ref Excp, main_account_Id);

            operations opp = new operations();
            MakeSubNewCode(ref Excp, main_account_Id);
            string mysql = " SELECT   Submain_Account.* FROM Submain_Account ";
            mysql = mysql + " WHERE (Main_Account_Id = " + main_account_Id + ")";

            DataTable dt = opp.return_DataTable(mysql);
            for (int x = 0; x < dt.Rows.Count; x++)
            {
                long SubAccount_Id = long.Parse(dt.Rows[x]["Id"].ToString());
                MakeSubNewCode_ALLCHILDE(ref Excp, SubAccount_Id);
            }
        }
        public void MakeSubNewCode(ref int Excp, long main_account_Id)
        {
            string Account_Code = "";

            long lastPartIncrease = 0;
            int Account_Level = 0;

            long Code = 0;

            string mysql = "";
            int Account_Nature_Type = 1;
            int Currency_Id = 1;
            int Account_Follows = 1;
            int Account_Customer_Type_Value = 0;
            int IS_SubAccount_AccountType = 1;
            int IS_SubAccount_FollowingAccount = 1;
            int Branch_ID = Common.Branch_Id;
            ////بيانات الحساب الرئيسى لتوضع فى الفرعيات

            mysql = " SELECT   Submain_Account.* FROM Submain_Account ";
            mysql = mysql + " WHERE ( Id = " + main_account_Id + ")";
            operations opp = new operations();
            DataTable dt_Main = opp.return_DataTable(mysql);
            if (dt_Main.Rows.Count == 0)
            {
                Excp = 0;
                return;
            }
            try
            {
                Account_Nature_Type = int.Parse(dt_Main.Rows[0]["Account_Nature_Type"].ToString());
            }
            catch
            {

            }
            try
            {
                Currency_Id = int.Parse(dt_Main.Rows[0]["Currency_Id"].ToString());
            }
            catch { }
            try
            {
                Account_Follows = int.Parse(dt_Main.Rows[0]["Account_Follows"].ToString());
            }
            catch { }
            try
            {
                Account_Customer_Type_Value = int.Parse(dt_Main.Rows[0]["Account_Customer_Type_Value"].ToString());
            }
            catch { }
            try
            {
                IS_SubAccount_AccountType = int.Parse(dt_Main.Rows[0]["IS_SubAccount_AccountType"].ToString());
            }
            catch { }
            try
            {
                IS_SubAccount_FollowingAccount = int.Parse(dt_Main.Rows[0]["IS_SubAccount_FollowingAccount"].ToString());
            }
            catch { }
            try
            {
                Branch_ID = int.Parse(dt_Main.Rows[0]["Branch_ID"].ToString());
            }
            catch { }

            ////////////الحسابات الفرعيه للحساب
            mysql = " SELECT   Submain_Account.* FROM Submain_Account ";
            mysql = mysql + " WHERE (Main_Account_Id = " + main_account_Id + ")";

            DataTable dt = opp.return_DataTable(mysql);
            if (dt.Rows.Count == 0)
            {
                Excp = 0;
                return;
            }
            sb.Account_Code = sb.Account_Code ?? "";
            sb.Code = sb.Code ?? 0;
            //mysql = "UPDATE  Submain_Account SET     Code = " + sb.Code + ", Account_Code = N'" + sb.Account_Code + "', lastPartIncrease = " + sb.lastPartIncrease + ", SubAccount_NO = N'" + sb.Account_Code + "', Account_Level = " + sb.Account_Level + "";
            mysql = "UPDATE  Submain_Account SET     Code = " + Code + ", Account_Code = N'" + Account_Code + "', lastPartIncrease = " + lastPartIncrease + ", SubAccount_NO = N'" + Account_Code + "', Account_Level = " + Account_Level + ""; 
            mysql = mysql + " WHERE (Main_Account_Id = " + main_account_Id + ")";
            opp.insert_me(mysql);



            try
            {
                Excp = 0;
                string Msg = "";
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    long SubAccount_Id = long.Parse(dt.Rows[x]["Id"].ToString());
                    Common.MakeAutomatic_AccontCode(ref Excp, ref Msg, main_account_Id, ref lastPartIncrease, ref Account_Code, ref Code, ref Account_Level);
                    //mysql = "UPDATE  Submain_Account SET     Code = " + sb.Code + ", Account_Code = N'" + sb.Account_Code + "', lastPartIncrease = " + sb.lastPartIncrease + ", SubAccount_NO = N'" + sb.Account_Code + "', Account_Level = " + sb.Account_Level + "";
                    mysql = "UPDATE  Submain_Account SET     Code = " +  Code + ", Account_Code = N'" +  Account_Code + "', lastPartIncrease = " +  lastPartIncrease + ", SubAccount_NO = N'" +  Account_Code + "', Account_Level = " +  Account_Level + "";
                    mysql = mysql + " WHERE   ( Id = " + SubAccount_Id + ")";
                    opp.insert_me(mysql);


                    //ConnectWith_Gl_Flage = 1,, Last_destruction = 0, Import_Value = 0, Export_Value = 0
                    mysql = " UPDATE  Submain_Account SET ";
                    //mysql = mysql + "   Accept_Cost_Center_Flage = 0, Account_Nature_Type = " + sb.Account_Nature_Type + ",   Currency_Id = " + sb.Currency_Id + ", CanDelete = 1, Account_Follows = " + sb.Account_Follows + ", ";
                    //mysql = mysql + "      Cost_Center_Tree_ID = 0, Account_Customer_Type_Value = " + sb.Account_Customer_Type_Value + ", IS_SubAccount_AccountType = " + sb.IS_SubAccount_AccountType + ", IS_SubAccount_FollowingAccount = " + sb.IS_SubAccount_FollowingAccount + ", Transform_Type = 1, ";
                    //mysql = mysql + "  Branch_ID = " + Branch_ID + " ";
                    //mysql = mysql + " WHERE   ( Id = " + sb.Id + ")";
                    mysql = mysql + "   Accept_Cost_Center_Flage = 0, Account_Nature_Type = " +  Account_Nature_Type + ",   Currency_Id = " + Currency_Id + ", CanDelete = 1, Account_Follows = " + Account_Follows + ", ";
                    mysql = mysql + "      Cost_Center_Tree_ID = 0,  IS_SubAccount_AccountType = " + IS_SubAccount_AccountType + ", IS_SubAccount_FollowingAccount = " + IS_SubAccount_FollowingAccount + ", Transform_Type = 1, ";
                    mysql = mysql + "  Branch_ID = " + Branch_ID + " ";
                    mysql = mysql + " WHERE   ( Id = " + SubAccount_Id  + ")";
                    //Account_Customer_Type_Value = " +  Account_Customer_Type_Value + ",
                    opp.insert_me(mysql);

                }
            }
            catch (Exception ee) { }


        }
        public DataSet Get_ListSub_Account(ref int Excp, long Main_Account_Id)
        {


            DataSet G_DS = new DataSet();
            try
            {
                string mysql;
                mysql = " SELECT Submain_Account.* FROM Submain_Account ";
                mysql = mysql + " WHERE (Main_Account_Id = " + Main_Account_Id + ")  ORDER BY SubAccount_NO DESC ";

                operations opp = new operations();
                G_DS = opp.return_dataset(mysql, "Submain_Account");
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch
            {
                Excp = -1;
                return G_DS;
            }
        }

        /*
        public void Delete_AllSubAccount(ref int Excp, ref string Msg, Submain_Account sa)
        {

            try
            {
                operations opp = new operations();
                Delete_AllSubAccount(ref Excp, ref Msg, ref opp, sa);
            }
            catch (Exception ee)
            {


            }
        }

        */

        /*
        public void Delete_AllSubAccount(ref int Excp, ref string Msg, ref operations opp, Submain_Account sa)
        {
            // Get_Submain_Account(ref operations opp, long SubAccount_Id)

            string mysql = "Select * from Submain_Account Where Main_Account_Id=" + sa.Id;
            DataTable dt = opp.return_DataTable(mysql);

            if (dt.Rows.Count > 0)
            {
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    long SubAccount_Id = long.Parse(dt.Rows[x]["Id"].ToString());
                    Submain_AccountCLS sa1 = new Submain_AccountCLS();
                    sa1.Get_Submain_Account(ref opp, SubAccount_Id);
                    Delete_AllSubAccount(ref Excp, ref Msg, ref opp, sa1.sb);

                }

            }
            else
            {
                mysql = "Select * from Submain_Account Where Id=" + sa.Id;
                DataTable dt_ = opp.return_DataTable(mysql);

                if (dt_.Rows.Count > 0)
                {
                    long SubAccount_Id = long.Parse(dt_.Rows[0]["SubAccount_Id"].ToString());
                    string SubAccount_Name = dt_.Rows[0]["SubAccount_Name"].ToString();
                    Submain_AccountCLS sa11 = new Submain_AccountCLS();
                    sa11.Get_Submain_Account(ref opp, SubAccount_Id);

                    Delete_Account(ref Excp, ref Msg, sa11.sb);
                }
            }
        }

        */

        public void MakeDeleteSubmain_Account(ref operations opp, ref int Excp, ref string Msg,  long SubAccount_Id)
        {



            if (Check_Account_No_Found_OnSubAccount(ref Excp,ref opp, SubAccount_Id))
            {
                Msg = "هذا الحساب لا يمكن حذفة لأنة أستخدم فى قائمة حسابات فرعية";
                Excp = 10;
                return;
            }
            if (Check_Account_No_WithEntry_Used(ref Excp,ref opp, SubAccount_Id))
            {
                Msg = "هذا الحساب تم إستخدامه فى قيود حسابيه ولا يمكن حذفه";
                Excp = 10;
                return;
            }

            try
            {
                string mysql = "";
                mysql = mysql + " Delete from Submain_Account where  Id=" + SubAccount_Id;
                mysql = mysql + " Delete from Account_Type_Main_Account_Id where Main_Account_Id=" + SubAccount_Id;
                //mysql = mysql + " Delete from Account_Entry_Master_Balance where Submain_Account_ID=" + SubAccount_Id;
                Account_Entry_Master_BalanceCLS account_Entry_Master_BalanceCLS = new Account_Entry_Master_BalanceCLS ();
                account_Entry_Master_BalanceCLS.Delete_Account_Entry_Master_Balance_ForSubAccounts(ref opp, SubAccount_Id);

                opp.insert_me(mysql);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Msg = ee.Message;
                Excp = 1;
            }
        }
        /*
        public void Delete_Account(ref int Excp, ref string Msg, Submain_Account sa)
        {

            operations opp = new operations(2);
            try
            {
                //Delete_Account(ref Excp, ref Msg, ref opp, sa);
                if (Excp == 0)
                {
                    opp.Commit();
                    Msg = "تم حذف الحساب بنجاح";
                }
                if (Excp == 10)
                {
                    opp.Rollback();

                }
                else if (Excp == 1)
                {
                    opp.Rollback();
                    string Msg2 = "خطأ فى حذف الحساب";

                    Msg = Msg2 + "\n\r" + Msg;


                }


            }
            catch (Exception e)
            {
                opp.Rollback();
                Msg = e.Message;
                Excp = 1;

            }
        }

        void Delete_Account(ref int Excp, ref string Msg, ref operations opp, Submain_Account sa)
        {
            Excp = 0;
             


            if (Check_Account_No_Found_OnSubAccount(ref Excp, sa.Id))
            {
                Msg = "هذا الحساب لا يمكن حذفة لأنة أستخدم فى قائمة حساب فرعية";
                Excp = 10;
                return;
            }
            if (Check_Account_No_WithEntry_Used(ref Excp, sa.Id))
            {
                Msg = "هذا الحساب تم إستخدامه فى قيود حسابيه ولا يمكن حذفه";
                Excp = 10;
                return;
            }




            int Account_Customer_Type_Value = sa.Account_Customer_Type_Value;
            long? Account_Customer_Type_ID = sa.Account_Customer_Type_ID;
            if (Account_Customer_Type_ID.Value == null)
            {
                Account_Customer_Type_ID = 0;
            }
            DeleteCustomerSupplierOther(ref Excp, ref Msg, ref opp, Account_Customer_Type_Value, (long)Account_Customer_Type_ID, sa.Id);


        }
        */
        public void DeleteSubmain_Account(ref operations opp, long Account_IdMasrofat, long Account_IdEradat)
        {
            string mysql = "Delete from Submain_Account Where  Id=" + Account_IdMasrofat + " Or  Id=" + Account_IdEradat + " ";
            opp.insert_me(mysql);


        }
        public void DeleteSubmain_Account(ref operations opp, long Account_Id)
        {
            string mysql = "Delete from Submain_Account Where  Id=" + Account_Id + "  ";
            opp.insert_me(mysql);


        }

        public bool Check_Account_No_Found_OnSubAccount(ref int Excp, ref operations opp, long Main_Account_Id)
        {

            //operations opp = new operations();

            try
            {
                string mysql;
                mysql = "Select * from Submain_Account  where Main_Account_Id=" + Main_Account_Id;
                DataSet G_DS = opp.return_dataset(mysql, "Submain_Account");
                if (G_DS.Tables[0].Rows.Count > 0)
                {
                    Excp = 0;
                   
                    return true;
                }
                else
                {
                    Excp = 0;
                    
                    return false;
                }
            }
            catch
            {
                Excp = 1;
                
                return true;
            }

        }
        public bool Check_Account_No_WithEntry_Used(ref int Excp, ref operations opp, long Submain_Account_ID)
        {
            Account_Entry_DetialsCls account_Entry_DetialsCls = new Account_Entry_DetialsCls();
            return account_Entry_DetialsCls.Check_Account_No_WithEntry_Used(ref Excp,ref opp, Submain_Account_ID);

        }
    }
}
