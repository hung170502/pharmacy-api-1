using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class ConfirmChangeEmailDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Code { get; set; }
        public string NewEmail { get; set; }

        public ConfirmChangeEmailDto()
        {
            Email = string.Empty;
            Code = string.Empty;
            NewEmail = string.Empty;
        }
    }
}
