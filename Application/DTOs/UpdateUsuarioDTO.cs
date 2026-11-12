using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateUsuarioDTO
    {
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = null!;

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        public string? Ubicacion { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20)]
        public string Estado { get; set; } = "activo";

        [Range(1, 2, ErrorMessage = "Selecciona un rol válido.")]
        public int IdRol { get; set; } = 2;
    }
}
