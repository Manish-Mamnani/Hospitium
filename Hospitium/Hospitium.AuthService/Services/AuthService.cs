using Hospitium.AuthService.Contracts.Events;
using Hospitium.AuthService.Data;
using Hospitium.AuthService.DTOs;
using Hospitium.AuthService.Exceptions;
using Hospitium.AuthService.Models;
using Hospitium.AuthService.Services.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly JwtService _jwtService;
        //private readonly IPublishEndpoint _publishEndpoint;

        public AuthService(AuthDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
            //_publishEndpoint = publishEndpoint;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

            if (existingUser != null)
            {
                throw new UserAlreadyExistsException("Email already registered");
            }

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                Role = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            //await _publishEndpoint.Publish(new UserRegisteredEvent
            //{
            //    UserId = user.UserId,
            //    Name = user.FullName,
            //    Email = user.Email
            //});

            return CreateAuthResponse(user, "User registered successfully");
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!validPassword)
            {
                throw new InvalidCredentialsException("Invalid credentials");
            }

            return CreateAuthResponse(user, "Login successful");
        }

        private AuthResponseDto CreateAuthResponse(User user, string message)
        {
            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Message = message,
                Token = token,
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
