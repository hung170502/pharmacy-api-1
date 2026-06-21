using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Services.Account;
using Pharmacy_API.Supports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Controllers
{
    [Route("api/Account/[controller]")]
    [ApiController]
    public class AuthController : ApiControllerBase
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IJwtAuthManagerService _jwtAuthManager;
        private readonly AppSettings _appSettings;
        private readonly IAuthManagerService _authManagerService;
        private readonly IDistributedCache _distributedCache;
        private readonly IEmailSenderService _emailSender;
        private readonly IPhoneVerificationService _phoneVerification;
        private readonly IPhoneOtpService _phoneOtpService;
        #endregion

        #region Constructors
        public AuthController(
              IOptions<AppSettings> appSettings,
              ILogger<AuthController> logger,
              SignInManager<ApplicationUser> signInManager,
              UserManager<ApplicationUser> userManager,
              RoleManager<Role> roleManager,
              IJwtAuthManagerService jwtAuthManager,
              IDistributedCache distributedCache,
              IAuthManagerService authManagerService,
              IEmailSenderService emailSender,
              IPhoneVerificationService phoneVerification,
              IPhoneOtpService phoneOtpService)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtAuthManager = jwtAuthManager;
            _authManagerService = authManagerService;
            _distributedCache = distributedCache;
            _emailSender = emailSender;
            _phoneVerification = phoneVerification;
            _phoneOtpService = phoneOtpService;
        }
        #endregion

        #region Functions

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = ResponseCode.UserNotFound,
                    Description = "User not found"
                });
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = "EmailNotConfirmed",
                    Description = "Email chưa được xác nhận"
                });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = ResponseCode.AuthInvalid,
                    Description = "Invalid credentials"
                });
            }

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            user.LastLogin = DateTime.UtcNow;
            user.IsOnline = true;

            await _userManager.UpdateAsync(user);

            var userClaims = await _jwtAuthManager.GetUserClaims(user);

            var jwtResult = await _jwtAuthManager.GenerateTokens(
                user,
                userClaims,
                DateTime.Now
            );

            await _distributedCache.RemoveAsync(user.Email ?? string.Empty);

            await _distributedCache.SetStringAsync(
                user.Email ?? string.Empty,
                jwtResult.AccessToken,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(
                        _appSettings.Jwt.AccessTokenExpiryInMinutes
                    )
                });

            await _userManager.SetAuthenticationTokenAsync(
                user,
                _appSettings.Jwt.AppName,
                _appSettings.Jwt.RefreshTokenName,
                jwtResult.RefreshToken
            );

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new LoginResponseDto
            {
                Data = new UserDataDto
                {
                    Email = user.Email,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken,
                    Name = user.UserName ?? string.Empty,
                    Phone = user.PhoneNumber ?? string.Empty,
                    UserId = user.Id,
                    Roles = roles.ToList(),
                    IsOnline = user.IsOnline
                }
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // ===== Xác minh SĐT nếu có =====
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                // Validate format SĐT
                var validation = _phoneVerification.Validate(request.PhoneNumber);
                if (!validation.IsValid)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Code = "InvalidPhone",
                        Description = validation.Message ?? "Số điện thoại không hợp lệ"
                    });
                }

                // Nếu có OTP thì verify
                if (!string.IsNullOrEmpty(request.PhoneOtp))
                {
                    var otpResult = await _phoneOtpService.VerifyOtpAsync(
                        validation.FormattedNumber!,
                        request.PhoneOtp);

                    if (!otpResult.Success)
                    {
                        return BadRequest(new ErrorResponseDto
                        {
                            Code = "InvalidOtp",
                            Description = otpResult.Message
                        });
                    }
                }
                else
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Code = "MissingOtp",
                        Description = "Vui lòng xác minh số điện thoại bằng OTP"
                    });
                }

                // Gán SĐT đã chuẩn hóa
                request.PhoneNumber = validation.FormattedNumber;
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PhoneNumberConfirmed = !string.IsNullOrEmpty(request.PhoneNumber)
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                try
                {
                    if (!await _roleManager.RoleExistsAsync("Customer"))
                    {
                        await _roleManager.CreateAsync(new Role
                        {
                            Name = "Customer",
                            DisplayName = "Khách hàng",
                            NormalizedName = "CUSTOMER"
                        });
                        _logger.LogInformation("Customer role created");
                    }

                    await _userManager.AddToRoleAsync(user, "Customer");
                    _logger.LogInformation($"Customer role assigned to {user.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to assign Customer role: {ex.Message}");
                }

                var claims = await _jwtAuthManager.GetUserClaims(user);
                var jwtResult = await _jwtAuthManager.GenerateTokens(user, claims, DateTime.UtcNow);

                await _userManager.SetAuthenticationTokenAsync(
                    user,
                    _appSettings.Jwt.AppName,
                    _appSettings.Jwt.RefreshTokenName,
                    jwtResult.RefreshToken);

                var roles = await _userManager.GetRolesAsync(user);

                // Gửi OTP Email xác nhận
                await _authManagerService.SendOtpAsync(user.Email);

                var loggedInUser = new
                {
                    Email = request.Email,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken,
                    UserId = user.Id,
                    Roles = roles.ToList(),
                    PhoneNumber = user.PhoneNumber,
                    PhoneConfirmed = user.PhoneNumberConfirmed
                };

                return Ok(loggedInUser);
            }

            return StatusCode(StatusCodes.Status400BadRequest,
                result.Errors.Select(x => new ErrorResponseDto { Code = x.Code, Description = x.Description })
                .First());
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ConfirmEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "Error finding user for unspecified email" });
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            return Ok(result);
        }

        [HttpPost]
        [Route("Refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user is null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new ErrorResponseDto { Code = ResponseCode.AuthInvalid, Description = "Invalid credentials" });
            }

            var isValid = await _userManager.VerifyUserTokenAsync(
                user, _appSettings.Jwt.AppName, _appSettings.Jwt.RefreshTokenName, request.RefreshToken);

            if (!isValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new ErrorResponseDto { Code = ResponseCode.AuthInvalid, Description = "Invalid credentials" });
            }

            await _distributedCache.RemoveAsync(user.Email ?? string.Empty);
            var userClaims = await _jwtAuthManager.GetUserClaims(user);
            var jwtResult = await _jwtAuthManager.GenerateTokens(user, userClaims, DateTime.Now);
            await _distributedCache.SetStringAsync(user.Email ?? string.Empty, jwtResult.AccessToken,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(_appSettings.Jwt.AccessTokenExpiryInMinutes),
                });

            await _userManager.SetAuthenticationTokenAsync(
                   user,
                   _appSettings.Jwt.AppName,
                   _appSettings.Jwt.RefreshTokenName,
                   jwtResult.RefreshToken);
            return Ok(jwtResult);
        }

        [HttpPost]
        [Authorize]
        [Route("Logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = ResponseCode.AuthInvalid,
                    Description = "Invalid credentials"
                });
            }

            await _distributedCache.RemoveAsync(email);

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                user.IsOnline = false;
                await _userManager.UpdateAsync(user);
            }

            return Ok(new
            {
                success = true,
                message = "Logout successful"
            });
        }

        #region Login Google
        [HttpGet("Login-google")]
        [AllowAnonymous]
        public IActionResult GetGoogleLoginUrl([FromQuery] string? platform = "web", [FromQuery] string? redirectUrl = null)
        {
            string clientId = _appSettings.Google.ClientId;
            string redirectUri = _appSettings.Google.RedirectUrl;
            string scope = "openid email profile";

            string state;
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                state = Uri.EscapeDataString(redirectUrl);
            }
            else if (platform == "mobile")
            {
                state = Uri.EscapeDataString("antamvietpharmacy://auth/callback");
            }
            else
            {
                state = Uri.EscapeDataString(_appSettings.FrontendUrl + "/auth/google/callback");
            }

            string googleUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                $"?response_type=code" +
                $"&client_id={clientId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&scope={scope}" +
                $"&state={state}";

            return Ok(new { url = googleUrl });
        }

        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(
     [FromQuery] string code,
     [FromQuery] string? state = null)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Authorization code not provided");

            var tokenResponse = await _authManagerService.ExchangeCodeForTokenAsync(code);
            if (tokenResponse == null)
                return BadRequest("Failed to exchange code for token");

            var userProfile = await _authManagerService.GetGoogleUserProfileAsync(tokenResponse.IdToken);
            if (userProfile == null)
                return BadRequest("Failed to retrieve user profile");

            var user = await _userManager.FindByEmailAsync(userProfile.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userProfile.Email,
                    Email = userProfile.Email,
                    FirstName = userProfile.Name,
                    LastName = userProfile.FamilyName,
                    EmailConfirmed = true,
                    Address = userProfile.Issuer
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        result.Errors.Select(x => new ErrorResponseDto { Code = x.Code, Description = x.Description }).First());
                }

                try
                {
                    if (!await _roleManager.RoleExistsAsync("Customer"))
                    {
                        await _roleManager.CreateAsync(new Role
                        {
                            Name = "Customer",
                            DisplayName = "Khách hàng",
                            NormalizedName = "CUSTOMER"
                        });
                    }
                    await _userManager.AddToRoleAsync(user, "Customer");
                    _logger.LogInformation($"Customer role assigned to Google user {user.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to assign Customer role in Google login: {ex.Message}");
                }
            }

            await _distributedCache.RemoveAsync(user.Email ?? string.Empty);
            var claims = await _jwtAuthManager.GetUserClaims(user);
            var jwtResult = await _jwtAuthManager.GenerateTokens(user, claims, DateTime.UtcNow);
            await _distributedCache.SetStringAsync(
                user.Email ?? string.Empty,
                jwtResult.AccessToken,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(_appSettings.Jwt.AccessTokenExpiryInMinutes),
                });

            await _userManager.SetAuthenticationTokenAsync(
                user, _appSettings.Jwt.AppName, _appSettings.Jwt.RefreshTokenName, jwtResult.RefreshToken);

            var roles = await _userManager.GetRolesAsync(user);

            var decodedState = !string.IsNullOrEmpty(state) ? Uri.UnescapeDataString(state) : "";

            if (decodedState.StartsWith("antamvietpharmacy://"))
            {
                var deepLinkUrl = $"{decodedState}" +
                    $"?accessToken={Uri.EscapeDataString(jwtResult.AccessToken)}" +
                    $"&refreshToken={Uri.EscapeDataString(jwtResult.RefreshToken)}" +
                    $"&email={Uri.EscapeDataString(user.Email ?? "")}" +
                    $"&userId={Uri.EscapeDataString(user.Id.ToString())}" +
                    $"&name={Uri.EscapeDataString(user.UserName ?? "")}" +
                    $"&roles={Uri.EscapeDataString(string.Join(",", roles))}";

                return Redirect(deepLinkUrl);
            }

            if (decodedState.StartsWith("http"))
            {
                var webRedirectUrl = $"{decodedState}" +
                    $"?accessToken={Uri.EscapeDataString(jwtResult.AccessToken)}" +
                    $"&refreshToken={Uri.EscapeDataString(jwtResult.RefreshToken)}" +
                    $"&email={Uri.EscapeDataString(user.Email ?? "")}" +
                    $"&userId={Uri.EscapeDataString(user.Id.ToString())}" +
                    $"&name={Uri.EscapeDataString(user.UserName ?? "")}" +
                    $"&roles={Uri.EscapeDataString(string.Join(",", roles))}";

                return Redirect(webRedirectUrl);
            }

            return Ok(new LoginResponseDto
            {
                Data = new UserDataDto
                {
                    Email = user.Email,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken,
                    Name = user.UserName ?? string.Empty,
                    Phone = user.PhoneNumber ?? string.Empty,
                    UserId = user.Id,
                    Roles = roles.ToList()
                }
            });
        }
        #endregion

        #region OTP

        // ===== Email OTP (cũ) =====
        [HttpPost("Send-Otp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            var result = await _authManagerService.SendOtpAsync(email);
            if (result)
                return Ok(new { success = true, message = "OTP sent to your email" });

            return BadRequest(new { success = false, message = "Failed to send OTP" });
        }

        [HttpPost("Verify-Otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _authManagerService.VerifyOtpAsync(request.Email, request.Code);
            if (result)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
                return Ok(new { success = true, message = "OTP verified successfully", emailConfirmed = true });
            }

            return BadRequest(new { success = false, message = "Invalid or expired OTP" });
        }

        // ===== Phone OTP (MỚI) =====

        /// <summary>
        /// Gửi mã OTP đến số điện thoại
        /// </summary>
        [HttpPost("Send-Phone-Otp")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendPhoneOtp([FromBody] SendPhoneOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                return BadRequest(new { success = false, message = "Số điện thoại không được để trống" });
            }

            // Validate SĐT
            var validation = _phoneVerification.Validate(request.PhoneNumber);
            if (!validation.IsValid)
            {
                return BadRequest(new { success = false, message = validation.Message });
            }

            // Gửi OTP
            var result = await _phoneOtpService.GenerateOtpAsync(validation.FormattedNumber!);

            if (result.Success)
            {
                _logger.LogInformation($"Phone OTP sent to {validation.FormattedNumber}");
                return Ok(new
                {
                    success = true,
                    message = "Mã OTP đã được gửi đến số điện thoại của bạn",
                    expiresInMinutes = result.ExpiresInMinutes,
                    carrier = validation.Carrier,
                    otp = result.Otp // ⚠️ XÓA DÒNG NÀY TRONG PRODUCTION
                });
            }

            return BadRequest(new { success = false, message = "Không thể gửi OTP. Vui lòng thử lại sau." });
        }

        /// <summary>
        /// Xác minh mã OTP từ số điện thoại
        /// </summary>
        [HttpPost("Verify-Phone-Otp")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyPhoneOtp([FromBody] VerifyPhoneOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.OtpCode))
            {
                return BadRequest(new { success = false, message = "Số điện thoại và mã OTP không được để trống" });
            }

            // Validate SĐT
            var validation = _phoneVerification.Validate(request.PhoneNumber);
            if (!validation.IsValid)
            {
                return BadRequest(new { success = false, message = validation.Message });
            }

            // Verify OTP
            var result = await _phoneOtpService.VerifyOtpAsync(validation.FormattedNumber!, request.OtpCode);

            if (result.Success)
            {
                _logger.LogInformation($"Phone verified: {validation.FormattedNumber}");
                return Ok(new
                {
                    success = true,
                    message = "Xác thực số điện thoại thành công!",
                    phoneNumber = validation.FormattedNumber,
                    carrier = validation.Carrier
                });
            }

            return BadRequest(new { success = false, message = result.Message });
        }

        // DTOs cho Phone OTP
        public class SendPhoneOtpRequest
        {
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public class VerifyPhoneOtpRequest
        {
            public string PhoneNumber { get; set; } = string.Empty;
            public string OtpCode { get; set; } = string.Empty;
        }

        public class VerifyOtpRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }
        #endregion

        #endregion
    }
}