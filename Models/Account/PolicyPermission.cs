using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Account
{
    [Table("PolicyPermissions")]
    public class PolicyPermission
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        [StringLength(36)]
        public string PolicyId { get; set; } = string.Empty;

        [Key]
        [Column(Order = 1)]
        [Required]
        [StringLength(36)]
        public string PermissionId { get; set; } = string.Empty;

        public virtual Policy Policy { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}