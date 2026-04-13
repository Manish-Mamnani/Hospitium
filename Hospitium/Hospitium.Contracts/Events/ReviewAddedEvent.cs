namespace Hospitium.Contracts.Events
{
    /// <summary>
    /// Represents an event triggered when a new review is added for a hotel.
    /// </summary>
    public class ReviewAddedEvent
    {
        public int HotelId { get; set; }
        public double Rating { get; set; }
    }
}
