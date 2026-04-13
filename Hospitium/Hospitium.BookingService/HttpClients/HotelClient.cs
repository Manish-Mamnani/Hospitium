using System;
using System.Linq;
using System.Text.Json;
using Hospitium.BookingService.DTOs;

namespace Hospitium.BookingService.HttpClients
{
    /// <summary>
    /// HTTP client implementation for interacting with the HotelService, forwarding JWT auth headers.
    /// </summary>
    public class HotelClient : IHotelClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelClient"/> class.
        /// </summary>
        /// <param name="httpClient">The underlying HTTP client configured for the HotelService base address.</param>
        /// <param name="httpContextAccessor">Accessor for the current HTTP context to forward the Authorization header.</param>
        public HotelClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeader()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
            }
        }

        public async Task<List<int>> GetManagerRoomIdsAsync()
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync("/api/hotels/my/room-ids");

            if (!response.IsSuccessStatusCode)
                return new List<int>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<int>>(content) ?? new List<int>();
        }

        public async Task<(bool IsAvailable, decimal Price)> CheckRoomAvailabilityAndPrice(int roomId, int requestedRooms)
        {
            var room = await GetRoomDetailsAsync(roomId);

            if (room != null && room.AvailableCount >= requestedRooms)
            {
                return (true, room.Price);
            }

            return (false, 0);
        }

        public async Task<RoomResponseDto?> GetRoomDetailsAsync(int roomId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/hotels/rooms/{roomId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    return null;

                return JsonSerializer.Deserialize<RoomResponseDto>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                return null; 
            }
        }

    }
}