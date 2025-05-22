using System.Text;
using MailClient.Constants;
using MailClient.Contracts;
using MailClient.Models;
using Serilog;

namespace MailClient
{
    /// <summary>
    /// Simple IMAP client for receiving emails over SSL.
    /// Exposes a single public method to fetch and parse all messages at once.
    /// </summary>
    public class ImapClient : IImapClient, IAsyncDisposable
    {
        /// <summary>
        /// The maximum time, in seconds, to wait for any IMAP operation before timing out.
        /// </summary>
        public int DefaultTimeoutSeconds { get; set; } = 60;

        private TextReader _reader;
        private TextWriter _writer;
        private int _tagCounter;
        private readonly ISmtpConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImapClient"/> class.
        /// </summary>
        /// <param name="connectionFactory">Factory to create the secure IMAP connection.</param>
        /// <param name="logger">Logger to record client‐server communication.</param>
        public ImapClient(ISmtpConnectionFactory connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Connects to the IMAP server, authenticates, selects the mailbox, and fetches all messages.
        /// </summary>
        /// <param name="request">
        /// A <see cref="FetchMessagesRequest"/> containing host, port, credentials, and mailbox name.
        /// </param>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the entire fetch operation.</param>
        /// <returns>
        /// A list of <see cref="RawEmailResponse"/> objects, each containing the UID and full raw content.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the operation exceeds <see cref="DefaultTimeoutSeconds"/> or if <paramref name="ct"/> is canceled.
        /// </exception>
        public async Task<List<RawEmailResponse>> FetchAllMessagesAsync(FetchMessagesRequest request, CancellationToken ct)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            var token = timeoutCts.Token;

            await ConnectAsync(request.Host, request.Port, token);
            await LoginAsync(request.Username, request.Password, token);
            await SelectMailboxAsync(request.Mailbox, token);

            var uids = await FetchMessageUidsAsync(token);
            if(uids.Count == 0)
                return new List<RawEmailResponse>();

            return await BulkFetchRawMessagesAsync(uids, token);
        }

        /// <summary>
        /// Gracefully logs out of the IMAP session and disposes underlying streams.
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTask"/> that completes when logout is sent and cleanup is done.
        /// </returns>
        /// <exception cref="Exception">Any error during logout or disposal is rethrown.</exception>
        public async ValueTask DisposeAsync()
        {
            if(_disposed) return;

            try
            {
                var tag = GetNextTag();
                await SendCommandAsync(tag, ImapCommands.Logout, CancellationToken.None);
                await ReadResponseAsync(tag, CancellationToken.None);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            finally
            {
                _writer?.Dispose();
                _reader?.Dispose();
                _disposed = true;
            }
        }

        private async Task ConnectAsync(string host, int port, CancellationToken ct)
        {
            var readerAndWriter = await _connectionFactory.CreateSecureConnectionAsync(host, port, ct);
            _reader = readerAndWriter.Reader;
            _writer = readerAndWriter.Writer;
            var greeting = await _reader.ReadLineAsync(ct);
            _logger.Information("S: " + greeting);
        }

        private async Task LoginAsync(string username, string password, CancellationToken ct)
        {
            string tag = GetNextTag();
            await SendCommandAsync(tag, $"{ImapCommands.Login}{username} {password}", ct);

            string line;
            while((line = await _reader.ReadLineAsync(ct)) != null)
            {
                _logger.Information("S: " + line);
                if(line.StartsWith(tag + " "))
                    break;
            }
        }

        private async Task SelectMailboxAsync(string mailbox, CancellationToken ct)
        {
            var tag = GetNextTag();
            await SendCommandAsync(tag, $"{ImapCommands.Select}{mailbox}", ct);
            var response = await ReadResponseAsync(tag, ct);
            _logger.Information(response);
        }

        private async Task<List<string>> FetchMessageUidsAsync(CancellationToken ct)
        {
            var tag = GetNextTag();
            await SendCommandAsync(tag, ImapCommands.UidSearchAll, ct);
            var lines = await ReadLinesUntilTagAsync(tag, ct);
            foreach(var line in lines)
            {
                if(line.StartsWith(ImapCommands.Search))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length > 2
                        ? parts.Skip(2).ToList()
                        : new List<string>();
                }
            }
            return new List<string>();
        }

        private async Task<List<RawEmailResponse>> BulkFetchRawMessagesAsync(List<string> uids, CancellationToken ct)
        {
            var set = uids.Count == 1
                ? uids[0]
                : $"{uids.First()}:{uids.Last()}";

            var tag = GetNextTag();
            await SendCommandAsync(tag, string.Format(ImapCommands.UidFetch, set), ct);

            var list = new List<RawEmailResponse>();
            var sb = new StringBuilder();
            string currentUid = null;
            bool collecting = false;

            while(true)
            {
                var line = await _reader.ReadLineAsync(ct);
                if(line == null) break;

                if(line.StartsWith("*") && line.Contains(ImapCommands.Fetch))
                {
                    if(collecting && currentUid != null)
                    {
                        list.Add(new RawEmailResponse(currentUid, sb.ToString()));
                        sb.Clear();
                    }
                    var parts = line.Split(new[] { ' ', '(' }, StringSplitOptions.RemoveEmptyEntries);
                    currentUid = parts.SkipWhile(p => p != "UID").Skip(1).FirstOrDefault();
                    collecting = true;
                    continue;
                }

                if(collecting)
                {
                    if(line.StartsWith(tag))
                    {
                        list.Add(new RawEmailResponse(currentUid, sb.ToString()));
                        break;
                    }
                    sb.AppendLine(line);
                }
            }

            return list;
        }

        private async Task SendCommandAsync(string tag, string cmd, CancellationToken ct)
        {
            var full = $"{tag} {cmd}";
            _logger.Information("C: " + full);
            await _writer.WriteLineAsync(full);
        }

        private async Task<string> ReadResponseAsync(string tag, CancellationToken ct)
        {
            var sb = new StringBuilder();
            string line;
            while((line = await _reader.ReadLineAsync(ct)) != null)
            {
                sb.AppendLine(line);
                if(line.StartsWith(tag))
                    break;
            }
            return sb.ToString();
        }

        private async Task<List<string>> ReadLinesUntilTagAsync(string tag, CancellationToken ct)
        {
            var lines = new List<string>();
            string line;
            while((line = await _reader.ReadLineAsync(ct)) != null)
            {
                lines.Add(line);
                if(line.StartsWith(tag))
                    break;
            }
            return lines;
        }

        private string GetNextTag() => "A" + (++_tagCounter).ToString("D3");
    }
}
