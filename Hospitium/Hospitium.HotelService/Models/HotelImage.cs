using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospitium.HotelService.Models
{
    public class HotelImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public int HotelId { get; set; }

        [ForeignKey("HotelId")]
        public Hotel? Hotel { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }
    }
}
