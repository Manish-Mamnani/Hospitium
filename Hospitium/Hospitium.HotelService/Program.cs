
using Hospitium.HotelService.Data;
using Hospitium.HotelService.Middleware;
using Hospitium.HotelService.Services;
using Hospitium.HotelService.Consumers;
using Hospitium.HotelService.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hospitium.HotelService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<BookingCreatedConsumer>();
                x.AddConsumer<BookingCancelledConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h => { });

                    // 🔥 EXPLICIT QUEUE (IMPORTANT FIX)
                    cfg.ReceiveEndpoint("booking-created-queue", e =>
                    {
                        e.ConfigureConsumer<BookingCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("booking-cancelled-queue", e =>
                    {
                        e.ConfigureConsumer<BookingCancelledConsumer>(context);
                    });
                });
            });

            builder.Services.AddDbContext<HotelDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IHotelQueryService, HotelQueryService>();
            builder.Services.AddScoped<IHotelService, Services.HotelService>();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    NameClaimType = "UserId",
                    RoleClaimType = "Role"
                };
            });

            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
