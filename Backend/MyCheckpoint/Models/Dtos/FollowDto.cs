using MyCheckpoint.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class FollowDto 
    {
        [Required]
        public UserSumaryDto Follower { get; set; }
        [Required]
        public UserSumaryDto Followed { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

    }
}
