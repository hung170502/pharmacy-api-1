using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pharmacy_API.Dtos.Account
{
    public partial class UserDto
    {
        [JsonPropertyName("userId")]
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public byte[]? AvatarContent { get; set; }
        public ICollection<RoleDto>? Roles { get; set; } = new List<RoleDto>();

        public ICollection<string> RoleNames { get; set; } = new List<string>();
        public HashSet<string> Permissions { get; set; }

        public DateTime? LastLogin { get; set; } // Trường này theo dõi thời gian đăng nhập cuối cùng
        public bool IsOnline { get; set; }
        public DateTime? GetLastLoginInVietnamTime(DateTimeOffset? lastLogin)
        {
            return lastLogin?.ToOffset(TimeSpan.FromHours(7)).DateTime;
        }

    }
}