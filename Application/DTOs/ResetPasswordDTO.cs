using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$",
            ErrorMessage = "La contraseña debe incluir mayúscula, minúscula, número y símbolo.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirma la contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
