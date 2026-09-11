using MyCheckpoint.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class FollowDto
    {
        [Required]
        public UserSummaryDto Follower { get; set; }
        [Required]
        public UserSummaryDto Followed { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

    }
}
