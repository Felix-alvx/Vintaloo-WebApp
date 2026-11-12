using Domain.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    [Collection("SqlServer")]
    public class CategoriaRepositorySqlTests : SqlServerTestBase
    {
        public CategoriaRepositorySqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Guardar_Y_ObtenerTodas_PersisteCategoria()
        {
            var categoria = CrearCategoria(nombre: $"Ropa_{Guid.NewGuid():N}");

            using var context = Fixture.CreateContext();
            var categorias = new CategoriaRepository(context).ObtenerTodas();

            Assert.Contains(categorias, c => c.IdCategoria == categoria.IdCategoria);
            Assert.Equal(categoria.Nombre, categorias.Single(c => c.IdCategoria == categoria.IdCategoria).Nombre);
        }

        [Fact]
        public void Actualizar_PersisteCambios()
        {
            var categoria = CrearCategoria();

            using (var context = Fixture.CreateContext())
            {
                new CategoriaRepository(context).Actualizar(new Categoria
                {
                    IdCategoria = categoria.IdCategoria,
                    Nombre = "Tecnología actualizada",
                    Descripcion = "Nueva descripción"
                });
            }

            using var verificacion = Fixture.CreateContext();
            var actualizada = verificacion.Categorias.AsNoTracking()
                .Single(c => c.IdCategoria == categoria.IdCategoria);

            Assert.Equal("Tecnología actualizada", actualizada.Nombre);
            Assert.Equal("Nueva descripción", actualizada.Descripcion);
        }

        [Fact]
        public void Eliminar_EliminaCategoriaSinArticulos()
        {
            var categoria = CrearCategoria();

            using (var context = Fixture.CreateContext())
            {
                new CategoriaRepository(context).Eliminar(categoria.IdCategoria);
            }

            using var verificacion = Fixture.CreateContext();
            Assert.Null(verificacion.Categorias.AsNoTracking()
                .SingleOrDefault(c => c.IdCategoria == categoria.IdCategoria));
        }
    }

    [Collection("SqlServer")]
    public class ArticuloRepositorySqlTests : SqlServerTestBase
    {
        public ArticuloRepositorySqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void GuardarArticulos_DevuelveIdYPersisteDatos()
        {
            var articulo = CrearArticulo(titulo: "Bicicleta de montaña", precio: 125000m);

            using var context = Fixture.CreateContext();
            var creado = new ArticuloRepository(context).obtenerArticuloPorId(articulo.IdArticulo);

            Assert.NotNull(creado);
            Assert.Equal("Bicicleta de montaña", creado!.Titulo);
            Assert.Equal(125000m, creado.Precio);
            Assert.Equal("disponible", creado.EstadoPublicacion);
            Assert.NotNull(creado.IdCategoriaNavigation);
            Assert.NotNull(creado.IdUsuarioNavigation);
        }

        [Fact]
        public void ObtenerArticulos_SoloDevuelveDisponibles()
        {
            var disponible = CrearArticulo(titulo: "Disponible");
            var vendido = CrearArticulo(titulo: "Vendido");

            using (var context = Fixture.CreateContext())
            {
                new ArticuloRepository(context).ActualizarEstado(vendido.IdArticulo, "vendido");
            }

            using var verificacion = Fixture.CreateContext();
            var articulos = new ArticuloRepository(verificacion).obtenerArticulos();

            Assert.Contains(articulos, a => a.IdArticulo == disponible.IdArticulo);
            Assert.DoesNotContain(articulos, a => a.IdArticulo == vendido.IdArticulo);
            Assert.All(articulos, a => Assert.Equal("disponible", a.EstadoPublicacion));
        }

        [Fact]
        public void ActualizarEstado_PersisteNuevoEstado()
        {
            var articulo = CrearArticulo();

            using (var context = Fixture.CreateContext())
            {
                new ArticuloRepository(context).ActualizarEstado(articulo.IdArticulo, "reservado");
            }

            using var verificacion = Fixture.CreateContext();
            var estado = verificacion.Articulos.AsNoTracking()
                .Where(a => a.IdArticulo == articulo.IdArticulo)
                .Select(a => a.EstadoPublicacion)
                .Single();

            Assert.Equal("reservado", estado);
        }

        [Fact]
        public void ObtenerPorUsuario_DevuelveSoloArticulosDelUsuario()
        {
            var usuario = CrearUsuario();
            var otroUsuario = CrearUsuario();
            var categoria = CrearCategoria();

            var primero = CrearArticulo(usuario, categoria);
            var segundo = CrearArticulo(usuario, categoria);
            CrearArticulo(otroUsuario, categoria);

            using var context = Fixture.CreateContext();
            var articulos = new ArticuloRepository(context).ObtenerPorUsuario(usuario.IdUsuario);

            Assert.Equal(2, articulos.Count);
            Assert.Contains(articulos, a => a.IdArticulo == primero.IdArticulo);
            Assert.Contains(articulos, a => a.IdArticulo == segundo.IdArticulo);
            Assert.All(articulos, a => Assert.Equal(usuario.IdUsuario, a.IdUsuario));
        }

        [Fact]
        public void Eliminar_EliminaArticulo()
        {
            var articulo = CrearArticulo();

            using (var context = Fixture.CreateContext())
            {
                new ArticuloRepository(context).Eliminar(articulo.IdArticulo);
            }

            using var verificacion = Fixture.CreateContext();
            Assert.Null(verificacion.Articulos.AsNoTracking()
                .SingleOrDefault(a => a.IdArticulo == articulo.IdArticulo));
        }
    }

    [Collection("SqlServer")]
    public class ImagenArticuloRepositorySqlTests : SqlServerTestBase
    {
        public ImagenArticuloRepositorySqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Guardar_Y_ObtenerPorArticulo_PersisteImagen()
        {
            var articulo = CrearArticulo();

            using (var context = Fixture.CreateContext())
            {
                new ImagenArticuloRepository(context).Guardar(new ImagenesArticulo
                {
                    IdArticulo = articulo.IdArticulo,
                    UrlImagen = "/imagen/bicicleta.png"
                });
            }

            using var verificacion = Fixture.CreateContext();
            var imagenes = new ImagenArticuloRepository(verificacion)
                .ObtenerPorArticulo(articulo.IdArticulo);

            var imagen = Assert.Single(imagenes);
            Assert.Equal("/imagen/bicicleta.png", imagen.UrlImagen);
        }
    }
}
