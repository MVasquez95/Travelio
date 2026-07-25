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
            try { await ExpireHoldsAsync(stoppingToken); }
            catch (Exception exception) { logger.LogError(exception, "Pre-booking expiration cycle failed"); }
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
}
