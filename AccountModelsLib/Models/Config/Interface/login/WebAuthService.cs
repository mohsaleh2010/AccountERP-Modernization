// AccountERP.Models/Menus/SysConfigMenus.cs

using AccountModelsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{


    public class WebAuthService : IAuthService
    {
        public DataTable dt_Branch { get; set; } = new DataTable();
        public List<Account_BranchesVM> account_BranchesLst { get; set; } = new List<Account_BranchesVM>();
        private readonly My_DbContext _dbContext; // سياق قاعدة البيانات الخاص بك
        private readonly operations _opp; // سياق قاعدة البيانات الخاص بك
        private readonly MenuRepository _menuRepo;
        private readonly MenuStateService _menuState;
        private EncryptProcess en;
        AuthService service;
        public WebAuthService(My_DbContext dbContext, operations opp, MenuRepository menuRepo, MenuStateService menuState)
        {
            _dbContext = dbContext;
            _menuRepo = menuRepo;
            _menuState = menuState;
            _opp = opp;
            en = new EncryptProcess();
            service = new AuthService(dbContext, opp, menuRepo, menuState);
        }

        public async Task<bool> LoginAsync(LoginViewModel loginModel)
        {
            bool Result = await service.LoginAsync(loginModel);
            dt_Branch = service.dt_Branch;
            account_BranchesLst = service.account_BranchesLst;
            return Result;
        }

        public async Task<bool> RestoreSessionAsync(long userId)
        {
            bool Result = await service.RestoreSessionAsync(userId);
            dt_Branch = service.dt_Branch;
            account_BranchesLst = service.account_BranchesLst;
            return Result;
        }

        public Task LogoutAsync()
        {
            _menuState.UserAllowedMenus = new List<MenuViewModel>(); // تفريغ القوائم
            _menuState.SetAuthenticated(false);
            return Task.CompletedTask;
        }

        public Task<ChangePasswordResult> ChangePasswordAsync(string oldPassword, string newPassword)
            => service.ChangePasswordAsync(oldPassword, newPassword);
    }
}