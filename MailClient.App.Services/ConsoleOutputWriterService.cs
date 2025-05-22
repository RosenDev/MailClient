using MailClient.App.Services.Contracts;

namespace MailClient.App.Services
{
    /// <summary>
    /// Writes output to the console.
    /// </summary>
    public class ConsoleOutputWriterService : IOutputWriterService
    {
        /// <summary>
        /// Writes the given message to the console without a newline.
        /// </summary>
        /// <param name="message">The text to write; can be null.</param>
        public void Write(string? message)
        {
            Console.Write(message);
        }

        /// <summary>
        /// Writes the given message to the console followed by a newline.
        /// </summary>
        /// <param name="message">The text to write; can be null.</param>
        public void WriteLine(string? message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Writes just a newline to the console.
        /// </summary>
        public void WriteLine()
        {
            Console.WriteLine();
        }
    }
}
