using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class GameListItemAddDto
    {
        [Required]
        public int GameId { get; set; }
    }
}
