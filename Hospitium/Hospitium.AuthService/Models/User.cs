using System.ComponentModel.DataAnnotations;

namespace Hospitium.AuthService.Models
{
    /// <summary>
    /// Represents a user entity within the authentication system.
    /// </summary>
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(6)]
        public string? ResetOtp { get; set; }

        public DateTime? ResetOtpExpiry { get; set; }
    }
}
