using AutoMapper;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Brand;
using Pharmacy_API.Dtos.Category;
using Pharmacy_API.Filters.Account;
using Pharmacy_API.Filters.Brand;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Models.Brand;
using Pharmacy_API.Models.Category;
using Pharmacy_API.Repositories.Brand;
using Pharmacy_API.Services.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Brand
{
    public class BrandService : IBrandService
    {
        private readonly ILogger<BrandService> _logger;
        private readonly IMapper _mapper;
        private readonly IBrandRepository _brandRepository;

        public BrandService(
            ILogger<BrandService> logger,
            IMapper mapper,
            IBrandRepository brandRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _brandRepository = brandRepository;
        }

        #region Insert Brand
        public async Task<BrandDto?> InsertBrandAsync(BrandRequestDto requestDto)
        {
            _logger.LogInformation("Insert Brand");

            Pharmacy_API.Models.Brand.Brand brand = new Pharmacy_API.Models.Brand.Brand();
            brand.BrandName = requestDto.BrandName;
            brand.Email = requestDto.Email;
            brand.PhoneNumber = requestDto.PhoneNumber;
            brand.BrandImage = requestDto.BrandImage;
            brand.Address = requestDto.Address;
            brand.Description = requestDto.Description;
            brand.Sort = requestDto.Sort;

            Pharmacy_API.Models.Brand.Brand? newBrand = await _brandRepository.InsertAsync(brand);

            return newBrand == null ? null : _mapper.Map<Pharmacy_API.Models.Brand.Brand, BrandDto>(newBrand);
        }
        #endregion

        #region Update Brand
        public async Task<int> UpdateBrandAsync(BrandRequestDto requestDto, int id)
        {
            _logger.LogInformation("Update Brand");


            Pharmacy_API.Models.Brand.Brand? brand = await _brandRepository.GetByIdAsync(id);
            if (brand != null)
            {
                brand.BrandName = requestDto.BrandName;
                brand.Email = requestDto.Email;
                brand.PhoneNumber = requestDto.PhoneNumber;
                brand.BrandImage = requestDto.BrandImage;
                brand.Address = requestDto.Address;
                brand.Description = requestDto.Description;
                brand.Sort = requestDto.Sort;

                return await _brandRepository.UpdateAsync(brand);
            }

            return 0;
        }
        #endregion

        #region Delete Brand
        public async Task<int> DeleteBrandAsync(int id)
        {
            _logger.LogInformation("Delete Brand");

            return await _brandRepository.DeleteAsync(id);
        }
        #endregion


        #region Get Brand
        public async Task<BrandDto?> GetBrandAsync(int id, bool isDeep = false)
        {
            _logger.LogInformation("Get Brand");


            Pharmacy_API.Models.Brand.Brand? brand = await _brandRepository.GetByIdAsync(id, isDeep);
            if (brand != null)
            {
                return _mapper.Map<Pharmacy_API.Models.Brand.Brand, BrandDto>(brand);
            }

            return null;
        }
        #endregion

        #region Get List Brands
        public async Task<PagedDto<BrandDto>> GetListBrandsAsync(BrandFilterDto filterDto)
        {
            _logger.LogInformation("GetList Brands");

            PagedDto<Pharmacy_API.Models.Brand.Brand> dt = await _brandRepository.GetListAsync(_mapper.Map<BrandFilterDto, BrandFilter>(filterDto));

            List<BrandDto> dtos = new List<BrandDto>();
            foreach (Pharmacy_API.Models.Brand.Brand item in dt.Data)
            {
                dtos.Add(_mapper.Map<Pharmacy_API.Models.Brand.Brand, BrandDto>(item));
            }

            return new PagedDto<BrandDto>(dt.TotalRecords, dtos);
        }
        #endregion
    }
}
