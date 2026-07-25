using System.Threading.Tasks;
using Travelio.Application.DTOs;

namespace Travelio.Application.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResponseDto> SearchAsync(SearchRequestDto request);
    }
}
