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

        public async Task<(IEnumerable<RawgGameSummary> Results, bool HasMore)> SearchAsync(string? term, string? genres, int page)
        {
            var query = $"games?key={_apiKey}&page={page}";
            if (!string.IsNullOrWhiteSpace(term))
                query += $"&search={Uri.EscapeDataString(term)}";
            if (!string.IsNullOrWhiteSpace(genres))
                query += $"&genres={Uri.EscapeDataString(genres)}";

            var response = await _httpClient.GetFromJsonAsync<RawgSearchResponse>(query);

            var results = response?.Results ?? Enumerable.Empty<RawgGameSummary>();
            var hasMore = !string.IsNullOrEmpty(response?.Next);
            return (results, hasMore);
        }

        public async Task<RawgGameDetail?> GetByIdAsync(int rawgId)
        {
            var response = await _httpClient.GetAsync($"games/{rawgId}?key={_apiKey}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RawgGameDetail>();
        }

        public async Task<IEnumerable<RawgGenreSummary>> GetGenresAsync()
        {
            // RAWG solo tiene ~19 géneros en total; con page_size=40 entran
            // todos en una sola página, sin tener que paginar.
            var response = await _httpClient.GetFromJsonAsync<RawgGenreListResponse>(
                $"genres?page_size=40&key={_apiKey}");

            return response?.Results ?? Enumerable.Empty<RawgGenreSummary>();
        }
    }
}
