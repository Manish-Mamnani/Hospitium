namespace Hospitium.Contracts.Events
{
    public class UserRegisteredEvent
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}