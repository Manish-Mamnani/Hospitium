namespace Hospitium.Contracts.Events
{
    /// <summary>
    /// Represents an event triggered when a hotel is approved by an admin.
    /// </summary>
    public class HotelApprovedEvent
    {
        public int HotelId { get; set; }
        public string ManagerEmail { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
    }
}