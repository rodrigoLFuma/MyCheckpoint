using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserGameModifyDto
    {
        [Required]
        public string Status { get; set; }
        public int? Rate { get; set; }
        public string? Title { get; set; }
        [MaxLength(2000)]
        public string? Review { get; set; }
        public DateTime? PlayedDate { get; set; }
    }
}
