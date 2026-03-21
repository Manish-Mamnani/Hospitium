using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospitium.HotelService.Controllers
{
    [ApiController]
    [Route("api/hotels")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IHotelQueryService _queryService;

        public HotelController(
            IHotelService hotelService,
            IHotelQueryService queryService)
        {
            _hotelService = hotelService;
            _queryService = queryService;
        }

        // 🔹 Create Hotel
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateHotel(CreateHotelDto dto)
        {
            var result = await _hotelService.CreateHotelAsync(dto);
            return Ok(result);
        }

        // 🔹 Create Room
        [Authorize(Roles = "Admin")]
        [HttpPost("rooms")]
        public async Task<IActionResult> CreateRoom(CreateRoomDto dto)
        {
            var result = await _hotelService.CreateRoomAsync(dto);
            return Ok(result);
        }

        // 🔹 Get Hotels (Search + Filter + Sort + Pagination)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetHotels([FromQuery] HotelQueryParams query)
        {
            var result = await _queryService.GetHotelsAsync(query);
            return Ok(result);
        }
    }
}