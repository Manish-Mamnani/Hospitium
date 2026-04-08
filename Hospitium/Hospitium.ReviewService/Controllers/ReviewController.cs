using Hospitium.ReviewService.DTOs;
using Hospitium.ReviewService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospitium.ReviewService.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddReview(CreateReviewDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var userName = User.FindFirst("Name")?.Value ?? User.FindFirst("Email")?.Value ?? "Guest";

            var result = await _reviewService.AddReviewAsync(userId, userName, dto);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("rate")]
        public async Task<IActionResult> RateHotel([FromQuery] int hotelId, [FromQuery] int rating)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var userName = User.FindFirst("Name")?.Value ?? User.FindFirst("Email")?.Value ?? "Guest";

            var result = await _reviewService.AddRatingAsync(userId, userName, hotelId, rating);
            return Ok(result);
        }

        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetReviewsByHotel(int hotelId)
        {
            var result = await _reviewService.GetReviewsByHotelIdAsync(hotelId);
            return Ok(result);
        }
    }
}
