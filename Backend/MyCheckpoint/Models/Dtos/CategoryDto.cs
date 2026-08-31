using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class CategoryDto : IDto
    {
        [Required]
        public int Id { get ; set ; }
        [Required]
        public string Name { get; set; }
        public string? RawgSlug { get; set; }
    }
}
