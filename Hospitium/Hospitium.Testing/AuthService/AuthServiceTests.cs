using Hospitium.AuthService.Data;
using Hospitium.AuthService.DTOs;
using Hospitium.AuthService.Exceptions;
using Hospitium.AuthService.Models;
using Hospitium.AuthService.Services;
using Hospitium.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Hospitium.Testing.AuthService
{
    /// <summary>
    /// Unit tests for the <see cref="Hospitium.AuthService.Services.AuthService"/> class,
    /// covering registration, login, OTP verification, password reset, and manager retrieval.
    /// </summary>
    [TestFixture]
    public class AuthServiceTests
    {
        private AuthDbContext _context = null!;
        private JwtService _jwtService = null!;
        private Mock<IPublishEndpoint> _publishMock = null!;
        private Hospitium.AuthService.Services.AuthService _authService = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AuthDbContext(options);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "super-secret-key-for-hospitium-testing-1234567890!" },
                    { "Jwt:Issuer", "Hospitium" },
                    { "Jwt:Audience", "HospitiumUsers" }
                })
                .Build();

            _jwtService = new JwtService(config);

            _publishMock = new Mock<IPublishEndpoint>();
            _publishMock
                .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userRepository = new Hospitium.AuthService.Repositories.UserRepository(_context);
            _authService = new Hospitium.AuthService.Services.AuthService(userRepository, _jwtService, _publishMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ──────────────────────────────────────────────
        // RegisterAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task RegisterAsync_ValidUser_ReturnsAuthResponse()
        {
            var dto = new RegisterDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "password123",
                Role = "User"
            };

            var result = await _authService.RegisterAsync(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("john@example.com"));
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Message, Is.EqualTo("User registered successfully"));
        }

        [Test]
        public async Task RegisterAsync_NormalizesEmailToLowercase()
        {
            var dto = new RegisterDto
            {
                FullName = "John Doe",
                Email = "JOHN@EXAMPLE.COM",
                Password = "password123",
                Role = "User"
            };

            var result = await _authService.RegisterAsync(dto);

            Assert.That(result.Email, Is.EqualTo("john@example.com"));
        }

        [Test]
        public async Task RegisterAsync_WithHotelManagerRole_AssignsHotelManagerRole()
        {
            var dto = new RegisterDto
            {
                FullName = "Hotel Manager",
                Email = "manager@hotel.com",
                Password = "password123",
                Role = "HotelManager"
            };

            var result = await _authService.RegisterAsync(dto);

            Assert.That(result.Role, Is.EqualTo("HotelManager"));
        }

        [Test]
        public async Task RegisterAsync_WithInvalidRole_DefaultsToUser()
        {
            var dto = new RegisterDto
            {
                FullName = "Admin Hacker",
                Email = "hacker@example.com",
                Password = "password123",
                Role = "Admin"
            };

            var result = await _authService.RegisterAsync(dto);

            Assert.That(result.Role, Is.EqualTo("User"));
        }

        [Test]
        public async Task RegisterAsync_DuplicateEmail_ThrowsUserAlreadyExistsException()
        {
            var dto = new RegisterDto
            {
                FullName = "Alice",
                Email = "alice@example.com",
                Password = "password123",
                Role = "User"
            };

            await _authService.RegisterAsync(dto);

            Assert.ThrowsAsync<UserAlreadyExistsException>(() => _authService.RegisterAsync(dto));
        }

        [Test]
        public async Task RegisterAsync_PublishesUserRegisteredEvent()
        {
            var dto = new RegisterDto
            {
                FullName = "Bob Smith",
                Email = "bob@example.com",
                Password = "password123",
                Role = "User"
            };

            await _authService.RegisterAsync(dto);

            _publishMock.Verify(p => p.Publish(
                It.IsAny<UserRegisteredEvent>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ──────────────────────────────────────────────
        // LoginAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange – register first
            await _authService.RegisterAsync(new RegisterDto
            {
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Password = "password123",
                Role = "User"
            });

            var loginDto = new LoginDto { Email = "jane@example.com", Password = "password123" };

            var result = await _authService.LoginAsync(loginDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("jane@example.com"));
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Message, Is.EqualTo("Login successful"));
        }

        [Test]
        public async Task LoginAsync_EmailNormalized_FindsUserCaseInsensitively()
        {
            await _authService.RegisterAsync(new RegisterDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "password123",
                Role = "User"
            });

            var result = await _authService.LoginAsync(new LoginDto
            {
                Email = "TEST@EXAMPLE.COM",
                Password = "password123"
            });

            Assert.That(result.Email, Is.EqualTo("test@example.com"));
        }

        [Test]
        public void LoginAsync_NonExistentEmail_ThrowsUserNotFoundException()
        {
            var loginDto = new LoginDto { Email = "nobody@example.com", Password = "password123" };

            Assert.ThrowsAsync<UserNotFoundException>(() => _authService.LoginAsync(loginDto));
        }

        [Test]
        public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
        {
            await _authService.RegisterAsync(new RegisterDto
            {
                FullName = "Alice",
                Email = "alice2@example.com",
                Password = "correctpassword",
                Role = "User"
            });

            Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(
                new LoginDto { Email = "alice2@example.com", Password = "wrongpassword" }));
        }

        [Test]
        public async Task LoginAsync_PublishesUserLoggedInEvent()
        {
            await _authService.RegisterAsync(new RegisterDto
            {
                FullName = "Event User",
                Email = "event@example.com",
                Password = "password123",
                Role = "User"
            });

            await _authService.LoginAsync(new LoginDto { Email = "event@example.com", Password = "password123" });

            _publishMock.Verify(p => p.Publish(
                It.IsAny<UserLoggedInEvent>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ──────────────────────────────────────────────
        // VerifyOtpAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task VerifyOtpAsync_ValidOtp_ReturnsTrue()
        {
            var email = "otp@example.com";
            var user = new User
            {
                FullName = "OTP User",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                Role = "User",
                ResetOtp = "123456",
                ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _authService.VerifyOtpAsync(new VerifyOtpDto { Email = email, Otp = "123456" });

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task VerifyOtpAsync_WrongOtp_ReturnsFalse()
        {
            var email = "otp2@example.com";
            var user = new User
            {
                FullName = "OTP User 2",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                Role = "User",
                ResetOtp = "123456",
                ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _authService.VerifyOtpAsync(new VerifyOtpDto { Email = email, Otp = "999999" });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task VerifyOtpAsync_ExpiredOtp_ReturnsFalse()
        {
            var email = "otp3@example.com";
            var user = new User
            {
                FullName = "Expired OTP User",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                Role = "User",
                ResetOtp = "123456",
                ResetOtpExpiry = DateTime.UtcNow.AddMinutes(-1) // Already expired
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _authService.VerifyOtpAsync(new VerifyOtpDto { Email = email, Otp = "123456" });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task VerifyOtpAsync_NonExistentUser_ReturnsFalse()
        {
            var result = await _authService.VerifyOtpAsync(
                new VerifyOtpDto { Email = "ghost@example.com", Otp = "123456" });

            Assert.That(result, Is.False);
        }

        // ──────────────────────────────────────────────
        // ResetPasswordAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task ResetPasswordAsync_ValidOtp_UpdatesPassword()
        {
            var email = "reset@example.com";
            var user = new User
            {
                FullName = "Reset User",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword"),
                Role = "User",
                ResetOtp = "654321",
                ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _authService.ResetPasswordAsync(new ResetPasswordDto
            {
                Email = email,
                Otp = "654321",
                NewPassword = "newpassword123"
            });

            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            Assert.That(BCrypt.Net.BCrypt.Verify("newpassword123", updatedUser!.PasswordHash), Is.True);
            Assert.That(updatedUser.ResetOtp, Is.Null);
            Assert.That(updatedUser.ResetOtpExpiry, Is.Null);
        }

        [Test]
        public async Task ResetPasswordAsync_InvalidOtp_ThrowsException()
        {
            var email = "reset2@example.com";
            var user = new User
            {
                FullName = "Reset User 2",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword"),
                Role = "User",
                ResetOtp = "654321",
                ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<Exception>(() => _authService.ResetPasswordAsync(new ResetPasswordDto
            {
                Email = email,
                Otp = "000000",
                NewPassword = "newpassword"
            }));
        }

        // ──────────────────────────────────────────────
        // GetAllManagersAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetAllManagersAsync_ReturnsOnlyHotelManagers()
        {
            _context.Users.AddRange(
                new User { FullName = "Manager A", Email = "mgra@h.com", PasswordHash = "x", Role = "HotelManager" },
                new User { FullName = "Manager B", Email = "mgrb@h.com", PasswordHash = "x", Role = "HotelManager" },
                new User { FullName = "Regular User", Email = "user@h.com", PasswordHash = "x", Role = "User" }
            );
            await _context.SaveChangesAsync();

            var managers = await _authService.GetAllManagersAsync();

            Assert.That(managers.Count(), Is.EqualTo(2));
            Assert.That(managers.All(m => m.Role == "HotelManager"), Is.True);
        }

        [Test]
        public async Task GetAllManagersAsync_NoManagers_ReturnsEmptyList()
        {
            _context.Users.Add(new User { FullName = "Regular", Email = "reg@h.com", PasswordHash = "x", Role = "User" });
            await _context.SaveChangesAsync();

            var managers = await _authService.GetAllManagersAsync();

            Assert.That(managers, Is.Empty);
        }
    }
}
