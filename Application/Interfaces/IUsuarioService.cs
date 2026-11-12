using Application.DTOs;
using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        public List<UsuarioDTO> ObtenerUsuarios();
        public UsuarioDTO? ObtenerUsuarioPorId(int id);
        public void GuardarUsuarioService(Usuario usuario);
        public void ActualizarUsuario(Usuario usuario);
        public bool CrearUsuario(CreateUsuarioDTO dto);
        public void ActualizarUsuarioAdmin(UpdateUsuarioDTO dto);
        public void EliminarUsuario(int id);

    }
}
