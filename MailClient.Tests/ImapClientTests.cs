using FluentAssertions;
using MailClient.Contracts;
using MailClient.Models;
using MailClient.Tests.Common;
using Moq;
using Serilog;

namespace MailClient.Tests
{
    public class ImapClientTests
    {
        private const string Host = "imap.example.com";
        private const int Port = 993;
        private const string Username = "user";
        private const string Password = "pass";
        private const string Mailbox = "INBOX";


        private ImapClient CreateClientWithStreams(TrackingReader reader, TrackingWriter writer, Mock<ILogger> loggerMock)
        {
            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(Host, Port, It.IsAny<CancellationToken>()))
                .ReturnsAsync((reader, writer));
            var client = new ImapClient(factoryMock.Object, loggerMock.Object)
            {
                DefaultTimeoutSeconds = 5
            };
            return client;
        }

        private FetchMessagesRequest CreateRequest() => new FetchMessagesRequest
        {
            Host = Host,
            Port = Port,
            Username = Username,
            Password = Password,
            Mailbox = Mailbox
        };

        [Fact]
        public async Task FetchAllMessagesAsync_NoMessages_ReturnsEmptyList()
        {
            var lines = new[]
            {
                "* OK IMAP4rev1 Service Ready",
                "A001 OK LOGIN completed",
                "A002 OK SELECT completed",
                "* SEARCH",
                "A003 OK SEARCH completed"
            };
            var reader = new TrackingReader(lines);
            var writer = new TrackingWriter();
            var loggerMock = new Mock<ILogger>();
            var client = CreateClientWithStreams(reader, writer, loggerMock);

            var result = await client.FetchAllMessagesAsync(CreateRequest(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FetchAllMessagesAsync_WithMessages_ReturnsRawEmailResponses()
        {
            var lines = new List<string>
            {
                "* OK IMAP4rev1 Service Ready",
                "A001 OK LOGIN completed",
                "A002 OK SELECT completed",
                "* SEARCH ALL 100 200",
                "A003 OK SEARCH completed",
                "* 1 FETCH (UID 100 RFC822 {123}",
                "Subject: Test1",
                "Body line1",
                "* 2 FETCH (UID 200 RFC822 {456}",
                "Subject: Test2",
                "Body line2",
                "A004 OK FETCH completed"
            };
            var reader = new TrackingReader(lines);
            var writer = new TrackingWriter();
            var loggerMock = new Mock<ILogger>();
            var client = CreateClientWithStreams(reader, writer, loggerMock);

            var result = await client.FetchAllMessagesAsync(CreateRequest(), CancellationToken.None);

            result.Should().HaveCount(2);
            result[0].Uid.Should().Be("100");
            result[0].RawContent.Should().Contain("Subject: Test1").And.Contain("Body line1");
            result[1].Uid.Should().Be("200");
            result[1].RawContent.Should().Contain("Subject: Test2").And.Contain("Body line2");
        }

        [Fact]
        public async Task FetchAllMessagesAsync_CanceledBeforeTimeout_ThrowsOperationCanceledException()
        {
            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns<string, int, CancellationToken>(async (h, p, ct) =>
                {
                    await Task.Delay(Timeout.Infinite, ct);
                    return (null, null);
                });
            var loggerMock = new Mock<ILogger>();
            var client = new ImapClient(factoryMock.Object, loggerMock.Object)
            {
                DefaultTimeoutSeconds = 1
            };
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            Func<Task> act = () => client.FetchAllMessagesAsync(CreateRequest(), cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
