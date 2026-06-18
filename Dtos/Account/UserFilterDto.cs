// Dtos/Account/UserFilterDto.cs
using Pharmacy_API.Supports;
using System;

namespace Pharmacy_API.Dtos.Account
{
    public class UserFilterDto : FilterBase
    {
        // ✅ Đổi từ bool? thành bool
        public bool ExcludeAdmins { get; set; } = true; // Mặc định true

        public string? Keyword { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsLocked { get; set; }
        public string? RoleName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsOnline { get; set; }
        public bool IsDeep { get; set; } = false;
    }
}