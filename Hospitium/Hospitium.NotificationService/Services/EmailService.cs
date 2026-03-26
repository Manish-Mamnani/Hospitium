namespace Hospitium.NotificationService.Services
{
    public class EmailService
    {
        public void SendEmail(string to, string subject, string body)
        {
            Console.WriteLine($"""
                ==========================
                Email To: {to}
                Subject: {subject}
                Body: {body}
                ==========================
                """);
        }
    }
}