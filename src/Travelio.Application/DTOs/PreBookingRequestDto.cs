using System.ComponentModel.DataAnnotations;

namespace Travelio.Application.DTOs
{
    public class PreBookingRequestDto
    {
        [Required] public string ClientId { get; set; } = null!;
        [Required] public string OfferId { get; set; } = null!;
        public string? SearchId { get; set; }
    }
}
