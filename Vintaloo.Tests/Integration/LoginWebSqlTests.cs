using System.Net;
using Domain.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    [Collection("SqlServer")]
    public class LoginWebSqlTests : SqlServerTestBase
    {
        private const string PasswordValida = "Password1!";
        private Usuario _admin = null!;

        public LoginWebSqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _admin = CrearUsuario(
                correo: CorreoUnico(),
                password: PasswordValida,
                nombre: "Admin Web",
                idRol: 1);
        }

        [Fact]
        public async Task LoginAdmin_ConBdReal_PermiteAccederALaAdministracion()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString);
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });

            var respuesta = await WebTestHelper.IniciarSesionAsync(client, _admin.Correo, PasswordValida);

            Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
            var destino = respuesta.Headers.Location?.ToString();
            Assert.True(destino == "/" || destino == "/Dashboard",
                $"Redirección inesperada después del login: '{destino}'.");

            var administracion = await client.GetAsync("/Usuario");
            var html = await administracion.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, administracion.StatusCode);
            Assert.Contains(_admin.Correo, html);
        }

        [Fact]
        public async Task LoginUsuarioNormal_NoPuedeAccederALaAdministracion()
        {
            var usuario = CrearUsuario(
                correo: CorreoUnico(),
                password: PasswordValida,
                nombre: "Usuario Web",
                idRol: 2);

            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString);
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });

            var respuesta = await WebTestHelper.IniciarSesionAsync(client, usuario.Correo, PasswordValida);
            Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

            var administracion = await client.GetAsync("/Usuario");

            Assert.Equal(HttpStatusCode.Forbidden, administracion.StatusCode);
        }

    }
}
