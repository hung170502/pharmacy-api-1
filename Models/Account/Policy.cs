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
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [Required]
        public int Sort { get; set; }
        public virtual ICollection<PolicyPermission> PolicyPermissions { get; set; }
        public Policy()
        {
            PolicyPermissions = new HashSet<PolicyPermission>();
        }

    }
}