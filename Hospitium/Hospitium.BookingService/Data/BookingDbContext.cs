using Hospitium.BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.BookingService.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }

    }
}
