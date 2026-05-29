using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Pharmacy_API.Models.Account;

namespace Pharmacy_API.Services.Account
{
    public interface IAuthManagerService
    {
        Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code);
        Task<GoogleJsonWebSignature.Payload> GetGoogleUserProfileAsync(string accessToken);
        Task<bool> RequestEmailActivation(ApplicationUser user);

        Task<bool> SendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string code);
    }
}
