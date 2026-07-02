using Infrastructure.Data;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Data;

namespace Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly VintalooDbContext _context;

        public UsuarioRepository(VintalooDbContext context)
        {
            _context = context;

        }


        // Método para obtener todos los articulos

        public List<Usuario> ObtenerTodos() {

            return _context.Usuarios
                .FromSqlRaw("EXEC sp_insertar_usuario")
                .ToList(); 
        }

        // Metodo que llama al sp y obtiene los usuarios según el id especifico
        public Usuario? ObtenerPorId(int id)
        {
            return _context.Usuarios
                .FromSqlRaw("EXEC sp_obtener_usuarios_porId @id_usuario = {0}", id)
                .AsEnumerable()
                .FirstOrDefault();
        }

        // Método para insertar un Usuario

        public void GuardarUsuario(Usuario usuario)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_insertar_usuario @nombre={0}, @correo={1}, @password_hash={2}, @telefono={3}, @ubicacion={4}, @foto_perfil={5}",
                usuario.Nombre,
                usuario.Correo,
                usuario.PasswordHash,
                usuario.Telefono,
                usuario.Ubicacion,
                usuario.FotoPerfil
            );
        }


        // Método de Actualizar usuario
        public void ActualizarUsuario(Usuario usuario)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_actualizar_usuario @id_usuario={0}, @nombre={1}, @telefono={2}, @ubicacion={3}, @foto_perfil={4}, @estado={5}",
                usuario.IdUsuario,
                usuario.Nombre,
                usuario.Telefono,
                usuario.Ubicacion,
                usuario.FotoPerfil,
                usuario.Estado
            );
        }

        // Método de Eliminar usuario
        public void EliminarUsuario(int id)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_eliminar_usuario @id_usuario={0}",
                id
            );
        }


        // ------ Métodos Auth ----

        public Usuario? ObtenerPorCorreo(string correo)
        {
            return _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo);
        }

        // Obtener rol del usuario
        public string? ObtenerRol(int idUsuario)
        {
            return _context.Database
                .SqlQueryRaw<string>(
                    "SELECT r.nombre FROM roles r " +
                    "INNER JOIN usuario_roles ur ON r.id_rol = ur.id_rol " +
                    "WHERE ur.id_usuario = {0}", idUsuario)
                .AsEnumerable()
                .FirstOrDefault();
        }

        // Asignar rol
        public void AsignarRol(int idUsuario, int idRol)
        {
            _context.Database.ExecuteSqlRaw(
                "INSERT INTO usuario_roles (id_usuario, id_rol) VALUES ({0}, {1})",
                idUsuario, idRol);
        }
    }
}
