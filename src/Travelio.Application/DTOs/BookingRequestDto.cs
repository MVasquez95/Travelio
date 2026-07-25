namespace Travelio.Application.DTOs
{
    public class BookingRequestDto
    {
        public string ClientId { get; set; }
        public string PreBookingId { get; set; }
        public string OfferId { get; set; }
        public object Payment { get; set; }
    }
}
