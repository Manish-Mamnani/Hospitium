using Hospitium.HotelService.DTOs;

namespace Hospitium.HotelService.Services.Interfaces
{
    public interface IHotelQueryService
    {
        Task<PaginatedResult<HotelResponseDto>> GetHotelsAsync(HotelQueryParams query);
    }
}
