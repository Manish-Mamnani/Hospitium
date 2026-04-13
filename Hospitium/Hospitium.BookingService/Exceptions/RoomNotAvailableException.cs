namespace Hospitium.BookingService.Exceptions
{
    /// <summary>
    /// Exception thrown when the requested room is not available or does not exist.
    /// </summary>
    public class RoomNotAvailableException : Exception
    {
        public RoomNotAvailableException(int roomId)
            : base($"Room with ID {roomId} is not available.") { }
    }
}