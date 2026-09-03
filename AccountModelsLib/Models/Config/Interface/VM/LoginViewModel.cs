// AccountERP.Models/Menus/SysConfigMenus.cs
using AccountModelsLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{


    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter username / برجاء إدخال اسم المستخدم")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter password / برجاء إدخال كلمة المرور")]
        public string Password { get; set; } = string.Empty;
    }
}