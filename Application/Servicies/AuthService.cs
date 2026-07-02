using Application.DTOs;
using Application.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Servicies
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IConfiguration _config;

        public AuthService(IUsuarioRepository usuarioRepo, IConfiguration config)
        {
            _usuarioRepo = usuarioRepo;
            _config = config;
        }

        public string? Login(LoginDTO dto)
        {
            // 1. Buscar usuario por correo
            var usuario = _usuarioRepo.ObtenerPorCorreo(dto.Correo);
            if (usuario == null) return null;

            // 2. Verificar contraseña con BCrypt
            
            bool passwordValido = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
            if (!passwordValido) return null;

            // 3. Obtener rol del usuario
            var rol = _usuarioRepo.ObtenerRol(usuario.IdUsuario) ?? "usuario";

            // 4. Generar JWT
            return GenerarToken(usuario.IdUsuario, usuario.Nombre, usuario.Correo, rol);
        }

        public bool Registrar(RegisterDTO dto)
        {
            // Verificar que el correo no exista
            var existente = _usuarioRepo.ObtenerPorCorreo(dto.Correo);
            if (existente != null) return false;

            // Hash de la contraseña con BCrypt
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            _usuarioRepo.GuardarUsuario(new Domain.Data.Usuario
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                PasswordHash = hash,
                Telefono = dto.Telefono,
                Ubicacion = dto.Ubicacion,
                FotoPerfil = ""
            });

            // Asignar rol "usuario" por defecto
            var nuevo = _usuarioRepo.ObtenerPorCorreo(dto.Correo);
            if (nuevo != null)
                _usuarioRepo.AsignarRol(nuevo.IdUsuario, 2); // id_rol=2 es "usuario"

            return true;
        }

        private string GenerarToken(int idUsuario, string nombre, string correo, string rol)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var horas = int.Parse(_config["Jwt:ExpiresInHours"] ?? "8");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
                new Claim(ClaimTypes.Name,           nombre),
                new Claim(ClaimTypes.Email,          correo),
                new Claim(ClaimTypes.Role,           rol)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(horas),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
