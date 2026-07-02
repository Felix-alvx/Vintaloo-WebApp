using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UpdateUsuarioDTO
    {
        // DTO para actuzalizar el Usuario

        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Ubicacion { get; set; }
    }
}
