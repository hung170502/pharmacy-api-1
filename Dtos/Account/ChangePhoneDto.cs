using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class ChangePhoneDto
    {
        public string Email { get; set; }

        public ChangePhoneDto()
        {
            Email = string.Empty;
        }
    }
}
