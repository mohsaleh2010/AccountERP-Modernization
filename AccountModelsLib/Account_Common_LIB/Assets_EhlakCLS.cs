using System;
using System.Collections.Generic;
using System.Text;
 
using System.Data;
 using AccountModelsLib; using AccountModelsLib.Models;
using System.Collections;
namespace AccountModelsLib
{
    public class Assets_EhlakCLS
    {
        public Assets_EhlakCLS()
        {

        }
        public DataTable Get_AssetsNotAssignEhlakForPeriod(ref AccountModelsLib.operations opp,long Branch_Id, DateTime DateFrom, DateTime DateTo)
        {
            return Get_AssetsNotAssignEhlakForPeriod_(ref opp,   Branch_Id,DateFrom, DateTo);
        }

        public DataTable Get_AssetsNotAssignEhlakForPeriod(long Branch_Id, DateTime DateFrom, DateTime DateTo)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            return Get_AssetsNotAssignEhlakForPeriod_(ref opp,  Branch_Id, DateFrom, DateTo);
        }
        DataTable Get_AssetsNotAssignEhlakForPeriod_(ref AccountModelsLib.operations opp,long Branch_Id, DateTime DateFrom, DateTime DateTo)
        {
            int Excp = 0;
            string mysql = "  ";


            mysql = @"SELECT DISTINCT FixedAssetData.*, Submain_Account.*
FROM      Submain_Account INNER JOIN
                         FixedAssetData ON Submain_Account.Id = FixedAssetData.SubAccount_Id ";

            mysql = mysql + " WHERE     Account_Ehlak_Status >= 2  ";
            mysql = mysql + "  And   (Submain_Account.Id NOT IN  (SELECT Assets_Account_ID  FROM SubMainAssets_Ehlak  WHERE (DateFrom = '" + AccountModelsLib.operations.dateFormat(DateFrom) + "') AND (DateTo = '" + AccountModelsLib.operations.dateFormat(DateTo) + "')))  ";
            if (Branch_Id > 0)
            {
                mysql = mysql + " And FixedAssetData.Branch_Id="+Branch_Id;
            }
            mysql = mysql + " ORDER BY SubAccount_Name ";

            string Msg = "";
            return Get_Requests_Items_Data( ref Excp, ref Msg, ref opp, mysql);

        }

        public DataTable Get_AssetsEhlakForPeriod(long Branch_Id, DateTime DateFrom, DateTime DateTo)
        {
            int Excp = 0;
            string mysql = " SELECT Submain_Account.*, SubMainAssets_Ehlak.* ";
            mysql = mysql + " FROM Submain_Account INNER JOIN  SubMainAssets_Ehlak ON Submain_Account.Id = SubMainAssets_Ehlak.Assets_Account_ID ";

            mysql = mysql + " WHERE (SubMainAssets_Ehlak.DateFrom = '" + AccountModelsLib.operations.dateFormat(DateFrom) + "') AND (SubMainAssets_Ehlak.DateTo = '" + AccountModelsLib.operations.dateFormat(DateTo) + "') ";

            if (Branch_Id > 0)
            {
                mysql = mysql + " And FixedAssetData.Branch_Id=" + Branch_Id;
            }
            
            string Msg = "";
            //  ref Excp,ref Msg,
            return Get_Requests_Items_Data(ref Excp, ref Msg, mysql);

        }
        // üü¢ ÿ•ÿµŸÑÿßÿ≠ ÿÆÿßÿµ ÿ®ŸÄBlazor ŸÅŸÇÿ∑ (ÿ®ŸÖŸàÿßŸÅŸÇÿ© ÿµÿ±Ÿäÿ≠ÿ© ŸÖŸÜ ÿßŸÑŸÖÿ≥ÿ™ÿÆÿØŸÖ) - ŸÑÿß ŸäŸÖÿ≥Ÿë Get_AssetsEhlakForPeriod ÿßŸÑÿ£ÿµŸÑŸäÿ©
        // ŸàŸÑÿß ÿ¥ÿßÿ¥ÿ© WinForms ÿßŸÑÿ™Ÿä ÿ™ÿ≥ÿ™ÿØÿπŸäŸáÿß (Assets_EhlakFrm_.fillEhlakAssets). ÿßŸÑÿØÿßŸÑÿ© ÿßŸÑÿ£ÿµŸÑŸäÿ© ÿ™ÿ≠ÿ™ŸàŸä ÿÆŸÑŸÑÿßŸã
        // ÿ®ÿ±ŸÖÿ¨ŸäÿßŸã ŸàŸÖŸàÿ∂ŸàÿπŸäÿßŸã (ŸÑŸäÿ≥ ŸÇÿßÿπÿØÿ© ÿπŸÖŸÑ): ÿπŸÜÿØ ÿ™ŸÖÿ±Ÿäÿ± Branch_Id > 0 ÿ™ÿ∂ŸäŸÅ ÿ¥ÿ±ÿ∑
        // "And FixedAssetData.Branch_Id=..." ÿ±ÿ∫ŸÖ ÿ£ŸÜ ÿ¨ÿØŸàŸÑ FixedAssetData ÿ∫Ÿäÿ± ŸÖÿ∞ŸÉŸàÿ± ÿ•ÿ∑ŸÑÿßŸÇÿßŸã ŸÅŸä FROM/JOIN
        // ÿßŸÑÿÆÿßÿµ ÿ®Ÿáÿ∞ÿß ÿßŸÑÿßÿ≥ÿ™ÿπŸÑÿßŸÖ (ÿßŸÑÿßÿ≥ÿ™ÿπŸÑÿßŸÖ Ÿäÿ±ÿ®ÿ∑ ŸÅŸÇÿ∑ Submain_Account ŸÖÿπ SubMainAssets_Ehlak) - ŸÖÿß Ÿäÿ¨ÿπŸÑ
        // SQL Server Ÿäÿ±ŸÅÿ∂ ÿßŸÑÿßÿ≥ÿ™ÿπŸÑÿßŸÖ ÿ®ÿÆÿ∑ÿ£ "multi-part identifier could not be bound" ŸÅŸä ŸÉŸÑ ŸÖÿ±ÿ© ŸäŸèÿÆÿ™ÿßÿ± ŸÅŸäŸáÿß
        // ŸÅÿ±ÿπ ŸÖÿ≠ÿØÿØ. Ÿáÿ∞Ÿá ÿßŸÑŸÜÿ≥ÿÆÿ© ÿ™ÿ∂ŸäŸÅ ÿßŸÑŸÄJOIN ÿßŸÑŸÜÿßŸÇÿµ ŸÑÿ™ÿµÿ≠Ÿäÿ≠ ÿßŸÑŸÖÿ¥ŸÉŸÑÿ© ÿØŸàŸÜ ÿ™ÿ∫ŸäŸäÿ± ÿ£Ÿä ŸÖŸÜÿ∑ŸÇ ÿ≠ÿ≥ÿßÿ®Ÿä ÿ£Ÿà ÿ¥ÿ±ÿ∑ ÿπŸÖŸÑ ÿ¢ÿÆÿ±.
        public DataTable Get_AssetsEhlakForPeriod_Full(long Branch_Id, DateTime DateFrom, DateTime DateTo)
        {
            int Excp = 0;
            string mysql = " SELECT Submain_Account.*, SubMainAssets_Ehlak.* ";
            mysql = mysql + " FROM Submain_Account INNER JOIN  SubMainAssets_Ehlak ON Submain_Account.Id = SubMainAssets_Ehlak.Assets_Account_ID ";
            mysql = mysql + " INNER JOIN FixedAssetData ON Submain_Account.Id = FixedAssetData.SubAccount_Id ";

            mysql = mysql + " WHERE (SubMainAssets_Ehlak.DateFrom = '" + AccountModelsLib.operations.dateFormat(DateFrom) + "') AND (SubMainAssets_Ehlak.DateTo = '" + AccountModelsLib.operations.dateFormat(DateTo) + "') ";

            if (Branch_Id > 0)
            {
                mysql = mysql + " And FixedAssetData.Branch_Id=" + Branch_Id;
            }

            string Msg = "";
            return Get_Requests_Items_Data(ref Excp, ref Msg, mysql);
        }

        public DataTable Get_FixedAssetToEhlak()
        {
            DataTable dt = new DataTable();
            try
            {
                string mysql;

                mysql = @" SELECT DISTINCT FixedAssetData.*, Submain_Account.*
FROM            Submain_Account INNER JOIN
                         FixedAssetData ON Submain_Account.Id = FixedAssetData.SubAccount_Id";
                mysql = mysql + " Where Account_Ehlak_Status > 2";


                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                dt = opp.return_DataTable(mysql);

                return dt;
            }
            catch (Exception ee)
            {



            }
            return dt;
        }
        public void Make_Ehlak(ref int Excp,ref string Msg, long branch_Id, ArrayList arr,  long Financial_Period_Id, DateTime DateFrom, DateTime DateTo)
        {
           
            AccountModelsLib.operations opp = new AccountModelsLib.operations(1);
            try
            {
               
                Make_Ehlak(ref   Excp,ref Msg,branch_Id, arr,ref   opp, Financial_Period_Id, DateFrom, DateTo);
                opp.Commit();
                Excp = 0;
                if (Msg.ToString() != "")
                {
                     
                }
            }
            catch(Exception ee)
            {
                opp.Rollback();
                Excp = 1;
            }
        }
        public void Make_Ehlak(ref int Excp, ref string Msg,long Branch_Id, ArrayList arr, ref AccountModelsLib.operations opp, long Financial_Period_Id, DateTime DateFrom, DateTime DateTo)
        {
        /////////Get_All Asset
             Msg = "";
             DataTable dt = Get_AssetsNotAssignEhlakForPeriod(ref opp, Branch_Id, DateFrom, DateTo);
            long SubAccount_Id = 0;
            int Account_Ehlak_Type = 0;
            string Account_Ehlak_Type_Str = "";
            decimal Book_value = 0;
            decimal Accumulated_Depreciation_Value=0;
            decimal Rate_of_depreciation = 0;
            DateTime beginning_service_Date;
            string Asset_Name = "";

            for (int c = 0; c < dt.Rows.Count; c++)
            {
                SubAccount_Id = long.Parse(dt.Rows[c]["SubAccount_Id"].ToString());

                int index=arr.IndexOf(SubAccount_Id.ToString());
                if(index==-1)
                {continue;}
                 
                Asset_Name =  dt.Rows[c]["SubAccount_Name"].ToString() ;

                
                Account_Ehlak_Type = int.Parse(dt.Rows[c]["Account_Ehlak_Type"].ToString());
                Account_Ehlak_Type_Str = dt.Rows[c]["Account_Ehlak_Type_Str"].ToString();

                Book_value = decimal.Parse(dt.Rows[c]["Main_value"].ToString());
                Accumulated_Depreciation_Value = decimal.Parse(dt.Rows[c]["Accumulated_Depreciation_Value"].ToString());
                Rate_of_depreciation = decimal.Parse(dt.Rows[c]["Rate_of_depreciation"].ToString());
                beginning_service_Date = DateTime.Parse(dt.Rows[c]["beginning_service_Date"].ToString());

                DataTable dt_Previous_Ehlak = new DataTable();
                decimal Previous_Ehlak = Get_Previous_Ehlak_Data(ref opp,SubAccount_Id, ref dt_Previous_Ehlak);

                ///«· √ﬂœ „‰  ”·”·  Ê«—ÌŒ « ”«» «·«Â·«ﬂ
                if (Previous_Ehlak > -1)
                {
                    DateTime PrevLast_DateTo = DateTime.Parse(dt_Previous_Ehlak.Rows[0]["DateTo"].ToString());
                    TimeSpan tsDay=new TimeSpan(1,0,0,0);
                   PrevLast_DateTo= PrevLast_DateTo.AddDays(1);  //«÷«›… ÌÊ„ Õ Ï Ìﬁ«—‰ » «—ÌŒ »œ«Ì… «·«Â·«ﬂ «·ÃœÌœ

                    if (DateFrom != PrevLast_DateTo)
                    {

                        Msg = Msg + "ÌÃ» ÷»ÿ  ”·”·  Ê«—ÌŒ «·› —«  «·„«·ÌÂ ·Õ”«» «·«Â·«ﬂ";
                        Msg = Msg + "\n\r";
                        Msg = Msg + " «·Œ«’ » " + Asset_Name;
                        Msg = Msg + "\n\r";
                        continue;
                    }

                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                TimeSpan ts = DateTo.Subtract(DateFrom);
                int WorksDays = ts.Days + 1;

                ////⁄œœ √Ì«„ «·”‰Â
                DateTime d2 = new DateTime(DateFrom.Year, 12, 31);
                int DaysOFYear = d2.DayOfYear;

                decimal Total_Ehlake = 0;

                decimal Asset_Value_Befor_Period_Ehalk = 0;
                decimal Balance = 0;
                decimal Period_Ehalk = 0;
                decimal Asset_Value_Now = 0;
               
                // //  decimal Asset_Value_Now = Compute_Period_Ehlak(ref Excp, ref opp, SubAccount_Id, Previous_Ehlak, DateFrom, DateTo, Balance, Rate_of_depreciation);
                     


               
                    if (Account_Ehlak_Type == Common.Ehlak_Kest_Sabt) // ﬁ’ À«» 
                    {
                      
                        Balance = Book_value;
                          Period_Ehalk = Balance * Rate_of_depreciation / 100 * WorksDays / DaysOFYear;
                        

                        Previous_Ehlak = Get_Previous_Ehlak(ref   opp, SubAccount_Id);

                        Total_Ehlake = Period_Ehalk + Previous_Ehlak;

                        ///ﬁÌ„… —’Ìœ «·«’·   ÿ—Õ „‰ «Ã„«·Ï «·Â«·ﬂ «·”«»ﬁ
                          Asset_Value_Now = Balance - Total_Ehlake; ;
                          Asset_Value_Befor_Period_Ehalk = Asset_Value_Now + Period_Ehalk;
                    }

                    if (Account_Ehlak_Type == Common.Ehlak_Kest_Motanakes) // ﬁ’ „ ‰«ﬁ’
                    {
                        if (Previous_Ehlak == -1)
                        { 
                            Balance = Book_value;
                        }
                        else
                        {
                            Balance = decimal.Parse(dt_Previous_Ehlak.Rows[0]["Asset_Value_Now"].ToString());
                        }
                   
                       Period_Ehalk = Balance * Rate_of_depreciation / 100 * WorksDays / DaysOFYear;
                     Asset_Value_Befor_Period_Ehalk = Balance;

                     Previous_Ehlak = Get_Previous_Ehlak(ref   opp, SubAccount_Id);

                     Total_Ehlake = Period_Ehalk + Previous_Ehlak;
                     ///ﬁÌ„… —’Ìœ «·«’·   ÿ—Õ „‰ «Ã„«·Ï «·Â«·ﬂ «·Õ«·Ï
                       Asset_Value_Now = Balance - Period_Ehalk; ;
                        
                     //string mysql = "Update FixedAssetData Set Book_value= " + Asset_Value_Now + " Where SubAccount_Id=" + SubAccount_Id;
                     //opp.insert_me(mysql);
                    }

                    Set_Ehlak_Asset(ref opp, SubAccount_Id, DateFrom, DateTo, Previous_Ehlak, Period_Ehalk, Total_Ehlake, Asset_Value_Now, Asset_Value_Befor_Period_Ehalk, 1);
               

                
            }


        }
        
       

     
        decimal Get_Previous_Ehlak_Data(ref AccountModelsLib.operations opp,long SubAccount_Id, ref DataTable dt)
        {
            string mysql = " ";
            //   mysql = " SELECT SUM(Previous_Ehlak) AS Previous_Ehlak FROM SubMainAssets_Ehlak ";
            // mysql = mysql + " WHERE (Assets_Account_ID = " + SubAccount_Id + ") GROUP BY Assets_Account_ID ";

            mysql = " SELECT SubMainAssets_Ehlak.*, Total_Eklak AS Previous_Ehlak  FROM SubMainAssets_Ehlak WHERE (Assets_Account_ID = " + SubAccount_Id + ")  ORDER BY Id DESC ";
            decimal Previous_Ehlak = 0;
            int Excp = 0;
            string Msg = "";
               dt = Get_Requests_Items_Data(ref Excp,ref Msg, ref opp, mysql);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Previous_Ehlak"].ToString() == "")
                {
                    Previous_Ehlak = -1;
                }
                else
                {
                    Previous_Ehlak = decimal.Parse(dt.Rows[0]["Previous_Ehlak"].ToString());
                }
            }
            else
            {
                Previous_Ehlak = -1;
            }

            return Previous_Ehlak;


        }

        decimal Get_Previous_Ehlak(ref AccountModelsLib.operations opp, long SubAccount_Id )
        {
            string mysql = " ";

            mysql = "  SELECT  SUM(Period_Ehalk) AS Total_Previous_Ehlak FROM    SubMainAssets_Ehlak ";
    mysql=mysql +" WHERE (Assets_Account_ID = " + SubAccount_Id + ") " ;
          
            decimal Previous_Ehlak = 0;
            int Excp = 0;
            string Msg = "";
          DataTable   dt = Get_Requests_Items_Data(ref Excp,ref Msg, ref opp, mysql);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Total_Previous_Ehlak"].ToString() == "")
                {
                    Previous_Ehlak = 0;
                }
                else
                {
                    Previous_Ehlak = decimal.Parse(dt.Rows[0]["Total_Previous_Ehlak"].ToString());
                }
            }
            else
            {
                Previous_Ehlak = 0;
            }

            return Previous_Ehlak;


        }
        decimal  Compute_Period_Ehlak(ref int Excp,ref string Msg, ref AccountModelsLib.operations opp, long SubAccount_Id, decimal Previous_Ehlak, DateTime DateFrom, DateTime DateTo, decimal Balance, decimal Rate)
        {
            decimal Asset_Value_Now = 0;
            try
            {
               
                
                int year = DateTo.Year - DateFrom.Year;
                int Month =( DateTo.Month - DateFrom.Month) + 1;

                TimeSpan ts = DateTo.Subtract(DateFrom);
                int WorksDays = ts.Days + 1;
                
                ////⁄œœ √Ì«„ «·”‰Â
                DateTime d2 = new DateTime(DateFrom.Year, 12, 31);
                int DaysOFYear = d2.DayOfYear;

               

                decimal Total_Ehlake = 0;
                
                decimal Asset_Value_Befor_Period_Ehalk = 0;
               
                
                //Month = Month + year * 12;
                //decimal Period_Ehalk = Balance * Rate / 100 * Month / 12;
                decimal Period_Ehalk = Balance * Rate / 100 * WorksDays / DaysOFYear;
                Asset_Value_Befor_Period_Ehalk = Balance;

                Previous_Ehlak = Get_Previous_Ehlak(ref   opp,   SubAccount_Id );

                Total_Ehlake = Period_Ehalk + Previous_Ehlak;
                  Asset_Value_Now = Balance - Period_Ehalk; ;
                Set_Ehlak_Asset(ref opp,SubAccount_Id, DateFrom, DateTo, Previous_Ehlak, Period_Ehalk, Total_Ehlake, Asset_Value_Now, Asset_Value_Befor_Period_Ehalk, 1);
                Excp = 0;
            }
            catch (Exception ee)
            {
                Excp = 1;
              Msg=ee.Message;
            }
            return Asset_Value_Now;
        }

         void Set_Ehlak_Asset(ref AccountModelsLib.operations opp,long Assets_Account_ID, DateTime DateFrom, DateTime DateTo, decimal Previous_Ehlak, decimal Period_Ehalk, decimal Total_Eklak, decimal Asset_Value_Now, decimal Asset_Value_Befor_Period_Ehalk, int First_Period_Flage)
        {
               
            
                Previous_Ehlak = Common.RoundMe(Previous_Ehlak);
                Period_Ehalk = Common.RoundMe(Period_Ehalk);
                Total_Eklak = Common.RoundMe(Total_Eklak);
                Asset_Value_Now = Common.RoundMe(Asset_Value_Now);
                Asset_Value_Befor_Period_Ehalk = Common.RoundMe(Asset_Value_Befor_Period_Ehalk);

               

                string mysql = " INSERT INTO SubMainAssets_Ehlak (Assets_Account_ID, DateFrom, DateTo, Previous_Ehlak, Period_Ehalk, Total_Eklak, Asset_Value_Now,Asset_Value_Befor_Period_Ehalk,First_Period_Flage) ";
                mysql = mysql + " VALUES (" + Assets_Account_ID + ",'" + AccountModelsLib.operations.dateFormat(DateFrom) + "','" + AccountModelsLib.operations.dateFormat(DateTo) + "'," + Previous_Ehlak + "," + Period_Ehalk + "," + Total_Eklak + "," + Asset_Value_Now + "," + Asset_Value_Befor_Period_Ehalk + "," + First_Period_Flage + ") ";
              
                opp.insert_me(mysql);


              

               
            
        }

        public void Update_Ehlak_Asset(ref string Msg, int SubMainAssets_Ehlak_ID, DateTime DateFrom, DateTime DateTo, decimal Previous_Ehlak, decimal Period_Ehalk, decimal Total_Eklak, decimal Asset_Value_Now, decimal Asset_Value_Befor_Period_Ehalk)
        {
            try
            {
                Previous_Ehlak = Common.RoundMe(Previous_Ehlak);
                Period_Ehalk = Common.RoundMe(Period_Ehalk);
                Total_Eklak = Common.RoundMe(Total_Eklak);
                Asset_Value_Now = Common.RoundMe(Asset_Value_Now);
                Asset_Value_Befor_Period_Ehalk = Common.RoundMe(Asset_Value_Befor_Period_Ehalk);


                string mysql = " Update SubMainAssets_Ehlak Set  Period_Ehalk= " + Period_Ehalk + ",Total_Eklak=" + Total_Eklak + ",Asset_Value_Now=" + Asset_Value_Now + ", Edit_User_Id=" + Common.User_Id + ", EditOperationDate='" + AccountModelsLib.operations.dateFormat(DateTime.Now) + "' where Id=" + SubMainAssets_Ehlak_ID;
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                opp.update_me(mysql);
            }
            catch (Exception ee)
            { Msg=ee.Message; }
        }
       
     

        public void Delete_Ehlak(ref int Excp, ArrayList arr)
        {
            string mysql = "";
            ArrayList arrD = new ArrayList();
            for (int c = 0; c < arr.Count; c++)
            {
                mysql = " Delete from  SubMainAssets_Ehlak where Id=" + arr[c].ToString();
                arrD.Add(mysql);
            }
            AccountModelsLib.operations opp = new AccountModelsLib.operations();
            int Status = opp.insert_me_transaction(arrD);
            if (Status == 1)
            { Excp = 0; }
            else
            { Excp = -1; }

        }
        private DataTable Get_Requests_Items_Data(ref int Excp,ref string Msg, ref AccountModelsLib.operations opp, string mysql)
        {
            DataTable G_DS = new DataTable();
            try
            {
               
                G_DS = opp.return_DataTable(mysql);
                Excp = 1;
                
                return G_DS;
            }
            catch (Exception ee)
            {

                Excp = -1;
                Msg = ee.Message;
                return G_DS;
            }
        }
        private DataTable Get_Requests_Items_Data(ref int Excp,ref string Msg, string mysql)
        {
            DataTable G_DS = new DataTable();
            try
            {
                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                G_DS = opp.return_DataTable(mysql);
                Excp = 1;
                opp = null;
                return G_DS;
            }
            catch (Exception ee)
            {

                Excp = -1;
                Msg = ee.Message;
                return G_DS;
            }
        }
    }
}
