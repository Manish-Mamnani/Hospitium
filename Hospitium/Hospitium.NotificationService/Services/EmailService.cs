using System.Net;
using System.Net.Mail;

namespace Hospitium.NotificationService.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var host = _config["Smtp:Host"];
            var port = int.Parse(_config["Smtp:Port"]!);
            var fromEmail = _config["Smtp:Email"];
            var password = _config["Smtp:Password"];

            var smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var mail = new MailMessage(fromEmail, to, subject, body);

            try
            {
                await smtpClient.SendMailAsync(mail);
                Console.WriteLine("✅ Email sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email failed: {ex.Message}");
            }
        }
    }
}