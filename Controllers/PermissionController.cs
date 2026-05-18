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
    public class PermissionsController : ApiControllerBase
    {
		#region Fields
		private readonly ILogger _logger;
		private readonly IPermissionService _permissionService;
		#endregion

		#region Constructors
		public PermissionsController(
			ILogger<PermissionsController> logger,
			IPermissionService permissionService)
        {
			_logger = logger;
			_permissionService = permissionService;
        }
		#endregion

		#region Insert Permission
		[HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PermissionDto?>> Insert([FromBody]PermissionRequestDto permissionRequestDto)
        {
			PermissionDto? permissionDto = await _permissionService.InsertPermissionAsync(permissionRequestDto);
			permissionRequestDto.SetUserID(await GetUserID());

			if(permissionDto != null)
			{
				_logger.LogInformation("Insert Success");

				return StatusCode(201, permissionDto);
			}

			return StatusCode(500);
        }
		#endregion

		#region Update Permission
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Update([FromBody]PermissionRequestDto permissionRequestDto, string id)
        {
			permissionRequestDto.SetUserID(await GetUserID());
			int total = await _permissionService.UpdatePermissionAsync(permissionRequestDto, id);
			if (total > 0)
			{
				_logger.LogInformation("Update Success");

				return Ok(total);
			}

			return StatusCode(500);
        }
        #endregion

		#region Delete Permission
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Delete(string id)
        {
			PermissionDto? permissionDto = await _permissionService.GetPermissionAsync(id, false);
			if(permissionDto == null)
			{
				return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found" });
			}

			int total = await _permissionService.DeletePermissionAsync(id);
			if (total > 0)
			{
				_logger.LogInformation("Delete Success");

				return Ok(total);
			}

			return StatusCode(500);
        }
        #endregion

		#region Get Permission
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PermissionDto?>> Get(string id, bool? isDeep)
        {
			PermissionDto? permissionDto = await _permissionService.GetPermissionAsync(id, isDeep ?? false);
			if(permissionDto == null)
			{
				return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description ="User not found"});
			}

			return Ok(permissionDto);
        }
        #endregion

		#region Get List Permissions
        [HttpGet]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedDto<PermissionDto>>> GetList([FromQuery]PermissionFilterDto filterDto)
        {
			return Ok(await _permissionService.GetListPermissionsAsync(filterDto));
        }
        #endregion

    }
}