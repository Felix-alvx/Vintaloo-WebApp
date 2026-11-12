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
                FotoPerfil = u.FotoPerfil,
                Estado = u.Estado,
                FechaRegistro = u.FechaRegistro,
                Rol = _repo.ObtenerRol(u.IdUsuario) ?? "usuario"
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
                Ubicacion = usuario.Ubicacion,
                FotoPerfil = usuario.FotoPerfil,
                Estado = usuario.Estado,
                FechaRegistro = usuario.FechaRegistro,
                Rol = _repo.ObtenerRol(usuario.IdUsuario) ?? "usuario"
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

            // Preservar datos que no forman parte del formulario de perfil
            var existente = _repo.ObtenerPorId(usuario.IdUsuario);
            if (existente != null)
            {
                if (string.IsNullOrWhiteSpace(usuario.FotoPerfil))
                    usuario.FotoPerfil = existente.FotoPerfil;

                if (string.IsNullOrWhiteSpace(usuario.Estado))
                    usuario.Estado = existente.Estado;
            }

            // Se delega la actualización al repositorio
            _repo.ActualizarUsuario(usuario);
        }

        /// <summary>
        /// Elimina un usuario del sistema.
        /// </summary>
        /// <param name="id">ID del usuario a eliminar.</param>
        /// <exception cref="ArgumentException">Se lanza si el ID es inválido.</exception>
        public bool CrearUsuario(CreateUsuarioDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "El usuario no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Correo))
                throw new ArgumentException("El correo del usuario es obligatorio.");

            if (!EsPasswordSegura(dto.Password))
                throw new ArgumentException("La contraseña no cumple con la política de seguridad.");

            if (dto.IdRol != 1 && dto.IdRol != 2)
                throw new ArgumentException("El rol seleccionado no es válido.");

            if (_repo.ObtenerPorCorreo(dto.Correo) != null)
                return false;

            _repo.GuardarUsuario(new Usuario
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Telefono = dto.Telefono,
                Ubicacion = dto.Ubicacion,
                FotoPerfil = ""
            });

            var nuevo = _repo.ObtenerPorCorreo(dto.Correo);
            if (nuevo == null) return false;

            _repo.AsignarRol(nuevo.IdUsuario, dto.IdRol);
            return true;
        }

        public void ActualizarUsuarioAdmin(UpdateUsuarioDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "El usuario no puede ser nulo.");

            if (dto.IdUsuario <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            if (dto.IdRol != 1 && dto.IdRol != 2)
                throw new ArgumentException("El rol seleccionado no es válido.");

            var existente = _repo.ObtenerPorId(dto.IdUsuario)
                ?? throw new ArgumentException("El usuario no existe.");

            _repo.ActualizarUsuario(new Usuario
            {
                IdUsuario = dto.IdUsuario,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Ubicacion = dto.Ubicacion,
                Estado = dto.Estado,
                FotoPerfil = existente.FotoPerfil
            });

            _repo.QuitarRoles(dto.IdUsuario);
            _repo.AsignarRol(dto.IdUsuario, dto.IdRol);
        }

        public void EliminarUsuario(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor a 0.", nameof(id));

            // Se delega la eliminación al repositorio
            _repo.EliminarUsuario(id);
        }

        private static bool EsPasswordSegura(string? password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 100)
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(
                password,
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$");
        }
    }
}
