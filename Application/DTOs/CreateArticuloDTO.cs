using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateArticuloDTO
    {
        // DTO para craear un articulo
        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }
        public string Titulo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }
        public string EstadoProducto { get; set; } = null!;
        public string? Ubicacion { get; set; }
        public List<string> Imagenes { get; set; } = new();
    }
}
