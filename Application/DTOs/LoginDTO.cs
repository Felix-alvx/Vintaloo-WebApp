using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class LoginDTO
    {
        // DTO para el Login del User 
        public string Correo { get; set; }  = null!; 
        public string Password { get; set; }  = null!; 
    }
}
