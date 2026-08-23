using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserSumaryDto : IDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        public string? ProfilePictureURL { get; set; }

    }
}
