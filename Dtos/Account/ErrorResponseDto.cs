using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class ErrorResponseDto
    {
        public string Description { get; set; }
        public string Code { get; set; }

        public ErrorResponseDto()
        {
            Code = string.Empty;
            Description = string.Empty;
        }
    }
}
