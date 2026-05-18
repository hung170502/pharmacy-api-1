using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Pharmacy_API.Supports;
using Microsoft.AspNetCore.Authorization;
using Pharmacy_API.Services.Account;
using Pharmacy_API.Dtos.Account;

namespace Pharmacy_API.Controllers
{
    [Route("api/Account/[controller]")]
	[ApiController]
    public class PoliciesController : ApiControllerBase
    {
		#region Fields
		private readonly ILogger _logger;
		private readonly IPolicyService _policyService;
		#endregion

		#region Constructors
		public PoliciesController(
			ILogger<PoliciesController> logger,
			IPolicyService policyService)
        {
			_logger = logger;
			_policyService = policyService;
        }
		#endregion

		#region Insert Policy
		[HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PolicyDto?>> Insert([FromBody]PolicyRequestDto policyRequestDto)
        {
			PolicyDto? policyDto = await _policyService.InsertPolicyAsync(policyRequestDto);
			policyRequestDto.SetUserID(await GetUserID());

			if(policyDto != null)
			{
				_logger.LogInformation("Insert Success");

				return StatusCode(201, policyDto);
			}

			return StatusCode(500);
        }
		#endregion

		#region Update Policy
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Update([FromBody]PolicyRequestDto policyRequestDto, string id)
        {
			policyRequestDto.SetUserID(await GetUserID());

			int total = await _policyService.UpdatePolicyAsync(policyRequestDto, id);
			if (total > 0)
			{
				_logger.LogInformation("Update Success");

				return Ok(total);
			}

			return StatusCode(500);
        }
        #endregion

		#region Delete Policy
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Delete(string id)
        {
			PolicyDto? policyDto = await _policyService.GetPolicyAsync(id, false);
			if(policyDto == null)
			{
				return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found" });
			}

			int total = await _policyService.DeletePolicyAsync(id);
			if (total > 0)
			{
				_logger.LogInformation("Delete Success");

				return Ok(total);
			}

			return StatusCode(500);
        }
        #endregion

		#region Get Policy
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PolicyDto?>> Get(string id, bool? isDeep)
        {
			PolicyDto? policyDto = await _policyService.GetPolicyAsync(id, isDeep ?? false);
			if(policyDto == null)
			{
				return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found"});
			}

			return Ok(policyDto);
        }
        #endregion

		#region Get List Policies
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedDto<PolicyDto>>> GetList([FromQuery]PolicyFilterDto filterDto)
        {
			return Ok(await _policyService.GetListPoliciesAsync(filterDto));
        }
        #endregion

    }
}