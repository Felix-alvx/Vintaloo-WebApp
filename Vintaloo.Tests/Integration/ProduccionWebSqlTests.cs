using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    [Collection("SqlServer")]
    public class ProduccionWebSqlTests : SqlServerTestBase
    {
        public ProduccionWebSqlTests(SqlServerTestFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Health_ConBdReal_RespondeHealthy()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString);
            using var client = factory.CreateClient();

            var respuesta = await client.GetAsync("/health");
            var contenido = await respuesta.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Contains("Healthy", contenido);
        }

        [Fact]
        public async Task Respuesta_IncluyeCabecerasDeSeguridad()
        {
            using var factory = new TestWebApplicationFactory(Fixture.TestConnectionString);
            using var client = factory.CreateClient();

            var respuesta = await client.GetAsync("/Auth/Login");

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.Equal("nosniff", respuesta.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("DENY", respuesta.Headers.GetValues("X-Frame-Options").Single());
            Assert.Equal(
                "strict-origin-when-cross-origin",
                respuesta.Headers.GetValues("Referrer-Policy").Single());
        }
    }
}
