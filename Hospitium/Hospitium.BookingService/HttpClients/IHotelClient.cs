using Hospitium.BookingService.DTOs;

namespace Hospitium.BookingService.HttpClients
{
    public interface IHotelClient
    {
        Task<(bool IsAvailable, decimal Price)> CheckRoomAvailabilityAndPrice(int roomId, int requestedRooms);
        Task<RoomResponseDto?> GetRoomDetailsAsync(int roomId);
        Task<List<int>> GetManagerRoomIdsAsync();
    }
}