using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class ChangePasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }

        public ChangePasswordDto()
        {
            Email = string.Empty;
            OldPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

    }
}
