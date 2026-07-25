using System;

namespace Travelio.Domain.Models
{
    public class IdempotencyKey
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string ClientId { get; set; }
        public string RequestHash { get; set; }
        public Guid? ResultBookingId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }
}
