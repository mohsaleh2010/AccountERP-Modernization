using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models 
{
    public class Govern_List
    {   //AlterBD_(ref opp, "Gov_City_List", "Gov_Country_ListId", "bigint", "0", "0");
        //AlterBD_(ref opp, "Gov_City_List", "Govern_AreaId", "bigint", "0", "0");
        //AlterBD_(ref opp, "Gov_City_List", "Govern_ListId", "bigint", "0", "0");
        public long Id { get; set; }
        public long Gov_Country_ListId { get; set; }
        public long Govern_Area_ListId { get; set; }

        public string  Gov_Name { get; set; }
        public string  Code { get; set; }
        public DateTime? OperationDate { get; set; }
        public int? User_Id { get; set; }
        public int? Branch_Id { get; set; }
        public DateTime? Insert_ServerDateTime { get; set; }
        public DateTime? Update_ServerDateTime { get; set; }
        public DateTime? EditOperationDate { get; set; }
        public int? Edit_User_Id { get; set; }
        public long? ProcessNo { get; set; }


        public Gov_Country_List Gov_Country_List { get; set; }
        public Govern_Area_List Govern_Area_List { get; set; }
         
       
        public List<Gov_City_List> Gov_City_List { get; set; }

    }
}
