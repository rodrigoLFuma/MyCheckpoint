using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class FollowAddDto
    {
        [Required]
        public int FollowedId { get; set; }
    }
}
