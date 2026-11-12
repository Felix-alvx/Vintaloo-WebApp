using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = null!;
    }
}
