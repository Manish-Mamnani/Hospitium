namespace Hospitium.Contracts.Events
{
    /// <summary>
    /// Represents an event triggered when a new user registers.
    /// </summary>
    public class UserRegisteredEvent
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}