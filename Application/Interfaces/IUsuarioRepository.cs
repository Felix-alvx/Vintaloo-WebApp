using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUsuarioRepository
    {
        // Data Acces to database 
        public List<Usuario> ObtenerTodos();
        public Usuario? ObtenerPorId(int id);

        public void GuardarUsuario(Usuario usuario);

        public void ActualizarUsuario(Usuario usuario);

        public void EliminarUsuario(int id);

        public Usuario? ObtenerPorCorreo(string correo);
        public string? ObtenerRol(int idUsuario);
        public void AsignarRol(int idUsuario, int idRol);
    }
}
