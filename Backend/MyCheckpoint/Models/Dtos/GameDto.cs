using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class GameDto : IDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime ReleaseDate { get; set; }
        public int? RawgId { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int? Metacritic { get; set; }
        public List<CategoryDto> Categories { get; set; }
    }
}
