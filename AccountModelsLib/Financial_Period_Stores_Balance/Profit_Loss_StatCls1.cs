
using System;
using System.Collections.Generic;
using System.Text;
 
using System.Data;
 
using AccountModelsLib;
using AccountModelsLib.Models;
using System.Collections;

namespace AccountModelsLib
{
    public  class Profit_Loss_StatCls1
    {
        public Profit_Loss_StatCls1()
        {

        }
        public void Get_Active_Financial_Period(ref int Excp, ref DateTime DateFrom, ref DateTime DateTo, ref int FirstPeriod)
        {

            string mysql = " SELECT * from  Financial_Period where Done=0";
            mysql = mysql + " ORDER BY Id DESC ";
            operations opp = new operations();
            DataSet ds = opp.return_dataset(mysql);

            if (ds.Tables[0].Rows.Count > 0)
            {
                DateFrom = DateTime.Parse(ds.Tables[0].Rows[0]["DateFrom"].ToString());
                DateTo = DateTime.Parse(ds.Tables[0].Rows[0]["DateTo"].ToString());
                FirstPeriod = int.Parse(ds.Tables[0].Rows[0]["FirstPeriod"].ToString());
                Excp = 0;

            }
            else
            {
                Excp = -1;
            }
        }

    }
}
