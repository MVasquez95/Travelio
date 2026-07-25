using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Travelio.Infrastructure.Data;
using Travelio.Domain.Models;

namespace Travelio.Api.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;

        public IdempotencyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TravelioDbContext db)
        {
            // Only intercept booking creation endpoint
            if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                && context.Request.Path.StartsWithSegments("/api/v1/bookings", StringComparison.OrdinalIgnoreCase))
            {
                if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idKey) || string.IsNullOrWhiteSpace(idKey))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { message = "Idempotency-Key header is required" });
                    return;
                }

                // Read and hash request body for basic payload fingerprinting
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                string requestHash;
                using (var sha = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
                    var hash = sha.ComputeHash(bytes);
                    requestHash = Convert.ToHexString(hash);
                }

                // Try insert idempotency record; if exists handle accordingly
                IdempotencyKey existing = null;
                try
                {
                    var ike = new IdempotencyKey
                    {
                        Key = idKey,
                        ClientId = null,
                        RequestHash = requestHash,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(1)
                    };
                    db.IdempotencyKeys.Add(ike);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Unique constraint violated - load existing
                    existing = await db.IdempotencyKeys.FirstOrDefaultAsync(k => k.Key == idKey);
                }

                if (existing == null)
                {
                    // If we just created the entry, load it to check later in pipeline if needed
                    existing = await db.IdempotencyKeys.FirstOrDefaultAsync(k => k.Key == idKey);
                }

                if (existing != null)
                {
                    // If there is already a result booking, return it
                    if (existing.ResultBookingId.HasValue)
                    {
                        var booking = await db.Bookings.FindAsync(existing.ResultBookingId.Value);
                        if (booking != null)
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                bookingId = booking.Id,
                                status = booking.Status,
                                providerBookingId = booking.ProviderBookingId,
                                amount = booking.Amount,
                                currency = booking.Currency
                            });
                            return;
                        }
                    }

                    // There's an idempotency entry but no result yet - another request is being processed.
                    // To keep the implementation simple for this MVP, return 409 Conflict indicating in-flight.
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new { message = "Idempotency key already exists and is being processed" });
                    return;
                }
            }

            await _next(context);
        }
    }
}
