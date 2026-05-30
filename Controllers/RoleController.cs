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
    public class RolesController : ApiControllerBase
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IRoleService _roleService;
        #endregion

        #region Constructors
        public RolesController(
            ILogger<RolesController> logger,
            IRoleService roleService)
        {
            _logger = logger;
            _roleService = roleService;
        }
        #endregion

        #region Insert Role
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RoleDto?>> Insert([FromBody] RoleRequestDto roleRequestDto)
        {
            RoleDto? roleDto = await _roleService.InsertRoleAsync(roleRequestDto);
            roleRequestDto.SetUserID(await GetUserID());

            if (roleDto != null)
            {
                _logger.LogInformation("Insert Success");

                return StatusCode(201, roleDto);
            }

            return StatusCode(500);
        }
        #endregion

        #region Update Role
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Update([FromBody] RoleRequestDto roleRequestDto, string id)
        {
            roleRequestDto.SetUserID(await GetUserID());
            int total = await _roleService.UpdateRoleAsync(roleRequestDto, id);
            if (total > 0)
            {
                _logger.LogInformation("Update Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Delete Role
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Delete(string id)
        {
            RoleDto? roleDto = await _roleService.GetRoleAsync(id, false);
            if (roleDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }

            int total = await _roleService.DeleteRoleAsync(id);
            if (total > 0)
            {
                _logger.LogInformation("Delete Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Get Role
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RoleDto?>> Get(string id, bool? isDeep)
        {
            RoleDto? roleDto = await _roleService.GetRoleAsync(id, isDeep ?? false);
            if (roleDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }

            return Ok(roleDto);
        }
        #endregion

        #region Get List Roles
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedDto<RoleDto>>> GetList([FromQuery] RoleFilterDto filterDto)
        {
            return Ok(await _roleService.GetListRolesAsync(filterDto));
        }
        #endregion
        #region Assign Policies to Role
        [HttpPost("{roleId}/policies")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignPolicies(string roleId, [FromBody] List<string> policyIds)
        {
            var result = await _roleService.AssignPoliciesToRoleAsync(roleId, policyIds);
            if (result)
            {
                _logger.LogInformation($"Policies assigned to role {roleId}");
                return Ok(new { Message = "Policies assigned successfully" });
            }

            return BadRequest(new ErrorResponseDto { Code = "AssignFailed", Description = "Failed to assign policies" });
        }
        #endregion

        #region Get Policies of Role
        [HttpGet("{roleId}/policies")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolePolicies(string roleId)
        {
            var policies = await _roleService.GetPoliciesByRoleIdAsync(roleId);
            return Ok(policies);
        }
        #endregion

        #region Get Permissions of Role
        [HttpGet("{roleId}/permissions")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolePermissions(string roleId)
        {
            var permissions = await _roleService.GetPermissionsByRoleIdAsync(roleId);
            return Ok(permissions);
        }
        #endregion
    }
}