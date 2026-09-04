using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class NotificationDto : IDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public UserSummaryDto Actor { get; set; }
        public int? UserGameId { get; set; }
        public string? GameName { get; set; }
        [Required]
        public bool IsRead { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
