using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserGameAddDto
    {
        [Required]
        public int GameId { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public int Rate { get; set; }
        public string? Review { get; set; }
        public DateTime? PlayedDate { get; set; }
    }
}
