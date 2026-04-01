using Hospitium.Contracts.Events;
using MassTransit;
using Hospitium.NotificationService.Services;

namespace Hospitium.NotificationService.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly EmailService _email;

        public UserRegisteredConsumer(EmailService email)
        {
            _email = email;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var user = context.Message;

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden;'>
                    <div style='background-color: #4F46E5; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px;'>Welcome to Hospitium!</h1>
                    </div>
                    <div style='padding: 30px;'>
                        <h2 style='color: #333;'>Hi {user.Name},</h2>
                        <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                            Your account has been successfully created. We are excited to have you join our community!
                        </p>
                        <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                            Start exploring thousands of amazing hotels and book your perfect stay today.
                        </p>
                        <div style='text-align: center; margin-top: 30px;'>
                            <a href='http://localhost:4200/home' style='background-color: #4F46E5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Explore Hotels</a>
                        </div>
                    </div>
                    <div style='background-color: #f3f4f6; color: #888; text-align: center; padding: 15px; font-size: 12px;'>
                        &copy; {DateTime.Now.Year} Hospitium Inc. All rights reserved.
                    </div>
                </div>";

            await _email.SendEmailAsync(
                user.Email,
                "Welcome to Hospitium! 🎉",
                htmlBody
            );

        }
    }
}