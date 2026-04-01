namespace Hospitium.HotelService.DTOs
{
    public class RoomResponseDto
    {
        public int RoomId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableCount { get; set; }
    }
}