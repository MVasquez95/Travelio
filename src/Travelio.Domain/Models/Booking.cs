using System;

namespace Travelio.Domain.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public Guid? PreBookingId { get; set; }
        public string ProviderBookingId { get; set; }
        public string IdempotencyKey { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
