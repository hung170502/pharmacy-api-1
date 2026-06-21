using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public partial class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        // ===== THÊM 2 TRƯỜNG MỚI =====
        public string? PhoneNumber { get; set; }
        public string? PhoneOtp { get; set; }

        public RegisterRequestDto()
        {
            Email = string.Empty;
            Password = string.Empty;
        }
    }
}