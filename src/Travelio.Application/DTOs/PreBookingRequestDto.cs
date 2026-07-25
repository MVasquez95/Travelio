namespace Travelio.Application.DTOs
{
    public class PreBookingRequestDto
    {
        public string ClientId { get; set; }
        public string OfferId { get; set; }
        public string SearchId { get; set; }
        public int? HoldTimeSeconds { get; set; }
    }
}
