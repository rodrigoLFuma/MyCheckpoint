using System.ComponentModel.DataAnnotations;

namespace MyCheckpoint.Models.Dtos
{
    public class LoginDto
    {
        // Admite tanto el email como el nombre de usuario en el mismo campo;
        // AuthService.LoginAsync prueba primero por email y si no encuentra
        // nada, por nombre de usuario.
        [Required]
        public string EmailOrUserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
