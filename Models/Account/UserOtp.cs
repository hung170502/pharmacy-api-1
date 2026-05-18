using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy_API.Models.Account
{
    public class UserOtp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(4)]
        public string Code { get; set; } = string.Empty;

        public DateTime ExpiredAt { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
