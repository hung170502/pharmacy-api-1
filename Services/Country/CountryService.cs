using AutoMapper;
using Pharmacy_API.Dtos.Country;
using Pharmacy_API.Filters.Country;
using Pharmacy_API.Repositories.Country;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Country
{
    public class CountryService : ICountryService
    {
        private readonly ILogger<CountryService> _logger;
        private readonly IMapper _mapper;
        private readonly ICountryRepository _countryRepository;

        public CountryService(
            ILogger<CountryService> logger,
            IMapper mapper,
            ICountryRepository countryRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _countryRepository = countryRepository;
        }

        #region Insert Country
        public async Task<CountryDto?> InsertCountryAsync(CountryRequestDto requestDto)
        {
            _logger.LogInformation("Insert Country");

            Pharmacy_API.Models.Country.Country country = new Pharmacy_API.Models.Country.Country();
            country.CountryName = requestDto.CountryName;
            country.Sort = requestDto.Sort;

            Pharmacy_API.Models.Country.Country? newCountry = await _countryRepository.InsertAsync(country);

            return newCountry == null ? null : _mapper.Map<Pharmacy_API.Models.Country.Country, CountryDto>(newCountry);
        }
        #endregion

        #region Update Country
        public async Task<int> UpdateCountryAsync(CountryRequestDto requestDto, int id)
        {
            _logger.LogInformation("Update Country");

            Pharmacy_API.Models.Country.Country? country = await _countryRepository.GetByIdAsync(id);
            if (country != null)
            {
                country.CountryName = requestDto.CountryName;
                country.Sort = requestDto.Sort;

                return await _countryRepository.UpdateAsync(country);
            }

            return 0;
        }
        #endregion

        #region Delete Country
        public async Task<int> DeleteCountryAsync(int id)
        {
            _logger.LogInformation("Delete Country");

            return await _countryRepository.DeleteAsync(id);
        }
        #endregion

        #region Get Coutnry
        public async Task<CountryDto?> GetCountryAsync(int id, bool isDeep = false)
        {
            _logger.LogInformation("Get Country");


            Pharmacy_API.Models.Country.Country? country = await _countryRepository.GetByIdAsync(id, isDeep);
            if (country != null)
            {
                return _mapper.Map<Pharmacy_API.Models.Country.Country, CountryDto>(country);
            }

            return null;
        }
        #endregion

        #region Get List Brands
        public async Task<PagedDto<CountryDto>> GetListCountriesAsync(CountryFilterDto filterDto)
        {
            _logger.LogInformation("GetList Countries");

            PagedDto<Pharmacy_API.Models.Country.Country> dt = await _countryRepository.GetListAsync(_mapper.Map<CountryFilterDto, CountryFilter>(filterDto));

            List<CountryDto> dtos = new List<CountryDto>();
            foreach (Pharmacy_API.Models.Country.Country item in dt.Data)
            {
                dtos.Add(_mapper.Map<Pharmacy_API.Models.Country.Country, CountryDto>(item));
            }

            return new PagedDto<CountryDto>(dt.TotalRecords, dtos);
        }
        #endregion
    }
}
