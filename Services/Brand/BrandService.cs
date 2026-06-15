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
        private readonly CloudinaryService _cloudinaryService;

        public BrandService(
            ILogger<BrandService> logger,
            IMapper mapper,
            IBrandRepository brandRepository,
            CloudinaryService cloudinaryService)
        {
            _logger = logger;
            _mapper = mapper;
            _brandRepository = brandRepository;
            _cloudinaryService = cloudinaryService;
        }

        #region Insert Brand
        public async Task<BrandDto?> InsertBrandAsync(BrandRequestDto requestDto, IFormFile? image)
        {
            _logger.LogInformation("Insert Brand");

            Pharmacy_API.Models.Brand.Brand brand = new Pharmacy_API.Models.Brand.Brand();
            brand.BrandName = requestDto.BrandName;
            brand.Email = requestDto.Email;
            brand.PhoneNumber = requestDto.PhoneNumber;
            brand.Address = requestDto.Address;
            brand.Description = requestDto.Description;
            brand.Sort = requestDto.Sort;

            if (image != null)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(image, "brands");
                if (uploadResult != null && uploadResult.Error == null)
                {
                    brand.BrandImage = uploadResult.SecureUrl.ToString();
                    brand.ImagePublicId = uploadResult.PublicId;
                    _logger.LogInformation($"Uploaded image: {brand.BrandImage}");
                }
                else
                {
                    _logger.LogError($"Upload failed: {uploadResult?.Error?.Message}");
                }
            }

            Pharmacy_API.Models.Brand.Brand? newBrand = await _brandRepository.InsertAsync(brand);

            return newBrand == null ? null : _mapper.Map<Pharmacy_API.Models.Brand.Brand, BrandDto>(newBrand);
        }
        #endregion

        #region Update Brand
        public async Task<int> UpdateBrandAsync(BrandRequestDto requestDto, int id, IFormFile? image)
        {
            _logger.LogInformation("Update Brand");

            Pharmacy_API.Models.Brand.Brand? brand = await _brandRepository.GetByIdAsync(id);
            if (brand != null)
            {
                brand.BrandName = requestDto.BrandName;
                brand.Email = requestDto.Email;
                brand.PhoneNumber = requestDto.PhoneNumber;
                brand.Address = requestDto.Address;
                brand.Description = requestDto.Description;
                brand.Sort = requestDto.Sort;

                if (image != null)
                {
                    if (!string.IsNullOrEmpty(brand.ImagePublicId))
                    {
                        bool deleted = await _cloudinaryService.DeleteImageAsync(brand.ImagePublicId);
                        _logger.LogInformation($"Deleted old image: {deleted}");
                    }

                    var uploadResult = await _cloudinaryService.UploadImageAsync(image, "brands");
                    if (uploadResult != null && uploadResult.Error == null)
                    {
                        brand.BrandImage = uploadResult.SecureUrl.ToString();
                        brand.ImagePublicId = uploadResult.PublicId;
                        _logger.LogInformation($"Uploaded new image: {brand.BrandImage}");
                    }
                }

                return await _brandRepository.UpdateAsync(brand);
            }

            return 0;
        }
        #endregion

        #region Delete Brand
        public async Task<int> DeleteBrandAsync(int id)
        {
            _logger.LogInformation("Delete Brand");

            Pharmacy_API.Models.Brand.Brand? brand = await _brandRepository.GetByIdAsync(id);
            if (brand != null && !string.IsNullOrEmpty(brand.ImagePublicId))
            {
                bool deleted = await _cloudinaryService.DeleteImageAsync(brand.ImagePublicId);
                _logger.LogInformation($"Deleted image: {deleted}");
            }

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