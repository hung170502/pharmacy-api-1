
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
        private readonly IJwtAuthManagerService _jwtAuthManager;
        private readonly AppSettings _appSettings;
        private readonly IAuthManagerService _authManagerService;
        private readonly IDistributedCache _distributedCache;
        private readonly IEmailSenderService _emailSender;
        #endregion

        #region Constructors
        public AuthController(IOptions<AppSettings> appSettings,
              ILogger<AuthController> logger,
              SignInManager<ApplicationUser> signInManager,
              UserManager<ApplicationUser> userManager,
              IJwtAuthManagerService jwtAuthManager,
              IDistributedCache distributedCache,
              IAuthManagerService authManagerService,
              IEmailSenderService emailSender) // 👈 Thêm
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtAuthManager = jwtAuthManager;
            _authManagerService = authManagerService;
            _distributedCache = distributedCache;
            _emailSender = emailSender; // 👈 Thêm
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

            // Kiểm tra email đã xác nhận chưa
            if (!user.EmailConfirmed)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = "EmailNotConfirmed",
                    Description = "Email chưa được xác nhận"
                });
            }

            // Kiểm tra password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = ResponseCode.AuthInvalid,
                    Description = "Invalid credentials"
                });
            }

            // Set LastLogin
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            user.LastLogin = DateTime.UtcNow;
            user.IsOnline = true;

            await _userManager.UpdateAsync(user);

            // JWT
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

            // Save refresh token
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
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var claims = await _jwtAuthManager.GetUserClaims(user);

                var jwtResult = await _jwtAuthManager.GenerateTokens(user, claims, DateTime.Now);

                //save in db
                await _userManager.SetAuthenticationTokenAsync(
                    user,
                    _appSettings.Jwt.AppName,
                    _appSettings.Jwt.RefreshTokenName,
                    jwtResult.RefreshToken);

                //await _authManagerService.RequestEmailActivation(user);
                await _authManagerService.SendOtpAsync(user.Email);

                var loggedInUser = new
                {
                    Email = request.Email,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken,
                    UserId = user.Id
                };

                return Ok(loggedInUser);
            }

            return StatusCode(StatusCodes.Status400BadRequest,
                result.Errors.Select(x => new ErrorResponseDto { Code = x.Code, Description = x.Description })
                .First());
        }


        #region Test & Debug

        [HttpGet("TestEmailConfig")]
        [AllowAnonymous]
        public async Task<IActionResult> TestEmailConfig([FromQuery] string testEmail)
        {
            if (string.IsNullOrEmpty(testEmail))
            {
                return BadRequest(new { Error = "Please provide testEmail parameter" });
            }

            try
            {
                _logger.LogInformation($"🧪 Starting email configuration test to {testEmail}");
                _logger.LogInformation($"📋 SMTP Settings - Host: {_appSettings.MailSettings.Host}, Port: {_appSettings.MailSettings.Port}");
                _logger.LogInformation($"📋 From: {_appSettings.MailSettings.Mail}, Display Name: {_appSettings.MailSettings.DisplayName}");

                var result = await _emailSender.SendEmailAsync(
                    testEmail,
                    "Test Email - Pharmacy API Configuration",
                    $@"
            <!DOCTYPE html>
            <html>
            <head><meta charset=""UTF-8""></head>
            <body style=""font-family: Arial, sans-serif; padding: 20px;"">
                <h1 style=""color: #2563eb;"">✅ Email Configuration Test</h1>
                <p>If you receive this email, your SMTP configuration is working correctly!</p>
                <hr>
                <h3>Configuration Details:</h3>
                <ul>
                    <li><strong>SMTP Host:</strong> {_appSettings.MailSettings.Host}</li>
                    <li><strong>SMTP Port:</strong> {_appSettings.MailSettings.Port}</li>
                    <li><strong>From Email:</strong> {_appSettings.MailSettings.Mail}</li>
                    <li><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                </ul>
            </body>
            </html>"
                );

                return Ok(new
                {
                    Success = result,
                    TestEmail = testEmail,
                    SmtpHost = _appSettings.MailSettings.Host,
                    SmtpPort = _appSettings.MailSettings.Port,
                    FromEmail = _appSettings.MailSettings.Mail,
                    Message = result ? "Email sent successfully!" : "Failed to send email. Check logs for details."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Test email failed: {ex.Message}");
                return BadRequest(new
                {
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("TestOtpGeneration")]
        [AllowAnonymous]
        public async Task<IActionResult> TestOtpGeneration([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { Error = "Please provide email parameter" });
            }

            try
            {
                var result = await _authManagerService.SendOtpAsync(email);

                return Ok(new
                {
                    Success = result,
                    Email = email,
                    Message = result ? "OTP generated and sent successfully" : "Failed to send OTP"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        #endregion


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
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto { Code = ResponseCode.UserNotFound, Description = "Error finding user for unspecified email" });
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
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto { Code = ResponseCode.AuthInvalid, Description = "Invalid credentials" });
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, _appSettings.Jwt.AppName, _appSettings.Jwt.RefreshTokenName, request.RefreshToken);

            if (!isValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto { Code = ResponseCode.AuthInvalid, Description = "Invalid credentials" });
            }

            await _distributedCache.RemoveAsync(user.Email ?? string.Empty);
            var userClaims = await _jwtAuthManager.GetUserClaims(user);
            var jwtResult = await _jwtAuthManager.GenerateTokens(user, userClaims, DateTime.Now);
            await _distributedCache.SetStringAsync(user.Email ?? string.Empty, jwtResult.AccessToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(_appSettings.Jwt.AccessTokenExpiryInMinutes),

            });

            //save in db
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

            // Xóa access token trong cache
            await _distributedCache.RemoveAsync(email);

            // Update trạng thái user
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

        #region Login google
        [HttpGet("Login-google")]
        [AllowAnonymous]
        public IActionResult GetGoogleLoginUrl([FromQuery] string? platform = "web", [FromQuery] string? redirectUrl = null)
        {
            string clientId = _appSettings.Google.ClientId;
            string redirectUri = _appSettings.Google.RedirectUrl;
            string scope = "openid email profile";

            // ✅ Tự động chọn state dựa vào platform
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
        #endregion

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
            }

            // Tạo token
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

            // ✅ Tự động detect platform từ state
            var decodedState = !string.IsNullOrEmpty(state) ? Uri.UnescapeDataString(state) : "";

            // Mobile App: state = "antamvietpharmacy://auth/callback"
            if (decodedState.StartsWith("antamvietpharmacy://"))
            {
                var deepLinkUrl = $"{decodedState}" +
                    $"?accessToken={Uri.EscapeDataString(jwtResult.AccessToken)}" +
                    $"&refreshToken={Uri.EscapeDataString(jwtResult.RefreshToken)}" +
                    $"&email={Uri.EscapeDataString(user.Email ?? "")}" +
                    $"&userId={Uri.EscapeDataString(user.Id.ToString())}" +
                    $"&name={Uri.EscapeDataString(user.UserName ?? "")}";

                return Redirect(deepLinkUrl);
            }

            // Web: state = "http://localhost:3000/auth/google/callback"
            if (decodedState.StartsWith("http"))
            {
                var webRedirectUrl = $"{decodedState}" +
                    $"?accessToken={Uri.EscapeDataString(jwtResult.AccessToken)}" +
                    $"&refreshToken={Uri.EscapeDataString(jwtResult.RefreshToken)}" +
                    $"&email={Uri.EscapeDataString(user.Email ?? "")}" +
                    $"&userId={Uri.EscapeDataString(user.Id.ToString())}" +
                    $"&name={Uri.EscapeDataString(user.UserName ?? "")}";

                return Redirect(webRedirectUrl);
            }

            // Fallback: Trả JSON
            return Ok(new LoginResponseDto
            {
                Data = new UserDataDto
                {
                    Email = user.Email,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken,
                    Name = user.UserName ?? string.Empty,
                    Phone = user.PhoneNumber ?? string.Empty,
                    UserId = user.Id
                }
            });
        }
        #endregion


        #region OTP
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
                    user.EmailConfirmed = true; // 🔹 confirm email khi OTP đúng
                    await _userManager.UpdateAsync(user);
                }
                return Ok(new { success = true, message = "OTP verified successfully", emailConfirmed = true });
            }

            return BadRequest(new { success = false, message = "Invalid or expired OTP" });
        }

        public class VerifyOtpRequest
        {
            public string Email { get; set; }
            public string Code { get; set; }
        }
        #endregion



    }
}
