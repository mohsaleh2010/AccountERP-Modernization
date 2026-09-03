// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{


 
    public class Account_BranchesVM
    {
        
       
        public long? Id { get; set; }
        public long? USER_DETAILES_Id { get; set; }

        public string Branch_Name { get; set; }

        
        public string Address { get; set; }

        public long? Code { get; set; }

     
        public int? Currency_Id { get; set; }

   
        public string Currency_Name { get; set; }

        

     
        public int? User_Id { get; set; }

 
        public int? Branch_Id { get; set; }

  
        
    }
}