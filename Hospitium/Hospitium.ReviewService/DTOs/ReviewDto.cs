namespace Hospitium.ReviewService.DTOs
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int HotelId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReviewDto
    {
        public int HotelId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
