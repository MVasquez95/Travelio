using System.Text.Json;
using System.Net.Http.Json;

namespace Travelio.Infrastructure.Services;

public sealed class ProviderAdapter
{
    private readonly IHttpClientFactory _clients;

    public ProviderAdapter(IHttpClientFactory clients) => _clients = clients;

    public async Task<(bool Success, string? Reference, int HoldSeconds)> HoldAsync(string provider, string providerOfferId, CancellationToken token)
    {
        var endpoint = provider == "provider1" ? "/prebook" : "/reserve";
        var response = await _clients.CreateClient(provider).PostAsJsonAsync(endpoint, new { offerId = providerOfferId }, token);
        if (!response.IsSuccessStatusCode) return (false, null, 0);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
        var root = document.RootElement;
        var success = provider == "provider1" ? root.GetProperty("success").GetBoolean() : root.GetProperty("ok").GetBoolean();
        if (!success) return (false, null, 0);
        return provider == "provider1"
            ? (true, root.GetProperty("prebooking_id").GetString(), root.GetProperty("hold_seconds").GetInt32())
            : (true, root.GetProperty("reservation_ref").GetString(), root.GetProperty("expires_in").GetInt32());
    }

    public async Task<(bool Success, string? Reference)> ConfirmAsync(string provider, string holdReference, string idempotencyKey, CancellationToken token)
    {
        var endpoint = provider == "provider1" ? "/book" : "/confirm";
        var response = await _clients.CreateClient(provider).PostAsJsonAsync(endpoint, new { prebookingId = holdReference, idempotencyKey }, token);
        if (!response.IsSuccessStatusCode) return (false, null);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
        var root = document.RootElement;
        var success = provider == "provider1" ? root.GetProperty("success").GetBoolean() : root.GetProperty("ok").GetBoolean();
        return success
            ? (true, provider == "provider1" ? root.GetProperty("booking_id").GetString() : root.GetProperty("booking_ref").GetString())
            : (false, null);
    }

    public async Task<bool> CancelAsync(string provider, string reference, CancellationToken token)
    {
        var endpoint = provider == "provider1" ? "/cancel" : "/revoke";
        var response = await _clients.CreateClient(provider).PostAsJsonAsync(endpoint, new { reference }, token);
        return response.IsSuccessStatusCode;
    }
}
