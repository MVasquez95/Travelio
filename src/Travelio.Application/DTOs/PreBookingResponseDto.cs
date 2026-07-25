using System;

namespace Travelio.Application.DTOs
{
    public class PreBookingResponseDto
    {
        public string PreBookingId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; }
        public string ProviderReference { get; set; }
    }
}
