using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserStatsDto
    {
        [Required]
        public int GamesCount { get; set; }
        public double? AverageRate { get; set; }
        public string? TopGenre { get; set; }
    }
}
