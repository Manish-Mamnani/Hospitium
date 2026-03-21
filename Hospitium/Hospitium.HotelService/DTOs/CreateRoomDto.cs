using System.ComponentModel.DataAnnotations;

namespace Hospitium.HotelService.DTOs
{
    public class CreateRoomDto
    {
        [Required]
        public int HotelId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Single, Double, Deluxe, etc.

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalCount { get; set; }
    }
}