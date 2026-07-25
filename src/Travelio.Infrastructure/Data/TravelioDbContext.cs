using Microsoft.EntityFrameworkCore;
using Travelio.Domain.Models;

namespace Travelio.Infrastructure.Data
{
    public class TravelioDbContext : DbContext
    {
        public TravelioDbContext(DbContextOptions<TravelioDbContext> options) : base(options) { }

        public DbSet<Provider> Providers { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<PreBooking> PreBookings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Provider>().ToTable("providers");
            modelBuilder.Entity<Offer>().ToTable("offers_normalized");
            modelBuilder.Entity<PreBooking>().ToTable("pre_bookings");
            modelBuilder.Entity<Booking>().ToTable("bookings");
            modelBuilder.Entity<IdempotencyKey>().ToTable("idempotency_keys");

            modelBuilder.Entity<Booking>().HasIndex(b => b.IdempotencyKey).IsUnique(false);
        }
    }
}
