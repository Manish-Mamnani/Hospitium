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

    public Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var user = context.Message;

        _email.SendEmail(
            user.Email,
            "Login Alert",
            $"Login detected at {user.LoginTime}"
        );

        return Task.CompletedTask;
    }
}