using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Pharmacy_API.Dtos.Account;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Account
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public byte[]? AvatarContent { get; set; }

        public DateTime? LastLogin { get; set; }

        public bool IsOnline { get; set; } // ✅ Thêm dòng này

    }
}