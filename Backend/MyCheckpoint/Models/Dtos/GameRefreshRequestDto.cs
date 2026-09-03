using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class GameRefreshRequestDto
    {
        [Required]
        public List<int> GameIds { get; set; } = new();
    }
}
