namespace Hospitium.BookingService.Exceptions
{
    /// <summary>
    /// Exception thrown when the requested number of rooms exceeds available capacity for the given date range.
    /// </summary>
    public class BookingConflictException : Exception
    {
        public BookingConflictException()
            : base("Room is already booked for the selected dates.") { }
    }
}
