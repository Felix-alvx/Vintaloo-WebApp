using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateArticuloDTO
    {
        // DTO para crear un articulo
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150)]
        public string Titulo { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = null!;

        [Range(0, 99999999.99, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El estado del producto es obligatorio.")]
        [StringLength(20)]
        public string EstadoProducto { get; set; } = null!;

        [StringLength(150)]
        public string? Ubicacion { get; set; }

        public List<string> Imagenes { get; set; } = new();
    }
}
