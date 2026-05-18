using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using Pharmacy_API.Supports;
using Pharmacy_API.Context;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Repositories.Account
{
    public partial class RoleRepository : AbstractEfRepository<AccountContext, Role>, IRoleRepository
    {
        public RoleRepository(AccountContext db, ILogger<RoleRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<Role> IncludeDeepObjects(IQueryable<Role> query)
        {
            return query.Include(o => o.RolePolicies).ThenInclude(p => p.Policy);
            //return query;
        }

        #region Get By Id
        public async Task<Role?> GetByIdAsync(string id, bool? isDeep = false)
        {
            IQueryable<Role> query = _db.Roles;
            query = query.Where(o => o.Id == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<Role>> GetListAsync(RoleFilter filter)
        {
            int total = 0;
            IQueryable<Role> query = _db.Roles;

            //query where
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                string keyword = filter.Keyword.ToLower();

                query = query = query.Where(role =>
                      role.Name.ToLower().Contains(keyword) ||
                      role.Id.ToLower().Contains(keyword)
                      );
                total = await query.CountAsync();
            }
            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.Id);
                total = await queryCount.CountAsync();
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
                case "NormalizedName":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.NormalizedName) : query.OrderBy(o => o.NormalizedName);
                    break;
                case "ConcurrencyStamp":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.ConcurrencyStamp) : query.OrderBy(o => o.ConcurrencyStamp);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<Role>(total, await query.ToListAsync());
        }
        #endregion

    }
}