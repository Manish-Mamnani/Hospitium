namespace Hospitium.BookingService.HttpClients
{
    public interface IHotelClient
    {
        Task<bool> IsRoomAvailable(int roomId);
    }
}