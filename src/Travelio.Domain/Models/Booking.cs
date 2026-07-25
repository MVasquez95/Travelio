using System;

namespace Travelio.Domain.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; } = null!;
        public Guid? PreBookingId { get; set; }
        public string? ProviderBookingId { get; set; }
        public string IdempotencyKey { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public PreBooking? PreBooking { get; set; }
    }
}
