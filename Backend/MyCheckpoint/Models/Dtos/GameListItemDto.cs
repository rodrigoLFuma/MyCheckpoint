using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class GameListItemDto
    {
        [Required]
        public GameDto Game { get; set; }
        [Required]
        public DateOnly AddedAt { get; set; }
    }
}
