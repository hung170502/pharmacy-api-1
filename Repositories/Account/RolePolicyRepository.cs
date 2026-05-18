using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pharmacy_API.Supports;
using Pharmacy_API.Context;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Repositories.Account
{
    public partial class RolePolicyRepository : AbstractEfRepository<AccountContext, RolePolicy>, IRolePolicyRepository
    {
        public RolePolicyRepository(AccountContext db, ILogger<RolePolicyRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<RolePolicy> IncludeDeepObjects(IQueryable<RolePolicy> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }
        #region Delete By Role Id
        public async Task<int> DeleteByRoleIdAsync(string roleId)
        {
            var rolePoliciesToDelete = await _db.RolePolicies
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            if (rolePoliciesToDelete != null && rolePoliciesToDelete.Any())
            {
                _db.RolePolicies.RemoveRange(rolePoliciesToDelete);
                return await _db.SaveChangesAsync();
            }

            return 0;
        }

        #endregion

        #region Get List
        public async Task<PagedDto<RolePolicy>> GetListAsync(RolePolicyFilter filter)
        {
            int total = 0;
            IQueryable<RolePolicy> query = _db.RolePolicies;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.RoleId);
                total = await queryCount.CountAsync();
            }

            if (filter.IsDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            switch (filter.OrderBy)
            {
                case "RoleId":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.RoleId) : query.OrderBy(o => o.RoleId);
                    break;
                case "PolicyId":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.PolicyId) : query.OrderBy(o => o.PolicyId);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.RoleId) : query.OrderBy(o => o.RoleId);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<RolePolicy>(total, await query.ToListAsync());
        }
        #endregion

        #region Get Policy Ids For Role
        public async Task<IEnumerable<string>> GetPolicyIdsForRoleAsync(string roleId)
        {
            var permissionSetIds = await _db.RolePolicies
                .Where(rps => rps.RoleId == roleId)
                .Select(rps => rps.PolicyId)
                .ToListAsync();

            return permissionSetIds;
        }
        #endregion
    }
}