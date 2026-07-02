using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PerfilDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Ubicacion { get; set; }
        public string? FotoPerfil { get; set; }

        // Artículos publicados por el usuario
        public List<ArticuloDTO> Articulos { get; set; } = new();
    }
}
