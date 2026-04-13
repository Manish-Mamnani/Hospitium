namespace Hospitium.BookingService.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to cancel a booking that is already in a cancelled state.
    /// </summary>
    public class BookingAlreadyCancelledException : Exception
    {
        public BookingAlreadyCancelledException(int id)
            : base($"Booking {id} is already cancelled.") { }
    }
}
