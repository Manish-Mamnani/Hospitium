using Hospitium.Contracts.Events;
using Hospitium.ReviewService.Data;
using Hospitium.ReviewService.DTOs;
using Hospitium.ReviewService.Models;
using Hospitium.ReviewService.Services.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.ReviewService.Services
{
    /// <summary>
    /// Service implementation for managing hotel reviews and ratings, persisting data and publishing events to update hotel aggregate ratings.
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly ReviewDbContext _context;
        private readonly IPublishEndpoint _publish;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewService"/> class.
        /// </summary>
        /// <param name="context">The review database context.</param>
        /// <param name="publish">The MassTransit publish endpoint for event publishing.</param>
        public ReviewService(ReviewDbContext context, IPublishEndpoint publish)
        {
            _context = context;
            _publish = publish;
        }

        public async Task<ReviewDto> AddReviewAsync(int userId, string userName, CreateReviewDto dto)
        {
            var review = new Review
            {
                HotelId = dto.HotelId,
                UserId = userId,
                UserName = userName,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Publish event to update hotel rating
            await _publish.Publish(new ReviewAddedEvent
            {
                HotelId = dto.HotelId,
                Rating = dto.Rating
            });

            return MapToDto(review);
        }

        public async Task<ReviewDto> AddRatingAsync(int userId, string userName, int hotelId, int rating)
        {
            var review = new Review
            {
                HotelId = hotelId,
                UserId = userId,
                UserName = userName,
                Rating = rating,
                Comment = "Rating only",
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Publish event to update hotel rating
            await _publish.Publish(new ReviewAddedEvent
            {
                HotelId = hotelId,
                Rating = rating
            });

            return MapToDto(review);
        }

        public async Task<List<ReviewDto>> GetReviewsByHotelIdAsync(int hotelId)
        {
            return await _context.Reviews
                .Where(r => r.HotelId == hotelId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToDto(r))
                .ToListAsync();
        }

        private static ReviewDto MapToDto(Review r)
        {
            return new ReviewDto
            {
                ReviewId = r.ReviewId,
                HotelId = r.HotelId,
                UserId = r.UserId,
                UserName = r.UserName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            };
        }
    }
}
