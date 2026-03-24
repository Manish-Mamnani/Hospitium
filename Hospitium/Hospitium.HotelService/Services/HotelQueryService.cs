using Hospitium.HotelService.Data;
using Hospitium.HotelService.DTOs;
using Hospitium.HotelService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.HotelService.Services
{
    public class HotelQueryService : IHotelQueryService
    {
        private readonly HotelDbContext _context;

        public HotelQueryService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<List<HotelResponseDto>> GetHotelsAsync(HotelQueryParams queryParams)
        {
            var query = _context.Hotels
                .Include(h => h.Rooms)
                .AsQueryable();

            //Only approved hotels
            query = query.Where(h => h.Status == "Approved");

            // Search
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.ToLower();
                query = query.Where(h => h.Name.ToLower().Contains(search));
            }

            // City filter
            if (!string.IsNullOrWhiteSpace(queryParams.City))
            {
                var city = queryParams.City.ToLower();
                query = query.Where(h => h.City.ToLower() == city);
            }

            // Price filter
            if (queryParams.MinPrice.HasValue)
            {
                query = query.Where(h => h.Rooms.Any() &&
                                         h.Rooms.Min(r => r.Price) >= queryParams.MinPrice.Value);
            }

            if (queryParams.MaxPrice.HasValue)
            {
                query = query.Where(h => h.Rooms.Any() &&
                                         h.Rooms.Min(r => r.Price) <= queryParams.MaxPrice.Value);
            }

            // Availability
            if (queryParams.AvailableOnly == true)
            {
                query = query.Where(h => h.Rooms.Any(r => r.AvailableCount > 0));
            }

            // Sorting
            query = ApplySorting(query, queryParams.SortBy, queryParams.Order);

            // Pagination
            var skip = (queryParams.Page - 1) * queryParams.PageSize;
            query = query.Skip(skip).Take(queryParams.PageSize);

            // Projection
            return await query.Select(h => new HotelResponseDto
            {
                HotelId = h.HotelId,
                Name = h.Name,
                City = h.City,
                Status = h.Status,
                Rating = h.AverageRating,
                MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.Price) : 0
            }).ToListAsync();
        }

        private IQueryable<Models.Hotel> ApplySorting(IQueryable<Models.Hotel> query, string? sortBy, string? order)
        {
            var isDescending = order?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "name" => isDescending
                    ? query.OrderByDescending(h => h.Name)
                    : query.OrderBy(h => h.Name),

                "price" => isDescending
                    ? query.OrderByDescending(h => h.Rooms.Any() ? h.Rooms.Min(r => r.Price) : decimal.MaxValue)
                    : query.OrderBy(h => h.Rooms.Any() ? h.Rooms.Min(r => r.Price) : decimal.MaxValue),

                "rating" => isDescending
                    ? query.OrderByDescending(h => h.AverageRating)
                    : query.OrderBy(h => h.AverageRating),

                _ => query.OrderBy(h => h.HotelId)
            };
        }
    }
}