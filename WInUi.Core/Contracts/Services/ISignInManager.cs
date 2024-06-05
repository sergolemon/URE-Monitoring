using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Identity;

namespace URE.Core.Contracts.Services
{
    public interface ISignInManager
    {
        public event Action<UserIdentity> Login;

        UserIdentity Identity { get; }

        Task SignInAsync(ApplicationUser user);

        Task SignOutAsync();

        Task Initialize();
    }
}
