using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Contracts.Repositories;
using URE.Core.Contracts.Services;
using URE.Core.Models.Db;
using URE.Core.Models.Identity;
using URE.ViewModels.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace URE.ViewModels
{
    public class UserSettingsVm : ObservableRecipient
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MeteoDbContext _dbContext;
        private readonly IUserIdentityRepository _userIdentityRepository;
        public ISignInManager SignInManager { get; }

        public UserSettingsVm(UserManager<ApplicationUser> userManager, MeteoDbContext dbContext, ISignInManager signInManager, IUserIdentityRepository userIdentityRepository)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _userIdentityRepository = userIdentityRepository;
            SignInManager = signInManager;

            var userConfigs = _dbContext.Users.AsNoTracking().Select(x => new UserConfigVm {
                Id = x.Id,
                CarNumber = x.CarNumber,
                Name = x.Name,
                Surname = x.Surname,
                Patronymic = x.Patronymic,
                IsActive = x.IsActive,
                Login = x.UserName,
                Roles = dbContext.Roles.Where(y => dbContext.UserRoles.Any(z => z.UserId == x.Id && z.RoleId == y.Id)).Select(y => y.Name!).ToList()
            }).ToList();

            foreach (var item in userConfigs)
            {
                item.SetUiStateByIdentity(SignInManager.Identity);
                UserConfigs.Add(item);
            }
        }
        //public UserSettingsVm? SelectedUserSettingsScreen { get; set; }

        private UserConfigVm? selectedUserConfig;
        public UserConfigVm? SelectedUserConfig { get => selectedUserConfig; set => SetProperty(ref selectedUserConfig, value); }

        public ObservableCollection<UserConfigVm> UserConfigs { get; } = new();

        public async Task DeleteUserAsync()
        {
            if (SignInManager.Identity.UserId == SelectedUserConfig.Id) return;

            var user = _userManager.Users.FirstOrDefault(x => x.Id == SelectedUserConfig.Id);

            await _userManager.DeleteAsync(user);
            UserConfigs.Remove(SelectedUserConfig);

            SelectedUserConfig = null;
        }

        public async Task UpdateUserAsync()
        {
            var editUser = _userManager.Users.FirstOrDefault(x => x.Id == SelectedUserConfig.Id);

            editUser.CarNumber = SelectedUserConfig.CarNumber;
            editUser.Name = SelectedUserConfig.Name;
            editUser.Surname = SelectedUserConfig.Surname;
            editUser.Patronymic = SelectedUserConfig.Patronymic;
            editUser.UserName = SelectedUserConfig.Login;
            editUser.IsActive = SelectedUserConfig.IsActive;

            if (!string.IsNullOrEmpty(SelectedUserConfig.Password))
            {
                var hasher = new PasswordHasher<ApplicationUser>();
                editUser.PasswordHash = hasher.HashPassword(editUser, SelectedUserConfig.Password);
            }

            await _userManager.RemoveFromRolesAsync(editUser, await _userManager.GetRolesAsync(editUser));
            await _userManager.UpdateAsync(editUser);
            await _userManager.AddToRolesAsync(editUser, SelectedUserConfig.Roles);

            if(SignInManager.Identity.UserId == editUser.Id)
            {
                SignInManager.Identity.User.Surname = editUser.Surname;
                SignInManager.Identity.User.Name = editUser.Name;
                SignInManager.Identity.User.Patronymic = editUser.Patronymic;
                SignInManager.Identity.UserName = editUser.UserName;
                SignInManager.Identity.User.CarNumber = editUser.CarNumber;

                ShellViewModel.CurrentUserInfo = SignInManager.Identity.User.PersonInfo;
            }

            var existsUserLine = UserConfigs.First(x => x.Id == SelectedUserConfig.Id);
            int index = UserConfigs.IndexOf(existsUserLine);
            SelectedUserConfig.SetUiStateByIdentity(SignInManager.Identity);
            UserConfigs.Insert(index, SelectedUserConfig);
            UserConfigs.Remove(existsUserLine);

            SelectedUserConfig = null;
        }

        public async Task AddNewUserAsync()
        {
            var newUser = new ApplicationUser
            {
                CarNumber = SelectedUserConfig.CarNumber,
                Name = SelectedUserConfig.Name,
                Surname = SelectedUserConfig.Surname,
                Patronymic = SelectedUserConfig.Patronymic,
                UserName = SelectedUserConfig.Login,
                IsActive = SelectedUserConfig.IsActive
            };

            var hasher = new PasswordHasher<ApplicationUser>();
            newUser.PasswordHash = hasher.HashPassword(newUser, SelectedUserConfig.Password);

            await _userManager.CreateAsync(newUser);
            await _userManager.AddToRolesAsync(newUser, SelectedUserConfig.Roles);
            SelectedUserConfig.SetUiStateByIdentity(SignInManager.Identity);
            SelectedUserConfig.Id = newUser.Id;
        }

        public ShellViewModel ShellViewModel { get; set; }

        public void OnNavigated(ShellViewModel shellViewModel)
        {
            ShellViewModel = shellViewModel;

            //PropertyChanged += (s, e) => {
            //    switch (e.PropertyName)
            //    {

            //    }
            //};
        }
    }
}
