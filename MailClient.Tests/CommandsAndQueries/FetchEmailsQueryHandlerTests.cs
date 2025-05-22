using FluentAssertions;
using MailClient.App.CommandsAndQueries.Constants;
using MailClient.App.CommandsAndQueries.Queries;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using MailClient.Contracts;
using MailClient.Models;
using Moq;

namespace MailClient.Tests.CommandsAndQueries
{
    public class FetchEmailsQueryHandlerTests
    {
        private readonly Mock<IServerCredentialsService> _credMock = new Mock<IServerCredentialsService>();
        private readonly Mock<IEmailStoreService> _storeMock = new Mock<IEmailStoreService>();
        private readonly Mock<IImapClient> _imapMock = new Mock<IImapClient>();
        private readonly Mock<IEmailMessageParser> _parserMock = new Mock<IEmailMessageParser>();
        private FetchEmailsQueryHandler CreateHandler() => new FetchEmailsQueryHandler(
            _imapMock.Object,
            _credMock.Object,
            _parserMock.Object,
            _storeMock.Object);

        [Fact]
        public async Task Handle_NoLocalNoFetched_ReturnsEmptyEmails()
        {
            var creds = new ServerCredentialModel { ImapServerAddress = "imap", ImapPort = 993, Username = "u", Password = "p" };
            _credMock.Setup(s => s.GetCredentialsByServerAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(creds);
            _storeMock.Setup(s => s.GetEmailsAsync("imap", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmailRawListModel { ServerAddress = "imap", RawEmails = new List<EmailRawModel>() });
            _imapMock.Setup(c => c.FetchAllMessagesAsync(It.IsAny<FetchMessagesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RawEmailResponse>());
            var handler = CreateHandler();
            var result = await handler.Handle(new FetchEmailsQuery { ServerId = 1 }, CancellationToken.None);
            result.Emails.Should().BeEmpty();
            _storeMock.Verify(s => s.StoreEmailsAsync(It.IsAny<EmailRawListModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_LocalExistsNoNew_ReturnsParsedLocalOnly()
        {
            var creds = new ServerCredentialModel { ImapServerAddress = "imap", ImapPort = 993, Username = "u", Password = "p" };
            var local = new EmailRawModel { Uid = "1", RawContent = "raw1" };
            _credMock.Setup(s => s.GetCredentialsByServerAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(creds);
            _storeMock.Setup(s => s.GetEmailsAsync("imap", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmailRawListModel { ServerAddress = "imap", RawEmails = new List<EmailRawModel> { local } });
            _imapMock.Setup(c => c.FetchAllMessagesAsync(It.IsAny<FetchMessagesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RawEmailResponse> { new RawEmailResponse("1", "raw1") });
            _parserMock.Setup(p => p.Parse("raw1")).Returns(new EmailModel { Subject = "s1" });
            var handler = CreateHandler();
            var result = await handler.Handle(new FetchEmailsQuery { ServerId = 2 }, CancellationToken.None);
            result.Emails.Should().HaveCount(1);
            result.Emails[0].Subject.Should().Be("s1");
            _storeMock.Verify(s => s.StoreEmailsAsync(It.IsAny<EmailRawListModel>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NewFetched_StoresAndReturnsAll()
        {
            var creds = new ServerCredentialModel { ImapServerAddress = "imap", ImapPort = 993, Username = "u", Password = "p" };
            var local = new EmailRawModel { Uid = "1", RawContent = "raw1" };
            var fetchedNew = new RawEmailResponse("2", "raw2");
            _credMock.Setup(s => s.GetCredentialsByServerAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(creds);
            _storeMock.Setup(s => s.GetEmailsAsync("imap", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmailRawListModel { ServerAddress = "imap", RawEmails = new List<EmailRawModel> { local } });
            _imapMock.Setup(c => c.FetchAllMessagesAsync(It.Is<FetchMessagesRequest>(r =>
                r.Host == "imap" && r.Port == 993 && r.Mailbox == Settings.DefaultMailbox),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RawEmailResponse> { new RawEmailResponse("1", "raw1"), fetchedNew });
            var parsed1 = new EmailModel { Subject = "s1" };
            var parsed2 = new EmailModel { Subject = "s2" };
            _parserMock.Setup(p => p.Parse("raw1")).Returns(parsed1);
            _parserMock.Setup(p => p.Parse("raw2")).Returns(parsed2);
            var handler = CreateHandler();
            var result = await handler.Handle(new FetchEmailsQuery { ServerId = 3 }, CancellationToken.None);
            _storeMock.Verify(s => s.StoreEmailsAsync(
                It.Is<EmailRawListModel>(m => m.RawEmails.Count == 1 && m.RawEmails[0].Uid == "2"),
                It.IsAny<CancellationToken>()), Times.Once);
            result.Emails.Should().BeEquivalentTo(new[] { parsed2, parsed1 }, options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task Handle_PassesCancellationToken_ToAllCalls()
        {
            var creds = new ServerCredentialModel { ImapServerAddress = "imap", ImapPort = 993, Username = "u", Password = "p" };
            _credMock.Setup(s => s.GetCredentialsByServerAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(creds);
            _storeMock.Setup(s => s.GetEmailsAsync("imap", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EmailRawListModel { ServerAddress = "imap", RawEmails = new List<EmailRawModel>() });
            _imapMock.Setup(c => c.FetchAllMessagesAsync(It.IsAny<FetchMessagesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RawEmailResponse>());
            _parserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(new EmailModel());
            var handler = CreateHandler();
            using var cts = new CancellationTokenSource();
            await handler.Handle(new FetchEmailsQuery { ServerId = 4 }, cts.Token);
            _credMock.Verify(s => s.GetCredentialsByServerAsync(4, cts.Token), Times.Once);
            _storeMock.Verify(s => s.GetEmailsAsync("imap", cts.Token), Times.Once);
            _imapMock.Verify(c => c.FetchAllMessagesAsync(It.IsAny<FetchMessagesRequest>(), cts.Token), Times.Once);
            _storeMock.Verify(s => s.StoreEmailsAsync(It.IsAny<EmailRawListModel>(), cts.Token), Times.Never);
        }
    }
}