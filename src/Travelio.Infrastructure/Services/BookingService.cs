using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Travelio.Application.DTOs;
using Travelio.Application.Interfaces;
using Travelio.Domain.Models;
using Travelio.Infrastructure.Data;

namespace Travelio.Infrastructure.Services;

public sealed class BookingService : IBookingService
{
    private readonly TravelioDbContext _db;
    private readonly ProviderAdapter _provider;
    public BookingService(TravelioDbContext db, ProviderAdapter provider) => (_db, _provider) = (db, provider);

    public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, string idempotencyKey)
    {
        var hash = Hash(JsonSerializer.Serialize(request));
        var existingKey = await _db.IdempotencyKeys.SingleOrDefaultAsync(x => x.ClientId == request.ClientId && x.Key == idempotencyKey);
        if (existingKey is not null)
        {
            if (existingKey.RequestHash != hash) throw new InvalidOperationException("Idempotency-Key was already used with another payload.");
            if (existingKey.ResultBookingId is not null)
                return ToResponse(await _db.Bookings.FindAsync(existingKey.ResultBookingId.Value) ?? throw new InvalidOperationException("Booking result is missing."));
            throw new InvalidOperationException("This request is already being processed.");
        }
        if (!Guid.TryParse(request.PreBookingId, out var preBookingId)) throw new InvalidOperationException("PreBookingId is required.");

        var preBooking = await _db.PreBookings.Include(x => x.Offer).SingleOrDefaultAsync(x => x.Id == preBookingId)
            ?? throw new KeyNotFoundException("Pre-booking not found.");
        if (preBooking.ClientId != request.ClientId) throw new InvalidOperationException("Pre-booking does not belong to this client.");
        if (preBooking.Status != "held" || preBooking.ExpiresAt <= DateTime.UtcNow) throw new InvalidOperationException("Pre-booking has expired or was already used.");
        if (preBooking.Offer is null || string.IsNullOrWhiteSpace(preBooking.ProviderPrebookingId)) throw new InvalidOperationException("Pre-booking is incomplete.");

        var booking = new Booking { Id = Guid.NewGuid(), ClientId = request.ClientId, PreBookingId = preBooking.Id, IdempotencyKey = idempotencyKey, Status = "pending", Amount = preBooking.Offer.PriceAmount, Currency = preBooking.Offer.Currency, Metadata = "{}" };
        var key = new IdempotencyKey { ClientId = request.ClientId, Key = idempotencyKey, RequestHash = hash, ResultBookingId = booking.Id, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        preBooking.Status = "confirming";
        _db.Bookings.Add(booking);
        _db.IdempotencyKeys.Add(key);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException) { throw new InvalidOperationException("A booking already exists for this pre-booking or idempotency key."); }

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var confirmation = await _provider.ConfirmAsync(preBooking.Offer.ProviderCode, preBooking.ProviderPrebookingId, idempotencyKey, timeout.Token);
            booking.Status = confirmation.Success ? "confirmed" : "failed";
            booking.ProviderBookingId = confirmation.Reference;
            preBooking.Status = confirmation.Success ? "confirmed" : "held";
        }
        catch (TaskCanceledException)
        {
            booking.Status = "unknown"; // A worker must reconcile an uncertain provider outcome.
            preBooking.Status = "unknown";
        }
        await _db.SaveChangesAsync();
        return ToResponse(booking);
    }

    public async Task<BookingResponseDto?> GetBookingAsync(string bookingId)
        => Guid.TryParse(bookingId, out var id) ? (await _db.Bookings.FindAsync(id) is { } booking ? ToResponse(booking) : null) : null;

    public async Task<BookingResponseDto> CancelBookingAsync(string bookingId)
    {
        if (!Guid.TryParse(bookingId, out var id)) throw new KeyNotFoundException("Booking not found.");
        var booking = await _db.Bookings.Include(x => x.PreBooking).ThenInclude(x => x!.Offer).SingleOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException("Booking not found.");
        if (booking.Status != "confirmed" || booking.PreBooking?.Offer is null || string.IsNullOrWhiteSpace(booking.ProviderBookingId)) throw new InvalidOperationException("Booking cannot be cancelled.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        if (!await _provider.CancelAsync(booking.PreBooking.Offer.ProviderCode, booking.ProviderBookingId, timeout.Token)) throw new InvalidOperationException("Provider rejected the cancellation.");
        booking.Status = "cancelled";
        await _db.SaveChangesAsync();
        return ToResponse(booking);
    }

    private static BookingResponseDto ToResponse(Booking booking) => new() { BookingId = booking.Id.ToString(), Status = booking.Status, ProviderBookingId = booking.ProviderBookingId, Amount = booking.Amount, Currency = booking.Currency };
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
