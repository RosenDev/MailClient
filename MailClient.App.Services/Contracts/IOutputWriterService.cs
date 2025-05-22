namespace MailClient.App.Services.Contracts
{
    /// <summary>
    /// Defines methods for writing output to a destination (e.g., console, file).
    /// </summary>
    public interface IOutputWriterService
    {
        /// <summary>
        /// Writes the specified message followed by a newline.
        /// </summary>
        /// <param name="message">The text to write; may be null.</param>
        void WriteLine(string? message);

        /// <summary>
        /// Writes only a newline to the output.
        /// </summary>
        void WriteLine();

        /// <summary>
        /// Writes the specified message without appending a newline.
        /// </summary>
        /// <param name="message">The text to write; may be null.</param>
        void Write(string? message);
    }
}
