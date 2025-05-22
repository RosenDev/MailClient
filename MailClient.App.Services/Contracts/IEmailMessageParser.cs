using MailClient.App.Models;

namespace MailClient.App.Services.Contracts
{
    /// <summary>
    /// Defines a parser that converts raw RFC-822 email text into an <see cref="EmailModel"/>.
    /// </summary>
    public interface IEmailMessageParser
    {
        /// <summary>
        /// Parses a complete raw email message string and extracts its headers and body.
        /// </summary>
        /// <param name="rawEmailMessage">The full RFC-822 message text to parse.</param>
        /// <returns>
        /// An <see cref="EmailModel"/> populated with From, To, Cc, Subject, and Body fields.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="rawEmailMessage"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// Thrown if required headers (From, To, or Subject) are missing or malformed.
        /// </exception>
        EmailModel Parse(string rawEmailMessage);
    }
}
