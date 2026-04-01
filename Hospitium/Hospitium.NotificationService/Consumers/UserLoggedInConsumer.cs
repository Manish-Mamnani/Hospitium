using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;
using MassTransit;

public class UserLoggedInConsumer : IConsumer<UserLoggedInEvent>
{
    private readonly EmailService _email;

    public UserLoggedInConsumer(EmailService email)
    {
        _email = email;
    }

    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var user = context.Message;

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden;'>
                <div style='background-color: #F59E0B; color: white; padding: 20px; text-align: center;'>
                    <h1 style='margin: 0; font-size: 24px;'>Hospitium Security</h1>
                    <p style='margin: 5px 0 0;'>New Login Alert</p>
                </div>
                <div style='padding: 30px;'>
                    <h2 style='color: #333;'>Hello,</h2>
                    <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                        We noticed a new login to your Hospitium account on <strong>{user.LoginTime:f}</strong>.
                    </p>
                    <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                        If this was you, no further action is required. If you did not authorize this login, please change your password immediately or contact our support team.
                    </p>
                </div>
                <div style='background-color: #f3f4f6; color: #888; text-align: center; padding: 15px; font-size: 12px;'>
                    &copy; {DateTime.Now.Year} Hospitium Inc. All rights reserved.
                </div>
            </div>";

        await _email.SendEmailAsync(
            user.Email,
            "Hospitium: Login Alert",
            htmlBody
        );

    }
}