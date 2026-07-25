using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Travelio.Application.DTOs;
using Travelio.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Travelio.Infrastructure.Services
{
    public class SearchService : ISearchService
    {
        private readonly IHttpClientFactory _clients;
        private readonly ILogger<SearchService> _logger;

        public SearchService(IHttpClientFactory clients, ILogger<SearchService> logger)
        {
            _clients = clients;
            _logger = logger;
        }

        public async Task<SearchResponseDto> SearchAsync(Travelio.Application.DTOs.SearchRequestDto request)
        {
            var providers = new[] { "provider1", "provider2" };
            var offers = new List<OfferDto>();
            foreach (var p in providers)
            {
                try
                {
                    var client = _clients.CreateClient(p);
                    var resp = await client.PostAsJsonAsync("/search", request);
                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Provider {p} returned {status}", p, resp.StatusCode);
                        continue;
                    }
                    var json = await resp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    // Normalize for known providers
                    if (p == "provider1" && root.TryGetProperty("offers", out var arr))
                    {
                        foreach (var it in arr.EnumerateArray())
                        {
                            offers.Add(new OfferDto
                            {
                                OfferId = it.GetProperty("id").GetString(),
                                Provider = "provider1",
                                ServiceType = it.GetProperty("type").GetString(),
                                Price = it.GetProperty("price").GetDecimal(),
                                Currency = it.GetProperty("currency").GetString(),
                                Availability = it.GetProperty("availability").GetInt32(),
                                Score = 0.5,
                                Bookable = it.GetProperty("availability").GetInt32() > 0
                            });
                        }
                    }
                    else if (p == "provider2")
                    {
                        // provider2 uses 'results' and cost structure
                        if (root.TryGetProperty("results", out var arr2))
                        {
                            foreach (var it in arr2.EnumerateArray())
                            {
                                offers.Add(new OfferDto
                                {
                                    OfferId = it.GetProperty("offer_code").GetString(),
                                    Provider = "provider2",
                                    ServiceType = it.GetProperty("category").GetString(),
                                    Price = it.GetProperty("cost").GetProperty("value").GetDecimal(),
                                    Currency = it.GetProperty("cost").GetProperty("curr").GetString(),
                                    Availability = it.GetProperty("seats").GetInt32(),
                                    Score = 0.4,
                                    Bookable = it.GetProperty("seats").GetInt32() > 0
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling provider {p}", p);
                }
            }

            // Simple sorting by score then price
            var sorted = offers.OrderByDescending(o => o.Score).ThenBy(o => o.Price).ToList();
            return new SearchResponseDto { SearchId = Guid.NewGuid().ToString(), Offers = sorted, CacheTtlSeconds = 30 };
        }
    }
}
