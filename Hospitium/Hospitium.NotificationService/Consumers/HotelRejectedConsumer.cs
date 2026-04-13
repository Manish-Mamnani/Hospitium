using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;
using MassTransit;

/// <summary>
/// MassTransit consumer that handles <see cref="HotelRejectedEvent"/> events by sending a rejection notification email to the hotel manager.
/// </summary>
public class HotelRejectedConsumer : IConsumer<HotelRejectedEvent>
{
    private readonly EmailService _email;

    /// <summary>
    /// Initializes a new instance of the <see cref="HotelRejectedConsumer"/> class.
    /// </summary>
    /// <param name="email">The email service for sending notifications.</param>
    public HotelRejectedConsumer(EmailService email)
    {
        _email = email;
    }

    public async Task Consume(ConsumeContext<HotelRejectedEvent> context)
    {
        var hotel = context.Message;

        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 8px; overflow: hidden;'>
                <div style='background-color: #DC2626; color: white; padding: 20px; text-align: center;'>
                    <h1 style='margin: 0; font-size: 24px;'>Hospitium Partners</h1>
                    <p style='margin: 5px 0 0;'>Update on Your Hotel Application</p>
                </div>
                <div style='padding: 30px;'>
                    <h2 style='color: #333;'>Application Status</h2>
                    <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                        Thank you for your interest in partnering with Hospitium. After reviewing your application for <strong>{hotel.HotelName}</strong>, we regret to inform you that we cannot approve it at this time.
                    </p>
                    <p style='color: #555; font-size: 16px; line-height: 1.5;'>
                        This decision is often based on missing documentation, property standards, or verification issues. You can review our partnership guidelines and resubmit your application once the requirements are met.
                    </p>
                    <div style='text-align: center; margin-top: 30px;'>
                        <a href='http://localhost:4200/support' style='background-color: #4B5563; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Contact Support</a>
                    </div>
                </div>
                <div style='background-color: #f3f4f6; color: #888; text-align: center; padding: 15px; font-size: 12px;'>
                    &copy; {DateTime.Now.Year} Hospitium Inc. All rights reserved.
                </div>
            </div>";

        await _email.SendEmailAsync(
            hotel.ManagerEmail,
            "Hospitium: Update regarding your hotel application",
            htmlBody
        );

    }
}