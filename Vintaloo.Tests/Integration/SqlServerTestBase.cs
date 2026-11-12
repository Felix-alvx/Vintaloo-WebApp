using Domain.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    public abstract class SqlServerTestBase : IAsyncLifetime
    {
        protected SqlServerTestFixture Fixture { get; }

        protected SqlServerTestBase(SqlServerTestFixture fixture)
        {
            Fixture = fixture;
        }

        public virtual Task InitializeAsync()
        {
            return Fixture.ResetAsync();
        }

        public virtual Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected static string CorreoUnico()
        {
            return $"test_{Guid.NewGuid():N}@vintaloo.test";
        }

        protected Usuario CrearUsuario(
            string? correo = null,
            string password = "Password1!",
            string nombre = "Usuario Prueba",
            string estado = "activo",
            int idRol = 2,
            bool asignarRol = true)
        {
            correo ??= CorreoUnico();

            using var context = Fixture.CreateContext();
            var repository = new UsuarioRepository(context);
            var usuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4),
                Telefono = "8888-8888",
                Ubicacion = "San José",
                FotoPerfil = "/perfiles/test.png",
                Estado = estado
            };

            repository.GuardarUsuario(usuario);
            var creado = repository.ObtenerPorCorreo(correo)
                ?? throw new InvalidOperationException("No se pudo crear el usuario de prueba.");

            if (!string.Equals(estado, "activo", StringComparison.OrdinalIgnoreCase))
            {
                context.Database.ExecuteSqlRaw(
                    "UPDATE usuarios SET estado = {0} WHERE id_usuario = {1}",
                    estado, creado.IdUsuario);
                creado.Estado = estado;
            }

            if (asignarRol)
                repository.AsignarRol(creado.IdUsuario, idRol);

            return creado;
        }

        protected Categoria CrearCategoria(string? nombre = null, string? descripcion = "Categoría de prueba")
        {
            nombre ??= $"Categoria_{Guid.NewGuid():N}";

            using var context = Fixture.CreateContext();
            var repository = new CategoriaRepository(context);
            repository.Guardar(new Categoria
            {
                Nombre = nombre,
                Descripcion = descripcion
            });

            return context.Categorias
                .AsNoTracking()
                .First(c => c.Nombre == nombre);
        }

        protected Articulo CrearArticulo(
            Usuario? usuario = null,
            Categoria? categoria = null,
            string? titulo = null,
            string descripcion = "Descripción de prueba",
            decimal precio = 1000m,
            string estadoProducto = "bueno",
            string ubicacion = "San José")
        {
            usuario ??= CrearUsuario();
            categoria ??= CrearCategoria();
            titulo ??= $"Artículo {Guid.NewGuid():N}";

            using var context = Fixture.CreateContext();
            var repository = new ArticuloRepository(context);
            var articulo = new Articulo
            {
                IdUsuario = usuario.IdUsuario,
                IdCategoria = categoria.IdCategoria,
                Titulo = titulo,
                Descripcion = descripcion,
                Precio = precio,
                EstadoProducto = estadoProducto,
                EstadoPublicacion = "disponible",
                Ubicacion = ubicacion
            };

            var id = repository.GuardarArticulos(articulo);
            if (id <= 0)
                throw new InvalidOperationException("No se pudo crear el artículo de prueba.");

            articulo.IdArticulo = id;
            return articulo;
        }

        protected static IConfiguration CrearConfiguracionJwt()
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
    }
}
