using System;

namespace Travelio.Application.DTOs
{
    public class OfferDto
    {
        public string OfferId { get; set; }
        public string Provider { get; set; }
        public string ServiceType { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public double Score { get; set; }
        public int? Availability { get; set; }
        public bool Bookable { get; set; }
    }
}
