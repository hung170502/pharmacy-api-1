using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Category;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Category
{
    public interface ICategoryService
    {
        Task<CategoryDto?> InsertCategoryAsync(CategoryRequestDto requestDto);
        Task<int> UpdateCategoryAsync(CategoryRequestDto requestDto, int id);
        Task<int> DeleteCategoryAsync(int id);
        Task<CategoryDto?> GetCategoryAsync(int id, bool isDeep = false);
        Task<PagedDto<CategoryDto>> GetListCategoriesAsync(CategoryFilterDto filterDto);
    }
}
