namespace MailClient.App.Infrastructure.Contracts
{
    public interface IAsyncPrompt<T>
    {
        Task<T> RunAsync(CancellationToken ct);
    }
}
