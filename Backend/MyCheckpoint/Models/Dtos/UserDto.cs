using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserDto : IDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        public string? ProfilePictureURL { get; set; }
        public string? Bibliography { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
