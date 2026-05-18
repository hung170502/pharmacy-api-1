using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
using Pharmacy_API.Filters.Brand;
using Pharmacy_API.Filters.Country;
using Pharmacy_API.Repositories.Brand;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Repositories.Country
{
    public partial class CountryRepository : AbstractEfRepository<AccountContext, Pharmacy_API.Models.Country.Country>, ICountryRepository
    {
        public CountryRepository(AccountContext db, ILogger<CountryRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<Pharmacy_API.Models.Country.Country> IncludeDeepObjects(IQueryable<Pharmacy_API.Models.Country.Country> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }

        #region Get By Id
        public async Task<Pharmacy_API.Models.Country.Country?> GetByIdAsync(int id, bool? isDeep = false)
        {
            IQueryable<Pharmacy_API.Models.Country.Country> query = _db.Countries;
            query = query.Where(o => o.CountryId == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<Pharmacy_API.Models.Country.Country>> GetListAsync(CountryFilter filter)
        {
            int total = 0;
            IQueryable<Pharmacy_API.Models.Country.Country> query = _db.Countries;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.CountryId);
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
                    query = filter.IsDescending ? query.OrderByDescending(o => o.CountryId) : query.OrderBy(o => o.CountryId);
                    break;
                case "Name":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.CountryName) : query.OrderBy(o => o.CountryName);
                    break;
                case "Sort":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Sort) : query.OrderBy(o => o.Sort);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.CountryId) : query.OrderBy(o => o.CountryId);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<Pharmacy_API.Models.Country.Country>(total, await query.ToListAsync());
        }
        #endregion
    }
}
