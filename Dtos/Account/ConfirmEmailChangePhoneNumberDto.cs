using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class ConfirmEmailChangePhoneNumberDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Code { get; set; }
        public string NewPhoneNumber { get; set; }

        public ConfirmEmailChangePhoneNumberDto()
        {
            Email = string.Empty;
            Code = string.Empty;
            NewPhoneNumber = string.Empty;
        }
    }
}
