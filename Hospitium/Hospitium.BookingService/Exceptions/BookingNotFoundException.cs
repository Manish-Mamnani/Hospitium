namespace Hospitium.BookingService.Exceptions
{
    public class BookingNotFoundException : Exception
    {
        public BookingNotFoundException(int id)
            : base($"Booking with ID {id} not found.") { }
    }
}
