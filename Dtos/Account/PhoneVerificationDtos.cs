using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Account
{
    /// <summary>
    /// Request: Kiểm tra định dạng số điện thoại
    /// </summary>
    public class PhoneValidationRequest
    {
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response: Kết quả kiểm tra số điện thoại
    /// </summary>
    public class PhoneValidationResponse
    {
        public bool IsValid { get; set; }
        public string? Carrier { get; set; }
        public string? FormattedNumber { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Request: Gửi OTP đến số điện thoại
    /// </summary>
    public class SendPhoneOtpRequest
    {
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request: Xác minh OTP
    /// </summary>
    public class VerifyPhoneOtpRequest
    {
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP không được để trống")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 ký tự")]
        public string OtpCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response: Kết quả gửi OTP
    /// </summary>
    public class SendPhoneOtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; } = 5;
        public string? Otp { get; set; }
        //public string? ZaloLink { get; set; }  
    }

    /// <summary>
    /// Response: Kết quả xác minh OTP
    /// </summary>
    public class VerifyPhoneOtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request: Đăng ký/Đăng nhập bằng số điện thoại
    /// </summary>
    public class PhoneLoginRequest
    {
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP không được để trống")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 ký tự")]
        public string OtpCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response: Kết quả đăng nhập bằng số điện thoại
    /// </summary>
    public class PhoneLoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? Message { get; set; }
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// Thông tin User cơ bản (dùng chung)
    /// </summary>
    public class UserInfo
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Carrier { get; set; }  // ← THÊM DÒNG NÀY
        public string? Avatar { get; set; }
    }
}