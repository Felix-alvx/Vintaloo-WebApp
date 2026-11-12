using Application.DTOs;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Vintaloo.Tests
{
    public class ValidationTests
    {
        private static IReadOnlyList<ValidationResult> Validar(object modelo)
        {
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(modelo, new ValidationContext(modelo), resultados, true);
            return resultados;
        }

        [Theory]
        [InlineData("123456")]
        [InlineData("password1!")]
        [InlineData("PASSWORD1!")]
        [InlineData("Password!")]
        [InlineData("Password1")]
        public void RegisterDTO_RechazaPasswordsDebiles(string password)
        {
            var errores = Validar(new RegisterDTO
            {
                Nombre = "Usuario",
                Correo = "usuario@test.com",
                Password = password,
                ConfirmPassword = password
            });

            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(RegisterDTO.Password)));
        }

        [Fact]
        public void RegisterDTO_AceptaPasswordSeguraYConfirmacion()
        {
            var errores = Validar(new RegisterDTO
            {
                Nombre = "Usuario",
                Correo = "usuario@test.com",
                Password = "Password1!",
                ConfirmPassword = "Password1!"
            });

            Assert.Empty(errores);
        }

        [Fact]
        public void CreateUsuarioDTO_RechazaConfirmacionDistinta()
        {
            var errores = Validar(new CreateUsuarioDTO
            {
                Nombre = "Usuario",
                Correo = "usuario@test.com",
                Password = "Password1!",
                ConfirmPassword = "Password2!",
                IdRol = 2
            });

            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(CreateUsuarioDTO.ConfirmPassword)));
        }

        [Fact]
        public void CreateUsuarioDTO_RechazaRolFueraDeRango()
        {
            var errores = Validar(new CreateUsuarioDTO
            {
                Nombre = "Usuario",
                Correo = "usuario@test.com",
                Password = "Password1!",
                ConfirmPassword = "Password1!",
                IdRol = 99
            });

            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(CreateUsuarioDTO.IdRol)));
        }

        [Fact]
        public void ResetPasswordDTO_RechazaPasswordDebil()
        {
            var errores = Validar(new ResetPasswordDTO
            {
                Token = "token",
                Password = "123456",
                ConfirmPassword = "123456"
            });

            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(ResetPasswordDTO.Password)));
        }

        [Fact]
        public void ForgotPasswordDTO_RechazaCorreoInvalido()
        {
            var errores = Validar(new ForgotPasswordDTO
            {
                Correo = "sin-arroba"
            });

            Assert.Contains(errores, e => e.MemberNames.Contains(nameof(ForgotPasswordDTO.Correo)));
        }
    }
}
