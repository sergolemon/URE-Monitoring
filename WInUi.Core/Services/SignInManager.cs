using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Contracts.Services;
using URE.Core.Models.Identity;
using URE.Core.Contracts.Repositories;
using Microsoft.AspNetCore.Identity;

namespace URE.Core.Services
{
    public class SignInManager : ISignInManager
    {
        public event Action<UserIdentity> Login;

        private UserIdentity _identity = new UserIdentity();

        public UserIdentity Identity { get => _identity; }

        private readonly IUserIdentityRepository _userIdentityRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public SignInManager(IUserIdentityRepository userIdentityRepository, UserManager<ApplicationUser> userManager) 
        {
            _userIdentityRepository = userIdentityRepository;
            _userManager = userManager;
        }

        public async Task SignInAsync(ApplicationUser user)
        { 
            if(!_identity.IsAuthenticated)
            {
                UserIdentity userIdentity = new UserIdentity
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = await _userManager.GetRolesAsync(user)
                };

                await _userIdentityRepository.Add(userIdentity);
                userIdentity.User = user;
                _identity = userIdentity;
                Login?.Invoke(_identity);
            }
        }

        public async Task SignOutAsync()
        {
            if(_identity.IsAuthenticated)
            {
                await _userIdentityRepository.Remove(_identity);
                _identity = new UserIdentity();
            }
        }

        public async Task Initialize()
        {
            _identity = await _userIdentityRepository.Get() ?? new UserIdentity();
        }
    }
}
