using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Travelio.Application.DTOs;
using Travelio.Application.Interfaces;
using Travelio.Infrastructure.Data;
using Travelio.Domain.Models;

namespace Travelio.Infrastructure.Services
{
    public class PreBookingService : IPreBookingService
    {
        private readonly TravelioDbContext _db;
        private readonly ILogger<PreBookingService> _logger;

        public PreBookingService(TravelioDbContext db, ILogger<PreBookingService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<PreBookingResponseDto> CreatePreBookingAsync(PreBookingRequestDto request)
        {
            // This implementation is intentionally simple for the MVP: it persists a prebooking record and returns it.
            var pb = new PreBooking
            {
                ClientId = request.ClientId,
                OfferId = Guid.TryParse(request.OfferId, out var g) ? g : (Guid?)null,
                SearchId = Guid.TryParse(request.SearchId, out var s) ? s : (Guid?)null,
                ProviderPrebookingId = null,
                ExpiresAt = DateTime.UtcNow.AddSeconds(request.HoldTimeSeconds ?? 300),
                Status = "reserved",
            };
            _db.PreBookings.Add(pb);
            await _db.SaveChangesAsync();

            return new PreBookingResponseDto
            {
                PreBookingId = pb.Id.ToString(),
                ExpiresAt = pb.ExpiresAt,
                Status = pb.Status,
                ProviderReference = pb.ProviderPrebookingId
            };
        }
    }
}
