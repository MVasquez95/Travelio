namespace Travelio.Application.DTOs
{
    public class BookingResponseDto
    {
        public string BookingId { get; set; }
        public string Status { get; set; }
        public string ProviderBookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
