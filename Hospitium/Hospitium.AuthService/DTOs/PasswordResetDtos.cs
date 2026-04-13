namespace Hospitium.AuthService.DTOs
{
    /// <summary>
    /// Data Transfer Object for requesting a password reset.
    /// </summary>
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for verifying an OTP during password reset.
    /// </summary>
    public class VerifyOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for resetting the password after OTP verification.
    /// </summary>
    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
