namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> EnviarRecuperacionAsync(string destinatario, string enlace);
    }
}
