using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.DTOs;
using Application.Servicies;
using Infrastructure.Repositories;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    [Collection("SqlServer")]
    public class AuthServiceSqlTests : SqlServerTestBase
    {
        private const string PasswordValida = "Password1!";

        public AuthServiceSqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Login_ConUsuarioReal_DevuelveTokenConRolYDatos()
        {
            var usuario = CrearUsuario(password: PasswordValida, idRol: 1);

            using var context = Fixture.CreateContext();
            var service = new AuthService(new UsuarioRepository(context), CrearConfiguracionJwt());

            var token = service.Login(new LoginDTO
            {
                Correo = usuario.Correo,
                Password = PasswordValida
            });

            Assert.NotNull(token);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal(usuario.IdUsuario.ToString(),
                jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(usuario.Correo,
                jwt.Claims.Single(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("admin",
                jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void Login_ConUsuarioInactivoEnBd_DevuelveNull()
        {
            var usuario = CrearUsuario(password: PasswordValida, estado: "bloqueado");

            using var context = Fixture.CreateContext();
            var service = new AuthService(new UsuarioRepository(context), CrearConfiguracionJwt());

            var token = service.Login(new LoginDTO
            {
                Correo = usuario.Correo,
                Password = PasswordValida
            });

            Assert.Null(token);
        }

        [Fact]
        public void Registrar_UsuarioNuevo_PersisteHashYAsignaRolUsuario()
        {
            var correo = CorreoUnico();
            using var context = Fixture.CreateContext();
            var service = new AuthService(
                new UsuarioRepository(context),
                CrearConfiguracionJwt());

            var resultado = service.Registrar(new RegisterDTO
            {
                Nombre = "Nuevo Usuario",
                Correo = correo,
                Password = PasswordValida,
                ConfirmPassword = PasswordValida,
                Telefono = "8888-0000",
                Ubicacion = "Alajuela"
            });

            Assert.True(resultado);

            using var verificacion = Fixture.CreateContext();
            var repository = new UsuarioRepository(verificacion);
            var usuario = repository.ObtenerPorCorreo(correo);

            Assert.NotNull(usuario);
            Assert.True(BCrypt.Net.BCrypt.Verify(PasswordValida, usuario!.PasswordHash));
            Assert.Equal("usuario", repository.ObtenerRol(usuario.IdUsuario));
        }

        [Fact]
        public void RestablecerPassword_ConTokenReal_ActualizaHashEInvalidaTokenAnterior()
        {
            var usuario = CrearUsuario(password: PasswordValida);

            using var context = Fixture.CreateContext();
            var repository = new UsuarioRepository(context);
            var service = new AuthService(repository, CrearConfiguracionJwt());

            var token = service.GenerarTokenRecuperacion(usuario.Correo);
            Assert.NotNull(token);

            var resultado = service.RestablecerPassword(token!, "NuevaPassword1!");
            Assert.True(resultado);

            using var verificacion = Fixture.CreateContext();
            var actualizado = new UsuarioRepository(verificacion).ObtenerPorId(usuario.IdUsuario);

            Assert.NotNull(actualizado);
            Assert.True(BCrypt.Net.BCrypt.Verify("NuevaPassword1!", actualizado!.PasswordHash));
            Assert.False(BCrypt.Net.BCrypt.Verify(PasswordValida, actualizado.PasswordHash));

            Assert.False(service.RestablecerPassword(token!, "OtraPassword1!"));
        }
    }
}
