using MailClient.App.CommandsAndQueries.Constants;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using MailClient.Contracts;
using MailClient.Models;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Queries
{
    public class FetchEmailsQueryHandler : IRequestHandler<FetchEmailsQuery, EmailListModel>
    {
        private readonly IImapClient _imapClient;
        private readonly IServerCredentialsService _serverCredentialService;
        private readonly IEmailMessageParser _emailMessageParser;
        private readonly IEmailStoreService _emailStoreService;

        public FetchEmailsQueryHandler(
            IImapClient imapClient,
            IServerCredentialsService serverCredentialService,
            IEmailMessageParser emailMessageParser,
            IEmailStoreService emailStoreService)
        {
            _imapClient = imapClient;
            _serverCredentialService = serverCredentialService;
            _emailMessageParser = emailMessageParser;
            _emailStoreService = emailStoreService;
        }

        public async Task<EmailListModel> Handle(FetchEmailsQuery request, CancellationToken cancellationToken)
        {
            var credentials = await _serverCredentialService
                .GetCredentialsByServerAsync(request.ServerId, cancellationToken);

            var localEmails = await _emailStoreService
                .GetEmailsAsync(credentials.ImapServerAddress, cancellationToken);

            var existingUids = new HashSet<string>(
                localEmails.RawEmails.Select(e => e.Uid),
                StringComparer.Ordinal);

            var fetched = await _imapClient.FetchAllMessagesAsync(
                new FetchMessagesRequest
                {
                    Host = credentials.ImapServerAddress,
                    Port = (int)credentials.ImapPort,
                    Username = credentials.Username,
                    Password = credentials.Password,
                    Mailbox = Settings.DefaultMailbox
                },
                cancellationToken);

            var newMessages = fetched
                .Where(m => !existingUids.Contains(m.Uid))
                .ToList();

            if(newMessages.Any())
            {
                var toStore = new EmailRawListModel
                {
                    ServerAddress = credentials.ImapServerAddress,
                    RawEmails = newMessages.Select(m => new EmailRawModel
                    {
                        Uid = m.Uid,
                        RawContent = m.RawContent
                    }).ToList()
                };
                await _emailStoreService.StoreEmailsAsync(toStore, cancellationToken);
            }

            var parsedEmails = new List<EmailModel>();

            newMessages.ForEach(email =>
            {
                parsedEmails.Add(_emailMessageParser.Parse(email.RawContent));
            });

            parsedEmails.AddRange(localEmails.RawEmails.Select(x => _emailMessageParser.Parse(x.RawContent)));

            return new EmailListModel
            {
                Emails = parsedEmails
            };
        }
    }
}
