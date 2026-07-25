using System;
using System.Text.Json.Serialization;

namespace Travelio.Application.DTOs
{
    public class OfferDto
    {
        public string OfferId { get; set; }
        [JsonIgnore]
        public string? ProviderOfferId { get; set; }
        public string Provider { get; set; }
        public string ServiceType { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public double Score { get; set; }
        public int? Availability { get; set; }
        public bool Bookable { get; set; }
        [JsonIgnore]
        public string? RawJson { get; set; }
    }
}
