using System.Net;
using System.Net.Http.Headers;
using Domain.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    [Collection("SqlServer")]
    public class ArchivoWebSqlTests : SqlServerTestBase
    {
        private const string PasswordValida = "Password1!";
        private readonly string _webRoot;
        private Usuario _usuario = null!;
        private Categoria _categoria = null!;

        public ArchivoWebSqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
            _webRoot = Path.Combine(
                Path.GetTempPath(),
                "vintaloo-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_webRoot);
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _usuario = CrearUsuario(
                correo: CorreoUnico(),
                password: PasswordValida,
                nombre: "Usuario Archivos");
            _categoria = CrearCategoria();
        }

        public override Task DisposeAsync()
        {
            try
            {
                if (Directory.Exists(_webRoot))
                    Directory.Delete(_webRoot, recursive: true);
            }
            catch
            {
                // La carpeta temporal se limpia en la siguiente ejecución si queda bloqueada.
            }

            return Task.CompletedTask;
        }

        [Fact]
        public async Task CrearArticulo_ConImagenesValidas_GuardaArchivosYRegistros()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString, _webRoot);
            using var client = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, _usuario.Correo, PasswordValida);
            var token = await WebTestHelper.ObtenerTokenAntiforgeryAsync(client, "/Articulo/Crear");
            var titulo = $"Artículo {Guid.NewGuid():N}";

            var jpg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
            var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            using var formulario = CrearFormularioArticulo(token, titulo);
            AgregarArchivo(formulario, "imagenes", "foto.jpg", "image/jpeg", jpg);
            AgregarArchivo(formulario, "imagenes", "foto.png", "image/png", png);

            var respuesta = await client.PostAsync("/Articulo/Crear", formulario);

            Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
            var destino = respuesta.Headers.Location?.ToString();
            Assert.True(destino == "/" || destino == "/Dashboard",
                $"Redirección inesperada después de crear el artículo: '{destino}'.");

            var carpeta = Path.Combine(_webRoot, "imagen");
            var archivos = Directory.GetFiles(carpeta);
            Assert.Equal(2, archivos.Length);
            Assert.All(archivos, archivo =>
                Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(archivo), out _)));
            Assert.Contains(archivos, archivo => Path.GetExtension(archivo) == ".jpg");
            Assert.Contains(archivos, archivo => Path.GetExtension(archivo) == ".png");

            using var context = Fixture.CreateContext();
            var articulo = context.Articulos
                .Include(a => a.ImagenesArticulos)
                .Single(a => a.Titulo == titulo);
            var urlsEsperadas = archivos
                .Select(archivo => $"/imagen/{Path.GetFileName(archivo)}")
                .OrderBy(url => url);
            var urlsGuardadas = articulo.ImagenesArticulos
                .Select(imagen => imagen.UrlImagen)
                .OrderBy(url => url);

            Assert.Equal(_usuario.IdUsuario, articulo.IdUsuario);
            Assert.Equal(urlsEsperadas, urlsGuardadas);
            Assert.Equal(jpg, File.ReadAllBytes(archivos.Single(a => Path.GetExtension(a) == ".jpg")));
        }

        [Fact]
        public async Task CrearArticulo_ConExtensionNoPermitida_NoGuardaArchivoNiArticulo()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString, _webRoot);
            using var client = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, _usuario.Correo, PasswordValida);
            var token = await WebTestHelper.ObtenerTokenAntiforgeryAsync(client, "/Articulo/Crear");
            var titulo = $"Artículo {Guid.NewGuid():N}";

            using var formulario = CrearFormularioArticulo(token, titulo);
            AgregarArchivo(formulario, "imagenes", "archivo.gif", "image/gif", new byte[] { 1, 2, 3 });

            var respuesta = await client.PostAsync("/Articulo/Crear", formulario);
            var html = await respuesta.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Contains("debe ser JPG, PNG o WEBP", html);
            Assert.False(ExisteArticulo(titulo));
            Assert.False(HayArchivosEn("imagen"));
        }

        [Fact]
        public async Task CrearArticulo_ConImagenMayorA5MB_NoGuardaArchivoNiArticulo()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString, _webRoot);
            using var client = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, _usuario.Correo, PasswordValida);
            var token = await WebTestHelper.ObtenerTokenAntiforgeryAsync(client, "/Articulo/Crear");
            var titulo = $"Artículo {Guid.NewGuid():N}";

            using var formulario = CrearFormularioArticulo(token, titulo);
            var archivoGrande = new byte[5 * 1024 * 1024 + 1];
            AgregarArchivo(formulario, "imagenes", "grande.jpg", "image/jpeg", archivoGrande);

            var respuesta = await client.PostAsync("/Articulo/Crear", formulario);
            var html = await respuesta.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Contains("no superar 5 MB", html);
            Assert.False(ExisteArticulo(titulo));
            Assert.False(HayArchivosEn("imagen"));
        }

        [Fact]
        public async Task EditarPerfil_ConFotoValida_GuardaArchivoYActualizaUsuario()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString, _webRoot);
            using var client = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, _usuario.Correo, PasswordValida);
            var token = await WebTestHelper.ObtenerTokenAntiforgeryAsync(client, "/Usuario/EditarPerfil");
            var foto = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            using var formulario = CrearFormularioPerfil(token);
            AgregarArchivo(formulario, "fotoPerfil", "perfil.png", "image/png", foto);

            var respuesta = await client.PostAsync("/Usuario/EditarPerfil", formulario);

            Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
            Assert.Equal("/Usuario/Perfil", respuesta.Headers.Location?.ToString());

            var carpeta = Path.Combine(_webRoot, "perfiles");
            var archivo = Assert.Single(Directory.GetFiles(carpeta));
            Assert.Equal(".png", Path.GetExtension(archivo));
            Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(archivo), out _));
            Assert.Equal(foto, File.ReadAllBytes(archivo));

            using var context = Fixture.CreateContext();
            var actualizado = context.Usuarios
                .AsNoTracking()
                .Single(usuario => usuario.IdUsuario == _usuario.IdUsuario);

            Assert.Equal($"/perfiles/{Path.GetFileName(archivo)}", actualizado.FotoPerfil);
            Assert.Equal("Perfil Actualizado", actualizado.Nombre);
        }

        [Fact]
        public async Task EditarPerfil_ConExtensionNoPermitida_NoGuardaArchivoNiActualizaFoto()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString, _webRoot);
            using var client = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, _usuario.Correo, PasswordValida);
            var token = await WebTestHelper.ObtenerTokenAntiforgeryAsync(client, "/Usuario/EditarPerfil");

            using var formulario = CrearFormularioPerfil(token);
            AgregarArchivo(formulario, "fotoPerfil", "perfil.gif", "image/gif", new byte[] { 1, 2, 3 });

            var respuesta = await client.PostAsync("/Usuario/EditarPerfil", formulario);
            var html = await respuesta.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Contains("La foto debe ser JPG, PNG o WEBP", html);
            Assert.False(HayArchivosEn("perfiles"));
            Assert.Equal("/perfiles/test.png", ObtenerFotoPerfilActual());
        }

        [Fact]
        public async Task EditarPerfil_ConFotoMayorA2MB_NoGuardaArchivoNiActualizaFoto()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString, _webRoot);
            using var client = await WebTestHelper.CrearClienteAutenticadoAsync(
                factory, _usuario.Correo, PasswordValida);
            var token = await WebTestHelper.ObtenerTokenAntiforgeryAsync(client, "/Usuario/EditarPerfil");

            using var formulario = CrearFormularioPerfil(token);
            var fotoGrande = new byte[2 * 1024 * 1024 + 1];
            AgregarArchivo(formulario, "fotoPerfil", "grande.png", "image/png", fotoGrande);

            var respuesta = await client.PostAsync("/Usuario/EditarPerfil", formulario);
            var html = await respuesta.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Contains("no superar 2 MB", html);
            Assert.False(HayArchivosEn("perfiles"));
            Assert.Equal("/perfiles/test.png", ObtenerFotoPerfilActual());
        }

        private MultipartFormDataContent CrearFormularioArticulo(string token, string titulo)
        {
            var formulario = new MultipartFormDataContent
            {
                { new StringContent(_categoria.IdCategoria.ToString()), "IdCategoria" },
                { new StringContent(titulo), "Titulo" },
                { new StringContent("Artículo de prueba para subida de archivos."), "Descripcion" },
                { new StringContent("1500"), "Precio" },
                { new StringContent("bueno"), "EstadoProducto" },
                { new StringContent("San José"), "Ubicacion" },
                { new StringContent(token), "__RequestVerificationToken" }
            };

            return formulario;
        }

        private static MultipartFormDataContent CrearFormularioPerfil(string token)
        {
            return new MultipartFormDataContent
            {
                { new StringContent("Perfil Actualizado"), "Nombre" },
                { new StringContent("8888-0000"), "Telefono" },
                { new StringContent("Cartago"), "Ubicacion" },
                { new StringContent(token), "__RequestVerificationToken" }
            };
        }

        private static void AgregarArchivo(
            MultipartFormDataContent formulario,
            string campo,
            string nombreArchivo,
            string tipoContenido,
            byte[] contenido)
        {
            var archivo = new ByteArrayContent(contenido);
            archivo.Headers.ContentType = new MediaTypeHeaderValue(tipoContenido);
            formulario.Add(archivo, campo, nombreArchivo);
        }

        private bool ExisteArticulo(string titulo)
        {
            using var context = Fixture.CreateContext();
            return context.Articulos.AsNoTracking().Any(articulo => articulo.Titulo == titulo);
        }

        private bool HayArchivosEn(string carpeta)
        {
            var ruta = Path.Combine(_webRoot, carpeta);
            return Directory.Exists(ruta) && Directory.EnumerateFiles(ruta).Any();
        }

        private string? ObtenerFotoPerfilActual()
        {
            using var context = Fixture.CreateContext();
            return context.Usuarios
                .AsNoTracking()
                .Single(usuario => usuario.IdUsuario == _usuario.IdUsuario)
                .FotoPerfil;
        }
    }
}
