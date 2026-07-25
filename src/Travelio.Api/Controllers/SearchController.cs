using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travelio.Application.Interfaces;
using Travelio.Application.DTOs;

namespace Travelio.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchRequestDto request)
        {
            var resp = await _searchService.SearchAsync(request);
            return Ok(resp);
        }
    }
}
