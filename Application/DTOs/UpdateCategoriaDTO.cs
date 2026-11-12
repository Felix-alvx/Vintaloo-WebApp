using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateCategoriaDTO
    {
        [Range(1, int.MaxValue)]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Descripcion { get; set; }
    }
}
