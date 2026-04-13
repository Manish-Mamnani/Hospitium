using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;
using MassTransit;

/// <summary>
/// MassTransit consumer that handles <see cref="BookingCreatedEvent"/> events by sending a booking confirmation email to the user.
/// </summary>
public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
{
    private readonly EmailService _email;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookingCreatedConsumer"/> class.
    /// </summary>
    /// <param name="email">The email service for sending notifications.</param>
    public BookingCreatedConsumer(EmailService email)
    {
        _email = email;
    }

    public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var booking = context.Message;
        var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        var checkInLocal = TimeZoneInfo.ConvertTimeFromUtc(booking.FromDate, istZone);
        var checkOutLocal = TimeZoneInfo.ConvertTimeFromUtc(booking.ToDate, istZone);

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden;'>
                <div style='background-color: #4F46E5; color: white; padding: 20px; text-align: center;'>
                    <h1 style='margin: 0; font-size: 24px;'>Hospitium</h1>
                    <p style='margin: 5px 0 0;'>Your Premium Booking Partner</p>
                </div>
                <div style='padding: 30px;'>
                    <h2 style='color: #333;'>Booking Confirmation</h2>
                    <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                        Good news! Your booking <strong>#{booking.BookingId}</strong> has been confirmed. 
                        Get ready for a wonderful stay.
                    </p>
                    <div style='background-color: #f9fafb; padding: 15px; border-radius: 5px; margin-top: 20px;'>
                        <p style='margin: 0 0 10px; color: #444;'><strong>Hotel:</strong> {booking.HotelName}</p>
                        <p style='margin: 0 0 10px; color: #444;'><strong>Stay Dates:</strong> {checkInLocal:dd-MM-yyyy} to {checkOutLocal:dd-MM-yyyy}</p>
                        <p style='margin: 0 0 10px; color: #444;'><strong>Booking ID:</strong> HB-{booking.BookingId}</p>
                        <p style='margin: 0 0 10px; color: #444;'><strong>Rooms Booked:</strong> {booking.NumberOfRooms}</p>
                    </div>
                    <p style='color: #777; font-size: 14px; margin-top: 30px;'>
                        Thank you for choosing Hospitium! If you have any questions, feel free to reply to this email.
                    </p>
                </div>
                <div style='background-color: #f3f4f6; color: #888; text-align: center; padding: 15px; font-size: 12px;'>
                    &copy; {DateTime.Now.Year} Hospitium Inc. All rights reserved.
                </div>
            </div>";

        await _email.SendEmailAsync(
            booking.UserEmail,
            "Hospitium: Your Booking is Confirmed! 🎉",
            htmlBody
        );

    }
}