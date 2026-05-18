using Pharmacy_API.Filters.Brand;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Repositories.Brand
{
    public interface IBrandRepository : IRepository<Models.Brand.Brand>
    {
        Task<Models.Brand.Brand?> GetByIdAsync(int id, bool? isDeep = false);
        Task<PagedDto<Models.Brand.Brand>> GetListAsync(BrandFilter filter);
        //put your code here
    }
}
