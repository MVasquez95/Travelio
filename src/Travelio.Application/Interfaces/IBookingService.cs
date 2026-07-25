using System.Threading.Tasks;
using Travelio.Application.DTOs;

namespace Travelio.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, string idempotencyKey);
        Task<BookingResponseDto?> GetBookingAsync(string bookingId);
        Task<BookingResponseDto> CancelBookingAsync(string bookingId);
    }
}
