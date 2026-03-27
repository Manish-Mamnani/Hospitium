using Hospitium.BookingService.DTOs;
using Hospitium.BookingService.Models;

namespace Hospitium.BookingService.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(int userId,string email, CreateBookingDto dto);
        Task<List<BookingResponseDto>> GetUserBookingsAsync(int userId, string? type);
        Task<List<BookingResponseDto>> GetAllBookingsAsync(DateTime? date);
        Task<BookingResponseDto> CancelBookingAsync(int bookingId, int userId, string email, string role);
        Task<BookingResponseDto> GetBookingByIdAsync(int bookingId, int userId, string role);
    }
}