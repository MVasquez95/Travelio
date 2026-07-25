using System;

namespace Travelio.Domain.Models
{
    public class PreBooking
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public Guid? SearchId { get; set; }
        public Guid? OfferId { get; set; }
        public string ProviderPrebookingId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; }
        public string LockToken { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
