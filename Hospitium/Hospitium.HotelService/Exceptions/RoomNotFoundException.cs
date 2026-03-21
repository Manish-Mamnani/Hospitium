namespace Hospitium.HotelService.Exceptions
{
    public class RoomNotFoundException : Exception
    {
        public RoomNotFoundException(int id)
            : base($"Room with ID {id} not found.") { }
    }
}