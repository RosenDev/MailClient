using System.Text;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Services
{
    /// <summary>
    /// Parses raw RFC-822 email messages into <see cref="EmailModel"/> instances.
    /// </summary>
    public class EmailMessageParser : IEmailMessageParser
    {
        /// <summary>
        /// Parses a raw email string into its headers and body.
        /// </summary>
        /// <param name="rawEmailMessage">The full RFC-822 message text.</param>
        /// <returns>
        /// An <see cref="EmailModel"/> populated with From, To, Cc, Subject, and Body.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="rawEmailMessage"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown if any required header (From, To, Subject) is missing or malformed.
        /// </exception>
        public EmailModel Parse(string rawEmailMessage)
        {
            if(string.IsNullOrWhiteSpace(rawEmailMessage))
                throw new ArgumentException("Raw email message cannot be empty.", nameof(rawEmailMessage));

            using var reader = new StringReader(rawEmailMessage);
            var headers = ReadHeaders(reader);
            var body = ReadBody(reader);

            if(!headers.TryGetValue("From", out var from) || string.IsNullOrWhiteSpace(from))
                throw new FormatException("Missing or empty 'From' header.");
            if(!headers.TryGetValue("To", out var toHeader) || string.IsNullOrWhiteSpace(toHeader))
                throw new FormatException("Missing or empty 'To' header.");
            if(!headers.TryGetValue("Subject", out var subject) || string.IsNullOrWhiteSpace(subject))
                throw new FormatException("Missing or empty 'Subject' header.");

            headers.TryGetValue("Cc", out var ccHeader);

            var to = SplitAddresses(toHeader);
            var cc = SplitAddresses(ccHeader);

            return new EmailModel
            {
                From = from,
                To = to,
                Cc = cc,
                Subject = subject,
                Body = body
            };
        }

        private IDictionary<string, string> ReadHeaders(StringReader reader)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? line;
            string? current = null;
            var valueBuffer = new StringBuilder();

            while((line = reader.ReadLine()) != null)
            {
                if(string.IsNullOrWhiteSpace(line))
                    break;

                if((line[0] == ' ' || line[0] == '\t') && current != null)
                {
                    valueBuffer.Append(' ').Append(line.Trim());
                    continue;
                }

                if(current != null)
                {
                    headers[current] = valueBuffer.ToString();
                    valueBuffer.Clear();
                }

                var idx = line.IndexOf(':');
                if(idx <= 0)
                    throw new FormatException($"Invalid header line: '{line}'");

                current = line.Substring(0, idx).Trim();
                var remainder = line.Substring(idx + 1).Trim();
                valueBuffer.Append(remainder);
            }

            if(current != null)
                headers[current] = valueBuffer.ToString();

            return headers;
        }

        private string ReadBody(StringReader reader)
        {
            var sb = new StringBuilder();
            string? line;
            while((line = reader.ReadLine()) != null)
                sb.AppendLine(line);
            return sb.ToString();
        }

        private List<string> SplitAddresses(string? headerValue)
        {
            if(string.IsNullOrWhiteSpace(headerValue))
                return new List<string>();

            return headerValue
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => a.Length > 0)
                .ToList();
        }
    }
}
