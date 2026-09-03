using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
  using AccountModelsLib;  using AccountModelsLib.Models;
 
namespace AccountModelsLib
{
   public  class Journal_DaysListCLS
    {
       public Journal_DaysListCLS()
       {
       }
       public DataTable  Gey_Journal_Codes(ref operations opp )
       {
           opp = new operations();
           string mysql = "Select * from Journal_Codes Order By Code ";

           DataTable dt = opp.return_DataTableWithBuilder(mysql);
           return dt;
       }
        
    }
}
