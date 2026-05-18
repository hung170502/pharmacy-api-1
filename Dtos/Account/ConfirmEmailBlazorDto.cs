using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class ConfirmEmailBlazorDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }

        public ConfirmEmailBlazorDto()
        {
            UserId = string.Empty;
            Email = string.Empty;
            Code = string.Empty;
        }
    }
}
