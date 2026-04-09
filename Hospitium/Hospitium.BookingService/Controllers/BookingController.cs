using Hospitium.BookingService.DTOs;
using Hospitium.BookingService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospitium.BookingService.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingController(IBookingService service)
        {
            _service = service;
        }

        // 🔐 Only authenticated users can book
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBookingDto dto)
        {
            // 🧠 Extract UserId from JWT
            var userIdClaim = User.FindFirst("UserId");
            var email = User.FindFirst("Email")!.Value;

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var result = await _service.CreateBookingAsync(userId, email, dto);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings([FromQuery] string? type)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);

            var result = await _service.GetUserBookingsAsync(userId,type);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllBookings([FromQuery] DateTime? date)
        {
            var result = await _service.GetAllBookingsAsync(date);

            return Ok(result);
        }

        [Authorize(Roles = "HotelManager")]
        [HttpGet("manager")]
        public async Task<IActionResult> GetManagerBookings()
        {
            var result = await _service.GetManagerBookingsAsync();

            return Ok(result);
        }

        [Authorize]
        [HttpPut("{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst("Role")!.Value;
            var email = User.FindFirst("Email")!.Value;

            var result = await _service.CancelBookingAsync(bookingId, userId, email, role);

            return Ok(result);
        }

        [Authorize(Roles = "HotelManager,Admin")]
        [HttpPut("{bookingId}/complete")]
        public async Task<IActionResult> CompleteBooking(int bookingId)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst("Role")!.Value;

            var result = await _service.CompleteBookingAsync(bookingId, userId, role);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst("Role")!.Value;

            var result = await _service.GetBookingByIdAsync(id, userId, role);

            return Ok(result);
        }
    }
}