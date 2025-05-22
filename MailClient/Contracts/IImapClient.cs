using MailClient.Models;

namespace MailClient.Contracts
{
    public interface IImapClient : IAsyncDisposable
    {
        Task<List<RawEmailResponse>> FetchAllMessagesAsync(FetchMessagesRequest fetchMessagesRequest, CancellationToken ct);
    }
}
