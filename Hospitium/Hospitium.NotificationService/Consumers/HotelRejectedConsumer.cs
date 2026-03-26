using Hospitium.Contracts.Events;
using Hospitium.NotificationService.Services;
using MassTransit;

public class HotelRejectedConsumer : IConsumer<HotelRejectedEvent>
{
    private readonly EmailService _email;

    public HotelRejectedConsumer(EmailService email)
    {
        _email = email;
    }

    public Task Consume(ConsumeContext<HotelRejectedEvent> context)
    {
        var hotel = context.Message;

        _email.SendEmail(
            hotel.ManagerEmail,
            "Hotel Rejected",
            $"Your hotel {hotel.HotelName} was rejected."
        );

        return Task.CompletedTask;
    }
}