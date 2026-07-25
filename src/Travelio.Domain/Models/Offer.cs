using System;

namespace Travelio.Domain.Models
{
    public class Offer
    {
        public Guid Id { get; set; }
        public Guid? ProviderId { get; set; }
        public string ProviderOfferId { get; set; }
        public string ServiceType { get; set; }
        public decimal PriceAmount { get; set; }
        public string Currency { get; set; }
        public int? AvailabilityCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RawJson { get; set; }
        public string SearchHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
