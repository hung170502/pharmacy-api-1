using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Pharmacy_API.Models.Account
{
    public class Role : IdentityRole
    {
        [StringLength(256)]
        public string? DisplayName { get; set; }

        [StringLength(4000)]
        public string? Description { get; set; }

        public int Sort { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<RolePolicy> RolePolicies { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }

        public Role()
        {
            RolePolicies = new HashSet<RolePolicy>();
            UserRoles = new HashSet<UserRole>();
        }
    }
}