using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class UserModifyDto
    {
        [Required]
        public string UserName { get; set; }

        // La foto de perfil NO se edita aquí: tiene sus propios endpoints
        // (POST/DELETE me/profile-picture) porque es un archivo, no un campo
        // de texto. Así se evita mezclar "subir una imagen" con "pegar
        // cualquier URL", que era el comportamiento anterior.
        public string? Bibliography { get; set; }
        public List<int> FavoriteGenreCategoryIds { get; set; } = new();
    }
}
