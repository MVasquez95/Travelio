using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Travelio.Infrastructure.Services
{
    // Very small adapter that calls provider endpoints and returns raw JSON for normalization
    public class ProviderAdapter
    {
        private readonly HttpClient _client;
        private readonly string _name;

        public ProviderAdapter(HttpClient client, string name)
        {
            _client = client;
            _name = name;
        }

        public async Task<JsonElement?> SearchAsync(object request)
        {
            var response = await _client.PostAsJsonAsync("/search", request);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var doc = await JsonDocument.ParseAsync(stream);
            return doc.RootElement;
        }

        public async Task<JsonElement?> PrebookAsync(object request, string endpoint = "/prebook")
        {
            var response = await _client.PostAsJsonAsync(endpoint, request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement;
        }

        public async Task<JsonElement?> BookAsync(object request, string endpoint = "/book")
        {
            var response = await _client.PostAsJsonAsync(endpoint, request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement;
        }
    }
}
