using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public partial class JwtAuthResultDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public JwtAuthResultDto()
        {
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
        }
    }
}
