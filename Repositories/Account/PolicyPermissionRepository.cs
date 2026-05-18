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
    public partial class PolicyPermissionRepository : AbstractEfRepository<AccountContext, PolicyPermission>, IPolicyPermissionRepository
    {
        public PolicyPermissionRepository(AccountContext db, ILogger<PolicyPermissionRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<PolicyPermission> IncludeDeepObjects(IQueryable<PolicyPermission> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }
        #region Delete By PolicyId
        public async Task<int> DeleteByPolicyIdAsync(string policyId)
        {
            var policyPermissionsToDelete = await _db.PolicyPermissions
                .Where(pp => pp.PolicyId == policyId)
                .ToListAsync();

            if (policyPermissionsToDelete != null && policyPermissionsToDelete.Any())
            {
                _db.PolicyPermissions.RemoveRange(policyPermissionsToDelete);
                return await _db.SaveChangesAsync();
            }

            return 0;
        }
        #endregion

        #region Get By Id
        public async Task<PolicyPermission?> GetByIdAsync(string policyId, string permissionId, bool? isDeep = false)
        {
            IQueryable<PolicyPermission> query = _db.PolicyPermissions;
            query = query.Where(o => o.PolicyId == policyId && o.PermissionId == permissionId);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<PolicyPermission>> GetListAsync(PolicyPermissionFilter filter)
        {
            int total = 0;
            IQueryable<PolicyPermission> query = _db.PolicyPermissions;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.PolicyId);
                total = await queryCount.CountAsync();
            }

            if (filter.IsDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            switch (filter.OrderBy)
            {
                case "PolicyId":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.PolicyId) : query.OrderBy(o => o.PolicyId);
                    break;
                case "PermissionId":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.PermissionId) : query.OrderBy(o => o.PermissionId);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.PermissionId) : query.OrderBy(o => o.PermissionId);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<PolicyPermission>(total, await query.ToListAsync());
        }
        #endregion

        #region Get Permission Ids by PermissionSet Id
        public async Task<IEnumerable<string>> GetPermissionIdsByPolicyIdAsync(string policyId)
        {
            var permissionIds = await _db.PolicyPermissions
                .Where(o => o.PolicyId == policyId)
                .Select(o => o.PermissionId)
                .ToListAsync();

            return permissionIds;
        }
        #endregion
    }
}