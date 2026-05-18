using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Dtos.Account
{
    public class SendEmailRequestDto
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public SendEmailRequestDto()
        {
            To = string.Empty;
            Subject = string.Empty;
            Body = string.Empty;
        }
    }
}
