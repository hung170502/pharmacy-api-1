using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Data;
using Pharmacy_API.Supports;
using Pharmacy_API.Context;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Repositories.Account
{
    public partial class UserRoleRepository : AbstractEfRepository<AccountContext, UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(AccountContext db, ILogger<UserRoleRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<UserRole> IncludeDeepObjects(IQueryable<UserRole> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }

        #region Get By Id
        public async Task<UserRole?> GetByIdAsync(string userId, string roleId, bool? isDeep = false)
        {
            IQueryable<UserRole> query = _db.UserRoles;
            query = query.Where(o => o.UserId == userId && o.RoleId == roleId);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<UserRole>> GetListAsync(UserRoleFilter filter)
        {
            int total = 0;
            IQueryable<UserRole> query = _db.UserRoles;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.UserId);
                total = await queryCount.CountAsync();
            }

            if (filter.IsDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            switch (filter.OrderBy)
            {
                case "UserId":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.UserId) : query.OrderBy(o => o.UserId);
                    break;
                case "RoleId":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.RoleId) : query.OrderBy(o => o.RoleId);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.RoleId) : query.OrderBy(o => o.RoleId);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<UserRole>(total, await query.ToListAsync());
        }

        #endregion

        #region Get Role By UserId
        public async Task<ICollection<string>> GetRolesByUserIdAsync(string userId)
        {
            var roleIds = await _db.UserRoles
                              .Where(ur => ur.UserId == userId)
                              .Select(ur => ur.RoleId)
                              .ToListAsync();

            return roleIds;
        }

        #endregion

    }
}