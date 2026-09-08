using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class RankedUserDto
    {
        [Required]
        public UserSummaryDto User { get; set; } = null!;
        [Required]
        public int FollowerCount { get; set; }
    }
}
