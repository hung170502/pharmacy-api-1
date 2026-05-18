using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy_API.Supports
{
    public class AppSettings
    {
        public DeepLinksSettings DeepLinksSettings { get; set; }
        public JwtSettings Jwt { get; set; }
        public Redis Redis { get; set; }

        public MailSettings MailSettings { get; set; }

        public Google Google { get; set; }

        public AppSettings()
        {
            DeepLinksSettings = new DeepLinksSettings();
            Jwt = new JwtSettings();
            Redis = new Redis();
            MailSettings = new MailSettings();
            ImageStoragePath = string.Empty;
            Google = new Google();
        }

        public string ImageStoragePath { get; set; }
    }

    public class MailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }

        public MailSettings()
        {
            Mail = string.Empty;
            DisplayName = string.Empty;
            Host = string.Empty;
            Port = 0;
            Password = string.Empty;
            EnableSsl = false;
        }
    }

    public class Redis
    {
        public string Url { get; set; }

        public Redis()
        {
            Url = string.Empty;
        }
    }

    public class DeepLinksSettings
    {
        public string BaseUrl { get; set; }
        public string VerifyRegisterUser { get; set; }
        public string ChangeEmail { get; set; }
        public string ForgotPassword { get; set; }

        public DeepLinksSettings()
        {
            BaseUrl = string.Empty;
            VerifyRegisterUser = string.Empty;
            ChangeEmail = string.Empty;
            ForgotPassword = string.Empty;
        }
    }

    public class JwtSettings
    {
        public string Key { get; set; }
        public double AccessTokenExpiryInMinutes { get; set; }
        public string RefreshTokenName { get; set; }
        public string AppName { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }

        public JwtSettings()
        {
            Key = string.Empty;
            AccessTokenExpiryInMinutes = 0;
            RefreshTokenName = string.Empty;
            AppName = string.Empty;
            Audience = string.Empty;
            Issuer = string.Empty;
        }
    }

    public class Google
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUrl { get; set; }

        public Google()
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            RedirectUrl = string.Empty;
        }
    }
}
