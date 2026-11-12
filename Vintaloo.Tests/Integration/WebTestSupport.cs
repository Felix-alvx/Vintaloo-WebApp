using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Vintaloo.Tests.Integration
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;
        private readonly string? _webRoot;

        public TestWebApplicationFactory(string connectionString, string? webRoot = null)
        {
            _connectionString = connectionString;
            _webRoot = webRoot;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            if (!string.IsNullOrWhiteSpace(_webRoot))
                builder.UseWebRoot(_webRoot);

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString,
                    ["Jwt:Key"] = "VintalooTestsSecretKey_1234567890_ABCDEF",
                    ["Jwt:Issuer"] = "VintalooTests",
                    ["Jwt:Audience"] = "VintalooTestsUsers",
                    ["Jwt:ExpiresInHours"] = "1"
                });
            });
        }
    }

    public sealed class IntegrationTestFactory : TestWebApplicationFactory
    {
        public IntegrationTestFactory()
            : base("Server=(localdb)\\mssqllocaldb;Database=vintaloo_tests;Trusted_Connection=True;TrustServerCertificate=True")
        {
        }
    }

    public static class WebTestHelper
    {
        public static async Task<HttpResponseMessage> IniciarSesionAsync(
            HttpClient client,
            string correo,
            string password)
        {
            var token = await ObtenerTokenAntiforgeryAsync(client, "/Auth/Login");

            var formulario = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Correo"] = correo,
                ["Password"] = password,
                ["__RequestVerificationToken"] = token
            });

            return await client.PostAsync("/Auth/Login", formulario);
        }

        public static async Task<string> ObtenerTokenAntiforgeryAsync(HttpClient client, string ruta)
        {
            var respuesta = await client.GetAsync(ruta);
            respuesta.EnsureSuccessStatusCode();

            var html = await respuesta.Content.ReadAsStringAsync();
            var token = Regex.Match(
                html,
                "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");

            Assert.True(token.Success, $"No se encontró el token antiforgery en '{ruta}'.");
            return token.Groups[1].Value;
        }

        public static async Task<HttpClient> CrearClienteAutenticadoAsync(
            TestWebApplicationFactory factory,
            string correo,
            string password)
        {
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });

            var respuesta = await IniciarSesionAsync(client, correo, password);
            Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
            return client;
        }
    }
}
