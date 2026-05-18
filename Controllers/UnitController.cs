using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Country;
using Pharmacy_API.Dtos.Unit;
using Pharmacy_API.Services.Unit;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Controllers
{
    [Route("api/Catalog/[controller]")]
    [ApiController]
    public class UnitController : ApiControllerBase
    {
        private readonly IUnitService _unitService;
        private readonly ILogger<UnitController> _logger;

        public UnitController(IUnitService unitService, ILogger<UnitController> logger)
        {
            _unitService = unitService;
            _logger = logger;
        }

        #region Insert Unit
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UnitDto?>> Insert([FromBody] UnitRequestDto unitRequestDto)
        {
            UnitDto? unitDto = await _unitService.InsertUnitAsync(unitRequestDto);
            unitRequestDto.SetUserID(await GetUserID());

            if (unitRequestDto != null)
            {
                _logger.LogInformation("Insert Success");

                return StatusCode(201, unitDto);
            }

            return StatusCode(500);
        }
        #endregion

        #region Update Unit
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Update([FromBody] UnitRequestDto unitRequestDto, int id)
        {
            unitRequestDto.SetUserID(await GetUserID());
            int total = await _unitService.UpdateUnitAsync(unitRequestDto, id);
            if (total > 0)
            {
                _logger.LogInformation("Update Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Delete Unit
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Delete(int id)
        {
            UnitDto? unitDto = await _unitService.GetUnitAsync(id, false);
            if (unitDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.CountryNotFound, Description = "Unit not found." });
            }

            int total = await _unitService.DeleteUnitAsync(id);
            if (total > 0)
            {
                _logger.LogInformation("Delete Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Get Unit
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UnitDto?>> Get(int id, bool? isDeep)
        {
            UnitDto? unitDto = await _unitService.GetUnitAsync(id, isDeep ?? false);
            if (unitDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.UnitNotFound, Description = "Unit not found" });
            }

            return Ok(unitDto);
        }
        #endregion

        #region Get List Units
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedDto<UnitDto>>> GetList([FromQuery] UnitFilterDto filterDto)
        {
            return Ok(await _unitService.GetListUnitsAsync(filterDto));
        }
        #endregion

    }
}
