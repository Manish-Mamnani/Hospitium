namespace Hospitium.Contracts.Events
{
    /// <summary>
    /// Represents an event triggered when a user logs in.
    /// </summary>
    public class UserLoggedInEvent
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
    }
}