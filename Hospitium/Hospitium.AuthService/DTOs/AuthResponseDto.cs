namespace Hospitium.AuthService.DTOs
{
    /// <summary>
    /// Data Transfer Object for authentication responses, containing the JWT and user info.
    /// </summary>
    public class AuthResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
