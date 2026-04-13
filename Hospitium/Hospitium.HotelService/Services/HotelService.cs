using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hospitium.Contracts.Events;
using Hospitium.HotelService.Data;
using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Exceptions;
using Hospitium.HotelService.Models;
using Hospitium.HotelService.Services.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.HotelService.Services
{
    /// <summary>
    /// Service implementation for managing hotel and room state, including creation, approval, rejection,
    /// image management, and publishing events for notifications and cross-service communication.
    /// </summary>
    public class HotelService : IHotelService
    {
        private readonly HotelDbContext _context;
        private readonly IPublishEndpoint _publish;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelService"/> class.
        /// </summary>
        /// <param name="context">The hotel database context.</param>
        /// <param name="publish">The MassTransit publish endpoint for event publishing.</param>
        public HotelService(HotelDbContext context, IPublishEndpoint publish)
        {
            _context = context;
            _publish = publish;
        }

        public async Task<HotelResponseDto> CreateHotelAsync(int userId, string email, CreateHotelDto dto)
        {
            var hotel = new Hotel
            {
                Name = dto.Name,
                City = dto.City,
                Description = dto.Description ?? string.Empty,
                AverageRating = 0,
                TotalReviews = 0,
                Status = "Pending",
                CreatedByUserId = userId,
                ManagerEmail = email
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return MapToHotelResponse(hotel);
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
                .Include(h => h.Images)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);

            if (hotel == null)
                throw new HotelNotFoundException(hotelId);

            if (hotel.Status == "Approved")
                throw new InvalidHotelOperationException("Hotel is already approved.");

            hotel.Status = "Approved";
            await _context.SaveChangesAsync();

            await _publish.Publish(new HotelApprovedEvent
            {
                HotelId = hotel.HotelId,
                HotelName = hotel.Name,
                ManagerEmail = hotel.ManagerEmail
            });

            return MapToHotelResponse(hotel);
        }

        public async Task<HotelResponseDto> RejectHotelAsync(int hotelId)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .FirstOrDefaultAsync(h => h.HotelId == hotelId);

            if (hotel == null)
                throw new HotelNotFoundException(hotelId);

            if (hotel.Status == "Rejected")
                throw new InvalidHotelOperationException("Hotel is already rejected.");

            hotel.Status = "Rejected";
            await _context.SaveChangesAsync();

            await _publish.Publish(new HotelRejectedEvent
            {
                HotelId = hotel.HotelId,
                HotelName = hotel.Name,
                ManagerEmail = hotel.ManagerEmail
            });

            return MapToHotelResponse(hotel);
        }

        public async Task<RoomResponseDto> GetRoomByIdAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                throw new RoomNotFoundException(roomId);

            return new RoomResponseDto
            {
                RoomId = room.RoomId,
                HotelName = room.Hotel?.Name ?? "Unknown Hotel",
                Type = room.Type,
                Price = room.Price,
                AvailableCount = room.AvailableCount
            };
        }

        public async Task<HotelResponseDto> GetHotelByIdAsync(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
                throw new HotelNotFoundException(id);

            if (hotel.Status != "Approved")
                throw new InvalidHotelOperationException("Hotel not available.");

            return MapToHotelResponse(hotel);
        }

        public async Task<List<HotelResponseDto>> GetApprovedHotelsAsync()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .Where(h => h.Status == "Approved")
                .ToListAsync();

            return hotels.Select(MapToHotelResponse).ToList();
        }

        public async Task<List<HotelResponseDto>> GetPendingHotelsAsync()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .Where(h => h.Status == "Pending")
                .ToListAsync();

            return hotels.Select(MapToHotelResponse).ToList();
        }

        public async Task<List<HotelResponseDto>> GetMyHotelsAsync(int userId)
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .Where(h => h.CreatedByUserId == userId)
                .ToListAsync();

            return hotels.Select(MapToHotelResponse).ToList();
        }

        public async Task<List<int>> GetMyRoomIdsAsync(int userId)
        {
            return await _context.Rooms
                .Where(r => r.Hotel.CreatedByUserId == userId)
                .Select(r => r.RoomId)
                .ToListAsync();
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

            if (hotel == null)
                throw new InvalidHotelOperationException("Room is not associated with a hotel.");

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

        public async Task<HotelResponseDto> UpdateHotelAsync(int id, int userId, string role, CreateHotelDto dto)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
                throw new HotelNotFoundException(id);

            if (hotel.CreatedByUserId != userId && role != "Admin")
                throw new UnauthorizedAccessException("You can only update your own hotels.");

            hotel.Name = dto.Name;
            hotel.City = dto.City;
            hotel.Description = dto.Description ?? string.Empty;

            await _context.SaveChangesAsync();

            return MapToHotelResponse(hotel);
        }

        public async Task DeleteHotelAsync(int id, int userId, string role)
        {
            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
                throw new HotelNotFoundException(id);

            if (hotel.CreatedByUserId != userId && role != "Admin")
                throw new UnauthorizedAccessException("You can only delete your own hotels.");

            hotel.Status = "Deleted";
            await _context.SaveChangesAsync();
        }

        public async Task<List<HotelResponseDto>> GetAllHotelsAsync()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Images)
                .OrderByDescending(h => h.HotelId)
                .ToListAsync();

            return hotels.Select(MapToHotelResponse).ToList();
        }

        public async Task<HotelImageResponseDto> AddHotelImageAsync(int hotelId, int userId, string role, string imageUrl, bool isPrimary)
        {
            var hotel = await _context.Hotels.Include(h => h.Images).FirstOrDefaultAsync(h => h.HotelId == hotelId);
            if (hotel == null) throw new HotelNotFoundException(hotelId);

            if (hotel.CreatedByUserId != userId && role != "Admin")
                throw new UnauthorizedAccessException("You can only add images to your own hotels.");

            if (hotel.Images.Count >= 10)
                throw new InvalidOperationException("Maximum limit of 10 images reached.");

            if (isPrimary && hotel.Images.Any())
            {
                foreach (var img in hotel.Images) img.IsPrimary = false;
            }
            else if (!hotel.Images.Any())
            {
                isPrimary = true;
            }

            var image = new HotelImage
            {
                HotelId = hotelId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary
            };

            _context.HotelImages.Add(image);
            await _context.SaveChangesAsync();

            return new HotelImageResponseDto
            {
                ImageId = image.ImageId,
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary
            };
        }

        public async Task DeleteHotelImageAsync(int hotelId, int imageId, int userId, string role)
        {
            var hotel = await _context.Hotels.Include(h => h.Images).FirstOrDefaultAsync(h => h.HotelId == hotelId);
            if (hotel == null) throw new HotelNotFoundException(hotelId);

            if (hotel.CreatedByUserId != userId && role != "Admin")
                throw new UnauthorizedAccessException("You can only delete images from your own hotels.");

            var image = hotel.Images.FirstOrDefault(i => i.ImageId == imageId);
            if (image == null) throw new KeyNotFoundException("Image not found.");

            _context.HotelImages.Remove(image);
            
            if (image.IsPrimary && hotel.Images.Count > 1)
            {
                var first = hotel.Images.FirstOrDefault(i => i.ImageId != imageId);
                if (first != null) first.IsPrimary = true;
            }

            await _context.SaveChangesAsync();
        }

        private HotelResponseDto MapToHotelResponse(Hotel h)
        {
            return new HotelResponseDto
            {
                HotelId = h.HotelId,
                Name = h.Name,
                City = h.City,
                Status = h.Status,
                ManagerEmail = h.ManagerEmail,
                Description = h.Description,
                Rating = h.AverageRating,
                MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.Price) : 0,
                Images = h.Images?.Select(i => new HotelImageResponseDto 
                { 
                    ImageId = i.ImageId, 
                    ImageUrl = i.ImageUrl, 
                    IsPrimary = i.IsPrimary 
                }).ToList() ?? new List<HotelImageResponseDto>()
            };
        }
    }
}