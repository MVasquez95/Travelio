using System.ComponentModel.DataAnnotations;

namespace Travelio.Application.DTOs
{
    public class BookingRequestDto
    {
        [Required] public string ClientId { get; set; } = null!;
        [Required] public string PreBookingId { get; set; } = null!;
    }
}
