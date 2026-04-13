using Hospitium.ReviewService.Data;
using Hospitium.ReviewService.Services;
using Hospitium.ReviewService.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hospitium.ReviewService
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

            // 🚌 MassTransit RabbitMQ Setup
            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h => { });
                });
            });

            // 🗄️ Database Setup
            builder.Services.AddDbContext<ReviewDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 💉 Dependency Injection
            builder.Services.AddScoped<IReviewService, Services.ReviewService>();

            // 🔐 Authentication Setup
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
                    NameClaimType = "UserId", // Maps sub to UserId
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

            // app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
