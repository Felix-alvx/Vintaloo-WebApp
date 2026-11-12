using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Vintaloo.WebApp.Controllers;
using Xunit;

namespace Vintaloo.Tests
{
    public class AuthorizationTests
    {
        [Fact]
        public void CategoriaController_RequiereRolAdmin()
        {
            var atributo = typeof(CategoriaController).GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(atributo);
            Assert.Equal("admin", atributo!.Roles);
        }

        [Fact]
        public void UsuarioController_IndexRequiereRolAdmin()
        {
            var metodo = typeof(UsuarioController).GetMethod(nameof(UsuarioController.Index));
            var atributo = metodo!.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(atributo);
            Assert.Equal("admin", atributo!.Roles);
        }

        [Fact]
        public void UsuarioController_PerfilSoloRequiereAutenticacion()
        {
            var metodo = typeof(UsuarioController).GetMethod(nameof(UsuarioController.Perfil));
            var atributo = metodo!.GetCustomAttribute<AuthorizeAttribute>();

            Assert.Null(atributo);
            Assert.NotNull(typeof(UsuarioController).GetCustomAttribute<AuthorizeAttribute>());
        }

        [Fact]
        public void ReporteController_CrearNoRequiereAdmin()
        {
            var metodo = typeof(ReporteController).GetMethod(nameof(ReporteController.Crear), new[] { typeof(Domain.Data.Reporte) });
            var atributo = metodo!.GetCustomAttribute<AuthorizeAttribute>();

            Assert.Null(atributo);
            Assert.NotNull(typeof(ReporteController).GetCustomAttribute<AuthorizeAttribute>());
        }

        [Fact]
        public void ReporteController_IndexRequiereRolAdmin()
        {
            var metodo = typeof(ReporteController).GetMethod(nameof(ReporteController.Index));
            var atributo = metodo!.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(atributo);
            Assert.Equal("admin", atributo!.Roles);
        }
    }
}
