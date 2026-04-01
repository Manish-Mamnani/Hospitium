using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hospitium.BookingService.Data;
using Hospitium.BookingService.DTOs;
using Hospitium.BookingService.Exceptions;
using Hospitium.BookingService.HttpClients;
using Hospitium.BookingService.Interfaces;
using Hospitium.BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingDbContext _context;
        private readonly IHotelClient _hotelClient;

        public BookingService(BookingDbContext context, IHotelClient hotelClient)
        {
            _context = context;
            _hotelClient = hotelClient;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(int userId, string email, CreateBookingDto dto)
        {
            // 🟡 Check availability and get room details via Hotel Service
            var room = await _hotelClient.GetRoomDetailsAsync(dto.RoomId);
            
            if (room == null || room.AvailableCount < dto.NumberOfRooms)
                throw new RoomNotAvailableException(dto.RoomId);

            // 🔴 Validate Dates
            if (dto.FromDate >= dto.ToDate)
                throw new InvalidBookingDatesException();

            // 🔴 Check overlapping bookings
            var isOverlapping = await _context.Bookings.AnyAsync(b =>
                b.RoomId == dto.RoomId &&
                b.Status != "Cancelled" &&
                dto.FromDate < b.ToDate &&
                dto.ToDate > b.FromDate);

            if (isOverlapping)
                throw new BookingConflictException();

            var days = (dto.ToDate - dto.FromDate).Days;
            if (days < 1) days = 1;
            
            var booking = new Booking
            {
                UserId = userId,
                RoomId = dto.RoomId,
                HotelName = room.HotelName,
                RoomType = room.Type,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                NumberOfRooms = dto.NumberOfRooms,
                TotalPrice = room.Price * dto.NumberOfRooms * days,
                Status = "Confirmed",
                UserEmail = email
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return await MapToResponse(booking);
        }

        public async Task<List<BookingResponseDto>> GetUserBookingsAsync(int userId, string? type)
        {
            var query = _context.Bookings.Where(b => b.UserId == userId);

            if (!string.IsNullOrEmpty(type))
            {
                if (type == "active")
                    query = query.Where(b => b.Status == "Confirmed");
                else if (type == "past")
                    query = query.Where(b => b.Status == "Cancelled" || b.Status == "Completed");
            }

            var bookings = await query.OrderByDescending(b => b.FromDate).ToListAsync();

            var dtos = new List<BookingResponseDto>();
            foreach (var b in bookings)
            {
                dtos.Add(await MapToResponse(b));
            }
            return dtos;
        }

        public async Task<List<BookingResponseDto>> GetAllBookingsAsync(DateTime? date)
        {
            var query = _context.Bookings.AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(b => b.FromDate.Date == date.Value.Date);
            }

            var bookings = await query.OrderByDescending(b => b.BookingId).ToListAsync();

            var responseTasks = bookings.Select(b => MapToResponse(b));
            var responseList = await Task.WhenAll(responseTasks);
            return responseList.ToList();
        }

        public async Task<List<BookingResponseDto>> GetManagerBookingsAsync()
        {
            var roomIds = await _hotelClient.GetManagerRoomIdsAsync();

            if (roomIds == null || !roomIds.Any())
                return new List<BookingResponseDto>();

            var bookings = await _context.Bookings
                .Where(b => roomIds.Contains(b.RoomId))
                .OrderByDescending(b => b.BookingId)
                .ToListAsync();

            var responseTasks = bookings.Select(b => MapToResponse(b));
            var responseList = await Task.WhenAll(responseTasks);
            return responseList.ToList();
        }

        public async Task<BookingResponseDto> CancelBookingAsync(int id, int userId, string email, string role)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                throw new BookingNotFoundException(id);

            if (role != "Admin" && booking.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own bookings.");

            if (booking.Status == "Cancelled")
                throw new BookingAlreadyCancelledException(id);

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return await MapToResponse(booking);
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(int id, int userId, string role)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                throw new BookingNotFoundException(id);

            if (role != "Admin" && booking.UserId != userId)
                throw new UnauthorizedAccessException("You can only view your own bookings.");

            return await MapToResponse(booking);
        }

        private async Task<BookingResponseDto> MapToResponse(Booking b)
        {
            var dto = new BookingResponseDto
            {
                BookingId = b.BookingId,
                UserId = b.UserId,
                UserEmail = b.UserEmail,
                RoomId = b.RoomId,
                HotelName = b.HotelName,
                RoomType = b.RoomType,
                FromDate = b.FromDate,
                ToDate = b.ToDate,
                NumberOfRooms = b.NumberOfRooms,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            };

            // Fallback for legacy bookings where denormalized fields are empty
            if (string.IsNullOrEmpty(dto.HotelName))
            {
                try
                {
                    var room = await _hotelClient.GetRoomDetailsAsync(b.RoomId);
                    if (room != null)
                    {
                        dto.HotelName = room.HotelName;
                        dto.RoomType = room.Type;
                    }
                    else
                    {
                        dto.HotelName = "Unknown Hotel";
                        dto.RoomType = "Unknown Room";
                    }
                }
                catch
                {
                    // Fail gracefully for legacy data fetching
                    dto.HotelName = "Unknown Hotel (Fetch Failed)";
                    dto.RoomType = "Unknown Room";
                }
            }

            return dto;
        }
    }
}