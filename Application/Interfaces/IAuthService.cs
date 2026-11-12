using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        public string? Login(LoginDTO dto);
        public bool Registrar(RegisterDTO dto);
        public string? GenerarTokenRecuperacion(string correo);
        public bool RestablecerPassword(string token, string nuevaPassword);

    }
}
