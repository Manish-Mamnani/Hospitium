
using Hospitium.NotificationService.Services;
using Hospitium.NotificationService.Consumers;
using MassTransit;

namespace Hospitium.NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<EmailService>();

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<UserRegisteredConsumer>();
                x.AddConsumer<UserLoggedInConsumer>();
                x.AddConsumer<BookingCreatedConsumer>();
                x.AddConsumer<BookingCancelledConsumer>();
                x.AddConsumer<HotelApprovedConsumer>();
                x.AddConsumer<HotelRejectedConsumer>();
                x.AddConsumer<PasswordResetRequestedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h => { });

                    cfg.ConfigureEndpoints(context);
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
