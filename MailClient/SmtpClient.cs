using System.Text;
using MailClient.Constants;
using MailClient.Contracts;
using MailClient.Models;
using Serilog;

namespace MailClient
{
    /// <summary>
    /// Simple SMTP client for sending emails over SSL/TLS.
    /// Ensures a QUIT command is sent on async disposal.
    /// Uses AUTH PLAIN for authentication.
    /// </summary>
    public class SmtpClient : ISmtpClient, IAsyncDisposable
    {
        /// <summary>
        /// Maximum time in seconds to wait for any SMTP operation before timing out.
        /// </summary>
        public int DefaultTimeoutSeconds { get; set; } = 60;

        private readonly ISmtpConnectionFactory _smtpConnectionFactory;
        private readonly ILogger _logger;
        private TextReader _reader;
        private TextWriter _writer;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpClient"/> class.
        /// </summary>
        /// <param name="smtpConnectionFactory">
        /// Factory to create secure SMTP connections.
        /// </param>
        /// <param name="logger">Logger for diagnostic messages.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any constructor argument is null.
        /// </exception>
        public SmtpClient(ISmtpConnectionFactory smtpConnectionFactory, ILogger logger)
        {
            _smtpConnectionFactory = smtpConnectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Connects to the SMTP server using implicit SSL (default port 465).
        /// </summary>
        /// <param name="host">The SMTP server hostname.</param>
        /// <param name="port">The SMTP server port (usually 465).</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous connect operation.</returns>
        public async Task ConnectAsync(string host, int port, CancellationToken ct)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            var token = timeoutCts.Token;

            try
            {
                var readerAndWriter = await _smtpConnectionFactory
                    .CreateSecureConnectionAsync(host, port, token);
                _reader = readerAndWriter.Reader;
                _writer = readerAndWriter.Writer;

                _logger.Information(await _reader.ReadLineAsync(token));
            }
            catch(Exception ex) when(!ct.IsCancellationRequested)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Authenticates using the AUTH PLAIN mechanism (Base64-encoded "\0username\0password").
        /// </summary>
        /// <param name="username">The SMTP username.</param>
        /// <param name="password">The SMTP password.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous authentication operation.</returns>
        public async Task AuthenticateAsync(string username, string password, CancellationToken ct)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            var token = timeoutCts.Token;

            try
            {
                var payload = "\0" + username + "\0" + password;
                var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

                await SendCommandAsync($"{SmtpCommands.AuthPlain}{base64}", token);
                _logger.Information(await _reader.ReadLineAsync(token));
            }
            catch(Exception ex) when(!ct.IsCancellationRequested)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Sends an email message with the specified envelope and body.
        /// </summary>
        /// <param name="email">The <see cref="EmailMessageRequest"/> containing From, To, Cc, Subject, and Body.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        public async Task SendMailAsync(EmailMessageRequest email, CancellationToken ct)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            var token = timeoutCts.Token;

            try
            {
                await SendCommandAsync($"{SmtpCommands.MailFrom}<{email.From}>", token);
                _logger.Information(await _reader.ReadLineAsync(token));

                foreach(string recipient in email.To)
                {
                    await SendCommandAsync($"{SmtpCommands.RcptTo}<{recipient}>", token);
                    _logger.Information(await _reader.ReadLineAsync(token));
                }

                if(email.Cc != null)
                {
                    foreach(string recipient in email.Cc)
                    {
                        await SendCommandAsync($"{SmtpCommands.RcptTo}<{recipient}>", token);
                        _logger.Information(await _reader.ReadLineAsync(token));
                    }
                }

                await SendCommandAsync(SmtpCommands.Data, token);
                _logger.Information(await _reader.ReadLineAsync(token));

                var sb = new StringBuilder();
                sb.Append($"{EmailHeaders.From}{email.From}\r\n");
                sb.Append($"{EmailHeaders.To}{string.Join(", ", email.To)}\r\n");
                if(email.Cc?.Count > 0)
                    sb.Append($"{EmailHeaders.Cc}{string.Join(", ", email.Cc)}\r\n");
                sb.Append($"{EmailHeaders.Subject}{email.Subject}\r\n");
                sb.Append($"{EmailHeaders.ContentType}text/plain; charset=utf-8\r\n");
                sb.Append("\r\n");
                sb.Append($"{email.Body}\r\n");
                sb.Append(".\r\n");

                await _writer.WriteLineAsync(sb.ToString());
                await _writer.FlushAsync(token);
                _logger.Information(await _reader.ReadLineAsync(token));
            }
            catch(Exception ex) when(!ct.IsCancellationRequested)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously disposes the client, sending QUIT and closing the connection.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous disposal.</returns>
        public async ValueTask DisposeAsync()
        {
            if(_disposed)
                return;

            if(_writer != null)
            {
                try
                {
                    _logger.Information("C: QUIT");
                    await _writer.WriteLineAsync(SmtpCommands.Quit);
                    await _writer.FlushAsync();
                }
                catch(Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw;
                }
            }

            try
            {
                _writer?.Dispose();
                _reader?.Dispose();
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            finally
            {
                _disposed = true;
            }
        }

        private async Task SendCommandAsync(string command, CancellationToken ct)
        {
            try
            {
                _logger.Information($"C: {command}");
                await _writer.WriteLineAsync(command);
            }
            catch(Exception ex) when(!ct.IsCancellationRequested)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }
    }
}
