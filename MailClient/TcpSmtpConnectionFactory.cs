using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using MailClient.Contracts;
using Serilog;

namespace MailClient
{
    /// <summary>
    /// Factory for creating secure SMTP connections over TCP with SSL/TLS.
    /// </summary>
    public class TcpSmtpConnectionFactory : ISmtpConnectionFactory
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="TcpSmtpConnectionFactory"/>.
        /// </summary>
        /// <param name="logger">The logger used to record connection errors.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is null.</exception>
        public TcpSmtpConnectionFactory(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Establishes a TCP connection to the specified host and port, upgrades it to SSL/TLS,
        /// and returns a <see cref="TextReader"/> and <see cref="TextWriter"/> over the encrypted stream.
        /// </summary>
        /// <param name="host">The SMTP server hostname to connect to.</param>
        /// <param name="port">The TCP port to connect on (typically 465).</param>
        /// <param name="ct">A cancellation token to cancel the connection attempt.</param>
        /// <returns>
        /// A tuple containing a <see cref="TextReader"/> for reading server responses
        /// and a <see cref="TextWriter"/> for sending commands.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="host"/> is null or empty.</exception>
        /// <exception cref="SocketException">Thrown on TCP connection failures.</exception>
        /// <exception cref="AuthenticationException">Thrown if SSL/TLS handshake fails.</exception>
        /// <exception cref="IOException">Thrown on read/write errors.</exception>
        public async Task<(TextReader Reader, TextWriter Writer)> CreateSecureConnectionAsync(string host, int port, CancellationToken ct)
        {
            try
            {
                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(host, port, ct);
                var sslStream = new SslStream(tcpClient.GetStream());
                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = host
                }, ct);

                var reader = new StreamReader(sslStream, Encoding.ASCII);
                var writer = new StreamWriter(sslStream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };

                return (reader, writer);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
