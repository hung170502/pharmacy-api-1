using Pharmacy_API.Filters.Category;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Repositories.Category
{
    public interface ICategoryRepository : IRepository<Models.Category.Category>
    {
        Task<Models.Category.Category?> GetByIdAsync(int id, bool? isDeep = false);
        Task<PagedDto<Models.Category.Category>> GetListAsync(CategoryFilter filter);
        //put your code here
    }
}
