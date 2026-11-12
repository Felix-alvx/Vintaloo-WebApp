using Domain.Data;
using Infrastructure.Repositories;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    [Collection("SqlServer")]
    public class UsuarioRepositorySqlTests : SqlServerTestBase
    {
        public UsuarioRepositorySqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void GuardarUsuario_Y_ObtenerPorCorreo_PersisteDatos()
        {
            var correo = CorreoUnico();

            using (var context = Fixture.CreateContext())
            {
                new UsuarioRepository(context).GuardarUsuario(new Usuario
                {
                    Nombre = "María",
                    Correo = correo,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!", workFactor: 4),
                    Telefono = "7777-7777",
                    Ubicacion = "Cartago",
                    FotoPerfil = "/perfiles/maria.png"
                });
            }

            using var verificacion = Fixture.CreateContext();
            var usuario = new UsuarioRepository(verificacion).ObtenerPorCorreo(correo);

            Assert.NotNull(usuario);
            Assert.True(usuario!.IdUsuario > 0);
            Assert.Equal("María", usuario.Nombre);
            Assert.Equal("Cartago", usuario.Ubicacion);
            Assert.True(BCrypt.Net.BCrypt.Verify("Password1!", usuario.PasswordHash));
        }

        [Fact]
        public void AsignarYQuitarRol_PersisteEnUsuarioRoles()
        {
            var usuario = CrearUsuario(asignarRol: false);

            using (var context = Fixture.CreateContext())
            {
                new UsuarioRepository(context).AsignarRol(usuario.IdUsuario, 1);
            }

            using (var context = Fixture.CreateContext())
            {
                Assert.Equal("admin", new UsuarioRepository(context).ObtenerRol(usuario.IdUsuario));
            }

            using (var context = Fixture.CreateContext())
            {
                new UsuarioRepository(context).QuitarRoles(usuario.IdUsuario);
            }

            using (var context = Fixture.CreateContext())
            {
                Assert.Null(new UsuarioRepository(context).ObtenerRol(usuario.IdUsuario));
            }
        }

        [Fact]
        public void ActualizarUsuario_PersisteCamposEditables()
        {
            var usuario = CrearUsuario();

            using (var context = Fixture.CreateContext())
            {
                new UsuarioRepository(context).ActualizarUsuario(new Usuario
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = "Nombre Actualizado",
                    Telefono = "6000-0000",
                    Ubicacion = "Heredia",
                    FotoPerfil = "/perfiles/nueva.png",
                    Estado = "bloqueado"
                });
            }

            using var verificacion = Fixture.CreateContext();
            var actualizado = new UsuarioRepository(verificacion).ObtenerPorId(usuario.IdUsuario);

            Assert.NotNull(actualizado);
            Assert.Equal("Nombre Actualizado", actualizado!.Nombre);
            Assert.Equal("6000-0000", actualizado.Telefono);
            Assert.Equal("Heredia", actualizado.Ubicacion);
            Assert.Equal("/perfiles/nueva.png", actualizado.FotoPerfil);
            Assert.Equal("bloqueado", actualizado.Estado);
        }

        [Fact]
        public void ActualizarPassword_PersisteNuevoHash()
        {
            var usuario = CrearUsuario();
            var nuevoHash = BCrypt.Net.BCrypt.HashPassword("NuevaPassword1!", workFactor: 4);

            using (var context = Fixture.CreateContext())
            {
                new UsuarioRepository(context).ActualizarPassword(usuario.IdUsuario, nuevoHash);
            }

            using var verificacion = Fixture.CreateContext();
            var actualizado = new UsuarioRepository(verificacion).ObtenerPorId(usuario.IdUsuario);

            Assert.NotNull(actualizado);
            Assert.True(BCrypt.Net.BCrypt.Verify("NuevaPassword1!", actualizado!.PasswordHash));
            Assert.False(BCrypt.Net.BCrypt.Verify("Password1!", actualizado.PasswordHash));
        }

        [Fact]
        public void EliminarUsuario_EliminaRegistro()
        {
            var usuario = CrearUsuario(asignarRol: false);

            using (var context = Fixture.CreateContext())
            {
                new UsuarioRepository(context).EliminarUsuario(usuario.IdUsuario);
            }

            using var verificacion = Fixture.CreateContext();
            Assert.Null(new UsuarioRepository(verificacion).ObtenerPorCorreo(usuario.Correo));
        }

        [Fact]
        public void ObtenerTodos_DevuelveLosUsuariosPersistidos()
        {
            var primero = CrearUsuario(nombre: "Primero");
            var segundo = CrearUsuario(nombre: "Segundo");

            using var context = Fixture.CreateContext();
            var usuarios = new UsuarioRepository(context).ObtenerTodos();

            Assert.Contains(usuarios, u => u.IdUsuario == primero.IdUsuario);
            Assert.Contains(usuarios, u => u.IdUsuario == segundo.IdUsuario);
        }
    }
}
