using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class CategoryTopUsersDto
    {
        [Required]
        public CategoryDto Category { get; set; } = null!;
        public List<UserSummaryDto> TopUsers { get; set; } = new();
    }
}
