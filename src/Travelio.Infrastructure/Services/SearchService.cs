using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Travelio.Application.DTOs;
using Travelio.Application.Interfaces;
using Travelio.Domain.Models;
using Travelio.Infrastructure.Data;

namespace Travelio.Infrastructure.Services;

public sealed class SearchService : ISearchService
{
    private const int CacheTtlSeconds = 30;
    private readonly IHttpClientFactory _clients;
    private readonly TravelioDbContext _db;
    private readonly IDatabase _cache;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IHttpClientFactory clients, TravelioDbContext db, IConnectionMultiplexer redis, ILogger<SearchService> logger)
        => (_clients, _db, _cache, _logger) = (clients, db, redis.GetDatabase(), logger);

    public async Task<SearchResponseDto> SearchAsync(SearchRequestDto request)
    {
        var cacheKey = $"search:{Hash(JsonSerializer.Serialize(request))}";
        var cached = await _cache.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var cachedResponse = JsonSerializer.Deserialize<SearchResponseDto>(cached!)!;
            return cachedResponse;
        }

        var search = new Search { Criteria = JsonSerializer.Serialize(request) };
        _db.Searches.Add(search);
        await _db.SaveChangesAsync();

        var results = await Task.WhenAll(
            SearchProviderAsync("provider1", request),
            SearchProviderAsync("provider2", request));
        var offers = results.SelectMany(x => x)
            .OrderByDescending(x => x.Score).ThenBy(x => x.Price).ToList();

        foreach (var offer in offers)
        {
            _db.Offers.Add(new Offer
            {
                Id = Guid.Parse(offer.OfferId), ProviderCode = offer.Provider,
                ProviderOfferId = offer.ProviderOfferId!, ServiceType = offer.ServiceType,
                PriceAmount = offer.Price, Currency = offer.Currency, AvailabilityCount = offer.Availability,
                SearchHash = cacheKey, RawJson = offer.RawJson!
            });
        }
        await _db.SaveChangesAsync();

        var response = new SearchResponseDto { SearchId = search.Id.ToString(), Offers = offers, CacheTtlSeconds = CacheTtlSeconds };
        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(response), TimeSpan.FromSeconds(CacheTtlSeconds));
        return response;
    }

    private async Task<IReadOnlyList<OfferDto>> SearchProviderAsync(string provider, SearchRequestDto request)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var response = await _clients.CreateClient(provider).PostAsJsonAsync("/search", request, timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Provider {Provider} returned {StatusCode}", provider, response.StatusCode);
                return [];
            }
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
            return Normalize(provider, document.RootElement);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Provider {Provider} did not answer the search", provider);
            return [];
        }
    }

    private static IReadOnlyList<OfferDto> Normalize(string provider, JsonElement root)
    {
        var source = provider == "provider1" ? root.GetProperty("offers") : root.GetProperty("results");
        return source.EnumerateArray().Select(item =>
        {
            var providerOfferId = provider == "provider1" ? item.GetProperty("id").GetString()! : item.GetProperty("offer_code").GetString()!;
            var availability = provider == "provider1" ? item.GetProperty("availability").GetInt32() : item.GetProperty("seats").GetInt32();
            var price = provider == "provider1" ? item.GetProperty("price").GetDecimal() : item.GetProperty("cost").GetProperty("value").GetDecimal();
            var currency = provider == "provider1" ? item.GetProperty("currency").GetString()! : item.GetProperty("cost").GetProperty("curr").GetString()!;
            return new OfferDto
            {
                OfferId = Guid.NewGuid().ToString(), ProviderOfferId = providerOfferId, Provider = provider,
                ServiceType = provider == "provider1" ? item.GetProperty("type").GetString()! : item.GetProperty("category").GetString()!,
                Price = price, Currency = currency, Availability = availability, Bookable = availability > 0,
                Score = provider == "provider1" ? .5 : .4, RawJson = item.GetRawText()
            };
        }).ToList();
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
