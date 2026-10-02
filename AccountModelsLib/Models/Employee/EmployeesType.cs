using System;
using System.Collections;
 

namespace AccountModelsLib
{
    /// <summary>
    /// Summary description for Show_Day_Time.
    /// </summary>
    public class EmployeesType
    {

        /* hours
         this.comboBox1.DataSource = Show_Day_Time.ShowHour() ;
            comboBox1.DisplayMember = "Hour_Text"      ;
            comboBox1.ValueMember = "Hour_Value" ;

        minute
            comboBox2.DataSource = Show_Day_Time.ShowMinute() ;
            comboBox2.DisplayMember = "Minute_Text"      ;
            comboBox2.ValueMember = "Minute_Value" ; 
         */

        public EmployeesType()
        {

           
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// This method return Arraylist used as data source for ComboBox  to show the hour or day
        ///  and DisplayMember = "Hour_Text"      ;
        ///  and ValueMember = "Hour_Value" ;
        ///  1 Am its value =1
        ///  1 pm its value =13 and so on
        /// </summary>
        public static ArrayList EmployeesOfFactory()
        {
            ArrayList Emp = new ArrayList();
            if (Common.CompanyTitle == Common.ClientCompanyTitle)
            {
                Emp.Add(new Employees("⁄„«· «·‰”ÌÃ", "1"));
                Emp.Add(new Employees("⁄„«· «·’»«€…", "2"));
                Emp.Add(new Employees("⁄„«· «·”œ«", "3"));
                Emp.Add(new Employees("⁄„«· «· ÿ—Ì“", "4"));
                Emp.Add(new Employees("⁄„«·  ÃÂÌ“", "5"));
                Emp.Add(new Employees("⁄„«·  ‘ÿÌ» Ê €·Ì›", "6"));
                Emp.Add(new Employees("«·≈œ«—ÌÌ‰", "7"));
            }
            else if (Common.CompanyTitle == Common.CompanyTitle_Client01)
            {
                Emp.Add(new Employees("ﬁ”„ «·œ«∆—Ï", "1"));
                Emp.Add(new Employees("ﬁ”„ «· ÿ—Ì“", "2"));
                Emp.Add(new Employees("ﬁ”„ «·≈‰ «Ã", "3"));
                Emp.Add(new Employees("ﬁ”„ «·„ﬂÊ«…", "4"));
                Emp.Add(new Employees("ﬁ”„  ÃÂÌ“", "5"));
                Emp.Add(new Employees("ﬁ”„ «·ﬁ’", "6"));
                Emp.Add(new Employees("«·≈œ«—ÌÌ‰", "7"));

            }
            else
            { 
                Emp.Add(new Employees("«·„ÊŸ›Ì‰ Ê«·⁄„«·", "0")); 
            }
            return Emp;

        }

        public static ArrayList EmployeesSalaryTypes_Client()
        {
            ArrayList Emp = new ArrayList();
            Emp.Add(new Employees("»«·„œ… ≈”»Ê⁄Ì‰", "4"));
            Emp.Add(new Employees("≈‰ «Ã  ÿ—Ì“", "5"));
            Emp.Add(new Employees("≈‰ «Ã ”œ«", "6"));
            Emp.Add(new Employees("≈‰ «Ã „»—“« Ï", "7"));
            Emp.Add(new Employees("»«·‘Â—", "1"));
            Emp.Add(new Employees("»«·≈”»Ê⁄", "2"));
          //  Emp.Add(new Employees("»«·”«⁄…", "3"));
            
            return Emp;

        }

        public static ArrayList EmployeesSalaryTypes_General()
        {
            ArrayList Emp = new ArrayList();
             Emp.Add(new Employees("»«·‘Â—", "1"));
            Emp.Add(new Employees("»«·≈”»Ê⁄", "2"));
            Emp.Add(new Employees("»«·”«⁄…", "3"));
            Emp.Add(new Employees("»«·≈‰ «Ã", "4"));
            //  Emp.Add(new Employees("»«·”«⁄…", "3"));

            return Emp;

        }


       
        public static ArrayList EmployeesMetwat()
        {
            ArrayList Emp = new ArrayList();
            Emp.Add(new Employees("„ÿÊ«… Ê»—…", "1"));
            Emp.Add(new Employees("„ÿÊ«… ·Õ«›", "2"));
            Emp.Add(new Employees("„ÿÊ«… √—÷Ì…", "3"));
            
            return Emp;

        }
      

        public static ArrayList Item_TypeList()
        {
            ArrayList Emp = new ArrayList();
            Emp.Add(new Employees("€Ì— „’‰›", "0"));
            Emp.Add(new Employees("Œ«„", "1"));
            Emp.Add(new Employees("√·Ê«‰", "2"));

            return Emp;

        }
        
        public static ArrayList CarShape(bool All)
        {
             
            ArrayList Emp = new ArrayList();
            if (All == true)
            {
                Emp.Add(new Employees("«·ﬂ·", "0"));
            }
            Emp.Add(new Employees("”Ìœ«‰", "1"));
            Emp.Add(new Employees("Â« ‘»«ﬂ", "2"));
            Emp.Add(new Employees("œ»«»…", "3"));

            return Emp;

        }

        //("ﬁ”„ «·‰”ÌÃ", "1"));
        //       ("ﬁ”„ «· ‘ÿÌ»", "2"));
        // ("«·›Õ’", "3"));
        //       ("„Œ“‰ «·Œ«„", "4"));
        //       ("«·„’»€…", "10"));
        //("«· ÿ—Ì“", "5"));
        //("«· ÿ—Ì“ «·Œ«—ÃÏ", "6"));
        // ("«· ⁄»∆… Ê«· €·Ì›", "7 "));

        public static ArrayList ItemTypeToSend(int Item_Type)
        {
            ArrayList Emp = new ArrayList();
            //Emp.Add(new Employees("€Ì— „Õœœ", "0"));
            if (Item_Type == 0) // €Ì— „’ﬂ›
            {
                Emp.Add(new Employees("ﬁ”„ «· ‘ÿÌ»", "2"));
            }
            if (Item_Type == 1) // Œ«„
            {
                Emp.Add(new Employees("ﬁ”„ «· ‘ÿÌ»", "2"));
                Emp.Add(new Employees("„Œ“‰ «·Œ«„", "4"));
                Emp.Add(new Employees("«·„’»€…", "10"));
            }
            if (Item_Type == 2) // «·Ê«‰
            {
                Emp.Add(new Employees("ﬁ”„ «· ‘ÿÌ»", "2"));
                Emp.Add(new Employees("«·›Õ’", "3"));
                
            }
            return Emp;

        }
        public static ArrayList ItemTypeToSendFromTashteeb(int Item_Type)
        {
            ArrayList Emp = new ArrayList();
            Emp.Add(new Employees("≈Œ Ì«— «·ﬁ”„", "0"));
            if (Item_Type == 1) // Œ«„
            {
                
                Emp.Add(new Employees("„Œ“‰ «·Œ«„", "4"));
                Emp.Add(new Employees("«·„’»€…", "10"));
            }
            if (Item_Type == 2) // «·Ê«‰
            {
                
                Emp.Add(new Employees("«·›Õ’", "3"));

            }
            return Emp;

        }
        public static ArrayList ItemTypeToSendFromFa7s()
        {
            ArrayList Emp = new ArrayList();
            Emp.Add(new Employees("≈Œ Ì«— «·ﬁ”„", "0"));
            Emp.Add(new Employees("«· ÿ—Ì“", "5"));
            Emp.Add(new Employees("«· ‘€Ì· «·Œ«—ÃÏ", "6"));
            Emp.Add(new Employees("«· ⁄»∆…", "7")); 
            return Emp;

        }

        public static ArrayList ItemTypeSendFromTatreez()
        {
            ArrayList Emp = new ArrayList();
            Emp.Add(new Employees("≈Œ Ì«— «·ﬁ”„", "0"));
            Emp.Add(new Employees("«·›Õ’", "3"));
            Emp.Add(new Employees("«· ⁄»∆…", "7"));
            return Emp;

        }

        public static ArrayList GetNewHour_Value()
        {
            ArrayList Emp = new ArrayList();
            Emp.Add(new Employees("—»⁄ ”«⁄…", "0.25"));
            Emp.Add(new Employees("‰’› ”«⁄…", "0.5"));
            Emp.Add(new Employees("”«⁄… ≈·« —»⁄", "0.75"));
            Emp.Add(new Employees("”«⁄…", "1"));
            Emp.Add(new Employees("”«⁄… Ê—»⁄", "1.25"));
            Emp.Add(new Employees("”«⁄… Ê‰’›", "1.5"));
            Emp.Add(new Employees("”«⁄« Ì‰ ≈·« —»⁄", "1.75"));
            Emp.Add(new Employees("”«⁄« Ì‰", "2"));
            Emp.Add(new Employees("”«⁄« Ì‰ Ê—»⁄", "2.25"));
            Emp.Add(new Employees("”«⁄« Ì‰ Ê‰’›", "2.5"));
            Emp.Add(new Employees("À·«À ”«⁄«  ≈·« —»⁄", "2.75"));
            Emp.Add(new Employees("À·«À ”«⁄« ", "3"));
            //  Emp.Add(new Employees("»«·”«⁄…", "3"));

            return Emp;

        }
        public static string GetEmployeesMetwatName(long Id)
        {
            ArrayList arr=EmployeesMetwat();
            
            string Name="";
            foreach (Employees em in arr)
            {
                if (Id.ToString() == em.Emp_Value)
                { 
                    Name = em.Emp_Text;
                    break;
                    
                }
            }
            if (Name!="")
            {
                return Name;
            }
            else
            {
                return "€Ì— „ÊÃÊœ";
            }
        }


        ///////////////////
        public static string EmployeesSalaryTypesName(long Id)
        {
            ArrayList arr = new ArrayList();
            if (Common.InstituteName == Common.ClientCompanyTitle)
            {
                  arr = EmployeesSalaryTypes_Client();
            }
            else
            {
                arr = EmployeesSalaryTypes_General();
            }
            string Name = "";
            foreach (Employees em in arr)
            {
                if (Id.ToString() == em.Emp_Value)
                {
                    Name = em.Emp_Text;
                    break;

                }
            }
            if (Name != "")
            {
                return Name;
            }
            else
            {
                return "€Ì— „ÊÃÊœ";
            }
        }

        ///////////////
        public static string EmployeesSalaryTypes_General_Name(long Id)
        {
            ArrayList arr = EmployeesSalaryTypes_General();

            string Name = "";
            foreach (Employees em in arr)
            {
                if (Id.ToString() == em.Emp_Value)
                {
                    Name = em.Emp_Text;
                    break;

                }
            }
            if (Name != "")
            {
                return Name;
            }
            else
            {
                return "€Ì— „ÊÃÊœ";
            }
        }
    }

    public class Employees
    {
        private string My_Emp_Text;
        private string My_Emp_Value;

       
        public Employees(string Text, string Value)
        {

            this.My_Emp_Text = Text;
            this.My_Emp_Value = Value;

           
        }
        public Employees()
        { }

        public string Emp_Value
        {
            get
            {
                return My_Emp_Value;
            }
        }

        public string Emp_Text
        {

            get
            {
                return My_Emp_Text;
            }
        }


         

    }

    public class AccountsData
    {
        private string Name;
        private string ID;
        private int AccountsFollows;
        private int Type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Account_Name"></param>
        /// <param name="Account_ID"></param>
        /// <param name="Accounts_Follows">«· ÊÃÌÂ… «·„Õ«”»Ï</param>
        /// <param name="Account_Type">1 mean Madin --- 2 >>Da2n</param>
        public AccountsData(string Account_Name, string Account_ID, int Accounts_Follows, int Account_Type)
        {

            this.Name = Account_Name;
            this.ID = Account_ID;
            this.AccountsFollows = Accounts_Follows;
            this.Type = Account_Type;


        }
        public AccountsData()
        { }

        public string Account_ID
        {
            get
            {
                return ID;
            }
        }

        public string Account_Name
        {

            get
            {
                return Name;
            }
        }
        public int Accounts_Follows
        {

            get
            {
                return AccountsFollows;
            }
        }
        public int Accounts_Type
        {

            get
            {
                return Type;
            }
        }



    }
}
