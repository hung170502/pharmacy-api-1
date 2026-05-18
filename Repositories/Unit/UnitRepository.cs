using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
using Pharmacy_API.Filters.Unit;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Repositories.Unit
{
    public partial class UnitRepository : AbstractEfRepository<AccountContext, Pharmacy_API.Models.Unit.Unit>, IUnitRepository
    {
        public UnitRepository(AccountContext db, ILogger<UnitRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<Pharmacy_API.Models.Unit.Unit> IncludeDeepObjects(IQueryable<Pharmacy_API.Models.Unit.Unit> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }

        #region Get By Id
        public async Task<Pharmacy_API.Models.Unit.Unit?> GetByIdAsync(int id, bool? isDeep = false)
        {
            IQueryable<Pharmacy_API.Models.Unit.Unit> query = _db.Units;
            query = query.Where(o => o.UnitId == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<Pharmacy_API.Models.Unit.Unit>> GetListAsync(UnitFilter filter)
        {
            int total = 0;
            IQueryable<Pharmacy_API.Models.Unit.Unit> query = _db.Units;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.UnitId);
                total = await queryCount.CountAsync();
            }
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                string keyword = filter.Keyword.ToLower();

                //query = query = query.Where(policy =>
                //        policy.Name.ToLower().Contains(keyword));
                total = await query.CountAsync();

            }
            if (filter.IsDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            switch (filter.OrderBy)
            {
                case "Id":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.UnitId) : query.OrderBy(o => o.UnitId);
                    break;
                case "Name":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.UnitName) : query.OrderBy(o => o.UnitName);
                    break;
                case "Sort":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Sort) : query.OrderBy(o => o.Sort);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.UnitId) : query.OrderBy(o => o.UnitId);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<Pharmacy_API.Models.Unit.Unit>(total, await query.ToListAsync());
        }
        #endregion
    }
}
