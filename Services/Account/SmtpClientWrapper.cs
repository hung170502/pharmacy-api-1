using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Services.Account
{
    public class SmtpClientWrapper : ISmtpClient
    {
        #region Properties
        public SmtpClient Client { get; }
        #endregion

        public SmtpClientWrapper(SmtpClient client)
        {
            Client = client;
        }

        public async Task SendMailAsync(MailMessage message)
        {
            await Client.SendMailAsync(message);
        }
    }
}
