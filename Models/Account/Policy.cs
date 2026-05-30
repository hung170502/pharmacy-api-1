using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Account
{
    [Table("Policies")]
    public class Policy
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;         // ProductManagement

        [Required]
        [StringLength(256)]
        public string DisplayName { get; set; } = string.Empty;   // Quản lý sản phẩm

        [StringLength(4000)]
        public string? Description { get; set; }

        [Required]
        public int Sort { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<PolicyPermission> PolicyPermissions { get; set; }
        public virtual ICollection<RolePolicy> RolePolicies { get; set; }

        public Policy()
        {
            PolicyPermissions = new HashSet<PolicyPermission>();
            RolePolicies = new HashSet<RolePolicy>();
        }
    }
}