namespace MailClient.Contracts
{
    /// <summary>
    /// Factory for creating secure SMTP connections over TCP with SSL/TLS.
    /// </summary>
    public interface ISmtpConnectionFactory
    {
        // <summary>
        /// Establishes connection to the specified host and port, upgrades it to SSL/TLS,
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
        Task<(TextReader Reader, TextWriter Writer)> CreateSecureConnectionAsync(string host, int port, CancellationToken ct);
    }
}
