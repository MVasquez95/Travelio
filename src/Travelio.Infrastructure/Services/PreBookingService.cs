using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Travelio.Application.DTOs;
using Travelio.Application.Interfaces;
using Travelio.Domain.Models;
using Travelio.Infrastructure.Data;

namespace Travelio.Infrastructure.Services;

public sealed class PreBookingService : IPreBookingService
{
    private readonly TravelioDbContext _db;
    private readonly ProviderAdapter _provider;
    private readonly IDatabase _redis;

    public PreBookingService(TravelioDbContext db, ProviderAdapter provider, IConnectionMultiplexer redis)
        => (_db, _provider, _redis) = (db, provider, redis.GetDatabase());

    public async Task<PreBookingResponseDto> CreatePreBookingAsync(PreBookingRequestDto request)
    {
        if (!Guid.TryParse(request.OfferId, out var offerId)) throw new InvalidOperationException("OfferId is invalid.");
        var offer = await _db.Offers.SingleOrDefaultAsync(x => x.Id == offerId) ?? throw new KeyNotFoundException("Offer was not found or has expired.");
        if (offer.AvailabilityCount <= 0) throw new InvalidOperationException("Offer is no longer available.");

        var token = Guid.NewGuid().ToString("N");
        var lockKey = $"lock:offer:{offer.Id}";
        if (!await _redis.StringSetAsync(lockKey, token, TimeSpan.FromSeconds(15), When.NotExists))
            throw new InvalidOperationException("This offer is being reserved. Retry shortly.");
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var hold = await _provider.HoldAsync(offer.ProviderCode, offer.ProviderOfferId, timeout.Token);
            if (!hold.Success) throw new InvalidOperationException("Provider could not hold this offer.");

            var expiresAt = DateTime.UtcNow.AddSeconds(hold.HoldSeconds);
            var preBooking = new PreBooking
            {
                ClientId = request.ClientId, OfferId = offer.Id,
                SearchId = Guid.TryParse(request.SearchId, out var searchId) ? searchId : null,
                ProviderPrebookingId = hold.Reference, ExpiresAt = expiresAt, Status = "held", LockToken = token
            };
            _db.PreBookings.Add(preBooking);
            await _db.SaveChangesAsync();
            return new PreBookingResponseDto { PreBookingId = preBooking.Id.ToString(), ExpiresAt = expiresAt, Status = preBooking.Status, ProviderReference = hold.Reference };
        }
        finally { await _redis.KeyDeleteAsync(lockKey); }
    }
}
