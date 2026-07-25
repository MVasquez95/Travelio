using System.Collections.Generic;

namespace Travelio.Application.DTOs
{
    public class SearchResponseDto
    {
        public string SearchId { get; set; } = null!;
        public List<OfferDto> Offers { get; set; } = [];
        public int CacheTtlSeconds { get; set; }
    }
}
