using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Services.Account
{
    public interface IEmailSenderService
    {
        Task<bool> SendEmailAsync(string email, string subject, string message);
    }
}
