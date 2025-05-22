namespace MailClient.App.Infrastructure.Contracts
{
    public interface IAppRunner
    {
        Task RunAsync(CancellationToken ct);
    }
}
