using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;
using MassTransit;

public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
{
    private readonly EmailService _email;

    public BookingCreatedConsumer(EmailService email)
    {
        _email = email;
    }

    public Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var booking = context.Message;

        _email.SendEmail(
            booking.UserEmail,
            "Booking Confirmed",
            $"Your booking #{booking.BookingId} is confirmed."
        );

        return Task.CompletedTask;
    }
}