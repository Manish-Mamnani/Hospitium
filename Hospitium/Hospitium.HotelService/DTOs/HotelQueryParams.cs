namespace Hospitium.HotelService.DTOs
{
    public class HotelQueryParams
    {
        public string? City { get; set; }

        public string? Search { get; set; }

        public string? SortBy { get; set; }  // name, price, rating
        public string? Order { get; set; }   // asc, desc

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public bool? AvailableOnly { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
