namespace Hospitium.Contracts.Events
{
    /// <summary>
    /// Represents an event triggered when a hotel is rejected by an admin.
    /// </summary>
    public class HotelRejectedEvent
    {
        public int HotelId { get; set; }
        public string ManagerEmail { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
    }
}