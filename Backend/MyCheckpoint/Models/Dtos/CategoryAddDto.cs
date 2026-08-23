using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class CategoryAddDto
    {
        [Required]
        public string Name { get; set; }
    }
}
