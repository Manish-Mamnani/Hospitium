using Hospitium.ReviewService.DTOs;

namespace Hospitium.ReviewService.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> AddReviewAsync(int userId, string userName, CreateReviewDto dto);
        Task<List<ReviewDto>> GetReviewsByHotelIdAsync(int hotelId);
        Task<ReviewDto> AddRatingAsync(int userId, string userName, int hotelId, int rating);
    }
}
