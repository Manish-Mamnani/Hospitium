namespace Hospitium.BookingService.Exceptions
{
    public class RoomNotAvailableException : Exception
    {
        public RoomNotAvailableException(int roomId)
            : base($"Room with ID {roomId} is not available.") { }
    }
}