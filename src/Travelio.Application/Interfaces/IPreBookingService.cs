using System.Threading.Tasks;
using Travelio.Application.DTOs;

namespace Travelio.Application.Interfaces
{
    public interface IPreBookingService
    {
        Task<PreBookingResponseDto> CreatePreBookingAsync(PreBookingRequestDto request);
    }
}
