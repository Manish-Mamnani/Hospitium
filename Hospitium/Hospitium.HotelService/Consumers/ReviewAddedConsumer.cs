using Hospitium.Contracts.Events;
using Hospitium.HotelService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.HotelService.Consumers
{
    /// <summary>
    /// MassTransit consumer that handles <see cref="ReviewAddedEvent"/> events by updating the hotel's average rating and review count.
    /// </summary>
    public class ReviewAddedConsumer : IConsumer<ReviewAddedEvent>
    {
        private readonly HotelDbContext _dbContext;
        private readonly ILogger<ReviewAddedConsumer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewAddedConsumer"/> class.
        /// </summary>
        /// <param name="dbContext">The hotel database context.</param>
        /// <param name="logger">The logger instance.</param>
        public ReviewAddedConsumer(HotelDbContext dbContext, ILogger<ReviewAddedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReviewAddedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing ReviewAddedEvent for HotelId: {HotelId}, Rating: {Rating}", message.HotelId, message.Rating);

            var hotel = await _dbContext.Hotels.FirstOrDefaultAsync(h => h.HotelId == message.HotelId);

            if (hotel == null)
            {
                _logger.LogWarning("Hotel not found for ReviewAddedEvent. HotelId: {HotelId}", message.HotelId);
                return;
            }

            // Update hotel rating and review count
            hotel.TotalReviews++;
            hotel.AverageRating = ((hotel.AverageRating * (hotel.TotalReviews - 1)) + message.Rating) / hotel.TotalReviews;

            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Successfully updated rating for HotelId: {HotelId}. New average: {AverageRating}", hotel.HotelId, hotel.AverageRating);
        }
    }
}
