using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Services.Account
{
    public interface ISmtpClient
    {
        Task SendMailAsync(MailMessage message);
    }
}
