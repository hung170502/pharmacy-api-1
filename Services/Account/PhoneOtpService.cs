using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pharmacy_API.Context;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Models.Account;

namespace Pharmacy_API.Services.Account
{
    public interface IPhoneOtpService
    {
        Task<SendPhoneOtpResponse> GenerateOtpAsync(string phoneNumber, string? userId = null);  // ← SỬA
        Task<VerifyPhoneOtpResponse> VerifyOtpAsync(string phoneNumber, string code);
        Task<bool> IsPhoneVerifiedAsync(string phoneNumber);
    }

    public class PhoneOtpService : IPhoneOtpService
    {
        private readonly AccountContext _context;
        private readonly ILogger<PhoneOtpService> _logger;

        public PhoneOtpService(AccountContext context, ILogger<PhoneOtpService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SendPhoneOtpResponse> GenerateOtpAsync(string phoneNumber, string? userId = null)  // ← SỬA
        {
            var oldOtps = await _context.PhoneOtps
                .Where(o => o.PhoneNumber == phoneNumber && !o.IsUsed)
                .ToListAsync();
            _context.PhoneOtps.RemoveRange(oldOtps);

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

            _logger.LogInformation($"📱 Generated OTP {code} for {phoneNumber}");

            await Task.CompletedTask;

            return new SendPhoneOtpResponse
            {
                Success = true,
                Message = "Mã OTP đã được tạo thành công",
                ExpiresInMinutes = 5,
                Otp = code
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
    }
}