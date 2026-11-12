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
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Correo) ||
                string.IsNullOrWhiteSpace(dto.Password))
                return null;

            // 1. Buscar usuario por correo
            var usuario = _usuarioRepo.ObtenerPorCorreo(dto.Correo);
            if (usuario == null) return null;

            // 2. Verificar contraseña con BCrypt
            bool passwordValido;
            try
            {
                passwordValido = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
            }
            catch
            {
                // Hash inválido o corrupto: no autenticar.
                return null;
            }

            if (!passwordValido) return null;

            // 3. Rechazar cuentas inactivas
            if (!string.IsNullOrWhiteSpace(usuario.Estado) &&
                !usuario.Estado.Equals("activo", StringComparison.OrdinalIgnoreCase))
                return null;

            // 4. Obtener rol del usuario
            var rol = _usuarioRepo.ObtenerRol(usuario.IdUsuario) ?? "usuario";

            // 4. Generar JWT
            return GenerarToken(usuario.IdUsuario, usuario.Nombre, usuario.Correo, rol);
        }

        public bool Registrar(RegisterDTO dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Correo) ||
                !EsPasswordSegura(dto.Password))
                return false;

            if (!string.IsNullOrWhiteSpace(dto.ConfirmPassword) &&
                dto.Password != dto.ConfirmPassword)
                return false;

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

        public string? GenerarTokenRecuperacion(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return null;

            var usuario = _usuarioRepo.ObtenerPorCorreo(correo);
            if (usuario == null)
                return null;

            if (!string.IsNullOrWhiteSpace(usuario.Estado) &&
                !usuario.Estado.Equals("activo", StringComparison.OrdinalIgnoreCase))
                return null;

            var key = ObtenerClaveFirma();
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim("purpose", "password-reset"),
                new Claim("pwd", ObtenerHuellaPassword(usuario.PasswordHash))
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool RestablecerPassword(string token, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(token) || !EsPasswordSegura(nuevaPassword))
                return false;

            ClaimsPrincipal principal;
            try
            {
                principal = new JwtSecurityTokenHandler().ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = _config["Jwt:Audience"],
                        IssuerSigningKey = ObtenerClaveFirma(),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    },
                    out _);
            }
            catch
            {
                return false;
            }

            if (principal.FindFirst("purpose")?.Value != "password-reset")
                return false;

            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null || !int.TryParse(idClaim.Value, out var idUsuario))
                return false;

            var usuario = _usuarioRepo.ObtenerPorId(idUsuario);
            if (usuario == null)
                return false;

            if (!string.IsNullOrWhiteSpace(usuario.Estado) &&
                !usuario.Estado.Equals("activo", StringComparison.OrdinalIgnoreCase))
                return false;

            var huellaToken = principal.FindFirst("pwd")?.Value;
            if (string.IsNullOrWhiteSpace(huellaToken) ||
                huellaToken != ObtenerHuellaPassword(usuario.PasswordHash))
                return false;

            _usuarioRepo.ActualizarPassword(idUsuario, BCrypt.Net.BCrypt.HashPassword(nuevaPassword));
            return true;
        }

        private string GenerarToken(int idUsuario, string nombre, string correo, string rol)
        {
            var key = ObtenerClaveFirma();
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

        private SymmetricSecurityKey ObtenerClaveFirma()
        {
            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("La clave JWT 'Jwt:Key' no está configurada.");

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }

        private static string ObtenerHuellaPassword(string passwordHash)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes(passwordHash));
            return Convert.ToHexString(bytes)[..16];
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
