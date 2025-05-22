using MailClient.Models;

namespace MailClient.Contracts
{
    /// <summary>
    /// Simple SMTP client for sending emails over SSL/TLS.
    /// Ensures a QUIT command is sent on async disposal.
    /// Uses AUTH PLAIN for authentication.
    /// </summary>
    public interface ISmtpClient : IAsyncDisposable
    {
        Task ConnectAsync(string host, int port, CancellationToken ct);
        Task AuthenticateAsync(string username, string password, CancellationToken ct);
        Task SendMailAsync(EmailMessageRequest email, CancellationToken ct);
    }
}
