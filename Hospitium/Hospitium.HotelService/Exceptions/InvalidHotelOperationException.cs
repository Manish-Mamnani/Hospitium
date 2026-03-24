namespace Hospitium.HotelService.Exceptions
{
    public class InvalidHotelOperationException : Exception
    {
        public InvalidHotelOperationException(string message) : base(message) { }
    }
}