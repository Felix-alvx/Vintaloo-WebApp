using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateUsuarioDTO
    {
        // DTO para crear un usuario y que recibe el service 
        public string Nombre { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string Password { get; set; } = null!; // Contraseña sin hash 
        public string? Telefono { get; set; }
        public string? Ubicacion { get; set; }

    }
}
