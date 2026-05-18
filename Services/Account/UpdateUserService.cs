using AutoMapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Repositories.Account;
using Pharmacy_API.Supports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Pharmacy_API.Services.Account
{
    public class UpdateUserService : IUpdateUserService
    {
        #region Fields
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSenderService _emailSender;
        private readonly AppSettings _configuration;
        private readonly ILogger<UpdateUserService> _logger;
        private readonly IUserRepository _userRepository;
        //private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private static Dictionary<string, string> _verificationCodes = new Dictionary<string, string>();
        #endregion

        #region Constructors
        public UpdateUserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSenderService emailSender,
            IOptions<AppSettings> configuration,
            ILogger<UpdateUserService> logger,
            IUserRepository userRepository,
            IWebHostEnvironment hostingEnvironment
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration.Value;
            _logger = logger;
            _userRepository = userRepository;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region Change User's Password
        public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            _logger.LogInformation("Change password.");
            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return IdentityResult.Failed(changePasswordResult.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
            }
            return IdentityResult.Success;
        }
        #endregion

        #region Get User's Permissions
        public async Task<(IList<string> Permissions, IdentityResult Result)> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (null, IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserNotFound, Description = "User not found" }));
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

            return (permissions, IdentityResult.Success);
        }
        #endregion

        #region Send Email
        public async Task<IdentityResult> SendEmailAsync(ChangeEmailDto changeEmailDto)
        {
            var user = await _userManager.FindByEmailAsync(changeEmailDto.Email);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }

            if (changeEmailDto.NewEmail == user.Email)
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserEmailNotMatch, Description = "New email is the same as the old email." });
            }

            if (_userManager.Users.Where(u => u.Email == changeEmailDto.NewEmail && u.Id != user.Id).FirstOrDefault() != null)
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserExisted, Description = $"{changeEmailDto.NewEmail} already exist." });
            };

            var code = await _userManager.GenerateChangeEmailTokenAsync(user, changeEmailDto.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{_configuration.DeepLinksSettings.BaseUrl + _configuration.DeepLinksSettings.ChangeEmail}?email={changeEmailDto.Email}&newEmail={changeEmailDto.NewEmail}&code={code}";

            await _emailSender.SendEmailAsync(changeEmailDto.NewEmail, "Confirm to change your email", HtmlEncoder.Default.Encode(callbackUrl));

            return IdentityResult.Success;
        }
        #endregion

        #region Change User's Email
        public async Task<IdentityResult> ChangeEmailAsync(ConfirmChangeEmailDto confirmEmailRequest)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailRequest.Email);

            if (user is null)
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }
            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(confirmEmailRequest.Code));
            user.UserName = confirmEmailRequest.NewEmail;
            user.NormalizedUserName = confirmEmailRequest.NewEmail.ToUpper();
            var result = await _userManager.ChangeEmailAsync(user, confirmEmailRequest.NewEmail, decodedCode);

            if (!result.Succeeded)
            {
                return IdentityResult.Failed(result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
            }

            return IdentityResult.Success;
        }

        #endregion

        #region Send email to change phone number
        //public async Task<bool> SendEmailChangePhoneNumberAsync(ChangePhoneDto changePhoneDto)
        //{
        //    var user = await _userManager.FindByEmailAsync(changePhoneDto.Email);
        //    if (user == null)
        //    {
        //        return false;
        //    }

        //    var code = _userRepository.GenerateRandomCode();
        //    _verificationCodes[changePhoneDto.Email] = code;
        //    await _emailSender.SendEmailAsync(changePhoneDto.Email, "Phone Number Change Code", $"Your verification code is {code}");

        //    return true;
        //}

        //#endregion

        //#region Confirm Email Change Phone Number
        //public async Task<string> ConfirmEmailChangePhoneNumberAsync(ConfirmEmailChangePhoneNumberDto confirmEmailRequest)
        //{
        //    var user = await _userManager.FindByEmailAsync(confirmEmailRequest.Email);
        //    if (user == null)
        //    {
        //        return null;
        //    }
        //    if (!_verificationCodes.TryGetValue(confirmEmailRequest.Email, out string savedCode) || savedCode != confirmEmailRequest.Code)
        //    {
        //        return null;
        //    }

        //    user.PhoneNumber = confirmEmailRequest.NewPhoneNumber;
        //    await _userManager.UpdateAsync(user);

        //    _verificationCodes.Remove(confirmEmailRequest.Email);

        //    return confirmEmailRequest.NewPhoneNumber;
        //}
        #endregion

        #region Change User's Avatar
        public async Task<IdentityResult> ChangeAvatarAsync(ApplicationUser user, IFormFile avatarFile)
        {
            var (uploadAvatarUrl, result) = await UploadImageAsync(avatarFile, user.Id);
            if (string.IsNullOrEmpty(uploadAvatarUrl) || !result.Succeeded)
            {
                return IdentityResult.Failed(result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
            }

            user.AvatarUrl = uploadAvatarUrl;
            var updateResult = await _userManager.UpdateAsync(user);

            return IdentityResult.Success;
        }
        #endregion

        #region Upload Image
        public async Task<(string, IdentityResult)> UploadImageAsync(IFormFile avatarFile, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (null, IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserNotFound, Description = "User not found" }));
                }
                string storagePath = _configuration.ImageStoragePath;

                if (avatarFile == null || avatarFile.Length <= 0)
                {
                    _logger.LogInformation("No image uploaded");
                    return (null, IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserNoImageUpload, Description = "No image uploaded" }));
                }

                string uploadsFolder = storagePath;

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                //string userFolder = Path.Combine(uploadsFolder, userId);
                //if (!Directory.Exists(userFolder))
                //{
                //    Directory.CreateDirectory(userFolder);
                //}

                //string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName)
                string uniqueFileName = userId + "_" + avatarFile.FileName;

                //string filePath = Path.Combine(userFolder, avatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }
                byte[] avatarContent;
                using (var memoryStream = new MemoryStream())
                {
                    await avatarFile.CopyToAsync(memoryStream);
                    avatarContent = memoryStream.ToArray();
                }
                user.AvatarContent = avatarContent;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Image {FileName} has been successfully uploaded to {UserFolder}", avatarFile.FileName, uploadsFolder);
                    return (uniqueFileName, IdentityResult.Success);
                }
                else
                {
                    _logger.LogError("Failed to add image");
                    return (null, IdentityResult.Failed(result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First()));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add image: {ErrorMessage}", ex.Message);
                throw;
            }
        }


        #endregion

        #region Update User's Info
        public async Task<IdentityResult> UpdateUser(UpdateUserInfoDto updateUser, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            user.FirstName = updateUser.FirstName;
            user.LastName = updateUser.LastName;
            user.Address = updateUser.Address;
            user.PhoneNumber = updateUser.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return IdentityResult.Success;
            }
            return IdentityResult.Failed(result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
        }
        #endregion

        #region Send Email (Identity)
        public async Task<IdentityResult> SendEmailBlazor(SendEmailRequestDto sendEmailRequestDto)
        {
            if (string.IsNullOrEmpty(sendEmailRequestDto.To) || string.IsNullOrEmpty(sendEmailRequestDto.Subject) || string.IsNullOrEmpty(sendEmailRequestDto.Body))
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserInvalidRequest, Description = "Invalid request" });
            }

            await _emailSender.SendEmailAsync(sendEmailRequestDto.To, sendEmailRequestDto.Subject, sendEmailRequestDto.Body);
            return IdentityResult.Success;
        }
        #endregion

        #region Forgot Password & Reset Password
        public async Task<IdentityResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{_configuration.DeepLinksSettings.BaseUrl + _configuration.DeepLinksSettings.ForgotPassword}?email={forgotPasswordDto.Email}&code={code}";

            await _emailSender.SendEmailAsync(forgotPasswordDto.Email, "Reset password", HtmlEncoder.Default.Encode(callbackUrl));

            return IdentityResult.Success;
        }
        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserNotFound, Description = "User not found" });
            }
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                return IdentityResult.Failed(new IdentityError { Code = ResponseCode.UserPasswordNotMatch, Description = "New password and confirm password do not match." });
            }

            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.Code));
            var result = await _userManager.ResetPasswordAsync(user, decodedCode, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return IdentityResult.Failed(result.Errors.Select(x => new IdentityError { Code = x.Code, Description = x.Description }).First());
            }
            return result;
        }
        #endregion
    }
}