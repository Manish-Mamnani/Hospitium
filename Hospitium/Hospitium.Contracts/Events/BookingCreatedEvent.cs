namespace Hospitium.Contracts.Events
{
    public class BookingCreatedEvent
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int NumberOfRooms { get; set; }
        public string UserEmail { get; set; } = string.Empty;
    }
}