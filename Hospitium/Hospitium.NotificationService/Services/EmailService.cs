using System.Net;
using System.Net.Mail;

namespace Hospitium.NotificationService.Services
{
    /// <summary>
    /// Service for sending HTML-formatted emails via SMTP. Used by notification consumers to deliver automated emails.
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="config">The application configuration containing SMTP settings.</param>
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Prefer environment variables directly; fall back to config
            var host     = Environment.GetEnvironmentVariable("SMTP_HOST")     ?? _config["Smtp:Host"];
            var portStr  = Environment.GetEnvironmentVariable("SMTP_PORT")     ?? _config["Smtp:Port"] ?? "587";
            var fromEmail= Environment.GetEnvironmentVariable("SMTP_EMAIL")    ?? _config["Smtp:Email"];
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? _config["Smtp:Password"];
            var port     = int.Parse(portStr);

            var smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var mail = new MailMessage(fromEmail, to, subject, body);
            mail.IsBodyHtml = true;

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