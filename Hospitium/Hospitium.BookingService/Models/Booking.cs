using System.ComponentModel.DataAnnotations;

namespace Hospitium.BookingService.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        public int UserId { get; set; }

        public int RoomId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Status { get; set; } = "Confirmed";

        public string UserEmail { get; set; } = string.Empty;

        public int NumberOfRooms { get; set; } = 1;
        
        public decimal TotalPrice { get; set; }
        
        public decimal? CancellationDeduction { get; set; }
        public decimal? RefundAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}