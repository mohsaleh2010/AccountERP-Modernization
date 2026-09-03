using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using AccountModelsLib.Models;
   
namespace AccountModelsLib 
{ 
	public class Checks_Status 
	{
		private string My_Status_Text ;
		private string My_Status_Value ;

		private string My_Minue_Text ;
		private string My_Minue_Value ;
    
        public  Checks_Status()
		{}

		public Checks_Status(string Text, string Value)
		{

			this.My_Status_Text = Text;
			this.My_Status_Value = Value;

			My_Minue_Text = Text;
			My_Minue_Value = Value;
		}
		

		public string Status_Value
		{
			get
			{
				return My_Status_Value;
			}
		}

		public string  Status_Text 
		{
        
			get
			{
				return My_Status_Text ;
			}
		}

        public static ArrayList ImportsCheckStatus_()
        {
            ArrayList St = new ArrayList();
            St.Add(new Checks_Status("ﬂ· «·‘Ìﬂ« ", "0"));
            St.Add(new Checks_Status("›Ï Õ«›Ÿ… «·‘Ìﬂ« ", "1"));
            St.Add(new Checks_Status(" Õ  «·„—«Ã⁄…", "4"));
            St.Add(new Checks_Status("„’—Ê›… „‰ «·»‰ﬂ", "10"));
            //St.Add(new Checks_Status("€Ì— „’—Ê›… „‰ «·»‰ﬂ", "2"));
            St.Add(new Checks_Status("„— Ã⁄ „‰ «·»‰ﬂ", "3"));
            //St.Add(new Checks_Status("„ÃÌ“… √Ê „ÕÊ·…", "5"));
            St.Add(new Checks_Status("‘Ìﬂ«   Õ  «· Õ’Ì·", "11"));
            St.Add(new Checks_Status("„— Ã⁄ ≈·Ï ⁄„Ì·", "12"));
            // St.Add(new Checks_Status("€Ì— „— Ã⁄ ≈·Ï ⁄„Ì·", "13"));
            St.Add(new Checks_Status("„ÃÌ“… √Ê „ÕÊ·…", "30"));
            if (Common.CompanyTitle == Common.CompanyTitle_Client01)
            {
               
            }
            return St;

        }

        public static ArrayList ExportsCheckStatus_()
        {
            ArrayList St = new ArrayList();
            St.Add(new Checks_Status("ﬂ· «·‘Ìﬂ« ", "0"));
            St.Add(new Checks_Status("‘Ìﬂ ›Ï Õ«›Ÿ… «·‘Ìﬂ«  «·’«œ—…", "32"));
            St.Add(new Checks_Status("‘Ìﬂ  Õ  «·„—«Ã⁄… «·√‰", "31"));
          //  St.Add(new Checks_Status("‘Ìﬂ ·„ Ì’—› „‰ «·»‰ﬂ", "2"));
            St.Add(new Checks_Status("‘Ìﬂ  „ ’—›…", "10"));
            St.Add(new Checks_Status("„— Ã⁄ ≈·Ï „Ê—œ", "12"));
          //  St.Add(new Checks_Status("€Ì— „— Ã⁄ ≈·Ï „Ê—œ", "13"));

            return St;

        }
       
    }
}