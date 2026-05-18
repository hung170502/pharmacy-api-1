using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Supports
{
    public static class ResponseCode
    {
        public const string AuthInvalid = "InvalidCredential";
        public const string UserNotFound = "UserNotFound";
        public const string UserNotActive = "UserNotActive";

        public const string UserFailChangePassword = "FailToChangePassword";
        public const string UserSendEmail = "FailToSendEmail";
        public const string UserConfirmEmail = "FailToConfirmEmail";
        public const string UserChangeAvatar = "FailToChangeAvatar";
        public const string UserResetPassword = "FailToResetPassword";
        public const string UserPasswordNotMatch = "PasswordNotMatch";
        public const string UserInvalidRequest = "InvalidRequest";
        public const string UserNoImageUpload = "NoImageUploaded";
        public const string UserEmailNotMatch = "EmailNotMatch";
        public const string UserExisted = "EmailExisted";
        public const string CategoryNotFound = "CategoryNotFound";
        public const string BrandNotFound = "BrandNotFound";
        public const string CountryNotFound = "CountryNotFound";
        public const string UnitNotFound = "UnitNotFound";
    }
}
