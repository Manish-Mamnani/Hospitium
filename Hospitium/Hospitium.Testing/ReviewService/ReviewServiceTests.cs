using Hospitium.Contracts.Events;
using Hospitium.ReviewService.Data;
using Hospitium.ReviewService.DTOs;
using Hospitium.ReviewService.Models;
using Hospitium.ReviewService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hospitium.Testing.ReviewService
{
    /// <summary>
    /// Unit tests for the <see cref="Hospitium.ReviewService.Services.ReviewService"/> class,
    /// covering review and rating submission, event publishing, and retrieval with ordering.
    /// </summary>
    [TestFixture]
    public class ReviewServiceTests
    {
        private ReviewDbContext _context = null!;
        private Mock<IPublishEndpoint> _publishMock = null!;
        private Hospitium.ReviewService.Services.ReviewService _reviewService = null!;

        private const int DefaultUserId = 1;
        private const string DefaultUserName = "Test User";

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ReviewDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ReviewDbContext(options);
            _publishMock = new Mock<IPublishEndpoint>();
            _publishMock
                .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _reviewService = new Hospitium.ReviewService.Services.ReviewService(_context, _publishMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ──────────────────────────────────────────────
        // AddReviewAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task AddReviewAsync_ValidInput_ReturnsReviewDto()
        {
            var dto = new CreateReviewDto { HotelId = 1, Rating = 5, Comment = "Excellent stay!" };

            var result = await _reviewService.AddReviewAsync(DefaultUserId, DefaultUserName, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.HotelId, Is.EqualTo(1));
            Assert.That(result.Rating, Is.EqualTo(5));
            Assert.That(result.Comment, Is.EqualTo("Excellent stay!"));
            Assert.That(result.UserId, Is.EqualTo(DefaultUserId));
            Assert.That(result.UserName, Is.EqualTo(DefaultUserName));
        }

        [Test]
        public async Task AddReviewAsync_PersistsReviewInDatabase()
        {
            var dto = new CreateReviewDto { HotelId = 2, Rating = 4, Comment = "Very good!" };

            await _reviewService.AddReviewAsync(DefaultUserId, DefaultUserName, dto);

            var count = await _context.Reviews.CountAsync();
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddReviewAsync_PublishesReviewAddedEvent()
        {
            var dto = new CreateReviewDto { HotelId = 3, Rating = 3, Comment = "Average stay." };

            await _reviewService.AddReviewAsync(DefaultUserId, DefaultUserName, dto);

            _publishMock.Verify(p => p.Publish(
                It.Is<ReviewAddedEvent>(e => e.HotelId == 3 && e.Rating == 3),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task AddReviewAsync_SetsCreatedAtToUtcNow()
        {
            var before = DateTime.UtcNow;
            var dto = new CreateReviewDto { HotelId = 1, Rating = 4, Comment = "Nice place" };

            var result = await _reviewService.AddReviewAsync(DefaultUserId, DefaultUserName, dto);

            Assert.That(result.CreatedAt, Is.GreaterThanOrEqualTo(before));
            Assert.That(result.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
        }

        [Test]
        public async Task AddReviewAsync_MultipleReviews_AllPersistedSeparately()
        {
            var dto1 = new CreateReviewDto { HotelId = 1, Rating = 5, Comment = "Amazing!" };
            var dto2 = new CreateReviewDto { HotelId = 1, Rating = 2, Comment = "Not great." };

            await _reviewService.AddReviewAsync(DefaultUserId, DefaultUserName, dto1);
            await _reviewService.AddReviewAsync(2, "Another User", dto2);

            var count = await _context.Reviews.CountAsync();
            Assert.That(count, Is.EqualTo(2));
        }

        // ──────────────────────────────────────────────
        // AddRatingAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task AddRatingAsync_ValidInput_ReturnsReviewDto()
        {
            var result = await _reviewService.AddRatingAsync(DefaultUserId, DefaultUserName, hotelId: 5, rating: 4);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.HotelId, Is.EqualTo(5));
            Assert.That(result.Rating, Is.EqualTo(4));
            Assert.That(result.Comment, Is.EqualTo("Rating only"));
        }

        [Test]
        public async Task AddRatingAsync_PublishesReviewAddedEvent()
        {
            await _reviewService.AddRatingAsync(DefaultUserId, DefaultUserName, hotelId: 7, rating: 5);

            _publishMock.Verify(p => p.Publish(
                It.Is<ReviewAddedEvent>(e => e.HotelId == 7 && e.Rating == 5),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task AddRatingAsync_PersistsRatingWithDefaultComment()
        {
            await _reviewService.AddRatingAsync(DefaultUserId, DefaultUserName, hotelId: 1, rating: 3);

            var review = await _context.Reviews.FirstOrDefaultAsync();
            Assert.That(review, Is.Not.Null);
            Assert.That(review!.Comment, Is.EqualTo("Rating only"));
        }

        // ──────────────────────────────────────────────
        // GetReviewsByHotelIdAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetReviewsByHotelIdAsync_ReturnsOnlyMatchingHotelReviews()
        {
            _context.Reviews.AddRange(
                new Review { HotelId = 1, UserId = 1, UserName = "Alice", Rating = 5, Comment = "Great", CreatedAt = DateTime.UtcNow },
                new Review { HotelId = 1, UserId = 2, UserName = "Bob", Rating = 3, Comment = "Ok", CreatedAt = DateTime.UtcNow },
                new Review { HotelId = 2, UserId = 3, UserName = "Charlie", Rating = 4, Comment = "Good", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var result = await _reviewService.GetReviewsByHotelIdAsync(1);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(r => r.HotelId == 1), Is.True);
        }

        [Test]
        public async Task GetReviewsByHotelIdAsync_NoReviews_ReturnsEmptyList()
        {
            var result = await _reviewService.GetReviewsByHotelIdAsync(999);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetReviewsByHotelIdAsync_OrderedByCreatedAtDescending()
        {
            var older = new Review { HotelId = 1, UserId = 1, UserName = "Alice", Rating = 4, Comment = "Old", CreatedAt = DateTime.UtcNow.AddDays(-2) };
            var newer = new Review { HotelId = 1, UserId = 2, UserName = "Bob", Rating = 5, Comment = "New", CreatedAt = DateTime.UtcNow };

            _context.Reviews.AddRange(older, newer);
            await _context.SaveChangesAsync();

            var result = await _reviewService.GetReviewsByHotelIdAsync(1);

            // Most recent should be first
            Assert.That(result[0].Comment, Is.EqualTo("New"));
            Assert.That(result[1].Comment, Is.EqualTo("Old"));
        }

        [Test]
        public async Task GetReviewsByHotelIdAsync_ContainsAllExpectedFields()
        {
            _context.Reviews.Add(new Review
            {
                HotelId = 10,
                UserId = 5,
                UserName = "Review User",
                Rating = 4,
                Comment = "Nice hotel!",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var result = await _reviewService.GetReviewsByHotelIdAsync(10);

            var review = result[0];
            Assert.That(review.ReviewId, Is.GreaterThan(0));
            Assert.That(review.HotelId, Is.EqualTo(10));
            Assert.That(review.UserId, Is.EqualTo(5));
            Assert.That(review.UserName, Is.EqualTo("Review User"));
            Assert.That(review.Rating, Is.EqualTo(4));
            Assert.That(review.Comment, Is.EqualTo("Nice hotel!"));
        }

        [Test]
        public async Task AddReviewAsync_RatingValues_StoredCorrectly(
            [Values(1, 2, 3, 4, 5)] int rating)
        {
            var dto = new CreateReviewDto { HotelId = 1, Rating = rating, Comment = "Test" };

            var result = await _reviewService.AddReviewAsync(DefaultUserId, DefaultUserName, dto);

            Assert.That(result.Rating, Is.EqualTo(rating));
        }
    }
}
