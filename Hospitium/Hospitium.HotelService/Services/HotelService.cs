using Hospitium.HotelService.Data;
using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Exceptions;
using Hospitium.HotelService.Models;
using Hospitium.HotelService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.HotelService.Services
{
    public class HotelService : IHotelService
    {
        private readonly HotelDbContext _context;

        public HotelService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<HotelResponseDto> CreateHotelAsync(int userId, CreateHotelDto dto)
        {
            var hotel = new Hotel
            {
                Name = dto.Name,
                City = dto.City,
                Description = dto.Description,
                AverageRating = 0,
                TotalReviews = 0,
                Status = "Pending",
                CreatedByUserId = userId
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return new HotelResponseDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                City = hotel.City,
                Rating = hotel.AverageRating,
                Status = hotel.Status,
                MinPrice = 0
            };
        }

        public async Task<RoomResponseDto> CreateRoomAsync(int userId, CreateRoomDto dto)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId == dto.HotelId);

            if (hotel == null)
                throw new HotelNotFoundException(dto.HotelId);

            if (hotel.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You cannot add rooms to this hotel.");

            if (hotel.Status != "Approved")
                throw new InvalidHotelOperationException("Hotel must be approved before adding rooms.");

            var room = new Room
            {
                HotelId = dto.HotelId,
                Type = dto.Type,
                Price = dto.Price,
                TotalCount = dto.TotalCount,
                AvailableCount = dto.TotalCount
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return new RoomResponseDto
            {
                RoomId = room.RoomId,
                Type = room.Type,
                Price = room.Price,
                AvailableCount = room.AvailableCount
            };
        }

        public async Task<HotelResponseDto> ApproveHotelAsync(int hotelId)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);

            if (hotel == null)
                throw new HotelNotFoundException(hotelId);

            if (hotel.Status == "Approved")
                throw new InvalidHotelOperationException("Hotel is already approved.");

            hotel.Status = "Approved";

            await _context.SaveChangesAsync();

            var minPrice = hotel.Rooms.Any()
                ? hotel.Rooms.Min(r => r.Price)
                : 0;

            return new HotelResponseDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                City = hotel.City,
                Status = hotel.Status,
                Rating = hotel.AverageRating,
                MinPrice = minPrice
            };
        }

        public async Task<HotelResponseDto> RejectHotelAsync(int hotelId)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);

            if (hotel == null)
                throw new HotelNotFoundException(hotelId);

            if (hotel.Status == "Rejected")
                throw new InvalidHotelOperationException("Hotel is already rejected.");

            hotel.Status = "Rejected";

            await _context.SaveChangesAsync();

            var minPrice = hotel.Rooms.Any()
                ? hotel.Rooms.Min(r => r.Price)
                : 0;

            return new HotelResponseDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                City = hotel.City,
                Status = hotel.Status,
                Rating = hotel.AverageRating,
                MinPrice = minPrice
            };
        }

        public async Task<RoomResponseDto> GetRoomByIdAsync(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
                throw new RoomNotFoundException(roomId);

            return new RoomResponseDto
            {
                RoomId = room.RoomId,
                Type = room.Type,
                Price = room.Price,
                AvailableCount = room.AvailableCount
            };
        }

        public async Task<HotelResponseDto> GetHotelByIdAsync(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
                throw new HotelNotFoundException(id);

            if (hotel.Status != "Approved")
                throw new InvalidHotelOperationException("Hotel not available.");

            var minPrice = hotel.Rooms.Any()
                ? hotel.Rooms.Min(r => r.Price)
                : 0;

            return new HotelResponseDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                City = hotel.City,
                Rating = hotel.AverageRating,
                Status = hotel.Status,
                MinPrice = minPrice
            };
        }

        public async Task<List<HotelResponseDto>> GetApprovedHotelsAsync()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Where(h => h.Status == "Approved")
                .ToListAsync();

            return hotels.Select(h => new HotelResponseDto
            {
                HotelId = h.HotelId,
                Name = h.Name,
                City = h.City,
                Status = h.Status,
                Rating = h.AverageRating,
                MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.Price) : 0
            }).ToList();
        }

        public async Task<List<HotelResponseDto>> GetPendingHotelsAsync()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Where(h => h.Status == "Pending")
                .ToListAsync();

            return hotels.Select(h => new HotelResponseDto
            {
                HotelId = h.HotelId,
                Name = h.Name,
                City = h.City,
                Status = h.Status,
                Rating = h.AverageRating,
                MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.Price) : 0
            }).ToList();
        }

        public async Task<List<HotelResponseDto>> GetMyHotelsAsync(int userId)
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Where(h => h.CreatedByUserId == userId)
                .ToListAsync();

            return hotels.Select(h => new HotelResponseDto
            {
                HotelId = h.HotelId,
                Name = h.Name,
                City = h.City,
                Rating = h.AverageRating,
                Status = h.Status,
                MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.Price) : 0
            }).ToList();
        }

        public async Task<List<RoomResponseDto>> GetRoomsByHotelIdAsync(int hotelId)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);

            if (hotel == null)
                throw new HotelNotFoundException(hotelId);

            if (hotel.Status != "Approved")
                throw new InvalidHotelOperationException("Hotel not available.");

            var rooms = await _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();

            return rooms.Select(r => new RoomResponseDto
            {
                RoomId = r.RoomId,
                Type = r.Type,
                Price = r.Price,
                AvailableCount = r.AvailableCount
            }).ToList();
        }

        public async Task<RoomResponseDto> UpdateRoomAsync(int roomId, int userId, UpdateRoomDto dto)
        {
            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                throw new RoomNotFoundException(roomId);

            var hotel = room.Hotel;

            if (hotel.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You cannot update this room.");

            if (hotel.Status != "Approved")
                throw new InvalidHotelOperationException("Cannot update rooms of unapproved hotel.");

            int difference = dto.TotalCount - room.TotalCount;

            room.Price = dto.Price;
            room.TotalCount = dto.TotalCount;
            room.AvailableCount = Math.Min(room.AvailableCount + difference, room.TotalCount);

            if (room.AvailableCount < 0)
                room.AvailableCount = 0;

            await _context.SaveChangesAsync();

            return new RoomResponseDto
            {
                RoomId = room.RoomId,
                Type = room.Type,
                Price = room.Price,
                AvailableCount = room.AvailableCount
            };
        }

        public async Task<HotelResponseDto> UpdateHotelAsync(int id, CreateHotelDto dto)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
                throw new HotelNotFoundException(id);

            hotel.Name = dto.Name;
            hotel.City = dto.City;
            hotel.Description = dto.Description;

            await _context.SaveChangesAsync();

            var minPrice = hotel.Rooms.Any()
                ? hotel.Rooms.Min(r => r.Price)
                : 0;

            return new HotelResponseDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                City = hotel.City,
                Status = hotel.Status,
                Rating = hotel.AverageRating,
                MinPrice = minPrice
            };
        }

        public async Task DeleteHotelAsync(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
                throw new HotelNotFoundException(id);

            hotel.Status = "Deleted";
            await _context.SaveChangesAsync();
        }

        public async Task<HotelResponseDto> AddRatingAsync(int hotelId, double rating)
        {
            if (rating < 1 || rating > 5)
                throw new InvalidRatingException();

            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);

            if (hotel == null)
                throw new HotelNotFoundException(hotelId);

            if (hotel.Status != "Approved")
                throw new InvalidHotelOperationException("Cannot rate unapproved hotel.");

            hotel.TotalReviews++;

            hotel.AverageRating =
                ((hotel.AverageRating * (hotel.TotalReviews - 1)) + rating)
                / hotel.TotalReviews;

            await _context.SaveChangesAsync();

            var minPrice = hotel.Rooms.Any()
                ? hotel.Rooms.Min(r => r.Price)
                : 0;

            return new HotelResponseDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                City = hotel.City,
                Status = hotel.Status,
                Rating = hotel.AverageRating,
                MinPrice = minPrice
            };
        }
    }
}