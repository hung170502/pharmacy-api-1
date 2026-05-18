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
    public partial class PolicyRepository : AbstractEfRepository<AccountContext, Policy>, IPolicyRepository
    {
        public PolicyRepository(AccountContext db, ILogger<PolicyRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<Policy> IncludeDeepObjects(IQueryable<Policy> query)
        {
            return query.Include(o => o.PolicyPermissions).ThenInclude(p => p.Permission);
            //return query;
        }

        #region Get By Id
        public async Task<Policy?> GetByIdAsync(string id, bool? isDeep = false)
        {
            IQueryable<Policy> query = _db.Policies;
            query = query.Where(o => o.Id == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<Policy>> GetListAsync(PolicyFilter filter)
        {
            int total = 0;
            IQueryable<Policy> query = _db.Policies;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.Id);
                total = await queryCount.CountAsync();
            }
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                string keyword = filter.Keyword.ToLower();

                query = query = query.Where(policy =>
                        policy.Name.ToLower().Contains(keyword) ||
                        policy.Description.ToLower().Contains(keyword));
                total = await query.CountAsync();

            }
            if (filter.IsDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            switch (filter.OrderBy)
            {
                case "Id":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id);
                    break;
                case "Name":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Name) : query.OrderBy(o => o.Name);
                    break;
                case "Description":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Description) : query.OrderBy(o => o.Description);
                    break;
                case "Sort":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Sort) : query.OrderBy(o => o.Sort);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<Policy>(total, await query.ToListAsync());
        }
        #endregion
    }
}