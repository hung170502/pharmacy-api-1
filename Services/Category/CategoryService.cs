using AutoMapper;
using Microsoft.EntityFrameworkCore;  // ✅ Thêm
using Pharmacy_API.Context;  // ✅ Thêm
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Category;
using Pharmacy_API.Filters.Account;
using Pharmacy_API.Filters.Category;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Models.Category;
using Pharmacy_API.Repositories.Category;
using Pharmacy_API.Services.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;
        private readonly AccountContext _context;  // ✅ Thêm

        public CategoryService(
            ILogger<CategoryService> logger,
            IMapper mapper,
            ICategoryRepository categoryRepository,
            AccountContext context)
        {
            _logger = logger;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        #region Insert Category
        public async Task<CategoryDto?> InsertCategoryAsync(CategoryRequestDto requestDto)
        {
            _logger.LogInformation("Insert Permission");

            Pharmacy_API.Models.Category.Category category = new Pharmacy_API.Models.Category.Category();
            category.CategoryName = requestDto.CategoryName;
            category.ParentId = requestDto.ParentId;
            category.CategoryAlias = requestDto.CategoryAlias;
            category.CategoryImage = requestDto.CategoryImage;
            category.Sort = requestDto.Sort;

            Pharmacy_API.Models.Category.Category? newCategory = await _categoryRepository.InsertAsync(category);

            return newCategory == null ? null : _mapper.Map<Pharmacy_API.Models.Category.Category, CategoryDto>(newCategory);
        }
        #endregion

        #region Update Category
        public async Task<int> UpdateCategoryAsync(CategoryRequestDto requestDto, int id)
        {
            _logger.LogInformation("Update Permission");


            Pharmacy_API.Models.Category.Category? category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                category.CategoryName = requestDto.CategoryName;
                category.ParentId = requestDto.ParentId;
                category.CategoryAlias = requestDto.CategoryAlias;
                category.CategoryImage = requestDto.CategoryImage;
                category.Sort = requestDto.Sort;

                return await _categoryRepository.UpdateAsync(category);
            }

            return 0;
        }
        #endregion

        #region Delete Category
        public async Task<int> DeleteCategoryAsync(int id)
        {
            _logger.LogInformation("Delete Permission");


            return await _categoryRepository.DeleteAsync(id);
        }
        #endregion


        #region Get Category
        public async Task<CategoryDto?> GetCategoryAsync(int id, bool isDeep = false)
        {
            _logger.LogInformation("Get Permission");


            Pharmacy_API.Models.Category.Category? permission = await _categoryRepository.GetByIdAsync(id, isDeep);
            if (permission != null)
            {
                return _mapper.Map<Pharmacy_API.Models.Category.Category, CategoryDto>(permission);
            }

            return null;
        }
        #endregion

        #region Get List Categories
        public async Task<PagedDto<CategoryDto>> GetListCategoriesAsync(CategoryFilterDto filterDto)
        {
            _logger.LogInformation("GetList Permissions");

            PagedDto<Pharmacy_API.Models.Category.Category> dt = await _categoryRepository.GetListAsync(_mapper.Map<CategoryFilterDto, CategoryFilter>(filterDto));

            List<CategoryDto> dtos = new List<CategoryDto>();
            foreach (Pharmacy_API.Models.Category.Category item in dt.Data)
            {
                var dto = _mapper.Map<Pharmacy_API.Models.Category.Category, CategoryDto>(item);

                // ✅ Đếm số sản phẩm trong danh mục
                dto.ProductCount = await _context.Products
                    .CountAsync(p => p.CategoryId == item.CategoryId);

                dtos.Add(dto);
            }

            return new PagedDto<CategoryDto>(dt.TotalRecords, dtos);
        }
        #endregion
    }
}
