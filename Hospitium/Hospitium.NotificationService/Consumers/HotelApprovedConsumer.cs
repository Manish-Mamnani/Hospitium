using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;
using MassTransit;

public class HotelApprovedConsumer : IConsumer<HotelApprovedEvent>
{
    private readonly EmailService _email;

    public HotelApprovedConsumer(EmailService email)
    {
        _email = email;
    }

    public async Task Consume(ConsumeContext<HotelApprovedEvent> context)
    {
        var hotel = context.Message;

        await _email.SendEmailAsync(
            hotel.ManagerEmail,
            "Hotel Approved 🎉",
            $"Your hotel {hotel.HotelName} is approved."
        );

    }
}