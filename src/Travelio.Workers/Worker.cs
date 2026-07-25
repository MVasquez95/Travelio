using Microsoft.EntityFrameworkCore;
using Travelio.Infrastructure.Data;
using Travelio.Infrastructure.Services;

namespace Travelio.Workers;

public sealed class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireHoldsAsync(stoppingToken);
                await ReconcileUnknownBookingsAsync(stoppingToken);
            }
            catch (Exception exception) { logger.LogError(exception, "Background reconciliation cycle failed"); }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ExpireHoldsAsync(CancellationToken token)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TravelioDbContext>();
        var provider = scope.ServiceProvider.GetRequiredService<ProviderAdapter>();
        var expired = await db.PreBookings.Include(x => x.Offer)
            .Where(x => x.Status == "held" && x.ExpiresAt <= DateTime.UtcNow)
            .Take(100).ToListAsync(token);
        foreach (var hold in expired)
        {
            if (hold.Offer is not null && !string.IsNullOrWhiteSpace(hold.ProviderPrebookingId))
                await provider.CancelAsync(hold.Offer.ProviderCode, hold.ProviderPrebookingId, token);
            hold.Status = "expired";
            hold.UpdatedAt = DateTime.UtcNow;
        }
        if (expired.Count > 0)
        {
            await db.SaveChangesAsync(token);
            logger.LogInformation("Expired {Count} pre-bookings", expired.Count);
        }
    }

    private async Task ReconcileUnknownBookingsAsync(CancellationToken token)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TravelioDbContext>();
        var provider = scope.ServiceProvider.GetRequiredService<ProviderAdapter>();
        var bookings = await db.Bookings.Include(x => x.PreBooking).ThenInclude(x => x!.Offer)
            .Where(x => x.Status == "unknown").Take(100).ToListAsync(token);
        foreach (var booking in bookings)
        {
            var hold = booking.PreBooking;
            if (hold?.Offer is null || string.IsNullOrWhiteSpace(hold.ProviderPrebookingId)) continue;
            var result = await provider.ConfirmAsync(hold.Offer.ProviderCode, hold.ProviderPrebookingId, booking.IdempotencyKey, token);
            booking.Status = result.Success ? "confirmed" : "failed";
            booking.ProviderBookingId = result.Reference;
            hold.Status = result.Success ? "confirmed" : "held";
        }
        if (bookings.Count > 0) await db.SaveChangesAsync(token);
    }
}
