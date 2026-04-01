namespace Hospitium.Contracts.Events
{
    public class BookingCancelledEvent
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int NumberOfRooms { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public DateTime CancelledAt { get; set; }
    }
}