using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Account
{
    [Table("Permissions")]
    public class Permission
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;        // Products.View

        [Required]
        [StringLength(256)]
        public string DisplayName { get; set; } = string.Empty;  // Xem sản phẩm

        [Required]
        [StringLength(128)]
        public string Group { get; set; } = string.Empty;        // Sản phẩm, Đơn hàng

        [StringLength(4000)]
        public string? Description { get; set; }

        [Required]
        public int Sort { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<PolicyPermission> PolicyPermissions { get; set; }

        public Permission()
        {
            PolicyPermissions = new HashSet<PolicyPermission>();
        }
    }
}