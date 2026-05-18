using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public ResetPasswordDto()
        {
            Email = string.Empty;
            Code = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}
