using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
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
        private readonly AccountContext _context;
        private readonly CloudinaryService _cloudinaryService; // THÊM

        public CategoryService(
            ILogger<CategoryService> logger,
            IMapper mapper,
            ICategoryRepository categoryRepository,
            AccountContext context,
            CloudinaryService cloudinaryService) // THÊM
        {
            _logger = logger;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        #region Insert Category
        public async Task<CategoryDto?> InsertCategoryAsync(CategoryRequestDto requestDto)
        {
            _logger.LogInformation("Insert Category");

            Pharmacy_API.Models.Category.Category category = new Pharmacy_API.Models.Category.Category();
            category.CategoryName = requestDto.CategoryName;
            category.ParentId = requestDto.ParentId;
            category.CategoryAlias = requestDto.CategoryAlias;
            category.Sort = requestDto.Sort;

            // Upload ảnh lên Cloudinary nếu có
            if (requestDto.Image != null)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(requestDto.Image, "categories");
                if (uploadResult != null && uploadResult.Error == null)
                {
                    category.CategoryImage = uploadResult.SecureUrl.ToString();
                    category.ImagePublicId = uploadResult.PublicId;
                    _logger.LogInformation($"Uploaded image: {category.CategoryImage}");
                }
            }

            Pharmacy_API.Models.Category.Category? newCategory = await _categoryRepository.InsertAsync(category);

            return newCategory == null ? null : _mapper.Map<Pharmacy_API.Models.Category.Category, CategoryDto>(newCategory);
        }
        #endregion

        #region Update Category
        public async Task<int> UpdateCategoryAsync(CategoryRequestDto requestDto, int id)
        {
            _logger.LogInformation("Update Category");

            Pharmacy_API.Models.Category.Category? category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                category.CategoryName = requestDto.CategoryName;
                category.ParentId = requestDto.ParentId;
                category.CategoryAlias = requestDto.CategoryAlias;
                category.Sort = requestDto.Sort;

                // Nếu có upload ảnh mới
                if (requestDto.Image != null)
                {
                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(category.ImagePublicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(category.ImagePublicId);
                    }

                    // Upload ảnh mới
                    var uploadResult = await _cloudinaryService.UploadImageAsync(requestDto.Image, "categories");
                    if (uploadResult != null && uploadResult.Error == null)
                    {
                        category.CategoryImage = uploadResult.SecureUrl.ToString();
                        category.ImagePublicId = uploadResult.PublicId;
                    }
                }

                return await _categoryRepository.UpdateAsync(category);
            }

            return 0;
        }
        #endregion

        #region Delete Category
        public async Task<int> DeleteCategoryAsync(int id)
        {
            _logger.LogInformation("Delete Category");

            // Xóa ảnh trên Cloudinary trước
            Pharmacy_API.Models.Category.Category? category = await _categoryRepository.GetByIdAsync(id);
            if (category != null && !string.IsNullOrEmpty(category.ImagePublicId))
            {
                await _cloudinaryService.DeleteImageAsync(category.ImagePublicId);
            }

            return await _categoryRepository.DeleteAsync(id);
        }
        #endregion

        #region Get Category
        public async Task<CategoryDto?> GetCategoryAsync(int id, bool isDeep = false)
        {
            _logger.LogInformation("Get Category");

            Pharmacy_API.Models.Category.Category? category = await _categoryRepository.GetByIdAsync(id, isDeep);
            if (category != null)
            {
                return _mapper.Map<Pharmacy_API.Models.Category.Category, CategoryDto>(category);
            }

            return null;
        }
        #endregion

        #region Get List Categories
        public async Task<PagedDto<CategoryDto>> GetListCategoriesAsync(CategoryFilterDto filterDto)
        {
            _logger.LogInformation("GetList Categories");

            PagedDto<Pharmacy_API.Models.Category.Category> dt = await _categoryRepository.GetListAsync(_mapper.Map<CategoryFilterDto, CategoryFilter>(filterDto));

            List<CategoryDto> dtos = new List<CategoryDto>();
            foreach (Pharmacy_API.Models.Category.Category item in dt.Data)
            {
                var dto = _mapper.Map<Pharmacy_API.Models.Category.Category, CategoryDto>(item);

                dto.ProductCount = await _context.Products
                    .CountAsync(p => p.CategoryId == item.CategoryId);

                dtos.Add(dto);
            }

            return new PagedDto<CategoryDto>(dt.TotalRecords, dtos);
        }
        #endregion
    }
}