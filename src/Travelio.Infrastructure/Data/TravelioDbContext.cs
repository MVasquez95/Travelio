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
        public DbSet<Search> Searches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Provider>().ToTable("providers");
            modelBuilder.Entity<Offer>().ToTable("offers_normalized");
            modelBuilder.Entity<PreBooking>().ToTable("pre_bookings");
            modelBuilder.Entity<Booking>().ToTable("bookings");
            modelBuilder.Entity<IdempotencyKey>().ToTable("idempotency_keys");
            modelBuilder.Entity<Search>().ToTable("searches");

            modelBuilder.Entity<Offer>().Property(x => x.RawJson).HasColumnType("jsonb");
            modelBuilder.Entity<Booking>().Property(x => x.Metadata).HasColumnType("jsonb");
            modelBuilder.Entity<Search>().Property(x => x.Criteria).HasColumnName("search_criteria").HasColumnType("jsonb");
            modelBuilder.Entity<IdempotencyKey>().HasIndex(x => new { x.ClientId, x.Key }).IsUnique();
            modelBuilder.Entity<Booking>().HasIndex(x => x.PreBookingId).IsUnique();

            modelBuilder.Entity<Booking>().HasIndex(b => new { b.ClientId, b.IdempotencyKey }).IsUnique();
            modelBuilder.Entity<PreBooking>().HasOne(x => x.Offer).WithMany().HasForeignKey(x => x.OfferId);
            modelBuilder.Entity<Booking>().HasOne(x => x.PreBooking).WithMany().HasForeignKey(x => x.PreBookingId);
        }
    }
}
