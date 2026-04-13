using System.ComponentModel.DataAnnotations;

namespace Hospitium.HotelService.Models
{
    /// <summary>
    /// Represents a room type within a hotel, including pricing and availability tracking.
    /// </summary>
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        public int HotelId { get; set; }

        public string Type { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int TotalCount { get; set; }

        public int AvailableCount { get; set; }

        public Hotel? Hotel { get; set; }
    }
}
