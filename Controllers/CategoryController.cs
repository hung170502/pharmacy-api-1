using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Category;
using Pharmacy_API.Services.Category;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Controllers
{
    [Route("api/Catalog/[controller]")]
    [ApiController]
    public class CategoryController : ApiControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        #region Insert Category
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto?>> Insert([FromForm] CategoryRequestDto categoryRequestDto)
        {
            categoryRequestDto.SetUserID(await GetUserID());
            CategoryDto? categoryDto = await _categoryService.InsertCategoryAsync(categoryRequestDto);

            if (categoryDto != null)
            {
                _logger.LogInformation("Insert Success");
                return StatusCode(201, categoryDto);
            }

            return StatusCode(500);
        }
        #endregion

        #region Update Category
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Update([FromForm] CategoryRequestDto categoryRequestDto, int id)
        {
            categoryRequestDto.SetUserID(await GetUserID());
            int total = await _categoryService.UpdateCategoryAsync(categoryRequestDto, id);
            if (total > 0)
            {
                _logger.LogInformation("Update Success");
                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Delete Category
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Delete(int id)
        {
            CategoryDto? categoryDto = await _categoryService.GetCategoryAsync(id, false);
            if (categoryDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.CategoryNotFound, Description = "Category not found." });
            }

            int total = await _categoryService.DeleteCategoryAsync(id);
            if (total > 0)
            {
                _logger.LogInformation("Delete Success");
                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Get Category
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto?>> Get(int id, bool? isDeep)
        {
            CategoryDto? categoryDto = await _categoryService.GetCategoryAsync(id, isDeep ?? false);
            if (categoryDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.CategoryNotFound, Description = "Category not found" });
            }

            return Ok(categoryDto);
        }
        #endregion

        #region Get List Categories
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedDto<CategoryDto>>> GetList([FromQuery] CategoryFilterDto filterDto)
        {
            return Ok(await _categoryService.GetListCategoriesAsync(filterDto));
        }
        #endregion
    }
}