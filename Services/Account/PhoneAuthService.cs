using Microsoft.Extensions.Logging;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Models.Account;

namespace Pharmacy_API.Services.Account
{
    public interface IPhoneAuthService
    {
        Task<PhoneLoginResponse> LoginWithPhoneAsync(PhoneLoginRequest request);
        Task<PhoneLoginResponse> RegisterWithPhoneAsync(PhoneLoginRequest request);
    }

    public class PhoneAuthService : IPhoneAuthService
    {
        private readonly IPhoneVerificationService _phoneVerification;
        private readonly IPhoneOtpService _phoneOtpService;
        private readonly ILogger<PhoneAuthService> _logger;
        // private readonly ITokenService _tokenService; // Nếu bạn có Token Service
        // private readonly UserManager<ApplicationUser> _userManager; // Nếu dùng Identity

        public PhoneAuthService(
            IPhoneVerificationService phoneVerification,
            IPhoneOtpService phoneOtpService,
            ILogger<PhoneAuthService> logger)
        {
            _phoneVerification = phoneVerification;
            _phoneOtpService = phoneOtpService;
            _logger = logger;
        }

        public async Task<PhoneLoginResponse> LoginWithPhoneAsync(PhoneLoginRequest request)
        {
            // 1. Validate format SĐT
            var validation = _phoneVerification.Validate(request.PhoneNumber);
            if (!validation.IsValid)
            {
                return new PhoneLoginResponse
                {
                    Success = false,
                    Message = validation.Message
                };
            }

            // 2. Verify OTP
            var otpResult = await _phoneOtpService.VerifyOtpAsync(
                validation.FormattedNumber!,
                request.OtpCode);

            if (!otpResult.Success)
            {
                return new PhoneLoginResponse
                {
                    Success = false,
                    Message = otpResult.Message
                };
            }

            // 3. Tìm hoặc tạo User
            // var user = await _userManager.FindByPhoneNumberAsync(validation.FormattedNumber);
            // if (user == null)
            // {
            //     // Tự động đăng ký nếu chưa có
            //     user = new ApplicationUser { PhoneNumber = validation.FormattedNumber };
            //     await _userManager.CreateAsync(user);
            // }

            // 4. Tạo Token
            // var token = await _tokenService.GenerateTokenAsync(user);
            // var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

            _logger.LogInformation($"User logged in with phone: {validation.FormattedNumber}");

            return new PhoneLoginResponse
            {
                Success = true,
                Message = "Đăng nhập thành công!",
                // Token = token,
                // RefreshToken = refreshToken,
                User = new UserInfo
                {
                    PhoneNumber = validation.FormattedNumber,
                    Carrier = validation.Carrier
                }
            };
        }

        public async Task<PhoneLoginResponse> RegisterWithPhoneAsync(PhoneLoginRequest request)
        {
            // 1. Validate format SĐT
            var validation = _phoneVerification.Validate(request.PhoneNumber);
            if (!validation.IsValid)
            {
                return new PhoneLoginResponse
                {
                    Success = false,
                    Message = validation.Message
                };
            }

            // 2. Verify OTP
            var otpResult = await _phoneOtpService.VerifyOtpAsync(
                validation.FormattedNumber!,
                request.OtpCode);

            if (!otpResult.Success)
            {
                return new PhoneLoginResponse
                {
                    Success = false,
                    Message = otpResult.Message
                };
            }

            // 3. Kiểm tra số đã tồn tại chưa
            // var existingUser = await _userManager.FindByPhoneNumberAsync(validation.FormattedNumber);
            // if (existingUser != null)
            // {
            //     return new PhoneLoginResponse
            //     {
            //         Success = false,
            //         Message = "Số điện thoại đã được đăng ký"
            //     };
            // }

            // 4. Tạo User mới
            // var user = new ApplicationUser
            // {
            //     PhoneNumber = validation.FormattedNumber,
            //     UserName = validation.FormattedNumber,
            //     CreatedAt = DateTime.UtcNow
            // };
            // await _userManager.CreateAsync(user);

            _logger.LogInformation($"New user registered with phone: {validation.FormattedNumber}");

            return new PhoneLoginResponse
            {
                Success = true,
                Message = "Đăng ký thành công!",
                User = new UserInfo
                {
                    PhoneNumber = validation.FormattedNumber,
                    Carrier = validation.Carrier
                }
            };
        }
    }
}