namespace Hospitium.HotelService.Exceptions
{
    public class HotelNotFoundException : Exception
    {
        public HotelNotFoundException(int id)
            : base($"Hotel with ID {id} not found.") { }
    }
}