using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserModifyDto
    {
        [Required]
        public string UserName { get; set; }
        public string? ProfilePictureURL { get; set; }
        public string? Bibliography { get; set; }
        public List<int> FavoriteGenreCategoryIds { get; set; } = new();
    }
}
