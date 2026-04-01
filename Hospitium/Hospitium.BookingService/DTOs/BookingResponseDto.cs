namespace Hospitium.BookingService.DTOs
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }

        public int RoomId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
        
        public string HotelName { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int NumberOfRooms { get; set; }

        public decimal TotalPrice { get; set; }
    }
}