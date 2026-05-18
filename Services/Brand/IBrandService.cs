using Pharmacy_API.Dtos.Brand;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Brand
{
    public interface IBrandService
    {
        Task<BrandDto?> InsertBrandAsync(BrandRequestDto requestDto);
        Task<int> UpdateBrandAsync(BrandRequestDto requestDto, int id);
        Task<int> DeleteBrandAsync(int id);
        Task<BrandDto?> GetBrandAsync(int id, bool isDeep = false);
        Task<PagedDto<BrandDto>> GetListBrandsAsync(BrandFilterDto filterDto);
    }
}
