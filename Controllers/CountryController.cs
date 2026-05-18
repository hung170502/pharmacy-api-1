using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Brand;
using Pharmacy_API.Dtos.Country;
using Pharmacy_API.Services.Brand;
using Pharmacy_API.Services.Country;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Controllers
{
    [Route("api/Catalog/[controller]")]
    [ApiController]
    public class CountryController : ApiControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly ILogger<CountryController> _logger;

        public CountryController(ICountryService countryService, ILogger<CountryController> logger)
        {
            _countryService = countryService;
            _logger = logger;
        }

        #region Insert Country
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountryDto?>> Insert([FromBody] CountryRequestDto countryRequestDto)
        {
            CountryDto? countryDto = await _countryService.InsertCountryAsync(countryRequestDto);
            countryRequestDto.SetUserID(await GetUserID());

            if (countryRequestDto != null)
            {
                _logger.LogInformation("Insert Success");

                return StatusCode(201, countryDto);
            }

            return StatusCode(500);
        }
        #endregion

        #region Update Country
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Update([FromBody] CountryRequestDto countryRequestDto, int id)
        {
            countryRequestDto.SetUserID(await GetUserID());
            int total = await _countryService.UpdateCountryAsync(countryRequestDto, id);
            if (total > 0)
            {
                _logger.LogInformation("Update Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Delete Country
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Delete(int id)
        {
            CountryDto? countryDto = await _countryService.GetCountryAsync(id, false);
            if (countryDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.CountryNotFound, Description = "Country not found." });
            }

            int total = await _countryService.DeleteCountryAsync(id);
            if (total > 0)
            {
                _logger.LogInformation("Delete Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Get Country
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountryDto?>> Get(int id, bool? isDeep)
        {
            CountryDto? countryDto = await _countryService.GetCountryAsync(id, isDeep ?? false);
            if (countryDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.CountryNotFound, Description = "Country not found" });
            }

            return Ok(countryDto);
        }
        #endregion

        #region Get List Countries
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedDto<CountryDto>>> GetList([FromQuery] CountryFilterDto filterDto)
        {
            return Ok(await _countryService.GetListCountriesAsync(filterDto));
        }
        #endregion
    }
}
