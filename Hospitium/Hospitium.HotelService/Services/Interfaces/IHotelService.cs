using Hospitium.HotelService.Models;
using Hospitium.HotelService.DTOs;

namespace Hospitium.HotelService.Services.Interfaces
{
    public interface IHotelService
    {
        Task<HotelResponseDto> CreateHotelAsync(int userId, CreateHotelDto dto);
        Task<HotelResponseDto> UpdateHotelAsync(int id, CreateHotelDto dto);
        Task<HotelResponseDto> ApproveHotelAsync(int hotelId);
        Task<HotelResponseDto> RejectHotelAsync(int hotelId);
        Task DeleteHotelAsync(int id);
        Task<HotelResponseDto> GetHotelByIdAsync(int id);
        Task<List<HotelResponseDto>> GetApprovedHotelsAsync();
        Task<List<HotelResponseDto>> GetPendingHotelsAsync();
        Task<List<HotelResponseDto>> GetMyHotelsAsync(int userId);
        Task<RoomResponseDto> CreateRoomAsync(int userId, CreateRoomDto dto);
        Task<RoomResponseDto> UpdateRoomAsync(int roomId, int userId, UpdateRoomDto dto);
        Task<RoomResponseDto> GetRoomByIdAsync(int roomId);
        Task<List<RoomResponseDto>> GetRoomsByHotelIdAsync(int hotelId);
        Task<HotelResponseDto> AddRatingAsync(int hotelId, double rating);
    }
}
