using Hospitium.HotelService.Data;
using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Models;
using Hospitium.HotelService.Services;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.Testing.HotelService
{
    [TestFixture]
    public class HotelQueryServiceTests
    {
        private HotelDbContext _context = null!;
        private HotelQueryService _queryService = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new HotelDbContext(options);
            _queryService = new HotelQueryService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private async Task SeedHotels()
        {
            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Name = "Alpha Suites", City = "Karachi", Description = "City suites",
                    Status = "Approved", AverageRating = 4.5, TotalReviews = 10,
                    CreatedByUserId = 1, ManagerEmail = "a@m.com",
                    Rooms = new List<Room>
                    {
                        new Room { Type = "Deluxe", Price = 150m, TotalCount = 5, AvailableCount = 3 }
                    }
                },
                new Hotel
                {
                    Name = "Beta Lodge", City = "Lahore", Description = "Budget lodge",
                    Status = "Approved", AverageRating = 3.2, TotalReviews = 5,
                    CreatedByUserId = 2, ManagerEmail = "b@m.com",
                    Rooms = new List<Room>
                    {
                        new Room { Type = "Standard", Price = 80m, TotalCount = 10, AvailableCount = 0 }
                    }
                },
                new Hotel
                {
                    Name = "Gamma Resort", City = "Islamabad", Description = "Premium resort",
                    Status = "Approved", AverageRating = 4.8, TotalReviews = 20,
                    CreatedByUserId = 3, ManagerEmail = "c@m.com",
                    Rooms = new List<Room>
                    {
                        new Room { Type = "Suite", Price = 300m, TotalCount = 2, AvailableCount = 2 }
                    }
                },
                new Hotel
                {
                    Name = "Delta Inn", City = "Karachi", Description = "Pending hotel",
                    Status = "Pending", AverageRating = 0, TotalReviews = 0,
                    CreatedByUserId = 4, ManagerEmail = "d@m.com"
                }
            };

            _context.Hotels.AddRange(hotels);
            await _context.SaveChangesAsync();
        }

        // ──────────────────────────────────────────────
        // Filtering Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetHotelsAsync_NoFilters_ReturnsOnlyApproved()
        {
            await SeedHotels();

            var result = await _queryService.GetHotelsAsync(new HotelQueryParams { Page = 1, PageSize = 10 });

            Assert.That(result.Data.All(h => h.Status == "Approved"), Is.True);
            Assert.That(result.Data.Count, Is.EqualTo(3)); // Delta Inn is Pending, so excluded
        }

        [Test]
        public async Task GetHotelsAsync_SearchByName_ReturnsMatchingHotels()
        {
            await SeedHotels();

            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                Search = "alpha",
                Page = 1, PageSize = 10
            });

            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Alpha Suites"));
        }

        [Test]
        public async Task GetHotelsAsync_FilterByCity_ReturnsMatchingHotels()
        {
            await SeedHotels();

            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                City = "karachi",
                Page = 1, PageSize = 10
            });

            // Only Alpha Suites is approved in Karachi (Delta Inn is Pending)
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data[0].City, Is.EqualTo("Karachi"));
        }

        [Test]
        public async Task GetHotelsAsync_FilterByMinPrice_ExcludesCheaperHotels()
        {
            await SeedHotels();

            // Only Gamma Resort has price >= 200
            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                MinPrice = 200m,
                Page = 1, PageSize = 10
            });

            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Gamma Resort"));
        }

        [Test]
        public async Task GetHotelsAsync_FilterByMaxPrice_ExcludesExpensiveHotels()
        {
            await SeedHotels();

            // Alpha Suites (150) and Beta Lodge (80) are <= 200
            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                MaxPrice = 200m,
                Page = 1, PageSize = 10
            });

            Assert.That(result.Data.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetHotelsAsync_AvailableOnly_ExcludesFullyBookedHotels()
        {
            await SeedHotels();

            // Beta Lodge has AvailableCount = 0, so should be excluded
            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                AvailableOnly = true,
                Page = 1, PageSize = 10
            });

            Assert.That(result.Data.All(h => h.Name != "Beta Lodge"), Is.True);
        }

        // ──────────────────────────────────────────────
        // Pagination Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetHotelsAsync_Pagination_ReturnsCorrectPage()
        {
            await SeedHotels();

            var page1 = await _queryService.GetHotelsAsync(new HotelQueryParams { Page = 1, PageSize = 2 });
            var page2 = await _queryService.GetHotelsAsync(new HotelQueryParams { Page = 2, PageSize = 2 });

            Assert.That(page1.Data.Count, Is.EqualTo(2));
            Assert.That(page2.Data.Count, Is.EqualTo(1)); // 3 approved, 2 on page 1, 1 on page 2
            Assert.That(page1.TotalCount, Is.EqualTo(3));
        }

        [Test]
        public async Task GetHotelsAsync_Pagination_PageSizeRespected()
        {
            await SeedHotels();

            var result = await _queryService.GetHotelsAsync(new HotelQueryParams { Page = 1, PageSize = 1 });

            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(1));
        }

        // ──────────────────────────────────────────────
        // Sorting Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetHotelsAsync_SortByNameAsc_ReturnsSortedAlphabetically()
        {
            await SeedHotels();

            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                SortBy = "name",
                Order = "asc",
                Page = 1, PageSize = 10
            });

            var names = result.Data.Select(h => h.Name).ToList();
            var sorted = names.OrderBy(n => n).ToList();

            Assert.That(names, Is.EqualTo(sorted));
        }

        [Test]
        public async Task GetHotelsAsync_SortByRatingDesc_HighestRatedFirst()
        {
            await SeedHotels();

            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                SortBy = "rating",
                Order = "desc",
                Page = 1, PageSize = 10
            });

            Assert.That(result.Data[0].Name, Is.EqualTo("Gamma Resort")); // Rating 4.8
        }

        [Test]
        public async Task GetHotelsAsync_SortByPriceAsc_CheapestFirst()
        {
            await SeedHotels();

            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                SortBy = "price",
                Order = "asc",
                Page = 1, PageSize = 10
            });

            // Beta Lodge (80) < Alpha Suites (150) < Gamma Resort (300)
            Assert.That(result.Data[0].Name, Is.EqualTo("Beta Lodge"));
        }

        // ──────────────────────────────────────────────
        // Edge Case Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetHotelsAsync_EmptyDatabase_ReturnsEmptyResult()
        {
            var result = await _queryService.GetHotelsAsync(new HotelQueryParams { Page = 1, PageSize = 10 });

            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public async Task GetHotelsAsync_NoMatchingSearch_ReturnsEmptyResult()
        {
            await SeedHotels();

            var result = await _queryService.GetHotelsAsync(new HotelQueryParams
            {
                Search = "XXXXXXXXXX",
                Page = 1, PageSize = 10
            });

            Assert.That(result.Data, Is.Empty);
        }
    }
}
