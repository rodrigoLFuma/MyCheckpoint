using System.Text.Json.Serialization;

namespace MyCheckpoint.Services.External
{
    public class RawgSearchResponse
    {
        public List<RawgGameSummary> Results { get; set; } = new();
    }

    public class RawgGameSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? Released { get; set; }
        [JsonPropertyName("background_image")]
        public string? BackgroundImage { get; set; }
    }

    public class RawgGameDetail
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? Released { get; set; }
        [JsonPropertyName("background_image")]
        public string? BackgroundImage { get; set; }
        [JsonPropertyName("description_raw")]
        public string? DescriptionRaw { get; set; }
        public int? Metacritic { get; set; }
        public List<RawgNamedRef> Developers { get; set; } = new();
        public List<RawgNamedRef> Publishers { get; set; } = new();
        public List<RawgGenre> Genres { get; set; } = new();
    }

    public class RawgNamedRef
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class RawgGenre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
