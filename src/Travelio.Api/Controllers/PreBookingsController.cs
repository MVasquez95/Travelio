using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travelio.Application.Interfaces;
using Travelio.Application.DTOs;

namespace Travelio.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PreBookingsController : ControllerBase
    {
        private readonly IPreBookingService _preBookingService;

        public PreBookingsController(IPreBookingService preBookingService)
        {
            _preBookingService = preBookingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PreBookingRequestDto request)
        {
            var resp = await _preBookingService.CreatePreBookingAsync(request);
            return CreatedAtAction(nameof(Get), new { id = resp.PreBookingId }, resp);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // For MVP this endpoint is a placeholder. Implement retrieval in next iteration.
            return NotFound();
        }
    }
}
