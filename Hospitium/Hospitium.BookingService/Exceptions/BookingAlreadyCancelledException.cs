namespace Hospitium.BookingService.Exceptions
{
    public class BookingAlreadyCancelledException : Exception
    {
        public BookingAlreadyCancelledException(int id)
            : base($"Booking {id} is already cancelled.") { }
    }
}
