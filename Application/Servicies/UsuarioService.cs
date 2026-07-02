using Application.DTOs;
using Application.Interfaces;
using Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Servicies
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repo;

       
        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Obtiene todos los usuarios registrados en el sistema.
        /// </summary>
        /// <returns> Lista de usuarios en formato DTO.</returns>
        public List<UsuarioDTO> ObtenerUsuarios()
        {
            return _repo.ObtenerTodos()
            .Select(u => new UsuarioDTO
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                Correo = u.Correo,
                Telefono = u.Telefono,
                Ubicacion = u.Ubicacion,
                FotoPerfil = u.FotoPerfil
            }).ToList();
        }

        /// <summary>
        /// Obtiene un usuario específico por su identificador.
        /// </summary>
        /// <param name="id">ID del usuario.</param>
        /// <returns>Usuario en formato DTO o null si no existe.</returns>
       
        public UsuarioDTO? ObtenerUsuarioPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.", nameof(id));

            var usuario = _repo.ObtenerPorId(id);

            if (usuario == null) return null;

            return new UsuarioDTO
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                Ubicacion = usuario.Ubicacion
            };
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="usuario">Entidad usuario a guardar.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el usuario es nulo.</exception>
        /// <exception cref="ArgumentException">Se lanza si los datos son inválidos.</exception>
        public void GuardarUsuarioService(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario), "El usuario no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(usuario.Correo))
                throw new ArgumentException("El correo del usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(usuario.PasswordHash))
                throw new ArgumentException("La contraseña del usuario es obligatoria.");

            // Se delega el guardado al repositorio
            _repo.GuardarUsuario(usuario);
        }

        /// <summary>
        /// Actualiza la información de un usuario existente.
        /// </summary>
        /// <param name="usuario">Entidad usuario con los datos actualizados.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el usuario es nulo.</exception>
        /// <exception cref="ArgumentException">Se lanza si los datos son inválidos.</exception>
        public void ActualizarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario), "El usuario no puede ser nulo.");

            if (usuario.IdUsuario <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            // Se delega la actualización al repositorio
            _repo.ActualizarUsuario(usuario);
        }

        /// <summary>
        /// Elimina un usuario del sistema.
        /// </summary>
        /// <param name="id">ID del usuario a eliminar.</param>
        /// <exception cref="ArgumentException">Se lanza si el ID es inválido.</exception>
        public void EliminarUsuario(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.", nameof(id));

            // Se delega la eliminación al repositorio
            _repo.EliminarUsuario(id);
        }
    }
}
