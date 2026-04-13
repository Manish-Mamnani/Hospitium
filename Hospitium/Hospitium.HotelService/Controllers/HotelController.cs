using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Exceptions;
using Hospitium.HotelService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.HotelService.Controllers
{
    /// <summary>
    /// Controller for managing hotel and room operations, including CRUD, image uploads, and administrative approval workflows.
    /// </summary>
    [ApiController]
    [Route("api/hotels")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IHotelQueryService _queryService;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelController"/> class.
        /// </summary>
        /// <param name="hotelService">The hotel management service.</param>
        /// <param name="queryService">The hotel query and search service.</param>
        /// <param name="environment">The web host environment for file storage operations.</param>
        public HotelController(
            IHotelService hotelService,
            IHotelQueryService queryService,
            IWebHostEnvironment environment)
        {
            _hotelService = hotelService;
            _queryService = queryService;
            _environment = environment;
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

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllHotelsForAdmin()
        {
            var result = await _hotelService.GetAllHotelsAsync();
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
        public async Task<IActionResult> GetRoom(int roomId)
        {
            var result = await _hotelService.GetRoomByIdAsync(roomId);
            return Ok(result);
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


        [Authorize(Roles = "Admin,HotelManager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, CreateHotelDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst("Role")!.Value;
            var result = await _hotelService.UpdateHotelAsync(id, userId, role, dto);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,HotelManager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst("Role")!.Value;
            await _hotelService.DeleteHotelAsync(id, userId, role);
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

        [Authorize(Roles = "HotelManager")]
        [HttpGet("my/room-ids")]
        public async Task<IActionResult> GetMyRoomIds()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var result = await _hotelService.GetMyRoomIdsAsync(userId);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,HotelManager")]
        [HttpPost("{id}/images")]
        public async Task<IActionResult> UploadHotelImages(int id, [FromForm] List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return BadRequest("No images uploaded.");

            if (images.Count > 10)
                return BadRequest("Maximum limit of 10 images per request.");

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst("Role")!.Value;

            var uploadedImages = new List<HotelImageResponseDto>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            long maxFileSize = 5 * 1024 * 1024; // 5 MB

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "hotels");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            bool isFirstUpload = true; // Temporary flag for Primary image
            foreach (var file in images)
            {
                if (file.Length == 0) continue;

                if (file.Length > maxFileSize)
                    return BadRequest($"File {file.FileName} exceeds the 5MB limit.");

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    return BadRequest($"File {file.FileName} has an invalid extension.");

                var uniqueFileName = $"{id}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/images/hotels/{uniqueFileName}";
                var result = await _hotelService.AddHotelImageAsync(id, userId, role, imageUrl, isFirstUpload);
                uploadedImages.Add(result);
                isFirstUpload = false;
            }

            return Ok(uploadedImages);
        }

        [Authorize(Roles = "Admin,HotelManager")]
        [HttpDelete("{id}/images/{imageId}")]
        public async Task<IActionResult> DeleteHotelImage(int id, int imageId)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var role = User.FindFirst("Role")!.Value;

            // In a real app, you should fetch the image URL and delete physical file as well
            // For now we remove db record
            await _hotelService.DeleteHotelImageAsync(id, imageId, userId, role);
            return NoContent();
        }
    }
}