using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pharmacy_API.Context;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Models.Account;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Pharmacy_API.Services.Account
{
    public interface IPhoneOtpService
    {
        Task<SendPhoneOtpResponse> GenerateOtpAsync(string phoneNumber, string? userId = null);
        Task<VerifyPhoneOtpResponse> VerifyOtpAsync(string phoneNumber, string code);
        Task<bool> IsPhoneVerifiedAsync(string phoneNumber);
    }

    public class PhoneOtpService : IPhoneOtpService
    {
        private readonly AccountContext _context;
        private readonly ILogger<PhoneOtpService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PhoneOtpService(
            AccountContext context,
            ILogger<PhoneOtpService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<SendPhoneOtpResponse> GenerateOtpAsync(string phoneNumber, string? userId = null)
        {
            // Xóa OTP cũ
            var oldOtps = await _context.PhoneOtps
                .Where(o => o.PhoneNumber == phoneNumber && !o.IsUsed)
                .ToListAsync();
            _context.PhoneOtps.RemoveRange(oldOtps);

            // Tạo OTP mới 6 số
            var code = new Random().Next(100000, 999999).ToString();
            var otpRecord = new PhoneOtp
            {
                PhoneNumber = phoneNumber,
                Code = code,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.PhoneOtps.Add(otpRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"📱 OTP {code} created for {phoneNumber}");

            // TODO: Sau này có Zalo OA thì gửi OTP qua Zalo ở đây
            // await SendZaloOtpAsync(phoneNumber, code);

            return new SendPhoneOtpResponse
            {
                Success = true,
                Message = "Mã OTP đã được tạo thành công",
                ExpiresInMinutes = 5,
                Otp = code,        // Hiển thị OTP trên màn hình
                //ZaloLink = null
            };
        }

        public async Task<VerifyPhoneOtpResponse> VerifyOtpAsync(string phoneNumber, string code)
        {
            var record = await _context.PhoneOtps
                .FirstOrDefaultAsync(o =>
                    o.PhoneNumber == phoneNumber &&
                    o.Code == code &&
                    !o.IsUsed);

            if (record == null)
            {
                return new VerifyPhoneOtpResponse
                {
                    Success = false,
                    Message = "Mã OTP không chính xác"
                };
            }

            if (record.ExpiredAt < DateTime.UtcNow)
            {
                return new VerifyPhoneOtpResponse
                {
                    Success = false,
                    Message = "Mã OTP đã hết hạn"
                };
            }

            record.IsUsed = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ OTP verified for {phoneNumber}");

            return new VerifyPhoneOtpResponse
            {
                Success = true,
                Message = "Xác thực thành công!"
            };
        }

        public async Task<bool> IsPhoneVerifiedAsync(string phoneNumber)
        {
            return await _context.PhoneOtps
                .AnyAsync(o =>
                    o.PhoneNumber == phoneNumber &&
                    o.IsUsed &&
                    o.CreatedAt > DateTime.UtcNow.AddDays(-30));
        }

        // TODO: Sau này có Zalo OA thì bỏ comment hàm này
        // private async Task<bool> SendZaloOtpAsync(string phoneNumber, string otpCode)
        // {
        //     // Gửi OTP qua Zalo OA
        // }
    }
}