using Hospitium.HotelService.DTOs;

namespace Hospitium.HotelService.Services.Interfaces
{
    public interface IHotelQueryService
    {
        Task<List<HotelResponseDto>> GetHotelsAsync(HotelQueryParams query);
    }
}
