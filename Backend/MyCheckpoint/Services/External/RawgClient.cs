using System.Net;
using System.Net.Http.Json;

namespace MyCheckpoint.Services.External
{
    public class RawgClient : IRawgClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public RawgClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Rawg:ApiKey"] ?? string.Empty;
        }

        public async Task<IEnumerable<RawgGameSummary>> SearchAsync(string term)
        {
            var response = await _httpClient.GetFromJsonAsync<RawgSearchResponse>(
                $"games?search={Uri.EscapeDataString(term)}&key={_apiKey}");

            return response?.Results ?? Enumerable.Empty<RawgGameSummary>();
        }

        public async Task<RawgGameDetail?> GetByIdAsync(int rawgId)
        {
            var response = await _httpClient.GetAsync($"games/{rawgId}?key={_apiKey}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RawgGameDetail>();
        }
    }
}
