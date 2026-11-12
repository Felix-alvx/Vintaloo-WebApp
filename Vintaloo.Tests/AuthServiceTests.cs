using Application.DTOs;
using Application.Interfaces;
using Application.Servicies;
using Domain.Data;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Vintaloo.Tests
{
    public class AuthServiceTests
    {
        private const string PasswordValida = "Password1!";

        private static IConfiguration CrearConfiguracion()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "VintalooTestsSecretKey_1234567890_ABCDEF",
                    ["Jwt:Issuer"] = "VintalooTests",
                    ["Jwt:Audience"] = "VintalooTestsUsers",
                    ["Jwt:ExpiresInHours"] = "1"
                })
                .Build();
        }

        private static Usuario CrearUsuario(string password = PasswordValida, string estado = "activo")
        {
            return new Usuario
            {
                IdUsuario = 1,
                Nombre = "Admin",
                Correo = "admin@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Estado = estado
            };
        }

        [Fact]
        public void Login_ConCredencialesValidas_DevuelveToken()
        {
            var usuario = CrearUsuario();
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo(usuario.Correo)).Returns(usuario);
            repo.Setup(r => r.ObtenerRol(usuario.IdUsuario)).Returns("admin");

            var service = new AuthService(repo.Object, CrearConfiguracion());

            var token = service.Login(new LoginDTO
            {
                Correo = usuario.Correo,
                Password = PasswordValida
            });

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void Login_ConPasswordIncorrecto_DevuelveNull()
        {
            var usuario = CrearUsuario();
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo(usuario.Correo)).Returns(usuario);

            var service = new AuthService(repo.Object, CrearConfiguracion());

            var token = service.Login(new LoginDTO
            {
                Correo = usuario.Correo,
                Password = "Incorrecta1!"
            });

            Assert.Null(token);
        }

        [Fact]
        public void Login_ConUsuarioInactivo_DevuelveNull()
        {
            var usuario = CrearUsuario(estado: "bloqueado");
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo(usuario.Correo)).Returns(usuario);

            var service = new AuthService(repo.Object, CrearConfiguracion());

            var token = service.Login(new LoginDTO
            {
                Correo = usuario.Correo,
                Password = PasswordValida
            });

            Assert.Null(token);
        }

        [Fact]
        public void Login_ConHashInvalido_DevuelveNull()
        {
            var usuario = CrearUsuario();
            usuario.PasswordHash = "123456";

            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo(usuario.Correo)).Returns(usuario);

            var service = new AuthService(repo.Object, CrearConfiguracion());

            var token = service.Login(new LoginDTO
            {
                Correo = usuario.Correo,
                Password = "123456"
            });

            Assert.Null(token);
        }

        [Theory]
        [InlineData("123456")]
        [InlineData("password1!")]
        [InlineData("PASSWORD1!")]
        [InlineData("Password!")]
        [InlineData("Password1")]
        public void Registrar_ConPasswordDebil_DevuelveFalse(string password)
        {
            var repo = new Mock<IUsuarioRepository>();
            var service = new AuthService(repo.Object, CrearConfiguracion());

            var resultado = service.Registrar(new RegisterDTO
            {
                Nombre = "Nuevo",
                Correo = "nuevo@test.com",
                Password = password,
                ConfirmPassword = password
            });

            Assert.False(resultado);
            repo.Verify(r => r.GuardarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void Registrar_ConCorreoExistente_DevuelveFalse()
        {
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo("nuevo@test.com")).Returns(CrearUsuario());

            var service = new AuthService(repo.Object, CrearConfiguracion());

            var resultado = service.Registrar(new RegisterDTO
            {
                Nombre = "Nuevo",
                Correo = "nuevo@test.com",
                Password = PasswordValida,
                ConfirmPassword = PasswordValida
            });

            Assert.False(resultado);
        }

        [Fact]
        public void Registrar_Valido_HasheaPasswordYAsignaRolUsuario()
        {
            var creado = new Usuario { IdUsuario = 10, Nombre = "Nuevo", Correo = "nuevo@test.com" };
            Usuario? usuarioGuardado = null;
            var repo = new Mock<IUsuarioRepository>();
            repo.SetupSequence(r => r.ObtenerPorCorreo("nuevo@test.com"))
                .Returns((Usuario?)null)
                .Returns(creado);
            repo.Setup(r => r.GuardarUsuario(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => usuarioGuardado = u);

            var service = new AuthService(repo.Object, CrearConfiguracion());

            var resultado = service.Registrar(new RegisterDTO
            {
                Nombre = "Nuevo",
                Correo = "nuevo@test.com",
                Password = PasswordValida,
                ConfirmPassword = PasswordValida
            });

            Assert.True(resultado);
            Assert.NotNull(usuarioGuardado);
            Assert.True(BCrypt.Net.BCrypt.Verify(PasswordValida, usuarioGuardado!.PasswordHash));
            repo.Verify(r => r.AsignarRol(10, 2), Times.Once);
        }

        [Fact]
        public void GenerarTokenRecuperacion_UsuarioActivo_DevuelveToken()
        {
            var usuario = CrearUsuario();
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo(usuario.Correo)).Returns(usuario);

            var service = new AuthService(repo.Object, CrearConfiguracion());

            var token = service.GenerarTokenRecuperacion(usuario.Correo);

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GenerarTokenRecuperacion_UsuarioInexistente_DevuelveNull()
        {
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo(It.IsAny<string>())).Returns((Usuario?)null);

            var service = new AuthService(repo.Object, CrearConfiguracion());

            Assert.Null(service.GenerarTokenRecuperacion("nadie@test.com"));
        }

        [Fact]
        public void RestablecerPassword_ConTokenValido_ActualizaPassword()
        {
            var usuario = CrearUsuario();
            string? nuevoHash = null;
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo(usuario.Correo)).Returns(usuario);
            repo.Setup(r => r.ObtenerPorId(usuario.IdUsuario)).Returns(usuario);
            repo.Setup(r => r.ActualizarPassword(usuario.IdUsuario, It.IsAny<string>()))
                .Callback<int, string>((_, hash) => nuevoHash = hash);

            var service = new AuthService(repo.Object, CrearConfiguracion());
            var token = service.GenerarTokenRecuperacion(usuario.Correo);

            var resultado = service.RestablecerPassword(token!, "NuevaPassword1!");

            Assert.True(resultado);
            Assert.NotNull(nuevoHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("NuevaPassword1!", nuevoHash));
        }

        [Fact]
        public void RestablecerPassword_ConTokenInvalido_DevuelveFalse()
        {
            var repo = new Mock<IUsuarioRepository>();
            var service = new AuthService(repo.Object, CrearConfiguracion());

            Assert.False(service.RestablecerPassword("token-invalido", "NuevaPassword1!"));
        }

        [Fact]
        public void RestablecerPassword_ConPasswordDebil_DevuelveFalse()
        {
            var repo = new Mock<IUsuarioRepository>();
            var service = new AuthService(repo.Object, CrearConfiguracion());

            Assert.False(service.RestablecerPassword("cualquier-token", "123456"));
        }
    }
}
