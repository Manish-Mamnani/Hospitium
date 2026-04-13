namespace Hospitium.Contracts.Events
{
    /// <summary>
    /// Represents an event triggered when a booking is cancelled.
    /// </summary>
    public class BookingCancelledEvent
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int NumberOfRooms { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal DeductionAmount { get; set; }
        public DateTime CancelledAt { get; set; }
    }
}