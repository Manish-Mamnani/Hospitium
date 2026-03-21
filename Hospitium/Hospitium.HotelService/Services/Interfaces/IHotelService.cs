using Hospitium.HotelService.Models;
using Hospitium.HotelService.DTOs;

namespace Hospitium.HotelService.Services.Interfaces
{
    public interface IHotelService
    {
        Task<HotelResponseDto> CreateHotelAsync(CreateHotelDto dto);
        Task<Object> CreateRoomAsync(CreateRoomDto dto);
    }
}
