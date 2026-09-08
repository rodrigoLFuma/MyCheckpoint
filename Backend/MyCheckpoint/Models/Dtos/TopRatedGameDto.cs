using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class TopRatedGameDto
    {
        [Required]
        public GameDto Game { get; set; } = null!;
        [Required]
        public double AverageRate { get; set; }
        [Required]
        public int RatingsCount { get; set; }
    }
}
