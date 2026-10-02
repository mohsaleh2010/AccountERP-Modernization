using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
  using AccountModelsLib; using AccountModelsLib.Models;
using System.Collections;
namespace AccountModelsLib
{
    public class Special_price_CLS
    {
         operations opp;
        DataSet All_prices;
        public Special_price_CLS()
        {
              opp = new operations();
              All_prices = new DataSet();
        }


        public void Update_Price(long id, long customer_id, long item_id, decimal Price_value)
        {
            string mysql = "UPDATE Capital_Money SET customer_id = '" + customer_id + "', item_id = " + item_id + ", Price_value= " + Price_value + " WHERE(Id = " + id + ")";
            opp.update_me(mysql);
        }
        public DataSet  GET_All_prices()
        {
            string mysql = "SELECT * FROM Special_Price_list";
            All_prices = opp.return_dataset(mysql);
            return All_prices;
        }
         public void add_NewPrice(long customer_id,long item_id, decimal Price_value)
        {
            string sql;
            sql = "INSERT INTO Special_Price_list(customer_id, Item_id,Value)";
            sql += " VALUES(" + customer_id + "  , " + item_id + " ' " + Price_value + ")";
            opp.insert_me(sql);

        }
         

        public void insert_pulled_capital(ref int Status,string DATE, int Capital_id, decimal vlaue, long Bank_id, string notes)
        {
            ArrayList arr = new ArrayList();
            string mysql = "INSERT INTO capital_pulled(DATE,Capital_id, Value, Banke_id, Notes)";
            mysql += "VALUES(CONVERT(DATETIME,'" + DATE + "', 102)," + Capital_id + " ," + vlaue + "," + Bank_id + ",' " + notes + "')";
            arr.Add(mysql );
            mysql = " UPDATE Banks SET Last_Balance = Last_Balance - " + vlaue + " WHERE (id = " + Bank_id + ")";
            arr.Add(mysql);
              Status=opp.insert_me_transaction(arr);
        }
        
    }
}
