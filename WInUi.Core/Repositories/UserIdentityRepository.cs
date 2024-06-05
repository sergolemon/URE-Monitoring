using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Db;
using URE.Core.Models.Identity;

namespace URE.Core.Repositories
{
    public class UserIdentityRepository : IUserIdentityRepository
    {
        private readonly MeteoDbContextFactory _dbContextFactory;

        public UserIdentityRepository(MeteoDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<UserIdentity> Get()
        {
            MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            var identityUser = await dbContext.UserIdentity.Include(x => x.User).FirstOrDefaultAsync();

            if (identityUser != null)
            {
                identityUser.Roles = dbContext.Roles.Where(x => dbContext.UserRoles.Any(y => x.Id == y.RoleId && identityUser.UserId == y.UserId)).Select(x => x.Name).ToList();
            }

            return identityUser;
        }

        public async Task Add(UserIdentity identity)
        {
            MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);
            dbContext.Add(identity);
            await dbContext.SaveChangesAsync();
        }

        public async Task Update(UserIdentity identity)
        {
            MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);
            dbContext.Update(identity);
            await dbContext.SaveChangesAsync();
        }

        public async Task Remove(UserIdentity identity)
        {
            MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);
            dbContext.Remove(identity);
            await dbContext.SaveChangesAsync();
        }
    }
}
