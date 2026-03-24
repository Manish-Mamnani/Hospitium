using System.Text.Json;

namespace Hospitium.BookingService.HttpClients
{
    public class HotelClient : IHotelClient
    {
        private readonly HttpClient _httpClient;

        public HotelClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> IsRoomAvailable(int roomId)
        {
            var response = await _httpClient.GetAsync($"/api/hotels/rooms/{roomId}");

            if (!response.IsSuccessStatusCode)
                return false;

            var content = await response.Content.ReadAsStringAsync();

            var room = JsonSerializer.Deserialize<RoomResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return room != null && room.AvailableCount > 0;
        }

        private class RoomResponse
        {
            public int RoomId { get; set; }
            public int AvailableCount { get; set; }
        }
    }
}