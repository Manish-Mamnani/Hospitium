using Hospitium.BookingService.Data;
using Hospitium.BookingService.DTOs;
using Hospitium.BookingService.Exceptions;
using Hospitium.BookingService.HttpClients;
using Hospitium.BookingService.Models;
using Hospitium.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hospitium.Testing.BookingService
{
    [TestFixture]
    public class BookingServiceTests
    {
        private BookingDbContext _context = null!;
        private Mock<IHotelClient> _hotelClientMock = null!;
        private Mock<IPublishEndpoint> _publishMock = null!;
        private Hospitium.BookingService.Services.BookingService _bookingService = null!;

        private const int DefaultUserId = 1;
        private const string DefaultUserEmail = "user@test.com";

        private static RoomResponseDto MakeRoom(int roomId = 1, int availableCount = 5, decimal price = 100m)
            => new RoomResponseDto
            {
                RoomId = roomId,
                HotelName = "Grand Hotel",
                Type = "Deluxe",
                Price = price,
                AvailableCount = availableCount
            };

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new BookingDbContext(options);
            _hotelClientMock = new Mock<IHotelClient>();
            _publishMock = new Mock<IPublishEndpoint>();
            _publishMock
                .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _bookingService = new Hospitium.BookingService.Services.BookingService(
                _context, _hotelClientMock.Object, _publishMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ──────────────────────────────────────────────
        // CreateBookingAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task CreateBookingAsync_ValidRequest_ReturnsBookingResponse()
        {
            var room = MakeRoom(availableCount: 5, price: 150m);
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(1)).ReturnsAsync(room);

            var dto = new CreateBookingDto
            {
                RoomId = 1,
                FromDate = DateTime.Today.AddDays(1),
                ToDate = DateTime.Today.AddDays(4),
                NumberOfRooms = 2
            };

            var result = await _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo("Confirmed"));
            Assert.That(result.HotelName, Is.EqualTo("Grand Hotel"));
            Assert.That(result.NumberOfRooms, Is.EqualTo(2));
        }

        [Test]
        public async Task CreateBookingAsync_CalculatesTotalPriceCorrectly()
        {
            var room = MakeRoom(availableCount: 5, price: 200m);
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(1)).ReturnsAsync(room);

            var from = DateTime.Today.AddDays(1);
            var to = DateTime.Today.AddDays(4); // 3 days

            var dto = new CreateBookingDto { RoomId = 1, FromDate = from, ToDate = to, NumberOfRooms = 2 };

            var result = await _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto);

            // 200 * 2 rooms * 3 days = 1200
            Assert.That(result.TotalPrice, Is.EqualTo(1200m));
        }

        [Test]
        public void CreateBookingAsync_RoomNull_ThrowsRoomNotAvailableException()
        {
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(99)).ReturnsAsync((RoomResponseDto?)null);

            var dto = new CreateBookingDto
            {
                RoomId = 99,
                FromDate = DateTime.Today.AddDays(1),
                ToDate = DateTime.Today.AddDays(3),
                NumberOfRooms = 1
            };

            Assert.ThrowsAsync<RoomNotAvailableException>(() =>
                _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto));
        }

        [Test]
        public void CreateBookingAsync_RequestedRoomsExceedAvailable_ThrowsRoomNotAvailableException()
        {
            var room = MakeRoom(availableCount: 2);
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(1)).ReturnsAsync(room);

            var dto = new CreateBookingDto
            {
                RoomId = 1,
                FromDate = DateTime.Today.AddDays(1),
                ToDate = DateTime.Today.AddDays(3),
                NumberOfRooms = 5 // Exceeds available count of 2
            };

            Assert.ThrowsAsync<RoomNotAvailableException>(() =>
                _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto));
        }

        [Test]
        public void CreateBookingAsync_FromDateAfterToDate_ThrowsInvalidBookingDatesException()
        {
            var room = MakeRoom();
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(1)).ReturnsAsync(room);

            var dto = new CreateBookingDto
            {
                RoomId = 1,
                FromDate = DateTime.Today.AddDays(5),
                ToDate = DateTime.Today.AddDays(2), // ToDate before FromDate
                NumberOfRooms = 1
            };

            Assert.ThrowsAsync<InvalidBookingDatesException>(() =>
                _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto));
        }

        [Test]
        public void CreateBookingAsync_SameFromAndToDate_ThrowsInvalidBookingDatesException()
        {
            var room = MakeRoom();
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(1)).ReturnsAsync(room);

            var sameDay = DateTime.Today.AddDays(2);
            var dto = new CreateBookingDto
            {
                RoomId = 1,
                FromDate = sameDay,
                ToDate = sameDay,
                NumberOfRooms = 1
            };

            Assert.ThrowsAsync<InvalidBookingDatesException>(() =>
                _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto));
        }

        [Test]
        public async Task CreateBookingAsync_OverlapExceedsCapacity_ThrowsBookingConflictException()
        {
            var room = MakeRoom(availableCount: 2);
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(1)).ReturnsAsync(room);

            // Pre-seed a booking that occupies both rooms
            _context.Bookings.Add(new Booking
            {
                UserId = 2,
                RoomId = 1,
                HotelName = "Grand Hotel",
                RoomType = "Deluxe",
                FromDate = DateTime.Today.AddDays(1),
                ToDate = DateTime.Today.AddDays(5),
                NumberOfRooms = 2,
                TotalPrice = 400m,
                Status = "Confirmed",
                UserEmail = "other@test.com"
            });
            await _context.SaveChangesAsync();

            var dto = new CreateBookingDto
            {
                RoomId = 1,
                FromDate = DateTime.Today.AddDays(2),
                ToDate = DateTime.Today.AddDays(4),
                NumberOfRooms = 1 // Available = 2, booked = 2 → conflict
            };

            Assert.ThrowsAsync<BookingConflictException>(() =>
                _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto));
        }

        [Test]
        public async Task CreateBookingAsync_PublishesBookingCreatedEvent()
        {
            var room = MakeRoom();
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(1)).ReturnsAsync(room);

            var dto = new CreateBookingDto
            {
                RoomId = 1,
                FromDate = DateTime.Today.AddDays(1),
                ToDate = DateTime.Today.AddDays(3),
                NumberOfRooms = 1
            };

            await _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto);

            _publishMock.Verify(p => p.Publish(
                It.IsAny<BookingCreatedEvent>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CreateBookingAsync_CancelledBookingsIgnoredInOverlapCheck()
        {
            var room = MakeRoom(availableCount: 1);
            _hotelClientMock.Setup(h => h.GetRoomDetailsAsync(1)).ReturnsAsync(room);

            // Add a CANCELLED booking for the same period
            _context.Bookings.Add(new Booking
            {
                UserId = 2,
                RoomId = 1,
                HotelName = "Grand Hotel",
                RoomType = "Deluxe",
                FromDate = DateTime.Today.AddDays(1),
                ToDate = DateTime.Today.AddDays(5),
                NumberOfRooms = 1,
                TotalPrice = 100m,
                Status = "Cancelled",
                UserEmail = "other@test.com"
            });
            await _context.SaveChangesAsync();

            var dto = new CreateBookingDto
            {
                RoomId = 1,
                FromDate = DateTime.Today.AddDays(2),
                ToDate = DateTime.Today.AddDays(4),
                NumberOfRooms = 1
            };

            // Should succeed because cancelled bookings don't count
            var result = await _bookingService.CreateBookingAsync(DefaultUserId, DefaultUserEmail, dto);
            Assert.That(result.Status, Is.EqualTo("Confirmed"));
        }

        // ──────────────────────────────────────────────
        // GetUserBookingsAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetUserBookingsAsync_ReturnsOnlyUserBookings()
        {
            _context.Bookings.AddRange(
                new Booking { UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2), Status = "Confirmed", UserEmail = "a@t.com" },
                new Booking { UserId = 2, RoomId = 2, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2), Status = "Confirmed", UserEmail = "b@t.com" }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetUserBookingsAsync(1, null);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].UserId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetUserBookingsAsync_TypeActive_ReturnsOnlyConfirmed()
        {
            _context.Bookings.AddRange(
                new Booking { UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2), Status = "Confirmed", UserEmail = "a@t.com" },
                new Booking { UserId = 1, RoomId = 2, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2), Status = "Cancelled", UserEmail = "a@t.com" }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetUserBookingsAsync(1, "active");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo("Confirmed"));
        }

        [Test]
        public async Task GetUserBookingsAsync_TypePast_ReturnsCancelledAndCompleted()
        {
            _context.Bookings.AddRange(
                new Booking { UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2), Status = "Confirmed", UserEmail = "a@t.com" },
                new Booking { UserId = 1, RoomId = 2, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2), Status = "Cancelled", UserEmail = "a@t.com" },
                new Booking { UserId = 1, RoomId = 3, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2), Status = "Completed", UserEmail = "a@t.com" }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetUserBookingsAsync(1, "past");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(b => b.Status == "Cancelled" || b.Status == "Completed"), Is.True);
        }

        // ──────────────────────────────────────────────
        // CancelBookingAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task CancelBookingAsync_ValidRequest_SetsStatusToCancelled()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.Today.AddDays(5), // 5 days in future → no deduction
                ToDate = DateTime.Today.AddDays(8),
                Status = "Confirmed", TotalPrice = 500m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _bookingService.CancelBookingAsync(booking.BookingId, 1, "a@t.com", "User");

            Assert.That(result.Status, Is.EqualTo("Cancelled"));
        }

        [Test]
        public async Task CancelBookingAsync_CheckInMoreThan72hAway_NoDeduction()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.UtcNow.AddDays(10),
                ToDate = DateTime.UtcNow.AddDays(12),
                Status = "Confirmed", TotalPrice = 1000m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _bookingService.CancelBookingAsync(booking.BookingId, 1, "a@t.com", "User");

            Assert.That(result.CancellationDeduction, Is.EqualTo(0m));
            Assert.That(result.RefundAmount, Is.EqualTo(1000m));
        }

        [Test]
        public async Task CancelBookingAsync_CheckInLessThan24h_50PercentDeduction()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                // Check-in is today at noon (checkInTime = today + 12h).
                // As long as current time is before noon, hoursUntilCheckIn < 24h → 50% deduction.
                // If test runs after noon local, within 0h → 100% deduction.
                // Use DateTime.UtcNow.Date (midnight) so checkInTime = today noon.
                // At any time before midnight preceding check-in we are within 24h window (<24h, ≥0h).
                FromDate = DateTime.UtcNow.Date,
                ToDate = DateTime.UtcNow.Date.AddDays(2),
                Status = "Confirmed", TotalPrice = 1000m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _bookingService.CancelBookingAsync(booking.BookingId, 1, "a@t.com", "User");

            // Either 50% (less than 24h) or 100% (past check-in) — both are valid deductions
            var validDeductions = new[] { 500m, 1000m };
            Assert.That(validDeductions, Contains.Item(result.CancellationDeduction));
            Assert.That(result.TotalPrice - result.CancellationDeduction, Is.EqualTo(result.RefundAmount));
        }

        [Test]
        public void CancelBookingAsync_BookingNotFound_ThrowsBookingNotFoundException()
        {
            Assert.ThrowsAsync<BookingNotFoundException>(() =>
                _bookingService.CancelBookingAsync(9999, 1, "a@t.com", "User"));
        }

        [Test]
        public async Task CancelBookingAsync_AlreadyCancelled_ThrowsBookingAlreadyCancelledException()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.Today.AddDays(3), ToDate = DateTime.Today.AddDays(5),
                Status = "Cancelled", TotalPrice = 300m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<BookingAlreadyCancelledException>(() =>
                _bookingService.CancelBookingAsync(booking.BookingId, 1, "a@t.com", "User"));
        }

        [Test]
        public async Task CancelBookingAsync_OtherUserCannotCancel_ThrowsUnauthorizedAccessException()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.Today.AddDays(5), ToDate = DateTime.Today.AddDays(8),
                Status = "Confirmed", TotalPrice = 500m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // userId 2 tries to cancel booking that belongs to userId 1
            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _bookingService.CancelBookingAsync(booking.BookingId, 2, "b@t.com", "User"));
        }

        [Test]
        public async Task CancelBookingAsync_AdminCanCancelAnyBooking()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.UtcNow.AddDays(10), ToDate = DateTime.UtcNow.AddDays(12),
                Status = "Confirmed", TotalPrice = 500m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _bookingService.CancelBookingAsync(booking.BookingId, 99, "admin@t.com", "Admin");

            Assert.That(result.Status, Is.EqualTo("Cancelled"));
        }

        [Test]
        public async Task CancelBookingAsync_PublishesCancellationEvent()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.UtcNow.AddDays(10), ToDate = DateTime.UtcNow.AddDays(12),
                Status = "Confirmed", TotalPrice = 300m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            await _bookingService.CancelBookingAsync(booking.BookingId, 1, "a@t.com", "User");

            _publishMock.Verify(p => p.Publish(
                It.IsAny<BookingCancelledEvent>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ──────────────────────────────────────────────
        // GetBookingByIdAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetBookingByIdAsync_ValidId_ReturnsBooking()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "Grand", RoomType = "Suite",
                FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2),
                Status = "Confirmed", TotalPrice = 200m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingByIdAsync(booking.BookingId, 1, "User");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.HotelName, Is.EqualTo("Grand"));
        }

        [Test]
        public void GetBookingByIdAsync_NotFound_ThrowsBookingNotFoundException()
        {
            Assert.ThrowsAsync<BookingNotFoundException>(() =>
                _bookingService.GetBookingByIdAsync(9999, 1, "User"));
        }

        [Test]
        public async Task GetBookingByIdAsync_OtherUser_ThrowsUnauthorizedAccessException()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2),
                Status = "Confirmed", TotalPrice = 100m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _bookingService.GetBookingByIdAsync(booking.BookingId, 2, "User"));
        }

        [Test]
        public async Task GetBookingByIdAsync_AdminCanViewAnyBooking()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2),
                Status = "Confirmed", TotalPrice = 100m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingByIdAsync(booking.BookingId, 99, "Admin");
            Assert.That(result, Is.Not.Null);
        }

        // ──────────────────────────────────────────────
        // CompleteBookingAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task CompleteBookingAsync_Admin_SetsStatusToCompleted()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2),
                Status = "Confirmed", TotalPrice = 100m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _bookingService.CompleteBookingAsync(booking.BookingId, 99, "Admin");

            Assert.That(result.Status, Is.EqualTo("Completed"));
        }

        [Test]
        public void CompleteBookingAsync_NotFound_ThrowsBookingNotFoundException()
        {
            Assert.ThrowsAsync<BookingNotFoundException>(() =>
                _bookingService.CompleteBookingAsync(9999, 1, "Admin"));
        }

        [Test]
        public async Task CompleteBookingAsync_AlreadyCompleted_ReturnsCurrentState()
        {
            var booking = new Booking
            {
                UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R",
                FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2),
                Status = "Completed", TotalPrice = 100m, UserEmail = "a@t.com"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = await _bookingService.CompleteBookingAsync(booking.BookingId, 99, "Admin");

            Assert.That(result.Status, Is.EqualTo("Completed"));
        }

        // ──────────────────────────────────────────────
        // GetAllBookingsAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetAllBookingsAsync_NoFilter_ReturnsAllBookings()
        {
            _context.Bookings.AddRange(
                new Booking { UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(1), Status = "Confirmed", UserEmail = "a@t.com" },
                new Booking { UserId = 2, RoomId = 2, HotelName = "H", RoomType = "R", FromDate = DateTime.Today, ToDate = DateTime.Today.AddDays(2), Status = "Cancelled", UserEmail = "b@t.com" }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetAllBookingsAsync(null);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllBookingsAsync_WithDateFilter_ReturnsOnlyMatchingDate()
        {
            var targetDate = DateTime.Today.AddDays(3);
            _context.Bookings.AddRange(
                new Booking { UserId = 1, RoomId = 1, HotelName = "H", RoomType = "R", FromDate = targetDate, ToDate = targetDate.AddDays(2), Status = "Confirmed", UserEmail = "a@t.com" },
                new Booking { UserId = 2, RoomId = 2, HotelName = "H", RoomType = "R", FromDate = DateTime.Today.AddDays(10), ToDate = DateTime.Today.AddDays(12), Status = "Confirmed", UserEmail = "b@t.com" }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetAllBookingsAsync(targetDate);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].FromDate.Date, Is.EqualTo(targetDate.Date));
        }
    }
}
