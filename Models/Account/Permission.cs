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
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Sort { get; set; }

    }
}