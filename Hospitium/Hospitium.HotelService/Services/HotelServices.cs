using Hospitium.HotelService.Data;
using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Exceptions;
using Hospitium.HotelService.Models;
using Hospitium.HotelService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.HotelService.Services
{
    public class HotelServices : IHotelService
    {
        private readonly HotelDbContext _context;

        public HotelServices(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<HotelResponseDto> CreateHotelAsync(CreateHotelDto dto)
        {
            var hotel = new Hotel
            {
                Name = dto.Name,
                City = dto.City,
                Description = dto.Description,
                AverageRating = 0,
                TotalReviews = 0
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return new HotelResponseDto
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                City = hotel.City,
                Rating = hotel.AverageRating,
                MinPrice = 0
            };
        }

        public async Task<object> CreateRoomAsync(CreateRoomDto dto)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId == dto.HotelId);

            if (hotel == null)
                throw new HotelNotFoundException(dto.HotelId);

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

            return new
            {
                room.RoomId,
                room.Type,
                room.Price,
                room.AvailableCount
            };
        }
    }
}