using System.Net;
using Domain.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    [Collection("SqlServer")]
    public class AdminWebSqlTests : SqlServerTestBase
    {
        private const string PasswordValida = "Password1!";

        public AdminWebSqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Admin_UsuarioCreaEditaYElimina()
        {
            var admin = CrearAdmin();
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString);
            using var client = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, admin.Correo, PasswordValida);
            var correo = CorreoUnico();

            var tokenCrear = await WebTestHelper.ObtenerTokenAntiforgeryAsync(client, "/Usuario/Crear");
            var crear = await client.PostAsync("/Usuario/Crear", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Nombre"] = "Usuario E2E",
                    ["Correo"] = correo,
                    ["Telefono"] = "8000-1111",
                    ["Ubicacion"] = "Alajuela",
                    ["Password"] = PasswordValida,
                    ["ConfirmPassword"] = PasswordValida,
                    ["IdRol"] = "2",
                    ["__RequestVerificationToken"] = tokenCrear
                }));

            AssertRedireccionA(crear, "/Usuario");
            var creado = BuscarUsuario(correo);
            Assert.NotNull(creado);
            Assert.Equal("Usuario E2E", creado!.Nombre);
            Assert.Equal("activo", creado.Estado);
            Assert.Equal("usuario", ObtenerRol(creado.IdUsuario));

            var tokenEditar = await WebTestHelper.ObtenerTokenAntiforgeryAsync(
                client, $"/Usuario/Editar/{creado.IdUsuario}");
            var editar = await client.PostAsync("/Usuario/Editar", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["IdUsuario"] = creado.IdUsuario.ToString(),
                    ["Nombre"] = "Usuario Editado",
                    ["Telefono"] = "8000-2222",
                    ["Ubicacion"] = "Heredia",
                    ["Estado"] = "bloqueado",
                    ["IdRol"] = "1",
                    ["__RequestVerificationToken"] = tokenEditar
                }));

            AssertRedireccionA(editar, "/Usuario");
            var actualizado = BuscarUsuario(correo);
            Assert.NotNull(actualizado);
            Assert.Equal("Usuario Editado", actualizado!.Nombre);
            Assert.Equal("8000-2222", actualizado.Telefono);
            Assert.Equal("Heredia", actualizado.Ubicacion);
            Assert.Equal("bloqueado", actualizado.Estado);
            Assert.Equal("admin", ObtenerRol(actualizado.IdUsuario));

            var tokenEliminar = await WebTestHelper.ObtenerTokenAntiforgeryAsync(
                client, $"/Usuario/Eliminar/{actualizado.IdUsuario}");
            var eliminar = await client.PostAsync(
                $"/Usuario/EliminarConfirmado/{actualizado.IdUsuario}",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = tokenEliminar
                }));

            AssertRedireccionA(eliminar, "/Usuario");
            Assert.Null(BuscarUsuario(correo));
        }

        [Fact]
        public async Task Admin_CategoriaCreaEditaYElimina()
        {
            var admin = CrearAdmin();
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString);
            using var client = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, admin.Correo, PasswordValida);
            var nombre = $"CategoriaE2E_{Guid.NewGuid():N}";
            var nombreEditado = $"{nombre}_editada";

            var tokenCrear = await WebTestHelper.ObtenerTokenAntiforgeryAsync(client, "/Categoria/Crear");
            var crear = await client.PostAsync("/Categoria/Crear", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Nombre"] = nombre,
                    ["Descripcion"] = "Categoría creada desde prueba E2E.",
                    ["__RequestVerificationToken"] = tokenCrear
                }));

            AssertRedireccionA(crear, "/Categoria");
            var creada = BuscarCategoria(nombre);
            Assert.NotNull(creada);
            Assert.Equal("Categoría creada desde prueba E2E.", creada!.Descripcion);

            var listado = await client.GetAsync("/Categoria");
            listado.EnsureSuccessStatusCode();
            Assert.Contains(nombre, await listado.Content.ReadAsStringAsync());

            var tokenEditar = await WebTestHelper.ObtenerTokenAntiforgeryAsync(
                client, $"/Categoria/Editar/{creada.IdCategoria}");
            var editar = await client.PostAsync("/Categoria/Editar", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["IdCategoria"] = creada.IdCategoria.ToString(),
                    ["Nombre"] = nombreEditado,
                    ["Descripcion"] = "Categoría editada desde prueba E2E.",
                    ["__RequestVerificationToken"] = tokenEditar
                }));

            AssertRedireccionA(editar, "/Categoria");
            var editada = BuscarCategoria(nombreEditado);
            Assert.NotNull(editada);
            Assert.Equal("Categoría editada desde prueba E2E.", editada!.Descripcion);
            Assert.Null(BuscarCategoria(nombre));

            var tokenEliminar = await WebTestHelper.ObtenerTokenAntiforgeryAsync(
                client, $"/Categoria/Eliminar/{editada.IdCategoria}");
            var eliminar = await client.PostAsync(
                $"/Categoria/EliminarConfirmado/{editada.IdCategoria}",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = tokenEliminar
                }));

            AssertRedireccionA(eliminar, "/Categoria");
            Assert.Null(BuscarCategoria(nombreEditado));
        }

        [Fact]
        public async Task Usuario_CreaReporte_YAdminLoVisualiza()
        {
            var admin = CrearAdmin();
            var usuario = CrearUsuario(correo: CorreoUnico(), password: PasswordValida, nombre: "Reportante");
            var categoria = CrearCategoria();
            var articulo = CrearArticulo(usuario, categoria);
            var motivo = $"Motivo E2E {Guid.NewGuid():N}";

            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString);
            using var clienteUsuario = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, usuario.Correo, PasswordValida);

            var token = await WebTestHelper.ObtenerTokenAntiforgeryAsync(clienteUsuario, "/Reporte/Crear");
            var crear = await clienteUsuario.PostAsync("/Reporte/Crear", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["IdArticulo"] = articulo.IdArticulo.ToString(),
                    ["Motivo"] = motivo,
                    ["Descripcion"] = "Descripción del reporte E2E.",
                    ["__RequestVerificationToken"] = token
                }));

            AssertRedireccionA(crear, "/Reporte");
            var reporte = BuscarReporte(motivo);
            Assert.NotNull(reporte);
            Assert.Equal(usuario.IdUsuario, reporte!.IdUsuario);
            Assert.Equal(articulo.IdArticulo, reporte.IdArticulo);
            Assert.Equal("Descripción del reporte E2E.", reporte.Descripcion);
            Assert.Equal("pendiente", reporte.Estado);

            Assert.Equal(HttpStatusCode.Forbidden, (await clienteUsuario.GetAsync("/Reporte")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await clienteUsuario.GetAsync("/Categoria")).StatusCode);

            using var clienteAdmin = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, admin.Correo, PasswordValida);
            var listado = await clienteAdmin.GetAsync("/Reporte");
            listado.EnsureSuccessStatusCode();
            var html = await listado.Content.ReadAsStringAsync();

            Assert.Contains(motivo, html);
            Assert.Contains($"#{articulo.IdArticulo}", html);
            Assert.Contains($"#{usuario.IdUsuario}", html);
        }

        private Usuario CrearAdmin()
        {
            return CrearUsuario(
                correo: CorreoUnico(),
                password: PasswordValida,
                nombre: "Admin E2E",
                idRol: 1);
        }

        private Usuario? BuscarUsuario(string correo)
        {
            using var context = Fixture.CreateContext();
            return context.Usuarios
                .AsNoTracking()
                .FirstOrDefault(usuario => usuario.Correo == correo);
        }

        private string? ObtenerRol(int idUsuario)
        {
            using var context = Fixture.CreateContext();
            return new UsuarioRepository(context).ObtenerRol(idUsuario);
        }

        private Categoria? BuscarCategoria(string nombre)
        {
            using var context = Fixture.CreateContext();
            return context.Categorias
                .AsNoTracking()
                .FirstOrDefault(categoria => categoria.Nombre == nombre);
        }

        private Reporte? BuscarReporte(string motivo)
        {
            using var context = Fixture.CreateContext();
            return context.Reportes
                .AsNoTracking()
                .FirstOrDefault(reporte => reporte.Motivo == motivo);
        }

        private static void AssertRedireccionA(HttpResponseMessage respuesta, string prefijo)
        {
            Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
            var destino = respuesta.Headers.Location?.ToString();
            Assert.False(string.IsNullOrWhiteSpace(destino));
            Assert.StartsWith(prefijo, destino);
        }
    }
}
