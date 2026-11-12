using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class LoginDTO
    {
        // DTO para el Login del User
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(255)]
        public string Password { get; set; } = null!;
    }
}
