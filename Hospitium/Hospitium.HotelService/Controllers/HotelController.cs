using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Exceptions;
using Hospitium.HotelService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [Authorize(Roles = "HotelManager")]
        [HttpPost]
        public async Task<IActionResult> CreateHotel(CreateHotelDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var email = User.FindFirst("Email")!.Value;
            var result = await _hotelService.CreateHotelAsync(userId, email,dto);
            return Ok(result);
        }

        // 🔹 Create Room
        [Authorize(Roles = "HotelManager")]
        [HttpPost("rooms")]
        public async Task<IActionResult> CreateRoom(CreateRoomDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var result = await _hotelService.CreateRoomAsync(userId, dto);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveHotel(int id)
        {
            var result = await _hotelService.ApproveHotelAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectHotel(int id)
        {
            var result = await _hotelService.RejectHotelAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedHotels()
        {
            var result = await _hotelService.GetApprovedHotelsAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingHotels()
        {
            var result = await _hotelService.GetPendingHotelsAsync();
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

        [HttpGet("rooms/{roomId}")]
        public async Task<IActionResult> GetRoomById(int roomId)
        {
            var room = await _hotelService.GetRoomByIdAsync(roomId);

            return Ok(new RoomAvailabilityDto
            {
                RoomId = room.RoomId,
                AvailableCount = room.AvailableCount
            });
        }

        [Authorize(Roles = "HotelManager")]
        [HttpPut("rooms/{roomId}")]
        public async Task<IActionResult> UpdateRoom(int roomId, UpdateRoomDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var result = await _hotelService.UpdateRoomAsync(roomId, userId, dto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotel(int id)
        {
            var result = await _hotelService.GetHotelByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/rooms")]
        public async Task<IActionResult> GetRooms(int id)
        {
            var result = await _hotelService.GetRoomsByHotelIdAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, CreateHotelDto dto)
        {
            var result = await _hotelService.UpdateHotelAsync(id, dto);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            await _hotelService.DeleteHotelAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "HotelManager")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyHotels()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);

            var result = await _hotelService.GetMyHotelsAsync(userId);

            return Ok(result);
        }

        // ⭐ Rating
        [Authorize]
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateHotel(int id, AddRatingDto dto)
        {
            var result = await _hotelService.AddRatingAsync(id, dto.Rating);
            return Ok(result);
        }
    }
}