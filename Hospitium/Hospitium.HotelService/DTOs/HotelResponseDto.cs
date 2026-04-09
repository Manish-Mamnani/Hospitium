namespace Hospitium.HotelService.DTOs
{
    public class HotelResponseDto
    {
        public int HotelId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public double Rating { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ManagerEmail { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal MinPrice { get; set; }

        public List<HotelImageResponseDto> Images { get; set; } = new();
    }
}
