using Pharmacy_API.Filters.Unit;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Repositories.Unit
{
    public interface IUnitRepository : IRepository<Models.Unit.Unit>
    {
        Task<Pharmacy_API.Models.Unit.Unit?> GetByIdAsync(int id, bool? isDeep = false);
        Task<PagedDto<Models.Unit.Unit>> GetListAsync(UnitFilter filter);
        //put your code here
    }
}
