using Hospitium.AuthService.DTOs;
using Hospitium.AuthService.Exceptions;
using Hospitium.AuthService.Models;
using Hospitium.AuthService.Repositories.Interfaces;
using Hospitium.AuthService.Services.Interfaces;
using Hospitium.Contracts.Events;
using MassTransit;

namespace Hospitium.AuthService.Services
{
    /// <summary>
    /// Service implementation for handling user authentication, registration, and password management.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly IPublishEndpoint _publish;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository for data access.</param>
        /// <param name="jwtService">The JWT service for token generation.</param>
        /// <param name="publish">The MassTransit publish endpoint for event-driven communication.</param>
        public AuthService(IUserRepository userRepository, JwtService jwtService, IPublishEndpoint publish)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _publish = publish;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (existingUser != null)
            {
                throw new UserAlreadyExistsException("Email already registered");
            }

            var requestedRole = dto.Role?.Trim();
            var finalRole = (requestedRole == "HotelManager") ? "HotelManager" : "User";

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                Role = finalRole,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            await _publish.Publish(new UserRegisteredEvent
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.FullName
            });

            return CreateAuthResponse(user, "User registered successfully");
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }

            var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!validPassword)
            {
                throw new InvalidCredentialsException("Invalid credentials");
            }

            await _publish.Publish(new UserLoggedInEvent
            {
                UserId = user.UserId,
                Email = user.Email,
                LoginTime = DateTime.UtcNow
            });

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

        public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            // Always return success to prevent email enumeration attacks
            if (user == null) return;

            // Generate a 6-digit numeric OTP
            var otp = new Random().Next(100000, 999999).ToString();

            user.ResetOtp = otp;
            user.ResetOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _userRepository.SaveChangesAsync();

            // Publish event so Notification Service sends the OTP email
            await _publish.Publish(new PasswordResetRequestedEvent
            {
                Email = user.Email,
                Name = user.FullName,
                Otp = otp
            });
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user == null || user.ResetOtp == null || user.ResetOtpExpiry == null)
                return false;

            if (user.ResetOtp != dto.Otp.Trim())
                return false;

            if (user.ResetOtpExpiry < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user == null)
                throw new Exception("User not found.");

            if (user.ResetOtp != dto.Otp.Trim() || user.ResetOtpExpiry < DateTime.UtcNow)
                throw new Exception("Invalid or expired OTP.");

            // Update password and clear OTP fields
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetOtp = null;
            user.ResetOtpExpiry = null;

            await _userRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserDto>> GetAllManagersAsync()
        {
            var managers = await _userRepository.GetAllByRoleAsync("HotelManager");

            return managers.Select(u => new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            });
        }
    }
}
