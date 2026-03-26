using MassTransit;
using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;

namespace Hospitium.NotificationService.Consumers
{
    public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
    {
        private readonly EmailService _email;

        public BookingCancelledConsumer(EmailService email)
        {
            _email = email;
        }

        public Task Consume(ConsumeContext<BookingCancelledEvent> context)
        {
            var booking = context.Message;

            _email.SendEmail(
                booking.UserEmail,
                "Booking Cancelled",
                $"Your booking #{booking.BookingId} was cancelled at {booking.CancelledAt}."
            );

            return Task.CompletedTask;
        }
    }
}