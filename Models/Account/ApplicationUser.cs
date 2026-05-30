using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Pharmacy_API.Models.Account
{
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public byte[]? AvatarContent { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsOnline { get; set; } = false;
        public DateTime? LastLogin { get; set; }

        // ===== THÊM DÒNG NÀY =====
        public virtual ICollection<UserRole> UserRoles { get; set; }
        // =========================

        public ApplicationUser()
        {
            UserRoles = new HashSet<UserRole>();
        }
    }
}