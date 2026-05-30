//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Mail;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Options;
//using Pharmacy_API.Supports;

//namespace Pharmacy_API.Services.Account
//{
//    public class EmailSenderService : IEmailSenderService
//    {
//        #region properties
//        private readonly ILogger _logger;
//        private readonly AppSettings _appSettings;
//        public ISmtpClient _smtpClient;
//        #endregion

//        public EmailSenderService(IOptions<AppSettings> appSettings,
//            ILogger<EmailSenderService> logger,
//            ISmtpClient smtpClient)
//        {
//            _logger = logger;
//            _appSettings = appSettings.Value;
//            _smtpClient = smtpClient;
//        }

//        #region Functions
//        /// <summary>
//        /// Sends an email asynchronously.
//        /// </summary>
//        /// <param name="email">The email address to send the email to.</param>
//        /// <param name="subject">The subject of the email.</param>
//        /// <param name="message">The body of the email.</param>
//        /// <returns>A task that represents the asynchronous operation.</returns>
//        public async Task<bool> SendEmailAsync(string email, string subject, string message)
//        {
//            try
//            {
//                using (MailMessage mailMessage = new MailMessage())
//                {
//                    mailMessage.From = new MailAddress(
//                        _appSettings.MailSettings.Mail,
//                        _appSettings.MailSettings.DisplayName
//                    );

//                    mailMessage.To.Add(email);
//                    mailMessage.Subject = subject;
//                    mailMessage.Body = message;
//                    mailMessage.IsBodyHtml = true;

//                    await _smtpClient.SendMailAsync(mailMessage);

//                    _logger.LogInformation("Email sent successfully!");
//                }

//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"EMAIL ERROR: {ex.Message}");
//                _logger.LogError(ex.StackTrace);

//                return false;
//            }
//        }
//        #endregion

//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Account
{
    public class EmailSenderService : IEmailSenderService
    {
        #region properties
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        public ISmtpClient _smtpClient;
        #endregion

        public EmailSenderService(IOptions<AppSettings> appSettings,
            ILogger<EmailSenderService> logger,
            ISmtpClient smtpClient)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _smtpClient = smtpClient;
        }

        #region Functions
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="email">The email address to send the email to.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="message">The body of the email.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                _logger.LogInformation($"📨 Preparing to send email to {email}");
                _logger.LogInformation($"📋 SMTP Config - Host: {_appSettings.MailSettings.Host}, Port: {_appSettings.MailSettings.Port}, From: {_appSettings.MailSettings.Mail}");

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(
                        _appSettings.MailSettings.Mail,
                        _appSettings.MailSettings.DisplayName
                    );

                    mailMessage.To.Add(email);
                    mailMessage.Subject = subject;
                    mailMessage.Body = message;
                    mailMessage.IsBodyHtml = true;

                    _logger.LogInformation($"✉️ Sending email with subject '{subject}' to {email}");

                    await _smtpClient.SendMailAsync(mailMessage);

                    _logger.LogInformation($"✅ Email sent successfully to {email}");
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"❌ SMTP ERROR: Status={smtpEx.StatusCode}, Message={smtpEx.Message}");
                _logger.LogError($"   Inner Exception: {smtpEx.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ EMAIL ERROR: {ex.Message}");
                _logger.LogError($"   Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"   Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
        #endregion
    }
}