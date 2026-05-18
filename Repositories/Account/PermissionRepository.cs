using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pharmacy_API.Context;
using Pharmacy_API.Supports;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Repositories.Account
{
    public partial class PermissionRepository : AbstractEfRepository<AccountContext, Permission>, IPermissionRepository
    {
        public PermissionRepository(AccountContext db, ILogger<PermissionRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<Permission> IncludeDeepObjects(IQueryable<Permission> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }

        #region Get By Id
        public async Task<Permission?> GetByIdAsync(string id, bool? isDeep = false)
        {
            IQueryable<Permission> query = _db.Permissions;
            query = query.Where(o => o.Id == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<Permission>> GetListAsync(PermissionFilter filter)
        {
            int total = 0;
            IQueryable<Permission> query = _db.Permissions;

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
                        policy.Name.ToLower().Contains(keyword));
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
                case "Sort":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Sort) : query.OrderBy(o => o.Sort);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<Permission>(total, await query.ToListAsync());
        }
        #endregion
    }
}