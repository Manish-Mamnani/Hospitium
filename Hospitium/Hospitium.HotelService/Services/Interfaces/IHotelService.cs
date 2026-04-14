using Hospitium.HotelService.DTOs;
using Microsoft.AspNetCore.Http;

namespace Hospitium.HotelService.Services.Interfaces
{
    public interface IHotelService
    {
        Task<HotelResponseDto> CreateHotelAsync(int userId, string email, CreateHotelDto dto);
        Task<HotelResponseDto> UpdateHotelAsync(int id, int userId, string role, CreateHotelDto dto);
        Task<HotelResponseDto> ApproveHotelAsync(int hotelId);
        Task<HotelResponseDto> RejectHotelAsync(int hotelId);
        Task DeleteHotelAsync(int id, int userId, string role);
        Task<HotelResponseDto> GetHotelByIdAsync(int id);
        Task<List<HotelResponseDto>> GetApprovedHotelsAsync();
        Task<List<HotelResponseDto>> GetPendingHotelsAsync();
        Task<List<HotelResponseDto>> GetMyHotelsAsync(int userId);
        Task<List<HotelResponseDto>> GetAllHotelsAsync();
        Task<RoomResponseDto> CreateRoomAsync(int userId, CreateRoomDto dto);
        Task<RoomResponseDto> UpdateRoomAsync(int roomId, int userId, UpdateRoomDto dto);
        Task<RoomResponseDto> GetRoomByIdAsync(int roomId);
        Task<List<RoomResponseDto>> GetRoomsByHotelIdAsync(int hotelId);
        Task<List<int>> GetMyRoomIdsAsync(int userId);
        Task<HotelImageResponseDto> AddHotelImageAsync(int hotelId, int userId, string role, string imageUrl, bool isPrimary);
        Task<List<HotelImageResponseDto>> UploadHotelImagesAsync(int hotelId, int userId, string role, List<IFormFile> images);
        Task DeleteHotelImageAsync(int hotelId, int imageId, int userId, string role);
    }
}
