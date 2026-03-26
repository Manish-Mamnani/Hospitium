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

        public Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var user = context.Message;

            _email.SendEmail(
                user.Email,
                "Welcome to Hospitium 🎉",
                $"Hi {user.Name}, your account has been created."
            );

            return Task.CompletedTask;
        }
    }
}