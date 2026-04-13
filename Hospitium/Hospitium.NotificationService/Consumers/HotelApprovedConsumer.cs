using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;
using MassTransit;

/// <summary>
/// MassTransit consumer that handles <see cref="HotelApprovedEvent"/> events by sending an approval notification email to the hotel manager.
/// </summary>
public class HotelApprovedConsumer : IConsumer<HotelApprovedEvent>
{
    private readonly EmailService _email;

    /// <summary>
    /// Initializes a new instance of the <see cref="HotelApprovedConsumer"/> class.
    /// </summary>
    /// <param name="email">The email service for sending notifications.</param>
    public HotelApprovedConsumer(EmailService email)
    {
        _email = email;
    }

    public async Task Consume(ConsumeContext<HotelApprovedEvent> context)
    {
        var hotel = context.Message;

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden;'>
                <div style='background-color: #10B981; color: white; padding: 20px; text-align: center;'>
                    <h1 style='margin: 0; font-size: 24px;'>Hospitium Partners</h1>
                    <p style='margin: 5px 0 0;'>Hotel Approved</p>
                </div>
                <div style='padding: 30px;'>
                    <h2 style='color: #333;'>Congratulations!</h2>
                    <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                        Your hotel, <strong>{hotel.HotelName}</strong>, has been verified and approved by our administration team.
                    </p>
                    <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                        Millions of travelers can now find and book your properties on Hospitium. Head over to your Manager Dashboard to add rooms, update prices, and view bookings.
                    </p>
                    <div style='text-align: center; margin-top: 30px;'>
                        <a href='http://localhost:4200/manager' style='background-color: #10B981; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Go to Dashboard</a>
                    </div>
                </div>
                <div style='background-color: #f3f4f6; color: #888; text-align: center; padding: 15px; font-size: 12px;'>
                    &copy; {DateTime.Now.Year} Hospitium Inc. All rights reserved.
                </div>
            </div>";

        await _email.SendEmailAsync(
            hotel.ManagerEmail,
            "Hospitium: Your Hotel is Approved! 🎉",
            htmlBody
        );

    }
}