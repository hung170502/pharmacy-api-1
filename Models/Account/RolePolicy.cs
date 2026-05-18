using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Account
{
    [Table("RolePolicies")]
    public class RolePolicy
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        [StringLength(450)]
        public string RoleId { get; set; } = string.Empty;

        [Key]
        [Column(Order = 1)]
        [Required]
        [StringLength(36)]
        public string PolicyId { get; set; } = string.Empty;

        public virtual Role Role { get; set; } = null!;
        public virtual Policy Policy { get; set; } = null!;
    }
}