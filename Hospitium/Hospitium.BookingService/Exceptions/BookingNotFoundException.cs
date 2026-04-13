namespace Hospitium.BookingService.Exceptions
{
    /// <summary>
    /// Exception thrown when a booking with the specified ID cannot be found.
    /// </summary>
    public class BookingNotFoundException : Exception
    {
        public BookingNotFoundException(int id)
            : base($"Booking with ID {id} not found.") { }
    }
}
