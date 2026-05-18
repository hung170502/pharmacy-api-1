using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
using Pharmacy_API.Filters.Brand;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Repositories.Brand
{
    public partial class BrandRepository : AbstractEfRepository<AccountContext, Pharmacy_API.Models.Brand.Brand>, IBrandRepository
    {
        public BrandRepository(AccountContext db, ILogger<BrandRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<Pharmacy_API.Models.Brand.Brand> IncludeDeepObjects(IQueryable<Pharmacy_API.Models.Brand.Brand> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }

        #region Get By Id
        public async Task<Pharmacy_API.Models.Brand.Brand?> GetByIdAsync(int id, bool? isDeep = false)
        {
            IQueryable<Pharmacy_API.Models.Brand.Brand> query = _db.Brands;
            query = query.Where(o => o.BrandId == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<Pharmacy_API.Models.Brand.Brand>> GetListAsync(BrandFilter filter)
        {
            int total = 0;
            IQueryable<Pharmacy_API.Models.Brand.Brand> query = _db.Brands;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.BrandId);
                total = await queryCount.CountAsync();
            }
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                string keyword = filter.Keyword.ToLower();

                query = query = query.Where(policy =>
                        policy.BrandName.ToLower().Contains(keyword));
                total = await query.CountAsync();

            }
            if (filter.IsDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            switch (filter.OrderBy)
            {
                case "Id":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.BrandId) : query.OrderBy(o => o.BrandId);
                    break;
                case "Name":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.BrandName) : query.OrderBy(o => o.BrandName);
                    break;
                case "Sort":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Sort) : query.OrderBy(o => o.Sort);
                    break;
                case "Email":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Email) : query.OrderBy(o => o.Email);
                    break;
                case "Description":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Description) : query.OrderBy(o => o.Description);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.BrandId) : query.OrderBy(o => o.BrandId);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<Pharmacy_API.Models.Brand.Brand>(total, await query.ToListAsync());
        }
        #endregion
    }
}
