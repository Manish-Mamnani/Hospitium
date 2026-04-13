namespace Hospitium.BookingService.DTOs
{
    /// <summary>
    /// Data Transfer Object for returning booking details in API responses.
    /// </summary>
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

        public decimal? CancellationDeduction { get; set; }

        public decimal? RefundAmount { get; set; }

        public int NumberOfRooms { get; set; }

        public decimal TotalPrice { get; set; }
    }
}