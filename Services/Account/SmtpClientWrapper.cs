//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Mail;
//using System.Text;
//using System.Threading.Tasks;

//namespace Pharmacy_API.Services.Account
//{
//    public class SmtpClientWrapper : ISmtpClient
//    {
//        #region Properties
//        public SmtpClient Client { get; }
//        #endregion

//        public SmtpClientWrapper(SmtpClient client)
//        {
//            Client = client;
//        }

//        public async Task SendMailAsync(MailMessage message)
//        {
//            await Client.SendMailAsync(message);
//        }
//    }
//}
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Pharmacy_API.Supports;
using System.Net.Mail;

namespace Pharmacy_API.Services.Account
{
    public class SmtpClientWrapper : ISmtpClient
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<SmtpClientWrapper> _logger;
        private SmtpClient smtpClient;

        public SmtpClientWrapper(SmtpClient smtpClient)
        {
            this.smtpClient = smtpClient;
        }

        public SmtpClientWrapper(
            IOptions<AppSettings> appSettings,
            ILogger<SmtpClientWrapper> logger)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public async Task SendMailAsync(MailMessage message)
        {
            try
            {
                using (var smtpClient = new SmtpClient(
                    _appSettings.MailSettings.Host,
                    _appSettings.MailSettings.Port))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential(
                        _appSettings.MailSettings.Mail,
                        _appSettings.MailSettings.Password
                    );
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    _logger.LogInformation($"Attempting to send email to {message.To} via {_appSettings.MailSettings.Host}:{_appSettings.MailSettings.Port}");

                    await smtpClient.SendMailAsync(message);

                    _logger.LogInformation($"Email sent successfully to {message.To}");
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"SMTP Error: StatusCode={smtpEx.StatusCode}, Message={smtpEx.Message}");
                _logger.LogError($"Inner Exception: {smtpEx.InnerException?.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"General email error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}