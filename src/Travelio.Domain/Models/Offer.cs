using System;

namespace Travelio.Domain.Models
{
    public class Offer
    {
        public Guid Id { get; set; }
        public Guid? ProviderId { get; set; }
        public string ProviderCode { get; set; } = null!;
        public string ProviderOfferId { get; set; } = null!;
        public string ServiceType { get; set; } = null!;
        public decimal PriceAmount { get; set; }
        public string Currency { get; set; } = null!;
        public int? AvailabilityCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RawJson { get; set; } = "{}";
        public string SearchHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
