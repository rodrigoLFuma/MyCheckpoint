using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class CategoryModifyDto
    {
        [Required]
        public string Name { get; set; }
    }
}
