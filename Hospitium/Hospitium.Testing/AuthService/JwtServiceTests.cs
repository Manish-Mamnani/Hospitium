using Hospitium.AuthService.Models;
using Hospitium.AuthService.Services;
using Microsoft.Extensions.Configuration;

namespace Hospitium.Testing.AuthService
{
    /// <summary>
    /// Unit tests for the <see cref="JwtService"/> class, verifying JWT token generation,
    /// claims content, expiry, and format validity.
    /// </summary>
    [TestFixture]
    public class JwtServiceTests
    {
        private JwtService _jwtService = null!;

        [SetUp]
        public void SetUp()
        {
            var inMemoryConfig = new Dictionary<string, string?>
            {
                { "Jwt:Key", "super-secret-key-for-hospitium-testing-1234567890!" },
                { "Jwt:Issuer", "Hospitium" },
                { "Jwt:Audience", "HospitiumUsers" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfig)
                .Build();

            _jwtService = new JwtService(configuration);
        }

        [Test]
        public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
        {
            var user = new User
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@hospitium.com",
                Role = "User",
                PasswordHash = "hashed"
            };

            var token = _jwtService.GenerateToken(user);

            Assert.That(token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GenerateToken_TokenIsValidJwtFormat()
        {
            var user = new User
            {
                UserId = 42,
                FullName = "JWT User",
                Email = "jwt@hospitium.com",
                Role = "HotelManager",
                PasswordHash = "hashed"
            };

            var token = _jwtService.GenerateToken(user);

            // A valid JWT has exactly 3 dot-separated segments
            var parts = token.Split('.');
            Assert.That(parts.Length, Is.EqualTo(3));
        }

        [Test]
        public void GenerateToken_DifferentUsersProduceDifferentTokens()
        {
            var user1 = new User { UserId = 1, FullName = "Alice", Email = "alice@h.com", Role = "User", PasswordHash = "h" };
            var user2 = new User { UserId = 2, FullName = "Bob", Email = "bob@h.com", Role = "User", PasswordHash = "h" };

            var token1 = _jwtService.GenerateToken(user1);
            var token2 = _jwtService.GenerateToken(user2);

            Assert.That(token1, Is.Not.EqualTo(token2));
        }

        [Test]
        public void GenerateToken_UserWithNullFullName_DoesNotThrow()
        {
            var user = new User
            {
                UserId = 99,
                FullName = null!,
                Email = "nullname@h.com",
                Role = "User",
                PasswordHash = "h"
            };

            Assert.DoesNotThrow(() => _jwtService.GenerateToken(user));
        }

        [Test]
        public void GenerateToken_AdminRole_ReturnsTokenWithAdminClaim()
        {
            var user = new User
            {
                UserId = 5,
                FullName = "Admin",
                Email = "admin@h.com",
                Role = "Admin",
                PasswordHash = "h"
            };

            var token = _jwtService.GenerateToken(user);

            // Decode the payload and verify the role claim is present
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "Role");
            Assert.That(roleClaim, Is.Not.Null);
            Assert.That(roleClaim!.Value, Is.EqualTo("Admin"));
        }

        [Test]
        public void GenerateToken_ContainsCorrectEmailClaim()
        {
            var user = new User
            {
                UserId = 7,
                FullName = "Email Check",
                Email = "emailcheck@h.com",
                Role = "User",
                PasswordHash = "h"
            };

            var token = _jwtService.GenerateToken(user);

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == "Email");
            Assert.That(emailClaim, Is.Not.Null);
            Assert.That(emailClaim!.Value, Is.EqualTo("emailcheck@h.com"));
        }

        [Test]
        public void GenerateToken_TokenExpiresInApproxTwoHours()
        {
            var user = new User
            {
                UserId = 10,
                FullName = "Expiry Test",
                Email = "expiry@h.com",
                Role = "User",
                PasswordHash = "h"
            };

            var before = DateTime.UtcNow;
            var token = _jwtService.GenerateToken(user);
            var after = DateTime.UtcNow;

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var expectedMin = before.AddHours(2).AddSeconds(-5);
            var expectedMax = after.AddHours(2).AddSeconds(5);

            Assert.That(jwt.ValidTo, Is.InRange(expectedMin, expectedMax));
        }
    }
}
