using Hospitium.BookingService.DTOs;
using Hospitium.BookingService.Models;

namespace Hospitium.BookingService.Interfaces
{
    /// <summary>
    /// Defines the contract for booking management operations.
    /// </summary>
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(int userId,string email, CreateBookingDto dto);
        Task<List<BookingResponseDto>> GetUserBookingsAsync(int userId, string? type);
        Task<List<BookingResponseDto>> GetAllBookingsAsync(DateTime? date);
        Task<List<BookingResponseDto>> GetManagerBookingsAsync();
        Task<BookingResponseDto> CancelBookingAsync(int bookingId, int userId, string email, string role);
        Task<BookingResponseDto> CompleteBookingAsync(int bookingId, int userId, string role);
        Task<BookingResponseDto> GetBookingByIdAsync(int bookingId, int userId, string role);
    }
}