using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Travelio.Application.Interfaces;
using Travelio.Application.DTOs;

namespace Travelio.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingRequestDto request)
        {
            if (!Request.Headers.TryGetValue("Idempotency-Key", out var idKey) || string.IsNullOrEmpty(idKey))
            {
                return BadRequest(new { message = "Idempotency-Key header is required" });
            }

            try
            {
                var resp = await _bookingService.CreateBookingAsync(request, idKey.ToString());
                return CreatedAtAction(nameof(Get), new { bookingId = resp.BookingId }, resp);
            }
            catch (System.InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> Get(string bookingId)
        {
            var booking = await _bookingService.GetBookingAsync(bookingId);
            return booking is null ? NotFound() : Ok(booking);
        }

        [HttpPost("{bookingId}/cancel")]
        public async Task<IActionResult> Cancel(string bookingId)
        {
            try { return Ok(await _bookingService.CancelBookingAsync(bookingId)); }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }
    }
}
