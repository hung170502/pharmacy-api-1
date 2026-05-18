using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
using Pharmacy_API.Filters.Category;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Repositories.Category
{
    public partial class CategoryRepository : AbstractEfRepository<AccountContext, Pharmacy_API.Models.Category.Category>, ICategoryRepository
    {
        public CategoryRepository(AccountContext db, ILogger<CategoryRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<Pharmacy_API.Models.Category.Category> IncludeDeepObjects(IQueryable<Pharmacy_API.Models.Category.Category> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }

        #region Get By Id
        public async Task<Pharmacy_API.Models.Category.Category?> GetByIdAsync(int id, bool? isDeep = false)
        {
            IQueryable<Pharmacy_API.Models.Category.Category> query = _db.Categories;
            query = query.Where(o => o.CategoryId == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<Pharmacy_API.Models.Category.Category>> GetListAsync(CategoryFilter filter)
        {
            int total = 0;
            IQueryable<Pharmacy_API.Models.Category.Category> query = _db.Categories;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.CategoryId);
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
                    query = filter.IsDescending ? query.OrderByDescending(o => o.CategoryId) : query.OrderBy(o => o.CategoryId);
                    break;
                case "Name":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.CategoryName) : query.OrderBy(o => o.CategoryName);
                    break;
                case "Sort":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Sort) : query.OrderBy(o => o.Sort);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.CategoryId) : query.OrderBy(o => o.CategoryId);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<Pharmacy_API.Models.Category.Category>(total, await query.ToListAsync());
        }
        #endregion
    }
}
