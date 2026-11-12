using Application.DTOs;
using Application.Interfaces;
using Application.Servicies;
using Domain.Data;
using Moq;
using Xunit;

namespace Vintaloo.Tests
{
    public class UsuarioServiceTests
    {
        private const string PasswordValida = "Password1!";

        [Fact]
        public void ObtenerUsuarios_MapeaRolEstadoYFecha()
        {
            var fecha = new DateTime(2026, 1, 1);
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerTodos()).Returns(new List<Usuario>
            {
                new()
                {
                    IdUsuario = 1,
                    Nombre = "Admin",
                    Correo = "admin@test.com",
                    Estado = "activo",
                    FechaRegistro = fecha
                }
            });
            repo.Setup(r => r.ObtenerRol(1)).Returns("admin");

            var service = new UsuarioService(repo.Object);
            var usuarios = service.ObtenerUsuarios();

            var usuario = Assert.Single(usuarios);
            Assert.Equal("admin", usuario.Rol);
            Assert.Equal("activo", usuario.Estado);
            Assert.Equal(fecha, usuario.FechaRegistro);
        }

        [Fact]
        public void CrearUsuario_ConCorreoDuplicado_DevuelveFalse()
        {
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorCorreo("nuevo@test.com"))
                .Returns(new Usuario { IdUsuario = 5, Correo = "nuevo@test.com" });

            var service = new UsuarioService(repo.Object);
            var resultado = service.CrearUsuario(new CreateUsuarioDTO
            {
                Nombre = "Nuevo",
                Correo = "nuevo@test.com",
                Password = PasswordValida,
                ConfirmPassword = PasswordValida,
                IdRol = 2
            });

            Assert.False(resultado);
            repo.Verify(r => r.GuardarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void CrearUsuario_Valido_HasheaPasswordYAsignaRol()
        {
            var creado = new Usuario { IdUsuario = 20, Nombre = "Nuevo", Correo = "nuevo@test.com" };
            Usuario? guardado = null;
            var repo = new Mock<IUsuarioRepository>();
            repo.SetupSequence(r => r.ObtenerPorCorreo("nuevo@test.com"))
                .Returns((Usuario?)null)
                .Returns(creado);
            repo.Setup(r => r.GuardarUsuario(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => guardado = u);

            var service = new UsuarioService(repo.Object);
            var resultado = service.CrearUsuario(new CreateUsuarioDTO
            {
                Nombre = "Nuevo",
                Correo = "nuevo@test.com",
                Password = PasswordValida,
                ConfirmPassword = PasswordValida,
                IdRol = 1
            });

            Assert.True(resultado);
            Assert.NotNull(guardado);
            Assert.True(BCrypt.Net.BCrypt.Verify(PasswordValida, guardado!.PasswordHash));
            repo.Verify(r => r.AsignarRol(20, 1), Times.Once);
        }

        [Fact]
        public void CrearUsuario_ConPasswordDebil_LanzaArgumentException()
        {
            var repo = new Mock<IUsuarioRepository>();
            var service = new UsuarioService(repo.Object);

            Assert.Throws<ArgumentException>(() => service.CrearUsuario(new CreateUsuarioDTO
            {
                Nombre = "Nuevo",
                Correo = "nuevo@test.com",
                Password = "123456",
                ConfirmPassword = "123456",
                IdRol = 2
            }));
        }

        [Fact]
        public void ActualizarUsuarioAdmin_ActualizaDatosYReemplazaRol()
        {
            Usuario? actualizado = null;
            var repo = new Mock<IUsuarioRepository>();
            repo.Setup(r => r.ObtenerPorId(1)).Returns(new Usuario
            {
                IdUsuario = 1,
                Nombre = "Anterior",
                Correo = "admin@test.com",
                FotoPerfil = "/perfiles/foto.png",
                Estado = "activo"
            });
            repo.Setup(r => r.ActualizarUsuario(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => actualizado = u);

            var service = new UsuarioService(repo.Object);
            service.ActualizarUsuarioAdmin(new UpdateUsuarioDTO
            {
                IdUsuario = 1,
                Nombre = "Admin",
                Telefono = "8888-8888",
                Ubicacion = "San José",
                Estado = "bloqueado",
                IdRol = 1
            });

            Assert.NotNull(actualizado);
            Assert.Equal("Admin", actualizado!.Nombre);
            Assert.Equal("bloqueado", actualizado.Estado);
            Assert.Equal("/perfiles/foto.png", actualizado.FotoPerfil);
            repo.Verify(r => r.QuitarRoles(1), Times.Once);
            repo.Verify(r => r.AsignarRol(1, 1), Times.Once);
        }

        [Fact]
        public void EliminarUsuario_ConIdInvalido_LanzaArgumentException()
        {
            var repo = new Mock<IUsuarioRepository>();
            var service = new UsuarioService(repo.Object);

            Assert.Throws<ArgumentException>(() => service.EliminarUsuario(0));
        }
    }
}
