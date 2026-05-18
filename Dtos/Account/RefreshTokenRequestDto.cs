using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public partial class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public string UserId { get; set; }

        public RefreshTokenRequestDto()
        {
            RefreshToken = string.Empty;
            UserId = string.Empty;
        }
    }
}
