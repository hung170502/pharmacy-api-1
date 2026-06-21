using Microsoft.Extensions.Logging;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Account
{
    public interface IPhoneVerificationService
    {
        PhoneValidationResponse Validate(string rawPhone);
    }

    public class PhoneVerificationService : IPhoneVerificationService
    {
        private readonly ILogger<PhoneVerificationService> _logger;

        public PhoneVerificationService(ILogger<PhoneVerificationService> logger)
        {
            _logger = logger;
        }

        public PhoneValidationResponse Validate(string rawPhone)
        {
            _logger.LogInformation($"Validating phone: {rawPhone}");

            var phone = VietnamesePhoneHelper.Normalize(rawPhone);

            if (!VietnamesePhoneHelper.IsValidFormat(phone))
            {
                return new PhoneValidationResponse
                {
                    IsValid = false,
                    Message = "Số điện thoại không đúng định dạng Việt Nam (0xxxxxxxxx)"
                };
            }

            var carrier = VietnamesePhoneHelper.GetCarrier(phone);

            _logger.LogInformation($"Phone {phone} is valid - Carrier: {carrier}");

            return new PhoneValidationResponse
            {
                IsValid = true,
                FormattedNumber = phone,
                Carrier = carrier,
                Message = $"Hợp lệ - Nhà mạng: {carrier}"
            };
        }
    }
}