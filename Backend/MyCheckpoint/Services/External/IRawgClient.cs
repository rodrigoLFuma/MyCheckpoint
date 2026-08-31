namespace MyCheckpoint.Services.External
{
    public interface IRawgClient
    {
        Task<(IEnumerable<RawgGameSummary> Results, bool HasMore)> SearchAsync(string? term, string? genres, int page);
        Task<RawgGameDetail?> GetByIdAsync(int rawgId);
        Task<IEnumerable<RawgGenreSummary>> GetGenresAsync();
    }
}
