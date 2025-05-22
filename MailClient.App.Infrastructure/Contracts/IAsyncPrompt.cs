namespace MailClient.App.Infrastructure.Contracts
{
    /// <summary>
    /// Prompts the user to enter input for existing operation, select operation
    /// or cancel to return to the main menu.
    /// </summary>
    public interface IAsyncPrompt<T>
    {
        /// <summary> Show the prompt, read and return a T (e.g. string, Credential, MailMessage, etc.) </summary>
        Task<T> RunAsync(CancellationToken ct);
    }
}
