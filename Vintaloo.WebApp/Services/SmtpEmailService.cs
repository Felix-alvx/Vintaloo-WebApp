using Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Vintaloo.WebApp.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> EnviarRecuperacionAsync(string destinatario, string enlace)
        {
            var host = _config["Smtp:Host"];
            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning("SMTP no configurado. No se envió el correo de recuperación.");
                return false;
            }

            var remitente = _config["Smtp:From"];
            if (string.IsNullOrWhiteSpace(remitente))
                remitente = "no-reply@vintaloo.com";

            var puerto = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
            var usarSsl = !bool.TryParse(_config["Smtp:EnableSsl"], out var ssl) || ssl;
            var usuario = _config["Smtp:User"];
            var password = _config["Smtp:Password"];

            var enlaceSeguro = WebUtility.HtmlEncode(enlace);
            var cuerpo = $@"
                <p>Hola,</p>
                <p>Recibimos una solicitud para restablecer tu contraseña de Vintaloo.</p>
                <p><a href=""{enlaceSeguro}"">Restablecer mi contraseña</a></p>
                <p>El enlace vence en 30 minutos. Si no solicitaste este cambio, ignora este correo.</p>";

            try
            {
                using var mensaje = new MailMessage(remitente, destinatario)
                {
                    Subject = "Recupera tu contraseña de Vintaloo",
                    Body = cuerpo,
                    IsBodyHtml = true
                };

                using var cliente = new SmtpClient(host, puerto)
                {
                    EnableSsl = usarSsl
                };

                if (!string.IsNullOrWhiteSpace(usuario))
                    cliente.Credentials = new NetworkCredential(usuario, password);

                await cliente.SendMailAsync(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo de recuperación.");
                return false;
            }
        }
    }
}
