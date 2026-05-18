using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public partial class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public LoginRequestDto()
        {
            Email = string.Empty;
            Password = string.Empty;
        }
    }
}
