using System;

namespace Travelio.Application.DTOs
{
    public class PreBookingResponseDto
    {
        public string PreBookingId { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; } = null!;
        public string? ProviderReference { get; set; }
    }
}
