namespace Hospitium.BookingService.Exceptions
{
    public class InvalidBookingDatesException : Exception
    {
        public InvalidBookingDatesException()
            : base("FromDate must be earlier than ToDate.") { }
    }
}