using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Services.Account
{
    public interface IUpdateUserService
    {
        public Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        public Task<IdentityResult> SendEmailAsync(ChangeEmailDto changeEmailDto);
        public Task<IdentityResult> ChangeEmailAsync(ConfirmChangeEmailDto confirmEmailRequest);
        //public Task<bool> SendEmailChangePhoneNumberAsync(ChangePhoneDto changePhoneDto);
        //      public Task<string> ConfirmEmailChangePhoneNumberAsync(ConfirmEmailChangePhoneNumberDto confirmEmailRequest);
        public Task<(string, IdentityResult)> UploadImageAsync(IFormFile avatarFile, string userId);
        public Task<IdentityResult> ChangeAvatarAsync(ApplicationUser user, IFormFile avatarFile);
        public Task<IdentityResult> UpdateUser(UpdateUserInfoDto updateUser, string userId);
        public Task<(IList<string> Permissions, IdentityResult Result)> GetUserPermissionsAsync(string userId);
        public Task<IdentityResult> SendEmailBlazor(SendEmailRequestDto sendEmailRequestDto);
        public Task<IdentityResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        public Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
