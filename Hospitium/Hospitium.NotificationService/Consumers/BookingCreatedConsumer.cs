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

    public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var booking = context.Message;

        await _email.SendEmailAsync(
            booking.UserEmail,
            "Booking Confirmed",
            $"Your booking #{booking.BookingId} is confirmed."
        );

    }
}