namespace Hospitium.ReviewService.DTOs
{
    /// <summary>
    /// Data Transfer Object for returning review data in API responses.
    /// </summary>
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

    /// <summary>
    /// Data Transfer Object for creating a new review submission.
    /// </summary>
    public class CreateReviewDto
    {
        public int HotelId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
