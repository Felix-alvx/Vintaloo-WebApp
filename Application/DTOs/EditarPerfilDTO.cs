using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class EditarPerfilDTO
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = null!;

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        public string? Ubicacion { get; set; }
    }
}
