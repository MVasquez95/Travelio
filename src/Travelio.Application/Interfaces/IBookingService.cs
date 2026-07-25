using System.Threading.Tasks;
using Travelio.Application.DTOs;

namespace Travelio.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, string idempotencyKey);
    }
}
