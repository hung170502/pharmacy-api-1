using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public partial class ConfirmEmailRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Code { get; set; }

        public ConfirmEmailRequestDto()
        {
            Email = string.Empty;
            Code = string.Empty;
        }
    }
}
