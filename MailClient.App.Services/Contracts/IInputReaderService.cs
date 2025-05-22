namespace MailClient.App.Services.Contracts
{
    /// <summary>
    /// Defines methods for reading user input from a source (e.g., console).
    /// </summary>
    public interface IInputReaderService
    {
        /// <summary>
        /// Reads a line of text from the input source.
        /// </summary>
        /// <returns>
        /// The line entered by the user, or <c>null</c> if no more input is available.
        /// </returns>
        string? ReadLine();

        /// <summary>
        /// Reads a secure field (such as a password), masking the input as it is typed.
        /// </summary>
        /// <returns>The unmasked text entered by the user.</returns>
        string ReadSecureField();
    }
}
