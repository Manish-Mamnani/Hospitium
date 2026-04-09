using Microsoft.EntityFrameworkCore;
using Hospitium.HotelService.Models;

namespace Hospitium.HotelService.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options)
            : base(options) { }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<HotelImage> HotelImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId);

            modelBuilder.Entity<Room>()
                .Property(r => r.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<HotelImage>()
                .HasOne(i => i.Hotel)
                .WithMany(h => h.Images)
                .HasForeignKey(i => i.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
