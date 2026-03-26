namespace Hospitium.Contracts.Events
{
    public class UserLoggedInEvent
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
    }
}