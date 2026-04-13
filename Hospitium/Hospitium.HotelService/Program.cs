
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
            // Load .env file into environment variables (ignored if file doesn't exist)
            var envFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
            if (File.Exists(envFile))
            {
                foreach (var line in File.ReadAllLines(envFile))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                        Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }

            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();

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
                x.AddConsumer<ReviewAddedConsumer>();

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

                    cfg.ReceiveEndpoint("review-added-queue", e =>
                    {
                        e.ConfigureConsumer<ReviewAddedConsumer>(context);
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
                var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"]!;
                var key = Encoding.UTF8.GetBytes(jwtKey);

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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
