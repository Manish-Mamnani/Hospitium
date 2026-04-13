using Hospitium.Contracts.Events;
using Hospitium.HotelService.Data;
using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Exceptions;
using Hospitium.HotelService.Models;
using Hospitium.HotelService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hospitium.Testing.HotelService
{
    [TestFixture]
    public class HotelServiceTests
    {
        private HotelDbContext _context = null!;
        private Mock<IPublishEndpoint> _publishMock = null!;
        private Hospitium.HotelService.Services.HotelService _hotelService = null!;

        private const int ManagerId = 10;
        private const string ManagerEmail = "manager@hotel.com";

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new HotelDbContext(options);
            _publishMock = new Mock<IPublishEndpoint>();
            _publishMock
                .Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _hotelService = new Hospitium.HotelService.Services.HotelService(_context, _publishMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // Helper: seed an approved hotel with rooms
        private async Task<Hotel> SeedApprovedHotel(int userId = ManagerId, int roomCount = 5, decimal price = 200m)
        {
            var hotel = new Hotel
            {
                Name = "Luxury Inn",
                City = "Karachi",
                Description = "A great place",
                Status = "Approved",
                CreatedByUserId = userId,
                ManagerEmail = ManagerEmail
            };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            var room = new Room
            {
                HotelId = hotel.HotelId,
                Type = "Deluxe",
                Price = price,
                TotalCount = roomCount,
                AvailableCount = roomCount
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return hotel;
        }

        // ──────────────────────────────────────────────
        // CreateHotelAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task CreateHotelAsync_ValidDto_ReturnsHotelResponse()
        {
            var dto = new CreateHotelDto { Name = "Grand Palace", City = "Lahore", Description = "Lovely hotel" };

            var result = await _hotelService.CreateHotelAsync(ManagerId, ManagerEmail, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Grand Palace"));
            Assert.That(result.City, Is.EqualTo("Lahore"));
            Assert.That(result.Status, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task CreateHotelAsync_NewHotelIsPending()
        {
            var dto = new CreateHotelDto { Name = "Budget Stay", City = "Islamabad" };

            var result = await _hotelService.CreateHotelAsync(ManagerId, ManagerEmail, dto);

            Assert.That(result.Status, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task CreateHotelAsync_NullDescription_UsesEmptyString()
        {
            var dto = new CreateHotelDto { Name = "No Desc Hotel", City = "Peshawar", Description = null };

            var result = await _hotelService.CreateHotelAsync(ManagerId, ManagerEmail, dto);

            Assert.That(result, Is.Not.Null);
            // Should not throw and description should default to empty
        }

        // ──────────────────────────────────────────────
        // ApproveHotelAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task ApproveHotelAsync_PendingHotel_ChangesStatusToApproved()
        {
            var hotel = new Hotel { Name = "Pending Inn", City = "Quetta", Status = "Pending", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            var result = await _hotelService.ApproveHotelAsync(hotel.HotelId);

            Assert.That(result.Status, Is.EqualTo("Approved"));
        }

        [Test]
        public async Task ApproveHotelAsync_PublishesHotelApprovedEvent()
        {
            var hotel = new Hotel { Name = "Event Hotel", City = "Karachi", Status = "Pending", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            await _hotelService.ApproveHotelAsync(hotel.HotelId);

            _publishMock.Verify(p => p.Publish(
                It.IsAny<HotelApprovedEvent>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ApproveHotelAsync_AlreadyApproved_ThrowsInvalidHotelOperationException()
        {
            var hotel = new Hotel { Name = "Already Approved", City = "Karachi", Status = "Approved", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidHotelOperationException>(() =>
                _hotelService.ApproveHotelAsync(hotel.HotelId));
        }

        [Test]
        public void ApproveHotelAsync_HotelNotFound_ThrowsHotelNotFoundException()
        {
            Assert.ThrowsAsync<HotelNotFoundException>(() =>
                _hotelService.ApproveHotelAsync(9999));
        }

        // ──────────────────────────────────────────────
        // RejectHotelAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task RejectHotelAsync_PendingHotel_ChangesStatusToRejected()
        {
            var hotel = new Hotel { Name = "Bad Hotel", City = "Karachi", Status = "Pending", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            var result = await _hotelService.RejectHotelAsync(hotel.HotelId);

            Assert.That(result.Status, Is.EqualTo("Rejected"));
        }

        [Test]
        public async Task RejectHotelAsync_AlreadyRejected_ThrowsInvalidHotelOperationException()
        {
            var hotel = new Hotel { Name = "Rejected", City = "Karachi", Status = "Rejected", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidHotelOperationException>(() =>
                _hotelService.RejectHotelAsync(hotel.HotelId));
        }

        [Test]
        public async Task RejectHotelAsync_PublishesHotelRejectedEvent()
        {
            var hotel = new Hotel { Name = "Reject Event Hotel", City = "Lahore", Status = "Pending", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            await _hotelService.RejectHotelAsync(hotel.HotelId);

            _publishMock.Verify(p => p.Publish(
                It.IsAny<HotelRejectedEvent>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ──────────────────────────────────────────────
        // CreateRoomAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task CreateRoomAsync_ValidRequest_ReturnsRoomResponse()
        {
            var hotel = await SeedApprovedHotel();
            // Remove the seed room so we can add fresh
            _context.Rooms.RemoveRange(_context.Rooms);
            await _context.SaveChangesAsync();

            var dto = new CreateRoomDto { HotelId = hotel.HotelId, Type = "Suite", Price = 350m, TotalCount = 3 };

            var result = await _hotelService.CreateRoomAsync(ManagerId, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Type, Is.EqualTo("Suite"));
            Assert.That(result.Price, Is.EqualTo(350m));
            Assert.That(result.AvailableCount, Is.EqualTo(3));
        }

        [Test]
        public async Task CreateRoomAsync_HotelNotFound_ThrowsHotelNotFoundException()
        {
            var dto = new CreateRoomDto { HotelId = 9999, Type = "Standard", Price = 100m, TotalCount = 2 };

            Assert.ThrowsAsync<HotelNotFoundException>(() =>
                _hotelService.CreateRoomAsync(ManagerId, dto));
        }

        [Test]
        public async Task CreateRoomAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            var hotel = await SeedApprovedHotel(userId: ManagerId);
            _context.Rooms.RemoveRange(_context.Rooms);
            await _context.SaveChangesAsync();

            var dto = new CreateRoomDto { HotelId = hotel.HotelId, Type = "Standard", Price = 100m, TotalCount = 2 };

            // UserId 999 is not the owner
            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _hotelService.CreateRoomAsync(999, dto));
        }

        [Test]
        public async Task CreateRoomAsync_HotelNotApproved_ThrowsInvalidHotelOperationException()
        {
            var hotel = new Hotel { Name = "Not Approved", City = "Lahore", Status = "Pending", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            var dto = new CreateRoomDto { HotelId = hotel.HotelId, Type = "Standard", Price = 100m, TotalCount = 2 };

            Assert.ThrowsAsync<InvalidHotelOperationException>(() =>
                _hotelService.CreateRoomAsync(ManagerId, dto));
        }

        // ──────────────────────────────────────────────
        // GetHotelByIdAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetHotelByIdAsync_ApprovedHotel_ReturnsHotelResponse()
        {
            var hotel = await SeedApprovedHotel();

            var result = await _hotelService.GetHotelByIdAsync(hotel.HotelId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Luxury Inn"));
        }

        [Test]
        public void GetHotelByIdAsync_NotFound_ThrowsHotelNotFoundException()
        {
            Assert.ThrowsAsync<HotelNotFoundException>(() =>
                _hotelService.GetHotelByIdAsync(9999));
        }

        [Test]
        public async Task GetHotelByIdAsync_PendingHotel_ThrowsInvalidHotelOperationException()
        {
            var hotel = new Hotel { Name = "Pending", City = "Karachi", Status = "Pending", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail };
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidHotelOperationException>(() =>
                _hotelService.GetHotelByIdAsync(hotel.HotelId));
        }

        // ──────────────────────────────────────────────
        // GetApprovedHotelsAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetApprovedHotelsAsync_ReturnsOnlyApproved()
        {
            _context.Hotels.AddRange(
                new Hotel { Name = "A", City = "C1", Status = "Approved", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail },
                new Hotel { Name = "B", City = "C2", Status = "Pending", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail },
                new Hotel { Name = "C", City = "C3", Status = "Approved", CreatedByUserId = ManagerId, ManagerEmail = ManagerEmail }
            );
            await _context.SaveChangesAsync();

            var result = await _hotelService.GetApprovedHotelsAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(h => h.Status == "Approved"), Is.True);
        }

        // ──────────────────────────────────────────────
        // UpdateHotelAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task UpdateHotelAsync_Owner_UpdatesHotelDetails()
        {
            var hotel = await SeedApprovedHotel();

            var dto = new CreateHotelDto { Name = "Updated Name", City = "Updated City", Description = "New desc" };

            var result = await _hotelService.UpdateHotelAsync(hotel.HotelId, ManagerId, "HotelManager", dto);

            Assert.That(result.Name, Is.EqualTo("Updated Name"));
            Assert.That(result.City, Is.EqualTo("Updated City"));
        }

        [Test]
        public async Task UpdateHotelAsync_NonOwner_ThrowsUnauthorizedAccessException()
        {
            var hotel = await SeedApprovedHotel(userId: ManagerId);

            var dto = new CreateHotelDto { Name = "Hack", City = "Hack City" };

            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _hotelService.UpdateHotelAsync(hotel.HotelId, 999, "HotelManager", dto));
        }

        [Test]
        public async Task UpdateHotelAsync_AdminCanUpdateAnyHotel()
        {
            var hotel = await SeedApprovedHotel();

            var dto = new CreateHotelDto { Name = "Admin Updated", City = "Admin City" };

            var result = await _hotelService.UpdateHotelAsync(hotel.HotelId, 999, "Admin", dto);

            Assert.That(result.Name, Is.EqualTo("Admin Updated"));
        }

        // ──────────────────────────────────────────────
        // DeleteHotelAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task DeleteHotelAsync_Owner_SetsStatusToDeleted()
        {
            var hotel = await SeedApprovedHotel();

            await _hotelService.DeleteHotelAsync(hotel.HotelId, ManagerId, "HotelManager");

            var dbHotel = await _context.Hotels.FindAsync(hotel.HotelId);
            Assert.That(dbHotel!.Status, Is.EqualTo("Deleted"));
        }

        [Test]
        public async Task DeleteHotelAsync_NonOwner_ThrowsUnauthorizedAccessException()
        {
            var hotel = await SeedApprovedHotel();

            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _hotelService.DeleteHotelAsync(hotel.HotelId, 999, "HotelManager"));
        }

        [Test]
        public void DeleteHotelAsync_NotFound_ThrowsHotelNotFoundException()
        {
            Assert.ThrowsAsync<HotelNotFoundException>(() =>
                _hotelService.DeleteHotelAsync(9999, ManagerId, "HotelManager"));
        }

        // ──────────────────────────────────────────────
        // GetRoomByIdAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task GetRoomByIdAsync_ExistingRoom_ReturnsRoomResponse()
        {
            var hotel = await SeedApprovedHotel();
            var room = _context.Rooms.First();

            var result = await _hotelService.GetRoomByIdAsync(room.RoomId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RoomId, Is.EqualTo(room.RoomId));
        }

        [Test]
        public void GetRoomByIdAsync_NotFound_ThrowsRoomNotFoundException()
        {
            Assert.ThrowsAsync<RoomNotFoundException>(() =>
                _hotelService.GetRoomByIdAsync(9999));
        }

        // ──────────────────────────────────────────────
        // UpdateRoomAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task UpdateRoomAsync_ValidRequest_UpdatesRoomDetails()
        {
            var hotel = await SeedApprovedHotel(roomCount: 5, price: 100m);
            var room = _context.Rooms.First();

            var dto = new UpdateRoomDto { TotalCount = 8, Price = 200m };

            var result = await _hotelService.UpdateRoomAsync(room.RoomId, ManagerId, dto);

            Assert.That(result.Price, Is.EqualTo(200m));
            Assert.That(result.AvailableCount, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public async Task UpdateRoomAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            var hotel = await SeedApprovedHotel();
            var room = _context.Rooms.First();

            var dto = new UpdateRoomDto { TotalCount = 3, Price = 150m };

            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _hotelService.UpdateRoomAsync(room.RoomId, 999, dto));
        }

        // ──────────────────────────────────────────────
        // AddHotelImageAsync Tests
        // ──────────────────────────────────────────────

        [Test]
        public async Task AddHotelImageAsync_FirstImage_IsPrimaryAutomatically()
        {
            var hotel = await SeedApprovedHotel();

            var result = await _hotelService.AddHotelImageAsync(hotel.HotelId, ManagerId, "HotelManager", "https://img.url/photo.jpg", false);

            Assert.That(result.IsPrimary, Is.True); // First image should always be primary
        }

        [Test]
        public async Task AddHotelImageAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            var hotel = await SeedApprovedHotel();

            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _hotelService.AddHotelImageAsync(hotel.HotelId, 999, "HotelManager", "https://img.url/photo.jpg", false));
        }

        [Test]
        public async Task AddHotelImageAsync_HotelNotFound_ThrowsHotelNotFoundException()
        {
            Assert.ThrowsAsync<HotelNotFoundException>(() =>
                _hotelService.AddHotelImageAsync(9999, ManagerId, "HotelManager", "https://img.url/photo.jpg", false));
        }

        [Test]
        public async Task AddHotelImageAsync_ExceedsTenImages_ThrowsInvalidOperationException()
        {
            var hotel = await SeedApprovedHotel();

            // Add 10 images
            for (int i = 0; i < 10; i++)
            {
                _context.HotelImages.Add(new HotelImage { HotelId = hotel.HotelId, ImageUrl = $"https://img.url/{i}.jpg", IsPrimary = i == 0 });
            }
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _hotelService.AddHotelImageAsync(hotel.HotelId, ManagerId, "HotelManager", "https://img.url/extra.jpg", false));
        }
    }
}
