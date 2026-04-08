using Hospitium.ReviewService.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospitium.ReviewService.Data
{
    public class ReviewDbContext : DbContext
    {
        public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Any specific configurations for Review entity can go here
        }
    }
}
