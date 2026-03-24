using Hospitium.Contracts;
using Hospitium.BookingService.Data;
using Hospitium.BookingService.DTOs;
using Hospitium.BookingService.Exceptions;
using Hospitium.BookingService.HttpClients;
using Hospitium.BookingService.Interfaces;
using Hospitium.BookingService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingDbContext _context;
        private readonly IHotelClient _hotelClient;
        private readonly IPublishEndpoint _publishEndpoint;

        public BookingService(BookingDbContext context,IHotelClient hotelClient,IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _hotelClient = hotelClient;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(int userId, CreateBookingDto dto)
        {
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

            // 🟡 Check availability via Hotel Service
            var isAvailable = await _hotelClient.IsRoomAvailable(dto.RoomId);

            if (!isAvailable)
                throw new RoomNotAvailableException(dto.RoomId);

            // 🟢 Create booking
            var booking = new Booking
            {
                UserId = userId,
                RoomId = dto.RoomId,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                Status = "Confirmed"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();


            // 📢 Publish event
            await _publishEndpoint.Publish(new BookingCreatedEvent
            {
                RoomId = booking.RoomId
            });

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                RoomId = booking.RoomId,
                FromDate = booking.FromDate,
                ToDate = booking.ToDate,
                Status = booking.Status
            };
        }

        public async Task<List<BookingResponseDto>> GetUserBookingsAsync(int userId, string? type)
        {
            var query = _context.Bookings.Where(b => b.UserId == userId);

            var now = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(type))
            {
                if (type.ToLower() == "active")
                {
                    query = query.Where(b =>
                        b.Status != "Cancelled" &&
                        b.ToDate >= now);
                }
                else if (type.ToLower() == "history")
                {
                    query = query.Where(b =>
                        b.Status == "Cancelled" ||
                        b.ToDate < now);
                }
            }

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(b => new BookingResponseDto
            {
                BookingId = b.BookingId,
                RoomId = b.RoomId,
                FromDate = b.FromDate,
                ToDate = b.ToDate,
                Status = b.Status
            }).ToList();
        }

        public async Task<List<BookingResponseDto>> GetAllBookingsAsync(DateTime? date)
        {
            var query = _context.Bookings.AsQueryable();

            // 📅 Filter by specific date (if provided)
            if (date.HasValue)
            {
                query = query.Where(b =>
                    date.Value >= b.FromDate && date.Value <= b.ToDate);
            }

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(b => new BookingResponseDto
            {
                BookingId = b.BookingId,
                RoomId = b.RoomId,
                FromDate = b.FromDate,
                ToDate = b.ToDate,
                Status = b.Status
            }).ToList();
        }

        public async Task<BookingResponseDto> CancelBookingAsync(int bookingId, int userId, string role)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
                throw new BookingNotFoundException(bookingId);

            if (booking.UserId != userId && role != "Admin")
                throw new UnauthorizedBookingAccessException();
            // ⚠️ Already cancelled
            if (booking.Status == "Cancelled")
                throw new BookingAlreadyCancelledException(bookingId);

            // 🔄 Update status
            booking.Status = "Cancelled";

            await _context.SaveChangesAsync();

            // 📢 Publish cancellation event
            await _publishEndpoint.Publish(new BookingCancelledEvent
            {
                RoomId = booking.RoomId
            });

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                RoomId = booking.RoomId,
                FromDate = booking.FromDate,
                ToDate = booking.ToDate,
                Status = booking.Status
            };
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(int bookingId, int userId, string role)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
                throw new BookingNotFoundException(bookingId);

            if (booking.UserId != userId && role != "Admin")
                throw new UnauthorizedBookingAccessException();

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                RoomId = booking.RoomId,
                FromDate = booking.FromDate,
                ToDate = booking.ToDate,
                Status = booking.Status
            };
        }
    }
}