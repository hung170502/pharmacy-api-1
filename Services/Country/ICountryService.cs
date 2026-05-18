using Pharmacy_API.Dtos.Country;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Country
{
    public interface ICountryService
    {
        Task<CountryDto?> InsertCountryAsync(CountryRequestDto requestDto);
        Task<int> UpdateCountryAsync(CountryRequestDto requestDto, int id);
        Task<int> DeleteCountryAsync(int id);
        Task<CountryDto?> GetCountryAsync(int id, bool isDeep = false);
        Task<PagedDto<CountryDto>> GetListCountriesAsync(CountryFilterDto filterDto);
    }
}
