namespace MyCheckpoint.Services.External
{
    public interface IRawgClient
    {
        Task<IEnumerable<RawgGameSummary>> SearchAsync(string term);
        Task<RawgGameDetail?> GetByIdAsync(int rawgId);
    }
}
