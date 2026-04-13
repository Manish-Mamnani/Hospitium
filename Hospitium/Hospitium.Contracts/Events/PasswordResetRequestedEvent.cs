namespace Hospitium.Contracts.Events
{
    /// <summary>
    /// Represents an event triggered when a password reset is requested.
    /// </summary>
    public class PasswordResetRequestedEvent
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
