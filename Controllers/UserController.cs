using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pharmacy_API.Supports;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Services.Account;
using Pharmacy_API.Dtos.Account;

namespace Pharmacy_API.Controllers
{
    [Route("api/Account/[controller]")]
    [ApiController]
    public class UsersController : ApiControllerBase
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IUpdateUserService _updateUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;  // ✅ Thêm dòng này

        #endregion

        #region Constructors
        public UsersController(
            ILogger<UsersController> logger,
            IUserService userService,
            IUpdateUserService updateUserService,
            UserManager<ApplicationUser> userManager,
            RoleManager<Role> roleManager)
        {
            _logger = logger;
            _userService = userService;
            _updateUserService = updateUserService;
            _userManager = userManager;
                _roleManager = roleManager;  // ✅ Thêm dòng này

        }
        #endregion

        #region Insert User
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto?>> Insert([FromBody] UserRequestDto userRequestDto)
        {
            UserDto? userDto = await _userService.InsertUserAsync(userRequestDto);
            userRequestDto.SetUserID(await GetUserID());

            if (userDto != null)
            {
                _logger.LogInformation("Insert Success");
                userDto.LastLogin = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
                return StatusCode(201, userDto);
            }

            return StatusCode(500);
        }
        #endregion

        #region Update User
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Update([FromBody] UserRequestDto userRequestDto, string id)
        {
            userRequestDto.SetUserID(await GetUserID());
            int total = await _userService.UpdateUserAsync(userRequestDto, id);
            if (total > 0)
            {
                _logger.LogInformation("Update Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Delete User
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> Delete(string id)
        {
            UserDto? userDto = await _userService.GetUserAsync(id, false);
            if (userDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }

            int total = await _userService.DeleteUserAsync(id);
            if (total > 0)
            {
                _logger.LogInformation("Delete Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Delete Many Users
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> DeleteMany([FromBody] ICollection<string> ids)
        {
            var usersToDelete = await _userManager.Users.Where(user => ids.Contains(user.Id)).ToListAsync();

            if (usersToDelete == null || !usersToDelete.Any())
            {
                _logger.LogInformation("No users found for the provided IDs.");
                return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "No users found for the provided IDs." });
            }
            else if (usersToDelete.Count != ids.Count)
            {

                _logger.LogInformation("The user requested to be deleted does not exist.");
                return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "The user requested to be deleted does not exist." });
            }

            int total = await _userService.DeleteManyUsersAsync(ids);
            if (total > 0)
            {
                _logger.LogInformation("Delete Success");

                return Ok(total);
            }

            return StatusCode(500);
        }
        #endregion

        #region Get User
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto?>> Get(string id, bool? isDeep)
        {
            UserDto? userDto = await _userService.GetUserAsync(id, isDeep ?? false);
            if (userDto == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }
            userDto.LastLogin = userDto.GetLastLoginInVietnamTime(userDto.LastLogin);
            return Ok(userDto);
        }
        #endregion

        #region Get List Users
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedDto<UserDto>>> GetList([FromQuery] UserFilterDto filterDto)
        {
            return Ok(await _userService.GetListUsersAsync(filterDto));
        }
        #endregion

        #region Change User's Password
        [HttpPost("ChangePassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var result = await _updateUserService.ChangePasswordAsync(changePasswordDto);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password changed successfully.");
                return Ok("Password changed successfully.");
            }
            return StatusCode(400, result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
        }
        #endregion

        #region Send email to change email
        [HttpPost("SendChangeEmail")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SendEmailAsync([FromBody] ChangeEmailDto changeEmailDto)
        {
            var result = await _updateUserService.SendEmailAsync(changeEmailDto);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email sent successfully.");
                return Ok(changeEmailDto.NewEmail);
            }
            return StatusCode(400, result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
        }
        #endregion

        #region Change User's Email
        [HttpPost("ConfirmChangeEmail")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ChangeEmailAsync([FromBody] ConfirmChangeEmailDto confirmEmailRequest)
        {
            var result = await _updateUserService.ChangeEmailAsync(confirmEmailRequest);
            if (!result.Succeeded)
            {
                return StatusCode(400, result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
            }
            return Ok("Email changed successfully.");
        }
        #endregion

        #region Send email to change phone number
        //[HttpPost("ChangePhoneNumber")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult> SendEmailChangePhoneAsync([FromBody] ChangePhoneDto changePhoneDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    bool result = await _updateUserService.SendEmailChangePhoneNumberAsync(changePhoneDto);
        //    if (result)
        //    {
        //        _logger.LogInformation("Send Email Success");
        //        return Ok(result);
        //    }
        //    return StatusCode(500);
        //}
        //#endregion

        //#region Change User's Phone
        //[HttpPost("ConfirmChangePhone")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult> ChangePhoneNumberAsync([FromBody] ConfirmEmailChangePhoneNumberDto confirmEmailRequest)
        //{
        //    var result = await _updateUserService.ConfirmEmailChangePhoneNumberAsync(confirmEmailRequest);
        //    if (result == null)
        //    {
        //        return StatusCode(500);
        //    }
        //    return Ok(result);
        //}
        #endregion

        #region Change User's Avatar
        [HttpPost("ChangeAvatar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ChangeAvatarAsync(IFormFile avatarFile)
        {
            var principal = HttpContext.User;
            var emailClaim = principal.FindFirst(ClaimTypes.Email);
            var email = emailClaim?.Value;

            var user = await _userManager.FindByEmailAsync(email);

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }

            var result = await _updateUserService.ChangeAvatarAsync(user, avatarFile);
            if (!result.Succeeded)
            {
                return StatusCode(400, result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
            }
            return Ok(user.AvatarUrl);
        }
        #endregion

        #region Update User's Info
        [HttpPut("UpdateUser/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserInfoDto updateUser, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }

            var result = await _updateUserService.UpdateUser(updateUser, user.Id);
            if (!result.Succeeded)
            {
                return StatusCode(400, result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
            }
            return Ok(user);
        }
        #endregion

        #region Send Email (Identity)
        [HttpPost("SendEmailIdentity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendEmailToBlazor([FromBody] SendEmailRequestDto sendEmailRequestDto)
        {
            if (sendEmailRequestDto == null)
            {
                return StatusCode(400, new ErrorResponseDto { Code = ResponseCode.UserSendEmail, Description = "Request is null." });
            }

            var result = await _updateUserService.SendEmailBlazor(sendEmailRequestDto);
            if (result.Succeeded)
            {
                return Ok("Email sent successfully.");
            }
            else
            {
                return StatusCode(400, result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
            }
        }
        #endregion

        #region Forgot Password
        [HttpPost("ForgotPassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (forgotPasswordDto == null)
            {
                return StatusCode(400, new ErrorResponseDto { Code = ResponseCode.UserSendEmail, Description = "Request is null." });
            }

            var result = await _updateUserService.ForgotPasswordAsync(forgotPasswordDto);
            if (result.Succeeded)
            {
                return Ok("Email sent successfully.");
            }

            return StatusCode(400, result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
        }
        #endregion

        #region Reset Password
        [HttpPost("ResetPassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var result = await _updateUserService.ResetPasswordAsync(resetPasswordDto);
            if (result.Succeeded)
            {
                return Ok("Reset password successfully.");
            }
            return StatusCode(400, result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
        }
        #endregion

        #region Get Permissions By User ID
        [HttpGet("GetAllPermissions/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponseDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HashSet<string>?>> GetAllPermissions(string userId)
        {
            try
            {
                // Check if user exists
                var userExists = await _userService.GetUserAsync(userId);
                if (userExists==null)
                {
                    return NotFound(new ErrorResponseDto { Description = "User not found", Code = "UserNotFound" });
                }

                // Get permissions
                var permissions = await _userService.GetPermissionsByUserIdAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        #endregion
        // =========================================
        // RBAC - ASSIGN ROLE
        // =========================================

        #region Assign Role
        [HttpPost("assign-role")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new ErrorResponseDto { Code = "UserNotFound", Description = "User not found" });

            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
                return NotFound(new ErrorResponseDto { Code = "RoleNotFound", Description = $"Role {request.RoleName} not found" });

            var alreadyInRole = await _userManager.IsInRoleAsync(user, request.RoleName);
            if (alreadyInRole)
                return BadRequest(new ErrorResponseDto { Code = "AlreadyInRole", Description = $"User already in role {request.RoleName}" });

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {request.RoleName} assigned to {request.Email}");
                return Ok(new { Message = $"Role {request.RoleName} assigned to {request.Email}" });
            }

            return BadRequest(result.Errors.Select(x => new ErrorResponseDto { Code = x.Code, Description = x.Description }).First());
        }
        #endregion

        #region Remove Role
        [HttpPost("remove-role")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new ErrorResponseDto { Code = "UserNotFound", Description = "User not found" });

            var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {request.RoleName} removed from {request.Email}");
                return Ok(new { Message = $"Role {request.RoleName} removed from {request.Email}" });
            }

            return BadRequest(result.Errors.Select(x => new ErrorResponseDto { Code = x.Code, Description = x.Description }).First());
        }
        #endregion

        #region Get User Roles
        [HttpGet("{userId}/roles")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ErrorResponseDto { Code = "UserNotFound", Description = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }
        #endregion

    }
}