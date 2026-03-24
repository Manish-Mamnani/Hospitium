namespace Hospitium.BookingService.Exceptions
{
    public class BookingConflictException : Exception
    {
        public BookingConflictException()
            : base("Room is already booked for the selected dates.") { }
    }
}
