using AutoMapper;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pharmacy_API.Context;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Supports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Services.Account
{
    public class AuthManagerService : IAuthManagerService
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSenderService _emailSender;
        private readonly AccountContext _context; // ✅ DbContext để lưu OTP

        #endregion

        public AuthManagerService(IOptions<AppSettings> appSettings,
            UserManager<ApplicationUser> userManager,
            ILogger<UserService> logger,
            IEmailSenderService emailSender,
            AccountContext context)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;  

        }
        #region OTP Functions
        /// <summary>
        /// Gửi OTP qua email
        /// </summary>
        public async Task<bool> SendOtpAsync(string email)
        {
            try
            {
                _logger.LogInformation($"🎯 Starting OTP generation for {email}");

                var code = new Random().Next(1000, 9999).ToString();

                var otp = new UserOtp
                {
                    Email = email,
                    Code = code,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _context.UserOtps.AddAsync(otp);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"💾 OTP {code} saved to database for {email}");

                _logger.LogInformation($"📤 Attempting to send OTP email to {email}");

                string emailContent = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Xác nhận mã OTP - Nhà thuốc An Tâm Việt</title>
    <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap"" rel=""stylesheet"">
</head>
<body style=""margin: 0; padding: 0; font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background-color: #f0f4f8;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f0f4f8; padding: 30px 15px;"">
        <tr>
            <td align=""center"">
                
                <!-- Main Container -->
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 24px rgba(0,0,0,0.08); overflow: hidden; max-width: 600px;"">
                    
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #0ea5e9 0%, #0284c7 100%); padding: 35px 40px; text-align: center;"">
                            <table cellpadding=""0"" cellspacing=""0"" style=""margin: 0 auto;"">
                                <tr>
                                    <td style=""text-align: center;"">
                                        <div style=""width: 56px; height: 56px; background-color: rgba(255,255,255,0.2); border-radius: 14px; display: inline-block; text-align: center; line-height: 56px; margin-bottom: 12px;"">
                                            <span style=""font-size: 28px;"">💊</span>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""text-align: center;"">
                                        <h1 style=""margin: 0; font-size: 26px; font-weight: 700; color: #ffffff; letter-spacing: -0.5px;"">Nhà thuốc An Tâm Việt</h1>
                                        <p style=""margin: 6px 0 0 0; font-size: 13px; color: rgba(255,255,255,0.85); font-weight: 400;"">Chăm sóc sức khỏe của bạn</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 40px 30px 40px;"">
                            
                            <!-- Icon -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"" style=""padding-bottom: 24px;"">
                                        <div style=""width: 64px; height: 64px; background-color: #f0f9ff; border-radius: 50%; display: inline-block; text-align: center; line-height: 64px;"">
                                            <span style=""font-size: 32px;"">🔐</span>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- Title -->
                            <h2 style=""margin: 0 0 10px 0; font-size: 22px; font-weight: 700; color: #0f172a; text-align: center;"">
                                Xác nhận mã OTP
                            </h2>
                            
                            <!-- Subtitle -->
                            <p style=""margin: 0 0 28px 0; font-size: 14px; color: #64748b; text-align: center; line-height: 1.6;"">
                                Vui lòng sử dụng mã bên dưới để hoàn tất xác thực
                            </p>

                            <!-- OTP Code Box -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 28px;"">
                                <tr>
                                    <td align=""center"">
                                        <div style=""display: inline-block; background-color: #f8fafc; border: 2px dashed #0ea5e9; border-radius: 12px; padding: 20px 40px;"">
                                            <p style=""margin: 0 0 6px 0; font-size: 11px; color: #94a3b8; text-transform: uppercase; letter-spacing: 2px; font-weight: 600;"">Mã xác thực của bạn</p>
                                            <span style=""font-size: 42px; font-weight: 700; color: #0284c7; letter-spacing: 8px; line-height: 1;"">{code}</span>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- Timer Info -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 24px;"">
                                <tr>
                                    <td align=""center"">
                                        <div style=""display: inline-flex; align-items: center; background-color: #fef3c7; border-radius: 8px; padding: 10px 18px;"">
                                            <span style=""font-size: 16px; margin-right: 8px;"">⏰</span>
                                            <span style=""font-size: 13px; color: #92400e; font-weight: 500;"">Mã có hiệu lực trong <strong>5 phút</strong></span>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- Warning -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 30px;"">
                                <tr>
                                    <td style=""background-color: #fef2f2; border-left: 3px solid #ef4444; border-radius: 6px; padding: 14px 18px;"">
                                        <table cellpadding=""0"" cellspacing=""0"">
                                            <tr>
                                                <td style=""vertical-align: top; padding-right: 12px;"">
                                                    <span style=""font-size: 16px;"">⚠️</span>
                                                </td>
                                                <td>
                                                    <p style=""margin: 0; font-size: 13px; color: #991b1b; line-height: 1.5;"">
                                                        <strong>Quan trọng:</strong> Không chia sẻ mã này với bất kỳ ai. Nhân viên nhà thuốc sẽ không bao giờ yêu cầu bạn cung cấp mã OTP.
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Divider -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 20px;"">
                                <tr>
                                    <td style=""border-top: 1px solid #e2e8f0;""></td>
                                </tr>
                            </table>

                            <!-- Help Text -->
                            <p style=""margin: 0 0 6px 0; font-size: 13px; color: #64748b; line-height: 1.6; text-align: center;"">
                                Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.
                            </p>
                            <p style=""margin: 0; font-size: 13px; color: #64748b; line-height: 1.6; text-align: center;"">
                                Cần hỗ trợ? Liên hệ: <a href=""mailto:support@antamviet.com"" style=""color: #0284c7; text-decoration: none; font-weight: 500;"">support@antamviet.com</a>
                            </p>

                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8fafc; padding: 24px 40px; border-top: 1px solid #e2e8f0; text-align: center;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"" style=""padding-bottom: 12px;"">
                                        <div style=""display: inline-flex; gap: 16px;"">
                                            <span style=""font-size: 12px; color: #94a3b8;"">📞 1900 1234</span>
                                            <span style=""font-size: 12px; color: #cbd5e1;"">|</span>
                                            <span style=""font-size: 12px; color: #94a3b8;"">🌐 antamviet.com</span>
                                            <span style=""font-size: 12px; color: #cbd5e1;"">|</span>
                                            <span style=""font-size: 12px; color: #94a3b8;"">📍 123 Nguyễn Huệ, Q.1, TP.HCM</span>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td align=""center"">
                                        <p style=""margin: 0; font-size: 11px; color: #94a3b8; line-height: 1.5;"">
                                            © 2024 Nhà thuốc An Tâm Việt. Tất cả quyền được bảo lưu.<br>
                                            Email này được gửi tự động, vui lòng không trả lời.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                </table>

                <!-- Trust Badge -->
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""margin-top: 20px;"">
                    <tr>
                        <td align=""center"">
                            <p style=""margin: 0; font-size: 11px; color: #94a3b8;"">
                                🔒 Email được bảo mật bởi Nhà thuốc An Tâm Việt
                            </p>
                        </td>
                    </tr>
                </table>

            </td>
        </tr>
    </table>
</body>
</html>";

                var sendResult = await _emailSender.SendEmailAsync(email, "OTP Verification", emailContent);

                if (sendResult)
                {
                    _logger.LogInformation($"✅ OTP email sent successfully to {email}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Failed to send OTP email to {email}");
                }

                return sendResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error sending OTP to {email}: {ex.Message}");
                _logger.LogError($"   Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Xác thực OTP
        /// </summary>
        public async Task<bool> VerifyOtpAsync(string email, string code)
        {
            var otp = _context.UserOtps
                .Where(x => x.Email == email && x.Code == code && !x.IsUsed && x.ExpiredAt > DateTime.UtcNow)
                .FirstOrDefault();

            if (otp == null)
                return false;

            otp.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }
        #endregion

        #region Function
        /// <summary>
        /// Sends an email to the user requesting their email to be activated.
        /// </summary>
        /// <param name="user">The user to send an activation email to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task<bool> RequestEmailActivation(ApplicationUser user)
        {
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            string link = $"{_appSettings.DeepLinksSettings.BaseUrl + _appSettings.DeepLinksSettings.VerifyRegisterUser}?email={user.Email}&activationToken={code.ToBase64()}";

            string emailContent = $"Thanks for subscribing to {_appSettings.Jwt.AppName}!" +
                $"<br/><br/>" +
                $"To activate your email, please click on one of the below links: " +
                $"<br/><br/>" +
                $"<a href=\"{link}\">Activation Link</a>" +
                $"<br/><br/>" +
                $"<a href=\"{link}\">{link}</a>" +
                $"<br/><br/>" +
                $"{_appSettings.Jwt.AppName} Team";

            await _emailSender.SendEmailAsync(user.Email ?? string.Empty, "Email Activation", emailContent);

            _logger.LogInformation($"An activation email was sent to {user.Email}");

            return true;

        }
        #endregion

        #region Google ExchangeCodeForToken
        public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            string clientId = _appSettings.Google.ClientId;
            string clientSecret = _appSettings.Google.ClientSecret;
            string redirectUri = _appSettings.Google.RedirectUrl;

            var requestBody = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

            //using var httpClient = new HttpClient();
            //var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(requestBody));
            //if (!response.IsSuccessStatusCode) return null;

            //var tokenContent = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<GoogleTokenResponse>(tokenContent);

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                // Xử lý tokenResponse (bao gồm access_token và refresh_token)
                return new GoogleTokenResponse
                {
                    AccessToken = tokenResponse.AccessToken,
                    ExpiresIn = tokenResponse.ExpiresInSeconds,
                    TokenType = tokenResponse.TokenType,
                    IdToken = tokenResponse.IdToken
                };
            }
            else
            {
                // Xử lý lỗi
                return null;
            }
        }
        #endregion

        #region Get User Profile
        public async Task<GoogleJsonWebSignature.Payload> GetGoogleUserProfileAsync(string accessToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken);
                return payload;
            }
            catch (InvalidJwtException ex)
            {
                // Xử lý lỗi khi JWT không hợp lệ
                throw new Exception("Invalid token.", ex);
            }
        }

        #endregion

    }
}
