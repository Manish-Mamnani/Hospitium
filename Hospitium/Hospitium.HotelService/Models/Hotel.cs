using System.ComponentModel.DataAnnotations;

namespace Hospitium.HotelService.Models
{
    public class Hotel
    {
        [Key]
        public int HotelId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public List<Room> Rooms { get; set; } = new();
    }
}
