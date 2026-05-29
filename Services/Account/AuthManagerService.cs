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
            var code = new Random().Next(1000, 9999).ToString(); // 4 số

            var otp = new UserOtp
            {
                Email = email,
                Code = code,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _context.UserOtps.Add(otp);
            await _context.SaveChangesAsync();

            string emailContent = $@"
               <!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Xác nhận mã OTP - Divine Shop</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f9fafb;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f9fafb; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <!-- Main Container -->
                <table width=""1000"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); overflow: hidden;"">
                    
                    <!-- Header -->
                    <tr>
                        <td align=""center"" style=""background-color: #ffffff; padding: 40px 30px; border-bottom: 1px solid #e5e7eb;"">
                            <table cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"">
                                        <div style=""width: 48px; height: 48px; background-color: #2563eb; border-radius: 50%; display: inline-block; vertical-align: middle; text-align: center; line-height: 48px;"">
                                            <span style=""color: #ffffff; font-size: 24px; font-weight: bold;"">D</span>
                                        </div>
                                    </td>
                                    <td style=""padding-left: 12px;"">
                                        <h1 style=""margin: 0; font-size: 28px; font-weight: bold; color: #2563eb;"">Nhà thuốc An Tâm Việt</h1>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <!-- Title -->
                            <h2 style=""margin: 0 0 30px 0; font-size: 24px; font-weight: bold; color: #111827; text-align: center;"">
                                Xác nhận mã OTP
                            </h2>

                            <!-- Greeting -->
                            <p style=""margin: 0 0 15px 0; font-size: 16px; color: #374151; line-height: 1.5;"">
                                Chào <span style=""text-transform: capitalize;"">{email}</span>,
                            </p>

                            <!-- Message -->
                            <p style=""margin: 0 0 30px 0; font-size: 16px; color: #6b7280; line-height: 1.5;"">
                                Bạn vừa nhận được mã OTP xác nhận tại Nhà thuốc An Tâm Việt.
                            </p>

                            <!-- OTP Code -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"" style=""padding: 0 0 30px 0;"">
                                        <div style=""display: inline-block; border: 2px solid #2563eb; border-radius: 6px; padding: 16px 32px; background-color: #eff6ff;"">
                                            <span style=""font-size: 36px; font-weight: bold; color: #2563eb; letter-spacing: 4px;"">
                                               {code} 
                                            </span>
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- Footer Message -->
                            <p style=""margin: 0 0 25px 0; font-size: 14px; color: #6b7280; line-height: 1.6;"">
                                Nếu bạn không thực hiện yêu cầu này xin vui lòng bỏ qua nó hoặc nếu cần hỗ trợ hãy liên hệ với chúng tôi ngay.
                            </p>

                            <!-- Signature -->
                            <p style=""margin: 0; font-size: 16px; color: #374151; line-height: 1.5;"">
                                Trân trọng,<br>
                                <strong>Divine Corp</strong>
                            </p>
                        </td>
                    </tr>

                  

                </table>
            </td>
        </tr>
    </table>
</body>
</html>
                ";
            await _emailSender.SendEmailAsync(email, "OTP Verification", emailContent);

            _logger.LogInformation($"OTP sent to {email}");

            return true;
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

        //#region Function
        ///// <summary>
        ///// Sends an email to the user requesting their email to be activated.
        ///// </summary>
        ///// <param name="user">The user to send an activation email to.</param>
        ///// <returns>A Task representing the asynchronous operation.</returns>
        //public async Task<bool> RequestEmailActivation(ApplicationUser user)
        //{
        //    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        //    string link = $"{_appSettings.DeepLinksSettings.BaseUrl + _appSettings.DeepLinksSettings.VerifyRegisterUser}?email={user.Email}&activationToken={code.ToBase64()}";

        //    string emailContent = $"Thanks for subscribing to {_appSettings.Jwt.AppName}!" +
        //        $"<br/><br/>" +
        //        $"To activate your email, please click on one of the below links: " +
        //        $"<br/><br/>" +
        //        $"<a href=\"{link}\">Activation Link</a>" +
        //        $"<br/><br/>" +
        //        $"<a href=\"{link}\">{link}</a>" +
        //        $"<br/><br/>" +
        //        $"{_appSettings.Jwt.AppName} Team";

        //    await _emailSender.SendEmailAsync(user.Email ?? string.Empty, "Email Activation", emailContent);

        //    _logger.LogInformation($"An activation email was sent to {user.Email}");

        //    return true;

        //}
        //#endregion

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

        public Task RequestEmailActivation(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
