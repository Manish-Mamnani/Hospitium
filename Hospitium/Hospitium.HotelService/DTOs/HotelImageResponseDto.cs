namespace Hospitium.HotelService.DTOs
{
    public class HotelImageResponseDto
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
