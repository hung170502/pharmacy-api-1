using Pharmacy_API.Filters.Country;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Repositories.Country
{
    public interface ICountryRepository : IRepository<Models.Country.Country>
    {
        Task<Pharmacy_API.Models.Country.Country?> GetByIdAsync(int id, bool? isDeep = false);
        Task<PagedDto<Models.Country.Country>> GetListAsync(CountryFilter filter);
        //put your code here
    }
}
