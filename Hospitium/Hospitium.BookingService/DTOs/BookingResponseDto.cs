namespace Hospitium.BookingService.DTOs
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }

        public int RoomId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}