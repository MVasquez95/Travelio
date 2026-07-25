using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Travelio.Application.DTOs;
using Travelio.Application.Interfaces;
using Travelio.Infrastructure.Data;
using Travelio.Domain.Models;

namespace Travelio.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly TravelioDbContext _db;
        private readonly ILogger<BookingService> _logger;

        public BookingService(TravelioDbContext db, ILogger<BookingService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, string idempotencyKey)
        {
            // Load or create idempotency entry (middleware should have created it, but be resilient)
            var existingKey = await _db.IdempotencyKeys.FirstOrDefaultAsync(k => k.Key == idempotencyKey);

            if (existingKey != null)
            {
                if (existingKey.ResultBookingId.HasValue)
                {
                    var existingBooking = await _db.Bookings.FindAsync(existingKey.ResultBookingId.Value);
                    if (existingBooking != null)
                    {
                        return new BookingResponseDto
                        {
                            BookingId = existingBooking.Id.ToString(),
                            Status = existingBooking.Status,
                            ProviderBookingId = existingBooking.ProviderBookingId,
                            Amount = existingBooking.Amount,
                            Currency = existingBooking.Currency
                        };
                    }
                }

                // If there is an idempotency entry but no result yet, proceed to create a booking and attach result to the existing entry.
                var booking = new Booking
                {
                    ClientId = request.ClientId,
                    PreBookingId = Guid.TryParse(request.PreBookingId, out var pb) ? pb : (Guid?)null,
                    ProviderBookingId = null,
                    IdempotencyKey = idempotencyKey,
                    Status = "pending",
                    Amount = 0m,
                    Currency = "USD",
                    Metadata = null
                };

                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync();

                // Link booking result to existing idempotency entry atomically
                existingKey.ResultBookingId = booking.Id;
                _db.IdempotencyKeys.Update(existingKey);
                await _db.SaveChangesAsync();

                return new BookingResponseDto
                {
                    BookingId = booking.Id.ToString(),
                    Status = booking.Status,
                    ProviderBookingId = booking.ProviderBookingId,
                    Amount = booking.Amount,
                    Currency = booking.Currency
                };
            }

            // If no idempotency entry exists (middleware may not have run), create one and map result.
            var newBooking = new Booking
            {
                ClientId = request.ClientId,
                PreBookingId = Guid.TryParse(request.PreBookingId, out var pb2) ? pb2 : (Guid?)null,
                ProviderBookingId = null,
                IdempotencyKey = idempotencyKey,
                Status = "pending",
                Amount = 0m,
                Currency = "USD",
                Metadata = null
            };
            _db.Bookings.Add(newBooking);
            await _db.SaveChangesAsync();

            var ik = new IdempotencyKey
            {
                Key = idempotencyKey,
                ClientId = request.ClientId,
                RequestHash = null,
                ResultBookingId = newBooking.Id
            };
            _db.IdempotencyKeys.Add(ik);
            await _db.SaveChangesAsync();

            return new BookingResponseDto
            {
                BookingId = newBooking.Id.ToString(),
                Status = newBooking.Status,
                ProviderBookingId = newBooking.ProviderBookingId,
                Amount = newBooking.Amount,
                Currency = newBooking.Currency
            };
        }
    }
}
