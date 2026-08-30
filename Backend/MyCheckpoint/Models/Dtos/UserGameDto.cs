using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserGameDto : IDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Status { get; set; }
        public int? Rate { get; set; }
        public string? Review { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? PlayedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [Required]
        public GameDto Game { get; set; }
        [Required]
        public UserSummaryDto UserSummary { get; set; }
    }
}
