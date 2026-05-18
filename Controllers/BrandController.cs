using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Brand;
using Pharmacy_API.Services.Brand;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Controllers
{
    [Route("api/Catalog/[controller]")]
    [ApiController]
    public class BrandController : ApiControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IBrandService brandService, ILogger<BrandController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        #region Insert Brand
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BrandDto?>> Insert([FromBody] BrandRequestDto brandRequestDto)
        {
            BrandDto? brandDto = await _brandService.InsertBrandAsync(brandRequestDto);
            brandRequestDto.SetUserID(await GetUserID());

            if (brandRequestDto != null)
            {
                _logger.LogInformation("Insert Success");

                return StatusCode(201, brandDto);
            }

            return StatusCode(500);
        }
        #endregion

        #region Update Brand
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Update([FromBody] BrandRequestDto brandRequestDto, int id)
        {
            brandRequestDto.SetUserID(await GetUserID());
            int total = await _brandService.UpdateBrandAsync(brandRequestDto, id);
            if (total > 0)
            {
                _logger.LogInformation("Update Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Delete Brand
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Delete(int id)
        {
            BrandDto? brandDto = await _brandService.GetBrandAsync(id, false);
            if (brandDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.BrandNotFound, Description = "Brand not found." });
            }

            int total = await _brandService.DeleteBrandAsync(id);
            if (total > 0)
            {
                _logger.LogInformation("Delete Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Get Brand
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BrandDto?>> Get(int id, bool? isDeep)
        {
            BrandDto? brandDto = await _brandService.GetBrandAsync(id, isDeep ?? false);
            if (brandDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.BrandNotFound, Description = "Brand not found" });
            }

            return Ok(brandDto);
        }
        #endregion

        #region Get List Brands
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedDto<BrandDto>>> GetList([FromQuery] BrandFilterDto filterDto)
        {
            return Ok(await _brandService.GetListBrandsAsync(filterDto));
        }
        #endregion
    }
}
