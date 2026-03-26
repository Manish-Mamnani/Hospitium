namespace Hospitium.Contracts.Events
{
    public class HotelRejectedEvent
    {
        public int HotelId { get; set; }
        public string ManagerEmail { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
    }
}