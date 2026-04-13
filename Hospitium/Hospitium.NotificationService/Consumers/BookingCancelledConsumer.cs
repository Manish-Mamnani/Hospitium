using MassTransit;
using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;

namespace Hospitium.NotificationService.Consumers
{
    /// <summary>
    /// MassTransit consumer that handles <see cref="BookingCancelledEvent"/> events by sending a cancellation
    /// confirmation email to the user, including refund and deduction details in IST timezone.
    /// </summary>
    public class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
    {
        private readonly EmailService _email;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingCancelledConsumer"/> class.
        /// </summary>
        /// <param name="email">The email service for sending notifications.</param>
        public BookingCancelledConsumer(EmailService email)
        {
            _email = email;
        }

        public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
        {
            var booking = context.Message;
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(booking.CancelledAt, istZone);

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden;'>
                    <div style='background-color: #EF4444; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px;'>Hospitium</h1>
                        <p style='margin: 5px 0 0;'>Booking Cancellation</p>
                    </div>
                    <div style='padding: 30px;'>
                        <h2 style='color: #333;'>Booking Cancelled</h2>
                        <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                            We're sorry to see you go. Your booking <strong>#{booking.BookingId}</strong> has been successfully cancelled.
                        </p>
                        <div style='background-color: #fce8e8; padding: 15px; border-radius: 5px; margin-top: 20px; border: 1px solid #fecaca;'>
                            <p style='margin: 0 0 10px; color: #7f1d1d;'><strong>Hotel:</strong> {booking.HotelName}</p>
                            <p style='margin: 0 0 10px; color: #7f1d1d;'><strong>Booking ID:</strong> HB-{booking.BookingId}</p>
                            <p style='margin: 0 0 10px; color: #7f1d1d;'><strong>Refundable Amount:</strong> ₹{booking.RefundAmount}</p>
                            <p style='margin: 0 0 10px; color: #7f1d1d;'><strong>Deduction:</strong> ₹{booking.DeductionAmount}</p>
                            <p style='margin: 0 0 10px; color: #7f1d1d;'><strong>Cancelled At:</strong> {localTime:dd-MM-yyyy HH:mm}</p>
                        </div>
                        <p style='color: #777; font-size: 14px; margin-top: 30px;'>
                            Any eligible refund will be processed within 5-7 business days. We hope to host you another time!
                        </p>
                    </div>
                    <div style='background-color: #f3f4f6; color: #888; text-align: center; padding: 15px; font-size: 12px;'>
                        &copy; {DateTime.Now.Year} Hospitium Inc. All rights reserved.
                    </div>
                </div>";

            await _email.SendEmailAsync(
                booking.UserEmail,
                "Hospitium: Booking Cancellation Notice",
                htmlBody
            );

        }
    }
}