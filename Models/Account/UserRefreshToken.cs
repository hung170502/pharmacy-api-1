using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Account
{
    [Table("UserRefreshToken")]
    public class UserRefreshToken
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(36)]
        public string TokenId { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string RefreshToken { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryTime { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
    }
}