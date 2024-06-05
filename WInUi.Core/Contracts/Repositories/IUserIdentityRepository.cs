using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Identity;

namespace URE.Core.Contracts.Repositories
{
    public interface IUserIdentityRepository
    {
        Task<UserIdentity> Get();
        Task Add(UserIdentity identity);
        Task Remove(UserIdentity identity);
        Task Update(UserIdentity identity);
    }
}
