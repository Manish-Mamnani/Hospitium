namespace Hospitium.BookingService.Exceptions
{
    /// <summary>
    /// Exception thrown when the booking FromDate is not earlier than the ToDate.
    /// </summary>
    public class InvalidBookingDatesException : Exception
    {
        public InvalidBookingDatesException()
            : base("FromDate must be earlier than ToDate.") { }
    }
}