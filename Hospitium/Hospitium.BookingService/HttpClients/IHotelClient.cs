using Hospitium.BookingService.DTOs;

namespace Hospitium.BookingService.HttpClients
{
    /// <summary>
    /// Defines the contract for communicating with the HotelService to retrieve room and availability data.
    /// </summary>
    public interface IHotelClient
    {
        Task<(bool IsAvailable, decimal Price)> CheckRoomAvailabilityAndPrice(int roomId, int requestedRooms);
        Task<RoomResponseDto?> GetRoomDetailsAsync(int roomId);
        Task<List<int>> GetManagerRoomIdsAsync();
    }
}