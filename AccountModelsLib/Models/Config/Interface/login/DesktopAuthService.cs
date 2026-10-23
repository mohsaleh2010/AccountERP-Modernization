// AccountERP.Models/Menus/SysConfigMenus.cs

using AccountModelsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{


    public class DesktopAuthService : IAuthService
    {
        public DataTable dt_Branch { get; set; } = new DataTable();
        public List<Account_BranchesVM> account_BranchesLst { get; set; } = new List<Account_BranchesVM>();
        private readonly My_DbContext _dbContext; // سياق قاعدة البيانات الخاص بك
        private readonly operations _opp; // سياق قاعدة البيانات الخاص بك
        private readonly MenuRepository _menuRepo;
        private readonly MenuStateService _menuState;
        private EncryptProcess en ;
         
        AuthService service;
        //public MobileAuthService(My_DbContext dbContext, MenuRepository menuRepo, MenuStateService menuState)
        //{
        //    _dbContext = dbContext;
        //    _menuRepo = menuRepo;
        //    _menuState = menuState;
        //    en = new EncryptProcess();
        //}
        private readonly HttpClient _http;
 
        public DesktopAuthService(My_DbContext dbContext, operations opp, MenuRepository menuRepo, MenuStateService menuState)
        {
            _dbContext = dbContext;
            _menuRepo = menuRepo;
            _menuState = menuState;
            _opp = opp;
            en = new EncryptProcess();
            service = new AuthService(dbContext, opp, menuRepo, menuState);
        }
        // نستقبل الـ HttpClient الجاهز في الماوي لضرب الـ API لاحقاً
        public DesktopAuthService(HttpClient http, MenuStateService menuState)
        {
            _http = http;
            _menuState = menuState;
           
        }
        public async Task<bool> LoginAsync(LoginViewModel loginModel)
        {
            bool Result = await service.LoginAsync(loginModel);
            dt_Branch = service.dt_Branch;
            account_BranchesLst = service.account_BranchesLst;
            return Result;
        }

        // 🟢 لا يوجد كوكي دخول على الديسك توب/الموبايل (لا يوجد HttpContext/متصفح يحمله) - غير مستخدمة هنا،
        // موجودة فقط لتحقيق واجهة IAuthService المشتركة مع الويب.
        public Task<bool> RestoreSessionAsync(long userId) => Task.FromResult(false);


        public Task LogoutAsync()
        {
            _menuState.UserAllowedMenus = new System.Collections.Generic.List<AccountModelsLib.MenuViewModel>();
            return Task.CompletedTask;
        }

        public Task<ChangePasswordResult> ChangePasswordAsync(string oldPassword, string newPassword)
            => service.ChangePasswordAsync(oldPassword, newPassword);
    }
    public class LoginResponseResult
    {
        public bool Success { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<MenuViewModel>? AllowedMenus { get; set; }
    }
}