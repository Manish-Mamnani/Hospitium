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
using MassTransit;
using Hospitium.Contracts.Events;

namespace Hospitium.BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingDbContext _context;
        private readonly IHotelClient _hotelClient;
        private readonly IPublishEndpoint _publish;

        public BookingService(BookingDbContext context, IHotelClient hotelClient, IPublishEndpoint publish)
        {
            _context = context;
            _hotelClient = hotelClient;
            _publish = publish;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(int userId, string email, CreateBookingDto dto)
        {
            // 🟡 Check availability and get room details via Hotel Service
            var room = await _hotelClient.GetRoomDetailsAsync(dto.RoomId);
            
            if (room == null || room.AvailableCount < dto.NumberOfRooms)
                throw new RoomNotAvailableException(dto.RoomId);

            // Validate Dates
            if (dto.FromDate >= dto.ToDate)
                throw new InvalidBookingDatesException();

            // Check overlapping bookings and calculate total occupied rooms
            var totalBookedDuringPeriod = await _context.Bookings
                .Where(b => b.RoomId == dto.RoomId && 
                            b.Status != "Cancelled" && 
                            dto.FromDate < b.ToDate && 
                            dto.ToDate > b.FromDate)
                .SumAsync(b => b.NumberOfRooms);

            if (totalBookedDuringPeriod + dto.NumberOfRooms > room.AvailableCount)
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

            // 📢 Publish Creation Event for Email notifications
            await _publish.Publish(new BookingCreatedEvent
            {
                BookingId = booking.BookingId,
                RoomId = booking.RoomId,
                NumberOfRooms = booking.NumberOfRooms,
                UserEmail = booking.UserEmail,
                HotelName = booking.HotelName,
                FromDate = booking.FromDate,
                ToDate = booking.ToDate
            });

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

            // Cancellation Policy Logic (Local timezone assumed as reference for check-in)
            var checkInTime = booking.FromDate.Date.AddHours(12);
            var now = DateTime.UtcNow;
            var hoursUntilCheckIn = (checkInTime - now).TotalHours;

            decimal deductionPercentage = 0;
            if (hoursUntilCheckIn < 0) deductionPercentage = 100;
            else if (hoursUntilCheckIn < 24) deductionPercentage = 50;
            else if (hoursUntilCheckIn < 72) deductionPercentage = 25;
            else deductionPercentage = 0;

            booking.CancellationDeduction = (booking.TotalPrice * deductionPercentage) / 100;
            booking.RefundAmount = booking.TotalPrice - booking.CancellationDeduction;
            booking.Status = "Cancelled";
            
            await _context.SaveChangesAsync();

            // Publish Cancellation Event for Email notifications
            await _publish.Publish(new BookingCancelledEvent
            {
                BookingId = booking.BookingId,
                RoomId = booking.RoomId,
                NumberOfRooms = booking.NumberOfRooms,
                UserEmail = booking.UserEmail,
                HotelName = booking.HotelName,
                RefundAmount = booking.RefundAmount ?? 0,
                DeductionAmount = booking.CancellationDeduction ?? 0,
                CancelledAt = DateTime.UtcNow
            });

            return await MapToResponse(booking);
        }

        public async Task<BookingResponseDto> CompleteBookingAsync(int id, int userId, string role)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                throw new BookingNotFoundException(id);

            // 🔐 Security: Managers can complete bookings for their hotels
            if (role != "Admin")
            {
                var roomIds = await _hotelClient.GetManagerRoomIdsAsync();
                if (roomIds == null || !roomIds.Contains(booking.RoomId))
                {
                    throw new UnauthorizedAccessException("You can only complete bookings for your own properties.");
                }
            }

            if (booking.Status == "Completed")
                return await MapToResponse(booking);

            booking.Status = "Completed";
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
                Status = b.Status,
                CancellationDeduction = b.CancellationDeduction,
                RefundAmount = b.RefundAmount
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