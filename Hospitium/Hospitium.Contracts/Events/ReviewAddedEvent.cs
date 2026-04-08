namespace Hospitium.Contracts.Events
{
    public class ReviewAddedEvent
    {
        public int HotelId { get; set; }
        public double Rating { get; set; }
    }
}
