using System.ComponentModel.DataAnnotations;

namespace Hospitium.AuthService.DTOs
{
    /// <summary>
    /// Data Transfer Object for user registration requests.
    /// </summary>
    public class RegisterDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User"; // Will accept "User" or "HotelManager"
    }
}
