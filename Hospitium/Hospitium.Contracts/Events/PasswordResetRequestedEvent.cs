namespace Hospitium.Contracts.Events
{
    public class PasswordResetRequestedEvent
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
