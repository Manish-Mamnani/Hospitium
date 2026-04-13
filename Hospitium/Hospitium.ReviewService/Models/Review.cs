using System.ComponentModel.DataAnnotations;

namespace Hospitium.ReviewService.Models
{
    /// <summary>
    /// Represents a review entity submitted by a user for a hotel.
    /// </summary>
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int HotelId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
