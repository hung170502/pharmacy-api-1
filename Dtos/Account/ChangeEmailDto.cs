using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class ChangeEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }

        public ChangeEmailDto()
        {
            Email = string.Empty;
            NewEmail = string.Empty;
        }

    }


}
