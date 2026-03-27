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

        await _email.SendEmailAsync(
            user.Email,
            "Login Alert",
            $"Login detected at {user.LoginTime}"
        );

    }
}