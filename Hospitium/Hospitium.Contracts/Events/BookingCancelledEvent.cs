namespace Hospitium.Contracts.Events
{
    public class BookingCancelledEvent
    {
        public int BookingId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime CancelledAt { get; set; }
    }
}