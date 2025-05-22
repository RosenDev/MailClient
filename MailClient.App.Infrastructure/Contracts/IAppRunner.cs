namespace MailClient.App.Infrastructure.Contracts
{
    /// <summary>
    /// AppRunner is the application entry point
    /// All actions happen here
    /// </summary>
    public interface IAppRunner
    {
        /// <summary>
        /// Used to run the app
        /// </summary>
        Task RunAsync(CancellationToken ct);
    }
}
