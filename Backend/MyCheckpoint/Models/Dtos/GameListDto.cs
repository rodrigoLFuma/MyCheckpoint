using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class GameListDto : IDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public bool IsPublic { get; set; }
        [Required]
        public UserSummaryDto User {  get; set; }
        public List<GameListItemDto> Games { get; set; }

    }
}
