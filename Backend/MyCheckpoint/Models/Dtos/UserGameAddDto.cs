using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserGameAddDto
    {
        [Required]
        public int GameId { get; set; }
        [Required]
        public string Status { get; set; }
        // Sin [Required]: un juego "Pendiente" no tiene puntuación todavía.
        // UserGameService la ignora igualmente si Status es "Pendiente".
        public int? Rate { get; set; }
        public string? Title { get; set; }
        [MaxLength(2000)]
        public string? Review { get; set; }
        public DateTime? PlayedDate { get; set; }
    }
}
