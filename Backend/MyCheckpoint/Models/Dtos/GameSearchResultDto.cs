using MyCheckpoint.Services.External;

namespace MyCheckpoint.Models.Dtos
{
    public class GameSearchResultDto
    {
        public List<RawgGameSummary> Results { get; set; } = new();
        public bool HasMore { get; set; }
    }
}
