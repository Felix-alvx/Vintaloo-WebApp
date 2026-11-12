using Domain.Data;
using Infrastructure.Repositories;
using Vintaloo_WebApp.src.Infrastructure.Repositories;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    [Collection("SqlServer")]
    public class FavoritoRepositorySqlTests : SqlServerTestBase
    {
        public FavoritoRepositorySqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void AgregarYConsultarFavorito_PersisteRelacion()
        {
            var usuario = CrearUsuario();
            var articulo = CrearArticulo(usuario);

            using (var context = Fixture.CreateContext())
            {
                new FavoritoRepository(context).AgregarFavorito(usuario.IdUsuario, articulo.IdArticulo);
            }

            using (var context = Fixture.CreateContext())
            {
                var repository = new FavoritoRepository(context);
                Assert.True(repository.ExisteFavorito(usuario.IdUsuario, articulo.IdArticulo));

                var favoritos = repository.ObtenerFavoritosUsuario(usuario.IdUsuario);
                Assert.Contains(favoritos, a => a.IdArticulo == articulo.IdArticulo);
            }
        }

        [Fact]
        public void EliminarFavorito_RemueveRelacion()
        {
            var usuario = CrearUsuario();
            var articulo = CrearArticulo(usuario);

            using (var context = Fixture.CreateContext())
            {
                var repository = new FavoritoRepository(context);
                repository.AgregarFavorito(usuario.IdUsuario, articulo.IdArticulo);
                repository.EliminarFavorito(usuario.IdUsuario, articulo.IdArticulo);
            }

            using var verificacion = Fixture.CreateContext();
            var repositorioVerificacion = new FavoritoRepository(verificacion);

            Assert.False(repositorioVerificacion.ExisteFavorito(usuario.IdUsuario, articulo.IdArticulo));
            Assert.Empty(repositorioVerificacion.ObtenerFavoritosUsuario(usuario.IdUsuario));
        }
    }

    [Collection("SqlServer")]
    public class ReporteRepositorySqlTests : SqlServerTestBase
    {
        public ReporteRepositorySqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void CrearReporte_Y_ObtenerReportes_PersisteReporte()
        {
            var usuario = CrearUsuario();
            var articulo = CrearArticulo(usuario);

            using (var context = Fixture.CreateContext())
            {
                new ReporteRepository(context).CrearReporte(new Reporte
                {
                    IdUsuario = usuario.IdUsuario,
                    IdArticulo = articulo.IdArticulo,
                    Motivo = "Contenido inapropiado",
                    Descripcion = "El artículo contiene información engañosa."
                });
            }

            using var verificacion = Fixture.CreateContext();
            var reportes = new ReporteRepository(verificacion).ObtenerReportes();

            var reporte = Assert.Single(reportes);
            Assert.Equal(usuario.IdUsuario, reporte.IdUsuario);
            Assert.Equal(articulo.IdArticulo, reporte.IdArticulo);
            Assert.Equal("Contenido inapropiado", reporte.Motivo);
            Assert.Equal("pendiente", reporte.Estado);
        }

        [Fact]
        public void ObtenerReportes_SinReportes_DevuelveListaVacia()
        {
            using var context = Fixture.CreateContext();
            var reportes = new ReporteRepository(context).ObtenerReportes();

            Assert.Empty(reportes);
        }
    }
}
