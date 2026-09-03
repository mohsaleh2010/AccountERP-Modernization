using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
 
using System.Collections;
 
using AccountModelsLib;
using AccountModelsLib.Models;
namespace AccountModelsLib
{
 public    class Capital_class
    {
         AccountModelsLib.operations opp;
        DataSet machineds;
        public Capital_class()
        {
              opp = new AccountModelsLib.operations();
            machineds=new DataSet();
        }


        public void Update_capital(int id, string partner_Name, decimal Percentage)
        {
            string mysql = "UPDATE Capital_Money SET partner_Name = '" + partner_Name + "', Percentage = " + Percentage + " WHERE(Id = " + id + ")";
            opp.update_me(mysql);
        }
        public DataSet  GET_Capital_list()
        {
            string mysql = "SELECT * FROM Capital_Money";
            machineds=opp.return_dataset(mysql);
            return machineds;
        }
        public void add_capital(string Capital_name, decimal percentage)
        {
            string sql;
            sql = "INSERT INTO Capital_Money(partner_Name, Percentage)";
            sql += "VALUES('" + Capital_name + " ' , " + percentage + ")";
            opp.insert_me(sql);

        }
        /* date = AccountModelsLib.operations.dateFormat(dateTimePicker1.Value);
            Capital_id = int.Parse(this.CMB_Capital_name.SelectedValue.ToString());
            vlaue = decimal.Parse(txt_Value.Text);
            notes = Txt_Notes.Text;
            Bank_id=int.Parse(this.banks_List1.cmbBankType.SelectedValue.ToString());
         
*/

        public DataSet GetMoneyPulled(ref int Excp, int Bank_Id, DateTime Day_DateFrom, DateTime Day_DateTo, bool Interval)
        {
            DataSet ds = null;
            string mysql;
            try
            {
               
                mysql = " SELECT capital_pulled.Capital_id, capital_pulled.DATE, capital_pulled.Value, capital_pulled.Banke_id, capital_pulled.Notes, Banks.Last_Balance , Capital_Money.partner_Name ";
                mysql = mysql + " FROM capital_pulled INNER JOIN  Banks ON capital_pulled.Banke_id = Banks.id INNER JOIN Capital_Money ON capital_pulled.Capital_id = Capital_Money.Id";
                
                mysql = mysql + "  WHERE (Banks.Is_Bank = 0) "; 
                 if (Interval)
                {
                    mysql = mysql + "   AND  (capital_pulled.DATE >= '" + AccountModelsLib.operations.dateFormat(Day_DateFrom) + "') AND (capital_pulled.DATE <= '" + AccountModelsLib.operations.dateFormat(Day_DateTo) + "')  ";
                }
                else
                {
                    mysql = mysql + "  AND (capital_pulled.DATE = '" + AccountModelsLib.operations.dateFormat(Day_DateFrom) + "')  ";
                }
                mysql = mysql + " ORDER BY capital_pulled.Id DESC ";


                AccountModelsLib.operations opp = new AccountModelsLib.operations();
                ds = opp.return_dataset(ref Excp, mysql, "capital_pulled");
                opp = null;
                Excp = 0;
                return ds;
            }
            catch (Exception e)
            {
                Excp = 1;
                throw (e);

            }
        }
        public void insert_pulled_capital(bool Accepted, ref int Status,DateTime  DATE, int Capital_id, decimal vlaue, int Bank_id, string notes,decimal Bank_Balance)
        {
             
            if (Accepted)
            {
                 
                ArrayList arr = new ArrayList();
                string mysql = "INSERT INTO capital_pulled(DATE,Capital_id, Value, Banke_id, Notes)";
                mysql += "VALUES('" + AccountModelsLib.operations.dateFormat(DATE) + "'," + Capital_id + " ," + vlaue + "," + Bank_id + ",' " + notes + "')";
                arr.Add(mysql);
                mysql = " UPDATE Banks SET Last_Balance = Last_Balance - " + vlaue + " WHERE (id = " + Bank_id + ")";
                arr.Add(mysql);
                Status = opp.insert_me_transaction(arr);
            }
            else 
            {
                ArrayList arr = new ArrayList();
                string mysql = "INSERT INTO capital_pulled(DATE,Capital_id, Value, Banke_id, Notes)";
                mysql += "VALUES('" + AccountModelsLib.operations.dateFormat(DATE) + "'," + Capital_id + " ," + vlaue + "," + -1 + ",' " + notes + "')";
                arr.Add(mysql);
                Status = opp.insert_me_transaction(arr);
 
            }
        }
        
    }
}
