using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Vintaloo.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Login_Get_RespondeOk()
        {
            var response = await _client.GetAsync("/Auth/Login");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("Iniciar sesión", html);
        }

        [Fact]
        public async Task ForgotPassword_Get_RespondeOk()
        {
            var response = await _client.GetAsync("/Auth/ForgotPassword");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ResetPassword_Get_ConToken_RespondeOk()
        {
            var response = await _client.GetAsync("/Auth/ResetPassword?token=token-de-prueba");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Usuario_SinAutenticacion_RedirigeAlLogin()
        {
            var response = await _client.GetAsync("/Usuario");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/Auth/Login", response.Headers.Location!.ToString());
        }

        [Fact]
        public async Task Categoria_SinAutenticacion_RedirigeAlLogin()
        {
            var response = await _client.GetAsync("/Categoria");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("/Auth/Login", response.Headers.Location!.ToString());
        }
    }
}
