using System;

namespace AccountModelsLib
{
	/// <summary>
	/// Summary description for Check_Static_Data.
	/// </summary>
	public class Check_Static_Data
	{
		public static bool Edit_Order;

		public static long Transfer_Check_Id; // —ﬁ„ «·‘Ìﬂ ”Ê› ÌŸÂ—
		public static bool Transfer_Check ;
		public static long Check_Id; 
		public static long Money_Transaction_Box_id;
		public static string Check_No; 
		public static DateTime Day_Date;
		public static DateTime Due_Date; 
		public static long Bank_id; 
		public static decimal Bank_Balance;
		public static decimal Check_Value; 
		public static string Check_Value_;
        public static decimal  Order_Total;
		public static long Customer_Id;
        public static decimal Customer_Balance; 
		public static string Customer_Name;
        public static long SalesMan_Id;
        public static string SalesMan_Name;
		public static long Supplier_Id;
        public static decimal  Supplier_Balance;
		public static string Supplier_Name;
	//	public static int Account_Id;
		public static string Due_Check;
		 
		public static string Exchanger_Name; 
		public static string  First_Drawer_Name; 
		public static string Last_Drawer_Name;
		public static string Notes;

		public static int Transaction_Type; 
		public static string Order_No;
		public static DateTime Order_Date;
		public static long Order_Id;
		//public static int Transaction_type ;
		public static decimal Total_Value;
		public static long Currency_Id;
		public static int Bank_Type;
		public static string Man_Name;
        public static string Order_Notes;
        public static string Bank_Name;
		//Account info
        public static long SubAccount_Id;
		public static string SubAccount_No;
		public static string SubAccount_Name;
        public static int  Account_Type_Value;
        public static decimal Submain_Account_Balance ; 
        public static string MainSubAccount_Name;
        public static long MainSubAccount_ID;
		public static bool Is_New_Check;

		public static decimal Cash_Value; 
       public static int Receipt;
        public static int Bank_Id_Under_Ta7seel;
        public static int BankAccount_Id;
		public static string Cash_Value_;
        public static int Head_Main_Account;
        public static int Balance_BeginingPeriod;
        public static int SpentTo_Bank_Id;
        public static int  SpentTo_Bank_Type;
		public Check_Static_Data(bool New_Order)
		{ 
			if(New_Order)
			{
				Reset_All_Order_Data();
			}
			else
			{
			Reset_Only_Check_Data();
			}
		}
		void Reset_Only_Check_Data()
		{
			Edit_Order=true;
			Check_Id=0 ; 
			Check_No="" ;
            Bank_Name = "";
			Day_Date=DateTime.Now.Date ;
			Due_Date=DateTime.Now.Date ;
		 Submain_Account_Balance=0;
			Bank_id=0 ; 
			Bank_Balance=0 ;
			Check_Value=0 ;
            Receipt = -1;
			Check_Value_="" ;
			//Customer_Id=0 ; 
			//Supplier_Id=0 ;
            BankAccount_Id = -1;
			 SubAccount_Id=0;
             MainSubAccount_ID = 0;
			 SubAccount_No="";
			 SubAccount_Name="";

			Due_Check="0" ;
			//Transaction_Type=0 ; 
			Exchanger_Name="" ; 
			First_Drawer_Name="" ; 
			Last_Drawer_Name="" ;
			Notes="" ;
            SpentTo_Bank_Id = -1;
            SpentTo_Bank_Type = -1;
            Bank_Id_Under_Ta7seel = -1;
		}
		void Reset_All_Order_Data()
		{
            SpentTo_Bank_Id = -1;
            SpentTo_Bank_Type = -1;
			Edit_Order=true;
			Check_Id=0 ; 
			Check_No="" ; 
			 
			Day_Date=DateTime.Now.Date ;
			  Due_Date=DateTime.Now.Date ;
			Order_Date=DateTime.Now.Date ;
		 Submain_Account_Balance=0;
			Bank_id=0 ; 
			Bank_Balance=0 ;
            Bank_Name = "";
			Check_Value=0 ;
            Receipt = -1;
			Check_Value_="" ;
			//Customer_Id=0 ; 
			//Supplier_Id=0 ;
            BankAccount_Id = -1;
			Due_Check="0" ;
			//Transaction_Type=0 ; 
			Exchanger_Name="" ; 
			First_Drawer_Name="" ; 
			Last_Drawer_Name="" ;
			Notes="" ;

			Order_No="" ;
			 
			Total_Value=0 ;
			Currency_Id=0 ;
			Bank_Type=1 ;
			Man_Name="" ;
            Order_Notes = "";
        //SubAccount_Id=0;
        //SubAccount_No="";
        // SubAccount_Name="";

        }
        public static void ResetData()
        {
        
		   

		   Transfer_Check_Id=0; // —ﬁ„ «·‘Ìﬂ ”Ê› ÌŸÂ—
		   
		   Check_Id=0; 
		   Money_Transaction_Box_id=0;
		   Check_No="0"; 
		  // Day_Date;
		   //Due_Date; 
		   Bank_id=0; 
		   Bank_Balance=0;
		   Check_Value=0; 
		   Check_Value_="0";
            Order_Total=0;
		   Customer_Id=0;
           Customer_Balance = 0;
		   Customer_Name="0"; 
		   Supplier_Id=0;
		   Supplier_Name="0";
           Supplier_Balance = 0;
	//	 int Account_Id;
		    Due_Check="0";
		 
		   Exchanger_Name=""; 
		    First_Drawer_Name=""; 
		   Last_Drawer_Name="";
		   Notes="";

		   Transaction_Type=0; 
		   Order_No="0";
		   //Order_Date;
		   Order_Id=0;
		 
		   Total_Value=0;
		   Currency_Id=0;
		   Bank_Type=-1;
		   Man_Name="";
            Order_Notes = "";

           SubAccount_Id =0;
           MainSubAccount_ID = 0;
		   SubAccount_No="0";
		   SubAccount_Name="0";
		  

		   Cash_Value=0 ;
          Receipt=-1;
		    Cash_Value_="";


        }
		
	}


    public class Check_Data
    {
        public   bool Edit_Order;

        public   long Transfer_Check_Id; // —ﬁ„ «·‘Ìﬂ ”Ê› ÌŸÂ—
        public   bool Transfer_Check;
        public   long Check_Id;
        public   long Money_Transaction_Box_id;
        public   string Check_No;
        public   DateTime Day_Date;
        public decimal Factor = 0;
        public   DateTime Due_Date;
        public   long Bank_id;
        public   decimal Bank_Balance;
        public   decimal Check_Value;
        public   string Check_Value_;
        public   decimal Order_Total;
        public   long  Customer_Id;
        public   decimal Customer_Balance;
        public   string Customer_Name;
        public   int SalesMan_Id;
        public   int SalesMan_Name;
        public   long Supplier_Id;
        public   decimal Supplier_Balance;
        public   string Supplier_Name;
        //	public   int Account_Id;
        public   string Due_Check;

        public   string Exchanger_Name;
        public   string First_Drawer_Name;
        public   string Last_Drawer_Name;
        public   string Notes;

        public   int Transaction_Type;
        public   string Order_No;
        public   DateTime Order_Date;
        public   long Order_Id;
        //public   int Transaction_type ;
        public   decimal Total_Value;
        public   long Currency_Id;
        public   int Bank_Type;
        public   string Man_Name;
        public string Order_Notes;
        public   string Bank_Name;
        //Account info
        public   long SubAccount_Id;
        public   string SubAccount_No;
        public   string SubAccount_Name;
        public   int Account_Type_Value;
        public   decimal Submain_Account_Balance;
        public   string MainSubAccount_Name;
        public   long MainSubAccount_ID;
        public   bool Is_New_Check;

        public   decimal Cash_Value;
        public   int Receipt;
        public   int Bank_Id_Under_Ta7seel;
        public   int BankAccount_Id;
        public   string Cash_Value_;
        public   int Head_Main_Account;
        public   int Balance_BeginingPeriod;
        public   int SpentTo_Bank_Id;
        public   int SpentTo_Bank_Type;
        public Check_Data(bool New_Order)
        {
            if (New_Order)
            {
                Reset_All_Order_Data();
            }
            else
            {
                Reset_Only_Check_Data();
            }
        }
        public Check_Data()
        {
             
                Reset_All_Order_Data();
            
        }
        void Reset_Only_Check_Data()
        {
            Edit_Order = true;
            Check_Id = 0;
            Check_No = "";
            Bank_Name = "";
            Day_Date = DateTime.Now.Date;
            Due_Date = DateTime.Now.Date;
            Submain_Account_Balance = 0;
            Bank_id = 0;
            Bank_Balance = 0;
            Check_Value = 0;
            Receipt = -1;
            Check_Value_ = "";
            //Customer_Id=0 ; 
            //Supplier_Id=0 ;
            BankAccount_Id = -1;
            SubAccount_Id = 0;
            MainSubAccount_ID = 0;
            SubAccount_No = "";
            SubAccount_Name = "";

            Due_Check = "0";
            //Transaction_Type=0 ; 
            Exchanger_Name = "";
            First_Drawer_Name = "";
            Last_Drawer_Name = "";
            Notes = "";
            SpentTo_Bank_Id = -1;
            SpentTo_Bank_Type = -1;
            Bank_Id_Under_Ta7seel = -1;
        }
        void Reset_All_Order_Data()
        {
            SpentTo_Bank_Id = -1;
            SpentTo_Bank_Type = -1;
            Edit_Order = true;
            Check_Id = 0;
            Check_No = "";

            Day_Date = DateTime.Now.Date;
            Due_Date = DateTime.Now.Date;
            Order_Date = DateTime.Now.Date;
            Submain_Account_Balance = 0;
            Bank_id = 0;
            Bank_Balance = 0;
            Bank_Name = "";
            Check_Value = 0;
            Receipt = -1;
            Check_Value_ = "";
            //Customer_Id=0 ; 
            //Supplier_Id=0 ;
            BankAccount_Id = -1;
            Due_Check = "0";
            //Transaction_Type=0 ; 
            Exchanger_Name = "";
            First_Drawer_Name = "";
            Last_Drawer_Name = "";
            Notes = "";

            Order_No = "";

            Total_Value = 0;
            Currency_Id = 0;
            Bank_Type = 1;
            Man_Name = "";
            Order_Notes = "";
            //SubAccount_Id=0;
            //SubAccount_No="";
            // SubAccount_Name="";

        }
        public   void ResetData()
        {



            Transfer_Check_Id = 0; // —ﬁ„ «·‘Ìﬂ ”Ê› ÌŸÂ—

            Check_Id = 0;
            Money_Transaction_Box_id = 0;
            Check_No = "0";
            // Day_Date;
            //Due_Date; 
            Bank_id = 0;
            Bank_Balance = 0;
            Check_Value = 0;
            Check_Value_ = "0";
            Order_Total = 0;
            Customer_Id = 0;
            Customer_Balance = 0;
            Customer_Name = "0";
            Supplier_Id = 0;
            Supplier_Name = "0";
            Supplier_Balance = 0;
            //	 int Account_Id;
            Due_Check = "0";

            Exchanger_Name = "";
            First_Drawer_Name = "";
            Last_Drawer_Name = "";
            Notes = "";

            Transaction_Type = 0;
            Order_No = "0";
            //Order_Date;
            Order_Id = 0;

            Total_Value = 0;
            Currency_Id = 0;
            Bank_Type = -1;
            Man_Name = "";
            Order_Notes = "";

            SubAccount_Id = 0;
            MainSubAccount_ID = 0;
            SubAccount_No = "0";
            SubAccount_Name = "0";


            Cash_Value = 0;
            Receipt = -1;
            Cash_Value_ = "";


        }

    }
}
